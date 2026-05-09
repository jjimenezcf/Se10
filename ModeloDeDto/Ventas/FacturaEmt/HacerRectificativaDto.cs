using ModeloDeDto.Presupuesto;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Ventas;
using Utilidades;

namespace ModeloDeDto.Ventas
{
    [IUDto(MostrarExpresion = nameof(IUsaNombreDto.Nombre))]
    public class HacerRectificativaDto : ISelectorDto
    {
        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
         Etiqueta = "Id de la factura a rectificar",
         Visible = false
         )]
        public int IdElemento { get; set; }

        [IUPropiedad(
            Etiqueta = "factura",
            Ayuda = "factura a rectificar",
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(IdElemento),
            Controlador = nameof(enumControladoresVentas.FacturasEmt),
            SeleccionarDe = typeof(FacturaEmtDto),
            VistaDondeNavegar = enumVistasVentas.CrudFacturasEmt,
            RestrictorFijo = ltrParametrosDto.Negocio + ";" + nameof(enumNegocio.FacturaEmitida) + ";" + nameof(enumModoDeAccesoDeDatos.Interventor),
            Negocio = enumNegocio.FacturaEmitida,
            LongitudMinimaParaBuscar = 1,
            MostrarExpresion = nameof(PresupuestoDto.Expresion),
            Tipo = typeof(string),
            Fila = 0,
            Columna = 0,
            EditableAlCrear = false,
            EditableAlEditar = false,
            AutoSpan = true)]
        public string Elemento { get; set; }

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Motivo de rectificación",
            TipoDeControl = enumTipoControl.Enumerado,
            Tipo = typeof(enumMotivoDeRectificacion),
            GuardarEn = nameof(Motivo),
            Fila = 2,
            Columna = 0,
            Obligatorio = true,
            VisibleEnGrid = false
          )
        ]
        public string Motivo { get; set; }

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
             Etiqueta = "Clase de rectificativa",
            TipoDeControl = enumTipoControl.Enumerado,
            Tipo = typeof(enumClaseDeRectificativa),
            GuardarEn = nameof(ClaseRectificativa),
            Fila = 2,
            Columna = 1,
            Obligatorio = true,
            VisibleEnGrid = false
          )
        ]
        public string ClaseRectificativa { get; set; }

        //----------------------------------------------------------------
        [IUPropiedad(Etiqueta = "Detalle del motivo por el que se rectifica"
            , TipoDeControl = enumTipoControl.AreaDeTexto
            , Fila = 3
            , Columna = 0
            , AutoSpan = true
            , Obligatorio = false)]
        public string MotivoDeRectificacion { get; set; }
        

    }
}
