namespace ApiPreguntasIa {

    export function InyectarAccesoIA(crud: Crud.CrudMnt): void {
        const contenedor = document.querySelector('.' + ltrCss.crud.filtro.expansor);
        if (contenedor && !document.getElementById('acceso-ia-preguntame')) {
            const linkIA = document.createElement('a');
            linkIA.id = 'acceso-ia-preguntame';
            linkIA.href = 'javascript:void(0);';
            linkIA.innerText = 'Pregúntame';
            linkIA.title = 'Pregúntame';
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

        const hayHistorial = crud.Preguntas.length > 0;
        modalOverlay.innerHTML = ltrHtml.Modales.PreguntasIa
            .replace('[htmlHistorial]', HtmlDelHistorial(crud))
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
            linkRespuesta.style.display = ltrStyle.display.none;
            const respuestaOriginal = (crud.Preguntas.find(p => p.pregunta === txtInput.value.trim()) ?? crud.UltimaPregunta)?.respuesta;
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

        const enlazarHistorial = () => {
            const itemsHistorial = modalOverlay.querySelectorAll('.' + ltrCss.crud.modal.LineaDelhistorialIa);
            itemsHistorial.forEach(item => {
                item.addEventListener('click', (e) => {
                    txtInput.value = (e.currentTarget as HTMLElement).innerText;
                    txtInput.focus();
                });
            });
        };
        enlazarHistorial();

        // Repinta las consultas recientes tras cada pregunta, sin cerrar el modal
        const repintarHistorial = () => {
            modalOverlay.querySelector('#seccion-historial-ia')?.remove();
            const html = HtmlDelHistorial(crud);
            if (html) vistaPregunta.querySelector('.' + ltrCss.crud.modal.cuerpo).insertAdjacentHTML('afterbegin', html);
            enlazarHistorial();
        };

        setTimeout(() => ApiControl.IncluirCss(modalOverlay, ltrCss.Modal.FadeIn), 10);
        const cerrar = () => {
            ApiControl.ExcluirCss(modalOverlay, ltrCss.Modal.FadeIn);
            setTimeout(() => modalOverlay.remove(), 300);
        };

        // El modal permanece abierto mientras dure la conversación: se aparta a una esquina
        // y deja ver (y usar) el grid con el resultado de cada pregunta
        let hayPreguntasEnLaConversacion = false;
        btnPreguntar?.addEventListener('click', () => {
            const pregunta = txtInput.value.trim();
            if (!pregunta) {
                txtInput.focus();
                txtInput.style.borderColor = 'red';
                return;
            }
            txtInput.style.borderColor = '';
            btnPreguntar.disabled = true;
            txtInput.disabled = true;
            btnPreguntar.innerText = 'Pensando...';
            ApiControl.IncluirCss(modalOverlay, ltrCss.crud.modal.conversandoIa);
            LanzarPregunta(crud, pregunta)
                .then(() => {
                    hayPreguntasEnLaConversacion = true;
                    txtInput.value = '';
                    (modalOverlay.querySelector('#chk-nueva-pregunta') as HTMLInputElement).checked = true;
                    repintarHistorial();
                })
                .catch(() => { /* el error ya lo muestra el grid */ })
                .finally(() => {
                    btnPreguntar.disabled = false;
                    txtInput.disabled = false;
                    btnPreguntar.innerText = 'Preguntar';
                    txtInput.focus();
                });
        });

        // Resetear: quita los filtros de la IA, recarga el grid con los filtros de pantalla
        // y la siguiente pregunta empieza una conversación nueva; el modal sigue abierto
        btnCerrar?.addEventListener('click', () => {
            crud.Pregunta = null;
            crud.NuevaConversacion = true;
            (modalOverlay.querySelector('#chk-nueva-pregunta') as HTMLInputElement).checked = false;
            hayPreguntasEnLaConversacion = false;
            txtInput.value = '';
            txtInput.style.borderColor = '';
            crud.CargarGrid().catch(() => { /* el error ya lo muestra el grid */ });
            const linkIA = document.getElementById('acceso-ia-preguntame');
            if (linkIA) ApiControl.ExcluirCss(linkIA, ltrCss.modalesDeFiltro.conFiltro);
            txtInput.focus();
        });

        // La × sólo cierra: se mantiene el filtro de la última respuesta
        (modalOverlay.querySelector('#btn-cerrar-x-ia') as HTMLElement)?.addEventListener('click', () => cerrar());
        modalOverlay.onclick = (e) => { if (e.target === modalOverlay && !hayPreguntasEnLaConversacion) cerrar(); };
        setTimeout(() => txtInput.focus(), 100);
    }

    function HtmlDelHistorial(crud: Crud.CrudMnt): string {
        const historial = crud.Preguntas;
        if (historial.length === 0) return '';
        const recientes = historial.slice(-5).reverse();
        return ltrHtml.Divs.PreguntasIa.replace('[PreguntasIa]', recientes.map(q => `<div class='${ltrCss.crud.modal.LineaDelhistorialIa}' title='${q.pregunta.replace(/"/g, '&quot;')}'>${q.pregunta}</div>`).join(''));
    }

    // Robot verde de "preguntar a la IA" (fichas del panel de control y modal del manual de una vista)
    export const SvgRobot = `<svg width="14" height="14" viewBox="0 0 16 16" fill="none"
        stroke="currentColor" stroke-width="1.3" stroke-linecap="round" stroke-linejoin="round">
        <line x1="5" y1="1.8" x2="6.2" y2="4"/>
        <line x1="11" y1="1.8" x2="9.8" y2="4"/>
        <circle cx="5" cy="1.6" r="0.7" fill="currentColor" stroke="none"/>
        <circle cx="11" cy="1.6" r="0.7" fill="currentColor" stroke="none"/>
        <rect x="2.5" y="4" width="11" height="9.5" rx="2.2"/>
        <circle cx="6" cy="8" r="1.1" fill="currentColor" stroke="none"/>
        <circle cx="10" cy="8" r="1.1" fill="currentColor" stroke="none"/>
        <line x1="6" y1="11.2" x2="10" y2="11.2"/>
    </svg>`;

    // ─── ¿Cómo se hace...? sobre el manual de una vista ─────────────────────
    // Se abre con el robot de la modal del manual (ApiDeArchivos.MostrarManualDeLaVista). Mismo diálogo que el de las fichas del
    // panel de control (clases grafica-conteo-*), pero la pregunta va a Archivos/epComoSeHace, que le pasa a la IA del usuario el
    // manual de la vista (lo monta el servidor con el controlador y la vista) junto con la pregunta.
    export function AbrirModalComoSeHace(controlador: string, vista: string, titulo: string): void {
        const css = ltrCss.PanelDeControl.Conteo;
        const ids = ltrIdsHtml.ComoSeHace;
        const overlay = document.createElement('div') as HTMLDivElement;
        ApiControl.IncluirCss(overlay, css.Overlay);
        ApiControl.IncluirCss(overlay, css.SobreModal);
        overlay.innerHTML = ltrHtml.Modales.ComoSeHace;
        (overlay.querySelector(`.${css.Titulo}`) as HTMLDivElement).textContent = `¿Cómo se hace...? ${titulo}`;
        document.body.appendChild(overlay);
        setTimeout(() => ApiControl.IncluirCss(overlay, ltrCss.Modal.FadeIn), 10);

        const txtPregunta = overlay.querySelector(`#${ids.Pregunta}`) as HTMLTextAreaElement;
        const areaRespuesta = overlay.querySelector(`#${ids.AreaRespuesta}`) as HTMLDivElement;
        const divRespuesta = overlay.querySelector(`#${ids.Respuesta}`) as HTMLDivElement;
        const btnPreguntar = overlay.querySelector(`#${ids.Preguntar}`) as HTMLButtonElement;
        const btnCerrar = overlay.querySelector(`#${ids.Cerrar}`) as HTMLButtonElement;
        const spanSegundero = overlay.querySelector(`#${ids.Segundero}`) as HTMLSpanElement;
        const spanSeg = overlay.querySelector(`#${ids.Segundos}`) as HTMLSpanElement;

        ApiControl.IncluirCss(btnCerrar, ltrCss.Modal.BotonSecundario);
        ApiControl.IncluirCss(btnPreguntar, ltrCss.Modal.BotonPrincipal);

        const cerrar = () => {
            window.removeEventListener('keydown', alPulsarEscape, true);
            ApiControl.ExcluirCss(overlay, ltrCss.Modal.FadeIn);
            setTimeout(() => overlay.remove(), 280);
        };
        // el Escape cierra este diálogo y no la modal del manual que hay debajo: se escucha en window, que en captura va antes que document
        const alPulsarEscape = (e: KeyboardEvent) => {
            if (e.key !== 'Escape') return;
            e.preventDefault();
            e.stopImmediatePropagation();
            cerrar();
        };
        window.addEventListener('keydown', alPulsarEscape, true);
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
            areaRespuesta.style.display = ltrStyle.display.none;

            let segundos = 0;
            spanSeg.textContent = '0';
            spanSegundero.style.display = '';
            const timer = setInterval(() => spanSeg.textContent = String(++segundos), 1000);

            const datos = new FormData();
            datos.append(literal.controlador, controlador);
            datos.append(literal.vista, vista);
            datos.append(Ajax.Param.Titulo, titulo);
            datos.append(Ajax.Param.Pregunta, pregunta);
            fetch(`/${Ajax.Archivos.controlador}/${Ajax.Archivos.accion.ComoSeHace}`, { method: 'POST', body: datos })
                .then(r => r.json())
                .then((resultado: any) => {
                    const ok = resultado?.estado === 'Ok' || resultado?.estado === 0;
                    divRespuesta.textContent = ok
                        ? (resultado.datos ?? '(sin respuesta)')
                        : `Error: ${resultado?.mensaje ?? 'respuesta desconocida'}`;
                })
                .catch(err => divRespuesta.textContent = `Error de red: ${err}`)
                .finally(() => {
                    clearInterval(timer);
                    spanSegundero.style.display = ltrStyle.display.none;
                    btnPreguntar.disabled = false;
                    areaRespuesta.style.display = '';
                });
        });

