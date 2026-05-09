using System;
using System.Collections.Generic;
using Utilidades;
using ModeloDeDto;
using ServicioDeDatos;

namespace MVCSistemaDeElementos.Descriptores
{

    public class AmpliacionDeEdicion : ControlHtml
    {
        public TablaEdicion Tabla { get; set; }

        public ICollection<ControlHtml> Controles => Tabla.Controles;

        public bool HayExpansor { get; private set; } = false;

        public bool Plegado { get; set; } = false;

        public Type Dto { get; set; }

        private string _controlador;
        public string Controlador { get { return _controlador.Replace(ltrEndPoint.Controller, ""); } set { _controlador = value; } }

        public AmpliacionDeEdicion(IControlConIdNegocio editor, string id, string titulo, Dimension dimension, string ayuda, List<string> propiedadesNoRenderizables = null)
        : base(
          padre: editor,
          id: $"{editor.Id}_{enumTipoControl.Ampliacion}_{id}",
          etiqueta: titulo,
          propiedad:null,
          ayuda: ayuda,
          posicion: null
        )
        {
            Tipo = enumTipoControl.Ampliacion;
            Tabla = new TablaEdicion(this, dimension, new List<ControlHtml>(), propiedadesNoRenderizables);
            Plegado = false;
        }


        private void AjustarDimensionDeLaTabla()
        {
            foreach (var control in Controles)
            {
                if (control.Posicion.fila >= Tabla.Dimension.Filas)
                    Tabla.Dimension.NumeroDeFilas(control.Posicion.fila + 1);
                if (control.Posicion.columna >= Tabla.Dimension.Columnas)
                    Tabla.Dimension.NumeroDeColumnas(control.Posicion.columna + 1);
            }
        }

        public string RenderContenedorDeCuerpo()
        {
            var idHtmlCuerpoDeCreacion = $"contenedor_dto_{IdHtml}";
            var htmlModal = $@"<div id=¨{idHtmlCuerpoDeCreacion}¨ class=¨{enumCssEdicion.ContenedorDeAmpliacion.Render()}¨ tipo='{Tipo.Render()}' es-ampliacion='true' controlador = '{Controlador}' tipoDto ='{Dto.FullName}'>
                                 {DescriptorDeTabla.htmlRenderObjetoVacio(Padre, Dto, Controlador, idHtmlCuerpoDeCreacion, enumCssEdicion.TablaDeEdicion.Render(), enumModoDeTrabajo.Edicion, Tabla.NoRenderizar)}
                               </div>";
            return htmlModal;
        }

        public override string RenderControl()
        {
            AjustarDimensionDeLaTabla();

            return $@"
                  <div id=¨mostrar.{IdHtml}¨ class='{enumCssEdicion.Ampliacion.Render()}'> 
                        <a id=¨mostrar.{IdHtml}.ref¨ 
                           style=¨margin-left: 2px;¨
                           href=¨javascript:Crud.{enumGestorDeEventos.EventosDelMantenimiento}('{eventosDeMnt.OcultarMostrarAmpliacion}', '{IdHtml}');¨>                           
                            <img id='imagen.{IdHtml}' class='{enumCssEdicion.AbrirImagenDeAmpliacion.Render()}'> 
                        </a>
                        <label id='titulo.{IdHtml}.ref' class='{enumCssEdicion.TituloDeAmpliacion.Render()}'>{Etiqueta}</label>  
                        <input id=¨expandir.{IdHtml}.input¨ type=¨hidden¨ value={(Plegado ? Literal.Uno : Literal.Cero)}> 
                        <div id=¨{IdHtml}¨  class=¨{Css.Render(Plegado ? enumCssDiv.DivVisible : enumCssDiv.DivOculto)}¨>
                          {RenderContenedorDeCuerpo()}
                        </div>
                   </div>";
        }

        internal string RenderAmpliacion()
        {
           return RenderControl();
        }
    }

}
