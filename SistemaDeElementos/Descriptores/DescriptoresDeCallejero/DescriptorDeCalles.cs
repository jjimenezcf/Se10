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
    public class DescriptorDeCalles : DescriptorDeCrud<CalleDto>
    {
        public DescriptorDeCalles(ContextoSe contexto, string renderCache) : base(contexto,renderCache)
        {
        }

        public DescriptorDeCalles(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto
               , nameof(CallesController)
               , nameof(CallesController.CrudCalles)
               , modo
               , rutaBase: enumNameSpaceTs.Callejero)
        {

            var listaPais = new ListasDinamicas<CalleDto>(Mnt.BloqueGeneral,
                 etiqueta: "Pais",
                 filtrarPor: nameof(CalleDto.IdPais),
                 ayuda: "seleccione el país",
                 seleccionarDe: nameof(PaisDto),
                 buscarPor: nameof(PaisDto.Nombre),
                 mostrarExpresion: $"([{nameof(PaisDto.Codigo)}]) [{nameof(PaisDto.Nombre)}]",
                 criterioDeBusqueda: enumCriteriosDeFiltrado.comienza,
                 posicion: new Posicion(0, 0),
                 controlador: nameof(PaisesController),
                 navegarA: enumVistasCallejero.CrudPaises,
                 restringirPor: "",
                 alSeleccionarBlanquearControl: nameof(CalleDto.IdProvincia));
            listaPais.LongitudMinimaParaBuscar = 1;

            var listaProvincia = new ListasDinamicas<CalleDto>(Mnt.BloqueGeneral,
                etiqueta: "Provincia",
                filtrarPor: nameof(CalleDto.IdProvincia),
                ayuda: "seleccione la provincia",
                seleccionarDe: nameof(ProvinciaDto),
                buscarPor: nameof(ProvinciaDto.Nombre),
                mostrarExpresion: $"([{nameof(ProvinciaDto.Codigo)}]) [{nameof(ProvinciaDto.Nombre)}]",
                criterioDeBusqueda: enumCriteriosDeFiltrado.comienza,
                posicion: new Posicion(0, 1),
                controlador: nameof(ProvinciasController),
                navegarA: enumVistasCallejero.CrudProvincias,
                restringirPor: nameof(MunicipioDto.IdPais),
                alSeleccionarBlanquearControl: nameof(CalleDto.IdMunicipio));
            listaProvincia.LongitudMinimaParaBuscar = 1;

            var listaMunicipio = new ListasDinamicas<CalleDto>(Mnt.BloqueGeneral,
                etiqueta: "Municipio",
                filtrarPor: nameof(CalleDto.IdMunicipio),
                ayuda: "seleccione el municipio",
                seleccionarDe: nameof(MunicipioDto),
                buscarPor: nameof(MunicipioDto.Nombre),
                mostrarExpresion: $"[{nameof(MunicipioDto.Nombre)}]",
                criterioDeBusqueda: enumCriteriosDeFiltrado.comienza,
                posicion: new Posicion(0, 2),
                controlador: nameof(MunicipiosController),
                navegarA: enumVistasCallejero.CrudMunicipios,
                restringirPor: nameof(MunicipioDto.IdProvincia),
                alSeleccionarBlanquearControl: "");
            listaMunicipio.LongitudMinimaParaBuscar = 1;

            new EditorFiltro<CalleDto>(padre: Mnt.BloqueGeneral
                , etiqueta: "Barrio"
                , propiedad: ltrCalles.filtroPorBarrio
                , ayuda: "buscar por nombre de barrio"
                , new Posicion { fila = 1, columna = 1 });

            new EditorFiltro<CalleDto>(padre: Mnt.BloqueGeneral
                , etiqueta: "Zona"
                , propiedad: ltrCalles.filtroPorZona
                , ayuda: "buscar por nombre de zona"
                , new Posicion { fila = 1, columna = 2 });

            RecolocarControl(Mnt.Filtro.FiltroDeNombre, new Posicion(1, 0), "Calle", "Buscar por nombre de calle");

            Mnt.OrdenacionInicial = @$"{nameof(CalleDto.Pais)}:municipio.provincia.pais.nombre:{enumModoOrdenacion.ascendente.Render()};
                                       {nameof(CalleDto.Provincia)}:municipio.provincia.nombre:{enumModoOrdenacion.ascendente.Render()};
                                       {nameof(CalleDto.Municipio)}:municipio.nombre:{enumModoOrdenacion.ascendente.Render()};
                                       {nameof(CalleDto.Expresion)}:nombre:{enumModoOrdenacion.ascendente.Render()}";

            var expansorDeCps = DescriptorDeExpansorCalleCps(Editor);
            Editor.Expanes.Insert(0, expansorDeCps);

            var expansorDeZonas = DescriptorDeExpansorZonas(Editor);
            Editor.Expanes.Insert(1, expansorDeZonas);

            var expansorDeBarrios = DescriptorDeExpansorBarrios(Editor);
            Editor.Expanes.Insert(2, expansorDeBarrios);

            var expansorDeMapa1 = DescriptorDeExpansorGoogleMap(Editor);
            Editor.Expanes.Insert(3, expansorDeMapa1);
            var expansorDeMapa2 = DescriptorDeExpansorStreetView(Editor);
            Editor.Expanes.Insert(4, expansorDeMapa2);

        }

        private DescriptorDeExpansor DescriptorDeExpansorGoogleMap(DescriptorDeEdicion<CalleDto> editor)
        {
            var expansorDeMapa = new DescriptorDeExpansor(editor, $"{editor.Id}-mapas-gmaps", "Google Maps", mostrarPlegado: true, "Visualizar mapa");
            return expansorDeMapa;
        }
        private DescriptorDeExpansor DescriptorDeExpansorStreetView(DescriptorDeEdicion<CalleDto> editor)
        {
            var expansorDeMapa = new DescriptorDeExpansor(editor, $"{editor.Id}-mapas-street", "Street view", mostrarPlegado: true, "Visualizar mapa");
            return expansorDeMapa;
        }

        private DescriptorDeExpansor DescriptorDeExpansorZonas(DescriptorDeEdicion<CalleDto> editor)
        {
            var expansorDeZonas = new DescriptorDeExpansor(editor, $"{editor.Id}-zonas", "Zonas a las que pertenece la calle", mostrarPlegado: true, "zonas de una calle");

            //Definimos el grid de detalles del cuerpo
            var columnasDeZonas = new DescriptorDeColumnas("zonas");
            columnasDeZonas.Add(titulo: "Zona", propiedad: nameof(ZonasDeUnaCalleDto.Zona), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnasDeZonas.Add(titulo: "Id", propiedad: nameof(ZonasDeUnaCalleDto.Id), alineacion: enumAliniacion.derecha, mostrar: false);

            var parametros = new Dictionary<string, object> {
                { nameof(GridDeRelacion.Controlador), typeof(ZonasDeUnaCalleController) }
              , { nameof(GridDeRelacion.AccionDeConsulta), nameof(EntidadController<ContextoSe,ZonasDeUnaCalleDtm, ZonasDeUnaCalleDto>.epLeerElementos)}
              , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(ZonasDeUnaCalleDto.IdCalle) }
            };

            new GridDeRelacion(expansorDeZonas, columnasDeZonas, parametros);

            expansorDeZonas.DescriptorDeCrearRelaciones(editor.Crud.Contexto, typeof(ZonasDeUnaCalleDto), typeof(ZonasDeUnaCalleController), nameof(ZonasDeUnaCalleDto.IdCalle), "Relacionar con una zona");
            return expansorDeZonas;
        }

        private DescriptorDeExpansor DescriptorDeExpansorBarrios(DescriptorDeEdicion<CalleDto> editor)
        {
            var expansorDeBarrios = new DescriptorDeExpansor(editor, $"{editor.Id}-barrios", "Barrios donde está la calle", mostrarPlegado: true, "barrios de una calle");

            //Definimos el grid de detalles del cuerpo
            var columnasDeBarrios = new DescriptorDeColumnas("barrios");
            columnasDeBarrios.Add(titulo: "Barrio", propiedad: nameof(BarriosDeUnaCalleDto.Barrio), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnasDeBarrios.Add(titulo: "Mano", propiedad: nameof(BarriosDeUnaCalleDto.Mano), alineacion: enumAliniacion.izquierda);
            columnasDeBarrios.Add(titulo: "Número desde", propiedad: nameof(BarriosDeUnaCalleDto.Desde), alineacion: enumAliniacion.derecha);
            columnasDeBarrios.Add(titulo: "Número hasta", propiedad: nameof(BarriosDeUnaCalleDto.Hasta), alineacion: enumAliniacion.derecha);
            columnasDeBarrios.Add(titulo: "Id", propiedad: nameof(BarriosDeUnaCalleDto.Id), alineacion: enumAliniacion.derecha, mostrar: false);

            var parametros = new Dictionary<string, object> {
                { nameof(GridDeRelacion.Controlador), typeof(BarriosDeUnaCalleController) }
              , { nameof(GridDeRelacion.AccionDeConsulta), nameof(EntidadController<ContextoSe,BarriosDeUnaCalleDtm, BarriosDeUnaCalleDto>.epLeerElementos)}
              , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(BarriosDeUnaCalleDto.IdCalle) }
            };

            new GridDeRelacion(expansorDeBarrios, columnasDeBarrios, parametros);

            expansorDeBarrios.DescriptorDeCrearRelaciones(editor.Crud.Contexto, typeof(BarriosDeUnaCalleDto), typeof(BarriosDeUnaCalleController), nameof(BarriosDeUnaCalleDto.IdCalle), "Relacionar con un barrio");
            expansorDeBarrios.DescriptorDeEditarRelaciones(editor.Crud.Contexto, typeof(BarriosDeUnaCalleDto), typeof(BarriosDeUnaCalleController), "Editar el barrio de una calle", false);

            return expansorDeBarrios;
        }

        private static DescriptorDeExpansor DescriptorDeExpansorCalleCps(DescriptorDeEdicion<CalleDto> editor)
        {
            var expansorDeCps = new DescriptorDeExpansor(editor, $"{editor.Id}-cps", "Códigos postales de la calle", mostrarPlegado: true, "códigos postales de una calle");

            //Definimos el grid de detalles del cuerpo
            var columnasDeCps = new DescriptorDeColumnas("codigos-postales");
            columnasDeCps.Add(titulo: "Código postal", propiedad: nameof(CpsDeUnaCalleDto.CodigoPostal), alineacion: enumAliniacion.derecha, mostrar: true);
            columnasDeCps.Add(titulo: "Mano", propiedad: nameof(CpsDeUnaCalleDto.Mano), alineacion: enumAliniacion.izquierda);
            columnasDeCps.Add(titulo: "Número desde", propiedad: nameof(CpsDeUnaCalleDto.Desde), alineacion: enumAliniacion.derecha);
            columnasDeCps.Add(titulo: "Número hasta", propiedad: nameof(CpsDeUnaCalleDto.Hasta), alineacion: enumAliniacion.derecha);
            columnasDeCps.Add(titulo: "Id", propiedad: nameof(CpsDeUnaCalleDto.Id), alineacion: enumAliniacion.derecha, mostrar: false);

            var parametros = new Dictionary<string, object> {
                { nameof(GridDeRelacion.Controlador), typeof(CpsDeUnaCalleController)}
              , { nameof(GridDeRelacion.AccionDeConsulta), nameof(EntidadController<ContextoSe,CpsDeUnaCalleDtm, CpsDeUnaCalleDto>.epLeerElementos)}
              , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(CpsDeUnaCalleDto.IdCalle) }
            };

            new GridDeRelacion(expansorDeCps, columnasDeCps, parametros);

            expansorDeCps.DescriptorDeCrearRelaciones(editor.Crud.Contexto, typeof(CpsDeUnaCalleDto), typeof(CpsDeUnaCalleController), nameof(CpsDeUnaCalleDto.IdCalle), "Relacionar con un CP");
            expansorDeCps.DescriptorDeEditarRelaciones(editor.Crud.Contexto, typeof(CpsDeUnaCalleDto), typeof(CpsDeUnaCalleController), "Editar el CP de una calle", false);

            return expansorDeCps;
        }


        public override string RenderControl()
        {
            if (!_renderCache.IsNullOrEmpty())
                return _renderCache;

            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
            var render = base.RenderControl();

            render = render +
                   $@"<script src=¨../../js/{RutaBase}/literales.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script src=¨../../js/{RutaBase}/ApiCallejero.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script src=¨../../js/{RutaBase}/GestorDeMapas.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script src=¨../../js/{RutaBase}/Calles.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script type=¨text/javascript¨ src=¨https://maps.googleapis.com/maps/api/js?key=AIzaSyBuOc8GEU0OqoW6L99Un7CPBXviRfCUFS0¨></script>
                      <script>
                         try {{      
                           {RutaBase}.CrearCrudDeCalles('{Mnt.IdHtml}','{Creador.IdHtml}','{Editor.IdHtml}', '{Borrado.IdHtml}') 
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
/*
 * 
 * 

 
 * 
 */