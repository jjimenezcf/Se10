using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Utilidades;
using ServicioDeDatos.Elemento;
using ServicioDeDatos;

namespace ModeloDeDto
{

    public class IUPropiedadAttribute : Attribute
    {
        private string etiquetaGrid;


        private bool _visibleEnGrid = true;

        private string _ayuda = "";

        public static string NombreConColSpan => "ConSpanEnColumnas";

        private bool _totalizar = true;
        public bool Totalizar
        {
            get
            {
                if (Oculto == true) return false;

                if (Tipo == typeof(int) || Tipo == typeof(int?) || Tipo == typeof(decimal) || Tipo == typeof(decimal?))
                    return _totalizar;

                return false;
            }
            set { _totalizar = value; }
        }

        public string EtiquetaGrid
        {
            get
            {
                if (Oculto == true) return null;

                if (VisibleAlEditar == false && EditableAlCrear == true && VisibleEnGrid == false)
                    return null;

                if (etiquetaGrid is not null)
                    return etiquetaGrid;

                if (!VisibleAlCrear && !VisibleAlEditar && !VisibleAlConsultar && !_visibleEnGrid)
                    return null;

                if (etiquetaGrid is null)
                    return Etiqueta;
                return etiquetaGrid;
            }
            set { etiquetaGrid = value; }
        }

        public string Etiqueta { get; set; } = "";

        public string EtiquetaRef { get; set; } = "";

        public string Ayuda
        {
            get
            {
                return _ayuda.IsNullOrEmpty()
                ? AyudaDeCriteriosDeBusqueda.IsNullOrEmpty()
                ? Etiqueta
                : AyudaDeCriteriosDeBusqueda
                : _ayuda;
            }
            set
            {
                _ayuda = value;
            }
        }

        /// <summary>
        /// se renderiza como control, pero no es visible
        /// </summary>
        public bool Oculto { get; set; } = false;

        /// <summary>
        /// Indica si un control es oculto si solo es para leer información el ts, no hace devolver el valor, ya que dicho valor si es necesario
        /// lo devolverá un control vinculado
        /// </summary>
        public bool SoloParaTs { get; set; } = false;

        /// <summary>
        /// No se renderiza como control, pero si se devuelve en un json tras una petición
        /// </summary>
        public bool Visible
        {
            get { return VisibleAlCrear && VisibleAlEditar && VisibleAlConsultar && _visibleEnGrid; }
            set { VisibleAlCrear = VisibleAlEditar = VisibleAlConsultar = _visibleEnGrid = value; }
        }
        public bool VisibleEnGrid
        {
            get
            {
                if (Oculto) return false;
                return _visibleEnGrid && TipoDeControl != enumTipoControl.UrlDeArchivo && TipoDeControl != enumTipoControl.AreaDeTexto;
            }
            set { _visibleEnGrid = value; }
        }
        public bool VisibleEnEdicion { get { return VisibleAlCrear && VisibleAlEditar && VisibleAlConsultar; } set { VisibleAlCrear = VisibleAlEditar = VisibleAlConsultar = value; } }

        private bool _visibleAlCrear = true;
        public bool VisibleAlCrear { get { return _visibleAlCrear; } set { _visibleAlCrear = value; } }

        private bool _visibleEnVisorDeArchivosAlCrear = true;
        public bool VisibleEnVisorAlCrear { get { return _visibleEnVisorDeArchivosAlCrear; } set { _visibleEnVisorDeArchivosAlCrear = value; } }

        private bool _visibleAlEditar = true;
        public bool VisibleAlEditar 
        { 
            get 
            { 
                return !_visibleAlCrear && EditableAlEditar ? true : _visibleAlEditar;
            } 
            set { _visibleAlEditar = value; } 
        }

        private bool _visibleAlConsultar = true;
        public bool VisibleAlConsultar { get { return !VisibleAlEditar ? false : _visibleAlConsultar; } set { _visibleAlConsultar = value; } }

