namespace Administracion {

    export function NavegarARelaciones(peticion: ApiDeAjax.DescriptorAjax, idRestrictor: string) {
        let datosDeEntrada: Parametros = new Parametros(peticion.DatosDeEntrada as Parametro[]);
        let opcion = datosDeEntrada.ObtenerValorDeParametro(ltrMenus.opcion);
        let ids = datosDeEntrada.ObtenerValorDeParametro(Ajax.Param.ids);
        let textos = datosDeEntrada.ObtenerValorDeParametro(ltrPropiedades.Elemento.Textos);
        let urlDestino: string = undefined;
        switch (opcion) {
            case ltrMenus.eventosDeMf.Administracion.Tareas.IrAPartesTr:
                urlDestino = `${window.location.origin}/${ltrUrls.Ventas.PartesTr}?${ltrParametrosUrl.filtros}=[${idRestrictor}=${ids[0]}=${textos[0]}]`;
                break;
            case ltrMenus.eventosDeMf.Administracion.Expedientes.IrATareas:
                urlDestino = `${window.location.origin}/${ltrUrls.Administracion.Tareas}?${ltrParametrosUrl.filtros}=[${idRestrictor}=${ids[0]}=${textos[0]}]`;
                break;
            case ltrMenus.eventosDeMf.Administracion.Expedientes.IrAFacturasRec:
                urlDestino = `${window.location.origin}/${ltrUrls.Gastos.FacturasRec}?${ltrParametrosUrl.filtros}=[${idRestrictor}=${ids[0]}=${textos[0]}]`;
                break;
            default: return false;
        }
        EntornoSe.AbrirPestana(urlDestino);
        return true;
    }
}