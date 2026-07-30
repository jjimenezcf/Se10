using Gestor.Errores;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Negocio;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Utilidades;

namespace ServicioDeDatos.Gastos
{

    public enum enumEtapasDeFacturasRec
    {
        [Description("Ids de estados en los que una factura se ha de cumplimentar")]
        FAR_Etapa_De_Cumplimentacion,
        [Description("Ids de estados en los que una factura se está aprobando")]
        FAR_Etapa_De_Aprobacion,
        [Description("Ids de estados en los que una factura está en aprobación o en cumplimentación")]
        FAR_Etapa_Anterior_A_Contabilidad,
        [Description("Ids de estados en los que una factura se ha de contabilizar")]
        FAR_Etapa_De_Contabilizacion,
        [Description("Ids de estados en los que una factura se ha de pagar")]
        FAR_Etapa_De_Pago,
        [Description("Ids de estados en los que una factura está pagada")]
        FAR_Etapa_Pagada,
        [Description("Ids de estados en los que una factura es devuelta a proveedor")]
        FAR_Etapa_Devuelta,
        [Description("Ids de estados en los que una factura es anulada")]
        FAR_Etapa_Anulada
    }

    public enum enumParametrosDeFacturasRec
    {
        [Description("Indica para un motivo y un estado de la factura que transición se le aplica")]
        FAR_Aplicar_Transicion,
        [Description("Indica el id de la unidad de medida por defecto")]
        FAR_Unidad_Medida,
        [Description("Indica el id de la naturaleza por defecto")]
        FAR_Naturaleza,
        [Description("Indica la tolerancia admitida entre las líneas y la BI y entre el total a pagar y el pago")]
        FAR_ToleranciaEnImportes,
        [Description("Indica si se permiten facturas negativas sin sindicar a quién rectifica")]
        FAR_PermitirFacturasNegativas,
        [Description("Indica el incremento al enumerar las filas")]
        FAR_IncrementarOrdenEn,
        [Description("Indica que si se intenta transitar a contabilizada y falta la fecha contable entonces se indica la del día")]
        FAR_Al_Contabilizar_Indicar_Fecha,
        [Description("Indica que al enviar a contabilizar, si está pagada, la envíe a la etapa de pago")]
        FAR_Dar_Por_Contabilizada_Si_Pagada,
        [Description("Indica cómo se ha de tratar la fecha de recepción")]
        FAR_Como_Tratar_La_Fecha_De_Recepcion,
        [Description("Indica para un id de tipo factura que id de tipo de preasiento se ha de usar")]
        FAR_Relacion_Con_Tipo_De_Preasiento,
        [Description("Tipo de archivador para facturas recibidas")]
        FAR_Tipo_De_Archivador,
        [Description("Pregunta para la IA para analizar una factura")]
        IA_Prompt,
        [Description("Indica si se ha de quitar el contrato automáticamente al cancelar una factura")]
        FAR_Quitar_Contrato_Al_Anular,
        [Description("Indica si se ha de quitar el expediente automáticamente al cancelar una factura")]
        FAR_Quitar_Expediente_Al_Anular,
        [Description("Indica si se ha de quitar el expediente automáticamente al cancelar una factura")]
        FAR_Quitar_Pagos_Al_Anular
    }

    public enum enumValoresDeComoTratarRecibidoEl
    {
        [Description("Indica que ha de asignar la misma fecha que la de facturación")]
        mismafecha,
        [Description("Indica que ha de asignar la fecha de hoy")]
        fechadehoy,
        [Description("Indica que si la fecha de factura es <= que hoy - 15 días asigne la fecha de factura si no la fecha de hoy")]
        fechadehoy15,
        [Description("Indica que si la fecha de factura es <= que hoy - 30 días asigne la fecha de factura si no la fecha de hoy")]
        fechadehoy30
    }


    public class RelacionDeTiposFarSpr
    {
        public int IdTipoFactura { get; set; }
        public int IdTipoPreasiento { get; set; }
    }

    public enum enumEtiquetasDeFacturasRec
    {
        BaseImponible,
        Total,
    }

