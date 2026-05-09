namespace ApiListaDinamica {

    export function AsignarValorConResaltado(lista: HTMLInputElement, id: number, texto: string, resalto: string, bloquear: boolean = false): void {
        AsignarValor(lista, id, texto, bloquear);
        ApiControl.ResaltarControl(lista, resalto);
    }

    export function AsignarValor(lista: HTMLInputElement, id: number, texto: string, bloquear: boolean = false): void {
        const idAlEntrar: number = Numero(lista.getAttribute(atListasDinamicas.idSelAlEntrar));
        lista.value = texto;
        if (idAlEntrar !== id) {
            MapearAlControl.ListaDinamica(lista, id, texto);
        }
        else {
            if (id > 0) {
                let accion = lista.getAttribute(atControl.TrasMapear);
                if (Definido(accion))
                    EvaluarConElemento('AsignarValor', accion, lista);
            }
        }
    }

    export function ObtenerFoco(lista: HTMLInputElement) {

        if (ApiControl.EsSoloLectura(lista))
            return;
        lista.setAttribute(atListasDinamicas.cargando, 'N')
        let idsel = lista.getAttribute(atListasDinamicas.idSeleccionado);
        lista.setAttribute(atListasDinamicas.idSelAlEntrar, idsel);
        if (Numero(idsel) === 0) {
            Tipos.ListaDinamica.Resetear(lista);
            //Cargar(lista, false, 0);
        }
    }

    export function ListaPulsada(lista: HTMLInputElement) {

        if (ApiControl.EsSoloLectura(lista))
            return;

        const listaDinamica = Tipos.ListaDinamica.Obtener(lista);

        if (listaDinamica.DropdownVisible) {
            if (IsNullOrEmpty(lista.value)) {
                listaDinamica.ocultarDropdown();
                return;
            }
        }
        else {
            if (IsNullOrEmpty(lista.value) && listaDinamica.Opciones.keys.length > 0 && listaDinamica.IdSeleccionado === 0) {
                listaDinamica.MostrarDropdown();
                return;
            }
            if (!IsNullOrEmpty(lista.value) && listaDinamica.IdSeleccionado > 0)
                return;
        }
        if (IsNullOrEmpty(lista.value)) listaDinamica.UltimaBusqueda = undefined;
        Cargar(lista, false, 0);
    }

    export function Cargar(lista: HTMLInputElement, necesitaAlgoEscrito: boolean = false, espera:number = 1000) {
        if (document.activeElement !== lista) {
            return;
        }

        const listaDinamica = Tipos.ListaDinamica.Obtener(lista);

        if (listaDinamica.UltimaBusqueda !== null && listaDinamica.UltimaBusqueda === lista.value)
            return;

        if (!sePuedeCargarLista(lista, necesitaAlgoEscrito))
            return;

        const informacion: { cargar: boolean; controlador: string; filtros: Array<ClausulaDeFiltrado>; parametros: Array<Parametro>; datosDeEntrada: string }
            = DefinirFiltrosParaCargar(lista, necesitaAlgoEscrito);

        if (!informacion.cargar)
            return;

        const timeoutIdActual: string | null = lista.getAttribute('data-debounce-id');

        if (timeoutIdActual !== null) {
            clearTimeout(parseInt(timeoutIdActual, 10));
        }
        const valorOriginal: string = lista.value;

        const newTimeoutId = setTimeout(() => {
            lista.removeAttribute('data-debounce-id');

            if (lista.value !== valorOriginal) {
                return;
            }

            try {
                lista.setAttribute(atListasDinamicas.cargando, 'S');
                lista.setAttribute(atListasDinamicas.ultimaCadenaBuscada, lista.value);
                ApiDePeticiones.LeerElementos(lista, informacion.controlador, Ajax.EndPoint.LeerElementos, informacion.filtros, informacion.parametros, informacion.datosDeEntrada)
                    .then((peticion) => {
                        listaDinamica.Informacion = informacion;
                        ListaDinamica_AnadirOpciones(peticion, false);
                    })
                    .catch((peticion) => ListasDinamica_SiHayErrorAlCargar(peticion))
                    .finally(() => {
                        lista.setAttribute(atListasDinamicas.cargando, 'N');
                    });
            }
            catch {
                lista.setAttribute(atListasDinamicas.cargando, 'N');
            }
        }, espera);


        lista.setAttribute('data-debounce-id', newTimeoutId.toString());
    }

    export function PerderFoco(lista: HTMLInputElement) {

        if (ApiControl.EsSoloLectura(lista))
            return;

        if (lista.getAttribute(atListasDinamicas.cargando) === 'S')
            return;

        // 1. Obtener la instancia
        const ldInstance: Tipos.ListaDinamica = Tipos.ListaDinamica.Obtener(lista);
        if (ldInstance.Cargando)
            return;

        // 2. Lógica central de validación y limpieza
        const ejecutarLogicaPerderFoco = () => {
            try {
                // --- LÓGICA ORIGINAL DE PERDERFOCO ---
                let blanquearAlSalir: boolean = EsTrue(lista.getAttribute(atListasDinamicas.BlanquearAlSalir));
                let idSeleccionado: number = Numero(lista.getAttribute(atListasDinamicas.idSeleccionado));
                let idAlEntrar: number = Numero(lista.getAttribute(atListasDinamicas.idSelAlEntrar));

                //si tengo el mismo id seleccionado que cuando entré
                if (idSeleccionado === idAlEntrar) {
                    //si he escrito algo en la lista, 
                    if (!IsNullOrEmpty(lista.value)) {
                        //pero no corresponde con ningun valor de BD y tengo que blanquear lo escrito
                        if (idSeleccionado === 0) {
                            if (blanquearAlSalir) {
                                IntentarRecuperarInformacionDeLoEscrito(lista, true);
                                lista.focus();
                            }
                        }
                        else {
                            const texto = ldInstance.BuscarPorId(idSeleccionado)
                            if (texto)
                                ldInstance.ListaHtml.value = texto;
                            else {
                                IntentarRecuperarInformacionDeLoEscrito(lista, true);
                                lista.focus();
                                return;
                            }
                        }
                    }
                    //si he blanqueado el texto del selector
                    else {
                        ldInstance.Vaciar();
                        Blanquear(lista, true);
                        BlanquearDependientes(lista);
                    }

                    return;
                }
                //a partir de aquí significa que el valor se ha cambiado
                //Si he anulado lo seleccionado tras entrar y he de blanquear el texto --> blanqueo y ejecuto la acción
                if (idSeleccionado === 0) {
                    if (blanquearAlSalir) {
                        IntentarRecuperarInformacionDeLoEscrito(lista, true);
                        lista.focus();
                    }
                    return;
                }

                //esto significa que el valor es nulo, aunque el id seleccionado es > 0 y eso es incoherente, por tanto blanqueo y ejecuto la acción
                if (IsNullOrEmpty(lista.value)) {
                    ldInstance.Vaciar();
                    Blanquear(lista, true);
                    BlanquearDependientes(lista);
                    return;
                }

                //si lo escrito no corresponde con niguna de las opciones disponibles cargadas y he de blanquear, blanqueo y ejecuto la acción
                if (BuscarOpciones(lista, lista.value) === 0) {
                    if (blanquearAlSalir)
                        Blanquear(lista, true);
                    return;
                }

                ConfigurarListaDinamicaTrasAsignarValor(lista);
                // --- FIN LÓGICA ORIGINAL ---

            }
            finally {
                // Se ejecuta después de la lógica, incluso si se hizo un 'return' o hubo error
                // Si la selección por click tuvo éxito, OcultarDropdown() ya se llamó dentro de seleccionarValor.
                // Si la selección falló (por teclado o ratón), esto asegura que el dropdown se oculte.
                ldInstance.ocultarDropdown();
                lista.setAttribute(atListasDinamicas.cargando, 'N');
            }
        };

        // 3. Mecanismo de aplazamiento (Anti-Conflicto Blur/Click)

        // Si el dropdown está visible, aplazamos la lógica de validación/limpieza.
        // Esto da al evento 'click' (ratón) tiempo para ejecutarse, fijar el valor y ocultar el dropdown.
        if (ldInstance.Dropdown && ApiControl.EsVisible(ldInstance.Dropdown)) {
            setTimeout(ejecutarLogicaPerderFoco, 0);
        } else {
            // Si el dropdown ya está oculto (ej. después de seleccionar por teclado) 
            // o si simplemente el foco se perdió sin interactuar con el dropdown, ejecutar inmediatamente.
            ejecutarLogicaPerderFoco();
        }
    }

    function ConfigurarListaDinamicaTrasAsignarValor(lista: HTMLInputElement) {
        MapearAlControl.DefinirNavegador(lista, Tipos.ListaDinamica.Obtener(lista)?.IdSeleccionado ?? 0);
        let accion = lista.getAttribute(atListas.trasSeleccionar);
        if (Definido(accion))
            Evaluar('ApiDeListasDinamicas.PerderFoco', accion, accion.includes('this') ? lista : undefined);
    }

    function IntentarRecuperarInformacionDeLoEscrito(lista: HTMLInputElement, ejecutarAccionDeBlanqueo: boolean) {

        if (!sePuedeCargarLista(lista, true)) {
            Blanquear(lista, ejecutarAccionDeBlanqueo);
            return;
        }

        let informacion: { cargar: boolean; controlador: string; filtros: Array<ClausulaDeFiltrado>; parametros: Array<Parametro>; datosDeEntrada: string }
            = DefinirFiltrosParaCargar(lista, true);

        if (!informacion.cargar) {
            Blanquear(lista, ejecutarAccionDeBlanqueo);
            return;
        }
        try {
            lista.setAttribute(atListasDinamicas.cargando, 'S');
            ApiDePeticiones.LeerElementos(lista, informacion.controlador, Ajax.EndPoint.LeerElementos, informacion.filtros, informacion.parametros, informacion.datosDeEntrada)
                .then((peticion) => {
                    ListaDinamica_AnadirOpciones(peticion, true);
                    if (peticion.resultado.datos.length === 0)
                        Blanquear(lista, ejecutarAccionDeBlanqueo);
                })
                .catch((peticion) => ListasDinamica_SiHayErrorAlCargar(peticion))
                .finally(() => {
                    lista.setAttribute(atListasDinamicas.cargando, 'N');
                });
        }
        catch {
            lista.setAttribute(atListasDinamicas.cargando, 'N');
        }
    }



    export function DefinirFiltrosParaCargar(lista: HTMLInputElement, necesitaAlgoEscrito: boolean = false): any {
        let filtros: Array<ClausulaDeFiltrado> = DefinirFiltroListaDinamica(lista, necesitaAlgoEscrito);

        if (filtros === null && necesitaAlgoEscrito)
            return { cargar: false, controlador: undefined, filtros: filtros, parametros: undefined, datosDeEntrada: undefined };

        let parametros: Array<Parametro> = new Array<Parametro>();
        let aplicarJoin = EsTrue(lista.getAttribute(atListasDinamicas.AplicarJoin));
        let cantidad: string = lista.getAttribute(atListasDinamicas.cantidad);
        parametros.push(new Parametro(Ajax.Param.aplicarJoin, aplicarJoin));
        parametros.push(new Parametro(Ajax.Param.cantidad, cantidad));
        parametros.push(new Parametro(Ajax.Param.obtenerSeguridad, false));
        parametros.push(new Parametro(Ajax.Param.cargarListaDinamica, true));


        let estaElModuloCargado = typeof Crud !== 'undefined';
        if (estaElModuloCargado && Definido(Crud.crudMnt)) {
            parametros.push(new Parametro(Ajax.Param.creandoEnCrud, Crud.crudMnt.EstoyCreando));
            parametros.push(new Parametro(Ajax.Param.descriptor, Crud.crudMnt.Descriptor));

            const vista = ApiControl.BuscarValorDelAtributo(lista, atControl.vistaMvc)
            parametros.push(new Parametro(Ajax.Param.Vista, Definido(vista) ? vista : Crud.crudMnt.VistaMvc));
        }
        else
            parametros.push(new Parametro(Ajax.Param.creandoEnCrud, false));

        let orden = lista.getAttribute(atControl.ordenarPor);
        if (!IsNullOrEmpty(orden))
            parametros.push(new Parametro(Ajax.Param.ordenarPor, orden));

        let negocio = lista.getAttribute(literal.negocio);
        if (Definido(negocio) && negocio !== ltrNegocioSe.Enumerado.NoDefinido) parametros.push(new Parametro(Ajax.Param.nombreDeNegocio, negocio));

        let accion = lista.getAttribute(atListas.antesDeBuscar);
        if (Definido(accion))
            EvaluarConParametros('ApiDeListasDinamicas.Cargar', accion, [filtros, parametros]);

        let clase: string = lista.getAttribute(atListasDinamicas.claseElemento);
        let idInput: string = lista.getAttribute(literal.id);

        let datosDeEntrada: string = `{"ClaseDeElemento":"${clase}", "IdInput":"${idInput}", "buscada":"${lista.value}"}`;
        let controlador: string = lista.getAttribute(atControl.controlador);

        return { cargar: true, controlador: controlador, filtros: filtros, parametros: parametros, datosDeEntrada: datosDeEntrada };
    }

    function sePuedeCargarLista(lista: HTMLInputElement, necesitaAlgoEscrito: boolean): boolean {

        if (document.readyState !== "complete")
            return false;

        let controlador: string = lista.getAttribute(atControl.controlador);
        if (IsNullOrEmpty(controlador)) {
            alert(`Función de carga obsoleta, indique el atributo controlodar en la lista dinámica ${lista.id}`);
            return false;
        }
        if (lista.getAttribute(atListasDinamicas.cargando) === 'S') {
            return false;
        }

        var caracteresNecesarios = Numero(lista.getAttribute(atListasDinamicas.longitudNecesaria))
        if (necesitaAlgoEscrito && (IsNullOrEmpty(lista.value) || lista.value.length < caracteresNecesarios))
            return false;

        let idsel: string = lista.getAttribute(atListasDinamicas.idSeleccionado);
        let criterio: string = lista.getAttribute(atListasDinamicas.criterio);
        if (Numero(idsel) > 0) {

            if (Tipos.ListaDinamica.Opciones(lista)?.keys.length === 0)
                return true;

            const ultimaBuscada: string = lista.getAttribute(atListasDinamicas.ultimaCadenaBuscada);
            if (!IsNullOrEmpty(ultimaBuscada)) {
                if (criterio === atCriterio.contiene && ultimaBuscada.includes(lista.value))
                    return false;
                if (criterio === atCriterio.comienza && ultimaBuscada.startsWith(lista.value))
                    return false;
            }
        }

        return true;
    }

    function DefinirFiltroListaDinamica(input: HTMLInputElement, necesitoAlgoEscrito: boolean): Array<ClausulaDeFiltrado> {
        let valor: string = input.value;
        if (necesitoAlgoEscrito) {
            let longitud: number = Numero(input.getAttribute(atListasDinamicas.longitudNecesaria));

            if (longitud == 0)
                longitud = 3;

            if (valor.length < longitud)
                return null;
        }

        let filtros: Array<ClausulaDeFiltrado> = AnadirRestrictores(input);
        AnadirRestrictoresDeVinculacion(input, filtros);

        let buscarPor: string = input.getAttribute(atListasDinamicas.buscarPor);

        let criterio: string = input.getAttribute(atListasDinamicas.criterio);
        //if (!IsNullOrEmpty(valor)) {
        let clausula: ClausulaDeFiltrado = new ClausulaDeFiltrado(buscarPor, criterio, valor);
        filtros.push(clausula);
        //}
        return filtros;
    }

    function AnadirRestrictoresDeVinculacion(input: HTMLInputElement, filtros: Array<ClausulaDeFiltrado>): void {
        let elemento = input.parentElement;
        while (Definido(elemento)) {
            if (elemento.getAttribute(atControl.tipoModal) === enumTipoDeModal.ModalParaVincular) {
                let idNegocio = elemento.getAttribute(atModal.idNegocio);
                let id = elemento.getAttribute(atModal.idElemento1);
                filtros.push(new ClausulaDeFiltrado(literal.filtro.vincularCon, atCriterio.restringido, `${idNegocio};${id}`));
                break;
            }
            elemento = elemento.parentElement;
        }
    }


    function AnadirRestrictores(input: HTMLInputElement): Array<ClausulaDeFiltrado> {
        var filtros = new Array<ClausulaDeFiltrado>();

        let restrictores: Array<ClausulaDeFiltrado> = ApiDeFiltro.RestringirPorOtrosControles(input);
        for (let i = 0; i < restrictores.length; i++) {
            const restrictor = restrictores[i];
            if ((EsNumero(restrictor.valor) && Numero(restrictor.valor) > 0) || (restrictor.valor.length > 0))
                filtros.push(restrictor);
        }

        let restrictoresFijos = input.getAttribute(atListasDinamicas.RestrictorFijo);
        if (Definido(restrictoresFijos)) {
            IncluirRestrictoresFijos(restrictoresFijos, input, filtros);
        }
        let soloEnAlta = input.getAttribute(atListasDinamicas.SoloEnAlta);
        if (Definido(soloEnAlta)) {
            let clausula = new ClausulaDeFiltrado(Ajax.Param.SoloEnAlta, atCriterio.igual, `${soloEnAlta}`);
            filtros.push(clausula);
        }
        return filtros;
    }

    function IncluirRestrictoresFijos(restrictoresFijos: string, input: HTMLInputElement, filtros: ClausulaDeFiltrado[]) {
        let restrictores = restrictoresFijos.split('|');
        for (let i: number = 0; i < restrictores.length; i++)
            IncluirRestrictorFijo(restrictores[i], input, filtros);
    }

    function IncluirRestrictorFijo(restrictorFijo: string, input: HTMLInputElement, filtros: ClausulaDeFiltrado[]) {
        let partes = restrictorFijo.split(';');
        if (partes.length < 2) {
            MensajesSe.EmitirMensajeDeExcepcion("IncluirRestrictorFijo",
                `Un filtro fijo en una lista dinámica se define como una cadena de dos partes, separadas por ; el nombre del filtro, y los restrictores. Corrija la lista ${input.id}`);
        }
        let resto = partes[1];
        for (let i: number = 2; i < partes.length; i++)
            resto = `${resto};${partes[i]}`;
        let clausula = new ClausulaDeFiltrado(partes[0], atCriterio.igual, `${resto}`);
        filtros.push(clausula);
    }

    export function Blanquearlas(panel: HTMLDivElement) {
        let listas: NodeListOf<HTMLInputElement> = panel.querySelectorAll(`input[${atControl.tipo}="${ltrTipoControl.ListaDinamica}"]`) as NodeListOf<HTMLInputElement>;
        for (let i = 0; i < listas.length; i++) {
            //ApiListaDinamica.BorrarOpcionesListaDinamica(listas[i]);
            ApiListaDinamica.Blanquear(listas[i]);
        }
    }

    export function Blanquear(lista: HTMLInputElement, ejecutarTrasBlanquear = false) {
        Tipos.ListaDinamica.Resetear(lista);
        lista.value = '';
        lista.setAttribute(atListasDinamicas.idSelAlEntrar, '0');
        lista.setAttribute(atListasDinamicas.idSeleccionado, '0');
        lista.setAttribute(atListasDinamicas.ultimaCadenaBuscada, '');
        lista.setAttribute(atListasDinamicas.cargando, 'N');
        lista.innerHTML = "";
        if (EsTrue(lista.getAttribute(atControl.editable))) {
            lista.readOnly = false;
            lista.disabled = false;
        }
        MapearAlControl.DefinirNavegador(lista, 0);
        if (ejecutarTrasBlanquear) {
            let accion = lista.getAttribute(atListas.trasBlanquear);
            if (Definido(accion))
                Evaluar('ApiDeListasDinamicas.DefinirNavegador', accion, accion.includes('this') ? lista : undefined);
        }
    }

    // Modificar la función ListaDinamica_AnadirOpciones
    function ListaDinamica_AnadirOpciones(peticion: ApiDeAjax.DescriptorAjax, fijarSiSoloHayUna: boolean = false) {
        let datosDeEntrada: Tipos.DatosPeticionDinamica = JSON.parse(peticion.DatosDeEntrada);
        let listaHtml: HTMLInputElement = document.getElementById(datosDeEntrada.IdInput) as HTMLInputElement;

        try {
            const listaDinamica = Tipos.ListaDinamica.Obtener(listaHtml);

            // Vaciar opciones anteriores
            listaDinamica.Vaciar();
            OpcionesDeLasListas.Vaciar(listaHtml.id);
            listaDinamica.UltimaBusqueda = listaHtml.value;

            let mostrarExpresion = listaHtml.getAttribute(atListasDinamicas.mostrarExpresion);
            let expresion: string = "";

            listaDinamica.AnadirOpcionesLeidas(peticion.resultado.datos);
            listaDinamica.AnadirOpcionLeerMas(peticion.resultado.datos.length);

            //for (var i = 0; i < peticion.resultado.datos.length; i++) {
            //    expresion = ParsearExpresion(peticion.resultado.datos[i], mostrarExpresion.toLocaleLowerCase());
            //    let valor: number = peticion.resultado.datos[i].id;

            //    if (NoDefinido(valor))
            //        MensajesSe.EmitirMensajeDeExcepcion("Añadir opciones a la lista dinámica", "No se ha definido el ID tras leer elementos en el servidor");

            //    const opcionDiv: HTMLDivElement = listaDinamica.AgregarOpcion(valor, expresion, peticion.resultado.datos[i]);

            //    if (Definido(opcionDiv)) {
            //        OpcionesDeLasListas.AgregarOpcion(listaHtml.id, peticion.resultado.datos[i]);
            //    }
            //}

            if (fijarSiSoloHayUna && peticion.resultado.datos.length === 1) {
                expresion = ParsearExpresion(peticion.resultado.datos[0], mostrarExpresion.toLocaleLowerCase());
                let valor: number = peticion.resultado.datos[0].id;
                MapearAlControl.ListaDinamica(listaHtml, valor, expresion);
                ConfigurarListaDinamicaTrasAsignarValor(listaHtml);
            }
            else {
                listaDinamica.MostrarDropdown();
            }
        }
        catch (error) {
            MensajesSe.Error("Al añadir opciones a la lista", error.message);
        }
        finally {
            listaHtml.setAttribute(atListasDinamicas.cargando, 'N');
            listaHtml.setAttribute(atListasDinamicas.ultimaCadenaBuscada, datosDeEntrada.buscada);
        }
    }

    function ListasDinamica_SiHayErrorAlCargar(peticion: ApiDeAjax.DescriptorAjax) {
        if (Definido(peticion['message']))
            MensajesSe.Error("SiHayErrorAlCargarListasDinamicas", peticion['message']);
        else {
            let datosDeEntrada: Tipos.DatosPeticionDinamica = JSON.parse(peticion.DatosDeEntrada);
            let listaHtml: HTMLInputElement = document.getElementById(datosDeEntrada.IdInput) as HTMLInputElement;
            const listaDinamica = Tipos.ListaDinamica.Obtener(listaHtml);
            listaDinamica.Vaciar();
            OpcionesDeLasListas.Vaciar(listaHtml.id);
            MensajesSe.Error("SiHayErrorAlCargarListasDinamicas", peticion.resultado.mensaje, peticion.resultado.consola);
            listaHtml.setAttribute(atListasDinamicas.ultimaCadenaBuscada, '');
            listaHtml.setAttribute(atListasDinamicas.cargando, 'N');
        }
    }

    export function BuscarOpciones(lista: HTMLInputElement, valor: string): number {

        // 1. Obtener la instancia de ListaDinamica asociada a este input
        const ldInstance: Tipos.ListaDinamica = Tipos.ListaDinamica.Obtener(lista);

        // 2. Delegar la búsqueda al método interno de la instancia
        // El método BuscarSeleccionado de la instancia ya tiene acceso al mapa _opciones.
        const idEncontrado = ldInstance.BuscarSeleccionado(valor);

        if (idEncontrado === 0) {
            // Opcional: Mantener el mensaje de error o log si es necesario
            // MensajesSe.EmitirExcepcion("Buscar opción en lista", `No se ha localizado el valor ${valor} en la lista ${lista.id}`);
        }

        return idEncontrado;
    }

    export function BlanquearDependientes(control: HTMLInputElement) {
        let dependientes: string = control.getAttribute(atListasDinamicas.BlanquearControlAsociado);
        if (!IsNullOrEmpty(dependientes)) {
            let controles = dependientes.split('|');
            for (let i: number = 0; i < controles.length; i++)
                BlanquearDependiente(control, controles[i].trim());
        }
    }

    export function Bloquear_Blanquear(panel: HTMLDivElement, propiedad: string): HTMLInputElement {
        let lista: HTMLInputElement = ApiControl.BlanquearListaDinamicaPorPropiedad(panel, propiedad);
        if (Definido(lista)) ApiControl.BloquearListaDinamica(lista);
        return lista;
    }

    export function BloquearBlanquear(lista: HTMLInputElement) {
        ApiListaDinamica.Blanquear(lista);
        ApiControl.BloquearListaDinamica(lista);
    }

    function BlanquearDependiente(lista: HTMLInputElement, dependiente: string): void {
        let contenedor: string = lista.getAttribute(atListasDinamicas.ContenidoEn);
        let divContenedor: HTMLDivElement = document.getElementById(contenedor) as HTMLDivElement;
        let controlDependiente: HTMLInputElement = divContenedor.querySelector(`[${atControl.propiedad}=${dependiente}]`);
        if (NoDefinido(controlDependiente) || controlDependiente.disabled)
            return;
        let esEditable = controlDependiente.getAttribute(atControl.editable);
        if (Definido(esEditable) && !EsTrue(esEditable))
            return;

        let tipo: string = controlDependiente.getAttribute(atControl.tipo);
        switch (tipo) {
            case ltrTipoControl.restrictorDeEdicion:
                ApiControl.Editor_Limpiar(controlDependiente);
                break;
            case ltrTipoControl.Editor:
                ApiControl.BlanquearEditor(controlDependiente);
                break;
            case ltrTipoControl.ListaDinamica:
                LimpiarSiNoBloqueada(controlDependiente);
                break;
            default: MensajesSe.Info(`No se ha definido para el tipo ${tipo} como blanquear el control dependiente ${dependiente} de la lista dínamica ${lista.id} `)
        }
    }

    function LimpiarSiNoBloqueada(lista: HTMLInputElement): boolean {
        if (lista.disabled)
            return false;

        MapearAlControl.ListaDinamica(lista, 0, "");
        return true;
    }

    export async function EsperarCarga(control: HTMLSelectElement) {
        let esperando = true;
        let veces = 0;
        while (esperando) {
            if (veces > 10) {
                console.log('He tenido que esperar más de 20sg para cargar el valor de la lista: ' + control.id);
                break;
            }
            await new Promise(resolve => setTimeout(resolve, 2000));
            esperando = EsTrue(control.getAttribute(atListasDeElemento.Cargando));
            veces++;
        }
    }



}
