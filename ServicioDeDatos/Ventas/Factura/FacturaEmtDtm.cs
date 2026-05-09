using Gestor.Errores;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Gastos;
using ServicioDeDatos.Juridico;
using ServicioDeDatos.MaestrosTecnico;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.Presupuesto;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Terceros;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Utilidades;
using static ServicioDeDatos.Elemento.Enumerados;

namespace ServicioDeDatos.Ventas
{

    public enum enumMotivoDeRectificacion
    {
        [Description("Datos erroneos")]
        DatosErroneos,
        [Description("Importes erroneos")]
        PorImportes,
        [Description("Por impago")]
        PorImpago
    }


    public enum enumClaseDeRectificativa
    {
        [Description("Complementaria")]
        OC,
        [Description("Total")]
        OR
    }

    public enum enumClaseDeEmision
    {
        [Description("eFactura 3.2")]
        eFactura32,
        [Description("eFactura 3.2.2")]
        eFactura322,
        [Description("Factura impresa")]
        Impresa
    }

    public enum enumTipoFacturaAeat
    {
        [Description("Factura")]
        F1,
        [Description("Factura Simplificada")]
        F2,
        [Description("Factura Rectificativa (por sustitución)")]
        R1,
        [Description("Factura Rectificativa (por diferencias)")]
        R2,
        [Description("Factura Rectificativa (por descuento por volumen de operaciones)")]
        R3,
        [Description("Resto)")]
        R4
    }


    public static class ltrSii
    {
        public const string IDEmisorFactura = nameof(IDEmisorFactura);
        public const string NumSerieFactura = nameof(NumSerieFactura);
        public const string FechaExpedicionFactura = nameof(FechaExpedicionFactura);
        public const string TipoFactura = nameof(TipoFactura);
        public const string CuotaTotal = nameof(CuotaTotal);
        public const string ImporteTotal = nameof(ImporteTotal);
        public const string Huella = nameof(Huella);
        public const string FechaHoraHusoGenRegistro = nameof(FechaHoraHusoGenRegistro);

        public const string SiglaTipoArchivadorSii = "SII";
        public const string ficheroCsv = nameof(ficheroCsv);
        public const string NoSePuedeReenviar = "no se puede volver a enviar";
        public const string YaExisteUnaEntradaConSellerId = "Ya existe una entrada con SellerID";
        public static readonly string FacturaEnviada = $"La factura '{nameof(FacturaEmtDtm.Referencia)}' ya se ha enviado al SII, {NoSePuedeReenviar}, se ha localizado en el fichero '{ltrSii.ficheroCsv}'";
        public static readonly string FacturaEnLaAEAT = $"La factura '{nameof(FacturaEmtDtm.Referencia)}' ya se ha enviado al SII, {NoSePuedeReenviar}, se ha localizado en la Aeat'";
        public static readonly string FacturaEnVerifactu = $"La factura '{nameof(FacturaEmtDtm.Referencia)}' ya se ha enviado al SII, {NoSePuedeReenviar}, se ha localizado en el registro Verifactu'";
        public static readonly string VerifactuNoActivo = "Error al acceder a los servidores de la AEAT";

        public static class Respuesta
        {
            public const string Correcta = "Correcto";
            public const string ParcialmenteCorrecta = "ParcialmenteCorrecto";
        }
    }
    public class AuditoriaSii
    {
        public required string InvoiceEntryID { get; set; }
        public required string EncodedInvoiceID { get; set; }
        public required string FicheroEnviado { get; set; }
        public required string FacturaEnviada { get; set; }
        public required string CSV { get; set; }
        public required string FicheroDeRespuesta { get; set; }
        public required string RutaBlockChain { get; set; }
        public required string Respuesta { get; set; }
        public string Codigo { get; set; }
        public string Error { get; set; }
        public required string Huella { get; set; }
    }

    public class IrpfEmtJson
    {
        public decimal BiSujeta { get; set; }
        public string Irpf;
    }

