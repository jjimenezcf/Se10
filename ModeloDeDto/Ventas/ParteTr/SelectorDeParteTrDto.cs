using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Ventas;
using Utilidades;

namespace ModeloDeDto.Ventas
{
    [IUDto(MostrarExpresion = nameof(IUsaNombreDto.Nombre))]
    public class SelectorDeParteTrDto : ISelectorDto
    {

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
         Etiqueta = "Id del parte seleccionado",
         Visible = false
         )]
        public int IdElemento { get; set; }

        [IUPropiedad(
            Etiqueta = "Parte de trabajo",
            Ayuda = "parte de trabajo a facturar",
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(IdElemento),
            Controlador = nameof(enumControladoresVentas.PartesTr),
            SeleccionarDe = typeof(ParteTrDto),
            VistaDondeNavegar = enumVistasVentas.CrudPartesDeTrabajo,
            RestrictorFijo = ltrParametrosDto.Negocio + ";" + nameof(enumNegocio.ParteDeTrabajo) + ";" + nameof(enumModoDeAccesoDeDatos.Consultor) + "|" +
                             ltrDeUnParteTr.FiltroPorEtapa + ";" + nameof(enumEtapasDePartesTr.PTR_Etapa_Pdt_Facturar),
            OrdenarListaDinamicaPor = nameof(ParteTrDtm.Referencia),
            Negocio = enumNegocio.ParteDeTrabajo,
            LongitudMinimaParaBuscar = 1,
            MostrarExpresion = nameof(ParteTrDto.Expresion),
            Tipo = typeof(string),
            Fila = 2,
            Columna = 0,
            EditableAlCrear = true,
            EditableAlEditar = false,
            AutoSpan = true)]
        public string Elemento { get; set; }
    }

}
