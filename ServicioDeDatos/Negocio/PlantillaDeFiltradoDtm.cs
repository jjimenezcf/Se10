using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;

namespace ServicioDeDatos.Negocio
{


    [Table(Tablas.PLANTILLA_FILTRADO , Schema = Esquemas.NEGOCIO)]
    public class PlantillaDeFiltradoDtm : PlantillaDeUsuario, IRegistroDeParametrizacion
    {
    }

    public static partial class ModeloDeNegocio
    {
        public static void PlantillasDeFiltrado(ModelBuilder modelBuilder) => ApiDeRegistroDtm.DefinirPantillaDeUsuario<PlantillaDeFiltradoDtm>(modelBuilder);
    }

}
