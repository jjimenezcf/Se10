using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Terceros;
using GestoresDeNegocio.TrabajosSometidos;
using ModeloDeDto.Callejero;
using ModeloDeDto.MaestrosTecnico;
using ImportarJuzgadosDto = ModeloDeDto.Terceros.ImportarJuzgados;
using OfficeOpenXml;
using ServicioDeDatos;
using ServicioDeDatos.Callejero;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.MaestrosTecnico;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.TrabajosSometidos;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Utilidades;

namespace GestoresDeNegocio.MaestrosTecnico
{
    public enum enumTrabajosDeMaestros
    {
        [Description("Importar catálogo de unitarios desde Excel")]
        ImportarCatalogoDeUnitarios,
        [Description("Importar juzgados desde Excel")]
        ImportarJuzgados
    }

    public class ResumenDeImportacionDeCatalogo
    {
        public int TotalFilas { get; set; }
        public int Creados { get; set; }
        public int Descartados { get; set; }
        public List<string> Errores { get; } = new List<string>();
    }

    public static class TrabajosParaMaestros
    {
        //-------------------------------------------------------------------------------------------------------------
        // Sometimiento del job
        //-------------------------------------------------------------------------------------------------------------
        public static TrabajoDeUsuarioDtm SometerImportarCatalogoDeUnitarios(ContextoSe contexto, int idArchivo)
        {
            var dll = Assembly.GetExecutingAssembly().GetName().Name;
            var clase = typeof(TrabajosParaMaestros).FullName;
            var ts = GestorDeTrabajosSometido.CrearObtener(contexto, enumTrabajosDeMaestros.ImportarCatalogoDeUnitarios.Descripcion(), dll, clase, nameof(enumTrabajosDeMaestros.ImportarCatalogoDeUnitarios), comunicarFin: true);

            var parametrosEntrada = new Dictionary<string, object> {
                { nameof(ImportarCatalogoDeUnitariosDto.IdArchivo), idArchivo }
            };
            var datosDeCreacion = new Dictionary<string, object>
            {
                { nameof(TrabajoDeUsuarioDtm.Parametros), parametrosEntrada.ToJson() },
                { nameof(TrabajoDeUsuarioDtm.Planificado), DateTime.Now.AddMinutes(-1) }
            };

            return GestorDeTrabajosDeUsuario.Crear(contexto, ts, datosDeCreacion);
        }

