# Manual de uso — Facturador externo

Hay **dos formas** de crear una factura vía API, ambas autenticadas con el `apiKey` del facturador dado de alta para la sociedad (no requieren login, son `[AllowAnonymous]`).

## Opción A — Modo directo (un solo paso)

`POST /Facturador/epCrearFactura?nif={nifEmisor}&apiKey={apiKey}`
Body: JSON de la factura.

En una sola llamada se valida el `apiKey`, se registra la petición y se crea/emite la factura, todo en la misma transacción. No hace falta GUID previo.

## Opción B — Modo en dos pasos (con GUID operacional)

**Paso 1:** `GET /Facturador/epSolicitarFacturador?nifEmisor={nif}&apiKey={apiKey}&peticion=CrearFactura` → devuelve un GUID en `Datos`.

**Paso 2:** `POST /Facturador/epCrearFacturaConGuid?nif={nif}&guid={guidObtenido}` con el JSON de la factura.

El GUID:
- Caduca al minuto de haberse generado.
- Solo es válido para crear una factura (no sirve para otras operaciones).
- Solo puede usarse una vez.

## ¿Cuándo usar cada una?

- **Directo**: integraciones sencillas, una sola llamada.
- **Dos pasos**: cuando el cliente quiere reservar/correlacionar la operación antes de mandar el JSON completo de la factura.

Ambas acaban llamando al mismo método interno `Facturador.CrearFactura(...)` en el backend.

En la respuesta de creación de la factura (ambos modos), además de `NumeroFactura` y `Mensaje`, se reciben tres campos pensados para consultas posteriores:

- `GuidDeConsultaPdf` / `GuidDeConsultaXml`: guids permanentes (no caducan) que hay que guardar para poder pedir después el PDF o el XML de esa factura.
- `UrlDeLaFactura`: URL informativa a la vista de la factura dentro de Se10, solo útil para quien tenga usuario y contraseña de la aplicación.

## Consulta de PDF / XML de una factura ya emitida

Una vez creada la factura, se puede pedir su PDF o su XML en cualquier momento usando el `apiKey`, el número de factura y el guid correspondiente (`GuidDeConsultaPdf` o `GuidDeConsultaXml`) recibido al crearla. Estos guids **no caducan y se pueden usar tantas veces como se quiera**.

`GET /Facturador/epSolicitarPdf?nif={nif}&apiKey={apiKey}&numeroFactura={numeroFactura}&guid={guidDeConsultaPdf}`
`GET /Facturador/epSolicitarXml?nif={nif}&apiKey={apiKey}&numeroFactura={numeroFactura}&guid={guidDeConsultaXml}`

Si los datos coinciden, el backend **no devuelve el fichero directamente**: devuelve una URL de descarga válida durante **1 hora**. Con esa URL (que ya incluye su propio guid de descarga) se obtiene el fichero, sin necesidad de `apiKey`. Pasada la hora, o si ya se agotó, hay que volver a llamar a `epSolicitarPdf`/`epSolicitarXml` para obtener una URL nueva.

