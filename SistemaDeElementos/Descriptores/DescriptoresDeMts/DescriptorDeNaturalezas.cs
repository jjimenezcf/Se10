using ServicioDeDatos;
using MVCSistemaDeElementos.Controllers;
using UtilidadesParaIu;
using Utilidades;
using ServicioDeDatos.Seguridad;
using ModeloDeDto.MaestrosTecnico;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeNaturalezas : DescriptorDeCrud<NaturalezaDto>
    {
        public DescriptorDeNaturalezas(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto
               , nameof(NaturalezasController)
               , nameof(NaturalezasController.CrudNaturalezas)
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
                   $@"<script src=¨../../js/{RutaBase}/Naturalezas.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{      
                           {RutaBase}.CrearCrudDeNaturalezas('{Mnt.IdHtml}', '{Creador.IdHtml}', '{Editor.IdHtml}', '{Borrado.IdHtml}') 
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
