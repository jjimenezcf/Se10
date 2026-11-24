# Gestión de juzgados

Catálogo de **juzgados** donde se celebran los pleitos y procedimientos (se eligen en el pleito y en los datos jurídicos del expediente).
Cada juzgado tiene su **clase de juzgado**.

## ¿Cómo hago...?

### ...para cargar los juzgados oficiales?

Menú → **Importar juzgados**: en la modal *Seleccionar juzgados a importar* elija los juzgados del catálogo judicial a importar. Los catálogos
judiciales de la sociedad se descargan desde *Gestión de sociedades* → *Catálogos judiciales*.

### ...para dar de alta un juzgado a mano?

**Crear**: nombre y clase de juzgado.

---

#### Detalle técnico

- Controlador: `JuzgadosController` (Controllers/Terceros). Vista: `Views/Terceros/CrudJuzgados.cshtml`.
- Descriptor: `DescriptorDeJuzgados` (modal `ImportarJuzgadosDto`). ts: `wwwroot/ts/Terceros/Juzgados.ts`.
