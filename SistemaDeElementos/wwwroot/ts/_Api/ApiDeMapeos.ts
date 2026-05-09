namespace MapearAlJson {

    export function Id(panel: HTMLDivElement): JSON {
        let input: HTMLInputElement = ApiControl.BuscarEditor(panel, literal.id);
        if (Number(input.value) <= 0)
            throw new Error(`El valor del id ${Number(input.value)} debe ser mayor a 0`);
        return JSON.parse(`{"${literal.id}":"${Number(input.value)}"}`);
    }

    export function ListaDinamicas(panel: HTMLDivElement, elementoJson: JSON): void {
        let lista: NodeListOf<HTMLInputElement> = panel.querySelectorAll(`input[${atControl.tipo}="${ltrTipoControl.ListaDinamica}"]`) as NodeListOf<HTMLInputElement>;
        for (let i = 0; i < lista.length; i++) {
            //let noMapeables = panel.getAttribute(atControl.NoMapeablesAlDto);
            //if (noMapeables.indexOf(lista[i].getAttribute(atControl.propiedad)) >= 0)
            //    continue;
            ListaDinamica(lista[i], elementoJson);
        }
    }

    function ListaDinamica(input: HTMLInputElement, elementoJson: JSON): void {
        let propiedadDto = input.getAttribute(atControl.propiedad);
        let guardarEn: string = input.getAttribute(atListasDinamicasDto.guardarEn);
        let obligatorio: boolean = EsTrue(input.getAttribute(atControl.obligatorio));
        let valor: number = Numero(input.getAttribute(atListasDinamicas.idSeleccionado));
        let blanquearAlSalir: boolean = EsTrue(input.getAttribute(atListasDinamicas.BlanquearAlSalir));
        let tenerEnCuentaLoEscrito = !IsNullOrEmpty(input.value) && !blanquearAlSalir;

        if (!tenerEnCuentaLoEscrito && obligatorio && valor === 0) {
            input.classList.remove(ltrCss.crtlValido);
            input.classList.add(ltrCss.crtlNoValido);
            throw new Error(`Debe seleccionar un elemento de la lista ${propiedadDto}`);
        }

        input.classList.remove(ltrCss.crtlNoValido);
        input.classList.add(ltrCss.crtlValido);
        //Si el id seleccionado es un 0 antes ponia '' , pero eso falla en el deserialice JSON, caso PuestosDeTrabajo, en el Idpuesto a copiar
        //elementoJson[guardarEn] = valor === 0 ? "0" : valor.toString();
        if (valor > 0) elementoJson[guardarEn] = valor;

        //Si no es obligatorio, y se quiere que tras persistir el dto, el negocio cree este registro en la BD (por ejemplo CP, o Barrio, o Zona tras crear una calle)
        if ((obligatorio || tenerEnCuentaLoEscrito) && valor === 0)
            elementoJson[propiedadDto] = input.value;
    }

    export function ListasDeValores(panel: HTMLDivElement, elementoJson: JSON): void {
        let selectores: NodeListOf<HTMLSelectElement> = panel.querySelectorAll(`select[${atControl.tipo}="${ltrTipoControl.ListaDeValores}"]`) as NodeListOf<HTMLSelectElement>;
        for (let i = 0; i < selectores.length; i++) {
            ListaDeValores(selectores[i], elementoJson);
        }
    }

    function ListaDeValores(selector: HTMLSelectElement, elementoJson: JSON) {
        let guardarEn: string = selector.getAttribute(atListas.guardarEn);
        elementoJson[guardarEn] = ApiControl.ValorDe(selector);
    }

    export function ListasDeElementos(panel: HTMLDivElement, elementoJson: JSON): void {
        let selectores: NodeListOf<HTMLSelectElement> = panel.querySelectorAll(`select[${atControl.tipo}="${ltrTipoControl.ListaDeElementos}"]`) as NodeListOf<HTMLSelectElement>;
        for (let i = 0; i < selectores.length; i++) {
            ListaDeElemento(selectores[i], elementoJson);
        }
    }

    function ListaDeElemento(selector: HTMLSelectElement, elementoJson: JSON) {
        let guardarEn: string = selector.getAttribute(atListas.guardarEn);
        elementoJson[guardarEn] = ApiControl.ValorDe(selector);
    }

    export function Restrictores(panel: HTMLDivElement, elementoJson: JSON): void {
        let restrictores: NodeListOf<HTMLInputElement> = panel.querySelectorAll(`input[${atControl.tipo}="${ltrTipoControl.restrictorDeEdicion}"]`) as NodeListOf<HTMLInputElement>;
        for (let i = 0; i < restrictores.length; i++) {
            Restrictor(restrictores[i], elementoJson);
        }
    }

    function Restrictor(input: HTMLInputElement, elementoJson: JSON): void {
        let propiedadDto: string = Definido(input.getAttribute(atControl.propiedadRestrictora))
            ? input.getAttribute(atControl.propiedadRestrictora)
            : input.getAttribute(atControl.propiedad);

        let idRestrictor: number = Numero(input.getAttribute(atControl.restrictor)) as number;
        let obligatorio: boolean = EsTrue(input.getAttribute(atControl.obligatorio));

        if (obligatorio && idRestrictor === 0) {
            input.classList.remove(ltrCss.crtlValido);
            input.classList.add(ltrCss.crtlNoValido);
            throw new Error(`El campo ${propiedadDto} es obligatorio`);
        }

        input.classList.remove(ltrCss.crtlNoValido);
        input.classList.add(ltrCss.crtlValido);

        if (propiedadDto.startsWith(literal.id) && idRestrictor === 0)
            return;

        elementoJson[propiedadDto] = idRestrictor;
    }


    export function Fechas(panel: HTMLDivElement, elementoJson: JSON): void {
        let fechas: NodeListOf<HTMLInputElement> = panel.querySelectorAll(`input[${atControl.tipo}="${ltrTipoControl.SelectorDeFecha}"]`) as NodeListOf<HTMLInputElement>;
        for (var i = 0; i < fechas.length; i++) {
            let fecha: HTMLInputElement = fechas[i] as HTMLInputElement;
            Fecha(fecha, elementoJson);
        }

        let fechasHoras: NodeListOf<HTMLInputElement> = panel.querySelectorAll(`input[${atControl.tipo}="${ltrTipoControl.SelectorDeFechaHora}"]`) as NodeListOf<HTMLInputElement>;
        for (var i = 0; i < fechasHoras.length; i++) {
            let fecha: HTMLInputElement = fechasHoras[i] as HTMLInputElement;
            Fecha(fecha, elementoJson);
        }
    }

    function Fecha(controlDeFecha: HTMLInputElement, elementoJson: JSON): void {
        let propiedadDto: string = controlDeFecha.getAttribute(atControl.propiedad);
        var utcFecha = ObtenerFecha(controlDeFecha, propiedadDto);
        if (Definido(utcFecha)) elementoJson[propiedadDto] = utcFecha;
        else if (ApiControl.EsVisible(controlDeFecha)) elementoJson[propiedadDto] = '';
    }

    export function Textos(panel: HTMLDivElement, elementoJson: JSON): void {
        let areas: NodeListOf<HTMLTextAreaElement> = panel.querySelectorAll(`textarea[${atControl.tipo}="${ltrTipoControl.AreaDeTexto}"]`) as NodeListOf<HTMLTextAreaElement>;
        for (let i = 0; i < areas.length; i++) {
            Texto(areas[i], elementoJson);
        }
    }

    function Texto(area: HTMLTextAreaElement, elementoJson: JSON): void {
        let propiedadDto: string = area.getAttribute(atControl.propiedad);
        let obligatorio: boolean = EsTrue(area.getAttribute(atControl.obligatorio));
        let valor: string = area.value; //.replace(/\n/g, "\r\n");
        if (obligatorio && NoDefinido(valor)) {
            area.classList.remove(ltrCss.crtlValido);
            area.classList.add(ltrCss.crtlNoValido);
            throw new Error(`El campo ${propiedadDto} es obligatorio`);
        }
        elementoJson[propiedadDto] = valor;
    }

    export function Editores(panel: HTMLDivElement, elementoJson: JSON): void {
        let editores: NodeListOf<HTMLInputElement> = panel.querySelectorAll(`input[${atControl.tipo}="${ltrTipoControl.Editor}"]`) as NodeListOf<HTMLInputElement>;
        for (let i = 0; i < editores.length; i++) {
            if (EsTrue(editores[i].getAttribute(atControl.SoloParaTs))) continue;
            Editor(editores[i], elementoJson);
        }
    }

    function Editor(input: HTMLInputElement, elementoJson: JSON): void {
        var propiedadDto = input.getAttribute(atControl.propiedad);
        let valor: string = (input as HTMLInputElement).value;
        let obligatorio: boolean = EsTrue(input.getAttribute(atControl.obligatorio));

        if (obligatorio && NoDefinido(valor)) {
            input.classList.remove(ltrCss.crtlValido);
            input.classList.add(ltrCss.crtlNoValido);
            throw new Error(`El campo ${propiedadDto} es obligatorio`);
        }

        ValidarMascara(input);
        input.classList.remove(ltrCss.crtlNoValido);
        input.classList.add(ltrCss.crtlValido);
        elementoJson[propiedadDto] = ApiControl.Valor(input);
    }

    export function Archivos(panel: HTMLDivElement, elementoJson: JSON): void {
        let archivos: NodeListOf<HTMLInputElement> = panel.querySelectorAll(`input[${atControl.tipo}="${ltrTipoControl.Archivo}"]`) as NodeListOf<HTMLInputElement>;
        for (let i = 0; i < archivos.length; i++) {
            Archivo(archivos[i], elementoJson);
        }
        archivos = panel.querySelectorAll(`input[${atControl.tipo}="${ltrTipoControl.SelectorDeUnArchivo}"]`) as NodeListOf<HTMLInputElement>;
        for (let i = 0; i < archivos.length; i++) {
            Archivo(archivos[i], elementoJson);
        }
    }

    function Archivo(archivo: HTMLInputElement, elementoJson: JSON): void {
        var propiedadDto = archivo.getAttribute(atControl.propiedad);
        let valor: string = archivo.getAttribute(atArchivo.idArchivo);
        let obligatorio: boolean = EsTrue(archivo.getAttribute(atControl.obligatorio));

        if (obligatorio && IsNullOrEmpty(valor)) {
            archivo.classList.remove(ltrCss.crtlValido);
            archivo.classList.add(ltrCss.crtlNoValido);
            throw new Error(`El campo ${propiedadDto} es obligatorio`);
        }

        archivo.classList.remove(ltrCss.crtlNoValido);
        archivo.classList.add(ltrCss.crtlValido);
        elementoJson[propiedadDto] = valor;
    }

    export function Urls(panel: HTMLDivElement, elementoJson: JSON): void {
        let urlsDeArchivos: NodeListOf<HTMLInputElement> = panel.querySelectorAll(`input[${atControl.tipo}="${ltrTipoControl.UrlDeArchivo}"]`) as NodeListOf<HTMLInputElement>;
        for (let i = 0; i < urlsDeArchivos.length; i++) {
            Url(urlsDeArchivos[i], elementoJson);
        }
    }


    function Url(urlDeArchivo: HTMLInputElement, elementoJson: JSON): void {
        var propiedadDto = urlDeArchivo.getAttribute(atControl.propiedad);
        let valor: string = urlDeArchivo.getAttribute(atArchivo.nombre);
        let obligatorio: boolean = EsTrue(urlDeArchivo.getAttribute(atControl.obligatorio));

        if (obligatorio && IsNullOrEmpty(valor)) {
            urlDeArchivo.classList.remove(ltrCss.crtlValido);
            urlDeArchivo.classList.add(ltrCss.crtlNoValido);
            throw new Error(`El campo ${propiedadDto} es obligatorio`);
        }

        urlDeArchivo.classList.remove(ltrCss.crtlNoValido);
        urlDeArchivo.classList.add(ltrCss.crtlValido);
        elementoJson[propiedadDto] = valor;
    }

    export function Checks(panel: HTMLDivElement, elementoJson: JSON): void {
        let checkes: NodeListOf<HTMLInputElement> = panel.querySelectorAll(`input[${atControl.tipo}="${ltrTipoControl.Check}"]`) as NodeListOf<HTMLInputElement>;
        for (let i = 0; i < checkes.length; i++) {
            Check(checkes[i], elementoJson);
        }
    }

    function Check(check: HTMLInputElement, elementoJson: JSON): void {
        var propiedadDto = check.getAttribute(atControl.propiedad);
        elementoJson[propiedadDto] = check.checked;
    }

};

