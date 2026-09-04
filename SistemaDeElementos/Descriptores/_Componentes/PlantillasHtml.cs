using System.Collections.Generic;
using Utilidades;
using ModeloDeDto;

namespace MVCSistemaDeElementos.Descriptores
{
    public static class PlantillasHtml
    {

        private static string atributosComunesDeUnControl = @"id=¨[IdHtml]¨
                                       propiedad=¨[Propiedad]¨ 
                                       class=¨[Css]¨ 
                                       tipo=¨[Tipo]¨
                                       style=¨[Estilos]¨";

        private static string atributosComunesDeUnControlDto = $@"{atributosComunesDeUnControl}
                                       obligatorio=¨[Obligatorio]¨ 
                                       ampliacion=¨[Ampliacion]¨ 
                                       [Readonly]";

        private static string atributosComunesDeUnControlflt =
                                    $@"{atributosComunesDeUnControl}
                                       control-de-filtro=¨S¨";

        private static string a = 
            $@"id=¨[IdHtml]¨
               propiedad=¨[Propiedad]¨ 
               class=¨[Css]¨ 
               tipo=¨[Tipo]¨
               style=¨[Estilos]¨
               control-de-filtro=¨S¨";


        public static string ExpansorSinCuerpo = $@"
        <div id=¨[IdHtml]¨ tipo=¨span-de-controles¨ class=¨[cssClase]¨>
           <div id=¨[IdHtml]-cabecera¨ class=¨[cssCabecera]¨>[RenderCabeceraDelExpansor]</div>
           <div id=¨[IdHtml]-cuerpo¨ class=¨[cssCuerpo]¨>
               [RenderCuerpo]
           </div>
           <div id=¨[IdHtml]-pie¨ class=¨[cssPie]¨>[RenderPieDelExpansor]</div>
        </div>";

        public static string Expansor = $@"
        <div id=¨[IdHtml]¨ tipo=¨span-de-controles¨ class=¨[cssClase]¨>
           <div id=¨[IdHtml]-cabecera¨ class=¨[cssCabecera]¨>[RenderCabeceraDelExpansor]</div>
           <div id=¨[IdHtml]-cuerpo¨ class=¨[cssCuerpo]¨>
              <div id=¨[IdHtml]-cuerpo-detalle¨ class=¨[cssCuerpoDetalle]¨ [MostrarDetalle]>
                 [RenderGridDeDetalle]
              </div>
              <div id=¨[IdHtml]-cuerpo-controles¨ class=¨[cssCuerpoControles]¨ [MostrarControles]>
                 [RenderCuerpoControles]
              </div>
           </div>
           <div id=¨[IdHtml]-pie¨ class=¨[cssPie]¨>[RenderPieDelExpansor]</div>
        </div>";

        public static string CabeceraExpansor = $@"	 
            <a id=¨mostrar.[IdHtml].ref¨
               class=¨[cssClase]¨
               titulo=¨[Titulo]¨
               href=¨javascript:[Evento];
               ¨>
               [Titulo]
            </a>
            <input id=¨expandir.[IdHtml].input¨ type=¨hidden¨ value=¨[mostrarPlegado]¨ />";

        public static string ControlDelExpansor = $@"[RenderEtiqueta][RenderControl]";

        public static string Etiqueta = $@"
        <div id=¨etiqueta-[IdDeControl]-contenedor¨ name=¨contenedor-etiqueta¨ class=¨[CssContenedor]¨>
        <label id=¨etiqueta-[IdDeControl]¨ for=¨[IdDeControl]¨ class=¨[CssEtiqueta]¨>[Etiqueta]</label>
        </div>
        ";

