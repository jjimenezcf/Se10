using System;
using iText.SigningExamples.Simple;
using PruebasDeIntegracion;
using TestLogistica;
using Utilidades;
using ValidacionesBase;
using ValidacionesDeRn;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.SetOut(new ConsolaSinLogDeEf(Console.Out));
Console.WriteLine("=== TestSe ===");
Console.WriteLine("Las pruebas se ejecutan en una transacción que se deshace al terminar: la BD queda como estaba.");

var config = Config.CargarOCrear();

// los pdfs y ficheros que se suben al servidor documental se generan antes en wwwroot/archivos del directorio actual (enumRutas.RutaDeDescarga)
System.IO.Directory.CreateDirectory(enumRutas.RutaDeDescarga);

// sólo contra la BD permitida: la cadena de conexión es la de appsettings.json de la aplicación web
var (servidor, baseDeDatos) = Inicializaciones.Conexion();
Console.WriteLine($"Base de datos: {baseDeDatos} en {servidor}");
if (!string.Equals(baseDeDatos, config.BaseDeDatosPermitida, StringComparison.OrdinalIgnoreCase))
{
    Console.WriteLine($"Sólo se permite ejecutar contra '{config.BaseDeDatosPermitida}' (ver config.json). Pulsa una tecla para salir.");
    Console.ReadKey();
    return;
}

// sin caché entre pruebas: lo leído dentro de una prueba se deshace con el rollback
ServicioDeCaches.UsaCache = false;

// los ámbitos funcionales: cada uno con las clases de pruebas de su carpeta (Logística, con su propio menú de procesos)
var ambitos = new (string Nombre, Action Mostrar)[]
{
    ("Administración", () => new MenuDeTests("Administración",
        typeof(TestConArchivadores),
        typeof(TestConObras),
        typeof(TestConPlantillas),
        typeof(TestConTareas),
        typeof(TestConRegistrosEs),
        typeof(TestProcesoCGI),
        typeof(TestProcesoFCT),
        typeof(TestProcesoIRPF),
        typeof(TestProcesoPIM),
        typeof(TestProcesoSprin)).Mostrar()),

    ("Gastos", () => new MenuDeTests("Gastos",
        typeof(TestConRemesasPag)).Mostrar()),

    ("Guarderías", () => new MenuDeTests("Guarderías",
        typeof(TestDeGuarderias)).Mostrar()),

    ("Jurídica", () => new MenuDeTests("Jurídica",
        typeof(TestConContratos),
        typeof(TestConPleitos),
        typeof(TestConSc)).Mostrar()),

    ("Logística", () => MenuDeLogistica.Mostrar(config.Logistica)),

    ("Maestros", () => new MenuDeTests("Maestros",
        typeof(TestConMaestros),
        typeof(TestConTerceros),
        typeof(TestInicializarBd),
        typeof(ConfiguracionTest),
        typeof(ConfiguracionAcromur),
        typeof(ConfiguracionCaifantasia),
        typeof(ConfiguracionCeom),
        typeof(ConfiguracionFemdek),
        typeof(ConfiguracionInteriorismo),
        typeof(TestSignBCSimple)).Mostrar()),

    ("Ventas", () => new MenuDeTests("Ventas",
        typeof(TestConFacturasEmt),
        typeof(TestConRemesasFae),
        typeof(TestConPlfsDeVenta)).Mostrar())
};

while (true)
{
    Console.WriteLine();
    Console.WriteLine("=== Ámbitos funcionales ===");
    for (var n = 0; n < ambitos.Length; n++)
        Console.WriteLine($"{n + 1}. {ambitos[n].Nombre}");
    Console.WriteLine("0. Salir");
    Console.Write("Ámbito: ");

    var opcion = Console.ReadLine()?.Trim();
    if (opcion is null || opcion == "0")
        return;
    if (int.TryParse(opcion, out var elegido) && elegido >= 1 && elegido <= ambitos.Length)
        ambitos[elegido - 1].Mostrar();
    else
        Console.WriteLine("Opción no válida.");
}
