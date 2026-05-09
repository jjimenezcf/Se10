using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Seguridad;
using Utilidades;

namespace ServicioDeDatos.Gastos
{

    [Table(Tablas.REMESA_PAG + "_" + Sufijo.ESTADO, Schema = Esquemas.GASTO)]
    public class EstadoDeUnaRemesaPagDtm : EstadoDtm, IInstanciaEstado
    {
        public static new enumNegocio Negocio => enumNegocio.RemesaPag;
    }

    [Table(Tablas.REMESA_PAG + "_" + Sufijo.TRANSICION, Schema = Esquemas.GASTO)]
    public class TransicionesDeUnaRemesaPagDtm : TransicionDtm
    {
    }

    [Table(Tablas.REMESA_PAG + "_" + Sufijo.ACCION, Schema = Esquemas.GASTO)]
    public class AccionesDeUnaRemesaPagDtm : AccionesDeTrnDtm
    {
    }


    [Table(Tablas.REMESA_PAG + "_" + Sufijo.TIPO, Schema = Esquemas.GASTO)]
    public class TipoDeRemesaPagDtm : TipoConFlujoDtm, IPermisoDeInterventor
    {
        public TipoDeRemesaPagDtm Padre { get; set; }
        public int IdPermisoInterventor { get; set; }
        public new EstadoDeUnaRemesaPagDtm Estado { get; set; }
        public PermisoDtm PermisoDeInterventor { get; set; }

        public override IEstado iEstado => Estado;
        public static new enumNegocio Negocio => enumNegocio.RemesaPag;
    }

    public static partial class ModeloDeRemesaPag
    {
        internal static void EstadosDeUnaRemesaPag(ModelBuilder modelBuilder)
        {
            ApiDeEstado.DefinirCampos<EstadoDeUnaRemesaPagDtm>(modelBuilder);
        }

        internal static void TransicionesDeUnaRemesaPag(ModelBuilder modelBuilder)
        {
            ApiDeTransicion.DefinirCampos<TransicionesDeUnaRemesaPagDtm, EstadoDeUnaRemesaPagDtm>(modelBuilder);
        }

        internal static void AccionesDeUnaRemesaPag(ModelBuilder modelBuilder)
        {
            ApiDeAccionDeTrn.DefinirCampos<AccionesDeUnaRemesaPagDtm, TransicionesDeUnaRemesaPagDtm>(modelBuilder);
        }

        internal static void TipoDeRemesaPag(ModelBuilder modelBuilder)
        {
            ApiTipoDeElementoDtm.DefinirCamposDelTipoElementoDtm<TipoDeRemesaPagDtm>(modelBuilder);

            modelBuilder.Entity<TipoDeRemesaPagDtm>().Property(x => x.IdEstado).HasColumnType(IDominio.INT).HasColumnName(ICampos.ID_ESTADO).IsRequired();


            ApiDeRegistroDtm.DefinirFk<TipoDeRemesaPagDtm>(modelBuilder, nameof(TipoDeRemesaPagDtm.Padre), nameof(TipoDeRemesaPagDtm.IdPadre), ICampos.ID_PADRE, unico: false);
            ApiDeRegistroDtm.DefinirFk<TipoDeRemesaPagDtm>(modelBuilder, nameof(TipoDeRemesaPagDtm.Estado), nameof(TipoDeRemesaPagDtm.IdEstado), ICampos.ID_ESTADO, unico: false);
        }
    }
}
