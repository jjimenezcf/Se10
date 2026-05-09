using System;
using System.Collections.Generic;
using Utilidades;
using ServicioDeDatos.Seguridad;
using ModeloDeDto;
using Gestor.Errores;
using ServicioDeDatos;
using static Gestor.Errores.GestorDeErrores;

namespace MVCSistemaDeElementos.Descriptores
{

    public class AtributosHtml
    {
        public string IdHtmlContenedor { get; set; }
        public string IdHtml { get; set; }
        public string Propiedad { get; set; }
        public enumTipoControl TipoDeControl { get; set; }
        public bool Visible { get; set; }
        public bool Editable { get; set; }
        public bool Obligatorio { get; set; }
        public string AnchoMaximo { get; set; }
        public int NumeroDeFilas { get; set; } = -1;
        public string Ayuda { get; internal set; }
        public object ValorPorDefecto { get; internal set; }
        public int LongitudMaxima { get; internal set; } = 0;
        public string Etiqueta { get; set; }
        public string Url { get; set; }
        public string NameSpaceTs { get; set; }
        public string IdPaginaDestino { get; set; }
        public string AlPerderElFoco { get; set; }
        public string AlPulsarElBoton { get; set; }

        public string Ampliacion { get; set; }

        public string PanelPadre { get; set; }
        public string SelectorHasta { get; set; }

        public bool ConAccion => CssBotonAccion != enumCssControles.Nulo || !OnClick.IsNullOrEmpty();

        public enumCssControles CssBotonAccion { get; set; } = enumCssControles.Nulo;
        public string OnClick { get; set; }

        public AtributosHtml()
        {

        }

        public AtributosHtml(string idHtmlContenedor, string idHtml, string propiedad, enumTipoControl tipoDeControl, bool visible, bool editable, bool obligatorio, string ayuda, object valorPorDefecto)
        {
            IdHtmlContenedor = idHtmlContenedor;
            IdHtml = idHtml;
            Propiedad = propiedad;
            TipoDeControl = tipoDeControl;
            Visible = visible;
            Editable = editable;
            Obligatorio = obligatorio;
            Ayuda = ayuda;
            ValorPorDefecto = valorPorDefecto;
        }

        public AtributosHtml(string idHtml, string propiedad, enumTipoControl tipoDeControl, bool visible, bool editable, bool obligatorio, string ayuda, object valorPorDefecto)
        : this($"{idHtml}-contenedor", idHtml, propiedad, tipoDeControl, visible, editable, obligatorio, ayuda, valorPorDefecto)
        {
        }

        public static AtributosHtml AtributosComunes(string idHtmlContenedor, string idHtml, string propiedad
            , enumTipoControl tipoDeControl
            , string ayuda)
        {
            var a = new AtributosHtml();
            a.IdHtmlContenedor = idHtmlContenedor;
            a.IdHtml = idHtml;
            a.Propiedad = propiedad;
            a.TipoDeControl = tipoDeControl;
            a.Editable = true;
            a.Visible = true;
            a.Obligatorio = false;
            a.Ayuda = ayuda;

            return a;
        }
        public static string Mf(enumCssOpcionMenu clase, enumModoDeAccesoDeDatos modoNecesario, bool permiteMultiSeleccion, int seleccionables = -1)
        {
            if (!permiteMultiSeleccion) seleccionables = 1;
            return $@"tipo=¨{enumTipoControl.Opcion.Render()}¨
                              clase=¨{clase.Render()}¨
                              permisos-necesarios=¨{modoNecesario.Render()}¨
                              permite-multi-seleccion={permiteMultiSeleccion}
                              numero-maximo-seleccionable='{seleccionables}' ";
        }


    }

    public static class AtributosHtmlExtension
    {
        public static Dictionary<string, object> MapearComunes(this AtributosHtml atributos)
        {
            var valores = new Dictionary<string, object>();

            valores["ContenidoEn"] = atributos.PanelPadre;
            valores["IdHtmlContenedor"] = atributos.IdHtmlContenedor;
            valores["IdHtml"] = atributos.IdHtml;
            valores["Propiedad"] = atributos.Propiedad;
            valores["Tipo"] = atributos.TipoDeControl.Render();
            valores["Obligatorio"] = atributos.Visible && atributos.Obligatorio ? "S" : "N";
            valores["Readonly"] = !atributos.Editable ? "editable=¨N¨ readonly" : "editable=¨S¨";
            valores["Estilos"] = atributos.AnchoMaximo.IsNullOrEmpty() ? "" : $"max-width: {atributos.AnchoMaximo};";
            valores["Etiqueta"] = atributos.Etiqueta;
            valores["Ayuda"] = atributos.Ayuda;
            valores["Ampliacion"] = atributos.Ampliacion;
            return valores;
        }

    }

