interface IDiccionario<T> {
    _claves: Array<any>;
    _valores: Array<T>;

    Agregar(clave: any, valor: T);
    Quitar(clave: any);
    Contiene(clave: any): boolean;
    Obtener(clave: any): T;
}

class ObjetoDelDiccionario<T> {
    clave: any;
    valor: T;
}

class Diccionario<T> implements IDiccionario<T> {

    _claves = new Array<any>();
    _valores = new Array<T>();

    public get Elementos(): number {
        return this._claves.length;
    }

    constructor(inicilizar?: { clave: any; valor: T; }[]) {
        if (inicilizar) {
            for (var x = 0; x < inicilizar.length; x++) {
                this.Agregar(inicilizar[x].clave, inicilizar[x].valor);
            }
        }
    }

    public Elemento(indice: number): ObjetoDelDiccionario<T> {
        let o = new ObjetoDelDiccionario<T>();
        if (indice > this.Elementos)
            MensajesSe.EmitirExcepcion("Diccionario.Elemento", `Se ha solicitado el elemento ${indice} y el diccionario sólo contiene ${this.Elementos}`);
        o.clave = this.Clave(indice);
        o.valor = this.Valor(indice);
        return o;
    }

    public AgregarElemento(elemento: ObjetoDelDiccionario<T>): void {
        this.Agregar(elemento.clave, elemento.valor);
    }

    public Agregar(clave: any, valor: T) {

        if (!this.Contiene(clave)) {
            this._claves.push(clave);
            this._valores.push(valor);
        }
        else {
            let i: number = this._claves.indexOf(clave);
            this._valores.splice(i, 1, valor);
        }
    }

    public Quitar(clave: any): void {
        var indice = this._claves.indexOf(clave, 0);
        if (indice >= 0) {
            this._claves.splice(indice, 1);
            this._valores.splice(indice, 1);
        }
    }

    public Contiene(clave: any): boolean {
        return this._claves.indexOf(clave) > -1;
    }

    public Obtener(clave: any): T {
        let pos: number = this._claves.indexOf(clave);
        if (pos >= 0)
            return this._valores.slice(pos)[0] as T;

        return undefined;
    }


    public Sacar(clave: any): T {
        let objeto: T = this.Obtener(clave);
        if (objeto !== undefined)
            this.Quitar(clave);

        return objeto;
    }

    public Valor(posicion: number): T {
        if (this._valores.length <= posicion)
            return undefined;

        let clave: any = this.Clave(posicion);
        return this.Obtener(clave);
    }

    public Clave(posicion: number): any {
        if (posicion <= this.Elementos)
            return this._claves.slice(posicion)[0];

        return undefined;
    }


}

function ObjetoToDiccionario<T>(objeto: object): Diccionario<T> {
    let diccionario: Diccionario<T> = new Diccionario<T>();
    for (var i = 0; i < objeto["_claves"].length; i++)
        diccionario.Agregar(objeto["_claves"][i], objeto["_valores"][i]);
    return diccionario;
}

function JsonToDiccionario<T>(json: string): Diccionario<T> {
    let pares: Diccionario<T> = JSON.parse(json);
    return ObjetoToDiccionario<T>(pares);
}

function DiccionarioToJson<T>(diccionario: Diccionario<T>): string {
    var json = [];
    for (var i = 0; i < diccionario._claves.length; i++)
        json.push([diccionario._claves[i], diccionario._valores[i]]);
    return JSON.stringify(json);
}

interface IPila<T> {
    meter(valor: T): void;
    sacar(): T;
}

class Pila<T> implements IPila<T> {
    _pila: T[] = [];

    constructor() {
    }

    meter(valor: T): void {
        this._pila.push(valor);
    }

    sacar(): T {
        let valor: T = this._pila[this._pila.length - 1];
        this._pila.splice(this._pila.length - 1);
        return valor;
    }

}