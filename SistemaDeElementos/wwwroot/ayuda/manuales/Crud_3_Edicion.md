## La edición de un elemento

Se abre con **Editar** desde el menú de la vista con uno o varios elementos seleccionados. Muestra:

- **Cabecera**: el título, el navegador entre los elementos seleccionados (primero, anterior, siguiente, último), el menú de opciones
  del elemento y los iconos de *Plegar todos los apartados*, *Desplegar todos los apartados* y *Mostrar u ocultar el visor*.
- **Datos principales** del elemento.
- **Ampliaciones**: apartados plegables con más información (líneas, cobros, enlaces, direcciones, observaciones, archivos...). Lo que
  deja plegado o desplegado se recuerda.
- **Visor de archivos**: a la derecha, los archivos asociados al elemento.

### ¿Cómo modifico un elemento?

1. Cambie los datos.
2. Pulse **Modificar**. Si otro usuario lo ha modificado mientras lo tenía abierto, se le avisa y no se graba: vuelva a leerlo y repita
   el cambio.
3. Para salir sin grabar, × o Escape: si ha cambiado algo, pregunta antes.

Si el elemento está bloqueado, dado de baja o su usuario sólo tiene permiso de consulta, los datos se muestran pero no se pueden cambiar.

### ¿Cómo paso al siguiente elemento seleccionado?

Con las flechas del navegador de la cabecera de la edición. Sólo aparecen si seleccionó varios en el grid.

### ¿Cómo avanzo o retrocedo el estado de un elemento? (elementos con proceso)

Con las flechas de **transitar** y **devolver** de la cabecera. El icono *Qué hacen las transiciones de las flechas* explica qué hará
cada una en el estado actual. Para elegir otra transición, use **Transitar** en el menú.

### ¿Cómo trabajo con los archivos del elemento?

En el visor de archivos:

- Subir o anexar archivos. Hay un tamaño máximo por archivo.
- Escribir un texto y anexarlo como archivo.
- Ver el archivo, pasar al anterior o al siguiente, descargarlo, convertirlo a pdf, compartirlo o crear un enlace en el portapapeles.
- Editar el texto mostrado (lápiz) y anexarlo como un archivo nuevo.
- Resumir o preguntar a la IA sobre el archivo.

### ¿Cómo comparto un elemento?

En las vistas que lo permiten, desde la cabecera se puede crear un enlace de consulta (o anularlo) y copiarlo al portapapeles, o
enviar el elemento por correo / mensaje.

### Opciones del menú de la edición

Además de las propias de cada vista, suelen estar: imprimir, enviar por correo, permisos del elemento, añadir observación, dirección,
evento o archivador, dar de alta / de baja y transitar.

---

#### Detalle técnico de la edición

- Servidor: `DescriptorDeEdicion<T>` (lo crea `DescriptorDeCrud<T>` como `Editor`). Las opciones del menú se añaden con
  `Editor.IncluirMfIndividual(...)` y las ampliaciones con los descriptores de ampliación que define cada vista.
- Cliente: `Crud.CrudEdicion` (ts/_Crud/CrudEdicion.ts), con los eventos de `eventosDeEdicion` (modificar, firmar, cancelar, plegar,
  desplegar, mostrar u ocultar el visor, avanzar, retroceder). La concurrencia se controla con la versión del elemento (RowVersion).
- Cada vista puede heredar `Crud.CrudEdicion` para bloquear o desbloquear campos y opciones según el elemento leído, recalcular datos
  o tratar sus opciones de menú.