    public class LineasEmtJson
    {
        public int Orden { get; set; }
        public enumTipoDeLinea TipoDeLinea { get; set; }
        public string Concepto { get; set; }
        public decimal? Cantidad { get; set; }
        public decimal? Precio { get; set; }
        public string Anotacion { get; set; }
        public decimal? Descuento { get; set; }
        public string Iva { get; set; }
        public string Unidad { get; set; }
        public string Naturaleza { get; set; }
        public enumClaseUnitario? Clase { get; set; }
    }
    public class FacturaEmtJson
    {
        public string eMail;

        public string NifDelCliente { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Contacto { get; set; }
        public string Telefono { get; set; }

        public IrpfEmtJson IrpfEmt { get; set; }

        public List<LineasEmtJson> Lineas { get; set; }

        public static FacturaEmtJson Parsear(string facturaJson)
        {
            var jsonRoot = extJson.Deserializar(facturaJson);
            var factura = new FacturaEmtJson();

            foreach (var parClaveValor in jsonRoot)
            {
                string nombreDePropiedad = parClaveValor.Key;
                JToken valorJson = parClaveValor.Value;

                switch (nombreDePropiedad)
                {
                    case nameof(FacturaEmtJson.NifDelCliente):
                        factura.NifDelCliente = valorJson.Value<string>();
                        break;
                    case nameof(FacturaEmtJson.Nombre):
                        factura.Nombre = valorJson.Value<string>();
                        break;
                    case nameof(FacturaEmtJson.Descripcion):
                        factura.Descripcion = valorJson.Value<string>();
                        break;
                    case nameof(FacturaEmtJson.Contacto):
                        factura.Contacto = valorJson.Value<string>();
                        break;
                    case nameof(FacturaEmtJson.Telefono):
                        factura.Telefono = valorJson.Value<string>();
                        break;
                    case nameof(FacturaEmtJson.eMail):
                        factura.eMail = valorJson.Value<string>();
                        break;
                    case nameof(FacturaEmtJson.Lineas):
                        if (valorJson.Type == JTokenType.Array)
                        {
                            var jArrayLineas = (JArray)valorJson;
                            factura.Lineas = jArrayLineas.ToObject<List<LineasEmtJson>>();
                        }
                        break;
                    case nameof(FacturaEmtJson.IrpfEmt):
                        if (valorJson.Type == JTokenType.Object)
                        {
                            factura.IrpfEmt = valorJson.ToObject<IrpfEmtJson>();
                        }
                        break;
                    default: 
                        GestorDeErrores.Emitir($"El nombre de la propiedad '{nombreDePropiedad}' no está contemplada, quítela del json proporcionado");
                        break;
                }
            }

            if (factura.Lineas == null)
            {
                factura.Lineas = new List<LineasEmtJson>();
            }

            return factura;
        }
    }

    [Table(Tablas.FACTURA_EMT, Schema = Esquemas.VENTA)]
    public class FacturaEmtDtm : ElementoDeProcesoDtm, IUsaCliente, IUsaArchivo, IUsaDirecciones, IUsaPreasiento, IUsaPresupuesto
    {
        public int? Ano { get; set; }
        public string Serie { get; set; }
        public int? Numero { get; set; }

        public int IdCliente { get; set; }
        public ClienteDtm Cliente { get; }
        public string Contacto { get; set; }
        public string Telefono { get; set; }
        public string eMail { get; set; }

        public new TipoDeFacturaEmtDtm Tipo { get; set; }
        public new EstadoDeUnaFacturaEmtDtm Estado { get; set; }

        public int? IdPresupuesto { get; set; }
        public int? IdContrato { get; set; }
        public int? IdParteTr { get; set; }
        public ParteTrDtm ParteTr { get; set; }
        public ContratoDtm Contrato { get; set; }
        public PresupuestoDtm Presupuesto { get; set; }

