namespace ApiDelCrud {

    export function ProponerSolicitante(editor: Crud.CrudEdicion, modal: HTMLDivElement) {
        if (editor.ModoTrabajo === enumModoTrabajo.mantenimiento) {
            let id: number = ObtenerPropiedad(editor.CrudDeMnt.InfoSelector.Seleccionados[0].Registro, literal.IdSolicitante, 0);
            let texto: string = ObtenerPropiedad(editor.CrudDeMnt.InfoSelector.Seleccionados[0].Registro, literal.Solicitante, "");
            let lista: HTMLInputElement = ApiControl.BuscarControl(editor.PanelDeEditar, literal.Solicitante, true) as HTMLInputElement;
            ApiListaDinamica.AsignarValor(lista, id, texto);
        }
        ApiDelCrud.ProponerPropiedad(editor.PanelDeEditar, modal, literal.Solicitante, false);
    }

    export function ProponerElTipo(modal: HTMLDivElement, valores: string) {
        const comandos = valores.split(ltrSimbolos.separadorDeValores);
        const comandoTipo = comandos.find(c => c.startsWith(ltrAccionesModal.ProponerElTipo));

        if (comandoTipo) {
            const partes = comandoTipo.split(ltrSimbolos.igual);

            // Validamos que existan exactamente los 3 elementos (comando, id, texto)
            if (partes.length !== 3) {
                MensajesSe.Error('ProponerElTipo', `Formato incorrecto en '${ltrAccionesModal.ProponerElTipo}'. Se esperaba '${ltrAccionesModal.ProponerElTipo}=id=texto' pero se recibió: '${comandoTipo}'`);
            }

            const idStr = partes[1];
            const texto = partes[2];
            const id = Numero(idStr);

            if (id === 0) {
                MensajesSe.Error('ProponerElTipo', `El valor ID '${idStr}' para proponer el tipo no es un número válido mayor que 0.`);
            }

            let lista: HTMLInputElement = ApiControl.BuscarListaDinamicaPorPropiedad(modal, ltrPropiedades.Elemento.ConTipo.Tipo) as HTMLInputElement;
            ApiListaDinamica.AsignarValor(lista, id, texto);
        }
    }

    export function MapearElementoEnModalDeDetalle(editor: Crud.CrudEdicion, modal: HTMLDivElement) {
        if (editor.ModoTrabajo === enumModoTrabajo.mantenimiento) {
            let id: number = ObtenerPropiedad(editor.CrudDeMnt.InfoSelector.Seleccionados[0].Registro, ltrPropiedades.Elemento.Id, 0);
            let texto: string = ObtenerPropiedad(editor.CrudDeMnt.InfoSelector.Seleccionados[0].Registro, ltrPropiedades.Elemento.Expresion, "");
            MapearAlControl.RestrictoresDeEdicion(modal, ltrPropiedades.Elemento.IdElemento, id, texto);
        }
        else {
            let id: number = ObtenerPropiedad(editor.Registro, ltrPropiedades.Elemento.Id, 0);
            let texto: string = ObtenerPropiedad(editor.Registro, ltrPropiedades.Elemento.Expresion, "");
            MapearAlControl.RestrictoresDeEdicion(modal, ltrPropiedades.Elemento.IdElemento, id, texto);
        }
    }

    export function MapearControlesDesdeElCrudAlJson(padre: Crud.CrudBase | Formulario.Base, panel: HTMLDivElement, modoDeTrabajo: string): JSON {
        if (padre instanceof Crud.CrudBase) {
            let elementoJson: JSON = padre.AntesDeMapearDatosDeIU(panel, modoDeTrabajo);
            elementoJson = ApiPanel.MapearControlesDesdeElPanelAlJson(panel, elementoJson);
            return padre.DespuesDeMapearDatosDeIU(padre, panel, elementoJson, modoDeTrabajo);
        }
        let elementoJson: JSON = padre.AntesDeMapearDatosDeIU(panel, modoDeTrabajo);
        elementoJson = ApiPanel.MapearControlesDesdeElPanelAlJson(panel, elementoJson);
        return padre.DespuesDeMapearDatosDeIU(padre, panel, elementoJson, modoDeTrabajo);

    }

    export function ProponerEnListaDinamica(modal: HTMLDivElement, registro: any, propiedad: string, propiedadId: string, erroSiNodDefinido: boolean = true) {
        let destino = ApiControl.BuscarControl(modal, propiedad, true) as HTMLInputElement;
        let id = ObtenerPropiedad(registro, propiedadId);
        if (!Definido(id)) {
            if (!erroSiNodDefinido) {
                ApiDeInicializacion.InicializarListaDinamica(destino);
                ApiListaDinamica.Blanquear(destino, false);
                return;
            }
            MensajesSe.EmitirExcepcion('ProponerEnListaDinamica', `No se puede asignar la propiedad ${propiedad} por no tener valor`);
        }
        destino.value = "";
        let informacion: { cargar: boolean; controlador: string; filtros: Array<ClausulaDeFiltrado>; parametros: Array<Parametro>; datosDeEntrada: string }
            = ApiListaDinamica.DefinirFiltrosParaCargar(destino, false);

        let clausula: ClausulaDeFiltrado = new ClausulaDeFiltrado(ltrPropiedades.Elemento.Id, atCriterio.igual, id);
        informacion.filtros.push(clausula);

        ApiDePeticiones.LeerElementos(destino, informacion.controlador, Ajax.EndPoint.LeerElementos, informacion.filtros, informacion.parametros, informacion.datosDeEntrada)
            .then((peticion) => {
                if (peticion.resultado.datos.length === 1) {
                    let texto = ObtenerPropiedad(registro, propiedad);
                    ApiListaDinamica.AsignarValor(destino, id, texto);
                }
            });
    }

    export function ProponerEnEditor(modal: HTMLDivElement, registro: any, propiedad: string) {
        let destino = ApiControl.BuscarControl(modal, propiedad, true) as HTMLInputElement;
        destino.value = ObtenerPropiedad(registro, propiedad);
    }

    export function MultiplicarPorMenos(modal: HTMLDivElement, registro: any, propiedad: string) {
        let destino = ApiControl.BuscarControl(modal, propiedad, true) as HTMLInputElement;
        const valor = Numero(ObtenerPropiedad(registro, propiedad));
        destino.value = (valor * -1).toString();
    }

    export function ProponerEnAreaDeTexto(modal: HTMLDivElement, registro: any, propiedad: string) {
        let destino = ApiControl.BuscarControl(modal, propiedad, true) as HTMLTextAreaElement;
        destino.value = ObtenerPropiedad(registro, propiedad);
    }

    export function ProponerEnRestrictor(modal: HTMLDivElement, registro: any, propiedad: string, propiedadId: string) {
        let destino = ApiControl.BuscarControl(modal, propiedadId, true) as HTMLInputElement;
        MapearAlControl.Restrictor(destino, ObtenerPropiedad(registro, propiedadId), ObtenerPropiedad(registro, propiedad));
    }

    export function ProponerParaRenombrar(modal: HTMLDivElement, registro: any) {
        let destino = ApiControl.BuscarRestrictor(modal, ltrPropiedades.Elemento.IdElemento, ltrTipoControl.restrictorDeEdicion) as HTMLInputElement;
        MapearAlControl.Restrictor(destino, ObtenerPropiedad(registro, literal.id), ObtenerPropiedad(registro, literal.nombre));
    }

    export function ProponerParaCambiarProveedor(modal: HTMLDivElement, registro: any) {
        let destino = ApiControl.BuscarRestrictor(modal, ltrPropiedades.Elemento.IdElemento, ltrTipoControl.restrictorDeEdicion) as HTMLInputElement;
        MapearAlControl.Restrictor(destino, ObtenerPropiedad(registro, literal.id), ObtenerPropiedad(registro, literal.nombre));
    }


    export function ProponerPropiedad(pnlOrigen: HTMLDivElement, pnlDestino: HTMLDivElement, propiedad: string, bloquear: boolean): void {
        MapearAlPanel.MapearPropiedadDesdeOtroPanel(pnlOrigen, pnlDestino, propiedad, bloquear);
    }

    export function OpcionMf(opcion: string): HTMLLIElement {
        let li: HTMLLIElement = ApiDeMenuFlotante.BuscarMf(this.CrudDeMnt.ContenedorDelMenuDelCrud, opcion);
        if (NoDefinido(li))
            MensajesSe.EmitirExcepcion("OpcionMf", `no se ha localizado la opción de menú ${opcion}`);
        return li;
    }

    export function InicializarDatosDelCertificado(panel: HTMLDivElement) {
        let datos: HTMLTextAreaElement = ApiControl.BuscarControl(panel, ltrPropiedades.Entorno.Usuario.DatosCertificado, false) as HTMLTextAreaElement;
        if (Definido(datos)) {
            datos.parentElement.parentElement.classList.remove(ltrCss.divNoVisible);
            datos.parentElement.parentElement.classList.add(ltrCss.divVisible);
        }

        let password: HTMLInputElement = ApiControl.BuscarControl(panel, ltrPropiedades.Entorno.Usuario.PassworDelCertificado, true) as HTMLInputElement;
        password.parentElement.parentElement.classList.remove(ltrCss.divVisible);
        password.parentElement.parentElement.classList.add(ltrCss.divNoVisible);
    }

    export function BlanquearPasswordDeCertificado(panel: HTMLDivElement) {
        let password: HTMLInputElement = ApiControl.BuscarControl(panel, ltrPropiedades.Entorno.Usuario.PassworDelCertificado, true) as HTMLInputElement;
        password.value = '';

        let archivo: HTMLInputElement = ApiControl.BuscarControl(panel, ltrPropiedades.Entorno.Usuario.IdArchivoCertificado, true) as HTMLInputElement;
        if (Numero(archivo.getAttribute(atArchivo.idArchivo)) > 0) {
            let datos: HTMLTextAreaElement = ApiControl.BuscarControl(panel, ltrPropiedades.Entorno.Usuario.DatosCertificado, false) as HTMLTextAreaElement;
            if (Definido(datos)) {
                datos.parentElement.parentElement.classList.add(ltrCss.divNoVisible);
                datos.parentElement.parentElement.classList.remove(ltrCss.divVisible);
            }
            password.parentElement.parentElement.classList.add(ltrCss.divVisible);
            password.parentElement.parentElement.classList.remove(ltrCss.divNoVisible);
        }
    }

