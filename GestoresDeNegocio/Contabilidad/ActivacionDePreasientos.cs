using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Ventas;
using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Gastos;
using ServicioDeDatos.Logistica;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.Ventas;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilidades;

namespace GestoresDeNegocio.Contabilidad
{
    // Lo que depende de la clase de preasiento al activarlos en una sociedad (trabajo ActivarPreasientosDeLaSociedad): qué documentos de la
    // sociedad hay que preasentar, qué parámetros se necesitan para hacerlo y cómo se preasienta y graba cada documento. Los documentos son
    // los que se habrían preasentado si la sociedad hubiese generado preasientos desde el principio
    public static class ActivacionDePreasientos
    {
        public static readonly List<enumNegocio> NegociosQueGeneranPreasientos = new()
        {
            enumNegocio.FacturaEmitida, enumNegocio.FacturaRecibida, enumNegocio.Cobro, enumNegocio.Pago, enumNegocio.Almacen
        };

        // Documentos de la sociedad sin preasiento que se han de preasentar, por orden de su fecha contable
        public static List<IUsaPreasiento> DocumentosSinPreasiento(ContextoSe contexto, int idSociedad, enumNegocio negocio)
        {
            var cgsDeLaSociedad = CgsDeLaSociedad(contexto, idSociedad);
            switch (negocio)
            {
                case enumNegocio.FacturaEmitida:
                    return EmitidasSinPreasiento(contexto, cgsDeLaSociedad).OrderBy(f => f.EmitidaEl).ThenBy(f => f.Id).ToList<IUsaPreasiento>();
                case enumNegocio.FacturaRecibida:
                    return RecibidasSinPreasiento(contexto, cgsDeLaSociedad).OrderBy(f => f.ContabilizadaEl).ThenBy(f => f.Id).ToList<IUsaPreasiento>();
                case enumNegocio.Cobro:
                    return CobrosSinPreasiento(contexto, cgsDeLaSociedad).OrderBy(c => c.CobradoEl).ThenBy(c => c.Id).ToList<IUsaPreasiento>();
                case enumNegocio.Pago:
                    return PagosSinPreasiento(contexto, cgsDeLaSociedad).OrderBy(p => p.PagadoEl).ThenBy(p => p.Id).ToList<IUsaPreasiento>();
                case enumNegocio.Almacen:
                    return MovimientosSinPreasiento(contexto, cgsDeLaSociedad).ToList<IUsaPreasiento>();
            }
            throw new Exception($"El negocio '{negocio}' no genera preasientos");
        }

        // La sociedad tiene activados negocios con documentos sin preasiento: la información contable ofrece relanzar la activación
        public static bool HayPreasientosPorActivar(ContextoSe contexto, SociedadDtm sociedad) =>
            !sociedad.Autonomo && NegociosConPreasientosPorActivar(contexto, sociedad).Any();

        public static IEnumerable<enumNegocio> NegociosConPreasientosPorActivar(ContextoSe contexto, SociedadDtm sociedad) =>
            NegociosQueGeneranPreasientos.Where(n => sociedad.GeneraPreasientos(n) == true && HayDocumentosSinPreasiento(contexto, sociedad.Id, n));

        // Como DocumentosSinPreasiento pero sin leerlos
        public static bool HayDocumentosSinPreasiento(ContextoSe contexto, int idSociedad, enumNegocio negocio)
        {
            var cgsDeLaSociedad = CgsDeLaSociedad(contexto, idSociedad);
            switch (negocio)
            {
                case enumNegocio.FacturaEmitida: return EmitidasSinPreasiento(contexto, cgsDeLaSociedad).Any();
                case enumNegocio.FacturaRecibida: return RecibidasSinPreasiento(contexto, cgsDeLaSociedad).Any();
                case enumNegocio.Cobro: return CobrosSinPreasiento(contexto, cgsDeLaSociedad).Any();
                case enumNegocio.Pago: return PagosSinPreasiento(contexto, cgsDeLaSociedad).Any();
                case enumNegocio.Almacen: return MovimientosSinPreasiento(contexto, cgsDeLaSociedad).Count > 0;
            }
            throw new Exception($"El negocio '{negocio}' no genera preasientos");
        }

