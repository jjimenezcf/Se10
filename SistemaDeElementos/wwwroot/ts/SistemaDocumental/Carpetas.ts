namespace SistemaDocumental {

    export let JerarquiaDeCarpetas: SistemaDocumental.Carpetas = null;

    export function CrearFormulario(idFormulario: string, negocio: string, idArchivador: number) {
        JerarquiaDeCarpetas = new SistemaDocumental.Carpetas(idFormulario, negocio, idArchivador);
        window.addEventListener("load", function () { JerarquiaDeCarpetas.InicializarJerarquia(true); }, false);

        window.onbeforeunload = function () {
            JerarquiaDeCarpetas.AntesDeSalir();
        };
    }


    export const ltrCarpetas = {
        archivos: {
            expansorDeArchivos: 'expandir.detalle-archivos.input',
            contenedorDeExpansor: 'detalle-archivos-cuerpo',
            contenedorDeArchivos: 'contenedor-detalle-archivos'
        }
    };

    export class Carpetas extends Formulario.Jerarquia {
        _idArchivador: number;
        _nombreArchivador: string;
        _configurado: boolean = false;

        constructor(idFormulario: string, negocio: string, idArchivador: number) {
            super(idFormulario, negocio);
            this._idArchivador = idArchivador;
        }

        public InicializarJerarquia(blanquearFiltros: boolean): void {
            super.InicializarJerarquia(blanquearFiltros);
            MapearAlControl.Propiedad(this.PanelDelDto, ltrPropiedades.SisDoc.Archivador.propiedad, this._idArchivador, "archivador", true, true);
            this.BotonDeAbrirModalDeFiltro.style.display = ltrStyle.display.none;
        }

        public AntesDePintarLaJerarquia(): void {
            super.AntesDePintarLaJerarquia();
            let documento = this.jerarquia.ramas[0];
            this._nombreArchivador = documento.dto.nombre;
            this.Titulo.text = documento.dto.nombre;
            for (let i = 0; i < this.jerarquia.ramas[0].hijos.length; i++)
                this.jerarquia.ramas.push(this.jerarquia.ramas[0].hijos[i]);
            this.jerarquia.ramas.splice(0, 1);
        }

        public DespuesDePintarLaJerarquia(): void {
            if (this.jerarquia.ramas.length == 0)
                super.DespuesDePintarLaJerarquia();
            else {
                let lis = this.ContenedorDeJerarquia.querySelectorAll('li') as NodeListOf<HTMLLIElement>;
                if (Definido(lis) && lis.length > 0) Formulario.NodoSeleccionado(lis[0].id);
            }
            MapearAlControl.Propiedad(this.PanelDelDto, ltrPropiedades.SisDoc.Archivador.propiedad, this._idArchivador, this._nombreArchivador, true, true);

            // Configurar los elementos destino (elementos li dentro del div de jerarquía)
            const destinos = document.querySelectorAll('#carpeta\\.jerarquia\\.ul li');
            destinos.forEach(destino => {
                destino.addEventListener('dragover', (event: DragEvent) => {
                    event.preventDefault();
                    var li = event.currentTarget as HTMLLIElement;
                    if (!li.classList.contains(ltrCss.formulario.archivoSobreLi))
                        li.classList.add(ltrCss.formulario.archivoSobreLi);
                });

                destino.addEventListener('dragleave', (event: DragEvent) => {
                    event.preventDefault();
                    var li = event.currentTarget as HTMLLIElement;
                    li.classList.remove(ltrCss.formulario.archivoSobreLi);
                    setTimeout(() => {
                        ApiLocalStorage.PararTesteoDeRecargarArchivos();
                    }, 3000);
                });



                destino.addEventListener('drop', (event: DragEvent) => {
                    try {
                        this.AlSoltarUnArchivo(event);
                    }
                    finally {
                        var li = event.currentTarget as HTMLLIElement;
                        li.classList.remove(ltrCss.formulario.archivoSobreLi);
                    }
                });
            });

        }

        private AlSoltarUnArchivo(event: DragEvent): void {
            event.preventDefault();
            event.stopPropagation();
            const esTextoPlano = event.dataTransfer.types.includes("text/plain");
            if (!esTextoPlano) {
                if (event.dataTransfer.types.includes("Files")) {
                    const files = event.dataTransfer.files;
                    const idCarpetaDestino = Numero((event.currentTarget as HTMLLIElement).getAttribute('id').replace('.li', ''));

                    // Crear un único input file que acepta múltiples archivos
                    const input = document.createElement('input');
                    input.type = 'file';
                    input.id = 'file-input-multiple';
                    input.multiple = true;
                    input.style.display = 'none';
                    input.setAttribute(atControl.negocio, this.Negocio);
                    input.setAttribute(atControl.idElemento, idCarpetaDestino.toString());
                    input.setAttribute('contenedor-donde-mostrar', 'contenedor-detalle-archivos');
                    input.id = 'selector-oculto'

                    var recargarArchivosTrasTerminarSubida = idCarpetaDestino === ObtenerPropiedad(this.RegistroEditado, literal.id, 0);

                    // Crear un FileList con todos los archivos
                    const dataTransfer = new DataTransfer();
                    for (let i = 0; i < files.length; i++) {
                        dataTransfer.items.add(files[i]);
                    }
                    input.files = dataTransfer.files;

                    this.PanelDelDto.appendChild(input);

                    try {
                        // Llamar a PulsadoAnexarArchivos
                        ApiDeArchivos.AnexarArchivosArrastradosAlArbol(input.id, recargarArchivosTrasTerminarSubida);
                    }
                    finally {
                        // Borrar el input después de la ejecución
                        setTimeout(() => {
                            this.PanelDelDto.removeChild(input);
                        }, 3000);
                    }
                }

                return;
            }
            event.dataTransfer.clearData("text/plain");
            let origen: { idCarpeta: number; idArchivo: number; } = ApiDeArchivos.ObtenerCarpetaArchivo(event.dataTransfer.getData('text'));
            if (origen.idCarpeta == 0) return;
            var li = event.currentTarget as HTMLLIElement;
            var idCarpetaDestino = Numero(li.getAttribute('id').replace('.li', ''));
            ApiDeArchivos.AlSoltarUnArchivoEnCarpeta(this.IdNegocio, origen.idCarpeta, idCarpetaDestino, origen.idArchivo)
                .then(() => {
                    this.IncrementarNumeroDeArchivos(li);
                    if (this.Guid === ApiLocalStorage.GuidDelFormularioOrigen())
                        ApiLocalStorage.PararTesteoDeRecargarArchivos();
                    else
                        ApiLocalStorage.ActivarRecargarArchivos()
                })
                .catch(() => {
                    ApiLocalStorage.PararTesteoDeRecargarArchivos();
                })
        }

        public ComenzarModoNuevo(): void {
            super.ComenzarModoNuevo();
            ApiPanel.OcultarPanelPorId(literal.ExpanDeArchivos);
            if (!this._configurado) {
                this._configurado = true
                ApiDeArchivos.ConfigurarDragAndDrop();
            }
        }

        protected AplicarPermisoDeCreacion(): void {

            ApiDePeticiones.LeerElementoPorId(this, ltrControladores.SisDoc.Archivador, this._idArchivador, new Array<Parametro>(), this._idArchivador)
                .then((peticion) => this.AjustarMenuDeCreacion(peticion))
                .catch((peticion) => this.DesactivarMenuDeCreacion(peticion));
        }

        private DesactivarMenuDeCreacion(peticion: any): any {
            super.AplicarPermisoDeCreacion();
        }

        private AjustarMenuDeCreacion(peticion: ApiDeAjax.DescriptorAjax): any {
            var archivadorBloqueado = ObtenerPropiedad(peticion.resultado.datos, ltrPropiedades.Elemento.Bloqueado, false);
            if (!ModoAcceso.HayPermisos(ModoAcceso.enumModoDeAccesoDeDatos.Gestor, ModoAcceso.Parsear(peticion.resultado.modoDeAcceso)) ||
                 archivadorBloqueado) {
                ApiControl.BloquearOpcionDeMenuPorId(this.IdDeLaOpcionDeMenu(Formulario.ltrJerarquia.opcionesDeMenu.crear));
                ApiPanel.DesactivarPanel(this.PanelDelDto);
                if (archivadorBloqueado) MensajesSe.Info("El archivador está bloqueado")
            }
        }

        public ComenzarModoEdicion(dto: any, modoAcceso: ModoAcceso.enumModoDeAccesoDeDatos, nodoSeleccionado: string): void {
            super.ComenzarModoEdicion(dto, modoAcceso, nodoSeleccionado);
            ApiPanel.MostrarPanelPorId(literal.ExpanDeArchivos);
            Crud.EventosDeExpansores(ltrEventos.Expansores.MostrarBloque, `${ltrCarpetas.archivos.expansorDeArchivos};${ltrCarpetas.archivos.contenedorDeExpansor}`);
            if (!this._configurado) {
                this._configurado = true
                ApiDeArchivos.ConfigurarDragAndDrop();
            }
        }

        public EsNodoSeleccionable(dto: Tipos.NodoDto): boolean {
            if (super.EsNodoSeleccionable(dto)) {
                return dto.negocio !== ltrNegocioSe.Nombre.Archivadores;
            }
            return false;
        }

        public PrepararfiltrosParaLeerLaJerarquia(datos: Diccionario<any>): boolean {
            let hayFiltros: boolean = super.PrepararfiltrosParaLeerLaJerarquia(datos);
            datos.Agregar(ltrPropiedades.SisDoc.Archivador.IdArchivador, this._idArchivador);
            return hayFiltros;
        }


        public TrasModificarConLaModal(peticion: ApiDeAjax.DescriptorAjax, modal: HTMLDivElement): any {
            super.TrasModificarConLaModal(peticion, modal);
            ApiDeArchivos.RecargarMostrarArchivosAnexados();
        }

        private IncrementarNumeroDeArchivos(li: HTMLLIElement) {
            const anchor = li.querySelector('a');
            var partes = anchor.innerText.split(ltrSimbolos.dosPuntosConEspacio)
            anchor.innerText = partes[0] + (partes.length == 2 ? `${ltrSimbolos.dosPuntosConEspacio}${Numero(partes[1]) + 1}` : '');
        }
    }

}