    export function MostrarPanelDeCreacion() {
        let panelCabecera = (document.getElementsByClassName(ltrCss.crud.cabecera)[0]) as HTMLDivElement;
        let panelDatos = (document.getElementsByClassName(ltrCss.crud.datos)[0]) as HTMLDivElement;
        let filtro = (document.getElementsByClassName(ltrCss.crud.grid.filto)[0]) as HTMLDivElement;
        let grid = (document.getElementsByClassName(ltrCss.crud.grid.grid)[0]) as HTMLDivElement;
        let panelPie = (document.getElementsByClassName(ltrCss.crud.pie)[0]) as HTMLDivElement;

        const creacionDiv = document.getElementsByClassName(ltrCss.crud.creacion);
        if (creacionDiv.length === 0) {
            MensajesSe.EmitirExcepcion("MostrarPanelDeCreacion", "No se ha encontrado el panel de creación del CRUD");
            return;
        };

        const edicionDiv = document.getElementsByClassName(ltrCss.crud.edicion);
        const historialDiv = document.getElementsByClassName(ltrCss.crud.PanelHistorial.historial);

        let panelCreacion = (creacionDiv[0]) as HTMLDivElement;
        let panelEdicion = (edicionDiv[0]) as HTMLDivElement;
        let panelHistorial = (historialDiv[0]) as HTMLDivElement;
        let crud = (document.getElementsByClassName(ltrCss.crud.cuerpo)[0]) as HTMLDivElement;
        ApiPanel.OcultarPanel(panelCabecera);
        ApiPanel.OcultarPanel(panelDatos);
        ApiPanel.OcultarPanel(filtro);
        ApiPanel.OcultarPanel(grid);
        ApiPanel.OcultarPanel(panelPie);
        ApiPanel.OcultarPanel(panelEdicion);
        ApiPanel.OcultarPanel(panelHistorial);
        crud.classList.add(ltrCss.crud.creando);
        ApiPanel.MostrarPanel(panelCreacion);
    }

    export function OcultarPanelDeCreacion() {
        let panelCabecera = (document.getElementsByClassName(ltrCss.crud.cabecera)[0]) as HTMLDivElement;
        let panelDatos = (document.getElementsByClassName(ltrCss.crud.datos)[0]) as HTMLDivElement;
        let filtro = (document.getElementsByClassName(ltrCss.crud.grid.filto)[0]) as HTMLDivElement;
        let grid = (document.getElementsByClassName(ltrCss.crud.grid.grid)[0]) as HTMLDivElement;
        let panelPie = (document.getElementsByClassName(ltrCss.crud.pie)[0]) as HTMLDivElement;


        let panelCreacion = (document.getElementsByClassName(ltrCss.crud.creacion)[0]) as HTMLDivElement;
        let panelEdicion = (document.getElementsByClassName(ltrCss.crud.edicion)[0]) as HTMLDivElement;
        let panelHistorial = (document.getElementsByClassName(ltrCss.crud.PanelHistorial.historial)[0]) as HTMLDivElement;
        let crud = (document.getElementsByClassName(ltrCss.crud.cuerpo)[0]) as HTMLDivElement;
        ApiPanel.MostrarPanel(panelCabecera);
        ApiPanel.MostrarPanel(panelDatos);
        ApiPanel.MostrarPanel(filtro);
        ApiPanel.MostrarPanel(grid);
        ApiPanel.MostrarPanel(panelPie);
        crud.classList.remove(ltrCss.crud.creando);
        ApiPanel.OcultarPanel(panelCreacion);
        ApiPanel.OcultarPanel(panelEdicion);
        ApiPanel.OcultarPanel(panelHistorial);
    }

    export function MostrarPanelDeEdicion() {
        let panelCabecera = (document.getElementsByClassName(ltrCss.crud.cabecera)[0]) as HTMLDivElement;
        let panelDatos = (document.getElementsByClassName(ltrCss.crud.datos)[0]) as HTMLDivElement;
        let filtro = (document.getElementsByClassName(ltrCss.crud.grid.filto)[0]) as HTMLDivElement;
        let grid = (document.getElementsByClassName(ltrCss.crud.grid.grid)[0]) as HTMLDivElement;
        let panelPie = (document.getElementsByClassName(ltrCss.crud.pie)[0]) as HTMLDivElement;

        const edicionDiv = document.getElementsByClassName(ltrCss.crud.edicion);
        if (edicionDiv.length === 0) {
            MensajesSe.EmitirExcepcion("MostrarPanelDeEdicion", "No se ha encontrado el panel de edición del CRUD");
            return;
        };
        const creacionDiv = document.getElementsByClassName(ltrCss.crud.creacion);
        const historialDiv = document.getElementsByClassName(ltrCss.crud.PanelHistorial.historial);

        let panelCreacion = (creacionDiv[0]) as HTMLDivElement;
        let panelEdicion = (edicionDiv[0]) as HTMLDivElement;
        let panelHistorial = (historialDiv[0]) as HTMLDivElement;
        let crud = (document.getElementsByClassName(ltrCss.crud.cuerpo)[0]) as HTMLDivElement;
        ApiPanel.OcultarPanel(panelCabecera);
        ApiPanel.OcultarPanel(panelDatos);
        ApiPanel.OcultarPanel(filtro);
        ApiPanel.OcultarPanel(grid);
        ApiPanel.OcultarPanel(panelPie);
        ApiPanel.OcultarPanel(panelCreacion);
        ApiPanel.OcultarPanel(panelHistorial);
        crud.classList.add(ltrCss.crud.editando);
        ApiPanel.MostrarPanel(panelEdicion);
    }

    export function OcultarPanelDeEdicion() {
        let panelCabecera = (document.getElementsByClassName(ltrCss.crud.cabecera)[0]) as HTMLDivElement;
        let panelDatos = (document.getElementsByClassName(ltrCss.crud.datos)[0]) as HTMLDivElement;
        let filtro = (document.getElementsByClassName(ltrCss.crud.grid.filto)[0]) as HTMLDivElement;
        let grid = (document.getElementsByClassName(ltrCss.crud.grid.grid)[0]) as HTMLDivElement;
        let panelPie = (document.getElementsByClassName(ltrCss.crud.pie)[0]) as HTMLDivElement;
        let panelCreacion = (document.getElementsByClassName(ltrCss.crud.creacion)[0]) as HTMLDivElement;
        let panelEdicion = (document.getElementsByClassName(ltrCss.crud.edicion)[0]) as HTMLDivElement;
        let panelHistorial = (document.getElementsByClassName(ltrCss.crud.PanelHistorial.historial)[0]) as HTMLDivElement;
        let crud = (document.getElementsByClassName(ltrCss.crud.cuerpo)[0]) as HTMLDivElement;
        ApiPanel.MostrarPanel(panelCabecera);
        ApiPanel.MostrarPanel(panelDatos);
        ApiPanel.MostrarPanel(filtro);
        ApiPanel.MostrarPanel(grid);
        ApiPanel.MostrarPanel(panelPie);
        crud.classList.remove(ltrCss.crud.editando);
        ApiPanel.OcultarPanel(panelCreacion);
        ApiPanel.OcultarPanel(panelEdicion);
        ApiPanel.OcultarPanel(panelHistorial);
    }
    export function MostrarPanelDeHistorial() {
        let panelCabecera = (document.getElementsByClassName(ltrCss.crud.cabecera)[0]) as HTMLDivElement;
        let panelDatos = (document.getElementsByClassName(ltrCss.crud.datos)[0]) as HTMLDivElement;
        let filtro = (document.getElementsByClassName(ltrCss.crud.grid.filto)[0]) as HTMLDivElement;
        let grid = (document.getElementsByClassName(ltrCss.crud.grid.grid)[0]) as HTMLDivElement;
        let panelPie = (document.getElementsByClassName(ltrCss.crud.pie)[0]) as HTMLDivElement;
        let panelCreacion = (document.getElementsByClassName(ltrCss.crud.creacion)[0]) as HTMLDivElement;
        let panelEdicion = (document.getElementsByClassName(ltrCss.crud.edicion)[0]) as HTMLDivElement;
        let panelHistorial = (document.getElementsByClassName(ltrCss.crud.PanelHistorial.historial)[0]) as HTMLDivElement;
        let crud = (document.getElementsByClassName(ltrCss.crud.cuerpo)[0]) as HTMLDivElement;
        ApiPanel.OcultarPanel(panelCabecera);
        ApiPanel.OcultarPanel(panelDatos);
        ApiPanel.OcultarPanel(filtro);
        ApiPanel.OcultarPanel(grid);
        ApiPanel.OcultarPanel(panelPie);
        ApiPanel.OcultarPanel(panelCreacion);
        ApiPanel.OcultarPanel(panelEdicion);
        crud.classList.add(ltrCss.crud.mostrandoHistorial);
        ApiPanel.MostrarPanel(panelHistorial);
    }
    export function OcultarPanelDeHistorial() {
        let panelCabecera = (document.getElementsByClassName(ltrCss.crud.cabecera)[0]) as HTMLDivElement;
        let panelDatos = (document.getElementsByClassName(ltrCss.crud.datos)[0]) as HTMLDivElement;
        let filtro = (document.getElementsByClassName(ltrCss.crud.grid.filto)[0]) as HTMLDivElement;
        let grid = (document.getElementsByClassName(ltrCss.crud.grid.grid)[0]) as HTMLDivElement;
        let panelPie = (document.getElementsByClassName(ltrCss.crud.pie)[0]) as HTMLDivElement;
        let panelCreacion = (document.getElementsByClassName(ltrCss.crud.creacion)[0]) as HTMLDivElement;
        let panelEdicion = (document.getElementsByClassName(ltrCss.crud.edicion)[0]) as HTMLDivElement;
        let panelHistorial = (document.getElementsByClassName(ltrCss.crud.PanelHistorial.historial)[0]) as HTMLDivElement;
        let crud = (document.getElementsByClassName(ltrCss.crud.cuerpo)[0]) as HTMLDivElement;
        ApiPanel.MostrarPanel(panelCabecera);
        ApiPanel.MostrarPanel(panelDatos);
        ApiPanel.MostrarPanel(filtro);
        ApiPanel.MostrarPanel(grid);
        ApiPanel.MostrarPanel(panelPie);
        crud.classList.remove(ltrCss.crud.PanelHistorial.historial);
        ApiPanel.OcultarPanel(panelCreacion);
        ApiPanel.OcultarPanel(panelEdicion);
        ApiPanel.OcultarPanel(panelHistorial);
    }

