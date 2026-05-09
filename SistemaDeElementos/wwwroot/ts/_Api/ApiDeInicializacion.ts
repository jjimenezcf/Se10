namespace ApiDeInicializacion {

    export function InicializarOpcionesDeMenu(contenedorDeBotones: HTMLDivElement): void {
        let opcionesDeElemento: NodeListOf<HTMLButtonElement> = contenedorDeBotones.querySelectorAll(`input[${atOpcionDeMenu.clase}="${enumCssOpcionMenu.DeVista}"]`) as NodeListOf<HTMLButtonElement>;
        for (var i = 0; i < opcionesDeElemento.length; i++) {
            let opcion: HTMLButtonElement = opcionesDeElemento[i];
            opcion.disabled = true;
        }
    }

    export function InicializarNavegadoresDeRestrictoresDeEdicion(panel: HTMLDivElement): void {
        let restrictores: NodeListOf<HTMLInputElement> = panel.querySelectorAll(`input[${atControl.tipo}="${ltrTipoControl.restrictorDeEdicion}"]`) as NodeListOf<HTMLInputElement>;

        for (let i = 0; i < restrictores.length; i++) {
            let restrictor = restrictores[i];
            if (!EsTrue(restrictor.getAttribute(atControl.conNavegador)))
                continue;

            let id = Numero(restrictor.getAttribute(atControl.restrictor));
            MapearAlControl.DefinirNavegador(restrictor, id);
        }
    }


    export function InicializarOpcionesDeMenuDeElemento(contenedorDeBotones: HTMLDivElement): void {
        let opcionesDeElemento: NodeListOf<HTMLButtonElement> = contenedorDeBotones.querySelectorAll(`input[${atOpcionDeMenu.clase}="${enumCssOpcionMenu.DeElemento}"]`) as NodeListOf<HTMLButtonElement>;
        for (var i = 0; i < opcionesDeElemento.length; i++) {
            let opcion: HTMLButtonElement = opcionesDeElemento[i];
            opcion.disabled = true;
        }
    }

    export function InicializarEditores(panel: HTMLDivElement): void {
        let editores: NodeListOf<HTMLInputElement> = panel.querySelectorAll(`input[${atControl.tipo}='${ltrTipoControl.Editor}']`) as NodeListOf<HTMLInputElement>;
        for (var i = 0; i < editores.length; i++) {
            var editor = editores[i] as HTMLInputElement;
            if (editor.hasAttribute(atControl.valorPorDefecto))
                editor.value = editor.getAttribute(atControl.valorPorDefecto);
            else
                editor.value = "";
            if (Definido(editor.getAttribute(atControl.editable)))
                editor.disabled = !EsTrue(editor.getAttribute(atControl.editable));
        }
    }

    export function Checkes(panel: HTMLDivElement): void {
        try {
            let checkes: NodeListOf<HTMLInputElement> = panel.querySelectorAll(`input[${atControl.tipo}='${ltrTipoControl.Check}']`) as NodeListOf<HTMLInputElement>;
            for (var i = 0; i < checkes.length; i++) {
                var check = checkes[i] as HTMLInputElement;
                let valor = check.hasAttribute(atControl.valorPorDefecto) ?
                    EsTrue(check.getAttribute(atControl.valorPorDefecto)) :
                    EsTrue(check.getAttribute(atControl.valorInput));

                if (valor != check.checked) {
                    check.checked = valor;
                    check.dispatchEvent(new Event('click'));
                }
                check.disabled = !EsTrue(check.getAttribute(atControl.editable));
            }

        }
        catch (error) {
            console.log("Error al procesar la inicialización de check: " + error.message);
        }
    }

    export function Editor(editor: HTMLInputElement): void {
        ApiControl.BlanquearEditor(editor);
        editor.disabled = !EsTrue(editor.getAttribute(atControl.editable));
        editor.readOnly = !EsTrue(editor.getAttribute(atControl.editable));
    }

    export function AreasDeTexto(panel: HTMLDivElement) {
        let areas: NodeListOf<HTMLTextAreaElement> = panel.querySelectorAll(`textarea[${atControl.tipo}="${ltrTipoControl.AreaDeTexto}"]`) as NodeListOf<HTMLTextAreaElement>;
        for (var i = 0; i < areas.length; i++) {
            let area: HTMLTextAreaElement = areas[i] as HTMLTextAreaElement;
            let valorPorDefecto = area.getAttribute(atControl.valorPorDefecto);
            MapearAlControl.MapearAreaDeTexto(area, valorPorDefecto, false);
        }
    }

    export function Archivos(panel: HTMLDivElement) {
        let archivos: NodeListOf<HTMLInputElement> = panel.querySelectorAll(`input[${atControl.tipo}="${ltrTipoControl.Archivo}"]`) as NodeListOf<HTMLInputElement>;
        archivos.forEach((archivo) => {
            ApiDeArchivos.BlanquearArchivo(archivo, true);
        });

        archivos = panel.querySelectorAll(`input[${atControl.tipo}="${ltrTipoControl.SelectorDeUnArchivo}"]`) as NodeListOf<HTMLInputElement>;
        archivos.forEach((archivo) => {
            ApiDeArchivos.BlanquearArchivo(archivo, true);
        });
    }

    export function InicializarSpanDeArchivos(panel: HTMLDivElement): void {
        let archivosSeleccionados: NodeListOf<HTMLDivElement> = panel.querySelectorAll(`div[${atControl.class}='${ltrCss.archivosSeleccionado}']`) as NodeListOf<HTMLDivElement>;
        for (let i = 0; i < archivosSeleccionados.length; i++) {
            archivosSeleccionados[i].innerHTML = "";
        }

        let visorDeArchivos: NodeListOf<HTMLDivElement> = panel.querySelectorAll(`div[${atControl.class}='${ltrCss.contenedorDeArchivos}']`) as NodeListOf<HTMLDivElement>;
        for (let i = 0; i < visorDeArchivos.length; i++) {
            visorDeArchivos[i].innerHTML = "";
        }
    }

    export function InicializarSelectoresDeFecha(panel: HTMLDivElement): void {
        let selectoresDeFecha: NodeListOf<HTMLInputElement> = panel.querySelectorAll(`input[${atControl.tipo}="${ltrTipoControl.SelectorDeFecha}"]`) as NodeListOf<HTMLInputElement>;
        for (let i = 0; i < selectoresDeFecha.length; i++) {
            InicializarFecha(selectoresDeFecha[i]);
        }
        for (let i = 0; i < selectoresDeFecha.length; i++) {
            var hayOnBlur = selectoresDeFecha[i].getAttribute('onblur');
            if (Definido(hayOnBlur) && hayOnBlur.includes(atSelectorDeFecha.ProponerFechaEn)) {
                var parametros = extraerParametrosDeProponerFechaEn(hayOnBlur);
                MapearAlControl.ProponerFechaEn(selectoresDeFecha[i], parametros.propiedad, parametros.dias);

                //selectoresDeFecha[i].blur();
            }
        }
        selectoresDeFecha = panel.querySelectorAll(`input[${atControl.tipo}="${ltrTipoControl.SelectorDeFechaHora}"]`) as NodeListOf<HTMLInputElement>;
        for (let i = 0; i < selectoresDeFecha.length; i++) {
            InicializarFecha(selectoresDeFecha[i]);
        }
    }

    function extraerParametrosDeProponerFechaEn(onBlur: string): { propiedad: string; dias: number } {
        const regex = /MapearAlControl\.ProponerFechaEn\(this,\s*'([^']+)',\s*(\d+)\)/;
        const match = onBlur.match(regex);

        if (match) {
            const propiedad = match[1];     // 'PlfDeFin'
            const dias = parseInt(match[2], 10); // Convierte el string a número
            return { propiedad, dias };
        } else {
            throw new Error("La cadena no coincide con el formato esperado.");
        }
    }

    export function InicializarFecha(fecha: HTMLInputElement): void {
        ApiControl.BlanquearFecha(fecha);
    }

    export function InicializarEntreFechas(panel: HTMLDivElement): void {
        let entreFecha: NodeListOf<HTMLInputElement> = panel.querySelectorAll(`input[${atControl.tipo}="${ltrTipoControl.FiltroEntreFechas}"]`) as NodeListOf<HTMLInputElement>;
        for (let i = 0; i < entreFecha.length; i++) {
            InicializarEntreFecha(entreFecha[i]);
        }
    }

    export function InicializarEntreFecha(fechaDesde: HTMLInputElement): void {
        ApiControl.BlanquearFecha(fechaDesde, true);
        let idFechaHasta: string = fechaDesde.getAttribute(atEntreFechas.idfechahasta)
        let idHoraHasta: string = fechaDesde.getAttribute(atEntreFechas.idhorahasta)
        if (!IsNullOrEmpty(idFechaHasta)) {
            let fechaHasta: HTMLInputElement = document.getElementById(idFechaHasta) as HTMLInputElement;
            fechaHasta.value = "";
        }
        if (!IsNullOrEmpty(idHoraHasta)) {
            let HoraHasta: HTMLInputElement = document.getElementById(idHoraHasta) as HTMLInputElement;
            HoraHasta.value = '';
        }

    }

    export function OcultarArchivos(panel: HTMLDivElement, ocultar: boolean) {
        let archivos: NodeListOf<HTMLInputElement> = panel.querySelectorAll(`input[${atControl.tipo}="${ltrTipoControl.Archivo}"]`) as NodeListOf<HTMLInputElement>;
        archivos.forEach((archivo) => {
            ApiDeArchivos.OcultarArchivo(archivo, ocultar);
            ModoAcceso.AplicarAlSelectorDeArchivos(archivo, ocultar ? ModoAcceso.enumModoDeAccesoDeDatos.Consultor : ModoAcceso.enumModoDeAccesoDeDatos.Gestor);
        });
        archivos = panel.querySelectorAll(`input[${atControl.tipo}="${ltrTipoControl.SelectorDeUnArchivo}"]`) as NodeListOf<HTMLInputElement>;
        archivos.forEach((archivo) => {
            ModoAcceso.AplicarAlSelectorDeArchivos(archivo, ocultar ? ModoAcceso.enumModoDeAccesoDeDatos.Consultor : ModoAcceso.enumModoDeAccesoDeDatos.Gestor);
        });
        let imagenes: NodeListOf<HTMLInputElement> = panel.querySelectorAll(`input[${atControl.tipo}="${ltrTipoControl.UrlDeArchivo}"]`) as NodeListOf<HTMLInputElement>;
        imagenes.forEach((imagen) => {
            ApiDeArchivos.OcultarArchivo(imagen, ocultar);
            ModoAcceso.AplicarAlSelectorDeArchivos(imagen, ocultar ? ModoAcceso.enumModoDeAccesoDeDatos.Consultor : ModoAcceso.enumModoDeAccesoDeDatos.Gestor);
        });
    }

    export function ListasDeValores(panel: HTMLDivElement, lanzarEventoDeCambio: boolean = true): void {
        let listas: NodeListOf<HTMLSelectElement> = panel.querySelectorAll(`select[${atControl.tipo}='${ltrTipoControl.ListaDeValores}']`) as NodeListOf<HTMLSelectElement>;
        for (var i = 0; i < listas.length; i++) {
            let lista: HTMLSelectElement = listas[i];
            ApiControl.SeleccionarElPrimeroYAplicarEditabilidad(lista);
            if (lanzarEventoDeCambio) {
                let accion = lista.getAttribute(atListasDeValores.OnChange);
                if (Definido(accion))
                    Evaluar('ApiDeInicializacion.ListasDeValores', accion, accion.includes('this') ? lista : undefined);
            }
        }
    }

    export function InicializarListasDinamicas(panel: HTMLDivElement): void {
        let listas: NodeListOf<HTMLInputElement> = panel.querySelectorAll(`input[${atControl.tipo}="${ltrTipoControl.ListaDinamica}"]`) as NodeListOf<HTMLInputElement>;
        for (let i = 0; i < listas.length; i++) {
            InicializarListaDinamica(listas[i]);
        }
    }

    export function InicializarListaDinamica(lista: HTMLInputElement): void {
        if (lista.disabled || lista.readOnly && Numero(lista.getAttribute(atListasDinamicas.idSeleccionado)) > 0)
            return;

        let editable = lista.getAttribute(atControl.editable);
        if (NoDefinido(editable) || EsTrue(editable)) {
            ApiListaDinamica.Blanquear(lista);
        }
        if (!EsTrue(editable))
            MapearAlControl.DefinirNavegador(lista, 0);
    }

    export function InicializarCanvas(panel: HTMLDivElement): void {
        let canvas: NodeListOf<HTMLInputElement> = panel.querySelectorAll(`input[${atControl.tipo}="${ltrTipoControl.UrlDeArchivo}"]`) as NodeListOf<HTMLInputElement>;
        for (let i = 0; i < canvas.length; i++) {
            ApiDeArchivos.BlanquearArchivo(canvas[i], true);
        }
    }

    export function InicializarListasDeElementos(panel: HTMLDivElement, resetearLista: boolean = false): void {
        let listas: NodeListOf<HTMLSelectElement> = panel.querySelectorAll(`select[${atControl.tipo}="${ltrTipoControl.ListaDeElementos}"]`) as NodeListOf<HTMLSelectElement>;
        for (let i = 0; i < listas.length; i++) {
            let cargarBajoDemanda: boolean = EsTrue(listas[i].getAttribute(atListasDeElemento.cargarBajoDemanda));
            if (resetearLista || cargarBajoDemanda)
                ApiControl.QuitarOpcionesDeLalista(listas[i]);
            if (cargarBajoDemanda)
                continue;
            MapearAlControl.ListaDeElementos(listas[i], new Array<ClausulaDeFiltrado>(), 0);
        }
    }

    export function BlanquearListasDeElementos(panel: HTMLDivElement): void {
        let listas: NodeListOf<HTMLSelectElement> = panel.querySelectorAll(`select[${atControl.tipo}="${ltrTipoControl.ListaDeElementos}"]`) as NodeListOf<HTMLSelectElement>;
        for (let i = 0; i < listas.length; i++)
            ApiControl.BlanquearListaDeElementos(listas[i]);
    }

    export function RecargarListaDeElementos(lista: HTMLSelectElement, filtros: Array<ClausulaDeFiltrado>, id: number) {
        if (EsTrue(lista.getAttribute(atListasDeElemento.Cargando)) && id === 0)
            return;
        ApiControl.QuitarOpcionesDeLalista(lista);
        MapearAlControl.ListaDeElementos(lista, filtros, id);
    }

    export function AsignarAtributo(panel: HTMLDivElement, propiedad: string, atributo: string, valor: string): HTMLInputElement {
        let padre: HTMLInputElement = ApiControl.BuscarControl(panel, propiedad, true) as HTMLInputElement;
        padre.setAttribute(atributo, valor);
        return padre;
    }

    export function InicializarLinkDeDireccion(idPanel: string) {
        let panel: HTMLInputElement = document.getElementById(idPanel) as HTMLInputElement;
        if (NoDefinido(panel))
            return;

        var link = `https://www.google.com/maps/place/`;
        var url = ApiControl.BuscarControl(panel, "url", false) as HTMLInputElement;
        url.value = link;
        MapearAlControl.DefinirLink(url);
    }

    //export function ComponerDireccion(idGridDeDetalle: string, numeroFila: number): string {
    //    let idDeLaFila = `${idGridDeDetalle.replace('-contenedor', '-tabla_d_tr')}_${numeroFila}`;
    //    var link = '';
    //    try {
    //        var calle = ApiDeExpansor.ObtenerElValorDeLaPropiedadDeLaFila(idDeLaFila, ltrPropiedades.Callejero.Direccion.Url);
    //        link = `https://www.google.com/maps/place/${calle}`;
    //    }
    //    catch (error) {
    //        MensajesSe.EmitirExcepcion("Componer dirección", `No se ha podido obtener componer la dirección de la fila ${numeroFila.toString()} del grid ${idGridDeDetalle}`, error);
    //    }
    //    return link;
    //}

    export function ComponerDireccion(idGridDeDetalle: string, numeroFila: number): string {
        let idDeLaFila = `${idGridDeDetalle.replace('-contenedor', '-tabla_d_tr')}_${numeroFila}`;
        var link = '';
        try {
            var calle = ApiDeExpansor.ObtenerElValorDeLaPropiedadDeLaFila(idDeLaFila, ltrPropiedades.Callejero.Direccion.Url);

            // --- CORRECCIÓN AQUÍ ---
            // 1. Codifica la dirección para que los espacios, comas, etc., se conviertan en caracteres válidos para una URL.
            const direccionCodificada = encodeURIComponent(calle);

            // 2. Usa la URL de búsqueda correcta de Google Maps.
            link = `https://www.google.com/maps/search/?api=1&query=${direccionCodificada}`;
            // --- FIN DE LA CORRECCIÓN ---

        }
        catch (error) {
            MensajesSe.EmitirExcepcion("Componer dirección", `No se ha podido componer la dirección de la fila ${numeroFila.toString()} del grid ${idGridDeDetalle}`, error);
        }
        return link;
    }

}

