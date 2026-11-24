# Tipos de vía

Maestro de **tipos de vía** (calle, avenida, plaza, camino...): **sigla** y nombre (*Tipo de vía*). Se usa en las calles y direcciones.

## ¿Cómo hago...?

### ...para añadir un tipo de vía?

**Crear**: sigla (p. ej. `AV`) y nombre (*Avenida*).

### ...para buscar?

Filtro **Sigla** o por nombre.

---

#### Detalle técnico

- Controlador: `TiposDeViaController` (Controllers/Callejero). Vista: `Views/Callejero/CrudTiposDeVia.cshtml`. Dto: `TipoDeViaDto`.
- Descriptor: `DescriptorTiposDeVia` (fichero `DescriptorDeTiposDeVia.cs`). ts: `wwwroot/ts/Callejero/TiposDeVia.ts`.
