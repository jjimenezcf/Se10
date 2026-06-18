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

    export async function BuscarDireccionEnNominatim(texto: string): Promise<any | null> {
        try {
            const params = new URLSearchParams({ q: texto, format: 'json', addressdetails: '1', limit: '1' });
            const response = await fetch(`https://nominatim.openstreetmap.org/search?${params.toString()}`, {
                headers: { 'Accept-Language': 'es', 'User-Agent': 'SistemaDeElementos/1.0' }
            });
            if (!response.ok) return null;
            const datos: Array<any> = await response.json();
            if (datos && datos.length > 0) return datos[0];
        } catch { }
        return null;
    }

    function ParametrosDeBusqueda(): Array<Parametro> {
        const p = new Array<Parametro>();
        p.push(new Parametro(Ajax.Param.cantidad, 2));
        p.push(new Parametro(Ajax.Param.obtenerSeguridad, false));
        p.push(new Parametro(Ajax.Param.peticion, ltrMenus.eventosDeMf.Maestros.Callejero.Calle.ValidarSiExiste));
        return p;
    }

    export function ValidarSiExisteCalle(llamador: HTMLDivElement, tipoVia: string, calle: string, municipio: string, cp: string): void {
        const filtros = [
            new ClausulaDeFiltrado(Callejero.objeto.calleDto.tipovia, atCriterio.igual, tipoVia),
            new ClausulaDeFiltrado(Callejero.objeto.calleDto.nombre, atCriterio.igual, calle),
            new ClausulaDeFiltrado(Callejero.objeto.calleDto.municipio, atCriterio.igual, municipio),
            new ClausulaDeFiltrado(Callejero.objeto.calleDto.cp, atCriterio.igual, cp)
        ];
        ApiDePeticiones.LeerElementos(llamador, ltrControladores.Callejero.Calles, Ajax.EndPoint.LeerElementos, filtros, ParametrosDeBusqueda(), null, false)
            .then((peticion) => {
               MapearDatosDeCalleParaCrear(llamador, peticion.resultado.datos[0]);
            })
            .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
        return null;
    }

    export async function BuscarPaisPorIso2(iso2: string, llamador: any): Promise<{ id: number, texto: string } | null> {
        try {
            const filtros = [new ClausulaDeFiltrado('iso2', atCriterio.igual, iso2)];
            const peticion = await ApiDePeticiones.LeerElementos(llamador, 'Paises', Ajax.EndPoint.LeerElementos, filtros, ParametrosDeBusqueda(), null, false);
            const datos: Array<any> = peticion.resultado.datos;
            if (datos && datos.length > 0)
                return { id: datos[0].id, texto: datos[0].expresion };
        } catch { }
        return null;
    }

    export async function BuscarProvinciaPorNombre(nombre: string, idPais: number, llamador: any): Promise<{ id: number, texto: string } | null> {
        try {
            const filtros = [
                new ClausulaDeFiltrado('nombre', atCriterio.comienza, nombre),
                new ClausulaDeFiltrado('idpais', atCriterio.igual, idPais.toString())
            ];
            const peticion = await ApiDePeticiones.LeerElementos(llamador, 'Provincias', Ajax.EndPoint.LeerElementos, filtros, ParametrosDeBusqueda(), null, false);
            const datos: Array<any> = peticion.resultado.datos;
            if (datos && datos.length > 0)
                return { id: datos[0].id, texto: datos[0].expresion };
        } catch { }
        return null;
    }

    export async function BuscarMunicipioPorNombre(nombre: string, idProvincia: number, llamador: any): Promise<{ id: number, texto: string } | null> {
        try {
            const filtros = [
                new ClausulaDeFiltrado('nombre', atCriterio.comienza, nombre),
                new ClausulaDeFiltrado('idprovincia', atCriterio.igual, idProvincia.toString())
            ];
            const peticion = await ApiDePeticiones.LeerElementos(llamador, 'Municipios', Ajax.EndPoint.LeerElementos, filtros, ParametrosDeBusqueda(), null, false);
            const datos: Array<any> = peticion.resultado.datos;
            if (datos && datos.length > 0)
                return { id: datos[0].id, texto: datos[0].expresion };
        } catch { }
        return null;
    }

    export async function BuscarTipoDeViaPorNombre(nombre: string, llamador: any): Promise<{ id: number, texto: string } | null> {
        try {
            const filtros = [new ClausulaDeFiltrado('nombre', atCriterio.igual, nombre)];
            const peticion = await ApiDePeticiones.LeerElementos(llamador, 'TiposDeVia', Ajax.EndPoint.LeerElementos, filtros, ParametrosDeBusqueda(), null, false);
            const datos: Array<any> = peticion.resultado.datos;
            if (datos && datos.length > 0)
                return { id: datos[0].id, texto: datos[0].expresion };
            const filtros2 = [new ClausulaDeFiltrado('nombre', atCriterio.comienza, nombre)];
            const peticion2 = await ApiDePeticiones.LeerElementos(llamador, 'TiposDeVia', Ajax.EndPoint.LeerElementos, filtros2, ParametrosDeBusqueda(), null, false);
            const datos2: Array<any> = peticion2.resultado.datos;
            if (datos2 && datos2.length > 0)
                return { id: datos2[0].id, texto: datos2[0].expresion };
        } catch { }
        return null;
    }

    export async function BuscarCpPorCodigo(codigo: string, idMunicipio: number, llamador: any): Promise<{ id: number, texto: string } | null> {
        try {
            const filtros = [
                new ClausulaDeFiltrado('codigo', atCriterio.igual, codigo),
                new ClausulaDeFiltrado('idmunicipio', atCriterio.igual, idMunicipio.toString())
            ];
            const peticion = await ApiDePeticiones.LeerElementos(llamador, 'CodigosPostales', Ajax.EndPoint.LeerElementos, filtros, ParametrosDeBusqueda(), null, false);
            const datos: Array<any> = peticion.resultado.datos;
            if (datos && datos.length > 0)
                return { id: datos[0].id, texto: datos[0].expresion };
        } catch { }
        return null;
    }

    function MapearDatosDeCalleParaCrear(panel: HTMLDivElement, objeto: any): void {
        const idPais: number = ObtenerPropiedad(objeto, Callejero.objeto.calleDto.idpais) as number;
        const pais: string = ObtenerPropiedad(objeto, Callejero.objeto.calleDto.pais) as string;

        const idProvincia: number = ObtenerPropiedad(objeto, Callejero.objeto.calleDto.idprovincia) as number;
        const provincia: string = ObtenerPropiedad(objeto, Callejero.objeto.calleDto.provincia) as string;

        const idMunicipio: number = ObtenerPropiedad(objeto, Callejero.objeto.calleDto.idmunicipio) as number;
        const municipio: string = ObtenerPropiedad(objeto, Callejero.objeto.calleDto.municipio) as string;

        const idtipovia: number = ObtenerPropiedad(objeto, Callejero.objeto.calleDto.idtipovia) as number;
        const tipovia: string = ObtenerPropiedad(objeto, Callejero.objeto.calleDto.tipovia) as string;

        const idcp: number = ObtenerPropiedad(objeto, Callejero.objeto.calleDto.idcp) as number;
        const cp: string = ObtenerPropiedad(objeto, Callejero.objeto.calleDto.cp) as string;

        let lista: HTMLInputElement = ApiControl.BuscarListaDinamicaPorPropiedad(panel, Callejero.objeto.calleDto.pais);
        MapearAlControl.ListaDinamica(lista, idPais, pais, true);

        lista = ApiControl.BuscarListaDinamicaPorPropiedad(panel, Callejero.objeto.calleDto.provincia);
        MapearAlControl.ListaDinamica(lista, idProvincia, provincia, true);

        lista = ApiControl.BuscarListaDinamicaPorPropiedad(panel, Callejero.objeto.calleDto.municipio);
        MapearAlControl.ListaDinamica(lista, idMunicipio, municipio, true);

        lista = ApiControl.BuscarListaDinamicaPorPropiedad(panel, Callejero.objeto.calleDto.tipovia);
        MapearAlControl.ListaDinamica(lista, idtipovia, tipovia, true);

        lista = ApiControl.BuscarListaDinamicaPorPropiedad(panel, Callejero.objeto.calleDto.cp);
        MapearAlControl.ListaDinamica(lista, idcp, cp, true);

        ApiControl.MapearEditor(panel, Callejero.objeto.calleDto.nombre, ObtenerPropiedad(objeto, Callejero.objeto.calleDto.nombre) as string)
    }

    export function MapearPais(crud: Crud.CrudMnt, objeto: any): void {
        let idPais: number = ObtenerPropiedad(Callejero.objeto.provinciaDto.idpais, objeto) as number;
        let pais: string = ObtenerPropiedad(Callejero.objeto.provinciaDto.pais, objeto) as string;

        let listaDeFiltro: HTMLInputElement = ApiControl.BuscarListaDinamicaPorPropiedad(crud.PanelFiltro, Callejero.atributo.propiedad.idpais);
        MapearAlControl.FijarValorEnListaDinamica(listaDeFiltro, idPais, pais);

        let listaDeCreacion: HTMLInputElement = ApiControl.BuscarListaDinamicaPorGuardarEn(crud.crudDeCreacion.PanelDeCrear, Callejero.atributo.guardarEn.idpais);
        MapearAlControl.FijarValorEnListaDinamica(listaDeCreacion, idPais, pais);

        let listaDeEdicion: HTMLInputElement = ApiControl.BuscarListaDinamicaPorGuardarEn(crud.crudDeEdicion.PanelDeEditar, Callejero.atributo.guardarEn.idpais);
        MapearAlControl.FijarValorEnListaDinamica(listaDeEdicion, idPais, pais);
    }

}