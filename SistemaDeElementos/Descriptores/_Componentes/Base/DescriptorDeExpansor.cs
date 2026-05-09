using Gestor.Errores;
using ModeloDeDto;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Seguridad;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilidades;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeExpansor : ControlHtml
    {
        public List<ControlHtml> Controles = new List<ControlHtml>();

        public string PostFijoNombreIdDeLaTabla { get; set; }

        public GridDeRelacion GidDeRelacion { get; set; }

        public ControlHtml CuerpoDelExpansor { get; set; }

        public string IdHtmlGridDeRelacion => GidDeRelacion.IdHtmlContenedor;

        public string IdHtmlModalDeCrearRelacion => $"{Id}-{enumTipoDeModal.ModalDeCrearRelacion}".ToLower();
        public string IdHtmlModalDeCrearVinculo => $"{Id}-{enumTipoDeModal.ModalDeCrearVinculo}".ToLower();
        public string IdHtmlModalDeCrearDetalle => $"{Id}-{enumTipoDeModal.ModalDeCrearDetalle}".ToLower();
        public string IdHtmlModalParaVincular => $"{Id}-{enumTipoDeModal.ModalParaVincular}".ToLower();

        public List<ControlHtml> ControlesDelPie = new List<ControlHtml>();

        public bool MostrarPlegado { get; }

        private bool _renderCabecera = true;
        private bool _renderPie = true;

        public BloqueAnexado Bloque => (BloqueAnexado)Padre;

        private int? _idnegocio = null;
        public int IdNegocio
        {
            get
            {
                if (_idnegocio == null)
                    return Padre is IControlConIdNegocio ? ((IControlConIdNegocio)Padre).IdNegocio : 0;
                return _idnegocio.Value;
            }
            set
            {
                _idnegocio = value;
            }
        }

        public bool EsAuditoria { get; set; }

        public bool EsDetalle { get; set; }


        public bool ElPadreEsEditor => Padre.GetType().FullName.Contains("DescriptorDeEdicion");

        public DescriptorDeExpansor(IControlConIdNegocio padre, string id, string etiqueta, bool mostrarPlegado, string ayuda)
        : base(padre, id, etiqueta, "", ayuda, null)
        {
            MostrarPlegado = mostrarPlegado;
        }

        public override string RenderControl()
        {
            return RenderExpansor();
        }

        public string RenderExpansor()
        {
            var compuesto = GidDeRelacion != null && Controles.Count > 0;


            if (CuerpoDelExpansor != null)
                return RenderExpansorConCuerpo(CuerpoDelExpansor.RenderControl());


            var valores = new Dictionary<string, object>();
            valores["IdHtml"] = IdHtml;
            valores["cssClase"] = Css.Render(enumCssExpansor.Contenedor);
            valores["cssCabecera"] = Css.Render(enumCssExpansor.Cabecera);
            valores["cssCuerpo"] = compuesto ? Css.Render(enumCssExpansor.CuerpoCompuesto) : Css.Render(enumCssExpansor.CuerpoSimple);
            valores["cssCuerpoControles"] = Css.Render(enumCssExpansor.CuerpoDeControles);
            valores["cssCuerpoDetalle"] = Css.Render(enumCssExpansor.CuerpoDeDetalle);
            valores["cssPie"] = Css.Render(enumCssExpansor.Pie);
            valores["RenderCabeceraDelExpansor"] = _renderCabecera ? RenderCabeceraExpansor() : "";
            valores["RenderCuerpoControles"] = RenderControlesDelCuerpoExpansor();
            valores["RenderGridDeDetalle"] = RenderGridDeRelacion();
            valores["RenderPieDelExpansor"] = _renderPie ? RenderPieExpansor() : "";
            valores["MostrarDetalle"] = GidDeRelacion == null ? "style = 'display: none;'" : "";
            valores["MostrarControles"] = Controles.Count == 0 ? "style = 'display: none;'" : "";

            valores["cssCuerpo"] = $"{valores["cssCuerpo"]}{(MostrarPlegado ? $" {enumCssDiv.DivOculto.Render()}" : "")}";
            valores["cssPie"] = $"{valores["cssPie"]}{(MostrarPlegado ? $" {enumCssDiv.DivOculto.Render()}" : "")}";

            var htmlExp = "";

            if (EsDetalle)
            {
                htmlExp = $@"
                  <div id=¨mostrar.{IdHtml}¨ class='{enumCssEdicion.Detalle.Render()}' es-detalle='true'> 
                        <a id=¨mostrar.{IdHtml}.ref¨ 
                           style=¨margin-left: 2px;¨
                           href=¨javascript:Crud.{enumGestorDeEventos.EventosDelMantenimiento}('{eventosDeMnt.OcultarMostrarDetalle}', '{IdHtml}');¨>                           
                            <img id='imagen.{IdHtml}' class='{enumCssEdicion.AbrirImagenDeDetalle.Render()}'> 
                        </a>
                        <label id='titulo.{IdHtml}.ref' class='{enumCssEdicion.TituloDeDetalle.Render()}'>{Etiqueta}</label>  
                        <input id=¨expandir.{IdHtml}.input¨ type=¨hidden¨ value={(MostrarPlegado ? "0" : "1")}> 
                        <div id=¨{IdHtml}¨  class=¨{Css.Render(MostrarPlegado ? enumCssDiv.DivOculto : enumCssDiv.DivVisible)}¨>
                           <div id=¨[IdHtml]-cuerpo-detalle¨ class=¨[cssCuerpoDetalle]¨>
                               [RenderGridDeDetalle]
                           </div>
                           <div id='[IdHtml]-pie' class='{enumCssEdicion.PieDeDetalle.Render()}'>[RenderPieDelExpansor]</div>
                        </div>
                   </div>";

                htmlExp = PlantillasHtml.Render(htmlExp, valores);

            }
            else htmlExp = PlantillasHtml.Render(PlantillasHtml.Expansor, valores);

            return htmlExp;
        }


        private string RenderExpansorConCuerpo(string cuerpoDelExpansor)
        {
            var valores = new Dictionary<string, object>();
            valores["IdHtml"] = IdHtml;
            valores["cssClase"] = Css.Render(enumCssExpansor.Contenedor);
            valores["cssCabecera"] = Css.Render(enumCssExpansor.Cabecera);
            valores["cssPie"] = Css.Render(enumCssExpansor.Pie);
            valores["RenderCabeceraDelExpansor"] = RenderCabeceraExpansor();
            valores["RenderCuerpo"] = cuerpoDelExpansor;
            valores["RenderPieDelExpansor"] = RenderPieExpansor();
            valores["cssCuerpo"] = Css.Render(enumCssExpansor.CuerpoSimple);
            valores["cssCuerpo"] = $"{valores["cssCuerpo"]}{(MostrarPlegado ? $" {enumCssDiv.DivOculto.Render()}" : "")}";
            valores["cssPie"] = $"{valores["cssPie"]}{(MostrarPlegado ? $" {enumCssDiv.DivOculto.Render()}" : "")}";

            return PlantillasHtml.Render(PlantillasHtml.ExpansorSinCuerpo, valores);
        }

        private object RenderGridDeRelacion()
        {
            if (GidDeRelacion != null)
                return GidDeRelacion.RenderGridDeRelacion();
            else
                return "";
        }

        private string RenderPieExpansor()
        {
            var htmlPie = "";
            foreach (ControlHtml control in ControlesDelPie)
            {
                if (control is ModalDeCreacionDeRelacion)
                    continue;
                if (control is ModalDeEditarRelacion)
                    continue;
                if (control is ModalDeCrearVinculo)
                    continue;
                if (control is ModalParaVincular)
                    continue;
                if (control is ModalDeCrearDetalle)
                    continue;

                if (control is Referencia)
                    htmlPie = $"{htmlPie}{Environment.NewLine}{((Referencia)control).RenderReferencia(new List<enumCssControles> { enumCssControles.ReferenciaAlineadaAlFinal })}";
                else
                    htmlPie = $"{htmlPie}{Environment.NewLine}{control.RenderControl()}";
            }

            return htmlPie;
        }

        private string RenderCabeceraExpansor()
        {
            var valores = new Dictionary<string, object>();
            valores["IdHtml"] = IdHtml;
            valores["cssClase"] = Css.Render(enumCssExpansor.Expansor);
            valores["Evento"] = $"Crud.{enumGestorDeEventos.EventosDeExpansores}('{eventosDeExpansor.OcultarMostrarBloque}','expandir.{IdHtml}.input;{IdHtml}-cuerpo')";
            valores["Titulo"] = Etiqueta;
            valores["mostrarPlegado"] = MostrarPlegado ? Literal.Cero : Literal.Uno;

            return PlantillasHtml.Render(PlantillasHtml.CabeceraExpansor, valores);
        }

        private string RenderControlesDelCuerpoExpansor()
        {
            var htmlCuerpoDelExpansor = "";
            foreach (var control in Controles)
            {
                htmlCuerpoDelExpansor = $"{(htmlCuerpoDelExpansor.IsNullOrEmpty() ? "" : htmlCuerpoDelExpansor + Environment.NewLine)}{RenderControlDelCuerpo(control)}";
            }

            return htmlCuerpoDelExpansor;
        }

        private object RenderControlDelCuerpo(ControlHtml control)
        {
            var valores = new Dictionary<string, object>();
            valores["IdHtml"] = control.IdHtml;
            valores["cssClase"] = Css.Render(enumCssExpansor.ContenedorDeControl);

            valores["RenderEtiqueta"] = control.Tipo == enumTipoControl.Opcion
                   ? ""
                   : RenderEtiqueta(control.IdHtml, control.IdHtml, control.Etiqueta);

            valores["RenderControl"] = control.RenderControl();

            return PlantillasHtml.Render(PlantillasHtml.ControlDelExpansor, valores);
        }

        internal void DescriptorDeNavegadorRefParaCrear(string texto, AccionDeGetionarDatosDependientes accion, string paraQueNavegar, bool ocultarEnConsulta = true)
        {
            var referencia = new Referencia(this, $"{Id}-crear", texto, accion, ocultarEnConsulta, paraQueNavegar);
            ControlesDelPie.Add(referencia);
        }

        internal ModalDeCrearDetalle DescriptorDeCrearDetalles(ContextoSe contexto, Type dtoDetalle, string controladorDeDetalle, string tituloModal,
            enumModoDeAccesoDeDatos? permisosNecesarios = null,
            string ayuda = "",
            bool ocultarEnConsulta = true,
            enumNameSpaceTs espacioDeNombre = enumNameSpaceTs.Crud,
            string accionControlador = "")
        {
            var referencia = new Referencia(this, $"{Id}-creardt", tituloModal, Referencia.AbrirModalDeCrearDetalle(IdHtmlModalDeCrearDetalle, espacioDeNombre), ayuda.IsNullOrEmpty() ? tituloModal : ayuda, ocultarEnConsulta, permisosNecesarios);
            ControlesDelPie.Add(referencia);
            var modal = new ModalDeCrearDetalle(contexto, this, dtoDetalle, controladorDeDetalle, referencia, espacioDeNombre);
            modal.AccionControlador = accionControlador;
            ControlesDelPie.Add(modal);
            return modal;
        }

        internal ModalDeCrearVinculo DescriptorDeCrearVinculos(ContextoSe contexto, Type dtoVinculado, string controladorDeRelacion, string tituloModal, string ayuda = "", bool ocultarEnConsulta = true)
        {
            var referencia = new Referencia(this, $"{Id}-crear", tituloModal, Referencia.AbrirModalDeCrearVinculo(IdHtmlModalDeCrearVinculo), ayuda.IsNullOrEmpty() ? tituloModal : ayuda, ocultarEnConsulta);

            ControlesDelPie.Add(referencia);
            var descriptor = new ModalDeCrearVinculo(contexto, this, dtoVinculado, controladorDeRelacion, referencia);
            ControlesDelPie.Add(descriptor);
            return descriptor;
        }

        internal ModalParaVincular DescriptorParaVincular(ContextoSe contexto, int idVinculado, Type dtoVinculado, string controladorDeRelacion, string tituloModal, string ayuda = "", bool ocultarEnConsulta = true)
        {
            var referencia = new Referencia(this, $"{Id}-vincular", tituloModal, Referencia.AbrirModalParaVincular(IdHtmlModalParaVincular), ayuda.IsNullOrEmpty() ? tituloModal : ayuda, ocultarEnConsulta);

            ControlesDelPie.Add(referencia);
            var descriptor = new ModalParaVincular(contexto, this, idVinculado, dtoVinculado, controladorDeRelacion, referencia.Etiqueta);

            ControlesDelPie.Add(descriptor);
            return descriptor;
        }

        internal ModalDeCreacionDeRelacion DescriptorDeCrearRelaciones(ContextoSe contexto, Type dtoDeRelacion, Type controladorDeRelacion, string propiedadRestrictora, string tituloModal, string ayuda = "",
            bool ocultarEnConsulta = true,
            enumNameSpaceTs espacioDeNombre = enumNameSpaceTs.Crud,
            string accionControlador = "")
        {
            if (accionControlador.IsNullOrEmpty()) try
                {
                    controladorDeRelacion.GetMethods().Where(x => x.Name == ltrEndPoint.epCrearRelacion).First();
                }
                catch
                {
                    GestorDeErrores.Emitir($"Debe definir el método '{ltrEndPoint.epCrearRelacion}' en el controlador {controladorDeRelacion.Name}");
                }
            var referencia = new Referencia(this, this.Id + "-mcr", tituloModal, Referencia.AbrirModalDeCrearRelacion(IdHtmlModalDeCrearRelacion, propiedadRestrictora, espacioDeNombre), ayuda.IsNullOrEmpty() ? tituloModal : ayuda, ocultarEnConsulta);
            ControlesDelPie.Add(referencia);

            var propiedadesJson = ApiClasesComunes.ObtenerAtributosJson(dtoDeRelacion, enumRutas.RutaDeJson, ServicioDeCaches.UsaCacheParaRenderizar);
            var propiedades = dtoDeRelacion.GetProperties();
            foreach (var p in propiedades.Where(p => propiedadRestrictora == p.Name))
            {
                IUPropiedadAttribute atributos = ApiDeAtributos.ObtenerAtributos(p, propiedadesJson);
                if (atributos.TipoDeControl != enumTipoControl.RestrictorDeEdicion &&
                    atributos.TipoDeControl != enumTipoControl.ListaDinamica)
                    GestorDeErrores.Emitir($"La propiedad proporcionada {propiedadRestrictora} como restrictor de relación no es correcta");
            }
            var modal = new ModalDeCreacionDeRelacion(contexto, this, dtoDeRelacion, controladorDeRelacion.Name, referencia, espacioDeNombre);
            modal.AccionControlador = accionControlador;
            ControlesDelPie.Add(modal);
            return modal;
        }

        internal ModalDeEditarRelacion DescriptorDeEditarRelaciones(ContextoSe contexto, Type dtoDeRelacion, Type controladorDeRelacion, string tituloModal, bool soloConsulta,
            enumNameSpaceTs espacioDeNombre = enumNameSpaceTs.Crud,
            string accionControlador = "")
        {
            if (accionControlador.IsNullOrEmpty() &&
                !controladorDeRelacion.GetMethods().Where(x => x.Name == ltrEndPoint.epModificarRelacion || x.Name == ltrEndPoint.epModificarRelacionPorPost).Any())
                GestorDeErrores.Emitir($"Debe definir el método '{ltrEndPoint.epModificarRelacion}' en el controlador {controladorDeRelacion.Name}");

            GidDeRelacion.PermitirEditar = true;
            var modal = new ModalDeEditarRelacion(contexto, this, dtoDeRelacion, controladorDeRelacion.Name, tituloModal, soloConsulta, espacioDeNombre);
            modal.AccionControlador = accionControlador;
            ControlesDelPie.Add(modal);
            return modal;
        }

        internal void NavegarA(string texto, string controlador, string crud, bool soloFiltra, string propiedadRestrictora, string idRestrictor, string textoMostrar, string ayuda = "")
        {
            ControlesDelPie.Add(new Referencia(this, this.Id, texto, $@"/{controlador}/{crud}?{(soloFiltra ? "filtros" : "restrictores")}=[{propiedadRestrictora}=[{idRestrictor}]=[{textoMostrar}]]", ayuda, false));
        }

        internal static DescriptorDeExpansor ExpansorDeAuditoria(IControlConIdNegocio padre, enumNameSpaceTs ruta, string renderNegocio)
        {
            var expanDeAuditoria = new DescriptorDeExpansor(padre, $"{padre.Id}-audt", "Auditoría", true, "Información de auditoría")
            {
                EsAuditoria = true,
                EsDetalle = false
            };
            var fechaCreacion = new EditorDeFecha(expanDeAuditoria, "Creado el", nameof(IAuditadoDto.CreadoEl), "fecha de cuando se creó el elemento");
            var fechaModificacion = new EditorDeFecha(expanDeAuditoria, "Modificado el", nameof(IAuditadoDto.ModificadoEl), "fecha de cuando se modificó por última vez");
            fechaCreacion.Editable = false;
            fechaModificacion.Editable = false;

            var creador = new EditorDeTexto(expanDeAuditoria, "Creado por", nameof(IAuditadoDto.Creador), "Quién lo creó");
            var modificador = new EditorDeTexto(expanDeAuditoria, "Modificado por", nameof(IAuditadoDto.Modificador), "Quién lo modificó");
            var mostrarHistorico = new NavegarDesdeEdicion(expanDeAuditoria, "Ver auditoría", "Histórico de modificaciones del registro", ruta.ToString(), $"/Auditoria/CrudDeAuditoria/?origen=edicion&negocio={renderNegocio}", "crud_auditoriadto_mantenimiento");
            creador.Editable = false;
            modificador.Editable = false;

            expanDeAuditoria.Controles.Add(fechaCreacion);
            expanDeAuditoria.Controles.Add(fechaModificacion);
            expanDeAuditoria.Controles.Add(creador);
            expanDeAuditoria.Controles.Add(new DivEnBlanco(expanDeAuditoria, "1"));
            expanDeAuditoria.Controles.Add(modificador);
            expanDeAuditoria.Controles.Add(new DivEnBlanco(expanDeAuditoria, "2"));
            expanDeAuditoria.Controles.Add(mostrarHistorico);
            return expanDeAuditoria;
        }
    }

    public static class ApiDeExpansores
    {
        internal static string Render(this List<DescriptorDeExpansor> expanes, List<AmpliacionDeEdicion> ampliaciones, Dictionary<string, string> ampliacionDetrasDe)
        {
            string html = "";
            foreach (var expan in expanes)
            {
                html = $"{(html.IsNullOrEmpty() ? "" : html + Environment.NewLine)}{expan.RenderExpansor()}";
                if (ampliacionDetrasDe is not null && ampliacionDetrasDe.ContainsValue(expan.Id))
                {
                    var entrada = ampliacionDetrasDe.First(x => x.Value == expan.Id);
                    var ampliacion = ampliaciones.First(a => a.Id == entrada.Key);
                    html = $"{html}{Environment.NewLine}{ampliacion.RenderAmpliacion()}";
                }
            }
            return html;

        }
    }

}