        // Parámetros que usa Preasentar para los documentos indicados que no están definidos o no son válidos (los mensajes de error que darían)
        public static List<string> ParametrosQueFaltan(ContextoSe contexto, SociedadDtm sociedad, enumNegocio negocio, List<IUsaPreasiento> documentos)
        {
            var faltan = new List<string>();
            void Validar(Action validacion)
            {
                try { validacion(); }
                catch (Exception e) { if (!faltan.Contains(e.Message)) faltan.Add(e.Message); }
            }

            if (documentos.Count == 0)
                return faltan;

            var tiposDePreasiento = enumNegocio.Preasiento.Tipos(contexto);
            void ValidarTipoDePreasiento(int idTipoPreasiento, string origen)
            {
                var tipoDePreasiento = tiposDePreasiento.FirstOrDefault(x => x.Id == idTipoPreasiento);
                if (tipoDePreasiento is null)
                    GestorDeErrores.Emitir($"el tipo de preasiento '{idTipoPreasiento}' indicado {origen} no existe");
                VariablesDePreasiento.CodigoDeDiario(tipoDePreasiento, contexto);
            }

            void ValidarCodigoFiscal()
            {
                if (sociedad.CodigoFiscal.IsNullOrEmpty())
                    GestorDeErrores.Emitir($"el 'código fiscal' de la sociedad '{sociedad.Expresion}' en contabilidad");
            }

            switch (negocio)
            {
                case enumNegocio.FacturaEmitida:
                    foreach (var emitida in documentos.Cast<FacturaEmtDtm>().GroupBy(f => f.IdTipo).Select(g => g.First()))
                        Validar(() => ValidarTipoDePreasiento(emitida.IdTipoPreasiento(contexto), $"en el parámetro '{enumParametrosDeFacturasEmt.FAE_Relacion_Con_Tipo_De_Preasiento}' para el tipo de factura '{emitida.IdTipo}'"));
                    Validar(VariablesDePreasiento.ValidarNaturalezasContables);
                    break;

                case enumNegocio.FacturaRecibida:
                    foreach (var recibida in documentos.Cast<FacturaRecDtm>().GroupBy(f => f.IdTipo).Select(g => g.First()))
                        Validar(() => ValidarTipoDePreasiento(recibida.IdTipoPreasiento(contexto), $"para el tipo de factura recibida '{recibida.IdTipo}'"));
                    Validar(VariablesDePreasiento.ValidarNaturalezasContables);
                    break;

                case enumNegocio.Cobro:
                    var cobros = documentos.Cast<CobroDeFaeDtm>().ToList();
                    Validar(() => ValidarTipoDePreasiento(cobros[0].IdTipoPreasiento(contexto), $"en el parámetro '{enumParametrosDeFacturasEmt.COB_Relacion_Con_Tipo_De_Preasiento}'"));
                    Validar(ValidarCodigoFiscal);
                    // cuenta contable de la caja y de cada cuenta bancaria de ingreso
                    foreach (var cobro in cobros.GroupBy(c => (c.Clase, c.IdCuentaDeIngreso)).Select(g => g.First()))
                        Validar(() => cobro.CuentaContableDelCobro(contexto));
                    break;

                case enumNegocio.Pago:
                    var pagos = documentos.Cast<PagoDtm>().ToList();
                    foreach (var pago in pagos.GroupBy(p => p.IdTipo).Select(g => g.First()))
                        Validar(() => ValidarTipoDePreasiento(pago.IdTipoPreasiento(contexto), $"en el parámetro '{enumParametrosDePagos.PAG_Relacion_Con_Tipo_De_Preasiento}' para el tipo de pago '{pago.IdTipo}'"));
                    Validar(ValidarCodigoFiscal);
                    // cuenta contable de cada tarjeta, cuenta bancaria o caja con la que se paga
                    foreach (var pago in pagos.GroupBy(p => (p.IdCuentaDePago, p.IdTarjetaDePago)).Select(g => g.First()))
                        Validar(() => pago.CuentaContableDelPago(contexto));
                    break;

                case enumNegocio.Almacen:
                    Validar(() => ValidarTipoDePreasiento(VariablesDePreasiento.IdTipoPreasientoDelNegocio(enumNegocio.Almacen.IdNegocio(), enumNegocio.Almacen.Singular()), $"para el negocio '{enumNegocio.Almacen.Singular()}'"));
                    Validar(VariablesDePreasiento.ValidarNaturalezasContables);
                    break;
            }
            return faltan;
        }

