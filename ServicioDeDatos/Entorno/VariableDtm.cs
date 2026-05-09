using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;

namespace ServicioDeDatos.Entorno
{

    public class ltrVariables
    {
        public static string ValorInicialDeAplicarTransicion = "[{\"estado\": 0,\"transicion\": 0},{\"estado\": 0,\"transicion\": 0 }]";
        public static string FiltroPorVariable = "porvariable";
    }

    [Table(Tablas.VARIABLE, Schema = Esquemas.ENTORNO)]
    public class VariableDtm : RegistroConNombreDtm
    {
        public string Descripcion { get; set; }

        public string Valor { get; set; }
    }

    public static class TablaVariable
    {
        public static void Definir(ModelBuilder modelBuilder)
        {
            ApiDeNombreDtm.DefinirCampoNombreDtm<VariableDtm>(modelBuilder, 50, unico:true);
            //modelBuilder.Entity<VariableDtm>().Property(p => p.Nombre).HasColumnName(ICampos.NOMBRE).HasColumnType(IDominio.VARCHAR_50).IsRequired();
            modelBuilder.Entity<VariableDtm>().Property(v => v.Descripcion).HasColumnName(ICampos.DESCRIPCION).HasColumnType(IDominio.VARCHAR_MAX).IsRequired(false);
            modelBuilder.Entity<VariableDtm>().Property(v => v.Valor).HasColumnName(ICampos.VALOR).HasColumnType(IDominio.VARCHAR_2000).IsRequired();

            //modelBuilder.Entity<VariableDtm>().HasIndex(v => new { v.Nombre }).IsUnique(true).HasDatabaseName("IND_VARIABLE_NOMBRE");

        }
    }


    public static class VariableSqls
    {
        public static string CrearVariable ="INSERT INTO ENTORNO.VARIABLE (NOMBRE, DESCRIPCION, VALOR) VALUES(@variable, @descripcion, @valor)"; 
        
        public static string ModificarVariable = "UPDATE ENTORNO.VARIABLE SET VALOR = @valor WHERE NOMBRE like @variable";

        public static string LeerValorDeVariable ="SELECT ID, NOMBRE, DESCRIPCION, VALOR FROM ENTORNO.VARIABLE with(nolock) WHERE NOMBRE like @variable";

        public static string BorrarVariable = "DELETE FROM ENTORNO.VARIABLE WHERE NOMBRE like @variable";

    }
}