    public interface IControlHtml
    {
        public string Id { get; }
        public string IdHtml { get; }
        public string Etiqueta { get; }
        public IControlHtml Padre { get; }
        string RenderControl();
    }

    public interface IControlConIdNegocio : IControlHtml
    {
        public int IdNegocio { get; }
    }

    public interface IExpanes 
    {
        public List<DescriptorDeExpansor> Expanes { get; set; }
    }


    public interface IControlConIdNegocioConExpansor : IControlConIdNegocio, IExpanes
    {
        ContextoSe Contexto { get; }
    }

    public abstract class ControlHtml : IControlHtml
    {
        private string _id;
        private static List<string> _ListaDeIdsAsignados = new List<string>();

        public string Id
        {
            get { return _id; }
            set
            {
                string idTmp = value;
                int cont = 1;
                while (_ListaDeIdsAsignados.Contains(idTmp))
                {
                    ServicioDeCaches.EliminarCachesDeDescriptores();
                    _ListaDeIdsAsignados.Remove(idTmp);
                    Emitir($"El id '{value}' ya está asignado", enumCodigoDeError.ElementoYaRenderizado);
                    idTmp = $"{value}_{cont}";
                    cont++;
                }
                _ListaDeIdsAsignados.Add(idTmp);
                _id = idTmp;
            }
        }
        public void ModificarId(string id)
        {
            _ListaDeIdsAsignados.Remove(Id);
            Id = id;          
        }

        public string IdHtml => Id.ToLower();
        public string Etiqueta { get; set; }
        public string Propiedad { get; private set; }
        public string PropiedadHtml => Propiedad.ToLower();
        public string Ayuda { get; set; }
        public Posicion Posicion { get; set; }
        public enumTipoControl Tipo { get; protected set; }

        public bool Visible { get; set; } = true;
        public bool Editable { get; set; } = true;
        public bool Obligatorio { get; set; } = false;
        public string AnchoMaximo { get; set; }

        public IControlHtml Padre { get; set; }

        public ControlHtml()
        {

        }

        public ControlHtml(IControlHtml padre, string id, string etiqueta, string propiedad, string ayuda, Posicion posicion, bool resetearListaDeIds = false)
        {
            if (resetearListaDeIds) _ListaDeIdsAsignados = new List<string>();
            Padre = padre;
            Id = id;
            Etiqueta = etiqueta;
            Propiedad = propiedad;
            Ayuda = ayuda;
            Posicion = posicion;
        }

        public string RenderEtiqueta()
        {
            return RenderEtiqueta(IdHtml, IdHtml, Etiqueta);
        }

        public void BlanquearListaDeIds()
        {
            _ListaDeIdsAsignados.Clear();
        }

        public abstract string RenderControl();

        public virtual string RenderAtributos(string atributos = "")
        {
            atributos += $@"tipo=¨{Tipo.Render()}¨
                            propiedad=¨{Propiedad.ToLower()}¨ ";
            return atributos;
        }

        public void CambiarAtributos(string etiqueta, string propiedad, string ayuda)
        {
            Id = $"{((ControlHtml)Padre).Id}_{Tipo.Render()}_{propiedad}";
            Propiedad = propiedad;
            CambiarEtiqueta(etiqueta, ayuda);
        }

        public void CambiarPropiedad(string propiedad)
        {
            Propiedad = propiedad;
        }

        public void CambiarEtiqueta(string etiqueta, string ayuda)
        {
            Etiqueta = etiqueta;
            if (!ayuda.IsNullOrEmpty())
                Ayuda = ayuda;
        }

