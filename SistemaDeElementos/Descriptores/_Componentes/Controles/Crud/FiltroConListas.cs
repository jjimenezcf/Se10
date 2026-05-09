using ModeloDeDto;
using ServicioDeDatos;
using System.Collections.Generic;
using Utilidades;

namespace MVCSistemaDeElementos.Descriptores
{

    public class FiltroConListas<TElemento> : ControlFiltroHtml where TElemento : ElementoDto
    {

        List<ListasDinamicas<TElemento>> Listas { get; }

        public FiltroConListas(IControlHtml padre, List<ListasDinamicas<TElemento>> listas)
        : base(padre, "", "", "", "", new Posicion(1,0))
        {
            Tipo = enumTipoControl.FiltroConListasDinamicas;
            Criterio = enumCriteriosDeFiltrado.deRelacion;
            Listas = listas;

        }

        public override string RenderControl()
        {
            return RenderFiltroConListases();
        }

        private string RenderFiltroConListases()
        {
            var renderlistas = "";
            foreach (var lista in Listas)
            {
                renderlistas = renderlistas + lista.RenderControl();
            }

            return PlantillasHtml.filtroConListas
                .Replace("[IdHtmlContenedor]", IdHtml + ".contenedor")
                .Replace("[Ayuda]", Ayuda)
                .Replace("[CssContenedor]", $"{enumCssFiltro.ContenedorConDosListasModal.Render()} {enumCssFiltro.ContenedorEnModalDeFiltros.Render()}")
                .Replace("[listas]", renderlistas);
        }
    }
}
