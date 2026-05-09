using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Entorno;
using GestoresDeNegocio.TrabajosSometidos;
using ModeloDeDto;
using Newtonsoft.Json;
using OfficeOpenXml;
using ServicioDeDatos;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.Gastos;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.TrabajosSometidos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Utilidades;

namespace GestoresDeNegocio.Gastos
{

    public static class ExportacionesDeFacturasRec
    {
        public static readonly string N_EnviarAContabilizar = "Enviar a contabilizar";

        internal static readonly string ltrEnviarAContabilizar = nameof(ltrEnviarAContabilizar);

        public static readonly string N_ReporteParaContabilidad = "Reporte para contabilizar";

        internal static readonly string ltrReporteParaContabilizar = nameof(ltrReporteParaContabilizar);

        public static void EnviarAContabilizar(EntornoDeUnaAccion entorno)
        {
            var facturasParaTransitar = new List<FacturaRecDtm>();

            List<ClausulaDeFiltrado> clausulas = ((string)entorno.Entrada[ltrFiltros.filtro]).JsonToLista<ClausulaDeFiltrado>();
            clausulas.Add(new ClausulaDeFiltrado { Clausula = ltrFiltros.IdsDeEstado, Criterio = enumCriteriosDeFiltrado.igual, Valor = VariableDeFacturasRec.Estados(enumEtapasDeFacturasRec.FAR_Etapa_De_Aprobacion) });

            var facturas = entorno.Contexto.SeleccionarTodos<FacturaRecDtm>(clausulas).OrderBy(x => x.Referencia).ToList();

            var trabajo = entorno.Entrada.LeerValor<TrabajoDeUsuarioDtm>(nameof(TrabajoDeUsuarioDtm), null);
            var contextoDelTrabajo = entorno.Entrada.LeerValor<ContextoSe>(nameof(ContextoSe), null);

            foreach (var factura in facturas)
            {
                var mensaje = factura.DimeSiEstaBien(entorno.Contexto);
                if (mensaje.IsNullOrEmpty())
                    facturasParaTransitar.Add(factura);
                else
                {
                    factura.CrearTraza(entorno.Contexto, "No exportada para contabilizar", mensaje);
                    if (trabajo is not null && contextoDelTrabajo is not null)
                        GestorDeErroresDeUnTrabajo.CrearError(contextoDelTrabajo, trabajo, $"factura {factura.Referencia}", mensaje);
                    factura.CrearTraza(entorno.Contexto, "No se pudo enviar a contabilizar", "La exportación a excel no pudo enviar la factura a contabilizar por: " + mensaje);
                }
            }

            if (facturasParaTransitar.Count == 0)
                GestorDeErrores.Emitir("No hay facturas para enviar a contabilidad, consulte los motivos");

            var enviarAContabilizar = new List<FacturaRecDtm>();
            foreach (var factura in facturasParaTransitar)
            {
                try
                {
                    factura.TransitarALaEtapa(entorno.Contexto,
                        enumEtapasDeFacturasRec.FAR_Etapa_De_Contabilizacion.EstadosDeLaEtapa(),
                        new Dictionary<string, object> { { ltrParametrosNeg.EstaEjecutandoUnaAccion, true } },
                        delSistema: true);
                    enviarAContabilizar.Add(factura);
                }
                catch (Exception e)
                {
                    factura.CrearTraza(entorno.Contexto, "No exportada para contabilizar", e.Message);
                    if (trabajo is not null && contextoDelTrabajo is not null)
                        GestorDeErroresDeUnTrabajo.CrearError(contextoDelTrabajo, trabajo, $"factura {factura.Referencia}", e.Message);
                    factura.CrearTraza(entorno.Contexto, "No se pudo enviar a contabilizar", "La exportación a excel no pudo enviar la factura a contabilizar por: " + e.Message);
                }
            }

            if (enviarAContabilizar.Count == 0)
                GestorDeErrores.Emitir("No hay facturas para enviar a contabilidad, consulte los motivos");

            var objeto = new ObjetoParaExportar(ruta: GestorDeVariables.RutaDeExportaciones,
                fichero: entorno.Plantilla.Nombre,
                datos: new Dictionary<string, object> { { ltrEnviarAContabilizar, enviarAContabilizar } });

            var excel = new ExcelDeEnviarAContabilizar(entorno.Contexto, objeto, N_EnviarAContabilizar, ltrEnviarAContabilizar);
            entorno.Salida.Add(nameof(ObjetoParaExportar.FicheroConRuta), excel.Exportar());
            var archivador = entorno.Entrada.LeerValor<ArchivadorDtm>(nameof(ArchivadorDtm), null);
            foreach (var factura in enviarAContabilizar)
            {
                if (trabajo is not null && contextoDelTrabajo is not null)
                    GestorDeTrazasDeUnTrabajo.AnotarTraza(contextoDelTrabajo, trabajo, $"factura '{factura.Referencia}' procesada");
                if (archivador is not null) GestorDeVinculos.Vincular(entorno.Contexto, factura, archivador);
            }
        }

