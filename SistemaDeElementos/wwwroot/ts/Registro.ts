namespace Registro {

    export const misRegistros = {
        UsuarioConectado: 'usuario-conectado',
        EsAdministrador: 'administrador',
        EsParametrizador: 'parametrizador',
        EsClienteWeb: 'cliente-web',
        ArbolDeMenu: 'arbol-de-menu',
        UrlsValidas: 'url-validas'
    };

    export class UsuarioDeConexion {
        public login: string;
        public id: number;
        public administrador: boolean;
        public parametrizador: boolean;
        public clienteWeb: boolean;
        public mail: string;
        public idAgenda: number;
        public NombreAgenda: string;
    }

    export function UrlValidada(url: string): boolean {
        let urls = sessionStorage.getItem(misRegistros.UrlsValidas);
        if (urls === null || urls === undefined)
            return false;

        let lista = JSON.parse(urls)
        return lista.findIndex(x => x.toLocaleLowerCase() === url.toLocaleLowerCase()) >= 0;
    }

    export function UrlValida(url: string) {
        var lista = new Array<string>();
        let urls = sessionStorage.getItem(misRegistros.UrlsValidas);
        if (Definido(urls))
            lista = JSON.parse(urls)

        if (lista.findIndex(x => x.toLocaleLowerCase() === url.toLocaleLowerCase()) < 0) {
            lista.push(url);
            sessionStorage.setItem(misRegistros.UrlsValidas, JSON.stringify(lista));
        }
    }

    export function HayUsuarioDeConexion(): boolean {
        let u = sessionStorage.getItem(misRegistros.UsuarioConectado);
        if (u === null || u === undefined)
            return false;

        let uc: UsuarioDeConexion = UsuarioConectado();
        return uc.id > 0;
    }

    export function HayArbolDeMenu(): boolean {
        let u = sessionStorage.getItem(misRegistros.ArbolDeMenu);
        return !IsNullOrEmpty(u);
    }

    function crearUsuarioDeConexion(usuario: any): UsuarioDeConexion {
        let u: UsuarioDeConexion = new UsuarioDeConexion();
        asignarUsuarioDeConexion(u, usuario);
        return u;
    }

    function asignarUsuarioDeConexion(u: UsuarioDeConexion, usuario: any): void {
        u.id = Numero(usuario['id']);
        u.login = usuario['login'];
        u.mail = usuario['mail'];
        u.administrador = EsTrue(usuario['administrador']);
        u.parametrizador = EsTrue(usuario['parametrizador']);
        u.clienteWeb = EsTrue(ObtenerPropiedad(usuario, 'clienteWeb'));
        u.NombreAgenda = ObtenerPropiedad(usuario, 'nombreagenda');
        u.idAgenda = Numero(ObtenerPropiedad(usuario, 'idagenda'));
    }

    function asignarUsuarioNulo(u: UsuarioDeConexion): void {
        u.id = 0;
        u.login = '';
        u.mail = '';
        u.administrador = false;
        u.parametrizador = false;
        u.clienteWeb = false;
        u.NombreAgenda = '';
        u.idAgenda = 0;
    }

    export function UsuarioConectado(): UsuarioDeConexion {
        let u: UsuarioDeConexion = new UsuarioDeConexion();
        try {
            asignarUsuarioDeConexion(u, JSON.parse(sessionStorage.getItem(misRegistros.UsuarioConectado)));
        }
        catch {
            asignarUsuarioNulo(u);
        }
        return u;
    };


    export function ObtenerArbolDeMenu(): string {
        return JSON.parse(sessionStorage.getItem(misRegistros.ArbolDeMenu));
    };

    export function GuardarArbolDeMenu(arbol: string): void {
        sessionStorage.setItem(misRegistros.ArbolDeMenu, JSON.stringify(arbol));
    };

    export function EsAdministrador(): boolean {
        return JSON.parse(sessionStorage.getItem(misRegistros.EsAdministrador));
    };

    export function EsClienteWeb(): boolean {
        return JSON.parse(sessionStorage.getItem(misRegistros.EsClienteWeb));
    };

    export function EsMovil(): boolean {
        const userAgent = navigator.userAgent || navigator.vendor;
        return /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(userAgent);
    }

    export function RegistrarUsuarioDeConexion(llamador: any): Promise<any> {

        function RegistrarUsuario(peticion: ApiDeAjax.DescriptorAjax): UsuarioDeConexion {
            let usuario: UsuarioDeConexion = crearUsuarioDeConexion(peticion.resultado.datos) as UsuarioDeConexion;
            sessionStorage.setItem(misRegistros.UsuarioConectado, JSON.stringify(usuario));
            sessionStorage.setItem(misRegistros.EsAdministrador, JSON.stringify(usuario.administrador));
            sessionStorage.setItem(misRegistros.EsParametrizador, JSON.stringify(usuario.parametrizador));
            sessionStorage.setItem(misRegistros.EsClienteWeb, JSON.stringify(usuario.clienteWeb));
            return usuario;
        }

        return new Promise((resolve, reject) => {

            let url: string = `/${Ajax.Usuarios.controlador}/${Ajax.Usuarios.accion.LeerUsuarioDeConexion}`;

            let a = new ApiDeAjax.DescriptorAjax(llamador
                , Ajax.Usuarios.accion.LeerUsuarioDeConexion
                , llamador
                , url
                , ApiDeAjax.TipoPeticion.Asincrona
                , ApiDeAjax.ModoPeticion.Get
                , (peticion) => {
                    resolve(RegistrarUsuario(peticion));
                }
                , () => {
                    reject();
                }
            );

            if (!HayUsuarioDeConexion())
                a.Ejecutar();
        });


    }

    export function EliminarUsuarioDeConexion() {
        sessionStorage.setItem(misRegistros.UsuarioConectado, '');
        sessionStorage.setItem(misRegistros.EsAdministrador, '');
        EliminarArbolDeMenu();
    }

    export function EliminarArbolDeMenu() {
        sessionStorage.setItem(misRegistros.ArbolDeMenu, '');
    }


}