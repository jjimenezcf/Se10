namespace ApiLocalStorage {

    const CLAVE_OPERACION = 'estado-operacion';
    const TIGGER_EJECUTAR = 'tigger-ejecutar'
    const INTERVALO_EN_MLS = 3000;
    const ARCHIVOS_SELECCIONADOS = 'archivos-seleccionados';
    const PADRE = 'padre-archivos';
    const RECARGAR_ORIGEN = 'recargar-origen';
    const ID_FORMULARIO = 'id_formulario'
    let intervalId: number;
    let NameOf_IniciarOperacion: string = 'IniciarOperacion';

    export function IniciarOperacion(boton: HTMLButtonElement, origenCopia: [number, number], listaIds: Array<number>) {
        var guid = generarUUID();
        boton.setAttribute(atControl.Guid, guid);

        var operacion = OperacionQueSeQuiereRealizar(boton);

        const state = {
            guid: guid,
            operacion: operacion,
            timestamp: Date.now()
        };
        localStorage.setItem(CLAVE_OPERACION, JSON.stringify(state));
        ApiLocalStorage.AlmacenarInformacioDeArchivos(origenCopia, listaIds);

        // Añade esta línea para activar el trigger
        localStorage.setItem(TIGGER_EJECUTAR, NameOf_IniciarOperacion);
        PararTesteoDeRecargarArchivos();
        // Ejecuta ActivarProceso en la pestaña actual
        ActivarProceso();
        IniciarTesteoDeRecargarArchivos();
    }

    export function IniciarTesteoDeRecargarArchivos(guid: string = '') {
        RecargarOrigen(false);
        localStorage.setItem(ID_FORMULARIO, guid );
        TestearSiSeHaDeRecargar(guid);
    }

    export function PararTesteoDeRecargarArchivos() {
        localStorage.removeItem(RECARGAR_ORIGEN);
        localStorage.removeItem(ID_FORMULARIO);
    }


    export function GuidDelFormularioOrigen(): string {
       return localStorage.getItem(ID_FORMULARIO);
    }

    export function ActivarRecargarArchivos() {
        RecargarOrigen(true);
    }

    function RecargarOrigen(valor: boolean) {
        localStorage.setItem(RECARGAR_ORIGEN, valor ? 'S' : 'N');
    }

    export function OperacionQueSeQuiereRealizar(boton: HTMLButtonElement): string {
        return boton.classList.contains(ltrCss.Archivos.Copiar)
            ? atArchivo.Operacion.Copiar
            : boton.classList.contains(ltrCss.Archivos.Mover)
                ? atArchivo.Operacion.Mover
                : atArchivo.Operacion.Enlazar;

    }

    export function FinalizarOperacionConArchivos() {
        ResetearOperacionConArchivos();
    }

    export function Guid(): string {
        const stateString = localStorage.getItem(CLAVE_OPERACION);
        if (stateString) {
            const state = JSON.parse(stateString);
            return state.guid;
        }
        return undefined;
    }

    export function Operacion(): string {
        const stateString = localStorage.getItem(CLAVE_OPERACION);
        if (stateString) {
            const state = JSON.parse(stateString);
            return state.operacion;
        }
        return undefined;
    }

    export function ArchivosSeleccionados(): Array<number> {
        const archivos = localStorage.getItem(ARCHIVOS_SELECCIONADOS);
        if (archivos) try {
            return JSON.parse(archivos);
        }
            catch (error) {
                ResetearOperacionConArchivos();
            }
        return undefined;
    }

    export function ActivarProceso() {
        if (Definido(intervalId))
            return;
        intervalId = setInterval(sincronizarIu, INTERVALO_EN_MLS);
        sincronizarIu();

        setTimeout(() => {
            localStorage.removeItem(TIGGER_EJECUTAR);
        }, 100);

    }
    export function RefrescarIu() {
        sincronizarIu();
    }

    function TestearSiSeHaDeRecargar(guid: string) {
        var valor = localStorage.getItem(RECARGAR_ORIGEN);
        if (valor === 'S') {
            //if (localStorage.getItem(ID_FORMULARIO) != guid)
            ApiDeArchivos.RecargarMostrarArchivosAnexados();
            PararTesteoDeRecargarArchivos();
        }
        else {
            setTimeout(() => { TestearSiSeHaDeRecargar(guid); }, INTERVALO_EN_MLS);
        }
    }

    export function ExisteOrigenCopia(): boolean {
        return Definido(localStorage.getItem(PADRE))
    }

    export function PadreDeLosArchivos(): [number, number] {
        const padreAlmacenado: string | null = localStorage.getItem(PADRE);
        try {
            const padre: [number, number] = JSON.parse(padreAlmacenado);
            return padre;
        }
        catch (error) {
            ResetearOperacionConArchivos();
        }
        return undefined;
    }

    export function AlmacenarInformacioDeArchivos(origenCopia: [number, number], listaIds: Array<number>) {
        localStorage.setItem(PADRE, JSON.stringify(origenCopia));
        localStorage.setItem(ARCHIVOS_SELECCIONADOS, JSON.stringify(listaIds));
    }

    function ResetearOperacionConArchivos() {
        localStorage.removeItem(CLAVE_OPERACION);
        localStorage.removeItem(PADRE);
        localStorage.removeItem(ARCHIVOS_SELECCIONADOS);
        localStorage.removeItem(TIGGER_EJECUTAR);
        sincronizarIu();
    }

    function sincronizarIu() {
        const selectorDeArchivos = document.querySelector('.' + ltrCss.Archivos.SelectorDeArchivos) as HTMLDivElement;
        if (Definido(selectorDeArchivos)) {
            const stateString = localStorage.getItem(CLAVE_OPERACION);
            if (stateString) {
                sincronizarBoton(selectorDeArchivos, atArchivo.Operacion.Copiar, stateString);
                sincronizarBoton(selectorDeArchivos, atArchivo.Operacion.Mover, stateString);
                sincronizarBoton(selectorDeArchivos, atArchivo.Operacion.Enlazar, stateString);
            }
            else {
                detenerVerificacion();
                resetearBoton(selectorDeArchivos, atArchivo.Operacion.Copiar);
                resetearBoton(selectorDeArchivos, atArchivo.Operacion.Mover);
                resetearBoton(selectorDeArchivos, atArchivo.Operacion.Enlazar);
            }
        }
    }

    function sincronizarBoton(selectorDeArchivos: HTMLDivElement, operacion: string, stateString: string): boolean {

        const state = JSON.parse(stateString);
        const tiempoTranscurrido = Date.now() - state.timestamp;

        if (tiempoTranscurrido > 60000) {
            FinalizarOperacionConArchivos();
            return true;
        }

        const idDeBoton = selectorDeArchivos.id + '-' + operacion;
        const boton = document.getElementById(idDeBoton) as HTMLButtonElement;
        if (state.operacion === operacion) {
            if (state.guid === boton.getAttribute(atControl.Guid)) {
                boton.classList.add(ltrCss.Archivos.CancelarOperacion);
                boton.setAttribute(atControl.title, 'Cancelar ' + operacion)
            } else {
                boton.setAttribute(atControl.title, CapitalizarPrimeraLetra(operacion) + 'seleccionados')
                boton.classList.add(ltrCss.Archivos.Pegar);
            }
            deshabilitarRestoDeBotones(selectorDeArchivos, operacion)
            return true;
        }
        return false;
    }

    function deshabilitarRestoDeBotones(selectorDeArchivos: HTMLDivElement, operacion: string) {
        if (operacion === atArchivo.Operacion.Copiar) {
            deshabilitarBoton(selectorDeArchivos, atArchivo.Operacion.Mover);
            deshabilitarBoton(selectorDeArchivos, atArchivo.Operacion.Enlazar);
        } else
            if (operacion === atArchivo.Operacion.Mover) {
                deshabilitarBoton(selectorDeArchivos, atArchivo.Operacion.Copiar);
                deshabilitarBoton(selectorDeArchivos, atArchivo.Operacion.Enlazar);
            } else
                if (operacion === atArchivo.Operacion.Enlazar) {
                    deshabilitarBoton(selectorDeArchivos, atArchivo.Operacion.Mover);
                    deshabilitarBoton(selectorDeArchivos, atArchivo.Operacion.Copiar);
                }
    }

    function deshabilitarBoton(selectorDeArchivos: HTMLDivElement, operacion: string) {
        const idDeBoton = selectorDeArchivos.id + '-' + operacion;
        let boton = document.getElementById(idDeBoton) as HTMLButtonElement;
        ApiControl.BloquearBoton(boton);

    }
    function detenerVerificacion() {
        if (intervalId) {
            clearInterval(intervalId);
            intervalId = undefined;
        }
    }

    function resetearBoton(selectorDeArchivos: HTMLDivElement, operacion: string) {
        const idDeBoton = selectorDeArchivos.id + '-' + operacion;
        const boton = document.getElementById(idDeBoton) as HTMLButtonElement;
        boton.removeAttribute(atControl.Guid);
        boton.classList.remove(ltrCss.Archivos.CancelarOperacion);
        boton.classList.remove(ltrCss.Archivos.Pegar);
        boton.setAttribute(atControl.title, CapitalizarPrimeraLetra(operacion) + ' archivos seleccionados')
        HabilitarBoton(selectorDeArchivos, atArchivo.Operacion.Copiar);
        HabilitarBoton(selectorDeArchivos, atArchivo.Operacion.Enlazar);
        HabilitarBoton(selectorDeArchivos, atArchivo.Operacion.Mover);
    }
    function HabilitarBoton(selectorDeArchivos: HTMLDivElement, operacion: string) {
        const idDeBoton = selectorDeArchivos.id + '-' + operacion;
        let boton = document.getElementById(idDeBoton) as HTMLButtonElement;
        ApiControl.DesbloquearBoton(boton);

    }

    intervalId = setInterval(sincronizarIu, INTERVALO_EN_MLS);

    // Escuchar cambios en localStorage
    window.addEventListener('storage', (event) => {
        if (event.key === TIGGER_EJECUTAR) {
            // Obtener el identificador de la función
            const funcionId = event.newValue;

            // Ejecutar la función correspondiente
            if (funcionId === NameOf_IniciarOperacion) {
                ActivarProceso();
            }
        }
    });


    const CLAVE_FICHADA = 'estado-fichada';
    const CANAL_FICHADA = 'canal_fichada';
    const MENSAJE_FICHADA = 'cambio_fichada';

    export function CambiarTextoFichada() {
        const enlaceFichada = document.querySelector('a[onclick="EntornoSe.Fichar(); return false"]');
        if (enlaceFichada) {
            const nuevoTexto = enlaceFichada.textContent.trim() === 'Fichar entrada' ? 'Fichar salida' : 'Fichar entrada';
            enlaceFichada.textContent = nuevoTexto;
            localStorage.setItem(CLAVE_FICHADA, nuevoTexto);

            // Enviar mensaje a otras pestañas
            const bc = new BroadcastChannel(CANAL_FICHADA);
            bc.postMessage({ tipo: MENSAJE_FICHADA, texto: nuevoTexto });
        }
        else {
            window.location.reload();
        }
    }

    const bc = new BroadcastChannel(CANAL_FICHADA);
    bc.onmessage = (event) => {
        if (event.data.tipo === MENSAJE_FICHADA) {
            actualizarTextoFichada(event.data.texto);
        }
    };

    function actualizarTextoFichada(nuevoTexto: string) {
        const enlaceFichada = document.querySelector('a[href="javascript: EntornoSe.Fichar()"]');
        if (enlaceFichada) {
            enlaceFichada.textContent = nuevoTexto;
        }
    }

}

