using Gestor.Errores;
using ServicioDeDatos;
using ServicioDeDatos.Seguridad;

namespace GestoresDeNegocio.Seguridad
{
    public static class AccionesDeSeguridad
    {
        public const string N_OtorgarPermisosDeConsultorAlPt = "Otorga permisos de consultor";
        public const string N_OtorgarPermisosDeGestorAlPt = "Otorga permisos de gestor";
        public const string N_QuitarPermisosDeConsultorAlPt = "Quitar permisos de consultor";
        public const string N_QuitarPermisosDeGestorAlPt = "Quitar permisos de gestor";

        public static void OtorgarPermisosDeConsultorAlPt(EntornoDeUnaAccion entorno)
        {
            if (!entorno.Parametros.ContainsKey(nameof(PermisosDirectosDtm.IdPuesto)))
                GestorDeErrores.Emitir($"Error en {nameof(OtorgarPermisosDeConsultorAlPt)} :Debe indicar el Id del puesto de trabajo al que otorgar seguridad");
            
            var idPt = (int)(long)entorno.Parametros[nameof(PermisosDirectosDtm.IdPuesto)];
            GestorDePermisosDirectos.OtorgarPermisos(entorno.Contexto, entorno.Negocio, entorno.Registro.Id,idPt, enumModoDeAccesoDeDatos.Consultor);
        }

        public static void OtorgarPermisosDeGestorAlPt(EntornoDeUnaAccion entorno)
        {
            if (!entorno.Parametros.ContainsKey(nameof(PermisosDirectosDtm.IdPuesto)))
                GestorDeErrores.Emitir($"Error en {nameof(OtorgarPermisosDeGestorAlPt)} :Debe indicar el Id del puesto de trabajo al que otorgar seguridad");

            var idPt = (int)(long)entorno.Parametros[nameof(PermisosDirectosDtm.IdPuesto)];
            GestorDePermisosDirectos.OtorgarPermisos(entorno.Contexto, entorno.Negocio, entorno.Registro.Id, idPt, enumModoDeAccesoDeDatos.Gestor);
        }

        public static void QuitarPermisosDeConsultorAlPt(EntornoDeUnaAccion entorno)
        {
            if (!entorno.Parametros.ContainsKey(nameof(PermisosDirectosDtm.IdPuesto)))
                GestorDeErrores.Emitir($"Error en {nameof(QuitarPermisosDeConsultorAlPt)} :Debe indicar el Id del puesto de trabajo al que quitar la seguridad");

            var idPt = (int)(long)entorno.Parametros[nameof(PermisosDirectosDtm.IdPuesto)];
            GestorDePermisosDirectos.QuitarPermisos(entorno.Contexto, entorno.Negocio, entorno.Registro.Id, idPt, enumModoDeAccesoDeDatos.Consultor);
        }

        public static void QuitarPermisosDeGestorAlPt(EntornoDeUnaAccion entorno)
        {
            if (!entorno.Parametros.ContainsKey(nameof(PermisosDirectosDtm.IdPuesto)))
                GestorDeErrores.Emitir($"Error en {nameof(QuitarPermisosDeGestorAlPt)} :Debe indicar el Id del puesto de trabajo al que quitar la seguridad");

            var idPt = (int)(long)entorno.Parametros[nameof(PermisosDirectosDtm.IdPuesto)];
            GestorDePermisosDirectos.QuitarPermisos(entorno.Contexto, entorno.Negocio, entorno.Registro.Id, idPt, enumModoDeAccesoDeDatos.Gestor);
        }


    }
}
