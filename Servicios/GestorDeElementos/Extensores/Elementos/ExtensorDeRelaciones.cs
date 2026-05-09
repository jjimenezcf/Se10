using ModeloDeDto;
using ServicioDeDatos.Elemento;
using ServicioDeDatos;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Seguridad;

namespace GestorDeElementos.Extensores
{
    public static class ExtensionesDeRelaciones
    {
        public static T CrearRelacion<T>(this ContextoSe contexto, string propiedad1, int id1, int id2, bool errorSiExiste = false)
        where T : RelacionDtm
        {
            var gestor = new GestorDeRelaciones<ContextoSe, T, ElementoDto>(contexto, contexto.Mapeador);
            var relacionado = gestor.CrearRelacion(propiedad1, id1, id2, errorSiExiste);
            return relacionado.relacio;
        }


        public static T CrearRelacionComoAdministrador<T>(this ContextoSe contexto, string propiedad1, int id1, int id2, bool errorSiExiste = false)
        where T : RelacionDtm
        {
            var usuarioDeConexion = contexto.SeleccionarPorId<UsuarioDtm>(contexto.DatosDeConexion.IdUsuario);
            var otorgado = usuarioDeConexion.OtorgarAdministrador(contexto);
            try
            {
                return contexto.CrearRelacion<T>(nameof(PermisosDeUnRolDtm.IdRol), id1, id2, errorSiExiste);
            }
            finally
            {
                usuarioDeConexion.AnularAdministrador(contexto, otorgado);
            }
        }

        public static T CrearRelacion<T>(this T elemento, ContextoSe contexto, bool errorSiExiste = false)
        where T : RelacionDtm
        =>
        contexto.CrearRelacion<T>(elemento.PropiedadDelIdElemento1, elemento.IdElemento1, elemento.IdElemento2, errorSiExiste);
    }
}
