using GestorDeElementos;
using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using System.Linq;

namespace SistemaDeElementos.Inicializador.Datos
{
    // Cuentas del PGC que necesitan los almacenes y las obras (ver ExtensorDeCuentas): existencias de aprovisionamientos (30, 31 y 32), sus compras
    // (600, 601 y 602), devoluciones de compras (608x), variación de existencias (610, 611 y 612), albaranes pendientes de factura (4009) y partidas
    // pendientes de aplicación (555). La variación es la de existencias con el 61 delante (310000 -> 611000), como la deduce el almacén.
    // Sólo se crean las que no existen (ni por código ni por nombre): las ya creadas (a mano o al definir un almacén) no se modifican
    public static class InzContable
    {
        public static void InicializarCuentasDeAlmacenesYObras(ContextoSe contexto)
        {
            // existencias
            CrearCuentaSiNoExiste(contexto, ltrCuenta.MercaderiasCodigo, ltrCuenta.MercaderiasNombre);
            CrearCuentaSiNoExiste(contexto, ltrCuenta.MateriasPrimasCodigo, ltrCuenta.MateriasPrimasNombre);
            CrearCuentaSiNoExiste(contexto, ltrCuenta.OtrosAprovisionamientosCodigo, ltrCuenta.OtrosAprovisionamientosNombre);

            // compras
            CrearCuentaSiNoExiste(contexto, ltrCuenta.ComprasDeMercaderiasCodigo, ltrCuenta.ComprasDeMercaderiasNombre);
            CrearCuentaSiNoExiste(contexto, ltrCuenta.ComprasDeMateriasPrimasCodigo, ltrCuenta.ComprasDeMateriasPrimasNombre);
            CrearCuentaSiNoExiste(contexto, ltrCuenta.ComprasDeOtrosAprovisionamientosCodigo, ltrCuenta.ComprasDeOtrosAprovisionamientosNombre);

            // devoluciones de compras
            CrearCuentaSiNoExiste(contexto, ltrCuenta.DevolucionesDeMercaderiasCodigo, ltrCuenta.DevolucionesDeMercaderiasNombre);
            CrearCuentaSiNoExiste(contexto, ltrCuenta.DevolucionesDeMateriasPrimasCodigo, ltrCuenta.DevolucionesDeMateriasPrimasNombre);
            CrearCuentaSiNoExiste(contexto, ltrCuenta.DevolucionesDeOtrosAprovisionamientosCodigo, ltrCuenta.DevolucionesDeOtrosAprovisionamientosNombre);

            // variación de existencias: cuadre al cerrar (con variación) y regularizaciones (sin variación)
            CrearCuentaSiNoExiste(contexto, ltrCuenta.VariacionDeMercaderiasCodigo, ltrCuenta.VariacionDeMercaderiasNombre);
            CrearCuentaSiNoExiste(contexto, ltrCuenta.VariacionDeMateriasPrimasCodigo, ltrCuenta.VariacionDeMateriasPrimasNombre);
            CrearCuentaSiNoExiste(contexto, ltrCuenta.VariacionDeOtrosAprovisionamientosCodigo, ltrCuenta.VariacionDeOtrosAprovisionamientosNombre);

            // contrapartidas: recepción de albaranes sin factura y apuntes de gastos e ingresos de los expedientes
            CrearCuentaSiNoExiste(contexto, VariablesDeCuentas.Albaranes, ltrCuenta.AlbaranesNombre);
            CrearCuentaSiNoExiste(contexto, VariablesDeCuentas.PendientesDeAplicacion, ltrCuenta.PendientesDeAplicacionNombre);
        }

        // Sólo se crea si no hay ninguna cuenta con su código ni con su nombre (el nombre es único y la cuenta puede existir con otro código,
        // p.e. 'Cuenta de Iva repercutido' con el 477001): las existentes no se modifican
        public static void CrearCuentaSiNoExiste(ContextoSe contexto, string codigo, string nombre)
        {
            if (contexto.Set<CuentaDtm>().Any(c => c.Codigo == codigo || c.Nombre == nombre))
                return;

            new CuentaDtm { Codigo = codigo, Nombre = nombre }.PersistirPorAk(contexto, nameof(CuentaDtm.Codigo));
        }

        // Cuenta por su código y, si no la hay, por su nombre; error si no existe ninguna
        public static CuentaDtm LeerCuenta(ContextoSe contexto, string codigo, string nombre)
        {
            var cuenta = contexto.Set<CuentaDtm>().FirstOrDefault(c => c.Codigo == codigo)
                      ?? contexto.Set<CuentaDtm>().FirstOrDefault(c => c.Nombre == nombre);
            if (cuenta is null)
                Gestor.Errores.GestorDeErrores.Emitir($"No existe la cuenta '{codigo}' ni ninguna con el nombre '{nombre}'");
            return cuenta;
        }
    }
}
