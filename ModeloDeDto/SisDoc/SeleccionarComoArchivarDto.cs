using ServicioDeDatos.Seguridad;
using Utilidades;

namespace ModeloDeDto.SistemaDocumental
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5)]
    public class SeleccionarComoArchivarDto : ISelectorDto
    {
        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
         Etiqueta = "Id del archivador seleccionado",
         Visible = false
         )]
        public int IdElemento { get; set; }

        [IUPropiedad(
            Etiqueta = "Archivador",
            Ayuda = "seleccione el archivador dónde anexar el correo",
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(IdElemento),
            Controlador = nameof(enumControladoresSistemaDocumental.Archivadores),
            SeleccionarDe = typeof(ArchivadorDto),
            VistaDondeNavegar = enumVistasSistemaDocumental.CrudArchivadores,
            RestrictorFijo = ltrParametrosDto.Negocio + ";" + nameof(enumNegocio.Archivador) + ";" + nameof(enumModoDeAccesoDeDatos.Gestor),
            Negocio = enumNegocio.Archivador,
            MostrarExpresion = nameof(ArchivadorDto.Expresion),
            LongitudMinimaParaBuscar = 1,
            Tipo = typeof(string),
            Fila = 0,
            Columna = 0,
            trasBlanquear = "javascript:" + nameof(enumNameSpaceTs.Entorno) + "." + nameof(enumFunctionTs.MiCorreo_Tras_Blanquear_Archivador) + "([" + nameof(enumParamTs.idLista) + "])",
            trasSeleccionar = "javascript:" + nameof(enumNameSpaceTs.Entorno) + "." + nameof(enumFunctionTs.MiCorreo_Tras_Seleccionar_Archivador) + "([" + nameof(enumParamTs.idLista) + "])",
            AutoSpan = true)]
        public string Elemento { get; set; }

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
            RestringidoPorControl = nameof(Elemento),
            CargarBajoDemanda = true,
            AutoSpan = true,
            Fila = 1,
            Columna = 0
            )
        ]
        public string CarpetaDestino { get; set; }

        //----------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Abrir tras asociar",
            Ayuda = "Indicar si al asociar un correo a un archivador lo muestra en otra pestaña",
            Fila = 2,
            Columna = 0,
            TipoDeControl = enumTipoControl.Check,
            css = enumCssControles.ControlApilado,
            ValorPorDefecto = true
            )
        ]
        public bool AbrirAlAsociar { get; set; }

        //----------------------------------------------------------------
        [IUPropiedad(
                Etiqueta = "Crear un nuevo archivador",
                Ayuda = "Crea un nuevo archivador e incluye el correo en él",
                VisibleEnGrid = false,
                Obligatorio = false,
                Fila = 3,
                Columna = 0,
                EnConsultaOcultar = false,
                TipoDeControl = enumTipoControl.Referencia,
                CssDelDivDeLaTd = enumCssDiv.SeparadorTop10px,
                css = enumCssControles.CheckApilado,
                AccionRef = "javascript: " + nameof(enumNameSpaceTs.Entorno) + "." + nameof(enumFunctionTs.MiCorreo_CrearArchivador) + "(" + nameof(IdElemento) + ")"
                )
            ]
        public string CrearArchivador { get; }
    }
}

