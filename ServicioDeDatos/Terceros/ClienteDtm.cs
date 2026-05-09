using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.SistemaDocumental;
using Utilidades;

namespace ServicioDeDatos.Terceros
{
    public static class ltrCliente
    {
        public static readonly string Cliente = nameof(Cliente);
        public static readonly string IdPersona = nameof(IdPersona);
        public static readonly string IdSociedad = nameof(IdSociedad);
        public static readonly string OtorgarPermisos = nameof(OtorgarPermisos);    }

    public static class msgCliente
    {
        public static readonly string FaltaParametroTipoArchivador = $"Debe indicar el nombre del tipo de archivador con el que crear los archivadores de un cliente en el parámetro '{enumParametrosDeCliente.CLI_TipoArchivador}'";
        public static readonly string FaltaParametroCg = $"Debe indicar el código del CG donde crear los archivadores de un cliente en el parámetro '{enumParametrosDeCliente.CLI_CG_De_Cliente}'";

    }


    [Table(Tablas.CLIENTE, Schema = Esquemas.TERCEROS)]
    public class ClienteDtm : ElementoDtm, IUsaBaja, IDatosDeContacto, IUsaTraza, IUsaDirecciones, ITerceroContable
    {
        public string eMail { get; set; }
        public string Telefono { get; set; }
        public bool Baja { get; set; }
        public int IdInterlocutor { get; set; }
        public InterlocutorDtm Interlocutor { get; set; }
        public int IdCuenta { get; set; }
        public CuentaDtm Cuenta { get; set; }
        public int? CodigoContable { get; set; }
        public string VAT { get; set; }
    }

    [Table(Tablas.CLIENTE + "_" + nameof(Sufijo.AUDITORIA), Schema = Esquemas.TERCEROS)]
    public class AuditoriaDeUnClienteDtm : AuditoriaDtm
    {
    }


    [Table(Tablas.CLIENTE + "_" + nameof(Sufijo.ARCHIVO), Schema = Esquemas.TERCEROS)]
    public class ArchivosDeUnClienteDtm : VinculoDtm
    {
        public ClienteDtm Cliente { get; set; }
        public ArchivoDtm Archivo { get; set; }
    }

    [Table(Tablas.CLIENTE + "_" + nameof(Sufijo.OBSERVACION), Schema = Esquemas.TERCEROS)]
    public class ObservacionesDeUnClienteDtm : ObservacionDtm
    {
        public override enumNegocio Negocio => enumNegocio.Cliente;
    }

    [Table(Tablas.CLIENTE + "_" + nameof(Sufijo.TRAZA), Schema = Esquemas.TERCEROS)]
    public class TrazasDeUnClienteDtm : TrazaDtm
    {
        public override enumNegocio Negocio => enumNegocio.Cliente;
    }


    [Table(Tablas.CLIENTE + "_" + nameof(Sufijo.DIRECCION), Schema = Esquemas.TERCEROS)]
    public class DireccionDeUnClienteDtm : DireccionDtm
    {
        public override enumNegocio Negocio => enumNegocio.Cliente;
    }

    [Table(Tablas.CLIENTE + "_" + nameof(Sufijo.ARCHIVADOR), Schema = Esquemas.TERCEROS)]
    public class ArchivadoresDeUnClienteDtm : VinculoDtm
    {
        public ClienteDtm Cliente { get; set; }
        public ArchivadorDtm Archivador { get; set; }
    }


    public static partial class ModeloDeTerceros
    {
        public static void Cliente(ModelBuilder modelBuilder)
        {
            ApiDeElementoDtm.DefinirCamposDelElementoDtm<ClienteDtm>(modelBuilder, indiceUnicoPorNombre: true);
            ApiDeRegistroDtm.DefinirDatosDeContacto<ClienteDtm>(modelBuilder);
            modelBuilder.Entity<ClienteDtm>().Property(p => p.CodigoContable).HasColumnName(ICampos.CODIGO_CONTABLE).HasColumnType(IDominio.DECIMAL_4).IsRequired(false);
            modelBuilder.Entity<ClienteDtm>().HasIndex(x => new { x.CodigoContable }).IsUnique(true).HasDatabaseName($"I_{Tablas.CLIENTE}_{ICampos.CODIGO_CONTABLE}");
            ApiDeRegistroDtm.DefinirCampoFk<ClienteDtm>(modelBuilder, nameof(ClienteDtm.Interlocutor), nameof(ClienteDtm.IdInterlocutor), ICampos.ID_INTERLOCUTOR, requerida: true, unico: true);
            ApiDeRegistroDtm.DefinirCampoFk<ClienteDtm>(modelBuilder, nameof(ClienteDtm.Cuenta), nameof(ClienteDtm.IdCuenta), ICampos.ID_CUENTA, requerida: true, unico: false);
            modelBuilder.Entity<ClienteDtm>().Property(p => p.VAT).HasColumnName(ICampos.VAT).HasColumnType(IDominio.VARCHAR_25).IsRequired(false);

        }

        public static void ClienteAudt(ModelBuilder modelBuilder)
        {
            ApiDeAuditoria.DefinirCamposDeAuditoriaDtm<AuditoriaDeUnClienteDtm>(modelBuilder);
        }

        internal static void ArchivosDeUnCliente(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<ArchivosDeUnClienteDtm>(modelBuilder, nameof(ArchivosDeUnClienteDtm.Cliente), nameof(ArchivosDeUnClienteDtm.Archivo));
        }

        internal static void ObservacionesDeUnCliente(ModelBuilder modelBuilder)
        {
            ApiDeObservaciones.DefinirCampos<ObservacionesDeUnClienteDtm, ClienteDtm>(modelBuilder);
        }

        internal static void DireccionesDeUnCliente(ModelBuilder modelBuilder)
        {
            ApiDireccionDtm.DefinirCampos<DireccionDeUnClienteDtm, ClienteDtm>(modelBuilder);
        }

        internal static void Archivadores(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<ArchivadoresDeUnClienteDtm>(modelBuilder, nameof(ArchivadoresDeUnClienteDtm.Cliente), nameof(ArchivadoresDeUnClienteDtm.Archivador));
        }

        internal static void TrazasDeUnCliente(ModelBuilder modelBuilder)
        {
            ApiTraza.DefinirCampos<TrazasDeUnClienteDtm, ClienteDtm>(modelBuilder);
        }


    }

}

