using ModeloDeDto.Negocio;
using ModeloDeDto.Terceros;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Terceros;
using Utilidades;

namespace ModeloDeDto.SistemaDocumental
{
    [IUDto(MostrarExpresion = nameof(IUsaNombreDto.Nombre))]
    public class CopiarArchDto : ISelectorDto
    {
        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
         Etiqueta = "Id del archivador seleccionado",
         Visible = false
         )]
        public int IdElemento { get; set; }

        [IUPropiedad(
            Etiqueta = "Archivador",
            Ayuda = "Archivador a copiar",
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(IdElemento),
            Controlador = nameof(enumControladoresSistemaDocumental.Archivadores),
            SeleccionarDe = typeof(ArchivadorDto),
            VistaDondeNavegar = enumVistasSistemaDocumental.CrudArchivadores,
            RestrictorFijo = ltrParametrosDto.Negocio + ";" + nameof(enumNegocio.Archivador) + ";" + nameof(enumModoDeAccesoDeDatos.Consultor),
            Negocio = enumNegocio.Archivador,
            LongitudMinimaParaBuscar = 1,
            MostrarExpresion = nameof(ArchivadorDto.Expresion),
            Tipo = typeof(string),
            Fila = 0,
            Columna = 0,
            EditableAlCrear = true,
            EditableAlEditar = false,
            trasSeleccionar = "javascript:" + nameof(enumNameSpaceTs.SistemaDocumental) + "." + nameof(enumFunctionTs.Arcdor_ProponerDatosDelArcSeleccionado) + "()",
            trasBlanquear = "javascript:" + nameof(enumNameSpaceTs.SistemaDocumental) + "." + nameof(enumFunctionTs.Arcdor_InicializarModalDeCopiado) + "()",
            AutoSpan = true)]
        public string Elemento { get; set; }

        //----------------------------------------------
        [IUPropiedad(
         Etiqueta = "",
         Oculto = true,
         Obligatorio = false,
         VisibleEnGrid = false,
         Fila = 1,
         Columna = 0,
         Posicion = 0
         )]
        public int? IdSociedadDelCg { get; set; }

        [IUPropiedad(
         Etiqueta = "Id del Cg",
         Visible = false
         )]
        public int IdCg { get; set; }

        [IUPropiedad(
            Etiqueta = "CG propuesto",
            Ayuda = "Centro gestor",
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(IdCg),
            MostrarExpresion = nameof(CentroGestorDto.Expresion),
            Controlador = nameof(enumControladoresTerceros.CentrosGestores),
            VistaDondeNavegar = enumVistasTerceros.CrudCentrosGestores,
            SeleccionarDe = typeof(CentroGestorDto),
            Negocio = enumNegocio.CentroGestor,
            RestrictorFijo = nameof(NegociosDeUnCgDtm.Negocio) + ";" + nameof(enumNegocio.CentroGestor) + ";" + nameof(enumModoDeAccesoDeDatos.Gestor),
            RestringidoPorControl = nameof(IdSociedadDelCg),
            PropiedadRestrictora = nameof(IdSociedadDelCg),
            SoloEnAlta = true,
            LongitudMinimaParaBuscar = 1,
            Tipo = typeof(string),
            Fila = 1,
            Columna = 0,
            Ordenar = true,
            EditableAlCrear = true,
            EditableAlEditar = false,
            Posicion = 1)]
        public string Cg { get; set; }

        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(
              Etiqueta = "Id del tipo de archivador",
              Visible = false
              )]
        public int IdTipo { get; set; }

        [IUPropiedad(
            Etiqueta = "Tipo de archivador",
            Ayuda = "Tipo de archivado",
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(IdTipo),
            SeleccionarDe = typeof(TipoDeArchivadorDto),
            BuscarPor = ltrTipoArchivador.SeleccionarParaCopiar,
            RestrictorFijo = ltrParametrosDto.Negocio + Simbolos.PuntoComa + nameof(enumNegocio.Archivador) + Simbolos.PuntoComa + nameof(enumModoDeAccesoDeDatos.Gestor),
            Controlador = nameof(enumControladoresNegocio.TiposDeElemento),
            VistaDondeNavegar = enumVistasSistemaDocumental.TiposDeArchivador,
            MostrarExpresion = nameof(TipoDeElementoDto.Nombre),
            SoloEnAlta = true,
            LongitudMinimaParaBuscar = 1,
            Negocio = enumNegocio.Archivador,
            Tipo = typeof(string),
            Fila = 1,
            Columna = 1,
            Ordenar = true,
            EditableAlCrear = true,
            EditableAlEditar = false,
            AutoSpan = true)]
        public string Tipo { get; set; }


        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Nombre",
            Ayuda = "nombre del archivador",
            Tipo = typeof(string),
            Fila = 3,
            Columna = 0,
            Ordenar = true,
            PorAnchoMnt = 50,
            Obligatorio = true,
            LongitudMaxima = 250,
            AutoSpan = true
          )
        ]
        public string Nombre { get; set; }

        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Descripción",
           Ayuda = "descripción del archivador",
           TipoDeControl = enumTipoControl.AreaDeTexto,
           VisibleEnGrid = false,
           Obligatorio = false,
           NumeroDeFilas = 5,
           Fila = 4,
           Columna = 0,
           AutoSpan = true
          )
        ]
        public string Descripcion { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Copiar carpetas",
           Ayuda = "Si existen, copia la estructura de carpetas",
           TipoDeControl = enumTipoControl.Check,
           Obligatorio = false,
           css = enumCssControles.CheckEnLinea,
           ValorPorDefecto = true,
           Fila = 6, Columna = 0, Posicion = 1)
        ]
        public bool CopiarCarpetas { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Copiar archivos",
           Ayuda = "Copia los archivos anexados a las carpetas o al archivador",
           TipoDeControl = enumTipoControl.Check,
           Obligatorio = false,
           css = enumCssControles.CheckEnLinea,
           ValorPorDefecto = false,
           OnChange = "javascript:" + nameof(enumNameSpaceTs.SistemaDocumental) + "." + nameof(enumFunctionTs.Arcdor_Tras_Pulsar_Copiar_Archivos) + "(this)",
           Fila = 6, Columna = 1, Posicion = 0)
        ]
        public bool CopiarArchivos { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Enlazar archivos",
           Ayuda = "Enlaza los archivos anexados a las carpetas o al archivador",
           TipoDeControl = enumTipoControl.Check,
           Obligatorio = false,
           css = enumCssControles.CheckEnLinea,
           ValorPorDefecto = false,
           OnChange = "javascript:" + nameof(enumNameSpaceTs.SistemaDocumental) + "." + nameof(enumFunctionTs.Arcdor_Tras_Pulsar_Enlazar_Archivos) + "(this)",
           Fila = 6, Columna = 1, Posicion = 0)
        ]
        public bool EnlazarArchivos { get; set; }

    }
}
