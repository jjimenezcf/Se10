
namespace ApiControl {

    export function EsVisible(control: HTMLElement | null): boolean {
        // 1. Caso base: Si el control es null, llegamos a la raíz y todo lo que revisamos está bien.
        if (control === null) {
            return true;
        }

        // 2. Obtener los estilos finales aplicados (incluyendo hojas de estilo)
        const estiloComputado = window.getComputedStyle(control);

        // 3. Comprobación principal: Si el 'display' efectivo es 'none'.
        // Esto resuelve el problema de los estilos definidos en CSS externos.
        if (estiloComputado.display === ltrStyle.display.none) {
            return false;
        }

        // 4. Comprobación de opacidad o visibilidad (opcional, pero útil para elementos ocultos)
        // Aunque el display no sea 'none', podría estar invisible por CSS.
        if (estiloComputado.visibility === ltrStyle.visibility.hidden || parseFloat(estiloComputado.opacity) === 0) {
            // Solo verificamos la opacidad si no está completamente oculto.
            // NOTA: Si necesitas que el control sea considerado "no visible" cuando
            // está fuera de la pantalla (ej. left: -9999px), necesitarías más lógica.
            // Pero para el caso de display/clase, esto es suficiente.
        }

        //// 5. Comprobación de clase específica (Tu lógica original para ltrCss.divNoVisible)
        //// Se mantiene por si esta clase tiene un propósito adicional en tu lógica de negocio.
        //if (control.classList.contains(ltrCss.divNoVisible)) {
        //    return false;
        //}

        // 6. Recursividad: Comprobar el padre.
        // Usamos control.parentElement. Esto es más seguro y maneja el caso de llegar al <html>
        return EsVisible(control.parentElement);
    }
    export function RemplazarCss(control: HTMLElement, cssActual: string, cssNueva: string): void {
        ExcluirCss(control, cssActual);
        IncluirCss(control, cssNueva);
    }

    export function IntercambiaCss(control: HTMLElement, cssUna: string, cssOtra: string): string {
        if (control.classList.contains(cssUna)) {
            ExcluirCss(control, cssUna);
            IncluirCss(control, cssOtra);
            return cssOtra;
        }
        ExcluirCss(control, cssOtra);
        IncluirCss(control, cssUna);
        return cssUna;
    }

    export function IncluirCss(control: HTMLElement, css: string): boolean {
        if (!Definido(control))
            return false;
        if (control.classList.contains(css))
            return false;
        control.classList.add(css);
        return true;
    }


    export function ResaltarControl(elemento: HTMLElement, clase: string) {

        const esControlValido = !Definido(elemento) || elemento instanceof HTMLInputElement ||
            elemento instanceof HTMLTextAreaElement ||
            elemento instanceof HTMLSelectElement;

        if (!esControlValido) {
            return;
        }

        const control = elemento as HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement;
        if (!Definido(control.value))
            return;

        if (control.classList.contains(ltrCss.Resalto.ConResalto)) {
            ApiControl.ExcluirCss(control, ltrCss.Resalto.Violeta);
            ApiControl.ExcluirCss(control, ltrCss.Resalto.Verde);
        }

        ApiControl.IncluirCss(control, ltrCss.Resalto.ConResalto);
        ApiControl.IncluirCss(control, clase);
        const valor = control.value;

        if (!(control instanceof HTMLSelectElement))
            // Añadir evento para quitar el borde azul cuado el usuario modifique el valor
            control.addEventListener('input', function () {
                if (control.value !== valor) {
                    ApiControl.ExcluirCss(control, clase);
                }
            });
        else
            control.addEventListener('change', function () {
                if (control.value !== valor) {
                    ApiControl.ExcluirCss(control, clase);
                }
            });
    }

    export function QuitarResalto(control: HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement): void {
        ApiControl.ExcluirCss(control, ltrCss.Resalto.Verde);
        ApiControl.ExcluirCss(control, ltrCss.Resalto.Violeta);
        ApiControl.ExcluirCss(control, ltrCss.ia.mapeado);
    }

    export function EsSoloLectura(control: HTMLElement): boolean {
        // 1. Comprobación inicial
        if (!Definido(control))
            return false;


        // 2. Comprobación de clase CSS
        if (control.classList.contains(ltrCss.soloLectura))
            return true;

        if (EsTrue(control.getAttribute(atControl.filtro)) || control.getAttribute('class') === 'fecha-flt')
            return false;

        // 3. Comprobación de atributo 'editable' (EXISTENTE)
        if (!EsTrue(control.getAttribute(atControl.editable)))
            return true;

        // 4. NUEVA COMPROBACIÓN: Propiedad 'disabled'
        // Esta propiedad es estándar en muchos elementos de formulario (input, button, select, etc.)
        if ((control as HTMLInputElement).disabled)
            return true;

        // 5. NUEVA COMPROBACIÓN: Propiedad 'readOnly'
        // Esta propiedad es estándar en inputs y textareas
        if ((control as HTMLInputElement).readOnly)
            return true;

        return false;
    }

    export function ExcluirCss(control: HTMLElement, css: string): boolean {
        if (!Definido(control))
            return false;
        if (!control.classList.contains(css))
            return false;
        control.classList.remove(css);
        return true;
    }

    export function EjecutarJs(id: string, texto: string, accion: Function): HTMLAnchorElement {
        let a: HTMLAnchorElement = document.createElement("a");
        a.id = id;
        a.onclick = () => accion();
        //a.classList.add(ltrCss.refJs);
        a.target = "_blank";
        let aTexto = document.createTextNode(texto);
        a.appendChild(aTexto);
        return a;
    }

    export function CrearCheck(id: string, clase: string): HTMLInputElement {
        let checkbox: HTMLInputElement = document.createElement('input') as HTMLInputElement;
        checkbox.type = 'checkbox';
        checkbox.id = id;
        if (Definido(clase)) checkbox.classList.add(clase);
        return checkbox
    }

    export function CrearRef(texto: string, accion: string, indicarTarget: boolean, ayuda: string = undefined): HTMLAnchorElement {
        if (IsNullOrEmpty(texto))
            return undefined;
        let a: HTMLAnchorElement = document.createElement("a");
        if (!IsNullOrEmpty(accion)) {
            a.setAttribute("href", accion);
            if (indicarTarget) a.target = "_blank";
        }
        let aTexto = document.createTextNode(texto);
        a.title = Definido(ayuda) ? ayuda : texto;
        a.appendChild(aTexto);
        return a;
    }

    export function CrearLiEnUl(ul: HTMLUListElement, idLi: string, texto: string, accion: string, menuFlotante: string, metadatos: string): HTMLLIElement {
        let a: HTMLAnchorElement = CrearRef(texto, accion, false);
        a.setAttribute(atControl.menuFlotante, menuFlotante);
        let li: HTMLLIElement = document.createElement("li");
        li.id = idLi;
        li.appendChild(a);
        li.setAttribute(atControl.metadatos, metadatos);
        ul.appendChild(li);
        return li;
    }

    export function CrearUlVacioEnLi(li: HTMLLIElement, idUl: string): HTMLUListElement {
        let ul: HTMLUListElement = document.createElement("ul");
        li.appendChild(ul);
        return ul;
    }

    export function CrearCanvas(panel: HTMLDivElement, idCanvas: string, claseCss: string): HTMLCanvasElement {
        let canvas: HTMLCanvasElement = document.createElement("canvas");
        canvas.id = idCanvas;
        if (!IsNullOrEmpty(claseCss))
            canvas.classList.add(claseCss);
        panel.appendChild(canvas);
        return canvas;
    }

    export function CrearDiv(panel: HTMLDivElement, idDiv: string, claseCss: string, ayuda: string = undefined): HTMLDivElement {
        let div: HTMLDivElement = document.createElement("div");
        div.id = idDiv;
        if (!IsNullOrEmpty(ayuda)) div.title = ayuda;
        if (!IsNullOrEmpty(claseCss))
            div.classList.add(claseCss);
        panel.appendChild(div);
        return div;
    }

    export function CrearVisorInfoArchivo(panel: HTMLDivElement, idDiv: string, claseCss: string, info: string, accionDeBorrar: Function): HTMLDivElement {
        let div: HTMLDivElement = CrearDiv(panel, idDiv, claseCss);
        CrearInfoArchivo(div, `info-${div.id}`, info);
        CrearBoton(div, `boton-borrar-${div.id}`, accionDeBorrar, ltrIconos.papelera, 'Eliminar archivo seleccionado', ltrCss.eliminarSeleccionado);
        panel.appendChild(div);
        return div;
    }

    export function CrearInfoArchivo(panel: HTMLDivElement, id: string, info: string): HTMLInputElement {
        let infoArchivo: HTMLInputElement = document.createElement("input") as HTMLInputElement;
        infoArchivo.id = id;
        infoArchivo.type = "text";
        infoArchivo.value = info;
        infoArchivo.setAttribute(atControl.tipo, ltrTipoControl.Editor);
        infoArchivo.classList.add(ltrCss.infoArchivo);
        panel.appendChild(infoArchivo);
        return infoArchivo;
    }

    export function CrearImagen(panel: HTMLDivElement, idImagen: string, claseCss: string): HTMLImageElement {
        let imagen: HTMLImageElement = document.createElement("img");
        imagen.id = idImagen;
        if (!IsNullOrEmpty(claseCss))
            imagen.classList.add(claseCss);
        panel.appendChild(imagen);
        return imagen;
    }

    function CrearBoton(divopciones: HTMLDivElement, id: string, accion: Function, icono: string, ayuda: string, css: string): HTMLButtonElement {
        let boton: HTMLButtonElement = document.createElement("button");
        boton.classList.add(css);
        boton.onclick = () => accion();
        boton.title = ayuda;
        boton.id = id;
        let imagen: HTMLImageElement = document.createElement("img");
        let puerto = !IsNullOrEmpty(window.location.port) ? `:${window.location.port}` : '';
        imagen.src = `${window.location.protocol}//${window.location.hostname}${puerto}/images/menu/${icono}`;
        boton.appendChild(imagen);
        divopciones.appendChild(boton);
        return boton;
    }

    function CambiarIcono(boton: HTMLButtonElement, icono: string) {
        const imageElement = boton.getElementsByTagName('img')[0];
        imageElement.remove();
        let imagen: HTMLImageElement = document.createElement("img");
        let puerto = !IsNullOrEmpty(window.location.port) ? `:${window.location.port}` : '';
        imagen.src = `${window.location.protocol}//${window.location.hostname}${puerto}/images/menu/${icono}`;
        boton.appendChild(imagen);
    }

