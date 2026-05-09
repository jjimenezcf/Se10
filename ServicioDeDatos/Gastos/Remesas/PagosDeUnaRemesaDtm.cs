using Microsoft.EntityFrameworkCore;
using Microsoft.SqlServer.Dac.Model;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Ventas;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Utilidades;

namespace ServicioDeDatos.Gastos
{
    [Table(Tablas.REMESA_PAG + "_" + Tablas.PAGO, Schema = Esquemas.GASTO)]
    public class PagoDeUnaRemesaDtm : RelacionDtm, IDetalle, IAuditoria
    {
        public int IdElemento { get; set; }
        public RemesaPagDtm Elemento { get; set; }
        IElementoDtm IDetalle.Elemento => Elemento;

        
        public int IdPago { get; set; }
        public PagoDtm Pago { get; set; }

        public DateTime? PagarEl { get; set; }

        public DateTime? PagadoEl { get; set; }

        public DateTime? AnuladoEl { get; set; }

        public string Motivo { get; set; }

        public enumNegocio Negocio => enumNegocio.RemesaPag;

        public int IdUsuaCrea { get; set; }
        public DateTime FechaCreacion { get; set; }
        public UsuarioDtm UsuarioCreador { get; set; }

        public int? IdUsuaModi { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public UsuarioDtm UsuarioModificador { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        public PagoDeUnaRemesaDtm()
        {
            PropiedadDelIdElemento1 = nameof(IdElemento);
            PropiedadDelIdElemento2 = nameof(IdPago);
        }
    }


    public static partial class ModeloDeRemesaPag
    {

        public static void PagosDeUnaRemesa(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PagoDeUnaRemesaDtm>().Ignore(x => x.Negocio);
            ApiDeRegistroDtm.DefinirCampoFk<PagoDeUnaRemesaDtm>(modelBuilder, nameof(PagoDeUnaRemesaDtm.Elemento), nameof(PagoDeUnaRemesaDtm.IdElemento), ICampos.ID_ELEMENTO, requerida: true, unico: false);
            ApiDeRegistroDtm.DefinirCampoFk<PagoDeUnaRemesaDtm>(modelBuilder, nameof(PagoDeUnaRemesaDtm.Pago), nameof(PagoDeUnaRemesaDtm.IdPago), ICampos.ID_PAGO, requerida: true, unico: false);

            modelBuilder.Entity<PagoDeUnaRemesaDtm>().Property(p => p.Motivo).HasColumnName(ICampos.MOTIVO).HasColumnType(IDominio.VARCHAR_250).IsRequired(false);

            modelBuilder.Entity<PagoDeUnaRemesaDtm>().Property(p => p.PagarEl).HasColumnName(ICampos.PAGAR_EL).HasColumnType(IDominio.DATETIME_2).IsRequired(false);
            modelBuilder.Entity<PagoDeUnaRemesaDtm>().Property(p => p.PagadoEl).HasColumnName(ICampos.PAGADO_EL).HasColumnType(IDominio.DATETIME_2).IsRequired(false);
            modelBuilder.Entity<PagoDeUnaRemesaDtm>().Property(p => p.AnuladoEl).HasColumnName(ICampos.ANULADO_EL).HasColumnType(IDominio.DATETIME_2).IsRequired(false);

            modelBuilder.Entity<PagoDeUnaRemesaDtm>().HasAlternateKey(x => new { x.IdElemento, x.IdPago}).HasName($"AK_{Tablas.REMESA_PAG}_{Tablas.PAGO}_{ICampos.ID_ELEMENTO}_{ICampos.ID_PAGO}");

            ApiDeElementoDtm.DefinirCamposDeAuditoria<PagoDeUnaRemesaDtm>(modelBuilder);
        }
    }
}
