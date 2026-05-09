using System;
using System.Text;
using System.Collections.Generic;
using MVCSistemaDeElementos.Descriptores;
using Utilidades;
using ModeloDeDto;
using GestorDeElementos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos;

namespace UtilidadesParaIu
{
    public class Grid
    {
        public string Id { get; private set; }

        public dynamic ZonaDeDatos { get; private set; }


        public string IdHtml => Id.ToLower();

        public string IdHtmlCabeceraDeTabla => $"{IdHtml}_cabecera";

        public string IdHtmlFilaCabecera => $"{IdHtml}_c_tr_0";

        public string IdHtmlTabla => $"{IdHtml}_table";
        public string IdHtmlNavegador => $"{IdHtml}_nav";
        public string IdHtmlNavegador_1 => $"{IdHtmlNavegador}_1";
        public string IdHtmlNavegador_2 => $"{IdHtmlNavegador}_2";
        public string IdHtmlNavegador_3 => $"{IdHtmlNavegador}_3";
        public string IdHtmlPorLeer => $"{IdHtmlNavegador_2}_reg";

        public string Controlador => ZonaDeDatos.EsHistorial ? ZonaDeDatos.Historial.Crud.Controlador : ZonaDeDatos.Mnt.Crud.Controlador;
        public bool ConReseteo => ZonaDeDatos.EsHistorial || ZonaDeDatos.Mnt.Crud.Negocio == enumNegocio.No_Definido ? false : true;


        public List<ColumnaDelGrid> columnas { get; private set; } = new List<ColumnaDelGrid>();
        public List<FilaDelGrid> filas { get; private set; } = new List<FilaDelGrid>();

        public int NumeroDeFilas => filas.Count;

        public int TotalEnBd => ZonaDeDatos.TotalEnBd;
        private int PosicionInicial => ZonaDeDatos.PosicionInicial;
        private int CantidadPorLeer => ZonaDeDatos.CantidadPorLeer;
        public int Seleccionables { get; set; }
        public int Ultimo_Leido => PosicionInicial + filas.Count;

        public Grid(dynamic zonaDeDatos)
        {
            ZonaDeDatos = zonaDeDatos;
            Id = ZonaDeDatos.Id;
            Seleccionables = 2;
        }

        public FilaDelGrid ObtenerFila(int i)
        {
            return filas[i];
        }


        public string ToHtml(bool tablacograficos)
        {
            ZonaDeDatos.CalcularAnchosColumnas();
            return tablacograficos ? RenderizarGridConGraficos(this) : RenderizarGrid(this);
        }

        public string NavegadorToHtml()
        {
            return RenderNavegadorGrid(this);
        }

