using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.SistemaDocumental;
using Utilidades;

namespace ServicioDeDatos.RegistroEs
{

    public enum enumClaseDeRegistroEs
    {
        [Description("Entrada")]
        E,
        [Description("Salida")]
        S
    }


    [Table(Tablas.REGISTRO + "_" + Sufijo.TIPO, Schema = Esquemas.REGISTRO)]
    public class TipoDeRegistroEsDtm : TipoConFlujoDtm, IPermisoDeInterventor
    {

        public TipoDeRegistroEsDtm Padre { get; set; }
        public int IdPermisoInterventor { get; set; }
        public string ClaseDeRegistro { get; set; }
        public int? IdTipoArchivadorDeEntrada { get; set; }
        public int? IdTipoArchivadorDeSalida { get; set; }
        public int? IdTipoArchivadorInterno { get; set; }
        public new EstadoDeUnRegistroEsDtm Estado { get ; set ; }
        public PermisoDtm PermisoDeInterventor { get; set; }
        public TipoDeArchivadorDtm TipoArchivadorDeEntrada { get; set; }
        public TipoDeArchivadorDtm TipoArchivadorDeSalida { get; set; }
        public TipoDeArchivadorDtm TipoArchivadorInterno { get; set; }

        public override IEstado iEstado => Estado;
        public static new enumNegocio Negocio => enumNegocio.Registro;

    }

    public static partial class ModeloDeRegistroEs
    {
        public static void TipoDeRegistroEs(ModelBuilder modelBuilder)
        {
            ApiTipoDeElementoDtm.DefinirCamposDelTipoElementoDtm<TipoDeRegistroEsDtm>(modelBuilder);

            modelBuilder.Entity<TipoDeRegistroEsDtm>().Property(x => x.IdEstado).HasColumnType(IDominio.INT).HasColumnName(ICampos.ID_ESTADO).IsRequired();
            modelBuilder.Entity<TipoDeRegistroEsDtm>().Property(p => p.ClaseDeRegistro).HasColumnName(ICampos.CLASE_ES).HasColumnType(IDominio.VARCHAR_1).IsRequired();
            modelBuilder.Entity<TipoDeRegistroEsDtm>().Property(x => x.IdTipoArchivadorDeEntrada).HasColumnType(IDominio.INT).HasColumnName(ICampos.ID_TIPO_ENTRADA).IsRequired(false);
            modelBuilder.Entity<TipoDeRegistroEsDtm>().Property(x => x.IdTipoArchivadorDeSalida).HasColumnType(IDominio.INT).HasColumnName(ICampos.ID_TIPO_SALIDA).IsRequired(false);
            modelBuilder.Entity<TipoDeRegistroEsDtm>().Property(x => x.IdTipoArchivadorInterno).HasColumnType(IDominio.INT).HasColumnName(ICampos.ID_TIPO_INTERNO).IsRequired(false);

            ApiDeRegistroDtm.DefinirFk<TipoDeRegistroEsDtm>(modelBuilder, nameof(TipoDeRegistroEsDtm.Padre), nameof(TipoDeRegistroEsDtm.IdPadre), ICampos.ID_PADRE, unico: false);
            ApiDeRegistroDtm.DefinirFk<TipoDeRegistroEsDtm>(modelBuilder, nameof(TipoDeRegistroEsDtm.Estado), nameof(TipoDeRegistroEsDtm.IdEstado), ICampos.ID_ESTADO, unico: false);

            ApiDeRegistroDtm.DefinirFk<TipoDeRegistroEsDtm>(modelBuilder, nameof(TipoDeRegistroEsDtm.TipoArchivadorDeEntrada),nameof(TipoDeRegistroEsDtm.IdTipoArchivadorDeEntrada),  ICampos.ID_TIPO_ENTRADA, unico: false);
            ApiDeRegistroDtm.DefinirFk<TipoDeRegistroEsDtm>(modelBuilder, nameof(TipoDeRegistroEsDtm.TipoArchivadorDeSalida), nameof(TipoDeRegistroEsDtm.IdTipoArchivadorDeSalida),   ICampos.ID_TIPO_SALIDA, unico: false);
            ApiDeRegistroDtm.DefinirFk<TipoDeRegistroEsDtm>(modelBuilder, nameof(TipoDeRegistroEsDtm.TipoArchivadorInterno), nameof(TipoDeRegistroEsDtm.IdTipoArchivadorInterno),    ICampos.ID_TIPO_INTERNO, unico: false);

        }

    }
}
