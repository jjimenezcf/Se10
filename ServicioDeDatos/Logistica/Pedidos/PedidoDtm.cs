using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.Juridico;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Terceros;
using Utilidades;

namespace ServicioDeDatos.Logistica
{

    public class ltrDeUnPedido
    {
        public static string EventoDeEntrega => $"Entrega: [{nameof(PedidoDtm.Referencia)}]";
        public const string FiltroPorProveedor = nameof(PedidoDtm.IdProveedor);
        public const string FiltroPorEjercicio = nameof(FiltroPorEjercicio);
        public const string FiltroPorFechaDePedido = nameof(FiltroPorFechaDePedido);
        public const string FiltroPorFechaDeEntrega = nameof(FiltroPorFechaDeEntrega);
        public const string FiltroPorFechaDeRecepcion = nameof(FiltroPorFechaDeRecepcion);
        public const string FiltroPorFechaDeCierre = nameof(FiltroPorFechaDeCierre);
        public const string FiltroPorPedidoReferencia = nameof(FiltroPorPedidoReferencia);


        public const string FiltroPorImporte = nameof(FiltroPorImporte);


        public const string PedidosImputablesAlContrato = nameof(PedidosImputablesAlContrato);
        public const string PedidosImputablesAlExpediente = nameof(PedidosImputablesAlExpediente);
        public const string FiltroPorEtapa = nameof(FiltroPorEtapa);

        public const string IdContrato = nameof(IdContrato);
        public const string IdExpediente = nameof(IdExpediente);
        public const string AsociadaAUnContrato = nameof(AsociadaAUnContrato);
        public const string AsociadaAUnExpediente = nameof(AsociadaAUnExpediente);

    }

    [Table(Tablas.PEDIDO, Schema = Esquemas.LOGISTICA)]
    public class PedidoDtm : ElementoDeProcesoDtm, IUsaProveedor, IUsaArchivo, IUsaDirecciones, IUsaExpediente
    {
        public int IdProveedor { get; set; }
        public ProveedorDtm Proveedor { get; set; }
        public string Contacto { get; set; }
        public string Telefono { get; set; }
        public string eMail { get; set; }

        public new TipoDePedidoDtm Tipo { get; set; }
        public new EstadoDeUnPedidoDtm Estado { get; set; }

        public int? IdContrato { get; set; }
        public ContratoDtm Contrato { get; set; }

        public int? IdExpediente { get; set; }
        public ExpedienteDtm Expediente { get; set; }

        public DateTime? PedidoEl { get; set; }
        public DateTime? EntregarEl { get; set; }
        public DateTime? RecibidoEl { get; set; }
        public DateTime? CerradoEl { get; set; }

        public int? IdArchivo { get; set; }
        public ArchivoDtm Archivo { get; set; }
    }


    [Table(Tablas.PEDIDO + "_" + nameof(Sufijo.AUDITORIA), Schema = Esquemas.LOGISTICA)]
    public class AuditoriaDeUnPedidoDtm : AuditoriaDtm
    {
    }


    [Table(Tablas.PEDIDO + "_" + nameof(Sufijo.ARCHIVO), Schema = Esquemas.LOGISTICA)]
    public class ArchivosDeUnPedidoDtm : VinculoDtm
    {
        public PedidoDtm Pedido { get; set; }
        public ArchivoDtm Archivo { get; set; }
    }

    [Table(Tablas.PEDIDO + "_" + nameof(Sufijo.AGENDA_EVENTO), Schema = Esquemas.LOGISTICA)]
    public class AgendaDeUnPedidoDtm : VinculoDtm
    {
        public PedidoDtm Pedido { get; set; }
        public EventoDeAgendaDtm Evento { get; set; }
    }

    [Table(Tablas.PEDIDO + "_" + nameof(Sufijo.OBSERVACION), Schema = Esquemas.LOGISTICA)]
    public class ObservacionesDeUnPedidoDtm : ObservacionDtm
    {
        public override enumNegocio Negocio => enumNegocio.Pedido;
    }

    [Table(Tablas.PEDIDO + "_" + nameof(Sufijo.PERMISO), Schema = Esquemas.LOGISTICA)]
    public class PermisoDelPedidoDtm : PermisosDelElementoDtm
    {
    }

    [Table(Tablas.PEDIDO + "_" + nameof(Sufijo.TRAZA), Schema = Esquemas.LOGISTICA)]
    public class TrazasDeUnPedidoDtm : TrazaDtm
    {
        public override enumNegocio Negocio => enumNegocio.Pedido;
    }

    [Table(Tablas.PEDIDO + "_" + nameof(Sufijo.DIRECCION), Schema = Esquemas.LOGISTICA)]
    public class DireccionDeUnPedidoDtm : DireccionDtm
    {
        public override enumNegocio Negocio => enumNegocio.Pedido;

    }

