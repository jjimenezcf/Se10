using Gestor.Errores;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Gastos;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.Terceros;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Utilidades;
using static ServicioDeDatos.Ventas.ParametrosDelSii;

namespace ServicioDeDatos.Ventas
{

    public enum enumEtapasDeFacturasEmt
    {
        [Description("Estados en los que una factura aun no es firme, no está emitida")]
        FAE_Etapa_Prefactura,
        [Description("Estados en los que una factura ya se ha emitido")]
        FAE_Etapa_Emitida,
        [Description("Estados en los que una factura puede ser cobrada")]
        FAE_Etapa_De_Cobro,
        [Description("Estados en los que una factura puede ser reclamada o está en reclamación")]
        FAE_Etapa_De_Reclamacion,
        [Description("Estados en los que una factura no es cobrable")]
        FAE_Etapa_No_Cobrable,
        [Description("Estados en los que una factura no es cobrable")]
        FAE_Etapa_Cobrada,
        [Description("Estados en los que una factura se ha cobrado")]
        FAE_Etapa_Anulada,
        [Description("Estados en los que una factura está parcialmente cobrada")]
        FAE_Etapa_Pago_Parcial,
        [Description("Estados en los que una factura está rectificada")]
        FAE_Etapa_Rectificada,
        [Description("Estados en los que una factura está remesada")]
        FAE_Etapa_Remesada,
        [Description("Estados en los que una factura está devuelta una vez remesada")]
        FAE_Etapa_Devuelta,
        [Description("Estados en los que una rectificativa está abonada")]
        FAE_Etapa_Abonada
    }

    public enum enumParametrosDeFacturasEmt
    {
        [Description("Indica para un motivo y un estado de la factura que transición se le aplica")]
        FAE_Aplicar_Transicion,
        [Description("Indica el texto a añadir a la factura en el SII y la emisión por impago")]
        FAE_REC_Texto_Por_Impago,
        [Description("Indica el texto a añadir a la factura en el SII y la emisión por diferencia")]
        FAE_REC_Texto_Por_Diferencia,
        [Description("Indica el texto a añadir a la factura en el SII y la emisión por errores")]
        FAE_REC_Texto_Por_Errores,
        [Description("Indica la tolerancia de conciliación entre una factura y los cobros totales")]
        FAE_ToleranciaAlConciliar,
        [Description("Indica si se pueden hacer restificativas parciales")]
        FAE_PermiteRectificativasParciales,
        [Description("Indica la URL para generar QR de una factura")]
        FAE_URL_GenerarQR,
        [Description("Indica la URL para generar QR de una factura que no usa Verifactu")]
        FAE_URL_GenerarQRNoVerifactu,
        [Description("Indica el tipo de archivo a usar para la reclamación de facturas")]
        FAE_TipoArchivadorDeReclamacion,
        [Description("Json con los datos de impresión")]
        FAE_DatosDeImpresion,
        [Description("Indica la serie por defecto a usar para el año anterior según el tipo")]
        FAE_Serie_Por_Tipo_Anterior,
        [Description("Indica para un id de tipo factura que id de tipo de preasiento se ha de usar")]
        FAE_Relacion_Con_Tipo_De_Preasiento,
        [Description("Indica para un id de tipo factura que id de tipo de pago se ha de usar")]
        FAE_Relacion_Con_Tipo_De_Pago,
        [Description("Indica el id de tipo de preasiento para un cobro de una factura")]
        COB_Relacion_Con_Tipo_De_Preasiento,
        [Description("Indica si el sii está activo en la emisión de facturas")]
        FAE_SII_Activo,
        [Description("Indica si el sii, para la sociedad indicada, está en producción")]
        FAE_SII_En_Productivo,
        [Description("Indica si hay que firmar el registro de una sociedad")]
        FAE_SII_Firmar,
        [Description("Indica el id del archivo que almacena la declaración responsable")]
        FAE_SII_Declaracion_Responsable,
        [Description("Indica el nombre del programa usado para implementar el SIF")]
        SII_SISTEMA_INFORMATICO,
        [Description("Indica la razón social")]
        SII_SSII_RAZON_SOCIAL,
        [Description("Indica el NIF de la empresa")]
        SII_SSII_NIF,
        [Description("Indica el nº de implantación")]
        SII_SSII_IMPLANTACION,
        [Description("Indica la Url de la AEAT para registrar la factura")]
        SII_URL_DE_VERIFICACION
    }

    public static class EndpointsDe
    {
        /// <summary>
        /// Prefijo del endpoint de pruebas.
        /// </summary>
        public const string Test = @"https://prewww1.aeat.es/wlpl/TIKE-CONT/ws/SistemaFacturacion/VerifactuSOAP";

        /// <summary>
        /// Prefijo del endpoint de producción.
        /// </summary>
        public const string Prod = @"https://www1.agenciatributaria.gob.es/wlpl/TIKE-CONT/ws/SistemaFacturacion/VerifactuSOAP";

        /// <summary>
        /// Prefijo del endpoint de pruebas de validación.
        /// </summary>
        public const string TestValidate = @"https://prewww2.aeat.es/wlpl/TIKE-CONT/ValidarQR";

        /// <summary>
        /// Prefijo del endpoint de producción de validación.
        /// </summary>
        public const string ProdValidate = @"https://www2.agenciatributaria.gob.es/wlpl/TIKE-CONT/ValidarQR";

        /// <summary>
        /// Prefijo del endpoint de pruebas de validación no verifactu.
        /// </summary>
        public const string TestNoVerifactu = @"https://prewww2.aeat.es/wlpl/TIKE-CONT/ValidarQRNoVerifactu";

        /// <summary>
        /// Prefijo del endpoint de producción de validación no verifactu.
        /// </summary>
        public const string ProdNoVerifactu = @"https://www2.agenciatributaria.gob.es/wlpl/TIKE-CONT/ValidarQRNoVerifactu";
    }

    public static class ParametrosDelSii
    {
        private static readonly string _jsonDeSSII_Implantacion = "[{\"IdSociedad\": 0,\"Valor\": \"BD-nif-id\"}]";

        public static class ltrParametrosDelSii
        {
            public static string Pendiente = _jsonDeSSII_Implantacion;
        }

        public static string SII_UrlDeRegistro => enumNegocio.FacturaEmitida.Parametro(enumParametrosDeFacturasEmt.SII_URL_DE_VERIFICACION
            , crearParametro: true
            , valorPorDefecto: EndpointsDe.Test).Valor;

        public static string SII_URLDeValidarQr => enumNegocio.FacturaEmitida.Parametro(enumParametrosDeFacturasEmt.FAE_URL_GenerarQR
            , crearParametro: true
            , valorPorDefecto: EndpointsDe.TestValidate).Valor;

        public static string SII_URLDeValidarQrNoVerifactu => enumNegocio.FacturaEmitida.Parametro(enumParametrosDeFacturasEmt.FAE_URL_GenerarQRNoVerifactu
            , crearParametro: true
            , valorPorDefecto: EndpointsDe.TestNoVerifactu).Valor;

        public static string SSII_Implantacion => enumNegocio.FacturaEmitida.Parametro(enumParametrosDeFacturasEmt.SII_SSII_IMPLANTACION, crearParametro: true, valorPorDefecto: ltrParametrosDelSii.Pendiente).Valor;
        public static string SSII_NIF => enumNegocio.FacturaEmitida.Parametro(enumParametrosDeFacturasEmt.SII_SSII_NIF, crearParametro: true, valorPorDefecto: "B02682953").Valor;
        public static string SSII_RazonSocial => enumNegocio.FacturaEmitida.Parametro(enumParametrosDeFacturasEmt.SII_SSII_RAZON_SOCIAL, crearParametro: true, valorPorDefecto: "FEMDEK SOLUTIONS, S.L.").Valor;
        public static string SSII_NombreDelSistema => enumNegocio.FacturaEmitida.Parametro(enumParametrosDeFacturasEmt.SII_SISTEMA_INFORMATICO, crearParametro: true, valorPorDefecto: "SistemaDeElementos").Valor;
    }

