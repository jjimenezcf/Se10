using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.MaestrosTecnico;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Terceros;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using Utilidades;

namespace ServicioDeDatos.Gastos
{
    [Table(Tablas.PAGO, Schema = Esquemas.GASTO)]
    public class PagoDtm : ElementoDeProcesoDtm, IPuedeUsarProveedor, IPuedeUsarTrabajador, IPuedeUsarCliente, IUsaSolicitante, IUsaPreasiento
    {
        public new TipoDePagoDtm Tipo { get; set; }
        public new EstadoDeUnPagoDtm Estado { get; set; }
        public enumClaseDePago Clase { get; set; }
        public enumModoDePagoContado? ModoDePago => Clase != enumClaseDePago.Contado
               ? null
               : IdTarjetaDePago is not null
               ? enumModoDePagoContado.Tarjeta
               : IdCuentaDePago is not null
               ? enumModoDePagoContado.Domiciliacion
               : enumModoDePagoContado.Contado;
        public string FormaDePago => Clase == enumClaseDePago.Contado && ModoDePago == enumModoDePagoContado.Contado
            ? enumClaseDePago.Contado.Descripcion()
            : Clase == enumClaseDePago.Contado && ModoDePago == enumModoDePagoContado.Tarjeta
            ? enumModoDePagoContado.Tarjeta.Descripcion()
            : Clase == enumClaseDePago.Contado && ModoDePago == enumModoDePagoContado.Domiciliacion
            ? enumModoDePagoContado.Domiciliacion.Descripcion()
            : Clase == enumClaseDePago.Transferencia
            ? enumClaseDePago.Transferencia.Descripcion()
            : enumClaseDePago.Remesa.Descripcion();
        public int IdSolicitante { get; set; }
        public InterlocutorDtm Solicitante { get; set; }
        public int? IdProveedor { get; set; }
        public ProveedorDtm Proveedor { get; set; }
        public int? IdCliente { get; set; }
        public ClienteDtm Cliente { get; set; }
        public int? IdTrabajador { get; set; }
        public TrabajadorDtm Trabajador { get; set; }
        public string Nif { get; set; }
        public string Contacto { get; set; }
        public string Telefono { get; set; }
        public string eMail { get; set; }
        public int? IdCuentaDePago { get; set; }
        public CuentaDeMiSociedadDtm CuentaDePago { get; set; }
        public int? IdTarjetaDePago { get; set; }
        public TarjetaDeMiSociedadDtm TarjetaDePago { get; set; }
        public int? IdCuentaDeAcreedor { get; set; }
        public CuentaBancariaDtm CuentaDeAcreedor { get; set; }
        public DateTime? PagarEl { get; set; }
        public DateTime? PagadoEl { get; set; }
        public decimal Importe { get; set; }
        public int? IdFacturaRec { get; set; }
        public FacturaRecDtm FacturaRec { get; set; }
        public int? IdNaturaleza { get; set; }
        public NaturalezaDtm Naturaleza { get; set; }
        public int? IdPreasiento { get; set; }
        public PreasientoDtm Preasiento { get; set; }
    }


    [Table(Tablas.PAGO + "_" + nameof(Sufijo.AUDITORIA), Schema = Esquemas.GASTO)]
    public class AuditoriaDeUnPagoDtm : AuditoriaDtm
    {
    }

    [Table(Tablas.PAGO + "_" + nameof(Sufijo.ARCHIVO), Schema = Esquemas.GASTO)]
    public class ArchivosDeUnPagoDtm : VinculoDtm
    {
        public PagoDtm Pago { get; set; }
        public ArchivoDtm Archivo { get; set; }
    }

    [Table(Tablas.PAGO + "_" + nameof(Sufijo.AGENDA_EVENTO), Schema = Esquemas.GASTO)]
    public class AgendaDeUnPagoDtm : VinculoDtm
    {
        public PagoDtm Pago { get; set; }
        public EventoDeAgendaDtm Evento { get; set; }
    }

