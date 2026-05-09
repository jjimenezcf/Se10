using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;

namespace GestorDeElementos.Extensores.Contabilidad
{
    using Gestor.Errores;
    using Microsoft.EntityFrameworkCore;
    using ModeloDeDto.Negocio;
    using ServicioDeDatos.Elemento;
    using ServicioDeDatos.Gastos;
    using ServicioDeDatos.Terceros;
    using ServicioDeDatos.Ventas;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Linq.Dynamic.Core;
    using System.Xml;
    using Utilidades;

    enum enumNcsCodigoConcepto : int
    {
        [Description("Este código corresponde a las facturas de ingresos")]
        FacturasIngresos = 1,
        [Description("Este código corresponde a las facturas de rectificación de ingresos")]
        RectificacionIngresos = 2,
        [Description("Este código corresponde a las facturas de pagos")]
        FacturasPagos = 3,
        [Description("Este código corresponde a las facturas de rectificación de pagos")]
        RectificacionPagos = 4,
        [Description("Este código corresponde a cobros de facturas de clientes")]
        CobrosFacturasClientes = 5,
        [Description("Este código corresponde a pago facturas de proveedores")]
        PagoFacturasProveedores = 6,
        [Description("Gastos de una sociedad sin iva")]
        GastosPorNaturaleza = 7,
        [Description("Este código corresponde a abono de remesa")]
        AbonoRemesa = 29,
        [Description("Este código corresponde al saldo de apertura")]
        SaldoApertura = 30,
        [Description("Este código corresponde a suplidos")]
        Suplidos = 31,
        [Description("Este código corresponde a liquidación de IVA")]
        LiquidacionIVA = 32,
        [Description("Este código corresponde cuando la empresa trabaja con plan de cuentas de mayor. Todos sus asientos irán con este código")]
        PlanCuentasMayor = 99
    }

    enum enumNcsValoracion : int
    {
        [Description("Para asientos que se visualizarán desde ambos criterios contables. Contabilidad oficial y real")]
        OficialReal = 0,
        [Description("Para asientos que se visualizarán solo desde criterio contable. Contabilidad real y no oficial")]
        RealNoOficial = 1,
        [Description("Para asiento que se visualizarán desde ambos criterios contables pero con distinta valoración interna. Contabilidad oficial y no real")]
        ValoracionDiferente = 2
    }

    static class ltrNcsPlanContable
    {
        internal const string IvaDePagos = "47200000001";
        internal const string IvaDeDevolucionPagos = "47200000002";
        internal const string IvaIngresos = "47700000001";
        internal const string IvaDevolucionIngresos = "47700000002";
        internal const string PerceptorDeRetencion = "40000000001";
        internal const string OrigenDeDatos = "3";
        internal const string VersionProveedores = "3.615";
        internal const int LongitudDeCuenta = 11;
        internal static readonly string AsientoLibre = "20";
        internal static readonly string AsientoConIva = "21";
        internal static readonly string DevolucionCompra = "3";
        internal static readonly string Compra = "2";
        internal static readonly string DevolucionVenta = "1";
        internal static readonly string Venta = "0";
        internal static readonly string Rectificativa = "R1";
    }

    class RegistroDeIva
    {
        internal GeneradorBaseNcs.enumNcsPorcentageIva PorcentajeNcs { get; set; }
        internal decimal Bi { get; set; }
        internal string CuentaProveedor { get; set; }
        internal string CuentaCliente { get; set; }
        internal string CuentaGasto { get; set; }
        internal string CuentaIngreso { get; set; }
        internal decimal Cuota { get; set; }
        internal GeneradorBaseNcs.enumNcsTipoOperacion TipoNcs { get; set; }
        internal bool EsIvaRepercutido { get; set; }
        internal bool EsIvaSoportado { get; set; }
        internal bool EsElIvaIsp { get; set; }
        internal string CuentaDeIvaSoportado { get; set; }
        internal string CuentaDeIvaRepercutido { get; set; }

    }

    public class LoteContableConNcs : GeneradorBaseNcs
    {
        private int NumeroDeOrden { get; set; }
        private List<PreasientoDtm> Preasientos { get; set; }

        public LoteContableConNcs(ContextoSe contexto, List<PreasientoDtm> preasientos, SociedadDtm sociedad, int ejercicio, bool respetarFechaContable, DateTime? fechaContable)
        : base(contexto, sociedad, fechaContable is null ? ejercicio : ((DateTime)fechaContable).Year)
        {
            Preasientos = preasientos;
            FechaContable = fechaContable;
            RespetarFechacontable = respetarFechaContable;
            NumeroDeOrden = 0;
            DiarioXml = Path.Combine(CacheDeVariable.Cfg_RutaDeDescarga, $"Diario{Sociedad.PlanContable().IdPlanContable.PadLeft(4, '0')}_{Ejercicio}.{enumExtensiones.xml}");
            TercerosXml = ApiDeArchivos.ObtenerNombreUnico(Path.Combine(CacheDeVariable.Cfg_RutaDeDescarga,
                $"CtaDetalle{Sociedad.PlanContable().IdPlanContable.PadLeft(4, '0')}_{Ejercicio}.{enumExtensiones.xml}"));
        }

        public void GenerarTerceros((List<ProveedorDtm> proveedores, List<ClienteDtm> clientes) terceros)
        {
            using (var fileStream = new FileStream(TercerosXml, FileMode.Create))
            using (var xmlWriter = XmlWriter.Create(fileStream, new XmlWriterSettings { Indent = true }))
            {
                xmlWriter.WriteStartDocument();
                xmlWriter.WriteStartElement("PlanCuentasDetalle");
                xmlWriter.WriteAttributeString("xmlns", "xsi", null, _xsi);
                xmlWriter.WriteAttributeString("xmlns", "xsd", null, _xsd);

                xmlWriter.WriteElementString("VERSION_SCRIPT", ltrNcsPlanContable.VersionProveedores);
                xmlWriter.WriteElementString("ORIGEN_DATOS", ltrNcsPlanContable.OrigenDeDatos);

                foreach (ITerceroContable tercero in terceros.proveedores)
                {
                    WriteTercero(xmlWriter, enumNegocio.Proveedor, tercero);
                }
                foreach (ITerceroContable tercero in terceros.clientes)
                {
                    WriteTercero(xmlWriter, enumNegocio.Cliente, tercero);
                }
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndDocument();
            }
        }
        private void WriteTercero(XmlWriter xmlWriter, enumNegocio negocioTercero, ITerceroContable tercero)
        {
            var nif = negocioTercero == enumNegocio.Proveedor ? ((ProveedorDtm)tercero).NIF(Contexto) : ((ClienteDtm)tercero).NIF(Contexto);
            var interlocutor = tercero.Interlocutor(Contexto);

            xmlWriter.WriteStartElement("CtaDetalle");
            xmlWriter.WriteElementString("COD_CUENTA", CuentaDelTercero(tercero.CodigoDeCtaContable(Contexto)));
            xmlWriter.WriteElementString("TITULO_CTA", interlocutor.RazonSocial(Contexto));
            xmlWriter.WriteElementString("EXPLOTACION", $"");
            xmlWriter.WriteElementString("CIF_NIF", nif);
            xmlWriter.WriteElementString("TIPO_DOCUM", $"9");
            xmlWriter.WriteElementString("TIPO_PERSON", $"{(interlocutor.EsPersona ? "1" : "2")}");
            xmlWriter.WriteElementString("APELLIDO_1", $"{(interlocutor.EsPersona ? interlocutor.Persona(Contexto).Apellidos : "")}");
            xmlWriter.WriteElementString("APELLIDO_2", "");
            xmlWriter.WriteElementString("NOMBRE", interlocutor.RazonSocial(Contexto));
            xmlWriter.WriteElementString("SUC", $"");
            xmlWriter.WriteElementString("SIT_FAMILIAR", $"");
            xmlWriter.WriteElementString("PAGINA_WEB", $"");
            xmlWriter.WriteElementString("TIPO_DOCUM_REPRESENTANTE", $"");
            xmlWriter.WriteElementString("TIPO_PERSONA_REPRESENTANTE", $"");
            xmlWriter.WriteElementString("CIF_NIF_REPRESENTANTE", $"");
            xmlWriter.WriteElementString("APELLIDO_1_REPRESENTANTE", $"");
            xmlWriter.WriteElementString("APELLIDO_2_REPRESENTANTE", $"");
            xmlWriter.WriteElementString("NOMBRE_REPRESENTANTE", $"");
            xmlWriter.WriteElementString("NOTA_1", $"");
            xmlWriter.WriteElementString("NOTA_2", $"");
            xmlWriter.WriteElementString("NOTA_3", $"");
            xmlWriter.WriteElementString("NOTA_4", $"");
            xmlWriter.WriteElementString("NOTA_5", $"");
            xmlWriter.WriteElementString("TITULO_LIBRE_1", $"");
            xmlWriter.WriteElementString("TITULO_LIBRE_2", $"");
            xmlWriter.WriteElementString("TITULO_LIBRE_3", $"");
            xmlWriter.WriteElementString("TITULO_LIBRE_4", $"");
            xmlWriter.WriteElementString("TITULO_LIBRE_5", $"");
            xmlWriter.WriteElementString("TITULO_LIBRE_6", $"");
            xmlWriter.WriteElementString("TITULO_LIBRE_7", $"");
            xmlWriter.WriteElementString("TITULO_LIBRE_8", $"");
            xmlWriter.WriteElementString("TITULO_LIBRE_9", $"");
            xmlWriter.WriteElementString("TITULO_LIBRE_10", $"");
            xmlWriter.WriteElementString("TITULO_LIBRE_11", $"");
            xmlWriter.WriteElementString("TITULO_LIBRE_12", $"");
            xmlWriter.WriteElementString("TITULO_LIBRE_13", $"");
            xmlWriter.WriteElementString("TITULO_LIBRE_14", $"");
            xmlWriter.WriteElementString("TITULO_LIBRE_18", $"");
            xmlWriter.WriteElementString("TITULO_LIBRE_19", $"");
            xmlWriter.WriteElementString("TITULO_LIBRE_20", $"");
            xmlWriter.WriteElementString("TEXTO_ASOCIADO", $"");

            xmlWriter.WriteStartElement("Domicilios");
            xmlWriter.WriteStartElement("Domicilio");
            xmlWriter.WriteElementString("SIGLAS", $"");
            xmlWriter.WriteElementString("DOMICILIO", $"");
            xmlWriter.WriteElementString("NUMERO", $"");
            xmlWriter.WriteElementString("LETRA", $"");
            xmlWriter.WriteElementString("ESCALERA", $"");
            xmlWriter.WriteElementString("BLOQUE", $"");
            xmlWriter.WriteElementString("PISO", $"");
            xmlWriter.WriteElementString("PUERTA", $"");
            xmlWriter.WriteElementString("ZONA", $"");
            xmlWriter.WriteElementString("C_POSTAL", $"");
            xmlWriter.WriteElementString("MUNICIPIO", $"");
            xmlWriter.WriteElementString("POBLACION", $"");
            xmlWriter.WriteElementString("PAIS", $"");
            xmlWriter.WriteElementString("TIPO", $"");
            xmlWriter.WriteElementString("ASIGDECLARACION", $"");
            xmlWriter.WriteEndElement(); // Domicilio
            xmlWriter.WriteEndElement(); // Domicilios

            xmlWriter.WriteStartElement("Telefonos");
            for (int i = 0; i < 2; i++)
            {
                xmlWriter.WriteStartElement("Telefono");
                xmlWriter.WriteElementString("TELEFONO", $"");
                xmlWriter.WriteElementString("TIPO", $"");
                xmlWriter.WriteElementString("USO", $"");
                xmlWriter.WriteElementString("CONTACTO", $"");
                xmlWriter.WriteEndElement(); // Telefono
            }
            xmlWriter.WriteEndElement(); // Telefonos

            xmlWriter.WriteStartElement("EMails");
            xmlWriter.WriteStartElement("EMail");
            xmlWriter.WriteElementString("EMAIL", $"");
            xmlWriter.WriteElementString("USO", $"");
            xmlWriter.WriteElementString("CONTACTO", $"");
            xmlWriter.WriteEndElement(); // EMail
            xmlWriter.WriteEndElement(); // EMails

            xmlWriter.WriteElementString("Datos_Bancarios", $"");

            xmlWriter.WriteEndElement(); // CtaDetalle
        }