    public class SeriesPorTipo
    {
        public int IdTipo { get; set; }
        public string Serie { get; set; }
    }

    public class RelacionDeTiposFaeSpr
    {
        public int IdTipoFactura { get; set; }
        public int IdTipoPreasiento { get; set; }
    }

    public class RelacionDeTiposFaePago
    {
        public int IdTipoFactura { get; set; }
        public int IdTipoPago { get; set; }
    }


    public class SSII_Implantacion
    {
        public int IdSociedad { get; set; }
        public string Valor { get; set; }
    }

    public class SSII_EnProductivo
    {
        public int IdSociedad { get; set; }
        public string Valor { get; set; }
    }

    public class SII_Firmar
    {
        public int IdSociedad { get; set; }
        public string Valor { get; set; }
    }

    public static class VariableDeFacturasEmt
    {

        internal static readonly string IA_Reglas_de_filtrado = @"
### R.FacturasVenta.1 · Cliente, Deudor o Identificación (`FiltrarPorNombreCliente`)
- **Disparador:** Facturas ""del cliente [Nombre]"", ""emitidas a [Empresa]"", ""compradas por [NIF/CIF]"", ""que debe [Nombre]"", ""del deudor [DNI]"".
- **Acción:** Genera un único objeto:
  1. `{""Clausula"": ""FiltrarPorNombreCliente"", ""Criterio"": ""contiene"", ""Valor"": ""busqueda""}`
- **Lógica de Valor:** - Si el usuario indica un **Nombre**, **Apellidos** o **Razón Social**, extrae el texto para una búsqueda parcial.
  - Si el usuario indica un **DNI**, **NIF** o **CIF**, extrae el identificador exacto (el sistema validará la coincidencia total).
- **Ejemplo de Salida:**
  - ""Facturas de Juan Pérez"" → `{""Clausula"": ""FiltrarPorNombreCliente"", ""Criterio"": ""contiene"", ""Valor"": ""Juan Pérez""}`
  - ""Facturas del CIF B12345678"" → `{""Clausula"": ""FiltrarPorNombreCliente"", ""Criterio"": ""contiene"", ""Valor"": ""B12345678""}`

### R.FacturasVenta.2 · Importes y Cobros**
**R.FacturasVenta.2.1 · Importe Total con IVA (`filtroporimportesiniva`)**
- **Disparador:** Facturas ""de entre [Monto] y [Monto] euros"", ""por más de [Monto]€"", ""con un total de [Monto]"", ""que sumen [Monto]"".
- **Acción:** `{""Clausula"": ""filtroporimportesiniva"", ""Criterio"": ""entreImportes"", ""Valor"": ""min;max""}`
- **Nota:** En caso de un solo importe: ""más de [X]"" → `X`; ""menos de [X]"" → `0;X`.

**R.FacturasVenta.2.2 · Importe Cobrado (`filtroporcobrado`)**
- **Disparador:** Facturas ""con un cobro de [Monto]"", ""donde se han cobrado [Monto]"", ""entre [Monto] y [Monto] cobrados"", ""sin cobros"", ""no cobradas"", ""con algún cobro"", ""ya cobradas parcial o totalmente"".
- **Acción:** `{""Clausula"": ""filtroporcobrado"", ""Criterio"": ""entreImportes"", ""Valor"": ""min;max""}`
- **Casos Especiales de Negocio:**
  - ""Sin cobro"", ""no cobradas"" o ""con cobro 0"" → `{""Clausula"": ""filtroporcobrado"", ""Criterio"": ""entreImportes"", ""Valor"": ""0;0""}`
  - ""Con algún cobro"", ""que tengan cobros"" o ""ya empezadas a cobrar"" → `{""Clausula"": ""filtroporcobrado"", ""Criterio"": ""entreImportes"", ""Valor"": ""0.01;""}`
  - ""Más de [X]"" → `X;`
  - ""Menos de [X]"" → `0;X`

### R.FacturasVenta.3 · Fechas Críticas**

**R.FacturasVenta.3.1 · Fecha de Emisión (`filtroporfechadeemision`)**
- **Disparador:** Facturas ""emitidas el [Fecha]"", ""hechas en [Periodo]"", ""con fecha de factura de [Fecha]"".
- **Acción:** `{""Clausula"": ""filtroporfechadeemision"", ""Criterio"": ""entreFechas"", ""Valor"": ""YYYY-MM-DDTHH:mm:ssZ-YYYY-MM-DDTHH:mm:ssZ""}`

**R.FacturasVenta.3.2 · Fecha de Vencimiento (`filtroporfechadevencimiento`)**
- **Disparador:** Facturas ""que vencen el [Fecha]"", ""con vencimiento en [Periodo]"", ""que caducan el [Fecha]"".
- **Acción:** `{""Clausula"": ""filtroporfechadevencimiento"", ""Criterio"": ""entreFechas"", ""Valor"": ""YYYY-MM-DDTHH:mm:ssZ-YYYY-MM-DDTHH:mm:ssZ""}`

### R.FacturasVenta.4 · Numeración y Series (`filtropornumerosdefactura`)**
- **Disparador:** Facturas ""desde la número [X] hasta la [Y]"", ""en el rango de números [Rango]"", ""entre la factura [Inicio] y la [Fin]"".
- **Acción:** Genera un objeto con el formato exacto requerido por el sistema:
  1. `{""Clausula"": ""filtropornumerosdefactura"", ""Criterio"": ""entreRangos"", ""Valor"": ""yyyy-serie-numeroDesde;yyyy-serie-numeroHasta""}`
- **Reglas de Formato (IMPORTANTE):**
  - Usa el punto y coma (`;`) para separar el inicio del fin.
  - Usa el guion (`-`) para separar Año, Serie y Número.
  - Si falta uno de los dos, usa `undefined` (ej: `2024-A-001;undefined`).
- **Ejemplo:** ""desde la 2024-A-001 hasta la 2024-A-010"" → **Valor:** `2024-A-001;2024-A-010`.


### R.FacturasVenta.5 · Relación con Presupuestos (PPT)
**R.FacturasVenta.5.1 · Búsqueda por datos de Presupuesto (`NombrePresupuesto`)**
- **Disparador:** Facturas ""del presupuesto [Nombre/Referencia]"", ""vinculadas al PPT [X]"", ""que pertenecen al presupuesto [Y]"", ""del presupuesto con número [Referencia]"".
- **Acción:** Genera el objeto:
  1. `{""Clausula"": ""NombrePresupuesto"", ""Criterio"": ""contiene"", ""Valor"": ""nombre_o_referencia""}`
- **Nota:** Esta regla busca coincidencias parciales en el nombre o la referencia de los presupuestos vinculados a la factura.

**R.FacturasVenta.5.2 · Situación: Facturas con cualquier Presupuesto (`AsociadaAUnPpt`)**
- **Disparador:** Facturas ""con presupuesto"", ""que tengan PPT"", ""vinculadas a algún presupuesto"", ""facturas presupuestadas"".
- **Acción:** Genera un objeto:
  1. `{""Clausula"": ""AsociadaAUnPpt"", ""Criterio"": ""igual"", ""Valor"": ""5""}` (Representa ConRelacion).

**R.FacturasVenta.5.3 · Situación: Facturas sin Presupuesto (`AsociadaAUnPpt`)**
- **Disparador:** Facturas ""sin presupuesto"", ""que no tengan PPT"", ""no presupuestadas"", ""pendientes de asociar a presupuesto"".
- **Acción:** Genera un objeto:
  1. `{""Clausula"": ""AsociadaAUnPpt"", ""Criterio"": ""igual"", ""Valor"": ""6""}` (Representa SinRelacion).

**Jerarquía:** Si se detecta una búsqueda por nombre o número (R.5.1), se omiten las reglas de situación (R.5.2 y R.5.3).

### R.FacturasVenta.6 · Planificaciones de Venta (PLV)
**R.FacturasVenta.6.1 · Búsqueda por datos de Planificación (`NombrePlfDeVenta`)**
- **Disparador:** Facturas ""de la planificación [Nombre/Referencia]"", ""vinculadas a la PLV [X]"", ""que pertenecen a la planificación de venta [Y]"".
- **Acción:** Genera el objeto:
  1. `{""Clausula"": ""NombrePlfDeVenta"", ""Criterio"": ""contiene"", ""Valor"": ""nombre_o_referencia""}`
- **Nota:** Esta regla busca facturas asociadas a planificaciones cuyo nombre o referencia coincida parcialmente con el texto.

**R.FacturasVenta.6.2 · Situación: Facturas con Planificación (`AsociadaAUnaPlv`)**
- **Disparador:** Facturas ""planificadas"", ""con planificación de venta"", ""vinculadas a alguna PLV"", ""asociadas a una planificación"".
- **Acción:** Genera un objeto:
  1. `{""Clausula"": ""AsociadaAUnaPlv"", ""Criterio"": ""igual"", ""Valor"": ""5""}` (Representa ConRelacion).

**R.FacturasVenta.6.3 · Situación: Facturas sin Planificación (`AsociadaAUnaPlv`)**
- **Disparador:** Facturas ""sin planificar"", ""sin planificación de venta"", ""no vinculadas a PLV"", ""pendientes de planificar"".
- **Acción:** Genera un objeto:
  1. `{""Clausula"": ""AsociadaAUnaPlv"", ""Criterio"": ""igual"", ""Valor"": ""6""}` (Representa SinRelacion).

**Jerarquía:** Si se detecta una búsqueda por nombre o número de PLV (R.6.1), se omiten las reglas de situación (R.6.2 y R.6.3).

### R.FacturasVenta.7 · Relación con Partes de Trabajo (PTR)

**R.FacturasVenta.7.1 · Búsqueda por datos de Parte de Trabajo (`NombreParteTr`)**
- **Disparador:** Facturas ""del parte [Nombre/Referencia]"", ""vinculadas al PTR [X]"", ""del parte de trabajo [Y]"", ""facturas del parte con número [Referencia]"".
- **Acción:** Genera el objeto:
  1. `{""Clausula"": ""NombreParteTr"", ""Criterio"": ""contiene"", ""Valor"": ""nombre_o_referencia""}`
- **Nota:** Esta regla busca coincidencias parciales en el nombre o la referencia de los partes de trabajo vinculados, ya sea en la cabecera de la factura o en sus líneas.

**R.FacturasVenta.7.2 · Situación: Facturas con cualquier Parte de Trabajo (`AsociadaAUnPtr`)**
- **Disparador:** Facturas ""con parte de trabajo"", ""que tengan PTR"", ""vinculadas a algún parte"", ""facturas con partes"".
- **Acción:** Genera un objeto:
  1. `{""Clausula"": ""AsociadaAUnPtr"", ""Criterio"": ""igual"", ""Valor"": ""5""}` (Representa ConRelacion).

**R.FacturasVenta.7.3 · Situación: Facturas sin Parte de Trabajo (`AsociadaAUnPtr`)**
- **Disparador:** Facturas ""sin parte de trabajo"", ""que no tengan PTR"", ""no asociadas a partes"", ""pendientes de asignar parte"".
- **Acción:** Genera un objeto:
  1. `{""Clausula"": ""AsociadaAUnPtr"", ""Criterio"": ""igual"", ""Valor"": ""6""}` (Representa SinRelacion).

**Jerarquía:** Si se detecta una búsqueda por nombre o número (R.7.1), se omiten las reglas de situación (R.7.2 y R.7.3).

### R.FacturasVenta.8 · Facturas Rectificadas (`Rectificadas`)

**R.FacturasVenta.8.1 · Búsqueda por datos de Rectificativa (`NombreRectificativa`)**
- **Disparador:** Facturas ""rectificadas por [Nombre/Referencia]"", ""cuya factura rectificativa es la [Año-Serie-Número]"", ""asociadas a la corrección [Nombre]"".
- **Acción:** Genera el objeto:
  1. `{""Clausula"": ""NombreRectificativa"", ""Criterio"": ""contiene"", ""Valor"": ""busqueda""}`
- **Lógica de Valor:** - Puede ser el **Nombre** o **Referencia** de la factura que emite la rectificación.
  - Si el usuario indica una numeración, debe usar el formato `YYYY-Serie-Número` (ej: `2024-A-101`) para que el sistema pueda desglosarlo correctamente.
- **Nota:** Este filtro busca las facturas originales que han sido objeto de una rectificación por parte de la factura indicada.

**R.FacturasVenta.8.2 · Situación: Facturas con Rectificación (`Rectificadas`)**
- **Disparador:** Facturas ""rectificadas"", ""con rectificativa"", ""que tengan relación de rectificación"", ""ya corregidas"".
- **Acción:** Genera el objeto:
  1. `{""Clausula"": ""Rectificadas"", ""Criterio"": ""igual"", ""Valor"": ""5""}` 
  2. `{""Clausula"": ""quemostrar"", ""Criterio"": ""igual"", ""Valor"": ""8""}`
(Representa ConRelacion).

**R.FacturasVenta.8.3 · Situación: Facturas SIN Rectificación (`Rectificadas`)**
- **Disparador:** Facturas ""no rectificadas"", ""sin rectificativa"", ""facturas limpias (sin rectificar)"", ""que no se hayan corregido"".
- **Acción:** Genera el objeto:
  1. `{""Clausula"": ""Rectificadas"", ""Criterio"": ""igual"", ""Valor"": ""6""}` (Representa SinRelacion).

**Jerarquía:** Si se detecta una búsqueda por nombre, referencia o número específico (R.8.1), se omiten las reglas de situación (R.8.2 y R.8.3).
";

