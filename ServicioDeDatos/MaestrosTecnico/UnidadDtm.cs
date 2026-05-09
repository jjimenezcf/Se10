using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServicioDeDatos.MaestrosTecnico
{

    public class ltrSiglasDeUnidades
    {
        public const string Unidad = "Ud";
        public const string Hora = "H";
    }

    public static class VariablesDeUnidadDeMedida
    {
        public static string Horas => CacheDeVariable.ObtenerVariable(Variable.UND_Hora, Descripciones.UND_Hora, ltrSiglasDeUnidades.Hora);
    }



    [Table(Tablas.MT_UNIDAD, Schema = Esquemas.MT)]
    public class UnidadDtm : RegistroConNombreDtm
    {
        public string Sigla { get; set; }
        public override string Expresion => $"({Sigla}) {Nombre}";
    }

    public static partial class ModeloDeMt
    {
        public static void UnidadesDeMedida(ModelBuilder modelBuilder)
        {
            ApiDeRegistroDtm.DefinirCampoIdDtm<UnidadDtm>(modelBuilder);
            ApiDeNombreDtm.DefinirCampoNombreDtm<UnidadDtm>(modelBuilder, unico: true);
            ApiDeNombreDtm.DefinirCampoDeSigla<UnidadDtm>(modelBuilder);       
        }
    }
}
