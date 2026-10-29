using System;
using Utilidades;

namespace ModeloDeDto.Contabilidad
{
    public static class ltrDeUnCierreContable
    {
        public const string Menu_CrearCierre = "Realizar cierre";
        public const string Menu_EliminarCierre = "Eliminar cierre";
        public const string FiltroPorLote = nameof(FiltroPorLote);
    }

    // Los cierres solo los registran los trabajos de crear y anular lote contable, desde la interfaz no se pueden crear, editar ni borrar.
    // Se deja la opción de editar para que el mantenimiento muestre su cabecera (menús y filtros), pero el controlador la fuerza siempre en consulta
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, MostrarExpresion = nameof(Nombre), OpcionDeCrear = false, OpcionDeBorrar = false, OpcionDeEnviar = false, OpcionDeTransitar = false)]
    public class CierreContableDto : ElmentoAuditadoDto
    {
        // AutoSpan: en la edición ocupa las columnas vacías (sociedad y lote solo se ven en el grid) hasta la fecha desde
        [IUPropiedad(Etiqueta = "Cierre", Ayuda = "cierre contable",
            Fila = 0,
            Columna = 0,
            VisibleEnGrid = true,
            Ordenar = true,
            PorAnchoMnt = 25,
            ColSpan = 3,
            EditableAlEditar = false)]
        public string Nombre { get; set; }

        //----------------------------------------------
        [IUPropiedad(Etiqueta = "Id de la sociedad", Visible = false)]
        public int IdSociedad { get; set; }

        [IUPropiedad(Etiqueta = "Sociedad cerrada",
            Ayuda = "ir a la sociedad cerrada",
            VisibleEnGrid = true,
            VisibleEnEdicion = false,
            EditableAlEditar = false,
            Ordenar = true,
            TipoDeControl = enumTipoControl.Referencia,
            AccionRef = "javascript: " + nameof(enumNameSpaceTs.Contabilidad) + "." + nameof(enumFunctionTs.CierreContable_IrALaSociedad) + "(numeroDeFila)")]
        public string Sociedad { get; set; }

        //----------------------------------------------
        [IUPropiedad(Etiqueta = "Id del lote", Visible = false)]
        public int IdLote { get; set; }

        [IUPropiedad(Etiqueta = "Lote",
            Ayuda = "ir al lote contable del cierre",
            VisibleEnGrid = true,
            VisibleEnEdicion = false,
            EditableAlEditar = false,
            TipoDeControl = enumTipoControl.Referencia,
            AccionRef = "javascript: " + nameof(enumNameSpaceTs.Contabilidad) + "." + nameof(enumFunctionTs.CierreContable_IrAlLote) + "(numeroDeFila)")]
        public string Lote { get; set; }

        //----------------------------------------------
        // El cierre traspasa los preasientos con fecha contable desde 'Desde' (incluida) hasta 'Hasta' (no incluida)
        // Calculada: el 'Hasta' del cierre anterior de la sociedad o el 1 de enero si es el primero (ver GestorDeCierresContables.FechaDesdeDelCierre)
        [IUPropiedad(Etiqueta = "Desde",
            Ayuda = "fecha contable desde la que se cierra (incluida)",
            TipoDeControl = enumTipoControl.SelectorDeFecha, Alineada = enumAliniacion.derecha,
            VisibleEnGrid = true,
            Fila = 0,
            Columna = 3,
            Posicion =0,
            EditableAlEditar = false)]
        public DateTime Desde { get; set; }

        [IUPropiedad(Etiqueta = "Hasta",
            Ayuda = "fecha contable hasta la que se cierra (no incluida)",
            TipoDeControl = enumTipoControl.SelectorDeFecha,
            Alineada = enumAliniacion.derecha,
            Fila = 0,
            Columna = 3,
            Posicion =1,
            VisibleEnGrid = true,
            Ordenar = true,
            EditableAlEditar = false)]
        public DateTime CerradaEl { get; set; }

        // Cuándo se realizó el cierre (creación del registro)
        [IUPropiedad(Etiqueta = "Cerrado el",
            Ayuda = "fecha y hora en la que se realizó el cierre",
            TipoDeControl = enumTipoControl.SelectorDeFechaHora,
            Alineada = enumAliniacion.derecha,
            Fila = 0,
            Columna = 4,
            VisibleEnGrid = true,
            EditableAlEditar = false)]
        public DateTime CerradoEl { get; set; }

        // Calculadas: asientos del lote por origen (ver GestorDeCierresContables.DespuesDeMapearElElemento)
        [IUPropiedad(Etiqueta = "Recibidas",
            Ayuda = "asientos de facturas recibidas del lote",
            Tipo = typeof(int),
            Alineada = enumAliniacion.derecha,
            Fila = 1,
            Columna = 3,
            Posicion =0,
            MantenerHuecoDeLaIzquierda =true,
            VisibleEnGrid = true,
            EditableAlEditar = false, 
            ColSpan = 2)]
        public int Recibidas { get; set; } 

        [IUPropiedad(Etiqueta = "Pagos",
            Ayuda = "asientos de pagos del lote",
            Tipo = typeof(int),
            Alineada = enumAliniacion.derecha,
            Fila = 1,
            Columna = 3,
            Posicion = 1,
            VisibleEnGrid = true,
            EditableAlEditar = false)]
        public int Pagos { get; set; }

        [IUPropiedad(Etiqueta = "Emitidas",
            Ayuda = "asientos de facturas emitidas del lote",
            Tipo = typeof(int),
            Alineada = enumAliniacion.derecha,
            Fila = 1,
            Columna = 3,
            Posicion = 2,
            VisibleEnGrid = true,
            EditableAlEditar = false)]
        public int Emitidas { get; set; }

        [IUPropiedad(Etiqueta = "Cobros",
            Ayuda = "asientos de cobros del lote",
            Tipo = typeof(int),
            Alineada = enumAliniacion.derecha,
            Fila = 1,
            Columna = 3,
            Posicion = 3,
            VisibleEnGrid = true,
            EditableAlEditar = false)]
        public int Cobros { get; set; }


        [IUPropiedad(Etiqueta = "Almacén",
            Ayuda = "asientos de movimientos de almacén del lote",
            Tipo = typeof(int),
            Alineada = enumAliniacion.derecha,
            Fila = 1,
            Columna = 3,
            Posicion = 4,
            VisibleEnGrid = true,
            EditableAlEditar = false)]
        public int Almacen { get; set; }

        // Calculada: solo se puede eliminar el último cierre realizado de la sociedad
        [IUPropiedad(Etiqueta = "Es el último cierre", Visible = false)]
        public bool EsElUltimoCierre { get; set; }
    }
}
