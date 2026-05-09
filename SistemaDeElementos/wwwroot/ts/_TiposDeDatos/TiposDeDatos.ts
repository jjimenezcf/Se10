namespace Tipos {

    export interface HistoricoIA {
        pregunta: string;
        respuesta: string | null;
    }

    export class OpcionesDeUnaLista {
        private listas: Diccionario<any>;

        constructor() {
            this.listas = new Diccionario<any>();
        }

        private Agregar(id: string, opciones: Diccionario<any>): void {
            this.listas.Agregar(id, opciones);
        }

        private Quitar(id: string): void {
            this.listas.Quitar(id);
        }

        private Obtener(id: string): any {
            return this.listas.Obtener(id);
        }

        public AgregarOpcion(id: string, registro: any): void {
            let opciones = this.Obtener(id);
            if (!Definido(opciones)) opciones = new Diccionario<any>();
            (opciones as Diccionario<any>).Agregar(registro.id, registro);
            this.Agregar(id, opciones);
        }

        public QuitarOpcion(id: string, registro: any): void {
            let opciones = this.Obtener(id);
            if (Definido(opciones)) {
                (opciones as Diccionario<any>).Quitar(registro.id);
                this.Agregar(id, opciones);
            }
        }

        public Vaciar(id: string): void {
            let opciones = this.Obtener(id);
            if (Definido(opciones)) {
                this.Quitar(id);
            }
        }

        public ObtenerObjeto(lista: HTMLInputElement | HTMLSelectElement): any {
            if (!Definido(lista)) return undefined;
            let opciones = this.Obtener(lista.id);
            if (!Definido(opciones)) return undefined;

            let idSeleccionado = lista instanceof HTMLInputElement
                ? Numero(lista.getAttribute(atListasDinamicas.idSeleccionado))
                : Numero(lista.value);

            return (opciones as Diccionario<any>).Obtener(idSeleccionado);
        }
    }


    export class Filtro {
        IdControl: string;
        Tipo: string;
        Atributos: Diccionario<any>;

        constructor(id: string, tipo: string) {
            this.Atributos = new Diccionario<any>();
            this.IdControl = id;
            this.Tipo = tipo;
        }
    }

    export function ClonarFiltro(objeto: Filtro): Tipos.Filtro {
        function clonarAtributos(objeto: Diccionario<any>): Diccionario<any> {
            let atributos = new Diccionario<any>();
            for (let i: number = 0; i < objeto._claves.length; i++)
                atributos.Agregar(objeto._claves[i], objeto._valores[i]);
            return atributos;
        }

        let filtro: Filtro = new Filtro(objeto.IdControl, objeto.Tipo);
        filtro.Atributos = clonarAtributos(objeto.Atributos);
        return filtro;
    }

    export class Orden {
        public IdColumna: string;
        public Propiedad: string;
        private modo: string;
        private ordenadoPor: string;

        get ccsClase(): string {
            if (this.modo === enumModoOrdenacion.ascedente)
                return ltrCss.ordenAscendente;
            if (this.modo === enumModoOrdenacion.descendente)
                return ltrCss.ordenDescendente;
            return ltrCss.sinOrden;
        }

        get OrdenarPor(): string {
            if (IsNullOrEmpty(this.ordenadoPor))
                return this.Propiedad;
            return this.ordenadoPor;
        }

        get Modo(): string {
            return this.modo;
        }

        set Modo(modo: string) {
            this.modo = modo;
        }

        constructor(idcolumna: string, propiedad: string, modo: string, ordenarPor: string) {
            this.Modo = modo;
            this.Propiedad = propiedad;
            this.IdColumna = idcolumna;
            this.ordenadoPor = ordenarPor;
        }
    }

    export class Restrictor {
        public Propiedad: string;
        public Valor: number;
        public Texto: string;
        public SoloFiltra: boolean;

        constructor(propiedad: string, valor: number, texto: string, soloFiltra = false) {
            this.Propiedad = propiedad;
            this.Valor = valor;
            this.Texto = texto;
            this.SoloFiltra = soloFiltra;
        }
    }

    export class ListaDeElemento {
        private lista: HTMLSelectElement;
        get Lista(): HTMLSelectElement {
            return this.lista;
        }

        constructor(idLista: string) {
            this.lista = document.getElementById(idLista) as HTMLSelectElement;
        }

        public Agregar(registro: any, expresion: string): void {
            if (this.EstaLaOpcion(registro.id))
                return;
            this.AgregarOpcion(registro.id, expresion);

            //opcion.setAttribute(atControl.objeto, JSON.stringify(registro));
            OpcionesDeLasListas.AgregarOpcion(this.lista.id, registro);
        }

        public AgregarOpcion(valor: number, texto: string): HTMLOptionElement {
            if (this.EstaLaOpcion(valor))
                return;
            var opcion = document.createElement("option");
            opcion.setAttribute("value", valor.toString());
            opcion.setAttribute("label", texto);
            this.Lista.appendChild(opcion);
            return opcion;
        }

        private EstaLaOpcion(valor: number): boolean {
            for (let i = 0; i < this.Lista.options.length; i++) {
                if (valor === Numero(this.Lista.options[i].value))
                    return true;
            }
            return false;
        }
    }

    export class DatosPeticionLista {
        ClaseDeElemento: string;
        IdLista: string;
        IdFijar: number;

        get Selector(): ListaDeElemento {
            return new ListaDeElemento(this.IdLista);
        }
    }

    export class ListaDinamica {
        private static _Instancias = new Map<string, ListaDinamica>();
        private _IdLista: string;
        private _dropdown: HTMLDivElement | null = null;
        private _opciones: Map<number, { texto: string; data: any }> = new Map();
        private _lista: HTMLInputElement | null = null;

        private _informacion: { cargar: boolean; controlador: string; filtros: Array<ClausulaDeFiltrado>; parametros: Array<Parametro>; datosDeEntrada: string }
        set Informacion(valor: any) {
            this._informacion = valor;
        }

        get Opciones(): Map<number, { texto: string; data: any }> {
            return this._opciones;
        }

        get ListaHtml(): HTMLInputElement {
            if (!Definido(this._lista)) {
                this._lista = document.getElementById(this._IdLista) as HTMLInputElement;
            }
            return this._lista;
        }


        get Dropdown(): HTMLDivElement | null {
            return this._dropdown;
        }


        public get DropdownVisible(): boolean {
            return this._dropdown && this._dropdown.style.display === 'block';
        }

        public get Cargando(): boolean {
            return this.ListaHtml.getAttribute(atListasDinamicas.cargando) === 'S';
        }

        private _ultimaBusqueda: string = null;
        get UltimaBusqueda(): string | null {
            return this._ultimaBusqueda;
        }
        set UltimaBusqueda(valor: string) {
            this._ultimaBusqueda = valor;
        }

        get IdSeleccionado(): number {
            return Numero(this.ListaHtml.getAttribute(atListasDinamicas.idSeleccionado));
        }
        set IdSeleccionado(valor: number) {
            this.ListaHtml.setAttribute(atListasDinamicas.idSeleccionado, valor.toString());
        }

        get IdSelAlEntrar(): number {
            return Numero(this.ListaHtml.getAttribute(atListasDinamicas.idSelAlEntrar));
        }
        set IdSelAlEntrar(valor: number) {
            this.ListaHtml.setAttribute(atListasDinamicas.idSelAlEntrar, valor.toString());
        }

        public static Opciones(lista: HTMLInputElement): Map<number, { texto: string; data: any }> | null {
            const clave = lista.id;
            if (ListaDinamica._Instancias?.has(clave)) {
                const ld: ListaDinamica = ListaDinamica._Instancias.get(clave) as ListaDinamica;
                return ld.Opciones;
            }
            return null;
        }

        public static Obtener(lista: HTMLInputElement): ListaDinamica {

            // Usamos el ID del input (o el valor del atributo 'list') como clave. 
            // Si el input tiene ID, es la mejor clave. Asumimos que tiene ID.
            const clave = lista.id;

            // 1. Intentar recuperar la instancia existente
            if (ListaDinamica._Instancias.has(clave)) {
                return ListaDinamica._Instancias.get(clave) as ListaDinamica;
            }

            // 2. Si no existe, crear y guardar la nueva instancia
            const nuevaInstancia = new ListaDinamica(lista);
            ListaDinamica._Instancias.set(clave, nuevaInstancia);

            return nuevaInstancia;
        }

        public static Resetear(lista: HTMLInputElement): void {
            const clave = lista.id;
            if (ListaDinamica._Instancias?.has(clave)) {
                const ld: ListaDinamica = ListaDinamica._Instancias.get(clave) as ListaDinamica;
                ld.Resetear();
            }
        }

        private constructor(lista: HTMLInputElement) {
            this._IdLista = lista.id; //.getAttribute(atListas.idDeLaLista);
            this._lista = lista;
            this.crearDropdown();
            this.configurarEventos();
        }

        private crearDropdown(): void {
            const dropdownId = `${this._IdLista}-dropdown`;

            // 1. ELIMINAR EL DESPLEGABLE ANTERIOR SI EXISTE (Limpieza Global)
            // Buscamos el elemento por ID en todo el documento. Si existe, lo eliminamos
            // de su padre actual (que será el body si ya fue creado, o el contenedor anterior).
            const dropdownExistente = document.getElementById(dropdownId);
            if (dropdownExistente && dropdownExistente.parentElement) {
                dropdownExistente.parentElement.removeChild(dropdownExistente);
                // console.log(`Dropdown anterior (${dropdownId}) eliminado del DOM.`);
            }

            // 2. CREAR EL NUEVO DESPLEGABLE
            this._dropdown = document.createElement('div');
            this._dropdown.id = dropdownId; // ID único
            this._dropdown.className = 'dropdown-personalizado';

            // Asignación de estilos
            this._dropdown.style.position = 'absolute'; // Necesario para posicionamiento global
            this._dropdown.style.border = '1px solid #ccc';
            this._dropdown.style.background = 'white';
            this._dropdown.style.maxHeight = '200px';
            this._dropdown.style.overflowY = 'auto';
            // Aumentamos el z-index para asegurar que esté sobre todos los modales, etc.
            this._dropdown.style.zIndex = '99999';
            this._dropdown.style.display = 'none'; // Inicialmente oculto
            this._dropdown.style.boxShadow = '0 2px 8px rgba(0,0,0,0.15)';
            this._dropdown.style.borderRadius = '4px';

            // 3. INSERTAR EN EL BODY
            // Al ser hijo directo del body, está FUERA del contexto de apilamiento
            // de las filas de la tabla. Su z-index de 99999 será globalmente efectivo.
            document.body.appendChild(this._dropdown);
        }

        private configurarEventos(): void {
            // Posicionar el dropdown cuando se enfoca
            this.ListaHtml.addEventListener('focus', () => {
                this.posicionarDropdown();
            });

            // Reposicionar al hacer scroll o resize
            const reposicionar = () => {
                if (this._dropdown && this._dropdown.style.display !== 'none') {
                    this.posicionarDropdown();
                }
            };

            window.addEventListener('scroll', reposicionar, true);
            window.addEventListener('resize', reposicionar);

            // Cerrar dropdown al hacer clic fuera
            document.addEventListener('click', (e: MouseEvent) => {
                if (this._dropdown &&
                    !this._dropdown.contains(e.target as Node) &&
                    e.target !== this._lista) {
                    this.ocultarDropdown();
                }
            });

            // Navegación con teclado
            this.ListaHtml.addEventListener('keydown', (e: KeyboardEvent) => {
                if (!this._dropdown || this._dropdown.style.display === 'none') return;

                const opciones = this._dropdown.querySelectorAll('.opcion-dropdown');
                const seleccionada = this._dropdown.querySelector('.opcion-seleccionada');
                let indiceActual = -1;

                if (seleccionada) {
                    indiceActual = Array.from(opciones).indexOf(seleccionada);
                }

                switch (e.key) {
                    case 'ArrowDown':
                        e.preventDefault();
                        this.seleccionarOpcion(opciones, indiceActual + 1);
                        break;
                    case 'ArrowUp':
                        e.preventDefault();
                        this.seleccionarOpcion(opciones, indiceActual - 1);
                        break;
                    case 'Enter':
                        e.preventDefault();
                        if (seleccionada) {
                            (seleccionada as HTMLElement).click();
                        }
                        break;
                    case 'Escape':
                        this.ocultarDropdown();
                        break;
                }
            });
        }

        private seleccionarOpcion(opciones: NodeListOf<Element>, indice: number): void {
            if (indice < 0 || indice >= opciones.length) return;

            // Remover selección anterior
            opciones.forEach(op => op.classList.remove('opcion-seleccionada'));

            // Agregar nueva selección
            const opcion = opciones[indice] as HTMLElement;
            opcion.classList.add('opcion-seleccionada');

            // Hacer scroll si es necesario
            opcion.scrollIntoView({ block: 'nearest', behavior: 'smooth' });
        }

        // Este método se llamará justo antes de this.mostrarDropdown()
        private posicionarDropdown(): void {
            if (this._dropdown) {
                // Obtenemos la posición y dimensiones del INPUT respecto al viewport (ventana)
                const rect = this._lista.getBoundingClientRect();

                // 1. TOP: Posición TOP del input + su altura + el scroll actual de la ventana
                // Usamos window.scrollY para que funcione incluso si la página tiene scroll.
                const topPosition = rect.top + rect.height + window.scrollY;

                // 2. LEFT: Posición LEFT del input + el scroll horizontal (si existe)
                const leftPosition = rect.left + window.scrollX;

                // Aplicar la posición y el ancho
                this._dropdown.style.top = `${topPosition}px`;
                this._dropdown.style.left = `${leftPosition}px`;
                this._dropdown.style.width = `${rect.width}px`;

            }
        }

        public Resetear() {
            this.Vaciar();
            this.ListaHtml.value = '';
            this.IdSelAlEntrar = 0;
            this.IdSeleccionado = 0;
            this.ListaHtml.setAttribute(atListasDinamicas.ultimaCadenaBuscada, '');
            this.ListaHtml.setAttribute(atListasDinamicas.cargando, 'N');
        }

        public AgregarOpcion(valor: number, texto: string, data?: any): HTMLDivElement | null {
            // Verificar si ya existe
            if (this._opciones.has(valor)) {
                return null;
            }

            // Guardar en el mapa
            this._opciones.set(valor, { texto, data });

            // Crear elemento visual
            const opcionDiv = document.createElement('div');
            opcionDiv.className = 'opcion-dropdown';
            opcionDiv.setAttribute(atListas.identificador, valor.toString());
            opcionDiv.textContent = texto;
            opcionDiv.style.padding = '8px 12px';
            opcionDiv.style.cursor = 'pointer';
            opcionDiv.style.transition = 'background-color 0.2s';
            if (valor === -1000) {
                opcionDiv.style.color = '#007bff'; // Azul profesional
                opcionDiv.style.fontWeight = 'bold';
                opcionDiv.style.textAlign = 'center';
                opcionDiv.style.borderTop = '1px solid #eee'; // Separador visual
            }

            // Eventos hover
            opcionDiv.addEventListener('mouseenter', () => {
                opcionDiv.style.background = '#f0f0f0';
            });

            opcionDiv.addEventListener('mouseleave', () => {
                if (!opcionDiv.classList.contains('opcion-seleccionada')) {
                    opcionDiv.style.background = 'white';
                }
            });

            opcionDiv.addEventListener('mousedown', (e) => {
                // 1. Evitar que el input pierda el foco y dispare PerderFoco antes de tiempo.
                e.preventDefault();

                // 2. Ejecutar la lógica de selección de valor inmediatamente.
                // Dado que mousedown no es el evento 'click' final, lo usamos para forzar
                // la ejecución de la lógica que debería estar en el click.
                this.seleccionarValor(valor, texto);
            });

            // Evento click
            opcionDiv.addEventListener('click', () => {
                this.seleccionarValor(valor, texto);
            });

            this._dropdown?.appendChild(opcionDiv);
            return opcionDiv;
        }

        private seleccionarValor(valor: number, texto: string): void {

            if (valor === -1000) {
                this.CargarMas();
                return;
            }

            this._lista.value = texto;
            this._lista.setAttribute(atListasDinamicas.idSeleccionado, valor.toString());

            this.ocultarDropdown();

            // Ejecutar callback si existe
            const trasSeleccionar = this._lista.getAttribute('tras-seleccionar');
            if (trasSeleccionar) {
                Evaluar('seleccionarValor', trasSeleccionar, trasSeleccionar.includes('this') ? this.ListaHtml : undefined);
            }
        }

        private CargarMas() {
            try {
                this.ListaHtml.setAttribute(atListasDinamicas.cargando, 'S');
                const posicion = this._informacion.parametros.find(p => p.parametro === Ajax.Param.posicion);
                if (Definido(posicion))
                    posicion.valor = this._dropdown.children.length - 1;
                else
                    this._informacion.parametros.push(new Parametro(Ajax.Param.posicion, this._dropdown.children.length - 1));
                ApiDePeticiones.LeerElementos(
                    this.ListaHtml,
                    this._informacion.controlador,
                    Ajax.EndPoint.LeerElementos,
                    this._informacion.filtros,
                    this._informacion.parametros,
                    this._informacion.datosDeEntrada
                )
                    .then((peticion) => {
                        const nuevosDatos = peticion.resultado.datos;
                        this.EliminarOpcion(-1000);
                        if (nuevosDatos && nuevosDatos.length > 0) {
                            this.AnadirOpcionesLeidas(nuevosDatos);
                            this.AnadirOpcionLeerMas(nuevosDatos.length);
                        }
                    })
                    .finally(() => {
                        this.ListaHtml.setAttribute(atListasDinamicas.cargando, 'N');
                    });
            }
            catch (error) {
                this.ListaHtml.setAttribute(atListasDinamicas.cargando, 'N');
            }
        }


        public AnadirOpcionLeerMas(leidas: number): void {

            const cantidad = Numero(this._informacion.parametros.find(p => p.parametro === Ajax.Param.cantidad).valor);
            if (cantidad === leidas)
                this.AgregarOpcion(-1000, "Leer más ....", { id: -1000, esAuxiliar: true });
        }

        public AnadirOpcionesLeidas(datos: any) {
            let mostrarExpresion = this.ListaHtml.getAttribute(atListasDinamicas.mostrarExpresion);
            let expresion: string = "";
            for (var i = 0; i < datos.length; i++) {
                expresion = ParsearExpresion(datos[i], mostrarExpresion.toLocaleLowerCase());
                let valor: number = datos[i].id;

                if (NoDefinido(valor))
                    MensajesSe.EmitirMensajeDeExcepcion("Añadir opciones a la lista dinámica", "No se ha definido el ID tras leer elementos en el servidor");

                const opcionDiv: HTMLDivElement = this.AgregarOpcion(valor, expresion, datos[i]);

                if (Definido(opcionDiv)) {
                    OpcionesDeLasListas.AgregarOpcion(this.ListaHtml.id, datos[i]);
                }
            }
        }

        public EliminarOpcion(valor: number): void {
            // 1. Eliminar del Mapa (para que el chequeo 'has' permita volver a añadirlo)
            this._opciones.delete(valor);

            // 2. Eliminar del DOM
            const selector = `[${atListas.identificador}="${valor}"]`;
            const opcionDiv = this._dropdown?.querySelector(selector);

            if (opcionDiv) {
                opcionDiv.remove();
            }
        }

        public BuscarSeleccionado(valor: string): number {
            for (const [id, opcion] of this._opciones) {
                if (opcion.texto === valor) {
                    return id;
                }
            }
            return 0;
        }

        public BuscarPorId(valor: number): string | null {
            // 1. Usar el método 'get' del Map para obtener la estructura { texto, data }
            const opcion = this._opciones.get(valor);

            // 2. Verificar si se encontró la opción
            if (opcion) {
                // 3. Si existe, devolver el valor de la propiedad 'texto'
                return opcion.texto;
            }

            // 4. Si no se encuentra el ID, devolver una cadena vacía (o podrías devolver 'null' si lo prefieres)
            return null;
        }

        public Vaciar(): void {
            this._opciones.clear();
            this.UltimaBusqueda = null;
            if (this._dropdown) {
                this._dropdown.innerHTML = '';
            }
        }

        public MostrarDropdown(): void {
            this.posicionarDropdown();
            this._dropdown.style.display = 'block';
        }

        public ocultarDropdown(): void {
            if (this._dropdown) {
                this._dropdown.style.display = 'none';
            }
        }

        public FiltrarOpciones(textoBusqueda: string): void {
            if (!this._dropdown) return;

            const opciones = this._dropdown.querySelectorAll('.opcion-dropdown');
            let hayVisibles = false;

            opciones.forEach((opcion: HTMLElement) => {
                const texto = opcion.textContent?.toLowerCase() || '';
                const coincide = texto.includes(textoBusqueda.toLowerCase());

                opcion.style.display = coincide ? 'block' : 'none';
                if (coincide) hayVisibles = true;
            });

            // Mostrar mensaje si no hay coincidencias
            if (!hayVisibles) {
                const mensajeExistente = this._dropdown.querySelector('.mensaje-sin-resultados');
                if (!mensajeExistente) {
                    const mensaje = document.createElement('div');
                    mensaje.className = 'mensaje-sin-resultados';
                    mensaje.style.padding = '8px 12px';
                    mensaje.style.color = '#999';
                    mensaje.style.fontStyle = 'italic';
                    mensaje.textContent = 'No se encontraron coincidencias';
                    this._dropdown.appendChild(mensaje);
                }
            } else {
                // Eliminar mensaje si existe
                const mensaje = this._dropdown.querySelector('.mensaje-sin-resultados');
                mensaje?.remove();
            }
        }

        public destruir(): void {
            if (this._dropdown) {
                this._dropdown.remove();
                this._dropdown = null;
            }
            this._opciones.clear();
        }
    }

    export class DatosPeticionDinamica {
        public ClaseDeElemento: string;
        public IdInput: string;
        public buscada: string;
        public criterio: string;
    }

    export class DatosParaRelacionar {
        public idOpcionDeMenu: string;
        public RelacionarCon: string;
        public idSeleccionado: number;
        public PropiedadQueRestringe: string;
        public PropiedadRestrictora: string;
        public MostrarEnElRestrictor: string;
        public FiltroRestrictor: Tipos.Restrictor;

        constructor() {
            this.FiltroRestrictor = null;
        }
    }

    export class DatosParaDependencias {
        public idOpcionDeMenu: string;
        public DatosDependientes: string;
        public idSeleccionado: number;
        public PropiedadQueRestringe: string;
        public PropiedadRestrictora: string;
        public MostrarEnElRestrictor: string;
        public FiltroRestrictor: Array<Tipos.Restrictor>;

        constructor() {
            this.FiltroRestrictor = new Array<Tipos.Restrictor>();
        }
    }

    export class ElementoDtoSimple {
        tipoDto: string;
        idElemento: number;
        referencia: string;

        constructor(dto: string, id: number, texto: string) {
            this.tipoDto = dto;
            this.idElemento = id;
            this.referencia = texto;
        }
    }


    export class DatosPeticionCargarDetalle {
        public idGridDeDetalle: string;
        public controlador: string;
        public accion: string;
        public restrictor: string;

        constructor(idGrid: string, controlador: string, accion: string, restrictor: string) {
            this.idGridDeDetalle = idGrid;
            this.controlador = controlador;
            this.accion = accion;
            this.restrictor = restrictor;
        }

    }

    /**********************************************************************************************************************
     * Objetos para la jerarquía
    **********************************************************************************************************************/
    export class NodoDto {
        public id: number;
        public nombre: string;
        public negocio: string;
        public idPadre: number;
        public activo: boolean;
        public tipoDto: string;
        public tipoDtm: string;
        public modoAcceso: ModoAcceso.enumModoDeAccesoDeDatos;
    }

    export class NodoDeJerarquiaDto {
        public dto: NodoDto;
        public hijos: Array<NodoDeJerarquiaDto> = [];
    }

    export class JerarquiaDto {
        public ramas: Array<NodoDeJerarquiaDto> = [];
    }

    /**********************************************************************************************************************
     * Objetos y métodos para la ordenación
    **********************************************************************************************************************/
    export class Ordenacion {
        private lista: Array<Tipos.Orden>;

        public Count(): number {
            return this.lista.length;
        }

        constructor() {
            this.lista = new Array<Tipos.Orden>();
        }

        private Anadir(idcolumna: string, propiedad: string, modo: string, ordenarPor: string): boolean {
            for (let i = 0; i < this.lista.length; i++) {
                if (this.lista[i].Propiedad === propiedad) {
                    this.lista[i].Modo = modo;
                    return ApiControl.AjustarColumnaDelGrid(this.lista[i]);
                }
            }
            let orden: Tipos.Orden = new Tipos.Orden(idcolumna, propiedad, modo, ordenarPor);

            if (ApiControl.AjustarColumnaDelGrid(orden)) {
                this.lista.push(orden);
                return true;
            }
            return false;
        }


        private Quitar(propiedad: string): boolean {
            for (let i = 0; i < this.lista.length; i++) {
                if (this.lista[i].Propiedad == propiedad) {
                    let orden: Tipos.Orden = this.lista[i] as Tipos.Orden;
                    orden.Modo = enumModoOrdenacion.sinOrden;
                    this.lista.splice(i, 1);
                    return ApiControl.AjustarColumnaDelGrid(orden);
                }
            }
        }

        public Clonar(ordenacion: Tipos.Ordenacion): void {
            this.AnularOrdenacion();
            for (let i: number = 0; i < ordenacion.Count(); i++) {
                this.Actualizar(ordenacion.lista[i].IdColumna, ordenacion.lista[i].Propiedad, ordenacion.lista[i].Modo, ordenacion.lista[i].OrdenarPor);
            }
        }

        public AnularOrdenacion(): void {
            for (let i = this.lista.length - 1; i >= 0; i--) {
                let orden: Tipos.Orden = this.lista[i] as Tipos.Orden;
                orden.Modo = enumModoOrdenacion.sinOrden;
                ApiControl.AjustarColumnaDelGrid(orden);
                this.lista.splice(i, 1);
            }
        }

        public Actualizar(idcolumna: string, propiedad: string, modo: string, ordenarPor: string): boolean {
            if (modo === enumModoOrdenacion.sinOrden)
                return this.Quitar(propiedad);
            else
                return this.Anadir(idcolumna, propiedad, modo, ordenarPor);
        }

        public Sustituir(idcolumna: string, propiedad: string, modo: string, ordenarPor: string): boolean {
            this.AnularOrdenacion();
            if (modo !== enumModoOrdenacion.sinOrden) {
                let orden: Tipos.Orden = new Tipos.Orden(idcolumna, propiedad, modo, ordenarPor);

                if (ApiControl.AjustarColumnaDelGrid(orden)) {
                    this.lista.push(orden);
                    return true;
                }
            }
            return false;
        }

        public Leer(i: number): Tipos.Orden {
            return this.lista[i];
        }

        public LeerPorPropiedad(propiedad: string): Tipos.Orden {
            for (let i = 0; i < this.lista.length; i++) {
                if (this.lista[i].Propiedad == propiedad)
                    return this.lista[i];
            }
        }
    }

    export function DeserializarOrdenacion(ordenacionJson: any): Tipos.Ordenacion {
        let o: Tipos.Ordenacion = new Ordenacion();
        let lista: [] = ordenacionJson["lista"];
        for (let i: number = 0; i < lista.length; i++) {
            o.Actualizar(lista[i]["IdColumna"], lista[i]["Propiedad"], lista[i]["modo"], lista[i]["ordenadoPor"]);
        }

        return o;
    }



}

