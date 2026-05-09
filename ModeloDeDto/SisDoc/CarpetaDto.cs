using Utilidades;

namespace ModeloDeDto.SistemaDocumental
{
    [IUDto(MostrarExpresion = nameof(IUsaNombreDto.Nombre))]
    public class CarpetaDto : ElementoDto, IUsaNombreDto
    {
        //-------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Archivador",
            Visible = false
            )
        ]
        public int IdArchivador { get; set; }

        [IUPropiedad(Etiqueta = "Archivador",
         Ayuda = "Carpetas de un archivador",
         TipoDeControl = enumTipoControl.ListaDinamica,
         SeleccionarDe = typeof(ArchivadorDto),
         GuardarEn = nameof(IdArchivador),
         MostrarExpresion = nameof(ArchivadorDto.Nombre),
         Controlador = nameof(enumControladoresSistemaDocumental.Archivadores),
         VistaDondeNavegar = enumVistasSistemaDocumental.CrudArchivadores,
         AlSeleccionarBlanquearControl = nameof(Padre),
         EditableAlCrear = false,
         Fila = 0,
         Columna = 0,
         VisibleEnGrid = false
         )]
        public string Archivador { get; set; }


        //----------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Id de carpeta padre",
            Visible = false
            )
        ]
        public int? IdPadre { get; set; }

        [IUPropiedad(
            Etiqueta = "Carpeta padre",
            Ayuda = "Indique la carpeta",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(CarpetaDto),
            GuardarEn = nameof(IdPadre),
            MostrarExpresion = nameof(Expresion),
            Controlador = nameof(enumControladoresSistemaDocumental.Carpetas),
            VistaDondeNavegar = enumVistasSistemaDocumental.CrudCarpetas,
            RestringidoPorControl = nameof(Archivador),
            PropiedadRestrictora = nameof(IdArchivador),
            AplicarJoin = true,
            Fila = 0,
            Columna = 1,
            Ordenar = true,
            Obligatorio = false
            )
        ]
        public string Padre { get; set; }

        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = "Carpeta",
            Ayuda = "Indique el nombre de la carpeta",
            Tipo = typeof(string),
            Fila = 0,
            Columna = 2,
            Ordenar = true,
            Obligatorio = true,
            LongitudMaxima = 250
          )
        ]
        public string Nombre { get; set; }

        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = "Expresión de la carpeta",
            Tipo = typeof(string),
            Visible = false
          )
        ]
        public string Expresion { get; set; }
    }
}