        private const string _jsonDeFAE_Serie_Por_Tipo_Anterior = "[{\"IdTipo\": 0,\"Serie\": \"A\"}]";
        private const string _jsonDeTipoDePreasientoDeFae = "[{\"IdTipoFactura\": 0,\"IdTipoPreasiento\": 0}]";
        private const string _jsonDeTipoDePagoDeFae = "[{\"IdTipoFactura\": 0,\"IdTipoPago\": 0}]";
        private static readonly string _jsonDeFAE_SII_Firmar = "[{\"IdSociedad\": 0,\"Valor\": \"?\"}]";
        private static readonly string _jsonDeFAE_SSII_EnProductivo = "[{\"IdSociedad\": 0,\"Valor\": \"?\"}]";

        private static string etapaPrefactura => enumNegocio.FacturaEmitida.Parametro(enumEtapasDeFacturasEmt.FAE_Etapa_Prefactura)?.Valor ?? null;
        private static string etapaDeEmitida => enumNegocio.FacturaEmitida.Parametro(enumEtapasDeFacturasEmt.FAE_Etapa_Emitida)?.Valor ?? null;
        private static string etapaDeCobro => enumNegocio.FacturaEmitida.Parametro(enumEtapasDeFacturasEmt.FAE_Etapa_De_Cobro)?.Valor ?? null;
        private static string etapaDeReclamacion => enumNegocio.FacturaEmitida.Parametro(enumEtapasDeFacturasEmt.FAE_Etapa_De_Reclamacion)?.Valor ?? null;
        private static string etapaNoCobrable => enumNegocio.FacturaEmitida.Parametro(enumEtapasDeFacturasEmt.FAE_Etapa_No_Cobrable)?.Valor ?? null;
        private static string etapaCobrada => enumNegocio.FacturaEmitida.Parametro(enumEtapasDeFacturasEmt.FAE_Etapa_Cobrada)?.Valor ?? null;
        private static string etapaRectificada => enumNegocio.FacturaEmitida.Parametro(enumEtapasDeFacturasEmt.FAE_Etapa_Rectificada)?.Valor ?? null;
        private static string etapaRemesada => enumNegocio.FacturaEmitida.Parametro(enumEtapasDeFacturasEmt.FAE_Etapa_Remesada)?.Valor ?? null;
        private static string etapaDevuelta => enumNegocio.FacturaEmitida.Parametro(enumEtapasDeFacturasEmt.FAE_Etapa_Devuelta)?.Valor ?? null;
        private static string etapaAnulada => enumNegocio.FacturaEmitida.Parametro(enumEtapasDeFacturasEmt.FAE_Etapa_Anulada)?.Valor ?? null;
        private static string etapaDeAbonada => enumNegocio.FacturaEmitida.Parametro(enumEtapasDeFacturasEmt.FAE_Etapa_Abonada)?.Valor ?? null;
        private static string etapaPagoParcial
        {
            get
            {
                var enPago = enumEtapasDeFacturasEmt.FAE_Etapa_De_Cobro.Lista();
                var emitida = enumEtapasDeFacturasEmt.FAE_Etapa_Emitida.Lista();
                var reclamada = enumEtapasDeFacturasEmt.FAE_Etapa_De_Reclamacion.Lista();
                foreach (var estado in emitida) if (enPago.Contains(estado)) enPago.Remove(estado);
                foreach (var estado in reclamada) if (enPago.Contains(estado)) enPago.Remove(estado);
                return string.Join(", ", enPago);
            }
        }