        public DateTime? EmitidaEl { get; set; }
        public DateTime? FacturadaEl { get; set; }
        public DateTime? VenceEl { get; set; }

        public enumClaseDeRectificativa? ClaseRectificativa { get; set; }

        public enumMotivoDeRectificacion? MotivoDeRectificacion { get; set; }

        public bool EsRectificativa => ClaseRectificativa != null;


        public enumClaseDeEmision ClaseDeEmision { get; set; }

        public int? IdArchivo { get; set; }
        public ArchivoDtm Archivo { get; set; }

        public int? IdArchivadorParaLaReclamacion { get; set; }
        public ArchivadorDtm ArchivadorParaLaReclamcion { get; set; }

        public int? IdCentroAdministrativo { get; set; }
        public CentroAdministrativoDtm CentroAdministrativo { get; set; }

        public string NumeroDeFactura => Numero == default ? "" : $"{Ano}-{Serie}-{Numero}";

        public string Moneda => "EUR";

        public string InformacionFiscal { get; set; }

        public string Huella { get; set; }

        public byte[] Firma { get; set; }

        public DateTime? FirmadaEl { get; set; }

        public PeriodoEmtDtm Periodo { get; set; }
        public IrpfEmtDtm IrpfEmt { get; set; }

        public int? IdPreasiento { get; set; }

        public PreasientoDtm Preasiento { get; set; }
        public VerifactuDtm Verifactu { get; set; }


        public void ValidarIva(List<LineaDeUnaFaeDtm> lineas)
        {
            foreach (var linea in lineas) linea.ValidarIva(Referencia);
        }

        public bool TieneIvaExento(List<LineaDeUnaFaeDtm> lineas)
        {
            var hayExento = lineas.Any(linea => linea.TipoDeLinea != Enumerados.enumTipoDeLinea.Comentario && linea.IvaRepercutido.Exento);
            if (!hayExento) return false;

            var lineaIncorrecta = lineas.FirstOrDefault(linea => linea.TipoDeLinea != Enumerados.enumTipoDeLinea.Comentario
                                                                 && linea.IvaRepercutido.Exento
                                                                 && (linea.IvaRepercutido.Porcentaje != 0 || linea.Iva != 0));
            if (lineaIncorrecta is not null)
                GestorDeErrores.Emitir($"La factura '{Referencia}' en la línea '{lineaIncorrecta.Concepto}' tiene un iva exento '{lineaIncorrecta.IvaRepercutido.Expresion}' y el importe o el porcentaje debe ser cero");

            return true;
        }

        public void ValidarIvaIntraComunitario(List<LineaDeUnaFaeDtm> lineas)
        {
            if (lineas.Any(linea => linea.TipoDeLinea != Enumerados.enumTipoDeLinea.Comentario && linea.IvaRepercutido.Clase != Contabilidad.enumClasesDeIvaRep.ISP))
            {
                GestorDeErrores.Emitir($"La factura '{Referencia}' es de exportación, y por tanto ha de tener ivas de exportación");
            }

            var todoExento = lineas.All(linea => linea.TipoDeLinea != Enumerados.enumTipoDeLinea.Comentario && linea.IvaRepercutido.Exento);
            if (!todoExento)
            {
                var lineaIncorrecta = lineas.First(linea => linea.TipoDeLinea != Enumerados.enumTipoDeLinea.Comentario && !linea.IvaRepercutido.Exento);
                GestorDeErrores.Emitir($"La factura '{Referencia}' es intracomunitaria, y por eso ha de se exenta de Iva");
            }
        }

