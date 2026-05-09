using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Serialization;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using Utilidades;

namespace ServicioDeDatos.Terceros
{

    public enum enumRolCentroAdministrativo
    {
        [XmlEnum("01")]
        [Description("Órgano Gestor")]
        OrganoGestor = 1,
        [XmlEnum("02")]
        [Description("Unidad Tramitadora")]
        UnidadTramitadora = 2,
        [XmlEnum("03")]
        [Description("Oficina contable")]
        OficinaContable = 3,
        [XmlEnum("04")]
        [Description("Otro")]
        Otro = 4,
        [XmlEnum("05")]
        [Description("No Especificado")]
        NoEspecificado = 5
    }

    [Table(Tablas.CLIENTE + "_" + nameof(Sufijo.CENTRO_ADMINISTRATIVO), Schema = Esquemas.TERCEROS)]
    public class CentroAdministrativoDtm : RegistroDtm, IDetalle
    {
        public int IdElemento { get; set; }
        public ClienteDtm Elemento { get; set; }
        IElementoDtm IDetalle.Elemento => Elemento;
        public string CodigoDir3 { get; set; }

        public string OrganoGestor { get; set; }
        public string UnidadTramitadora { get; set; }
        public string OficinaContable { get; set; }

        public string Alias { get; set; }

        public int IdContacto { get; set; }
        public InterlocutorDtm Contacto { get; set; }

        public enumRolCentroAdministrativo Rol => UnidadTramitadora.IsNullOrEmpty()
        ? enumRolCentroAdministrativo.OrganoGestor
        : OficinaContable.IsNullOrEmpty()
        ? enumRolCentroAdministrativo.UnidadTramitadora
        : enumRolCentroAdministrativo.OficinaContable;

        public bool Activa { get; set; }

        public enumNegocio Negocio => enumNegocio.Cliente;

        public string Expresion => Rol == enumRolCentroAdministrativo.OrganoGestor 
            ? $"{OrganoGestor}"
            : Rol == enumRolCentroAdministrativo.UnidadTramitadora 
            ? $"{OrganoGestor} - {UnidadTramitadora}"
            : Rol == enumRolCentroAdministrativo.OficinaContable
            ? $"{OrganoGestor} - {UnidadTramitadora} - {OficinaContable}"
            : OrganoGestor;
    }

    public static partial class ModeloDeTerceros
    {
        public static void CentroAdministrativo(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<CentroAdministrativoDtm>().Ignore(x => x.Negocio);
            modelBuilder.Entity<CentroAdministrativoDtm>().Ignore(x => x.Elemento);

            modelBuilder.Entity<CentroAdministrativoDtm>().Property(nameof(CentroAdministrativoDtm.IdElemento)).HasColumnName(ICampos.ID_ELEMENTO).HasColumnType(IDominio.INT).IsRequired(true);
            ApiDeRegistroDtm.DefinirFk<CentroAdministrativoDtm, ClienteDtm>(modelBuilder, nameof(CentroAdministrativoDtm.IdElemento), ICampos.ID_ELEMENTO, unico: false);
            modelBuilder.Entity<CentroAdministrativoDtm>().Property(p => p.CodigoDir3).HasColumnName(ICampos.CODIGO).HasColumnType(IDominio.VARCHAR_10).IsRequired();
            modelBuilder.Entity<CentroAdministrativoDtm>().Property(p => p.OrganoGestor).HasColumnName(ICampos.ORGANO_GESTOR).HasColumnType(IDominio.VARCHAR_250).IsRequired();
            modelBuilder.Entity<CentroAdministrativoDtm>().Property(p => p.UnidadTramitadora).HasColumnName(ICampos.UNIDAD_TRAMITADORA).HasColumnType(IDominio.VARCHAR_250).IsRequired(false);
            modelBuilder.Entity<CentroAdministrativoDtm>().Property(p => p.OficinaContable).HasColumnName(ICampos.OFICINA_CONTABLE).HasColumnType(IDominio.VARCHAR_250).IsRequired(false);
            modelBuilder.Entity<CentroAdministrativoDtm>().Property(p => p.Alias).HasColumnName(ICampos.ALIAS).HasColumnType(IDominio.VARCHAR_250).IsRequired(false);

            ApiDeRegistroDtm.DefinirDependencia<CentroAdministrativoDtm>(modelBuilder, nameof(CentroAdministrativoDtm.Contacto), nameof(CentroAdministrativoDtm.IdContacto), ICampos.ID_INTERLOCUTOR);

            modelBuilder.Entity<CentroAdministrativoDtm>().Property(x=> x.Activa).HasColumnName(ICampos.ACTIVA).HasColumnType(IDominio.BIT).IsRequired(true).HasDefaultValue(true);
        }
    }

}

