namespace MapearAlControl {

    export function MapearImagenes(elementoJson: JSON, visorVinculado: string): void {
        let visor: HTMLImageElement = document.getElementById(visorVinculado) as HTMLImageElement;
        let propiedadDelVisor: string = visor.getAttribute(atControl.propiedad);
        let url: string = Json_BuscarValorEn(propiedadDelVisor, elementoJson) as string;
        const estaElModuloCargado = typeof Crud !== 'undefined';

        if (estaElModuloCargado && Definido(Crud.Consultor)) {
            const urlObj = new URL(url, window.location.origin);
            const negocio = urlObj.searchParams.get('negocio');
            const idElemento = urlObj.searchParams.get('idElemento');
            const idArchivo = urlObj.searchParams.get('idArchivo');
            url = `/${ltrControladores.SisDoc.Archivos}/${Ajax.Archivos.accion.DescargarPorGuid}?negocio=${encodeURIComponent(negocio)}&idElemento=${idElemento}&idArchivo=${idArchivo}&guid=${Crud.Consultor.GuidDeConsulta}`;
        }

        MapearAlControl.MapearUrlALaImagenYCambas(visor, url);
    }

    export function ImagenUrl(control: HTMLInputElement, imagen: string): void {
        let visorVinculado: string = control.getAttribute(atArchivo.imagen);
        let ruta: string = control.getAttribute(atArchivo.rutaDestino);
        let visor: HTMLImageElement = document.getElementById(visorVinculado) as HTMLImageElement;
        control.setAttribute(atArchivo.nombre, imagen);
        MapearAlControl.MapearUrlALaImagenYCambas(visor, `${ruta}/${imagen}`);
    }

    export function MapearUrlALaImagenYCambas(visor: HTMLImageElement, url: any) {
        visor.setAttribute('src', url);
        let idCanva: string = visor.getAttribute(atControl.id).replace('img', 'canvas');
        let htmlCanvas: HTMLCanvasElement = document.getElementById(idCanva) as HTMLCanvasElement;
        Canva(htmlCanvas, 100, 100, url);
    }

    export function Imagen(imagen: HTMLImageElement, url: string) {
        imagen.setAttribute('src', url);
    }

    export function Canva(canvas: HTMLCanvasElement, ancho: number, alto: number, url: string) {

        function cargarImagenPorDefecto(e) {
            e.target.src = '/images/menu/not-found.svg';
        }

        canvas.width = ancho;
        canvas.height = alto;
        var canvas2d = canvas.getContext('2d');
        var img = new Image();
        var err = new Image();
        err.src = "/images/menu/not-found.svg";
        img.src = url;
        img.onload = function () {
            canvas2d.drawImage(img, 0, 0, 100, 100);
        };
        img.onerror = cargarImagenPorDefecto;
    }

    export function Texto(area: HTMLTextAreaElement, texto: string): void {
        area.textContent = texto;
    }

    export function RestrictoresDeEdicion(panel: HTMLDivElement, propiedad: string, id: number, texto: string) {
        let restrictores: NodeListOf<HTMLInputElement> = panel.querySelectorAll(`input[${atControl.tipo}="${ltrTipoControl.restrictorDeEdicion}"]`) as NodeListOf<HTMLInputElement>;

        for (let i = 0; i < restrictores.length; i++) {
            if (restrictores[i].getAttribute(atControl.propiedad) === propiedad.toLocaleLowerCase()) {
                Restrictor(restrictores[i], id, texto);
            }
        }
    }

    export function Check(panel: HTMLDivElement, propiedad: string, activo: boolean, bloqueada: boolean) {
        let controles: NodeListOf<HTMLInputElement> = panel.querySelectorAll(`input[${atControl.propiedad}="${propiedad.toLocaleLowerCase()}"]`) as NodeListOf<HTMLInputElement>;

        if (controles.length > 1)
            MensajesSe.EmitirMensajeDeExcepcion("Mapeo de propiedad", `Hay más de un check con la propiedad ${propiedad} en el panel ${panel.id}`);
        if (controles.length === 0)
            MensajesSe.EmitirMensajeDeExcepcion("Mapeo de propiedad", `No existe check ${propiedad} en el panel ${panel.id}`);

        var valorAnterior = controles[0].checked;
        controles[0].checked = activo;
        ApiControl.BloquearCheck(controles[0], bloqueada);
        if (valorAnterior !== activo) {
            var event = new Event('change');
            controles[0].dispatchEvent(event);
        }
    }

    export function Propiedad(panel: HTMLDivElement, propiedad: string, id: number, texto: string, esRestrictor: boolean, asignarPorDefecto: boolean, emitirExcepcion: boolean = true): HTMLElement {
        let controles: NodeListOf<HTMLElement> = panel.querySelectorAll(`[${atControl.propiedad}="${propiedad.toLocaleLowerCase()}"]`) as NodeListOf<HTMLElement>;

        if (controles.length > 1) {
            if (emitirExcepcion)
                MensajesSe.EmitirMensajeDeExcepcion("Mapeo de propiedad", `Hay más de un control con la propiedad ${propiedad} en el panel ${panel.id}`);
            return undefined;
        }

        if (controles.length === 0) {
            if (emitirExcepcion)
                MensajesSe.EmitirMensajeDeExcepcion("Mapeo de propiedad", `No existe la propiedad ${propiedad} en el panel ${panel.id}`);
            return undefined;
        }

        return MapearValores(controles[0], id, texto, esRestrictor, asignarPorDefecto) as HTMLElement;
    }

