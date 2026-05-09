using System;
using System.Linq;
using ServicioDeDatos.Ventas;
using ServicioDeDatos;
using Utilidades;
using GestoresDeNegocio.Terceros;
using ServicioDeDatos.Terceros;
using System.Collections.Generic;
using GestorDeElementos;
using ServicioDeDatos.Elemento;
using GestorDeElementos.Extensores;

namespace GestoresDeNegocio.Ventas
{
    public static class FiltrosDeAsignacionesPtr
    {

        public static IQueryable<AsignacionDePtrDtm> FiltroPorParteTr(this IQueryable<AsignacionDePtrDtm> consulta, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnaAsignacion.IdParteTr.ToLower());
            if (filtro != null)
            {
                consulta = consulta.Where(x => x.IdElemento == filtro.Valor.Entero());
                filtro.Aplicado = true;
            }
            return consulta;
        }

        public static IQueryable<AsignacionDePtrDtm> FiltroPorContrato(this IQueryable<AsignacionDePtrDtm> consulta, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnaAsignacion.IdContrato.ToLower());
            if (filtro != null)
            {
                consulta = consulta.Where(x => x.Elemento.IdContrato == filtro.Valor.Entero());
                filtro.Aplicado = true;
            }

            filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnaAsignacion.FiltroPorConOSinContrato.ToLower());
            return filtro != null ? consulta.FiltroSiElDetalleDependeDe(filtro, nameof(FacturaEmtDtm.IdContrato)) : consulta;
        }


        public static IQueryable<AsignacionDePtrDtm> FiltroPorPresupuesto(this IQueryable<AsignacionDePtrDtm> consulta, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnaAsignacion.IdContrato.ToLower());
            if (filtro != null)
            {
                consulta = consulta.Where(x => x.Elemento.IdContrato == filtro.Valor.Entero());
                filtro.Aplicado = true;
            }

            filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnaAsignacion.FiltroPorConOSinPresupuesto.ToLower());
            return filtro != null ? consulta.FiltroSiElDetalleDependeDe(filtro, nameof(FacturaEmtDtm.IdPresupuesto)) : consulta;
        }

        public static IQueryable<AsignacionDePtrDtm> FiltroPorUnitario(this IQueryable<AsignacionDePtrDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnaAsignacion.IdUnitario.ToLower());
            if (filtro != null)
            {
                consulta = consulta.Where(a => contexto.Set<LineaDeUnPtrDtm>().Where(x => x.IdElemento == a.IdElemento && x.IdUnitario == filtro.Valor.Entero()).Any());
                filtro.Aplicado = true;
            }
            return consulta;
        }

        public static IQueryable<AsignacionDePtrDtm> FiltroPorCliente(this IQueryable<AsignacionDePtrDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnaAsignacion.IdCliente.ToLower());
            if (filtro != null)
            {
                consulta = consulta.Where(a => contexto.Set<ParteTrDtm>().Where(x => x.IdCliente == filtro.Valor.Entero()).Any(y => y.Id == a.IdElemento));
                filtro.Aplicado = true;
            }
            return consulta;
        }

        public static IQueryable<AsignacionDePtrDtm> FiltroPorTrabajador(this IQueryable<AsignacionDePtrDtm> consulta, List<ClausulaDeFiltrado> filtros)

        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula == nameof(AsignacionDePtrDtm.IdTrabajador));
            if (filtro != null)
            {
                consulta = consulta.Where(x => x.IdTrabajador == filtro.Valor.Entero());
                filtro.Aplicado = true;
            }
            return consulta;
        }

        public static IQueryable<AsignacionDePtrDtm> FiltroPorNombreDeTrabajador(this IQueryable<AsignacionDePtrDtm> consulta, ContextoSe contexto, ClausulaDeFiltrado filtro)
        {
            var personas = (ApiDeTerceros.ValidarNif(filtro.Valor).IsNullOrEmpty()
                ? contexto.Set<PersonaDtm>().Where(y => y.NIF == filtro.Valor)
                : filtro.Valor.Contains("@")
                ? contexto.Set<PersonaDtm>().Where(y => y.eMail.Contains(filtro.Valor))
                : contexto.Set<PersonaDtm>().Where(y => y.Id == -1));

            var interlocutores = !filtro.Valor.Contains("@")
            ? contexto.Set<InterlocutorDtm>().Where(x => (personas.FirstOrDefault() != null && x.IdPersona == personas.FirstOrDefault().Id))
            : contexto.Set<InterlocutorDtm>().Where(x => (personas.FirstOrDefault() != null && x.IdPersona == personas.FirstOrDefault().Id) ||
            x.eMail.Contains(filtro.Valor));

            consulta = consulta.Where(x => contexto.Set<TrabajadorDtm>().Where(y => y.Nombre.Contains(filtro.Valor) ||
                                           (interlocutores.FirstOrDefault() != null && y.IdInterlocutor == interlocutores.FirstOrDefault().Id)
                                     ).Any(z => z.Id == x.IdTrabajador));
            filtro.Aplicado = true;
            return consulta;
        }

        public static IQueryable<AsignacionDePtrDtm> FiltroPorEstadoDeLaAsignacion(this IQueryable<AsignacionDePtrDtm> consulta, List<ClausulaDeFiltrado> filtros)
        {
            var filtroSoloEje = filtros.FirstOrDefault(x => x.Clausula == ltrDeUnParteTr.MostrarPtrsAsignadosEjecutados);
            var filtroSoloPdt = filtros.FirstOrDefault(x => x.Clausula == ltrDeUnParteTr.MostrarPtrsAsignadosPendientes);
            var mostrarEje = filtroSoloEje != null ? (bool)filtroSoloEje.Valor.TrueOrNull(nuloEsFalse: true) : true;
            var mostrarPdt = filtroSoloPdt != null ? (bool)filtroSoloPdt.Valor.TrueOrNull(nuloEsFalse: true) : true;
            if (mostrarEje && mostrarPdt)
                return consulta;

            if (filtroSoloEje != null)
            {
                consulta = consulta.Where(x => mostrarEje ? x.Finalizada != null : x.Finalizada == null);
                filtroSoloEje.Aplicado = true;
            }

            if (filtroSoloPdt != null)
            {
                consulta = consulta.Where(x => mostrarPdt ? x.Finalizada == null && x.Iniciada == null : x.Iniciada != null);
                filtroSoloPdt.Aplicado = true;
            }


            return consulta;
        }
    }
}