    public static class VariableDeFacturasRec
    {
        internal static readonly string IA_Modelo_de_datos = $@"
## MODELO DE DATOS ESPECÍFICO: FacturaRecDtm

### Propiedades específicas de FacturaRecDtm:
| Propiedad      | Tipo       | Descripción                                              |
|----------------|------------|----------------------------------------------------------|
| IdProveedor    | int        | ID del proveedor (→ TerceroDtm: Nombre, Nif)             |
| FacturadaEl    | DateTime?  | Fecha de la factura del proveedor                        |
| RecibidaEl     | DateTime?  | Fecha en que se recibió la factura                       |
| VenceEl        | DateTime?  | Fecha de vencimiento del pago                            |
| BaseImponible  | decimal    | Base imponible de la factura                             |
| TotalDelPago   | decimal    | Importe total a pagar (con IVA y retenciones)            |
| IdContrato     | int?       | ID del contrato asociado (→ ContratoDtm)                 |
| IdExpediente   | int?       | ID del expediente asociado (→ ExpedienteDtm)             |

### Métodos calculados disponibles (usar en Campo de métricas):
- `calculado:{nameof(enumEtiquetasDeFacturasRec.BaseImponible)}` — base imponible de la factura
- `calculado:{nameof(enumEtiquetasDeFacturasRec.Total)}` — total a pagar (con IVA y retenciones)

### Objetos relacionados adicionales:
- **TerceroDtm** (`IdProveedor`): `Id`, `Nombre`, `Apellido`, `Nif`, `Email`
- **PagoDtm**: pagos vinculados a esta factura recibida
- **ContratoDtm** (`IdContrato`): contrato asociado si existe
";