        public void GenerarLoteContable()
        {
            using (var fileStream = new FileStream(DiarioXml, FileMode.Create))
            using (var xmlWriter = XmlWriter.Create(fileStream, new XmlWriterSettings { Indent = true }))
            {
                xmlWriter.WriteStartDocument();
                xmlWriter.WriteStartElement("LISTAASIENTOS");
                xmlWriter.WriteAttributeString("xmlns", "xsi", null, _xsi);
                xmlWriter.WriteAttributeString("xmlns", "xsd", null, _xsd);

                xmlWriter.WriteStartElement("ORIGEN");
                xmlWriter.WriteElementString("ORIGEN_DATOS", ltrNcsPlanContable.OrigenDeDatos);
                xmlWriter.WriteElementString("EJERCICIO", Ejercicio.ToString());
                xmlWriter.WriteElementString("CIF_NIF", Sociedad.NIF);
                xmlWriter.WriteEndElement();

                foreach (var preasiento in Preasientos)
                {
                    try
                    {
                        WriteAsiento(xmlWriter, preasiento);
                    }
                    catch (Exception ex)
                    {
                        throw Excepciones.Emitir($"No se ha podido generar el asiento contable del preasiento '{preasiento.Referencia}'{Environment.NewLine}{GestorDeErrores.Detalle(ex)}");
                    }
                }

                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndDocument();
            }
        }

        private void WriteAsiento(XmlWriter xmlWriter, PreasientoDtm preasiento)
        {
            if (preasiento.NegocioReferenciado == enumNegocio.FacturaRecibida)
            {
                var elemento = preasiento.Referenciado(Contexto);
                if (elemento.GetType() != typeof(FacturaRecDtm))
                {
                    throw Excepciones.Emitir($"El preasiento '{preasiento.Referencia}' tiene como negocio referenciado '{preasiento.NegocioReferenciado.Singular()}' pero el elemento referenciado es de tipo '{elemento.GetType().Name}' en lugar de ser de tipo '{typeof(FacturaRecDtm).Name}'");
                }
            }

            xmlWriter.WriteStartElement("ASIENTO");
            switch (preasiento.NegocioReferenciado)
            {
                case enumNegocio.FacturaEmitida:
                    AsientoDeFacturaEmitida(xmlWriter, preasiento);
                    break;
                case enumNegocio.Cobro:
                    AsientoDeCobro(xmlWriter, preasiento);
                    break;
                case enumNegocio.RemesaFae:
                    break;
                case enumNegocio.Pago:
                    AsientoDePago(xmlWriter, preasiento);
                    break;
                case enumNegocio.RemesaPag:
                    break;
                case enumNegocio.FacturaRecibida:
                    AsientoDeFacturaRecibida(xmlWriter, preasiento);
                    break;
                default:
                    GestorDeErrores.Emitir($"Falta implementar cómo definir un asiento para el negocio '{preasiento.NegocioReferenciado.Singular()}'");
                    break;
            }
            xmlWriter.WriteEndElement();
        }

        private void AsientoDePago(XmlWriter xmlWriter, PreasientoDtm preasiento)
        {
            WriteCabecera(xmlWriter, preasiento);
            xmlWriter.WriteStartElement("LISTAREGIVA");
            xmlWriter.WriteEndElement();
            WriteAsientoDePago(xmlWriter, preasiento);
        }

        private void AsientoDeCobro(XmlWriter xmlWriter, PreasientoDtm preasiento)
        {
            WriteCabecera(xmlWriter, preasiento);
            xmlWriter.WriteStartElement("LISTAREGIVA");
            xmlWriter.WriteEndElement();
            WriteAsientoDeCobro(xmlWriter, preasiento);
        }
        private void WriteAsientoDePago(XmlWriter xmlWriter, PreasientoDtm preasiento)
        {
            xmlWriter.WriteStartElement("LISTAAPUNTES");

            var pago = (PagoDtm)preasiento.Referenciado(Contexto);
            if (pago.IdNaturaleza is null)
                ApunteDeAcreedor(xmlWriter, preasiento);
            else
                ApuntesDeGastoIngreso(xmlWriter, preasiento, enumNcsCodigoConcepto.GastosPorNaturaleza);
            ApunteDePago(xmlWriter, preasiento);
            xmlWriter.WriteEndElement();
        }

        private void WriteAsientoDeCobro(XmlWriter xmlWriter, PreasientoDtm preasiento)
        {
            xmlWriter.WriteStartElement("LISTAAPUNTES");
            ApunteDeDeudor(xmlWriter, preasiento);
            ApunteDeCobro(xmlWriter, preasiento);
            xmlWriter.WriteEndElement();
        }
        private void ApunteDeAcreedor(XmlWriter xmlWriter, PreasientoDtm preasiento)
        {
            var apunte = preasiento.Detalles<ApunteDeUnPreasientoDtm>(Contexto).First(apunte => apunte.Clase == enumClaseDeApunte.Pag_Acreedor);

            xmlWriter.WriteStartElement("APUNTE");
            xmlWriter.WriteElementString("CODIGOCTA", CuentaDelTercero(apunte.Cuenta));
            xmlWriter.WriteElementString("IMPORTE", apunte.Importe.Formatear(decimales: 6, alineacion: false, separadorDecimal: ","));
            xmlWriter.WriteElementString("PORCIVA", "");
            xmlWriter.WriteElementString("OBSERVACION", ((IElementoDtm)preasiento.Referenciado(Contexto)).CrearLink(Contexto));
            xmlWriter.WriteElementString("PUNTEO", "0");
            xmlWriter.WriteElementString("NUMDOCUMENTO", preasiento.IdReferenciado.ToString().Right(7));
            xmlWriter.WriteElementString("EXPLOTACION", "");
            xmlWriter.WriteElementString("TIPOOPERMEMO", "");
            xmlWriter.WriteElementString("TIPOCOBRO_CODIGO", "");
            xmlWriter.WriteElementString("TIPOCOBRO_NOMBRE", "");
            xmlWriter.WriteElementString("TIPOCOBRO_ACUMULA", "");
            xmlWriter.WriteElementString("EJERMETALICO", "");
            xmlWriter.WriteEndElement();
        }