    export function CrearVisor(ctdArchivosAnexados: HTMLDivElement, idVisorDelArchivo: string, idCtdDelArchivo: string, divCss: string,
        imagenCss, archivoDto: any, accionDeMostrar: Function,
        accionDeDescarga: Function,
        accionDeBorrado: Function,
        accionDeFirmar: Function,
        accionDeBloquear: Function): [visor: HTMLDivElement, imagen: HTMLImageElement] {

        let idArchivo: number = Numero(ObtenerPropiedad(archivoDto, ltrPropiedades.Elemento.Id));
        let nombre: string = ObtenerPropiedad(archivoDto, ltrPropiedades.Elemento.Nombre);

        let estaCancelado = nombre.startsWith(ltrSimbolos.ArchivoCancelado) || ObtenerPropiedad(archivoDto, ltrPropiedades.Elemento.EstaCancelada, false);
        if (estaCancelado) {
            nombre = nombre.replace(ltrSimbolos.ArchivoCancelado, '');
        }
        let delSistema: boolean = ObtenerPropiedad(archivoDto, ltrPropiedades.DelSistema, false);
        let padreBloqueado: boolean = ObtenerPropiedad(archivoDto, ltrPropiedades.SisDoc.Archivo.PadreBloqueado, false);
        let visorArchivo: HTMLDivElement = CrearDiv(ctdArchivosAnexados, idVisorDelArchivo, divCss);
        let divEtiqueta: HTMLDivElement = CrearDiv(visorArchivo, `contenedor-ref-archivo-${idArchivo}`, ltrCss.contenedorVisorRef, 'Muestra datos o previsualiza el archivo si el visor está abierto') as HTMLDivElement;
        let divImagen: HTMLDivElement = CrearDiv(visorArchivo, `contenedor-imagen-archivo-${idArchivo}`, ltrCss.contenedorVisorImg) as HTMLDivElement;
        let divOpciones: HTMLDivElement = CrearDiv(visorArchivo, `contenedor-opciones-archivo-${idArchivo}`, ltrCss.contenedorDeOpcion) as HTMLDivElement;

        let a: HTMLAnchorElement = EjecutarJs(`refJs-${idArchivo}`, nombre, accionDeMostrar);
        let check: HTMLInputElement = CrearCheck(`check-${idArchivo}`, null);
        divEtiqueta.appendChild(check);
        divEtiqueta.appendChild(a);
        if (estaCancelado) {
            a.classList.add(ltrCss.Archivos.Cancelado);
        }

        let boton_descarga = CrearBoton(divOpciones, `${ltrEventos.Archivo.Descargar}-${idArchivo}`, accionDeDescarga, ltrIconos.descargar, 'descargar archivo', ltrCss.descargarAnexado);
        let boton_borrado = CrearBoton(divOpciones, `${ltrEventos.Archivo.Eliminar}-${idArchivo}`, accionDeBorrado, ltrIconos.papelera, 'Eliminar archivo', ltrCss.borrarAnexado);
        let boton_firma = CrearBoton(divOpciones, `${ltrEventos.Archivo.Firmar}-${idArchivo}`, accionDeFirmar, ltrIconos.firmar, 'firmar archivo', ltrCss.firmarArchivo);
        let boton_bloquear = CrearBoton(divOpciones, `${ltrEventos.Archivo.Bloquear}-${idArchivo}`, accionDeBloquear, ltrIconos.bloquearArchivo, 'bloquear archivo', ltrCss.bloquearArchivo);


        var esDeUnArchivadorVinculado = EsTrue(ObtenerPropiedad(archivoDto, ltrPropiedades.SisDoc.Archivo.EsDeUnArchivadorVinculado, false));
        if (esDeUnArchivadorVinculado || estaCancelado || delSistema || padreBloqueado) {
            boton_borrado.style.display = ltrStyle.display.none;
            boton_firma.style.display = ltrStyle.display.none;
            boton_bloquear.style.display = ltrStyle.display.none;
            //if (EsDeUnArchivador || estaCancelado || delSistema)
            //    check.style.display = ltrStyle.display.none;
            // boton_descarga.style.display = ltrStyle.display.none;
        }
        else {
            var bloqueado = EsTrue(ObtenerPropiedad(archivoDto, ltrPropiedades.SisDoc.Archivo.EstaBloqueado, false));
            if (bloqueado) {
                boton_bloquear.classList.add(ltrCss.desbloquearArchivo);
                boton_bloquear.title = 'Desbloquear archivo';
                CambiarIcono(boton_bloquear, ltrIconos.desbloquearArchivo);
                boton_borrado.style.display = ltrStyle.display.none;
                boton_firma.style.display = ltrStyle.display.none;
            }
            visorArchivo.setAttribute(atArchivo.bloqueado, bloqueado ? 'true' : 'false');

            var idOriginal = Numero(ObtenerPropiedad(archivoDto, ltrPropiedades.SisDoc.Archivo.idOriginal));
            if (idOriginal > 0) {
                boton_firma.classList.add(ltrCss.archivoFirmado);
                boton_firma.title = 'Mostrar datos de la firma';
                boton_borrado.style.display = ltrStyle.display.none;
            }
            visorArchivo.setAttribute(atArchivo.firmado, idOriginal > 0 ? 'true' : 'false');

            let tieneAuditoriaDeBloqueo: boolean = !IsNullOrEmpty(ObtenerPropiedad(archivoDto, ltrPropiedades.SisDoc.Archivo.Auditoria, ''));
            if (tieneAuditoriaDeBloqueo)
                boton_borrado.style.display = ltrStyle.display.none
        }

        return [visorArchivo, CrearImagen(divImagen, idCtdDelArchivo, imagenCss)];
    }

    export function CrearUnContenedorConCanvas(panel: HTMLDivElement, idCanvas: string, divCss: string, canvasCss): HTMLCanvasElement {
        let div: HTMLDivElement = CrearDiv(panel, `contenedor-${idCanvas}`, divCss) as HTMLDivElement;
        return CrearCanvas(div, idCanvas, canvasCss);
    }

    export function Expansor_OcultarMostrar(idHtmlExpansor: string, idHtmlBloque: string): void {
        let extensor: HTMLInputElement = document.getElementById(`${idHtmlExpansor}`) as HTMLInputElement;
        if (EsMayorDeCero(extensor.value)) {
            extensor.value = "0";
            ApiPanel.OcultarPanel(document.getElementById(`${idHtmlBloque}`) as HTMLDivElement);
            ApiPanel.OcultarPanel(document.getElementById(`${idHtmlBloque.replace('cuerpo', 'pie')}`) as HTMLDivElement);
        }
        else
            Expansor_Mostrar(idHtmlExpansor, idHtmlBloque);
    }

    export function MostrarMcrRefSi(id: string, mostrar: boolean): void {
        const ref = document.getElementById(id);
        ref.style.display = mostrar
            ? ltrStyle.display.block
            : ltrStyle.display.none;
        ApiPanel.OcultarMostrarPanel((ref.parentElement as HTMLDivElement), !mostrar);
    }

    export function MostrarPropiedadSi(panel: HTMLDivElement, propiedad: string, criterio: boolean, recolocarControles = false): HTMLElement {
        var control = BuscarControl(panel, propiedad, true);
        MostrarControlSi(control, criterio, recolocarControles);
        return control;
    }

    export function OcultarControlSi(control: HTMLElement, criterio: boolean, recolocarControles = false): HTMLElement {
        return MostrarControlSi(control, !criterio, recolocarControles);
    }

    export function MostrarControlSi(control: HTMLElement, criterio: boolean, recolocarControles = false): HTMLElement {
        const esControlDeEdicion = Definido(control.parentElement) && control.parentElement.getAttribute(atControl.name) === atNombre.contenedorControl;
        const esControlDeFiltro = Definido(control.parentElement) && Definido(control.parentElement.parentElement) && control.parentElement.parentElement.classList.contains('columna-filtro');

        if (esControlDeEdicion || esControlDeFiltro) {
            const celda = control.parentElement.parentElement as HTMLDivElement;

            if (criterio && celda.classList.contains(ltrCss.divNoVisible))
                celda.classList.remove(ltrCss.divNoVisible);
            if (!criterio && !celda.classList.contains(ltrCss.divNoVisible))
                celda.classList.add(ltrCss.divNoVisible);

            if (recolocarControles) {
                const contenedorDeCelda = celda.parentElement as HTMLDivElement;
                const posicionCelda = ObtenerPosicionDeLaCelda(contenedorDeCelda.id);
                if (!Definido(posicionCelda)) return;

                const fila = contenedorDeCelda.parentElement as HTMLDivElement;
                mostrarCeldaEnFila(fila, posicionCelda, criterio);
            }

        }
        return control;
    }

    function mostrarCeldaEnFila(fila: HTMLDivElement, posicion: number, mostrar: boolean): boolean {

        // 1. Salida temprana si 'fila' o 'posicion' no están definidos
        // En JavaScript, 0 es 'falsy', por lo que debemos verificar el tipo de 'posicion'.
        if (!fila || typeof posicion !== 'number') {
            return false;
        }

        // 2. Verificar si gridTemplateColumns está definido
        const columnasString = fila.style.gridTemplateColumns;
        if (!columnasString) {
            return false;
        }

        // 3. Obtener las definiciones de las columnas
        // Se divide la cadena (e.g., "1fr 1fr 0fr 1fr") en un array de definiciones (e.g., ["1fr", "1fr", "0fr", "1fr"])
        const definiciones = columnasString.split(/\s+/).filter(d => d.trim() !== '');

        // 4. Validar si la posición existe en las definiciones
        if (posicion >= definiciones.length) {
            return false; // La posición está fuera del rango de columnas definidas.
        }

        // 5. Sustituir la definición de la columna
        const nuevoAncho = mostrar ? '1fr' : '0fr';
        definiciones[posicion] = nuevoAncho;

        // 6. Reaplicar la nueva definición al estilo de la fila
        fila.style.gridTemplateColumns = definiciones.join(' ');

        return true; // Operación exitosa
    }

    function ObtenerPosicionDeLaCelda(idCelda): number | null {
        const ultimoGuionBajo = idCelda.lastIndexOf('_');
        const resultado = idCelda.substring(ultimoGuionBajo + 1);
        if (EsNumeroNoNulo(resultado)) {
            return Numero(resultado);
        }
        return null;
    }

    export function ColSpan(panel: HTMLDivElement, propiedad: string, columnas: number) {
        var celda = ApiPanel.BuscarCeldaPorPropiedad(panel, propiedad);
        //var control = BuscarControl(panel, propiedad, true);
        //control.parentElement.parentElement.parentElement.setAttribute('colspan', `${Numero(columnas)}`);
        celda.setAttribute('colspan', columnas.toString());
    }


    export function Expansor_Mostrar(idHtmlExpansor: string, idHtmlBloque: string): void {
        let extensor: HTMLInputElement = document.getElementById(`${idHtmlExpansor}`) as HTMLInputElement;
        if (!EsMayorDeCero(extensor.value)) {
            extensor.value = "1";
            ApiPanel.MostrarPanel(document.getElementById(`${idHtmlBloque}`) as HTMLDivElement);
            ApiPanel.MostrarPanel(document.getElementById(`${idHtmlBloque.replace('cuerpo', 'pie')}`) as HTMLDivElement);

            let cuerpo: HTMLDivElement = document.getElementById(`${idHtmlBloque}`) as HTMLDivElement;
            let tabla: HTMLDivElement = cuerpo.querySelector('table') as HTMLDivElement;
            //if (Definido(tabla)) {
            //    ApiDeGrid.ResetearAnchoDeTabla(tabla);
            //}
        }
    }

    export function BloquearMenu(panel: HTMLDivElement): void {
        let opciones: NodeListOf<HTMLButtonElement> = panel.querySelectorAll(`input[${atControl.tipo}="${ltrTipoControl.opcion}"]`) as NodeListOf<HTMLButtonElement>;
        for (var i = 0; i < opciones.length; i++) {
            let opcion: HTMLButtonElement = opciones[i];
            let clase: string = opcion.getAttribute(atOpcionDeMenu.clase);
            if (clase === enumCssOpcionMenu.Basico)
                continue;
            bloquearOpcionDeMenu(opcion, true);
        }
    }

    export function OcultarOpcionDeMenuPorNombre(panel: HTMLDivElement, nombreOpcion: string): boolean {
        let opcion: HTMLButtonElement = buscarOpcionDeMenu(panel, nombreOpcion);
        if (Definido(opcion)) {
            ocultarOpcionDeMenu(opcion, true);
            return true;
        }
        return false;
    }

    export function OcultarOpcionDeMenuPorId(id: string): boolean {
        let opcion: HTMLButtonElement = document.getElementById(id) as HTMLButtonElement;
        if (Definido(opcion)) {
            ocultarOpcionDeMenu(opcion, true);
            return true;
        }
        return false;
    }

    export function CambiarLiteralDeMenuPorNombre(panel: HTMLDivElement, nombreOpcion: string, nuevoNombre: string): boolean {
        let opcion: HTMLButtonElement = buscarOpcionDeMenu(panel, nombreOpcion);
        return CambiarLiteralDeMenu(opcion, nuevoNombre);
    }


    export function CambiarLiteralDeIdMenu(id: string, nuevoNombre: string): boolean {
        let opcion: HTMLButtonElement = document.getElementById(id) as HTMLButtonElement;
        return CambiarLiteralDeMenu(opcion, nuevoNombre);
    }

    export function CambiarLiteralDeMenu(opcion: HTMLButtonElement, nuevoNombre: string): boolean {
        if (Definido(opcion)) {
            opcion.value = nuevoNombre;
            opcion.title = nuevoNombre;
            return true;
        }
        return false;
    }
    export function MostrarOpcionDeMenuPorId(id: string): boolean {
        let opcion: HTMLButtonElement = document.getElementById(id) as HTMLButtonElement;
        if (Definido(opcion)) {
            ocultarOpcionDeMenu(opcion, false);
            return true;
        }
        return false;
    }

    export function MostrarOpcionDeMenuPorNombre(panel: HTMLDivElement, nombreOpcion: string): boolean {
        let opcion: HTMLButtonElement = buscarOpcionDeMenu(panel, nombreOpcion);
        if (Definido(opcion)) {
            ocultarOpcionDeMenu(opcion, false);
            return true;
        }
        return false;
    }