        private static string RenderColumnaCabecera(ColumnaDelGrid columna)
        {
            var padreEsEditor = columna.ZonaDeDatos.EsHistorial && columna.ZonaDeDatos.Historial.ContenidoEnEdicion;

            var padre = columna.ZonaDeDatos.EsHistorial 
                ? !padreEsEditor
                ? columna.ZonaDeDatos.Historial.Crud 
                : columna.ZonaDeDatos.Historial.Editor 
                : columna.ZonaDeDatos.Mnt.Crud;

            var porcentaje = padreEsEditor ? columna.PorAnchoMnt : padre.Modo == ModoDescriptor.SeleccionarParaFiltrar
                          || padre.Modo == ModoDescriptor.Relacion
                          || padre.Modo == ModoDescriptor.Imputar
                          || padre.Modo == ModoDescriptor.ModalDeConsulta
                ? columna.PorAnchoSel
                : columna.PorAnchoMnt;

            var esUnMantenimientoDeCrud = !padreEsEditor && padre.Modo == ModoDescriptor.Mantenimiento;
            var atributoWidth = columna.TamanoFijo.IsNullOrEmpty() && (columna.Visible || porcentaje > 0) ? $"width: {porcentaje}%;" : "";
            var atributoTamano = !columna.TamanoFijo.IsNullOrEmpty() ? $"tamano-fijo='{columna.TamanoFijo}'" : "";

            var atributosDelEstilo = $"text-align: {columna.AlineacionCss};";
            atributosDelEstilo = $"{atributoWidth} {atributosDelEstilo}";
            string htmlRef = columna.ConOrdenacion
                ? RenderAccionOrdenar(padre, padreEsEditor, columna)
                : columna.Titulo;

            string claseCss = columna.Visible ? Css.Render(enumCssGrid.ColumnaCabecera) : Css.Render(enumCssGrid.ColumnaOculta);

            if (columna.Visible && columna.CssDeLaColumna == enumCssGrid.ColumnaOculta)
                claseCss = Css.Render(enumCssGrid.ColumnaOculta);

            string claseCssAlineacion = columna.Alineada == enumAliniacion.derecha
                ? $"class=¨{enumCssGrid.ColumnaDiv.Render()} {enumCssGrid.ColumnaAlineadaDerecha.Render()}¨"
                : $"class=¨{enumCssGrid.ColumnaDiv.Render()}¨";

            var renderizarIconos = columna.ZonaDeDatos.EsHistorial ? false : !columna.ZonaDeDatos.Mnt.Crud.EsModal;
            var ayuda = renderizarIconos && !columna.Ayuda.IsNullOrEmpty() && columna.Titulo != columna.Ayuda
                ? $" title='{columna.Ayuda.Replace("[Negocio]", ((enumNegocio)padre.Negocio).ConArticulo())}'"
                : null;

            var renderizarTransicion = false;
            if (renderizarIconos)
            {
                if (columna.Propiedad.ToLower() == nameof(IUsaEstado.Estado).ToLower() && ((enumNegocio)padre.Negocio).UsaFlujo())
                    renderizarTransicion = true;
            }

            var htmlTh = $@"{Environment.NewLine}
                          <div scope=¨col¨
                              id = ¨{columna.IdHtml}¨ 
                              class=¨{enumCssDiv.Th.Render()} {claseCss}¨ 
                              propiedad = ¨{columna.Propiedad.ToLower()}¨
                              tipo-control = '{columna.TipoDeControl}'
                              es-fecha = {columna.EsFecha}
                              columna-accion = {columna.EsAccion}
                              {(columna.EsAccion ? $"accion = ¨{columna.Accion}¨" : "")}
                              formato = '{columna.Formato.Descripcion()}'
                              modo-ordenacion=¨{(columna.cssOrdenacion == enumCssOrdenacion.SinOrden ? $"{enumModoOrdenacion.sinOrden.Render()}" : $"{enumModoOrdenacion.ascendente.Render()}")}¨ 
                              {atributoTamano}
                              style = ¨{atributosDelEstilo}¨
                              alineacion=¨{columna.AlineacionCss}¨
                              ordenar-por = ¨{columna.OrdenarPor}¨>
                              <div id ='{columna.IdHtml}_encabezado' {claseCssAlineacion}{ayuda}>
                                {htmlRef}
                                {(columna.Propiedad.ToLower() != ltrColumnasDelGrid.chksel ?
                                             "" :
                                             $"<img id='{columna.IdHtml}_menu' alt='copiar urls' class='fa-icono img-menu-chksel' onclick='ApiCrud.CopiarUrls()'>"
                                )}
                              </div>
                                {(columna.Propiedad.ToLower() == ltrColumnasDelGrid.chksel || !renderizarIconos ?
                                             "" :
                                             $"<span class='{enumCssGrid.OcultarColumna.Render()}' title='Ocultar columna'" +
                                             $"onclick=¨Crud.EventosDelMantenimiento('{eventosDeMnt.OcultarMostrarColumnas}', '{columna.Propiedad.ToLower()}')¨></span>")}
                                {(columna.Propiedad.ToLower() == ltrColumnasDelGrid.chksel || !renderizarIconos ?
                                             "" :
                                             $"<span class='{enumCssGrid.OrdenarColumna.Render()}{(!columna.ConOrdenacion ? $" {enumCssGrid.OrdenarColumnaNoPermitido.Render()}" : "")}'  title='{(!columna.ConOrdenacion ? "No permite ordenar por este campo" : "Ordenar")}' {(!columna.ConOrdenacion ? " disabled" : "")}" +
                                             $"onclick=¨Crud.EventosDelMantenimiento('{eventosDeMnt.OrdenarPor}', '{columna.IdHtml}')¨></span>")}           
                                {(columna.Propiedad.ToLower() == ltrColumnasDelGrid.chksel || !renderizarTransicion ?
                                             "" :
                                             $"<span class='{enumCssGrid.TransitarElementos.Render()}'  title='transitar elementos seleccionados'></span>")}                                               
                              </div>";
            return htmlTh;
        }