        private bool _editableAlCrear = true;
        private bool _editableAlEditar = true;
        public bool EditableAlCrear { get { return _visibleAlCrear && _editableAlCrear; } set { _editableAlCrear = value; } }
        public bool EditableAlEditar { get { return _visibleAlEditar && _editableAlEditar; } set { _editableAlEditar = value; } }

        /// <summary>
        /// Si una columna donde se renderiza un control, su columna inmediatamente anterior es no visible, esta propiedad indica se se ha de ocultar o mostrar vacía
        /// </summary>
        public bool MantenerHuecoDeLaIzquierda { get; set; } = false;

        //Nº de filas de un textArea
        public int NumeroDeFilas { get; set; } = 5;

        //Indica si una propiedad es mapeable a un control o sólo se usa para acondicionar los controles del ts, por ejemplo el mododeacceso
        public bool Mapeable { get; set; } = true;

        /// <summary>
        /// Indica si al renderizar el dto, le ha de ocupar más de una columna de la tabla
        /// El número de columnas a ocupar se calcula automáticamente dependiendo de los controles posicionados en la fila
        /// </summary>
        public bool AutoSpan { get; set; } = false;

        /// <summary>
        /// cuando me interesa decir el numero de columnas que va a ocupar el control
        /// </summary>
        public int ColSpan { get; set; } = 0;

        public bool Obligatorio { get; set; } = true;
        public Type Tipo { get; set; } = typeof(string);
        public short Fila { get; set; }
        public short Columna { get; set; }

        public short Posicion { get; set; } = 0;
        public object ValorPorDefecto { get; set; }
        public bool Ordenar { get; set; } = false;
        public bool EsFecha => TipoDeControl == enumTipoControl.SelectorDeFechaHora || TipoDeControl == enumTipoControl.SelectorDeFecha;
        public bool EsDecimal => Tipo == typeof(decimal) || Tipo == typeof(decimal?);

        private enumFormato formato = enumFormato.Sin_Formato;
        public enumFormato Formato
        {
            get
            {
                if (EsDecimal && formato == enumFormato.Sin_Formato) return enumFormato.Numero_2;
                if (EsFecha && formato == enumFormato.Sin_Formato)
                {
                    return TipoDeControl == enumTipoControl.SelectorDeFechaHora
                           ? enumFormato.FechaHoraMinutos
                           : enumFormato.Fecha;
                }
                return formato;
            }
            set
            {
                formato = value;
            }
        }


        public string ExpresionRegular { get; set; }

        public string OrdenarGridPor { get; set; }
        public string OrdenarListaDinamicaPor { get; set; }
        public string AnchoMaximo { get; set; } = null;
        public string AnchoMaximoContenedor { get; set; } = null;


        public int PosicionEnGrid { get; set; } = -1;
        private enumAliniacion _alineada = enumAliniacion.no_definida;
        public enumAliniacion Alineada
        {
            get
            {
                return Formato == enumFormato.Sin_Formato ? _alineada : _alineada != enumAliniacion.no_definida ? _alineada : enumAliniacion.derecha;
            }
            set { _alineada = value; }
        }

        /// <summary>
        /// Sirve para alinear el contenido de una td en la derecha
        /// </summary>
        public enumAliniacion AlinearContenido { get; set; } = enumAliniacion.izquierda;

        public int PorAnchoMnt { get; set; } = 0;
        public int PorAnchoSel { get; set; } = 0;
        public string TamanoFijo { get; set; }

        public enumTipoControl TipoDeControl { get; set; } = enumTipoControl.Editor;

        enumTipoControl? _tipoDeControlEnGrid = null;
        public enumTipoControl TipoDeControlEnGrid { get { return _tipoDeControlEnGrid ?? TipoDeControl; } set { _tipoDeControlEnGrid = value; } }

        /// <summary>
        /// Indica a la interface de usuario que el contenido de este control puede ser almacenado para al crear que lo proponga
        /// </summary>
        public bool EsAlmacenable { get; set; } = false;

