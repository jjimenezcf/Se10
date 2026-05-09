using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServicioDeDatos.Entorno
{
    [Table(Tablas.MICORREO, Schema = Esquemas.ENTORNO)]
    public class MiCorreoDtm : RegistroDtm
    {
        public string IdMensaje { get; set; }
        public string Buzon { get; set; }
        public DateTime Fecha { get; set; }
        public string Emisor { get; set; }
        public string To { get; set; }
        public string Asunto { get; set; }
        public string Cuerpo { get; set; }
        public string Adjuntos { get; set; }
        public string Accion { get; set; }

        public int? IdNegocio { get; set; }
        public int? IdElemento { get; set; }
        public int? IdObservacion { get; set; }
    }

    public static partial class ModeloDeEntorno
    {
        public static void MiCorreo(ModelBuilder modelBuilder)
        {
            var tabla = ApiDeRegistroDtm.NombreDeTabla(typeof(MiCorreoDtm));
            ApiDeRegistroDtm.DefinirCampoIdDtm<CertificadoDtm>(modelBuilder);

            modelBuilder.Entity<MiCorreoDtm>().Property(nameof(MiCorreoDtm.IdMensaje)).HasColumnName(ICampos.ID_MENSAJE).HasColumnType(IDominio.VARCHAR_250).IsRequired(true);
            modelBuilder.Entity<MiCorreoDtm>().Property(nameof(MiCorreoDtm.Buzon)).HasColumnName(ICampos.BUZON).HasColumnType(IDominio.VARCHAR_250).IsRequired(true);
            modelBuilder.Entity<MiCorreoDtm>().Property(nameof(MiCorreoDtm.Fecha)).HasColumnName(ICampos.FECHA).HasColumnType(IDominio.DATETIME_2).IsRequired(true);
            modelBuilder.Entity<MiCorreoDtm>().Property(nameof(MiCorreoDtm.Emisor)).HasColumnName(ICampos.EMISOR).HasColumnType(IDominio.VARCHAR_250).IsRequired(true);
            modelBuilder.Entity<MiCorreoDtm>().Property(nameof(MiCorreoDtm.To)).HasColumnName(ICampos.TO).HasColumnType(IDominio.VARCHAR_250).IsRequired(true);

            modelBuilder.Entity<MiCorreoDtm>().Property(nameof(MiCorreoDtm.Asunto)).HasColumnName(ICampos.ASUNTO).HasColumnType(IDominio.VARCHAR_250).IsRequired(true);
            modelBuilder.Entity<MiCorreoDtm>().Property(nameof(MiCorreoDtm.Cuerpo)).HasColumnName(ICampos.CUERPO).HasColumnType(IDominio.VARCHAR_2000).IsRequired(true);
            modelBuilder.Entity<MiCorreoDtm>().Property(nameof(MiCorreoDtm.Adjuntos)).HasColumnName(ICampos.ADJUNTOS).HasColumnType(IDominio.VARCHAR_2000).IsRequired(true);
            modelBuilder.Entity<MiCorreoDtm>().Property(nameof(MiCorreoDtm.Accion)).HasColumnName(ICampos.ACCION).HasColumnType(IDominio.VARCHAR_250).IsRequired(true);

            modelBuilder.Entity<MiCorreoDtm>().Property(nameof(MiCorreoDtm.IdNegocio)).HasColumnName(ICampos.ID_NEGOCIO).HasColumnType(IDominio.INT).IsRequired(false);
            modelBuilder.Entity<MiCorreoDtm>().Property(nameof(MiCorreoDtm.IdElemento)).HasColumnName(ICampos.ID_ELEMENTO).HasColumnType(IDominio.INT).IsRequired(false);
            modelBuilder.Entity<MiCorreoDtm>().Property(nameof(MiCorreoDtm.IdObservacion)).HasColumnName(ICampos.ID_OBSERVACION).HasColumnType(IDominio.INT).IsRequired(false);

            modelBuilder.Entity<MiCorreoDtm>().HasAlternateKey([nameof(MiCorreoDtm.IdMensaje)]).HasName($"AK_{tabla}_{ICampos.ID_MENSAJE}");

            modelBuilder.Entity<MiCorreoDtm>()
                .HasIndex(e => new { e.IdNegocio, e.IdElemento, e.IdObservacion })
                .HasDatabaseName($"I_{tabla}_{ICampos.ID_NEGOCIO}_{ICampos.ID_ELEMENTO}_{ICampos.ID_OBSERVACION}");
        }
    }

}
