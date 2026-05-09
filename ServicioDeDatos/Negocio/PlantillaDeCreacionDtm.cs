using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;

namespace ServicioDeDatos.Negocio
{

    [Table(Tablas.PLANTILLA_CREACION, Schema = Esquemas.NEGOCIO)]
    public class PlantillaDeCreacionDtm : PlantillaDeUsuario
    {

    }

    public static partial class ModeloDeNegocio
    {
        public static void PlantillasDeCreacion(ModelBuilder modelBuilder) => ApiDeRegistroDtm.DefinirPantillaDeUsuario<PlantillaDeCreacionDtm>(modelBuilder);
    }

}
