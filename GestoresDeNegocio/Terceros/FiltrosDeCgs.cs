using GestorDeElementos;
using ServicioDeDatos;
using System.Linq;
using System.Collections.Generic;
using ServicioDeDatos.Terceros;
using Utilidades;
using Gestor.Errores;
using ServicioDeDatos.Seguridad;
using System;

namespace GestoresDeNegocio.Terceros
{
    internal static class FiltrosDeCgs
    {

        public static IQueryable<CentroGestorDtm> FiltrarLosCgsSeleccionables(this IQueryable<CentroGestorDtm> consulta, ContextoSe contexto,  List<ClausulaDeFiltrado> filtros)
        {
            if (contexto.DatosDeConexion.EsAdministrador)
                return consulta;

            var filtro = filtros.FirstOrDefault(filtro => filtro.Clausula.ToLower().Equals(nameof(NegociosDeUnCgDtm.Negocio).ToLower()));
            if (filtro != null)
            {
                var negocio = enumNegocio.No_Definido;
                var partes = filtro.Valor.Split(Simbolos.PuntoComa);
                if (partes.Length != 2)
                    GestorDeErrores.Emitir("No es valído el filtro a aplicar por CG, necestita indicar el negocio y el modo solicitado");
                if (Enum.TryParse(partes[0], out negocio))
                {
                    var idNegocio = NegociosDeSe.IdNegocio(negocio);
                    enumModoDeAccesoDeDatos modo = enumModoDeAccesoDeDatos.SinPermiso;
                    if (Enum.TryParse(partes[1], out modo))
                    {
                        consulta = consulta.Where(x => contexto.Set<PermisosPorCgDtm>().Any(y => y.IdCg.Equals(x.Id)
                        && y.IdNegocio.Equals(idNegocio)
                        && y.IdUsuario.Equals(contexto.DatosDeConexion.IdUsuario)
                                                        && contexto.Set<NegociosDeUnCgDtm>().Any(t => modo.Equals(enumModoDeAccesoDeDatos.Gestor)
                                                                                                      ? t.IdGestor.Equals(y.IdPermiso)
                                                                                                      : t.IdConsultor.Equals(y.IdPermiso)
                                                                                                   && t.IdCg.Equals(x.Id)
                                                                                                   && t.IdNegocio.Equals(idNegocio))));
                    }
                    else
                    {
                        consulta = consulta.Where(x => false);
                    }
                }
                else
                {
                    consulta = consulta.Where(x => false);
                }
                filtro.Aplicado = true;
            }

            return consulta;
        }

    }
}
