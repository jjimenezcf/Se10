namespace MensajesSe {

    export enum enumTipoMensaje { informativo, advertencia, error };

    export class clsNotificacion {
        _tipo: enumTipoMensaje;
        public get tipo(): enumTipoMensaje {
            return this._tipo;
        }

        _mensaje: string;
        public get mensaje(): string {
            return this._mensaje;
        }
        constructor(tipo: enumTipoMensaje, mensaje: string) {
            this._tipo = tipo;
            this._mensaje = mensaje;
        }
    }

    class clsMensaje {
        _tipo: enumTipoMensaje;
        public get tipo(): string {
            return this._tipo.toString();
        }

        _mensaje: string;
        public get mensaje(): string {
            return this._mensaje;
        }
        _origen: string;
        public get origen(): string {
            return this._origen;
        }

        _fecha: Date;
        public get fecha(): string {
            return this._fecha.toISOString();
        }

        constructor(tipo: enumTipoMensaje, origen: string, mensaje: string) {
            this._tipo = tipo;
            this._mensaje = mensaje;
            this._fecha = new Date(Date.now());
            this._origen = origen;
        }
    }

    class AlmacenDeMensajes {
        private _mensajes: clsMensaje[] = [];

        public get Mensajes(): clsMensaje[] {
            let mensajesGuardados: string = sessionStorage.getItem('mensajes-guardados');
            if (!(mensajesGuardados === null || mensajesGuardados == undefined)) {
                this._mensajes = JSON.parse(mensajesGuardados);
            };
            return this._mensajes;
        }

        constructor() {
        }

        public Error(origen: string, mensaje: string): void {
            this.Mensajes.push(new clsMensaje(enumTipoMensaje.error, origen, mensaje));
            this.Persistir();
        }

        public Info(mensaje: string): void {
            if (mensaje === 'Leyendo ...' || mensaje === 'Petición realizada' || mensaje === 'El nodo se ha leido')
                return;
            this.Mensajes.push(new clsMensaje(enumTipoMensaje.informativo, 'info', mensaje));
            this.Persistir();
        }

        public Advertencia(mensaje: string): void {
            this.Mensajes.push(new clsMensaje(enumTipoMensaje.advertencia, EntornoSe.Llamador(), mensaje));
            this.Persistir();
        }

        public BorrarMensajes(): void {
            this._mensajes.splice(0, this._mensajes.length);
            this.Persistir();
        }

        private Persistir(): void {
            sessionStorage.setItem('mensajes-guardados', JSON.stringify(_Almacen._mensajes));
        }

    }

    let _Almacen: AlmacenDeMensajes = new AlmacenDeMensajes();

    export function MostraExcepcion(excepcion: any, origen: string = "", tipo: enumTipoMensaje = enumTipoMensaje.error) {
        let mensaje: string;
        let a_consola: string = "";

        if (excepcion instanceof DOMException)
            mensaje = (excepcion as DOMException).message;
        else if (EsError(excepcion)) {
            mensaje = (excepcion as Error).message;
            a_consola = (excepcion as Error).message + newLine + (excepcion as Error).stack;
        }
        else if (ObtenerPropiedad(excepcion, 'resultado', null) !== null) {
            mensaje = excepcion.resultado.mensaje;
            a_consola = excepcion.resultado.consola;
        }
        else if (EsString(excepcion))
            mensaje = excepcion;

        switch (tipo) {
            case enumTipoMensaje.informativo:
                _Almacen.Info(mensaje);
                break;
            case enumTipoMensaje.advertencia:
                _Almacen.Advertencia(mensaje);
                break;
            case enumTipoMensaje.error:
                _Almacen.Error(IsNullOrEmpty(origen) ? "Mostrar Excepción" : origen, mensaje);
                break;
        }
        MensajesSe.Apilar(tipo, mensaje, a_consola);
    }

    export function Info(mensaje: string, consola?: string) {
        if (Definido(mensaje)) {
            if (mensaje !== 'Leyendo ...' && mensaje !== 'Petición realizada' && mensaje !== 'El nodo se ha leido' && !mensaje.includes('vínculo entre'))
                _Almacen.Info(mensaje);
            MensajesSe.Apilar(enumTipoMensaje.informativo, mensaje, consola);
        }
        else if (Definido(consola))
            console.log(consola);
    }

    export function Advertencia(mensaje: string, consola?: string) {
        MensajesSe.Apilar(enumTipoMensaje.advertencia, mensaje, consola);
    }
    export function Error(origen: string, mensaje: string, consola?: string) {
        _Almacen.Error(origen, mensaje);
        if (!Definido(consola))
            consola = `Error originado en '${origen}': '${mensaje}''`;

        MensajesSe.Apilar(enumTipoMensaje.error, mensaje, consola);
    }

    export function EmitirMensajeDeExcepcion(origen: string, mensaje: string, consola?: string) {
        Error(origen, mensaje, consola);
        throw EvalError(mensaje);
    }

    export function EmitirExcepcion(origen: string, mensaje: string, excepcion: any = undefined) {
        var a_consola = undefined;

        if (!NoDefinido(excepcion)) {
            if (excepcion instanceof DOMException)
                a_consola = (excepcion as DOMException);
            else if (EsError(excepcion)) {
                a_consola = (excepcion as Error).message + newLine + (excepcion as Error).stack;
                mensaje = mensaje + newLine + (excepcion as Error).message;
            }
            else if (EsString(excepcion))
                a_consola = excepcion;
        }

        EmitirMensajeDeExcepcion(origen, mensaje, a_consola);
    }

    export function MostrarExcepcion(origen: string, excepcion: any) {
        var a_consola = undefined;
        var mensaje = '';

        if (excepcion instanceof DOMException)
            a_consola = (excepcion as DOMException);
        else if (EsError(excepcion)) {
            a_consola = (excepcion as Error).message + newLine + (excepcion as Error).stack;
            mensaje = (excepcion as Error).message;
        }
        else if (EsString(excepcion)) {
            a_consola = excepcion;
            mensaje = a_consola;
        }

        Error(origen, mensaje, a_consola);
    }

    export function MostrarMensajes() {
        let modal: HTMLDivElement = document.getElementById("id-modal-historial") as HTMLDivElement;
        MapearMensajesAlGrid();
        let contenedor: HTMLDivElement = document.getElementById("id-contenedor-historial") as HTMLDivElement;
        let tabla: HTMLDivElement = document.getElementById('id-historial-cuerpo.tabla') as HTMLDivElement;
        tabla.style.height = `${contenedor.getBoundingClientRect().height - 130}px`;
        modal.style.display = ltrStyle.display.block;
        EntornoSe.AjustarModalesAbiertas();
    }

    export function CerrarHistorial() {
        let modal: HTMLDivElement = document.getElementById("id-modal-historial") as HTMLDivElement;
        ApiPanel.CerrarModal(modal);
    }

    export function BorrarHistorial() {
        _Almacen.BorrarMensajes();
        CerrarHistorial();
        let contenedor: HTMLDivElement = document.getElementById("id-contenedor-historial") as HTMLDivElement;
        let tabla: HTMLDivElement = document.getElementById('id-historial-cuerpo.tabla') as HTMLDivElement;
        tabla.style.height = ``;
        contenedor.style.height = ``;
        EntornoSe.AjustarModalesAbiertas();
    }

    export function MapearMensajesAlGrid() {

        function crearFila(filaCabecera: HTMLDivElement, mensaje: clsMensaje): HTMLDivElement {
            function convertirAHHMMSS(fecha: Date): string {
                const horas = fecha.getUTCHours().toString().padStart(2, '0');
                const minutos = fecha.getUTCMinutes().toString().padStart(2, '0');
                const segundos = fecha.getUTCSeconds().toString().padStart(2, '0');
                return `${horas}:${minutos}:${segundos}`;
            }

            function crearCelda(celdaCabecera: HTMLDivElement, mensaje: clsMensaje): HTMLDivElement {
                let celda: HTMLDivElement = document.createElement("div");
                celda.classList.add(ltrCss.crud.celda);
                let propiedad: string = celdaCabecera.getAttribute('propiedad');
                celda.style.width = celdaCabecera.style.width;
                celda.style.textAlign = celdaCabecera.style.textAlign;

                if (propiedad === '_fecha') {
                    const fecha = new Date(mensaje[propiedad]);
                    celda.textContent = convertirAHHMMSS(fecha);
                } else {
                    celda.textContent = mensaje[propiedad];
                }

                return celda;
            }


            let fila = document.createElement("div");
            fila.classList.add(ltrCss.crud.fila);
            var encolumnado = filaCabecera.querySelectorAll('.' + ltrCss.crud.columna) as NodeListOf<HTMLDivElement>;
            for (let i: number = 0; i < encolumnado.length; i++) {
                let celda: HTMLDivElement = crearCelda(encolumnado[i], mensaje);
                fila.append(celda);
            }
            return fila;
        }

        let tabla: HTMLDivElement = document.getElementById('id-historial-cuerpo.tabla') as HTMLDivElement;
        let cuerpoOld: HTMLDivElement = tabla.querySelector('.' + ltrCss.crud.tbody) as HTMLDivElement;
        tabla.removeChild(cuerpoOld);

        let cuerpo: HTMLDivElement = document.createElement("div");
        cuerpo.id = cuerpoOld.id;
        cuerpo.classList.add(ltrCss.crud.tbody, 'thead-historial-mensajes');
        let filaCabecera: HTMLDivElement = document.getElementById('id-historial-tabla.cabecera.fila') as HTMLDivElement;
        for (let i: number = _Almacen.Mensajes.length - 1; i >= 0; i--) {
            let fila: HTMLDivElement = crearFila(filaCabecera, _Almacen.Mensajes[i]);
            cuerpo.append(fila);
        }
        tabla.append(cuerpo);
    }

    export let Notificaciones: clsNotificacion[];

    export function Apilar(tipo: enumTipoMensaje, mensaje: string, mensajeDeConsola?: string) {
        let n: clsNotificacion = new clsNotificacion(tipo, mensaje);
        if (EsNulo(Notificaciones))
            AsignarMemoria();

        Notificaciones.push(n);
        Notificar(tipo, mensaje, mensajeDeConsola);
    }

    export function Sacar() {
        if (EsNulo(Notificaciones))
            return;

        if (Notificaciones.length > 0) {
            let n = Notificaciones.pop();
            Notificar(n.tipo, n.mensaje);
        }
        else {
            let cadena = 'No hay más mensajes';
            if (mensajeMostrado().indexOf(cadena) > 0)
                BlanquearMensajeMostrado();
            else
                Notificar(enumTipoMensaje.informativo, cadena);
        }
    }

    function AsignarMemoria() {
        MensajesSe.Notificaciones = [] as MensajesSe.clsNotificacion[];
    }

    function Notificar(tipo: enumTipoMensaje, mensaje: any, mensajeDeConsola?: string) {

        if (mensaje.indexOf('Petición realizada') > -1 || mensaje.indexOf('El nodo se ha leido') > -1) {
            console.log(mensaje);
            return;
        }

        const control = <HTMLInputElement>document.getElementById("Mensaje");
        let mensajeConTipo: string = "";

        if (EsError(mensaje))
            mensajeConTipo = `Error: ${(mensaje as Error).message}`;
        else {
            if (Definido(mensaje)) {
                var posicion = enumTipoMensaje.error ? mensaje.indexOf(`Error:`) : mensaje.indexOf(`Informativo:`);
                mensajeConTipo = posicion === -1 ? `${tipo === enumTipoMensaje.informativo ? 'Informativo' : 'Error'}: ${mensaje}` : mensaje;
            }
            else mensajeConTipo = 'Error, acceda a la consola';
        }

        if (control) {
            control.value = `${mensajeConTipo.replace('Informativo: ', '').replace('Error: ', '')}`;

            // Removemos las clases existentes
            ApiControl.ExcluirCss(control, ltrCss.Mensajes.Informativo);
            ApiControl.ExcluirCss(control, ltrCss.Mensajes.Error);

            // Agregamos la nueva clase
            if (tipo === enumTipoMensaje.error) {
                ApiControl.IncluirCss(control, ltrCss.Mensajes.Error);
            } else {
                ApiControl.IncluirCss(control, ltrCss.Mensajes.Informativo);
            }

            if (IsNullOrEmpty(mensajeDeConsola))
                mensajeDeConsola = mensajeConTipo;
            else
                mensajeDeConsola = `${mensaje}${newLine}${mensajeDeConsola}`;
        }
        else
            mensajeDeConsola = `${mensajeConTipo}`

        if (enumTipoMensaje.error === tipo)
            console.error(mensajeDeConsola);
        else
            console.log(mensajeDeConsola);

        // Temporizador para blanquear el mensaje
        setTimeout(BlanquearMensajeMostrado, 9000);
    }

    export function BlanquearMensajeMostrado(): void {
        var control = <HTMLInputElement>document.getElementById("Mensaje");
        if (control) {
            control.value = "";
            control.classList.remove("mensaje-informativo", "mensaje-error"); // Removemos las clases
        }
    }


    function mensajeMostrado(): string {
        var control = <HTMLInputElement>document.getElementById("Mensaje");
        if (control)
            return control.value;
        else "";
    }

}