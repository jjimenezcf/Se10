namespace ApiVisorDeArchivos {

    // ─── Variables de estado del splitter datos/visor ────────────────────────

    var _cambiandoAncho = false;
    var _contenedorDeDatos: HTMLDivElement;
    var _contenedorDelVisor: HTMLDivElement;

    // ─── Variables de estado del splitter tabla/gráficos ─────────────────────

    var _cambiandoAnchoTabla = false;
    var _contenedorDeTabla: HTMLDivElement;

    // ─── Cálculo y ajuste del visor ──────────────────────────────────────────

    export function CalcularTamanoDelVisor(): void {
        var crud = Crud.crudMnt;
        var visor = crud.EstoyCreando ? crud.crudDeCreacion.DivVisor : crud.crudDeEdicion.ContenedorDelVisorDeArchivoConHistorial;
        if (!Definido(visor))
            return;

        var contenedorCabecera = crud.EstoyCreando ? crud.crudDeCreacion.ContenedorDeCabecera : crud.crudDeEdicion.ContenedorDeCabecera;
        var contenedorDelVisor = crud.EstoyCreando ? crud.crudDeCreacion.ContenedorDelVisor : crud.crudDeEdicion.ContenedorDelVisorDeArchivoConHistorial;
        var contenedorDeDatos = crud.EstoyCreando ? crud.crudDeCreacion.ContenedorDeDatos : crud.crudDeEdicion.ContenedorDeDatos;
        var contenedorDeDatosMasVisor = crud.EstoyCreando ? crud.crudDeCreacion.ContenedorDeDatosMasVisor : crud.crudDeEdicion.ContenedorDeDatosMasVisor;

        const anchoVisor = contenedorDelVisor.clientWidth;
        ApiVisorDeArchivos.AjustarAnchoPanelDelVisor(crud.EstoyCreando, contenedorCabecera, contenedorDeDatosMasVisor, contenedorDeDatos, contenedorDelVisor, anchoVisor);
    }

    export function AjustarAnchoPanelDelVisor(estoyCreando: boolean, ContenedorDeCabecera: HTMLDivElement, ContenedorDeDatosMasVisor: HTMLDivElement, ContenedorDeDatos: HTMLDivElement, ContenedorDelVisor: HTMLDivElement, anchoVisor: number): void {
        if (!Definido(ContenedorDeCabecera))
            return;
        const anchoVentana = Math.max(document.documentElement.clientWidth || 0, window.innerWidth || 0);
        const padding = estoyCreando ? 11 : 2;
        ContenedorDeCabecera.style.width = `${anchoVentana - padding}px`;
        const anchoCabecera = anchoVentana - padding;

        const anchoMinimoVisor = 200;
        anchoVisor = Math.max(anchoVisor, anchoMinimoVisor);

        const anchoMaximoVisor = anchoCabecera - 200;
        anchoVisor = Math.min(anchoVisor, anchoMaximoVisor);

        var crud = Crud.crudMnt;
        var splitter = crud.EstoyCreando ? crud.crudDeCreacion.Splitter : crud.crudDeEdicion.Splitter;
        const anchoSplitter = splitter.clientWidth;
        ContenedorDelVisor.style.width = `${anchoVisor - anchoSplitter - 5}px`;

        const anchoDatos = anchoCabecera - anchoVisor;
        ContenedorDeDatos.style.width = `${anchoDatos}px`;
        ContenedorDeDatosMasVisor.style.width = `${anchoCabecera}px`;
    }

    export function AjustarAnchoDeDatosMasVisor(): void {
        var crud = Crud.crudMnt;
        if (crud.EstoyCreando && crud.crudDeCreacion.IdArchivoMostrado > 0) {
            CalcularTamanoDelVisor();
        }
        else {
            if (crud.crudDeEdicion.IdArchivoMostrado > 0) {
                CalcularTamanoDelVisor();
            }
            crud.crudDeEdicion.ContenedorDelVisorDeArchivoConHistorial.style.maxHeight = crud.crudDeEdicion.ContenedorDeDatos.clientHeight + 'px';
        }
    }

    // ─── Renderización de archivos en el visor ───────────────────────────────

    export async function RenderizarUrlsEnVisor(crud: Crud.CrudMnt, idArchivo: number, nombre: string, ajustarVisor: boolean) {
        var visor = crud.EstoyCreando ? crud.crudDeCreacion.DivVisor : crud.crudDeEdicion.DivVisor;
        if (!Definido(visor))
            return;

        var contenedorDelVisor = crud.ContenedorDelVisor;
        var contenedorDeDatos = crud.EstoyCreando ? crud.crudDeCreacion.ContenedorDeDatos : crud.crudDeEdicion.ContenedorDeDatos;
        var contenedorCabecera = crud.EstoyCreando ? crud.crudDeCreacion.ContenedorDeCabecera : crud.crudDeEdicion.ContenedorDeCabecera;
        var contenedorDeDatosMasVisor = crud.EstoyCreando ? crud.crudDeCreacion.ContenedorDeDatosMasVisor : crud.crudDeEdicion.ContenedorDeDatosMasVisor;
        if (crud.EstoyEnMantenimiento) ajustarVisor = false;

        let input = contenedorDelVisor.getElementsByClassName(ltrCss.crud.panelDeEdicion.VisorDeNombreAnexados) as HTMLCollectionOf<HTMLInputElement>;

        visor.innerHTML = 'Cargando...';
        let url: string = undefined;
        if (crud.EstoyCreando) {
            let parametros = `idArchivo=${idArchivo}`;
            url = `/${Ajax.Archivos.controlador}/${Ajax.Archivos.accion.DescargarParaCrear}?${parametros}`;
        }
        else {
            let parametros = `negocio=${crud.NombreDeNegocio}`;
            parametros = `${parametros}&idElemento=${crud.EstoyEnMantenimiento ? crud.InfoSelector.IdsSeleccionados[0] : crud.crudDeEdicion.ElementoEditado.Id}`;
            parametros = `${parametros}&idArchivo=${idArchivo}`;
            parametros = `${parametros}&auditar=false`;
            url = `/${Ajax.Archivos.controlador}/${Ajax.Archivos.accion.Descargar}?${parametros}`;
        }

        try {
            const response = await fetch(url);
            const blob = await response.blob();
            if (!crud.EstoyCreando)
                ApiControl.ExcluirCss(crud.crudDeEdicion.BotonVisor, ltrCss.crud.panelDeEdicion.Acciones.SinVisor);
            const renderizado = await RenderizarBlobEnVisor(visor, blob, (accion) => `/${Ajax.Archivos.controlador}/${accion}?idArchivo=${idArchivo}`);
            if (!renderizado) {
                if (crud.EstoyCreando) {
                    ApiControl.IncluirCss(crud.crudDeCreacion.ContenedorDeDatosMasVisor, ltrCss.crud.panelCreacion.VisorOculto);
                    return;
                } else {
                    const linkElement = document.createElement('a');
                    linkElement.href = URL.createObjectURL(blob);
                    linkElement.textContent = `Descargar archivo`;
                    linkElement.download = nombre;
                    visor.innerHTML = '';
                    visor.appendChild(linkElement);
                }
            }
            if (crud.EstoyCreando)
                crud.crudDeCreacion.AsignarIdArchivo(idArchivo, ajustarVisor);
            else
                crud.crudDeEdicion.AsignarIdArchivo(idArchivo, ajustarVisor);

            input[0].value = nombre;
            if (ajustarVisor) {
                const contenedor = crud.EstoyCreando ? contenedorDelVisor : contenedorDelVisor.parentElement as HTMLDivElement;
                ApiVisorDeArchivos.AjustarAnchoPanelDelVisor(crud.EstoyCreando, contenedorCabecera, contenedorDeDatosMasVisor, contenedorDeDatos, contenedor, crud.TamanoDelVisor);
            }
        } catch (error) {
            visor.innerHTML = 'Error al cargar el archivo';
        }
    }

    // Pinta el blob en el visor según su tipo. Los tipos que se convierten a html en el servidor se piden a
    // urlDeConversion(accion), que en edición/creación apunta al endpoint de la conversión y en la consulta
    // por guid a su versión validada. Devuelve false si el tipo no se sabe previsualizar.
    export async function RenderizarBlobEnVisor(visor: HTMLDivElement, blob: Blob, urlDeConversion: (accion: string) => string): Promise<boolean> {
        const objectUrl = URL.createObjectURL(blob);
        if (blob.type.startsWith('image/')) {
            ApiPanel.RenderizarContenidoImagen(visor, `<img src="${objectUrl}" alt="Archivo descargado" style="max-width: 100%; height: auto;">`);
        }
        else if (blob.type === 'application/pdf') {
            ApiPanel.RenderizarContenidoPdf(visor, objectUrl);
        }
        else if (blob.type === 'application/xml' || blob.type === 'text/xml') {
            ApiPanel.RenderizarXml(visor, objectUrl);
        }
        else if (blob.type === 'text/csv') {
            ApiPanel.RenderizarUrlToHtml(visor, urlDeConversion(Ajax.Archivos.accion.DescargarCsvToHtml));
        }
        else if (blob.type === 'application/rtf') {
            ApiPanel.RenderizarUrlToHtml(visor, urlDeConversion(Ajax.Archivos.accion.DescargarRtfToHtml));
        }
        else if (blob.type === 'application/vnd.openxmlformats-officedocument.wordprocessingml.document') {
            ApiPanel.RenderizarUrlToHtml(visor, urlDeConversion(Ajax.Archivos.accion.DescargarDocxToHtml));
        }
        else if (blob.type === 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' || blob.type === 'application/vnd.ms-excel') {
            ApiPanel.RenderizarUrlToHtml(visor, urlDeConversion(Ajax.Archivos.accion.DescargarXlsxToHtml));
        }
        else if (blob.type === 'application/x-zip-compressed' || blob.type === 'application/x-7z-compressed') {
            ApiPanel.RenderizarUrlToHtml(visor, urlDeConversion(Ajax.Archivos.accion.DescargarZipToHtml));
        }
        else if (blob.type === 'text/html') {
            ApiPanel.RenderizarUrlToHtml(visor, urlDeConversion(Ajax.Archivos.accion.DescargarHtmlSanitizado));
        }
        else if (blob.type === 'text/plain' || blob.type === 'application/json' || blob.type === 'application/text' || blob.type === 'application/octet-stream') {
            const text = await blob.text();
            ApiPanel.RenderizarContenido(visor, text, (blob.type === 'text/plain' || blob.type === 'application/text' || blob.type === 'application/octet-stream') && !(text.indexOf('</html>') > 0)
                ? 'texto'
                : blob.type === 'application/json'
                    ? 'json'
                    : 'html');
        }
        else {
            URL.revokeObjectURL(objectUrl);
            return false;
        }
        return true;
    }

    export async function ProcesarRenderizar(crud: Crud.CrudMnt, idArchivo: number, accion: string): Promise<boolean> {
        const { visor, contenedorDelVisor } = obtenerElementosVisuales(crud);
        if (!visor) return false;

        const input = contenedorDelVisor.getElementsByClassName(ltrCss.crud.panelDeEdicion.VisorDeNombreAnexados)[0] as HTMLInputElement;

        if (await mapearDatosSiEsUnaFacturaJson(crud, idArchivo, accion))
            return true;

        actualizarMensajeVisor(visor, accion);

        const url = construirUrl(crud, idArchivo, accion);

        try {
            const resultado = await obtenerResultado(url);

            if (resultado.estado === 'Ok') {
                const resultadoProcesado = await procesarResultadoExitoso(crud, visor, resultado, accion);
                asignarIdArchivo(crud, resultadoProcesado.idArchivo === 0 ? idArchivo : resultadoProcesado.idArchivo);
                input.value = resultadoProcesado.nombreArchivo;
                if (accion === ltrEventos.Edicion.FacturasRec.Analizar)
                    await mapearDatosSiEsUnaFacturaJson(crud, resultadoProcesado.idArchivo, accion);
                return true;
            }
            else {
                manejarError(crud, contenedorDelVisor, idArchivo, resultado);
                return false;
            }
        } catch (error) {
            visor.innerHTML = `Error al '${accion}' del archivo`;
            MensajesSe.Error('ProcesarRenderizar', 'Error al analizar la factura, acceda a la consola', error);
            return false;
        }
    }

    // ─── Funciones privadas de apoyo ─────────────────────────────────────────

    function obtenerElementosVisuales(crud: Crud.CrudMnt): { visor: HTMLDivElement, contenedorDelVisor } {
        const visor = crud.EstoyCreando ? crud.crudDeCreacion.DivVisor : crud.crudDeEdicion.DivVisor;
        const contenedorDelVisor = crud.ContenedorDelVisor;
        return { visor, contenedorDelVisor };
    }

    function actualizarMensajeVisor(visor: HTMLElement, accion: string) {
        visor.innerHTML = accion === ltrEventos.Edicion.PasarOcr
            ? 'Pasando OCR...'
            : accion === ltrEventos.Edicion.ResumirArchivo
                ? 'Resumiendo...'
                : 'Analizando factura ...';
    }

    async function mapearDatosSiEsUnaFacturaJson(crud: Crud.CrudMnt, idArchivo: number, accion: string): Promise<boolean> {
        if (accion === ltrEventos.Edicion.FacturasRec.Analizar) {
            const resultado = await ApiDeArchivos.EsFicheroJson(crud.NombreDeNegocio, crud.EstoyCreando ? 0 : crud.crudDeEdicion.ElementoEditado.Id, idArchivo);
            if (resultado.esJson) {
                crud.MapearDatosJsonDesdeElVisor(resultado.json);
                return true;
            }
        }
        return false;
    }

    function construirUrl(crud: Crud.CrudMnt, idArchivo: number, accion: string): string {
        const parametros = new URLSearchParams({
            idArchivo: idArchivo.toString(),
            accion,
            negocio: crud.EnumeradoDeNegocio as string,
            idElemento: (crud.EstoyCreando ? 0 : crud.crudDeEdicion.ElementoEditado.Id).toString()
        });
        return `/${Ajax.Archivos.controlador}/${Ajax.Archivos.accion.ProcesarAccion}?${parametros}`;
    }

    async function obtenerResultado(url: string) {
        const response = await fetch(url);
        return await response.json();
    }

    async function procesarResultadoExitoso(crud: Crud.CrudMnt, visor: HTMLDivElement, resultado: any, accion: string): Promise<{ idArchivo: number, nombreArchivo: string }> {
        if (accion === ltrEventos.Edicion.FacturasRec.Analizar) {
            return facturaAnalizadaCorrectamente(crud, visor, resultado);
        } else {
            const idArchivo = Numero(resultado.datos);
            const nombreArchivo = accion === ltrEventos.Edicion.PasarOcr ? 'Ocr' : 'Resumido';
            await ApiPanel.RenderizarToHtml(visor, idArchivo, Ajax.Archivos.accion.DescargarHtmlSanitizado);
            return { idArchivo, nombreArchivo };
        }
    }

    async function facturaAnalizadaCorrectamente(crud: Crud.CrudMnt, visor: HTMLDivElement, resultado: any): Promise<{ idArchivo: number, nombreArchivo: string }> {
        if (typeof resultado.datos === 'object' && resultado.datos !== null && ltrPropiedades.Ia.IdArchivo in resultado.datos) {
            const idArchivo = ObtenerPropiedad(resultado.datos, ltrPropiedades.Ia.IdArchivo);
            const nombre = ObtenerPropiedad(resultado.datos, ltrPropiedades.Ia.Nombre);
            await RenderizarUrlsEnVisor(crud, idArchivo, nombre, false);
            if (crud.EstoyEditando) ApiDeArchivos.MostrarArchivosAnexados(
                crud.crudDeEdicion.PanelDeArchivos.id,
                crud.NombreDeNegocio,
                crud.crudDeEdicion.ElementoEditado.Id, null
            );
            return { idArchivo: idArchivo, nombreArchivo: nombre };
        }

        ApiPanel.RenderizarContenido(visor, resultado.datos, 'json');
        return { idArchivo: 0, nombreArchivo: "Factura analizada" };
    }

    function manejarError(crud: Crud.CrudMnt, contenedorDelVisor: HTMLElement, idArchivo: number, resultado: any) {
        MensajesSe.Error("ProcesarRenderizar", resultado.mensaje, resultado.consola);
        const input = contenedorDelVisor.getElementsByClassName(ltrCss.crud.panelDeEdicion.VisorDeNombreAnexados)[0] as HTMLInputElement;
        RenderizarUrlsEnVisor(crud, idArchivo, input.value, false);
    }

    function asignarIdArchivo(crud: Crud.CrudMnt, idArchivoResumido: number) {
        if (crud.EstoyCreando) {
            crud.crudDeCreacion.AsignarIdArchivo(idArchivoResumido, false);
        } else {
            crud.crudDeEdicion.AsignarIdArchivo(idArchivoResumido, false);
        }
    }

    // ─── Splitter datos/visor ────────────────────────────────────────────────

    export function ConfigurarEventosDeCambioDelAnchoContenedorDeDatos() {
        var crud = Crud.crudMnt;
        var contenedorDeDatosMasVisor = crud.EstoyCreando ? crud.crudDeCreacion.ContenedorDeDatosMasVisor : crud.crudDeEdicion.ContenedorDeDatosMasVisor;
        var splitter = crud.EstoyCreando ? crud.crudDeCreacion.Splitter : crud.crudDeEdicion.Splitter;

        ResetearParametrosDeArrastre();

        ApiControl.IncluirCss(contenedorDeDatosMasVisor, crud.EstoyCreando ? ltrCss.crud.panelCreacion.VisorOculto : ltrCss.crud.panelDeEdicion.VisorOculto);
        // se llama cada vez que se entra en creación o edición, pero el splitter sólo se inicializa la primera vez
        ApiPanel.InicializarSplitter(splitter, {
            alEmpezar: () => ComienzoCambioDelAnchoContenedorDeDatos(),
            alMover: (clientX: number) => CambiarDeAnchoDelContenedorDeDatos(clientX),
            alSoltar: () => FinalizarCambioDeAnchoDelContenedorDeDatos()
        });
    }

    function ComienzoCambioDelAnchoContenedorDeDatos() {
        var crud = Crud.crudMnt;
        var contenedorDeDatos = crud.EstoyCreando ? crud.crudDeCreacion.ContenedorDeDatos : crud.crudDeEdicion.ContenedorDeDatos;
        var contenedorCabecera = crud.EstoyCreando ? crud.crudDeCreacion.ContenedorDeCabecera : crud.crudDeEdicion.ContenedorDeCabecera;
        var contenedorDelVisor = crud.EstoyCreando ? crud.crudDeCreacion.ContenedorDelVisor : crud.crudDeEdicion.ContenedorDelVisor;
        var contenedorDeDatosMasVisor = crud.EstoyCreando ? crud.crudDeCreacion.ContenedorDeDatosMasVisor : crud.crudDeEdicion.ContenedorDeDatosMasVisor;

        _cambiandoAncho = true;
        _contenedorDeDatos = contenedorDeDatos;
        _contenedorDelVisor = contenedorDelVisor;
        contenedorCabecera.style.width = "auto";
        contenedorDeDatosMasVisor.style.width = "auto";
    }

    function CambiarDeAnchoDelContenedorDeDatos(clientX: number) {
        if (!_cambiandoAncho || !Definido(_contenedorDeDatos)) return;

        const crud = Crud.crudMnt;
        const splitter = crud.EstoyCreando ? crud.crudDeCreacion.Splitter : crud.crudDeEdicion.Splitter;
        const contenedorEditorRect = _contenedorDeDatos.parentElement.getBoundingClientRect();

        // si el ratón se sale de los límites la barra se queda en el tope, en vez de dejar de seguirlo
        const minWidth = 100;
        const nuevoAnchoDatos = Math.min(Math.max(clientX - contenedorEditorRect.left, minWidth), contenedorEditorRect.width - 10 - minWidth);
        const nuevoAnchoVisor = contenedorEditorRect.width - nuevoAnchoDatos - 10;

        _contenedorDeDatos.style.width = `${nuevoAnchoDatos}px`;
        _contenedorDelVisor.style.width = `${nuevoAnchoVisor - splitter.clientWidth}px`;
        if (!crud.EstoyCreando) _contenedorDelVisor.parentElement.style.width = `${nuevoAnchoVisor - splitter.clientWidth}px`;
        splitter.style.left = `${nuevoAnchoDatos}px`;
    }

    function FinalizarCambioDeAnchoDelContenedorDeDatos() {
        if (!_cambiandoAncho) return;

        var crud = Crud.crudMnt;
        try {
            ApiVisorDeArchivos.AjustarAnchoDeDatosMasVisor();
            GuardarTamanoDelVisor(crud);
        }
        finally {
            ResetearParametrosDeArrastre();
        }
    }

    function ResetearParametrosDeArrastre() {
        _cambiandoAncho = false;
        _contenedorDeDatos = undefined;
        _contenedorDelVisor = undefined;
    }

    async function GuardarTamanoDelVisor(crud: Crud.CrudMnt) {
        const params2 = {
            [Ajax.Param.idNegocio]: Encriptar(literal.ClaveDeEncriptacion, crud.IdNegocio),
            [Ajax.Param.idVista]: Encriptar(literal.ClaveDeEncriptacion, crud.IdVista),
            [Ajax.Param.peticion]: Encriptar(literal.ClaveDeEncriptacion, ltrMenus.eventosDeMf.Comun.TamanoDelVisor)
        };
        const url2 = `/${crud.Controlador}/${Ajax.EndPoint.ProcesarPeticion}?${new URLSearchParams(params2)}`;
        await fetch(url2, {
            method: 'POST',
            body: TamanoDelVisor(crud),
            keepalive: true
        });
    }

    function TamanoDelVisor(crud: Crud.CrudMnt) {
        let parametros: Array<Parametro> = new Array<Parametro>();
        let datosParaGuardar = Numero(_contenedorDelVisor.style.width.replace('px', ''));
        parametros.push(new Parametro(Ajax.Param.datosPeticion, datosParaGuardar));
        crud.TamanoDelVisor = datosParaGuardar;
        return JSON.stringify(parametros);
    }

    export async function GuardarMostrarVisorAlIniciar(crud: Crud.CrudMnt, mostrar: boolean) {
        const params2 = {
            [Ajax.Param.idNegocio]: Encriptar(literal.ClaveDeEncriptacion, crud.IdNegocio),
            [Ajax.Param.idVista]: Encriptar(literal.ClaveDeEncriptacion, crud.IdVista),
            [Ajax.Param.peticion]: Encriptar(literal.ClaveDeEncriptacion, ltrMenus.eventosDeMf.Comun.MostrarVisorAlIniciar)
        };

        let parametros: Array<Parametro> = new Array<Parametro>();
        parametros.push(new Parametro(Ajax.Param.datosPeticion, mostrar));

        const url2 = `/${crud.Controlador}/${Ajax.EndPoint.ProcesarPeticion}?${new URLSearchParams(params2)}`;
        await fetch(url2, {
            method: 'POST',
            body: JSON.stringify(parametros),
            keepalive: true
        });
    }

    // ─── Splitter tabla/gráficos ─────────────────────────────────────────────

    export function ConfigurarEventosDeCambioDelAnchoContenedorDeTablaConGraficos() {
        const crud = Crud.crudMnt;
        if (!Definido(crud.ContenedorDeTablaConGraficos))
            return;

        ResetearParametrosDeArrastreDeGraficos();
        // el splitter sólo se inicializa la primera vez que se llama
        ApiPanel.InicializarSplitter(crud.Splitter, {
            alEmpezar: () => ComienzoCambioDelAnchoContenedorDeTablaConGraficos(),
            alMover: (clientX: number) => CambiarDeAnchoDelContenedorDeTablaConGraficos(clientX),
            alSoltar: () => FinalizarCambioDeAnchoDelContenedorDeTablaConGraficos()
        });
    }

    function ComienzoCambioDelAnchoContenedorDeTablaConGraficos() {
        const crud = Crud.crudMnt;
        _cambiandoAnchoTabla = true;
        _contenedorDeTabla = crud.ContenedorDeTabla;
    }

    function CambiarDeAnchoDelContenedorDeTablaConGraficos(clientX: number) {
        if (!_cambiandoAnchoTabla || !Definido(_contenedorDeTabla)) return;

        const crud = Crud.crudMnt;
        const contenedorPrincipalRect = _contenedorDeTabla.parentElement.getBoundingClientRect();
        const anchoTotal = contenedorPrincipalRect.width;
        const anchoSplitter = crud.Splitter.clientWidth;

        // si el ratón se sale de los límites la barra se queda en el tope, en vez de dejar de seguirlo
        const minWidth = 100;
        const nuevoAnchoTabla = Math.min(Math.max(clientX - contenedorPrincipalRect.left, minWidth), anchoTotal - anchoSplitter - minWidth);

        // div-graficos ocupa por CSS (flex: 1) lo que queda a la derecha del splitter
        _contenedorDeTabla.style.width = `${nuevoAnchoTabla}px`;
    }

    function FinalizarCambioDeAnchoDelContenedorDeTablaConGraficos() {
        if (!_cambiandoAnchoTabla) return;

        // GuardarTamanoDeGraficos(crud);
        ResetearParametrosDeArrastreDeGraficos();
    }

    function ResetearParametrosDeArrastreDeGraficos() {
        _cambiandoAnchoTabla = false;
        _contenedorDeTabla = undefined;
    }

    export function OcultarContenedorDeGraficos(): boolean {
        const crud = Crud.crudMnt;
        if (!Definido(crud.ContenedorDeTablaConGraficos)) return false;

        const contenedorTabla = crud.ContenedorDeTabla;
        const splitter = crud.Splitter;
        const contenedorDeGraficos = crud.ContenedorDeGraficos;

        const contenedorPrincipalRect = contenedorTabla.parentElement.getBoundingClientRect();
        const nuevoAnchoTabla = contenedorPrincipalRect.width;

        contenedorTabla.style.width = `${nuevoAnchoTabla}px`;
        splitter.style.removeProperty('width');
        contenedorDeGraficos.style.removeProperty('width');
        return true;
    }

    export function MostrarContenedorDeGraficos(): boolean {
        const crud = Crud.crudMnt;
        if (!Definido(crud.ContenedorDeTablaConGraficos)) return false;

        const contenedorDeTabla = crud.ContenedorDeTabla;
        const contenedorPrincipalRect = contenedorDeTabla.parentElement.getBoundingClientRect();
        const anchoTotal = contenedorPrincipalRect.width;
        const anchoSplitter = 6;

        let nuevoAnchoTabla: number;
        if (crud.VistaDeFichasActiva) {
            // El tablero de fichas no ocupa un 60% fijo: como mucho, la suma del ancho de
            // sus encolumnados (su ancho de contenido real). Solo si eso dejara menos de un
            // 50% para datos/totales/documento, se limita al 50% (y el propio overflow-x:auto
            // de div-vista-fichas muestra el scroll horizontal).
            // El elemento todavía conserva el ancho (100%/60%/50%) que le dejó la última
            // Ocultar/MostrarContenedorDeGraficos, así que su scrollWidth de por sí ya sale
            // "estirado" a ese ancho aunque el contenido (p.ej. una sola columna) sea más
            // estrecho; se fuerza brevemente a que se ajuste a su contenido para medir el
            // ancho real, y se restaura antes de decidir el nuevo ancho.
            const anchoPrevio = contenedorDeTabla.style.width;
            contenedorDeTabla.style.width = 'max-content';
            const anchoNatural = contenedorDeTabla.scrollWidth;
            contenedorDeTabla.style.width = anchoPrevio;

            const anchoGraficosSiNatural = anchoTotal - anchoNatural - anchoSplitter;
            nuevoAnchoTabla = anchoGraficosSiNatural >= anchoTotal * 0.5 ? anchoNatural : anchoTotal * 0.5;
        }
        else {
            nuevoAnchoTabla = anchoTotal * 60 / 100;
        }

        const nuevoAnchoGraficos = anchoTotal - nuevoAnchoTabla - anchoSplitter;

        const minWidth = 100;
        if (nuevoAnchoTabla < minWidth || nuevoAnchoGraficos < minWidth) return;

        // div-graficos ocupa por CSS (flex: 1) lo que queda a la derecha del splitter
        contenedorDeTabla.style.width = `${nuevoAnchoTabla}px`;
        return true;
    }

    async function GuardarTamanoDeGraficos(crud: Crud.CrudMnt) {
        const params2 = {
            [Ajax.Param.idNegocio]: Encriptar(literal.ClaveDeEncriptacion, crud.IdNegocio),
            [Ajax.Param.peticion]: Encriptar(literal.ClaveDeEncriptacion, ltrMenus.eventosDeMf.Comun.TamanoDelVisor)
        };
        const url2 = `/${crud.Controlador}/${Ajax.EndPoint.ProcesarPeticion}?${new URLSearchParams(params2)}`;
        await fetch(url2, {
            method: 'POST',
            body: TamanoDelVisor(crud),
            keepalive: true
        });
    }

}
