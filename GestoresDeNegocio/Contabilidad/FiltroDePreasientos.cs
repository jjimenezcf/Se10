using GestorDeElementos;
using GestorDeElementos.Extensores;
using ModeloDeDto;
using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Gastos;
using ServicioDeDatos.Ventas;
using System.Collections.Generic;
using System.Linq;
using Utilidades;

namespace GestoresDeNegocio.Contabilidad
{
    public static class FiltroDePreasientos
    {
        public static IQueryable<PreasientoDtm> FiltroPorReferenciado(this IQueryable<PreasientoDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(f => f.Clausula.ToLower() == ltrDeUnPreasiento.FiltroPorReferenciado.ToLower());
            if (filtro != null)
            {
                var filtroValor = filtro.Valor.ToLower().Trim();

                consulta = consulta.Where(spr =>
                   (spr.NegocioReferenciado == enumNegocio.FacturaEmitida && contexto.Set<FacturaEmtDtm>().Any(emt => emt.Id == spr.IdReferenciado && emt.Referencia.Contains(filtroValor))) ||
                   (spr.NegocioReferenciado == enumNegocio.FacturaRecibida && contexto.Set<FacturaRecDtm>().Any(rec => rec.Id == spr.IdReferenciado && rec.Referencia.Contains(filtroValor))) ||
                   (spr.NegocioReferenciado == enumNegocio.Pago && contexto.Set<PagoDtm>().Any(pag => pag.Id == spr.IdReferenciado && pag.Referencia.Contains(filtroValor)))
                   );

                filtro.Aplicado = true;
            }
            return consulta;
        }

        public static IQueryable<PreasientoDtm> FiltroPorLoteContable(this IQueryable<PreasientoDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(f => f.Clausula.ToLower() == ltrDeUnPreasiento.FiltroLoteContable.ToLower());
            if (filtro != null)
            {
                var idLote = filtro.Valor.Entero();

                consulta = consulta.Where(spr => contexto.Set<CircuitoDocDeUnPreasientoDtm>().Any(cp => cp.idElemento2 == idLote && cp.idElemento1 == spr.Id));

                filtro.Aplicado = true;
            }
            return consulta;
        }

        public static IQueryable<PreasientoDtm> FiltroPorPreasientoReferenciaCuentaApunte(this IQueryable<PreasientoDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtroPorNombre = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrFiltros.Nombre.ToLower() && !x.Aplicado);
            if (filtroPorNombre != null && !filtros.Exists(x => x.Clausula.ToLower() == ltrDeUnPreasiento.NombreCuentaApunte.ToLower()))
                filtroPorNombre.Clausula = ltrDeUnPreasiento.NombreCuentaApunte;

            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnPreasiento.NombreCuentaApunte.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                if (filtro.Valor.ToLower().StartsWith("p:"))
                    consulta = consulta.Where(f => f.Nombre.ToLower().Contains(filtro.Valor.Substring(2).Trim().ToLower()));
                else if (filtro.Valor.ToLower().StartsWith("c:"))
                {
                    var apuntes = contexto.Set<ApunteDeUnPreasientoDtm>().Where(apunte => apunte.Cuenta.Contains(filtro.Valor.Substring(2).Trim()));
                    consulta = consulta.Where(p => apuntes.Any(apunte => apunte.IdElemento == p.Id));
                }
                else if (filtro.Valor.ToLower().StartsWith("r:"))
                    consulta = consulta.Where(f => f.Referencia.ToLower().Contains(filtro.Valor.Substring(2).Trim().ToLower()));
                else if (filtro.Valor.ToLower().StartsWith("a:"))
                {
                    var apuntes = contexto.Set<ApunteDeUnPreasientoDtm>().Where(apunte => apunte.Concepto.ToLower().Contains(filtro.Valor.Substring(2).ToLower().Trim()));
                    consulta = consulta.Where(p => apuntes.Any(apunte => apunte.IdElemento == p.Id));
                }
                else
                    consulta = consulta.Where(f => f.Nombre.ToLower().Contains(filtro.Valor.Substring(2).Trim().ToLower()));
                filtro.Aplicado = true;
            }

            return consulta;
        }

        public static IQueryable<PreasientoDtm> FiltroEntreImportes(this IQueryable<PreasientoDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnPreasiento.FiltroEntreImporte.ToLower() && !x.Valor.IsNullOrEmpty());
            if (filtro != null)
            {
                var rango = filtro.ParsearRango();

                consulta = consulta.Where(spr => contexto.Set<ApunteDeUnPreasientoDtm>().Any(apt => apt.IdElemento == spr.Id && 
                                      (rango.desde == null ? true : apt.Importe >= rango.desde) &&
                                      (rango.hasta == null ? true : apt.Importe < rango.hasta) &&
                                      (apt.Clase.Equals(enumClaseDeApunte.Far_Proveedor) || apt.Clase.Equals(enumClaseDeApunte.Pag_Acreedor) ||
                                       apt.Clase.Equals(enumClaseDeApunte.Fae_Cliente) || apt.Clase.Equals(enumClaseDeApunte.Cob_Deudor))
                                      ));
                filtro.Aplicado = true;
            }
            return consulta;
        }

    }
}
