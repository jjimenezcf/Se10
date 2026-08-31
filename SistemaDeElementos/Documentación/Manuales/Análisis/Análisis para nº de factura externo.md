# Análisis para nº de factura externo

Alcance: se contempla para **todos** los tipos de factura emitida, tanto las que usan Veri*factu (eFactura 3.2 / 3.2.2) como las de clase `Impresa`. Esto incluye, por tanto, el punto más delicado del sistema: la cadena de hash de Veri*factu y el envío a la AEAT.

## Objetivo

Permitir que, al crear una factura (vía backoffice o vía el Facturador externo), se pueda indicar opcionalmente un **número de factura externo** (`numExternoDeFactura`), de forma que el sistema no calcule su propio número correlativo (`Ano-Serie-Numero`) sino que use el valor indicado por quien la crea.

Caso de uso típico: migración de facturación desde otro sistema (que ya lleva su propia numeración correlativa y legal), o integraciones externas donde el número lo decide el sistema origen.

## Problemática

### Cómo se genera el número hoy

El número se calcula **solo al emitir** la factura (transición Prefactura → Emitida), en `AntesDeEmitirFactura` (`Servicios/GestorDeElementos/Extensores/Venta/ExtensorDeFacturasEmt.cs:1165-1223`):

```csharp
factura.Serie  = factura.EsRectificativa ? ltrDeFacturaRectificada.Serie : tipoFactura.Serie;
factura.Ano    = factura.FacturadaEl?.Year ?? DateTime.Now.Year;
var numero     = factura.Sociedad(contexto).UltimoNumeroDeLaSerie(contexto, factura.Ano, factura.Serie);
factura.Numero = numero == default ? 1 : numero + 1;
```

`UltimoNumeroDeLaSerie` (`ExtensorDeFacturasEmt.cs:55-60`) es un `MAX(Numero)` por **sociedad + serie + año** (no una secuencia de BD dedicada). `Serie` sale de `TipoDeFacturaEmtDtm.Serie` (`ServicioDeDatos/Ventas/Factura/TipoDeFacturaEmtDtm.cs:36`), fija por tipo de factura; no existe hoy ningún flag de configuración para "numeración externa".

Este es el punto de intercepción natural: si se recibe un número externo, esta rama debe saltarse el cálculo y usar el valor recibido (validado).

### Todo lo que asume que el número es un entero correlativo autogenerado

| Área | Dónde | Qué asume |
|---|---|---|
| Unicidad en BD | `FacturaEmtDtm.cs:454` — índice único `(IdCg, Ano, Serie, Numero)` | `Numero` es `int`. Único **por Centro Gestor**, mientras que el contador se calcula **por Sociedad** — inconsistencia ya existente hoy, que se agrava con números externos (dos CG de la misma sociedad podrían repetir número sin que la BD lo impida). |
| Deshacer última factura | `ExtensorDeFacturasEmt.cs:1482-1486`, `AntesDeDevolverAPrefactura` | Exige que `Numero` coincida con el último de la serie (`MAX`), para permitir "deshacer". Con numeración externa no garantizada correlativa, esta validación deja de tener sentido. |
| **Cadena de hash Veri*factu** | `ExtensorDeFacturasEmt.cs:115-143`, `CalcularHuella` | `NumeroDeFactura` entra como string en el hash — esto en sí no rompe nada. |
| **Localizar "factura anterior" para encadenar** | `ExtensorDeFacturasEmt.cs:73-100`, método `Anterior()` | Filtra `Numero < factura.Numero` dentro de Serie/Año y ordena por fecha. **Depende críticamente de que `Numero` sea un entero estrictamente creciente en el mismo orden que la emisión real.** Si no lo es, se puede encadenar la huella con la factura equivocada. |
| Reconstrucción de lote de envío a la AEAT | `GestoresDeNegocio/Venta/Factura/GestorDeFacturasEmt.cs:1039-1068` | Se apoya en el mismo `Anterior()` para incluir en el lote todas las facturas previas no enviadas. Mismo riesgo que el punto anterior. |
| Consulta de facturas emitidas a la AEAT | `ServicioSepa/Verifactu.cs:92-100` | Hace `InvoiceID.Split('-')` asumiendo el formato exacto `Año-Serie-Número` sin guiones dentro de la Serie. Un número o serie externos con `-` rompen este parseo. |
| XML Facturae | `GestoresDeNegocio/Venta/Factura/GeneradorDeFacturaEmtXml322.cs:179-180` | Campos `string` libres en el código; el XSD real sí impone longitudes máximas no validadas aquí. |
| Búsqueda libre por número | `GestoresDeNegocio/Venta/Factura/FiltrosDeFacturasEmt.cs:442-477` | Hace `Split('-')` exigiendo 3 partes numéricas (`año-serie-número`). |
| Filtro de rango en el grid | `SistemaDeElementos/Descriptores/DescriptoresDeVentas/DescriptorDeFacturasEmt.cs:171-180` | `PlaceHolder = "yyyy-s-numero"` y una `ExpresioRegular` que asume el mismo formato fijo (y que ya está rota/incompleta hoy). |
| Facturador externo (API) | `GestoresDeNegocio/Venta/Factura/Facturador.cs:99-102` | Busca la petición por `NumeroDeFactura == numeroFactura` recibido del cliente. Seguiría funcionando igual, siempre que el número (externo o no) sea único por facturador. |

