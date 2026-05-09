using Gestor.Errores;
using GestorDeElementos;
using ModeloDeDto;
using ModeloDeDto.Negocio;
using ModeloDeDto.Seguridad;
using MVCSistemaDeElementos.Controllers;
using MVCSistemaDeElementos.UtilidadesIu;
using ServicioDeDatos;
using ServicioDeDatos.Seguridad;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilidades;
using UtilidadesParaIu;
using static SistemaDeElementos.Inicializador.enumVistas;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeEdicion<TElemento> : ControlHtml, IControlConIdNegocioConExpansor where TElemento : ElementoDto
    {
        public DescriptorDeCrud<TElemento> Crud => Padre is DescriptorDePaginaDeConsulta ? null : (DescriptorDeCrud<TElemento>)Padre;
        public DescriptorDePaginaDeConsulta Pagina => Padre is DescriptorDePaginaDeConsulta ? (DescriptorDePaginaDeConsulta)Padre : null;
        public DescriptorDeMantenimiento<TElemento> Mnt => Crud?.Mnt;
        public ZonaDeMenu<TElemento> MenuDeEdicion { get; private set; }
        public bool AbrirEnModal { set; get; }

        public List<DescriptorDeExpansor> Expanes { get; set; } = new List<DescriptorDeExpansor>();

        public List<AmpliacionDeEdicion> Ampliaciones = new List<AmpliacionDeEdicion>();

        public Dictionary<string, string> AmpliacionesDetrasDe = new Dictionary<string, string>();

        public ContenedorDeArchivos CtdDeArchivo
        {
            get
            {
                foreach (var e in Expanes)
                    if (e.CuerpoDelExpansor is ContenedorDeArchivos)
                        return (ContenedorDeArchivos)e.CuerpoDelExpansor;
                throw new Exception("Debe definir un contenedor de archivos como cuerpo de un expansor de archivos");
            }
        }

        public List<string> OpcionesMf { get; set; } = new List<string>();

        private enumNegocio _negocio;

        public int IdNegocio => _negocio == enumNegocio.No_Definido ? 0 : NegociosDeSe.IdNegocio(_negocio);

        public ContextoSe Contexto => (Padre is DescriptorDePaginaDeConsulta) ? Pagina.Contexto : Crud.Contexto;

        bool _permiteConsultaConGuid = false;

        public bool PermiteConsultasConGuid
        {
            get
            {
                return _permiteConsultaConGuid;
            }
            set { _permiteConsultaConGuid = value; }
        }

        public DescriptorDeEdicion(DescriptorDeCrud<TElemento> crud, string etiqueta)
        : base(
          padre: crud,
          id: $"{crud.Id}_{enumTipoControl.pnlEditor.Render()}",
          etiqueta: etiqueta,
          propiedad: null,
          ayuda: null,
          posicion: null
        )
        {
            Tipo = enumTipoControl.pnlEditor;
            _negocio = crud.Negocio;
            _permiteConsultaConGuid = _negocio.PermiteConultasConGuid();
            MenuDeEdicion = new ZonaDeMenu<TElemento>(editor: this);
            MenuDeEdicion.AnadirOpcionDeModificarElemento();
            if (typeof(TElemento).ImplementaProcesoDto()) MenuDeEdicion.AnadirOpcionDeIrAHistorial();
            MenuDeEdicion.AnadirOpcionDeCancelarEdicion();

            DefinirMfIndividual(crud.Negocio, OpcionesMf);
        }


        public DescriptorDeEdicion(DescriptorDePaginaDeConsulta contenedor, enumNegocio negocio, string etiqueta)
        : base(
          padre: contenedor,
          id: $"{contenedor.Id}_{enumTipoControl.pnlEditor.Render()}",
          etiqueta: etiqueta,
          propiedad: null,
          ayuda: null,
          posicion: null
        )
        {
            Tipo = enumTipoControl.pnlEditor;
            _negocio = negocio;
            MenuDeEdicion = new ZonaDeMenu<TElemento>(editor: this);
            MenuDeEdicion.AnadirOpcionDeCancelarEdicion();
        }

        internal static void DefinirMfIndividual(enumNegocio Negocio, List<string> mfDeEdicion)
        {
            if (Negocio.UsaBaja())
            {
                mfDeEdicion.Add($"<li id='{DescriptorDeCrud<ElementoDto>.menuEdicion}.{eventosDeMf.DarDeBaja}' accion-menu='{eventosDeMf.DarDeBaja}' {AtributosHtml.Mf(enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Gestor, false)}>Dar de baja</li>");
                mfDeEdicion.Add($"<li id='{DescriptorDeCrud<ElementoDto>.menuEdicion}.{eventosDeMf.DarDeAlta}' accion-menu='{eventosDeMf.DarDeAlta}' {AtributosHtml.Mf(enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Gestor, false)}>Dar de alta</li>");
            }

            if (Negocio.UsaHitos())
                mfDeEdicion.Add($"<li id='{DescriptorDeCrud<ElementoDto>.menuEdicion}.transitar' accion-menu='{eventosDeMf.AbrirTransitar}' " +
                    $"{AtributosHtml.Mf(enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Consultor, false)}>Transitar</li>");

            //if ((bool)ApiDeAtributos.ValorDelAtributo(typeof(TElemento), nameof(IUDtoAttribute.OpcionDeEnviar)) &&
            //    GestorDeCorreos.HayVistaParaMostrarElDto<TElemento>())
            //    mfDeEdicion.Add($"<li id='{DescriptorDeCrud<ElementoDto>.menuEdicion}.{eventosDeMf.AbrirEnviarCorreo}' accion-menu='{eventosDeMf.AbrirEnviarCorreo}' " +
            //        $"{AtributosHtml.Mf(enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Consultor, true)}>Enviar por e-mail</li>");

            if (Negocio.UsaArchivadores())
                mfDeEdicion.Add($"<li id='{DescriptorDeCrud<ElementoDto>.menuEdicion}.archivadores' accion-menu='{eventosDeMf.CrearArchivador}' {AtributosHtml.Mf(enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Consultor, false)}>Añadir archivador</li>");

            if (Negocio.UsaObservaciones())
                mfDeEdicion.Add($"<li id='{DescriptorDeCrud<ElementoDto>.menuEdicion}.observaciones' accion-menu='{eventosDeMf.CrearObservacion}' {AtributosHtml.Mf(enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Consultor, false)}>Añadir observación</li>");

            if (Negocio.UsaAgenda())
                mfDeEdicion.Add($"<li id='{DescriptorDeCrud<ElementoDto>.menuEdicion}.agenda' accion-menu='{eventosDeMf.ModalDeCrearEvento}' {AtributosHtml.Mf(enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Gestor, false)}>Añadir evento</li>");

            if (Negocio.UsaDirecciones())
                mfDeEdicion.Add($"<li id='{DescriptorDeCrud<ElementoDto>.menuEdicion}.direcciones' accion-menu='{eventosDeMf.ModalDeCrearDirecciones}' {AtributosHtml.Mf(enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Gestor, false)}>Añadir dirección</li>");

            if (Negocio != enumNegocio.No_Definido)
                mfDeEdicion.Add($"<li id='{DescriptorDeCrud<ElementoDto>.menuEdicion}.imprimir' accion-menu='{eventosDeMf.Comun_Imprimir}' {AtributosHtml.Mf(enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Consultor, false)}>Imprimir seleccionado</li>");

            if (Negocio.UsaPermisosPorElemento())
            {
                mfDeEdicion.Add("<hr>");
                mfDeEdicion.Add($"<li id='{DescriptorDeCrud<ElementoDto>.menuEdicion}.permisos' accion-menu='{eventosDeMf.Comun_PermisosDeElemento}' {AtributosHtml.Mf(enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Consultor, false)}>Permiso del elemento</li>");
            }
        }

        internal void IncluirMfIndividual(string texto, string evento, enumCssOpcionMenu claseOpcion, enumModoDeAccesoDeDatos permisos)
        =>
        DescriptorDeEdicion<ElementoDto>.IncluirMfIndividual(OpcionesMf, $"<li id='{DescriptorDeCrud<ElementoDto>.menuEdicion}.{evento}' accion-menu='{evento}' {AtributosHtml.Mf(claseOpcion, permisos, false)}>{texto}</li>");


        internal static void IncluirMfIndividual(List<string> opcionesPorElemento, string opcionHtml)
        {
            opcionesPorElemento.Add(opcionHtml);
        }


        internal static void QuitarOpcionDeMf(List<string> opcionesPorElemento, string evento, bool errorSiNoExiste)
        {
            foreach (var opcion in opcionesPorElemento)
            {
                if (opcion.Contains($"accion-menu='{evento}'"))
                {
                    opcionesPorElemento.Remove(opcion);
                    return;
                }
            }

            if (errorSiNoExiste) GestorDeErrores.Emitir($"Se ha solicitado quitar la opción de menú {evento}, y no se ha localizado");
        }



        public string RenderDeEdicion()
        {
            return RenderControl();
        }

        public override string RenderControl()
        {
            if (Padre is DescriptorDePaginaDeConsulta)
                return RenderPaginaDeConsulta();

            var tabla = new DescriptorDeTabla(Padre, typeof(TElemento), enumModoDeTrabajo.Edicion, Crud.Controlador, $"contenedor_edicion_cuerpo_{IdHtml}");
            string htmContenedorEdt;
            if (AbrirEnModal)
            {
                htmContenedorEdt = RendelModal(tabla);
            }
            else
            {
                if (!Crud.NegocioActivo || !Crud.Editor.Editable)
                    MenuDeEdicion.QuitarOpcionDeMenu(eventosDeEdicion.ModificarElemento);

                Dictionary<string, object> propiedadesNoMapeables = ApiDeAtributos.PropiedadesDeDtoNoMapeables(typeof(TElemento));
                htmContenedorEdt =
                 $@"
                   <div id=¨{IdHtml}¨ 
                        class=¨{Css.Render(enumCssDiv.DivOculto)} {Css.Render(enumCssEdicion.CuerpoDeEdicion)}¨
                        controlador=¨{Crud.Controlador}¨
                        solo-consulta='{!Editable || !Mnt.Crud.NegocioActivo}'
                        {propiedadesNoMapeables.First().Key} = '{propiedadesNoMapeables[propiedadesNoMapeables.First().Key]}'>
                           {RenderContenedorDeEdicionCabecera()}
                           {RenderContenedorDeEdicionCuerpo(Padre, typeof(TElemento), IdHtml, Crud.Controlador, Expanes, Ampliaciones, AmpliacionesDetrasDe)}
                           {RenderContenedorDeEdicionPie(IdHtml, tabla.IdHtml, AbrirEnModal, Crud.UsaCompartir, PermiteConsultasConGuid)}
                   </div>
                ";
            }

            return htmContenedorEdt.Render();
        }

        public string RenderPaginaDeConsulta()
        {
            var tabla = new DescriptorDeTabla(Padre, typeof(TElemento), enumModoDeTrabajo.Consulta, Pagina.Controlador, idHtmlContenedor: $"contenedor_edicion_cuerpo_{IdHtml}");
            string htmContenedorEdt;


            Dictionary<string, object> propiedadesNoMapeables = ApiDeAtributos.PropiedadesDeDtoNoMapeables(typeof(TElemento));
            htmContenedorEdt =
             $@"
                   <div id=¨{IdHtml}¨ 
                        class=¨{enumCssEdicion.CuerpoDeEdicion.Render()} {enumCssEdicion.CuerpoDeEdicionSoloConsulta.Render()}¨
                        controlador=¨{Pagina.Controlador}¨
                        id-negocio='{NegociosDeSe.IdNegocio(_negocio)}'
                        negocio='{_negocio.ToNombre()}'
                        solo-consulta='{true}'
                        {propiedadesNoMapeables.First().Key} = '{propiedadesNoMapeables[propiedadesNoMapeables.First().Key]}'>
                        {RenderContenedorDeEdicionCabecera()}
                        {RenderContenedorDePaginaCuerpo(Padre, typeof(TElemento), IdHtml, Pagina.Controlador, Expanes, Ampliaciones, AmpliacionesDetrasDe)}
                        {RenderContenedorDeEdicionPieSoloConsulta(IdHtml, tabla.IdHtml, abrirEnModal: false, UsaCompartir: false)}
                   </div>
                ";

            return htmContenedorEdt.Render();
        }

        private string RendelModal(DescriptorDeTabla tabla)
        {
            Dictionary<string, object> otrosAtributos = ApiDeAtributos.PropiedadesDeDtoNoMapeables(typeof(TElemento));

            var htmlModal = RenderizarModal(enumTipoDeModal.ModalDeEdicion,
                idHtml: IdHtml
                , controlador: Crud.Controlador
                , tituloH2: ""
                , cuerpo: RenderContenedorDeEdicionCuerpo(Padre, typeof(TElemento), IdHtml, Crud.Controlador, null, null) + RenderContenedorDeEdicionPie(IdHtml, tabla.IdHtml, AbrirEnModal)
                , idOpcion: $"{IdHtml}-modificar"
                , opcion: Crud.NegocioActivo ? "Modificar" : ""
                , accion: Crud.NegocioActivo ? $"Crud.{enumGestorDeEventos.EventosModalDeEdicion}('{eventosDeEdicion.ModificarElemento}')" : ""
                , cerrar: $"Crud.{enumGestorDeEventos.EventosModalDeEdicion}('{eventosDeEdicion.CerrarModal}')"
                , navegador: HtmlRenderNavegadorDeSeleccionados(IdHtml, AbrirEnModal, UsaCompartir: false, permiteConsultar: false)
                , claseBoton: enumCssOpcionMenu.DeElemento
                , permisosNecesarios: enumModoDeAccesoDeDatos.Gestor
                , otrosAtributos);

            return htmlModal;
        }

        private static Dictionary<string, object> PropiedadesDeDtoNoMapeables()
        {
            var propiedadesNoMapeablesAlDto = new List<string>();
            var propiedadesJson = ApiClasesComunes.ObtenerAtributosJson(typeof(TElemento), enumRutas.RutaDeJson, ServicioDeCaches.UsaCacheParaRenderizar);
            foreach (var p in typeof(TElemento).GetProperties())
            {
                IUPropiedadAttribute atributos = ApiDeAtributos.ObtenerAtributos(p, propiedadesJson);
                if (!p.Name.Equals(nameof(ElementoDto.Id)) && !atributos.VisibleAlEditar)
                    propiedadesNoMapeablesAlDto.Add(p.Name.ToLower());
            }

            var otrosAtributos = new Dictionary<string, object> { { "propiedades-no-mapeables", $"'{propiedadesNoMapeablesAlDto.ToString("; ")}'" } };
            return otrosAtributos;
        }

        private string RenderContenedorDeEdicionCabecera()
        {
            var htmlModal = Pagina != null
                ? $@"
                   <div id=¨{IdHtml}.cabecera¨ class=¨{enumCssEdicion.ContenedorDeEdicionCabecera.Render()} {enumCssEdicion.ContenedorDeEdicionCabeceraSoloConsulta.Render()}¨>
                      <div id=¨{IdHtml}.titulo¨ class=¨{Css.Render(enumCssEdicion.Titulo)}¨>{Etiqueta}</div>
                      <div id=¨{IdHtml}.flujo¨ class='{enumCssEdicion.AccionesDelPanelDeEdicion.Render()}'>                       
                         <a id=¨mostrar.{IdHtml}.flujo.visor.ref¨ style=¨margin-left: 2px;¨ href=¨javascript:Crud.{enumGestorDeEventos.EventosDeEdicion}('{eventosDeEdicion.MostrarOcultarVisor}');¨> 
                            <img id=¨imagen.{IdHtml}.flujo.visor¨ title='Mostrar o ocultar el visor' class='{enumCssAccionesPanelEdicion.Visor.Descripcion()} {enumCssAccionesPanelEdicion.SinVisor.Descripcion()}'> 
                         </a>
                         <a id=¨mostrar.{IdHtml}.flujo.plegar.ref¨ style=¨margin-left: 2px;¨ href=¨javascript:Crud.{enumGestorDeEventos.EventosDeEdicion}('{eventosDeEdicion.PlegarTodo}');¨> 
                            <img id=¨imagen.{IdHtml}.flujo.plegar¨ title='Plegar todos los apartados'  class='{enumCssAccionesPanelEdicion.Plegar.Descripcion()}'> 
                          </a>
                         <a id=¨mostrar.{IdHtml}.flujo.desplegar.ref¨ style=¨margin-left: 2px;¨ href=¨javascript:Crud.{enumGestorDeEventos.EventosDeEdicion}('{eventosDeEdicion.DesplegarTodo}');¨> 
                            <img id=¨imagen.{IdHtml}.flujo.desplegar¨ title='Desplegar todos los apartados'  class='{enumCssAccionesPanelEdicion.Desplegar.Descripcion()}'> 
                          </a>    
                      </div> 
                   </div>"
                : $@"
                  <div id=¨{IdHtml}.cabecera¨ class=¨{Css.Render(enumCssEdicion.ContenedorDeEdicionCabecera)}¨>
                     {MenuDeEdicion.RenderControl()}
                     <div id=¨{IdHtml}.titulo¨ class=¨{Css.Render(enumCssEdicion.Titulo)}¨>{Etiqueta}</div>
                     <div id=¨{IdHtml}.flujo¨ class='{enumCssEdicion.AccionesDelPanelDeEdicion.Render()}'>                       
                        <a id=¨mostrar.{IdHtml}.flujo.retroceder.ref¨ style=¨margin-left: 2px;¨ href=¨javascript:Crud.{enumGestorDeEventos.EventosDeEdicion}('{eventosDeEdicion.Retroceder}');¨> 
                          <img id=¨imagen.{IdHtml}.flujo.retroceder¨ title='devolver' class='{enumCssAccionesPanelEdicion.Retroceder.Descripcion()}'> 
                        </a>
                        <a id=¨mostrar.{IdHtml}.flujo.avanzar.ref¨ style=¨margin-left: 2px;¨ href=¨javascript:Crud.{enumGestorDeEventos.EventosDeEdicion}('{eventosDeEdicion.Avanzar}');¨> 
                          <img id=¨imagen.{IdHtml}.flujo.avanzar¨ title='transitar' class='{enumCssAccionesPanelEdicion.Avanzar.Descripcion()}'> 
                        </a> 
                        <a id=¨mostrar.{IdHtml}.flujo.visor.ref¨ style=¨margin-left: 2px;¨ href=¨javascript:Crud.{enumGestorDeEventos.EventosDeEdicion}('{eventosDeEdicion.MostrarOcultarVisor}');¨> 
                           <img id=¨imagen.{IdHtml}.flujo.visor¨ title='Mostrar o ocultar el visor' class='{enumCssAccionesPanelEdicion.Visor.Descripcion()} {enumCssAccionesPanelEdicion.SinVisor.Descripcion()}'> 
                        </a>
                        <a id=¨mostrar.{IdHtml}.flujo.plegar.ref¨ style=¨margin-left: 2px;¨ href=¨javascript:Crud.{enumGestorDeEventos.EventosDeEdicion}('{eventosDeEdicion.PlegarTodo}');¨> 
                           <img id=¨imagen.{IdHtml}.flujo.plegar¨ title='Plegar todos los apartados'  class='{enumCssAccionesPanelEdicion.Plegar.Descripcion()}'> 
                         </a>
                        <a id=¨mostrar.{IdHtml}.flujo.desplegar.ref¨ style=¨margin-left: 2px;¨ href=¨javascript:Crud.{enumGestorDeEventos.EventosDeEdicion}('{eventosDeEdicion.DesplegarTodo}');¨> 
                           <img id=¨imagen.{IdHtml}.flujo.desplegar¨ title='Desplegar todos los apartados'  class='{enumCssAccionesPanelEdicion.Desplegar.Descripcion()}'> 
                         </a>    
                     </div> 
                     <div id=¨{IdHtml}.{DescriptorDeCrud<TElemento>.menuEdicion}¨ class=¨{Css.Render(enumCssEdicion.MenuIndividual)}¨ offset-x = 150 menu-flotante='{DescriptorDeCrud<TElemento>.menuEdicion}'> </div> 
                  </div>";
            return htmlModal;
        }

        public string RenderMenuFlotanteDeEdicion(string idMenu)
        {
            var opciones = "";
            foreach (var o in OpcionesMf) opciones = $"{opciones}{(opciones.IsNullOrEmpty() ? "" : Environment.NewLine)}{o}";
            var htmMenuEdicion = $@"
             <!-- ****************************** menu flotante de edición ***************************************-->
             <ul id='{idMenu}' class=¨{enumCssMenuFlotante.menuFlotante.Render()} {enumCssMenuFlotante.Blanco.Render()} {enumCssMenuFlotante.SombraBlanca.Render()}¨>
             {opciones}
             </ul>";
            return htmMenuEdicion;
        }

        public string RenderModalDePermisosPorElemento()
        {
            if (!NegociosDeSe.UsaPermisosPorElemento(Crud.Negocio))
                return "";

            var modal = new ModalDeConsultaDto(Crud.Contexto, this, typeof(PermisosDelElementoDto), nameof(PermisosDelElementoController), "Permisos de un elemento");
            return modal.RendelModalDeConsultaDto();

        }

        public static string RenderContenedorDeEdicionCuerpo(IControlHtml padre, Type dto, string idHtml, string controlador, List<DescriptorDeExpansor> expanes, List<AmpliacionDeEdicion> ampliaciones, Dictionary<string, string> ampliacionDetrasDe = null)
        {
            var idHtmlCuerpoDeEdicion = $"contenedor_edicion_cuerpo_{idHtml}";
            var htmlExpanes = expanes != null ? RenderExpanes(expanes, ampliaciones, ampliacionDetrasDe) : "";
            var htmlAmpliaciones = ampliaciones != null ? RenderAmpliaciones(ampliaciones, ampliacionDetrasDe) : "";

            var htmlDatosPrincipales =
                expanes == null && ampliaciones == null
                ? DescriptorDeTabla.htmlRenderObjetoVacio(padre, dto, controlador, idHtml, enumCssEdicion.TablaDeEdicion.Render(), enumModoDeTrabajo.Edicion)
                : RenderDatosPrincipales(padre, dto, idHtml, controlador);

            DescriptorDeCrud<TElemento> crud = padre is DescriptorDeCrud<TElemento> ? crud = padre as DescriptorDeCrud<TElemento> : null;

            var htmlModal = string.Empty;
            if (expanes != null && crud != null)
            {
                var htmlVisorDeArchivos = RenderVisorDeArchivos(crud, idHtmlCuerpoDeEdicion);
                var htmlVisorDeHistorial = RenderVisorDeHistorial(crud, idHtmlCuerpoDeEdicion);
                htmlModal = $@"<div id='{idHtmlCuerpoDeEdicion}' class='{enumCssEdicion.ContenedorDeEdicionEditor.Render()} {enumCssEdicion.VisorOculto.Render()}'>
                                  <div id='{idHtmlCuerpoDeEdicion}_Datos' class='{enumCssEdicion.ContenedorDeEdicionCuerpoDatos.Render()}'>
                                   {htmlDatosPrincipales}
                                   {htmlAmpliaciones}                                
                                   {htmlExpanes}
                                  </div>
                                  <div class='splitter'></div>
                                  <div class='{enumCssEdicion.ContenedorDelVisorDeArchivoConHistorial.Render()}'>
                                     {htmlVisorDeArchivos}   
                                     {htmlVisorDeHistorial}
                                  </div>
                              </div>";
            }
            else
            {
                htmlModal = $@"<div id=¨{idHtmlCuerpoDeEdicion}¨ class=¨{Css.Render(enumCssEdicion.ContenedorDeEdicionCuerpo)}¨>
                              {htmlDatosPrincipales}
                              {htmlAmpliaciones}                                
                              {htmlExpanes}
                             </div>";
            }
            return htmlModal;
        }


        public static string RenderContenedorDePaginaCuerpo(IControlHtml pagina, Type dto, string idHtml, string controlador, List<DescriptorDeExpansor> expanes, List<AmpliacionDeEdicion> ampliaciones, Dictionary<string, string> ampliacionDetrasDe = null)
        {
            var idHtmlCuerpoDeEdicion = $"contenedor_edicion_cuerpo_{idHtml}";
            var htmlExpanes = expanes != null ? RenderExpanes(expanes, ampliaciones, ampliacionDetrasDe) : "";
            var htmlAmpliaciones = ampliaciones != null ? RenderAmpliaciones(ampliaciones, ampliacionDetrasDe) : "";

            var htmlDatosPrincipales =
                expanes == null && ampliaciones == null
                ? DescriptorDeTabla.htmlRenderObjetoVacio(pagina, dto, controlador, idHtml, enumCssEdicion.TablaDeEdicion.Render(), enumModoDeTrabajo.Edicion)
                : RenderDatosPrincipales(pagina, dto, idHtml, controlador);

            var htmlModal = string.Empty;
            var htmlVisorDeArchivos = RenderVisorDeArchivos(pagina, idHtmlCuerpoDeEdicion);
            htmlModal = $@"<div id='{idHtmlCuerpoDeEdicion}' class='{enumCssEdicion.ContenedorDeEdicionEditor.Render()} {enumCssEdicion.VisorOculto.Render()}'>
                                  <div id='{idHtmlCuerpoDeEdicion}_Datos' class='{enumCssEdicion.ContenedorDeEdicionCuerpoDatos.Render()}'>
                                   {htmlDatosPrincipales}
                                   {htmlAmpliaciones}                                
                                   {htmlExpanes}
                                  </div>
                                  <div class='splitter'></div>
                                  <div class='{enumCssEdicion.ContenedorDelVisorDeArchivoConHistorial.Render()}'>
                                     {htmlVisorDeArchivos}   
                                  </div>
                              </div>";
            return htmlModal;
        }

        private static string RenderVisorDeArchivos(IControlHtml contenedor, string idHtmlCuerpoDeEdicion)
        {
            var crud = contenedor is DescriptorDePaginaDeConsulta ? null : (DescriptorDeCrud<TElemento>)contenedor;

            var uriDescargar = new UriBuilder(CacheDeVariable.Cfg_UrlBase) { Path = $"/images/menu/DescargarArchivo.png" };
            var uriWhatsapp = new UriBuilder(CacheDeVariable.Cfg_UrlBase) { Path = $"/images/whatsapp.png" };
            var uriCompartir = new UriBuilder(CacheDeVariable.Cfg_UrlBase) { Path = $"/images/compartir.png" };
            var htmlVisorDeArchivos = $@"
                                  <div id='{idHtmlCuerpoDeEdicion}_Contenedor_Visor' class='{enumCssEdicion.ContenedorDeEdicionCuerpoVisor.Render()}'>
                                    <div id='{idHtmlCuerpoDeEdicion}_Visor' class='{enumCssEdicion.VisorDeEdicion.Render()}'></div>
                                    <div class='{enumCssEdicion.NavegadorDeEdicionCuerpoVisor.Render()}'>
                                      <button class=""{enumCssControles.DescargarArchivo.Render()}"" title=""descargar archivo"" onclick=""Crud.EventosDeEdicion('descargar-archivo')"">
                                         <img src='{uriDescargar}'>
                                      </button>
                                      <div class=""{enumCssControles.NavegacionImagenes.Render()}"">
                                         <img src=""/images/paginaAnterior.png"" alt=""archivo anterior"" title=""archivo anterior"" onclick=""Crud.EventosDeEdicion('{enumAccionVisorArchivo.Anterior.Descripcion()}')"">
                                         <img src=""/images/paginaSiguiente.png"" alt=""archivo siguiente"" title=""archivo siguiente"" onclick=""Crud.EventosDeEdicion('{enumAccionVisorArchivo.Siguiente.Descripcion()}')"">
                                          {(crud == null ? "" : $@"
                                         <button class=""{enumCssControles.ProcesarConIa.Render()}"" title=""{crud.Mnt.IaTitulo}"" onclick=""Crud.EventosDeEdicion('{crud.Mnt.IaAccion.Descripcion()}')""></button>"
                                          )}
                                         <button class=""{enumCssControles.PasarOcr.Render()}"" title=""Pasar OCR"" onclick=""Crud.EventosDeEdicion('{enumAccionVisorArchivo.PasarOcr.Descripcion()}')""></button>
                                      </div>
                                      <input type=""text"" value="""" readonly class='{enumCssEdicion.VisorNombreAnexado.Render()}'>
                                      <button class=""{enumCssControles.CompartirConWhatsApp.Render()}"" title=""compartir por WhatsApp"" onclick=""Crud.EventosDeEdicion('compartir-con-whatsapp')"">
                                         <img src='{uriWhatsapp}'>
                                      <button class=""{enumCssControles.CompartirConGuid.Render()}"" title=""crear enlace y asignar al portapapeles"" onclick=""Crud.EventosDeEdicion('compartir-con-guid')"">
                                         <img src='{uriCompartir}'>
                                      </button>                                     
                                    </div>                                  
                                 </div>
             ";
            return htmlVisorDeArchivos;
        }

        private static object RenderVisorDeHistorial(DescriptorDeCrud<TElemento> crud, string idHtmlCuerpoDeEdicion)
        {
            var htmlVisorDeHistorial = $@"
                                  <div id='{idHtmlCuerpoDeEdicion}_Contenedor_Historial' class='{enumCssEdicion.ContenedorDeEdicionCuerpoHistorial.Render()} {enumCssControles.DivNoVisible.Render()}'>
                                      <div id='{idHtmlCuerpoDeEdicion}_Visor_Historial' class='{enumCssEdicion.VisorDeHistorial.Render()}'></div>
                                      <div id='{idHtmlCuerpoDeEdicion}_Menu_Historial' class='{enumCssEdicion.MenuDeHistorial.Render()}'></div>
                                  </div>
             ";
            return htmlVisorDeHistorial;
        }

        private static string RenderExpanes(List<DescriptorDeExpansor> expanes, List<AmpliacionDeEdicion> ampliaciones, Dictionary<string, string> ampliacionDetrasDe)
        {
            //string html = "";
            //foreach (var expan in expanes)
            //{
            //    html = $"{(html.IsNullOrEmpty() ? "" : html + Environment.NewLine)}{expan.RenderExpansor()}";
            //    if (ampliacionDetrasDe is not null && ampliacionDetrasDe.ContainsValue(expan.Id))
            //    {
            //        var entrada = ampliacionDetrasDe.First(x => x.Value == expan.Id);
            //        var ampliacion = ampliaciones.First(a => a.Id == entrada.Key);
            //        html = $"{html}{Environment.NewLine}{ampliacion.RenderAmpliacion()}";
            //    }
            //}
            //return html;

            return expanes.Render(ampliaciones, ampliacionDetrasDe);

        }

        private static string RenderAmpliaciones(List<AmpliacionDeEdicion> ampliaciones, Dictionary<string, string> ampliacionDetrasDe)
        {
            string html = "";
            foreach (var ampliacion in ampliaciones)
            {
                if (ampliacionDetrasDe is not null && ampliacionDetrasDe.ContainsKey(ampliacion.Id))
                    continue;
                html = $"{(html.IsNullOrEmpty() ? "" : html + Environment.NewLine)}{ampliacion.RenderAmpliacion()}";
            }
            return html;
        }

        private static string RenderDatosPrincipales(IControlHtml padre, Type dto, string IdHtmlEdicion, string controlador)
        {
            var idHtml = $"{IdHtmlEdicion}-dp";
            return $@"
                  <div id=¨mostrar.{idHtml}¨ class='{enumCssEdicion.DatosPrincipales.Render()}'> 
                        <a id=¨mostrar.{idHtml}.ref¨ 
                           style=¨margin-left: 2px;¨
                           href=¨javascript:Crud.{enumGestorDeEventos.EventosDelMantenimiento}('{eventosDeMnt.OcultarMostrarAmpliacion}', '{idHtml}');¨>                           
                            <img id='imagen.{idHtml}' class='{enumCssEdicion.CerrarImagenDeAmpliacion.Render()}'> 
                        </a>
                        <label id='titulo.{idHtml}.ref' class='{enumCssEdicion.TituloDeAmpliacion.Render()}'>{"Datos principales"}</label>  
                        <input id=¨expandir.{idHtml}.input¨ type=¨hidden¨ value={(true ? Literal.Uno : Literal.Cero)}> 
                        <div id='{idHtml}'  class='padre-{enumCssEdicion.ContenedorDatosPrincipales.Render()} {enumCssDiv.DivVisible.Render()}'>
                          {RenderContenedorDatosPrincipales(padre, dto, idHtml, controlador)}
                        </div>
                   </div>";
        }

        public static string RenderContenedorDatosPrincipales(IControlHtml padre, Type dto, string IdHtml, string controlador)
        {
            var idHtmlDatosPrincipales = $"contenedor_dto_{IdHtml}";
            var htmlModal = $@"<div id='{idHtmlDatosPrincipales}' class='{enumCssEdicion.ContenedorDatosPrincipales.Render()}' controlador = '{controlador}'>
                                 {DescriptorDeTabla.htmlRenderObjetoVacio(padre, dto, controlador, idHtmlDatosPrincipales, enumCssEdicion.TablaDeEdicion.Render(), enumModoDeTrabajo.Edicion)}
                               </div>";
            return htmlModal;
        }

        public static string RenderContenedorDeEdicionPie(string idHtml, string idHtmlDeLaTabla, bool abrirEnModal, bool UsaCompartir = false, bool permiteConsultar = false)
        {
            var htmlModal = $@"<div id=¨contenedor_edicion_pie_{idHtml}¨ class=¨{Css.Render(enumCssEdicion.ContenedorDeEdicionPie)}¨>
                                  {htmlRenderPie(idHtml, idHtmlDeLaTabla)}
                                  {(abrirEnModal ? "" : HtmlRenderNavegadorDeSeleccionados(idHtml, abrirEnModal, UsaCompartir, permiteConsultar))}
                               </div>";
            return htmlModal;
        }

        public static string RenderContenedorDeEdicionPieSoloConsulta(string idHtml, string idHtmlDeLaTabla, bool abrirEnModal, bool UsaCompartir = false)
        {
            var htmlModal = $@"<div id=¨contenedor_edicion_pie_{idHtml}¨ class=¨{enumCssEdicion.ContenedorDeEdicionPie.Render()} {enumCssEdicion.ContenedorDeEdicionPieSoloLectura.Render()}¨>
                                  {htmlRenderPie(idHtml, idHtmlDeLaTabla)}
                               </div>";
            return htmlModal;
        }

        private static string HtmlRenderNavegadorDeSeleccionados(string idHtml, bool abrirEnModal, bool UsaCompartir = false, bool permiteConsultar = false)
        {
            var clase = abrirEnModal ? "contenido-modal-pie-navegador" : "contenedor-pie-navegador";
            var htmlCompartir = "";
            if (UsaCompartir)
            {
                htmlCompartir = $@"<button class='{enumCssControles.CompartirConGuid.Render()}' title='crear enlace de la fila editada y asignar al portapapeles' onclick=¨Crud.EventosDeEdicion('compartir-elemento')¨> 
                                     <img src='{CacheDeVariable.Uri_Compartir}' style='margin-top: -3px;'>
                                   </button> ";
                if (permiteConsultar)
                {
                    var htmlConsultar = $@"
                                   <input type=¨text¨ id=¨dias-para-consultar-por-gid¨ value=¨30¨ title=¨Indique el número de días a compartir el elemento indicado, 0 para anular las comparticiones¨/>
                                   <button class='{enumCssControles.ConsultarConGuid.Render()}' title='crear o anular enlace de consulta y asignar al portapapeles' onclick=¨Crud.EventosDeEdicion('consultar-con-guid')¨> 
                                     <img class='icono-consultar-con-guid' style='margin-top: -3px;'>
                                   </button> ";

                    htmlCompartir = htmlCompartir + Environment.NewLine + htmlConsultar;
                }
            }

            var htmlNavegadorGrid = $@"
                <div id= ¨pie-edicion-{idHtml}-navegador¨ class = ¨{clase}¨>
                        <img src=¨/images/paginaInicial.png¨ alt=¨Primera página¨ title=¨Primer elemento¨ onclick=¨Crud.EventosDeEdicion('mostrar-primero')¨>

                        <input type=¨text¨ 
                               id=¨{idHtml}-posicionador¨ 
                               value=¨0¨ 
                               title=¨Elemento editado¨
                               readonly/>

                        <input type=¨text¨ 
                               id=¨{idHtml}-total-seleccionados¨ 
                               value=¨0¨ 
                               title=¨Elementos seleccionados¨
                               readonly/>

                        <img src=¨/images/paginaAnterior.png¨ alt=¨Primera página¨ title=¨Elemento anterior¨ onclick=¨Crud.EventosDeEdicion('mostrar-anterior')¨>
                        <img src=¨/images/paginaSiguiente.png¨ alt=¨Siguiente página¨ title=¨Elemento siguiente¨ onclick=¨Crud.EventosDeEdicion('mostrar-siguiente')¨>
                        <img src=¨/images/paginaUltima.png¨ alt=¨Última página¨ title=¨Último elemento¨ onclick=¨Crud.EventosDeEdicion('mostrar-ultimo')¨>
                        {htmlCompartir}
                </div>
            ";

            return htmlNavegadorGrid;
        }

        public static string htmlRenderPie(string idHtml, string idHtmlDeLaTabla)
        {
            var htmContenedorPie =
                   $@"
                   <Div id=¨{idHtml}-contenedor-id¨ class=¨{Css.Render(enumCssEdicion.ContenedorId)}¨>
                     {RenderInputId(idHtmlDeLaTabla)}
                  </Div>
                ";
            return htmContenedorPie;
        }

        private static string RenderInputId(string idHtmlDeLaTabla)
        {
            var htmdDescriptorControl = $@"<input id=¨{idHtmlDeLaTabla}_idElemento¨ 
                                             propiedad=¨{nameof(ElementoDto.Id).ToLower()}¨ 
                                             class=¨propiedad propiedad-id¨ 
                                             tipo=¨{enumTipoControl.Editor.Render()}¨ 
                                             type=¨text¨ 
                                             editable =¨N¨ 
                                             readonly
                                             value =¨¨>
                                           </input>";
            return htmdDescriptorControl;
        }

        internal string RenderizarLasModalesDeLosExpansores()
        {
            var htmlModales = "";
            foreach (var expanes in Expanes)
            {
                foreach (ControlHtml control in expanes.ControlesDelPie)
                {
                    if (control is ModalDeCreacionDeRelacion)
                        htmlModales = $"{htmlModales}{Environment.NewLine}{((ModalDeCreacionDeRelacion)control).RendelModalDeCreacionDeRelacion()}";
                    if (control is ModalDeEditarRelacion)
                        htmlModales = $"{htmlModales}{Environment.NewLine}{((ModalDeEditarRelacion)control).RendelModalDeEditarRelacion()}";
                    if (control is ModalDeCrearVinculo)
                        htmlModales = $"{htmlModales}{Environment.NewLine}{((ModalDeCrearVinculo)control).RendelModalDeCrearVinculo()}";
                    if (control is ModalParaVincular)
                        htmlModales = $"{htmlModales}{Environment.NewLine}{((ModalParaVincular)control).RendelModalParaVincular()}";
                    if (control is ModalDeCrearDetalle)
                        htmlModales = $"{htmlModales}{Environment.NewLine}{((ModalDeCrearDetalle)control).RenderControl()}";
                }
            }

            if (typeof(TElemento).Equals(typeof(AuditoriaDto))) return htmlModales;
            if (typeof(TElemento).Equals(typeof(EstadoDto))) return htmlModales;
            if (typeof(TElemento).Equals(typeof(TransicionDto))) return htmlModales;
            if (typeof(TElemento).Equals(typeof(AccionesDeTrnDto))) return htmlModales;

            if (NegociosDeSe.UsaArchivos(Crud.Negocio))
            {
                htmlModales = htmlModales + Environment.NewLine + ContenedorDeArchivos.RenderModalDeVisorDeArchivo(Crud.Contexto, Crud.Negocio, CtdDeArchivo);
                htmlModales = htmlModales + Environment.NewLine + ContenedorDeArchivos.RenderModalDeFirmaDeArchivo(Crud.Contexto, Crud.Negocio, CtdDeArchivo);
                htmlModales = htmlModales + Environment.NewLine + ContenedorDeArchivos.RenderModalDeDatosDeFirma(Crud.Contexto, Crud.Negocio, CtdDeArchivo);
                htmlModales = htmlModales + Environment.NewLine + ContenedorDeArchivos.RenderModalDeBloqueoArchivo(Crud.Contexto, Crud.Negocio, CtdDeArchivo);
                htmlModales = htmlModales + Environment.NewLine + ContenedorDeArchivos.RenderModalDeDesbloqueoArchivo(Crud.Contexto, Crud.Negocio, CtdDeArchivo);
                htmlModales = htmlModales + Environment.NewLine + ContenedorDeArchivos.RenderModalDeSeleccionarDestino(Crud.Contexto, Crud.Negocio, CtdDeArchivo);
                htmlModales = htmlModales + Environment.NewLine + ContenedorDeArchivos.RenderModalDeGenerarZip(Crud.Contexto, Crud.Negocio, CtdDeArchivo);
                htmlModales = htmlModales + Environment.NewLine + ContenedorDeArchivos.RenderModalDeBloqueoMultiple(Crud.Contexto, Crud.Negocio, CtdDeArchivo);
                htmlModales = htmlModales + Environment.NewLine + ContenedorDeArchivos.RenderModalDeDesbloqueoMultiple(Crud.Contexto, Crud.Negocio, CtdDeArchivo);
            }

            return htmlModales;
        }
    }
}