        /*
         
                                {(columna.Propiedad.ToLower() == ltrColumnasDelGrid.chksel ? 
                                             "": 
                                             $"<span class='{enumCssGrid.OcultarColumna.Render()} " +
                                             $"{(columna.Alineada == enumAliniacion.derecha ? enumCssGrid.OcultarColumnaDerecha.Render(): 
                                                 columna.Alineada == enumAliniacion.izquierda ? enumCssGrid.OcultarColumnaIzquierda.Render(): enumCssGrid.OcultarColumnaCentrado.Render())}' " +
                                             $"onclick=¨Crud.EventosDelMantenimiento('{eventosDeMnt.OcultarMostrarColumnas}', '{columna.Propiedad.ToLower()}')¨></span>")
                                }
         * */

        private static string RenderAccionOrdenar(dynamic padre, bool padreEsEditor, ColumnaDelGrid columna)
        {

            var gestorDeEventos = RenderGestorDeEventos(padreEsEditor ? ModoDescriptor.Consulta: padre.Modo);

            var parametros = $"{columna.IdHtml}";
            if (!padreEsEditor && (padre.Modo == ModoDescriptor.SeleccionarParaFiltrar ||
                padre.Modo == ModoDescriptor.Relacion ||
                padre.Modo == ModoDescriptor.Imputar ||
                padre.Modo == ModoDescriptor.ModalDeConsulta))
            {
                parametros = $"{columna.ZonaDeDatos.IdHtmlModal}#{parametros}";
            }

            string htmlRef = $"href =¨javascript: Crud.{gestorDeEventos}('ordenar-por', '{parametros}')¨";

            return $@"<a {htmlRef} class=¨{Css.Render(columna.cssOrdenacion)}¨>{columna.Titulo}</a>";
        }

        private static string RenderEventoPuslsa(CeldaDelGrid celda, string idControlHtml)
        {
            var crud = celda.Fila.Datos.EsHistorial ? celda.Fila.Datos.Historial.Crud : celda.Fila.Datos.Mnt.Crud;
            var getorDeEventos = RenderGestorDeEventos(crud.Modo);

            var parametros = $"{celda.Fila.idHtmlCheckDeSeleccion}#{idControlHtml}";
            if (crud.Modo == ModoDescriptor.SeleccionarParaFiltrar ||
                crud.Modo == ModoDescriptor.Relacion ||
                crud.Modo == ModoDescriptor.Imputar ||
                crud.Modo == ModoDescriptor.ModalDeConsulta)
            {
                parametros = $"{celda.Fila.Datos.IdHtmlModal}#{parametros}";
            }

            return $"Crud.{getorDeEventos}('{eventosDeMnt.FilaPulsada}', '{parametros}');";
        }

        private static string RenderGestorDeEventos(ModoDescriptor modo)
        {
            var getorDeEventos = $"{enumGestorDeEventos.EventosDelMantenimiento}";
            if (modo == ModoDescriptor.SeleccionarParaFiltrar)
            {
                getorDeEventos = $"{enumGestorDeEventos.EventosModalDeSeleccion}";
            }
            else if (modo == ModoDescriptor.ParaSeleccionar)
            {
                getorDeEventos = $"{enumGestorDeEventos.EventosModalParaSeleccionar}";
            }
            else if (modo == ModoDescriptor.Relacion)
            {
                getorDeEventos = $"{enumGestorDeEventos.EventosModalDeCrearRelaciones}";
            }
            else if (modo == ModoDescriptor.Imputar)
            {
                getorDeEventos = $"{enumGestorDeEventos.EventosModalParaImputar}";
            }
            else if (modo == ModoDescriptor.ModalDeConsulta)
            {
                getorDeEventos = $"{enumGestorDeEventos.EventosModalDeConsultaDeRelaciones}";
            }
            return getorDeEventos;
        }


