using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.SistemaDocumental;
using System.ComponentModel.DataAnnotations.Schema;
using Utilidades;

namespace ServicioDeDatos.Logistica
{

    public static class ltrDeUnAlmacen
    {
        public const string FiltrarParaRegularizar = nameof(FiltrarParaRegularizar);
    }

    [Table(Tablas.ALMACEN, Schema = Esquemas.LOGISTICA)]
    public class AlmacenDtm : ElementoDeProcesoDtm, IUsaDirecciones
    {
        public new TipoDeAlmacenDtm Tipo { get; set; }
        public new EstadoDeUnAlmacenDtm Estado { get; set; }

        // Información contable (ver ExtensorDeAlmacenes.ValidarInformacionContable): está sin definir mientras no tiene cuenta de existencias,
        // y entonces no se pueden preasentar sus movimientos. Sin variación de existencias cada movimiento va a la cuenta de existencias; con
        // ella las compras van a la de compras (o a la de gasto de la naturaleza) y las existencias se cuadran contra la de variación al cerrar
        public bool UsaVariacionDeExistencias { get; set; }
        public int? IdCuentaDeExistencias { get; set; }
        public CuentaDtm CuentaDeExistencias { get; set; }
        public int? IdCuentaDeVariacion { get; set; }
        public CuentaDtm CuentaDeVariacion { get; set; }
        public int? IdCuentaDeCompras { get; set; }
        public CuentaDtm CuentaDeCompras { get; set; }
        public int? IdCuentaDeDevoluciones { get; set; }
        public CuentaDtm CuentaDeDevoluciones { get; set; }
    }


    [Table(Tablas.ALMACEN + "_" + nameof(Sufijo.AUDITORIA), Schema = Esquemas.LOGISTICA)]
    public class AuditoriaDeUnAlmacenDtm : AuditoriaDtm
    {
    }


    [Table(Tablas.ALMACEN + "_" + nameof(Sufijo.ARCHIVO), Schema = Esquemas.LOGISTICA)]
    public class ArchivosDeUnAlmacenDtm : VinculoDtm
    {
        public AlmacenDtm Almacen { get; set; }
        public ArchivoDtm Archivo { get; set; }
    }

    [Table(Tablas.ALMACEN + "_" + nameof(Sufijo.OBSERVACION), Schema = Esquemas.LOGISTICA)]
    public class ObservacionesDeUnAlmacenDtm : ObservacionDtm
    {
        public override enumNegocio Negocio => enumNegocio.Almacen;
    }

    [Table(Tablas.ALMACEN + "_" + nameof(Sufijo.PERMISO), Schema = Esquemas.LOGISTICA)]
    public class PermisoDelAlmacenDtm : PermisosDelElementoDtm
    {
    }

    [Table(Tablas.ALMACEN + "_" + nameof(Sufijo.TRAZA), Schema = Esquemas.LOGISTICA)]
    public class TrazasDeUnAlmacenDtm : TrazaDtm
    {
        public override enumNegocio Negocio => enumNegocio.Almacen;
    }

    [Table(Tablas.ALMACEN + "_" + nameof(Sufijo.DIRECCION), Schema = Esquemas.LOGISTICA)]
    public class DireccionDeUnAlmacenDtm : DireccionDtm
    {
        public override enumNegocio Negocio => enumNegocio.Almacen;

    }

    [Table(Tablas.ALMACEN + "_" + nameof(Sufijo.HISTORIA), Schema = Esquemas.LOGISTICA)]
    public class HitosDeUnAlmacenDtm : HitoDtm
    {

    }

    [Table(Tablas.ALMACEN + "_" + nameof(Sufijo.ARCHIVADOR), Schema = Esquemas.LOGISTICA)]
    public class ArchivadoresDeUnAlmacenDtm : VinculoDtm
    {
        public AlmacenDtm Almacen { get; set; }
        public ArchivadorDtm Archivador { get; set; }
    }


    public static partial class ModeloDeAlmacen
    {

