using ModeloDeDto.Ventas;
using ServicioDeDatos.Juridico;
using ServicioDeDatos.Seguridad;
using System;
using Utilidades;

namespace ModeloDeDto.Juridico
{
    [IUDto(MostrarExpresion = nameof(IUsaNombreDto.Nombre), OpcionDeEditar = false, OpcionDeBorrar = false)]

    public class CopiarPlfDeVentaDto : ISelectorDto
    {

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(Etiqueta = "", Oculto = true, Obligatorio = true, VisibleEnGrid = false, Fila = 0, Columna = 0, Posicion = 0, TipoDeControl = enumTipoControl.Editor)]
        public enumClaseDeContrato ClaseDeContrato { get; set; }
        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(Etiqueta = "contrato destino", Visible = false)]
        public string Elemento { get; set; }

        [IUPropiedad(
            Etiqueta = "Copiar A",
            Ayuda = "Contrato al que se copia",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Elemento),
            Fila = 0,
            Columna = 0,
            EditableAlCrear = false,
            EditableAlEditar = false,
            AutoSpan = true,
            Controlador = nameof(enumControladoresJuridicos.Contratos),
            VistaDondeNavegar = enumVistasJuridicos.CrudContratos,
            Obligatorio = false
            )]
        public int IdElemento { get; set; }

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(Etiqueta = "Id del contrato seleccionado",Visible = false)]
        public int IdContrato { get; set; }

        [IUPropiedad(
            Etiqueta = "Contrato",
            Ayuda = "Seleccionar contrato",
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(IdContrato),
            Controlador = nameof(enumControladoresJuridicos.Contratos),
            SeleccionarDe = typeof(ContratoDto),
            VistaDondeNavegar = enumVistasJuridicos.CrudContratos,
            RestrictorFijo = ltrParametrosDto.Negocio + ";" + nameof(enumNegocio.Contrato) + ";" + nameof(enumModoDeAccesoDeDatos.Consultor),
            RestringidoPorControl = nameof(ClaseDeContrato),
            OrdenarListaDinamicaPor = nameof(ContratoDtm.Referencia),
            Negocio = enumNegocio.Contrato,
            LongitudMinimaParaBuscar = 1,
            MostrarExpresion = nameof(ContratoDto.Expresion),
            Tipo = typeof(string),
            Fila = 1,
            Columna = 0,
            EditableAlCrear = true,
            EditableAlEditar = false,
            trasSeleccionar = "javascript:" + nameof(enumNameSpaceTs.Juridico) + "." + nameof(enumFunctionTs.Ctr_Tras_Seleccionar_CopiarContratoDeVenta) + "([" + nameof(enumParamTs.idLista) + "])",
            trasBlanquear = "javascript:" + nameof(enumNameSpaceTs.Juridico) + "." + nameof(enumFunctionTs.Ctr_Tras_Blanquear_CopiarContratoDeVenta) + "([" + nameof(enumParamTs.idLista) + "])",
            AutoSpan = true)]
        public string Contrato { get; set; }

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(Etiqueta = "Id de planificador del contrato", Visible = false)]
        public int? IdPlanificador { get; set; }

        [IUPropiedad(
            Etiqueta = "Planificador",
            Ayuda = "Especifique cual copiar (si no selecciona uno se copiaran todos)",
            TipoDeControl = enumTipoControl.ListaDeElemento,
            SeleccionarDe = typeof(PlanificadorDeVentaDto),
            Controlador = nameof(enumControladoresJuridicos.PlanificadorDeVentas),
            GuardarEn = nameof(IdPlanificador),
            Obligatorio = false,
            RestringidoPorControl = nameof(Contrato),
            PropiedadRestrictora = nameof(IdContrato),
            MostrarExpresion = nameof(PlanificadorDeVentaDtm.Expresion),
            EditableAlCrear = true,
            EditableAlEditar = false,
            Fila = 2,
            Columna = 0,
            CargarBajoDemanda = true,
            OnChange = "javascript:" + nameof(enumNameSpaceTs.Juridico) + "." + nameof(enumFunctionTs.Ctr_Tras_Seleccionar_CopiarPlfDeVenta) + "()"
            )
        ]
        public string Planificador { get; set; }

        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Fecha de comienzo",
            Ayuda = "Inicio de comienzo del planificador del contrato",
            TipoDeControl = enumTipoControl.SelectorDeFecha,
            Obligatorio = true,
            Fila = 2,
            Columna = 1,
            Posicion = 0
           )
        ]
        public DateTime Inicio { get; set; }

        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Fecha de fin",
            Ayuda = "fin de terminación del planificador del contrato",
            TipoDeControl = enumTipoControl.SelectorDeFecha,
            Obligatorio = true,
            Fila = 2,
            Columna = 1,
            Posicion = 1
           )
        ]
        public DateTime Hasta { get; set; }
    }
}
