namespace ApiDeJerarquia {

    export function DesSeleccionarNodo(panel: HTMLDivElement) {
        let lis: NodeListOf<HTMLLIElement> = panel.querySelectorAll(`li`) as NodeListOf<HTMLLIElement>;
        for (var i = 0; i < lis.length; i++)
            lis[i].classList.remove(ltrCss.nodoSeleccionado);
    }

    export function NodoPulsado(idNodo: string, alPulsar: Function): void {
        let nodo = document.getElementById(idNodo) as HTMLLIElement;

        let estabaPulsado: boolean = EsTrue(nodo.getAttribute('estaba-pulsada'));
        if (estabaPulsado) {
            nodo.setAttribute('estaba-pulsada', 'false');
            alPulsar();
        }
        else {
            nodo.setAttribute('estaba-pulsada', 'true');
            setTimeout(() => DesplegarRama(nodo), 500);
        }
    }

    function DesplegarRama(nodo: HTMLLIElement) {
        if (!EsTrue(nodo.getAttribute('estaba-pulsada')))
            return;

        nodo.setAttribute('estaba-pulsada', 'false');
        let nodosHijos = nodo.querySelectorAll("ul") as NodeListOf<HTMLUListElement>;
        for (let i = 0; i < nodosHijos.length; i++) {
            nodosHijos[i].style.display = nodosHijos[i].style.display == ltrStyle.display.none ? ltrStyle.display.block : ltrStyle.display.none;
        }
    }
}