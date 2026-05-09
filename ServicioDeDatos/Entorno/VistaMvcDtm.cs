using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Seguridad;

namespace ServicioDeDatos.Entorno
{
    [Table(Tablas.VISTA_MVC, Schema = Esquemas.ENTORNO)]
    public class VistaMvcDtm : RegistroConNombreDtm
    {

        public string Controlador { get; set; }
        public string Accion { get; set; }

        public string Parametros { get; set; }

        public bool MostrarEnModal { get; set; }

        public List<MenuDtm> Menus { get; set; }

        public int IdPermiso { get; set; }
        
        public PermisoDtm Permiso { get; set; }

        public string ElementoDto { get; set; }

    }
    public static class VistaMvcSqls
    {
        public static readonly string LeerVistaPorDto = $@"
            SELECT {ICampos.ID}           as {nameof(VistaMvcDtm.Id)}
                  ,{ICampos.NOMBRE}       as {nameof(VistaMvcDtm.Nombre)}
                  ,{ICampos.CONTROLADOR}  as {nameof(VistaMvcDtm.Controlador)}
                  ,{ICampos.ACCION}       as {nameof(VistaMvcDtm.Accion)}
                  ,{ICampos.PARAMETROS}   as {nameof(VistaMvcDtm.Parametros)}
                  ,{ICampos.MODAL}        as {nameof(VistaMvcDtm.MostrarEnModal)}
                  ,{ICampos.IDPERMISO}    as {nameof(VistaMvcDtm.IdPermiso)}
                  ,{ICampos.ELEMENTO_DTO} as {nameof(VistaMvcDtm.ElementoDto)}
            FROM  {Esquemas.ENTORNO}.{Tablas.VISTA_MVC}
            WHERE {ICampos.ELEMENTO_DTO} = @{ICampos.ELEMENTO_DTO}
            "; 
        
        public static readonly string LeerVistaPorVista = $@"{LeerVistaPorDto} AND {ICampos.ACCION} = @{ICampos.VISTA}";
    }
    public static partial class ModeloDeEntorno
    {
        internal static string TablaVistasMvc => $"{ApiDeRegistroDtm.EsquemaDeTabla(typeof(VistaMvcDtm))}.{ApiDeRegistroDtm.NombreDeTabla(typeof(VistaMvcDtm))}";

        public static void VistaMvc(ModelBuilder modelBuilder)
        {
            ApiDeNombreDtm.DefinirCampoNombreDtm<VistaMvcDtm>(modelBuilder);
            modelBuilder.Entity<VistaMvcDtm>().Property(p => p.Parametros).HasColumnName(ICampos.PARAMETROS).HasColumnType(IDominio.VARCHAR_250).IsRequired(false);
            modelBuilder.Entity<VistaMvcDtm>().Property(p => p.ElementoDto).HasColumnName(ICampos.ELEMENTO_DTO).HasColumnType(IDominio.VARCHAR_250).IsRequired(false);
            modelBuilder.Entity<VistaMvcDtm>().Property(p => p.MostrarEnModal).HasColumnName(ICampos.MODAL).HasColumnType(IDominio.BIT).IsRequired();
            modelBuilder.Entity<VistaMvcDtm>().Property(p => p.Controlador).HasColumnName(ICampos.CONTROLADOR).HasColumnType(IDominio.VARCHAR_250).IsRequired();
            modelBuilder.Entity<VistaMvcDtm>().Property(p => p.Accion).HasColumnName(ICampos.ACCION).HasColumnType(IDominio.VARCHAR_250).IsRequired();

            ApiDeRegistroDtm.DefinirCampoFk<VistaMvcDtm>(modelBuilder, nameof(VistaMvcDtm.Permiso), nameof(VistaMvcDtm.IdPermiso), ICampos.IDPERMISO,true,true);

            modelBuilder.Entity<VistaMvcDtm>()
               .HasIndex(vista => new { vista.Controlador, vista.Accion, vista.Parametros })
               .IsUnique(true)
               .HasDatabaseName($"I_{Tablas.VISTA_MVC}_{ICampos.CONTROLADOR}_{ICampos.ACCION}");

            modelBuilder.Entity<VistaMvcDtm>()
                .HasMany(vista => vista.Menus)
                .WithOne(vista => vista.VistaMvc)
                .HasForeignKey(menu => menu.IdVistaMvc);
        }
    }


}