    export function OcultarMostrarOpcionDeMenuPorId(id: string, ocultar: boolean): boolean {
        let opcion: HTMLButtonElement = document.getElementById(id) as HTMLButtonElement;
        if (Definido(opcion)) {
            OcultarMostrarOpcionDeMenu(opcion, ocultar);
            return true;
        }
        return false;
    }

    export function OcultarMostrarOpcionDeMenu(opcion: HTMLButtonElement, ocultar: boolean): void {
        ocultarOpcionDeMenu(opcion, ocultar);
    }

    export function BloquearDesbloquearOpcionDeMf(opcion: HTMLLIElement, bloquear: boolean): void {
        if (bloquear && !opcion.classList.contains(enumCssOpcionMenu.Bloqueada))
            opcion.classList.add(enumCssOpcionMenu.Bloqueada);

        if (!bloquear && opcion.classList.contains(enumCssOpcionMenu.Bloqueada))
            opcion.classList.remove(enumCssOpcionMenu.Bloqueada);

        //opcion.style.color = bloquear ? 'gainsboro' : '';
    }

    export function EstaDeshabilitada(opcion: HTMLLIElement): boolean {
        return opcion.classList.contains(enumCssOpcionMenu.Bloqueada); // .style.color === 'gainsboro';
    }

    export function BloquearDesbloquearOpcionDeMenu(opcion: HTMLButtonElement, bloquear: boolean): void {
        bloquearOpcionDeMenu(opcion, bloquear);
    }

    export function BloquearOpcionDeMenuPorId(id: string): boolean {
        let opcion: HTMLButtonElement = document.getElementById(id) as HTMLButtonElement;
        if (Definido(opcion)) {
            bloquearOpcionDeMenu(opcion, true);
            return true;
        }
        return false;
    }

    export function BloquearOpcionDeMenuPorNombre(panel: HTMLDivElement, nombreOpcion: string): boolean {
        let opcion: HTMLButtonElement = buscarOpcionDeMenu(panel, nombreOpcion);
        if (Definido(opcion)) {
            bloquearOpcionDeMenu(opcion, true);
            return true;
        }
        return false;
    }

    export function DesbloquearOpcionDeMenuPorNombre(panel: HTMLDivElement, nombreOpcion: string): boolean {
        let opcion: HTMLButtonElement = buscarOpcionDeMenu(panel, nombreOpcion);
        if (Definido(opcion)) {
            bloquearOpcionDeMenu(opcion, false);
            return true;
        }
        return false;
    }
    export function DesbloquearOpcionDeMenuPorId(id: string): boolean {
        let opcion: HTMLButtonElement = document.getElementById(id) as HTMLButtonElement;
        if (Definido(opcion)) {
            bloquearOpcionDeMenu(opcion, false);
            return true;
        }
        return false;
    }


    export function BuscarValorDelAtributo(control: HTMLElement | null, atributo: string): string | undefined {
        // Si llegamos a un nodo nulo (encima del <html>), no se encontró
        if (!control) {
            return undefined;
        }

        // Comprobamos si el control actual tiene el atributo
        if (control.hasAttribute(atributo)) {
            return control.getAttribute(atributo) ?? undefined;
        }

        // Llamada recursiva al padre
        return BuscarValorDelAtributo(control.parentElement, atributo);
    }

    function buscarOpcionDeMenu(panel: HTMLDivElement, nombreOpcion: string): HTMLButtonElement {
        let opciones: NodeListOf<HTMLButtonElement> = panel.querySelectorAll(`input[${atControl.tipo}="${ltrTipoControl.opcion}"]`) as NodeListOf<HTMLButtonElement>;
        for (var i = 0; i < opciones.length; i++) {
            let opcion: HTMLButtonElement = opciones[i];
            if (opcion.value === nombreOpcion)
                return opcion;
        }
        return null;
    }

    function bloquearOpcionDeMenu(opcion: HTMLButtonElement, bloquear: boolean): void {
        opcion.disabled = bloquear;
        opcion.setAttribute(atOpcionDeMenu.bloqueada, bloquear ? "S" : "N");
    }

    function ocultarOpcionDeMenu(opcion: HTMLButtonElement, ocultar: boolean): void {
        opcion.hidden = ocultar;
        opcion.setAttribute(atOpcionDeMenu.oculta, ocultar ? "S" : "N");
    }

    export function EstaBloqueada(opcion: HTMLButtonElement) { return opcion.getAttribute(atOpcionDeMenu.bloqueada) === "S" || opcion.disabled; }

    export function EstaOculta(opcion: HTMLButtonElement) { return opcion.getAttribute(atOpcionDeMenu.oculta) === "S" || opcion.hidden; }

    export function BloquearListaDeElementoSi(panel: HTMLDivElement, propiedad: string, condicion: boolean, anularSeleccion: boolean = false, fijarElPrimero: boolean = false): void {
        if (condicion)
            BloquearListaDeElemento(panel, propiedad, anularSeleccion);
        else
            DesbloquearListaDeElemento(panel, propiedad);
    }

    export function BloquearListaDeElemento(panel: HTMLDivElement, propiedad: string, anularSeleccion: boolean = false): boolean {
        let lista: HTMLSelectElement = BuscarListaDeElementos(panel, propiedad, atControl.propiedad);
        if (Definido(lista)) {
            ApiControl.BloquearLaLista(lista, true);
            if (anularSeleccion) lista.selectedIndex = 0;
        }
        return false;
    }

    export function EstaBloqueadaLaLista(lista: HTMLSelectElement): boolean { return EsTrue(lista.getAttribute("readOnly")) || lista.disabled; }

    export function BloquearLaLista(lista: HTMLSelectElement, bloquear: boolean): void {
        lista.disabled = bloquear;
        lista.setAttribute("readOnly", `${bloquear ? 'true' : 'false'}`);
        if (bloquear) {
            MapearAlControl.DesfijarElPrimero(lista);
            let cargarBajoDemanda: boolean = EsTrue(lista.getAttribute(atListasDeElemento.cargarBajoDemanda));
            if (cargarBajoDemanda) {
                ApiControl.QuitarOpcionesDeLalista(lista);
            }
        }
        else {
            MapearAlControl.FijarElPrimero(lista);
        }
    }

    export function DesbloquearListaDeElemento(panel: HTMLDivElement, propiedad: string): boolean {
        let lista: HTMLSelectElement = BuscarListaDeElementos(panel, propiedad, atControl.propiedad);
        if (!NoDefinido(lista) && EsTrue(lista.getAttribute(atControl.editable))) {
            BloquearLaLista(lista, false);
            return true;
        }
        return false;
    }

    export function BloquearListaDeValores(panel: HTMLDivElement, propiedad: string): boolean {
        let lista: HTMLSelectElement = BuscarListaDeValores(panel, propiedad, atControl.propiedad);
        if (!NoDefinido(lista)) {
            lista.disabled = true;
            lista.setAttribute("readOnly", "true");
            return true;
        }
        return false;
    }

    export function DesbloquearListaDeValores(panel: HTMLDivElement, propiedad: string): boolean {
        let lista: HTMLSelectElement = BuscarListaDeValores(panel, propiedad, atControl.propiedad);
        return DesbloquearLaListaDeValores(lista);
    }
    export function DesbloquearLaListaDeValores(lista: HTMLSelectElement): boolean {
        if (!NoDefinido(lista) && EsTrue(lista.getAttribute(atControl.editable))) {
            lista.disabled = false;
            lista.setAttribute("readOnly", "false");
            return true;
        }
        return false;
    }
    export function AgregarOpcionAlfabeticamente(selectElement: HTMLSelectElement, valor: string, texto: string) {
        const nuevaOpcion = document.createElement('option');
        nuevaOpcion.value = valor;
        nuevaOpcion.text = texto;

        // Convertir las opciones a un array para poder ordenarlas
        const opciones = Array.from(selectElement.options);

        // Encontrar la posición correcta para insertar la nueva opción
        const indiceInsercion = opciones.findIndex(opcion =>
            opcion.text.localeCompare(texto, undefined, { sensitivity: 'base' }) > 0
        );

        if (indiceInsercion === -1) {
            // Si no se encontró una posición, añadir al final
            selectElement.add(nuevaOpcion);
        } else {
            // Insertar en la posición correcta
            selectElement.add(nuevaOpcion, indiceInsercion + 1);
        }
    }

    export function BloquearListaDinamicaSi(panel: HTMLDivElement, propiedad: string, criterio: boolean): HTMLInputElement {
        let lista: HTMLInputElement = BuscarListaDinamica(panel, propiedad, atControl.propiedad);
        if (Definido(lista)) {
            if (criterio)
                BloquearListaDinamica(lista);
            else
                DesbloquearListaDinamica(lista);
            return lista;
        }
        return undefined;
    }

    export function BlanquearBloquearListaDinamicaPorPropiedad(panel: HTMLDivElement, propiedad: string): HTMLInputElement {
        let lista: HTMLInputElement = BuscarListaDinamica(panel, propiedad, atControl.propiedad);
        if (Definido(lista)) {
            ApiListaDinamica.Blanquear(lista);
            BloquearListaDinamica(lista);
            return lista;
        }
        return undefined;
    }

    export function BloquearListaDinamicaPorPropiedad(panel: HTMLDivElement, propiedad: string): HTMLInputElement {
        let lista: HTMLInputElement = BuscarListaDinamica(panel, propiedad, atControl.propiedad);
        if (Definido(lista)) {
            BloquearListaDinamica(lista);
            return lista;
        }
        return undefined;
    }

    export function BloquearListaDinamica(lista: HTMLInputElement): void {
        lista.disabled = true;
        lista.readOnly = true;
    }

    export function BlanquearListaDinamicaPorPropiedad(panel: HTMLDivElement, propiedad: string): HTMLInputElement {
        let lista: HTMLInputElement = BuscarListaDinamica(panel, propiedad, atControl.propiedad);
        if (Definido(lista)) {
            ApiListaDinamica.Blanquear(lista);
            return lista;
        }
        return undefined;
    }

    export function BloquearSelectorDeFechaPorPropiedad(panel: HTMLDivElement, propiedad: string): HTMLInputElement {
        let fecha: HTMLInputElement = BuscarSelectorDeFecha(panel, propiedad, atControl.propiedad);
        if (Definido(fecha)) {
            fecha.disabled = true;
            fecha.readOnly = true;
            return fecha;
        }

        return undefined;
    }

    export function DesbloquearSelectorDeFechaPorPropiedad(panel: HTMLDivElement, propiedad: string): HTMLInputElement {
        let fecha: HTMLInputElement = BuscarSelectorDeFecha(panel, propiedad, atControl.propiedad);
        if (Definido(fecha) && EsTrue(fecha.getAttribute(atControl.editable))) {
            fecha.disabled = false;
            fecha.readOnly = false;
            return fecha;
        }
        return undefined;
    }

    export function BloquearSelectorDeFechaHoraPorPropiedad(panel: HTMLDivElement, propiedad: string): boolean {
        let fecha: HTMLInputElement = BuscarSelectorDeFechaHora(panel, propiedad, atControl.propiedad);
        if (Definido(fecha)) {
            fecha.disabled = true;
            fecha.readOnly = true;
            let idHora: string = fecha.getAttribute(atSelectorDeFecha.hora);
            let controlHora: HTMLInputElement = document.getElementById(idHora) as HTMLInputElement;
            controlHora.readOnly = true;
            controlHora.disabled = true;
            return true;
        }
        return false;
    }

    export function DesbloquearSelectorDeFechaHoraPorPropiedad(panel: HTMLDivElement, propiedad: string): boolean {
        let fecha: HTMLInputElement = BuscarSelectorDeFechaHora(panel, propiedad, atControl.propiedad);
        return DesbloquearSelectorDeFechaHora(fecha);
    }

    export function DesbloquearSelectorDeFechaHora(fecha: HTMLInputElement): boolean {
        if (Definido(fecha) && EsTrue(fecha.getAttribute(atControl.editable))) {
            fecha.disabled = false;
            fecha.readOnly = false;
            let idHora: string = fecha.getAttribute(atSelectorDeFecha.hora);
            let controlHora: HTMLInputElement = document.getElementById(idHora) as HTMLInputElement;
            controlHora.readOnly = false;
            controlHora.disabled = false;
            return true;
        }
        return false;
    }

    export function DesbloquearListaDinamicaPorPropiedad(panel: HTMLDivElement, propiedad: string): boolean {
        let lista: HTMLInputElement = BuscarListaDinamica(panel, propiedad, atControl.propiedad);
        return DesbloquearListaDinamica(lista);
    }

