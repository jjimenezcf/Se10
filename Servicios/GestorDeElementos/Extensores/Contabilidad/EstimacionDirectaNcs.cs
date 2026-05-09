using Gestor.Errores;
using ServicioDeDatos;
using ServicioDeDatos.Callejero;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Gastos;
using ServicioDeDatos.MaestrosTecnico;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.Ventas;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Utilidades;

namespace GestorDeElementos.Extensores.Contabilidad
{
    static class ltrEd
    {
        internal const string OrigenDeDatos = "0";
    }
    static class ltrEdTipoDocumento
    {
        internal const string Nif = "1";
        internal const string Pasaporte = "2";
        internal const string Nie = "6";
        internal const string Cif = "9";
        internal const string Comunitario = "10";
        internal const string Otro = "11";
    }

    static class ltrEdTipoConceptoDeIngresos
    {
        internal const string IngresosExplotacion = "1";
        internal const string OtrosIngresos = "2";
        internal const string SubvencionesCorrientes = "3";
        internal const string SubvencionesDeCapital = "4";
        internal const string Indemnizaciones = "5";
        internal const string Autoconsumo = "6";
        internal const string NoResidentes = "7";
        internal const string IngresosNoComputablesFiscalmente = "8";
        internal const string ProvisionDeFondos = "9";
        internal const string Suplidos = "10";
    }

    public static class ltrEdTipoConceptoDeGasto
    {
        internal const string SueldosYSalarios = "1";
        internal const string Compras = "2";
        internal const string SeguridadSocial = "3";
        internal const string OtrosGastosDePersonal = "4";
        internal const string ArrendamientosYCanones = "5";
        internal const string ReparacionesYConservacion = "6";
        internal const string ServiciosProfesionalesIndependientes = "7";
        internal const string Suministros = "8";
        internal const string OtrosServiciosExteriores = "9";
        internal const string PrimasDeSeguro = "10";
        internal const string TributosFiscalmenteDeducibles = "11";
        internal const string GastosFinancieros = "12";
        internal const string Amortizaciones = "13";
        internal const string PerdidasPorDeterioro = "14";
        internal const string IncentivosAlMecenazgoConvenios = "15";
        internal const string IncentivosAlMecenazgoGastos = "16";
        internal const string OtrosConceptosFiscalmenteDeducibles = "17";
        internal const string SeguroDeEnfermedad = "18";
        internal const string Provisiones = "19";
        internal const string GastosNoDeduciblesFiscalmente = "20";
    }

    public class EstimacionDirectaNcs : GeneradorBaseNcs
    {

        public bool HayTerceros { get; protected set; } = true;


        string _Actividad;
        public EstimacionDirectaNcs(ContextoSe contexto, SociedadDtm sociedad, int ejercicio, string actividad)
        : base(contexto, sociedad, ejercicio)
        {
            _Actividad = actividad;

            var codigoEmpresa = Sociedad.PlanContable().IdPlanContable.Entero() > 0 ? Sociedad.PlanContable().IdPlanContable.Entero().ToString().PadLeft(5, '0') : Sociedad.PlanContable().IdPlanContable;
            var actividadEmpresa = _Actividad.Entero() > 0 ? _Actividad.Entero().ToString().PadLeft(2, '0') : _Actividad;

            DiarioXml = Path.Combine(CacheDeVariable.Cfg_RutaDeDescarga, $"DiarioEOS_{codigoEmpresa}_{Ejercicio}_{actividadEmpresa}.{enumExtensiones.xml}");
            TercerosXml = ApiDeArchivos.ObtenerNombreUnico(Path.Combine(CacheDeVariable.Cfg_RutaDeDescarga, $"Terceros{codigoEmpresa}.{enumExtensiones.xml}"));
        }

