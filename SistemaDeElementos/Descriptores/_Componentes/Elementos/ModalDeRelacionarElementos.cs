using Utilidades;
using ModeloDeDto;

namespace MVCSistemaDeElementos.Descriptores
{

    public class ModalDeRelacionarElementos<TElemento, TSeleccionado> : ControlFiltroHtml
    where TElemento : ElementoDto
    where TSeleccionado : ElementoDto
    {
        public DescriptorDeCrud<TSeleccionado> CrudModal { get; set; }

        public string PropiedadRestrictora { get; private set; }
        public string FiltrarPor { get; private set; }

        public ModalDeRelacionarElementos(DescriptorDeMantenimiento<TElemento> mantenimiento, string tituloModal, DescriptorDeCrud<TSeleccionado> crudModal, string propiedadRestrictora, string filtrarPor = null)
        : base(padre: mantenimiento
              , id: $"{mantenimiento.Id}-{enumTipoControl.ModalDeRelacion.Render()}-{typeof(TSeleccionado).Name}"
              , etiqueta: tituloModal
              , propiedad: ""
              , ayuda: ""
              , posicion: null)
        {
            CrudModal = crudModal;
            PropiedadRestrictora = propiedadRestrictora.ToLower();
            FiltrarPor = filtrarPor.IsNullOrEmpty() ? propiedadRestrictora: filtrarPor;
        }

        private string RenderModalDeRelacionarElementos()
        {
            //enumCssModal.EstiloModalConCabecera.Render()
            //{enumCssModal.EstiloContenidoCuerpo.Render()
            string _htmlMiModal = $@"<div id=¨{IdHtml}¨ class=¨contenedor-modal¨ crud-modal=¨{CrudModal.Mnt.IdHtml}¨ propiedad-restrictora=¨{PropiedadRestrictora}¨ filtrar-por=¨{FiltrarPor}¨>
                              		    <div id=¨{IdHtml}_contenido¨ class=¨{enumCssModal.ContenidoModalConCabecera.Render()}¨>
                              		       <div id=¨{IdHtml}_cabecera¨ class=¨{enumCssModal.ContenidoCabecera.Render()} {enumCssModal.CabeceraRelacionarElementos.Render()}¨>
                              		       	titulo
                                           </div>
                              		       <div id=¨{IdHtml}_cuerpo¨ class=¨{enumCssModal.ContenidoCuerpoConGrid.Render()}¨>
                              		 	       crudDeRelacion
                                           </div>
                                           <div id=¨{IdHtml}_pie¨ class=¨contenido-pie¨>
                                              <input type=¨text¨ id=¨{IdHtml}-relacionar¨ class=¨boton-modal¨ value=¨Relacionar¨ readonly onclick=¨Crud.{enumGestorDeEventos.EventosModalDeCrearRelaciones}('{eventosDeRelacionar.Relacionar}','{IdHtml}')¨/>
                                              <input type=¨text¨ id=¨{IdHtml}-cerrar¨  class=¨boton-modal¨ value=¨Cerrar¨ readonly onclick=¨Crud.{enumGestorDeEventos.EventosModalDeCrearRelaciones}('{eventosDeRelacionar.Cerrar}','{IdHtml}')¨ />
                                           </div>
                                        </div>
                                     </div>";

            return _htmlMiModal
                .Replace("titulo", Etiqueta)
                .Replace("crudDeRelacion", CrudModal.RenderCrudModal(idModal: this.IdHtml, enumTipoDeModal.ModalDeRelacion));
        }

        public override string RenderControl()
        {
            return RenderModalDeRelacionarElementos();
        }

    }
}
