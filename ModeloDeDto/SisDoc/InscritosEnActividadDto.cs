using ModeloDeDto.Terceros;
using Utilidades;

namespace ModeloDeDto.SistemaDocumental
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5)]
    public class InscritosEnActividadDto :  EsUnDetalleDto
    {
        [IUPropiedad(Etiqueta = "Id del interlocutor", Visible = false)]
        public int IdInterlocutor { get; set; }

        [IUPropiedad(
            Etiqueta = "Inscrito",
            AyudaDeCriteriosDeBusqueda = "seleccione por nombre, dni, correo o teléfono de la persona",
            GuardarEn = nameof(IdInterlocutor),
            BuscarPor = ltrInterlocutor.BuscarPorPersona,
            MostrarExpresion = nameof(InterlocutorDto.Expresion),
            Controlador = nameof(enumControladoresTerceros.Interlocutores),
            VistaDondeNavegar = enumVistasTerceros.CrudInterlocutores,
            SeleccionarDe = typeof(InterlocutorDto),
            Negocio = enumNegocio.Interlocutor,
            SoloEnAlta = true,
            EditableAlCrear = true,
            EditableAlEditar = true,
            TipoDeControl = enumTipoControl.ListaDinamica,
            Fila = 0,
            Columna = 1,
            Ordenar = true,
            PorAnchoMnt = 50,
            AutoSpan = true
          )
        ]
        public string Interlocutor { get; set; }


        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Pagado",
           Tipo = typeof(decimal),
           Ayuda = "importe pagado",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           Obligatorio = false,
           Formato = enumFormato.Numero_6,
           Fila = 1,
           Columna = 2,
            MantenerHuecoDeLaIzquierda = true)
        ]
        public decimal? Pagado { get; set; }

        //----------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Asistió",
            Ayuda = "indica si el inscrito ha asistido",
            VisibleEnGrid = false,
            Obligatorio = false,
            Fila = 1,
            Columna = 0,
            TipoDeControl = enumTipoControl.Check,
            css = enumCssControles.CheckEnLinea,
            ValorPorDefecto = true,
            EditableAlCrear = true,
            EditableAlEditar = true
            )
        ]
        public bool Asistio { get; set; }

        //------------------------------------------------------------------------
        [IUPropiedad(
            VisibleEnGrid = false,
            Etiqueta = "Justificante",
            Ayuda = "Seleccione el fichero justificante de pago",
            Tipo = typeof(int),
            TipoDeControl = enumTipoControl.SelectorDeUnArchivo,
            ExtensionesValidas = ExtensorDeTipoDeArchivos.NoEditables,
            Fila = 2,
            Columna = 0,
            AutoSpan = true)]
        public int? IdArchivo { get; set; }

        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(
         Etiqueta = "Descargar justificante",
         VisibleAlCrear = false,
         VisibleAlEditar = true,
         TipoDeControl = enumTipoControl.Referencia,
         AccionRef = "/" + nameof(enumControladoresSistemaDocumental.Archivos) + "/" + ltrEndPoint.epDescargaConGuid + "?guid=[" + nameof(GuidDeDescarga) + "]&id=[" + nameof(IdArchivo) + "]",
         Alineada = enumAliniacion.derecha,
         Fila = 3,
         Columna = 0)]
        public string Accion { get; set; }

        //----------------------------------------------
        [IUPropiedad(Visible = false)]
        public string NombreDeAccion { get; set; }


        //----------------------------------------------
        [IUPropiedad(Visible = false, Etiqueta = "Guid de descarga")]
        public string GuidDeDescarga { get; set; }

    }
}