        public static string listaDinamicaFlt = $@"<div id=¨[IdHtmlContenedor]¨ name=¨contenedor-control¨ class=¨[CssContenedor]¨>
                                  <input id=¨[IdHtml]¨
                                         propiedad=¨[Propiedad]¨ 
                                         class=¨[Css]¨ 
                                         tipo=¨[Tipo]¨
                                         style=¨[Estilos]¨
                                         control-de-filtro=¨S¨
                                         clase-elemento=¨[ClaseElemento]¨ 
                                         mostrar-expresion=¨[MostrarExpresion]¨
                                         como-buscar=¨[BuscarPor]¨
                                         criterio-de-filtro=¨[CriterioDeFiltro]¨
                                         restringido-por=¨[RestringidoPor]¨
                                         solo-en-alta=¨[{nameof(IUPropiedadAttribute.SoloEnAlta)}]¨
                                         negocio='[{nameof(ltrParametrosEp.negocio)}]'
                                         propiedad-restrictora=¨[PropiedadRestrictora]¨
                                         contenido-en=¨[ContenidoEn]¨
                                         controlador=¨[Controlador]¨
                                         blanquear-controles-dependientes =¨[BlanquearControlesDependientes]¨
                                         longitud=¨[Longitud]¨ 
                                         aplicar-join=¨[AplicarJoin]¨
                                         cantidad-a-leer=¨[Cantidad]¨ 
                                         placeholder =¨[Placeholder]¨
                                         oninput=¨[OnInput]¨ 
                                         onfocus=¨[OnFocus]¨ 
                                         onblur=¨[OnChange]¨ 
                                         onclick='javascript:ApiListaDinamica.ListaPulsada(this)'
                                         tras-mapear='[TrasMapear]'
                                         ordenar-por=¨[ordenarPor]¨
                                         blanquear-al-salir =¨[BlanquearAlSalir]¨
                                         autocomplete='off'
                                         parametros-para-navegar='[{nameof(IUPropiedadAttribute.ParametrosParaNavegar)}]'
                                         ¨>
                                         </input>

                                         [Navegador]
                             </div>";

        public static string listaDinamicaDto = $@"<div id=¨[IdHtmlContenedor]¨ name=¨contenedor-control¨ class=¨[CssContenedor]¨>
                                  <input id=¨[IdHtml]¨
                                         propiedad=¨[Propiedad]¨ 
                                         class=¨[Css]¨ 
                                         tipo=¨[Tipo]¨
                                         style=¨[Estilos]¨
                                         obligatorio=¨[Obligatorio]¨ 
                                         ampliacion=¨[Ampliacion]¨ 
                                         [Readonly]
                                         clase-elemento=¨[ClaseElemento]¨ 
                                         mostrar-expresion=¨[MostrarExpresion]¨
                                         como-buscar=¨[BuscarPor]¨
                                         criterio-de-filtro=¨[CriterioDeFiltro]¨
                                         restringido-por=¨[RestringidoPor]¨
                                         restrictor-fijo=¨[{nameof(IUPropiedadAttribute.RestrictorFijo)}]¨
                                         otros-parametros-de-filtrado=¨[{nameof(IUPropiedadAttribute.OtrosParametrosDeFiltrado)}]¨
                                         solo-en-alta=¨[{nameof(IUPropiedadAttribute.SoloEnAlta)}]¨
                                         propiedad-restrictora=¨[PropiedadRestrictora]¨
                                         contenido-en=¨[ContenidoEn]¨
                                         controlador=¨[Controlador]¨
                                         blanquear-controles-dependientes =¨[BlanquearControlesDependientes]¨
                                         longitud=¨[Longitud]¨ 
                                         aplicar-join=¨[AplicarJoin]¨
                                         cantidad-a-leer=¨[Cantidad]¨ 
                                         placeholder =¨[Placeholder]¨
                                         oninput=¨[OnInput]¨ 
                                         onfocus=¨[OnFocus]¨ 
                                         onblur=¨[OnChange]¨ 
                                         onclick='javascript:ApiListaDinamica.ListaPulsada(this)'
                                         [{nameof(IUPropiedadAttribute.trasSeleccionar)}]
                                         [{nameof(IUPropiedadAttribute.trasBlanquear)}]
                                         [{nameof(IUPropiedadAttribute.antesDeBuscar)}]
                                         [{nameof(IUPropiedadAttribute.EsAlmacenable)}]
                                         ordenar-por=¨[ordenarPor]¨
                                         guardar-en=¨[GuardarEn]¨ 
                                         [LongitudMaxima]
                                         blanquear-al-salir =¨[BlanquearAlSalir]¨
                                         con-navegador='[ConNavegador]'
                                         parametros-para-navegar='[{nameof(IUPropiedadAttribute.ParametrosParaNavegar)}]'
                                         negocio='[{nameof(ltrParametrosEp.negocio)}]'
                                         autocomplete='off'
                                         ¨>                                            
                                         </input>
                                         [Navegador]
                                  </div>";