### Lo que NO se ve afectado

- **Impresión/PDF** (`ServicioDeReportes/Ventas/Facturas emitidas/ReporteDeFacturaEmt.cs:46-57`): imprime `NumeroFactura` como texto libre, sin máscara.
- **Nombres de fichero** (PDF/XML): usan `factura.Referencia` (`Servicios/GestorDeElementos/GestorDeElementos.cs:748`), una secuencia interna totalmente distinta al número fiscal.
- **Prompt de IA**: solo se usa `NumeroFactura` para *extraer* el número de facturas **recibidas** (OCR/eFactura entrante) en `Ayudas/Extensiones/IIa.cs`, flujo inverso al que aquí se plantea.

## Riesgos

1. **Riesgo legal (el más importante)**: el Reglamento Veri*Factu (RD 1007/2023) exige numeración **correlativa** por serie. Aceptar un número externo arbitrario para facturas que se comunican a la AEAT puede incumplir la normativa si no se garantiza que ese número sea, en origen, correlativo y sin huecos. Esto no es solo un problema de código: hay que decidir, a nivel funcional, bajo qué condiciones se acepta (p. ej. solo en migraciones controladas, con el usuario asumiendo la responsabilidad de la correlatividad del origen).

2. **Ruptura de la cadena de hash**: si `Anterior()` encadena con la factura equivocada porque `Numero` no refleja el orden real de emisión, la huella generada no sería la que exige el AEAT — un fallo silencioso y grave, difícil de detectar a posteriori.

3. **Colisión de números**: el desajuste entre el índice único (por Cg) y el contador (por sociedad) ya existe hoy; con números externos aumenta la probabilidad de choque si dos orígenes externos (o un externo y el cálculo interno) usan el mismo valor.

4. **Rotura de parseos rígidos**: tanto la consulta a la AEAT (`Verifactu.cs`) como los buscadores (`FiltrosDeFacturasEmt`, `DescriptorDeFacturasEmt`) asumen `Año-Serie-Número` sin guiones internos y `Numero` numérico. Un número externo con otro formato puede romper resultados de búsqueda o el parseo de respuestas de la AEAT.

5. **Concurrencia**: no hay lock dedicado a nivel de sociedad+serie+año, solo un semáforo genérico de negocio. Dos peticiones simultáneas con número externo podrían intentar usar el mismo valor si no se valida con cuidado (transacción + comprobación de unicidad antes de persistir).

6. **"Deshacer" factura roto**: la validación de `AntesDeDevolverAPrefactura` deja de tener sentido si el número no es MAX+1; sin ajustarla, se podría permitir (o bloquear incorrectamente) deshacer una factura con número externo.

7. **Tipo de dato**: `Numero` es `int` en BD. Si el número externo de origen no es un entero limpio, hay que decidir si se transforma `Numero`/`Serie` a texto libre (impacto amplio: índice único, comparaciones `<` en `Anterior()`, etc.) o si se guarda el valor externo en un campo aparte y se sigue manteniendo un `Numero` interno solo para uso técnico (encadenado, orden). Esta segunda opción es la que se recomienda más abajo.

## Pasos de implementación

Dado el riesgo del punto 2 (cadena de hash), se recomienda una implementación por fases, empezando por el escenario de menor riesgo y añadiendo Veri*factu solo cuando el resto esté validado.

### Fase 0 — Decisiones de diseño (antes de tocar código)

- ¿`Numero` externo será siempre numérico, o hay que admitir alfanumérico? Determina si se reutiliza la columna `Numero` o se añade un campo nuevo.
- **Recomendación**: separar el "número que se muestra/comunica" (puede ser externo) del "orden interno" que usa `Anterior()` para encadenar el hash. Por ejemplo, mantener un contador interno propio (o usar `Id`/`EmitidaEl`) para determinar predecesor/sucesor en la cadena, independiente de si `Numero`/`Serie` visibles son externos. Esto blinda el punto más grave (riesgo 2) sin renunciar a aceptar cualquier formato de número externo.
- ¿Quién puede marcar una factura con número externo? (permiso, tipo de factura habilitado, o parámetro libre en cualquier creación).
- ¿Se permite mezclar, dentro de la misma serie, facturas con número interno y externo? Si no, hay que bloquearlo explícitamente.

### Fase 1 — Modelo de datos

