namespace Logistica {

    export function Ral_Tras_Seleccionar_Almacen(): void {
        if (Crud.ModoTrabajo() !== enumModoTrabajo.creando) return;

        const panel = Crud.crudMnt.crudDeCreacion.PanelDeCrear;
        const almacenLista = ApiControl.BuscarListaDinamicaPorPropiedad(panel, ltrPropiedades.Logistica.Regularizacion.Almacen) as HTMLInputElement;
        const almacen = OpcionesDeLasListas.ObtenerObjeto(almacenLista);
        if (Definido(almacen)) {
            ApiDelCrud.MapearDatosSocietariosYDepartamentales(panel, almacen);
        }
    }

    export function Ral_Tras_Blanquear_Almacen(): void {
        if (Crud.ModoTrabajo() !== enumModoTrabajo.creando) return;

        const panel = Crud.crudMnt.crudDeCreacion.PanelDeCrear;
        const cgLista = ApiControl.BuscarListaDinamicaPorPropiedad(panel, literal.Cg) as HTMLInputElement;
        ApiListaDinamica.AsignarValor(cgLista, 0, "");

        const sociedad = ApiControl.BuscarEditor(panel, ltrPropiedades.Elemento.ConCg.IdSociedadDelCg) as HTMLInputElement;
        if (Definido(sociedad))
            sociedad.value = "";
    }
}
