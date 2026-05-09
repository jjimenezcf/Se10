using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.TrabajosSometidos;
using ModeloDeDto.Ventas;
using ServicioDeDatos;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.Ventas;
using ServicioXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Utilidades;
using VeriFactu.Common.Exceptions;

namespace GestoresDeNegocio.Venta.Factura
{
    public class GeneradorSii
    {
        public ContextoSe Contexto { get; }
        public FacturaEmtDtm Factura { get; }
        public SociedadDtm Sociedad { get; }
        public CertificadoDtm Certificado { get; }
        private string _password => ApiDeCertificados.LeerPasswordDeCertificado(Contexto, Certificado.Id);
        private Certificado _certificado => ApiDeCertificados.ObtenerCertificado(Contexto, Certificado.Id, _password);

        private string _numeroImplantacion => Sociedad.ObtenerSSII_Implantacion(Contexto);

        public GeneradorSii(ContextoSe contexto, FacturaEmtDtm factura)
        : this(contexto, factura.Cg(contexto).Sociedad(contexto))
        {
            Factura = factura;
        }

        public GeneradorSii(ContextoSe contexto, SociedadDtm sociedad)
        {
            Contexto = contexto;
            Sociedad = sociedad;
            Certificado = ExtensorDeSociedades.ObtenerCertificado(sociedad, contexto);
            CrearDirectorios(sociedad);
            ValidarConexionConLaAeat();
        }

        internal static void CrearDirectorios(SociedadDtm sociedad)
        {

            if (Directory.Exists(CacheDeVariable.CFG_Ruta_Ficheros_De_Sii) == false) Directory.CreateDirectory(CacheDeVariable.CFG_Ruta_Ficheros_De_Sii);
            if (Directory.Exists(CacheDeVariable.CFG_Ruta_Fichero_De_Sii_Log) == false) Directory.CreateDirectory(CacheDeVariable.CFG_Ruta_Fichero_De_Sii_Log);
            if (Directory.Exists(CacheDeVariable.CFG_Ruta_Fichero_De_Sii_Inbox) == false) Directory.CreateDirectory(CacheDeVariable.CFG_Ruta_Fichero_De_Sii_Inbox);
            if (Directory.Exists(CacheDeVariable.CFG_Ruta_Fichero_De_Sii_Outbox) == false) Directory.CreateDirectory(CacheDeVariable.CFG_Ruta_Fichero_De_Sii_Outbox);
            if (Directory.Exists(CacheDeVariable.CFG_Ruta_Fichero_De_Sii_Invoices) == false) Directory.CreateDirectory(CacheDeVariable.CFG_Ruta_Fichero_De_Sii_Invoices);
            if (Directory.Exists(CacheDeVariable.CFG_Ruta_Fichero_De_Sii_Blockchains) == false) Directory.CreateDirectory(CacheDeVariable.CFG_Ruta_Fichero_De_Sii_Blockchains);

            if (!Path.Exists(CacheDeVariable.CFG_Ruta_Fichero_De_Sii_Log))
                Directory.CreateDirectory(CacheDeVariable.CFG_Ruta_Fichero_De_Sii_Log);

            var rutaLog = Path.Combine(CacheDeVariable.CFG_Ruta_Fichero_De_Sii_Log, sociedad.NIFSinIsoEs);
            if (!Path.Exists(rutaLog))
                Directory.CreateDirectory(rutaLog);

            var rutaBc = Path.Combine(CacheDeVariable.CFG_Ruta_Fichero_De_Sii_Blockchains, sociedad.NIFSinIsoEs);
            if (!Path.Exists(rutaBc))
                Directory.CreateDirectory(rutaBc);
        }

