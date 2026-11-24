# Variables de entorno

**Variables de configuración** de la instalación: nombre, descripción y **valor**. Controlan comportamientos generales (rutas, tamaños
máximos, servidores, cuentas de correo...). Por ejemplo, `CFG_ARCHIVOS_MAXIMO_TAMAÑO` fija el tamaño máximo de los archivos que se suben
(25 MB por defecto).

Los parámetros propios de cada negocio (p. ej. `FAR_...`, `REM_...`) no están aquí: se gestionan en *Parámetros de negocio*.

## ¿Cómo hago...?

### ...para cambiar una configuración?

Busque la variable (por nombre o **Valor**) y modifique su valor. Afecta a toda la instalación: hágalo con cuidado.

---

#### Detalle técnico

- Controlador: `VariablesController` (Controllers/Entorno). Vista: `Views/Entorno/CrudVariable.cshtml`. Dto: `VariableDto`.
- Descriptor: `DescriptorDeVariable`.
