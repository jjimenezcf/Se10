using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.Juridico;
using ServicioDeDatos.MaestrosTecnico;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Terceros;
using Utilidades;

namespace ServicioDeDatos.Gastos
{

    [Table(Tablas.FACTURA_REC, Schema = Esquemas.GASTO)]
    public class FacturaRecDtm : ElementoDeProcesoDtm, IUsaProveedor,IUsaArchivo, IUsaDirecciones, IUsaExpediente, IUsaPreasiento
    {

        public string Numero { get; set; }

        public int IdProveedor { get; set; }
        public ProveedorDtm Proveedor { get; set; }
        public string Contacto { get; set; }
        public string Telefono { get; set; }
        public string eMail { get; set; }

        public new TipoDeFacturaRecDtm Tipo { get; set; }
        public new EstadoDeUnaFacturaRecDtm Estado { get; set; }

        public Ventas.enumClaseDeRectificativa? ClaseRectificativa { get; set; }

        public Ventas.enumMotivoDeRectificacion? MotivoDeRectificacion { get; set; }

        public bool EsRectificativa => IdRectificada != null || BaseImponible < 0;
        public int? IdRectificada { get; set; }
        public FacturaRecDtm Rectificada { get; set; }

        public int? IdContrato { get; set; }
        public ContratoDtm Contrato { get;  set; }

        public int? IdExpediente { get; set; }
        public ExpedienteDtm Expediente { get; set; }

        public DateTime FacturadaEl { get; set; }
        public DateTime RecibidaEl { get; set; }
        public DateTime VenceEl { get; set; }
        public DateTime? ContabilizadaEl { get; set; }

        public int? IdNaturaleza { get; set; }
        public NaturalezaDtm Naturaleza { get; set; }

        public int? IdArchivo { get; set; }
        public ArchivoDtm Archivo { get; set ; }

        public decimal BaseImponible { get; set; }  
        public decimal TotalDelPago { get; set; }

        public int? IdPreasiento { get; set; }

        public PreasientoDtm Preasiento { get; set; }

    }


    [Table(Tablas.FACTURA_REC + "_" + nameof(Sufijo.AUDITORIA), Schema = Esquemas.GASTO)]
    public class AuditoriaDeUnaFacturaRecDtm : AuditoriaDtm
    {
    }

    [Table(Tablas.FACTURA_REC + "_" + Tablas.PAGO, Schema = Esquemas.GASTO)]
    public class PagosDeUnaFacturaRecDtm : VinculoDtm
    {
        public FacturaRecDtm FacturaRec { get; set; }
        public PagoDtm Pago { get; set; }
    }

    [Table(Tablas.FACTURA_REC + "_" + nameof(Sufijo.ARCHIVO), Schema = Esquemas.GASTO)]
    public class ArchivosDeUnaFacturaRecDtm : VinculoDtm
    {
        public FacturaRecDtm FacturaRec { get; set; }
        public ArchivoDtm Archivo { get; set; }
    }

    [Table(Tablas.FACTURA_REC + "_" + nameof(Sufijo.AGENDA_EVENTO), Schema = Esquemas.GASTO)]
    public class AgendaDeUnaFacturaRecDtm : VinculoDtm
    {
        public FacturaRecDtm FacturaRec { get; set; }
        public EventoDeAgendaDtm Evento { get; set; }
    }

    [Table(Tablas.FACTURA_REC + "_" + nameof(Sufijo.OBSERVACION), Schema = Esquemas.GASTO)]
    public class ObservacionesDeUnaFacturaRecDtm : ObservacionDtm
    {
        public override enumNegocio Negocio => enumNegocio.FacturaRecibida;
    }

    [Table(Tablas.FACTURA_REC + "_" + nameof(Sufijo.PERMISO), Schema = Esquemas.GASTO)]
    public class PermisoDeLaFacturaRecDtm : PermisosDelElementoDtm
    {
    }

    [Table(Tablas.FACTURA_REC + "_" + nameof(Sufijo.TRAZA), Schema = Esquemas.GASTO)]
    public class TrazasDeUnaFacturaRecDtm : TrazaDtm
    {
        public override enumNegocio Negocio => enumNegocio.FacturaRecibida;
    }