        public static void Almacen(ModelBuilder modelBuilder)
        {
            ApiDeElementoDtm.DefinirCamposDelElementoDtm<AlmacenDtm>(modelBuilder);
            ApiDeElementoDtm.DefinirCampoCg<AlmacenDtm>(modelBuilder, nameof(AlmacenDtm.Cg));
            ApiDeElementoDtm.DefinirCampoTipo<AlmacenDtm>(modelBuilder, nameof(AlmacenDtm.Tipo));
            ApiDeElementoDtm.DefinirCampoEstado<AlmacenDtm>(modelBuilder, nameof(AlmacenDtm.Estado));

            modelBuilder.Entity<AlmacenDtm>().Property(p => p.UsaVariacionDeExistencias).HasColumnName(ICampos.USA_VARIACION_EXISTENCIAS).HasColumnType(IDominio.BIT).HasDefaultValue(false).IsRequired();
            ApiDeRegistroDtm.DefinirCampoFk<AlmacenDtm>(modelBuilder, nameof(AlmacenDtm.CuentaDeExistencias), nameof(AlmacenDtm.IdCuentaDeExistencias), ICampos.ID_CUENTA_EXISTENCIAS, requerida: false, unico: false);
            ApiDeRegistroDtm.DefinirCampoFk<AlmacenDtm>(modelBuilder, nameof(AlmacenDtm.CuentaDeVariacion), nameof(AlmacenDtm.IdCuentaDeVariacion), ICampos.ID_CUENTA_VARIACION, requerida: false, unico: false);
            ApiDeRegistroDtm.DefinirCampoFk<AlmacenDtm>(modelBuilder, nameof(AlmacenDtm.CuentaDeCompras), nameof(AlmacenDtm.IdCuentaDeCompras), ICampos.ID_CUENTA_COMPRAS, requerida: false, unico: false);
            ApiDeRegistroDtm.DefinirCampoFk<AlmacenDtm>(modelBuilder, nameof(AlmacenDtm.CuentaDeDevoluciones), nameof(AlmacenDtm.IdCuentaDeDevoluciones), ICampos.ID_CUENTA_DEVOLUCIONES, requerida: false, unico: false);
        }

        internal static void Trazas(ModelBuilder modelBuilder)
        {
            ApiTraza.DefinirCampos<TrazasDeUnAlmacenDtm, AlmacenDtm>(modelBuilder);
        }

        internal static void Auditoria(ModelBuilder modelBuilder)
        {
            ApiDeAuditoria.DefinirCamposDeAuditoriaDtm<AuditoriaDeUnAlmacenDtm>(modelBuilder);
        }

        internal static void Archivos(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<ArchivosDeUnAlmacenDtm>(modelBuilder, nameof(ArchivosDeUnAlmacenDtm.Almacen), nameof(ArchivosDeUnAlmacenDtm.Archivo));

        }

        internal static void Observaciones(ModelBuilder modelBuilder)
        {
            ApiDeObservaciones.DefinirCampos<ObservacionesDeUnAlmacenDtm, AlmacenDtm>(modelBuilder);
        }

        internal static void Permisos(ModelBuilder modelBuilder)
        {
            ApiPermisosDelElemento.DefinirCampos<PermisoDelAlmacenDtm, AlmacenDtm>(modelBuilder);
        }

        internal static void Direcciones(ModelBuilder modelBuilder)
        {
            ApiDireccionDtm.DefinirCampos<DireccionDeUnAlmacenDtm, AlmacenDtm>(modelBuilder);
        }

        internal static void Historia(ModelBuilder modelBuilder)
        {
            ApiDeHitos.DefinirCampos<HitosDeUnAlmacenDtm, AlmacenDtm, EstadoDeUnAlmacenDtm, TransicionesDeUnAlmacenDtm, ObservacionesDeUnAlmacenDtm>(modelBuilder);
        }

        internal static void Archivadores(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<ArchivadoresDeUnAlmacenDtm>(modelBuilder, nameof(ArchivadoresDeUnAlmacenDtm.Almacen), nameof(ArchivadoresDeUnAlmacenDtm.Archivador));
        }

    }
}
