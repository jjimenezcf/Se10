using System;
using PruebasDeIntegracion;
using Utilidades;
using ValidacionesBase;
using ValidacionesDeRn;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.SetOut(new ConsolaSinLogDeEf(Console.Out));
Console.WriteLine("=== TestGuarderias ===");
Console.WriteLine("Los tests que usan EjecutarConRollback deshacen lo que hacen: la BD queda como estaba.");

var config = Config.CargarOCrear();

// los ficheros que se suben al servidor documental se generan antes en wwwroot/archivos del directorio actual (enumRutas.RutaDeDescarga)
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

// sin caché entre tests: lo leído dentro de un test se deshace con el rollback
ServicioDeCaches.UsaCache = false;

new MenuDeTests("Guarderías",
    typeof(TestDeGuarderias)
).Mostrar();
