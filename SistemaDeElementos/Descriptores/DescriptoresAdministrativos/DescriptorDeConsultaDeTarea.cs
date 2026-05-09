using ModeloDeDto.Tarea;
using MVCSistemaDeElementos.Controllers;
using ServicioDeDatos;
using ServicioDeDatos.Tarea;
using Utilidades;
using UtilidadesParaIu;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeConsultaDeTarea : DescriptorDePaginaDeConsulta
    {
        public DescriptorDeConsultaDeTarea(ContextoSe contexto) :
        base(contexto, nameof(TareasController), nameof(EntidadController<ContextoSe, TareaDtm,TareaDto>.Consultar), rutaBase: enumNameSpaceTs.Administracion, typeof(TareaDto), "Consulta de tareas")
        {
        }

        public override string RenderControl()
        {
            var render = base.RenderControl();

            render = render +
                   $@"<script src=¨../../js/{RutaBase}/ApiDeAdministracion.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script src=¨../../js/{RutaBase}/Tareas.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{      
                           {RutaBase}.CrearConsultaDeTarea('{Pagina.IdHtml}') 
                         }}
                         catch(error) {{                           
                            MensajesSe.Error('Creando el crud', error.message);
                         }}
                      </script>
                    ";
            return render.Render();
        }
    }

}
