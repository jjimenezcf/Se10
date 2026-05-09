using ModeloDeDto.Negocio;
using ModeloDeDto.Terceros;
using System.Collections.Generic;

namespace ModeloDeDto.Ventas
{

    public class ParteTrRpt : IInformacionRpt<ParteTrDto>
    {
        public ParteTrDto Datos { get; set; }
        public List<LineaDeUnPtrDto> Lineas { get; set; }
        public SociedadDto Sociedad { get; set; }
        public ClienteDto Cliente { get; set; }
        public DireccionDto Direccion { get; set; }
        public string Logo { get; set; }
    }
}
