using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServicioDeDatos.Entorno
{

    public enum enumClaseDeAcceso
    {
        [Description("Edicion de registro")]
        Registros,
        [Description("Acceso a Menú")]
        Menu
    }


    [Table(Tablas.ACCESO_RECIENTE, Schema = Esquemas.ENTORNO)]
    public class AccesoRecienteDtm : RegistroConNombreDtm
    {
        public int IdUsuario { get; set; }
        public enumClaseDeAcceso ClaseDeAcceso { get; set; }
        public int? IdMenu { get; set; }
        public int IdVista { get; set; }
        public string OpcionHtml { get; set; }
        public string UrlAccedida { get; set; }
        public DateTime AccedioEl { get; set; }

        public MenuDtm Menu { get; set; }
        public VistaMvcDtm Vista { get; set; }
        public UsuarioDtm Usuario { get; set; }
    }

    public static partial class ModeloDeEntorno
    {
        public static void AccesosRecientes(ModelBuilder modelBuilder)
        {
            ApiDeNombreDtm.DefinirCampoNombreDtm<AccesoRecienteDtm>(modelBuilder);

            modelBuilder.Entity<AccesoRecienteDtm>().Property(p => p.ClaseDeAcceso).HasColumnName(ICampos.CLASE_DE_ACCESO).HasColumnType(IDominio.VARCHAR_10).IsRequired();

            ApiDeRegistroDtm.DefinirCampoFk<AccesoRecienteDtm>(modelBuilder, nameof(AccesoRecienteDtm.Usuario), nameof(AccesoRecienteDtm.IdUsuario), ICampos.ID_USUARIO, requerida: true, unico: false);
            ApiDeRegistroDtm.DefinirCampoFk<AccesoRecienteDtm>(modelBuilder, nameof(AccesoRecienteDtm.Vista), nameof(AccesoRecienteDtm.IdVista), ICampos.IDVISTA_MVC, requerida: true, unico: false);
            ApiDeRegistroDtm.DefinirCampoFk<AccesoRecienteDtm>(modelBuilder, nameof(AccesoRecienteDtm.Menu), nameof(AccesoRecienteDtm.IdMenu), ICampos.ID_MENU, requerida: false, unico: false);

            modelBuilder.Entity<AccesoRecienteDtm>().Property(p => p.AccedioEl).HasColumnName(ICampos.ACCEDIDO_EL).HasColumnType(IDominio.DATETIME_2).IsRequired(true);
            modelBuilder.Entity<AccesoRecienteDtm>().Property(p => p.OpcionHtml).HasColumnName(ICampos.OPCION_HTML).HasColumnType(IDominio.VARCHAR_MAX).HasMaxLength(IDominio.Longitud(IDominio.VARCHAR_MAX)).IsRequired(true);
            modelBuilder.Entity<AccesoRecienteDtm>().Property(p => p.UrlAccedida).HasColumnName(ICampos.URL).HasColumnType(IDominio.URL).IsRequired(true);

            modelBuilder.Entity<AccesoRecienteDtm>()
               .HasIndex(p => new { p.IdUsuario, p.IdVista })
               .IsUnique(false)
               .HasDatabaseName($"I_{Tablas.ACCESO_RECIENTE}_{ICampos.ID_USUARIO}_{ICampos.IDVISTA_MVC}");

            //modelBuilder.Entity<AccesoRecienteDtm>()
            //   .HasIndex(p => new { p.IdUsuario, p.ClaseDeAcceso, p.IdVista, p.UrlAccedida })
            //   .IsUnique(true)
            //   .HasDatabaseName($"I_AK_{Tablas.ACCESO_RECIENTE}");

        }
    }


}
