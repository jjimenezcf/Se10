using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.MaestrosTecnico;
using System.ComponentModel.DataAnnotations.Schema;
using Utilidades;
using static ServicioDeDatos.Elemento.Enumerados;

namespace ServicioDeDatos.Ventas
{
    [Table(Tablas.PLANIFICACION_VENTA + "_" + nameof(Sufijo.LINEA), Schema = Esquemas.VENTA)]
    public class LineaDeUnaPlfVentaDtm : RegistroDtm, IDetalle
    {
        public int IdElemento { get; set; }
        public PlanificacionDeVentaDtm Elemento { get; set; }
        IElementoDtm IDetalle.Elemento => Elemento;

        public int Orden { get; set; }
        public enumTipoDeLinea TipoDeLinea { get; set; }
        public int? IdUnitario { get; set; }
        public UnitarioDtm Unitario { get; set; }
        public string Concepto { get; set; }
        public decimal? Cantidad { get; set; }
        public decimal? Coste { get; set; }
        public decimal? Venta { get; set; }
        public string Anotacion { get; set; }
        public decimal ? Descuento { get; set; }
        public int? IdIvaR { get; set; }
        public IvaRepercutidoDtm IvaRepercutido { get; set; }
        public decimal? Iva { get; set; }
        public decimal ImporteSinDto => Venta.HasValue && Cantidad.HasValue ? (decimal)Venta * (decimal)Cantidad : 0;
        public decimal ImporteDeDto => ImporteSinDto * (Descuento == null ? 0 : (decimal)Descuento / 100);
        public decimal ImporteConDto => ImporteSinDto - ImporteDeDto;

        public decimal ImporteDeIva => Iva.HasValue ? ImporteConDto * (decimal)Iva / 100 : 0;
        public decimal ImporteDeLinea => ImporteConDto + ImporteDeIva;

        public enumClaseUnitario? Clase { get; set; }
        public int? IdUnidad { get; set; }
        public int? IdNaturaleza { get; set; }

        public UnidadDtm Unidad { get; set; }
        public NaturalezaDtm Naturaleza { get; set; }

        public enumNegocio Negocio => enumNegocio.PlanificacionDeVenta;
    }