        public void GenerarTerceros(List<ProveedorDtm> proveedores, List<ClienteDtm> clientes)
        {
            if (proveedores.Count == 0 && clientes.Count == 0)
                HayTerceros = false;

            using (var fileStream = new FileStream(TercerosXml, FileMode.Create))
            using (var xmlWriter = XmlWriter.Create(fileStream, new XmlWriterSettings { Indent = true }))
            {
                xmlWriter.WriteStartDocument();
                xmlWriter.WriteStartElement("Terceros");
                xmlWriter.WriteAttributeString("xmlns", "xsi", null, _xsi);
                xmlWriter.WriteAttributeString("xmlns", "xsd", null, _xsd);

                xmlWriter.WriteElementString("ORIGEN_DATOS", ltrEd.OrigenDeDatos);

                foreach (ITerceroContable tercero in proveedores)
                {
                    WriteTercero(xmlWriter, enumNegocio.Proveedor, tercero);
                }
                foreach (ITerceroContable tercero in clientes)
                {
                    WriteTercero(xmlWriter, enumNegocio.Cliente, tercero);
                }
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndDocument();
            }
        }

        public void GenerarAsientosEd(List<FacturaEmtDtm> emitidas, List<FacturaRecDtm> recibidas, List<PagoDtm> pagos)
        {
            if (emitidas.Count == 0 && recibidas.Count == 0 && pagos.Count == 0)
            {
                GestorDeErrores.Emitir("No hay ni facturas ni pagos que preasentar");
            }

            using (var fileStream = new FileStream(DiarioXml, FileMode.Create))
            using (var xmlWriter = XmlWriter.Create(fileStream, new XmlWriterSettings { Indent = true }))
            {
                xmlWriter.WriteStartDocument();
                xmlWriter.WriteStartElement("LISTAASIENTOSGL");
                xmlWriter.WriteAttributeString("xmlns", "xsi", null, _xsi);
                xmlWriter.WriteAttributeString("xmlns", "xsd", null, _xsd);

                xmlWriter.WriteStartElement("ORIGEN");
                xmlWriter.WriteElementString("ORIGEN_DATOS", ltrEd.OrigenDeDatos);
                xmlWriter.WriteEndElement();
                GenerarFacturasRecibidas(xmlWriter, recibidas);
                GenerarFacturasEmitidas(xmlWriter, emitidas);
                GenerarPagos(xmlWriter, pagos);
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndDocument();
            }
            var clientes = emitidas.Select(emt => emt.Cliente(Contexto)).Distinct().ToList();
            var proveedores = recibidas.Select(rec => rec.Proveedor(Contexto)).Distinct().ToList();
            GenerarTerceros(proveedores, clientes);
        }

        private void GenerarFacturasEmitidas(XmlWriter xmlWriter, List<FacturaEmtDtm> emitidas)
        {
            foreach (var emitida in emitidas)
            {
                if (Sociedad.CodigoDeActividad(enumNegocio.FacturaEmitida.IdNegocio(), emitida.IdTipo) != _Actividad)
                    continue;
                
                var tipoOperacion = TipoDeOperacion(emitida);
                var rectificada = emitida.RectificaA(Contexto, errorSiNoHay: false);

                xmlWriter.WriteStartElement("ASIENTO");

                xmlWriter.WriteStartElement("APUNTE");

                // Campos del APUNTE (valores en blanco/listos para reemplazar)
                xmlWriter.WriteElementString("CLIENTE_COD", Sociedad.PlanContable().IdPlanContable); //7052
                xmlWriter.WriteElementString("COD_ACTIVEJER", _Actividad); //1
                xmlWriter.WriteElementString("NIF_TERCERO", emitida.Cliente(Contexto).NIF(Contexto, quitarPrefijoEs: true)); //A01004548
                xmlWriter.WriteElementString("FECHA", emitida.EmitidaEl.Fecha().ToString("dd/MM/yyyy")); //11/03/2018 0:00:00
                xmlWriter.WriteElementString("FE_OPERACION", "");
                xmlWriter.WriteElementString("TIPO_CONCEPTO", ltrEdTipoConceptoDeIngresos.IngresosExplotacion); //1
                xmlWriter.WriteElementString("CLVTIPOOPERACION", tipoOperacion.ToString()); //IV01
                xmlWriter.WriteElementString("NUMDOC", "I" + emitida.Id.ToString().PadLeft(6, '0')); //I000003
                xmlWriter.WriteElementString("ALNUMDOC", "");
                xmlWriter.WriteElementString("RECTIFICATIVO", emitida.EsRectificativa ? "1" : "0"); //0
                xmlWriter.WriteElementString("IMPORTE", "0,000000");
                xmlWriter.WriteElementString("IMP_RETENCION", emitida.TotalDeIrpf(Contexto).Formatear(decimales: 6, alineacion: false, separadorDecimal: ",")); //0,000000
                xmlWriter.WriteElementString("IMP_SEGSOC", "0,000000");
                xmlWriter.WriteElementString("DOC_AD_BI", "");
                xmlWriter.WriteElementString("EJERCICIO_BI", "0");
                xmlWriter.WriteElementString("DOCORIGEN", rectificada != null ? "I" + rectificada.Id.ToString().PadLeft(6, '0') : "");
                xmlWriter.WriteElementString("ALDOCORIGEN", "");
                xmlWriter.WriteElementString("METALICO", "0,000000");
                xmlWriter.WriteElementString("NUM_IDEN_FACT", emitida.NumeroDeFactura); //2514-2018
                xmlWriter.WriteElementString("OBSERVACIONES", emitida.EsRectificativa ? "Devolución" : "");
                xmlWriter.WriteElementString("COMPUT_FISCAL", "0");
                xmlWriter.WriteElementString("NO_347", "0");
                xmlWriter.WriteElementString("FEXPEDICION", "");
                xmlWriter.WriteElementString("ESINGRESOS", "1"); //1
                xmlWriter.WriteElementString("REFER_CATASTRAL", "");

                xmlWriter.WriteEndElement(); // </APUNTE>

                GenerarRegistroDeIvaRep(xmlWriter, emitida, tipoOperacion);

                xmlWriter.WriteEndElement(); // </ASIENTO>
            }
        }

