using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using NUnit.Framework;
using Utilidades;
using ValidacionesBase;

namespace PruebasDeIntegracion
{
    // Menú de dos niveles: primero el fichero (la clase de tests) y luego cada uno de sus [Test], o todos.
    // Los tests son los de NUnit de ValidacionesDeReglasDeNegocio tal cual: se descubren por su atributo [Test] y se ejecutan aquí;
    // Assert.Pass lanza SuccessException (correcto) y Assert.Fail AssertionException (fallo)
    public class MenuDeTests
    {
        private readonly string _titulo;
        private readonly Type[] _clases;

        public MenuDeTests(string titulo, params Type[] clases)
        {
            _titulo = titulo;
            _clases = clases;
        }

        private static List<MethodInfo> Tests(Type clase) =>
            clase.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                 .Where(m => m.GetCustomAttribute<TestAttribute>() is not null)
                 .OrderBy(m => m.MetadataToken)
                 .ToList();

        public void Mostrar()
        {
            while (true)
            {
                Console.WriteLine();
                Console.WriteLine($"=== {_titulo} ===");
                for (var n = 0; n < _clases.Length; n++)
                    Console.WriteLine($"{n + 1}. {_clases[n].Name} ({Tests(_clases[n]).Count} test{(Tests(_clases[n]).Count == 1 ? "" : "s")})");
                Console.WriteLine("0. Salir");
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
            var tests = Tests(clase);
            while (true)
            {
                Console.WriteLine();
                Console.WriteLine($"--- {clase.Name} ---");
                for (var n = 0; n < tests.Count; n++)
                    Console.WriteLine($"{n + 1}. {tests[n].Name}");
                Console.WriteLine("T. Todos");
                Console.WriteLine("0. Volver");
                Console.Write("Test: ");

                var opcion = Console.ReadLine()?.Trim();
                if (opcion is null || opcion == "0")
                    return;
                if (opcion.Equals("T", StringComparison.OrdinalIgnoreCase))
                {
                    var fallidos = tests.Count(t => !Ejecutar(clase, t));
                    Console.WriteLine();
                    Escribir(fallidos == 0 ? ConsoleColor.Green : ConsoleColor.Red, $"{clase.Name}: {tests.Count - fallidos} correctos, {fallidos} con error");
                }
                else if (int.TryParse(opcion, out var elegido) && elegido >= 1 && elegido <= tests.Count)
                    Ejecutar(clase, tests[elegido - 1]);
                else
                    Console.WriteLine("Opción no válida.");
            }
        }

        // Cada test con un contexto nuevo; los que usan EjecutarConRollback deshacen lo que hacen
        private static bool Ejecutar(Type clase, MethodInfo test)
        {
            Console.WriteLine();
            Console.WriteLine($"> {clase.Name}.{test.Name}");
            var reloj = Stopwatch.StartNew();
            string error = null;
            try
            {
                Inicializaciones.NuevoAmbito();
                var instancia = Activator.CreateInstance(clase, nonPublic: true);
                try
                {
                    test.Invoke(instancia, null);
                }
                catch (TargetInvocationException e) when (e.InnerException is not null)
                {
                    ExceptionDispatchInfo.Capture(e.InnerException).Throw();
                }
            }
            catch (SuccessException) { }
            catch (IgnoreException e) { error = $"ignorado: {e.Message}"; }
            catch (AssertionException e) { error = e.Message; }
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
                Escribir(ConsoleColor.Green, $"  correcto ({reloj.Elapsed.TotalSeconds:0.0} s)");
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
