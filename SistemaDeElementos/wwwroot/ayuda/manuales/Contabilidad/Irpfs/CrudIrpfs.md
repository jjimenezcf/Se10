# Tipos de IRPF

Maestro de los **tipos de retención de IRPF**: código, descripción, **porcentaje** (*Irpf(%)*) y la **cuenta contable** a la que va la
retención en los preasientos. Se eligen en las líneas de las facturas recibidas y emitidas que llevan retención.

## ¿Cómo hago...?

### ...para dar de alta un tipo de IRPF?

**Crear**: código, descripción, porcentaje y cuenta contable (sin cuenta no se pueden preasentar las facturas con esa retención).

### ...para buscar un tipo?

Filtro **Irpf** (por código o descripción).

---

#### Detalle técnico

- Controlador: `IrpfsController` (Controllers/Contabilidad). Vista: `Views/Contabilidad/CrudIrpfs.cshtml`. Dto: `IrpfDto`.
- Descriptor: `DescriptorDeIrpfs`. ts: `wwwroot/ts/Contabilidad/Irpfs.ts`.
