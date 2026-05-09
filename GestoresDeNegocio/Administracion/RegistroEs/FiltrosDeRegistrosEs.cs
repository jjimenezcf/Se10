using GestorDeElementos.Extensores;
using GestorDeElementos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos;
using System;
using System.Linq;
using Utilidades;

namespace GestoresDeNegocio.RegistroEs
{
    internal static class FiltrosDeRegistrosEs
    {
        public static IQueryable<T> RegistrosEsRelacionados<T>(this IQueryable<T> consulta, ContextoSe Contexto, ClausulaDeFiltrado filtro)
        where T : RegistroDtm
        {
            if (!ApiDeInterfaceDtm.ImplementaUsaFlujo(typeof(T)))
                return consulta;

            var negocio = typeof(T).NegocioDeUnDtm();
            if (negocio.UsaRegistrosEs())
            {

                if (filtro.Clausula.Equals(ltrDeUnRegistroEs.MostraRegistrosEsRelacionados, StringComparison.CurrentCultureIgnoreCase))
                {
                    if (filtro.Valor.Entero() == ltrParametrosNeg.ConRelacion)
                        consulta = consulta.Where(x => negocio.RegistrosEs(Contexto).Any(y => y.idElemento1 == x.Id));
                    if (filtro.Valor.Entero() == ltrParametrosNeg.SinRelacion)
                        consulta = consulta.Where(x => !negocio.RegistrosEs(Contexto).Any(y => y.idElemento1 == x.Id));
                    filtro.Aplicado = true;
                }

                if (filtro.Clausula.Equals(ltrDeUnRegistroEs.IdRegistroEs, StringComparison.CurrentCultureIgnoreCase))
                {
                    consulta = consulta.Where(x => negocio.RegistrosEs(Contexto).Any(y => y.idElemento1 == x.Id && y.idElemento2 == filtro.Valor.Entero()));
                    filtro.Aplicado = true;
                }
            }
            return consulta;
        }
    }
}
