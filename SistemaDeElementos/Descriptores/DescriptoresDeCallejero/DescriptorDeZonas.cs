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
    public class DescriptorDeZonas : DescriptorDeCrud<ZonaDto>
    {
        public DescriptorDeZonas(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto
               , nameof(ZonasController)
               , nameof(ZonasController.CrudZonas)
               , modo
               , rutaBase: enumNameSpaceTs.Callejero)
        {
            var fltGeneral = Mnt.Filtro.ObtenerBloquePorEtiqueta(ltrBloques.General);
            new RestrictorDeFiltro<ZonaDto>(padre: fltGeneral
                  , etiqueta: "Municipio"
                  , propiedad: nameof(ZonaDto.IdMunicipio)
                  , ayuda: "buscar por municipio"
                  , new Posicion { fila = 0, columna = 0 })
            {
                Controlador = nameof(MunicipiosController),
                VistaDondeNavegar = nameof(MunicipiosController.CrudMunicipios),
                Negocio = enumNegocio.Municipio
            };

            AnadirOpciondeRelacion(Mnt
                , controlador: nameof(CallesDeUnaZonaController)
                , vista: nameof(CallesDeUnaZonaController.CrudCallesDeUnaZona)
                , relacionarCon: nameof(CalleDto)
                , navegarAlCrud: DescriptorDeMantenimiento<CallesDeUnaZonaDto>.NombreMnt
                , nombreOpcion: "Calles"
                , propiedadQueRestringe: nameof(ZonaDto.Id)
                , propiedadRestrictora: nameof(CallesDeUnaZonaDto.IdZona)
                , "Añadir calles a una zona");

            BuscarControlEnFiltro(ltrFiltros.Nombre).CambiarAtributos("Zona", nameof(ZonaDto.Nombre), "Buscar por 'zona'");
            Mnt.OrdenacionInicial = @$"{nameof(ZonaDto.Nombre)}:{nameof(ZonaDto.Nombre)}:{enumModoOrdenacion.ascendente.Render()}";


            var expansorDeCalles = DescriptorDeExpansorCalles(Editor);
            Editor.Expanes.Insert(0, expansorDeCalles);
        }
        private DescriptorDeExpansor DescriptorDeExpansorCalles(DescriptorDeEdicion<ZonaDto> editor)
        {
            var expansorDeZonas = new DescriptorDeExpansor(editor, $"{editor.Id}-calles", "Calles", mostrarPlegado: true, "calles de un zona");

            //Definimos el grid de detalles del cuerpo
            var columnasDeZonas = new DescriptorDeColumnas("calles");
            columnasDeZonas.Add(titulo: "Calle", propiedad: nameof(CallesDeUnaZonaDto.Calle), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnasDeZonas.Add(titulo: "Id", propiedad: nameof(CallesDeUnaZonaDto.Id), alineacion: enumAliniacion.derecha, mostrar: false);

            var parametros = new Dictionary<string, object> {
                { nameof(GridDeRelacion.Controlador), typeof(CallesDeUnaZonaController)}
              , { nameof(GridDeRelacion.AccionDeConsulta), nameof(EntidadController<ContextoSe,ZonasDeUnaCalleDtm, CallesDeUnaZonaDto>.epLeerElementos)}
              , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(CallesDeUnaZonaDto.IdZona) }
            };

            new GridDeRelacion(expansorDeZonas, columnasDeZonas, parametros);

            expansorDeZonas.DescriptorDeCrearRelaciones(editor.Crud.Contexto, typeof(CallesDeUnaZonaDto), typeof(CallesDeUnaZonaController), nameof(CallesDeUnaZonaDto.IdZona), "Relacionar con un calle");

            return expansorDeZonas;
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
                      <script src=¨../../js/{RutaBase}/Zonas.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{      
                           {RutaBase}.CrearCrudDeZonas('{Mnt.IdHtml}','{Creador.IdHtml}','{Editor.IdHtml}', '{Borrado.IdHtml}') 
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
