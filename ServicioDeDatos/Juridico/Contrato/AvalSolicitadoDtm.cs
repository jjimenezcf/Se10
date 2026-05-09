using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.SqlServer.Dac.Model;
using ServicioDeDatos.Elemento;
using Utilidades;

namespace ServicioDeDatos.Juridico
{

    [Table(Tablas.CONTRATO + "_" + nameof(Sufijo.AVAL_SOLICITADO), Schema = Esquemas.JURIDICO)]
    public class AvalSolicitadoDtm : Ampliacion<ContratoDtm>
    {
        public override enumNegocio Negocio => enumNegocio.Contrato;
        //public new ContratoDtm Elemento;

        public decimal ImporteAval { get; set; }
        public int? MesesDeAval { get; set; }
        public bool? AvisoEnviado { get; set; }
    }

    public static partial class ModeloDeContrato
    {
        internal static void AvalSolicitadoDelContrato(ModelBuilder modelBuilder)
        {

            ModeloDeAmpliaciones.DefinirAmpliacion<ContratoDtm, AvalSolicitadoDtm>(modelBuilder, nameof(ContratoDtm.Aval));

            modelBuilder.Entity<AvalSolicitadoDtm>().Property(p => p.ImporteAval).HasColumnName(ICampos.IMPORTE).HasColumnType(IDominio.DECIMAL).IsRequired();
            modelBuilder.Entity<AvalSolicitadoDtm>().Property(p => p.MesesDeAval).HasColumnName(ICampos.MESES).HasColumnType(IDominio.INT).IsRequired(false);
            modelBuilder.Entity<AvalSolicitadoDtm>().Property(p => p.AvisoEnviado).HasColumnName(ICampos.AVISO).HasColumnType(IDominio.BIT).IsRequired(false).HasDefaultValue(false);
        }

        public static void InsertarAvalSolicitadoDelContrato(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
                insert into {Esquemas.JURIDICO}.{Tablas.CONTRATO}_{Sufijo.AVAL_SOLICITADO}
                  ({ICampos.ID_ELEMENTO}
                  ,{ICampos.IMPORTE}
                  ,{ICampos.MESES}
                  ,{ICampos.AVISO}) 
                select {ICampos.ID},0,Null,Null 
                from {Esquemas.JURIDICO}.{Tablas.CONTRATO}
                go
                ");
        }
    }
}
