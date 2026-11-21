using System.IO;
using System.Text;

namespace PruebasDeIntegracion
{
    // El contexto escribe en la consola el log de EF (ContextoSe.OnConfiguring: LogTo(Console.WriteLine)): cada mensaje es una llamada a
    // WriteLine que empieza por el nivel ("dbug: ", "info: "...) y se descarta para que se vea el resultado de la prueba
    public class ConsolaSinLogDeEf : TextWriter
    {
        private static readonly string[] Niveles = { "trce: ", "dbug: ", "info: ", "warn: " };
        private readonly TextWriter _consola;

        public ConsolaSinLogDeEf(TextWriter consola) => _consola = consola;

        public override Encoding Encoding => _consola.Encoding;

        public override void Write(char value) => _consola.Write(value);
        public override void Write(string value) { if (!EsDeEf(value)) _consola.Write(value); }
        public override void WriteLine(string value) { if (!EsDeEf(value)) _consola.WriteLine(value); }
        public override void Flush() => _consola.Flush();

        private static bool EsDeEf(string texto)
        {
            if (texto is null) return false;
            foreach (var nivel in Niveles)
                if (texto.StartsWith(nivel)) return true;
            return false;
        }
    }
}
