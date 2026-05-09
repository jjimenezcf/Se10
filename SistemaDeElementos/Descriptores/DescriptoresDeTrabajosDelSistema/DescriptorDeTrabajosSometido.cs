using UtilidadesParaIu;
using MVCSistemaDeElementos.Controllers;
using ModeloDeDto.TrabajosSometidos;
using ServicioDeDatos;
using Utilidades;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeTrabajosSometido : DescriptorDeCrud<TrabajoSometidoDto>
    {
        public DescriptorDeTrabajosSometido(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto: contexto
               , controlador: nameof(TrabajosSometidoController)
               , vista: $"{nameof(TrabajosSometidoController.CrudDeTrabajosSometido)}"
               , modo: modo
               , rutaBase: enumNameSpaceTs.TrabajosSometido)
        {
        }

        public override string RenderControl()
        {
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
if (ServicioDeCaches.Obtener(CacheDe.RenderCrud).ContainsKey(indice))
				 return (string)ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice];
var render = base.RenderControl();

            render = render +
                   $@"<script src=¨../../js/{RutaBase}/TrabajosSometido.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{                           
                            TrabajosSometido.CrearCrudDeTrabajosSometido('{Mnt.IdHtml}','{Creador.IdHtml}','{Editor.IdHtml}', '{Borrado.IdHtml}') 
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