        private static string RenderTd(CeldaDelGrid celda)
        {

            var nombreTd = $"td.{celda.Propiedad}.{celda.Fila.Datos.IdHtml}".ToLower();
            string pulsarCheck = RenderEventoPuslsa(celda, celda.idHtmlTd);

            var onclickTd = $"onclick=¨{pulsarCheck}¨";
            var ocultar = celda.Visible ? "" : "hidden";

            var tdHtml = $@"<div id=¨{celda.idHtmlTd}¨ class='{enumCssDiv.Td.Render()}'
                                name=¨{nombreTd}¨ 
                                style=¨text-align: {celda.AlineacionCss()};¨ 
                                propiedad=¨{celda.Propiedad}¨ 
                                {onclickTd} 
                                {ocultar} >
                                {RenderCeldaDelTd(celda)}
                           </div>";
            return tdHtml;
        }

        private static string RenderCeldaDelTd(CeldaDelGrid celda)
        {
            var idDelInput = $"{celda.idHtml}";
            string pulsarCheck = RenderEventoPuslsa(celda, idDelInput);

            var tipoHtml = celda.Tipo == typeof(bool) ? "type =¨checkbox¨" : "type =¨text¨";
            var onclick = celda.Tipo == typeof(bool)
                  ? $"onclick=¨{pulsarCheck}¨"
                  : "";


            var editable = !celda.Editable ? "readonly" : "";

            var nombreInput = $"{celda.Propiedad}.{celda.Fila.Datos.IdHtml}".ToLower();

            var input = $" <input {tipoHtml} id=¨{idDelInput}¨ " +
            $"        name=¨{nombreInput}¨ " +
            $"        style=¨width:100%; border:0; text-align: {celda.AlineacionCss()};¨ " +
            $"        propiedad=¨{celda.Propiedad}¨ " +
            $"        style=¨width:100%; border:0¨ " +
            $"        {editable} " +
            $"        {onclick} " +
            $"        value=¨{celda.Valor}¨ />";

            return input;
        }


        private static string RenderFila(FilaDelGrid fila)
        {
            var filaHtml = new StringBuilder();
            var numCol = 0;
            for (var j = 0; j < fila.NumeroDeCeldas; j++)
            {
                var celda = fila.ObtenerCelda(j);
                filaHtml.AppendLine(RenderTd(celda));
                numCol++;
            }
            return $@"{filaHtml}";
        }

        private static string RenderFilaSeleccionable(FilaDelGrid fila)
        {
            string celdaDelCheck = ""; // RenderCeldaCheck(fila.Datos.IdHtml, fila.IdHtml, fila.NumeroDeCeldas);
            string filaHtml = RenderFila(fila);

            return $"<div id='{fila.IdHtml}'>{Environment.NewLine} class = '{enumCssDiv.Tr.Render()}'" +
                   $"   {celdaDelCheck}{filaHtml}{Environment.NewLine}" +
                   $"</div>{Environment.NewLine}";
        }

        private static string RenderCabecera(Grid grid)
        {
            var cabeceraHtml = new StringBuilder();
            foreach (var columna in grid.columnas)
            {
                cabeceraHtml.Append(RenderColumnaCabecera(columna));
            }

            return $@"<div id='{grid.IdHtmlCabeceraDeTabla}' class=¨{enumCssDiv.Thead.Render()} {Css.Render(enumCssCuerpo.CuerpoDatosGridThead)}¨ >{Environment.NewLine}
                         <div id=¨{grid.IdHtmlFilaCabecera}¨ class='{enumCssDiv.Tr.Render()}'>
                            {cabeceraHtml}
                         </div>
                      </div>";

        }

        private static string AplicarCss(bool mostrarElNavegadorEnElGrid, enumCssNavegadorEnModal claseCss)
        {
            if (mostrarElNavegadorEnElGrid)
            {
                return Css.Render(claseCss);
            }
            else
            {
                switch (claseCss)
                {
                    case enumCssNavegadorEnModal.InfoGrid: return Css.Render(enumCssNavegadorEnMnt.InfoGrid);
                    case enumCssNavegadorEnModal.Mensaje: return Css.Render(enumCssNavegadorEnMnt.Mensaje);
                    case enumCssNavegadorEnModal.Cantidad: return Css.Render(enumCssNavegadorEnMnt.Cantidad);
                    case enumCssNavegadorEnModal.Opcion: return Css.Render(enumCssNavegadorEnMnt.Opcion);
                    case enumCssNavegadorEnModal.Navegador: return Css.Render(enumCssNavegadorEnMnt.Navegador);
                }

            }
            throw new Exception($"No se ha definido la clase a aplicar a para {claseCss} del enumerado del navegador");
        }

