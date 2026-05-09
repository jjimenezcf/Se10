using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Terceros;
using Utilidades;

namespace ServicioDeDatos.Gastos
{
    [Table(Tablas.GASTO, Schema = Esquemas.GASTO)]
    public class GastoDtm : ElementoDeProcesoDtm, IUsaPreasiento
    {
        public new TipoDeGastoDtm Tipo { get; set; }
        public new EstadoDeUnGastoDtm Estado { get; set; }
        public int? IdMiCuentaBancaria { get; set; }
        public CuentaDeMiSociedadDtm MiCuentaBancaria { get; set; }
        public int? IdMiTarjetaDeGasto { get; set; }
        public TarjetaDeMiSociedadDtm MiTarjetaDeGasto { get; set; }
        public DateTime? PagarEl { get; set; }
        public DateTime? PagadoEl { get; set; }
        public decimal Importe { get; set; }
        public enumModoDePagoDelGasto? ModoDePago => IdMiTarjetaDeGasto is not null
               ? enumModoDePagoDelGasto.Tarjeta
               : IdMiCuentaBancaria is not null
               ? enumModoDePagoDelGasto.Banco
               : enumModoDePagoDelGasto.Contado;

        public int? IdPreasiento { get; set; }

        public PreasientoDtm Preasiento { get; set; }
    }


    [Table(Tablas.GASTO + "_" + nameof(Sufijo.AUDITORIA), Schema = Esquemas.GASTO)]
    public class AuditoriaDeUnGastoDtm : AuditoriaDtm
    {
    }

    [Table(Tablas.GASTO + "_" + nameof(Sufijo.ARCHIVO), Schema = Esquemas.GASTO)]
    public class ArchivosDeUnGastoDtm : VinculoDtm
    {
        public GastoDtm Gasto { get; set; }
        public ArchivoDtm Archivo { get; set; }
    }

    [Table(Tablas.GASTO + "_" + nameof(Sufijo.OBSERVACION), Schema = Esquemas.GASTO)]
    public class ObservacionesDeUnGastoDtm : ObservacionDtm
    {
        public override enumNegocio Negocio => enumNegocio.Gasto;
    }

    [Table(Tablas.GASTO + "_" + nameof(Sufijo.PERMISO), Schema = Esquemas.GASTO)]
    public class PermisoDelGastoDtm : PermisosDelElementoDtm
    {
    }

    [Table(Tablas.GASTO + "_" + nameof(Sufijo.TRAZA), Schema = Esquemas.GASTO)]
    public class TrazasDeUnGastoDtm : TrazaDtm
    {
        public override enumNegocio Negocio => enumNegocio.Gasto;
    }

    [Table(Tablas.GASTO + "_" + nameof(Sufijo.HISTORIA), Schema = Esquemas.GASTO)]
    public class HitosDeUnGastoDtm : HitoDtm
    {

    }

    [Table(Tablas.GASTO + "_" + nameof(Sufijo.ARCHIVADOR), Schema = Esquemas.GASTO)]
    public class ArchivadoresDeUnGastoDtm : VinculoDtm
    {
        public GastoDtm Gasto { get; set; }
        public ArchivadorDtm Archivador { get; set; }
    }

    [Table(Tablas.GASTO + "_" + nameof(Sufijo.CIRCUITO_DOC), Schema = Esquemas.GASTO)]
    public class CircuitoDocDeUnGastoDtm : VinculoDtm
    {
        public GastoDtm Gasto { get; set; }
        public CircuitoDocDtm Circuito { get; set; }
    }

    public static partial class ModeloDeGasto
    {

        public static void Gasto(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GastoDtm>().Ignore(x => x.ModoDePago);
            modelBuilder.Entity<GastoDtm>().Ignore(x => x.MiCuentaBancaria);
            modelBuilder.Entity<GastoDtm>().Ignore(x => x.MiTarjetaDeGasto);


            ApiDeElementoDtm.DefinirCamposDelElementoDtm<GastoDtm>(modelBuilder);
            ApiDeElementoDtm.DefinirCampoCg<GastoDtm>(modelBuilder, nameof(GastoDtm.Cg));
            ApiDeElementoDtm.DefinirCampoTipo<GastoDtm>(modelBuilder, nameof(GastoDtm.Tipo));
            ApiDeElementoDtm.DefinirCampoEstado<GastoDtm>(modelBuilder, nameof(GastoDtm.Estado));

            modelBuilder.Entity<GastoDtm>().Property(p => p.PagadoEl).HasColumnName(ICampos.PAGADO_EL).HasColumnType(IDominio.DATETIME_2).IsRequired(false);
            modelBuilder.Entity<GastoDtm>().Property(p => p.PagarEl).HasColumnName(ICampos.PAGAR_EL).HasColumnType(IDominio.DATETIME_2).IsRequired(false);
            modelBuilder.Entity<GastoDtm>().Property(p => p.Importe).HasColumnName(ICampos.IMPORTE).HasColumnType(IDominio.DECIMAL).IsRequired(true);

            ApiDeRegistroDtm.DefinirCampoFk<GastoDtm>(modelBuilder, nameof(GastoDtm.MiTarjetaDeGasto), nameof(GastoDtm.IdMiTarjetaDeGasto), ICampos.ID_TARJETA, requerida: false, unico: false);
            ApiDeRegistroDtm.DefinirCampoFk<GastoDtm>(modelBuilder, nameof(GastoDtm.MiCuentaBancaria), nameof(GastoDtm.IdMiCuentaBancaria), ICampos.ID_CUENTA_CARGO, requerida: false, unico: false);
        }

        internal static void Trazas(ModelBuilder modelBuilder)
        {
            ApiTraza.DefinirCampos<TrazasDeUnGastoDtm, GastoDtm>(modelBuilder);
        }

        internal static void Auditoria(ModelBuilder modelBuilder)
        {
            ApiDeAuditoria.DefinirCamposDeAuditoriaDtm<AuditoriaDeUnGastoDtm>(modelBuilder);
        }

        internal static void Archivos(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<ArchivosDeUnGastoDtm>(modelBuilder, nameof(ArchivosDeUnGastoDtm.Gasto), nameof(ArchivosDeUnGastoDtm.Archivo));

        }

        internal static void Observaciones(ModelBuilder modelBuilder)
        {
            ApiDeObservaciones.DefinirCampos<ObservacionesDeUnGastoDtm, GastoDtm>(modelBuilder);
        }

        internal static void Permisos(ModelBuilder modelBuilder)
        {
            ApiPermisosDelElemento.DefinirCampos<PermisoDelGastoDtm, GastoDtm>(modelBuilder);
        }

        internal static void Historia(ModelBuilder modelBuilder)
        {
            ApiDeHitos.DefinirCampos<HitosDeUnGastoDtm, GastoDtm, EstadoDeUnGastoDtm, TransicionesDeUnGastoDtm, ObservacionesDeUnGastoDtm>(modelBuilder);
        }

        internal static void Archivadores(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<ArchivadoresDeUnGastoDtm>(modelBuilder, nameof(ArchivadoresDeUnGastoDtm.Gasto), nameof(ArchivadoresDeUnGastoDtm.Archivador));
        }


        internal static void Circuitos(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<CircuitoDocDeUnGastoDtm>(modelBuilder, nameof(CircuitoDocDeUnGastoDtm.Gasto), nameof(CircuitoDocDeUnGastoDtm.Circuito));
        }


    }
}