    export function MapearValores(control: HTMLElement, id: number, texto: string, esRestrictor: boolean, asignarPorDefecto: boolean): HTMLElement {
        let tipo: string = control.getAttribute(atControl.tipo);
        return mapearPropiedad(tipo, control, id, texto, esRestrictor, asignarPorDefecto) as HTMLElement;
    }

    export function MapearEditor(input: HTMLInputElement, id: number, texto: string, bloqueado: boolean, asignarPorDefecto: boolean) {
        if (id == -1 || NoDefinido(id))
            AsignarValor(input, texto);
        else
            FijarValorEnEditor(input, id, texto, bloqueado, asignarPorDefecto);
    }

    export function MapearAreaDeTexto(areaDeTexto: HTMLTextAreaElement, texto: string, bloquear: boolean) {
        areaDeTexto.value = texto;
        if (bloquear)
            ApiControl.BloquearAreaDeTexto(areaDeTexto);

        if (ApiControl.EstaEnUnaModal(areaDeTexto))
            return;

        ajustarAltura(areaDeTexto);
        areaDeTexto.addEventListener('input', () => ajustarAltura(areaDeTexto));
        window.addEventListener('resize', () => ajustarAltura(areaDeTexto));
    }

    function ajustarAltura(elemento: HTMLTextAreaElement): void {
        // Restablecer la altura a auto para obtener el scrollHeight correcto
        elemento.style.height = 'auto';

        // Calcular la altura máxima (70% del alto de la ventana)
        const maxHeight = window.innerHeight * 0.7;

        // Establecer una altura mínima (ajusta según tus necesidades)
        const minHeight = 50;

        // Calcular la nueva altura
        const newHeight = Math.min(Math.max(elemento.scrollHeight, minHeight), maxHeight);

        // Aplicar la nueva altura
        elemento.style.height = `${newHeight}px`;

        // Si el contenido excede la altura máxima, mostrar la barra de desplazamiento
        elemento.style.overflowY = elemento.scrollHeight > maxHeight ? 'auto' : 'hidden';
    }

    export function MapearSelectorDeFecha(control: HTMLInputElement, fecha: string): void {
        if (!IsNullOrEmpty(fecha)) {
            MapearAlControl.Fecha(control, fecha);
            let tipo: string = control.getAttribute(atControl.tipo);
            if (tipo === ltrTipoControl.SelectorDeFechaHora) {
                MapearAlControl.Hora(control, fecha);
            }
        }
        else
            ApiControl.BlanquearFecha(control);
    }

    export function MapearReferencias(panel: HTMLDivElement, objeto: any, modoDeAcceso: ModoAcceso.enumModoDeAccesoDeDatos): void {
        let referencias: NodeListOf<HTMLAnchorElement> = panel.querySelectorAll(`a[${atControl.tipo}='${ltrTipoControl.Referencia}']`) as NodeListOf<HTMLAnchorElement>;
        for (var i = 0; i < referencias.length; i++) {
            MapearReferencia(referencias[i], objeto, modoDeAcceso);
        }
    }

    export function MapearReferenciasPost(panel: HTMLDivElement, objeto: any, modoDeAcceso: ModoAcceso.enumModoDeAccesoDeDatos): void {
        let referencias: NodeListOf<HTMLInputElement> = panel.querySelectorAll(`a[${atControl.tipo}='${ltrTipoControl.ReferenciaPost}']`) as NodeListOf<HTMLInputElement>;
        for (var i = 0; i < referencias.length; i++) {
            MapearReferenciaPost(referencias[i], objeto, modoDeAcceso);
        }
    }

    export function MapearLinks(panel: HTMLDivElement, objeto: any, modoDeAcceso: ModoAcceso.enumModoDeAccesoDeDatos): void {
        let link: NodeListOf<HTMLAnchorElement> = panel.querySelectorAll(`a[${atControl.tipo}='${ltrTipoControl.Link}']`) as NodeListOf<HTMLAnchorElement>;
        for (var i = 0; i < link.length; i++) {
            RedefinirLink(link[i]);
        }
    }

    export function MapearReferenciaPost(ref: HTMLInputElement, objeto: any, modoDeAcceso: ModoAcceso.enumModoDeAccesoDeDatos) {
        let accion = ref.getAttribute(atControl.valorPorDefecto);
        if (NoDefinido(accion))
            MensajesSe.Info(`la referencia post ${ref.id} no tiene definido la acción inicial`);
        else {
            ref.setAttribute(atControl.eventoJs.onclick, SustituirPropiedades(accion, objeto));
            if (!ModoAcceso.EsGestor(modoDeAcceso)) {
                ref.style.display = EsTrue(ref.getAttribute(atRef.enConsultaOcultar)) ? ltrStyle.display.none : ltrStyle.display.block;
            }
        }
    }

    export function MapearReferencia(ref: HTMLAnchorElement, objeto: any, modoDeAcceso: ModoAcceso.enumModoDeAccesoDeDatos) {
        let accion = ref.getAttribute(atControl.valorPorDefecto);
        if (NoDefinido(accion)) {
            let url = ObtenerPropiedad(objeto, ref.getAttribute(atControl.propiedad));
            if (EsUrl(url))
                ref.href = url;
            else
                MensajesSe.Info(`la referencia ${ref.id} no tiene definido la acción inicial`);
        }
        else {
            ref.href = SustituirPropiedades(accion, objeto);
            if (!ModoAcceso.EsGestor(modoDeAcceso)) {
                var ocultar = EsTrue(ref.getAttribute(atRef.enConsultaOcultar)) ? true : false;
                ApiControl.OcultarHtmlAnchor(ref, ocultar);
                // ref.style.display = EsTrue(ref.getAttribute(atRef.enConsultaOcultar)) ? ltrStyle.display.none : ltrStyle.display.block;
            }
        }

        const nombreAccion = ObtenerPropiedad(objeto, atControl.NombreDeAccion);
        if (Definido(nombreAccion))
            ref.innerText = nombreAccion;
    }

