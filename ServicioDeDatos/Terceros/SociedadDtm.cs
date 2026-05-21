
using Gestor.Errores;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Callejero;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.SistemaDocumental;
using System.ComponentModel.DataAnnotations.Schema;
using Utilidades;

namespace ServicioDeDatos.Terceros
{

    public static class ltrDeSociedad
    {
        public const string FiltroParaSociedadesDeClientes = nameof(FiltroParaSociedadesDeClientes);
        public const string FiltroParaSociedadesGestionadas = nameof(FiltroParaSociedadesGestionadas);
        public const string CentroGestorDeClientesWeb = "Clientes WEB";
        public const string CentroGestorDeDocumentacion = "Documentacion del sistema";
        public const string CodigoCgClienteWeb = "WEB";
        public const string CodigoCgDocumentacion = "SDC";
        public const string SiglasDelCGDeClienteWeb = "CGC";
        public const string SiglasDelCGDeDocumentacion = "CGD";
        public static string FiltoPorCg = nameof(FiltoPorCg);
        public static string ConContactos = nameof(ConContactos);
        public static string ConCertificado = nameof(ConCertificado);
        public const string Procuradores = nameof(Procuradores);
        public const string Abogados = nameof(Abogados);
        public const string Proveedores = nameof(Proveedores);
        public const string Clientes = nameof(Clientes);
        public const string Interlocutores = nameof(Interlocutores);
        public const string SociedadNula = Literal.Cero;
        public const string FiltroPorIdSociedad = nameof(FiltroPorIdSociedad);
    }


    [Table(Tablas.SOCIEDAD, Schema = Esquemas.TERCEROS)]
    public class SociedadDtm : ElementoDtm, IUsaTraza, IUsaBaja, IEsUnTercero, ITieneReferencia, IUsaArchivo, IPuedeUsarAgenda, IUsaAmpliaciones, IUsaDirecciones
    {
        public string RazonSocial { get; set; }
        public string CodigoFiscal { get; set; }
        public string NIF { get; set; }
        public string eMail { get; set; }
        public string Telefono { get; set; }
        public bool Baja { get; set; }
        public override string Expresion => $"({NIF}) {base.Expresion}";
        public bool EsInterlocutor { get; set; }
        public string Referencia => NIF.ToUpper();
        public int? IdAgenda { get; set; }
        public string MiAgenda => $"Agenda de la Sociedad: {Referencia}";

        public string NIFConIsoEs => PonerIsoDeNif(NIF); //!NIF.StartsWith(ltrIsoPaises.Spain) ? (ltrIsoPaises.Spain + NIF).ToUpper().Trim() : NIF.ToUpper().Trim();
        public string NIFSinIsoEs => QuitarIsoDeNif(NIF); // NIF.StartsWith(ltrIsoPaises.Spain) ? NIF.Replace(ltrIsoPaises.Spain,"").ToUpper().Trim() : NIF.ToUpper().Trim();
        public AgendaDtm Agenda { get; set; }
        public int? IdArchivo { get; set; }
        public ArchivoDtm Archivo { get; set; }

        public ParametrosDeMiSociedadDtm Parametros { get; set; }

        public string CodigoContable
        {
            get
            {
                if (CodigoFiscal.IsNullOrEmpty())
                    GestorDeErrores.Emitir($"Ha de indicar el código fiscal de la sociedad '{Expresion}'");
                return CodigoFiscal;
            }
        }

        public bool Autonomo => ApiDeTerceros.TipoDeTerceroEsp(NIFConIsoEs) == enumTipoTercero.Autonomo;

        public static string QuitarIsoDeNif(string nif)
        {
            if (ApiDeTerceros.ClaseDeNacionalidad(nif) != enumClaseDeNacionalidad.Nacional)
                return nif.ToUpper().Trim();

            return nif.StartsWith(ltrIsoPaises.Spain) ? nif.Replace(ltrIsoPaises.Spain, "").ToUpper().Trim() : nif.ToUpper().Trim();
        }
        public static string PonerIsoDeNif(string nif)
        {
            if (ApiDeTerceros.ClaseDeNacionalidad(nif) != enumClaseDeNacionalidad.Nacional)
                return nif.ToUpper().Trim();

            return !nif.StartsWith(ltrIsoPaises.Spain) ? (ltrIsoPaises.Spain + nif).ToUpper().Trim() : nif.ToUpper().Trim();
        }
    }

    [Table(Tablas.SOCIEDAD + "_" + nameof(Sufijo.AUDITORIA), Schema = Esquemas.TERCEROS)]
    public class AuditoriaDeUnaSociedadDtm : AuditoriaDtm
    {
    }

    [Table(Tablas.SOCIEDAD + "_" + nameof(Sufijo.CERTIFICADO), Schema = Esquemas.TERCEROS)]
    public class CertificadosDeUnaSociedadDtm : VinculoDtm
    {
        public SociedadDtm Sociedad { get; set; }
        public CertificadoDtm Certificado { get; set; }
    }

    [Table(Tablas.SOCIEDAD + "_" + nameof(Sufijo.AGENDA_EVENTO), Schema = Esquemas.TERCEROS)]
    public class AgendaDeUnaSociedadDtm : VinculoDtm
    {
        public SociedadDtm Sociedad { get; set; }
        public EventoDeAgendaDtm Evento { get; set; }
    }


    [Table(Tablas.SOCIEDAD + "_" + nameof(Sufijo.ARCHIVO), Schema = Esquemas.TERCEROS)]
    public class ArchivosDeUnaSociedadDtm : VinculoDtm
    {
        public SociedadDtm Sociedad { get; set; }
        public ArchivoDtm Archivo { get; set; }
    }

