using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Gestor.Errores;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.MaestrosTecnico;
using Utilidades;
using static ServicioDeDatos.Elemento.Enumerados;

namespace ServicioDeDatos.Ventas
{

    [Table(Tablas.FACTURA_EMT + "_" + Sufijo.LINEA, Schema = Esquemas.VENTA)]
    public class LineaDeUnaFaeDtm : RegistroDtm, IDetalle, IAuditoria
    {
        public int IdElemento { get; set; }
        public FacturaEmtDtm Elemento { get; set; }
        IElementoDtm IDetalle.Elemento => Elemento;

        public int Orden { get; set; }
        public enumTipoDeLinea TipoDeLinea { get; set; }
        public int? IdUnitario { get; set; }
        public UnitarioDtm Unitario { get; set; }
        public string Concepto { get; set; }
        public decimal? Cantidad { get; set; }
        public decimal? Precio { get; set; }
        public string Anotacion { get; set; }
        public decimal? Descuento { get; set; }

        public int? IdIvaR { get; set; }
        public IvaRepercutidoDtm IvaRepercutido { get; set; }
        public decimal? Iva { get; set; }
        public decimal ImporteSinDto => (Precio is null || Cantidad is null) ? 0 : (decimal)Precio * (decimal)Cantidad;
        public decimal ImporteDeDto => ImporteSinDto * (Descuento is null ? 0 : (decimal)Descuento / 100);
        public decimal ImporteConDto => ImporteSinDto - ImporteDeDto;

        public decimal ImporteDeIva => ImporteConDto * (Iva is null ? 0 : (decimal)Iva / 100);
        public decimal? ImporteDeLinea => (Precio is null || Cantidad is null) ? null : ImporteConDto + ImporteDeIva;

        public enumClaseUnitario? Clase { get; set; }
        public int? IdUnidad { get; set; }
        public int? IdNaturaleza { get; set; }

        public UnidadDtm Unidad { get; set; }
        public NaturalezaDtm Naturaleza { get; set; }
        public int? IdParteTr { get; set; }
        public ParteTrDtm ParteTr { get; set; }

        public enumNegocio Negocio => enumNegocio.FacturaEmitida;

        public int IdUsuaCrea { get; set; }
        public DateTime FechaCreacion { get; set; }
        public UsuarioDtm UsuarioCreador { get; set; }

