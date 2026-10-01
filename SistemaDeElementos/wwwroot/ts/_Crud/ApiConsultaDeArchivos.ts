namespace ApiConsultaDeArchivos {

    const anchoDelSplitter = 6;
    const anchoMinimo = 200;
    const altoMinimo = 300;

    interface ArchivoDeConsulta {
        id: number;
        nombre: string;
    }

    // Visor de archivos de la página de consulta por guid (acceso sin validarse). Reutiliza el contenedor
    // datos + splitter + visor que ya pinta el servidor (oculto con visor-oculto): al crearse mete los
    // expansores de la página en el lado de los datos, y el visor se muestra al recibir el primer archivo
    // que se puede previsualizar, ocupando la mitad derecha de la pantalla.
    export class VisorParaConsultaDeArchivos {
        private readonly _edicion: Crud.CrudEdicion;
        private _archivos: Array<ArchivoDeConsulta> = [];
        private _indice = -1;
        private _cargando = false;

        private get Contenedor(): HTMLDivElement {
            return this._edicion.ContenedorDeDatosMasVisor;
        }

        private get NombreDelArchivo(): HTMLInputElement {
            return this._edicion.ContenedorDelVisor.querySelector(`.${ltrCss.crud.panelDeEdicion.VisorDeNombreAnexados}`) as HTMLInputElement;
        }

        private get VisorVisible(): boolean {
            return !this.Contenedor.classList.contains(ltrCss.crud.panelDeEdicion.VisorOculto);
        }

        constructor(edicion: Crud.CrudEdicion) {
            this._edicion = edicion;
            this.RecolocarExpanes();
            this._edicion.Splitter.addEventListener('mousedown', (e: MouseEvent) => this.IniciarArrastre(e));
            window.addEventListener('resize', () => this.AjustarAlto());
        }

        // Los anexados se leen por páginas y se recibe cada una; la primera que trae algo previsualizable lo muestra.
        public AgregarArchivos(archivosDto: Array<any>): void {
            for (const archivoDto of archivosDto) {
                const id = Numero(ObtenerPropiedad(archivoDto, ltrPropiedades.Elemento.Id));
                const nombre: string = ObtenerPropiedad(archivoDto, ltrPropiedades.Elemento.Nombre);
                if (EsRenderizable(nombre) && !this._archivos.some(a => a.id === id))
                    this._archivos.push({ id: id, nombre: nombre });
            }

            if (this._indice < 0 && this._archivos.length > 0)
                this.Mostrar(0);
        }

        public MostrarArchivo(idArchivo: number): boolean {
            const indice = this._archivos.findIndex(a => a.id === idArchivo);
            if (indice < 0)
                return false;
            this.Mostrar(indice);
            return true;
        }

        public EjecutarAccion(accion: string): boolean {
            switch (accion) {
                case ltrEventos.Edicion.SiguienteArchivo: {
                    this.Desplazar(1);
                    return true;
                }
                case ltrEventos.Edicion.AnteriorArchivo: {
                    this.Desplazar(-1);
                    return true;
                }
                case ltrEventos.Edicion.DescargarArchivo: {
                    this.Descargar();
                    return true;
                }
            }
            return false;
        }

        private Desplazar(incremento: number): void {
            const total = this._archivos.length;
            if (total < 2)
                return;
            this.Mostrar((this._indice + incremento + total) % total);
        }

        private Descargar(): void {
            if (this._indice < 0)
                return;
            const idArchivo = this._archivos[this._indice].id;
            ApiDeArchivos.DescargarAnexado(`${ltrEventos.Archivo.Descargar}-${idArchivo}`, this.Url(Ajax.Archivos.accion.DescargarPorGuid, idArchivo));
        }

        private async Mostrar(indice: number): Promise<void> {
            if (this._cargando)
                return;

            this._cargando = true;
            this._indice = indice;
            const archivo = this._archivos[indice];
            const visor = this._edicion.DivVisor;
            this.MostrarVisor();
            this.NombreDelArchivo.value = archivo.nombre;
            visor.innerHTML = 'Cargando...';
            try {
                const response = await fetch(this.Url(Ajax.Archivos.accion.DescargarPorGuid, archivo.id));
                const blob = await response.blob();
                const renderizado = await ApiVisorDeArchivos.RenderizarBlobEnVisor(visor, blob,
                    (accion) => this.Url(Ajax.Archivos.accion.DescargarComoHtmlPorGuid, archivo.id, accion));
                if (!renderizado)
                    visor.innerHTML = 'Este archivo no se puede previsualizar, descárguelo';
            }
            catch (error) {
                visor.innerHTML = 'Error al cargar el archivo';
            }
            finally {
                this._cargando = false;
            }
        }

        private Url(endPoint: string, idArchivo: number, accion: string = undefined): string {
            const parametros = new URLSearchParams({
                negocio: this._edicion.NombreDeNegocio,
                idElemento: this._edicion.IdDeConsulta.toString(),
                idArchivo: idArchivo.toString(),
                guid: this._edicion.GuidDeConsulta
            });
            if (Definido(accion))
                parametros.append('accion', accion);
            return `/${Ajax.Archivos.controlador}/${endPoint}?${parametros}`;
        }

        // El servidor pinta los expansores de la página (observaciones, archivos...) detrás del panel de
        // edición; se pasan al lado de los datos para que el visor ocupe todo el lado derecho.
        private RecolocarExpanes(): void {
            const datos = this._edicion.ContenedorDeDatos;
            const expanes = Array.from(this._edicion.PanelDeEditar.parentElement.children)
                .filter(e => e.getAttribute(atControl.tipo) === ltrTipoControl.spanDeControles);
            for (const expan of expanes)
                datos.appendChild(expan);
        }

        private MostrarVisor(): void {
            if (this.VisorVisible)
                return;

            const contenedor = this.Contenedor;
            ApiControl.ExcluirCss(contenedor, ltrCss.crud.panelDeEdicion.VisorOculto);
            // mitad para los datos y mitad para el visor; la fila al alto del contenedor para que los datos
            // hagan scroll por su cuenta y el visor ocupe todo el alto
            contenedor.style.gridTemplateColumns = `minmax(0, 1fr) ${anchoDelSplitter}px minmax(0, 1fr)`;
            contenedor.style.gridTemplateRows = 'minmax(0, 1fr)';
            this.AjustarAlto();
        }

        private AjustarAlto(): void {
            if (!this.VisorVisible)
                return;

            const contenedor = this.Contenedor;
            const pie = document.getElementById('pie-de-pagina');
            const limite = Definido(pie) ? pie.getBoundingClientRect().top : window.innerHeight;
            contenedor.style.height = `${Math.max(limite - contenedor.getBoundingClientRect().top - 5, altoMinimo)}px`;
        }

        private IniciarArrastre(e: MouseEvent): void {
            e.preventDefault();
            const contenedor = this.Contenedor;
            const visor = this._edicion.DivVisor;
            // el iframe del visor se queda con los eventos del ratón si el cursor pasa por encima al arrastrar
            visor.style.pointerEvents = 'none';

            const mover = (ev: MouseEvent) => {
                const rect = contenedor.getBoundingClientRect();
                const anchoDeDatos = Math.min(Math.max(ev.clientX - rect.left, anchoMinimo), rect.width - anchoDelSplitter - anchoMinimo);
                contenedor.style.gridTemplateColumns = `${anchoDeDatos}px ${anchoDelSplitter}px minmax(0, 1fr)`;
            };
            const soltar = () => {
                visor.style.pointerEvents = '';
                document.removeEventListener('mousemove', mover);
                document.removeEventListener('mouseup', soltar);
            };
            document.addEventListener('mousemove', mover);
            document.addEventListener('mouseup', soltar);
        }
    }
}
