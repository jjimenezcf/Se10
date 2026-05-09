using Utilidades;
using System;
using ServicioDeDatos.Expediente;
using ModeloDeDto.MaestrosTecnico;
using ServicioDeDatos.MaestrosTecnico;

namespace ModeloDeDto.Expediente
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5)]
    public class ApunteDeExpedienteDto : EsUnDetalleDto, IAuditadoDto
    {
        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Orden",
            Ayuda = "orden de la línea",
            Tipo = typeof(int),
            Fila = 1,
            Columna = 0
            )
        ]
        public int Orden { get; set; }

        //-------------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Clase",
            Ayuda = "seleccione la clase de apunte",
            TipoDeControl = enumTipoControl.Enumerado,
            Tipo = typeof(enumApunteDeExpediente),
            GuardarEn = nameof(Clase),
            Fila = 1,
            Columna = 1,
            EditableAlEditar = false
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
            Obligatorio = false,
            MostrarExpresion = nameof(NaturalezaDtm.Expresion),
            EditableAlCrear = true,
            EditableAlEditar = false,
            Fila = 1,
            Columna = 2
            )
        ]
        public string Naturaleza { get; set; }

        //-----------------------------------------------------
        [IUPropiedad(
          Etiqueta = "Concepto",
          Ayuda = "concepto imputado",
          Tipo = typeof(string),
          TipoDeControl = enumTipoControl.Editor,
          Fila = 2,
          Columna = 0,
          Obligatorio = false,
          AutoSpan = true
          )
        ]
        public string Concepto { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Valor",
           Tipo = typeof(decimal),
           Ayuda = "valor de lo imputado",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           Obligatorio = false,
           MantenerHuecoDeLaIzquierda = true,
           Formato = enumFormato.Numero_6,
           Fila = 4,
           Columna = 1)
        ]
        public decimal Valor { get; set; }

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
              PorAnchoMnt = 15
            , Etiqueta = "Imputado el"
            , TipoDeControl = enumTipoControl.SelectorDeFecha
            , VisibleEnGrid = false
            , EditableAlCrear = true
            , EditableAlEditar = true
            , TamanoFijo = "15em"
            , Fila = 4
            , Columna = 2)]
        public DateTime ImputadoEl { get; set; }

        //------------------------------------------------------------------------
        [IUPropiedad(
            VisibleEnGrid = false,
            Etiqueta = "Justificante",
            Ayuda = "Seleccione el fichero justificante del gasto",
            Tipo = typeof(int),
            TipoDeControl = enumTipoControl.SelectorDeUnArchivo,
            ExtensionesValidas = ExtensorDeTipoDeArchivos.NoEditables,
            Fila = 3,
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
         Fila = 4,
         Columna = 0)]
        public string Accion { get;  set; }

        //----------------------------------------------
        [IUPropiedad(Visible = false)]
        public string NombreDeAccion { get; set; } 


        //----------------------------------------------
        [IUPropiedad(Visible = false, Etiqueta = "Guid de descarga")]
        public string GuidDeDescarga { get; set; }


        //----------------------------------------------
        [IUPropiedad(Visible = false, PorAnchoMnt = 20, Etiqueta = "Creado por")]
        public string Creador { get; set; }

        //----------------------------------------------
        [IUPropiedad(Visible = false, Etiqueta = "Id del creador")]
        public int IdCreador { get; set; }

        //----------------------------------------------
        [IUPropiedad(Visible = false, PorAnchoMnt = 20, Etiqueta = "Modificado por")]
        public string Modificador { get; set; }

        //----------------------------------------------
        [IUPropiedad(Visible = false, Etiqueta = "Id del modificador")]
        public int? IdModificador { get; set; }

        //----------------------------------------------
        [IUPropiedad(Visible = false, Etiqueta = "creada el")]
        public DateTime CreadoEl { get; set; }

        //----------------------------------------------
        [IUPropiedad(Visible = false, Etiqueta = "modificada el")]
        public DateTime? ModificadoEl { get; set; }
    }
}
