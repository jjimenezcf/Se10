namespace HistorialSe {


    function Crear(pagina: string): EstadoPagina {
        let estadoDePagina: EstadoPagina = new EstadoPagina(pagina);
        return estadoDePagina;
    }

    function Constructor<T>(newable: new () => T): T {
        const objeto = new newable();
        return objeto;
    }

    export class EstadoPagina extends Diccionario<any> implements IDiccionario<any> {


        constructor(pagina: string) {
            super();
            this.Agregar(ltrClaveDeEstado.paginaActual, pagina);
        }

        public Guardar(): void {
            EntornoSe.Historial.GuardarEstado(this);
        }

        public ObtenerObjeto<T>(clave: string, newable: any): T {
            let objeto: T = super.Obtener(clave);
            if (NoDefinido(objeto))
                return newable() as T;
            return objeto;
        }


        ///Formato --> propiedad=valor=texto
        private AsignarTerna(ternas: Tipos.Restrictor[], terna: string, soloFiltra: boolean) {
            let partes = terna.split('=');
            let existe = false;
            for (var j = 0; j < ternas.length; j++) {
                if (ternas[j].Propiedad === partes[0]) {
                    ternas[j].Valor = Numero(partes[1]);
                    ternas[j].Texto = partes.length === 3 ? partes[2] : '';
                    existe = true;
                    break;
                }
            }
            if (!existe)
                ternas.push(new Tipos.Restrictor(partes[0], Numero(partes[1]), partes.length === 3 ? partes[2] : '', soloFiltra));
        }

        public ExtraerParametrosDeLaUrl() {
            //Formato -->?restrictor=[IdCg=13=(aa.1)%20Alcaldía][....]
            this.ExtraerParametros(ltrClaveDeEstado.restrictoresUrl, false);

            //Formato --> filtro=[IdCg=13=(aa.1)% 20Alcaldía][idPropiedad=xxx=yyyyy]
            this.ExtraerParametros(ltrClaveDeEstado.filtrosUrl, true);
        }

        private ExtraerParametros(tipoParametro: string, soloFiltro: boolean) {
            let parametro = ObtenerParametroUrl(tipoParametro);
            if (Definido(parametro)) {
                let restrictores: Tipos.Restrictor[] = this.Obtener(tipoParametro) as Tipos.Restrictor[];
                if (!Definido(restrictores))
                    restrictores = new Array<Tipos.Restrictor>();

                let items: Array<string> = ObtenerSubcadenasEntreCorchetes(parametro);
                for (let i: number = 0; i < items.length; i++) {
                    this.AsignarTerna(restrictores, items[i], soloFiltro);
                }
                this.Agregar(tipoParametro, restrictores);
            }
        }
    }

    export class HistorialDeNavegacion {

        private _paginas: Diccionario<EstadoPagina> = undefined;

        public get Paginas(): Diccionario<EstadoPagina> {
            return this._paginas;
        }

        public get Elementos(): number {
            return this._paginas.Elementos;
        }

        constructor() {
            this._paginas = this.leerHistorial();
        }

        public ObtenerEstado(pagina: string): EstadoPagina {
            let estadoDePagina: EstadoPagina = this._paginas.Obtener(pagina);

            if (estadoDePagina === undefined) {
                estadoDePagina = Crear(pagina);
                EntornoSe.Historial.GuardarEstado(estadoDePagina);
            }

            return this.ObjetoToEstadoPagina(pagina, estadoDePagina);
        }

        public Elemento(posicion: number): EstadoPagina {
            let objeto: EstadoPagina = this._paginas.Valor(posicion);
            if (objeto !== undefined) {
                return this.ObjetoToEstadoPagina(this._paginas.Clave(posicion), objeto);
            }
            return undefined;
        }

        public GuardarEstado(estado: EstadoPagina): void {
            let clave: string = estado.Obtener(ltrClaveDeEstado.paginaActual);
            this._paginas.Agregar(clave, estado);
        }

        public GuardarValor(pagina: string, clave: string, valor: any): HistorialSe.EstadoPagina {
            let estado: HistorialSe.EstadoPagina = EntornoSe.Historial.ObtenerEstado(pagina);
            estado.Agregar(clave, valor);
            return estado;
        }

        public Persistir(): void {
            this.cachearHistorial(this._paginas);
        }

        public HayHistorial(pagina: string): boolean {
            return this._paginas.Contiene(pagina);
        }

        private cachearHistorial(historial: Diccionario<EstadoPagina>): string {
            let jsonStringify: string = JSON.stringify(historial);
            sessionStorage.setItem(ltrClaveDeEstado.historial, jsonStringify);
            return jsonStringify;
        }

        private ObjetoToEstadoPagina(pagina: string, objeto: object): EstadoPagina {
            let estadoDeLaPagina: EstadoPagina = new EstadoPagina(pagina);
            for (var i = 0; i < objeto["_claves"].length; i++)
                estadoDeLaPagina.Agregar(objeto["_claves"][i], objeto["_valores"][i]);
            return estadoDeLaPagina;
        }

        private leerHistorial(): Diccionario<EstadoPagina> {
            let _historialJson: string = sessionStorage.getItem(ltrClaveDeEstado.historial);

            if (IsNullOrEmpty(_historialJson)) {
                var a = new Diccionario<EstadoPagina>();
                _historialJson = this.cachearHistorial(a);
            }
            let diccionario: Diccionario<EstadoPagina> = JsonToDiccionario(_historialJson);

            return diccionario;
        };

    }


}
