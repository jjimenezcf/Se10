using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.MaestrosTecnico;
using ServicioDeDatos.Seguridad;
using Utilidades;

namespace ServicioDeDatos.Ventas
{

    [Table(Tablas.FACTURA_EMT + "_" + Sufijo.ESTADO, Schema = Esquemas.VENTA)]
    public class EstadoDeUnaFacturaEmtDtm : EstadoDtm, IInstanciaEstado
    {
        public static new enumNegocio Negocio => enumNegocio.FacturaEmitida;
    }

    [Table(Tablas.FACTURA_EMT + "_" + Sufijo.TRANSICION, Schema = Esquemas.VENTA)]
    public class TransicionesDeUnaFacturaEmtDtm : TransicionDtm
    {
    }

    [Table(Tablas.FACTURA_EMT + "_" + Sufijo.ACCION, Schema = Esquemas.VENTA)]
    public class AccionesDeUnaFacturaEmtDtm : AccionesDeTrnDtm
    {
    }


    [Table(Tablas.FACTURA_EMT + "_" + Sufijo.TIPO, Schema = Esquemas.VENTA)]
    public class TipoDeFacturaEmtDtm : TipoConFlujoDtm, IPermisoDeInterventor
    {
        public TipoDeFacturaEmtDtm Padre { get; set; }
        public int IdPermisoInterventor { get; set; }
        public new EstadoDeUnaFacturaEmtDtm Estado { get; set; }
        public PermisoDtm PermisoDeInterventor { get; set; }
        public string Serie { get; set; }

        public override IEstado iEstado => Estado;
        public static new enumNegocio Negocio => enumNegocio.FacturaEmitida;
        
        public int? IdIvaRDefecto { get; set; }
        public enumClaseUnitario? ClaseDefecto { get; set; }
        public int? IdUnidadDefecto { get; set; }
        public int? IdNaturalezaDefecto { get; set; }
        public IvaRepercutidoDtm IvaRepercutido { get; set; }
        public UnidadDtm Unidad { get; set; }
        public NaturalezaDtm Naturaleza { get; set; }
        public int Vencimiento { get; set; }
        public bool EsExportacion { get; set; }
        public bool ConPeriodoEmt { get; set; }
    }


    [Table(Tablas.FACTURA_EMT + "_" + Sufijo.PLANTILLA + "_" + Sufijo.TIPO, Schema = Esquemas.VENTA)]
    public class PlantillaPorTipoDeFacturaEmtDtm : PlantillaPorTipoDtm
    {

        public new enumNegocio Negocio => enumNegocio.FacturaEmitida;

        public new TipoDeFacturaEmtDtm Tipo { get; set; }

    }

    public static partial class ModeloDeFacturaEmt
    {
        internal static void EstadosDeUnaFacturaEmt(ModelBuilder modelBuilder)
        {
            ApiDeEstado.DefinirCampos<EstadoDeUnaFacturaEmtDtm>(modelBuilder);
        }

        internal static void TransicionesDeUnaFacturaEmt(ModelBuilder modelBuilder)
        {
            ApiDeTransicion.DefinirCampos<TransicionesDeUnaFacturaEmtDtm, EstadoDeUnaFacturaEmtDtm>(modelBuilder);
        }

        internal static void AccionesDeUnaFacturaEmt(ModelBuilder modelBuilder)
        {
            ApiDeAccionDeTrn.DefinirCampos<AccionesDeUnaFacturaEmtDtm, TransicionesDeUnaFacturaEmtDtm>(modelBuilder);
        }

        internal static void TipoDeFacturaEmt(ModelBuilder modelBuilder)
        {
            ApiTipoDeElementoDtm.DefinirCamposDelTipoElementoDtm<TipoDeFacturaEmtDtm>(modelBuilder);

            modelBuilder.Entity<TipoDeFacturaEmtDtm>().Property(x => x.IdEstado).HasColumnType(IDominio.INT).HasColumnName(ICampos.ID_ESTADO).IsRequired();

            modelBuilder.Entity<TipoDeFacturaEmtDtm>().Property(p => p.Serie).HasColumnName(ICampos.SERIE).HasColumnType(IDominio.VARCHAR_3).IsRequired(true);

            ApiDeRegistroDtm.DefinirFk<TipoDeFacturaEmtDtm>(modelBuilder, nameof(TipoDeFacturaEmtDtm.Padre), nameof(TipoDeFacturaEmtDtm.IdPadre), ICampos.ID_PADRE, unico: false);
            ApiDeRegistroDtm.DefinirFk<TipoDeFacturaEmtDtm>(modelBuilder, nameof(TipoDeFacturaEmtDtm.Estado), nameof(TipoDeFacturaEmtDtm.IdEstado), ICampos.ID_ESTADO, unico: false);
            ApiDeRegistroDtm.DefinirCampoFk<TipoDeFacturaEmtDtm>(modelBuilder, nameof(TipoDeFacturaEmtDtm.IvaRepercutido), nameof(TipoDeFacturaEmtDtm.IdIvaRDefecto), ICampos.ID_IVA_R, requerida: false, unico: false);
            modelBuilder.Entity<TipoDeFacturaEmtDtm>().Property(p => p.ClaseDefecto).HasColumnName(ICampos.CLASE).HasColumnType(IDominio.VARCHAR_30).IsRequired(false);
            ApiDeRegistroDtm.DefinirCampoFk<TipoDeFacturaEmtDtm>(modelBuilder, nameof(TipoDeFacturaEmtDtm.Unidad), nameof(TipoDeFacturaEmtDtm.IdUnidadDefecto), ICampos.ID_UNIDAD, requerida: false, unico: false);
            ApiDeRegistroDtm.DefinirCampoFk<TipoDeFacturaEmtDtm>(modelBuilder, nameof(TipoDeFacturaEmtDtm.Naturaleza), nameof(TipoDeFacturaEmtDtm.IdNaturalezaDefecto), ICampos.ID_NATURALEZA, requerida: false, unico: false);
            modelBuilder.Entity<TipoDeFacturaEmtDtm>().Property(p => p.Vencimiento).HasColumnName(ICampos.VENCIMIENTO).HasColumnType(IDominio.INT).IsRequired(true);
            modelBuilder.Entity<TipoDeFacturaEmtDtm>().Property(nameof(TipoDeFacturaEmtDtm.EsExportacion)).HasColumnName(ICampos.ES_EXPORTACION).HasColumnType(IDominio.BIT).IsRequired(true);
            modelBuilder.Entity<TipoDeFacturaEmtDtm>().Property(nameof(TipoDeFacturaEmtDtm.ConPeriodoEmt)).HasColumnName(ICampos.CON_PERIODO).HasColumnType(IDominio.BIT).IsRequired(true);

        }
        internal static void PlantillaPorTipo(ModelBuilder modelBuilder)
        {
            Elemento.PlantillaPorTipo.DefinirCamposDePlantillaPorTipoDtm<PlantillaPorTipoDeFacturaEmtDtm>(modelBuilder);
        }
    }
}
