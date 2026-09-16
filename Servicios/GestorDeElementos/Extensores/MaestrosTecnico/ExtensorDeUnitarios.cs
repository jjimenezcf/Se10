using ServicioDeDatos;
using ServicioDeDatos.MaestrosTecnico;

namespace GestorDeElementos.Extensores
{
    public static class ExtensorDeUnitarios
    {
        public static NaturalezaDtm Naturaleza(this UnitarioDtm unitario, ContextoSe contexto, bool aplicarJoin = false)
        {
            if (unitario.Naturaleza != null && unitario.Naturaleza.Id == unitario.IdNaturaleza)
                return unitario.Naturaleza;

            return unitario.Naturaleza = contexto.SeleccionarPorId<NaturalezaDtm>(unitario.IdNaturaleza, aplicarJoin: aplicarJoin);
        }
    }
}
