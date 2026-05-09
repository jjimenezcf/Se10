using Gestor.Errores;
using GestorDeElementos.Extensores;
using ModeloXml.eFactura;
using ModeloXml.eFactura.Facturae32;
using ModeloXml.eFactura.Facturae322;
using ServicioDeDatos;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.Ventas;
using System;
using System.Collections.Generic;
using Utilidades;
using GestorDeElementos;
using static ServicioDeDatos.Elemento.Enumerados;
namespace GestoresDeNegocio.Ventas
{


    public class GeneradorDeFacturaEmtXml
    {
        protected ContextoSe Contexto { get; }
        protected FacturaEmtDtm Factura { get; }
        protected string Ruta { get; }

        protected SociedadDtm _emisor = null;
        protected SociedadDtm Emisor => _emisor ?? Factura.Sociedad(Contexto);

        public GeneradorDeFacturaEmtXml(ContextoSe contexto, FacturaEmtDtm factura, string rutaConFichero)
        {
            Contexto = contexto;
            Factura = factura;
            Ruta = rutaConFichero;
        }

        public string Generar() => Generar(Factura.ClaseDeEmision);


        public string Generar(enumClaseDeEmision version)
        {
            switch (version)
            {
                case enumClaseDeEmision.eFactura322:
                    return new GeneradorDeFacturaEmtXml322(Contexto, Factura, Ruta).GenerarXml(new eFactura322());
                case enumClaseDeEmision.eFactura32:
                    return new GeneradorDeFacturaEmtXml32(Contexto, Factura, Ruta).GenerarXml(new eFactura32());
            }

            throw new NotImplementedException($"no está implementado como generar el xml asociado a {Factura.ClaseDeEmision}");
        }

        protected virtual string GenerarXml(IeFactura efactura) => string.Empty;

        protected string[] LiteralesLegales()
        {
            var legalLiterals = new List<string>();
            var ivas = Factura.Ivas(Contexto);
            var lineas = Factura.Detalles<LineaDeUnaFaeDtm>(Contexto);

            foreach (var linea in lineas)
            {
                if (linea.TipoDeLinea == enumTipoDeLinea.Comentario || linea.IdIvaR is null)
                    continue;

                var ivaDeLaLinea = linea.IvaRepercutido(Contexto);
                if (ivaDeLaLinea.Clase == ServicioDeDatos.Contabilidad.enumClasesDeIvaRep.ISP)
                {
                    if (ivaDeLaLinea.DescripcionFiscal.IsNullOrEmpty())
                        GestorDeErrores.Emitir($"Debe indicar el motivo ISP para el iva '{ivaDeLaLinea.Nombre}'");

                }
                else if (ivaDeLaLinea.Exento)
                {
                    if (ivaDeLaLinea.DescripcionFiscal.IsNullOrEmpty())
                        GestorDeErrores.Emitir($"Debe indicar el motivo de la excción fiscal para el iva '{ivaDeLaLinea.Nombre}'");
                }
                else continue;


                if (legalLiterals.Contains(ivaDeLaLinea.DescripcionFiscal))
                    continue;

                legalLiterals.Add(ivaDeLaLinea.DescripcionFiscal);
            }

            return legalLiterals.Count > 0 ? legalLiterals.ToArray() : null;
        }
    }
}
