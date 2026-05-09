using ModeloDeDto.Expediente;
using ModeloDeDto.MaestrosTecnico;
using ModeloDeDto.Negocio;
using ModeloDeDto.Tarea;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.MaestrosTecnico;
using ServicioDeDatos.Seguridad;
using System;
using Utilidades;
using static ServicioDeDatos.Elemento.Enumerados;

namespace ModeloDeDto.Ventas
{
    [IUDto(MostrarExpresion = nameof(IUsaNombreDto.Nombre))]
    public class FacturarTareasDto : ISelectorDto
    {       
        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
         Etiqueta = "Id de la factura seleccionada",
         Visible = false
         )]
        public int IdElemento { get; set; }

        [IUPropiedad(
            Etiqueta = "Factura",
            Ayuda = "factura seleccionada",
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(IdElemento),
            Controlador = nameof(enumControladoresVentas.FacturasEmt),
            SeleccionarDe = typeof(FacturaEmtDto),
            VistaDondeNavegar = enumVistasVentas.CrudFacturasEmt,
            RestrictorFijo = ltrParametrosDto.Negocio + ";" + nameof(enumNegocio.FacturaEmitida) + ";" + nameof(enumModoDeAccesoDeDatos.Consultor),
            Negocio = enumNegocio.FacturaEmitida,
            LongitudMinimaParaBuscar = 1,
            MostrarExpresion = nameof(FacturaEmtDto.Expresion),
            Tipo = typeof(string),
            Fila = 0,
            Columna = 0,
            EditableAlCrear = false,
            EditableAlEditar = false,
            AutoSpan = true)]
        public string Elemento { get; set; }

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
         Etiqueta = "Id del expediente con tareas a asociar",
         Visible = false
         )]
        public int IdExpediente { get; set; }

        [IUPropiedad(
            Etiqueta = "Expediente",
            Ayuda = "expediente con tareas a asociar",
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(IdExpediente),
            BuscarPor = ltrDeUnExpediente.AsociarTareasDelExpediente,
            OrdenarListaDinamicaPor = nameof(ExpedienteDtm.Referencia),
            Controlador = nameof(enumControladoresAdministrativos.Expedientes),
            SeleccionarDe = typeof(ExpedienteDto),
            VistaDondeNavegar = enumVistasAdministrativo.CrudExpedientes,
            RestrictorFijo = ltrParametrosDto.Negocio + ";" + nameof(enumNegocio.Expediente) + ";" + nameof(enumModoDeAccesoDeDatos.Consultor),
            Negocio = enumNegocio.Expediente,
            MostrarExpresion = nameof(ExpedienteDto.Expresion),
            LongitudMinimaParaBuscar = 1,
            Tipo = typeof(string),
            Fila = 1,
            Columna = 0,
            Obligatorio = false,
            EditableAlCrear = true,
            EditableAlEditar = false,
            AutoSpan = true)]
        public string Expediente { get; set; }
        
        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(
              Etiqueta = "Id del tipo de tareas",
              Visible = false
              )]
        public int IdTipoTarea { get; set; }

        [IUPropiedad(
            Etiqueta = "Del tipo",
            Ayuda = "Tipo de tareas a asociar",
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(IdTipoTarea),
            SeleccionarDe = typeof(TipoDeTareaDto),
            RestrictorFijo = ltrParametrosDto.Negocio + ";" + nameof(enumNegocio.Tarea) + ";" + nameof(enumModoDeAccesoDeDatos.Consultor),
            Controlador = nameof(enumControladoresNegocio.TiposDeElemento),
            VistaDondeNavegar = enumVistasAdministrativo.TiposDeTarea,
            MostrarExpresion = nameof(TipoDeElementoDto.Nombre),
            SoloEnAlta = true,
            LongitudMinimaParaBuscar = 1,
            Negocio = enumNegocio.Tarea,
            Tipo = typeof(string),
            Fila = 2,
            Columna = 0,
            Obligatorio = false,
            EditableAlCrear = true,
            EditableAlEditar = false,
            AutoSpan = true)]
        public string TipoTarea { get; set; }

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
             Etiqueta = "Desde"
            , TipoDeControl = enumTipoControl.SelectorDeFecha
            , EditableAlCrear = true
            , EditableAlEditar = false
            , SelectorHasta = nameof(PlfDeFin) + ":30"
            , Fila = 2
            , Columna = 1
            , Posicion = 0
            , Obligatorio = false)]
        public DateTime? PlfDeInicio { get; set; }

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
             Etiqueta = "Hasta"
            , TipoDeControl = enumTipoControl.SelectorDeFecha
            , EditableAlCrear = true
            , EditableAlEditar = true
            , Fila = 2
            , Columna = 1
            , Posicion = 1
            , Obligatorio = false)]
        public DateTime? PlfDeFin { get; set; }

        //------------------------------------------------
        [IUPropiedad(
            Etiqueta = "medido en",
            Ayuda = "Indique unidad de facturación",
            TipoDeControl = enumTipoControl.Enumerado,
            Tipo = typeof(enumDurabilidad),
            GuardarEn = nameof(MedidoEn),
            Fila = 2,
            Columna = 2,
            Obligatorio = false
          )
        ]
        public enumDurabilidad? MedidoEn { get; set; }

        //----------------------------------------------
        [IUPropiedad(Etiqueta = "Id de la naturaleza contable", Visible = false)]
        public int? IdNaturaleza { get; set; }

        [IUPropiedad(
            Etiqueta = "Naturaleza",
            Ayuda = "Seleccione la naturaleza contable",
            TipoDeControl = enumTipoControl.ListaDeElemento,
            SeleccionarDe = typeof(NaturalezaDto),
            Controlador = nameof(enumControladoresMt.Naturalezas),
            GuardarEn = nameof(IdNaturaleza),
            Obligatorio = false,
            MostrarExpresion = nameof(NaturalezaDtm.Expresion),
            EditableAlCrear = true,
            Fila = 3,
            Columna = 1
            )
        ]
        public string Naturaleza { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Precio unitario",
           Tipo = typeof(decimal),
           Ayuda = "indicar precio por unidad calculada",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           Obligatorio = false,
           MantenerHuecoDeLaIzquierda = true,
           Formato = enumFormato.Numero_6,
           Fila = 3,
           Columna = 2)
        ]
        public decimal? Precio { get; set; }

        //-------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Totalizar tareas",
            Ayuda = "indica si tras asociar se crea un línea de factura por el total ",
            EditableAlCrear = true,
            EditableAlEditar = false,
            Obligatorio = false,
            Fila = 3,
            Columna = 0,
            TipoDeControl = enumTipoControl.Check,
            ValorPorDefecto = true,
            css = enumCssControles.CheckEnLinea
            )
        ]
        public bool CrearLineaPorTotal { get; set; }

        //-------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Línea por tarea ",
            Ayuda = "indica si tras asociar se crea un línea de factura por cada tarea asociada ",
            EditableAlCrear = true,
            EditableAlEditar = false,
            Obligatorio = false,
            Fila = 3,
            Columna = 0,
            TipoDeControl = enumTipoControl.Check,
            ValorPorDefecto = false,
            css = enumCssControles.CheckEnLinea
            )
        ]
        public bool CrearLineasPorTarea { get; set; }

    }
}