        private void GenerarRegistroDeIvaRep(XmlWriter xmlWriter, FacturaEmtDtm emitida, enumNcsTipoOperacion tipoOperacion)
        {
            var ivas = emitida.Ivas(Contexto);
            var retenciones = emitida.Irpfs(Contexto).ToList();

            xmlWriter.WriteStartElement("LISTAREGIVAGL");

            // Para llevar registro de las BI únicas encontradas
            var biUnicasRetenciones = retenciones
                .Select(ret => ret.BI)
                .Distinct()
                .ToList();

            bool biDiferenteProcesada = false;

            foreach (var iva in ivas)
            {
                var retencionMismaBi = retenciones.FirstOrDefault(ret => ret.BI == iva.BI);

                // Lógica especial para BI diferente (sólo permitir una vez)
                if (retencionMismaBi == null && !biDiferenteProcesada && retenciones.Count > 0)
                {
                    retencionMismaBi = retenciones[0];
                    biDiferenteProcesada = true; // Marcar como procesada
                }

                RegistrarIvaRep(xmlWriter, emitida, iva, tipoOperacion, retencionMismaBi);

                if (retencionMismaBi != null)
                {
                    retenciones.Remove(retencionMismaBi);
                }
            }

            // Validación final mejorada
            if (biUnicasRetenciones.Count > 1 && !biDiferenteProcesada)
            {
                GestorDeErrores.Emitir($"Al hacer el apunte de estimación directa de la factura emitida '{emitida.Referencia}' " +
                    "se encontraron múltiples BI diferentes en las retenciones que no coinciden con los IVA");
            }
            else if (retenciones.Count > 0)
            {
                GestorDeErrores.Emitir($"Al hacer el apunte de estimación directa de la factura emitida '{emitida.Referencia}' " +
                    $"quedaron {retenciones.Count} retenciones sin procesar");
            }

            xmlWriter.WriteEndElement();
        }

