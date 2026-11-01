using System;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.Ventas;
using System.ComponentModel.DataAnnotations.Schema;
using Utilidades;

namespace ServicioDeDatos.Logistica
{

    [Table(Tablas.VENTA, Schema = Esquemas.LOGISTICA)]
    public class VentaDtm : ElementoDeProcesoDtm, IUsaCliente
    {
        public int IdCliente { get; set; }
        public ClienteDtm Cliente { get; }
        public string Contacto { get; set; }
        public string Telefono { get; set; }
        public string eMail { get; set; }

        public int IdAlmacen { get; set; }
        public AlmacenDtm Almacen { get; set; }

        public DateTime? SolicitadoEl { get; set; }
        public DateTime? ServidoEl { get; set; }

        public int? IdFactura { get; set; }
        public FacturaEmtDtm Factura { get; set; }

        public new TipoDeVentaDtm Tipo { get; set; }
        public new EstadoDeUnaVentaDtm Estado { get; set; }
    }


    [Table(Tablas.VENTA + "_" + nameof(Sufijo.AUDITORIA), Schema = Esquemas.LOGISTICA)]
    public class AuditoriaDeUnaVentaDtm : AuditoriaDtm
    {
    }


    [Table(Tablas.VENTA + "_" + nameof(Sufijo.ARCHIVO), Schema = Esquemas.LOGISTICA)]
    public class ArchivosDeUnaVentaDtm : VinculoDtm
    {
        public VentaDtm Venta { get; set; }
        public ArchivoDtm Archivo { get; set; }
    }

    [Table(Tablas.VENTA + "_" + nameof(Sufijo.OBSERVACION), Schema = Esquemas.LOGISTICA)]
    public class ObservacionesDeUnaVentaDtm : ObservacionDtm
    {
        public override enumNegocio Negocio => enumNegocio.Venta;
    }

    [Table(Tablas.VENTA + "_" + nameof(Sufijo.PERMISO), Schema = Esquemas.LOGISTICA)]
    public class PermisoDeLaVentaDtm : PermisosDelElementoDtm
    {
    }

    [Table(Tablas.VENTA + "_" + nameof(Sufijo.TRAZA), Schema = Esquemas.LOGISTICA)]
    public class TrazasDeUnaVentaDtm : TrazaDtm
    {
        public override enumNegocio Negocio => enumNegocio.Venta;
    }

    [Table(Tablas.VENTA + "_" + nameof(Sufijo.HISTORIA), Schema = Esquemas.LOGISTICA)]
    public class HitosDeUnaVentaDtm : HitoDtm
    {

    }

    [Table(Tablas.VENTA + "_" + nameof(Sufijo.ARCHIVADOR), Schema = Esquemas.LOGISTICA)]
    public class ArchivadoresDeUnaVentaDtm : VinculoDtm
    {
        public VentaDtm Venta { get; set; }
        public ArchivadorDtm Archivador { get; set; }
    }


    public static partial class ModeloDeVenta
    {

        public static void Venta(ModelBuilder modelBuilder)
        {
            ApiDeElementoDtm.DefinirCamposDelElementoDtm<VentaDtm>(modelBuilder);
            ApiDeElementoDtm.DefinirCampoCg<VentaDtm>(modelBuilder, nameof(VentaDtm.Cg));
            ApiDeElementoDtm.DefinirCampoTipo<VentaDtm>(modelBuilder, nameof(VentaDtm.Tipo));
            ApiDeElementoDtm.DefinirCampoEstado<VentaDtm>(modelBuilder, nameof(VentaDtm.Estado));
            ApiDeElementoDtm.DefinirCliente<VentaDtm>(modelBuilder);

            ApiDeRegistroDtm.DefinirCampoFk<VentaDtm>(modelBuilder, nameof(VentaDtm.Almacen), nameof(VentaDtm.IdAlmacen), ICampos.ID_ALMACEN, requerida: true, unico: false);

            modelBuilder.Entity<VentaDtm>().Property(p => p.SolicitadoEl).HasColumnName(ICampos.SOLICITADO_EL).HasColumnType(IDominio.DATETIME_2).IsRequired(false);
            modelBuilder.Entity<VentaDtm>().Property(p => p.ServidoEl).HasColumnName(ICampos.SERVIDO_EL).HasColumnType(IDominio.DATETIME_2).IsRequired(false);

            ApiDeRegistroDtm.DefinirCampoFk<VentaDtm>(modelBuilder, nameof(VentaDtm.Factura), nameof(VentaDtm.IdFactura), ICampos.ID_FACTURA_EMT, requerida: false, unico: false);
        }

        internal static void Trazas(ModelBuilder modelBuilder)
        {
            ApiTraza.DefinirCampos<TrazasDeUnaVentaDtm, VentaDtm>(modelBuilder);
        }

        internal static void Auditoria(ModelBuilder modelBuilder)
        {
            ApiDeAuditoria.DefinirCamposDeAuditoriaDtm<AuditoriaDeUnaVentaDtm>(modelBuilder);
        }

        internal static void Archivos(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<ArchivosDeUnaVentaDtm>(modelBuilder, nameof(ArchivosDeUnaVentaDtm.Venta), nameof(ArchivosDeUnaVentaDtm.Archivo));
        }

        internal static void Observaciones(ModelBuilder modelBuilder)
        {
            ApiDeObservaciones.DefinirCampos<ObservacionesDeUnaVentaDtm, VentaDtm>(modelBuilder);
        }

        internal static void Permisos(ModelBuilder modelBuilder)
        {
            ApiPermisosDelElemento.DefinirCampos<PermisoDeLaVentaDtm, VentaDtm>(modelBuilder);
        }

        internal static void Historia(ModelBuilder modelBuilder)
        {
            ApiDeHitos.DefinirCampos<HitosDeUnaVentaDtm, VentaDtm, EstadoDeUnaVentaDtm, TransicionesDeUnaVentaDtm, ObservacionesDeUnaVentaDtm>(modelBuilder);
        }

        internal static void Archivadores(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<ArchivadoresDeUnaVentaDtm>(modelBuilder, nameof(ArchivadoresDeUnaVentaDtm.Venta), nameof(ArchivadoresDeUnaVentaDtm.Archivador));
        }

    }
}