    [Table(Tablas.FACTURA_REC + "_" + nameof(Sufijo.DIRECCION), Schema = Esquemas.GASTO)]
    public class DireccionDeUnaFacturaRecDtm : DireccionDtm
    {
        public override enumNegocio Negocio => enumNegocio.FacturaRecibida;

    }

    [Table(Tablas.FACTURA_REC + "_" + nameof(Sufijo.HISTORIA), Schema = Esquemas.GASTO)]
    public class HitosDeUnaFacturaRecDtm : HitoDtm
    {

    }

    [Table(Tablas.FACTURA_REC + "_" + nameof(Sufijo.ARCHIVADOR), Schema = Esquemas.GASTO)]
    public class ArchivadoresDeUnaFacturaRecDtm : VinculoDtm
    {
        public FacturaRecDtm FacturaRec { get; set; }
        public ArchivadorDtm Archivador { get; set; }
    }

    [Table(Tablas.FACTURA_REC + "_" + nameof(Sufijo.CIRCUITO_DOC), Schema = Esquemas.GASTO)]
    public class CircuitoDocDeUnaFacturaRecDtm : VinculoDtm
    {
        public FacturaRecDtm FacturaRec { get; set; }
        public CircuitoDocDtm Circuito { get; set; }
    }
    public static partial class ModeloDeFacturaRec
    {

        public static void FacturaRec(ModelBuilder modelBuilder)
        {
            ApiDeElementoDtm.DefinirCamposDelElementoDtm<FacturaRecDtm>(modelBuilder);
            ApiDeElementoDtm.DefinirCampoCg<FacturaRecDtm>(modelBuilder, nameof(FacturaRecDtm.Cg));
            ApiDeElementoDtm.DefinirCampoTipo<FacturaRecDtm>(modelBuilder, nameof(FacturaRecDtm.Tipo));
            ApiDeElementoDtm.DefinirCampoEstado<FacturaRecDtm>(modelBuilder, nameof(FacturaRecDtm.Estado));
            ApiDeElementoDtm.DefinirProveedor<FacturaRecDtm>(modelBuilder);

            modelBuilder.Entity<FacturaRecDtm>().Property(p => p.Numero).HasColumnName(ICampos.NUMERO).HasColumnType(IDominio.VARCHAR_30).IsRequired(false);

            modelBuilder.Entity<FacturaRecDtm>().Property(p => p.FacturadaEl).HasColumnName(ICampos.FACTURADA_EL).HasColumnType(IDominio.DATETIME_2).IsRequired(true);
            modelBuilder.Entity<FacturaRecDtm>().Property(p => p.RecibidaEl).HasColumnName(ICampos.RECIBIDA_EL).HasColumnType(IDominio.DATETIME_2).IsRequired(true);
            modelBuilder.Entity<FacturaRecDtm>().Property(p => p.VenceEl).HasColumnName(ICampos.VENCE_EL).HasColumnType(IDominio.DATETIME_2).IsRequired(true);
            modelBuilder.Entity<FacturaRecDtm>().Property(p => p.ContabilizadaEl).HasColumnName(ICampos.CONTABILIZADA_EL).HasColumnType(IDominio.DATETIME_2).IsRequired(false);

            modelBuilder.Entity<FacturaRecDtm>().Property(nameof(FacturaRecDtm.BaseImponible)).HasColumnName(ICampos.BI).HasColumnType(IDominio.DECIMAL).IsRequired(true);
            modelBuilder.Entity<FacturaRecDtm>().Property(nameof(FacturaRecDtm.TotalDelPago)).HasColumnName(ICampos.TOTAL).HasColumnType(IDominio.DECIMAL).IsRequired(true);

            modelBuilder.Entity<FacturaRecDtm>().Property(nameof(FacturaRecDtm.ClaseRectificativa)).HasColumnName(ICampos.CLASE_RECTIFICATIVA).HasColumnType(IDominio.VARCHAR_30).IsRequired(false);
            modelBuilder.Entity<FacturaRecDtm>().Property(nameof(FacturaRecDtm.MotivoDeRectificacion)).HasColumnName(ICampos.MOTIVO_RECTIFICACION).HasColumnType(IDominio.VARCHAR_30).IsRequired(false);
            ApiDeRegistroDtm.DefinirDependencia<FacturaRecDtm, FacturaRecDtm>(modelBuilder, apuntadoPor: nameof(FacturaRecDtm.IdRectificada), idCampo: ICampos.ID_FACTURA_REC, requerido: false);

            ApiDeRegistroDtm.DefinirDependencia<FacturaRecDtm, ContratoDtm>(modelBuilder, apuntadoPor: nameof(FacturaRecDtm.IdContrato), idCampo: ICampos.ID_CONTRATO, requerido: false);

            ModeloDeExpediente.DefinirExpediente<FacturaRecDtm>(modelBuilder, false);

            ApiDeRegistroDtm.DefinirCampoFk<FacturaRecDtm>(modelBuilder, nameof(FacturaRecDtm.Naturaleza), nameof(FacturaRecDtm.IdNaturaleza), ICampos.ID_NATURALEZA, requerida: false, unico: false);

            ApiDeElementoDtm.DefinirCampoArchivo<FacturaRecDtm>(modelBuilder);
        }

