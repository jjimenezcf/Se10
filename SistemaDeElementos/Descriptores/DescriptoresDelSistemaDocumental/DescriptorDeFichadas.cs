using ServicioDeDatos;
using MVCSistemaDeElementos.Controllers;
using UtilidadesParaIu;
using Utilidades;
using ServicioDeDatos.Seguridad;
using ModeloDeDto.SistemaDocumental;
namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeFichadas : DescriptorDeCrud<CircuitoDocDto>
    {
        public DescriptorDeFichadas(ContextoSe contexto, string renderCache) : base(contexto,renderCache)
        {
        }

        public DescriptorDeFichadas(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto
               , nameof(CircuitosDocController)
               , nameof(CircuitosDocController.CrudFichadas)
               , modo
               , rutaBase: enumNameSpaceTs.SistemaDocumental
               , eliminarCreacion: true)
        {
            Mnt.OrdenacionInicial = $"{nameof(CircuitoDocDto.Referencia)}:{nameof(CircuitoDocDto.Referencia)}:{enumModoOrdenacion.descendente.Render()}";

            Mnt.Etiqueta = "Gestión de fichadas";
            Editor.Etiqueta = "Consultar fichada";
            Editor.PermiteConsultasConGuid = false;
        }

        public override string RenderControl()
        {
            if (!_renderCache.IsNullOrEmpty())
                return _renderCache;

            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
            var render = base.RenderControl();

            render = render +
                   $@"<script src=¨../../js/{RutaBase}/Fichadas.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{      
                           {RutaBase}.CrearCrudDeFichadas('{Mnt.IdHtml}', '{Creador.IdHtml}', '{Editor.IdHtml}', '{Borrado.IdHtml}') 
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
