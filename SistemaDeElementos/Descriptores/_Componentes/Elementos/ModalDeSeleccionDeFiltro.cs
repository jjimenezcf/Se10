using Utilidades;
using ModeloDeDto;

namespace MVCSistemaDeElementos.Descriptores
{

    public class ModalDeSeleccionDeFiltro<TElemento, TSeleccionado> : ControlFiltroHtml where TElemento : ElementoDto where TSeleccionado : ElementoDto
    {
        public SelectorDeFiltro<TElemento, TSeleccionado> Selector { get; set; }
        public DescriptorDeCrud<TSeleccionado> CrudModal { get; set; }

        public string Titulo => Ayuda;

        public ModalDeSeleccionDeFiltro(SelectorDeFiltro<TElemento, TSeleccionado> selector, DescriptorDeCrud<TSeleccionado> crudModal)
        : base(
          padre: selector.Bloque,
          id: $"Modal_{selector.IdHtml}",    
          etiqueta: $"Seleccionar {selector.propiedadParaMostrar}",
          propiedad: selector.propiedadParaMostrar,
          ayuda: selector.Ayuda,
          posicion: null
        )
        {
            Tipo = enumTipoControl.GridModal;
            Selector = selector;
            Selector.Modal = this;
            CrudModal = crudModal;
        }
        
        //Lo llama el método RenderModalesBloque()
        private string RenderModalDeSeleccionDeFiltro()
        {
            //enumCssModal.EstiloModalConCabecera.Render()
            //{enumCssModal.EstiloContenidoCuerpo.Render()
            string _htmlMiModal = $@"
                                    <!--  ******************  Modal de selección de filtro para {CrudModal.RenderNegocio} ********************************* -->
                                    <div id=¨{IdHtml}¨ class=¨contenedor-modal¨ selector=¨idSelector¨ crud-modal=¨{CrudModal.Mnt.IdHtml}¨>
                              		<div id=¨{IdHtml}_contenido¨ class=¨{enumCssModal.ContenidoModalConCabecera.Render()}¨>
                              		    <div id=¨{IdHtml}_cabecera¨ class=¨contenido-cabecera¨>
                              		    	titulo
                                        </div>
                              		    <div id=¨{IdHtml}_cuerpo¨ class=¨{enumCssModal.ContenidoCuerpoConGrid.Render()}¨>
                              			    crudDeSeleccion
                                        </div>
                                        <div id=¨{IdHtml}_pie¨ class=¨{enumCssModal.ContenidoPieSoloBotones.Render()}¨>
                                           <input type=¨text¨ id=¨{IdHtml}_Cerrar¨  class=¨{enumCssModal.BotonSecundario.Render()}¨ value=¨Cerrar¨ clase=¨{Css.Render(enumCssOpcionMenu.Basico)}¨ readonly onclick=¨Crud.EventosModalDeSeleccion('cerrar-modal-seleccion','{IdHtml}')¨ />
                                           <input type=¨text¨ id=¨{IdHtml}_Aceptar¨ class=¨{enumCssModal.BotonPrincipal.Render()}¨ value=¨Seleccionar¨ clase=¨{Css.Render(enumCssOpcionMenu.Basico)}¨ readonly onclick=¨Crud.EventosModalDeSeleccion('seleccionar-elementos','{IdHtml}')¨ />
                                        </div>
                                      </div>
                              </div>";

            return _htmlMiModal
                .Replace("titulo", Titulo)
                .Replace("crudDeSeleccion", CrudModal.RenderCrudModal(idModal: this.IdHtml, enumTipoDeModal.ModalDeSeleccion))
                .Replace("idSelector", Selector.IdHtml);

        }

        public override string RenderControl()
        {
            return RenderModalDeSeleccionDeFiltro();
        }
    }
}