    export function DesbloquearListaDinamica(lista: HTMLInputElement): boolean {
        if (lista !== null && EsTrue(lista.getAttribute(atControl.editable))) {
            lista.disabled = false;
            lista.readOnly = false;
            return true;
        }
        return false;
    }

    export function BloquearCheckPorPropiedad(panel: HTMLDivElement, propiedad: string, bloquear: boolean, errorSiNoHay: boolean = true, desmarcar: boolean = false): boolean {
        let check: HTMLInputElement = BuscarCheck(panel, propiedad);
        if (!Definido(check)) {
            if (errorSiNoHay)
                MensajesSe.Error('BloquearCheckPorPropiedad', "No esta definido el check: " + propiedad);
            return null;
        }
        return BloquearCheck(check, bloquear, desmarcar);
    }

    export function BloquearCheck(check: HTMLInputElement, bloquear: boolean, desmarcar: boolean = false): boolean {
        if (!EsTrue(check.getAttribute(atControl.editable))) bloquear = true;
        if (Definido(check)) {
            if (desmarcar) {
                const originalOnChange = check.onchange;
                check.onchange = null;
                check.checked = false;
                check.onchange = originalOnChange;
            }
            check.disabled = bloquear;
            check.readOnly = bloquear;
            return true;
        }
        return false;
    }

    export function BloquearEditorPorPropiedad(panel: HTMLDivElement, propiedad: string, bloquear: boolean = true): HTMLInputElement {
        let editor: HTMLInputElement = BuscarEditor(panel, propiedad);
        if (Definido(editor)) {
            if (bloquear) {
                if (BloquearInput(editor))
                    return editor;
            }
            else if (DesbloquearEditor(editor))
                return editor;
        }
        return undefined;
    }

    export function BloquearAreaDeTextoPorPropiedad(panel: HTMLDivElement, propiedad: string, bloquear: boolean = true): HTMLTextAreaElement {
        let area: HTMLTextAreaElement = BuscarAreaDeTexto(panel, propiedad);
        if (area !== null) {
            if (bloquear) {
                if (BloquearAreaDeTexto(area))
                    return area;
            }
            else if (DesbloquearAreaDeTexto(area))
                return area;
        }
        return undefined;
    }

    export function HabilitarReferenciaPost(panel: HTMLDivElement, idReferencia: string, habilitar: boolean) {
        let controles = panel.querySelectorAll(`input[${atControl.tipo}="${ltrTipoControl.ReferenciaPost}"][id="${idReferencia}"]`) as NodeListOf<HTMLInputElement>;

        if (controles.length !== 1)
            MensajesSe.Info(`No se ha localizado el control o hay más de uno, ${idReferencia}`);
        else {
            if (habilitar)
                controles[0].classList.remove(ltrCss.controlOculto);
            else
                controles[0].classList.add(ltrCss.controlOculto);
        }
    }

    export function BloquearReferenciaPostDeCreacion(idDiv: string) {
        let div: HTMLDivElement = document.getElementById(idDiv) as HTMLDivElement;

        if (!Definido(div)) {
            MensajesSe.Advertencia('Div no definido, consulte la consola', `Se quería bloquear la opción de creación en el div '${idDiv}', pero este no existe`);
            return;
        }

        let refPost: HTMLInputElement = document.getElementById(div.id + `-${ltrEspanes.Opcion.crearRef}.ref`) as HTMLInputElement;
        ApiControl.BloquearReferenciaPost(refPost);
    }

    export function DesbloquearReferenciaPostDeCreacion(idDiv: string) {
        let div: HTMLDivElement = document.getElementById(idDiv) as HTMLDivElement;
        let refPost: HTMLInputElement = document.getElementById(div.id + `-${ltrEspanes.Opcion.crearRef}.ref`) as HTMLInputElement;
        ApiControl.DesbloquearReferenciaPost(refPost);
    }
    function handleDisabledClick(e: MouseEvent) {
        e.preventDefault();
        e.stopPropagation();
    }

    export function DeshabilitarRef(ref: HTMLAnchorElement): void {
        ApiControl.IncluirCss(ref, 'deshabilitado');
        ref.addEventListener('click', handleDisabledClick);
    }

    export function HabilitarRef(ref: HTMLAnchorElement): void {
        ApiControl.ExcluirCss(ref, 'deshabilitado');
        ref.removeEventListener('click', handleDisabledClick);
    }

    export function BloquearReferenciaPost(ref: HTMLInputElement) {
        ref.style.visibility = "hidden";
    }

    export function DesbloquearReferenciaPost(ref: HTMLInputElement) {
        ref.style.removeProperty("visibility");
    }

    export function BloquearInput(editor: HTMLInputElement): boolean {
        if (editor !== null) {
            editor.disabled = true;
            editor.readOnly = true;
            return true;
        }
        return false;
    }

    export function BloquearBoton(boton: HTMLButtonElement): boolean {
        if (boton !== null) {
            boton.disabled = true;
            return true;
        }
        return false;
    }

    export function BloquearAreaDeTextoSi(panel: HTMLDivElement, propiedad: string, bloquear: boolean): boolean {
        var area = ApiControl.BuscarAreaDeTexto(panel, propiedad);
        if (Definido(area)) {
            if (bloquear) return BloquearAreaDeTexto(area);
            return DesbloquearAreaDeTexto(area);
        }
        return false;
    }

    export function BloquearAreaDeTexto(area: HTMLTextAreaElement): boolean {
        if (Definido(area)) {
            area.disabled = true;
            area.readOnly = true;
            return true;
        }
        return false;
    }

    export function DesbloquearAreaDeTexto(area: HTMLTextAreaElement): boolean {
        if (Definido(area)) {
            area.disabled = false;
            area.readOnly = false;
            return true;
        }
        return false;
    }
    export function DesbloquearBoton(boton: HTMLButtonElement): boolean {
        if (boton !== null) {
            boton.disabled = false;
            return true;
        }
        return false;
    }

    export function DesbloquearEditorPorPropiedad(panel: HTMLDivElement, propiedad: string): HTMLInputElement {
        let editor: HTMLInputElement = BuscarEditor(panel, propiedad);
        if (editor !== null) {
            return DesbloquearEditor(editor) ? editor : undefined;
        }
        return undefined;
    }

    export function DesbloquearEditor(editor: HTMLInputElement): boolean {
        if (editor !== null) {
            const editable = editor.getAttribute(atControl.editable);
            if (Definido(editable)) {
                const habilitar = EsTrue(editable)
                editor.disabled = !habilitar;
                editor.readOnly = !habilitar;
                return habilitar;
            }
            else {
                editor.disabled = false;
                editor.readOnly = false;
                return true;
            }
        }
        return false;
    }

    export function EstaContenidoEn(contenedor: HTMLElement, control: HTMLElement) {
        if (Definido(contenedor)) {
            var padre = control.parentNode;
            if (Definido(padre)) {
                do {
                    if (contenedor === padre)
                        return true;
                    padre = padre.parentNode;
                }
                while (Definido(padre));
            }
        }
        return false;
    }

    export function BuscarTabla(control: HTMLElement): HTMLDivElement {
        if (Definido(control)) {
            var padre = control.parentNode;
            if (Definido(padre)) {
                do {
                    if (padre instanceof HTMLDivElement && padre.classList.contains(ltrCss.crud.tabla))
                        return padre;
                    padre = padre.parentNode;
                }
                while (Definido(padre));
            }
        }
        return undefined;
    }


    export function BuscarFila(control: HTMLElement): HTMLDivElement {
        if (Definido(control)) {
            var padre = control.parentNode;
            if (Definido(padre)) {
                do {
                    if (padre instanceof HTMLDivElement && padre.classList.contains(ltrCss.crud.fila))
                        return padre;
                    padre = padre.parentNode;
                }
                while (Definido(padre) || !(padre instanceof HTMLDivElement));
            }
        }
        return undefined;
    }

    export function BuscarCelda(control: HTMLElement): HTMLDivElement {
        if (Definido(control)) {
            var padre = control.parentNode;
            if (Definido(padre)) {
                do {
                    if (padre instanceof HTMLDivElement && padre.classList.contains(ltrCss.crud.celda))
                        return padre;
                    padre = padre.parentNode;
                }
                while (Definido(padre) || !(padre instanceof HTMLDivElement));
            }
        }
        return undefined;
    }

    export function BuscarCeldaConClase(control: HTMLElement, clase: string): HTMLDivElement {
        if (Definido(control)) {
            var padre = control.parentNode;
            if (Definido(padre)) {
                do {
                    if (padre instanceof HTMLDivElement && padre.classList.contains(ltrCss.crud.celda) && padre.classList.contains(clase))
                        return padre;
                    padre = padre.parentNode;
                }
                while (Definido(padre) || !(padre instanceof HTMLDivElement));
            }
        }
        return undefined;
    }
    export function EstaEnUnaModal(control: HTMLElement) {
        var div = BuscarDivConClase(control, ltrCss.contenedorModal)
        return Definido(div);
    }

    export function BuscarDivConClase(control: HTMLElement, clase: string): HTMLDivElement {
        if (Definido(control)) {
            var padre = control.parentNode;
            if (Definido(padre)) {
                do {
                    if (padre instanceof HTMLDivElement && padre.classList.contains(clase))
                        return padre;
                    padre = padre.parentNode;
                }
                while (Definido(padre));
            }
        }
        return undefined;
    }

    export function BuscarControl(panel: HTMLDivElement, propiedad: string, erroSiNoLoEncuentra: boolean): HTMLElement {
        let a = undefined;

        a = BuscarEditor(panel, propiedad);
        if (Definido(a))
            return a;

        a = BuscarEditorDeFiltrado(panel, propiedad);
        if (Definido(a))
            return a;

        a = BuscarRestrictor(panel, propiedad, ltrTipoControl.restrictorDeEdicion);
        if (Definido(a))
            return a;

        a = BuscarRestrictor(panel, propiedad, ltrTipoControl.restrictorDeFiltro);
        if (Definido(a))
            return a;

        a = BuscarCheck(panel, propiedad);
        if (Definido(a))
            return a;

        a = BuscarListaDinamica(panel, propiedad, atControl.propiedad);
        if (Definido(a))
            return a;

        a = BuscarListaDeValores(panel, propiedad, atControl.propiedad);
        if (Definido(a))
            return a;

        a = BuscarListaDeElementos(panel, propiedad, atControl.propiedad);
        if (Definido(a))
            return a;

        a = BuscarSelectorDeArchivos(panel, propiedad, atControl.propiedad);
        if (Definido(a))
            return a;

        a = BuscarUrlDelArchivo(panel, propiedad);
        if (Definido(a))
            return a;

        a = BuscarVisorDeImagen(panel, propiedad);
        if (Definido(a))
            return a;

        a = BuscarAreaDeTexto(panel, propiedad, atControl.propiedad);
        if (Definido(a))
            return a;

        a = BuscarSelectorDeFecha(panel, propiedad, atControl.propiedad);
        if (Definido(a))
            return a;

        a = BuscarSelectorDeFechaHora(panel, propiedad, atControl.propiedad);
        if (Definido(a))
            return a;

        a = BuscarReferencia(panel, propiedad, atControl.propiedad);
        if (Definido(a))
            return a;

        a = BuscarReferenciaPost(panel, propiedad, atControl.propiedad);
        if (Definido(a))
            return a;

        if (erroSiNoLoEncuentra) {
            MensajesSe.EmitirMensajeDeExcepcion("Buscando un control", `No se ha localizado el control asociado a la propiedad ${propiedad} en el panel ${panel.id}`);
        }

        return undefined;
    }

    export function BuscarListaDinamicaPorGuardarEn(panel: HTMLDivElement, guardarEn: string): HTMLInputElement {
        return BuscarListaDinamica(panel, guardarEn, atListasDinamicasDto.guardarEn);
    }

    export function BuscarListaDinamicaPorPropiedad(panel: HTMLDivElement, propiedad: string): HTMLInputElement {
        return BuscarListaDinamica(panel, propiedad, atControl.propiedad);
    }

