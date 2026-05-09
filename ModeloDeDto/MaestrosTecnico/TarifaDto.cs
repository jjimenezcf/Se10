using ServicioDeDatos;
using Utilidades;
using ModeloDeDto.Terceros;

namespace ModeloDeDto.MaestrosTecnico
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, OpcionDeBorrar = false)]
    public class TarifaDto : EsUnDetalleDto
    {   
        //----------------------------------------------------------
        [IUPropiedad(Etiqueta = "Id del proveedor",Visible = false)]
        public int IdProveedor { get; set; }


        [IUPropiedad(
            Etiqueta = "Proveedor",
            Ayuda = "Indique el proveedor",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(ProveedorDto),
            GuardarEn = nameof(IdProveedor),
            RestringidoPorControl = nameof(IdElemento),
            Controlador = nameof(enumControladoresTerceros.Proveedores),
            VistaDondeNavegar = enumVistasTerceros.CrudProcuradores,
            BuscarPor = nameof(ProveedorDto.Expresion),
            CriterioDeBusqueda = enumCriteriosDeFiltrado.contiene,
            LongitudMinimaParaBuscar = 3,
            Fila = 1,
            Columna = 0,
            AutoSpan = true
            )
        ]
        public string Proveedor { get; set; }

        //-----------------------------------------------------
        [IUPropiedad(
          Etiqueta = "Referencia",
          Ayuda = "Referencia del proveedor",
          Tipo = typeof(string),
          TipoDeControl = enumTipoControl.Editor,
          Fila = 2,
          Columna = 0,
          Obligatorio = true
          )
        ]
        public string Referencia { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Tarifa",
           Tipo = typeof(decimal),
           Ayuda = "Tarifa de compra",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           Fila = 2,
           Columna = 1)
        ]
        public decimal Tarifa { get; set; }
    }
}
