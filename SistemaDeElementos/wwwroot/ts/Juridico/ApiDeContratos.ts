namespace Juridico {

    const enumClaseDeContrato = {
        Venta: 'venta',
        Compra: 'compra'
    };

    export enum enumEtapasDeContratos {
        CTR_Etapa_En_Elaboracion,
        CTR_Etapa_Vigente,
        CTR_Etapa_Pdt_Prorroga,
        CTR_Etapa_Derogado,
        CTR_Etapa_Finalizacion,
        CTR_Etapa_Cancelado
    }

    export function ParsearEtapa(etapa: string): enumEtapasDeContratos {
        if (EsNumeroNoNulo(etapa))
            return Numero(etapa);

        if (etapa === ObtenerEnumerado(enumEtapasDeContratos, enumEtapasDeContratos.CTR_Etapa_En_Elaboracion)) return enumEtapasDeContratos.CTR_Etapa_En_Elaboracion;
        if (etapa === ObtenerEnumerado(enumEtapasDeContratos, enumEtapasDeContratos.CTR_Etapa_Vigente)) return enumEtapasDeContratos.CTR_Etapa_Vigente;
        if (etapa === ObtenerEnumerado(enumEtapasDeContratos, enumEtapasDeContratos.CTR_Etapa_Pdt_Prorroga)) return enumEtapasDeContratos.CTR_Etapa_Pdt_Prorroga;
        if (etapa === ObtenerEnumerado(enumEtapasDeContratos, enumEtapasDeContratos.CTR_Etapa_Derogado)) return enumEtapasDeContratos.CTR_Etapa_Derogado;
        if (etapa === ObtenerEnumerado(enumEtapasDeContratos, enumEtapasDeContratos.CTR_Etapa_Finalizacion)) return enumEtapasDeContratos.CTR_Etapa_Finalizacion;
        if (etapa === ObtenerEnumerado(enumEtapasDeContratos, enumEtapasDeContratos.CTR_Etapa_Cancelado)) return enumEtapasDeContratos.CTR_Etapa_Cancelado;

        MensajesSe.EmitirExcepcion("Parsear etapa de circuito de contratos", `la etapa ${etapa} no está definida`);
        return null;
    }

    export function Ctr_AjustarControlesDeEdicionDelPlanificador(idModal: string) {
        let modal = document.getElementById(idModal) as HTMLDivElement;
        let objeto = Crud.crudMnt.crudDeEdicion.ObjetoDeExpansor(idModal);  
        Plv_AjustarControlesDeEdicion(modal, objeto);
    }

    export function Plv_AjustarControlesDeEdicion(panelContenedor: HTMLDivElement, registro: any) {
        if (Numero(ObtenerPropiedad(registro, ltrPropiedades.Juridico.PlanificadorDeVenta.IdTipoDeFactura, 0)) > 0) {
            let lista = ApiControl.BuscarListaDinamicaPorPropiedad(panelContenedor, ltrPropiedades.Juridico.PlanificadorDeVenta.TipoDeParte);
            let panel = ApiPanel.ContenedorDe(lista.id);
            ApiListaDinamica.Bloquear_Blanquear(panel, ltrPropiedades.Juridico.PlanificadorDeVenta.TipoDeParte);
        }
        else {
            let lista = ApiControl.BuscarListaDinamicaPorPropiedad(panelContenedor, ltrPropiedades.Juridico.PlanificadorDeVenta.TipoDeFactura);
            let panel = ApiPanel.ContenedorDe(lista.id);
            ApiListaDinamica.Bloquear_Blanquear(panel, ltrPropiedades.Juridico.PlanificadorDeVenta.TipoDeFactura);
        }
    }

    export function Plv_Tras_Seleccionar_TipoDeFactura(idLista: string) {
        let lista: HTMLInputElement = document.getElementById(idLista) as HTMLInputElement;

        if (Tipos.ListaDinamica.Obtener(lista).IdSeleccionado > 0) {
            let panel = ApiPanel.ContenedorDe(idLista);
            ApiListaDinamica.Bloquear_Blanquear(panel, ltrPropiedades.Juridico.PlanificadorDeVenta.TipoDeParte);
        }
        else
            Plv_Tras_Blanquear_TipoDeFactura(idLista);

    }
    export function Plv_Tras_Blanquear_TipoDeFactura(idLista: string) {
        let panel = ApiPanel.ContenedorDe(idLista);
        ApiControl.DesbloquearListaDinamicaPorPropiedad(panel, ltrPropiedades.Juridico.PlanificadorDeVenta.TipoDeParte);
    }

    export function Plv_Tras_Seleccionar_TipoDeParte(idLista: string) {
        let lista: HTMLInputElement = document.getElementById(idLista) as HTMLInputElement;
        if (Tipos.ListaDinamica.Obtener(lista).IdSeleccionado > 0) {
            let panel = ApiPanel.ContenedorDe(idLista);
            ApiListaDinamica.Bloquear_Blanquear(panel, ltrPropiedades.Juridico.PlanificadorDeVenta.TipoDeFactura);
        }
        else
            Plv_Tras_Blanquear_TipoDeParte(idLista);

    }
    export function Plv_Tras_Blanquear_TipoDeParte(idLista: string) {
        let panel = ApiPanel.ContenedorDe(idLista);
        ApiControl.DesbloquearListaDinamicaPorPropiedad(panel, ltrPropiedades.Juridico.PlanificadorDeVenta.TipoDeFactura);
    }

    export function epDeContratos(llamador: any, controlador: string, accion:string, parametros: Array<Parametro>, datosDeEntrada: Array<Parametro>): Promise<ApiDeAjax.DescriptorAjax> {

        return new Promise((resolve, reject) => {

            let url: string = `/${controlador}/${accion}?${Ajax.Param.parametros}=${Encriptar(literal.ClaveDeEncriptacion, JSON.stringify(parametros))}`;

            let a = new ApiDeAjax.DescriptorAjax(llamador
                , accion
                , datosDeEntrada
                , url
                , ApiDeAjax.TipoPeticion.Asincrona
                , ApiDeAjax.ModoPeticion.Get
                , (peticion) => {
                    resolve(peticion);
                }
                , (peticion) => {
                    reject(peticion);
                }
            );

            a.Ejecutar();
        });
    }

}