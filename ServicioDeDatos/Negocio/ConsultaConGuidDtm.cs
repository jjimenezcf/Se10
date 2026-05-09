using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServicioDeDatos.Negocio
{

    [Table(Tablas.CONSULTA_CON_GUID, Schema = Esquemas.NEGOCIO)]
    public class ConsultaConGuidDtm : RegistroDtm, ITieneCampoNegocio
    {
        public int IdNegocio { get; set; }
        public int IdElemento { get; set; }
        public Guid Guid { get; set; }
        public int IdUsuario { get; set; }
        public UsuarioDtm Usuario { get; set; }
        public DateTime CreadoEl {  get; set; }
        public DateTime? DescargadoEl { get; set; }
        public DateTime? CaducaEl { get; set; }
        public int? MaximoDescargas { get; set; }
        public int? Descargas { get; set; }

        public NegocioDtm Negocio { get; set; }
}

    public static partial class ModeloDeNegocio
    {
        public static void ConsultasConGuid(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ConsultaConGuidDtm>().Ignore(x => x.Negocio);

            ModeloDeNegocio.DefinirCampoNegocio<ConsultaConGuidDtm>(modelBuilder, unico: false);
            modelBuilder.Entity<ConsultaConGuidDtm>().Property(x => x.IdElemento).HasColumnName(ICampos.ID_ELEMENTO).HasColumnType(IDominio.INT).IsRequired(true);

            modelBuilder.Entity<ConsultaConGuidDtm>().Property(x => x.Guid).HasColumnName(ICampos.GUID).HasColumnType(IDominio.UNIQUEIDENTIFIER).IsRequired(true);
            ApiDeRegistroDtm.DefinirCampoFk<ConsultaConGuidDtm>(modelBuilder, nameof(ConsultaConGuidDtm.Usuario), nameof(ConsultaConGuidDtm.IdUsuario), ICampos.ID_USUARIO, requerida: true, unico: false);

            modelBuilder.Entity<ConsultaConGuidDtm>().Property(nameof(ConsultaConGuidDtm.CreadoEl)).HasColumnName(ICampos.CREADO_EL).HasColumnType(IDominio.DATETIME_2).IsRequired(true);
            modelBuilder.Entity<ConsultaConGuidDtm>().Property(nameof(ConsultaConGuidDtm.DescargadoEl)).HasColumnName(ICampos.DESCARGADO_EL).HasColumnType(IDominio.DATETIME_2).IsRequired(false);
            modelBuilder.Entity<ConsultaConGuidDtm>().Property(nameof(ConsultaConGuidDtm.CaducaEl)).HasColumnName(ICampos.CADUCA_EL).HasColumnType(IDominio.DATETIME_2).IsRequired(false);
            modelBuilder.Entity<ConsultaConGuidDtm>().Property(nameof(ConsultaConGuidDtm.MaximoDescargas)).HasColumnName(ICampos.MAXIMO_DESCARGAS).HasColumnType(IDominio.INT).IsRequired(false);
            modelBuilder.Entity<ConsultaConGuidDtm>().Property(nameof(ConsultaConGuidDtm.Descargas)).HasColumnName(ICampos.NUMERO).HasColumnType(IDominio.INT).IsRequired(false);
        }

    }


}
