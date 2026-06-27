namespace ApiPreguntasIa {

    export function InyectarAccesoIA(crud: Crud.CrudMnt): void {
        const contenedor = document.querySelector('.div-mnt-filtro-expansor');
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

}
