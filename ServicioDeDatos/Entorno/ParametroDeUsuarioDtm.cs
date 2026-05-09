using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;

namespace ServicioDeDatos.Entorno
{

    public class EstadoDeEspanDeUnaVista
    {
        public string IdDelEspan { get; set; }
        public bool Abierto { get; set; }
    }

    public class VisibilidadDeColumnaDeUnaVista
    {
        public string Id { get; set; }
        public string Propiedad { get; set; }
        public bool Visible { get; set; }
    }

    public class DisposicionDeColumnaDeUnaVista
    {
        public string Id { get; set; }
        public string Anterior { get; set; }
        public string Propiedad { get; set; }
        public string Posterior { get; set; }
        public int Posicion { get; set; }
    }

    public class TamanoDeColumnaDeUnaVista
    {
        public string Id { get; set; }
        public string Tamano { get; set; }
    }

    [Table(Tablas.PARAMETRO_VISTA_USUARIO, Schema = Esquemas.ENTORNO)]
    public class ParametroVistaPorUsuarioDtm : RegistroConNombreDtm, IRegistroDeParametrizacion
    {
        public string Valor { get; set; }
        public int IdVista { get; set; }
        public VistaMvcDtm Vista { get; set; }
        public int IdUsuario { get; set; }
        public UsuarioDtm Usuario { get; set; }

    }

    public static class ltrParametrosDeVistaPorUsuario
    {
        public static readonly string columnasJson = nameof(columnasJson);
        public static readonly string encolumnado = nameof(encolumnado);
        public static readonly string tamanos = nameof(tamanos);
        public static readonly string cantidadPorLeer = nameof(cantidadPorLeer);
    }

    public static partial class ModeloDeEntorno
    {
        public static void ParametrosVistaPorUsuario(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ParametroVistaPorUsuarioDtm>().Property(p => p.Nombre).HasColumnName(ICampos.NOMBRE).HasColumnType(IDominio.VARCHAR_250).IsRequired();
            modelBuilder.Entity<ParametroVistaPorUsuarioDtm>().Property(p => p.Valor).HasColumnName(ICampos.VALOR).HasColumnType(IDominio.VARCHAR_MAX).IsRequired(true);

            //ApiDeRegistroDtm.DefinirCampoFk<ParametrosVistaPorUsuario>(modelBuilder, nameof(ParametrosVistaPorUsuario.Negocio), nameof(ParametrosVistaPorUsuario.IdNegocio), ICampos.ID_NEGOCIO, requerida: true, unico: false);
            ApiDeRegistroDtm.DefinirCampoFk<ParametroVistaPorUsuarioDtm>(modelBuilder, nameof(ParametroVistaPorUsuarioDtm.Usuario), nameof(ParametroVistaPorUsuarioDtm.IdUsuario), ICampos.ID_USUARIO, requerida: true, unico: false);

            ApiDeRegistroDtm.DefinirCampoFk<ParametroVistaPorUsuarioDtm>(modelBuilder, nameof(ParametroVistaPorUsuarioDtm.Vista), nameof(ParametroVistaPorUsuarioDtm.IdVista), ICampos.IDVISTA_MVC, requerida: true, unico: false);

            modelBuilder.Entity<ParametroVistaPorUsuarioDtm>()
               .HasIndex(p => new { p.IdVista, p.IdUsuario, p.Nombre })
               .IsUnique(true)
               .HasDatabaseName($"I_{Tablas.PARAMETRO_VISTA_USUARIO}_{ICampos.IDVISTA_MVC}_{ICampos.ID_USUARIO}_{ICampos.NOMBRE}");

        }
    }

}
