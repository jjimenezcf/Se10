namespace ApiDePassword
{
    const ltrCambioDePassword = {
        propiedades: {
            actual: 'Actual',
            nueva: 'Nueva',
            repetida: 'Repetida'
        }
    }

    const ltrMiUsuario = {
        propiedades: {
            eMail: 'email',
            idArchivo: 'idarchivo'
        }
    }
    export function InicializarModalMiUsuario(idModal: string): void {
        let modal = document.getElementById(idModal) as HTMLDivElement;
        let parametros: Array<Parametro> = new Array<Parametro>();
        parametros.push(new Parametro(Ajax.Param.leerPorIdParaEditar, true));
        ApiDePeticiones.LeerElementoPorId(modal, ltrControladores.Entorno.Usuarios, Registro.UsuarioConectado().id, parametros, Registro.UsuarioConectado())
            .then((peticion) => 
                MapearAlPanel.ElObjeto(modal, peticion.resultado.datos, ModoAcceso.enumModoDeAccesoDeDatos.Gestor)
            )
            .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
    }

    export function InicializarModalCambiarPassword(idModal: string): void {
        let modal = document.getElementById(idModal) as HTMLDivElement;
        ApiDeInicializacion.Archivos(modal);
        let password: HTMLInputElement = ApiControl.BuscarControl(modal, ltrCambioDePassword.propiedades.actual, true) as HTMLInputElement;
        password.value = '';
    }

    export function CambiarPassword(idModal: string): void {
        let modal = document.getElementById(idModal) as HTMLDivElement;

        let actual: HTMLInputElement = ApiControl.BuscarControl(modal, ltrCambioDePassword.propiedades.actual, true) as HTMLInputElement;
        let nueva: HTMLInputElement = ApiControl.BuscarControl(modal, ltrCambioDePassword.propiedades.nueva, true) as HTMLInputElement;
        let repetida: HTMLInputElement = ApiControl.BuscarControl(modal, ltrCambioDePassword.propiedades.repetida, true) as HTMLInputElement;

        if (IsNullOrEmpty(actual.value)) MensajesSe.EmitirExcepcion("CambiarPassword", "Ha de indicar la password actual");
        if (IsNullOrEmpty(nueva.value)) MensajesSe.EmitirExcepcion("CambiarPassword", "Ha de indicar la nueva password");
        if (IsNullOrEmpty(repetida.value)) MensajesSe.EmitirExcepcion("CambiarPassword", "Ha de indicar reintroducir la nueva password"); 
        if (nueva.value !== repetida.value) MensajesSe.EmitirExcepcion("CambiarPassword", "No ha indicado la misma password");

        ApiDePeticiones.CambiarPassword(modal, actual.value, nueva.value, repetida.value)
            .then((peticion) => {
                ApiPanel.CerrarModal(modal);
            })
            .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
    }


    export function CambiarDatosUsuario(idModal: string): void {
        let modal = document.getElementById(idModal) as HTMLDivElement;
        
        const archivo = ApiControl.BuscarSelectorDeArchivos(modal, ltrMiUsuario.propiedades.idArchivo, atControl.propiedad);
        const idArchivo = Numero(archivo.getAttribute(atArchivo.idArchivo));
        const eMail = ApiControl.BuscarEditor(modal, ltrMiUsuario.propiedades.eMail).value;

        if (IsNullOrEmpty(eMail))
            MensajesSe.EmitirExcepcion("CambiarDatosUsuario", "El campo eMail no puede ser nulo");

        ApiDePeticiones.CambiarMisDatos(modal,idArchivo, eMail)
            .then((peticion) => {
                ApiPanel.CerrarModal(modal);
            })
            .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
    }
}

