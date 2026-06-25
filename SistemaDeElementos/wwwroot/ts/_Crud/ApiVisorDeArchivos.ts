namespace ApiVisorDeArchivos {

    // ─── Variables de estado del splitter datos/visor ────────────────────────

    var _cambiandoAncho = false;
    var _posicioInicial: number;
    var _anchoInicial: number;
    let _lastExecution = 0;
    var _contenedorDeDatos: HTMLDivElement;
    var _contenedorDelVisor: HTMLDivElement;

    // ─── Variables de estado del splitter tabla/gráficos ─────────────────────

    var _cambiandoAnchoTabla = false;
    var _posicioInicialSplitter: number;
    var _anchoInicialTabla: number;
    let _ultimaEjecucion = 0;
    var _contenedorDeTabla: HTMLDivElement;
    var _contenedorDeGraficos: HTMLDivElement;

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

        var contenedorDelVisor = crud.EstoyCreando ? crud.crudDeCreacion.ContenedorDelVisor : crud.crudDeEdicion.ContenedorDelVisor;
        var contenedorDeDatos = crud.EstoyCreando ? crud.crudDeCreacion.ContenedorDeDatos : crud.crudDeEdicion.ContenedorDeDatos;
        var contenedorCabecera = crud.EstoyCreando ? crud.crudDeCreacion.ContenedorDeCabecera : crud.crudDeEdicion.ContenedorDeCabecera;
        var contenedorDeDatosMasVisor = crud.EstoyCreando ? crud.crudDeCreacion.ContenedorDeDatosMasVisor : crud.crudDeEdicion.ContenedorDeDatosMasVisor;

        let input = contenedorDelVisor.getElementsByClassName(ltrCss.crud.panelDeEdicion.VisorDeNombreAnexados) as HTMLCollectionOf<HTMLInputElement>;

        visor.innerHTML = 'Cargando...';
        let url: string = undefined;
        if (crud.EstoyCreando) {
            let parametros = `idArchivo=${idArchivo}`;
            url = `/${Ajax.Archivos.controlador}/${Ajax.Archivos.accion.DescargarParaCrear}?${parametros}`;
        }
        else {
            let parametros = `negocio=${crud.NombreDeNegocio}`;
            parametros = `${parametros}&idElemento=${crud.EstoyCreando ? 0 : crud.crudDeEdicion.ElementoEditado.Id}`;
            parametros = `${parametros}&idArchivo=${idArchivo}`;
            parametros = `${parametros}&auditar=false`;
            url = `/${Ajax.Archivos.controlador}/${Ajax.Archivos.accion.Descargar}?${parametros}`;
        }

        try {
            const response = await fetch(url);
            const blob = await response.blob();
            if (!crud.EstoyCreando)
                ApiControl.ExcluirCss(crud.crudDeEdicion.BotonVisor, ltrCss.crud.panelDeEdicion.Acciones.SinVisor);
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
                ApiPanel.RenderizarToHtml(visor, idArchivo, Ajax.Archivos.accion.DescargarCsvToHtml);
            }
            else if (blob.type === 'application/rtf') {
                ApiPanel.RenderizarToHtml(visor, idArchivo, Ajax.Archivos.accion.DescargarRtfToHtml);
            }
            else if (blob.type === 'application/vnd.openxmlformats-officedocument.wordprocessingml.document') {
                ApiPanel.RenderizarToHtml(visor, idArchivo, Ajax.Archivos.accion.DescargarDocxToHtml);
            }
            else if (blob.type === 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' || blob.type === 'application/vnd.ms-excel') {
                ApiPanel.RenderizarToHtml(visor, idArchivo, Ajax.Archivos.accion.DescargarXlsxToHtml);
            }
            else if (blob.type === 'application/x-zip-compressed' || blob.type === 'application/x-7z-compressed') {
                ApiPanel.RenderizarToHtml(visor, idArchivo, Ajax.Archivos.accion.DescargarZipToHtml);
            }
            else if (blob.type === 'text/html') {
                ApiPanel.RenderizarToHtml(visor, idArchivo, Ajax.Archivos.accion.DescargarHtmlSanitizado);
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
                if (crud.EstoyCreando) {
                    ApiControl.IncluirCss(crud.crudDeCreacion.ContenedorDeDatosMasVisor, ltrCss.crud.panelCreacion.VisorOculto);
                    return;
                } else {
                    const linkElement = document.createElement('a');
                    linkElement.href = objectUrl;
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
        const contenedorDelVisor = crud.EstoyCreando ? crud.crudDeCreacion.ContenedorDelVisor : crud.crudDeEdicion.ContenedorDelVisor;
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

        _cambiandoAncho = false;
        _posicioInicial = undefined;
        _anchoInicial = undefined;
        _contenedorDeDatos = undefined;
        _contenedorDelVisor = undefined;

        ApiControl.IncluirCss(contenedorDeDatosMasVisor, crud.EstoyCreando ? ltrCss.crud.panelCreacion.VisorOculto : ltrCss.crud.panelDeEdicion.VisorOculto);
        splitter.addEventListener('mousedown', (e: MouseEvent) => {
            ComienzoCambioDelAnchoContenedorDeDatos(e);
        });
    }

    function ComienzoCambioDelAnchoContenedorDeDatos(e: MouseEvent) {
        e.preventDefault();
        e.stopPropagation();

        var crud = Crud.crudMnt;
        var contenedorDeDatos = crud.EstoyCreando ? crud.crudDeCreacion.ContenedorDeDatos : crud.crudDeEdicion.ContenedorDeDatos;
        var contenedorCabecera = crud.EstoyCreando ? crud.crudDeCreacion.ContenedorDeCabecera : crud.crudDeEdicion.ContenedorDeCabecera;
        var contenedorDelVisor = crud.EstoyCreando ? crud.crudDeCreacion.ContenedorDelVisor : crud.crudDeEdicion.ContenedorDelVisor;
        var contenedorDeDatosMasVisor = crud.EstoyCreando ? crud.crudDeCreacion.ContenedorDeDatosMasVisor : crud.crudDeEdicion.ContenedorDeDatosMasVisor;
        var splitter = crud.EstoyCreando ? crud.crudDeCreacion.Splitter : crud.crudDeEdicion.Splitter;

        _cambiandoAncho = true;
        _posicioInicial = e.clientX;
        _anchoInicial = contenedorDeDatos.offsetWidth;
        _contenedorDeDatos = contenedorDeDatos;
        _contenedorDelVisor = contenedorDelVisor;
        contenedorCabecera.style.width = "auto";
        contenedorDeDatosMasVisor.style.width = "auto";

        document.addEventListener('mousemove', CambiarDeAnchoDelContenedorDeDatos.bind(splitter));
        document.addEventListener('mouseup', FinalizarCambioDeAnchoDelContenedorDeDatos.bind(splitter));
        document.addEventListener('mouseleave', FinalizarCambioDeAnchoDelContenedorDeDatos.bind(splitter));
    }

    function CambiarDeAnchoDelContenedorDeDatos(e: MouseEvent) {
        e.preventDefault();
        e.stopPropagation();

        if ((e.buttons & 1) === 0) {
            FinalizarCambioDeAnchoDelContenedorDeDatos(e);
            return;
        }

        if (!_cambiandoAncho) return;

        if (_lastExecution && Date.now() - _lastExecution < 16) return;
        _lastExecution = Date.now();

        const crud = Crud.crudMnt;
        const splitter = crud.EstoyCreando ? crud.crudDeCreacion.Splitter : crud.crudDeEdicion.Splitter;

        const contenedorEditorRect = _contenedorDeDatos.parentElement.getBoundingClientRect();
        const splitterRect = splitter.getBoundingClientRect();

        const margenAmpliado = 50;
        const dentroDelRango =
            e.clientX >= contenedorEditorRect.left &&
            e.clientX <= contenedorEditorRect.right &&
            e.clientX >= splitterRect.left - margenAmpliado &&
            e.clientX <= splitterRect.right + margenAmpliado;

        if (!dentroDelRango) {
            FinalizarCambioDeAnchoDelContenedorDeDatos(e);
            return;
        }

        const nuevoAnchoDatos = e.clientX - contenedorEditorRect.left;
        const nuevoAnchoVisor = contenedorEditorRect.width - nuevoAnchoDatos - 10;

        const minWidth = 100;
        if (nuevoAnchoDatos < minWidth || nuevoAnchoVisor < minWidth) return;

        requestAnimationFrame(() => {
            if (!Definido(_contenedorDeDatos)) return;
            _contenedorDeDatos.style.width = `${nuevoAnchoDatos}px`;
            _contenedorDelVisor.style.width = `${nuevoAnchoVisor - splitter.clientWidth}px`;
            if (!crud.EstoyCreando) _contenedorDelVisor.parentElement.style.width = `${nuevoAnchoVisor - splitter.clientWidth}px`;
            splitter.style.left = `${nuevoAnchoDatos}px`;
        });
    }

    function FinalizarCambioDeAnchoDelContenedorDeDatos(e: MouseEvent) {
        var crud = Crud.crudMnt;
        var splitter = crud.EstoyCreando ? crud.crudDeCreacion.Splitter : crud.crudDeEdicion.Splitter;
        if (!_cambiandoAncho || _posicioInicial === undefined) return;

        console.log("Terminar el arrastre:");
        try {
            ApiVisorDeArchivos.AjustarAnchoDeDatosMasVisor();
            GuardarTamanoDelVisor(crud);
        }
        finally {
            document.removeEventListener('mousemove', CambiarDeAnchoDelContenedorDeDatos.bind(splitter));
            document.removeEventListener('mouseup', FinalizarCambioDeAnchoDelContenedorDeDatos.bind(splitter));
            document.removeEventListener('mouseleave', FinalizarCambioDeAnchoDelContenedorDeDatos.bind(splitter));
            ResetearParametrosDeArrastre();
        }
    }

    function ResetearParametrosDeArrastre() {
        _cambiandoAncho = false;
        _posicioInicial = undefined;
        _anchoInicial = undefined;
        _contenedorDeDatos = undefined;
        _contenedorDelVisor = undefined;
        _lastExecution = 0;
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
        const splitter = crud.Splitter;

        _cambiandoAnchoTabla = false;
        _posicioInicialSplitter = undefined;
        _anchoInicialTabla = undefined;
        _contenedorDeTabla = undefined;
        _contenedorDeGraficos = undefined;

        splitter.addEventListener('mousedown', (e: MouseEvent) => {
            ComienzoCambioDelAnchoContenedorDeTablaConGraficos(e);
        });
    }

    function ComienzoCambioDelAnchoContenedorDeTablaConGraficos(e: MouseEvent) {
        e.preventDefault();
        e.stopPropagation();

        const crud = Crud.crudMnt;
        const contenedorDeTablaConGraficos = crud.ContenedorDeTablaConGraficos;
        const splitter = crud.Splitter;
        const contenedorDeTabla = crud.ContenedorDeTabla;
        const contenedorDeGraficos = crud.ContenedorDeGraficos;

        _cambiandoAnchoTabla = true;
        _posicioInicialSplitter = e.clientX;
        _anchoInicialTabla = contenedorDeTabla.offsetWidth;
        _contenedorDeTabla = contenedorDeTabla;
        _contenedorDeGraficos = contenedorDeGraficos;

        document.addEventListener('mousemove', CambiarDeAnchoDelContenedorDeTablaConGraficos.bind(splitter));
        document.addEventListener('mouseup', FinalizarCambioDeAnchoDelContenedorDeTablaConGraficos.bind(splitter));
        document.addEventListener('mouseleave', FinalizarCambioDeAnchoDelContenedorDeTablaConGraficos.bind(splitter));
    }

    export function CambiarDeAnchoDelContenedorDeTablaConGraficos(e: MouseEvent) {
        e.preventDefault();
        e.stopPropagation();

        if ((e.buttons & 1) === 0) {
            FinalizarCambioDeAnchoDelContenedorDeTablaConGraficos(e);
            return;
        }

        if (!_cambiandoAnchoTabla) return;

        if (_ultimaEjecucion && Date.now() - _ultimaEjecucion < 16) return;
        _ultimaEjecucion = Date.now();

        const crud = Crud.crudMnt;
        const splitter = crud.Splitter;

        const contenedorPrincipalRect = _contenedorDeTabla.parentElement.getBoundingClientRect();
        const nuevoAnchoTabla = e.clientX - contenedorPrincipalRect.left;
        const anchoTotal = contenedorPrincipalRect.width;
        const anchoSplitter = splitter.clientWidth;
        const nuevoAnchoGraficos = anchoTotal - nuevoAnchoTabla - anchoSplitter;

        const minWidth = 100;
        if (nuevoAnchoTabla < minWidth || nuevoAnchoGraficos < minWidth) return;

        requestAnimationFrame(() => {
            if (!Definido(_contenedorDeTabla) || !Definido(_contenedorDeGraficos)) return;
            _contenedorDeTabla.style.width = `${nuevoAnchoTabla}px`;
            _contenedorDeGraficos.style.width = `${nuevoAnchoGraficos}px`;
        });
    }

    function FinalizarCambioDeAnchoDelContenedorDeTablaConGraficos(e: MouseEvent) {
        var crud = Crud.crudMnt;
        var splitter = crud.Splitter;
        if (!_cambiandoAnchoTabla || _posicioInicialSplitter === undefined) return;

        try {
            // GuardarTamanoDeGraficos(crud);
        }
        finally {
            document.removeEventListener('mousemove', CambiarDeAnchoDelContenedorDeTablaConGraficos.bind(splitter));
            document.removeEventListener('mouseup', FinalizarCambioDeAnchoDelContenedorDeTablaConGraficos.bind(splitter));
            document.removeEventListener('mouseleave', FinalizarCambioDeAnchoDelContenedorDeTablaConGraficos.bind(splitter));
            ResetearParametrosDeArrastreDeGraficos();
        }
    }

    function ResetearParametrosDeArrastreDeGraficos() {
        _cambiandoAnchoTabla = false;
        _posicioInicialSplitter = undefined;
        _anchoInicialTabla = undefined;
        _contenedorDeTabla = undefined;
        _contenedorDeGraficos = undefined;
        _ultimaEjecucion = 0;
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

        const contenedorPrincipalRect = crud.ContenedorDeTabla.parentElement.getBoundingClientRect();
        const anchoTotal = contenedorPrincipalRect.width;
        const nuevoAnchoTabla = contenedorPrincipalRect.width * 60 / 100;
        const anchoSplitter = 6;
        const nuevoAnchoGraficos = anchoTotal - nuevoAnchoTabla - anchoSplitter;

        const minWidth = 100;
        if (nuevoAnchoTabla < minWidth || nuevoAnchoGraficos < minWidth) return;

        crud.ContenedorDeTabla.style.width = `${nuevoAnchoTabla}px`;
        crud.ContenedorDeGraficos.style.width = `${nuevoAnchoGraficos}px`;
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
