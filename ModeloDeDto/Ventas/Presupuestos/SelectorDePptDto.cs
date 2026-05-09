using ServicioDeDatos.Presupuesto;
using ServicioDeDatos.Seguridad;
using Utilidades;

namespace ModeloDeDto.Presupuesto
{
    [IUDto(MostrarExpresion = nameof(IUsaNombreDto.Nombre))]
    public class SelectorDePptDto: ISelectorDto
    {

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
         Etiqueta = "Id del presupuesto seleccionado",
         Visible = false
         )]
        public int IdElemento { get; set; }

        [IUPropiedad(
            Etiqueta = "Presupuesto",
            Ayuda = "Presupuesto a asociar",
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(IdElemento),
            Controlador = nameof(enumControladoresVentas.Presupuestos),
            SeleccionarDe = typeof(PresupuestoDto),
            VistaDondeNavegar = enumVistasVentas.CrudPresupuestos,
            RestrictorFijo = ltrParametrosDto.Negocio + ";" + nameof(enumNegocio.Presupuesto) + ";" + nameof(enumModoDeAccesoDeDatos.Gestor) + "|" +
                             ltrDeUnPresupuesto.DependeDeExpediente + ";" + ltrFiltros.SinRelacion,
            OrdenarListaDinamicaPor = nameof(PresupuestoDtm.Referencia),
            Negocio = enumNegocio.Presupuesto,
            LongitudMinimaParaBuscar = 1,
            MostrarExpresion = nameof(PresupuestoDto.Expresion),
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
