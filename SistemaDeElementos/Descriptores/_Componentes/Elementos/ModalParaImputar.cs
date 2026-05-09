using Utilidades;
using ModeloDeDto;

namespace MVCSistemaDeElementos.Descriptores
{

    public class ModalParaImputar<TElemento, TSeleccionado> : ControlFiltroHtml
    where TElemento : ElementoDto
    where TSeleccionado : ElementoDto
    {
        public DescriptorDeCrud<TSeleccionado> CrudModal { get; set; }

        public string PropiedadRestrictora { get; private set; }
        public string FiltrarPor { get; private set; }
        public string FaltaRestrictor { get; private set; }

        public ModalParaImputar(DescriptorDeMantenimiento<TElemento> mantenimiento, string tituloModal, DescriptorDeCrud<TSeleccionado> crudModal, string propiedadRestrictora, string filtrarPor, string faltaRestrictor)
        : this(mantenimiento: mantenimiento
              , id: $"{mantenimiento.Id}-{enumTipoControl.ModalParaImputar.Render()}-{typeof(TSeleccionado).Name}"
              , tituloModal: tituloModal
              , crudModal: crudModal
              , propiedadRestrictora: propiedadRestrictora
              , filtrarPor: filtrarPor
              , faltaRestrictor: faltaRestrictor)
        {
        }

        public ModalParaImputar(DescriptorDeMantenimiento<TElemento> mantenimiento,string id, string tituloModal, DescriptorDeCrud<TSeleccionado> crudModal, string propiedadRestrictora, string filtrarPor, string faltaRestrictor)
        : base(padre: mantenimiento
              , id: id
              , etiqueta: tituloModal
              , propiedad: ""
              , ayuda: ""
              , posicion: null)
        {
            CrudModal = crudModal;
            PropiedadRestrictora = propiedadRestrictora.ToLower();
            FiltrarPor = filtrarPor.IsNullOrEmpty() ? propiedadRestrictora : filtrarPor;
            FaltaRestrictor = faltaRestrictor;
        }

        private string RenderModalDeImputarElementos()
        {
            string _htmlMiModal = $@"<div id=¨{IdHtml}¨ class=¨contenedor-modal¨ crud-modal=¨{CrudModal.Mnt.IdHtml}¨ id-negocio=¨{CrudModal.RenderIdDeNegocio}¨ propiedad-restrictora=¨{PropiedadRestrictora}¨ filtrar-por=¨{FiltrarPor}¨ criterio-por=¨{Criterio}¨ falta-restrictor=¨{FaltaRestrictor}¨>
                              		    <div id=¨{IdHtml}_contenido¨ class=¨{enumCssModal.ContenidoModalConCabecera.Render()}¨>
                              		       <div id=¨{IdHtml}_cabecera¨ class=¨{enumCssModal.ContenidoCabecera.Render()} {enumCssModal.CabeceraParaImputar.Render()}¨>
                              		       	titulo
                                           </div>
                              		       <div id=¨{IdHtml}_cuerpo¨ class=¨{enumCssModal.ContenidoCuerpoConGrid.Render()}¨>
                              		 	       crudDeImputacion
                                           </div>
                                           <div id=¨{IdHtml}_pie¨ class=¨contenido-pie¨>
                                              <input type=¨text¨ id=¨{IdHtml}-imputar¨ class=¨boton-modal¨ value=¨Imputar¨ readonly onclick=¨Crud.{enumGestorDeEventos.EventosModalParaImputar}('{eventosDeImputar.Imputar}','{IdHtml}')¨/>
                                              <input type=¨text¨ id=¨{IdHtml}-cerrar¨  class=¨boton-modal¨ value=¨Cerrar¨ readonly onclick=¨Crud.{enumGestorDeEventos.EventosModalParaImputar}('{eventosDeImputar.Cerrar}','{IdHtml}')¨ />
                                           </div>
                                        </div>
                                     </div>";

            return _htmlMiModal
                .Replace("titulo", Etiqueta)
                .Replace("crudDeImputacion", CrudModal.RenderCrudModal(idModal: this.IdHtml, enumTipoDeModal.ModalParaImputar));
        }

        public override string RenderControl()
        {
            return RenderModalDeImputarElementos();
        }

    }
}
