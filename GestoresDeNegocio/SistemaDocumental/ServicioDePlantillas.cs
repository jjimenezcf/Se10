using GestorDeElementos;
using GestorDeElementos.Extensores;
using ModeloDeDto.SistemaDocumental;
using ModeloDeDto.Ventas;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Ventas;
using System.Collections.Generic;
using System.Linq;
using Utilidades;

namespace GestoresDeNegocio.SistemaDocumental
{
    public class PlantillasDto
    {

        public List<IPlantillaPlt> Plantillas { get; set; }
        public bool Abrir { get; set; }

    }

    public interface IServicioDePlantillas
    {
        public PlantillasDto Plantillas();
    }


    public class ServicioDePlantillas : IServicioDePlantillas
    {
        public ContextoSe Contexto { get; }
        public enumNegocio Negocio { get; }
        public int? IdTipo { get; }

        public int? IdElemento { get; }

        public ServicioDePlantillas(ContextoSe contexto)
        {
            Contexto = contexto;
            Negocio = enumNegocio.No_Definido;
        }

        public ServicioDePlantillas(ContextoSe contexto, enumNegocio negocio)
        : this(contexto, negocio, null)
        {
        }

        public ServicioDePlantillas(ContextoSe contexto, enumNegocio negocio, List<int> ids)
        : this(contexto)
        {
            Negocio = negocio;
            IdTipo = negocio.UsaTipo() && ids is not null && ids.Count > 0 
            ? ((IUsaTipo)Negocio.SeleccionarPorId(contexto, ids[0])).IdTipo 
            : null;

            if (ids != null && ids.Count == 1) IdElemento = ids[0];

        }

        public PlantillasDto Plantillas()
        {
            var plantillas = new PlantillasDto();
            List<IPlantillaPlt> plantillasFijas = new List<IPlantillaPlt> { new EstandarPlt() };

            switch (Negocio)
            {
                case enumNegocio.FacturaEmitida:
                    if (IdElemento is not null)
                    {
                        var factura = Contexto.SeleccionarPorId<FacturaEmtDtm>((int)IdElemento);
                        plantillasFijas = new List<IPlantillaPlt> 
                        { 
                            factura.Etapas().Contains(enumEtapasDeFacturasEmt.FAE_Etapa_Prefactura)
                            ? new PrefacturaPlt()
                            : new CopiaDeFacturaPlt()
                        };
                    }
                    else
                    {
                        plantillasFijas = new List<IPlantillaPlt> { new PrefacturaPlt() };
                    }
                    break;
                case enumNegocio.ParteDeTrabajo:
                    plantillasFijas = new List<IPlantillaPlt> { new ParteTrValoradoPlt(), new ParteTrSinValorarPlt() };
                    break;
            }

            var pantillasDeBd = Negocio.LeerPlantillasPlt(Contexto, IdTipo);

            List<IPlantillaPlt> listaUnida = pantillasDeBd.Count > 0
            ? Enumerable.Concat(plantillasFijas, pantillasDeBd).ToList()
            : plantillasFijas;

            plantillas.Plantillas = listaUnida;
            plantillas.Abrir = plantillas.Plantillas.Count > 1;
            return plantillas;
        }

    }
}
