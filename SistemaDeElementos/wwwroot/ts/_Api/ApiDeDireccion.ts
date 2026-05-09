namespace ApiDeDireccion {
    export function Direccion_Tras_Seleccionar_Calle(idLista: string): void {
        let lista: HTMLInputElement = document.getElementById(idLista) as HTMLInputElement;
        let objeto = OpcionesDeLasListas.ObtenerObjeto(lista);
        if (!Definido(objeto))
            return;
        let tabla: HTMLDivElement = document.getElementById(idLista.replace('-calle', '')).parentElement as HTMLDivElement;
        let pais: HTMLInputElement = ApiControl.BuscarListaDinamicaPorPropiedad(tabla, ltrPropiedades.Callejero.Direccion.Pais);
        let provincia: HTMLInputElement = ApiControl.BuscarListaDinamicaPorPropiedad(tabla, ltrPropiedades.Callejero.Direccion.Provincia);
        let municipio: HTMLInputElement = ApiControl.BuscarListaDinamicaPorPropiedad(tabla, ltrPropiedades.Callejero.Direccion.Municipio);
        MapearAlControl.ListaDinamicaSinAcciones(pais, ObtenerPropiedad(objeto, ltrPropiedades.Callejero.Direccion.IdPais), ObtenerPropiedad(objeto, ltrPropiedades.Callejero.Direccion.Pais));
        MapearAlControl.ListaDinamicaSinAcciones(provincia, ObtenerPropiedad(objeto, ltrPropiedades.Callejero.Direccion.IdProvincia), ObtenerPropiedad(objeto, ltrPropiedades.Callejero.Direccion.Provincia));
        MapearAlControl.ListaDinamicaSinAcciones(municipio, ObtenerPropiedad(objeto, ltrPropiedades.Callejero.Direccion.IdMunicipio), ObtenerPropiedad(objeto, ltrPropiedades.Callejero.Direccion.Municipio));
        Direccion_Tras_Blanquear_Calle(idLista);

        let cp: HTMLInputElement = ApiControl.BuscarListaDinamicaPorPropiedad(tabla, ltrPropiedades.Callejero.Direccion.Cp);
        let idCp = ObtenerPropiedad(objeto, ltrPropiedades.Callejero.Direccion.IdCp);
        if (Numero(idCp) > 0) MapearAlControl.ListaDinamicaSinAcciones(cp, idCp, ObtenerPropiedad(objeto, ltrPropiedades.Callejero.Direccion.Cp));

        let barrio: HTMLInputElement = ApiControl.BuscarListaDinamicaPorPropiedad(tabla, ltrPropiedades.Callejero.Direccion.Barrio);
        if (Definido(barrio)) {
            let idBarrio = ObtenerPropiedad(objeto, ltrPropiedades.Callejero.Direccion.IdBarrio);
            if (Numero(idBarrio) > 0) MapearAlControl.ListaDinamicaSinAcciones(barrio, idBarrio, ObtenerPropiedad(objeto, ltrPropiedades.Callejero.Direccion.Barrio));
        }

        let zona: HTMLInputElement = ApiControl.BuscarListaDinamicaPorPropiedad(tabla, ltrPropiedades.Callejero.Direccion.Zona);
        if (Definido(zona)) {
            let idZona = ObtenerPropiedad(objeto, ltrPropiedades.Callejero.Direccion.IdZona);
            if (Numero(idZona) > 0) MapearAlControl.ListaDinamicaSinAcciones(zona, idZona, ObtenerPropiedad(objeto, ltrPropiedades.Callejero.Direccion.Zona));
        }
    }
    export function Direccion_Tras_Blanquear_Calle(idLista: string): void {
        let tabla: HTMLDivElement = document.getElementById(idLista.replace('-calle', '')).parentElement as HTMLDivElement;
        ApiControl.BlanquearListaDinamicaPorPropiedad(tabla, ltrPropiedades.Callejero.Direccion.Barrio);
        ApiControl.BlanquearListaDinamicaPorPropiedad(tabla, ltrPropiedades.Callejero.Direccion.Zona);
        ApiControl.BlanquearListaDinamicaPorPropiedad(tabla, ltrPropiedades.Callejero.Direccion.Cp);

        let numero: HTMLInputElement = ApiControl.BuscarEditor(tabla, ltrPropiedades.Callejero.Direccion.Numero);
        let escalera: HTMLInputElement = ApiControl.BuscarEditor(tabla, ltrPropiedades.Callejero.Direccion.Escalera);
        let piso: HTMLInputElement = ApiControl.BuscarEditor(tabla, ltrPropiedades.Callejero.Direccion.Piso);
        let puerta: HTMLInputElement = ApiControl.BuscarEditor(tabla, ltrPropiedades.Callejero.Direccion.Puerta);
        numero.value = '';
        escalera.value = '';
        piso.value = '';
        puerta.value = '';
    }

    export function Direccion_Tras_Seleccionar_Municipio(idLista: string): void {
        let lista: HTMLInputElement = document.getElementById(idLista) as HTMLInputElement;
        let objeto = OpcionesDeLasListas.ObtenerObjeto(lista);
        if (!Definido(objeto))
            return;
        let tabla: HTMLDivElement = document.getElementById(idLista.replace('-municipio', '')).parentElement as HTMLDivElement;
        let pais: HTMLInputElement = ApiControl.BuscarListaDinamicaPorPropiedad(tabla, ltrPropiedades.Callejero.Direccion.Pais);
        let provincia: HTMLInputElement = ApiControl.BuscarListaDinamicaPorPropiedad(tabla, ltrPropiedades.Callejero.Direccion.Provincia);
        MapearAlControl.ListaDinamicaSinAcciones(pais, ObtenerPropiedad(objeto, ltrPropiedades.Callejero.Direccion.IdPais), ObtenerPropiedad(objeto, ltrPropiedades.Callejero.Direccion.Pais));
        MapearAlControl.ListaDinamicaSinAcciones(provincia, ObtenerPropiedad(objeto, ltrPropiedades.Callejero.Direccion.IdProvincia), ObtenerPropiedad(objeto, ltrPropiedades.Callejero.Direccion.Provincia));
    }

    export function Direccion_Tras_Seleccionar_Provincia(idLista: string): void {
        let lista: HTMLInputElement = document.getElementById(idLista) as HTMLInputElement;
        let objeto = OpcionesDeLasListas.ObtenerObjeto(lista);
        if (!Definido(objeto))
            return;
        let tabla: HTMLDivElement = document.getElementById(idLista.replace('-provincia', '')).parentElement as HTMLDivElement;
        let pais: HTMLInputElement = ApiControl.BuscarListaDinamicaPorPropiedad(tabla, ltrPropiedades.Callejero.Direccion.Pais);
        MapearAlControl.ListaDinamicaSinAcciones(pais, ObtenerPropiedad(objeto, ltrPropiedades.Callejero.Direccion.IdPais), ObtenerPropiedad(objeto, ltrPropiedades.Callejero.Direccion.Pais));
    }


    export function Calle_Tras_Seleccionar_Municipio(idLista: string): void {
        let lista: HTMLInputElement = document.getElementById(idLista) as HTMLInputElement;
        let objeto = OpcionesDeLasListas.ObtenerObjeto(lista);
        if (!Definido(objeto))
            return;
        let tabla: HTMLDivElement = document.getElementById(idLista.replace('-municipio', '')).parentElement as HTMLDivElement;
        let pais: HTMLInputElement = ApiControl.BuscarListaDinamicaPorPropiedad(tabla, ltrPropiedades.Callejero.Calle.Pais);
        let provincia: HTMLInputElement = ApiControl.BuscarListaDinamicaPorPropiedad(tabla, ltrPropiedades.Callejero.Calle.Provincia);
        MapearAlControl.ListaDinamicaSinAcciones(pais, ObtenerPropiedad(objeto, ltrPropiedades.Callejero.Calle.IdPais), ObtenerPropiedad(objeto, ltrPropiedades.Callejero.Calle.Pais));
        MapearAlControl.ListaDinamicaSinAcciones(provincia, ObtenerPropiedad(objeto, ltrPropiedades.Callejero.Calle.IdProvincia), ObtenerPropiedad(objeto, ltrPropiedades.Callejero.Calle.Provincia));
    }

    export function Calle_Tras_Seleccionar_Provincia(idLista: string): void {
        let lista: HTMLInputElement = document.getElementById(idLista) as HTMLInputElement;
        let objeto = OpcionesDeLasListas.ObtenerObjeto(lista);
        if (!Definido(objeto))
            return;
        let tabla: HTMLDivElement = document.getElementById(idLista.replace('-provincia', '')).parentElement as HTMLDivElement;
        let pais: HTMLInputElement = ApiControl.BuscarListaDinamicaPorPropiedad(tabla, ltrPropiedades.Callejero.Calle.Pais);
        MapearAlControl.ListaDinamicaSinAcciones(pais, ObtenerPropiedad(objeto, ltrPropiedades.Callejero.Calle.IdPais), ObtenerPropiedad(objeto, ltrPropiedades.Callejero.Calle.Pais));
    }

    export function FijarCalificador(panel: HTMLDivElement, ampliacion: string, calificador: string): HTMLDivElement {
        var ampliacionDiv = ApiPanel.BuscarAmpliacion(panel, ltrAmpliaciones.Comunes.CrearDireccion)
        if (Definido(ampliacion)) {
            var lista = ApiControl.BuscarListaDeValores(ampliacionDiv, ltrPropiedades.Callejero.Direccion.Calificador)
            enumCssOpcionMenu
            const opcionEjecucion = lista.querySelector(`option[value="${calificador}"]`);
            if (opcionEjecucion) {
                lista.selectedIndex = opcionEjecucion['index'];
            }
        }
        return ampliacionDiv;
    }

}