        internal static void Trazas(ModelBuilder modelBuilder)
        {
            ApiTraza.DefinirCampos<TrazasDeUnaFacturaRecDtm, FacturaRecDtm>(modelBuilder);
        }

        internal static void Auditoria(ModelBuilder modelBuilder)
        {
            ApiDeAuditoria.DefinirCamposDeAuditoriaDtm<AuditoriaDeUnaFacturaRecDtm>(modelBuilder);
        }

        internal static void Pagos(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<PagosDeUnaFacturaRecDtm>(modelBuilder, nameof(PagosDeUnaFacturaRecDtm.FacturaRec), nameof(PagosDeUnaFacturaRecDtm.Pago), hijoUnico: true);

        }
        internal static void Archivos(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<ArchivosDeUnaFacturaRecDtm>(modelBuilder, nameof(ArchivosDeUnaFacturaRecDtm.FacturaRec), nameof(ArchivosDeUnaFacturaRecDtm.Archivo));

        }
        internal static void Agenda(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<AgendaDeUnaFacturaRecDtm>(modelBuilder, nameof(AgendaDeUnaFacturaRecDtm.FacturaRec), nameof(AgendaDeUnaFacturaRecDtm.Evento));
        }

        internal static void Observaciones(ModelBuilder modelBuilder)
        {
            ApiDeObservaciones.DefinirCampos<ObservacionesDeUnaFacturaRecDtm, FacturaRecDtm>(modelBuilder);
        }

        internal static void Permisos(ModelBuilder modelBuilder)
        {
            ApiPermisosDelElemento.DefinirCampos<PermisoDeLaFacturaRecDtm, FacturaRecDtm>(modelBuilder);
        }

        internal static void Direcciones(ModelBuilder modelBuilder)
        {
            ApiDireccionDtm.DefinirCampos<DireccionDeUnaFacturaRecDtm, FacturaRecDtm>(modelBuilder);
        }

        internal static void Historia(ModelBuilder modelBuilder)
        {
            ApiDeHitos.DefinirCampos<HitosDeUnaFacturaRecDtm, FacturaRecDtm, EstadoDeUnaFacturaRecDtm, TransicionesDeUnaFacturaRecDtm, ObservacionesDeUnaFacturaRecDtm>(modelBuilder);
        }

        internal static void Archivadores(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<ArchivadoresDeUnaFacturaRecDtm>(modelBuilder, nameof(ArchivadoresDeUnaFacturaRecDtm.FacturaRec), nameof(ArchivadoresDeUnaFacturaRecDtm.Archivador));
        }

        internal static void Circuitos(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<CircuitoDocDeUnaFacturaRecDtm>(modelBuilder, nameof(CircuitoDocDeUnaFacturaRecDtm.FacturaRec), nameof(CircuitoDocDeUnaFacturaRecDtm.Circuito));
        }

    }
}