namespace MapearAlDiccionario {

    export function ListaDinamicas(panel: HTMLDivElement, diccionario: Diccionario<any>): void {
        let lista: NodeListOf<HTMLInputElement> = panel.querySelectorAll(`input[${atControl.tipo}="${ltrTipoControl.ListaDinamica}"]`) as NodeListOf<HTMLInputElement>;
        for (let i = 0; i < lista.length; i++) {
            ListaDinamica(lista[i], diccionario);
        }
    }

    function ListaDinamica(input: HTMLInputElement, diccionario: Diccionario<any>): void {
        let propiedadDto = input.getAttribute(atControl.propiedad);
        let valor: number = Numero(input.getAttribute(atListasDinamicas.idSeleccionado));
        let esDeFiltrado = EsTrue(input.getAttribute(atControl.filtro));
        if (esDeFiltrado) {
            if (valor > 0)
                diccionario.Agregar(propiedadDto, valor);
        }
        else {
            let guardarEn: string = input.getAttribute(atListasDinamicasDto.guardarEn);
            let obligatorio: string = input.getAttribute(atControl.obligatorio);

            if (obligatorio === "S" && Number(valor) === 0) {
                input.classList.remove(ltrCss.crtlValido);
                input.classList.add(ltrCss.crtlNoValido);
                throw new Error(`Debe seleccionar un elemento de la lista ${propiedadDto}`);
            }
            input.classList.remove(ltrCss.crtlNoValido);
            input.classList.add(ltrCss.crtlValido);
            diccionario.Agregar(guardarEn, valor === 0 ? '0' : valor.toString());
        }
    }

    export function Checks(panel: HTMLDivElement, diccionario: Diccionario<any>): void {
        let checkes: NodeListOf<HTMLInputElement> = panel.querySelectorAll(`input[${atControl.tipo}="${ltrTipoControl.Check}"]`) as NodeListOf<HTMLInputElement>;
        for (let i = 0; i < checkes.length; i++) {
            Check(checkes[i], diccionario);
        }
    }

    function Check(check: HTMLInputElement, diccionario: Diccionario<any>): void {
        var propiedadDto = check.getAttribute(atControl.propiedad);
        diccionario.Agregar(propiedadDto, check.checked);
    }


    export function ListasDeValores(panel: HTMLDivElement, diccionario: Diccionario<any>): void {
        let listas: NodeListOf<HTMLSelectElement> = panel.querySelectorAll(`select[${atControl.tipo}="${ltrTipoControl.ListaDeValores}"]`) as NodeListOf<HTMLSelectElement>;
        for (let i = 0; i < listas.length; i++) {
            ListaDeValores(listas[i], diccionario);
        }
    }

    function ListaDeValores(lista: HTMLSelectElement, diccionario: Diccionario<any>): void {
        var propiedadDto = lista.getAttribute(atControl.propiedad);
        diccionario.Agregar(propiedadDto, lista.value);
    }

}

namespace MapearALaListaDeParametros {

    export function ListaDinamicas(panel: HTMLDivElement, parametros: Array<Parametro>): void {
        let lista: NodeListOf<HTMLInputElement> = panel.querySelectorAll(`input[${atControl.tipo}="${ltrTipoControl.ListaDinamica}"]`) as NodeListOf<HTMLInputElement>;
        for (let i = 0; i < lista.length; i++) {
            ListaDinamica(lista[i], parametros);
        }
    }

    function ListaDinamica(input: HTMLInputElement, parametros: Array<Parametro>): void {
        let propiedadDto = input.getAttribute(atControl.propiedad);
        let valor: number = Numero(input.getAttribute(atListasDinamicas.idSeleccionado));
        let esDeFiltrado = EsTrue(input.getAttribute(atControl.filtro));
        if (esDeFiltrado) {
            if (valor > 0)
                parametros.push(new Parametro(propiedadDto, valor));
        }
        else {
            let guardarEn: string = input.getAttribute(atListasDinamicasDto.guardarEn);
            let obligatorio: string = input.getAttribute(atControl.obligatorio);

            if (obligatorio === "S" && Number(valor) === 0) {
                input.classList.remove(ltrCss.crtlValido);
                input.classList.add(ltrCss.crtlNoValido);
                throw new Error(`Debe seleccionar un elemento de la lista ${propiedadDto}`);
            }
            input.classList.remove(ltrCss.crtlNoValido);
            input.classList.add(ltrCss.crtlValido);
            parametros.push(new Parametro(guardarEn, valor === 0 ? null : valor));
        }
    }