        internal static readonly string IA_Reglas_de_filtrado = @"# PROMPT DE EXTRACCIÓN: FACTURAS RECIBIDAS

Este bloque de reglas se integra en la sección **## REGLAS ESPECÍFICAS DEL ELEMENTO** cuando el `{NegocioTratado}` es ""Facturas Recibidas"". Utiliza el prefijo **R.FacturasRec**.

### R.FacturasRec.1 · Proveedor (`FiltroPorProveedor`)
- **Disparador:** Facturas ""del proveedor [Nombre]"", ""compradas a [Empresa]"", ""de [Nombre de empresa/CIF]"".
- **Acción:** Generar una lista de objetos en el array de filtros:
    1. **Búsqueda:**
       `{""Clausula"": ""FiltroPorProveedor"", ""Criterio"": ""contiene"", ""Valor"": ""[texto_buscado]""}`
    2. **Vista:**
       `{""Clausula"": ""quemostrar"", ""Criterio"": ""igual"", ""Valor"": ""8""}`
- **Nota:**
  * Extrae tanto el nombre comercial como el CIF/NIF si el usuario lo proporciona.
  * La inclusión de la cláusula `quemostrar` con valor `8` se aplica si pide ""todas"".

### R.FacturasRec.2 · Importes y Totales
**R.FacturasRec.2.1 · Base Imponible (`FiltroPorImporteSinIva`)**
- **Disparador:** ""con base de [Monto]"", ""BI de [Monto]"", ""importe neto de [Monto]"".
- **Acción:** `{""Clausula"": ""FiltroPorImporteSinIva"", ""Criterio"": ""entreImportes"", ""Valor"": ""min;max""}`

**R.FacturasRec.2.2 · Total Factura (`FiltroPorTotalFactura`)**
- **Disparador:** ""total de [Monto]"", ""por valor de [Monto] con iva"", ""que sumen [Monto] en total"".
- **Acción:** `{""Clausula"": ""FiltroPorTotalFactura"", ""Criterio"": ""entreImportes"", ""Valor"": ""min;max""}`

### R.FacturasRec.3 · Fechas Operativas
**R.FacturasRec.3.1 · Fecha de Emisión/Factura (`FiltroPorFechaDeEmision`)**
- **Disparador:** ""emitidas el..."", ""con fecha de factura de..."", ""facturadas en [Periodo]"".
- **Acción:** `{""Clausula"": ""FiltroPorFechaDeEmision"", ""Criterio"": ""entreFechas"", ""Valor"": ""YYYY-MM-DDTHH:mm:ssZ-YYYY-MM-DDTHH:mm:ssZ""}`

**R.FacturasRec.3.2 · Fecha de Vencimiento (`FiltroPorFechaDeVencimiento`)**
- **Disparador:** ""que vencen el..."", ""vencimiento en [Periodo]"", ""para pagar en [Mes]"".
- **Acción:** `{""Clausula"": ""FiltroPorFechaDeVencimiento"", ""Criterio"": ""entreFechas"", ""Valor"": ""YYYY-MM-DDTHH:mm:ssZ-YYYY-MM-DDTHH:mm:ssZ""}`

**R.FacturasRec.3.3 · Año fiscal de la factura (`FiltroPorEjercicioDeFactura`)**
- **Disparador:** ""del año [YYYY]"", ""del ejercicio [YYYY]"", ""facturadas en [YYYY]"" (cuando se indica solo el año sin rango de fechas).
- **Acción:** `{""Clausula"": ""FiltroPorEjercicioDeFactura"", ""Criterio"": ""igual"", ""Valor"": ""YYYY""}`
- **Nota:** Usar esta regla cuando el usuario indica un año concreto. Si indica un rango de fechas completo, usar R.3.1.

### R.FacturasRec.4 · Impuestos y Retenciones (`FiltroDeIvaIrpf`)
- **Disparador:** Menciona tipos de impuestos específicos.
- **Mapeo de Valores Obligatorio:**
  - ""con iva"" (normal) → `""FiltroConIva""`
  - ""con irpf"" o ""con retención"" → `""FiltroConIrpf""`
  - ""iva exento"" → `""FiltroConIvaExento""`
  - ""sujeto pasivo"" o ""isp"" → `""FiltroConIvaIsp""`
  - ""no sujeto"" → `""FiltroConIvaNsj""`
  - ""sin iva ni irpf"" → `""FiltroSinIvaNiIrpf""`
- **Acción:** `{""Clausula"": ""FiltroDeIvaIrpf"", ""Criterio"": ""esAlgunoDe"", ""Valor"": ""Valor1;Valor2""}`

### R.FacturasRec.5 · Preasientos y Contabilización (`FiltroSiHayPreasiento`)
- **Disparador:** Estado de contabilización o existencia de preasiento.
- **Mapeo de Valores Obligatorio:**
  - ""con preasiento"" → `""FiltroConSpr""`
  - ""sin preasiento"" → `""FiltroSinSpr""`
  - ""preasiento cancelado"" → `""FiltroConSprCan""`
- **Acción:** `{""Clausula"": ""FiltroSiHayPreasiento"", ""Criterio"": ""igual"", ""Valor"": ""ValorMapeado""}`

### R.FacturasRec.6 · Naturaleza contable (`NombreNaturaleza`)
- **Disparador positivo:** Facturas ""de naturaleza [Nombre]"", ""con naturaleza [Sigla]"", ""de tipo contable [Nombre]"", ""de la naturaleza [X]"".
- **Disparador negativo:** Facturas ""que NO sean de naturaleza [X]"", ""sin naturaleza [X]"", ""cuya naturaleza no sea [X]"".
- **Acción (positivo):** `{""Clausula"": ""NombreNaturaleza"", ""Criterio"": ""contiene"", ""Valor"": ""nombre_o_sigla""}`
- **Acción (negativo):** `{""Clausula"": ""NombreNaturaleza"", ""Criterio"": ""noContiene"", ""Valor"": ""nombre_o_sigla""}`
- **Nota:** Busca coincidencia parcial en el nombre o sigla de la naturaleza contable de las líneas de la factura.

### R.FacturasRec.7 · Forma de Pago (`FiltroPorFormaDePago`)
- **Disparador:** ""pagadas por..."", ""forma de pago [Modo]"".
- **Mapeo de Valores:**
  - ""contado"" → `FiltroDePagosContado`
  - ""tarjeta"" → `FiltroDePagosTarjeta`
  - ""domiciliada"" → `FiltroDePagosDomiciliado`
  - ""transferencia"" → `FiltroDePagosTransferencia`
  - ""remesa"" → `FiltroDePagosRemesa`
- **Acción:** `{""Clausula"": ""FiltroPorFormaDePago"", ""Criterio"": ""igual"", ""Valor"": ""ValorMapeado""}`

### R.FacturasRec.8 · Relación con Expedientes

**R.FacturasRec.8.1 · Búsqueda por datos de Expediente (`NombreExpediente`)**
- **Disparador positivo:** Facturas ""del expediente [Nombre/Referencia]"", ""imputadas al expediente [X]"", ""vinculadas al expediente [Y]"".
- **Disparador negativo:** Facturas ""que NO sean del expediente [X]"", ""no imputadas al expediente [Y]"", ""fuera del expediente [X]"".
- **Acción (positivo):** `{""Clausula"": ""NombreExpediente"", ""Criterio"": ""contiene"", ""Valor"": ""nombre_o_referencia""}`
- **Acción (negativo):** `{""Clausula"": ""NombreExpediente"", ""Criterio"": ""noContiene"", ""Valor"": ""nombre_o_referencia""}`
- **Nota:** Busca coincidencia parcial en nombre o referencia del expediente.

**R.FacturasRec.8.2 · Situación: Facturas con expediente (`AsociadaAUnExpediente`)**
- **Disparador:** Facturas ""con expediente"", ""imputadas a algún expediente"", ""que tengan expediente"".
- **Acción:** `{""Clausula"": ""AsociadaAUnExpediente"", ""Criterio"": ""igual"", ""Valor"": ""5""}` (Representa ConRelacion).

**R.FacturasRec.8.3 · Situación: Facturas sin expediente (`AsociadaAUnExpediente`)**
- **Disparador:** Facturas ""sin expediente"", ""no imputadas a ningún expediente"", ""pendientes de imputar"".
- **Acción:** `{""Clausula"": ""AsociadaAUnExpediente"", ""Criterio"": ""igual"", ""Valor"": ""6""}` (Representa SinRelacion).

**Jerarquía:** Si se detecta búsqueda por nombre (R.8.1), se omiten las reglas de situación (R.8.2 y R.8.3).

### R.FacturasRec.9 · Relación con Contratos

**R.FacturasRec.9.1 · Búsqueda por datos de Contrato (`NombreContrato`)**
- **Disparador positivo:** Facturas ""del contrato [Nombre/Referencia]"", ""vinculadas al contrato [X]"", ""imputadas al contrato [Y]"".
- **Disparador negativo:** Facturas ""que NO sean del contrato [X]"", ""no vinculadas al contrato [Y]"", ""fuera del contrato [X]"".
- **Acción (positivo):** `{""Clausula"": ""NombreContrato"", ""Criterio"": ""contiene"", ""Valor"": ""nombre_o_referencia""}`
- **Acción (negativo):** `{""Clausula"": ""NombreContrato"", ""Criterio"": ""noContiene"", ""Valor"": ""nombre_o_referencia""}`
- **Nota:** Busca coincidencia parcial en nombre o referencia del contrato.

**R.FacturasRec.9.2 · Situación: Facturas con contrato (`AsociadaAUnContrato`)**
- **Disparador:** Facturas ""con contrato"", ""vinculadas a algún contrato"", ""que tengan contrato"".
- **Acción:** `{""Clausula"": ""AsociadaAUnContrato"", ""Criterio"": ""igual"", ""Valor"": ""5""}` (Representa ConRelacion).

**R.FacturasRec.9.3 · Situación: Facturas sin contrato (`AsociadaAUnContrato`)**
- **Disparador:** Facturas ""sin contrato"", ""no contractuales"", ""pendientes de asignar contrato"".
- **Acción:** `{""Clausula"": ""AsociadaAUnContrato"", ""Criterio"": ""igual"", ""Valor"": ""6""}` (Representa SinRelacion).

**Jerarquía:** Si se detecta búsqueda por nombre (R.9.1), se omiten las reglas de situación (R.9.2 y R.9.3).

### R.FacturasRec.10 · Relación con Remesas de Pago

**R.FacturasRec.10.1 · Búsqueda por datos de Remesa (`NombreRemesaPag`)**
- **Disparador positivo:** Facturas ""de la remesa [Nombre/Referencia]"", ""incluidas en la remesa de pago [X]"", ""de la remesa [Y]"".
- **Disparador negativo:** Facturas ""que NO sean de la remesa [X]"", ""no incluidas en la remesa de pago [Y]"", ""fuera de la remesa [X]"".
- **Acción (positivo):** `{""Clausula"": ""NombreRemesaPag"", ""Criterio"": ""contiene"", ""Valor"": ""nombre_o_referencia""}`
- **Acción (negativo):** `{""Clausula"": ""NombreRemesaPag"", ""Criterio"": ""noContiene"", ""Valor"": ""nombre_o_referencia""}`
- **Nota:** Busca coincidencia parcial en nombre o referencia de la remesa de pago.

**R.FacturasRec.10.2 · Situación: Facturas con pagos remesados (`FiltroPorRemesaPag`)**
- **Disparador:** Facturas ""remesadas"", ""cuyos pagos están en una remesa"", ""con pago remesado"", ""pagadas por remesa"".
- **Acción:** `{""Clausula"": ""FiltroPorRemesaPag"", ""Criterio"": ""igual"", ""Valor"": ""5""}` (Representa ConRelacion).

**R.FacturasRec.10.3 · Situación: Facturas sin pagos remesados (`FiltroPorRemesaPag`)**
- **Disparador:** Facturas ""sin remesar"", ""cuyos pagos no están en remesa"", ""sin pago remesado"".
- **Acción:** `{""Clausula"": ""FiltroPorRemesaPag"", ""Criterio"": ""igual"", ""Valor"": ""6""}` (Representa SinRelacion).

**Jerarquía:** Si se detecta búsqueda por nombre (R.10.1), se omiten las reglas de situación (R.10.2 y R.10.3).

### R.FacturasRec.11 · Relación con Estimaciones Directas

**R.FacturasRec.11.1 · Búsqueda por datos de Estimación Directa (`NombreEstimacion`)**
- **Disparador positivo:** Facturas ""de la estimación [Nombre/Referencia]"", ""incluidas en la estimación directa [X]"", ""de la estimación contable [Y]"".
- **Disparador negativo:** Facturas ""que NO sean de la estimación [X]"", ""no incluidas en la estimación directa [Y]"", ""fuera de la estimación [X]"".
- **Acción (positivo):** `{""Clausula"": ""NombreEstimacion"", ""Criterio"": ""contiene"", ""Valor"": ""nombre_o_referencia""}`
- **Acción (negativo):** `{""Clausula"": ""NombreEstimacion"", ""Criterio"": ""noContiene"", ""Valor"": ""nombre_o_referencia""}`
- **Nota:** Busca coincidencia parcial en nombre o referencia de la estimación directa.

**R.FacturasRec.11.2 · Situación: Facturas en alguna estimación (`VinculosAUnaEstimacion`)**
- **Disparador:** Facturas ""en una estimación"", ""incluidas en estimación directa"", ""ya estimadas"", ""con estimación"".
- **Acción:** `{""Clausula"": ""VinculosAUnaEstimacion"", ""Criterio"": ""igual"", ""Valor"": ""5""}` (Representa ConRelacion).

**R.FacturasRec.11.3 · Situación: Facturas sin estimación (`VinculosAUnaEstimacion`)**
- **Disparador:** Facturas ""pendientes de estimación"", ""sin estimación"", ""no incluidas en ninguna estimación directa"".
- **Acción:** `{""Clausula"": ""VinculosAUnaEstimacion"", ""Criterio"": ""igual"", ""Valor"": ""6""}` (Representa SinRelacion).

**Jerarquía:** Si se detecta búsqueda por nombre (R.11.1), se omiten las reglas de situación (R.11.2 y R.11.3).

### R.FacturasRec.12 · Relación con Lotes Contables

**R.FacturasRec.12.1 · Búsqueda por datos de Lote Contable (`NombreLoteContable`)**
- **Disparador positivo:** Facturas ""del lote [Nombre/Referencia]"", ""incluidas en el lote contable [X]"", ""del lote de contabilidad [Y]"".
- **Disparador negativo:** Facturas ""que NO sean del lote [X]"", ""no incluidas en el lote contable [Y]"", ""fuera del lote [X]"".
- **Acción (positivo):** `{""Clausula"": ""NombreLoteContable"", ""Criterio"": ""contiene"", ""Valor"": ""nombre_o_referencia""}`
- **Acción (negativo):** `{""Clausula"": ""NombreLoteContable"", ""Criterio"": ""noContiene"", ""Valor"": ""nombre_o_referencia""}`
- **Nota:** Busca coincidencia parcial en nombre o referencia del lote contable.

**R.FacturasRec.12.2 · Situación: Facturas en algún lote contable (`VinculosAUnLote`)**
- **Disparador:** Facturas ""en un lote contable"", ""contabilizadas en lote"", ""incluidas en lote"", ""que tengan lote"".
- **Acción:** `{""Clausula"": ""VinculosAUnLote"", ""Criterio"": ""igual"", ""Valor"": ""5""}` (Representa ConRelacion).

**R.FacturasRec.12.3 · Situación: Facturas sin lote contable (`VinculosAUnLote`)**
- **Disparador:** Facturas ""pendientes de lote"", ""sin lote contable"", ""fuera de lote contable"", ""sin contabilizar en lote"".
- **Acción:** `{""Clausula"": ""VinculosAUnLote"", ""Criterio"": ""igual"", ""Valor"": ""6""}` (Representa SinRelacion).

**Jerarquía:** Si se detecta búsqueda por nombre (R.12.1), se omiten las reglas de situación (R.12.2 y R.12.3).";

