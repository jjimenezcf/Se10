using System;
using Utilidades;
using ServicioDeDatos.Ventas;
using ServicioDeDatos;
using static Gestor.Errores.GestorDeErrores;
using ServicioDeDatos.Elemento;
using System.Collections.Generic;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.Juridico;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Tarea;

namespace GestorDeElementos.Extensores
{
    public static class ExtensorDeCobrosDeFae
    {

        public static (CuentaBancariaDtm cb, string alias) CuentaDeCargo(this CobroDeFaeDtm cobro, ContextoSe contexto, bool errorSiNoHay = true)
        {
            if (cobro.IdCuentaDeCargo is null && errorSiNoHay)
                Emitir($"Se ha solicitado la cuenta de cargo para uno de los cobro de la factura {cobro.Elemento.NumeroDeFactura}, y dicho cobro no es de {enumClaseDeCobro.Remesa}");

            if (cobro.IdCuentaDeCargo is null)
                return (null, null);

            var cc = cobro.CuentaDeCargo == null ? contexto.SeleccionarPorId<CuentaDeClienteDtm>(cobro.IdCuentaDeCargo.Entero()) : cobro.CuentaDeCargo;
            var cb = ExtensorDeClientes.CuentaBancaria(cc, contexto);
            return (cb: cb, alias: cc.Alias);
        }

        public static CuentaBancariaDtm CuentaDeIngreso(this CobroDeFaeDtm cobro, ContextoSe contexto, bool errorSiNoHay = true)
        {
            if (cobro.IdCuentaDeIngreso is null && errorSiNoHay)
                Emitir($"Se ha solicitado la cuenta de ingreso para uno de los cobro de la factura {cobro.Elemento.NumeroDeFactura}, y dicho cobro no es de {enumClaseDeCobro.Contado} ");

            if (cobro.IdCuentaDeIngreso is null)
                return null;

            var cs = cobro.CuentaDeIngreso == null ? contexto.SeleccionarPorId<CuentaDeMiSociedadDtm>(cobro.IdCuentaDeIngreso.Entero()) : cobro.CuentaDeIngreso;
            var cb = cs.Cuenta(contexto);
            return cb;
        }

        public static FacturaEmtDtm Factura(this CobroDeFaeDtm cobro, ContextoSe contexto, bool aplicarJoin = false)
        {
            if (cobro.Elemento == null)
            {
                cobro.Elemento = cobro.DetalleDe<FacturaEmtDtm>(contexto, aplicarJoin);
            }
            return cobro.Elemento;
        }

        public static SociedadDtm Sociedad(this CobroDeFaeDtm cobro, ContextoSe contexto) => cobro.Factura(contexto, aplicarJoin: false).Sociedad(contexto);

        public static ClienteDtm Cliente(this CobroDeFaeDtm cobro, ContextoSe contexto) => cobro.Factura(contexto, aplicarJoin: false).Cliente(contexto);

        public static string Referencia(this CobroDeFaeDtm cobro, ContextoSe contexto)
        {
            cobro.CalcularReferencia(contexto);
            return cobro.Referencia;
        }

        public static string CalcularReferencia(this CobroDeFaeDtm cobro, ContextoSe contexto)
        => cobro.Referencia.IsNullOrEmpty() || cobro.Referencia.EndsWith("-0")
        ? cobro.Referencia = cobro.Factura(contexto, aplicarJoin: false).Referencia + "-" + cobro.Id
        : cobro.Referencia;

