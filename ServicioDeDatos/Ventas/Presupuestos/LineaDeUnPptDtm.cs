using System.ComponentModel.DataAnnotations.Schema;
using Gestor.Errores;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.MaestrosTecnico;
using Utilidades;
using static ServicioDeDatos.Elemento.Enumerados;

namespace ServicioDeDatos.Presupuesto
{

    [Table(Tablas.PRESUPUESTO + "_" + nameof(Sufijo.LINEA), Schema = Esquemas.PRESUPUESTO)]
    public class LineaDeUnPptDtm: RegistroDtm, IDetalle
    {
        public int IdElemento { get; set; }
        public PresupuestoDtm Elemento { get; set; }
        IElementoDtm IDetalle.Elemento => Elemento;

        public int Orden { get; set; }
        public enumTipoDeLinea TipoDeLinea { get; set; }
        public int ? IdUnitario { get; set; }
        public UnitarioDtm Unitario { get; set; }
        public string Concepto { get; set; }
        public decimal? Cantidad { get; set; }
        public decimal? Precio { get; set; }
        public string Anotacion { get; set; }
        public decimal ? Descuento { get; set; }
        public int ? IdIvaR { get; set; }
        public IvaRepercutidoDtm IvaRepercutido { get; set; }
        public decimal ? Iva { get; set; }
        public decimal ImporteSinDto => (Precio == null || Cantidad == null) ? 0 : (decimal)Precio * (decimal)Cantidad;
        public decimal ImporteDeDto=> ImporteSinDto * (Descuento == null ? 0 : (decimal)Descuento / 100);
        public decimal ImporteConDto => ImporteSinDto - ImporteDeDto;

        public decimal ImporteDeIva => ImporteConDto * (Iva == null ? 0 : (decimal)Iva / 100);
        public decimal? ImporteDeLinea => (Precio == null || Cantidad == null) ? null: ImporteConDto + ImporteDeIva;

        public enumClaseUnitario? Clase { get; set; }
        public int? IdUnidad { get; set; }
        public int? IdNaturaleza { get; set; }

        public UnidadDtm Unidad { get; set; }
        public NaturalezaDtm Naturaleza { get; set; }

        public enumNegocio Negocio => enumNegocio.Presupuesto;

        public void ValidarIva(string referenciaPpt)
        {
            if (TipoDeLinea != enumTipoDeLinea.Comentario && Iva == null)
                GestorDeErrores.Emitir($"Debe definir el iva del {enumNegocio.Presupuesto.Singular(true)} '{referenciaPpt}' para la línea '{Concepto}'");
        }
    }

    public static partial class ModeloDePresupuesto
    {
        internal static void DatosDeLineaDeUnPpt(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LineaDeUnPptDtm>().Ignore(x => x.Negocio);
            modelBuilder.Entity<LineaDeUnPptDtm>().Ignore(x => x.Elemento);
            modelBuilder.Entity<LineaDeUnPptDtm>().Ignore(x => x.ImporteSinDto);
            modelBuilder.Entity<LineaDeUnPptDtm>().Ignore(x => x.ImporteDeDto);
            modelBuilder.Entity<LineaDeUnPptDtm>().Ignore(x => x.ImporteConDto);
            modelBuilder.Entity<LineaDeUnPptDtm>().Ignore(x => x.ImporteDeIva);
            modelBuilder.Entity<LineaDeUnPptDtm>().Ignore(x => x.ImporteDeLinea);

            modelBuilder.Entity<LineaDeUnPptDtm>().Property(nameof(LineaDeUnPptDtm.IdElemento)).HasColumnName(ICampos.ID_ELEMENTO).HasColumnType(IDominio.INT).IsRequired(true);
            ApiDeRegistroDtm.DefinirFk<LineaDeUnPptDtm, PresupuestoDtm>(modelBuilder, nameof(LineaDeUnPptDtm.IdElemento), ICampos.ID_ELEMENTO, unico: false);
            modelBuilder.Entity<LineaDeUnPptDtm>().Property(nameof(LineaDeUnPptDtm.Orden)).HasColumnName(ICampos.ORDEN).HasColumnType(IDominio.INT).IsRequired(true);
            modelBuilder.Entity<LineaDeUnPptDtm>().Property(nameof(LineaDeUnPptDtm.TipoDeLinea)).HasColumnName(ICampos.TIPO_LINEA).HasColumnType(IDominio.VARCHAR_30).IsRequired(true);

            ApiDeRegistroDtm.DefinirCampoFk<LineaDeUnPptDtm>(modelBuilder, nameof(LineaDeUnPptDtm.Unitario), nameof(LineaDeUnPptDtm.IdUnitario), ICampos.ID_UNITARIO, requerida: false, unico: false);
            ApiDeRegistroDtm.DefinirCampoFk<LineaDeUnPptDtm>(modelBuilder, nameof(LineaDeUnPptDtm.IvaRepercutido), nameof(LineaDeUnPptDtm.IdIvaR), ICampos.ID_IVA_R, requerida: false, unico: false);

            modelBuilder.Entity<LineaDeUnPptDtm>().Property(nameof(LineaDeUnPptDtm.Concepto)).HasColumnName(ICampos.CONCEPTO).HasColumnType(IDominio.VARCHAR_250).IsRequired(true);
            modelBuilder.Entity<LineaDeUnPptDtm>().Property(nameof(LineaDeUnPptDtm.Anotacion)).HasColumnName(ICampos.ANOTACION).HasColumnType(IDominio.VARCHAR_2000).IsRequired(false);

            modelBuilder.Entity<LineaDeUnPptDtm>().Property(nameof(LineaDeUnPptDtm.Cantidad)).HasColumnName(ICampos.CANTIDAD).HasColumnType(IDominio.DECIMAL).IsRequired(false);
            modelBuilder.Entity<LineaDeUnPptDtm>().Property(nameof(LineaDeUnPptDtm.Precio)).HasColumnName(ICampos.PRECIO).HasColumnType(IDominio.DECIMAL).IsRequired(false);
            modelBuilder.Entity<LineaDeUnPptDtm>().Property(nameof(LineaDeUnPptDtm.Iva)).HasColumnName(ICampos.IVA).HasColumnType(IDominio.PORCENTAJE_MENOR_100).IsRequired(false);
            modelBuilder.Entity<LineaDeUnPptDtm>().Property(nameof(LineaDeUnPptDtm.Descuento)).HasColumnName(ICampos.DESCUENTO).HasColumnType(IDominio.PORCENTAJE_MENOR_100).IsRequired(false);

            modelBuilder.Entity<LineaDeUnPptDtm>().Property(p => p.Clase).HasColumnName(ICampos.CLASE).HasColumnType(IDominio.VARCHAR_30).IsRequired(false);
            ApiDeRegistroDtm.DefinirCampoFk<LineaDeUnPptDtm>(modelBuilder, nameof(LineaDeUnPptDtm.Unidad), nameof(LineaDeUnPptDtm.IdUnidad), ICampos.ID_UNIDAD, requerida: false, unico: false);
            ApiDeRegistroDtm.DefinirCampoFk<LineaDeUnPptDtm>(modelBuilder, nameof(LineaDeUnPptDtm.Naturaleza), nameof(LineaDeUnPptDtm.IdNaturaleza), ICampos.ID_NATURALEZA, requerida: false, unico: false);

        }

    }
}