        public static void ReporteParaContabilidad(EntornoDeUnaAccion entorno)
        {
            var filtrosJson = entorno.Entrada.LeerValor(ltrFiltros.filtro, "");
            var filtros = filtrosJson.IsNullOrEmpty() ? new List<ClausulaDeFiltrado>() : JsonConvert.DeserializeObject<List<ClausulaDeFiltrado>>(filtrosJson);
            var facturas = entorno.Contexto.SeleccionarTodos<FacturaRecDtm>(clausulas: filtros, aplicarJoin: true).OrderByDescending(x => x.FacturadaEl).ToList();


            var objeto = new ObjetoParaExportar(ruta: GestorDeVariables.RutaDeExportaciones,
                fichero: entorno.Plantilla.Nombre,
                datos: new Dictionary<string, object> { { ltrReporteParaContabilizar, facturas } });

            var excel = new ExcelDeEnviarAContabilizar(entorno.Contexto, objeto, N_ReporteParaContabilidad, ltrReporteParaContabilizar);
            entorno.Salida.Add(nameof(ObjetoParaExportar.FicheroConRuta), excel.Exportar());

            var archivador = entorno.Entrada.LeerValor<ArchivadorDtm>(nameof(ArchivadorDtm), null);
            if (archivador is not null)
            {
                foreach (var factura in facturas)
                {
                    GestorDeVinculos.Vincular(entorno.Contexto, factura, archivador);
                }
            }
        }
    }


    internal class ExcelDeEnviarAContabilizar : IExportadorExcel
    {
        private List<FacturaRecDtm> _facturas { get; set; }
        private string _fichero { get; set; }
        private ContextoSe _contexto { get; set; }
        public string Titulo { get; }
        public string Parametro { get; }
        private ExcelPackage _libroExcel { get; set; }

        public ExcelDeEnviarAContabilizar(ContextoSe contexto, ObjetoParaExportar objeto, string titulo, string parametro)
        {
            _contexto = contexto;
            Titulo = titulo;
            Parametro = parametro;
            Inicializar(objeto);
        }

        public void Inicializar(ObjetoParaExportar objeto)
        {
            _fichero = objeto.FicheroConRuta;
            _facturas = objeto.Datos.LeerValor<List<FacturaRecDtm>>(Parametro);
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            _libroExcel = new ExcelPackage();
            _libroExcel.DefinirEstilos();
        }

        public string Exportar()
        {
            CrearHojaDeExcel(_facturas);

            _libroExcel.Workbook.Calculate();
            File.WriteAllBytes(_fichero, _libroExcel.GetAsByteArray());

            return _fichero;
        }

        private void CrearHojaDeExcel(List<FacturaRecDtm> facturas)
        {
            var hoja = _libroExcel.Workbook.Worksheets.Add(Titulo);

            var datosDeFacturas = facturas.Select(factura => new List<ValorDeCelda> {
                   new ValorDeCelda { Valor = factura.Referencia },
                   new ValorDeCelda { Valor = factura.Proveedor(_contexto).NIF(_contexto) },
                   new ValorDeCelda { Valor = factura.Proveedor(_contexto).Interlocutor(_contexto).Nombre(_contexto) },
                   new ValorDeCelda { Valor = factura.Numero },
                   new ValorDeCelda { Valor = factura.Nombre },
                   new ValorDeCelda { Valor = factura.FacturadaEl },
                   new ValorDeCelda { Valor = factura.BaseImponible, Formato = enumFormato.Numero_2 },
                   new ValorDeCelda { Valor = factura.Total(_contexto, ServicioDeDatos.Elemento.Enumerados.enumImporteFar.TotalIva), Formato = enumFormato.Numero_2 },
                   new ValorDeCelda { Valor = factura.Total(_contexto, ServicioDeDatos.Elemento.Enumerados.enumImporteFar.TotalIrpf), Formato = enumFormato.Numero_2 },
                   new ValorDeCelda { Valor = factura.Total(_contexto, ServicioDeDatos.Elemento.Enumerados.enumImporteFar.TotalPagar), Formato = enumFormato.Numero_2 },
                   new ValorDeCelda { Valor = factura.ImportesDePagosConfirmados(_contexto), Formato = enumFormato.Numero_2 },
                   new ValorDeCelda { Valor = factura.ImpuestosToString(_contexto)},
                   new ValorDeCelda { Valor = factura.FormasDePagoToString(_contexto)},
                   new ValorDeCelda { Valor = factura.ClaseRectificativa?.Descripcion() ?? ""}
            }).ToList();

            hoja
            .Informe("A1:N1", $"{Titulo}: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}")
            .Encolumnado("A", 3, "Referencia|NIF|Proveedor|Número|Asunto|Fecha factura|BI|IVA|IRPF|Total a pagar|Pagado|Impuestos|Formas de pago|Rectificativa")
            .Tabla("A", 4, datosDeFacturas, totalizarColumnas: new List<int> { 6, 7, 8, 9, 10 })
            .Cells.AutoFitColumns();
        }
    }

}