        public static string listaDeElementos =
            $@"
              <div id='[IdHtmlContenedor]' name='contenedor-control' class='[CssContenedor]'>
                   <select clase-elemento='[ClaseElemento]' 
                           id='[IdHtml]'
                           propiedad='[Propiedad]' 
                           class='[Css]' 
                           tipo='[Tipo]'
                           style='[Estilos]'
                           controlador='[Controlador]'
                           onchange='[OnChange]'
                           mostrar-expresion='[MostrarExpresion]'  >
                           <option value='0'>Seleccionar ...</option>
                   </select>
               </div>";

        public static string listaDeElementosDto =
            $@"
              <div id='[IdHtmlContenedor]' name='contenedor-control' class='[CssContenedor]' title='[Ayuda]'>
                   <select clase-elemento='[ClaseElemento]' 
                           id='[IdHtml]'
                           propiedad='[Propiedad]' 
                           class='[Css]' 
                           tipo='[Tipo]'
                           style='[Estilos]'
                           controlador='[Controlador]'
                           guardar-en='[GuardarEn]'
                           obligatorio=¨[Obligatorio]¨ 
                           ampliacion=¨[Ampliacion]¨ 
                           contenido-en=¨[ContenidoEn]¨
                           restrictor-fijo='[{nameof(IUPropiedadAttribute.RestrictorFijo)}]'
                           restringido-por='[{nameof(IUPropiedadAttribute.RestringidoPorControl)}]'
                           propiedad-restrictora='[{nameof(IUPropiedadAttribute.PropiedadRestrictora)}]'
                           cargar-bajo-demanda='[{nameof(IUPropiedadAttribute.CargarBajoDemanda)}]'                           
                           [{nameof(IUPropiedadAttribute.OnBlur)}]
                           [{nameof(IUPropiedadAttribute.OnChange)}]
                           [{nameof(IUPropiedadAttribute.TrasCargar)}]
                           [{nameof(IUPropiedadAttribute.AutoPosicionamiento)}]
                           [{nameof(IUPropiedadAttribute.EsAlmacenable)}]
                           [deshabilitada]
                           mostrar-expresion='[MostrarExpresion]'  >
                           <option value='0'>Seleccionar ...</option>
                   </select>
               </div>"; 

        public static string listaDeElementosFlt =
            $@"
              <div id='[IdHtmlContenedor]' name='contenedor-control' class='[CssContenedor]' title='[Ayuda]'>
                   <select clase-elemento='[ClaseElemento]' 
                           id='[{nameof(ControlHtml.IdHtml)}]'
                           propiedad='[{nameof(ControlHtml.Propiedad)}]' 
                           class='[Css]' 
                           tipo='[{nameof(ControlHtml.Tipo)}]'
                           control-de-filtro='S' 
                           style='[Estilos]'
                           controlador='[{nameof(ListaDeElemento<ElementoDto>.Controlador)}]'
                           mostrar-expresion='[{nameof(ListaDeElemento<ElementoDto>.MostrarExpresion)}]'
                           onChange=¨[{nameof(ListaDeElemento<ElementoDto>.OnChange)}]¨
                           id-negocio ='[{nameof(ltrParametrosEp.idNegocio)}]'
                           [{nameof(IUPropiedadAttribute.AutoPosicionamiento)}]>
                           <option value='0'>[{nameof(ControlHtml.Ayuda)}]</option>
                   </select>
               </div>";

