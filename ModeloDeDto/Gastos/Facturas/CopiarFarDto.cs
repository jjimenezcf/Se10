using ModeloDeDto.Expediente;
using ModeloDeDto.Juridico;
using ModeloDeDto.Negocio;
using ModeloDeDto.Terceros;
using ServicioDeDatos.Juridico;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Terceros;
using System;
using Utilidades;

namespace ModeloDeDto.Gastos
{
    [IUDto(MostrarExpresion = nameof(IUsaNombreDto.Nombre))]
    public class CopiarFarDto : ISelectorDto
    {
        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad( Etiqueta = "Id de la factura seleccionada",Visible = false )]
        public int IdElemento { get; set; }

        [IUPropiedad(
            Etiqueta = "Factura",
            Ayuda = "Factura a copiar",
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(IdElemento),
            Controlador = nameof(enumControladoresGastos.FacturasRec),
            SeleccionarDe = typeof(FacturaRecDto),
            VistaDondeNavegar = enumVistasGastos.CrudFacturasRec,
            RestrictorFijo = ltrParametrosDto.Negocio + ";" + nameof(enumNegocio.FacturaRecibida) + ";" + nameof(enumModoDeAccesoDeDatos.Consultor),
            Negocio = enumNegocio.FacturaRecibida,
            LongitudMinimaParaBuscar = 1,
            MostrarExpresion = nameof(FacturaRecDto.Expresion),
            Tipo = typeof(string),
            Fila = 0,
            Columna = 0,
            EditableAlCrear = true,
            EditableAlEditar = false,
            trasSeleccionar = "javascript:" + nameof(enumNameSpaceTs.Gasto) + "." + nameof(enumFunctionTs.Far_ProponerDatosDelaFarSeleccionada) + "()",
            trasBlanquear = "javascript:" + nameof(enumNameSpaceTs.Gasto) + "." + nameof(enumFunctionTs.Far_InicializarModalDeCopiado) + "()",
            AutoSpan = true)]
        public string Elemento { get; set; }

        //--------------------------------------------------------------------------------------------------------
        [IUPropiedad( Oculto = true, Fila = 1,Columna = 0)]
        public int? IdSociedadDelCg { get; set; }

        [IUPropiedad( Etiqueta = "Id del Cg", Visible = false )]
        public int IdCg { get; set; }

