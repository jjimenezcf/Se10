using System.Collections.Generic;
using ModeloDeDto;
using MVCSistemaDeElementos.Descriptores;
using Utilidades;

namespace MVCSistemaDeElementos.Descriptores
{
    public class ListaDinamicaConCheck<TElemento> : ControlFiltroHtml where TElemento : ElementoDto
    {
        ListasDinamicas<TElemento> ListaDinamica { get; }

        CheckFiltro<TElemento> Check { get; }

        BloqueDeFitro<TElemento> Bloque => (Padre is BloqueDeFitro<TElemento>) ? (Padre as BloqueDeFitro<TElemento>) : null;

        public ListaDinamicaConCheck(ListasDinamicas<TElemento> lista, CheckFiltro<TElemento> check)
        : base(padre: lista.Padre
              , id: $"{lista.Id}_{check.Id}"
              , ""
              , ""
              , ""
              , new Posicion(1, 1)
              )
        {
            Tipo = enumTipoControl.ListaDinamicaParaMostrarColumna;

            Check = check;
                       
            ListaDinamica = lista;
           
            if (Bloque != null)
            {
                Bloque.AnadirControlEn(this);
            }
        }

        public override string RenderControl()
        {
            return RenderListaDinamicaParaMostrar();
        }

        public string RenderListaDinamicaParaMostrar()
        {
            var a = AtributosHtml.AtributosComunes($"div_{IdHtml}", IdHtml, PropiedadHtml, Tipo, Ayuda);
            var valores = a.MapearComunes();

            valores["CssContenedor"] = enumCssFiltro.ContenedorListaDinamicaConChek.Render();
            valores["Ayuda"] = "filtros a aplicar y check para indicar si se muestra la información";

            var control = PlantillasHtml.Render(PlantillasHtml.listaDinamicaConCheck, valores);
            return control.Replace("[ListaDinamica]", ListaDinamica.RenderControl())
                .Replace("[CheckDeMostrar]", Check.RenderControl());
        }

    }

}

