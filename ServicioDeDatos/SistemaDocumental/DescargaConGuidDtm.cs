using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServicioDeDatos.SistemaDocumental
{

    [Table(Tablas.ARCHIVO + "_" + Sufijo.DESCARGA_GUID, Schema = Esquemas.SISDOC)]
    public class DescargaConGuidDtm : RegistroDtm
    {
        public int IdArchivo { get; set; }
        public ArchivoDtm Archivo { get; set; }
        public Guid Guid { get; set; }
        public int IdUsuario { get; set; }
        public UsuarioDtm Usuario { get; set; }
        public DateTime CreadoEl {  get; set; }
        public DateTime? DescargadoEl { get; set; }
        public DateTime? CaducaEl { get; set; }
        public int? MaximoDescargas { get; set; }
        public int? Descargas { get; set; }
    }

    public static partial class ModeloDocumental
    {
        public static void DescargaConGuid(ModelBuilder modelBuilder)
        {
            ApiDeElementoDtm.DefinirCampoArchivo<DescargaConGuidDtm>(modelBuilder, obligatorio: true, unico: false);
            modelBuilder.Entity<DescargaConGuidDtm>().Property(x => x.Guid).HasColumnName(ICampos.GUID).HasColumnType(IDominio.UNIQUEIDENTIFIER).IsRequired(true);


            ApiDeRegistroDtm.DefinirCampoFk<DescargaConGuidDtm>(modelBuilder, nameof(DescargaConGuidDtm.Usuario), nameof(DescargaConGuidDtm.IdUsuario), ICampos.ID_USUARIO, requerida: true, unico: false);

            modelBuilder.Entity<DescargaConGuidDtm>().Property(nameof(DescargaConGuidDtm.CreadoEl)).HasColumnName(ICampos.CREADO_EL).HasColumnType(IDominio.DATETIME_2).IsRequired(true);
            modelBuilder.Entity<DescargaConGuidDtm>().Property(nameof(DescargaConGuidDtm.DescargadoEl)).HasColumnName(ICampos.DESCARGADO_EL).HasColumnType(IDominio.DATETIME_2).IsRequired(false);
            modelBuilder.Entity<DescargaConGuidDtm>().Property(nameof(DescargaConGuidDtm.CaducaEl)).HasColumnName(ICampos.CADUCA_EL).HasColumnType(IDominio.DATETIME_2).IsRequired(false);
            modelBuilder.Entity<DescargaConGuidDtm>().Property(nameof(DescargaConGuidDtm.MaximoDescargas)).HasColumnName(ICampos.MAXIMO_DESCARGAS).HasColumnType(IDominio.INT).IsRequired(false);
            modelBuilder.Entity<DescargaConGuidDtm>().Property(nameof(DescargaConGuidDtm.Descargas)).HasColumnName(ICampos.NUMERO).HasColumnType(IDominio.INT).IsRequired(false);
        }

    }


}
