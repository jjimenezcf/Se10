using System;
using Utilidades;
using ModeloDeDto;
using UtilidadesParaIu;
using System.Collections.Generic;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Elemento;
using ModeloDeDto.Negocio;
using ServicioDeDatos.Negocio;
using GestorDeElementos;
using MVCSistemaDeElementos.UtilidadesIu;
using Newtonsoft.Json.Linq;
using GestorDeElementos.Extensores;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeMantenimiento<TElemento> : ControlHtml where TElemento : ElementoDto
    {
        public static string NombreMnt = $"{DescriptorDeCrud<TElemento>.NombreCrud}_{enumTipoControl.Mantenimiento.Render()}".ToLower();

        public DescriptorDeCrud<TElemento> Crud => (DescriptorDeCrud<TElemento>)Padre;
        public ZonaDeMenu<TElemento> ZonaMenu { get; private set; }
        public ZonaDeFiltro<TElemento> Filtro { get; private set; }
        public ZonaDeDatos<TElemento> Datos { get; set; }
        public BloqueDeFitro<TElemento> BloqueGeneral => Filtro.ObtenerBloquePorEtiqueta(ltrBloques.General);
        public BloqueDeFitro<TElemento> BloqueComun => Filtro.ObtenerBloquePorEtiqueta(ltrBloques.Comun);

        public List<string> OpcionesPorElemento { get; set; } = new List<string>();
        public List<string> OpcionesContextuales { get; set; } = new List<string>();
        public List<string> OpcionesDeRelacion { get; set; } = new List<string>();
        public List<string> OpcionesDeFiltro { get; set; } = new List<string>();

        public bool PermiteCrear { get; set; } 

        public string OrdenacionInicial { get; set; }

        public string IaTitulo { get; set; }
        public enumAccionVisorArchivo IaAccion { get; set; } = enumAccionVisorArchivo.Resumir;

        //public new string IdHtml => NombreMnt;

        public string IdHtmlZonaNavegador => $"cuerpo.pie.{IdHtml}";

        public DescriptorDeMantenimiento(DescriptorDeCrud<TElemento> crud, string etiqueta)
            : base(
              padre: crud,
              id: $"{crud.Id}_{enumTipoControl.Mantenimiento.Render()}",
              etiqueta: etiqueta,
              propiedad: null,
              ayuda: null,
              posicion: null
            )
        {
            Tipo = enumTipoControl.Mantenimiento;
            ZonaMenu = new ZonaDeMenu<TElemento>(padre: this);
            Filtro = new ZonaDeFiltro<TElemento>(padre: this);
            Datos = new ZonaDeDatos<TElemento>(padre: this);

            DescriptorDeEdicion<TElemento>.DefinirMfIndividual(crud.Negocio, OpcionesPorElemento);
            DefinirMfContextual();
            DefinirMfDeFiltro();
            var p = crud.AtributosJson.Find(x => x.propiedad.Equals(nameof(OrdenacionInicial), StringComparison.CurrentCultureIgnoreCase));
            if (p != null) OrdenacionInicial = p.ordenarGridPor;

            PermiteCrear = Crud.NegocioActivo && Crud.Modo == ModoDescriptor.Mantenimiento && (bool)ApiDeAtributos.ValorDelAtributo(typeof(TElemento), nameof(IUDtoAttribute.OpcionDeCrear));
        }

        public void CrearMenuDeCreacionDeUsuario(enumNameSpaceTs nameSpaceTs, enumFunctionTs functionTs)
        {
            if (Crud.Negocio == enumNegocio.No_Definido)
                return;

            ZonaMenu.OpcionesDesplegables.Add("-1", "Opciones de creación");
            var opciones = Crud.Contexto.SeleccionarTodos<PlantillaDeCreacionDtm>(new Dictionary<string, object>
            {
                { nameof(PlantillaDeCreacionDtm.IdNegocio), Crud.Negocio.IdNegocio()},
                { nameof(PlantillaDeCreacionDtm.IdUsuario),Crud.Contexto.DatosDeConexion.IdUsuario},
                { nameof(PlantillaDeCreacionDtm.Vista),Etiqueta}
            });
            foreach (var opcion in opciones)
            {
                ZonaMenu.OpcionesDesplegables.Add(opcion.Id.ToString(), opcion.Nombre);
            }

            ZonaMenu.ProcesarOpcionesDesplegables = $"javascript: {nameSpaceTs}.{functionTs}()";
        }

        private void DefinirMfContextual()
        {
            if ((bool)ApiDeAtributos.ValorDelAtributo(typeof(TElemento), nameof(IUDtoAttribute.OpcionDeExportar)))
                IncluirMfContextual($"<li id='{DescriptorDeCrud<TElemento>.menuContextual}.{eventosDeMf.ModalDeExportar}' accion-menu='{eventosDeMf.ModalDeExportar}' {AtributosHtml.Mf(enumCssOpcionMenu.DeVista, enumModoDeAccesoDeDatos.Consultor, false)}>Exportar datos</li>");
            //IncluirMfContextual($"<li id='menu.contextual.salvar' accion-menu='{eventosDeMf.SalvarDatosDelGrid}' {AtributosHtml.Mf(enumCssOpcionMenu.DeVista, enumModoDeAccesoDeDatos.Consultor, false)}>Salvar nº de filas</li>");

        }
        private void DefinirMfDeFiltro()
        {
            if (Crud.Negocio != enumNegocio.No_Definido)
            {
                Crud.modalesParaPedirDatos.Add(new ModalParaPedirDatos(Crud, typeof(PlantillaDeFiltradoDto), eventosDeMf.Comun_GuardarPlantillaFiltrado, $"Plantilla para filtrar {Crud.Negocio.Plural(true)}"));
                Crud.modalesParaPedirDatos.Add(new ModalParaPedirDatos(Crud, typeof(EliminarFiltroDto), eventosDeMf.Comun_EliminarPlantillaFiltrado, $"Eliminar plantilla de filtrado de {Crud.Negocio.Plural(true)}"));
                IncluirMfDeFiltro($"<li id='{DescriptorDeCrud<TElemento>.menuDeFiltro}.{eventosDeMf.Comun_GuardarPlantillaFiltrado}' accion-menu='{eventosDeMf.Comun_GuardarPlantillaFiltrado}' {AtributosHtml.Mf(enumCssOpcionMenu.DeVista, enumModoDeAccesoDeDatos.Consultor, false)}>Guardar filtros</li>");
                IncluirMfDeFiltro($"<li id='{DescriptorDeCrud<TElemento>.menuDeFiltro}.{eventosDeMf.Comun_EliminarPlantillaFiltrado}' accion-menu='{eventosDeMf.Comun_EliminarPlantillaFiltrado}' {AtributosHtml.Mf(enumCssOpcionMenu.DeVista, enumModoDeAccesoDeDatos.Consultor, false)}>Eliminar filtros</li>");

                Crud.modalesParaPedirDatos.Add(new ModalParaOcultarColumnas(Crud, eventosDeMf.Comun_OcultarColumnas, $"Mostrar las columnas marcadas"));
                IncluirMfDeFiltro($"<li id='{DescriptorDeCrud<TElemento>.menuDeFiltro}.{eventosDeMf.Comun_OcultarColumnas}' accion-menu='{eventosDeMf.Comun_OcultarColumnas}' {AtributosHtml.Mf(enumCssOpcionMenu.DeVista, enumModoDeAccesoDeDatos.Consultor, false)}>Columnas visibles</li>");
            }
        }

        internal void IncluirMfContextual(string opcionHtml)
        {
            OpcionesContextuales.Add(opcionHtml);
        }

        internal void IncluirMfDeRelacion(string opcionHtml)
        {
            OpcionesDeRelacion.Add(opcionHtml);
        }

        internal void IncluirMfDeFiltro(string opcionHtml)
        {
            OpcionesDeFiltro.Add(opcionHtml);
        }


        public string RenderDelMantenimiento()
        {
            return RenderControl();
        }

        public string RenderMntModal(string idModal, enumTipoDeModal tipoDeModal)
        {
            Datos.IdHtmlModal = idModal.ToLower();

            var htmlMnt =
                   Filtro.RenderFiltroDeUnaModal(tipoDeModal) + Environment.NewLine +
                   Datos.RenderControl() + Environment.NewLine;

            var htmContenedorMnt =
                $@"
                   <div id=¨{IdHtml}¨ 
                     class=¨{enumCssModal.ContenedorCuerpoConGrid.Render()} {Css.Render(enumCssDiv.DivVisible)}¨ 
                     grid-del-mnt=¨{Datos.IdHtml}¨ 
                     filtro =¨{Filtro.IdHtml}¨
                     controlador = ¨{Crud.Controlador}¨>
                     {htmlMnt}
                   </div>
                ";

            return htmContenedorMnt.Render();
        }

        public override string RenderControl()
        {

            var htmlCuerpoCabecera = RenderCuerpoCabecera(RenderMenuDelMnt());
            var htmlCuerpoDatos = RenderCuerpoDatos(Filtro.RenderZonaDeFiltroNoModal(), Datos.RenderControl());
            var htmlCuerpoPie = RenderCuerpoPie();

            var htmContenedorMnt =
                $@"  
                  <!--  ******************* Cabecera del cuerpo: título y menú ******************* -->
                     {htmlCuerpoCabecera} 
                  <!--  ******************* Datos del cuerpo: filtro y grid de datos ******************* -->
                     {htmlCuerpoDatos}                  
                  <!--  ******************* Pié del cuerpo: zona de navegación ******************* -->
                     {htmlCuerpoPie}
                ";

            foreach (var o in ZonaMenu.Menu.OpcionesDeMenu)
            {
                if (o.Accion.TipoDeAccion == eventosDeMnt.AbrirModalParaRelacionar)
                {
                    var renderModal = ((RelacionarElementos)o.Accion).RenderDeLaModal();
                    htmContenedorMnt = htmContenedorMnt + Environment.NewLine + renderModal;
                }
                else if (o.Accion.TipoDeAccion == eventosDeMnt.AbrirModalParaImputar)
                {
                    var renderModal = ((ImputarElementos)o.Accion).RenderDeLaModal();
                    htmContenedorMnt = htmContenedorMnt + Environment.NewLine + renderModal;
                }
                else if (o.Accion.TipoDeAccion == eventosDeMnt.AbrirModalParaConsultarRelaciones)
                {
                    var renderModal = ((ConsultarRelaciones)o.Accion).RenderDeLaModal();
                    htmContenedorMnt = htmContenedorMnt + Environment.NewLine + renderModal;
                }
            }

            return htmContenedorMnt.Render();
        }


        private string RenderCuerpoCabecera(string htmlMenu)
        {
            List<OrdenDeColumna> ordenacion = new List<OrdenDeColumna> { };
            if (Crud.Negocio != enumNegocio.No_Definido && (!Datos?.EsHistorial ?? false) && !Crud.EsModal)
            {
                JObject ordenacionJobject = (JObject)Crud.Negocio.LeerParametroDeUsuario<JObject>(Crud.Contexto, enumParametrosDeUsuario.USU_Ordenacion_Del_Resultado);
                if (ordenacionJobject.HasValues)
                    ordenacion = ordenacionJobject[ltrParametrosDeUsuarios.ordenacion].ToObject<List<OrdenDeColumna>>();
            }
            
            if (ordenacion.Count > 0)
            {
                OrdenacionInicial = "";
                foreach (var orden in ordenacion)
                {
                    OrdenacionInicial =  $"{(OrdenacionInicial.IsNullOrEmpty()?"":$"{OrdenacionInicial};")}{orden.Propiedad}:{orden.OrdenadoPor}:{orden.Modo}";
                }
            }
            else
            {
                if (OrdenacionInicial.IsNullOrEmpty())
                {
                    if (typeof(TElemento).TienenLaPropiedad(nameof(ElmentoAuditadoDto.CreadoEl)))
                        OrdenacionInicial = $"{nameof(ElmentoAuditadoDto.CreadoEl)}:{nameof(ElementoDtm.FechaCreacion)}:{enumModoOrdenacion.descendente.Render()}";
                    else
                    if (typeof(TElemento).ImplementaReferenciaDto())
                        OrdenacionInicial = $"{nameof(IUsaReferenciaDto.Referencia)}:{nameof(IUsaReferencia.Referencia)}:{enumModoOrdenacion.ascendente.Render()}";
                    else
                    if (typeof(TElemento).ImplementaNombreDto())
                        OrdenacionInicial = $"{nameof(IUsaNombreDto.Nombre)}:{nameof(INombre.Nombre)}:{enumModoOrdenacion.ascendente.Render()}";
                }
            }

            var clausulaDeOrdenInicial = "";
            if (!OrdenacionInicial.IsNullOrEmpty())
                clausulaDeOrdenInicial = $"{Environment.NewLine}orden-inicial='{OrdenacionInicial.ToLower()}'";


            var propiedades = $@" id='{IdHtml}' 
                        class='{Css.Render(enumCssCuerpo.CuerpoCabecera)}' 
                        grid-del-mnt='{Datos.IdHtml}' 
                        zona-de-filtro='{Filtro.IdHtml}' 
                        zona-de-menu='{ZonaMenu.IdHtml}' 
                        controlador='{Crud.Controlador}' 
                        negocio='{Crud.RenderNegocio}'
                        enumNegocio='{Crud.Negocio}',
                        {ltrParametrosEp.Descriptor}='{Crud.GetType().Name}',
                        {ltrParametrosEp.Vista}='{Crud.Vista}',
                        dto='{Crud.RenderDto}'
                        permite-crear = {PermiteCrear}
                        permite-editar = {Crud.NegocioActivo && Crud.Modo == ModoDescriptor.Mantenimiento && (bool)ApiDeAtributos.ValorDelAtributo(typeof(TElemento), nameof(IUDtoAttribute.OpcionDeEditar))}
                        permite-borrar = {Crud.NegocioActivo && Crud.Modo == ModoDescriptor.Mantenimiento && (bool)ApiDeAtributos.ValorDelAtributo(typeof(TElemento), nameof(IUDtoAttribute.OpcionDeBorrar))}
                        id-negocio='{Crud.RenderIdDeNegocio}'{clausulaDeOrdenInicial}
                        id-vista='{Crud.IdVista}'
                     ";

            return ModoDescriptor.Mantenimiento == ((DescriptorDeCrud<TElemento>)Padre).Modo ?
            $@"<div {propiedades}
                    {htmlMenu}
               </div>
                " :
            $@"<div {propiedades}>
               </div>";
        }

        private object RenderCuerpoDatos(string htmlFiltro, string htmlDatos)
        {
            return
            $@"<div id='cuerpo.datos.{IdHtml}' class='{Css.Render(enumCssCuerpo.CuerpoDatos)}'>
                     {htmlFiltro}
                     {htmlDatos}
               </div>";
        }

        private object RenderCuerpoPie()
        {
            //return $@"<div id=¨{IdHtmlZonaNavegador}¨ class=¨{Css.Render(enumCssCuerpo.CuerpoPie)}¨ style=¨grid-template-columns: 1fr 1fr 1fr 1fr 0fr;¨>
            //           {Datos.Grid.NavegadorToHtml()}
            //           <div id = ¨formulario-pie¨ class=¨{Css.Render(enumCssCuerpo.CuerpoPieFormulario)}¨ style=¨display: none; height: 0px; width: 0px;¨>
            //           </div>
            //         </div>";

            return $@"<div id='{IdHtmlZonaNavegador}' class='{Css.Render(enumCssCuerpo.CuerpoPie)}'>
                       {Datos.Grid.NavegadorToHtml()}
                     </div>";
        }


        private string RenderMenuDelMnt()
        {
            if (Etiqueta.IsNullOrEmpty()) Etiqueta = Crud.Negocio == enumNegocio.No_Definido
            ? typeof(TElemento).Name.Replace("Dto", "")
            : $"Gestión de {Crud.Negocio.Plural().ToLower()}";

            var cssDivNulo = !(bool)ApiDeAtributos.ValorDelAtributo(typeof(TElemento), nameof(IUDtoAttribute.ConMfs))
                ? enumCssMnt.DivNulo.Render()
                : "";

            var sinMfs = cssDivNulo.IsNullOrEmpty() ? "" : "sin-mfs";

            var htmlVisorDeDetalle = Crud.HayTablaConGraficos
                ? $"<div id='{IdHtml}.{DescriptorDeCrud<TElemento>.menuDeDetalles}' class='{enumCssMnt.MenuDeDetalle.Render()} {enumCssMnt.MenuDeDetalleOculto.Render()}' title='Mostrar o ocultar detalle' onclick=¨javascript:Crud.{enumGestorDeEventos.EventosDelMantenimiento}('{eventosDeMnt.MostrarOcultarVisorDeDetalle}');¨> </div>"
                : "";

            var htmlParteSuperiror = $@"
            <!--  ******************* menú ******************* -->
            <div id = ¨contenedor.{IdHtml}.MenuDelMnt¨ class=¨{Css.Render(enumCssMnt.MntMenuContenedor)} {sinMfs}¨>  
               <div id = '{ZonaMenu.IdHtml}'  class=¨{Css.Render(enumCssDiv.DivVisible)} {Css.Render(enumCssMnt.MntMenuZona)}¨>     
                 {ZonaMenu.RenderControl()} 
                </div> 
                <div id=¨{IdHtml}.menu.del.flujo¨ class='{Css.Render(enumCssMnt.MenuDelProceso)} {Css.Render(enumCssEdicion.Titulo)}'> 
                {Etiqueta}
                </div> 
                <div id =¨div.mostrar.{IdHtml}¨ class=¨{Css.Render(enumCssDiv.DivVisible)} {Css.Render(enumCssMnt.MntFiltroExpansor)}¨>     
                  <a id = ¨mostrar.{IdHtml}.ref¨ href=¨javascript:Crud.{enumGestorDeEventos.EventosDelMantenimiento}('{eventosDeMnt.OcultarMostrarFiltro}', '{("")}');¨>Ocultar filtro</a>
                  <input id=¨expandir.{IdHtml}¨ type=¨hidden¨ value=¨1¨ >  
                </div>
                {htmlVisorDeDetalle}
                <div id='{IdHtml}.{DescriptorDeCrud<TElemento>.menuDeFiltro}' class='{Css.Render(enumCssMnt.MenuDeFiltro)} {cssDivNulo}' offset-x = 110 menu-flotante='{DescriptorDeCrud<TElemento>.menuDeFiltro}'> </div>
                <div id='{IdHtml}.{DescriptorDeCrud<TElemento>.menuContextual}' class='{Css.Render(enumCssMnt.MenuContextual)} {cssDivNulo}' offset-x = 110 menu-flotante='{DescriptorDeCrud<TElemento>.menuContextual}'> </div>
                <div id='{IdHtml}.{DescriptorDeCrud<TElemento>.menuIndividual}' class='{Css.Render(enumCssMnt.MenuIndividual)} {cssDivNulo}' offset-x = 150 menu-flotante='{DescriptorDeCrud<TElemento>.menuIndividual}'> </div> 
                <div id='{IdHtml}.{DescriptorDeCrud<TElemento>.menuDeRelaciones}' class='{Css.Render(enumCssMnt.MenuDeRelaciones)} {cssDivNulo}' offset-x = 150 menu-flotante='{DescriptorDeCrud<TElemento>.menuDeRelaciones}'> </div> 
                <div id='{IdHtml}.menu.vacio' class='{Css.Render(enumCssMnt.DivVacioDeLaDerecha)}'> </div>
            </div>";
            return htmlParteSuperiror;
        }

        //private string RenderTitulo()
        //{
        //    var htmlCabecera = $"<div id='{IdHtml}.titulo'>{(Etiqueta.IsNullOrEmpty() ? "" : $"<h2>{Etiqueta}</h2>")}</div>";
        //    return htmlCabecera;
        //}


        public string RenderMenuFlotanteIndividual(string idMenu)
        {
            var opciones = "";
            foreach (var o in OpcionesPorElemento) opciones = $"{opciones}{(opciones.IsNullOrEmpty() ? "" : Environment.NewLine)}{o}";
            var htmMenuIndividual = $@"
             <!-- ****************************** menu individual ***************************************-->
             <ul id='{idMenu}' class=¨{enumCssMenuFlotante.menuFlotante.Render()} {enumCssMenuFlotante.Blanco.Render()} {enumCssMenuFlotante.SombraBlanca.Render()}¨>
             {opciones}
             </ul>";
            return htmMenuIndividual;
        }

        protected string OtrasOpcionesIndividuales()
        {
            return "";
        }

        public string RenderMenuFlotanteContextual(string idMenu)
        {
            var opciones = "";
            foreach (var o in OpcionesContextuales) opciones = $"{opciones}{(opciones.IsNullOrEmpty() ? "" : Environment.NewLine)}{o}";
            var htmMenuContextual = $@"
             <!-- ****************************** menu contextual ***************************************-->
             <ul id='{idMenu}' class=¨{enumCssMenuFlotante.menuFlotante.Render()} {enumCssMenuFlotante.Blanco.Render()} {enumCssMenuFlotante.SombraBlanca.Render()}¨>
             {opciones}
             </ul>";
            return htmMenuContextual;
        }

        public string RenderMenuFlotanteDeRelaciones(string idMenu)
        {
            var opciones = "";
            foreach (var o in OpcionesDeRelacion) opciones = $"{opciones}{(opciones.IsNullOrEmpty() ? "" : Environment.NewLine)}{o}";
            var htmMenuDeRelacion = $@"
             <!-- ****************************** menu de relaciones ***************************************-->
             <ul id='{idMenu}' class=¨{enumCssMenuFlotante.menuFlotante.Render()} {enumCssMenuFlotante.Blanco.Render()} {enumCssMenuFlotante.SombraBlanca.Render()}¨>
             {opciones}
             </ul>";
            return htmMenuDeRelacion;
        }

        public string RenderMenuFlotanteDeFiltro(string idMenu)
        {
            var opciones = "";
            foreach (var o in OpcionesDeFiltro) opciones = $"{opciones}{(opciones.IsNullOrEmpty() ? "" : Environment.NewLine)}{o}";
            var htmMenuDeFiltro = $@"
             <!-- ****************************** menu de filtro ***************************************-->
             <ul id='{idMenu}' class=¨{enumCssMenuFlotante.menuFlotante.Render()} {enumCssMenuFlotante.Blanco.Render()} {enumCssMenuFlotante.SombraBlanca.Render()}¨>
             {opciones}
             </ul>";
            return htmMenuDeFiltro;
        }
    }
}


/*       menu-flotante='[menu]' -- Se asocia el atributo donde se quiere levantar el menú (div o li)
         <!-- ****************************** menu individual [menu] ***************************************-->
         <ul id='[menu]' class=¨{enumCssMenuFlotante.menuFlotante.Render()} {enumCssMenuFlotante.Blanco.Render()} {enumCssMenuFlotante.SombraBlanca.Render()}¨>
             <li id='[menu].observaciones' accion-menu='observaciones'>Gestionar observaciones</li>
             <li id='[menu].direcciones' accion-menu='direcciones'>Gestionar direcciones</li>
             <li id='[menu].imprimir' accion-menu='imprimir'>Imprimir seleccionado</li>
             <hr>   
             <li id='[menu].permisos' accion-menu='permiso'>Crear permiso</li>
         </ul>   
*/
