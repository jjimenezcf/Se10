// Plantillas html y los ids de sus elementos que se buscan desde el ts.
// Depende de literales_Css.
// Orden de carga: ListaDeScripts.cs. Al definirse, un literal solo puede leer los de ficheros anteriores de esa lista.

// ids de los elementos de las plantillas de ltrHtml que se buscan desde el ts
const ltrIdsHtml = {
    // diálogo "¿Cómo se hace...?" del manual de una vista (ltrHtml.Modales.ComoSeHace, ApiPreguntasIa.AbrirModalComoSeHace)
    ComoSeHace: {
        Pregunta: 'csh-pregunta',
        AreaRespuesta: 'csh-area-respuesta',
        Respuesta: 'csh-respuesta',
        Segundero: 'csh-segundero',
        Segundos: 'csh-seg',
        Cerrar: 'csh-btn-cerrar',
        Preguntar: 'csh-btn-preguntar'
    },
    // diálogo de preguntar a la IA del robot de las tarjetas del panel de control (ltrHtml.Modales.Conteo, ApiPreguntasIa.AbrirModalConteo)
    Conteo: {
        Pregunta: 'gc-pregunta',
        AreaRespuesta: 'gc-area-respuesta',
        Respuesta: 'gc-respuesta',
        Preguntar: 'gc-btn-preguntar',
        Cerrar: 'gc-btn-cerrar',
        MismaConversacion: 'gc-chk-misma-conv',
        Segundero: 'gc-segundero',
        Segundos: 'gc-seg',
        Consultar: 'gc-link-consultar',
        MostrarFiltro: 'gc-link-toggle-filtro'
    }
}

