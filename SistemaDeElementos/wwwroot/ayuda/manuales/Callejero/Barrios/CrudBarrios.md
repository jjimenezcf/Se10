# Gestión de barrios

Maestro de **barrios** de un municipio. Se abre normalmente desde el municipio (filtro **Municipio**). Cada barrio agrupa calles.

## ¿Cómo hago...?

### ...para crear un barrio?

**Crear**: municipio y nombre.

### ...para ver o asignar sus calles?

Desde el barrio, *Calles de un barrio*.

### ...para buscar?

Filtro **Barrio**.

---

#### Detalle técnico

- Controlador: `BarriosController` (Controllers/Callejero). Vista: `Views/Callejero/CrudBarrios.cshtml`. Dto: `BarrioDto`.
- Descriptor: `DescriptorDeBarrios` (restrictor por municipio). ts: `wwwroot/ts/Callejero/Barrios.ts`.
