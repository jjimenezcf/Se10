using Gestor.Errores;
using GestorDeElementos;
using GestoresDeNegocio.Entorno;
using ModeloDeDto;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using ServicioDeDatos;
using ServicioDeDatos.MaestrosTecnico;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Utilidades;

namespace GestoresDeNegocio.MaestrosTecnico
{
    public static class ExportacionesDeUnitarios
    {
        public static readonly string N_ParaProveedor = "Tarifa para proveedor";

        internal static readonly string ltrUnitariosParaProveedor = nameof(ltrUnitariosParaProveedor);

        /// <summary>
        /// Excel con los unitarios seleccionados para que un proveedor indique su referencia y su tarifa.
        /// </summary>
        public static void ParaProveedor(EntornoDeUnaAccion entorno)
        {
            var filtrosJson = entorno.Entrada.LeerValor(ltrFiltros.filtro, "");
            var filtros = filtrosJson.IsNullOrEmpty() ? new List<ClausulaDeFiltrado>() : JsonConvert.DeserializeObject<List<ClausulaDeFiltrado>>(filtrosJson);
            var unitarios = entorno.Contexto.SeleccionarTodos<UnitarioDtm>(clausulas: filtros, aplicarJoin: true).OrderBy(x => x.Referencia).ToList();

            if (unitarios.Count == 0)
                GestorDeErrores.Emitir("No hay unitarios en la selección para exportar");

            var objeto = new ObjetoParaExportar(ruta: GestorDeVariables.RutaDeExportaciones,
                fichero: entorno.Plantilla.Nombre,
                datos: new Dictionary<string, object> { { ltrUnitariosParaProveedor, unitarios } });

            var excel = new ExportarMtParaProveedor(entorno.Contexto, objeto);
            entorno.Salida.Add(nameof(ObjetoParaExportar.FicheroConRuta), excel.Exportar());
        }
    }

    /// <summary>
    /// Columnas: Mi Referencia | Su Referencia | Nombre | Descripción | Unidad | Tarifa
    /// Solo son editables 'Su Referencia' y 'Tarifa', el resto de celdas quedan bloqueadas.
    /// </summary>
    internal class ExportarMtParaProveedor : IExportadorExcel
    {
        private const int FilaDeTitulo = 1;
        private const int FilaDeEncolumnado = 3;
        private const int PrimeraFilaDeDatos = 4;
        private const int ColumnaSuReferencia = 2;
        private const int ColumnaTarifa = 6;
        private const double AnchoMinimoDeLasEditables = 20;

        public const string Titulo = "Tarifa para proveedor";

        private List<UnitarioDtm> _unitarios { get; set; }
        private string _fichero { get; set; }
        private ContextoSe _contexto { get; set; }
        private ExcelPackage _libroExcel { get; set; }

        public ExportarMtParaProveedor(ContextoSe contexto, ObjetoParaExportar objeto)
        {
            _contexto = contexto;
            Inicializar(objeto);
        }

        public void Inicializar(ObjetoParaExportar objeto)
        {
            _fichero = objeto.FicheroConRuta;
            _unitarios = objeto.Datos.LeerValor<List<UnitarioDtm>>(ExportacionesDeUnitarios.ltrUnitariosParaProveedor);
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            _libroExcel = new ExcelPackage();
            _libroExcel.DefinirEstilos();
        }

        public string Exportar()
        {
            CrearHojaDeExcel();

            _libroExcel.Workbook.Calculate();
            File.WriteAllBytes(_fichero, _libroExcel.GetAsByteArray());

            return _fichero;
        }

        private void CrearHojaDeExcel()
        {
            var hoja = _libroExcel.Workbook.Worksheets.Add(Titulo);
            var siglasDeUnidades = _contexto.Set<UnidadDtm>().ToDictionary(u => u.Id, u => u.Sigla);

            var filas = _unitarios.Select(unitario => new List<ValorDeCelda> {
                new ValorDeCelda { Valor = unitario.Referencia, Bloqueada = true },
                new ValorDeCelda { Valor = null, Bloqueada = false },
                new ValorDeCelda { Valor = unitario.Nombre, Bloqueada = true },
                new ValorDeCelda { Valor = unitario.Descripcion, Bloqueada = true },
                new ValorDeCelda { Valor = siglasDeUnidades.GetValueOrDefault(unitario.IdUnidad), Bloqueada = true },
                new ValorDeCelda { Valor = null, Bloqueada = false }
            }).ToList();

            var ultimaFila = PrimeraFilaDeDatos + filas.Count - 1;

            hoja
            .Informe($"A{FilaDeTitulo}:F{FilaDeTitulo}", $"{Titulo}: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}")
            .Encolumnado("A", FilaDeEncolumnado, $"Mi Referencia{Simbolos.separadorDeValores}Su Referencia{Simbolos.separadorDeValores}Nombre{Simbolos.separadorDeValores}Descripción{Simbolos.separadorDeValores}Unidad{Simbolos.separadorDeValores}Tarifa")
            .Tabla("A", PrimeraFilaDeDatos, filas)
            .Cells.AutoFitColumns();

            MarcarLasColumnasEditables(hoja, ultimaFila);

            hoja.Cells[$"F{PrimeraFilaDeDatos}:F{ultimaFila}"].Style.Numberformat.Format = "#,##0.00";
            hoja.Proteger();
        }

        private static void MarcarLasColumnasEditables(ExcelWorksheet hoja, int ultimaFila)
        {
            foreach (var columna in new[] { ColumnaSuReferencia, ColumnaTarifa })
            {
                var celdas = hoja.Cells[PrimeraFilaDeDatos, columna, ultimaFila, columna];
                celdas.Style.Fill.PatternType = ExcelFillStyle.Solid;
                celdas.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightYellow);
                hoja.Column(columna).Width = Math.Max(hoja.Column(columna).Width, AnchoMinimoDeLasEditables);
            }
        }
    }
}
