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
            EntornoSe.OcultarMenusRapidos();
        }
        else {
            CerrarMenu();
        }
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
        PonerCapa();
        EntornoSe.OcultarMenusRapidos();
        try {
            let urlBase: string = window.location.origin;
            let pagina: string = `${urlBase}/${controlador}/${accion}?origen=menu`;
            let url: string = `${pagina}${IsNullOrEmpty(parametros) ? '' : `&${parametros.replace('|', '&')}`}`;

            if (accion === Ajax.Entorno.ArbolMenu.Inicializar)
                Registro.EliminarArbolDeMenu();

            if (parametros.indexOf('guid=0') >= 0)
                url = url.replace('guid=0', 'guid=' + generarUUID());

            await GuardarMenuAccedido(idVistaMvc, parametros, url);

            if (!Definido(event) || !event['ctrlKey'])
                EntornoSe.NavegarAUrl(url);
            else
                EntornoSe.AbrirPestana(url);
        }
        finally {
            QuitarCapa();
        }
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

    export function SolicitarModal(idContenedor: string, idModal: string): void {
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






