using Utilidades;

namespace MVCSistemaDeElementos.Descriptores
{
    public class ControlDeArchivoEnFormulario : ControlDeFormulario
    {
        public string IdInfoArchivo => $"{IdHtml}.nombre";
        public string IdHtmlSelector => $"{IdHtml}.ref";
        public string IdHtmlBarra => $"{IdHtml}.barra";
        public string IdHtmlContenedorBarra => $"{IdHtml}.contenedor.barra";

        public int LimiteEnByte { get; set; } = 0;
        public string ExtensionesValidas { get; }

        public ControlDeArchivoEnFormulario(BloqueApilado padre, string id, string etiqueta, string ayuda, string extensionesValidas, int limiteEnByte = 0)
            : base(padre, id, enumTipoControl.Archivo, etiqueta, enumCssControlesFormulario.Archivo, ayuda)
        {
            ExtensionesValidas = extensionesValidas;
            LimiteEnByte = limiteEnByte;
        }

        public string RenderArchivo()
        {            
            var htmlArchivo = $@"<form method=¨post¨ action=¨SubirArchivo¨ enctype=¨multipart/form-data¨>
                                   <input  {RenderAtributos(Id, IdHtml, Tipo, ClaseCss, Ayuda)}
                                       type=¨{enumInputType.file.Render()}¨ 
                                       name=¨fichero¨  
                                       style=¨display: none;¨
                                       accept=¨{ExtensionesValidas}¨
                                       controlador=¨{Padre.Cuerpo.Formulario.Controlador}¨
                                       info-archivo=¨{IdInfoArchivo}¨
                                       limite-en-byte = {LimiteEnByte}
                                       barra-vinculada = ¨{IdHtmlBarra}¨ 
                                       onChange=¨ApiDeArchivos.MostrarArchivoSelecionado('{Padre.Cuerpo.IdHtml}','{IdHtml}','{IdInfoArchivo}')¨ />
                                   <input {RenderAtributos(propiedad: ""
                                       , IdInfoArchivo
                                       , enumTipoControl.Editor
                                       , enumCssControlesFormulario.InfoArchivo
                                       , ayuda: ""
                                       , $"type = ¨{enumInputType.text.Render()}¨")}
                                       readonly>
                                   </input>
                                   <div id = ¨{IdHtmlContenedorBarra}¨ class=¨{Css.Render(enumCssControlesFormulario.InfoArchivo)}¨ style=¨display: none;¨>
                                      <div id = ¨{IdHtmlBarra}¨ class=¨{Css.Render(enumCssControles.BarraAzulArchivo)}¨ contenedor-barra = ¨{IdHtmlContenedorBarra}¨ style=¨display: none;¨>
                                         <span></span>
                                      </div>
                                   </div>
                                 </form>
                                ";
            return htmlArchivo;
        }
    }
}
