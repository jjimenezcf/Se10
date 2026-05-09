using ServicioDeDatos.Gastos;
using ServicioDeDatos.Ventas;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ServicioDeDatos.Contabilidad
{
    public class ImportePorTipoDeIva
    {
        public int IdIva { get; set; }
        public string Tipo { get; set; }
        public decimal BI { get; set; }
        public decimal Porcentaje { get; set; }
        public decimal Importe { get; set; }
        public decimal Cuota { get; set; }
        public bool EsExportacion => EsExtraComunitario || EsIntraComunitario;
        public bool EsExtraComunitario { get; set; }
        public bool EsIntraComunitario { get; set; }
        public enumClasesDeIvaRep? ClaseDeIvaRep { get; set; }
        public enumClasesDeIvaSop? ClaseDeIvaSop { get; set; }
        public bool EsIsp => ClaseDeIvaRep == null ? ClaseDeIvaSop == enumClasesDeIvaSop.ISP : ClaseDeIvaRep == enumClasesDeIvaRep.ISP;
        public bool EsNosujeto => ClaseDeIvaRep == null ? ClaseDeIvaSop == enumClasesDeIvaSop.NSJ : ClaseDeIvaRep == enumClasesDeIvaRep.NSJ;
        public bool EsExento => Porcentaje == 0;

    }

    public static class ImportePorTipoDeIvaExtension
    {

        public static void TotalizarIvaSoportado(this List<ImportePorTipoDeIva> ivas, LineaDeUnaFarDtm linea, bool esIntraComunitario, bool esExtraComunitario)
        {
            var iva = ivas.FirstOrDefault(i => i.IdIva == linea.IvaSoportado.Id);
            if (iva == null)
            {
                iva = new ImportePorTipoDeIva
                {
                    IdIva = linea.IvaSoportado.Id,
                    Tipo = linea.IvaSoportado.Expresion,
                    Porcentaje = linea.IvaSoportado.Porcentaje,
                    ClaseDeIvaRep = null,
                    ClaseDeIvaSop = linea.IvaSoportado.Clase,
                    EsIntraComunitario = esIntraComunitario,
                    EsExtraComunitario = esExtraComunitario
                };
                ivas.Add(iva);
            }
            iva.BI += linea.BaseImponible;
            iva.Importe += linea.ImporteDeIva ?? 0;
        }


        public static void TotalizarIvaRepercutido(this List<ImportePorTipoDeIva> ivas, LineaDeUnaFaeDtm linea, bool esExtraComunitario, bool esIntraComunitario)
        {
            if (linea.IdIvaR is null)
                return;

            var seleccionado = ivas.FirstOrDefault(x => x.IdIva == linea.IvaRepercutido.Id);
            if (seleccionado != null)
            {
                seleccionado.Importe = seleccionado.Importe + linea.ImporteDeIva;
                seleccionado.Cuota = seleccionado.Cuota + linea.CuotaRepercutida(esExtraComunitario, esIntraComunitario);
                seleccionado.BI = seleccionado.BI + linea.ImporteConDto;
            }
            else
            {
                var iva = new ImportePorTipoDeIva
                {
                    IdIva = linea.IvaRepercutido.Id,
                    Tipo = linea.IvaRepercutido.Porcentaje == 0 ? linea.IvaRepercutido.Exencion : linea.IvaRepercutido.Expresion,
                    BI = linea.ImporteConDto,
                    Porcentaje = linea.IvaRepercutido.Porcentaje,
                    Importe = linea.ImporteDeIva,
                    ClaseDeIvaSop = null,
                    ClaseDeIvaRep = linea.IvaRepercutido.Clase,
                    EsExtraComunitario = esExtraComunitario,
                    EsIntraComunitario = esIntraComunitario
                };
                iva.Cuota = iva.CuotaRepercutida();
                ivas.Add(iva);
            }
        }

        private static decimal CuotaRepercutida(this ImportePorTipoDeIva iva)
        {
            if (iva.EsNosujeto || iva.EsIntraComunitario || iva.EsExtraComunitario)
                return 0;
            return Math.Round(iva.Importe, 2);
        }

        private static decimal CuotaRepercutida(this LineaDeUnaFaeDtm linea, bool esExtraComunitario, bool esIntraComunitario)
        {
            if (linea.IvaRepercutido.Clase == enumClasesDeIvaRep.NSJ || esIntraComunitario || esExtraComunitario)
                return 0;
            return Math.Round(linea.Iva.HasValue ? (decimal)linea.Iva : 0, 2);
        }
    }
}
