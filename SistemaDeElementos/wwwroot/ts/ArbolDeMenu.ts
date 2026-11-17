namespace ArbolDeMenu {

    export function ObtenerDatosMenu(): { modalMenu: HTMLDivElement; estadoMenu: HTMLElement } {
        let estadoMenu: HTMLImageElement = document.getElementById('id-menu') as HTMLImageElement;
        let modalMenu: HTMLDivElement = undefined;
        if (!Definido(estadoMenu)) {
            EntornoSe.NavegarAtras();
        }
        else {
            let idModalMenu: string = estadoMenu.getAttribute('modal-menu');
            modalMenu = document.getElementById(idModalMenu) as HTMLDivElement;
        }
        return { modalMenu, estadoMenu };
    }

    export function MostrarMenu() {
        let { modalMenu, estadoMenu }: { modalMenu: HTMLDivElement; estadoMenu: HTMLElement; } = ObtenerDatosMenu();
        if (!Definido(modalMenu))
            return;

        var menuAbierto = estadoMenu.getAttribute(atMenu.abierto);
        if (NoDefinido(menuAbierto) || menuAbierto === literal.false) {
            estadoMenu.setAttribute(atMenu.abierto, literal.true);
            modalMenu.style.display = ltrStyle.display.block;
            modalMenu.style.height = `${AlturaDelMenu().toString()}px`;
            RestaurarAnchoDelMenu(modalMenu);
            EntornoSe.OcultarMenusRapidos();
        }
        else {
            CerrarMenu();
        }
    }

    const claveAnchoDelMenu: string = 'se-ancho-menu';
    const anchoDelBordeDeRestablecer: number = 16;
    let observadorDelAncho: ResizeObserver = undefined;

    // El menú ajusta su ancho al contenido (seMenu.css). Si el usuario lo ensancha arrastrando la esquina,
    // el navegador fija style.width: ese ancho se guarda en localStorage y se aplica al volver a abrir el menú.
    function RestaurarAnchoDelMenu(modalMenu: HTMLDivElement): void {
        try {
            const ancho: string = localStorage.getItem(claveAnchoDelMenu);
            if (ancho)
                modalMenu.style.width = ancho;
        }
        catch { /* sin localStorage el menú se ajusta al contenido */ }

        if (observadorDelAncho !== undefined || typeof ResizeObserver === 'undefined')
            return;

        // Doble clic en el borde derecho (donde se arrastra): se olvida el ancho y el menú vuelve a ajustarse al contenido
        modalMenu.addEventListener('dblclick', (event: MouseEvent) => {
            const borde: DOMRect = modalMenu.getBoundingClientRect();
            if (event.clientX < borde.right - anchoDelBordeDeRestablecer)
                return;
            event.preventDefault();
            modalMenu.style.width = '';
            try {
                localStorage.removeItem(claveAnchoDelMenu);
            }
            catch { /* nada que olvidar */ }
        });

        observadorDelAncho = new ResizeObserver(() => {
            const ancho: string = modalMenu.style.width;
            if (!ancho || modalMenu.style.display === ltrStyle.display.none)
                return;
            try {
                localStorage.setItem(claveAnchoDelMenu, ancho);
            }
            catch { /* no se puede recordar el ancho */ }
        });
        observadorDelAncho.observe(modalMenu);
    }

    export function CerrarMenu(): void {
        let { modalMenu, estadoMenu }: { modalMenu: HTMLDivElement; estadoMenu: HTMLElement; } = ObtenerDatosMenu();
        estadoMenu.setAttribute(atMenu.abierto, literal.false);
        modalMenu.style.display = ltrStyle.display.none;
    }

    export async function OpcionSeleccionada(idVistaMvc: string, controlador: string, accion: string, parametros: string, event: KeyboardEvent | MouseEvent) {
        const target = event.target as HTMLElement;
        let dentroDeUl = false;
        if (target) {
            let elem: HTMLElement | null = target;
            while (elem) {
                if (elem.id === ltrMenus.PanelDeControl.UltimosMenu) {
                    dentroDeUl = true;
                    break;
                }
                elem = elem.parentElement;
            }
        }

        if (!dentroDeUl) {
            MostrarMenu();
        }
        let url = EntornoSe.AbrirVista(controlador, accion, parametros, event);
        await GuardarMenuAccedido(idVistaMvc, parametros, url);
    }

    export async function UrlSeleccionada(url: string, event: KeyboardEvent | MouseEvent) {
        PonerCapa();
        EntornoSe.OcultarMenusRapidos();
        try {
            if (!Definido(event) || !event['ctrlKey'])
                EntornoSe.NavegarAUrl(url);
            else
                EntornoSe.AbrirPestana(url);
        }
        finally {
            QuitarCapa();
        }
    }

    export function MenuPulsado(id_menu_pulsado: string) {
        let menuHtmlPulsado: HTMLMenuElement = document.getElementById(id_menu_pulsado) as HTMLMenuElement;


        if (menuHtmlPulsado.getAttribute(atMenu.plegado) == literal.false) {
            plegarMenu(menuHtmlPulsado);
            return;
        }

        desplegarMenu(menuHtmlPulsado);

        let padreHtml: HTMLElement = menuHtmlPulsado.parentElement;
        while (padreHtml !== null) {
            if (padreHtml.constructor.toString().indexOf("HTMLUListElement") > 0)
                desplegarMenu(padreHtml as HTMLMenuElement);
            padreHtml = padreHtml.parentElement;
        }

    }

    export function SolicitarArbolDeMenu(idContenedorMenu: string): void {
        if (!Registro.HayArbolDeMenu()) {
            ApiDePeticiones.SolicitarArbolDelMenu(this, idContenedorMenu, new Array<Parametro>)
                .then((peticion) => DespuesDeSolitarMenu(peticion))
                .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
        }
        else {
            var htmlContenedorMenu = document.getElementById(idContenedorMenu);
            htmlContenedorMenu.innerHTML = Registro.ObtenerArbolDeMenu();
        }


    }

    function DespuesDeSolitarMenu(peticion: ApiDeAjax.DescriptorAjax): void {
        let idContenedorMenu: string = peticion.DatosDeEntrada["idContenedorMenu"]
        var htmlContenedorMenu = document.getElementById(`${idContenedorMenu}`);
        if (!htmlContenedorMenu) {
            MensajesSe.Apilar(MensajesSe.enumTipoMensaje.error, `No se ha localizado el contenedor ${idContenedorMenu}`);
            return;
        }
        htmlContenedorMenu.innerHTML = (peticion.resultado as ApiDeAjax.ResultadoHtml).html;
        Registro.GuardarArbolDeMenu(htmlContenedorMenu.innerHTML);
    }


    function desplegarMenu(menuHtml: HTMLMenuElement) {
        menuHtml.style.display = ltrStyle.display.block;
        menuHtml.compact = false;
        menuHtml.setAttribute(atMenu.plegado, literal.false);
    }

    function plegarMenu(menuHtml: HTMLMenuElement) {
        menuHtml.style.display = ltrStyle.display.none;
        menuHtml.compact = true;
        menuHtml.setAttribute(atMenu.plegado, literal.true);
    }

    //function desplegarMenu(menuHtml: HTMLMenuElement) {
    //    menuHtml.style.display = 'block';
    //    menuHtml.classList.remove('menu-plegado');
    //    menuHtml.classList.add('menu-desplegado');
    //    menuHtml.setAttribute('data-plegado', 'false');
    //}

    //function plegarMenu(menuHtml: HTMLMenuElement) {
    //    menuHtml.style.display = 'none';
    //    menuHtml.classList.remove('menu-desplegado');
    //    menuHtml.classList.add('menu-plegado');
    //    menuHtml.setAttribute('data-plegado', 'true');
    //}


}