    export function BuscarSelectorDeFecha(panel: HTMLDivElement, propiedad: string, atributo: string = atControl.propiedad) {
        let fechas: NodeListOf<HTMLInputElement> = panel.querySelectorAll(`input[${atControl.tipo}="${ltrTipoControl.SelectorDeFecha}"]`) as NodeListOf<HTMLInputElement>;
        for (var i = 0; i < fechas.length; i++) {
            let fecha: HTMLInputElement = fechas[i] as HTMLInputElement;
            if (fecha.getAttribute(atributo) === propiedad.toLocaleLowerCase())
                return fecha;
        }
        return null;
    }

    export function BuscarFiltroEntreFechas(panel: HTMLDivElement, propiedad: string, atributo: string = atControl.propiedad) {
        let fechas: NodeListOf<HTMLInputElement> = panel.querySelectorAll(`input[${atControl.tipo}="${ltrTipoControl.FiltroEntreFechas}"]`) as NodeListOf<HTMLInputElement>;
        for (var i = 0; i < fechas.length; i++) {
            let fecha: HTMLInputElement = fechas[i] as HTMLInputElement;
            if (fecha.getAttribute(atributo) === propiedad.toLocaleLowerCase())
                return fecha;
        }
        return null;
    }
    export function BuscarReferencia(panel: HTMLDivElement, propiedad: string, atributo: string = atControl.propiedad): HTMLAnchorElement {
        let referencias: NodeListOf<HTMLAnchorElement> = panel.querySelectorAll(`a[${atControl.tipo}="${ltrTipoControl.Referencia}"]`) as NodeListOf<HTMLAnchorElement>;
        for (var i = 0; i < referencias.length; i++) {
            let referencia: HTMLAnchorElement = referencias[i] as HTMLAnchorElement;
            if (referencia.getAttribute(atributo) === propiedad.toLocaleLowerCase())
                return referencia;
        }
        return null;
    }

    export function BuscarReferenciaPost(panel: HTMLDivElement, propiedad: string, atributo: string) {
        let referencias: NodeListOf<HTMLInputElement> = panel.querySelectorAll(`a[${atControl.tipo}="${ltrTipoControl.ReferenciaPost}"]`) as NodeListOf<HTMLInputElement>;
        for (var i = 0; i < referencias.length; i++) {
            let referencia: HTMLInputElement = referencias[i] as HTMLInputElement;
            if (referencia.getAttribute(atributo) === propiedad.toLocaleLowerCase())
                return referencia;
        }
        return null;
    }

    export function BuscarSelectorDeFechaHora(panel: HTMLDivElement, propiedad: string, atributo: string = atControl.propiedad): HTMLInputElement {
        let fechas: NodeListOf<HTMLInputElement> = panel.querySelectorAll(`input[${atControl.tipo}="${ltrTipoControl.SelectorDeFechaHora}"]`) as NodeListOf<HTMLInputElement>;
        for (var i = 0; i < fechas.length; i++) {
            let fecha: HTMLInputElement = fechas[i] as HTMLInputElement;
            if (fecha.getAttribute(atributo) === propiedad.toLocaleLowerCase())
                return fecha;
        }
        return null;
    }

    export function MapearAreaDeTextoConResalto(panel: HTMLDivElement, propiedad: string, valor: string, resalto: string): HTMLTextAreaElement {
        var area = BuscarAreaDeTexto(panel, propiedad);
        area.value = valor;
        ApiControl.ResaltarControl(area, resalto);
        return area
    }
    export function MapearAreaDeTexto(panel: HTMLDivElement, propiedad: string, valor: string): HTMLTextAreaElement {
        var area = BuscarAreaDeTexto(panel, propiedad);
        area.value = valor;
        return area
    }

    export function MapearEnElAreaDeTextoUnJoson(area: HTMLTextAreaElement, valor: any): void {
        let valorParaMostrar: string = "";
        if (valor) {
            try {
                // 1. Intentamos convertir el string en un objeto real
                // Si viene con escapes ("{ \n...}"), parse lo limpia.
                let objetoOString = JSON.parse(valor);

                // 2. Si el servidor mandó un string doblemente serializado, 
                // el resultado de parse sigue siendo un string. Parseamos de nuevo.
                if (typeof objetoOString === 'string') {
                    objetoOString = JSON.parse(objetoOString);
                }

                // 3. Ahora sí, formateamos el OBJETO real con indentación de 4 espacios
                valorParaMostrar = JSON.stringify(objetoOString, null, 4);
            } catch (e) {
                // Si falla el parseo, mostramos el original quitando comillas extremas manuales
                valorParaMostrar = valor.replace(/^"|"$/g, '');
                console.error("No se pudo parsear el JSON de la respuesta", e);
            }
        }

        area.value = valorParaMostrar;
    }

    export function BuscarAreaDeTexto(panel: HTMLDivElement, propiedad: string, atributo: string = atControl.propiedad): HTMLTextAreaElement {
        let areas: NodeListOf<HTMLTextAreaElement> = panel.querySelectorAll(`textarea[${atControl.tipo}="${ltrTipoControl.AreaDeTexto}"]`) as NodeListOf<HTMLTextAreaElement>;
        for (var i = 0; i < areas.length; i++) {
            let area: HTMLTextAreaElement = areas[i] as HTMLTextAreaElement;
            if (area.getAttribute(atributo).toLocaleLowerCase() === propiedad.toLocaleLowerCase())
                return area;
        }
        return null;
    }

    export function BuscarEtiqueta(panel: HTMLDivElement, propiedad: string): HTMLLabelElement {
        let control = BuscarControl(panel, propiedad, true);
        let contenedor = control.parentElement.parentElement as HTMLDivElement;
        return contenedor.querySelector('label') as HTMLLabelElement;
    }

    export function MapearEditorConResalto(panel: HTMLDivElement, propiedad: string, valor: string, resalto: string) {
        var editor = BuscarEditor(panel, propiedad);
        AsignarValorConResalto(editor, valor, resalto);
    }

    export function MapearEditor(panel: HTMLDivElement, propiedad: string, valor: string) {
        var editor = BuscarEditor(panel, propiedad);
        AsignarValor(editor, valor);
    }


    export function Valor(input: HTMLInputElement): string | number {
        let valor: string = input.value;
        if (input.type == 'number')
            return IsNullOrEmpty(valor) ? null : Numero(valor);

        var formato = input.getAttribute(atControl.formato);
        if (formato === enumFormato.Moneda || formato === enumFormato.Porcentaje)
            return IsNullOrEmpty(valor) ? null : Importe(valor, true, false)
        if (formato === enumFormato.Numero || formato === enumFormato.Numero_2 || formato === enumFormato.Numero_6)
            return IsNullOrEmpty(valor) ? null : Importe(valor, true, true);
        if (formato === enumFormato.base64 && valor === '0' || IsNullOrEmpty(valor))
            return btoa(valor);

        return valor;
    }

    export function BuscarEditor(panel: HTMLDivElement, propiedad: string): HTMLInputElement {
        let editores: NodeListOf<HTMLInputElement> = panel.querySelectorAll(`input[${atControl.tipo}="${ltrTipoControl.Editor}"]`) as NodeListOf<HTMLInputElement>;
        for (let i = 0; i < editores.length; i++) {
            let editor: HTMLInputElement = editores[i];
            if (editor.getAttribute(atControl.propiedad).toLocaleLowerCase() === propiedad.toLocaleLowerCase()) {
                return editor;
            }
        }
        return null;
    }

    export function BuscarEditorDeFiltrado(panel: HTMLDivElement, propiedad: string): HTMLInputElement {
        let editores: NodeListOf<HTMLInputElement> = panel.querySelectorAll(`input[${atControl.tipo}="${ltrTipoControl.ConEditor}"]`) as NodeListOf<HTMLInputElement>;
        for (let i = 0; i < editores.length; i++) {
            let editor: HTMLInputElement = editores[i];
            if (editor.getAttribute(atControl.propiedad) === propiedad.toLocaleLowerCase()) {
                return editor;
            }
        }
        return null;
    }

    export function BuscarBotonPorClase(panel: HTMLDivElement, clase: string): HTMLInputElement {
        let boton: HTMLInputElement = panel.querySelector(`input[type="button"][${atControl.tipo}="${ltrTipoControl.opcion}"].${clase}, input[type="text"].${ltrCss.Modal.Boton}.${ltrCss.Modal.BotonPorDefecto}`) as HTMLInputElement;

        return boton;
    }

    export function BuscarBoton(panel: HTMLDivElement, opcion: string): HTMLInputElement {
        let botones: NodeListOf<HTMLInputElement> = panel.querySelectorAll(`input[type="button"][${atControl.tipo}="${ltrTipoControl.opcion}"]`) as NodeListOf<HTMLInputElement>;
        for (let i = 0; i < botones.length; i++) {
            let editor: HTMLInputElement = botones[i];
            if (editor.getAttribute(atControl.propiedad) === opcion.toLocaleLowerCase()) {
                return editor;
            }
        }
        return null;
    }

    export function BuscarRestrictor(panel: HTMLDivElement, propiedad: string, tipoDeControl: string): HTMLInputElement {
        let restrictores: NodeListOf<HTMLInputElement> = panel.querySelectorAll(`input[${atControl.tipo}="${tipoDeControl}"]`) as NodeListOf<HTMLInputElement>;
        for (let i = 0; i < restrictores.length; i++) {
            let restrictor: HTMLInputElement = restrictores[i];
            if (restrictor.getAttribute(atControl.propiedad) === propiedad.toLocaleLowerCase()) {
                return restrictor;
            }
        }
        return null;
    }

    export function BuscarCheck(panel: HTMLDivElement, propiedad: string): HTMLInputElement {
        let checks: NodeListOf<HTMLInputElement> = panel.querySelectorAll(`input[${atControl.tipo}="${ltrTipoControl.Check}"]`) as NodeListOf<HTMLInputElement>;
        for (let i = 0; i < checks.length; i++) {
            let check: HTMLInputElement = checks[i];
            if (check.getAttribute(atControl.propiedad) === propiedad.toLocaleLowerCase()) {
                return check;
            }
        }
        return null;
    }

    function BuscarListaDinamica(panel: HTMLDivElement, propiedad: string, atributo: string): HTMLInputElement {
        let listas: NodeListOf<HTMLInputElement> = panel.querySelectorAll(`input[${atControl.tipo}="${ltrTipoControl.ListaDinamica}"]`) as NodeListOf<HTMLInputElement>;
        for (let i = 0; i < listas.length; i++) {
            let lista: HTMLInputElement = listas[i] as HTMLInputElement;
            if (lista.getAttribute(atributo).toLocaleLowerCase() === propiedad.toLocaleLowerCase()) {
                return lista;
            }
        }
        return null;
    }

    export function BuscarListaDeValores(panel: HTMLDivElement, propiedad: string, atributo: string = atControl.propiedad): HTMLSelectElement {
        let listas: NodeListOf<HTMLSelectElement> = panel.querySelectorAll(`select[${atControl.tipo}="${ltrTipoControl.ListaDeValores}"]`) as NodeListOf<HTMLSelectElement>;
        for (let i = 0; i < listas.length; i++) {
            let lista: HTMLSelectElement = listas[i] as HTMLSelectElement;
            if (lista.getAttribute(atributo).toLocaleLowerCase() === propiedad.toLocaleLowerCase()) {
                return lista;
            }
        }
        return null;
    }

    export function BuscarListaDeElementos(panel: HTMLDivElement, propiedad: string, atributo: string = atControl.propiedad): HTMLSelectElement {
        let listas: NodeListOf<HTMLSelectElement> = panel.querySelectorAll(`select[${atControl.tipo}="${ltrTipoControl.ListaDeElementos}"]`) as NodeListOf<HTMLSelectElement>;
        for (let i = 0; i < listas.length; i++) {
            let lista: HTMLSelectElement = listas[i] as HTMLSelectElement;
            if (lista.getAttribute(atributo).toLocaleLowerCase() === propiedad.toLocaleLowerCase()) {
                return lista;
            }
        }
        return null;
    }

    export function BuscarSelectorDeArchivos(panel: HTMLDivElement, propiedad: string, atributo: string): HTMLInputElement {
        let selectores: NodeListOf<HTMLInputElement> = panel.querySelectorAll(`input[${atControl.tipo}="${ltrTipoControl.Archivo}"]`) as NodeListOf<HTMLInputElement>;
        for (let i = 0; i < selectores.length; i++) {
            let selector: HTMLInputElement = selectores[i] as HTMLInputElement;
            if (selector.getAttribute(atributo).toLocaleLowerCase() === propiedad.toLocaleLowerCase()) {
                return selector;
            }
        }
        selectores = panel.querySelectorAll(`input[${atControl.tipo}="${ltrTipoControl.SelectorDeUnArchivo}"]`) as NodeListOf<HTMLInputElement>;
        for (let i = 0; i < selectores.length; i++) {
            let selector: HTMLInputElement = selectores[i] as HTMLInputElement;
            if (selector.getAttribute(atributo).toLocaleLowerCase() === propiedad.toLocaleLowerCase()) {
                return selector;
            }
        }

        return null;
    }


