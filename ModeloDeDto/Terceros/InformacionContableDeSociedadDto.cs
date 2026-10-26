using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Terceros;
using System;
using Utilidades;

namespace ModeloDeDto.Terceros
{
    // Ampliación de la sociedad con cómo se contabiliza: no tiene tabla, se lee y se graba sobre los parámetros de preasientos que se definen
    // por sociedad (ver ExtensorDeSociedades.InformacionContable y PersistirInformacionContable); es editable si el usuario puede parametrizar,
    // es gestor de la sociedad y la sociedad es gestionada
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, OpcionDeCrear = false, OpcionDeBorrar = false)]
    public class InformacionContableDeSociedadDto : EsUnaAmpliacionDto
    {
        //------------------------------------------------------------------------------------------- fila 0: dónde y cómo se contabiliza
        // SPR_Sociedad_CG_Documental
        [IUPropiedad(Etiqueta = "Id del CG documental", Visible = false)]
        public int? IdCgDocumental { get; set; }

        [IUPropiedad(
            Etiqueta = "CG documental",
            Ayuda = "Centro gestor de la sociedad donde se crean sus lotes contables",
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(IdCgDocumental),
            MostrarExpresion = nameof(CentroGestorDto.Expresion),
            Controlador = nameof(enumControladoresTerceros.CentrosGestores),
            VistaDondeNavegar = enumVistasTerceros.CrudCentrosGestores,
            SeleccionarDe = typeof(CentroGestorDto),
            Negocio = enumNegocio.CentroGestor,
            RestrictorFijo = nameof(NegociosDeUnCgDtm.Negocio) + ";" + nameof(enumNegocio.CentroGestor) + ";" + nameof(enumModoDeAccesoDeDatos.Gestor),
            LongitudMinimaParaBuscar = 1,
            antesDeBuscar = "javascript:" + nameof(enumNameSpaceTs.Terceros) + "." + nameof(enumFunctionTs.Sociedad_Antes_De_Buscar_CgDocumental),
            Tipo = typeof(string),
            Obligatorio = false,
            Fila = 0,
            Columna = 0)]
        public string CgDocumental { get; set; }

        [IUPropiedad(
            Etiqueta = "Último cierre",
            Ayuda = "Fecha hasta la que está cerrada contablemente la sociedad",
            TipoDeControl = enumTipoControl.SelectorDeFecha,
            EditableAlCrear = false,
            EditableAlEditar = false,
            Obligatorio = false,
            Fila = 0,
            Columna = 1)]
        public DateTime? UltimoCierre { get; set; }

        // SPR_Contabilizar_En
        [IUPropiedad(
            Etiqueta = "Contabilizar en",
            Ayuda = "Seleccionar...",
            TipoDeControl = enumTipoControl.Enumerado,
            Tipo = typeof(enumSistemaContable),
            GuardarEn = nameof(Contabilidad),
            Obligatorio = false,
            OnChange = "javascript:" + nameof(enumNameSpaceTs.Terceros) + "." + nameof(enumFunctionTs.Sociedad_Tras_Cambiar_Contabilidad) + "()",
            Fila = 0,
            Columna = 2)]
        public string Contabilidad { get; set; }

        // SPR_Contabilizar_En.Digitos: Ncs siempre 11, ContaSol entre 4 y 10 (8 si no se indica); comparte celda con el plan contable
        [IUPropiedad(
            Etiqueta = "Dígitos",
            Ayuda = "Dígitos de las cuentas auxiliares en la contabilidad",
            TipoDeControl = enumTipoControl.Editor,
            Tipo = typeof(int),
            Alineada = enumAliniacion.derecha,
            Obligatorio = false,
            LongitudMaxima = 2,
            OnBlur = "javascript:" + nameof(enumNameSpaceTs.Terceros) + "." + nameof(enumFunctionTs.Sociedad_Tras_Salir_De_Digitos) + "()",
            Fila = 0,
            Columna = 3,
            Posicion = 0,
            AutoSpan = true)]
        public int Digitos { get; set; }

        // SPR_Plan_Contable
        [IUPropiedad(
            Etiqueta = "Plan contable",
            Ayuda = "Identificador del plan contable de la sociedad en la contabilidad",
            TipoDeControl = enumTipoControl.Editor,
            Obligatorio = false,
            LongitudMaxima = 10,
            Fila = 0,
            Columna = 3,
            Posicion = 1)]
        public string PlanContable { get; set; }

        //------------------------------------------------------------------------------------------- fila 1: qué genera preasientos
        // SPR_Generar_Preasiento_De_FacturaEmitida, _FacturaRecibida, _Cobro, _Pago y _Almacen
        [IUPropiedad(Etiqueta = "Preasientos de emitidas", Ayuda = "Genera preasientos de las facturas emitidas", TipoDeControl = enumTipoControl.Check, css = enumCssControles.CheckApilado, Obligatorio = false, Fila = 1, Columna = 0)]
        public bool GenerarEmitidas { get; set; }

        [IUPropiedad(Etiqueta = "Preasientos de recibidas", Ayuda = "Genera preasientos de las facturas recibidas", TipoDeControl = enumTipoControl.Check, css = enumCssControles.CheckApilado, Obligatorio = false, Fila = 1, Columna = 1)]
        public bool GenerarRecibidas { get; set; }

        [IUPropiedad(Etiqueta = "Preasientos de cobros", Ayuda = "Genera preasientos de los cobros", TipoDeControl = enumTipoControl.Check, css = enumCssControles.CheckApilado, Obligatorio = false, Fila = 1, Columna = 2)]
        public bool GenerarCobros { get; set; }

        [IUPropiedad(Etiqueta = "Preasientos de pagos", Ayuda = "Genera preasientos de los pagos", TipoDeControl = enumTipoControl.Check, css = enumCssControles.CheckApilado, Obligatorio = false, Fila = 1, Columna = 3)]
        public bool GenerarPagos { get; set; }

        [IUPropiedad(Etiqueta = "Preasientos de almacén", Ayuda = "Genera preasientos de las entradas de almacén", TipoDeControl = enumTipoControl.Check, css = enumCssControles.CheckApilado, Obligatorio = false, Fila = 1, Columna = 4)]
        public bool GenerarAlmacen { get; set; }

        //------------------------------------------------------------------------------------------- fila 2: relanzar la activación de preasientos
        // la última activación de preasientos terminó con errores (su traza en la sociedad) o hay negocios activados con documentos sin
        // preasiento (lo completa InformacionContableDeSociedadController); no se pinta, la usa la acción
        [IUPropiedad(Visible = false)]
        public bool PuedeRelanzarActivacionDePreasientos { get; set; }

        // solo se muestra si se puede relanzar (ver Sociedades.ts); resomete el trabajo ActivarPreasientosDeLaSociedad
        [IUPropiedad(
            Etiqueta = "Relanzar la activación de preasientos",
            Ayuda = "hay documentos de negocios activados sin preasiento (o la última activación terminó con errores), una vez subsanado vuelva a generarlos",
            TipoDeControl = enumTipoControl.Referencia,
            css = enumCssControles.RefApilado,
            EnConsultaOcultar = true,
            AccionRef = "javascript:" + nameof(enumNameSpaceTs.Terceros) + "." + nameof(enumFunctionTs.Sociedad_Relanzar_Activacion_De_Preasientos) + "([" + nameof(PuedeRelanzarActivacionDePreasientos) + "])",
            Obligatorio = false,
            Fila = 2,
            Columna = 0)]
        public string RelanzarActivacionDePreasientos { get; }
    }
}
