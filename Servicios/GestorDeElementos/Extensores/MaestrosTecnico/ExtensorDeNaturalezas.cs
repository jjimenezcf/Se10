using Gestor.Errores;
using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.MaestrosTecnico;

namespace GestorDeElementos.Extensores
{
    public static class ExtensorDeNaturalezas
    {
        public static CuentaDtm CuentaDeGasto(this NaturalezaDtm naturaleza, ContextoSe contexto, bool errorSiNoHay = true)
        {
            if (naturaleza.CuentaDeGasto != null) return naturaleza.CuentaDeGasto;
            if (naturaleza.IdCuentaDeGasto is null && errorSiNoHay) GestorDeErrores.Emitir($"la naturaleza '{naturaleza.Nombre}' no tiene indicado la cuenta de gasto");
            if (naturaleza.IdCuentaDeGasto is null && !errorSiNoHay) return null;

            naturaleza.CuentaDeGasto = contexto.SeleccionarPorId<CuentaDtm>((int)naturaleza.IdCuentaDeGasto);

            return naturaleza.CuentaDeGasto;
        }

        public static NaturalezaDtm Naturaleza(this IPuedeUsarNaturaleza linea, ContextoSe contexto, bool aplicarJoin = false)
        {
            if (linea.IdNaturaleza is null)
                return null;

            if (linea.Naturaleza != null && linea.Naturaleza.Id == linea.IdNaturaleza)
                return linea.Naturaleza;

            return linea.Naturaleza = contexto.SeleccionarPorId<NaturalezaDtm>((int)linea.IdNaturaleza, aplicarJoin: aplicarJoin);
        }

    }
}
