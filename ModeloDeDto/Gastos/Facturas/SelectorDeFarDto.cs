using ServicioDeDatos.Gastos;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Ventas;
using Utilidades;

namespace ModeloDeDto.Gastos
{
    [IUDto(MostrarExpresion = nameof(IUsaNombreDto.Nombre))]
    public class SelectorDeFarDto: ISelectorDto
    {

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
         Etiqueta = "Id de la factura seleccionada",
         Visible = false
         )]
        public int IdElemento { get; set; }

        [IUPropiedad(
            Etiqueta = "Factura",
            Ayuda = "Asociar factura",
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(IdElemento),
            Controlador = nameof(enumControladoresGastos.FacturasRec),
            SeleccionarDe = typeof(FacturaRecDto),
            VistaDondeNavegar = enumVistasGastos.CrudFacturasRec,
            RestrictorFijo = ltrParametrosDto.Negocio + ";" + nameof(enumNegocio.FacturaRecibida) + ";" + nameof(enumModoDeAccesoDeDatos.Gestor) + "|" +
                       ltrDeUnaFacturaRec.AsociadaAUnElemento + ";" + ltrFiltros.SinRelacion,
            OrdenarListaDinamicaPor = nameof(FacturaRecDtm.Referencia),
            Negocio = enumNegocio.FacturaRecibida,
            LongitudMinimaParaBuscar = 1,
            MostrarExpresion = nameof(FacturaRecDtm.Expresion),
            Tipo = typeof(string),
            Fila = 2,
            Columna = 0,
            EditableAlCrear = true,
            EditableAlEditar = false,
            SoloEnAlta = true,
            AutoSpan = true)]
        public string Elemento { get; set; }
    }

}
