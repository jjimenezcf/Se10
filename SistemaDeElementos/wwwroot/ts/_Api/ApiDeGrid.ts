namespace ApiDeGrid {
    export class PropiedadesDeLaFila {
        id: string;
        propiedad: string;
        visible: boolean;
        estilo: CSSStyleDeclaration;
        claseCss: string;
        editable: boolean;
        tipo: string;
        anchoEnPixel: number;
        esAccion: boolean;
        esFecha: boolean;
        formato: string;
        accion: string;
        constructor() {

        }

        get Clases(): string[] {
            return this.claseCss.split(' ').filter(clase => clase.trim() !== '');
        }
    }

    export class EstadoDeLaColumna {
        id: string;
        propiedad: string;
        etiqueta: string;
        visible: boolean;
        constructor(id: string, propiedad: string, etiqueta: string, visible: boolean) {
            this.id = id;
            this.propiedad = propiedad;
            this.etiqueta = etiqueta;
            this.visible = visible;
        }
    }

    export class VisibilidadDeColumna {
        Id: string;
        Propiedad: string;
        Visible: boolean;
        constructor(id: string, propiedad: string, visible: boolean) {
            this.Id = id;
            this.Propiedad = propiedad;
            this.Visible = visible;
        }
    }

    export class TamanoDeColumna {
        Id: string;
        Tamano: string;
        constructor(id: string, tamano: string) {
            this.Id = id;
            this.Tamano = tamano;
        }
    }

    export class DisposicionDeColumna {
        Id: string;
        Anterior: string;
        Propiedad: string;
        Posterior: string;
        Posicion: number;
        constructor(id: string, propiedad: string, anterior: string, posterior: string, posicion: number) {
            this.Id = id;
            this.Anterior = anterior;
            this.Propiedad = propiedad;
            this.Posterior = posterior;
            this.Posicion = posicion;
        }
    }

    export class OrdenDeColumna {
        Id: string;
        Propiedad: string;
        OrdenadoPor: string;
        Modo: string;
        constructor(id: string, propiedad: string, ordenadoPor: string, modo: string) {
            this.Id = id;
            this.Propiedad = propiedad;
            this.OrdenadoPor = ordenadoPor;
            this.Modo = modo;
        }
    }

    export function OrdenacionDelResultado(ordenacion: Tipos.Ordenacion): Array<OrdenDeColumna> {
        var array = new Array<OrdenDeColumna>();
        for (let i = 0; i < ordenacion.Count(); i++) {
            const orden = ordenacion.Leer(i);
            array.push(new OrdenDeColumna(orden.IdColumna, orden.Propiedad, orden.OrdenarPor, orden.Modo))
        }
        return array;
    }

    export function DisposicionDelEncolumnado(tabla: HTMLDivElement): Array<DisposicionDeColumna> {
        var array = new Array<DisposicionDeColumna>();
        var cabecera = tabla.querySelectorAll('.' + ltrCss.crud.fila)[0] as HTMLDivElement;
        var ths = cabecera.querySelectorAll('.' + ltrCss.crud.columna);
        var anterior = ""
        for (let i = 0; i < ths.length; i++) {
            let columna = ths[i];
            let propiedad = columna.getAttribute(atControl.propiedad);
            let posterior = i === ths.length - 1 ? "" : ths[i + 1].getAttribute(atControl.propiedad);
            array.push(new DisposicionDeColumna(columna.id, propiedad, anterior, posterior, i));
            anterior = propiedad;
        }
        return array;
    }

    export function TamanoDelEncolumnado(tabla: HTMLDivElement): Array<TamanoDeColumna> {
        var array = new Array<TamanoDeColumna>();
        var cabecera = tabla.querySelectorAll('.' + ltrCss.crud.fila)[0] as HTMLDivElement;
        var ths = cabecera.querySelectorAll('.' + ltrCss.crud.columna) as NodeListOf<HTMLDivElement>;
        for (let i = 0; i < ths.length; i++) {
            array.push(new TamanoDeColumna(ths[i].getAttribute(atControl.propiedad), ths[i].style.width));
        }
        return array;
    }
    export function Encolumnado(filaDeCabecera: Array<PropiedadesDeLaFila>): Array<EstadoDeLaColumna> {

        let resultado = new Array<EstadoDeLaColumna>();
        for (let a: number = 0; a < filaDeCabecera.length; a++) {
            var th = document.getElementById(filaDeCabecera[a].id) as HTMLDivElement;
            var propiedad = th.getAttribute(atControl.propiedad) ?? "";
            if (th.classList.contains(ltrCss.columnaOculta) && propiedad.toLowerCase().startsWith(literal.id) && th.getAttribute(atControl.tipoColumna) === ltrTipoControl.Editor)
                continue;
            if (propiedad === ltrMantenimiento.CheckDeSeleccion)
                continue;

            var href = th.querySelector('a') as HTMLAnchorElement;
            var etiqueta = "";
            if (Definido(href)) etiqueta = href.textContent.trim();
            else {
                var div = th.querySelector('.div-de-columna') as HTMLDivElement;
                etiqueta = div.textContent.trim();
            }

            if (IsNullOrEmpty(etiqueta) || etiqueta === '_')
                continue;

            let visible = !th.classList.contains(ltrCss.columnaOculta)

            resultado.push(new EstadoDeLaColumna(filaDeCabecera[a].id, propiedad, etiqueta, visible));

        }
        return resultado;
    }

    export function CrearHTMLParaOcultar(array: Array<EstadoDeLaColumna>): string {
        array.sort((a, b) => {
            if (a.etiqueta.toLowerCase() < b.etiqueta.toLowerCase()) return -1;
            if (a.etiqueta.toLowerCase() > b.etiqueta.toLowerCase()) return 1;
            return 0;
        });
        let html = "";
        let grupo = [];
        let estiloPorLinea = `style="display: flex; justify-content: space-between;margin-left: 10px;margin-top: 10px;"`;
        for (let i = 0; i < array.length; i++) {
            grupo.push(array[i]);
            if (grupo.length === 4) {
                html += `<div ${estiloPorLinea}>
                           ${IncluirCheck(grupo, 0)}
                           ${IncluirCheck(grupo, 1)}
                           ${IncluirCheck(grupo, 2)}
                           ${IncluirCheck(grupo, 3)}
                        </div>
                        <br>`;
                grupo = [];
            }
        }
        if (grupo.length > 0) {
            html += `<div ${estiloPorLinea}>`;
            html += IncluirCheck(grupo, 0);
            if (grupo.length > 1) {
                html += IncluirCheck(grupo, 1);
                if (grupo.length > 2) {
                    html += IncluirCheck(grupo, 2);
                    if (grupo.length > 3) {
                        html += IncluirCheck(grupo, 3);
                    }
                    else {
                        html += `<div style="width: 25%;"></div>`
                    }
                }
                else {
                    html += `<div style="width: 25%;"></div><div style="width: 25%;"></div>`
                }
            }
            else {
                html += `<div style="width: 25%;"></div><div style="width: 25%;"></div><div style="width: 25%;"></div>`
            }
            html += `</div><br>`;
        }
        return html;
    }

    function IncluirCheck(grupo, indice): string {
        let check =
            `<div style="width: 25%;">
         ${grupo[indice].visible
                ?
                `<input type="checkbox" id="${grupo[indice].id}" propiedad="${grupo[indice].propiedad}" value="${grupo[indice].propiedad}" checked>
            <label for="${grupo[indice].id}" propiedad="${grupo[indice].propiedad}">${grupo[indice].etiqueta}</label>`
                :
                `<input type="checkbox" id="${grupo[indice].id}" propiedad="${grupo[indice].propiedad}" value="${grupo[indice].propiedad}">
            <label for="${grupo[indice].id}" propiedad="${grupo[indice].propiedad}">${grupo[indice].etiqueta}</label>`}
         </div>`;

        return check;
    }

    export function VisibilidadDeColumnas(modal: HTMLDivElement): Array<VisibilidadDeColumna> {
        let checkboxes: NodeListOf<HTMLInputElement> = modal.querySelectorAll('input[type="checkbox"]') as NodeListOf<HTMLInputElement>;

        var visibles = new Array<VisibilidadDeColumna>();
        for (var i = 0; i < checkboxes.length; i++) {
            visibles.push(new VisibilidadDeColumna(checkboxes[i].id, checkboxes[i].getAttribute(atControl.propiedad), checkboxes[i].checked));
        }
        return visibles;
    }

    export function AplicarVisibilidad(tabla: HTMLDivElement, modal: HTMLDivElement): void {
        let visibilidad = VisibilidadDeColumnas(modal);
        for (var i = 0; i < visibilidad.length; i++) {
            ApiDeGrid.OcultarMostrarColumna(tabla, visibilidad[i].Propiedad, visibilidad[i].Visible);
        }
    }

    export function ObtenerDescriptorDeLaCabeceraMovil(tabla: HTMLDivElement): Array<PropiedadesDeLaFila> {
        const propiedadesVisibles = ['chksel', 'nombre'];

        var cabecera = tabla.querySelectorAll('.' + ltrCss.crud.fila)[0] as HTMLDivElement;
        var ths = cabecera.querySelectorAll('.' + ltrCss.crud.columna) as NodeListOf<HTMLDivElement>;

        // Convertimos el NodeList a Array para usar 'some' y verificar si existe la propiedad 'nombre'
        const existeColumnaNombre = Array.from(ths).some(th =>
            th.getAttribute(atControl.propiedad)?.toLowerCase() === 'nombre'
        );

        if (!existeColumnaNombre) {
            // Si no existe la columna 'nombre', abortamos este método y devolvemos el descriptor estándar
            return ObtenerDescriptorDeLaCabecera(tabla);
        }

        ths.forEach((th) => {
            const propiedad = th.getAttribute(atControl.propiedad)?.toLowerCase();
            if (propiedadesVisibles.includes(propiedad)) {
                th.classList.remove(ltrCss.columnaOculta);
            } else {
                th.classList.add(ltrCss.columnaOculta);
            }
        });

        var columnas = ObtenerDescriptorDeLaCabecera(tabla);

        columnas.forEach((col) => {
            const propiedad = col.propiedad?.toLowerCase();
            if (!propiedadesVisibles.includes(propiedad)) {
                col.visible = false;
                if (!col.Clases.includes(ltrCss.columnaOculta)) {
                    col.claseCss = (col.claseCss + ' ' + ltrCss.columnaOculta).trim();
                }
            }
        });

        return columnas;
    }

    export function ObtenerDescriptorDeLaCabecera(tabla: HTMLDivElement): Array<PropiedadesDeLaFila> {
        let columnasVisiblesSinTamano = 0;
        let filaCabecera: Array<PropiedadesDeLaFila> = new Array<PropiedadesDeLaFila>();
        var cabecera = tabla.querySelectorAll('.' + ltrCss.crud.fila)[0] as HTMLDivElement;
        var ths = cabecera.querySelectorAll('.' + ltrCss.crud.columna) as NodeListOf<HTMLDivElement>;

        for (let i = 0; i < ths.length; i++) {
            let tamanoFijo = SepararCadenaEnNumeroYUnidad(ths[i].getAttribute(atGrid.tamanoFijo));
            if (!estaOculta(ths[i]) && !EsMayorDeCero(tamanoFijo[0]) && ths[i].getAttribute(atControl.propiedad) !== ltrMantenimiento.CheckDeSeleccion)
                columnasVisiblesSinTamano = columnasVisiblesSinTamano + 1;
        }

        let porcentajeAncho: number = 100 / columnasVisiblesSinTamano;

        for (let i = 0; i < ths.length; i++) {
            let p: PropiedadesDeLaFila = new PropiedadesDeLaFila();
            p.id = ths[i].id;
            p.visible = !estaOculta(ths[i]);
            p.claseCss = ths[i].className;
            p.estilo = ths[i].style;
            let propiedad = ths[i].getAttribute(atControl.propiedad);
            if (!estaOculta(ths[i])) {
                if (propiedad === ltrMantenimiento.CheckDeSeleccion) {
                    AsignarWidth(p, atGrid.tamanoFijoChkSel);
                }
                else {
                    let atTamanoFijo = ths[i].getAttribute(atGrid.tamanoFijo);
                    let tamanoFijo = SepararCadenaEnNumeroYUnidad(atTamanoFijo);
                    if (tamanoFijo[0] === 0 && (tamanoFijo[1].trim() === '%' || IsNullOrEmpty(tamanoFijo[1]))) {
                        AsignarWidth(p, IsNullOrEmpty(ths[i].style.width) ? porcentajeAncho + '%' : ths[i].style.width);
                    }
                    else {
                        if (EsMayorDeCero(tamanoFijo[0]) && IsNullOrEmpty(tamanoFijo[1])) {
                            tamanoFijo[1] = `px`;
                        }

                        if (!p.Clases.find(item => item === ltrCss.crud.columna) || !p.Clases.find(item => item === ltrCss.Detalle.ColumnaAccion))
                            AsignarWidth(p, tamanoFijo[0] + tamanoFijo[1]);
                    }
                }
            }
            else {
                if (!ApiDelCrud.EsElGridDeUnMnt(tabla)) {
                    AsignarWidth(p, 0 + 'px');
                }
                else {
                    let tamanoFijo = SepararCadenaEnNumeroYUnidad(ths[i].getAttribute(atGrid.tamanoFijo));
                    if (EsMayorDeCero(tamanoFijo[0]))
                        AsignarWidth(p, tamanoFijo[0] + (IsNullOrEmpty(tamanoFijo[1]) ? 'px' : tamanoFijo[1]));
                }
            }

            p.anchoEnPixel = ths[i].getBoundingClientRect().width;
            p.editable = false;
            p.propiedad = ths[i].getAttribute(atControl.propiedad);
            p.tipo = ths[i].getAttribute(atControl.tipoColumna)
            p.esAccion = EsTrue(ths[i].getAttribute(atGridDeDetalle.EsAccion));
            if (p.esAccion) p.accion = ths[i].getAttribute(atGridDeDetalle.Accion);
            p.esFecha = EsTrue(ths[i].getAttribute(atGrid.EsFecha));
            p.formato = ths[i].getAttribute(atGrid.formato);
            filaCabecera.push(p);
        }
        return filaCabecera;
    }


    export function AsignarWidth(p: PropiedadesDeLaFila, tamanoFijo: string) {
        p.estilo.width = `${tamanoFijo}`;
        p.estilo.minWidth = `${tamanoFijo}`;
        p.estilo.maxWidth = `${tamanoFijo}`;
    }

    export function ColumnaVisible(tabla: HTMLDivElement, idColumna: string) {
        let columna: HTMLDivElement = document.getElementById(idColumna) as HTMLDivElement;
        var tamano = columna.getAttribute(atGrid.tamanoFijo);
        hacerVisible(tabla, columna, tamano);
    }

    export function ColumnaInvisible(tabla: HTMLDivElement, idColumna: string) {
        let columna: HTMLDivElement = document.getElementById(idColumna) as HTMLDivElement;
        hacerInvisible(tabla, columna);
    }

    export function OcultarMostrarColumna(tabla: HTMLDivElement, propiedad: string, visible: boolean = null, recorrerCeldas: boolean = false) {
        var cabecera = tabla.querySelectorAll('.' + ltrCss.crud.fila)[0] as HTMLDivElement;
        //const clickedElement = event.currentTarget as HTMLElement;
        var ths = cabecera.querySelectorAll('.' + ltrCss.crud.columna) as NodeListOf<HTMLDivElement>;
        for (let i = 0; i < ths.length; i++) {
            if (ths[i].getAttribute(atControl.propiedad) === propiedad.toLocaleLowerCase()) {
                let columna: HTMLDivElement = ths[i];
                var tamano = columna.getAttribute(atGrid.tamanoFijo);
                if (visible === null) {
                    if (estaOculta(columna))
                        hacerVisible(tabla, columna, tamano);
                    else
                        hacerInvisible(tabla, columna);
                }
                else {
                    if (recorrerCeldas) {
                        if (visible)
                            hacerVisible(tabla, columna, tamano);
                        else
                            hacerInvisible(tabla, columna);
                    }
                    else {
                        if (estaOculta(columna)) {
                            if (!Definido(columna.getAttribute(atGrid.tamanoFijo))) {
                                columna.setAttribute(atGrid.tamanoFijo, atGrid.tamanoFijoAlHacerVisible)
                                tamano = atGrid.tamanoFijoAlHacerVisible;
                            }
                            if (visible)
                                hacerVisible(tabla, columna, tamano);
                        }
                        else {
                            if (!visible)
                                hacerInvisible(tabla, columna);
                        }
                    }
                }
            }
        }
    }

    export function ObtenerAnchos(tabla: HTMLDivElement): Array<Parametro> {
        let Anchos: Array<Parametro> = new Array<Parametro>();
        var cabecera = tabla.querySelectorAll('.' + ltrCss.crud.fila)[0] as HTMLDivElement;
        var columnas = cabecera.querySelectorAll('.' + ltrCss.crud.columna);
        let sumaLoQueHayComoPorcentaje: number = 0;
        for (let i = 0; i < columnas.length; i++) {
            let ancho = (columnas[i] as HTMLDivElement).style.width;
            if (ancho.indexOf('%') > 0 && !columnas[i].classList.contains(ltrCss.columnaOculta)) {
                sumaLoQueHayComoPorcentaje = sumaLoQueHayComoPorcentaje + Numero(ancho, false, '.');
            }
        }

        let loQueHay: number = 0;
        let loQueDebeSer: number = 0;
        for (let i = 0; i < columnas.length; i++) {
            let ancho = (columnas[i] as HTMLDivElement).style.width;
            if ((columnas[i] as HTMLDivElement).style.width === '')
                Anchos.push(new Parametro(columnas[i].id, '0%'));
            else {
                if (ancho.indexOf('%') > 0 && !columnas[i].classList.contains(ltrCss.columnaOculta)) {
                    loQueHay = Numero(ancho, false, '.');
                    loQueDebeSer = (loQueHay * 100) / sumaLoQueHayComoPorcentaje;
                    Anchos.push(new Parametro(columnas[i].id, loQueDebeSer + '%'));
                }
                else
                    Anchos.push(new Parametro(columnas[i].id, ancho));
            }
        }
        return Anchos;
    }

    //export function AplicarPorcentajes(tabla: HTMLDivElement, anchos: Parametro[]) {
    //    var cabecera =  tabla.querySelectorAll('.' + ltrCss.crud.fila)[0] as HTMLDivElement;
    //    var columnas = cabecera.querySelectorAll('.' + ltrCss.crud.columna);
    //    for (let i = 0; i < columnas.length; i++) {
    //        let idCabecera: string = columnas[i].id;
    //        let ancho = ObtenerPropiedad(anchos, idCabecera);
    //        AsignarWidthALaCelda(columnas[i], ancho);
    //        var partes = SepararCadenaEnNumeroYUnidad(ancho);
    //        if (partes[1] === "%") {
    //            ApiDeGrid.ResetearWidthAlTh(columnas[i], columnas[i].getBoundingClientRect().width + 'px');
    //        }
    //    }
    //    ApiDeGrid.AplicarWidthAlBody(tabla);
    //}

    export function UltimaColumnaVisible(tabla: HTMLDivElement): HTMLDivElement {
        const thead = tabla.querySelector('.' + ltrCss.crud.thead);
        const columnas = thead.querySelectorAll<HTMLDivElement>('.' + ltrCss.crud.columna + `:not(.${ltrCss.columnaOculta})`);

        return Array.from(columnas).reverse().find(col => col.offsetWidth > 0);
    }

    export function DefinirCuerpoDeLaTabla(tabla: HTMLDivElement, idGrid: string): HTMLDivElement {
        let cuerpoDeLaTabla: HTMLDivElement = document.createElement("div");
        cuerpoDeLaTabla.id = `${idGrid}_tbody`;
        cuerpoDeLaTabla.classList.add(ltrCss.crud.tbody)
        cuerpoDeLaTabla.classList.add(ltrCss.cuerpoDeLaTabla);
        return cuerpoDeLaTabla;
    }

    export function MarcarElementos(grid: Crud.GridDeDatos, actualizarInfoSelector: boolean): void {
        if (grid.InfoSelector.Cantidad === 0)
            return;

        var celdasId = grid.ChecksDeSeleccion;
        var len = celdasId.length;
        for (var i = grid.InfoSelector.Cantidad - 1; i >= 0; i--) {
            let elemento: Elemento = grid.InfoSelector.LeerElemento(i);
            for (var j = 0; j < len; j++) {
                if (Numero((<HTMLInputElement>celdasId[j]).value) == elemento.Id) {
                    var idCheck = celdasId[j].id.replace(`.${atControl.id}`, `.${ltrMantenimiento.CheckDeSeleccion}`);
                    var check = document.getElementById(idCheck);
                    MarcarFila(<HTMLInputElement>check);
                    if (actualizarInfoSelector) grid.ActualizarInfoSelector(grid, elemento);
                    break;
                }
            }
        }
        grid.InfoSelector.SincronizarCheck();
    }


    function estaOculta(columna: HTMLDivElement) {
        return columna.classList.contains(ltrCss.columnaOculta);
    }

    function hacerVisible(tabla: HTMLDivElement, columna: HTMLDivElement, tamano: string) {
        columna.classList.remove(ltrCss.columnaOculta);
        columna.classList.add(ltrCss.columnaCabecera);
        var partes = SepararCadenaEnNumeroYUnidad(tamano);
        if (partes[0] === 0)
            ResetearWidthAlTh(columna, atGrid.tamanoFijoAlHacerVisible);
        else
            //AplicarEstiloAColumna(tabla, UltimaColumnaVisible(tabla), columna.id, atGrid.tamanoFijoAlHacerVisible);
            AsignarWidthAUnaCelda(columna, atGrid.tamanoFijoAlHacerVisible);

        let cuerpoDeLaTabla: HTMLDivElement = tabla.querySelectorAll('.' + ltrCss.crud.tbody)[0] as HTMLDivElement;
        var tds = cuerpoDeLaTabla.querySelectorAll('.' + ltrCss.crud.celda) as NodeListOf<HTMLDivElement>;
        for (let i = 0; i < tds.length; i++) {
            if (tds[i].getAttribute('headers') === columna.id) {
                tds[i].classList.remove(ltrCss.columnaOculta);
                tds[i].classList.add(ltrCss.columnaCabecera);
                AsignarWidthAUnaCelda(tds[i], atGrid.tamanoFijoAlHacerVisible);
            }
        }
    }

    function hacerInvisible(tabla: HTMLDivElement, columna: HTMLDivElement) {
        columna.classList.add(ltrCss.columnaOculta);
        columna.classList.remove(ltrCss.columnaCabecera);
        let cuerpoDeLaTabla: HTMLDivElement = tabla.querySelectorAll('.' + ltrCss.crud.tbody)[0] as HTMLDivElement;
        var tds = cuerpoDeLaTabla.querySelectorAll('.' + ltrCss.crud.celda) as NodeListOf<HTMLDivElement>;
        for (let i = 0; i < tds.length; i++) {
            if (tds[i].getAttribute('headers') === columna.id) {
                tds[i].classList.add(ltrCss.columnaOculta);
                tds[i].classList.remove(ltrCss.columnaCabecera);
            }
        }
    }


    export function ResetearAnchoDeTabla(tabla: HTMLDivElement, ajustarAltura: boolean): void {
        ResetearAnchoDeCabecera(tabla, ajustarAltura);
        var cabecera = tabla.querySelectorAll('.' + ltrCss.crud.fila)[0] as HTMLDivElement;
        var columnas = cabecera.querySelectorAll('.' + ltrCss.crud.columna) as NodeListOf<HTMLDivElement>;
        let tbody: HTMLTableSectionElement = tabla.querySelector(".div-tbody");
        if (Definido(tbody)) {
            let filas: NodeListOf<HTMLDivElement> = tbody.querySelectorAll(".div-tr") as NodeListOf<HTMLDivElement>;
            for (let i: number = 0; i < filas.length; i++) {
                ResetearFila(columnas, filas[i]);
            }
        }
    }

    function ResetearFila(columnas: NodeListOf<HTMLDivElement>, fila: HTMLDivElement) {
        var celdas = fila.querySelectorAll('.' + ltrCss.crud.celda) as NodeListOf<HTMLDivElement>;
        for (let i: number = 0; i < columnas.length; i++) {
            if (celdas[i].classList.contains(ltrCss.columnaOculta))
                continue;
            AsignarWidthAUnaCelda(celdas[i], columnas[i].style.width);
        }
    }

    function ColumnasVisibles(ths: NodeListOf<HTMLDivElement>): Array<HTMLDivElement> {
        var columnas = new Array<HTMLDivElement>();
        for (let i = 0; i < ths.length; i++) {
            if (ths[i].classList.contains(ltrCss.columnaOculta))
                continue;
            if (ths[i].getAttribute(atControl.propiedad) === ltrMantenimiento.CheckDeSeleccion)
                continue;
            columnas.push(ths[i]);
        }
        return columnas;
    }

    export function AplicarTamanosALaTabla(tabla: HTMLDivElement, tamanos: Array<TamanoDeColumna>) {
        var cabecera = tabla.querySelectorAll('.' + ltrCss.crud.fila)[0] as HTMLDivElement;
        var columnas = cabecera.querySelectorAll('.' + ltrCss.crud.columna) as NodeListOf<HTMLDivElement>;
        for (let columna: number = 0; columna < columnas.length; columna++) {
            var tamano = tamanos.find(x => x.Id == columnas[columna].getAttribute(atControl.propiedad));
            if (Definido(tamano))
                AplicarTamanoAUnaColumna(columnas[columna], tamano);
        }
    }

    function AplicarTamanoAUnaColumna(columna: HTMLDivElement, tamano: TamanoDeColumna) {
        ResetearWidthAUnaColumna(columna, tamano.Tamano);
        ResetearWidthAlTh(columna, tamano.Tamano);
        const celdas = document.querySelectorAll(`td[headers="${columna.id}`) as NodeListOf<HTMLDivElement>;
        for (let fila: number = 0; fila < celdas.length; fila++) {
            AsignarWidthAUnaCelda(celdas[fila], tamano.Tamano);
        }
    }

    function ResetearWidthAUnaColumna(columna: HTMLDivElement, width: string) {
        ResetearWidthAlTh(columna, width);
        const celdas = document.querySelectorAll(`td[headers="${columna.id}`) as NodeListOf<HTMLDivElement>;
        for (let fila: number = 0; fila < celdas.length; fila++) {
            AsignarWidthAUnaCelda(celdas[fila], width);
        }
    }

    export function ResetearWidthAlTh(celdaCabecera: HTMLDivElement, width: string): void {
        if (celdaCabecera.classList.contains(ltrCss.columnaOculta))
            return;
        celdaCabecera.setAttribute(atGrid.tamanoFijo, width);
        AsignarWidthALaCelda(celdaCabecera, width);
    }

    export function AplicarWidthAlBody(tabla: HTMLDivElement) {
        var cabecera = tabla.querySelectorAll('.' + ltrCss.crud.fila)[0] as HTMLDivElement;
        var columnas = cabecera.querySelectorAll('.' + ltrCss.crud.columna);
        let tbody: HTMLDivElement = tabla.querySelector(".div-tbody");
        if (Definido(tbody)) {
            let filas: NodeListOf<HTMLDivElement> = tbody.querySelectorAll(".div-tr") as NodeListOf<HTMLDivElement>;
            for (let i: number = 0; i < filas.length; i++) {
                let celdas: NodeListOf<HTMLDivElement> = filas[i].querySelectorAll(".div-td") as NodeListOf<HTMLDivElement>;
                for (let j: number = 0; j < celdas.length; j++) {
                    AsignarWidthAUnaCelda(celdas[j], (columnas[j] as HTMLDivElement).style.width);
                }
            }
        }
    }


    export function AsignarWidthAUnaCelda(celda: HTMLDivElement, width: string): void {
        if (celda.classList.contains(ltrCss.columnaOculta))
            return;
        AsignarWidthALaCelda(celda, width);
    }

    function AsignarTamanoFijo(columnas: Array<HTMLDivElement>, ajustarAltura: boolean): void {
        var alturaTr = (columnas[0].querySelector('.div-de-columna') as HTMLDivElement).getBoundingClientRect().height;
        var cabecera = columnas[0].parentNode as HTMLTableRowElement;
        var anchoCabecera = cabecera.getBoundingClientRect().width;
        for (let i = 0; i < columnas.length; i++) {
            if (columnas[i].classList.contains(ltrCss.columnaOculta))
                continue;
            let atTamano = columnas[i].getAttribute(atGrid.tamanoFijo);
            if (!Definido(atTamano)) {
                atTamano = columnas[i].style.width;
            }
            let tamano: [number, string] = SepararCadenaEnNumeroYUnidad(atTamano);
            if (tamano[1] === '%') {
                tamano[0] = anchoCabecera * tamano[0] / 100;
                tamano[1] = 'px';
            }
            if (tamano[0] === 0) {
                tamano[0] = SepararCadenaEnNumeroYUnidad(atGrid.tamanoFijoAlHacerVisible)[0];
                tamano[1] = SepararCadenaEnNumeroYUnidad(atGrid.tamanoFijoAlHacerVisible)[1];
            }
            ApiDeGrid.ResetearWidthAlTh(columnas[i], `${tamano[0]}${tamano[1]}`);
            if (!ajustarAltura) continue;
            var anchoCelada = (columnas[i] as HTMLDivElement).getBoundingClientRect().width;
            var div = columnas[i].querySelector('.div-de-columna') as HTMLDivElement;
            let intentos = 0;
            const maxIntentos = 20; // Previene bucles infinitos
            const anchoMaximo = 500; // O cualquier valor máximo razonable
            var altoDelContenedor = div.getBoundingClientRect().height;
            while (altoDelContenedor > alturaTr && intentos < maxIntentos && anchoCelada < anchoMaximo) {
                //console.log(`Alto del div: ${altoDelContenedor}, alto del tr: ${alturaTr}, ancho de la celda ${anchoCelada}`)
                anchoCelada = anchoCelada + 5;
                ApiDeGrid.ResetearWidthAlTh(columnas[i], `${anchoCelada}px`);
                intentos++;
            }
        }
    }

    function AsignarWidthALaCelda(celda: HTMLDivElement, width: string): void {
        celda.style.width = CalcularWidthALaCelda(celda, width);
        celda.style.minWidth = celda.style.width;
        celda.style.maxWidth = celda.style.width;
        celda.style.whiteSpace = 'nowrap';
    }


    export function CalcularWidthALaCelda(celda: HTMLDivElement, width: string): string {
        var divs = celda.querySelectorAll('div') as NodeListOf<HTMLDivElement>;
        var tamanoContenedores = 0
        for (var i = 0; i < divs.length; i++) {
            tamanoContenedores = tamanoContenedores + divs[i].getBoundingClientRect().width;
        }
        var tamanoCelda = SepararCadenaEnNumeroYUnidad(width);
        if (tamanoCelda[1] === 'px' && tamanoCelda[0] < tamanoContenedores) {
            return tamanoContenedores + 'px';
        }
        return width;
    }

    export function ResetearAnchoDeCabecera(tabla: HTMLDivElement, ajustarAltura: boolean): void {
        var cabecera = tabla.querySelectorAll('.' + ltrCss.crud.fila)[0] as HTMLDivElement;
        var ths = cabecera.querySelectorAll('.' + ltrCss.crud.columna) as NodeListOf<HTMLDivElement>;
        let tdsVisibles: Array<HTMLDivElement> = ColumnasVisibles(ths);
        AsignarTamanoFijo(tdsVisibles, ajustarAltura);
    }


    export function CrearCeldaDelTd(idPadre: string, fila: HTMLDivElement, numeroDeCelda: number, columnaCabecera: ApiDeGrid.PropiedadesDeLaFila, esLaTablaDelMnt: boolean) {
        let celdaDelTd: HTMLDivElement = document.createElement("div");
        celdaDelTd.classList.add(ltrCss.crud.celda);
        celdaDelTd.id = `${fila.id}.${numeroDeCelda}`;
        celdaDelTd.setAttribute('headers', `${columnaCabecera.id}`);
        celdaDelTd.setAttribute(atControl.name, `td.${columnaCabecera.propiedad}.${idPadre}`);
        celdaDelTd.setAttribute(atControl.propiedad, `${columnaCabecera.propiedad}`);
        celdaDelTd.style.textAlign = columnaCabecera.estilo.textAlign;
        if (!esLaTablaDelMnt) {
            celdaDelTd.style.width = columnaCabecera.estilo.width;
            celdaDelTd.style.minWidth = columnaCabecera.estilo.minWidth;
        }
        //else
        //    celdaDelTd.style.width = 'auto';
        if (celdaDelTd.style.textAlign == "right")
            celdaDelTd.style.paddingRight = "10px";

        if (columnaCabecera.claseCss.includes(ltrCss.columnaOculta)) {
            celdaDelTd.classList.add(ltrCss.columnaOculta);
        }
        return celdaDelTd;
    }

    export function CrearInputDeLaCelda(idPadre: string, idFila: string, columnaCabecera: ApiDeGrid.PropiedadesDeLaFila, celdaDelTd: HTMLDivElement, valor: any) {
        let input = document.createElement("input");
        input.type = "text";
        input.id = `${idFila}.${columnaCabecera.propiedad}`;
        input.name = `${columnaCabecera.propiedad}.${idPadre}`;
        input.setAttribute(atControl.propiedad, columnaCabecera.propiedad);

        input.style.border = "0px";
        input.style.textAlign = columnaCabecera.estilo.textAlign;
        input.style.width = "100%";
        input.style.backgroundColor = "inherit";

        input.classList.add('input-centrado-verticalmente')



        input.readOnly = true;
        input.hidden = celdaDelTd.hidden;

        mapearValor(input, valor, columnaCabecera);
        return input;
    }

    export function CrearCirculoEnCelda(idPadre: string, idFila: string, columnaCabecera: ApiDeGrid.PropiedadesDeLaFila, celdaDelTd: HTMLDivElement, valor: any): HTMLDivElement {
        let div: HTMLDivElement = document.createElement("div");
        div.id = `${idFila}.${columnaCabecera.propiedad}`;
        div.setAttribute(atControl.name, `${columnaCabecera.propiedad}.${idPadre}`)
        div.setAttribute(atControl.propiedad, columnaCabecera.propiedad);
        div.hidden = celdaDelTd.hidden;
        var csss = valor.split(ltrSimbolos.separadorDeCss);
        for (let i = 0; i < csss.length; i++) div.classList.add(csss[i]);
        return div;
    }

    function mapearValor(input: HTMLInputElement, valor: any, columnaCabecera: ApiDeGrid.PropiedadesDeLaFila) {

        if (columnaCabecera.esFecha) {
            if (IsNullOrEmpty(columnaCabecera.formato)) {
                input.value = new Date(valor).toLocaleString('es-ES');
            }
            else {
                input.value = FormatearFecha(new Date(valor), columnaCabecera.formato);
            }
        }
        else if (EsNumero(valor) && !IsNullOrEmpty(columnaCabecera.formato)) {
            input.value = FormatearNumero(Numero(valor), columnaCabecera.formato);
        }
        else
            if (valor instanceof Array) {
                let v: string = "";
                for (let i: number = 0; i < valor.length; i++) {
                    v = IsNullOrEmpty(v) ? v : v + ' | ' + valor[i];
                }
                input.value = v;
            }
            else
                input.value = NoDefinido(valor) || EsNumero(valor) || EsBool(valor) ? valor : valor.replace(/\r?\n|\r/g, " | ");
    }


    export function Expansor_OcultarPorId(idGridDeDetalle: string): HTMLDivElement {
        var grid = document.getElementById(idGridDeDetalle) as HTMLDivElement;
        Expansor_OcultarSpan(grid);
        return grid;
    }

    export function Expansor_OcultarSpan(gridDeDetalle: HTMLDivElement) {
        let espan: HTMLDivElement = gridDeDetalle.parentElement.parentElement.parentElement as HTMLDivElement;
        ApiPanel.OcultarPanel(espan);
    }

    export function Expansor_MostrarPorId(idGridDeDetalle: string): HTMLDivElement {
        var grid = document.getElementById(idGridDeDetalle) as HTMLDivElement;
        Expansor_MostrarSpan(grid);
        return grid;
    }

    export function Expansor_MostrarSpan(gridDeDetalle: HTMLDivElement) {
        let espan: HTMLDivElement = gridDeDetalle.parentElement.parentElement.parentElement as HTMLDivElement;
        ApiPanel.MostrarPanel(espan);
    }

    export function Expansor_ObtenerPropiedadDeLaFila(idGridDeDetalle: string, numeroFila: number, propiedad: string): string {
        let idDeLaFila = `${idGridDeDetalle.replace('-contenedor', '-tabla_d_tr')}_${numeroFila}`;
        let valor: string = '';
        try {
            valor = ApiDeExpansor.ObtenerElValorDeLaPropiedadDeLaFila(idDeLaFila, propiedad);
            if (NoDefinido(valor)) {
                throw new Error(`No se ha localizado la propiedad ${propiedad} de la fila ${idDeLaFila} en el grid ${idGridDeDetalle}`);
            }
        }
        catch (error) {
            MensajesSe.EmitirExcepcion("Obtener coluna del grid de realción", 'No se ha podido obtener el Id de la relación', error);
        }
        return valor;
    }

    export function Expansor_PonerEnConsulta(idGrid) {
        let grid: HTMLDivElement = document.getElementById(idGrid) as HTMLDivElement;
        let tbody = grid.querySelector('.' + ltrCss.crud.tbody);
        let tablarows = tbody.querySelectorAll<HTMLDivElement>('.' + ltrCss.crud.fila);
        if (!Definido(tbody)) return
        for (var i = 0; i < tablarows.length; i++) {
            let fila: HTMLDivElement = tablarows[i];
            let filacells = fila.querySelectorAll<HTMLDivElement>('.' + ltrCss.crud.celda);
            for (var j = 0; j < filacells.length; j++) {
                let columna: HTMLDivElement = filacells[j];
                if (columna.getAttribute(atControl.propiedad) === ltrMenus.BarraDeMenu.Borrar) {
                    var link = columna.querySelector("a");
                    if (Definido(link)) {
                        link.href = "javascript:void(0)";
                        link.innerHTML = ''
                    }
                }
                if (columna.getAttribute(atControl.propiedad) === ltrMenus.BarraDeMenu.Editar) {
                    var link = columna.querySelector("a");
                    link.innerHTML = 'Consultar'
                }

            }
        }
    }

    export function Expansor_PonerEnEdicion(idGrid) {
        let grid: HTMLDivElement = document.getElementById(idGrid) as HTMLDivElement;
        let tbody = grid.querySelector('.' + ltrCss.crud.tbody);
        if (Definido(tbody) === false)
            return;
        let tablarows = tbody.querySelectorAll<HTMLDivElement>('.' + ltrCss.crud.fila);
        for (var i = 0; i < tablarows.length; i++) {
            let fila: HTMLDivElement = tablarows[i];
            let filacells = fila.querySelectorAll<HTMLDivElement>('.' + ltrCss.crud.celda);
            for (var j = 0; j < filacells.length; j++) {
                let columna: HTMLDivElement = filacells[j];
                if (columna.getAttribute(atControl.propiedad) === ltrMenus.BarraDeMenu.Borrar) {
                    var link = columna.querySelector("a");
                    if (Definido(link)) {
                        link.href = `javascript:Crud.EventosDeExpansores('${ltrEventos.Expansores.BorrarRelacion}', '${grid.id};${i}')`;
                        link.innerHTML = ltrMenus.BarraDeMenu.Borrar;
                    }
                }
                if (columna.getAttribute(atControl.propiedad) === ltrMenus.BarraDeMenu.Editar) {
                    var link = columna.querySelector("a");
                    link.innerHTML = 'Editar';
                }

            }
        }
    }

    export function MarcarFila(check: HTMLInputElement): void {
        check.checked = true;
        var fila = ApiControl.BuscarFila(check);
        fila.classList.add(ltrCss.filaSeleccionada);
    }

    export function DesmarcarFila(check: HTMLInputElement): void {
        check.checked = false;
        var fila = ApiControl.BuscarFila(check);
        fila.classList.remove(ltrCss.filaSeleccionada);
    }

    export function ObtenerValorDeLaFilaParaLaPropiedad(tabla: HTMLDivElement, numeroDeFila: number, propiedad: string): string {
        var filas: NodeListOf<HTMLDivElement> = tabla.querySelectorAll('.' + ltrCss.crud.fila) as NodeListOf<HTMLDivElement>;
        let fila: HTMLDivElement = filas[numeroDeFila + 1] as HTMLDivElement;
        if (fila === null)
            throw Error(`la fila indicada '${numeroDeFila}' no está en la tabla, la tabla tiene  '${filas.length - 1}' filas`);

        let celda: HTMLDivElement = ApiDeGrid.ObtenerCelda(fila, propiedad.toLowerCase());
        let input: HTMLInputElement = celda.querySelector("input");
        if (input === null)
            throw Error(`la celda asociada a la propiedad '${propiedad}' no tiene un control input definido`);

        return input.value;
    }

    export function ObtenerCelda(fila: HTMLDivElement, propiedadBuscada: string): HTMLDivElement {
        var celdas = fila.querySelectorAll('.' + ltrCss.crud.celda) as NodeListOf<HTMLDivElement>;
        for (var j = 0; j < celdas.length; j++) {
            let celda: HTMLDivElement = celdas[j];
            let propiedadCelda: string = celda.getAttribute(atControl.propiedad);
            if (propiedadCelda.toLocaleLowerCase() === propiedadBuscada)
                return celda;
        }
        throw Error(`No se ha localizado una celda con la propiedad '${propiedadBuscada}' definida`);
    }

    export function ObtenerColumna(fila: HTMLDivElement, propiedadBuscada: string): HTMLDivElement {
        var celdas = fila.querySelectorAll('.' + ltrCss.crud.columna) as NodeListOf<HTMLDivElement>;
        for (var j = 0; j < celdas.length; j++) {
            let celda: HTMLDivElement = celdas[j];
            let propiedadCelda: string = celda.getAttribute(atControl.propiedad);
            if (propiedadCelda.toLocaleLowerCase() === propiedadBuscada)
                return celda;
        }
        throw Error(`No se ha localizado una celda con la propiedad '${propiedadBuscada}' definida`);
    }

    export function AplicarDisposicionDelEncolumnado(tabla: HTMLDivElement, disposicionDeColumnas: DisposicionDeColumna[]): void {
        // Obtener referencias a los th y td
        const ths = Array.from(tabla.querySelectorAll<HTMLDivElement>('.' + ltrCss.crud.columna));

        // Ordenar las columnas del thead
        //const columnasConPropiedad = ths.sort((a, b) => {
        //    const disposicionA = disposicionDeColumnas.find(d => d.Id === a.id)!;
        //    const disposicionB = disposicionDeColumnas.find(d => d.Id === b.id)!;
        //    return disposicionA.Posicion - disposicionB.Posicion;
        //});

        // Ordenar las columnas del thead
        const columnasConPropiedad: HTMLDivElement[] = [];

        for (let i = 0; i < ths.length; i++) {
            let posicionMenor = i;
            for (let j = i + 1; j < ths.length; j++) {
                const disposicionA = disposicionDeColumnas.find(d => d.Id === ths[posicionMenor].id);
                const disposicionB = disposicionDeColumnas.find(d => d.Id === ths[j].id);

                if (disposicionA && disposicionB && disposicionA.Posicion > disposicionB.Posicion) {
                    posicionMenor = j;
                }
            }

            if (posicionMenor !== i) {
                const temp = ths[i];
                ths[i] = ths[posicionMenor];
                ths[posicionMenor] = temp;
            }

            columnasConPropiedad.push(ths[i]);
        }



        // Crear un nuevo thead en el orden deseado
        const nuevaFila = document.createElement('div');
        nuevaFila.classList.add(ltrCss.crud.fila);
        columnasConPropiedad.forEach(th => {
            nuevaFila.appendChild(th);
        });
        const thead = tabla.querySelector('.' + ltrCss.crud.thead);
        thead!.innerHTML = '';
        thead!.appendChild(nuevaFila);

        // Eliminar las filas del tbody existente
        const tbody = tabla.querySelector<HTMLDivElement>('.' + ltrCss.crud.tbody);
        const filasBody = Array.from(tbody!.querySelectorAll<HTMLDivElement>('.' + ltrCss.crud.fila));
        filasBody.forEach(fila => fila.remove());

        // Crear un nuevo tbody en el orden deseado
        filasBody.forEach(fila => {
            const nuevaFila = document.createElement('div');
            const celdas = Array.from(fila.querySelectorAll<HTMLDivElement>('.' + ltrCss.crud.celda));
            celdas.sort((a, b) => {
                const disposicionA = disposicionDeColumnas.find(d => d.Propiedad === a.getAttribute('propiedad'))!;
                const disposicionB = disposicionDeColumnas.find(d => d.Propiedad === b.getAttribute('propiedad'))!;
                return disposicionA.Posicion - disposicionB.Posicion;
            });
            celdas.forEach(td => {
                const disposicionColumna = disposicionDeColumnas.find(d => d.Propiedad === td.getAttribute('propiedad'));
                if (disposicionColumna) {
                    nuevaFila.appendChild(td);
                }
            });
            tbody!.appendChild(nuevaFila);
        });
    }

    export function AplicarEstiloAColumna(tabla: HTMLDivElement, ultimaColumnaVisible: HTMLDivElement, idColumna: string, width: string): void {

        const estilo = document.createElement('style');

        estilo.textContent = `
            div[id="${idColumna}"], div[headers="${idColumna}"] {
            width: ${width} !important;
            max-width: ${width} !important;
            min-width: ${width} !important;
          }
        `;

        const estiloFit = document.createElement('style');
        estiloFit.textContent = `
            div[id="${ultimaColumnaVisible.id}"], div[headers="${ultimaColumnaVisible.id}"] {
            width: fit-content !important;
            max-width: fit-content !important;
            min-width: fit-content !important;
          }
        `;

        // Buscar el estilo existente y reemplazarlo
        const estilosExistentes = document.querySelectorAll('style');
        let remplazadoEstilo = false;
        let remplazadoFit = false;
        for (let i = 0; i < estilosExistentes.length; i++) {
            if (!Definido(estilosExistentes[i].parentElement))
                continue;
            if (estilosExistentes[i].textContent.includes(`div[id="${idColumna}"]`) || estilosExistentes[i].textContent.includes(`div[headers="${idColumna}"]`)) {
                estilosExistentes[i].parentElement!.replaceChild(estilo, estilosExistentes[i]);
                remplazadoEstilo = true;
            }
            if (!remplazadoEstilo && (estilosExistentes[i].textContent.includes(`div[id="${ultimaColumnaVisible.id}"]`) || estilosExistentes[i].textContent.includes(`div[headers="${ultimaColumnaVisible.id}"]`))) {
                estilosExistentes[i].parentElement!.replaceChild(estiloFit, estilosExistentes[i]);
                remplazadoFit = true;
            }
            if (remplazadoEstilo && remplazadoFit) break;
        }
        if (!remplazadoEstilo) document.head.appendChild(estilo);
        if (!remplazadoFit) document.head.appendChild(estiloFit);

    }

    export function AsignarAncho(div: HTMLDivElement, width: string) {
        setTimeout(() => {
            div.style.cssText += `
            width: ${width} !important;
            max-width: ${width} !important;
            min-width: ${width} !important;
            box-sizing: border-box !important;
            flex-basis: ${width} !important;
            flex-grow: 0 !important;
            flex-shrink: 0 !important;
        `;
            div.setAttribute(atGrid.tamanoFijo, width);
        }, 0);
    }

    export function ModificarTamano(tabla: HTMLDivElement, columna: HTMLDivElement, desplazamientoX: number, reajustar: boolean = true) {
        const thId = columna.id;
        const anchoActual = columna.offsetWidth;
        const nuevoAncho = anchoActual + desplazamientoX;
        if (desplazamientoX === 0) return
        if (nuevoAncho <= 32) return;

        // Ajustar el ancho del th
        ApiDeGrid.AsignarAncho(columna, nuevoAncho + 'px');

        // Ajustar el ancho de todas las celdas asociadas
        const celdas = document.querySelectorAll(`.${ltrCss.crud.celda}[headers="${thId}"]`) as NodeListOf<HTMLDivElement>;
        for (let i = 0; i < celdas.length; i++) {
            ApiDeGrid.AsignarAncho(celdas[i], nuevoAncho + 'px');
        }

        if (reajustar) ReajustarUltimaColumna(tabla);
    }

    export function ReajustarUltimaColumna(tabla: HTMLDivElement) {
        setTimeout(() => {
            const contenedorTabla = document.getElementById(tabla.id.replace('_table', ''));
            const tamanoDelContenedor = contenedorTabla?.getBoundingClientRect().width || 0;
            const anchoTablaActual = tabla.getBoundingClientRect().width;

            if (anchoTablaActual < tamanoDelContenedor) {
                const diferenciaAncho = tamanoDelContenedor - anchoTablaActual;
                const ultimaColumnaVisible = ApiDeGrid.UltimaColumnaVisible(tabla);

                if (ultimaColumnaVisible) {
                    ApiDeGrid.ModificarTamano(tabla, ultimaColumnaVisible as HTMLDivElement, diferenciaAncho - 24, false);
                }
            }
        }, 0);
    }


}