        private void RegistrarIvaRep(XmlWriter xmlWriter, FacturaEmtDtm emitida, ImportePorTipoDeIva iva, enumNcsTipoOperacion tipoOperacion, ImportePorTipoDeIrpf retencion)
        {
            var factor = emitida.EsRectificativa ? -1 : 1;
            xmlWriter.WriteStartElement("REGIVA");
            xmlWriter.WriteElementString("BASEIMPONIBLE", (factor * iva.BI).Formatear(decimales: 6, alineacion: false, separadorDecimal: ","));
            xmlWriter.WriteElementString("PORCIVA", PorcentajeDeIvaRepNcs(Contexto.SeleccionarPorId<IvaRepercutidoDtm>(iva.IdIva), tipoOperacion).Valor());
            xmlWriter.WriteElementString("CUOTAIVA", (factor * iva.Importe).Formatear(decimales: 6, alineacion: false, separadorDecimal: ","));
            xmlWriter.WriteElementString("CUOTAREC", "0,000000");
            xmlWriter.WriteElementString("CUOTARECAGR", "0,000000");
            xmlWriter.WriteElementString("TOTAL", (iva.BI + iva.Importe).Formatear(decimales: 6, alineacion: false, separadorDecimal: ","));
            xmlWriter.WriteElementString("IMP_RETENCION", retencion?.Importe.Formatear(decimales: 6, alineacion: false, separadorDecimal: ",") ?? "0,000000");
            xmlWriter.WriteElementString("BASE_RETENCION", retencion?.BI.Formatear(decimales: 6, alineacion: false, separadorDecimal: ",") ?? "0,000000");
            xmlWriter.WriteEndElement();
        }

        private void GenerarFacturasRecibidas(XmlWriter xmlWriter, List<FacturaRecDtm> recibidas)
        {

            foreach (var recibida in recibidas)
            {
                if (Sociedad.CodigoDeActividad(enumNegocio.FacturaRecibida.IdNegocio(), recibida.IdTipo) != _Actividad)
                    continue;

                if (!Sociedad.UsaRoi() && recibida.Proveedor(Contexto).EsIntraComunitario(Contexto))
                    continue;

                var proveedor = recibida.Proveedor(Contexto);

                var codigosContables = recibida.CodigosContablesPorNaturaleza(Contexto);
                var baseMayor =  recibida.BiDelIvaPorNaturaleza(Contexto).OrderByDescending(b => b.Bi).First();
                var idNaturaleza = codigosContables.Count == 1 ? baseMayor.IdNaturaleza : proveedor.IdNaturaleza.HasValue ? (int)proveedor.IdNaturaleza : baseMayor.IdNaturaleza;
                var codigosContable = codigosContables.FirstOrDefault(cc => cc.IdNaturaleza == idNaturaleza);
                if (codigosContable == null)
                {
                    GestorDeErrores.Emitir($"No se ha encontrado los códigos contable para la naturaleza '{Contexto.SeleccionarPorId<NaturalezaDtm>(idNaturaleza).Nombre}' defínalo en el parámetro '{enumParametrosDePreasiento.SPR_CodigosPorNaturaleza}'");
                    continue;
                }
                var concepto = codigosContable.CodigoConcepto;

                var tipoOperacion = codigosContable.CodigoIva.IsNullOrEmpty() 
                    ? TipoDeOperacion(recibida, concepto) 
                    : ApiDeEnsamblados.ToEnumerado<enumNcsTipoOperacion>(codigosContable.CodigoIva);

                xmlWriter.WriteStartElement("ASIENTO");

                xmlWriter.WriteStartElement("APUNTE");

                // Campos del APUNTE (valores en blanco/listos para reemplazar)
                xmlWriter.WriteElementString("CLIENTE_COD", Sociedad.PlanContable().IdPlanContable); //7052
                xmlWriter.WriteElementString("COD_ACTIVEJER", _Actividad); //1
                xmlWriter.WriteElementString("NIF_TERCERO", recibida.Proveedor(Contexto).NIF(Contexto, quitarPrefijoEs: true)); //A01004548
                xmlWriter.WriteElementString("FECHA", recibida.FacturadaEl.ToString("dd/MM/yyyy")); //11/03/2018 0:00:00
                xmlWriter.WriteElementString("FE_OPERACION", "");
                xmlWriter.WriteElementString("TIPO_CONCEPTO", concepto); //1
                xmlWriter.WriteElementString("CLVTIPOOPERACION", tipoOperacion.ToString()); //IV01
                xmlWriter.WriteElementString("NUMDOC", "P" + recibida.Id.ToString().PadLeft(6, '0')); //I000003
                xmlWriter.WriteElementString("ALNUMDOC", "");
                xmlWriter.WriteElementString("RECTIFICATIVO", recibida.EsRectificativa ? "1" : "0"); //0
                xmlWriter.WriteElementString("IMPORTE", "0,000000");
                xmlWriter.WriteElementString("CLV_RETENCION", recibida.Total(Contexto, Enumerados.enumImporteFar.TotalIrpf) > 0 ? "G1" : ""); //si >0,000000 --> G1
                xmlWriter.WriteElementString("IMP_RETENCION", recibida.Total(Contexto, Enumerados.enumImporteFar.TotalIrpf).Formatear(decimales: 6, alineacion: false, separadorDecimal: ",")); //0,000000
                xmlWriter.WriteElementString("IMP_SEGSOC", "0,000000");
                xmlWriter.WriteElementString("DOC_AD_BI", "");
                xmlWriter.WriteElementString("EJERCICIO_BI", "0");
                xmlWriter.WriteElementString("DOCORIGEN", recibida.Rectificada(Contexto, errorSiNoHay: false) is var r && r != null ? "P" + r.Id.ToString().PadLeft(6, '0') : "");
                xmlWriter.WriteElementString("ALDOCORIGEN", "");
                xmlWriter.WriteElementString("METALICO", "0,000000");
                xmlWriter.WriteElementString("NUM_IDEN_FACT", recibida.Numero); //2514-2018
                xmlWriter.WriteElementString("OBSERVACIONES", recibida.EsRectificativa ? "Devolución" : "");
                xmlWriter.WriteElementString("COMPUT_FISCAL", "0");
                xmlWriter.WriteElementString("NO_347", "0");
                xmlWriter.WriteElementString("FEXPEDICION", "");
                xmlWriter.WriteElementString("ESINGRESOS", "0");
                xmlWriter.WriteElementString("REFER_CATASTRAL", "");

                xmlWriter.WriteEndElement(); // </APUNTE>

                GenerarRegistroDeIvaSop(xmlWriter, recibida, tipoOperacion);

                xmlWriter.WriteEndElement(); // </ASIENTO>
            }
        }

