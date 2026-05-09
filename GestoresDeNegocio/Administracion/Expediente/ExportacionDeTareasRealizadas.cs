using DocumentFormat.OpenXml.Wordprocessing;
using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Entorno;
using Microsoft.CodeAnalysis;
using ModeloDeDto;
using ModeloDeDto.Tarea;
using Newtonsoft.Json;
using OfficeOpenXml;
using ServicioDeDatos;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Tarea;
using ServicioDeDatos.Terceros;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Utilidades;

namespace GestoresDeNegocio.Administracion
{
    public class TareasConSolicitante
    {
        internal TareaDtm Tarea { get; set; }
        internal InterlocutorDtm Solicitante { get; set; }
    }

    class TareasRealizadas
    {
        internal ExpedienteDtm Expediente { get; set; }
        internal List<TareasConSolicitante> TareasConSol { get; set; }
    }

    public class ExportacionDeTareasRealizadas
    {
        public static readonly string N_TareasRealizadas = "Exportar las tareas realizadas de los expedientes seleccionados";

        internal static readonly string ltrExpedientesConSusTareas = nameof(ltrExpedientesConSusTareas);

        public static void ExpedientesConSusTareas(EntornoDeUnaAccion entorno)
        {
            var datos = new List<TareasRealizadas>();


            var filtrosJson = entorno.Entrada.LeerValor(ltrFiltros.filtro, "");
            var filtros = filtrosJson.IsNullOrEmpty() ? new List<ClausulaDeFiltrado>() : JsonConvert.DeserializeObject<List<ClausulaDeFiltrado>>(filtrosJson);

            var expedientes = entorno.Contexto.SeleccionarTodos<ExpedienteDtm>(clausulas: filtros, aplicarJoin: true, parametros: new Dictionary<string, object> { { ltrParametrosNeg.Peticion, enumPeticion.epExportar } }).OrderBy(x => x.IdTipo).ToList();

            foreach (var expediente in expedientes)
            {
                var tareas = expediente.Tareas(entorno.Contexto, excluirTerminadas: false).ToList();
                if (!tareas.Any())
                    continue;

                var tareasOrdenadas = tareas.OrderBy(t =>
                {
                    var split = t.Referencia.Split('-');
                    var año = int.Parse(split[0]);
                    var tipo = split[1] + split[2];
                    var numero = int.Parse(split[3]);
                    return (año, tipo, numero);
                }).ToList();

                var tareasConSolicitante = tareasOrdenadas.Select(tarea => new TareasConSolicitante { Tarea = tarea, Solicitante = tarea.Solicitante(entorno.Contexto) }).ToList();
                datos.Add(new TareasRealizadas { Expediente = expediente, TareasConSol = tareasConSolicitante });
            }

            if (datos.Count == 0)
                GestorDeErrores.Emitir("En la selección de expedientes no los hay con tareas relacionadas que estén todas terminadas");

            var objeto = new ObjetoParaExportar(ruta: GestorDeVariables.RutaDeExportaciones,
                fichero: entorno.Plantilla.Nombre,
                datos: new Dictionary<string, object> { { ExportacionDeTareasRealizadas.ltrExpedientesConSusTareas, datos } });

            var excel = new ExcelDeTareasRealizadasDeExpedientes(entorno.Contexto, objeto);

            var archivador = entorno.Entrada.LeerValor<ArchivadorDtm>(nameof(ArchivadorDtm), null);
            if (archivador is not null)
            {
                foreach (var expediente in datos)
                {
                    GestorDeVinculos.Vincular(entorno.Contexto, expediente.Expediente, archivador);
                    var tareas = expediente.TareasConSol;
                    foreach (var tarea in tareas) GestorDeVinculos.Vincular(entorno.Contexto, tarea.Tarea, archivador);
                }
            }

            entorno.Salida.Add(nameof(ObjetoParaExportar.FicheroConRuta), excel.Exportar());
        }

        internal class ExcelDeTareasRealizadasDeExpedientes : IExportadorExcel
        {
            private List<TareasRealizadas> _expedientes { get; set; }
            private string _fichero { get; set; }
            private ContextoSe _contexto { get; set; }
            private ExcelPackage _libroExcel { get; set; }

            public ExcelDeTareasRealizadasDeExpedientes(ContextoSe contexto, ObjetoParaExportar objeto)
            {
                _contexto = contexto;
                Inicializar(objeto);
            }