        public static void BlanquearDirectorios(SociedadDtm sociedad)
        {
            // 1. Definimos la lista de rutas base a limpiar
            List<string> rutasALimpiar = new List<string>
            {
                Path.Combine(CacheDeVariable.CFG_Ruta_Fichero_De_Sii_Log, sociedad.NIFSinIsoEs),
                Path.Combine(CacheDeVariable.CFG_Ruta_Fichero_De_Sii_Inbox, sociedad.NIFSinIsoEs),
                Path.Combine(CacheDeVariable.CFG_Ruta_Fichero_De_Sii_Outbox, sociedad.NIFSinIsoEs),
                Path.Combine(CacheDeVariable.CFG_Ruta_Fichero_De_Sii_Invoices, sociedad.NIFSinIsoEs),
                Path.Combine(CacheDeVariable.CFG_Ruta_Fichero_De_Sii_Blockchains, sociedad.NIFSinIsoEs),
            };

            foreach (var ruta in rutasALimpiar)
            {
                if (Directory.Exists(ruta))
                {
                    LimpiarArchivosDeCarpeta(ruta);
                }
            }
        }

        private static void LimpiarArchivosDeCarpeta(string ruta)
        {
            try
            {
                string[] ficheros = Directory.GetFiles(ruta);
                foreach (string fichero in ficheros)
                {
                    File.Delete(fichero);
                }
            }
            catch (Exception ex)
            {
                throw Excepciones.Emitir($"No se pudo limpiar la carpeta {ruta}: {ex.Message}");
            }
        }

        public void AltaDeFacturaSif(bool esUnLote)
        {
            Contexto.IniciarTraza($"{nameof(AltaDeFacturaSif)}_{Factura.Referencia}".NormalizarFichero(), debugar: true);
            try
            {
                if (!esUnLote)
                {
                    var log = Contexto.SeleccionarPorPropiedad<LogDeEnvioDeFacturaDtm>(nameof(LogDeEnvioDeFacturaDtm.IdFactura), Factura.Id, errorSiNoHay: false);
                    if (log is null)
                        GestorDeErrores.Emitir($"La factura '{Factura.Referencia}' no está indicada que se ha de enviar a la AEAT");

                    if (log.EnviadaEl is not null)
                        GestorDeErrores.Emitir($"La factura '{Factura.Referencia}' se envió a la AEAT el {log.EnviadaEl.Fecha().ToString("dd-MM-yyyy HH:mm:ss")}");
                }

                var nif = Factura.Sociedad(Contexto).NIFSinIsoEs;
                var nombreRazon = Factura.Sociedad(Contexto).RazonSocial;
                AuditoriaSii auditoriaSii = null;
                var ficheroCsv = FicheroDondeEstaLaFactura();
                if (!ficheroCsv.IsNullOrEmpty())
                {
                    RecomponerAuditoria(ficheroCsv);
                    GestorDeErrores.Emitir(ltrSii.FacturaEnviada.Replace(nameof(FacturaEmtDtm.Referencia), Factura.Referencia).Replace(ltrSii.ficheroCsv, ficheroCsv));
                }

                var facturaAeat = BuscarFactura();
                if (facturaAeat != null)
                    GestorDeErrores.Emitir(ltrSii.FacturaEnLaAEAT.Replace(nameof(FacturaEmtDtm.Referencia), Factura.Referencia));

                var verifactu = Factura.Verifactu(Contexto, errorSiNoHay: false);
                if (verifactu != null)
                    GestorDeErrores.Emitir(ltrSii.FacturaEnVerifactu.Replace(nameof(FacturaEmtDtm.Referencia), Factura.Referencia));

                try
                {
                    if (Factura.EsRectificativa)
                        auditoriaSii = AltaDeFacturaRectificativa();
                    else
                        auditoriaSii = Factura.AltaDeFacturaSIF(Contexto, _certificado, _numeroImplantacion, ((EntornoDeTrabajo)Contexto.Entorno).CrearTraza);
                    Factura.AsociarAuditoriaSii(Contexto, auditoriaSii);
                }
                finally
                {
                    _certificado.Dispose();
                }
            }
            catch (Exception e)
            {
                Contexto.AnotarExcepcion(e);
                throw;
            }
            finally
            {
                Contexto.CerrarTraza();
            }
        }