        setTimeout(() => txtPregunta.focus(), 80);
    }

    export function LanzarPregunta(crud: Crud.CrudMnt, texto: string): Promise<boolean> {
        crud.Pregunta = texto;
        crud.NuevaConversacion = !(document.getElementById('chk-nueva-pregunta') as HTMLInputElement).checked;
        return crud.CargarGrid().then((resultado) => {
            const linkIA = document.getElementById('acceso-ia-preguntame');
            if (linkIA) ApiControl.IncluirCss(linkIA, ltrCss.modalesDeFiltro.conFiltro);
            return resultado;
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
        const ids = ltrIdsHtml.Conteo;
        const overlay = document.createElement('div') as HTMLDivElement;
        ApiControl.IncluirCss(overlay, ltrCss.PanelDeControl.Conteo.Overlay);

        // GUID único por instancia de modal — identifica la sesión de conversación
        const sessionGuid = 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, c => {
            const r = Math.random() * 16 | 0;
            return (c === 'x' ? r : (r & 0x3 | 0x8)).toString(16);
        });

        // URL para "Consultar respuesta": misma vista pero con origen=paneldecontrol
        const urlConsultar = urlBase
            ? urlBase.replace(/,\s*null\s*,/, ",'origen=paneldecontrol',")
            : null;

        overlay.innerHTML = ltrHtml.Modales.Conteo.replace(/\[negocio\]/g, EscapeHtml(negocioNombre ?? ''));

        document.body.appendChild(overlay);
        setTimeout(() => ApiControl.IncluirCss(overlay, ltrCss.Modal.FadeIn), 10);

        const txtPregunta = overlay.querySelector(`#${ids.Pregunta}`) as HTMLTextAreaElement;
        const areaRespuesta = overlay.querySelector(`#${ids.AreaRespuesta}`) as HTMLDivElement;
        const divRespuesta = overlay.querySelector(`#${ids.Respuesta}`) as HTMLDivElement;
        const btnPreguntar = overlay.querySelector(`#${ids.Preguntar}`) as HTMLButtonElement;
        const btnCerrar = overlay.querySelector(`#${ids.Cerrar}`) as HTMLButtonElement;
        const chkMismaConv = overlay.querySelector(`#${ids.MismaConversacion}`) as HTMLInputElement;
        const spanSegundero = overlay.querySelector(`#${ids.Segundero}`) as HTMLSpanElement;
        const spanSeg = overlay.querySelector(`#${ids.Segundos}`) as HTMLSpanElement;
        const linkConsultar = overlay.querySelector(`#${ids.Consultar}`) as HTMLAnchorElement;
        const linkToggleFiltro = overlay.querySelector(`#${ids.MostrarFiltro}`) as HTMLAnchorElement;

        ApiControl.IncluirCss(btnCerrar, ltrCss.Modal.BotonSecundario);
        ApiControl.IncluirCss(btnPreguntar, ltrCss.Modal.BotonPrincipal);

        const cerrar = () => {
            ApiControl.ExcluirCss(overlay, ltrCss.Modal.FadeIn);
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
            areaRespuesta.style.display = ltrStyle.display.none;
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
                spanSegundero.style.display = ltrStyle.display.none;
                btnPreguntar.disabled = false;

                _textoRespuesta = respuesta;
                _filtrosRespuesta = filtrosIa;
                _mostrando = 'respuesta';
                divRespuesta.textContent = respuesta;
                linkToggleFiltro.textContent = 'Mostrar filtro';
                linkToggleFiltro.style.display = filtrosIa ? '' : ltrStyle.display.none;
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
