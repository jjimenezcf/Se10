using System;
using Utilidades;

namespace ModeloDeDto.Entorno
{

    public static class UsuariosPor
    {
        public const string NombreCompleto = nameof(NombreCompleto);
        public static string Permisos = nameof(Permisos).ToLower();
        public static string AlgunUsuario = nameof(AlgunUsuario).ToLower();
        public static string eMail = nameof(eMail).ToLower();
    }

    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, MostrarExpresion = ExpresionElemento)]
    public class UsuarioDto : ElementoDto, IUsaArchivoDto
    {
        internal const string ExpresionElemento = nameof(NombreCompleto);

        //------------------------------------------------------------------------
        [IUPropiedad(
            VisibleEnGrid = true,
            VisibleEnEdicion = false,
            Etiqueta = "Usuario",
            Ordenar = true,
            OrdenarGridPor = nameof(UsuarioDto.Login),
            PorAnchoMnt = 35
         )
        ]
        public string NombreCompleto { get; set; } // => $"({Login}) {Apellido}, {Nombre}";

        //------------------------------------------------------------------------
        [IUPropiedad(
            VisibleEnGrid = false,
            Etiqueta = "Usuario",
            Ayuda = "Usuario de conexión",
            Tipo = typeof(string),
            Fila = 1,
            Columna = 0,
            Ordenar = true,
            PorAnchoMnt = 25
            )
        ]
        public string Login { get; set; }

        //------------------------------------------------------------------------
        [IUPropiedad(
            VisibleEnGrid = false,
            Etiqueta = "Apellidos",
            Ayuda = "Apellidos",
            Tipo = typeof(string),
            Fila = 3,
            Columna = 0,
            Ordenar = true,
            PorAnchoMnt = 45
            )
        ]
        public string Apellido { get; set; }

        //------------------------------------------------------------------------
        [IUPropiedad(
            VisibleEnGrid = false,
            Etiqueta = "Nombre",
            Ayuda = "Nombre",
            Tipo = typeof(string),
            Fila = 3,
            Columna = 1,
            Posicion = 0
            )
        ]
        public string Nombre { get; set; }

        //------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "eMail",
            Ayuda = "eMail",
            Tipo = typeof(string),
            Fila = 1,
            Columna = 1,
            Posicion = 0
            )
        ]
        public string eMail { get; set; }

        //------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Fecha de alta",
            EtiquetaGrid = "Alta",
            TipoDeControl = enumTipoControl.SelectorDeFecha,
            Alineada = enumAliniacion.izquierda,
            VisibleEnGrid = true,
            VisibleAlCrear = false,
            VisibleAlEditar = true,
            VisibleAlConsultar = true,
            EditableAlEditar = false,
            EditableAlCrear = false,
            Fila = 4,
            Columna = 0,
            AnchoMaximo = "8.7em",
            Ordenar = true
            )
        ]
        public DateTime Alta { get; set; }

        //------------------------------------------------------------------------
        [IUPropiedad(
            VisibleEnGrid = false,
            VisibleEnEdicion = true,
            Etiqueta = "Fotografía",
            Ayuda = "Seleccione un fichero",
            Tipo = typeof(int),
            TipoDeControl = enumTipoControl.Archivo,
            ExtensionesValidas = ExtensorDeTipoDeArchivos.Imagenes,
            UrlDelArchivo = nameof(Archivo),
            Obligatorio = false,
            Fila = 5,
            Columna = 0)]
        public int? IdArchivo { get; set; }

        [IUPropiedad(TipoDeControl = enumTipoControl.ImagenDelCanvas)]
        public string Archivo { get; set; }

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
            Fila = 8,
            Obligatorio = false,
            NumeroDeFilas =3,
            EditableAlEditar = false,
            Columna = 0)]
        public string DatosCertificado { get; set; }

        //------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Cliente",
            Ayuda = "Cliente al que pertenece el usuario",
            Tipo = typeof(string),
            Fila = 9,
            Columna = 0,
            Posicion = 0,
            EditableAlEditar = false,
            VisibleAlCrear = false,
            VisibleEnGrid = false,
            Obligatorio = false
            )
        ]
        public string Cliente { get; set; }

        //------------------------------------------------------------------------
        [IUPropiedad(Visible = false)]
        public bool EsClienteWeb { get; set; }

        //------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Administrador",
            Ayuda = "indica si el usuario es administrador",
            VisibleEnGrid = false,
            Obligatorio = false,
            Fila = 10,
            Columna = 0,
            TipoDeControl = enumTipoControl.Check,
            css = enumCssControles.ControlApilado,
            ValorPorDefecto = false
            )
        ]
        public bool EsAdministrador { get; set; }



        //----------------------------------------------------------------

        [IUPropiedad(
            Etiqueta = "Usuario activo",
            Ayuda = "indica si el usuario está activo",
            VisibleEnGrid = false,
            Obligatorio = true,
            Fila = 11,
            Columna = 0,
            TipoDeControl = enumTipoControl.Check,
            css = enumCssControles.ControlApilado,
            VisibleAlCrear = false,
            VisibleAlEditar = true,
            EditableAlEditar = true,
            ValorPorDefecto = true
            )
        ]
        public bool Activo { get; set; }
    }



}
