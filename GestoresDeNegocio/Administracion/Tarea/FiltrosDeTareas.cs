using GestorDeElementos;
using GestorDeElementos.Extensores;
using ModeloDeDto;
using ServicioDeDatos;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.Presupuesto;
using ServicioDeDatos.Tarea;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.Ventas;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilidades;

namespace GestoresDeNegocio.Tarea
{
    internal static class FiltrosDeTareas
    {
        public static IQueryable<TareaDtm> FiltroPorPresupuestos(this IQueryable<TareaDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            consulta.FiltroDeElementosConVinculosCon<TareaDtm, TareasDeUnPresupuestoDtm>(contexto, filtros, ltrDeUnaTarea.IdPresupuesto, ltrDeUnaTarea.RelacionadaConPpt);
            var filtroPorNombre = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnaTarea.PresupuestosDeTareas.ToLower());
            if (filtroPorNombre != null)
            {
                var valores = filtroPorNombre.Valor.Split(Simbolos.separadorDeCadenasDeFiltrado, StringSplitOptions.RemoveEmptyEntries).Select(v => v.ToLower()).ToList();
                consulta = consulta.Where(tar =>
                    contexto.Set<PresupuestoDtm>().Any(ppt =>
                        valores.Any(v => (ppt.Nombre.ToLower().Contains(v) || ppt.Referencia.ToLower().Contains(v)) &&
                        contexto.Set<TareasDeUnPresupuestoDtm>().Any(ppt_tar => ppt_tar.idElemento1 == ppt.Id && ppt_tar.idElemento2 == tar.Id))));
                filtroPorNombre.Aplicado = true;
            }

            return consulta;
        }

        public static IQueryable<TareaDtm> FiltroPorEtapa(this IQueryable<TareaDtm> consulta, List<ClausulaDeFiltrado> filtros)
        {
            // Buscamos todos los filtros de etapa que aún no han sido aplicados
            var filtrosEtapa = filtros.Where(x => x.Clausula.ToLower() == ltrDeUnaTarea.FiltroPorEtapa.ToLower() && !x.Aplicado).ToList();

            foreach (var filtro in filtrosEtapa)
            {
                consulta = consulta.FiltrosPorEtapa<TareaDtm>(filtro);
            }

            return consulta;
        }

        public static IQueryable<TareaDtm> FiltroPorExpedientes(this IQueryable<TareaDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            consulta = consulta.FiltroDeElementosConVinculosCon<TareaDtm, TareasDeUnExpedienteDtm>(contexto, filtros, ltrDeUnaTarea.IdExpediente, ltrDeUnaTarea.RelacionadaConExpediente);

            var filtroPorNombre = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnaTarea.ExpedientesDeTareas.ToLower());
            if (filtroPorNombre != null)
            {
                var valores = filtroPorNombre.Valor.Split(Simbolos.separadorDeCadenasDeFiltrado, StringSplitOptions.RemoveEmptyEntries).Select(v => v.ToLower()).ToList();
                consulta = consulta.Where(tar =>
                    contexto.Set<ExpedienteDtm>().Any(exp =>
                        valores.Any(v => (exp.Nombre.ToLower().Contains(v) || exp.Referencia.ToLower().Contains(v)) &&
                        contexto.Set<TareasDeUnExpedienteDtm>().Any(exp_tar => exp_tar.idElemento1 == exp.Id && exp_tar.idElemento2 == tar.Id))));
                filtroPorNombre.Aplicado = true;
            }

