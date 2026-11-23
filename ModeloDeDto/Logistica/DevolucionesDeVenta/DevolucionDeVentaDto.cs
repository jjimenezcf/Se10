using System;
using Utilidades;
using ModeloDeDto.Terceros;
using ServicioDeDatos;
using ServicioDeDatos.Seguridad;

namespace ModeloDeDto.Logistica
{

    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, MostrarExpresion = nameof(IUsaNombreDto.Nombre), EditarTrasCrear = true)]
    public class DevolucionDeVentaDto : ElementoDeUnProcesoDto, IDatosDeContactoDto
    {

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
         Etiqueta = "Id del cliente de la devolución",
         Visible = false
         )]
        public int IdCliente { get; set; }

        [IUPropiedad(
            Etiqueta = "Cliente",
            Ayuda = "Quién devuelve",
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(IdCliente),
            Controlador = nameof(enumControladoresTerceros.Clientes),
            VistaDondeNavegar = enumVistasTerceros.CrudClientes,
            SeleccionarDe = typeof(ClienteDto),
            RestrictorFijo = ltrParametrosDto.Negocio + ";" + nameof(enumNegocio.Cliente) + ";" + nameof(enumModoDeAccesoDeDatos.Consultor),
            Negocio = enumNegocio.Cliente,
            LongitudMinimaParaBuscar = 1,
            Tipo = typeof(string),
            Fila = 2,
            Columna = 0,
            Posicion = 0,
            Obligatorio = true,
            EditableAlCrear = true,
            EditableAlEditar = false,
            AutoSpan = true)]
        public string Cliente { get; set; }

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
              PorAnchoMnt = 15
            , Etiqueta = "Devuelta el"
            , Ayuda = "Fecha de la devolución y de las entradas en el almacén al aceptarla (si no se indica, la del día)"
            , Ordenar = true
            , OrdenarGridPor = nameof(DevolucionDeVentaDto.DevueltaEl)
            , TipoDeControl = enumTipoControl.SelectorDeFecha
            , VisibleEnGrid = true
            , EditableAlCrear = true
            , EditableAlEditar = true
            , Formato = enumFormato.Fecha
            , Fila = 2
            , Columna = 0
            , Posicion = 1
            , AnchoMaximo = "11em")]
        public DateTime? DevueltaEl { get; set; }

        //-------------------------------------------------------------------------------------------------------------------
        [IUPropiedad(TipoDeControl = enumTipoControl.Editor,
            Fila = 3, Columna = 0
            , Etiqueta = "Contacto"
            , VisibleAlCrear = false
            , VisibleAlEditar = true
            , VisibleEnGrid = false)]
        public string Contacto { get; set; }

        //-------------------------------------------------------------------------------------------------------------------
        [IUPropiedad(TipoDeControl = enumTipoControl.Editor,
            Fila = 3, Columna = 1
            , Etiqueta = "Teléfono"
            , VisibleAlCrear = false
            , VisibleAlEditar = true
            , VisibleEnGrid = false)]
        public string Telefono { get; set; }

        //-------------------------------------------------------------------------------------------------------------------
        [IUPropiedad(TipoDeControl = enumTipoControl.Editor,
            Fila = 3, Columna = 2
            , Etiqueta = "eMail"
            , VisibleAlCrear = false
            , VisibleAlEditar = true
            , VisibleEnGrid = false)]
        public string eMail { get; set; }

        //--------------------------------------------
        // última fila, a la derecha: suma de los importes de las líneas (ver GestorDeDevolucionesDeVenta.DespuesDeMapearElElemento)
        [IUPropiedad(
           Etiqueta = "Total (sin IVA)",
           Tipo = typeof(decimal),
           Ayuda = "importe total devuelto sin iva: suma de las líneas con su descuento",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           VisibleAlCrear = false,
           EditableAlEditar = false,
           Obligatorio = false,
           Formato = enumFormato.Moneda,
           AnchoMaximo = "300px",
           MantenerHuecoDeLaIzquierda = true,
           Fila = 12,
           Columna = 2,
           VisibleEnGrid = false
            )
        ]
        public decimal? TotalSinIva { get; set; }

    }
}
