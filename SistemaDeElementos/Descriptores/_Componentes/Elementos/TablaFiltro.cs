using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ModeloDeDto;
using Utilidades;
using UtilidadesParaIu;

namespace MVCSistemaDeElementos.Descriptores
{

    public class Dimension
    {
        public int Filas { get; private set; }
        public int Columnas { get; private set; }

        public Dimension(int filas, int columnas)
        {
            Filas = filas;
            Columnas = columnas;
        }

        public void CambiarDimension(Posicion posicion)
        {
            if (posicion.fila >= Filas)
                Filas = posicion.fila + 1;

            if (posicion.columna >= Columnas)
                Columnas = posicion.columna + 1;
        }

        internal void NumeroDeFilas(int filas)
        {
            Filas = filas;
        }
        internal void NumeroDeColumnas(int columnas)
        {
            Columnas = columnas;
        }
    }

    public class TablaFiltro : ControlFiltroHtml
    {
        private Dimension _dimension;
        public Dimension Dimension
        {
            get
            {
                if (Controles.Count == 0) return _dimension;
                try
                {

                    return new Dimension(
                             Controles.OrderByDescending(x => x.Posicion?.fila ?? null).FirstOrDefault()?.Posicion.fila + 1 ?? 0,
                             Controles.OrderByDescending(x => x.Posicion?.columna ?? null).FirstOrDefault()?.Posicion.columna + 1 ?? 0
                             );
                }
                catch { return _dimension; }
            }
            private set
            {
                _dimension = value;
            }
        }
        public ICollection<ControlFiltroHtml> Controles { get; set; }
        public object AnchoDeEtiqueta { get; set; } = 10;


        public TablaFiltro(ControlFiltroHtml padre, Dimension dimension, ICollection<ControlFiltroHtml> controles)
        : base(
          padre: padre,
          id: $"{padre.Id}_Tabla",
          etiqueta: null,
          propiedad: null,
          ayuda: null,
          posicion: null
        )
        {
            Tipo = enumTipoControl.TablaBloque;
            Dimension = dimension;
            Controles = controles;


        }

        public override string RenderControl()
        {
            return RenderTabla(enumCssFiltro.FilaFiltro);
        }

        public string RenderTabla(enumCssFiltro cssDeLaFila)
        {
            var htmlTabla = $@" 
                <!--  ***************** tabla de filtrado: {Padre.Etiqueta} ******************* -->
                [filas]
                ";
            var htmlFilas = "";
            for (var i = 0; i < Dimension.Filas; i++)
                if (HayControlesEnLaFila(i))
                    htmlFilas = $"{htmlFilas}{(htmlFilas.IsNullOrEmpty() ? "" : Environment.NewLine)}{RenderFila(i, cssDeLaFila)}";

            return htmlTabla.Replace("[filas]", htmlFilas);
        }

        private string RenderFila(int i, enumCssFiltro cssDeLaFila)
        {
            var templateColumn = "";
            var columnas = Dimension.Columnas; // ColumnasPorFila(i);
            //var porcentaje = 100 / ColumnasTotales();
            for (int ii = 0; ii < columnas; ii++) //-1
                templateColumn = $"{templateColumn} 1fr"; //{porcentaje}% 

            var idFila = $"{IdHtml}_{i}";
            var htmlFila = $@"<div id=¨{idFila}¨ class='{cssDeLaFila.Render()}' style = 'grid-template-columns: {templateColumn};'>
                                 columnas
                              </div>";
            var htmlColumnas = "";

            //style = 'grid-template-columns: {templateColumn} auto;'

            for (var j = 0; j < columnas; j++)
            {
                var idColumna = $"{idFila}_{j}";

                htmlColumnas = $"{htmlColumnas}{(htmlColumnas.IsNullOrEmpty() ? "" : Environment.NewLine)}" +
                    $"<div id='{idColumna}' class=¨{enumCssFiltro.ColumnaFiltro.Render()}¨>" +
                    $"{(!HayControlesEnLaColumnaDeLaFila(i, j) ? "" : RenderColumnasControl(idFila, i, j))}" +
                    $"</div>"
                    ;
            }

            // style=¨display: grid;grid-template-columns: {AnchoDeEtiqueta}em auto;¨

            return htmlFila.Replace("columnas", htmlColumnas);
        }

