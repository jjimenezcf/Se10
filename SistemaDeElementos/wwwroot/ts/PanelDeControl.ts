namespace PanelDeControl {

    // ─── Tipos ────────────────────────────────────────────────────────────────

    type ModoVista = 'matriz' | 'tartas' | 'barras';

    interface ItemNombreId {
        id: number;
        nombre: string;
    }

    interface CeldaDeMatriz {
        idTipo: number;
        idEstado: number;
        cantidad: number;
    }

    interface DatosDeNegocio {
        nombre: string;
        enumerado: string;
        numTipos: number;
        numEstados: number;
        tipos: ItemNombreId[];
        estados: ItemNombreId[];
        cantidades: CeldaDeMatriz[];   // renombrado de celdas → cantidades (servidor)
        url?: string;                  // enlace para gestionar los objetos del negocio
    }

    /** DTO para serializar/deserializar hacia el servidor */
    interface EstadoDeDashBoardDto {
        negocio: string;           // valor del enumNegocio
        clase: string;             // 'Matriz' | 'Tarta' | 'Coordenadas'
        visible: boolean;
        posicion: number | null;   // orden en el grid; null → al final
    }

    // Alias corto para los literales del DashBoard
    const css = ltrCss.PanelDeControl.DashBoard;

    // Índice de datos por enumerado
    const _negociosData = new Map<string, DatosDeNegocio>();

    // ─── Colores (.fila-estado-N en Grid.css) ────────────────────────────────

    const COLORES_ESTADO: string[] = [
        '#607d8b', '#2e7d32', '#827717', '#006064', '#1a237e',
        '#6a1b9a', '#795548', '#3e2723', '#263238', '#00796b',
        '#303f9f', '#7b1fa2', '#0288d1', '#e65100', '#4527a0',
        '#bf360c', '#01579b', '#004d40', '#1b5e20', '#33691e', '#880e4f'
    ];

    function colorDeEstado(idEstado: number): string {
        const idx = (idEstado % 20) + 1;
        return COLORES_ESTADO[idx] || COLORES_ESTADO[0];
    }

    // ─── Mapeo ModoVista ↔ enumClaseDeDashBoard ───────────────────────────────

    function mapearModoAClase(modo: ModoVista): string {
        switch (modo) {
            case 'matriz':  return 'Matriz';
            case 'tartas':  return 'Tarta';
            case 'barras':  return 'Coordenadas';
        }
    }

    function mapearClaseAModo(clase: string): ModoVista {
        switch (clase) {
            case 'Tarta':       return 'tartas';
            case 'Coordenadas': return 'barras';
            default:            return 'matriz';
        }
    }

    // ─── Modos disponibles para la asignación aleatoria ──────────────────────

    const MODOS_DISPONIBLES: ModoVista[] = ['matriz', 'tartas', 'barras'];

    // ─── Iconos SVG ───────────────────────────────────────────────────────────

    const SVG_TARTA = `<svg width="13" height="13" viewBox="0 0 14 14" fill="currentColor">
        <circle cx="7" cy="7" r="6" fill="none" stroke="currentColor" stroke-width="1.5"/>
        <path d="M7,7 L7,1 A6,6 0,0,1 13,7 Z" opacity="0.85"/>
        <path d="M7,7 L13,7 A6,6 0,0,1 3.5,12.2 Z" opacity="0.55"/>
        <path d="M7,7 L3.5,12.2 A6,6 0,0,1 1,7 Z" opacity="0.35"/>
    </svg>`;

    const SVG_BARRAS = `<svg width="13" height="13" viewBox="0 0 14 14" fill="currentColor">
        <rect x="1"   y="7"  width="3" height="6"/>
        <rect x="5.5" y="4"  width="3" height="9"/>
        <rect x="10"  y="1"  width="3" height="12"/>
        <line x1="1" y1="13" x2="13" y2="13" stroke="currentColor" stroke-width="1"/>
    </svg>`;

    const SVG_MATRIZ = `<svg width="13" height="13" viewBox="0 0 15 15" fill="currentColor">
        <rect x="1" y="1" width="4" height="4" opacity="0.9"/>
        <rect x="6" y="1" width="4" height="4" opacity="0.5"/>
        <rect x="1" y="6" width="4" height="4" opacity="0.5"/>
        <rect x="6" y="6" width="4" height="4" opacity="0.9"/>
        <rect x="11" y="1" width="3" height="12" opacity="0.25"/>
        <rect x="1" y="11" width="12" height="3" opacity="0.25"/>
    </svg>`;

    const SVG_PREGUNTA = `<svg width="11" height="11" viewBox="0 0 14 14" fill="none"
        stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round">
        <circle cx="7" cy="7" r="6"/>
        <path d="M5.2,5.2 a1.8,1.8 0 1,1 1.8,1.8 v1.2"/>
        <circle cx="7" cy="11" r="0.5" fill="currentColor" stroke="none"/>
    </svg>`;

    const SVG_FLECHA_DER = `<svg width="10" height="10" viewBox="0 0 10 10"
        fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
        <polyline points="3,1 7,5 3,9"/>
    </svg>`;

    const SVG_FLECHA_IZQ = `<svg width="10" height="10" viewBox="0 0 10 10"
        fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
        <polyline points="7,1 3,5 7,9"/>
    </svg>`;

    // ─── Entrada pública ──────────────────────────────────────────────────────

    export function Inicializar() {
        const peticion = new ApiDeAjax.DescriptorAjax(
            null, 'epGraficasDeNegocio', null,
            `/Home/epGraficasDeNegocio`,
            ApiDeAjax.TipoPeticion.Asincrona, ApiDeAjax.ModoPeticion.Get,
            (pet: ApiDeAjax.DescriptorAjax) => {
                RenderTarjetas(pet.resultado.datos as DatosDeNegocio[]);
                AplicarDisposicion();
            },
            (pet: ApiDeAjax.DescriptorAjax) => {
                console.error(`Error cargando gráficas: ${pet.resultado.mensaje}`);
            }
        );
        peticion.Ejecutar();
    }

    // ─── Reset del dashboard ──────────────────────────────────────────────────

    export function ResetearDashBoard() {
        fetch('/Home/epResetearDashBoard', { method: 'GET' })
            .then(() => window.location.reload())
            .catch(err => console.error('Error reseteando el dashboard:', err));
    }

    // ─── Construcción de tarjetas ─────────────────────────────────────────────

    function RenderTarjetas(negocios: DatosDeNegocio[]) {
        const contenedor = document.getElementById(css.PanelGraficas);
        if (!contenedor) return;
        contenedor.innerHTML = '';
        _negociosData.clear();

        negocios.forEach(negocio => {
            if (negocio.numTipos === 0 && negocio.numEstados === 0) return;
            _negociosData.set(negocio.enumerado, negocio);

            const tarjeta = document.createElement('div');
            tarjeta.className = css.GraficaContenedor;
            tarjeta.id = `grafica-${negocio.enumerado}`;
            tarjeta.dataset.modoActual = 'matriz';

            // ── ✕ Cerrar ──
            const btnCerrar = document.createElement('button');
            btnCerrar.className = css.GraficaCerrar;
            btnCerrar.title = 'Ocultar';
            btnCerrar.textContent = '✕';
            btnCerrar.addEventListener('click', () => {
                tarjeta.style.display = 'none';
                GuardarDisposicion();
            });

            // ── Modo vista ──
            const btnModo = document.createElement('button');
            btnModo.className = css.GraficaModo;
            ponerIconoModo(btnModo, 'matriz');
            btnModo.addEventListener('click', () => {
                CambiarModo(tarjeta, negocio,
                    btnModo.dataset.siguienteModo as ModoVista);
                GuardarDisposicion();
            });

            // ── ? Pregunta IA ──
            const btnPregunta = document.createElement('button');
            btnPregunta.className = css.GraficaPreguntar;
            btnPregunta.title = 'Hacer una pregunta';
            btnPregunta.innerHTML = SVG_PREGUNTA;
            ApiPreguntasIa.InyectarConteoIA(negocio.enumerado, negocio.nombre, btnPregunta, negocio.url);

            // ── → Mover derecha ──
            const btnDer = document.createElement('button');
            btnDer.className = `${css.GraficaMover} ${css.GraficaMoverDer}`;
            btnDer.title = 'Mover a la derecha';
            btnDer.innerHTML = SVG_FLECHA_DER;
            btnDer.addEventListener('click', () => MoverTarjeta(tarjeta, 'derecha'));

            // ── ← Mover izquierda ──
            const btnIzq = document.createElement('button');
            btnIzq.className = `${css.GraficaMover} ${css.GraficaMoverIzq}`;
            btnIzq.title = 'Mover a la izquierda';
            btnIzq.innerHTML = SVG_FLECHA_IZQ;
            btnIzq.addEventListener('click', () => MoverTarjeta(tarjeta, 'izquierda'));

            // ── Título (enlace si hay url, texto plano si no) ──
            const titulo = document.createElement('h3');
            titulo.className = css.GraficaTitulo; // Esto aplicará "grafica-titulo"

            if (negocio.url) {
                const enlace = document.createElement('a');

                // 1. Establecemos el href a "#" tal y como quieres
                enlace.setAttribute('href', '#');

                // 2. Le inyectamos el string que viene de C# en el atributo 'onclick'
                // Además, le concatenamos el '; return false;' para anular el comportamiento del '#'
                enlace.setAttribute('onclick', `${negocio.url}; return false;`);

                // 3. El texto visible del enlace
                enlace.textContent = negocio.nombre;

                titulo.appendChild(enlace);
            } else {
                titulo.textContent = negocio.nombre;
            }

            // ── Vista ──
            const vistaDiv = document.createElement('div');
            vistaDiv.className = css.GraficaVistaContenedor;
            vistaDiv.appendChild(ConstruirMatriz(negocio));

            tarjeta.appendChild(btnCerrar);
            tarjeta.appendChild(btnModo);
            tarjeta.appendChild(btnPregunta);
            tarjeta.appendChild(btnDer);
            tarjeta.appendChild(btnIzq);
            tarjeta.appendChild(titulo);
            tarjeta.appendChild(vistaDiv);
            contenedor.appendChild(tarjeta);
        });
    }

    // ─── Mover tarjeta ────────────────────────────────────────────────────────

    function MoverTarjeta(tarjeta: HTMLDivElement, direccion: 'izquierda' | 'derecha') {
        const contenedor = tarjeta.parentElement;
        if (!contenedor) return;

        const todas = Array.from(
            contenedor.querySelectorAll<HTMLDivElement>(`.${css.GraficaContenedor}`));
        const idx = todas.indexOf(tarjeta);

        if (direccion === 'derecha' && idx < todas.length - 1) {
            // Insertar la siguiente antes de esta
            contenedor.insertBefore(todas[idx + 1], tarjeta);
        } else if (direccion === 'izquierda' && idx > 0) {
            // Insertar esta antes de la anterior
            contenedor.insertBefore(tarjeta, todas[idx - 1]);
        } else {
            return; // ya está en el borde, nada que hacer
        }

        GuardarDisposicion();
    }

    // ─── Cambio de modo ───────────────────────────────────────────────────────

    function CambiarModo(tarjeta: HTMLDivElement, negocio: DatosDeNegocio, modo: ModoVista) {
        const vistaDiv = tarjeta.querySelector(`.${css.GraficaVistaContenedor}`) as HTMLDivElement;
        const btnModo  = tarjeta.querySelector(`.${css.GraficaModo}`) as HTMLButtonElement;
        vistaDiv.innerHTML = '';

        switch (modo) {
            case 'matriz':  vistaDiv.appendChild(ConstruirMatriz(negocio)); break;
            case 'tartas':  vistaDiv.appendChild(ConstruirTartas(negocio)); break;
            case 'barras':  vistaDiv.appendChild(ConstruirBarras(negocio)); break;
        }
        tarjeta.dataset.modoActual = modo;
        ponerIconoModo(btnModo, modo);
    }

    function ponerIconoModo(btn: HTMLButtonElement, modoActual: ModoVista) {
        switch (modoActual) {
            case 'matriz':
                btn.innerHTML = SVG_TARTA;
                btn.title = 'Ver diagramas de tarta';
                btn.dataset.siguienteModo = 'tartas';
                break;
            case 'tartas':
                btn.innerHTML = SVG_BARRAS;
                btn.title = 'Ver gráficas de barras';
                btn.dataset.siguienteModo = 'barras';
                break;
            case 'barras':
                btn.innerHTML = SVG_MATRIZ;
                btn.title = 'Ver como matriz';
                btn.dataset.siguienteModo = 'matriz';
                break;
        }
    }

    // ─── Persistencia ─────────────────────────────────────────────────────────

    function AplicarDisposicion() {
        const peticion = new ApiDeAjax.DescriptorAjax(
            null, 'epLeerDisposicionDashBoard', null,
            `/Home/epLeerDisposicionDashBoard`,
            ApiDeAjax.TipoPeticion.Asincrona, ApiDeAjax.ModoPeticion.Get,
            (pet: ApiDeAjax.DescriptorAjax) => {
                const estados: EstadoDeDashBoardDto[] = pet.resultado.datos;

                if (!estados || estados.length === 0) {
                    AplicarDisposicionAleatoria();
                    AjustarAnchura();
                    return;
                }

                // 1. Reordenar DOM según posición guardada
                ReordenarTarjetas(estados);

                // 2. Aplicar visibilidad y modo
                estados.forEach(estado => {
                    const tarjeta = document.getElementById(
                        `grafica-${estado.negocio}`) as HTMLDivElement | null;
                    if (!tarjeta) return;

                    if (!estado.visible) {
                        tarjeta.style.display = 'none';
                    } else {
                        const modoDeseado = mapearClaseAModo(estado.clase);
                        const modoActual  = (tarjeta.dataset.modoActual || 'matriz') as ModoVista;
                        if (modoDeseado !== modoActual) {
                            const nd = _negociosData.get(estado.negocio);
                            if (nd) CambiarModo(tarjeta, nd, modoDeseado);
                        }
                    }
                });

                AjustarAnchura();
            },
            (pet: ApiDeAjax.DescriptorAjax) => {
                console.error(`Error leyendo disposición: ${pet.resultado.mensaje}`);
            }
        );
        peticion.Ejecutar();
    }

    /**
     * Reordena los nodos del DOM según el campo `posicion` guardado.
     * Los que tienen posicion=null van al final en orden original.
     */
    function ReordenarTarjetas(estados: EstadoDeDashBoardDto[]) {
        const contenedor = document.getElementById(css.PanelGraficas);
        if (!contenedor) return;

        // Ordenar por posición (nulos al final)
        const conPosicion = [...estados]
            .filter(e => e.posicion != null)
            .sort((a, b) => (a.posicion as number) - (b.posicion as number));

        conPosicion.forEach(estado => {
            const tarjeta = document.getElementById(`grafica-${estado.negocio}`);
            if (tarjeta) contenedor.appendChild(tarjeta); // mueve al final en orden
        });
        // Los no guardados quedan donde estaban (al final naturalmente)
    }

    function GuardarDisposicion() {
        AjustarAnchura();
        const estados = RecopilarEstado();
        const cuerpo = JSON.stringify([{ parametro: 'datosPeticion', valor: estados }]);
        fetch('/Home/epGrabarGraficasDeNegocio', { method: 'POST', body: cuerpo })
            .catch(err => console.error('Error guardando disposición:', err));
    }

    /**
     * Si solo hay una tarjeta visible la hace ocupar todo el ancho del grid.
     * Si hay más de una, restaura el ancho normal en todas.
     */
    function AjustarAnchura() {
        const contenedor = document.getElementById(css.PanelGraficas);
        if (!contenedor) return;

        const todas = Array.from(
            contenedor.querySelectorAll<HTMLDivElement>(`.${css.GraficaContenedor}`));
        const visibles = todas.filter(t => t.style.display !== 'none');

        todas.forEach(t => t.classList.remove(css.GraficaAnchoCompleto));

        if (visibles.length === 1)
            visibles[0].classList.add(css.GraficaAnchoCompleto);
    }

    function RecopilarEstado(): EstadoDeDashBoardDto[] {
        const contenedor = document.getElementById(css.PanelGraficas);
        if (!contenedor) return [];

        const resultado: EstadoDeDashBoardDto[] = [];
        let posicion = 0;

        contenedor.querySelectorAll<HTMLDivElement>(`.${css.GraficaContenedor}`)
            .forEach(tarjeta => {
                const enumerado  = tarjeta.id.replace('grafica-', '');
                const visible    = tarjeta.style.display !== 'none';
                const modoActual = (tarjeta.dataset.modoActual || 'matriz') as ModoVista;

                resultado.push({
                    negocio:  enumerado,
                    clase:    mapearModoAClase(modoActual),
                    visible,
                    posicion: posicion++   // posición según orden actual del DOM
                });
            });

        return resultado;
    }

    // ─── Distribución aleatoria (primera vez) ────────────────────────────────

    function AplicarDisposicionAleatoria() {
        const contenedor = document.getElementById(css.PanelGraficas);
        if (!contenedor) return;

        const tarjetas = Array.from(
            contenedor.querySelectorAll<HTMLDivElement>(`.${css.GraficaContenedor}`));

        const hayConDatos = tarjetas.some(t => {
            const nd = _negociosData.get(t.id.replace('grafica-', ''));
            return nd?.cantidades?.some(c => c.cantidad > 0) ?? false;
        });

        tarjetas.forEach(tarjeta => {
            const enumerado   = tarjeta.id.replace('grafica-', '');
            const negocioData = _negociosData.get(enumerado);
            const tieneElementos = negocioData?.cantidades?.some(c => c.cantidad > 0) ?? false;

            if (!tieneElementos && hayConDatos) {
                tarjeta.style.display = 'none';
                return;
            }
            if (!negocioData) return;

            const modo = MODOS_DISPONIBLES[
                Math.floor(Math.random() * MODOS_DISPONIBLES.length)];
            if (modo !== 'matriz') CambiarModo(tarjeta, negocioData, modo);
        });
    }

    // ─── MODO MATRIZ ──────────────────────────────────────────────────────────

    function ConstruirMatriz(negocio: DatosDeNegocio): HTMLDivElement {
        const wrap = document.createElement('div');
        wrap.className = css.GraficaMatrizContenedor;

        const tabla = document.createElement('table');
        tabla.className = css.GraficaMatriz;

        const indice = new Map<string, number>();
        negocio.cantidades.forEach(c => indice.set(`${c.idTipo}_${c.idEstado}`, c.cantidad));
        const maxVal = negocio.cantidades.length > 0
            ? Math.max(...negocio.cantidades.map(c => c.cantidad)) : 0;

        const thead = tabla.createTHead();
        const filaCab = thead.insertRow();
        const th0 = document.createElement('th');
        th0.textContent = 'Tipo \\ Estado';
        filaCab.appendChild(th0);
        negocio.estados.forEach(e => {
            const th = document.createElement('th');
            th.textContent = e.nombre;
            filaCab.appendChild(th);
        });

        const tbody = tabla.createTBody();
        negocio.tipos.forEach(tipo => {
            const fila = tbody.insertRow();
            const tdTipo = document.createElement('td');
            tdTipo.className = css.CeldaTipo;
            tdTipo.textContent = tipo.nombre;
            fila.appendChild(tdTipo);

            negocio.estados.forEach(estado => {
                const cantidad = indice.get(`${tipo.id}_${estado.id}`) ?? 0;
                const td = fila.insertCell();
                td.className = css.CeldaValor;
                td.textContent = cantidad > 0 ? cantidad.toString() : '';
                if (cantidad > 0 && maxVal > 0) {
                    td.style.backgroundColor =
                        `rgba(78,121,167,${(cantidad / maxVal).toFixed(2)})`;
                    td.style.color = cantidad / maxVal > 0.55 ? '#fff' : '#333';
                }
            });
        });

        wrap.appendChild(tabla);
        return wrap;
    }

    // ─── MODO TARTAS ──────────────────────────────────────────────────────────

    function ConstruirTartas(negocio: DatosDeNegocio): HTMLDivElement {
        const wrap = document.createElement('div');
        wrap.className = css.GraficaTartasContenedor;

        negocio.tipos.forEach(tipo => {
            const celdasTipo = negocio.cantidades.filter(
                c => c.idTipo === tipo.id && c.cantidad > 0);
            if (celdasTipo.length === 0) return;

            const total = celdasTipo.reduce((s, c) => s + c.cantidad, 0);
            const tipoDiv = document.createElement('div');
            tipoDiv.className = css.GraficaTartaItem;

            const tituloT = document.createElement('div');
            tituloT.className = css.GraficaTartaTitulo;
            tituloT.textContent = tipo.nombre;
            tipoDiv.appendChild(tituloT);

            const R = 44, cx = 50, cy = 50;
            let paths = '', leyenda = '', angulo = -Math.PI / 2;

            celdasTipo.forEach(celda => {
                const estado = negocio.estados.find(e => e.id === celda.idEstado);
                const color = colorDeEstado(celda.idEstado);
                const delta = (celda.cantidad / total) * 2 * Math.PI;
                const fin = angulo + delta;
                const x1 = cx + R * Math.cos(angulo), y1 = cy + R * Math.sin(angulo);
                const x2 = cx + R * Math.cos(fin),    y2 = cy + R * Math.sin(fin);
                const grande = delta > Math.PI ? 1 : 0;

                if (celda.cantidad / total >= 0.9999) {
                    paths += `<circle cx="${cx}" cy="${cy}" r="${R}" fill="${color}">
                        <title>${estado?.nombre}: ${celda.cantidad}</title></circle>`;
                } else {
                    paths += `<path d="M${cx},${cy} L${x1.toFixed(2)},${y1.toFixed(2)} ` +
                        `A${R},${R} 0 ${grande},1 ${x2.toFixed(2)},${y2.toFixed(2)} Z" ` +
                        `fill="${color}" stroke="#fff" stroke-width="1">` +
                        `<title>${estado?.nombre}: ${celda.cantidad}</title></path>`;
                }
                leyenda += `<div class="${css.GraficaLeyendaItem}">
                    <span class="${css.GraficaLeyendaColor}" style="background:${color}"></span>
                    <span>${estado?.nombre ?? '?'}: <b>${celda.cantidad}</b></span></div>`;
                angulo = fin;
            });

            const svg = document.createElementNS('http://www.w3.org/2000/svg', 'svg');
            svg.setAttribute('width', '100');
            svg.setAttribute('height', '100');
            svg.setAttribute('viewBox', '0 0 100 100');
            svg.classList.add(css.GraficaTartaSvg);
            svg.innerHTML = paths;

            const leyendaDiv = document.createElement('div');
            leyendaDiv.className = css.GraficaLeyenda;
            leyendaDiv.innerHTML = leyenda;

            tipoDiv.appendChild(svg);
            tipoDiv.appendChild(leyendaDiv);
            wrap.appendChild(tipoDiv);
        });

        return wrap;
    }

    // ─── MODO BARRAS ──────────────────────────────────────────────────────────

    function ConstruirBarras(negocio: DatosDeNegocio): HTMLDivElement {
        const wrap = document.createElement('div');
        wrap.className = css.GraficaBarrasContenedor;

        const W = 220, H = 120;
        const pL = 28, pB = 36, pT = 12, pR = 8;
        const aW = W - pL - pR, aH = H - pT - pB;

        negocio.tipos.forEach(tipo => {
            const celdasTipo = negocio.cantidades.filter(
                c => c.idTipo === tipo.id && c.cantidad > 0);
            if (celdasTipo.length === 0) return;

            const tipoDiv = document.createElement('div');
            tipoDiv.className = css.GraficaBarraItem;

            const tituloB = document.createElement('div');
            tituloB.className = css.GraficaTartaTitulo;
            tituloB.textContent = tipo.nombre;
            tipoDiv.appendChild(tituloB);

            const maxVal = Math.max(...celdasTipo.map(c => c.cantidad));
            const n = celdasTipo.length;
            const slotW = aW / n;
            const barW = Math.max(Math.floor(slotW * 0.65), 4);
            let bars = '', yLines = '', labels = '';

            for (let i = 0; i <= 3; i++) {
                const val = Math.round((maxVal / 3) * i);
                const y = pT + aH - Math.round((val / maxVal) * aH);
                yLines += `<line x1="${pL}" y1="${y}" x2="${W - pR}" y2="${y}"
                    stroke="#ddd" stroke-width="0.5"/>`;
                yLines += `<text x="${pL - 3}" y="${y + 3}" text-anchor="end"
                    font-size="7" fill="#888">${val}</text>`;
            }

            celdasTipo.forEach((celda, i) => {
                const estado = negocio.estados.find(e => e.id === celda.idEstado);
                const color = colorDeEstado(celda.idEstado);
                const cx = pL + i * slotW + slotW / 2;
                const bx = cx - barW / 2;
                const h = Math.max(Math.round((celda.cantidad / maxVal) * aH), 2);
                const by = pT + aH - h;

                bars += `<rect x="${bx.toFixed(1)}" y="${by}" width="${barW}" height="${h}"
                    fill="${color}" rx="1">
                    <title>${estado?.nombre}: ${celda.cantidad}</title></rect>`;
                bars += `<text x="${cx.toFixed(1)}" y="${by - 2}"
                    text-anchor="middle" font-size="7" fill="#444">${celda.cantidad}</text>`;

                labels += `<text x="${cx.toFixed(1)}" y="${pT + aH + 8}"
                    text-anchor="end" font-size="6.5" fill="#555"
                    transform="rotate(-40 ${cx.toFixed(1)} ${pT + aH + 8})">
                    ${(estado?.nombre ?? '').substring(0, 10)}</text>`;
            });

            const svg = document.createElementNS('http://www.w3.org/2000/svg', 'svg');
            svg.setAttribute('width',   W.toString());
            svg.setAttribute('height',  H.toString());
            svg.setAttribute('viewBox', `0 0 ${W} ${H}`);
            svg.classList.add(css.GraficaBarraSvg);
            svg.innerHTML = `
                ${yLines}
                <line x1="${pL}" y1="${pT}" x2="${pL}" y2="${pT + aH}"
                    stroke="#aaa" stroke-width="1"/>
                <line x1="${pL}" y1="${pT + aH}" x2="${W - pR}" y2="${pT + aH}"
                    stroke="#aaa" stroke-width="1"/>
                ${bars}${labels}`;

            tipoDiv.appendChild(svg);
            wrap.appendChild(tipoDiv);
        });

        return wrap;
    }
}