    export function CambiarPanelActivoDelCrud(modoDeTrabajo: string) {
        let panelCabecera = (document.getElementsByClassName(ltrCss.crud.cabecera)[0]) as HTMLDivElement;
        let panelDatos = (document.getElementsByClassName(ltrCss.crud.datos)[0]) as HTMLDivElement;
        let filtro = (document.getElementsByClassName(ltrCss.crud.grid.filto)[0]) as HTMLDivElement;
        let grid = (document.getElementsByClassName(ltrCss.crud.grid.grid)[0]) as HTMLDivElement;
        let panelPie = (document.getElementsByClassName(ltrCss.crud.pie)[0]) as HTMLDivElement;
        let panelCreacion = (document.getElementsByClassName(ltrCss.crud.creacion)[0]) as HTMLDivElement;
        let panelEdicion = (document.getElementsByClassName(ltrCss.crud.edicion)[0]) as HTMLDivElement;
        let panelHistorial = (document.getElementsByClassName(ltrCss.crud.PanelHistorial.historial)[0]) as HTMLDivElement;
        let crud = (document.getElementsByClassName(ltrCss.crud.cuerpo)[0]) as HTMLDivElement;

        crud.classList.remove(ltrCss.crud.creando);
        crud.classList.remove(ltrCss.crud.editando);
        if (Definido(panelHistorial)) crud.classList.remove(ltrCss.crud.mostrandoHistorial);

        if (modoDeTrabajo === enumModoTrabajo.mantenimiento) {
            crud.classList.remove(ltrCss.crud.editarTrasCrear);
            ApiPanel.MostrarPanel(panelCabecera);
            ApiPanel.MostrarPanel(panelDatos);
            ApiPanel.MostrarPanel(filtro);
            ApiPanel.MostrarPanel(grid);
            ApiPanel.MostrarPanel(panelPie);
            if (Definido(panelCreacion)) ApiPanel.OcultarPanel(panelCreacion);
            if (Definido(panelEdicion)) ApiPanel.OcultarPanel(panelEdicion);
            if (Definido(panelHistorial)) ApiPanel.OcultarPanel(panelHistorial);
            Crud.crudMnt.AplicarTamanosAlEncolumnado();
        }
        else {
            ApiPanel.OcultarPanel(panelCabecera);
            ApiPanel.OcultarPanel(panelDatos);
            ApiPanel.OcultarPanel(filtro);
            ApiPanel.OcultarPanel(grid);
            ApiPanel.OcultarPanel(panelPie);
            if (modoDeTrabajo === enumModoTrabajo.creando) {
                crud.classList.remove(ltrCss.crud.editarTrasCrear);
                if (Definido(panelEdicion)) ApiPanel.OcultarPanel(panelEdicion);
                if (Definido(panelHistorial)) ApiPanel.OcultarPanel(panelHistorial);
                crud.classList.add(ltrCss.crud.creando);
                ApiPanel.MostrarPanel(panelCreacion);
            }
            else if (modoDeTrabajo === enumModoTrabajo.editando) {
                if (Definido(panelCreacion)) ApiPanel.OcultarPanel(panelCreacion);
                if (Definido(panelHistorial)) ApiPanel.OcultarPanel(panelHistorial);
                crud.classList.add(ltrCss.crud.editando);
                ApiPanel.MostrarPanel(panelEdicion);
            }
            else if (modoDeTrabajo === enumModoTrabajo.historial) {
                crud.classList.remove(ltrCss.crud.editarTrasCrear);
                if (Definido(panelCreacion)) ApiPanel.OcultarPanel(panelCreacion);
                if (Definido(panelEdicion)) ApiPanel.OcultarPanel(panelEdicion);
                ApiControl.IncluirCss(crud, ltrCss.crud.mostrandoHistorial);
                ApiPanel.MostrarPanel(panelHistorial);
            }
        }
    }

    export function PosicionarCrudDeCreacionConCgYTipo(panel: HTMLDivElement): void {
        ApiPanel.PosicionarEn(panel);
    }

    export function MapearDatosDeUnExpediente(panel: HTMLDivElement, datos: any) {
        ApiDelCrud.MapearDatosSocietariosYDepartamentales(panel, datos);
        ApiDelCrud.MapearSolicitante(panel, datos);
        ApiDelCrud.PosicionarCrudDeCreacionConCgYTipo(panel);
    }

    export function MapearDatosSocietariosYDepartamentales(panel: HTMLDivElement, datos: any): void {
        let idCg: number = ObtenerPropiedad(datos, literal.idCg, 0, true);
        let cg: string = ObtenerPropiedad(datos, literal.Cg, "", true);
        let cgLista: HTMLInputElement = ApiControl.BuscarListaDinamicaPorPropiedad(panel, literal.Cg) as HTMLInputElement;
        ApiListaDinamica.AsignarValor(cgLista, idCg, cg);
        MapearDatosSocietarios(panel, datos);
    }

    export function Neg_Tras_Blanquear_CG(idLista: string): void {
        if (Crud.ModoTrabajo === enumModoTrabajo.creando) {
            Crud.crudMnt.crudDeCreacion.TrasBlanquearCg();
        }
    }
    export function Neg_Tras_Seleccionar_CG(idLista: string): void {


        if (Crud.ModoTrabajo === enumModoTrabajo.creando) {
            Crud.crudMnt.crudDeCreacion.TrasBlanquearCg();
            Crud.crudMnt.crudDeCreacion.TrasSeleccionarCg(idLista);
        }
    }
    export function MapearIdSociedad(panel: HTMLDivElement, cg: any): void {
        let idSociedad: number = ObtenerPropiedad(cg, ltrPropiedades.Elemento.ConCg.IdSociedad, 0, true);
        let sociedad = ApiControl.BuscarEditor(panel, ltrPropiedades.Elemento.ConCg.IdSociedadDelCg) as HTMLInputElement;
        if (Definido(sociedad))
            sociedad.value = idSociedad.toString();
    }

    export function MapearDatosSocietarios(panel: HTMLDivElement, datos: any): void {
        let idSociedad: number = ObtenerPropiedad(datos, ltrPropiedades.Elemento.ConCg.IdSociedadDelCg, 0, true);
        let sociedad = ApiControl.BuscarEditor(panel, ltrPropiedades.Elemento.ConCg.IdSociedadDelCg) as HTMLInputElement;
        sociedad.value = idSociedad.toString();
    }

    export function MapearSolicitante(panel: HTMLDivElement, datos: any): void {
        let idSolicitante: number = ObtenerPropiedad(datos, literal.IdSolicitante, 0, true);
        let solicitante: string = ObtenerPropiedad(datos, literal.Solicitante, "", true);
        let solicitanteLista: HTMLInputElement = ApiControl.BuscarListaDinamicaPorPropiedad(panel, literal.Solicitante) as HTMLInputElement;
        ApiListaDinamica.AsignarValor(solicitanteLista, idSolicitante, solicitante);


    }

    export function MapearNombreDescripcion(panel: HTMLDivElement, datos: any): void {
        let nombre: HTMLInputElement = ApiControl.BuscarEditor(panel, ltrPropiedades.Elemento.Nombre) as HTMLInputElement;
        let descripcion: HTMLTextAreaElement = ApiControl.BuscarAreaDeTexto(panel, ltrPropiedades.Elemento.Descripcion) as HTMLTextAreaElement;
        nombre.value = ObtenerPropiedad(datos, ltrPropiedades.Elemento.Nombre);
        descripcion.value = ObtenerPropiedad(datos, ltrPropiedades.Elemento.Descripcion);
    }

    export function BloquearReferenciaModalDeCreacion(idDiv: string) {
        let div: HTMLDivElement = document.getElementById(idDiv) as HTMLDivElement;
        let refParaAbrirModal: HTMLInputElement = document.getElementById(div.id + `-mcr-${enumPostfijoControl.Referencia}`) as HTMLInputElement;
        ApiControl.BloquearReferenciaPost(refParaAbrirModal);
    }

    export function DesbloquearReferenciaModalDeCreacion(idDiv: string) {
        let div: HTMLDivElement = document.getElementById(idDiv) as HTMLDivElement;
        let refParaAbrirModal: HTMLInputElement = document.getElementById(div.id + `-mcr-${enumPostfijoControl.Referencia}`) as HTMLInputElement;
        ApiControl.DesbloquearReferenciaPost(refParaAbrirModal);
    }

    export function BloquearDesbloquearReferenciaPostDeCreacion(etapas: Array<string>, tipo, etapa, expansor: string) {
        if (EstaElEnumerado(etapas, tipo, etapa))
            ApiControl.DesbloquearReferenciaPostDeCreacion(expansor);
        else
            ApiControl.BloquearReferenciaPostDeCreacion(expansor);
    }

    export function EsElGridDeUnMnt(tabla: HTMLDivElement): boolean {
        //Vemos si el el grid de mantenimiento, los otros grid son los de los extensores
        return tabla.classList.contains(ltrCss.crud.grid.tabla);
    }

    export function BloquearAmpliacion(ampliacion: HTMLDivElement) {
        ModoAcceso.AplicarloAlPanel(ampliacion, ModoAcceso.enumModoDeAccesoDeDatos.Consultor, true);
    }

    export function AplicarModoDeAcceso(ampliacion: HTMLDivElement, modo: ModoAcceso.enumModoDeAccesoDeDatos, baja: boolean = false) {
        ModoAcceso.AplicarloAlPanel(ampliacion, modo, baja);
    }

    export function AnalizarSiExpandirContraer(panel: HTMLDivElement, estadosDelLosExpansores: Array<Crud.EstadoDeEspan>) {
        let expansor: HTMLDivElement = document.getElementById(panel.id.replace('mostrar.', '')) as HTMLDivElement;
        for (let i = 1; i < estadosDelLosExpansores.length; i++) {
            var idDelExpansor = ObtenerPropiedad(estadosDelLosExpansores[i], 'IdDelEspan');
            if (expansor.id === idDelExpansor.replace('-cuerpo', '')) {
                ApiDelCrud.ExpandirContraer(expansor, estadosDelLosExpansores[i]);
                return;
            }
        }
    }

    export function OcultarMostrarExpansor(idHtmlDetalle: string): void {
        let detalle: HTMLInputElement = document.getElementById(`expandir.${idHtmlDetalle}.input`) as HTMLInputElement;
        if (!Definido(detalle)) return;

        if (EsMayorDeCero(detalle.value)) {
            ApiDelCrud.ContraerExpansor(idHtmlDetalle);
        }
        else {
            ApiDelCrud.ExpandirExpansor(idHtmlDetalle);
        }
    }

    export function ExpandirContraer(expansor: HTMLDivElement, estadoDelExpansor: Crud.EstadoDeEspan) {
        let debeEstarAbierto: boolean = ObtenerPropiedad(estadoDelExpansor, 'abierto');
        if (debeEstarAbierto && ApiDelCrud.EstaElExpansorContraido(expansor.id))
            ApiDelCrud.ExpandirExpansor(expansor.id);
        else if (!debeEstarAbierto && ApiDelCrud.EstaElExpansorExpandido(expansor.id))
            ApiDelCrud.ContraerExpansor(expansor.id);
    }

    export function EstaElExpansorExpandido(idHtmlExpansor): boolean {
        let expansor: HTMLInputElement = document.getElementById(`expandir.${idHtmlExpansor}.input`) as HTMLInputElement;
        if (!Definido(expansor))
            return true;
        return EsMayorDeCero(expansor.value);
    }

