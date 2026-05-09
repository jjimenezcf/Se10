using ModeloDeDto.SistemaDocumental;
using System.ComponentModel;
using Utilidades;

namespace ModeloDeDto.Ventas
{

    public enum enumPltPartesTrRpt
    {
        [Description("Parte valorado")]
        parteValorado,
        [Description("Parte sin valorado")]
        parteSinValorar
    }

    public class ParteTrValoradoPlt : IPlantillaPlt
    {
        public int IdPlantilla { get; set; } = 1;
        public string Plantilla { get; set; } = enumPltPartesTrRpt.parteValorado.Descripcion();
        public enumClaseDePlantilla Clase { get; set; } = enumClaseDePlantilla.programada;

         
    }

    public class ParteTrSinValorarPlt : IPlantillaPlt
    {
        public int IdPlantilla { get; set; } = 2;
        public string Plantilla { get; set; } = enumPltPartesTrRpt.parteSinValorar.Descripcion();
        public enumClaseDePlantilla Clase { get; set; } = enumClaseDePlantilla.programada;
    }

}