        private static string etapaDeCumplimentacion => enumNegocio.FacturaRecibida.Parametro(enumEtapasDeFacturasRec.FAR_Etapa_De_Cumplimentacion)?.Valor ?? null;
        private static string etapaDeAprobacion => enumNegocio.FacturaRecibida.Parametro(enumEtapasDeFacturasRec.FAR_Etapa_De_Aprobacion)?.Valor ?? null;
        private static string etapaDeCobro => enumNegocio.FacturaRecibida.Parametro(enumEtapasDeFacturasRec.FAR_Etapa_De_Contabilizacion)?.Valor ?? null;
        private static string etapaDePago => enumNegocio.FacturaRecibida.Parametro(enumEtapasDeFacturasRec.FAR_Etapa_De_Pago)?.Valor ?? null;
        private static string etapaDeCierre => enumNegocio.FacturaRecibida.Parametro(enumEtapasDeFacturasRec.FAR_Etapa_Pagada)?.Valor ?? null;
        private static string etapaDeDevolucion => enumNegocio.FacturaRecibida.Parametro(enumEtapasDeFacturasRec.FAR_Etapa_Devuelta)?.Valor ?? null;
        private static string etapaAnulada => enumNegocio.FacturaRecibida.Parametro(enumEtapasDeFacturasRec.FAR_Etapa_Anulada)?.Valor ?? null;