        private void ApunteDeDeudor(XmlWriter xmlWriter, PreasientoDtm preasiento)
        {
            var apunte = preasiento.Detalles<ApunteDeUnPreasientoDtm>(Contexto).First(apunte => apunte.Clase == enumClaseDeApunte.Cob_Deudor);

            xmlWriter.WriteStartElement("APUNTE");
            xmlWriter.WriteElementString("CODIGOCTA", CuentaDelTercero(apunte.Cuenta));
            xmlWriter.WriteElementString("IMPORTE", (-1 * apunte.Importe).Formatear(decimales: 6, alineacion: false, separadorDecimal: ","));
            xmlWriter.WriteElementString("PORCIVA", "");
            xmlWriter.WriteElementString("OBSERVACION", ((CobroDeFaeDtm)preasiento.Referenciado(Contexto)).Factura(Contexto).CrearLink(Contexto));
            xmlWriter.WriteElementString("PUNTEO", "0");
            xmlWriter.WriteElementString("NUMDOCUMENTO", preasiento.IdReferenciado.ToString().Right(7));
            xmlWriter.WriteElementString("EXPLOTACION", "");
            xmlWriter.WriteElementString("TIPOOPERMEMO", "");
            xmlWriter.WriteElementString("TIPOCOBRO_CODIGO", "");
            xmlWriter.WriteElementString("TIPOCOBRO_NOMBRE", "");
            xmlWriter.WriteElementString("TIPOCOBRO_ACUMULA", "");
            xmlWriter.WriteElementString("EJERMETALICO", "");
            xmlWriter.WriteEndElement();
        }

        private void ApunteDePago(XmlWriter xmlWriter, PreasientoDtm preasiento)
        {
            var apunte = preasiento.Detalles<ApunteDeUnPreasientoDtm>(Contexto).First(apunte => apunte.Clase == enumClaseDeApunte.Pag_Pago);
            xmlWriter.WriteStartElement("APUNTE");
            xmlWriter.WriteElementString("CODIGOCTA", CuentaDeNcs(apunte.Cuenta));
            xmlWriter.WriteElementString("IMPORTE", (-1 * apunte.Importe).Formatear(decimales: 6, alineacion: false, separadorDecimal: ","));
            xmlWriter.WriteElementString("PORCIVA", "");
            xmlWriter.WriteElementString("OBSERVACION", apunte.Concepto.Left(250));
            xmlWriter.WriteElementString("PUNTEO", "0");
            xmlWriter.WriteElementString("NUMDOCUMENTO", preasiento.IdReferenciado.ToString().Right(7));
            xmlWriter.WriteElementString("EXPLOTACION", "");
            xmlWriter.WriteElementString("TIPOOPERMEMO", "");
            xmlWriter.WriteElementString("TIPOCOBRO_CODIGO", "");
            xmlWriter.WriteElementString("TIPOCOBRO_NOMBRE", "");
            xmlWriter.WriteElementString("TIPOCOBRO_ACUMULA", "");
            xmlWriter.WriteElementString("EJERMETALICO", "");
            xmlWriter.WriteEndElement();
        }

        private void ApunteDeCobro(XmlWriter xmlWriter, PreasientoDtm preasiento)
        {
            var apunte = preasiento.Detalles<ApunteDeUnPreasientoDtm>(Contexto).First(apunte => apunte.Clase == enumClaseDeApunte.Cob_Cobro);
            xmlWriter.WriteStartElement("APUNTE");
            xmlWriter.WriteElementString("CODIGOCTA", CuentaDeNcs(apunte.Cuenta));
            xmlWriter.WriteElementString("IMPORTE", apunte.Importe.Formatear(decimales: 6, alineacion: false, separadorDecimal: ","));
            xmlWriter.WriteElementString("PORCIVA", "");
            xmlWriter.WriteElementString("OBSERVACION", apunte.Concepto.Left(250));
            xmlWriter.WriteElementString("PUNTEO", "0");
            xmlWriter.WriteElementString("NUMDOCUMENTO", preasiento.IdReferenciado.ToString().Right(7));
            xmlWriter.WriteElementString("EXPLOTACION", "");
            xmlWriter.WriteElementString("TIPOOPERMEMO", "");
            xmlWriter.WriteElementString("TIPOCOBRO_CODIGO", "");
            xmlWriter.WriteElementString("TIPOCOBRO_NOMBRE", "");
            xmlWriter.WriteElementString("TIPOCOBRO_ACUMULA", "");
            xmlWriter.WriteElementString("EJERMETALICO", "");
            xmlWriter.WriteEndElement();
        }

        private void AsientoDeFacturaEmitida(XmlWriter xmlWriter, PreasientoDtm preasiento)
        {
            WriteCabecera(xmlWriter, preasiento);
            var registroDeIva = WriteListaRegIvaRepercutido(xmlWriter, preasiento);
            WriteAsientoDeVenta(xmlWriter, registroDeIva, preasiento);
            WriteListaDeRetenciones(xmlWriter, registroDeIva, preasiento);
            WriteListaDirDiarioAdicional(xmlWriter, preasiento);
            WriteDirPlantilla(xmlWriter, preasiento);
        }

        private void AsientoDeFacturaRecibida(XmlWriter xmlWriter, PreasientoDtm preasiento)
        {
            WriteCabecera(xmlWriter, preasiento);
            var registroDeIva = WriteListaRegIvaSoportado(xmlWriter, preasiento);
            WriteAsientoDeCompra(xmlWriter, registroDeIva, preasiento);
            WriteListaDeRetenciones(xmlWriter, registroDeIva, preasiento);
            WriteListaDirDiarioAdicional(xmlWriter, preasiento);
            WriteDirPlantilla(xmlWriter, preasiento);
        }

        private void WriteListaDeRetenciones(XmlWriter xmlWriter, List<RegistroDeIva> registroDeIva, PreasientoDtm preasiento)
        {
            xmlWriter.WriteStartElement("LISTAREGRETENCION");
            var apuntes = preasiento.NegocioReferenciado == enumNegocio.FacturaRecibida
            ? preasiento.Detalles<ApunteDeUnPreasientoDtm>(Contexto).Where(apunte => apunte.Clase == enumClaseDeApunte.Far_Irpf)
            : preasiento.Detalles<ApunteDeUnPreasientoDtm>(Contexto).Where(apunte => apunte.Clase == enumClaseDeApunte.Fae_Irpf);
            foreach (var retencion in apuntes)
            {
                var importe = preasiento.NegocioReferenciado == enumNegocio.FacturaRecibida ? retencion.Importe : retencion.Importe * -1;
                var bi = preasiento.NegocioReferenciado == enumNegocio.FacturaRecibida ? retencion?.BaseDelImporte ?? 0 : retencion?.BaseDelImporte * -1 ?? 0;
                xmlWriter.WriteStartElement("REGRETENCION");
                xmlWriter.WriteElementString("PERCEPTOR", CuentaDelTercero(preasiento));
                xmlWriter.WriteElementString("IMPRETENCION", importe.Formatear(decimales: 6, alineacion: false, separadorDecimal: ","));
                xmlWriter.WriteElementString("IMPAPORTASEGSOC", "");
                xmlWriter.WriteElementString("CTAAPORTASEGSOC", "");
                xmlWriter.WriteElementString("CLVRETENCION", "");
                xmlWriter.WriteElementString("IMPDEVENGO", bi.Formatear(decimales: 6, alineacion: false, separadorDecimal: ","));
                xmlWriter.WriteElementString("CTARETENCION", CuentaDeNcs(retencion.Cuenta));
                xmlWriter.WriteEndElement();
            }
            xmlWriter.WriteEndElement();
        }

