using GestorDeElementos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos;
using Utilidades;
using AutoMapper;
using GestoresDeNegocio.Seguridad;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Entorno;
using GestorDeElementos.Extensores;
using ServicioDeReportes.Base;
using ServicioDeDatos.SistemaDocumental;
using Gestor.Errores;
using ServicioDeDatos.Negocio;
using ModeloDeDto.Negocio;
using GestoresDeNegocio.SistemaDocumental;

namespace GestoresDeNegocio.Negocio
{

    public class GestorDePlantillasDeNegocio : GestorDeElementos<ContextoSe, PlantillaDeNegocioDtm, PlantillaDeNegocioDto>
    {
        public class MapearPlantillasDeNegocio : Profile
        {
            public MapearPlantillasDeNegocio()
            {
                CreateMap<PlantillaDeNegocioDtm, PlantillaDeNegocioDto>()
                .ForMember(dto => dto.Permiso, dtm => dtm.MapFrom(x => x.Permiso == null ? null : x.Permiso.Nombre))
                .ForMember(dto => dto.Accion, dtm => dtm.MapFrom(x => x.Accion == null ? null : x.Accion.Nombre))
                .ForMember(dto => dto.Plantilla, dtm => dtm.MapFrom(x => x.fichero));

                CreateMap<PlantillaDeNegocioDto, PlantillaDeNegocioDtm>()
                .ForMember(dtm => dtm.Permiso, dto => dto.Ignore())
                .ForMember(dtm => dtm.Accion, dto => dto.Ignore())
                .ForMember(dtm => dtm.Archivo, dto => dto.Ignore())
                .ForMember(dtm => dtm.Negocio, dto => dto.Ignore());
            }
        }

        public GestorDePlantillasDeNegocio(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }
        internal static GestorDePlantillasDeNegocio Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDePlantillasDeNegocio(contexto, mapeador);
        }

        protected override void AntesDePersistir(PlantillaDeNegocioDtm plantilla, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(plantilla, parametros);
            if (parametros.Insertando)
            {
                Contexto.CrearPAParaDatosPlantilla(ApiDeRegistroDtm.EsquemaDeTabla(typeof(PlantillaDeNegocioDtm)), plantilla.NombrePa);

                var accion = new AccionDtm
                {
                    Nombre = $"Plantilla: {plantilla.Nombre}",
                    Descripcion = $"Plantilla de impresión asociada al negocio {NegociosDeSe.LeerNegocioPorId(plantilla.IdNegocio).Nombre}",
                    ClaseDeAccion = enumClaseDeAccion.PA.ToString(),
                    Esquema = ApiDeRegistroDtm.EsquemaDeTabla(typeof(PlantillaDeNegocioDtm)),
                    Pa = plantilla.NombrePa,
                }.Insertar(Contexto);
                plantilla.IdAccion = accion.Id;

                if (plantilla.IdArchivo is null)
                {
                    var rutaPlantilla = ApiDePlantillas.CrearLaPlantilla(plantilla.fichero);
                    plantilla.IdArchivo = ServidorDocumental.SubirArchivo(Contexto, rutaPlantilla, sanitizar: false);
                }
                else
                {
                    var archivo = Contexto.SeleccionarPorId<ArchivoDtm>(plantilla.IdArchivo.Entero());
                    if (archivo.Nombre != plantilla.fichero)
                        archivo.Renombrar(Contexto, plantilla.fichero);
                }
                plantilla.IdPermiso = GestorDePermisos.CrearObtener(Contexto, Negocio, plantilla.Nombre, enumClaseDePermiso.Plantilla, enumModoDeAccesoDeDatos.Gestor).Id;
            }
            else if (parametros.Modificando)
            {
                var anterior = (PlantillaDeNegocioDtm)parametros.registroEnBd;
                if (plantilla.IdArchivo is null)
                    plantilla.IdArchivo = anterior.IdArchivo;
                plantilla.IdPermiso = GestorDePermisos.ModificarPermisoDeDatos(Contexto, Negocio, anterior.IdPermiso, plantilla.Nombre, enumClaseDePermiso.Plantilla, enumModoDeAccesoDeDatos.Gestor).Id;
            }
        }

        protected override void DespuesDePersistir(PlantillaDeNegocioDtm plantilla, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(plantilla, parametros);

            if (parametros.Eliminando)
            {
                GestorDePermisos.EliminarRegistroPorId(Contexto, plantilla.IdPermiso, parametros: parametros.Parametros);
                Contexto.EliminarPa(ApiDeRegistroDtm.EsquemaDeTabla(typeof(PlantillaDeNegocioDtm)), plantilla.NombrePa);
                Contexto.EliminarPorId<AccionDtm>(plantilla.IdAccion);
                Contexto.EliminarPorId<ArchivoDtm>(plantilla.IdArchivo.Entero());
                plantilla.EliminarLaPlantilla();
            }
            else
            if (parametros.Modificando)
            {
                var anterior = (PlantillaDeNegocioDtm)parametros.registroEnBd;
                if (plantilla.SeHaModificadoElCampo<int?>(x => x.Name == nameof(PlantillaDeNegocioDtm.IdArchivo), parametros) ||
                    plantilla.SeHaModificadoElCampo<string>(x => x.Name == nameof(PlantillaDeNegocioDtm.Nombre), parametros))
                {
                    var archivo = Contexto.SeleccionarPorId<ArchivoDtm>(plantilla.IdArchivo.Entero());

                    Contexto.RenombrarPa(ApiDeRegistroDtm.EsquemaDeTabla(typeof(PlantillaDeNegocioDtm)), plantilla.NombrePa, anterior.NombrePa);

                    var accion = Contexto.SeleccionarPorId<AccionDtm>(plantilla.IdAccion);
                    accion.Nombre = $"Plantilla: {plantilla.Nombre}";
                    accion.Pa = plantilla.NombrePa;
                    accion.Modificar(Contexto, accionEjecutada: enumAccionesSistemaDocumental.RenombrarPlantilla);

                    if (archivo.Nombre != plantilla.fichero)
                        archivo.Renombrar(Contexto, plantilla.fichero);

                    anterior.EliminarLaPlantilla();
                }
            }
        }

        protected override void DespuesDeMapearElElemento(PlantillaDeNegocioDtm plantilla, PlantillaDeNegocioDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(plantilla, elemento, parametros);
            var accion = Contexto.SeleccionarPorId<AccionDtm>(plantilla.IdAccion);
            elemento.Accion = accion.Nombre;

            var negocio = NegociosDeSe.LeerNegocioPorId(plantilla.IdNegocio);

            if (accion.ClaseDeAccion == enumClaseDeAccion.PA.ToString())
            {
                if (!Contexto.ExistePa(accion.Esquema, accion.Pa)) Contexto.CrearPAParaDatosPlantilla(accion.Esquema, accion.Pa);
            }
            else
                GestorDeErrores.Emitir($"No se ha implementado como tratar la acción '{accion.Nombre}' para obtener datos de la plantilla '{plantilla.Nombre}'");

            elemento.Permiso = Contexto.SeleccionarPorId<PermisoDtm>(plantilla.IdPermiso).Nombre;
            //elemento.Plantilla = plantilla.fichero;
            elemento.ModoDeAcceso = negocio.Activo && Contexto.DatosDeConexion.EsAdministrador ? enumModoDeAccesoDeDatos.Administrador : enumModoDeAccesoDeDatos.Consultor;
        }

    }
}
