namespace ModeloDeDto.SistemaDocumental
{
    public enum enumClaseDePlantilla { programada, porTipo, deNegocio}

    public interface IPlantillaPlt
    {
        public int IdPlantilla { get; set; } 
        public string Plantilla { get; set; } 
        public enumClaseDePlantilla Clase {  get; set; }  
    }

    public class EstandarPlt : IPlantillaPlt
    {
        public int IdPlantilla { get ; set; } = 0;
        public string Plantilla { get; set; } = "Estandard";
        public enumClaseDePlantilla Clase { get; set; } = enumClaseDePlantilla.programada;
    }

    public class PlantillaPlt : IPlantillaPlt
    {
        public int IdPlantilla { get; set; } = 0;
        public string Plantilla { get; set; } = "Estandard";
        public enumClaseDePlantilla Clase { get; set; } = enumClaseDePlantilla.programada;
    }

}
