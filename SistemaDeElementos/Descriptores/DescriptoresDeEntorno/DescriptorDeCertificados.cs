using ModeloDeDto;
using ModeloDeDto.Entorno;
using MVCSistemaDeElementos.Controllers;
using ServicioDeDatos;
using Utilidades;
using UtilidadesParaIu;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeCertificados : DescriptorDeCrud<CertificadoDto>
    {
        public DescriptorDeCertificados(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto: contexto
               , nameof(CertificadosController),nameof(CertificadosController.CrudCertificados),modo, enumNameSpaceTs.Entorno)
        {

        }


    public override string RenderControl()
    {
        var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
if (ServicioDeCaches.Obtener(CacheDe.RenderCrud).ContainsKey(indice))
				 return (string)ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice];
var render = base.RenderControl();

        render = render +
               $@"<script src=¨../../js/{RutaBase}/Certificados.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{      
                           Entorno.CrearCrudDeCertificados('{Mnt.IdHtml}','{Creador.IdHtml}','{Editor.IdHtml}', '{Borrado.IdHtml}') 
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