        private string RenderColumnasControl(string idFila, int i, int j)
        {
            var htmlColumnaEtiqueta = "etiqueta";

            var htmlColumnaControl = "control";

            var htmlControl = "";
            var htmlEtiqueta = "";
            var controlesEnLaCelda = Controles.Where(x => x.Posicion != null && x.Posicion.fila == i && x.Posicion.columna == j);

            foreach (ControlHtml c in controlesEnLaCelda) 
            {
                if (c is null) continue;

                if (c.Posicion.fila >= Dimension.Filas)
                    Gestor.Errores.GestorDeErrores.Emitir($"El control {c.Propiedad} no puede ser renderizado en la fila indicada {c.Posicion.fila}, solo hay {Dimension.Filas} filas");

                if (c.Posicion.columna >= Dimension.Columnas)
                    Gestor.Errores.GestorDeErrores.Emitir($"El control {c.Propiedad} no puede ser renderizado en la columna indicada {c.Posicion.columna}, solo hay {Dimension.Columnas} columnas");

                if (!c.Etiqueta.IsNullOrEmpty())
                    htmlEtiqueta = $"{(c.Tipo == enumTipoControl.Check ? "<div></div>" : c.RenderEtiqueta())}";

                if (!htmlControl.IsNullOrEmpty())
                    htmlControl = $"<div class='{enumCssFiltro.ContenedorDeDosControlesEnCelda.Render()}'>{htmlControl}{c.RenderControl()}</div>";
                else
                    htmlControl = c.RenderControl();
            }


            var htmlCelda = htmlColumnaEtiqueta.Replace("etiqueta", htmlEtiqueta) +
                   Environment.NewLine +
                   htmlColumnaControl.Replace("control", htmlControl);

            return htmlCelda;
        }

        private bool HayControlesEnLaFila(int fila)
        {

            foreach (ControlHtml c in Controles)
            {
                if (c.Posicion == null)
                    continue;
                if (c.Posicion.fila == fila) return true;
            }
            return false;
        }
        private bool HayControlesEnLaColumnaDeLaFila(int fila, int columna)
        {

            foreach (ControlHtml c in Controles)
            {
                if (c.Posicion == null)
                    continue;
                if (c.Posicion.fila == fila && c.Posicion.columna == columna) return true;
            }
            return false;
        }

        public ControlFiltroHtml ControlEn(Posicion posicion)
        {
            return Controles.First(x => x.Posicion.fila == posicion.fila && x.Posicion.columna == posicion.columna);
        }

        public bool HayControlEn(Posicion posicion)
        {
            return Controles.Where(x => x.Posicion.fila == posicion.fila && x.Posicion.columna == posicion.columna).Count() > 0;
        }

        public bool HayControlDelanteDe(Posicion posicion)
        {
            return HayControlEn(new Posicion { fila = posicion.fila, columna = posicion.columna - 1 });
        }

        private int ColumnasTotales()
        {
            var numero = 0;
            foreach (var c in Controles)
            {
                if (c.Posicion == null)
                    continue;

                if (c.Posicion.columna > numero)
                    numero = c.Posicion.columna;
            }
            return numero + 1;
        }

        private int ColumnasPorFila(int fila)
        {
            var numero = 0;
            foreach (var c in Controles)
            {
                if (c.Posicion == null)
                    continue;

                if (c.Posicion.fila != fila)
                    continue;

                if (c.Posicion.columna > numero)
                    numero = c.Posicion.columna;
            }
            return numero + 1;
        }

    }
}