    export function Restrictor(restrictor: HTMLInputElement, id: number, texto: string, bloquear: boolean = false): void {
        restrictor.setAttribute(atControl.valorInput, texto);
        restrictor.setAttribute(atControl.restrictor, id.toString());
        restrictor.value = texto;
        ApiListaDinamica.BlanquearDependientes(restrictor);
        DefinirNavegador(restrictor, id);
        if (bloquear)
            ApiControl.BloquearInput(restrictor);
    }

    export function ListaDeElementos(lista: HTMLSelectElement, filtros: Array<ClausulaDeFiltrado>, idQueHayQueSeleccionar: number, trasMapear: Function = null, restringirPor: ClausulaDeFiltrado[] = null): void {

        if (EsTrue(lista.getAttribute(atListasDeElemento.Cargando)) && idQueHayQueSeleccionar === 0)
            return;

        if (EsTrue(lista.getAttribute(atListasDeElemento.Cargando)) && idQueHayQueSeleccionar >= 0) {
            setTimeout(() => ListaDeElementos(lista, filtros, idQueHayQueSeleccionar), 500);
            return;
        }

        let controlador: string = lista.getAttribute(atControl.controlador);
        let claseElemento: string = lista.getAttribute(atListas.claseElemento);
        let datosDeEntrada: string = `{"ClaseDeElemento":"${claseElemento}", "IdLista":"${lista.id}", "IdFijar":${idQueHayQueSeleccionar}}`;

        if (EsTrue(lista.getAttribute(atListas.yaCargado))) {

            if (idQueHayQueSeleccionar == 0 && lista.selectedIndex > 0)
                return;

            var fijado = fijarValorEnLista(lista, idQueHayQueSeleccionar);
            if (!fijado && idQueHayQueSeleccionar > 0 && ApiControl.EstaBloqueadaLaLista(lista)) {
                ApiDePeticiones.LeerElementoPorId(lista, controlador, idQueHayQueSeleccionar, new Array<Parametro>(), datosDeEntrada, null).
                    then((peticion) => FijarEnLaListaElElementoLeidoPorId(peticion)).
                    catch((peticion) => ApiDePeticiones.EmitirError(peticion));
            }
            return;
        }

        let restrictoresDefinidos: Array<ClausulaDeFiltrado> = ApiDeFiltro.RestringirPorOtrosControles((lista as HTMLSelectElement));
        for (let i = 0; i < restrictoresDefinidos.length; i++) {
            const restrictor = restrictoresDefinidos[i];
            if (Definido(restrictor) && Numero(restrictor.valor) === 0) {
                setTimeout(() => ListaDeElementos(lista, filtros, idQueHayQueSeleccionar), 500);
                return;
            }
        }

        let restrictoresFijos = lista.getAttribute(atListasDeElemento.RestrictorFijo);
        if (Definido(restrictoresFijos)) {

            var restrictores = restrictoresFijos.split(ltrSimbolos.separadorDeValores);
            for (var i = 0; i < restrictores.length; i++) {
                let partes = restrictores[i].split(';');
                if (partes.length === 2) {
                    let restrictorFijo: ClausulaDeFiltrado = new ClausulaDeFiltrado(partes[0], atCriterio.igual, partes[1]);
                    filtros.push(restrictorFijo);
                }
                else if (partes.length === 1) {
                    let restrictorFijo: ClausulaDeFiltrado = ObtenerRestrictorDeLaVista(lista, partes[0]);
                    filtros.push(restrictorFijo);
                }
            }
        }

        for (let i = 0; i < restrictoresDefinidos.length; i++) {
            const restrictor = restrictoresDefinidos[i];
            if ((EsNumero(restrictor.valor) && Numero(restrictor.valor) > 0) || (restrictor.valor.length > 0))
                filtros.push(restrictor);
        }

        if (Definido(restringirPor)) {
            for (let rp of restringirPor) {
                let filtroCoincidente = filtros.find(filtro => filtro.clausula === rp.clausula);
                if (filtroCoincidente) {
                    filtros = filtros.filter(filtro => filtro !== filtroCoincidente);
                    filtros.push(rp);
                } else {
                    filtros.push(rp);
                }
            }
        }

        if (IsNullOrEmpty(controlador))
            MensajesSe.EmitirExcepcion("MapearAlControl.Lista", `Error al cargar Lista de elementos de ${claseElemento}, no está definido el controlador`);

        let parametros: Array<Parametro> = new Array<Parametro>();
        let idNegocio: number = Numero(lista.getAttribute(atListasDeElemento.idNegocio));
        if (idNegocio > 0) parametros.push(new Parametro(Ajax.Param.idNegocio, idNegocio));

        parametros.push(new Parametro(Ajax.Param.aplicarJoin, false));
        parametros.push(new Parametro(Ajax.Param.cantidad, -1));
        parametros.push(new Parametro(Ajax.Param.obtenerSeguridad, false));
        parametros.push(new Parametro(Ajax.Param.cargarListaDeElementos, true));

        lista.setAttribute(atListasDeElemento.Cargando, "S");
        const estaElModuloCargado = typeof Crud !== 'undefined';
        if (estaElModuloCargado && Definido(Crud.crudMnt)) {
            parametros.push(new Parametro(Ajax.Param.modo, Crud.crudMnt.EstoyEnMantenimiento ? enumModoTrabajo.mantenimiento :
                Crud.crudMnt.EstoyCreando ? enumModoTrabajo.creando :
                    Crud.crudMnt.EstoyConsultando ? enumModoTrabajo.consultando :
                        Crud.crudMnt.EstoyEditando ?
                            (ModoAcceso.EsGestor(Crud.crudMnt.crudDeEdicion.ModoDeAcceso) ? enumModoTrabajo.editando : ModoAcceso.EsConsultor(Crud.crudMnt.crudDeEdicion.ModoDeAcceso) ? enumModoTrabajo.consultando : '') :
                            ''));
        }

        ApiDePeticiones.LeerElementos(lista, controlador, Ajax.EndPoint.LeerElementos, filtros, parametros, datosDeEntrada)
            .then((peticion) => {
                var fijado = MapearAlControl.MapearElementosEnLista(peticion);
                if (!fijado && idQueHayQueSeleccionar > 0 && ApiControl.EstaBloqueadaLaLista(lista)) {
                    ApiDePeticiones.LeerElementoPorId(lista, controlador, idQueHayQueSeleccionar, new Array<Parametro>(), datosDeEntrada, null)
                        .then((peticion) => {
                            FijarEnLaListaElElementoLeidoPorId(peticion);
                            if (Definido(trasMapear)) trasMapear();
                        })
                        .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
                }
                else if (Definido(trasMapear)) trasMapear();
                lista.setAttribute(atListasDeElemento.yaCargado, "S");
            })
            .catch((peticion) => {
                lista.setAttribute(atListasDeElemento.yaCargado, "N");
                ApiDePeticiones.EmitirError(peticion);
            })
            .finally(() => {
                lista.setAttribute(atListasDeElemento.Cargando, "N");
            });
    }

