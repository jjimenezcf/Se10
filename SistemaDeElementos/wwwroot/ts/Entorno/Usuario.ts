namespace Entorno {

    export function CrearCrudDeUsuarios(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
        Crud.crudMnt = new Entorno.CrudDeUsuarios(idPanelMnt, idPanelCreacion, idPanelEdicion, idModalBorrar);

        window.addEventListener("load", function () {
            Crud.crudMnt.Inicializar(idPanelMnt);
        }, false);

        window.onbeforeunload = function () {
            Crud.crudMnt.AntesDeSalir();
        };
    }
    export class CrudDeUsuarios extends Crud.CrudMnt {

        constructor(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
            super(idPanelMnt, idModalBorrar);
            this.crudDeCreacion = new CrudCreacionUsuario(this, idPanelCreacion);
            this.crudDeEdicion = new CrudEdicionUsuario(this, idPanelEdicion);
        }
    }

    export class CrudCreacionUsuario extends Crud.CrudCreacion {
        constructor(crud: Crud.CrudMnt, idPanelCreacion: string) {
            super(crud, idPanelCreacion);
        }
    }

    export class CrudEdicionUsuario extends Crud.CrudEdicion {

        constructor(crud: Crud.CrudMnt, idPanelEdicion: string) {
            super(crud, idPanelEdicion);
        }

        public ParametrosParaLeerElementoPorId(): Array<Parametro> {
            var p = super.ParametrosParaLeerElementoPorId();
            p.push(new Parametro(ltrParametrosNeg.ObtenerCertificado, true));
            return p;
        }

        protected AntesDeMapearElementoDevuelto(peticion: ApiDeAjax.DescriptorAjax): void {
            super.AntesDeMapearElementoDevuelto(peticion);

            let datosDelCertificado: HTMLTextAreaElement = ApiControl.BuscarControl(Crud.crudMnt.crudDeEdicion.PanelDeEditar, ltrPropiedades.Entorno.Usuario.DatosCertificado, true) as HTMLTextAreaElement;

            if (Registro.UsuarioConectado().id !== peticion.resultado.datos.id) {
                datosDelCertificado.parentElement.parentElement.classList.remove(ltrCss.divVisible);
                datosDelCertificado.parentElement.parentElement.classList.add(ltrCss.divNoVisible);
            } else {
                datosDelCertificado.parentElement.parentElement.classList.remove(ltrCss.divNoVisible);
                datosDelCertificado.parentElement.parentElement.classList.add(ltrCss.divVisible);
            }

            //let password: HTMLInputElement = ApiControl.BuscarControl(Crud.crudMnt.crudDeEdicion.PanelDeEditar, ltrPropiedades.Entorno.Usuario.PassworDelCertificado, true) as HTMLInputElement;
            //password.parentElement.parentElement.classList.remove(ltrCss.divVisible);
            //password.parentElement.parentElement.classList.add(ltrCss.divNoVisible);
        }

        protected DespuesDeMapearElementoDevuelto(panel: HTMLDivElement, peticion: ApiDeAjax.DescriptorAjax): void {
            super.DespuesDeMapearElementoDevuelto(panel, peticion);
            if (!Registro.EsAdministrador()) {
                ApiControl.BloquearCheckPorPropiedad(this.PanelDeEditar, ltrPropiedades.Entorno.Usuario.Activo, true);
                ApiControl.BloquearCheckPorPropiedad(this.PanelDeEditar, ltrPropiedades.Entorno.Usuario.Administrador, true);
            }

            //    if (Registro.UsuarioConectado().id !== peticion.resultado.datos.id)
            //        return;

            //    let menu: HTMLUListElement = document.getElementById(ltrMenus.menu.edicion) as HTMLUListElement;
            //    let li: HTMLLIElement = ApiDeMenuFlotante.BuscarOpcionMf(menu, ltrMenus.opcionMf.usuario.MiCertificado);
            //    ApiDeMenuFlotante.HabilitarOpcionMf(li, true, ltrMenus.enumOrigen.edicion);
        }

        //public ProcesarOpcionMf(idNegocio: number, opcion: string, esContextual: boolean): void {
        //    if (opcion === ltrMenus.eventosDeMf.Entorno.Usuario.MiCertificado) {
        //        let modal = this.CrudDeMnt.Cuerpo.querySelector(`div[${atControl.tipoModal}="${enumTipoDeModal.ModalMiCertificado}"]`) as HTMLDivElement;
        //        ApiPanel.AbriModal(modal);
        //    }
        //    else
        //        super.ProcesarOpcionMf(idNegocio, opcion, esContextual);
        //}

    }

}

