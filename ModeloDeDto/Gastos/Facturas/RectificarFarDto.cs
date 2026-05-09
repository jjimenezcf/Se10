using ModeloDeDto.Negocio;
using ModeloDeDto.Terceros;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Terceros;
using System;
using Utilidades;

namespace ModeloDeDto.Gastos
{
    [IUDto(MostrarExpresion = nameof(IUsaNombreDto.Nombre))]
    public class RectificarFarDto : ISelectorDto
    {
        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad( Etiqueta = "Id de la factura a rectificar",Visible = false )]
        public int IdElemento { get; set; }

        [IUPropiedad(
            Etiqueta = "Factura",
            Ayuda = "Factura a rectificar",
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
            EditableAlCrear = false,
            EditableAlEditar = false,
            AutoSpan = true)]
        public string Elemento { get; set; }


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
            EditableAlCrear = false,
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
         Etiqueta = "Factura rectificativa",
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
        public int? IdArchivoRectificativa { get; set; }


    }
}