    export function EstaElExpansorContraido(idHtmlExpansor): boolean {
        let expansor: HTMLInputElement = document.getElementById(`expandir.${idHtmlExpansor}.input`) as HTMLInputElement;
        return Numero(expansor.value) === 0;
    }
    export function ContraerExpansor(idHtmlExpansor: string) {
        let expansor: HTMLInputElement = document.getElementById(`expandir.${idHtmlExpansor}.input`) as HTMLInputElement;
        let imagen: HTMLInputElement = document.getElementById(`imagen.${idHtmlExpansor}`) as HTMLInputElement;
        expansor.value = "0";
        if (Definido(imagen)) {
            imagen.classList.remove(ltrCss.Detalle.CerrarDetalle);
            imagen.classList.add(ltrCss.Detalle.AbrirDetalle);
            ApiPanel.OcultarPanel(document.getElementById(`${idHtmlExpansor}`) as HTMLDivElement);
        }
        else {
            ApiPanel.OcultarPanel(document.getElementById(`${idHtmlExpansor}-cuerpo`) as HTMLDivElement);
            let pie: HTMLDivElement = (document.getElementById(`${idHtmlExpansor}-pie`) as HTMLDivElement);
            if (Definido(pie)) ApiPanel.OcultarPanel(pie);
        }
    }
    export function ExpandirExpansor(idHtmlExpansor: string) {
        let expansor: HTMLInputElement = document.getElementById(`expandir.${idHtmlExpansor}.input`) as HTMLInputElement;
        let imagen: HTMLInputElement = document.getElementById(`imagen.${idHtmlExpansor}`) as HTMLInputElement;
        expansor.value = "1";
        if (Definido(imagen)) {
            imagen.classList.remove(ltrCss.Detalle.AbrirDetalle);
            imagen.classList.add(ltrCss.Detalle.CerrarDetalle);
            ApiPanel.MostrarPanel(document.getElementById(`${idHtmlExpansor}`) as HTMLDivElement);
        }
        else {
            ApiPanel.MostrarPanel(document.getElementById(`${idHtmlExpansor}-cuerpo`) as HTMLDivElement);
            let pie: HTMLDivElement = (document.getElementById(`${idHtmlExpansor}-pie`) as HTMLDivElement);
            if (Definido(pie)) ApiPanel.MostrarPanel(pie);
        }
    }

    export function MapearDatosDeCreacionSalvables(panel: HTMLDivElement): any {
        let datosParaGuardar = {};
        var cg = ApiControl.BuscarListaDinamicaPorPropiedad(panel, ltrPropiedades.Elemento.ConCg.Cg);
        if (Definido(cg)) datosParaGuardar[ltrPropiedades.Elemento.ConCg.IdCg] = Numero(cg.getAttribute(atListasDinamicas.idSeleccionado));

        var tipo = ApiControl.BuscarListaDinamicaPorPropiedad(panel, ltrPropiedades.Elemento.ConTipo.Tipo);
        if (Definido(tipo)) datosParaGuardar[ltrPropiedades.Elemento.ConTipo.IdTipo] = Numero(tipo.getAttribute(atListasDinamicas.idSeleccionado));

        var nombre = ApiControl.BuscarEditor(panel, ltrPropiedades.Elemento.Nombre);
        if (Definido(nombre)) datosParaGuardar[ltrPropiedades.Elemento.Nombre] = nombre.value;

        var descripcion = ApiControl.BuscarAreaDeTexto(panel, ltrPropiedades.Elemento.Descripcion);
        if (Definido(descripcion)) datosParaGuardar[ltrPropiedades.Elemento.Descripcion] = descripcion.value;
        ApiPanel.MapearEditoresAlmacenables(panel, datosParaGuardar);
        ApiPanel.MapearSelectoresAlmacenables(panel, datosParaGuardar);
        return datosParaGuardar;
    }

    export function EstadosDeLosExpansores(panel: HTMLDivElement): Array<Crud.EstadoDeEspan> {
        let datosParaGuardar = new Array<Crud.EstadoDeEspan>();
        let idPanelDto = panel.id + '-dp';
        datosParaGuardar.push(new Crud.EstadoDeEspan(idPanelDto, !(document.getElementById(idPanelDto) as HTMLDivElement).classList.contains(ltrCss.divNoVisible)));

        let ampliaciones: NodeListOf<HTMLDivElement> = panel.querySelectorAll(`.${ltrCss.Ampliacion.Contenedor}`) as NodeListOf<HTMLDivElement>;
        for (var i = 0; i < ampliaciones.length; i++) {
            let ampliacion: HTMLDivElement = document.getElementById(ampliaciones[i].id.replace('mostrar.', '')) as HTMLDivElement;
            datosParaGuardar.push(new Crud.EstadoDeEspan(ampliacion.id, !ampliacion.classList.contains(ltrCss.divNoVisible)));
        }

        let detalles: NodeListOf<HTMLDivElement> = panel.querySelectorAll(`.${ltrCss.Detalle.Contenedor}`) as NodeListOf<HTMLDivElement>;
        for (var i = 0; i < detalles.length; i++) {
            let detalle: HTMLDivElement = document.getElementById(detalles[i].id.replace('mostrar.', '')) as HTMLDivElement;
            datosParaGuardar.push(new Crud.EstadoDeEspan(detalle.id, !detalle.classList.contains(ltrCss.divNoVisible)));
        }

        let bloques: NodeListOf<HTMLDivElement> = panel.querySelectorAll(`.${ltrCss.Bloque.Contenedor}`) as NodeListOf<HTMLDivElement>;
        for (var i = 0; i < bloques.length; i++) {
            let bloque: HTMLDivElement = document.getElementById(bloques[i].id + '-cuerpo') as HTMLDivElement;
            datosParaGuardar.push(new Crud.EstadoDeEspan(bloque.id, !bloque.classList.contains(ltrCss.divNoVisible)));
        }
        return datosParaGuardar;
    }

    export function AbrirModalDeRelacionParaCrear(idModalDeCreacion: string, propiedadRestrictora: string, idRestrictor: number, texto: string, idNegocio: number, nombreNegocio: string): void {
        let modal: HTMLDivElement = document.getElementById(idModalDeCreacion) as HTMLDivElement;
        if (!Definido(modal)) MensajesSe.EmitirMensajeDeExcepcion('AbrirModalDeRelacionParaCrear', `la modal para relacionar '${idModalDeCreacion}' no está definida`);
        ApiPanel.BlanquearControlesDeIU(modal, false);
        MapearAlPanel.RestrictoresPorPropiedad(modal, propiedadRestrictora, idRestrictor, texto);

        if (propiedadRestrictora !== ltrPropiedades.Negocio.idNegocio) {
            let inpuIdNegocio: HTMLInputElement = modal.querySelector(`[${atControl.propiedad}=${ltrPropiedades.Negocio.idNegocio}]`) as HTMLInputElement;
            if (Definido(inpuIdNegocio)) {
                inpuIdNegocio.setAttribute(atControl.restrictor, idNegocio.toString());
                inpuIdNegocio.value = nombreNegocio;
            }
        }
        ApiDeInicializacion.InicializarListasDeElementos(modal, true);
        //Todo:
        /*
         * Leer modo de acceso a la relación (he de pasar los dos negocios a relacionar, deben ser propiedades del link)
         * Si hay modo gestión se abre, si no se muestra mensaje
         */

        ApiPanel.AbrirModal(modal);
    }

    export function Neg_Tras_Seleccionar_Tipo(idLista: any): void {
        const tipoctrl = document.getElementById(idLista) as HTMLInputElement;
        const contenedor = ApiPanel.BuscarContenedorDeTablaDto(tipoctrl);
        const tipo = OpcionesDeLasListas.ObtenerObjeto(tipoctrl);
        const usaClase = ObtenerPropiedad(tipo, ltrPropiedades.TipoDeElemento.HayClases, null);

        if (tipo === undefined || usaClase === null) {
            var negocio = tipoctrl.getAttribute(atControl.negocio);
            var idTipo = Numero(tipoctrl.getAttribute(atListasDinamicas.idSeleccionado));
            if (idTipo === 0) return;

            let parametros: Array<Parametro> = new Array<Parametro>();
            if (Definido(Crud.Consultor)) {
                parametros.push(new Parametro(Ajax.Parametros.ConsultarConGuid, Crud.Consultor.PaginaDeConsultaConGuid));
                parametros.push(new Parametro(Ajax.Param.guid, Crud.Consultor.GuidDeConsulta));
                parametros.push(new Parametro(Ajax.Param.id, Crud.Consultor.RegistroId));
            }
            ApiDePeticiones.LeerTipo(ApiDelCrud, negocio, idTipo, parametros)
                .then((peticion) => {
                    var tipocrtl = ApiControl.BuscarListaDinamicaPorPropiedad(contenedor, ltrPropiedades.Elemento.ConTipo.Tipo);
                    OpcionesDeLasListas.AgregarOpcion(tipocrtl.id, peticion.resultado.datos);
                    AplicarTipo(contenedor, peticion.resultado.datos);
                })
                .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
        }
        else AplicarTipo(contenedor, tipo);
    }

    export function Neg_Tras_Blanquear_Tipo(idLista: any): void {
        var tipoctrl = document.getElementById(idLista) as HTMLInputElement;
        Neg_Tras_Blanquear_CtrlTipo(tipoctrl);
    }

    export function Neg_Tras_Blanquear_CtrlTipo(tipoctrl: HTMLInputElement): void {
        var contenedor = ApiPanel.BuscarContenedorDeTablaDto(tipoctrl);
        var nombrectrl = ApiControl.BuscarEditor(contenedor, ltrPropiedades.Elemento.Nombre);
        if (!Definido(nombrectrl))
            return;
        nombrectrl.removeAttribute(atControl.mascara);
        nombrectrl.setAttribute(atControl.placeholder, 'Indique el tipo de proceso');

        var listaDeClases = ApiControl.BuscarListaDeElementos(contenedor, ltrPropiedades.Elemento.ClaseDeElemento);
        if (Definido(listaDeClases)) OcultarClase(listaDeClases);
    }

    export function Neg_ProcesarOpcionDeMenuLista(): void {
        var lista = Crud.crudMnt.MenuLista;
        if (Definido(lista)) {
            if (lista.selectedIndex == 0) return;
            Crud.crudMnt.IraCrear()
            Crud.crudMnt.crudDeCreacion.ProcesarOpcionMf(Crud.crudMnt.IdNegocio, `${ltrPropiedades.Negocio.PlantillaDeCreacion.Plantilla}_${lista.value}`, true);
            lista.selectedIndex = 0;
        }
    }

    function AplicarTipo(contenedor: HTMLDivElement, tipo: any) {
        var marcador = ObtenerPropiedad(tipo, ltrPropiedades.TipoDeElemento.Marcador);
        if (Definido(marcador)) {
            var nombrectrl = ApiControl.BuscarEditor(contenedor, ltrPropiedades.Elemento.Nombre);
            nombrectrl.setAttribute(atControl.placeholder, marcador);
            var mascara = ObtenerPropiedad(tipo, ltrPropiedades.TipoDeElemento.Mascara);
            if (Definido(mascara)) {
                nombrectrl.setAttribute(atControl.mascara, mascara);
                var marcador = ObtenerPropiedad(tipo, ltrPropiedades.TipoDeElemento.Marcador);
                if (!IsNullOrEmpty(marcador)) nombrectrl.placeholder = marcador;
            }
        }

        RenderClasePorTipo(contenedor, tipo);
    }

