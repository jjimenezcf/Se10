namespace Cookies {

    export const Cooky = {
        UsuarioConectado: 'usuario-conectado'
    };

    export function Guardar(nombre: string, valor: any): void {
        document.cookie = `${nombre}=${JSON.stringify(valor)}`;
    }

    export function LeerCookie(nombre): any {
        let lista: string[] = document.cookie.split(";");
        let micookie: string = "";
        let valor: string = "";
        for (let i: number = 0; i < lista.length; i++) {
            var busca = lista[i].search(nombre);
            if (busca > -1) {
                micookie = lista[i];
                break;
            }
        }
        if (!IsNullOrEmpty(micookie)) {
            var igual = micookie.indexOf("=");
            valor = micookie.substring(igual + 1);
        }
        return IsNullOrEmpty(valor) ? null : JSON.parse(valor);
    }

    export function IdUsuarioConectado(): number {
        let valor: any = JSON.parse(Cookies.LeerCookie(Cookies.Cooky.UsuarioConectado));
        return valor[1];
    }; 

}