        private AuditoriaSii AltaDeFacturaRectificativa()
        {
            if (Factura.MotivoDeRectificacion == enumMotivoDeRectificacion.DatosErroneos)
            {
                var rectificada = Factura.RectificaA(Contexto);
                if (rectificada.Verifactu(Contexto).Cancelada == false)
                    throw new System.Exception($"No se puede crear la factura '{Factura.Referencia}' ya que previamente se ha de cancelar la rectificada '{rectificada.Referencia}'");

                return Factura.AltaDeFacturaSIF(Contexto, _certificado, _numeroImplantacion, ((EntornoDeTrabajo)Contexto.Entorno).CrearTraza);
            }

            if (Factura.MotivoDeRectificacion == enumMotivoDeRectificacion.PorImportes)
            {
                var rectificada = Factura.RectificaA(Contexto);
                if (rectificada.Verifactu(Contexto).Cancelada == true)
                    throw new System.Exception($"No se puede crear la factura '{Factura.Referencia}' ya que la rectificada '{rectificada.Referencia}' está cancelada en la AEAT");

                return Factura.ClaseRectificativa == enumClaseDeRectificativa.OR
                ? Factura.RectificarFacturaPorSustitucionSIF(Contexto, _certificado, _numeroImplantacion, ((EntornoDeTrabajo)Contexto.Entorno).CrearTraza)
                : Factura.RectificarFacturaPorDiferenciaSIF(Contexto, _certificado, _numeroImplantacion, ((EntornoDeTrabajo)Contexto.Entorno).CrearTraza);
            }

            throw new System.Exception($"El motivo de rectificación '{Factura.MotivoDeRectificacion.Descripcion()}' de la factura '{Factura.Referencia}' no está implementado para enviar al sii");
        }

        public void CancelarLaRectificada()
        {
            var rectificada = Factura.RectificaA(Contexto);
            var verifactu = rectificada.Verifactu(Contexto);
            if (verifactu.Cancelada == true)
                GestorDeErrores.Emitir($"No se puede emitir la factura '{Factura.Referencia}' ya que la factura que rectifica '{rectificada.Referencia}' se ha cancelado en la AEAT, cree una nueva factura y cancele esta en su sistema");

            if (Factura.MotivoDeRectificacion == enumMotivoDeRectificacion.PorImpago)
                GestorDeErrores.Emitir($"El motivo de rectificación '{Factura.MotivoDeRectificacion.Descripcion()}' de la factura '{Factura.Referencia}' no está implementado para enviar al sii");

            rectificada.CancelarFacturaSIF(Contexto, _certificado, _numeroImplantacion);
            verifactu.Cancelada = true;
            verifactu.ModificarComoAdministrador(Contexto);
        }


        private void ValidarConexionConLaAeat()
        {
            try
            {
                Sociedad.ValidarConexionConLaAeat(Contexto, _certificado, _numeroImplantacion, DateTime.Now.Year, DateTime.Now.Month);
            }
            catch (Exception e)
            {
                if (e.GetType() == typeof(FaultException))
                    throw Excepciones.Emitir($"{ltrSii.VerifactuNoActivo}: {((FaultException)e).Fault.faultstring}");
                throw;
            }
            finally
            {
                _certificado.Dispose();
            }
        }

        private List<FacturaAeatDto> ConsultaDeFacturasInterna(int ano, int mes)
        {
            try
            {
                return Sociedad.ConsultaDeFacturasEmitidas(Contexto, _certificado, _numeroImplantacion, ano, mes);
            }
            finally
            {
                _certificado.Dispose();
            }
        }

        public List<FacturaAeatDto> ConsultaDeFacturas(int ano, int mes)
        {
            return ConsultaDeFacturasInterna(ano, mes);
        }

        public void ValidarNif()
        {

            try
            {
                Factura.ValidarNif(Contexto, _certificado, _numeroImplantacion);
            }
            finally
            {
                _certificado.Dispose();
            }
        }


