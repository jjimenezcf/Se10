using System;
using Utilidades;

namespace ModeloDeDto.Negocio
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, OpcionDeBorrar = false)]
    public class ObservacionDto : ElementoDto
    {
        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Negocio",
            Ayuda = "Observacion del negocio",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Negocio),
            Fila = 0,
            Columna = 0,
            EditableAlCrear = false,
            EditableAlEditar = false,
            VisibleEnGrid = false
            )
        ]
        public int IdNegocio { get; set; }

        [IUPropiedad(Visible = false)]
        public string Negocio { get; set; }

        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Elemento",
            Ayuda = "Observacion del elemento",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Elemento),
            Fila = 0,
            Columna = 1,
            EditableAlCrear = false,
            EditableAlEditar = false,
            VisibleEnGrid = false
            )
        ]
        public int IdElemento { get; set; }

        [IUPropiedad(Visible = false)]
        public string Elemento { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Asunto",
           Ayuda = "indique el asunto",
           Visible = false,
           VisibleEnGrid = true,
           VisibleAlCrear = true,
           VisibleAlEditar = true,
           EditableAlEditar = false,
           EditableAlCrear = true,
           TipoDeControl = enumTipoControl.Editor,
           LongitudMaxima = 250,
           Obligatorio = true,
           AutoSpan = true,
           Fila = 2,
           Columna = 0)
        ]
        public string Nombre { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Observación",
           Ayuda = "Detalle",
           TipoDeControl = enumTipoControl.AreaDeTexto,
           LongitudMaxima = 2000,
           Fila = 3,
           Columna = 0,
           AutoSpan = true
           )
        ]
        public string Descripcion { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Observación",
           Ayuda = "Detalle",
           LongitudMaxima = 2000,
           Visible = false
           )
        ]
        public string DescripcionEnLinea => Descripcion.Left(200) + $"{(Descripcion.Length > 200 ? "..." : "")}";

        //--------------------------------------------
        [IUPropiedad(Visible = false)]
        public int IdCreador { get; set; }

        [IUPropiedad(
           Etiqueta = "Creada por",
           Ayuda = "Creada por",
           Visible = false,
           VisibleEnGrid = true,
           VisibleAlEditar = true,
           EditableAlEditar = false,
           TipoDeControl = enumTipoControl.RestrictorDeEdicion,
           PropiedadRestrictora = nameof(IdCreador),
           MostrarExpresion = nameof(Creador),
           Fila = 6,
           Columna = 0)
        ]
        public string Creador { get; set; }


        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Creada el",
           Ayuda = "Fecha de creación",
           TipoDeControl = enumTipoControl.SelectorDeFechaHora,
           Formato = enumFormato.Fecha,
           Fila = 6,
           Columna = 1,
           VisibleEnGrid = true,
           VisibleAlCrear = false,
           EditableAlCrear = false,
           EditableAlEditar = false
           )
        ]
        public DateTime CreadaEl { get; set; }

        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(
         Etiqueta = "Descargar archivo",
         VisibleAlCrear = false,
         VisibleAlEditar = true,
         TipoDeControl = enumTipoControl.Referencia,
         AccionRef = "/" + nameof(enumControladoresSistemaDocumental.Archivos) + "/" + ltrEndPoint.epDescargaConGuid + "?guid=[" + nameof(GuidDeDescarga) + "]&id=[" + nameof(IdArchivo) + "]",
         Alineada = enumAliniacion.derecha,
         css = enumCssControles.RefApilado,
         Fila = 4,
         Columna = 0)]
        public string Accion { get; set; }

        //------------------------------------------------------------------------
        [IUPropiedad(
            VisibleEnGrid = false,
            Etiqueta = "Archivo",
            Ayuda = "Seleccione asociado a la observación",
            Tipo = typeof(int),
            TipoDeControl = enumTipoControl.SelectorDeUnArchivo,
            ExtensionesValidas = ExtensorDeTipoDeArchivos.NoEditables,
            Fila = 5,
            Columna = 0,
            AutoSpan = true)]
        public int? IdArchivo { get; set; }

        //----------------------------------------------
        [IUPropiedad(Visible = false)]
        public string NombreDeAccion { get; set; }


        //----------------------------------------------
        [IUPropiedad(Visible = false, Etiqueta = "Guid de descarga")]
        public string GuidDeDescarga { get; set; }
    }

}
