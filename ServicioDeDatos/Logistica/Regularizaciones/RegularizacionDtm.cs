using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.SistemaDocumental;
using System.ComponentModel.DataAnnotations.Schema;
using Utilidades;

namespace ServicioDeDatos.Logistica
{

    [Table(Tablas.REGULARIZACION, Schema = Esquemas.LOGISTICA)]
    public class RegularizacionDtm : ElementoDeProcesoDtm
    {
        public int IdAlmacen { get; set; }
        public AlmacenDtm Almacen { get; set; }

        public new TipoDeRegularizacionDtm Tipo { get; set; }
        public new EstadoDeUnaRegularizacionDtm Estado { get; set; }
    }


    [Table(Tablas.REGULARIZACION + "_" + nameof(Sufijo.AUDITORIA), Schema = Esquemas.LOGISTICA)]
    public class AuditoriaDeUnaRegularizacionDtm : AuditoriaDtm
    {
    }


    [Table(Tablas.REGULARIZACION + "_" + nameof(Sufijo.ARCHIVO), Schema = Esquemas.LOGISTICA)]
    public class ArchivosDeUnaRegularizacionDtm : VinculoDtm
    {
        public RegularizacionDtm Regularizacion { get; set; }
        public ArchivoDtm Archivo { get; set; }
    }

    [Table(Tablas.REGULARIZACION + "_" + nameof(Sufijo.OBSERVACION), Schema = Esquemas.LOGISTICA)]
    public class ObservacionesDeUnaRegularizacionDtm : ObservacionDtm
    {
        public override enumNegocio Negocio => enumNegocio.Regularizacion;
    }

    [Table(Tablas.REGULARIZACION + "_" + nameof(Sufijo.PERMISO), Schema = Esquemas.LOGISTICA)]
    public class PermisoDeLaRegularizacionDtm : PermisosDelElementoDtm
    {
    }

    [Table(Tablas.REGULARIZACION + "_" + nameof(Sufijo.TRAZA), Schema = Esquemas.LOGISTICA)]
    public class TrazasDeUnaRegularizacionDtm : TrazaDtm
    {
        public override enumNegocio Negocio => enumNegocio.Regularizacion;
    }

    [Table(Tablas.REGULARIZACION + "_" + nameof(Sufijo.HISTORIA), Schema = Esquemas.LOGISTICA)]
    public class HitosDeUnaRegularizacionDtm : HitoDtm
    {

    }

    [Table(Tablas.REGULARIZACION + "_" + nameof(Sufijo.ARCHIVADOR), Schema = Esquemas.LOGISTICA)]
    public class ArchivadoresDeUnaRegularizacionDtm : VinculoDtm
    {
        public RegularizacionDtm Regularizacion { get; set; }
        public ArchivadorDtm Archivador { get; set; }
    }


    public static partial class ModeloDeRegularizacion
    {

        public static void Regularizacion(ModelBuilder modelBuilder)
        {
            ApiDeElementoDtm.DefinirCamposDelElementoDtm<RegularizacionDtm>(modelBuilder);
            ApiDeElementoDtm.DefinirCampoCg<RegularizacionDtm>(modelBuilder, nameof(RegularizacionDtm.Cg));
            ApiDeElementoDtm.DefinirCampoTipo<RegularizacionDtm>(modelBuilder, nameof(RegularizacionDtm.Tipo));
            ApiDeElementoDtm.DefinirCampoEstado<RegularizacionDtm>(modelBuilder, nameof(RegularizacionDtm.Estado));

            ApiDeRegistroDtm.DefinirCampoFk<RegularizacionDtm>(modelBuilder, nameof(RegularizacionDtm.Almacen), nameof(RegularizacionDtm.IdAlmacen), ICampos.ID_ALMACEN, requerida: true, unico: false);
        }

        internal static void Trazas(ModelBuilder modelBuilder)
        {
            ApiTraza.DefinirCampos<TrazasDeUnaRegularizacionDtm, RegularizacionDtm>(modelBuilder);
        }

        internal static void Auditoria(ModelBuilder modelBuilder)
        {
            ApiDeAuditoria.DefinirCamposDeAuditoriaDtm<AuditoriaDeUnaRegularizacionDtm>(modelBuilder);
        }

        internal static void Archivos(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<ArchivosDeUnaRegularizacionDtm>(modelBuilder, nameof(ArchivosDeUnaRegularizacionDtm.Regularizacion), nameof(ArchivosDeUnaRegularizacionDtm.Archivo));
        }

        internal static void Observaciones(ModelBuilder modelBuilder)
        {
            ApiDeObservaciones.DefinirCampos<ObservacionesDeUnaRegularizacionDtm, RegularizacionDtm>(modelBuilder);
        }

        internal static void Permisos(ModelBuilder modelBuilder)
        {
            ApiPermisosDelElemento.DefinirCampos<PermisoDeLaRegularizacionDtm, RegularizacionDtm>(modelBuilder);
        }

        internal static void Historia(ModelBuilder modelBuilder)
        {
            ApiDeHitos.DefinirCampos<HitosDeUnaRegularizacionDtm, RegularizacionDtm, EstadoDeUnaRegularizacionDtm, TransicionesDeUnaRegularizacionDtm, ObservacionesDeUnaRegularizacionDtm>(modelBuilder);
        }

        internal static void Archivadores(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<ArchivadoresDeUnaRegularizacionDtm>(modelBuilder, nameof(ArchivadoresDeUnaRegularizacionDtm.Regularizacion), nameof(ArchivadoresDeUnaRegularizacionDtm.Archivador));
        }

    }
}
