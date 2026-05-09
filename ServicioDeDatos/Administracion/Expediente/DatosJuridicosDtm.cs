using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Terceros;
using Utilidades;

namespace ServicioDeDatos.Expediente
{

    [Table(Tablas.EXPEDIENTE + "_" + Sufijo.DATOS_JURIDICOS, Schema = Esquemas.EXPEDIENTE)]
    public class DatosJuridicosDtm : Ampliacion<ExpedienteDtm>
    {
        public override enumNegocio Negocio => enumNegocio.Expediente;
        public new ExpedienteDtm Elemento;

        public int? IdProcurador { get; set; }
        public int? IdAbogado { get; set; }
        public int? IdJuzgado { get; set; }
        public ProcuradorDtm Procurador { get; set; }
        public AbogadoDtm Abogado { get; set; }
        public JuzgadoDtm Juzgado { get; set; }        


        public string NIG { get; set; }
        public string Procedimiento { get; set; }

        public decimal? Litigado { get; set; }
        public decimal? Costas { get; set; }
        public decimal? Sentenciado { get; set; }
        public DateTime? SentenciadoEl { get; set; }
    }


    public static partial class ModeloDeExpediente
    {
        internal static void DatosJuridicos(ModelBuilder modelBuilder)
        {
            ModeloDeAmpliaciones.DefinirAmpliacion<ExpedienteDtm, DatosJuridicosDtm>(modelBuilder, nameof(ExpedienteDtm.DatosJuridicos));

            ApiDeRegistroDtm.DefinirDependencia<DatosJuridicosDtm>(modelBuilder, nameof(DatosJuridicosDtm.Procurador), nameof(DatosJuridicosDtm.IdProcurador),  ICampos.ID_PROCURADOR, requerido: false, unico: false);
            ApiDeRegistroDtm.DefinirDependencia<DatosJuridicosDtm>(modelBuilder, nameof(DatosJuridicosDtm.Abogado), nameof(DatosJuridicosDtm.IdAbogado), ICampos.ID_ABOGADO, requerido: false, unico: false);
            ApiDeRegistroDtm.DefinirDependencia<DatosJuridicosDtm>(modelBuilder, nameof(DatosJuridicosDtm.Juzgado), nameof(DatosJuridicosDtm.IdJuzgado), ICampos.ID_JUZGADO, requerido: false, unico: false);

            modelBuilder.Entity<DatosJuridicosDtm>().Property(p => p.NIG).HasColumnName(ICampos.NIG).HasColumnType(IDominio.VARCHAR_25).IsRequired(false);
            modelBuilder.Entity<DatosJuridicosDtm>().Property(p => p.Procedimiento).HasColumnName(ICampos.REFERENCIA).HasColumnType(IDominio.VARCHAR_50).IsRequired(false);
            modelBuilder.Entity<DatosJuridicosDtm>().Property(p => p.Litigado).HasColumnName(ICampos.LITIGADO).HasColumnType(IDominio.DECIMAL).IsRequired(false);
            modelBuilder.Entity<DatosJuridicosDtm>().Property(p => p.Costas).HasColumnName(ICampos.COSTAS).HasColumnType(IDominio.DECIMAL).IsRequired(false);

            modelBuilder.Entity<DatosJuridicosDtm>().Property(p => p.Sentenciado).HasColumnName(ICampos.SENTENCIADO).HasColumnType(IDominio.DECIMAL).IsRequired(false);
            modelBuilder.Entity<DatosJuridicosDtm>().Property(p => p.SentenciadoEl).HasColumnName(ICampos.SENTENCIADO_EL).HasColumnType(IDominio.DATETIME_2).IsRequired(false);        }
    }

}
