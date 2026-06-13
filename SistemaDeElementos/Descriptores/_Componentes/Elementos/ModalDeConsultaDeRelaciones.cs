using Utilidades;
using ModeloDeDto;

namespace MVCSistemaDeElementos.Descriptores
{

    public class ModalDeConsultaDeRelaciones<TElemento, TSeleccionado> : ControlFiltroHtml
    where TElemento : ElementoDto
    where TSeleccionado : ElementoDto
    {
        public DescriptorDeCrud<TSeleccionado> CrudModal { get; set; }

        public string PropiedadRestrictora { get; private set; }

        public ModalDeConsultaDeRelaciones(DescriptorDeMantenimiento<TElemento> mantenimiento, string tituloModal, DescriptorDeCrud<TSeleccionado> crudModal, string propiedadRestrictora)
        : base(padre: mantenimiento
              ,id: $"{mantenimiento.Id}-{enumTipoControl.ModalDeConsulta.Render()}-{typeof(TSeleccionado).Name}"
              ,etiqueta: tituloModal
              ,propiedad: ""
              ,ayuda: ""
              ,posicion: null)
        {
            CrudModal = crudModal;
            PropiedadRestrictora = propiedadRestrictora.ToLower();
        }

        private string RenderModalDeConsultaDeRelaciones()
        {
            //enumCssModal.EstiloModalConCabecera.Render()
            //{enumCssModal.EstiloContenidoCuerpo.Render()
            string _htmlMiModal = $@"<div id=¨{IdHtml}¨ class=¨contenedor-modal¨ crud-modal=¨{CrudModal.Mnt.IdHtml}¨ propiedad-restrictora=¨{PropiedadRestrictora}¨>
                              		<div id=¨{IdHtml}_contenido¨ class=¨{enumCssModal.ContenidoModalConCabecera.Render()}¨>
                              		    <div id=¨{IdHtml}_cabecera¨ class=¨contenido-cabecera¨>
                              		    	titulo
                                        </div>
                              		    <div id=¨{IdHtml}_cuerpo¨ class=¨{enumCssModal.ContenidoCuerpoConGrid.Render()}¨>
                              			    crudDeConsulta
                                        </div>
                                        <div id=¨{IdHtml}_pie¨ class=¨{enumCssModal.ContenidoPieSoloBotones.Render()}¨>
                                           <input type=¨text¨ id=¨{IdHtml}-cerrar¨  class=¨{enumCssModal.BotonPrincipal.Render()}¨ value=¨Cerrar¨ readonly onclick=¨Crud.{enumGestorDeEventos.EventosModalDeConsultaDeRelaciones}('{eventosDeConsulta.Cerrar}','{IdHtml}')¨ />
                                        </div>
                                      </div>
                              </div>";

            return _htmlMiModal
                .Replace("titulo", Etiqueta)
                .Replace("crudDeConsulta", CrudModal.RenderCrudModal(idModal: this.IdHtml, enumTipoDeModal.ModalDeConsulta));
        }

        public override string RenderControl()
        {
            return RenderModalDeConsultaDeRelaciones();
        }

    }
}
