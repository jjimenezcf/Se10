namespace ApiDeGraficos {

    export function InsertarGraficaDeTotales(contenedorControles: HTMLDivElement, divPadre: HTMLDivElement): void {
        const idGrafica = `grafica-totales-${contenedorControles.id}`;
        const anterior = document.getElementById(idGrafica);
        if (Definido(anterior)) anterior.remove();

        const controles = contenedorControles.querySelectorAll(`[${atControl.propiedad}]`) as NodeListOf<HTMLInputElement>;
        const datos: { etiqueta: string; valor: number }[] = [];
        controles.forEach(ctrl => {
            const valor = parseFloat((ctrl.value ?? '').replace(/\./g, '').replace(',', '.'));
            if (!isNaN(valor) && valor > 0) {
                const label = document.getElementById(`etiqueta-${ctrl.id}`);
                const etiqueta = (label?.textContent ?? ctrl.getAttribute(atControl.propiedad) ?? '').replace(/:$/, '').trim();
                datos.push({ etiqueta, valor });
            }
        });

        if (datos.length === 0) return;

        const contenedor = document.createElement('div');
        contenedor.id = idGrafica;

        if (Definido(divPadre)) {
            const anchoPadre = divPadre.offsetWidth || contenedorControles.offsetWidth;
            const altoSvg = 220;
            const separacion = 8;
            const margenSuperior = Math.max(divPadre.offsetHeight - contenedorControles.offsetHeight - altoSvg - separacion, separacion);
            contenedor.style.cssText = `width:${anchoPadre}px;margin-top:${margenSuperior}px;overflow-x:auto;display:flex;justify-content:center;`;
            contenedor.innerHTML = CrearSvgDeBarras(datos, anchoPadre);
            divPadre.appendChild(contenedor);
        } else {
            contenedor.style.cssText = `margin-top:12px;overflow-x:auto;display:flex;justify-content:center;`;
            contenedor.innerHTML = CrearSvgDeBarras(datos);
            contenedorControles.parentElement.insertBefore(contenedor, contenedorControles.nextSibling);
        }
    }

    function CrearSvgDeBarras(datos: { etiqueta: string; valor: number }[], anchoPadre: number = 0): string {
        const anchoTotal = anchoPadre > 0 ? anchoPadre : Math.max(datos.length * 80, 300);
        const alto = 220;
        const margenIzq = 60;
        const margenInf = 60;
        const margenSup = 10;
        const anchoBarra = 40;
        const separacion = (anchoTotal - margenIzq) / datos.length;
        const maxValor = Math.max(...datos.map(d => d.valor));
        const alturaDisponible = alto - margenInf - margenSup;

        const barras = datos.map((d, i) => {
            const alturaBarra = maxValor > 0 ? (d.valor / maxValor) * alturaDisponible : 0;
            const x = margenIzq + i * separacion + (separacion - anchoBarra) / 2;
            const y = margenSup + alturaDisponible - alturaBarra;
            const etiquetaX = x + anchoBarra / 2;
            const etiquetaRecortada = d.etiqueta.length > 10 ? d.etiqueta.substring(0, 9) + '…' : d.etiqueta;
            const valorFormateado = d.valor.toLocaleString('es-ES', { maximumFractionDigits: 2 });
            return `
                    <rect x="${x}" y="${y}" width="${anchoBarra}" height="${alturaBarra}" fill="#1976d2" rx="3"/>
                    <text x="${etiquetaX}" y="${y - 4}" text-anchor="middle" font-size="10" fill="#333">${valorFormateado}</text>
                    <text x="${etiquetaX}" y="${alto - margenInf + 14}" text-anchor="middle" font-size="10" fill="#555" transform="rotate(-25,${etiquetaX},${alto - margenInf + 14})">${etiquetaRecortada}</text>`;
        }).join('');

        const escalaY = CrearEscalaY(maxValor, alturaDisponible, margenIzq, margenSup);

        return `<svg xmlns="http://www.w3.org/2000/svg" width="${anchoTotal}" height="${alto}" style="display:block;">
                <line x1="${margenIzq}" y1="${margenSup}" x2="${margenIzq}" y2="${margenSup + alturaDisponible}" stroke="#aaa" stroke-width="1"/>
                <line x1="${margenIzq}" y1="${margenSup + alturaDisponible}" x2="${anchoTotal}" y2="${margenSup + alturaDisponible}" stroke="#aaa" stroke-width="1"/>
                ${escalaY}
                ${barras}
            </svg>`;
    }

    function CrearEscalaY(maxValor: number, alturaDisponible: number, margenIzq: number, margenSup: number): string {
        const pasos = 4;
        let resultado = '';
        for (let i = 0; i <= pasos; i++) {
            const valor = (maxValor / pasos) * i;
            const y = margenSup + alturaDisponible - (alturaDisponible / pasos) * i;
            const etiqueta = valor.toLocaleString('es-ES', { maximumFractionDigits: 0 });
            resultado += `
                    <line x1="${margenIzq - 4}" y1="${y}" x2="${margenIzq}" y2="${y}" stroke="#aaa" stroke-width="1"/>
                    <text x="${margenIzq - 6}" y="${y + 4}" text-anchor="end" font-size="9" fill="#777">${etiqueta}</text>`;
        }
        return resultado;
    }
}