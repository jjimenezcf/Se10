using GestorDeElementos;
using GestorDeElementos.Extensores;
using ServicioDeDatos;
using ServicioDeDatos.Logistica;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilidades;

namespace GestoresDeNegocio.Logistica
{
    public static class FiltrosDeDevolucionesDeVenta
    {
        // devoluciones en tramitación: las que se proponen al devolver una línea vendida (ver ExtensorDeDevolucionesDeVenta.DevolverLineaDeVenta)
        public static IQueryable<DevolucionDeVentaDtm> FiltroEnTramitacion(this IQueryable<DevolucionDeVentaDtm> consulta, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => !x.Aplicado && x.Clausula.Equals(ltrDeUnaDevolucionDeVenta.FiltroEnTramitacion, StringComparison.CurrentCultureIgnoreCase));
            if (filtro != null)
            {
                var estados = enumEtapasDeDevolucionDeVenta.DDV_Etapa_Tramitacion.Lista();
                consulta = consulta.Where(x => estados.Contains(x.IdEstado));
                filtro.Aplicado = true;
            }
            return consulta;
        }

        // devoluciones de un cliente
        public static IQueryable<DevolucionDeVentaDtm> FiltroPorCliente(this IQueryable<DevolucionDeVentaDtm> consulta, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => !x.Aplicado && x.Clausula.Equals(ltrDeUnaDevolucionDeVenta.FiltroPorCliente, StringComparison.CurrentCultureIgnoreCase));
            if (filtro != null)
            {
                var idCliente = filtro.Valor.Entero();
                consulta = consulta.Where(x => x.IdCliente == idCliente);
                filtro.Aplicado = true;
            }
            return consulta;
        }

        // líneas devueltas de un documento de venta: el grid de devoluciones de la edición del documento de venta, que las restringe por
        // IdVenta (la línea vendida es del documento de venta)
        public static IQueryable<LineaDeUnaDevolucionDeVentaDtm> FiltroPorVenta(this IQueryable<LineaDeUnaDevolucionDeVentaDtm> consulta, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => !x.Aplicado && x.Clausula.Equals(ltrDeUnaDevolucionDeVenta.FiltroPorVenta, StringComparison.CurrentCultureIgnoreCase));
            if (filtro != null)
            {
                var idVenta = filtro.Valor.Entero();
                consulta = consulta.Where(x => x.LineaDeVenta.IdElemento == idVenta);
                filtro.Aplicado = true;
            }
            return consulta;
        }
    }
}