    function BuscarUrlDelArchivo(panel: HTMLDivElement, propiedad: string): HTMLInputElement {
        let selectores: NodeListOf<HTMLInputElement> = panel.querySelectorAll(`input[${atControl.tipo}="${ltrTipoControl.UrlDeArchivo}"]`) as NodeListOf<HTMLInputElement>;
        for (var i = 0; i < selectores.length; i++) {
            var control = selectores[i] as HTMLInputElement;
            var dto = control.getAttribute(atControl.propiedad);
            if (dto === propiedad.toLowerCase())
                return control;
        }
        return null;
    }

    function BuscarVisorDeImagen(panel: HTMLDivElement, propiedadDto: string): HTMLImageElement {
        let visor: NodeListOf<HTMLImageElement> = panel.querySelectorAll(`img[${atControl.tipo}='${ltrTipoControl.VisorDeArchivo}']`) as NodeListOf<HTMLImageElement>;
        for (var i = 0; i < visor.length; i++) {
            var control = visor[i] as HTMLImageElement;
            var dto = control.getAttribute(atControl.propiedad);
            if (dto === propiedadDto.toLowerCase())
                return control;
        }
        return null;
    }

    export function BlanquearGridDeDetalle(grid: HTMLDivElement): void {
        //obtener la tabla 
        let tabla: HTMLDivElement = grid.querySelector(".div-tabla") as HTMLDivElement;
        let tbody: HTMLDivElement = tabla.querySelector(".div-tbody") as HTMLDivElement;
        if (Definido(tbody)) {
            //Eliminar y asignar un body vacio
            tbody.innerHTML = null;
            tbody.style.height = "0px";
        }

        ResetearEtiquetaDelGrid(grid);
    }

    export function ResetearEtiquetaDelGrid(grid: HTMLDivElement): void {
        if (grid?.parentElement?.parentElement?.parentElement) {
            var espan = grid.parentElement.parentElement.parentElement;
            let referencia = espan.querySelector(`a[${atControl.class}*=${ltrCss.Espan.cssExpansor}]`) as HTMLAnchorElement;
            if (Definido(referencia)) {
                let titulo = referencia.getAttribute(ltrEspanes.Atributos.titulo);
                referencia.innerText = `${titulo}`;
                referencia.classList.remove(ltrCss.Espan.conContenido);
            }
        }
    }


    export function BlanquearFecha(fecha: HTMLInputElement, esEntreFecha: boolean = false): void {
        fecha.value = "";
        if (!esEntreFecha) {
            fecha.disabled = !EsTrue(fecha.getAttribute(atControl.editable));
            fecha.readOnly = !EsTrue(fecha.getAttribute(atControl.editable));

            var valorPorDefecto = fecha.getAttribute(atControl.valorPorDefecto);
            if (!IsNullOrEmpty(valorPorDefecto) && valorPorDefecto === literal.Hoy)
                MapearAlControl.FechaDate(fecha, new Date());
        }

        let tipo: string = fecha.getAttribute(atControl.tipo);
        if (tipo === ltrTipoControl.SelectorDeFechaHora || tipo === ltrTipoControl.FiltroEntreFechas) {

            let idHora: string = fecha.getAttribute(atControl.tipo) === ltrTipoControl.SelectorDeFechaHora ?
                fecha.getAttribute(atSelectorDeFecha.hora) :
                fecha.getAttribute(atEntreFechas.idHoraDesde);

            if (!IsNullOrEmpty(idHora)) {
                let hora: HTMLInputElement = document.getElementById(idHora) as HTMLInputElement;
                hora.value = ''; if (!esEntreFecha) {
                    hora.disabled = !EsTrue(fecha.getAttribute(atControl.editable));
                    hora.readOnly = !EsTrue(fecha.getAttribute(atControl.editable));
                    hora.setAttribute(atSelectorDeFecha.milisegundos, '0');
                }
            }
        }
    }

    export function AsignarFecha(panel: HTMLDivElement, propiedad: string, fecha: Date | string): boolean {
        let control: HTMLInputElement = BuscarFecha(panel, propiedad);
        if (Definido(control)) {
            let date: Date = typeof fecha === 'string' ? new Date(fecha) : fecha;
            if (!EsFechaValida(date)) {
                control.value = "";
                return;
            }
            if (date.getTime() === new Date('1901-01-01T00:00:00').getTime()) {
                return false;
            }
            AsignarFechaHora(control, date);
            return true;
        }
        return false;
    }

    export function AsignarFechaHora(fechaHora: HTMLInputElement, fecha: Date | string): boolean {

        let date: Date = typeof fecha === 'string' ? new Date(fecha) : fecha;
        MapearAlControl.FechaDate(fechaHora, date);

        if (fechaHora.getAttribute(atControl.tipo) === ltrTipoControl.SelectorDeFechaHora ||
            (fechaHora.getAttribute(atControl.tipo) === ltrTipoControl.FiltroEntreFechas && !IsNullOrEmpty(fechaHora.getAttribute(atEntreFechas.idHoraDesde))))
            return MapearAlControl.HoraDate(fechaHora, date);
        return true;
    }

    export function BuscarFecha(panel: HTMLDivElement, propiedad: string): HTMLInputElement {
        let fechas: NodeListOf<HTMLInputElement> = panel.querySelectorAll(`input[${atControl.tipo}="${ltrTipoControl.SelectorDeFecha}"]`) as NodeListOf<HTMLInputElement>;
        for (var i = 0; i < fechas.length; i++) {
            let fecha: HTMLInputElement = fechas[i] as HTMLInputElement;
            if (fecha.getAttribute(atControl.propiedad) == propiedad.toLocaleLowerCase()) {
                return fecha;
            }
        }

        fechas = panel.querySelectorAll(`input[${atControl.tipo}="${ltrTipoControl.SelectorDeFechaHora}"]`) as NodeListOf<HTMLInputElement>;
        for (var i = 0; i < fechas.length; i++) {
            let fecha: HTMLInputElement = fechas[i] as HTMLInputElement;
            if (fecha.getAttribute(atControl.propiedad) == propiedad.toLocaleLowerCase()) {
                return fecha;
            }
        }

        return null;
    }

    export function AjustarColumnaDelGrid(columanDeOrdenacion: Tipos.Orden): boolean {
        let columna: HTMLTableCellElement = document.getElementById(columanDeOrdenacion.IdColumna) as HTMLTableCellElement;
        if (NoDefinido(columna)) {
            MensajesSe.Error("AjustarColumnaDelGrid", `la columna ${columanDeOrdenacion.IdColumna} no está definida en el Grid`);
            return false;
        }
        MapearComoOrdenar(columna, columanDeOrdenacion);
        return true;
    }

    export function Editor_Limpiar(editor: HTMLInputElement): void {
        MapearAlControl.Restrictor(editor, 0, "");
        ApiListaDinamica.BlanquearDependientes(editor);
    }

    export function BlanquearListaDeElementos(lista: HTMLSelectElement) {
        lista.selectedIndex = 0;
        QuitarOpcionesDeLalista(lista);
        lista.setAttribute(atListasDeElemento.Cargando, "N");
    }

    export function QuitarOpcionesDeLalista(lista: HTMLSelectElement) {
        lista.setAttribute(atListas.yaCargado, 'N');
        for (let i: number = lista.options.length - 1; i > 0; i--)
            lista.options.remove(i);

        ApiControl.QuitarResalto(lista);
    }


    export function BlanquearControlOculto(editor: HTMLInputElement): void {
        editor.value = "";
    }

    export function BlanquearEditor(editor: HTMLInputElement, usarDefaulValue: boolean = true): void {

        if (editor.classList.contains(ltrCss.controlOculto))
            return;

        AnularError(editor);
        editor.value = IsNullOrEmpty(editor.defaultValue) ? "" : usarDefaulValue ? editor.defaultValue : "";
        ApiControl.DesbloquearEditor(editor);
        //let editable = editor.getAttribute(atControl.editable);
        //if (Definido(editable)) {
        //    editor.disabled = !EsTrue(editable);
        //    editor.readOnly = !EsTrue(editable);
        //}
        //else {
        //    editor.disabled = false;
        //    editor.readOnly = false;
        //}
    }

    export function AnularError(control: HTMLInputElement): void {
        control.classList.remove(ltrCss.crtlNoValido);
        control.classList.add(ltrCss.crtlValido);
    }

    export function MarcarError(control: HTMLInputElement): void {
        control.classList.add(ltrCss.crtlNoValido);
        control.classList.remove(ltrCss.crtlValido);
    }

    export function SeleccionarElPrimeroYAplicarEditabilidad(selector: HTMLSelectElement): void {
        selector.classList.remove(ltrCss.crtlNoValido);
        selector.classList.add(ltrCss.crtlValido);
        let selectedIndex = 0;
        for (let i = 0; i < selector.options.length; i++) {
            if (selector.options[i].hasAttribute('selected')) {
                selectedIndex = i;
                break;
            }
        }
        selector.selectedIndex = selectedIndex;
        let esEditable = EsTrue(selector.getAttribute(atControl.filtro)) || EsTrue(selector.getAttribute(atControl.editable));

        selector.disabled = !esEditable;
        selector.setAttribute("readonly", esEditable ? 'false' : 'true');
    }

    export function LeerEntreRangos(controlDeRangoDesde: HTMLInputElement): string {
        let entreRangos: string = LeerRango(controlDeRangoDesde).toString();
        let idRangoHasta = controlDeRangoDesde.getAttribute(atEntreRangos.idRangoHasta);
        let rangoHasta: HTMLInputElement = document.getElementById(idRangoHasta) as HTMLInputElement;
        let valorHasta = LeerRango(rangoHasta);
        entreRangos = entreRangos + ltrSimbolos.separadorDeRangos + valorHasta;
        return entreRangos;
    }

    function LeerRango(controlRango: HTMLInputElement): string {
        let valor: string = controlRango.value;
        if (!IsNullOrEmpty(valor))
            return valor;
        return literal.undefined;
    }

    export function LeerEntreImportes(controlDeImporteDesde: HTMLInputElement): string {
        let entreImportes: string = LeerImporte(controlDeImporteDesde).toString();
        let idImporteHasta = controlDeImporteDesde.getAttribute(atEntreImportes.idImporteHasta);
        let importeHasta: HTMLInputElement = document.getElementById(idImporteHasta) as HTMLInputElement;
        let valorHasta = LeerImporte(importeHasta);
        if (Numero(entreImportes) > Numero(valorHasta)) {
            MensajesSe.Info(`Rango no válido, ${entreImportes} - ${valorHasta}`);
            entreImportes = valorHasta = literal.undefined;
        }
        entreImportes = entreImportes + ltrSimbolos.separadorDeRangos + valorHasta;
        return entreImportes;
    }

    function LeerImporte(controlImporte: HTMLInputElement): string {
        let valor: string = controlImporte.value;
        if (!IsNullOrEmpty(valor))
            return Numero(valor).toString();
        return literal.undefined;
    }
    export function LeerFechaHoraPorPropiedad(panel: HTMLDivElement, propiedad: string): Date {
        const control = BuscarFecha(panel, propiedad);
        if (Definido(control))
            return LeerFechaHoraDate(control, control.id + '.hora');
        return null
    }

    export function LeerFechaHoraDate(controlDeFecha: HTMLInputElement, idHora: string): Date | null {
        let fechaHoraIso: string = LeerFechaHora(controlDeFecha, idHora);
        if (IsNullOrEmpty(fechaHoraIso)) {
            return null;
        }
        let fecha: Date = new Date(fechaHoraIso);
        if (EsFechaValida(fecha)) {
            return fecha;
        }
        return null;
    }

    export function LeerEntreFechas(controlDeFechaDesde: HTMLInputElement): string {
        let idHora = controlDeFechaDesde.getAttribute(atEntreFechas.idHoraDesde);
        let entreFechas: string = LeerFechaHora(controlDeFechaDesde, idHora);
        let idFechaHasta = controlDeFechaDesde.getAttribute(atEntreFechas.idFechaHasta);
        let fechaHasta: HTMLInputElement = document.getElementById(idFechaHasta) as HTMLInputElement;
        idHora = controlDeFechaDesde.getAttribute(atEntreFechas.idHoraHasta);
        entreFechas = entreFechas + ltrSimbolos.separadorDeDosFechas + LeerFechaHora(fechaHasta, idHora);
        return entreFechas;
    }