    export function Fechas(panel: HTMLDivElement, parametros: Array<Parametro>): void {
        let fechas: NodeListOf<HTMLInputElement> = panel.querySelectorAll(`input[${atControl.tipo}="${ltrTipoControl.SelectorDeFecha}"]`) as NodeListOf<HTMLInputElement>;
        for (let i = 0; i < fechas.length; i++) {
            Fecha(fechas[i], parametros);
        }
        let fechasHoras: NodeListOf<HTMLInputElement> = panel.querySelectorAll(`input[${atControl.tipo}="${ltrTipoControl.SelectorDeFechaHora}"]`) as NodeListOf<HTMLInputElement>;
        for (let i = 0; i < fechasHoras.length; i++) {
            Fecha(fechasHoras[i], parametros);
        }
    }

    function Fecha(controlDeFecha: HTMLInputElement, parametros: Array<Parametro>): void {
        let propiedadDto: string = controlDeFecha.getAttribute(atControl.propiedad);
        var utcFecha = ObtenerFecha(controlDeFecha, propiedadDto);
        if (Definido(utcFecha)) parametros.push(new Parametro(propiedadDto, utcFecha));
    }

    export function Restrictores(panel: HTMLDivElement, parametros: Array<Parametro>): void {
        let editores: NodeListOf<HTMLInputElement> = panel.querySelectorAll(`input[${atControl.tipo}="${ltrTipoControl.restrictorDeEdicion}"]`) as NodeListOf<HTMLInputElement>;
        for (let i = 0; i < editores.length; i++) {
            Restrictor(editores[i], parametros);
        }
    }

    function Restrictor(input: HTMLInputElement, parametros: Array<Parametro>): void {
        var propiedadDto = input.getAttribute(atControl.propiedad);
        let valor: number = Numero(input.getAttribute(atControl.restrictor));
        parametros.push(new Parametro(propiedadDto, valor));
    }

    export function Editores(panel: HTMLDivElement, parametros: Array<Parametro>): void {
        let editores: NodeListOf<HTMLInputElement> = panel.querySelectorAll(`input[${atControl.tipo}="${ltrTipoControl.Editor}"]`) as NodeListOf<HTMLInputElement>;
        for (let i = 0; i < editores.length; i++) {
            Editor(editores[i], parametros);
        }
    }

    function Editor(input: HTMLInputElement, parametros: Array<Parametro>): void {
        var propiedadDto = input.getAttribute(atControl.propiedad);
        let valor: string = (input as HTMLInputElement).value;
        let obligatorio: string = input.getAttribute(atControl.obligatorio);

        if (EsTrue(obligatorio) && NoDefinido(valor)) {
            input.classList.remove(ltrCss.crtlValido);
            input.classList.add(ltrCss.crtlNoValido);
            throw new Error(`El campo ${propiedadDto} es obligatorio`);
        }
        ValidarMascara(input);
        input.classList.remove(ltrCss.crtlNoValido);
        input.classList.add(ltrCss.crtlValido);
        parametros.push(new Parametro(propiedadDto, ApiControl.Valor(input)));
    }


    export function Textos(panel: HTMLDivElement, parametros: Array<Parametro>): void {
        let areas: NodeListOf<HTMLTextAreaElement> = panel.querySelectorAll(`textarea[${atControl.tipo}="${ltrTipoControl.AreaDeTexto}"]`) as NodeListOf<HTMLTextAreaElement>;
        for (let i = 0; i < areas.length; i++) {
            Texto(areas[i], parametros);
        }
    }

    function Texto(area: HTMLTextAreaElement, parametros: Array<Parametro>): void {
        let propiedadDto: string = area.getAttribute(atControl.propiedad);
        let obligatorio: string = area.getAttribute(atControl.obligatorio);
        let valor: string = area.value; //.replace(/\n/g, "\r\n");
        if (obligatorio === "S" && NoDefinido(valor)) {
            area.classList.remove(ltrCss.crtlValido);
            area.classList.add(ltrCss.crtlNoValido);
            throw new Error(`El campo ${propiedadDto} es obligatorio`);
        }
        parametros.push(new Parametro(propiedadDto, valor));
    }

    export function ListasDeElementos(panel: HTMLDivElement, parametros: Array<Parametro>): void {
        let listas: NodeListOf<HTMLSelectElement> = panel.querySelectorAll(`select[${atControl.tipo}="${ltrTipoControl.ListaDeElementos}"]`) as NodeListOf<HTMLSelectElement>;
        for (let i = 0; i < listas.length; i++) {
            ListaDeElemento(listas[i], parametros);
        }
    }

    function ListaDeElemento(selector: HTMLSelectElement, parametros: Array<Parametro>): void {
        let guardarEn: string = selector.getAttribute(atListas.guardarEn);
        let valor: string = ApiControl.ValorDe(selector);
        parametros.push(new Parametro(guardarEn, EsNumeroNoNulo(valor) ? Number(valor) : valor));
    }

    export function ListasDeValores(panel: HTMLDivElement, parametros: Array<Parametro>): void {
        let listas: NodeListOf<HTMLSelectElement> = panel.querySelectorAll(`select[${atControl.tipo}="${ltrTipoControl.ListaDeValores}"]`) as NodeListOf<HTMLSelectElement>;
        for (let i = 0; i < listas.length; i++) {
            ListaDeValores(listas[i], parametros);
        }
    }

    function ListaDeValores(selector: HTMLSelectElement, parametros: Array<Parametro>): void {
        let guardarEn: string = selector.getAttribute(atListas.guardarEn);
        parametros.push(new Parametro(guardarEn, ApiControl.ValorDe(selector)));
    }

    export function Checks(panel: HTMLDivElement, parametros: Array<Parametro>): void {
        let checks: NodeListOf<HTMLInputElement> = panel.querySelectorAll(`input[${atControl.tipo}="${ltrTipoControl.Check}"]`) as NodeListOf<HTMLInputElement>;
        for (let i = 0; i < checks.length; i++) {
            Check(checks[i], parametros);
        }
    }

    function Check(check: HTMLInputElement, parametros: Array<Parametro>): void {
        let guardarEn: string = check.getAttribute(atControl.propiedad);
        parametros.push(new Parametro(guardarEn, check.checked));
    }


    export function Archivos(panel: HTMLDivElement, parametros: Array<Parametro>): void {
        let archivos: NodeListOf<HTMLInputElement> = panel.querySelectorAll(`input[${atControl.tipo}="${ltrTipoControl.Archivo}"]`) as NodeListOf<HTMLInputElement>;
        for (let i = 0; i < archivos.length; i++) {
            Archivo(archivos[i], parametros);
        }
        archivos = panel.querySelectorAll(`input[${atControl.tipo}="${ltrTipoControl.SelectorDeUnArchivo}"]`) as NodeListOf<HTMLInputElement>;
        for (let i = 0; i < archivos.length; i++) {
            Archivo(archivos[i], parametros);
        }
    }

    function Archivo(archivo: HTMLInputElement, parametros: Array<Parametro>): void {
        var propiedadDto = archivo.getAttribute(atControl.propiedad);
        let valor: number = Numero(archivo.getAttribute(atArchivo.idArchivo));
        let obligatorio: boolean = EsTrue(archivo.getAttribute(atControl.obligatorio));

        if (obligatorio && valor == 0) {
            archivo.classList.remove(ltrCss.crtlValido);
            archivo.classList.add(ltrCss.crtlNoValido);
            throw new Error(`El campo ${propiedadDto} es obligatorio`);
        }

        archivo.classList.remove(ltrCss.crtlNoValido);
        archivo.classList.add(ltrCss.crtlValido);
        parametros.push(new Parametro(propiedadDto, valor));
    }


}

namespace MapearAlFiltro {

    export function MapearRestrictor(panel: HTMLDivElement, propiedad: string, id: number, texto: string, indicarNoEncontrado: boolean): HTMLElement {
        let control = ApiControl.BuscarControl(panel, propiedad, false);
        if (NoDefinido(control)) {
            if (indicarNoEncontrado)
                MensajesSe.Info(`la propiedad: ${propiedad} no se ha localizado en el panel ${panel.id}`);
            return undefined;
        }

        return MapearAlControl.MapearValores(control, id, texto, true, false) as HTMLElement;
    }

    export function MapearFiltros(div: HTMLDivElement, propiedad: string, id: number, texto: string) {
        let restrictores: NodeListOf<HTMLInputElement> = div.querySelectorAll(`input[${atControl.tipo}="${ltrTipoControl.restrictorDeFiltro}"]`) as NodeListOf<HTMLInputElement>;
        for (let i = 0; i < restrictores.length; i++)
            if (restrictores[i].getAttribute(atControl.propiedad) === propiedad) return;
        MapearAlControl.Propiedad(div, propiedad, id, texto, false, false, false);
    }

