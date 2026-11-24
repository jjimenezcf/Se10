# Tipos de IVA soportado

Maestro de los **tipos de IVA que se pagan** en las facturas recibidas: código, descripción, **porcentaje** (*Iva(%)*), si es **exento** y
la **cuenta contable** del IVA soportado para los preasientos de gasto.

## ¿Cómo hago...?

### ...para dar de alta un tipo de IVA soportado?

**Crear**: código, descripción, porcentaje y cuenta. Si se marca **Iva exento**, el porcentaje queda bloqueado.

### ...para buscar un tipo?

Filtro **Iva soportado** (por código o descripción).

---

#### Detalle técnico

- Controlador: `IvasSoportadoController` (Controllers/Contabilidad). Vista: `Views/Contabilidad/CrudIvasSoportado.cshtml`. Dto:
  `IvaSoportadoDto`.
- Descriptor: `DescriptorDeIvasSoportado`. ts: `wwwroot/ts/Contabilidad/IvasSoportado.ts`: con *Iva exento* bloquea el porcentaje.
