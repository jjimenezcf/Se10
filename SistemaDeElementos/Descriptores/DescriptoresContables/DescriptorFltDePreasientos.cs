using ModeloDeDto.Contabilidad;
using ModeloDeDto.SistemaDocumental;
using MVCSistemaDeElementos.Controllers;
using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.SistemaDocumental;
using Utilidades;

namespace MVCSistemaDeElementos.Descriptores
{

    public class DescriptorFltDePreasientos : ControlFiltroHtml 
    {
        FiltroConEditor<PreasientoDto> edtEjercicio { get; }
        FiltroConEditor<PreasientoDto> edtNegocioReferenciado{ get; }
        FiltroConEditor<PreasientoDto> edtReferencia { get; }
        ListasDinamicas<PreasientoDto> lstCircuito { get; }
        FiltroEntreImportes<PreasientoDto> importes { get; }
        public DescriptorFltDePreasientos(ModalDeFiltrado<PreasientoDto> modal)
        : base(modal, $"{modal.Id}-spr", "Preasiento", "preasiento", "Búsqueda por datos del preasiento", new Posicion(1, 0))
        {
            edtEjercicio = new FiltroConEditor<PreasientoDto>(modal, "Datos", nameof(PreasientoDtm.Ejercicio), "Ejercicio", posicion: null, renderEtiqueta: false)
            {
                Criterio = ServicioDeDatos.enumCriteriosDeFiltrado.igual
            };
            //edtNegocioReferenciado = new FiltroConEditor<TElemento>(padre, "", nameof(PreasientoDtm.NegocioReferenciado), "Negocio", posicion: null, renderEtiqueta: false);
            edtReferencia = new FiltroConEditor<PreasientoDto>(modal, "", nameof(ltrDeUnPreasiento.FiltroPorReferenciado), "Referenciado", posicion: null, renderEtiqueta: false);

            lstCircuito = new ListasDinamicas<PreasientoDto>(modal,
                      etiqueta: ltrDeUnPreasiento.EtiquetaLoteContable,
                      filtrarPor: ltrDeUnPreasiento.FiltroLoteContable,
                      ayuda: $"seleccione el {ltrDeUnPreasiento.EtiquetaLoteContable.ToLower()}",
                      seleccionarDe: nameof(CircuitoDocDto),
                      buscarPor: nameof(ltrDeUnCircuito.SeleccionarParaFiltrarPorLoteContable),
                      mostrarExpresion: nameof(CircuitoDocDto.Expresion),
                      criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                      posicion: new Posicion(1, 0),
                      controlador: nameof(CircuitosDocController),
                      navegarA: nameof(CircuitosDocController.CrudLotesContables),
                      restringirPor: "",
                      alSeleccionarBlanquearControl: "");

            importes = new FiltroEntreImportes<PreasientoDto>(modal,
                       etiqueta: "Importes",
                       propiedad: ltrDeUnPreasiento.FiltroEntreImporte,
                       ayuda: "filtrar por rango de importes");
        }

        public override string RenderControl()
        {
            return RenderFiltroDePreasientos();
        }

        private string RenderFiltroDePreasientos()
        {
            var edtEjercicio = this.edtEjercicio.RenderControl();

            //var edtNegocio = edtNegocioReferenciado.RenderControl();
            var edtReferencia = this.edtReferencia.RenderControl();

            var lstCircuito = this.lstCircuito.RenderControl();


            var html = PlantillasHtml.filtroDePreasientos.Replace("[IdHtmlContenedor]", IdHtml + ".contenedor")
                .Replace("[Ayuda]", Ayuda)
                .Replace("[etiqueta]", RenderEtiqueta())
                .Replace("[CssContenedorLista]", $"{enumCssFiltro.ContenedorEnModalDeFiltros.Render()}")
                .Replace("[CssContenedorEditores]", $"{enumCssFiltro.ContenedorPreasientos.Render()} {enumCssFiltro.ContenedorEnModalDeFiltros.Render()}")
                .Replace("[lstCircuito]", lstCircuito)
                .Replace("[edtEjercicio]", edtEjercicio)
                .Replace("[edtReferencia]", edtReferencia)
                .Replace("[filtroEntreImportes]",importes.RenderControl());

            return html;
        }
    }
}