    [Table(Tablas.SOCIEDAD + "_" + nameof(Sufijo.OBSERVACION), Schema = Esquemas.TERCEROS)]
    public class ObservacionesDeUnaSociedadDtm : ObservacionDtm
    {
        public override enumNegocio Negocio => enumNegocio.Sociedad;
    }

    [Table(Tablas.SOCIEDAD + "_" + nameof(Sufijo.TRAZA), Schema = Esquemas.TERCEROS)]
    public class TrazasDeUnaSociedadDtm : TrazaDtm
    {
        public override enumNegocio Negocio => enumNegocio.Sociedad;
    }


    [Table(Tablas.SOCIEDAD + "_" + nameof(Sufijo.PERMISO), Schema = Esquemas.TERCEROS)]
    public class PermisoDeLaSociedadDtm : PermisosDelElementoDtm
    {
    }

    [Table(Tablas.SOCIEDAD + "_" + nameof(Sufijo.DIRECCION), Schema = Esquemas.TERCEROS)]
    public class DireccionDeLaSociedadDtm : DireccionDtm
    {
        public override enumNegocio Negocio => enumNegocio.Sociedad;
    }

    [Table(Tablas.SOCIEDAD + "_" + nameof(Sufijo.ARCHIVADOR), Schema = Esquemas.TERCEROS)]
    public class ArchivadoresDeUnaSociedadDtm : VinculoDtm
    {
        public SociedadDtm Sociedad { get; set; }
        public ArchivadorDtm Archivador { get; set; }
    }

    public static partial class ModeloDeTerceros
    {
        public static void Sociedad(ModelBuilder modelBuilder)
        {
            ApiDeElementoDtm.DefinirCamposDelElementoDtm<SociedadDtm>(modelBuilder);

            modelBuilder.Entity<SociedadDtm>().Property(p => p.RazonSocial).HasColumnName(ICampos.RAZON_SOCIAL).HasColumnType(IDominio.VARCHAR_255).IsRequired();
            modelBuilder.Entity<SociedadDtm>()
                 .Property(p => p.CodigoFiscal)
                 .HasColumnName(ICampos.CODIGO_FISCAL)
                 .HasColumnType(IDominio.VARCHAR_4)
                 .IsRequired(false)
                 .HasConversion(
                     v => string.IsNullOrWhiteSpace(v) ? null : v,
                     v => v ?? string.Empty
                 );

            modelBuilder.Entity<SociedadDtm>().Property(p => p.NIF).HasColumnName(ICampos.NIF).HasColumnType(IDominio.VARCHAR_25).IsRequired();
            ApiDeInterlocutorDtm.DefinirCamposUsaInterlocutor<SociedadDtm>(modelBuilder);

            modelBuilder.Entity<SociedadDtm>().HasIndex(x => new { x.NIF }).IsUnique(true).HasDatabaseName($"I_{Tablas.SOCIEDAD}_{ICampos.NIF}");
            modelBuilder.Entity<SociedadDtm>().HasIndex(x => new { x.CodigoFiscal }).IsUnique(true).HasDatabaseName($"I_{Tablas.SOCIEDAD}_{ICampos.CODIGO_FISCAL}");

            ApiDeElementoDtm.DefinirCampoAgenda<SociedadDtm>(modelBuilder, obligatorio: false, unico: true);
            ApiDeElementoDtm.DefinirCampoArchivo<SociedadDtm>(modelBuilder);

            modelBuilder.Entity<SociedadDtm>().Ignore(x => x.Parametros);
            modelBuilder.Entity<SociedadDtm>().Ignore(x => x.CodigoContable);

        }

        public static void SociedadAudt(ModelBuilder modelBuilder)
        {
            ApiDeAuditoria.DefinirCamposDeAuditoriaDtm<AuditoriaDeUnaSociedadDtm>(modelBuilder);
        }

        internal static void CertificadosDeUnaSociedad(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<CertificadosDeUnaSociedadDtm>(modelBuilder, nameof(CertificadosDeUnaSociedadDtm.Sociedad), nameof(CertificadosDeUnaSociedadDtm.Certificado));
        }

        internal static void ArchivosDeUnaSociedad(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<ArchivosDeUnaSociedadDtm>(modelBuilder, nameof(ArchivosDeUnaSociedadDtm.Sociedad), nameof(ArchivosDeUnaSociedadDtm.Archivo));
        }

        internal static void ObservacionesDeUnaSociedad(ModelBuilder modelBuilder)
        {
            ApiDeObservaciones.DefinirCampos<ObservacionesDeUnaSociedadDtm, SociedadDtm>(modelBuilder);
        }

        internal static void PermisosPorSociedad(ModelBuilder modelBuilder)
        {
            ApiPermisosDelElemento.DefinirCampos<PermisoDeLaSociedadDtm, SociedadDtm>(modelBuilder);
        }
        internal static void DireccionesDeUnaSociedad(ModelBuilder modelBuilder)
        {
            ApiDireccionDtm.DefinirCampos<DireccionDeLaSociedadDtm, SociedadDtm>(modelBuilder);
        }
        internal static void ArchivadoresDeUnaSociedad(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<ArchivadoresDeUnaSociedadDtm>(modelBuilder, nameof(ArchivadoresDeUnaSociedadDtm.Sociedad), nameof(ArchivadoresDeUnaSociedadDtm.Archivador));
        }
        internal static void TrazasDeUnaSociedad(ModelBuilder modelBuilder)
        {
            ApiTraza.DefinirCampos<TrazasDeUnaSociedadDtm, SociedadDtm>(modelBuilder);
        }

    }
}
