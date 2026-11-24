# Tipos de IVA repercutido

Maestro de los **tipos de IVA que se cobran** en las facturas emitidas: **clase**, descripción, **porcentaje** (*Iva(%)*), si es **exento**,
el **motivo de exención** fiscal y la **cuenta contable** del IVA repercutido para los preasientos. Lo usan las líneas de presupuestos,
partes de trabajo, documentos de venta y facturas emitidas, y la comunicación a la AEAT (Verifactu / SII).

## ¿Cómo hago...?

### ...para dar de alta un IVA exento?

**Crear** marcando **Iva exento**: el porcentaje queda bloqueado (a cero) y se habilita el **Motivo de exención**, que se declara a la
AEAT. Si no es exento, se indica el porcentaje y el motivo queda bloqueado.

### ...para buscar un tipo?

Filtro **Iva repercutido** (por código o descripción).

---

#### Detalle técnico

- Controlador: `IvasRepercutidoController` (Controllers/Contabilidad). Vista: `Views/Contabilidad/CrudIvasRepercutido.cshtml`. Dto:
  `IvaRepercutidoDto`.
- Descriptor: `DescriptorDeIvasRepercutido`. ts: `wwwroot/ts/Contabilidad/IvasRepercutido.ts`: al marcar *Iva exento* bloquea el
  porcentaje y habilita el motivo de exención (y al revés).