        public void ValidarIvaNoSujeto(List<LineaDeUnaFaeDtm> lineas)
        {
            if (lineas.Any(linea => linea.TipoDeLinea != Enumerados.enumTipoDeLinea.Comentario && linea.IvaRepercutido.Clase != Contabilidad.enumClasesDeIvaRep.NSJ))
            {
                GestorDeErrores.Emitir($"La factura '{Referencia}' es de exportación, y por tanto ha de tener ivas de exportación");
            }

            var todoExento = lineas.All(linea => linea.TipoDeLinea != Enumerados.enumTipoDeLinea.Comentario && linea.IvaRepercutido.Exento);
            if (!todoExento)
            {
                var lineaIncorrecta = lineas.First(linea => linea.TipoDeLinea != Enumerados.enumTipoDeLinea.Comentario && !linea.IvaRepercutido.Exento);
                GestorDeErrores.Emitir($"La factura '{Referencia}' es de exportación, y por tanto ha de ser exenta de iva");
            }
        }

        public string MotivosDeExeccion(List<LineaDeUnaFaeDtm> lineas)
        {
            var motivo = "";
            var lineasExentas = lineas.Where(linea => linea.TipoDeLinea != Enumerados.enumTipoDeLinea.Comentario && linea.IvaRepercutido != null && linea.IvaRepercutido.Exento);
            foreach (var l in lineasExentas)
            {
                if (motivo.Contains(l.IvaRepercutido.DescripcionFiscal))
                    continue;

                motivo = $"{(motivo.IsNullOrEmpty() ? "" : motivo + Environment.NewLine)}{l.IvaRepercutido.DescripcionFiscal}";
            }
            return motivo;
        }

        public void InicializarPrefactura()
        {
            EmitidaEl = null;
            FacturadaEl = null;
            VenceEl = null;
            Huella = null;
            Numero = null;
            Firma = null;
            FirmadaEl = null;
        }

        public object PeticionDeFacturador(ContextoSe contexto)
        {
            throw new NotImplementedException();
        }
    }


    [Table(Tablas.FACTURA_EMT + "_" + nameof(Sufijo.AUDITORIA), Schema = Esquemas.VENTA)]
    public class AuditoriaDeUnaFacturaEmtDtm : AuditoriaDtm
    {
    }

    [Table(Tablas.FACTURA_EMT + "_" + nameof(Sufijo.ARCHIVO), Schema = Esquemas.VENTA)]
    public class ArchivosDeUnaFacturaEmtDtm : VinculoDtm
    {
        public FacturaEmtDtm FacturaEmt { get; set; }
        public ArchivoDtm Archivo { get; set; }
    }

    [Table(Tablas.FACTURA_EMT + "_" + nameof(Sufijo.AGENDA_EVENTO), Schema = Esquemas.VENTA)]
    public class AgendaDeUnaFacturaEmtDtm : VinculoDtm
    {
        public FacturaEmtDtm FacturaEmt { get; set; }
        public EventoDeAgendaDtm Evento { get; set; }
    }

    [Table(Tablas.FACTURA_EMT + "_" + nameof(Sufijo.OBSERVACION), Schema = Esquemas.VENTA)]
    public class ObservacionesDeUnaFacturaEmtDtm : ObservacionDtm
    {
        public override enumNegocio Negocio => enumNegocio.FacturaEmitida;
    }

    [Table(Tablas.FACTURA_EMT + "_" + nameof(Sufijo.PERMISO), Schema = Esquemas.VENTA)]
    public class PermisoDeLaFacturaEmtDtm : PermisosDelElementoDtm
    {
    }

    [Table(Tablas.FACTURA_EMT + "_" + nameof(Sufijo.TRAZA), Schema = Esquemas.VENTA)]
    public class TrazasDeUnaFacturaEmtDtm : TrazaDtm
    {
        public override enumNegocio Negocio => enumNegocio.FacturaEmitida;
    }

    [Table(Tablas.FACTURA_EMT + "_" + nameof(Sufijo.DIRECCION), Schema = Esquemas.VENTA)]
    public class DireccionDeUnaFacturaEmtDtm : DireccionDtm
    {
        public override enumNegocio Negocio => enumNegocio.FacturaEmitida;
    }

