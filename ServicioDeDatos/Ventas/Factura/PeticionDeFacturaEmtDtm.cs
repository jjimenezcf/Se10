using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Terceros;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServicioDeDatos.Ventas
{
    public enum enumOperacionFacturador
    {
        [Description("Crear factura")]
        CrearFactura,
        [Description("Anular factura")]
        AnularFactura,
        [Description("Solicitar pdf")]
        SolicitarPdf,
        [Description("Solicitar Xml")]
        SolicitarXml
    }

    public static class ltrFacturador
    {
        public const string SometidoEnvioDeFactura = "Factura emitida y sometido su envío";
        public const string SometidoLoteDeEnvio = "Se ha sometido un lote de envío de factura, acceda a la aplicación para cercionar el envío";
        public const string NoUsaVerifactu = "La factura no se ha enviado a la AEAT ya que la sociedad emisora no usa verifatu o el sistema no tiene activado el envío";
        public const string ErrorAlImprimir = "Error al generar el PDF o el XML de la factura";
        public const string ErrorAlComunicarALaAEAT = "Factura devuelta a prefactura, consúltela para ver el detalle del error o acceda al trabajo usado para comunicarla";
    }

    [Table(Tablas.PETICION_DE_FACTURA_EMT, Schema = Esquemas.VENTA)]
    public class PeticionDeFacturaEmtDtm : RegistroDtm
    {
        public Guid Guid { get; set; }
        public DateTime SolicitadaEl { get; set; }
        public enumOperacionFacturador Peticion { get; set; }
        public int IdFacturador { get; set; }
        public FacturadorDeSociedadDtm Facturador { get; set; }
        public string FacturaJson { get; set; }
        public int? IdFactura { get; set; }
        public FacturaEmtDtm Factura { get; set; }
        public string Error { get; set; }
        public string ValidadorJson { get; set; }
    }


    public static partial class ModeloDeFacturaEmt
    {
        public static void PeticionDeFacturaEmt(ModelBuilder modelBuilder)
        {
            ApiDeRegistroDtm.DefinirCampoIdDtm<PeticionDeFacturaEmtDtm>(modelBuilder);
            modelBuilder.Entity<PeticionDeFacturaEmtDtm>().Property(x => x.Guid).HasColumnName(ICampos.GUID).HasColumnType(IDominio.UNIQUEIDENTIFIER).IsRequired(true);
            modelBuilder.Entity<PeticionDeFacturaEmtDtm>().Property(nameof(PeticionDeFacturaEmtDtm.SolicitadaEl)).HasColumnName(ICampos.CREADO_EL).HasColumnType(IDominio.DATETIME_2).IsRequired(true);
            modelBuilder.Entity<PeticionDeFacturaEmtDtm>().Property(nameof(PeticionDeFacturaEmtDtm.Peticion)).HasColumnName(ICampos.PETICION).HasColumnType(IDominio.VARCHAR_50).IsRequired(true);
            ApiDeRegistroDtm.DefinirCampoFk<PeticionDeFacturaEmtDtm>(modelBuilder, nameof(PeticionDeFacturaEmtDtm.Facturador), nameof(PeticionDeFacturaEmtDtm.IdFacturador), ICampos.ID_FACTURADOR, requerida: true, unico: false);
            ApiDeRegistroDtm.DefinirCampoFk<PeticionDeFacturaEmtDtm>(modelBuilder, nameof(PeticionDeFacturaEmtDtm.Factura), nameof(PeticionDeFacturaEmtDtm.IdFactura), ICampos.ID_FACTURA_EMT, requerida:false, unico: true);
            modelBuilder.Entity<PeticionDeFacturaEmtDtm>().Property(nameof(PeticionDeFacturaEmtDtm.FacturaJson)).HasColumnName(ICampos.FACTURA_JSON).HasColumnType(IDominio.VARCHAR_MAX).IsRequired(false);
            modelBuilder.Entity<PeticionDeFacturaEmtDtm>().Property(nameof(PeticionDeFacturaEmtDtm.Error)).HasColumnName(ICampos.ERROR).HasColumnType(IDominio.VARCHAR_MAX).IsRequired(false);
            modelBuilder.Entity<PeticionDeFacturaEmtDtm>().Property(nameof(PeticionDeFacturaEmtDtm.ValidadorJson)).HasColumnName(ICampos.VALIDADOR_JSON).HasColumnType(IDominio.VARCHAR_MAX).IsRequired(false);
        }
    }
}