    function LeerFechaHora(controlDeFecha: HTMLInputElement, idHora: string): string {
        let resultado: string = "";
        let fecha: Date = new Date(controlDeFecha.value);
        if (Definido(fecha)) {
            //let opciones: Intl.DateTimeFormatOptions = { day: '2-digit', month: '2-digit', year: 'numeric' };
            let controlDeHora: HTMLInputElement = document.getElementById(idHora) as HTMLInputElement;
            let valorDeHora: string = controlDeHora.value;
            if (!IsNullOrEmpty(valorDeHora)) {
                fecha = new Date(controlDeFecha.value + ' ' + valorDeHora);
            }
            return FechaToIso(fecha, valorDeHora);
        }

        return resultado;
    }

    export function MapearComoOrdenar(columna: HTMLTableCellElement, orden: Tipos.Orden) {
        let a: HTMLElement = columna.getElementsByTagName('a')[0] as HTMLElement;
        if (NoDefinido(a)) {
            MensajesSe.Error("AjustarColumnaDelGrid", `el orden aplicado a la propiedad ${orden.Propiedad} no se puede aplicar`);
            return false;
        }
        columna.setAttribute(atControl.ordenarPor, orden.OrdenarPor);
        columna.setAttribute(atControl.modoOrdenacion, orden.Modo);
        a.setAttribute("class", orden.ccsClase);
    }

    export function HacerVisibleLaBarra(barraHtml: HTMLDivElement, mostrar: boolean) {
        let idContenedorBarra = barraHtml.getAttribute(atArchivo.contenedorBarra);
        if (!IsNullOrEmpty(idContenedorBarra)) {
            let contenedorBarraHtml: HTMLDivElement = document.getElementById(idContenedorBarra) as HTMLDivElement;
            contenedorBarraHtml.style.display = mostrar ? ltrStyle.display.block : ltrStyle.display.none;
        }
        barraHtml.style.display = mostrar ? ltrStyle.display.block : ltrStyle.display.none;
    }

    export function AsignarControl(origen: HTMLElement, destino: HTMLElement, bloquear: boolean): void {
        let horaDestino: HTMLInputElement = undefined;

        var tipoOrigen = origen.getAttribute(atControl.tipo);
        var tipoDestino = destino.getAttribute(atControl.tipo);

        if (tipoOrigen === ltrTipoControl.Editor && tipoDestino === ltrTipoControl.Editor) {
            MapearAlControl.MapearEditor(destino as HTMLInputElement, 0, (origen as HTMLInputElement).value, bloquear, false);
        }
        else if (tipoOrigen === ltrTipoControl.restrictorDeEdicion && tipoDestino === ltrTipoControl.restrictorDeEdicion) {
            var id = Numero((destino as HTMLInputElement).getAttribute(atControl.restrictor));
            MapearAlControl.MapearEditor(destino as HTMLInputElement, id, (origen as HTMLInputElement).value, bloquear, false);
        }
        else if (tipoOrigen === ltrTipoControl.ListaDinamica && tipoDestino === ltrTipoControl.ListaDinamica) {
            var id = Numero((origen as HTMLInputElement).getAttribute(atListasDinamicas.idSeleccionado));
            ApiListaDinamica.AsignarValor(destino as HTMLInputElement, id, (origen as HTMLInputElement).value, bloquear);
        }
        else if (tipoOrigen === ltrTipoControl.AreaDeTexto && tipoDestino === ltrTipoControl.AreaDeTexto) {
            MapearAlControl.MapearAreaDeTexto(destino as HTMLTextAreaElement, (origen as HTMLInputElement).value, bloquear);
        }
        else if (tipoDestino === ltrTipoControl.ListaDinamica)
            ApiListaDinamica.AsignarValor(destino as HTMLInputElement, Numero(origen.getAttribute(atListasDinamicas.idSeleccionado)), (origen as HTMLInputElement).value);
        else if (tipoDestino === ltrTipoControl.Editor) {
            MensajesSe.EmitirExcepcion("Asignar", `no se ha implementado cómo asignar el valor desde  ${origen.id} a ${destino.id}`);
        }
        else if (tipoDestino === ltrTipoControl.SelectorDeFecha || tipoDestino === ltrTipoControl.SelectorDeFechaHora) {
            (destino as HTMLInputElement).value = (origen as HTMLInputElement).value;
            let idHoraOrigen = origen.getAttribute(atSelectorDeFecha.hora);
            if (!IsNullOrEmpty(idHoraOrigen)) {
                let idHoraDestino = destino.getAttribute(atSelectorDeFecha.hora);
                let horaOrigen: HTMLInputElement = document.getElementById(idHoraOrigen) as HTMLInputElement;
                horaDestino = document.getElementById(idHoraDestino) as HTMLInputElement;
                horaDestino.value = horaOrigen.value;
            }
        }
        else if (tipoOrigen === ltrTipoControl.ListaDeValores && tipoDestino === ltrTipoControl.ListaDeValores) {
            var valor = (origen as HTMLSelectElement).value;
            if (Numero(valor) > 0) MapearAlControl.ListaDeValores(destino as HTMLSelectElement, valor);
        }
        else MensajesSe.EmitirExcepcion("Asignar", `no se ha implementado cómo asignar el valor desde  ${origen.id} a ${destino.id}`);

        if (bloquear) {
            ApiControl.BloquearInput(destino as HTMLInputElement);
            if (Definido(horaDestino)) ApiControl.BloquearInput(horaDestino);
        }
    }

    export function ValorDe(selector: HTMLSelectElement): any {
        let propiedadDto = selector.getAttribute(atControl.propiedad);
        let obligatorio: boolean = EsTrue(selector.getAttribute(atControl.obligatorio));

        if (!obligatorio && Number(selector.value) === -1)
            return;

        if (obligatorio && (Number(selector.value) === 0 || Number(selector.value) === Numero(literal.menos1))) {
            selector.classList.remove(ltrCss.crtlValido);
            selector.classList.add(ltrCss.crtlNoValido);
            throw new Error(`Debe seleccionar un elemento de la lista ${propiedadDto}`);
        }
        selector.classList.remove(ltrCss.crtlNoValido);
        selector.classList.add(ltrCss.crtlValido);
        return selector.value;
    };

    export function OcultarHtmlAnchor(referencia: HTMLAnchorElement, ocultar: boolean): HTMLDivElement {
        let contenedor: HTMLDivElement = referencia.parentElement as HTMLDivElement;
        if (ocultar) {
            if (Definido(contenedor)) ApiPanel.OcultarPanel(contenedor);
            referencia.style.display = ltrStyle.display.none;
        }
        else {
            if (Definido(contenedor)) ApiPanel.MostrarPanel(contenedor);
            referencia.style.display = ltrStyle.display.block;
        }
        return contenedor;
    }

    export function OcultarEditor(editor: HTMLInputElement, ocultar: boolean): HTMLDivElement {
        let contenedor: HTMLDivElement = editor.parentNode as HTMLDivElement;
        if (ocultar)
            ApiPanel.OcultarPanel(contenedor);
        else
            ApiPanel.MostrarPanel(contenedor);
        return contenedor;
    }

    export function OcultarLista(lista: HTMLSelectElement, ocultar: boolean): HTMLDivElement {
        let contenedor: HTMLDivElement = lista.parentNode as HTMLDivElement;
        if (ocultar)
            ApiPanel.OcultarPanel(contenedor);
        else
            ApiPanel.MostrarPanel(contenedor);
        return contenedor;
    }

    export function ObtenerObjetoListaDinamica(lista: HTMLInputElement, controlador: string, procesar: Function): void {
        var objeto = OpcionesDeLasListas.ObtenerObjeto(lista);
        if (!Definido(objeto)) {
            ApiDePeticiones.LeerElementoPorId(lista, controlador, Numero(lista.getAttribute(atListasDinamicas.idSeleccionado)), new Array<Parametro>(), null)
                .then((peticion) => {
                    procesar(lista.id, peticion.resultado.datos)
                })
                .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
        }
        else
            procesar(lista.id, objeto);
    }
}
namespace ApiRuote {

    export function NavegarARelacionar(crud: Crud.GridDeDatos, idOpcionDeMenu: string, idSeleccionado: number, filtroRestrictor: Tipos.Restrictor) {

        let filtroJson: string = ApiDeFiltro.DefinirRestrictorNumerico(filtroRestrictor.Propiedad, filtroRestrictor.Valor);

        let form: HTMLFormElement = document.getElementById(idOpcionDeMenu) as HTMLFormElement;

        if (form === null) {
            throw new Error(`La opción de menú '${idOpcionDeMenu}' está mal definida, actualice el descriptor`);
        }

        let navegarAlCrud: string = form.getAttribute(atNavegar.navegarAlCrud);
        let idRestrictor: string = form.getAttribute(atNavegar.idRestrictor) as string;
        let idOrden: string = form.getAttribute(atNavegar.orden) as string;

        let restrictor: HTMLInputElement = document.getElementById(idRestrictor) as HTMLInputElement;
        restrictor.value = filtroJson;
        let ordenInput: HTMLInputElement = document.getElementById(idOrden) as HTMLInputElement;
        ordenInput.value = "";

        let valores: Diccionario<any> = new Diccionario<any>();
        let filtros: Tipos.Restrictor[] = [];
        filtros.push(filtroRestrictor);
        valores.Agregar(ltrClaveDeEstado.paginaDestino, navegarAlCrud);
        valores.Agregar(ltrClaveDeEstado.restrictoresDeUnPost, filtros);
        valores.Agregar(ltrClaveDeEstado.idSeleccionado, idSeleccionado);
        Navegar(crud, form, valores);
    }

    export function NavegarADependientes(crud: Crud.GridDeDatos, idOpcionDeMenu: string, idSeleccionado: number, filtroRestrictor: Array<Tipos.Restrictor>, grabar: boolean) {

        let form: HTMLFormElement = document.getElementById(idOpcionDeMenu) as HTMLFormElement;

        if (grabar) {
            let datosDeEntrada: Array<Parametro> = new Array<Parametro>();
            datosDeEntrada.push(new Parametro(ltrOperacion.AccionTrasGuardar, () => ApiRuote.NavegarADependientes(crud, idOpcionDeMenu, idSeleccionado, filtroRestrictor, false)));
            (crud as Crud.CrudMnt).crudDeEdicion.Modificar(ltrOperacion.ModificarPorId, null, datosDeEntrada);
            return;
        }

        if (form === null)
            throw new Error(`La opción de menú '${idOpcionDeMenu}' está mal definida, actualice el descriptor`);

        let navegarAlCrud: string = form.getAttribute(atNavegar.navegarAlCrud);
        let paraqueNavegar: string = form.getAttribute(atNavegar.paraqueNavegar);
        let soloMapearEnELFiltro: boolean = EsTrue(form.getAttribute(atNavegar.soloMapearEnElFiltro));

        for (var i = 0; i < filtroRestrictor.length; i++) {
            filtroRestrictor[i].SoloFiltra = soloMapearEnELFiltro;
        }

        let valores: Diccionario<any> = new Diccionario<any>();
        valores.Agregar(ltrClaveDeEstado.paginaDestino, navegarAlCrud);
        valores.Agregar(ltrClaveDeEstado.restrictoresDeUnPost, filtroRestrictor);
        valores.Agregar(ltrClaveDeEstado.idSeleccionado, idSeleccionado);
        valores.Agregar(ltrClaveDeEstado.paraqueNavegar, paraqueNavegar);

        Navegar(crud, form, valores);
    }

    function Navegar(crud: Crud.GridDeDatos, form: HTMLFormElement, valores: Diccionario<any>) {
        crud.AntesDeNavegar(valores);
        EntornoSe.Sumit(form);
    }
};

namespace ApiDeFiltro {

    export function OcultarControlDeFiltro(panelDeFiltro: HTMLDivElement, propiedad: string) {
        const control = ApiControl.BuscarControl(panelDeFiltro, propiedad, true);
        if (Definido(control)) {
            ApiControl.MostrarControlSi(control, false, false);
        }
    }

