using System;
using System.Collections.Generic;
using ModeloDeDto;
using Utilidades;

namespace MVCSistemaDeElementos.Descriptores
{

    public class TablaEdicion: ControlHtml
    {
        public Dimension Dimension { get; private set; }
        public ICollection<ControlHtml> Controles { get; set; }
        public object AnchoDeEtiqueta { get; set; } = 10;
        public List<string> NoRenderizar { get; }

        public TablaEdicion(ControlHtml padre, Dimension dimension, ICollection<ControlHtml> controles, List<string> propiedadesNoRenderizables = null)
        : base(
          padre: padre,
          id: $"{padre.Id}_Tabla",
          etiqueta: null,
          ayuda: null,
          propiedad: null,
          posicion:null
        )
        {
            Tipo = enumTipoControl.TablaBloque;
            Dimension = dimension;
            Controles = controles;
            NoRenderizar = propiedadesNoRenderizables;
        }

        public string RenderTablaEdicion()
        {
            var htmlTabla = $@" 
                <!--  ***************** tabla de edición: {((BloqueApilado)Padre).Titulo} ******************* -->
                <div id=¨{IdHtml}¨ class=¨{Css.Render(enumCssMnt.MntTablaDeFiltro)}¨>
                    filas
                </div>";
            var htmlFilas = "";
            for (var i = 0; i < Dimension.Filas; i++)
                htmlFilas = $"{htmlFilas}{(htmlFilas.IsNullOrEmpty() ? "" : Environment.NewLine)}{RenderFila(i)}";

            return htmlTabla.Replace("filas", htmlFilas);
        }

        private string RenderFila(int i)
        {
            var templateColumn = "";
            var columnas = ColumnasPorFila(i);
            var porcentaje = 100 / ColumnasTotales();
            for (int ii = 0; ii < columnas -1 ; ii++)
                templateColumn = $"{templateColumn} {porcentaje}% ";

            var idFila = $"{IdHtml}_{i}";
            var htmlFila = $@"<div id=¨{idFila}¨ class='fila-filtro' style = 'grid-template-columns: {templateColumn} auto;'>
                                 columnas
                              </div>";
            var htmlColumnas = "";


            for (var j = 0; j < columnas; j++)
            {
                var idColumna = $"{idFila}_{j}";
                htmlColumnas = $"{htmlColumnas}{(htmlColumnas.IsNullOrEmpty() ? "" : Environment.NewLine)}" +
                    $"<div id='{idColumna}' style=¨display: grid;grid-template-columns: {AnchoDeEtiqueta}em auto;¨>" +
                    $"{RenderColumnasControl(idFila, i, j)}" +
                    $"</div>"
                    ;
            }


            return htmlFila.Replace("columnas", htmlColumnas);
        }

        private string RenderColumnasControl(string idFila, int i, int j)
        {

            var idColumna = $"{idFila}_{j}";
            var htmlColumnaEtiqueta = $@"<div id=¨{idColumna}_e¨>
                                            etiqueta
                                         </div>";

            var htmlColumnaControl = $@"<div id=¨{idColumna}_c¨>
                                           control
                                        </div>";

            var htmlControl = "";
            var htmlEtiqueta = "";
            foreach (ControlHtml c in Controles)
            {
                if (c.Posicion == null)
                    continue;

                if (NoRenderizar != null && NoRenderizar.Contains(c.Propiedad))
                    continue;

                if (c.Posicion.fila >= Dimension.Filas)
                    Gestor.Errores.GestorDeErrores.Emitir($"El control {c.Propiedad} no puede ser renderizado en la fila indicada {c.Posicion.fila}, solo hay {Dimension.Filas} filas");

                if (c.Posicion.columna >= Dimension.Columnas)
                    Gestor.Errores.GestorDeErrores.Emitir($"El control {c.Propiedad} no puede ser renderizado en la columna indicada {c.Posicion.columna}, solo hay {Dimension.Columnas} columnas");

                if (c.Posicion.fila == i && c.Posicion.columna == j)
                    htmlEtiqueta = $"{(c.Tipo == enumTipoControl.Check ? "" : c.RenderEtiqueta())}";

                if (c.Posicion.fila == i && c.Posicion.columna == j)
                    htmlControl = $"{c.RenderControl()}";
            }


            return htmlColumnaEtiqueta.Replace("etiqueta", htmlEtiqueta) +
                   Environment.NewLine +
                   htmlColumnaControl.Replace("control", htmlControl);
        }

        private int ColumnasTotales()
        {
            var numero = 0;
            foreach (var c in Controles)
            {
                if (c.Posicion == null)
                    continue;

                if (NoRenderizar != null && NoRenderizar.Contains(c.Propiedad))
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

                if (NoRenderizar != null && NoRenderizar.Contains(c.Propiedad))
                    continue;

                if (c.Posicion.columna > numero)
                    numero = c.Posicion.columna;
            }
            return numero +1;
        }

        public override string RenderControl()
        {
            return RenderTablaEdicion();
        }
    }
}
