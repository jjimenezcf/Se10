using GestorDeElementos.Extensores;
using Newtonsoft.Json;
using System.Reflection;
using Newtonsoft.Json.Linq;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.Utilidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilidades;
using ServicioDeDatos.Callejero;

namespace GestorDeElementos
{
    // Delegados públicos para que los gestores de negocio puedan registrar sus resolvers
    public delegate decimal? ResolverCalculadoDelegate(ElementoDeProcesoDtm registro, MetricaDeTotales metrica, ContextoSe contexto);
    public delegate string ResolverEtiquetaDelegate(string prop, object id, ContextoSe contexto, List<ITipoDeElementoDtm> tipos, List<EstadoDtm> estados, List<CentroGestorDtm> cgs);

    public static class PreguntaParaLaIa
    {
        // DTO interno para deserializar la respuesta de la IA
        private class RespuestaDeConteo
        {
            public List<ClausulaDeFiltrado> Filtros { get; set; } = new();
            public List<string> AgruparPor { get; set; } = new();
            public List<MetricaDeTotales> Metricas { get; set; } = new();
        }

        // Cache de resolvers por negocio — se cargan por reflexión desde los metadatos la primera vez
        private static readonly Dictionary<enumNegocio, ResolverCalculadoDelegate> _calculadoResolvers     = new();
        private static readonly Dictionary<enumNegocio, ResolverEtiquetaDelegate>  _etiquetaResolvers      = new();
        // Método fábrica que, dado un conjunto de IDs y el contexto, devuelve un resolver de claves virtuales
        private static readonly Dictionary<enumNegocio, MethodInfo>                _claveVirtualFactories  = new();

        private static void CargarResolversDeMetadatos(enumNegocio negocio)
        {
            var tipo = negocio.ObtenerMetadatos(emitirError: false)?.TipoDeAgregados;
            if (tipo == null) return;

            var flags = BindingFlags.Public | BindingFlags.Static;

            var mCalc = tipo.GetMethod(MetadatosDelNegocio.Metodo_ResolverCampoCalculado, flags)
                     ?? tipo.GetMethod(nameof(ResolverCalculadoDelegate), flags);
            if (mCalc != null)
                _calculadoResolvers[negocio] = (r, m, ctx) => (decimal?)mCalc.Invoke(null, new object[] { r, m, ctx });

            var mEtiq = tipo.GetMethod(nameof(ResolverEtiquetaDelegate), flags)
                     ?? tipo.GetMethod(MetadatosDelNegocio.Metodo_ResolverEtiqueta, flags);
            if (mEtiq != null)
                _etiquetaResolvers[negocio] = (prop, id, ctx, tip, est, cgs) => (string)mEtiq.Invoke(null, new object[] { prop, id, ctx, tip, est, cgs });

            var mClaves = tipo.GetMethod(MetadatosDelNegocio.Metodo_ObtenerResolverDeClaves, flags);
            if (mClaves != null)
                _claveVirtualFactories[negocio] = mClaves;
        }

