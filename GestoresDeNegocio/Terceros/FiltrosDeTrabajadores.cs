using GestorDeElementos;
using ServicioDeDatos;
using System.Linq;
using System.Collections.Generic;
using ServicioDeDatos.Terceros;
using ModeloDeDto.Terceros;
using Utilidades;
using ModeloDeDto.Ventas;
using ModeloDeDto;
using GestorDeElementos.Extensores;

namespace GestoresDeNegocio.Terceros
{
    internal static class FiltrosDeTrabajadores
    {
        public static IQueryable<TrabajadorDtm> FiltrarPorTrabajadoresNoAsignados(this IQueryable<TrabajadorDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrFiltros.VincularCon);
            if (filtro != null)
            {

                filtro.Aplicado = true;
            }
            return consulta;
        }

        public static IQueryable<TrabajadorDtm> FiltrarPorSociedadDelTrabajador(this IQueryable<TrabajadorDtm> consulta, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == nameof(AsignacionDePtrDto.IdSociedadDelCg).ToLower());
            if (filtro != null)
            {
                consulta = consulta.AplicarPredicado(filtro, x => x.Cg.IdSociedad == filtro.Valor.Entero());
                filtro.Aplicado = true;
            }
            return consulta;
        }
        public static IQueryable<TrabajadorDtm> FiltrarPorPersona(this IQueryable<TrabajadorDtm> consulta, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrFiltros.VincularCon);
            if (filtro != null)
            {
                consulta = consulta.AplicarPredicado(filtro, x => x.Interlocutor.IdPersona == filtro.Valor.Entero());
                filtro.Aplicado = true;
            }
            return consulta;
        }

        public static IQueryable<TrabajadorDtm> FiltrarPorExpresion(this IQueryable<TrabajadorDtm> consulta, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == nameof(InterlocutorDto.Expresion).ToLower());
            if (filtro != null)
            {
                if (ApiDeTerceros.ValidarNif(filtro.Valor).IsNullOrEmpty())
                    consulta = consulta.AplicarPredicado(filtro, x => x.Interlocutor.Persona.NIF.ToLower() == filtro.Valor);
                else if (filtro.Valor.EsEntero())
                    consulta = consulta.AplicarPredicado(filtro, x => x.Interlocutor.Persona.eMail.Contains(filtro.Valor)
                                                              || x.Interlocutor.Persona.Telefono.Contains(filtro.Valor));
                else if (filtro.Valor.Contains("@"))
                    consulta = consulta.AplicarPredicado(filtro, x => x.Interlocutor.Persona.eMail.Contains(filtro.Valor));
                else
                    consulta = consulta.AplicarPredicado(filtro, x => x.Interlocutor.Persona.Apellidos.Contains(filtro.Valor) ||
                                                                      x.Nombre.Contains(filtro.Valor));
                filtro.Aplicado = true;
            }
            return consulta;
        }
    }
}