    [Table(Tablas.PAGO + "_" + nameof(Sufijo.OBSERVACION), Schema = Esquemas.GASTO)]
    public class ObservacionesDeUnPagoDtm : ObservacionDtm
    {
        public override enumNegocio Negocio => enumNegocio.Pago;
    }

    [Table(Tablas.PAGO + "_" + nameof(Sufijo.PERMISO), Schema = Esquemas.GASTO)]
    public class PermisoDelPagoDtm : PermisosDelElementoDtm
    {
    }

    [Table(Tablas.PAGO + "_" + nameof(Sufijo.TRAZA), Schema = Esquemas.GASTO)]
    public class TrazasDeUnPagoDtm : TrazaDtm
    {
        public override enumNegocio Negocio => enumNegocio.Pago;
    }

    [Table(Tablas.PAGO + "_" + nameof(Sufijo.HISTORIA), Schema = Esquemas.GASTO)]
    public class HitosDeUnPagoDtm : HitoDtm
    {

    }

    [Table(Tablas.PAGO + "_" + nameof(Sufijo.ARCHIVADOR), Schema = Esquemas.GASTO)]
    public class ArchivadoresDeUnPagoDtm : VinculoDtm
    {
        public PagoDtm Pago { get; set; }
        public ArchivadorDtm Archivador { get; set; }
    }

    [Table(Tablas.PAGO + "_" + nameof(Sufijo.CIRCUITO_DOC), Schema = Esquemas.GASTO)]
    public class CircuitoDocDeUnPagoDtm : VinculoDtm
    {
        public PagoDtm Pago { get; set; }
        public CircuitoDocDtm Circuito { get; set; }
    }

    public static partial class ModeloDePago
    {

        public static void Pago(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PagoDtm>().Ignore(x => x.ModoDePago);
            modelBuilder.Entity<PagoDtm>().Ignore(x => x.CuentaDeAcreedor);
            modelBuilder.Entity<PagoDtm>().Ignore(x => x.CuentaDePago);
            modelBuilder.Entity<PagoDtm>().Ignore(x => x.TarjetaDePago);
            modelBuilder.Entity<PagoDtm>().Ignore(x => x.Trabajador);
            modelBuilder.Entity<PagoDtm>().Ignore(x => x.Proveedor);
            modelBuilder.Entity<PagoDtm>().Ignore(x => x.Cliente);


            ApiDeElementoDtm.DefinirCamposDelElementoDtm<PagoDtm>(modelBuilder);
            ApiDeElementoDtm.DefinirCampoCg<PagoDtm>(modelBuilder, nameof(PagoDtm.Cg));
            ApiDeElementoDtm.DefinirCampoTipo<PagoDtm>(modelBuilder, nameof(PagoDtm.Tipo));
            ApiDeElementoDtm.DefinirCampoEstado<PagoDtm>(modelBuilder, nameof(PagoDtm.Estado));
            ApiDeElementoDtm.DefinirProveedor<PagoDtm>(modelBuilder);
            ApiDeElementoDtm.DefinirCliente<PagoDtm>(modelBuilder);
            ApiDeElementoDtm.DefinirTrabajador<PagoDtm>(modelBuilder);
            ApiDeElementoDtm.DefinirSolicitante<PagoDtm>(modelBuilder);

            modelBuilder.Entity<PagoDtm>().Property(p => p.PagadoEl).HasColumnName(ICampos.PAGADO_EL).HasColumnType(IDominio.DATETIME_2).IsRequired(false);
            modelBuilder.Entity<PagoDtm>().Property(p => p.PagarEl).HasColumnName(ICampos.PAGAR_EL).HasColumnType(IDominio.DATETIME_2).IsRequired(false);
            modelBuilder.Entity<PagoDtm>().Property(p => p.Importe).HasColumnName(ICampos.IMPORTE).HasColumnType(IDominio.DECIMAL).IsRequired(true);
            modelBuilder.Entity<PagoDtm>().Property(nameof(PagoDtm.Clase)).HasColumnName(ICampos.CLASE).HasColumnType(IDominio.VARCHAR_30).IsRequired(true);

            ApiDeRegistroDtm.DefinirCampoFk<PagoDtm>(modelBuilder, nameof(PagoDtm.TarjetaDePago), nameof(PagoDtm.IdTarjetaDePago), ICampos.ID_TARJETA, requerida: false, unico: false);
            ApiDeRegistroDtm.DefinirCampoFk<PagoDtm>(modelBuilder, nameof(PagoDtm.CuentaDePago), nameof(PagoDtm.IdCuentaDePago), ICampos.ID_CUENTA_CARGO, requerida: false, unico: false);
            ApiDeRegistroDtm.DefinirCampoFk<PagoDtm>(modelBuilder, nameof(PagoDtm.CuentaDeAcreedor), nameof(PagoDtm.IdCuentaDeAcreedor), ICampos.ID_CUENTA_INGRESO, requerida: false, unico: false);

            modelBuilder.Entity<PagoDtm>().Property(p => p.Nif).HasColumnName(ICampos.NIF).HasColumnType(IDominio.VARCHAR_15).IsRequired();

            ApiDeRegistroDtm.DefinirCampoFk<PagoDtm>(modelBuilder, nameof(PagoDtm.FacturaRec), nameof(PagoDtm.IdFacturaRec), ICampos.ID_FACTURA_REC, requerida: false, unico: false);
            ApiDeRegistroDtm.DefinirCampoFk<PagoDtm>(modelBuilder, nameof(PagoDtm.Naturaleza), nameof(TipoDeGastoDtm.IdNaturaleza), ICampos.ID_NATURALEZA, requerida: false, unico: false);
        }

