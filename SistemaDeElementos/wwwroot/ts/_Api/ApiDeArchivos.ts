namespace ApiDeArchivos {

    var archivosLeidos = new Array<any>();

    export function SisDoc_CarpetasDeUnArchivador(parametros: string) {
        let partes = parametros.split(';');
        let idGrid: string = partes[0];
        let fila: number = Numero(partes[1]);
        let valor: number = Numero(ApiDeGrid.Expansor_ObtenerPropiedadDeLaFila(idGrid, fila, ltrParametrosUrl.id));
        let url = `${window.location.origin}/${ltrUrls.SistemaDocumental.Carpetas}?${ltrParametrosUrl.SisDoc.IdArchivador}=${valor}`;
        EntornoSe.AbrirPestana(url);
    }

    export function SisDoc_CambiarEncolumnado(idContenedor: string): void {
        let contenedorDeArchivos = document.getElementById(idContenedor) as HTMLDivElement;
        let columnas = Numero(contenedorDeArchivos.getAttribute('columnas'));
        columnas = AplicarEncolumnado(columnas - 1, contenedorDeArchivos);
        GuardarDisposicionDeArchivos(columnas);
    }

    export function SisDoc_SeleccionarTodo(idContenedor: string): void {
        let contenedor = document.getElementById(idContenedor);
        const noSeleccionados = contenedor.querySelectorAll<HTMLInputElement>('input[type="checkbox"]:not(:checked)');
        noSeleccionados.forEach((checkbox) => {
            checkbox.checked = true && checkbox.style.display !== ltrStyle.display.none;
        });
    }

    export function SisDoc_Tras_Seleccionar_Archivador(idLista: string): void {
        var lista = document.getElementById(idLista) as HTMLSelectElement;
        var modal = ApiPanel.BuscarModalContenedora(lista);
        if (lista.selectedIndex === Numero(literal.cero)) {
            var destino = ApiControl.BuscarListaDinamicaPorPropiedad(modal, ltrPropiedades.SisDoc.Archivo.Destino);
            ApiControl.DesbloquearListaDinamica(destino);
        }
        else {
            ApiListaDinamica.Bloquear_Blanquear(modal, ltrPropiedades.SisDoc.Archivo.Destino);
        }
    }
    export function SisDoc_Tras_Seleccionar_Destino(idLista: string): void {
        var lista = document.getElementById(idLista) as HTMLInputElement;
        var modal = ApiPanel.BuscarModalContenedora(lista);
        ApiControl.BloquearListaDeElemento(modal, ltrPropiedades.SisDoc.Archivo.ArchivadorDestino);
    }

    export function SisDoc_Tras_Blanquear_Destino(idLista: string): void {
        var lista = document.getElementById(idLista) as HTMLInputElement;
        var modal = ApiPanel.BuscarModalContenedora(lista);
        ApiControl.DesbloquearListaDeElemento(modal, ltrPropiedades.SisDoc.Archivo.ArchivadorDestino);
    }

    export function SisDoc_AnularSeleccion(idContenedor: string): void {
        let contenedor = document.getElementById(idContenedor);
        const seleccionados = contenedor.querySelectorAll<HTMLInputElement>('input[type="checkbox"]:checked');
        seleccionados.forEach((checkbox) => {
            checkbox.checked = false;
        });
    }

    export function SisDoc_AplicarOperacion(idModal: string) {
        let opcion: HTMLButtonElement = document.getElementById(idModal + '-' + atArchivo.AplicarOperacion) as HTMLButtonElement;
        var archivos = ArchivosSeleccionados(idModal)
        if (opcion.value.toLocaleLowerCase() === atArchivo.Operacion.Copiar) {
            AplicarOperacionALosArchivos(idModal, atArchivo.Operacion.Copiar, archivos);
        }
        if (opcion.value.toLocaleLowerCase() === atArchivo.Operacion.Mover) {
            AplicarOperacionALosArchivos(idModal, atArchivo.Operacion.Mover, archivos);
        }
        if (opcion.value.toLocaleLowerCase() === atArchivo.Operacion.Enlazar) {
            AplicarOperacionALosArchivos(idModal, atArchivo.Operacion.Enlazar, archivos);
        }
    }

    export function SisDoc_BloqueoMultiple(idModal: string) {
        let archivos: Array<HTMLInputElement> = ArchivosSeleccionados(idModal)
        let modal: HTMLDivElement = document.getElementById(idModal) as HTMLDivElement;

        let motivo = ApiControl.BuscarEditor(modal, ltrPropiedades.SisDoc.Bloqueo.Motivo);
        if (IsNullOrEmpty(motivo.value)) {
            MensajesSe.Info('Debe indicar el motivo de bloqueo');
            return;
        }
        let { idnegocio, idOrigen }: { idnegocio: number; idOrigen: number; } = ObtenerNegocioYElElemento(modal);
        let idsDeArchivos: Array<number> = ObtenerArchivosSeleccionados(archivos);

        let parametros = new Array<Parametro>();
        parametros.push(new Parametro(ltrPropiedades.SisDoc.Archivo.IdsDeArchivos, idsDeArchivos));

        ApiDePeticiones.ProcesarBloquearArchivos(modal, idnegocio, idOrigen, motivo.value, parametros)
            .then((peticion) => DespuesDeBloquearArchivos(peticion))
            .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
    }


    export function SisDoc_DesbloqueoMultiple(idModal: string) {
        let archivos: Array<HTMLInputElement> = ArchivosSeleccionados(idModal)
        let modal: HTMLDivElement = document.getElementById(idModal) as HTMLDivElement;

        let motivo = ApiControl.BuscarEditor(modal, ltrPropiedades.SisDoc.Bloqueo.Motivo);
        if (IsNullOrEmpty(motivo.value)) {
            MensajesSe.Info('Debe indicar el motivo de desbloqueo');
            return;
        }
        let { idnegocio, idOrigen }: { idnegocio: number; idOrigen: number; } = ObtenerNegocioYElElemento(modal);
        let idsDeArchivos: Array<number> = ObtenerArchivosSeleccionados(archivos);

        let parametros = new Array<Parametro>();
        parametros.push(new Parametro(ltrPropiedades.SisDoc.Archivo.IdsDeArchivos, idsDeArchivos));

        ApiDePeticiones.ProcesarDesbloquearArchivos(modal, idnegocio, idOrigen, motivo.value, parametros)
            .then((peticion) => DespuesDeDesbloquearArchivos(peticion))
            .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
    }

    export function SisDoc_GenerarZip(idModal: string) {
        let archivos: Array<HTMLInputElement> = ArchivosSeleccionados(idModal)
        let modal: HTMLDivElement = document.getElementById(idModal) as HTMLDivElement;

        let nombreArchivador = ApiControl.BuscarEditor(modal, ltrPropiedades.Elemento.Nombre);
        if (IsNullOrEmpty(nombreArchivador.value)) {
            MensajesSe.Info('Debe indicar el nombre del archivador a generar');
            return;
        }
        let { idnegocio, idOrigen }: { idnegocio: number; idOrigen: number; } = ObtenerNegocioYElElemento(modal);
        let idsDeArchivos: Array<number> = ObtenerArchivosSeleccionados(archivos);

        let parametros = new Array<Parametro>();
        parametros.push(new Parametro(ltrPropiedades.SisDoc.Archivo.IdsDeArchivos, idsDeArchivos));

        ApiDePeticiones.ProcesarGenerarZip(modal, idnegocio, idOrigen, nombreArchivador.value, parametros)
            .then((peticion) => DespuesDeGenerarZip(peticion))
            .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
    }

    export function SisDoc_BloquearArchivo(idModal: string, bloquear: boolean) {
        let modal: HTMLDivElement = document.getElementById(idModal) as HTMLDivElement;
        let motivo = ApiControl.BuscarEditor(modal, ltrPropiedades.SisDoc.Bloqueo.Motivo).value;
        let idArchivo = Numero(ApiControl.BuscarRestrictor(modal, ltrPropiedades.SisDoc.Bloqueo.idArchivo, ltrTipoControl.restrictorDeEdicion).getAttribute(atRestrictor.idRestrictor));
        let idnegocio: number = 0
        let idElemento: number = 0
        let estaElModuloCargado = typeof SistemaDocumental !== 'undefined';
        if (estaElModuloCargado && Definido(SistemaDocumental.JerarquiaDeCarpetas)) {
            idnegocio = SistemaDocumental.JerarquiaDeCarpetas.IdNegocio;
            idElemento = Numero(ApiControl.BuscarEditor(SistemaDocumental.JerarquiaDeCarpetas.ContenedorDelId, 'id').value);
        }
        else {
            idElemento = Numero(ObtenerPropiedad(Crud.crudMnt.crudDeEdicion.Registro, ltrPropiedades.Elemento.Id));
            let cabecera: HTMLDivElement = modal.parentElement.querySelector('.' + ltrCss.crud.cabecera);
            idnegocio = Numero(cabecera.getAttribute(literal.idNegocio));
        }
        ApiDePeticiones.BloquearArchivo(modal, bloquear ? Ajax.Archivos.accion.BloquearArchivo : Ajax.Archivos.accion.DesbloquearArchivo, idnegocio, idElemento, idArchivo, motivo)
            .then((peticion) => DespuesDeBloquearArchivo(peticion))
            .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
    }


    export function SisDoc_MostrarModalDeBloqueoMultiple(idModal: string): void {
        let seleccionados = ArchivosSeleccionados(idModal);
        if (seleccionados.length === 0) {
            MensajesSe.Info('Ha de seleccionar que archivos quiere bloquear');
            return;
        }
        let modal: HTMLDivElement = document.getElementById(idModal) as HTMLDivElement;

        ApiPanel.AbrirModal(modal);
        let motivo = ApiControl.BuscarEditor(modal, ltrPropiedades.SisDoc.Bloqueo.Motivo);
        motivo.focus();
    }

    export function SisDoc_MostrarModalDeDesbloqueoMultiple(idModal: string): void {
        let seleccionados = ArchivosSeleccionados(idModal);
        if (seleccionados.length === 0) {
            MensajesSe.Info('Ha de seleccionar que archivos quiere desbloquear');
            return;
        }
        let modal: HTMLDivElement = document.getElementById(idModal) as HTMLDivElement;

        ApiPanel.AbrirModal(modal);
        let motivo = ApiControl.BuscarEditor(modal, ltrPropiedades.SisDoc.Bloqueo.Motivo);
        motivo.focus();
    }
    export function SisDoc_MostrarModalDeGenerarZip(idModal: string): void {
        let seleccionados = ArchivosSeleccionados(idModal);
        if (seleccionados.length === 0) {
            MensajesSe.Info('Ha de seleccionar que archivos quiere descargar');
            return;
        }
        let modal: HTMLDivElement = document.getElementById(idModal) as HTMLDivElement;

        ApiPanel.AbrirModal(modal);
        let nombreArchivador = ApiControl.BuscarEditor(modal, ltrPropiedades.Elemento.Nombre);
        nombreArchivador.focus();
    }

    export function SisDoc_Procesar(idModal: string, operacion: string): void {
        const selectorDeArchivos = document.querySelector('.' + ltrCss.Archivos.SelectorDeArchivos) as HTMLDivElement;
        if (operacion.toLocaleLowerCase() === atArchivo.Operacion.Copiar || operacion === atArchivo.Operacion.Mover || operacion === atArchivo.Operacion.Enlazar) {
            Procesar(selectorDeArchivos, operacion)
            return;
        }

        MensajesSe.EmitirExcepcion('SisDoc_Procesar', 'No exite la operacion: ' + operacion + '. Impleméntela primero');
    }

    export function SisDoc_DescargarConGuid(boton: HTMLButtonElement) {
        const modal = ApiPanel.BuscarModalContenedora(boton);
        let input: HTMLInputElement = modal.querySelector(`input[propiedad="${literal.id}"]`) as HTMLInputElement;
        if (Definido(input)) {
            let idArchivo = Numero(input.value);
            if (idArchivo > 0) {
                let caducaEl: Date = ApiControl.LeerFechaHoraPorPropiedad(modal, ltrPropiedades.SisDoc.Archivo.CaducaEl);
                if (!EsFechaValida(caducaEl)) {
                    //caducaEl = new Date(); // Fecha y hora actuales
                    //caducaEl.setHours(caducaEl.getHours() + 2); // +1 hora
                    //MapearAlControl.FechaDatePorPropiedad(modal, ltrPropiedades.SisDoc.Archivo.CaducaEl, caducaEl);
                    caducaEl = null;
                }
                ApiDePeticiones.RegistrarDescargaConGuid(modal, idArchivo, caducaEl)
                    .then((peticion) => DespuesDeRegistrarDescargaConGuid(peticion))
                    .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
            }
        }
    }

    export function SisDoc_FiltrarEnSelector(filtro: HTMLInputElement) {
        const contenedor = document.getElementById(filtro.id.replace('-selector-de-archivos-filtrar', ''));
        const visores = contenedor.querySelectorAll<HTMLDivElement>(`div.${ltrCss.contenedorVisor}`);
        const filtroTexto = filtro.value.toLowerCase().trim();
        const subcadenas = filtroTexto.split('%'); // Divide el texto en subcadenas usando '%' como separador

        visores.forEach(div => {
            // Buscar la etiqueta <a> dentro del div
            const aTag = div.querySelector('a');
            // Obtener el texto del enlace (o cadena vacía si no existe)
            const texto = aTag?.textContent?.toLowerCase() || '';

            // Verificar si todas las subcadenas están presentes en el texto
            const todasLasSubcadenasPresentes = subcadenas.every(subcadena => texto.includes(subcadena));

            // Manipular clases según si todas las subcadenas están presentes o no
            if (todasLasSubcadenasPresentes) {
                ApiControl.ExcluirCss(div, ltrCss.contenedorVisorOculto);
            } else {
                ApiControl.IncluirCss(div, ltrCss.contenedorVisorOculto);
            }
        });
    }

    export function SisDoc_RecargarVisor(filtro: HTMLInputElement) {
        let estaElModuloCargado = typeof Crud !== 'undefined';
        if (estaElModuloCargado && Definido(Crud.crudMnt) && Crud.crudMnt.EstoyEditandoConsultando) {
            Crud.crudMnt.crudDeEdicion.RenderizarElPrimeroRenderizable(filtro);
        }
    }

    function DespuesDeRegistrarDescargaConGuid(peticion: ApiDeAjax.DescriptorAjax): any {
        var modal = peticion.llamador as HTMLDivElement;
        const idArchivo = peticion.DatosDeEntrada[ltrPropiedades.SisDoc.Archivo.idArchivo]
        const guid = peticion.resultado.datos;
        if (guid) {
            const dominio = window.location.hostname;
            const puerto = window.location.port;
            const protocolo = window.location.protocol;
            const urlDeDescarga = `${protocolo}//${dominio}${(IsNullOrEmpty(puerto) ? "" : `:${puerto}`)}/${ltrControladores.SisDoc.Archivos}/${Ajax.Archivos.accion.DescargaConGuid}?guid=${guid}&id=${idArchivo}`;
                        
            CopiarUrlAlPortapapeles(urlDeDescarga,''); 

            //    let caducaEl: Date = ApiControl.LeerFechaHoraPorPropiedad(modal, ltrPropiedades.SisDoc.Archivo.CaducaEl);
            //    CopiarUrlAlPortapapeles(urlDeDescarga, `La URL para descargar el archivo se ha copiado al porta papeles con validez hasta ${(caducaEl ? FormatearFecha(caducaEl, enumFormato.ddMMyyyyHHmm) : 'dentro de una hora')}`);
        }
    }
    export function SisDoc_MostrarModalParaSeleccionarDestino(idModal: string, operacion: string): void {
        operacion = operacion.toLocaleLowerCase();
        let seleccionados = ArchivosSeleccionados(idModal);
        if (seleccionados.length === 0) {
            MensajesSe.Info('Ha de seleccionar que archivos quiere ' + operacion);
            return;
        }
        let modal: HTMLDivElement = document.getElementById(idModal) as HTMLDivElement;

        let opcion: HTMLButtonElement = document.getElementById(idModal + '-' + atArchivo.AplicarOperacion) as HTMLButtonElement;
        let destino: HTMLInputElement = ApiControl.BuscarListaDinamicaPorPropiedad(modal, ltrPropiedades.SisDoc.Archivo.Destino);
        ApiListaDinamica.Blanquear(destino);

        let estaElModuloCargado = typeof SistemaDocumental !== 'undefined';
        let expresion: string = undefined;
        let idOrigenDiferente: number = 0
        let controlador: string = undefined;
        let negocio: string = undefined;
        if (estaElModuloCargado && Definido(SistemaDocumental.JerarquiaDeCarpetas)) {
            expresion = ApiControl.BuscarListaDinamicaPorPropiedad(SistemaDocumental.JerarquiaDeCarpetas.PanelDelDto, ltrPropiedades.SisDoc.Carpeta.Archivador).value + ": " +
                ApiControl.BuscarEditor(SistemaDocumental.JerarquiaDeCarpetas.PanelDelDto, ltrPropiedades.SisDoc.Carpeta.Nombre).value;
            idOrigenDiferente = Numero(ApiControl.BuscarEditor(SistemaDocumental.JerarquiaDeCarpetas.ContenedorDelId, literal.id).value);
            negocio = SistemaDocumental.JerarquiaDeCarpetas.Negocio;
            controlador = SistemaDocumental.JerarquiaDeCarpetas.Controlador;
            ApiPanel.OcultarFila(modal, ltrPropiedades.SisDoc.Archivo.ArchivadorDestino);
        }
        else {
            var etiquetaDestino = ApiControl.BuscarEtiqueta(modal, ltrPropiedades.SisDoc.Archivo.Destino);
            expresion = ObtenerPropiedad(Crud.crudMnt.crudDeEdicion.Registro, ltrPropiedades.Elemento.Expresion);
            idOrigenDiferente = Numero(ObtenerPropiedad(Crud.crudMnt.crudDeEdicion.Registro, ltrPropiedades.Elemento.Id));
            let cabecera: HTMLDivElement = modal.parentElement.querySelector('.' + ltrCss.crud.cabecera);
            negocio = cabecera.getAttribute(atControl.negocio);
            controlador = cabecera.getAttribute(atControl.controlador);
            etiquetaDestino.innerText = 'Destino: ' + Crud.crudMnt.NombreDeNegocio;
            if (operacion !== atArchivo.Operacion.Mover.toLocaleLowerCase() || Crud.crudMnt.EnumeradoDeNegocio === enumNegocio.Archivador)
                ApiPanel.OcultarFila(modal, ltrPropiedades.SisDoc.Archivo.ArchivadorDestino);
            else {
                let parametros: Array<Parametro> = new Array<Parametro>();
                ApiDePeticiones.LeerVinculosConEnum(modal, controlador, Crud.crudMnt.IdNegocio, enumNegocio.Archivador, idOrigenDiferente, parametros, new Array<Parametro>()).
                    then((peticion) => DespuesDeLeerVinculosConEnum(peticion)).
                    catch((peticion) => ApiDePeticiones.EmitirError(peticion));
            }
        }

        MapearAlControl.Restrictor(ApiControl.BuscarRestrictor(modal, ltrPropiedades.SisDoc.Archivo.IdOrigenDiferente, ltrTipoControl.restrictorDeEdicion), idOrigenDiferente, expresion);
        destino.setAttribute(atControl.controlador, controlador);
        destino.setAttribute(atControl.negocio, negocio);
        ApiPanel.AbrirModal(modal);
        opcion.value = operacion.charAt(0).toUpperCase() + operacion.slice(1);
        destino.focus();
    }

    function DespuesDeLeerVinculosConEnum(peticion: ApiDeAjax.DescriptorAjax): any {
        var modal = peticion.llamador as HTMLDivElement;
        if (Definido(peticion.resultado.datos) && peticion.resultado.datos.length > 0) {
            ApiPanel.MostrarFila(modal, ltrPropiedades.SisDoc.Archivo.ArchivadorDestino);
            var lista = ApiControl.BuscarListaDeElementos(modal, ltrPropiedades.SisDoc.Archivo.ArchivadorDestino);
            MapearAlControl.MapearElementosEnLaLista(lista.id, Numero(literal.cero), peticion.resultado.datos);
            ApiControl.DesbloquearListaDeElemento(modal, ltrPropiedades.SisDoc.Archivo.ArchivadorDestino);
        }
        else {
            ApiPanel.OcultarFila(modal, ltrPropiedades.SisDoc.Archivo.ArchivadorDestino);
        }
    }

    export function MostrarNuevoNombre(modal: HTMLDivElement) {
        let input: HTMLInputElement = modal.querySelector(`input[propiedad="${literal.id}"]`) as HTMLInputElement;
        if (Definido(input)) {
            let idArchivo = Numero(input.value);
            if (idArchivo > 0) {
                let ref: HTMLAnchorElement = document.getElementById(`refJs-${idArchivo}`) as HTMLAnchorElement;
                let nombre: HTMLInputElement = modal.querySelector(`input[propiedad="${literal.nombre}"]`) as HTMLInputElement;
                ref.text = nombre.value;
            }
        }
    }

    export function BlanquearArchivo(archivo: HTMLInputElement, blanquearImagen: boolean) {
        archivo.setAttribute(atArchivo.idArchivo, '0');
        archivo.value = "";
        archivo.removeAttribute(atArchivo.idArchivo);
        archivo.removeAttribute(atArchivo.nombre);
        archivo.classList.remove(ltrCss.crtlNoValido);
        archivo.classList.add(ltrCss.crtlValido);
        InicializarBarra(archivo);
        BlanquearInfoArchivo(archivo);
        if (EsImagen(archivo) && blanquearImagen) {
            BlanquearImagen(archivo);
        }
    }

    export function OcultarArchivo(archivo: HTMLInputElement, ocultar: boolean): void {
        if (!IsNullOrEmpty(archivo.getAttribute(atArchivo.canvas)))
            OcultarImagen(archivo, ocultar);
    }

    export function PulsadoSubirPortaPapeles(idSelector: string) {

        let selectorDeArchivos: HTMLInputElement = document.getElementById(idSelector) as HTMLInputElement;

        navigator.clipboard.read().then(data => {
            for (const item of data) {
                if (item.types.includes('image/png') || item.types.includes('image/jpeg')) {
                    item.getType('image/png')
                        .then(blob => {
                            CrearFicheroDelPortaPapeles(selectorDeArchivos, 'imagen', blob);
                        })
                        .catch(error => {
                            MensajesSe.Error("PegarPortaPapeles", "No se pudo obtener la imagen del portapapeles");
                        });
                }
                else if (item.types.includes('text/plain')) {
                    item.getType('text/plain')
                        .then(blob => {
                            CrearFicheroDelPortaPapeles(selectorDeArchivos, 'texto', blob);
                        })
                        .catch(error => { MensajesSe.Error("PegarPortaPapeles", "No se pudo obtener el texto del portapapeles"); });

                }
                else {
                    MensajesSe.Info("No hay imagen ni texto plano en el portapapeles");
                }
            }
        });
    }


    function CrearFicheroDelPortaPapeles(selectorDeArchivos: HTMLInputElement, tipo: string, blob: Blob) {
        let negocio = selectorDeArchivos.getAttribute(atControl.negocio);
        let idElemento = selectorDeArchivos.getAttribute(atControl.idElemento);
        let idDondeMostrar = selectorDeArchivos.getAttribute('contenedor-donde-mostrar');
        let dondeMostrar: HTMLDivElement = document.getElementById(idDondeMostrar) as HTMLDivElement;

        if (NoDefinido(negocio))
            MensajesSe.EmitirExcepcion("PulsarAnexarArchivos", "Debe definir un negocio al que anexar los archivos");

        if (Numero(idElemento) == 0)
            MensajesSe.EmitirExcepcion("PulsarAnexarArchivos", "Debe definir el elemento al que anexar los archivos");

        const fechaActual = new Date();
        const extension = tipo === 'texto' ? 'txt' : 'png';
        const prostfijo = `${fechaActual.getFullYear()}${(fechaActual.getMonth() + 1).toString().padStart(2, '0')}${fechaActual.getDate().toString().padStart(2, '0')}_${fechaActual.getHours().toString().padStart(2, '0')}${fechaActual.getMinutes().toString().padStart(2, '0')}${fechaActual.getSeconds().toString().padStart(2, '0')}`;
        const nombreArchivo = `${tipo}_${prostfijo}.${extension}`;
        const archivoDesdePortapapeles = tipo === 'texto' ? new File([blob], nombreArchivo, { type: 'text/plain' }) : new File([blob], nombreArchivo, { type: 'image/png' });
        let idInfoArchivo = `info-${selectorDeArchivos.id}.portapapeles`;
        let infoArchivo: HTMLInputElement = document.getElementById(idInfoArchivo) as HTMLInputElement;
        if (!infoArchivo) {
            infoArchivo = document.createElement('input');
            infoArchivo.type = 'hidden';
            infoArchivo.id = idInfoArchivo;
            selectorDeArchivos.appendChild(infoArchivo);
        }
        AnexarArchivo(negocio, idElemento, archivoDesdePortapapeles, infoArchivo, dondeMostrar);
    }


    export function PulsadoAnexarArchivos(idSelector: string) {
        let selectorDeArchivos: HTMLInputElement = document.getElementById(idSelector) as HTMLInputElement;
        let negocio = selectorDeArchivos.getAttribute(atControl.negocio);
        let idElemento = selectorDeArchivos.getAttribute(atControl.idElemento);
        let idDondeMostrar = selectorDeArchivos.getAttribute('contenedor-donde-mostrar');
        let dondeMostrar: HTMLDivElement = document.getElementById(idDondeMostrar) as HTMLDivElement;

        if (NoDefinido(negocio))
            MensajesSe.EmitirExcepcion("PulsarAnexarArchivos", "Debe definir un negocio al que anexar los archivos");

        if (Numero(idElemento) == 0)
            MensajesSe.EmitirExcepcion("PulsarAnexarArchivos", "Debe definir el elemento al que anexar los archivos");

        for (let i: number = 0; i < selectorDeArchivos.files.length; i++) {
            let idInfoArchivo = `info-${selectorDeArchivos.id}.${i}`;
            let infoArchivo: HTMLInputElement = document.getElementById(idInfoArchivo) as HTMLInputElement;
            if (NoDefinido(infoArchivo))
                continue;
            AnexarArchivo(negocio, idElemento, selectorDeArchivos.files[i], infoArchivo, dondeMostrar);
        }
    }


    export function AnexarArchivosArrastradosAlArbol(idSelector: string, recargarArchivosTrasTerminarSubida: boolean) {
        let selectorDeArchivos: HTMLInputElement = document.getElementById(idSelector) as HTMLInputElement;
        let negocio = selectorDeArchivos.getAttribute(atControl.negocio);
        let idElemento = selectorDeArchivos.getAttribute(atControl.idElemento);
        let idDondeMostrar = selectorDeArchivos.getAttribute('contenedor-donde-mostrar');
        let dondeMostrar: HTMLDivElement = document.getElementById(idDondeMostrar) as HTMLDivElement;

        if (NoDefinido(negocio))
            MensajesSe.EmitirExcepcion("PulsarAnexarArchivos", "Debe definir un negocio al que anexar los archivos");

        if (Numero(idElemento) == 0)
            MensajesSe.EmitirExcepcion("PulsarAnexarArchivos", "Debe definir el elemento al que anexar los archivos");

        for (let i: number = 0; i < selectorDeArchivos.files.length; i++) {
            AnexarArchivo(negocio, idElemento, selectorDeArchivos.files[i], null, dondeMostrar, i == selectorDeArchivos.files.length - 1 ? recargarArchivosTrasTerminarSubida : false);
        }
    }


    export function SubirArchivos(panel: HTMLDivElement): Promise<string[]> {

        const promesas: Promise<string>[] = [];

        let archivos: NodeListOf<HTMLInputElement> = panel.querySelectorAll(`[${atControl.tipo}=${ltrTipoControl.Archivo}]`) as NodeListOf<HTMLInputElement>;
        BlanquearEstado(archivos);

        for (let i: number = 0; i < archivos.length; i++) {
            if (archivos[i].files.length > 0) {
                let idArchivo: string = archivos[i].getAttribute(literal.id);
                let controlador: string = archivos[i].getAttribute(atArchivo.controlador);

                let promesa: Promise<string> = SubirArchivo(controlador, idArchivo);

                promesas.push(promesa);
            }
            else {
                CambiarEstado(archivos[i], atArchivo.situacion.sinArchivo);
            }
        }
        return Promise.all(promesas);
    }

    function BlanquearEstado(archivos: NodeListOf<HTMLInputElement>) {
        for (let i: number = 0; i < archivos.length; i++) {
            CambiarEstado(archivos[i], atArchivo.situacion.pendiente);
        }
    }

    export function SeleccionarImagen(idArchivo: string) {
        let inputFile: HTMLDivElement = document.getElementById(idArchivo) as HTMLDivElement;
        if (inputFile) {
            inputFile.click();
        }
    }

    export function MostrarCanvas(controlador: string, idArchivo: string, idCanva: string) {

        function visializarImagen() {
            let htmlCanvas: HTMLCanvasElement = document.getElementById(idCanva) as HTMLCanvasElement;
            if (htmlCanvas) {
                htmlCanvas.width = 100;
                htmlCanvas.height = 100;
                var canvas = htmlCanvas.getContext('2d');
                canvas.drawImage(img, 0, 0, 100, 100);
            }
            SubirArchivo(controlador, idArchivo)
                .then(() => {
                    if (!Definido(canvas) && typeof Crud !== 'undefined' && Definido(Crud.CrudCreacion) && Crud.crudMnt.EstoyCreando) {
                        let archivo: HTMLInputElement = document.getElementById(idArchivo) as HTMLInputElement
                        let id = archivo.getAttribute(atArchivo.idArchivo);
                        Crud.crudMnt.crudDeCreacion.MostrarArchivo(archivo, Numero(id), 'imagen-pegada.png');
                    }
                })
                .catch(() => {
                    let archivo: HTMLInputElement = document.getElementById(idArchivo) as HTMLInputElement;
                    BlanquearImagen(archivo);
                });
        }

        function ErrorAlVisializar() {
            ApiDeArchivos.BlanquearArchivo(archivo, true);
            MensajesSe.Error("ErrorAlVisializar", "Fichero no válido para mostrar en un Canvas");
        }

        let archivo: HTMLInputElement = document.getElementById(idArchivo) as HTMLInputElement;
        InicializarBarra(archivo);
        let ficheros = archivo.files;

        let filePath: string = ficheros[0].name;
        let extensiones: string = archivo.getAttribute(atArchivo.extensionesValidas);

        var ext = filePath.substring(filePath.lastIndexOf('.') + 1).toLowerCase();
        if (extensiones !== '*' && extensiones.indexOf(ext) < 0) {
            MensajesSe.Error("MostrarCanvas", `Extensión no valida, sólo se permite extensiones del tipo '${extensiones}'`);
            return;
        }
        OcultarArchivo(archivo, false);
        var img = new Image();
        img.src = URL.createObjectURL(ficheros[0]);

        img.onload = visializarImagen;
        img.onerror = ErrorAlVisializar;
    };

    export function SeleccionarArchivos(idSelector: string) {
        let inputFile: HTMLDivElement = document.getElementById(idSelector) as HTMLDivElement;
        if (inputFile) {
            inputFile.click();
        }
    }

    export function SeleccionarArchivo(idSelector: string) {
        let inputFile: HTMLDivElement = document.getElementById(idSelector) as HTMLDivElement;
        if (inputFile) {
            inputFile.click();
        }
    }

    export function CrearArchivosPendietesDeSubir(idSelector: string, filesDropped?: FileList): void {
        var div = document.getElementsByClassName('selector-de-archivos')[0] as HTMLDivElement;
        if (!ApiControl.EsVisible(div) || div.classList.contains(ltrCss.Archivos.SelectorDeArchivosEnConsulta)) {
            MensajesSe.Info("El selector de archivos no está habilitado");
            return;
        }

        const selectorDeArchivos = document.getElementById(idSelector) as HTMLInputElement;
        let ficheros: File[];

        if (filesDropped && filesDropped.length > 0) {
            ficheros = Array.from(filesDropped);
            // Asignar los archivos arrastrados al selectorDeArchivos.files
            const dataTransfer = new DataTransfer();
            ficheros.forEach(file => dataTransfer.items.add(file));
            selectorDeArchivos.files = dataTransfer.files;
        } else if (selectorDeArchivos.files && selectorDeArchivos.files.length > 0) {
            ficheros = Array.from(selectorDeArchivos.files);
        } else {
            console.log("No hay archivos seleccionados o arrastrados");
            return;
        }

        //contenedor-crud_archivadordto_panel-editor-archivos-selector-seleccionados
        const idContenedor = selectorDeArchivos.getAttribute(atArchivo.archivosSeleccionados);
        const panel = document.getElementById(idContenedor) as HTMLDivElement;

        // Función para convertir File[] a FileList
        const arrayToFileList = (array: File[]): FileList => {
            const dt = new DataTransfer();
            array.forEach(file => dt.items.add(file));
            return dt.files;
        };

        for (let i = 0; i < ficheros.length; i++) {
            const file = ficheros[i];
            ValidarArchivoSeleccionado(selectorDeArchivos, file);
            ApiControl.CrearVisorInfoArchivo(
                panel,
                `${idSelector}.${i}`,
                ltrCss.contenedorInfoArchivo,
                file.name,
                () => {
                    const fileList = arrayToFileList(ficheros);
                    ApiDeArchivos.QuitarArchivoSeleccionado(fileList, idContenedor, `${idSelector}.${i}`);
                    // Actualizar ficheros después de quitar el archivo
                    ficheros = Array.from(fileList);
                }
            );
        }
    }

    export function QuitarArchivoSeleccionado(ficheros: FileList, idContenedor: string, idInfoArchivo: string): void {
        let seleccionados: HTMLInputElement = document.getElementById(idContenedor) as HTMLInputElement;
        let infoArchivo: HTMLInputElement = document.getElementById(idInfoArchivo) as HTMLInputElement;
        let i = Numero(infoArchivo.id.substring(infoArchivo.id.indexOf('.') + 1));
        seleccionados.removeChild(infoArchivo);
        ficheros.item(i).stream = null;
    }
    function TamanoDelArchivo(bytes: number, enMB: boolean): string {
        if (enMB) {
            const tamañoMB = bytes / (1024 * 1024);
            return tamañoMB.toLocaleString(undefined, {
                minimumFractionDigits: 2,
                maximumFractionDigits: 2
            }) + ' MB';
        } else {
            const tamañoKB = bytes / 1024;
            return tamañoKB.toLocaleString(undefined, {
                minimumFractionDigits: 2,
                maximumFractionDigits: 2
            }) + ' KB';
        }
    }

    function ValidarArchivoSeleccionado(archivo: HTMLInputElement, fichero: File) {
        let extensiones: string = archivo.getAttribute(atArchivo.extensionesValidas);
        let limite: number = Numero(archivo.getAttribute(atArchivo.limiteEnByte));

        const LIMITE_EN_MB = limite / (1024 * 1024);
        const mostrarEnMB = LIMITE_EN_MB > 1;

        var ext = fichero.name.substring(fichero.name.lastIndexOf('.') + 1).toLowerCase();
        if (extensiones !== "*" && extensiones.toLocaleLowerCase().indexOf(ext.toLocaleLowerCase()) < 0) {
            MensajesSe.EmitirExcepcion("ValidarArchivo", `Extensión no valida, sólo se permite extensiones del tipo '${extensiones}'`);
        }

        if (limite > 0 && limite < fichero.size) {
            const tamañoArchivo = TamanoDelArchivo(fichero.size, mostrarEnMB);
            let limiteFormateado: string;

            if (mostrarEnMB) {
                limiteFormateado = LIMITE_EN_MB.toLocaleString(undefined, {
                    minimumFractionDigits: 2,
                    maximumFractionDigits: 2
                }) + ' MB';
            } else {
                const limiteKB = limite / 1024;
                limiteFormateado = limiteKB.toLocaleString(undefined, {
                    minimumFractionDigits: 2,
                    maximumFractionDigits: 2
                }) + ' KB';
            }

            MensajesSe.EmitirExcepcion("ValidarArchivo", `Tamaño del fichero demasiado grande: ${tamañoArchivo}, el límite es: ${limiteFormateado}`);
        }
    }

    export function MostrarArchivoSelecionado(idContenedor: string, idArchivo: string, idInfoArchivo: string) {
        let contenedor: HTMLDivElement = document.getElementById(idContenedor) as HTMLDivElement;
        let archivo: HTMLInputElement = contenedor.querySelector(`input[id='${idArchivo}']`) as HTMLInputElement;
        MostrarArchivoPendienteDeSubir(archivo, idInfoArchivo);
    }

    export function SubirArchivoSeleccionado(idContenedor: string, idArchivo: string, idInfoArchivo: string) {

        const contenedor: HTMLDivElement = document.getElementById(idContenedor) as HTMLDivElement;
        const estaElModuloCargado = typeof Crud !== 'undefined';
        if (estaElModuloCargado && Definido(Crud.crudMnt) && Crud.crudMnt.EstoyEnMantenimiento && Crud.crudMnt.EstaVisualizandoUnSeleccionado) {
            if (ApiControl.EstaContenidoEn(Crud.crudMnt.ContenedorDeGraficos, contenedor)) {
                MensajesSe.Info("Para subir un archivo, edite el registro seleccionado");
                return;
            }
        }

        const archivo: HTMLInputElement = contenedor.querySelector(`input[id='${idArchivo}']`) as HTMLInputElement;
        MostrarArchivoPendienteDeSubir(archivo, idInfoArchivo);
        const controlador: string = archivo.getAttribute(atArchivo.controlador);

        CambiarEstado(archivo, atArchivo.situacion.subiendo);
        ApiDePeticiones.SubirPorTrozos(controlador, archivo.files, idArchivo);
    }

    function TrasSubirArchivoSeleccionado(peticion: any) {
        TrasSubirElArchivo(peticion);
        let datos: ApiDePeticiones.DatosPeticionSubirArchivo = peticion.DatosDeEntrada;
        let archivo: HTMLInputElement = datos.Archivo();
        let accion = archivo.getAttribute(atArchivo.trasSeleccionar);
        if (Definido(accion))
            Evaluar('ApiDeArchivos.TrasSubirArchivoSeleccionado', accion, accion.includes('this') ? archivo : undefined);
    }

    function ErrorAlSubirArchivoSeleccionado(peticion: any) {
        if (peticion.Request.status === 413) return;
        ApiDePeticiones.EmitirError(peticion);
    }

    function ArchivosSeleccionados(idModal: string): Array<HTMLInputElement> {
        var idContenedor = idModal.replace('modal-contenedor-', '')
            .replace('-seleccionar-destino', '')
            .replace('-bloqueo-multiple', '')
            .replace('-desbloqueo-multiple', '')
            .replace('-generar-zip', '');
        let contenedor = document.getElementById(idContenedor);
        const checkboxes = contenedor.querySelectorAll<HTMLInputElement>('input[type="checkbox"]:checked');
        const selectedCheckboxes = Array.from(checkboxes);
        return selectedCheckboxes;
    }

    function Procesar(selectorDeArchivos: HTMLDivElement, operacion: string) {
        const idBoton = selectorDeArchivos.id + '-' + operacion.toLowerCase();
        const boton = document.getElementById(idBoton) as HTMLButtonElement;
        let guid = ApiLocalStorage.Guid();

        if (!Definido(guid))
            GuardarArchivosParaProcesar(boton, operacion);
        else if (Definido(guid) && boton.getAttribute(atControl.Guid) === guid) {
            ApiLocalStorage.FinalizarOperacionConArchivos();
            ApiLocalStorage.PararTesteoDeRecargarArchivos();
        }
        else {
            if (ApiLocalStorage.Operacion() === operacion)
                ProcesarArchivosSeleccionados(boton);
            else {
                ApiLocalStorage.FinalizarOperacionConArchivos();
                GuardarArchivosParaProcesar(boton, operacion);
            }
        }

    }

    function GuardarArchivosParaProcesar(boton: HTMLButtonElement, operacion: string): void {
        const contenedorDeArchivos = document.querySelector('.' + ltrCss.Archivos.ContenedorDeArchivo) as HTMLDivElement;
        const checksSeleccionados = contenedorDeArchivos.querySelectorAll('input[type="checkbox"]:checked');
        if (checksSeleccionados.length === 0) {
            ApiLocalStorage.FinalizarOperacionConArchivos();
            MensajesSe.Info('Ha de seleccionar los archivos a ' + operacion);
            return;
        }
        boton.classList.remove(ltrCss.Archivos.CancelarOperacion);

        const listaIds = Array.from(checksSeleccionados).map(check => {
            const id = check.id.replace('check-', '');
            return parseInt(id, 10);
        }).filter(id => !isNaN(id));

        let { idNegocio, idElemento }: { idNegocio: number; idElemento: number; } = ObtenerNegocioElemento();
        const origenOperacion: [number, number] = [idNegocio, idElemento];

        ApiLocalStorage.IniciarOperacion(boton, origenOperacion, listaIds);
    }
    function ProcesarArchivosSeleccionados(boton: HTMLButtonElement): void {

        let parametros = new Array<Parametro>();
        const idsDeArchivos = ApiLocalStorage.ArchivosSeleccionados();
        const origen: [number, number] = ApiLocalStorage.PadreDeLosArchivos();

        if (Definido(idsDeArchivos) && Definido(origen)) {
            parametros.push(new Parametro(ltrPropiedades.SisDoc.Archivo.IdsDeArchivos, idsDeArchivos));
            let { idNegocio, idElemento }: { idNegocio: number; idElemento: number; } = ObtenerNegocioElemento();

            var operacionSolicitada = ApiLocalStorage.Operacion();
            var operacionPulsada = ApiLocalStorage.OperacionQueSeQuiereRealizar(boton);
            if (operacionPulsada !== operacionSolicitada) {
                MensajesSe.Info('No puede ' + operacionSolicitada + ' los archivo seleccionados porque ha pulsado ' + operacionPulsada);
                ApiLocalStorage.RefrescarIu();
                return;
            }

            if (Numero(origen[0]) === idNegocio && Numero(origen[1]) === idElemento) {
                ApiLocalStorage.FinalizarOperacionConArchivos();
                MensajesSe.Info('No puede ' + operacionSolicitada + ' los archivo seleccionados a si mismo');
                return;
            }

            ApiDePeticiones.OperacionConArchivos(boton, operacionSolicitada, Numero(origen[0]), Numero(origen[1]), idNegocio, idElemento, parametros)
                .then((peticion) => DespuesDeOperacionConArchivos(peticion))
                .catch((peticion) => ApiDePeticiones.EmitirError(peticion))
                .finally(() => ApiLocalStorage.FinalizarOperacionConArchivos());
        }
        else {
            ApiLocalStorage.FinalizarOperacionConArchivos();
            MensajesSe.Info('La información de ' + operacionSolicitada + ' está corrompida vuelva a realizar la operación');
        }
    }

    function DespuesDeOperacionConArchivos(peticion: ApiDeAjax.DescriptorAjax): any {
        let operacion = ObtenerPropiedad(peticion.DatosDeEntrada, Ajax.Param.operacion);
        let boton = peticion.llamador as HTMLButtonElement;
        RecargarMostrarArchivosAnexados();
        ApiLocalStorage.FinalizarOperacionConArchivos();
        ApiLocalStorage.ActivarRecargarArchivos();
    }

    function MostrarArchivoPendienteDeSubir(archivo: HTMLInputElement, idInfoArchivo: string): void {

        if (archivo.files === undefined || archivo.files.length === 0 || IsNullOrEmpty(archivo.files[0].name)) {
            BlanquearInfoArchivo(archivo);
            return;
        }

        ValidarArchivoSeleccionado(archivo, archivo.files[0]);
        let ficheros = archivo.files;
        InicializarBarra(archivo);
        let filePath: string = ficheros[0].name;

        let infoArchivo: HTMLInputElement = document.getElementById(idInfoArchivo) as HTMLInputElement;
        infoArchivo.style.display = ltrStyle.display.block;
        infoArchivo.value = `${filePath} (${ficheros[0].size} bytes, ${ficheros[0].type} )`;
    };

    export function MostrarArchivosAnexados(idContenedor: string, negocio: string, idElemento: number, AlTerminarDeLeer: Function = null): HTMLDivElement {
        let contenedor: HTMLDivElement = document.getElementById(idContenedor) as HTMLDivElement;
        //ApiLocalStorage.ActivarProceso();
        if (Definido(contenedor)) {
            contenedor.innerHTML = "";
            ApiDeArchivos.LeerAnexados(contenedor, negocio, idElemento, 0, Numero(Ajax.Archivos.parametro.Cantidad), undefined, AlTerminarDeLeer);
            return contenedor;
        }
        return undefined;
    }

    let leyendo: Array<string> = new Array<string>();
    let procesoDeLectura = new Map<string, number>();

    export function LeerAnexados(contenedor: HTMLDivElement, negocio: string, idElemento: number, posicion: number, cantidad: number, guid: string = undefined, alterminarDeleer: Function = null) {

        if (!Definido(guid)) {
            let estaElModuloCargado = typeof SistemaDocumental !== 'undefined';
            if (estaElModuloCargado && Definido(SistemaDocumental.JerarquiaDeCarpetas)) {
                guid = SistemaDocumental.JerarquiaDeCarpetas.Guid;
            }
            else {
                guid = Crud.Consultor ? Crud.Consultor.GuidDeConsulta :  Crud.crudMnt.Guid;
            }
        }

        if (!leyendo.includes(guid + ':' + idElemento))
            leyendo.push(guid + ':' + idElemento);

        procesoDeLectura.set(guid, posicion);

        ApiDePeticiones.LeerAnexados(contenedor, negocio, idElemento, posicion, cantidad, guid)
            .then((peticion) => {
                if (Definido(alterminarDeleer)) alterminarDeleer(peticion);
                return ApiDeArchivos.DespuesDeLeerAnexados(peticion, posicion, cantidad, guid, alterminarDeleer);
            })
            .catch((peticion) => {
                leyendo = leyendo.filter(numero => numero !== guid + ':' + idElemento);
                ApiDeAjax.ErrorTrasPeticion('LeerAnexados', peticion)
            });
    }

    export function DespuesDeLeerAnexados(peticion: ApiDeAjax.DescriptorAjax, posicion: number, cantidad: number, guid: string, alterminarDeleer: Function) {

        var posicionLeida = procesoDeLectura.get(guid);
        if (posicionLeida === 0 && posicion > 0)
            return;

        let dondeMostrar: HTMLDivElement = peticion.llamador as HTMLDivElement;
        let negocio: string = peticion.DatosDeEntrada.negocio as string;
        let idElemento: number = peticion.DatosDeEntrada.idElemento as number;
        let archivosDto = peticion.resultado.datos as Array<any>;

        //if (!ApiPanel.EsVisible(dondeMostrar))
        //    return;

        let referencia = document.getElementById(dondeMostrar.id.replace('contenedor-', 'mostrar.') + '.ref') as HTMLAnchorElement;
        if (Definido(referencia)) {
            if (peticion.resultado.datos.length + posicion > 0) {
                referencia.innerText = `Archivos: ${posicion + archivosDto.length}`;
                referencia.classList.add(ltrCss.Espan.conContenido);
            }
            else {
                referencia.innerText = `Archivos`;
                referencia.classList.remove(ltrCss.Espan.conContenido);
            }
        }
        var modo = ModoAcceso.Parsear(peticion.resultado.modoDeAcceso);
        for (let i: number = 0; i < archivosDto.length; i++) {
            var visor = ApiDeArchivos.MostrarArchivoAnexado(dondeMostrar, negocio, idElemento, archivosDto[i]);
            if (!ModoAcceso.EsGestor(modo))
                ModoAcceso.AplicarAlContenedorDeArchivos(visor, modo);
        };

        var selector = document.querySelector('.' + ltrCss.Archivos.SelectorDeArchivos) as HTMLDivElement;
        if (archivosDto.length > 0) {
            if (!selector.classList.contains(ltrCss.divNoVisible))
                selector.classList.add(ltrCss.divNoVisible);
            ApiDeArchivos.LeerAnexados(dondeMostrar, negocio, idElemento, posicion + archivosDto.length, cantidad, guid, alterminarDeleer);
        }
        else {
            selector.classList.remove(ltrCss.divNoVisible);
            leyendo = leyendo.filter(numero => numero !== guid + ':' + idElemento);
            if (negocio === ltrNegocioSe.Nombre.Carpetas) {
                const dtoFormulario = SistemaDocumental.JerarquiaDeCarpetas.PanelDelDto;
                const idCarpetaSeleccionada = dtoFormulario.getAttribute(Formulario.ltrJerarquia.arbol.nodoSeleccionado);
                const idCarpeta = Numero(idCarpetaSeleccionada.replace('.li', ''));
                const anchor = (document.getElementById(idCarpetaSeleccionada) as HTMLAnchorElement).querySelector('a');
                var partes = anchor.innerText.split(ltrSimbolos.dosPuntosConEspacio)
                anchor.innerText = partes[0] + (partes.length == 2 ? `${ltrSimbolos.dosPuntosConEspacio}${posicion}` : '');
                const draggableElements = document.querySelectorAll(`.${ltrCss.contenedorVisorRef} a`);
                draggableElements.forEach(element => {
                    element.setAttribute('draggable', 'true');
                    element.addEventListener('dragstart', (event: DragEvent) => {
                        const target = event.target as HTMLAnchorElement;
                        const container = target.closest(`.${ltrCss.contenedorVisorRef}`);
                        const idArchivo = container.id.split('-').pop();
                        event.dataTransfer.setData('text/plain', `archivo-carpeta:${idArchivo}-${idCarpeta}`);
                        event.dataTransfer.effectAllowed = "move";
                        ApiLocalStorage.IniciarTesteoDeRecargarArchivos(SistemaDocumental.JerarquiaDeCarpetas.Guid);
                    });
                });
            }
        }
    }

    function AnexarArchivo(negocio: string, idElemento: string, fichero: File, infoArchivo: HTMLInputElement, dondeMostrar: HTMLDivElement, recargarArchivosTrasTerminarSubida: boolean = false) {
        ApiDePeticiones.AnexarArchivo(this, negocio, idElemento, fichero, infoArchivo, dondeMostrar)
            .then((peticion) => {
                ApiDeArchivos.DespuesDeAnexarArchivo(peticion);
                if (recargarArchivosTrasTerminarSubida)
                    ApiDeArchivos.RecargarMostrarArchivosAnexados();
            })
            .catch((peticion) => ApiDeAjax.ErrorTrasPeticion(Ajax.Archivos.accion.AnexarArchivo, peticion));
    }

    export function DespuesDeAnexarArchivo(peticion: ApiDeAjax.DescriptorAjax) {
        let infoarchivo: HTMLInputElement = peticion.DatosDeEntrada.infoArchivo as HTMLInputElement;
        let dondeMostrar: HTMLDivElement = peticion.DatosDeEntrada.dondeMostrar as HTMLDivElement;
        let negocio: string = peticion.DatosDeEntrada.negocio as string;
        let idElemento: number = peticion.DatosDeEntrada.idElemento as number;
        let archivoDto = peticion.resultado.datos;

        if (!infoarchivo) {
            return;
        }

        if (!infoarchivo.id.includes('portapapeles')) infoarchivo.parentElement.remove();
        ApiDeArchivos.MostrarArchivoAnexado(dondeMostrar, negocio, idElemento, archivoDto);

        let referencia = document.getElementById(dondeMostrar.id.replace('contenedor-', 'mostrar.') + '.ref') as HTMLAnchorElement;
        if (Definido(referencia)) {
            let ficheros = dondeMostrar.querySelectorAll(`div[${atControl.class}=${ltrCss.Espan.cssVisorArchivos}]`) as NodeListOf<HTMLDivElement>;
            let titulo = referencia.getAttribute(ltrEspanes.Atributos.titulo);
            if (ficheros.length > 0) {
                referencia.innerText = `${titulo}: ${ficheros.length}`;
                referencia.classList.add(ltrCss.Espan.conContenido);
            }
            else {
                referencia.innerText = `${titulo}`;
                referencia.classList.remove(ltrCss.Espan.conContenido);
            }
        }
        let estaElModuloCargado = typeof Crud !== 'undefined';
        if (estaElModuloCargado && Definido(Crud.crudMnt) && Crud.crudMnt.EstoyEditando) {
            Crud.crudMnt.crudDeEdicion.DespuesDeAnexarMostrarArchivo(archivoDto);
        }
        MensajesSe.Info(`se ha anexado el archivo ${archivoDto.id}`);
    }

    export function MostrarArchivoAnexado(ctdArchivosAnexados: HTMLDivElement, negocio: string, idElemento: number, archivoDto: any): HTMLDivElement {
        const idArchivo: number = ObtenerPropiedad(archivoDto, ltrPropiedades.Elemento.Id);
        let parametros = `negocio=${negocio}`;
        parametros = `${parametros}&idElemento=${idElemento}`;
        parametros = `${parametros}&idArchivo=${idArchivo}`;

        let accion = Ajax.Archivos.accion.Descargar;
        if (typeof Crud !== 'undefined' && Crud.Consultor) {
            parametros = `${parametros}&guid=${Crud.Consultor.GuidDeConsulta}`;
            accion = Ajax.Archivos.accion.DescargarPorGuid;
        }
        else {
            parametros = `${parametros}&auditar=true`;
        }

        const descargar: string = `/${Ajax.Archivos.controlador}/${accion}?${parametros}`;

        const idCtdDelArchivo = `${ctdArchivosAnexados.id}-${idArchivo}`;
        const idVisorDelArchivo = `${atArchivo.visorArchivo}-${idArchivo}`;
        const idOpcionDescargar = `${ltrEventos.Archivo.Descargar}-${idArchivo}`

        const contenedor: [HTMLDivElement, HTMLImageElement] = ApiControl.CrearVisor(ctdArchivosAnexados
            , idVisorDelArchivo
            , idCtdDelArchivo
            , ltrCss.contenedorVisor
            , ltrCss.imagen100_100
            , archivoDto
            , () => ApiDeArchivos.MostrarModalDeCambioDeNombre(ctdArchivosAnexados, negocio, idElemento, idArchivo)
            , () => ApiDeArchivos.DescargarAnexado(idOpcionDescargar, descargar)
            , () => ApiDeArchivos.QuitarAnexado(ctdArchivosAnexados, idVisorDelArchivo, negocio, idElemento, idArchivo)
            , () => ApiDeArchivos.MostrarModalDeFirma(ctdArchivosAnexados, negocio, idElemento, idArchivo)
            , () => ApiDeArchivos.MostrarModalDeBloqueo(ctdArchivosAnexados, negocio, idElemento, idArchivo));
        contenedor[0].setAttribute(atArchivo.EsDeUnArchivador, ObtenerPropiedad(archivoDto, ltrPropiedades.SisDoc.Archivo.EsDeUnArchivadorVinculado));
        const idOriginal: number = Numero(ObtenerPropiedad(archivoDto, ltrPropiedades.SisDoc.Archivo.idOriginal));
        if (idOriginal > 0) {
            contenedor[0].setAttribute(atArchivo.Original, ObtenerPropiedad(archivoDto, ltrPropiedades.SisDoc.Archivo.Original));
            contenedor[0].setAttribute(atArchivo.IdOriginal, idOriginal.toString());
        }
        parametros = `${parametros}&ancho=${100}`;
        parametros = `${parametros}&alto=${100}`;
        //MapearAlControl.Imagen(contenedor[1], `/${Ajax.Archivos.controlador}/${Ajax.Archivos.accion.Thumsnail}?${parametros}`);
        archivosLeidos.push(archivoDto);
        return contenedor[0];
    }

    export function MostrarModalDeCambioDeNombre(ctdArchivosAnexados: HTMLDivElement, negocio: string, idElemento: number, idArchivo: number): void {

        let datosDeEntrada: Array<Parametro> = new Array<Parametro>();
        datosDeEntrada.push(new Parametro(ltrPropiedades.Negocio.nombre, negocio));
        datosDeEntrada.push(new Parametro(ltrPropiedades.Elemento.IdElemento, idElemento));

        let estaElModuloCargado = typeof SistemaDocumental !== 'undefined';
        let usandoJerarquiasDeCarpetas = estaElModuloCargado && Definido(SistemaDocumental.JerarquiaDeCarpetas);
        if (!usandoJerarquiasDeCarpetas && Definido(Crud.Consultor))
            return;

        var archivo = archivosLeidos.find(x => x.id === idArchivo);
        let nombre: string = ObtenerPropiedad(archivo, literal.nombre);
        let partes = nombre.split(ltrSimbolos.dosPuntosConEspacio);
        const nombreArchivo = partes.length > 1 ? partes[1] : partes[0];

        if (!usandoJerarquiasDeCarpetas && Crud.crudMnt.crudDeEdicion.VisorVisible && EsRenderizable(nombreArchivo)) {
            Crud.crudMnt.crudDeEdicion.RenderizarElSeleccionado(idArchivo, nombreArchivo);
            return;
        }

        ApiDePeticiones.LeerElementoPorId(ctdArchivosAnexados, ltrControladores.SisDoc.Archivos, idArchivo, new Array<Parametro>(), datosDeEntrada)
            .then((peticion: ApiDeAjax.DescriptorAjax) => ApiDeArchivos.MapearArchivo(peticion))
            .catch((peticion: ApiDeAjax.DescriptorAjax) => ApiDePeticiones.EmitirError(peticion));
    };

    export function MapearArchivo(peticion: ApiDeAjax.DescriptorAjax) {
        let ctdArchivosAnexados: HTMLDivElement = peticion.llamador as HTMLDivElement;
        let idModalDelVisor = `modal-${ctdArchivosAnexados.id}`;
        let ModalDeCambioDeNombre = ApiPanel.AbrirModalPorId(idModalDelVisor);
        ModalDeCambioDeNombre.setAttribute(atModal.NombreNegocio, peticion.DatosDeEntrada[0].valor);
        ModalDeCambioDeNombre.setAttribute(atModal.idElemento, peticion.DatosDeEntrada[1].valor);
        ModalDeCambioDeNombre.setAttribute(atModal.trasModificar, ltrEventos.Archivo.ModalDeCambioDeNombre.modificarNombreArchivo);

        let botonEliminar = document.getElementById(`boton-eliminar-${ObtenerPropiedad(peticion.resultado.datos, literal.id)}`);
        let modoDeAcceso = botonEliminar.style.display === ltrStyle.display.none
            ? ModoAcceso.enumModoDeAccesoDeDatos.Consultor
            : ObtenerPropiedad(peticion.resultado.datos, literal.ModoDeAcceso, ModoAcceso.enumModoDeAccesoDeDatos.Consultor);
        MapearAlPanel.ElObjeto(ModalDeCambioDeNombre, peticion.resultado.datos, modoDeAcceso, new Array<string>(), false);
        let usaBaja = ExistePropiedad(peticion.resultado.datos, ltrPropiedades.baja);
        let estaDeBaja = usaBaja ? ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.baja, false) : false;
        ModoAcceso.AplicarloAlPanel(ModalDeCambioDeNombre, modoDeAcceso, estaDeBaja);

        let caducaEl: HTMLInputElement = ApiControl.BuscarSelectorDeFechaHora(ModalDeCambioDeNombre, ltrPropiedades.SisDoc.Archivo.CaducaEl) as HTMLInputElement;
        ApiControl.DesbloquearSelectorDeFechaHora(caducaEl);
        const fechaIncrementada = new Date();
        fechaIncrementada.setHours(fechaIncrementada.getHours() + 1);
        ApiControl.AsignarFechaHora(caducaEl, fechaIncrementada);

    }

    function IdModalDeDatosDeFirma(ctdArchivosAnexados: HTMLDivElement): string {
        return `modal-${ctdArchivosAnexados.id}-datos-firma`;
    }

    function IdModalDeDatosDeBloqueo(ctdArchivosAnexados: HTMLDivElement): string {
        return `modal-${ctdArchivosAnexados.id}-datos-bloqueo`;
    }
    function IdModalDeDatosDeDesbloqueo(ctdArchivosAnexados: HTMLDivElement): string {
        return `modal-${ctdArchivosAnexados.id}-datos-desbloqueo`;
    }

    function IdModalDeFirma(ctdArchivosAnexados: HTMLDivElement): string {
        return `modal-${ctdArchivosAnexados.id}-firma`;
    }

    function MostrarModalDeDatosDeFirma(ctdArchivosAnexados: HTMLDivElement, negocio: string, idElemento: number, idArchivo: number): void {
        let datosDeEntrada: Array<Parametro> = new Array<Parametro>();
        datosDeEntrada.push(new Parametro(ltrPropiedades.Negocio.nombre, negocio));
        datosDeEntrada.push(new Parametro(ltrPropiedades.Elemento.IdElemento, idElemento));
        datosDeEntrada.push(new Parametro(ltrPropiedades.Elemento.IdArchivo, idArchivo));

        ApiDePeticiones.LeerDatosDeFirma(ctdArchivosAnexados, negocio, idElemento, idArchivo, datosDeEntrada)
            .then((peticion: ApiDeAjax.DescriptorAjax) => ApiDeArchivos.MostrarDatosDeFirma(peticion))
            .catch((peticion: ApiDeAjax.DescriptorAjax) => ApiDePeticiones.EmitirError(peticion));
    };

    export function MostrarDatosDeFirma(peticion: ApiDeAjax.DescriptorAjax) {
        let ctdArchivosAnexados: HTMLDivElement = peticion.llamador as HTMLDivElement;
        let modalDeDatosDeFirma = ApiPanel.AbrirModalPorId(IdModalDeDatosDeFirma(ctdArchivosAnexados));

        modalDeDatosDeFirma.setAttribute(atModal.NombreNegocio, peticion.DatosDeEntrada[0].valor);
        modalDeDatosDeFirma.setAttribute(atModal.idElemento, peticion.DatosDeEntrada[1].valor);
        modalDeDatosDeFirma.setAttribute(atArchivo.idArchivo, peticion.DatosDeEntrada[2].valor);
        modalDeDatosDeFirma.setAttribute(literal.ModoDeAcceso, peticion.resultado.modoDeAcceso);

        let modoDeAcceso = ModoAcceso.Parsear(peticion.resultado.modoDeAcceso);
        MapearAlPanel.ElObjeto(modalDeDatosDeFirma, peticion.resultado.datos, modoDeAcceso);
        ModoAcceso.AplicarloAlPanel(modalDeDatosDeFirma, modoDeAcceso, false);
    }

    export function MostrarModalDeFirma(ctdArchivosAnexados: HTMLDivElement, negocio: string, idElemento: number, idArchivo: number): void {

        let visorDelArchivo: HTMLDivElement = document.getElementById(`${atArchivo.visorArchivo}-${idArchivo}`) as HTMLDivElement;
        if (EsTrue(visorDelArchivo.getAttribute(atArchivo.firmado))) {
            MostrarModalDeDatosDeFirma(ctdArchivosAnexados, negocio, idElemento, idArchivo);
        }
        else {
            let datosDeEntrada: Array<Parametro> = new Array<Parametro>();
            datosDeEntrada.push(new Parametro(ltrPropiedades.Negocio.nombre, negocio));
            datosDeEntrada.push(new Parametro(ltrPropiedades.Elemento.IdElemento, idElemento));
            datosDeEntrada.push(new Parametro(ltrPropiedades.Elemento.IdArchivo, idArchivo));

            ApiDePeticiones.LeerCertificadosParaFirmarElArchivo(ctdArchivosAnexados, negocio, idElemento, idArchivo, datosDeEntrada)
                .then((peticion: ApiDeAjax.DescriptorAjax) => ApiDeArchivos.PrepararParaFirmar(peticion))
                .catch((peticion: ApiDeAjax.DescriptorAjax) => ApiDePeticiones.EmitirError(peticion));
        }
    };

    export function MostrarModalDeBloqueo(ctdArchivosAnexados: HTMLDivElement, negocio: string, idElemento: number, idArchivo: number): void {

        let visorDelArchivo: HTMLDivElement = document.getElementById(`${atArchivo.visorArchivo}-${idArchivo}`) as HTMLDivElement;


        let modal = EsTrue(visorDelArchivo.getAttribute(atArchivo.bloqueado))
            ? ApiPanel.AbrirModalPorId(IdModalDeDatosDeDesbloqueo(ctdArchivosAnexados))
            : ApiPanel.AbrirModalPorId(IdModalDeDatosDeBloqueo(ctdArchivosAnexados))

        let expresion: string = undefined;
        let estaElModuloCargado = typeof SistemaDocumental !== 'undefined';
        let usandoJerarquiasDeCarpetas = estaElModuloCargado && Definido(SistemaDocumental.JerarquiaDeCarpetas);
        var archivo = archivosLeidos.find(x => x.id === idArchivo);

        if (usandoJerarquiasDeCarpetas) {
            expresion = ApiControl.BuscarListaDinamicaPorPropiedad(SistemaDocumental.JerarquiaDeCarpetas.PanelDelDto, ltrPropiedades.SisDoc.Carpeta.Archivador).value + ": " +
                ApiControl.BuscarEditor(SistemaDocumental.JerarquiaDeCarpetas.PanelDelDto, ltrPropiedades.SisDoc.Carpeta.Nombre).value;
        }
        else {
            expresion = ObtenerPropiedad(Crud.crudMnt.crudDeEdicion.Registro, ltrPropiedades.Elemento.Expresion);
        }
        MapearAlControl.Restrictor(ApiControl.BuscarRestrictor(modal, ltrPropiedades.SisDoc.Bloqueo.idArchivo, ltrTipoControl.restrictorDeEdicion), idArchivo, expresion);
        let controlAuditoria = ApiControl.BuscarAreaDeTexto(modal, ltrPropiedades.SisDoc.Bloqueo.Auditoria) as HTMLTextAreaElement;
        MapearAlControl.MapearAreaDeTexto(controlAuditoria, Definido(archivo) ? ObtenerPropiedad(archivo, ltrPropiedades.SisDoc.Archivo.Auditoria, '') : '', true);
        ApiControl.BuscarEditor(modal, ltrPropiedades.SisDoc.Bloqueo.Motivo).focus();
    }

    export function PrepararParaFirmar(peticion: ApiDeAjax.DescriptorAjax) {
        let ctdArchivosAnexados: HTMLDivElement = peticion.llamador as HTMLDivElement;
        let modalDeFirma = ApiPanel.AbrirModalPorId(IdModalDeFirma(ctdArchivosAnexados));

        modalDeFirma.setAttribute(atModal.NombreNegocio, peticion.DatosDeEntrada[0].valor);
        modalDeFirma.setAttribute(atModal.idElemento, peticion.DatosDeEntrada[1].valor);
        modalDeFirma.setAttribute(atArchivo.idArchivo, peticion.DatosDeEntrada[2].valor);
        modalDeFirma.setAttribute(literal.ModoDeAcceso, peticion.resultado.modoDeAcceso);

        let modoDeAcceso = ModoAcceso.Parsear(peticion.resultado.modoDeAcceso);
        ModoAcceso.AplicarloAlPanel(modalDeFirma, modoDeAcceso, false);
        var lista: HTMLSelectElement = modalDeFirma.querySelector('select') as HTMLSelectElement;
        let objetoLista = new Tipos.ListaDeElemento(lista.id);
        MapearAlControl.MapearElementosEnListaDeElementos(objetoLista, peticion.resultado.datos);
    }

    export function FirmarArchivo(idModalDeFirma: string) {
        let modal: HTMLDivElement = document.getElementById(idModalDeFirma) as HTMLDivElement;
        let idnegocio: number = Numero(modal.getAttribute(atModal.idNegocio));
        let idElemento: number = Numero(modal.getAttribute(atModal.idElemento));
        let idArchivo: number = Numero(modal.getAttribute(atArchivo.idArchivo));
        let Certificado: HTMLSelectElement = ApiControl.BuscarListaDeElementos(modal, ltrPropiedades.Certificado.propiedad);
        var idCertificado = Numero(Certificado.selectedOptions[0].value);
        let modoDeAcceso: string = modal.getAttribute(literal.ModoDeAcceso);

        let password = modal.querySelector(`input[propiedad='${Ajax.Param.password}']`) as HTMLInputElement;

        ApiDePeticiones.FirmarArchivo(modal, idnegocio, idElemento, idArchivo, idCertificado, modoDeAcceso, password.value)
            .then((peticion) => DespuesDeFirmarArchivo(peticion))
            .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
    };

    export function DespuesDeFirmarArchivo(peticion: ApiDeAjax.DescriptorAjax): any {
        let modal: HTMLDivElement = peticion.llamador as HTMLDivElement;
        // `modal-${ctdArchivosAnexados.id}-firma`;
        let idContenedor: string = modal.id.replace(ltrPrefijo.Modal, '').replace(ltrPosfijo.Firma, '');
        let contenedor: HTMLDivElement = document.getElementById(idContenedor) as HTMLDivElement;
        contenedor.innerHTML = '';
        let negocio: string = modal.getAttribute(atModal.NombreNegocio);
        let idNegocio: number = Numero(ObtenerPropiedad(peticion.DatosDeEntrada, ltrPropiedades.Negocio.idNegocio));
        let idElemento: number = Numero(ObtenerPropiedad(peticion.DatosDeEntrada, ltrPropiedades.Elemento.IdElemento));
        let gridDeTrazas: HTMLDivElement = document.getElementById(ltrGridDeUnExpansor.Trazas) as HTMLDivElement;
        LeerAnexados(contenedor, negocio, idElemento, 0, Numero(Ajax.Archivos.parametro.Cantidad));
        if (Definido(gridDeTrazas)) MapearAlGrid.MapearGridDeDetalle(gridDeTrazas, idNegocio, idElemento, null);
        ApiPanel.CerrarModal(modal);
    }

    export function AnularFirma(idModalDeFirma: string): void {
        let modal: HTMLDivElement = document.getElementById(idModalDeFirma) as HTMLDivElement;
        let idnegocio: number = Numero(modal.getAttribute(atModal.idNegocio));
        let idElemento: number = Numero(modal.getAttribute(atModal.idElemento));
        let idArchivo: number = Numero(modal.getAttribute(atArchivo.idArchivo));

        ApiDePeticiones.AnularFirma(modal, idnegocio, idElemento, idArchivo)
            .then((peticion) => ApiDeArchivos.DespuesDeAnularFirma(peticion))
            .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
    };

    export function DespuesDeAnularFirma(peticion: ApiDeAjax.DescriptorAjax): any {
        let modal: HTMLDivElement = peticion.llamador as HTMLDivElement;
        // `modal-${ctdArchivosAnexados.id}-datos-firma`;
        let idContenedor: string = modal.id.replace('modal-', '').replace('-datos-firma', '');
        let contenedor: HTMLDivElement = document.getElementById(idContenedor) as HTMLDivElement;
        contenedor.innerHTML = '';
        let negocio: string = modal.getAttribute(atModal.NombreNegocio);
        let idNegocio: number = Numero(ObtenerPropiedad(peticion.DatosDeEntrada, ltrPropiedades.Negocio.idNegocio));
        let idElemento: number = Numero(ObtenerPropiedad(peticion.DatosDeEntrada, ltrPropiedades.Elemento.IdElemento));
        let gridDeTrazas: HTMLDivElement = document.getElementById(ltrGridDeUnExpansor.Trazas) as HTMLDivElement;
        LeerAnexados(contenedor, negocio, idElemento, 0, Numero(Ajax.Archivos.parametro.Cantidad));
        if (Definido(gridDeTrazas)) MapearAlGrid.MapearGridDeDetalle(gridDeTrazas, idNegocio, idElemento, null);
        ApiPanel.CerrarModal(modal);
    }

    export function DescargarAnexado(idDescargar: string, descargar: string): void {
        try {
            DescargarArchivo(idDescargar, descargar);
        }
        finally {
            MensajesSe.Info("Descarga realizada")
        }
    };

    export function QuitarAnexado(ctdArchivosAnexados: HTMLDivElement, idVisorDelArchivo: string, negocio: string, idElemento: number, idArchivo: number): void {
        ApiDePeticiones.QuitarAnexado(this, ctdArchivosAnexados, idVisorDelArchivo, negocio, idElemento, idArchivo)
            .then((peticion) => ApiDeArchivos.DespuesDeQuitarAnexado(peticion))
            .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
    };

    export function DespuesDeQuitarAnexado(peticion: ApiDeAjax.DescriptorAjax): any {
        let ctdDelArchivo: HTMLDivElement = document.getElementById(peticion.DatosDeEntrada.idVisorDelArchivo) as HTMLDivElement;
        let referencia = ctdDelArchivo.parentElement.parentElement.parentElement.parentElement.querySelector(`a[${atControl.class}*=${ltrCss.Espan.cssExpansor}]`) as HTMLAnchorElement;

        if (Definido(referencia)) {
            let contenedor = ctdDelArchivo.parentElement;
            let ficheros = contenedor.querySelectorAll(`div[${atControl.class}=${ltrCss.Espan.cssVisorArchivos}]`) as NodeListOf<HTMLDivElement>;
            let titulo = referencia.getAttribute(ltrEspanes.Atributos.titulo);
            if (ficheros.length > 1) {
                referencia.innerText = `${titulo}: ${ficheros.length - 1}`;
                referencia.classList.add(ltrCss.Espan.conContenido);
            }
            else {
                referencia.innerText = `${titulo}`;
                referencia.classList.remove(ltrCss.Espan.conContenido);
            }
        }
        ctdDelArchivo.remove();
        let estaElModuloCargado = typeof Crud !== 'undefined';
        if (estaElModuloCargado && Definido(Crud.crudMnt)) {
            Crud.crudMnt.crudDeEdicion.RecargarGridDeTrazas();
            Crud.crudMnt.crudDeEdicion.SiguienteArchivo();
        }
    }

    export function DescargarArchivo(idDescargar: string, url: string): any {
        if (NoDefinido(idDescargar)) return;
        let boton = document.getElementById(idDescargar) as HTMLButtonElement;
        //boton.setAttribute('url', url);
        //window.open(url, '_blank', 'noopener,noreferrer');
        EntornoSe.AbrirPestana(url, false);
        return;
    }

    function DespuesDeGenerarZip(peticion: ApiDeAjax.DescriptorAjax): any {
        let modal: HTMLDivElement = peticion.llamador as HTMLDivElement;
        ApiPanel.CerrarModal(modal);
    }

    function DespuesDeBloquearArchivos(peticion: ApiDeAjax.DescriptorAjax): any {
        let modal: HTMLDivElement = peticion.llamador as HTMLDivElement;
        RecargarArchivosAnexadosCerrarModal(modal);
    }

    function DespuesDeDesbloquearArchivos(peticion: ApiDeAjax.DescriptorAjax): any {
        let modal: HTMLDivElement = peticion.llamador as HTMLDivElement;
        RecargarArchivosAnexadosCerrarModal(modal);
    }

    function DespuesDeBloquearArchivo(peticion: any) {
        let modal: HTMLDivElement = peticion.llamador as HTMLDivElement;
        RecargarArchivosAnexadosCerrarModal(modal);
    }

    function RecargarArchivosAnexadosCerrarModal(modal: HTMLDivElement) {
        let estaElModuloCargado = typeof SistemaDocumental !== 'undefined';
        if (estaElModuloCargado && Definido(SistemaDocumental.JerarquiaDeCarpetas))
            ApiDeArchivos.MostrarArchivosAnexados(SistemaDocumental.ltrCarpetas.archivos.contenedorDeArchivos, SistemaDocumental.JerarquiaDeCarpetas.Negocio, Numero(ApiControl.BuscarEditor(SistemaDocumental.JerarquiaDeCarpetas.ContenedorDelId, 'id').value));
        else
            ApiDeArchivos.MostrarArchivosAnexados(Crud.crudMnt.crudDeEdicion.PanelDeArchivos.id, Crud.crudMnt.NombreDeNegocio, Crud.crudMnt.crudDeEdicion.Registro.id);
        ApiPanel.CerrarModal(modal);
    }

    function AplicarOperacionALosArchivos(idModal: string, operacion: string, archivos: Array<HTMLInputElement>) {
        let modal: HTMLDivElement = document.getElementById(idModal) as HTMLDivElement;
        let enumNegocioDestino = enumNegocio.No_Definido;
        let idDestino = Numero(ApiControl.BuscarListaDinamicaPorPropiedad(modal, ltrPropiedades.SisDoc.Archivo.Destino).getAttribute(atListasDinamicas.idSeleccionado));
        if (operacion != atArchivo.Operacion.Mover.toLocaleLowerCase() && idDestino === 0) {
            MensajesSe.Info('Debe indicar el elemento al que ' + operacion.toLowerCase());
            return;
        }

        if (operacion === atArchivo.Operacion.Mover.toLocaleLowerCase() && idDestino === 0) {
            idDestino = Numero(ApiControl.BuscarListaDeElementos(modal, ltrPropiedades.SisDoc.Archivo.ArchivadorDestino).value);
            if (idDestino === 0) {
                MensajesSe.Info('Debe indicar el elemento o archivador al que ' + operacion.toLowerCase());
                return;
            }
            enumNegocioDestino = enumNegocio.Archivador;
        }

        let { idnegocio, idOrigen }: { idnegocio: number; idOrigen: number; } = ObtenerNegocioYElElemento(modal);

        let parametros = new Array<Parametro>();
        let idsDeArchivos: Array<number> = ObtenerArchivosSeleccionados(archivos);
        parametros.push(new Parametro(ltrPropiedades.SisDoc.Archivo.IdsDeArchivos, idsDeArchivos));

        ApiDePeticiones.ProcesarArchivos(modal, operacion, idnegocio, idOrigen, idDestino, enumNegocioDestino, parametros)
            .then((peticion) => ApiDeArchivos.DespuesDeProcesarArchivos(peticion))
            .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
    }

    function ObtenerArchivosSeleccionados(archivos: HTMLInputElement[]): Array<number> {
        let idsDeArchivos = new Array<number>();
        for (let i: number = 0; i < archivos.length; i++) {
            idsDeArchivos.push(Numero(archivos[i].id.replace('check-', '')));
        }
        return idsDeArchivos;
    }

    function ObtenerNegocioYElElemento(modal: HTMLDivElement) {
        let estaElModuloCargado = typeof SistemaDocumental !== 'undefined';
        let idnegocio: number = 0;
        let idOrigen: number = 0;
        if (estaElModuloCargado && Definido(SistemaDocumental.JerarquiaDeCarpetas)) {
            idnegocio = SistemaDocumental.JerarquiaDeCarpetas.IdNegocio;
            idOrigen = Numero(ApiControl.BuscarEditor(SistemaDocumental.JerarquiaDeCarpetas.ContenedorDelId, 'id').value);
        }
        else {
            idOrigen = Numero(ObtenerPropiedad(Crud.crudMnt.crudDeEdicion.Registro, ltrPropiedades.Elemento.Id));
            let cabecera: HTMLDivElement = modal.parentElement.querySelector('.' + ltrCss.crud.cabecera);
            idnegocio = Numero(cabecera.getAttribute(literal.idNegocio));
        }
        return { idnegocio, idOrigen };
    }

    function ObtenerNegocioElemento() {
        let estaElModuloCargado = typeof SistemaDocumental !== 'undefined';
        let idNegocio: number = 0;
        let idElemento: number = 0;
        if (estaElModuloCargado && Definido(SistemaDocumental.JerarquiaDeCarpetas)) {
            idNegocio = SistemaDocumental.JerarquiaDeCarpetas.IdNegocio;
            idElemento = Numero(ApiControl.BuscarEditor(SistemaDocumental.JerarquiaDeCarpetas.ContenedorDelId, 'id').value);
        }
        else {
            idNegocio = Crud.crudMnt.IdNegocio
            idElemento = Numero(ObtenerPropiedad(Crud.crudMnt.crudDeEdicion.Registro, ltrPropiedades.Elemento.Id));
        }
        return { idNegocio, idElemento };
    }

    export function DespuesDeProcesarArchivos(peticion: ApiDeAjax.DescriptorAjax): any {
        let modal: HTMLDivElement = peticion.llamador as HTMLDivElement;
        let operacion = ObtenerPropiedad(peticion.DatosDeEntrada, Ajax.Param.operacion);
        if (operacion.toLocaleLowerCase() === atArchivo.Operacion.Mover.toLocaleLowerCase()) {
            RecargarMostrarArchivosAnexados();
        }
        ApiPanel.CerrarModal(modal);
    }
    let recargando = false;
    export function RecargarMostrarArchivosAnexados() {
        if (recargando)
            return;
        try {
            recargando = true;
            let estaElModuloCargado = typeof SistemaDocumental !== 'undefined';
            if (estaElModuloCargado && Definido(SistemaDocumental.JerarquiaDeCarpetas))
                ApiDeArchivos.MostrarArchivosAnexados(SistemaDocumental.ltrCarpetas.archivos.contenedorDeArchivos, SistemaDocumental.JerarquiaDeCarpetas.Negocio, Numero(ApiControl.BuscarEditor(SistemaDocumental.JerarquiaDeCarpetas.ContenedorDelId, 'id').value));
            else {
                if (!Definido(Crud.crudMnt.crudDeEdicion.Registro))
                    return;
                ApiDeArchivos.MostrarArchivosAnexados(Crud.crudMnt.crudDeEdicion.PanelDeArchivos.id, Crud.crudMnt.NombreDeNegocio, Crud.crudMnt.crudDeEdicion.Registro.id);
                if (Crud.crudMnt.NombreDeNegocio !== enumNegocio.Archivador)
                    Crud.crudMnt.crudDeEdicion.RecargarGridDeArchivadores()
            }
        }
        finally {
            recargando = false
        }
    }

    export async function GuardarDisposicionDeArchivos(columnas: number) {
        let idVista: number = 0;
        let controlador: string = ltrControladores.Comunes.Base;
        let estaElModuloCargado = typeof SistemaDocumental !== 'undefined';
        if (estaElModuloCargado && Definido(SistemaDocumental.JerarquiaDeCarpetas)) {
            idVista = SistemaDocumental.JerarquiaDeCarpetas.IdVista;
            controlador = SistemaDocumental.JerarquiaDeCarpetas.Controlador;
        }
        else {

            if (Definido(Crud.Consultor))
                return;

            idVista = Crud.crudMnt.IdVista;
            controlador = Crud.crudMnt.Controlador;
        }


        let parametros: Array<Parametro> = new Array<Parametro>();
        parametros.push(new Parametro(Ajax.Param.datosPeticion, columnas));

        const params2 = {
            [Ajax.Param.idNegocio]: Encriptar(literal.ClaveDeEncriptacion, 0),
            [Ajax.Param.idVista]: Encriptar(literal.ClaveDeEncriptacion, idVista),
            [Ajax.Param.peticion]: Encriptar(literal.ClaveDeEncriptacion, ltrMenus.eventosDeMf.Comun.GuardarDisposicionDeArchivos)
        };
        const url2 = `/${controlador}/${Ajax.EndPoint.ProcesarPeticion}?${new URLSearchParams(params2)}`;
        await fetch(url2, {
            method: 'POST',
            body: JSON.stringify(parametros),
            keepalive: true
        });
    }

    export async function AplicarDisposicionDeArchivos(contenedor: HTMLDivElement) {
        let idVista: number = 0;
        let controlador: string = ltrControladores.Comunes.Base;

        // Determinar idVista y controlador
        if (typeof SistemaDocumental !== 'undefined' && Definido(SistemaDocumental.JerarquiaDeCarpetas)) {
            idVista = SistemaDocumental.JerarquiaDeCarpetas.IdVista;
            controlador = SistemaDocumental.JerarquiaDeCarpetas.Controlador;
        } else {
            idVista = Crud.crudMnt.IdVista;
            controlador = Crud.crudMnt.Controlador;
        }

        // Preparar parámetros para la petición
        const params = {
            [Ajax.Param.idNegocio]: Encriptar(literal.ClaveDeEncriptacion, 0),
            [Ajax.Param.idVista]: Encriptar(literal.ClaveDeEncriptacion, idVista),
            [Ajax.Param.peticion]: Encriptar(literal.ClaveDeEncriptacion, ltrMenus.eventosDeMf.Comun.LeerDisposicionDeArchivos)
        };

        // Construir la URL
        const url = `/${controlador}/${Ajax.EndPoint.ProcesarPeticion}?${new URLSearchParams(params)}`;

        try {
            // Realizar la petición
            const response = await fetch(url, {
                method: 'POST',
                keepalive: true
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            // Leer y parsear la respuesta
            const data = await response.json();

            // Asumiendo que el servidor devuelve un objeto con una propiedad que contiene el valor entero
            // Ajusta 'valorColumnas' según la estructura real de tu respuesta
            const valorColumnas = data.datos;
            AplicarEncolumnado(valorColumnas, contenedor);

        } catch (error) {
            console.error('Error al leer la disposición de archivos:', error);
        }
    }

    export function PegarPortaPapeles(eventOrNull, controlador: string, idCanvas: string, idImagen: string, idInputNombreArchivo: string, idInputIdArchivo: string) {
        // Función para procesar la imagen


        function procesarImagen(blob) {
            // Crear un objeto File a partir del Blob
            var file = new File([blob], "imagen_pegada.png", { type: "image/png" });

            // Crear un FileList simulado
            var dataTransfer = new DataTransfer();
            dataTransfer.items.add(file);

            // Asignar el FileList al input de archivo
            var inputArchivo = document.getElementById(idInputIdArchivo) as HTMLInputElement;
            inputArchivo.files = dataTransfer.files;

            // Actualizar el nombre del archivo
            (document.getElementById(idInputNombreArchivo) as HTMLInputElement).value = "imagen_pegada.png";

            // Llamar a MostrarCanvas para mostrar y subir el archivo
            ApiDeArchivos.MostrarCanvas(controlador, idInputIdArchivo, idCanvas);
        }

        function crearFicheroPlano(texto: string) {
            // Crear un objeto Blob a partir del texto
            var blob = new Blob([texto], { type: "text/plain" });

            // Crear un objeto File a partir del Blob
            var file = new File([blob], "fichero_plano.txt", { type: "text/plain" });

            // Crear un FileList simulado
            var dataTransfer = new DataTransfer();
            dataTransfer.items.add(file);

            // Asignar el FileList al input de archivo
            var inputArchivo = document.getElementById(idInputIdArchivo) as HTMLInputElement;
            inputArchivo.files = dataTransfer.files;

            // Actualizar el nombre del archivo
            (document.getElementById(idInputNombreArchivo) as HTMLInputElement).value = "fichero_plano.txt";

            // Aquí puedes llamar a una función para procesar el archivo de texto si es necesario,
            // similar a cómo se hace con la imagen. Por ejemplo:
            CambiarEstado(inputArchivo, atArchivo.situacion.subiendo);
            ApiDePeticiones.SubirPorTrozos(controlador, inputArchivo.files, idInputIdArchivo);
        }

        var inputArchivo = document.getElementById(idInputIdArchivo) as HTMLInputElement;

        let estaElModuloCargado = typeof Crud !== 'undefined';
        if (estaElModuloCargado && Definido(Crud.crudMnt) && Crud.crudMnt.EstoyEnMantenimiento && Crud.crudMnt.EstaVisualizandoUnSeleccionado) {
            if (ApiControl.EstaContenidoEn(Crud.crudMnt.ContenedorDeGraficos, inputArchivo)) {
                MensajesSe.Info("Para subir un archivo, edite el registro seleccionado");
                return;
            }
        }

        // Si es un evento de pegado (Ctrl+V)
        if (eventOrNull && eventOrNull.clipboardData) {
            var items = eventOrNull.clipboardData.items;

            for (var i = 0; i < items.length; i++) {
                if (items[i].type.indexOf("image") !== -1) {
                    var blob = items[i].getAsFile();
                    procesarImagen(blob);
                    return;
                }
            }
            MensajesSe.Info("Del contenido en el porta papeles nada es una imagen");
        }
        // Si es un clic en el botón
        else {
            navigator.clipboard.read()
                .then(clipboardItems => {
                    for (const clipboardItem of clipboardItems) {
                        if (clipboardItem.types.includes('image/png') || clipboardItem.types.includes('image/jpeg')) {
                            clipboardItem.getType('image/png')
                                .then(blob => {
                                    procesarImagen(blob);
                                })
                                .catch(error => {
                                    MensajesSe.Error("PegarPortaPapeles", "No se pudo obtener la imagen del portapapeles");
                                });
                            return;
                        }
                        else if (clipboardItem.types.includes('text/plain')) {
                            clipboardItem.getType('text/plain')
                                .then(blob => {
                                    const reader = new FileReader();
                                    reader.onload = () => {
                                        crearFicheroPlano(reader.result as string);
                                    };
                                    reader.readAsText(blob);
                                })
                                .catch(error => { MensajesSe.Error("PegarPortaPapeles", "No se pudo obtener el texto del portapapeles"); });
                            return;
                        }
                    }
                    MensajesSe.Info("No hay imagen ni texto plano en el portapapeles");

                })
                .catch(error => {
                    MensajesSe.Error("PegarPortaPapeles", "No se pudo acceder al portapapeles");
                });
        }
    }

    function SubirArchivo(controlador: string, idArchivo: string): Promise<string> {

        return new Promise((resolve, reject) => {

            let archivo: HTMLInputElement = document.getElementById(idArchivo) as HTMLInputElement;
            let ficheros = archivo.files;

            let url: string = `/${controlador}/${Ajax.Archivos.accion.SubirArchivo}`;

            let a = new ApiDeAjax.DescriptorAjax(this
                , Ajax.Archivos.accion.SubirArchivo
                , new ApiDePeticiones.DatosPeticionSubirArchivo(idArchivo, archivo.files[0].name)
                , url
                , ApiDeAjax.TipoPeticion.Asincrona
                , ApiDeAjax.ModoPeticion.Post
                , (peticion) => {
                    TrasSubirElArchivo(peticion);
                    resolve(`el archivo ${idArchivo} ha subido`);
                }
                , (peticion) => {
                    SiHayErrorAlSubirElArchivo(peticion);
                    let etiqueta: HTMLElement = document.getElementById(`${idArchivo}.ref`);
                    reject(`el archivo '${etiqueta !== null && etiqueta !== undefined ? etiqueta.innerText : idArchivo}' no se ha podido subir, el trabajo no será sometido`);
                }
            );

            let datosPost = new FormData();
            datosPost.append(Ajax.Param.fichero, ficheros[0]);

            let rutaDestino: string = archivo.getAttribute(atArchivo.rutaDestino);
            datosPost.append(Ajax.Param.rutaDestino, IsNullOrEmpty(rutaDestino) ? '' : rutaDestino);

            let extensionesValidas: string = archivo.getAttribute(atArchivo.extensionesValidas);
            datosPost.append(Ajax.Param.extensiones, extensionesValidas);
            a.DatosPost = datosPost;
            ApiDePeticiones.DefinirBarraDeProceso(a, archivo);
            CambiarEstado(archivo, atArchivo.situacion.subiendo);
            a.Ejecutar();
        });
    }

    function CambiarEstado(archivo: HTMLInputElement, situacion: string) {
        let idInfoArchivo: string = archivo.getAttribute(atArchivo.infoArchivo);
        let infoArchivoHtml: HTMLInputElement = (NoDefinido(idInfoArchivo)) ? archivo : document.getElementById(idInfoArchivo) as HTMLInputElement;
        infoArchivoHtml.setAttribute(atArchivo.estado, situacion);
    }

    function TrasSubirElArchivo(peticion: ApiDeAjax.DescriptorAjax) {
        let datos: ApiDePeticiones.DatosPeticionSubirArchivo = peticion.DatosDeEntrada;
        let archivo: HTMLInputElement = datos.Archivo();
        MapearResultadosTrasSubirElArchivo(archivo, datos.NombreArchivo, peticion.resultado.datos);
    }


    export function MapearResultadosTrasSubirElArchivo(archivo: HTMLInputElement, nombreArchivo: string, idArchivo_Url: string) {
        CambiarEstado(archivo, atArchivo.situacion.subido);
        BlanquearArchivo(archivo, false);
        OcultarArchivo(archivo, false);
        //MostrarCanvas
        VisualizarBarraDeOk(archivo, nombreArchivo);
        let tipo: string = archivo.getAttribute(atControl.tipo);
        if (tipo === ltrTipoControl.Archivo || tipo === ltrTipoControl.SelectorDeUnArchivo)
            archivo.setAttribute(atArchivo.idArchivo, idArchivo_Url);

        if (tipo === ltrTipoControl.UrlDeArchivo)
            archivo.setAttribute(atArchivo.nombre, idArchivo_Url);
    }

    function SiHayErrorAlSubirElArchivo(peticion: ApiDeAjax.DescriptorAjax) {
        let datos: ApiDePeticiones.DatosPeticionSubirArchivo = peticion.DatosDeEntrada;
        let archivo: HTMLInputElement = datos.Archivo();
        ApiDeAjax.ErrorTrasPeticion("Subir archivo", peticion);
        CambiarEstado(archivo, atArchivo.situacion.error);
        BlanquearArchivo(archivo, true);
        VisualizarBarraDeError(archivo);

        //Mensaje(MensajesSe.enumTipoMensaje.error, peticion.resultado.mensaje);
    }

    function BlanquearImagen(archivo: HTMLInputElement): void {
        let canvasHtml: HTMLCanvasElement = document.getElementById(archivo.getAttribute(atArchivo.canvas)) as HTMLCanvasElement;
        if (canvasHtml) {
            canvasHtml.width = canvasHtml.width;
            let imagenHtml: HTMLImageElement = document.getElementById(archivo.getAttribute(atArchivo.canvas)) as HTMLImageElement;
            imagenHtml.src = "";
        }
    }

    function OcultarImagen(archivo: HTMLInputElement, ocultar: boolean): void {
        let canvasHtml: HTMLCanvasElement = document.getElementById(archivo.getAttribute(atArchivo.canvas)) as HTMLCanvasElement;
        canvasHtml.style.display = ocultar ? ltrStyle.display.none : ltrStyle.display.block;
    }

    function OcultarBarra(archivo: HTMLInputElement): void {
        let barraHtml: HTMLDivElement = document.getElementById(archivo.getAttribute(atArchivo.barra)) as HTMLDivElement;
        barraHtml.classList.remove(ltrCss.barraRoja);
        barraHtml.classList.remove(ltrCss.barraAzul);
        barraHtml.classList.remove(ltrCss.barraVerde);
        ApiControl.HacerVisibleLaBarra(barraHtml, false);
    }

    function VisualizarBarraDeOk(archivo: HTMLInputElement, nombreArchivo: string): void {
        let barraHtml: HTMLDivElement = document.getElementById(archivo.getAttribute(atArchivo.barra)) as HTMLDivElement;
        barraHtml.classList.remove(ltrCss.barraRoja);
        barraHtml.classList.remove(ltrCss.barraAzul);
        barraHtml.classList.add(ltrCss.barraVerde);
        let span: Element = barraHtml.children[0];
        if (!IsNullOrEmpty(nombreArchivo))
            span.innerHTML = SanitizeHTML(nombreArchivo);
        else
            span.innerHTML = "Proceso completado";
        ApiControl.HacerVisibleLaBarra(barraHtml, true);
        let infoArchivo: HTMLInputElement = document.getElementById(archivo.getAttribute(atArchivo.infoArchivo)) as HTMLInputElement;
        infoArchivo.style.display = ltrStyle.display.none;
    }

    function VisualizarBarraDeError(archivo: HTMLInputElement): void {
        let barraHtml: HTMLDivElement = document.getElementById(archivo.getAttribute(atArchivo.barra)) as HTMLDivElement;
        barraHtml.classList.remove(ltrCss.barraVerde);
        barraHtml.classList.remove(ltrCss.barraAzul);
        barraHtml.classList.add(ltrCss.barraRoja);
        let span: Element = barraHtml.children[0];
        span.innerHTML = "Error al subir el fichero";
        ApiControl.HacerVisibleLaBarra(barraHtml, true);
        let infoArchivo: HTMLInputElement = document.getElementById(archivo.getAttribute(atArchivo.infoArchivo)) as HTMLInputElement;
        infoArchivo.style.display = ltrStyle.display.none;
    }

    function InicializarBarra(archivo: HTMLInputElement): void {
        let barraHtml: HTMLDivElement = document.getElementById(archivo.getAttribute(atArchivo.barra)) as HTMLDivElement;
        ApiControl.HacerVisibleLaBarra(barraHtml, false);
        barraHtml.classList.remove(ltrCss.barraVerde);
        barraHtml.classList.remove(ltrCss.barraRoja);
        barraHtml.classList.add(ltrCss.barraAzul);
    }

    function BlanquearInfoArchivo(archivo: HTMLInputElement): void {
        let idInfoArchivo: string = archivo.getAttribute(atArchivo.infoArchivo);
        let infoArchivoHtml: HTMLInputElement = document.getElementById(idInfoArchivo) as HTMLInputElement;
        infoArchivoHtml.value = "";
        if (!EsImagen(archivo))
            infoArchivoHtml.style.display = ltrStyle.display.block;
    }

    function EsImagen(archivo: HTMLInputElement): boolean {
        return archivo.getAttribute(atArchivo.canvas) !== null;
    }


    function AplicarEncolumnado(columnas: number, contenedorDeArchivos: HTMLDivElement) {

        if (EsDispositvoMovil)
            columnas = 1;

        contenedorDeArchivos.classList.value = ltrCss.Archivos.ContenedorDeArchivo;
        if (columnas <= 0 || columnas >= 5)
            columnas = 5;
        else if (columnas === 1) {
            contenedorDeArchivos.classList.add(ltrCss.Archivos.UnaColumna);
        } else if (columnas === 2) {
            contenedorDeArchivos.classList.add(ltrCss.Archivos.DosColumnas);
        } else if (columnas === 3) {
            contenedorDeArchivos.classList.add(ltrCss.Archivos.TresColumnas);
        } else {
            contenedorDeArchivos.classList.add(ltrCss.Archivos.CuatroColumnas);
        }
        var ref = document.getElementById('ref-cambiar-encolumnado') as HTMLAnchorElement;
        var partes = ref.innerHTML.split(':');
        ref.innerHTML = partes[0] + ': ' + columnas.toString();
        contenedorDeArchivos.setAttribute('columnas', columnas.toString());
        return columnas;
    }

    export async function EsFicheroJson(nombreNegocio: string, idElemento: number, idArchivo: number): Promise<{ esJson: boolean, json: JSON }> {
        let url = '';
        if (idElemento === 0) {
            let parametros = `idArchivo=${idArchivo}`;
            url = `/${Ajax.Archivos.controlador}/${Ajax.Archivos.accion.DescargarParaCrear}?${parametros}`;
        }
        else {

            const parametros = `negocio=${nombreNegocio}&idElemento=${idElemento}&idArchivo=${idArchivo}&auditar=false`;
            url = `/${Ajax.Archivos.controlador}/${Ajax.Archivos.accion.Descargar}?${parametros}`;
        }
        try {
            const contenido = await LeerFicheroDeUna(url);
            const resultado = EsJsonValido(contenido);
            return { esJson: resultado.esValido, json: resultado.json };
        } catch (error) {
            console.error('Error al leer el archivo:', error);
            return { esJson: false, json: undefined };
        }
    }



}