    export function RenderClasePorTipo(contenedor: HTMLDivElement, tipo: any) {
        var hayClases = ObtenerPropiedad(tipo, ltrPropiedades.TipoDeElemento.HayClases);
        var listaDeClases = ApiControl.BuscarListaDeElementos(contenedor, ltrPropiedades.Elemento.ClaseDeElemento);
        if (EsTrue(hayClases)) {
            MostrarClase(listaDeClases);
        }
        else {
            OcultarClase(listaDeClases);
        }
    }

    export function OcultarClaseDeElemento(contenedor: HTMLDivElement): void {
        var listaDeClases = ApiControl.BuscarListaDeElementos(contenedor, ltrPropiedades.Elemento.ClaseDeElemento);
        if (Definido(listaDeClases))
            OcultarClase(listaDeClases)
    }

    export function QuitarResaltos(contenedor: HTMLDivElement, resaltos: string | null = null): void {
        if (Definido(resaltos))
            QuitarResaltosInterna(contenedor, resaltos);
        else {
            QuitarResaltosInterna(contenedor, ltrCss.Resalto.Verde);
            QuitarResaltosInterna(contenedor, ltrCss.Resalto.Violeta);
        }
    }

    function QuitarResaltosInterna(contenedor: HTMLDivElement, resalto: string): void {
        const controlesConResalto: NodeListOf<HTMLElement> = contenedor.querySelectorAll(`.${resalto}`);
        controlesConResalto.forEach((control: HTMLElement) => {
            ApiControl.ExcluirCss(control, resalto);
        });
    }

    function MostrarClase(listaDeClases: HTMLSelectElement) {
        var divDelTipoClase = ApiPanel.BuscarContenedorDeMasDeUnControl(listaDeClases);
        ApiControl.ExcluirCss(divDelTipoClase, ltrCss.mostrarSoloElPrimerControl);
        ApiControl.MostrarControlSi(listaDeClases, true);
        if (!EsTrue(listaDeClases.getAttribute(atListasDeElemento.Cargando))) {
            ApiControl.BlanquearListaDeElementos(listaDeClases);

            //let idClase: number = 0;
            //if (Definido(Crud.Consultor))
            //    idClase = ObtenerPropiedad(Crud.Consultor.Registro, ltrPropiedades.Elemento.IdClaseDeElemento, 0)
            //else
            //    if (Crud.crudMnt.EstoyEditandoConsultando)
            //        idClase = ObtenerPropiedad(Crud.crudMnt.crudDeEdicion.Registro, ltrPropiedades.Elemento.IdClaseDeElemento, 0)

            // Determinamos el origen de los datos
            const registroOrigen = Definido(Crud.Consultor)
                ? Crud.Consultor.Registro
                : (Crud.crudMnt.EstoyEditandoConsultando ? Crud.crudMnt.crudDeEdicion.Registro : null);

            // Llamamos a la función una sola vez
            const idClase: number = registroOrigen
                ? ObtenerPropiedad(registroOrigen, ltrPropiedades.Elemento.IdClaseDeElemento, 0)
                : 0;

            MapearAlControl.ListaDeElementos(listaDeClases, new Array<ClausulaDeFiltrado>(), idClase, null);
        }
        return divDelTipoClase;
    }

    function OcultarClase(listaDeClases: HTMLSelectElement) {
        var divDelTipoClase = ApiPanel.BuscarContenedorDeMasDeUnControl(listaDeClases);
        ApiControl.MostrarControlSi(listaDeClases, false);
        var divDelTipoClase = ApiPanel.BuscarContenedorDeMasDeUnControl(listaDeClases);
        ApiControl.IncluirCss(divDelTipoClase, ltrCss.mostrarSoloElPrimerControl);
        ApiControl.BlanquearListaDeElementos(listaDeClases);
    }

    export function ModalParaPedirDatos_Abrir(idModal: string, idElemento: number): void {
        let modal: HTMLDivElement = document.getElementById(idModal) as HTMLDivElement;
        if (!Definido(modal))
            MensajesSe.EmitirMensajeDeExcepcion('ModalParaPedirDatos_Abrir', `la modal de pedir datos '${idModal}' no está definida`);
        ApiPanel.BlanquearControlesDeIU(modal);
        modal.setAttribute(atModal.idElemento1, idElemento === 0 ? this.ElementoEditado.Id.toString() : idElemento.toString());
        ApiPanel.AbrirModal(modal);
    }

    export function PanelActivo(): HTMLDivElement {
        let panel: HTMLDivElement = ApiDelCrud.ModalAbierta()
        if (!Definido(panel)) {
            panel = Crud.crudMnt.PanelDto;
        }
        return panel;
    }

    export function ModalAbierta(): HTMLDivElement {
        return document.querySelector(`.${ltrCss.contenedorModal}[style*="display: block;"]`);
    }

    export function Expotacion_AlCambiar_Plantilla(lista: HTMLSelectElement) {
        var check = ApiControl.BuscarCheck(ModalAbierta(), ltrPropiedades.Dialogos.Exportacion.LasMostaradas);
        if (lista.selectedIndex > 0) {
            if (!check.parentElement.classList.contains(ltrCss.divNoVisible))
                check.parentElement.classList.add(ltrCss.divNoVisible);
        }
        else {
            check.parentElement.classList.remove(ltrCss.divNoVisible);
        }
    }

    export async function Totales(controlador: string, posicion: number, cantidad: number, parametros: Array<Parametro>): Promise<any> {
        var totales = undefined;
        try {
            const peticion = await ApiDePeticiones.Totales(this, controlador, 0, -1, parametros);
            totales = peticion.resultado.datos;
        }
        catch (peticion) {
            ApiDePeticiones.EmitirError(peticion);
        }
        return totales;
    }


    var _cambiandoAncho = false;
    var _posicioInicial: number;
    var _anchoInicial: number;
    let _lastExecution = 0;
    var _contenedorDeDatos: HTMLDivElement
    var _contenedorDelVisor: HTMLDivElement

    export function CalcularTamanoDelVisor(): void {
        var crud = Crud.crudMnt;
        var visor = crud.EstoyCreando ? crud.crudDeCreacion.DivVisor : crud.crudDeEdicion.ContenedorDelVisorDeArchivoConHistorial;
        if (!Definido(visor))
            return;

        var contenedorCabecera = crud.EstoyCreando ? crud.crudDeCreacion.ContenedorDeCabecera : crud.crudDeEdicion.ContenedorDeCabecera;
        var contenedorDelVisor = crud.EstoyCreando ? crud.crudDeCreacion.ContenedorDelVisor : crud.crudDeEdicion.ContenedorDelVisorDeArchivoConHistorial;
        var contenedorDeDatos = crud.EstoyCreando ? crud.crudDeCreacion.ContenedorDeDatos : crud.crudDeEdicion.ContenedorDeDatos;
        var contenedorDeDatosMasVisor = crud.EstoyCreando ? crud.crudDeCreacion.ContenedorDeDatosMasVisor : crud.crudDeEdicion.ContenedorDeDatosMasVisor;

        const anchoVisor = contenedorDelVisor.clientWidth;
        ApiDelCrud.AjustarAnchoPanelDelVisor(crud.EstoyCreando, contenedorCabecera, contenedorDeDatosMasVisor, contenedorDeDatos, contenedorDelVisor, anchoVisor);
    }

    export function AjustarAnchoPanelDelVisor(estoyCreando: boolean, ContenedorDeCabecera: HTMLDivElement, ContenedorDeDatosMasVisor: HTMLDivElement, ContenedorDeDatos: HTMLDivElement, ContenedorDelVisor: HTMLDivElement, anchoVisor: number): void {
        if (!Definido(ContenedorDeCabecera))
            return;
        // Obtener el ancho del viewport
        const anchoVentana = Math.max(document.documentElement.clientWidth || 0, window.innerWidth || 0);
        const padding = estoyCreando ? 11 : 2;
        ContenedorDeCabecera.style.width = `${anchoVentana - padding}px`;
        const anchoCabecera = anchoVentana - padding;

        // Asegurar que el ancho del visor no sea menor que el mínimo permitido
        const anchoMinimoVisor = 200; // Ajusta este valor según tus necesidades
        anchoVisor = Math.max(anchoVisor, anchoMinimoVisor);

        // Calcular el ancho máximo disponible para el visor
        const anchoMaximoVisor = anchoCabecera - 200; // 200px es el ancho mínimo para datos

        // Limitar el ancho del visor al máximo disponible
        anchoVisor = Math.min(anchoVisor, anchoMaximoVisor);

        var crud = Crud.crudMnt;
        var splitter = crud.EstoyCreando ? crud.crudDeCreacion.Splitter : crud.crudDeEdicion.Splitter;
        const anchoSplitter = splitter.clientWidth;
        ContenedorDelVisor.style.width = `${anchoVisor - anchoSplitter - 5}px`;
        //if (!estoyCreando) ContenedorDelVisor.parentElement.style.width = `${anchoVisor - anchoSplitter - 5}px`;

        // Calcular el nuevo ancho de datos
        const anchoDatos = anchoCabecera - anchoVisor;

        // Aplicar los nuevos anchos
        ContenedorDeDatos.style.width = `${anchoDatos}px`;
        ContenedorDeDatosMasVisor.style.width = `${anchoCabecera}px`;
    }

