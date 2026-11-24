# Panel de control

Es la página que se ve **nada más entrar** en la aplicación. Muestra una **tarjeta por cada negocio con flujo** al que tiene acceso
(facturas, pedidos, expedientes, tareas...) con **cuántos elementos hay en cada tipo y estado**, para ver de un vistazo dónde hay trabajo
pendiente. Desde cada tarjeta se va a su mantenimiento, se crea un elemento, se consulta la ayuda del proceso o se pregunta a la IA.

No es un mantenimiento (no tiene grid, filtro ni menú de la vista): por eso este manual no incluye el funcionamiento común de las vistas.

## La cabecera (común a todas las páginas)

- **☰ / Sistema de elementos**: despliega el **menú** con todas las opciones a las que tiene acceso.
- **Mis filtros**: los filtros que ha guardado en cada mantenimiento (*Guardar filtros*), para abrir la vista ya filtrada.
- **Últimos registros** y **Últimos accesos**: los elementos y las vistas que ha usado últimamente.
- **Favoritos**: sus vistas favoritas.
- **Menú de información (i)**: elegir **la IA** que usa (Géminis, Mistral, Perplexity...), **Resetear el panel de control**, descargar la
  declaración responsable de **Verifactu** y **Logout** (cerrar sesión).

El pie de página es el de siempre: mensajes (historial y papelera) y el **?** que muestra este manual.

## Las tarjetas

Cada tarjeta tiene:

- **Título**: el nombre del negocio; pulsándolo se abre su **mantenimiento**.
- **+**: crear un elemento del negocio (sólo si tiene permiso para crear en alguno de sus tipos).
- **La vista de datos**, que se puede ver de tres formas (botón de **modo**, que va rotando):
  - **Matriz**: filas = tipos, columnas = estados; cada celda, la cantidad, más oscura cuanta más hay.
  - **Tartas**: una tarta por tipo con el reparto por estados.
  - **Barras**: barras por tipo y estado.
  Pasando el ratón por un sector o una barra se ve el estado y la cantidad.
- **?**: la **ayuda del proceso** del negocio (el flujo de estados y qué hace cada transición), de sólo consulta.
- **Robot**: **preguntar a la IA** sobre el negocio (p. ej. *¿cuántas facturas vencidas hay por cliente?*). La respuesta aparece en el diálogo;
  con *Mostrar filtro* se ve el filtro que ha deducido la IA y con **Consultar respuesta** se abre el mantenimiento con ese filtro aplicado.
  *Misma conversación* hace que la siguiente pregunta tenga en cuenta las anteriores.
- **← / →**: mover la tarjeta a la izquierda o a la derecha.
- **✕**: ocultar la tarjeta.

## ¿Cómo hago...?

### ...para organizar el panel a mi gusto?

Mueva las tarjetas con las flechas, ocúltelas con la ✕ y elija el modo de cada una. La disposición (orden, tarjetas visibles y modo) **se guarda
para su usuario** y se recupera la próxima vez. La primera vez las tarjetas aparecen con un modo al azar.

### ...para recuperar las tarjetas ocultas o volver a empezar?

Menú de información (i) → **Resetear el panel de control**: borra su disposición guardada y recarga el panel con todas las tarjetas.

### ...para ver los elementos de un estado concreto?

Pulse el título de la tarjeta para ir al mantenimiento y filtre por el estado, o pregúnteselo a la IA con el robot y use **Consultar respuesta**.

### ...para cambiar la IA que responde?

Menú de información (i) → elija la IA.

### ¿Por qué no veo un negocio?

Sólo aparecen los negocios con flujo (con tipos o estados) a los que tiene acceso según sus puestos de trabajo, y los que no haya ocultado.

---

#### Detalle técnico

- Página: `PanelDeControl.RenderPagina` (Descriptores/PanelDeControl.cs, cuerpo `#panel-graficas`), servida por `HomeController`. No
  tiene la cabecera de un crud: el **?** la reconoce por su contenedor de tarjetas (`paginasSinCabecera` en `ApiDeArchivos.MostrarManualDeLaVista`)
  y pide `/Home/epManualDeLaVista?vista=PanelDeControl`.
- ts: `wwwroot/ts/PanelDeControl.ts`: `Inicializar` lee los datos del dashboard, `RenderTarjetas` pinta las tarjetas y sus botones,
  `ConstruirMatriz` / `ConstruirTartas` / `ConstruirBarras` los modos, `GuardarDisposicion` / `AplicarDisposicion` la persistencia,
  `AplicarDisposicionAleatoria` la primera vez y `ResetearDashBoard` el reseteo. La ayuda del proceso sale de
  `EstadosController.epAyudaDelNegocio` y las preguntas de `ApiPreguntasIa.AbrirModalConteo`.
