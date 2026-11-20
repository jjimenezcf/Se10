using System;
using System.Collections.Generic;

namespace TestLogistica
{
    // Sólo se muestra el paso que se hace y, si algo falla, su mensaje de error; al final una línea con el resultado
    public class Informe
    {
        private readonly string _prueba;
        private readonly List<string> _fallos = new();
        private int _paso;

        public Informe(string prueba)
        {
            _prueba = prueba;
            Console.WriteLine();
            Console.WriteLine($"=== {prueba} ===");
        }

        public void Paso(string descripcion)
        {
            _paso++;
            Console.WriteLine($"{_paso}. {descripcion}");
        }

        // lo que se ha hecho en el paso: no se muestra (se deja para depurar)
        public void Dato(string texto) { }

        public bool Comprobar(bool condicion, string que, string detalleSiFalla = null)
        {
            if (condicion)
                return true;

            var fallo = detalleSiFalla is null ? que : $"{que}: {detalleSiFalla}";
            _fallos.Add($"paso {_paso}: {fallo}");
            Escribir(ConsoleColor.Red, $"   ERROR {fallo}");
            return false;
        }

        public void Excepcion(Exception e)
        {
            while (e.InnerException is not null && e.Message.Contains("inner exception"))
                e = e.InnerException;
            _fallos.Add($"paso {_paso}: {e.Message}");
            Escribir(ConsoleColor.Red, $"   ERROR {e.Message}");
        }

        public void Resumen()
        {
            Console.WriteLine();
            if (_fallos.Count == 0)
                Escribir(ConsoleColor.Green, $"{_prueba}: correcta");
            else
                Escribir(ConsoleColor.Red, $"{_prueba}: {_fallos.Count} error(es)");
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