    function ObtenerRestrictorDeLaVista(lista: HTMLSelectElement, propiedad: string): ClausulaDeFiltrado {
        var contenedor = ApiPanel.BuscarModalContenedora(lista);
        if (!Definido(contenedor))
            contenedor = ApiPanel.BuscarContenedorDeTablaDto(lista);
        if (!Definido(contenedor))
            MensajesSe.EmitirExcepcion('MapearAlControl.ListaDeElementos', `No se ha localizado el contenedor para el control '${lista.id}'`)

        var control = ApiControl.BuscarControl(contenedor, propiedad, false);
        let restrictorFijo: ClausulaDeFiltrado = undefined;
        if (Definido(control)) {
            var tipo = control.getAttribute(atControl.tipo);
            if (tipo === ltrTipoControl.restrictorDeEdicion)
                restrictorFijo = new ClausulaDeFiltrado(propiedad, atCriterio.igual, Numero(control.getAttribute(atRestrictor.idRestrictor)));
            else if (tipo === ltrTipoControl.ListaDinamica)
                restrictorFijo = new ClausulaDeFiltrado(propiedad, atCriterio.igual, Numero(control.getAttribute(atListasDinamicas.idSeleccionado)));
            else if (tipo === ltrTipoControl.Editor)
                restrictorFijo = new ClausulaDeFiltrado(propiedad, atCriterio.igual, Numero(control.innerText));
            else
                MensajesSe.EmitirExcepcion('MapearAlControl.ListaDeElementos', `No se ha definido como mapear el restrictor fijo '${propiedad}' del tipo '${tipo}'`)
        }
        else {
            const tablaEdicion = contenedor.querySelector('.' + ltrCss.crud.tabla);
            if (tablaEdicion)
                restrictorFijo = new ClausulaDeFiltrado(propiedad, atCriterio.igual, Numero(tablaEdicion.getAttribute(atListasDeElemento.idNegocio)));
            else
                MensajesSe.EmitirExcepcion('MapearAlControl.ListaDeElementos', `No se ha localizado la tabla en el contenedor '${contenedor.id}' para buscar la propiedad '${atListasDeElemento.idNegocio}'`)
        }
        return restrictorFijo;
    }

    export function FijarEnListaDeElementos(lista: HTMLSelectElement, id: number): boolean {

        if (EsTrue(lista.getAttribute(atListas.yaCargado))) {
            fijarValorEnLista(lista, id);
            return true;
        }
        return false;
    }

    export function MapearElementosEnLista(peticion: ApiDeAjax.DescriptorAjax): boolean {
        let datosDeEntrada: Tipos.DatosPeticionLista = JSON.parse(peticion.DatosDeEntrada);
        return MapearElementosEnLaLista(datosDeEntrada.IdLista, datosDeEntrada.IdFijar, peticion.resultado.datos);
    }


    export function MapearElementosEnLaLista(idLista: string, idFijar: number, valores: any): boolean {
        let lista = new Tipos.ListaDeElemento(idLista);

        MapearElementosEnListaDeElementos(lista, valores);

        lista.Lista.setAttribute(atListasDeElemento.yaCargado, "S");
        lista.Lista.setAttribute(atListasDeElemento.Cargando, "N");
        return fijarValorEnLista(lista.Lista, idFijar);
    }