        [IUPropiedad(
            Etiqueta = "CG propuesto",
            Ayuda = "Centro gestor",
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(IdCg),
            MostrarExpresion = nameof(CentroGestorDto.Expresion),
            Controlador = nameof(enumControladoresTerceros.CentrosGestores),
            VistaDondeNavegar = enumVistasTerceros.CrudCentrosGestores,
            SeleccionarDe = typeof(CentroGestorDto),
            Negocio = enumNegocio.CentroGestor,
            RestrictorFijo = nameof(NegociosDeUnCgDtm.Negocio) + ";" + nameof(enumNegocio.CentroGestor) + ";" + nameof(enumModoDeAccesoDeDatos.Gestor),
            RestringidoPorControl = nameof(IdSociedadDelCg),
            PropiedadRestrictora = nameof(IdSociedadDelCg),
            SoloEnAlta = true,
            LongitudMinimaParaBuscar = 1,
            Tipo = typeof(string),
            Fila = 1,
            Columna = 0,
            Ordenar = true,
            EditableAlCrear = true,
            EditableAlEditar = false,
            Posicion = 1)]
        public string Cg { get; set; }

        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(Etiqueta = "Id del tipo de factura", Visible = false )]
        public int IdTipo { get; set; }

        [IUPropiedad(
            Etiqueta = "Tipo factura",
            Ayuda = "Tipo de factura",
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(IdTipo),
            SeleccionarDe = typeof(TipoDeFacturaRecDto),
            RestrictorFijo = ltrParametrosDto.Negocio + ";" + nameof(enumNegocio.FacturaRecibida) + ";" + nameof(enumModoDeAccesoDeDatos.Gestor),
            Controlador = nameof(enumControladoresNegocio.TiposDeElemento),
            VistaDondeNavegar = enumVistasGastos.TiposDeFacturaRec,
            MostrarExpresion = nameof(TipoDeElementoDto.Nombre),
            SoloEnAlta = true,
            LongitudMinimaParaBuscar = 1,
            Negocio = enumNegocio.FacturaRecibida,
            Tipo = typeof(string),
            Fila = 1,
            Columna = 1,
            Ordenar = true,
            EditableAlCrear = true,
            EditableAlEditar = false,
            AutoSpan = true)]
        public string Tipo { get; set; }

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad( Etiqueta = "Id del solicitante del Factura", Visible = false)]
        public int IdProveedor { get; set; }

        [IUPropiedad(
            Etiqueta = "Proveedor",
            Ayuda = "indica proveedor de la factura",
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(IdProveedor),
            Controlador = nameof(enumControladoresTerceros.Proveedores),
            SeleccionarDe = typeof(ProveedorDto),
            VistaDondeNavegar = enumVistasTerceros.CrudProveedores,
            RestrictorFijo = ltrParametrosDto.Negocio + ";" + nameof(enumNegocio.Proveedor) + ";" + nameof(enumModoDeAccesoDeDatos.Consultor),
            Negocio = enumNegocio.Proveedor,
            LongitudMinimaParaBuscar = 1,
            Tipo = typeof(string),
            Fila = 2,
            Columna = 0,
            EditableAlCrear = true,
            EditableAlEditar = false,
            AutoSpan = true)]
        public string Proveedor { get; set; }


        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Asunto",
            Ayuda = "asunto de la factura",
            Tipo = typeof(string),
            Fila = 2,
            Columna = 1,
            Ordenar = true,
            PorAnchoMnt = 50,
            Obligatorio = true,
            LongitudMaxima = 250,
            AutoSpan = true
          )
        ]
        public string Nombre { get; set; }

        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(
         Etiqueta = "Factura proveedor",
         Tipo = typeof(string),
         Ayuda = "nº de factura de proveedor",
         EtiquetaGrid = "NºFac",
         TipoDeControl = enumTipoControl.Editor,
         Obligatorio = true,
         Fila =3,
         Columna = 0,
         Posicion = 0,
         LongitudMaxima = 25
          )
        ]
        public string Numero { get; set; }

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
             Etiqueta = "Facturada el"
            , TipoDeControl = enumTipoControl.SelectorDeFecha
            , Formato = enumFormato.Fecha
            , Fila = 3
            , Columna = 0
            , Posicion = 1)]
        public DateTime FacturadaEl { get; set; }


        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Base Imponible",
           Tipo = typeof(decimal),
           EtiquetaGrid = "B.I",
           Ayuda = "importe sin iva ni irpf de la factura",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = true,
           EditableAlEditar = true,
           Fila = 3,
           Columna = 1,
           Posicion = 0,
           AutoSpan = true,
           MantenerHuecoDeLaIzquierda = true,
           Formato = enumFormato.Numero_6
            )
        ]
        public decimal BaseImponible { get; set; }

        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Total a pagar",
           EtiquetaGrid = "A pagar",
           Tipo = typeof(decimal),
           Ayuda = "importe de la factura con iva y sin Irpf",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = true,
           EditableAlEditar = true,
           Fila = 3,
           Columna = 1,
           Posicion =1,
           Formato = enumFormato.Numero_6
            )
        ]
        public decimal TotalDelPago { get; set; }


        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Descripción",
           Ayuda = "descripción de la factura",
           TipoDeControl = enumTipoControl.AreaDeTexto,
           VisibleEnGrid = false,
           Obligatorio = false,
           NumeroDeFilas = 4,
           Fila = 4,
           Columna = 0,
           AutoSpan = true
          )
        ]
        public string Descripcion { get; set; }

        //------------------------------------------------------------------------------------------------------
        [IUPropiedad( Etiqueta = "Id del contrato", Visible = false)]
        public int? IdContrato { get; set; }

        [IUPropiedad(
            Etiqueta = "Contrato de compra",
            EtiquetaGrid = "Contrato",
            Ayuda = "Contrato asociable",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(ContratoDto),
            GuardarEn = nameof(IdContrato),
            MostrarExpresion = nameof(ContratoDtm.Expresion),
            Controlador = nameof(enumControladoresJuridicos.Contratos),
            VistaDondeNavegar = enumVistasJuridicos.CrudContratos,
            ParametrosParaNavegar = nameof(ltrParametrosEp.Clase) + Simbolos.Igual + nameof(enumClaseDeContrato.Compra),
            RestrictorFijo = ltrDeUnContrato.SelectorParaUnaFacturaRec + Simbolos.PuntoComa + nameof(enumEtapasDeContratos.CTR_Etapa_Vigente) + Simbolos.PuntoComa + nameof(enumEtapasDeContratos.CTR_Etapa_Finalizacion),
            RestringidoPorControl = nameof(IdSociedadDelCg),
            PropiedadRestrictora = nameof(IdSociedadDelCg),
            Fila = 5,
            Columna = 0,
            Posicion = 0,
            VisibleEnGrid = false,
            LongitudMinimaParaBuscar = 1,
            AutoSpan = false
            )
        ]
        public string Contrato { get; set; }


        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
         Etiqueta = "Id del expediente seleccionado", Visible = false )]
        public int idExpediente { get; set; }


        [IUPropiedad(
            Etiqueta = "Expediente propuesto",
            Ayuda = "expediente de la nueva factura",
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(idExpediente),
            Controlador = nameof(enumControladoresAdministrativos.Expedientes),
            SeleccionarDe = typeof(ExpedienteDto),
            VistaDondeNavegar = enumVistasAdministrativo.CrudExpedientes,
            RestrictorFijo = ltrParametrosDto.Negocio + ";" + nameof(enumNegocio.Expediente) + ";" + nameof(enumModoDeAccesoDeDatos.Consultor),
            RestringidoPorControl = nameof(IdSociedadDelCg),
            PropiedadRestrictora = nameof(IdSociedadDelCg),
            Negocio = enumNegocio.Expediente,
            LongitudMinimaParaBuscar = 1,
            MostrarExpresion = nameof(FacturaRecDto.Expresion),
            Tipo = typeof(string),
            Fila = 5,
            Columna = 1,
            Obligatorio = false,
            AutoSpan = false)]
        public string Expediente { get; set; }

        //----------------------------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
            VisibleEnGrid = false,
            EtiquetaGrid = "",
            Etiqueta = "Archivo factura",
            Ayuda = "Seleccione el fichero de la factura recibida",
            Tipo = typeof(int),
            TipoDeControl = enumTipoControl.SelectorDeUnArchivo,
            ExtensionesValidas = ExtensorDeTipoDeArchivos.NoEditables,
            Fila = 6,
            Columna = 0,
            AutoSpan = true)]
        public int? IdArchivoAlCopiar { get; set; }

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Pagada el",
            Ayuda = "Si indica fecha de pago, se aplicará el modo y fecha de pago de la factura original dándose por pagada si la fecha es anterior o igual a hoy",
            TipoDeControl = enumTipoControl.SelectorDeFecha,
            Formato = enumFormato.Fecha,
            Fila = 6,
            Columna = 1)]
        public DateTime? PagadaEl { get; set; }

    }
}
