using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using Utilidades;
using ValidacionesBase;

namespace PruebasDeIntegracion
{
    // Menú de un ámbito funcional, en dos niveles: primero el fichero (la clase de pruebas) y luego cada una de sus pruebas, o todas.
    // Las pruebas son los métodos marcados con [Prueba]: una prueba es correcta si termina sin excepciones; Validar.Fallo lanza
    // FalloDeValidacion, de la que sólo se muestra el mensaje, y cualquier otra excepción se muestra con su pila
    public class MenuDeTests
    {
        private readonly string _titulo;
        private readonly Type[] _clases;

        public MenuDeTests(string titulo, params Type[] clases)
        {
            _titulo = titulo;
            _clases = clases;
        }

        private static List<MethodInfo> Pruebas(Type clase) =>
            clase.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                 .Where(m => m.GetCustomAttribute<PruebaAttribute>() is not null)
                 .OrderBy(m => m.MetadataToken)
                 .ToList();

        public void Mostrar()
        {
            while (true)
            {
                Console.WriteLine();
                Console.WriteLine($"=== {_titulo} ===");
                for (var n = 0; n < _clases.Length; n++)
                {
                    var cuantas = Pruebas(_clases[n]).Count;
                    Console.WriteLine($"{n + 1}. {_clases[n].Name} ({cuantas} prueba{(cuantas == 1 ? "" : "s")})");
                }
                Console.WriteLine("0. Volver");
                Console.Write("Fichero: ");

                var opcion = Console.ReadLine()?.Trim();
                if (opcion is null || opcion == "0")
                    return;
                if (int.TryParse(opcion, out var elegida) && elegida >= 1 && elegida <= _clases.Length)
                    MostrarClase(_clases[elegida - 1]);
                else
                    Console.WriteLine("Opción no válida.");
            }
        }

        private static void MostrarClase(Type clase)
        {
            var pruebas = Pruebas(clase);
            while (true)
            {
                Console.WriteLine();
                Console.WriteLine($"--- {clase.Name} ---");
                for (var n = 0; n < pruebas.Count; n++)
                    Console.WriteLine($"{n + 1}. {pruebas[n].Name}");
                Console.WriteLine("T. Todas");
                Console.WriteLine("0. Volver");
                Console.Write("Prueba: ");

                var opcion = Console.ReadLine()?.Trim();
                if (opcion is null || opcion == "0")
                    return;
                if (opcion.Equals("T", StringComparison.OrdinalIgnoreCase))
                {
                    var fallidas = pruebas.Count(p => !Ejecutar(clase, p));
                    Console.WriteLine();
                    Escribir(fallidas == 0 ? ConsoleColor.Green : ConsoleColor.Red, $"{clase.Name}: {pruebas.Count - fallidas} correctas, {fallidas} con error");
                }
                else if (int.TryParse(opcion, out var elegida) && elegida >= 1 && elegida <= pruebas.Count)
                    Ejecutar(clase, pruebas[elegida - 1]);
                else
                    Console.WriteLine("Opción no válida.");
            }
        }

        // Cada prueba con un contexto nuevo; las que usan EjecutarConRollback deshacen lo que hacen
        private static bool Ejecutar(Type clase, MethodInfo prueba)
        {
            Console.WriteLine();
            Console.WriteLine($"> {clase.Name}.{prueba.Name}");
            var reloj = Stopwatch.StartNew();
            string error = null;
            try
            {
                Inicializaciones.NuevoAmbito();
                var instancia = Activator.CreateInstance(clase, nonPublic: true);
                try
                {
                    prueba.Invoke(instancia, null);
                }
                catch (TargetInvocationException e) when (e.InnerException is not null)
                {
                    ExceptionDispatchInfo.Capture(e.InnerException).Throw();
                }
            }
            catch (FalloDeValidacion e) { error = e.Message; }
            catch (Exception e)
            {
                while (e.InnerException is not null && e.Message.Contains("inner exception"))
                    e = e.InnerException;
                error = $"{e.Message}{Environment.NewLine}{e.StackTrace}";
            }
            finally
            {
                ServicioDeCaches.EliminarTodas();
            }

            reloj.Stop();
            if (error is null)
                Escribir(ConsoleColor.Green, $"  correcta ({reloj.Elapsed.TotalSeconds:0.0} s)");
            else
                Escribir(ConsoleColor.Red, $"  ERROR ({reloj.Elapsed.TotalSeconds:0.0} s): {error}");
            return error is null;
        }

        private static void Escribir(ConsoleColor color, string texto)
        {
            var anterior = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(texto);
            Console.ForegroundColor = anterior;
        }
    }
}