        private void GenerarRegistroDeIvaSop(XmlWriter xmlWriter, FacturaRecDtm recibida, enumNcsTipoOperacion tipoOperacion)
        {
            var ivas = recibida.Ivas(Contexto);
            var retenciones = recibida.Irpfs(Contexto).ToList();

            xmlWriter.WriteStartElement("LISTAREGIVAGL");

            // Para llevar registro de las BI únicas encontradas
            var biUnicasRetenciones = retenciones.Select(ret => ret.BI).Distinct().ToList();

            bool biDiferenteProcesada = false;

            foreach (var iva in ivas)
            {
                var retencionMismaBi = retenciones.FirstOrDefault(ret => ret.BI == iva.BI);

                // Lógica especial para BI diferente (sólo permitir una vez)
                if (retencionMismaBi == null && !biDiferenteProcesada && retenciones.Count > 0)
                {
                    retencionMismaBi = retenciones[0];
                    biDiferenteProcesada = true; // Marcar como procesada
                }

                RegistrarIvaSop(xmlWriter, recibida, iva, tipoOperacion, retencionMismaBi);

                if (retencionMismaBi != null)
                {
                    retenciones.Remove(retencionMismaBi);
                }
            }

            // Validación final mejorada
            if (biUnicasRetenciones.Count > 1 && !biDiferenteProcesada)
            {
                GestorDeErrores.Emitir($"Al hacer el apunte de estimación directa de la factura recibida '{recibida.Referencia}' " +
                    "se encontraron múltiples BI diferentes en las retenciones que no coinciden con los IVA");
            }
            else if (retenciones.Count > 0)
            {
                GestorDeErrores.Emitir($"Al hacer el apunte de estimación directa de la factura recibida '{recibida.Referencia}' " +
                    $"quedaron {retenciones.Count} retenciones sin procesar");
            }

            xmlWriter.WriteEndElement();
        }

