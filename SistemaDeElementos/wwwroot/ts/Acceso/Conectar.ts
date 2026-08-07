namespace Acceso {

    export let Gestor: GestorDeAcceso = undefined;

    //document.addEventListener("DOMContentLoaded", function () {
    //    const passwordInput = document.getElementById("password");
    //    if (passwordInput)
    //        passwordInput.addEventListener("keydown", function (keyboardEvent) {
    //            if (keyboardEvent.key === "Enter") {
    //                Acceso.AlPulsarConectar();
    //            }
    //        });
    //    else
    //        Acceso.AlCargarPaginaNuevaContrasena();
    //});

    document.addEventListener("DOMContentLoaded", function () {
        // 1. Ocultar el mensaje siempre al iniciar
        OcultarMensaje();

        const passwordInput = document.getElementById("password");
        if (passwordInput !== undefined && passwordInput !== null) {
            passwordInput.addEventListener("keydown", function (keyboardEvent) {
                if (keyboardEvent.key === "Enter") {
                    Acceso.AlPulsarConectar();
                }
            });
        } else {
            Acceso.AlCargarPaginaNuevaContrasena();
        }
    });

    export function AlSalirDeLogin() {
        OcultarMensaje();
        let login: HTMLInputElement = document.getElementById('login') as HTMLInputElement;
        if (IsNullOrEmpty(login.value))
            return;
        Gestor = new GestorDeAcceso(login.value);
    }

    export function AlPulsarConectar() {
        OcultarMensaje();
        if (Gestor === undefined) {
            MostrarMensaje("Debe indicar un usuario");
            return; // Añadido return para evitar flujo si no hay gestor
        }

        let password: HTMLInputElement = document.getElementById('password') as HTMLInputElement;
        if (IsNullOrEmpty(password.value)) {
            MostrarMensaje("Debe indicar una password");
            return;
        }
        Gestor.ValidarAcceso(password.value);
    }

    export function AlPulsarValidarCodigo() {
        OcultarMensaje();
        if (Gestor === undefined) {
            MostrarMensaje("Debe indicar un usuario");
            return;
        }

        let codigo2F: HTMLInputElement = document.getElementById('codigo2F') as HTMLInputElement;
        if (IsNullOrEmpty(codigo2F.value)) {
            MostrarMensaje("Debe indicar el código recibido por correo");
            return;
        }

        let dispositivoDeConfianza: HTMLInputElement = document.getElementById('dispositivoDeConfianza') as HTMLInputElement;

        Gestor.Validar2F(codigo2F.value, dispositivoDeConfianza.checked);
    }

    export function AlPulsarReenviarCodigo() {
        OcultarMensaje();
        if (Gestor === undefined) {
            MostrarMensaje("Debe indicar un usuario");
            return;
        }

        Gestor.Reenviar2F();
    }

    export function AlPulsarCancelar2F() {
        OcultarMensaje();

        let codigo2F: HTMLInputElement = document.getElementById('codigo2F') as HTMLInputElement;
        codigo2F.value = "";

        let dispositivoDeConfianza: HTMLInputElement = document.getElementById('dispositivoDeConfianza') as HTMLInputElement;
        dispositivoDeConfianza.checked = false;

        let divCodigo2F: HTMLDivElement = document.getElementById('div-codigo2F') as HTMLDivElement;
        divCodigo2F.style.display = 'none';

        let login: HTMLInputElement = document.getElementById('login') as HTMLInputElement;
        login.disabled = false;

        let password: HTMLInputElement = document.getElementById('password') as HTMLInputElement;
        password.disabled = false;
        password.value = "";

        let botonConectar: HTMLButtonElement = document.getElementById('IngresoLog') as HTMLButtonElement;
        botonConectar.style.display = '';

        let divOlvideContrasena: HTMLDivElement = document.getElementById('div-olvide-contrasena') as HTMLDivElement;
        divOlvideContrasena.style.display = '';
    }

    export function AlPulsarOlvideContrasena() {
        OcultarMensaje();
        if (Gestor === undefined) {
            MostrarMensaje("Debe indicar un usuario");
            return;
        }
        Gestor.SolicitarNuevaContrasena();
    }

    export function AlCargarPaginaNuevaContrasena() {
        const urlParams = new URLSearchParams(window.location.search);
        const guid = urlParams.get('guid');

        if (guid) {
            let inputGuid = document.getElementById('guid_cambio') as HTMLInputElement;
            if (inputGuid) inputGuid.value = guid;

            const motivo = urlParams.get('motivo');
            const msg = (Numero(motivo) === 2)
                ? "Indique una contraseña segura: 8 caracteres con al menos un número y una mayúscula"
                : "Cree su nueva contraseña: 8 caracteres con al menos un número y una mayúscula";

            MostrarMensaje(msg, "alert-info"); // Usamos azul para info inicial

        } else {
            MostrarMensaje("El enlace no contiene un código de validación válido.", "alert-danger");
        }
    }

    export function AlPulsarNuevaContrasena() {
        OcultarMensaje();

        let p1: HTMLInputElement = document.getElementById('password_nueva') as HTMLInputElement;
        let p2: HTMLInputElement = document.getElementById('password_confirmar') as HTMLInputElement;
        let guidInput: HTMLInputElement = document.getElementById('guid_cambio') as HTMLInputElement;

        if (IsNullOrEmpty(p1.value) || IsNullOrEmpty(p2.value)) {
            MostrarMensaje("Debe completar ambas contraseñas");
            return;
        }

        if (p1.value !== p2.value) {
            MostrarMensaje("Las contraseñas no coinciden");
            return;
        }

        let url: string = `/${ltrControladores.Entorno.Acceso}/${Ajax.EpDeAcceso.ActualizarContrasena}`;

        let a = new ApiDeAjax.DescriptorAjax(this
            , Ajax.EpDeAcceso.ActualizarContrasena
            , null
            , url
            , ApiDeAjax.TipoPeticion.Asincrona
            , ApiDeAjax.ModoPeticion.Post
            , (pet) => {
                alert(pet.resultado.mensaje);
                window.location.href = `/${ltrControladores.Entorno.Acceso}/${ltrVistas.Entorno.Conectar}.html`;
            }
            , (pet) => { MostrarMensaje(pet.resultado.mensaje); }
        );

        let datosPost = new FormData();
        datosPost.append("guid", guidInput.value);
        let passwordEncriptada: string = Encriptar(guidInput.value, p1.value);
        datosPost.append("password", passwordEncriptada);

        a.DatosPost = datosPost;
        a.Ejecutar();
    }

    /**
     * Muestra el mensaje usando un div Alert de Bootstrap que crece con el texto.
     * @param claseBootstrap Permite cambiar el color (alert-danger, alert-info, alert-warning)
     */
    function MostrarMensaje(mensaje: string, claseBootstrap: string = "alert-danger") {
        let divInfoConexion = document.getElementById('div-info-conexion') as HTMLDivElement;
        let textContainer = document.getElementById('info-conexion');

        // Limpiar clases de color previas y añadir la nueva
        divInfoConexion.classList.remove("alert-danger", "alert-info", "alert-warning");
        divInfoConexion.classList.add(claseBootstrap);

        textContainer.innerText = mensaje;
        divInfoConexion.style.setProperty("display", "flex", "important");
    }

    function OcultarMensaje() {
        let divInfoConexion = document.getElementById('div-info-conexion') as HTMLDivElement;
        let textContainer = document.getElementById('info-conexion') as HTMLDivElement;

        if (textContainer) textContainer.innerText = "";

        if (divInfoConexion) {
            // Esta es la forma correcta de pasar el !important desde JS
            divInfoConexion.style.setProperty("display", "none", "important");
        }
    }

    export class GestorDeAcceso {
        private _login: string;

        constructor(login: string) {
            this._login = login;
            this.LeerUsuario();
        }

        private LeerUsuario() {
            let restrictor: string = DefinirRestrictorCadena(Ajax.Param.login, this._login);
            let url: string = `/Acceso/${Ajax.EpDeAcceso.ReferenciarFoto}?restrictor=${restrictor}`;

            let a = new ApiDeAjax.DescriptorAjax(this
                , Ajax.EpDeAcceso.ReferenciarFoto
                , null
                , url
                , ApiDeAjax.TipoPeticion.Asincrona
                , ApiDeAjax.ModoPeticion.Get
                , this.MapearFoto
                , this.SiHayErrorTrasPeticionAjax
            );
            a.Ejecutar();
        }

        private MapearFoto(peticion: ApiDeAjax.DescriptorAjax) {
            let visor: HTMLImageElement = document.getElementById('img-usuario') as HTMLImageElement;
            (peticion.llamador as GestorDeAcceso).MostrarImagenUrl(visor, peticion.resultado.datos);
            OcultarMensaje();
        }

        private MostrarImagenUrl(visor: HTMLImageElement, url: any) {
            visor.setAttribute('src', url);
            let idCanva: string = visor.getAttribute(atControl.id).replace('img', 'canvas');
            let htmlCanvas: HTMLCanvasElement = document.getElementById(idCanva) as HTMLCanvasElement;
            htmlCanvas.width = 90;
            htmlCanvas.height = 90;
            var canvas = htmlCanvas.getContext('2d');
            var img = new Image();
            img.src = url;
            img.onload = function () {
                canvas.drawImage(img, 0, 0, 90, 90);
            };

            let divCambas: HTMLDivElement = document.getElementById(`div-${idCanva}`) as HTMLDivElement;
            divCambas.style.display = ltrStyle.display.block;

            let idIcono = idCanva.replace('canvas', 'icono');
            let divIcono: HTMLDivElement = document.getElementById(`div-${idIcono}`) as HTMLDivElement;
            divIcono.style.display = ltrStyle.display.none;
        }

        public ValidarAcceso(password: string) {
            let url: string = `/Acceso/${Ajax.EpDeAcceso.ValidarAcceso}`;
            let a = new ApiDeAjax.DescriptorAjax(this
                , Ajax.EpDeAcceso.ValidarAcceso
                , null
                , url
                , ApiDeAjax.TipoPeticion.Asincrona
                , ApiDeAjax.ModoPeticion.Post
                , this.TrasValidarAcceso
                , this.SiHayErrorTrasPeticionAjax
            );

            let datosPost = new FormData();
            datosPost.append(Ajax.Param.login, this._login);
            let passwordEncriptada: string = Encriptar(this._login, password);
            datosPost.append(Ajax.Param.password, passwordEncriptada);

            a.DatosPost = datosPost;
            a.Ejecutar();
        }

        private TrasValidarAcceso(peticion: ApiDeAjax.DescriptorAjax) {
            let gestor = peticion.llamador as GestorDeAcceso;
            if (peticion.resultado.datos === true) {
                gestor.MostrarPaso2F(peticion);
            } else {
                gestor.Conectar(peticion);
            }
        }

        public Validar2F(codigo2F: string, esDispositivo2FA: boolean) {
            let url: string = `/Acceso/${Ajax.EpDeAcceso.Validar2F}`;
            let a = new ApiDeAjax.DescriptorAjax(this
                , Ajax.EpDeAcceso.Validar2F
                , null
                , url
                , ApiDeAjax.TipoPeticion.Asincrona
                , ApiDeAjax.ModoPeticion.Post
                , this.Conectar
                , this.SiHayErrorTrasPeticionAjax
            );

            let datosPost = new FormData();
            datosPost.append(Ajax.Param.login, this._login);
            datosPost.append(Ajax.Param.codigo2F, codigo2F);
            datosPost.append(Ajax.Param.esDispositivo2FA, esDispositivo2FA.toString());

            a.DatosPost = datosPost;
            a.Ejecutar();
        }

        public Reenviar2F() {
            let url: string = `/Acceso/${Ajax.EpDeAcceso.Reenviar2F}`;
            let a = new ApiDeAjax.DescriptorAjax(this
                , Ajax.EpDeAcceso.Reenviar2F
                , null
                , url
                , ApiDeAjax.TipoPeticion.Asincrona
                , ApiDeAjax.ModoPeticion.Post
                , (pet) => { MostrarMensaje(pet.resultado.mensaje, "alert-info"); }
                , this.SiHayErrorTrasPeticionAjax
            );

            let datosPost = new FormData();
            datosPost.append(Ajax.Param.login, this._login);
            a.DatosPost = datosPost;
            a.Ejecutar();
        }

        private MostrarPaso2F(peticion: ApiDeAjax.DescriptorAjax) {
            MostrarMensaje(peticion.resultado.mensaje, "alert-info");

            let botonConectar: HTMLButtonElement = document.getElementById('IngresoLog') as HTMLButtonElement;
            botonConectar.style.display = 'none';

            let divOlvideContrasena: HTMLDivElement = document.getElementById('div-olvide-contrasena') as HTMLDivElement;
            divOlvideContrasena.style.display = 'none';

            let login: HTMLInputElement = document.getElementById('login') as HTMLInputElement;
            login.disabled = true;

            let password: HTMLInputElement = document.getElementById('password') as HTMLInputElement;
            password.disabled = true;

            let divCodigo2F: HTMLDivElement = document.getElementById('div-codigo2F') as HTMLDivElement;
            divCodigo2F.style.display = 'block';
        }

        public SolicitarNuevaContrasena() {
            let url: string = `/Acceso/${Ajax.EpDeAcceso.SolicitarNuevaContrasena}`;
            let a = new ApiDeAjax.DescriptorAjax(this
                , Ajax.EpDeAcceso.SolicitarNuevaContrasena
                , null
                , url
                , ApiDeAjax.TipoPeticion.Asincrona
                , ApiDeAjax.ModoPeticion.Post
                , (pet) => { MostrarMensaje(pet.resultado.mensaje, "alert-info"); }
                , this.SiHayErrorTrasPeticionAjax
            );

            let datosPost = new FormData();
            datosPost.append(Ajax.Param.login, this._login);
            a.DatosPost = datosPost;
            a.Ejecutar();
        }

        private Conectar(peticion: ApiDeAjax.DescriptorAjax) {
            let l: HTMLInputElement = document.getElementById('l') as HTMLInputElement;
            l.value = (peticion.llamador as GestorDeAcceso)._login;
            let password: HTMLInputElement = document.getElementById('password') as HTMLInputElement;
            let p: HTMLInputElement = document.getElementById('p') as HTMLInputElement;
            p.value = Encriptar(l.value, password.value);
            Registro.EliminarUsuarioDeConexion();
            let f: HTMLFormElement = document.getElementById('FormDeConexion') as HTMLFormElement;

            let urlParams = new URLSearchParams(window.location.search);
            let queryParam = urlParams.get('ReturnUrl');

            let formAction = f.getAttribute('action');
            if (queryParam) {
                formAction += "?ReturnUrl=" + encodeURIComponent(queryParam);
            }

            f.setAttribute('action', formAction);
            f.submit();
        }

        private SiHayErrorTrasPeticionAjax(peticion: ApiDeAjax.DescriptorAjax) {
            MostrarMensaje(peticion.resultado.mensaje, "alert-danger");
            console.error(peticion.resultado.consola);
        }
    }
}