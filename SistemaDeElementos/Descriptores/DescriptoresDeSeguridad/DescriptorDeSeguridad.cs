using System;
using ModeloDeDto;
using ModeloDeDto.Entorno;
using ModeloDeDto.Seguridad;
using MVCSistemaDeElementos.Controllers;
using ServicioDeDatos;
using Utilidades;
using UtilidadesParaIu;

namespace MVCSistemaDeElementos.Descriptores
{
    /// <summary>
    /// Vista de la seguridad de un usuario: selector de usuario arriba, árbol de permisos a la izquierda y,
    /// separado por un splitter, los datos en consulta del puesto, rol o permiso seleccionado a la derecha
    /// </summary>
    public class DescriptorDeSeguridad : ControlHtml
    {
        public const string IdArbol = "div_arbol_de_permisos";
        public const string IdDatos = "div_datos_del_objeto";
        public const string CssCuerpo = "cuerpo-de-seguridad";

        public ContextoSe Contexto { get; }
        public string Titulo => Etiqueta;
        private ListasDinamicas<UsuarioDto> SelectorDeUsuario { get; }

        public DescriptorDeSeguridad(ContextoSe contexto)
        : base(padre: null, id: "seguridad", etiqueta: "Seguridad de usuarios", propiedad: "", ayuda: "", posicion: null, resetearListaDeIds: true)
        {
            Contexto = contexto;

            SelectorDeUsuario = new ListasDinamicas<UsuarioDto>(this,
                                    etiqueta: "Usuario",
                                    filtrarPor: nameof(NodoDeSeguridadDto.IdUsuario),
                                    ayuda: "Seleccione el usuario",
                                    seleccionarDe: nameof(UsuarioDto),
                                    buscarPor: nameof(UsuarioDto.Nombre),
                                    mostrarExpresion: $"[{nameof(UsuarioDto.NombreCompleto)}]",
                                    criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                                    posicion: new Posicion(0, 0),
                                    controlador: nameof(UsuariosController),
                                    navegarA: nameof(UsuariosController.CrudUsuario),
                                    restringirPor: "",
                                    alSeleccionarBlanquearControl: "")
            {
                LongitudMinimaParaBuscar = 1,
                TrasSeleccionar = $"{enumNameSpaceTs.Seguridad}.UsuarioSeleccionado(this)"
            };
        }

        public override string RenderControl()
        {
            try
            {
                return PanelDeControl.RenderPagina(Contexto, RenderCuerpo(), CssCuerpo).Render();
            }
            finally
            {
                BlanquearListaDeIds();
            }
        }

        private string RenderCuerpo()
        {
            var controlador = nameof(SeguridadController).Replace(ltrEndPoint.Controller, "");

            return $@"
            <div id='{IdHtml}' class='contenedor-seguridad' controlador='{controlador}'>
                <div id='{IdHtml}-cabecera' class='seguridad-cabecera'>
                    <label for='{SelectorDeUsuario.IdHtml}' class='seguridad-etiqueta'>{SelectorDeUsuario.Etiqueta}:</label>
                    {SelectorDeUsuario.RenderControl()}
                </div>
                <div id='{IdHtml}-cuerpo' class='seguridad-cuerpo'>
                    <div id='{IdArbol}' class='contenedor-arbol seguridad-arbol'>
                        <ul id='{IdHtml}-arbol'></ul>
                    </div>
                    <div id='{IdHtml}-splitter' class='splitter' title='Arrastre para redimensionar'></div>
                    <div id='{IdDatos}' class='seguridad-datos'>
                        <div class='seguridad-titulo-datos'>
                            <a id='{IdHtml}-editar' href='#' class='seguridad-editar {enumCssControles.DivNoVisible.Render()}' title='Editar en una nueva pestaña'>
                                <img src='/images/menu/Editar.png' alt='editar'>
                            </a>
                            <span id='{IdHtml}-titulo-datos'>Seleccione un usuario</span>
                        </div>
                        {RenderPanelDeDatos(enumObjetoDeSeguridad.Puesto, typeof(PuestoDto), nameof(PuestoDeTrabajoController), nameof(PuestoDeTrabajoController.CrudPuestoDeTrabajo))}
                        {RenderPanelDeDatos(enumObjetoDeSeguridad.Rol, typeof(RolDto), nameof(RolController), nameof(RolController.CrudRol))}
                        {RenderPanelDeDatos(enumObjetoDeSeguridad.Permiso, typeof(PermisoDto), nameof(PermisosController), nameof(PermisosController.CrudPermiso))}
                    </div>
                </div>
            </div>
            <script src='../../js/{enumNameSpaceTs.Seguridad}/Seguridad.js?v={DateTime.Now.Ticks}'></script>
            <script>
               try {{
                  {enumNameSpaceTs.Seguridad}.CrearVistaDeSeguridad('{IdHtml}');
               }}
               catch(error) {{
                  MensajesSe.Error('Creando la vista de seguridad', error.message);
               }}
            </script>";
        }

        // los datos del objeto se pintan en modo consulta, igual que en el cuerpo del panel de edición de su crud;
        // pagina-de-edicion es el crud donde se abre el objeto al pulsar el lápiz
        private string RenderPanelDeDatos(enumObjetoDeSeguridad objeto, Type dto, string controlador, string vistaDeEdicion)
        {
            var idHtml = $"{IdHtml}-{objeto.ToString().ToLower()}";
            controlador = controlador.Replace(ltrEndPoint.Controller, "");
            return $@"
                        <div id='{idHtml}' class='seguridad-panel-objeto {enumCssControles.DivNoVisible.Render()}' objeto='{objeto}' pagina-de-edicion='{controlador}/{vistaDeEdicion}'>
                            <div id='contenedor_edicion_cuerpo_{idHtml}' class='{enumCssEdicion.ContenedorDeEdicionCuerpo.Render()}'>
                                {DescriptorDeTabla.htmlRenderObjetoVacio(this, dto, controlador, idHtml, enumCssEdicion.TablaDeEdicion.Render(), enumModoDeTrabajo.Consulta)}
                            </div>
                        </div>";
        }
    }
}
