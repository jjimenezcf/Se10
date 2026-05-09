using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Gastos;
using ServicioDeDatos.MaestrosTecnico;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.SistemaDocumental;
using Utilidades;

namespace ServicioDeDatos.Terceros
{
    [Table(Tablas.PROVEEDOR, Schema = Esquemas.TERCEROS)]
    public class ProveedorDtm : ElementoDtm, IUsaTraza, IUsaBaja, IDatosDeContacto, IUsaDirecciones, ITerceroContable
    {
        public string eMail { get; set; }
        public string Telefono { get; set; }
        public bool Baja { get; set; }
        public int IdInterlocutor { get; set; }
        public InterlocutorDtm Interlocutor { get; set; }

        public int IdCuenta { get; set; }
        public CuentaDtm Cuenta { get; set; }

        public int? CodigoContable { get; set; }

        public int? IdTipoFarPropuesto { get; set; }
        public TipoDeFacturaRecDtm TipoFarPropuesto { get; set; }

        public int? IdCgPropuesto { get; set; }
        public CentroGestorDtm CgPropuesto { get; set; }
        public int? IdNaturaleza { get; set; }
        public NaturalezaDtm Naturaleza { get; set; }
        public int? IdUnidad { get; set; }
        public UnidadDtm Unidad { get; set; }
        public string Concepto { get; set; }

        public decimal? BiPropuesto { get; set; }

        public int? IdIvaS { get; set; }
        public IvaSoportadoDtm IvaSoportado { get; set; }

        public int? IdIrpf { get; set; }
        public IrpfDtm Irpf { get; set; }

        public enumModoDePagoContado? ModoDePago { get; set; }
        public int? IdTarjeta { get; set; }
        public int? IdDomiciliadaEn { get; set; }
        public TarjetaDeMiSociedadDtm Tarjeta { get; set; }
        public CuentaDeMiSociedadDtm DomiciliadaEn { get; set; }

    }

    [Table(Tablas.PROVEEDOR + "_" + nameof(Sufijo.AUDITORIA), Schema = Esquemas.TERCEROS)]
    public class AuditoriaDeUnProveedorDtm : AuditoriaDtm
    {
    }


    [Table(Tablas.PROVEEDOR + "_" + nameof(Sufijo.ARCHIVO), Schema = Esquemas.TERCEROS)]
    public class ArchivosDeUnProveedorDtm : VinculoDtm
    {
        public ProveedorDtm Proveedor { get; set; }
        public ArchivoDtm Archivo { get; set; }
    }

    [Table(Tablas.PROVEEDOR + "_" + nameof(Sufijo.OBSERVACION), Schema = Esquemas.TERCEROS)]
    public class ObservacionesDeUnProveedorDtm : ObservacionDtm
    {
        public override enumNegocio Negocio => enumNegocio.Proveedor;
    }

    [Table(Tablas.PROVEEDOR + "_" + nameof(Sufijo.TRAZA), Schema = Esquemas.TERCEROS)]
    public class TrazasDeUnProveedorDtm : TrazaDtm
    {
        public override enumNegocio Negocio => enumNegocio.Proveedor;
    }


    [Table(Tablas.PROVEEDOR + "_" + nameof(Sufijo.DIRECCION), Schema = Esquemas.TERCEROS)]
    public class DireccionDeUnProveedorDtm : DireccionDtm
    {
        public override enumNegocio Negocio => enumNegocio.Proveedor;
    }

