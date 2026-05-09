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

namespace ServicioDeDatos.Gastos
{

    [Table(Tablas.FACTURA_REC + "_" + Sufijo.LINEA, Schema = Esquemas.GASTO)]
    public class LineaDeUnaFarDtm : RegistroDtm, IDetalle, IAuditoria
    {
        public int IdElemento { get; set; }
        public FacturaRecDtm Elemento { get; set; }
        IElementoDtm IDetalle.Elemento => Elemento;

        public int Orden { get; set; }
        public enumClaseDeLineaFar Clase { get; set; }
        public string Concepto { get; set; }
        public int? IdNaturaleza { get; set; }
        public NaturalezaDtm Naturaleza { get; set; }

        public int? IdIvaS { get; set; }
        public IvaSoportadoDtm IvaSoportado { get; set; }
        public decimal? PorcentajeIva { get; set; }

        public int? IdIrpf { get; set; }
        public IrpfDtm Irpf { get; set; }
        public decimal? PorcentajeIrpf { get; set; }

        public decimal BaseImponible { get; set; }
        public decimal? Cantidad { get; set; }
        public int? IdUnidad { get; set; }
        public UnidadDtm Unidad { get; set; }

        public decimal? ImporteDeIva => 
        Clase == enumClaseDeLineaFar.BaseImponible || Clase == enumClaseDeLineaFar.LineaDeIrpf
        ? null
        : BaseImponible * (PorcentajeIva is null ? 0 : (decimal)PorcentajeIva / 100);

        public decimal? ImporteDeIrpf =>
        Clase == enumClaseDeLineaFar.BaseImponible || Clase == enumClaseDeLineaFar.LineaDeIva || Clase == enumClaseDeLineaFar.BiConIva || Clase == enumClaseDeLineaFar.BiExenta
        ? null
        : BaseImponible * (PorcentajeIrpf is null ? 0 : (decimal)PorcentajeIrpf / 100);

        public enumNegocio Negocio => enumNegocio.FacturaRecibida;

        public int IdUsuaCrea { get; set; }
        public DateTime FechaCreacion { get; set; }
        public UsuarioDtm UsuarioCreador { get; set; }

        public int? IdUsuaModi { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public UsuarioDtm UsuarioModificador { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }
    }

    public static partial class ModeloDeFacturaRec
    {
        internal static void DatosDeLineaDeUnaFacturaRec(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LineaDeUnaFarDtm>().Ignore(x => x.Negocio);
            modelBuilder.Entity<LineaDeUnaFarDtm>().Ignore(x => x.Elemento);
            modelBuilder.Entity<LineaDeUnaFarDtm>().Ignore(x => x.ImporteDeIva);
            modelBuilder.Entity<LineaDeUnaFarDtm>().Ignore(x => x.ImporteDeIrpf);

            ApiDeRegistroDtm.DefinirCampoFk<LineaDeUnaFarDtm>(modelBuilder, nameof(LineaDeUnaFarDtm.Elemento), nameof(LineaDeUnaFarDtm.IdElemento), ICampos.ID_ELEMENTO, requerida: true, unico: false);

            modelBuilder.Entity<LineaDeUnaFarDtm>().Property(nameof(LineaDeUnaFarDtm.Orden)).HasColumnName(ICampos.ORDEN).HasColumnType(IDominio.INT).IsRequired(true);
            modelBuilder.Entity<LineaDeUnaFarDtm>().Property(nameof(LineaDeUnaFarDtm.Clase)).HasColumnName(ICampos.CLASE).HasColumnType(IDominio.VARCHAR_30).IsRequired(true);

            modelBuilder.Entity<LineaDeUnaFarDtm>().Property(nameof(LineaDeUnaFarDtm.Concepto)).HasColumnName(ICampos.CONCEPTO).HasColumnType(IDominio.VARCHAR_250).IsRequired(true);
            modelBuilder.Entity<LineaDeUnaFarDtm>().Property(nameof(LineaDeUnaFarDtm.BaseImponible)).HasColumnName(ICampos.BI).HasColumnType(IDominio.DECIMAL).IsRequired(true);

            ApiDeRegistroDtm.DefinirCampoFk<LineaDeUnaFarDtm>(modelBuilder, nameof(LineaDeUnaFarDtm.IvaSoportado), nameof(LineaDeUnaFarDtm.IdIvaS), ICampos.ID_IVA_S, requerida: false, unico: false);
            modelBuilder.Entity<LineaDeUnaFarDtm>().Property(nameof(LineaDeUnaFarDtm.PorcentajeIva)).HasColumnName(ICampos.IVA).HasColumnType(IDominio.PORCENTAJE_MENOR_100).IsRequired(false);

            ApiDeRegistroDtm.DefinirCampoFk<LineaDeUnaFarDtm>(modelBuilder, nameof(LineaDeUnaFarDtm.Irpf), nameof(LineaDeUnaFarDtm.IdIrpf), ICampos.ID_IRPF, requerida: false, unico: false);
            modelBuilder.Entity<LineaDeUnaFarDtm>().Property(nameof(LineaDeUnaFarDtm.PorcentajeIrpf)).HasColumnName(ICampos.IRPF).HasColumnType(IDominio.PORCENTAJE_MENOR_100).IsRequired(false);

            ApiDeRegistroDtm.DefinirCampoFk<LineaDeUnaFarDtm>(modelBuilder, nameof(LineaDeUnaFarDtm.Naturaleza), nameof(LineaDeUnaFarDtm.IdNaturaleza), ICampos.ID_NATURALEZA, requerida: false, unico: false);

            modelBuilder.Entity<LineaDeUnaFarDtm>().Property(nameof(LineaDeUnaFarDtm.Cantidad)).HasColumnName(ICampos.CANTIDAD).HasColumnType(IDominio.DECIMAL).IsRequired(false);
            ApiDeRegistroDtm.DefinirCampoFk<LineaDeUnaFarDtm>(modelBuilder, nameof(LineaDeUnaFarDtm.Unidad), nameof(LineaDeUnaFarDtm.IdUnidad), ICampos.ID_UNIDAD, requerida: false, unico: false);

            ApiDeElementoDtm.DefinirCamposDeAuditoria<LineaDeUnaFarDtm>(modelBuilder);

        }

    }
}
