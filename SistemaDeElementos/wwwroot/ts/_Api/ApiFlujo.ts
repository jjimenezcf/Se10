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
//
//  Los estilos visuales están en Flujo.css, referenciados desde ltrCss.flujo
//  y aplicados/quitados con ApiControl.IncluirCss / ApiControl.ExcluirCss.
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
        negocio:         string                   = '',
        idEstadoInicial: number                   = 0,
        posGuardadas:    Array<TPosicionGuardada>  = []
    ): void {

        const estados:     Array<{ id: number, nombre: string }> = datos.estados     || [];
        const transiciones: Array<{ id: number, nombre: string, idOrigen: number, idDestino: number }> = datos.transiciones || [];
        const idNegocio: number = Numero(datos.idNegocio) || 0;

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
        const svg = document.createElementNS(ns, 'svg') as unknown as SVGSVGElement;
        svg.setAttribute('width',  String(svgAncho));
        svg.setAttribute('height', String(svgAlto));
        ApiControl.IncluirCss(svg as unknown as HTMLElement, ltrCss.flujo.svg);

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
        const hoverMarkers = document.createElementNS(ns, 'g') as unknown as HTMLElement;
        ApiControl.IncluirCss(hoverMarkers, ltrCss.flujo.marcadorHover);
        const mkInicio = document.createElementNS(ns, 'circle');
        mkInicio.setAttribute('r', '5'); mkInicio.setAttribute('fill', '#ffd600');
        mkInicio.setAttribute('stroke', '#b8a000'); mkInicio.setAttribute('stroke-width', '1.2');
        const mkFin = document.createElementNS(ns, 'circle');
        mkFin.setAttribute('r', '5'); mkFin.setAttribute('fill', '#ffd600');
        mkFin.setAttribute('stroke', '#b8a000'); mkFin.setAttribute('stroke-width', '1.2');
        hoverMarkers.appendChild(mkInicio); hoverMarkers.appendChild(mkFin);

        // Tooltip (click en flecha)
        const tooltip = document.createElementNS(ns, 'g') as unknown as HTMLElement;
        ApiControl.IncluirCss(tooltip, ltrCss.flujo.tooltip);
        const ttRect = document.createElementNS(ns, 'rect') as unknown as HTMLElement;
        ApiControl.IncluirCss(ttRect, ltrCss.flujo.tooltipRect);
        const ttText = document.createElementNS(ns, 'text') as unknown as HTMLElement;
        ApiControl.IncluirCss(ttText, ltrCss.flujo.tooltipTexto);
        ttText.setAttribute('x', '0'); ttText.setAttribute('y', '0');
        tooltip.appendChild(ttRect as unknown as Node); tooltip.appendChild(ttText as unknown as Node);

        // Pinta el tooltip con una línea por cada elemento de `lineas` (usando <tspan>)
        // y lo posiciona/redimensiona junto al punto donde se hizo click.
        const _mostrarTooltip = (lineas: string[], ev: MouseEvent) => {
            const rect = (svg as unknown as SVGSVGElement).getBoundingClientRect();
            while (ttText.firstChild) ttText.removeChild(ttText.firstChild);
            lineas.forEach((linea, i) => {
                const tspan = document.createElementNS(ns, 'tspan') as unknown as HTMLElement;
                tspan.setAttribute('x', '0');
                tspan.setAttribute('dy', i === 0 ? '0' : '14');
                tspan.textContent = linea;
                ttText.appendChild(tspan as unknown as Node);
            });
            ApiControl.IncluirCss(tooltip, ltrCss.flujo.tooltipVisible);
            tooltip.setAttribute('transform',
                `translate(${(ev.clientX - rect.left + 10).toFixed(0)},${(ev.clientY - rect.top - 20).toFixed(0)})`);
            const bb = (ttText as unknown as SVGTextElement).getBBox();
            ttRect.setAttribute('x',      (bb.x - 6).toFixed(1));
            ttRect.setAttribute('y',      (bb.y - 4).toFixed(1));
            ttRect.setAttribute('width',  (bb.width  + 12).toFixed(1));
            ttRect.setAttribute('height', (bb.height +  8).toFixed(1));
        };

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
        // Evita que la respuesta tardía de LeerElementos reabra el tooltip
        // si el usuario ya cambió de flecha o lo cerró.
        let tokenTooltipActivo = 0;

        for (const t of transiciones) {
            if (!posicionPorId.get(t.idOrigen) || !posicionPorId.get(t.idDestino)) continue;
            const nOrigen  = nivelPorId.get(t.idOrigen)  || 0;
            const nDestino = nivelPorId.get(t.idDestino) || 0;
            const esAvance = nDestino > nOrigen;
            const marcador = esAvance ? 'url(#flecha-av)' : 'url(#flecha-re)';

            const pts = calcPuntos(t); if (!pts) continue;
            let { x1, y1, x2, y2 } = pts;

            const g = document.createElementNS(ns, 'g') as unknown as HTMLElement;
            ApiControl.IncluirCss(g, ltrCss.flujo.grupoFlecha);
            const hit   = document.createElementNS(ns, 'line') as unknown as HTMLElement;
            const linea = document.createElementNS(ns, 'line') as unknown as HTMLElement;

            const aplicarPuntos = (p: typeof pts) => {
                for (const el of [hit, linea]) {
                    el.setAttribute('x1', p.x1.toFixed(1)); el.setAttribute('y1', p.y1.toFixed(1));
                    el.setAttribute('x2', p.x2.toFixed(1)); el.setAttribute('y2', p.y2.toFixed(1));
                }
            };
            aplicarPuntos(pts);
            ApiControl.IncluirCss(hit,   ltrCss.flujo.lineaHit);
            ApiControl.IncluirCss(linea, ltrCss.flujo.linea);
            ApiControl.IncluirCss(linea, esAvance ? ltrCss.flujo.lineaAvance : ltrCss.flujo.lineaRetroceso);
            linea.setAttribute('marker-end', marcador);
            g.appendChild(hit); g.appendChild(linea); svg.appendChild(g as unknown as Node);

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
                ApiControl.IncluirCss(linea, ltrCss.flujo.lineaHover);
                mkInicio.setAttribute('cx', x1.toFixed(1)); mkInicio.setAttribute('cy', y1.toFixed(1));
                mkFin.setAttribute('cx',    x2.toFixed(1)); mkFin.setAttribute('cy',    y2.toFixed(1));
                ApiControl.IncluirCss(hoverMarkers, ltrCss.flujo.marcadorHoverVisible);
                flechaConHover = updateArrow;
            });
            g.addEventListener('mouseleave', () => {
                ApiControl.ExcluirCss(linea, ltrCss.flujo.lineaHover);
                ApiControl.ExcluirCss(hoverMarkers, ltrCss.flujo.marcadorHoverVisible);
                ApiControl.ExcluirCss(tooltip, ltrCss.flujo.tooltipVisible);
                flechaConHover = null;
                tokenTooltipActivo++;
            });
            g.addEventListener('click', (ev: MouseEvent) => {
                ev.stopPropagation();
                const miToken = ++tokenTooltipActivo;

                // Mostrar de inmediato el nombre de la transición…
                _mostrarTooltip([t.nombre], ev);

                // …y a continuación las acciones que se ejecutan en ella (si se conoce el negocio)
                if (idNegocio > 0) {
                    const filtros = [
                        new ClausulaDeFiltrado('IdTransicion',     literal.filtro.criterio.igual, t.id),
                        new ClausulaDeFiltrado(Ajax.Param.idNegocio, literal.filtro.criterio.igual, idNegocio)
                    ];
                    const parametros: Array<Parametro> = [
                        new Parametro(Ajax.Param.aplicarJoin,      true),
                        new Parametro(Ajax.Param.cantidad,         '-1'),
                        new Parametro(Ajax.Param.obtenerSeguridad, false),
                        new Parametro(Ajax.Param.ordenarPor,       ''),
                        new Parametro(Ajax.Param.fitrarPara,       'CargarGridDeRelacion')
                    ];
                    ApiDePeticiones.LeerElementos(
                        g,
                        ltrControladores.Negocio.AccionesDeTrn,
                        Ajax.EndPoint.LeerElementos,
                        filtros,
                        parametros,
                        new Array<Parametro>(),
                        false
                    ).then((pAcciones: ApiDeAjax.DescriptorAjax) => {
                        if (miToken !== tokenTooltipActivo) return; // el usuario ya cambió de flecha/cerró
                        const acciones: Array<{ accion: string }> = pAcciones.resultado.datos || [];
                        const lineas = [t.nombre, ...acciones.map(a => `• ${a.accion}`)];
                        _mostrarTooltip(lineas, ev);
                    }).catch(() => { /* se deja el tooltip sólo con el nombre de la transición */ });
                }
            });
        }

        // ── 6. Nodos arrastrables ─────────────────────────────────────────────
        let arrastrando: {
            id: number, startX: number, startY: number, startMX: number, startMY: number,
            circ: HTMLElement, textos: Array<{ el: HTMLElement, offsetY: number }>
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

            const circ = document.createElementNS(ns, 'circle') as unknown as HTMLElement;
            circ.setAttribute('cx', String(pos.x)); circ.setAttribute('cy', String(pos.y));
            circ.setAttribute('r', String(radio));
            ApiControl.IncluirCss(circ, ltrCss.flujo.nodoCirculo);
            svg.appendChild(circ as unknown as Node);

            const alturaTexto = lineas.length * 14;
            const textos = lineas.map((l, i) => {
                const offsetY = -alturaTexto / 2 + 14 + i * 14;
                const txt = document.createElementNS(ns, 'text') as unknown as HTMLElement;
                txt.setAttribute('x', String(pos.x));
                txt.setAttribute('y', (pos.y + offsetY).toFixed(1));
                ApiControl.IncluirCss(txt, ltrCss.flujo.nodoTexto);
                txt.textContent = l;
                svg.appendChild(txt as unknown as Node);
                return { el: txt, offsetY };
            });

            circ.addEventListener('mousedown', (ev: MouseEvent) => {
                ev.preventDefault(); ev.stopPropagation();
                ApiControl.ExcluirCss(tooltip, ltrCss.flujo.tooltipVisible);
                ApiControl.ExcluirCss(hoverMarkers, ltrCss.flujo.marcadorHoverVisible);
                tokenTooltipActivo++;
                arrastrando = {
                    id: e.id,
                    startX: posicionPorId.get(e.id)!.x,
                    startY: posicionPorId.get(e.id)!.y,
                    startMX: ev.clientX, startMY: ev.clientY,
                    circ, textos
                };
                ApiControl.IncluirCss(circ, ltrCss.flujo.nodoCirculoArrastrando);
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
            if (arrastrando) {
                ApiControl.ExcluirCss(arrastrando.circ, ltrCss.flujo.nodoCirculoArrastrando);
                arrastrando = null;
            }
        };
        svg.addEventListener('mouseup',    soltarNodo);
        svg.addEventListener('mouseleave', soltarNodo);

        svg.appendChild(hoverMarkers as unknown as Node);
        svg.appendChild(tooltip as unknown as Node);
        svg.addEventListener('click', () => { ApiControl.ExcluirCss(tooltip, ltrCss.flujo.tooltipVisible); tokenTooltipActivo++; });

        // ── 7. Montar la modal ────────────────────────────────────────────────
        const idModal = 'modal-flujo-estados';
        let modal = document.getElementById(idModal) as HTMLDivElement;
        if (modal) modal.remove();

        modal = document.createElement('div');
        modal.id = idModal;
        ApiControl.IncluirCss(modal, ltrCss.flujo.modalOverlay);

        const caja = document.createElement('div');
        ApiControl.IncluirCss(caja, ltrCss.flujo.modalCaja);

        // Cabecera: título + botón guardar + botón cerrar
        const cabecera = document.createElement('div');
        ApiControl.IncluirCss(cabecera, ltrCss.flujo.cabecera);

        const titulo = document.createElement('h5');
        ApiControl.IncluirCss(titulo, ltrCss.flujo.titulo);
        titulo.textContent = `Flujo desde: ${nombreEstado}`;

        // Botones a la derecha (guardar + cerrar)
        const botones = document.createElement('div');
        ApiControl.IncluirCss(botones, ltrCss.flujo.botones);

        // Botón guardar (sólo visible si se conoce negocio e idEstadoInicial)
        if (negocio && idEstadoInicial > 0) {
            const btnGuardar = document.createElement('button');
            btnGuardar.title = 'Grabar disposición de estados';
            ApiControl.IncluirCss(btnGuardar, ltrCss.flujo.btnGuardar);
            btnGuardar.innerHTML = _iconoDisquete;
            btnGuardar.addEventListener('mouseenter', () => ApiControl.IncluirCss(btnGuardar, ltrCss.flujo.btnGuardarHover));
            btnGuardar.addEventListener('mouseleave', () => ApiControl.ExcluirCss(btnGuardar, ltrCss.flujo.btnGuardarHover));
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
        ApiControl.IncluirCss(btnX, ltrCss.flujo.btnCerrar);
        btnX.innerHTML = '&times;';
        botones.appendChild(btnX);

        cabecera.appendChild(titulo);
        cabecera.appendChild(botones);
        caja.appendChild(cabecera);
        caja.appendChild(svg as unknown as Node);
        modal.appendChild(caja);
        document.body.appendChild(modal);

        btnX.addEventListener('click', () => modal.remove());
        modal.addEventListener('click', (ev) => { if (ev.target === modal) modal.remove(); });
    }

}