        private void WriteCabecera(XmlWriter xmlWriter, PreasientoDtm preasiento)
        {
            DateTime fechaContable = DateTime.Now;
            if (FechaContable is null)
            {
                fechaContable = preasiento.NegocioReferenciado == enumNegocio.Pago
                ? (DateTime)((PagoDtm)preasiento.Referenciado(Contexto)).PagadoEl
                : preasiento.NegocioReferenciado == enumNegocio.FacturaEmitida
                ? (DateTime)((FacturaEmtDtm)preasiento.Referenciado(Contexto)).FacturadaEl
                : preasiento.NegocioReferenciado == enumNegocio.Cobro
                ? (DateTime)((CobroDeFaeDtm)preasiento.Referenciado(Contexto)).CobradoEl
                : (DateTime)((FacturaRecDtm)preasiento.Referenciado(Contexto)).FacturadaEl;
            }
            else
            {
                fechaContable = (DateTime)FechaContable;
            }

            fechaContable = RespetarFechacontable ? fechaContable : ObtenerFechaContableParaElTrimestre(fechaContable);
            var fechaDeEmision = RespetarFechacontable ? FechaDeEmision(preasiento) : ObtenerFechaContableParaElTrimestre(FechaDeEmision(preasiento));

            string rectificado = CodigoAsientoRectificado(preasiento);

            xmlWriter.WriteStartElement("CABECERA");
            xmlWriter.WriteElementString("ORIGEN", preasiento.NegocioReferenciado == enumNegocio.FacturaEmitida || preasiento.NegocioReferenciado == enumNegocio.FacturaRecibida
                ? ltrNcsPlanContable.AsientoConIva
                : ltrNcsPlanContable.AsientoLibre);
            var codigo = (int)enumCodigoConcepto(preasiento);
            xmlWriter.WriteElementString("NUMORDEN", (++NumeroDeOrden).ToString());
            xmlWriter.WriteElementString("SERIEASIENTO", "");
            xmlWriter.WriteElementString("NUMEROASIENTO", preasiento.Id.ToString().PadLeft(6, '0'));
            xmlWriter.WriteElementString("FECHA", fechaContable.ToString("dd/MM/yyyy") + " 00:00:00");
            xmlWriter.WriteElementString("CODIGOCONCEPTO", codigo.ToString());
            xmlWriter.WriteElementString("NOMCODCONCEPTO", "");
            xmlWriter.WriteElementString("VALOR", "0");
            xmlWriter.WriteElementString("FECHAEMISION", fechaDeEmision.ToString("dd/MM/yyyy") + " 00:00:00");
            xmlWriter.WriteElementString("DOCORIGEN", rectificado);
            xmlWriter.WriteElementString("ANULADO", "False");
            xmlWriter.WriteElementString("TEXTOASIENTO", $"Preasiento: {preasiento.Referencia}");
            xmlWriter.WriteElementString("FECHAOPERACION", "");
            xmlWriter.WriteElementString("PLAZOPROVEEDORES", "");
            xmlWriter.WriteElementString("FEALBARAN", "");

            if ((preasiento.NegocioReferenciado == enumNegocio.FacturaEmitida && ((FacturaEmtDtm)preasiento.Referenciado(Contexto)).EsRectificativa) ||
                (preasiento.NegocioReferenciado == enumNegocio.FacturaRecibida && ((FacturaRecDtm)preasiento.Referenciado(Contexto)).EsRectificativa))
            {
                xmlWriter.WriteElementString("TIPOFACTURA", ltrNcsPlanContable.Rectificativa);
            }

            if (preasiento.NegocioReferenciado == enumNegocio.FacturaRecibida)
            {
                xmlWriter.WriteElementString("NUMIDENTFACTURA", ((FacturaRecDtm)preasiento.Referenciado(Contexto)).Numero);
            }

            xmlWriter.WriteElementString("IDREFCONTRAT", "");
            xmlWriter.WriteEndElement(); // CABECERA
        }

        private string CodigoAsientoRectificado(PreasientoDtm preasiento)
        {
            var rectificado = "";
            if (preasiento.NegocioReferenciado == enumNegocio.FacturaEmitida && ((FacturaEmtDtm)preasiento.Referenciado(Contexto)).EsRectificativa)
            {
                var rectificada = ((FacturaEmtDtm)preasiento.Referenciado(Contexto)).RectificaA(Contexto, errorSiNoHay: false);
                rectificado = CodigoAsientoRectificado(rectificada);
            }
            else if (preasiento.NegocioReferenciado == enumNegocio.FacturaRecibida && ((FacturaRecDtm)preasiento.Referenciado(Contexto)).EsRectificativa)
            {
                var rectificada = ((FacturaRecDtm)preasiento.Referenciado(Contexto)).Rectificada(Contexto, errorSiNoHay: false);
                rectificado = CodigoAsientoRectificado(rectificada);
            }

            return rectificado;
        }

        private string CodigoAsientoRectificado(IUsaPreasiento rectificada)
        {
            string rectificado = "*******";
            var idAnterior = rectificada?.IdPreasiento ?? null;
            if (idAnterior is not null)
            {
                var anterior = Contexto.SeleccionarPorId<PreasientoDtm>((int)idAnterior);
                if (anterior.EstaEnLaEtapa(enumEtapasDePreasiento.SPR_Etapa_Contabilizado))
                    rectificado = idAnterior.ToString().PadLeft(6, '0');
            }

            return rectificado;
        }

        private List<RegistroDeIva> WriteListaRegIvaRepercutido(XmlWriter xmlWriter, PreasientoDtm preasiento)
        {
            var registrosDeIva = new List<RegistroDeIva>();
            var cuentaCliente = CuentaDelTercero(preasiento);
            var esRectificativa = ((FacturaEmtDtm)preasiento.Referenciado(Contexto)).EsRectificativa;
            xmlWriter.WriteStartElement("LISTAREGIVA");
            var apuntes = preasiento.Detalles<ApunteDeUnPreasientoDtm>(Contexto).Where(apunte => apunte.Clase == enumClaseDeApunte.Fae_Ingreso);
            foreach (var apunte in apuntes)
            {
                var ivarep = Contexto.SeleccionarPorId<IvaRepercutidoDtm>(apunte.IdIva.Entero(), aplicarJoin: true);
                var tipoOperacion = TipoDeOperacion(preasiento, ivarep.Clase, ivarep.Exento);
                var porcentajeNcs = PorcentajeDeIvaRepNcs(ivarep, tipoOperacion);
                var ctaIngreso = CuentaDeNcs(apunte.Cuenta);
                var registrodeIva = new RegistroDeIva
                {
                    PorcentajeNcs = porcentajeNcs,
                    CuentaCliente = cuentaCliente,
                    CuentaIngreso = ctaIngreso,
                    Cuota = porcentajeNcs == enumNcsPorcentageIva.NoSujeto ? 0 : (-1 * apunte.IvaDelImporte).Decimal(),
                    Bi = apunte.Importe,
                    TipoNcs = tipoOperacion,
                    EsIvaSoportado = false,
                    EsIvaRepercutido = true,
                    CuentaDeIvaRepercutido = esRectificativa ? ltrNcsPlanContable.IvaDevolucionIngresos : ltrNcsPlanContable.IvaIngresos, // CuentaDeNcs(ivarep.Cuenta.Codigo),
                    EsElIvaIsp = false
                };
                WriteRegistroDeIvaRep(xmlWriter, registrodeIva);
                if (ivarep.Clase == enumClasesDeIvaRep.ISP)
                {
                    var ivaSop = Contexto.Set<IvaSoportadoDtm>().Include(x => x.Cuenta).FirstOrDefault(x => x.Clase == enumClasesDeIvaSop.ISP && x.Porcentaje == ivarep.Porcentaje);
                    if (ivaSop == null)
                        GestorDeErrores.Emitir($"Ha de definir el iva de la clase '{enumClasesDeIvaSop.ISP.Descripcion()}' y porcentaje '{ivaSop.Porcentaje}'");
                    registrodeIva.EsElIvaIsp = true;
                    registrodeIva.CuentaDeIvaSoportado = esRectificativa ? ltrNcsPlanContable.IvaDeDevolucionPagos : ltrNcsPlanContable.IvaDePagos; // CuentaDeNcs(ivaSop.Cuenta.Codigo);
                    WriteRegistroDeIvaSujetoPasivoFae(xmlWriter, registrodeIva);
                }
                registrosDeIva.Add(registrodeIva);
            }

            xmlWriter.WriteEndElement(); // LISTAREGIVA
            return registrosDeIva;
        }

