using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GestorDeElementos;
using ModeloDeDto.Seguridad;
using ServicioDeDatos;
using ServicioDeDatos.Seguridad;

namespace GestoresDeNegocio.Seguridad
{


    public class GestorDeClaseDePermisos : GestorDeElementos<ContextoSe, ClasePermisoDtm, ClasePermisoDto>
    {

        public class MapearClasePermiso : Profile
        {
            public MapearClasePermiso()
            {
                CreateMap<ClasePermisoDtm, ClasePermisoDto>();
                CreateMap<ClasePermisoDto, ClasePermisoDtm>();
            }
        }

        public GestorDeClaseDePermisos(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {

        }


        internal static GestorDeClaseDePermisos Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeClaseDePermisos(contexto, mapeador);
        }

        internal ClasePermisoDtm Crear(enumClaseDePermiso nombreDeClase)
        {
            var registro = new ClasePermisoDtm();
            registro.Nombre = nombreDeClase.ToString();
            PersistirRegistro(registro, new ParametrosDeNegocio(enumTipoOperacion.Insertar));
            return registro;
        }
    }
}