        /// <summary>
        /// Indicar los permisos para que un control esté habilitado, por defecto aplica el modo de acceso al elemento, pero
        /// hay situaciones en que interesa un control específico, por ejemplo ser el creador del elemento
        /// </summary>
        public string PermisosNecesarios { get; set; }

        /// <summary>
        /// Indica que al procesar el modo de acceso de un dto, si este no es gestor ha de ocultar el href
        /// </summary>
        public bool EnConsultaOcultar { get; set; } = true;

        /// <summary>
        /// Indica como se ha de mostrar el control, por ejemplo en línea  apilado
        /// </summary>
        public enumCssControles css { get; set; } = enumCssControles.CheckEnLinea;

        public enumCssControles CssDelArea { get; set; } = enumCssControles.Nulo;

        /// <summary>
        /// Indica la css del contenedor de un control
        /// </summary>
        public enumCssControles CssDelContenedor { get; set; } = enumCssControles.Nulo;

        /// <summary>
        /// Dto del que se van a seleccionar los valores
        /// </summary>
        public Type SeleccionarDe { get; set; }

        /// <summary>
        /// Indica el negocio con el que el control va a trabajar en una lista dinámica o en un restrictor si usa navegador
        /// </summary>
        public enumNegocio Negocio { get; set; } = enumNegocio.No_Definido;

        /// <summary>
        /// sirve para indicar como a de filtrar el Gestor de negocio.
        /// En el caso de estar aplicado a un Dto que implementa la interface IRelacion, si es diferente mostrará todos los registros relacionables que no lo estén ya
        /// </summary>
        public enumCriteriosDeFiltrado CriterioDeBusqueda { get; set; } = enumCriteriosDeFiltrado.contiene;

        private string _ayudaDeCriteriosDeBusqueda = null;
        /// <summary>
        /// Indica coomo se ha de buscar en una lista dinámica
        /// </summary>
        public string AyudaDeCriteriosDeBusqueda
        {
            get
            {
                return _ayudaDeCriteriosDeBusqueda.IsNullOrEmpty()
                    ? _ayuda
                    : _ayudaDeCriteriosDeBusqueda;
            }
            set
            {
                _ayudaDeCriteriosDeBusqueda = value;
            }
        }

        public string GuardarEn { get; set; }

        /// <summary>
        /// Propiedad que sirve para indicar:
        /// En una lista dinámica si los valores a cargar vienen restringidos por el valor de otro control del tipo:
        ///    Lista dinámica restringida por un restrictor de edición --> ej: al seleccionar una CP para una calle RestringidoPorControl = nameof(IdCalle)
        ///    Lista dinamica restringida por otra lista dinámica --> ej: al seleccionar una provincia Restringimos por la listaDinamica = nameof(Pais)
        ///    Lista de elementos restringida por un restrictor --> ej: Lotes de un planificador de ventas restringido por el contrato
        /// </summary>
        public string RestringidoPorControl { get; set; } = "";


        /// <summary>
        /// Le indica al sistema que no se ha de inicializar la lista de elementos mientras no se le indique por parte de un evento
        /// </summary>
        public bool CargarBajoDemanda { get; set; } = false;

        /// <summary>
        /// Casos:
        /// En un restrictor: Propiedad que me indica como en un restricctor cual es el id de la propiedad que restringe
        /// En una lista dinámica: Indica que filtro he de mandar para restringir la selección, ejemplo. Codigos postales de una calle
        /// p.e: PropiedadRestrictora = nameof(IdCalle) le indico al gestor que fitre por el valor de idCalle
        /// En un restrictor de edición, indica la propieda con la se le asigna el valor del dto
        /// </summary>
        public string PropiedadRestrictora { get; set; } = "";

