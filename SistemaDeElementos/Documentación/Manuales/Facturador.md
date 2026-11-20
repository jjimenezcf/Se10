# Manual del programador — API del Facturador

Este documento describe cómo un sistema externo puede **dar de alta clientes, emitir facturas, rectificarlas y descargar su PDF/XML** en Se10 mediante llamadas HTTP.

## Índice

1. [Conceptos básicos](#1-conceptos-básicos)
2. [Resumen de las llamadas](#2-resumen-de-las-llamadas)
3. [Formato de las respuestas](#3-formato-de-las-respuestas)
4. [Definición de las llamadas](#4-definición-de-las-llamadas)
   - [4.1 Crear un cliente](#41-crear-un-cliente--epcrearcliente)
   - [4.2 Crear una factura (modo directo)](#42-crear-una-factura-modo-directo--epcrearfactura)
   - [4.3 Crear una factura (modo en dos pasos)](#43-crear-una-factura-modo-en-dos-pasos--epsolicitarfacturador--epcrearfacturaconguid)
   - [4.4 Solicitar el PDF o el XML](#44-solicitar-el-pdf-o-el-xml--epsolicitarpdf--epsolicitarxml)
   - [4.5 Descargar el fichero](#45-descargar-el-fichero--epdescargaconguid)
   - [4.6 Rectificar una factura por datos erróneos](#46-rectificar-una-factura-por-datos-erróneos--eprectificarporde)
5. [Estructura del JSON de una factura](#5-estructura-del-json-de-una-factura)
6. [Estructura del JSON de un cliente](#6-estructura-del-json-de-un-cliente)
7. [Errores habituales](#7-errores-habituales)
8. [Herramienta de pruebas: TestFacturador](#8-herramienta-de-pruebas-testfacturador)
9. [Configuración del facturador en Se10 (apiKey y mapeos)](#9-configuración-del-facturador-en-se10-apikey-y-mapeos)
10. [Anexo — Funcionamiento interno](#10-anexo--funcionamiento-interno)

---

## 1. Conceptos básicos

| Concepto | Descripción |
|---|---|
| **Url base** | `https://biwe.femdek.com` (producción). Todas las rutas de este manual cuelgan de ella. |
| **Sociedad emisora** | La sociedad que factura. Se identifica por su NIF (parámetro `nif` o `nifEmisor`). |
| **apiKey** | Clave que identifica al *facturador* de la sociedad emisora. La proporciona el administrador de Se10. Es secreta: no debe exponerse en un navegador ni en código cliente público. |
| **Autenticación** | No hay login ni cookies: cada llamada lleva el `apiKey` en la query string (salvo `epCrearFacturaConGuid` y la descarga, que usan un guid). |
| **Número de factura** | Formato `Año-Serie-Número`, p. ej. `2026-S-123`. Es el que devuelve Se10 al crear la factura (`numeroFactura`). |
| **Guids de consulta** | Al crear una factura se reciben `guidDeConsultaPdf` y `guidDeConsultaXml`. **Hay que guardarlos**: son permanentes y hacen falta para pedir después el PDF o el XML. |

Reglas generales:

- **Codificar la query string**: todos los valores deben ir *URL-encoded* (p. ej. con `encodeURIComponent` en JavaScript o `Uri.EscapeDataString` en .NET).
- **El JSON enviado** (body) debe usar exactamente los nombres de propiedad de este manual (respetando mayúsculas). Una propiedad desconocida provoca un error.
- **El servidor responde siempre HTTP 200**. El éxito o el fallo se sabe leyendo el campo `estado` de la respuesta (ver [apartado 3](#3-formato-de-las-respuestas)).
- Las llamadas que crean algo (`POST`) **no son idempotentes**, salvo `epCrearCliente`: repetir `epCrearFactura` crea otra factura.

## 2. Resumen de las llamadas

| # | Operación | Método | Ruta | Autenticación | Body |
|---|---|---|---|---|---|
| 4.1 | Crear / actualizar cliente | `POST` | `/Facturador/epCrearCliente?nif=&apiKey=` | apiKey | JSON del cliente |
| 4.2 | Crear factura (directo) | `POST` | `/Facturador/epCrearFactura?nif=&apiKey=` | apiKey | JSON de la factura |
| 4.3 | Reservar guid para crear factura | `GET` | `/Facturador/epSolicitarFacturador?nifEmisor=&apiKey=&peticion=CrearFactura` | apiKey | — |
| 4.3 | Crear factura con guid | `POST` | `/Facturador/epCrearFacturaConGuid?nif=&guid=` | guid operacional | JSON de la factura |
| 4.4 | Pedir url de descarga del PDF | `GET` | `/Facturador/epSolicitarPdf?nif=&apiKey=&numeroFactura=&guid=` | apiKey + guidDeConsultaPdf | — |
| 4.4 | Pedir url de descarga del XML | `GET` | `/Facturador/epSolicitarXml?nif=&apiKey=&numeroFactura=&guid=` | apiKey + guidDeConsultaXml | — |
| 4.5 | Descargar el fichero | `GET` | `/Archivos/epDescargaConGuid?guid=&id=` | guid de descarga (1 h) | — |
| 4.6 | Rectificar por datos erróneos | `POST` | `/Facturador/epRectificarPorDe?apiKey=&numeroFactura=` | apiKey | Texto plano (motivo) |

Flujo típico:

```
epCrearCliente (opcional, si el cliente puede no existir)
      │
      ▼
epCrearFactura ──► guardar numeroFactura + guidDeConsultaPdf + guidDeConsultaXml
      │
      ▼ (cuando se necesite el documento)
epSolicitarPdf / epSolicitarXml ──► url de descarga (válida 1 hora)
      │
      ▼
GET url de descarga ──► fichero PDF / XML
```

## 3. Formato de las respuestas

Todas las llamadas (salvo la descarga del fichero, que devuelve el fichero) devuelven HTTP 200 con este "sobre" JSON:

```json
{
  "estado": "Ok",
  "mensaje": null,
  "consola": "Texto descriptivo del resultado",
  "total": 0,
  "datos": "<depende de la llamada: texto u objeto>",
  "modoDeAcceso": "Consultor",
  "logout": false
}
```

| Campo | Descripción |
|---|---|
| `estado` | `"Ok"` o `"Error"`. **Es el campo que hay que mirar** para saber si la llamada ha ido bien. |
| `mensaje` | Con `estado = "Error"`: texto del error, apto para mostrar al usuario. Con `"Ok"` normalmente viene a `null`. |
| `consola` | Con `"Ok"`: texto descriptivo del resultado. Con `"Error"`: detalle técnico del error (útil para el log). |
| `datos` | El resultado de la llamada (ver cada llamada). Con error suele venir a `null` (ver excepción más abajo). |
| `total`, `modoDeAcceso`, `logout` | Campos internos; ignorarlos. |

Respuesta con error (ejemplo):

```json
{
  "estado": "Error",
  "mensaje": "La sociedad '00811725D' no tiene ningún facturador para el ApiKey 'XXXXXXXX' proporcionado",
  "consola": "Detalle técnico del error",
  "total": 0,
  "datos": null,
  "modoDeAcceso": null,
  "logout": false
}
```

> **Nombres en camelCase.** Las propiedades de la respuesta llegan en camelCase (`estado`, `datos`, `numeroFactura`...). Se recomienda deserializar sin distinguir mayúsculas/minúsculas e **ignorar las propiedades desconocidas**: el objeto `datos` de las facturas lleva además campos técnicos internos (`id`, `rowVersion`, `estaCancelada`...) que pueden cambiar.

> **Excepción en las llamadas que crean factura** (4.2, 4.3 y 4.6): si la prefactura llegó a crearse pero la emisión falló, la respuesta trae `estado = "Error"` **y** `datos` relleno con el objeto de la factura, cuyo campo `datos.mensaje` explica qué falló.

## 4. Definición de las llamadas

En los ejemplos: `nif` de la sociedad emisora `00811725D`, `apiKey` `XXXXXXXX`, número de factura `2026-S-123`. Sustituirlos por los valores reales.

### 4.1 Crear un cliente — `epCrearCliente`

Da de alta el cliente al que se va a facturar (o actualiza sus datos si ya existe). Es opcional: solo hace falta si el cliente (`NifDelCliente` de la factura) puede no existir aún en Se10. **Es seguro repetirla**: busca el cliente por su NIF y no lo duplica.

**Petición**

```
POST https://biwe.femdek.com/Facturador/epCrearCliente?nif=00811725D&apiKey=XXXXXXXX
Content-Type: application/json

{
  "NIF": "B73954455",
  "Nombre": "Cliente Ejemplo SL",
  "eMail": "administracion@ejemplo.com",
  "Telefono": "915559876",
  "Municipio": "Madrid",
  "CodigoPostal": "28001",
  "TipoDeVia": "Calle",
  "Calle": "Serrano",
  "Numero": 45,
  "ValidarEnLaAeat": false,
  "SustituirDatosIdentificativos": true,
  "SustituirDatosDeContacto": true,
  "CrearCalleSiNoExiste": true,
  "ValidarEnCatastro": true
}
```

Ejemplo de persona física (lleva `Apellidos` y `Nombre` es el nombre de pila):

```json
{
  "NIF": "27485405Z",
  "Nombre": "Juan",
  "Apellidos": "Pérez García",
  "eMail": "juan@ejemplo.com",
  "Telefono": "915551234"
}
```

Parámetros de la URL: `nif` (NIF de la sociedad emisora) y `apiKey`. Campos del body: ver [apartado 6](#6-estructura-del-json-de-un-cliente).

**Respuesta**

```json
{
  "estado": "Ok",
  "mensaje": null,
  "consola": "Cliente '(B73954455) Cliente Ejemplo SL' disponible para facturar",
  "total": 0,
  "datos": {
    "id": 4521,
    "nif": "B73954455",
    "nombre": "(B73954455) Cliente Ejemplo SL"
  },
  "modoDeAcceso": "Consultor",
  "logout": false
}
```

| Campo de `datos` | Descripción |
|---|---|
| `id` | Id interno del cliente en Se10. |
| `nif` | NIF enviado. |
| `nombre` | Nombre con el que ha quedado el cliente en Se10. |

### 4.2 Crear una factura (modo directo) — `epCrearFactura`

Crea y emite la factura en una sola llamada. Es el modo recomendado.

**Petición**

```
POST https://biwe.femdek.com/Facturador/epCrearFactura?nif=00811725D&apiKey=XXXXXXXX
Content-Type: application/json

{
  "NifDelCliente": "B73954455",
  "Nombre": "Prueba del facturador",
  "Descripcion": "Factura por servicios de consultoría y licencia.",
  "Contacto": "Juan",
  "Telefono": "915551234",
  "eMail": "juan@ejemplo.com",
  "Lineas": [
    {
      "Orden": 1,
      "TipoDeLinea": "Alzada",
      "Concepto": "Licencia anual de software de gestión (QLIK)",
      "Cantidad": 1.00,
      "Precio": 1250.00,
      "Anotacion": "Licencia del 01/01 al 31/12",
      "Descuento": 0.00,
      "Iva": "21",
      "Unidad": "Unidad",
      "Naturaleza": "Servicios"
    },
    {
      "Orden": 2,
      "TipoDeLinea": "Alzada",
      "Concepto": "Servicio de consultoría e implementación",
      "Cantidad": 20.00,
      "Precio": 85.00,
      "Anotacion": "20 horas a 85€/hora",
      "Descuento": 0.00,
      "Iva": "21",
      "Unidad": "Hora",
      "Naturaleza": "Servicios"
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
      "Unidad": null,
      "Naturaleza": null
    }
  ]
}
```

Parámetros de la URL: `nif` (NIF de la sociedad emisora) y `apiKey`. Campos del body: ver [apartado 5](#5-estructura-del-json-de-una-factura).

**Respuesta**

```json
{
  "estado": "Ok",
  "mensaje": null,
  "consola": "Factura emitida y sometido su envío",
  "total": 0,
  "datos": {
    "solicitadaEl": "2026-11-20T10:15:00",
    "peticion": "CrearFactura",
    "facturador": "Nombre del facturador",
    "idFactura": 456,
    "numeroFactura": "2026-S-123",
    "mensaje": "Factura emitida y sometido su envío",
    "guidDeConsultaPdf": "6f9619ff-8b86-d011-b42d-00c04fc964ff",
    "guidDeConsultaXml": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
    "urlDeLaFactura": "https://biwe.femdek.com/FacturaEmt/Consultar?id=456"
  },
  "modoDeAcceso": "Consultor",
  "logout": false
}
```

| Campo de `datos` | Descripción |
|---|---|
| `solicitadaEl` | Fecha y hora de la petición. |
| `peticion` | Tipo de operación (`CrearFactura`, `RectificarPorDe`). |
| `facturador` | Nombre del facturador usado. |
| `idFactura` | Id interno de la factura en Se10. |
| `numeroFactura` | **Número de la factura** (`Año-Serie-Número`). Guardarlo. |
| `mensaje` | Resultado de la emisión (ver tabla siguiente). |
| `guidDeConsultaPdf` / `guidDeConsultaXml` | **Guardarlos.** Permanentes; necesarios para [4.4](#44-solicitar-el-pdf-o-el-xml--epsolicitarpdf--epsolicitarxml). |
| `urlDeLaFactura` | Enlace a la factura dentro de Se10; solo útil para usuarios de la aplicación. |

`estado` es `"Ok"` cuando `datos.mensaje` es uno de estos textos:

| `datos.mensaje` | Significado |
|---|---|
| `Factura emitida y sometido su envío` | Emitida y enviada a la AEAT (Verifactu). |
| `Se ha sometido un lote de envío de factura, acceda a la aplicación para cercionar el envío` | Emitida; el envío a la AEAT va en un lote. |
| `La factura no se ha enviado a la AEAT ya que la sociedad emisora no usa verifatu o el sistema no tiene activado el envío` | Emitida y generado su PDF, sin envío a la AEAT. |

Cualquier otro texto implica `estado = "Error"`; `datos.mensaje` y `consola` explican la causa.

### 4.3 Crear una factura (modo en dos pasos) — `epSolicitarFacturador` + `epCrearFacturaConGuid`

Para cuando el sistema externo quiere reservar la operación antes de enviar el JSON. El resultado final es el mismo que en 4.2.

**Paso 1 — Petición: reservar un guid operacional**

```
GET https://biwe.femdek.com/Facturador/epSolicitarFacturador?nifEmisor=00811725D&apiKey=XXXXXXXX&peticion=CrearFactura
```

| Parámetro | Descripción |
|---|---|
| `nifEmisor` | NIF de la sociedad emisora. |
| `apiKey` | Clave del facturador. |
| `peticion` | Siempre `CrearFactura`. |

**Paso 1 — Respuesta**

```json
{
  "estado": "Ok",
  "mensaje": null,
  "consola": "Solicitud de operación registrada correctamente",
  "total": 0,
  "datos": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "modoDeAcceso": "Consultor",
  "logout": false
}
```

`datos` es el guid operacional. Este guid:

- **Caduca al minuto** de generarse.
- Solo sirve para crear una factura.
- Solo se puede usar **una vez**.

**Paso 2 — Petición: crear la factura con el guid (sin apiKey)**

```
POST https://biwe.femdek.com/Facturador/epCrearFacturaConGuid?nif=00811725D&guid=3fa85f64-5717-4562-b3fc-2c963f66afa6
Content-Type: application/json

{ ...mismo JSON de factura que en 4.2... }
```

| Parámetro | Descripción |
|---|---|
| `nif` | NIF de la sociedad emisora; debe ser el mismo del paso 1. |
| `guid` | El guid devuelto en el paso 1. |

**Paso 2 — Respuesta**: idéntica a la de [4.2](#42-crear-una-factura-modo-directo--epcrearfactura).

### 4.4 Solicitar el PDF o el XML — `epSolicitarPdf` / `epSolicitarXml`

No devuelven el fichero, sino una **url de descarga válida durante 1 hora**.

**Petición (PDF)**

```
GET https://biwe.femdek.com/Facturador/epSolicitarPdf?nif=00811725D&apiKey=XXXXXXXX&numeroFactura=2026-S-123&guid=6f9619ff-8b86-d011-b42d-00c04fc964ff
```

**Petición (XML)**

```
GET https://biwe.femdek.com/Facturador/epSolicitarXml?nif=00811725D&apiKey=XXXXXXXX&numeroFactura=2026-S-123&guid=7c9e6679-7425-40de-944b-e07fc1f90ae7
```

| Parámetro | Descripción |
|---|---|
| `nif` | NIF de la sociedad emisora. |
| `apiKey` | Clave del facturador. |
| `numeroFactura` | Número recibido en `datos.numeroFactura` al crear la factura (URL-encoded). |
| `guid` | `guidDeConsultaPdf` (para el PDF) o `guidDeConsultaXml` (para el XML). |

**Respuesta (igual para PDF y XML)**

```json
{
  "estado": "Ok",
  "mensaje": null,
  "consola": "Url de descarga generada correctamente",
  "total": 0,
  "datos": "https://biwe.femdek.com/Archivos/epDescargaConGuid?guid=9b2e1a34-1c2d-4e5f-8a9b-0c1d2e3f4a5b&id=789",
  "modoDeAcceso": "Consultor",
  "logout": false
}
```

`datos` es la url de descarga ([4.5](#45-descargar-el-fichero--epdescargaconguid)).

Notas:

- `guidDeConsultaPdf` / `guidDeConsultaXml` **no caducan ni se gastan**: se pueden usar tantas veces como haga falta.
- Una factura de clase *Impresa* no tiene XML: pedirlo devuelve error indicando que se solicite el PDF.
- Si el documento todavía no existe (p. ej. el XML antes del envío a la AEAT), la respuesta es `estado = "Error"` con el motivo.

### 4.5 Descargar el fichero — `epDescargaConGuid`

**Petición**: un `GET` a la url recibida en `datos`, tal cual, sin `apiKey`.

```
GET https://biwe.femdek.com/Archivos/epDescargaConGuid?guid=9b2e1a34-1c2d-4e5f-8a9b-0c1d2e3f4a5b&id=789
```

**Respuesta**: no es JSON, es el propio fichero (PDF o XML). El nombre del fichero viene en la cabecera `Content-Disposition`.

La url es válida durante **1 hora**. Pasado ese tiempo hay que volver a llamar a `epSolicitarPdf` / `epSolicitarXml` para obtener otra.

### 4.6 Rectificar una factura por datos erróneos — `epRectificarPorDe`

Anula totalmente una factura ya emitida: se crea y emite automáticamente una **rectificativa** con las mismas líneas en negativo, asociada a la original (rectificación *Total*, motivo *Datos erróneos*).

**Petición**

```
POST https://biwe.femdek.com/Facturador/epRectificarPorDe?apiKey=XXXXXXXX&numeroFactura=2026-S-123
Content-Type: text/plain

Se ha detectado un error en el NIF del cliente
```

| Parámetro | Descripción |
|---|---|
| `apiKey` | Clave del facturador de la sociedad emisora. **No lleva `nif`**: se obtiene de la propia factura. |
| `numeroFactura` | Número de la factura **original** a rectificar (URL-encoded). |
| Body | Texto libre con el motivo de la rectificación. |

**Respuesta**

```json
{
  "estado": "Ok",
  "mensaje": null,
  "consola": "Factura emitida y sometido su envío",
  "total": 0,
  "datos": {
    "solicitadaEl": "2026-11-20T10:15:00",
    "peticion": "RectificarPorDe",
    "facturador": "Nombre del facturador",
    "idFactura": 789,
    "numeroFactura": "2026-R-7",
    "mensaje": "Factura emitida y sometido su envío",
    "guidDeConsultaPdf": "1a2b3c4d-5e6f-7890-abcd-ef1234567890",
    "guidDeConsultaXml": "0f1e2d3c-4b5a-6978-fedc-ba0987654321",
    "urlDeLaFactura": "https://biwe.femdek.com/FacturaEmt/Consultar?id=789"
  },
  "modoDeAcceso": "Consultor",
  "logout": false
}
```

Los datos devueltos son los de la **rectificativa recién creada**, no los de la original. Sus guids se usan igual que los de cualquier factura para pedir su PDF/XML. Las reglas de `estado` / `datos.mensaje` son las mismas que en [4.2](#42-crear-una-factura-modo-directo--epcrearfactura).

Notas:

- Si el número no existe, o hay varias facturas con ese número en distintas sociedades, devuelve error.
- Si la factura ya estaba rectificada, devuelve error indicando qué rectificativa la sustituyó.

## 5. Estructura del JSON de una factura

Body de `epCrearFactura` y `epCrearFacturaConGuid`.

| Campo | Tipo | Obligatorio | Descripción |
|---|---|---|---|
| `NifDelCliente` | texto | Sí | NIF/CIF/NIE del cliente facturado. Debe existir en Se10 (ver [4.1](#41-crear-un-cliente--epcrearcliente)). |
| `Nombre` | texto | Sí | Nombre/título de la factura. |
| `Descripcion` | texto | No | Descripción de la factura. |
| `Contacto` | texto | No | Persona de contacto. |
| `Telefono` | texto | No | Teléfono de contacto. |
| `eMail` | texto | No | Email de contacto. |
| `IrpfEmt` | objeto | No | Retención de IRPF: `{ "BiSujeta": 1250.00, "Irpf": "15" }` (`Irpf` es una clave del mapeo `Irpfs`). |
| `Lineas` | lista | Sí | Líneas de la factura (ver tabla siguiente). |

Campos de cada línea:

| Campo | Tipo | Descripción |
|---|---|---|
| `Orden` | entero | Orden de la línea en la factura. |
| `TipoDeLinea` | texto | `Alzada` (partida alzada), `Unitario` (elemento del catálogo) o `Comentario` (solo texto). |
| `Concepto` | texto | Texto de la línea. |
| `Cantidad` | decimal | Cantidad. `null` en comentarios. |
| `Precio` | decimal | Precio unitario **sin IVA**. `null` en comentarios. |
| `Anotacion` | texto | Texto adicional de la línea. |
| `Descuento` | decimal | Descuento de la línea. |
| `Iva` | texto | Clave del mapeo `Ivas` del facturador (p. ej. `"21"`). `null` en comentarios. |
| `Unidad` | texto | Clave del mapeo `Unidades` (p. ej. `"Unidad"`, `"Hora"`). `null` en comentarios. |
| `Naturaleza` | texto | Clave del mapeo `Naturalezas` (p. ej. `"Servicios"`). `null` en comentarios. |

> Los valores de `Iva`, `Unidad`, `Naturaleza` e `Irpf` **no son libres**: deben ser claves del mapeo configurado en el facturador de la sociedad ([apartado 9](#9-configuración-del-facturador-en-se10-apikey-y-mapeos)). El administrador de Se10 debe facilitar la lista de claves válidas.

## 6. Estructura del JSON de un cliente

Body de `epCrearCliente`.

| Campo | Obligatorio | Descripción |
|---|---|---|
| `NIF` | Sí | NIF, NIE o CIF del cliente. Es la clave de búsqueda: si ya existe, se actualiza en vez de crearse. |
| `TipoDeCliente` | No | `F` (física) o `J` (jurídica); también `Fisica`/`Física`/`Juridica`/`Jurídica`. Si se omite se deduce del NIF (NIF/NIE → física, CIF → jurídica). |
| `Nombre` | Sí | Persona física: nombre de pila. Persona jurídica: razón social (si es un autónomo dado de alta como `J`, nombre y apellidos juntos). |
| `Apellidos` | Solo persona física | Obligatorio en persona física; en persona jurídica **no** debe enviarse (da error). |
| `eMail` | Sí | Email de contacto. |
| `Telefono` | Sí | Teléfono de contacto. |
| `ValidarEnLaAeat` | No (`false`) | Si `true`, valida NIF y nombre contra la AEAT antes de guardar; si no coinciden, error. |
| `SustituirDatosIdentificativos` | No (`false`) | Si el cliente ya existía y `Nombre`/`Apellidos` difieren, con `true` se actualizan; con `false` se ignoran. |
| `SustituirDatosDeContacto` | No (`false`) | Igual para `eMail`/`Telefono`. |
| `Municipio` | Con dirección | Nombre del municipio; debe existir en Se10. |
| `CodigoPostal` | Con dirección | Debe existir en Se10. |
| `TipoDeVia` | Con dirección | Nombre (`Calle`, `Avenida`...), sigla (`Cl`, `Av`...) o clave del mapeo `TiposDeVia`. |
| `Calle` | Con dirección | Nombre de la calle. |
| `Numero` | Con dirección | Número de policía. |
| `CrearCalleSiNoExiste` | No (`false`) | Si la calle no existe en Se10: con `true` se crea; con `false`, error. |
| `ValidarEnCatastro` | No (`false`) | Si `true`, exige que la calle exista en el callejero del Catastro (solo España). |

Dirección fiscal:

- Los campos `Municipio`, `CodigoPostal`, `TipoDeVia`, `Calle` y `Numero` son opcionales **en bloque**: si se envía `Municipio`, son obligatorios también `CodigoPostal`, `TipoDeVia` y `Calle`.
- Si no se envían, no se toca la dirección del cliente.
- Si se envían y el cliente ya tenía otra dirección fiscal distinta, se da de baja la anterior y se crea la nueva. Si es la misma, no se hace nada.

## 7. Errores habituales

Todos llegan con `estado = "Error"` y el texto en `mensaje`.

| Mensaje (resumido) | Causa |
|---|---|
| `La sociedad '…' no está dada de alta o no está activa en la BD` | `nif` de la sociedad emisora incorrecto. |
| `La sociedad '…' no tiene ningún facturador para el ApiKey '…' proporcionado` | `apiKey` incorrecto o facturador desactivado. |
| `El nombre de la propiedad '…' no está contemplada, quítela del json proporcionado` | El JSON de la factura lleva una propiedad desconocida (revisar mayúsculas). |
| `El cliente indicado '…' no se ha localizado en la BD` | `NifDelCliente` no existe en Se10: darlo de alta antes con `epCrearCliente`. |
| `La clave del Iva / de la unidad / de la naturaleza / del irpf '…' no está contenida en el mapeador del facturador '…'` | Se ha enviado un valor que no está en el mapeo del facturador. |
| `El guid operacional ha caducado por haber pasado más de 1 minuto…` | En el modo en dos pasos, el paso 2 llegó tarde: pedir otro guid. |
| `El guid operacional ya ha sido usado…` | Se reutilizó un guid operacional. |
| `La sociedad '…' no corresponde con la del facturador` | El `nif` del paso 2 no coincide con el del paso 1. |
| `No se ha encontrado ninguna factura con el número '…' para el facturador indicado` | `numeroFactura` incorrecto o de otra sociedad. |
| `El guid proporcionado no corresponde con la factura número '…'` | Guid de consulta incorrecto (o se usó el del PDF para el XML, o viceversa). |
| `El número de factura '…' no tiene el formato esperado 'Año-Serie-Número'` | `numeroFactura` mal formado o sin codificar. |
| `No se tiene un xml / pdf de la factura '…'` | El documento aún no se ha generado. |

## 8. Herramienta de pruebas: TestFacturador

El proyecto `Pruebas/Facturador/TestFacturador` es una consola que permite probar la API sin escribir JSON a mano. Al arrancar pide la url base, el NIF de la sociedad emisora y el `apiKey` (se recuerdan para la siguiente vez) y muestra:

```
1. Crear cliente
2. Crear factura
3. Definición de las llamadas
0. Salir
```

- **1. Crear cliente**: pregunta uno a uno los campos de `epCrearCliente`, enseña la petición y pide confirmación antes de enviarla.
- **2. Crear factura**: pide el NIF del cliente, envía una factura de ejemplo a `epCrearFactura` y, si se crea, abre un submenú para descargar el XML/PDF, abrir la factura en Se10, rectificarla por datos erróneos o crear un cliente.
- **3. Definición de las llamadas**: **no llama al servidor**; muestra cómo debe montar un cliente externo cada petición (método, url completa con la url base y el NIF configurados, cabeceras y body) y el JSON que devuelve, es decir, el contenido del [apartado 4](#4-definición-de-las-llamadas) listo para copiar. El `apiKey` siempre se muestra como `XXXXXXXX`, de modo que la salida se puede enviar a un tercero sin exponer la clave. Submenú:

  ```
  1. Convenciones comunes (formato de respuesta y errores)
  2. Crear cliente            POST epCrearCliente
  3. Crear factura (directo)  POST epCrearFactura
  4. Crear factura (2 pasos)  GET epSolicitarFacturador + POST epCrearFacturaConGuid
  5. Solicitar PDF            GET epSolicitarPdf
  6. Solicitar XML            GET epSolicitarXml
  7. Descargar el fichero     GET epDescargaConGuid
  8. Rectificar por datos erróneos  POST epRectificarPorDe
  T. Todas
  0. Volver al menú principal
  ```

También hay ficheros `.http` de ejemplo en `SistemaDeElementos/PeticionesTest/`.

## 9. Configuración del facturador en Se10 (apiKey y mapeos)

*Apartado para el administrador de Se10.* Cada sociedad que factura vía API tiene uno o varios **facturadores** (en la ficha de la sociedad), uno por combinación de centro gestor y tipo de factura. Al dar de alta un facturador:

- Se genera su `apiKey` a partir de `IdSociedad + IdCg + IdTipoDeFactura`; es el que se entrega al sistema externo. Si se desactiva el facturador, sus peticiones fallan.
- Se indica el **JSON de mapeo**, que traduce los códigos que envía el sistema externo a valores de Se10. Se valida al guardar; una clave no reconocida da error.

JSON de mapeo (valor por defecto):

```json
{
  "ClaseDeEmision": "eFactura322",
  "Unidades": [
    { "clave": "Unidad", "valor": 1 },
    { "clave": "Hora", "valor": 2 }
  ],
  "Naturalezas": [
    { "clave": "Materiales", "valor": 1 },
    { "clave": "Servicios", "valor": 2 }
  ],
  "Ivas": [
    { "clave": "21", "valor": 1 },
    { "clave": "10", "valor": 2 }
  ],
  "Irpfs": [
    { "clave": "15", "valor": 1 },
    { "clave": "7", "valor": 2 }
  ],
  "TiposDeVia": [
    { "clave": "CL", "valor": "Cl" },
    { "clave": "Avd", "valor": "Av" }
  ]
}
```

| Clave | Obligatoria | `clave` (lo que envía el sistema externo) | `valor` (en Se10) |
|---|---|---|---|
| `ClaseDeEmision` | Sí | — (valor único) | Enumerado `enumClaseDeEmision` (p. ej. `eFactura322`). |
| `Unidades` | Sí, para crear facturas | `Unidad` de la línea | Id de la unidad. |
| `Naturalezas` | Sí, para crear facturas | `Naturaleza` de la línea | Id de la naturaleza. |
| `Ivas` | Sí, para crear facturas | `Iva` de la línea | Id del IVA repercutido. |
| `Irpfs` | No | `Irpf` de `IrpfEmt` | Id del IRPF. |
| `TiposDeVia` | No | `TipoDeVia` de `epCrearCliente` | **Sigla** del tipo de vía (no el id). |

Sobre `TiposDeVia`:

- Al validar se comprueba que cada `valor` es una sigla existente del maestro de tipos de vía, y que no hay claves vacías ni repetidas.
- Es opcional; sin ella el tipo de vía se busca solo por nombre y por sigla.
- Orden de resolución del `TipoDeVia` en `epCrearCliente` (gana el primero que encuentre): 1) por nombre, 2) por el mapeo `TiposDeVia` (sin distinguir mayúsculas), 3) por sigla. Si no se localiza, error.

## 10. Anexo — Funcionamiento interno

*Información de referencia sobre el backend (`FacturadorController` / `Facturador`); no es necesaria para integrarse.*

1. **Autenticación**: el `apiKey` se valida contra el generado a partir de `IdSociedad + IdCg + IdTipoDeFactura`.
2. **Registro de la petición**: se crea una fila en `PeticionDeFacturaEmtDtm` con guid propio, fecha y tipo de operación (`enumOperacionFacturador`: `CrearFactura`, `AnularFactura`, `SolicitarPdf`, `SolicitarXml`, `RectificarPorDe`, `CrearCliente`), junto con `GuidDeConsultaPdf` y `GuidDeConsultaXml`. Se registra fuera de la transacción de creación para que, si algo falla, quede anotado el error.
3. **Creación de la prefactura** a partir del JSON.
4. **Transición a "Emitida"** (`FAE_Etapa_Emitida`).
5. **Envío a la AEAT (Verifactu)** si la sociedad lo usa y está activo; si no, se genera solo el PDF.
6. **Resultado**: `PeticionDeFacturaEmtDto` con `Mensaje` y `NumeroFactura`; el `estado` se decide según el texto de `Mensaje`.
7. **Errores**: si falla la emisión pero la prefactura ya existe, se confirma igualmente para conservar la trazabilidad.
8. **Usuario**: cada endpoint se ejecuta internamente como `Administrador`, con transacción y traza propias.