    [Table(Tablas.FACTURA_EMT + "_" + nameof(Sufijo.HISTORIA), Schema = Esquemas.VENTA)]
    public class HitosDeUnaFacturaEmtDtm : HitoDtm
    {

    }

    [Table(Tablas.FACTURA_EMT + "_" + nameof(Sufijo.ARCHIVADOR), Schema = Esquemas.VENTA)]
    public class ArchivadoresDeUnaFacturaEmtDtm : VinculoDtm
    {
        public FacturaEmtDtm FacturaEmt { get; set; }
        public ArchivadorDtm Archivador { get; set; }
    }

    [Table(Tablas.FACTURA_EMT + "_" + nameof(Sufijo.CIRCUITO_DOC), Schema = Esquemas.VENTA)]
    public class CircuitoDocDeUnaFacturaEmtDtm : VinculoDtm
    {
        public FacturaEmtDtm FacturaEmt { get; set; }
        public CircuitoDocDtm Circuito { get; set; }
    }


    [Table(Tablas.FACTURA_EMT + "_" + nameof(Sufijo.PAGO), Schema = Esquemas.VENTA)]
    public class AbonoDeFaeDtm : VinculoDtm
    {
        public FacturaEmtDtm FacturaEmt { get; set; }
        public PagoDtm Pago { get; set; }

    }


    public static partial class ModeloDeFacturaEmt
    {

