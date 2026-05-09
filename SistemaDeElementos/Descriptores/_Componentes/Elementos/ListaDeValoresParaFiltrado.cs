using System.Collections.Generic;
using ModeloDeDto;
using MVCSistemaDeElementos.Descriptores;
using Utilidades;

namespace MVCSistemaDeElementos.Descriptores
{
    public class ListaDeValoresParaFiltrado<TElemento> : ControlFiltroHtml where TElemento : ElementoDto
    {
        ListaDeValores<TElemento> Lista { get; set; }

        BloqueDeFitro<TElemento> Bloque => (Padre is BloqueDeFitro<TElemento>) ? (Padre as BloqueDeFitro<TElemento>) : null;

        public string OtrasCssDelContenedor { get; set; }

        public ListaDeValoresParaFiltrado(IControlHtml Padre, string columna, string filtrarPor, Dictionary<string, string> opciones, string primeroOpcion = "", Posicion posicion = null)
        : base(padre: Padre
              , id: $"{Padre.Id}_{enumTipoControl.Check.Render()}_{columna}"
              , ""
              , ""
              , ""
              , posicion == null ? new Posicion(1, 1) : posicion
              )
        {
            //columna, tituloCheck, 
            var lista = new ListaDeValores<TElemento>(this, "", primeroOpcion.IsNullOrEmpty() ? "Seleccionar si mostrar con contenido": primeroOpcion, opciones, filtrarPor);
            IniciarClase(lista, posicion);
        }

        public ListaDeValoresParaFiltrado(IControlHtml Padre, ListaDeValores<TElemento> lista, string columna, Posicion posicion = null)
        : base(padre: Padre
              , id: $"{Padre.Id}_{enumTipoControl.Check.Render()}_{columna}"
              , ""
              , ""
              , ""
              , posicion == null ? new Posicion(1, 1) : posicion
              )
        {
            lista.Padre = this;
            IniciarClase(lista, posicion);
        }

        private void IniciarClase(ListaDeValores<TElemento> lista, Posicion posicion)
        {
            Tipo = enumTipoControl.ListaDinamicaParaMostrarColumna;

            Lista = lista;
            Etiqueta = lista.Etiqueta;

            if (Bloque != null)
            {
                Bloque.Tabla.Dimension.CambiarDimension(posicion);
                Bloque.AnadirControlEn(this);
            }
        }

        public override string RenderControl()
        {
            return RenderListaDeValoresParaMostrar();
        }

        public string RenderListaDeValoresParaMostrar()
        {
            var a = AtributosHtml.AtributosComunes($"div_{IdHtml}", IdHtml, PropiedadHtml, Tipo, Ayuda);
            var valores = a.MapearComunes();

            valores["CssContenedor"] = enumCssFiltro.ContenedorListaDeElementosMostrarModal.Render();
            if (!OtrasCssDelContenedor.IsNullOrEmpty())
                valores["CssContenedor"] = valores["CssContenedor"] + " " + OtrasCssDelContenedor;
            if (Bloque != null)
                valores["CssContenedor"] = $"{enumCssImportant.SinMarginTop.Render()} {valores["CssContenedor"]}";

            valores["Ayuda"] = "filtros a aplicar y check para indicar si se muestra la información";

            var control = PlantillasHtml.Render(PlantillasHtml.listaDeValoresConCheckDeMostrar, valores);

            control = control.Replace("[CssContenedorLv]",
                $"{(Bloque != null ? enumCssImportant.SinMarginTop.Render(): "")} {enumCssControles.ListaValoresMostrar.Render()}");

            return control.Replace("[ListaDeValores]", Lista.RenderListaDeValores());
        }

    }

}

