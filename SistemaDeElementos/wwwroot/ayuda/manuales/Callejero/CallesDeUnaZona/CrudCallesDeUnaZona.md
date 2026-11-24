# Calles de una zona

Relación de las **calles que pertenecen a una zona**. Se abre desde la zona y está filtrada por ella (filtro **Zona**).

## ¿Cómo hago...?

### ...para añadir una calle a la zona?

**Crear** eligiendo la calle. **Borrar** quita la calle de la zona (no borra la calle).

### ...para buscar?

Filtro **Calle**.

---

#### Detalle técnico

- Controlador: `CallesDeUnaZonaController` (Controllers/Callejero). Vista: `Views/Callejero/CrudCallesDeUnaZona.cshtml`.
- Descriptor: `DescriptorDeCallesDeUnaZona` (restrictor por zona). ts: `wwwroot/ts/Callejero/CallesDeUnaZona.ts`.
