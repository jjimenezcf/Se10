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

    // Icono del rótulo de la modal (mismo formato que el resto de modales: svg 24x24 con un path)
    const _iconoDeLaModal = 'M6 4a2 2 0 1 0 0 4a2 2 0 1 0 0-4z M18 10a2 2 0 1 0 0 4a2 2 0 1 0 0-4z M6 16a2 2 0 1 0 0 4a2 2 0 1 0 0-4z M8 7l8 4 M8 17l8-4';

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
            // el array va tal cual (no como cadena): el servidor lo recibe como JArray y LeerValor<List<PosicionDeEstado>> lo convierte
            { parametro: 'datospeticion', valor: posiciones }
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

        CargarYMostrarFlujo(navegador, nombreEstado, negocio, idEstado);
        return false;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  CargarYMostrarFlujo
    //  Obtiene el flujo desde `idEstado` (epObtenerFlujo) y abre la modal.
    //  · Sin `posiciones`: usa las que el usuario tenga grabadas (epLeerPosicionesDeEstados).
    //  · Con `posiciones` (botón Recargar): conserva la disposición que se está viendo.
    // ─────────────────────────────────────────────────────────────────────────
    export function CargarYMostrarFlujo(
        llamador:     any,
        nombreEstado: string,
        negocio:      string,
        idEstado:     number,
        posiciones?:  Array<TPosicionGuardada>
    ): void {
        const parFlujo: Array<Parametro> = [
            new Parametro(Ajax.Param.enumNegocio, negocio),
            new Parametro(Ajax.Param.idEstado,    idEstado)
        ];

        ApiDePeticiones.EjecutarPeticion(
            llamador,
            ltrControladores.Negocio.Estados,
            Ajax.EndPoint.Negocio.Estados.ObtenerFlujo,
            parFlujo,
            new Array<Parametro>()
        ).then((pFlujo: ApiDeAjax.DescriptorAjax) => {
            const datosFlujo = pFlujo.resultado.datos;
            const posiciones$ = posiciones ? Promise.resolve(posiciones) : _leerPosicionesGuardadas(negocio);
            posiciones$.then(posGuardadas =>
                MostrarModalDeFlujo(nombreEstado, datosFlujo, negocio, idEstado, posGuardadas)
            );
        }).catch((p: ApiDeAjax.DescriptorAjax) => ApiDePeticiones.EmitirError(p));
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

        // ── 0. Estructura de la modal (misma que el resto de modales) ─────────
        //  contenedor-modal
        //   └ contenido-modal
        //      ├ cabecera-de-modal : barra azul con el rótulo y la × de cerrar
        //      ├ contenido-cuerpo  : div con el gráfico (hace scroll si el svg es mayor que él)
        //      └ contenido-pie     : botones Recargar y Grabar, abajo a la derecha
        // Se monta y se muestra ANTES de calcular la disposición para conocer el tamaño visible del cuerpo.
        const ns = 'http://www.w3.org/2000/svg';
        const idModal = 'modal-flujo-estados';
        let modal = document.getElementById(idModal) as HTMLDivElement;
        if (modal) modal.remove();

        modal = document.createElement('div');
        modal.id = idModal;
        ApiControl.IncluirCss(modal, ltrCss.flujo.contenedorModal);

        const contenido = document.createElement('div');
        ApiControl.IncluirCss(contenido, ltrCss.flujo.contenidoModal);

        // Cabecera: rótulo (icono + título) a la izquierda y × de cerrar a la derecha
        const cabecera = document.createElement('div');
        ApiControl.IncluirCss(cabecera, ltrCss.flujo.cabeceraDeModal);

        const rotulo = document.createElement('span');
        ApiControl.IncluirCss(rotulo, ltrCss.flujo.rotuloDeModal);
        const icono = document.createElementNS(ns, 'svg') as unknown as HTMLElement;
        icono.setAttribute('viewBox', '0 0 24 24');
        icono.setAttribute('aria-hidden', 'true');
        ApiControl.IncluirCss(icono, ltrCss.flujo.iconoDeModal);
        const trazo = document.createElementNS(ns, 'path');
        trazo.setAttribute('d', _iconoDeLaModal);
        icono.appendChild(trazo);
        const titulo = document.createElement('h2');
        titulo.textContent = `Flujo desde: ${nombreEstado}`;
        rotulo.appendChild(icono as unknown as Node);
        rotulo.appendChild(titulo);

        const btnX = document.createElement('span');
        ApiControl.IncluirCss(btnX, ltrCss.flujo.cerrarDeModal);
        btnX.title = 'Cerrar';
        btnX.innerHTML = '&times;';
        btnX.addEventListener('click', () => modal.remove());

        cabecera.appendChild(rotulo);
        cabecera.appendChild(btnX);

        // Cuerpo: div con el gráfico
        const cuerpo = document.createElement('div');
        ApiControl.IncluirCss(cuerpo, ltrCss.flujo.cuerpo);
        const grafico = document.createElement('div');
        ApiControl.IncluirCss(grafico, ltrCss.flujo.grafico);
        cuerpo.appendChild(grafico);

        contenido.appendChild(cabecera);
        contenido.appendChild(cuerpo);
        modal.appendChild(contenido);
        document.body.appendChild(modal);
        // .contenedor-modal está oculta por defecto en CrudModal.css
        modal.style.display = ltrStyle.display.block;

        // tamaño visible del cuerpo (la altura la fija Flujo.css, no depende del svg)
        const anchoVisible = cuerpo.clientWidth;
        const altoVisible  = cuerpo.clientHeight;


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
        const CRECIMIENTO = 200;   // px que se amplía el lienzo cuando un nodo llega a su borde derecho/inferior
        const MARGEN_BORDE = 4;    // separación mínima entre un nodo y el borde del lienzo
        const HOLGURA_BARRA = 20;  // alto que se deja libre por si aparece la barra de scroll horizontal
        const posicionPorId: Map<number, { x: number, y: number }> = new Map();
        const anchoMax = Math.max(...niveles.map(n => (porNivel.get(n) || []).length));
        const anchoCalculado = anchoMax * (radio * 2 + padH) + margen * 2;
        const altoCalculado  = niveles.length * (radio * 2 + padV) + margen * 2;
        // El lienzo ocupa como mínimo el área visible del cuerpo de la modal (así la disposición automática
        // queda centrada); si el dibujo es mayor, el cuerpo muestra las barras de scroll.
        let svgAncho = Math.max(anchoVisible, anchoCalculado);
        let svgAlto  = Math.max(altoVisible - HOLGURA_BARRA, altoCalculado);

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
        // Ningún nodo queda cortado: por arriba/izquierda se acerca al borde (no se puede hacer scroll
        // hacia coordenadas negativas) y por abajo/derecha se amplía el lienzo.
        const minimo = radio + MARGEN_BORDE;
        posicionPorId.forEach(p => {
            p.x = Math.max(p.x, minimo);
            p.y = Math.max(p.y, minimo);
            svgAncho = Math.max(svgAncho, p.x + minimo);
            svgAlto  = Math.max(svgAlto,  p.y + minimo);
        });

        // ── 4. SVG base ───────────────────────────────────────────────────────
        // El tamaño del svg se fija en px (no por CSS) para poder ampliarlo al arrastrar un nodo hasta el borde;
        // el cuerpo de la modal (overflow:auto) muestra las barras de scroll cuando el svg es mayor que él.
        const svg = document.createElementNS(ns, 'svg') as unknown as SVGSVGElement;
        const _aplicarTamano = () => {
            svg.setAttribute('width',  String(svgAncho));
            svg.setAttribute('height', String(svgAlto));
        };
        _aplicarTamano();

        // Convierte la posición del ratón a coordenadas del svg (tiene en cuenta el scroll del cuerpo)
        // para el arrastre de nodos y el tooltip.
        const _puntoEnSvg = (ev: MouseEvent): { x: number, y: number } => {
            const s = svg as unknown as SVGSVGElement;
            const ctm = s.getScreenCTM();
            if (!ctm) return { x: ev.clientX, y: ev.clientY };
            const pt = s.createSVGPoint();
            pt.x = ev.clientX; pt.y = ev.clientY;
            const p = pt.matrixTransform(ctm.inverse());
            return { x: p.x, y: p.y };
        };
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
            const punto = _puntoEnSvg(ev);
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
                `translate(${(punto.x + 10).toFixed(0)},${(punto.y - 20).toFixed(0)})`);
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
                    startMX: _puntoEnSvg(ev).x, startMY: _puntoEnSvg(ev).y,
                    circ, textos
                };
                ApiControl.IncluirCss(circ, ltrCss.flujo.nodoCirculoArrastrando);
            });

            // Doble click: abre el estado en una pestaña (en edición) para modificarlo o añadirle transiciones
            circ.addEventListener('dblclick', (ev: MouseEvent) => {
                ev.preventDefault(); ev.stopPropagation();
                arrastrando = null;
                const url = `${window.location.origin}/${ltrUrls.Entorno.Estados}`
                          + `?${literal.negocio}=${encodeURIComponent(negocio)}&id=${e.id}`;
                EntornoSe.AbrirPestana(url);
            });
        }

        svg.addEventListener('mousemove', (ev: MouseEvent) => {
            if (!arrastrando) return;
            const punto = _puntoEnSvg(ev);
            const minimoNodo = radio + MARGEN_BORDE;
            const newX = Math.max(minimoNodo, arrastrando.startX + (punto.x - arrastrando.startMX));
            const newY = Math.max(minimoNodo, arrastrando.startY + (punto.y - arrastrando.startMY));
            // si el nodo llega al borde derecho/inferior se amplía el lienzo y el cuerpo muestra el scroll
            let ampliado = false;
            while (newX + minimoNodo > svgAncho) { svgAncho += CRECIMIENTO; ampliado = true; }
            while (newY + minimoNodo > svgAlto)  { svgAlto  += CRECIMIENTO; ampliado = true; }
            if (ampliado) _aplicarTamano();
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

        // ── 7. Gráfico en el cuerpo y pie de la modal ─────────────────────────
        grafico.appendChild(svg as unknown as Node);

        // Pie: botones Recargar y Grabar (sólo si se conoce el negocio y el estado inicial)
        if (negocio && idEstadoInicial > 0) {
            // posiciones que se están viendo, en el formato del servidor: {idEstado, posX, posY}
            const posicionesActuales = (): Array<TPosicionGuardada> =>
                Array.from(posicionPorId.entries())
                    .map(([id, p]) => ({ idEstado: id, posX: Math.round(p.x), posY: Math.round(p.y) }));

            const pie = document.createElement('div');
            ApiControl.IncluirCss(pie, ltrCss.flujo.pie);

            // Recargar: vuelve a leer el flujo (p.e. tras editar un estado o una transición en otra pestaña)
            // conservando la disposición actual; los estados nuevos se colocan donde les corresponda.
            const btnRecargar = document.createElement('input');
            btnRecargar.type = 'text';
            btnRecargar.readOnly = true;
            btnRecargar.value = 'Recargar';
            btnRecargar.title = 'Volver a leer el flujo, conservando la disposición actual';
            ApiControl.IncluirCss(btnRecargar, ltrCss.flujo.botonRecargar);
            btnRecargar.addEventListener('click', () =>
                CargarYMostrarFlujo(btnRecargar, nombreEstado, negocio, idEstadoInicial, posicionesActuales()));

            const btnGrabar = document.createElement('input');
            btnGrabar.type = 'text';
            btnGrabar.readOnly = true;
            btnGrabar.value = 'Grabar';
            btnGrabar.title = 'Grabar disposición de estados';
            ApiControl.IncluirCss(btnGrabar, ltrCss.flujo.botonGrabar);
            btnGrabar.addEventListener('click', () => _grabarPosiciones(negocio, posicionesActuales()));

            pie.appendChild(btnRecargar);
            pie.appendChild(btnGrabar);
            contenido.appendChild(pie);
        }
    }

}