namespace Modales {

    export function SolicitarModal(idModal: string): void {
        const idContenedor: string = `contenedor-${idModal}`;
        let contenedor: HTMLDivElement = document.getElementById(idContenedor) as HTMLDivElement;
        if (IsNullOrEmpty(contenedor.innerHTML)) {
            let modal = contenedor.querySelector('div') as HTMLDivElement;
            ApiPanel.AbrirModal(modal);
            return;
        }

        ApiDePeticiones.SolicitarModal(this, idContenedor, idModal, new Array<Parametro>)
            .then((peticion) => DespuesDeSolitarModal(peticion))
            .catch((peticion) => ApiDePeticiones.EmitirError(peticion));


        function DespuesDeSolitarModal(peticion: ApiDeAjax.DescriptorAjax): void {
            let idContenedor: string = peticion.DatosDeEntrada["idContenedor"];
            var contenedor: HTMLDivElement = document.getElementById(`${idContenedor}`) as HTMLDivElement;
            if (!contenedor) {
                MensajesSe.Apilar(MensajesSe.enumTipoMensaje.error, `No se ha localizado el contenedor ${idContenedor}`);
                return;
            }
            contenedor.innerHTML = (peticion.resultado as ApiDeAjax.ResultadoHtml).html;
            let modal = contenedor.querySelector('div') as HTMLDivElement;
            ApiPanel.AbrirModal(modal);
        }
    }


}