        private static string listaDeValores = 
                          $@"<div id=¨[IdHtmlContenedor]¨ name=¨contenedor-control¨ class=¨[CssContenedor]¨>
                                  <select [RestoDeAtributos] >                                          
                                          [opcionesDeLaLista]
                                  </select>
                             </div>";

        public static string listaDeValoresFlt = listaDeValores.Replace("[RestoDeAtributos]", atributosComunesDeUnControlflt);

        public static string listaDeValoresDto =
                          $@"<div id=¨[IdHtmlContenedor]¨ name=¨contenedor-control¨ class=¨[CssContenedor]¨ title='[Ayuda]'>
                                  <select id=¨[IdHtml]¨
                                          propiedad=¨[Propiedad]¨ 
                                          class=¨[Css]¨ 
                                          tipo=¨[Tipo]¨
                                          style=¨[Estilos]¨
                                          guardar-en=¨[GuardarEn]¨ 
                                          obligatorio=¨[Obligatorio]¨ 
                                          ampliacion=¨[Ampliacion]¨ 
                                          [{nameof(IUPropiedadAttribute.OnBlur)}]
                                          [{nameof(IUPropiedadAttribute.OnChange)}]
                                          [{nameof(IUPropiedadAttribute.OnReset)}]
                                          [{nameof(IUPropiedadAttribute.EsAlmacenable)}]
                                          [deshabilitada]>                                          
                                          [opcionesDeLaLista]
                                  </select>
                             </div>";


        public static string opcionNavegar = @$" 
                           <div id=¨[IdHtmlContenedor]¨ name=¨contenedor-control¨ class=¨[CssContenedor]¨ title='[Ayuda]'>
                                    <input id=¨[IdHtml]¨
                                           type=¨button¨
                                           class=¨[Css]¨
                                           tipo=¨[Tipo]¨
                                           clase=¨[claseBoton]¨
                                           permisos-necesarios=¨[PermisosNecesarios]¨
                                           value=¨[Etiqueta]¨
                                           onClick=¨[Accion]¨
                                           [disbled] />
                           </div>";


        public static string controlOculto = @$"
                             <input id=¨[IdHtml]¨
                                         propiedad=¨[Propiedad]¨ 
                                         class=¨[Css]¨ 
                                         tipo=¨[Tipo]¨
                                         type=¨[type]¨
                                         valor-de-defecto=¨[ValorPorDefecto]¨
                                         solo-para-ts=¨[SoloParaTs]¨
                                         [{nameof(IUPropiedadAttribute.OnChange)}]
                                         value=¨[ValorPorDefecto]¨
                                         obligatorio=¨[Obligatorio]¨ 
                                         [{nameof(IUPropiedadAttribute.Formato)}]
                                         ampliacion=¨[Ampliacion]¨ 
                                         [Readonly]>
                                  </input>";

        public static string editorDto = @$"
                           <div id=¨[IdHtmlContenedor]¨ name=¨contenedor-control¨ class=¨[CssContenedor]¨ title='[Ayuda]'>
                                  <input id=¨[IdHtml]¨
                                         propiedad=¨[Propiedad]¨ 
                                         class=¨[Css]¨ 
                                         tipo=¨[Tipo]¨
                                         style=¨[Estilos]¨
                                         type=¨[type]¨
                                         [LongitudMaxima]
                                         placeholder =¨[Placeholder]¨
                                         valor-de-defecto=¨[ValorPorDefecto]¨
                                         permisos-necesarios=¨[{nameof(IUPropiedadAttribute.PermisosNecesarios)}]¨
                                         [{nameof(IUPropiedadAttribute.Formato)}]
                                         [onBlur]
                                         [onKeyPress]
                                         [onfocus]
                                         [{nameof(IUPropiedadAttribute.OnChange)}]
                                         [{nameof(IUPropiedadAttribute.OnPaste)}]
                                         [{nameof(IUPropiedadAttribute.EsAlmacenable)}]
                                         value=¨[ValorPorDefecto]¨
                                         obligatorio=¨[Obligatorio]¨ 
                                         ampliacion=¨[Ampliacion]¨ 
                                         [Readonly]>
                                  </input>
                                  [Navegador]
                           </div>";


