// ─────────────────────────────────────────────────────────────────────────────
//  ApiFlujo.ts
//  Namespace Flujo — visualizador interactivo del flujo de estados.
//
//  MostrarFlujoDeEstado(navegador)
//    · Llamado desde el onclick del <a> navegador cuando VistaDondeNavegar == "CrudDeEstados"
//    · Obtiene el flujo (epObtenerFlujo) y las posiciones guardadas
//      (epLeerPosicionesDeEstados) y luego pinta la modal.
//
//  MostrarModalDeFlujo(nombre, datos, negocio, idInicial, posGuardadas)
//    · SVG interactivo: nodos arrastrables, flechas azul/rojo, hover, tooltip
//    · Botón 💾 graba la disposición actual llamando a epGrabarDisposicionDeEstados
// ─────────────────────────────────────────────────────────────────────────────

namespace Flujo {

    // ── Icono disquete (Bootstrap Icons) ─────────────────────────────────────
    const _iconoDisquete =
        `<svg xmlns="http://www.w3.org/2000/svg" width="17" height="17" fill="currentColor" viewBox="0 0 16 16">
            <path d="M1.5 0A1.5 1.5 0 0 0 0 1.5v13A1.5 1.5 0 0 0 1.5 16h13a1.5 1.5 0 0 0 1.5-1.5V3.914
                     a1.5 1.5 0 0 0-.44-1.06L13.146.439A1.5 1.5 0 0 0 12.086 0H1.5zm1.5 5V1h8v4
                     a.5.5 0 0 1-.5.5h-7A.5.5 0 0 1 3 5z"/>
            <path d="M3 13h10v-4H3v4z"/>
        </svg>`;

    // Tipo que devuelve epLeerPosicionesDeEstados
    type TPosicionGuardada = { idEstado: number, posX: number, posY: number };

    // ── Helpers de fetch directo (los ep usan negocio en query-string, no en parametrosJson) ──

    function _leerPosicionesGuardadas(negocio: string): Promise<Array<TPosicionGuardada>> {
        const url = `/${ltrControladores.Negocio.Estados}/${Ajax.EndPoint.Negocio.Estados.LeerPosiciones}?negocio=${encodeURIComponent(negocio)}`;
        return fetch(url, { credentials: 'same-origin' })
            .then(r => r.json())
            .then(d => (d.datos || []) as Array<TPosicionGuardada>)
            .catch(() => [] as Array<TPosicionGuardada>);
    }

