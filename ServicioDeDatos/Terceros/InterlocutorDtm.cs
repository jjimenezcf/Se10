using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.SistemaDocumental;
using Utilidades;

namespace ServicioDeDatos.Terceros
{

    public enum enumParametrosDeInterlocutor
    {
        [Description("Valor bolleano que indica si se usan terceros judiciales")]
        INT_TercerosJudiciales
    }

    [Table(Tablas.INTERLOCUTOR, Schema = Esquemas.TERCEROS)]
    public class InterlocutorDtm : ElementoDtm, IUsaTraza, IUsaBaja, IDatosDeContacto, IUsaDirecciones
    {
        public string eMail { get; set; }
        public string Telefono { get; set; }
        public bool Baja { get; set; }
        public int? IdPersona { get; set; }
        public int? IdSociedad { get; set; }
        public int? IdContacto { get; set; }
        public PersonaDtm Persona { get; set; }
        public ContactoDtm Contacto { get; set; }
        public SociedadDtm Sociedad { get; set; }
        public bool EsPersona => IdPersona.Entero() > 0;
        public bool EsSociedad => IdSociedad.Entero() > 0 && IdContacto.Entero() == 0;
        public bool EsContacto => IdContacto.Entero() > 0;
    }

    [Table(Tablas.INTERLOCUTOR + "_" + nameof(Sufijo.AUDITORIA), Schema = Esquemas.TERCEROS)]
    public class AuditoriaDeUnInterlocutorDtm : AuditoriaDtm
    {
    }


    [Table(Tablas.INTERLOCUTOR + "_" + nameof(Sufijo.ARCHIVO), Schema = Esquemas.TERCEROS)]
    public class ArchivosDeUnInterlocutorDtm : VinculoDtm
    {
        public InterlocutorDtm Interlocutor { get; set; }
        public ArchivoDtm Archivo { get; set; }
    }

    [Table(Tablas.INTERLOCUTOR + "_" + nameof(Sufijo.OBSERVACION), Schema = Esquemas.TERCEROS)]
    public class ObservacionesDeUnInterlocutorDtm : ObservacionDtm
    {
        public override enumNegocio Negocio => enumNegocio.Interlocutor;
    }

    [Table(Tablas.INTERLOCUTOR + "_" + nameof(Sufijo.TRAZA), Schema = Esquemas.TERCEROS)]
    public class TrazasDeUnInterlocutorDtm : TrazaDtm
    {
        public override enumNegocio Negocio => enumNegocio.Interlocutor;
    }


    [Table(Tablas.INTERLOCUTOR + "_" + nameof(Sufijo.DIRECCION), Schema = Esquemas.TERCEROS)]
    public class DireccionDeInterlocutorDtm : DireccionDtm
    {
        public override enumNegocio Negocio => enumNegocio.Interlocutor;
    }

    public static partial class ModeloDeTerceros
    {
        public static void Interlocutor(ModelBuilder modelBuilder)
        {
            ApiDeElementoDtm.DefinirCamposDelElementoDtm<InterlocutorDtm>(modelBuilder, indiceUnicoPorNombre: true);
            ApiDeRegistroDtm.DefinirDatosDeContacto<InterlocutorDtm>(modelBuilder);

            modelBuilder.Entity<InterlocutorDtm>().Property(x => x.IdPersona).HasColumnName(ICampos.ID_PERSONA).HasColumnType(IDominio.INT).IsRequired(false);
            ApiDeRegistroDtm.DefinirCampoFk<InterlocutorDtm>(modelBuilder
                , nameof(InterlocutorDtm.Persona)
                , nameof(InterlocutorDtm.IdPersona)
                , nameof(ICampos.ID_PERSONA)
                , false
                , true);

            modelBuilder.Entity<InterlocutorDtm>().Property(x => x.IdSociedad).HasColumnName(ICampos.ID_SOCIEDAD).HasColumnType(IDominio.INT).IsRequired(false);
            ApiDeRegistroDtm.DefinirCampoFk<InterlocutorDtm>(modelBuilder
                , nameof(InterlocutorDtm.Sociedad)
                , nameof(InterlocutorDtm.IdSociedad)
                , nameof(ICampos.ID_SOCIEDAD)
                , false
                , unico: false);

            modelBuilder.Entity<InterlocutorDtm>().Property(x => x.IdContacto).HasColumnName(ICampos.ID_CONTACTO).HasColumnType(IDominio.INT).IsRequired(false);
            ApiDeRegistroDtm.DefinirCampoFk<InterlocutorDtm>(modelBuilder
                , nameof(InterlocutorDtm.Contacto)
                , nameof(InterlocutorDtm.IdContacto)
                , nameof(ICampos.ID_CONTACTO)
                , false
                , true);

            modelBuilder.Entity<InterlocutorDtm>()
                        .HasIndex(x => new {x.IdPersona, x.IdSociedad, x.IdContacto })
                        .IsUnique(true)
                        .HasDatabaseName($"AK_{Tablas.INTERLOCUTOR}");
        }