    export async function RenderizarUrlsEnVisor(crud: Crud.CrudMnt, idArchivo: number, nombre: string, ajustarVisor: boolean) {

        var visor = crud.EstoyCreando ? crud.crudDeCreacion.DivVisor : crud.crudDeEdicion.DivVisor;
        if (!Definido(visor))
            return;

        var contenedorDelVisor = crud.EstoyCreando ? crud.crudDeCreacion.ContenedorDelVisor : crud.crudDeEdicion.ContenedorDelVisor;
        var contenedorDeDatos = crud.EstoyCreando ? crud.crudDeCreacion.ContenedorDeDatos : crud.crudDeEdicion.ContenedorDeDatos;
        var contenedorCabecera = crud.EstoyCreando ? crud.crudDeCreacion.ContenedorDeCabecera : crud.crudDeEdicion.ContenedorDeCabecera;
        var contenedorDeDatosMasVisor = crud.EstoyCreando ? crud.crudDeCreacion.ContenedorDeDatosMasVisor : crud.crudDeEdicion.ContenedorDeDatosMasVisor;

        let input = contenedorDelVisor.getElementsByClassName(ltrCss.crud.panelDeEdicion.VisorDeNombreAnexados) as HTMLCollectionOf<HTMLInputElement>;


        visor.innerHTML = 'Cargando...';
        let url: string = undefined;
        if (crud.EstoyCreando) {
            let parametros = `idArchivo=${idArchivo}`;
            url = `/${Ajax.Archivos.controlador}/${Ajax.Archivos.accion.DescargarParaCrear}?${parametros}`;
        }
        else {
            let parametros = `negocio=${crud.NombreDeNegocio}`;
            parametros = `${parametros}&idElemento=${crud.EstoyCreando ? 0 : crud.crudDeEdicion.ElementoEditado.Id}`;
            parametros = `${parametros}&idArchivo=${idArchivo}`;
            parametros = `${parametros}&auditar=false`;
            url = `/${Ajax.Archivos.controlador}/${Ajax.Archivos.accion.Descargar}?${parametros}`;
        }

        try {
            const response = await fetch(url);
            const blob = await response.blob();
            if (!crud.EstoyCreando)
                ApiControl.ExcluirCss(crud.crudDeEdicion.BotonVisor, ltrCss.crud.panelDeEdicion.Acciones.SinVisor);
            const objectUrl = URL.createObjectURL(blob);
            if (blob.type.startsWith('image/')) {
                ApiPanel.RenderizarContenidoImagen(visor, `<img src="${objectUrl}" alt="Archivo descargado" style="max-width: 100%; height: auto;">`);
            }
            else if (blob.type === 'application/pdf') {
                //visor.innerHTML = `<iframe src="${objectUrl}" style="width: 100%; height: 100%; border: none;"></iframe>`;
                ApiPanel.RenderizarContenidoPdf(visor, objectUrl);
            }
            else if (blob.type === 'application/xml' || blob.type === 'text/xml') {
                ApiPanel.RenderizarXml(visor, objectUrl);
            }
            else if (blob.type === 'text/csv') {
                ApiPanel.RenderizarToHtml(visor, idArchivo, Ajax.Archivos.accion.DescargarCsvToHtml);
            }
            else if (blob.type === 'application/rtf') {
                ApiPanel.RenderizarToHtml(visor, idArchivo, Ajax.Archivos.accion.DescargarRtfToHtml);
            }
            else if (blob.type === 'application/vnd.openxmlformats-officedocument.wordprocessingml.document') {
                ApiPanel.RenderizarToHtml(visor, idArchivo, Ajax.Archivos.accion.DescargarDocxToHtml);
            }
            else if (blob.type === 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' || blob.type === 'application/vnd.ms-excel') {
                ApiPanel.RenderizarToHtml(visor, idArchivo, Ajax.Archivos.accion.DescargarXlsxToHtml);
            }
            else if (blob.type === 'application/x-zip-compressed' || blob.type === 'application/x-7z-compressed') {
                ApiPanel.RenderizarToHtml(visor, idArchivo, Ajax.Archivos.accion.DescargarZipToHtml);
            }
            else
                if (blob.type === 'text/html') {
                    ApiPanel.RenderizarToHtml(visor, idArchivo, Ajax.Archivos.accion.DescargarHtmlSanitizado);
                }
                else if (blob.type === 'text/plain' || blob.type === 'application/json' || blob.type === 'application/text' || blob.type === 'application/octet-stream') {
                    const text = await blob.text();
                    ApiPanel.RenderizarContenido(visor, text, (blob.type === 'text/plain' || blob.type === 'application/text' || blob.type === 'application/octet-stream') && !(text.indexOf('</html>') > 0)
                        ? 'texto'
                        : blob.type === 'application/json'
                            ? 'json'
                            : 'html');
                }
                else {
                    if (crud.EstoyCreando) {
                        ApiControl.IncluirCss(crud.crudDeCreacion.ContenedorDeDatosMasVisor, ltrCss.crud.panelCreacion.VisorOculto);
                        return;
                    } else {
                        const linkElement = document.createElement('a');
                        linkElement.href = objectUrl;
                        linkElement.textContent = `Descargar archivo`;
                        linkElement.download = nombre;
                        visor.innerHTML = '';
                        visor.appendChild(linkElement);
                    }
                }
            if (crud.EstoyCreando)
                crud.crudDeCreacion.AsignarIdArchivo(idArchivo, ajustarVisor);
            else
                crud.crudDeEdicion.AsignarIdArchivo(idArchivo, ajustarVisor);

            input[0].value = nombre;
            if (ajustarVisor) {
                const contenedor = crud.EstoyCreando ? contenedorDelVisor : contenedorDelVisor.parentElement as HTMLDivElement;
                ApiDelCrud.AjustarAnchoPanelDelVisor(crud.EstoyCreando, contenedorCabecera, contenedorDeDatosMasVisor, contenedorDeDatos, contenedor, crud.TamanoDelVisor);
            }
        } catch (error) {
            visor.innerHTML = 'Error al cargar el archivo';
        }
    }

    export async function ProcesarRenderizar(crud: Crud.CrudMnt, idArchivo: number, accion: string): Promise<boolean> {
        const { visor, contenedorDelVisor } = obtenerElementosVisuales(crud);
        if (!visor) return false;

        const input = contenedorDelVisor.getElementsByClassName(ltrCss.crud.panelDeEdicion.VisorDeNombreAnexados)[0] as HTMLInputElement;


        if (await mapearDatosSiEsUnaFacturaJson(crud, idArchivo, accion))
            return true;

        actualizarMensajeVisor(visor, accion);

        const url = construirUrl(crud, idArchivo, accion);

        try {
            const resultado = await obtenerResultado(url);

            if (resultado.estado === 'Ok') {
                const resultadoProcesado = await procesarResultadoExitoso(crud, visor, resultado, accion);
                asignarIdArchivo(crud, resultadoProcesado.idArchivo === 0 ? idArchivo : resultadoProcesado.idArchivo);
                input.value = resultadoProcesado.nombreArchivo;
                if (accion === ltrEventos.Edicion.FacturasRec.Analizar)
                    await mapearDatosSiEsUnaFacturaJson(crud, resultadoProcesado.idArchivo, accion);
                return true;
            }
            else {
                manejarError(crud, contenedorDelVisor, idArchivo, resultado);
                return false;
            }
        } catch (error) {
            visor.innerHTML = `Error al '${accion}' del archivo`;
            MensajesSe.Error('ProcesarRenderizar', 'Error al analizar la factura, acceda a la consola', error);
            return false;
        }
    }

    function obtenerElementosVisuales(crud: Crud.CrudMnt): { visor: HTMLDivElement, contenedorDelVisor } {
        const visor = crud.EstoyCreando ? crud.crudDeCreacion.DivVisor : crud.crudDeEdicion.DivVisor;
        const contenedorDelVisor = crud.EstoyCreando ? crud.crudDeCreacion.ContenedorDelVisor : crud.crudDeEdicion.ContenedorDelVisor;
        return { visor, contenedorDelVisor };
    }

    function actualizarMensajeVisor(visor: HTMLElement, accion: string) {
        visor.innerHTML = accion === ltrEventos.Edicion.PasarOcr
            ? 'Pasando OCR...'
            : accion === ltrEventos.Edicion.ResumirArchivo
                ? 'Resumiendo...'
                : 'Analizando factura ...';
    }

    async function mapearDatosSiEsUnaFacturaJson(crud: Crud.CrudMnt, idArchivo: number, accion: string): Promise<boolean> {
        if (accion === ltrEventos.Edicion.FacturasRec.Analizar) {
            const resultado = await ApiDeArchivos.EsFicheroJson(crud.NombreDeNegocio, crud.EstoyCreando ? 0 : crud.crudDeEdicion.ElementoEditado.Id, idArchivo);
            if (resultado.esJson) {
                crud.MapearDatosJsonDesdeElVisor(resultado.json);
                return true;
            }
        }
        return false;
    }

    function construirUrl(crud: Crud.CrudMnt, idArchivo: number, accion: string): string {
        const parametros = new URLSearchParams({
            idArchivo: idArchivo.toString(),
            accion,
            negocio: crud.EnumeradoDeNegocio as string,
            idElemento: (crud.EstoyCreando ? 0 : crud.crudDeEdicion.ElementoEditado.Id).toString()
        });
        return `/${Ajax.Archivos.controlador}/${Ajax.Archivos.accion.ProcesarAccion}?${parametros}`;
    }

    async function obtenerResultado(url: string) {
        const response = await fetch(url);
        return await response.json();
    }
    async function procesarResultadoExitoso(crud: Crud.CrudMnt, visor: HTMLDivElement, resultado: any, accion: string): Promise<{ idArchivo: number, nombreArchivo: string }> {
        if (accion === ltrEventos.Edicion.FacturasRec.Analizar) {
            return facturaAnalizadaCorrectamente(crud, visor, resultado);
        } else {
            const idArchivo = Numero(resultado.datos);
            const nombreArchivo = accion === ltrEventos.Edicion.PasarOcr ? 'Ocr' : 'Resumido';
            await ApiPanel.RenderizarToHtml(visor, idArchivo, Ajax.Archivos.accion.DescargarHtmlSanitizado);
            return { idArchivo, nombreArchivo };
        }
    }


    async function facturaAnalizadaCorrectamente(crud: Crud.CrudMnt, visor: HTMLDivElement, resultado: any): Promise<{ idArchivo: number, nombreArchivo: string }> {
        if (typeof resultado.datos === 'object' && resultado.datos !== null && ltrPropiedades.Ia.IdArchivo in resultado.datos) {
            const idArchivo = ObtenerPropiedad(resultado.datos, ltrPropiedades.Ia.IdArchivo);
            const nombre = ObtenerPropiedad(resultado.datos, ltrPropiedades.Ia.Nombre);
            await RenderizarUrlsEnVisor(crud, idArchivo, nombre, false);
            if (crud.EstoyEditando) ApiDeArchivos.MostrarArchivosAnexados(
                crud.crudDeEdicion.PanelDeArchivos.id,
                crud.NombreDeNegocio,
                crud.crudDeEdicion.ElementoEditado.Id, null
            );
            return { idArchivo: idArchivo, nombreArchivo: nombre };
        }

        ApiPanel.RenderizarContenido(visor, resultado.datos, 'json');
        return { idArchivo: 0, nombreArchivo: "Factura analizada" };
    }

    function manejarError(crud: Crud.CrudMnt, contenedorDelVisor: HTMLElement, idArchivo: number, resultado: any) {
        MensajesSe.Error("ProcesarRenderizar", resultado.mensaje, resultado.consola);
        const input = contenedorDelVisor.getElementsByClassName(ltrCss.crud.panelDeEdicion.VisorDeNombreAnexados)[0] as HTMLInputElement;
        RenderizarUrlsEnVisor(crud, idArchivo, input.value, false);
    }

    function asignarIdArchivo(crud: Crud.CrudMnt, idArchivoResumido: number) {
        if (crud.EstoyCreando) {
            crud.crudDeCreacion.AsignarIdArchivo(idArchivoResumido, false);
        } else {
            crud.crudDeEdicion.AsignarIdArchivo(idArchivoResumido, false);
        }
    }

    function actualizarValorInput(input: HTMLInputElement, accion: string) {

    }

    export function AjustarAnchoDeDatosMasVisor(): void {
        var crud = Crud.crudMnt;
        if (crud.EstoyCreando && crud.crudDeCreacion.IdArchivoMostrado > 0) {
            CalcularTamanoDelVisor();
        }
        else {
            if (crud.crudDeEdicion.IdArchivoMostrado > 0) {
                CalcularTamanoDelVisor();
            }
            crud.crudDeEdicion.ContenedorDelVisorDeArchivoConHistorial.style.maxHeight = crud.crudDeEdicion.ContenedorDeDatos.clientHeight + 'px';
        }
    }


