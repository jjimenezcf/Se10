using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.SistemaDocumental;
using System.ComponentModel.DataAnnotations.Schema;
using Utilidades;

namespace ServicioDeDatos.Logistica
{

    [Table(Tablas.ALMACEN, Schema = Esquemas.LOGISTICA)]
    public class AlmacenDtm : ElementoDeProcesoDtm, IUsaDirecciones
    {
        public new TipoDeAlmacenDtm Tipo { get; set; }
        public new EstadoDeUnAlmacenDtm Estado { get; set; }
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
