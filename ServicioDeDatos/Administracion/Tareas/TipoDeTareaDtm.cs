using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.SistemaDocumental;
using Utilidades;

namespace ServicioDeDatos.Tarea
{

    public enum enumClaseDeTarea
    {
        [Description("De registro")]
        Registro,
        [Description("De control")]
        Control
    }


    [Table(Tablas.TAREA + "_" + Sufijo.TIPO, Schema = Esquemas.TAREA)]
    public class TipoDeTareaDtm : TipoConFlujoDtm, IPermisoDeInterventor
    {

        public TipoDeTareaDtm Padre { get; set; }
        public int IdPermisoInterventor { get; set; }
        public enumClaseDeTarea ClaseDeTarea { get; set; }
        public int? IdTipoArchivador { get; set; }
        public bool UsaPlanificacion { get; set; }
        public bool EsFacturable { get; set; }
        public bool CopiarDireccionDelSolicitante { get; set; }
        public new EstadoDeUnaTareaDtm Estado { get ; set ; }
        public PermisoDtm PermisoDeInterventor { get; set; }
        public TipoDeArchivadorDtm TipoArchivador { get; set; }

        public override IEstado iEstado => Estado;
        public static new enumNegocio Negocio => enumNegocio.Tarea;

    }

    public static partial class ModeloDeTarea
    {
        public static void TipoDeTarea(ModelBuilder modelBuilder)
        {
            ApiTipoDeElementoDtm.DefinirCamposDelTipoElementoDtm<TipoDeTareaDtm>(modelBuilder);

            modelBuilder.Entity<TipoDeTareaDtm>().Property(x => x.IdEstado).HasColumnType(IDominio.INT).HasColumnName(ICampos.ID_ESTADO).IsRequired();
            DefinirClaseDeTarea<TipoDeTareaDtm>(modelBuilder);
            modelBuilder.Entity<TipoDeTareaDtm>().Property(x => x.IdTipoArchivador).HasColumnType(IDominio.INT).HasColumnName(ICampos.ID_TIPO_ARCHIVADOR).IsRequired(false);
            ApiDeRegistroDtm.DefinirFk<TipoDeTareaDtm>(modelBuilder, nameof(TipoDeTareaDtm.TipoArchivador), nameof(TipoDeTareaDtm.IdTipoArchivador), ICampos.ID_TIPO_ARCHIVADOR, unico: false);

            ApiDeRegistroDtm.DefinirFk<TipoDeTareaDtm>(modelBuilder, nameof(TipoDeTareaDtm.Padre), nameof(TipoDeTareaDtm.IdPadre), ICampos.ID_PADRE, unico: false);
            ApiDeRegistroDtm.DefinirFk<TipoDeTareaDtm>(modelBuilder, nameof(TipoDeTareaDtm.Estado), nameof(TipoDeTareaDtm.IdEstado), ICampos.ID_ESTADO, unico: false);

            modelBuilder.Entity<TipoDeTareaDtm>().Property(p => p.UsaPlanificacion).HasColumnName(ICampos.USA_PLANIFICACION).HasColumnType(IDominio.BIT).HasDefaultValue(false).IsRequired();
            modelBuilder.Entity<TipoDeTareaDtm>().Property(p => p.EsFacturable).HasColumnName(ICampos.ES_FACTURABLE).HasColumnType(IDominio.BIT).HasDefaultValue(false).IsRequired();
            modelBuilder.Entity<TipoDeTareaDtm>().Property(p => p.CopiarDireccionDelSolicitante).HasColumnName(ICampos.COPIAR_DIRECCION).HasColumnType(IDominio.BIT).HasDefaultValue(false).IsRequired();

        }

        private static void DefinirClaseDeTarea<T>(ModelBuilder modelBuilder) where T:RegistroDtm
        {
            modelBuilder.Entity<T>().Property(nameof(TipoDeTareaDtm.ClaseDeTarea)).HasColumnName(ICampos.CLASE_TAREA).HasColumnType(IDominio.VARCHAR_15).IsRequired();
        }
    }
}