        public static string RenderEtiqueta(string idControl, string idEtiqueta, string etiqueta, Dictionary<string, string> otrosAtributos = null, Dictionary<string, string> otrosAtributosDelContenedor = null, bool excluirFor = false)
        {
            etiqueta = etiqueta == "_" ? " " : etiqueta;
            if (etiqueta.IsNullOrEmpty()) excluirFor = true;
            var clases = otrosAtributos.LeerCadena("clases", "");
            var ayuda = otrosAtributos.LeerCadena("ayuda", etiqueta);
            var htmlEtiqueta = $@"<div id='etiqueta-{idEtiqueta}-contenedor' name='contenedor-etiqueta' 
                                       class='{enumCssControles.ContenedorEtiqueta.Render()}'
                                       [estilo_contenedor]>
                                   <label id='etiqueta-{idEtiqueta}' { ( excluirFor ? "": $"for='{idControl}'") }
                                          class='{enumCssControles.Etiqueta.Render()}{(clases.IsNullOrEmpty() ? "" : $" {clases}")}' 
                                          title='{ayuda}'
                                          [estilo]>
                                     {etiqueta}
                                   </label>
                                 </div>";

            if (otrosAtributos == null)
                otrosAtributos = new Dictionary<string, string>();

            if (otrosAtributosDelContenedor == null)
                otrosAtributosDelContenedor = new Dictionary<string, string>();

            htmlEtiqueta = htmlEtiqueta.Replace("[estilo_contenedor]", otrosAtributosDelContenedor.ContainsKey("estilo_contenedor") ? otrosAtributosDelContenedor["estilo_contenedor"] : "");

            htmlEtiqueta = htmlEtiqueta.Replace("[estilo]", otrosAtributos.ContainsKey("estilo") ? otrosAtributos["estilo"] + Environment.NewLine : "");

            return htmlEtiqueta;
        }

        public static string RenderListaConEtiquetaEncima(string IdHtml, Type elemetoDto, string mostrarExpresion, string propiedad, string etiqueta, string controlador, string onChange)
        {
            var etiquetaHtml = RenderEtiqueta(IdHtml, $"{IdHtml}_lista", etiqueta, excluirFor: true);

            var valores = new Dictionary<string, object>();

            valores["IdHtmlContenedor"] = $"{IdHtml}_contenedor_lista";
            valores["IdHtml"] = $"{IdHtml}_lista";
            valores["Tipo"] = enumTipoControl.ListaDeElemento.Render();
            valores["CssContenedor"] = enumCssControles.ContenedorListaDeElementos.Render();
            valores["Css"] = enumCssControles.ListaDeElementos.Render();
            valores["ClaseElemento"] = elemetoDto.Name;
            valores["MostrarExpresion"] = mostrarExpresion;
            valores["Controlador"] = controlador;
            valores["Propiedad"] = propiedad;
            valores["OnChange"] = onChange;

            var listaHtml = PlantillasHtml.Render(PlantillasHtml.listaDeElementos, valores).Replace("style='[Estilos]'", "");
            return etiquetaHtml + Environment.NewLine + listaHtml;
        }

        public string RenderCheck(string idHtml, string propiedadHtml, bool chequeado, string etiqueta, string accion,
            enumCssFiltro? css = null,
            string otraClaseDelContenedor = null)
        {
            var a = AtributosHtml.AtributosComunes($"div-{idHtml}", idHtml, propiedadHtml, enumTipoControl.Check, Ayuda);

            Dictionary<string, object> valores = AtributosHtmlExtension.MapearComunes(a);
            valores["CssContenedor"] = Css.Render(enumCssFiltro.ContenedorCheck) + $"{(otraClaseDelContenedor.IsNullOrEmpty() ? "" : " " + otraClaseDelContenedor)}";
            valores["Checked"] = chequeado ? "true" : "false";
            valores["Etiqueta"] = etiqueta;
            valores["Accion"] = accion;
            if (css != null) 
                valores["Css"] = ((enumCssFiltro)css).Render();

            return PlantillasHtml.Render(PlantillasHtml.checkFlt, valores)
                .Replace("class=¨[CssContenedor]¨", "")
                .Replace("class=¨[Css]¨", "")
                .Replace("filtrar-por-false=¨[FiltrarPorFalse]¨", "");

        }