        private void RegistrarIvaSop(XmlWriter xmlWriter, FacturaRecDtm recibida, ImportePorTipoDeIva iva, enumNcsTipoOperacion tipoOperacion, ImportePorTipoDeIrpf retencion)
        {
            var factor = recibida.EsRectificativa ? -1 : 1;
            xmlWriter.WriteStartElement("REGIVA");
            xmlWriter.WriteElementString("BASEIMPONIBLE", (factor * iva.BI).Formatear(decimales: 6, alineacion: false, separadorDecimal: ","));
            xmlWriter.WriteElementString("PORCIVA", PorcentajeDeIvaSopNcs(Contexto.SeleccionarPorId<IvaSoportadoDtm>(iva.IdIva), tipoOperacion).Valor());
            xmlWriter.WriteElementString("CUOTAIVA", (factor * iva.Importe).Formatear(decimales: 6, alineacion: false, separadorDecimal: ","));
            xmlWriter.WriteElementString("CUOTAREC", "0,000000");
            xmlWriter.WriteElementString("CUOTARECAGR", "0,000000");
            xmlWriter.WriteElementString("TOTAL", (iva.BI + iva.Importe).Formatear(decimales: 6, alineacion: false, separadorDecimal: ","));
            xmlWriter.WriteElementString("IMP_RETENCION", retencion?.Importe.Formatear(decimales: 6, alineacion: false, separadorDecimal: ",") ?? "0,000000");
            xmlWriter.WriteElementString("BASE_RETENCION", retencion?.BI.Formatear(decimales: 6, alineacion: false, separadorDecimal: ",") ?? "0,000000");
            xmlWriter.WriteEndElement();
        }

        private void GenerarPagos(XmlWriter xmlWriter, List<PagoDtm> pagos)
        {
            foreach (var pago in pagos)
            {
                var codigo = pago.Naturaleza(Contexto).CodigosPorNaturaleza();

                xmlWriter.WriteStartElement("ASIENTO");
                xmlWriter.WriteStartElement("APUNTE");

                xmlWriter.WriteElementString("CLIENTE_COD", Sociedad.PlanContable().IdPlanContable);
                xmlWriter.WriteElementString("COD_ACTIVEJER", Sociedad.CodigoDeActividad(enumNegocio.Pago.IdNegocio(), pago.IdTipo));
                xmlWriter.WriteElementString("NIF_TERCERO", pago.Solicitante(Contexto).NIF(Contexto));
                xmlWriter.WriteElementString("FECHA", pago.PagarEl.Fecha().ToString("dd/MM/yyyy"));
                xmlWriter.WriteElementString("FE_OPERACION", "");
                xmlWriter.WriteElementString("TIPO_CONCEPTO", codigo.CodigoConcepto);
                xmlWriter.WriteElementString("TC_NOMBRE", pago.Naturaleza(Contexto).Nombre);


                xmlWriter.WriteElementString("NUMDOC", "G" + pago.Id.ToString().PadLeft(6, '0'));
                xmlWriter.WriteElementString("ALNUMDOC", "");
                xmlWriter.WriteElementString("RECTIFICATIVO", "0");
                xmlWriter.WriteElementString("IMPORTE", pago.Importe.Formatear(decimales: 6, alineacion: false, separadorDecimal: ","));
                xmlWriter.WriteElementString("IMP_RETENCION", "0,000000");
                xmlWriter.WriteElementString("IMP_SEGSOC", "0,000000");
                xmlWriter.WriteElementString("DOC_AD_BI", "");
                xmlWriter.WriteElementString("EJERCICIO_BI", "0");
                xmlWriter.WriteElementString("DOCORIGEN", "");
                xmlWriter.WriteElementString("ALDOCORIGEN", "");
                xmlWriter.WriteElementString("METALICO", "0,000000");
                xmlWriter.WriteElementString("NUM_IDEN_FACT", "");
                xmlWriter.WriteElementString("OBSERVACIONES", "");
                xmlWriter.WriteElementString("COMPUT_FISCAL", "0");
                xmlWriter.WriteElementString("NO_347", "0");
                xmlWriter.WriteElementString("FEXPEDICION", "");
                xmlWriter.WriteElementString("ESINGRESOS", "0");
                xmlWriter.WriteElementString("REFER_CATASTRAL", "");


                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();
            }
        }

