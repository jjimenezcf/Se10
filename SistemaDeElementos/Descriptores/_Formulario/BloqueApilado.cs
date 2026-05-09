using System.Collections.Generic;
using Utilidades;

namespace MVCSistemaDeElementos.Descriptores
{
    public class BloqueApilado: IBloqueDeFormulario
    {
        public CuerpoDeFormulario Cuerpo { get; }
        public string Id { get; }

        public string IdHtml => $"{Cuerpo.IdHtml}-{Id.ToLower()}";
        public string Titulo { get; }
        public List<ControlDeFormulario> Izquierdo { get; }
        public List<ControlDeFormulario> Derecho { get; }

        public BloqueApilado(CuerpoDeFormulario cuerpo, string id, string titulo)
        {
            Cuerpo = cuerpo;
            Id = id;
            Titulo = titulo;
            Izquierdo = new List<ControlDeFormulario>();
            Derecho = new List<ControlDeFormulario>();
        }

        public string RenderBloqueApilado()
        {
            var maximoNumeroDeFila = Izquierdo.Count > Derecho.Count ? Izquierdo.Count : Derecho.Count;

            var bloqueHtml = $@"<div id = {IdHtml}-izquierdo class = {Css.Render(enumCssFormulario.BloqueIzquierdo)}>
                                    <div class = '{enumCssDiv.Tabla.Render()} {Css.Render(enumCssFormulario.Tabla)}'>
                                    <div class = '{enumCssDiv.Thead.Render()}></div>
                                    <div class = '{enumCssDiv.Tbody.Render()}>
                                         {RenderTbody(Izquierdo, maximoNumeroDeFila)}
                                    </div>
                                </div>
                                </div>
                                <div id = {IdHtml}-derecho class = {Css.Render(enumCssFormulario.BloqueDerecho)}>
                                    <div class = '{enumCssDiv.Tabla.Render()} {Css.Render(enumCssFormulario.Tabla)}'>
                                    <div class = '{enumCssDiv.Thead.Render()}></div>
                                    <div class = '{enumCssDiv.Tbody.Render()}>
                                         {RenderTbody(Derecho, maximoNumeroDeFila)}
                                    </div>
                                </div>
                                </div>
                              ";
            return bloqueHtml;
        }

        private string RenderTbody(List<ControlDeFormulario> controles, int numeroDeFilas)
        {
            var filas = "";
            var i = 0;
            foreach (var control in controles)
            {
                filas = $@"{filas}{RenderFila(control)}";
                i++;
            }
            if (i < numeroDeFilas)
                for (int j = i; j < numeroDeFilas; j++)
                    filas = $@"{filas}{RenderFilaVacia()}";

            return filas;
        }


        public string RenderFilaVacia()
        {
            return $@"<div class = ¨{enumCssDiv.Tr.Render()} {Css.Render(enumCssFormulario.fila)}¨>
                        <div class = ¨{enumCssDiv.Td.Render()} {Css.Render(enumCssFormulario.columnaLabel)}¨>
                        </div>
                        <div class = ¨{enumCssDiv.Td.Render()} {Css.Render(enumCssFormulario.columnaControl)}¨>
                        </div>
                      </div>";
        }

        public string RenderFila(ControlDeFormulario control)
        {
            return $@"<div class = ¨{enumCssDiv.Tr.Render()} {Css.Render(enumCssFormulario.fila)}¨>
                        {RenderContenidoDeLaFila(control)}
                      </div>";
        }

        private object RenderContenidoDeLaFila(ControlDeFormulario control)
        {
            switch (control.Tipo)
            {
                case enumTipoControl.Archivo: return RenderArchivo((ControlDeArchivoEnFormulario)control);
                case enumTipoControl.Editor: return RenderEditor(control);
            }

            throw new System.Exception($"No se ha implementado como renderizar un control del tipo {control.Tipo.Render()}");
        }

        private object RenderEditor(ControlDeFormulario control)
        {
            var htmlfilaEditor = $@"
                        <div class = ¨{enumCssDiv.Td.Render()} {Css.Render(enumCssFormulario.columnaLabel)}¨>
                           <label for=¨{control.IdHtml}¨>{control.Etiqueta}</label>
                        </div>
                        <div class = ¨{enumCssDiv.Td.Render()} {Css.Render(enumCssFormulario.columnaControl)}¨>
                           {((ControlDeEdicion)control).RenderEditor()}
                        </div>";
            return htmlfilaEditor;
        }

        private object RenderArchivo(ControlDeArchivoEnFormulario control)
        {
            var htmlfilaArchivo = $@"
                        <div class = ¨{enumCssDiv.Td.Render()} {Css.Render(enumCssFormulario.columnaLabel)}¨>
                           <a id=¨{control.IdHtmlSelector}¨ 
                              class=¨{Css.Render(enumCssControlesFormulario.SelectorArchivo)}¨ 
                              href=¨javascript:ApiDeArchivos.SeleccionarArchivo('{control.IdHtml}')¨>
                              {control.Etiqueta}
                           </a>
                        </div>
                        <div class = ¨{enumCssDiv.Td.Render()} {Css.Render(enumCssFormulario.columnaControl)}¨>
                           {control.RenderArchivo()}
                        </div>";
            return htmlfilaArchivo;
        }


    }
}