        public string RenderCheckFiltroOnOff(string idHtml, string propiedadHtml, bool chequeado, string etiqueta, string accion, string otraClaseDelContenedor = null)
        {
            var a = AtributosHtml.AtributosComunes($"div-{idHtml}", idHtml, propiedadHtml, enumTipoControl.Check, Ayuda);

            Dictionary<string, object> valores = AtributosHtmlExtension.MapearComunes(a);
            valores["Checked"] = chequeado ? "true" : "false";
            valores["Etiqueta"] = etiqueta;
            valores["Accion"] = accion;
           
            return PlantillasHtml.Render(PlantillasHtml.checkFiltroOnOff, valores)
                .Replace("class=¨[CssContenedor]¨", "")
                .Replace("filtrar-por-false=¨[FiltrarPorFalse]¨", "");

        }

        public string RenderCheckFormulario(string idHtml, string propiedadHtml, bool chequeado, string etiqueta, string accion, string css)
        {
            var a = AtributosHtml.AtributosComunes($"div-{idHtml}", idHtml, propiedadHtml, enumTipoControl.Check, Ayuda);

            Dictionary<string, object> valores = AtributosHtmlExtension.MapearComunes(a);
            valores["Checked"] = chequeado ? "true" : "false";
            valores["Etiqueta"] = etiqueta;
            valores["Accion"] = accion;
            valores["Css"] = css;

            return PlantillasHtml.Render(PlantillasHtml.checkFormulario, valores)
                .Replace("class=¨[CssContenedor]¨", "")
                .Replace("class=¨[Css]¨", "")
                .Replace("filtrar-por-false=¨[FiltrarPorFalse]¨", "");

        }

        public static string RenderDivConEtiquetaParaLinks(string idHtml, string etiqueta, Dictionary<string, string> otrosAtributosEtiqueta = null, Dictionary<string, string> otrosAtributosDelContenedor = null)
        {
            var html = $@"<div id='{idHtml}' name='contenedor-control' class='{enumCssControles.ContenedorEtiqueta.Render()}'>
                            {RenderEtiqueta(idHtml, idHtml, etiqueta, otrosAtributosEtiqueta, otrosAtributosDelContenedor, excluirFor: true)}
                            <div id='{idHtml}_ref' class='{enumCssControles.ContenedorReferencias.Render()}'>
                            </div>
                         </div>";

            return html;
        }

        public static string RenderTextArea(string idHtml, string etiqueta, string propiedad, string ayuda, Dictionary<string, string> otrosAtributosTextArea = null)
        {

            var html = @$"<div id=¨div-{idHtml}¨ name=¨contenedor-control¨ class=¨{enumCssControles.ContenedorAreaDeTexto.Render()}¨>
                           <textarea id=¨{idHtml}¨
                                     type=¨text¨ 
                                     propiedad=¨{propiedad}¨ 
                                     class=¨{enumCssControles.AreaDeTexto.Render()}¨ 
                                     tipo=¨{enumTipoControl.AreaDeTexto.Render()}¨
                                     placeholder ='{ayuda}'
                                     [ValorPorDefecto][LongitudMaxima][estilo][readOnly][obligatorio][onBlur][onFocus]>
                            </textarea>
                          </div>";

            if (otrosAtributosTextArea == null)
                otrosAtributosTextArea = new Dictionary<string, string>();


            string alto = $"calc({(double)(1.5 * 5)}em + .75rem + 2px);".Replace(",", ".");
            otrosAtributosTextArea["estilo"] = $"style='height: {alto};'";


            html = html.Replace("[onFocus]", otrosAtributosTextArea.ContainsKey("onFocus") ? otrosAtributosTextArea["onFocus"] + Environment.NewLine : "");
            html = html.Replace("[onBlur]", otrosAtributosTextArea.ContainsKey("onBlur") ? otrosAtributosTextArea["onBlur"] + Environment.NewLine : "");
            html = html.Replace("[onKeyPress]", otrosAtributosTextArea.ContainsKey("onKeyPress") ? otrosAtributosTextArea["onKeyPress"] + Environment.NewLine : "");
            html = html.Replace("[estilo]", otrosAtributosTextArea.ContainsKey("estilo") ? otrosAtributosTextArea["estilo"] + Environment.NewLine : "");
            html = html.Replace("[readOnly]", otrosAtributosTextArea.ContainsKey("readOnly") ? otrosAtributosTextArea["readOnly"] + Environment.NewLine : "");
            html = html.Replace("[obligatorio]", otrosAtributosTextArea.ContainsKey("obligatorio") ? otrosAtributosTextArea["obligatorio"] + Environment.NewLine : "");
            html = html.Replace("[LongitudMaxima]", otrosAtributosTextArea.ContainsKey("LongitudMaxima") ? otrosAtributosTextArea["LongitudMaxima"] + Environment.NewLine : "");

            var remplazo = otrosAtributosTextArea.ContainsKey("valorPorDefecto") && !otrosAtributosTextArea["valorPorDefecto"].ToString().IsNullOrEmpty()
                ? $"valorPorDefecto=¨{otrosAtributosTextArea["valorPorDefecto"]}¨{Environment.NewLine}value=¨{otrosAtributosTextArea["valorPorDefecto"]}¨"
                : "";
            html = html.Replace("[ValorPorDefecto]", remplazo);

            return html;
        }

