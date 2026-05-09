using Utilidades;
using ServicioDeDatos.Contabilidad;
using System;

namespace ModeloDeDto.Terceros
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5)]
    public class CuentaDeTrabajadorDto : EsUnDetalleDto, IAuditadoDto
    {

        //-------------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Clase de cuenta",
            Ayuda = "seleccione la clase de cuenta",
            TipoDeControl = enumTipoControl.Enumerado,
            Tipo = typeof(enumClaseDeCuentaBancaria),
            GuardarEn = nameof(Clase),
            Fila = 0,
            Columna = 3,
            ColSpan = 3,
            EditableAlEditar = false
          )
        ]
        public string Clase { get; set; }

        //----------------------------------------------------------
        [IUPropiedad(Visible = false)]
        public string Cuenta => $"{Iban}-{Entidad}-{Oficina}-{DcCcc}-{Numero}";

        //----------------------------------------------------------
        [IUPropiedad(Visible = false)]
        public int IdCuenta { get; set; }

        //----------------------------------------------------------
        [IUPropiedad(Etiqueta = "", Ayuda = "Iban (País-DC) o Pegue el nº de cuenta", 
            Fila = 1, 
            Columna = 0, 
            LongitudMaxima = 4, 
            EditableAlEditar = false,
            OnPaste = "javascript:" + nameof(enumNameSpaceTs.Terceros) + "." + nameof(enumFunctionTs.Trb_AlPegar_Iban) + "(event)")
        ]
        public string Iban { get; set; }
        [IUPropiedad(Etiqueta = "", Ayuda = "Entidad", Fila = 1, Columna = 1, LongitudMaxima = 4, EditableAlEditar = false)]
        public string Entidad { get; set; }
        [IUPropiedad(Etiqueta = "", Ayuda = "Oficina", Fila = 1, Columna = 2, LongitudMaxima = 4, EditableAlEditar = false)]
        public string Oficina { get; set; }
        [IUPropiedad(Etiqueta = "", Ayuda = "DC", Fila = 1, Columna = 3, Posicion = 0, AnchoMaximo ="3em", LongitudMaxima = 2, EditableAlEditar = false)]
        public string DcCcc { get; set; }
        [IUPropiedad(Etiqueta = "", Ayuda = "Número", Fila = 1, Columna = 3, Posicion = 1, LongitudMaxima =10, EditableAlEditar = false)]
        public string Numero { get; set; }

        //------------------------------------------------------------------------
        [IUPropiedad(
            VisibleEnGrid = false,
            Etiqueta = "Certificado bancario",
            Ayuda = "Seleccione el fichero de certificado bancario",
            Tipo = typeof(int),
            TipoDeControl = enumTipoControl.SelectorDeUnArchivo,
            ExtensionesValidas = ExtensorDeTipoDeArchivos.NoEditables,
            Fila = 3,
            Columna = 0,
            AutoSpan = true)]
        public int? IdArchivo { get; set; }

        //----------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Nombre del archivo",
            Ayuda = "nombre del archivo del certificado",
            Fila = 4,
            Columna = 0,
            VisibleAlCrear = false,
            EditableAlEditar = false,
            AutoSpan = true
            )
        ]
        public string NombreArchivo { get; set; }

        //------------------------------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Activa",
           Ayuda = "indica si la cuenta del trabajador está activa",
           VisibleEnGrid = false,
           Obligatorio = false,
           Fila = 5,
           Columna = 0,
           TipoDeControl = enumTipoControl.Check,
           css = enumCssControles.ControlApilado,
           ValorPorDefecto = true,
           VisibleAlCrear = false,
           EditableAlEditar = true,
           OnChange = "javascript:" + nameof(enumNameSpaceTs.Terceros) + "." + nameof(enumFunctionTs.Trb_AlCambiar_CuentaActiva) + "(this)"
           )]
        public bool Activa { get; set; }


        //----------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Alias",
            Ayuda = "Alias de la cuenta",
            Fila = 2,
            Columna = 0,
            VisibleAlCrear = true,
            EditableAlEditar = true,
            AutoSpan = true,
            LongitudMaxima = 250,
            Obligatorio = false
            )
        ]
        public string Alias { get; set; }

        //----------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Banco",
            Ayuda = "Banco asociado a la cuenta",
            Fila = 2,
            Columna = 3,
            VisibleAlCrear = false,
            EditableAlEditar = false,
            ColSpan = 2
            )
        ]
        public string Banco { get; set; }
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