    export function MapearElementosEnListaDeElementos(lista: Tipos.ListaDeElemento, datos: any) {
        let expresion: string = "";
        let mostrarExpresion = lista.Lista.getAttribute(atListasDeElemento.mostrarExpresion);

        for (var i = 0; i < datos.length; i++) {
            if (atListasDeElemento.expresionPorDefecto !== mostrarExpresion)
                expresion = ParsearExpresion(datos[i], mostrarExpresion.toLocaleLowerCase());
            else
                expresion = datos[i][mostrarExpresion];
            lista.Agregar(datos[i], expresion);
        }
    }


    export function MapearObjetoEnListaDeValores(lista: HTMLSelectElement, datos: any, nombreId: string, nombreValor: string, otrosAtributos: Array<string>, fijarPrimero: boolean) {
        lista.innerHTML = "";
        if (!Definido(datos) || datos.length == 0) return;
        for (var i = 0; i < datos.length; i++) {
            let id: number = Numero(ObtenerPropiedad(datos[i], nombreId));
            let b: boolean = true;
            for (var j = 0; j < lista.options.length; j++) {
                if (Numero(lista.options.item(j).value) === id) {
                    b = false;
                    break;
                }
            }

            if (!b)
                continue;

            var opcion = document.createElement("option");
            opcion.setAttribute("value", id.toString());
            opcion.setAttribute("label", ObtenerPropiedad(datos[i], nombreValor));
            for (let elemento of otrosAtributos) {
                opcion.setAttribute(elemento, ObtenerPropiedad(datos[i], elemento));
            }
            lista.appendChild(opcion);
        }
        if (fijarPrimero && lista.options.length > 1) lista.selectedIndex = 1;
    }

    export function Fecha(control: HTMLInputElement, fecha: string) {
        if (!Definido(fecha)) {
            control.value = "";
            return;
        }
        var fechaLeida = new Date(fecha);
        if (EsFechaValida(fechaLeida)) {
            FechaDate(control, fechaLeida);
        }
        else {
            var propiedad: string = control.getAttribute(atControl.propiedad);
            MensajesSe.Error("MapearFechaAlControl", `Fecha leida para la propiedad ${propiedad} es no válida, valor ${fecha}`);
        }
    }

    export function FechaDatePorPropiedad(modal: HTMLDivElement, propiedad: string, caducaEl: Date): boolean {
        return ApiControl.AsignarFecha(modal, propiedad, caducaEl)
    }

    export function FechaDate(control: HTMLInputElement, fecha: Date) {
        if (!Definido(fecha)) {
            control.value = "";
            return;
        }
        var fechaLeida = new Date(fecha);
        if (EsFechaValida(fechaLeida)) {
            let dia: number = fechaLeida.getDate();
            let mes: number = fechaLeida.getMonth() + 1;
            let ano: number = fechaLeida.getFullYear();
            control.value = `${ano}-${PadLeft(mes.toString(), "00")}-${PadLeft(dia.toString(), "00")}`;
        }
        else {
            var propiedad: string = control.getAttribute(atControl.propiedad);
            MensajesSe.Error("MapearFechaAlControl", `Fecha leida para la propiedad ${propiedad} es no válida, valor ${fecha}`);
        }
    }

    export function Hora(control: HTMLInputElement, fechaHora: string) {
        var fechaLeida = new Date(fechaHora);
        if (EsFechaValida(fechaLeida)) {
            HoraDate(control, fechaLeida);
        }
        else {
            var propiedad: string = control.getAttribute(atControl.propiedad);
            MensajesSe.Error("MapearHoraAlControl", `Fecha leida para la propiedad ${propiedad} es no válida, valor ${fechaHora}`);
        }
    }

    export function HoraDate(control: HTMLInputElement, fechaLeida: Date): boolean {
        let hora: number = fechaLeida.getHours();
        let minuto: number = fechaLeida.getMinutes();
        let segundos: number = fechaLeida.getSeconds();
        let milisegundos: number = fechaLeida.getMilliseconds();

        if (hora + minuto + segundos + milisegundos === 0) return true;

        let idHora: string = control.getAttribute(atControl.tipo) === ltrTipoControl.SelectorDeFechaHora ?
            control.getAttribute(atSelectorDeFecha.hora) :
            control.getAttribute(atEntreFechas.idHoraDesde);

        if (!IsNullOrEmpty(idHora)) {
            let controlHora: HTMLInputElement = document.getElementById(idHora) as HTMLInputElement;
            controlHora.value = `${PadLeft(hora.toString(), "00")}:${PadLeft(minuto.toString(), "00")}:${PadLeft(segundos.toString(), "00")}`;
            controlHora.setAttribute(atSelectorDeFecha.milisegundos, milisegundos.toString());
            return true;
        }
        return false;
    }

