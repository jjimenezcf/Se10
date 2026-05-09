using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using System.ComponentModel.DataAnnotations.Schema;
using Utilidades;

namespace ServicioDeDatos.Contabilidad
{
    public class ImportePorTipoDeIrpf
    {
        public int IdIrpf { get; set; }
        public string Tipo { get; set; }
        public decimal BI { get; set; }
        public decimal Porcentaje { get; set; }
        public decimal Importe { get; set; }
    }

    [Table(Tablas.IRPF, Schema = Esquemas.CONTABILIDAD)]
    public class IrpfDtm : RegistroConNombreDtm, IRegistroDeParametrizacion
    {
        public string Codigo { get; set; }
        public override string Expresion => $"({Codigo}) {Porcentaje.Porcentaje(alineacion: false)}";
        public string Detalle => $"({Porcentaje}) {Nombre}";
        public decimal Porcentaje { get; set; }
        public int IdCuenta { get; set; }
        public CuentaDtm Cuenta { get; set; }
    }


    public static partial class ModeloContable
    {
        public static void Irpfs(ModelBuilder modelBuilder)
        {
            ApiDeRegistroDtm.DefinirCampoIdDtm<IrpfDtm>(modelBuilder);
            ApiDeNombreDtm.DefinirCampoNombreDtm<IrpfDtm>(modelBuilder, unico: true);

            modelBuilder.Entity<IrpfDtm>().Property(p => p.Codigo).HasColumnName(ICampos.CODIGO).HasColumnType($"{IDominio.VARCHAR_10})").IsRequired();
            modelBuilder.Entity<IrpfDtm>()
            .HasIndex(p => p.Codigo)
            .HasDatabaseName($"I_{Tablas.IRPF}_{ICampos.CODIGO}").IsUnique(true);

            modelBuilder.Entity<IrpfDtm>().Property(nameof(IrpfDtm.Porcentaje)).HasColumnName(ICampos.PORCENTAJE).HasColumnType(IDominio.PORCENTAJE_MENOR_100).IsRequired(true);
            modelBuilder.Entity<IrpfDtm>().Ignore(p => p.Detalle);
            ApiDeRegistroDtm.DefinirCampoFk<IrpfDtm>(modelBuilder, nameof(IrpfDtm.Cuenta), nameof(IrpfDtm.IdCuenta), ICampos.ID_CUENTA, requerida: true, unico: false);
        }
    }
}
