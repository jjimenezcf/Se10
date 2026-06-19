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

    // ---- Asistente de mapas ----

    const _nominatimResultados: { [idAsistente: string]: any } = {};

    export function InicializarAsistenteDeMapas(crud: Crud.CrudMnt): void {
        const panel = crud.EstoyCreando ? crud.crudDeCreacion.PanelDeCrear : crud.crudDeEdicion.PanelDeEditar;
        const idAsistente = `${panel.id}-asistente-maps`;

        if (document.getElementById(idAsistente)) {
            const div = document.getElementById(idAsistente) as HTMLDivElement;
            const input = document.getElementById(`${idAsistente}-input`) as HTMLInputElement;
            if (input) input.value = '';

            if (!crud.EstoyCreando) {
                MontarAsistenteEnEdicion(panel, div);
            }

            return;
        }

        const divAsistente = CrearDivAsistente(idAsistente, crud.EstoyCreando);

        if (crud.EstoyCreando)
            MontarAsistenteEnCreacion(panel, divAsistente, idAsistente);
        else
            MontarAsistenteEnEdicion(panel, divAsistente);

        const divMapa = document.getElementById(`${idAsistente}-mapa`) as HTMLDivElement;
        GestorDeMapas.MostrarFrameStreetView(divMapa, ltrValores.Maestros.Callejero.Paises.Espana, 5);
    }

    function CrearDivAsistente(idAsistente: string, conBotonMapear: boolean): HTMLDivElement {
        const divAsistente = document.createElement('div');
        divAsistente.id = idAsistente;
        ApiControl.IncluirCss(divAsistente, ltrCss.crud.panelCreacion.AsistenteDelMapa);

        const divCabecera = document.createElement('div');
        ApiControl.IncluirCss(divCabecera, ltrCss.crud.panelCreacion.barraBusquedaMapa);

        const inputDireccion = document.createElement('input');
        inputDireccion.type = 'text';
        inputDireccion.id = `${idAsistente}-input`;
        inputDireccion.placeholder = 'Escriba la dirección a buscar...';
        inputDireccion.addEventListener('keydown', (e) => { if (e.key === 'Enter') BuscarEnMapsDesdeAsistente(idAsistente); });

        const btnBuscar = document.createElement('input');
        btnBuscar.type = 'button';
        btnBuscar.value = 'Buscar';
        btnBuscar.className = enumCssOpcionMenu.DeElemento;
        btnBuscar.onclick = () => BuscarEnMapsDesdeAsistente(idAsistente);

        divCabecera.appendChild(inputDireccion);
        divCabecera.appendChild(btnBuscar);

        if (conBotonMapear) {
            const btnMapear = document.createElement('input');
            btnMapear.type = 'button';
            btnMapear.id = `${idAsistente}-btn-mapear`;
            btnMapear.value = 'Mapear';
            btnMapear.title = 'Mapea los datos de la calle marcada para poder crear';
            btnMapear.className = enumCssOpcionMenu.DeElemento;
            btnMapear.style.display = 'none';
            btnMapear.onclick = () => MapearDesdeAsistente(idAsistente);
            divCabecera.appendChild(btnMapear);
        }

        const divMapa = document.createElement('div');
        divMapa.id = `${idAsistente}-mapa`;
        ApiControl.IncluirCss(divMapa, ltrCss.crud.panelCreacion.divMapa);

        divAsistente.appendChild(divCabecera);
        divAsistente.appendChild(divMapa);

        return divAsistente;
    }

    function MontarAsistenteEnCreacion(panel: HTMLDivElement, divAsistente: HTMLDivElement, idAsistente: string): void {
        const divCuerpo = panel.querySelector('.' + ltrCss.contenedorEdicionCuerpo) as HTMLDivElement;
        if (!divCuerpo) return;

        const divCuerpoCreacion = divCuerpo.closest('.' + ltrCss.crud.creacion) as HTMLDivElement;
        if (divCuerpoCreacion) ApiControl.IncluirCss(divCuerpo, ltrCss.crud.panelCreacion.CuerpoCreacionConMapa);
        ApiControl.IncluirCss(divCuerpo, ltrCss.crud.panelCreacion.CreacionConMapa);

        const primerHijo = divCuerpo.firstElementChild as HTMLElement;
        if (primerHijo) ApiControl.IncluirCss(primerHijo, ltrCss.crud.panelCreacion.DtoConMapa);

        const divSplitter = document.createElement('div');
        divSplitter.id = `${idAsistente}-splitter`;
        divSplitter.title = 'Arrastre para redimensionar';
        ApiControl.IncluirCss(divSplitter, 'splitter');
        ApiControl.IncluirCss(divSplitter, ltrCss.crud.panelCreacion.SplitterMapa);

        divCuerpo.appendChild(divSplitter);
        divCuerpo.appendChild(divAsistente);

        InicializarSplitter(divSplitter, primerHijo, divAsistente, divCuerpo);
    }

    function MontarAsistenteEnEdicion(panel: HTMLDivElement, divAsistente: HTMLDivElement): void {
        const divContenedorEditor = panel.querySelector('.' + ltrCss.crud.panelDeEdicion.ContenedorDeDatosMasVisor) as HTMLDivElement;
        if (!divContenedorEditor) return;

        ApiControl.ExcluirCss(divContenedorEditor, ltrCss.crud.panelDeEdicion.VisorOculto);
        const divContenedorNavegador = panel.querySelector('.' + ltrCss.crud.panelDeEdicion.NavegadorDeEdicionCuerpoVisor) as HTMLDivElement;
        
        ApiControl.IncluirCss(divContenedorNavegador, ltrCss.divNoVisible);

        const divContenedorVisor = divContenedorEditor.querySelector('.' + ltrCss.crud.panelDeEdicion.ContenedorVisor) as HTMLDivElement;
        if (!divContenedorVisor) return;

        const divVisorDeEdicion = divContenedorVisor.querySelector('.' + ltrCss.crud.panelDeEdicion.VisorDeEdicion) as HTMLDivElement;
        if (!divVisorDeEdicion) return;

        divVisorDeEdicion.innerHTML = '';
        ApiControl.IncluirCss(divAsistente, ltrCss.crud.panelDeEdicion.AsistenteMapaEnEdicion);
        divVisorDeEdicion.appendChild(divAsistente);
    }

    function InicializarSplitter(splitter: HTMLDivElement, panelIzq: HTMLElement, panelDer: HTMLElement, contenedor: HTMLElement): void {
        const anchoSplitter = splitter.offsetWidth || 6;
        let arrastrando = false;
        let ultimaEjecucion: number = undefined;

        const onMouseDown = (e: MouseEvent) => {
            e.preventDefault();
            e.stopPropagation();
            arrastrando = true;
            ultimaEjecucion = undefined;
            document.body.style.cursor = 'col-resize';
            document.body.style.userSelect = 'none';
            document.addEventListener('mousemove', onMouseMove);
            document.addEventListener('mouseup', onMouseUp);
            document.addEventListener('mouseleave', onMouseUp);
        };

        const onMouseMove = (e: MouseEvent) => {
            e.preventDefault();
            e.stopPropagation();
            if ((e.buttons & 1) === 0) { onMouseUp(e); return; }
            if (!arrastrando) return;
            if (ultimaEjecucion && Date.now() - ultimaEjecucion < 16) return;
            ultimaEjecucion = Date.now();

            const rect = contenedor.getBoundingClientRect();
            const nuevoAnchoIzq = e.clientX - rect.left;
            const nuevoAnchoDer = rect.width - nuevoAnchoIzq - anchoSplitter;
            if (nuevoAnchoIzq < 150 || nuevoAnchoDer < 150) return;

            requestAnimationFrame(() => {
                panelIzq.style.flex = 'none';
                panelIzq.style.width = `${nuevoAnchoIzq}px`;
                panelDer.style.flex = 'none';
                panelDer.style.width = `${nuevoAnchoDer}px`;
            });
        };

        const onMouseUp = (e: MouseEvent) => {
            if (!arrastrando) return;
            arrastrando = false;
            ultimaEjecucion = undefined;
            document.body.style.cursor = '';
            document.body.style.userSelect = '';
            document.removeEventListener('mousemove', onMouseMove);
            document.removeEventListener('mouseup', onMouseUp);
            document.removeEventListener('mouseleave', onMouseUp);
        };

        splitter.addEventListener('mousedown', onMouseDown);

        window.addEventListener('resize', () => {
            if (!panelIzq.style.width) return;
            const rect = contenedor.getBoundingClientRect();
            const izqPx = parseFloat(panelIzq.style.width);
            if (isNaN(izqPx)) return;
            const total = rect.width - anchoSplitter;
            const pct = izqPx / (izqPx + (parseFloat(panelDer.style.width) || total));
            panelIzq.style.width = `${total * pct}px`;
            panelDer.style.width = `${total * (1 - pct)}px`;
        });
    }

    async function BuscarEnMapsDesdeAsistente(idAsistente: string): Promise<void> {
        const input = document.getElementById(`${idAsistente}-input`) as HTMLInputElement;
        if (!Definido(input) || IsNullOrEmpty(input.value)) {
            MensajesSe.Info('Indique una dirección para buscar');
            return;
        }
        const divMapa = document.getElementById(`${idAsistente}-mapa`) as HTMLDivElement;
        const btnMapear = document.getElementById(`${idAsistente}-btn-mapear`) as HTMLInputElement;
        delete _nominatimResultados[idAsistente];
        if (btnMapear) btnMapear.style.display = 'none';

        // 💡 Una única llamada: Renderiza el mapa Y nos devuelve el objeto de resultados de Nominatim
        const resultado = await GestorDeMapas.MostrarFrameStreetView(divMapa, input.value, 0.002);

        // Si la función localizó la dirección, guardamos el resultado para el proceso de guardado/mapeo
        if (resultado) {
            _nominatimResultados[idAsistente] = resultado;
            if (btnMapear && Crud.crudMnt.EstoyCreando) btnMapear.style.display = '';
        }
    }

    function ParsearRuta(ruta: string): { tipoDeVia: string, nombre: string } {
        if (IsNullOrEmpty(ruta)) return { tipoDeVia: '', nombre: '' };
        const partes = ruta.trim().split(' ');
        if (partes.length <= 1) return { tipoDeVia: '', nombre: ruta };
        return { tipoDeVia: partes[0], nombre: partes.slice(1).join(' ') };
    }

    function MapearDesdeAsistente(idAsistente: string): void {
        const resultado = _nominatimResultados[idAsistente];
        if (!resultado) return;
        const addr = resultado.address;
        const { tipoDeVia, nombre } = ParsearRuta(addr.road || '');
        const municipioNombre = addr.city || addr.town || addr.village || addr.municipality || '';
        const cpCodigo = addr.postcode || '';
        ValidarSiExisteCalle(Crud.crudMnt.crudDeCreacion.PanelDeCrear, tipoDeVia, nombre, municipioNombre, cpCodigo);
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