        public enum enumMotivoTransicion { };

        public static string TransicionesPorMotivo => enumNegocio.FacturaRecibida.Parametro(enumParametrosDeFacturasRec.FAR_Aplicar_Transicion,
            valorPorDefecto: JsonConvert.SerializeObject(new List<object> {new TransicionPorMotivo
                        {
                            Motivo = "",
                            IdEstado = 0,
                            IdTransicion = 0
                        }
                    }
                )).Valor;


        public static string Estados(this enumEtapasDeFacturasRec etapa)
        {
            string estados = null;
            switch (etapa)
            {
                case enumEtapasDeFacturasRec.FAR_Etapa_De_Cumplimentacion: estados = etapaDeCumplimentacion; break;
                case enumEtapasDeFacturasRec.FAR_Etapa_De_Aprobacion: estados = etapaDeAprobacion; break;
                case enumEtapasDeFacturasRec.FAR_Etapa_Anterior_A_Contabilidad:
                    estados = etapaDeCumplimentacion != null ? etapaDeCumplimentacion : "";
                    if (estados.IsNullOrEmpty()) 
                        estados = etapaDeAprobacion;
                    else
                        estados = estados + $"{(etapaDeAprobacion == null ? "": "," + etapaDeAprobacion)}" ; 
                    break;
                case enumEtapasDeFacturasRec.FAR_Etapa_De_Contabilizacion: estados = etapaDeCobro; break;
                case enumEtapasDeFacturasRec.FAR_Etapa_De_Pago: estados = etapaDePago; break;
                case enumEtapasDeFacturasRec.FAR_Etapa_Pagada: estados = etapaDeCierre; break;
                case enumEtapasDeFacturasRec.FAR_Etapa_Devuelta: estados = etapaDeDevolucion; break;
                case enumEtapasDeFacturasRec.FAR_Etapa_Anulada: estados = etapaAnulada; break;

            }

            return estados.IsNullOrEmpty() ? enumNegocio.FacturaRecibida.DefinirEtapaSiLoIndicaConfiguracion(etapa, ltrEstados.EstadoNulo) : estados;
        }

