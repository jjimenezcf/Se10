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
        public string PromptFactura { get; set; }

        public static readonly string Prompt = $@"
Rol: Actúa como un experto en contabilidad y extracción de datos OCR. Tu objetivo es procesar el texto de una factura y devolver un JSON estrictamente estructurado.

Instrucciones de Formateo de Datos:

Fechas: Usa siempre el formato YYYY-MM-DD. Si no existe vencimiento, devuelve null.

Números (Total, BI, IVA, IRPF): Devuelve valores numéricos (decimales), no cadenas. Usa el punto . como separador decimal.

NIF: Elimina puntos, guiones y espacios en blanco.

Clasificación de Pago (Lógica de Negocio):

ClaseDePago: Debe ser exactamente uno de estos: {{enumClaseDePago.Contado.Descripcion()}}, {{enumClaseDePago.Transferencia.Descripcion()}} o {{enumClaseDePago.Remesa.Descripcion()}}.

Nota: Si es domiciliación, tarjeta o efectivo, la clase es Contado.

FormaDePago: Si la clase es Contado, clasifica como: {{enumModoDePagoContado.Contado.Descripcion()}} (para efectivo/contado), {{enumModoDePagoContado.Tarjeta.Descripcion()}} o {{enumModoDePagoContado.Domiciliacion.Descripcion()}}.

Dirección: Desglosa la dirección del emisor con precisión según los campos solicitados.

Restricción Crítica: Devuelve ÚNICAMENTE el objeto JSON. No incluyas introducciones, ni bloques de código Markdown (```json), ni explicaciones. Si un campo no es identificable, su valor debe ser null.

Esquema JSON Requerido:

JSON
{{
  ""Proveedor"": ""Nombre legal o comercial"",
  ""Nif"": ""Solo letras y números"",
  ""eMail"": ""ejemplo@dominio.com"",
  ""Telefono"": ""Número limpio"",
  ""NumeroFactura"": ""Serie y número completo"",
  ""Concepto"": ""Resumen claro de los servicios o productos"",
  ""fecha"": ""YYYY-MM-DD"",
  ""FechaVencimiento"": ""YYYY-MM-DD o null"",
  ""total"": 0.00,
  ""bi"": 0.00,
  ""totalIva"": 0.00,
  ""totalIrpf"": 0.00,
  ""ClaseDePago"": ""Valor del enumerado"",
  ""FormaDePago"": ""Valor del enumerado o null"",
  ""CuentaBancaria"": ""IBAN completo sin espacios"",
  ""CodigoPostal"": ""5 dígitos"",
  ""Pais"": ""Nombre del país"",
  ""Provincia"": ""Nombre de la provincia"",
  ""Municipio"": ""Ciudad/Pueblo"",
  ""TipoDeVia"": ""Calle, Avenida, etc."",
  ""Calle"": ""Nombre de la vía"",
  ""NumeroPolicia"": ""Número y letra si existe"",
  ""RestoDireccion"": ""Piso, puerta o detalles adicionales""
}}
Texto de la factura (OCR):
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


### R9 · Observación
- **Disparador:** si se solicita que contenga alguna o algunas palabras en una observación 
-  **Acción:** Genera el objeto:
   `{""Clausula"": ""observacion"", ""Criterio"": ""contiene"", ""Valor"": ""palabra1;palabra2;...""}`

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


    public class FacturaJson
    {
        public string Proveedor { get; set; }
        public string Nif { get; set; }
        public string eMail { get; set; }
        public string Telefono { get; set; }
        public string NumeroFactura { get; set; }
        public string Concepto { get; set; }
        public string Fecha { get; set; }
        public string FechaVencimiento { get; set; }
        public decimal Total { get; set; } = 0;
        public decimal Bi { get; set; } = 0;
        public decimal TotalIva { get; set; } = 0;
        public decimal TotalIrpf { get; set; } = 0;
        public string FormaDePago { get; set; }
        public string ClaseDePago { get; set; }
        public string CuentaBancaria { get; set; }
        public string CodigoPostal { get; set; }
        public string Pais { get; set; }
        public string Provincia { get; set; }
        public string Municipio { get; set; }
        public string TipoDeVia { get; set; }
        public string Calle { get; set; }
        public string NumeroPolicia { get; set; }
        public string RestoDireccion { get; set; }
    }

}
