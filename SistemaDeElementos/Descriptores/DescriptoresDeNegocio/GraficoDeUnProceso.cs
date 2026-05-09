using ServicioDeDatos;
using Utilidades;

namespace MVCSistemaDeElementos.Descriptores
{
    public class GraficoDeUnProceso : ControlHtml
    {

        ContextoSe Contexto { get; }
        public GraficoDeUnProceso(ContextoSe contexto, string id)
        : base(null, id, "", "", "", null)
        {
            Contexto = contexto;
        }

        public override string RenderControl()
        {
            return PanelDeControl.RenderPagina(Contexto, RenderCuerpo());
        }

        private static string RenderCuerpo()
        {
            return
            $@"<div id='my-sigma-container'></div>              
              <script src='../../js/{enumNameSpaceTs.Negocio}/GraficoDeUnProceso.js?v={System.DateTime.Now.Ticks}'></script>
               <script>
                  try {{                           
                     PintarGrafico(); 
                  }}
                  catch(error) {{                           
                     MensajesSe.Error('Creando el crud', error.message);
                  }}
               </script>
             ";
        }
    }
}