        public static decimal ToleranciaDeCobro => ToleranciaAlConciliar();
        public static bool PermiteRectificativasParciales => PermitirRectificativasParciales();

        public enum enumMotivoTransicion { AnularPago, VenceFactura, ModificarVencimiento, RealizarPagoTotal, RealizarPagoParcial, RectificarFactura, RemesarFactura, DesremesarFactura, DevolverPagoRemesado, AbonarPagoRemesado, AnularPresentacionDeRemesa };

        public static string TextoPorImpago => enumParametrosDeFacturasEmt.FAE_Aplicar_Transicion.LeerVariable("Conforme el artículo 80 de la Ley del IVA se anula la cuota de IVA de la factura original [NumeroDeFactura] emitida el [FacturadaEl]");

        public static string TransicionesPorMotivo => enumNegocio.FacturaEmitida.Parametro(enumParametrosDeFacturasEmt.FAE_Aplicar_Transicion).Valor;

        public static string Estados(this enumEtapasDeFacturasEmt etapa)
        {
            string estados = null;
            switch (etapa)
            {
                case enumEtapasDeFacturasEmt.FAE_Etapa_Prefactura: estados = etapaPrefactura; break;
                case enumEtapasDeFacturasEmt.FAE_Etapa_Emitida: estados = etapaDeEmitida; break;
                case enumEtapasDeFacturasEmt.FAE_Etapa_De_Cobro: estados = etapaDeCobro; break;
                case enumEtapasDeFacturasEmt.FAE_Etapa_De_Reclamacion: estados = etapaDeReclamacion; break;
                case enumEtapasDeFacturasEmt.FAE_Etapa_No_Cobrable: estados = etapaNoCobrable; break;
                case enumEtapasDeFacturasEmt.FAE_Etapa_Cobrada: estados = etapaCobrada; break;
                case enumEtapasDeFacturasEmt.FAE_Etapa_Rectificada: estados = etapaRectificada; break;
                case enumEtapasDeFacturasEmt.FAE_Etapa_Remesada: estados = etapaRemesada; break;
                case enumEtapasDeFacturasEmt.FAE_Etapa_Anulada: estados = etapaAnulada; break;
                case enumEtapasDeFacturasEmt.FAE_Etapa_Abonada: estados = etapaDeAbonada; break;
                case enumEtapasDeFacturasEmt.FAE_Etapa_Pago_Parcial: estados = etapaPagoParcial; break;
                case enumEtapasDeFacturasEmt.FAE_Etapa_Devuelta: estados = etapaDevuelta; break;

            }
            return estados.IsNullOrEmpty() ? enumNegocio.FacturaEmitida.DefinirEtapaSiLoIndicaConfiguracion(etapa, ltrEstados.EstadoNulo) : estados;
        }

        public static bool EstaEnLaEtapa(this FacturaEmtDtm factura, enumEtapasDeFacturasEmt etapa) => etapa.Lista().Contains(factura.IdEstado);

        public static bool ContieneLaEtapa(this List<enumEtapasDeFacturasEmt> etapas, enumEtapasDeFacturasEmt etapa) => etapas.Contains(etapa);

        public static bool EstaEnAlgunaDeLasEtapa(this FacturaEmtDtm factura, List<enumEtapasDeFacturasEmt> etapas)
        {
            var etapasDeLaFactura = factura.Etapas();
            foreach (var etapa in etapas)
                if (etapasDeLaFactura.Contains(etapa)) return true;
            return false;
        }

        public static bool EstaEnEtapaNoContabilizada(this FacturaEmtDtm factura)
        {
            return factura.EstaEnAlgunaDeLasEtapa(new List<enumEtapasDeFacturasEmt> {
                enumEtapasDeFacturasEmt.FAE_Etapa_Prefactura,
                enumEtapasDeFacturasEmt.FAE_Etapa_Anulada});
        }
        public static (List<int> estados, enumEtapasDeFacturasEmt etapa) EstadosDeLaEtapa(this enumEtapasDeFacturasEmt etapa) => (etapa.Lista(), etapa);

        public static List<int> Lista(this enumEtapasDeFacturasEmt etapa) => etapa.Estados().ToLista<int>(Simbolos.Coma);

