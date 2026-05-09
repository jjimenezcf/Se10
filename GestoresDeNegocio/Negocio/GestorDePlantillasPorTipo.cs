using GestorDeElementos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos;
using Utilidades;
using ModeloDeDto;
using AutoMapper;
using GestoresDeNegocio.Seguridad;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Entorno;
using GestorDeElementos.Extensores;
using ServicioDeReportes.Base;
using GestoresDeNegocio.SistemaDocumental;
using ServicioDeDatos.SistemaDocumental;
using Gestor.Errores;

namespace GestoresDeNegocio.Negocio
{
    public class GestorDePlantillasPorTipo<TContexto, TPltPorTipoDtm, TPltPorTipoDto> : GestorDeElementos<TContexto, TPltPorTipoDtm, TPltPorTipoDto>
        where TContexto : ContextoSe
        where TPltPorTipoDtm : PlantillaPorTipoDtm
        where TPltPorTipoDto : PlantillaPorTipoDto
    {

        public class MapearPlantillasPorTipo : Profile
        {
            public MapearPlantillasPorTipo()
            {

            }

            protected IMappingExpression<E, D> ReglasDeMapeoDelDtoAlDtm<E, D>(IMappingExpression<E, D> reglasDeMapeo)
            where D : TPltPorTipoDtm
            where E : TPltPorTipoDto
            {
                reglasDeMapeo = reglasDeMapeo.ForMember(dtm => dtm.Permiso, dto => dto.Ignore());
                reglasDeMapeo = reglasDeMapeo.ForMember(dtm => dtm.Accion, dto => dto.Ignore());
                reglasDeMapeo = reglasDeMapeo.ForMember(dtm => dtm.Archivo, dto => dto.Ignore());
                return reglasDeMapeo;
            }

            protected IMappingExpression<D, E> ReglasDeMapeoDelDtmAlDto<D, E>(IMappingExpression<D, E> rn)
            where D : TPltPorTipoDtm
            where E : TPltPorTipoDto
            {
                rn = rn
                .ForMember(dto => dto.Permiso, dtm => dtm.MapFrom(x => x.Permiso == null ? null : x.Permiso.Nombre))
                .ForMember(dto => dto.Tipo, dtm => dtm.MapFrom(x => x.Tipo == null ? null : x.Tipo.Expresion))
                .ForMember(dto => dto.Accion, dtm => dtm.MapFrom(x => x.Accion == null ? null : x.Accion.Nombre))
                .ForMember(dto => dto.Plantilla, dtm => dtm.MapFrom(x => x.fichero));
                return rn;
            }
        }


        enumNegocio _negocioDePlantillas;
        public override enumNegocio Negocio => _negocioDePlantillas;



        public GestorDePlantillasPorTipo(TContexto contexto, IMapper mapeador, enumNegocio negocio)
        : base(contexto, mapeador)
        {
            _negocioDePlantillas = negocio;
        }

        public static GestorDePlantillasPorTipo<TContexto, TPltPorTipoDtm, TPltPorTipoDto> Gestor(TContexto contexto, IMapper mapeador, enumNegocio negocio)
        {

            var cache = ServicioDeCaches.Obtener(nameof(GestorDeElementos));
            var indice = $"{negocio}-{typeof(GestorDePlantillasPorTipo<TContexto, TPltPorTipoDtm, TPltPorTipoDto>).Name}";
            if (!cache.ContainsKey(indice))
                cache[indice] = new GestorDePlantillasPorTipo<TContexto, TPltPorTipoDtm, TPltPorTipoDto>(contexto, mapeador, negocio);
            return (GestorDePlantillasPorTipo<TContexto, TPltPorTipoDtm, TPltPorTipoDto>)cache[indice];

        }

        protected override void AntesDePersistir(TPltPorTipoDtm plantilla, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(plantilla, parametros);

            if (parametros.Insertando)
            {
                Contexto.CrearPAParaDatosPlantilla(ApiDeRegistroDtm.EsquemaDeTabla(Negocio.ObtenerMetadatos().TipoDtm), plantilla.NombrePa);
                var tipo = (ITipoDtm)Negocio.CrearGestorDeTipo(Contexto).LeerRegistroPorId(plantilla.IdTipo, aplicarJoin: false);
                var accion = new AccionDtm
                {
                    Nombre = $"Plantilla: {plantilla.Nombre}",
                    Descripcion = $"Plantilla de impresión asociada al tipo {tipo.Nombre}",
                    ClaseDeAccion = enumClaseDeAccion.PA.ToString(),
                    Esquema = ApiDeRegistroDtm.EsquemaDeTabla(Negocio.ObtenerMetadatos().TipoDtm),
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
            else
            if (parametros.Modificando)
            {
                var anterior = (PlantillaPorTipoDtm)parametros.registroEnBd;
                if (plantilla.IdArchivo is null)
                    plantilla.IdArchivo = anterior.IdArchivo;
                plantilla.IdPermiso = GestorDePermisos.ModificarPermisoDeDatos(Contexto, Negocio, anterior.IdPermiso, plantilla.Nombre, enumClaseDePermiso.Plantilla, enumModoDeAccesoDeDatos.Gestor).Id;
            }
        }

        protected override void DespuesDePersistir(TPltPorTipoDtm plantilla, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(plantilla, parametros);

            if (parametros.Eliminando)
            {
                GestorDePermisos.EliminarRegistroPorId(Contexto, plantilla.IdPermiso, parametros: parametros.Parametros);
                Contexto.EliminarPa(ApiDeRegistroDtm.EsquemaDeTabla(Negocio.ObtenerMetadatos().TipoDtm), plantilla.NombrePa);
                Contexto.EliminarPorId<AccionDtm>(plantilla.IdAccion);
                Contexto.EliminarPorId<ArchivoDtm>(plantilla.IdArchivo.Entero());
                plantilla.EliminarLaPlantilla();
            }
            else
            if (parametros.Modificando)
            {
                var anterior = (PlantillaPorTipoDtm)parametros.registroEnBd;
                if (plantilla.SeHaModificadoElCampo<int?>(x => x.Name == nameof(PlantillaPorTipoDtm.IdArchivo), parametros) ||
                    plantilla.SeHaModificadoElCampo<string>(x => x.Name == nameof(PlantillaPorTipoDtm.Nombre), parametros))
                {
                    var archivo = Contexto.SeleccionarPorId<ArchivoDtm>(plantilla.IdArchivo.Entero());

                    Contexto.RenombrarPa(ApiDeRegistroDtm.EsquemaDeTabla(Negocio.ObtenerMetadatos().TipoDtm), plantilla.NombrePa, anterior.NombrePa);

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

        protected override void DespuesDeMapearElElemento(TPltPorTipoDtm plantilla, TPltPorTipoDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(plantilla, elemento, parametros);
            var tipo = (ITipoDtm)Negocio.CrearGestorDeTipo(Contexto).LeerRegistroPorId(plantilla.IdTipo, aplicarJoin: false);
            var accion = Contexto.SeleccionarPorId<AccionDtm>(plantilla.IdAccion);
            elemento.Accion = accion.Nombre;

            if (accion.ClaseDeAccion == enumClaseDeAccion.PA.ToString())
            {
                if (!Contexto.ExistePa(accion.Esquema, accion.Pa)) Contexto.CrearPAParaDatosPlantilla(accion.Esquema, accion.Pa);
            }
            else
                GestorDeErrores.Emitir($"No se ha implementado como tratar la acción '{accion.Nombre}' para obtener datos de la plantilla '{plantilla.Nombre}'");

            elemento.Tipo = tipo.Nombre;
            elemento.Permiso = Contexto.SeleccionarPorId<PermisoDtm>(plantilla.IdPermiso).Nombre;
            //elemento.Plantilla = plantilla.fichero;
            elemento.ModoDeAcceso = tipo.Activo && Contexto.DatosDeConexion.EsAdministrador ? enumModoDeAccesoDeDatos.Administrador : enumModoDeAccesoDeDatos.Consultor;
            elemento.IdNegocio = Negocio.IdNegocio();
        }
    }
}