        public void ValidarNif(string nif, string razonSocial)
        {
            try
            {
                Verifactu.ValidarNif(Contexto, nif, razonSocial, _certificado, _numeroImplantacion);
            }
            finally
            {
                _certificado.Dispose();
            }
        }
       
        public void ValidarVat()
        {

            try
            {
                Factura.ValidarVat(Contexto, _certificado, _numeroImplantacion);
            }
            finally
            {
                _certificado.Dispose();
            }
        }

        public string FicheroDondeEstaLaFactura()
        {
            var mesBuscado = Factura.FacturadaEl.Fecha().Month;
            var anoDeEmision = Factura.FacturadaEl.Fecha().Year;
            var mesActual = DateTime.Now.Month;

            var rutaBlockChain = Path.Combine(CacheDeVariable.CFG_Ruta_Fichero_De_Sii_Blockchains, Factura.Sociedad(Contexto).NIFSinIsoEs);

            while (mesBuscado <= mesActual)
            {
                var fichero = $"{anoDeEmision}{mesBuscado.ToString().PadLeft(2, '0')}.csv";
                var rutaFicheroCsv = Path.Combine(rutaBlockChain, fichero);
                if (File.Exists(rutaFicheroCsv))
                {
                    bool esta = EstaEnElFichero(rutaFicheroCsv);
                    if (esta) return fichero;
                }
                mesBuscado++;
            }
            return null;
        }

        private bool EstaEnElFichero(string rutaFichero)
        {
            var fichero = new FicheroCsv(rutaFichero);
            var linea = 0;
            foreach (var fila in fichero)
            {
                linea++;
                if (fila.EnBlanco)
                    continue;

                if (fila.Columnas != 7)
                    GestorDeErrores.Emitir($" El fichero '{rutaFichero}' está corrupto, ya que ha de tener '7' columnas, y tiene '{fila.Columnas}' para la fila '{linea}'");
                var numeroDeFacturaEnviada = fila["F"].Trim();
                if (numeroDeFacturaEnviada == Factura.NumeroDeFactura)
                {
                    return true;
                }
            }
            return false;
        }

        private void RecomponerAuditoria(string ficheroCsv)
        {
            if (Factura.Verifactu(Contexto, errorSiNoHay: false) != null)
                return;

            var rutaBlockChain = Path.Combine(CacheDeVariable.CFG_Ruta_Fichero_De_Sii_Blockchains, Factura.Sociedad(Contexto).NIFSinIsoEs);
            var rutaFicheroCsv = Path.Combine(rutaBlockChain, ficheroCsv);
            var fila = BuscarFactura(rutaFicheroCsv);

            Factura.RecomponerVerifactu(Contexto, fila["C"]);
        }

        private CsvFila BuscarFactura(string rutaFichero)
        {
            var fichero = new FicheroCsv(rutaFichero);
            var linea = 0;
            foreach (var fila in fichero)
            {
                linea++;
                if (fila.EnBlanco)
                    continue;

                if (fila.Columnas != 7)
                    GestorDeErrores.Emitir($" El fichero '{rutaFichero}' está corrupto, ya que ha de tener '7' columnas, y tiene '{fila.Columnas}' para la fila '{linea}'");
                var numeroDeFacturaEnviada = fila["F"].Trim();
                if (numeroDeFacturaEnviada == Factura.NumeroDeFactura)
                {
                    return fila;
                }
            }
            return null;
        }

