using ModeloDeDto.SistemaDocumental;
using MVCSistemaDeElementos.Controllers;
using ServicioDeDatos;
using ServicioDeDatos.Seguridad;
using Utilidades;
using UtilidadesParaIu;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeCircuitosDoc : DescriptorDeCrud<CircuitoDocDto>
    {
        public DescriptorDeCircuitosDoc(ContextoSe contexto, string renderCache) : base(contexto,renderCache)
        {
        }

        public DescriptorDeCircuitosDoc(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto
               , nameof(CircuitosDocController)
               , nameof(CircuitosDocController.CrudCircuitosDoc)
               , modo
               , rutaBase: enumNameSpaceTs.SistemaDocumental)
        {
            Mnt.OrdenacionInicial = $"{nameof(CircuitoDocDto.Referencia)}:{nameof(CircuitoDocDto.Referencia)}:{enumModoOrdenacion.descendente.Render()}";

            Editor.PermiteConsultasConGuid = false;
            // DefinirDescriptorDePreasientos();
        }

        public override string RenderControl()
        {
            if (!_renderCache.IsNullOrEmpty())
                return _renderCache;

            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
            var render = base.RenderControl();

            render = render +
                   $@"<script src=¨../../js/{RutaBase}/CircuitosDoc.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{      
                           {RutaBase}.CrearCrudDeCircuitosDoc('{Mnt.IdHtml}', '{Creador.IdHtml}', '{Editor.IdHtml}', '{Borrado.IdHtml}') 
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
