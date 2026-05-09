namespace ApiPanel {

    //modal-para-pedir-datos
    export function HayModalAbierta(padre: HTMLDivElement, tipoModal: string): boolean {
        // 1. Definir el selector CSS para encontrar los elementos que cumplen ambas condiciones
        const selector = IsNullOrEmpty(tipoModal) ? `div.${ltrCss.contenedorModal}` : `div.${ltrCss.contenedorModal}[tipomodal="${tipoModal}"]`;

        // 2. Obtener todos los divs que cumplen el selector dentro del 'padre'
        const posiblesModales = padre.querySelectorAll(selector);

        // 3. Convertir NodeListOf a Array para usar el método 'some'
        // 4. Usar 'some' para verificar si AL MENOS UN elemento cumple la condición de visibilidad

        for (let i = 0; i < posiblesModales.length; i++) {
            const divElement = posiblesModales[i];
            const esVisible = EsVisible(divElement as HTMLDivElement);
            if (esVisible) return true;
        }
        return false;
        //return Array.from(posiblesModales).some((divElement) => {
        //    // Se realiza un casting para asegurar que el método EsVisible reciba el tipo correcto
        //    return EsVisible(divElement as HTMLDivElement);
        //});
    }

    export function EsVisible(panel: HTMLDivElement): boolean { return ApiControl.EsVisible(panel); }

    export function MostrarBarrarCargando(panel: HTMLDivElement) {
        if (!Definido(panel))
            return;
        const barraDeCarga = document.createElement('div');
        barraDeCarga.className = ltrCss.Espan.barra;
        barraDeCarga.innerHTML = '<div class="' + ltrCss.Espan.animarBarrra + '"></div>';
        barraDeCarga.id = panel.id + '_' + 'cargando';
        panel.appendChild(barraDeCarga);
    }

    export function QuitarBarraDeCarga(grid: HTMLDivElement) {
        const panel = MapearAlGrid.PanelDeBarra(grid);
        if (!Definido(panel))
            return;
        const barraDeCarga = document.getElementById(panel.id + '_' + 'cargando');
        if (barraDeCarga && barraDeCarga.parentNode) {
            barraDeCarga.parentNode.removeChild(barraDeCarga);
        }

        let estaElModuloCargado = typeof Crud !== 'undefined';
        if (estaElModuloCargado && Definido(Crud.crudMnt) && Crud.crudMnt.EstoyEditandoConsultando) {
            const span: HTMLDivElement = grid.parentElement.parentElement.parentElement as HTMLDivElement;
            Crud.crudMnt.crudDeEdicion.CargaSpanCompletada(span);
        }
    }

    export function ContenedorDe(id: string): HTMLDivElement {
        let input = document.getElementById(id);
        let contenidaEn = input.getAttribute(atControl.ContenidoEn);
        return document.getElementById(contenidaEn) as HTMLDivElement;
    }

    export function MostrarEspanSegunPropiedad(idPanel: string, datos: any, propiedad: string) {
        let panel: HTMLDivElement = document.getElementById(idPanel) as HTMLDivElement;

        if (!Definido(panel)) {
            if (ObtenerPropiedad(datos, propiedad, false)) return;
            else {
                MensajesSe.Advertencia('Div no existe', `Se devía evaluar si muetra un div según la propiedad '${propiedad}' pero el div no existe`);
                return;
            }
        }

        if (ObtenerPropiedad(datos, propiedad) && panel.classList.contains(ltrCss.Espan.noVisible))
            panel.classList.remove(ltrCss.Espan.noVisible);
        if (!ObtenerPropiedad(datos, propiedad) && !panel.classList.contains(ltrCss.Espan.noVisible))
            panel.classList.add(ltrCss.Espan.noVisible);
    }

    export function MostrarDetalleSi(panel: HTMLDivElement, datos: any, criterio: boolean) {
        if (criterio && panel.classList.contains(ltrCss.Espan.noVisible))
            panel.classList.remove(ltrCss.Espan.noVisible);
        if (!criterio && !panel.classList.contains(ltrCss.Espan.noVisible))
            panel.classList.add(ltrCss.Espan.noVisible);
    }

    export function HayEditoresConValor(panel: HTMLDivElement, tipoFiltro: string): boolean {
        let editores: NodeListOf<HTMLInputElement> = panel.querySelectorAll(`input[${atControl.tipo}="${tipoFiltro}"]`) as NodeListOf<HTMLInputElement>;
        for (let i = 0; i < editores.length; i++) {
            let editor: HTMLInputElement = editores[i];
            if (!IsNullOrEmpty(editor.value))
                return true;
        }
        return false;
    }

    export function HayFiltroEntreFechasConValor(panel: HTMLDivElement): boolean {
        let fechas: NodeListOf<HTMLInputElement> = panel.querySelectorAll(`input[${atControl.tipo}="${ltrTipoControl.FiltroEntreFechas}"]`) as NodeListOf<HTMLInputElement>;
        for (let i = 0; i < fechas.length; i++) {
            let fecha: HTMLInputElement = fechas[i];
            let valor: string = ApiControl.LeerEntreFechas(fecha);
            if (valor.trim() !== ltrSimbolos.separadorDeDosFechas)
                return true;
        }
        return false;
    }

    export function HayFiltroEntreImportesConValor(panel: HTMLDivElement): boolean {
        let importes: NodeListOf<HTMLInputElement> = panel.querySelectorAll(`input[${atControl.tipo}="${ltrTipoControl.FiltroEntreImportes}"]`) as NodeListOf<HTMLInputElement>;
        for (let i = 0; i < importes.length; i++) {
            let importe: HTMLInputElement = importes[i];
            let entreImportes: string = ApiControl.LeerEntreImportes(importe);
            if (entreImportes.trim() !== literal.undefined + ltrSimbolos.separadorDeRangos + literal.undefined)
                return true;
        }
        return false;
    }

    export function HayFiltroEntreRangosConValor(panel: HTMLDivElement): boolean {
        let rangos: NodeListOf<HTMLInputElement> = panel.querySelectorAll(`input[${atControl.tipo}="${ltrTipoControl.FiltroEntreRangos}"]`) as NodeListOf<HTMLInputElement>;
        for (let i = 0; i < rangos.length; i++) {
            let rango: HTMLInputElement = rangos[i];
            let entrerangos: string = ApiControl.LeerEntreRangos(rango);
            if (entrerangos.trim() !== literal.undefined + ltrSimbolos.separadorDeRangos + literal.undefined)
                return true;
        }
        return false;
    }

    export function HayRestrictoresDeFiltroConValor(panel: HTMLDivElement): boolean {
        let restrictores: NodeListOf<HTMLInputElement> = panel.querySelectorAll(`input[${atControl.tipo}="${ltrTipoControl.restrictorDeFiltro}"]`) as NodeListOf<HTMLInputElement>;
        for (let i = 0; i < restrictores.length; i++) {
            let restrictor: HTMLInputElement = restrictores[i];
            if (Numero(restrictor.getAttribute(atControl.restrictor)) > 0)
                return true;
        }
        return false;
    }

    export function HayListasDinamicasConValor(panel: HTMLDivElement): boolean {
        let listas: NodeListOf<HTMLInputElement> = panel.querySelectorAll(`input[${atControl.tipo}="${ltrTipoControl.ListaDinamica}"]`) as NodeListOf<HTMLInputElement>;
        for (let i = 0; i < listas.length; i++) {
            let lista: HTMLInputElement = listas[i];
            if (Numero(lista.getAttribute(atListasDinamicas.idSeleccionado)) > 0)
                return true;
        }
        return false;
    }

    export function HayListasDeElementosConValor(panel: HTMLDivElement): boolean {
        let listas: NodeListOf<HTMLSelectElement> = panel.querySelectorAll(`select[${atControl.tipo}="${ltrTipoControl.ListaDeElementos}"]`) as NodeListOf<HTMLSelectElement>;
        for (let i = 0; i < listas.length; i++) {
            let lista: HTMLSelectElement = listas[i];
            if (lista.selectedIndex > 0)
                return true;
        }
        return false;
    }

    export function HayListasDeValoresConValor(panel: HTMLDivElement): boolean {
        let listas: NodeListOf<HTMLSelectElement> = panel.querySelectorAll(`select[${atControl.tipo}="${ltrTipoControl.ListaDeValores}"]`) as NodeListOf<HTMLSelectElement>;
        for (let i = 0; i < listas.length; i++) {
            let lista: HTMLSelectElement = listas[i];
            if (lista.selectedIndex > 0)
                return true;
        }
        return false;
    }

    export function AnadirIdsDeControlesDeFiltroDeTipoInput(panel: HTMLDivElement, arrayIds: string[]): void {
        var arrayHtmlInput = panel.querySelectorAll(`input[${atControl.filtro}="S"]`) as NodeListOf<HTMLInputElement>;
        for (let i = 0; i < arrayHtmlInput.length; i++) {
            var htmlInput = arrayHtmlInput[i];
            var id = htmlInput.getAttribute(atControl.id);
            if (id === null)
                console.log(`Falta el atributo id del componente de filtro ${htmlInput}`);

            else
                arrayIds.push(id);
        }
    }


    export function AnadirColumnasAdicionales(panel: HTMLDivElement, arrayIds: string[]): void {
        var chekes = panel.querySelectorAll(`input[${atCheck.MostrarColumna}="S"]`) as NodeListOf<HTMLInputElement>;
        for (let i = 0; i < chekes.length; i++) {
            var htmlInput = chekes[i];
            var columna = htmlInput.getAttribute(atCheck.Columna);
            if (!Definido(columna) === null)
                console.log(`Falta el atributo Columna del componente de filtro ${htmlInput}`);
            else
                if (chekes[i].checked) arrayIds.push(columna);
        }
    }

    export function AnadirIdsDeControlesDeFiltroDeTipoSelect(panel: HTMLDivElement, arrayIds: string[]): void {
        var arrayHtmlSelect = panel.querySelectorAll(`select[${atControl.filtro}="S"]`) as NodeListOf<HTMLSelectElement>;
        for (let i = 0; i < arrayHtmlSelect.length; i++) {
            var htmlSelect = arrayHtmlSelect[i];
            var id = htmlSelect.getAttribute(atControl.id);
            if (id === null)
                console.log(`Falta el atributo id del componente de filtro ${htmlSelect}`);
            else
                arrayIds.push(id);
        }
    }

    export function PonerEnModoConsulta(panel: HTMLDivElement, estaDeBaja: boolean = false) {
        ModoAcceso.AplicarloAlPanel(panel, ModoAcceso.enumModoDeAccesoDeDatos.Consultor, estaDeBaja);
    }

    export function CrearEnlaceAlElemento(divDeElementos: HTMLDivElement, elemento: Elemento) {
        let url: string = `${window.location}`;
        if (url.indexOf("?id=") <= 0)
            url = url + `?id=${elemento.Id}`;
        let a: HTMLAnchorElement = ApiControl.CrearRef(elemento.Texto, url, true);
        a.setAttribute(atControl.idElemento, elemento.Id.toString());
        divDeElementos.appendChild(a);
        var br = document.createElement("br");
        divDeElementos.appendChild(br);
    }
    export function BuscarAmpliacion(panel: HTMLDivElement, ampliacion: string): HTMLDivElement {
        let ampliaciones: NodeListOf<HTMLDivElement> = panel.querySelectorAll(`div[${atControl.esAmpliacion}="true"]`) as NodeListOf<HTMLDivElement>;
        for (let i = 0; i < ampliaciones.length; i++) {
            if (ampliaciones[i].getAttribute(ltrAmpliaciones.AccionTrasCrear).toLowerCase() === ampliacion.toLowerCase())
                return ampliaciones[i]
        }
        return undefined
    }


    export async function RenderizarToHtml(panel: HTMLDivElement, idArchivo: number, accion: string) {
        let parametros = `idArchivo=${idArchivo}`;
        var url = `/${Ajax.Archivos.controlador}/${accion}?${parametros}`;
        const response = await fetch(url);

        const errorMessage = response.headers.get('X-Error-Message');
        if (errorMessage) {
            panel.innerHTML = errorMessage;
            return;
        }

        const blob = await response.blob();
        const text = await blob.text(); // Obtener el contenido del blob como texto
        RenderizarContenido(panel, text, blob.type === 'text/plain' ? 'texto' : 'html');
    }


    //export function RenderizarXml(panel: HTMLDivElement, objectUrl: string) {
    //    fetch(objectUrl)
    //        .then(response => response.text())
    //        .then(xmlString => {
    //            const parser = new DOMParser();
    //            const xmlDoc = parser.parseFromString(xmlString, "text/xml");
    //            const serializer = new XMLSerializer();
    //            let formattedXml = serializer.serializeToString(xmlDoc);
    //            let xml = EscapeHtml(formattedXml).trimStart();
    //            if (!xml.startsWith('&lt;?xml version&', 0) && !xml.startsWith('&lt;soapenv:Envelope')) {
    //                formattedXml = FormatearXmlManualmente(formattedXml);
    //                RenderizarContenido(panel, formattedXml, 'texto');
    //                return;
    //            }
    //            let contenido = `<pre class='visor-iframe-body-pre'">${xml}</pre>`;
    //            RenderizarContenido(panel, contenido, 'xml');

    //        })
    //        .catch(error => {
    //            panel.innerHTML = 'Error al cargar el archivo XML';
    //        });
    //}

    //function FormatearXmlManualmente(xmlString: string): string {
    //    // Reemplazar '><' con '>\n<' para insertar saltos de línea
    //    let formattedXml = xmlString.replace(/>\s*</g, '><br><');

    //    // Agregar indentación
    //    let indent = 0;
    //    const lines = formattedXml.split('<br>');
    //    formattedXml = lines.map(line => {
    //        if (line.match(/<\//)) {
    //            indent--;
    //        }
    //        const formattedLine = ' '.repeat(indent * 2) + line;
    //        if (line.match(/<[^/].*>/) && !line.match(/\/>/)) {
    //            indent++;
    //        }
    //        return formattedLine;
    //    }).join('\n');

    //    return formattedXml;
    //}


    //export function RenderizarContenido(panel: HTMLDivElement, contenido: string, tipoContenido: string) {
    //    panel.innerHTML = '';
    //    const iframe = document.createElement('iframe');
    //    iframe.classList.add('visor-iframe-txt')
    //    panel.style.overflow = 'hidden';
    //    panel.appendChild(iframe);

    //    iframe.onload = () => {
    //        const iframeDoc = iframe.contentDocument || iframe.contentWindow.document;

    //        // Envuelve el contenido en un div con estilos específicos
    //        iframeDoc.body.classList.add('visor-iframe-txt-body')
    //    };

    //    iframe.contentWindow.document.open();
    //    if (tipoContenido === 'xml') {
    //        iframe.contentWindow.document.write(contenido);
    //    } else if (tipoContenido === 'json' || tipoContenido === 'texto') {
    //        const formattedContent = `<div style="white-space: pre-wrap;">${contenido}</div>`;
    //        iframe.contentWindow.document.write(formattedContent);
    //    } else {
    //        const unescapedContent = UnEscapeHtml(contenido);
    //        iframe.contentWindow.document.write(unescapedContent);
    //    }
    //    iframe.contentWindow.document.close();
    //}
    export function RenderizarXml(panel: HTMLDivElement, objectUrl: string) {
        fetch(objectUrl)
            .then(response => response.text())
            .then(xmlString => {
                // Siempre formateamos el XML manualmente
                const formattedXml = FormatearXmlManualmente(xmlString);
                const escapedXml = EscapeHtml(formattedXml);
                const contenido = `<pre class='visor-iframe-body-pre'>${escapedXml}</pre>`;
                RenderizarContenido(panel, contenido, 'xml');
            })
            .catch(error => {
                panel.innerHTML = 'Error al cargar el archivo XML';
            });
    }
    function FormatearXmlManualmente(xmlString: string): string {
        // Paso 1: Separar etiquetas y aplicar indentación básica
        let formattedXml = xmlString.replace(/>\s*</g, '>\n<');
        let indent = 0;

        const lines = formattedXml.split('\n');
        formattedXml = lines.map(line => {
            line = line.trim();
            if (line.startsWith('</')) {
                indent = Math.max(0, indent - 1); // Reducir indentación ANTES de aplicar
            }
            const indentedLine = ' '.repeat(indent * 2) + line;
            if (line.startsWith('<') && !line.startsWith('</') && !line.includes('</') && !line.endsWith('/>')) {
                indent++; // Aumentar indentación solo para etiquetas de apertura no cerradas
            }
            return indentedLine;
        }).join('\n');

        // Paso 2: Colapsar elementos simples (sin hijos)
        const simpleElementRegex = /(<(\w+)(?:\s+[^>]*)?>)\s*\n\s*([^<\n]*?)\s*\n\s*(<\/\2>)/g;
        formattedXml = formattedXml.replace(simpleElementRegex, (_, openTag, __, content, closeTag) => {
            return `${openTag}${content.trim()}${closeTag}`;
        });

        return formattedXml;
    }

    export function RenderizarContenido(panel: HTMLDivElement, contenido: string, tipoContenido: string) {
        panel.innerHTML = '';
        const iframe = document.createElement('iframe');
        iframe.classList.add('visor-iframe-txt')
        panel.style.overflow = 'hidden';
        panel.appendChild(iframe);

        iframe.onload = () => {
            const iframeDoc = iframe.contentDocument || iframe.contentWindow.document;
            iframeDoc.body.classList.add('visor-iframe-txt-body');

            // Añade estilos para preservar los espacios en blanco
            const style = iframeDoc.createElement('style');
            style.textContent = `
            body { white-space: pre-wrap; font-family: monospace; }
            .visor-iframe-body-pre { margin: 0; }
        `;
            iframeDoc.head.appendChild(style);
        };

        iframe.contentWindow.document.open();
        if (tipoContenido === 'xml') {
            iframe.contentWindow.document.write(contenido);
        } else if (tipoContenido === 'json' || tipoContenido === 'texto') {
            let formattedContent: string
            if (EsJson(contenido)) {
                const jsonObject = JSON.parse(contenido);
                formattedContent = `<div style="white-space: pre-wrap; font-family: monospace; line-height: 1.6">${JSON.stringify(jsonObject, null, 2)}</div>`;
            }
            else {
                formattedContent = `<div style="white-space: pre-wrap;">${contenido}</div>`;
            }
            iframe.contentWindow.document.write(formattedContent);
        } else {
            const unescapedContent = UnEscapeHtml(contenido);
            iframe.contentWindow.document.write(unescapedContent);
        }
        iframe.contentWindow.document.close();
    }


    export function ajustarZoomIframe(iframe: HTMLIFrameElement) {
        const iframeDoc = iframe.contentDocument || iframe.contentWindow?.document;
        if (iframeDoc && iframeDoc.body) {
            // Aplicar zoom al contenido del PDF
            iframeDoc.body.style.transform = 'scale(1.5)';
            iframeDoc.body.style.transformOrigin = 'top left';
        } else {
            console.warn('No se pudo acceder al documento del iframe para ajustar el zoom.');
        }
    }

    export function RenderizarContenidoPdf(panel: HTMLDivElement, contenido: string) {
        panel.innerHTML = '';
        const iframe = document.createElement('iframe');
        iframe.classList.add('visor-iframe-pdf');
        panel.style.overflow = 'hidden';
        panel.appendChild(iframe);

        iframe.onload = () => {
            const iframeDoc = iframe.contentDocument || iframe.contentWindow?.document;
            if (iframeDoc) {
                iframeDoc.body.classList.add('visor-iframe-pdf-body');

                // Crear un contenedor para el PDF
                const pdfContainer = iframeDoc.createElement('div');
                pdfContainer.classList.add('pdf-container');
                iframeDoc.body.appendChild(pdfContainer);

                // Cargar el PDF en un objeto <object>
                const pdfObject = iframeDoc.createElement('object');
                pdfObject.id = panel.id + '_objeto';
                pdfObject.setAttribute('data', contenido);
                pdfObject.setAttribute('type', 'application/pdf');
                AjustarZoomPDF(pdfObject, 0);
                pdfContainer.appendChild(pdfObject);
            }
        };

        iframe.src = 'about:blank';
    }

    export function AjustarZoomPDF(pdfObject: HTMLObjectElement, incremeto: number) {
        pdfObject.style.width = (100 + 100 * incremeto).toString() + '%';
        pdfObject.style.height = (100 + 100 * incremeto).toString() + '%';
        pdfObject.setAttribute('incremento', incremeto.toString());
    }

    export function RenderizarContenidoImagen(panel: HTMLDivElement, contenido: string) {
        panel.innerHTML = '';
        const iframe = document.createElement('iframe');
        iframe.classList.add('visor-iframe-img');
        panel.style.overflow = 'hidden';
        panel.appendChild(iframe);

        iframe.onload = () => {
            const iframeDoc = iframe.contentDocument || iframe.contentWindow.document;
            iframeDoc.body.style.margin = '0';
            iframeDoc.body.style.overflow = 'auto'; // Permite scroll en el cuerpo del iframe


            const wrapper = iframeDoc.createElement('div');
            wrapper.style.minWidth = '100%';
            wrapper.style.height = '100%';
            wrapper.style.display = 'flex';
            wrapper.style.justifyContent = 'left';
            wrapper.style.alignItems = 'stretch';

            wrapper.innerHTML = contenido;
            iframeDoc.body.appendChild(wrapper);

            // Ajusta el estilo de la imagen si existe
            const img = iframeDoc.querySelector('img');
            if (img) {
                img.style.maxWidth = 'none'; // Elimina la restricción de ancho máximo
                img.style.height = 'auto';
                img.style.width = 'auto';
            }
        };

        iframe.contentWindow.document.open();
        iframe.contentWindow.document.write('<body></body>');
        iframe.contentWindow.document.close();
    }
    export function MapearControlesDesdeElPanelAlJson(panel: HTMLDivElement, elementoJson: JSON): JSON {

        if (panel.classList.contains(ltrCss.crud.edicion))
            panel = document.getElementsByClassName(ltrCss.crud.datosPrincipales)[0] as HTMLDivElement;

        MapearAlJson.ListasDeElementos(panel, elementoJson);
        MapearAlJson.ListasDeValores(panel, elementoJson);
        MapearAlJson.ListaDinamicas(panel, elementoJson);
        MapearAlJson.Restrictores(panel, elementoJson);
        MapearAlJson.Editores(panel, elementoJson);
        MapearAlJson.Textos(panel, elementoJson);
        MapearAlJson.Archivos(panel, elementoJson);
        MapearAlJson.Urls(panel, elementoJson);
        MapearAlJson.Checks(panel, elementoJson);
        MapearAlJson.Fechas(panel, elementoJson);
        return elementoJson;
    }

    export function MapearControlesDesdeElPanelAlDiccionario(panel: HTMLDivElement, diccionario: Diccionario<any>): void {
        MapearAlDiccionario.ListaDinamicas(panel, diccionario);
        MapearAlDiccionario.Checks(panel, diccionario);
        MapearAlDiccionario.ListasDeValores(panel, diccionario);
    }

    export function MapearControlesDesdeElPanelALaListaDeParametros(panel: HTMLDivElement, parametros: Array<Parametro>): void {
        MapearALaListaDeParametros.ListaDinamicas(panel, parametros);
        MapearALaListaDeParametros.Restrictores(panel, parametros);
        MapearALaListaDeParametros.Editores(panel, parametros);
        MapearALaListaDeParametros.Textos(panel, parametros);
        MapearALaListaDeParametros.Fechas(panel, parametros);
        MapearALaListaDeParametros.ListasDeElementos(panel, parametros);
        MapearALaListaDeParametros.ListasDeValores(panel, parametros);
        MapearALaListaDeParametros.Checks(panel, parametros);
        MapearALaListaDeParametros.Archivos(panel, parametros);
    }

    export function BlanquearControlesOcultos(panel: HTMLDivElement) {
        let ocultos: NodeListOf<HTMLInputElement> = panel.querySelectorAll(`.${ltrCss.controlOculto}`) as NodeListOf<HTMLInputElement>;
        for (let i = 0; i < ocultos.length; i++) {
            ApiControl.BlanquearControlOculto(ocultos[i]);
        }
    }

    export function BlanquearControlesDeIU(panel: HTMLDivElement, cargarListasDeElementos: boolean = true) {
        BlanquearEditores(panel);
        BlanquearSeleccionEnLasListasDeElementos(panel);
        ApiDeInicializacion.Archivos(panel);
        ApiDeInicializacion.Checkes(panel);
        ApiDeInicializacion.AreasDeTexto(panel);
        if (cargarListasDeElementos) {
            ApiDeInicializacion.InicializarListasDeElementos(panel);
        }
        ApiDeInicializacion.ListasDeValores(panel, cargarListasDeElementos);
        ApiDeInicializacion.InicializarSelectoresDeFecha(panel);
        ApiDeInicializacion.InicializarSpanDeArchivos(panel);
        ApiDeInicializacion.InicializarListasDinamicas(panel);
        ApiDeInicializacion.InicializarCanvas(panel);
    }

    export function MostrarPanelPorId(idPanel: string): HTMLDivElement {
        let panel: HTMLDivElement = document.getElementById(`${idPanel}`) as HTMLDivElement;
        MostrarPanel(panel);
        return panel;
    }

    export function OcultarMostrarPanelPorId(idPanel: string, ocultar: boolean): HTMLDivElement {
        let panel: HTMLDivElement = document.getElementById(`${idPanel}`) as HTMLDivElement;
        ApiPanel.OcultarMostrarPanel(panel, ocultar);
        return panel;
    }

    export function OcultarMostrarPanel(panel: HTMLDivElement, ocultar: boolean): void {
        if (ocultar)
            ApiPanel.OcultarPanel(panel);
        else
            ApiPanel.MostrarPanel(panel);
    }

    export function MostrarPanel(panel: HTMLDivElement) {
        const tipoPanel = panel.getAttribute(atControl.tipo);
        if (tipoPanel === ltrTipoControl.spanDeControles) {
            ApiControl.ExcluirCss(panel, ltrCss.Espan.noVisible);
        }
        else {
            ApiControl.ExcluirCss(panel, ltrCss.divNoVisible);
            if (Definido(panel.style.display))
                panel.style.display = panel.getAttribute(atControl.anteriorDisplay);
        }

        //const esPanelDeDto = panel.querySelector("div[name='tr_lbl_propiedad']") !== null;
        //if (esPanelDeDto) {
        //    AdaptarFilasAlMovil(panel);
        //}
    }

    //export function AdaptarFilasAlMovil(panel: HTMLDivElement): void {
    //    if (!EsDispositvoMovil) return;

    //    const filas = panel.querySelectorAll<HTMLDivElement>("div[name='tr_lbl_propiedad']");
    //    filas.forEach(fila => {
    //        // Contar cuántas columnas tiene actualmente (cuántos '1fr' hay)
    //        const estilo = fila.style.gridTemplateColumns;
    //        const columnas = (estilo.match(/1fr/g) || []).length;
    //        if (columnas > 1) {
    //            // Colapsar a una sola columna
    //            fila.style.gridTemplateColumns = '1fr';
    //        }
    //    });
    //}

    export function OcultarPanelPorId(idPanel: string): HTMLDivElement {
        let panel: HTMLDivElement = document.getElementById(`${idPanel}`) as HTMLDivElement;
        OcultarPanel(panel);
        return panel;
    }

    export function OcultarContenedorDto(panelDelDto: HTMLDivElement, propiedad: string, ocultar: boolean): HTMLDivElement {
        let control = ApiControl.BuscarControl(panelDelDto, propiedad, false);
        if (Definido(control)) {
            let contenedor: HTMLDivElement = (control.parentElement.parentElement) as HTMLDivElement;
            if (ocultar)
                ApiPanel.OcultarPanel(contenedor);
            else
                ApiPanel.MostrarPanel(contenedor);
            return contenedor;
        }
        return undefined;
    }

    export function OcultarPanel(panel: HTMLDivElement) {
        const tipoPanel = panel.getAttribute(atControl.tipo);
        if (tipoPanel === ltrTipoControl.spanDeControles) {
            ApiControl.IncluirCss(panel, ltrCss.Espan.noVisible);
        }
        else {
            ApiControl.RemplazarCss(panel, ltrCss.divVisible, ltrCss.divNoVisible);

            if (Definido(panel.style.display)) {
                panel.setAttribute(atControl.anteriorDisplay, panel.style.display);
                panel.style.display = '';
            }
        }

    }

    export function CerrarModalPorId(id: string) {
        let modal: HTMLDivElement = document.getElementById(id) as HTMLDivElement;
        if (NoDefinido(modal))
            throw new Error(`La modal ${id} no está definida`);
        CerrarModal(modal);
    }

    export function OcultarModalPorId(id: string) {
        let modal: HTMLDivElement = document.getElementById(id) as HTMLDivElement;
        if (NoDefinido(modal))
            throw new Error(`La modal ${id} no está definida`);
        OcultarModal(modal);
    }

    export function CerrarModal(modal: HTMLDivElement) {
        BlanquearSelectoresDeElemento(modal);
        OcultarModal(modal);
    }

    export function OcultarModal(modal: HTMLDivElement) {
        modal.style.display = ltrStyle.display.none;
        let accion = modal.getAttribute(atModal.trasCerrar);

        let boton = ApiControl.BuscarBotonPorClase(modal, ltrCss.Modal.BotonPorDefecto) as HTMLInputElement;
        if (Definido(boton)) modal.addEventListener('hidden.bs.modal', function () {
            document.removeEventListener('keydown', function (event) {
                if (event.key === 'Enter') {
                    boton.click();
                }
            });
        });

        if (Definido(accion))
            Evaluar('ApiDeControladores.OcultarModal', accion, accion.includes('this') ? modal : undefined);
    }

    export function AbrirModalPorId(id: string): HTMLDivElement {
        let modal: HTMLDivElement = document.getElementById(id) as HTMLDivElement;
        if (NoDefinido(modal))
            throw new Error(`La modal ${id} no está definida`);
        AbrirModal(modal);
        return modal;
    }

    export function AbrirModalDeDatos(idModal: string): HTMLDivElement {
        let modal: HTMLDivElement = document.getElementById(idModal) as HTMLDivElement;
        if (!Definido(modal)) MensajesSe.EmitirMensajeDeExcepcion('AbrirModalDeDatos', `la modal para pedir datos '${idModal}' no está definida`);
        ApiPanel.BlanquearControlesDeIU(modal);
        ApiPanel.AbrirModal(modal);
        return modal;
    }

    export function HayCambios(panel: HTMLDivElement, valores: Map<string, any>): boolean {
        var controles = panel.querySelectorAll('input, select, textarea');

        for (var i = 0; i < controles.length; i++) {
            var control = controles[i];
            if (valores.has(control.id)) {
                var valor = valores.get(control.id);
                if (control instanceof HTMLInputElement && valor !== control.value) {
                    return true;
                }
                if (control instanceof HTMLTextAreaElement && valor !== control.value) {
                    return true;
                }
                if (control instanceof HTMLSelectElement && valor !== control.value) {
                    return true;
                }
            }
        }
        return false;
    }
    export function AlmacenarValoresDelPanel(panel: HTMLDivElement, valores: Map<string, any>): void {
        var controles = panel.querySelectorAll('input, select, textarea');

        for (var i = 0; i < controles.length; i++) {
            var control = controles[i];
            if (control instanceof HTMLInputElement) {
                valores.set(control.id, control.value);
            }
            if (control instanceof HTMLTextAreaElement) {
                valores.set(control.id, control.value);
            }
            if (control instanceof HTMLSelectElement) {
                valores.set(control.id, control.value);
            }
        }
    }

    export function AbrirModal(modal: HTMLDivElement): void {

        let introPulsadoEnAreaDeTexto: boolean = false;

        const listener = function (event: KeyboardEvent) {
            if (event.key === 'Enter') {
                if (!introPulsadoEnAreaDeTexto) {
                    boton.click();
                    modal.removeEventListener('keydown', listener);
                }
                else {
                    introPulsadoEnAreaDeTexto = false;
                }
            }
        };

        modal.style.display = ltrStyle.display.block;

        let boton: HTMLInputElement = ApiControl.BuscarBotonPorClase(modal, ltrCss.Modal.BotonPorDefecto) as HTMLInputElement;
        if (Definido(boton)) modal.addEventListener('keydown', listener);


        let areas: NodeListOf<HTMLTextAreaElement> = modal.querySelectorAll(`textarea[${atControl.tipo}="${ltrTipoControl.AreaDeTexto}"]`) as NodeListOf<HTMLTextAreaElement>;
        for (var i = 0; i < areas.length; i++) {
            areas[i].addEventListener('keydown', function (e) {
                if (e.key === 'Enter') {
                    introPulsadoEnAreaDeTexto = true;
                    //e.preventDefault(); 
                }
            });
        }

        let accion = modal.getAttribute(atModal.trasAbrir);
        if (Definido(accion))
            Evaluar('AbrirModal', accion, accion.includes('this') ? modal : undefined);

        PosicionarEn(modal);

        EntornoSe.AjustarModalesAbiertas();
    }

    export function PosicionarEn(modal: HTMLDivElement) {
        const controlVacio = modal.querySelectorAll(`input:not([disabled]):not([type='checkbox']):not([tipo='${ltrTipoControl.restrictorDeFiltro}'])` +
            `:not([tipo='${ltrTipoControl.restrictorDeEdicion}']):not(.${ltrCss.controlOculto}),` +
            ` select:not([disabled][value='${atListasDeElemento.Seleccionar}'])`) as NodeListOf<HTMLInputElement | HTMLSelectElement>;

        var encontrado = false;
        controlVacio.forEach(control => {
            if (!encontrado) {
                if (control instanceof HTMLInputElement && IsNullOrEmpty(control.value)) {
                    control.focus();
                    encontrado = true;
                }
                if (control instanceof HTMLSelectElement) {
                    var selector = control as HTMLSelectElement;
                    var opcion = selector.options[selector.value] as HTMLOptionElement;
                    if (Definido(opcion) && opcion.text === atListasDeElemento.Seleccionar) {
                        control.focus();
                        encontrado = true;
                    }
                }
            }
        });
    }

    export function ModalAbierta(modal: HTMLDivElement): boolean {
        return modal.style.display === ltrStyle.display.block || (modal.classList.contains(ltrCss.divVisible) && modal.style.display !== ltrStyle.display.none);
    }

    export function QuitarClaseDeCtrlNoValido(panel: HTMLDivElement) {
        let crtls: HTMLCollectionOf<HTMLElement> = panel.getElementsByClassName(ltrCss.crtlNoValido) as HTMLCollectionOf<HTMLElement>;
        for (let i = 0; i < crtls.length; i++) {
            crtls[i].classList.remove(ltrCss.crtlNoValido);
        }

    }

    export function ValidarSiSeMantieneActiva(opciones: NodeListOf<HTMLButtonElement>, activas: string[], seleccionadas: number): void {
        for (var i = 0; i < opciones.length; i++) {
            let opcion: HTMLButtonElement = opciones[i];
            if (ApiControl.EstaBloqueada(opcion))
                continue;

            let literal: string = opcion.value.toLowerCase();
            if (activas.indexOf(literal) >= 0) {

                let permiteMultiSeleccion: string = opcion.getAttribute(atOpcionDeMenu.permiteMultiSeleccion);
                if (!EsTrue(permiteMultiSeleccion))
                    opcion.disabled = !(seleccionadas === 1);

                if (EsTrue(permiteMultiSeleccion)) {
                    let numero: number = Numero(opcion.getAttribute(atOpcionDeMenu.numeroMaximoSeleccionable));
                    if (numero === -1 || seleccionadas <= numero)
                        opcion.disabled = false;
                }
            }
        }
    }

    export function DesactivarPanel(panel: HTMLDivElement): void {
        let inputs: NodeListOf<HTMLInputElement> = panel.querySelectorAll("input") as NodeListOf<HTMLInputElement>;
        for (let i: number = 0; i < inputs.length; i++)
            ApiControl.BloquearInput(inputs[i]);
    }

    export function BloquearControlesPorPropieda(panel: HTMLDivElement, propiedades: string) {
        if (Definido(propiedades)) {
            let lista: string[] = propiedades.split('|');
            for (var i = 0; i < lista.length; i++) {
                var control = ApiControl.BuscarControl(panel, lista[i], false);
                if (Definido(control)) {
                    if (control instanceof HTMLInputElement) {
                        (control as HTMLInputElement).disabled = true;
                        (control as HTMLInputElement).readOnly = true;
                    }
                }
            }
        }
    }

    export function DesactivarOpciones(opciones: NodeListOf<HTMLButtonElement>, desactivas: string[]): void {
        for (var i = 0; i < opciones.length; i++) {
            let opcion: HTMLButtonElement = opciones[i];
            if (ApiControl.EstaBloqueada(opcion))
                continue;

            let literal: string = opcion.value.toLowerCase();
            if (desactivas.indexOf(literal) >= 0)
                opcion.disabled = true;
        }
    }

    export function DesactivarConMultiSeleccion(opciones: NodeListOf<HTMLButtonElement>, seleccionadas: number): void {
        for (var i = 0; i < opciones.length; i++) {
            let opcion: HTMLButtonElement = opciones[i];
            if (ApiControl.EstaBloqueada(opcion))
                continue;

            if (!opcion.disabled) {
                let permiteMultiSeleccion: string = opcion.getAttribute(atOpcionDeMenu.permiteMultiSeleccion);
                if (!EsTrue(permiteMultiSeleccion)) {
                    opcion.disabled = !(seleccionadas === 1);
                    return;
                }

                let numero: number = Numero(opcion.getAttribute(atOpcionDeMenu.numeroMaximoSeleccionable));
                if (numero !== -1)
                    opcion.disabled = (seleccionadas > numero);
            }

        }
    }

    export function CambiarLiteralOpcion(opciones: NodeListOf<HTMLButtonElement>, antiguo: string, nuevo: string): void {
        for (var i = 0; i < opciones.length; i++) {
            let opcion: HTMLButtonElement = opciones[i];
            if (ApiControl.EstaBloqueada(opcion))
                continue;

            let literal: string = opcion.value.toLowerCase();
            if (literal.toLowerCase() === antiguo)
                opcion.value = nuevo;
        }
    }

    export function ObtenerSelector(idSelector: string): HTMLDivElement {
        let selector: HTMLDivElement = document.getElementById(idSelector) as HTMLDivElement;
        if (NoDefinido(selector))
            throw new Error(`el selector ${idSelector} no está definido`);
        return selector;
    }

    export function ObtenerEditorAsociadoAlSelector(selector: HTMLDivElement): HTMLInputElement {
        let idEditor = selector.getAttribute(atSelectorDeElementos.EditorAsociado);
        let editor: HTMLInputElement = document.getElementById(idEditor) as HTMLInputElement;
        if (NoDefinido(editor))
            throw new Error(`el editor ${idEditor} no está definido en el selector ${selector.id}`);
        return editor;
    }

    export function BlanquearEditores(panel: HTMLDivElement) {
        let editores: NodeListOf<HTMLInputElement> = panel.querySelectorAll(`input[${atControl.tipo}="${ltrTipoControl.Editor}"]`) as NodeListOf<HTMLInputElement>;
        for (let i = 0; i < editores.length; i++) {
            ApiControl.BlanquearEditor(editores[i]);
        }
    }

    export function BlanquearRestrictoresDeEdicion(panel: HTMLDivElement) {
        let editores: NodeListOf<HTMLInputElement> = panel.querySelectorAll(`input[${atControl.tipo}="${ltrTipoControl.restrictorDeEdicion}"]`) as NodeListOf<HTMLInputElement>;
        for (let i = 0; i < editores.length; i++) {
            MapearAlControl.MapearEditor(editores[i], 0, "", true, true);
        }
    }

    function BlanquearSeleccionEnLasListasDeElementos(panel: HTMLDivElement) {
        let selectores: NodeListOf<HTMLSelectElement> = panel.querySelectorAll(`select[${atControl.tipo}="${ltrTipoControl.ListaDeElementos}"]`) as NodeListOf<HTMLSelectElement>;
        for (let i = 0; i < selectores.length; i++) {
            //selectores[i].setAttribute(atListas.yaCargado, 'N');
            selectores[i].setAttribute(atListasDeElemento.Cargando, 'N');
            ApiControl.SeleccionarElPrimeroYAplicarEditabilidad(selectores[i]);
        }
    }

    export function BlanquearArchivos(panel: HTMLDivElement) {
        let archivos: NodeListOf<HTMLInputElement> = panel.querySelectorAll(`${atControl.tipo}[${atControl.tipo}="${ltrTipoControl.Archivo}"]`) as NodeListOf<HTMLInputElement>;
        for (let i = 0; i < archivos.length; i++) {
            ApiDeArchivos.BlanquearArchivo(archivos[i], true);
        }

        archivos = panel.querySelectorAll(`${atControl.tipo}[${atControl.tipo}="${ltrTipoControl.SelectorDeUnArchivo}"]`) as NodeListOf<HTMLInputElement>;
        for (let i = 0; i < archivos.length; i++) {
            ApiDeArchivos.BlanquearArchivo(archivos[i], true);
        }
    }

    function BlanquearSelectoresDeElemento(modal: HTMLDivElement) {
        let selectores: NodeListOf<HTMLInputElement> = modal.querySelectorAll(`[${atControl.tipo}=${ltrTipoControl.SelectorDeElemento}]`) as NodeListOf<HTMLInputElement>;
        for (let i: number = 0; i < selectores.length; i++) {
            selectores[i].setAttribute(atSelectorDeElementos.Seleccionados, '');
            let idEditor = selectores[i].getAttribute(atSelectorDeElementos.EditorAsociado);
            let editor: HTMLInputElement = document.getElementById(idEditor) as HTMLInputElement;
            ApiControl.BlanquearEditor(editor);
        }
    }

    export function EliminarReferenciasDeUnDiv(modal: HTMLDivElement) {
        let referencias: NodeListOf<HTMLElement> = modal.querySelectorAll("a") as NodeListOf<HTMLElement>;
        for (let i: number = 0; i < referencias.length; i++) {
            referencias[i].remove();
        }
    }

    export function BuscarContenedorDeTablaDto(control: HTMLElement): HTMLDivElement {

        var tabla = ApiControl.BuscarTabla(control);
        if (Definido(tabla) && tabla.parentNode instanceof HTMLDivElement)
            return tabla.parentNode;

        let padre = control.parentElement;
        while (Definido(padre)) {
            if (padre.classList.contains(ltrCss.contenedorEdicionCuerpo))
                return padre as HTMLDivElement;
            padre = padre.parentElement;

        }
        return undefined;
    }

    export function BuscarTablaDto(modal: HTMLDivElement): HTMLDivElement | null {
        return modal.querySelector('.' + ltrCss.crud.tabla) as HTMLDivElement | null;
    }

    export function BuscarBodyDto(modal: HTMLDivElement): HTMLDivElement | null {
        return modal.querySelector('.' + ltrCss.crud.cuerpo) as HTMLDivElement | null;
    }

    export function MostrarOcultarFilaDtoSi(modal: HTMLDivElement, numero: number, mostrar: boolean): void {
        const fila = BuscarFilaDto(modal, numero);
        if (Definido(fila)) {
            if (mostrar)
                ApiControl.ExcluirCss(fila, ltrCss.filaNoVisible);
            else
                ApiControl.IncluirCss(fila, ltrCss.filaNoVisible);
        }
    }

    export function OcultarFilaDto(modal: HTMLDivElement, numero: number): void {
        const fila = BuscarFilaDto(modal, numero);
        if (Definido(fila)) {
            ApiControl.IncluirCss(fila, ltrCss.filaNoVisible);
        }
    }

    export function MostrarFilaDto(modal: HTMLDivElement, numero: number): void {
        const fila = BuscarFilaDto(modal, numero);
        if (Definido(fila)) {
            ApiControl.ExcluirCss(fila, ltrCss.filaNoVisible);
        }
    }

    export function BuscarFilaDto(modal: HTMLDivElement, fila: number): HTMLDivElement | null {
        // Busca el cuerpo por si se necesita, aunque en este caso no se usa directamente
        // const body = BuscarBodyDto(modal);
        // Selecciona todas las filas
        const filas = modal.querySelectorAll('.' + ltrCss.crud.fila) as NodeListOf<HTMLDivElement>;
        // Busca la fila cuyo id termina en `-' + fila`
        const filaEncontrada = Array.from(filas).find(div =>
            div.id.endsWith('-' + fila)
        );
        return filaEncontrada || null;
    }



    export function BuscarDtoDelFormulario(control: HTMLElement): HTMLDivElement {
        let padre = control.parentElement;
        while (Definido(padre)) {
            if (padre.classList.contains(ltrCss.formulario.dto))
                return padre as HTMLDivElement;
            padre = padre.parentElement;

        }
        return undefined;
    }
    export function BuscarContenedorDeMasDeUnControl(control: HTMLElement): HTMLDivElement {
        let padre = control.parentElement;
        while (Definido(padre)) {
            if (padre.classList.contains(ltrCss.contenedorConMasDeUnControl))
                return padre as HTMLDivElement;
            padre = padre.parentElement;
        }
        return undefined;
    }

    export function BuscarContenedor(control: HTMLElement, clase: string): HTMLDivElement {
        let padre = control.parentElement;
        while (Definido(padre)) {
            if (padre.classList.contains(clase))
                return padre as HTMLDivElement;
            padre = padre.parentElement;
        }
        return undefined;
    }

    export function BuscarGridPorControlador(panel: HTMLDivElement, controlador: string): HTMLDivElement {
        let div: HTMLDivElement = panel.querySelector((`div[${atControl.controlador}=${controlador}]`)) as HTMLDivElement;
        return div;
    }

    export function CopiarPorPropiedad(panel: HTMLDivElement, propiedad1: string, propiedad2: string, bloquear: boolean): void {
        let control1 = ApiControl.BuscarControl(panel, propiedad1, true);
        let control2 = ApiControl.BuscarControl(panel, propiedad2, true);
        ApiControl.AsignarControl(control1, control2, bloquear);
    }

    export function MostrarOcultarFila(panel: HTMLDivElement, propiedad: string, mostrar: boolean) {
        if (mostrar)
            MostrarFila(panel, propiedad);
        else
            OcultarFila(panel, propiedad);
    }

    export function MostrarOcultarCelda(panel: HTMLDivElement, propiedad: string, mostrar: boolean): HTMLDivElement {
        if (mostrar)
            return MostrarCelda(panel, propiedad);
        else
            return OcultarCelda(panel, propiedad);
    }

    export function OcultarFila(panel: HTMLDivElement, propiedad: string) {
        var fila = BuscarFilaPorPropiedad(panel, propiedad);
        if (Definido(fila)) {
            ApiControl.IncluirCss(fila, ltrCss.filaNoVisible);
        }
    }

    export function MostrarFila(panel: HTMLDivElement, propiedad: string) {
        var fila = BuscarFilaPorPropiedad(panel, propiedad);
        if (Definido(fila)) {
            ApiControl.ExcluirCss(fila, ltrCss.filaNoVisible);
        }
    }


    export function OcultarCelda(panel: HTMLDivElement, propiedad: string): HTMLDivElement {
        var celda = BuscarCeldaPorPropiedad(panel, propiedad);
        if (Definido(celda)) {
            celda.classList.add(ltrCss.celdaNoVisible);
        }
        return celda
    }

    export function MostrarCelda(panel: HTMLDivElement, propiedad: string): HTMLDivElement {
        var celda = BuscarCeldaPorPropiedad(panel, propiedad);
        if (Definido(celda)) {
            celda.classList.remove(ltrCss.celdaNoVisible);
        }
        return celda
    }

    export function BuscarModalContenedora(control: HTMLElement): HTMLDivElement {
        if (Definido(control)) {
            var padre = control.parentElement;
            do {
                if (padre.classList.contains(ltrCss.contenedorModal))
                    return padre as HTMLDivElement;
                padre = padre.parentElement;
            }
            while (Definido(padre));
        }
        return undefined;
    }


    export function BuscarFilaPorPropiedad(panel: HTMLDivElement, propiedad: string): HTMLDivElement {
        var control = ApiControl.BuscarControl(panel, propiedad, false);
        return ApiControl.BuscarFila(control);
    }

    export function BuscarCeldaPorPropiedad(panel: HTMLDivElement, propiedad: string): HTMLDivElement {
        var control = ApiControl.BuscarControl(panel, propiedad, false);
        return ApiControl.BuscarCelda(control);
    }

    export function MapearEditoresAlmacenables(panel: HTMLDivElement, datosParaGuardar: any) {
        var editores: NodeListOf<HTMLInputElement> = panel.querySelectorAll(`input[${atControl.esAlmacenable}="${literal.true}"]`) as NodeListOf<HTMLInputElement>;
        for (let i: number = 0; i < editores.length; i++) {
            let control: HTMLInputElement = editores[i];
            let tipoControl: string = control.getAttribute(atControl.tipo);
            switch (tipoControl) {
                case ltrTipoControl.Editor:
                    datosParaGuardar[control.getAttribute(atControl.propiedad)] = ApiControl.Valor(control);
                    break;
                case ltrTipoControl.ListaDinamica:
                    let idSeleccionado = Numero(control.getAttribute(atListasDinamicas.idSeleccionado));
                    if (idSeleccionado > 0) {
                        datosParaGuardar[control.getAttribute(atControl.propiedad)] = control.value;
                        datosParaGuardar[control.getAttribute(atListasDinamicasDto.guardarEn)] = idSeleccionado;
                    }
                    break;
                case ltrTipoControl.Check:
                    datosParaGuardar[control.getAttribute(atControl.propiedad)] = control.checked;
                    break;
                default: MensajesSe.Error("MapearEditoresAlmacenables", `No se ha indicado cómo almacenar el dato del tipo de control ${tipoControl} asociado a la propiedad ${control.getAttribute(atControl.propiedad)}`);
                    break;
            }
        }
    }

    export function MapearSelectoresAlmacenables(panel: HTMLDivElement, datosParaGuardar: any) {
        var listas: NodeListOf<HTMLSelectElement> = panel.querySelectorAll(`select[${atControl.esAlmacenable}="${literal.true}"]`) as NodeListOf<HTMLSelectElement>;
        for (let i: number = 0; i < listas.length; i++) {
            let control: HTMLSelectElement = listas[i];
            let tipoControl: string = control.getAttribute(atControl.tipo);
            let guardarEn: string = control.getAttribute(atListas.guardarEn);
            switch (tipoControl) {
                case ltrTipoControl.ListaDeValores:
                    if (Number(control.value) === -1) continue;
                    datosParaGuardar[guardarEn] = control.value;
                    break;
                case ltrTipoControl.ListaDeElementos:
                    if (Number(control.value) <= 0) continue;
                    let propiedad: string = control.getAttribute(atControl.propiedad);
                    datosParaGuardar[guardarEn] = control.options[control.selectedIndex].value;
                    datosParaGuardar[propiedad] = control.options[control.selectedIndex].label;
                    break;
                default: MensajesSe.Error("MapearSelectoresAlmacenables", `No se ha indicado cómo almacenar el dato del tipo de control ${tipoControl} asociado a la propiedad ${control.getAttribute(atControl.propiedad)}`);
                    break;
            }
        }
    }

    export function InicializarDivEscrolable(panel: HTMLDivElement, id: string): HTMLDivElement {

        const divExistente = document.getElementById(id) as HTMLDivElement;
        if (divExistente) {
            divExistente.innerHTML = "";
            return divExistente;
        }


        const divContenedor = document.createElement("div");
        divContenedor.id = id + '-contenedor';
        panel.appendChild(divContenedor);

        const h4Adjuntos = document.createElement('label');
        h4Adjuntos.textContent = 'Adjuntos';
        h4Adjuntos.classList.add(ltrCss.controlesDto.etiqueta);
        divContenedor.appendChild(h4Adjuntos);

        const div = document.createElement("div");
        div.id = id;
        div.style.maxHeight = "200px";
        div.style.overflow = "auto";
        divContenedor.appendChild(div)
        return div;
    }

    export function QuitarClaseDeMapeadoPoIa(panel: HTMLDivElement) {
        const controles = panel.querySelectorAll('.' + ltrCss.ia.mapeado) as NodeListOf<HTMLElement>;
        controles.forEach(control => {
            ApiControl.ExcluirCss(control, ltrCss.ia.mapeado);
            const nuevoControl = control.cloneNode(true) as HTMLElement;
            control.parentNode.replaceChild(nuevoControl, control);
        });
    }


    export function MapearListaDinamica(modal: HTMLDivElement, registro: any, propiedad: string, bloquear: boolean, errorSiNoSePuede: boolean): void {
        const lista = ApiControl.BuscarListaDinamicaPorPropiedad(modal, propiedad);
        if (!Definido(lista) && errorSiNoSePuede)
            MensajesSe.Error('MapearEnLaListaDinamicaDeLaModal', `No se ha localizado la lista dinámica con la propiedad '${propiedad}' en la modal '${modal.id}'`)

        const idPropiedad = 'id' + propiedad;
        const id = ObtenerPropiedad(registro, idPropiedad, undefined);
        if (!Definido(id) && errorSiNoSePuede)
            MensajesSe.Error('MapearEnLaListaDinamicaDeLaModal', `No se ha localizado en el registro el valor para el campo '${idPropiedad}' que se quiere mapear en la modal '${modal.id}' en la lista '${lista.id}'`)

        if (!Definido(id))
            return;

        const texto = ObtenerPropiedad(registro, propiedad, undefined);
        if (!Definido(texto) && errorSiNoSePuede)
            MensajesSe.Error('MapearEnLaListaDinamicaDeLaModal', `No se ha localizado en el registro el valor para el campo '${propiedad}' que se quiere mapear en la modal '${modal.id}' en la lista '${lista.id}'`)

        MapearAlControl.ListaDinamica(lista, id, texto, bloquear);
    }

}