    public static partial class ModeloDeTerceros
    {
        public static void Proveedor(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<ProveedorDtm>().Ignore(x => x.DomiciliadaEn);
            modelBuilder.Entity<ProveedorDtm>().Ignore(x => x.Tarjeta);

            ApiDeElementoDtm.DefinirCamposDelElementoDtm<ProveedorDtm>(modelBuilder, indiceUnicoPorNombre: true);
            ApiDeRegistroDtm.DefinirDatosDeContacto<ProveedorDtm>(modelBuilder);
            modelBuilder.Entity<ProveedorDtm>().Property(p => p.CodigoContable).HasColumnName(ICampos.CODIGO_CONTABLE).HasColumnType(IDominio.DECIMAL_4).IsRequired(false);
            modelBuilder.Entity<ProveedorDtm>().HasIndex(x => new { x.CodigoContable }).IsUnique(true).HasDatabaseName($"I_{Tablas.PROVEEDOR}_{ICampos.CODIGO_CONTABLE}");
            ApiDeRegistroDtm.DefinirCampoFk<ProveedorDtm>(modelBuilder, nameof(ProveedorDtm.Interlocutor), nameof(ProveedorDtm.IdInterlocutor), ICampos.ID_INTERLOCUTOR, requerida: true, unico: true);
            ApiDeRegistroDtm.DefinirCampoFk<ProveedorDtm>(modelBuilder, nameof(ProveedorDtm.Cuenta), nameof(ProveedorDtm.IdCuenta), ICampos.ID_CUENTA, requerida: true, unico: false);
            ApiDeRegistroDtm.DefinirCampoFk<ProveedorDtm>(modelBuilder, nameof(ProveedorDtm.CgPropuesto), nameof(ProveedorDtm.IdCgPropuesto), ICampos.ID_CG, requerida: false, unico: false);
            ApiDeRegistroDtm.DefinirCampoFk<ProveedorDtm>(modelBuilder, nameof(ProveedorDtm.TipoFarPropuesto), nameof(ProveedorDtm.IdTipoFarPropuesto), ICampos.ID_TIPO_FAR, requerida: false, unico: false);


            modelBuilder.Entity<ProveedorDtm>().Property(nameof(ProveedorDtm.Concepto)).HasColumnName(ICampos.CONCEPTO).HasColumnType(IDominio.VARCHAR_250).IsRequired(false);
            ApiDeRegistroDtm.DefinirCampoFk<ProveedorDtm>(modelBuilder, nameof(ProveedorDtm.Naturaleza), nameof(ProveedorDtm.IdNaturaleza), ICampos.ID_NATURALEZA, requerida: false, unico: false);
            ApiDeRegistroDtm.DefinirCampoFk<ProveedorDtm>(modelBuilder, nameof(ProveedorDtm.Unidad), nameof(ProveedorDtm.IdUnidad), ICampos.ID_UNIDAD, requerida: false, unico: false);

            modelBuilder.Entity<ProveedorDtm>().Property(nameof(ProveedorDtm.BiPropuesto)).HasColumnName(ICampos.BI).HasColumnType(IDominio.DECIMAL).IsRequired(false);

            ApiDeRegistroDtm.DefinirCampoFk<ProveedorDtm>(modelBuilder, nameof(ProveedorDtm.IvaSoportado), nameof(ProveedorDtm.IdIvaS), ICampos.ID_IVA_S, requerida: false, unico: false);
            ApiDeRegistroDtm.DefinirCampoFk<ProveedorDtm>(modelBuilder, nameof(ProveedorDtm.Irpf), nameof(ProveedorDtm.IdIrpf), ICampos.ID_IRPF, requerida: false, unico: false);

            modelBuilder.Entity<ProveedorDtm>().Property(nameof(ProveedorDtm.ModoDePago)).HasColumnName(ICampos.MODO).HasColumnType(IDominio.VARCHAR_30).IsRequired(false);
            ApiDeRegistroDtm.DefinirCampoFk<ProveedorDtm>(modelBuilder, nameof(ProveedorDtm.Tarjeta), nameof(ProveedorDtm.IdTarjeta), ICampos.ID_TARJETA, requerida: false, unico: false);
            ApiDeRegistroDtm.DefinirCampoFk<ProveedorDtm>(modelBuilder, nameof(ProveedorDtm.DomiciliadaEn), nameof(ProveedorDtm.IdDomiciliadaEn), ICampos.ID_CUENTA_CARGO, requerida: false, unico: false);
        }

        public static void ProveedorAudt(ModelBuilder modelBuilder)
        {
            ApiDeAuditoria.DefinirCamposDeAuditoriaDtm<AuditoriaDeUnProveedorDtm>(modelBuilder);
        }

        internal static void ArchivosDeUnProveedor(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<ArchivosDeUnProveedorDtm>(modelBuilder, nameof(ArchivosDeUnProveedorDtm.Proveedor), nameof(ArchivosDeUnProveedorDtm.Archivo));
        }

        internal static void ObservacionesDeUnProveedor(ModelBuilder modelBuilder)
        {
            ApiDeObservaciones.DefinirCampos<ObservacionesDeUnProveedorDtm, ProveedorDtm>(modelBuilder);
        }

        internal static void DireccionesDeUnProveedor(ModelBuilder modelBuilder)
        {
            ApiDireccionDtm.DefinirCampos<DireccionDeUnProveedorDtm, ProveedorDtm>(modelBuilder);
        }
        internal static void TrazasDeUnProveedor(ModelBuilder modelBuilder)
        {
            ApiTraza.DefinirCampos<TrazasDeUnProveedorDtm, ProveedorDtm>(modelBuilder);
        }


    }

}