        public static void InterlocutorAudt(ModelBuilder modelBuilder)
        {
            ApiDeAuditoria.DefinirCamposDeAuditoriaDtm<AuditoriaDeUnInterlocutorDtm>(modelBuilder);
        }

        internal static void ArchivosDeUnInterlocutor(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<ArchivosDeUnInterlocutorDtm>(modelBuilder, nameof(ArchivosDeUnInterlocutorDtm.Interlocutor), nameof(ArchivosDeUnInterlocutorDtm.Archivo));
        }

        internal static void ObservacionesDeUnInterlocutor(ModelBuilder modelBuilder)
        {
            ApiDeObservaciones.DefinirCampos<ObservacionesDeUnInterlocutorDtm, InterlocutorDtm>(modelBuilder);
        }

        internal static void DireccionesDeUnInterlocutor(ModelBuilder modelBuilder)
        {
            ApiDireccionDtm.DefinirCampos<DireccionDeInterlocutorDtm, InterlocutorDtm>(modelBuilder);
        }
        internal static void TrazasDeUnInterlocutor(ModelBuilder modelBuilder)
        {
            ApiTraza.DefinirCampos<TrazasDeUnInterlocutorDtm, InterlocutorDtm>(modelBuilder);
        }


    }

    public static class ApiDeInterlocutorDtm
    {
        internal static void DefinirCamposUsaInterlocutor<TEntity>(ModelBuilder modelBuilder) where TEntity : RegistroConNombreDtm
        {
            ApiDeRegistroDtm.DefinirDatosDeContacto<TEntity>(modelBuilder);

            modelBuilder.Entity<TEntity>().Property(p => ((IEsUnTercero)p).EsInterlocutor)
            .HasColumnName(ICampos.ES_INTERLOCUTOR)
            .HasColumnType(IDominio.BIT)
            .HasComputedColumnSql($@"{Esquemas.TERCEROS}.CC_{ApiDeRegistroDtm.NombreDeTabla(typeof(TEntity))}_{ICampos.ES_INTERLOCUTOR}({ICampos.ID})");
        }
    }
}
/*
 * 
            migrationBuilder.Sql($@"CREATE FUNCTION [TERCEROS].[CC_PERSONA_ES_INTERLOCUTOR] (@ID int)
                RETURNS BIT
                AS
                begin
                  declare @resultado BIT

                  set @resultado = (select count(*)
                  from TERCEROS.INTERLOCUTOR 
                  where ID_PERSONA = @ID)

                  return @resultado
                END
                GO");
            
               migrationBuilder.Sql($@"CREATE FUNCTION [TERCEROS].[CC_SOCIEDAD_ES_INTERLOCUTOR] (@ID int)
                RETURNS BIT
                AS
                begin
                  declare @resultado BIT

                  set @resultado = (select count(*)
                  from TERCEROS.INTERLOCUTOR 
                  where ID_SOCIEDAD = @ID and ID_CONTACTO IS NULL)

                  return @resultado
                END
                GO");           

                migrationBuilder.Sql($@"CREATE FUNCTION [TERCEROS].[CC_SOCIEDAD_CONTACTO_ES_INTERLOCUTOR] (@ID int)
                RETURNS BIT
                AS
                begin
                  declare @resultado BIT

                  set @resultado = (select count(*)
                  from TERCEROS.INTERLOCUTOR 
                  where ID_CONTACTO = @ID)

                  return @resultado
                END
                GO");

 */
