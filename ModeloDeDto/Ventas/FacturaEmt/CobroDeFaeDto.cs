using Utilidades;
using ServicioDeDatos.Ventas;
using ModeloDeDto.Terceros;
using System;

namespace ModeloDeDto.Ventas
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, OpcionDeBorrar = false)]
    public class CobroDeFaeDto : EsUnDetalleDto
    {

        //-------------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Clase de cobro",
            Ayuda = "indique la clase de cobro",
            TipoDeControl = enumTipoControl.Enumerado,
            Tipo = typeof(enumClaseDeCobro),
            GuardarEn = nameof(Clase),
            OnChange = "javascript:" + nameof(enumNameSpaceTs.Venta) + "." + nameof(enumFunctionTs.Fae_Tras_Cambiar_Clase_De_Cobro) + "()",
            AutoSpan = true,
            Fila = 0,
            Columna = 1,
            EditableAlEditar = false
          )
        ]
        public string Clase { get; set; }


        //-------------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Id de la cuenta bancaria",
            Visible = false
            )
        ]
        public int? IdCuentaDeIngreso { get; set; }

        [IUPropiedad(
            Etiqueta = "Cuenta de ingreso",
            Ayuda = "seleccione la cuenta de ingreso",
            TipoDeControl = enumTipoControl.ListaDeElemento,
            SeleccionarDe = typeof(CuentaDeMiSociedadDto),
            Controlador = nameof(enumControladoresTerceros.CuentasDeMiSociedad),
            GuardarEn = nameof(IdCuentaDeIngreso),
            Obligatorio = false,
            RestrictorFijo = nameof(ltrParametrosEp.negocio) + ";" + nameof(enumNegocio.FacturaEmitida),
            RestringidoPorControl = nameof(IdElemento),
            MostrarExpresion = "([" + nameof(CuentaDeMiSociedadDto.Alias) + "]) " + "[" + nameof(CuentaDeMiSociedadDto.Cuenta) + "]",
            EditableAlCrear = true,
            EditableAlEditar = false,
            AutoSpan = true,
            Fila =1,
            Columna = 0,
            Posicion =0
            )
        ]
        public string CuentaDeIngreso { get; set; }


        //--------------------------------------------------------
        [IUPropiedad(Oculto = true)]
        public int? IdCuentaDeCargo { get; set; }

        [IUPropiedad(
           Etiqueta = "Cuenta de cargo",
           Tipo = typeof(string),
           Ayuda = "Cuenta de cargo indicada en la remesa",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           Obligatorio = false,
           EditableAlCrear = false,
           EditableAlEditar = false,
           AutoSpan = true,
           Fila = 1,
           Columna = 0,
           Posicion = 1)
        ]
        public string CuentaDeCargo { get; set; }

        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Cobrado el",
            EtiquetaGrid = "Fecha de cobro",
            PorAnchoSel = 20,
            Ayuda = "Fecha del cobro",
            TipoDeControl = enumTipoControl.SelectorDeFechaHora,
            Fila = 2,
            Columna = 0,
            Obligatorio = true
           )
        ]
        public DateTime CobradoEl { get; set; }

        //--------------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Pendiente",
           Tipo = typeof(decimal),
           Ayuda = "importe pendiente",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           Obligatorio = false,
           MantenerHuecoDeLaIzquierda = true,
           Fila = 2,
           Columna = 1,
           Posicion =0,
           Formato = enumFormato.Moneda,
           EditableAlCrear = false,
           EditableAlEditar = false)
        ]
        public decimal Pendiente { get; set; }

        //--------------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Cobrado",
           Tipo = typeof(decimal),
           Ayuda = "importe cobrado",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           Obligatorio = true,
           Fila =2,
           Formato = enumFormato.Moneda,
           Columna = 1,
           Posicion =1)
        ]
        public decimal Cobrado { get; set; }


    }
}
