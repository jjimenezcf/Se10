namespace ApiDeAjax {

    export class ResultadoJson {
        estado: number|string;
        mensaje: string;
        consola: string;
        total: number;
        datos: any;
        modoDeAcceso: string;
        error: boolean;
        logout: boolean;
    }

    export class ResultadoHtml extends ResultadoJson {
        html: string;
    }

    export enum TipoPeticion {
        Sincrona,
        Asincrona
    }

    function EsAsincrona(valor: TipoPeticion): boolean {
        if (valor === TipoPeticion.Sincrona)
            return false;
        return true;
    }

    export enum ModoPeticion {
        Get,
        Post
    }

    function ParsearModo(modo: ModoPeticion) {
        if (modo === ModoPeticion.Get)
            return 'get';
        return 'post';
    }

    export class DescriptorAjax {
        private _tipoPeticion: TipoPeticion;
        private _modoPeticion: ModoPeticion;
        private _req: XMLHttpRequest;
        private _url: string;
        private _datosPost: FormData;
        private _tipoDeEspera: number = 120000;

        public llamador: any;
        public nombre: string;
        public DatosDeEntrada: any;
        public resultado: ResultadoJson | ResultadoHtml;
        public Error: boolean = false;

        public get Tipo(): TipoPeticion { return this._tipoPeticion; }
        public get Request(): XMLHttpRequest { return this._req; }
        public get Url(): string { return this._url; }
        public get Modo(): ModoPeticion { return this._modoPeticion; }

        public set DatosPost(datos: FormData) { this._datosPost = datos; }

        public TrasLaPeticion: Function;
        public ProcesarError: Function;
        public ConCapa: boolean;
        constructor(llamante: any
            , peticion: string
            , datosDeEntrada: any
            , url: string
            , tipo: TipoPeticion
            , modo: ModoPeticion
            , trasLaPeticion: Function
            , siHayError: Function
            , conCapa: boolean = true) {
            this.ConCapa = conCapa;
            this.llamador = llamante;
            this.nombre = peticion;
            this.DatosDeEntrada = datosDeEntrada;
            this.resultado = undefined;
            this._tipoPeticion = tipo;
            this._modoPeticion = modo;
            this.Inicializar(url, trasLaPeticion, siHayError);
        }

        private ParsearRespuesta() {
            try {
                if (this.Request.response.includes('mensaje') && this.Request.response.includes('datos') && this.Request.response.includes('estado'))
                    this.resultado = JSON.parse(this.Request.response);
                else {
                    this.resultado = new ResultadoJson();
                    this.resultado.datos = this.Request.response;
                }
            }
            catch
            {
                MensajesSe.Error("ParsearRespuesta", `Error al procesar la respuesta de ${this.nombre}`);
            }
        }

        public Inicializar(url: string, trasLaPeticion: Function, siHayError: Function) {
            this._req = new XMLHttpRequest();
            this._url = url;
            this.TrasLaPeticion = trasLaPeticion;
            this.ProcesarError = siHayError;
        }

        public Ejecutar(parametros: string = null) {
            this.PeticionAjax(parametros);
            if (this.Error) throw `${this.resultado.mensaje}`;
        }

        private PeticionAjax(parametros: string) {

            function RespuestaCorrecta(descriptor: DescriptorAjax) {
                try {
                    if (descriptor.Request.status === 500)
                        descriptor.Error500();
                    else
                    if (IsNullOrEmpty(descriptor.Request.response))
                        descriptor.ErrorEnPeticion();
                    else {
                        descriptor.ParsearRespuesta();

                        if (descriptor.resultado === undefined || descriptor.resultado.estado === Ajax.jsonResultError || descriptor.resultado.estado === Ajax.Error)
                            descriptor.ErrorEnPeticion();
                        else
                            descriptor.DespuesDeLaPeticion();
                    }
                }
                finally {
                    if (descriptor.ConCapa)
                        QuitarCapa();
                }
            }
            function RespuestaErronea(descriptor: DescriptorAjax) {
                try {
                    if (descriptor.Request.status === 413) {
                        console.error('El archivo es demasiado grande para ser cargado.');
                        // Informar al usuario
                    } else {
                        descriptor.ErrorEnPeticion();
                    }
                }
                finally {
                    if (descriptor.ConCapa)
                        QuitarCapa();
                }
            }

            function RespuestaPorTimeout(descriptor: DescriptorAjax) {
                try {
                    console.log('La petición ha excedido el tiempo de espera.')
                    MensajesSe.Info('La petición ha excedido el tiempo de espera.');
                }
                finally {
                    if (descriptor.ConCapa)
                        QuitarCapa();
                }
            }

            this.Request.addEventListener(Ajax.eventoLoad, () => RespuestaCorrecta(this));
            this.Request.addEventListener(Ajax.eventoError, () => RespuestaErronea(this));
            this.Request.addEventListener('timeout', () => RespuestaPorTimeout(this));
            this.Request.open(ParsearModo(this.Modo), this.Url, EsAsincrona(this.Tipo));
            this.Request.timeout = this._tipoDeEspera
            //this.Request.setRequestHeader('Access-Control-Allow-Origin', 'http:192.168.0.1');
            this.Request.setRequestHeader('cache-control', 'no-cache');

            if (EsAsincrona(this.Tipo) && this.ConCapa) {
                PonerCapa();
            }

            if (this._datosPost != undefined)
                this.Request.send(this._datosPost);
            else if (parametros !== null) {
                this.Request.setRequestHeader("Content-Type", "application/json");
                this.Request.send(parametros);
            }
            else
                this.Request.send();
        }

        private ErrorEnPeticion() {
            this.Error = true;
            if (this.Request.status === 404) {
                this.resultado = new ResultadoJson();
                this.resultado.mensaje = `Error al acceder al servidor`;
                console.error(`Error al ejecutar la peticion '${this.nombre}'. Petición no definida. No está definida la petición con los parámetros indicados: ${this.Url}`);
            }
            else if (this.Request.status === 401) {
                EntornoSe.NavegarAUrl(`${window.location.origin}/${ltrControladores.Entorno.Acceso}/${ltrVistas.Entorno.Conectar}.html`);
            }
            else if (this.Request.status === 413) {
                this.resultado = new ResultadoJson();
                this.resultado.mensaje = 'El archivo es demasiado grande para ser cargado.';
            }
            else  {
                if (NoDefinido(this.Request.response)) {
                    this.resultado = new ResultadoJson();
                    this.resultado.mensaje = `Error al ejecutar la peticion '${this.nombre}'.`;
                }
                else {
                    try {
                        this.resultado = JSON.parse(this.Request.response);
                    }
                    catch {
                        this.resultado = new ResultadoJson();
                        this.resultado.mensaje = `Error al ejecutar la peticion '${this.nombre}'. código: ${this.Request.status}`;
                    }
                }

                if (NoDefinido(this.resultado)) {
                    this.resultado.mensaje = `Error al ejecutar la peticion '${this.nombre}'. Petición no realizada`;
                    this.resultado.consola = `Error al ejecutar la peticion '${this.nombre}'. Petición no realizada`;
                }
                else {
                    if (this.resultado.logout) {
                        let logoutButton = document.getElementById('logout-button') as HTMLButtonElement;
                        logoutButton.dispatchEvent(new MouseEvent('click', { bubbles: true }));
                    }
                    else {
                        if (IsNullOrEmpty(this.resultado.consola))
                            this.resultado.consola = `Error al ejecutar la peticion '${this.nombre}'`;
                        if (IsNullOrEmpty(this.resultado.mensaje))
                            this.resultado.mensaje = `Error al ejecutar la peticion '${this.nombre}'. Petición no realizada`;
                    }
                }

            }

            if (this.ProcesarError)
                this.ProcesarError(this);
        }

        private Error500() {
            this.resultado = new ResultadoJson();
            let mensajedeConsola: string = `Petición mal definida: ${this.Url}`;

            if (NoDefinido(this.Request.response) || EsString(this.Request.response))
                mensajedeConsola = mensajedeConsola + '.' + this.Request.response;

            else
                mensajedeConsola = mensajedeConsola + '.' + JSON.parse(this.Request.response);

            this.resultado.mensaje = `Error al ejecutar la peticion '${this.nombre}'. Acceda a la consola`;
            console.error(mensajedeConsola);

            if (this.ProcesarError)
                this.ProcesarError(this);
        }

        private DespuesDeLaPeticion() {

            if (this.Request.status >= 200 && this.Request.status < 300) {
                try {
                    if (this.Request.response) {
                        this.resultado = JSON.parse(this.Request.response);
                        MensajesSe.Info(this.resultado?.mensaje, this.resultado?.consola);
                        if (this.TrasLaPeticion)
                            try {
                                this.TrasLaPeticion(this);
                            }
                            catch (error) {
                                MensajesSe.Error("DespuesDeLaPeticion", `Error al procesar la peticion ${this.nombre}`, error.message);
                                throw error;
                            }
                    }
                    else {
                        console.log('Respuesta vacía pero exitosa');
                    }
                }
                catch (error) {
                    console.error('Error al analizar la respuesta JSON:', error);
                    this.ErrorEnPeticion();
                }
            }
            else {
                console.error(`Error en la petición: Estado ${this.Request.status}`);
                this.ErrorEnPeticion();
            }
        }
    }

    export function ErrorTrasPeticion(origen: string, peticion: ApiDeAjax.DescriptorAjax) {

        if (EsObjetoDe(peticion, ApiDeAjax.DescriptorAjax))
            MensajesSe.EmitirExcepcion(origen, peticion.resultado.mensaje, peticion.resultado.consola);
        else
            if (EsError(peticion))
                MensajesSe.EmitirExcepcion(origen, ((peticion as unknown) as Error).message, ((peticion as unknown) as Error).stack);
            else
                MensajesSe.EmitirExcepcion(origen, `${peticion}`);

    }




}