        /// <summary>
        /// Sirve para indicar como actua el filtro del restrictor, lo normal es que al seleccionar muestre aquellos que cumplen con la propiedad restrictora, pero en algunos casos
        /// como pasa con los unitarios de un lote, al seleccionar el unitario por nombre lo que se desea es que muestre los unitarios del maestro que no estén relacionados con el lote
        /// para ello hay que poner el CriterioDeLRestrictor el que se indique y no el de por defecto que es igual. ver como se compota la lista de unitarios en una línea de un 
        /// planificador de ventas o como se comporta al seleccionar unitarios para incluirlos en un lote
        /// </summary>
        public enumCriteriosDeFiltrado CriterioDelRestrictor { get; set; } = enumCriteriosDeFiltrado.igual;


        /// <summary>
        /// propiedad que permite restringir en un control (lista dinámica por conjunto de cláusulas y un valor fijo) por ejemeplo
        /// Restringir por modo de acceso al negocio más la etapa del elemento -->
        /// Negocio;Archivador;Gestor + "|" + 
        /// ltrDeUnParteTr.FiltroPorEtapa + ";" + nameof(enumEtapasDePartesTr.PTR_Etapa_Pdt_Facturar) ,
        /// 
        /// indica que se selecciona del negocio archivador y con permisos de gestor
        /// </summary>
        public string RestrictorFijo { get; set; } = "";

        /// <summary>
        /// Parámetros de navegación que se han de añadir a las url de los navegadores, por ejemplo para navegar a un contrato se añade a la url la clase "clase=venta"
        /// </summary>
        public string ParametrosParaNavegar { get; set; } = "";

        /// <summary>
        /// propiedad que permite restringir en una lista dinámica si son válidos los elementos de baja
        /// </summary>
        public bool SoloEnAlta { get; set; } = true;

        public string Controlador { get; set; } = "";

        /// <summary>
        /// Sirve para los selectores de fecha y hora le indiquen al sistema con qué otros selectores se vinculen y los actualicen al perder el foco.
        /// Por ejemplo: 
        /// nameof(PlfDeFin)+":0:1" --> indica que al perder el foco en el día sume 0 días a plfDeFin y 1 hora a la hora de PlfDeFin
        /// nameof(PlfDeFin)+":7"   --> indica que al perder el foco en el día sume 7 días a plfDeFin
        /// </summary>
        public string SelectorHasta { get; set; }

        /// <summary>
        /// Al seleccionar un valor en la lista desplegable se blanquea el control indicado, por ejemplo, se selecciona un pais, y se blanquea la provincia
        /// </summary>
        public string AlSeleccionarBlanquearControl { get; set; } = "";

        /// <summary>
        /// Indica, que cuando se sale de una lista desplegable, si no se ha encontrado valor, deja lo escrito para que lo cree el servicio tras enviar el dto
        /// </summary>
        public bool BlanquearAlSalir { get; set; } = true;

        public string BuscarPor { get; set; } = ltrFiltros.Nombre;
        public int LongitudMinimaParaBuscar { get; set; } = 3;
        public bool AplicarJoin { get; set; } = false;

        public string MostrarExpresion { get; set; }

        public string VistaDondeNavegar { get; set; } = "";

        public bool ConLink { get; set; }

        /// <summary>
        /// Indica si hay un botón al lado del editor para que al pulsar se ejecute un javascript 
        /// </summary>
        public bool ConAccion => CssBotonAccion != enumCssControles.Nulo || !OnClick.IsNullOrEmpty();

        /// <summary>
        /// indica la css que se le aplicará al botón
        /// </summary>
        public enumCssControles CssBotonAccion { get; set; } = enumCssControles.Nulo;

        public bool CargaDinamica => TipoDeControl == enumTipoControl.ListaDinamica;

        public string UrlDelArchivo { get; set; }

        /// <summary>
        /// Estilo a aplicar a la TD que contiene al control en la Tabla del RenderDto
        /// </summary>
        public string DisplaCssDelTd { get; set; }

        public enumCssDiv CssDelDivDeLaTd { get; set; } = enumCssDiv.Nulo;

        public enumCssGrid CssDeLaColumna { get; set; } = enumCssGrid.Nulo;