        private List<RegistroDeIva> WriteListaRegIvaSoportado(XmlWriter xmlWriter, PreasientoDtm preasiento)
        {
            var registrosDeIva = new List<RegistroDeIva>();
            var cuentaProveedor = CuentaDelTercero(preasiento);
            var esRectificativa = ((FacturaRecDtm)preasiento.Referenciado(Contexto)).EsRectificativa;
            xmlWriter.WriteStartElement("LISTAREGIVA");
            var apuntes = preasiento.Detalles<ApunteDeUnPreasientoDtm>(Contexto).Where(apunte => apunte.Clase == enumClaseDeApunte.Far_Gasto);
            foreach (var apunte in apuntes)
            {
                var ivasop = Contexto.SeleccionarPorId<IvaSoportadoDtm>(apunte.IdIva.Entero(), aplicarJoin: true);
                var tipoOperacion = TipoDeOperacion(preasiento, ivasop.Clase, ivasop.Exento);
                enumNcsPorcentageIva porcentajeNcs = PorcentajeDeIvaSopNcs(ivasop, tipoOperacion);
                var ctaGasto = CuentaDeNcs(apunte.Cuenta);
                var registrodeIva = new RegistroDeIva
                {
                    PorcentajeNcs = porcentajeNcs,
                    CuentaProveedor = cuentaProveedor,
                    CuentaGasto = ctaGasto,
                    Cuota = CalcularCuota(tipoOperacion, porcentajeNcs, apunte.Importe, apunte.IvaDelImporte.Decimal()),
                    Bi = apunte.Importe,
                    TipoNcs = tipoOperacion,
                    EsIvaSoportado = true,
                    EsIvaRepercutido = false,
                    CuentaDeIvaSoportado = esRectificativa ? ltrNcsPlanContable.IvaDeDevolucionPagos : ltrNcsPlanContable.IvaDePagos, //CuentaDeNcs(ivasop.Cuenta.Codigo),
                    EsElIvaIsp = false
                };

                WriteRegistroDeIvaSop(xmlWriter, registrodeIva);
                if (ivasop.Clase == enumClasesDeIvaSop.ISP)
                {
                    var ivaRep = Contexto.Set<IvaRepercutidoDtm>().Include(x => x.Cuenta).FirstOrDefault(x => x.Clase == enumClasesDeIvaRep.ISP && x.Porcentaje == ivasop.Porcentaje);
                    if (ivaRep == null)
                        GestorDeErrores.Emitir($"Ha de definir el iva de la clase '{enumClasesDeIvaRep.ISP.Descripcion()}' y porcentaje '{ivasop.Porcentaje}'");
                    registrodeIva.EsElIvaIsp = true;
                    registrodeIva.CuentaDeIvaRepercutido = esRectificativa ? ltrNcsPlanContable.IvaDevolucionIngresos : ltrNcsPlanContable.IvaIngresos; // CuentaDeNcs(ivaRep.Cuenta.Codigo);
                    WriteRegistroDeIvaSujetoPasivoFar(xmlWriter, registrodeIva);
                }
                registrosDeIva.Add(registrodeIva);
            }

            xmlWriter.WriteEndElement(); // LISTAREGIVA
            return registrosDeIva;
        }

        private static void WriteRegistroDeIvaRep(XmlWriter xmlWriter, RegistroDeIva registroDeIvaNcs)
        {
            xmlWriter.WriteStartElement("REGIVA");
            xmlWriter.WriteElementString("PORCIVA", registroDeIvaNcs.PorcentajeNcs.Valor());
            xmlWriter.WriteElementString("CTADESTINATARIO", registroDeIvaNcs.CuentaCliente);
            xmlWriter.WriteElementString("CTACONTRAPARTIDA", registroDeIvaNcs.CuentaIngreso);
            xmlWriter.WriteElementString("CTADETALLE", registroDeIvaNcs.CuentaDeIvaRepercutido);
            xmlWriter.WriteElementString("CUOTA", Math.Abs(registroDeIvaNcs.Cuota).Formatear(decimales: 6, alineacion: false, separadorDecimal: ","));
            xmlWriter.WriteElementString("CUOTARECARGO", "0,000000");
            xmlWriter.WriteElementString("BASEIMPONIBLE", Math.Abs(registroDeIvaNcs.Bi).Formatear(decimales: 6, alineacion: false, separadorDecimal: ","));
            xmlWriter.WriteElementString("TIPOOPERACION", registroDeIvaNcs.TipoNcs.ToString());
            xmlWriter.WriteElementString("DTOPP", "0,000000");
            xmlWriter.WriteEndElement(); // REGIVA
        }

        private static void WriteRegistroDeIvaSujetoPasivoFae(XmlWriter xmlWriter, RegistroDeIva registroDeIvaNcs)
        {
            xmlWriter.WriteStartElement("REGIVA");
            xmlWriter.WriteElementString("PORCIVA", registroDeIvaNcs.PorcentajeNcs.Valor());
            xmlWriter.WriteElementString("CTADESTINATARIO", registroDeIvaNcs.CuentaCliente);
            xmlWriter.WriteElementString("CTACONTRAPARTIDA", registroDeIvaNcs.CuentaIngreso);
            xmlWriter.WriteElementString("CTADETALLE", registroDeIvaNcs.CuentaDeIvaSoportado);
            xmlWriter.WriteElementString("CUOTA", Math.Abs(registroDeIvaNcs.Cuota).Formatear(decimales: 6, alineacion: false, separadorDecimal: ","));
            xmlWriter.WriteElementString("CUOTARECARGO", "0,000000");
            xmlWriter.WriteElementString("BASEIMPONIBLE", Math.Abs(registroDeIvaNcs.Bi).Formatear(decimales: 6, alineacion: false, separadorDecimal: ","));
            xmlWriter.WriteElementString("TIPOOPERACION", registroDeIvaNcs.TipoNcs.ToString());
            xmlWriter.WriteElementString("DTOPP", "0,000000");
            xmlWriter.WriteEndElement(); // REGIVA
        }

        private static void WriteRegistroDeIvaSop(XmlWriter xmlWriter, RegistroDeIva registroDeIvaNcs)
        {
            xmlWriter.WriteStartElement("REGIVA");
            xmlWriter.WriteElementString("PORCIVA", registroDeIvaNcs.PorcentajeNcs.Valor());
            xmlWriter.WriteElementString("CTADESTINATARIO", registroDeIvaNcs.CuentaProveedor);
            xmlWriter.WriteElementString("CTACONTRAPARTIDA", registroDeIvaNcs.CuentaGasto);
            xmlWriter.WriteElementString("CTADETALLE", registroDeIvaNcs.CuentaDeIvaSoportado);
            xmlWriter.WriteElementString("CUOTA", Math.Abs(registroDeIvaNcs.Cuota).Formatear(decimales: 6, alineacion: false, separadorDecimal: ","));
            xmlWriter.WriteElementString("CUOTARECARGO", "0,000000");
            xmlWriter.WriteElementString("BASEIMPONIBLE", Math.Abs(registroDeIvaNcs.Bi).Formatear(decimales: 6, alineacion: false, separadorDecimal: ","));
            xmlWriter.WriteElementString("TIPOOPERACION", registroDeIvaNcs.TipoNcs.ToString());
            xmlWriter.WriteElementString("DTOPP", "0,000000");
            xmlWriter.WriteEndElement(); // REGIVA
        }

        private static void WriteRegistroDeIvaSujetoPasivoFar(XmlWriter xmlWriter, RegistroDeIva registroDeIvaNcs)
        {
            xmlWriter.WriteStartElement("REGIVA");
            xmlWriter.WriteElementString("PORCIVA", registroDeIvaNcs.PorcentajeNcs.Valor());
            xmlWriter.WriteElementString("CTADESTINATARIO", registroDeIvaNcs.CuentaProveedor);
            xmlWriter.WriteElementString("CTACONTRAPARTIDA", registroDeIvaNcs.CuentaGasto);
            xmlWriter.WriteElementString("CTADETALLE", registroDeIvaNcs.CuentaDeIvaRepercutido);
            xmlWriter.WriteElementString("CUOTA", Math.Abs(registroDeIvaNcs.Cuota).Formatear(decimales: 6, alineacion: false, separadorDecimal: ","));
            xmlWriter.WriteElementString("CUOTARECARGO", "0,000000");
            xmlWriter.WriteElementString("BASEIMPONIBLE", Math.Abs(registroDeIvaNcs.Bi).Formatear(decimales: 6, alineacion: false, separadorDecimal: ","));
            xmlWriter.WriteElementString("TIPOOPERACION", registroDeIvaNcs.TipoNcs.ToString());
            xmlWriter.WriteElementString("DTOPP", "0,000000");
            xmlWriter.WriteEndElement(); // REGIVA
        }

        private void WriteAsientoDeCompra(XmlWriter xmlWriter, List<RegistroDeIva> registroDeIva, PreasientoDtm preasiento)
        {
            var codiconcepto = enumCodigoConcepto(preasiento);

            xmlWriter.WriteStartElement("LISTAAPUNTES");
            ApunteDelTercero(xmlWriter, registroDeIva, preasiento, codiconcepto);
            ApuntesDeGastoIngreso(xmlWriter, preasiento, codiconcepto);
            ApuntesDeInverionSujetoPasivo(xmlWriter, registroDeIva, preasiento);
            ApuntesDeIva(xmlWriter, registroDeIva, preasiento);
            ApuntesDeRetenciones(xmlWriter, registroDeIva, preasiento);
            xmlWriter.WriteEndElement(); // LISTAAPUNTES
        }

