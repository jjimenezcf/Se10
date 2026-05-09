using NUnit.Framework;
using ServicioDeDatos;
using System.Collections.Generic;
using System.Linq;
using ValidacionesBase;
using DocumentFormat.OpenXml.Packaging;
using System.IO;
using ServicioDeReportes.Base;

using pltMapeosDeTablas = System.Collections.Generic.Dictionary<string, string>;
using pltFilasDeTablas = System.Collections.Generic.Dictionary<string, string>;
using pltDatosDePlantilla = System.Collections.Generic.Dictionary<string, string>;
using pltFormulasDePlantilla = System.Collections.Generic.Dictionary<string, string>;
using GestorDeElementos.Extensores;
using Utilidades;
using GestorDeElementos;
using GestoresDeNegocio;
using ServicioDeDatos.Ventas;
using GestoresDeNegocio.SistemaDocumental;
using ConvertApiDotNet;
using System.Threading.Tasks;
using System;
using Gestor.Errores;
using ServicioDeDatos.SistemaDocumental;

namespace ValidacionesDeRn
{
    /*
     
Detalle de la factura
----------------------------------------------------------------------------------
{{{lineasdefactura}}}					
----------------------------------------------------------------------------------
Concepto    Cantidad    Unidad      Precio	    Importe	    IVA	        Total
-----------------------------------------------------------------------------------
{{{col0}}}	{{{col1}}}	{{{col2}}}	{{{col3}}}	{{{col4}}}	{{{col5}}}	{{{col6}}}
-----------------------------------------------------------------------------------

Base imponible: 	{{{factura.bi}}}
Iva: 	{{{factura.iva}}}
Total: 	{{{factura.total}}}


Cobros realizados
{{{cobros}}}			
Fecha	Pendiente	Importe	Resto
{{{col0}}}	{{{col1}}}	{{{col2}}}	{{{col3}}}


     */


    class TestConPlantillas
    {

        static Dictionary<string, pltMapeosDeTablas> _descriptorDeMapeos = new()
        {
            ["lineasdefactura"] = new() { ["col0"] = "Concepto", ["col1"] = "Cantidad", ["col2"] = "Unidad", ["col3"] = "Precio", ["col4"] = "Importe", ["col5"] = "Iva", ["col6"] = "Total" },
            ["cobros"] = new() { ["col0"] = "Fecha", ["col1"] = "Pendiente", ["col2"] = "Pagado", ["col3"] = "Resto" }
        };

        static pltFormulasDePlantilla _formulasDePlantilla = new()
        {
            ["impresaEl"] = "DateTime.Now"
        };

        static Dictionary<string, pltDatosDePlantilla> _datosDePlantilla = new Dictionary<string, pltDatosDePlantilla>()
        {
            ["documento"] = new() { ["titulo"] = "Factura de prueba", ["impresaEl"] = "DateTime.Now" },
            ["factura"] = new() { ["numero"] = "256-96", ["sociedad"] = "Femdek SL", ["direccion"] = $"Calle sociedad Nº23", ["municipio"] = "Murcia 30008", ["bi"] = "1500.00", ["iva"] = "200.00", ["total"] = "1700.00" },
            ["cliente"] = new() { ["nombre"] = "Raúl Miras", ["direccion"] = $"Calle obispo frutos", ["municipio"] = "Murcia 30018" },
            ["cobrado"] = new() { ["total"] = "1221.00" }
        };

        static Dictionary<string, List<pltFilasDeTablas>> _filasDeTablas = new Dictionary<string, List<pltFilasDeTablas>>
    {
        {"lineasdefactura", new List<pltFilasDeTablas>
           {
             new() {["Concepto"] = "Sercicio x" , ["Cantidad"] = "1.00" ,  ["Unidad"] = "Ud",  ["Precio"] = "1000.00" ,  ["Importe"] = "1000.00", ["Iva"] = "210.00",["Total"] = "1210.00" },
             new() {["Concepto"] = "Sercicio x" , ["Cantidad"] = "1.00" ,  ["Unidad"] = "Ud",  ["Precio"] = "1000.00" ,  ["Importe"] = "1000.00", ["Iva"] = "210.00",["Total"] = "1210.00" },
             new() {["Concepto"] = "Sercicio x" , ["Cantidad"] = "1.00" ,  ["Unidad"] = "Ud",  ["Precio"] = "1000.00" ,  ["Importe"] = "1000.00", ["Iva"] = "210.00",["Total"] = "1210.00" },
             new() {["Concepto"] = "Sercicio x" , ["Cantidad"] = "1.00" ,  ["Unidad"] = "Ud",  ["Precio"] = "1000.00" ,  ["Importe"] = "1000.00", ["Iva"] = "210.00",["Total"] = "1210.00" },
             new() {["Concepto"] = "Sercicio x" , ["Cantidad"] = "1.00" ,  ["Unidad"] = "Ud",  ["Precio"] = "1000.00" ,  ["Importe"] = "1000.00", ["Iva"] = "210.00",["Total"] = "1210.00" },
             new() {["Concepto"] = "Sercicio x" , ["Cantidad"] = "1.00" ,  ["Unidad"] = "Ud",  ["Precio"] = "1000.00" ,  ["Importe"] = "1000.00", ["Iva"] = "210.00",["Total"] = "1210.00" },

           }
        },
        {"cobros", new List<pltFilasDeTablas>
           {
             new() {["Fecha"] = "25-03-2023" , ["Pendiente"] = "1210.00" ,  ["Pagado"] = "5", ["Resto"] = "1200.00"  },
             new() {["Fecha"] = "25-03-2023" , ["Pendiente"] = "1210.00" ,  ["Pagado"] = "5", ["Resto"] = "1200.00"  },
             new() {["Fecha"] = "25-03-2023" , ["Pendiente"] = "1210.00" ,  ["Pagado"] = "5", ["Resto"] = "1200.00"  },
             new() {["Fecha"] = "25-03-2023" , ["Pendiente"] = "1210.00" ,  ["Pagado"] = "5", ["Resto"] = "1200.00"  }
           }
        }
    };

