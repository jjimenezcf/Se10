using Microsoft.CodeAnalysis.CSharp.Syntax;
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

        public static UnitarioDtm Unitario(this IPuedeUsarUnitario linea, ContextoSe contexto, bool aplicarJoin = false)
        {
            if (linea.IdUnitario is null)
                return null;

            if (linea.Unitario != null && linea.Unitario.Id == linea.IdUnitario)
                return linea.Unitario;

            return linea.Unitario = contexto.SeleccionarPorId<UnitarioDtm>((int)linea.IdUnitario, aplicarJoin: aplicarJoin);
        }
    }
}