        public static void FacturaEmt(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<FacturaEmtDtm>().Ignore(x => x.Periodo);

            ApiDeElementoDtm.DefinirCamposDelElementoDtm<FacturaEmtDtm>(modelBuilder);
            ApiDeElementoDtm.DefinirCampoCg<FacturaEmtDtm>(modelBuilder, nameof(FacturaEmtDtm.Cg));
            ApiDeElementoDtm.DefinirCampoTipo<FacturaEmtDtm>(modelBuilder, nameof(FacturaEmtDtm.Tipo));
            ApiDeElementoDtm.DefinirCampoEstado<FacturaEmtDtm>(modelBuilder, nameof(FacturaEmtDtm.Estado));
            ApiDeElementoDtm.DefinirCliente<FacturaEmtDtm>(modelBuilder);

            modelBuilder.Entity<FacturaEmtDtm>().Property(nameof(FacturaEmtDtm.ClaseRectificativa)).HasColumnName(ICampos.CLASE_RECTIFICATIVA).HasColumnType(IDominio.VARCHAR_30).IsRequired(false);
            modelBuilder.Entity<FacturaEmtDtm>().Property(nameof(FacturaEmtDtm.MotivoDeRectificacion)).HasColumnName(ICampos.MOTIVO_RECTIFICACION).HasColumnType(IDominio.VARCHAR_30).IsRequired(false);
            modelBuilder.Entity<FacturaEmtDtm>().Property(nameof(FacturaEmtDtm.ClaseDeEmision)).HasColumnName(ICampos.CLASE_EMISION).HasColumnType(IDominio.VARCHAR_15).IsRequired(true);

            modelBuilder.Entity<FacturaEmtDtm>().Property(p => p.Ano).HasColumnName(ICampos.ANO).HasColumnType(IDominio.DECIMAL_4).IsRequired(false);
            modelBuilder.Entity<FacturaEmtDtm>().Property(p => p.Serie).HasColumnName(ICampos.SERIE).HasColumnType(IDominio.VARCHAR_3).IsRequired(false);
            modelBuilder.Entity<FacturaEmtDtm>().Property(p => p.Numero).HasColumnName(ICampos.NUMERO).HasColumnType(IDominio.DECIMAL_6).IsRequired(false);
            modelBuilder.Entity<FacturaEmtDtm>().HasIndex(p => new { p.IdCg, p.Ano, p.Serie, p.Numero }).IsUnique(true).HasDatabaseName($"I_{Tablas.FACTURA_EMT}_{ICampos.ID_CG}_{ICampos.ANO}_{ICampos.SERIE}_{ICampos.NUMERO}");

            modelBuilder.Entity<FacturaEmtDtm>().Property(p => p.EmitidaEl).HasColumnName(ICampos.EMITIDA_EL).HasColumnType(IDominio.DATETIME_2).IsRequired(false);
            modelBuilder.Entity<FacturaEmtDtm>().Property(p => p.FacturadaEl).HasColumnName(ICampos.FACTURADA_EL).HasColumnType(IDominio.DATETIME_2).IsRequired(false);
            modelBuilder.Entity<FacturaEmtDtm>().Property(p => p.VenceEl).HasColumnName(ICampos.VENCE_EL).HasColumnType(IDominio.DATETIME_2).IsRequired(false);
            modelBuilder.Entity<FacturaEmtDtm>().Property(p => p.FirmadaEl).HasColumnName(ICampos.FIRMADO_EL).HasColumnType(IDominio.DATETIME_2).IsRequired(false);

            modelBuilder.Entity<FacturaEmtDtm>().Property(p => p.Huella).HasColumnName(ICampos.HUELLA).HasColumnType(IDominio.CHAR_64).IsRequired(false);
            modelBuilder.Entity<FacturaEmtDtm>().Property(p => p.Firma).HasColumnName(ICampos.FIRMA).HasColumnType(IDominio.BINARY).IsRequired(false);


            ApiDeRegistroDtm.DefinirDependencia<FacturaEmtDtm, ContratoDtm>(modelBuilder, apuntadoPor: nameof(FacturaEmtDtm.IdContrato), idCampo: ICampos.ID_CONTRATO, requerido: false);
            ApiDeRegistroDtm.DefinirDependencia<FacturaEmtDtm, PresupuestoDtm>(modelBuilder, apuntadoPor: nameof(FacturaEmtDtm.IdPresupuesto), idCampo: ICampos.ID_PRESUPUESTO, requerido: false);
            ApiDeRegistroDtm.DefinirDependencia<FacturaEmtDtm, ParteTrDtm>(modelBuilder, apuntadoPor: nameof(FacturaEmtDtm.IdParteTr), idCampo: ICampos.ID_PARTE_TR, requerido: false);

            ApiDeElementoDtm.DefinirCampoArchivo<FacturaEmtDtm>(modelBuilder);
            ApiDeElementoDtm.DefinirDependenciaDe<FacturaEmtDtm>(modelBuilder, nameof(FacturaEmtDtm.ArchivadorParaLaReclamcion), nameof(FacturaEmtDtm.IdArchivadorParaLaReclamacion), ICampos.ID_ARCHIVADOR_INTERNO, requerido: false, unico: true);

            ApiDeElementoDtm.DefinirDependenciaDe<FacturaEmtDtm>(modelBuilder, nameof(FacturaEmtDtm.CentroAdministrativo), nameof(FacturaEmtDtm.IdCentroAdministrativo), ICampos.ID_CENTRO_ADMINISTRATIVO, requerido: false, unico: false);


            modelBuilder.Entity<FacturaEmtDtm>().Property(p => p.InformacionFiscal).HasColumnName(ICampos.DES_FISCAL).HasColumnType(IDominio.VARCHAR_250).IsRequired(false);
        }

        internal static void Trazas(ModelBuilder modelBuilder)
        {
            ApiTraza.DefinirCampos<TrazasDeUnaFacturaEmtDtm, FacturaEmtDtm>(modelBuilder);
        }

        internal static void Auditoria(ModelBuilder modelBuilder)
        {
            ApiDeAuditoria.DefinirCamposDeAuditoriaDtm<AuditoriaDeUnaFacturaEmtDtm>(modelBuilder);
        }

