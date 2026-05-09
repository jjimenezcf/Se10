using System;
using ModeloDeDto.Terceros;
using ServicioDeDatos.Ventas;
using Utilidades;
/*
0: cg:(select2)   tipo: (select2)  estado: (editor en consulta)
1: referencia:(editor en consulta) | nombre: (editor)
2: descripción: (text area)
3: Acreedor: (sociedad del CG + NIF, solo consulta) | Presentador: (editor, por defecto acreedor) | Nif Presentador:  (editor, por defecto acreedor)
4: Abonar en: (lista) | Entidad: (solo consulta, del iban) Oficina: (solo consulta, del iban)
5: Generada El: (selector de fecha) Presentada el(selector de fecha, solo lectura) | Cargar El: (selector de fecha)  Importe (solo lectura) | Cobrado(sl) devuelto (sl)  
 * */
namespace ModeloDeDto.Ventas
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, MostrarExpresion = nameof(IUsaNombreDto.Nombre), EditarTrasCrear = true)]
    public class RemesaFaeDto : ElementoDeUnProcesoDto
    {

        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = "Acreedor",
            Ayuda = "Acreedor u ordenante de la remesa",
            TipoDeControl = enumTipoControl.Editor,
            VisibleEnGrid = false,
            Fila = 3,
            Columna = 0,
            Posicion = 0,
            EditableAlCrear = false,
            EditableAlEditar = false
            )
        ]
        public string Acreedor { get; set; }

        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = "Sufijo",
            Ayuda = "Sufijo del acreedor",
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
        public string SufijoAcreedor { get; set; }

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
        [IUPropiedad(Etiqueta = "Id la cuenta de abono", Visible = false)]
        public int IdCuentaDeAbono { get; set; }

        [IUPropiedad(
            Etiqueta = "Abonar en",
            Ayuda = "Seleccione la cuenta bancaria de abono",
            TipoDeControl = enumTipoControl.ListaDeElemento,
            SeleccionarDe = typeof(CuentaDeMiSociedadDto),
            Controlador = nameof(enumControladoresTerceros.CuentasDeMiSociedad),
            MostrarExpresion = nameof(CuentaDeMiSociedadDto.Cuenta),
            GuardarEn = nameof(IdCuentaDeAbono),
            RestringidoPorControl =nameof(IdSociedadDelCg),
            CargarBajoDemanda = true,
            VisibleEnGrid = true,
            AutoSpan = true,
            Fila = 4,
            Columna = 0,
            OnChange = "javascript:" + nameof(enumNameSpaceTs.Venta) + "." + nameof(enumFunctionTs.Rem_Tras_Seleccionar_Cuenta_Abono) + "()"
            )
        ]
        public string CuentaDeAbono { get; set; }

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
            Tipo = typeof(enumClaseDeRemesaFae),
            GuardarEn = nameof(Clase),
            Fila = 4,
            Columna = 2,
            Posicion = 0,
            EditableAlCrear = true,
            EditableAlEditar = false
          )
        ]
        public enumClaseDeRemesaFae Clase { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Incluidas",
           EtiquetaGrid = "Incluidas",
           Tipo = typeof(int),
           Ayuda = "facturas incluidas en la remesa",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Fila = 4,
           Columna = 2,
           Posicion = 1
            )
        ]
        public decimal? Incluidas { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Devueltas",
           EtiquetaGrid = "Devueltas",
           Tipo = typeof(int),
           Ayuda = "facturas devueltas en la remesa",
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
        public decimal? Devueltas { get; set; }

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
            , Etiqueta = "Cargar el"
            , Ordenar = true
            , TipoDeControl = enumTipoControl.SelectorDeFecha
            , VisibleEnGrid = false
            , EditableAlCrear = false
            , EditableAlEditar = false
            , Formato = enumFormato.Fecha
            , Fila = 5
            , Columna = 1
            , Posicion = 0)]
        public DateTime? CargarEl { get; set; }

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
             Etiqueta = "Cargada el"
            , TipoDeControl = enumTipoControl.SelectorDeFecha
            , VisibleEnGrid = false
            , EditableAlCrear = false
            , EditableAlEditar = false
            , Formato = enumFormato.Fecha
            , Fila = 5
            , Columna = 1
            , Posicion = 1)]
        public DateTime? CargadaEl { get; set; }

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
           Etiqueta = "Cobrado",
           EtiquetaGrid = "Cobrado",
           Tipo = typeof(decimal),
           Ayuda = "importe de la remesa cobrado",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Obligatorio = false,
           VisibleEnGrid = false,
           Fila = 5,
           Columna = 2,
           Posicion = 1,
           Formato = enumFormato.Moneda
            )
        ]
        public decimal? Cobrado { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Pendiente",
           EtiquetaGrid = "Pendiente",
           Tipo = typeof(decimal),
           Ayuda = "importe pendiente de cobro",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Obligatorio = false,
           VisibleEnGrid = false,
           Fila = 5,
           Columna = 2,
           Posicion = 2,
           Formato = enumFormato.Moneda
            )
        ]
        public decimal? Pendiente { get; set; }
    }


}
