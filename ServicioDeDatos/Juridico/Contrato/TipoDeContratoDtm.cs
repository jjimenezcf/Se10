using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.SqlServer.Dac.Model;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Ventas;
using Utilidades;

namespace ServicioDeDatos.Juridico
{

    public enum enumClaseDeContrato
    {
        [Description("Prestación de servicios")]
        Venta,
        [Description("Contratación de servicios")]
        Compra,
        [Description("Subvención")]
        Subvencion,
        [Description("Colaboración")]
        Colaboracion,
        [Description("Acuerdo marco")]
        Marco,
        [Description("Matrícula de guardería")]
        MatriculaDeGuarderia
    }

    [Table(Tablas.CONTRATO + "_" + Sufijo.ESTADO, Schema = Esquemas.JURIDICO)]
    public class EstadoDeUnContratoDtm : EstadoDtm, IInstanciaEstado
    {
        public static new enumNegocio Negocio => enumNegocio.Contrato;
    }

    [Table(Tablas.CONTRATO + "_" + Sufijo.TRANSICION, Schema = Esquemas.JURIDICO)]
    public class TransicionesDeUnContratoDtm : TransicionDtm
    {
    }

    [Table(Tablas.CONTRATO + "_" + Sufijo.ACCION, Schema = Esquemas.JURIDICO)]
    public class AccionesDeUnContratoDtm : AccionesDeTrnDtm
    {
    }


    [Table(Tablas.CONTRATO + "_" + Sufijo.TIPO, Schema = Esquemas.JURIDICO)]
    public class TipoDeContratoDtm : TipoConFlujoDtm, IPermisoDeInterventor
    {

        public TipoDeContratoDtm Padre { get; set; }
        public int IdPermisoInterventor { get; set; }
        public enumClaseDeContrato ClaseDeContrato { get; set; }
        public new EstadoDeUnContratoDtm Estado { get; set; }
        public PermisoDtm PermisoDeInterventor { get; set; }
        public int IdTipoArchivador { get; set; }
        public TipoDeArchivadorDtm TipoArchivador { get; set; }

        public int? IdTipoFacturaEmt { get; set; }
        public TipoDeFacturaEmtDtm TipoFacturaEmt { get; set; }

        public override IEstado iEstado => Estado;
        public new static enumNegocio Negocio => enumNegocio.Contrato;

    }

    public static partial class ModeloDeContrato
    {
        internal static void EstadosDeUnContrato(ModelBuilder modelBuilder)
        {
            ApiDeEstado.DefinirCampos<EstadoDeUnContratoDtm>(modelBuilder);
        }

        internal static void TransicionesDeUnContrato(ModelBuilder modelBuilder)
        {
            ApiDeTransicion.DefinirCampos<TransicionesDeUnContratoDtm, EstadoDeUnContratoDtm>(modelBuilder);
        }

        internal static void AccionesDeUnContrato(ModelBuilder modelBuilder)
        {
            ApiDeAccionDeTrn.DefinirCampos<AccionesDeUnContratoDtm, TransicionesDeUnContratoDtm>(modelBuilder);
        }

        internal static void TipoDeContrato(ModelBuilder modelBuilder)
        {
            ApiTipoDeElementoDtm.DefinirCamposDelTipoElementoDtm<TipoDeContratoDtm>(modelBuilder);

            modelBuilder.Entity<TipoDeContratoDtm>().Property(x => x.IdEstado).HasColumnType(IDominio.INT).HasColumnName(ICampos.ID_ESTADO).IsRequired();
            modelBuilder.Entity<TipoDeContratoDtm>().Property(nameof(TipoDeContratoDtm.ClaseDeContrato)).HasColumnName(ICampos.CLASE_CONTRATO).HasColumnType(IDominio.VARCHAR_30).IsRequired();

            ApiDeRegistroDtm.DefinirFk<TipoDeContratoDtm>(modelBuilder, nameof(TipoDeContratoDtm.Padre), nameof(TipoDeContratoDtm.IdPadre), ICampos.ID_PADRE, unico: false);
            ApiDeRegistroDtm.DefinirFk<TipoDeContratoDtm>(modelBuilder, nameof(TipoDeContratoDtm.Estado), nameof(TipoDeContratoDtm.IdEstado), ICampos.ID_ESTADO, unico: false);

            modelBuilder.Entity<TipoDeContratoDtm>().Property(x => x.IdTipoArchivador).HasColumnType(IDominio.INT).HasColumnName(ICampos.ID_TIPO_ARCHIVADOR).IsRequired();
            ApiDeRegistroDtm.DefinirFk<TipoDeContratoDtm>(modelBuilder, nameof(TipoDeContratoDtm.TipoArchivador), nameof(TipoDeContratoDtm.IdTipoArchivador), ICampos.ID_TIPO_ARCHIVADOR, false);
            ApiDeRegistroDtm.DefinirCampoFk<TipoDeContratoDtm>(modelBuilder, nameof(TipoDeContratoDtm.TipoFacturaEmt), nameof(TipoDeContratoDtm.IdTipoFacturaEmt), ICampos.ID_TIPO_FACTURA, requerida: false, unico: false);
        }

    }
}