        private void WriteTercero(XmlWriter xmlWriter, enumNegocio negocioTercero, ITerceroContable tercero)
        {
            var nif = negocioTercero == enumNegocio.Proveedor ? ((ProveedorDtm)tercero).NIF(Contexto) : ((ClienteDtm)tercero).NIF(Contexto);

            if (nif.StartsWith(ltrIsoPaises.Spain)) nif = nif.Replace(ltrIsoPaises.Spain, "");
            var interlocutor = tercero.Interlocutor(Contexto);

            var direcciodto = negocioTercero == enumNegocio.Proveedor
                ? ((ProveedorDtm)tercero).DireccionFiscal(Contexto, errorSiNoHay: false)
                : ((ClienteDtm)tercero).DireccionFiscal(Contexto, errorSiNoHay: false);


            //if (direcciodto == null)
            //    GestorDeErrores.Emitir("Para poder generar la estimación directa contable se necesita la dirección fiscal");

            var direccionFiscal = direcciodto is null ? null : GestorDeDirecciones.Gestor(Contexto, NegociosDeSe.ToEnumerado(direcciodto.Negocio)).LeerRegistroPorId(direcciodto.Id);
            var tipo = direccionFiscal == null ? ltrEdTipoDocumento.Cif : !direccionFiscal.EsIntraComunitario(Contexto) && !direccionFiscal.EsExtraComunitario(Contexto)
                ? ltrEdTipoDocumento.Cif
                : direccionFiscal.EsIntraComunitario(Contexto)
                ? ltrEdTipoDocumento.Comunitario
                : ltrEdTipoDocumento.Otro;

            var tipoDocumento = interlocutor.EsPersona ? interlocutor.Persona(Contexto).EsNie ? ltrEdTipoDocumento.Nie : ltrEdTipoDocumento.Nif : tipo;
            xmlWriter.WriteStartElement("Tercero");

            xmlWriter.WriteElementString("CIF", nif);
            xmlWriter.WriteElementString("TIPO_DOCUM", tipoDocumento);
            xmlWriter.WriteElementString("APELLIDO1", interlocutor.EsPersona ? interlocutor.Persona(Contexto).Apellidos : "");
            xmlWriter.WriteElementString("APELLIDO2", "");
            xmlWriter.WriteElementString("NOMBRE", interlocutor.EsPersona ? interlocutor.Persona(Contexto).Nombre : interlocutor.RazonSocial(Contexto));
            xmlWriter.WriteElementString("NOMBRE_COMERCIAL", interlocutor.EsPersona ? "" : interlocutor.Sociedad(Contexto).Nombre);
            xmlWriter.WriteElementString("FISICA", interlocutor.EsPersona ? "1" : "2");
            xmlWriter.WriteElementString("TIPO_ENTIDAD", "");
            xmlWriter.WriteElementString("PAGINA_WEB", "");

            foreach (var direccion in ((IElementoDtm)tercero).Direcciones(Contexto))
            {
                xmlWriter.WriteStartElement("Domicilios");
                WriteDireccion(xmlWriter, direccion);
                xmlWriter.WriteEndElement();
            }

            xmlWriter.WriteStartElement("Telefonos");
            xmlWriter.WriteStartElement("Telefono");
            xmlWriter.WriteElementString("TELEFONO", "");
            xmlWriter.WriteElementString("TIPO", "");
            xmlWriter.WriteElementString("USO", "");
            xmlWriter.WriteElementString("CONTACTO", "");
            xmlWriter.WriteEndElement(); // Telefono
            xmlWriter.WriteEndElement(); // Telefonos

            xmlWriter.WriteStartElement("EMails");
            xmlWriter.WriteStartElement("EMail");
            xmlWriter.WriteElementString("EMAIL", "");
            xmlWriter.WriteElementString("USO", "");
            xmlWriter.WriteElementString("CONTACTO", "");
            xmlWriter.WriteEndElement(); // EMail
            xmlWriter.WriteEndElement(); // EMails

            xmlWriter.WriteStartElement("Datos_Bancarios");
            xmlWriter.WriteStartElement("Datos_Banco");
            xmlWriter.WriteElementString("ENTIDAD", "");
            xmlWriter.WriteElementString("OFICINA", "");
            xmlWriter.WriteElementString("DC", "");
            xmlWriter.WriteElementString("NUMCTA", "");
            xmlWriter.WriteElementString("IBAN", "");
            xmlWriter.WriteElementString("BIC", "");
            xmlWriter.WriteElementString("NOMBRE_BANCO", "");
            xmlWriter.WriteElementString("ABREVIATURA", "");
            xmlWriter.WriteElementString("PAGINA_WEB", "");
            xmlWriter.WriteElementString("ID_SIGLA", "");
            xmlWriter.WriteElementString("SIGLA", "");
            xmlWriter.WriteElementString("DOMICILIO", "");
            xmlWriter.WriteElementString("POBLACION", "");
            xmlWriter.WriteElementString("ID_MUNICIPIO", "");
            xmlWriter.WriteElementString("USO_CTA_BANC", "");
            xmlWriter.WriteElementString("TELEFONO", "");
            xmlWriter.WriteElementString("FAX", "");
            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndElement();

            xmlWriter.WriteEndElement();
        }