        public int LimiteEnByte { get; set; } = 3000000;
        public string ExtensionesValidas { get; set; } = "*.*";
        public object RutaDestino { get; set; }

        public bool EsVisible(enumModoDeTrabajo modo)
        {
            if (enumTipoControl.ImagenDelCanvas == TipoDeControl)
                return false;

            if (Visible)
                return true;

            if (modo == enumModoDeTrabajo.Jerarquia)
                return VisibleEnEdicion;

            if (modo == enumModoDeTrabajo.Edicion)
                return VisibleAlEditar;

            if (modo == enumModoDeTrabajo.Nuevo || modo == enumModoDeTrabajo.NuevaRelacion)
                return VisibleAlCrear;

            if (modo == enumModoDeTrabajo.Consulta)
                return VisibleAlConsultar;

            if (modo == enumModoDeTrabajo.Mantenimiento)
                return VisibleEnGrid && !Oculto;

            return false;
        }
        public bool EsEditable(enumModoDeTrabajo modo)
        {
            if (enumTipoControl.RestrictorDeEdicion == TipoDeControl)
                return false;

            if (EsVisible(modo))
            {
                if (modo == enumModoDeTrabajo.Jerarquia && !EsVisible(enumModoDeTrabajo.Nuevo))
                    return EditableAlEditar;
                else
                if (modo == enumModoDeTrabajo.Edicion)
                    return EditableAlEditar;
                else
                if (modo == enumModoDeTrabajo.Nuevo || modo == enumModoDeTrabajo.Jerarquia || modo == enumModoDeTrabajo.NuevaRelacion)
                    return EditableAlCrear;
            }

            return false;
        }

        public int LongitudMaxima { get; set; }

        /// <summary>
        /// Al pulsar una tecla en un editor de javascript llama a un método que debe estar definido en el módulo
        /// </summary>
        public string OnKeyPress { get; set; }
        /// <summary>
        /// Al entrar en un campo del tipo editor que evento se ha de lanzar
        /// </summary>
        public string OnFocus { get; set; }
        /// <summary>
        /// Al salir de un campo del tipo editor que evento se ha de lanzar
        /// </summary>
        public string OnBlur { get; set; }

        /// <summary>
        /// Acción que se dispara al cambiar un check o lista de valores
        /// </summary>
        public string OnChange { get; set; }

        /// <summary>
        /// Acción que se dispara al inicializar una lista de valores con valor null
        /// </summary>
        public string OnReset { get; set; }

        /// <summary>
        /// Evento que se dispara al copiar en un input box
        /// </summary>
        public string OnPaste { get; set; }

        /// <summary>
        /// Acción que se dispara al pulsar en el control
        /// </summary>
        public string OnClick { get; set; }

        /// <summary>
        /// Tras seleccionar un valor en una lista dinámica se evalua el js asociado
        /// </summary>
        public string trasSeleccionar { get; set; }

        /// <summary>
        /// Tras blanquear la lista dinámica se evalua el js asociado
        /// </summary>
        public string trasBlanquear { get; set; }

        /// <summary>
        /// Antes de lanzar la petición ajax para leer elementos (epLeerElementos) se ejecuta este método en el cliente y nos permite añadir filtros y párametros
        /// para filtrar en la lectura
        /// </summary>
        public string antesDeBuscar { get; set; }


        /// <summary>
        /// Tras cargar los elementos de un control (por ejemplo una lista de elemento, al finalizar la llamada ajax que la carga)
        /// </summary>
        public string TrasCargar { get; set; }

        /// <summary>
        /// Acción que se dispara al pulsar una referencia Web
        /// </summary>
        public string AccionRef { get; set; }

        /// <summary>
        /// Si solo hay un registro en la lista desplegable, lo fija
        /// </summary>
        public bool AutoPosicionamiento { get; set; } = false;
    }

    public class IUDtoAttribute : Attribute
    {
        /// <summary>
        /// Ancho que se les da a las etiquetas en la iu
        /// </summary>
        public short AnchoEtiqueta { get; set; } = 15;

