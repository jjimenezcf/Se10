using System.Collections.Generic;
using GestorDeElementos;
using ModeloDeDto.Entorno;
using MVCSistemaDeElementos.Controllers;
using ServicioDeDatos;
using ServicioDeDatos.Seguridad;
using Utilidades;
using UtilidadesParaIu;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeMiCalendario : DescriptorDeCrud<EventoDeAgendaDto>
    {
        public DescriptorDeMiCalendario(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto: contexto, nameof(VisorDeAgendaController), nameof(VisorDeAgendaController.MiCalendario), modo, enumNameSpaceTs.Entorno)
        {
            Mnt.OrdenacionInicial = @$"{nameof(EventoDeAgendaDto.Inicio)}:inicio:{enumModoOrdenacion.ascendente.Render()}";
            RenombrarEtiqueta("Nombre", "Evento");

            var listaAgenda = new ListasDinamicas<EventoDeAgendaDto>(Mnt.BloqueGeneral,
                etiqueta: "Agenda",
                filtrarPor: nameof(EventoDeAgendaDto.IdAgenda),
                ayuda: "seleccione la agenda",
                seleccionarDe: nameof(AgendaDto),
                buscarPor: nameof(AgendaDto.Nombre),
                mostrarExpresion: $"[{nameof(AgendaDto.Nombre)}]",
                criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                posicion: new Posicion(0, 1),
                controlador: nameof(AgendasController),
                navegarA: nameof(AgendasController.CrudAgendas),
                restringirPor: "",
                alSeleccionarBlanquearControl: "");
            listaAgenda.LongitudMinimaParaBuscar = 1;

            new FiltroEntreFechas<EventoDeAgendaDto>(Mnt.BloqueGeneral,
                    etiqueta: "Inicio",
                    propiedad: nameof(EventoDeAgendaDto.Inicio),
                    ayuda: "eventos entre fechas",
                    posicion: new Posicion() { fila = 1, columna = 0 });

            var opciones = new Dictionary<string, string>();
            foreach (var negocio in NegociosDeSe.NegociosConAgenda())
                opciones.Add(negocio.Key.ToString(), negocio.Value.Singular());

            var lv = new ListaDeValores<EventoDeAgendaDto>(this,
                "Negocio",
                "eventos del negocio ...",
                opciones,
                nameof(EventoDeAgendaDto.IdNegocio));

            new ListaDeValoresParaFiltrado<EventoDeAgendaDto>(Mnt.BloqueGeneral,
                lv,
                nameof(EventoDeAgendaDto.Agenda),
                new Posicion(1, 1))
            {
                OtrasCssDelContenedor = enumGridTemplateColumn.GridTemplateColumn_2_100_80.Render()
            };

        }


        private void DefinirMf(string idMenu, List<string> opciones)
        {
            DescriptorDeEdicion<AgendaDto>.IncluirMfIndividual(opciones, "<hr>");
            DescriptorDeEdicion<AgendaDto>.IncluirMfIndividual(opciones
                , $"<li id='{idMenu}.{eventosDeMf.Transiciones}' accion-menu='{eventosDeMf.AbrirAgenda}' {AtributosHtml.Mf(enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Consultor, false)}>Abrir agenda</li>");
        }


        public override string RenderControl()
        {
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
            if (ServicioDeCaches.Obtener(CacheDe.RenderCrud).ContainsKey(indice))
				 return (string)ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice];
            var render = base.RenderControl();

            render = render +
                   $@"<script src=¨../../js/{RutaBase}/MiCalendario.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{      
                           Entorno.CrearCrudDeMiCalendario('{Mnt.IdHtml}','{Creador.IdHtml}','{Editor.IdHtml}', '{Borrado.IdHtml}') 
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
