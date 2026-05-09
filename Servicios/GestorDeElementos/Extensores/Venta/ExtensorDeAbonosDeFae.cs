using Gestor.Errores;
using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Gastos;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.Ventas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using Utilidades;
using static Gestor.Errores.GestorDeErrores;

namespace GestorDeElementos.Extensores
{
    public static class ExtensorDeAbonosDeFae
    {
        public static FacturaEmtDtm Factura(this AbonoDeFaeDtm abono, ContextoSe contexto, bool aplicarJoin = false)
        {
            if (abono.FacturaEmt == null)
            {
                abono.FacturaEmt = contexto.SeleccionarPorId<FacturaEmtDtm>(abono.idElemento1, aplicarJoin: aplicarJoin);
            }
            return abono.FacturaEmt;
        }
        public static PagoDtm Pago(this AbonoDeFaeDtm abono, ContextoSe contexto, bool aplicarJoin = false)
        {
            if (abono.Pago == null)
            {
                abono.Pago = contexto.SeleccionarPorId<PagoDtm>(abono.idElemento2, aplicarJoin: aplicarJoin);
            }
            return abono.Pago;
        }

        public static FacturaEmtDtm FacturaAbonada(this PagoDtm pago, ContextoSe contexto, bool errorSiNoHay = true)
        {
            var facturas = pago.Vinculos(contexto, enumNegocio.FacturaEmitida);
            if (facturas.Count == 0)
            {
               if (errorSiNoHay) Emitir($"El pago '{pago.Referencia}' no está asociado a ninguna factura");
               return null;
            }
            
            if (facturas.Count > 1)
            {
                var referencias = new List<string>();
                foreach (var factura in facturas)
                {
                    referencias.Add(contexto.SeleccionarPorId<FacturaEmtDtm>(factura.Id).Referencia);
                }
                Emitir($"El pago '{pago.Referencia}' está asociado a factura a '{string.Join(Simbolos.PuntoComa, referencias)}' y sólo puede estar a una");
            }

            return contexto.SeleccionarPorId<FacturaEmtDtm>(facturas[0].idElemento1);
        }

        public static bool EsAbono(this PagoDtm pago, ContextoSe contexto)
        {
            return pago.IdCliente.HasValue;
        }

        public static CuentaBancariaDtm CuentaDeCargo(this AbonoDeFaeDtm abono, ContextoSe contexto, bool errorSiNoHay = true)
        {
            var pagoDelAbono = abono.Pago(contexto);

            if (pagoDelAbono.IdCuentaDePago is null && errorSiNoHay)
                Emitir($"Se ha solicitado la cuenta de cargo para el abono {pagoDelAbono.Referencia}, y no está identificada");

            if (pagoDelAbono.IdCuentaDePago is null)
                return null;

            return pagoDelAbono.CuentaBancariaDePago(contexto);
        }

        public static CuentaBancariaDtm CuentaDeAbono(this AbonoDeFaeDtm abono, ContextoSe contexto, bool errorSiNoHay = true)
        {
            var pagoDelAbono = abono.Pago(contexto);

            if (pagoDelAbono.IdCuentaDeAcreedor is null && errorSiNoHay)
                Emitir($"Se ha solicitado la cuenta de abono para el abono {pagoDelAbono.Referencia}, y no está identificada");

            if (pagoDelAbono.IdCuentaDeAcreedor is null)
                return null;

            return pagoDelAbono.CuentaBancariaAcreedora(contexto);
        }

        public static void ValidarAbono(this FacturaEmtDtm factura, ContextoSe contexto, PagoDtm pago)
        {
            if (factura.EsRectificativa == false)
                Emitir($"la factura '{factura.Referencia}' no es rectificativa. No se puede abonar");

            if (pago.Clase == enumClaseDePago.Remesa)
                Emitir($"No se pueden crear abonos usando '{enumClaseDePago.Remesa.Descripcion()}', realícelo en '{enumClaseDePago.Contado.Descripcion()}' o por '{enumClaseDePago.Transferencia.Descripcion()}'");

            if (!pago.IdCliente.HasValue)
                Emitir($"No se pueden crear abonos si no se indica el cliente'");

            var maximo = factura.PendientePorAbonar(contexto);

            var valorAbonado = pago.Importe;
            if (valorAbonado <= 0)
                Emitir($"El Abono '{pago.Referencia}' no puede ser menor o igual a cero");

            var abonoMaximo = factura.PendientePorAbonar(contexto);

            if (abonoMaximo <= 0 && Math.Abs(abonoMaximo) > VariableDeFacturasEmt.ToleranciaDeCobro)
                Emitir($"No se puede abonar la factura '{factura.Referencia}' ya que no hay nada pendiente, o por que no se ha cobrado la factura o lo cobrado ya ha sido abonado");

            if (abonoMaximo - valorAbonado < 0 && Math.Abs(abonoMaximo - valorAbonado) > VariableDeFacturasEmt.ToleranciaDeCobro)
                Emitir($"No se puede abonar la factura '{factura.Referencia}' por  valor de '{valorAbonado.ToMoneda()}' ya que solo queda pendiente por devolver '{abonoMaximo.ToMoneda()}'");

            if (pago.PagadoEl.HasValue && pago.PagadoEl < ((DateTime)factura.FacturadaEl).Date)
                Emitir($"No se puede crear el abono ya que la fecha de abono '{pago.PagadoEl.FechaCorta()}' es anterior a la emisión de la factura '{factura.FacturadaEl.FechaCorta()}");

            if (pago.PagarEl.HasValue && pago.PagarEl < ((DateTime)factura.FacturadaEl).Date)
                Emitir($"No se puede crear el abono ya que la fecha de prevista de pago '{pago.PagarEl.FechaCorta()}' es anterior a la emisión de la factura '{factura.FacturadaEl.FechaCorta()}");

            if (pago.PagadoEl.HasValue && pago.PagadoEl.Fecha().Date >= DateTime.Now)
                Emitir($"No se puede crear un abono con fecha futura");

            if (pago.Clase == enumClaseDePago.Transferencia)
            {
                if (pago.IdCuentaDeAcreedor is null)
                    Emitir($"El abono '{pago.Referencia}' debe indicar la cuenta de ingreso");

                if (pago.IdCuentaDePago is null)
                    Emitir($"El abono '{pago.Referencia}' debe indicar la cuenta de cargo");

                var sociedad = pago.Sociedad(contexto);
                var cbDeMiSociedad = sociedad.Detalles<CuentaDeMiSociedadDtm>(contexto).FirstOrDefault(x => x.Id == pago.IdCuentaDePago);
                if (cbDeMiSociedad is null)
                    Emitir($"La cuenta de cargo del abono '{pago.Referencia}' debe ser de la sociedad que hace el abono");

                if (!cbDeMiSociedad.Activa)
                    Emitir($"La cuenta de cargo '{pago.CuentaBancariaDePago(contexto).NumeroIban}' del abono '{pago.Referencia}' debe estar activa");
            }
        }

