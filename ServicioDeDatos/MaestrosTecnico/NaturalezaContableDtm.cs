using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServicioDeDatos.MaestrosTecnico
{
    public class ltrSiglasDeNaturalezas
    {
        public const string MAT = nameof(MAT);
    }

    public class BiDelIvaPorNaturaleza
    {
        public int idIva { get; set; }
        public int IdNaturaleza { get; set; }
        public decimal Bi { get; set; }
        public decimal ImporteDeIva { get; set; }
        public string Concepto { get; set; }
    }

    [Table(Tablas.MT_NATURALEZA, Schema = Esquemas.MT)]
    public class NaturalezaDtm : RegistroConNombreDtm, IRegistroDeParametrizacion
    {
        public string Sigla { get; set; }
        public override string Expresion => $"({Sigla}) {Nombre}";

        public int? IdCuentaDeGasto { get; set; }
        public int? IdCuentaDeIngreso { get; set; }
        public CuentaDtm CuentaDeGasto { get; set; }
        public CuentaDtm CuentaDeIngreso { get; set; }
    }

    public static partial class ModeloDeMt
    {
        public static void NaturalezaContable(ModelBuilder modelBuilder)
        {
            ApiDeRegistroDtm.DefinirCampoIdDtm<NaturalezaDtm>(modelBuilder);
            ApiDeNombreDtm.DefinirCampoNombreDtm<NaturalezaDtm>(modelBuilder, unico: true);
            ApiDeNombreDtm.DefinirCampoDeSigla<NaturalezaDtm>(modelBuilder);
            ApiDeRegistroDtm.DefinirCampoFk<NaturalezaDtm>(modelBuilder, nameof(NaturalezaDtm.CuentaDeGasto), nameof(NaturalezaDtm.IdCuentaDeGasto), ICampos.ID_CUENTA_GASTO, requerida: false, unico: false);
            ApiDeRegistroDtm.DefinirCampoFk<NaturalezaDtm>(modelBuilder, nameof(NaturalezaDtm.CuentaDeIngreso), nameof(NaturalezaDtm.IdCuentaDeIngreso), ICampos.ID_CUENTA_INGRESO, requerida: false, unico: false);
        }
    }
}
