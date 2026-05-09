using System;
using ModeloDeDto.Terceros;
using ServicioDeDatos.Gastos;
using Utilidades;
/*
0: cg:(select2)   tipo: (select2)  estado: (editor en consulta)
1: referencia:(editor en consulta) | nombre: (editor)
2: descripción: (text area)
3: Deudor: (sociedad del CG + NIF, solo consulta) | Presentador: (editor, por defecto Deudor) | Nif Presentador:  (editor, por defecto Deudor)
4: Abonar en: (lista) | Entidad: (solo consulta, del iban) Oficina: (solo consulta, del iban)
5: Generada El: (selector de fecha) Presentada el(selector de fecha, solo lectura) | Pagar El: (selector de fecha)  Importe (solo lectura) | Cobrado(sl) devuelto (sl)  
 * */
namespace ModeloDeDto.Gastos
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, MostrarExpresion = nameof(IUsaNombreDto.Nombre), EditarTrasCrear = true)]
    public class RemesaPagDto : ElementoDeUnProcesoDto
    {

        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = "Deudor",
            Ayuda = "Deudor u ordenante de la remesa",
            TipoDeControl = enumTipoControl.Editor,
            VisibleEnGrid = false,
            Fila = 3,
            Columna = 0,
            Posicion = 0,
            EditableAlCrear = false,
            EditableAlEditar = false
            )
        ]
        public string Deudor { get; set; }

        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = "Sufijo",
            Ayuda = "Sufijo del Deudor",
            TipoDeControl = enumTipoControl.Editor,
            VisibleEnGrid = false,
            Fila = 3,
            Columna = 0,
            Posicion = 1,
            LongitudMaxima = 3,
            ValorPorDefecto = "000",
            AnchoMaximo = "6em",
            Alineada = enumAliniacion.derecha,
            EditableAlCrear = true,
            EditableAlEditar = true
            )
        ]
        public string SufijoDeudor { get; set; }

        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = "Presentador",
            Ayuda = "presentador de la remesa",
            TipoDeControl = enumTipoControl.Editor,
            VisibleEnGrid = false,
            Fila = 3,
            Columna = 1,
            EditableAlCrear = true,
            EditableAlEditar = true
            )
        ]
        public string Presentador { get; set; }

        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = "NIF del Presentador",
            Ayuda = "Nif del presentador de la remesa",
            TipoDeControl = enumTipoControl.Editor,
            VisibleEnGrid = false,
            Fila = 3,
            Columna = 2,
            Posicion = 0,
            EditableAlCrear = true,
            EditableAlEditar = true
            )
        ]
        public string NifDelPresentador { get; set; }

        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = "Sufijo",
            Ayuda = "Sufijo del presentador",
            TipoDeControl = enumTipoControl.Editor,
            VisibleEnGrid = false,
            Fila = 3,
            Columna = 2,
            Posicion = 1,
            LongitudMaxima = 3,
            ValorPorDefecto = "000",
            AnchoMaximo = "6em",
            Alineada = enumAliniacion.derecha,
            EditableAlCrear = true,
            EditableAlEditar = true
            )
        ]
        public string SufijoPresentador { get; set; }


        //----------------------------------------------
        [IUPropiedad(Etiqueta = "Id la cuenta de pago", Visible = false)]
        public int IdCuentaDePago { get; set; }

        [IUPropiedad(
            Etiqueta = "Pagar de",
            Ayuda = "Seleccione la cuenta bancaria de pago",
            TipoDeControl = enumTipoControl.ListaDeElemento,
            SeleccionarDe = typeof(CuentaDeMiSociedadDto),
            Controlador = nameof(enumControladoresTerceros.CuentasDeMiSociedad),
            MostrarExpresion = nameof(CuentaDeMiSociedadDto.Cuenta),
            GuardarEn = nameof(IdCuentaDePago),
            RestringidoPorControl =nameof(IdSociedadDelCg),
            CargarBajoDemanda = true,
            VisibleEnGrid = true,
            AutoSpan = true,
            Fila = 4,
            Columna = 0,
            OnChange = "javascript:" + nameof(enumNameSpaceTs.Gasto) + "." + nameof(enumFunctionTs.Rem_Tras_Seleccionar_Cuenta_Pago) + "()"
            )
        ]
        public string CuentaDePago { get; set; }

        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = "Entidad",
            Ayuda = "Entidad bancaria",
            TipoDeControl = enumTipoControl.Editor,
            VisibleEnGrid = false,
            Fila = 4,
            Columna = 1,
            Posicion = 0,
            Alineada = enumAliniacion.derecha,
            EditableAlCrear = false,
            EditableAlEditar = false
            )
        ]
        public string Entidad { get; set; }

        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = "Oficina",
            Ayuda = "Oficina bancaria",
            TipoDeControl = enumTipoControl.Editor,
            VisibleEnGrid = false,
            Fila = 4,
            Columna = 1,
            Posicion = 1,
            Alineada = enumAliniacion.derecha,
            EditableAlCrear = false,
            EditableAlEditar = false
            )
        ]
        public string Oficina { get; set; }

        //-------------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Clase de remesa",
            Ayuda = "indique la clase de remesa",
            TipoDeControl = enumTipoControl.Enumerado,
            Tipo = typeof(enumClaseDeRemesaPag),
            GuardarEn = nameof(Clase),
            Fila = 4,
            Columna = 2,
            Posicion = 0,
            EditableAlCrear = true,
            EditableAlEditar = false
          )
        ]
        public string Clase { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Incluidos",
           EtiquetaGrid = "Incluidos",
           Tipo = typeof(int),
           Ayuda = "pagos incluidos en la remesa",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Fila = 4,
           Columna = 2,
           Posicion = 1
            )
        ]
        public decimal? Incluidos { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "No realizados",
           EtiquetaGrid = "No realizados",
           Tipo = typeof(int),
           Ayuda = "pagos no realizados en la remesa",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           VisibleEnGrid = false,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Fila = 4,
           Columna = 2,
           Posicion = 2
            )
        ]
        public decimal? NoRealizados { get; set; }

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
              PorAnchoMnt = 15
            , Etiqueta = "Generada el"
            , TipoDeControl = enumTipoControl.SelectorDeFecha
            , VisibleEnGrid = false
            , EditableAlCrear = false
            , EditableAlEditar = false
            , Fila = 5
            , Columna = 0
            , Posicion = 0
            , Alineada = enumAliniacion.centrada)]
        public DateTime? GeneradaEl { get; set; }

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
              PorAnchoMnt = 15
            , Etiqueta = "Presentada el"
            , TipoDeControl = enumTipoControl.SelectorDeFecha
            , VisibleEnGrid = false
            , EditableAlCrear = false
            , EditableAlEditar = false
            , Formato = enumFormato.Fecha
            , Fila = 5
            , Columna = 0
            , Posicion = 1
            , Alineada = enumAliniacion.centrada)]
        public DateTime? PresentadaEl { get; set; }

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
              PorAnchoMnt = 15
            , Etiqueta = "Pagar el"
            , Ordenar = true
            , TipoDeControl = enumTipoControl.SelectorDeFecha
            , VisibleEnGrid = false
            , EditableAlCrear = true
            , EditableAlEditar = true
            , Formato = enumFormato.Fecha
            , Fila = 5
            , Columna = 1
            , Posicion = 0)]
        public DateTime? PagarEl { get; set; }

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
             Etiqueta = "Pagada el"
            , TipoDeControl = enumTipoControl.SelectorDeFecha
            , VisibleEnGrid = false
            , EditableAlCrear = false
            , EditableAlEditar = false
            , Formato = enumFormato.Fecha
            , Fila = 5
            , Columna = 1
            , Posicion = 1)]
        public DateTime? PagadaEl { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Importe",
           Tipo = typeof(decimal),
           EtiquetaGrid = "Importe",
           Ayuda = "Importe de la remesa",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Obligatorio = false,
           Fila = 5,
           Columna = 2,
           Posicion = 0,
           Formato = enumFormato.Moneda
            )
        ]
        public decimal? ImporteRemesa { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Pagado",
           Tipo = typeof(decimal),
           EtiquetaGrid = "Pagado",
           Ayuda = "Importe pagado",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Obligatorio = false,
           Fila = 5,
           Columna = 2,
           Posicion = 1,
           Formato = enumFormato.Moneda
            )
        ]
        public decimal? Pagado { get; set; }

    }


}
