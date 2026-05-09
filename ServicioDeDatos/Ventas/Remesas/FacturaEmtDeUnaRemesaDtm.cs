using Microsoft.EntityFrameworkCore;
using Microsoft.SqlServer.Dac.Model;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Utilidades;

namespace ServicioDeDatos.Ventas
{
    [Table(Tablas.REMESA_FAE + "_" + Tablas.FACTURA_EMT, Schema = Esquemas.VENTA)]
    public class FacturaEmtDeUnaRemesaDtm : RelacionDtm, IDetalle, IAuditoria
    {
        public int IdElemento { get; set; }
        public RemesaFaeDtm Elemento { get; set; }
        IElementoDtm IDetalle.Elemento => Elemento;

        public int IdFactura { get; set; }
        public FacturaEmtDtm Factura { get; set; }

        public DateTime? CargadaEl { get; set; }

        public DateTime? FechaMaximaDeDevolucion { get; set; }
        public DateTime? DevueltoEl { get; set; }
        public string Motivo { get; set; }

        public enumNegocio Negocio => enumNegocio.RemesaFae;

        public int IdUsuaCrea { get; set; }
        public DateTime FechaCreacion { get; set; }
        public UsuarioDtm UsuarioCreador { get; set; }

        public int? IdUsuaModi { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public UsuarioDtm UsuarioModificador { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        public FacturaEmtDeUnaRemesaDtm()
        {
            PropiedadDelIdElemento1 = nameof(IdElemento);
            PropiedadDelIdElemento2 = nameof(IdFactura);
        }
    }


    public static partial class ModeloDeRemesaFae
    {

        public static void FacturasEmtDeUnaRemesa(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FacturaEmtDeUnaRemesaDtm>().Ignore(x => x.Negocio);
            ApiDeRegistroDtm.DefinirCampoFk<FacturaEmtDeUnaRemesaDtm>(modelBuilder, nameof(FacturaEmtDeUnaRemesaDtm.Elemento), nameof(FacturaEmtDeUnaRemesaDtm.IdElemento), ICampos.ID_ELEMENTO, requerida: true, unico: false);
            ApiDeRegistroDtm.DefinirCampoFk<FacturaEmtDeUnaRemesaDtm>(modelBuilder, nameof(FacturaEmtDeUnaRemesaDtm.Factura), nameof(FacturaEmtDeUnaRemesaDtm.IdFactura), ICampos.ID_FACTURA_EMT, requerida: true, unico: false);

            modelBuilder.Entity<FacturaEmtDeUnaRemesaDtm>().Property(p => p.Motivo).HasColumnName(ICampos.MOTIVO).HasColumnType(IDominio.VARCHAR_250).IsRequired(false);

            modelBuilder.Entity<FacturaEmtDeUnaRemesaDtm>().Property(p => p.CargadaEl).HasColumnName(ICampos.CARGADA_EL).HasColumnType(IDominio.DATETIME_2).IsRequired(false);
            modelBuilder.Entity<FacturaEmtDeUnaRemesaDtm>().Property(p => p.DevueltoEl).HasColumnName(ICampos.DEVUELTA_EL).HasColumnType(IDominio.DATETIME_2).IsRequired(false);
            modelBuilder.Entity<FacturaEmtDeUnaRemesaDtm>().Property(p => p.FechaMaximaDeDevolucion).HasColumnName(ICampos.MAX_FECDEV).HasColumnType(IDominio.DATETIME_2).IsRequired(false);


            modelBuilder.Entity<FacturaEmtDeUnaRemesaDtm>().HasAlternateKey(x => new { x.IdElemento, x.IdFactura}).HasName($"AK_{Tablas.REMESA_FAE}_{Tablas.FACTURA_EMT}_{ICampos.ID_ELEMENTO}_{ICampos.ID_FACTURA_EMT}");

            ApiDeElementoDtm.DefinirCamposDeAuditoria<FacturaEmtDeUnaRemesaDtm>(modelBuilder);
        }
    }
}
