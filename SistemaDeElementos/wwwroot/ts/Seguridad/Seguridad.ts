namespace Seguridad {

    const ltrVistaDeSeguridad = {
        endPoint: {
            mostrarSeguridad: 'epMostrarSeguridad',
            leerNodosHijos: 'epLeerNodosHijos',
            leerDatosDelObjeto: 'epLeerDatosDelObjeto'
        },
        parametro: {
            idUsuario: 'idUsuario',
            clave: 'clave',
            tipo: 'tipo',
            objeto: 'objeto',
            id: 'id',
            idNegocio: 'idNegocio',
            clase: 'clase'
        },
        objeto: {
            ninguno: 'Ninguno',
            puesto: 'Puesto',
            rol: 'Rol',
            permiso: 'Permiso'
        },
        tituloDelObjeto: {
            Puesto: 'Puesto de trabajo',
            Rol: 'Rol',
            Permiso: 'Permiso'
        },
        tipo: {
            usuario: 'Usuario'
        },
        imagenDeRecargar: '/images/menu/Recargar.svg',
        idArbol: 'div_arbol_de_permisos',
        atributo: {
            hijosCargados: 'hijos-cargados',
            tipoNodo: 'tipo-nodo',
            paginaDeEdicion: 'pagina-de-edicion'
        },
        css: {
            expansor: 'seguridad-expansor',
            icono: 'seguridad-icono',
            recargar: 'seguridad-recargar',
            nodoGrupo: 'nodo-seguridad-grupo',
            nodoHeredado: 'nodo-seguridad-heredado',
            nodoConHijos: 'nodo-seguridad-con-hijos',
            nodoDesplegado: 'nodo-seguridad-desplegado',
            nodoCargando: 'nodo-seguridad-cargando'
        }
    };

    interface NodoDeSeguridad {
        clave: string;
        tipo: string;
        objeto: string;
        id: number;
        idUsuario: number;
        idNegocio: number;
        clase: string;
        nombre: string;
        icono: string;
        ayuda: string;
        cantidad: number;
        tieneHijos: boolean;
        heredado: boolean;
        hijos: Array<NodoDeSeguridad>;
    }

    let vistaDeSeguridad: VistaDeSeguridad = undefined;

    export function CrearVistaDeSeguridad(idVista: string): void {
        vistaDeSeguridad = new VistaDeSeguridad(idVista);
        window.addEventListener("load", function () { vistaDeSeguridad.Inicializar(); }, false);
    }

    // se evalúa desde el atributo tras-seleccionar del selector de usuario
    export function UsuarioSeleccionado(lista: HTMLInputElement): void {
        if (NoDefinido(vistaDeSeguridad))
            return;
        vistaDeSeguridad.MostrarSeguridad(Numero(lista.getAttribute(atListasDinamicas.idSeleccionado)));
    }

    class VistaDeSeguridad {

        private readonly idVista: string;
        private claveSeleccionada: string = undefined;
        // nodo cuyo objeto se muestra en el panel de datos
        private nodoMostrado: NodoDeSeguridad = undefined;
        // usuarios cuya raíz se está leyendo
        private usuariosPendientes: Set<number> = new Set<number>();
        // datos de puestos, roles y permisos ya leídos, por objeto e id
        private objetos: Map<string, any> = new Map<string, any>();

        constructor(idVista: string) {
            this.idVista = idVista;
        }

        private get Contenedor(): HTMLDivElement {
            return document.getElementById(this.idVista) as HTMLDivElement;
        }

        private get Controlador(): string {
            return this.Contenedor.getAttribute('controlador');
        }

        private get Arbol(): HTMLUListElement {
            return document.getElementById(`${this.idVista}-arbol`) as HTMLUListElement;
        }

        private get PanelDelArbol(): HTMLDivElement {
            return document.getElementById(ltrVistaDeSeguridad.idArbol) as HTMLDivElement;
        }

        private get Splitter(): HTMLDivElement {
            return document.getElementById(`${this.idVista}-splitter`) as HTMLDivElement;
        }

        private get TituloDeDatos(): HTMLSpanElement {
            return document.getElementById(`${this.idVista}-titulo-datos`) as HTMLSpanElement;
        }

        private get BotonEditar(): HTMLAnchorElement {
            return document.getElementById(`${this.idVista}-editar`) as HTMLAnchorElement;
        }

        private PanelDelObjeto(objeto: string): HTMLDivElement {
            return document.getElementById(`${this.idVista}-${objeto.toLowerCase()}`) as HTMLDivElement;
        }

        public Inicializar(): void {
            ApiPanel.InicializarSplitterDelPanelIzquierdo(this.Splitter, this.PanelDelArbol);
            this.BotonEditar.addEventListener('click', (evento) => {
                evento.preventDefault();
                this.EditarObjetoMostrado();
            });
        }

        // abre en una nueva pestaña el crud del objeto mostrado, editando el elemento
        private EditarObjetoMostrado(): void {
            if (NoDefinido(this.nodoMostrado))
                return;

            let pagina: string = this.PanelDelObjeto(this.nodoMostrado.objeto).getAttribute(ltrVistaDeSeguridad.atributo.paginaDeEdicion);
            EntornoSe.AbrirPestana(`${window.location.origin}/${pagina}?${ltrParametrosUrl.id}=${this.nodoMostrado.id}`);
        }

        private RaizDelUsuario(idUsuario: number): HTMLLIElement {
            return document.getElementById(`${this.idVista}.usuario-${idUsuario}`) as HTMLLIElement;
        }

        // cada usuario seleccionado añade su raíz al árbol, para poder comparar los permisos de varios usuarios
        public MostrarSeguridad(idUsuario: number): void {
            if (idUsuario <= 0)
                return;

            let raiz: HTMLLIElement = this.RaizDelUsuario(idUsuario);
            if (Definido(raiz)) {
                raiz.scrollIntoView({ block: 'nearest' });
                return;
            }

            this.SolicitarUsuario(idUsuario, (nodo) => this.PintarRaiz(nodo));
        }

        private SolicitarUsuario(idUsuario: number, alRecibir: (raiz: NodoDeSeguridad) => void): void {
            // el selector puede notificar la misma selección al pulsar y al perder el foco
            if (this.usuariosPendientes.has(idUsuario))
                return;

            this.usuariosPendientes.add(idUsuario);
            let parametros: Array<Parametro> = new Array<Parametro>();
            parametros.push(new Parametro(ltrVistaDeSeguridad.parametro.idUsuario, idUsuario));
            ApiDePeticiones.EjecutarPeticion(this, this.Controlador, ltrVistaDeSeguridad.endPoint.mostrarSeguridad, parametros, new Array<Parametro>())
                .then((peticion) => alRecibir(peticion.resultado.datos as NodoDeSeguridad))
                .catch((peticion) => ApiDePeticiones.EmitirError(peticion))
                .finally(() => this.usuariosPendientes.delete(idUsuario));
        }

        private PintarRaiz(raiz: NodoDeSeguridad): void {
            if (Definido(this.RaizDelUsuario(raiz.idUsuario)))
                return;

            let li: HTMLLIElement = this.CrearRaiz(raiz);
            this.Arbol.appendChild(li);
            li.scrollIntoView({ block: 'nearest' });

            if (NoDefinido(this.claveSeleccionada))
                this.TituloDeDatos.textContent = 'Seleccione un puesto de trabajo, un rol o un permiso';
        }

        private CrearRaiz(raiz: NodoDeSeguridad): HTMLLIElement {
            let li: HTMLLIElement = this.CrearNodo(raiz);
            li.classList.add(ltrVistaDeSeguridad.css.nodoDesplegado);
            this.PintarHijos(li, raiz.hijos);
            return li;
        }

        // pliega el árbol del usuario y lo vuelve a leer, para ver los cambios hechos en sus puestos, roles o permisos
        private RecargarUsuario(idUsuario: number): void {
            let li: HTMLLIElement = this.RaizDelUsuario(idUsuario);
            if (NoDefinido(li))
                return;

            li.classList.remove(ltrVistaDeSeguridad.css.nodoDesplegado);
            let ul: HTMLUListElement = this.HijosDelNodo(li);
            if (Definido(ul)) ul.style.display = ltrStyle.display.none;

            this.SolicitarUsuario(idUsuario, (raiz) => {
                let actual: HTMLLIElement = this.RaizDelUsuario(idUsuario);
                if (NoDefinido(actual))
                    return;

                actual.replaceWith(this.CrearRaiz(raiz));

                // los datos leídos pueden haber cambiado, y el nodo mostrado ya no está en el árbol
                this.objetos.clear();
                if (Definido(this.nodoMostrado) && this.nodoMostrado.clave.startsWith(`${raiz.clave}.`)) {
                    this.claveSeleccionada = undefined;
                    this.OcultarDatos('Seleccione un puesto de trabajo, un rol o un permiso');
                }
            });
        }

        private PintarNodo(ul: HTMLUListElement, nodo: NodoDeSeguridad): HTMLLIElement {
            let li: HTMLLIElement = this.CrearNodo(nodo);
            ul.appendChild(li);
            return li;
        }

        private CrearNodo(nodo: NodoDeSeguridad): HTMLLIElement {
            let li: HTMLLIElement = document.createElement('li');
            li.id = `${this.idVista}.${nodo.clave}`;
            li.setAttribute(ltrVistaDeSeguridad.atributo.tipoNodo, nodo.tipo);
            li.classList.add(ltrCss.nodoDeJerarquia);
            if (nodo.tieneHijos) li.classList.add(ltrVistaDeSeguridad.css.nodoConHijos);
            if (nodo.objeto === ltrVistaDeSeguridad.objeto.ninguno) li.classList.add(ltrVistaDeSeguridad.css.nodoGrupo);
            if (nodo.heredado) li.classList.add(ltrVistaDeSeguridad.css.nodoHeredado);

            let expansor: HTMLSpanElement = document.createElement('span');
            expansor.classList.add(ltrVistaDeSeguridad.css.expansor);
            expansor.addEventListener('click', (evento) => {
                evento.stopPropagation();
                this.AlternarNodo(li, nodo);
            });
            li.appendChild(expansor);

            if (nodo.tipo === ltrVistaDeSeguridad.tipo.usuario) {
                let recargar: HTMLImageElement = document.createElement('img');
                recargar.src = ltrVistaDeSeguridad.imagenDeRecargar;
                recargar.alt = 'recargar';
                recargar.title = 'Recargar la seguridad del usuario';
                recargar.classList.add(ltrVistaDeSeguridad.css.icono, ltrVistaDeSeguridad.css.recargar);
                recargar.addEventListener('click', (evento) => {
                    evento.stopPropagation();
                    this.RecargarUsuario(nodo.idUsuario);
                });
                li.appendChild(recargar);
            }

            let a: HTMLAnchorElement = document.createElement('a');
            a.href = '#';
            if (!IsNullOrEmpty(nodo.icono)) {
                let icono: HTMLImageElement = document.createElement('img');
                icono.src = `/images/menu/${nodo.icono}`;
                icono.alt = '';
                icono.classList.add(ltrVistaDeSeguridad.css.icono);
                a.appendChild(icono);
            }
            a.appendChild(document.createTextNode(nodo.cantidad >= 0 ? `${nodo.nombre} (${nodo.cantidad})` : nodo.nombre));
            if (!IsNullOrEmpty(nodo.ayuda)) a.title = nodo.ayuda;
            a.addEventListener('click', (evento) => {
                evento.preventDefault();
                this.NodoPulsado(li, nodo);
            });
            li.appendChild(a);

            return li;
        }

        private PintarHijos(li: HTMLLIElement, hijos: Array<NodoDeSeguridad>): void {
            let ul: HTMLUListElement = this.HijosDelNodo(li);
            if (NoDefinido(ul)) {
                ul = document.createElement('ul');
                li.appendChild(ul);
            }

            ul.innerHTML = '';
            if (Definido(hijos))
                for (let i = 0; i < hijos.length; i++)
                    this.PintarNodo(ul, hijos[i]);

            li.setAttribute(ltrVistaDeSeguridad.atributo.hijosCargados, 'S');
            ul.style.display = li.classList.contains(ltrVistaDeSeguridad.css.nodoDesplegado) ? ltrStyle.display.block : ltrStyle.display.none;
        }

        private HijosDelNodo(li: HTMLLIElement): HTMLUListElement {
            return li.querySelector(':scope > ul') as HTMLUListElement;
        }

        // pulsar en un puesto, rol o permiso muestra sus datos y despliega sus hijos; pulsar en un grupo lo pliega o despliega
        private NodoPulsado(li: HTMLLIElement, nodo: NodoDeSeguridad): void {
            if (nodo.objeto === ltrVistaDeSeguridad.objeto.ninguno) {
                this.AlternarNodo(li, nodo);
                return;
            }

            this.SeleccionarNodo(li);
            // en el móvil no se muestra el panel de datos, así que no se leen
            if (!EsDispositvoMovil())
                this.MostrarObjeto(nodo);
            this.Desplegar(li, nodo, true);
        }

        private SeleccionarNodo(li: HTMLLIElement): void {
            ApiDeJerarquia.DesSeleccionarNodo(this.PanelDelArbol);
            li.classList.add(ltrCss.nodoSeleccionado);
        }

        private AlternarNodo(li: HTMLLIElement, nodo: NodoDeSeguridad): void {
            this.Desplegar(li, nodo, !li.classList.contains(ltrVistaDeSeguridad.css.nodoDesplegado));
        }

        private Desplegar(li: HTMLLIElement, nodo: NodoDeSeguridad, desplegar: boolean): void {
            if (!nodo.tieneHijos)
                return;

            let ul: HTMLUListElement = this.HijosDelNodo(li);
            if (!desplegar) {
                li.classList.remove(ltrVistaDeSeguridad.css.nodoDesplegado);
                if (Definido(ul)) ul.style.display = ltrStyle.display.none;
                return;
            }

            li.classList.add(ltrVistaDeSeguridad.css.nodoDesplegado);
            if (EsTrue(li.getAttribute(ltrVistaDeSeguridad.atributo.hijosCargados))) {
                ul.style.display = ltrStyle.display.block;
                return;
            }

            if (li.classList.contains(ltrVistaDeSeguridad.css.nodoCargando))
                return;

            li.classList.add(ltrVistaDeSeguridad.css.nodoCargando);
            let parametros: Array<Parametro> = new Array<Parametro>();
            parametros.push(new Parametro(ltrVistaDeSeguridad.parametro.clave, nodo.clave));
            parametros.push(new Parametro(ltrVistaDeSeguridad.parametro.tipo, nodo.tipo));
            parametros.push(new Parametro(ltrVistaDeSeguridad.parametro.idUsuario, nodo.idUsuario));
            parametros.push(new Parametro(ltrVistaDeSeguridad.parametro.id, nodo.id));
            parametros.push(new Parametro(ltrVistaDeSeguridad.parametro.idNegocio, nodo.idNegocio));
            parametros.push(new Parametro(ltrVistaDeSeguridad.parametro.clase, IsNullOrEmpty(nodo.clase) ? '' : nodo.clase));
            ApiDePeticiones.EjecutarPeticion(this, this.Controlador, ltrVistaDeSeguridad.endPoint.leerNodosHijos, parametros, new Array<Parametro>())
                .then((peticion) => this.PintarHijos(li, peticion.resultado.datos as Array<NodoDeSeguridad>))
                .catch((peticion) => {
                    li.classList.remove(ltrVistaDeSeguridad.css.nodoDesplegado);
                    ApiDePeticiones.EmitirError(peticion);
                })
                .finally(() => li.classList.remove(ltrVistaDeSeguridad.css.nodoCargando));
        }

        private MostrarObjeto(nodo: NodoDeSeguridad): void {
            this.claveSeleccionada = nodo.clave;
            let claveDelObjeto: string = `${nodo.objeto}-${nodo.id}`;
            if (this.objetos.has(claveDelObjeto)) {
                this.MapearObjeto(nodo, this.objetos.get(claveDelObjeto));
                return;
            }

            let parametros: Array<Parametro> = new Array<Parametro>();
            parametros.push(new Parametro(ltrVistaDeSeguridad.parametro.objeto, nodo.objeto));
            parametros.push(new Parametro(ltrVistaDeSeguridad.parametro.id, nodo.id));
            ApiDePeticiones.EjecutarPeticion(this, this.Controlador, ltrVistaDeSeguridad.endPoint.leerDatosDelObjeto, parametros, new Array<Parametro>())
                .then((peticion) => {
                    this.objetos.set(claveDelObjeto, peticion.resultado.datos);
                    if (this.claveSeleccionada === nodo.clave)
                        this.MapearObjeto(nodo, peticion.resultado.datos);
                })
                .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
        }

        private MapearObjeto(nodo: NodoDeSeguridad, datos: any): void {
            this.OcultarDatos(`${ltrVistaDeSeguridad.tituloDelObjeto[nodo.objeto]}: ${nodo.nombre}`);
            let panel: HTMLDivElement = this.PanelDelObjeto(nodo.objeto);
            ApiPanel.BlanquearControlesDeIU(panel, false);
            MapearAlPanel.ElObjeto(panel, datos, ModoAcceso.enumModoDeAccesoDeDatos.Consultor);
            ApiControl.ExcluirCss(panel, ltrCss.divNoVisible);
            this.nodoMostrado = nodo;
            ApiControl.ExcluirCss(this.BotonEditar, ltrCss.divNoVisible);
        }

        private OcultarDatos(titulo: string): void {
            this.TituloDeDatos.textContent = titulo;
            this.nodoMostrado = undefined;
            ApiControl.IncluirCss(this.BotonEditar, ltrCss.divNoVisible);
            ApiControl.IncluirCss(this.PanelDelObjeto(ltrVistaDeSeguridad.objeto.puesto), ltrCss.divNoVisible);
            ApiControl.IncluirCss(this.PanelDelObjeto(ltrVistaDeSeguridad.objeto.rol), ltrCss.divNoVisible);
            ApiControl.IncluirCss(this.PanelDelObjeto(ltrVistaDeSeguridad.objeto.permiso), ltrCss.divNoVisible);
        }
    }
}
