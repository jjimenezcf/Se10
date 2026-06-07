using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using Utilidades;

namespace ServicioDeDatos.Negocio
{


    [Table(Tablas.PLANTILLA_FILTRADO, Schema = Esquemas.NEGOCIO)]
    public class PlantillaDeFiltradoDtm : PlantillaDeUsuarioDtm, IRegistroDeParametrizacion
    {
        public int? IdVista { get; set; }
        public VistaMvcDtm VistaDtm { get; set; }
    }

    public class MisFiltros
    {
        public int Id { get; set; }
        public int IdVista { get; set; }
        public string Negocio { get; set; }
        public enumNegocio Enumerado { get; set; }
        public string Url { get; set; }
        public string Nombre { get; set; }
        public string Titulo { get; set; }
    }

    public static partial class ModeloDeNegocio
    {
        public static void PlantillasDeFiltrado(ModelBuilder modelBuilder)
        {
            ApiDeRegistroDtm.DefinirPantillaDeUsuario<PlantillaDeFiltradoDtm>(modelBuilder);
            ApiDeRegistroDtm.DefinirCampoFk<PlantillaDeFiltradoDtm>(modelBuilder, nameof(PlantillaDeFiltradoDtm.VistaDtm), nameof(PlantillaDeFiltradoDtm.IdVista), ICampos.ID_VISTA, requerida: false, unico: false);
        }
    }

}