        internal static void Trazas(ModelBuilder modelBuilder)
        {
            ApiTraza.DefinirCampos<TrazasDeUnPagoDtm, PagoDtm>(modelBuilder);
        }

        internal static void Auditoria(ModelBuilder modelBuilder)
        {
            ApiDeAuditoria.DefinirCamposDeAuditoriaDtm<AuditoriaDeUnPagoDtm>(modelBuilder);
        }

        internal static void Archivos(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<ArchivosDeUnPagoDtm>(modelBuilder, nameof(ArchivosDeUnPagoDtm.Pago), nameof(ArchivosDeUnPagoDtm.Archivo));

        }
        internal static void Agenda(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<AgendaDeUnPagoDtm>(modelBuilder, nameof(AgendaDeUnPagoDtm.Pago), nameof(AgendaDeUnPagoDtm.Evento));
        }

        internal static void Observaciones(ModelBuilder modelBuilder)
        {
            ApiDeObservaciones.DefinirCampos<ObservacionesDeUnPagoDtm, PagoDtm>(modelBuilder);
        }

        internal static void Permisos(ModelBuilder modelBuilder)
        {
            ApiPermisosDelElemento.DefinirCampos<PermisoDelPagoDtm, PagoDtm>(modelBuilder);
        }

        internal static void Historia(ModelBuilder modelBuilder)
        {
            ApiDeHitos.DefinirCampos<HitosDeUnPagoDtm, PagoDtm, EstadoDeUnPagoDtm, TransicionesDeUnPagoDtm, ObservacionesDeUnPagoDtm>(modelBuilder);
        }

        internal static void Archivadores(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<ArchivadoresDeUnPagoDtm>(modelBuilder, nameof(ArchivadoresDeUnPagoDtm.Pago), nameof(ArchivadoresDeUnPagoDtm.Archivador));
        }


        internal static void Circuitos(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<CircuitoDocDeUnPagoDtm>(modelBuilder, nameof(CircuitoDocDeUnPagoDtm.Pago), nameof(CircuitoDocDeUnPagoDtm.Circuito));
        }


    }
}