        public static bool EstaEnLaEtapa(this FacturaRecDtm factura, enumEtapasDeFacturasRec etapa) => etapa.Lista().Contains(factura.IdEstado);

        public static bool ContieneLaEtapa(this List<enumEtapasDeFacturasRec> etapas, enumEtapasDeFacturasRec etapa) => etapas.Contains(etapa);

        public static bool EstaEnAlgunaDeLasEtapa(this FacturaRecDtm factura, List<enumEtapasDeFacturasRec> etapas)
        {
            var etapasDeLaFactura = factura.Etapas();
            foreach (var etapa in etapas)
                if (etapasDeLaFactura.Contains(etapa)) return true;
            return false;
        }

        public static bool EstaEnEtapaNoContabilizada(this FacturaRecDtm factura)
        {
            return factura.EstaEnAlgunaDeLasEtapa(new List<enumEtapasDeFacturasRec> {
                enumEtapasDeFacturasRec.FAR_Etapa_De_Cumplimentacion,
                enumEtapasDeFacturasRec.FAR_Etapa_De_Aprobacion,
                enumEtapasDeFacturasRec.FAR_Etapa_De_Contabilizacion,
                enumEtapasDeFacturasRec.FAR_Etapa_Anulada,
                enumEtapasDeFacturasRec.FAR_Etapa_Devuelta});
        }