            public void Inicializar(ObjetoParaExportar objeto)
            {
                _fichero = objeto.FicheroConRuta;
                _expedientes = objeto.Datos.LeerValor<List<TareasRealizadas>>(ExportacionDeTareasRealizadas.ltrExpedientesConSusTareas);
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                _libroExcel = new ExcelPackage();
                _libroExcel.DefinirEstilos();
            }

            public string Exportar()
            {

                foreach (var expediente in _expedientes)
                    CrearHojaDeExcel(expediente);

                _libroExcel.Workbook.Calculate();
                File.WriteAllBytes(_fichero, _libroExcel.GetAsByteArray());

                return _fichero;
            }

            private void CrearHojaDeExcel(TareasRealizadas expediente)
            {
                HojaDeDetalles(expediente);
                HojaDeTiempos(expediente);
            }
            private void HojaDeDetalles(TareasRealizadas expediente)
            {
                var hoja = _libroExcel.Workbook.Worksheets.Add("D:" + expediente.Expediente.Referencia);
                var datos = expediente.Expediente.Tiempos(_contexto);

                var valoresDeLosTitulos = new List<ValorDeCelda> {
                    new ValorDeCelda { Valor = expediente.Expediente.Expresion },
                    new ValorDeCelda { Valor = datos.Planificacion },
                    new ValorDeCelda { Valor = datos.Horas }
                };

                var dias = Convert.ToInt32((datos.FecFinPla.Fecha() - datos.FecIniPla.Fecha()).TotalDays);

                var tareas = expediente.TareasConSol.Select(tarea =>
                {
                    var fila = new List<ValorDeCelda>
                    {
                        new ValorDeCelda { Valor = tarea.Tarea.DefinirLink(_contexto),  Formato = enumFormato.LinkHtml },
                        new ValorDeCelda { Valor = tarea.Tarea.Expresion },
                        new ValorDeCelda { Valor = tarea.Solicitante.Expresion }
                    };
                    return fila;
                }).ToList();

                var filaTitulo = 4;
                var filaTabla = 4 + valoresDeLosTitulos.Count + 2;

                hoja
                .Informe("A1:C1", "Tareas realizadas")
                .Titulos("A", filaTitulo, $"Nombre{Simbolos.separadorDeValores}Periodo{Simbolos.separadorDeValores}Horas")
                .Valores("B", filaTitulo, valoresDeLosTitulos, ltrExcelEstilos.ValoresDeTitulos)
                .Encolumnado("A", filaTabla, $"Referencia{Simbolos.separadorDeValores}Asunto{Simbolos.separadorDeValores}Solicitante")
                .Tabla("A", filaTabla + 1, tareas)
                .Cells.AutoFitColumns();
            }


