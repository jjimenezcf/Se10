using ModeloDeDto.Gastos;
using ModeloDeDto.Terceros;
using ServicioDeDatos;
using System;
using Utilidades;


namespace ModeloDeDto.Expediente
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, OpcionDeBorrar = false)]
    public class DatosJuridicosDto : EsUnaAmpliacionDto
    {
        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = nameof(Abogado),
           Ayuda = "Seleccione el abogado",
           TipoDeControl = enumTipoControl.ListaDinamica,
           SeleccionarDe = typeof(AbogadoDto),
           GuardarEn = nameof(IdAbogado),
           Controlador = nameof(enumControladoresTerceros.Abogados),
           VistaDondeNavegar = enumVistasTerceros.CrudAbogados,
           CriterioDeBusqueda = enumCriteriosDeFiltrado.comienza,
           LongitudMinimaParaBuscar = 1,
           Fila = 0,
           Columna = 0,
           Obligatorio = false
           )
        ]
        public string Abogado { get; set; }

        [IUPropiedad(Etiqueta = "Id del abogado", Visible = false)]
        public int? IdAbogado { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = nameof(Procurador),
           Ayuda = "Seleccione el procurador",
           TipoDeControl = enumTipoControl.ListaDinamica,
           SeleccionarDe = typeof(ProcuradorDto),
           GuardarEn = nameof(IdProcurador),
           Controlador = nameof(enumControladoresTerceros.Procuradores),
           VistaDondeNavegar = enumVistasTerceros.CrudProcuradores,
           CriterioDeBusqueda = enumCriteriosDeFiltrado.comienza,
           LongitudMinimaParaBuscar = 1,
           Fila = 0,
           Columna = 0,
           Obligatorio = false
           )
        ]
        public string Procurador { get; set; }

        [IUPropiedad(Etiqueta = "Id del procurador", Visible = false)]
        public int? IdProcurador { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = nameof(Juzgado),
           Ayuda = "Seleccione el juzgado",
           TipoDeControl = enumTipoControl.ListaDinamica,
           SeleccionarDe = typeof(JuzgadoDto),
           GuardarEn = nameof(IdProcurador),
           Controlador = nameof(enumControladoresTerceros.Juzgados),
           VistaDondeNavegar = enumVistasTerceros.CrudJuzgados,
           CriterioDeBusqueda = enumCriteriosDeFiltrado.comienza,
           LongitudMinimaParaBuscar = 1,
           Fila = 0,
           Columna = 0,
           Obligatorio = false
           )
        ]
        public string Juzgado { get; set; }

        [IUPropiedad(Etiqueta = "Id del juzgado", Visible = false)]
        public int? IdJuzgado { get; set; }

        //----------------------------------------------
        [IUPropiedad(
           Etiqueta = "NIG",
           Ayuda = "Nº de identificación judicial",
           Fila = 1,
           Columna = 0,
           LongitudMaxima = 25,
           Obligatorio = false
          )
        ]
        public string Nig { get; set; }


        //----------------------------------------------
        [IUPropiedad(
           Etiqueta = "Procedimiento",
           Ayuda = "Nº de auto o procedimiento",
           Fila = 1,
           Columna = 0,
           LongitudMaxima = 50,
           Obligatorio = false
          )
        ]
        public string Referencia { get; set; }

        //----------------------------------------------
        [IUPropiedad(
           Etiqueta = "Litigado",
           Tipo = typeof(decimal),
           Ayuda = "importe litigado",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           Obligatorio = false,
           Formato = enumFormato.Numero_6,
           Fila = 1,
           Columna = 3)
        ]
        public decimal? Litigado { get; set; }

        //----------------------------------------------
        [IUPropiedad(
          Etiqueta = "Sentenciado",
          Tipo = typeof(decimal),
          Ayuda = "importe de sentencia",
          TipoDeControl = enumTipoControl.Editor,
          Alineada = enumAliniacion.derecha,
          Obligatorio = false,
          Formato = enumFormato.Numero_6,
          Fila = 2,
          Columna = 0)
       ]
        public decimal? Sentenciado { get; set; }


        //----------------------------------------------
        [IUPropiedad(
          Etiqueta = "Costas",
          Tipo = typeof(decimal),
          Ayuda = "Costas judiciales",
          TipoDeControl = enumTipoControl.Editor,
          Alineada = enumAliniacion.derecha,
          Obligatorio = false,
          Formato = enumFormato.Numero_6,
          Fila = 2,
          Columna = 1)
       ]
        public decimal? Costas { get; set; }



        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Sentenciado el",
            Ordenar = true,
            OrdenarGridPor = nameof(PagoDto.PagadoEl),
            TipoDeControl = enumTipoControl.SelectorDeFecha,
            TamanoFijo = "15em",
            Fila = 2,
            Columna = 3)]
        public DateTime? SentenciadoEl { get; set; }

    }
}
