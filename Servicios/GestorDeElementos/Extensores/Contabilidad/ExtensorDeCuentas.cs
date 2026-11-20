using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using System.Linq;
using Utilidades;
using static Gestor.Errores.GestorDeErrores;

namespace GestorDeElementos.Extensores
{
    // Cuentas del PGC de los almacenes y de los materiales de las obras. Las existencias de un almacén son de aprovisionamientos: grupo 30
    // (mercaderías), 31 (materias primas) o 32 (otros aprovisionamientos); su segundo dígito decide el resto de cuentas del almacén:
    //   existencias 30x -> compras 600, variación 610
    //   existencias 31x -> compras 601, variación 611
    //   existencias 32x -> compras 602, variación 612
    // y las devoluciones de compras van a la 608 (6080, 6081 o 6082). Los grupos 33 a 39 son productos u obra en curso, su variación va a la
    // 71x: no son de un almacén de compras
    public static class ExtensorDeCuentas
    {
        public const string PrefijoDeDevoluciones = ltrCuenta.SubgrupoDeDevolucionesDeCompras;
        public const string PrefijoDeGastoDeMateriales = ltrCuenta.GrupoDeComprasYGastos;

        public static CuentaDtm LeerCuenta(ContextoSe contexto, int? idCuenta) =>
            idCuenta is null ? null : contexto.SeleccionarPorId<CuentaDtm>((int)idCuenta);

        // Segundo dígito de la cuenta de existencias (0, 1 o 2); error si no es de los grupos 30, 31 o 32
        public static char GrupoDeExistencias(this CuentaDtm existencias)
        {
            var codigo = existencias.Codigo ?? "";
            if (codigo.Length < 3 || codigo[0] != '3' || !"012".Contains(codigo[1]))
                Emitir($"La cuenta de existencias '{existencias.Expresion}' ha de ser del grupo 30 (mercaderías), 31 (materias primas) o 32 (otros aprovisionamientos)");
            return codigo[1];
        }

        public static string DescripcionDelGrupo(char grupo) => grupo switch
        {
            '0' => "mercaderías",
            '1' => "materias primas",
            _ => "otros aprovisionamientos"
        };

        public static string PrefijoDeVariacion(char grupo) => $"61{grupo}";
        public static string PrefijoDeCompras(char grupo) => $"60{grupo}";

        // La cuenta ha de empezar por el prefijo indicado; 'uso' es lo que es la cuenta, para el mensaje
        public static void ValidarPrefijo(this CuentaDtm cuenta, string prefijo, string uso)
        {
            if (!(cuenta.Codigo ?? "").StartsWith(prefijo))
                Emitir($"La {uso} '{cuenta.Expresion}' ha de empezar por '{prefijo}'");
        }

        // Variación de existencias deducida de la cuenta de existencias: 61 + su segundo dígito + lo que sigue a su tercer dígito, así cada
        // subcuenta de existencias tiene la suya (311000 -> 611000, 3100005 -> 6100005); se crea si no existe
        public static CuentaDtm DeducirCuentaDeVariacion(this CuentaDtm existencias, ContextoSe contexto)
        {
            var grupo = existencias.GrupoDeExistencias();
            var codigo = $"{PrefijoDeVariacion(grupo)}{existencias.Codigo.Substring(3)}";
            return ObtenerOCrearCuenta(contexto, codigo, $"Variación de existencias de {DescripcionDelGrupo(grupo)}");
        }

        // Cuenta con el código indicado; si no existe se crea en el plan de cuentas (el nombre es único: se le añade el código)
        public static CuentaDtm ObtenerOCrearCuenta(ContextoSe contexto, string codigo, string nombre)
        {
            var cuenta = contexto.Set<CuentaDtm>().FirstOrDefault(c => c.Codigo == codigo);
            if (cuenta is not null)
                return cuenta;

            return new CuentaDtm { Codigo = codigo, Nombre = $"{nombre} ({codigo})" }.InsertarComoAdministrador(contexto);
        }
    }
}
