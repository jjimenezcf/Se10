using ModeloDeDto;
using ModeloDeDto.Entorno;
using MVCSistemaDeElementos.Controllers;
using ServicioDeDatos;
using Utilidades;
using UtilidadesParaIu;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeVistaMvc : DescriptorDeCrud<VistaMvcDto>
    {
        public DescriptorDeVistaMvc(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto: contexto
               , nameof(VistaMvcController), nameof(VistaMvcController.CrudVistaMvc), modo, enumNameSpaceTs.Entorno)
        {
            var fltGeneral = Mnt.Filtro.ObtenerBloquePorEtiqueta(ltrBloques.General);
            
            new EditorFiltro<VistaMvcDto>(Mnt.BloqueGeneral
                , etiqueta: "Controlador"
                , propiedad: nameof(VistaMvcDto.Controlador)
                , ayuda: "buscar por controlador"
                , new Posicion { fila = 0, columna = 1 });

            new CheckFiltro<VistaMvcDto>(Mnt.BloqueGeneral,
                etiqueta: "Mostrar solo las modales",
                filtrarPor: nameof(VistaMvcDto.MostrarEnModal),
                ayuda: "Sólo las las modales",
                valorInicial: false,
                filtrarPorFalse: false,
                posicion: new Posicion(0, 2));
        }


        public override string RenderControl()
        {
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
            var cache = ServicioDeCaches.Obtener(CacheDe.RenderCrud);
            if (cache.ContainsKey(indice))
				 return (string)ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice];
            var render = base.RenderControl();

            render = render +
                   $@"<script src=¨../../js/{RutaBase}/VistaMvc.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{                           
                            Entorno.CrearCrudVistaMvc('{Mnt.IdHtml}','{Creador.IdHtml}','{Editor.IdHtml}', '{Borrado.IdHtml}');
                         }}
                         catch(error) {{                           
                            MensajesSe.Error('Creando el crud', error.message);
                         }}
                      </script>
                    ";
            cache[indice] = render.Render();
			return (string)cache[indice];
        }
    }
}
