# Gestión de zonas

Maestro de **zonas** de un municipio (agrupaciones de calles para organizar el trabajo, p. ej. por rutas o servicios). Se abre normalmente
desde el municipio (filtro **Municipio**).

## ¿Cómo hago...?

### ...para crear una zona?

**Crear**: municipio y nombre.

### ...para asignar calles a la zona?

Desde la zona, *Calles de una zona*.

### ...para buscar?

Filtro **Zona**.

---

#### Detalle técnico

- Controlador: `ZonasController` (Controllers/Callejero). Vista: `Views/Callejero/CrudZonas.cshtml`. Dto: `ZonaDto`.
- Descriptor: `DescriptorDeZonas` (restrictor por municipio). ts: `wwwroot/ts/Callejero/Zonas.ts`.