        public static List<enumEtapasDeFacturasEmt> Etapas(this FacturaEmtDtm factura)
        {
            var etapas = new List<enumEtapasDeFacturasEmt>();
            if (factura.EstaEnLaEtapa(enumEtapasDeFacturasEmt.FAE_Etapa_Prefactura))
                etapas.Add(enumEtapasDeFacturasEmt.FAE_Etapa_Prefactura);
            if (factura.EstaEnLaEtapa(enumEtapasDeFacturasEmt.FAE_Etapa_Emitida))
                etapas.Add(enumEtapasDeFacturasEmt.FAE_Etapa_Emitida);
            if (factura.EstaEnLaEtapa(enumEtapasDeFacturasEmt.FAE_Etapa_De_Cobro))
                etapas.Add(enumEtapasDeFacturasEmt.FAE_Etapa_De_Cobro);
            if (factura.EstaEnLaEtapa(enumEtapasDeFacturasEmt.FAE_Etapa_Pago_Parcial))
                etapas.Add(enumEtapasDeFacturasEmt.FAE_Etapa_Pago_Parcial);
            if (factura.EstaEnLaEtapa(enumEtapasDeFacturasEmt.FAE_Etapa_De_Reclamacion))
                etapas.Add(enumEtapasDeFacturasEmt.FAE_Etapa_De_Reclamacion);
            if (factura.EstaEnLaEtapa(enumEtapasDeFacturasEmt.FAE_Etapa_No_Cobrable))
                etapas.Add(enumEtapasDeFacturasEmt.FAE_Etapa_No_Cobrable);
            if (factura.EstaEnLaEtapa(enumEtapasDeFacturasEmt.FAE_Etapa_Cobrada))
                etapas.Add(enumEtapasDeFacturasEmt.FAE_Etapa_Cobrada);
            if (factura.EstaEnLaEtapa(enumEtapasDeFacturasEmt.FAE_Etapa_Rectificada))
                etapas.Add(enumEtapasDeFacturasEmt.FAE_Etapa_Rectificada);
            if (factura.EstaEnLaEtapa(enumEtapasDeFacturasEmt.FAE_Etapa_Remesada))
                etapas.Add(enumEtapasDeFacturasEmt.FAE_Etapa_Remesada);
            if (factura.EstaEnLaEtapa(enumEtapasDeFacturasEmt.FAE_Etapa_Devuelta))
                etapas.Add(enumEtapasDeFacturasEmt.FAE_Etapa_Devuelta);
            if (factura.EstaEnLaEtapa(enumEtapasDeFacturasEmt.FAE_Etapa_Anulada))
                etapas.Add(enumEtapasDeFacturasEmt.FAE_Etapa_Abonada);
            if (factura.EstaEnLaEtapa(enumEtapasDeFacturasEmt.FAE_Etapa_Abonada))
                etapas.Add(enumEtapasDeFacturasEmt.FAE_Etapa_Abonada);
            return etapas;
        }

        public static string CadenaDeEtapas(this FacturaEmtDtm factura) => string.Join(Simbolos.separadorDeEtapas, factura.Etapas());

        public static enumEtapasDeFacturasEmt Etapa(this FacturaEmtDtm factura)
        {
            var etapas = factura.Etapas();
            if (etapas.Count == 0)
                throw new Exception($"No se ha definido la etapa de la {enumNegocio.FacturaEmitida.Singular(true)}, " +
                    $"cuando éste está en el estado {factura.Propiedad<EstadoDtm>(typeof(EstadoDeUnaFacturaEmtDtm)).Nombre}");
            if (etapas.Count > 1)
                throw new Exception($"El estado de la factura {enumNegocio.FacturaEmitida.Singular(true)} '{factura.Referencia}' " +
                    $"se encuentra en las etapas {string.Join(',', etapas)} y sólo ha de estar en una");
            return etapas[0];
        }

        public static string Nombre(this enumEtapasDeFacturasEmt etapa, bool minusculas)
        {
            switch (etapa)
            {
                case enumEtapasDeFacturasEmt.FAE_Etapa_Prefactura: return minusculas ? "prefacturación" : "Prefacturación";
                case enumEtapasDeFacturasEmt.FAE_Etapa_Emitida: return minusculas ? "facturada" : "Facturada";
                case enumEtapasDeFacturasEmt.FAE_Etapa_De_Cobro: return minusculas ? "pendiente" : "Pendiente";
                case enumEtapasDeFacturasEmt.FAE_Etapa_De_Reclamacion: return minusculas ? "en reclamación" : "En reclamación";
                case enumEtapasDeFacturasEmt.FAE_Etapa_No_Cobrable: return minusculas ? "no cobrable" : "No cobrable";
                case enumEtapasDeFacturasEmt.FAE_Etapa_Cobrada: return minusculas ? "cobrada" : "Cobrada";
                case enumEtapasDeFacturasEmt.FAE_Etapa_Rectificada: return minusculas ? "rectificada" : "Rectificada";
                case enumEtapasDeFacturasEmt.FAE_Etapa_Remesada: return minusculas ? "remesada" : "Remesada";
                case enumEtapasDeFacturasEmt.FAE_Etapa_Devuelta: return minusculas ? "devuelta" : "Devuelta";
                case enumEtapasDeFacturasEmt.FAE_Etapa_Abonada: return minusculas ? "abonada" : "Abonada";
                case enumEtapasDeFacturasEmt.FAE_Etapa_Anulada: return minusculas ? "anulada" : "Anulada";
                case enumEtapasDeFacturasEmt.FAE_Etapa_Pago_Parcial: return minusculas ? "parcialmente cobrada" : "Parcialmente cobrada";
            }
            return etapa.ToString();
        }

        internal static decimal ToleranciaAlConciliar()
        {
            var tolerancia = enumNegocio.FacturaEmitida.Parametro(enumParametrosDeFacturasEmt.FAE_ToleranciaAlConciliar, emitirError: false);
            if (tolerancia is null)
            {
                ParametroDeNegocioSql.Crear(NegocioSqls.LeerNegocioPorEnumerado(enumNegocio.FacturaEmitida).Id, enumParametrosDeFacturasEmt.FAE_ToleranciaAlConciliar, 0.01M.ToString());
                return 0.01M;
            }

            return Convert.ToDecimal(tolerancia.Valor);
        }

        internal static bool PermitirRectificativasParciales()
        {
            var permitir = enumNegocio.FacturaEmitida.Parametro(enumParametrosDeFacturasEmt.FAE_PermiteRectificativasParciales, emitirError: false);
            if (permitir is null)
            {
                ParametroDeNegocioSql.Crear(NegocioSqls.LeerNegocioPorEnumerado(enumNegocio.FacturaEmitida).Id, enumParametrosDeFacturasEmt.FAE_PermiteRectificativasParciales, "N");
                return false;
            }

            return permitir.Valor.EsTrue();
        }


        public static string URL_GenerarQR()
        {
            var url = enumNegocio.FacturaEmitida.Parametro(enumParametrosDeFacturasEmt.FAE_URL_GenerarQR, emitirError: false);
            if (url is null)
            {
                string variable = "https://prewww2.aeat.es/wlpl/TIKE-CONT/ValidarQR?";
                ParametroDeNegocioSql.Crear(NegocioSqls.LeerNegocioPorEnumerado(enumNegocio.FacturaEmitida).Id, enumParametrosDeFacturasEmt.FAE_URL_GenerarQR, variable);
                return variable;
            }

            return url.Valor;
        }


        public static string SerieAnoAnterior(this ITipoDeElementoDtm tipo, ContextoSe contexto)
        {
            var json = enumNegocio.FacturaEmitida.Parametro(enumParametrosDeFacturasEmt.FAE_Serie_Por_Tipo_Anterior, crearParametro: true, valorPorDefecto: _jsonDeFAE_Serie_Por_Tipo_Anterior).Valor;

            var relaciones = ParsearSeries(json);
            var relacion = relaciones.FirstOrDefault(x => x.IdTipo == tipo.Id);
            if (relacion == null)
                GestorDeErrores.Emitir($"Ha de configurar el parámetro '{enumParametrosDeFacturasEmt.FAE_Serie_Por_Tipo_Anterior}' para el id de tipo de factura '({tipo.Id}) {tipo.Nombre}'");

            ValidarSerie(contexto, relacion.Serie);
            return relacion.Serie;
        }