        /// <summary>
        /// Separación entre la etiqueta y el control que muestra el dato
        /// </summary>
        public short AnchoSeparador { get; set; } = 2;

        private bool _opcionCrear = true;
        public bool OpcionDeCrear { get { return SoloGrid ? false : _opcionCrear; } set { _opcionCrear = value; } }

        private bool _opcionEditar = true;
        public bool OpcionDeEditar { get { return SoloGrid ? false : _opcionEditar; } set { _opcionEditar = value; } }

        private bool _opcionBorrar = true;
        public bool OpcionDeBorrar { get { return SoloGrid ? false : _opcionBorrar; } set { _opcionBorrar = value; } }

        public bool SoloGrid { get; set; } = false;

        public bool ConMfs { get; set; } = true;

        bool _opcionDeExportar = true;
        public bool OpcionDeExportar { get { return ConMfs && _opcionDeExportar; } set { _opcionDeExportar = value; } }

        bool _opcionDeEnviar = true;
        public bool OpcionDeEnviar { get { return ConMfs && _opcionDeEnviar; } set { _opcionDeEnviar = value; } }

        bool _opcionDeTransitar = false;
        public bool OpcionDeTransitar { get { return ConMfs && _opcionDeTransitar; } set { _opcionDeTransitar = value; } }

        public bool EditarTrasCrear { get; set; } = false;

        private string _mostrarExpresion = $"[{nameof(INombre.Nombre)}]";
        public string MostrarExpresion
        {
            get { return _mostrarExpresion; }
            set
            {
                _mostrarExpresion = value;
                if (!_mostrarExpresion.Contains('[') && !_mostrarExpresion.Contains(']'))
                    _mostrarExpresion = $"[{_mostrarExpresion}]";
            }
        }

    }

    public static class ApiDeAtributos
    {

