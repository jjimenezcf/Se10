# Gestión de municipios

Maestro de **municipios** de cada provincia (nombre y dígito de control **DC** del código INE). Desde un municipio se consultan sus
**códigos postales**, **barrios** y **zonas**.

## ¿Cómo hago...?

### ...para cargar los municipios oficiales?

Menú → **Importar municipios**: en *Seleccionar municipios a importar* elija los del catálogo a importar.

### ...para buscar un municipio?

Filtros por nombre, **Pais**, **Provincia** y **CP**.

### ...para ver sus códigos postales, barrios o zonas?

Desde el municipio: *Códigos postales de un municipio*, *Gestión de barrios* y *Gestión de zonas* (filtrados por el municipio).

---

#### Detalle técnico

- Controlador: `MunicipiosController` (Controllers/Callejero). Vista: `Views/Callejero/CrudMunicipios.cshtml`. Dto: `MunicipioDto`.
- Descriptor: `DescriptorDeMunicipios` (modal `ImportarMunicipiosDto`). ts: `wwwroot/ts/Callejero/Municipios.ts`.
