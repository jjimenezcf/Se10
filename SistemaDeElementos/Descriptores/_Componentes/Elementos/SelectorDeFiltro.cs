using Utilidades;
using ModeloDeDto;
using ServicioDeDatos;
using UtilidadesParaIu;

namespace MVCSistemaDeElementos.Descriptores
{

    public class SelectorDeFiltro<TElemento,TSeleccionado> : ControlFiltroHtml where TElemento : ElementoDto where TSeleccionado : ElementoDto
    {
        public BloqueDeFitro<TElemento> Bloque => (Padre is BloqueDeFitro<TElemento>) ? (Padre as BloqueDeFitro<TElemento>) : null;
        public string propiedadParaFiltrar { get; private set; }
        public string propiedadParaMostrar { get; private set; }
        public ModalDeSeleccionDeFiltro<TElemento, TSeleccionado> Modal { get; set; }

        public string idBtnSelectorHtml => $"{IdHtml}_btnsel";

        public string PropiedadDondeMapear { get; private set; }

        public DescriptorDeCrud<TSeleccionado> CrudModal { get; private set; }

        public SelectorDeFiltro(IControlHtml padre, string etiqueta, string filtrarPor, string ayuda, Posicion posicion, string paraFiltrar, string paraMostrar, DescriptorDeCrud<TSeleccionado> crudModal, string propiedadDondeMapear)
        : base(
          padre: padre
          , id: $"{padre.Id}_{enumTipoControl.SelectorDeFiltro.Render()}_{filtrarPor}" 
          , etiqueta
          , filtrarPor
          , ayuda
          , posicion
          )
        {
            Tipo = enumTipoControl.SelectorDeFiltro;
            propiedadParaFiltrar = paraFiltrar.ToLower();
            propiedadParaMostrar = paraMostrar.ToLower();
            Modal = new ModalDeSeleccionDeFiltro<TElemento, TSeleccionado>(this, crudModal);
            Criterio = enumCriteriosDeFiltrado.igual;
            CrudModal = crudModal;
            PropiedadDondeMapear = propiedadDondeMapear;
            if (Bloque != null) Bloque.AnadirSelector(this);
        }

        public string RenderSelector()
        {
            ControlHtml edt = CrudModal.Mnt.Filtro.BuscarControlPorPropiedad(PropiedadDondeMapear);

            return $@"<div class=¨{ExtensorDeCss.Render(enumCssFiltro.ContenedorSelector)}¨>
                       <input id=¨{IdHtml}¨ 
                              type = ¨text¨ 
                              class=¨form-control¨ 
                              placeholder=¨{Ayuda}¨
                              {base.RenderAtributos()} 
                              criterioBuscar=¨{enumCriteriosDeFiltrado.contiene}¨
                              propiedadBuscar=¨{ltrFiltros.Nombre}¨
                              propiedad-mostrar=¨{propiedadParaMostrar}¨
                              propiedadFiltrar=¨{propiedadParaFiltrar}¨
                              id-modal=¨{Modal.IdHtml}¨
                              id-grid-modal=¨{CrudModal.Mnt.Datos.IdHtml}¨
                              idBtnSelector=¨{idBtnSelectorHtml}¨
                              idEditorMostrar=¨{edt.IdHtml}¨
                              refCheckDeSeleccion=¨{ltrColumnasDelGrid.chksel}.{CrudModal.Mnt.Datos.IdHtml}¨
                              onchange =¨Crud.EventosDelMantenimiento('cambiar-selector','{IdHtml}')¨>
                       <input type=¨text¨ 
                              id=¨{idBtnSelectorHtml}¨ 
                              class=¨boton-de-seleccion¨ 
                              value=¨...¨ 
                              onclick=¨Crud.EventosModalDeSeleccion('abrir-modal-seleccion', '{Modal.IdHtml}')¨  
                              readonly />
                    </div>
                  ";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string RenderControl()
        {
            return RenderSelector();
        }
    }
}