    export function MostrarControlDeFiltro(panelDeFiltro: HTMLDivElement, propiedad: string) {
        const control = ApiControl.BuscarControl(panelDeFiltro, propiedad, true);
        if (Definido(control)) {
            ApiControl.MostrarControlSi(control, true, false);
        }
    }
    export function RestringirPorOtrosControles(control: HTMLInputElement | HTMLSelectElement): Array<ClausulaDeFiltrado> {
        const clausualasRestrictoras = new Array<ClausulaDeFiltrado>();

        let restringirPor: string = control.getAttribute(atListas.RestringidoPor);
        if (IsNullOrEmpty(restringirPor))
            return clausualasRestrictoras;

        let idContenedor: string = control.getAttribute(atListas.ContenidoEn);
        if (NoDefinido(idContenedor))
            MensajesSe.EmitirMensajeDeExcepcion("Definir filtro lista dinámica",
                `No se puede definir el filtro para la propiedad ${control.id} ya que no se ha definido el atributo ${atListas.ContenidoEn}`);

        let contenedor: HTMLDivElement = document.getElementById(idContenedor) as HTMLDivElement;
        const restrictores = restringirPor.split(';')
        for (let i = 0; i < restrictores.length; i++) {

            if (restrictores[i] === literal.filtro.idEditado) {
                if (Crud.crudMnt.EstoyEditando)
                    clausualasRestrictoras.push(new ClausulaDeFiltrado(literal.filtro.idEditado, literal.filtro.criterio.diferente, Crud.crudMnt.crudDeEdicion.IdEditor.value))
            }
            else {
                let restrictor: HTMLElement = contenedor.querySelector(`[${atControl.propiedad}=${restrictores[i]}]`);
                if (NoDefinido(restrictor))
                    MensajesSe.EmitirMensajeDeExcepcion("Definir filtro lista dinámica", `La lista ${control.id} dice que se ha de restringir por la propiedad '${restringirPor}' y no hay ningún control en el panel ${idContenedor} con dicha propiedad`);
                const c = DefinirFiltroRestrictor(control, restrictor);
                if (Definido(c)) clausualasRestrictoras.push(c);
            }


        }
        return clausualasRestrictoras;
    }

    export function MapearFiltrosPasados(filtroDeIu: Tipos.Filtro) {
        let control = document.getElementById(filtroDeIu.IdControl);
        if (Definido(control)) {
            let tipo = control.getAttribute(atControl.tipo);
            switch (tipo) {
                case ltrTipoControl.Editor: mapearFiltroEnEditor(filtroDeIu, control as HTMLInputElement); break;
                case ltrTipoControl.Check: mapearFiltroEnCheck(filtroDeIu, control as HTMLInputElement); break;
                case ltrTipoControl.ListaDinamica: mapearFiltroEnListaDinamica(filtroDeIu, control as HTMLInputElement); break;
                case ltrTipoControl.ListaDeElementos: mapearFiltroEnListaDeValores(filtroDeIu, control as HTMLSelectElement); break;
                case ltrTipoControl.ListaDeValores: mapearFiltroEnListaDeValores(filtroDeIu, control as HTMLSelectElement); break;
                case ltrTipoControl.FiltroEntreImportes: mapearFiltroEntreImportes(filtroDeIu, control as HTMLInputElement); break;
                case ltrTipoControl.FiltroEntreRangos: mapearFiltroEntreRangos(filtroDeIu, control as HTMLInputElement); break;
                case ltrTipoControl.FiltroEntreFechas: mapearFiltroEntreFechas(filtroDeIu, control as HTMLInputElement); break;
            }
        }
    }

    export function DefinirFiltroPorId(id: number): string {
        return ApiDeFiltro.DefinirRestrictorNumerico(literal.filtro.clausulaId, id);
    }

    export function DefinirRestrictorNumerico(propiedad: string, valor: number): string {
        var clausulas = new Array<ClausulaDeFiltrado>();
        var clausula: ClausulaDeFiltrado = new ClausulaDeFiltrado(propiedad, literal.filtro.criterio.igual, `${valor}`);
        clausulas.push(clausula);
        return JSON.stringify(clausulas);
    }

    export function IncluirPlantillaDeFiltrado(plantilla: any) {
        let menu = document.getElementById(ltrMenus.menu.filtro) as HTMLUListElement;

        if (menu.children.length === Numero(ltrMenus.MenuDeFiltrado.NumeroDeOpciones)) menu.appendChild(document.createElement('hr'));

        let id = `${ltrPropiedades.Negocio.PlantillaDeFiltrado.Plantilla}_${ObtenerPropiedad(plantilla, literal.id)}`;
        let titulo = ObtenerPropiedad(plantilla, ltrPropiedades.Negocio.PlantillaDeFiltrado.Plantilla);
        ApiDeMenuFlotante.IncluirOpcionPosteriorALaPosicion(menu, Numero(ltrMenus.MenuDeFiltrado.NumeroDeOpciones) + 1, id, titulo, ltrMenus.enumOrigen.crud, enumCssOpcionMenu.DeVista);
    }

    function mapearFiltroEnEditor(filtro: Tipos.Filtro, control: HTMLInputElement) {
        let valor = filtro.Atributos.Obtener(atControl.valorInput);
        if (IsNullOrEmpty(valor))
            return;
        if (control.value === valor)
            return;
        control.value = valor;
    }
    function mapearFiltroEnCheck(filtro: Tipos.Filtro, control: HTMLInputElement) {
        let valor = filtro.Atributos.Obtener(atCheck.chequeado);
        if (control.checked === valor)
            return;
        control.checked = valor;
        control.dispatchEvent(new Event('click'));
    }

    function mapearFiltroEnListaDinamica(filtro: Tipos.Filtro, lista: HTMLInputElement) {
        let idSeleccionado: number = Numero(filtro.Atributos.Obtener(atListasDinamicas.idSeleccionado));
        let ultimoBuscado: string = filtro.Atributos.Obtener(atListasDinamicas.ultimaCadenaBuscada);
        let valor: string = filtro.Atributos.Obtener(atControl.valorInput);
        if (idSeleccionado === 0)
            ApiListaDinamica.Blanquear(lista);
        else {
            lista.setAttribute(atListasDinamicas.ultimaCadenaBuscada, ultimoBuscado);
            ApiListaDinamica.AsignarValor(lista, idSeleccionado, valor);
        }
    }

    function mapearFiltroEntreRangos(filtro: Tipos.Filtro, rangoDesde: HTMLInputElement) {
        let idRangoHasta = rangoDesde.getAttribute(atEntreRangos.idRangoHasta);
        let rangoHasta: HTMLInputElement = document.getElementById(idRangoHasta) as HTMLInputElement;
        let valorDesde: string = filtro.Atributos.Obtener(atControl.valorInput);
        let valorHasta: string = filtro.Atributos.Obtener(atEntreRangos.valorHasta);
        rangoDesde.value = valorDesde;
        rangoHasta.value = valorHasta;
    }

    function mapearFiltroEntreImportes(filtro: Tipos.Filtro, importeDesde: HTMLInputElement) {
        let idImporteHasta = importeDesde.getAttribute(atEntreImportes.idImporteHasta);
        let importeHasta: HTMLInputElement = document.getElementById(idImporteHasta) as HTMLInputElement;
        let valorDesde: number = Numero(filtro.Atributos.Obtener(atControl.valorInput));
        let valorHasta: number = Numero(filtro.Atributos.Obtener(atEntreImportes.valorHasta));
        AsignarValor(importeDesde, valorDesde.toString());
        AsignarValor(importeHasta, valorHasta.toString());
    }

    function mapearFiltroEntreFechas(filtro: Tipos.Filtro, fechaDesde: HTMLInputElement) {
        let idFechaHasta: string = fechaDesde.getAttribute(atEntreFechas.idFechaHasta);
        let valorFechaHasta = filtro.Atributos.Obtener(atEntreFechas.valorFechaHasta);
        let valorFechaDesde = filtro.Atributos.Obtener(atControl.valorInput);
        let fechaHasta: HTMLInputElement = document.getElementById(idFechaHasta) as HTMLInputElement;
        ApiControl.AsignarFechaHora(fechaDesde, CrearFecha(valorFechaDesde));
        ApiControl.AsignarFechaHora(fechaHasta, CrearFecha(valorFechaHasta));
    }

    function mapearFiltroEnListaDeValores(filtro: Tipos.Filtro, control: HTMLSelectElement) {
        let valor = filtro.Atributos.Obtener(atControl.valorInput);
        if (NoDefinido(valor))
            return;

        control.value = valor;
    }


    function DefinirFiltroRestrictor(control: HTMLInputElement | HTMLSelectElement, restrictor: HTMLElement): ClausulaDeFiltrado {
        let a: ClausulaDeFiltrado = undefined;
        if (restrictor instanceof HTMLInputElement) {
            let propiedadRestrictora: string = control.getAttribute(atListasDinamicas.PropiedadRestrictora);
            if (NoDefinido(propiedadRestrictora)) propiedadRestrictora = restrictor.getAttribute(atControl.propiedad);
            let tipoRestrictor: string = restrictor.getAttribute(atControl.tipo);
            let tipoControl: string = control.getAttribute(atControl.tipo);

            if (tipoRestrictor === ltrTipoControl.restrictorDeEdicion || tipoRestrictor === ltrTipoControl.restrictorDeFiltro) {
                let restringirPor: string = control.getAttribute(atListasDinamicas.RestringidoPor);
                a = ClausulaDeFiltradoDelRestrictorDeEdicion(restrictor, propiedadRestrictora, restringirPor);
                if (control instanceof HTMLInputElement && tipoControl === ltrTipoControl.ListaDinamica && Definido(a) && Numero(a.valor) === 0)
                    MensajesSe.EmitirMensajeDeExcepcion("DefinirFiltroRestrictor", `No se  ha definido el valor en el control ${restrictor.id} para restringir en el control ${control.id}`);
            }
            else if (tipoRestrictor === ltrTipoControl.ListaDinamica) {
                let valorRestrictor: string = restrictor.getAttribute(atListasDinamicas.idSeleccionado);
                a = ObtenerValorDeLaListaDinamica(restrictor, propiedadRestrictora, Numero(valorRestrictor));
            }
            else if (tipoRestrictor === ltrTipoControl.Editor) {
                a = ClausulaDeFiltradoDelEditor(restrictor, propiedadRestrictora);
            }
            else
                MensajesSe.EmitirMensajeDeExcepcion("DefinirFiltroRestrictor",
                    `No se ha definido para el tipo ${tipoRestrictor} de restrictor como crear una cláusula de filtrado`);
        }
        return a;
    }

    function ObtenerValorDeLaListaDinamica(lista: HTMLInputElement, propiedadRestrictora: string, valorRestrictor: number): ClausulaDeFiltrado {

        if (IsNullOrEmpty(propiedadRestrictora))
            MensajesSe.EmitirMensajeDeExcepcion("ObtenerValorDeLaListaDinamica", `no se ha definido la propiedad restrictora en el control ${lista.id}`);

        if (Number(valorRestrictor) === 0)
            return undefined;

        return new ClausulaDeFiltrado(propiedadRestrictora, atCriterio.igual, valorRestrictor.toString());

    }

    function ClausulaDeFiltradoDelRestrictorDeEdicion(restrictor: HTMLInputElement, propiedadRestrictora: string, restringirPor: string): ClausulaDeFiltrado {
        let valorRestrictor: string = (restrictor as HTMLInputElement).getAttribute(atControl.restrictor);
        let criterio: string = restrictor.getAttribute(atControl.criterio);
        if (Numero(valorRestrictor) === 0 && !EsTrue(restrictor.getAttribute(atControl.obligatorio)))
            return undefined;

        return new ClausulaDeFiltrado(
            NoDefinido(propiedadRestrictora) ? restringirPor : propiedadRestrictora,
            IsNullOrEmpty(criterio) ? atCriterio.igual : criterio,
            Numero(valorRestrictor) === 0 ? 0 : valorRestrictor);
    }

    function ClausulaDeFiltradoDelEditor(editor: HTMLInputElement, restringirPor: string): ClausulaDeFiltrado {
        let valorRestrictor: string = (editor as HTMLInputElement).value;
        if (!Definido(restringirPor))
            MensajesSe.Error("ClausulaDeFiltradoDelEditor", "No se ha definido el nombre de la propiedad restrictora para el editor:" + editor.id);
        return new ClausulaDeFiltrado(restringirPor, atCriterio.igual, EsNumero(valorRestrictor) ? Numero(valorRestrictor) : valorRestrictor);
    }

}

