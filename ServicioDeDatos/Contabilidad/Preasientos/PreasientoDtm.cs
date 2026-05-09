using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.SistemaDocumental;
using Utilidades;

namespace ServicioDeDatos.Contabilidad
{

    public static class ltrDeUnPreasiento
    {
        public const string NombreFicheroNcs = nameof(NombreFicheroNcs);
        public const string Accion_AnularPreasiento = nameof(Accion_AnularPreasiento);
        public const string FiltroDeEjercicioAnterior = nameof(FiltroDeEjercicioAnterior);
        public const string FiltroSinReferencia = nameof(FiltroSinReferencia);

        public const string IdFacturaRecibida = nameof(IdFacturaRecibida);
        public const string IdFacturaEmitida = nameof(IdFacturaEmitida);
        public const string IdPago = nameof(IdPago);
        public const string NombreCuentaApunte = nameof(NombreCuentaApunte);
        public const string FiltroPorReferenciado = nameof(FiltroPorReferenciado);

        public const string EtiquetaLoteContable = "Lote contable";
        public const string FiltroLoteContable = nameof(FiltroLoteContable);
        public const string FiltroEntreImporte = nameof(FiltroEntreImporte);        

        public const string Menu_CrearLoteContable = "Crear lote contable";
        public const string Menu_RegenerarPreasiento = "Regenerar preasiento";
    }

    public class ltrTrazasDeUnPreasiento
    {
        public static readonly string EliminarPreasientoDelLote = "Eliminado del lote contable";
    }


    [Table(Tablas.PREASIENTO, Schema = Esquemas.CONTABILIDAD)]
    public class PreasientoDtm : ElementoDeProcesoDtm
    {
        public new TipoDePreasientoDtm Tipo { get; set; }
        public new EstadoDeUnPreasientoDtm Estado { get; set; }

        public string SociedadContable { get; set; }
        public int Ejercicio { get; set; }
        public string CodigoDiario { get; set; }
        public DateTime FechaContable { get; set; }

        public enumNegocio NegocioReferenciado { get; set; }
        public int? IdReferenciado { get; set; }


    }


    [Table(Tablas.PREASIENTO + "_" + nameof(Sufijo.AUDITORIA), Schema = Esquemas.CONTABILIDAD)]
    public class AuditoriaDeUnPreasientoDtm : AuditoriaDtm
    {
    }


    [Table(Tablas.PREASIENTO + "_" + nameof(Sufijo.OBSERVACION), Schema = Esquemas.CONTABILIDAD)]
    public class ObservacionesDeUnPreasientoDtm : ObservacionDtm
    {
        public override enumNegocio Negocio => enumNegocio.Preasiento;
    }


    [Table(Tablas.PREASIENTO + "_" + nameof(Sufijo.TRAZA), Schema = Esquemas.CONTABILIDAD)]
    public class TrazasDeUnPreasientoDtm : TrazaDtm
    {
        public override enumNegocio Negocio => enumNegocio.Preasiento;
    }

    [Table(Tablas.PREASIENTO + "_" + nameof(Sufijo.HISTORIA), Schema = Esquemas.CONTABILIDAD)]
    public class HitosDeUnPreasientoDtm : HitoDtm
    {

    }

    [Table(Tablas.PREASIENTO + "_" + nameof(Sufijo.ARCHIVADOR), Schema = Esquemas.CONTABILIDAD)]
    public class ArchivadoresDeUnPreasientoDtm : VinculoDtm
    {
        public PreasientoDtm Preasiento { get; set; }
        public ArchivadorDtm Archivador { get; set; }
    }


    [Table(Tablas.PREASIENTO + "_" + nameof(Sufijo.CIRCUITO_DOC), Schema = Esquemas.CONTABILIDAD)]
    public class CircuitoDocDeUnPreasientoDtm : VinculoDtm
    {
        public PreasientoDtm Preasiento { get; set; }
        public CircuitoDocDtm Circuito { get; set; }
    }