        /* Comentarios tras devolver un cobro
         * 1. Devuelvo el cobro de una factura en estado Cobrada
            - Si no hay más cobros: devolver un cobro total
               - si está vencida 
                  transito a etapa de reclamación: Cobrada --> Vencida
               - Si no está vencida 
                  transito a etapa de emitida: Cobrada --> Emitida
            - Si hay mas de un cobro: devolver un cobro parcial
               - Si está vencida
                  transito a etapa de reclamación: Cobrada --> Vencida
               - Si no está vencida 
                  transito a parcialmentecobrada: Cobrada --> Parc. Cobrada (etapa pdt. pago- etapa de reclamación - etapa emitida)
            - Si la factura es remesada la paso a devuelta
         
         2. Devuelvo el cobro de una factura en estado parcl. cobrada
            - Si no hay más cobros
               - si está vencida 
                  transito a etapa de reclamación: Cobrada --> Vencida
               - Si no está vencida 
                  transito a etapa de emitida: Cobrada --> Emitida
            - Si hay mas de un cobro
               - Recalculo vencimiento
               - Si está vencida
                  transito a etapa de reclamación: Cobrada --> Vencida
               - Si no está vencida (sigo en Prcl. cobrada)
         
         3. Devuelvo el cobro de una factura en estado reclamación o vencida 
            - No se hace nada
         * */
        public static FacturaEmtDtm TrasEliminarUnCobro(this CobroDeFaeDtm cobro, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            var factura = cobro.Factura(contexto);

            if (parametros.ContainsKey(nameof(VariableDeFacturasEmt.enumMotivoTransicion)))
                factura.TransitarPorMotivo(contexto, VariableDeFacturasEmt.TransicionesPorMotivo, (Enum)parametros[nameof(VariableDeFacturasEmt.enumMotivoTransicion)]);
            else
            //1. Devuelvo el cobro de una factura en estado Cobrada
            if (factura.EstaEnLaEtapa(enumEtapasDeFacturasEmt.FAE_Etapa_Cobrada) && cobro.IdFacturaRemesada is null)
            {
                if (factura.EstaParcialmenteCobrada(contexto))
                    factura = factura.EliminarUnCobroParcialDeUnaFacturaCobrada(contexto, parametros);
                else
                    factura = factura.EliminarUnCobroTotalDeUnaFacturaCobrada(contexto, parametros);
            }
            //2.Devuelvo el cobro de una factura en estado parcl. cobrada
            else if (factura.EstaEnLaEtapa(enumEtapasDeFacturasEmt.FAE_Etapa_Pago_Parcial))
            {
                if (factura.EstaPendiente(contexto))
                {
                    factura = factura.EstaVencida(contexto)
                    ? factura.DevolverAVencidaTrasAnularUnCobroParcial(contexto, parametros)
                    : factura.TransitarALaEtapa(contexto, enumEtapasDeFacturasEmt.FAE_Etapa_Emitida.EstadosDeLaEtapa(), parametros);
                }
                else if (factura.EstaVencida(contexto))
                    factura.DevolverAVencidaTrasAnularUnCobroParcial(contexto, parametros);
            }

            if (factura.IdContrato.Entero() > 0)
            {
                var devueltoSinIva = factura.Bi(contexto) * cobro.Cobrado / factura.APagar(contexto);
                var contrato = contexto.SeleccionarPorId<ContratoDtm>(factura.IdContrato.Entero());
                contrato.RecalcularAvance(contexto, enumAvaceOperacion.AnularCobro, incremento: (decimal)0.0, devueltoSinIva);
            }

            return factura;
        }

