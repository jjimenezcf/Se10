using ModeloDeDto.Contabilidad;
using ModeloDeDto.MaestrosTecnico;
using ModeloDeDto.Terceros;
using ServicioDeDatos.MaestrosTecnico;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.SistemaDocumental;
using Utilidades;

namespace ModeloDeDto.Presupuesto
{
    [IUDto(MostrarExpresion = nameof(IUsaNombreDto.Nombre), OpcionDeEditar = false, OpcionDeBorrar = false)]
    public class ValoracionDto : ElementoDeUnProcesoDto
    {


        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
         Etiqueta = "Id del solicitante del Presupuesto",
         Visible = false
         )]
        public int IdSolicitante { get; set; }

        [IUPropiedad(
            Etiqueta = "Solicitante",
            Ayuda = "Quién solicita el Presupuesto",
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(IdSolicitante),
            Controlador = nameof(enumControladoresTerceros.Interlocutores),
            SeleccionarDe = typeof(InterlocutorDto),
            VistaDondeNavegar = enumVistasTerceros.CrudInterlocutores,
            RestrictorFijo = ltrParametrosDto.Negocio + ";" + nameof(enumNegocio.Interlocutor) + ";" + nameof(enumModoDeAccesoDeDatos.Consultor),
            Negocio = enumNegocio.Interlocutor,
            LongitudMinimaParaBuscar = 1,
            Tipo = typeof(string),
            Fila = 0,
            Columna = 2,
            AutoSpan = true)]
        public string Solicitante { get; set; }


        //-----------------------------------------------------
        [IUPropiedad(
          Etiqueta = "Concepto",
          Ayuda = "Concepto presupuestado",
          Tipo = typeof(string),
          TipoDeControl = enumTipoControl.Editor,
          Fila = 3,
          Columna = 0,
          ColSpan = 2
          )
        ]
        public string Concepto { get; set; }


        //----------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Clase",
            Ayuda = "Seleccione la clase del unitario",
            TipoDeControl = enumTipoControl.Enumerado,
            Tipo = typeof(enumClaseUnitario),
            GuardarEn = nameof(Clase),
            Fila = 3,
            Columna = 3
            )
        ]
        public string Clase { get; set; }

        //----------------------------------------------
        [IUPropiedad(Etiqueta = "Id de la naturaleza contable", Visible = false)]
        public int IdNaturaleza { get; set; }

        [IUPropiedad(
            Etiqueta = "Naturaleza",
            Ayuda = "Seleccione la naturaleza contable",
            TipoDeControl = enumTipoControl.ListaDeElemento,
            SeleccionarDe = typeof(NaturalezaDto),
            Controlador = nameof(enumControladoresMt.Naturalezas),
            GuardarEn = nameof(IdNaturaleza),
            MostrarExpresion = nameof(NaturalezaDtm.Expresion),
            Fila = 3,
            Columna = 4
            )
        ]
        public string Naturaleza { get; set; }

        //----------------------------------------------
        [IUPropiedad(Etiqueta = "Id la unidad de medida", Visible = false)]
        public int IdUnidad { get; set; }

        [IUPropiedad(
            Etiqueta = "Unidad",
            Ayuda = "Seleccione la unidad de medida",
            TipoDeControl = enumTipoControl.ListaDeElemento,
            SeleccionarDe = typeof(UnidadDto),
            Controlador = nameof(enumControladoresMt.Unidades),
            GuardarEn = nameof(IdUnidad),
            Fila = 3,
            Columna = 5
            )
        ]
        public string Unidad { get; set; }


        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Cantidad",
           Tipo = typeof(decimal),
           Ayuda = "cantidad presupuestada",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           OnBlur = "javascript:" + nameof(enumNameSpaceTs.Administracion) + "." + nameof(enumFunctionTs.Exp_CalcularValoracion) + "()",
           Formato = enumFormato.Numero_6,
           Fila = 4,
           Columna = 0)
        ]
        public decimal Cantidad { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "P. de venta",
           Tipo = typeof(decimal),
           Ayuda = "precio de venta",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           OnBlur = "javascript:" + nameof(enumNameSpaceTs.Administracion) + "." + nameof(enumFunctionTs.Exp_CalcularValoracion) + "()",
           Formato = enumFormato.Numero_6,
           Fila = 4,
           Columna = 1)
        ]
        public decimal Precio { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Descuento(%)",
           Tipo = typeof(decimal),
           Ayuda = "porcentaje de descuento por línea",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           OnBlur = "javascript:" + nameof(enumNameSpaceTs.Administracion) + "." + nameof(enumFunctionTs.Exp_CalcularValoracion) + "()",
           Obligatorio = false,
           Formato = enumFormato.Porcentaje,
           Fila = 4,
           Columna = 2)
        ]
        public decimal? Descuento { get; set; }

        //----------------------------------------------
        [IUPropiedad(Etiqueta = "Id del iva repercutido", Visible = false)]
        public int IdIvaR { get; set; }

        [IUPropiedad(
            Etiqueta = "Iva",
            Ayuda = "Seleccione el tipo de iva",
            TipoDeControl = enumTipoControl.ListaDeElemento,
            SeleccionarDe = typeof(IvaRepercutidoDto),
            Controlador = nameof(enumControladoresContables.IvasRepercutido),
            GuardarEn = nameof(IdIvaR),
            OnBlur = "javascript:" + nameof(enumNameSpaceTs.Administracion) + "." + nameof(enumFunctionTs.Exp_CalcularValoracion) + "()",
            Fila = 4,
            Columna = 3
            )
        ]
        public string IvaRepercutido { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Total",
           Tipo = typeof(decimal),
           Ayuda = "importe total",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = false,
           Obligatorio = false,
           MantenerHuecoDeLaIzquierda = true,
           Formato = enumFormato.Moneda,
           Fila = 4,
           Columna = 5)
        ]
        public decimal? ImporteDeLinea { get; set; }

        //------------------------------------------------------------------------
        [IUPropiedad(
            VisibleEnGrid = false,
            Etiqueta = "Archivo del ppt",
            Ayuda = "Subir archivo",
            Tipo = typeof(int),
            TipoDeControl = enumTipoControl.SelectorDeUnArchivo,
            ExtensionesValidas = "." + nameof(enumExtensiones.docx) + ", ." + nameof(enumExtensiones.pdf) + ", ." + nameof(enumExtensiones.xlsx),
            Fila = 5,
            Columna = 0,
            AutoSpan = true)]
        public int? IdArchivo { get; set; }


        //----------------------------------------------------------
        [IUPropiedad(
         Etiqueta = "",
         Oculto = true,
         Obligatorio = false,
         Fila = 0,
         Columna = 0,
         Posicion = 1)]
        public int? idExpediente { get; set; }
    }


}