        public static void ValidarSerie(this ITipoDeElementoDtm tipo, ContextoSe contexto)
        {
            var json = enumNegocio.FacturaEmitida.Parametro(enumParametrosDeFacturasEmt.FAE_Serie_Por_Tipo_Anterior, crearParametro: true, valorPorDefecto: _jsonDeFAE_Serie_Por_Tipo_Anterior).Valor;

            var relaciones = ParsearSeries(json);
            var relacion = relaciones.FirstOrDefault(x => x.Serie == ((TipoDeFacturaEmtDtm)tipo).Serie);
            if (relacion != null)
                GestorDeErrores.Emitir($"La serie '{((TipoDeFacturaEmtDtm)tipo).Serie}' está configurada en el parámetro '{enumParametrosDeFacturasEmt.FAE_Serie_Por_Tipo_Anterior}' " +
                    $"para el tipo '{contexto.Set<TipoDeFacturaEmtDtm>().FirstOrDefault(x => x.Id == relacion.IdTipo)?.Nombre ?? $"id no definido: {relacion.IdTipo}"}', use otra serie");
        }


        private static void ValidarSerie(ContextoSe contexto, string serie)
        {
            var json = enumNegocio.FacturaEmitida.Parametro(enumParametrosDeFacturasEmt.FAE_Serie_Por_Tipo_Anterior, crearParametro: true, valorPorDefecto: _jsonDeFAE_Serie_Por_Tipo_Anterior).Valor;

            var tipo = contexto.Set<TipoDeFacturaEmtDtm>().FirstOrDefault(x => x.Serie == serie);
            if (tipo != null)
                GestorDeErrores.Emitir($"La serie '{serie}' está configurada en el tipo '{tipo.Nombre}', use otra serie para el parámetro '{enumParametrosDeFacturasEmt.FAE_Serie_Por_Tipo_Anterior}' ");
        }

        private static List<SeriesPorTipo> ParsearSeries(string json)
        {
            try
            {
                var jsonArray = JArray.Parse(json);
                return jsonArray.Select(item => new SeriesPorTipo
                {
                    IdTipo = item["IdTipo"].Value<int>(),
                    Serie = item["Serie"].Value<string>(),
                }).ToList();
            }
            catch (Exception ex)
            {
                GestorDeErrores.Emitir($"Error al parsear el json: '{json}' al objeto del tipo '{_jsonDeFAE_Serie_Por_Tipo_Anterior}', debe definirlo en el parámetro de negocio '{enumParametrosDeFacturasEmt.FAE_Serie_Por_Tipo_Anterior}'", ex);
            }
            return null;
        }


        public static int IdTipoPreasiento(this FacturaEmtDtm factura, ContextoSe contexto)
        {
            var json = enumNegocio.FacturaEmitida.Parametro(enumParametrosDeFacturasEmt.FAE_Relacion_Con_Tipo_De_Preasiento, crearParametro: true, valorPorDefecto: _jsonDeTipoDePreasientoDeFae).Valor;

            var relaciones = ParsearRealacion(json);
            var relacion = relaciones.FirstOrDefault(x => x.IdTipoFactura == factura.IdTipo);
            if (relacion == null)
                GestorDeErrores.Emitir($"Ha de configurar el parámetro '{enumParametrosDeFacturasEmt.FAE_Relacion_Con_Tipo_De_Preasiento}' para el id de tipo factura '{factura.IdTipo}'");

            return relacion.IdTipoPreasiento;
        }

        public static int IdTipoPago(this FacturaEmtDtm factura, ContextoSe contexto)
        {
            var json = enumNegocio.FacturaEmitida.Parametro(enumParametrosDeFacturasEmt.FAE_Relacion_Con_Tipo_De_Pago, crearParametro: true, valorPorDefecto: _jsonDeTipoDePagoDeFae).Valor;

            var relaciones = ParsearTipoDePagoDeUnaFae(json);
            var relacion = relaciones.FirstOrDefault(x => x.IdTipoFactura == factura.IdTipo);
            if (relacion == null)
                GestorDeErrores.Emitir($"Ha de configurar el parámetro '{enumParametrosDeFacturasEmt.FAE_Relacion_Con_Tipo_De_Pago}' para el id de tipo factura '{factura.IdTipo}'");

            if (relacion.IdTipoPago == 0)
                GestorDeErrores.Emitir($"Ha de configurar el parámetro '{enumParametrosDeFacturasEmt.FAE_Relacion_Con_Tipo_De_Pago}' para el id de tipo factura '{factura.IdTipo}', indicando el id del tipo de pago para hacer abonos");

            return relacion.IdTipoPago;
        }


        public static int IdTipoPreasiento(this CobroDeFaeDtm cobro, ContextoSe contexto)
        {
            var idTipo = enumNegocio.FacturaEmitida.Parametro(enumParametrosDeFacturasEmt.COB_Relacion_Con_Tipo_De_Preasiento, crearParametro: true, valorPorDefecto: Literal.Cero).Valor.Entero();

            if (idTipo == 0)
                GestorDeErrores.Emitir($"Ha de configurar el parámetro '{enumParametrosDeFacturasEmt.COB_Relacion_Con_Tipo_De_Preasiento}' para el cobro de una factura emitida");

            return idTipo;
        }

        public static bool Fae_Sii_Activo()
        {
            return enumNegocio.FacturaEmitida.Parametro(enumParametrosDeFacturasEmt.FAE_SII_Activo, crearParametro: true, valorPorDefecto: "N").Valor.EsTrue();
        }

        public static bool UsaVerifactu(this FacturaEmtDtm factura, ContextoSe contexto, string errorSiNoValida = null)
        {
            if (enumNegocio.FacturaEmitida.Parametro(enumParametrosDeFacturasEmt.FAE_SII_Activo, crearParametro: true, valorPorDefecto: "N").Valor.EsTrue())
            {
                if (factura.ClaseDeEmision != enumClaseDeEmision.Impresa)
                {
                    var cg = contexto.Set<CentroGestorDtm>().First(cg => cg.Id == factura.IdCg);
                    if (cg.Baja)
                        GestorDeErrores.Emitir($"El cg '{cg.Expresion}' está dada de baja, no puede usar el servicio verifactu");

                    var sociedad = contexto.Set<SociedadDtm>().First(sociedad => sociedad.Id == cg.IdSociedad);
                    if (sociedad.Baja)
                        GestorDeErrores.Emitir($"La sociedad '{sociedad.Referencia}' está dada de baja, no puede usar el servicio verifactu");

                    return sociedad.TieneConfiguradoEl_SSII_Implantacion();
                }
                else GestorDeErrores.Emitir(errorSiNoValida.IsNullOrEmpty()
                    ? $"Si usa verifatu, parámetro '{enumParametrosDeFacturasEmt.FAE_SII_Activo}' activo, La factura '{factura.Referencia}' no puede tener clase '{enumClaseDeEmision.Impresa}'"
                    : errorSiNoValida);
            }
            return false;
        }

        public static bool UsaVerifactu(this SociedadDtm sociedad)
        {
            var usaVerficatu = enumNegocio.FacturaEmitida.Parametro(enumParametrosDeFacturasEmt.FAE_SII_Activo, crearParametro: true, valorPorDefecto: "N").Valor.EsTrue();
            var enProductivo = enumNegocio.FacturaEmitida.Parametro(enumParametrosDeFacturasEmt.FAE_SII_En_Productivo, crearParametro: true, valorPorDefecto: "N").Valor.EsTrue();
            if (!usaVerficatu && enProductivo)
                GestorDeErrores.Emitir($"El parámetro indicado '{enumParametrosDeFacturasEmt.FAE_SII_Activo}' es 'N' y '{enumParametrosDeFacturasEmt.FAE_SII_En_Productivo}' es 'S', esto es incoherente, corríjalo");

            if (usaVerficatu)
            {
                if (sociedad.Baja)
                    GestorDeErrores.Emitir($"La sociedad '{sociedad.Referencia}' está dada de baja");

                return sociedad.TieneConfiguradoEl_SSII_Implantacion();
            }
            return false;
        }

