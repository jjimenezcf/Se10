namespace Contabilidad {


    enum enumEtapasDePreasiento {
    }

    function ParsearEtapa(etapa: string): enumEtapasDePreasiento {

        MensajesSe.EmitirExcepcion("Parsear etapa de preasiento", `la etapa ${etapa} no está definida`);
        return null;
    }

    function EstaElPreasientoEtapa(etapas: string, etapa: enumEtapasDePreasiento): boolean {
        if (!Definido(etapas))
            return false;

        let lista = etapas.split("|");
        for (let i = 0; i < lista.length; i++) {
            if (ParsearEtapa(lista[i]) === etapa)
                return true;
        }
        return false;
    }

    export function CrearCrudDePreasientos(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
        Crud.crudMnt = new Contabilidad.CrudDePreasientos(idPanelMnt, idPanelCreacion, idPanelEdicion, idModalBorrar);
        window.addEventListener("load", function () { Crud.crudMnt.Inicializar(idPanelMnt); }, false);

        window.onbeforeunload = function () {
            Crud.crudMnt.AntesDeSalir();
        };
    }

    export class CrudDePreasientos extends Crud.CrudMnt {

        public get ModalContabilizar(): HTMLDivElement { return document.getElementById(this.IdCrud + '-' + ltrMenus.eventosDeMf.Contabilidad.Preasientos.CrearLoteContable) as HTMLDivElement; }

        constructor(idPanelMnt: string, idPanelCreacion: string, idPanelEdicion: string, idModalBorrar: string) {
            super(idPanelMnt, idModalBorrar);
            this.crudDeCreacion = new CrudCreacionPreasiento(this, idPanelCreacion);
            this.crudDeEdicion = new CrudEdicionPreasiento(this, idPanelEdicion);
        }

        public DespuesDeProcesarOpcionMf(peticion: ApiDeAjax.DescriptorAjax): boolean {
            if (super.DespuesDeProcesarOpcionMf(peticion))
                return true;
            let opcion: string = ObtenerPropiedad(peticion.DatosDeEntrada, ltrMenus.opcion);


            if (opcion === ltrMenus.eventosDeMf.Comun.Totalizador_Mostrar) {
                this.MostrarPanelDeTotales(ltrControladores.Contabilidad.Preasientos);
                return true;
            }

            if (opcion === ltrMenus.eventosDeMf.Contabilidad.Preasientos.CrearLoteContable) {
                this.crudDeEdicion.Expansor_AbrirModalParaPedirDatos((Crud.crudMnt as CrudDePreasientos).ModalContabilizar.id, 0);
                return true;
            }
        }

        public ModalDePedirDatos_Aceptar(modal: HTMLDivElement) {
            let datosDeEntrada: Array<Parametro> = new Array<Parametro>();
            let parametros: Array<Parametro> = new Array<Parametro>();
            ApiPanel.MapearControlesDesdeElPanelALaListaDeParametros(modal, parametros);

            if (modal.id === (Crud.crudMnt as CrudDePreasientos).ModalContabilizar.id) {
                parametros.push(new Parametro(Ajax.Param.filtro, Crud.crudMnt.ObtenerFiltros(ltrOperacion.CargarDatos)));
                ApiDePeticiones.EjecutarPeticion(this, this.Controlador, Ajax.EndPoint.Contabilidad.Preasientos.CrearLoteContable, parametros, datosDeEntrada)
                    .then((peticion) => {
                        super.ModalDePedirDatos_Cerrar(modal);
                        MensajesSe.Info(peticion.resultado.mensaje);
                    })
                    .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
            }
            else super.ModalDePedirDatos_Aceptar(modal);
        }

    }

    export class CrudCreacionPreasiento extends Crud.CrudCreacion {

        constructor(crud: Crud.CrudMnt, idPanelCreacion: string) {
            super(crud, idPanelCreacion);
        }

        //protected AlCerrarIrAEdicion(peticion: ApiDeAjax.DescriptorAjax): boolean {
        //    super.AlCerrarIrAEdicion(peticion);
        //    return true;
        //}

    }

    export class CrudEdicionPreasiento extends Crud.CrudEdicion {

        constructor(crud: Crud.CrudMnt, idPanelEdicion: string) {
            super(crud, idPanelEdicion);
        }


        protected AntesDeMapearElementoDevuelto(peticion: ApiDeAjax.DescriptorAjax) {
            super.AntesDeMapearElementoDevuelto(peticion);
        }

        protected DespuesDeMapearElementoDevuelto(panel: HTMLDivElement, peticion: ApiDeAjax.DescriptorAjax): void {
            super.DespuesDeMapearElementoDevuelto(panel, peticion);
            let textarea: HTMLTextAreaElement = document.getElementById('table-preasientodto-edicion-descripcion') as HTMLTextAreaElement;
            var content = textarea.value; // o textarea.textContent si prefieres

            // Encuentra la línea más larga
            var lines = content.split('\n');
            var maxLength = Math.max(...lines.map(line => line.length));

            // Centra cada línea
            var centeredContent = lines.map(line => line.padStart((maxLength + line.length) / 2, ' ').padEnd(maxLength, ' ')).join('\n');

            textarea.value = centeredContent; // o textarea.textContent = centeredContent;
            var esAdministrador = Registro.UsuarioConectado().administrador;

            ApiDeMenuFlotante.BloquearOpcionDeMenuSi(this.ContenedorMenu, ltrMenus.eventosDeMf.Contabilidad.Preasientos.ContabilizarPreasiento, ltrMenus.enumOrigen.edicion, !esAdministrador || this.EstaCancelada);
            ApiDeMenuFlotante.BloquearOpcionDeMenuSi(this.ContenedorMenu, ltrMenus.eventosDeMf.Contabilidad.Preasientos.RegenerarPreasiento, ltrMenus.enumOrigen.edicion, !esAdministrador || this.EstaCancelada);
        }

        public DespuesDeProcesarOpcionMf(peticion: ApiDeAjax.DescriptorAjax): boolean {
            if (super.DespuesDeProcesarOpcionMf(peticion)) return true;
            let opcion: string = ObtenerPropiedad(peticion.DatosDeEntrada, ltrMenus.opcion);
            if (opcion === ltrMenus.eventosDeMf.Contabilidad.Preasientos.ContabilizarPreasiento
                || opcion === ltrMenus.eventosDeMf.Contabilidad.Preasientos.RegenerarPreasiento
            ) {
                this.RecargarValoresDeCabecera(this.ElementoEditado.Id);
                this.RecargarGridDeHitos();
                this.RecargarGridDeTrazas();
                this.RecargarGridDeArchivadores();
                return true;
            }
        }
    }

    export function Preasiento_IrAlOrigenPorId(id: number) {
        let parametros: Array<Parametro> = new Array<Parametro>();
        parametros.push(new Parametro(literal.id, id));
        ApiDePeticiones.EjecutarPeticion(Crud.crudMnt, Crud.crudMnt.Controlador, Ajax.EndPoint.Contabilidad.Preasientos.ObtenerUrlAlOrigen, parametros, new Array<Parametro>())
            .then((peticion) => {
                var url = `${window.location.origin}/${peticion.resultado.datos}`;
                EntornoSe.AbrirPestana(url);
            })
            .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
    }

    export function Preasiento_IrAlOrigenDeLaFila(numeroDeFila: number) {
        let idPreasiento = Numero(ApiDeGrid.ObtenerValorDeLaFilaParaLaPropiedad(Crud.crudMnt.Tabla, numeroDeFila, ltrPropiedades.Elemento.Id));
        Preasiento_IrAlOrigenPorId(idPreasiento);
    }

    export function Preasiento_IrAlLoteContable(id: number) {
        let parametros: Array<Parametro> = new Array<Parametro>();
        parametros.push(new Parametro(literal.id, id));
        ApiDePeticiones.EjecutarPeticion(Crud.crudMnt, Crud.crudMnt.Controlador, Ajax.EndPoint.Contabilidad.Preasientos.ObtenerUrlAlLoteContable, parametros, new Array<Parametro>())
            .then((peticion) => {
                var url = `${window.location.origin}/${peticion.resultado.datos}`;
                EntornoSe.AbrirPestana(url);
            })
            .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
    }


}