    export function ProponerFechaEn(fechaDesde: HTMLInputElement, propiedadFechaHasta: string, incrementoEnDias: number): void {
        let desde: Date = new Date(fechaDesde.value);
        if (!EsFechaValida(desde)) return;
        let contenedorDesde = ApiPanel.BuscarContenedorDeTablaDto(fechaDesde);
        if (!Definido(contenedorDesde)) return;

        let controlHasta = ApiControl.BuscarSelectorDeFechaHora(contenedorDesde, propiedadFechaHasta);
        if (NoDefinido(controlHasta))
            controlHasta = ApiControl.BuscarSelectorDeFecha(contenedorDesde, propiedadFechaHasta);
        if (!Definido(controlHasta)) return;

        if (controlHasta.readOnly)
            return;

        var hasta = new Date(controlHasta.value);
        if (!EsFechaValida(hasta)) {
            const nuevaFecha = new Date(desde);
            if (incrementoEnDias === 30) {
                nuevaFecha.setMonth(nuevaFecha.getMonth() + 1);
            } else if (incrementoEnDias === 365) {
                nuevaFecha.setFullYear(nuevaFecha.getFullYear() + 1);
            } else {
                nuevaFecha.setDate(nuevaFecha.getDate() + incrementoEnDias);
            }
            FechaDate(controlHasta, nuevaFecha);
        }

        if (desde === hasta && incrementoEnDias > 0) {
            FechaDate(controlHasta, new Date(desde.setDate(desde.getDate() + incrementoEnDias)));
            return;
        }

        if (desde > hasta) {
            FechaDate(controlHasta, new Date(desde.setDate(desde.getDate() + incrementoEnDias)));
            return;
        }

        var dias = new Date(hasta.getTime() - desde.getTime()).getDate() - 1;
        if (desde < hasta && incrementoEnDias !== dias)
            FechaDate(controlHasta, new Date(desde.setDate(desde.getDate() + incrementoEnDias)));
    }

    export function ProponerHoraEn(horaDesde: HTMLInputElement, fechaHasta: string, incremento: number): void {
        let desde = horaDesde.value.split(":");
        if (desde.length != 2) return;
        let idFechaDesde = horaDesde.id.replace(".hora", "");
        let contenedorDto = ApiPanel.BuscarContenedorDeTablaDto(horaDesde);
        if (Definido(contenedorDto)) {
            let fechaDesde = ApiControl.BuscarSelectorDeFechaHora(contenedorDto, idFechaDesde, atControl.id);
            if (Definido(fechaDesde)) {
                if (!EsFechaValida(new Date(fechaDesde.value))) return;
                let controlHasta = ApiControl.BuscarSelectorDeFechaHora(contenedorDto, fechaHasta);
                var fecha = new Date(controlHasta.value);
                if (Definido(controlHasta) && EsFechaValida(fecha)) {
                    AsignarTiempo(fecha, desde, incremento);
                    MapearAlControl.HoraDate(controlHasta, fecha);
                }
            }
        }
    }

    export function FijarValorEnListaDinamica(input: HTMLInputElement, id: number, texto: string) {
        ProponerValorEnListaDinamica(input, id, texto);
        ApiControl.BloquearInput(input);
    }

    export function ListaDinamica(lista: HTMLInputElement, id: number, texto: string, bloquear: boolean = false) {
        const ld = Tipos.ListaDinamica.Obtener(lista);
        try {
            if (ld.IdSeleccionado > 0 && id === ld.IdSeleccionado) {
                return;
            }

            ld.IdSeleccionado = Numero(id);
            if (ld.IdSeleccionado > 0) {
                lista.value = texto;
                ld.AgregarOpcion(id, texto);
            }
            else
                lista.value = "";

            ApiListaDinamica.BlanquearDependientes(lista);
            if (bloquear)
                ApiControl.BloquearListaDinamica(lista);

            if (ld.IdSeleccionado > 0) {
                let accion = lista.getAttribute(atControl.TrasMapear);
                if (Definido(accion))
                    EvaluarConElemento('ApiDeListasDinamicas.Cargar', accion, lista);
            }

            DefinirNavegador(lista, ld.IdSeleccionado);
            if (ld.IdSeleccionado > 0) {
                let accion = lista.getAttribute(atListas.trasSeleccionar);
                if (Definido(accion))
                    Evaluar('MapearAlControl.ListaDinamica', accion, accion.includes('this') ? lista : undefined);
            }
            else {
                if (ld.IdSelAlEntrar > 0) {
                    let accion = lista.getAttribute(atListas.trasBlanquear);
                    if (Definido(accion))
                        Evaluar('MapearAlControl.ListaDinamica', accion, accion.includes('this') ? lista : undefined);
                }
            }
        }
        catch (e) {
            ld.Resetear()
            MensajesSe.MostraExcepcion('MapearAlControl.ListaDinamica', e);
        }
        finally {
            lista.setAttribute(atListasDinamicas.cargando, 'N');
            lista.setAttribute(atListasDinamicas.ultimaCadenaBuscada, '');
            ld.IdSeleccionado = id;
            ld.IdSelAlEntrar = id;

        }
    }

    export function ListaDinamicaSinAcciones(lista: HTMLInputElement, id: number, texto: string, bloquear: boolean = false) {
        try {
            lista.setAttribute(atListasDinamicas.idSeleccionado, Numero(id).toString());
            lista.value = texto;
            const listaDinamica = Tipos.ListaDinamica.Obtener(lista);
            listaDinamica.AgregarOpcion(id, texto);
            DefinirNavegador(lista, id);
            if (bloquear) {
                lista.disabled = true;
                lista.readOnly = true;
            }
        }
        finally {
            lista.setAttribute(atListasDinamicas.cargando, 'N');
            lista.setAttribute(atListasDinamicas.ultimaCadenaBuscada, '');
        }
    }

    export function ListaDeValoresConResalto(lista: HTMLSelectElement, valor: string, resalto: string): boolean {
        const resultado = ListaDeValores(lista, valor);
        ApiControl.ResaltarControl(lista, resalto);
        return resultado;
    }

