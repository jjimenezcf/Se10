namespace ApiDeMenuFlotante {

    export function CerrarMf(panel: HTMLDivElement) {
        let contenedorMenu: NodeListOf<HTMLDivElement> = panel.querySelectorAll(`[${atControl.MenuFlotante}]`) as NodeListOf<HTMLDivElement>;
        for (let i = 0; i < contenedorMenu.length; i++) {
            let idMenu: string = contenedorMenu[i].getAttribute(atControl.MenuFlotante);
            let menu: HTMLUListElement = document.getElementById(idMenu) as HTMLUListElement;
            OcultarMenu(menu);
        }
    }

    export function OcultarLosMf(panel: HTMLDivElement) {
        let contenedores: NodeListOf<HTMLDivElement> = panel.querySelectorAll(`[${atControl.MenuFlotante}]`) as NodeListOf<HTMLDivElement>;
        for (let i = 0; i < contenedores.length; i++) {
            ApiPanel.OcultarPanel(contenedores[i]);
        }
    }
    export function MostrarLosMf(panel: HTMLDivElement) {
        let contenedores: NodeListOf<HTMLDivElement> = panel.querySelectorAll(`[${atControl.MenuFlotante}]`) as NodeListOf<HTMLDivElement>;
        for (let i = 0; i < contenedores.length; i++) {
            ApiPanel.MostrarPanel(contenedores[i]);
        }
    }

    export function AplicarModoAcceso(origen: string, panel: HTMLDivElement, seleccionados: number, permisos: ModoAcceso.enumModoDeAccesoDeDatos) {
        let contenedorMenu: NodeListOf<HTMLDivElement> = panel.querySelectorAll(`[${atControl.MenuFlotante}]`) as NodeListOf<HTMLDivElement>;
        for (let i = 0; i < contenedorMenu.length; i++) {
            let idMenu: string = contenedorMenu[i].getAttribute(atControl.MenuFlotante);
            let menu: HTMLUListElement = document.getElementById(idMenu) as HTMLUListElement;
            let opcionesLi: NodeListOf<HTMLLIElement> = menu.querySelectorAll("li") as NodeListOf<HTMLLIElement>;
            if (opcionesLi.length > 0) {
                for (let j = 0; j < opcionesLi.length; j++) {
                    let opcion: HTMLLIElement = opcionesLi[j];
                    let clase: string = opcion.getAttribute(atOpcionDeMenu.clase);
                    if (clase !== enumCssOpcionMenu.DeElemento) continue;

                    let hayQueHabilitar = ApiControl.EstaDeshabilitada(opcion)
                        ? ModoAcceso.HayQueHabilitar(seleccionados, opcion, permisos)
                        : !ModoAcceso.HayQueDesHabilitar(seleccionados, opcion, permisos);

                    HabilitarOpcionMf(opcion, hayQueHabilitar, origen);
                }
            }
        }
    }


    export function InicializarMfs(origen: string, panel: HTMLDivElement, claseOpcion: string, modoDeAccesoDelUsuario: ModoAcceso.enumModoDeAccesoDeDatos) {
        let contenedorMenu: NodeListOf<HTMLDivElement> = panel.querySelectorAll(`[${atControl.MenuFlotante}]`) as NodeListOf<HTMLDivElement>;
        for (let i = 0; i < contenedorMenu.length; i++) {
            let div: HTMLDivElement = contenedorMenu[i];
            InicializarMenuFlotante(div, origen, claseOpcion, modoDeAccesoDelUsuario);
        }
    }

    export function AplicarClienteWeb(panel: HTMLDivElement, seleccionadas: number) {
        if (NoDefinido(panel))
            return;
        MostrarOpcionDeMenuPorId(panel, ltrMenus.IdsDeOpcinesMf.idAlta, false);
        MostrarOpcionDeMenuPorId(panel, ltrMenus.IdsDeOpcinesMf.idBaja, false);
        MostrarOpcionDeMenuPorId(panel, ltrMenus.IdsDeOpcinesMf.idImprimir, false);
        MostrarOpcionDeMenuPorId(panel, ltrMenus.IdsDeOpcinesMf.idPermisos, false);
        MostrarOpcionDeMenuPorId(panel, ltrMenus.IdsDeOpcinesMf.idObservacion, false);
        if (seleccionadas === 1)
            DesbloquearOpcionDeMenu(panel, ltrMenus.OpcinesMf.enviarMail, ltrMenus.enumOrigen.crud);
        else
            BloquearOpcionDeMenu(panel, ltrMenus.OpcinesMf.enviarMail, ltrMenus.enumOrigen.crud);
    }

    export function AplicarBaja(origen: string, panel: HTMLDivElement, estaDeBaja: boolean, permiso: ModoAcceso.enumModoDeAccesoDeDatos) {
        if (NoDefinido(panel))
            return;
        let alta = MostrarOpcionDeMenuPorId(panel, ltrMenus.IdsDeOpcinesMf.idAlta, estaDeBaja);
        MostrarOpcionDeMenuPorId(panel, ltrMenus.IdsDeOpcinesMf.idBaja, !estaDeBaja);

        MostrarOpcionDeMenuPorId(panel, ltrMenus.IdsDeOpcinesMf.idObservacion, !estaDeBaja);
        MostrarOpcionDeMenuPorId(panel, ltrMenus.IdsDeOpcinesMf.idArchivador, !estaDeBaja);
        if (!Definido(alta))
            return;

        let activarOpcionAlta: boolean = ModoAcceso.HayPermisos(ModoAcceso.Parsear(ModoAcceso.ModoDeAccesoDeDatos.Gestor), permiso)
        if (activarOpcionAlta) {
            ApiControl.BloquearDesbloquearOpcionDeMf(alta, false);
            AsociarEvento(alta, "click", () => opcionDeMenuSeleccionada(alta));
            alta.setAttribute(ltrMenus.aQuienAfectaLaOpicionDeMenu, origen);
        }
        else {
            ApiControl.BloquearDesbloquearOpcionDeMf(alta, true);
            DesasociarEvento(alta, "click", () => opcionDeMenuSeleccionada(alta));
            alta.setAttribute(ltrMenus.aQuienAfectaLaOpicionDeMenu, origen);
        }
    }

    function MostrarOpcionDeMenuPorId(div: HTMLDivElement, id: string, mostrar: boolean): HTMLLIElement {
        let idMenu: string = div.getAttribute(atControl.MenuFlotante);
        let menu: HTMLUListElement = document.getElementById(idMenu) as HTMLUListElement;
        let opcionesLi: NodeListOf<HTMLLIElement> = menu.querySelectorAll("li") as NodeListOf<HTMLLIElement>;
        if (opcionesLi.length > 0) {
            for (let i = 0; i < opcionesLi.length; i++) {
                if (opcionesLi[i].id === id) {
                    opcionesLi[i].style.display = mostrar ? ltrStyle.display.block : ltrStyle.display.none;
                    return opcionesLi[i];
                }
            }
        }
    }

    export function BuscarMf(panel: HTMLDivElement, opcion: string): HTMLLIElement {
        let contenedorMenu: NodeListOf<HTMLDivElement> = panel.querySelectorAll(`[${atControl.MenuFlotante}]`) as NodeListOf<HTMLDivElement>;
        for (let i = 0; i < contenedorMenu.length; i++) {
            let div: HTMLDivElement = contenedorMenu[i];
            let idMenu: string = div.getAttribute(atControl.MenuFlotante);
            let menu: HTMLUListElement = document.getElementById(idMenu) as HTMLUListElement;
            BuscarOpcionMf(menu, opcion);
        }
        return null;
    }

    export function BuscarOpcionMf(menu: HTMLUListElement, opcion: string): HTMLLIElement {
        let opcionesLi: NodeListOf<HTMLLIElement> = menu.querySelectorAll("li") as NodeListOf<HTMLLIElement>;
        if (opcionesLi.length > 0) {
            for (let j = 0; j < opcionesLi.length; j++) {
                if (opcionesLi[j].getAttribute(ltrMenus.accion) === opcion)
                    return opcionesLi[j];
            }
        }
        return null;
    }

    export function InicializarMenuFlotante(div: HTMLDivElement, origen: string, claseOpcion: string, modoDeAccesoDelUsuario: ModoAcceso.enumModoDeAccesoDeDatos): void {

        if (NoDefinido(div))
            return;

        let idMenu: string = div.getAttribute(atControl.MenuFlotante);

        if (IsNullOrEmpty(idMenu)) {
            MensajesSe.Info(`No se ha indicado la propiedad '${atControl.MenuFlotante}' para el contenedor del menú con id ${div.id}`);
            return;
        }

        let menu: HTMLUListElement = document.getElementById(idMenu) as HTMLUListElement;
        if (NoDefinido(menu)) {
            MensajesSe.Info(`No se ha definido el menú flotante ${idMenu}`);
            return;
        }

        let opcionesLi: NodeListOf<HTMLLIElement> = menu.querySelectorAll("li") as NodeListOf<HTMLLIElement>;
        if (opcionesLi.length > 0) {
            AsociarEvento(div, "click", () => mostrarMenuFlotante(event, div));
            for (let j = 0; j < opcionesLi.length; j++) {
                InicializarOpcion(opcionesLi[j], origen, claseOpcion, modoDeAccesoDelUsuario);
            }
        }
    }

    function InicializarOpcion(li: HTMLLIElement, origen: string, claseOpcion: string, modoDeAccesoDelUsuario: ModoAcceso.enumModoDeAccesoDeDatos): void {
        let clase: string = li.getAttribute(atOpcionDeMenu.clase);
        if (clase !== claseOpcion)
            return;
        let modoAcceso: string = li.getAttribute(atOpcionDeMenu.permisosNecesarios);
        let hayPermisos: boolean = ModoAcceso.HayPermisos(ModoAcceso.Parsear(modoAcceso), modoDeAccesoDelUsuario);
        HabilitarOpcionMf(li, hayPermisos, origen);
    }

    export function HabilitarOpcionMf(opcion: HTMLLIElement, hayPermisos: boolean, origen: string): void {
        if (!Definido(opcion)) return;
        ApiControl.BloquearDesbloquearOpcionDeMf(opcion, !hayPermisos);
        if (hayPermisos) {
            AsociarEvento(opcion, "click", () => opcionDeMenuSeleccionada(opcion));
            opcion.setAttribute(ltrMenus.aQuienAfectaLaOpicionDeMenu, origen);
        }
        else {
            DesasociarEvento(opcion, "click", () => opcionDeMenuSeleccionada(opcion));
            opcion.setAttribute(ltrMenus.aQuienAfectaLaOpicionDeMenu, origen);
        }
    }


    export function ValidarSiSeMantieneActiva(opciones: NodeListOf<HTMLLIElement>, activas: string[], seleccionadas: number): void {
        for (var i = 0; i < opciones.length; i++) {
            let opcion: HTMLLIElement = opciones[i];
            if (ApiControl.EstaDeshabilitada(opcion))
                continue;

            let literal: string = opcion.getAttribute(ltrMenus.accion);
            if (activas.indexOf(literal) >= 0) {

                let permiteMultiSeleccion: string = opcion.getAttribute(atOpcionDeMenu.permiteMultiSeleccion);
                if (!EsTrue(permiteMultiSeleccion))
                    ApiControl.BloquearDesbloquearOpcionDeMf(opcion, !(seleccionadas === 1));

                if (EsTrue(permiteMultiSeleccion)) {
                    let numero: number = Numero(opcion.getAttribute(atOpcionDeMenu.numeroMaximoSeleccionable));
                    if (numero === -1 || seleccionadas <= numero)
                        ApiControl.BloquearDesbloquearOpcionDeMf(opcion, false);
                }
            }
        }
    }


    export function ValidarSiSeMantieneBloqueada(opciones: NodeListOf<HTMLLIElement>, desactivas: string[]): void {
        for (var i = 0; i < opciones.length; i++) {
            let opcion: HTMLLIElement = opciones[i];
            if (ApiControl.EstaDeshabilitada(opcion))
                continue;

            let literal: string = opcion.getAttribute(ltrMenus.accion);
            if (desactivas.indexOf(literal) >= 0)
                ApiControl.BloquearDesbloquearOpcionDeMf(opcion, true);
        }
    }

    export function BloquearOpcionDeMenuSi(panel: HTMLDivElement, opcion: string, origen: string, bloquear: boolean) {
        if (bloquear)
            BloquearOpcionDeMenu(panel, opcion, origen);
        else
            DesbloquearOpcionDeMenu(panel, opcion, origen);
    }

    export function DesbloquearOpcionDeMenuSi(panel: HTMLDivElement, opcion: string, origen: string, desbloquear: boolean) {
        if (!desbloquear)
            BloquearOpcionDeMenu(panel, opcion, origen);
        else
            DesbloquearOpcionDeMenu(panel, opcion, origen);
    }
    export function PermisosNecesarios(panel: HTMLDivElement, opcion: string): ModoAcceso.enumModoDeAccesoDeDatos {
        let menu: HTMLUListElement = ObtenerMenuDelContenedor(panel);
        let li: HTMLLIElement = ApiDeMenuFlotante.BuscarOpcionMf(menu, opcion);

        let modoAcceso: string = li.getAttribute(atOpcionDeMenu.permisosNecesarios);
        return ModoAcceso.Parsear(modoAcceso);
    }

    export function BloquearOpcionDeMenu(panel: HTMLDivElement, opcion: string, origen: string) {
        let menu: HTMLUListElement = ObtenerMenuDelContenedor(panel);
        let li: HTMLLIElement = ApiDeMenuFlotante.BuscarOpcionMf(menu, opcion);
        HabilitarOpcionMf(li, false, origen)
        //ApiControl.BloquearDesbloquearOpcionDeMf(li, true);
    }

    export function DesbloquearOpcionDeMenu(panel: HTMLDivElement, opcion: string, origen: string) {
        let menu: HTMLUListElement = ObtenerMenuDelContenedor(panel);
        let li: HTMLLIElement = ApiDeMenuFlotante.BuscarOpcionMf(menu, opcion);
        HabilitarOpcionMf(li, true, origen)
        //ApiControl.BloquearDesbloquearOpcionDeMf(li, false, );
    }

    export function OcultarOpcionDeMenu(panel: HTMLDivElement, opcion: string) {
        MostrarOpcionDeMenuSi(panel, opcion, false);
    }

    export function MostrarOpcionDeMenuSi(panel: HTMLDivElement, opcion: string, mostrar: boolean) {
        let menu: HTMLUListElement = ObtenerMenuDelContenedor(panel);
        let li: HTMLLIElement = ApiDeMenuFlotante.BuscarOpcionMf(menu, opcion);
        if (!Definido(li))
            return;
        if (mostrar === true) {
            ApiControl.ExcluirCss(li, enumCssOpcionMenu.OcultarLi);

            // Buscar hacia atrás el primer <hr> visible (sin clase OcultarHr)
            let prev = li.previousElementSibling as HTMLElement | null;
            while (prev) {
                if (prev.tagName === 'HR') {
                    ApiControl.ExcluirCss(prev, enumCssOpcionMenu.OcultarHr);
                    break;
                }
                if (prev && prev.tagName === 'LI' && !prev.classList.contains(enumCssOpcionMenu.OcultarLi))
                    break;
                prev = prev.previousElementSibling as HTMLElement | null;
            }
        }
        else {
            ApiControl.IncluirCss(li, enumCssOpcionMenu.OcultarLi);

            // Similar lógica para buscar hacia atrás <hr>
            let prev = li.previousElementSibling as HTMLElement | null;
            while (prev) {
                if (prev.tagName === 'HR') {
                    const next = li.nextElementSibling as HTMLElement | null;
                    if (next === null || next.tagName === 'HR') {
                        ApiControl.IncluirCss(prev, enumCssOpcionMenu.OcultarHr);
                    }
                    break; // Ya procesamos el primer <hr> relevante
                }
                if (prev && prev.tagName === 'LI' && !prev.classList.contains(enumCssOpcionMenu.OcultarLi))
                    break;
                prev = prev.previousElementSibling as HTMLElement | null;
            }
        }
    }
    export function OcultarUltimoHr(panel: HTMLDivElement): void {
        const menu: HTMLUListElement = ObtenerMenuDelContenedor(panel);

        // Función auxiliar para verificar si un <hr> es el 'último visible'
        const esUltimoHrVisible = (hrElement: Element | null): boolean => {
            if (!hrElement || hrElement.tagName !== 'HR' || hrElement.classList.contains(enumCssOpcionMenu.OcultarHr)) {
                return false;
            }

            let nextElement = hrElement.nextElementSibling as HTMLElement | null;

            // Se considera el 'último visible' si todo lo que le sigue hasta el final
            // son <li> ocultos o más <hr> ocultos (o no hay más elementos).
            while (nextElement) {
                // Si encontramos un <li> visible, este <hr> no es el último visible
                if (nextElement.tagName === 'LI' && !nextElement.classList.contains(enumCssOpcionMenu.OcultarLi)) {
                    return false;
                }

                // Si encontramos un <hr> visible, este <hr> no es el 'último visible'
                // ya que el visible podría ser el candidato a ocultar en una iteración posterior.
                if (nextElement.tagName === 'HR' && !nextElement.classList.contains(enumCssOpcionMenu.OcultarHr)) {
                    return false;
                }

                // Si llegamos a otro elemento (<li> o <hr>) oculto, o cualquier otro tag, seguimos buscando.
                nextElement = nextElement.nextElementSibling as HTMLElement | null;
            }

            // Si el bucle termina sin encontrar un <li> o <hr> visible, entonces este <hr> es el último visible.
            return true;
        };

        let hrOcultado: boolean = false;

        do {
            hrOcultado = false;

            // Iteramos todos los hijos del menú de atrás hacia adelante para encontrar el último visible <hr>
            let ultimoHrCandidato: HTMLElement | null = null;

            for (let i = menu.children.length - 1; i >= 0; i--) {
                const child = menu.children[i] as HTMLElement;

                // Si encontramos un <li> visible, salimos porque nada anterior puede ser el "último hr"
                if (child.tagName === 'LI' && !child.classList.contains(enumCssOpcionMenu.OcultarLi)) {
                    ultimoHrCandidato = null;
                    break;
                }

                // Si encontramos un <hr> *visible*, este es nuestro candidato actual para el último <hr> visible.
                if (child.tagName === 'HR' && !child.classList.contains(enumCssOpcionMenu.OcultarHr)) {
                    ultimoHrCandidato = child;
                    break;
                }
            }

            // Si encontramos un candidato y, al verificar con la función auxiliar,
            // confirmamos que es el último <hr> visible, lo ocultamos.
            if (ultimoHrCandidato && esUltimoHrVisible(ultimoHrCandidato)) {
                ApiControl.IncluirCss(ultimoHrCandidato, enumCssOpcionMenu.OcultarHr);
                hrOcultado = true; // Se ocultó uno, necesitamos volver a checar
            }

        } while (hrOcultado); // Repetir mientras se haya ocultado al menos un <hr> en la iteración anterior
    }

    // Nota: He asumido que tienes acceso a `ObtenerMenuDelContenedor`, `ApiControl`, `enumCssOpcionMenu`
    // y que la estructura del menú es una `<ul>` conteniendo `<li>` (opciones) y ` <hr> ` (separadores).
    export function CambiarNombre(panel: HTMLDivElement, opcion: string, nombre: string) {
        let menu: HTMLUListElement = ObtenerMenuDelContenedor(panel);
        let li: HTMLLIElement = ApiDeMenuFlotante.BuscarOpcionMf(menu, opcion);
        li.innerText = nombre;
    }

    export function QuitarOpcion(menu: HTMLUListElement, id: string) {
        var li = menu.querySelector(`li[id='${id}']`)
        menu.removeChild(li);
    }
    export function QuitarUltimoHr(menu: HTMLUListElement) {
        var br = menu.querySelector("hr:last-child");
        menu.removeChild(br);
    }

    export function IncluirOpcionPosteriorALaPosicion(menu: HTMLUListElement, posicion: number, id: string, titulo: string, origen: string, claseDeOpcion: string, accion: string = null): HTMLLIElement {

        let opciones: NodeListOf<HTMLLIElement> = menu.querySelectorAll("li:nth-child(n+" + posicion + ")");

        if (opciones.length > 0)
            for (let i = 0; i < opciones.length; i++) {
                let r = opciones[i].textContent.localeCompare(titulo);
                if (r == 0) return opciones[i];
                if (r > 0) {
                    return menu.insertBefore(CrearOpcion(id, accion, origen, titulo, claseDeOpcion), opciones[i]);
                }
            }
        return IncluirOpcion(menu, id, titulo, origen, accion);
    }

    export function IncluirOpcion(menu: HTMLUListElement, id: string, titulo: string, origen: string, claseDeOpcion: string, accion: string = null): HTMLLIElement {
        var opcion = CrearOpcion(id, accion, origen, titulo, claseDeOpcion);
        return menu.appendChild(opcion);
    }

    function CrearOpcion(id: string, accion: string, origen: string, titulo: string, claseDeOpcion: string): HTMLLIElement {
        var opcion = document.createElement("li");
        opcion.id = id;
        opcion.setAttribute(ltrMenus.accion, Definido(accion) ? accion : opcion.id);
        opcion.setAttribute(atControl.tipo, ltrMenus.opcion);
        opcion.setAttribute(atOpcionDeMenu.clase, claseDeOpcion);
        opcion.setAttribute('permite-multi-seleccion', 'False');
        opcion.setAttribute('numero-maximo-seleccionable', '1');
        opcion.setAttribute('evento-asociado', 'true');
        opcion.setAttribute(ltrMenus.aQuienAfectaLaOpicionDeMenu, origen);
        opcion.setAttribute(atOpcionDeMenu.permisosNecesarios, ModoAcceso.ModoDeAccesoDeDatos.Gestor);
        opcion.setAttribute(atControl.eventoJs.onclick, 'javascript:ApiDeMenuFlotante.OpcionDeMenuSeleccionada(this)');
        opcion.innerHTML = SanitizeHTML(titulo);
        return opcion;
    }

    function ObtenerMenuDelContenedor(panel: HTMLDivElement): HTMLUListElement {
        let idMenu: string = panel.getAttribute(atControl.MenuFlotante);
        if (IsNullOrEmpty(idMenu)) {
            MensajesSe.Info(`No se ha indicado la propiedad '${atControl.MenuFlotante}' para el contenedor del menú con id ${panel.id}`);
            return;
        }
        return document.getElementById(idMenu) as HTMLUListElement;
    }

    function ejecutarOpcionDeMenu(origen: string, opcion: string, esContextual: boolean): void {
        switch (origen) {
            case ltrMenus.enumOrigen.crud:
                Crud.EventosDelMantenimiento(ltrEventos.Mnt.OpcionMenuFlotante, `${opcion}#${esContextual}`);
                break;
            case ltrMenus.enumOrigen.edicion:
                Crud.EventosDelMfDeEdicion(opcion, esContextual);
                break;
            case ltrMenus.enumOrigen.creacion:
                Crud.EventosDelMfDeCreacion(opcion, esContextual);
                break;
            case ltrMenus.enumOrigen.formulario:
                Formulario.EventosDelFormulario(ltrEventos.Formulario.OpcionMenuFlotante, `${opcion}#${esContextual}`);
                break;
        }
    }

    function esOpcionDeMenu(elemento): boolean {
        while (elemento !== document) {
            if (elemento instanceof HTMLElement) {
                if (!IsNullOrEmpty((elemento as HTMLElement).getAttribute('menu-flotante')))
                    return true;
            }
            elemento = elemento.parentNode;
        }
        return false;
    }

    function mostrarOcultarMenu(menu, mostrar: boolean) {
        let b: boolean = EsTrue(menu.getAttribute('esta-mostrado'));
        if (b === false)
            MostrarMenu(menu);
        else
            OcultarMenu(menu);
    }

    export function MostrarMenu(menu: any) {
        EntornoSe.OcultarMenusRapidos();
        OcultarMfQueNoSea(menu);
        setTimeout(function () {
            menu.style.display = ltrStyle.display.block;
            menu.setAttribute('esta-mostrado', 'true');
            menu.style.opacity = 1;
        }, 200);
    }

    export function OcultarMenu(menu: any) {
        if (!Definido(menu))
            return;
        setTimeout(function () {
            menu.style.display = ltrStyle.display.none;
            menu.style.opacity = 0;
            menu.setAttribute('esta-mostrado', 'false');
        }, 30);
    }

    function AsociarEvento(objeto: HTMLDivElement | HTMLLIElement, evento, funcion) {
        if (EsTrue(objeto.getAttribute('evento-asociado')))
            return;
        objeto.addEventListener(evento, funcion);
        objeto.setAttribute('evento-asociado', 'true');
    }

    function DesasociarEvento(objeto: HTMLDivElement | HTMLLIElement, evento, funcion) {
        if (EsTrue(objeto.getAttribute('evento-asociado')))
            return;
        objeto.removeEventListener(evento, funcion);
        objeto.setAttribute('evento-asociado', 'false');
    }

    function OcultarMfQueNoSea(menu: any) {
        let contenedorMenu: NodeListOf<HTMLDivElement> = document.querySelectorAll(`[${atControl.MenuFlotante}]`) as NodeListOf<HTMLDivElement>;
        for (let i = 0; i < contenedorMenu.length; i++) {
            let idMenu: string = contenedorMenu[i].getAttribute(atControl.MenuFlotante);
            if (idMenu === menu.id)
                continue;
            let otroMenu = document.getElementById(idMenu);
            if (NoDefinido(otroMenu))
                continue;
            OcultarMenu(otroMenu);
        }
    }

    //function mostrarMenuFlotante(evento: any, elemento: HTMLDivElement): void {
    //    evento.preventDefault();
    //    var idMenu = elemento.getAttribute(atControl.MenuFlotante);
    //    var offsetX = Numero(elemento.getAttribute('offset-x'));
    //    let menu: HTMLUListElement = document.getElementById(idMenu) as HTMLUListElement;

    //    // Mostrar el menú temporalmente para calcular sus dimensiones
    //    menu.style.visibility = 'hidden';
    //    menu.style.display = 'block';

    //    // Calcular si el scroll está activado
    //    const tieneScroll = menu.scrollHeight > menu.clientHeight;

    //    // Calcular el ancho de la barra de scroll (normalmente alrededor de 17px)
    //    const scrollBarWidth = tieneScroll ? menu.offsetWidth - menu.clientWidth : 0;

    //    // Ajustar la posición left
    //    menu.style.left = `${evento.pageX - offsetX - scrollBarWidth- 25}px`;
    //    menu.style.top = `${evento.pageY + 10}px`;

    //    // Mostrar el menú
    //    menu.style.visibility = 'visible';
    //    mostrarOcultarMenu(menu, true);
    //}

    function mostrarMenuFlotante(evento: any, elemento: HTMLDivElement): void {
        evento.preventDefault();
        var idMenu = elemento.getAttribute(atControl.MenuFlotante);
        var offsetX = Numero(elemento.getAttribute('offset-x')); // Offset personalizado del elemento que dispara el menú
        let menu: HTMLUListElement = document.getElementById(idMenu) as HTMLUListElement;

        // 1. Mostrar el menú temporalmente con 'display: block' y 'visibility: hidden'
        // Esto es crucial para que el navegador calcule sus dimensiones reales (offsetWidth, offsetHeight).
        menu.style.visibility = 'hidden';
        menu.style.display = 'block';

        // 2. Obtener las dimensiones del menú y del viewport (ventana visible del navegador)
        const menuWidth = menu.offsetWidth; // Ancho total del menú, incluyendo padding, borde y scrollbar si está visible
        const menuHeight = menu.offsetHeight;
        const viewportWidth = window.innerWidth; // Ancho del área visible del navegador
        const viewportHeight = window.innerHeight; // Altura del área visible del navegador

        // 3. Calcular la posición inicial deseada del menú
        // Se mantiene el 'offsetX' y el '-25' original como ajustes visuales que ya tenías.
        // IMPORTANTE: Se elimina la resta de 'scrollBarWidth' aquí, ya que 'menuWidth' ya considera su espacio.
        let initialLeft = evento.pageX - offsetX - 25;
        let initialTop = evento.pageY + 10; // Posiciona el menú 10px debajo del cursor

        // 4. Definir un margen de seguridad para que el menú no se pegue exactamente a los bordes de la ventana
        const edgeBuffer = 10; // 10 píxeles de margen en todos los lados

        // 5. Ajustar la posición horizontal (left) para que el menú no se salga de la ventana
        let finalLeft = initialLeft;
        // Si el menú se sale por el lado derecho de la ventana
        if (initialLeft + menuWidth + edgeBuffer > viewportWidth) {
            finalLeft = viewportWidth - menuWidth - edgeBuffer; // Mueve el menú hacia la izquierda
        }
        // Asegurarse de que el menú no se salga por el lado izquierdo de la ventana
        finalLeft = Math.max(edgeBuffer, finalLeft); // Si finalLeft es menor que edgeBuffer, usa edgeBuffer

        // 6. Ajustar la posición vertical (top) para que el menú no se salga de la ventana
        let finalTop = initialTop;
        // Si el menú se sale por la parte inferior de la ventana
        if (initialTop + menuHeight + edgeBuffer > viewportHeight) {
            finalTop = viewportHeight - menuHeight - edgeBuffer; // Mueve el menú hacia arriba
        }
        // Asegurarse de que el menú no se salga por la parte superior de la ventana
        finalTop = Math.max(edgeBuffer, finalTop); // Si finalTop es menor que edgeBuffer, usa edgeBuffer

        // 7. Aplicar las posiciones finales calculadas al menú
        menu.style.left = `${finalLeft}px`;
        menu.style.top = `${finalTop}px`;

        // 8. Mostrar el menú con la posición final.
        // Asumo que 'mostrarOcultarMenu' maneja la transición de opacidad y el 'display' final a 'block'.
        menu.style.visibility = 'visible';
        mostrarOcultarMenu(menu, true);
    }

    export function OpcionDeMenuSeleccionada(opcion: HTMLLIElement) {
        opcionDeMenuSeleccionada(opcion);
    }

    function opcionDeMenuSeleccionada(opcion: HTMLLIElement) {
        if (eval("typeof(ejecutarOpcionDeMenu)==typeof(Function)") && !opcion.classList.contains(enumCssOpcionMenu.Bloqueada)) {
            let esContextual = opcion.getAttribute(atOpcionDeMenu.clase) !== enumCssOpcionMenu.DeElemento;
            ejecutarOpcionDeMenu(opcion.getAttribute(ltrMenus.aQuienAfectaLaOpicionDeMenu), opcion.getAttribute(ltrMenus.accion), esContextual);
        }
        let menu: HTMLUListElement = opcion.parentElement as HTMLUListElement;
        OcultarMenu(menu);
    }
}