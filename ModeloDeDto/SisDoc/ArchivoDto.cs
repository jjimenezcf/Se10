using System;
using Utilidades;

namespace ModeloDeDto.SistemaDocumental
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5)]
    public class ArchivoDto: ElementoDto, IAuditadoDto
    {
        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = "Archivo",
            Ayuda = "nombre del archivo",
            Tipo = typeof(string),
            Fila = 0,
            Columna = 0,
            EditableAlEditar = true,
            LongitudMaxima = 250,
            AutoSpan = true
          )
        ]
        public string Nombre { get; set; }

        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = "Almacenado en",
            Ayuda = "ruta documental",
            Tipo = typeof(string),
            Fila = 1,
            Columna = 0,
            EditableAlEditar = false,
            AutoSpan = true
          )
        ]
        public string AlmacenadoEn { get; set; }

        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = "Crear y compartir",
            Ayuda = "Crea y comparte un enlace hasta la fecha indicada",
            TipoDeControl = enumTipoControl.SelectorDeFechaHora,
            Fila = 1,
            Columna = 2,
            EditableAlEditar = true,
            VisibleEnGrid = false,
            VisibleAlCrear = false,
            AutoSpan = true,
            Obligatorio = false,
            CssBotonAccion = enumCssControles.BotonCompartir,
            OnClick = "javascript:" + nameof(enumNameSpaceTs.ApiDeArchivos) + "." + nameof(enumFunctionTs.SisDoc_DescargarConGuid) + "(this)"
          )
        ]
        public DateTime? CaducaEl { get; set; }

        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = "Enlaces",
            TipoDeControl = enumTipoControl.Editor,
            Fila =2,
            Columna = 0,
            EditableAlEditar = false,
            AutoSpan = true,
            Obligatorio = false
            )
        ]
        public string EnlazadoA { get; set; }

        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = "Auditoría",
            Ayuda = "Auditoría de un archivo",
            TipoDeControl = enumTipoControl.AreaDeTexto,
            EditableAlEditar = false,
            Fila = 3,
            Columna = 0,
            AutoSpan = true,
            Obligatorio = false
            )
        ]
        public string Auditoria { get; set; }

        //---------------------------------------------------
        [IUPropiedad( Etiqueta = "Creado el",
            Tipo = typeof(DateTime),
            Fila = 4,
            Columna = 0,
            Obligatorio = false,
            EditableAlEditar = false)]
        public DateTime CreadoEl { get; set; }

        //----------------------------------------------
        [IUPropiedad(Etiqueta = "Modificado el",
            Tipo = typeof(DateTime),
            Fila = 4,
            Columna = 1,
            Obligatorio = false,
            EditableAlEditar = false)]
        public DateTime? ModificadoEl { get; set; }

        //----------------------------------------------
        [IUPropiedad(Etiqueta = "Creado por",
            Tipo = typeof(string),
            Fila = 5,
            Columna = 0,
            EditableAlEditar = false)]
        public string Creador { get; set; }

        //----------------------------------------------
        [IUPropiedad(Etiqueta = "Modificado por",
            Tipo = typeof(string),
            Fila = 5,
            Columna = 1,
            Obligatorio = false,
            EditableAlEditar = false)]
        public string Modificador { get; set; }

        //----------------------------------------------
        [IUPropiedad(Etiqueta = "Id del creador",
            Visible = false)]
        public int IdCreador { get; set; }

        //----------------------------------------------
        [IUPropiedad(Etiqueta = "Id del modificador",
            Visible = false)]
        public int? IdModificador { get; set; }

        //----------------------------------------------
        [IUPropiedad(Etiqueta = "Id del fichero original",  Visible = false)]
        public int? IdOriginal { get; set; }
        //----------------------------------------------
        [IUPropiedad(Etiqueta = "Original",  Visible = false)]
        public string Original { get; set; }

        //----------------------------------------------
        [IUPropiedad(Visible = false)]
        public bool EstaBloqueado { get; set; } = false;

        //----------------------------------------------
        [IUPropiedad(Visible = false)]
        public bool EstaEnlazado { get; set; } = false;

        //----------------------------------------------
        [IUPropiedad(Visible = false)]
        public bool EsDeUnArchivadorVinculado { get; set; } = false;
        //----------------------------------------------
        [IUPropiedad(Visible = false)]
        public bool PadreBloqueado { get; set; } = false;

        
    }
}