    export function PlantillasDeFiltrado(panel: HTMLDivElement, filtros: Array<ClausulaDeFiltrado>) {
        for (let i: number = 0; i < filtros.length; i++) {
            let filtro: ClausulaDeFiltrado = filtros[i];
        }
    }

}

namespace MapearPanelDeCreacion {

    export function MapearRestrictores(zonaDeCreacion: HTMLDivElement, propiedad: string, id: number, texto: string) {
        let mapeado: boolean = RestrictoresDeCreacion(zonaDeCreacion, propiedad, id, texto);
        if (!mapeado) {
            let lista: HTMLInputElement = ApiControl.BuscarListaDinamicaPorGuardarEn(zonaDeCreacion, propiedad);
            if (Definido(lista)) {
                MapearAlControl.FijarValorEnListaDinamica(lista, id, texto);
            }
            else
                MapearAlControl.Propiedad(zonaDeCreacion, propiedad, id, texto, true, false);
        }
    }

    function RestrictoresDeCreacion(panel: HTMLDivElement, propiedad: string, id: number, texto: string): boolean {
        let restrictores: NodeListOf<HTMLInputElement> = panel.querySelectorAll(`input[${atControl.tipo}="${ltrTipoControl.restrictorDeEdicion}"]`) as NodeListOf<HTMLInputElement>;

        for (let i = 0; i < restrictores.length; i++) {
            if (restrictores[i].getAttribute(atControl.propiedad) === propiedad) {
                MapearAlControl.Restrictor(restrictores[i], id, texto);
                return true;
            }
        }
        return false;
    }

}

namespace MapearPanelDeEdicion {

    export function MapearRestrictores(zonaDeEdicion: HTMLDivElement, propiedad: string, id: number, texto: string) {
        let mapeado: boolean = RestrictoresDeEdicion(zonaDeEdicion, propiedad, id, texto);
        if (!mapeado) {
            let lista: HTMLInputElement = ApiControl.BuscarListaDinamicaPorGuardarEn(zonaDeEdicion, propiedad);
            if (Definido(lista)) {
                MapearAlControl.FijarValorEnListaDinamica(lista, id, texto);
            }
            else
                MapearAlControl.Propiedad(zonaDeEdicion, propiedad, id, texto, true, false);
        }
    }

    function RestrictoresDeEdicion(panel: HTMLDivElement, propiedad: string, id: number, texto: string): boolean {
        let restrictores: NodeListOf<HTMLInputElement> = panel.querySelectorAll(`input[${atControl.tipo}="${ltrTipoControl.restrictorDeEdicion}"]`) as NodeListOf<HTMLInputElement>;

        for (let i = 0; i < restrictores.length; i++) {
            if (restrictores[i].getAttribute(atControl.propiedad) === propiedad) {
                MapearAlControl.Restrictor(restrictores[i], id, texto);
                return true;
            }
        }
        return false;
    }
}

namespace MapearAlGrid {

    export function MapearLosGridDeDetalle(panel: HTMLDivElement, idNegocio: number, datos: any, guid: string) {

        let grids: NodeListOf<HTMLInputElement> = panel.querySelectorAll(`div[${atControl.tipo}="${ltrTipoControl.gridDeDetalle}"]`) as NodeListOf<HTMLInputElement>;

        for (var i = 0; i < grids.length; i++) {
            let grid: HTMLDivElement = grids[i] as HTMLDivElement;
            if (EsTrue(grid.getAttribute(atGridDeDetalle.CargarPorEvento)))
                continue;
            let contenedorGrid = ApiControl.BuscarDivConClase(grid, ltrCss.Bloque.Contenedor)
            if (Definido(contenedorGrid)) {
                if (!ApiControl.EsVisible(contenedorGrid))
                    continue;
            }
            else {
                contenedorGrid = ApiControl.BuscarDivConClase(grid, ltrCss.Detalle.Contenedor)
                if (!Definido(contenedorGrid))
                    MensajesSe.Advertencia(`No se ha podido localizar el contenedor del grid de detalle '${grid.id}''`);
                if (!ApiControl.EsVisible(contenedorGrid))
                    continue;
            }
            //let espan: HTMLDivElement = grid.parentElement.parentElement.parentElement as HTMLDivElement;
            //if (espan.classList.contains(ltrCss.Espan.noVisible))
            //    continue;

            const campoRestrictor: string = grid.getAttribute(atGridDeDetalle.campoRestrictor);
            const idElemento = ObtenerCampoRestrictor(datos, campoRestrictor, literal.id);
            MapearGridDeDetalle(grid, idNegocio, idElemento, guid);

            let referencias: NodeListOf<HTMLAnchorElement> = contenedorGrid.querySelectorAll(`[tipo='${ltrTipoControl.Referencia}']`) as NodeListOf<HTMLAnchorElement>;
            for (let i: number = 0; i < referencias.length; i++) {
                MapearAlControl.MapearReferencia(referencias[i], datos, ModoAcceso.enumModoDeAccesoDeDatos.Consultor);
            }
        }
    }

    export function MapearGridDeDetalle(grid: HTMLDivElement, idNegocio: number, idDelElemento: number, guid: any) {
        let controlador: string = grid.getAttribute(atGridDeDetalle.controlador);
        let accion: string = grid.getAttribute(atGridDeDetalle.accionDeConsulta);
        let conCapa: boolean = true; // EsTrue(grid.getAttribute(atGridDeDetalle.conCapa))
        let idVinculado: number = Numero(grid.getAttribute(atGridDeDetalle.IdNegocioVinculado));

        let estaElModuloCargado = typeof Crud !== 'undefined';
        if (estaElModuloCargado && Definido(Crud.crudMnt)) {
            conCapa = Crud.crudMnt.CapaConVinculados;
        }
        EliminarRegistrosDeLaTabla(grid);
        ApiPanel.MostrarBarrarCargando(PanelDeBarra(grid));
        let idNegocioDelGrid: number = Numero(grid.getAttribute(atGridDeDetalle.idNegocio));
        if (idNegocioDelGrid !== idNegocio)
            idNegocio = idNegocioDelGrid;

        if (accion === Ajax.EndPoint.LeerVinculosCon) {
            GridDeDetalle_LeerVinculos(grid, controlador, idNegocio, idVinculado, idDelElemento, conCapa);
        }
        else {
            GridDeDetalle_LeerElementos(grid, controlador, accion, idNegocio, idDelElemento, guid);
        }
    }

    function GridDeDetalle_LeerVinculos(grid: HTMLDivElement, controlador: string, idNegocio: number, idVinculado: number, idDelElemento: number, conCapa: boolean) {

        let datosDeEntrada: Array<Parametro> = new Array<Parametro>();
        datosDeEntrada.push(new Parametro(ltrTipoControl.gridDeDetalle, grid.id));
        datosDeEntrada.push(new Parametro(literal.controlador, controlador));
        datosDeEntrada.push(new Parametro(literal.idNegocio, idNegocio));
        datosDeEntrada.push(new Parametro(atControl.idVinculado, idVinculado));
        datosDeEntrada.push(new Parametro(atControl.idElemento, idDelElemento));
        datosDeEntrada.push(new Parametro(Ajax.Param.posicion, 0));
        datosDeEntrada.push(new Parametro(atGridDeDetalle.conCapa, false));
        let parametros: Array<Parametro> = new Array<Parametro>();
        parametros.push(new Parametro(Ajax.Param.posicion, conCapa ? 0 : -1));
        ApiDePeticiones.LeerVinculos(this, controlador, idNegocio, idVinculado, idDelElemento, parametros, datosDeEntrada, false).
            then(async (peticion) => await DespuesDeLeerElGridDeDetalle(peticion)).
            catch((peticion) => {
                ApiPanel.QuitarBarraDeCarga(grid);
                ApiDePeticiones.EmitirError(peticion)
            });
    }

    function GridDeDetalle_LeerMasVinculos(grid: HTMLDivElement, datosDeEntrada: Array<Parametro> = new Array<Parametro>(), posicion: number) {
        posicion = posicion + 10;
        let controlador = datosDeEntrada.find((p) => p.parametro === literal.controlador).valor;
        const idNegocio = datosDeEntrada.find((p) => p.parametro === literal.idNegocio).valor;
        const idVinculado = datosDeEntrada.find((p) => p.parametro === atControl.idVinculado).valor;
        const idelemento = datosDeEntrada.find((p) => p.parametro === atControl.idElemento).valor;

        const objPosicion = datosDeEntrada.find((p) => p.parametro === Ajax.Param.posicion);
        objPosicion.valor = posicion;

        let parametros: Array<Parametro> = new Array<Parametro>();
        parametros.push(new Parametro(Ajax.Param.posicion, posicion));
        ApiDePeticiones.LeerVinculos(this, controlador, idNegocio, idVinculado, idelemento, parametros, datosDeEntrada, false).
            then(async (peticion) => await DespuesDeLeerElGridDeDetalle(peticion)).
            catch((peticion) => {
                ApiPanel.QuitarBarraDeCarga(grid);
                ApiDePeticiones.EmitirError(peticion)
            });
    }

