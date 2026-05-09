using AutoMapper;
using GestorDeElementos;
using ModeloDeDto.Seguridad;
using ServicioDeDatos;
using ServicioDeDatos.Seguridad;

namespace GestoresDeNegocio.Seguridad
{
    public class GestorDeTipoPermiso : GestorDeElementos<ContextoSe, TipoPermisoDtm, TipoPermisoDto>
    {
        public class MapearTipoPermiso : Profile
        {
            public MapearTipoPermiso()
            {
                CreateMap<TipoPermisoDtm, TipoPermisoDto>();
            }
        }

        public GestorDeTipoPermiso(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }


        internal static GestorDeTipoPermiso Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeTipoPermiso(contexto, mapeador);
        }

        internal TipoPermisoDtm CrearTipoPermisoDeDatos(enumModoDeAccesoDeDatos acceso)
        {
            var registro = new TipoPermisoDtm();
            registro.Nombre = ModoDeAcceso.Nombre(acceso);
            PersistirRegistro(registro, new ParametrosDeNegocio(enumTipoOperacion.Insertar));
            return registro;
        }

        internal TipoPermisoDtm CrearTipoPermisoFuncional(enumModoDeAccesoFuncional acceso)
        {
            var registro = new TipoPermisoDtm();
            registro.Nombre = ModoDeAcceso.ToString(acceso);
            PersistirRegistro(registro, new ParametrosDeNegocio(enumTipoOperacion.Insertar));
            return registro;
        }
    }
}
