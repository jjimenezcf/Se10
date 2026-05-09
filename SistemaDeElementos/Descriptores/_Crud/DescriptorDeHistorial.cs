using System;
using Utilidades;
using ModeloDeDto;
using UtilidadesParaIu;
using System.Collections.Generic;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Elemento;
namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeHistorial<TPadre, TElemento> : ControlHtml 
    where TPadre: ElementoDto
    where TElemento : ElementoDto
    {
        public static string Historial = $"{DescriptorDeCrud<TElemento>.NombreCrud}_{enumTipoControl.Historial.Render()}".ToLower();

        public DescriptorDeCrud<TPadre> Crud => (DescriptorDeCrud<TPadre>)Padre;
        public DescriptorDeEdicion<TPadre> Editor => (DescriptorDeEdicion<TPadre>)Padre;
        public ZonaDeMenu<TElemento> MenuHistorial { get; private set; }
        public ZonaDeFiltro<TElemento> Filtro { get; private set; }
        public ZonaDeDatos<TElemento> Datos { get; set; }
        public BloqueDeFitro<TElemento> BloqueGeneral => Filtro.ObtenerBloquePorEtiqueta(ltrBloques.General);
        public BloqueDeFitro<TElemento> BloqueComun => Filtro.ObtenerBloquePorEtiqueta(ltrBloques.Comun);

        public List<string> OpcionesHistorial { get; set; } = new List<string>();
        public Dictionary<string, string> OpcionesDeFiltrado { get; set; } = new Dictionary<string, string>();

        private string OrdenacionInicial { get; set; }

        public new string IdHtml => Historial;

        public string IdHtmlZonaNavegador => $"cuerpo.pie.{IdHtml}";

        public bool  ContenidoEnEdicion { get; set; } = false;

        public DescriptorDeHistorial(DescriptorDeCrud<TPadre> crud, string etiqueta)
          : base(
            padre: crud,
            id: $"{crud.Id}_{enumTipoControl.Historial.Render()}",
            etiqueta: etiqueta,
            propiedad: null,
            ayuda: null,
            posicion: null
          )
        {
            Tipo = enumTipoControl.Historial;
            ModificarId( Id.Replace(typeof(TPadre).Name, typeof(TElemento).Name));
            MenuHistorial = new ZonaDeMenu<TElemento>(padre: this);
            Filtro = new ZonaDeFiltro<TElemento>(padre: this);
            Datos = new ZonaDeDatos<TElemento>(padre: this);

            DefinirColumnas();

            MenuHistorial.AnadirOpcionDeCerrarHistorial();

            OrdenacionInicial = $"{nameof(HistorialDto.OcurridoEl)}:{nameof(ElementoDtm.FechaCreacion)}:{enumModoOrdenacion.descendente.Render()}";
            DefinirMfHistorial(OpcionesHistorial);

            DefinirOpcionesDeFiltrado();

        }

        public DescriptorDeHistorial(DescriptorDeEdicion<TPadre> editor)
        : base(
          padre: editor,
          id: $"{editor.Id}_{enumTipoControl.Historial.Render()}",
          etiqueta: "",
          propiedad: null,
          ayuda: null,
          posicion: null
        )
        {
            Tipo = enumTipoControl.Historial;
            ModificarId(Id.Replace(typeof(TPadre).Name, typeof(TElemento).Name));
            Datos = new ZonaDeDatos<TElemento>(padre: this);
            DefinirColumnas();
            OrdenacionInicial = $"{nameof(HistorialDto.OcurridoEl)}:{nameof(ElementoDtm.FechaCreacion)}:{enumModoOrdenacion.descendente.Render()}";
            ContenidoEnEdicion = true;
        }

        protected void DefinirOpcionesDeFiltrado()
        {
            OpcionesDeFiltrado = new Dictionary<string, string> {
                    { "-1", "excluir el ..." },
                    { ltrSucesosExcluir.hitos, "Hitos" },
                    { ltrSucesosExcluir.eventos, "Eventos" },
                    { ltrSucesosExcluir.archivos, "Archivos" },
                    { ltrSucesosExcluir.correos, "Correos" },
                    { ltrSucesosExcluir.observaciones, "Observaciones" },
                    { ltrSucesosExcluir.archivadores, "Archivadores" },
                    { ltrSucesosExcluir.tareas, "Tareas" },
                    { ltrSucesosExcluir.nivel2hitos, "Hitos secundarios" },
                    { ltrSucesosExcluir.nivel2correos, "Correos secundarios" },
                    { ltrSucesosExcluir.nivel2ob, "Observaciones secundarios" },
                    { ltrSucesosExcluir.nivel2archivos, "Archivos secundarios" }
                };
        }

        private void DefinirColumnas()
        {
            Datos.AnadirColumna(ZonaDeDatos<TElemento>.ColumnaDeControlDeSeleccion());
            var propiedades = typeof(TElemento).GetProperties();
            var columnasPendientes = new Dictionary<int, ColumnaDelGrid>();
            ZonaDeDatos<TElemento>.AnadirPropiedades(Datos, propiedades, columnasPendientes, hayPosicionCero: false, new List<PropiedaJson>());
        }

        internal void DefinirMfHistorial(List<string> opciones)
        {
        }

        public string RenderDelHistorial()
        {
            return RenderControl();
        }


        public override string RenderControl()
        {

            var htmlCuerpoCabecera = RenderCuerpoCabecera(RenderMenuDelHistorial());
            var htmlCuerpoDatos = RenderCuerpoDatos(Filtro.RenderZonaDeFiltroNoModal(), Datos.RenderControl());
            var htmlCuerpoPie = RenderCuerpoPie();

            var htmContenedorMnt =
                $@" <div id='{IdHtml}' class='{enumCssDiv.DivOculto.Render()} {enumCssHistorial.CuerpoHistorial.Render()}' controlador=¨{Crud.Controlador}¨> 
                      <!--  ******************* Cabecera del historial: título y menú ******************* -->
                         {htmlCuerpoCabecera} 
                      <!--  ******************* Datos del cuerpo del historial: filtro y grid de datos ******************* -->
                         {htmlCuerpoDatos}                  
                      <!--  ******************* Pié del cuerpo del historial: zona de navegación ******************* -->
                         {htmlCuerpoPie}
                         {ModalDeMensaje.RenderModal<DetalleDeUnSucesoDto>(this, enumNameSpaceTs.Crud, "Detalle del suceso", parametrosTrasApertura: null)}
                    </div>
                ";

            return htmContenedorMnt.Render();
        }


        private string RenderCuerpoCabecera(string htmlMenu)
        {
            var clausulaDeOrdenInicial = $"{Environment.NewLine}orden-inicial='{OrdenacionInicial.ToLower()}'";
            var propiedades = $@" id='cuerpo.cabecera.{IdHtml}' 
                        class='{enumCssHistorial.ContenedorCabecera.Render()}' 
                        grid-del-mnt='{Datos.IdHtml}' 
                        zona-de-filtro='{Filtro.IdHtml}' 
                        zona-de-menu='{MenuHistorial.IdHtml}' 
                        controlador='{Crud.Controlador}' 
                        negocio='{Crud.RenderNegocio}'
                        enumNegocio='{Crud.Negocio}'
                        dto='{Crud.RenderDto}'
                        permite-crear = {false}
                        permite-editar = {false}
                        permite-borrar = {false}
                        id-negocio='{Crud.RenderIdDeNegocio}'{clausulaDeOrdenInicial}>
                     ";

            return $@"<div {propiedades}
                       {htmlMenu}
                    </div>";
        }

        private object RenderCuerpoDatos(string htmlFiltro, string htmlDatos)
        {
            return
            $@"<div id='cuerpo.datos.{IdHtml}' class='{enumCssHistorial.ContenedorCuerpo.Render()}'>
                     {htmlFiltro}
                     {htmlDatos}
               </div>";
        }

        private object RenderCuerpoPie()
        {
            return $@"<div id='{IdHtmlZonaNavegador}' class='{enumCssHistorial.ContenedorPie.Render()}' >
                       {Datos.Grid.NavegadorToHtml()}
                     </div>";
        }


        private string RenderMenuDelHistorial()
        {
            if (Etiqueta.IsNullOrEmpty()) Etiqueta = Crud.Negocio == enumNegocio.No_Definido
            ? typeof(TElemento).Name.Replace("Dto", "")
            : $"Gestión de {Crud.Negocio.Plural().ToLower()}";

            var htmlParteSuperiror = $@"
            <!--  ******************* menú ******************* -->
            <div id = ¨contenedor.{IdHtml}.MenuDelHistorial¨ class=¨{enumCssHistorial.ContenedorMenu.Render()}¨>  
                <div id = ¨contenedor.{IdHtml}.ZonaMenuDelHistorial¨  class=¨{Css.Render(enumCssDiv.DivVisible)} {enumCssHistorial.ZonaMenu.Render()}¨>     
                 {MenuHistorial.RenderControl()} 
                </div> 
                <div id=¨{IdHtml}.menu.del.flujo¨ class='{enumCssHistorial.Proceso.Render()} {enumCssHistorial.Titulo.Render()}'> 
                {Etiqueta}
                </div> 
                <div id =¨div.mostrar.{IdHtml}¨ class=¨{Css.Render(enumCssDiv.DivVisible)} {enumCssHistorial.FiltroExpansor.Render()}¨>     
                  <a id = ¨mostrar.{IdHtml}.ref¨ href=¨javascript:{enumNameSpaceTs.Crud}.{enumGestorDeEventos.EjecutarMenuHistorial}('{eventosDeHistorial.OcultarMostrarFiltro}', '{("")}');¨>Ocultar filtro</a>
                  <input id=¨expandir.{IdHtml}¨ type=¨hidden¨ value=¨1¨ >  
                </div>
                <div id='{IdHtml}.{DescriptorDeCrud<TElemento>.menuHistorial}' class='{enumCssHistorial.MenuMfHistorial.Render()}' offset-x = 150 menu-flotante='{DescriptorDeCrud<TElemento>.menuHistorial}'> </div> 
                <div id='{IdHtml}.menu.vacio' class='{Css.Render(enumCssMnt.DivVacioDeLaDerecha)}'> </div>
            </div>";
            return htmlParteSuperiror;
        }

        public string RenderMenuFlotanteHistorial(string idMenu)
        {
            var opciones = "";
            foreach (var o in OpcionesHistorial) opciones = $"{opciones}{(opciones.IsNullOrEmpty() ? "" : Environment.NewLine)}{o}";
            var htmMenuHistorial = $@"
             <!-- ****************************** menu individual ***************************************-->
             <ul id='{idMenu}' class=¨{enumCssMenuFlotante.menuFlotante.Render()} {enumCssMenuFlotante.Blanco.Render()} {enumCssMenuFlotante.SombraBlanca.Render()}¨>
             {opciones}
             </ul>";
            return htmMenuHistorial;
        }

    }
}