        private void WriteAsientoDeVenta(XmlWriter xmlWriter, List<RegistroDeIva> registroDeIva, PreasientoDtm preasiento)
        {
            var codiconcepto = enumCodigoConcepto(preasiento);

            xmlWriter.WriteStartElement("LISTAAPUNTES");
            ApunteDelTercero(xmlWriter, registroDeIva, preasiento, codiconcepto);
            ApuntesDeGastoIngreso(xmlWriter, preasiento, codiconcepto);
            ApuntesDeInverionSujetoPasivo(xmlWriter, registroDeIva, preasiento);
            ApuntesDeIva(xmlWriter, registroDeIva, preasiento);
            ApuntesDeRetenciones(xmlWriter, registroDeIva, preasiento);
            xmlWriter.WriteEndElement(); // LISTAAPUNTES
        }

        private void ApuntesDeInverionSujetoPasivo(XmlWriter xmlWriter, List<RegistroDeIva> registroDeIvaNcs, PreasientoDtm preasiento)
        {
            foreach (var ivaIsp in registroDeIvaNcs.Where(x => x.EsElIvaIsp && x.Cuota != 0))
            {
                var cuenta = preasiento.NegocioReferenciado == enumNegocio.FacturaRecibida ? ivaIsp.CuentaDeIvaRepercutido : ivaIsp.CuentaDeIvaSoportado;
                var importe = preasiento.NegocioReferenciado == enumNegocio.FacturaRecibida ? -1 * ivaIsp.Cuota : ivaIsp.Cuota;
                xmlWriter.WriteStartElement("APUNTE");
                xmlWriter.WriteElementString("CODIGOCTA", cuenta);
                xmlWriter.WriteElementString("IMPORTE", importe.Formatear(decimales: 6, alineacion: false, separadorDecimal: ","));
                xmlWriter.WriteElementString("PORCIVA", ivaIsp.PorcentajeNcs.Valor());
                xmlWriter.WriteElementString("NOMBRECL", "");
                xmlWriter.WriteElementString("OBSERVACION", preasiento.Nombre.Left(250));
                xmlWriter.WriteElementString("PUNTEO", "0");
                xmlWriter.WriteElementString("NUMDOCUMENTO", preasiento.IdReferenciado.ToString().Right(7));
                xmlWriter.WriteElementString("EXPLOTACION", "");
                xmlWriter.WriteElementString("TIPOOPERMEMO", "");
                xmlWriter.WriteElementString("TIPOCOBRO_CODIGO", "");
                xmlWriter.WriteElementString("TIPOCOBRO_NOMBRE", "");
                xmlWriter.WriteElementString("TIPOCOBRO_ACUMULA", "");
                xmlWriter.WriteElementString("EJERMETALICO", "");
                xmlWriter.WriteEndElement(); // APUNTE
            }
        }

        private void ApuntesDeRetenciones(XmlWriter xmlWriter, List<RegistroDeIva> registroDeIva, PreasientoDtm preasiento)
        {
            var apuntes = preasiento.NegocioReferenciado == enumNegocio.FacturaRecibida
            ? preasiento.Detalles<ApunteDeUnPreasientoDtm>(Contexto).Where(apunte => apunte.Clase == enumClaseDeApunte.Far_Irpf)
            : preasiento.Detalles<ApunteDeUnPreasientoDtm>(Contexto).Where(apunte => apunte.Clase == enumClaseDeApunte.Fae_Irpf);

            foreach (var retencion in apuntes)
            {
                var importe = preasiento.NegocioReferenciado == enumNegocio.FacturaRecibida ? -1 * retencion.Importe : retencion.Importe;
                xmlWriter.WriteStartElement("APUNTE");
                xmlWriter.WriteElementString("CODIGOCTA", CuentaDeNcs(retencion.Cuenta));
                xmlWriter.WriteElementString("IMPORTE", importe.Formatear(decimales: 6, alineacion: false, separadorDecimal: ","));
                xmlWriter.WriteElementString("PORCIVA", "");
                xmlWriter.WriteElementString("NOMBRECL", "");
                xmlWriter.WriteElementString("OBSERVACION", preasiento.Nombre.Left(250));
                xmlWriter.WriteElementString("PUNTEO", "0");
                xmlWriter.WriteElementString("NUMDOCUMENTO", preasiento.IdReferenciado.ToString().Right(7));
                xmlWriter.WriteElementString("EXPLOTACION", "");
                xmlWriter.WriteElementString("TIPOOPERMEMO", "");
                xmlWriter.WriteElementString("TIPOCOBRO_CODIGO", "");
                xmlWriter.WriteElementString("TIPOCOBRO_NOMBRE", "");
                xmlWriter.WriteElementString("TIPOCOBRO_ACUMULA", "");
                xmlWriter.WriteElementString("EJERMETALICO", "");
                xmlWriter.WriteEndElement(); // APUNTE
            }
        }

        private void ApuntesDeIva(XmlWriter xmlWriter, List<RegistroDeIva> registroDeIvaNcs, PreasientoDtm preasiento)
        {
            bool esIvaSoportado = preasiento.NegocioReferenciado == enumNegocio.FacturaRecibida;

            var gruposIvalst = registroDeIvaNcs
                .Where(x => (esIvaSoportado ? x.EsIvaSoportado : x.EsIvaRepercutido) && x.PorcentajeNcs != enumNcsPorcentageIva.NoSujeto && x.Cuota != 0)
                .GroupBy(x => new
                {
                    CuentaDeIva = esIvaSoportado ? x.CuentaDeIvaSoportado : x.CuentaDeIvaRepercutido,
                    x.PorcentajeNcs,
                    preasiento.Nombre,
                    preasiento.IdReferenciado
                }).ToList();

            var gruposIva = gruposIvalst.Select(g => new
            {
                g.Key.CuentaDeIva,
                g.Key.PorcentajeNcs,
                g.Key.Nombre,
                g.Key.IdReferenciado,
                CuotaTotal = g.Sum(x => x.Cuota)
            }).ToList();

            foreach (var grupo in gruposIva)
            {
                xmlWriter.WriteStartElement("APUNTE");
                xmlWriter.WriteElementString("CODIGOCTA", grupo.CuentaDeIva);
                xmlWriter.WriteElementString("IMPORTE", grupo.CuotaTotal.Formatear(decimales: 6, alineacion: false, separadorDecimal: ","));
                xmlWriter.WriteElementString("PORCIVA", grupo.PorcentajeNcs.Valor());
                xmlWriter.WriteElementString("NOMBRECL", "");
                xmlWriter.WriteElementString("OBSERVACION", grupo.Nombre.Left(250));
                xmlWriter.WriteElementString("PUNTEO", "0");
                xmlWriter.WriteElementString("NUMDOCUMENTO", grupo.IdReferenciado.ToString().Right(7));
                xmlWriter.WriteElementString("EXPLOTACION", "");
                xmlWriter.WriteElementString("TIPOOPERMEMO", "");
                xmlWriter.WriteElementString("TIPOCOBRO_CODIGO", "");
                xmlWriter.WriteElementString("TIPOCOBRO_NOMBRE", "");
                xmlWriter.WriteElementString("TIPOCOBRO_ACUMULA", "");
                xmlWriter.WriteElementString("EJERMETALICO", "");
                xmlWriter.WriteEndElement(); // APUNTE
            }
        }

        private void ApuntesDeGastoIngreso(XmlWriter xmlWriter, PreasientoDtm preasiento, enumNcsCodigoConcepto codiconcepto)
        {
            var apuntes = preasiento.NegocioReferenciado == enumNegocio.Pago
            ? preasiento.Detalles<ApunteDeUnPreasientoDtm>(Contexto).Where(apunte => apunte.Clase == enumClaseDeApunte.Pag_Gasto)
            : preasiento.NegocioReferenciado == enumNegocio.FacturaRecibida
            ? preasiento.Detalles<ApunteDeUnPreasientoDtm>(Contexto).Where(apunte => apunte.Clase == enumClaseDeApunte.Far_Gasto)
            : preasiento.Detalles<ApunteDeUnPreasientoDtm>(Contexto).Where(apunte => apunte.Clase == enumClaseDeApunte.Fae_Ingreso);
            foreach (var apunte in apuntes)
            {
                //la rectificación de facturas de pago, no la toco, ya que yo la tengo en negativo y como va al haber y en ncs para meter en el haber ha de ir en negativo
                //la factura recibida, no la toco, ya que yo la tengo en positivo y como va al debe y en ncs para meter en el debe ha de ir en positivo
                //el gasto de un pago no lo modifico  ya que va al debe y al tenerlo en positivo, en el debe de ncs todo en positivo    
                var importe = apunte.Importe;

                //Si es una emisión de factura que yo tengo en positivo, un pago de factura que tengo en positivo, Va al haber por eso se cambia el signo
                //Si tengo una rectificativa emitida, la tengo en negativo y va al debe, cambio el signo
                if (codiconcepto == enumNcsCodigoConcepto.FacturasIngresos || codiconcepto == enumNcsCodigoConcepto.RectificacionIngresos)
                    importe = -1 * apunte.Importe;

                xmlWriter.WriteStartElement("APUNTE");
                xmlWriter.WriteElementString("CODIGOCTA", CuentaDeNcs(apunte.Cuenta));
                xmlWriter.WriteElementString("IMPORTE", importe.Formatear(decimales: 6, alineacion: false, separadorDecimal: ","));
                xmlWriter.WriteElementString("PORCIVA", "");
                xmlWriter.WriteElementString("NOMBRECL", "");
                xmlWriter.WriteElementString("OBSERVACION", apunte.Concepto.Left(250));
                xmlWriter.WriteElementString("PUNTEO", "0");
                xmlWriter.WriteElementString("NUMDOCUMENTO", preasiento.IdReferenciado.ToString().Right(7));
                xmlWriter.WriteElementString("EXPLOTACION", "");
                xmlWriter.WriteElementString("TIPOOPERMEMO", "");
                xmlWriter.WriteElementString("TIPOCOBRO_CODIGO", "");
                xmlWriter.WriteElementString("TIPOCOBRO_NOMBRE", "");
                xmlWriter.WriteElementString("TIPOCOBRO_ACUMULA", "");
                xmlWriter.WriteElementString("EJERMETALICO", "");
                xmlWriter.WriteEndElement(); // APUNTE
            }
        }

