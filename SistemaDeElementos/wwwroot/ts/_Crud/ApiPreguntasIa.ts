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

        const hayHistorial = historial.length > 0;
        modalOverlay.innerHTML = ltrHtml.Modales.PreguntasIa
            .replace('[htmlHistorial]', htmlHistorial)
            .replace('[pregunta]', '')
            .replace('[nuevaconservacion]', hayHistorial ? 'checked' : '');
        crud.NuevaConversacion = !hayHistorial;

        document.body.appendChild(modalOverlay);

        const vistaPregunta = modalOverlay.querySelector('#vista-pregunta-ia') as HTMLDivElement;
        const vistaJson = modalOverlay.querySelector('#vista-edicion-json') as HTMLDivElement;
        const txtInput = modalOverlay.querySelector('#input-pregunta-ia') as HTMLTextAreaElement;
        const txtJson = modalOverlay.querySelector('#textarea-json-ia') as HTMLTextAreaElement;

        const linkRespuesta = modalOverlay.querySelector('#link-ver-respuesta-ia') as HTMLAnchorElement;
        linkRespuesta.style.display = '';
        const btnVolver = modalOverlay.querySelector('#btn-volver-pregunta') as HTMLButtonElement;
        const btnGrabar = modalOverlay.querySelector('#btn-grabar-json') as HTMLButtonElement;
        const btnPreguntar = modalOverlay.querySelector('#btn-ejecutar-pregunta') as HTMLButtonElement;
        const btnCerrar = modalOverlay.querySelector('#btn-cerrar-dinamico') as HTMLButtonElement;

        ApiControl.IncluirCss(btnCerrar, ltrCss.Modal.BotonSecundario);
        ApiControl.IncluirCss(btnPreguntar, ltrCss.Modal.BotonPrincipal);
        ApiControl.IncluirCss(btnVolver, ltrCss.Modal.BotonSecundario);
        ApiControl.IncluirCss(btnGrabar, ltrCss.Modal.BotonPrincipal);

        linkRespuesta?.addEventListener('click', () => {
            ApiPanel.OcultarPanel(vistaPregunta);
            ApiPanel.MostrarPanel(vistaJson);
            linkRespuesta.style.display = 'none';
            const respuestaOriginal = crud.Preguntas.find(p => p.pregunta === txtInput.value)?.respuesta;
            ApiControl.MapearEnElAreaDeTextoUnJoson(txtJson, respuestaOriginal);
        });

        btnVolver?.addEventListener('click', () => {
            ApiPanel.OcultarPanel(vistaJson);
            ApiPanel.MostrarPanel(vistaPregunta);
            linkRespuesta.style.display = '';
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

        btnCerrar?.addEventListener('click', () => {
            crud.Pregunta = null;
            crud.CargarGrid();
            const linkIA = document.getElementById('acceso-ia-preguntame');
            if (linkIA) ApiControl.ExcluirCss(linkIA, ltrCss.modalesDeFiltro.conFiltro);
            cerrar();
        });
        modalOverlay.onclick = (e) => { if (e.target === modalOverlay) cerrar(); };
        setTimeout(() => txtInput.focus(), 100);
    }

    export function LanzarPregunta(crud: Crud.CrudMnt, texto: string): void {
        crud.Pregunta = texto;
        crud.NuevaConversacion = !(document.getElementById('chk-nueva-pregunta') as HTMLInputElement).checked;
        crud.CargarGrid().then(() => {
            const linkIA = document.getElementById('acceso-ia-preguntame');
            if (linkIA) ApiControl.IncluirCss(linkIA, ltrCss.modalesDeFiltro.conFiltro);
        });
    }

    // ─── Conteo IA desde el Panel de Control ────────────────────────────────

    const _ClaveDiccionarioDeFiltros = 'panelDeControl.diccionarioDeFiltros';

    function _generarGuid(): string {
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, c => {
            const r = Math.random() * 16 | 0;
            return (c === 'x' ? r : (r & 0x3 | 0x8)).toString(16);
        });
    }

    function _guardarFiltrosConGuid(filtrosJson: string): string {
        const guid = _generarGuid();
        const diccionario: Record<string, string> = JSON.parse(localStorage.getItem(_ClaveDiccionarioDeFiltros) ?? '{}');
        diccionario[guid] = filtrosJson;
        localStorage.setItem(_ClaveDiccionarioDeFiltros, JSON.stringify(diccionario));
        return guid;
    }

    export function LeerFiltrosPorGuid(guid: string): string | null {
        const diccionario: Record<string, string> = JSON.parse(localStorage.getItem(_ClaveDiccionarioDeFiltros) ?? '{}');
        return diccionario[guid] ?? null;
    }

    export function InyectarConteoIA(negocioEnum: string, negocioNombre: string, btn: HTMLButtonElement, urlBase?: string): void {
        btn.dataset.negocio = negocioEnum;
        btn.addEventListener('click', () => AbrirModalConteo(negocioEnum, negocioNombre, urlBase));
    }

    export function AbrirModalConteo(negocioEnum: string, negocioNombre: string, urlBase?: string): void {
        const overlay = document.createElement('div');
        overlay.className = 'grafica-conteo-overlay';

        // GUID único por instancia de modal — identifica la sesión de conversación
        const sessionGuid = 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, c => {
            const r = Math.random() * 16 | 0;
            return (c === 'x' ? r : (r & 0x3 | 0x8)).toString(16);
        });

        // URL para "Consultar respuesta": misma vista pero con origen=paneldecontrol
        const urlConsultar = urlBase
            ? urlBase.replace(/,\s*null\s*,/, ",'origen=paneldecontrol',")
            : null;

        overlay.innerHTML = `
            <div class="grafica-conteo-modal">
                <div class="grafica-conteo-cabecera">
                    <div class="grafica-conteo-titulo">Pregunta sobre ${negocioNombre}</div>
                    <a id="gc-link-consultar" class="grafica-conteo-consultar" href="#" style="display:none">Consultar respuesta</a>
                </div>
                <textarea id="gc-pregunta" class="grafica-conteo-pregunta"
                    placeholder="Escribe tu pregunta, p.ej.: ¿cuántos ${negocioNombre} hay por tipo?" rows="5"></textarea>
                <div id="gc-area-respuesta" style="display:none">
                    <div class="grafica-conteo-respuesta-cabecera">
                        <a id="gc-link-toggle-filtro" class="grafica-conteo-consultar" href="#">Mostrar filtro</a>
                    </div>
                    <div id="gc-respuesta" class="grafica-conteo-respuesta"></div>
                </div>
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

        const txtPregunta     = overlay.querySelector('#gc-pregunta')           as HTMLTextAreaElement;
        const areaRespuesta   = overlay.querySelector('#gc-area-respuesta')     as HTMLDivElement;
        const divRespuesta    = overlay.querySelector('#gc-respuesta')           as HTMLDivElement;
        const btnPreguntar    = overlay.querySelector('#gc-btn-preguntar')       as HTMLButtonElement;
        const btnCerrar       = overlay.querySelector('#gc-btn-cerrar')         as HTMLButtonElement;
        const chkMismaConv    = overlay.querySelector('#gc-chk-misma-conv')    as HTMLInputElement;
        const spanSegundero   = overlay.querySelector('#gc-segundero')          as HTMLSpanElement;
        const spanSeg         = overlay.querySelector('#gc-seg')                as HTMLSpanElement;
        const linkConsultar   = overlay.querySelector('#gc-link-consultar')     as HTMLAnchorElement;
        const linkToggleFiltro = overlay.querySelector('#gc-link-toggle-filtro') as HTMLAnchorElement;

        ApiControl.IncluirCss(btnCerrar,    'boton-modal');
        ApiControl.IncluirCss(btnCerrar,    'btn');
        ApiControl.IncluirCss(btnCerrar,    'btn-secondary');
        ApiControl.IncluirCss(btnPreguntar, 'boton-modal');
        ApiControl.IncluirCss(btnPreguntar, 'btn');
        ApiControl.IncluirCss(btnPreguntar, 'btn-primary');
        ApiControl.IncluirCss(btnPreguntar, 'boton-por-defecto');

        const cerrar = () => {
            overlay.classList.remove('fade-in');
            setTimeout(() => overlay.remove(), 280);
        };

        btnCerrar.addEventListener('click', cerrar);

        let _textoRespuesta = '';
        let _filtrosRespuesta: string | null = null;
        let _mostrando: 'respuesta' | 'filtro' = 'respuesta';

        linkToggleFiltro.addEventListener('click', (e) => {
            e.preventDefault();
            if (_mostrando === 'respuesta' && _filtrosRespuesta) {
                try {
                    divRespuesta.textContent = JSON.stringify(JSON.parse(_filtrosRespuesta), null, 2);
                } catch {
                    divRespuesta.textContent = _filtrosRespuesta;
                }
                linkToggleFiltro.textContent = 'Mostrar respuesta';
                _mostrando = 'filtro';
            } else {
                divRespuesta.textContent = _textoRespuesta;
                linkToggleFiltro.textContent = 'Mostrar filtro';
                _mostrando = 'respuesta';
            }
        });

        btnPreguntar.addEventListener('click', () => {
            const pregunta = txtPregunta.value.trim();
            if (!pregunta) {
                txtPregunta.style.borderColor = 'red';
                txtPregunta.focus();
                return;
            }
            txtPregunta.style.borderColor = '';
            btnPreguntar.disabled = true;
            areaRespuesta.style.display = 'none';
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

            LanzarPreguntaDeConteo(negocioEnum, pregunta, sessionGuid, continuarConversacion, (respuesta, filtrosIa) => {
                clearInterval(timer);
                spanSegundero.style.display = 'none';
                btnPreguntar.disabled = false;

                _textoRespuesta = respuesta;
                _filtrosRespuesta = filtrosIa;
                _mostrando = 'respuesta';
                divRespuesta.textContent = respuesta;
                linkToggleFiltro.textContent = 'Mostrar filtro';
                linkToggleFiltro.style.display = filtrosIa ? '' : 'none';
                areaRespuesta.style.display = '';

                // Mostrar "Consultar respuesta" si la tarjeta tiene vista y la IA devolvió filtros
                if (urlConsultar && linkConsultar && filtrosIa) {
                    const guid = _guardarFiltrosConGuid(filtrosIa);
                    const urlConGuid = urlConsultar.replace(
                        "'origen=paneldecontrol'",
                        `'origen=paneldecontrol&${Ajax.Param.guidDeFiltros}=${guid}'`
                    );
                    linkConsultar.setAttribute('onclick', `${urlConGuid}; return false;`);
                    linkConsultar.style.display = '';
                }
            });
        });

        setTimeout(() => txtPregunta.focus(), 80);
    }

    export function LanzarPreguntaDeConteo(
        negocioEnum: string,
        pregunta: string,
        sessionGuid: string,
        continuarConversacion: boolean,
        onResultado: (respuesta: string, filtrosIa: string | null) => void
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
                const datos = resultado.datos;
                const texto    = datos?.texto   ?? datos ?? '(sin respuesta)';
                const filtros  = datos?.filtrosIa ?? null;
                onResultado(texto, filtros);
            } else {
                onResultado(`Error: ${resultado?.mensaje ?? 'respuesta desconocida'}`, null);
            }
        })
        .catch(err => onResultado(`Error de red: ${err}`, null));
    }

}
