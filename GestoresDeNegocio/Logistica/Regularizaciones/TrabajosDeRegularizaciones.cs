using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Entorno;
using GestoresDeNegocio.MaestrosTecnico;
using GestoresDeNegocio.TrabajosSometidos;
using ModeloDeDto;
using ModeloDeDto.Logistica;
using OfficeOpenXml;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Logistica;
using ServicioDeDatos.MaestrosTecnico;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.TrabajosSometidos;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using Utilidades;

namespace GestoresDeNegocio.Logistica
{
    public enum enumTrabajosDeRegularizaciones
    {
        [Description("Importar un recuento desde Excel")]
        ImportarRecuentos,

        [Description("Preparar recuento")]
        PrepararRecuento
    }

    public static class TrabajosParaRegularizaciones
    {
        // Columnas del Excel del recuento (las dos últimas son las que rellena el operario). Los títulos de Cantidad y Precio son los que busca
        // la importación del recuento (ver ImportarRecuentos), por eso el valor medio no se llama "precio"
        private const int FilaDeTituloDelRecuento = 1;
        private const int FilaDeEncolumnadoDelRecuento = 3;
        private const int PrimeraFilaDeDatosDelRecuento = 4;
        private const int ColumnaDeCantidadDelRecuento = 6;
        private const int ColumnaDePrecioDelRecuento = 7;
        private const double AnchoMinimoDeLasEditables = 20;