    [Table(Tablas.PEDIDO + "_" + nameof(Sufijo.HISTORIA), Schema = Esquemas.LOGISTICA)]
    public class HitosDeUnPedidoDtm : HitoDtm
    {

    }

    [Table(Tablas.PEDIDO + "_" + nameof(Sufijo.ARCHIVADOR), Schema = Esquemas.LOGISTICA)]
    public class ArchivadoresDeUnPedidoDtm : VinculoDtm
    {
        public PedidoDtm Pedido { get; set; }
        public ArchivadorDtm Archivador { get; set; }
    }


    public static partial class ModeloDePedido
    {

        public static void Pedido(ModelBuilder modelBuilder)
        {
            ApiDeElementoDtm.DefinirCamposDelElementoDtm<PedidoDtm>(modelBuilder);
            ApiDeElementoDtm.DefinirCampoCg<PedidoDtm>(modelBuilder, nameof(PedidoDtm.Cg));
            ApiDeElementoDtm.DefinirCampoTipo<PedidoDtm>(modelBuilder, nameof(PedidoDtm.Tipo));
            ApiDeElementoDtm.DefinirCampoEstado<PedidoDtm>(modelBuilder, nameof(PedidoDtm.Estado));
            ApiDeElementoDtm.DefinirProveedor<PedidoDtm>(modelBuilder);

            modelBuilder.Entity<PedidoDtm>().Property(p => p.PedidoEl).HasColumnName(ICampos.PEDIDO_EL).HasColumnType(IDominio.DATETIME_2).IsRequired(false);
            modelBuilder.Entity<PedidoDtm>().Property(p => p.EntregarEl).HasColumnName(ICampos.ENTREGAR_EL).HasColumnType(IDominio.DATETIME_2).IsRequired(false);
            modelBuilder.Entity<PedidoDtm>().Property(p => p.RecibidoEl).HasColumnName(ICampos.RECIBIDA_EL).HasColumnType(IDominio.DATETIME_2).IsRequired(false);
            modelBuilder.Entity<PedidoDtm>().Property(p => p.CerradoEl).HasColumnName(ICampos.CERRADO_EL).HasColumnType(IDominio.DATETIME_2).IsRequired(false);

            ApiDeRegistroDtm.DefinirDependencia<PedidoDtm, ContratoDtm>(modelBuilder, apuntadoPor: nameof(PedidoDtm.IdContrato), idCampo: ICampos.ID_CONTRATO, requerido: false);

            ModeloDeExpediente.DefinirExpediente<PedidoDtm>(modelBuilder, false);

            ApiDeElementoDtm.DefinirCampoArchivo<PedidoDtm>(modelBuilder);
        }

        internal static void Trazas(ModelBuilder modelBuilder)
        {
            ApiTraza.DefinirCampos<TrazasDeUnPedidoDtm, PedidoDtm>(modelBuilder);
        }

        internal static void Auditoria(ModelBuilder modelBuilder)
        {
            ApiDeAuditoria.DefinirCamposDeAuditoriaDtm<AuditoriaDeUnPedidoDtm>(modelBuilder);
        }

        internal static void Archivos(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<ArchivosDeUnPedidoDtm>(modelBuilder, nameof(ArchivosDeUnPedidoDtm.Pedido), nameof(ArchivosDeUnPedidoDtm.Archivo));

        }
        internal static void Agenda(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<AgendaDeUnPedidoDtm>(modelBuilder, nameof(AgendaDeUnPedidoDtm.Pedido), nameof(AgendaDeUnPedidoDtm.Evento));
        }

        internal static void Observaciones(ModelBuilder modelBuilder)
        {
            ApiDeObservaciones.DefinirCampos<ObservacionesDeUnPedidoDtm, PedidoDtm>(modelBuilder);
        }

        internal static void Permisos(ModelBuilder modelBuilder)
        {
            ApiPermisosDelElemento.DefinirCampos<PermisoDelPedidoDtm, PedidoDtm>(modelBuilder);
        }

        internal static void Direcciones(ModelBuilder modelBuilder)
        {
            ApiDireccionDtm.DefinirCampos<DireccionDeUnPedidoDtm, PedidoDtm>(modelBuilder);
        }

        internal static void Historia(ModelBuilder modelBuilder)
        {
            ApiDeHitos.DefinirCampos<HitosDeUnPedidoDtm, PedidoDtm, EstadoDeUnPedidoDtm, TransicionesDeUnPedidoDtm, ObservacionesDeUnPedidoDtm>(modelBuilder);
        }

        internal static void Archivadores(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<ArchivadoresDeUnPedidoDtm>(modelBuilder, nameof(ArchivadoresDeUnPedidoDtm.Pedido), nameof(ArchivadoresDeUnPedidoDtm.Archivador));
        }

    }
}
