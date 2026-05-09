using ModeloDeDto;
using ModeloDeDto.Callejero;
using MVCSistemaDeElementos.Controllers;
using ServicioDeDatos;
using ServicioDeDatos.Callejero;
using ServicioDeDatos.Seguridad;
using Utilidades;
using UtilidadesParaIu;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeCallesDeUnaZona : DescriptorDeCrud<CallesDeUnaZonaDto>
    {
        
        public DescriptorDeCallesDeUnaZona(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto: contexto
               , nameof(CallesDeUnaZonaController), nameof(CallesDeUnaZonaController.CrudCallesDeUnaZona), modo, enumNameSpaceTs.Callejero)
        {
            var fltGeneral = Mnt.Filtro.ObtenerBloquePorEtiqueta(ltrBloques.General);
            new RestrictorDeFiltro<CallesDeUnaZonaDto>(padre: fltGeneral
                  , etiqueta: "Zona"
                  , propiedad:nameof(CallesDeUnaZonaDto.IdZona)
                  , ayuda: "buscar por zona"
                  , new Posicion { fila = 0, columna = 0 })
            {
                Controlador = nameof(ZonasController),
                VistaDondeNavegar = nameof(ZonasController.CrudZonas),
                Negocio = enumNegocio.Zona
            };

            BuscarControlEnFiltro(ltrFiltros.Nombre).CambiarAtributos("Calle", nameof(CallesDeUnaZonaDto.Calle), "Buscar por 'calle'");

            var modalDeCalles = new ModalDeRelacionarElementos<CallesDeUnaZonaDto, CalleDto>(mantenimiento: Mnt
                              , tituloModal: "Seleccione las calles a relacionar"
                              , crudModal: new DescriptorDeCalles(contexto,ModoDescriptor.Relacion)
                              , propiedadRestrictora: nameof(CallesDeUnaZonaDto.IdZona));
            var relacionarCalles = new RelacionarElementos(modalDeCalles.IdHtml, () => modalDeCalles.RenderControl(), "Añadir calles a la zona");
            var opcion = new OpcionDeMenu<CallesDeUnaZonaDto>(Mnt.ZonaMenu.Menu, relacionarCalles, $"Calles", enumModoDeAccesoDeDatos.Gestor);
            Mnt.ZonaMenu.Menu.Add(opcion);

            Mnt.OrdenacionInicial = @$"{nameof(CallesDeUnaZonaDto.Calle)}:{nameof(ZonasDeUnaCalleDtm.Calle)}.{nameof(ZonasDeUnaCalleDtm.Calle.Nombre)}:{enumModoOrdenacion.ascendente.Render()}";
        }


        public override string RenderControl()
        {
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
if (ServicioDeCaches.Obtener(CacheDe.RenderCrud).ContainsKey(indice))
				 return (string)ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice];
var render = base.RenderControl();

            render = render +
                   $@"<script src=¨../../js/{RutaBase}/CallesDeUnaZona.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{                           
                            {RutaBase}.CrearCrudDeCallesDeUnaZona('{Mnt.IdHtml}','{Creador.IdHtml}','{Editor.IdHtml}', '{Borrado.IdHtml}') 
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