const ltrHtml = {
    Divs: {
        PreguntasIa: `
            <div class='${ltrCss.crud.modal.ContenedorDeHistorialIa}' id="seccion-historial-ia">
                <p>Consultas recientes:</p>
                <div id='contenedor-historial-ia'>
                    [PreguntasIa]
                </div>
            </div>`
    },
    Modales: {
        // Modal para editar el texto que muestra el visor (ver ApiDeArchivos.AbrirModalDeEditarTexto). Usa las clases comunes de las
        // modales (CrudModal.css): cabecera azul con el icono, el título y la × de cerrar, y el botón Aceptar en el pie.
        // [titulo] es el nombre del archivo; el texto se asigna al textarea desde el código (no se escapa en la plantilla)
        EditarTexto: `
                 <div class="${ltrCss.flujo.contenidoModal} ${ltrCss.crud.modal.editarTexto}">
                     <div class="${ltrCss.flujo.cabeceraDeModal}">
                         <span class="${ltrCss.flujo.rotuloDeModal}">
                             <svg class="${ltrCss.flujo.iconoDeModal}" viewBox="0 0 24 24" aria-hidden="true"><path d="M12 20h9"/><path d="M16.5 3.5a2.12 2.12 0 0 1 3 3L7 19l-4 1 1-4Z"/></svg>
                             <h2>Editar: [titulo]</h2>
                         </span>
                         <span id="btn-editar-texto-cerrar" class="${ltrCss.flujo.cerrarDeModal}" title="Cerrar"><svg class="${ltrCss.flujo.iconoDeModal}" viewBox="0 0 24 24" aria-hidden="true"><line x1="6" y1="6" x2="18" y2="18"/><line x1="18" y1="6" x2="6" y2="18"/></svg></span>
                     </div>
                     <div class="${ltrCss.flujo.cuerpo}">
                         <textarea id="editar-texto-contenido" spellcheck="false"></textarea>
                     </div>
                     <div class="${ltrCss.flujo.pie}">
                         <button id="btn-editar-texto-aceptar" title="Guarda el texto como un archivo nuevo y quita el que se está editando">Aceptar</button>
                     </div>
                 </div>`,
        // Modal para elegir la página con la que se convierte una hoja de cálculo a pdf (ver ApiVisorDeArchivos.PedirOpcionesDePdf)
        OpcionesDePdf: `
                 <div class="modal-opciones-pdf">
                     <h2>Convertir a PDF</h2>
                     <div class="${ltrCss.crud.modal.cuerpo}">
                         <label class="label-propiedad" for="pdf-tamano">Tamaño de la hoja</label>
                         <select id="pdf-tamano">
                             <option value="A4" selected>A4</option>
                             <option value="A3">A3</option>
                         </select>
                         <label class="label-propiedad" for="pdf-orientacion">Orientación</label>
                         <select id="pdf-orientacion">
                             <option value="Horizontal" selected>Horizontal</option>
                             <option value="Vertical">Vertical</option>
                         </select>
                     </div>
                     <div class="${ltrCss.crud.modal.pie}">
                         <button id="btn-pdf-cancelar">Cancelar</button>
                         <button id="btn-pdf-aceptar">Aceptar</button>
                     </div>
                 </div>`,
        // Modal para pedir el nombre del archivo de texto que se escribe desde el selector de archivos (ver ApiDeArchivos.PedirNombreDeArchivo)
        NombreDeArchivo: `
                 <div class="modal-nombre-de-archivo">
                     <h2>Nombre del archivo</h2>
                     <div class="${ltrCss.crud.modal.cuerpo}">
                         <label class="label-propiedad" for="nombre-de-archivo">Se anexará como un archivo .txt</label>
                         <input id="nombre-de-archivo" type="text" maxlength="200" autocomplete="off" />
                     </div>
                     <div class="${ltrCss.crud.modal.pie}">
                         <button id="btn-nombre-de-archivo-cancelar">Cancelar</button>
                         <button id="btn-nombre-de-archivo-aceptar">Aceptar</button>
                     </div>
                 </div>`,
        // Diálogo de preguntar a la IA del robot de una tarjeta del panel de control. [negocio] es el nombre del negocio (ya escapado)
        Conteo: `
                 <div class="${ltrCss.PanelDeControl.Conteo.Modal}">
                     <div class="${ltrCss.PanelDeControl.Conteo.Cabecera}">
                         <div class="${ltrCss.PanelDeControl.Conteo.Titulo}">Pregunta sobre [negocio]</div>
                         <a id="${ltrIdsHtml.Conteo.Consultar}" class="${ltrCss.PanelDeControl.Conteo.Consultar}" href="#" style="display:none">Consultar respuesta</a>
                     </div>
                     <textarea id="${ltrIdsHtml.Conteo.Pregunta}" class="${ltrCss.PanelDeControl.Conteo.Pregunta}"
                         placeholder="Escribe tu pregunta, p.ej.: ¿cuántos [negocio] hay por tipo?" rows="5"></textarea>
                     <div id="${ltrIdsHtml.Conteo.AreaRespuesta}" style="display:none">
                         <div class="${ltrCss.PanelDeControl.Conteo.RespuestaCabecera}">
                             <a id="${ltrIdsHtml.Conteo.MostrarFiltro}" class="${ltrCss.PanelDeControl.Conteo.Consultar}" href="#">Mostrar filtro</a>
                         </div>
                         <div id="${ltrIdsHtml.Conteo.Respuesta}" class="${ltrCss.PanelDeControl.Conteo.Respuesta}"></div>
                     </div>
                     <div class="${ltrCss.PanelDeControl.Conteo.Botones}">
                         <label class="${ltrCss.PanelDeControl.Conteo.MismaConversacion}">
                             <input type="checkbox" id="${ltrIdsHtml.Conteo.MismaConversacion}" checked>
                             Misma conversación
                         </label>
                         <span id="${ltrIdsHtml.Conteo.Segundero}" class="${ltrCss.PanelDeControl.Conteo.Segundero}" style="display:none">⏱ <span id="${ltrIdsHtml.Conteo.Segundos}">0</span>s</span>
                         <button id="${ltrIdsHtml.Conteo.Cerrar}">Cerrar</button>
                         <button id="${ltrIdsHtml.Conteo.Preguntar}">Preguntar</button>
                     </div>
                 </div>`,
        // Diálogo "¿Cómo se hace...?" del manual de una vista: mismo aspecto que el de preguntar a la IA de las tarjetas del panel de
        // control. El título se asigna desde el código (textContent) para no tener que escaparlo
        ComoSeHace: `
                 <div class="${ltrCss.PanelDeControl.Conteo.Modal}">
                     <div class="${ltrCss.PanelDeControl.Conteo.Cabecera}">
                         <div class="${ltrCss.PanelDeControl.Conteo.Titulo}"></div>
                     </div>
                     <textarea id="${ltrIdsHtml.ComoSeHace.Pregunta}" class="${ltrCss.PanelDeControl.Conteo.Pregunta}" rows="4"
                         placeholder="¿Cómo se hace...? p.ej.: ¿cómo registro un cobro?"></textarea>
                     <div id="${ltrIdsHtml.ComoSeHace.AreaRespuesta}" style="display:none">
                         <div id="${ltrIdsHtml.ComoSeHace.Respuesta}" class="${ltrCss.PanelDeControl.Conteo.Respuesta} ${ltrCss.PanelDeControl.Conteo.RespuestaDeTexto}"></div>
                     </div>
                     <div class="${ltrCss.PanelDeControl.Conteo.Botones}">
                         <span id="${ltrIdsHtml.ComoSeHace.Segundero}" class="${ltrCss.PanelDeControl.Conteo.Segundero}" style="display:none">⏱ <span id="${ltrIdsHtml.ComoSeHace.Segundos}">0</span>s</span>
                         <button id="${ltrIdsHtml.ComoSeHace.Cerrar}">Cerrar</button>
                         <button id="${ltrIdsHtml.ComoSeHace.Preguntar}">Preguntar</button>
                     </div>
                 </div>`,
        PreguntasIa: `
                 <div id="modal-ia-container">
                     <div class="grafica-conteo-cabecera">
                         <h2>Asistente de Filtrado</h2>
                         <span style="display:inline-flex; align-items:center; gap:12px;">
                             <a id="link-ver-respuesta-ia" class="grafica-conteo-consultar" href="javascript:void(0);" style="display:none">Respuesta</a>
                             <span id="btn-cerrar-x-ia" class="cerrar-de-modal" title="Cerrar">&times;</span>
                         </span>
                     </div>

                     <div id="vista-pregunta-ia">
                         <div class="${ltrCss.crud.modal.cuerpo}">
                             [htmlHistorial]
                             <p>¿Qué estás buscando? Cuéntamelo y generaré los filtros por ti:</p>
                             <textarea id="input-pregunta-ia" placeholder="Indícame nombre, tipo...">[pregunta]</textarea>
                         </div>
                         <div class="${ltrCss.crud.modal.pie}">
                             <label class="grafica-conteo-misma-conv">
                                 <input type="checkbox" id="chk-nueva-pregunta" [nuevaconservacion] />
                                 Misma conversación
                             </label>
                             <button id="btn-cerrar-dinamico" title="Quita los filtros de la IA y empieza una nueva conversación">Resetear</button>
                             <button id="btn-ejecutar-pregunta">Preguntar</button>
                         </div>
                     </div>

                     <div id="vista-edicion-json" style="display: none;">
                         <div class="${ltrCss.crud.modal.cuerpo}">
                             <p>Configuración técnica del filtro (JSON):</p>
                             <textarea id="textarea-json-ia" style="width: 100%; height: 250px; padding: 10px; border-radius: 4px; border: 1px solid #ccc; font-family: monospace; font-size: 12px; resize: vertical;"></textarea>
                         </div>
                         <div class="${ltrCss.crud.modal.pie}">
                             <button id="btn-volver-pregunta">Cerrar</button>
                             <button id="btn-grabar-json">Grabar</button>
                         </div>
                     </div>
                 </div>`
    }
};
