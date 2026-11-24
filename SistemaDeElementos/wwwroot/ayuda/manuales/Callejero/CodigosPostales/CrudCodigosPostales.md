# Gestión de códigos postales

Maestro de **códigos postales**: el **CP**, su **provincia** y los **municipios** que abarca. Se usa en las direcciones y en las calles.

## ¿Cómo hago...?

### ...para buscar un código postal?

Filtro **Código Postal**.

### ...para ver a qué municipios corresponde?

Abra el código postal: se listan sus municipios. También desde el municipio (*Códigos postales de un municipio*) o la provincia.

---

#### Detalle técnico

- Controlador: `CodigosPostalesController` (Controllers/Callejero). Vista: `Views/Callejero/CrudCodigosPostales.cshtml`. Dto:
  `CodigoPostalDto`.
- Descriptor: `DescriptorDeCodigosPostales`. ts: `wwwroot/ts/Callejero/CodigosPostales.ts`.
