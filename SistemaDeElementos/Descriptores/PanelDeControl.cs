using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Entorno;
using ModeloDeDto.Entorno;
using MVCSistemaDeElementos.Controllers;
using ServicioDeDatos;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Seguridad;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilidades;
using UtilidadesParaIu;

namespace MVCSistemaDeElementos.Descriptores
{
    public static class ltrPanelDeControl
    {
        public const string UltimosMenusAccedidos = "[ultimosMenusAccedidos]";
        public const string IdUltimosMenusAccedidos = "ultimos-accesos-menu-1";
        public const string UltimosRegistros = "[ultimosMenusRegistros]";
        public const string IdUltimosRegistros = "ultimos-registros-menu-1";

    }

    public class PanelDeControl : ControlHtml
    {

        ContextoSe Contexto { get; }
        string CuerpoHtml { get; }

        public PanelDeControl(ContextoSe contexto, string id, string cuerpoHtml)
        : this(contexto, id)
        {
            CuerpoHtml = cuerpoHtml;
        }
        public PanelDeControl(ContextoSe contexto, string id)
        : base(null, id, "", "", "", null, resetearListaDeIds: true)
        {
            Contexto = contexto;
        }

        public override string RenderControl()
        {
            return RenderPagina(Contexto, CuerpoHtml.IsNullOrEmpty() ? RenderCuerpo() : CuerpoHtml);
        }


        public string RenderModalCambiarPassword()
        {
            var idHtml = $"modal-cambiar-password";
            var eventos = $"Crud.{enumGestorDeEventos.EventosModalDeEdicion}";
            Dictionary<string, object> otros = new Dictionary<string, object>();

            otros[EventosModal.TrasAbrir] = $"javascript: {enumNameSpaceTs.ApiDePassword}.{enumFunctionTs.InicializarModalCambiarPassword}('{idHtml}')";
            var htmlModal = RenderizarModal(enumTipoDeModal.ModalCambiarPassword,
                idHtml: idHtml
                , controlador: nameof(UsuariosController)
                , tituloH2: "Cambiar la password"
                , cuerpo: DescriptorDeEdicion<MiCertificadoDto>.RenderContenedorDeEdicionCuerpo(this, typeof(CambiarPasswordDto), idHtml, nameof(UsuariosController), null, null)
                , idOpcion: $"{idHtml}-cambiar"
                , opcion: "Cambiar"
                , accion: $"Javascript: {enumNameSpaceTs.ApiDePassword}.{enumFunctionTs.CambiarPassword}('{idHtml}')"
                , cerrar: $"{eventos}('{eventosDeEdicion.CerrarModal}','{idHtml}')"
                , navegador: ""
                , claseBoton: enumCssOpcionMenu.DeElemento
                , permisosNecesarios: enumModoDeAccesoDeDatos.Consultor
                , otrosAtributos: otros);

            return htmlModal;
        }

        public string RenderModalSubirCertificado()
        {
            var idHtml = $"modal-subir-certificado";
            var eventos = $"{enumNameSpaceTs.Crud}.{enumGestorDeEventos.EventosModalDeEdicion}";

            Dictionary<string, object> otros = new Dictionary<string, object>();
            otros[EventosModal.TrasAbrir] = $"javascript: {enumNameSpaceTs.ApiDeCertificados}.{enumFunctionTs.InicializarModalMiCertificado}('{idHtml}')";
            var htmlModal = RenderizarModal(enumTipoDeModal.ModalMiCertificado,
                idHtml: idHtml
                , controlador: nameof(UsuariosController)
                , tituloH2: "Subir mi certificado"
                , cuerpo: DescriptorDeEdicion<MiCertificadoDto>.RenderContenedorDeEdicionCuerpo(this, typeof(MiCertificadoDto), idHtml, nameof(UsuariosController), null, null)
                , idOpcion: $"{idHtml}-subir"
                , opcion: "Subir"
                , accion: $"Javascript: {enumNameSpaceTs.ApiDeCertificados}.{enumFunctionTs.SubirMiCertificado}('{idHtml}')"
                , cerrar: $"{eventos}('{eventosDeEdicion.CerrarModal}','{idHtml}')"
                , navegador: ""
                , claseBoton: enumCssOpcionMenu.DeElemento
                , permisosNecesarios: enumModoDeAccesoDeDatos.Consultor
                , otrosAtributos: otros);

            return htmlModal;
        }

