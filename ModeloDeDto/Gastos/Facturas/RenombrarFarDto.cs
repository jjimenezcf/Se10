using ModeloDeDto.Contabilidad;
using ModeloDeDto.MaestrosTecnico;
using ServicioDeDatos.MaestrosTecnico;
using Utilidades;

namespace ModeloDeDto.Gastos
{
    [IUDto(MostrarExpresion = nameof(IUsaNombreDto.Nombre))]
    public class RenombrarFarDto : IRenombrarDto
    {
        [IUPropiedad(
            Etiqueta = "Factura",
            Ayuda = "Factura a renombrar",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Elemento),
            Fila = 0,
            Columna = 0,
            VisibleEnGrid = false,
            Controlador = nameof(enumControladoresGastos.FacturasRec),
            VistaDondeNavegar = enumVistasGastos.CrudFacturasRec
            )
        ]
        public int IdElemento { get; set; }

        [IUPropiedad(Etiqueta = "Factura", Visible = false)]
        public string Elemento { get; set; }


        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = "Asunto",
            Ayuda = "Indique el nuevo asunto",
            Tipo = typeof(string),
            Fila = 1,
            Columna = 0,
            Obligatorio = false,
            LongitudMaxima = 250
          )
        ]
        public string Nombre { get; set; }

        //----------------------------------------------
        [IUPropiedad(Etiqueta = "Id de la naturaleza a cambiar", Visible = false)]
        public int? IdNaturalezaAnterior { get; set; }

        [IUPropiedad(
            Etiqueta = "Naturaleza a cambiar",
            Ayuda = "Seleccione la naturaleza a cambiar",
            TipoDeControl = enumTipoControl.ListaDeElemento,
            SeleccionarDe = typeof(NaturalezaDto),
            Controlador = nameof(enumControladoresMt.Naturalezas),
            GuardarEn = nameof(IdNaturalezaAnterior),
            Obligatorio = false,
            MostrarExpresion = nameof(NaturalezaDtm.Expresion),
            VisibleEnGrid = false,
            Fila = 2,
            Columna = 0
            )
        ]
        public string NaturalezaAnterior { get; set; }

        //----------------------------------------------
        [IUPropiedad(Etiqueta = "Id de la naturaleza nueva", Visible = false)]
        public int? IdNaturalezaNueva { get; set; }

        [IUPropiedad(
            Etiqueta = "Naturaleza nueva",
            Ayuda = "Seleccione la naturaleza nueva",
            TipoDeControl = enumTipoControl.ListaDeElemento,
            SeleccionarDe = typeof(NaturalezaDto),
            Controlador = nameof(enumControladoresMt.Naturalezas),
            GuardarEn = nameof(IdNaturalezaNueva),
            Obligatorio = false,
            MostrarExpresion = nameof(NaturalezaDtm.Expresion),
            VisibleEnGrid = false,
            Fila = 2,
            Columna = 1
            )
        ]
        public string NaturalezaNueva { get; set; }

        //-----------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Iva a cambiar",
            Ayuda = "iva a cambiar",
            TipoDeControl = enumTipoControl.ListaDeElemento,
            SeleccionarDe = typeof(IvaSoportadoDto),
            Controlador = nameof(enumControladoresContables.IvasSoportado),
            GuardarEn = nameof(IdIvaSoportadoAnterior),
            Obligatorio = false,
            VisibleEnGrid = false,
            Fila = 2,
            Columna = 2
            )
        ]
        public string IvaSoportadoAnterior { get; set; }

        [IUPropiedad(Etiqueta = "Id del iva soportado anterior", Visible = false)]
        public int? IdIvaSoportadoAnterior { get; set; }


        //-----------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Nuevo Iva",
            Ayuda = "Nuevo iva",
            TipoDeControl = enumTipoControl.ListaDeElemento,
            SeleccionarDe = typeof(IvaSoportadoDto),
            Controlador = nameof(enumControladoresContables.IvasSoportado),
            GuardarEn = nameof(IdIvaSoportadoNuevo),
            Obligatorio = false,
            VisibleEnGrid = false,
            Fila = 2,
            Columna = 2
            )
        ]
        public string IvaSoportadoNuevo { get; set; }

        [IUPropiedad(Etiqueta = "Id del iva soportado anterior", Visible = false)]
        public int? IdIvaSoportadoNuevo { get; set; }

    }
}