    function GridDeDetalle_LeerElementos(grid: HTMLDivElement, controlador: string, accion: string, idNegocio: number, idDelElemento: number, guid: any) {
        let restrictor: string = grid.getAttribute(atGridDeDetalle.restrictor);
        let restrictorFijo: string = grid.getAttribute(atGridDeDetalle.restrictorFijo);
        let filtros: Array<ClausulaDeFiltrado> = new Array<ClausulaDeFiltrado>();
        let filtro = new ClausulaDeFiltrado(restrictor, atCriterio.igual, idDelElemento.toString());
        filtros.push(filtro);
        if (restrictor.toLowerCase() !== Ajax.Param.idNegocio.toLowerCase()) {
            filtro = new ClausulaDeFiltrado(Ajax.Param.idNegocio, atCriterio.igual, idNegocio.toString());
            filtros.push(filtro);
        }

        if (Definido(restrictorFijo)) {
            const partes = restrictorFijo.split(ltrSimbolos.igual);
            const filtro2 = new ClausulaDeFiltrado(partes[0], atCriterio.igual, partes[1]);
            filtros.push(filtro2);
        }

        let parametros: Array<Parametro> = new Array<Parametro>();
        let aplicarJoin = EsTrue(grid.getAttribute(atGridDeDetalle.AplicarJoin));
        let cantidad: string = grid.getAttribute(atGridDeDetalle.cantidad);
        let ordenarPor: string = grid.getAttribute(atGridDeDetalle.orden);
        let filtrarPara: string = grid.getAttribute(atGridDeDetalle.filtrarPara);
        parametros.push(new Parametro(Ajax.Param.aplicarJoin, aplicarJoin));
        parametros.push(new Parametro(Ajax.Param.cantidad, cantidad));
        parametros.push(new Parametro(Ajax.Param.obtenerSeguridad, false));
        parametros.push(new Parametro(Ajax.Param.ordenarPor, ordenarPor));
        parametros.push(new Parametro(Ajax.Param.fitrarPara, filtrarPara));
        parametros.push(new Parametro(Ajax.Param.guid, guid));

        let datosDeEntrada: Array<Parametro> = new Array<Parametro>();
        datosDeEntrada.push(new Parametro(ltrTipoControl.gridDeDetalle, grid.id));

        ApiDePeticiones.LeerElementos(this, controlador, accion, filtros, parametros, datosDeEntrada, false).
            then((peticion) => DespuesDeLeerElGridDeDetalle(peticion)).
            catch((peticion) => {
                ApiPanel.QuitarBarraDeCarga(grid);
                ApiDePeticiones.EmitirError(peticion)
            });
    }

    function DespuesDeLeerElGridDeDetalle(peticion: ApiDeAjax.DescriptorAjax) {
        let idGrid: string = peticion.DatosDeEntrada[0].valor;
        let grid: HTMLDivElement = document.getElementById(idGrid) as HTMLDivElement;
        let espan: HTMLDivElement = grid.parentElement.parentElement.parentElement as HTMLDivElement;

        if (EsTrue(espan.getAttribute(atControl.esDetalle)) && NoDefinido(peticion.resultado.datos) &&
            ModoAcceso.Parsear(peticion.resultado.modoDeAcceso) === ModoAcceso.enumModoDeAccesoDeDatos.SinPermiso) {
            ApiDeGrid.Expansor_OcultarSpan(grid);
            ApiPanel.QuitarBarraDeCarga(grid);
            console.log(peticion.resultado.mensaje);
            return;
        }

        const datosDeEntrada = peticion.DatosDeEntrada as Array<Parametro>;
        let posicion = datosDeEntrada.find((p) => p.parametro === Ajax.Param.posicion)?.valor ?? 0;
        if (posicion > 0 && peticion.resultado.datos.length === 0) {
            ApiPanel.QuitarBarraDeCarga(grid);
            return;
        }
        if (!Definido(peticion.resultado.datos) || (peticion.resultado.datos.length === 0 && EsTrue(grid.getAttribute(atGridDeDetalle.OcultarSiVacio)))) {
            ApiDeGrid.Expansor_OcultarSpan(grid);
            ApiPanel.QuitarBarraDeCarga(grid);
            return;
        }


        ApiDeGrid.Expansor_MostrarSpan(grid);
        let referencia = espan.querySelector(`a[${atControl.class}*=${ltrCss.Espan.cssExpansor}]`) as HTMLAnchorElement;
        let incluir = false;
        let titulo = "";
        if (Definido(referencia)) {
            titulo = referencia.getAttribute(ltrEspanes.Atributos.titulo);
            const partes = referencia.innerText.split(':');
            const mapeados = partes.length === 2 ? Numero(partes[1].trim()) : 0
            incluir = false;

            if (peticion.resultado.datos?.length > 0) {
                incluir = mapeados > 0;
                titulo = `${titulo}: ${peticion.resultado.datos.length + mapeados}`;
            }
            else {
                if (mapeados === 0) {
                    referencia.innerText = `${titulo}`;
                }
            }
        }

        if (Definido(peticion.resultado.datos) && incluir === false) {
            MapearDatosAlGridDeDetalle(grid, peticion.resultado.datos);
            RescribirReferencia(referencia, titulo);
        }

        if (incluir) {
            IncluirFilas(grid, peticion.resultado.datos);
            RescribirReferencia(referencia, titulo);
        }

        const cargarTodoDeUna = datosDeEntrada.find((p) => p.parametro === atGridDeDetalle.conCapa);
        let quitarBarra = true;
        if (Definido(cargarTodoDeUna) && cargarTodoDeUna.valor === false && peticion.resultado.datos?.length === 10) {
            quitarBarra = false;
            GridDeDetalle_LeerMasVinculos(grid, datosDeEntrada, posicion);
        }

        FormatarAlGridDeDetalle(grid);
        adjustColumnWidths(grid);

        if (quitarBarra)
            ApiPanel.QuitarBarraDeCarga(grid);

        //EntornoSe.AjustarDivs();
    }

    export function PanelDeBarra(grid: HTMLDivElement): HTMLDivElement {
        let espan: HTMLDivElement = grid.parentElement.parentElement.parentElement as HTMLDivElement;
        const panelParaBarra = espan.querySelector(`.${ltrCss.Espan.Cabecera}`) as HTMLDivElement;
        return panelParaBarra
    }

    function RescribirReferencia(referencia: HTMLAnchorElement, titulo: string) {
        if (!Definido(referencia))
            return;
        referencia.innerText = titulo;
        const partes = titulo.split(':');
        if (partes.length > 1)
            ApiControl.IncluirCss(referencia, ltrCss.Espan.conContenido);
        else
            ApiControl.ExcluirCss(referencia, ltrCss.Espan.conContenido);
    }

    function adjustColumnWidths(grid: HTMLDivElement) {
        let tabla: HTMLDivElement = document.getElementById(grid.id.replace("contenedor", "tabla")) as HTMLDivElement;
        const headers = tabla.querySelectorAll('th.auto-ajustable');

        headers.forEach(header => {
            const columnIndex = Array.from(header.parentNode.children).indexOf(header);
            const cells = tabla.querySelectorAll(`td:nth-child(${columnIndex + 1})`);

            // Calcular el ancho mínimo basado en el contenido del encabezado
            const headerSpan = document.createElement('span');
            headerSpan.style.visibility = 'hidden';
            headerSpan.style.position = 'absolute';
            headerSpan.style.whiteSpace = 'nowrap';
            headerSpan.textContent = header.textContent;
            document.body.appendChild(headerSpan);

            let minWidth = headerSpan.offsetWidth + 40; // Aumentar el padding
            document.body.removeChild(headerSpan);

            let maxWidth = minWidth;

            cells.forEach(cell => {
                const input = cell.querySelector('input');
                if (input) {
                    const tempSpan = document.createElement('span');
                    tempSpan.style.visibility = 'hidden';
                    tempSpan.style.position = 'absolute';
                    tempSpan.style.whiteSpace = 'nowrap';
                    tempSpan.style.font = window.getComputedStyle(input).font; // Usar el mismo estilo de fuente que el input
                    tempSpan.textContent = input.value  // Considerar tanto el valor como el placeholder
                    document.body.appendChild(tempSpan);

                    const width = tempSpan.offsetWidth;
                    if (width > maxWidth) {
                        maxWidth = width;
                    }

                    document.body.removeChild(tempSpan);
                }
            });

            // Añadir más padding al maxWidth
            //maxWidth += 1;

            // Asegurar que maxWidth no sea menor que minWidth
            maxWidth = Math.max(maxWidth, minWidth);

            // Establecer el ancho para el encabezado y todas las celdas en esta columna
            (header as HTMLDivElement).style.width = `${maxWidth}px`;
            (header as HTMLDivElement).style.minWidth = `${minWidth}px`;
            (header as HTMLDivElement).style.maxWidth = `${maxWidth}px`;

            cells.forEach(cell => {
                (cell as HTMLDivElement).style.width = `${maxWidth}px`;
                (cell as HTMLDivElement).style.minWidth = `${minWidth}px`;
                (cell as HTMLDivElement).style.maxWidth = `${maxWidth}px`;

                // Si hay un input, ajustar su ancho también
                const input = cell.querySelector('input');
                if (input) {
                    input.style.width = '100%';
                    input.style.minWidth = `${minWidth}px`;
                    input.style.maxWidth = `${maxWidth}px`;
                }
            });
        });
    }

