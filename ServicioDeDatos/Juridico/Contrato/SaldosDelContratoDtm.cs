using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.SqlServer.Dac.Model;
using ServicioDeDatos.Elemento;
using Utilidades;

namespace ServicioDeDatos.Juridico
{

    [Table(Tablas.CONTRATO + "_" + nameof(Sufijo.SALDOS), Schema = Esquemas.JURIDICO)]
    public class SaldosDelContratoDtm : Ampliacion<ContratoDtm>
    {
        public override enumNegocio Negocio => enumNegocio.Contrato;
        //public new ContratoDtm Elemento;

        public decimal Importe { get; set; }
        public decimal Adendado { get; set; }
        public decimal Aviso { get; set; }
        public decimal Bloqueo { get; set; }
        public bool? Notificado { get; set; }

    }

    public static partial class ModeloDeContrato
    {
        internal static void SaldosDelContrato(ModelBuilder modelBuilder)
        {
            ModeloDeAmpliaciones.DefinirAmpliacion<ContratoDtm, SaldosDelContratoDtm>(modelBuilder, nameof(ContratoDtm.Saldos));

            modelBuilder.Entity<SaldosDelContratoDtm>().Property(p => p.Importe).HasColumnName(ICampos.IMPORTE).HasColumnType(IDominio.DECIMAL).IsRequired();
            modelBuilder.Entity<SaldosDelContratoDtm>().Property(p => p.Adendado).HasColumnName(ICampos.ADENDADO).HasColumnType(IDominio.DECIMAL).IsRequired();
            modelBuilder.Entity<SaldosDelContratoDtm>().Property(p => p.Aviso).HasColumnName(ICampos.AVISO).HasColumnType(IDominio.PORCENTAJE_MENOR_100).IsRequired();
            modelBuilder.Entity<SaldosDelContratoDtm>().Property(p => p.Bloqueo).HasColumnName(ICampos.BLOQUEO).HasColumnType(IDominio.PORCENTAJE_MENOR_1000).IsRequired();
            modelBuilder.Entity<SaldosDelContratoDtm>().Property(p => p.Notificado).HasColumnName(ICampos.NOTIFICADO).HasColumnType(IDominio.BIT).IsRequired(false).HasDefaultValue(false);
        }

        public static void InsertarSaldosDelContrato(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
                insert into {Esquemas.JURIDICO}.{Tablas.CONTRATO}_{Sufijo.SALDOS}
                ({ICampos.ID_ELEMENTO},{ICampos.IMPORTE},{ICampos.ADENDADO},{ICampos.AVISO},{ICampos.BLOQUEO}, {ICampos.NOTIFICADO}) 
                select {ICampos.ID},0,0,0,0,0 
                from {Esquemas.JURIDICO}.{Tablas.CONTRATO}
                go
                ");
        }
    }
}