        //-------------------------------------------------------------------------------------------------------------
        // Sometimiento del job: la regularización ha de estar recontando y el usuario ha de ser gestor
        //-------------------------------------------------------------------------------------------------------------
        public static TrabajoDeUsuarioDtm SometerImportarRecuentos(ContextoSe contexto, int idArchivo, int idRegularizacion)
        {
            var regularizacion = contexto.SeleccionarPorId<RegularizacionDtm>(idRegularizacion, usarLaCache: false);
            regularizacion.ValidarQueSePuedeImportarUnRecuento(contexto);

            var dll = Assembly.GetExecutingAssembly().GetName().Name;
            var clase = typeof(TrabajosParaRegularizaciones).FullName;
            var ts = GestorDeTrabajosSometido.CrearObtener(contexto, enumTrabajosDeRegularizaciones.ImportarRecuentos.Descripcion(), dll, clase, nameof(enumTrabajosDeRegularizaciones.ImportarRecuentos), comunicarFin: true);

            var parametrosEntrada = new Dictionary<string, object> {
                { nameof(RecuentoDto.IdArchivo), idArchivo },
                { ltrDeUnaRegularizacion.IdRegularizacion, idRegularizacion }
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
        public static void ImportarRecuentos(EntornoDeTrabajo entorno)
        {
            var contexto = entorno.contextoDelProceso;
            Dictionary<string, object> parametros = entorno.TrabajoDeUsuario.Parametros.ToDiccionarioDeParametros();
            var idArchivo = (int)parametros.LeerValor<long>(nameof(RecuentoDto.IdArchivo));
            var idRegularizacion = (int)parametros.LeerValor<long>(ltrDeUnaRegularizacion.IdRegularizacion);

            contexto.IniciarTraza(nameof(enumTrabajosDeRegularizaciones.ImportarRecuentos));
            var otorgado = entorno.Ejecutor.OtorgarAdministrador(contexto);
            var idSemaforo = 0;
            try
            {
                // mientras se cargan las líneas nadie más puede importar un recuento en la misma regularización
                var referencia = contexto.SeleccionarPorId<RegularizacionDtm>(idRegularizacion, usarLaCache: false).Referencia;
                idSemaforo = SemaforoDeProcesoSql.PonerSemaforo(entorno.ContextoDelEntorno, enumNegocio.Regularizacion.IdNegocio(), idRegularizacion, enumOpercionesDeSemaforo.IRAL, referencia).Id;

                var resumen = ImportarRecuentosInterno(entorno, idArchivo, idRegularizacion);
                entorno.CrearTraza($"Importación finalizada: {resumen.TotalFilas} filas en el Excel, {resumen.Creados} importadas correctamente, {resumen.Descartados} con error");

                var regularizacion = contexto.SeleccionarPorId<RegularizacionDtm>(idRegularizacion, usarLaCache: false);
                AnexarElArchivoALaRegularizacion(entorno, contexto, regularizacion, contexto.SeleccionarPorId<ArchivoDtm>(idArchivo));
                entorno.AnotarElTrabajoEnElElemento(contexto, regularizacion, "Importación de recuento");
            }
            catch (Exception e)
            {
                entorno.AnotarError(e);
            }
            finally
            {
                if (idSemaforo > 0) SemaforoDeProcesoSql.QuitarSemaforo(entorno.ContextoDelEntorno, idSemaforo);
                if (otorgado) entorno.Ejecutor.AnularAdministrador(contexto, otorgado);
                contexto.CerrarTraza();
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        // Deja el fichero importado anexado a la regularización con el nombre "yyyy-MM-dd Recuento importado_x" (conservando la
        // extensión), donde x es 1 si ese día no hay ningún recuento importado anexado, o el siguiente número.
        // Es informativo: si falla no afecta a las líneas importadas
        //-------------------------------------------------------------------------------------------------------------
        private static void AnexarElArchivoALaRegularizacion(EntornoDeTrabajo entorno, ContextoSe contexto, RegularizacionDtm regularizacion, ArchivoDtm archivo)
        {
            var tran = contexto.IniciarTransaccion();
            try
            {
                var prefijo = $"{DateTime.Today:yyyy-MM-dd} Recuento importado_";

                var ultimoNumero = regularizacion.Vinculados<ArchivoDtm>(contexto)
                    .Select(a => Path.GetFileNameWithoutExtension(a.Nombre))
                    .Where(nombre => nombre.StartsWith(prefijo, StringComparison.OrdinalIgnoreCase))
                    .Select(nombre => int.TryParse(nombre.Substring(prefijo.Length), out var numero) ? numero : 0)
                    .DefaultIfEmpty(0)
                    .Max();

                archivo.Renombrar(contexto, $"{prefijo}{ultimoNumero + 1}{Path.GetExtension(archivo.Nombre)}");
                regularizacion.Vincular(contexto, archivo);
                contexto.Commit(tran);
            }
            catch (Exception e)
            {
                contexto.Rollback(tran);
                entorno.CrearTraza($"No se ha podido anexar el archivo '{archivo.Nombre}' a la regularización '{regularizacion.Referencia}': {e.Message}");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        // Lógica de importación: cada fila con error se descarta (con rollback) y se anota en el log del trabajo,
        // sin abortar el resto del fichero. Las líneas se numeran de incremento en incremento a continuación de las existentes
        //-------------------------------------------------------------------------------------------------------------
        public static ResumenDeImportacionDeCatalogo ImportarRecuentosInterno(EntornoDeTrabajo entorno, int idArchivo, int idRegularizacion)
        {
            var contexto = entorno.contextoDelProceso;
            var regularizacion = contexto.SeleccionarPorId<RegularizacionDtm>(idRegularizacion, usarLaCache: false);
            if (!regularizacion.EstaEnLaEtapa(enumEtapasDeRegularizacion.RAL_Recontando))
                GestorDeErrores.Emitir($"La regularización '{regularizacion.Referencia}' ha de estar {enumEtapasDeRegularizacion.RAL_Recontando.Nombre()} para importar un recuento");

            var archivo = contexto.SeleccionarPorId<ArchivoDtm>(idArchivo);
            var fichero = ApiDeArchivos.ObtenerRutaArchivo(archivo);
            var resumen = new ResumenDeImportacionDeCatalogo();

            var incremento = ExtensorDeRegularizaciones.IncrementarOrdenEn(contexto);
            var orden = contexto.Set<LineasDeUnaRegularizacionDtm>().Where(x => x.IdElemento == idRegularizacion).Max(x => (int?)x.Orden) ?? 0;

            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            using (var libro = new ExcelPackage(new FileInfo(fichero)))
            {
                var hoja = libro.Workbook.Worksheets.FirstOrDefault(h => h.Dimension != null);
                if (hoja == null)
                    GestorDeErrores.Emitir("El fichero no contiene ninguna hoja con datos");

                var columnas = LocalizarColumnasDelRecuento(hoja, out int filaCabecera);

                for (int fila = filaCabecera + 1; fila <= hoja.Dimension.End.Row; fila++)
                {
                    var referencia = TrabajosParaMaestros.Texto(hoja, fila, columnas["Referencia"]);
                    var textoDeCantidad = TrabajosParaMaestros.Texto(hoja, fila, columnas["Cantidad"]);
                    var textoDePrecio = TrabajosParaMaestros.Texto(hoja, fila, columnas["Precio"]);
                    if (referencia.IsNullOrEmpty() && textoDeCantidad.IsNullOrEmpty() && textoDePrecio.IsNullOrEmpty())
                        continue; // fila en blanco, no cuenta como fila de datos

                    resumen.TotalFilas++;
                    var tran = contexto.IniciarTransaccion();
                    try
                    {
                        if (referencia.IsNullOrEmpty())
                            GestorDeErrores.Emitir("no se indica la referencia del unitario");

                        var unitario = contexto.Set<UnitarioDtm>().FirstOrDefault(u => u.Referencia == referencia);
                        if (unitario == null)
                            GestorDeErrores.Emitir($"no existe ningún unitario con la referencia '{referencia}'");

                        var clase = contexto.SeleccionarPorId<NaturalezaDtm>(unitario.IdNaturaleza).Clase;
                        if (clase != enumClaseUnitario.Material)
                            GestorDeErrores.Emitir($"el unitario '{referencia}' no es un material, es de la clase '{clase}'");

                        if (contexto.Set<LineasDeUnaRegularizacionDtm>().Any(x => x.IdElemento == idRegularizacion && x.IdUnitario == unitario.Id))
                            GestorDeErrores.Emitir($"el material '{referencia}' ya está en el recuento de la regularización '{regularizacion.Referencia}'");

                        if (textoDeCantidad.IsNullOrEmpty())
                            GestorDeErrores.Emitir($"no se indica la cantidad del material '{referencia}'");
                        if (!TrabajosParaMaestros.TryDecimal(hoja, fila, columnas["Cantidad"], out var cantidad))
                            GestorDeErrores.Emitir($"la cantidad '{textoDeCantidad}' del material '{referencia}' no es un número");
                        if (cantidad < 0)
                            GestorDeErrores.Emitir($"la cantidad del material '{referencia}' no puede ser negativa");

                        if (textoDePrecio.IsNullOrEmpty())
                            GestorDeErrores.Emitir($"no se indica el precio del material '{referencia}'");
                        if (!TrabajosParaMaestros.TryDecimal(hoja, fila, columnas["Precio"], out var precio))
                            GestorDeErrores.Emitir($"el precio '{textoDePrecio}' del material '{referencia}' no es un número");
                        if (precio < 0)
                            GestorDeErrores.Emitir($"el precio del material '{referencia}' no puede ser negativo");

                        new LineasDeUnaRegularizacionDtm
                        {
                            IdElemento = idRegularizacion,
                            Orden = orden + incremento,
                            IdUnitario = unitario.Id,
                            Cantidad = cantidad,
                            Precio = precio
                        }.Insertar(contexto);

                        contexto.Commit(tran);
                        orden += incremento;
                        resumen.Creados++;
                    }
                    catch (Exception e)
                    {
                        contexto.Rollback(tran);
                        resumen.Descartados++;
                        entorno.AnotarError($"Fila {fila}", e);
                    }
                }
            }

            return resumen;
        }

        //-------------------------------------------------------------------------------------------------------------
        // Localización de la cabecera y de las columnas por nombre (no por posición). Nombre y Total son opcionales
        //-------------------------------------------------------------------------------------------------------------
        private static Dictionary<string, int> LocalizarColumnasDelRecuento(ExcelWorksheet hoja, out int filaCabecera)
        {
            var clavesBuscadas = new[]
            {
                ("Referencia", "referencia"),
                ("Cantidad", "cantidad"),
                ("Precio", "precio"),
                ("Nombre", "nombre"),
                ("Total", "total")
            };
            var obligatorias = new[] { "Referencia", "Cantidad", "Precio" };

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

                if (columnas.ContainsKey("Referencia") && columnas.ContainsKey("Cantidad"))
                {
                    var faltantes = obligatorias.Where(o => !columnas.ContainsKey(o)).ToList();
                    if (faltantes.Count > 0)
                        GestorDeErrores.Emitir($"No se han encontrado en la cabecera del recuento las columnas: {string.Join(", ", faltantes)}");

                    filaCabecera = fila;
                    return columnas;
                }
            }

            GestorDeErrores.Emitir("No se ha encontrado, en las primeras filas del fichero, una fila de cabecera con las columnas 'Referencia unitario', 'Cantidad' y 'Precio'");
            filaCabecera = 0;
            return null;
        }

        private static void ValidarQueSePuedeImportarUnRecuento(this RegularizacionDtm regularizacion, ContextoSe contexto)
        {
            if (!ApiDePermisos.LeerModoDeAcceso(contexto, enumNegocio.Regularizacion, regularizacion).HayPermisosDe(enumModoDeAccesoDeDatos.Gestor))
                GestorDeErrores.Emitir($"Ha de ser gestor de la regularización '{regularizacion.Referencia}' para importar un recuento");

            if (!regularizacion.EstaEnLaEtapa(enumEtapasDeRegularizacion.RAL_Recontando))
                GestorDeErrores.Emitir($"La regularización '{regularizacion.Referencia}' ha de estar {enumEtapasDeRegularizacion.RAL_Recontando.Nombre()} para importar un recuento");
        }

        //-------------------------------------------------------------------------------------------------------------
        // Preparar recuento. Sometimiento del job: genera el Excel del inventario de hoy del almacén para hacer un recuento
        //-------------------------------------------------------------------------------------------------------------
        public static TrabajoDeUsuarioDtm SometerPrepararRecuento(ContextoSe contexto, int idAlmacen, string referencia, string nombre)
        {
            var almacen = contexto.SeleccionarPorId<AlmacenDtm>(idAlmacen, usarLaCache: false);

            var dll = Assembly.GetExecutingAssembly().GetName().Name;
            var clase = typeof(TrabajosParaRegularizaciones).FullName;
            var ts = GestorDeTrabajosSometido.CrearObtener(contexto, enumTrabajosDeRegularizaciones.PrepararRecuento.Descripcion(), dll, clase, nameof(enumTrabajosDeRegularizaciones.PrepararRecuento), comunicarFin: true);

            var parametrosEntrada = new Dictionary<string, object> {
                { nameof(AlmacenDtm.Id), almacen.Id },
                { nameof(UnitarioDtm.Referencia), referencia ?? "" },
                { nameof(UnitarioDtm.Nombre), nombre ?? "" }
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
        public static void PrepararRecuento(EntornoDeTrabajo entorno)
        {
            var contexto = entorno.contextoDelProceso;
            Dictionary<string, object> parametros = entorno.TrabajoDeUsuario.Parametros.ToDiccionarioDeParametros();
            var idAlmacen = (int)parametros.LeerValor<long>(nameof(AlmacenDtm.Id));
            var referencia = parametros.LeerValor(nameof(UnitarioDtm.Referencia), "");
            var nombre = parametros.LeerValor(nameof(UnitarioDtm.Nombre), "");

            contexto.IniciarTraza(nameof(enumTrabajosDeRegularizaciones.PrepararRecuento));
            var otorgado = entorno.Ejecutor.OtorgarAdministrador(contexto);
            try
            {
                var almacen = contexto.SeleccionarPorId<AlmacenDtm>(idAlmacen, usarLaCache: false);
                var fichero = GenerarElExcelDelRecuento(entorno, contexto, almacen, referencia, nombre);

                // se anexa una copia al almacén y el original se envía por correo a quien sometió el trabajo
                almacen.AnexarArchivo(contexto, fichero, copiar: true);
                var correo = contexto.SeleccionarPorId<UsuarioDtm>(entorno.TrabajoDeUsuario.IdEjecutor).eMail;
                GestorDeCorreos.CrearCorreoPara(contexto
                    , new List<string> { correo }
                    , $"Recuento del almacén {almacen.Referencia}"
                    , $"Se le adjunta el Excel con el inventario del almacén '{almacen.Expresion}' a fecha {DateTime.Today:dd/MM/yyyy} para realizar el recuento"
                    , new List<TipoDtoElmento>()
                    , new List<string> { fichero });

                entorno.AnotarElTrabajoEnElElemento(contexto, almacen);
                entorno.CrearTraza($"Recuento preparado: '{Path.GetFileName(fichero)}'");
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
        // Excel con el inventario de hoy del almacén: el último movimiento de cada material hasta el fin del día es el que tiene el
        // stock y el valor medio vigentes (misma consulta que GestorDeInventario). Sólo se pueden editar Cantidad y Precio
        //-------------------------------------------------------------------------------------------------------------
        private static string GenerarElExcelDelRecuento(EntornoDeTrabajo entorno, ContextoSe contexto, AlmacenDtm almacen, string referencia, string nombre)
        {
            var hasta = DateTime.Today.AddDays(1);
            var movimientos = contexto.Set<MovimientoDeAlmacenDtm>();
            var consulta = movimientos.Where(m => m.IdAlmacen == almacen.Id
                && m.RealizadoEl < hasta
                && m.TipoMovimiento.ClaseMovimiento != enumClaseDeMovimiento.Nulo
                && !movimientos.Any(p => p.IdAlmacen == m.IdAlmacen
                                      && p.IdUnitario == m.IdUnitario
                                      && p.RealizadoEl < hasta
                                      && p.TipoMovimiento.ClaseMovimiento != enumClaseDeMovimiento.Nulo
                                      && (p.RealizadoEl > m.RealizadoEl || (p.RealizadoEl == m.RealizadoEl && p.Id > m.Id))));

            // misma sintaxis que los filtros de las pantallas (';', '%', '_', '=' y '|')
            consulta = consulta
                .FiltrarPorTexto($"{nameof(MovimientoDeAlmacenDtm.Unitario)}.{nameof(UnitarioDtm.Referencia)}", referencia)
                .FiltrarPorTexto($"{nameof(MovimientoDeAlmacenDtm.Unitario)}.{nameof(UnitarioDtm.Nombre)}", nombre);

            var lineas = consulta
                .Select(m => new { m.Unitario.Referencia, m.Unitario.Nombre, m.Unitario.IdUnidad, m.Stock, m.Valor })
                .OrderBy(x => x.Referencia)
                .ToList();

            if (lineas.Count == 0)
                GestorDeErrores.Emitir($"No hay materiales en el inventario del almacén '{almacen.Expresion}' con los filtros indicados");

            entorno.CrearTraza($"Materiales en el recuento: {lineas.Count}");

            var siglasDeUnidades = contexto.Set<UnidadDtm>().ToDictionary(u => u.Id, u => u.Sigla);
            var filas = lineas.Select(l => new List<ValorDeCelda> {
                new ValorDeCelda { Valor = l.Referencia, Bloqueada = true },
                new ValorDeCelda { Valor = l.Nombre, Bloqueada = true },
                new ValorDeCelda { Valor = siglasDeUnidades.GetValueOrDefault(l.IdUnidad), Bloqueada = true },
                new ValorDeCelda { Valor = l.Stock, Bloqueada = true },
                new ValorDeCelda { Valor = l.Valor, Bloqueada = true },
                new ValorDeCelda { Valor = null, Bloqueada = false },
                new ValorDeCelda { Valor = l.Valor, Bloqueada = false }
            }).ToList();

            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            using var libro = new ExcelPackage();
            libro.DefinirEstilos();

            var hoja = libro.Workbook.Worksheets.Add(ltrDeUnRecuento.Fichero);
            var ultimaFila = PrimeraFilaDeDatosDelRecuento + filas.Count - 1;
            hoja
            .Informe($"A{FilaDeTituloDelRecuento}:G{FilaDeTituloDelRecuento}", $"Recuento del almacén {almacen.Expresion}: {DateTime.Today:yyyy-MM-dd}")
            .Encolumnado("A", FilaDeEncolumnadoDelRecuento, $"Referencia{Simbolos.separadorDeValores}Nombre{Simbolos.separadorDeValores}Unidad{Simbolos.separadorDeValores}Stock{Simbolos.separadorDeValores}Valor medio{Simbolos.separadorDeValores}Cantidad{Simbolos.separadorDeValores}Precio")
            .Tabla("A", PrimeraFilaDeDatosDelRecuento, filas)
            .Cells.AutoFitColumns();

            foreach (var columna in new[] { ColumnaDeCantidadDelRecuento, ColumnaDePrecioDelRecuento })
            {
                var celdas = hoja.Cells[PrimeraFilaDeDatosDelRecuento, columna, ultimaFila, columna];
                celdas.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                celdas.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightYellow);
                hoja.Column(columna).Width = Math.Max(hoja.Column(columna).Width, AnchoMinimoDeLasEditables);
            }
            hoja.Cells[$"D{PrimeraFilaDeDatosDelRecuento}:G{ultimaFila}"].Style.Numberformat.Format = "#,##0.00####";
            hoja.Proteger();

            Directory.CreateDirectory(GestorDeVariables.RutaDeExportaciones);
            var fichero = Path.Combine(GestorDeVariables.RutaDeExportaciones, $"{ltrDeUnRecuento.Fichero} {DateTime.Today:yyyy-MM-dd}.xlsx");
            File.WriteAllBytes(fichero, libro.GetAsByteArray());
            return fichero;
        }
    }

    public static class ltrDeUnRecuento
    {
        public const string Fichero = "Recuento";
    }
}
