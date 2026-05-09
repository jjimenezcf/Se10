namespace MapearAlPanel {

    export function MapearControlesDesdeOtroPanel(origen: HTMLDivElement, destino: HTMLDivElement) {
        MapearListasDinamica(origen, destino);
        MapearListasDeValores(origen, destino);
        MapearRestrictores(origen, destino);
        MapearTextAreas(origen, destino);
    }

    export function MapearPropiedadDesdeOtroPanel(origen: HTMLDivElement, destino: HTMLDivElement, propiedad: string, bloquear: boolean) {
        var crtlOrigen = ApiControl.BuscarControl(origen, propiedad, false);
        if (Definido(crtlOrigen)) {
            var crtlDestino = ApiControl.BuscarControl(destino, propiedad, false);
            if (Definido(crtlDestino)) {
                ApiControl.AsignarControl(crtlOrigen, crtlDestino, bloquear);
            }
        }
    }

    function MapearListasDinamica(origen: HTMLDivElement, destino: HTMLDivElement) {
        let listas: NodeListOf<HTMLInputElement> = origen.querySelectorAll(`input[${atControl.tipo}="${ltrTipoControl.ListaDinamica}"]`) as NodeListOf<HTMLInputElement>;
        for (let i = 0; i < listas.length; i++) {
            let lista = listas[i];
            let propiedad = lista.getAttribute(atControl.propiedad);
            let control = EsTrue(lista.getAttribute(atControl.filtro))
                ? ApiControl.BuscarListaDinamicaPorGuardarEn(destino, propiedad)
                : ApiControl.BuscarListaDinamicaPorPropiedad(destino, propiedad);
            if (Definido(control)) {
                let valor: number = Numero(lista.getAttribute(atListasDinamicas.idSeleccionado));
                if (valor > 0) MapearAlControl.ListaDinamica(control, valor, lista.value, false);
            }
        }
    }

    function MapearListasDeValores(origen: HTMLDivElement, destino: HTMLDivElement) {
        let listas: NodeListOf<HTMLSelectElement> = origen.querySelectorAll(`select[${atControl.tipo}="${ltrTipoControl.ListaDeValores}"]`) as NodeListOf<HTMLSelectElement>;
        for (let i = 0; i < listas.length; i++) {
            let lista = listas[i];
            let propiedad = lista.getAttribute(atControl.propiedad);
            let control: HTMLSelectElement = ApiControl.BuscarListaDeValores(destino, propiedad) as HTMLSelectElement;
            if (Definido(control) && Numero(lista.value) > 0) MapearAlControl.ListaDeValores(control, lista.value);
        }
    }

    function MapearRestrictores(origen: HTMLDivElement, destino: HTMLDivElement) {
        let restrictores: NodeListOf<HTMLInputElement> = origen.querySelectorAll(`input[${atControl.tipo}="${ltrTipoControl.restrictorDeFiltro}"]`) as NodeListOf<HTMLInputElement>;
        for (let i = 0; i < restrictores.length; i++) {
            let restrictor = restrictores[i];
            let propiedad = restrictor.getAttribute(atControl.propiedad);
            let control: HTMLInputElement = ApiControl.BuscarRestrictor(destino, propiedad, ltrTipoControl.restrictorDeEdicion) as HTMLInputElement;
            let id = Numero(restrictor.getAttribute(atControl.restrictor));
            if (Definido(control) && id > 0) MapearAlControl.Restrictor(control, id, restrictor.value);
        }
    }

    function MapearTextAreas(origen: HTMLDivElement, destino: HTMLDivElement) {
        let areas: NodeListOf<HTMLTextAreaElement> = origen.querySelectorAll(`input[${atControl.tipo}="${ltrTipoControl.AreaDeTexto}"]`) as NodeListOf<HTMLTextAreaElement>;
        for (let i = 0; i < areas.length; i++) {
            let area = areas[i];
            let propiedad = area.getAttribute(atControl.propiedad);
            let control: HTMLTextAreaElement = ApiControl.BuscarAreaDeTexto(destino, propiedad, ltrTipoControl.restrictorDeEdicion) as HTMLTextAreaElement;
            MapearAlControl.MapearAreaDeTexto(control, area.value, false);
        }
    }

    export function RestrictoresPorPropiedad(panel: HTMLDivElement, propiedad: string, id: number, texto: string): boolean {

        let control: HTMLInputElement = panel.querySelector(`[${atControl.propiedad}=${propiedad.toLowerCase()}]`) as HTMLInputElement;
        if (!Definido(control))
            return false;
        let tipo = control.getAttribute(atControl.tipo);
        switch (tipo) {
            case ltrTipoControl.Editor: MapearAlControl.Restrictor(control, id, texto, true);
                break;
            case ltrTipoControl.restrictorDeEdicion: MapearAlControl.Restrictor(control, id, texto, true);
                break;
            case ltrTipoControl.ListaDinamica: MapearAlControl.ListaDinamica(control, id, texto, true);
                break;
        }
        return true;
    }


    export function DatosPropuestos(panel: HTMLDivElement, datosPropuestos: any) {

        for (let i: number = 0; i < Object.getOwnPropertyNames(datosPropuestos).length; i++) {
            let propiedad = Object.getOwnPropertyNames(datosPropuestos)[i];
            const control = mapearPropiedad(panel, propiedad, datosPropuestos, false, ModoAcceso.enumModoDeAccesoDeDatos.Gestor);
            if (Definido(control)) ApiControl.ResaltarControl(control, ltrCss.Resalto.Violeta);
        }

        MapearAlControl.MapearLinks(panel, datosPropuestos, ModoAcceso.enumModoDeAccesoDeDatos.Gestor);
    }

    export function ElObjeto(panel: HTMLDivElement, objeto: any, modoDeAcceso: ModoAcceso.enumModoDeAccesoDeDatos, noMapear: Array<string> = new Array<string>(), informarDePropiedadNoEncontrada: boolean = false) {
        let selectorDeArchivos = panel.querySelector(`[${atControl.tipo}=${ltrTipoControl.SelectorDeArchivos}]`);
        if (Definido(selectorDeArchivos))
            selectorDeArchivos.setAttribute(atControl.idElemento, ObtenerPropiedad(objeto, ltrPropiedades.Elemento.Id));

        let usaBaja = ExistePropiedad(objeto, ltrPropiedades.baja);
        let estaDeBaja = usaBaja ? ObtenerPropiedad(objeto, ltrPropiedades.baja, false) : false;
        if (estaDeBaja) modoDeAcceso = ModoAcceso.enumModoDeAccesoDeDatos.Consultor;

        for (let i: number = 0; i < Object.getOwnPropertyNames(objeto).length; i++) {
            let propiedad = Object.getOwnPropertyNames(objeto)[i];
            if (noMapear.includes(propiedad.toLowerCase()))
                continue;
            mapearPropiedad(panel, propiedad, objeto, informarDePropiedadNoEncontrada, modoDeAcceso);
        }

        MapearAlControl.MapearLinks(panel, objeto, modoDeAcceso);
    }

    function mapearPropiedad(panel: HTMLDivElement, propiedad: string, objeto: any, informarDePropiedadNoEncontrada: boolean, modoDeAcceso: ModoAcceso.enumModoDeAccesoDeDatos): HTMLElement {
        let control = ApiControl.BuscarControl(panel, propiedad, false);
        if (NoDefinido(control)) {
            if (informarDePropiedadNoEncontrada)
                MensajesSe.Info(`la propiedad: ${propiedad} no se ha localizado en el panel ${panel.id}`);
            return undefined;
        }
        let tipoControl: string = control.getAttribute(`${atControl.tipo}`);

        switch (tipoControl) {
            case ltrTipoControl.Editor:
                MapearAlControl.MapearEditor(control as HTMLInputElement, Numero(atRestrictor.noRestringido), objeto[propiedad], false, false);
                (control as HTMLInputElement).readOnly = (control as HTMLInputElement).readOnly || !ModoAcceso.EsGestor(modoDeAcceso);
                return control;
            case ltrTipoControl.Check:
                (control as HTMLInputElement).checked = EsTrue(objeto[propiedad]);
                (control as HTMLInputElement).readOnly = (control as HTMLInputElement).readOnly || !ModoAcceso.EsGestor(modoDeAcceso);
                return control;
            case ltrTipoControl.restrictorDeEdicion:
                let mostrarPropiedad = control.getAttribute(atRestrictor.mostrarPropiedad);
                let propiedadRestrictora = control.getAttribute(atRestrictor.propiedadRestrictora);
                let valorMostrar: string = Json_BuscarValorEn(mostrarPropiedad, objeto);
                let idRestrictor: number = Definido(propiedadRestrictora)
                    ? Numero(Json_BuscarValorEn(propiedadRestrictora, objeto))
                    : Numero(Json_BuscarValorEn(propiedad, objeto));
                MapearAlControl.Restrictor((control as HTMLInputElement), idRestrictor, valorMostrar);
                return control;
            case ltrTipoControl.ListaDinamica:
                let guardarEn = control.getAttribute(atListasDinamicasDto.guardarEn);
                let valor = Json_BuscarValorEn(guardarEn, objeto);
                MapearAlControl.ListaDinamica((control as HTMLInputElement), Numero(valor), objeto[propiedad]);
                (control as HTMLInputElement).readOnly = (control as HTMLInputElement).readOnly || !ModoAcceso.EsGestor(modoDeAcceso);
                return control;
            case ltrTipoControl.ListaDeValores:
                let guardarEnLv = control.getAttribute(atListas.guardarEn);
                let valorLv = Json_BuscarValorEn(guardarEnLv, objeto);
                MapearAlControl.ListaDeValores((control as HTMLSelectElement), valorLv);
                (control as HTMLSelectElement).disabled = (control as HTMLSelectElement).disabled || !ModoAcceso.EsGestor(modoDeAcceso);
                return control;
            case ltrTipoControl.ListaDeElementos:
                let guardarEnLe = control.getAttribute(atListas.guardarEn);
                let id: number = Numero(Json_BuscarValorEn(guardarEnLe, objeto));

                let cargarBajoDemanda: boolean = EsTrue(control.getAttribute(atListasDeElemento.cargarBajoDemanda));
                if (id === 0 && cargarBajoDemanda)
                    ApiControl.QuitarOpcionesDeLalista((control as HTMLSelectElement));
                else
                   MapearAlControl.ListaDeElementos((control as HTMLSelectElement), new Array<ClausulaDeFiltrado>(), id);
                (control as HTMLSelectElement).disabled = (control as HTMLSelectElement).disabled || !ModoAcceso.EsGestor(modoDeAcceso);
                return control;
            case ltrTipoControl.Archivo:
                let idArchivo: number = Numero(Json_BuscarValorEn(propiedad, objeto)) as number;
                if (idArchivo > 0) {
                    let visorVinculado: string = control.getAttribute(atArchivo.imagen);
                    control.setAttribute(atArchivo.idArchivo, idArchivo.toString());
                    MapearAlControl.MapearImagenes(objeto, visorVinculado);
                    if (!ModoAcceso.EsGestor(modoDeAcceso)) {
                        let ref: HTMLAnchorElement = document.getElementById(`${control.id}.ref`) as HTMLAnchorElement;
                        ref.style.display = ltrStyle.display.none;
                    }
                }
                else {
                    ApiDeArchivos.BlanquearArchivo(control as HTMLInputElement, true);
                    if (!ModoAcceso.EsGestor(modoDeAcceso)) {
                        let tabla: HTMLDivElement = control.parentNode.parentNode.parentNode.parentNode as HTMLDivElement;
                        tabla.style.display = ltrStyle.display.none;
                    }
                }
                return control;
            case ltrTipoControl.SelectorDeUnArchivo:
                let idSelectorDeUnArchivo: number = Numero(Json_BuscarValorEn(propiedad, objeto)) as number;
                if (idSelectorDeUnArchivo > 0) {
                    //mostrar nombre
                    if (!ModoAcceso.EsGestor(modoDeAcceso)) {
                        let ref: HTMLAnchorElement = document.getElementById(`${control.id}.ref`) as HTMLAnchorElement;
                        ref.style.display = ltrStyle.display.none;
                    }
                }
                else {
                    ApiDeArchivos.BlanquearArchivo(control as HTMLInputElement, true);
                    if (!ModoAcceso.EsGestor(modoDeAcceso)) {
                        let tabla: HTMLDivElement = control.parentNode.parentNode.parentNode.parentNode as HTMLDivElement;
                        tabla.style.display = ltrStyle.display.none;
                    }
                }
                return control;
            case ltrTipoControl.UrlDeArchivo:
                let imagen = objeto[propiedad];
                MapearAlControl.ImagenUrl(control as HTMLInputElement, imagen);
                if (!ModoAcceso.EsGestor(modoDeAcceso)) {
                    var td = ApiControl.BuscarCeldaConClase(control, ltrCss.tdPropiedad);
                    var div = td.querySelector('.' + ltrCss.contenedorDeEtiqueta) as HTMLDivElement;
                    ApiPanel.OcultarPanel(div);
                }
                return control;
            case ltrTipoControl.AreaDeTexto:
                MapearAlControl.MapearAreaDeTexto(control as HTMLTextAreaElement, objeto[propiedad], false);
                (control as HTMLTextAreaElement).readOnly = (control as HTMLTextAreaElement).readOnly || !ModoAcceso.EsGestor(modoDeAcceso);
                return control;
            case ltrTipoControl.SelectorDeFecha:
                let fecha: string = Json_BuscarValorEn(propiedad, objeto) as string;
                MapearAlControl.MapearSelectorDeFecha(control as HTMLInputElement, fecha);
                (control as HTMLInputElement).readOnly = (control as HTMLInputElement).readOnly || !ModoAcceso.EsGestor(modoDeAcceso);
                return;
            case ltrTipoControl.SelectorDeFechaHora:
                let fechaHora: string = Json_BuscarValorEn(propiedad, objeto) as string;
                MapearAlControl.MapearSelectorDeFecha(control as HTMLInputElement, fechaHora);
                ModoAcceso.AplicarloALaFecha((control as HTMLInputElement), modoDeAcceso);
                //(control as HTMLInputElement).readOnly = (control as HTMLInputElement).readOnly || !ModoAcceso.EsGestor(modoDeAcceso);
                return control;
            case ltrTipoControl.Referencia:
                let ref = control as HTMLAnchorElement;
                MapearAlControl.MapearReferencia(ref, objeto, modoDeAcceso);
                return control;
            case ltrTipoControl.ReferenciaPost:
                let refPost = control as HTMLInputElement;
                MapearAlControl.MapearReferenciaPost(refPost, objeto, modoDeAcceso);
                return control;
        }

        return control;
    }

    export function MaperaListasDeElementos(panel: HTMLDivElement, elementoJson: JSON) {
        let listas: HTMLCollectionOf<HTMLSelectElement> = panel.getElementsByTagName('select') as HTMLCollectionOf<HTMLSelectElement>;
        for (var i = 0; i < listas.length; i++) {
            let lista: HTMLSelectElement = listas[i] as HTMLSelectElement;
            let guardarEn: string = lista.getAttribute(atListas.guardarEn);
            let id: number = Numero(Json_BuscarValorEn(guardarEn, elementoJson));
            MapearAlControl.ListaDeElementos(lista, new Array<ClausulaDeFiltrado>(), id);
        }
    }

    export function MapearLasAmpliaciones(edicion: Crud.CrudEdicion, idNegocio: number, idDelElemento: number, idTipo: number) {
        let panel = edicion.PanelDeEditar;
        let ampliaciones: NodeListOf<HTMLDivElement> = panel.querySelectorAll(`div[${atControl.esAmpliacion}='${literal.true}']`) as NodeListOf<HTMLDivElement>;
        edicion.AmpliacionesPorCargar = ampliaciones.length;
        for (var i = 0; i < ampliaciones.length; i++) {
            let ampliacion: HTMLDivElement = ampliaciones[i] as HTMLDivElement;
            MapearAmpliacion(edicion, ampliacion, idNegocio, idDelElemento, idTipo);
        }
    }

    function MapearAmpliacion(edicion: Crud.CrudEdicion, ampliacion: HTMLDivElement, idNegocio: number, idDelElemento: number, idTipo: number) {
        let controlador: string = ampliacion.getAttribute(ltrAmpliaciones.controlador);

        let datosDeEntrada: Array<Parametro> = new Array<Parametro>();
        datosDeEntrada.push(new Parametro(ltrTipoControl.ampliacion, ampliacion.id));
        let parametros = new Array<Parametro>();
        parametros.push(new Parametro(ltrPropiedades.Elemento.ConTipo.IdTipo, idTipo));
        ApiDePeticiones.LeerAmpliacion(edicion, controlador, idNegocio, idDelElemento, parametros, datosDeEntrada).
            then((peticion) => DespuesDeLeerAmpliacion(peticion)).
            catch((peticion) => ApiDePeticiones.EmitirError(peticion));
    }

    function DespuesDeLeerAmpliacion(peticion: ApiDeAjax.DescriptorAjax) {
        let edicion: Crud.CrudEdicion = peticion.llamador as Crud.CrudEdicion;
        let idAmpliacion: string = peticion.DatosDeEntrada[0].valor;
        let ampliacion: HTMLDivElement = document.getElementById(idAmpliacion) as HTMLDivElement;
        let contenedor: HTMLDivElement = ampliacion.parentElement.parentElement as HTMLDivElement;
        ApiPanel.BlanquearControlesDeIU(ampliacion, false);
        ApiPanel.BlanquearRestrictoresDeEdicion(ampliacion);

        if (NoDefinido(peticion.resultado.datos) && ModoAcceso.Parsear(peticion.resultado.modoDeAcceso) === ModoAcceso.enumModoDeAccesoDeDatos.SinPermiso) {
            ApiPanel.OcultarPanel(contenedor);
        }
        else {
            ApiPanel.MostrarPanel(contenedor);
            let propiedades = ToLista(ampliacion.getAttribute(atControl.NoMapeablesAlDto), ';');
            let modoDeAcceso = ObtenerPropiedad(peticion.resultado.datos, literal.ModoDeAcceso, ModoAcceso.enumModoDeAccesoDeDatos.Consultor);
            if (Definido(peticion.resultado.datos))
                MapearAlPanel.ElObjeto(ampliacion, peticion.resultado.datos, modoDeAcceso, propiedades);
            ModoAcceso.AplicarloAlPanel(ampliacion, ModoAcceso.Parsear(peticion.resultado.modoDeAcceso), false);
        }
        edicion.Expansor_TrasCargarAmpliacion(ampliacion);
    }
}
