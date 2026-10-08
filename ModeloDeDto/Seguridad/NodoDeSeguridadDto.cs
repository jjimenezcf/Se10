using System.Collections.Generic;

namespace ModeloDeDto.Seguridad
{
    public enum enumNodoDeSeguridad
    {
        Usuario,
        Puestos,
        Puesto,
        RolesDelPuesto,
        Rol,
        PermisosHeredados,
        PermisosDirectos,
        PermisosPorNegocio,
        PermisosPorTipo,
        PermisosPorEstado,
        PermisosPorTransicion,
        PermisosPorCg,
        PermisosPorElemento,
        NegocioConPermisos,
        Permiso
    }

    public enum enumObjetoDeSeguridad
    {
        Ninguno,
        Puesto,
        Rol,
        Permiso
    }

    /// <summary>
    /// Nodo del árbol de seguridad de un usuario. Los hijos se leen bajo demanda, salvo los del nodo raíz
    /// </summary>
    public class NodoDeSeguridadDto
    {
        // identificador único del nodo dentro del árbol (un mismo permiso puede colgar de varias ramas)
        public string Clave { get; set; }
        public string Tipo { get; set; }
        // objeto que se muestra en el panel de datos al seleccionar el nodo
        public string Objeto { get; set; } = enumObjetoDeSeguridad.Ninguno.ToString();
        // id del puesto, rol o permiso, según el tipo del nodo
        public int Id { get; set; }
        public int IdUsuario { get; set; }
        public int IdNegocio { get; set; }
        // en los nodos de negocio, grupo de permisos al que pertenece (PermisosPorTipo, PermisosPorEstado...)
        public string Clase { get; set; }
        public string Nombre { get; set; }
        // fichero de /images/menu que se pinta delante del nombre, vacío si no lleva
        public string Icono { get; set; }
        public string Ayuda { get; set; }
        // número de elementos que agrupa el nodo (puestos, roles o permisos), -1 si no se conoce
        public int Cantidad { get; set; } = -1;
        public bool TieneHijos { get; set; }
        public bool Heredado { get; set; }
        public List<NodoDeSeguridadDto> Hijos { get; set; }
    }
}
