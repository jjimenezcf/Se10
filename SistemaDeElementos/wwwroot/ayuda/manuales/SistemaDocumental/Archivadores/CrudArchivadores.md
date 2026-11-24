# Gestión de archivadores

Un archivador es un **contenedor de documentos** organizado en **carpetas**. Lo tienen muchos elementos (expedientes, facturas, almacenes...)
y algunos se crean solos (p. ej. el archivador de reclamación de una factura vencida o los archivadores anuales de un expediente de
asesoría). Desde aquí se gestionan sus archivos, se importan y exportan en bloque y se pueden **crear facturas recibidas con IA** a partir
de los documentos de una carpeta.

## ¿Cómo hago...?

### ...para crear un archivador?

**Crear**: centro gestor (**CG**), **Tipo**, nombre y descripción. Para copiar la estructura de otro: menú contextual → **Copiar archivador**.

### ...para ver y organizar sus carpetas?

**Mostrar carpetas** (menú) o el enlace **Carpetas del archivador** de la edición: abre *Carpetas de un archivador*.

### ...para subir muchos documentos de una vez?

Menú de la edición → **Importar Zip** (interventor): elija el **zip** y cómo tratar lo que ya existe: **Renombrar** el importado si hay uno
con el mismo nombre, **Remplazar** el existente, **Eliminar archivo** (borra los que no estén en el zip) o **Eliminar carpeta** (borra las
carpetas que no estén en el zip).

### ...para descargar todo el archivador?

**Exportar** (interventor): se somete un trabajo; recibirá un correo y se creará un archivador con la exportación.

### ...para sincronizar con una carpeta de Windows?

Indique **Sincronizar con** (la carpeta) y pulse **Sincronizar**: se somete el trabajo de sincronización.

### ...para crear facturas recibidas a partir de los documentos?

**Crear facturas** (gestor): elija la **Carpeta** con las facturas, el **CG propuesto**, el **Tipo propuesto** y, si todas son del mismo,
el **Proveedor**. La IA lee cada documento y crea la factura recibida en cumplimentación. El enlace al trabajo queda en las
observaciones del archivador.

### Otras opciones

**Descontabilizar** (administrador) en los archivadores de lotes contabilizados. Los archivadores se pueden **bloquear** o dar de **baja**.

---

#### Detalle técnico

- Controlador: `ArchivadoresController` (Controllers/SistemaDocumental). Vista: `Views/SistemaDocumental/CrudArchivadores.cshtml`. Dto:
  `ArchivadorDto`.
- Descriptor: `DescriptorDeArchivadores`: filtro *Archivador*, modales *Importar Zip*, *Procesar archivos* (crear facturas con IA),
  *Copiar archivador*; opciones *Exportar*, *Descontabilizar*, *Mostrar carpetas*.
- ts: `wwwroot/ts/SistemaDocumental/Archivadores.ts`: somete la sincronización y la exportación, recarga tras los trabajos, habilita
  *Descontabilizar* sólo a administradores.