        public static string RenderEditor(string idHtml, string propiedad, string ayuda, Dictionary<string, string> otrosAtributos)
        {
            var htmlEditor = $@"<div id=¨{idHtml}_contenedor¨ name=¨contenedor-control¨ class=¨{enumCssControles.ContenedorEditor.Render()}¨>
                                     <input id=¨{idHtml}¨
                                         type=¨text¨ 
                                         propiedad=¨{propiedad}¨ 
                                         class=¨{enumCssControles.Editor.Render()}¨ 
                                         tipo=¨{enumTipoControl.Editor.Render()}¨
                                         placeholder =¨{ayuda}¨
                                         [ValorPorDefecto]
                                         [LongitudMaxima]
                                         [estilo]
                                         [readOnly]
                                         [obligatorio]
                                         [onKeyPress]
                                         [onBlur]
                                         [onFocus]>
                                     </input>
                                </div>";

            if (otrosAtributos == null)
                otrosAtributos = new Dictionary<string, string>();

            htmlEditor = htmlEditor.Replace("[onFocus]", otrosAtributos.ContainsKey("onFocus") ? otrosAtributos["onFocus"] + Environment.NewLine : "");
            htmlEditor = htmlEditor.Replace("[onBlur]", otrosAtributos.ContainsKey("onBlur") ? otrosAtributos["onBlur"] + Environment.NewLine : "");
            htmlEditor = htmlEditor.Replace("[onKeyPress]", otrosAtributos.ContainsKey("onKeyPress") ? otrosAtributos["onKeyPress"] + Environment.NewLine : "");
            htmlEditor = htmlEditor.Replace("[estilo]", otrosAtributos.ContainsKey("estilo") ? otrosAtributos["estilo"] + Environment.NewLine : "");
            htmlEditor = htmlEditor.Replace("[readOnly]", otrosAtributos.ContainsKey("readOnly") ? otrosAtributos["readOnly"] + Environment.NewLine : "");
            htmlEditor = htmlEditor.Replace("[obligatorio]", otrosAtributos.ContainsKey("obligatorio") ? otrosAtributos["obligatorio"] + Environment.NewLine : "");
            htmlEditor = htmlEditor.Replace("[LongitudMaxima]", otrosAtributos.ContainsKey("LongitudMaxima") ? otrosAtributos["LongitudMaxima"] + Environment.NewLine : "");

            var remplazo = otrosAtributos.ContainsKey("valorPorDefecto") && !otrosAtributos["valorPorDefecto"].ToString().IsNullOrEmpty()
                ? $"valorPorDefecto=¨{otrosAtributos["valorPorDefecto"]}¨{Environment.NewLine}value=¨{otrosAtributos["valorPorDefecto"]}¨"
                : "";
            htmlEditor = htmlEditor.Replace("[ValorPorDefecto]", remplazo);

            return htmlEditor;
        }

        public static string RenderEditorConEtiquetaEncima(string idHtml, string etiqueta, string propiedad, string ayuda, Dictionary<string, string> otrosAtributosEditor = null, Dictionary<string, string> otrosAtributosEtiqueta = null)
        {
            if (otrosAtributosEditor == null)
                otrosAtributosEditor = new Dictionary<string, string>();

            var htmlEtiqueta = RenderEtiqueta(idHtml, idHtml, etiqueta, otrosAtributosEtiqueta);
            var htmlEditor = RenderEditor(idHtml, propiedad, ayuda, otrosAtributosEditor);

            return htmlEtiqueta + htmlEditor;
        }

