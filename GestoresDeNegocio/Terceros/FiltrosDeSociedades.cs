using GestorDeElementos;
using ServicioDeDatos;
using System.Linq;
using System.Collections.Generic;
using Utilidades;
using ModeloDeDto.Entorno;
using ServicioDeDatos.Terceros;
using ModeloDeDto.Terceros;
using ServicioDeDatos.Callejero;

namespace GestoresDeNegocio.Terceros
{
    internal static class FiltrosDeSociedades
    {
        public static IQueryable<SociedadDtm> FiltrarPorPuestoDeCliente(this IQueryable<SociedadDtm> consulta, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeSociedad.FiltroParaSociedadesDeClientes.ToLower());
            if (filtro != null)
            {
                consulta = filtroInternoPorExpresion(consulta, filtro);
                consulta = consulta.Where(x => !x.Baja);
                filtro.Aplicado = true;
            }
            return consulta;
        }

        public static IQueryable<SociedadDtm> FiltrarPorExpresion(this IQueryable<SociedadDtm> consulta, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == UsuariosPor.NombreCompleto.ToLower() || x.Clausula.ToLower() == nameof(SociedadDtm.Expresion).ToLower());
            if (filtro != null)
            {
                consulta = filtroInternoPorExpresion(consulta, filtro);
                filtro.Aplicado = true;
            }
            return consulta;
        }

        private static IQueryable<SociedadDtm> filtroInternoPorExpresion(IQueryable<SociedadDtm> consulta, ClausulaDeFiltrado filtro)
        {
            if (ApiDeTerceros.CifValido(filtro.Valor))
                consulta = consulta.AplicarPredicado(filtro, x => x.NIF.ToLower() == filtro.Valor);
            else if (filtro.Valor.EsEntero())
                consulta = consulta.AplicarPredicado(filtro, x => x.eMail.Contains(filtro.Valor) || x.Telefono.Contains(filtro.Valor));
            else if (filtro.Valor.Contains("@"))
                consulta = consulta.AplicarPredicado(filtro, x => x.eMail.Contains(filtro.Valor));
            else
                consulta = consulta.AplicarPredicado(filtro, x => x.RazonSocial.Contains(filtro.Valor) || x.Nombre.Contains(filtro.Valor) || x.eMail.Contains(filtro.Valor));
            return consulta;
        }

        public static IQueryable<SociedadDtm> FiltrarPorCg(this IQueryable<SociedadDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeSociedad.FiltoPorCg.ToLower());
            if (filtro != null)
            {
                consulta = consulta.Where(x => contexto.Set<CentroGestorDtm>().Any(p => p.IdSociedad.Equals(x.Id)));
                filtro.Aplicado = true;
            }

            return consulta;
        }

        public static IQueryable<SociedadDtm> FiltrarPorConContactos(this IQueryable<SociedadDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeSociedad.ConContactos.ToLower());
            if (filtro != null)
            {
                consulta = consulta.Where(x => contexto.Set<ContactoDtm>().Any(p => p.IdSociedad.Equals(x.Id)));
                filtro.Aplicado = true;
            }

            return consulta;
        }

        public static IQueryable<SociedadDtm> FiltrarPorConCertificados(this IQueryable<SociedadDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeSociedad.ConCertificado.ToLower());
            if (filtro != null)
            {
                consulta = consulta.Where(x => contexto.Set<CertificadosDeUnaSociedadDtm>().Any(p => p.idElemento1.Equals(x.Id)));
                filtro.Aplicado = true;
            }

            return consulta;
        }

        public static IQueryable<SociedadDtm> FiltrarPorSerInterlocutor(this IQueryable<SociedadDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == nameof(SociedadDto.EsInterlocutor).ToLower());
            if (filtro != null)
            {
                consulta = consulta.Where(x => contexto.Set<InterlocutorDtm>().Where(i => i.IdSociedad == x.Id).Any());
                filtro.Aplicado = true;
            }

            return consulta;
        }

        public static IQueryable<SociedadDtm> FiltrarPorSociedadesGestionadas(this IQueryable<SociedadDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == nameof(ltrDeSociedad.FiltroParaSociedadesGestionadas).ToLower());
            if (filtro != null)
            {
                consulta = consulta.Where(soc => contexto.Set<CentroGestorDtm>().Any(cg => cg.IdSociedad == soc.Id));
                if (!filtro.Valor.EsTrue())
                {
                    consulta = filtroInternoPorExpresion(consulta, filtro);
                }

                filtro.Aplicado = true;
            }

            return consulta;
        }

        
        public static IQueryable<SociedadDtm> FiltrarPorCif(this IQueryable<SociedadDtm> consulta, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == nameof(SociedadDtm.NIF).ToLower());
            if (filtro != null)
            {
                if (filtro.Valor.ToUpper().StartsWith(ltrIsoPaises.Spain))
                    consulta = consulta.Where(x => x.NIF.ToLower() == filtro.Valor.Substring(2).ToLower() || x.NIF.ToLower() == filtro.Valor.ToLower());
                else
                    consulta = consulta.Where(x => x.NIF.ToLower() == filtro.Valor.ToLower() || x.NIF.ToLower() == (ltrIsoPaises.Spain + filtro.Valor).ToLower());

                filtro.Aplicado = true;
            }

            return consulta;
        }

    }
}
