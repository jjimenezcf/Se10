namespace ApiDeCertificados
{
    export function BlanquearPasswordDelCertificado(idModal: string) {
        let modal = document.getElementById(idModal) as HTMLDivElement;
        ApiDelCrud.BlanquearPasswordDeCertificado(modal);
    }

    export function InicializarModalMiCertificado(idModal: string): void {
        let modal = document.getElementById(idModal) as HTMLDivElement;
        ApiDeInicializacion.Archivos(modal);
        let password: HTMLInputElement = ApiControl.BuscarControl(modal, ltrPropiedades.Entorno.Usuario.PassworDelCertificado, true) as HTMLInputElement;
        password.value = '';
    }

    export function SubirMiCertificado(idModal: string): void {
        let modal = document.getElementById(idModal) as HTMLDivElement;
        let archivo = modal.querySelector(`input[${atControl.tipo}="${ltrTipoControl.SelectorDeUnArchivo}"]`) as HTMLInputElement;
        let idArchivo: number = Numero(archivo.getAttribute(atArchivo.idArchivo));
        if (idArchivo === 0) MensajesSe.EmitirExcepcion("SubirMiCertificado", "Ha de subir el certificado");

        let password: HTMLInputElement = ApiControl.BuscarControl(modal, ltrPropiedades.Entorno.Usuario.PassworDelCertificado, true) as HTMLInputElement;
        if (IsNullOrEmpty(password.value)) MensajesSe.EmitirExcepcion("SubirMiCertificado", "Ha de indicar la password");              

        ApiDePeticiones.SubirMiCertificado(modal, idArchivo, password.value)
            .then((peticion) => {
                ApiPanel.CerrarModal(modal);
                //if (Definido(Crud.crudMnt) && Crud.crudMnt instanceof Entorno.CrudDeUsuarios) Crud.crudMnt.crudDeEdicion.Recargar();
            })
            .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
    }
}