            private void HojaDeTiempos(TareasRealizadas expediente)
            {
                var hoja = _libroExcel.Workbook.Worksheets.Add("T:" + expediente.Expediente.Referencia);
                var datos = expediente.Expediente.Tiempos(_contexto);

                var valoresDeLosTitulos = new List<ValorDeCelda> {
                    new ValorDeCelda { Valor = expediente.Expediente.Expresion },
                    new ValorDeCelda { Valor = datos.Planificacion },
                    new ValorDeCelda { Valor = datos.Horas }
                };

                var dias = Convert.ToInt32((datos.FecFinPla.Fecha() - datos.FecIniPla.Fecha()).TotalDays);
                var celdasDeDias = $"{datos.FecIniPla.Fecha().ToString("dd-MM-yyyy")}";
                var diasNoLaborables = 0;
                for (int i = 1; i <= dias; i++)
                {
                    var fecha = datos.FecIniPla.Fecha().AddDays(i);
                    if (fecha.DayOfWeek == DayOfWeek.Saturday || fecha.DayOfWeek == DayOfWeek.Sunday)
                    {
                        diasNoLaborables++;
                        continue;
                    }
                    celdasDeDias = $"{celdasDeDias}{Simbolos.separadorDeValores}{fecha.ToString("dd-MM-yyyy")}";
                }

                var detalleDeTareas = expediente.TareasConSol.Select(tarea =>
                {
                    var fila = new List<ValorDeCelda> { new ValorDeCelda { Valor = tarea.Tarea.DefinirLink(_contexto), Formato = enumFormato.LinkHtml } };
                    if (dias > 0) fila.AddRange(Enumerable.Range(0, dias + 1 - diasNoLaborables).Select(_ => new ValorDeCelda { Valor = 0 }).ToList());
                    fila.Add(new ValorDeCelda { Valor = !tarea.Tarea.HayPlanificacion(_contexto) ? null : tarea.Tarea.Planificacion(_contexto).Horas });
                    return fila;
                }).ToList();
                decimal totalHorasDedicadas = expediente.TareasConSol.Sum(tarea => (decimal)(tarea.Tarea.Planificacion(_contexto)?.Horas ?? 0));

                detalleDeTareas = RepartirTiempoTareas(detalleDeTareas, expediente.TareasConSol, totalHorasDedicadas);

                var filaTitulo = 4;
                var filaMatriz = 4 + valoresDeLosTitulos.Count + 2;

                hoja
                .Informe("A1:C1", "Tareas realizadas")
                .Titulos("A", filaTitulo, $"Nombre{Simbolos.separadorDeValores}Periodo{Simbolos.separadorDeValores}Horas")
                .Valores("B", filaTitulo, valoresDeLosTitulos, ltrExcelEstilos.ValoresDeTitulos)
                .Encolumnado("A", filaMatriz, $"Referencia{Simbolos.separadorDeValores}{(dias < 1 ? "" : celdasDeDias + Simbolos.separadorDeValores)}Horas")
                .Tabla("A", filaMatriz + 1, detalleDeTareas, totalizarColumnas: new List<int> { 2 + dias - diasNoLaborables })
                .Cells.AutoFitColumns();
            }
            public static List<List<ValorDeCelda>> RepartirTiempoTareas(List<List<ValorDeCelda>> detalleDeTareas, List<TareasConSolicitante> tareasConSol, decimal totalHorasDedicadas)
            {
                int numDias = detalleDeTareas[0].Count - 2;

                if (numDias <= 0)
                    GestorDeErrores.Emitir("No puedo repartir el trabajo, ya que no se ha definido un rango valido de días de trabajo");

                const decimal MAX_HORAS_POR_TAREA_POR_DIA = 8;
                decimal numTrabajadoresBase = 2; // Empezamos con 2 trabajadores
                decimal MAX_HORAS_POR_DIA = numTrabajadoresBase * MAX_HORAS_POR_TAREA_POR_DIA; // Inicializamos con 16 horas

                // Calculamos la capacidad total de trabajo con la configuración actual
                decimal capacidadTotalTrabajo = numDias * MAX_HORAS_POR_DIA;

                // Ajustamos MAX_HORAS_POR_DIA si la capacidad no es suficiente
                while (totalHorasDedicadas > capacidadTotalTrabajo)
                {
                    numTrabajadoresBase = numTrabajadoresBase + 0.5m; // Incrementamos el número de trabajadores (media jornada)
                    MAX_HORAS_POR_DIA = numTrabajadoresBase * MAX_HORAS_POR_TAREA_POR_DIA; // Actualizamos MAX_HORAS_POR_DIA
                    capacidadTotalTrabajo = numDias * MAX_HORAS_POR_DIA; // Recalculamos la capacidad total
                }

                decimal[] horasPorDia = new decimal[numDias];

                for (int i = 0; i < detalleDeTareas.Count; i++)
                {
                    var tarea = detalleDeTareas[i];
                    decimal horasTotales = Convert.ToInt32(tarea[tarea.Count - 1].Valor);
                    decimal horasRestantes = horasTotales;
                    int diaActual = 0;

                    while (horasRestantes > 0 && diaActual < numDias)
                    {
                        decimal horasDisponiblesEnDia = Math.Min(MAX_HORAS_POR_DIA - horasPorDia[diaActual], MAX_HORAS_POR_TAREA_POR_DIA);
                        decimal horasAsignadas = Math.Min(horasRestantes, horasDisponiblesEnDia);

                        tarea[diaActual + 1].Valor = horasAsignadas;
                        horasPorDia[diaActual] += horasAsignadas;
                        horasRestantes -= horasAsignadas;
                        diaActual++;
                    }

                    if (horasRestantes > 0)
                    {
                        tarea[numDias].Valor = horasRestantes; // Asignar horas restantes a la última columna
                    }
                    else
                    {
                        tarea[numDias].Valor = 0;
                    }
                }

                return detalleDeTareas;
            }


        }

    }
}

