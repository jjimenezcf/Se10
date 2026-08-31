using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using Utilidades;

namespace ServicioDeDatos.Logistica
{

    public enum enumRegularizacionAlm
    {
        [Description("Inicial")]
        Inicial,
        [Description("Recuento")]
        Recuento,
        [Description("Ajuste de precio")]
        AjusteDePrecio
    }

    [Table(Tablas.REGULARIZACION + "_" + Sufijo.ESTADO, Schema = Esquemas.LOGISTICA)]
    public class EstadoDeUnaRegularizacionDtm : EstadoDtm, IInstanciaEstado
    {
        public static new enumNegocio Negocio => enumNegocio.Regularizacion;
    }

    [Table(Tablas.REGULARIZACION + "_" + Sufijo.TRANSICION, Schema = Esquemas.LOGISTICA)]
    public class TransicionesDeUnaRegularizacionDtm : TransicionDtm
    {
    }

    [Table(Tablas.REGULARIZACION + "_" + Sufijo.ACCION, Schema = Esquemas.LOGISTICA)]
    public class AccionesDeUnaRegularizacionDtm : AccionesDeTrnDtm
    {
    }


    [Table(Tablas.REGULARIZACION + "_" + Sufijo.TIPO, Schema = Esquemas.LOGISTICA)]
    public class TipoDeRegularizacionDtm : TipoConFlujoDtm
    {
        public TipoDeRegularizacionDtm Padre { get; set; }
        public new EstadoDeUnaRegularizacionDtm Estado { get; set; }

        public enumRegularizacionAlm ClaseDeRegularizacion { get; set; }

        public override IEstado iEstado => Estado;
        public static new enumNegocio Negocio => enumNegocio.Regularizacion;
    }

    public static partial class ModeloDeRegularizacion
    {
        internal static void EstadosDeUnaRegularizacion(ModelBuilder modelBuilder)
        {
            ApiDeEstado.DefinirCampos<EstadoDeUnaRegularizacionDtm>(modelBuilder);
        }

        internal static void TransicionesDeUnaRegularizacion(ModelBuilder modelBuilder)
        {
            ApiDeTransicion.DefinirCampos<TransicionesDeUnaRegularizacionDtm, EstadoDeUnaRegularizacionDtm>(modelBuilder);
        }

        internal static void AccionesDeUnaRegularizacion(ModelBuilder modelBuilder)
        {
            ApiDeAccionDeTrn.DefinirCampos<AccionesDeUnaRegularizacionDtm, TransicionesDeUnaRegularizacionDtm>(modelBuilder);
        }

        internal static void TipoDeRegularizacion(ModelBuilder modelBuilder)
        {
            ApiTipoDeElementoDtm.DefinirCamposDelTipoElementoDtm<TipoDeRegularizacionDtm>(modelBuilder);

            modelBuilder.Entity<TipoDeRegularizacionDtm>().Property(x => x.IdEstado).HasColumnType(IDominio.INT).HasColumnName(ICampos.ID_ESTADO).IsRequired();

            modelBuilder.Entity<TipoDeRegularizacionDtm>().Property(x => x.ClaseDeRegularizacion).HasColumnName(ICampos.CLASE_REGULARIZACION).HasColumnType(IDominio.VARCHAR_20).IsRequired();

            ApiDeRegistroDtm.DefinirFk<TipoDeRegularizacionDtm>(modelBuilder, nameof(TipoDeRegularizacionDtm.Padre), nameof(TipoDeRegularizacionDtm.IdPadre), ICampos.ID_PADRE, unico: false);
            ApiDeRegistroDtm.DefinirFk<TipoDeRegularizacionDtm>(modelBuilder, nameof(TipoDeRegularizacionDtm.Estado), nameof(TipoDeRegularizacionDtm.IdEstado), ICampos.ID_ESTADO, unico: false);
        }
    }
}
