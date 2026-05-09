using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServicioDeDatos.SistemaDocumental
{
    [Table(Tablas.FIRMADO, Schema = Esquemas.SISDOC)]
    public class FirmadoDtm : RegistroDtm
    {
        public int IdOriginal { get; set; }
        public int IdFirmado { get; set; }
        public int IdUsuario { get; set; }
        public int IdCertificado { get; set; }
        public DateTime FirmadoEl { get; set; }
        public string Motivo { get; set; }
    }
   

    public static partial class ModeloDocumental
    {
        public static void Firmado(ModelBuilder modelBuilder)
        {
            ApiDeRegistroDtm.DefinirCampoIdDtm<FirmadoDtm>(modelBuilder);
            modelBuilder.Entity<FirmadoDtm>().Property(x => x.IdOriginal).HasColumnName(ICampos.ID_ORIGINAL).HasColumnType(IDominio.INT).IsRequired();
            modelBuilder.Entity<FirmadoDtm>().Property(x => x.IdFirmado).HasColumnName(ICampos.ID_FIRMADO).HasColumnType(IDominio.INT).IsRequired();
            modelBuilder.Entity<FirmadoDtm>().Property(x => x.IdCertificado).HasColumnName(ICampos.ID_CERTIFICADO).HasColumnType(IDominio.INT).IsRequired();
            modelBuilder.Entity<FirmadoDtm>().Property(x => x.IdUsuario).HasColumnName(ICampos.ID_USUARIO).HasColumnType(IDominio.INT).IsRequired();

            ApiDeRegistroDtm.DefinirFk<FirmadoDtm, ArchivoDtm>(modelBuilder, nameof(FirmadoDtm.IdOriginal), ICampos.ID_ORIGINAL, unico: true);
            ApiDeRegistroDtm.DefinirFk<FirmadoDtm, ArchivoDtm>(modelBuilder, nameof(FirmadoDtm.IdFirmado), ICampos.ID_FIRMADO, unico: true);
            ApiDeRegistroDtm.DefinirFk<FirmadoDtm, CertificadoDtm>(modelBuilder, nameof(FirmadoDtm.IdCertificado), ICampos.ID_CERTIFICADO, unico: false);
            ApiDeRegistroDtm.DefinirFk<FirmadoDtm, UsuarioDtm>(modelBuilder, nameof(FirmadoDtm.IdUsuario), ICampos.ID_USUARIO, unico: false);

            modelBuilder.Entity<FirmadoDtm>().Property(x => x.Motivo).HasColumnName(ICampos.MOTIVO).HasColumnType(IDominio.VARCHAR_2000).IsRequired();
            modelBuilder.Entity<FirmadoDtm>().Property(x => x.FirmadoEl).HasColumnName(ICampos.FIRMADO_EL).HasColumnType(IDominio.DATETIME_2).IsRequired();
        }

    }


}
