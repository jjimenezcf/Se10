using Utilidades;
using ModeloDeDto;
using System.Collections.Generic;

namespace MVCSistemaDeElementos.Descriptores
{
    public class CheckDeMostrarColumna<TElemento> : ControlFiltroHtml where TElemento : ElementoDto
    {
        public bool ValorInicial { get; private set; }

        public string Accion { get; private set; }
        public string Columna { get; }

        BloqueDeFitro<TElemento> Bloque => (Padre is BloqueDeFitro<TElemento>) ? (Padre as BloqueDeFitro<TElemento>) : null;


        public CheckDeMostrarColumna(IControlHtml padre, string etiqueta, string ayuda, bool valorInicial, string columna, Posicion posicion = null, List<string> columnas = null, string accion = null)
        : base(padre: padre
              , id: $"{padre.Id}_{enumTipoControl.Check.Render()}_{columna}"
              , etiqueta
              , ""
              , ayuda
              , posicion == null ? new Posicion(1,1): posicion
              )
        {
            Tipo = enumTipoControl.Check;
            ValorInicial = valorInicial;
            Columna = columna;
            if (columnas == null || columnas.Count == 0) columnas = new List<string> { columna };

            Accion = accion.IsNullOrEmpty() ? 
            $"onclick = javascript:{enumNameSpaceTs.Crud}.{enumGestorDeEventos.EventosDelMantenimiento}('{eventosDeMnt.OcultarMostrarColumnas}','{string.Join(Simbolos.separadorDeColumnas,columnas)}');":
            accion; 

            if (Bloque != null)
            {
                Bloque.Tabla.Dimension.CambiarDimension(posicion);
                Bloque.AnadirControlEn(this);
            }
        }

        public override string RenderControl()
        {
            return RenderCheck();
        }

        public string RenderCheck()
        {
            var a = AtributosHtml.AtributosComunes($"div_{IdHtml}", IdHtml, PropiedadHtml, Tipo, Ayuda);
            var valores = a.MapearComunes();

            var otraCalse = Padre is ModalDeFiltrado<TElemento> ? enumCssFiltro.ContenedorEnModalDeFiltros.Render() : "";
            valores["CssContenedor"] = Css.Render(enumCssFiltro.ContenedorCheck) + " " + otraCalse;
            valores["Css"] = Css.Render(enumCssFiltro.Check);
            valores["Etiqueta"] = Etiqueta;
            valores["Checked"] = ValorInicial.ToString().ToLower();
            valores["Accion"] = Accion;
            valores["Columna"] = Columna;

            return PlantillasHtml.Render(PlantillasHtml.checkDeMostrarColumna, valores);
        }

    }

}