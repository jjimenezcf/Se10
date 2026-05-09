using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.SqlServer.Dac.Model;
using ServicioDeDatos.Elemento;
using Utilidades;

namespace ServicioDeDatos.Juridico
{

    public enum enumClaseDeProrroga
    {
        [Description("No prórrogable")]
        noProrrogable,
        [Description("Tácita")]
        tacita,
        [Description("Explícita")]
        explicita
    }

    [Table(Tablas.CONTRATO + "_" + nameof(Sufijo.PRORROGA), Schema = Esquemas.JURIDICO)]
    public class ProrrogaDtm : Ampliacion<ContratoDtm>
    {
        public override enumNegocio Negocio => enumNegocio.Contrato;
        //public new ContratoDtm Elemento;

        public enumClaseDeProrroga ClaseDeProrroga { get; set; }
        public int? Meses { get; set; }
        public DateTime? FechaUltimaProrroga { get; set; }

        public DateTime? FechaDeProximaProrroga(DateTime? fechaFinContrato)
        {
            if (Meses == default) return default(DateTime?);
            if (FechaUltimaProrroga == default) return default(DateTime?);
            if (fechaFinContrato == default) return default(DateTime?);
            return ((DateTime)fechaFinContrato).AddMonths((int)Meses);
        }
    }

    public static partial class ModeloDeContrato
    {
        internal static void DatosDeProrroga(ModelBuilder modelBuilder)
        {
            ModeloDeAmpliaciones.DefinirAmpliacion<ContratoDtm, ProrrogaDtm>(modelBuilder, nameof(ContratoDtm.Prorroga));

            modelBuilder.Entity<ProrrogaDtm>().Property(p => p.ClaseDeProrroga).HasColumnName(ICampos.CLASE_PRORROGA).HasColumnType(IDominio.VARCHAR_30).IsRequired();
            modelBuilder.Entity<ProrrogaDtm>().Property(p => p.Meses).HasColumnName(ICampos.MESES).HasColumnType(IDominio.INT).IsRequired(false);
            modelBuilder.Entity<ProrrogaDtm>().Property(p => p.FechaUltimaProrroga).HasColumnName(ICampos.ULTIMA).HasColumnType(IDominio.DATETIME_2).IsRequired(false);
        }

        public static void InsertarDatosDeProrrogas(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
                insert into {Esquemas.JURIDICO}.{Tablas.CONTRATO}_{Sufijo.PRORROGA}
                ({ICampos.ID_ELEMENTO},{ICampos.CLASE_PRORROGA},{ICampos.MESES},{ICampos.ULTIMA}) 
                select {ICampos.ID},'{enumClaseDeProrroga.noProrrogable}',null,null 
                from {Esquemas.JURIDICO}.{Tablas.CONTRATO}
                go
                ");
        }
    }
}