        public static string editorFlt = @$" <div id=¨[IdHtmlContenedor]¨ name=¨contenedor-control¨ class=¨[CssContenedor]¨ title='[Ayuda]'>
                                  <input id='[IdHtml]'
                                         propiedad='[Propiedad]' 
                                         control-de-filtro='S'
                                         class='[Css]' 
                                         tipo='[Tipo]'
                                         style='[Estilos]'
                                         como-buscar=¨[BuscarPor]¨
                                         criterio-de-filtro='[CriterioDeFiltrado]'
                                         type='text' [LongitudMaxima]
                                         placeholder ='[Placeholder]'
                                         valor-de-defecto='[ValorPorDefecto]'
                                         [{nameof(IUPropiedadAttribute.Formato)}]
                                         [onBlur]
                                         value='[ValorPorDefecto]'>
                                  </input>
                             </div>";

        public static string restrictorDto = @$" <div id=¨[IdHtmlContenedor]¨ name=¨contenedor-control¨ class=¨[CssContenedor]¨>
                                  <input {atributosComunesDeUnControlDto}
                                         type=¨text¨
                                         propiedad-mostrar=¨[MostrarPropiedad]¨
                                         placeholder =¨[Placeholder]¨
                                         valor-de-defecto=¨[ValorPorDefecto]¨
                                         con-navegador='[ConNavegador]'
                                         criterio-de-filtro='[CriterioDeFiltrado]'
                                         propiedad-restrictora='[{nameof(IUPropiedadAttribute.PropiedadRestrictora)}]'
                                         parametros-para-navegar='[{nameof(IUPropiedadAttribute.ParametrosParaNavegar)}]'
                                         negocio='[{nameof(ltrParametrosEp.negocio)}]'
                                         value=¨¨>
                                  </input>
                                  [Navegador]
                             </div>";
        /* propiedad-restrictora=¨[PropiedadRestrictora]¨*/

        public static string checkFlt = $@"<div id=¨[IdHtmlContenedor]¨ name=¨contenedor-control¨ class=¨[CssContenedor]¨>
                                               <input  id='[IdHtml]'
                                                       type='checkbox' 
                                                       propiedad='[Propiedad]' 
                                                       class='[Css]' 
                                                       tipo='check'
                                                       control-de-filtro='S'
                                                       filtrar-por-false='[FiltrarPorFalse]'
                                                       value ='[Checked]' 
                                                       style='[Estilos]'
                                                       [Readonly]
                                                       [Accion]> 
                                               </input>
                                               <label for=¨[IdHtml]¨>[Etiqueta]</label>
                                          </div>";

        public static string checkFiltroOnOff = $@"<div id=¨[IdHtmlContenedor]¨ name=¨contenedor-control¨ class='contenedor-check-on-off'>
                                               <input  id='[IdHtml]'
                                                       type='checkbox' 
                                                       propiedad='[Propiedad]' 
                                                       class='check-on-off' 
                                                       tipo='check'
                                                       control-de-filtro='S'
                                                       filtrar-por-false='[FiltrarPorFalse]'
                                                       value ='[Checked]' 
                                                       style='[Estilos]'
                                                       [Readonly]
                                                       [Accion]> 
                                               </input>
                                               <label class='check-on-off-label' for=¨[IdHtml]¨></label>
                                               <span>[Etiqueta]</span>
                                          </div>";
        public static string checkDeMostrarColumna = $@"<div id=¨[IdHtmlContenedor]¨ name=¨contenedor-control¨ class=¨[CssContenedor]¨>
                                               <input  id='[IdHtml]'
                                                       type='checkbox' 
                                                       mostrar-columna = 'S'
                                                       columna='[Columna]' 
                                                       class='[Css]' 
                                                       tipo='check'
                                                       control-de-filtro='S'
                                                       filtrar-por-false='[FiltrarPorFalse]'
                                                       value ='[Checked]' 
                                                       style='[Estilos]'
                                                       [Readonly]
                                                       [Accion]> 
                                               </input>
                                               <label for=¨[IdHtml]¨>[Etiqueta]</label>
                                          </div>"; 

