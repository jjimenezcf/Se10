using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;

namespace ServicioDeDatos.Negocio
{
    public enum enumParametrosDeUsuario
    {
        [Description("Información de algunos valores por defecto del usuario")]
        USU_Valores_Por_Defecto,
        [Description("Información de la disposición de paneles de la vista de edición asociada al negocio")]
        USU_Vista_De_Edicion,
        [Description("Información de que columnas del grid se han de mostrar")]
        USU_Colunas_Del_Grid,
        [Description("Información como están dispuestas las columnas")]
        USU_Disposicion_Del_Encolumnado,
        [Description("Información como se ordena el resultado")]
        USU_Ordenacion_Del_Resultado,
        [Description("Información del tamaño de las columnas")]
        USU_Tamano_Del_Encolumnado,
        [Description("Información de la cantidad de registros por leer")]
        USU_Cantidad_A_Leer,
        [Description("Último tamaño del visor de archivos")]
        USU_Tamano_Del_Visor,
        [Description("Información sobre la disposición de los archivos")]
        USU_Disposicion_Archivos,
        [Description("Indica si ha de mostrar el visor de archivo al iniciar la edición")]
        USU_Mostrar_El_Visor_Al_Iniciar,
        [Description("Indica la Ia usada por el usuario")]
        USU_Ia_Usada
    }

    public class EstadoDeEspan
    {
        public string IdDelEspan { get; set; }
        public bool Abierto { get; set; }
    }

    public class VisibilidadDeColumna
    {
        public string Id { get; set; }
        public string Propiedad { get; set; }
        public bool Visible { get; set; }
    }

    public class DisposicionDeColumna
    {
        public string Id { get; set; }
        public string Anterior { get; set; }
        public string Propiedad { get; set; }
        public string Posterior { get; set; }
        public int Posicion { get; set; }
    }

    public class OrdenDeColumna
    {
        public string Id { get; set; }
        public string Propiedad { get; set; }
        public string OrdenadoPor { get; set; }
        public string Modo { get; set; }
    }

    public class TamanoDeColumna
    {
        public string Id { get; set; }
        public string Tamano { get; set; }
    }

    [Table(Tablas.PARAMETRO_USUARIO, Schema = Esquemas.NEGOCIO)]
    public class ParametroDeUsuarioDtm : RegistroConNombreDtm, IRegistroDeParametrizacion, ITieneCampoNegocio
    {
        public string Valor { get; set; }
        public int IdNegocio { get; set; }
        public NegocioDtm Negocio { get; set; }
        public int IdUsuario { get; set; }
        public UsuarioDtm Usuario { get; set; }

    }

    public static class ltrParametrosDeUsuarios
    {
        public static readonly string columnasJson = nameof(columnasJson);
        public static readonly string encolumnado = nameof(encolumnado);
        public static readonly string ordenacion = nameof(ordenacion);
        public static readonly string tamanos = nameof(tamanos);
        public static readonly string cantidadPorLeer = nameof(cantidadPorLeer);
        public static readonly string tamanoDelVisor = nameof(tamanoDelVisor);
    }

    public static partial class ModeloDeNegocio
    {
        public static void ParametrosDeUsuario(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ParametroDeUsuarioDtm>().Property(p => p.Nombre).HasColumnName(ICampos.NOMBRE).HasColumnType(IDominio.VARCHAR_250).IsRequired();
            modelBuilder.Entity<ParametroDeUsuarioDtm>().Property(p => p.Valor).HasColumnName(ICampos.VALOR).HasColumnType(IDominio.VARCHAR_MAX).IsRequired(true);

            //ApiDeRegistroDtm.DefinirCampoFk<ParametroDeUsuarioDtm>(modelBuilder, nameof(ParametroDeUsuarioDtm.Negocio), nameof(ParametroDeUsuarioDtm.IdNegocio), ICampos.ID_NEGOCIO, requerida: true, unico: false);
            ApiDeRegistroDtm.DefinirCampoFk<ParametroDeUsuarioDtm>(modelBuilder, nameof(ParametroDeUsuarioDtm.Usuario), nameof(ParametroDeUsuarioDtm.IdUsuario), ICampos.ID_USUARIO, requerida: true, unico: false);

            DefinirCampoNegocio<ParametroDeUsuarioDtm>(modelBuilder, unico: false);

            modelBuilder.Entity<ParametroDeUsuarioDtm>()
               .HasIndex(p => new { p.IdNegocio, p.IdUsuario, p.Nombre })
               .IsUnique(true)
               .HasDatabaseName($"I_{Tablas.PARAMETRO_USUARIO}_{ICampos.ID_NEGOCIO}_{ICampos.ID_USUARIO}_{ICampos.NOMBRE}");

        }
    }

}
