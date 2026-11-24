# Calles de un barrio

Relación de las **calles que pertenecen a un barrio**. Se abre desde el barrio y está filtrada por él (filtro **Barrio**).

## ¿Cómo hago...?

### ...para añadir una calle al barrio?

**Crear** eligiendo la calle. **Borrar** quita la calle del barrio (no borra la calle).

### ...para buscar?

Filtro **Calle**.

---

#### Detalle técnico

- Controlador: `CallesDeUnBarrioController` (Controllers/Callejero). Vista: `Views/Callejero/CrudCallesDeUnBarrio.cshtml`.
- Descriptor: `DescriptorDeCallesDeUnBarrio` (restrictor por barrio). ts: `wwwroot/ts/Callejero/CallesDeUnBarrio.ts`.