        public static bool EstaContabilizada(this FacturaRecDtm factura)
        {
            return !factura.EstaEnAlgunaDeLasEtapa(new List<enumEtapasDeFacturasRec> {
                enumEtapasDeFacturasRec.FAR_Etapa_De_Cumplimentacion,
                enumEtapasDeFacturasRec.FAR_Etapa_De_Aprobacion,
                enumEtapasDeFacturasRec.FAR_Etapa_De_Contabilizacion,
                enumEtapasDeFacturasRec.FAR_Etapa_Anulada,
                enumEtapasDeFacturasRec.FAR_Etapa_Devuelta});
        }

        public static (List<int> estados, enumEtapasDeFacturasRec etapa) EstadosDeLaEtapa(this enumEtapasDeFacturasRec etapa) => (etapa.Lista(), etapa);

        public static List<int> Lista(this enumEtapasDeFacturasRec etapa) => etapa.Estados().ToLista<int>(Simbolos.Coma);

        public static List<enumEtapasDeFacturasRec> Etapas(this FacturaRecDtm factura)
        {
            var etapas = new List<enumEtapasDeFacturasRec>();
            if (factura.EstaEnLaEtapa(enumEtapasDeFacturasRec.FAR_Etapa_De_Cumplimentacion))
                etapas.Add(enumEtapasDeFacturasRec.FAR_Etapa_De_Cumplimentacion);
            if (factura.EstaEnLaEtapa(enumEtapasDeFacturasRec.FAR_Etapa_De_Aprobacion))
                etapas.Add(enumEtapasDeFacturasRec.FAR_Etapa_De_Aprobacion);
            if (factura.EstaEnLaEtapa(enumEtapasDeFacturasRec.FAR_Etapa_De_Contabilizacion))
                etapas.Add(enumEtapasDeFacturasRec.FAR_Etapa_De_Contabilizacion);
            if (factura.EstaEnLaEtapa(enumEtapasDeFacturasRec.FAR_Etapa_De_Pago))
                etapas.Add(enumEtapasDeFacturasRec.FAR_Etapa_De_Pago);
            if (factura.EstaEnLaEtapa(enumEtapasDeFacturasRec.FAR_Etapa_Pagada))
                etapas.Add(enumEtapasDeFacturasRec.FAR_Etapa_Pagada);
            if (factura.EstaEnLaEtapa(enumEtapasDeFacturasRec.FAR_Etapa_Devuelta))
                etapas.Add(enumEtapasDeFacturasRec.FAR_Etapa_Devuelta);
            if (factura.EstaEnLaEtapa(enumEtapasDeFacturasRec.FAR_Etapa_Anulada))
                etapas.Add(enumEtapasDeFacturasRec.FAR_Etapa_Anulada);
            return etapas;
        }

        public static string CadenaDeEtapas(this FacturaRecDtm factura) => string.Join(Simbolos.separadorDeEtapas, factura.Etapas());

        public static List<string> ListaDeEtapas(this FacturaRecDtm factura) => factura.CadenaDeEtapas().ToLista<string>(Simbolos.separadorDeEtapas);

        public static enumEtapasDeFacturasRec Etapa(this FacturaRecDtm factura)
        {
            var etapas = factura.Etapas();
            if (etapas.Count == 0)
                throw new Exception($"No se ha definido la etapa de la {enumNegocio.FacturaRecibida.Singular(true)}, " +
                    $"cuando éste está en el estado {factura.Propiedad<EstadoDtm>(typeof(EstadoDeUnaFacturaRecDtm)).Nombre}");
            if (etapas.Count > 1)
                throw new Exception($"El estado de la factura {enumNegocio.FacturaRecibida.Singular(true)} '{factura.Referencia}' " +
                    $"se encuentra en las etapas {string.Join(',', etapas)} y sólo ha de estar en una");
            return etapas[0];
        }

        public static string Nombre(this enumEtapasDeFacturasRec etapa, bool minusculas = true)
        {
            switch (etapa)
            {
                case enumEtapasDeFacturasRec.FAR_Etapa_De_Cumplimentacion: return minusculas ? "cumplimentación" : "Cumplimentación";
                case enumEtapasDeFacturasRec.FAR_Etapa_De_Aprobacion: return minusculas ? "aprobación" : "Aprobada";
                case enumEtapasDeFacturasRec.FAR_Etapa_De_Contabilizacion: return minusculas ? "contabilización" : "Contabilización";
                case enumEtapasDeFacturasRec.FAR_Etapa_De_Pago: return minusculas ? "de pago" : "De pago";
                case enumEtapasDeFacturasRec.FAR_Etapa_Pagada: return minusculas ? "pagada" : "Pagada";
                case enumEtapasDeFacturasRec.FAR_Etapa_Devuelta: return minusculas ? "devuelta" : "Devuelta";
                case enumEtapasDeFacturasRec.FAR_Etapa_Anulada: return minusculas ? "anulada" : "Anulada";
                case enumEtapasDeFacturasRec.FAR_Etapa_Anterior_A_Contabilidad: return minusculas ? "no contabilizada" : "No contabilizada";
            }
            return etapa.ToString();
        }