Ver el detalle en el [Anexo 3](#anexo-3--consulta-de-pdf--xml-de-una-factura).

## Rectificar una factura por datos erróneos

Si una factura ya emitida tiene datos incorrectos, se puede anular mediante una **rectificativa total por datos erróneos**: se crea automáticamente una factura rectificativa (con las mismas líneas en negativo), se asocia a la original y se emite, sin intervención manual.

`POST /Facturador/epRectificarPorDe?apiKey={apiKey}&numeroFactura={numeroFactura}`
Body: texto libre con el motivo de la rectificación.

No hace falta indicar `nif`: el sistema localiza la factura original por su número, obtiene la sociedad a la que pertenece y valida el `apiKey` contra esa sociedad — igual que en el modo directo de creación.

Ver el detalle en el [Anexo 4](#anexo-4--rectificar-una-factura-por-datos-erróneos).

## Resumen del backend

1. **Autenticación**: `apiKey` se valida contra uno generado a partir de `IdSociedad + IdCg + IdTipoDeFactura`. Si no coincide → error.
2. **Registro de la petición**: se crea una fila en `PeticionDeFacturaEmtDtm` con GUID propio, timestamp de solicitud y el tipo de operación (`enumOperacionFacturador`: `CrearFactura`, `AnularFactura`, `SolicitarPdf`, `SolicitarXml`, `RectificarPorDe`). Al crearla también se generan `GuidDeConsultaPdf` y `GuidDeConsultaXml`, permanentes, para consultas futuras del documento.
3. **Creación de la prefactura**: se parsea el JSON recibido (cliente, líneas, etc.) y se crea la prefactura.
4. **Transición a "Emitida"**: la prefactura pasa a la etapa `FAE_Etapa_Emitida`.
5. **Envío a la AEAT (Verifactu)**: si la sociedad usa Verifactu y está activo, se envía la factura — el mensaje de resultado indica si se sometió el envío individual o en lote. Si no usa Verifactu, simplemente se genera el PDF.
6. **Resultado**: se devuelve un `PeticionDeFacturaEmtDto` con `Mensaje` (texto descriptivo) y `NumeroFactura`. El `Estado` (`Ok`/`Error`) se decide mirando si el `Mensaje` contiene alguno de los textos de éxito.
7. **Manejo de errores**: si falla la emisión pero la prefactura ya se creó, se guarda igualmente la referencia de la prefactura y se registra el error — así queda trazabilidad de qué pasó aunque la factura no se haya podido emitir completamente.
8. **Trazas y transacciones**: cada endpoint abre transacción y traza, y se ejecuta como usuario `Administrador` internamente (el cliente externo nunca necesita credenciales de la aplicación).

---

## Anexo 1 — Modo directo (un solo paso)

Este modo crea y emite la factura en una única llamada HTTP.

### Petición

```
POST https://biwe.femdek.com/Facturador/epCrearFactura?nif=00811725D&apiKey=XXXXXXXX
Content-Type: application/json

{
  "NifDelCliente": "27485405Z",
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
      "Naturaleza": "Servicios",
      "Clase": "Servicio"
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
      "Naturaleza": "Servicios",
      "Clase": "Servicio"
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
      "Naturaleza": null,
      "Clase": null
    }
  ]
}
```

### Parámetros de la URL

- `nif`: NIF de la sociedad emisora (la que factura).
- `apiKey`: clave de API asignada al facturador de esa sociedad.

### Respuesta (ejemplo)

```json
{
  "Datos": {
    "SolicitadaEl": "2026-08-07T10:15:00",
    "Peticion": "CrearFactura",
    "Facturador": "Nombre del facturador",
    "NumeroFactura": "F2026/00123",
    "Mensaje": "Factura emitida y sometido su envío",
    "GuidDeConsultaPdf": "6f9619ff-8b86-d011-b42d-00c04fc964ff",
    "GuidDeConsultaXml": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
    "UrlDeLaFactura": "https://biwe.femdek.com/FacturaEmt/Consultar?id=456"
  },
  "Estado": "Ok",
  "Consola": "Factura emitida y sometido su envío"
}
```

### Notas

- Si `Estado` es `Error`, el campo `Mensaje` / `Consola` describe la causa.
- Si la sociedad no usa Verifactu, la factura se emite igualmente y se genera el PDF, pero no se envía a la AEAT.

---

## Anexo 2 — Modo en dos pasos (con GUID operacional)

Este modo reserva primero la operación (obteniendo un GUID) y después envía los datos de la factura usando ese GUID.

El GUID obtenido en el paso 1:

- Caduca al minuto de haberse generado.
- Solo es válido para crear una factura (no sirve para otras operaciones).
- Solo puede usarse una vez.

### Paso 1 — Solicitar el GUID operacional

```
GET https://biwe.femdek.com/Facturador/epSolicitarFacturador?nifEmisor=00811725D&apiKey=XXXXXXXX&peticion=CrearFactura
```

Parámetros de la URL:

- `nifEmisor`: NIF de la sociedad emisora.
- `apiKey`: clave de API asignada al facturador de esa sociedad.
- `peticion`: tipo de operación a reservar. Para crear una factura, usar siempre el valor `CrearFactura`.

Respuesta (ejemplo):

```json
{
  "Datos": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "Estado": "Ok",
  "Consola": "Solicitud de operación registrada correctamente"
}
```

El valor de `Datos` es el GUID que se debe usar en el paso 2.

### Paso 2 — Crear la factura usando el GUID obtenido

```
POST https://biwe.femdek.com/Facturador/epCrearFacturaConGuid?nif=00811725D&guid=3fa85f64-5717-4562-b3fc-2c963f66afa6
Content-Type: application/json

{
  "NifDelCliente": "27485405Z",
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
      "Naturaleza": "Servicios",
      "Clase": "Servicio"
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
      "Naturaleza": "Servicios",
      "Clase": "Servicio"
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
      "Naturaleza": null,
      "Clase": null
    }
  ]
}
```

Parámetros de la URL:

- `nif`: NIF de la sociedad emisora (debe coincidir con el de la sociedad propietaria del facturador asociado al GUID).
- `guid`: el GUID devuelto en el paso 1.

Respuesta (ejemplo):

```json
{
  "Datos": {
    "SolicitadaEl": "2026-08-07T10:15:00",
    "Peticion": "CrearFactura",
    "Facturador": "Nombre del facturador",
    "NumeroFactura": "F2026/00123",
    "Mensaje": "Factura emitida y sometido su envío",
    "GuidDeConsultaPdf": "6f9619ff-8b86-d011-b42d-00c04fc964ff",
    "GuidDeConsultaXml": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
    "UrlDeLaFactura": "https://biwe.femdek.com/FacturaEmt/Consultar?id=456"
  },
  "Estado": "Ok",
  "Consola": "Factura emitida y sometido su envío"
}
```

### Notas

- Si pasa más de 1 minuto entre el paso 1 y el paso 2, el GUID caduca y hay que solicitar uno nuevo.
- Si el GUID ya se usó anteriormente, el paso 2 devuelve un error indicando la factura ya asociada a ese GUID.

---

## Anexo 3 — Consulta de PDF / XML de una factura

Una vez creada la factura (Anexo 1 o Anexo 2), se recibieron en la respuesta `GuidDeConsultaPdf` y `GuidDeConsultaXml`. Guárdalos: son la llave para pedir el documento más adelante, tantas veces como se necesite (no caducan).

### Pedir el PDF

```
GET https://biwe.femdek.com/Facturador/epSolicitarPdf?nif=00811725D&apiKey=XXXXXXXX&numeroFactura=F2026%2F00123&guid=6f9619ff-8b86-d011-b42d-00c04fc964ff
```

### Pedir el XML

```
GET https://biwe.femdek.com/Facturador/epSolicitarXml?nif=00811725D&apiKey=XXXXXXXX&numeroFactura=F2026%2F00123&guid=7c9e6679-7425-40de-944b-e07fc1f90ae7
```

Parámetros de la URL (ambos endpoints):

- `nif`: NIF de la sociedad emisora.
- `apiKey`: clave de API asignada al facturador de esa sociedad.
- `numeroFactura`: número de la factura tal como se recibió en `NumeroFactura` al crearla.
- `guid`: `GuidDeConsultaPdf` (para `epSolicitarPdf`) o `GuidDeConsultaXml` (para `epSolicitarXml`).

Respuesta (ejemplo):

```json
{
  "Datos": "https://biwe.femdek.com/Archivos/epDescargaConGuid?guid=9b2e1a34-...&id=789",
  "Estado": "Ok",
  "Consola": "Url de descarga generada correctamente"
}
```

### Descargar el fichero

El valor de `Datos` es una URL de descarga directa (no requiere `apiKey`, ya lleva su propio guid de un solo sistema de descarga genérico). Basta con hacer un `GET` a esa URL para obtener el fichero.

```
GET https://biwe.femdek.com/Archivos/epDescargaConGuid?guid=9b2e1a34-...&id=789
```

### Notas

- La URL de descarga es válida durante **1 hora** desde que se genera. Pasado ese tiempo, o si el enlace ya no es válido, hay que volver a llamar a `epSolicitarPdf`/`epSolicitarXml` para obtener uno nuevo.
- `GuidDeConsultaPdf`/`GuidDeConsultaXml` no caducan ni se consumen: se pueden reutilizar todas las veces que haga falta.
- Si el documento solicitado aún no existe (por ejemplo, el XML antes de que la factura se envíe a la AEAT), la respuesta viene con `Estado: "Error"` y el mensaje lo indica.

---

## Anexo 4 — Rectificar una factura por datos erróneos

Este modo anula totalmente una factura ya emitida, creando y emitiendo automáticamente su rectificativa (líneas iguales a la original, en negativo).

### Petición

```
POST https://biwe.femdek.com/Facturador/epRectificarPorDe?apiKey=XXXXXXXX&numeroFactura=F2026%2F00123
Content-Type: text/plain

Se ha detectado un error en el NIF del cliente
```

### Parámetros de la URL

- `apiKey`: clave de API asignada al facturador de la sociedad emisora (no hace falta indicar `nif`, se obtiene de la propia factura).
- `numeroFactura`: número de la factura original a rectificar, tal como se recibió en `NumeroFactura` al crearla.

### Body

Texto libre con el motivo de la rectificación. Se guarda como detalle del motivo (`enumMotivoDeRectificacion.DatosErroneos`, rectificación de clase "Total").

### Respuesta (ejemplo)

```json
{
  "Datos": {
    "SolicitadaEl": "2026-08-25T09:30:00",
    "Peticion": "RectificarPorDe",
    "Facturador": "Nombre del facturador",
    "NumeroFactura": "F2026/00456",
    "Mensaje": "Factura emitida y sometido su envío",
    "GuidDeConsultaPdf": "1a2b3c4d-5e6f-7890-abcd-ef1234567890",
    "GuidDeConsultaXml": "0f1e2d3c-4b5a-6978-fedc-ba0987654321",
    "UrlDeLaFactura": "https://biwe.femdek.com/FacturaEmt/Consultar?id=789"
  },
  "Estado": "Ok",
  "Consola": "Factura emitida y sometido su envío"
}
```

`NumeroFactura`, `GuidDeConsultaPdf`, `GuidDeConsultaXml` y `UrlDeLaFactura` corresponden a la **rectificativa recién creada**, no a la original — se pueden usar igual que con cualquier otra factura (Anexo 3) para descargar su PDF/XML.

### Notas

- Si el número de factura indicado no existe, o hay más de una factura con ese número (caso ambiguo entre sociedades), la petición falla con `Estado: "Error"`.
- Si la factura original ya estaba rectificada, la petición falla indicando qué rectificativa la sustituyó.
- Igual que al crear una factura: si la sociedad usa Verifactu, la rectificativa se envía a la AEAT; si no, simplemente se genera su PDF.