        public void RecomponerBlockChain()
        {
            var ano = DateTime.Now.Year;
            var mesActual = DateTime.Now.Month;
            var indice = 0;

            var blockChain = CancelarArchivosDeBlockChain(ano);

            for (int mes = 1; mes <= mesActual; mes++)
            {
                var nombreArchivo = $"{ano}{mes.ToString().PadLeft(2, '0')}.csv";
                var rutaBc = Path.Combine(CacheDeVariable.CFG_Ruta_Fichero_De_Sii_Blockchains, Sociedad.NIFSinIsoEs, nombreArchivo);
                if (File.Exists(rutaBc)) File.Delete(rutaBc);
                FacturaEmtDtm ultimaDelMes = null;
                (indice, ultimaDelMes) = RecomponerBlockChain(ano, mes, indice, blockChain);
                if (ultimaDelMes is not null)
                {
                    blockChain.AnexarArchivo(Contexto, rutaBc, copiar: true);
                }
            }

            if (indice == 0)
                return;

            var ultima = Sociedad.UltimaFactura(Contexto, ano, errorSiNoHay: false);
            if (ultima is null)
                return;

            var ruta = Path.Combine(CacheDeVariable.CFG_Ruta_Fichero_De_Sii_Blockchains, Sociedad.NIFSinIsoEs, $"_{Sociedad.NIFSinIsoEs}.csv");
            var lineas = new StringBuilder();
            var verifactu = ultima.Verifactu(Contexto);
            lineas.AppendLine($"{indice};{ultima.FacturadaEl.FechaFormatoIso8601()};{verifactu.Huella};{ultima.FacturadaEl.FechaCorta(formato: "dd/MM/yyyy")};{Sociedad.NIFSinIsoEs};{ultima.NumeroDeFactura}");
            File.WriteAllText(ruta, lineas.ToString());
        }

        public void SincronizarConDatosDeLaAeat()
        {
            var facturaAeat = BuscarFactura();
            if (facturaAeat is null)
                GestorDeErrores.Emitir($"No se ha encontrado la factura '{Factura.Referencia}' en la AEAT");
            var verifactu = Factura.Verifactu(Contexto, errorSiNoHay: false);

            if (verifactu == null)
            {
                var ficheroCsv = FicheroDondeEstaLaFactura();
                if (ficheroCsv.IsNullOrEmpty())
                {
                    var rutaBlockChain = Path.Combine(CacheDeVariable.CFG_Ruta_Fichero_De_Sii_Blockchains, Factura.Sociedad(Contexto).NIFSinIsoEs);
                    GestorDeErrores.Emitir($"La factura '{Factura.Referencia}' no se localiza en el directorio '{rutaBlockChain}', considere reconstruir el fichero de BlockChain en la edición de la sociedad '{Sociedad.NIF}'");
                }
                Factura.RecomponerVerifactu(Contexto, facturaAeat.Huella);
            }


            var log = Contexto.SeleccionarPorPropiedad<LogDeEnvioDeFacturaDtm>(nameof(LogDeEnvioDeFacturaDtm.IdFactura), Factura.Id);
            if (log.EnviadaEl is not null)
                GestorDeErrores.Emitir($"La factura '{Factura.Referencia}' se envió el '{log.EnviadaEl.Fecha().ToString("dd-MM-yyyy HH:mm:ss")}', según el log de envío");

            log.EnviadaEl = DateTime.Now;
            log.Modificar(Contexto);
        }

        private ArchivadorDtm CancelarArchivosDeBlockChain(int ano)
        {
            var ultimaEnAEAT = Sociedad.UltimaFactura(Contexto, ano, errorSiNoHay: false);
            if (ultimaEnAEAT is null)
                GestorDeErrores.Emitir($"No hay facturas emitidas en la sociedad '{Sociedad.Referencia}' en el año '{ano}'");

            while (true)
            {
                if (ultimaEnAEAT.Verifactu(Contexto, errorSiNoHay: false) != null)
                    break;

                var aux = ultimaEnAEAT.Anterior(Contexto);
                if (aux is null || aux.Ano < ano)
                    break;

                ultimaEnAEAT = aux;
            }

            var verifactu = ultimaEnAEAT.Verifactu(Contexto, errorSiNoHay: false);
            if (verifactu is null)
                GestorDeErrores.Emitir($"De las facturas emitidas en la sociedad '{Sociedad.Referencia}' en el año '{ano}' no hay ninguna con auditoría de Verifactu");

            var archivador = verifactu.ObtenerBlockChain(Contexto);
            archivador.MarcarArchivosComoCancelados(Contexto);
            return archivador;
        }

