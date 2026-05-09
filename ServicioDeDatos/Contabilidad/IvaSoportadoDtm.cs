using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Utilidades;


namespace ServicioDeDatos.Contabilidad
{
    public enum enumClasesDeIvaSop
    {
        [Description("General")]
        ISG,
        [Description("Reducido")]
        ISR,
        [Description("Super reducido")]
        ISS,
        [Description("No sujeto")]
        NSJ,
        [Description("Inversión sujeto pasivo")]
        ISP
    }

    [Table(Tablas.IVA_SOPORTADO, Schema = Esquemas.CONTABILIDAD)]
    public class IvaSoportadoDtm : RegistroConNombreDtm, IRegistroDeParametrizacion
    {
        public enumClasesDeIvaSop Clase { get; set; }
        public override string Expresion => $"({Clase}) {Porcentaje.Porcentaje(alineacion:false)}";
        public string Detalle => $"({Porcentaje.Porcentaje(alineacion: false)}) {Nombre}";
        public bool Exento { get; set; }
        public decimal Porcentaje { get; set; }
        public int IdCuenta { get; set; }
        public CuentaDtm Cuenta { get; set; }
    }


    public static partial class ModeloContable
    {
        public static void IvasSoportados(ModelBuilder modelBuilder)
        {
            ApiDeRegistroDtm.DefinirCampoIdDtm<IvaSoportadoDtm>(modelBuilder);
            ApiDeNombreDtm.DefinirCampoNombreDtm<IvaSoportadoDtm>(modelBuilder, unico: true);

            modelBuilder.Entity<IvaSoportadoDtm>().Property(p => p.Clase).HasColumnName(ICampos.CLASE).HasColumnType($"{IDominio.VARCHAR_10})").IsRequired();
            modelBuilder.Entity<IvaSoportadoDtm>()
            .HasIndex(p => p.Clase)
            .HasDatabaseName($"I_{Tablas.IVA_SOPORTADO}_{ICampos.CLASE}").IsUnique(false);

            modelBuilder.Entity<IvaSoportadoDtm>().Property(nameof(IvaSoportadoDtm.Exento)).HasColumnName(ICampos.EXENTO).HasColumnType(IDominio.BIT).IsRequired(true);
            modelBuilder.Entity<IvaSoportadoDtm>().Property(nameof(IvaSoportadoDtm.Porcentaje)).HasColumnName(ICampos.PORCENTAJE).HasColumnType(IDominio.PORCENTAJE_MENOR_100).IsRequired(true);
            modelBuilder.Entity<IvaSoportadoDtm>().Ignore(p => p.Detalle);
            ApiDeRegistroDtm.DefinirCampoFk<IvaSoportadoDtm>(modelBuilder, nameof(IvaSoportadoDtm.Cuenta), nameof(IvaSoportadoDtm.IdCuenta), ICampos.ID_CUENTA, requerida: true, unico: false);
        }

    }
}