        public static string RenderEditorConEtiquetaIzquierda(string idHtml, string etiqueta, string propiedad, string ayuda, Dictionary<string, string> otrosAtributosEditor = null, Dictionary<string, string> otrosAtributosEtiqueta = null)
        {
            var html = @$"<div id=¨div-{idHtml}¨ name=¨contenedor-control¨ class=¨{enumCssControles.ContenedorEditorConEtiquetaIzquierda.Render()}¨>
                           EtiquetaIzq
                           Editor
                          </div>";
            html = html.Replace("EtiquetaIzq", RenderEtiqueta(idHtml, idHtml, etiqueta, otrosAtributosEtiqueta));
            html = html.Replace("Editor", RenderEditor(idHtml, propiedad, ayuda, otrosAtributosEditor));
            return html;
        }


        internal static string RenderizarModal(enumTipoDeModal tipoModal
            , string idHtml
            , string controlador
            , string tituloH2
            , string cuerpo
            , string idOpcion
            , string opcion
            , string accion
            , string cerrar
            , string navegador
            , enumCssOpcionMenu claseBoton
            , enumModoDeAccesoDeDatos permisosNecesarios
            , Dictionary<string, object> otrosAtributos = null)
        {
            controlador = controlador.Replace(ltrEndPoint.Controller, "");
            var AtributosHtml = "";
            if (otrosAtributos != null)
                foreach (var k in otrosAtributos.Keys)
                {
                    if (otrosAtributos[k] == null || otrosAtributos[k].ToString().IsNullOrEmpty())
                        continue;
                    AtributosHtml = $"{AtributosHtml} {k}=¨{otrosAtributos[k]}¨";
                }

            var tituloOpcionCerrar = otrosAtributos.LeerValor<string>(EventosModal.TituloOpcionCerrar, null);
            var htmlOpcion = "";
            if (!opcion.IsNullOrEmpty() && !accion.IsNullOrEmpty())
            {
                htmlOpcion = $@"<input id=¨{idOpcion}¨ 
                                       type=¨button¨ 
                                       tipo=¨{enumTipoControl.Opcion.Render()}¨
                                       clase=¨{Css.Render(claseBoton)}¨ 
                                       class=¨{Css.Render(enumCssOpcionMenu.BotonPorDefecto)}¨ 
                                       permisos-necesarios=¨{permisosNecesarios.Render()}¨ 
                                       value=¨{opcion}¨ 
                                       onclick=¨{accion}¨ />";
            }

            var htmlCerrar = $@"<input id=¨{idHtml}-cerrar¨ 
                                       type=¨button¨ 
                                       tipo=¨{enumTipoControl.Opcion.Render()}¨
                                       clase=¨{Css.Render(enumCssOpcionMenu.Basico)}¨ 
                                       permisos-necesarios=¨{enumModoDeAccesoDeDatos.SinPermiso.Render()}¨ 
                                       value=¨{(tituloOpcionCerrar.IsNullOrEmpty()?"Cerrar":tituloOpcionCerrar)}¨
                                       onclick=¨{cerrar}¨ />";

            var htmlLadoIzquierdoMenuPie = enumTipoDeModal.ModalDeTotales == tipoModal
            ? $"<div><input id='{idHtml}_{nameof(ITotalesDto.Procesados)}' class='{enumCssControles.Editor.Render()} {enumCssControles.InfoDeTotales.Render()}' propiedad='{nameof(ITotalesDto.Procesados)}' tipo='editor' readonly disabled></input></div>"
            : navegador;
            //todo: ver como meto el título, para ello analizar donde se usa esta css contenido-modal
            var htmlModal = $@" <!--  *******************  modal de {idHtml} ******************* -->
                                <div id=¨{idHtml}¨ class=¨contenedor-modal¨ controlador=¨{controlador}¨ tipoModal = ¨{tipoModal.Render()}¨ {AtributosHtml}>
                              		<div id=¨{idHtml}_contenido¨ class=¨{enumCssModal.ContenidoModal.Render()}¨>
                              		    <div id=¨{idHtml}_cabecera¨ class=¨{enumCssModal.ContenidoCabecera.Render()}¨>
                              		    	<h2></h2>
                                        </div>
                              		    <div id=¨{idHtml}_cuerpo¨ class=¨{enumCssModal.ContenidoCuerpo.Render()}¨>
                                        {cuerpo}
                                        </div>
                                        <div id=¨{idHtml}_pie¨ class=¨{enumCssModal.ContenidoPie.Render()}{(enumTipoDeModal.ModalDeTotales == tipoModal? " " + enumCssModal.PieDeTotales.Render() : "")}¨>
                                          <div id=¨{idHtml}_menu¨ class=¨contenido-modal-pie-menu¨>
                                           {htmlOpcion}
                                           {htmlCerrar}
                                          </div>
                                           {htmlLadoIzquierdoMenuPie}
                                        </div>
                                   </div>
                              </div>
                              <!--  *******************  fin de la modal ******************* -->
                              ";

            return htmlModal;
        }


