using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Seguridad;
using Utilidades;

namespace ServicioDeDatos.Logistica
{

    public enum enumAlmacenCalculo
    {
        [Description("Fifo (primero en entrar, primero en salir)")]
        Fifo,
        [Description("Lifo (último en entrar, primero en salir)")]
        Lifo,
        [Description("Precio medio ponderado")]
        PMP
    }

    [Table(Tablas.ALMACEN + "_" + Sufijo.ESTADO, Schema = Esquemas.LOGISTICA)]
    public class EstadoDeUnAlmacenDtm : EstadoDtm, IInstanciaEstado
    {
        public static new enumNegocio Negocio => enumNegocio.Almacen;
    }

    [Table(Tablas.ALMACEN + "_" + Sufijo.TRANSICION, Schema = Esquemas.LOGISTICA)]
    public class TransicionesDeUnAlmacenDtm : TransicionDtm
    {
    }

    [Table(Tablas.ALMACEN + "_" + Sufijo.ACCION, Schema = Esquemas.LOGISTICA)]
    public class AccionesDeUnAlmacenDtm : AccionesDeTrnDtm
    {
    }


    [Table(Tablas.ALMACEN + "_" + Sufijo.TIPO, Schema = Esquemas.LOGISTICA)]
    public class TipoDeAlmacenDtm : TipoConFlujoDtm, IPermisoDeInterventor
    {
        public TipoDeAlmacenDtm Padre { get; set; }
        public int IdPermisoInterventor { get; set; }
        public new EstadoDeUnAlmacenDtm Estado { get; set; }
        public PermisoDtm PermisoDeInterventor { get; set; }

        public enumAlmacenCalculo Calculo { get; set; }

        public override IEstado iEstado => Estado;
        public static new enumNegocio Negocio => enumNegocio.Almacen;
    }

    public static partial class ModeloDeAlmacen
    {
        internal static void EstadosDeUnAlmacen(ModelBuilder modelBuilder)
        {
            ApiDeEstado.DefinirCampos<EstadoDeUnAlmacenDtm>(modelBuilder);
        }

        internal static void TransicionesDeUnAlmacen(ModelBuilder modelBuilder)
        {
            ApiDeTransicion.DefinirCampos<TransicionesDeUnAlmacenDtm, EstadoDeUnAlmacenDtm>(modelBuilder);
        }

        internal static void AccionesDeUnAlmacen(ModelBuilder modelBuilder)
        {
            ApiDeAccionDeTrn.DefinirCampos<AccionesDeUnAlmacenDtm, TransicionesDeUnAlmacenDtm>(modelBuilder);
        }

        internal static void TipoDeAlmacen(ModelBuilder modelBuilder)
        {
            ApiTipoDeElementoDtm.DefinirCamposDelTipoElementoDtm<TipoDeAlmacenDtm>(modelBuilder);

            modelBuilder.Entity<TipoDeAlmacenDtm>().Property(x => x.IdEstado).HasColumnType(IDominio.INT).HasColumnName(ICampos.ID_ESTADO).IsRequired();

            modelBuilder.Entity<TipoDeAlmacenDtm>().Property(x => x.Calculo).HasColumnName(ICampos.CALCULO).HasColumnType(IDominio.VARCHAR_20).IsRequired();

            ApiDeRegistroDtm.DefinirFk<TipoDeAlmacenDtm>(modelBuilder, nameof(TipoDeAlmacenDtm.Padre), nameof(TipoDeAlmacenDtm.IdPadre), ICampos.ID_PADRE, unico: false);
            ApiDeRegistroDtm.DefinirFk<TipoDeAlmacenDtm>(modelBuilder, nameof(TipoDeAlmacenDtm.Estado), nameof(TipoDeAlmacenDtm.IdEstado), ICampos.ID_ESTADO, unico: false);
        }
    }
}