        public static decimal ToleranciaEnImportes()
        {
            var tolerancia = enumNegocio.FacturaRecibida.Parametro(enumParametrosDeFacturasRec.FAR_ToleranciaEnImportes, emitirError: false);
            if (tolerancia is null)
            {
                ParametroDeNegocioSql.Crear(NegocioSqls.LeerNegocioPorEnumerado(enumNegocio.FacturaRecibida).Id, enumParametrosDeFacturasRec.FAR_ToleranciaEnImportes, 0.01M.ToString());
                return 0.01M;
            }

            return Convert.ToDecimal(tolerancia.Valor);
        }

        public static bool PermitirFacturasNegativa()
        {
            var permitir = enumNegocio.FacturaRecibida.Parametro(enumParametrosDeFacturasRec.FAR_PermitirFacturasNegativas, emitirError: false);
            if (permitir is null)
            {
                ParametroDeNegocioSql.Crear(NegocioSqls.LeerNegocioPorEnumerado(enumNegocio.FacturaRecibida).Id, enumParametrosDeFacturasRec.FAR_PermitirFacturasNegativas, false.ToString());
                return false;
            }

            return Convert.ToBoolean(permitir.Valor);
        }

        private const string _jsonDeTipoDePreasiento = "[{\"IdTipoFactura\": 0,\"IdTipoPreasiento\": 0}]";


        public static int IdTipoPreasiento(this FacturaRecDtm factura, ContextoSe contexto)
        {
            var json = enumNegocio.FacturaRecibida.Parametro(enumParametrosDeFacturasRec.FAR_Relacion_Con_Tipo_De_Preasiento, crearParametro: true, valorPorDefecto: _jsonDeTipoDePreasiento).Valor;

            var relaciones = ParsearRealacion(json);
            var relacion = relaciones.FirstOrDefault(x => x.IdTipoFactura == factura.IdTipo);
            if (relacion == null)
                GestorDeErrores.Emitir($"Ha de configurar el parámetro '{enumParametrosDeFacturasRec.FAR_Relacion_Con_Tipo_De_Preasiento}' para el id de tipo factura '{factura.IdTipo}'");

            return relacion.IdTipoPreasiento;
        }

        private static List<RelacionDeTiposFarSpr> ParsearRealacion(string json)
        {
            try
            {
                var jsonArray = JArray.Parse(json);
                return jsonArray.Select(item => new RelacionDeTiposFarSpr
                {
                    IdTipoFactura = item["IdTipoFactura"].Value<int>(),
                    IdTipoPreasiento = item["IdTipoPreasiento"].Value<int>(),
                }).ToList();
            }
            catch (Exception ex)
            {
                GestorDeErrores.Emitir($"Error al parsear el json: '{json}' al un objeto del tipo '{_jsonDeTipoDePreasiento}', debe definirlo en el parámetro de negocio '{enumParametrosDeFacturasRec.FAR_Relacion_Con_Tipo_De_Preasiento}'", ex);
            }
            return null;
        }

        public static int Entero(this enumParametrosDeFacturasRec parametro, bool errorSiNoDefinido = true)
        {
            try
            {
                return parametro.ValidarQueEstaDefinido().Entero();
            }
            catch
            {
                if (errorSiNoDefinido)
                    throw;
            }
            return 0;
        }

        public static string Cadena(this enumParametrosDeFacturasRec parametro, bool errorSiNoDefinido = true, string valorPorDefecto = "")
        {
            try
            {
                return parametro.ValidarQueEstaDefinido(valorPorDefecto: valorPorDefecto);
            }
            catch
            {
                if (errorSiNoDefinido)
                    throw;
            }
            return valorPorDefecto;
        }


        public static string ValidarQueEstaDefinido(this enumParametrosDeFacturasRec parametro, string valorPorDefecto = Literal.Cero)
        {
            var valor = enumNegocio.FacturaRecibida.Parametro(parametro, emitirError: false, crearParametro: true, valorPorDefecto: valorPorDefecto).Valor;
            if (valor == valorPorDefecto.ToString())
            {
                GestorDeErrores.Emitir($"Defina el parametro '{parametro}' que sirve para indicar '{parametro.Descripcion()}'");
            }
            return valor;
        }

    }

}
