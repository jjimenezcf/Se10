using System;
using TestLogistica;
using Utilidades;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.SetOut(new ConsolaSinLogDeEf(Console.Out));
Console.WriteLine("=== TestLogistica ===");
Console.WriteLine("Cada prueba se ejecuta en una transacción que se deshace al terminar: la BD queda como estaba.");

var config = Config.CargarOCrear();

// los pdfs y ficheros que se suben al servidor documental se generan antes en wwwroot/archivos del directorio actual (enumRutas.RutaDeDescarga)
System.IO.Directory.CreateDirectory(enumRutas.RutaDeDescarga);

// sólo contra la BD permitida: la cadena de conexión es la de appsettings.json de la aplicación web
var (servidor, baseDeDatos) = Entorno.Conexion();
Console.WriteLine($"Base de datos: {baseDeDatos} en {servidor}");
if (!string.Equals(baseDeDatos, config.BaseDeDatosPermitida, StringComparison.OrdinalIgnoreCase))
{
    Console.WriteLine($"Sólo se permite ejecutar contra '{config.BaseDeDatosPermitida}' (ver config.json). Pulsa una tecla para salir.");
    Console.ReadKey();
    return;
}

// sin caché entre pruebas: lo leído dentro de una prueba se deshace con el rollback
ServicioDeCaches.UsaCache = false;

var pruebas = new (string Nombre, Action<ServicioDeDatos.ContextoSe, Informe> Ejecutar)[]
{
    (new PruebaDeCompras(config, conVariacion: true).Nombre, (c, i) => new PruebaDeCompras(config, conVariacion: true).Ejecutar(c, i)),
    (new PruebaDeCompras(config, conVariacion: false).Nombre, (c, i) => new PruebaDeCompras(config, conVariacion: false).Ejecutar(c, i)),
    (new PruebaDeCierre(config, conVariacion: true).Nombre, (c, i) => new PruebaDeCierre(config, conVariacion: true).Ejecutar(c, i)),
    (new PruebaDeCierre(config, conVariacion: false).Nombre, (c, i) => new PruebaDeCierre(config, conVariacion: false).Ejecutar(c, i))
};

while (true)
{
    Console.WriteLine();
    for (var n = 0; n < pruebas.Length; n++)
        Console.WriteLine($"{n + 1}. {pruebas[n].Nombre} (sociedad {config.IdSociedad}, almacén {config.IdAlmacen})");
    Console.WriteLine("0. Salir");
    Console.Write("Opción: ");

    var opcion = Console.ReadLine()?.Trim();
    if (int.TryParse(opcion, out var elegida) && elegida >= 1 && elegida <= pruebas.Length)
    {
        Entorno.EjecutarConRollback(pruebas[elegida - 1].Nombre, pruebas[elegida - 1].Ejecutar);
        continue;
    }

    if (opcion == "0")
        return;
    Console.WriteLine("Opción no válida.");
}