        public static async Task<(string Texto, string FiltrosJson)> Resolver(ContextoSe contexto, enumNegocio negocio, string pregunta, List<ClausulaDeFiltrado> filtrosDePantalla, string historial = null)
        {
            // --- Cargar metadatos del negocio ---
            var tipos = negocio.UsaTipo() ? negocio.Tipos(contexto).ToList() : new List<ITipoDeElementoDtm>();
            var estados = negocio.UsaEstado() ? negocio.Estados(contexto).ToList() : new List<EstadoDtm>();
            var transiciones = negocio.UsaEstado() ? negocio.Transiciones(contexto).ToList() : new List<TransicionDtm>();
            var etapas = negocio.ListaDeEtapas(erroSiNoHay: false);
            var cgs = negocio.UsaCg()
                ? contexto.SeleccionarTodos<CentroGestorDtm>(new List<ClausulaDeFiltrado>())
                : new List<CentroGestorDtm>();

            // --- Preparar contexto textual para la IA ---
            var listaDeCgs = $"ID|Nombre{Environment.NewLine}" + string.Join(Environment.NewLine, cgs.Select(c => $"{c.Id}|{c.Nombre}"));
            var listaDeTipos = $"ID|Nombre{Environment.NewLine}" + string.Join(Environment.NewLine, tipos.Select(t => $"{t.Id}|{t.Nombre}"));
            var listaDeEstados = $"ID|Nombre|Inicial|Terminado|Cancelado{Environment.NewLine}" + string.Join(Environment.NewLine, estados.Select(e => $"{e.Id}|{e.Nombre}|{e.Inicial}|{e.Terminado}|{e.Cancelado}"));
            var listaDeTransiciones = $"ID|Nombre|IdOrigen|IdDestino{Environment.NewLine}" + string.Join(Environment.NewLine, transiciones.Select(t => $"{t.Id}|{t.Nombre}|{t.IdOrigen}|{t.IdDestino}"));
            var listaDeEtapas = $"Etapa|Descripción{Environment.NewLine}" + string.Join(Environment.NewLine, etapas.Select(e => $"{e.nombre}|{e.descripcion}"));
            var listaDePropiedades = PropiedadesDelNegocio(negocio);

            // --- Llamar a la IA ---
            var iaUsada = ExtensorDeUsuarios.IaUsada(contexto);
            var ia = ExtensorDeIa.CrearIa(iaUsada);

            contexto.IniciarTraza($"{negocio}-prompt-conteo-{contexto.Usuario.Login}", debugar: true);
            try
            {
                var json = await ExtensorDeIa.AnalizarPreguntaDeConteo(contexto, negocio, ia, pregunta, listaDeCgs, listaDeTipos, listaDeEstados, listaDeEtapas, listaDePropiedades, listaDeTransiciones, historial);

                // --- Deserializar la respuesta ---
                RespuestaDeConteo respuesta;
                try
                {
                    respuesta = JsonConvert.DeserializeObject<RespuestaDeConteo>(json)
                        ?? new RespuestaDeConteo();
                }
                catch
                {
                    return ($"La IA devolvió una respuesta no interpretable: {json}", null);
                }

                // Garantizar al menos la métrica Cuenta si la IA no devolvió métricas
                if (!respuesta.Metricas.Any())
                    respuesta.Metricas.Add(new MetricaDeTotales(enumOperacionDeTotales.Cuenta, "", "Cantidad"));

                // --- Fusionar filtros de pantalla con los filtros de la IA ---
                var filtrosEfectivos = Filtrar.FusionarFiltros(filtrosDePantalla, respuesta.Filtros);
                List<ClausulaDeFiltrado> clausulasDefiltrado = filtrosEfectivos == null ? new List<ClausulaDeFiltrado>() : JsonConvert.DeserializeObject<List<ClausulaDeFiltrado>>(filtrosEfectivos);

                // --- Obtener registros con seguridad aplicada ---
                var registros = contexto.SeleccionarTodos(negocio, clausulasDefiltrado, aplicarJoin: false);

                // --- Batch load direcciones si la consulta agrupa por propiedades de dirección ---
                var idElementos2 = registros.Select(r => r.Id).ToHashSet();
                Dictionary<int, List<DireccionDtm>> cacheDirecciones = null;
                if (respuesta.AgruparPor.Any(EsPropiedadDeDireccion))
                {
                    try
                    {
                        cacheDirecciones = negocio.Direcciones(contexto)
                            .Where(d => idElementos2.Contains(d.IdElemento))
                            .ToList()
                            .GroupBy(d => d.IdElemento)
                            .ToDictionary(g => g.Key, g => g.ToList());
                    }
                    catch { cacheDirecciones = new Dictionary<int, List<DireccionDtm>>(); }
                }

                // --- Delegados por negocio: carga lazy desde metadatos la primera vez ---
                if (!_calculadoResolvers.ContainsKey(negocio) && !_etiquetaResolvers.ContainsKey(negocio))
                    CargarResolversDeMetadatos(negocio);

                Func<ElementoDeProcesoDtm, MetricaDeTotales, decimal?> resolverEspecifico =
                    _calculadoResolvers.TryGetValue(negocio, out var calc)
                        ? (r, m) => calc(r, m, contexto)
                        : null;

                // Resolver de claves virtuales específico del negocio (N:M joins, etc.)
                // Se pasa agruparPor para que solo cargue las tablas que realmente se necesitan.
                Func<string, ElementoDeProcesoDtm, string> resolverClavesNegocio = null;
                if (_claveVirtualFactories.TryGetValue(negocio, out var mFab))
                {
                    try
                    {
                        resolverClavesNegocio = (Func<string, ElementoDeProcesoDtm, string>)
                            mFab.Invoke(null, new object[] { idElementos2.ToList(), contexto, respuesta.AgruparPor });
                    }
                    catch { /* si falla el batch-load, simplemente no hay resolver */ }
                }

                // Fusionar: primero el resolver de negocio, luego el de direcciones
                Func<string, ElementoDeProcesoDtm, string> resolverClaveVirtual = null;
                if (resolverClavesNegocio != null || cacheDirecciones != null)
                {
                    resolverClaveVirtual = (prop, r) =>
                    {
                        if (resolverClavesNegocio != null)
                        {
                            var val = resolverClavesNegocio(prop, r);
                            if (val != null) return val;
                        }
                        if (cacheDirecciones != null && EsPropiedadDeDireccion(prop))
                            return ResolverClaveDireccion(prop, r.Id, cacheDirecciones);
                        return null;
                    };
                }

                // Resolver genérico de tiempo en estado (válido para cualquier negocio con hitos).
                // Carga los hitos de todos los registros en un único batch al primer acceso por campo.
                var cacheHitos = new Dictionary<string, Dictionary<int, long>>();
                // Cache para "calculado:TiempoHastaEstado": IdElemento → minFecha.Ticks del primer hito en los estados destino
                var cacheHastaEstado = new Dictionary<string, Dictionary<int, long>>();
                // Cache para "calculado:TiempoEnEstado" sin IDs: media histórica de ticks por IdEstado
                // (incluye todos los elementos del negocio, no solo los del resultado actual)
                Dictionary<int, long> cacheMediaHistoricaPorEstado = null;
                var idElementos = registros.Select(r => r.Id).ToHashSet();

                Func<ElementoDeProcesoDtm, MetricaDeTotales, decimal?> resolverCalculado = (r, m) =>
                {
                    if (m.Campo.StartsWith(IIaPromptConteo.CampoTiempoHastaEstado + ":", StringComparison.OrdinalIgnoreCase))
                    {
                        // Tiempo desde FechaCreacion hasta el primer hito en los estados indicados
                        var cacheKey = m.Campo.ToLowerInvariant();
                        if (!cacheHastaEstado.ContainsKey(cacheKey))
                        {
                            var idsStr = m.Campo.Substring((IIaPromptConteo.CampoTiempoHastaEstado + ":").Length);
                            var ids = idsStr.Split(',')
                                           .Select(s => int.TryParse(s.Trim(), out var i) ? i : 0)
                                           .Where(i => i > 0).ToList();
                            try
                            {
                                // No filtramos por Tiempo — usamos Fecha (cuando entró al estado)
                                var hitos = negocio.Hitos(contexto)
                                                   .Where(h => idElementos.Contains(h.IdElemento) && ids.Contains(h.IdEstado))
                                                   .ToList();
                                cacheHastaEstado[cacheKey] = hitos
                                    .GroupBy(h => h.IdElemento)
                                    .ToDictionary(g => g.Key, g => g.Min(h => h.Fecha).Ticks);
                            }
                            catch { cacheHastaEstado[cacheKey] = new Dictionary<int, long>(); }
                        }
                        if (!cacheHastaEstado[cacheKey].TryGetValue(r.Id, out var fechaTicks)) return null;
                        var dias = (decimal)TimeSpan.FromTicks(fechaTicks - r.FechaCreacion.Ticks).TotalDays;
                        return dias > 0 ? dias : null;
                    }

                    if (m.Campo.StartsWith(IIaPromptConteo.CampoTiempoEnEstado + ":", StringComparison.OrdinalIgnoreCase))
                    {
                        // Con IDs específicos: calculado:TiempoEnEstado:5,24
                        var cacheKey = m.Campo.ToLowerInvariant();
                        if (!cacheHitos.ContainsKey(cacheKey))
                        {
                            var idsStr = m.Campo.Substring((IIaPromptConteo.CampoTiempoEnEstado + ":").Length);
                            var ids = idsStr.Split(',')
                                           .Select(s => int.TryParse(s.Trim(), out var i) ? i : 0)
                                           .Where(i => i > 0).ToList();
                            try
                            {
                                var hitos = negocio.Hitos(contexto)
                                                   .Where(h => idElementos.Contains(h.IdElemento)
                                                             && ids.Contains(h.IdEstado)
                                                             && h.Tiempo.HasValue && h.Tiempo.Value > 0)
                                                   .ToList();
                                cacheHitos[cacheKey] = hitos
                                    .GroupBy(h => h.IdElemento)
                                    .ToDictionary(g => g.Key, g => g.Sum(h => h.Tiempo!.Value));
                            }
                            catch { cacheHitos[cacheKey] = new Dictionary<int, long>(); }
                        }
                        if (!cacheHitos[cacheKey].TryGetValue(r.Id, out var ticks) || ticks == 0) return null;
                        return (decimal)TimeSpan.FromTicks(ticks).TotalDays;
                    }

                    if (m.Campo.Equals(IIaPromptConteo.CampoTiempoEnEstado, StringComparison.OrdinalIgnoreCase))
                    {
                        // Sin IDs: media histórica de todos los hitos del negocio por IdEstado.
                        // Incluye elementos ya terminados/cancelados, no solo los del resultado actual.
                        // Todos los elementos del mismo grupo devuelven el mismo valor para que
                        // la operación Media produzca la media histórica de ese estado.
                        if (cacheMediaHistoricaPorEstado == null)
                        {
                            cacheMediaHistoricaPorEstado = new Dictionary<int, long>();
                            try
                            {
                                var grupos = negocio.Hitos(contexto)
                                                    .Where(h => h.Tiempo.HasValue && h.Tiempo.Value > 0)
                                                    .ToList()
                                                    .GroupBy(h => h.IdEstado)
                                                    .Select(g => new { Estado = g.Key, Sum = g.Sum(h => h.Tiempo!.Value), Cnt = (long)g.Count() });
                                foreach (var g in grupos)
                                    if (g.Cnt > 0)
                                        cacheMediaHistoricaPorEstado[g.Estado] = g.Sum / g.Cnt;
                            }
                            catch { }
                        }
                        if (!cacheMediaHistoricaPorEstado.TryGetValue(r.IdEstado, out var avgTicks) || avgTicks == 0) return null;
                        return (decimal)TimeSpan.FromTicks(avgTicks).TotalDays;
                    }

                    return resolverEspecifico?.Invoke(r, m);
                };

                Func<string, object, string> resolverEtiqueta =
                    _etiquetaResolvers.TryGetValue(negocio, out var etiq)
                        ? (prop, id) => etiq(prop, id, contexto, tipos, estados, cgs)
                        : (prop, id) => ResolverNombreComun(prop, id, tipos, estados, cgs);

                var resultado = "";

                // Detectar consulta histórica por estado: agrupa desde TAREA_HISTORIA directamente
                // en lugar de desde los elementos activos del resultado (TareasDtm).
                bool esHistoricaPorEstado =
                    respuesta.AgruparPor.Count == 1
                    && respuesta.AgruparPor[0].Equals("IdEstado", StringComparison.OrdinalIgnoreCase)
                    && respuesta.Metricas.Any(m => m.Campo.Equals(IIaPromptConteo.CampoTiempoEnEstado, StringComparison.OrdinalIgnoreCase));

                // --- Contar / agregar ---
                if (esHistoricaPorEstado)
                {
                    resultado = FormatearTiempoEnEstadoHistorico(contexto, negocio, respuesta, resolverEtiqueta);
                }
                else if (!respuesta.AgruparPor.Any())
                    resultado = FormatearOpcionA(registros, negocio, respuesta.Metricas, pregunta, resolverCalculado);
                else
                {
                    var bloque = registros.ObtenerTotalesConCalculados(
                        new List<AgrupacionDeTotales>
                        {
                    new AgrupacionDeTotales(respuesta.AgruparPor, respuesta.Metricas)
                        },
                        resolverCalculado,
                        resolverClaveVirtual)[0];

                    resultado = FormatearOpcionB(bloque, respuesta.AgruparPor, respuesta.Metricas, resolverEtiqueta);
                }

                contexto.AnotarTraza($"Respuesta:{Environment.NewLine}", resultado);
                return (resultado, filtrosEfectivos);
            }
            catch(Exception e)
            {
                contexto.AnotarExcepcion(e);
                throw;
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }

        // --- Histórico por estado: agrupa desde TAREA_HISTORIA, incluye todos los elementos (activos y finalizados) ---
        private static string FormatearTiempoEnEstadoHistorico(
            ContextoSe contexto,
            enumNegocio negocio,
            RespuestaDeConteo respuesta,
            Func<string, object, string> resolverEtiqueta)
        {
            // Extraer IDs de estado excluidos / incluidos del filtro generado por la IA
            HashSet<int> excluidos = new();
            HashSet<int> incluidos = null;
            foreach (var f in respuesta.Filtros)
            {
                if (!f.Clausula.Equals("IdEstado", StringComparison.OrdinalIgnoreCase)) continue;
                var ids = f.Valor.Split(',')
                           .Select(s => int.TryParse(s.Trim(), out var i) ? i : 0)
                           .Where(i => i > 0).ToHashSet();
                if (f.Criterio == enumCriteriosDeFiltrado.noEsNingunoDe)
                    excluidos = ids;
                else if (f.Criterio == enumCriteriosDeFiltrado.esAlgunoDe || f.Criterio == enumCriteriosDeFiltrado.igual)
                    incluidos = ids;
            }

            // Cargar TODOS los hitos con tiempo cerrado (tiempo is not null)
            var hitos = negocio.Hitos(contexto)
                               .Where(h => h.Tiempo.HasValue && h.Tiempo.Value > 0)
                               .ToList();

            // Aplicar filtro de estados sobre los hitos (no sobre TareasDtm)
            if (excluidos.Any())
                hitos = hitos.Where(h => !excluidos.Contains(h.IdEstado)).ToList();
            else if (incluidos != null)
                hitos = hitos.Where(h => incluidos.Contains(h.IdEstado)).ToList();

            // Agrupar por IdEstado del hito y construir BloqueDeTotales
            var bloque = new BloqueDeTotales
            {
                PropiedadesDeAgrupacion = new List<string> { "IdEstado" },
                Filas = new List<FilaDeTotales>()
            };

            foreach (var grupo in hitos.GroupBy(h => h.IdEstado))
            {
                var diasPorHito = grupo
                    .Select(h => (decimal)TimeSpan.FromTicks(h.Tiempo!.Value).TotalDays)
                    .ToList();

                var fila = new FilaDeTotales
                {
                    Claves = new Dictionary<string, object> { { "IdEstado", (object)grupo.Key } }
                };

                foreach (var metrica in respuesta.Metricas)
                {
                    object val;
                    if (metrica.Campo.Equals(IIaPromptConteo.CampoTiempoEnEstado, StringComparison.OrdinalIgnoreCase))
                        val = AgregadosHelper.Calcular(diasPorHito, metrica.Operacion);
                    else if (metrica.Operacion == enumOperacionDeTotales.Cuenta)
                        val = (object)grupo.Count();
                    else
                        val = null;

                    if (val != null)
                        fila.Totales[metrica.Alias] = val;
                }

                bloque.Filas.Add(fila);
            }

            return FormatearOpcionB(bloque, new List<string> { "IdEstado" }, respuesta.Metricas, resolverEtiqueta);
        }

        // --- Opción A: sin agrupación — respuesta en una o pocas líneas ---
        private static string FormatearOpcionA(
            List<ElementoDeProcesoDtm> registros,
            enumNegocio negocio,
            List<MetricaDeTotales> metricas,
            string pregunta,
            Func<ElementoDeProcesoDtm, MetricaDeTotales, decimal?> resolverCalculado)
        {
            var sb = new StringBuilder();
            var nombreNegocio = negocio.Plural().ToLower();
            var tipoReal = registros.FirstOrDefault()?.GetType() ?? typeof(ElementoDeProcesoDtm);

            foreach (var metrica in metricas)
            {
                if (metrica.Operacion == enumOperacionDeTotales.Cuenta)
                {
                    if (metrica.Campo.StartsWith("calculado:", StringComparison.OrdinalIgnoreCase) && resolverCalculado != null)
                    {
                        var conValor = registros.Count(r => { var v = resolverCalculado(r, metrica); return v.HasValue && v.Value != 0m; });
                        sb.AppendLine($"{metrica.Alias}: {conValor}");
                    }
                    else
                    {
                        sb.AppendLine($"Hay {registros.Count} {nombreNegocio}.");
                    }
                    continue;
                }

                if (metrica.Campo.StartsWith("calculado:", StringComparison.OrdinalIgnoreCase))
                {
                    if (resolverCalculado != null)
                    {
                        var vals = registros.Select(r => resolverCalculado(r, metrica)).Where(v => v.HasValue).Select(v => v.Value).ToList();
                        var res = AgregadosHelper.Calcular(vals, metrica.Operacion);
                        sb.AppendLine(res != null
                            ? $"{metrica.Alias}: {Convert.ToDecimal(res).Formatear(alineacion: false)}"
                            : $"{metrica.Alias}: (sin datos)");
                    }
                    else
                    {
                        sb.AppendLine($"{metrica.Alias}: (campo calculado no disponible para este negocio)");
                    }
                    continue;
                }

                var propInfo = tipoReal.GetProperty(metrica.Campo,
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);

                if (propInfo != null)
                {
                    var valores = registros
                        .Select(r => propInfo.GetValue(r))
                        .Where(v => v != null)
                        .Select(v => Convert.ToDecimal(v))
                        .ToList();

                    var resultado = AgregadosHelper.Calcular(valores, metrica.Operacion);
                    sb.AppendLine(resultado != null
                        ? $"{metrica.Alias}: {Convert.ToDecimal(resultado).Formatear(alineacion: false)}"
                        : $"{metrica.Alias}: (sin datos)");
                }
            }

            return sb.ToString().TrimEnd();
        }

        // --- Opción B: con agrupación — tabla monospace ---
        private static string FormatearOpcionB(
            BloqueDeTotales bloque,
            List<string> agruparPor,
            List<MetricaDeTotales> metricas,
            Func<string, object, string> resolverEtiqueta)
        {
            const int anchoMaxClave = 45;
            const int anchoMinMet   = 6;
            const string sep        = "  ";

            var filas = bloque.Filas.ToList();

            // Pre-calcular etiquetas de clave para determinar anchos dinámicos
            var etiquetasPorFila = filas.Select(fila =>
                agruparPor.Select(prop => {
                    var idVal = fila.Claves.ContainsKey(prop) ? fila.Claves[prop] : null;
                    return resolverEtiqueta(prop, idVal);
                }).ToList()
            ).ToList();

            // Ancho dinámico por columna clave: max(header, contenido real), tope en anchoMaxClave
            var anchosClave = agruparPor.Select((prop, ci) => {
                var maxContenido = etiquetasPorFila.Select(row => row[ci].Length).DefaultIfEmpty(0).Max();
                var headerLen    = NombreColumna(prop).Length;
                return Math.Min(anchoMaxClave, Math.Max(headerLen, maxContenido));
            }).ToList();

            // Pre-formatear valores de métricas
            var valoresPorFila = filas.Select(fila =>
                metricas.Select(m =>
                {
                    if (!fila.Totales.ContainsKey(m.Alias)) return "-";
                    var v = fila.Totales[m.Alias];
                    return v is int or long
                        ? Convert.ToInt32(v).ToString()
                        : Convert.ToDecimal(v).Formatear(alineacion: false);
                }).ToList()
            ).ToList();

            // Ancho de cada columna métrica = max(alias, valor más largo, mínimo)
            var anchos = metricas.Select((m, i) =>
            {
                var maxVal = valoresPorFila.Select(row => row[i].Length).DefaultIfEmpty(0).Max();
                return Math.Max(Math.Max(m.Alias.Length, maxVal), anchoMinMet);
            }).ToList();

            var sb = new StringBuilder();

            // Cabecera
            var cabeceraClave = string.Join(" | ", agruparPor.Select((p, ci) => NombreColumna(p).PadRight(anchosClave[ci])));
            var cabeceraMets  = string.Join(sep, metricas.Select((m, i) => m.Alias.PadRight(anchos[i])));
            sb.AppendLine($"{cabeceraClave}{sep}{cabeceraMets}");

            // Separador
            var totalAnchoClave = anchosClave.Sum() + (agruparPor.Count - 1) * 3;
            var totalAncho = totalAnchoClave + sep.Length + anchos.Sum() + sep.Length * (metricas.Count - 1);
            sb.AppendLine(new string('-', totalAncho));

            // Filas ordenadas por primera métrica descendente
            var primera = metricas.First().Alias;
            var indices = filas
                .Select((f, i) => (f, i))
                .OrderByDescending(x => x.f.Totales.ContainsKey(primera) ? Convert.ToDecimal(x.f.Totales[primera]) : 0m)
                .Select(x => x.i).ToList();

            foreach (var idx in indices)
            {
                var celdas = string.Join(" | ", agruparPor.Select((prop, ci) =>
                {
                    var texto = etiquetasPorFila[idx][ci];
                    var ancho = anchosClave[ci];
                    return texto.Length > ancho ? texto[..(ancho - 1)] + "…" : texto.PadRight(ancho);
                }));

                var valores = string.Join(sep, valoresPorFila[idx].Select((v, i) => extCadenas.Centrar(v, anchos[i])));
                sb.AppendLine($"{celdas}{sep}{valores}");
            }

            return sb.ToString();
        }


        private static readonly HashSet<string> _propsDireccion = new(StringComparer.OrdinalIgnoreCase)
        {
            "Calle","CalleObra","CalleEjecucion","CalleFiscal","CalleEntrega","CalleContacto","CalleCorrespondencia",
            "Municipio","MunicipioObra","MunicipioEjecucion","MunicipioFiscal","MunicipioEntrega",
            "Provincia","ProvinciaObra","ProvinciaFiscal",
            "Pais","PaisObra","PaisFiscal",
            "CodigoPostal","CodigoPostalObra","CodigoPostalFiscal"
        };

        private static bool EsPropiedadDeDireccion(string prop) => _propsDireccion.Contains(prop);

        private static string ResolverClaveDireccion(string prop, int idElemento, Dictionary<int, List<DireccionDtm>> cache)
        {
            if (!cache.TryGetValue(idElemento, out var dirs) || !dirs.Any()) return "—";

            // Extraer campo base y calificador del nombre de la propiedad
            var (campo, calificador) = ParsearPropDireccion(prop);

            var dir = calificador.HasValue
                ? dirs.FirstOrDefault(d => d.Calificador == calificador.Value && d.Activo)
                  ?? dirs.FirstOrDefault(d => d.Calificador == calificador.Value)
                : dirs.FirstOrDefault(d => d.Activo) ?? dirs.FirstOrDefault();

            if (dir == null) return "—";
            return campo switch
            {
                "calle"          => dir.Calle          ?? "—",
                "municipio"      => dir.Municipio      ?? "—",
                "provincia"      => dir.Provincia      ?? "—",
                "pais"           => dir.Pais           ?? "—",
                "codigopostal"   => dir.Cp             ?? "—",
                _                => "—"
            };
        }

        private static (string campo, enumCalificadorDireccion? calificador) ParsearPropDireccion(string prop)
        {
            var p = prop.ToLowerInvariant();
            var calificadores = new (string sufijo, enumCalificadorDireccion cal)[]
            {
                ("obra",            enumCalificadorDireccion.ejecucion),
                ("ejecucion",       enumCalificadorDireccion.ejecucion),
                ("fiscal",          enumCalificadorDireccion.fiscal),
                ("entrega",         enumCalificadorDireccion.entrega),
                ("contacto",        enumCalificadorDireccion.contacto),
                ("correspondencia", enumCalificadorDireccion.correspondencia),
            };
            var bases = new[] { "codigopostal", "provincia", "municipio", "calle", "pais" };
            foreach (var b in bases)
            {
                if (!p.StartsWith(b)) continue;
                var sufijo = p.Substring(b.Length);
                if (sufijo == string.Empty) return (b, null);
                foreach (var (s, cal) in calificadores)
                    if (sufijo == s) return (b, cal);
            }
            return (p, null);
        }

        private static string NombreColumna(string prop) => prop.ToLowerInvariant() switch
        {
            "idtipo"                => "Tipo",
            "idestado"              => "Estado",
            "idcg"                  => "Centro Gestor",
            "idresponsable"         => "Responsable",
            "idsolicitante"         => "Solicitante",
            "idproveedor"           => "Proveedor",
            "idcliente"             => "Cliente",
            "idusuacrea"            => "Creador",
            "idusuamodi"            => "Modificador",
            "idfacturaemt"          => "Factura emitida",
            "porcentajeirpf"        => "% IRPF",
            "porcentajeiva"         => "Tipo IVA",
            "referenciaexpediente"  => "Expediente",
            "nombreexpediente"      => "Nombre expediente",
            "referenciappt"         => "Presupuesto",
            "nombreppt"             => "Nombre presupuesto",
            "calle"                 => "Calle",
            "calleobra"             => "Calle (obra)",
            "calleejecucion"        => "Calle (ejecución)",
            "callefiscal"           => "Calle (fiscal)",
            "municipio"             => "Municipio",
            "municipioobra"         => "Municipio (obra)",
            "municipiofiscal"       => "Municipio (fiscal)",
            "provincia"             => "Provincia",
            "provinciaobra"         => "Provincia (obra)",
            "pais"                  => "País",
            "codigopostal"          => "CP",
            _ => prop
        };

        private static string ResolverNombreComun(string prop, object idVal, List<ITipoDeElementoDtm> tipos, List<EstadoDtm> estados, List<CentroGestorDtm> cgs)
        {
            if (idVal == null) return "—";
            if (!int.TryParse(idVal.ToString(), out var id)) return idVal.ToString();

            return prop.ToLowerInvariant() switch
            {
                "idtipo" => tipos.FirstOrDefault(t => t.Id == id)?.Nombre ?? idVal.ToString(),
                "idestado" => estados.FirstOrDefault(e => e.Id == id)?.Nombre ?? idVal.ToString(),
                "idcg" => cgs.FirstOrDefault(c => c.Id == id)?.Nombre ?? idVal.ToString(),
                _ => idVal.ToString()
            };
        }

        // Lista de propiedades directas del DTM de proceso disponibles para métricas/agrupación.
        // Los gestores específicos pueden ampliarla según el negocio.
        private static string PropiedadesDelNegocio(enumNegocio negocio)
        {
            var tipoDtm = negocio.TipoDtm();
            var propiedades = tipoDtm
                .GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                .Where(p => p.PropertyType.IsPrimitive || p.PropertyType == typeof(decimal) || p.PropertyType == typeof(decimal?)
                         || p.PropertyType == typeof(string) || p.PropertyType == typeof(DateTime) || p.PropertyType == typeof(DateTime?))
                .Select(p => $"{p.Name} ({p.PropertyType.Name})")
                .ToList();
            return string.Join(Environment.NewLine, propiedades);
        }
    }
}