        public string RenderModalIa()
        {
            var htmlModal = "";

            return htmlModal;
        }

        public static string RenderPagina(ContextoSe contexto, string cuerpo, string claseAdicional = null)
        {
            var paginaDeConsulta = claseAdicional == enumCssCuerpo.CuerpoSoloConsulta.Render();
            var pagina =   $@"
            <div class='pagina'>
                  <div id='cabecera-de-pagina' class='cabecera'>
                    {(paginaDeConsulta ? RenderCabeceraDeConsulta (contexto): RenderCabeceraDePagina(contexto))}
                  </div>
                  <div id='cuerpo-de-pagina'  class='{enumCssCuerpo.Cuerpo.Render()}{(claseAdicional.IsNullOrEmpty() ? "" : $" {claseAdicional}")}'>
                    {cuerpo}
                  </div>
                  <div id = 'pie-de-pagina' class='pie'>                     
                    {RenderPieDePagina(contexto)}
                  </div>
            </div>

            <div id='CapaDeBloqueo' class='sin-capa-de-bloqueo'>
               <span id=""TextoContador""></span>
            </div>
            ";
            return pagina;
        }

        private static string RenderCabeceraDePagina(ContextoSe contexto)
        {
            var cabeceraDePagina = $@"
             <div class='cabecera-menu'>
               <img id ='id-menu'
                    src='/images/menu/bars-solid.svg' alt='desplegar menú' 
                    class='btn-menu fa-icono'
                    modal-menu='id-modal-menu'
                    onclick='ArbolDeMenu.MostrarMenu()'
                    menu-abierto='false' />    

                <input id='id-producto' type='text' value='Sistema de elementos' 
                    onclick='ArbolDeMenu.MostrarMenu()'
                    readonly />
            </div>
            <div class='cabecera-separador'></div>
            <!-- Button Container: Favoritos & About -->
            <div class='contenedor-menu-derecho-cabecera'>
                <!-- Ultimos registros Button -->
                {MenuDeUltimosRegistros(contexto)}
                <!-- Ultimos accesos Button -->
                {MenuDeUltimosAccesos(contexto)}
                <!-- Favoritos Button -->
                {MenuDeFavoritos(contexto)}
                <!-- About Button with Menu -->
                {MenuDeAbout(contexto)}
            </div> 
            ";

            return cabeceraDePagina;
        }

        private static string RenderCabeceraDeConsulta(ContextoSe contexto)
        {
            var cabeceraDePagina = $@"
             <div class='cabecera-menu'>
               <img id ='id-menu'
                    src='/images/menu/bars-solid.svg' alt='desplegar menú' 
                    class='btn-menu fa-icono'
                    modal-menu='id-modal-menu'
                    menu-abierto='false' />    

                <input id='id-producto' type='text' value='Sistema de elementos' 
                       readonly />
            </div>
            <div class='cabecera-separador'></div>
            <div class='contenedor-menu-derecho-cabecera'>
            </div> 
            ";

            return cabeceraDePagina;
        }

        private static string MenuDeUltimosRegistros(ContextoSe contexto)
        {
            var menu = CabeceraDeMenu(id: "opcion-registros", clase: "btn-ultimos-registros", onclick: "EntornoSe.MostarUltimosRegistros()", imagen: "UltimosRegistros.png", ayuda: "últimos registros") +
                 $@"<div id='contenedor-menu-ultimos-registros'  class='menu-pnlctr-oculto contenedor-menu-ultimos-registros' >     
                      {ltrPanelDeControl.UltimosRegistros}            
                    </div>";


            var registros = GestorDeAccesosRecientes.LeerAccesosRecientes(contexto, enumClaseDeAcceso.Registros);
            var opciones = $@"<ul id='{ltrPanelDeControl.IdUltimosRegistros}'>
                                  {string.Join(Environment.NewLine, registros.Select(accesos => accesos.OpcionHtml))}
                              </ul>";

            menu = menu.Replace(ltrPanelDeControl.UltimosRegistros, opciones);

            return menu;
        }

