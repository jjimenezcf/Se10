namespace MVCSistemaDeElementos;

/// <summary>
/// Scripts comunes del front, en el orden en que se cargan. Es la única lista: la usa Startup para generar wwwroot/js/bundle.js
/// (UsarBundle = true) y _Layout.cshtml para pintar un &lt;script&gt; por fichero (UsarBundle = false).
/// El ORDEN importa: los .ts no son módulos, cada fichero declara constantes globales y un fichero solo puede leer, al cargarse,
/// lo que declaran los anteriores (si no, ReferenceError en el navegador; tsc no lo detecta).
/// Los literales (_Literales/literales_*.js) van primero y en el orden de sus dependencias; se puede comprobar con
/// Documentación/ScriptCmd/InstantaneaDeLiterales.js.
/// OJO: las páginas estáticas wwwroot/Acceso/Conectar.html y NuevaContrasena.html (sin layout) repiten a mano la lista de
/// literales: si se añade o quita un literales_*.js, hay que cambiarlas también.
/// </summary>
public static class ListaDeScripts
{
    /// <summary>Rutas relativas a wwwroot/js, con '/' como separador</summary>
    public static readonly string[] Comunes =
    {
        "_Literales/literales_Basicos.js",
        "_Literales/literales_Enumerados.js",
        "_Literales/literales_Controladores.js",
        "_Literales/literales_Atributos.js",
        "_Literales/literales_Controles.js",
        "_Literales/literales_Css.js",
        "_Literales/literales_Ajax.js",
        "_Literales/literales_Eventos.js",
        "_Literales/literales_Propiedades.js",
        "_Literales/literales_Propiedades_Entorno.js",
        "_Literales/literales_Propiedades_SisDoc.js",
        "_Literales/literales_Propiedades_Terceros.js",
        "_Literales/literales_Propiedades_Juridico.js",
        "_Literales/literales_Propiedades_Logistica.js",
        "_Literales/literales_Propiedades_Venta.js",
        "_Literales/literales_Propiedades_Gasto.js",
        "_Literales/literales_Menus.js",
        "_Literales/literales_Svg.js",
        "_Literales/literales_Html.js",
        "Mensajes.js",
        "Diccionario.js",
        "_TiposDeDatos/TiposDeDatos.js",
        "_TiposDeDatos/ModoAcceso.js",
        "Utilidades.js",
        "InfoSelector.js",
        "_Api/ApiLocalStorage.js",
        "Registro.js",
        "Cookies.js",
        "HistorialDeNavegacion.js",
        "_Api/ApiDeAjax.js",
        "_Api/ApiDeAgenda.js",
        "_Api/ApiDeCertificados.js",
        "_Api/ApiDePassword.js",
        "_Api/ApiDeInicializacion.js",
        "_Api/ApiDeArchivos.js",
        "_Api/ApiDragAndDrop.js",
        "_Api/ApiDeListasDinamicas.js",
        "_Api/ApiDePeticiones.js",
        "_Api/ApiFlujo.js",
        "_Api/ApiDeMapeos.js",
        "_Api/ApiDeExpansor.js",
        "_Api/ApiDeGrid.js",
        "_Api/ApiDeControles.js",
        "_Api/ApiDeCriterioDeTexto.js",
        "_Api/ApiPanel.js",
        "_Api/ApiDeMenuFlotante.js",
        "_Api/ApiDeJerarquia.js",
        "_Api/MapearAlPanel.js",
        "_Api/MapearAlControl.js",
        "_Api/ApiDeDireccion.js",
        "_Api/ApiDeGraficos.js",
        "ArbolDeMenu.js",
        "EntornoSe.js",
        "_Formulario/FormularioBase.js",
        "_Formulario/Jerarquia.js",
        "_Crud/ApiDelCrud.js",
        "_Crud/ApiVisorDeArchivos.js",
        "_Crud/ApiConsultaDeArchivos.js",
        "_Crud/ApiPreguntasIa.js",
        "_Crud/CrudBase.js",
        "_Crud/GridDeDatos.js",
        "_Crud/CrudCreacion.js",
        "_Crud/CrudEdicion.js",
        "_Crud/CrudHistorial.js",
        "_Crud/ModalConGrid.js",
        "_Crud/ModalParaRelacionar.js",
        "_Crud/ModalParaImputar.js",
        "_Crud/ModalParaConsultarRelaciones.js",
        "_Crud/ModalParaSeleccionar.js",
        "_Crud/ModalDeSeleccion.js",
        "_Crud/CrudMantenimiento.js",
        "_Crud/GestorDeEventos.js",
        // PanelDeControl usa ApiPreguntasIa (_Crud): va el último
        "PanelDeControl.js"
    };
}