            return consulta;
        }

        public static IQueryable<TareaDtm> FiltrarParaVincular(this IQueryable<TareaDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrFiltros.VincularCon.ToLower());
            if (filtro != null)
            {
                var vinculos = NegociosDeSe.ToEnumerado(filtro.IdNegocio).Tareas(contexto).Where(x => x.idElemento1 == filtro.IdElemento);
                consulta = consulta.ElementosNoVinculadosDeLaMismaSociedad(contexto, filtro, vinculos);
                filtro.Aplicado = true;
            }
            return consulta;
        }

        public static IQueryable<TareaDtm> ExcluirTareasYaRelacionadasConExpediente(this IQueryable<TareaDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrFiltros.VincularCon.ToLower());
            if (filtro != null && NegociosDeSe.ToEnumerado(filtro.IdNegocio) == enumNegocio.Expediente)
            {
                consulta = consulta.Where(t => !contexto.Set<TareasDeUnExpedienteDtm>().Any(x => x.idElemento2 == t.Id));
                filtro.Aplicado = true;
            }
            return consulta;
        }

        public static IQueryable<TareaDtm> FiltrarPorPlfDeInicio(this IQueryable<TareaDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrPlfDeTarea.FiltroPorPlfDeInicio.ToLower());
            if (filtro != null)
            {
                var planificaciones = contexto.Set<PlfDeTareaDtm>().AplicarFiltroPorFecha(filtro, nameof(PlfDeTareaDtm.PlfDeInicio));
                consulta = consulta.Where(x => planificaciones.Any(p => p.IdElemento == x.Id));
                filtro.Aplicado = true;
            }
            return consulta;
        }

        public static IQueryable<TareaDtm> FiltrarPorPlfDeFin(this IQueryable<TareaDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrPlfDeTarea.FiltroPorPlfDeFin.ToLower());
            if (filtro != null)
            {
                var planificaciones = contexto.Set<PlfDeTareaDtm>().AplicarFiltroPorFecha(filtro, nameof(PlfDeTareaDtm.PlfDeFin));
                consulta = consulta.Where(x => planificaciones.Any(p => p.IdElemento == x.Id));
                filtro.Aplicado = true;
            }
            return consulta;
        }

        public static IQueryable<TareaDtm> FiltrosDeFacturas(this IQueryable<TareaDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = consulta.FiltroSiHayDependenciaDe(filtros, filtrarPor: ltrDeUnaTarea.IdFacturaEmt, filtroDeAsociacion: ltrDeUnaTarea.Facturada, parametros, aplicarFiltroDeEstado: false);
            var filtroPorNombre = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnaTarea.FacturasDeTareas.ToLower());
            if (filtroPorNombre != null)
            {
                var valores = filtroPorNombre.Valor.Split(Simbolos.separadorDeCadenasDeFiltrado, StringSplitOptions.RemoveEmptyEntries).Select(v => v.ToLower()).ToList();
                consulta = consulta.Where(tarea =>
                    contexto.Set<FacturaEmtDtm>().Any(fact =>
                        valores.Any(v => fact.Nombre.ToLower().Contains(v) || fact.NumeroDeFactura.ToLower().Contains(v) || fact.Referencia.ToLower().Contains(v)) &&
                        tarea.IdFacturaEmt == fact.Id));
                filtroPorNombre.Aplicado = true;
            }

            return consulta;
        }

        public static IQueryable<TareaDtm> FiltrosDeSolicitantes(this IQueryable<TareaDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtroPorId = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnaTarea.IdSolicitante.ToLower());
            if (filtroPorId != null)
            {
                consulta = consulta.Where(tar => tar.IdSolicitante == filtroPorId.Valor.Entero() || contexto.Set<InterlocutoresDeUnaTareaDtm>()
                                              .Any(tar_int => tar_int.idElemento2 == filtroPorId.Valor.Entero() && tar_int.idElemento1 == tar.Id));
                filtroPorId.Aplicado = true;
            }
            else
            {
                var filtroPorNombre = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnaTarea.SolicitantesDeTarea.ToLower());
                if (filtroPorNombre != null)
                {
                    var valores = filtroPorNombre.Valor.Split(Simbolos.separadorDeCadenasDeFiltrado, StringSplitOptions.RemoveEmptyEntries).Select(v => v.ToLower()).ToList();
                    consulta = consulta.Where(tar =>
                        contexto.Set<InterlocutorDtm>().Any(inte =>
                            valores.Any(v => inte.Nombre.ToLower().Contains(v)) &&
                            (tar.IdSolicitante == inte.Id ||
                             contexto.Set<InterlocutoresDeUnaTareaDtm>().Any(y => y.idElemento2 == inte.Id && y.idElemento1 == tar.Id))));
                    filtroPorNombre.Aplicado = true;
                }
            }
            return consulta;
        }

        public static IQueryable<TareaDtm> FiltrosDeResponsables(this IQueryable<TareaDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtroPorLogin = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnaTarea.ResponsablesDeTarea.ToLower());
            if (filtroPorLogin != null)
            {
                var logins = filtroPorLogin.Valor.Split(Simbolos.separadorDeCadenasDeFiltrado, StringSplitOptions.RemoveEmptyEntries).Select(v => v.ToLower()).ToList();
                consulta = consulta.Where(t => contexto.Set<UsuarioDtm>().Any(u => logins.Any(l => u.Login.ToLower().Contains(l)) && t.IdResponsable == u.Id));
                filtroPorLogin.Aplicado = true;
            }
            return consulta;
        }

        public static IQueryable<TareaDtm> FiltroDeVinculadosA(this IQueryable<TareaDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrFiltros.VinculadosA.ToLower());
            if (filtro != null)
            {
                var negocio = NegociosDeSe.ToEnumerado(filtro.Valor.Split(Simbolos.separadorDeValores)[0]);
                var idElemento = filtro.Valor.Split(Simbolos.separadorDeValores)[1].Entero();

                consulta = consulta.Where(elemento => negocio.Tareas(contexto).Where(x => x.idElemento1 == idElemento).Any(vinculo => vinculo.idElemento2 == elemento.Id));
                filtro.Aplicado = true;
            }
            return consulta;
        }
    }
}