        public static IUPropiedadAttribute ObtenerAtributos(PropertyInfo propiedad, List<PropiedaJson> propiedadesJsons)
        {

            List<Attribute> listaAtrb = null;

            var iEnumerableAtrb = propiedad.GetCustomAttributes(typeof(IUPropiedadAttribute));
            if (iEnumerableAtrb == null || iEnumerableAtrb.ToList().Count == 0)
                Gestor.Errores.GestorDeErrores.Emitir($"No se puede definir el descriptor para el tipo {propiedad.DeclaringType} " +
                    $"por no tener definidas los atributos {typeof(IUPropiedadAttribute)} para la propiedad {propiedad.Name}");

            listaAtrb = iEnumerableAtrb.ToList();

            if (listaAtrb.Count != 1)
                Gestor.Errores.GestorDeErrores.Emitir($"No se puede definir el descriptor para el tipo {propiedad.DeclaringType} por tener mas de una definición para {typeof(IUPropiedadAttribute)}");

            var atributos = (IUPropiedadAttribute)propiedad.GetCustomAttributes(typeof(IUPropiedadAttribute)).ToList()[0];
            if (propiedadesJsons != null)
            {
                var pJson = propiedadesJsons.Find(x => x.propiedad.Equals(propiedad.Name, StringComparison.CurrentCultureIgnoreCase));
                if (pJson != null)
                {
                    if (pJson.posicion != null)
                    {
                        atributos.Columna = (short)pJson.posicion.columna;
                        atributos.Fila = (short)pJson.posicion.fila;
                    }
                    if (pJson.PosicionEnElDiv > 0)
                        atributos.Posicion = pJson.PosicionEnElDiv;

                    if (pJson.autospan != null)
                        atributos.AutoSpan = (bool)pJson.autospan;

                    if (pJson.colspan != null && (pJson.autospan == null || (bool)pJson.autospan == false))
                    {
                        atributos.AutoSpan = false;
                        atributos.ColSpan = (short)pJson.colspan;
                    }

                    if (!pJson.css.IsNullOrEmpty())
                    {
                        atributos.css = ApiDeEnsamblados.ToEnumerado<enumCssControles>(pJson.css);
                    }

                    if (!pJson.ordenarGridPor.IsNullOrEmpty()) atributos.OrdenarGridPor = pJson.ordenarGridPor;

                    if (pJson.PosicionEnGrid > -1) atributos.PosicionEnGrid = pJson.PosicionEnGrid;

                    if (pJson.PorAnchoMnt > 0) atributos.PorAnchoMnt = pJson.PorAnchoMnt;
                    if (!pJson.TamanoFijo.IsNullOrEmpty())
                    {
                        atributos.PorAnchoMnt = 0;
                        atributos.TamanoFijo = pJson.TamanoFijo;
                    }

                    if (!pJson.SeleccionarDe.IsNullOrEmpty()) atributos.SeleccionarDe = ApiDeEnsamblados.ObtenerType(ApiDeEnsamblados.DllDelModeloDeDto, pJson.SeleccionarDe);
                    if (!pJson.EtiquetaGrid.IsNullOrEmpty())
                        atributos.EtiquetaGrid = pJson.EtiquetaGrid;
                    if (!pJson.Etiqueta.IsNullOrEmpty()) atributos.Etiqueta = pJson.Etiqueta;
                    if (!pJson.Controlador.IsNullOrEmpty()) atributos.Controlador = pJson.Controlador;
                    if (!pJson.VistaDondeNavegar.IsNullOrEmpty()) atributos.VistaDondeNavegar = pJson.VistaDondeNavegar;
                    if (!pJson.RestrictorFijo.IsNullOrEmpty()) atributos.RestrictorFijo = pJson.RestrictorFijo;
                    if (!pJson.RestrictorFijo.IsNullOrEmpty()) atributos.RestrictorFijo = pJson.RestrictorFijo;
                    if (!pJson.Negocio.IsNullOrEmpty()) atributos.Negocio = ApiDeEnsamblados.ToEnumerado<enumNegocio>(pJson.Negocio);
                    if (pJson.VisibleAlCrear != default) atributos.VisibleAlCrear = (bool)pJson.VisibleAlCrear;
                    if (pJson.VisibleAlEditar != default) atributos.VisibleAlEditar = (bool)pJson.VisibleAlEditar;
                    if (pJson.VisibleAlConsultar != default) atributos.VisibleAlConsultar = (bool)pJson.VisibleAlConsultar;
                    if (pJson.VisibleEnGrid != default) atributos.VisibleEnGrid = (bool)pJson.VisibleEnGrid;
                    if (pJson.EditableAlCrear != default) atributos.EditableAlCrear = (bool)pJson.EditableAlCrear;
                    if (pJson.EditableAlEditar != default) atributos.EditableAlEditar = (bool)pJson.EditableAlEditar;
                    if (!pJson.trasSeleccionar.IsNullOrEmpty()) atributos.trasSeleccionar = pJson.trasSeleccionar;
                    if (!pJson.trasBlanquear.IsNullOrEmpty()) atributos.trasBlanquear = pJson.trasBlanquear;
                    if (pJson.Formato == enumFormato.Sin_Formato) atributos.Formato = pJson.Formato != null ? (enumFormato)pJson.Formato : enumFormato.Sin_Formato;
                    if (!pJson.AnchoMaximo.IsNullOrEmpty()) atributos.AnchoMaximo = pJson.AnchoMaximo;
                    if (pJson.EsAlmacenable != default) atributos.EsAlmacenable = (bool)pJson.EsAlmacenable;
                    if (!pJson.Ayuda.IsNullOrEmpty())
                        atributos.Ayuda = pJson.Ayuda;
                }
            }

            return atributos;
        }