        //----------------------------------------------------------------------------------------------------------------------------------
        [Test]
        public void GenerarPlantillas()
        {
            var contexto = Inicializaciones.CrearContextoParaUsuario(ContextoSe.Login_Admin);
            void prueba()
            {
                var ruta = @"..\..\..\..\SistemaDeElementos\Documentación\Plantillas";
                var plantilla = Path.Combine(ruta, "Test_FacturasPlt.docx");
                var resultado = Path.Combine(ruta, "Docu_Facturas.docx");
                var resultadopdf = Path.Combine(ruta, "Docu_Facturas.pdf");

                var idFactura = 2; // Reemplaza con el valor correcto

                var datos = ExtensorDePlantillas.LeerDatos(contexto, "VENTA", "Plt_EmisionDeFactura", enumNegocio.FacturaEmitida.IdNegocio(), idFactura);
                var datosDeFactura = contexto.SeleccionarPorId<FacturaEmtDtm>(idFactura).ToDictionary(contexto);

                File.Copy(plantilla, resultado, overwrite: true);
                using (WordprocessingDocument wordDocument = WordprocessingDocument.Open(resultado, true))
                {
                    var documento = wordDocument.MainDocumentPart;
                    if (documento != null)
                    {
                        var cuerpo = documento?.Document;
                        if (cuerpo != null)
                        {
                            ApiDePlantillas.ProcesarParte(cuerpo, datos.formulas, datos.datos, new Dictionary<string, Dictionary<string, object>>
                           {
                               { nameof(FacturaEmtDtm).Replace("Dtm", ""), datosDeFactura }
                           });
                            ApiDePlantillas.ProcesarMapeosDeTablasDelPa(cuerpo, datos.mapeos, datos.filas);
                        }

                        var pie = documento?.FooterParts.FirstOrDefault();
                        if (pie != null) ApiDePlantillas.ProcesarParte(pie.Footer, datos.formulas, datos.datos, new Dictionary<string, Dictionary<string, object>>
                           {
                               { nameof(FacturaEmtDtm).Replace("Dtm", ""), datosDeFactura }
                           });
                    }
                }
                var resultadoPdf =  DocxToPdf(resultado);
            }
            ApiDeValidaciones.EjecutarConRollback(contexto, prueba);
        }

        private static string DocxToPdf(string ficheroDocx)
        {
            Exception excepcionEnTaskRun = null;
            Task.Run(async () =>
            {
                try
                {
                    var covertapi = new ConvertApi("1CPToQIEOjozKPfq");
                    var resutadoConversion = await covertapi.ConvertAsync(enumExtensiones.docx.ToString(), enumExtensiones.pdf.ToString(), new ConvertApiFileParam("File", ficheroDocx));
                    if (resutadoConversion.Files.Count() == 1)
                    {
                        foreach (var file in resutadoConversion.Files)
                        {
                            var directorioDeSalida = Path.GetDirectoryName(ficheroDocx);
                            await resutadoConversion.SaveFilesAsync(directorioDeSalida);
                        }
                    }
                    else
                    {
                        GestorDeErrores.Emitir("La conversión no produjo archivos. Revisa el resultado.");
                    }
                }
                catch (Exception ex)
                {
                    excepcionEnTaskRun = ex;
                }
            }).Wait();

            if (excepcionEnTaskRun != null)
                throw excepcionEnTaskRun;

            return Path.Combine(Path.GetDirectoryName(ficheroDocx), $"{Path.GetFileNameWithoutExtension(ficheroDocx)}.{enumExtensiones.pdf}");
        }

        //---------------------------------------------------------------------------------------------------------------------
        [Test]
        public void ServiciodeImpresion()
        {
            var contexto = Inicializaciones.CrearContextoParaUsuario(ContextoSe.Login_Admin);
            void prueba()
            {
                var fichero = new ServicioDeImpresion(contexto, enumNegocio.FacturaEmitida, 103, 10).Imprimir(ModeloDeDto.SistemaDocumental.enumClaseDePlantilla.deNegocio);
            }
            ApiDeValidaciones.EjecutarConRollback(contexto, prueba);
        }
    }

}
