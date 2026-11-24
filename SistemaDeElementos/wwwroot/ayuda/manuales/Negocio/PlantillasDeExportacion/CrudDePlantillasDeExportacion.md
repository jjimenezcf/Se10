# Plantillas de exportación

Plantillas que definen **qué columnas y en qué formato se exportan** los datos de un negocio con *Exportar datos* (menú contextual de los
mantenimientos). Cada plantilla tiene su permiso de acceso (`Exportacion`).

## ¿Cómo hago...?

### ...para preparar una exportación a medida?

**Crear** la plantilla para el negocio con las columnas que quiere. Al exportar, elíjala en la modal de exportación.

---

#### Detalle técnico

- Controlador: `PlantillasDeExportacionController` (Controllers/Negocio). Vista: `Views/Negocio/CrudDePlantillasDeExportacion.cshtml`.
  Descriptor: `DescriptorDePlantillasDeExportacion`.