        // Genera el preasiento del documento como antiguo (con su fecha contable, sin ajustarla al último cierre) y graba el documento con la
        // acción con la que lo hace la opción de regenerar su preasiento, para que su gestor no le quite el preasiento
        public static PreasientoDtm Preasentar(ContextoSe contexto, IUsaPreasiento documento, enumNegocio negocio)
        {
            switch (negocio)
            {
                case enumNegocio.FacturaEmitida:
                    var emitida = ((FacturaEmtDtm)documento).Preasentar(contexto, generandoPreasientosAntiguos: true);
                    emitida.ModificarComoAdministrador(contexto, accionQueSeEjecuta: nameof(GestorDeFacturasEmt.GenerarPreasiento));
                    return emitida.Preasiento(contexto);

                case enumNegocio.FacturaRecibida:
                    var recibida = ((FacturaRecDtm)documento).Preasentar(contexto, generandoPreasientosAntiguos: true);
                    recibida.ModificarComoAdministrador(contexto, accionQueSeEjecuta: ltrDeUnaFacturaRec.Accion_GenerarPreasiento);
                    return recibida.Preasiento(contexto);

                case enumNegocio.Cobro:
                    var cobro = ((CobroDeFaeDtm)documento).Preasentar(contexto, generandoPreasientosAntiguos: true);
                    cobro.ModificarComoAdministrador(contexto, accionQueSeEjecuta: nameof(GestorDeCobrosDeFae.GenerarPreasiento));
                    return cobro.Preasiento(contexto);

                case enumNegocio.Pago:
                    var pago = ((PagoDtm)documento).Preasentar(contexto, generandoPreasientosAntiguos: true);
                    pago.ModificarComoAdministrador(contexto, accionQueSeEjecuta: ltrDeUnPago.Accion_GenerarPreasiento);
                    return pago.Preasiento(contexto);

                case enumNegocio.Almacen:
                    // como al regenerar el preasiento de un movimiento: se graba sin recalcular el inventario ni barrer los posteriores
                    var movimiento = ((MovimientoDeAlmacenDtm)documento).Preasentar(contexto, generandoPreasientosAntiguos: true);
                    movimiento.ModificarComoAdministrador(contexto, parametros: new Dictionary<string, object> { { ltrDeUnMovimientoDeAlmacen.EstoyBarriendo, true } });
                    return movimiento.Preasiento(contexto);
            }
            throw new Exception($"El negocio '{negocio}' no genera preasientos");
        }

        public static string Referencia(ContextoSe contexto, IUsaPreasiento documento)
        {
            switch (documento)
            {
                case CobroDeFaeDtm cobro: return $"cobro {cobro.Referencia(contexto)}";
                case MovimientoDeAlmacenDtm movimiento: return $"movimiento {movimiento.Referencia(contexto)}";
                case ElementoConCgDtm elemento: return elemento.Referencia;
            }
            return documento.ToString();
        }

        private static IQueryable<int> CgsDeLaSociedad(ContextoSe contexto, int idSociedad) =>
            contexto.Set<CentroGestorDtm>().Where(cg => cg.IdSociedad == idSociedad).Select(cg => cg.Id);

