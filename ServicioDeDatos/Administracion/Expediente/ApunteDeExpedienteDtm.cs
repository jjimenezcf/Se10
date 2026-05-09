using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DocumentFormat.OpenXml;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.MaestrosTecnico;
using ServicioDeDatos.SistemaDocumental;
using Utilidades;

namespace ServicioDeDatos.Expediente
{
    public enum enumApunteDeExpediente { Ingreso, Gasto}

    [Table(Tablas.APUNTE , Schema = Esquemas.EXPEDIENTE)]
    public class ApunteDeExpedienteDtm : RegistroDtm, IDetalle, IUsaArchivo, IAuditoria
    {
        public int IdElemento { get; set; }
        public ExpedienteDtm Elemento { get; set; }
        IElementoDtm IDetalle.Elemento => Elemento;

        public enumApunteDeExpediente Clase { get; set; }
        public int Orden { get; set; }
        public string Concepto { get; set; }

        public int IdNaturaleza { get; set; }
        public NaturalezaDtm Naturaleza { get; set; }

        public decimal Valor { get; set; }
        public DateTime ImputadoEl { get; set; }

        public int? IdArchivo { get; set ; }
        public ArchivoDtm Archivo { get ; set ; }

        public int IdUsuaCrea { get; set; }
        public DateTime FechaCreacion { get; set; }
        public UsuarioDtm UsuarioCreador { get; set; }

        public int? IdUsuaModi { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public UsuarioDtm UsuarioModificador { get; set; }
        public enumNegocio Negocio => enumNegocio.Expediente;


        [Timestamp]
        public byte[] RowVersion { get; set; }
    }

    public static partial class ModeloDeExpediente
    {
        internal static void ApuntesDeExpediente(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ApunteDeExpedienteDtm>().Ignore(x => x.Elemento);
            modelBuilder.Entity<ApunteDeExpedienteDtm>().Ignore(x => x.Negocio);
            modelBuilder.Entity<ApunteDeExpedienteDtm>().Property(nameof(ApunteDeExpedienteDtm.IdElemento)).HasColumnName(ICampos.ID_ELEMENTO).HasColumnType(IDominio.INT).IsRequired();
            ApiDeRegistroDtm.DefinirFk<ApunteDeExpedienteDtm, ExpedienteDtm>(modelBuilder, nameof(ApunteDeExpedienteDtm.IdElemento), ICampos.ID_ELEMENTO, unico: false);

            modelBuilder.Entity<ApunteDeExpedienteDtm>().Property(p => p.Clase).HasColumnName(ICampos.CLASE).HasColumnType(IDominio.VARCHAR_30).IsRequired(true);
            modelBuilder.Entity<ApunteDeExpedienteDtm>().Property(nameof(ApunteDeExpedienteDtm.Orden)).HasColumnName(ICampos.ORDEN).HasColumnType(IDominio.INT).IsRequired(true);
            modelBuilder.Entity<ApunteDeExpedienteDtm>().Property(nameof(ApunteDeExpedienteDtm.Concepto)).HasColumnName(ICampos.CONCEPTO).HasColumnType(IDominio.VARCHAR_250).IsRequired(true);
            modelBuilder.Entity<ApunteDeExpedienteDtm>().Property(nameof(ApunteDeExpedienteDtm.Valor)).HasColumnName(ICampos.VALOR).HasColumnType(IDominio.DECIMAL).IsRequired(true);

            modelBuilder.Entity<ApunteDeExpedienteDtm>().Property(nameof(ApunteDeExpedienteDtm.ImputadoEl)).HasColumnName(ICampos.IMPUTADO_EL).HasColumnType(IDominio.DATETIME_2).IsRequired(true);

            ApiDeRegistroDtm.DefinirCampoFk<ApunteDeExpedienteDtm>(modelBuilder, nameof(ApunteDeExpedienteDtm.Naturaleza), nameof(ApunteDeExpedienteDtm.IdNaturaleza), ICampos.ID_NATURALEZA, requerida: true, unico: false);

            ApiDeElementoDtm.DefinirCampoArchivo<ApunteDeExpedienteDtm>(modelBuilder);

            ApiDeElementoDtm.DefinirCamposDeAuditoria<ApunteDeExpedienteDtm>(modelBuilder);

        }
    }
}
