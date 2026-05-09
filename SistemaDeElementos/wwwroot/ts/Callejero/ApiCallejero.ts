namespace ApiCallejero {

    export function MapearPaisProvincia(crud: Crud.CrudMnt, peticion: ApiDeAjax.DescriptorAjax): void {
        MapearProvincia(crud, peticion);
        MapearPais(crud, peticion);
    }

    function MapearProvincia(crud: Crud.CrudMnt, peticion: ApiDeAjax.DescriptorAjax): void {
        let idProvincia: number = Json_BuscarValorEn(Callejero.objeto.municipioDto.idprovincia, peticion.resultado.datos) as number;
        let provincia: string = Json_BuscarValorEn(Callejero.objeto.municipioDto.provincia, peticion.resultado.datos) as string;
        let listaDeFiltro: HTMLInputElement = ApiControl.BuscarListaDinamicaPorPropiedad(crud.PanelFiltro, Callejero.atributo.propiedad.idprovincia);
        MapearAlControl.FijarValorEnListaDinamica(listaDeFiltro, idProvincia, provincia);

        let listaDeCreacion: HTMLInputElement = ApiControl.BuscarListaDinamicaPorGuardarEn(crud.crudDeCreacion.PanelDeCrear, Callejero.atributo.guardarEn.idprovincia);
        MapearAlControl.FijarValorEnListaDinamica(listaDeCreacion, idProvincia, provincia);

        let listaDeEdicion: HTMLInputElement = ApiControl.BuscarListaDinamicaPorGuardarEn(crud.crudDeEdicion.PanelDeEditar, Callejero.atributo.guardarEn.idprovincia);
        MapearAlControl.FijarValorEnListaDinamica(listaDeEdicion, idProvincia, provincia);
    }
    
    export function MapearPais(crud: Crud.CrudMnt, peticion: ApiDeAjax.DescriptorAjax): void {
        let idPais: number = Json_BuscarValorEn(Callejero.objeto.provinciaDto.idpais, peticion.resultado.datos) as number;
        let pais: string = Json_BuscarValorEn(Callejero.objeto.provinciaDto.pais, peticion.resultado.datos) as string;
        let listaDeFiltro: HTMLInputElement = ApiControl.BuscarListaDinamicaPorPropiedad(crud.PanelFiltro, Callejero.atributo.propiedad.idpais);
        MapearAlControl.FijarValorEnListaDinamica(listaDeFiltro, idPais, pais);

        let listaDeCreacion: HTMLInputElement = ApiControl.BuscarListaDinamicaPorGuardarEn(crud.crudDeCreacion.PanelDeCrear, Callejero.atributo.guardarEn.idpais);
        MapearAlControl.FijarValorEnListaDinamica(listaDeCreacion, idPais, pais);

        let listaDeEdicion: HTMLInputElement = ApiControl.BuscarListaDinamicaPorGuardarEn(crud.crudDeEdicion.PanelDeEditar, Callejero.atributo.guardarEn.idpais);
        MapearAlControl.FijarValorEnListaDinamica(listaDeEdicion, idPais, pais);
    }

}