        public static void PonerElVerifactuEnProductivo(this SociedadDtm sociedad)
        {

            var parametro = enumParametrosDeFacturasEmt.FAE_SII_En_Productivo;
            var jsonLeido = enumNegocio.FacturaEmitida.Parametro(parametro, crearParametro: true, valorPorDefecto: _jsonDeFAE_SSII_EnProductivo).Valor;

            var enProductivo = new SSII_EnProductivo
            {
                IdSociedad = sociedad.Id,
                Valor = "S"
            };

            var relaciones = ParsearFAE_SII_EnProductivo(jsonLeido);

            var enTest = relaciones.First(r => r.IdSociedad == sociedad.Id);
            relaciones.Remove(enTest);
            relaciones.Add(enProductivo);
            enumNegocio.FacturaEmitida.Actualizar(parametro, SerializarSSII_EnProductivo(relaciones));
        }

        public static bool EstaElVerifactuEnProductivo(this SociedadDtm sociedad)
        {
            if (!sociedad.UsaVerifactu())
            {
                return false;
            }

            var parametro = enumParametrosDeFacturasEmt.FAE_SII_En_Productivo;
            var jsonLeido = enumNegocio.FacturaEmitida.Parametro(parametro, crearParametro: true, valorPorDefecto: _jsonDeFAE_SSII_EnProductivo).Valor;
            var enproductivo = new SSII_EnProductivo
            {
                IdSociedad = sociedad.Id,
                Valor = "N"
            };

            if (extJson.JsonEquals(jsonLeido, _jsonDeFAE_SSII_EnProductivo))
            {
                enumNegocio.FacturaEmitida.Actualizar(parametro, SerializarSSII_EnProductivo(new List<SSII_EnProductivo> { enproductivo }));
                return false;
            }

            try
            {
                var relaciones = ParsearFAE_SII_EnProductivo(jsonLeido);

                if (relaciones.FirstOrDefault(x => x.IdSociedad == sociedad.Id) == null)
                {
                    relaciones.Add(enproductivo);
                    enumNegocio.FacturaEmitida.Actualizar(parametro, SerializarSSII_EnProductivo(relaciones));
                }

                return relaciones.First(x => x.IdSociedad == sociedad.Id).Valor.EsTrue();
            }
            catch (Exception exc)
            {
                throw exc.Emitir($"Error al ver si la sociedad '{sociedad.NIF}' con id '{sociedad.Id}' usa verifactu y está en productivo, corrija el parámetro '{parametro}'");
            }
        }

        public static int DeclaracíonResponsable()
        {
            if (enumNegocio.FacturaEmitida.Parametro(enumParametrosDeFacturasEmt.FAE_SII_Activo, crearParametro: true, valorPorDefecto: "N").Valor.EsTrue())
            {
                var idArchivo = enumNegocio.FacturaEmitida.Parametro(enumParametrosDeFacturasEmt.FAE_SII_Declaracion_Responsable, crearParametro: true, valorPorDefecto: Literal.Cero).Valor.Entero();
                if (idArchivo == 0)
                    throw Excepciones.Emitir($"Debe configurar el parámetro {enumParametrosDeFacturasEmt.FAE_SII_Declaracion_Responsable} con el id del archivo que contiene la declaración responsable");
                return idArchivo;
            }
            throw Excepciones.Emitir($"Debe activar el parámetro {enumParametrosDeFacturasEmt.FAE_SII_Activo} para indicar que está el VERI*FACTU activo, y posteriormente subir el archivo de declaración responsable");
        }

        private static List<RelacionDeTiposFaeSpr> ParsearRealacion(string json)
        {
            try
            {
                var jsonArray = JArray.Parse(json);
                return jsonArray.Select(item => new RelacionDeTiposFaeSpr
                {
                    IdTipoFactura = item["IdTipoFactura"].Value<int>(),
                    IdTipoPreasiento = item["IdTipoPreasiento"].Value<int>(),
                }).ToList();
            }
            catch (Exception ex)
            {
                GestorDeErrores.Emitir($"Error al parsear el json: '{json}' al un objeto del tipo '{_jsonDeTipoDePreasientoDeFae}', debe definirlo en el parámetro de negocio '{enumParametrosDeFacturasEmt.FAE_Relacion_Con_Tipo_De_Preasiento}'", ex);
            }
            return null;
        }

        private static List<RelacionDeTiposFaePago> ParsearTipoDePagoDeUnaFae(string json)
        {
            try
            {
                var jsonArray = JArray.Parse(json);
                return jsonArray.Select(item => new RelacionDeTiposFaePago
                {
                    IdTipoFactura = item["IdTipoFactura"].Value<int>(),
                    IdTipoPago = item["IdTipoPago"].Value<int>(),
                }).ToList();
            }
            catch (Exception ex)
            {
                GestorDeErrores.Emitir($"Error al parsear el json: '{json}' al un objeto del tipo '{_jsonDeTipoDePagoDeFae}', debe definirlo en el parámetro de negocio '{enumParametrosDeFacturasEmt.FAE_Relacion_Con_Tipo_De_Pago}'", ex);
            }
            return null;
        }

        public static bool HayQueFirmarFacturas(this SociedadDtm sociedad, ContextoSe contexto)
        {
            var parametro = enumParametrosDeFacturasEmt.FAE_SII_Firmar;
            var json = enumNegocio.FacturaEmitida.Parametro(parametro, crearParametro: true, valorPorDefecto: _jsonDeFAE_SII_Firmar).Valor;
            if (json == _jsonDeFAE_SII_Firmar)
            {
                GestorDeErrores.Emitir($"Ha de indicar en el parámetro '{parametro}' si se la sociedad '{sociedad.Id}' ha de firmar los registros");
            }
            var relaciones = ParsearFAE_SII_Firmar(json);

            var relacion = relaciones.FirstOrDefault(x => x.IdSociedad == sociedad.Id);

            if (relacion == null)
                GestorDeErrores.Emitir($"Ha de configurar el parámetro '{parametro}' para el id de sociedad '{sociedad.Id}'");

            if (relacion.Valor.IsNullOrEmpty())
                GestorDeErrores.Emitir($"El valor asignado en el parámetro '{parametro}' para el id de sociedad '{sociedad.Id}' no es válido, ha de indicar si se firman o no (S/N)");

            return relacion.Valor.EsTrue();
        }

        public static bool TieneConfiguradoEl_SSII_Implantacion(this SociedadDtm sociedad)
        {
            var parametro = enumParametrosDeFacturasEmt.SII_SSII_IMPLANTACION;
            var json = enumNegocio.FacturaEmitida.Parametro(parametro, crearParametro: true, valorPorDefecto: ltrParametrosDelSii.Pendiente).Valor;
            if (json == ltrParametrosDelSii.Pendiente)
            {
                GestorDeErrores.Emitir($"Ha de indicar en el parámetro '{parametro}' el nº de implantación SIF para la sociedad '{sociedad.Id}'");
            }
            var relaciones = ParsearSSII_Implantacion(json);

            var relacion = relaciones.FirstOrDefault(x => x.IdSociedad == sociedad.Id);

            return relacion != null;
        }

