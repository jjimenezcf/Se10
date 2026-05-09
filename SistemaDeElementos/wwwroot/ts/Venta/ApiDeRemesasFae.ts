namespace Venta {

    export enum enumEtapasDeRemesaFae {
        REM_Etapa_De_Cumplimentacion,
        REM_Etapa_Generada,
        REM_Etapa_De_Presentacion,
        REM_Etapa_De_Cierre,
        REM_Etapa_Cancelada
    }

    export function CargarRemesa(llamador: any, controlador: string, parametros: Array<Parametro>, datosDeEntrada: Array<Parametro>): Promise<ApiDeAjax.DescriptorAjax> {
        return ApiDePeticiones.EjecutarPeticion(llamador, controlador, Ajax.EndPoint.Venta.RemesasFae.CargarRemesa, parametros, datosDeEntrada);
    }

    export function AnularCargoRemesa(llamador: any, controlador: string, parametros: Array<Parametro>, datosDeEntrada: Array<Parametro>): Promise<ApiDeAjax.DescriptorAjax> {
        return ApiDePeticiones.EjecutarPeticion(llamador, controlador, Ajax.EndPoint.Venta.RemesasFae.AnularCargoRemesa, parametros, datosDeEntrada);
    }

}

