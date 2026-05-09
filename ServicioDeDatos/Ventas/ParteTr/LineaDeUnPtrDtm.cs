using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.MaestrosTecnico;
using Utilidades;
using static ServicioDeDatos.Elemento.Enumerados;

namespace ServicioDeDatos.Ventas
{

    [Table(Tablas.PARTE_TR + "_" + nameof(Sufijo.LINEA), Schema = Esquemas.VENTA)]
    public class LineaDeUnPtrDtm: RegistroDtm, IDetalle, IAuditoria
    {
        public int IdElemento { get; set; }
        public ParteTrDtm Elemento { get; set; }
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

        public enumNegocio Negocio => enumNegocio.ParteDeTrabajo;

        public int IdUsuaCrea { get; set; }
        public DateTime FechaCreacion { get; set; }
        public UsuarioDtm UsuarioCreador { get; set; }

        public int? IdUsuaModi { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public UsuarioDtm UsuarioModificador { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }
    }

    public static partial class ModeloDeParteTr
    {
        internal static void DatosDeLineaDeUnParteTr(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LineaDeUnPtrDtm>().Ignore(x => x.Negocio);
            modelBuilder.Entity<LineaDeUnPtrDtm>().Ignore(x => x.Elemento);
            modelBuilder.Entity<LineaDeUnPtrDtm>().Ignore(x => x.ImporteSinDto);
            modelBuilder.Entity<LineaDeUnPtrDtm>().Ignore(x => x.ImporteDeDto);
            modelBuilder.Entity<LineaDeUnPtrDtm>().Ignore(x => x.ImporteConDto);
            modelBuilder.Entity<LineaDeUnPtrDtm>().Ignore(x => x.ImporteDeIva);
            modelBuilder.Entity<LineaDeUnPtrDtm>().Ignore(x => x.ImporteDeLinea);

            modelBuilder.Entity<LineaDeUnPtrDtm>().Property(nameof(LineaDeUnPtrDtm.IdElemento)).HasColumnName(ICampos.ID_ELEMENTO).HasColumnType(IDominio.INT).IsRequired(true);
            ApiDeRegistroDtm.DefinirFk<LineaDeUnPtrDtm, ParteTrDtm>(modelBuilder, nameof(LineaDeUnPtrDtm.IdElemento), ICampos.ID_ELEMENTO, unico: false);
            modelBuilder.Entity<LineaDeUnPtrDtm>().Property(nameof(LineaDeUnPtrDtm.Orden)).HasColumnName(ICampos.ORDEN).HasColumnType(IDominio.INT).IsRequired(true);
            modelBuilder.Entity<LineaDeUnPtrDtm>().Property(nameof(LineaDeUnPtrDtm.TipoDeLinea)).HasColumnName(ICampos.TIPO_LINEA).HasColumnType(IDominio.VARCHAR_30).IsRequired(true);

            ApiDeRegistroDtm.DefinirCampoFk<LineaDeUnPtrDtm>(modelBuilder, nameof(LineaDeUnPtrDtm.Unitario), nameof(LineaDeUnPtrDtm.IdUnitario), ICampos.ID_UNITARIO, requerida: false, unico: false);
            ApiDeRegistroDtm.DefinirCampoFk<LineaDeUnPtrDtm>(modelBuilder, nameof(LineaDeUnPtrDtm.IvaRepercutido), nameof(LineaDeUnPtrDtm.IdIvaR), ICampos.ID_IVA_R, requerida: false, unico: false);

            modelBuilder.Entity<LineaDeUnPtrDtm>().Property(nameof(LineaDeUnPtrDtm.Concepto)).HasColumnName(ICampos.CONCEPTO).HasColumnType(IDominio.VARCHAR_250).IsRequired(true);
            modelBuilder.Entity<LineaDeUnPtrDtm>().Property(nameof(LineaDeUnPtrDtm.Anotacion)).HasColumnName(ICampos.ANOTACION).HasColumnType(IDominio.VARCHAR_2000).IsRequired(false);

            modelBuilder.Entity<LineaDeUnPtrDtm>().Property(nameof(LineaDeUnPtrDtm.Cantidad)).HasColumnName(ICampos.CANTIDAD).HasColumnType(IDominio.DECIMAL).IsRequired(false);
            modelBuilder.Entity<LineaDeUnPtrDtm>().Property(nameof(LineaDeUnPtrDtm.Precio)).HasColumnName(ICampos.PRECIO).HasColumnType(IDominio.DECIMAL).IsRequired(false);
            modelBuilder.Entity<LineaDeUnPtrDtm>().Property(nameof(LineaDeUnPtrDtm.Iva)).HasColumnName(ICampos.IVA).HasColumnType(IDominio.PORCENTAJE_MENOR_100).IsRequired(false);
            modelBuilder.Entity<LineaDeUnPtrDtm>().Property(nameof(LineaDeUnPtrDtm.Descuento)).HasColumnName(ICampos.DESCUENTO).HasColumnType(IDominio.PORCENTAJE_MENOR_100).IsRequired(false);

            modelBuilder.Entity<LineaDeUnPtrDtm>().Property(p => p.Clase).HasColumnName(ICampos.CLASE).HasColumnType(IDominio.VARCHAR_30).IsRequired(false);
            ApiDeRegistroDtm.DefinirCampoFk<LineaDeUnPtrDtm>(modelBuilder, nameof(LineaDeUnPtrDtm.Unidad), nameof(LineaDeUnPtrDtm.IdUnidad), ICampos.ID_UNIDAD, requerida: false, unico: false);
            ApiDeRegistroDtm.DefinirCampoFk<LineaDeUnPtrDtm>(modelBuilder, nameof(LineaDeUnPtrDtm.Naturaleza), nameof(LineaDeUnPtrDtm.IdNaturaleza), ICampos.ID_NATURALEZA, requerida: false, unico: false);

            ApiDeElementoDtm.DefinirCamposDeAuditoria<LineaDeUnPtrDtm>(modelBuilder);

        }

    }
}