    function _grabarPosiciones(
        negocio:   string,
        posiciones: Array<{ idEstado: number, posX: number, posY: number }>
    ): Promise<void> {
        const url  = `/${ltrControladores.Negocio.Estados}/${Ajax.EndPoint.Negocio.Estados.GrabarDisposicion}?negocio=${encodeURIComponent(negocio)}`;
        // El cuerpo sigue el mismo patrón que EjecutarPeticionPost:
        // encodeURIComponent(JSON.stringify([{parametro, valor}]))
        const body = encodeURIComponent(JSON.stringify([
            { parametro: 'datospeticion', valor: JSON.stringify(posiciones) }
        ]));
        return fetch(url, { method: 'POST', credentials: 'same-origin', body })
            .then(r => r.json())
            .then(d => {
                if (d.estado === 'Ok') MensajesSe.Info('Disposición de estados guardada');
                else                   MensajesSe.Error('Error', d.mensaje || 'Error al guardar');
            })
            .catch(() => MensajesSe.Error('Error', 'No se pudo guardar la disposición'));
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  MostrarFlujoDeEstado
    // ─────────────────────────────────────────────────────────────────────────
    export function MostrarFlujoDeEstado(navegador: HTMLElement): boolean {
        const a = navegador as HTMLAnchorElement;

        // id del estado: del href que DefinirNavegador (TS) pone como ?id=N
        let idEstado = 0;
        try {
            const url = new URL(a.href, window.location.origin);
            idEstado = Numero(url.searchParams.get('id') || '0');
        } catch (_) { /* href aún es '#' */ }

        if (idEstado <= 0) {
            MensajesSe.Info('Seleccione un estado para ver su flujo');
            return false;
        }

        const input        = a.parentElement?.querySelector('input') as HTMLInputElement | null;
        const nombreEstado = input?.value || '';
        const negocio      = a.getAttribute('negocio') || '';

        // 1. Obtener el flujo
        const parFlujo: Array<Parametro> = [
            new Parametro(Ajax.Param.enumNegocio, negocio),
            new Parametro(Ajax.Param.idEstado,    idEstado)
        ];

        ApiDePeticiones.EjecutarPeticion(
            navegador,
            ltrControladores.Negocio.Estados,
            Ajax.EndPoint.Negocio.Estados.ObtenerFlujo,
            parFlujo,
            new Array<Parametro>()
        ).then((pFlujo: ApiDeAjax.DescriptorAjax) => {
            const datosFlujo = pFlujo.resultado.datos;
            // 2. Cargar posiciones guardadas (fetch directo, negocio en query-string)
            _leerPosicionesGuardadas(negocio).then(posGuardadas =>
                MostrarModalDeFlujo(nombreEstado, datosFlujo, negocio, idEstado, posGuardadas)
            );
        }).catch((p: ApiDeAjax.DescriptorAjax) => ApiDePeticiones.EmitirError(p));

        return false;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  MostrarModalDeFlujo
    // ─────────────────────────────────────────────────────────────────────────
    export function MostrarModalDeFlujo(
        nombreEstado:    string,
        datos:           any,
        negocio:         string               = '',
        idEstadoInicial: number               = 0,
        posGuardadas:    Array<TPosicionGuardada> = []
    ): void {

        const estados:     Array<{ id: number, nombre: string }> = datos.estados     || [];
        const transiciones: Array<{ id: number, nombre: string, idOrigen: number, idDestino: number }> = datos.transiciones || [];

        // ── 1. BFS → niveles ──────────────────────────────────────────────────
        const nivelPorId: Map<number, number> = new Map();
        const idInicial = estados.length > 0 ? estados[0].id : 0;
        const cola: number[] = [idInicial];
        nivelPorId.set(idInicial, 0);
        let cab = 0;
        while (cab < cola.length) {
            const actual = cola[cab++];
            for (const t of transiciones.filter(t => t.idOrigen === actual)) {
                if (!nivelPorId.has(t.idDestino)) {
                    nivelPorId.set(t.idDestino, (nivelPorId.get(actual) || 0) + 1);
                    cola.push(t.idDestino);
                }
            }
        }
        estados.forEach(e => { if (!nivelPorId.has(e.id)) nivelPorId.set(e.id, 999); });

        // ── 2. Posiciones iniciales (calculadas) ──────────────────────────────
        const porNivel: Map<number, Array<{ id: number, nombre: string }>> = new Map();
        estados.forEach(e => {
            const n = nivelPorId.get(e.id) || 0;
            if (!porNivel.has(n)) porNivel.set(n, []);
            porNivel.get(n).push(e);
        });
        const niveles  = Array.from(porNivel.keys()).sort((a, b) => a - b);
        const radio    = 38, padH = 90, padV = 110, margen = 30;
        const posicionPorId: Map<number, { x: number, y: number }> = new Map();
        const anchoMax = Math.max(...niveles.map(n => (porNivel.get(n) || []).length));
        const svgAncho = Math.max(420, anchoMax * (radio * 2 + padH) + margen * 2);
        const svgAlto  = niveles.length * (radio * 2 + padV) + margen * 2;

        niveles.forEach(n => {
            const grupo    = porNivel.get(n) || [];
            const espacioH = radio * 2 + padH;
            const offsetX  = (svgAncho - grupo.length * espacioH) / 2 + radio + padH / 2;
            grupo.forEach((e, i) =>
                posicionPorId.set(e.id, {
                    x: offsetX + i * espacioH,
                    y: margen + n * (radio * 2 + padV) + radio
                })
            );
        });

        // ── 3. Aplicar posiciones guardadas (sobreescriben las calculadas) ────
        // El servidor devuelve {idEstado, posX, posY}
        for (const saved of posGuardadas) {
            if (posicionPorId.has(saved.idEstado))
                posicionPorId.set(saved.idEstado, { x: saved.posX, y: saved.posY });
        }

        // ── 4. SVG base ───────────────────────────────────────────────────────
        const ns  = 'http://www.w3.org/2000/svg';
        const svg = document.createElementNS(ns, 'svg') as SVGSVGElement;
        svg.setAttribute('width',  String(svgAncho));
        svg.setAttribute('height', String(svgAlto));
        (svg as SVGElement).style.userSelect = 'none';

        const defs = document.createElementNS(ns, 'defs');
        for (const [id, color] of [['flecha-av', '#4a86e8'], ['flecha-re', '#e05252']]) {
            const mk = document.createElementNS(ns, 'marker');
            mk.setAttribute('id', id); mk.setAttribute('markerWidth', '10');
            mk.setAttribute('markerHeight', '7'); mk.setAttribute('refX', '10');
            mk.setAttribute('refY', '3.5'); mk.setAttribute('orient', 'auto');
            const poly = document.createElementNS(ns, 'polygon');
            poly.setAttribute('points', '0 0, 10 3.5, 0 7'); poly.setAttribute('fill', color);
            mk.appendChild(poly); defs.appendChild(mk);
        }
        svg.appendChild(defs);

        // Círculos amarillos inicio/fin (hover)
        const hoverMarkers = document.createElementNS(ns, 'g');
        (hoverMarkers as SVGElement).style.display = 'none';
        hoverMarkers.setAttribute('pointer-events', 'none');
        const mkInicio = document.createElementNS(ns, 'circle');
        mkInicio.setAttribute('r', '5'); mkInicio.setAttribute('fill', '#ffd600');
        mkInicio.setAttribute('stroke', '#b8a000'); mkInicio.setAttribute('stroke-width', '1.2');
        const mkFin = document.createElementNS(ns, 'circle');
        mkFin.setAttribute('r', '5'); mkFin.setAttribute('fill', '#ffd600');
        mkFin.setAttribute('stroke', '#b8a000'); mkFin.setAttribute('stroke-width', '1.2');
        hoverMarkers.appendChild(mkInicio); hoverMarkers.appendChild(mkFin);

        // Tooltip (click en flecha)
        const tooltip = document.createElementNS(ns, 'g');
        (tooltip as SVGElement).style.display = 'none';
        tooltip.setAttribute('pointer-events', 'none');
        const ttRect = document.createElementNS(ns, 'rect');
        ttRect.setAttribute('rx', '4'); ttRect.setAttribute('ry', '4');
        ttRect.setAttribute('fill', '#333'); ttRect.setAttribute('opacity', '0.88');
        const ttText = document.createElementNS(ns, 'text');
        ttText.setAttribute('fill', '#fff'); ttText.setAttribute('font-size', '12');
        ttText.setAttribute('font-family', 'sans-serif');
        ttText.setAttribute('dominant-baseline', 'middle');
        tooltip.appendChild(ttRect); tooltip.appendChild(ttText);

        // ── 5. Flechas ────────────────────────────────────────────────────────
        const DESP = 6;
        const arrowsPorNodo = new Map<number, Array<() => void>>();
        const registrarArrow = (idNodo: number, fn: () => void) => {
            if (!arrowsPorNodo.has(idNodo)) arrowsPorNodo.set(idNodo, []);
            arrowsPorNodo.get(idNodo)!.push(fn);
        };

        const calcPuntos = (t: { idOrigen: number, idDestino: number }) => {
            const desde = posicionPorId.get(t.idOrigen)!;
            const hasta  = posicionPorId.get(t.idDestino)!;
            if (!desde || !hasta) return null;
            const dx = hasta.x - desde.x, dy = hasta.y - desde.y;
            const dist = Math.sqrt(dx * dx + dy * dy);
            if (dist < 1) return null;
            const ux = dx / dist, uy = dy / dist;
            const nOrigen  = nivelPorId.get(t.idOrigen)  || 0;
            const nDestino = nivelPorId.get(t.idDestino) || 0;
            const esAvance = nDestino > nOrigen;
            const nodoBajo = esAvance ? desde : hasta;
            const nodoAlto = esAvance ? hasta : desde;
            const cDx = nodoAlto.x - nodoBajo.x, cDy = nodoAlto.y - nodoBajo.y;
            const cDist = Math.sqrt(cDx * cDx + cDy * cDy) || 1;
            const cux = cDx / cDist, cuy = cDy / cDist;
            const signPerp = esAvance ? 1 : -1;
            const ox = signPerp * cuy * DESP, oy = signPerp * (-cux) * DESP;
            return {
                x1: desde.x + ux * radio        + ox,
                y1: desde.y + uy * radio        + oy,
                x2: hasta.x  - ux * (radio + 8) + ox,
                y2: hasta.y  - uy * (radio + 8) + oy
            };
        };

        let flechaConHover: (() => void) | null = null;

        for (const t of transiciones) {
            if (!posicionPorId.get(t.idOrigen) || !posicionPorId.get(t.idDestino)) continue;
            const nOrigen  = nivelPorId.get(t.idOrigen)  || 0;
            const nDestino = nivelPorId.get(t.idDestino) || 0;
            const esAvance = nDestino > nOrigen;
            const color    = esAvance ? '#4a86e8' : '#e05252';
            const marcador = esAvance ? 'url(#flecha-av)' : 'url(#flecha-re)';

            const pts = calcPuntos(t); if (!pts) continue;
            let { x1, y1, x2, y2 } = pts;

            const g    = document.createElementNS(ns, 'g');
            (g as SVGElement).style.cursor = 'pointer';
            const hit   = document.createElementNS(ns, 'line');
            const linea = document.createElementNS(ns, 'line');

            const aplicarPuntos = (p: typeof pts) => {
                for (const el of [hit, linea]) {
                    el.setAttribute('x1', p.x1.toFixed(1)); el.setAttribute('y1', p.y1.toFixed(1));
                    el.setAttribute('x2', p.x2.toFixed(1)); el.setAttribute('y2', p.y2.toFixed(1));
                }
            };
            aplicarPuntos(pts);
            hit.setAttribute('stroke', 'transparent'); hit.setAttribute('stroke-width', '14');
            linea.setAttribute('stroke', color); linea.setAttribute('stroke-width', '1.8');
            linea.setAttribute('stroke-opacity', '0.25'); linea.setAttribute('marker-end', marcador);
            (linea as SVGElement).style.transition = 'stroke-opacity .12s';
            g.appendChild(hit); g.appendChild(linea); svg.appendChild(g);

            const updateArrow = () => {
                const p = calcPuntos(t); if (!p) return;
                x1 = p.x1; y1 = p.y1; x2 = p.x2; y2 = p.y2;
                aplicarPuntos(p);
                if (flechaConHover === updateArrow) {
                    mkInicio.setAttribute('cx', x1.toFixed(1)); mkInicio.setAttribute('cy', y1.toFixed(1));
                    mkFin.setAttribute('cx',    x2.toFixed(1)); mkFin.setAttribute('cy',    y2.toFixed(1));
                }
            };
            registrarArrow(t.idOrigen,  updateArrow);
            registrarArrow(t.idDestino, updateArrow);

            g.addEventListener('mouseenter', () => {
                if (arrastrando) return;
                linea.setAttribute('stroke-opacity', '1');
                mkInicio.setAttribute('cx', x1.toFixed(1)); mkInicio.setAttribute('cy', y1.toFixed(1));
                mkFin.setAttribute('cx',    x2.toFixed(1)); mkFin.setAttribute('cy',    y2.toFixed(1));
                (hoverMarkers as SVGElement).style.display = '';
                flechaConHover = updateArrow;
            });
            g.addEventListener('mouseleave', () => {
                linea.setAttribute('stroke-opacity', '0.25');
                (hoverMarkers as SVGElement).style.display = 'none';
                (tooltip     as SVGElement).style.display = 'none';
                flechaConHover = null;
            });
            g.addEventListener('click', (ev: MouseEvent) => {
                ev.stopPropagation();
                const rect = svg.getBoundingClientRect();
                ttText.textContent = t.nombre;
                (tooltip as SVGElement).style.display = '';
                tooltip.setAttribute('transform',
                    `translate(${(ev.clientX - rect.left + 10).toFixed(0)},${(ev.clientY - rect.top - 20).toFixed(0)})`);
                const bb = (ttText as SVGTextElement).getBBox();
                ttRect.setAttribute('x',      (bb.x - 6).toFixed(1));
                ttRect.setAttribute('y',      (bb.y - 4).toFixed(1));
                ttRect.setAttribute('width',  (bb.width  + 12).toFixed(1));
                ttRect.setAttribute('height', (bb.height +  8).toFixed(1));
            });
        }

        // ── 6. Nodos arrastrables ─────────────────────────────────────────────
        let arrastrando: {
            id: number, startX: number, startY: number, startMX: number, startMY: number,
            circ: SVGCircleElement, textos: Array<{ el: SVGTextElement, offsetY: number }>
        } | null = null;

        for (const e of estados) {
            const pos = posicionPorId.get(e.id); if (!pos) continue;

            const palabras = e.nombre.split(' ');
            const lineas: string[] = [];
            let actual = '';
            for (const p of palabras) {
                if ((actual + ' ' + p).trim().length > 14) { if (actual) lineas.push(actual); actual = p; }
                else { actual = (actual + ' ' + p).trim(); }
            }
            if (actual) lineas.push(actual);

            const circ = document.createElementNS(ns, 'circle') as SVGCircleElement;
            circ.setAttribute('cx', String(pos.x)); circ.setAttribute('cy', String(pos.y));
            circ.setAttribute('r', String(radio));
            circ.setAttribute('fill', '#e8f0fe'); circ.setAttribute('stroke', '#4a86e8');
            circ.setAttribute('stroke-width', '2');
            (circ as SVGElement).style.cursor = 'grab';
            svg.appendChild(circ);

            const alturaTexto = lineas.length * 14;
            const textos = lineas.map((l, i) => {
                const offsetY = -alturaTexto / 2 + 14 + i * 14;
                const txt = document.createElementNS(ns, 'text') as SVGTextElement;
                txt.setAttribute('x', String(pos.x));
                txt.setAttribute('y', (pos.y + offsetY).toFixed(1));
                txt.setAttribute('text-anchor', 'middle'); txt.setAttribute('font-size', '11');
                txt.setAttribute('fill', '#1a1a2e'); txt.setAttribute('font-family', 'sans-serif');
                txt.setAttribute('pointer-events', 'none');
                txt.textContent = l;
                svg.appendChild(txt);
                return { el: txt, offsetY };
            });

            circ.addEventListener('mousedown', (ev: MouseEvent) => {
                ev.preventDefault(); ev.stopPropagation();
                (tooltip      as SVGElement).style.display = 'none';
                (hoverMarkers as SVGElement).style.display = 'none';
                arrastrando = {
                    id: e.id,
                    startX: posicionPorId.get(e.id)!.x,
                    startY: posicionPorId.get(e.id)!.y,
                    startMX: ev.clientX, startMY: ev.clientY,
                    circ, textos
                };
                (circ as SVGElement).style.cursor = 'grabbing';
            });
        }

        svg.addEventListener('mousemove', (ev: MouseEvent) => {
            if (!arrastrando) return;
            const newX = arrastrando.startX + (ev.clientX - arrastrando.startMX);
            const newY = arrastrando.startY + (ev.clientY - arrastrando.startMY);
            posicionPorId.set(arrastrando.id, { x: newX, y: newY });
            arrastrando.circ.setAttribute('cx', String(newX));
            arrastrando.circ.setAttribute('cy', String(newY));
            arrastrando.textos.forEach(({ el, offsetY }) => {
                el.setAttribute('x', String(newX));
                el.setAttribute('y', (newY + offsetY).toFixed(1));
            });
            (arrowsPorNodo.get(arrastrando.id) || []).forEach(fn => fn());
        });

        const soltarNodo = () => {
            if (arrastrando) { (arrastrando.circ as SVGElement).style.cursor = 'grab'; arrastrando = null; }
        };
        svg.addEventListener('mouseup',    soltarNodo);
        svg.addEventListener('mouseleave', soltarNodo);

        svg.appendChild(hoverMarkers);
        svg.appendChild(tooltip);
        svg.addEventListener('click', () => { (tooltip as SVGElement).style.display = 'none'; });

        // ── 7. Montar la modal ────────────────────────────────────────────────
        const idModal = 'modal-flujo-estados';
        let modal = document.getElementById(idModal) as HTMLDivElement;
        if (modal) modal.remove();

        modal = document.createElement('div');
        modal.id = idModal;
        modal.style.cssText =
            'position:fixed;top:0;left:0;width:100%;height:100%;' +
            'background:rgba(0,0,0,.45);z-index:9999;display:flex;' +
            'align-items:center;justify-content:center';

        const caja = document.createElement('div');
        caja.style.cssText =
            'background:#fff;border-radius:8px;padding:20px;' +
            'max-width:90vw;max-height:90vh;overflow:auto;' +
            'box-shadow:0 4px 24px rgba(0,0,0,.3)';

        // Cabecera: título + botón guardar + botón cerrar
        const cabecera = document.createElement('div');
        cabecera.style.cssText =
            'display:flex;justify-content:space-between;align-items:center;margin-bottom:12px';

        const titulo = document.createElement('h5');
        titulo.style.cssText = 'margin:0;font-family:sans-serif';
        titulo.textContent = `Flujo desde: ${nombreEstado}`;

        // Botones a la derecha (guardar + cerrar)
        const botones = document.createElement('div');
        botones.style.cssText = 'display:flex;align-items:center;gap:6px';

        // Botón guardar (sólo visible si se conoce negocio e idEstadoInicial)
        if (negocio && idEstadoInicial > 0) {
            const btnGuardar = document.createElement('button');
            btnGuardar.title = 'Grabar disposición de estados';
            btnGuardar.style.cssText =
                'border:none;background:none;cursor:pointer;padding:2px 4px;' +
                'color:#555;display:flex;align-items:center;' +
                'border-radius:4px;transition:color .15s,background .15s';
            btnGuardar.innerHTML = _iconoDisquete;
            btnGuardar.addEventListener('mouseenter', () => {
                btnGuardar.style.color = '#1a6fb0'; btnGuardar.style.background = '#e8f0fe';
            });
            btnGuardar.addEventListener('mouseleave', () => {
                btnGuardar.style.color = '#555'; btnGuardar.style.background = 'none';
            });
            btnGuardar.addEventListener('click', (ev) => {
                ev.stopPropagation();
                // Convertir posicionPorId al formato que espera el servidor: {idEstado, posX, posY}
                const posiciones = Array.from(posicionPorId.entries())
                    .map(([id, p]) => ({
                        idEstado: id,
                        posX:     Math.round(p.x),
                        posY:     Math.round(p.y)
                    }));
                _grabarPosiciones(negocio, posiciones);
            });
            botones.appendChild(btnGuardar);
        }

        // Botón cerrar
        const btnX = document.createElement('button');
        btnX.style.cssText =
            'border:none;background:none;font-size:1.4rem;cursor:pointer;' +
            'line-height:1;padding:0 2px';
        btnX.innerHTML = '&times;';
        botones.appendChild(btnX);

        cabecera.appendChild(titulo);
        cabecera.appendChild(botones);
        caja.appendChild(cabecera);
        caja.appendChild(svg);
        modal.appendChild(caja);
        document.body.appendChild(modal);

        btnX.addEventListener('click', () => modal.remove());
        modal.addEventListener('click', (ev) => { if (ev.target === modal) modal.remove(); });
    }

}
