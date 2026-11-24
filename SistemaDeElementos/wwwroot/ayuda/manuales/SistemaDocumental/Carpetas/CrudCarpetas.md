# Carpetas de un archivador

Árbol de **carpetas** de un archivador. Se abre desde el archivador (*Mostrar carpetas* o *Carpetas del archivador*) y muestra sólo las
suyas. Las carpetas organizan los archivos del archivador; también se crean al importar un zip.

## ¿Cómo hago...?

### ...para crear una carpeta?

Seleccione la carpeta padre y cree la nueva dentro (nombre). Se pueden renombrar y eliminar.

### ...para mover documentos entre carpetas?

Desde el visor de archivos del archivador, eligiendo la carpeta de destino.

---

#### Detalle técnico

- Controlador: `CarpetasController` (Controllers/SistemaDocumental), vista `CrudCarpetas`: formulario con `DescriptorDeCarpetas`
  (*Carpetas de un archivador*) restringido por el id del archivador.
- ts: `wwwroot/ts/SistemaDocumental/Carpetas.ts`.