        private void WriteDireccion(XmlWriter xmlWriter, DireccionDtm direccion)
        {
            xmlWriter.WriteStartElement("Domicilio");
            xmlWriter.WriteElementString("SIGLAS", "");
            xmlWriter.WriteElementString("DOMICILIO", direccion.Nombre(Contexto));
            xmlWriter.WriteElementString("TIPO_NUM", "");
            xmlWriter.WriteElementString("NUMERO", "");
            xmlWriter.WriteElementString("CALIF_NUMERO", "");
            xmlWriter.WriteElementString("LETRA", "");
            xmlWriter.WriteElementString("ESCALERA", "");
            xmlWriter.WriteElementString("BLOQUE", "");
            xmlWriter.WriteElementString("PORTAL", "");
            xmlWriter.WriteElementString("PISO", "");
            xmlWriter.WriteElementString("PUERTA", "");
            xmlWriter.WriteElementString("ZONA", "");
            xmlWriter.WriteElementString("C_POSTAL", "");
            xmlWriter.WriteElementString("MUNICIPIO", "");
            xmlWriter.WriteElementString("POBLACION", "");
            xmlWriter.WriteElementString("PAIS", "");
            xmlWriter.WriteElementString("DAT_COMP", "");
            xmlWriter.WriteElementString("REF_CATASTRAL", "");
            xmlWriter.WriteElementString("ARRENDAMIENTO", "False");
            xmlWriter.WriteEndElement();
        }

        private enumNcsTipoOperacion TipoDeOperacion(FacturaEmtDtm emitida)
        {
            var tiposDeIva = emitida.Ivas(Contexto);
            if (tiposDeIva.Any(tipo => tipo.EsIsp))
                return enumNcsTipoOperacion.IV31;

            var nacionalidad = ApiDeTerceros.ClaseDeNacionalidad(emitida.Cliente(Contexto).NIF(Contexto));
            if (nacionalidad == enumClaseDeNacionalidad.Intracomunitario)
                return emitida.EsMaterial(Contexto) ? enumNcsTipoOperacion.IV04 : enumNcsTipoOperacion.IV19;
            if (nacionalidad == enumClaseDeNacionalidad.Extracomunitario)
                return enumNcsTipoOperacion.IV05;

            return enumNcsTipoOperacion.IV01;
        }

        private enumNcsTipoOperacion TipoDeOperacion(FacturaRecDtm recibida, string concepto)
        {
            if (concepto == ltrEdTipoConceptoDeGasto.ArrendamientosYCanones) return enumNcsTipoOperacion.IV13;
            var tiposDeIva = recibida.Ivas(Contexto);
            if (tiposDeIva.Any(tipo => tipo.EsIsp))
                return enumNcsTipoOperacion.IV17;

            var nacionalidad = ApiDeTerceros.ClaseDeNacionalidad(recibida.Proveedor(Contexto).NIF(Contexto));
            if (nacionalidad == enumClaseDeNacionalidad.Intracomunitario)
                return recibida.EsMaterial(Contexto) ? enumNcsTipoOperacion.IV02 : enumNcsTipoOperacion.IV20;
            if (nacionalidad == enumClaseDeNacionalidad.Extracomunitario)
                return enumNcsTipoOperacion.IV03;

            return enumNcsTipoOperacion.IV01;
        }

    }
}
