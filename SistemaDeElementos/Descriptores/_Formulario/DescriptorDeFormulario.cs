using System;
using System.Collections.Generic;
using System.Linq;
using Utilidades;
using Gestor.Errores;
using GestoresDeNegocio.Entorno;
using ModeloDeDto;
using ServicioDeDatos;
using ServicioDeDatos.Entorno;
using GestorDeElementos;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeFormulario : ControlHtml
    {
        public ContextoSe Contexto { get; }
        public string Titulo { get; }
        public string Controlador { get; }
        public string Vista { get; }
        protected string Accion { get; set; }
        public int IdVista => VistaDtm.Id;
        public VistaMvcDtm VistaDtm => Contexto.SeleccionarPorPropiedad<VistaMvcDtm>(nameof(VistaMvcDtm.Accion), Accion.IsNullOrEmpty() ? Vista: Accion);

        public string namespaceTs = "Formulario";

        public CabeceraDeFormulario Cabecera { get; set; }
        public CuerpoDeFormulario Cuerpo { get; set; }
        public PieDeFormulario Pie { get; set; }

        public FiltroDelFormulario Filtro {get; set;}

        public enumNameSpaceTs RutaVista { get; set; }
        public UsuarioDtm UsuarioConectado { get; internal set; }
        public GestorDeUsuarios GestorDeUsuario { get; internal set; }
        
        protected Type Dto { get; set; }
        protected enumNegocio Negocio { get; set; }

        public int IdNegocio => Negocio.EsUnNegocio() ? NegociosDeSe.IdNegocio(Negocio) : 0;

        public readonly string MenuFormulario = "menu.formulario";

        public DescriptorDeFormulario(ContextoSe contexto, string id, string titulo, string controlador, enumNameSpaceTs ruta, string vista)
        : base(null, id, titulo, "", "", null)
        {
            Contexto = contexto;
            //Id = idHtml;
            Titulo = titulo;
            Controlador = controlador.Replace(ltrEndPoint.Controller, "");
            Vista = vista;
            RutaVista = ruta;

            Cabecera = new CabeceraDeFormulario(this);
            Cuerpo = new CuerpoDeFormulario(this);
            Pie = new PieDeFormulario(this);
            Filtro = new FiltroDelFormulario(this, "Seleccione tipos a mostrar");
        }


        protected static (BloqueAnexado contenedorJerarquia, BloqueAnexado contenedorDto) DefinirPanelesDeJerarquia(DescriptorDeFormulario formulario, string tituloNodoRaiz, string tituloBloqueJerarquia, string tituloBloqueDto)
        {
            //gestorDeNegocio.TipoDto, IdHtml, Controlador, RutaVista,

            string contenidoJerarquico = $@"
                       <a id='[idHtml].jerarquia.ref' href=javascript:Formulario.{enumGestorDeEventos.EventosDeJerarquia}('{eventosDeJerarquia.Plegar}','{formulario.IdHtml}');>{tituloNodoRaiz}</a>
                       <ul id='[idHtml].jerarquia.ul'>
                   
                       </ul>
                   ";

            //$@"
            //       <div id = '[idHtml].jerarquia' class='{enumCssJerarquia.Contenedor.Render()}'> 
            //           <a id='[idHtml].jerarquia.ref' href=javascript:Formulario.{enumGestorDeEventos.EventosDeJerarquia}('{eventosDeJerarquia.Plegar}','{formulario.IdHtml}');>{tituloNodoRaiz}</a>
            //           <ul id='[idHtml].jerarquia.ul'>
                   
            //           </ul>
            //       </div>
            //       ";

            var idHtmlDeLaTabla = DescriptorDeTabla.IdHtmlDeTabla(formulario.Dto.Name, enumModoDeTrabajo.Jerarquia, postFijo: "");
            var idHtmlDelCuerpoDto = $"{formulario.Cuerpo.IdHtml}-dto";

            Dictionary<string, object> propiedadesNoMapeables = ApiDeAtributos.PropiedadesDeDtoNoMapeables(formulario.Dto);

            var idHtmlPanelDto = $"{formulario.Cuerpo.IdHtml}-dto";
            var idhtmContenedorDto = $"{idHtmlPanelDto}-contenedor";
            var htmlPanelDto = $@"<div id=¨{idHtmlPanelDto}¨ 
                                       class=¨{enumCssFormulario.ContenedorEdicionDto.Render()}¨ 
                                       tabla-dto = ¨{idHtmlDeLaTabla}¨
                                       {propiedadesNoMapeables.First().Key} = '{propiedadesNoMapeables[propiedadesNoMapeables.First().Key]}'
                                       id-negocio = '{formulario.IdNegocio}'
                                       id-vista = '{formulario.IdVista}'
                                    >
                                    <div id=¨{idhtmContenedorDto}¨ grid-area='contenedor-dto' class=¨{enumCssFormulario.DatosDto.Render()}¨ >
                                       {DescriptorDeTabla.htmlRenderObjetoVacio(formulario.Cuerpo, formulario.Dto, formulario.Controlador, idHtmlDelCuerpoDto, Css.Render(enumCssCreacion.TablaDeCreacion), enumModoDeTrabajo.Jerarquia)}
                                       [divExpansores]
                                    </div>
                                       {DescriptorDeEdicion<ElementoDto>.htmlRenderPie(idHtmlDelCuerpoDto, idHtmlDeLaTabla)}
                                  </div>";

            var htmlJerarquia = contenidoJerarquico.Replace("[idHtml]", formulario.IdHtml).Replace("[ruta]", formulario.RutaVista.ToString());

            var contenedorJerarquia = new BloqueAnexado(formulario.Cuerpo, "jerarquia", tituloBloqueJerarquia, ancho: "20%", htmlJerarquia);
            formulario.Cuerpo.BloquesAnexados.Add(contenedorJerarquia);

            var contenedorDto = new BloqueAnexado(formulario.Cuerpo, "detalle", tituloBloqueDto, ancho: "80%", htmlPanelDto);
            formulario.Cuerpo.BloquesAnexados.Add(contenedorDto);

            MenuDePie.OpcionesDelMenuDelPie(formulario.Pie.Menu);

            formulario.Pie.RenderizarMenu = true;
            formulario.Cabecera.RenderizarMenu = false;
            formulario.Filtro.RenderizarFiltro = true;

            return (contenedorJerarquia, contenedorDto);
        }



        public string RenderFormulario()
        {
            return PanelDeControl.RenderPagina(Contexto, RenderControl(), "cuerpo-de-formulario"); 
        }

        public override string RenderControl()
        {
            string formularioHtml;
            try
            {

                var htmlModales = !(this is DescriptorDeTiposDeElemento) && NegociosDeSe.UsaArchivos(Negocio) 
                    ? Environment.NewLine + ContenedorDeArchivos.RenderModalDeVisorDeArchivo(Contexto, Negocio, Cuerpo.CtdDeArchivo)+
                      Environment.NewLine + ContenedorDeArchivos.RenderModalDeFirmaDeArchivo(Contexto, Negocio, Cuerpo.CtdDeArchivo) +
                      Environment.NewLine + ContenedorDeArchivos.RenderModalDeDatosDeFirma(Contexto, Negocio, Cuerpo.CtdDeArchivo) +
                      Environment.NewLine + ContenedorDeArchivos.RenderModalDeBloqueoArchivo(Contexto, Negocio, Cuerpo.CtdDeArchivo) +
                      Environment.NewLine + ContenedorDeArchivos.RenderModalDeDesbloqueoArchivo(Contexto, Negocio, Cuerpo.CtdDeArchivo) +
                      Environment.NewLine + ContenedorDeArchivos.RenderModalDeSeleccionarDestino(Contexto, Negocio, Cuerpo.CtdDeArchivo) +
                      Environment.NewLine + ContenedorDeArchivos.RenderModalDeBloqueoMultiple(Contexto, Negocio, Cuerpo.CtdDeArchivo) +
                      Environment.NewLine + ContenedorDeArchivos.RenderModalDeDesbloqueoMultiple(Contexto, Negocio, Cuerpo.CtdDeArchivo) +
                      Environment.NewLine + ContenedorDeArchivos.RenderModalDeGenerarZip(Contexto, Negocio, Cuerpo.CtdDeArchivo) 
                    : "";

                foreach (var bloques in  this.Cuerpo.BloquesAnexados)
                {
                    htmlModales = htmlModales + Environment.NewLine + bloques.RenderModalesDeCreacion();                    
                }

                //var estilo = Cuerpo.BloquesAnexados.Count>0 ? "style=¨display: flex; overflow-y: hidden;¨" : "";
                formularioHtml = $@"
                         <!--  ******************* cabecera de los datos del formulario ******************* -->
                         <div id=¨{Cabecera.IdHtml}¨ class=¨{enumCssCuerpo.CuerpoCabeceraFormulario.Render()}¨ controlador={Controlador} vista={Vista} datos={Cuerpo.IdHtml} pie={Pie.IdHtml}>
                             {Cabecera.RenderCabecera()}
                         </div>            
                         <!--  *******************   cuerpo del formulario   ******************* -->
                         <div id=¨{Cuerpo.IdHtml}¨ class=¨{enumCssCuerpo.CuerpoDatosFormulario.Render()}¨ >
                           {Cuerpo.RenderCuerpo()}
                         </div>
                         <!--  *******************   pie del formulario     ******************* -->
                         <div id=¨{Pie.IdHtml}¨ class=¨{enumCssCuerpo.CuerpoPie.Render()}¨ style= ¨grid-template-columns: 100%;¨>
                              {Pie.RenderPie()}
                         </div>
                         {Filtro.RenderFiltro()}
                         {htmlModales}                              
                         {RenderMenuFlotanteIndividual(MenuFormulario)}                    
                         ";
            }
            catch (Exception e)
            {
                return $@"
                   <input id=error>{GestorDeErrores.Detalle(e).Replace(Environment.NewLine, "<br>")}</input>
                ";
            }
            finally
            {
                BlanquearListaDeIds();
            }

            return formularioHtml;
        }

        public string RenderMenuFlotanteIndividual(string idMenu)
        {
            if (Cabecera.OpcionesIndividuales.Count == 0) return "";

            var opciones = "";
            foreach (var o in Cabecera.OpcionesIndividuales) 
                opciones = $"{opciones}{(opciones.IsNullOrEmpty() ? "" : Environment.NewLine)}{o}";
            
            var htmMenuIndividual = $@"
             <!-- ****************************** menu individual ***************************************-->
             <ul id='{idMenu}' class=¨{enumCssMenuFlotante.menuFlotante.Render()} {enumCssMenuFlotante.Blanco.Render()} {enumCssMenuFlotante.SombraBlanca.Render()}¨>
             {opciones}
             </ul>";
            return htmMenuIndividual;
        }
    }
}
