using GestorDeElementos;
using ServicioDeDatos;
using System.Linq;
using System.Collections.Generic;
using ServicioDeDatos.Terceros;
using ModeloDeDto.Terceros;
using Utilidades;
using ModeloDeDto;
using GestorDeElementos.Extensores;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.Elemento;
using Gestor.Errores;

namespace GestoresDeNegocio.Terceros
{
    internal static class FiltrosDeInterlocutores
    {
        public static IQueryable<InterlocutorDtm> FiltrarPorInterlocutoresNoVinculados(this IQueryable<InterlocutorDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrFiltros.VincularCon);
            if (filtro != null)
            {
                var vinculos = NegociosDeSe.ToEnumerado(filtro.IdNegocio).Interlocutores(contexto).Where(x => x.idElemento1 == filtro.IdElemento);
                consulta = consulta.ElementosNoVinculadosDeLaMismaSociedad(contexto, filtro, vinculos);
                filtro.Aplicado = true;
            }
            return consulta;
        }

        public static IQueryable<InterlocutorDtm> FiltrarParaInfantes(this IQueryable<InterlocutorDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(fi => fi.Clausula.ToLower() == ltrInterlocutor.ParaInfante.ToLower());
            if (filtro != null)
            {
                return FiltrarPorExpresion(consulta, contexto, filtro);
            }

            return consulta;
        }

        public static IQueryable<InterlocutorDtm> FiltrarParaExpedientes(this IQueryable<InterlocutorDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtroParaExpediente = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrInterlocutor.ParaExpediente.ToLower());
            if (filtroParaExpediente != null)
            {
                consulta = FiltrarPorExpresion(consulta, contexto, filtroParaExpediente);

                var filtroportipo = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrParametrosEp.Tipo.ToLower());
                if (filtroportipo is not null && filtroportipo.Valor.Entero() > 0)
                {
                    var tipo = contexto.SeleccionarPorId<TipoDeExpedienteDtm>(filtroportipo.Valor.Entero());
                    if (tipo.ClaseDeExpediente == enumClaseDeExpediente.DeCliente)
                    {
                        consulta = consulta.Where(x => contexto.Set<ClienteDtm>().Any(c => c.IdInterlocutor == x.Id));
                    }
                }
            }

            return consulta;
        }

        private static IQueryable<InterlocutorDtm> FiltrarPorExpresion(this IQueryable<InterlocutorDtm> consulta, ContextoSe contexto, ClausulaDeFiltrado filtro)
        {
            if (!filtro.Valor.IsNullOrEmpty())
            {

                if (ApiDeTerceros.ValidarNif(filtro.Valor).IsNullOrEmpty())
                    consulta = consulta.AplicarPredicado(filtro, x => x.Persona.NIF.ToLower() == filtro.Valor);
                else
                    if (ApiDeTerceros.CifValido(filtro.Valor))
                        consulta = consulta.AplicarPredicado(filtro, x => x.Sociedad.NIF.ToLower() == filtro.Valor || x.Contacto.Sociedad.NIF.ToLower() == filtro.Valor);
                    else if (filtro.Valor.EsEntero())
                        consulta = consulta.AplicarPredicado(filtro, x => x.Nombre.Contains(filtro.Valor) 
                                                                  || x.Persona.eMail.Contains(filtro.Valor)
                                                                  || x.Sociedad.eMail.Contains(filtro.Valor)
                                                                  || x.Contacto.eMail.Contains(filtro.Valor)
                                                                  || x.Persona.Telefono.Contains(filtro.Valor)
                                                                  || x.Sociedad.Telefono.Contains(filtro.Valor)
                                                                  || x.Contacto.Telefono.Contains(filtro.Valor));
                    else if (filtro.Valor.Contains("@"))
                        consulta = consulta.AplicarPredicado(filtro, x => x.Persona.eMail.Contains(filtro.Valor)
                                                                  || x.Sociedad.eMail.Contains(filtro.Valor)
                                                                  || x.Contacto.eMail.Contains(filtro.Valor));
                    else
                        consulta = consulta.AplicarPredicado(filtro, x => x.Nombre.Contains(filtro.Valor));
            }
            filtro.Aplicado = true;

            return consulta;
        }


        public static IQueryable<InterlocutorDtm> FiltrarPorContactosDelCliente(this IQueryable<InterlocutorDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrInterlocutor.BuscarPorContactoCliente.ToLower());
            if (filtro != null)
            {
                filtro.Aplicado = true;
                var filtroCliente = filtros.FirstOrDefault(x => x.Clausula.ToLower() == nameof(IDetalle.IdElemento).ToLower());
                if (filtroCliente == null)
                    GestorDeErrores.Emitir($"No se ha indicado el cliente del que obtener sus contactos");

                consulta = consulta.Where(inter => inter.IdSociedad ==
                                                   contexto.Set<InterlocutorDtm>().First(x => x.Id == contexto.Set<ClienteDtm>().First(Cli => Cli.Id == filtroCliente.Valor.Entero()).IdInterlocutor).IdSociedad
                                                && inter.IdContacto != null
                                                && !inter.Baja);
                filtroCliente.Aplicado = true;
            }
            return consulta;
        }

        public static IQueryable<InterlocutorDtm> FiltrarPorNIFDeSociedad(this IQueryable<InterlocutorDtm> consulta, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == nameof(InterlocutorDtm.Sociedad.NIF).ToLower());
            if (filtro != null)
            {
                consulta = consulta.Where(x => x.IdSociedad > 0);
                consulta = consulta.AplicarPredicado(filtro, x => x.Sociedad.NIF.ToLower() == filtro.Valor);
                filtro.Aplicado = true;

                var filtroPorContacto = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrInterlocutor.BuscarPorContacto.ToLower());
                if (filtroPorContacto != null)
                {
                    consulta = consulta.Where(x => filtroPorContacto.Valor.EsTrue() ? x.IdContacto > 0 : x.IdContacto == null);
                    filtroPorContacto.Aplicado = true;
                }

            }
            return consulta;
        }

        public static IQueryable<InterlocutorDtm> FiltrarPorPersona(this IQueryable<InterlocutorDtm> consulta, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrInterlocutor.BuscarPorPersona.ToLower());
            if (filtro != null)
            {
                consulta = consulta.Where(x => x.IdPersona > 0);

                if (ApiDeTerceros.ValidarNif(filtro.Valor).IsNullOrEmpty())
                    consulta = consulta.AplicarPredicado(filtro, x => x.Persona.NIF.ToLower() == filtro.Valor);
                else
                    if (filtro.Valor.EsEntero())
                        consulta = consulta.AplicarPredicado(filtro, x => x.Persona.eMail.Contains(filtro.Valor)
                                                                  || x.Persona.Telefono.Contains(filtro.Valor));
                    else if (filtro.Valor.Contains("@"))
                        consulta = consulta.AplicarPredicado(filtro, x => x.Persona.eMail.Contains(filtro.Valor));
                    else
                        consulta = consulta.AplicarPredicado(filtro, x => x.Persona.Apellidos.Contains(filtro.Valor) || x.Nombre.Contains(filtro.Valor));
                filtro.Aplicado = true;
            }
            return consulta;
        }

    }
}