        private void ApunteDelTercero(XmlWriter xmlWriter, List<RegistroDeIva> registrodeIva, PreasientoDtm preasiento, enumNcsCodigoConcepto codiconcepto)
        {
            var apuntesConRetencion = preasiento.NegocioReferenciado == enumNegocio.FacturaRecibida
                ? preasiento.Detalles<ApunteDeUnPreasientoDtm>(Contexto).Where(apunte => apunte.Clase == enumClaseDeApunte.Far_Irpf)
                : preasiento.Detalles<ApunteDeUnPreasientoDtm>(Contexto).Where(apunte => apunte.Clase == enumClaseDeApunte.Fae_Irpf);

            decimal importeRetencion = apuntesConRetencion.Sum(r => r.Importe);
            if (preasiento.NegocioReferenciado == enumNegocio.FacturaRecibida)
                importeRetencion = -1 * importeRetencion;

            // Sumar todas las cuotas para calcular la proporción
            decimal sumaCuotas = registrodeIva.Sum(r => Math.Abs(r.EsElIvaIsp ? 0 : r.Cuota));

            foreach (var registro in registrodeIva)
            {
                // Calcular el importe base
                var importeBase = codiconcepto == enumNcsCodigoConcepto.RectificacionPagos ||
                                  codiconcepto == enumNcsCodigoConcepto.FacturasIngresos ||
                                  codiconcepto == enumNcsCodigoConcepto.PagoFacturasProveedores
                    ? Math.Abs(registro.Bi) + Math.Abs(registro.EsElIvaIsp ? 0 : registro.Cuota)
                    : -1 * (Math.Abs(registro.Bi) + Math.Abs(registro.EsElIvaIsp ? 0 : registro.Cuota));

                // Calcular la parte proporcional de la retención para este registro
                decimal cuotaActual = Math.Abs(registro.EsElIvaIsp ? 0 : registro.Cuota);
                decimal proporcionRetencion = sumaCuotas == 0 ? 0 : (cuotaActual / sumaCuotas);
                decimal retencionProporcional = importeRetencion * proporcionRetencion;

                // Restar la retención proporcional al importe base
                var importe = importeBase - retencionProporcional;

                var cuentaTercero = preasiento.NegocioReferenciado == enumNegocio.FacturaRecibida ? registro.CuentaProveedor : registro.CuentaCliente;

                xmlWriter.WriteStartElement("APUNTE");
                xmlWriter.WriteElementString("CODIGOCTA", cuentaTercero);
                xmlWriter.WriteElementString("IMPORTE", importe.Formatear(decimales: 6, alineacion: false, separadorDecimal: ","));
                xmlWriter.WriteElementString("PORCIVA", registro.PorcentajeNcs.Valor());
                xmlWriter.WriteElementString("NOMBRECL", "");
                xmlWriter.WriteElementString("OBSERVACION", ((IElementoDtm)preasiento.Referenciado(Contexto)).CrearLink(Contexto));
                xmlWriter.WriteElementString("PUNTEO", "0");
                xmlWriter.WriteElementString("NUMDOCUMENTO", preasiento.IdReferenciado.ToString().Right(7));
                xmlWriter.WriteElementString("EXPLOTACION", "");
                xmlWriter.WriteElementString("TIPOOPERMEMO", "");
                xmlWriter.WriteElementString("TIPOCOBRO_CODIGO", "");
                xmlWriter.WriteElementString("TIPOCOBRO_NOMBRE", "");
                xmlWriter.WriteElementString("TIPOCOBRO_ACUMULA", "");
                xmlWriter.WriteElementString("EJERMETALICO", "");
                xmlWriter.WriteEndElement(); // APUNTE
            }
        }

        private void WriteListaDirDiarioAdicional(XmlWriter xmlWriter, PreasientoDtm preasiento)
        {
            var factura = Contexto.Set<FacturaRecDtm>().Find(preasiento.IdReferenciado);
            var apunte = preasiento.NegocioReferenciado == enumNegocio.FacturaRecibida
                ? preasiento.Detalles<ApunteDeUnPreasientoDtm>(Contexto).First(apunte => apunte.Clase == enumClaseDeApunte.Far_Proveedor)
                : preasiento.Detalles<ApunteDeUnPreasientoDtm>(Contexto).First(apunte => apunte.Clase == enumClaseDeApunte.Fae_Cliente);

            xmlWriter.WriteStartElement("LISTADIRDIARIOADICIONAL");
            xmlWriter.WriteStartElement("DIRDIARIOADICIONAL");
            xmlWriter.WriteElementString("CTADEVENGO", CuentaDeNcs(apunte.Cuenta));
            xmlWriter.WriteElementString("IMPPRONTOPAGO", "0,000000");
            xmlWriter.WriteElementString("INCL_IMP_TOTAL", "0");
            xmlWriter.WriteEndElement(); // DIRDIARIOADICIONAL
            xmlWriter.WriteEndElement(); // LISTADIRDIARIOADICIONAL
        }

        private void WriteDirPlantilla(XmlWriter xmlWriter, PreasientoDtm preasiento)
        {
            string tipoPlantilla = null;
            if (preasiento.NegocioReferenciado == enumNegocio.FacturaRecibida)
            {
                if (((FacturaRecDtm)preasiento.Referenciado(Contexto)).EsRectificativa)
                    tipoPlantilla = ltrNcsPlanContable.DevolucionCompra;
                else
                    tipoPlantilla = ltrNcsPlanContable.Compra;
            }
            else
            {
                if (((FacturaEmtDtm)preasiento.Referenciado(Contexto)).EsRectificativa)
                    tipoPlantilla = ltrNcsPlanContable.DevolucionVenta;
                else tipoPlantilla = ltrNcsPlanContable.Venta;
            }

            xmlWriter.WriteStartElement("DIRPLANTILLA");
            xmlWriter.WriteElementString("TIPO_PLANTILLA", tipoPlantilla);
            xmlWriter.WriteElementString("ASIENTO_GLOBALIZADO", "0");
            xmlWriter.WriteElementString("CTA_GLOBALIZADO", "");
            xmlWriter.WriteElementString("INCL_IMP_TOTAL", "0");
            xmlWriter.WriteElementString("INCL_COBRO", "0");
            xmlWriter.WriteElementString("IMP_COBRO", "0,000000");
            xmlWriter.WriteElementString("CTA_CONTRAP_COBRO", "");
            xmlWriter.WriteElementString("IMP_SUPLIDO", "0,000000");
            xmlWriter.WriteElementString("CTA_SUPLIDOS", "");
            xmlWriter.WriteElementString("IMP_SEGSOCIAL_EMPR", "0,000000");
            xmlWriter.WriteElementString("CTA_SEGSOCIAL_EMPR", "");
            xmlWriter.WriteElementString("NUMASTOSPLANTILLAACTUAL", "1");
            xmlWriter.WriteElementString("CTARECARGO", "");
            xmlWriter.WriteElementString("FECOBRO", "");
            xmlWriter.WriteEndElement(); // DIRPLANTILLA
        }