- `TipoDeFacturaEmtDtm`: nuevo flag de configuración (p. ej. `PermiteNumeracionExterna`).
- `FacturaEmtDtm` / `FacturaEmtDto`: campo(s) para recibir el número externo; si se seguía la recomendación de la Fase 0, añadir también el campo de "orden interno de encadenado" si no se reutiliza uno existente (`Id` o `EmitidaEl` ya podrían servir sin cambios de esquema).
- Migración EF Core con los nuevos campos/flag.
- Revisar el índice único `(IdCg, Ano, Serie, Numero)` — valorar si conviene ampliarlo a nivel de Sociedad, dado el desajuste ya descrito.

### Fase 2 — Lógica de emisión

- En `AntesDeEmitirFactura` (`ExtensorDeFacturasEmt.cs:1195-1211`): si hay número externo, usarlo en vez de `UltimoNumeroDeLaSerie(...)+1`, validando que no exista ya en `(IdCg/Sociedad, Ano, Serie, Numero)`.
- Validar formato del número externo según lo decidido en la Fase 0.

### Fase 3 — "Deshacer" factura

- Ajustar `AntesDeDevolverAPrefactura` (`ExtensorDeFacturasEmt.cs:1482-1486`) para números externos: decidir si se permite deshacer (y bajo qué condición) o se bloquea directamente para facturas con numeración externa.

### Fase 4 — Cadena de hash Veri*factu

- Adaptar `Anterior()` (`ExtensorDeFacturasEmt.cs:73-100`) para usar el criterio de orden interno decidido en la Fase 0, no `Numero` directamente si este puede ser externo y no correlativo.
- Revisar `CalcularHuella` (`ExtensorDeFacturasEmt.cs:115-143`) — no debería requerir cambios si `NumeroDeFactura` se sigue tratando como string opaco, pero conviene verificarlo con un caso de prueba real.

### Fase 5 — Envío y consulta AEAT

- Corregir el `Split('-')` de `Verifactu.cs:92-100` para que no asuma formato fijo (o documentar que los números externos no pueden contener `-`).
- Validar longitud/formato del número antes de enviarlo (requisitos del XSD Facturae en `GeneradorDeFacturaEmtXml322.cs:179-180`).

### Fase 6 — Búsquedas y filtros

- Adaptar `FiltrosDeFacturasEmt.cs:442-477` (buscador libre) para no depender de `Split('-')` con 3 partes numéricas.
- Revisar el `PlaceHolder`/`ExpresioRegular` del filtro de rango en `DescriptorDeFacturasEmt.cs:171-180` (aprovechar para arreglar la regex, que ya está rota).

### Fase 7 — Facturador externo

- Añadir el parámetro `numExternoDeFactura` (nombre a definir) en `epCrearFactura`/`epCrearFacturaConGuid` (`SistemaDeElementos/Controllers/Venta/FacturadorController.cs`) y propagarlo hasta `Facturador.CrearFactura`.

### Fase 8 — Validaciones de negocio transversales

- Unicidad (con transacción, dado que no hay secuencia de BD dedicada).
- Permisos: quién puede crear facturas con número externo.
- Bloqueo explícito si se intenta combinar número externo con un tipo de factura que no lo permite (Fase 0/1).

## Pruebas

1. **Creación normal (regresión)**: crear una factura sin número externo — debe comportarse exactamente igual que hoy (correlativo interno).
2. **Creación con número externo**: crear una factura indicando `numExternoDeFactura` — debe persistirse ese número y no el calculado.
3. **Duplicado**: intentar crear dos facturas con el mismo número externo en la misma sociedad/serie/año — debe rechazarse con un mensaje claro.
4. **Formato inválido**: número externo con caracteres no permitidos (según lo decidido en Fase 0) — debe rechazarse antes de persistir.
5. **Cadena de hash Veri*factu**: crear una secuencia de facturas mezclando números externos no estrictamente crecientes y verificar que `Anterior()` encadena con la factura realmente emitida justo antes (por orden interno), no por el valor del número externo.
6. **Envío a la AEAT**: someter una factura con número externo (incluyendo uno con guion, si el formato lo permite) y comprobar que `Verifactu.cs` no rompe al parsear la respuesta/consulta.
7. **Deshacer factura**: intentar devolver a prefactura una factura con número externo y comprobar que el sistema se comporta según lo decidido en la Fase 3 (permitido con condición, o bloqueado con mensaje claro).
8. **Búsquedas**: buscar por el número externo tanto en el buscador libre como en el filtro de rango del grid, y comprobar que la encuentra.
9. **Concurrencia**: lanzar dos peticiones simultáneas intentando crear facturas con el mismo número externo — solo una debe tener éxito.
10. **Facturador externo**: crear una factura vía API pasando `numExternoDeFactura` y comprobar que la respuesta (`NumeroFactura`, `GuidDeConsultaPdf/Xml`, `UrlDeLaFactura`) es coherente con el número indicado.
11. **Impresión y XML**: comprobar que el PDF y el XML Facturae muestran el número externo correctamente, incluyendo casos límite de longitud.
12. **Multi-CG**: dos centros gestores de la misma sociedad, cada uno con una factura con el mismo número externo — verificar el comportamiento decidido en la Fase 1 respecto al índice único.
