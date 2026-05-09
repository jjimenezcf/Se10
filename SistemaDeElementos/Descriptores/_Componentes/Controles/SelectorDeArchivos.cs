using Utilidades;
using GestorDeElementos;
using ServicioDeDatos;

namespace MVCSistemaDeElementos.Descriptores
{
    public class SelectorDeArchivos : ControlHtml
    {
        public string IdHtmlNombre => $"{IdHtml}.nombre";
        public string IdHtmlSelector => $"{IdHtml}.ref";
        public string IdHtmlBarra => $"{IdHtml}.barra";
        public string IdHtmlContenedorBarra => $"{IdHtml}.contenedor.barra";

        public int LimiteEnByte { get; set; } = 0;
        public string ExtensionesValidas { get; }

        public ContenedorDeArchivos Contenedor => (ContenedorDeArchivos)Padre;

        public SelectorDeArchivos(ContenedorDeArchivos padre, string id, string etiqueta, string ayuda, string extensionesValidas, int limiteEnByte = 0)
            : base(padre, id,etiqueta,"",ayuda,null)
        {
            ExtensionesValidas = extensionesValidas;
            LimiteEnByte = limiteEnByte;
            Tipo = enumTipoControl.SelectorDeArchivos;
        }

        public string RenderSelector()
        {
            var idSelector = $"{IdHtml}-de-archivos"; // -selector-de-archivos";
            var idContenedorDeArchivosSeleccionados = $"{IdHtml}-seleccionados";
            var htmlArchivo = $@"<div id = ¨{idSelector}¨ class=¨{Css.Render(enumCssControles.SelectorDeArchivos)}¨ >
                                   <div class='{enumCssControles.ContenedorDeOpcion.Render()}'>
                                     <button id='{idSelector}-seleccionar' title='Seleccionar archivos' onclick={enumNameSpaceTs.ApiDeArchivos}.SeleccionarArchivos('{IdHtml}')>
                                         <img src='/images/menu/SeleccionarArchivo.png' >
                                     </button>
                                     <button id='{idSelector}-subir-porta-papeles' title='Subir porta papeles' onclick={enumNameSpaceTs.ApiDeArchivos}.PulsadoSubirPortaPapeles('{IdHtml}')> 
                                        <img src='{CacheDeVariable.Uri_SubirDelPortaPapeles}'>
                                      </button>
                                   </div>
                                   <div class='{enumCssControles.ContenedorDeOpcion.Render()}'>

                                      <button id='{idSelector}-subir' title='Subir archivos seleccionados' onclick={enumNameSpaceTs.ApiDeArchivos}.PulsadoAnexarArchivos('{IdHtml}')> 
                                        <img src='/images/menu/SubirArchivo.png'>
                                      </button>

                                      <button id='{idSelector}-copiar' class='copiar-archivos operaciones-archivos' title='Copiar archivos seleccionados' onclick={enumNameSpaceTs.ApiDeArchivos}.{enumFunctionTs.SisDoc_Procesar}('modal-{IdHtml.Replace("-selector", "-seleccionar-destino")}','copiar')> 
                                       <span class='icon'></span>
                                      </button>

                                      <button id='{idSelector}-mover'  class='mover-archivos operaciones-archivos' title='Mover archivos seleccionados' onclick={enumNameSpaceTs.ApiDeArchivos}.{enumFunctionTs.SisDoc_Procesar}('modal-{IdHtml.Replace("-selector", "-seleccionar-destino")}','mover')> 
                                        <span class='icon'></span>
                                      </button>

                                      <button id='{idSelector}-enlazar' class='enlazar-archivos operaciones-archivos' title='Enlazar archivos seleccionados' onclick={enumNameSpaceTs.ApiDeArchivos}.{enumFunctionTs.SisDoc_Procesar}('modal-{IdHtml.Replace("-selector", "-seleccionar-destino")}','enlazar')> 
                                       <span class='icon'></span>
                                      </button>

                                      <button id='{idSelector}-BloqueoMultiple' class='bloquear-archivos operaciones-archivos' title='Bloquear archivos seleccionados' onclick={enumNameSpaceTs.ApiDeArchivos}.{enumFunctionTs.SisDoc_MostrarModalDeBloqueoMultiple}('modal-{IdHtml.Replace("-selector", "-bloqueo-multiple")}')> 
                                        <span class='icon'></span>
                                      </button>

                                      <button id='{idSelector}-DesbloqueoMultiple' class='desbloquear-archivos operaciones-archivos' title='Desbloquear archivos seleccionados' onclick={enumNameSpaceTs.ApiDeArchivos}.{enumFunctionTs.SisDoc_MostrarModalDeDesbloqueoMultiple}('modal-{IdHtml.Replace("-selector", "-desbloqueo-multiple")}')> 
                                       <span class='icon'></span>                                      
                                      </button>

                                      <button id='{idSelector}-generarZip' class='zippear-archivos operaciones-archivos' title='Genera archivador con los archivos seleccionados en un ZIP' onclick={enumNameSpaceTs.ApiDeArchivos}.{enumFunctionTs.SisDoc_MostrarModalDeGenerarZip}('modal-{IdHtml.Replace("-selector", "-generar-zip")}')> 
                                        <span class='icon'></span>
                                      </button>

                                   </div>
                                      <div id ='div-contenedor-de-opcion-derecha' class='{enumCssControles.ContenedorDeOpcionDerecha.Render()} {enumCssControles.ContenedorDeOpcion.Render()}'>
                                        <input id='{idSelector}-filtrar' class='{enumCssControles.FiltroSelectorDeArchivos.Render()}' 
                                                      placeholder='filtro por nombre de fichero, puede indicar % para varios patrones' 
                                                      oninput='javascript:{nameof(enumNameSpaceTs.ApiDeArchivos)}.{nameof(enumFunctionTs.SisDoc_FiltrarEnSelector)}(this)' 
                                                      onchange='javascript:{nameof(enumNameSpaceTs.ApiDeArchivos)}.{nameof(enumFunctionTs.SisDoc_RecargarVisor)}(this)'></input>
                                        <div class='{enumCssControles.LinksOpcionesSelectorDeArchivos.Render()}'>
                                          <a id ='ref-cambiar-encolumnado' href='#' onclick='{enumNameSpaceTs.ApiDeArchivos}.{enumFunctionTs.SisDoc_CambiarEncolumnado}(¨{Padre.IdHtml}¨); return false;' >Nº de columnas: 5</a>
                                          <a id ='ref-seleccionar-todo' href='#' onclick='{enumNameSpaceTs.ApiDeArchivos}.{enumFunctionTs.SisDoc_SeleccionarTodo}(¨{Padre.IdHtml}¨); return false;' >Seleccionar todo</a>
                                          <a id ='ref-anular-seleccion' href='#' onclick='{enumNameSpaceTs.ApiDeArchivos}.{enumFunctionTs.SisDoc_AnularSeleccion}(¨{Padre.IdHtml}¨); return false;' >Deseleccionar</a>
                                        </div>
                                      </div>
                                 </div>
                                 <div id = ¨{idContenedorDeArchivosSeleccionados}¨ class=¨{Css.Render(enumCssControles.ArchivosSeleccionados)}¨ >
                                 </div>
                                   <div>
                                      <input  {RenderAtributos(Id, IdHtml, Tipo, enumCssControlesFormulario.Archivo.Render(), Ayuda)}
                                         type=¨{enumInputType.file.Render()}¨ 
                                         multiple 
                                         name=¨fichero¨    
                                         style=¨display: none;¨
                                         accept=¨{ExtensionesValidas}¨
                                         negocio=¨{NegociosDeSe.ToNombre(Contenedor.Negocio)}¨
                                         archivos-seleccionados ='{idContenedorDeArchivosSeleccionados}'
                                         contenedor-donde-mostrar ='{Contenedor.IdHtml}'
                                         limite-en-byte = {LimiteEnByte}
                                         barra-vinculada = ¨{IdHtmlBarra}¨ 
                                         onChange=¨ApiDeArchivos.CrearArchivosPendietesDeSubir('{IdHtml}', null)¨ />
                                   </div>
                                 <div id = ¨{IdHtmlContenedorBarra}¨ class=¨{Css.Render(enumCssControlesFormulario.InfoArchivo)}¨ style=¨display: none;¨>
                                    <div id = ¨{IdHtmlBarra}¨ class=¨{Css.Render(enumCssControles.BarraAzulArchivo)}¨ contenedor-barra = ¨{IdHtmlContenedorBarra}¨ style=¨display: none;¨>
                                       <span></span>
                                    </div>
                                 </div>
                                ";
            return htmlArchivo;
        }

        public override string RenderControl()
        {
            return RenderSelector();
        }
    }
}
