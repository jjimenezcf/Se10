
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;

namespace ServicioDeDatos.Terceros
{
    [Table(Tablas.SOCIEDAD_CONTACTO, Schema = Esquemas.TERCEROS)]
    public class ContactoDtm : RegistroConNombreDtm, IUsaDescripcion, IUsaBaja, IEsUnTercero
    {
        public int IdSociedad { get; set; }
        public string eMail { get; set; }
        public string Telefono { get; set; }
        public bool Baja { get; set; }
        public string Descripcion { get; set; }
        public SociedadDtm Sociedad { get; set; }
        public override string Expresion => Sociedad == null ? Nombre :  $"({Sociedad.NIF}) {Sociedad.Nombre}: {Nombre}";
        public bool EsInterlocutor { get; set; }
    }

    public static partial class ModeloDeTerceros
    {
        public static void ContactosDeUnaSociedad(ModelBuilder modelBuilder)
        {

            ApiDeRegistroDtm.DefinirCampoIdDtm<ContactoDtm>(modelBuilder);
            ApiDeNombreDtm.DefinirCampoNombreDtm<ContactoDtm>(modelBuilder, conIndice: false);

            modelBuilder.Entity<ContactoDtm>().Property(p => p.IdSociedad).HasColumnName(ICampos.ID_SOCIEDAD).HasColumnType(IDominio.INT).IsRequired();
            ApiDeNombreDtm.DefinirCampoDescripcion<ContactoDtm>(modelBuilder);
            ApiDeRegistroDtm.DefinirCampoBaja<ContactoDtm>(modelBuilder);
            ApiDeInterlocutorDtm.DefinirCamposUsaInterlocutor<ContactoDtm>(modelBuilder);

            modelBuilder.Entity<ContactoDtm>().HasIndex(p => new { p.IdSociedad, p.Nombre }).HasDatabaseName($"I_{ApiDeRegistroDtm.NombreDeTabla(typeof(ContactoDtm))}_{ICampos.ID_SOCIEDAD}_{ICampos.NOMBRE}").IsUnique();
            ApiDeRegistroDtm.DefinirFk<ContactoDtm>(modelBuilder, nameof(ContactoDtm.Sociedad), nameof(ContactoDtm.IdSociedad), ICampos.ID_SOCIEDAD, unico: false);

        }


    }
}