    public static partial class ModeloDePlanificacionDeVenta
    {
        internal static void DatosDeLineaDeUnaPlfVenta(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LineaDeUnaPlfVentaDtm>().Ignore(x => x.Negocio);
            modelBuilder.Entity<LineaDeUnaPlfVentaDtm>().Ignore(x => x.Elemento);
            modelBuilder.Entity<LineaDeUnaPlfVentaDtm>().Ignore(x => x.ImporteSinDto);
            modelBuilder.Entity<LineaDeUnaPlfVentaDtm>().Ignore(x => x.ImporteDeDto);
            modelBuilder.Entity<LineaDeUnaPlfVentaDtm>().Ignore(x => x.ImporteConDto);
            modelBuilder.Entity<LineaDeUnaPlfVentaDtm>().Ignore(x => x.ImporteDeIva);
            modelBuilder.Entity<LineaDeUnaPlfVentaDtm>().Ignore(x => x.ImporteDeLinea);

            modelBuilder.Entity<LineaDeUnaPlfVentaDtm>().Property(nameof(LineaDeUnaPlfVentaDtm.IdElemento)).HasColumnName(ICampos.ID_ELEMENTO).HasColumnType(IDominio.INT).IsRequired(true);
            ApiDeRegistroDtm.DefinirFk<LineaDeUnaPlfVentaDtm, PlanificacionDeVentaDtm>(modelBuilder, nameof(LineaDeUnaPlfVentaDtm.IdElemento), ICampos.ID_ELEMENTO, unico: false);
            modelBuilder.Entity<LineaDeUnaPlfVentaDtm>().Property(nameof(LineaDeUnaPlfVentaDtm.Orden)).HasColumnName(ICampos.ORDEN).HasColumnType(IDominio.INT).IsRequired(true);
            modelBuilder.Entity<LineaDeUnaPlfVentaDtm>().Property(nameof(LineaDeUnaPlfVentaDtm.TipoDeLinea)).HasColumnName(ICampos.TIPO_LINEA).HasColumnType(IDominio.VARCHAR_30).IsRequired(true)
                .HasDefaultValue(enumTipoDeLinea.Unitario);


            ApiDeRegistroDtm.DefinirCampoFk<LineaDeUnaPlfVentaDtm>(modelBuilder, nameof(LineaDeUnaPlfVentaDtm.Unitario), nameof(LineaDeUnaPlfVentaDtm.IdUnitario), ICampos.ID_UNITARIO, requerida: false, unico: false);
            ApiDeRegistroDtm.DefinirCampoFk<LineaDeUnaPlfVentaDtm>(modelBuilder, nameof(LineaDeUnaPlfVentaDtm.IvaRepercutido), nameof(LineaDeUnaPlfVentaDtm.IdIvaR), ICampos.ID_IVA_R, requerida: false, unico: false);

            modelBuilder.Entity<LineaDeUnaPlfVentaDtm>().Property(nameof(LineaDeUnaPlfVentaDtm.Concepto)).HasColumnName(ICampos.CONCEPTO).HasColumnType(IDominio.VARCHAR_250).IsRequired(true);
            modelBuilder.Entity<LineaDeUnaPlfVentaDtm>().Property(nameof(LineaDeUnaPlfVentaDtm.Anotacion)).HasColumnName(ICampos.ANOTACION).HasColumnType(IDominio.VARCHAR_2000).IsRequired(false);

            modelBuilder.Entity<LineaDeUnaPlfVentaDtm>().Property(nameof(LineaDeUnaPlfVentaDtm.Cantidad)).HasColumnName(ICampos.CANTIDAD).HasColumnType(IDominio.DECIMAL).IsRequired(false);
            modelBuilder.Entity<LineaDeUnaPlfVentaDtm>().Property(nameof(LineaDeUnaPlfVentaDtm.Coste)).HasColumnName(ICampos.COSTE).HasColumnType(IDominio.DECIMAL).IsRequired(false);
            modelBuilder.Entity<LineaDeUnaPlfVentaDtm>().Property(nameof(LineaDeUnaPlfVentaDtm.Venta)).HasColumnName(ICampos.PRECIO).HasColumnType(IDominio.DECIMAL).IsRequired(false);
            modelBuilder.Entity<LineaDeUnaPlfVentaDtm>().Property(nameof(LineaDeUnaPlfVentaDtm.Iva)).HasColumnName(ICampos.IVA).HasColumnType(IDominio.PORCENTAJE_MENOR_100).IsRequired(false);
            modelBuilder.Entity<LineaDeUnaPlfVentaDtm>().Property(nameof(LineaDeUnaPlfVentaDtm.Descuento)).HasColumnName(ICampos.DESCUENTO).HasColumnType(IDominio.PORCENTAJE_MENOR_100).IsRequired(false);

            modelBuilder.Entity<LineaDeUnaPlfVentaDtm>().Property(p => p.Clase).HasColumnName(ICampos.CLASE).HasColumnType(IDominio.VARCHAR_30).IsRequired(false);
            ApiDeRegistroDtm.DefinirCampoFk<LineaDeUnaPlfVentaDtm>(modelBuilder, nameof(LineaDeUnaPlfVentaDtm.Unidad), nameof(LineaDeUnaPlfVentaDtm.IdUnidad), ICampos.ID_UNIDAD, requerida: false, unico: false);
            ApiDeRegistroDtm.DefinirCampoFk<LineaDeUnaPlfVentaDtm>(modelBuilder, nameof(LineaDeUnaPlfVentaDtm.Naturaleza), nameof(LineaDeUnaPlfVentaDtm.IdNaturaleza), ICampos.ID_NATURALEZA, requerida: false, unico: false);
        }

    }
}
