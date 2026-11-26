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
- En el móvil, donde el visor está oculto, cada archivo de la lista tiene un **ojo** (*ver archivo*), junto al de descargar, que lo
  abre en una ventana de consulta. También está en la página de consulta por enlace.
- Resumir con la IA, pasar el **OCR** al archivo o preguntar a la IA sobre él (el robot verde, junto al icono del OCR).

### ¿Para qué sirve cada icono de la barra del apartado Archivos?

En la parte de arriba del apartado **Archivos** hay una barra con iconos para subir y organizar archivos. Pase el dedo o el ratón por
un icono para ver su nombre; son estos:

| Icono | Nombre | Para qué sirve |
|---|---|---|
| Seleccionar archivo | **Seleccionar archivos** | Abre el selector del dispositivo para elegir uno o varios archivos. También puede arrastrarlos al apartado. Quedan pendientes de subir. |
| Portapapeles | **Subir portapapeles** | Sube como archivo la imagen que tenga copiada en el portapapeles (por ejemplo una captura de pantalla). |
| Escribir | **Escribir un archivo de texto y anexarlo** | Abre un cuadro para escribir un texto; al aceptar le pide un nombre y lo anexa como archivo `.txt`. |
| Subir | **Subir archivos seleccionados** | Anexa al elemento los archivos elegidos con *Seleccionar archivos* o *Subir portapapeles*. Hasta que no pulse este icono no se guardan. |
| Copiar | **Copiar archivos seleccionados** | Copia los archivos marcados en la lista a otro elemento (ver más abajo). |
| Mover | **Mover archivos seleccionados** | Mueve los archivos marcados a otro elemento: dejan de estar en éste. |
| Enlazar | **Enlazar archivos seleccionados** | Hace que los archivos marcados aparezcan también en otro elemento sin duplicarlos. |
| Candado cerrado | **Bloquear archivos seleccionados** | Bloquea a la vez todos los archivos marcados; abre una ventana donde se confirma. |
| Candado abierto | **Desbloquear archivos seleccionados** | Desbloquea a la vez todos los archivos marcados; abre una ventana donde se confirma. |
| ZIP | **Genera archivador con los archivos seleccionados en un ZIP** | Junta los archivos marcados en un único archivo ZIP, que se guarda como un archivador. |

Los iconos que actúan sobre los archivos marcados (copiar, mover, enlazar, bloquear, desbloquear, ZIP) necesitan que marque antes la
casilla de los archivos; si no hay ninguno marcado, avisan de que hay que seleccionarlos.

A la derecha de la barra hay:

- Un **filtro por nombre**: escriba parte del nombre para ver sólo esos archivos. Puede usar `%` para varios trozos
  (`factura%2026`).
- **Nº de columnas**: cada pulsación cambia en cuántas columnas se reparten los archivos (se recuerda para su usuario).
- **Seleccionar todo** y **Deseleccionar**: marcan o desmarcan las casillas de todos los archivos.

#### Copiar, mover o enlazar archivos a otro elemento

1. Marque las casillas de los archivos y pulse **Copiar**, **Mover** o **Enlazar**. Los archivos quedan "en espera" y el icono cambia
   para indicarlo.
2. Vaya al elemento de destino (abra su edición) y pulse el mismo icono: los archivos se aplican allí.
3. Para arrepentirse, pulse el icono de **cancelar operación** antes de aplicarla.

### ¿Para qué sirve cada icono de cada archivo de la lista?

En el apartado **Archivos**, a la derecha de cada archivo hay unos iconos. En el móvil no se puede leer su ayuda al pasar el ratón, así
que son estos (según el archivo y su estado pueden no aparecer todos):

| Icono | Nombre | Para qué sirve |
|---|---|---|
| Flecha de descarga | **Descargar archivo** | Baja el archivo a su dispositivo. |
| Ojo | **Ver archivo** | Sólo en el móvil: abre el archivo en una ventana para verlo (pdf, imágenes, texto...). En pantallas grandes se ve en el visor de la derecha. No sale si el navegador no sabe mostrar ese tipo de archivo. |
| Papelera | **Eliminar archivo** | Borra el archivo del elemento; pide confirmación. No sale en archivos bloqueados, cancelados, del sistema o de un archivador vinculado. |
| Firma | **Firmar archivo** | Firma digitalmente el archivo con uno de sus certificados; pide el certificado y la contraseña. |
| Candado cerrado | **Bloquear archivo** | Impide que se modifique o se borre; pide el motivo. Mientras está bloqueado desaparecen la papelera y la firma. |
| Candado abierto | **Desbloquear archivo** | Sustituye al candado cerrado en los archivos bloqueados; vuelve a permitir modificarlos y borrarlos. |

Además, a la izquierda del nombre hay una casilla para marcar el archivo (para las acciones sobre varios archivos), y pulsando el
nombre se muestra en el visor.

### ¿Cómo le pregunto a la IA sobre un archivo?

1. Seleccione en el visor el archivo sobre el que quiere preguntar.
2. Pulse el **robot verde** (*Pregúntame sobre este archivo*), a la derecha del icono del OCR en la barra del visor.
3. Escriba su pregunta, p. ej. *¿cuál es el importe total?*, y pulse **Preguntar**. Mientras la IA responde se muestra un segundero.
4. La respuesta aparece debajo de la pregunta. Puede hacer otra pregunta sobre el mismo archivo o pulsar **Cerrar** (o Escape).

Tenga en cuenta que:

- La respuesta sale sólo del contenido del archivo: si no está en él, la IA lo dice y no inventa datos.
- Sirve con pdf, documentos de Word, rtf, html y archivos de texto. Con imágenes no está disponible: pásele antes el **OCR** y
  pregunte sobre el texto obtenido.
- En los pdf el texto se extrae con OCR en cada pregunta, así que la primera respuesta puede tardar un poco más.
- En documentos muy largos sólo se tiene en cuenta el principio (unos 100.000 caracteres).
- Se usa la IA que tiene configurada su usuario.

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
- Pregúntame sobre el archivo: el botón (`enumCssControles.PreguntameSobre`, clases `preguntame-sobre boton-ia-de-modal`) lo pinta
  `DescriptorDeEdicion.RenderVisorDeArchivos` junto al del OCR y lanza el evento `preguntame-sobre`, que atiende
  `CrudEdicion.PreguntameSobre` → `ApiVisorDeArchivos.PreguntarSobreElArchivoMostrado` → `ApiPreguntasIa.AbrirModalPreguntameSobre`
  (el mismo diálogo que "¿Cómo se hace...?"). La pregunta va a `Archivos/epPreguntameSobre` (negocio, idElemento, idArchivo,
  pregunta), que valida el acceso al archivo, extrae su texto con `ApiDeArchivos.ExtraerTextoPlano` y se lo pasa, con la pregunta, a la
  IA del usuario.
- Cada vista puede heredar `Crud.CrudEdicion` para bloquear o desbloquear campos y opciones según el elemento leído, recalcular datos
  o tratar sus opciones de menú.