    var _cambiandoAnchoTabla = false;
    var _posicioInicialSplitter: number;
    var _anchoInicialTabla: number;
    let _ultimaEjecucion = 0;
    var _contenedorDeTabla: HTMLDivElement
    var _contenedorDeGraficos: HTMLDivElement

    export function ConfigurarEventosDeCambioDelAnchoContenedorDeTablaConGraficos() {
        const crud = Crud.crudMnt;
        if (!Definido(crud.ContenedorDeTablaConGraficos))
            return;
        const splitter = crud.Splitter;

        _cambiandoAnchoTabla = false;
        _posicioInicialSplitter = undefined;
        _anchoInicialTabla = undefined;
        _contenedorDeTabla = undefined;
        _contenedorDeGraficos = undefined;

        splitter.addEventListener('mousedown', (e: MouseEvent) => {
            ComienzoCambioDelAnchoContenedorDeTablaConGraficos(e);
        });
    }

    function ComienzoCambioDelAnchoContenedorDeTablaConGraficos(e: MouseEvent) {

        e.preventDefault();
        e.stopPropagation();

        const crud = Crud.crudMnt;
        const contenedorDeTablaConGraficos = crud.ContenedorDeTablaConGraficos;
        const splitter = crud.Splitter;
        const contenedorDeTabla = crud.ContenedorDeTabla;
        const contenedorDeGraficos = crud.ContenedorDeGraficos;

        _cambiandoAnchoTabla = true;
        _posicioInicialSplitter = e.clientX;
        _anchoInicialTabla = contenedorDeTabla.offsetWidth;
        _contenedorDeTabla = contenedorDeTabla;
        _contenedorDeGraficos = contenedorDeGraficos;

        document.addEventListener('mousemove', CambiarDeAnchoDelContenedorDeTablaConGraficos.bind(splitter));
        document.addEventListener('mouseup', FinalizarCambioDeAnchoDelContenedorDeTablaConGraficos.bind(splitter));
        document.addEventListener('mouseleave', FinalizarCambioDeAnchoDelContenedorDeTablaConGraficos.bind(splitter));

    }

    export function CambiarDeAnchoDelContenedorDeTablaConGraficos(e: MouseEvent) {

        // Prevenir comportamiento predeterminado y propagación
        e.preventDefault();
        e.stopPropagation();

        // Verificar si el botón izquierdo está presionado (bitmask 1)
        if ((e.buttons & 1) === 0) { // 0 = botón izquierdo no presionado
            FinalizarCambioDeAnchoDelContenedorDeTablaConGraficos(e);
            return;
        }

        if (!_cambiandoAnchoTabla) {
            return;
        }

        // Implementar throttle
        if (_ultimaEjecucion && Date.now() - _ultimaEjecucion < 16) { // 60 FPS
            return;
        }
        _ultimaEjecucion = Date.now();

        const crud = Crud.crudMnt;
        const splitter = crud.Splitter;

        // El contenedor de la tabla (el padre del div de la tabla) es el que tiene el width
        // El contenedor principal de TODO es el padre de _contenedorDeTabla.
        const contenedorPrincipalRect = _contenedorDeTabla.parentElement.getBoundingClientRect();

        // Calcula el nuevo ancho de la tabla basado en la posición actual del ratón.
        // El nuevo ancho de la tabla es la distancia desde el borde izquierdo del contenedor principal hasta el ratón.
        const nuevoAnchoTabla = e.clientX - contenedorPrincipalRect.left;

        // Calcula el espacio horizontal total disponible.
        const anchoTotal = contenedorPrincipalRect.width;

        // Ancho total disponible menos el nuevo ancho de la tabla y el ancho fijo del splitter.
        const anchoSplitter = splitter.clientWidth;
        const nuevoAnchoGraficos = anchoTotal - nuevoAnchoTabla - anchoSplitter;

        // Ensure minimum widths
        const minWidth = 100;
        if (nuevoAnchoTabla < minWidth || nuevoAnchoGraficos < minWidth) {
            return;
        }

        requestAnimationFrame(() => {
            if (!Definido(_contenedorDeTabla) || !Definido(_contenedorDeGraficos)) return;

            // Aplica el ancho directamente a los contenedores
            _contenedorDeTabla.style.width = `${nuevoAnchoTabla}px`;
            _contenedorDeGraficos.style.width = `${nuevoAnchoGraficos}px`;

            // IMPORTANTE: NO intentes mover el splitter. Su posición es establecida por Flexbox.
            // splitter.style.left = `${nuevoAnchoTabla}px`; // <-- ELIMINAR ESTA LÍNEA
        });
    }



    export function OcultarContenedorDeGraficos(): boolean {
        const crud = Crud.crudMnt;
        if (!Definido(crud.ContenedorDeTablaConGraficos))
            return false;
        const contenedorTabla = crud.ContenedorDeTabla;

        const splitter = crud.Splitter;
        const contenedorDeGraficos = crud.ContenedorDeGraficos;

        const contenedorPrincipalRect = contenedorTabla.parentElement.getBoundingClientRect();
        const nuevoAnchoTabla = contenedorPrincipalRect.width;

        contenedorTabla.style.width = `${nuevoAnchoTabla}px`;
        splitter.style.removeProperty('width');
        contenedorDeGraficos.style.removeProperty('width');
        return true;
    }

    export function MostrarContenedorDeGraficos(): boolean {
        const crud = Crud.crudMnt;
        if (!Definido(crud.ContenedorDeTablaConGraficos)) return false;

        const contenedorPrincipalRect = crud.ContenedorDeTabla.parentElement.getBoundingClientRect();

        //asigno el 60%.
        const anchoTotal = contenedorPrincipalRect.width;
        const nuevoAnchoTabla = contenedorPrincipalRect.width * 60 / 100;

        // Ancho total disponible menos el nuevo ancho de la tabla y el ancho fijo del splitter.
        const anchoSplitter = 6;//splitter.clientWidth;
        const nuevoAnchoGraficos = anchoTotal - nuevoAnchoTabla - anchoSplitter;

        // Ensure minimum widths
        const minWidth = 100;
        if (nuevoAnchoTabla < minWidth || nuevoAnchoGraficos < minWidth) {
            return;
        }
        crud.ContenedorDeTabla.style.width = `${nuevoAnchoTabla}px`;
        crud.ContenedorDeGraficos.style.width = `${nuevoAnchoGraficos}px`;

        //ApiDelCrud.ConfigurarEventosDeCambioDelAnchoContenedorDeTablaConGraficos();

        return true;
    }

    function FinalizarCambioDeAnchoDelContenedorDeTablaConGraficos(e: MouseEvent) {

        var crud = Crud.crudMnt;
        var splitter = crud.Splitter;
        if (!_cambiandoAnchoTabla || _posicioInicialSplitter === undefined)
            return;

        try {
            //GuardarTamanoDeGraficos(crud);
        }
        finally {
            document.removeEventListener('mousemove', CambiarDeAnchoDelContenedorDeTablaConGraficos.bind(splitter));
            document.removeEventListener('mouseup', FinalizarCambioDeAnchoDelContenedorDeTablaConGraficos.bind(splitter));
            document.removeEventListener('mouseleave', FinalizarCambioDeAnchoDelContenedorDeTablaConGraficos.bind(splitter));
            ResetearParametrosDeArrastre();
        }
    }


    async function GuardarTamanoDeGraficos(crud: Crud.CrudMnt) {
        const params2 = {
            [Ajax.Param.idNegocio]: Encriptar(literal.ClaveDeEncriptacion, crud.IdNegocio),
            [Ajax.Param.peticion]: Encriptar(literal.ClaveDeEncriptacion, ltrMenus.eventosDeMf.Comun.TamanoDelVisor)
        };
        const url2 = `/${crud.Controlador}/${Ajax.EndPoint.ProcesarPeticion}?${new URLSearchParams(params2)}`;
        await fetch(url2, {
            method: 'POST',
            body: TamanoDelVisor(crud),
            keepalive: true
        });
    }




    export function ConfigurarEventosDeCambioDelAnchoContenedorDeDatos() {
        var crud = Crud.crudMnt;
        var contenedorDeDatosMasVisor = crud.EstoyCreando ? crud.crudDeCreacion.ContenedorDeDatosMasVisor : crud.crudDeEdicion.ContenedorDeDatosMasVisor;
        var splitter = crud.EstoyCreando ? crud.crudDeCreacion.Splitter : crud.crudDeEdicion.Splitter;

        _cambiandoAncho = false;
        _posicioInicial = undefined;
        _anchoInicial = undefined;
        _contenedorDeDatos = undefined;
        _contenedorDelVisor = undefined;

        ApiControl.IncluirCss(contenedorDeDatosMasVisor, crud.EstoyCreando ? ltrCss.crud.panelCreacion.VisorOculto : ltrCss.crud.panelDeEdicion.VisorOculto);
        splitter.addEventListener('mousedown', (e: MouseEvent) => {
            ComienzoCambioDelAnchoContenedorDeDatos(e);
        });
    }

    function ComienzoCambioDelAnchoContenedorDeDatos(e: MouseEvent) {

        e.preventDefault();
        e.stopPropagation();

        var crud = Crud.crudMnt;
        var contenedorDeDatos = crud.EstoyCreando ? crud.crudDeCreacion.ContenedorDeDatos : crud.crudDeEdicion.ContenedorDeDatos;

        var contenedorCabecera = crud.EstoyCreando ? crud.crudDeCreacion.ContenedorDeCabecera : crud.crudDeEdicion.ContenedorDeCabecera;
        var contenedorDelVisor = crud.EstoyCreando ? crud.crudDeCreacion.ContenedorDelVisor : crud.crudDeEdicion.ContenedorDelVisor;
        var contenedorDeDatosMasVisor = crud.EstoyCreando ? crud.crudDeCreacion.ContenedorDeDatosMasVisor : crud.crudDeEdicion.ContenedorDeDatosMasVisor;

        var splitter = crud.EstoyCreando ? crud.crudDeCreacion.Splitter : crud.crudDeEdicion.Splitter;

        _cambiandoAncho = true;
        _posicioInicial = e.clientX;
        _anchoInicial = contenedorDeDatos.offsetWidth;
        _contenedorDeDatos = contenedorDeDatos;
        _contenedorDelVisor = contenedorDelVisor;
        contenedorCabecera.style.width = "auto";
        contenedorDeDatosMasVisor.style.width = "auto";

        //console.log("Comienzo");

        //console.log("contenedorDeDatos: " + contenedorDeDatos.id );
        //console.log("contenedorCabecera: " + contenedorCabecera.id);
        //console.log("contenedorDelVisor: " + contenedorDelVisor.id );
        //console.log("contenedorDeDatosMasVisor: " + contenedorDeDatosMasVisor.id );


        document.addEventListener('mousemove', CambiarDeAnchoDelContenedorDeDatos.bind(splitter));
        document.addEventListener('mouseup', FinalizarCambioDeAnchoDelContenedorDeDatos.bind(splitter));
        document.addEventListener('mouseleave', FinalizarCambioDeAnchoDelContenedorDeDatos.bind(splitter));

    }

