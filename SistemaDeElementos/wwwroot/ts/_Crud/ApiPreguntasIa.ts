namespace ApiPreguntasIa {

    export function InyectarAccesoIA(crud: Crud.CrudMnt): void {
        const contenedor = document.querySelector('.' + ltrCss.crud.filtro.expansor);
        if (contenedor && !document.getElementById('acceso-ia-preguntame')) {
            const linkIA = document.createElement('a');
            linkIA.id = 'acceso-ia-preguntame';
            linkIA.href = 'javascript:void(0);';
            linkIA.innerText = 'Pregúntame';
            linkIA.style.marginRight = '15px';
            linkIA.style.fontWeight = 'bold';
            linkIA.style.color = '#007bff';
            linkIA.onclick = () => AbrirModal(crud);
            contenedor.insertBefore(linkIA, contenedor.firstChild);
        }
    }

    export function AbrirModal(crud: Crud.CrudMnt): void {
        const modalOverlay = document.createElement('div') as HTMLDivElement;
        ApiControl.IncluirCss(modalOverlay, ltrCss.crud.modal.dinamica);

        const historial = crud.Preguntas;

        let htmlHistorial = '';
        if (historial.length > 0) {
            const recientes = historial.slice(-5).reverse();
            htmlHistorial = ltrHtml.Divs.PreguntasIa.replace('[PreguntasIa]', recientes.map(q => `<div class='${ltrCss.crud.modal.LineaDelhistorialIa}' title='${q.pregunta.replace(/"/g, '&quot;')}'>${q.pregunta}</div>`).join(''));
        }

        modalOverlay.innerHTML = ltrHtml.Modales.PreguntasIa
            .replace('[htmlHistorial]', htmlHistorial)
            .replace('[pregunta]', '');

        if (crud.NuevaConversacion === true || historial.length === 0) {
            modalOverlay.innerHTML = modalOverlay.innerHTML.replace('[nuevaconservacion]', 'checked');
            crud.NuevaConversacion = true;
        }
        document.body.appendChild(modalOverlay);

        const vistaPregunta = modalOverlay.querySelector('#vista-pregunta-ia') as HTMLDivElement;
        const vistaJson = modalOverlay.querySelector('#vista-edicion-json') as HTMLDivElement;
        const txtInput = modalOverlay.querySelector('#input-pregunta-ia') as HTMLTextAreaElement;
        const txtJson = modalOverlay.querySelector('#textarea-json-ia') as HTMLTextAreaElement;

        const btnRespuesta = modalOverlay.querySelector('#btn-ver-respuesta-json');
        const btnVolver = modalOverlay.querySelector('#btn-volver-pregunta');
        const btnGrabar = modalOverlay.querySelector('#btn-grabar-json');
        const btnPreguntar = modalOverlay.querySelector('#btn-ejecutar-pregunta');
        const btnCerrar = modalOverlay.querySelector('#btn-cerrar-dinamico');

        btnRespuesta?.addEventListener('click', () => {
            ApiPanel.OcultarPanel(vistaPregunta);
            ApiPanel.MostrarPanel(vistaJson);
            const respuestaOriginal = crud.Preguntas.find(p => p.pregunta === txtInput.value)?.respuesta;
            ApiControl.MapearEnElAreaDeTextoUnJoson(txtJson, respuestaOriginal);
        });

        btnVolver?.addEventListener('click', () => {
            ApiPanel.OcultarPanel(vistaJson);
            ApiPanel.MostrarPanel(vistaPregunta);
        });

        btnGrabar?.addEventListener('click', () => {
            // pendiente: guardar filtro IA en servidor
        });

        const itemsHistorial = modalOverlay.querySelectorAll('.' + ltrCss.crud.modal.LineaDelhistorialIa);
        itemsHistorial.forEach(item => {
            item.addEventListener('click', (e) => {
                txtInput.value = (e.currentTarget as HTMLElement).innerText;
                txtInput.focus();
            });
        });

        setTimeout(() => ApiControl.IncluirCss(modalOverlay, 'fade-in'), 10);
        const cerrar = () => {
            ApiControl.ExcluirCss(modalOverlay, 'fade-in');
            setTimeout(() => modalOverlay.remove(), 300);
        };

        btnPreguntar?.addEventListener('click', () => {
            const pregunta = txtInput.value.trim();
            if (pregunta) {
                LanzarPregunta(crud, pregunta);
                cerrar();
            } else {
                txtInput.focus();
                txtInput.style.borderColor = 'red';
            }
        });

        btnCerrar?.addEventListener('click', cerrar);
        modalOverlay.onclick = (e) => { if (e.target === modalOverlay) cerrar(); };
        setTimeout(() => txtInput.focus(), 100);
    }

    export function LanzarPregunta(crud: Crud.CrudMnt, texto: string): void {
        crud.Pregunta = texto;
        crud.NuevaConversacion = (document.getElementById('chk-nueva-pregunta') as HTMLInputElement).checked;
        crud.CargarGrid();
    }

    // ─── Conteo IA desde el Panel de Control ────────────────────────────────

    export function InyectarConteoIA(negocioEnum: string, negocioNombre: string, btn: HTMLButtonElement): void {
        btn.dataset.negocio = negocioEnum;
        btn.addEventListener('click', () => AbrirModalConteo(negocioEnum, negocioNombre));
    }

    export function AbrirModalConteo(negocioEnum: string, negocioNombre: string): void {
        const overlay = document.createElement('div');
        overlay.className = 'grafica-conteo-overlay';

        // GUID único por instancia de modal — identifica la sesión de conversación
        const sessionGuid = 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, c => {
            const r = Math.random() * 16 | 0;
            return (c === 'x' ? r : (r & 0x3 | 0x8)).toString(16);
        });

        overlay.innerHTML = `
            <div class="grafica-conteo-modal">
                <div class="grafica-conteo-titulo">Pregunta sobre ${negocioNombre}</div>
                <textarea id="gc-pregunta" class="grafica-conteo-pregunta"
                    placeholder="Escribe tu pregunta, p.ej.: ¿cuántos ${negocioNombre} hay por tipo?" rows="5"></textarea>
                <div id="gc-respuesta" class="grafica-conteo-respuesta" style="display:none"></div>
                <div class="grafica-conteo-botones">
                    <label class="grafica-conteo-misma-conv">
                        <input type="checkbox" id="gc-chk-misma-conv" checked>
                        Misma conversación
                    </label>
                    <span id="gc-segundero" class="grafica-conteo-segundero" style="display:none">⏱ <span id="gc-seg">0</span>s</span>
                    <button id="gc-btn-cerrar">Cerrar</button>
                    <button id="gc-btn-preguntar">Preguntar</button>
                </div>
            </div>`;

        document.body.appendChild(overlay);
        setTimeout(() => overlay.classList.add('fade-in'), 10);

        const txtPregunta  = overlay.querySelector('#gc-pregunta')       as HTMLTextAreaElement;
        const divRespuesta = overlay.querySelector('#gc-respuesta')       as HTMLDivElement;
        const btnPreguntar = overlay.querySelector('#gc-btn-preguntar')   as HTMLButtonElement;
        const btnCerrar    = overlay.querySelector('#gc-btn-cerrar')      as HTMLButtonElement;
        const chkMismaConv = overlay.querySelector('#gc-chk-misma-conv') as HTMLInputElement;
        const spanSegundero = overlay.querySelector('#gc-segundero')      as HTMLSpanElement;
        const spanSeg       = overlay.querySelector('#gc-seg')            as HTMLSpanElement;

        ApiControl.IncluirCss(btnCerrar,    'boton-modal');
        ApiControl.IncluirCss(btnCerrar,    'btn');
        ApiControl.IncluirCss(btnCerrar,    'btn-secondary');
        ApiControl.IncluirCss(btnCerrar,    'boton-por-defecto');
        ApiControl.IncluirCss(btnPreguntar, 'boton-modal');
        ApiControl.IncluirCss(btnPreguntar, 'btn');
        ApiControl.IncluirCss(btnPreguntar, 'btn-primary');
        ApiControl.IncluirCss(btnPreguntar, 'boton-por-defecto');

        const cerrar = () => {
            overlay.classList.remove('fade-in');
            setTimeout(() => overlay.remove(), 280);
        };

        btnCerrar.addEventListener('click', cerrar);

        btnPreguntar.addEventListener('click', () => {
            const pregunta = txtPregunta.value.trim();
            if (!pregunta) {
                txtPregunta.style.borderColor = 'red';
                txtPregunta.focus();
                return;
            }
            txtPregunta.style.borderColor = '';
            btnPreguntar.disabled = true;
            divRespuesta.style.display = 'none';
            divRespuesta.textContent = '';

            // Iniciar segundero
            let segundos = 0;
            spanSeg.textContent = '0';
            spanSegundero.style.display = '';
            const timer = setInterval(() => {
                segundos++;
                spanSeg.textContent = String(segundos);
            }, 1000);

            const continuarConversacion = chkMismaConv.checked;

            LanzarPreguntaDeConteo(negocioEnum, pregunta, sessionGuid, continuarConversacion, respuesta => {
                clearInterval(timer);
                spanSegundero.style.display = 'none';
                divRespuesta.textContent = respuesta;
                divRespuesta.style.display = '';
                btnPreguntar.disabled = false;
            });
        });

        setTimeout(() => txtPregunta.focus(), 80);
    }

    export function LanzarPreguntaDeConteo(
        negocioEnum: string,
        pregunta: string,
        sessionGuid: string,
        continuarConversacion: boolean,
        onResultado: (respuesta: string) => void
    ): void {

        const url = `/${Ajax.Entorno.Ia.controlador}/${Ajax.Entorno.Ia.PregutaConConteo}` +
            `?${literal.negocio}=${encodeURIComponent(negocioEnum)}` +
            `&pregunta=${encodeURIComponent(pregunta)}` +
            `&sessionGuid=${encodeURIComponent(sessionGuid)}` +
            `&continuarConversacion=${continuarConversacion}`;
        const body = JSON.stringify([{ parametro: 'filtro', valor: '' }]);

        fetch(url, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body
        })
        .then(r => r.json())
        .then((resultado: any) => {
            if (resultado?.estado === 'Ok' || resultado?.estado === 0) {
                onResultado(resultado.datos ?? '(sin respuesta)');
            } else {
                onResultado(`Error: ${resultado?.mensaje ?? 'respuesta desconocida'}`);
            }
        })
        .catch(err => onResultado(`Error de red: ${err}`));
    }

}
