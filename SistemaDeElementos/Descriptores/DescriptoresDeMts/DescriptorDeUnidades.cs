using ServicioDeDatos;
using MVCSistemaDeElementos.Controllers;
using UtilidadesParaIu;
using Utilidades;
using ServicioDeDatos.Seguridad;
using ModeloDeDto.MaestrosTecnico;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeUnidades : DescriptorDeCrud<UnidadDto>
    {
        public DescriptorDeUnidades(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto
               , nameof(UnidadesController)
               , nameof(UnidadesController.CrudUnidades)
               , modo
               , rutaBase: enumNameSpaceTs.MaestrosTecnico)
        {
            
        }


        public override string RenderControl()
        {
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
if (ServicioDeCaches.Obtener(CacheDe.RenderCrud).ContainsKey(indice))
				 return (string)ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice];
var render = base.RenderControl();

            render = render +
                   $@"<script src=¨../../js/{RutaBase}/Unidades.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{      
                           {RutaBase}.CrearCrudDeUnidades('{Mnt.IdHtml}', '{Creador.IdHtml}', '{Editor.IdHtml}', '{Borrado.IdHtml}') 
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