        //-------------------------------------------------------------------------------------------------------------
        // Punto de entrada del job (invocado por el motor de trabajos sometidos)
        //-------------------------------------------------------------------------------------------------------------
        public static void ImportarCatalogoDeUnitarios(EntornoDeTrabajo entorno)
        {
            var contexto = entorno.contextoDelProceso;
            Dictionary<string, object> parametros = entorno.TrabajoDeUsuario.Parametros.ToDiccionarioDeParametros();
            var idArchivo = (int)parametros.LeerValor<long>(nameof(ImportarCatalogoDeUnitariosDto.IdArchivo));

            contexto.IniciarTraza(nameof(enumTrabajosDeMaestros.ImportarCatalogoDeUnitarios));
            var otorgado = entorno.Ejecutor.OtorgarAdministrador(contexto);
            try
            {
                var resumen = ImportarCatalogoDeUnitariosInterno(contexto, idArchivo);

                var traza = $"Importación finalizada: {resumen.TotalFilas} filas leídas, {resumen.Creados} creados, {resumen.Descartados} descartados";
                entorno.CrearTraza(traza);
                foreach (var errores in resumen.Errores)
                {
                    entorno.CrearTraza(errores);
                }

            }
            catch (Exception e)
            {
                entorno.AnotarError(e);
            }
            finally
            {
                if (otorgado) entorno.Ejecutor.AnularAdministrador(contexto, otorgado);
                contexto.CerrarTraza();
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        // Lógica de importación: descarta (con rollback) cualquier fila que falle, sin abortar el resto del catálogo
        //-------------------------------------------------------------------------------------------------------------
        public static ResumenDeImportacionDeCatalogo ImportarCatalogoDeUnitariosInterno(ContextoSe contexto, int idArchivo)
        {
            var archivo = contexto.SeleccionarPorId<ArchivoDtm>(idArchivo);
            var fichero = ApiDeArchivos.ObtenerRutaArchivo(archivo);
            var resumen = new ResumenDeImportacionDeCatalogo();

            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            using (var libro = new ExcelPackage(new FileInfo(fichero)))
            {
                var hoja = libro.Workbook.Worksheets.FirstOrDefault(h => h.Dimension != null);
                if (hoja == null)
                    GestorDeErrores.Emitir("El fichero no contiene ninguna hoja con datos");

                var columnas = LocalizarColumnas(hoja, out int filaCabecera);
                var gestor = GestorDeUnitarios.Gestor(contexto, contexto.Mapeador);
                var gestorDeNaturalezas = GestorDeNaturalezas.Gestor(contexto, contexto.Mapeador);

                for (int fila = filaCabecera + 1; fila <= hoja.Dimension.End.Row; fila++)
                {
                    var referencia = Texto(hoja, fila, columnas["Referencia"]);
                    var nombre = Texto(hoja, fila, columnas["Nombre"]);
                    if (referencia.IsNullOrEmpty() && nombre.IsNullOrEmpty())
                        continue; // fila en blanco, no cuenta como fila de datos

                    resumen.TotalFilas++;
                    var tran = contexto.IniciarTransaccion();
                    try
                    {
                        var siglaUnidad = Texto(hoja, fila, columnas["Unidad"]);
                        var unidad = contexto.Set<UnidadDtm>().FirstOrDefault(u => u.Sigla == siglaUnidad);
                        if (unidad == null)
                            throw new Exception($"la unidad de sigla '{siglaUnidad}' no existe");

                        var siglaNaturaleza = Texto(hoja, fila, columnas["SiglaNaturaleza"]);
                        var nombreNaturaleza = Texto(hoja, fila, columnas["NaturalezaNombre"]);
                        var codigoCuentaDeGasto = Texto(hoja, fila, columnas["CuentaDeGasto"]);
                        var codigoCuentaDeIngreso = Texto(hoja, fila, columnas["CuentaDeIngreso"]);
                        var naturaleza = ResolverOCrearNaturaleza(contexto, gestorDeNaturalezas, siglaNaturaleza, nombreNaturaleza, codigoCuentaDeGasto, codigoCuentaDeIngreso);

                        var elemento = new UnitarioDto
                        {
                            Nombre = nombre,
                            Clase = enumClaseUnitario.Material,
                            IdUnidad = unidad.Id,
                            IdNaturaleza = naturaleza.Id,
                            Descripcion = Texto(hoja, fila, columnas.GetValueOrDefault("Descripcion", 0)),
                            Referencia = referencia,
                            Coste = Decimal(hoja, fila, columnas["Coste"]),
                            Venta = Decimal(hoja, fila, columnas["Venta"]),
                            Proponer = false,
                            Baja = EsSiNo(Texto(hoja, fila, columnas.GetValueOrDefault("Baja", 0)))
                        };

                        gestor.PersistirElementoDto(elemento, new ParametrosDeNegocio(enumTipoOperacion.Insertar));
                        contexto.Commit(tran);
                        resumen.Creados++;
                    }
                    catch (Exception e)
                    {
                        contexto.Rollback(tran);
                        resumen.Descartados++;
                        resumen.Errores.Add($"Fila {fila}: {GestorDeErrores.Detalle(e)}");
                    }
                }
            }

            return resumen;
        }

        //-------------------------------------------------------------------------------------------------------------
        // Localización de la cabecera y de las columnas por nombre (no por posición)
        //-------------------------------------------------------------------------------------------------------------
        private static Dictionary<string, int> LocalizarColumnas(ExcelWorksheet hoja, out int filaCabecera)
        {
            // Ordenadas de la palabra clave más larga a la más corta: así "sigla naturaleza" se resuelve
            // antes que "naturaleza" para la misma celda, y cada celda de cabecera solo puede satisfacer una clave.
            var clavesBuscadas = new[]
            {
                ("SiglaNaturaleza", "sigla naturaleza"),
                ("Referencia", "referencia"),
                ("NaturalezaNombre", "naturaleza"),
                ("Descripcion", "descrip"),
                ("CuentaDeIngreso", "ingreso"),
                ("Nombre", "nombre"),
                ("Unidad", "unidad"),
                ("CuentaDeGasto", "gasto"),
                ("Coste", "coste"),
                ("Venta", "venta"),
                ("Baja", "baja")
            }.OrderByDescending(c => c.Item2.Length).ToList();

            var obligatorias = new[] { "Referencia", "Nombre", "Unidad", "SiglaNaturaleza", "NaturalezaNombre", "Coste", "Venta", "CuentaDeGasto", "CuentaDeIngreso" };

            var filasAExplorar = Math.Min(20, hoja.Dimension.End.Row);
            for (int fila = 1; fila <= filasAExplorar; fila++)
            {
                var columnas = new Dictionary<string, int>();
                for (int columna = 1; columna <= hoja.Dimension.End.Column; columna++)
                {
                    var texto = hoja.Cells[fila, columna].Text?.Trim().ToLowerInvariant();
                    if (texto.IsNullOrEmpty()) continue;

                    foreach (var (clave, palabra) in clavesBuscadas)
                    {
                        if (columnas.ContainsKey(clave) || !texto.Contains(palabra)) continue;
                        columnas[clave] = columna;
                        break;
                    }
                }

                if (columnas.ContainsKey("Referencia") && columnas.ContainsKey("Nombre"))
                {
                    var faltantes = obligatorias.Where(o => !columnas.ContainsKey(o)).ToList();
                    if (faltantes.Count > 0)
                        GestorDeErrores.Emitir($"No se han encontrado en la cabecera del catálogo las columnas: {string.Join(", ", faltantes)}");

                    filaCabecera = fila;
                    return columnas;
                }
            }

            GestorDeErrores.Emitir("No se ha encontrado, en las primeras filas del fichero, una fila de cabecera con las columnas 'Referencia' y 'Nombre'");
            filaCabecera = 0;
            return null;
        }

        //-------------------------------------------------------------------------------------------------------------
        // Resuelve la naturaleza por sigla; si no existe, comprueba si ya existe una con el mismo nombre (sigla
        // desactualizada en el Excel) y si tampoco existe, la crea con las cuentas de gasto e ingreso indicadas.
        //-------------------------------------------------------------------------------------------------------------
        private static NaturalezaDtm ResolverOCrearNaturaleza(ContextoSe contexto, GestorDeNaturalezas gestorDeNaturalezas, string sigla, string nombre, string codigoCuentaDeGasto, string codigoCuentaDeIngreso)
        {
            if (sigla.IsNullOrEmpty())
                throw new Exception("no se ha indicado la sigla de la naturaleza");

            var porSigla = contexto.Set<NaturalezaDtm>().FirstOrDefault(n => n.Sigla == sigla);
            if (porSigla != null) return porSigla;

            if (nombre.IsNullOrEmpty())
                throw new Exception($"la sigla de naturaleza '{sigla}' no existe y no se ha indicado el nombre de la naturaleza para poder crearla");

            var porNombre = contexto.Set<NaturalezaDtm>().FirstOrDefault(n => n.Nombre == nombre);
            if (porNombre != null)
                throw new Exception($"la sigla de naturaleza '{sigla}' no existe, pero sí existe una naturaleza con el nombre '{nombre}' (con sigla '{porNombre.Sigla}'); corrija la sigla en el Excel");

            var cuentaDeGasto = BuscarCuenta(contexto, codigoCuentaDeGasto);
            var cuentaDeIngreso = BuscarCuenta(contexto, codigoCuentaDeIngreso);

            var nuevaNaturaleza = new NaturalezaDto
            {
                Sigla = sigla,
                Nombre = nombre,
                IdCuentaDeGasto = cuentaDeGasto?.Id,
                IdCuentaDeIngreso = cuentaDeIngreso?.Id
            };
            var creada = gestorDeNaturalezas.PersistirElementoDto(nuevaNaturaleza, new ParametrosDeNegocio(enumTipoOperacion.Insertar));
            return contexto.SeleccionarPorId<NaturalezaDtm>(creada.Id);
        }

        private static CuentaDtm BuscarCuenta(ContextoSe contexto, string codigo)
        {
            if (codigo.IsNullOrEmpty()) return null;
            var cuenta = contexto.Set<CuentaDtm>().FirstOrDefault(c => c.Codigo == codigo);
            if (cuenta == null)
                throw new Exception($"no existe la cuenta contable con código '{codigo}'");
            return cuenta;
        }

        private static decimal Decimal(ExcelWorksheet hoja, int fila, int columna)
        {
            if (columna <= 0) return 0m;
            var celda = hoja.Cells[fila, columna];
            if (celda.Value is double numero) return (decimal)numero;
            if (celda.Value is decimal importe) return importe;

            var texto = celda.Text;
            if (texto.IsNullOrEmpty()) return 0m;
            if (decimal.TryParse(texto, NumberStyles.Any, CultureInfo.InvariantCulture, out var valor)) return valor;
            if (decimal.TryParse(texto, NumberStyles.Any, CultureInfo.CurrentCulture, out valor)) return valor;
            return 0m;
        }

        private static bool EsSiNo(string texto)
        {
            if (texto.IsNullOrEmpty()) return false;
            var valor = texto.Trim().ToUpperInvariant();
            return valor == "SI" || valor == "SÍ" || valor == "S" || valor == "TRUE" || valor == "X" || valor == "1";
        }

        //-------------------------------------------------------------------------------------------------------------
        // Sometimiento del job de importación de juzgados
        //-------------------------------------------------------------------------------------------------------------
        public static TrabajoDeUsuarioDtm SometerImportarJuzgados(ContextoSe contexto, int idArchivo, int? idProvincia)
        {
            var dll = Assembly.GetExecutingAssembly().GetName().Name;
            var clase = typeof(TrabajosParaMaestros).FullName;
            var ts = GestorDeTrabajosSometido.CrearObtener(contexto, enumTrabajosDeMaestros.ImportarJuzgados.Descripcion(), dll, clase, nameof(enumTrabajosDeMaestros.ImportarJuzgados), comunicarFin: true);

            var parametrosEntrada = new Dictionary<string, object> {
                { nameof(ImportarJuzgadosDto.IdArchivo), idArchivo },
                { nameof(ImportarJuzgadosDto.IdProvincia), idProvincia }
            };
            var datosDeCreacion = new Dictionary<string, object>
            {
                { nameof(TrabajoDeUsuarioDtm.Parametros), parametrosEntrada.ToJson() },
                { nameof(TrabajoDeUsuarioDtm.Planificado), DateTime.Now.AddMinutes(-1) }
            };

            return GestorDeTrabajosDeUsuario.Crear(contexto, ts, datosDeCreacion);
        }

        //-------------------------------------------------------------------------------------------------------------
        // Punto de entrada del job (invocado por el motor de trabajos sometidos)
        //-------------------------------------------------------------------------------------------------------------
        public static void ImportarJuzgados(EntornoDeTrabajo entorno)
        {
            var contexto = entorno.contextoDelProceso;
            Dictionary<string, object> parametros = entorno.TrabajoDeUsuario.Parametros.ToDiccionarioDeParametros();
            var idArchivo = (int)parametros.LeerValor<long>(nameof(ImportarJuzgadosDto.IdArchivo));
            var idProvincia = (int?)parametros.LeerValor<long?>(nameof(ImportarJuzgadosDto.IdProvincia), valorPorDefecto: (long?)null);

            contexto.IniciarTraza(nameof(enumTrabajosDeMaestros.ImportarJuzgados));
            var otorgado = entorno.Ejecutor.OtorgarAdministrador(contexto);
            try
            {
                var resumen = ImportarJuzgadosInterno(contexto, idArchivo, idProvincia);

                var traza = $"Importación finalizada: {resumen.TotalFilas} filas leídas, {resumen.Creados} creados, {resumen.Descartados} descartados";
                if (resumen.Errores.Count > 0)
                    traza += Environment.NewLine + string.Join(Environment.NewLine, resumen.Errores);

                entorno.CrearTraza(traza);
            }
            catch (Exception e)
            {
                entorno.AnotarError(e);
            }
            finally
            {
                if (otorgado) entorno.Ejecutor.AnularAdministrador(contexto, otorgado);
                contexto.CerrarTraza();
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        // Lógica de importación: descarta (con rollback) cualquier fila que falle, sin abortar el resto del catálogo
        // Si la clase de juzgado indicada no existe se crea; el municipio (Provincia + Municipio) debe existir ya en el callejero
        // Si se indica idProvincia, se descartan (sin contabilizar) las filas del excel de otra provincia
        //-------------------------------------------------------------------------------------------------------------
        public static ResumenDeImportacionDeCatalogo ImportarJuzgadosInterno(ContextoSe contexto, int idArchivo, int? idProvincia)
        {
            var archivo = contexto.SeleccionarPorId<ArchivoDtm>(idArchivo);
            var fichero = ApiDeArchivos.ObtenerRutaArchivo(archivo);
            var resumen = new ResumenDeImportacionDeCatalogo();

            var nombreProvinciaFiltro = idProvincia.HasValue
                ? contexto.SeleccionarPorId<ProvinciaDtm>(idProvincia.Value).Nombre
                : null;

            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            using (var libro = new ExcelPackage(new FileInfo(fichero)))
            {
                var hoja = libro.Workbook.Worksheets.FirstOrDefault(h => h.Dimension != null);
                if (hoja == null)
                    GestorDeErrores.Emitir("El fichero no contiene ninguna hoja con datos");

                var columnas = LocalizarColumnasDeJuzgados(hoja, out int filaCabecera);

                for (int fila = filaCabecera + 1; fila <= hoja.Dimension.End.Row; fila++)
                {
                    var claseTexto = Texto(hoja, fila, columnas["Clase"]);
                    var provincia = Texto(hoja, fila, columnas["Provincia"]);
                    var municipio = Texto(hoja, fila, columnas["Municipio"]);
                    var calificador = Texto(hoja, fila, columnas["Calificador"]);
                    if (claseTexto.IsNullOrEmpty() && provincia.IsNullOrEmpty() && municipio.IsNullOrEmpty() && calificador.IsNullOrEmpty())
                        continue; // fila en blanco, no cuenta como fila de datos

                    if (nombreProvinciaFiltro != null && !nombreProvinciaFiltro.Equals(provincia, StringComparison.OrdinalIgnoreCase))
                        continue; // fila de otra provincia, se descarta sin contabilizar

                    resumen.TotalFilas++;
                    var tran = contexto.IniciarTransaccion();
                    try
                    {
                        if (claseTexto.IsNullOrEmpty())
                            throw new Exception("no se ha indicado la clase de juzgado");
                        if (provincia.IsNullOrEmpty() || municipio.IsNullOrEmpty())
                            throw new Exception("no se ha indicado la provincia y/o el municipio");
                        if (calificador.IsNullOrEmpty())
                            throw new Exception("no se ha indicado el calificador del juzgado");

                        var provinciaDtm = contexto.SeleccionarPorPropiedad<ProvinciaDtm>(nameof(ProvinciaDtm.Nombre), provincia, errorSiNoHay: false);
                        if (provinciaDtm == null)
                            throw new Exception($"la provincia '{provincia}' no existe");

                        var municipioDtm = contexto.Set<MunicipioDtm>().FirstOrDefault(m => m.IdProvincia == provinciaDtm.Id && m.Nombre == municipio);
                        if (municipioDtm == null)
                            throw new Exception($"el municipio '{municipio}' de la provincia '{provincia}' no existe");

                        var claseDto = GestorDeClasesDeJuzgado.CrearClaseDto(contexto, claseTexto);
                        var municipioDto = municipioDtm.MapearDto<MunicipioDto>(contexto);

                        var nombre = $"{claseDto.Nombre} {calificador} de {municipioDto.Nombre}";
                        var existente = contexto.SeleccionarPorPropiedad<JuzgadoDtm>(nameof(JuzgadoDtm.Nombre), nombre, errorSiNoHay: false);
                        if (existente != null)
                        {
                            contexto.Rollback(tran);
                            resumen.Descartados++;
                            resumen.Errores.Add($"Fila {fila}: el juzgado '{nombre}' ya existe");
                            continue;
                        }

                        GestorDeJuzgados.CrearJuzgado(contexto, claseDto, calificador, municipioDto);
                        contexto.Commit(tran);
                        resumen.Creados++;
                    }
                    catch (Exception e)
                    {
                        contexto.Rollback(tran);
                        resumen.Descartados++;
                        resumen.Errores.Add($"Fila {fila}: {GestorDeErrores.Detalle(e)}");
                    }
                }
            }

            return resumen;
        }

        //-------------------------------------------------------------------------------------------------------------
        // Localización de la cabecera y de las columnas por nombre (no por posición)
        //-------------------------------------------------------------------------------------------------------------
        private static Dictionary<string, int> LocalizarColumnasDeJuzgados(ExcelWorksheet hoja, out int filaCabecera)
        {
            var clavesBuscadas = new Dictionary<string, string>
            {
                { "Clase", "clase" },
                { "Provincia", "provincia" },
                { "Municipio", "municipio" },
                { "Calificador", "calificador" }
            };
            var obligatorias = new[] { "Clase", "Provincia", "Municipio", "Calificador" };

            var filasAExplorar = Math.Min(20, hoja.Dimension.End.Row);
            for (int fila = 1; fila <= filasAExplorar; fila++)
            {
                var columnas = new Dictionary<string, int>();
                for (int columna = 1; columna <= hoja.Dimension.End.Column; columna++)
                {
                    var texto = hoja.Cells[fila, columna].Text?.Trim().ToLowerInvariant();
                    if (texto.IsNullOrEmpty()) continue;

                    foreach (var clave in clavesBuscadas)
                        if (!columnas.ContainsKey(clave.Key) && texto.Contains(clave.Value))
                            columnas[clave.Key] = columna;
                }

                var faltantes = obligatorias.Where(o => !columnas.ContainsKey(o)).ToList();
                if (faltantes.Count == 0)
                {
                    filaCabecera = fila;
                    return columnas;
                }
            }

            GestorDeErrores.Emitir("No se ha encontrado, en las primeras filas del fichero, una fila de cabecera con las columnas 'Clase', 'Provincia', 'Municipio' y 'Calificador'");
            filaCabecera = 0;
            return null;
        }

        private static string Texto(ExcelWorksheet hoja, int fila, int columna)
        {
            if (columna <= 0) return null;
            var valor = hoja.Cells[fila, columna].Text;
            return valor.IsNullOrEmpty() ? null : valor.Trim();
        }
    }
}