        public static string listaDinamicaConCheck = $@"
        <div id='[IdHtmlContenedor]' name='contenedor-control' class='input-group [CssContenedor]' title='[Ayuda]'>    
            <div class='lista-dinamica-mostrar'>
              [ListaDinamica]
            </div>
            <div class='check-mostrar'>
              [CheckDeMostrar]
            </div>
         </div>
        ";

        public static string listaDeValoresConCheckDeMostrar = $@"
        <div id='[IdHtmlContenedor]' name='contenedor-control' class='input-group [CssContenedor]' title='[Ayuda]'>            
            <div class= '[CssContenedorLv]'>
              [ListaDeValores]
            </div>
         </div>
        ";

        /*  <div class='check-mostrar'>
              [CheckDeMostrar]
            </div>
         * */

        public static string EntreImportesConCheckDeMostrar = $@"
        <div id='[IdHtmlContenedor]' name='contenedor-control' class='input-group [CssContenedor]' title='[Ayuda]'>   
              [EtiquetaEntreImportes]
            <div class= '[CssContenedorEi]'>
              [EntreImportes]
            </div>
            <div class='check-mostrar'>
              [CheckDeMostrar]
            </div>
         </div>
        ";

        public static string EntreFechasConCheckDeMostrar = $@"
        <div id='[IdHtmlContenedor]' name='contenedor-control' class='input-group [CssContenedor]' title='[Ayuda]'>   
              [EtiquetaEntreFechas]
            <div class= '[CssContenedorEf]'>
              [EntreFechas]
            </div>
            <div class='check-mostrar'>
              [CheckDeMostrar]
            </div>
         </div>
        ";

        public static string checkDto = $@"<div id=¨[IdHtmlContenedor]¨ name=¨contenedor-control¨ 
                                                class=¨[{nameof(IUPropiedadAttribute.CssDelContenedor)}]¨
                                                title=¨[{nameof(IUPropiedadAttribute.Ayuda)}]¨>
                                               <input id=¨[IdHtml]¨
                                                  type=¨checkbox¨ 
                                                  propiedad=¨[Propiedad]¨ 
                                                  class=¨[Css]¨ 
                                                  tipo=¨[Tipo]¨
                                                  style=¨[Estilos]¨
                                                  valor-de-defecto=¨[ValorPorDefecto]¨
                                                  [Readonly]
                                                  value =¨[Checked]¨ 
                                                  [Checkeado]
                                                  ampliacion=¨[Ampliacion]¨ 
                                                  [{nameof(IUPropiedadAttribute.EsAlmacenable)}]
                                                  [{nameof(IUPropiedadAttribute.OnChange)}]> 
                                               <label for=¨[IdHtml]¨>[Etiqueta]</label>
                                           </div>";

        public static string checkFormulario = $@"<div id=¨[IdHtmlContenedor]¨ name=¨contenedor-control¨ class=¨{enumCssControles.ContenedorCheck.Render()}¨>
                                               <input id=¨[IdHtml]¨
                                                  type=¨checkbox¨ 
                                                  propiedad=¨[Propiedad]¨ 
                                                  class=¨[Css]¨
                                                  tipo='check'
                                                  style=¨[Estilos]¨
                                                  value =¨[Checked]¨ [Accion]> 
                                               <label for=¨[IdHtml]¨>[Etiqueta]</label>
                                           </div>";

        public static string AreaDeTextoDto = $@"
        <div id=¨[IdHtmlContenedor]¨ name=¨contenedor-control¨ class=¨input-group [CssContenedor]¨>
            <textarea {atributosComunesDeUnControlDto} 
                   placeholder =¨[Placeholder]¨
                   valor-de-defecto=¨[ValorPorDefecto]¨
                   [informativa]
                   value=¨¨>
            </textarea>
        </div>
        ";