    export function ListaDeValores(lista: HTMLSelectElement, valor: string): boolean {
        let fijado: boolean = false;
        if (Definido(lista) === false)
            MensajesSe.EmitirExcepcion("ListaDeValores", `No se ha indicado la lista de valores donde mapear el valor '${valor}''`);
        if (NoDefinido(valor))
            lista.selectedIndex = 0;
        else
            fijado = fijarValorEnListaDeValores(lista, valor);

        if (fijado) {
            let accion = lista.getAttribute(atListasDeValores.OnChange);
            if (Definido(accion))
                Evaluar('MapearAlControl.ListaDeValores', accion, accion.includes('this') ? lista : undefined);
        }
        else {
            let accion = lista.getAttribute(atListasDeValores.OnReset);
            if (Definido(accion))
                Evaluar('MapearAlControl.ListaDeValores', accion, accion.includes('this') ? lista : undefined);
        }
        //const event = new Event('change');
        //lista.dispatchEvent(event);

        return fijado;
    }

    export function EliminarOpcion(lista: HTMLSelectElement, valor: string): boolean {
        if (NoDefinido(valor))
            lista.selectedIndex = 0;
        else
            return eliminarValorEnListaDeValores(lista, valor);

        return false;
    }

    export function DefinirNavegador(input: HTMLInputElement, id: number) {
        let contenedor = input.parentNode;
        if (contenedor instanceof HTMLDivElement) {
            let a: HTMLAnchorElement = contenedor.querySelector("a") as HTMLAnchorElement;
            if (Definido(a)) {
                let accion = a.getAttribute(atRef.onclick);
                if (Definido(accion)) {
                    a.href = accion;
                    return;
                }

                let controlador = a.getAttribute(atControl.controlador);
                let vista = a.getAttribute(atControl.vista);
                if (Definido(vista)) {
                    let parametros = '?';
                    a.href = `/${controlador}/${vista}`;

                    var negocio = input.getAttribute(literal.negocio);
                    if (!Definido(negocio) || negocio === enumNegocio.No_Definido)
                        negocio = a.getAttribute(literal.negocio);
                    if (Definido(negocio) && negocio !== enumNegocio.No_Definido)
                        parametros = `${parametros}negocio=${negocio}`;
                    else {
                        negocio = ObtenerParametroUrl(ltrPropiedades.Negocio.propiedad, enumNegocio.No_Definido, false);
                        if (Definido(negocio)) parametros = `${parametros}negocio=${negocio}`;
                    }

                    var parametrosParaNavegar = input.getAttribute(atControl.parametrosParaNavegador);
                    if (Definido(parametrosParaNavegar)) {
                        parametros = `${parametros}${(parametros.length > 1 ? "&" : "")}${parametrosParaNavegar}`;
                    }

                    if (id > 0) {
                        parametros = `${parametros}${(parametros.length > 1 ? "&" : "")}id=${id}`;
                    }
                    a.href = parametros === '?' ? a.href : `${a.href}${parametros}`;
                }
            }
        }
    }

    export function DefinirLink(input: HTMLInputElement) {
        let contenedor = input.parentNode;
        if (contenedor instanceof HTMLDivElement) {
            let a: HTMLAnchorElement = contenedor.querySelector("a") as HTMLAnchorElement;
            if (Definido(a)) {
                var url = input.value;
                if (Definido(url)) {
                    a.href = url;
                    var negocio = ObtenerParametroUrl(ltrPropiedades.Negocio.propiedad, '', false);
                    if (!IsNullOrEmpty(negocio)) {
                        a.href = a.href.indexOf('?') > 0 ? `${a.href}&negocio=${negocio}` : `${a.href}?negocio=${negocio}`;
                    }
                }
            }
        }
    }

    export function RedefinirLink(ref: HTMLAnchorElement) {
        let contenedor = ref.parentNode;
        if (contenedor instanceof HTMLDivElement) {
            let input: HTMLInputElement = contenedor.querySelector("input") as HTMLInputElement;
            if (Definido(input)) {
                var url = input.value;
                if (Definido(url)) {
                    if (ref.getAttribute('url-externa'))
                        ref.href = ref.href + url;
                    else {
                        ref.href = url;
                        var negocio = ObtenerParametroUrl(ltrPropiedades.Negocio.propiedad, '', false);
                        if (!IsNullOrEmpty(negocio)) {
                            ref.href = ref.href.indexOf('?') > 0 ? `${ref.href}&negocio=${negocio}` : `${ref.href}?negocio=${negocio}`;
                        }
                    }
                }
            }
        }
    }

    export function FijarValorEnEditor(input: HTMLInputElement, id: number, texto: string, bloquear: boolean, asignarPorDefecto: boolean) {
        Restrictor(input, id, texto, bloquear);
        if (!bloquear) ApiControl.DesbloquearEditor(input);

        if (asignarPorDefecto) {
            let formato = input.getAttribute(atControl.formato);
            if (!IsNullOrEmpty(formato) && EsDecimal(texto)) {
                if ((formato === enumFormato.Moneda || formato === enumFormato.Porcentaje) && input.type === enumTipoControl.number)
                    formato = enumFormato.Numero_2;
                input.defaultValue = FormatearNumero(Numero(texto), formato);
            }
            else
                input.defaultValue = texto;
        }
    }


