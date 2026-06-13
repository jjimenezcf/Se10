using Utilidades;
using ModeloDeDto;

namespace MVCSistemaDeElementos.Descriptores
{

    public class ModalParaSeleccionar<TSeleccionado> : ControlFiltroHtml
    where TSeleccionado : ElementoDto
    {
        public DescriptorDeCrud<TSeleccionado> CrudModal { get; set; }

        public string PropiedadRestrictora { get; private set; }

        public ModalParaSeleccionar(ControlHtml controlPadre, string tituloModal, DescriptorDeCrud<TSeleccionado> crudModal, string propiedadRestrictora)
        : base(padre: controlPadre
              ,id: $"{controlPadre.Id}-{enumTipoControl.ModalDeSeleccion.Render()}-{typeof(TSeleccionado).Name}"
              ,etiqueta: tituloModal
              ,propiedad: ""
              ,ayuda: ""
              ,posicion: null)
        {
            CrudModal = crudModal;
            PropiedadRestrictora = propiedadRestrictora.ToLower();
        }

        public string RenderModalParaSeleccionar()
        {
          return RenderControl();
        }

        public override string RenderControl()
        {
            //enumCssModal.EstiloModalConCabecera.Render()
            //{enumCssModal.EstiloContenidoCuerpo.Render()
            string _htmlMiModal = $@"<div id=¨{IdHtml}¨ class=¨contenedor-modal¨ crud-modal=¨{CrudModal.Mnt.IdHtml}¨ propiedad-restrictora=¨{PropiedadRestrictora}¨>
                              		<div id=¨{IdHtml}_contenido¨ class=¨{enumCssModal.ContenidoModalConCabecera.Render()}¨>
                              		    <div id=¨{IdHtml}_cabecera¨ class=¨contenido-cabecera¨>
                              		    	titulo
                                        </div>
                              		    <div id=¨{IdHtml}_cuerpo¨ class=¨{enumCssModal.ContenidoCuerpoConGrid.Render()}¨>
                              			    crudParaSeleccionar
                                        </div>
                                        <div id=¨{IdHtml}_pie¨ class=¨{enumCssModal.ContenidoPieSoloBotones.Render()}¨>
                                           <input type=¨text¨ id=¨{IdHtml}-cerrar¨  class=¨{enumCssModal.BotonPrincipal.Render()}¨ value=¨Cerrar¨ readonly onclick=¨Crud.{enumGestorDeEventos.EventosModalParaSeleccionar}('{eventosParaSeleccionar.Cerrar}','{IdHtml}')¨ />
                                        </div>
                                      </div>
                              </div>";

            return _htmlMiModal
                .Replace("titulo", Etiqueta)
                .Replace("crudParaSeleccionar", CrudModal.RenderCrudModal(idModal: this.IdHtml, enumTipoDeModal.ModalParaSeleccionar));
        }

    }
}