        public static string Titulo(PropertyInfo propiedad, IUPropiedadAttribute atributos)
        {
            var titulo =
            atributos.TipoDeControl == enumTipoControl.Editor && atributos.Etiqueta.ToLower().StartsWith("id ") ||
                (atributos.TipoDeControl == enumTipoControl.RestrictorDeEdicion && (propiedad.PropertyType == typeof(int) || propiedad.PropertyType == typeof(int?))) ||
                atributos.TipoDeControl == enumTipoControl.ReferenciaPost ||
                atributos.TipoDeControl == enumTipoControl.SelectorDeArchivos ||
                atributos.TipoDeControl == enumTipoControl.SelectorDeUnArchivo ||
               (atributos.TipoDeControl == enumTipoControl.ListaDeElemento && atributos.CargarBajoDemanda)
            ? ""
            : atributos.EtiquetaGrid;
            return titulo;
        }

        public static Dictionary<string, object> PropiedadesDeDtoNoMapeables(Type tipo)
        {
            var propiedadesNoMapeablesAlDto = new List<string>();
            var propiedadesJson = ApiClasesComunes.ObtenerAtributosJson(tipo, enumRutas.RutaDeJson, ServicioDeCaches.UsaCacheParaRenderizar);
            foreach (var p in tipo.GetProperties())
            {
                IUPropiedadAttribute atributos = ObtenerAtributos(p, propiedadesJson);
                if (!p.Name.Equals(nameof(ElementoDto.Id)) && !atributos.Mapeable)
                    propiedadesNoMapeablesAlDto.Add(p.Name.ToLower());
            }

            var otrosAtributos = new Dictionary<string, object> { { "propiedades-no-mapeables", $"{propiedadesNoMapeablesAlDto.ToString("; ")}" } };
            return otrosAtributos;
        }

        public static object ValorDelAtributo(Type clase, string nombreAtributo, bool obligatorio = true, bool emitirError = true)
        {
            Attribute[] atributosDeDto = System.Attribute.GetCustomAttributes(clase);

            if (atributosDeDto == null || atributosDeDto.Length == 0)
            {
                if (!emitirError)
                    return null;

                Gestor.Errores.GestorDeErrores.Emitir($"No hay definido descriptores {nameof(IUDtoAttribute)} para el dto {clase.Name}");
            }

            foreach (Attribute propiedad in atributosDeDto)
            {
                if (propiedad is IUDtoAttribute)
                {
                    IUDtoAttribute a = (IUDtoAttribute)propiedad;
                    switch (nombreAtributo)
                    {
                        case nameof(IUDtoAttribute.AnchoEtiqueta):
                            return a.AnchoEtiqueta;

                        case nameof(IUDtoAttribute.AnchoSeparador):
                            return a.AnchoSeparador;

                        case nameof(IUDtoAttribute.MostrarExpresion):
                            return a.MostrarExpresion;

                        case nameof(IUDtoAttribute.OpcionDeCrear):
                            return a.OpcionDeCrear;

                        case nameof(IUDtoAttribute.EditarTrasCrear):
                            return a.EditarTrasCrear;

                        case nameof(IUDtoAttribute.OpcionDeBorrar):
                            return a.OpcionDeBorrar;

                        case nameof(IUDtoAttribute.OpcionDeEditar):
                            return a.OpcionDeEditar;

                        case nameof(IUDtoAttribute.OpcionDeExportar):
                            return a.OpcionDeExportar;

                        case nameof(IUDtoAttribute.OpcionDeEnviar):
                            return a.OpcionDeEnviar;

                        case nameof(IUDtoAttribute.SoloGrid):
                            return a.SoloGrid;

                        case nameof(IUDtoAttribute.ConMfs):
                            return a.ConMfs;
                    }
                    if (obligatorio)
                        throw new Exception($"Se ha solicitado el atributo {nameof(IUDtoAttribute)}.{nombreAtributo} de la clase {clase} y no está definido");
                }
            }

            return null;

        }

        public static List<PropertyInfo> PropiedadesDelTipo(this Type tipo, enumTipoControl tipoControl)
        {
            return tipo.GetProperties()
                .Where(prop => prop.GetCustomAttributes(typeof(IUPropiedadAttribute), true)
                    .OfType<IUPropiedadAttribute>()
                    .Any(attr => attr.TipoDeControl == tipoControl))
                .ToList();
        }
    }
}