    function IncluirFilas(grid: HTMLDivElement, datos: any) {
        let tabla: HTMLDivElement = document.getElementById(grid.id.replace('contenedor', 'tabla')) as HTMLDivElement;
        let filaCabecera: ApiDeGrid.PropiedadesDeLaFila[] = ApiDeGrid.ObtenerDescriptorDeLaCabecera(tabla);
        let cuerpoDeLaTabla: HTMLDivElement = tabla.querySelector('.' + ltrCss.crud.tbody);
        let filasExistentes = (cuerpoDeLaTabla.querySelectorAll('.' + ltrCss.crud.fila) as NodeListOf<HTMLDivElement>).length;
        for (let i = 0; i < datos.length; i++) {
            let fila: HTMLDivElement = crearFila(tabla, filaCabecera, datos[i], i + filasExistentes);
            cuerpoDeLaTabla.append(fila);
        }
    }

    function MapearDatosAlGridDeDetalle(grid: HTMLDivElement, datos: any) {
        let tabla: HTMLDivElement = document.getElementById(grid.id.replace('contenedor', 'tabla')) as HTMLDivElement;
        let tbody: HTMLDivElement = tabla.querySelector('.' + ltrCss.crud.tbody);
        EliminarRegistrosDeLaTabla(grid);
        tbody = CrearCuerpoDeLaTabla(tabla, datos);
        var filas = tbody.querySelectorAll<HTMLDivElement>('.' + ltrCss.crud.fila);
        if (filas.length === 0)
            tbody.style.height = "0px";

        tabla.append(tbody);
    }

    function EliminarRegistrosDeLaTabla(grid: HTMLDivElement) {
        let tabla: HTMLDivElement = document.getElementById(grid.id.replace('contenedor', 'tabla')) as HTMLDivElement;
        let tbody: HTMLDivElement = tabla?.querySelector('.' + ltrCss.crud.tbody);
        if (Definido(tbody))
            tabla.removeChild(tbody);

        ApiControl.ResetearEtiquetaDelGrid(grid);
    }

    function FormatarAlGridDeDetalle(grid: HTMLDivElement) {
        let tabla: HTMLDivElement = document.getElementById(grid.id.replace('contenedor', 'tabla')) as HTMLDivElement;
        let tbody: HTMLDivElement = tabla.querySelector('.' + ltrCss.crud.tbody);

        if (!Definido(tbody))
            return;

        const inputsTrue = Array.from(tbody.querySelectorAll('div.div-td input')).filter(input => (input as HTMLInputElement).value === 'true');
        inputsTrue.forEach(input => {
            (input as HTMLInputElement).removeAttribute('readonly');
            ApiControl.IncluirCss(input as HTMLInputElement, ltrCss.Espan.valorTrue);
        });


        const inputsFalse = Array.from(tbody.querySelectorAll('div.div-td input')).filter(input => (input as HTMLInputElement).value === 'false');
        inputsFalse.forEach(input => {
            (input as HTMLInputElement).removeAttribute('readonly');
            ApiControl.IncluirCss(input as HTMLInputElement, ltrCss.Espan.valorFalse);
        });

        let trasCargarGrid = grid.getAttribute(atGridDeDetalle.TrasCargarGrid);
        if (Definido(trasCargarGrid))
            Evaluar('ApiDeMapeos.MapearDatosAlGridDeDetalle', trasCargarGrid, trasCargarGrid.includes('this') ? grid : undefined);
        else {
            let estaElModuloCargado = typeof Crud !== 'undefined';
            if (estaElModuloCargado && Definido(Crud.crudMnt)) {
                Crud.EventosDeExpansores(ltrEventos.Expansores.TrasCargarExpansor, grid.id);
            }
            else {
                if (Definido(Formulario.formulario)) {
                    Formulario.EventosDeExpansores(ltrEventos.Expansores.TrasCargarExpansor, grid.id);
                }
            }
        }
    }

    function CrearCuerpoDeLaTabla(tabla: HTMLDivElement, registros: any): HTMLDivElement {
        let filaCabecera: ApiDeGrid.PropiedadesDeLaFila[] = ApiDeGrid.ObtenerDescriptorDeLaCabecera(tabla);
        let cuerpoDeLaTabla: HTMLDivElement = document.createElement("div");
        cuerpoDeLaTabla.id = tabla.id.replace('-tabla', '-tbody');
        cuerpoDeLaTabla.classList.add(ltrCss.crud.tbody);
        cuerpoDeLaTabla.classList.add(ltrCss.cuerpoDeLaTablaGrid);
        for (let i = 0; i < registros.length; i++) {
            let fila: HTMLDivElement = crearFila(tabla, filaCabecera, registros[i], i);
            cuerpoDeLaTabla.append(fila);
        }
        return cuerpoDeLaTabla;
    }


    function crearFila(tabla: HTMLDivElement, filaCabecera: ApiDeGrid.PropiedadesDeLaFila[], registro: any, numeroDeFila: number): HTMLDivElement {
        let fila = document.createElement("div");
        fila.id = `${tabla.id}_d_tr_${numeroDeFila}`;
        fila.classList.add(ltrCss.crud.fila);
        fila.classList.add(ltrCss.filaDelGrid);
        let idDelElemento: number = 0;
        for (let j = 0; j < filaCabecera.length; j++) {
            let columnaCabecera: ApiDeGrid.PropiedadesDeLaFila = filaCabecera[j];
            var propiedad = columnaCabecera.propiedad === ltrPropiedades.Elemento.DarDeAlta ? ltrPropiedades.Elemento.Baja : columnaCabecera.propiedad;
            var valor = ObtenerPropiedad(registro, propiedad, "", false);
            if (columnaCabecera.propiedad === atControl.id) {
                if (!EsNumero(valor))
                    MensajesSe.EmitirMensajeDeExcepcion(`Al crear una fila`, `No se puede crear la fila en la tabla ${tabla.id} ya que el id del elemento leido debe ser numérico y es ${valor}`);
                idDelElemento = Numero(valor);
            }

            let celdaDelTd: HTMLDivElement = crearCelda(tabla, numeroDeFila, fila, columnaCabecera, j, valor);
            fila.append(celdaDelTd);
        }


        fila.setAttribute(atControl.idDelElemento, idDelElemento.toString());
        let title: string = ObtenerPropiedad(registro, ltrPropiedades.Elemento.Descripcion, "", false);
        if (IsNullOrEmpty(title)) {
            title = ObtenerPropiedad(registro, ltrPropiedades.Elemento.ConLineas.Anotacion, "", false);
            if (IsNullOrEmpty(title)) {
                title = ObtenerPropiedad(registro, ltrPropiedades.Elemento.Detalle.Titulo, "", false);
            }
        }
        if (!IsNullOrEmpty(title)) fila.setAttribute(atControl.title, title);

        return fila;
    }

    function crearCelda(tabla: HTMLDivElement, numeroDeFila: number, fila: HTMLDivElement, columnaCabecera: ApiDeGrid.PropiedadesDeLaFila, numeroDeCelda: number, valor: string): HTMLDivElement {
        let celdaDelTd: HTMLDivElement = ApiDeGrid.CrearCeldaDelTd(tabla.id, fila, numeroDeCelda, columnaCabecera, false);
        if (columnaCabecera.esAccion) {
            InsertarUnaReferenciaEnLaCelda(numeroDeFila, columnaCabecera, celdaDelTd, valor, 'propiedad')
        }
        else if (columnaCabecera.tipo == ltrTipoControl.CirculoEnCelda) {
            InsertarUnCirculoEnLaCelda(tabla.id, fila.id, columnaCabecera, celdaDelTd, valor);
        }
        else
            insertarInputEnElTd(tabla, fila.id, columnaCabecera, celdaDelTd, valor);
        return celdaDelTd;
    }