        private static string RenderNavegadorGrid(Grid grid)
        {
            dynamic crud = null;
            var navegadorEnFinDePagina = true;
            if (grid.ZonaDeDatos.EsHistorial)
                crud = grid.ZonaDeDatos.Historial.Crud;
            else
            {
                crud = grid.ZonaDeDatos.Mnt.Crud;
                navegadorEnFinDePagina = !crud.EsModal || crud.MantenimientoSoloConGrid;
            }

            var gestorDeEventos = RenderGestorDeEventos(crud.Modo);
            var idHtmlModal = crud.Modo == ModoDescriptor.Mantenimiento || crud.MantenimientoSoloConGrid
                ? ""
                : $"{grid.ZonaDeDatos.IdHtmlModal}";

            var accionUltimos = $"Crud.{gestorDeEventos}('obtener-ultimos','{idHtmlModal}')";
            var accionBuscar = $"Crud.{gestorDeEventos}('buscar-elementos','{idHtmlModal}')";
            var accionAnterior = $"Crud.{gestorDeEventos}('obtener-anteriores','{idHtmlModal}')";
            var accionSiguiente = $"Crud.{gestorDeEventos}('obtener-siguientes','{idHtmlModal}')";
            var accionCompartir = $"Crud.{gestorDeEventos}('compartir-elemento')";
            var accionEnviar = $"Crud.{gestorDeEventos}('enviar-elemento')";

            var htmlContenedorNavegador = !navegadorEnFinDePagina
                ? $@"
                   <!-- ***************** Navegador del grid ****************** -->
                   <div id= ¨{grid.IdHtml}_pie¨ class=¨{Css.Render(enumCssNavegadorEnModal.Contenedor)}¨>
                     htmlNavegadorGrid
                   </div>
                 "
                 : "htmlNavegadorGrid";

            var htmlCompartir = "";
            if (crud.UsaCompartir)
            {
                htmlCompartir = $@"
                        <button class=""{enumCssControles.CompartirConGuid.Render()}"" title=""envía la instancia seleccionada por correo"" onclick=""{accionEnviar}"">
                           <img src='{CacheDeVariable.Uri_EnviarCorreo}' style=""margin-top: -3px;"">
                        </button>
                        <button class=""{enumCssControles.CompartirConGuid.Render()}"" title=""crear enlace de la fila seleccionada y asignar al portapapeles"" onclick=""{accionCompartir}"">
                           <img src='{CacheDeVariable.Uri_Compartir}' style=""margin-top: -5px;"">
                        </button>";
            }


            var idModal = "";
            var mostrarElNavegadorEnElPieDePagina = true;
            if (!grid.ZonaDeDatos.EsHistorial)
            {
                mostrarElNavegadorEnElPieDePagina = !grid.ZonaDeDatos.Mnt.Crud.EsModal || grid.ZonaDeDatos.Mnt.Crud.MantenimientoSoloConGrid;
                idModal = mostrarElNavegadorEnElPieDePagina ? "" : $"{grid.ZonaDeDatos.Mnt.Datos.IdHtmlModal}";
            }

            var htmlNavegadorGrid = $@"
            <div id=¨{grid.IdHtmlNavegador}¨ class=¨{AplicarCss(!mostrarElNavegadorEnElPieDePagina, enumCssNavegadorEnModal.Navegador)}¨>
                 {htmlCompartir}
                 <div id=¨{grid.IdHtmlNavegador_2}¨>
                        <input type=¨number¨ 
                               id=¨{grid.IdHtmlPorLeer}¨ 
                               class = ¨{AplicarCss(!mostrarElNavegadorEnElPieDePagina, enumCssNavegadorEnModal.Cantidad)}¨
                               value=¨{grid.CantidadPorLeer}¨ 
                               min=¨1¨ step=¨1¨ max=¨999¨ 
                               pagina=¨1¨  
                               posicion=¨{grid.Ultimo_Leido}¨  
                               controlador=¨{grid.Controlador}¨  
                               onkeypress=¨javascript:Crud.{enumGestorDeEventos.EventosDelMantenimiento}('{eventosDeMnt.TeclaPulsada}', '{idModal}');¨
                               total-en-bd=¨{grid.TotalEnBd}¨ 
                               title=¨Pagina: 1 de un total de {Math.Ceiling((decimal)(grid.TotalEnBd / grid.CantidadPorLeer))}¨ />
                 </div>                 
                 <div id=¨{grid.IdHtmlNavegador_1}¨ data-type=¨img¨>
                        <img src=¨/images/paginaInicial.png¨ alt=¨Primera página¨ title=¨Ir al primer registro¨ onclick=¨{accionBuscar}¨>
                 </div>
                 <div id=¨{grid.IdHtmlNavegador_3}¨ data-type=¨img¨ >
                        <img src=¨/images/paginaAnterior.png¨ alt=¨Primera página¨ title=¨Página anterior¨ onclick=¨{accionAnterior}¨>
                        <img src=¨/images/paginaSiguiente.png¨ alt=¨Siguiente página¨ title=¨Página siguiente¨ onclick=¨{accionSiguiente}¨>
                        <img src=¨/images/paginaUltima.png¨ alt=¨Última página¨ title=¨Última página¨ onclick=¨{accionUltimos}¨>
                 </div>
            </div>
            <div id = ¨div.seleccion.{grid.IdHtml}¨ class=¨{AplicarCss(!mostrarElNavegadorEnElPieDePagina, enumCssNavegadorEnModal.Opcion)}¨>     
              {RenderOpcionesGridPlano(idHtmlModal, grid.IdHtml, grid.ConReseteo)}
            </div>
            <div id= ¨{grid.IdHtml}_mensaje¨ class=¨{AplicarCss(!mostrarElNavegadorEnElPieDePagina, enumCssNavegadorEnModal.Mensaje)}¨>
               Seleccionadas: 0 de {grid.TotalEnBd}
            </div>
            <div id= ¨{grid.IdHtml}_info¨ class=¨{AplicarCss(!mostrarElNavegadorEnElPieDePagina, enumCssNavegadorEnModal.InfoGrid)}¨>
               Pagina: 1 de un total de {Math.Ceiling((decimal)(grid.TotalEnBd / grid.CantidadPorLeer))}
            </div>
            ";
            return htmlContenedorNavegador.Replace("htmlNavegadorGrid", htmlNavegadorGrid);
        }


