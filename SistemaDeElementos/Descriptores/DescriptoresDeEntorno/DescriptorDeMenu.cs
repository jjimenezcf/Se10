using ModeloDeDto;
using ModeloDeDto.Entorno;
using MVCSistemaDeElementos.Controllers;
using ServicioDeDatos;
using Utilidades;
using UtilidadesParaIu;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeMenu : DescriptorDeCrud<MenuDto>
    {
        public DescriptorDeMenu(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto: contexto
              , controlador: nameof(MenusController)
              , vista: nameof(MenusController.CrudMenu)
              , modo: modo
              , rutaBase: enumNameSpaceTs.Entorno)
        {

            RecolocarControl(Mnt.Filtro.FiltroDeNombre, new Posicion(0, 0), "Menú", "buscar por nombre del menú");
            var listaMenu = new ListasDinamicas<MenuDto>(Mnt.BloqueGeneral,
                 etiqueta: "Menu padre",
                 filtrarPor: nameof(MenuDto.idPadre),
                 ayuda: "seleccionar padre",
                 seleccionarDe: nameof(MenuDto),
                 buscarPor: nameof(MenuDto.Nombre),
                 mostrarExpresion: $"[{nameof(MenuDto.Padre)}].[{nameof(MenuDto.Nombre)}]",
                 criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                 posicion: new Posicion() { fila = 0, columna = 1 },
                 controlador: nameof(MenusController),
                 navegarA: nameof(MenusController.CrudMenu),
                 restringirPor: "",
                 alSeleccionarBlanquearControl: "");
            listaMenu.AplicarJoin = true;
            listaMenu.LongitudMinimaParaBuscar = 1;

            new CheckFiltro<MenuDto>(Mnt.BloqueGeneral,
                etiqueta: "Mostrar las activas",
                filtrarPor: nameof(MenuDto.Activo),
                ayuda: "Sólo las activos",
                valorInicial: false,
                filtrarPorFalse: false,
                posicion: new Posicion(0, 2));
        }
        public override string RenderControl()
        {
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
if (ServicioDeCaches.Obtener(CacheDe.RenderCrud).ContainsKey(indice))
				 return (string)ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice];
var render = base.RenderControl();

            render = render +
                      $@"<script src=¨../../js/{RutaBase}/Menu.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{                           
                            {RutaBase}.CrearCrudDeMenus('{Mnt.IdHtml}','{Creador.IdHtml}','{Editor.IdHtml}', '{Borrado.IdHtml}') 
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
                    
*/