        internal static void Archivos(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<ArchivosDeUnaFacturaEmtDtm>(modelBuilder, nameof(ArchivosDeUnaFacturaEmtDtm.FacturaEmt), nameof(ArchivosDeUnaFacturaEmtDtm.Archivo));

        }
        internal static void Agenda(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<AgendaDeUnaFacturaEmtDtm>(modelBuilder, nameof(AgendaDeUnaFacturaEmtDtm.FacturaEmt), nameof(AgendaDeUnaFacturaEmtDtm.Evento));
        }

        internal static void Observaciones(ModelBuilder modelBuilder)
        {
            ApiDeObservaciones.DefinirCampos<ObservacionesDeUnaFacturaEmtDtm, FacturaEmtDtm>(modelBuilder);
        }

        internal static void Permisos(ModelBuilder modelBuilder)
        {
            ApiPermisosDelElemento.DefinirCampos<PermisoDeLaFacturaEmtDtm, FacturaEmtDtm>(modelBuilder);
        }

        internal static void Direcciones(ModelBuilder modelBuilder)
        {
            ApiDireccionDtm.DefinirCampos<DireccionDeUnaFacturaEmtDtm, FacturaEmtDtm>(modelBuilder);
        }

        internal static void Historia(ModelBuilder modelBuilder)
        {
            ApiDeHitos.DefinirCampos<HitosDeUnaFacturaEmtDtm, FacturaEmtDtm, EstadoDeUnaFacturaEmtDtm, TransicionesDeUnaFacturaEmtDtm, ObservacionesDeUnaFacturaEmtDtm>(modelBuilder);
        }

        internal static void Archivadores(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<ArchivadoresDeUnaFacturaEmtDtm>(modelBuilder, nameof(ArchivadoresDeUnaFacturaEmtDtm.FacturaEmt), nameof(ArchivadoresDeUnaFacturaEmtDtm.Archivador));
        }

        internal static void Circuitos(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<CircuitoDocDeUnaFacturaEmtDtm>(modelBuilder, nameof(CircuitoDocDeUnaFacturaEmtDtm.FacturaEmt), nameof(CircuitoDocDeUnaFacturaEmtDtm.Circuito));
        }

        internal static void AbonoDeFae(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<AbonoDeFaeDtm>(modelBuilder, nameof(AbonoDeFaeDtm.FacturaEmt), nameof(AbonoDeFaeDtm.Pago));
        }

    }
}


/***************************************
JSON de ejemplo
 {
  "NifDelCliente": "A87654321",
  "Nombre": "Cliente Ejemplo SL",
  "Descripcion": "Factura por servicios de consultoría y licencia.",
  "Contacto": "Elena Gómez",
  "Telefono": "915551234",
  "eMail": "elena.gomez@ejemplo.com",
  "Lineas": [
    {
      "Orden": 1,
      "TipoDeLinea": "PartidaAlzada",
      "Concepto": "Licencia anual de software de gestión (QLIK)",
      "Cantidad": 1.00,
      "Precio": 1250.00,
      "Anotacion": "Licencia del 01/01 al 31/12",
      "Descuento": 0.00,
      "Iva": "21",
      "Irpf": "0",
      "Unidad": "Ud",
      "Naturaleza": "SER"
    },
    {
      "Orden": 2,
      "TipoDeLinea": "PartidaAlzada",
      "Concepto": "Servicio de consultoría e implementación",
      "Cantidad": 20.00,
      "Precio": 85.00,
      "Anotacion": "20 horas a 85€/hora",
      "Descuento": 0.00,
      "Iva": "21",
      "Irpf": "15",
      "Unidad": "H",
      "Naturaleza": "SER"
    },
    {
      "Orden": 3,
      "TipoDeLinea": "Comentario",
      "Concepto": "NOTA: Todos los precios son sin IVA.",
      "Cantidad": null,
      "Precio": null,
      "Anotacion": null,
      "Descuento": null,
      "Iva": null,
      "Irpf": null,
      "Unidad": null,
      "Naturaleza": null
    }
  ]
}
 */