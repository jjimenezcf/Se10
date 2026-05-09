using ModeloDeDto.Expediente;
using ModeloDeDto.Gastos;
using ModeloDeDto.RegistroEs;
using ModeloDeDto.SistemaDocumental;
using ModeloDeDto.Tarea;
using ServicioDeDatos.Seguridad;
using Utilidades;

namespace ModeloDeDto.Entorno
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5)]
    public class SeleccionarComoVincularDto : ISelectorDto
    {
        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(Visible = false, Etiqueta = "Id del mensaje de correo referenciado")]
        public int IdElemento { get; set; }

        [IUPropiedad(Visible = false)]
        public string Elemento { get; set; }

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
         Etiqueta = "Id de la tarea seleccionada", Visible = false)]
        public int IdTarea { get; set; }

        [IUPropiedad(
            Etiqueta = "Tarea",
            Ayuda = "seleccione la tarea dónde anexar el correo",
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(IdTarea),
            Controlador = nameof(enumControladoresAdministrativos.Tareas),
            SeleccionarDe = typeof(TareaDto),
            VistaDondeNavegar = enumVistasAdministrativo.CrudTareas,
            RestrictorFijo = ltrParametrosDto.Negocio + ";" + nameof(enumNegocio.Tarea) + ";" + nameof(enumModoDeAccesoDeDatos.Gestor),
            Negocio = enumNegocio.Tarea,
            MostrarExpresion = nameof(TareaDto.Expresion),
            LongitudMinimaParaBuscar = 1,
            Tipo = typeof(string),
            Fila = 0,
            Columna = 0,
            trasBlanquear = "javascript:" + nameof(enumNameSpaceTs.Entorno) + "." + nameof(enumFunctionTs.MiCorreo_Tras_Blanquear_Tarea) + "([" + nameof(enumParamTs.idLista) + "])",
            trasSeleccionar = "javascript:" + nameof(enumNameSpaceTs.Entorno) + "." + nameof(enumFunctionTs.MiCorreo_Tras_Seleccionar_Tarea) + "([" + nameof(enumParamTs.idLista) + "])",
            AutoSpan = true)]
        public string Tarea { get; set; }

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
         Etiqueta = "Id del registro de E/S seleccionado", Visible = false)]
        public int IdRegistroEs { get; set; }

        [IUPropiedad(
            Etiqueta = "RegistroEs",
            Ayuda = "seleccione el registro dónde anexar el correo",
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(IdRegistroEs),
            Controlador = nameof(enumControladoresAdministrativos.RegistrosEs),
            SeleccionarDe = typeof(RegistroEsDto),
            VistaDondeNavegar = enumVistasAdministrativo.CrudRegistrosEs,
            RestrictorFijo = ltrParametrosDto.Negocio + ";" + nameof(enumNegocio.Registro) + ";" + nameof(enumModoDeAccesoDeDatos.Gestor),
            Negocio = enumNegocio.Registro,
            MostrarExpresion = nameof(RegistroEsDto.Expresion),
            LongitudMinimaParaBuscar = 1,
            Tipo = typeof(string),
            Fila = 0,
            Columna = 1,
            trasBlanquear = "javascript:" + nameof(enumNameSpaceTs.Entorno) + "." + nameof(enumFunctionTs.MiCorreo_Tras_Blanquear_RegistroEs) + "([" + nameof(enumParamTs.idLista) + "])",
            trasSeleccionar = "javascript:" + nameof(enumNameSpaceTs.Entorno) + "." + nameof(enumFunctionTs.MiCorreo_Tras_Seleccionar_RegistroEs) + "([" + nameof(enumParamTs.idLista) + "])",
            AutoSpan = true)]
        public string RegistroEs { get; set; }

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
         Etiqueta = "Id de la factura seleccionada", Visible = false)]
        public int IdFacturaRec { get; set; }

        [IUPropiedad(
            Etiqueta = "Factura",
            Ayuda = "seleccione la factura recibida dónde anexar el correo",
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(IdFacturaRec),
            Controlador = nameof(enumControladoresGastos.FacturasRec),
            SeleccionarDe = typeof(FacturaRecDto),
            VistaDondeNavegar = enumVistasGastos.CrudFacturasRec,
            RestrictorFijo = ltrParametrosDto.Negocio + ";" + nameof(enumNegocio.FacturaRecibida) + ";" + nameof(enumModoDeAccesoDeDatos.Gestor),
            Negocio = enumNegocio.FacturaRecibida,
            MostrarExpresion = nameof(FacturaRecDto.Expresion),
            LongitudMinimaParaBuscar = 1,
            Tipo = typeof(string),
            Fila = 0,
            Columna = 2,
            trasBlanquear = "javascript:" + nameof(enumNameSpaceTs.Entorno) + "." + nameof(enumFunctionTs.MiCorreo_Tras_Blanquear_FacturaRec) + "([" + nameof(enumParamTs.idLista) + "])",
            trasSeleccionar = "javascript:" + nameof(enumNameSpaceTs.Entorno) + "." + nameof(enumFunctionTs.MiCorreo_Tras_Seleccionar_FacturaRec) + "([" + nameof(enumParamTs.idLista) + "])",
            AutoSpan = true)]
        public string FacturaRec { get; set; }

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
         Etiqueta = "Id del expediente seleccionado", Visible = false)]
        public int IdExpediente { get; set; }

        [IUPropiedad(
            Etiqueta = "Expediente",
            Ayuda = "seleccione el expediente dónde anexar el correo",
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(IdExpediente),
            Controlador = nameof(enumControladoresAdministrativos.Expedientes),
            SeleccionarDe = typeof(ExpedienteDto),
            VistaDondeNavegar = enumVistasAdministrativo.CrudExpedientes,
            RestrictorFijo = ltrParametrosDto.Negocio + ";" + nameof(enumNegocio.Expediente) + ";" + nameof(enumModoDeAccesoDeDatos.Gestor),
            Negocio = enumNegocio.Expediente,
            MostrarExpresion = nameof(ExpedienteDto.Expresion),
            LongitudMinimaParaBuscar = 1,
            Tipo = typeof(string),
            Fila = 0,
            Columna = 3,
            trasBlanquear = "javascript:" + nameof(enumNameSpaceTs.Entorno) + "." + nameof(enumFunctionTs.MiCorreo_Tras_Blanquear_Expediente) + "([" + nameof(enumParamTs.idLista) + "])",
            trasSeleccionar = "javascript:" + nameof(enumNameSpaceTs.Entorno) + "." + nameof(enumFunctionTs.MiCorreo_Tras_Seleccionar_Expediente) + "([" + nameof(enumParamTs.idLista) + "])",
            AutoSpan = true)]
        public string Expediente { get; set; }

        //----------------------------------------------
        [IUPropiedad(Etiqueta = "Id tarea de destito", Visible = false)]
        public int IdTareaDeDestino { get; set; }

        [IUPropiedad(
            Etiqueta = "Tarea",
            Ayuda = "Seleccione la tarea",
            TipoDeControl = enumTipoControl.ListaDeElemento,
            SeleccionarDe = typeof(TareaDto),
            Controlador = nameof(enumControladoresAdministrativos.Tareas),
            MostrarExpresion = nameof(TareaDto.Expresion),
            GuardarEn = nameof(IdTareaDeDestino),
            CargarBajoDemanda = true,
            Fila = 2,
            Columna = 0,
            OnChange = "javascript:" + nameof(enumNameSpaceTs.Entorno) + "." + nameof(enumFunctionTs.MiCorreo_Tras_Seleccionar_TareaDestino) + "(this)"
            )
        ]
        public string TareaDestino { get; set; }

        //----------------------------------------------
        [IUPropiedad(Etiqueta = "Id del archivador de destito", Visible = false)]
        public int IdArchivadorDeDestino { get; set; }

        [IUPropiedad(
            Etiqueta = "Archivador",
            Ayuda = "Seleccione el archivador",
            TipoDeControl = enumTipoControl.ListaDeElemento,
            SeleccionarDe = typeof(ArchivadorDto),
            Controlador = nameof(enumControladoresSistemaDocumental.Archivadores),
            MostrarExpresion = nameof(ArchivadorDto.Expresion),
            GuardarEn = nameof(IdArchivadorDeDestino),
            CargarBajoDemanda = true,
            Fila = 2,
            Columna = 1,
            OnChange = "javascript:" + nameof(enumNameSpaceTs.Entorno) + "." + nameof(enumFunctionTs.MiCorreo_Tras_Seleccionar_ArchivadorDestino) + "(this)"
            )
        ]
        public string ArchivadorDestino { get; set; }

        //----------------------------------------------
        [IUPropiedad(Etiqueta = "Id de la carpeta de destito", Visible = false)]
        public int IdCarpetaDeDestino { get; set; }

        [IUPropiedad(
            Etiqueta = "Carpeta",
            Ayuda = "Seleccione la carpeta del archivador",
            TipoDeControl = enumTipoControl.ListaDeElemento,
            SeleccionarDe = typeof(CarpetaDto),
            Controlador = nameof(enumControladoresSistemaDocumental.Carpetas),
            MostrarExpresion = nameof(ArchivadorDto.Expresion),
            GuardarEn = nameof(IdCarpetaDeDestino),
            CargarBajoDemanda = true,
            Fila = 2,
            Columna = 2
            )
        ]
        public string CarpetaDestino { get; set; }

        //----------------------------------------------------------------
        [IUPropiedad(
                Etiqueta = "Crear una tarea",
                Ayuda = "Crea una tarea e incluye el correo en ella",
                VisibleEnGrid = false,
                Obligatorio = false,
                Fila = 4,
                Columna = 0,
                EnConsultaOcultar = false,
                TipoDeControl = enumTipoControl.Referencia,
                CssDelDivDeLaTd = enumCssDiv.SeparadorTop10px,
                css = enumCssControles.CheckApilado,
                AccionRef = "javascript: " + nameof(enumNameSpaceTs.Entorno) + "." + nameof(enumFunctionTs.MiCorreo_CrearTarea) + "(" + nameof(IdElemento) + ")"
                )
            ]
        public string CrearTarea { get; }

        //----------------------------------------------------------------
        [IUPropiedad(
                Etiqueta = "Crear un registro",
                Ayuda = "Crea un registro e incluye el correo en él",
                VisibleEnGrid = false,
                Obligatorio = false,
                Fila = 4,
                Columna = 1,
                EnConsultaOcultar = false,
                TipoDeControl = enumTipoControl.Referencia,
                CssDelDivDeLaTd = enumCssDiv.SeparadorTop10px,
                css = enumCssControles.CheckApilado,
                AccionRef = "javascript: " + nameof(enumNameSpaceTs.Entorno) + "." + nameof(enumFunctionTs.MiCorreo_CrearRegistroEs) + "(" + nameof(IdElemento) + ")"
                )
            ]
        public string CrearRegistroEs { get; }


        //----------------------------------------------------------------
        [IUPropiedad(
                Etiqueta = "Crear una Factura",
                Ayuda = "Crea una factura recibida e incluir el correo en ella",
                VisibleEnGrid = false,
                Obligatorio = false,
                Fila = 4,
                Columna = 2,
                EnConsultaOcultar = false,
                TipoDeControl = enumTipoControl.Referencia,
                CssDelDivDeLaTd = enumCssDiv.SeparadorTop10px,
                css = enumCssControles.CheckApilado,
                AccionRef = "javascript: " + nameof(enumNameSpaceTs.Entorno) + "." + nameof(enumFunctionTs.MiCorreo_CrearFacturaRec) + "(" + nameof(IdElemento) + ")"
                )
            ]
        public string CrearFacturaRec { get; }

        //----------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Abrir tras asociar",
            Ayuda = "Indicar si al asociar un correo a un elemento lo muestra en otra pestaña",
            Fila = 3,
            Columna = 0,
            TipoDeControl = enumTipoControl.Check,
            css = enumCssControles.ControlApilado,
            ValorPorDefecto = true
            )
        ]
        public bool AbrirAlAsociar { get; set; }

    }
}

