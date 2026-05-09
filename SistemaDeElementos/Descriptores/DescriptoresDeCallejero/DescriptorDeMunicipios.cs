using ModeloDeDto.Callejero;
using ServicioDeDatos;
using MVCSistemaDeElementos.Controllers;
using UtilidadesParaIu;
using ModeloDeDto;
using System.Collections.Generic;
using ServicioDeDatos.Callejero;
using Utilidades;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeMunicipios : DescriptorDeCrud<MunicipioDto>
    {
        public DescriptorDeMunicipios(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto
               , nameof(MunicipiosController)
                 , nameof(MunicipiosController.CrudMunicipios)
                 , modo
                 , enumNameSpaceTs.Callejero)
        {

            var listaPais = new ListasDinamicas<MunicipioDto>(Mnt.BloqueGeneral,
                 etiqueta: "Pais",
                 filtrarPor: nameof(MunicipioDto.IdPais),
                 ayuda: "seleccione el país",
                 seleccionarDe: nameof(PaisDto),
                 buscarPor: nameof(PaisDto.Nombre),
                 mostrarExpresion: $"([{nameof(PaisDto.Codigo)}]) [{nameof(PaisDto.Nombre)}]",
                 criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                 posicion: new Posicion(0, 0),
                 controlador: nameof(PaisesController),
                 navegarA: enumVistasCallejero.CrudPaises,
                 restringirPor: "",
                 alSeleccionarBlanquearControl: nameof(CalleDto.IdProvincia));
            listaPais.LongitudMinimaParaBuscar = 1;

            var listaProvincia = new ListasDinamicas<MunicipioDto>(Mnt.BloqueGeneral,
                etiqueta: "Provincia",
                filtrarPor: nameof(MunicipioDto.IdProvincia),
                ayuda: "seleccione la provincia",
                seleccionarDe: nameof(ProvinciaDto),
                buscarPor: nameof(ProvinciaDto.Nombre),
                mostrarExpresion: $"([{nameof(ProvinciaDto.Codigo)}]) [{nameof(ProvinciaDto.Nombre)}]",
                criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                posicion: new Posicion(0, 1),
                controlador: nameof(ProvinciasController),
                navegarA: enumVistasCallejero.CrudProvincias,
                restringirPor: nameof(MunicipioDto.IdPais),
                alSeleccionarBlanquearControl: "");
            listaProvincia.LongitudMinimaParaBuscar = 1;

            new EditorFiltro<MunicipioDto>(padre: Mnt.BloqueGeneral
                , etiqueta: "CP"
                , propiedad: nameof(CpsDeUnMunicipioDto.CodigoPostal)
                , ayuda: "buscar por codigo postal"
                , new Posicion { fila = 1, columna = 1 });

            AnadirOpcionDeDependencias(Mnt
            , controlador: nameof(BarriosController)
            , vista: nameof(BarriosController.CrudBarrios)
            , datosDependientes: nameof(BarrioDto)
            , navegarAlCrud: DescriptorDeMantenimiento<BarrioDto>.NombreMnt
            , nombreOpcion: "Barrios"
            , propiedadQueRestringe: nameof(MunicipioDto.Id)
            , propiedadRestrictora: nameof(BarrioDto.IdMunicipio)
            , "Barrios de un municipio");

            AnadirOpcionDeDependencias(Mnt
            , controlador: nameof(ZonasController)
            , vista: nameof(ZonasController.CrudZonas)
            , datosDependientes: nameof(ZonaDto)
            , navegarAlCrud: DescriptorDeMantenimiento<ZonaDto>.NombreMnt
            , nombreOpcion: "Zonas"
            , propiedadQueRestringe: nameof(MunicipioDto.Id)
            , propiedadRestrictora: nameof(ZonaDto.IdMunicipio)
            , "Zonas de un municipio");

            AnadirOpcionDeDependencias(Mnt
            , controlador: nameof(CallesController)
            , vista: nameof(CallesController.CrudCalles)
            , datosDependientes: nameof(CalleDto)
            , navegarAlCrud: DescriptorDeMantenimiento<CalleDto>.NombreMnt
            , nombreOpcion: "Calles"
            , propiedadQueRestringe: nameof(MunicipioDto.Id)
            , propiedadRestrictora: nameof(CalleDto.IdMunicipio)
            , "Calles de un municipio");

            AnadirOpciondeRelacion(Mnt
                , controlador: nameof(CpsDeUnMunicipioController)
                , vista: nameof(CpsDeUnMunicipioController.CrudCpsDeUnMunicipio)
                , relacionarCon: nameof(CodigoPostalDto)
                , navegarAlCrud: DescriptorDeMantenimiento<CpsDeUnMunicipioDto>.NombreMnt
                , nombreOpcion: "C.P."
                , propiedadQueRestringe: nameof(MunicipioDto.Id)
                , propiedadRestrictora: nameof(CpsDeUnMunicipioDto.IdMunicipio)
                , "Añadir puestos al usuario seleccionado");

            RecolocarControl(Mnt.Filtro.FiltroDeNombre, new Posicion(1, 0), "Municipio", "Buscar por nombre de municipio");
            Mnt.OrdenacionInicial = @$"{nameof(MunicipioDto.Pais)}:provincia.pais.nombre:{enumModoOrdenacion.ascendente.Render()};
                                       {nameof(MunicipioDto.Provincia)}:provincia.nombre:{enumModoOrdenacion.ascendente.Render()};
                                       {nameof(MunicipioDto.Nombre)}:nombre:{enumModoOrdenacion.ascendente.Render()}";


            var expansorDeZonas = DescriptorDeExpansorZonasDeUnMunicipio(Editor);
            Editor.Expanes.Insert(0, expansorDeZonas);

            var expansorDeBarrios = DescriptorDeExpansorBarriosDeUnMunicipio(Editor);
            Editor.Expanes.Insert(1, expansorDeBarrios);

            var expansorDeCps = DescriptorDeExpansorCpsDeUnMunicipio(Editor);
            Editor.Expanes.Insert(2, expansorDeCps);


        }

        private static DescriptorDeExpansor DescriptorDeExpansorZonasDeUnMunicipio(DescriptorDeEdicion<MunicipioDto> editor)
        {
            var expansorDeZonas = new DescriptorDeExpansor(editor, $"{editor.Id}-zonas", "Zonas del municipio", mostrarPlegado: true, "zonas del municipio");

            //Definimos el grid de detalles del cuerpo
            var columnasDeCps = new DescriptorDeColumnas("Zonas");
            columnasDeCps.Add(titulo: "Zonas", propiedad: nameof(ZonaDto.Nombre), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnasDeCps.Add(titulo: "Id", propiedad: nameof(ZonaDto.Id), alineacion: enumAliniacion.derecha, mostrar: false);
            var parametros = new Dictionary<string, object> {
                { nameof(GridDeRelacion.Controlador), typeof(ZonasController) }
              , { nameof(GridDeRelacion.AccionDeConsulta), nameof(EntidadController<ContextoSe,ZonaDtm, ZonaDto>.epLeerElementos)}
              , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(ZonaDtm.IdMunicipio) }
            };
            var gridDeRelacion = new GridDeRelacion(expansorDeZonas, columnasDeCps, parametros);
            gridDeRelacion.PermitirBorrar = false;

            return expansorDeZonas;
        }

        private static DescriptorDeExpansor DescriptorDeExpansorBarriosDeUnMunicipio(DescriptorDeEdicion<MunicipioDto> editor)
        {
            var expansor = new DescriptorDeExpansor(editor, $"{editor.Id}-barrios", "Barrios del municipio", mostrarPlegado: true, "barrios del municipio");

            //Definimos el grid de detalles del cuerpo
            var columnas = new DescriptorDeColumnas("Barrios");
            columnas.Add(titulo: "Barrios", propiedad: nameof(BarrioDto.Nombre), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnas.Add(titulo: "Id", propiedad: nameof(BarrioDto.Id), alineacion: enumAliniacion.derecha, mostrar: false);
            var parametros = new Dictionary<string, object> {
                { nameof(GridDeRelacion.Controlador), typeof(BarriosController) }
              , { nameof(GridDeRelacion.AccionDeConsulta), nameof(EntidadController<ContextoSe,BarrioDtm, BarrioDto>.epLeerElementos)}
              , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(BarrioDtm.IdMunicipio) }
            };
            var gridDeRelacion = new GridDeRelacion(expansor, columnas, parametros);
            gridDeRelacion.PermitirBorrar = false;

            return expansor;
        }

        private static DescriptorDeExpansor DescriptorDeExpansorCpsDeUnMunicipio(DescriptorDeEdicion<MunicipioDto> editor)
        {
            var expansorDeCps = new DescriptorDeExpansor(editor, $"{editor.Id}-cps", "Códigos postales", mostrarPlegado: true, "códigos postales de un municipio");

            //Definimos el grid de detalles del cuerpo
            var columnasDeCps = new DescriptorDeColumnas("codigos-postales");
            columnasDeCps.Add(titulo: "Código postal", propiedad: nameof(CpsDeUnMunicipioDto.CodigoPostal), alineacion: enumAliniacion.derecha, mostrar: true);
            columnasDeCps.Add(titulo: "Id", propiedad: nameof(CpsDeUnMunicipioDto.Id), alineacion: enumAliniacion.derecha, mostrar: false);
            var parametros = new Dictionary<string, object> {
                { nameof(GridDeRelacion.Controlador), typeof(CpsDeUnMunicipioController)}
              , { nameof(GridDeRelacion.AccionDeConsulta), nameof(EntidadController<ContextoSe,CpsDeUnMunicipioDtm, CpsDeUnMunicipioDto>.epLeerElementos)}
              , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(CpsDeUnMunicipioDtm.IdMunicipio) }
            };
            var gridDeRelacion = new GridDeRelacion(expansorDeCps, columnasDeCps, parametros);

            expansorDeCps.DescriptorDeCrearRelaciones(editor.Crud.Contexto, typeof(CpsDeUnMunicipioDto), typeof(CpsDeUnMunicipioController), nameof(CpsDeUnMunicipioDto.IdMunicipio), "Relacionar con un CP");
            return expansorDeCps;
        }


        public override string RenderControl()
        {
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
if (ServicioDeCaches.Obtener(CacheDe.RenderCrud).ContainsKey(indice))
				 return (string)ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice];
var render = base.RenderControl();

            render = render +
                   $@"<script src=¨../../js/{RutaBase}/literales.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script src=¨../../js/{RutaBase}/ApiCallejero.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script src=¨../../js/{RutaBase}/Municipios.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{      
                           {RutaBase}.CrearCrudDeMunicipios('{Mnt.IdHtml}','{Creador.IdHtml}','{Editor.IdHtml}', '{Borrado.IdHtml}') 
                         }}
                         catch(error) {{                           
                            MensajesSe.Error('Creando el crud', error.message);
                         }}
                      </script>
                    ";
            ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice] = render.Render();
			return (string)ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice];
        }
    }
}
