using Utilidades;

namespace ModeloDeDto.Expediente
{
    [IUDto()]
    public class TotalesDeExpedientes : TotalesDto
    {
       
        //-----------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Presupuestado",
           Tipo = typeof(decimal),
           Ayuda = "total de los presupuestos seleccionados",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Fila = 0,
           Columna = 0,
           Formato = enumFormato.Moneda
            )
        ]
        public decimal Presupuestado { get; set; }

        //-----------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Gastos",
           Tipo = typeof(decimal),
           Ayuda = "total de gastos imputados",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Fila = 0,
           Columna = 1,
           Formato = enumFormato.Moneda
            )
        ]
        public decimal Gastos { get; set; }
       
        //-----------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Ingresos",
           Tipo = typeof(decimal),
           Ayuda = "importe de ingresos generados",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Fila = 0,
           Columna = 2,
           Formato = enumFormato.Moneda
            )
        ]
        public decimal Ingresos { get; set; }

        //-----------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Pagos",
           Tipo = typeof(decimal),
           Ayuda = "total de pagos realizado",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           MantenerHuecoDeLaIzquierda = true,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Fila = 1,
           Columna = 1,
           Formato = enumFormato.Moneda
            )
        ]
        public decimal Pagos { get; set; }

        //-----------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Cobrado",
           Tipo = typeof(decimal),
           Ayuda = "importe de total de cobro realizados",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Fila = 1,
           Columna = 2,
           Formato = enumFormato.Moneda
            )
        ]
        public decimal Cobros { get; set; }


        //-----------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Margen",
           Tipo = typeof(decimal),
           Ayuda = "Margen de beneficio",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           MantenerHuecoDeLaIzquierda = true,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Fila = 2,
           Columna = 1,
           Formato = enumFormato.Porcentaje
            )
        ]
        public decimal Margen { get; set; }

        //-----------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Rentabilidad",
           Tipo = typeof(decimal),
           Ayuda = "Rentabilidad sobre lo invertido",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Fila = 2,
           Columna = 2,
           Formato = enumFormato.Porcentaje
            )
        ]
        public decimal Rentabilidad { get; set; }

        //-----------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Tareas del expediente",
           Ayuda = "muestra la información de los totales calculados",
           TipoDeControl = enumTipoControl.AreaDeTexto,
           EditableAlCrear = false,
           EditableAlEditar = false,
           NumeroDeFilas = 7,
           Fila = 3,
           Columna = 0
            )
        ]
        public string Totales { get; set; }

    }
}