    function mapearPropiedad(tipo: string, control: HTMLElement, id: number, texto: string, esRestrictor: boolean, asignarPorDefecto: boolean) {
        if (tipo === ltrTipoControl.ListaDinamica)
            FijarValorEnListaDinamica(control as HTMLInputElement, id, texto);
        else if (tipo === ltrTipoControl.Editor)
            MapearEditor(control as HTMLInputElement, id, texto, esRestrictor, asignarPorDefecto);
        else if (tipo === ltrTipoControl.restrictorDeEdicion)
            MapearEditor(control as HTMLInputElement, id, texto, true, asignarPorDefecto);
        else if (tipo === ltrTipoControl.restrictorDeFiltro)
            MapearEditor(control as HTMLInputElement, id, texto, true, asignarPorDefecto);
        else if (tipo === ltrTipoControl.Check)
            (control as HTMLInputElement).checked = EsTrue(id);
        if (tipo === ltrTipoControl.ListaDeValores)
            (control as HTMLSelectElement).selectedIndex = id;

        return control;
    }


    function fijarValorEnLista(lista: HTMLSelectElement, id: number): boolean {

        let fijado: boolean = false;
        if (!EsMayorDeCero(id)) {
            lista.selectedIndex = 0;
            let accion = lista.getAttribute(atListasDeElemento.trasCargar);
            if (Definido(accion))
                Evaluar('MapearAlControl.fijarValorEnLista.trasCargar', accion, accion.includes('this') ? lista : undefined);
        }
        else {
            for (var j = 0; j < lista.options.length; j++) {
                if (Numero(lista.options[j].value) === id) {
                    lista.selectedIndex = j;
                    fijado = true;
                }
            }
        }

        if (!fijado && EsTrue(lista.getAttribute(atListasDeElemento.AutoPosicionamiento)) && !lista.disabled && lista.options.length === 2 && (lista.options[0].text === atListasDeElemento.Seleccionar || lista.options[0].text === 'Seleccione la transición a ejecutar')) {
            if (Numero(lista.options[1].value) === id || id === 0) {
                lista.selectedIndex = 1;
                fijado = true;
            }
        }

        let accion = lista.getAttribute(atListasDeValores.OnChange);
        if (Definido(accion))
            Evaluar('MapearAlControl.fijarValorEnLista.OnChange', accion, accion.includes('this') ? lista : undefined);

        return fijado
    }

    export function FijarElPrimero(lista: HTMLSelectElement): void {
        if (EsTrue(lista.getAttribute(atListasDeElemento.AutoPosicionamiento)) && lista.options.length === 2 && lista.options[0].text === atListasDeElemento.Seleccionar) {
            if (Numero(lista.options[1].value) > 0) {
                lista.selectedIndex = 1;
            }
        }
    }
    export function DesfijarElPrimero(lista: HTMLSelectElement): void {
        if (EsTrue(lista.getAttribute(atListasDeElemento.AutoPosicionamiento)) && lista.options.length === 2 && lista.options[0].text === atListasDeElemento.Seleccionar) {
            if (Numero(lista.options[1].value) > 0) {
                lista.selectedIndex = 0;
                ApiControl.QuitarResalto(lista);
            }
        }
    }

    function fijarValorEnListaDeValores(lista: HTMLSelectElement, valor: string): boolean {
        if (EsNumero(valor)) {
            let incremento = (Numero(lista.options[0].value) === Numero(literal.menos1)) ? 1 : 0;
            for (var j = 0; j < lista.options.length; j++)
                if (lista.options[j].index === Numero(valor) + incremento) {
                    lista.selectedIndex = j;
                    return true;
                }
        }
        else {
            for (var j = 0; j < lista.options.length; j++)
                if (lista.options[j].value.toLocaleLowerCase() === valor.toLocaleLowerCase()) {
                    lista.selectedIndex = j;
                    return true;
                }
        }
        return false;
    }

    function eliminarValorEnListaDeValores(lista: HTMLSelectElement, valor: string): boolean {
        if (EsNumero(valor)) {
            for (var j = lista.options.length - 1; j >= 0; j--)
                if (lista.options[j].index === Numero(valor)) {
                    lista.options.remove(j);
                    return true;
                }
        }
        else {
            for (var j = lista.options.length - 1; j >= 0; j--)
                if (lista.options[j].value === valor) {
                    lista.options.remove(j);
                    return true;
                }
        }
        return false;
    }

    function ProponerValorEnListaDinamica(input: HTMLInputElement, id: number, texto: string) {
        if (Numero(id) > 0) {
            const listaDinamica = Tipos.ListaDinamica.Obtener(input);
            listaDinamica.AgregarOpcion(id, texto);
            input.setAttribute(atListasDinamicas.idSelAlEntrar, Numero(id).toString());
        }
        ListaDinamica(input, id, texto);
    }

    function FijarEnLaListaElElementoLeidoPorId(peticion: ApiDeAjax.DescriptorAjax): any {
        let datosDeEntrada: Tipos.DatosPeticionLista = JSON.parse(peticion.DatosDeEntrada);
        let lista: Tipos.ListaDeElemento = new Tipos.ListaDeElemento(datosDeEntrada.IdLista);
        let registro = peticion.resultado.datos;
        let expresion: string = "";
        let mostrarExpresion = lista.Lista.getAttribute(atListasDeElemento.mostrarExpresion);
        if (atListasDeElemento.expresionPorDefecto !== mostrarExpresion)
            expresion = ParsearExpresion(registro, mostrarExpresion.toLocaleLowerCase());
        else
            expresion = registro[mostrarExpresion];
        lista.Agregar(registro, expresion);
        fijarValorEnLista(lista.Lista, registro.id);
    }
}

