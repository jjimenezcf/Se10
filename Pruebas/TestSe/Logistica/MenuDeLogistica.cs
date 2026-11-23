using System;
using ServicioDeDatos;

namespace TestLogistica
{
    // Las pruebas de logística son procesos completos con sus datos en config.json (sección Logistica) y su informe paso a paso:
    // cada una se ejecuta en una transacción que se deshace al terminar
    public static class MenuDeLogistica
    {
        public static void Mostrar(ConfigDeLogistica config)
        {
            var pruebas = new (string Nombre, Action<ContextoSe, Informe> Ejecutar)[]
            {
                (new PruebaDeCompras(config, conVariacion: true).Nombre, (c, i) => new PruebaDeCompras(config, conVariacion: true).Ejecutar(c, i)),
                (new PruebaDeCompras(config, conVariacion: false).Nombre, (c, i) => new PruebaDeCompras(config, conVariacion: false).Ejecutar(c, i)),
                (new PruebaDeCierre(config, conVariacion: true).Nombre, (c, i) => new PruebaDeCierre(config, conVariacion: true).Ejecutar(c, i)),
                (new PruebaDeCierre(config, conVariacion: false).Nombre, (c, i) => new PruebaDeCierre(config, conVariacion: false).Ejecutar(c, i)),
                (new PruebaDeVentas(config, conVariacion: true).Nombre, (c, i) => new PruebaDeVentas(config, conVariacion: true).Ejecutar(c, i)),
                (new PruebaDeVentas(config, conVariacion: false).Nombre, (c, i) => new PruebaDeVentas(config, conVariacion: false).Ejecutar(c, i)),
                (new PruebaDeDevolucionesDeVenta(config, conVariacion: true).Nombre, (c, i) => new PruebaDeDevolucionesDeVenta(config, conVariacion: true).Ejecutar(c, i)),
                (new PruebaDeDevolucionesDeVenta(config, conVariacion: false).Nombre, (c, i) => new PruebaDeDevolucionesDeVenta(config, conVariacion: false).Ejecutar(c, i)),
                (PruebaDeDevolucionDeFacturaEmitida.Nombre, (c, i) => new PruebaDeDevolucionDeFacturaEmitida().Ejecutar(c, i))
            };

            while (true)
            {
                Console.WriteLine();
                Console.WriteLine("=== Logística ===");
                for (var n = 0; n < pruebas.Length; n++)
                    Console.WriteLine($"{n + 1}. {pruebas[n].Nombre} (sociedad {config.IdSociedad}, almacén {config.IdAlmacen})");
                Console.WriteLine("0. Volver");
                Console.Write("Prueba: ");

                var opcion = Console.ReadLine()?.Trim();
                if (opcion is null || opcion == "0")
                    return;
                if (int.TryParse(opcion, out var elegida) && elegida >= 1 && elegida <= pruebas.Length)
                    Entorno.EjecutarConRollback(pruebas[elegida - 1].Nombre, pruebas[elegida - 1].Ejecutar);
                else
                    Console.WriteLine("Opción no válida.");
            }
        }
    }
}