        private static string MenuDeAbout(ContextoSe contexto)
        {
            var iaUsada = ExtensorDeUsuarios.IaUsada(contexto);
            var ias = VariableDeMenu.Ias();
            var iasOpciones = "";
            foreach (var ia in ias)
            {
                var clase = enumCssMenuFlotante.IaDisponible.Render();
                if (iaUsada.Enumerado == ia.Enumerado)
                {
                    clase = clase + " " + enumCssMenuFlotante.IaSeleccionada.Render();
                }

                iasOpciones += CrearOpcion(ia.Nombre, $"{enumNameSpaceTs.EntornoSe}.{enumFunctionTs.SeleccionarIa}(this)", "Seleccione la IA que desea usar", nombre: ia.Enumerado.ToString(), claseHref: clase);
            }

            iasOpciones = 
                "<li class='opc-menu-prc-ia' title='Seleccione la IA que desea usar'>" +
                   "<a href='#'>IA</a>" +
                   "<ul class='submenu-ia'>" +
                      iasOpciones +
                   "</ul>" +
                "</li>"
                ;

            var menu = CabeceraDeMenu(id: "opcion-about", clase: "btn-about", onclick: $"{enumNameSpaceTs.EntornoSe}.{enumFunctionTs.MostarOcultarAbout}()", imagen: "About.png", ayuda: "información y cerrar") +
                $@"                <div id='contenedor-menu-about' class='menu-pnlctr-oculto contenedor-menu-about' >
                                    <ul id='about-menu-1'>
                                       {iasOpciones}
                                        <hr>
                                       {CrearOpcion("Veri*fatu", $"{enumNameSpaceTs.EntornoSe}.{enumFunctionTs.DescargarDeclaracionResponsable}()", "Descargar declaración responsable", claseLi: "opc-menu-prc-verfactu")}
                                       {CrearOpcion("Logout", $"{enumNameSpaceTs.EntornoSe}.{enumFunctionTs.Logout}()", "Cerrar sesión", claseLi: "opc-menu-prc-logout")}
                                   </ul>
                               </div>";
            return menu;
        }

        private static string MenuDeUltimosAccesos(ContextoSe contexto)
        {
            var menu = CabeceraDeMenu(id: "opcion-ultimos", clase: "btn-ultimos-accesos", onclick: "EntornoSe.MostarUltimosAccesos()", imagen: "UltimosAccesos3.png", ayuda: "últimos accesos") +
                 $@"<div id='contenedor-menu-ultimos-accesos'  class='menu-pnlctr-oculto contenedor-menu-ultimos-accesos' >     
                      {ltrPanelDeControl.UltimosMenusAccedidos}            
                    </div>";


            var accesos = GestorDeAccesosRecientes.LeerAccesosRecientes(contexto, enumClaseDeAcceso.Menu);
            var opciones = $@"<ul id='{ltrPanelDeControl.IdUltimosMenusAccedidos}'>
                                  {string.Join(Environment.NewLine, accesos.Select(accesos => accesos.OpcionHtml))}
                              </ul>";

            menu = menu.Replace(ltrPanelDeControl.UltimosMenusAccedidos, opciones);

            return menu;
        }

