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
    public class DescriptorDeCallesDeUnBarrio : DescriptorDeCrud<CallesDeUnBarrioDto>
    {
        
        public DescriptorDeCallesDeUnBarrio(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto: contexto,
              nameof(CallesDeUnBarrioController),
              nameof(CallesDeUnBarrioController.CrudCallesDeUnBarrio),
              modo,
              enumNameSpaceTs.Callejero)
        {
            var fltGeneral = Mnt.Filtro.ObtenerBloquePorEtiqueta(ltrBloques.General);
            new RestrictorDeFiltro<CallesDeUnBarrioDto>(padre: fltGeneral
                  , etiqueta: "Barrio"
                  , propiedad:nameof(CallesDeUnBarrioDto.IdBarrio)
                  , ayuda: "buscar por barrio"
                  , new Posicion { fila = 0, columna = 0 })
            {
                Controlador = nameof(BarriosController),
                VistaDondeNavegar = nameof(BarriosController.CrudBarrios),
                Negocio = enumNegocio.Barrio
            };

            BuscarControlEnFiltro(ltrFiltros.Nombre).CambiarAtributos("Calle", nameof(CallesDeUnBarrioDto.Calle), "Buscar por 'calle'");

            var modalDeCalles = new ModalDeRelacionarElementos<CallesDeUnBarrioDto, CalleDto>(mantenimiento: Mnt
                              , tituloModal: "Seleccione las calles a relacionar"
                              , crudModal: new DescriptorDeCalles(contexto,ModoDescriptor.Relacion)
                              , propiedadRestrictora: nameof(CallesDeUnBarrioDto.IdBarrio));
            var relacionarCalles = new RelacionarElementos(modalDeCalles.IdHtml, () => modalDeCalles.RenderControl(), "Añadir calles al barrio");
            var opcion = new OpcionDeMenu<CallesDeUnBarrioDto>(Mnt.ZonaMenu.Menu, relacionarCalles, $"Calles", enumModoDeAccesoDeDatos.Gestor);
            Mnt.ZonaMenu.Menu.Add(opcion);

            Mnt.OrdenacionInicial = @$"{nameof(CallesDeUnBarrioDto.Calle)}:{nameof(BarriosDeUnaCalleDtm.Calle)}.{nameof(BarriosDeUnaCalleDtm.Calle.Nombre)}:{enumModoOrdenacion.ascendente.Render()}";
        }


        public override string RenderControl()
        {
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
if (ServicioDeCaches.Obtener(CacheDe.RenderCrud).ContainsKey(indice))
				 return (string)ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice];
var render = base.RenderControl();

            render = render +
                   $@"<script src=¨../../js/{RutaBase}/CallesDeUnBarrio.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{                           
                            {RutaBase}.CrearCrudDeCallesDeUnBarrio('{Mnt.IdHtml}','{Creador.IdHtml}','{Editor.IdHtml}', '{Borrado.IdHtml}') 
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