        private static string RenderOpcionesGridPlano(string IdHtmlModal, string IdHtmlGrid, bool conReseteo)
        {
            var htmlOpcionesGrid = @$"             

            <nav class='{enumCssMenuDelGrid.menuDelGrid.Render()}'>
                <ul>
                    <li class='{enumCssMenuDelGrid.menuDelGridRaiz.Render()}'>
                        <a href='#'>Opciones</a>
                        <ul>
                            <li class='{enumCssMenuDelGrid.menuDelGridOpcion.Render()}' >
                                <a href=¨javascript:Crud.{enumGestorDeEventos.EventosMenuDelGrid}('{eventosDeAccionDeGrid.RecargarGrid}', '{IdHtmlModal}');¨>Buscar</a>
                            </li>
                            <li class='{enumCssMenuDelGrid.menuDelGridOpcion.Render()}' >
                                <a href=¨javascript:Crud.{enumGestorDeEventos.EventosMenuDelGrid}('{eventosDeAccionDeGrid.SeleccionarTodo}', '{IdHtmlModal}');¨>seleccionar todo</a>
                            </li>
                            <li class='{enumCssMenuDelGrid.menuDelGridOpcion.Render()}' >
                                 <a href=¨javascript:Crud.{enumGestorDeEventos.EventosMenuDelGrid}('{eventosDeAccionDeGrid.AnularSeleccion}', '{IdHtmlModal}');¨>anular selección</a>
                            </li>
                            <li class='{enumCssMenuDelGrid.menuDelGridOpcion.Render()}'>
                                 <a id='div.seleccion.{IdHtmlGrid}.ref' href=¨javascript:Crud.{enumGestorDeEventos.EventosMenuDelGrid}('{eventosDeAccionDeGrid.MostrarSoloSeleccionadas}', '{IdHtmlModal}');¨>Solo las seleccionadas</a>
                                 <input id=¨div.seleccion.{IdHtmlGrid}.input¨ type=¨hidden¨ value=¨0¨ > 
                            </li>
                            <li class='{enumCssMenuDelGrid.menuDelGridOpcion.Render()}' >
                                 <a href=¨javascript:Crud.{enumGestorDeEventos.EventosMenuDelGrid}('{eventosDeAccionDeGrid.AnularOrden}', '{IdHtmlModal}');¨>anular orden</a>
                            </li>
                            <li class='{enumCssMenuDelGrid.menuDelGridOpcion.Render()}' {(!conReseteo ? "style = 'padding-bottom: 3px;'" : "")} >
                                 <a href=¨javascript:Crud.{enumGestorDeEventos.EventosMenuDelGrid}('{eventosDeAccionDeGrid.AplicarOrdenInicial}', '{IdHtmlModal}');¨>orden inicial</a>
                            </li>
                            {(conReseteo ? $@"
                            <li class='{enumCssMenuDelGrid.menuDelGridOpcion.Render()}' style='padding-bottom: 3px;' >
                                 <a href=¨javascript:Crud.{enumGestorDeEventos.EventosMenuDelGrid}('{EventosModal.ResetearVista}', '{IdHtmlModal}');¨>Resetar columnas</a>
                            </li>" : "")}
                        </ul>
                    </li>
                </ul>
            </nav>
             ";
            return htmlOpcionesGrid;
        }

        private static string RenderizarGrid(Grid grid)
        {
            var htmlTabla = $@"<div class = 'contenedor-tabla'>
                               <div id=¨{grid.IdHtmlTabla}¨ class=¨{enumCssDiv.Tabla.Render()} {enumCssDiv.CuerpoDatosTabla.Render()} {enumCssBootStrap.table.Render()} {enumCssBootStrap.tableStriped.Render()}¨ style=¨margin-bottom: 0px;¨>
                                    {RenderCabecera(grid)}
                               </div>
                               <div id=¨{grid.IdHtmlTabla}-barra¨ class=¨vertical-bar¨></div>
                               </div>
                             ";
            var htmlGrid = "";
            if (grid.ZonaDeDatos.EsHistorial)
            {
                htmlGrid = htmlTabla;
            }
            else
            {
                var mostrarElNavegadorEnElGrid = grid.ZonaDeDatos.Mnt.Crud.EsModal && !grid.ZonaDeDatos.Mnt.Crud.MantenimientoSoloConGrid;
                htmlGrid = mostrarElNavegadorEnElGrid ? htmlTabla + RenderNavegadorGrid(grid) : htmlTabla;
            }

            return htmlGrid;
        }
        private static string RenderizarGridConGraficos(Grid grid)
        {
            // Usamos 'div-grid-tabla' como un nuevo contenedor para aplicar 'flex-basis' (ancho inicial)
            var htmlTabla = $@"<div class = '{enumCssGrid.ContenedorDeLaTablaConGraficos.Render()}'>
                            <div class='{enumCssGrid.ContenedorDelGridConElDivDeLaTabla.Render()}'> 
                                <div id='{grid.IdHtmlTabla}' class='{enumCssDiv.Tabla.Render()} {enumCssDiv.CuerpoDatosTabla.Render()} {enumCssBootStrap.table.Render()} {enumCssBootStrap.tableStriped.Render()}' style='margin-bottom: 0px;'>
                                    {RenderCabecera(grid)}
                                </div>
                            </div>
                            <div class='{enumCssGrid.Splitter.Render()} {enumCssControles.DivNoVisible.Render()}'></div>
                            <div id='{grid.IdHtmlTabla}-graficos' class='{enumCssGrid.ContenedorDelGridConElDivDeGraficos.Render()} {enumCssControles.DivNoVisible.Render()}'></div>
                        </div>
                      ";
            var htmlGrid = "";
            if (grid.ZonaDeDatos.EsHistorial)
            {
                htmlGrid = htmlTabla;
            }
            else
            {
                var mostrarElNavegadorEnElGrid = grid.ZonaDeDatos.Mnt.Crud.EsModal && !grid.ZonaDeDatos.Mnt.Crud.MantenimientoSoloConGrid;
                htmlGrid = mostrarElNavegadorEnElGrid ? htmlTabla + RenderNavegadorGrid(grid) : htmlTabla;
            }

            return htmlGrid;
        }

    }
}