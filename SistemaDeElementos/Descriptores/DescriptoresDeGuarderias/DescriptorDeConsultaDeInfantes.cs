using ModeloDeDto.Guarderias;
using MVCSistemaDeElementos.Controllers;
using ServicioDeDatos;
using ServicioDeDatos.Guarderias;
using Utilidades;
using UtilidadesParaIu;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeConsultaDeInfante : DescriptorDePaginaDeConsulta
    {
        public DescriptorDeConsultaDeInfante(ContextoSe contexto) :
        base(contexto, nameof(InfantesController), nameof(EntidadController<ContextoSe, InfanteDtm,InfanteDto>.Consultar), rutaBase: enumNameSpaceTs.Guarderias, typeof(InfanteDto), "Consulta de niños")
        {
        }

        public override string RenderControl()
        {
            var render = base.RenderControl();

            render = render +
                   $@"<script src=¨../../js/{RutaBase}/Infantes.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{      
                           {RutaBase}.CrearConsultaDeInfante('{Pagina.IdHtml}') 
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
