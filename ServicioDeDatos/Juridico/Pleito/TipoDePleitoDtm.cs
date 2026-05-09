using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Seguridad;
using Utilidades;

namespace ServicioDeDatos.Juridico
{

    public enum enumClaseDePleito
    {
        [Description("Violencia")]
        violencia,
        [Description("Familia")]
        familia,
        [Description("Cobro")]
        recobro
    }

    [Table(Tablas.PLEITO + "_" + Sufijo.ESTADO, Schema = Esquemas.JURIDICO)]
    public class EstadoDeUnPleitoDtm : EstadoDtm, IInstanciaEstado
    {
        public static new enumNegocio Negocio => enumNegocio.Pleito;
    }

    [Table(Tablas.PLEITO + "_" + Sufijo.TRANSICION, Schema = Esquemas.JURIDICO)]
    public class TransicionesDeUnPleitoDtm : TransicionDtm
    {
    }

    [Table(Tablas.PLEITO + "_" + Sufijo.ACCION, Schema = Esquemas.JURIDICO)]
    public class AccionesDeUnPleitoDtm : AccionesDeTrnDtm
    {
    }


    [Table(Tablas.PLEITO + "_" + Sufijo.TIPO, Schema = Esquemas.JURIDICO)]
    public class TipoDePleitoDtm : TipoConFlujoDtm, IPermisoDeInterventor
    {

        public TipoDePleitoDtm Padre { get; set; }
        public int IdPermisoInterventor { get; set; }
        public enumClaseDePleito ClaseDePleito { get; set; }
        public new EstadoDeUnPleitoDtm Estado { get; set; }
        public PermisoDtm PermisoDeInterventor { get; set; }

        public override IEstado iEstado => Estado;
        public static new enumNegocio Negocio => enumNegocio.Pleito;

    }

    [Table(Tablas.PLEITO + "_" + Sufijo.PLANTILLA + "_" + Sufijo.TIPO, Schema = Esquemas.JURIDICO)]
    public class PlantillaPorTipoDePleitoDtm : PlantillaPorTipoDtm
    {

        public new enumNegocio Negocio => enumNegocio.Pleito;

        public new TipoDePleitoDtm Tipo { get; set; }

    }
    public static partial class ModeloDePleito
    {
        internal static void EstadosDeUnPleito(ModelBuilder modelBuilder)
        {
            ApiDeEstado.DefinirCampos<EstadoDeUnPleitoDtm>(modelBuilder);
        }

        internal static void TransicionesDeUnPleito(ModelBuilder modelBuilder)
        {
            ApiDeTransicion.DefinirCampos<TransicionesDeUnPleitoDtm, EstadoDeUnPleitoDtm>(modelBuilder);
        }

        internal static void AccionesDeUnPleito(ModelBuilder modelBuilder)
        {
            ApiDeAccionDeTrn.DefinirCampos<AccionesDeUnPleitoDtm, TransicionesDeUnPleitoDtm>(modelBuilder);
        }

        internal static void TipoDePleito(ModelBuilder modelBuilder)
        {
            ApiTipoDeElementoDtm.DefinirCamposDelTipoElementoDtm<TipoDePleitoDtm>(modelBuilder);

            modelBuilder.Entity<TipoDePleitoDtm>().Property(x => x.IdEstado).HasColumnType(IDominio.INT).HasColumnName(ICampos.ID_ESTADO).IsRequired();
            modelBuilder.Entity<TipoDePleitoDtm>().Property(nameof(TipoDePleitoDtm.ClaseDePleito)).HasColumnName(ICampos.CLASE_PLEITO).HasColumnType(IDominio.VARCHAR_30).IsRequired();

            ApiDeRegistroDtm.DefinirFk<TipoDePleitoDtm>(modelBuilder, nameof(TipoDePleitoDtm.Padre), nameof(TipoDePleitoDtm.IdPadre), ICampos.ID_PADRE, unico: false);
            ApiDeRegistroDtm.DefinirFk<TipoDePleitoDtm>(modelBuilder, nameof(TipoDePleitoDtm.Estado), nameof(TipoDePleitoDtm.IdEstado), ICampos.ID_ESTADO, unico: false);
        }

        internal static void PlantillaPorTipo(ModelBuilder modelBuilder)
        {
            Elemento.PlantillaPorTipo.DefinirCamposDePlantillaPorTipoDtm<PlantillaPorTipoDePleitoDtm>(modelBuilder);
        }
    }
}
