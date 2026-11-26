## La página de consulta

Ha llegado aquí desde un enlace que le ha enviado quien gestiona el elemento. No necesita usuario ni contraseña: la página es **sólo
de consulta**, no se puede modificar nada.

Muestra:

- **Los datos del elemento**, arriba.
- **Observaciones**, con los comentarios que se han ido anotando.
- **Archivos**, con los documentos anexados al elemento.

Los apartados se muestran siempre abiertos.

### ¿Cómo veo un archivo?

1. En el apartado **Archivos**, pulse el nombre del archivo.
2. En una pantalla grande se muestra en el visor, a la derecha. Con las flechas de la barra del visor pasa al archivo anterior o al
   siguiente.
3. En el móvil el visor no se muestra: pulse el **ojo** (*ver archivo*) que hay junto al botón de descargar y se abre en una ventana.
   Para cerrarla, pulse la × o Escape.

Sólo se pueden ver los archivos que el navegador sabe mostrar (pdf, imágenes, documentos de texto...). Con los demás, descárguelos.

### ¿Cómo descargo un archivo?

Pulse el botón de descargar, que está junto al nombre del archivo en la lista y también en la barra del visor.

### ¿Qué hago si la página muestra un error?

El enlace ya no es válido (por ejemplo, porque se ha anulado). Pida un enlace nuevo a quien se lo envió.

---

#### Detalle técnico de la consulta

- Servidor: la acción `Consultar(guid, id)` de `EntidadController` valida el guid y pinta `DescriptorDePaginaDeConsulta`, que crea el
  descriptor de edición del negocio, el apartado de observaciones, el de archivos (y el de eventos de agenda en infantes). Los apartados
  se pintan abiertos (`MostrarPlegado = false`).
- Cliente: la página arranca con `CrudEdicion.ConsultarSeleccionado`, que valida el guid (`ValidarConsultaPorGuid`) y lee el elemento.
  Los archivos se piden por guid (`epLeerAnexadosPorGuid`, `epDescargarArchivoPorGuid`, `epDescargarComoHtmlPorGuid`).
- Archivos: la fila de cada archivo la pinta `ApiControl.CrearVisor`. El ojo (`ApiDeArchivos.AbrirModalDeVerArchivo`) sólo es visible en
  móvil (`ControlesDto.css`); en pantallas grandes el visor es `ApiConsultaDeArchivos.VisorParaConsultaDeArchivos`.
- Este manual: el `?` del pie llama a `ApiDeArchivos.MostrarManualDeLaVista`, que en estas páginas (`#cuerpo-de-pagina.cuerpo-solo-consulta`)
  pide `/{controlador}/epManualDeLaVista?vista=Consultar`. `epManualDeLaVista` admite llamadas sin usuario sólo para esa vista, y no se
  ofrece el robot de la IA (necesita usuario).
