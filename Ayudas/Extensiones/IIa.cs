using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Utilidades
{
    public enum enumIa { IaGeminis, IaMistral, IaPerplexity, IaOpenAI, IaDeepSeek, IaApyhub, IaClaude }

    public class ltrIa
    {
        public const string ApiKey_NoDefinida = "No definida";
        public const string Modelo_PorDefecto = "Por defecto";
        public const string Bearer = nameof(Bearer);


        public const string Mensaje_Prohibido = "Las credencial para el uso de la AI '[ia]' está prohibida";
        public const string Mensaje_Bloqueado = "Las credencial para el uso de la AI '[ia]' está bloqueada";
        public const string Mensaje_ApiKey_NoDefinida = "La ApiKey para la ia '[ia]' no es válida, actualicela";
        public const string Mensaje_Demasadas_Peticiones = "Demasadas peticiones a la ia '[ia]', actualice a una versión de pago o pruebe más adelante";

    }

    public interface IIa : IDisposable
    {

        public const string IA_ApiKey = nameof(IA_ApiKey);
        public const string IA_Usada = nameof(IA_Usada);

        public const string IA_ApiKey_No_Valida = $"Ha de definir la ApiKey para la Ia seleccionada";

        public const string IA_Modelo_No_Valido = $"Modelo no válido";

        public const string IA_Seleccionada_No_Valida = $"La Ia indicada no es válida. Variable: '{IIa.IA_Usada}'";

        private string ApiKey { get { return null; } set { } }

        private HttpClient Cliente { get { return null; } set { } }

        public Task<string> Resumir(string origen);

        public Task<string> AnalizarFactura(string origen);

        public Task<string> AnalizarTextoParaFiltros(string origen);

    }

    public interface IIaPromptFactura
    {
        public const string ReglasEspecificas = "[ReglasEspecificas]";
        public static readonly string SinReglas = "Sin reglas específicas";

        public string PromptFactura { get; set; }

        public static readonly string Prompt = $@"
# ROL
Eres un experto en contabilidad española y extracción de datos de facturas.
Tu objetivo es analizar el contenido recibido —texto OCR de una imagen/PDF escaneado, o XML en formato eFactura 3.2/3.2.1/3.2.2, o XML en formato UBL 2.1/2.5— y devolver **únicamente** un objeto JSON estrictamente estructurado según el esquema indicado al final de este prompt.

---

# DETECCIÓN DEL FORMATO DE ENTRADA

Antes de extraer datos, identifica el formato:

- Si el contenido contiene `urn:oasis:names:specification:ubl:schema:xsd:Invoice-2` → formato **UBL 2.1/2.5**. Extrae los datos directamente del XML estructurado.
- Si el contenido contiene `http://www.facturae.es/` o el elemento raíz es `fe:Facturae` → formato **eFactura 3.2.2**. Extrae los datos directamente del XML estructurado.
- En cualquier otro caso → trata el contenido como **texto OCR** de una factura escaneada o impresa.

---

# REGLAS DE FORMATEO

- **Fechas**: Siempre `YYYY-MM-DD`. Si no existe, devuelve `null`.
- **Números** (importes, porcentajes): Valores decimales numéricos, sin cadenas. Separador decimal: punto `.`. Sin símbolos de moneda.
- **NIF**: Solo letras y números, sin puntos, guiones ni espacios. Si lleva prefijo de país (p.ej. `ESB73961450`), consérvalo tal cual.
- **Campo no identificable**: Su valor debe ser `null`.
- **Clasificación de pago**:
  - `ClaseDePago` debe ser exactamente uno de: `{enumClaseDePago.Contado.Descripcion()}`, `{enumClaseDePago.Transferencia.Descripcion()}` o `{enumClaseDePago.Remesa.Descripcion()}`.
  - Domiciliación, tarjeta o efectivo → `{enumClaseDePago.Contado.Descripcion()}`.
  - Si `ClaseDePago` es Contado, `FormaDePago` debe ser: `{enumModoDePagoContado.Contado.Descripcion()}` (efectivo), `{enumModoDePagoContado.Tarjeta.Descripcion()}` o `{enumModoDePagoContado.Domiciliacion.Descripcion()}`.

---

# MAPEO DE CAMPOS POR FORMATO XML

## UBL 2.1/2.5

| Campo JSON             | Origen en el XML UBL                                                                   |
|------------------------|----------------------------------------------------------------------------------------|
| NumeroFactura          | cbc:ID                                                                                 |
| Fecha                  | cbc:IssueDate                                                                          |
| FechaVencimiento       | cac:PaymentMeans/cbc:PaymentDueDate                                                    |
| Proveedor              | cac:AccountingSupplierParty/cac:Party/cac:PartyName/cbc:Name                           |
| Nif                    | cac:AccountingSupplierParty/cac:Party/cac:PartyTaxScheme/cbc:CompanyID                 |
| BaseImponible          | cac:LegalMonetaryTotal/cbc:TaxExclusiveAmount                                          |
| TotalIva               | cac:TaxTotal/cbc:TaxAmount                                                             |
| TotalIrpf              | cac:WithholdingTaxTotal/cbc:TaxAmount (si existe)                                      |
| Total                  | cac:LegalMonetaryTotal/cbc:PayableAmount                                               |
| Concepto               | cbc:Note (primera nota relevante)                                                      |
| CuentaBancaria         | cac:PaymentMeans/cac:PayeeFinancialAccount/cbc:ID                                      |
| Lineas[n].Concepto     | cac:InvoiceLine/cac:Item/cbc:Description                                               |
| Lineas[n].BaseImponible| cac:InvoiceLine/cbc:LineExtensionAmount                                                |
| Lineas[n].PorcentajeIva| cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cbc:Percent               |
| Lineas[n].ImporteIva   | cac:InvoiceLine/cac:TaxTotal/cbc:TaxAmount                                             |
| Lineas[n].Exenta       | true si cac:TaxCategory/cbc:ID es 'E' o TaxExemptionCode está presente                |

## eFactura 3.2/3.2.1/3.2.2

| Campo JSON              | Origen en el XML eFactura                                                                    |
|-------------------------|----------------------------------------------------------------------------------------------|
| NumeroFactura           | fe:Invoices/fe:Invoice/fe:InvoiceHeader/fe:InvoiceNumber                                     |
| Fecha                   | fe:InvoiceIssueData/fe:IssueDate                                                             |
| Concepto                | fe:InvoiceIssueData/fe:InvoiceDescription                                                    |
| FechaVencimiento        | fe:PaymentDetails/fe:Installment/fe:InstallmentDueDate (primer plazo)                        |
| Proveedor               | fe:Parties/fe:SellerParty/.../fe:CorporateName                                               |
| Nif                     | fe:Parties/fe:SellerParty/fe:TaxIdentification/fe:TaxIdentificationNumber                    |
| BaseImponible           | fe:InvoiceTotals/fe:TotalGrossAmountBeforeTaxes                                              |
| TotalIva                | fe:InvoiceTotals/fe:TotalTaxOutputs                                                          |
| TotalIrpf               | fe:InvoiceTotals/fe:TotalTaxesWithheld                                                       |
| Total                   | fe:InvoiceTotals/fe:InvoiceTotal                                                             |
| Lineas[n].Concepto      | fe:Items/fe:InvoiceLine/fe:ItemDescription                                                   |
| Lineas[n].BaseImponible | fe:Items/fe:InvoiceLine/fe:GrossAmount                                                       |
| Lineas[n].PorcentajeIva | fe:Items/fe:InvoiceLine/fe:TaxesOutputs/fe:Tax/fe:TaxRate (cuando TaxTypeCode es '01')       |
| Lineas[n].ImporteIva    | fe:Items/fe:InvoiceLine/fe:TaxesOutputs/fe:Tax/fe:TaxAmount/fe:TotalAmount                   |
| Lineas[n].Exenta        | true si fe:Tax/fe:TaxTypeCode es '05' (operación exenta)                                     |
| Irpf.PorcentajeRetencion| fe:Items/fe:InvoiceLine/fe:TaxesWithheld/fe:Tax/fe:TaxRate, o si no, TaxesOutputs con TaxTypeCode '03' o '04' |
| Irpf.BaseRetencion      | fe:InvoiceTotals/fe:TotalTaxesWithheld (base del importe retenido)                           |
| Irpf.ImporteRetencion   | fe:InvoiceTotals/fe:TotalTaxesWithheld (importe total retenido)                              |

---

# REGLA DE CUADRE

Verifica siempre antes de responder:
`BaseImponible` + `TotalIva` - `TotalIrpf` = `Total`

La suma de todos los `Lineas[n].BaseImponible` debe coincidir con `BaseImponible` de cabecera.
Si detectas discrepancia, usa los valores más fiables del documento (normalmente los totales de cabecera).
Si no hay retención IRPF, devuelve `""Irpf"": null`.

---

# RESTRICCIÓN CRÍTICA

Devuelve **ÚNICAMENTE** el objeto JSON. Sin introducciones, sin bloques de código Markdown, sin explicaciones.

---

# ESQUEMA JSON REQUERIDO

{{
  ""Proveedor"": ""Nombre legal o comercial"",
  ""Nif"": ""Solo letras y números (con prefijo país si lo tiene)"",
  ""eMail"": ""ejemplo@dominio.com"",
  ""Telefono"": ""Número limpio"",
  ""NumeroFactura"": ""Serie y número completo"",
  ""Concepto"": ""Resumen claro de los servicios o productos"",
  ""Fecha"": ""YYYY-MM-DD"",
  ""FechaVencimiento"": ""YYYY-MM-DD o null"",
  ""Total"": 0.00,
  ""BaseImponible"": 0.00,
  ""TotalIva"": 0.00,
  ""TotalIrpf"": 0.00,
  ""ClaseDePago"": ""Valor del enumerado"",
  ""FormaDePago"": ""Valor del enumerado o null"",
  ""CuentaBancaria"": ""IBAN completo sin espacios o null"",
  ""CodigoPostal"": ""5 dígitos"",
  ""Pais"": ""Nombre del país"",
  ""Provincia"": ""Nombre de la provincia"",
  ""Municipio"": ""Ciudad/Pueblo"",
  ""TipoDeVia"": ""Calle, Avenida, etc."",
  ""Calle"": ""Nombre de la vía"",
  ""NumeroPolicia"": ""Número y letra si existe"",
  ""RestoDireccion"": ""Piso, puerta o detalles adicionales"",
  ""Lineas"": [
    {{
      ""Concepto"": ""Descripción del servicio o producto"",
      ""BaseImponible"": 0.00,
      ""PorcentajeIva"": 0.00,
      ""ImporteIva"": 0.00,
      ""Exenta"": false
    }}
  ],
  ""Irpf"": {{
    ""PorcentajeRetencion"": 0.00,
    ""BaseRetencion"": 0.00,
    ""ImporteRetencion"": 0.00
  }}
}}

---

# REGLAS ESPECÍFICAS DE IMPLANTACIÓN

[ReglasEspecificas]

---

Contenido a analizar:
[CONTENIDO_FACTURA]";

    }

    public interface IIaPromptResumen
    {
        public string PromptResumen { get; set; }

        public static readonly string PromptDeResumenDeFichero = $"Por favor, puedes resumir y explicar texto del fichero adjunto y darme la respuesta en español";

        public static readonly string PromptDeResumen = $"Por favor, puedes resumir y explicar el siguiente texto y darme la respuesta en español:\n\r[CONTENIDO]";

        public static readonly string PromptDePago = @"De este documento de pago me puedes indicar en un json los siguientes campos (y solo quiero el json, no quiero explicaciones). 
Si el campo no lo consigues devuelveme un nulo.
Campos del Json: Ordenante, Benificiario, BancoDestino, CuentaOrigen, CuentaDestino, FechaDeEmisión, FechaValor, Importe

La información de la cuenta origen y destino puedes dármela desglosada en 4 dígitos 
CuentaOrigen: CodigoIbanOrigen (4 caracteres), por ejemplo ES36, EntidadOrigen (4 dígitos) , SucursalOrigen(4 digitos) , CodigoControlOrigen (2 dígitos), NumeroCuentaOrigen(10)
CuentaDesino: CodigoIbanDestino (4 caracteres), por ejemplo ES36, EntidadDestino (4 dígitos) , SucursalDestino(4 digitos) , CodigoControlDestino (2 dígitos), NumeroCuentaDestino(10)

El resultado debería ser algo similar a este json
{
  ""Ordenante"": ""JUAN ANTONIO JIMENEZ CERVANTES FRIGOLS"",
  ""Beneficiario"": ""ELVIRA FUSTER MARQUINA"",
  ""BancoDestino"": ""ING BANK NV SPANISH BRANCH (DIRECT)"",
  ""CuentaOrigen"": {
    ""CodigoIbanOrigen"": ""ES36"",
    ""EntidadOrigen"": ""0128"",
    ""SucursalOrigen"": ""0650"",
    ""CodigoControlOrigen"": ""13"",
    ""NumeroCuentaOrigen"": ""0100122343""
  },
  ""cuentaDestino"": {
    ""codigoIbanDestino"": null,
    ""EntidadDestino"": ""1465"",
    ""SucursalDestino"": ""0100"",
    ""CodigoControlDestino"": ""91"",
    ""NumeroCuentaDestino"": ""1708359630""
  },
  ""FechaDeEmisión"": ""17-03-25"",
  ""FechaValor"": ""16-03-25"",
  ""Importe"": 500.00
}

Te adjunto el contenido a analizar
[CONTENIDO]";

    }



    public interface IIaPromptFiltrar
    {
        public const string ListaDeCentrosGestores = "[ListaDeCentrosGestores]";
        public const string ListaDeTipos = "[ListaDeTipos]";
        public const string ListaDeEstados = "[ListaDeEstados]";
        public const string ListaDeTransiciones = "[ListaDeTransiciones]";
        public const string Texto = "[Texto]";
        public const string FechaDeHoy = "[FechaDeHoy]";
        public const string ListaDeEtapas = "[ListaDeEtapas]";
        public const string ReglasEspecíficas = "[ReglasEspecíficas]";
        public const string NegocioTratado = "[NegocioTratado]";
        public const string ListaDeUsuarios = "[ListaDeUsuarios]";
        public const string HistorialDeSesion = "[HistorialDeSesion]";

        public string PromptFiltrar { get; set; }

        public static readonly string SinReglas = "Sin reglas específicas";
        public static readonly string Prompt = @"# PROMPT DE EXTRACCIÓN DE FILTROS
**ROL:**
Eres un experto en extracción de entidades y generación de filtros estructurados. Tu único objetivo es transformar una solicitud en lenguaje natural en un array JSON de objetos `ClausulaDeFiltrado`, siguiendo estrictamente las reglas de este documento.
La solicitud está en el apartado **TEXTO A ANALIZAR**.
Puede ser que se adjunte un historial de preguntas y respuestas anteriores, que se encuentra en el apartado **Preguntas anteriores**. Usa esta información para entender mejor el contexto, pero no la uses para generar los filtros a menos que se indique explícitamente en la pregunta actual.
Si al generar la respuesta hay ambiguedad o contradición con las respuestas anteriores, serán prioritarias las respuesta actual

**ESTRUCTURA DEL OBJETO:**
`{ ""Clausula"": ""string"", ""Criterio"": ""enum"", ""Valor"": ""string"" }`

**Criterios permitidos:** `igual`, `mayor`, `menor`, `esNulo`, `noEsNulo`, `contiene`, `noContiene`, `comienza`, `termina`, `mayorIgual`, `menorIgual`, `diferente`, `esAlgunoDe`, `noEsNingunoDe`, `entreFechas`, `porReferencia`, `entreImportes`, `entreRangos`.

---

## REGLAS — APLICAR EN ESTE ORDEN:

### R1 · Centros Gestores (`IdCg`)
- Si el texto coincide con nombres de `CONTEXTO DE DATOS:: Centros Gestores`: usa Clausula `""IdCg""` y Criterio `""esAlgunoDe""`. Valor: IDs numéricos separados por coma. Si solo hay una coincidencia entonces el criterio puede ser `""igual""`
- Si no hay coincidencia exacta: usa `""NombreCg""` con Criterio `""contiene""`.

### R2 · Tipos (`IdTipo`)
- Si el texto coincide con nombres de `CONTEXTO DE DATOS:: Tipos`: usa Clausula `""IdTipo""` y Criterio `""esAlgunoDe""`. Valor: IDs numéricos separados por coma. Si sólo hay una coincidencia entonces el criterio puede ser `""igual""`

### R3 · Visibilidad General (`quemostrar`)
- ""cancelados"" → Valor: `""2""`
- ""terminados"" → Valor: `""3""`
- ""en curso Y terminados"" / ""excluir cancelados"" → Valor: `""8""`
- ""todos (incluyendo cancelados)"" → Valor: `""9""`
- *Nota:* Si el usuario pide ""en curso"", ""activos"" o un estado concreto, no generes esta cláusula a menos que lo exijan las reglas R4.

### R4 · Lógica de Estados (Historial vs. Actual)

#### R4.1 · Estado en un Periodo (Historial de estados)
- **Disparador:** Elementos que ""estuvieron"", ""estaban"", ""pasaron por"" o frases tipo ""[estado] en [periodo]"" (ej: ""asignadas en febrero"").
- **Acción:** Genera dos objetos:
  1. `{""Clausula"": ""IdsDeEstado"", ""Criterio"": ""esAlgunoDe"", ""Valor"": ""IDs_encontrados""}`
  2. `{""Clausula"": ""FechasDeEstado"", ""Criterio"": ""entreFechas"", ""Valor"": ""inicio-fin""}`
  3.  Busca el ID del estado en `CONTEXTO DE DATOS:: Estados`


#### R4.2 · transicion en un Periodo (Historial de transiciones)
- **Disparador:** Elementos que han  ""transitado"", ""enviado"", ""devuelto"", ""retrocedido"", ""con observacion"" o frases tipo ""[transición] en [periodo]"" (ej: ""finalizadas en febrero"").
- **Acción:** Genera dos objetos:
  1. `{""Clausula"": ""IdsDeTransicion"", ""Criterio"": ""esAlgunoDe"", ""Valor"": ""IDs_encontrados""}`
  2. `{""Clausula"": ""FechasDeTransicion"", ""Criterio"": ""entreFechas"", ""Valor"": ""inicio-fin""}`
  3.  Busca el ID de la transición en `CONTEXTO DE DATOS:: Transiciones`
  
#### R4.3 · Estado Actual
- **Disparador:** Elementos que ""están"", ""son"" o mención directa del estado (ej: ""dame las asignadas"").
- **Acción:** Genera el objeto:
  1. si se encuenta Id encontrado en `CONTEXTO DE DATOS:: Estados` `{""Clausula"": ""idestado"", ""Criterio"": ""igual"", ""Valor"": ""Id""}`
  2. si no se encuenta Id encontrado en `CONTEXTO DE DATOS:: Estados` y parte de los nombres de estado se encuentran en `CONTEXTO DE DATOS:: Estados` `{""Clausula"": ""estados"", ""Criterio"": ""contiene"", ""Valor"": ""nombre1;nombre2;...""}`

### R5 · Fecha de Creación (`fechacreacion`)
- Usa esta cláusula si el usuario pide fechas pero **NO** se cumple el disparador de historial (R4.1), o si usa palabras como ""creadas o creados"", ""nuevas o nuevos"" o ""de hoy"".
- `{""Clausula"": ""fechacreacion"", ""Criterio"": ""entreFechas"", ""Valor"": ""inicio-fin""}
- formato: YYYY-MM-DDTHH:mm:ssZ-YYYY-MM-DDTHH:mm:ssZ

### R6 · Fecha de modificación (`fechamodificacion`)
- Usa esta cláusula si el usuario pide fechas pero **NO** se cumple el disparador de historial (R4.1), o si usa palabras como ""modificadas o modificados"", ""variadas o variados"" .
- `{""Clausula"": ""fechamodificacion"", ""Criterio"": ""entreFechas"", ""Valor"": ""inicio-fin""}
- formato: YYYY-MM-DDTHH:mm:ssZ-YYYY-MM-DDTHH:mm:ssZ

### R7 Nombre o asunto del elemento o objeto
- **Disparador:** si se solicita que el nombre del elemento contenga algunas palabras indicadas
-  **Acción:** Genera el objeto:
   `{""Clausula"": ""nombre"", ""Criterio"": ""contiene"", ""Valor"": ""palabra1;palabra2;...""}`


### R8 Referencia del elemento o objeto
- **Disparador:** si se solicita que la referencia del elemento contenga algunas palabras indicadas
-  **Acción:** Genera el objeto:
   `{""Clausula"": ""referencia"", ""Criterio"": ""contiene"", ""Valor"": ""palabra1;palabra2;...""}`


### R9 · Búsqueda por contenido semántico

**R9.1 · Observación explícita**
- **Disparador:** El usuario menciona explícitamente ""observación"", ""con nota"", ""cuya observación contenga"", ""que tengan la observación"", o construcciones que citan literalmente el campo.
- **Acción:** Genera el objeto:
  `{""Clausula"": ""observacion"", ""Criterio"": ""contiene"", ""Valor"": ""texto_literal_indicado""}`

**R9.2 · Descripción / Temática implícita**
- **Disparador:** El usuario usa frases como ""que se refieran a [X]"", ""sobre [X]"", ""acerca de [X]"", ""relacionadas con [X]"", ""que traten de [X]"", ""del tema [X]"", ""referentes a [X]"" **sin mencionar** explícitamente la palabra ""observación"".
- **Acción:** Extrae las palabras clave de [X] aplicando la regla de limpieza de palabras vacías (ver REGLAS TRANSVERSALES) y genera:
  `{""Clausula"": ""descripcion"", ""Criterio"": ""esAlgunoDe"", ""Valor"": ""palabra1;palabra2;...""}`
- **Ejemplo:** ""que se refieran a la seguridad de acceso a contratos"" → eliminar artículos/preposiciones → `""seguridad;acceso;contratos""` → `{""Clausula"": ""descripcion"", ""Criterio"": ""esAlgunoDe"", ""Valor"": ""seguridad;acceso;contratos""}`

### R10 · nombre de archivo (nombredearchivo)
- **Disparador:** si se solicita que contenga algun archivo adjunto y con nombre de archivo : (palabra 1, palabra 2, etc)
-  **Acción:** Genera el objeto:
   `{""Clausula"": ""nombredearchivo"", ""Criterio"": ""contiene"", ""Valor"": ""palabra1;palabra2;...""}`
   
### R11 · usuario creador (idusuacrea)
- **Disparador:** si se solicita saber los registros que ha creado un usuario habrá de buscarse en la tabla `CONTEXTO DE DATOS:: Usuarios` el nombre de usuario o login
-  **Acción:** Genera el objeto 1 y si se pide cuándo generar el 2:
  1. `{""Clausula"": ""idusuacrea"", ""Criterio"": ""igual"", ""Valor"": ""id encontrado""}`
  2. `{""Clausula"": ""fechacreacion"", ""Criterio"": ""entreFechas"", ""Valor"": ""inicio-fin""}`
  3.  Busca el ID del usuario en `CONTEXTO DE DATOS:: Usuarios`
  
### R12 · usuario modificador (idusuamodi)
- **Disparador:** si se solicita saber los registros que ha modificado un usuario habrá de buscarse en la tabla `CONTEXTO DE DATOS:: Usuarios` el nombre de usuario o login
-  **Acción:** Genera el objeto 1 y si se pide cuándo generar el 2:
  1. `{""Clausula"": ""idusuamodi"", ""Criterio"": ""igual"", ""Valor"": ""id encontrado""}`
  2. `{""Clausula"": ""fechamodificacion"", ""Criterio"": ""entreFechas"", ""Valor"": ""inicio-fin""}`
  3.  Busca el ID del usuario en `CONTEXTO DE DATOS:: Usuarios`

  
### R13 · Etapas de un elemento de negocio (`FiltroPorEtapa`)

Esta regla actúa como el **filtro de conveniencia de negocio** para el estado actual del elemento. Tiene prioridad sobre la **R4 (Lógica de Estados Históricos)** siempre que el usuario se exprese en tiempo presente.

#### R13.1 · Definición de la Acción y Lógica de Operadores
- **Disparador:** Frases como ""en etapa [Etapa]"", ""que estén [Estado]"", ""que sean [Etapa1] y [Etapa2]"", ""que no estén en [Etapa]"".
- **Lógica de Intersección (AND) - Predeterminada:** 
  - Si el usuario solicita que el elemento cumpla varias condiciones simultáneamente (ej: ""pagadas y cumplimentadas""), se deben generar **objetos independientes** por cada etapa.
  - **Acción:** Generar N objetos `{""Clausula"": ""FiltroPorEtapa"", ""Criterio"": ""igual"", ""Valor"": ""ETAPA_N""}`.
- **Lógica de Unión (OR) - Explícita:** 
  - Solo si el usuario utiliza la conjunción **""o""** (ej: ""en etapa A o B""), se consolidarán los valores en un único objeto.
  - **Acción:** `{""Clausula"": ""FiltroPorEtapa"", ""Criterio"": ""igual"", ""Valor"": ""VALOR1|VALOR2""}`.
- **Lógica de Exclusión (NOT):** 
  - Si el usuario pide omitir o excluir (ej: ""excepto las [Etapa]""), se usa el criterio de diferencia.
  - **Acción:** `{""Clausula"": ""FiltroPorEtapa"", ""Criterio"": ""diferente"", ""Valor"": ""ETAPA_X""}`.

#### R13.2 · Mapeo de Etapas
El modelo debe mapear los conceptos del usuario (ej: ""pagada"", ""pendiente"") a los nombres técnicos definidos en el `CONTEXTO DE DATOS` (ej: `FAR_Etapa_Pagada`, `FAR_Etapa_en_cumplimentacion`).

#### R13.3 · Ejemplos de Respuesta

- **Frase:** *""Dame las facturas pagadas y cumplimentadas""* (Intersección AND)
  1. `{""Clausula"": ""FiltroPorEtapa"", ""Criterio"": ""igual"", ""Valor"": ""FAR_Etapa_Pagada""}`
  2. `{""Clausula"": ""FiltroPorEtapa"", ""Criterio"": ""igual"", ""Valor"": ""FAR_Etapa_en_cumplimentacion""}`

- **Frase:** *""Dame los elementos de este mes excepto las yyyyy""* (Exclusión)
  1. `{""Clausula"": ""FiltroPorFechaDeEmision"", ""Criterio"": ""entreFechas"", ""Valor"": ""...""}`
  2. `{""Clausula"": ""FiltroPorEtapa"", ""Criterio"": ""diferente"", ""Valor"": ""yyyyy""}`

- **Frase:** *""Dame las yyyyy pero que no estén zzzzz""* (Inclusión + Exclusión)
  1. `{""Clausula"": ""FiltroPorEtapa"", ""Criterio"": ""igual"", ""Valor"": ""yyyyy""}`
  2. `{""Clausula"": ""FiltroPorEtapa"", ""Criterio"": ""diferente"", ""Valor"": ""zzzzzz""}`

**Jerarquía:** Si el usuario usa verbos en pasado (""estuvo"", ""pasó por"") o pide un rango de fechas para el estado (ej: ""que estuvieran en X el mes pasado""), **ignora esta regla R13** y usa obligatoriamente la **R4 (Lógica de Estados Históricos)**.

---

## REGLAS TRANSVERSALES:
- **Sin duplicados:** Agrupa valores de la misma cláusula (IDs con coma, texto con ;).
- **Formato de Valor en Fechas:** Siempre usa el guion `-` como separador de rango: `FECHA_INICIO-FECHA_FIN`.
- **Salida:** Devuelve ÚNICAMENTE el array JSON. Si no hay filtros, devuelve `[]`.
- **Limpieza de palabras vacías en `esAlgunoDe` textual:** Cuando el criterio es `esAlgunoDe` y el Valor son palabras de texto (no IDs numéricos), elimina los artículos, preposiciones y conjunciones del español antes de construir el Valor. Lista de palabras a eliminar: `el, la, los, las, un, una, unos, unas, de, del, al, a, en, con, por, para, que, se, y, o, e, u, no, ni, su, sus, lo, le, les, es, son, esto, esta, esos, esas`. Cada palabra clave resultante se separa con `;`.

---


## REGLAS EXPECIFICAS DEL ELEMENTO:
- **Objetos/registros/elementos gestionados:** [NegocioTratado]
- **Reglas específicas de los objetos gestionados:
 [ReglasEspecíficas]

---

## CONTEXTO DE DATOS:
- **Fecha de Hoy:** [FechaDeHoy]
- **Centros Gestores (ID - Nombre):** [ListaDeCentrosGestores]
- **Tipos (ID - Nombre):** [ListaDeTipos]
- **Estados (ID - Nombre - Flags):**[ListaDeEstados]
- **Transiciones (ID - Estado Origen- Nombre - Estado final):** [ListaDeTransiciones]
- **Usuarios (ID - Nombre):** [ListaDeUsuarios]
- **Etapas (Nombre etapa - Descripción):** [ListaDeEtapas]

---

## TEXTO A ANALIZAR:
[Texto]
Preguntas anteriores
[HistorialDeSesion]
"";";
    }

    public interface IIaTiposMimesAdmitidos
    {
        public HashSet<string> TiposMimeAdmitidosParaResumen { get; set; }
        public HashSet<string> TiposMimeAdmitidosParaFacturas { get; set; }

        public Task<string> ResumirConOcr(string rutaArchivo);

        public Task<string> AnalizarFacturaConOcr(string rutaArchivo);
    }

}
