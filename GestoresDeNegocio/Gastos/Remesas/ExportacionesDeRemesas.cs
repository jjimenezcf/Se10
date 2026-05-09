using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Entorno;
using OfficeOpenXml;
using ServicioDeDatos;
using ServicioDeDatos.Gastos;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Utilidades;

namespace GestoresDeNegocio.Gastos
{
    class DetalleDeRemesa
    {
        internal RemesaPagDtm Remesa { get; set; }
        internal List<PagoDeUnaRemesaDtm> Pagos { get; set; }
    }

    public static class ExportacionesDeRemesas
    {
        public static readonly string N_RemesasConSusPagos = "Exportar un conjunto de remesas y sus pagos";

        internal static readonly string ltrRemesasConSusPagos = nameof(ltrRemesasConSusPagos);

        public static void RemesasConSusPagos(EntornoDeUnaAccion entorno)
        {
            var datos = new List<DetalleDeRemesa>();

            var remesas = entorno.Contexto.SeleccionarTodos<RemesaPagDtm>(new Dictionary<string, object>()).OrderBy(x => x.Referencia).ToList();

            foreach (var remesa in remesas)
            {
                datos.Add(new DetalleDeRemesa
                {
                    Remesa = remesa,
                    Pagos = remesa.Detalles<PagoDeUnaRemesaDtm>(entorno.Contexto, aplicarJoin: true)
                });
            }

            var objeto = new ObjetoParaExportar(ruta: GestorDeVariables.RutaDeExportaciones,
                fichero: entorno.Plantilla.Nombre,
                datos: new Dictionary<string, object> { { ExportacionesDeRemesas.ltrRemesasConSusPagos, datos } });

            var excel = new ExcelDeRemesasConSusPagos(entorno.Contexto, objeto);
            entorno.Salida.Add(nameof(ObjetoParaExportar.FicheroConRuta), excel.Exportar());
        }

    }


    internal class ExcelDeRemesasConSusPagos : IExportadorExcel
    {
        private List<DetalleDeRemesa> _remesas { get; set; }
        private string _fichero { get; set; }
        private ContextoSe _contexto { get; set; }
        private ExcelPackage _libroExcel { get; set; }

        public ExcelDeRemesasConSusPagos(ContextoSe contexto, ObjetoParaExportar objeto)
        {
            _contexto = contexto;
            Inicializar(objeto);
        }

        public void Inicializar(ObjetoParaExportar objeto)
        {
            _fichero = objeto.FicheroConRuta;
            _remesas = objeto.Datos.LeerValor<List<DetalleDeRemesa>>(ExportacionesDeRemesas.ltrRemesasConSusPagos);
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            _libroExcel = new ExcelPackage();
            _libroExcel.DefinirEstilos();
        }

        public string Exportar()
        {

            foreach (var remesa in _remesas)
                CrearHojaDeExcel(remesa);

            _libroExcel.Workbook.Calculate();
            File.WriteAllBytes(_fichero, _libroExcel.GetAsByteArray());

            return _fichero;
        }

        private void CrearHojaDeExcel(DetalleDeRemesa remesa)
        {
            var hoja = _libroExcel.Workbook.Worksheets.Add(remesa.Remesa.Referencia);
            var valoresDeLosTitulos = new List<ValorDeCelda> {
                    new ValorDeCelda { Valor = remesa.Remesa.Expresion },
                    new ValorDeCelda { Valor = remesa.Remesa.PagadaEl, Formato = enumFormato.Fecha },
                    new ValorDeCelda { Valor = remesa.Remesa.Total(_contexto), Formato = enumFormato.Moneda } };

            var detalleDePagos = remesa.Pagos.Select(pago => new List<ValorDeCelda> {
                   new ValorDeCelda { Valor = pago.Pago.Referencia },
                   new ValorDeCelda { Valor = pago.Pago.Importe, Formato = enumFormato.Numero_2 } 
            }).ToList();


            hoja
            .Informe("A1:C1", "Remesas y pagos")
            .Titulos("A", 4, "Nombre|Fecha de Pago|Importe")
            .Valores("B", 4, valoresDeLosTitulos, ltrExcelEstilos.ValoresDeTitulos)
            .Encolumnado("A", 4 + valoresDeLosTitulos.Count + 2, "Referencia|Importe")
            .Tabla("A", 4 + valoresDeLosTitulos.Count + 3, detalleDePagos, totalizarColumnas: new List<int> { 1 })
            .Cells.AutoFitColumns();
        }
    }
}
