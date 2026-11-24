# Gestión de provincias

Maestro de **provincias**: nombre, **código**, **sigla** y **prefijo telefónico**, dentro de su país. Desde una provincia se consultan sus
**códigos postales**.

## ¿Cómo hago...?

### ...para buscar una provincia?

Filtros por nombre, **Código** o **CP** (provincias que tienen ese código postal).

### ...para ver los códigos postales de una provincia?

Desde la provincia, la relación de códigos postales abre *Códigos postales de una provincia*.

---

#### Detalle técnico

- Controlador: `ProvinciasController` (Controllers/Callejero). Vista: `Views/Callejero/CrudProvincias.cshtml`. Dto: `ProvinciaDto`.
- Descriptor: `DescriptorDeProvincias`. ts: `wwwroot/ts/Callejero/Provincias.ts`.