        public int? IdUsuaModi { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public UsuarioDtm UsuarioModificador { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        public void ValidarIva(string referenciaFae)
        {
            if (TipoDeLinea != enumTipoDeLinea.Comentario && Iva is null)
                GestorDeErrores.Emitir($"Debe definir el iva del {enumNegocio.FacturaEmitida.Singular(true)} '{referenciaFae}' para la línea '{Concepto}'");
        }
    }

    public static partial class ModeloDeFacturaEmt
    {
        internal static void DatosDeLineaDeUnaFacturaEmt(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LineaDeUnaFaeDtm>().Ignore(x => x.Negocio);
            modelBuilder.Entity<LineaDeUnaFaeDtm>().Ignore(x => x.Elemento);
            modelBuilder.Entity<LineaDeUnaFaeDtm>().Ignore(x => x.ImporteSinDto);
            modelBuilder.Entity<LineaDeUnaFaeDtm>().Ignore(x => x.ImporteDeDto);
            modelBuilder.Entity<LineaDeUnaFaeDtm>().Ignore(x => x.ImporteConDto);
            modelBuilder.Entity<LineaDeUnaFaeDtm>().Ignore(x => x.ImporteDeIva);
            modelBuilder.Entity<LineaDeUnaFaeDtm>().Ignore(x => x.ImporteDeLinea);

            modelBuilder.Entity<LineaDeUnaFaeDtm>().Property(nameof(LineaDeUnaFaeDtm.IdElemento)).HasColumnName(ICampos.ID_ELEMENTO).HasColumnType(IDominio.INT).IsRequired(true);
            ApiDeRegistroDtm.DefinirFk<LineaDeUnaFaeDtm, FacturaEmtDtm>(modelBuilder, nameof(LineaDeUnaFaeDtm.IdElemento), ICampos.ID_ELEMENTO, unico: false);
            modelBuilder.Entity<LineaDeUnaFaeDtm>().Property(nameof(LineaDeUnaFaeDtm.Orden)).HasColumnName(ICampos.ORDEN).HasColumnType(IDominio.INT).IsRequired(true);
            modelBuilder.Entity<LineaDeUnaFaeDtm>().Property(nameof(LineaDeUnaFaeDtm.TipoDeLinea)).HasColumnName(ICampos.TIPO_LINEA).HasColumnType(IDominio.VARCHAR_30).IsRequired(true);

            ApiDeRegistroDtm.DefinirCampoFk<LineaDeUnaFaeDtm>(modelBuilder, nameof(LineaDeUnaFaeDtm.Unitario), nameof(LineaDeUnaFaeDtm.IdUnitario), ICampos.ID_UNITARIO, requerida: false, unico: false);
            ApiDeRegistroDtm.DefinirCampoFk<LineaDeUnaFaeDtm>(modelBuilder, nameof(LineaDeUnaFaeDtm.IvaRepercutido), nameof(LineaDeUnaFaeDtm.IdIvaR), ICampos.ID_IVA_R, requerida: false, unico: false);

            modelBuilder.Entity<LineaDeUnaFaeDtm>().Property(nameof(LineaDeUnaFaeDtm.Concepto)).HasColumnName(ICampos.CONCEPTO).HasColumnType(IDominio.VARCHAR_250).IsRequired(true);
            modelBuilder.Entity<LineaDeUnaFaeDtm>().Property(nameof(LineaDeUnaFaeDtm.Anotacion)).HasColumnName(ICampos.ANOTACION).HasColumnType(IDominio.VARCHAR_2000).IsRequired(false);

            modelBuilder.Entity<LineaDeUnaFaeDtm>().Property(nameof(LineaDeUnaFaeDtm.Cantidad)).HasColumnName(ICampos.CANTIDAD).HasColumnType(IDominio.DECIMAL).IsRequired(false);
            modelBuilder.Entity<LineaDeUnaFaeDtm>().Property(nameof(LineaDeUnaFaeDtm.Precio)).HasColumnName(ICampos.PRECIO).HasColumnType(IDominio.DECIMAL).IsRequired(false);
            modelBuilder.Entity<LineaDeUnaFaeDtm>().Property(nameof(LineaDeUnaFaeDtm.Iva)).HasColumnName(ICampos.IVA).HasColumnType(IDominio.PORCENTAJE_MENOR_100).IsRequired(false);
            modelBuilder.Entity<LineaDeUnaFaeDtm>().Property(nameof(LineaDeUnaFaeDtm.Descuento)).HasColumnName(ICampos.DESCUENTO).HasColumnType(IDominio.PORCENTAJE_MENOR_100).IsRequired(false);

            modelBuilder.Entity<LineaDeUnaFaeDtm>().Property(p => p.Clase).HasColumnName(ICampos.CLASE).HasColumnType(IDominio.VARCHAR_30).IsRequired(false);
            ApiDeRegistroDtm.DefinirCampoFk<LineaDeUnaFaeDtm>(modelBuilder, nameof(LineaDeUnaFaeDtm.Unidad), nameof(LineaDeUnaFaeDtm.IdUnidad), ICampos.ID_UNIDAD, requerida: false, unico: false);
            ApiDeRegistroDtm.DefinirCampoFk<LineaDeUnaFaeDtm>(modelBuilder, nameof(LineaDeUnaFaeDtm.Naturaleza), nameof(LineaDeUnaFaeDtm.IdNaturaleza), ICampos.ID_NATURALEZA, requerida: false, unico: false);


            ApiDeRegistroDtm.DefinirDependencia<LineaDeUnaFaeDtm, ParteTrDtm>(modelBuilder, apuntadoPor: nameof(LineaDeUnaFaeDtm.IdParteTr), idCampo: ICampos.ID_PARTE_TR, requerido: false);

            ApiDeElementoDtm.DefinirCamposDeAuditoria<LineaDeUnaFaeDtm>(modelBuilder);

        }

    }
}
