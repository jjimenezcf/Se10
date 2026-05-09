using Utilidades;

namespace ModeloDeDto.Presupuesto
{
    [IUDto(MostrarExpresion = nameof(IUsaNombreDto.Nombre))]
    public class RenombrarPptDto : IRenombrarDto
    {
        [IUPropiedad(
            Etiqueta = "Presupuesto",
            Ayuda = "Presupuesto a renombrar",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Elemento),
            Fila = 0,
            Columna = 0,
            VisibleEnGrid = false,
            Controlador = nameof(enumControladoresVentas.Presupuestos),
            VistaDondeNavegar = enumVistasVentas.CrudPresupuestos
            )
        ]
        public int IdElemento { get; set; }

        [IUPropiedad(Etiqueta = "Presupuesto", Visible = false)]
        public string Elemento { get; set; }


        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = "Nombre",
            Ayuda = "Indique el nuevo nombre",
            Tipo = typeof(string),
            Fila = 1,
            Columna = 0,
            Obligatorio = true,
            LongitudMaxima = 250
          )
        ]
        public string Nombre { get; set; }
    }
}
