using ModeloDeDto;
using ServicioDeDatos;
using System.Collections.Generic;
using Utilidades;

namespace MVCSistemaDeElementos.Descriptores
{

    public class FiltroDeRelacion<TElemento> : ControlFiltroHtml where TElemento : ElementoDto
    {

        ListasDinamicas<TElemento> ListaDinamica { get; }
        ListaDeValores<TElemento> ListaDeValores { get; }

        public FiltroDeRelacion(IControlHtml padre, ListasDinamicas<TElemento> lista, Dictionary<string, string> opciones, string propiedad, string filtrarPor, string ayuda)
        : base(padre, $"{padre.Id}-{propiedad}", "", propiedad, ayuda, new Posicion(1,0))
        {
            Tipo = enumTipoControl.FiltroDeRelacion;
            Criterio = enumCriteriosDeFiltrado.deRelacion;
            ListaDinamica = lista;

            ListaDeValores = new ListaDeValores<TElemento>(padre
                  , "Mostrar"
                  , ""
                  , opciones
                  , filtrarPor
                  , new Posicion() { fila = 0, columna = 2 });
        }

        public override string RenderControl()
        {
            return RenderFiltroDeRelaciones();
        }

        private string RenderFiltroDeRelaciones()
        {
            //var renderEtiqueta = $"<div class='{enumCssFiltro.EtiquetaEntreFechas.Render()}'>{Etiqueta}</div>";
            var renderlistaDinamica = ListaDinamica.RenderControl();
            var renderListaDeValores = ListaDeValores.RenderControl();

            /*
             *$@"
        <div id='[IdHtmlContenedor]' name='contenedor-control' class='input-group [CssContenedor]' title='[Ayuda]'>
            [etiqueta]
            [listaValores]
            [listaDinamica]
         </div>
        ";
             * */

            return PlantillasHtml.filtroDeRelaciones.Replace("[etiqueta]","")
                .Replace("[IdHtmlContenedor]", IdHtml + ".contenedor")
                .Replace("[Ayuda]", Ayuda)
                .Replace("[CssContenedor]", $"{enumCssFiltro.ContenedorDeRelacionModal.Render()} {enumCssFiltro.ContenedorEnModalDeFiltros.Render()}")
                .Replace("[listaValores]", renderListaDeValores)
                .Replace("[listaDinamica]", renderlistaDinamica);
        }
    }
}