        /* Comentarios tras cobrar una factura       
        4. Pago de una factura emitida   
           - Si la salda
             transito a la etapa cobrada: emitida --> cobrada
           - Si no la salda
             Recalculo el vencimiento
        	 transito a parcialmentecobrada: emitida a parcial
        
        5. Pago de una factura parcial
           - Si la salda
             transito a la etapa cobrada: parcial --> cobrada
           - Si no la salda
             Recalculo vencimiento
        
        6. Pago de una factura vencida
           - Si la salda
             transito a la etapa cobrada: vencida --> cobrada
           - Si no la salda
             Recalculo vencimiento
        	 Transito a parcial cobrada: vencida --> parcial (etapa pdt. pago- etapa de reclamación - etapa emitida)
           
        7. Pago de una factura en reclamación
           - Si la salda
             transito a la etapa cobrada: en reclamación --> cobrada
           - Si no la salda
             Recalculo vencimiento
        	 Transito a parcial cobrada: en reclamación --> parcial (etapa pdt. pago- etapa de reclamación - etapa emitida)
        8. Pago una factura remesada--> paso a cobrada
        
        **/
        public static FacturaEmtDtm TrasRealizarUnCobro(this CobroDeFaeDtm cobro, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            var factura = cobro.DetalleDe<FacturaEmtDtm>(contexto);
            if (factura.EstaEnLaEtapa(enumEtapasDeFacturasEmt.FAE_Etapa_Emitida))
                factura = factura.TransitarUnaFacturaEmitida(contexto, parametros);
            else if (!factura.EstaRectificada(contexto))
            {
                if (factura.EstaEnLaEtapa(enumEtapasDeFacturasEmt.FAE_Etapa_Pago_Parcial))
                    factura = factura.TransitarUnaFacturaParcialmenteCobrada(contexto, parametros);
                else if (factura.EstaEnLaEtapa(enumEtapasDeFacturasEmt.FAE_Etapa_Remesada))
                    factura = (FacturaEmtDtm)factura.TransitarPorMotivo(contexto, VariableDeFacturasEmt.TransicionesPorMotivo, VariableDeFacturasEmt.enumMotivoTransicion.AbonarPagoRemesado);
                else
                    factura = factura.TransitarUnaFacturaVencidaOReclamada(contexto, parametros);
            }

            if (factura.IdContrato.Entero() > 0)
            {
                var cobradoSinIva = factura.Bi(contexto) * cobro.Cobrado / factura.APagar(contexto);
                var contrato = contexto.SeleccionarPorId<ContratoDtm>(factura.IdContrato.Entero());
                contrato.RecalcularAvance(contexto, enumAvaceOperacion.CobrarFactura, incremento: cobradoSinIva, decremento: (decimal)0.0);
            }
            return factura;
        }


        private static FacturaEmtDtm EliminarUnCobroTotalDeUnaFacturaCobrada(this FacturaEmtDtm factura, ContextoSe contexto, Dictionary<string, object> parametros)
        =>
        factura.EstaVencida(contexto)
        ? factura.TransitarALaEtapa(contexto, enumEtapasDeFacturasEmt.FAE_Etapa_De_Reclamacion.EstadosDeLaEtapa(), parametros)
        : factura.TransitarALaEtapa(contexto, enumEtapasDeFacturasEmt.FAE_Etapa_Emitida.EstadosDeLaEtapa(), parametros);

        private static FacturaEmtDtm TransitarUnaFacturaEmitida(this FacturaEmtDtm factura, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            if (factura.EstaCobrada(contexto))
            {
                factura = (FacturaEmtDtm)factura.TransitarPorMotivo(contexto, VariableDeFacturasEmt.TransicionesPorMotivo, VariableDeFacturasEmt.enumMotivoTransicion.RealizarPagoTotal);
                return factura;
            }

            return factura.TransitarTrasHacerUnPagoParcial(contexto, parametros);
        }

        private static FacturaEmtDtm TransitarUnaFacturaVencidaOReclamada(this FacturaEmtDtm factura, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            if (factura.EstaCobrada(contexto))
                return factura.TransitarALaEtapa(contexto, enumEtapasDeFacturasEmt.FAE_Etapa_Cobrada.EstadosDeLaEtapa(), parametros);

            factura = factura.RecalcularVencimiento(contexto, estoyCobrando: true);
            var resultado = factura.IntentarAplicarTransicion(contexto, 
                TransicionAplicable.Transiciones(VariableDeFacturasEmt.TransicionesPorMotivo, VariableDeFacturasEmt.enumMotivoTransicion.RealizarPagoParcial, errorSiNoHay: true));

            return resultado.elemento;
        }

        private static FacturaEmtDtm TransitarUnaFacturaParcialmenteCobrada(this FacturaEmtDtm factura, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            if (factura.EstaCobrada(contexto))
                return factura.TransitarALaEtapa(contexto, enumEtapasDeFacturasEmt.FAE_Etapa_Cobrada.EstadosDeLaEtapa(), parametros);

            return factura.RecalcularVencimiento(contexto, estoyCobrando: true);
        }


    }
}
