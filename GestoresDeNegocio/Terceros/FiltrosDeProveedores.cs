using GestorDeElementos;
using ServicioDeDatos;
using System.Linq;
using System.Collections.Generic;
using ServicioDeDatos.Terceros;
using ModeloDeDto.Terceros;
using Utilidades;
using GestorDeElementos.Extensores;
using Gestor.Errores;
using ModeloDeDto;
using ServicioDeDatos.Callejero;

namespace GestoresDeNegocio.Terceros
{
    internal static class FiltrosDeProveedores
    {
        public static IQueryable<ProveedorDtm> FiltrarPorExpresion(this IQueryable<ProveedorDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == nameof(ProveedorDto.Expresion).ToLower());
            if (filtro is not null)
            {
                filtros.Add(new ClausulaDeFiltrado { Clausula = ltrProveedor.NIF, Criterio = enumCriteriosDeFiltrado.igual, Valor = filtro.Valor });
                consulta = consulta.FiltrarPorNif(contexto, filtros, emitirError: false);

                if (filtro.Valor.EsEntero())
                    consulta = consulta.AplicarPredicado(filtro, x => x.Interlocutor.Persona.eMail.Contains(filtro.Valor)
                                                              || x.Interlocutor.Sociedad.eMail.Contains(filtro.Valor)
                                                              || x.Interlocutor.Persona.Telefono.Contains(filtro.Valor)
                                                              || x.Interlocutor.Sociedad.Telefono.Contains(filtro.Valor));
                else if (filtro.Valor.Contains("@"))
                    consulta = consulta.AplicarPredicado(filtro, x => x.Interlocutor.Persona.eMail.Contains(filtro.Valor)
                                                              || x.Interlocutor.Sociedad.eMail.Contains(filtro.Valor));
                else
                    consulta = consulta.AplicarPredicado(filtro, x => x.Interlocutor.Persona.Apellidos.Contains(filtro.Valor)
                                                                   || x.Interlocutor.Sociedad.Nombre.Contains(filtro.Valor));
            }

            return consulta;
        }


        public static IQueryable<ProveedorDtm> FiltrarPorNif(this IQueryable<ProveedorDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros, bool emitirError)
        {
            var filtroPorNif = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrProveedor.NIF.ToLower());
            if (filtroPorNif is not null)
            {
                filtroPorNif.Aplicado = true;
                IQueryable<InterlocutorDtm> interlocutor = null;
                if (ApiDeTerceros.ValidarNif(filtroPorNif.Valor).IsNullOrEmpty())
                {
                    var sinEs = filtroPorNif.Valor;
                    if (sinEs.Length == 11 && sinEs.StartsWith(ltrIsoPaises.Spain)) sinEs = sinEs.Right(9);
                    interlocutor = filtroPorNif.Valor != sinEs
                    ? contexto.Set<InterlocutorDtm>()
                            .Where(inter => contexto.Set<PersonaDtm>()
                            .Any(per => (per.NIF.ToLower() == filtroPorNif.Valor || per.NIF.ToLower() == sinEs) && inter.IdPersona == per.Id))
                    : contexto.Set<InterlocutorDtm>()
                            .Where(inter => contexto.Set<PersonaDtm>()
                            .Any(per => per.NIF.ToLower() == filtroPorNif.Valor && inter.IdPersona == per.Id));
                }
                else
                if (ApiDeTerceros.CifValido(filtroPorNif.Valor))
                {

                    var sinEs = filtroPorNif.Valor;
                    if (sinEs.Length == 11 && sinEs.StartsWith(ltrIsoPaises.Spain)) sinEs = sinEs.Right(9);
                    if (filtroPorNif.Valor != sinEs)
                        interlocutor = contexto.Set<InterlocutorDtm>()
                            .Where(inter => contexto.Set<SociedadDtm>()
                            .Any(soc => (soc.NIF.ToLower() == filtroPorNif.Valor || soc.NIF.ToLower() == sinEs) && inter.IdSociedad == soc.Id && inter.IdContacto == null));
                    else
                        interlocutor = contexto.Set<InterlocutorDtm>().Where(inter => contexto.Set<SociedadDtm>().Any(soc => soc.NIF.ToLower() == filtroPorNif.Valor && inter.IdSociedad == soc.Id && inter.IdContacto == null));


                }
                else
                if (emitirError)
                    GestorDeErrores.Emitir($"El NIF '{filtroPorNif.Valor}' no es válido");
                else
                    return consulta;
                consulta = consulta.Where(prov => interlocutor.Any(inter => inter.Id == prov.IdInterlocutor));
            }
            return consulta;
        }

        public static IQueryable<ProveedorDtm> FiltrarPorIdSociedad(this IQueryable<ProveedorDtm> consulta, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrProveedor.IdSociedad.ToLower());
            if (filtro is not null)
            {
                consulta = consulta.AplicarPredicado(filtro, x => x.Interlocutor.IdSociedad == filtro.Valor.Entero() && x.Interlocutor.IdContacto == null);
            }
            return consulta;
        }

        public static IQueryable<ProveedorDtm> FiltrarPorIdPersona(this IQueryable<ProveedorDtm> consulta, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrProveedor.IdPersona.ToLower());
            if (filtro is not null)
            {
                consulta = consulta.AplicarPredicado(filtro, x => x.Interlocutor.IdPersona == filtro.Valor.Entero());
            }
            return consulta;
        }

        public static IQueryable<ProveedorDtm> FiltrarPorVinculadosCon(this IQueryable<ProveedorDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrFiltros.VincularCon.ToLower());
            if (filtro is not null)
            {
                var vinculos = NegociosDeSe.ToEnumerado(filtro.IdNegocio).Proveedores(contexto).Where(x => x.idElemento1 == filtro.IdElemento);
                consulta = consulta.ElementosNoVinculadosDeLaMismaSociedad(contexto, filtro, vinculos);
            }
            return consulta;
        }
    }
}