        public static decimal Pendiente(this AbonoDeFaeDtm abono, ContextoSe contexto)
        {
            var abonado = abono.Factura(contexto).Abonado(contexto);
            var heCobrado = abono.Factura(contexto).Cobrado(contexto);
            return heCobrado - abonado <= 0 ? 0 : heCobrado - abonado;
        }

        public static bool HayPagos(this FacturaEmtDtm rectificativa, ContextoSe contexto)
        {
            return rectificativa.HayVinculos(contexto, enumNegocio.Pago);
        }

        public static decimal Abonado(this FacturaEmtDtm rectificativa, ContextoSe contexto)
        {
            if (rectificativa is null)
                return 0;

            var cache = ServicioDeCaches.Obtener(CacheDe.Fae_Abonado);
            if (!cache.ContainsKey(rectificativa.Id.ToString()))
            {
                var abonosRealizados = rectificativa.Vinculados<PagoDtm>(contexto);
                decimal importeAbonado = 0;
                foreach (var abonoRealizado in abonosRealizados)
                {
                    if (abonoRealizado.Estado(contexto).Cancelado)
                        continue;
                    importeAbonado += abonoRealizado.Importe;
                }

                cache[rectificativa.Id.ToString()] = importeAbonado;
            }
            return (decimal)cache[rectificativa.Id.ToString()];
        }

        public static decimal PendientePorAbonar(this FacturaEmtDtm rectificativa, ContextoSe contexto)
        {
            if (!rectificativa.EsRectificativa)
                return 0;

            if (rectificativa.EstaEnLaEtapa(enumEtapasDeFacturasEmt.FAE_Etapa_Anulada))
                return 0;

            decimal loQueNoHayQueAbonar = 0;
            var factura = rectificativa.RectificaA(contexto);
            var cobrado = factura.Cobrado(contexto);
            var abonado = rectificativa.Abonado(contexto);
            var diferencia = cobrado - abonado;
            if (rectificativa.ClaseRectificativa == enumClaseDeRectificativa.OC)
            {
                loQueNoHayQueAbonar = factura.APagar(contexto) + rectificativa.APagar(contexto);
                diferencia = diferencia - loQueNoHayQueAbonar;
            }

            return diferencia <= 0 || Math.Abs(diferencia) < VariableDeFacturasEmt.ToleranciaDeCobro ? 0 : diferencia;
        }

        public static bool EstaAbonada(this FacturaEmtDtm rectificativa, ContextoSe contexto, bool errorSiNoEsRectificativa)
        {
            if (!rectificativa.EsRectificativa)
                Emitir($"La factura '{rectificativa.Referencia}' no se puede dar por abonada por no ser rectificativa");

            return rectificativa.PendientePorAbonar(contexto) <= VariableDeFacturasEmt.ToleranciaDeCobro;
        }

        public static void TrasRealizarUnAbono(this AbonoDeFaeDtm abono, ContextoSe contexto, Dictionary<string, object> dictionary)
        {
            if (abono.Factura(contexto).PendientePorAbonar(contexto)<= VariableDeFacturasEmt.ToleranciaDeCobro)
            {
                abono.Factura(contexto).TransitarALaEtapa(contexto, enumEtapasDeFacturasEmt.FAE_Etapa_Abonada.EstadosDeLaEtapa());
            }
        }

        public static void TrasEliminarUnAbono(this AbonoDeFaeDtm abono, ContextoSe contexto, Dictionary<string, object> dictionary)
        {
            throw new NotImplementedException();
        }


    }
}
