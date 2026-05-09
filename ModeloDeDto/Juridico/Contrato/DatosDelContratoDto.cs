using System;
using ModeloDeDto.Terceros;
using ServicioDeDatos.Seguridad;
using Utilidades;


namespace ModeloDeDto.Juridico
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, OpcionDeBorrar = false)]
    public class DatosDelContratoDto : EsUnaAmpliacionDto
    {

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
         Etiqueta = "Id del proveedor del contrato",
         Visible = false
         )]
        public int? IdProveedor { get; set; }

        [IUPropiedad(
            Etiqueta = "Proveedor",
            Ayuda = "Proveedor del contrato",
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(IdProveedor),
            Controlador = nameof(enumControladoresTerceros.Proveedores),
            SeleccionarDe = typeof(ProveedorDto),
            VistaDondeNavegar = enumVistasTerceros.CrudProveedores,
            RestrictorFijo = ltrParametrosDto.Negocio + ";" + nameof(enumNegocio.Proveedor) + ";" + nameof(enumModoDeAccesoDeDatos.Consultor),
            Negocio = enumNegocio.Proveedor,
            LongitudMinimaParaBuscar = 1,
            Tipo = typeof(string),
            Fila = 0,
            Columna = 0,
            EditableAlCrear = true,
            EditableAlEditar = true, 
            Obligatorio = false,
            trasSeleccionar = "javascript:" + nameof(enumNameSpaceTs.Juridico) + "." + nameof(enumFunctionTs.Ctr_Tras_Seleccionar_Proveedor) + "([" + nameof(enumParamTs.idLista) + "])",
            trasBlanquear = "javascript:" + nameof(enumNameSpaceTs.Juridico) + "." + nameof(enumFunctionTs.Ctr_Tras_Blanquear_Proveedor) + "([" + nameof(enumParamTs.idLista) + "])",
            AutoSpan = true)]
        public string Proveedor { get; set; }


        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
         Etiqueta = "Id del cliente del contrato",
         Visible = false
         )]
        public int? IdCliente { get; set; }

        [IUPropiedad(
            Etiqueta = "Cliente",
            Ayuda = "Cliente del contrato",
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(IdCliente),
            Controlador = nameof(enumControladoresTerceros.Clientes),
            SeleccionarDe = typeof(ClienteDto),
            VistaDondeNavegar = enumVistasTerceros.CrudClientes,
            RestrictorFijo = ltrParametrosDto.Negocio + ";" + nameof(enumNegocio.Cliente) + ";" + nameof(enumModoDeAccesoDeDatos.Consultor),
            Negocio = enumNegocio.Cliente,
            LongitudMinimaParaBuscar = 1,
            Tipo = typeof(string),
            Fila = 0,
            Columna = 0,
            EditableAlCrear = true,
            EditableAlEditar = true,
            Obligatorio = false,
            trasSeleccionar = "javascript:" + nameof(enumNameSpaceTs.Juridico) + "." + nameof(enumFunctionTs.Ctr_Tras_Seleccionar_Cliente) + "([" + nameof(enumParamTs.idLista) + "])",
            trasBlanquear = "javascript:" + nameof(enumNameSpaceTs.Juridico) + "." + nameof(enumFunctionTs.Ctr_Tras_Blanquear_Cliente) + "([" + nameof(enumParamTs.idLista) + "])",
            AutoSpan = true)]
        public string Cliente { get; set; }

        //-------------------------------------------------------------------------------------------------------------------
        [IUPropiedad(TipoDeControl = enumTipoControl.Editor,
            Fila = 1, Columna = 0
            , Etiqueta = "Contacto"
            , Obligatorio = false
            , VisibleAlCrear = false
            , VisibleAlEditar = true
            , VisibleEnGrid = false)]
        public string Contacto { get; set; }

        //-------------------------------------------------------------------------------------------------------------------
        [IUPropiedad(TipoDeControl = enumTipoControl.Editor,
            Fila = 1, Columna = 1
            , Etiqueta = "Teléfono"
            , Obligatorio = false
            , VisibleAlCrear = false
            , VisibleAlEditar = true
            , VisibleEnGrid = false)]
        public string Telefono { get; set; }

        //-------------------------------------------------------------------------------------------------------------------
        [IUPropiedad(TipoDeControl = enumTipoControl.Editor,
            Fila = 1, Columna = 2
            , Etiqueta = "eMail"
            , Obligatorio = false
            , VisibleAlCrear = false
            , VisibleAlEditar = true
            , VisibleEnGrid = false)]
        public string eMail { get; set; }


        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Fecha de inicio",
            Ayuda = "Inicio del contrato",
            TipoDeControl = enumTipoControl.SelectorDeFecha,
            Fila = 2,
            Columna = 0,
            Obligatorio = true
           )
        ]
        public DateTime InicioContrato { get; set; }

        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Fecha de fin",
            Ayuda = "fin del contrato",
            TipoDeControl = enumTipoControl.SelectorDeFecha,
            Fila = 2,
            Columna = 1,
            Obligatorio = false
           )
        ]
        public DateTime? FinContrato { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Avisar antes de los x meses de su finalización",
           Tipo = typeof(int),
           Ayuda = "cuando se debe avisar que va a finalizar",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           Obligatorio = false,
           Fila = 2,
           Columna = 2)
        ]
        public int? AvisarAntesDe { get; set; }

        //----------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Aviso enviado",
            Ayuda = "indica si se ha enviado el aviso",
            Fila = 3,
            Columna = 2,
            TipoDeControl = enumTipoControl.Check,
            CssDelContenedor = enumCssControles.ContenedorCheckRight,
            css = enumCssControles.ControlApilado,
            Alineada = enumAliniacion.derecha,
            MantenerHuecoDeLaIzquierda = true,
            EditableAlEditar =false,
            ValorPorDefecto = false
            )
        ]
        public bool? RecordatorioEnviado { get; set; }
    }
}