        private FacturaAeatDto BuscarFactura()
        {
            if (Factura.FacturadaEl is null)
                GestorDeErrores.Emitir($"La factura '{Factura.Referencia}' no tiene fecha de emisión");
            var facturas = ConsultaDeFacturasInterna(((DateTime)Factura.FacturadaEl).Year, ((DateTime)Factura.FacturadaEl).Month);
            return facturas.FirstOrDefault(f => f.NumeroFactura == Factura.NumeroDeFactura);
        }

        private (int indice, FacturaEmtDtm ultimaDelMes) RecomponerBlockChain(int ano, int mes, int indice, ArchivadorDtm blockChain)
        {
            var facturasAeat = ConsultaDeFacturasInterna(ano, mes);
            var rutaMes = Path.Combine(CacheDeVariable.CFG_Ruta_Fichero_De_Sii_Blockchains, Sociedad.NIFSinIsoEs, $"{ano}{mes.ToString().PadLeft(2, '0')}.csv");
            var lineas = new StringBuilder();
            var indiceFinal = indice;

            var ultimaDelMes = null as FacturaEmtDtm;
            foreach (var facturaAeat in facturasAeat)
            {
                var factura = Contexto.SeleccionarPorId<FacturaEmtDtm>(facturaAeat.Id);

                var verifactu = factura.Verifactu(Contexto, errorSiNoHay: false);
                if (verifactu is null)
                    continue;

                if (verifactu.IdBlockChain != blockChain.Id)
                {
                    verifactu.IdBlockChain = blockChain.Id;
                    verifactu.ModificarComoAdministrador(Contexto);
                }

                ultimaDelMes = factura;
                var anterior = factura.Anterior(Contexto);
                var datosAnteriores = "";
                if (anterior is not null)
                {
                    var verifactuAnt = anterior.Verifactu(Contexto, errorSiNoHay: false);
                    if (verifactuAnt is not null)
                    {
                        var cuota = extNumeros.Formatear(anterior.Ivas(Contexto).Sum(iva => iva.Cuota), alineacion: false, separadorDecimal: ".");
                        var total = extNumeros.Formatear(anterior.BiConIva(Contexto) - anterior.Irpf(Contexto), alineacion: false, separadorDecimal: ".");
                        datosAnteriores = $"[IDEmisorFactura={Sociedad.NIFSinIsoEs}&NumSerieFactura={anterior.NumeroDeFactura}&FechaExpedicionFactura={anterior.FacturadaEl.FechaCorta()}&TipoFactura={facturaAeat.TipoAeat}&CuotaTotal={cuota}&ImporteTotal={total}&Huella={verifactuAnt.Huella}&FechaHoraHusoGenRegistro={anterior.EmitidaEl.FechaFormatoIso8601()}]";
                    }
                }
                indiceFinal++;
                lineas.AppendLine($"{indiceFinal};{factura.FacturadaEl.FechaFormatoIso8601()};{verifactu.Huella};{factura.FacturadaEl.FechaCorta(formato: "dd/MM/yyyy")};{Sociedad.NIFSinIsoEs};{factura.NumeroDeFactura};{datosAnteriores}");
            }

            if (indice < indiceFinal)
            {
                File.WriteAllText(rutaMes, lineas.ToString());
            }

            return (indice: indiceFinal, ultimaDelMes: ultimaDelMes);
        }

        public static bool VerifactuActivo(ContextoSe contexto, FacturaEmtDtm facturaDmt)
        {
            try
            {
                var _ = new GeneradorSii(contexto, facturaDmt.Sociedad(contexto));
                return true;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains(ltrSii.VerifactuNoActivo))
                    return false;
                throw;
            }
        }
    }
}