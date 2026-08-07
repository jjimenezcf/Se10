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

## Resumen del backend

1. **Autenticación**: `apiKey` se valida contra uno generado a partir de `IdSociedad + IdCg + IdTipoDeFactura`. Si no coincide → error.
2. **Registro de la petición**: se crea una fila en `PeticionDeFacturaEmtDtm` con GUID propio, timestamp de solicitud y el tipo de operación (`enumOperacionFacturador`: `CrearFactura`, `AnularFactura`, `SolicitarPdf`, `SolicitarXml` — solo `CrearFactura` está implementada por ahora).
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
    "Mensaje": "Factura emitida y sometido su envío"
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
    "Mensaje": "Factura emitida y sometido su envío"
  },
  "Estado": "Ok",
  "Consola": "Factura emitida y sometido su envío"
}
```

### Notas

- Si pasa más de 1 minuto entre el paso 1 y el paso 2, el GUID caduca y hay que solicitar uno nuevo.
- Si el GUID ya se usó anteriormente, el paso 2 devuelve un error indicando la factura ya asociada a ese GUID.
