
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos;
using System.Collections.Generic;
using ServicioDeDatos.Entorno;

namespace GestorDeElementos.Extensores
{
    public static class ExtensorDeSeguridad
    {
        public static List<PermisosDeUnRolDtm> AsignarPermisosDeUnTipo<T>(this RolDtm rol, ContextoSe contexto, string nombreTipo, enumModoDeAccesoDeDatos modo)
        where T : TipoDeElementoDtm
        {
            var permisosDeUnRol = new List<PermisosDeUnRolDtm>();
            var tipo = contexto.SeleccionarPorPropiedad<T>(nameof(INombre.Nombre), nombreTipo, aplicarJoin: true);
            permisosDeUnRol.Add(contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, ModoDeAcceso.SoyGestor(modo)
                ? tipo.IdPermisoDeGestor
                : tipo.IdPermisoDeConsultor)
                );

            if (ModoDeAcceso.SoyGestor(modo))
                permisosDeUnRol.Add(contexto.CrearRelacion<PermisosDeUnRolDtm>(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, tipo.IdPermisoDeConsultor));

            return permisosDeUnRol;
        }

        public static PermisoDtm Permiso(this VistaMvcDtm vista, ContextoSe contexto)
        {
            if (vista.Permiso != null && vista.Permiso.Id == vista.IdPermiso)
                return vista.Permiso;

            vista.Permiso  = contexto.SeleccionarPorId<PermisoDtm>(vista.IdPermiso, aplicarJoin: true);
            return vista.Permiso;
        }


    }
}