    function insertarInputEnElTd(tabla: HTMLDivElement, idFila: string, columnaCabecera: ApiDeGrid.PropiedadesDeLaFila, celdaDelTd: HTMLDivElement, valor: any) {
        let input = ApiDeGrid.CrearInputDeLaCelda(tabla.id, idFila, columnaCabecera, celdaDelTd, valor);
        celdaDelTd.append(input);
    }

    export function InsertarUnCirculoEnLaCelda(idPadre: string, idFila: string, columnaCabecera: ApiDeGrid.PropiedadesDeLaFila, celdaDelTd: HTMLDivElement, valor: any) {
        let input = ApiDeGrid.CrearCirculoEnCelda(idPadre, idFila, columnaCabecera, celdaDelTd, valor);
        celdaDelTd.append(input);
        celdaDelTd.style.verticalAlign = 'middle';
    }

    export function InsertarUnaReferenciaEnLaCelda(numeroDeFila: number, columnaCabecera: ApiDeGrid.PropiedadesDeLaFila, celdaDelTd: HTMLDivElement, valor: any, mostrar: string) {
        let accion: string = columnaCabecera.accion.replace('-id', '-tabla_d_tr');
        accion = accion.replace('numeroDeFila', numeroDeFila.toString());
        let a: HTMLAnchorElement = columnaCabecera.propiedad === ltrPropiedades.Elemento.DarDeAlta && !valor
            ? ApiControl.CrearRef(ltrPropiedades.Elemento.DarDeBaja, accion.replace(ltrEventos.ModalEdicion.DarDeAlta, ltrEventos.ModalEdicion.DarDeBaja), false)
            : ApiControl.CrearRef(mostrar == 'propiedad' ? columnaCabecera.propiedad : valor, accion, false);
        if (Definido(a)) {
            //a.style.setProperty("textAlign", celdaDelTd.style.textAlign, "important");

            a.style.textAlign = celdaDelTd.style.textAlign;
            celdaDelTd.append(a);
        }
        celdaDelTd.style.textAlign = "center";
    }



}

namespace MapearAlDiccionario {
    export function Filtros(panel: HTMLDivElement, diccionario: Diccionario<any>, clave: string = ltrClaveDeEstado.filtrosDeUnPost): void {
        let controles: NodeListOf<HTMLElement> = panel.querySelectorAll(`[${atControl.tipo}]:not([${atControl.tipo}=''])`) as NodeListOf<HTMLElement>;
        let filtrosDeIu: Array<Tipos.Filtro> = diccionario.Contiene(clave)
            ? diccionario.Obtener(clave)
            : new Array<Tipos.Filtro>();

        for (let i: number = 0; i < controles.length; i++) {
            let control = controles[i];
            if (!EsTrue(control.getAttribute(atControl.filtro)))
                continue;

            let tipo = controles[i].getAttribute(atControl.tipo);
            switch (tipo) {
                case ltrTipoControl.Editor: guardarEditor(control as HTMLInputElement, filtrosDeIu); break;
                case ltrTipoControl.Check: guardarCheck(control as HTMLInputElement, filtrosDeIu); break;
                case ltrTipoControl.ListaDinamica: guardarListaDinamica(control as HTMLInputElement, filtrosDeIu); break;
                case ltrTipoControl.ListaDeElementos: guardarListaDeElementos(control as HTMLSelectElement, filtrosDeIu); break;
                case ltrTipoControl.ListaDeValores: guardarListaDeValores(control as HTMLSelectElement, filtrosDeIu); break;
                case ltrTipoControl.FiltroEntreImportes: guardarFiltroEntreImportes(control as HTMLInputElement, filtrosDeIu); break;
                case ltrTipoControl.FiltroEntreRangos: guardarFiltroEntreRangos(control as HTMLInputElement, filtrosDeIu); break;
                case ltrTipoControl.FiltroEntreFechas: guardarFiltroEntreFechas(control as HTMLInputElement, filtrosDeIu); break;
            }

            diccionario.Agregar(clave, filtrosDeIu);
        }
    }

    export function UnRestrictor(restrictor: HTMLInputElement, diccionario: Diccionario<any>): void {
        let filtrosDeIu: Array<Tipos.Filtro> = diccionario.Contiene(ltrClaveDeEstado.filtrosDeUnPost)
            ? diccionario.Obtener(ltrClaveDeEstado.filtrosDeUnPost)
            : new Array<Tipos.Filtro>();
        guardarEditor(restrictor, filtrosDeIu);
        diccionario.Agregar(ltrClaveDeEstado.filtrosDeUnPost, filtrosDeIu);
    }

    function guardarEditor(control: HTMLInputElement, filtros: Array<Tipos.Filtro>): void {
        if (IsNullOrEmpty(control.value))
            return;
        let filtro = new Tipos.Filtro(control.id, ltrTipoControl.Editor);
        filtro.Atributos.Agregar(atControl.valorInput, control.value);
        filtros.push(filtro);
    }
    function guardarCheck(control: HTMLInputElement, filtros: Array<Tipos.Filtro>): void {
        let filtro = new Tipos.Filtro(control.id, ltrTipoControl.Check);
        filtro.Atributos.Agregar(atCheck.chequeado, control.checked);
        filtros.push(filtro);
    }
    function guardarListaDinamica(control: HTMLInputElement, filtros: Array<Tipos.Filtro>): void {
        let seleccionado = Numero(control.getAttribute(atListasDinamicas.idSeleccionado));
        if (seleccionado === 0)
            return;
        let filtro = new Tipos.Filtro(control.id, ltrTipoControl.ListaDinamica);
        filtro.Atributos.Agregar(atControl.valorInput, control.value);
        filtro.Atributos.Agregar(atListasDinamicas.idSeleccionado, seleccionado);
        filtro.Atributos.Agregar(atListasDinamicas.idSelAlEntrar, Numero(control.getAttribute(atListasDinamicas.idSelAlEntrar)));
        filtro.Atributos.Agregar(atListasDinamicas.ultimaCadenaBuscada, control.getAttribute(atListasDinamicas.ultimaCadenaBuscada));
        filtros.push(filtro);
    }
    function guardarListaDeValores(lista: HTMLSelectElement, filtros: Array<Tipos.Filtro>): void {
        if (IsNullOrEmpty(lista.value) || Numero(lista.value) === Numero(atListas.noSeleccionado))
            return;

        let filtro = new Tipos.Filtro(lista.id, ltrTipoControl.ListaDeValores);
        filtro.Atributos.Agregar(atControl.valorInput, lista.value);
        filtros.push(filtro);
    }
    function guardarListaDeElementos(lista: HTMLSelectElement, filtros: Array<Tipos.Filtro>): void {
        if (IsNullOrEmpty(lista.value) || Numero(lista.value) === Numero(atListas.noSeleccionado))
            return;

        let filtro = new Tipos.Filtro(lista.id, ltrTipoControl.ListaDeElementos);
        filtro.Atributos.Agregar(atControl.valorInput, lista.value);
        filtros.push(filtro);
    }

    function guardarFiltroEntreImportes(importeDesde: HTMLInputElement, filtros: Array<Tipos.Filtro>): void {
        var importes = ApiControl.LeerEntreImportes(importeDesde).split(ltrSimbolos.separadorDeRangos);
        if (importes.length != 2 || importes[0] === literal.undefined || importes[1] === literal.undefined)
            return;

        let filtro = new Tipos.Filtro(importeDesde.id, ltrTipoControl.FiltroEntreImportes);
        filtro.Atributos.Agregar(atControl.valorInput, Numero(importes[0]));
        filtro.Atributos.Agregar(atEntreImportes.valorHasta, Numero(importes[1]));
        filtros.push(filtro);
    }