        private static string MenuDeFavoritos(ContextoSe contexto)
        {
            var buzones = ExtensorDeSociedades.PermisosDeBuzones(contexto);
            var menuDeBuzones = ApiDePermisos.TieneAlgunPermiso(contexto, buzones)
                ? CrearOpcion(opcion: "Correo de entrada", evento: "EntornoSe.MiCorreo()", "Acceder al correo del sistema") : "";

            bool? hayFichada = ExtensorDeTrabajadores.HayFichadas(contexto);

            var menuDeFichada = hayFichada is not null
                ? (bool)hayFichada
                ? CrearOpcion(opcion: "Fichar salida", evento: "EntornoSe.Fichar()", "Fichar la salida")
                : CrearOpcion(opcion: "Fichar entrada", evento: "EntornoSe.Fichar()", "Fichar entrada")
                : "";

            var menu = CabeceraDeMenu(id: "opcion-favoritos", clase: "btn-favoritos", onclick: "EntornoSe.MostarOcultarFavoritos()", imagen: "Favoritos.png", ayuda: "mis datos") +
                $@"                <div id='contenedor-menu-favoritos'  class='menu-pnlctr-oculto contenedor-menu-favoritos' >
                                    <ul id='favoritos-menu-1'>
                                       {CrearOpcion("Cambiar contraseña", "EntornoSe.CambiarPassword()", "Cambiar mi contraseña en el SE")}
                                       {menuDeBuzones}
                                       {CrearOpcion("Mi certificado", "EntornoSe.SubirCertificado()", "Subir mi certificado personal para firmar documentos")}
                                       {CrearOpcion("Mi calendario", "EntornoSe.MiCalendario()", "Ver mi agenda")}
                                       {menuDeFichada}
                                   </ul>
                               </div>";
            return menu;

            //{ CrearOpcion("Preguntar", "EntornoSe.AbrirModalIa()", "Conversar con la Ia")}
        }
        private static string CabeceraDeMenu(string id, string clase, string onclick, string imagen, string ayuda)
        {
            return $@"<button id='{id}' class='{clase}' onclick='{onclick}' title='{ayuda}'>
                         <img src='/images/{imagen}' alt='Favoritos' style='width:32px;height:32px;' />
                     </button>";
        }

        private static string CrearOpcion(string opcion, string evento, string ayuda, string claseLi = "", string claseHref = "", string nombre = "")
        {
            return $@"<li {(nombre.IsNullOrEmpty() 
                ? "" 
                : $"name='{nombre}' ")}{(claseLi.IsNullOrEmpty() ? "" 
                                         : $"class='{claseLi}' ")}title='{ayuda}'><a href='#' onclick='{evento}; return false' {(claseHref.IsNullOrEmpty() ? "" 
                                         : $"class='{claseHref}' ")}>{opcion}</a></li>";
        }

        private static string RenderPieDePagina(ContextoSe contexto)
        {
            var piePagina = $@"
            <div class='pie-mensaje'>
                <div class='pie-mensaje-contenedor-hitorial'>
                    <img id='id-abrir-historial'
                         src='/images/menu/bars-solid.svg'
                         class='btn-menu fa-icono'
                         modal-menu='id-modal-historial'
                         onclick='MensajesSe.MostrarMensajes()'
                         historial-abierto='false' />
                </div>
                <div class='pie-mensaje-contenedor'>
                    <input id='Mensaje' readonly value='' />
                </div>
                <div class='pie-mensaje-contenedor-borrado'>
                    <img id='id-borrar-mensaje'
                         src='/images/menu/papelera.svg'
                         style='width:32px; height:22px; '
                         class='btn-menu'
                         onclick='MensajesSe.Sacar()' />
                </div>
            </div>
            <div class='pie-conexion'>
                <div class='pie-conexion-datos'>
                    <input id='i-pie-conexion-datos' readonly value='{contexto.DatosDeConexion.ServidorWeb} - {contexto.DatosDeConexion.ServidorBd}.{contexto.DatosDeConexion.Bd}' />
                </div>
                <div class='pie-conexion-usuario'>
                    <input id='i-pie-conexion-usuario' readonly value='Version:{contexto.DatosDeConexion.Version} - Usuario: {contexto.DatosDeConexion.Login}' />
                </div>
            </div>
            ";

            return piePagina;
        }

        private static string RenderCuerpo()
        {
            //            return $@"  
            //    <div class='cuerpo-cabecera'>cabecera</div>
            //    <div class='cuerpo-datos'>
            //      <div class='cuerpo-datos-filtro'>filtro</div>
            //      <div class='cuerpo-datos-grid'>grid</div>
            //    </div>
            //    <div class='cuerpo-pie'>pie</div>
            //    <div class='cuerpo-creacion'>creacion</div>
            //";
            return "";
        }
    }
}
