using ModeloDeDto.Negocio;
using ModeloDeDto.Terceros;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Gastos;
using System.Collections.Generic;
using System.Linq;

namespace ModeloDeDto.Gastos
{

    public class FacturaRecRpt : IInformacionRpt<FacturaRecDto>
    {
        public FacturaRecDto Datos { get; set; }
        public List<LineaDeUnaFarDto> Lineas { get; set; }

        public List<ObservacionDtm> Observaciones { get; set; }

        public List<TrazaDtm> Trazas { get; set; }

        public List<HitoDtm> Hitos { get; set; }

        public List<ImportePorTipoDeIva> Ivas { get; set; } = new List<ImportePorTipoDeIva>();
        public SociedadDto Sociedad { get; set; }
        public ProveedorDto Proveedor { get; set; }
        public DireccionDto Direccion { get; set; }

        public List<HistorialRpt> Historial { get; set; }

        public string Logo { get; set; }

        public void IncluirIva(LineaDeUnaFarDtm linea, bool esExtraComunitario, bool esIntraComunitario)
        {
            if (linea.IdIvaS is null)
                return;

            if (linea.IvaSoportado.Exento)
                return;

            var seleccionado = Ivas.FirstOrDefault(x => x.IdIva == linea.IvaSoportado.Id);
            if (seleccionado != null)
            {
                seleccionado.Importe = seleccionado.Importe + (decimal)linea.ImporteDeIva;
                seleccionado.BI = seleccionado.BI + linea.BaseImponible;
            }
            else
                Ivas.Add(new ImportePorTipoDeIva { 
                    IdIva = linea.IvaSoportado.Id,
                    Tipo = linea.IvaSoportado.Expresion,
                    Importe = (decimal)linea.ImporteDeIva, 
                    BI = linea.BaseImponible,
                    Porcentaje = linea.IvaSoportado.Porcentaje,
                    ClaseDeIvaRep = null,
                    ClaseDeIvaSop = linea.IvaSoportado.Clase,
                    EsExtraComunitario = esExtraComunitario,
                    EsIntraComunitario = esIntraComunitario
                });
        }
    }
}