    [Table(Tablas.PREASIENTO + "_" + nameof(Sufijo.PERMISO), Schema = Esquemas.CONTABILIDAD)]
    public class PermisoDelPreasientoDtm : PermisosDelElementoDtm
    {
    }

    public static partial class ModeloDePreasiento
    {

        public static void Preasiento(ModelBuilder modelBuilder)
        {
            ApiDeElementoDtm.DefinirCamposDelElementoDtm<PreasientoDtm>(modelBuilder);
            ApiDeElementoDtm.DefinirCampoCg<PreasientoDtm>(modelBuilder, nameof(PreasientoDtm.Cg));
            ApiDeElementoDtm.DefinirCampoTipo<PreasientoDtm>(modelBuilder, nameof(PreasientoDtm.Tipo));
            ApiDeElementoDtm.DefinirCampoEstado<PreasientoDtm>(modelBuilder, nameof(PreasientoDtm.Estado));

            modelBuilder.Entity<PreasientoDtm>().Property(p => p.SociedadContable).HasColumnName(ICampos.CODIGO_CONTABLE).HasColumnType(IDominio.VARCHAR_4).IsRequired(true);
            modelBuilder.Entity<PreasientoDtm>().Property(p => p.CodigoDiario).HasColumnName(ICampos.CODIGO_DIARIO).HasColumnType(IDominio.VARCHAR_4).IsRequired(true);
            modelBuilder.Entity<PreasientoDtm>().Property(p => p.Ejercicio).HasColumnName(ICampos.EJERCICIO).HasColumnType(IDominio.INT).IsRequired(true);
            modelBuilder.Entity<PreasientoDtm>().Property(p => p.NegocioReferenciado).HasColumnName(ICampos.ENUMERADO).HasColumnType(IDominio.NEGOCIO_ENUMERADO).IsRequired(true).HasDefaultValue(enumNegocio.No_Definido);
            modelBuilder.Entity<PreasientoDtm>().Property(p => p.IdReferenciado).HasColumnName(ICampos.ID_ELEMENTO).HasColumnType(IDominio.INT).IsRequired(false);

            modelBuilder.Entity<PreasientoDtm>().Property(p => p.FechaContable).HasColumnName(ICampos.FECHA_CONTABLE).HasColumnType(IDominio.DATETIME_2).IsRequired(true);

        }

        internal static void Trazas(ModelBuilder modelBuilder)
        {
            ApiTraza.DefinirCampos<TrazasDeUnPreasientoDtm, PreasientoDtm>(modelBuilder);
        }

        internal static void Auditoria(ModelBuilder modelBuilder)
        {
            ApiDeAuditoria.DefinirCamposDeAuditoriaDtm<AuditoriaDeUnPreasientoDtm>(modelBuilder);
        }

        internal static void Observaciones(ModelBuilder modelBuilder)
        {
            ApiDeObservaciones.DefinirCampos<ObservacionesDeUnPreasientoDtm, PreasientoDtm>(modelBuilder);
        }


        internal static void Historia(ModelBuilder modelBuilder)
        {
            ApiDeHitos.DefinirCampos<HitosDeUnPreasientoDtm, PreasientoDtm, EstadoDeUnPreasientoDtm, TransicionesDeUnPreasientoDtm, ObservacionesDeUnPreasientoDtm>(modelBuilder);
        }

        internal static void Archivadores(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<ArchivadoresDeUnPreasientoDtm>(modelBuilder, nameof(ArchivadoresDeUnPreasientoDtm.Preasiento), nameof(ArchivadoresDeUnPreasientoDtm.Archivador));
        }

        internal static void Circuitos(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<CircuitoDocDeUnPreasientoDtm>(modelBuilder, nameof(CircuitoDocDeUnPreasientoDtm.Preasiento), nameof(CircuitoDocDeUnPreasientoDtm.Circuito));
        }

        internal static void Permisos(ModelBuilder modelBuilder)
        {
            ApiPermisosDelElemento.DefinirCampos<PermisoDelPreasientoDtm, PreasientoDtm>(modelBuilder);
        }

    }
}