        public static string filtroEntreRangos = $@"
        <div id='[IdHtmlContenedor]' name='contenedor-control' class='input-group [CssContenedor]' title='[Ayuda]'>
            [etiqueta]
            <div class='rango-desde'>
                 <input {atributosComunesDeUnControlflt} 
                        style=¨cursor: pointer;¨
                        idRangoHasta=¨[IdHtml].hasta¨
                        placeholder =¨[Placeholder]¨
                        [{nameof(IUPropiedadAttribute.ExpresionRegular)}]
                        value=¨¨>
                 </input>
            </div>
            <div class='rango-hasta'>
                 <input id=¨[IdHtml].hasta¨
                        class=¨[Css]¨ 
                        style=¨cursor: pointer;¨ 
                        placeholder =¨[Placeholder]¨
                        [{nameof(IUPropiedadAttribute.ExpresionRegular)}]
                        value=¨¨>
                 </input>
            </div>
         </div>
        ";

        public static string filtroEntreImportes = $@"
        <div id='[IdHtmlContenedor]' name='contenedor-control' class='input-group [CssContenedor]' title='[Ayuda]'>
            [etiqueta]
            <div class='importe-desde'>
                 <input {atributosComunesDeUnControlflt} 
                        style=¨cursor: pointer;¨
                        type=¨number¨
                        idImporteHasta=¨[IdHtml].hasta¨
                        value=¨¨>
                 </input>
            </div>
            <div class='importe-hasta'>
                 <input id=¨[IdHtml].hasta¨
                        class=¨[Css]¨ 
                        style=¨[Estilos]¨style=¨cursor: pointer;¨
                        type=¨number¨
                        value=¨¨>
                 </input>
            </div>
         </div>
        ";

        public static string filtroEntreFechas = $@"
        <div id='[IdHtmlContenedor]' name='contenedor-control' class='input-group [CssContenedor]' title='[Ayuda]'>
            [etiqueta]
            <div class='fecha-desde'>
                 <input {atributosComunesDeUnControlflt} 
                        style=¨cursor: pointer;¨
                        type=¨date¨
                        idHoraDesde=¨[IdHtml].hora¨
                        idFechaHasta=¨[IdHtml].hasta¨
                        idHoraHasta=¨[IdHtml].hora.hasta¨
                        value=¨¨>
                 </input>
                 <input id=¨[IdHtml].hora¨ 
                         class=¨[CssHora]¨ 
                         style=¨cursor: pointer;¨
                         type=¨time¨
                         value=¨¨>
                 </input>
            </div>
            <div class='fecha-hasta'>
                 <input id=¨[IdHtml].hasta¨
                        class=¨[Css]¨ 
                        style=¨[Estilos]¨style=¨cursor: pointer;¨
                        type=¨date¨
                        value=¨¨>
                 </input>
                 <input id=¨[IdHtml].hora.hasta¨ 
                         class=¨[CssHora]¨ 
                         style=¨cursor: pointer;¨
                         type=¨time¨
                         value=¨¨>
                 </input>
            </div>
         </div>
        ";


        public static string filtroDelTipoRelacionado = $@"
        <div id='[IdHtmlContenedor]' name='contenedor-control' class='input-group [CssContenedor]' title='[Ayuda]'>
            [Tipos]
            [Estados]
         </div>
        ";

        public static string filtroDeRelaciones = $@"
        <div id='[IdHtmlContenedor]' name='contenedor-control' class='input-group [CssContenedor]' title='[Ayuda]'>
            [etiqueta]
            [listaValores]
            [listaDinamica]
         </div>
        ";

        public static string filtroDeListaDeValoresConEditor = $@"
        <div id='[IdHtmlContenedor]' name='contenedor-control' class='input-group [CssContenedor] anular-margin-top' title='[Ayuda]'>
            [etiqueta]
            [listaValores]
            [editor]
         </div>
        ";