        public static string ObtenerSSII_Implantacion(this SociedadDtm sociedad, ContextoSe contexto)
        {
            var parametro = enumParametrosDeFacturasEmt.SII_SSII_IMPLANTACION;
            var json = enumNegocio.FacturaEmitida.Parametro(parametro, crearParametro: true, valorPorDefecto: ltrParametrosDelSii.Pendiente).Valor;
            if (json == ltrParametrosDelSii.Pendiente)
            {
                GestorDeErrores.Emitir($"Ha de indicar en el parámetro '{parametro}' el nº de implantación SIF para la sociedad '{sociedad.Id}'");
            }
            var relaciones = ParsearSSII_Implantacion(json);

            var relacion = relaciones.FirstOrDefault(x => x.IdSociedad == sociedad.Id);

            if (relacion == null)
                GestorDeErrores.Emitir($"Ha de configurar el parámetro '{parametro}' para el id de sociedad '{sociedad.Id}'");

            var items = relaciones.Count(x => x.IdSociedad == sociedad.Id);
            if (items > 1)
                GestorDeErrores.Emitir($"El parámetro '{parametro}' tiene más de una relación para la sociedad '{sociedad.Id}', ha de tener una sola relación por sociedad");

            if (relacion.Valor.IsNullOrEmpty())
                GestorDeErrores.Emitir($"El valor asignado en el parámetro '{parametro}' para el id de sociedad '{sociedad.Id}' no es válido, ha de indicar el nº de implantación");

            var partes = relacion.Valor.Split("-");
            if (partes.Count() != 3)
                GestorDeErrores.Emitir($"El valor asignado en el parámetro '{parametro}' para el id de sociedad '{sociedad.Id}' no es válido, el valor ha de seguir el patrón 'BD-nif-id'");

            if (partes[0] != contexto.DatosDeConexion.Bd)
                GestorDeErrores.Emitir($"El valor asignado en el parámetro '{parametro}' para el id de sociedad '{sociedad.Id} 'indica que la BD es '{partes[0]}' y la indicada en el contexto es '{contexto.DatosDeConexion.Bd}'");

            if (partes[1] != sociedad.NIF)
                GestorDeErrores.Emitir($"El valor asignado en el parámetro '{parametro}' para el id de sociedad '{sociedad.Id} 'indica que el nif es '{partes[1]}' y el nif de la sociedad es '{sociedad.NIF}'");

            if (partes[2] != sociedad.Id.ToString())
                GestorDeErrores.Emitir($"El valor asignado en el parámetro '{parametro}' para el id de sociedad '{sociedad.Id} 'indica que la BD es '{partes[2]}' y el id de la sociedad es '{sociedad.Id}'");

            return relacion.Valor;
        }


        public static string SII_ActivarVerifactuEnLaSociedad(this SociedadDtm sociedad, ContextoSe contexto)
        {
            var parametro = enumParametrosDeFacturasEmt.SII_SSII_IMPLANTACION;
            var jsonLeido = enumNegocio.FacturaEmitida.Parametro(parametro, crearParametro: true, valorPorDefecto: ltrParametrosDelSii.Pendiente).Valor;
            var implantacion = new SSII_Implantacion
            {
                IdSociedad = sociedad.Id,
                Valor = $"{contexto.DatosDeConexion.Bd}-{sociedad.NIF}-{sociedad.Id}"
            };

            if (extJson.JsonEquals(jsonLeido, ltrParametrosDelSii.Pendiente))
            {
                return SerializarSSII_Implantacion(new List<SSII_Implantacion> { implantacion });
            }

            var relaciones = ParsearSSII_Implantacion(jsonLeido);

            if (relaciones.FirstOrDefault(x => x.IdSociedad == sociedad.Id) != null)
                GestorDeErrores.Emitir($"La sociedad '{sociedad.Referencia}' ya estaba configurada para usar el servicio verifactu. Id de implantación '{relaciones.First(x => x.IdSociedad == sociedad.Id).Valor}'");

            relaciones.Add(implantacion);
            return SerializarSSII_Implantacion(relaciones);
        }

        public static string SII_DesactivarVerifactuEnLaSociedad(this SociedadDtm sociedad, ContextoSe contexto)
        {
            var parametro = enumParametrosDeFacturasEmt.SII_SSII_IMPLANTACION;
            var jsonLeido = enumNegocio.FacturaEmitida.Parametro(parametro, crearParametro: true, valorPorDefecto: ltrParametrosDelSii.Pendiente).Valor;
            var relaciones = ParsearSSII_Implantacion(jsonLeido);
            var implantacion = relaciones.FirstOrDefault(x => x.IdSociedad == sociedad.Id);

            if (relaciones.Count == 1 && implantacion != null)
            {
                implantacion.IdSociedad = 0;
                implantacion.Valor = "BD-nif-id";
            }
            else if (implantacion != null)
            {
                relaciones.Remove(implantacion);
            }
            return SerializarSSII_Implantacion(relaciones);
        }

        private static List<SII_Firmar> ParsearFAE_SII_Firmar(string json)
        {
            try
            {
                var jsonArray = JArray.Parse(json);
                return jsonArray.Select(item => new SII_Firmar
                {
                    IdSociedad = item["IdSociedad"].Value<int>(),
                    Valor = item["Valor"].Value<string>(),
                }).ToList();
            }
            catch (Exception ex)
            {
                enumNegocio.FacturaEmitida.Actualizar(enumParametrosDeFacturasEmt.FAE_SII_Firmar, _jsonDeFAE_SII_Firmar);
                throw ex.Emitir($"Error al parsear el json: '{json}' al objeto del tipo '{_jsonDeFAE_SII_Firmar}'");
            }
        }

        private static List<SSII_EnProductivo> ParsearFAE_SII_EnProductivo(string json)
        {
            try
            {
                var jsonArray = JArray.Parse(json);
                return jsonArray.Select(item => new SSII_EnProductivo
                {
                    IdSociedad = item["IdSociedad"].Value<int>(),
                    Valor = item["Valor"].Value<string>(),
                }).ToList();
            }
            catch (Exception ex)
            {
                enumNegocio.FacturaEmitida.Actualizar(enumParametrosDeFacturasEmt.FAE_SII_En_Productivo, _jsonDeFAE_SSII_EnProductivo);
                throw ex.Emitir($"Error al parsear el json: '{json}' al objeto del tipo '{_jsonDeFAE_SII_Firmar}', corrija el parámetro '{enumParametrosDeFacturasEmt.FAE_SII_En_Productivo}'");
            }
        }

        private static List<SSII_Implantacion> ParsearSSII_Implantacion(string json)
        {
            try
            {
                var jsonArray = JArray.Parse(json);
                return jsonArray.Select(item => new SSII_Implantacion
                {
                    IdSociedad = item["IdSociedad"].Value<int>(),
                    Valor = item["Valor"].Value<string>(),
                }).ToList();
            }
            catch (Exception ex)
            {
                enumNegocio.FacturaEmitida.Actualizar(enumParametrosDeFacturasEmt.SII_SSII_IMPLANTACION, ltrParametrosDelSii.Pendiente);
                GestorDeErrores.Emitir($"Error al parsear el json: '{json}' al objeto del tipo '{ltrParametrosDelSii.Pendiente}'", ex);
            }
            return null;
        }

        private static string SerializarSSII_Implantacion(List<SSII_Implantacion> lista)
        {
            try
            {
                return JsonConvert.SerializeObject(lista);
            }
            catch (Exception ex)
            {
                // Manejo de errores similar al de tu método de parseo
                GestorDeErrores.Emitir("Error al serializar la lista SSII_Implantacion", ex);
            }
            return null;
        }

        private static string SerializarSSII_EnProductivo(List<SSII_EnProductivo> lista)
        {
            try
            {
                return JsonConvert.SerializeObject(lista);
            }
            catch (Exception ex)
            {
                // Manejo de errores similar al de tu método de parseo
                GestorDeErrores.Emitir("Error al serializar la lista SSII_EnProductivo", ex);
            }
            return null;
        }
    }

}
