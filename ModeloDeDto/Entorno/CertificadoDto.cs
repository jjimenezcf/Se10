using ServicioDeDatos.Entorno;
using Utilidades;

namespace ModeloDeDto.Entorno
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5)]
    public class CertificadoDto : ElementoDto
    {       
        [IUPropiedad(
            Etiqueta = "Clase de certificado",
            Ayuda = "Indique la clase del certificado",
            TipoDeControl = enumTipoControl.Enumerado,
            Tipo = typeof(enumClaseDeCertificados),
            GuardarEn = nameof(Clase),
            Fila = 0,
            Columna = 0,
            Posicion = 0,
            Obligatorio = false,
            VisibleEnEdicion = true,
            LongitudMaxima = 250
          )
        ]        
        public string Clase { get; set; }

        //------------------------------------------------
        [IUPropiedad(
          Etiqueta = "Certificado",
          Ayuda = "Nombre del certificado",
          Tipo = typeof(string),
          Fila = 0,
          Columna = 0,
          Posicion = 1,
          Ordenar = true,
          PorAnchoMnt = 75,
          ColSpan =2
          )
        ]
        public string Nombre { get; set; }


        //------------------------------------------------------------------------
        [IUPropiedad(
            VisibleEnGrid = false,
            Etiqueta = "Certificado",
            Ayuda = "Seleccione un certificado",
            Tipo = typeof(int),
            TipoDeControl = enumTipoControl.SelectorDeUnArchivo,
            ExtensionesValidas = ".cer, .pfx, .p12",
            trasSeleccionar = "javascript:" + nameof(enumNameSpaceTs.Entorno) + "." + nameof(enumFunctionTs.BlanquearPassword) + "()",
            Fila = 1,
            Columna = 0)]
        public int? IdArchivoDelCertificado { get; set; }


        //------------------------------------------------------------------------
        [IUPropiedad(
            VisibleEnGrid = false,
            Etiqueta = "Password",
            Ayuda = "password del certificado",
            TipoDeControl = enumTipoControl.Password,
            Tipo = typeof(string),
            Obligatorio = false,
            Fila = 2,
            Columna = 0)]
        public string PassworDelCertificado { get; set; }

        //------------------------------------------------------------------------
        [IUPropiedad(
            VisibleEnGrid = false,
            VisibleAlCrear = false,
            VisibleAlConsultar = true,
            VisibleAlEditar = true,
            Etiqueta = "",
            Ayuda = "información del certificado de usuario",
            Tipo = typeof(string),
            TipoDeControl = enumTipoControl.AreaDeTexto,
            Fila = 3,
            Obligatorio = false,
            NumeroDeFilas = 3,
            Columna = 0,
            EditableAlEditar = false,
            AutoSpan = true)]
        public string DatosCertificado { get; set; }

        //-------------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "P. de Gestor",
            Ayuda = "Permiso de gestor (permite el uso del certificado)",
            EditableAlEditar = false,
            VisibleAlCrear = false,
            VisibleAlConsultar = true,
            VisibleAlEditar = true,
            VisibleEnGrid = false,
            Tipo = typeof(int),
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Gestor),
            Controlador = nameof(enumControladoresSeguridad.Permisos),
            VistaDondeNavegar = enumVistasSeguridad.CrudPermiso,
            Fila = 4,
            Columna = 0,
            Obligatorio = false,
            AutoSpan = true
            )
        ]
        public int IdGestor { get; set; }
        [IUPropiedad(Visible = false)]
        public string Gestor { get; set; }

    }
}
