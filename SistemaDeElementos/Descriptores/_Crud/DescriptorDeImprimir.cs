using System.Collections.Generic;
using Utilidades;
using ModeloDeDto;
using MVCSistemaDeElementos.Descriptores;
using ServicioDeDatos.Seguridad;
using UtilidadesParaIu;
using ModeloDeDto.SistemaDocumental;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeImprimir<TElemento> : ControlHtml where TElemento : ElementoDto
    {
        public DescriptorDeCrud<TElemento> Crud => (DescriptorDeCrud<TElemento>)Padre;

        public ListaDeValores<TElemento> Plantillas { get; }

        public DescriptorDeImprimir(DescriptorDeCrud<TElemento> crud)
        : base(
          padre: crud,
          id: $"{crud.Id}_{enumTipoControl.pnlImprimir.Render()}",
          etiqueta: "Imprimir",
          propiedad: null,
          ayuda: null,
          posicion: null
        )
        {
            Plantillas = new ListaDeValores<TElemento>(
                  padre: this
                , etiqueta: "Seleccionar la plantilla"
                , ayuda: "Seleccione la plantilla con la que imprimir"
                , opciones: new Dictionary<string, string>()
                , filtraPor: nameof(IPlantillaPlt.IdPlantilla));

        }

        public string RenderDeImprimir()
        {
            return RenderControl();
        }

        public override string RenderControl()
        {
            var htmlModal = RenderizarModal(enumTipoDeModal.ModalDeImprimir,
                idHtml: IdHtml
                , controlador: Crud.Controlador
                , tituloH2: Etiqueta
                , cuerpo: cuerpoDeImprimir()
                , idOpcion: $"{IdHtml}-imprimir"
                , opcion: Crud.NegocioActivo ? "Imprimir" : ""
                , accion: Crud.NegocioActivo ? $"Crud.{enumGestorDeEventos.EventosModalDeImprimir}('{eventosDeImpresion.Imprimir}','{IdHtml}')" : ""
                , cerrar: $"Crud.{enumGestorDeEventos.EventosModalDeImprimir}('{eventosDeImpresion.Cerrar}','{IdHtml}')"
                , navegador: ""
                , claseBoton: enumCssOpcionMenu.DeElemento
                , permisosNecesarios: enumModoDeAccesoDeDatos.Consultor);

            return htmlModal;
        }

        private string cuerpoDeImprimir()
        {
            var htmlCuerpo = $@"<div id=¨{IdHtml}_cuerpo_imprimir¨ class=¨{enumCssDeImprimir.Cuerpo.Render()}¨>
                                        {Plantillas.RenderControl()}
                                </div>
                                ";
            return htmlCuerpo;
        }

    }
}