    function CambiarDeAnchoDelContenedorDeDatos(e: MouseEvent) {
        // Prevenir comportamiento predeterminado y propagación
        e.preventDefault();
        e.stopPropagation();

        // Verificar si el botón izquierdo está presionado (bitmask 1)
        if ((e.buttons & 1) === 0) { // 0 = botón izquierdo no presionado
            FinalizarCambioDeAnchoDelContenedorDeDatos(e);
            return;
        }

        if (!_cambiandoAncho) {
            return;
        }

        // Implementar throttle
        if (_lastExecution && Date.now() - _lastExecution < 16) { // 60 FPS
            return;
        }
        _lastExecution = Date.now();

        const crud = Crud.crudMnt;
        const splitter = crud.EstoyCreando ? crud.crudDeCreacion.Splitter : crud.crudDeEdicion.Splitter;

        const contenedorEditorRect = _contenedorDeDatos.parentElement.getBoundingClientRect();
        const splitterRect = splitter.getBoundingClientRect();

        // Ampliar área de detección del splitter
        const margenAmpliado = 50; // Margen adicional para tolerancia
        const dentroDelRango =
            e.clientX >= contenedorEditorRect.left &&
            e.clientX <= contenedorEditorRect.right &&
            e.clientX >= splitterRect.left - margenAmpliado &&
            e.clientX <= splitterRect.right + margenAmpliado;

        if (!dentroDelRango) {
            FinalizarCambioDeAnchoDelContenedorDeDatos(e);
            return;
        }

        const nuevoAnchoDatos = e.clientX - contenedorEditorRect.left;
        const nuevoAnchoVisor = contenedorEditorRect.width - nuevoAnchoDatos - 10; // 10px para splitter width

        // Ensure minimum widths
        const minWidth = 100; // Set a minimum width for both panels
        if (nuevoAnchoDatos < minWidth || nuevoAnchoVisor < minWidth) {
            return;
        }

        requestAnimationFrame(() => {
            if (!Definido(_contenedorDeDatos)) return;

            //console.log("Ancho de datos:" + `${nuevoAnchoDatos}px`);
            //console.log("Ancho de visor:" + `${nuevoAnchoVisor - splitter.clientWidth}px`);
            //console.log("Ancho de padre:" + `${nuevoAnchoVisor - splitter.clientWidth}px`);
            //console.log("splitter.style.left:" + `${nuevoAnchoDatos}px`);
            //console.log("Padre:" + `${_contenedorDelVisor.parentElement.id}`);

            _contenedorDeDatos.style.width = `${nuevoAnchoDatos}px`;
            _contenedorDelVisor.style.width = `${nuevoAnchoVisor - splitter.clientWidth}px`;
            if (!crud.EstoyCreando) _contenedorDelVisor.parentElement.style.width = `${nuevoAnchoVisor - splitter.clientWidth}px`;
            splitter.style.left = `${nuevoAnchoDatos}px`;
        });
    }


    function FinalizarCambioDeAnchoDelContenedorDeDatos(e: MouseEvent) {

        var crud = Crud.crudMnt;
        var splitter = crud.EstoyCreando ? crud.crudDeCreacion.Splitter : crud.crudDeEdicion.Splitter;
        if (!_cambiandoAncho || _posicioInicial === undefined)
            return;

        console.log("Terminar el arrastre:");
        try {
            ApiDelCrud.AjustarAnchoDeDatosMasVisor();
            GuardarTamanoDelVisor(crud);
        }
        finally {
            document.removeEventListener('mousemove', CambiarDeAnchoDelContenedorDeDatos.bind(splitter));
            document.removeEventListener('mouseup', FinalizarCambioDeAnchoDelContenedorDeDatos.bind(splitter));
            document.removeEventListener('mouseleave', FinalizarCambioDeAnchoDelContenedorDeDatos.bind(splitter));
            ResetearParametrosDeArrastre();
        }
    }


    async function GuardarTamanoDelVisor(crud: Crud.CrudMnt) {
        const params2 = {
            [Ajax.Param.idNegocio]: Encriptar(literal.ClaveDeEncriptacion, crud.IdNegocio),
            [Ajax.Param.idVista]: Encriptar(literal.ClaveDeEncriptacion, crud.IdVista),
            [Ajax.Param.peticion]: Encriptar(literal.ClaveDeEncriptacion, ltrMenus.eventosDeMf.Comun.TamanoDelVisor)
        };
        const url2 = `/${crud.Controlador}/${Ajax.EndPoint.ProcesarPeticion}?${new URLSearchParams(params2)}`;
        await fetch(url2, {
            method: 'POST',
            body: TamanoDelVisor(crud),
            keepalive: true
        });
    }

    function TamanoDelVisor(crud: Crud.CrudMnt) {
        let parametros: Array<Parametro> = new Array<Parametro>();
        let datosParaGuardar = Numero(_contenedorDelVisor.style.width.replace('px', ''));
        parametros.push(new Parametro(Ajax.Param.datosPeticion, datosParaGuardar));

        crud.TamanoDelVisor = datosParaGuardar;

        return JSON.stringify(parametros);
    }

    export async function GuardarMostrarVisorAlIniciar(crud: Crud.CrudMnt, mostrar: boolean) {
        const params2 = {
            [Ajax.Param.idNegocio]: Encriptar(literal.ClaveDeEncriptacion, crud.IdNegocio),
            [Ajax.Param.idVista]: Encriptar(literal.ClaveDeEncriptacion, crud.IdVista),
            [Ajax.Param.peticion]: Encriptar(literal.ClaveDeEncriptacion, ltrMenus.eventosDeMf.Comun.MostrarVisorAlIniciar)
        };

        let parametros: Array<Parametro> = new Array<Parametro>();
        parametros.push(new Parametro(Ajax.Param.datosPeticion, mostrar));

        const url2 = `/${crud.Controlador}/${Ajax.EndPoint.ProcesarPeticion}?${new URLSearchParams(params2)}`;
        await fetch(url2, {
            method: 'POST',
            body: JSON.stringify(parametros),
            keepalive: true
        });
    }


    function ResetearParametrosDeArrastre() {
        _cambiandoAncho = false;
        _posicioInicial = undefined;
        _anchoInicial = undefined;
        _contenedorDeDatos = undefined;
        _contenedorDelVisor = undefined;
        _lastExecution = 0;
    }

    export function IndicarControlMapeadoPoIa(control: HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement) {
        ApiControl.ResaltarControl(control, ltrCss.ia.mapeado);
        //ApiControl.IncluirCss(control, ltrCss.ia.mapeado);
        //const valor = control.value;

        //if (!(control instanceof HTMLSelectElement))
        //    // Añadir evento para quitar el borde azul cuando el usuario modifique el valor
        //    control.addEventListener('input', function () {
        //        if (control.value !== valor) {
        //            ApiControl.ExcluirCss(control, ltrCss.ia.mapeado);
        //        }
        //    });
        //else
        //    control.addEventListener('change', function () {
        //        if (control.value !== valor) {
        //            ApiControl.ExcluirCss(control, ltrCss.ia.mapeado);
        //        }
        //    });

    }

    export function Negocio_IrAlProveedor(numeroDeFila: number) {
        let idRegistro = Numero(ApiDeGrid.ObtenerValorDeLaFilaParaLaPropiedad(Crud.crudMnt.Tabla, numeroDeFila, ltrPropiedades.Elemento.Id));
        IrAProveedor(idRegistro);
    }

    export function Negocio_IrAlCliente(numeroDeFila: number) {
        let idRegistro = Numero(ApiDeGrid.ObtenerValorDeLaFilaParaLaPropiedad(Crud.crudMnt.Tabla, numeroDeFila, ltrPropiedades.Elemento.Id));
        IrACliente(idRegistro);
    }

    export function Negocio_IrALaFactura(numeroDeFila: number) {
        let idRegistro = Numero(ApiDeGrid.ObtenerValorDeLaFilaParaLaPropiedad(Crud.crudMnt.Tabla, numeroDeFila, ltrPropiedades.Elemento.Id));
        IrALaFactura(idRegistro);
    }

    export function Negocio_IrAlExpediente(numeroDeFila: number) {
        let idRegistro = Numero(ApiDeGrid.ObtenerValorDeLaFilaParaLaPropiedad(Crud.crudMnt.Tabla, numeroDeFila, ltrPropiedades.Elemento.Id));
        IrAExpediente(idRegistro);
    }

    function IrAExpediente(id: number) {
        let parametros: Array<Parametro> = new Array<Parametro>();
        parametros.push(new Parametro(literal.enumNegocio, Crud.crudMnt.EnumeradoDeNegocio));
        parametros.push(new Parametro(literal.id, id));
        ApiDePeticiones.EjecutarPeticion(Crud.crudMnt, Crud.crudMnt.Controlador, Ajax.EndPoint.Genericas.ObtenerUrlAlExpediente, parametros, new Array<Parametro>())
            .then((peticion) => {
                var url = `${window.location.origin}/${peticion.resultado.datos}`;
                EntornoSe.AbrirPestana(url);
            })
            .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
    }

    function IrAProveedor(id: number) {
        let parametros: Array<Parametro> = new Array<Parametro>();
        parametros.push(new Parametro(literal.enumNegocio, Crud.crudMnt.EnumeradoDeNegocio));
        parametros.push(new Parametro(literal.id, id));
        ApiDePeticiones.EjecutarPeticion(Crud.crudMnt, Crud.crudMnt.Controlador, Ajax.EndPoint.Genericas.ObtenerUrlAlProveedor, parametros, new Array<Parametro>())
            .then((peticion) => {
                var url = `${window.location.origin}/${peticion.resultado.datos}`;
                EntornoSe.AbrirPestana(url);
            })
            .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
    }

    function IrACliente(id: number) {
        let parametros: Array<Parametro> = new Array<Parametro>();
        parametros.push(new Parametro(literal.enumNegocio, Crud.crudMnt.EnumeradoDeNegocio));
        parametros.push(new Parametro(literal.id, id));
        ApiDePeticiones.EjecutarPeticion(Crud.crudMnt, Crud.crudMnt.Controlador, Ajax.EndPoint.Genericas.ObtenerUrlAlCliente, parametros, new Array<Parametro>())
            .then((peticion) => {
                var url = `${window.location.origin}/${peticion.resultado.datos}`;
                EntornoSe.AbrirPestana(url);
            })
            .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
    }

    export function IrALaFactura(id: number) {
        var url = `${window.location.origin}/${ltrUrls.Ventas.FacturasEmt}?id=${id}`;
        EntornoSe.AbrirPestana(url);
    }

}