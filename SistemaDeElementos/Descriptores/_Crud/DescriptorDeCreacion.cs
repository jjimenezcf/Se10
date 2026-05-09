using System;
using UtilidadesParaIu;
using ModeloDeDto;
using Utilidades;
using System.Collections.Generic;
using GestorDeElementos;
using ServicioDeDatos.Seguridad;
using ModeloDeDto.Negocio;
using ServicioDeDatos;
using MVCSistemaDeElementos.UtilidadesIu;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeCreacion<TElemento> : ControlHtml where TElemento : ElementoDto
    {
        public DescriptorDeCrud<TElemento> Crud => (DescriptorDeCrud<TElemento>)Padre;

        public DescriptorDeMantenimiento<TElemento> Mnt => Crud.Mnt;

        public ZonaDeMenu<TElemento> MenuCreacion { get; private set; }

        public List<IControlHtml> Ampliaciones { get;set; } = new List<IControlHtml>();

        public string htmlDeCreacionEspecifico { get; set; }

        public bool AbrirEnModal { set; get; }

        public List<string> OpcionesMf { get; set; } = new List<string>();
        public int IdNegocio => NegociosDeSe.IdNegocio(Crud.Negocio);

        public DescriptorDeCreacion(DescriptorDeCrud<TElemento> crud, string etiqueta)
        : base(
          padre: crud,
          id: $"{crud.Id}_{enumTipoControl.pnlCreador.Render()}",
          etiqueta: etiqueta,
          propiedad: null,
          ayuda: null,
          posicion: null
        )
        {
            Tipo = enumTipoControl.pnlCreador;
            MenuCreacion = new ZonaDeMenu<TElemento>(creador: this);
            MenuCreacion.AnadirOpcionDeNuevoElemento();
            MenuCreacion.AnadirOpcionDeCerrarCreacion();

            if (Crud.Negocio != enumNegocio.No_Definido)
            {
                Crud.modalesParaPedirDatos.Add(new ModalParaPedirDatos(Crud, typeof(PlantillaDeCreacionDto), eventosDeMf.Comun_GuardarPlantillaCreacion, $"Plantilla de creación de {Crud.Negocio.Plural(true)}"));
                Crud.modalesParaPedirDatos.Add(new ModalParaPedirDatos(Crud, typeof(EliminarPlantillaDto), eventosDeMf.Comun_EliminarPlantillaCreacion, $"Eliminar plantilla {Crud.Negocio.Plural(true)}"));
                DefinirMfIndividual(crud.Negocio, OpcionesMf);
            }
        }

        internal static void DefinirMfIndividual(enumNegocio Negocio, List<string> mfDeCreacion)
        {            
            mfDeCreacion.Add($"<li id='{DescriptorDeCrud<ElementoDto>.menuCreacion}.{eventosDeMf.Comun_GuardarDatosCreacion}' accion-menu='{eventosDeMf.Comun_GuardarDatosCreacion}' {AtributosHtml.Mf(enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Gestor, false)}>Guardar datos por defecto</li>");
            mfDeCreacion.Add($"<li id='{DescriptorDeCrud<ElementoDto>.menuCreacion}.{eventosDeMf.Comun_GuardarPlantillaCreacion}' accion-menu='{eventosDeMf.Comun_GuardarPlantillaCreacion}' {AtributosHtml.Mf(enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Gestor, false)}>Guardar plantilla</li>");
        }

        public string RenderDeCreacion()
        {
            return RenderControl();
        }

        public override string RenderControl()
        {
            if (!Crud.NegocioActivo || Crud.Modo == ModoDescriptor.Consulta || !(bool)ApiDeAtributos.ValorDelAtributo(typeof(TElemento), nameof(IUDtoAttribute.OpcionDeCrear)))
            {
                MenuCreacion.QuitarOpcionDeMenu(eventosDeCreacion.NuevoElemento);
            }

            string htmContenedorCreacion;
            if (AbrirEnModal)
            {
                htmContenedorCreacion = RendelModalDeCreacion();
            }
            else
            {
                var editarTrasCrear = (bool)ApiDeAtributos.ValorDelAtributo(typeof(TElemento), nameof(IUDtoAttribute.EditarTrasCrear));
                htmContenedorCreacion =
                 $@"
                   <div id=¨{IdHtml}¨ 
                         class=¨{enumCssDiv.DivOculto.Render()} {enumCssCreacion.CuerpoDeCrearcion.Render()} {(editarTrasCrear ? enumCssCreacion.EditarTrasCrear.Render() : "")}¨
                         controlador=¨{Crud.Controlador}¨
                         tabla-de-controles='{DescriptorDeTabla.IdHtmlDeTabla(typeof(TElemento).Name, enumModoDeTrabajo.Nuevo, postFijo: "")}'>
                           {RenderContenedorDeCreacionCabecera(MenuCreacion, IdHtml)}
                           {RenderContenedorDeCreacionCuerpo(Padre, typeof(TElemento), IdHtml, Crud.Controlador, RenderOtrosDivDeCreacion())}
                           {RenderContenedorDeCreacionPie(IdHtml, AbrirEnModal, htmlDeCreacionEspecifico, renderCheck: !editarTrasCrear)}
                   </div>
                ";
            }

            return htmContenedorCreacion.Render();
        }

        private string RenderOtrosDivDeCreacion()
        {
            var ampliaciones = "";
            foreach (var ampliacion in Ampliaciones)
            {
              ampliaciones = ampliaciones + Environment.NewLine + ampliacion.RenderControl();
            }

            return ampliaciones;
        }

        private string RendelModalDeCreacion()
        {
            var otrosAtributos = new Dictionary<string, object> { { "tabla-de-controles", DescriptorDeTabla.IdHtmlDeTabla(typeof(TElemento).Name, enumModoDeTrabajo.Nuevo, postFijo: "") } };

            var htmlModal = RenderizarModal(enumTipoDeModal.ModalDeCreacion,
                idHtml: IdHtml
                , controlador: Crud.Controlador
                , tituloH2: ""
                , cuerpo: RenderContenedorDeCreacionCuerpo(Padre, typeof(TElemento), IdHtml, Crud.Controlador) + RenderContenedorDeCreacionPie(IdHtml, AbrirEnModal, htmlDeCreacionEspecifico)
                , idOpcion: $"{IdHtml}-crear"
                , opcion: Crud.NegocioActivo ? "Crear" : ""
                , accion: Crud.NegocioActivo ? "Crud.EventosModalDeCreacion('crear-elemento')" : ""
                , cerrar: "Crud.EventosModalDeCreacion('cerrar-modal')"
                , navegador: htmlRenderOpciones(IdHtml, AbrirEnModal)
                , claseBoton: enumCssOpcionMenu.DeElemento
                , permisosNecesarios: ServicioDeDatos.Seguridad.enumModoDeAccesoDeDatos.Gestor
                , otrosAtributos);

            return htmlModal;
        }

        public string RenderContenedorDeCreacionCabecera(ZonaDeMenu<TElemento> menuCreacion, string idHtml)
        {
            var htmlModal = Crud.Negocio != enumNegocio.No_Definido && Crud.RutaBase != enumNameSpaceTs.Negocio
                ?
                $@"<div id=¨contenedor_creacion_cabecera_{idHtml}¨ class=¨{enumCssEdicion.ContenedorDeEdicionCabecera.Render()}¨>
                                 {menuCreacion.RenderControl()}
                                 <div id=¨{IdHtml}.titulo¨ class=¨{Css.Render(enumCssEdicion.Titulo)}¨>{Etiqueta}</div>
                                 <div id=¨{IdHtml}.flujo¨ class=¨{Css.Render(enumCssEdicion.MenuDelProceso)}¨> </div> 
                                 <div id=¨{IdHtml}.{DescriptorDeCrud<TElemento>.menuCreacion}¨ class=¨{Css.Render(enumCssEdicion.MenuIndividual)}¨ offset-x = 150 menu-flotante='{DescriptorDeCrud<TElemento>.menuCreacion}'> </div> 
                              </div>"
                :
                $@"<div id=¨contenedor_creacion_cabecera_{idHtml}¨ class=¨{enumCssEdicion.ContenedorDeEdicionCabecera.Render()}¨>
                                 {menuCreacion.RenderControl()}
                                 <div id=¨{IdHtml}.titulo¨ class=¨{Css.Render(enumCssEdicion.Titulo)}¨>{Etiqueta}</div>
                                 <div id=¨{IdHtml}.flujo¨ class=¨{Css.Render(enumCssEdicion.MenuDelProceso)}¨> </div> 
                                 <div id=¨{IdHtml}.flotante¨> </div> 
                              </div>";

            return htmlModal;
        }

        public static string RenderContenedorDeCreacionCuerpo(IControlHtml padre, Type dto, string idHtml, string controlador, string ampliacionesEnElCuerpo = null)
        {
            var incluirVisorDeArchivo = false;
            DescriptorDeCrud<TElemento> crud = padre is DescriptorDeCrud<TElemento> ? crud = padre as DescriptorDeCrud<TElemento> : null;
            if (crud is not null)
            {
                var selectores = typeof(TElemento).PropiedadesDelTipo(enumTipoControl.SelectorDeUnArchivo);
                
                if (selectores.Count > 0 && crud.Negocio.UsaTipo())
                {
                    var propiedadesJson = ApiClasesComunes.ObtenerAtributosJson(typeof(TElemento), enumRutas.RutaDeJson, ServicioDeCaches.UsaCacheParaRenderizar);

                    foreach (var selector in selectores)
                    {
                        IUPropiedadAttribute atributos = ApiDeAtributos.ObtenerAtributos(selectores[0], propiedadesJson);
                        if (!atributos.VisibleEnVisorAlCrear)
                            continue; 
                        incluirVisorDeArchivo = atributos.VisibleAlCrear;
                        break;
                    }
                }

            }

            var uri = new UriBuilder(CacheDeVariable.Cfg_UrlBase)
            {
                Path = $"/images/menu/DescargarArchivo.png"
            };

            var idHtmlCuerpoDeCreacion = $"contenedor_creacion_cuerpo_{idHtml}";
            var htmlModal = crud is not null &&  incluirVisorDeArchivo ?
                $@"<div id='{idHtmlCuerpoDeCreacion}' class='{enumCssCreacion.ContenedorDeCreacioCuerpo.Render()} {enumCssCreacion.VisorOculto.Render()}'>
                        <div id=¨{idHtmlCuerpoDeCreacion}_Datos¨ class=¨{enumCssCreacion.ContenedorDeCreacioDatos.Render()}¨>
                                 {DescriptorDeTabla.htmlRenderObjetoVacio(padre, dto, controlador, idHtmlCuerpoDeCreacion, Css.Render(enumCssCreacion.TablaDeCreacion), enumModoDeTrabajo.Nuevo)}
                                 {ampliacionesEnElCuerpo}
                        </div>
                        <div class='splitter'></div>
                        <div id='{idHtmlCuerpoDeCreacion}_Visor' class='{enumCssCreacion.ContenedorDeCreacioVisor.Render()}'>
                          <div class='{enumCssCreacion.VisorDeCreacion.Render()}'></div>
                          <div class='{enumCssCreacion.NavegadorDeCreacion.Render()}'>
                            <div class=""navegacion-imagenes"">
                               <img id='{idHtmlCuerpoDeCreacion}_img_ia_archivo' class='img-ia' alt=""archivo a analizar"" title=""{crud.Mnt.IaTitulo}"" onclick=""Crud.EventosDeEdicion('{crud.Mnt.IaAccion.Descripcion()}')"">
                               <img id='{idHtmlCuerpoDeCreacion}_img_ocr_archivo' class='img-ocr' alt=""archivo Ocr"" title=""Pasar OCR"" onclick=""Crud.EventosDeEdicion('{enumAccionVisorArchivo.PasarOcr.Descripcion()}')"">
                             </div>
                             <input type='text' value='' readonly class='{enumCssCreacion.VisorNombreAnexado.Render()}'>
                          </div>
                        </div>                               
                     </div>"
                :           
                $@"<div id=¨{idHtmlCuerpoDeCreacion}¨ class=¨{enumCssEdicion.ContenedorDeEdicionCuerpo.Render()}¨>
                                 {DescriptorDeTabla.htmlRenderObjetoVacio(padre, dto, controlador, idHtmlCuerpoDeCreacion, Css.Render(enumCssCreacion.TablaDeCreacion), enumModoDeTrabajo.Nuevo)}
                                 {ampliacionesEnElCuerpo}
                   </div>";
            return htmlModal;
        }

        public static string RenderContenedorDeCreacionPie(string idHtml, bool abrirEnModal, string htmlDeCreacionEspecifico, bool renderCheck = true)
        {
            var html = renderCheck
                ? $@"<div id=¨contenedor_creacion_pie_{idHtml}¨ class=¨{Css.Render(enumCssEdicion.ContenedorDeEdicionPie)} ¨>
                       {htmlDeCreacionEspecifico}
                       {(abrirEnModal ? "" : htmlRenderOpciones(idHtml, abrirEnModal))}
                    </div>"
                : $@"<div id=¨contenedor_creacion_pie_{idHtml}¨ class=¨{Css.Render(enumCssEdicion.ContenedorDeEdicionPie)} ¨>
                
                     </div>";
            return html;
        }

        public static string htmlRenderOpciones(string idHtml, bool abrirEnModal)
        {

            var htmdDescriptorControl = $@"<input id=¨{idHtml}-crear-mas¨ type=¨checkbox¨ checked/>
                                           <label for=¨{idHtml}-crear-mas¨>Cerrar tras crear</label>";


            var htmContenedorPie =
                   $@"
                   <div id=¨opciones-{idHtml}¨ class=¨{(!abrirEnModal ? enumCssControles.ContenedorCheck.Render() : Css.Render(enumCssCreacion.ContenedorPieModalOpciones))}¨>
                    {htmdDescriptorControl}
                  </div>
                ";
            return htmContenedorPie;
        }


        public string RenderMenuFlotanteDeCreacion(string idMenu)
        {
            var opciones = "";
            foreach (var o in OpcionesMf) opciones = $"{opciones}{(opciones.IsNullOrEmpty() ? "" : Environment.NewLine)}{o}";
            var htmMenuCreacion = $@"
             <!-- ****************************** menu flotante de edición ***************************************-->
             <ul id='{idMenu}' class='{enumCssMenuFlotante.menuFlotante.Render()} {enumCssMenuFlotante.Blanco.Render()} {enumCssMenuFlotante.SombraBlanca.Render()} {enumCssMenuFlotante.MenuConScroll.Render()}'>
             {opciones}
             </ul>";
            return htmMenuCreacion;
        }

    }
}