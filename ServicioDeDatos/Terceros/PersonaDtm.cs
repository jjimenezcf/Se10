using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Callejero;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.SistemaDocumental;
using System.ComponentModel.DataAnnotations.Schema;
using Utilidades;

namespace ServicioDeDatos.Terceros
{
    public static class ltrDePersonas
    {
        public const string Procuradores = nameof(Procuradores);
        public const string Abogados = nameof(Abogados);
        public const string Proveedores = nameof(Proveedores);
        public const string Clientes = nameof(Clientes);
        public const string Interlocutores = nameof(Interlocutores);
        public const string FiltarPorNombreDeMadrePadre = nameof(FiltarPorNombreDeMadrePadre);
        public static readonly string Personas = nameof(Personas);
    }

    [Table(Tablas.PERSONA, Schema = Esquemas.TERCEROS)]
    public class PersonaDtm : ElementoDtm, IUsaTraza, IUsaBaja, IEsUnTercero, ITieneReferencia, IUsaDirecciones
    {
        public string Apellidos { get; set; }
        public bool EsNie { get; set; }
        public string NIF { get; set; }
        public string eMail { get; set; }
        public string Telefono { get; set; }
        public bool Baja { get; set; }
        public override string Expresion => $"({NIF}) {Apellidos}, {base.Expresion}";
        public bool EsInterlocutor { get; set; }

        public string NIFConIsoEs {
            get
            {
                if (ApiDeTerceros.ClaseDeNacionalidad(NIF) != enumClaseDeNacionalidad.Nacional)
                    return NIF.ToUpper().Trim();

                return !NIF.StartsWith(ltrIsoPaises.Spain) ? (ltrIsoPaises.Spain + NIF).ToUpper() : NIF.ToUpper();
            }
        }

        public string NIFSinIsoEs {
            get
            {
                if (ApiDeTerceros.ClaseDeNacionalidad(NIF) != enumClaseDeNacionalidad.Nacional)
                    return NIF.ToUpper().Trim();

                return NIF.StartsWith(ltrIsoPaises.Spain) ? NIF.Substring(ltrIsoPaises.Spain.Length).ToUpper() : NIF.ToUpper();
            }
        }

        public string Referencia => NIF.ToUpper();
    }

    [Table(Tablas.PERSONA + "_" + nameof(Sufijo.AUDITORIA), Schema = Esquemas.TERCEROS)]
    public class AuditoriaDeUnaPersonaDtm : AuditoriaDtm
    {
    }


    [Table(Tablas.PERSONA + "_" + nameof(Sufijo.ARCHIVO), Schema = Esquemas.TERCEROS)]
    public class ArchivosDeUnaPersonaDtm : VinculoDtm
    {
        public PersonaDtm Persona { get; set; }
        public ArchivoDtm Archivo { get; set; }
    }

    [Table(Tablas.PERSONA + "_" + nameof(Sufijo.OBSERVACION), Schema = Esquemas.TERCEROS)]
    public class ObservacionesDeUnaPersonaDtm : ObservacionDtm
    {
        public override enumNegocio Negocio => enumNegocio.Persona;
    }

    [Table(Tablas.PERSONA + "_" + nameof(Sufijo.TRAZA), Schema = Esquemas.TERCEROS)]
    public class TrazasDeUnaPersonaDtm : TrazaDtm
    {
        public override enumNegocio Negocio => enumNegocio.Persona;
    }

    [Table(Tablas.PERSONA + "_" + nameof(Sufijo.ARCHIVADOR), Schema = Esquemas.TERCEROS)]
    public class ArchivadoresDeUnaPersonaDtm : VinculoDtm
    {
        public PersonaDtm Persona { get; set; }
        public ArchivadorDtm Archivador { get; set; }
    }

    [Table(Tablas.PERSONA + "_" + nameof(Sufijo.DIRECCION), Schema = Esquemas.TERCEROS)]
    public class DireccionDeUnaPersonaDtm : DireccionDtm
    {
        public override enumNegocio Negocio => enumNegocio.Persona;
    }

    public static partial class ModeloDeTerceros
    {
        public static void Persona(ModelBuilder modelBuilder)
        {
            ApiDeElementoDtm.DefinirCamposDelElementoDtm<PersonaDtm>(modelBuilder);

            modelBuilder.Entity<PersonaDtm>().Property(p => p.Apellidos).HasColumnName(ICampos.APELLIDO).HasColumnType(IDominio.VARCHAR_255).IsRequired();
            modelBuilder.Entity<PersonaDtm>().Property(p => p.EsNie).HasColumnName(ICampos.ES_NIE).HasColumnType(IDominio.BIT).IsRequired();
            modelBuilder.Entity<PersonaDtm>().Property(p => p.NIF).HasColumnName(ICampos.NIF).HasColumnType(IDominio.VARCHAR_15).IsRequired();
            ApiDeInterlocutorDtm.DefinirCamposUsaInterlocutor<PersonaDtm>(modelBuilder);

            modelBuilder.Entity<PersonaDtm>().HasIndex(x => new { x.NIF }).IsUnique(true).HasDatabaseName($"I_{Tablas.PERSONA}_{ICampos.NIF}");
        }

        public static void AuditoriaDeUnaPersona(ModelBuilder modelBuilder)
        {
            ApiDeAuditoria.DefinirCamposDeAuditoriaDtm<AuditoriaDeUnaPersonaDtm>(modelBuilder);
        }

        internal static void ArchivosDeUnaPersona(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<ArchivosDeUnaPersonaDtm>(modelBuilder, nameof(ArchivosDeUnaPersonaDtm.Persona), nameof(ArchivosDeUnaPersonaDtm.Archivo));
        }

        internal static void ObservacionesDeUnaPersona(ModelBuilder modelBuilder)
        {
            ApiDeObservaciones.DefinirCampos<ObservacionesDeUnaPersonaDtm, PersonaDtm>(modelBuilder);
        }

        internal static void DireccionesDeUnaPersona(ModelBuilder modelBuilder)
        {
            ApiDireccionDtm.DefinirCampos<DireccionDeUnaPersonaDtm, PersonaDtm>(modelBuilder);
        }

        internal static void ArchivadoresDeUnaPersona(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<ArchivadoresDeUnaPersonaDtm>(modelBuilder, nameof(ArchivadoresDeUnaPersonaDtm.Persona), nameof(ArchivadoresDeUnaPersonaDtm.Archivador));
        }

        internal static void TrazasDeUnaPersona(ModelBuilder modelBuilder)
        {
            ApiTraza.DefinirCampos<TrazasDeUnaPersonaDtm, PersonaDtm>(modelBuilder);
        }

    }
}
