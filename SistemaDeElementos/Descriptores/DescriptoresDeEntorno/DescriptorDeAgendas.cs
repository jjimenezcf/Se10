using System.Collections.Generic;
using ModeloDeDto.Entorno;
using MVCSistemaDeElementos.Controllers;
using ServicioDeDatos;
using ServicioDeDatos.Seguridad;
using Utilidades;
using UtilidadesParaIu;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeAgendas : DescriptorDeCrud<AgendaDto>
    {
        public DescriptorDeAgendas(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto: contexto
               , nameof(AgendasController), nameof(AgendasController.CrudAgendas), modo, enumNameSpaceTs.Entorno)
        {
            //new EditorFiltro<AgendaDto>(Mnt.BloqueGeneral
            //    , etiqueta: "Agenda"
            //    , propiedad: nameof(AgendaDto.Nombre)
            //    , ayuda: "buscar por valor"
            //    , new Posicion { fila = 1, columna = 1 });

            Mnt.Filtro.FiltroDeNombre.CambiarEtiqueta("Agenda", "Buscar por nombre(contenido)");

            DefinirMf(menuIndividual, Mnt.OpcionesPorElemento);

        }


        private void DefinirMf(string idMenu, List<string> opciones)
        {
            DescriptorDeEdicion<AgendaDto>.IncluirMfIndividual(opciones, "<hr>");
            DescriptorDeEdicion<AgendaDto>.IncluirMfIndividual(opciones
                , $"<li id='{idMenu}.{eventosDeMf.Transiciones}' accion-menu='{eventosDeMf.AbrirAgenda}' {AtributosHtml.Mf(enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Consultor, false)}>Abrir agenda</li>");
        }


        public override string RenderControl()
        {
            var render = base.RenderControl();

            render = render +
                   $@"<script src=¨../../js/{RutaBase}/Agendas.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{      
                           Entorno.CrearCrudDeAgendas('{Mnt.IdHtml}','{Creador.IdHtml}','{Editor.IdHtml}', '{Borrado.IdHtml}') 
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