        public static string RenderAtributos(string propiedad, string idHtml, enumTipoControl tipo, string clase, string ayuda, string otrosAtributos = "")
        {
            var atributos = $@"id=¨{idHtml}¨ {otrosAtributos}
                            tipo=¨{tipo.Render()}¨
                            class=¨{clase}¨
                            title=¨{ayuda}¨
                            propiedad=¨{propiedad}¨";
            return atributos;
        }

        public static string RenderEtiquetaParaSeleccionarUnArchivo(string idHtml, string etiqueta)
        {
            var hiperLink = $@"
                             <button 
                                 type=¨button¨
                                 class=¨btn-pegar-portapapeles¨
                                 title=¨Subir desde portapapeles¨
                                 onclick=¨ApiDeArchivos.PegarPortaPapeles(event, 'controlador', 'idCanvas', 'idImagen', 'idInputNombreArchivo', 'idInputIdArchivo')¨
                               >
                                 <img src=¨{CacheDeVariable.Uri_SubirDelPortaPapeles}¨ alt=¨Icono subir portapapeles¨>
                             </button>

                           <a id=¨{idHtml}.ref¨ 
                              class=¨{Css.Render(enumCssControlesFormulario.SelectorArchivo)} {enumCssControles.Etiqueta.Render()}¨ 
                              href=¨javascript:ApiDeArchivos.SeleccionarArchivo('{idHtml}')¨>
                              {etiqueta}
                           </a>";
            return hiperLink;
        }

        public static string DefinirFuncionPegarArchivo(string html, string controlador, string idHtmlControl)
        {
            var idHtmlImg = $"img-{idHtmlControl}";
            var idHtmlCanva = $"canvas-{idHtmlControl}";
            var idHtmlInfoArchivo = $"nombre-{idHtmlControl}";

            html = html.Replace("controlador", controlador).
                Replace("idCanvas", idHtmlCanva).
                Replace("idImagen", idHtmlImg).
                Replace("idInputNombreArchivo", idHtmlInfoArchivo).
                Replace("idInputIdArchivo", idHtmlControl);

            return html;
        }

        public static string RenderEtiquetaParaSeleccionarUnaImagen(string idHtml, string etiqueta)
        {
            var hiperLink = $@"

                             <button 
                                 type=¨button¨
                                 class=¨btn-pegar-portapapeles¨
                                 title=¨Subir desde portapapeles¨
                                 onclick=¨ApiDeArchivos.PegarPortaPapeles(event, 'controlador', 'idCanvas', 'idImagen', 'idInputNombreArchivo', 'idInputIdArchivo')¨
                               >
                                 <img src=¨{CacheDeVariable.Uri_SubirDelPortaPapeles}¨ alt=¨Icono subir portapapeles¨>
                             </button>

                             <a id=¨{idHtml}.ref¨ 
                                class=¨{Css.Render(enumCssControlesFormulario.SelectorArchivo)} {enumCssControles.Etiqueta.Render()}¨ 
                                en-consulta-ocultar=true 
                                href=¨javascript:ApiDeArchivos.SeleccionarImagen('{idHtml}')¨>
                                {etiqueta}
                             </a>
                             ";
            return hiperLink;
        }


    }




}