        public static string filtroDeDireccion = $@"
        <div id='[IdHtmlContenedor]' name='contenedor-control' class='input-group anular-margin-top' title='[Ayuda]'>
           <div class='[CssContenedorEditores]'>
            [etiqueta]
            [edtCalle]
            [edtMunicipio]
            [edtCp]
            [edtZona]
            [edtBarrio]
           </div>
         </div>
        ";

        public static string filtroDePreasientos = $@"
        <div id='[IdHtmlContenedor]' name='contenedor-control' class='input-group anular-margin-top' title='[Ayuda]'>
           <div class='[CssContenedorLista]'>
            [lstCircuito]
           </div>
           <div class='[CssContenedorEditores]'>
            [etiqueta]
            [edtEjercicio]
            [edtReferencia]
           </div>
         </div>
         [filtroEntreImportes]
        ";

        public static string filtroConListas = $@"
        <div id='[IdHtmlContenedor]' name='contenedor-control' class='input-group [CssContenedor]' title='[Ayuda]'>
            [listas]
         </div>
        ";

        public static string selectorDeFechaDto = $@"
        <div id=¨[IdHtmlContenedor]¨ name=¨contenedor-control¨ class=¨input-group [CssContenedor]¨>
            <input {atributosComunesDeUnControlDto} 
                   style=¨cursor: pointer;¨
                   type=¨date¨
                   placeholder =¨[Placeholder]¨
                   valor-de-defecto=¨[ValorPorDefecto]¨
                   {nameof(IUPropiedadAttribute.OnBlur)}=¨[{ValoresPorDefecto.ActualizarFechaHasta}]¨
                   {nameof(IUPropiedadAttribute.OnPaste)}=¨javascript:handlePasteDate(event)¨
                   value=¨¨>
            </input>
        </div>
        ";


        public static string selectorDeFechaHoraDto = $@"
        <div id=¨[IdHtmlContenedor]¨ name=¨contenedor-control¨ class=¨input-group [CssContenedor]¨>
            <input {atributosComunesDeUnControlDto} 
                   style=¨cursor: pointer;¨
                   type=¨date¨
                   idDeLaHora=¨[IdHtml].hora¨
                   placeholder =¨[Placeholder]¨
                   valor-de-defecto=¨[ValorPorDefecto]¨
                   {nameof(IUPropiedadAttribute.OnBlur)}=¨[ActualizarFechaHasta]¨
                   value=''>
            </input>
            <input id=¨[IdHtml].hora¨ 
                    obligatorio=¨[Obligatorio]¨ 
                    [Readonly]
                    class=¨[CssHora]¨ 
                    style=¨cursor: pointer;¨
                    type=¨time¨
                    valor-de-defecto=¨[ValorPorDefecto]¨
                   {nameof(IUPropiedadAttribute.OnBlur)}=¨[ActualizarHoraHasta]¨
                    value=''>
            </input>
            [{nameof(IUPropiedadAttribute.ConAccion)}]
        </div>
        ";

        public static string DivEnBlanco = $@"
        <div id=¨[IdHtmlContenedor]¨ name=¨contenedor-control¨ class=¨[Css]¨>
        </div>
        ";

        public static string BotonSeleccion = $@"
        <div id = ¨[IdHtmlContenedor]¨ class=¨[cssContenedor]¨ title='[Ayuda]'>
           <input id=¨[IdHtml]¨ 
                  type=¨button¨ 
                  class=¨[css]¨ 
                  value=¨[Etiqueta]¨ 
                  [onClick]/>
        </div>
         ";

        public static string Render(string plantilla, Dictionary<string, object> valores)
        {
            foreach (var indice in valores.Keys)
            {
                plantilla = plantilla.Replace($"[{indice}]", valores[indice] == null ? "" : valores[indice].ToString());
            }

            plantilla = plantilla.Replace("style=¨[Estilos]¨", "");

            return plantilla;
        }

    }
}
