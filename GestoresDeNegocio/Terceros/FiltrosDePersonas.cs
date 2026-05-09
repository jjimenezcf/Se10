using ServicioDeDatos;
using System.Linq;
using System.Collections.Generic;
using ServicioDeDatos.Terceros;
using Utilidades;

namespace GestoresDeNegocio.Terceros
{
    internal static class FiltrosDePersonas
    {
        public static IQueryable<PersonaDtm> FiltrarPorNombreMadrePadre(this IQueryable<PersonaDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDePersonas.FiltarPorNombreDeMadrePadre.ToLower());
            if (filtro != null)
            {
                if (!filtro.Valor.IsNullOrEmpty()) consulta = consulta.Where(x => x.Nombre.Contains(filtro.Valor) ||
                                   x.Apellidos.Contains(filtro.Valor) ||
                                   x.NIF.Contains(filtro.Valor));
                filtro.Aplicado = true;
            }
            return consulta;
        }


    }
}
