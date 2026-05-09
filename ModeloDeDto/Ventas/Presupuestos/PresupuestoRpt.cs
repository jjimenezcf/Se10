using ModeloDeDto.Negocio;
using ModeloDeDto.Reporte;
using ModeloDeDto.Terceros;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Presupuesto;
using System.Collections.Generic;
using System.Linq;

namespace ModeloDeDto.Presupuesto
{

    public class PresupuestoRpt : InformacionBaseRpt<PresupuestoDto>
    {
        public List<LineaDeUnPptDto> Lineas { get; set; }
        public List<ImportePorTipoDeIva> Ivas { get; set; } = new List<ImportePorTipoDeIva>();
        public SociedadDto Sociedad { get; set; }
        public InterlocutorDto Solicitante { get; set; }
        public DireccionDto Direccion { get; set; }
        public string Logo { get; set; }
        public string RazonSocialDelCliente { get; set; }

        public string PieDePresupuesto { get; set; }
        public string InscritoEn { get; set; }
        public bool HayDescuento => Lineas.Any(linea => linea.ImporteDeDto is not null && linea.ImporteDeDto > 0);

        public void IncluirIva(LineaDeUnPptDtm linea)
        {
            if (linea.IdIvaR is null)
                return;

            var seleccionado = Ivas.FirstOrDefault(x => x.IdIva == linea.IvaRepercutido.Id);
            if (seleccionado != null)
                seleccionado.Importe = seleccionado.Importe + linea.ImporteDeIva;
            else
                Ivas.Add(new ImportePorTipoDeIva
                {
                    IdIva = linea.IvaRepercutido.Id,
                    Tipo = linea.IvaRepercutido.Expresion,
                    Importe = linea.ImporteDeIva
                });
        }
    }
}