        // Facturas emitidas de la sociedad que ya se han emitido: ni prefacturas ni anuladas
        private static IQueryable<FacturaEmtDtm> EmitidasPreasentables(ContextoSe contexto, IQueryable<int> cgsDeLaSociedad)
        {
            var noPreasentables = (enumEtapasDeFacturasEmt.FAE_Etapa_Prefactura.Estados() + "," + enumEtapasDeFacturasEmt.FAE_Etapa_Anulada.Estados()).ToLista<int>(separador: ",");
            return contexto.Set<FacturaEmtDtm>().Where(f => f.FacturadaEl != null && cgsDeLaSociedad.Contains(f.IdCg) && !noPreasentables.Contains(f.IdEstado));
        }

        private static IQueryable<FacturaEmtDtm> EmitidasSinPreasiento(ContextoSe contexto, IQueryable<int> cgsDeLaSociedad) =>
            EmitidasPreasentables(contexto, cgsDeLaSociedad).Where(f => f.IdPreasiento == null);

        // en etapa de pago o pagadas, con fecha contable
        private static IQueryable<FacturaRecDtm> RecibidasSinPreasiento(ContextoSe contexto, IQueryable<int> cgsDeLaSociedad)
        {
            var preasentables = (enumEtapasDeFacturasRec.FAR_Etapa_De_Pago.Estados() + "," + enumEtapasDeFacturasRec.FAR_Etapa_Pagada.Estados()).ToLista<int>(separador: ",");
            return contexto.Set<FacturaRecDtm>().Where(f => f.IdPreasiento == null && f.ContabilizadaEl != null && cgsDeLaSociedad.Contains(f.IdCg) && preasentables.Contains(f.IdEstado));
        }

        // al registrar un cobro solo se preasientan los de contado y por transferencia
        private static IQueryable<CobroDeFaeDtm> CobrosSinPreasiento(ContextoSe contexto, IQueryable<int> cgsDeLaSociedad)
        {
            var facturas = EmitidasPreasentables(contexto, cgsDeLaSociedad).Select(f => f.Id);
            return contexto.Set<CobroDeFaeDtm>()
                .Where(c => c.IdPreasiento == null && (c.Clase == enumClaseDeCobro.Contado || c.Clase == enumClaseDeCobro.Transferencia) && facturas.Contains(c.IdElemento));
        }

        private static IQueryable<PagoDtm> PagosSinPreasiento(ContextoSe contexto, IQueryable<int> cgsDeLaSociedad)
        {
            var pagados = enumEtapasDePagos.PAG_Etapa_Pagado.Estados().ToLista<int>(separador: ",");
            return contexto.Set<PagoDtm>().Where(p => p.IdPreasiento == null && p.PagadoEl != null && cgsDeLaSociedad.Contains(p.IdCg) && pagados.Contains(p.IdEstado));
        }

        // de momento solo se preasientan las entradas de compra (las que vienen de la recepción de una línea de pedido), y no se pueden
        // preasentar movimientos de almacenes cancelados o cerrados
        private static List<MovimientoDeAlmacenDtm> MovimientosSinPreasiento(ContextoSe contexto, IQueryable<int> cgsDeLaSociedad)
        {
            var entradas = contexto.Set<TipoMovimientoDtm>().Where(t => t.ClaseMovimiento == enumClaseDeMovimiento.Entrada).Select(t => t.Id);
            var almacenes = contexto.Set<AlmacenDtm>().Where(a => cgsDeLaSociedad.Contains(a.IdCg)).Select(a => a.Id);
            return contexto.Set<MovimientoDeAlmacenDtm>()
                .Where(m => m.IdPreasiento == null && m.IdLineaDeRecepcion != null && entradas.Contains(m.IdTipoMovimiento) && almacenes.Contains(m.IdAlmacen))
                .OrderBy(m => m.RealizadoEl).ThenBy(m => m.Id)
                .ToList()
                .Where(m => !m.Almacen(contexto).EstaEnLaEtapa(enumEtapasDeAlmacen.ALM_Etapa_Cancelado) && !m.Almacen(contexto).EstaEnLaEtapa(enumEtapasDeAlmacen.ALM_Etapa_Cerrado))
                .ToList();
        }
    }
}
