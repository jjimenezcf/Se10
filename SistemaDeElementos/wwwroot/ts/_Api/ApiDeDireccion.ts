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

    // abre en una pestaña nueva la creación de calles pasándole en la url el país, la provincia y el municipio indicados en la dirección,
    // para que el crud de calles los mapee al inicializar la creación (ver Callejero.CrudCreacionCalle)
    export function Direccion_Crear_Calle_En_Pestana(idLista: string, url: string): void {
        let tabla: HTMLDivElement = document.getElementById(idLista.replace('-calle', '')).parentElement as HTMLDivElement;
        let listas: Array<[string, string, string]> = [
            [ltrPropiedades.Callejero.Direccion.Pais, ltrPropiedades.Callejero.Calle.IdPais, ltrPropiedades.Callejero.Calle.Pais],
            [ltrPropiedades.Callejero.Direccion.Provincia, ltrPropiedades.Callejero.Calle.IdProvincia, ltrPropiedades.Callejero.Calle.Provincia],
            [ltrPropiedades.Callejero.Direccion.Municipio, ltrPropiedades.Callejero.Calle.IdMunicipio, ltrPropiedades.Callejero.Calle.Municipio]
        ];
        for (let [propiedad, parametroId, parametroTexto] of listas) {
            let lista: HTMLInputElement = ApiControl.BuscarListaDinamicaPorPropiedad(tabla, propiedad);
            let id: number = Definido(lista) ? Numero(lista.getAttribute(atListasDinamicas.idSeleccionado)) : 0;
            if (id > 0)
                url = `${url}&${parametroId}=${id}&${parametroTexto}=${encodeURIComponent(lista.value)}`;
        }
        EntornoSe.AbrirPestana(url);
    }

    // id de la modal que renderiza el DescriptorDeCrud de los negocios que usan direcciones (DescriptorDeCrud.IdModalDeMapaDeCalle)
    const idModalDeMapaDeCalle: string = 'modal-mapa-de-calle';

    // abre la modal del mapa con la calle seleccionada en la lista: se lee la calle para tener tipo de vía, municipio, provincia, país, zona y cp,
    // y se sitúa con el mismo visor que la edición de calles
    export function Direccion_Mostrar_Calle_En_Mapa(idLista: string): void {
        let lista: HTMLInputElement = document.getElementById(idLista) as HTMLInputElement;
        let idCalle: number = Numero(lista.getAttribute(atListasDinamicas.idSeleccionado));
        if (idCalle <= 0) {
            MensajesSe.Info('Seleccione una calle para verla en el mapa');
            return;
        }

        if (NoDefinido(document.getElementById(idModalDeMapaDeCalle))) {
            MensajesSe.Error('Direccion_Mostrar_Calle_En_Mapa', `La modal '${idModalDeMapaDeCalle}' no está renderizada en esta página`);
            return;
        }

        ApiDePeticiones.LeerElementoPorId(Crud.crudMnt, lista.getAttribute(atControl.controlador), idCalle, new Array<Parametro>(), idCalle)
            .then((peticion: ApiDeAjax.DescriptorAjax) => {
                let calle = peticion.resultado.datos;
                let pais: string = ObtenerPropiedad(calle, ltrPropiedades.Callejero.Calle.Pais).substring(6);
                let provincia: string = ObtenerPropiedad(calle, ltrPropiedades.Callejero.Calle.Provincia);
                let municipio: string = ObtenerPropiedad(calle, ltrPropiedades.Callejero.Calle.Municipio);
                let tipoDeVia: string = ObtenerPropiedad(calle, ltrPropiedades.Callejero.Calle.TipoDeVia);
                let nombre: string = ObtenerPropiedad(calle, ltrPropiedades.Callejero.Calle.Nombre);
                let zona: string = ObtenerPropiedad(calle, ltrPropiedades.Callejero.Calle.Zona, '');
                let cp: string = ObtenerPropiedad(calle, ltrPropiedades.Callejero.Calle.Cp, '');

                // se muestra antes de pintar el mapa para que el div tenga dimensiones reales
                ApiPanel.AbrirModalPorId(idModalDeMapaDeCalle);
                let divMapa: HTMLDivElement = document.getElementById(`${idModalDeMapaDeCalle}-mapa`) as HTMLDivElement;
                GestorDeMapas.MostrarVisorDeOpenStreetView(divMapa, pais, provincia, municipio, zona, tipoDeVia, nombre, cp);
            })
            .catch((peticion: ApiDeAjax.DescriptorAjax) => ApiDePeticiones.EmitirError(peticion));
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