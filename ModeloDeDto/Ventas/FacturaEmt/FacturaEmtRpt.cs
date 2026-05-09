using ModeloDeDto.Negocio;
using ModeloDeDto.Reporte;
using ModeloDeDto.Terceros;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Ventas;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilidades;

namespace ModeloDeDto.Ventas
{

    public static class ltrParametrosFaeRpt
    {
        public static string ImprimirVencimiento = nameof(ImprimirVencimiento);
        public static string MostrarPalabraCopia = nameof(MostrarPalabraCopia);
    }


    public class FacturaEmtRpt : InformacionBaseRpt<FacturaEmtDto>
    {
        public FacturaEmtDtm FacturaDtm { get; set; }  
        public List<LineaDeUnaFaeDto> Lineas { get; set; }
        public List<ImportePorTipoDeIva> Ivas { get; set; } = new List<ImportePorTipoDeIva>();
        public List<ImportePorTipoDeIrpf> Retenciones { get; set; } = new List<ImportePorTipoDeIrpf>();
        public List<ImportePorTipoDeIva> Exenciones { get; set; } = new List<ImportePorTipoDeIva>();
        public SociedadDto Sociedad { get; set; }
        public ClienteDto Cliente { get; set; }
        public DireccionDto Direccion { get; set; }
        public string Logo { get; set; }
        public string InformacionFiscal { get; set; }
        public string PieDeFactura { get; set; }
        public string InscritoEn { get; set; }
        public bool HayDescuento => Lineas.Any(linea => linea.ImporteDeDto is not null && linea.ImporteDeDto > 0);
        public bool ImprimirVencimiento => extDiccionarios.LeerValor(Parametros, ltrParametrosFaeRpt.ImprimirVencimiento, false);
        public bool MostrarPalabraCopia => extDiccionarios.LeerValor(Parametros, ltrParametrosFaeRpt.MostrarPalabraCopia, true);

        public decimal BiConIva { get; set; }

        public string UrlAeat { get; set; }
        public string UrlSe { get; set; }

        public string LeyendaAeat { get; set; }
        public string LeyendaSe { get; set; }

        public void IncluirIva(LineaDeUnaFaeDtm linea)
        {
            if (linea.IdIvaR is null)
                return;

            if (linea.IvaRepercutido.Exento)
                return;

            var seleccionado = Ivas.FirstOrDefault(x => x.IdIva == linea.IvaRepercutido.Id);
            if (seleccionado != null)
            {
                seleccionado.Importe = seleccionado.Importe + linea.ImporteDeIva;
                seleccionado.BI = seleccionado.BI + linea.ImporteConDto;
            }
            else
                Ivas.Add(new ImportePorTipoDeIva { 
                    IdIva = linea.IvaRepercutido.Id,
                    Tipo = linea.IvaRepercutido.Expresion, 
                    BI = linea.ImporteConDto,
                    Porcentaje = linea.IvaRepercutido.Porcentaje,
                    Importe = linea.ImporteDeIva });
        }

        public void IncluirExento(LineaDeUnaFaeDtm linea)
        {
            if (linea.IdIvaR is null)
                return;

            if (!linea.IvaRepercutido.Exento)
                return;

            var seleccionado = Exenciones.FirstOrDefault(x => x.IdIva == linea.IvaRepercutido.Id);
            if (seleccionado != null)
                seleccionado.BI = seleccionado.BI + linea.ImporteConDto;
            else
                Exenciones.Add(new ImportePorTipoDeIva { 
                    IdIva = linea.IvaRepercutido.Id,
                    Tipo = linea.IvaRepercutido.Exencion, 
                    BI = linea.ImporteConDto, 
                    Porcentaje = 0,
                    Importe = 0 });
        }

        public void IncluirIrpf(IrpfEmtDtm irpfEmt)
        {
            var seleccionado = Retenciones.FirstOrDefault(x => x.IdIrpf == irpfEmt.TipoIrpf.Id);
            if (seleccionado != null)
            {
                seleccionado.Importe = seleccionado.Importe + (decimal)irpfEmt.Importe;
                seleccionado.BI = seleccionado.BI + (decimal)irpfEmt.BiSujeta;
            }
            else
                Retenciones.Add(new ImportePorTipoDeIrpf { 
                    IdIrpf = irpfEmt.TipoIrpf.Id,
                    Tipo = $"IRPF: {irpfEmt.TipoIrpf.Detalle}", 
                    BI = (decimal)irpfEmt.BiSujeta, 
                    Porcentaje = (decimal)irpfEmt.Irpf, 
                    Importe = (decimal)irpfEmt.Importe });
        }

        public override bool VerificarVersionDeParametros()
        =>
        base.VerificarVersionDeParametros() && 
        Parametros.ContieneClave(ltrParametrosFaeRpt.ImprimirVencimiento) &&
        Parametros.ContieneClave(ltrParametrosFaeRpt.MostrarPalabraCopia);

        public override void ActualizarVersionDeParametros(int idNegocio, Enum parametro)
        {
            if (Parametros is null) Parametros = new Dictionary<string, object>();
            if (!Parametros.ContieneClave(ltrParametrosFaeRpt.ImprimirVencimiento)) Parametros[ltrParametrosFaeRpt.ImprimirVencimiento] = true;
            if (!Parametros.ContieneClave(ltrParametrosFaeRpt.MostrarPalabraCopia)) Parametros[ltrParametrosFaeRpt.MostrarPalabraCopia] = true;
            base.ActualizarVersionDeParametros(idNegocio, parametro);
        }
    }
}
