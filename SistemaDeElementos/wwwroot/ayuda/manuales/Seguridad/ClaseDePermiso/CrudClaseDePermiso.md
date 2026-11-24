# Clases de permiso

Maestro de las **clases de permiso**: qué tipo de objeto protege un permiso (*Vista*, *Negocio*, *Tipo*, *CentroGestor*, *Estado*,
*Transicion*, *Elemento*, *Agenda*, *Certificado*, *Buzon*, *Plantilla*, *Exportacion*; *Menu* y *Funcion* existen sin uso). Es un maestro del
sistema: normalmente sólo se consulta. Se usa como filtro en *Gestión de permisos*.

---

#### Detalle técnico

- Controlador: `ClaseDePermisoController` (Controllers/Seguridad). Vista: `Views/Seguridad/CrudClaseDePermiso.cshtml`. Descriptor:
  `DescriptorDeClaseDePermiso`. Modelo: `wwwroot/ayuda/seguridad/ModeloDeSeguridad.md`.