    function guardarFiltroEntreRangos(rangoDesde: HTMLInputElement, filtros: Array<Tipos.Filtro>): void {
        var rangos = ApiControl.LeerEntreRangos(rangoDesde).split(ltrSimbolos.separadorDeRangos);
        if (rangos.length != 2 || rangos[0] === literal.undefined || rangos[1] === literal.undefined)
            return;

        let filtro = new Tipos.Filtro(rangoDesde.id, ltrTipoControl.FiltroEntreRangos);
        filtro.Atributos.Agregar(atControl.valorInput, rangos[0]);
        filtro.Atributos.Agregar(atEntreImportes.valorHasta, rangos[1]);
        filtros.push(filtro);
    }
    function guardarFiltroEntreFechas(fechaDesde: HTMLInputElement, filtros: Array<Tipos.Filtro>): void {
        var fechas = ApiControl.LeerEntreFechas(fechaDesde).split(ltrSimbolos.separadorDeDosFechas);
        if (fechas.length == 2 && IsNullOrEmpty(fechas[0]) || IsNullOrEmpty(fechas[1]))
            return;
        var fecha1 = "";
        var fecha2 = "";

        if (fechas.length == 2) {
            fecha1 = fechas[0];
            fecha2 = fechas[1];
        }
        if (fechas.length == 4) {
            if (IsNullOrEmpty(fechas[3])) fecha1 = `${fechas[0]}-${fechas[1]}-${fechas[2]}`;
            if (IsNullOrEmpty(fechas[0])) fecha2 = `${fechas[1]}-${fechas[2]}-${fechas[3]}`;
        }
        if (fechas.length == 6) {
            fecha1 = `${fechas[0]}-${fechas[1]}-${fechas[2]}`;
            fecha2 = `${fechas[3]}-${fechas[4]}-${fechas[5]}`;
        }

        let filtro = new Tipos.Filtro(fechaDesde.id, ltrTipoControl.FiltroEntreFechas);
        filtro.Atributos.Agregar(atControl.valorInput, fecha1);
        filtro.Atributos.Agregar(atEntreFechas.valorFechaHasta, fecha2);
        filtros.push(filtro);
    }
}

namespace MapearAlCrud {

    export function RestrictoresDeLaUrl(crud: Crud.CrudMnt): void {
        if (crud.PeticionDeMenu && !Definido(ObtenerParametroUrl(ltrClaveDeEstado.restrictoresUrl)))
            return;

        Restrictores(crud, ltrClaveDeEstado.restrictoresUrl);
    }

    export function RestrictoresPasadosEnEstado(crud: Crud.CrudMnt): void {
        Restrictores(crud, ltrClaveDeEstado.restrictoresDeUnPost);
    }

    function Restrictores(crud: Crud.CrudMnt, claveRestrictora): void {
        let restrictores: Tipos.Restrictor[] = crud.Estado.Obtener(claveRestrictora) as Tipos.Restrictor[];
        if (NoDefinido(restrictores))
            return;
        for (let i = 0; i < restrictores.length; i++) {
            aplicarRestrictor(crud, restrictores[i]);
        }
    }

    export function FiltrosPasadosEnEstado(crud: Crud.CrudMnt | Formulario.Jerarquia): void {
        let filtrosDeIu: Array<Tipos.Filtro> = crud.Estado.Sacar(ltrClaveDeEstado.filtrosDeUnPost) as Array<Tipos.Filtro>;
        if (NoDefinido(filtrosDeIu))
            return;
        for (let i: number = 0; i < filtrosDeIu.length; i++) {
            let filtroDeIu: Tipos.Filtro = Tipos.ClonarFiltro(filtrosDeIu[i] as Tipos.Filtro);
            ApiDeFiltro.MapearFiltrosPasados(filtroDeIu);
        }
    }

    export function FiltrosDeLaUrl(crud: Crud.CrudMnt): void {

        if (crud.PeticionDeMenu && !Definido(ObtenerParametroUrl(ltrClaveDeEstado.filtrosUrl)))
            return;

        let filtros: Tipos.Restrictor[] = crud.Estado.Obtener(ltrClaveDeEstado.filtrosUrl) as Tipos.Restrictor[];
        if (NoDefinido(filtros))
            return;
        for (let i = 0; i < filtros.length; i++)
            aplicarRestrictor(crud, filtros[i]);
    }

    function aplicarRestrictor(crud: Crud.CrudMnt, restrictor: Tipos.Restrictor): void {
        let control = MapearAlFiltro.MapearRestrictor(crud.PanelFiltro, restrictor.Propiedad.toLowerCase(), restrictor.Valor, restrictor.Texto, false);

        if (!Definido(control)) {
            let divs: NodeListOf<HTMLDivElement> = crud.Cuerpo.querySelectorAll(`div[${atControl.tipoModal}=${enumTipoDeModal.ModalDeFiltrado}`) as NodeListOf<HTMLDivElement>;
            for (let i: number = 0; i < divs.length; i++) {
                control = MapearAlFiltro.MapearRestrictor(divs[i], restrictor.Propiedad.toLowerCase(), restrictor.Valor, restrictor.Texto, false);
                if (Definido(control)) break;
            }
        }

        if (!Definido(control))
            MensajesSe.Info(`no se ha localizado el ${(!restrictor.SoloFiltra ? 'restrictor' : 'filtro')}  '${restrictor.Propiedad}' en la zona de filtro`);

        if (!restrictor.SoloFiltra) {
            //if (!EsTrue(crud.Estado.Obtener(ClaveDeEstado.soloMapearEnElFiltro))) {
            if (EsTrue(crud.CuerpoCabecera.getAttribute(atMantenimniento.permiteCrear)))
                MapearPanelDeCreacion.MapearRestrictores(crud.crudDeCreacion.PanelDeCrear, restrictor.Propiedad.toLowerCase(), restrictor.Valor, restrictor.Texto);

            if (EsTrue(crud.CuerpoCabecera.getAttribute(atMantenimniento.permiteEditar)))
                MapearPanelDeEdicion.MapearRestrictores(crud.crudDeEdicion.PanelDeEditar, restrictor.Propiedad.toLowerCase(), restrictor.Valor, restrictor.Texto);
            //}
        }
        crud.DespuesDeAplicarUnRestrictor(restrictor);
    }
}

function ObtenerFecha(controlDeFecha: HTMLInputElement, propiedadDto: string): Date {
    let obligatorio: boolean = EsTrue(controlDeFecha.getAttribute(atControl.obligatorio));
    let valorDeFecha: string = controlDeFecha.value; //.replace(/\n/g, "\r\n");
    let fechaHoraFijada = false;
    if (obligatorio && NoDefinido(valorDeFecha)) {
        if (controlDeFecha.readOnly) {
            valorDeFecha = new Date(Date.now()).toISOString();
            fechaHoraFijada = true;
        }
        else {
            controlDeFecha.classList.remove(ltrCss.crtlValido);
            controlDeFecha.classList.add(ltrCss.crtlNoValido);
            throw new Error(`El campo: ${propiedadDto}, es obligatorio`);
        }
    }

    let fecha: Date = new Date(valorDeFecha);
    let utcFecha: Date = undefined;
    if (EsFechaValida(fecha)) {
        HoraVinculadaAlControl(controlDeFecha, fechaHoraFijada, fecha);
        utcFecha = new Date(Date.UTC(fecha.getFullYear(), fecha.getMonth(), fecha.getDate(), fecha.getHours(), fecha.getMinutes(), fecha.getSeconds(), fecha.getMilliseconds()));
    }
    return utcFecha;
}

function HoraVinculadaAlControl(controlDeFecha: HTMLInputElement, fechaHoraFijada: boolean, fecha: Date) {
    let idHora = controlDeFecha.getAttribute(atSelectorDeFecha.hora);
    if (!IsNullOrEmpty(idHora)) {
        let controlDeHora: HTMLInputElement = document.getElementById(idHora) as HTMLInputElement;

        if (!fechaHoraFijada) {
            let valorDeHora = controlDeHora.value.split(':');
            let hora: number = Numero(valorDeHora[0]);
            let minuto: number = Numero(valorDeHora[1]);
            let segundos: number = Numero(valorDeHora[2]);
            let milisegundos: number = Numero(controlDeHora.getAttribute(atSelectorDeFecha.milisegundos));
            fecha.setHours(hora);
            fecha.setMinutes(minuto);
            fecha.setSeconds(segundos);
            fecha.setMilliseconds(milisegundos);
        }
    }
    else {
        fecha.setHours(0);
        fecha.setMinutes(0);
        fecha.setSeconds(0);
        fecha.setMilliseconds(0);
    }
}

function ValidarMascara(input: HTMLInputElement) {
    var mascara = input.getAttribute(atControl.mascara);
    if (Definido(mascara)) {
        var expresionRegular = new RegExp(mascara);
        if (!expresionRegular.test(input.value)) {
            input.classList.remove(ltrCss.crtlValido);
            input.classList.add(ltrCss.crtlNoValido);
            MensajesSe.EmitirExcepcion('ValidarMascara',
                `En el control '${input.id}' ha de indicar '${input.getAttribute(atControl.placeholder)}'`,
                `En el control '${input.id}' ha de cumplir con la máscara '${input.getAttribute(atControl.mascara)}'`);
        }
    }
}
