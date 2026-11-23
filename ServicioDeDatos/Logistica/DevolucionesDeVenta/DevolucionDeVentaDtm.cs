using System;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Terceros;
using System.ComponentModel.DataAnnotations.Schema;
using Utilidades;

namespace ServicioDeDatos.Logistica
{
    public class ltrDeUnaDevolucionDeVenta
    {
        // devoluciones en tramitación (ver FiltrosDeDevolucionesDeVenta), las que se proponen al devolver una línea vendida
        public static string FiltroEnTramitacion => nameof(FiltroEnTramitacion);
        public static string FiltroPorCliente => nameof(FiltroPorCliente);
        // acción con la que el sistema quita de la prefactura lo devuelto al aceptar la devolución (ver ExtensorDeDevolucionesDeVenta.AjustarPrefactura)
        public const string Accion_AjustarPrefactura = nameof(Accion_AjustarPrefactura);
        // líneas devueltas de un documento de venta: es la propiedad restrictora (IdVenta del dto de la línea) del grid de devoluciones de
        // la edición del documento de venta
        public static string FiltroPorVenta => "IdVenta";
    }

    // Devolución de venta: lo que un cliente devuelve de uno o varios documentos de venta (sus líneas apuntan a las líneas vendidas). Al
    // aceptarla el material vuelve a entrar en el almacén de cada venta y se hacen las prefacturas rectificativas de lo facturado
    [Table(Tablas.DEVOLUCIONVENTA, Schema = Esquemas.LOGISTICA)]
    public class DevolucionDeVentaDtm : ElementoDeProcesoDtm, IUsaCliente
    {
        public int IdCliente { get; set; }
        public ClienteDtm Cliente { get; }
        public string Contacto { get; set; }
        public string Telefono { get; set; }
        public string eMail { get; set; }

        // fecha con la que se hacen los movimientos de almacén al aceptarla; si no se indica, la del día
        public DateTime? DevueltaEl { get; set; }

        public new TipoDeDevolucionDeVentaDtm Tipo { get; set; }
        public new EstadoDeUnaDevolucionDeVentaDtm Estado { get; set; }
    }


    [Table(Tablas.DEVOLUCIONVENTA + "_" + nameof(Sufijo.AUDITORIA), Schema = Esquemas.LOGISTICA)]
    public class AuditoriaDeUnaDevolucionDeVentaDtm : AuditoriaDtm
    {
    }


    [Table(Tablas.DEVOLUCIONVENTA + "_" + nameof(Sufijo.ARCHIVO), Schema = Esquemas.LOGISTICA)]
    public class ArchivosDeUnaDevolucionDeVentaDtm : VinculoDtm
    {
        public DevolucionDeVentaDtm DevolucionDeVenta { get; set; }
        public ArchivoDtm Archivo { get; set; }
    }

    [Table(Tablas.DEVOLUCIONVENTA + "_" + nameof(Sufijo.OBSERVACION), Schema = Esquemas.LOGISTICA)]
    public class ObservacionesDeUnaDevolucionDeVentaDtm : ObservacionDtm
    {
        public override enumNegocio Negocio => enumNegocio.DevolucionVenta;
    }

    [Table(Tablas.DEVOLUCIONVENTA + "_" + nameof(Sufijo.PERMISO), Schema = Esquemas.LOGISTICA)]
    public class PermisoDeLaDevolucionDeVentaDtm : PermisosDelElementoDtm
    {
    }

    [Table(Tablas.DEVOLUCIONVENTA + "_" + nameof(Sufijo.TRAZA), Schema = Esquemas.LOGISTICA)]
    public class TrazasDeUnaDevolucionDeVentaDtm : TrazaDtm
    {
        public override enumNegocio Negocio => enumNegocio.DevolucionVenta;
    }

    [Table(Tablas.DEVOLUCIONVENTA + "_" + nameof(Sufijo.HISTORIA), Schema = Esquemas.LOGISTICA)]
    public class HitosDeUnaDevolucionDeVentaDtm : HitoDtm
    {

    }

    [Table(Tablas.DEVOLUCIONVENTA + "_" + nameof(Sufijo.ARCHIVADOR), Schema = Esquemas.LOGISTICA)]
    public class ArchivadoresDeUnaDevolucionDeVentaDtm : VinculoDtm
    {
        public DevolucionDeVentaDtm DevolucionDeVenta { get; set; }
        public ArchivadorDtm Archivador { get; set; }
    }


    public static partial class ModeloDeDevolucionDeVenta
    {

        public static void DevolucionDeVenta(ModelBuilder modelBuilder)
        {
            ApiDeElementoDtm.DefinirCamposDelElementoDtm<DevolucionDeVentaDtm>(modelBuilder);
            ApiDeElementoDtm.DefinirCampoCg<DevolucionDeVentaDtm>(modelBuilder, nameof(DevolucionDeVentaDtm.Cg));
            ApiDeElementoDtm.DefinirCampoTipo<DevolucionDeVentaDtm>(modelBuilder, nameof(DevolucionDeVentaDtm.Tipo));
            ApiDeElementoDtm.DefinirCampoEstado<DevolucionDeVentaDtm>(modelBuilder, nameof(DevolucionDeVentaDtm.Estado));
            ApiDeElementoDtm.DefinirCliente<DevolucionDeVentaDtm>(modelBuilder);

            modelBuilder.Entity<DevolucionDeVentaDtm>().Property(p => p.DevueltaEl).HasColumnName(ICampos.DEVUELTA_EL).HasColumnType(IDominio.DATETIME_2).IsRequired(false);
        }

        internal static void Trazas(ModelBuilder modelBuilder)
        {
            ApiTraza.DefinirCampos<TrazasDeUnaDevolucionDeVentaDtm, DevolucionDeVentaDtm>(modelBuilder);
        }

        internal static void Auditoria(ModelBuilder modelBuilder)
        {
            ApiDeAuditoria.DefinirCamposDeAuditoriaDtm<AuditoriaDeUnaDevolucionDeVentaDtm>(modelBuilder);
        }

        internal static void Archivos(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<ArchivosDeUnaDevolucionDeVentaDtm>(modelBuilder, nameof(ArchivosDeUnaDevolucionDeVentaDtm.DevolucionDeVenta), nameof(ArchivosDeUnaDevolucionDeVentaDtm.Archivo));
        }

        internal static void Observaciones(ModelBuilder modelBuilder)
        {
            ApiDeObservaciones.DefinirCampos<ObservacionesDeUnaDevolucionDeVentaDtm, DevolucionDeVentaDtm>(modelBuilder);
        }

        internal static void Permisos(ModelBuilder modelBuilder)
        {
            ApiPermisosDelElemento.DefinirCampos<PermisoDeLaDevolucionDeVentaDtm, DevolucionDeVentaDtm>(modelBuilder);
        }

        internal static void Historia(ModelBuilder modelBuilder)
        {
            ApiDeHitos.DefinirCampos<HitosDeUnaDevolucionDeVentaDtm, DevolucionDeVentaDtm, EstadoDeUnaDevolucionDeVentaDtm, TransicionesDeUnaDevolucionDeVentaDtm, ObservacionesDeUnaDevolucionDeVentaDtm>(modelBuilder);
        }

        internal static void Archivadores(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<ArchivadoresDeUnaDevolucionDeVentaDtm>(modelBuilder, nameof(ArchivadoresDeUnaDevolucionDeVentaDtm.DevolucionDeVenta), nameof(ArchivadoresDeUnaDevolucionDeVentaDtm.Archivador));
        }

    }
}
