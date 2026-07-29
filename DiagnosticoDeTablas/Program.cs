using System.Reflection;
using ModeloDeDto;

namespace DiagnosticoDeTablas
{
    // Reproduce, solo con reflection sobre los atributos IUPropiedad, el mismo algoritmo de
    // colocación que usa DescriptorDeTabla.AnadirControl (en SistemaDeElementos) para poder
    // detectar, sin tener que levantar la aplicación, en qué Dto el orden visual resultante
    // (por clave física, que es como itera RenderDto) no coincide con el orden que cabría
    // esperar según la propiedad Posicion declarada en el atributo.
    internal static class Program
    {
        private class ControlEncontrado
        {
            public PropertyInfo Propiedad;
            public short PosicionDeclarada;
            public short PosicionFisica;
            public int OrdenDeProceso; // orden en el que reflection devolvió la propiedad
        }

        private static void Main()
        {
            var ensamblado = typeof(IUPropiedadAttribute).Assembly;

            List<Type> tipos;
            try
            {
                tipos = ensamblado.GetTypes().ToList();
            }
            catch (ReflectionTypeLoadException ex)
            {
                tipos = ex.Types.Where(t => t != null).ToList();
            }

            var tiposDto = tipos
                .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("Dto"))
                .OrderBy(t => t.FullName)
                .ToList();

            Console.WriteLine($"Analizando {tiposDto.Count} Dto...");
            Console.WriteLine();

            var graves = new List<string>();
            var informativas = new List<string>();

            foreach (var tipo in tiposDto)
            {
                AnalizarTipo(tipo, graves, informativas);
            }

            Console.WriteLine("==================== ORDEN VISUAL INCORRECTO (bug real) ====================");
            Console.WriteLine("El orden en que se pintan los controles NO coincide con su Posicion declarada.");
            Console.WriteLine();
            if (graves.Count == 0)
                Console.WriteLine("Ninguna.");
            else
                foreach (var g in graves) Console.WriteLine(g);

            Console.WriteLine();
            Console.WriteLine("================ COLISIONES DE POSICION (solo informativo) =================");
            Console.WriteLine("Varias propiedades comparten el mismo valor de Posicion en la misma celda.");
            Console.WriteLine("El orden resultante coincide con el declarado (se apilan por orden de reflection),");
            Console.WriteLine("pero es frágil: si el orden de reflection cambia (p.ej. al mover una propiedad");
            Console.WriteLine("a una clase base/derivada) podría dejar de coincidir.");
            Console.WriteLine();
            if (informativas.Count == 0)
                Console.WriteLine("Ninguna.");
            else
                foreach (var i in informativas) Console.WriteLine(i);

            Console.WriteLine();
            Console.WriteLine($"Total: {graves.Count} incidencias graves, {informativas.Count} colisiones informativas.");
        }

        private static void AnalizarTipo(Type tipo, List<string> graves, List<string> informativas)
        {
            var propiedades = tipo.GetProperties();

            // Agrupamos por (Fila, Columna), igual que hace DescriptorDeTabla, y dentro de
            // cada grupo simulamos la colocación con el mismo orden con el que reflection
            // devuelve las propiedades (derivadas antes que las de la clase base).
            var columnas = new Dictionary<(short fila, short columna), Dictionary<short, ControlEncontrado>>();

            var orden = 0;
            foreach (var propiedad in propiedades)
            {
                var atributo = propiedad.GetCustomAttribute<IUPropiedadAttribute>(inherit: false);
                if (atributo == null)
                    continue;

                // Los campos ocultos/no visibles en ningún modo (auditoría, claves internas,
                // banderas de control, etc.) no llegan a pintarse, así que su posición es
                // irrelevante y sólo generarían ruido en el diagnóstico.
                var potencialmenteVisible = !atributo.Oculto &&
                    (atributo.VisibleAlCrear || atributo.VisibleAlEditar || atributo.VisibleAlConsultar);
                if (!potencialmenteVisible)
                    continue;

                var clave = (atributo.Fila, atributo.Columna);
                if (!columnas.TryGetValue(clave, out var controles))
                {
                    controles = new Dictionary<short, ControlEncontrado>();
                    columnas[clave] = controles;
                }

                AnadirControl(controles, atributo.Posicion, new ControlEncontrado
                {
                    Propiedad = propiedad,
                    PosicionDeclarada = atributo.Posicion,
                    OrdenDeProceso = orden++
                });
            }

            foreach (var (clave, controles) in columnas)
            {
                if (controles.Count < 2)
                    continue;

                // Orden físico: el que realmente pinta RenderDto (itera clave ascendente).
                var ordenFisico = controles.OrderBy(c => c.Key).Select(c => c.Value).ToList();

                // Orden lógico: lo que cabría esperar según Posicion declarada (estable,
                // desempatando por el orden en que se procesó la propiedad).
                var ordenLogico = ordenFisico
                    .OrderBy(c => c.PosicionDeclarada)
                    .ThenBy(c => c.OrdenDeProceso)
                    .ToList();

                var hayColision = controles.Values
                    .GroupBy(c => c.PosicionDeclarada)
                    .Any(g => g.Count() > 1);

                var difierenLosOrdenes = !ordenFisico.SequenceEqual(ordenLogico, PropiedadComparer.Instance);

                if (!hayColision && !difierenLosOrdenes)
                    continue;

                var texto = new System.Text.StringBuilder();
                texto.AppendLine($"[{tipo.FullName}] Fila={clave.fila} Columna={clave.columna}");

                if (difierenLosOrdenes)
                    texto.AppendLine("  -> El orden visual (clave física) NO coincide con el orden esperado por Posicion:");
                else
                    texto.AppendLine("  -> Colisión sin efecto visible (el orden coincide por casualidad del orden de reflection):");

                texto.AppendLine("     Propiedad".PadRight(30) + "Posicion declarada".PadRight(20) + "Posicion física resultante");
                foreach (var c in ordenFisico)
                {
                    texto.AppendLine($"     {c.Propiedad.Name}".PadRight(30) + $"{c.PosicionDeclarada}".PadRight(20) + $"{c.PosicionFisica}");
                }

                if (difierenLosOrdenes)
                    graves.Add(texto.ToString());
                else
                    informativas.Add(texto.ToString());
            }
        }

        private static void AnadirControl(Dictionary<short, ControlEncontrado> controles, short pos, ControlEncontrado control)
        {
            if (!controles.ContainsKey(pos))
            {
                control.PosicionFisica = pos;
                controles[pos] = control;
            }
            else
                AnadirControl(controles, (short)(pos + 1), control);
        }

        private class PropiedadComparer : IEqualityComparer<ControlEncontrado>
        {
            public static readonly PropiedadComparer Instance = new();

            public bool Equals(ControlEncontrado x, ControlEncontrado y) => x.Propiedad == y.Propiedad;
            public int GetHashCode(ControlEncontrado obj) => obj.Propiedad.GetHashCode();
        }
    }
}