        private enumNcsCodigoConcepto enumCodigoConcepto(PreasientoDtm preasiento)
        {
            var negocio = preasiento.NegocioReferenciado;

            if (negocio == enumNegocio.FacturaRecibida)
            {
                return ((FacturaRecDtm)preasiento.Referenciado(Contexto)).EsRectificativa
                    ? enumNcsCodigoConcepto.RectificacionPagos
                    : enumNcsCodigoConcepto.FacturasPagos;
            }
            if (negocio == enumNegocio.FacturaEmitida)
            {
                return ((FacturaEmtDtm)preasiento.Referenciado(Contexto)).EsRectificativa
                    ? enumNcsCodigoConcepto.RectificacionIngresos
                    : enumNcsCodigoConcepto.FacturasIngresos;
            }
            if (negocio == enumNegocio.Pago)
            {
                return ((PagoDtm)preasiento.Referenciado(Contexto)).IdNaturaleza is null ? enumNcsCodigoConcepto.PagoFacturasProveedores : enumNcsCodigoConcepto.PlanCuentasMayor;
            }

            if (negocio == enumNegocio.Cobro)
                return enumNcsCodigoConcepto.CobrosFacturasClientes;

            throw new Exception($"Ha de indicar el código del concepto asentado en la contabilidad de Ncs para el negocio '{negocio.Singular()}'");
        }

        private DateTime FechaDeEmision(PreasientoDtm preasiento)
        {
            switch (preasiento.NegocioReferenciado)
            {
                case enumNegocio.FacturaEmitida:
                    return ((FacturaEmtDtm)preasiento.Referenciado(Contexto)).FacturadaEl.Fecha();
                case enumNegocio.RemesaFae:
                    break;
                case enumNegocio.Pago:
                    return ((PagoDtm)preasiento.Referenciado(Contexto)).PagadoEl.Fecha();
                case enumNegocio.RemesaPag:
                    break;
                case enumNegocio.FacturaRecibida:
                    return ((FacturaRecDtm)preasiento.Referenciado(Contexto)).FacturadaEl;
                case enumNegocio.Cobro:
                    return ((CobroDeFaeDtm)preasiento.Referenciado(Contexto)).CobradoEl;
            }
            throw new Exception($"Falta implementar cómo obtener la 'Fecha De Emision' para '{preasiento.NegocioReferenciado.Singular()}'");
        }

        private enumNcsTipoOperacion TipoDeOperacion(PreasientoDtm preasiento, enumClasesDeIvaSop claseDeIva, bool esExento)
        {
            if (claseDeIva == enumClasesDeIvaSop.ISP)
                return enumNcsTipoOperacion.IV17;

            var factura = (FacturaRecDtm)preasiento.Referenciado(Contexto);
            DireccionDto direccionFiscal = factura.DireccionFiscal(Contexto, errorSiNoHay: false);

            if (direccionFiscal is not null && direccionFiscal.IntraComunitaria)
                return preasiento.EsMaterial(Contexto)
                    ? enumNcsTipoOperacion.IV02
                    : enumNcsTipoOperacion.IV20;

            if (direccionFiscal is not null && direccionFiscal.ExtraComunitaria)
                return enumNcsTipoOperacion.IV03;

            if (claseDeIva == enumClasesDeIvaSop.NSJ)
                return enumNcsTipoOperacion.IV01;

            if (esExento)
                return enumNcsTipoOperacion.IV01;

            return enumNcsTipoOperacion.IV01;
        }

        private enumNcsTipoOperacion TipoDeOperacion(PreasientoDtm preasiento, enumClasesDeIvaRep claseDeIva, bool esExento)
        {
            if (claseDeIva == enumClasesDeIvaRep.ISP)
                return enumNcsTipoOperacion.IV31;

            DireccionDto direccionFiscal = preasiento.NegocioReferenciado == enumNegocio.FacturaRecibida
            ? ((FacturaRecDtm)preasiento.Referenciado(Contexto)).DireccionFiscal(Contexto, errorSiNoHay: false)
            : ((FacturaEmtDtm)preasiento.Referenciado(Contexto)).DireccionFiscal(Contexto);

            if (direccionFiscal is not null && direccionFiscal.IntraComunitaria)
                return preasiento.EsMaterial(Contexto)
                    ? enumNcsTipoOperacion.IV04
                    : enumNcsTipoOperacion.IV19;

            if (direccionFiscal is not null && direccionFiscal.ExtraComunitaria)
                return enumNcsTipoOperacion.IV05;

            if (claseDeIva == enumClasesDeIvaRep.NSJ)
                return enumNcsTipoOperacion.IV01;

            if (esExento)
                return enumNcsTipoOperacion.IV01;

            return enumNcsTipoOperacion.IV01;
        }

        private string CuentaDelTercero(PreasientoDtm preasiento)
        {
            var posicionDelTercero = preasiento.NegocioReferenciado == enumNegocio.FacturaRecibida
             ? preasiento.Detalles<ApunteDeUnPreasientoDtm>(Contexto)
                .Where(apunte => apunte.Posicion == enumPosicionContable.Haber && apunte.Clase == enumClaseDeApunte.Far_Proveedor)
                .First()
             : preasiento.Detalles<ApunteDeUnPreasientoDtm>(Contexto)
                .Where(apunte => apunte.Posicion == enumPosicionContable.Debe && apunte.Clase == enumClaseDeApunte.Fae_Cliente)
                .First();
            return CuentaDelTercero(posicionDelTercero.Cuenta);
        }

        private static string CuentaDelTercero(string cuenta)
        {
            string cuentaAjustada = "";
            // Insertar el cero antes del penúltimo dígito
            string parteInicial = cuenta.Substring(0, cuenta.Length - 4);
            string parteFinal = cuenta.Substring(cuenta.Length - 4, 4);
            return cuentaAjustada = parteInicial + "0" + parteFinal;

        }



        //475101 --> 47500000101
        private static string CuentaDeNcs(string cuenta)
        {
            const int longitudObjetivo = 11;

            // Buscar el último cero desde el final
            int ultimoCero = cuenta.LastIndexOf('0');

            string parteInicial, parteFinal;
            int cerosNecesarios;

            if (ultimoCero != -1)
            {
                // Caso con ceros: dividir en el último cero
                parteInicial = cuenta.Substring(0, ultimoCero + 1);
                parteFinal = cuenta.Substring(ultimoCero + 1);
                cerosNecesarios = longitudObjetivo - (parteInicial.Length + parteFinal.Length);
            }
            else
            {
                // Caso sin ceros: toda la cuenta + 5 ceros
                parteInicial = cuenta;
                parteFinal = "";
                cerosNecesarios = 5; // Añadir exactamente 5 ceros
            }

            // Construir la nueva cuenta
            string cuentaAjustada = parteInicial +
                                  new string('0', cerosNecesarios) +
                                  parteFinal;

            // Asegurar longitud exacta de 11 dígitos
            return cuentaAjustada.PadRight(longitudObjetivo, '0')
                                .Substring(0, longitudObjetivo);
        }


        private decimal CalcularCuota(enumNcsTipoOperacion tipoOperacion, enumNcsPorcentageIva porcentajeNcs, decimal importe, decimal cuota)
        {
            if (tipoOperacion != enumNcsTipoOperacion.IV02 && tipoOperacion != enumNcsTipoOperacion.IV17)
                return cuota;

            //por ser Inversión sujero pasivo o compra extracomunitaria
            return importe * 21 / 100;
        }


        public static DateTime ObtenerFechaContableParaElTrimestre(DateTime fechaContable)
        {
            DateTime hoy = DateTime.Today;

            // 1. Verificar si las fechas están en el mismo año. Si no, no se aplica la regla de 'i+x'.
            //ejemplo: si viene una fecha contable del año 2024 la paso a uno de enero del 2025
            if (fechaContable.Year + 1 == hoy.Year)
            {
                fechaContable = new DateTime(hoy.Year, 1, 1);
            }

            if (fechaContable.Year == hoy.Year)
            {
                // 2. Determinar el número de trimestre (1 a 4) para ambas fechas.
                int trimestreContable = (fechaContable.Month - 1) / 3 + 1;
                int trimestreActual = (hoy.Month - 1) / 3 + 1;

                // 3. Determinar el primer día del trimestre actual.
                // Meses de inicio de trimestre: 1, 4, 7, 10
                int mesInicioTrimestreActual = 3 * trimestreActual - 2;
                DateTime primerDiaTrimestreActual = new DateTime(hoy.Year, mesInicioTrimestreActual, 1);

                // 4. Aplicar la lógica: Si la fecha de entrada (i) está en un trimestre anterior
                // al trimestre actual (i + x), devolvemos el primer día del trimestre actual.
                // La condición (trimestreActual > trimestreContable) ya cubre i+1, i+2, i+3.
                if (trimestreActual > trimestreContable)
                {
                    return primerDiaTrimestreActual;
                }

                // 5. Si no se cumple la condición (i=i, i>i), devolvemos la fecha original.
                return fechaContable;
            }

            throw Excepciones.Emitir($"La fecha contable '{fechaContable.ToString("dd-MM.yyyy")}' no puede anterior a {hoy.Year - 1}, ni mayor al año {hoy.Year + 1}.");
        }


    }

}

