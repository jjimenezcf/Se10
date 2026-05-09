namespace ApiDeArchivos {

    export function ConfigurarDragAndDrop() {
        const inputs = document.querySelectorAll(`input[tipo='${ltrTipoControl.SelectorDeUnArchivo}']`) as NodeListOf<HTMLInputElement>;
        const contenedoresDeOpcion = document.querySelectorAll(`.${ltrCss.Archivos.ComponenteParaAnexar}`) as NodeListOf<HTMLDivElement>;

        inputs.forEach(configurarDragAndDropParaInput);
        contenedoresDeOpcion.forEach(configurarDragAndDropParaContenedor);
    }

    function configurarDragAndDropParaInput(input: HTMLInputElement) {
        const celda = input.closest('.' + ltrCss.crud.celda) as HTMLDivElement;
        if (!celda) return;

        configurarEventosDragAndDrop(celda, (files) => {
            if (files.length === 0) return;
            const file = files[0];

            // Verificar si el archivo es del tipo aceptado
            const acceptAttr = input.getAttribute('accept');
            let todas: boolean = acceptAttr === '*';
            if (!todas && acceptAttr) {
                const allowedExtensions = acceptAttr.split(',').map(ext => ext.trim().toLowerCase());
                const fileExtension = '.' + file.name.split('.').pop().toLowerCase();
                if (!allowedExtensions.includes(fileExtension)) {
                    alert(`Por favor, arrastre solo archivos de tipo: ${acceptAttr}`);
                    return;
                }
            }

            // Asignar el archivo al input
            const dataTransfer = new DataTransfer();
            dataTransfer.items.add(file);
            input.files = dataTransfer.files;

            // Obtener los parámetros necesarios para SubirArchivoSeleccionado
            const tabla = ApiControl.BuscarTabla(input);
            if (NoDefinido(tabla)) {
                MensajesSe.Error("función Drop", "No se ha localizado el contenedor donde se ubica el selector de archivos");
                return;
            }
            const contenedorId = tabla.parentElement.id;
            const inputId = input.id;
            const infoArchivoId = input.getAttribute('info-archivo');

            // Llamar a la función de subida
            if (typeof ApiDeArchivos !== 'undefined' && ApiDeArchivos.SubirArchivoSeleccionado) {
                ApiDeArchivos.SubirArchivoSeleccionado(contenedorId, inputId, infoArchivoId);
            } else {
                console.error('ApiDeArchivos.SubirArchivoSeleccionado no está definido');
            }
        });
    }
    function configurarDragAndDropParaContenedor(contenedor: HTMLDivElement) {
        const botonSeleccionar = contenedor.querySelector('button[title="Seleccionar archivos"]') as HTMLButtonElement;
        if (!botonSeleccionar) return;

        configurarEventosDragAndDrop(contenedor, (files) => {
            const onclickAttr = botonSeleccionar.getAttribute('onclick');
            if (onclickAttr) {
                const match = onclickAttr.match(/ApiDeArchivos\.SeleccionarArchivos\('(.+?)'\)/);
                if (match) {
                    const selectorId = match[1];
                    if (typeof ApiDeArchivos !== 'undefined' && ApiDeArchivos.CrearArchivosPendietesDeSubir) {
                        ApiDeArchivos.CrearArchivosPendietesDeSubir(selectorId, files);
                    } else {
                        console.error('ApiDeArchivos.CrearArchivosPendietesDeSubir no está definido');
                    }
                }
            }
        });
    }

    function configurarEventosDragAndDrop(elemento: HTMLElement, manejadorArchivos: (files: FileList) => void) {
        elemento.addEventListener('dragover', (e) => {
            e.preventDefault();
            e.stopPropagation();
            if (e.dataTransfer.types.includes("text/plain")) {
                elemento.style.backgroundColor = 'lightgray';
            }
            else if (e.dataTransfer.types.includes("Files") || (e.dataTransfer?.files && e.dataTransfer.files.length > 0)) {
                elemento.style.backgroundColor = '#e1e7f0';
            }
        });


        elemento.addEventListener('dragleave', (e) => {
            e.preventDefault();
            e.stopPropagation();
            elemento.style.backgroundColor = '';
        });

        // Evento dragend para limpiar estilos
        elemento.addEventListener("dragend", (e) => {
            if (e.dataTransfer.types.includes("text/plain")) {
                elemento.style.backgroundColor = '';
                setTimeout(() => {
                    ApiLocalStorage.PararTesteoDeRecargarArchivos();
                }, 3000);
            }
        });

        elemento.addEventListener('dragstart', (e) => {
            let crtlOrigen = e.srcElement;

            if (!Definido(crtlOrigen) || !(crtlOrigen instanceof HTMLAnchorElement)) {
                e.preventDefault();
                e.stopPropagation();
                return; // Detener el arrastre si no es un HTMLAnchorElement
            }

            if (!e.dataTransfer.types.includes("text/plain")) {
                e.preventDefault();
                e.stopPropagation();
                return; // Detener el arrastre si no incluye "text/plain"
            }

            // Si llegamos aquí, permitimos el arrastre y establecemos los datos
            var nombreFichero = crtlOrigen.innerText;
            e.dataTransfer.setData("application/json", JSON.stringify({ nombreFichero: nombreFichero }));
        });

        elemento.addEventListener('drop', (e) => {
            e.preventDefault();
            e.stopPropagation();

            try {
                if (e.dataTransfer.types.includes("text/plain")) {

                    if (elemento instanceof HTMLDivElement && !elemento.classList.contains(ltrCss.crud.celda)) {
                        let carpetaDestino: { idNegocio: number; idCarpeta: number } = ValidarCarpetaDeDestino();
                        if (carpetaDestino.idNegocio == 0) return;
                        let origen: { idCarpeta: number; idArchivo: number; } = ObtenerCarpetaArchivo(e.dataTransfer.getData('text'));
                        if (origen.idCarpeta == 0) return;
                        ApiDeArchivos.AlSoltarUnArchivoEnCarpeta(carpetaDestino.idNegocio, origen.idCarpeta, carpetaDestino.idCarpeta, origen.idArchivo)
                            .then(() => {
                                ApiLocalStorage.ActivarRecargarArchivos()
                            })
                            .catch(() => {
                                ApiLocalStorage.PararTesteoDeRecargarArchivos()
                            })
                    }
                    else if (elemento instanceof HTMLDivElement && elemento.classList.contains(ltrCss.crud.celda)) {
                        var selector = elemento.querySelector(`input[${atControl.tipo}="${ltrTipoControl.SelectorDeUnArchivo}"]`) as HTMLInputElement;
                        let origen: { idCarpeta: number; idArchivo: number; } = ApiDeArchivos.ObtenerCarpetaArchivo(e.dataTransfer.getData('text'));
                        let nombreFichero = 'fichero';
                        if (e.dataTransfer.types.includes("application/json")) {
                            const jsonData = JSON.parse(e.dataTransfer.getData("application/json"));
                            nombreFichero = jsonData.nombreFichero;
                        }
                        EnlazarUnArchivoAlSelectorDeFichero(selector.id, origen.idArchivo, nombreFichero);
                    }
                }
                else if (e.dataTransfer?.files && e.dataTransfer.files.length > 0) {
                    manejadorArchivos(e.dataTransfer.files);
                }
            }
            finally {
                elemento.style.backgroundColor = '';
                e.dataTransfer.clearData("text/plain");
            }

        });
    }

    function EnlazarUnArchivoAlSelectorDeFichero(idCtrlArchivo: string, idArchivo: number, nombreFichero: string) {
        let archivo: HTMLInputElement = document.getElementById(idCtrlArchivo) as HTMLInputElement;
        const infoArchivo = document.getElementById(archivo.getAttribute(atArchivo.infoArchivo)) as HTMLInputElement;
        const barraHtml = document.getElementById(archivo.getAttribute(atArchivo.barra)) as HTMLDivElement;
        const span = barraHtml.children[0] as HTMLElement;
        infoArchivo.style.display = ltrStyle.display.none;
        ApiControl.HacerVisibleLaBarra(barraHtml, true);
        barraHtml.style.width = '100%';
        span.innerHTML = '100%';
        MensajesSe.Info(`El fichero '${nombreFichero}', que has arrastrado, está disponible para ser enlazado`);
        ApiDePeticiones.TrasSubirUnArchivoAlSelectorDeFichero(archivo, barraHtml, idArchivo);
    }

    function ValidarCarpetaDeDestino() {
        var dtoDelFormulario = document.querySelector(`.${ltrCss.formulario.dto}`) as HTMLElement;
        if (!Definido(dtoDelFormulario)) {
            MensajesSe.Info('El destino donde arrastrar un fichero ha de ser la edición de una carpeta')
            return { idNegocio: 0, idCarpeta: 0 };
        }
        var idNegocio = Numero(dtoDelFormulario.getAttribute('id-negocio'));
        if (idNegocio === 0) {
            MensajesSe.Info(`No se ha definido el negocio en el control ${dtoDelFormulario.id}`)
            return { idNegocio: 0, idCarpeta: 0 };
        }
        var idCarpetaDestino = Numero(dtoDelFormulario.getAttribute(Formulario.ltrJerarquia.arbol.nodoSeleccionado).replace('.li', ''));
        if (idCarpetaDestino === 0) {
            MensajesSe.Info(`No se ha definido la carpeta destino en el control ${dtoDelFormulario.id}`)
            return { idNegocio: 0, idCarpeta: 0 };
        }
        return { idNegocio: idNegocio, idCarpeta: idCarpetaDestino };
    }

    export function ObtenerCarpetaArchivo(texto: string) {
        const partes = texto.split(':');
        if (partes.length !== 2 || partes[0] !== 'archivo-carpeta') {
            MensajesSe.Info(`Datos no admitidos ${texto}`);
            return { idCarpeta: 0, idArchivo: 0 };
        }
        var archivoCarpeta = partes[1].split('-');
        if (archivoCarpeta.length !== 2) {
            MensajesSe.Info(`Datos no admitidos ${texto}`);
            return { idCarpeta: 0, idArchivo: 0 };
        }
        const idArchivo = Numero(archivoCarpeta[0]);
        const idCarpeta = Numero(archivoCarpeta[1]);

        if (idCarpeta === 0 || idArchivo === 0) {
            const dtoDelFormulario = document.querySelector(`.${ltrCss.formulario.dto}`) as HTMLElement;
            MensajesSe.Info(`No se ha definido la carpeta origen o el archivo en el evento ${dtoDelFormulario.id}`)

            return { idCarpeta: 0, idArchivo: 0 };
        }

        return { idCarpeta, idArchivo };
    }

    export function AlSoltarUnArchivoEnCarpeta(idNegocioDeCarpeta: number, idCarpetaOrigen: number, idCarpetaDestino: number, idArchivo: number): Promise<void> {
        if (idCarpetaOrigen === idCarpetaDestino) {
            MensajesSe.Info(`No se puede mover un archivo a su misma carpeta, debe seleccionar otra diferente`);
            return Promise.reject();
        }

        let parametros = new Array<Parametro>();
        let idsDeArchivos: Array<number> = new Array<number>();
        idsDeArchivos.push(idArchivo);
        parametros.push(new Parametro(ltrPropiedades.SisDoc.Archivo.IdsDeArchivos, idsDeArchivos));

        return ApiDePeticiones.OperacionConArchivos(idNegocioDeCarpeta, atArchivo.Operacion.Mover, idNegocioDeCarpeta, idCarpetaOrigen, idNegocioDeCarpeta, idCarpetaDestino, parametros)
            .then(() => {
                return ApiDeArchivos.RecargarMostrarArchivosAnexados();
            })
            .catch((peticion) => {
                ApiDePeticiones.EmitirError(peticion);
                throw peticion; // Re-throw the error to maintain the rejection
            });
    }
}