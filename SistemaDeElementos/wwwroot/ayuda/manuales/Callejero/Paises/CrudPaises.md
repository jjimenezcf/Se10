# Gestión de países

Maestro de **países**: código, nombre, **Iso 2**, **prefijo telefónico**, nombre en inglés y si es **intracomunitario** (UE). Se usa en las
direcciones, en el NIF/VAT de clientes y proveedores extranjeros y en la fiscalidad de las facturas (operaciones intracomunitarias).

## ¿Cómo hago...?

### ...para buscar un país?

Filtro **Codigo** o por nombre.

### ...para dar de alta un país?

**Crear**: código, nombre, Iso 2, prefijo telefónico, nombre en inglés y marca de intracomunitario.

---

#### Detalle técnico

- Controlador: `PaisesController` (Controllers/Callejero). Vista: `Views/Callejero/CrudPaises.cshtml`. Dto: `PaisDto`.
- Descriptor: `DescriptorDePais`. ts: `wwwroot/ts/Callejero/Paises.ts`.
