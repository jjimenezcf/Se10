using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.Juridico;
using ServicioDeDatos.Presupuesto;
using ServicioDeDatos.Tarea;
using ServicioDeDatos.Ventas;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilidades;

namespace GestoresDeNegocio.Ventas
{
    public static class FiltrosDePresupuestos
    {

        public static IQueryable<PresupuestoDtm> FiltroPorFactura(this IQueryable<PresupuestoDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {

            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnPresupuesto.FiltroPorConOSinFacturaEmt.ToLower());
            if (filtro != null)
            {
                consulta = consulta.Where(x => x.ClaseDePresupuesto == enumClaseDePresupuesto.venta);
                if (filtro.Valor.Entero() == ltrParametrosNeg.ConRelacion)
                    consulta = consulta.Where(x => contexto.Set<FacturaEmtDtm>().Any(y => y.IdPresupuesto == x.Id));
                if (filtro.Valor.Entero() == ltrParametrosNeg.SinRelacion)
                    consulta = consulta.Where(x => !contexto.Set<FacturaEmtDtm>().Any(y => y.IdPresupuesto == x.Id));
                filtro.Aplicado = true;
            }


            filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnPresupuesto.IdFacturaEmt.ToLower());
            if (filtro != null)
            {
                consulta = consulta.Where(ppt => contexto.Set<FacturaEmtDtm>().Any(fae => fae.IdPresupuesto == ppt.Id && fae.Id == filtro.Valor.Entero()));
                filtro.Aplicado = true;
            }
            return consulta;
        }


        public static IQueryable<PresupuestoDtm> FiltroParaImputarFactura(this IQueryable<PresupuestoDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnPresupuesto.SelectorParaUnaFacturaEmt.ToLower());
            if (filtro != null)
            {
                consulta = consulta.Where(x => x.ClaseDePresupuesto == enumClaseDePresupuesto.venta);
                var etapas = filtro.Valor.Split(Simbolos.separadorDeEtapas);
                var estados = ApiDeEnsamblados.ToEnumerado<enumEtapasDePpts>(etapas[0]).Estados();
                for (var i = 1; i < etapas.Length; i++) estados = estados + Simbolos.Coma + ApiDeEnsamblados.ToEnumerado<enumEtapasDePpts>(etapas[i]).Estados();
                var filtroPorEstados = new ClausulaDeFiltrado(nameof(IUsaEstado.IdEstado), enumCriteriosDeFiltrado.esAlgunoDe, estados);
                consulta = consulta.AplicarFiltroPorEntero(filtroPorEstados);

                filtro.Aplicado = true;
            }
            return consulta;
        }

        public static IQueryable<PresupuestoDtm> FiltroPorPartes(this IQueryable<PresupuestoDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {

            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnPresupuesto.FiltroPorConOSinParteTr.ToLower());
            if (filtro != null)
            {
                consulta = consulta.Where(x => x.ClaseDePresupuesto == enumClaseDePresupuesto.venta);
                if (filtro.Valor.Entero() == ltrParametrosNeg.ConRelacion)
                    consulta = consulta.Where(x => contexto.Set<ParteTrDtm>().Any(y => y.IdPresupuesto == x.Id));
                if (filtro.Valor.Entero() == ltrParametrosNeg.SinRelacion)
                    consulta = consulta.Where(x => !contexto.Set<ParteTrDtm>().Any(y => y.IdPresupuesto == x.Id));

                if (filtro.Valor.Equals(ltrDeUnPresupuesto.ConPartesPdtDeFacturar, StringComparison.InvariantCultureIgnoreCase))
                {                    
                    consulta = consulta.Where(x => contexto.Set<ParteTrDtm>()
                                          .Any(y => y.IdPresupuesto == x.Id &&
                                                   VariableDePartesTr.Lista(enumEtapasDePartesTr.PTR_Etapa_Pdt_Facturar).Contains(y.IdEstado)));
                }
                

                filtro.Aplicado = true;
            }


            filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnPresupuesto.IdParteTr.ToLower());
            if (filtro != null)
            {
                consulta = consulta.Where(ppt => contexto.Set<ParteTrDtm>().Any(ptr => ptr.IdPresupuesto == ppt.Id && ptr.Id == filtro.Valor.Entero()));
                filtro.Aplicado = true;
            }
            return consulta;
        }

        public static IQueryable<PresupuestoDtm> FiltroPorPartesPrefacturados(this IQueryable<PresupuestoDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnPresupuesto.FiltroPorPartesPrefacturados.ToLower());

            if (filtro != null)
            {
                var estadosPrefactura = enumEtapasDeFacturasEmt.FAE_Etapa_Prefactura.Lista();

                if (!estadosPrefactura.Any())
                    GestorDeErrores.Emitir($"Debe definir el parámetro '{enumEtapasDeFacturasEmt.FAE_Etapa_Prefactura}' del negocio de '{enumNegocio.FacturaEmitida}'");

                var partesPorFactura = contexto.Set<FacturaEmtDtm>().Where(f => f.IdParteTr != null && estadosPrefactura.Contains(f.IdEstado)).Select(f => f.IdParteTr.Value);

                var partesPorLinea = contexto.Set<LineaDeUnaFaeDtm>().Where(l =>l.IdParteTr != null &&  contexto.Set<FacturaEmtDtm>().Any(f =>f.Id == l.IdElemento && estadosPrefactura.Contains(f.IdEstado)))
                    .Select(l => l.IdParteTr.Value);

                var partesPrefacturadosIds = partesPorFactura
                    .Union(partesPorLinea)
                    .Distinct();

                consulta = consulta.Where(p =>
                    contexto.Set<ParteTrDtm>().Any(parte =>
                        parte.IdPresupuesto == p.Id &&
                        partesPrefacturadosIds.Contains(parte.Id)
                    )
                );
            }

            return consulta;
        }
    }
}
