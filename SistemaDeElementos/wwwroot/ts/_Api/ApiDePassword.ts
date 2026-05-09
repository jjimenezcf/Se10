namespace ApiDePassword
{
    const ltrCambioDePassword = {
        propiedades: {
            actual: 'Actual',
            nueva: 'Nueva',
            repetida: 'Repetida'
        }
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
}
