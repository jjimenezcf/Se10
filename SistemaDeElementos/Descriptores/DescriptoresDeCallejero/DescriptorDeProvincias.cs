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
    public class DescriptorDeProvincias : DescriptorDeCrud<ProvinciaDto>
    {
        public DescriptorDeProvincias(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto
               , nameof(ProvinciasController)
                 , nameof(ProvinciasController.CrudProvincias)
                 , modo
                 , enumNameSpaceTs.Callejero)
        {
            var listaPais = new ListasDinamicas<ProvinciaDto>(Mnt.BloqueGeneral,
                etiqueta: enumNegocio.Pais.Singular(),
                filtrarPor: nameof(ProvinciaDto.IdPais),
                ayuda: $"seleccione el {enumNegocio.Pais.Singular(true)}",
                seleccionarDe: nameof(PaisDto),
                buscarPor: nameof(PaisDto.Nombre),
                mostrarExpresion: $"([{nameof(PaisDto.Codigo)}]) [{nameof(PaisDto.Nombre)}]",
                criterioDeBusqueda: enumCriteriosDeFiltrado.comienza,
                posicion: new Posicion(0, 0),
                controlador: nameof(PaisesController),
                navegarA: enumVistasCallejero.CrudPaises,
                restringirPor: "",
                alSeleccionarBlanquearControl: "");
            listaPais.LongitudMinimaParaBuscar = 1;

            new EditorFiltro<ProvinciaDto>(padre: Mnt.BloqueGeneral
                , etiqueta: "Código"
                , propiedad: nameof(ProvinciaDto.Codigo)
                , ayuda: "buscar por código de provincia"
                , new Posicion { fila = 0, columna = 1 });

            AnadirOpcionDeDependencias(Mnt
                , controlador: nameof(MunicipiosController)
                , vista: nameof(MunicipiosController.CrudMunicipios)
                , datosDependientes: nameof(MunicipioDto)
                , navegarAlCrud: DescriptorDeMantenimiento<MunicipioDto>.NombreMnt
                , nombreOpcion: "Municipios"
                , propiedadQueRestringe: nameof(ProvinciaDto.Id)
                , propiedadRestrictora: nameof(MunicipioDto.IdProvincia)
                , "Municipios de una provincia");

            new EditorFiltro<ProvinciaDto>(padre: Mnt.BloqueGeneral
                , etiqueta: "CP"
                , propiedad: nameof(CpsDeUnMunicipioDto.CodigoPostal)
                , ayuda: "buscar por codigo postal"
                , new Posicion { fila = 1, columna = 1 });


            AnadirOpciondeRelacion(Mnt
                , controlador: nameof(CpsDeUnaProvinciaController)
                , vista: nameof(CpsDeUnaProvinciaController.CrudCpsDeUnaProvincia)
                , relacionarCon: nameof(CodigoPostalDto)
                , navegarAlCrud: DescriptorDeMantenimiento<CpsDeUnaProvinciaDto>.NombreMnt
                , nombreOpcion: "C.P."
                , propiedadQueRestringe: nameof(ProvinciaDto.Id)
                , propiedadRestrictora: nameof(CpsDeUnaProvinciaDto.IdProvincia)
                , "Añadir puestos al usuario seleccionado");

            RecolocarControl(Mnt.Filtro.FiltroDeNombre, new Posicion(1, 0), "Provincia", "Buscar por nombre de provincia");
            Mnt.OrdenacionInicial = @$"{nameof(ProvinciaDto.Pais)}:pais.nombre:{enumModoOrdenacion.ascendente.Render()};
                                       {nameof(ProvinciaDto.Nombre)}:nombre:{enumModoOrdenacion.ascendente.Render()}";

            var expansorDeCps = DescriptorDeExpansorProvinciaCps(Editor);
            Editor.Expanes.Insert(0, expansorDeCps);
        }

        private static DescriptorDeExpansor DescriptorDeExpansorProvinciaCps(DescriptorDeEdicion<ProvinciaDto> editor)
        {
            var expansorDeCps = new DescriptorDeExpansor(editor, $"{editor.Id}-cps", "Códigos postales", mostrarPlegado: true, "códigos postales de una provincia");

            //Definimos el grid de detalles del cuerpo
            var columnasDeCps = new DescriptorDeColumnas("codigos-postales");
            columnasDeCps.Add(titulo: "Código postal", propiedad: nameof(CpsDeUnaProvinciaDto.CodigoPostal), alineacion: enumAliniacion.derecha, mostrar: true);
            columnasDeCps.Add(titulo: "Id", propiedad: nameof(CpsDeUnaProvinciaDto.Id), alineacion: enumAliniacion.derecha, mostrar: false);
            var parametros = new Dictionary<string, object> {
                { nameof(GridDeRelacion.Controlador), typeof(CpsDeUnaProvinciaController)}
              , { nameof(GridDeRelacion.AccionDeConsulta), nameof(EntidadController<ContextoSe,CpsDeUnaProvinciaDtm, CpsDeUnaProvinciaDto>.epLeerElementos)}
              , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(CpsDeUnaProvinciaDtm.IdProvincia) }
            };
            new GridDeRelacion(expansorDeCps, columnasDeCps, parametros);

            expansorDeCps.DescriptorDeCrearRelaciones(editor.Crud.Contexto, typeof(CpsDeUnaProvinciaDto), typeof(CpsDeUnaProvinciaController), nameof(CpsDeUnaProvinciaDto.IdProvincia), "Relacionar con un CP");

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
                      <script src=¨../../js/{RutaBase}/Provincias.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{      
                           {RutaBase}.CrearCrudDeProvincias('{Mnt.IdHtml}','{Creador.IdHtml}','{Editor.IdHtml}', '{Borrado.IdHtml}') 
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

