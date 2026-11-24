# Códigos postales de un municipio

Relación de los **códigos postales de un municipio**. Se abre desde el municipio y está filtrada por él (filtro **Municipio**).

## ¿Cómo hago...?

### ...para añadir un código postal al municipio?

**Crear** eligiendo el código postal. **Borrar** quita la relación (no borra el código postal).

### ...para buscar?

Filtro **Código postal**.

---

#### Detalle técnico

- Controlador: `CpsDeUnMunicipioController` (Controllers/Callejero). Vista: `Views/Callejero/CrudCpsDeUnMunicipio.cshtml`.
- Descriptor: `DescriptorDeCpsDeUnMunicipio` (restrictor por municipio). ts: `wwwroot/ts/Callejero/CpsDeUnMunicipio.ts`.
