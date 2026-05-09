
let OpcionesDeLasListas: Tipos.OpcionesDeUnaLista = new Tipos.OpcionesDeUnaLista();

function AlturaCabeceraPnlControl(): number {
    let cabecera: HTMLDivElement = document.getElementById("cabecera-de-pagina") as HTMLDivElement;
    return cabecera.getBoundingClientRect().height;
}

function AlturaPiePnlControl(): number {
    let pie: HTMLDivElement = document.getElementById("pie-de-pagina") as HTMLDivElement;
    return pie.getBoundingClientRect().height;
}

function AlturaFormulario() {
    return document.defaultView.innerHeight;
}

function AlturaDelCuerpo(): number {
    let cuerpo: HTMLDivElement = document.getElementById("cuerpo-de-pagina") as HTMLDivElement;
    return cuerpo.getBoundingClientRect().height;
}

function AlturaDelGrid(): number {
    let grid: HTMLDivElement = (document.getElementsByClassName(ltrCss.crud.grid.grid)[0]) as HTMLDivElement;
    return grid.getBoundingClientRect().height;
}

function AlturaDelMenu(): number {
    return AlturaDelCuerpo() - 4;
}

let intervaloContador: any = null; // Guardará el setInterval
let segundosTranscurridos: number = 0;

function PonerCapa() {
    let capa: HTMLDivElement = document.getElementById("CapaDeBloqueo") as HTMLDivElement;
    if (capa != null) {
        let numero: number = Numero(capa.getAttribute('numero-de-capas'));

        if (numero <= 0) {
            ApiControl.ExcluirCss(capa, ltrCss.sinCapaDeBloqueo);
            ApiControl.IncluirCss(capa, ltrCss.conCapaDeBloqueo);

            // Inyectamos el span pero vacío (sin el 0)
            capa.innerHTML = '<span id="TextoContador"></span>';

            segundosTranscurridos = 0;

            intervaloContador = setInterval(() => {
                // Primero sumamos el segundo
                segundosTranscurridos++;

                // Luego lo mostramos (así el primer valor que verá el usuario es 1)
                const span = document.getElementById("TextoContador");
                if (span) {
                    span.innerText = segundosTranscurridos.toString();
                }
            }, 1000);

            numero = 0;
        }

        numero = numero + 1;
        capa.setAttribute('numero-de-capas', numero.toString());
    }
}

function QuitarCapa() {
    let capa: HTMLDivElement = document.getElementById("CapaDeBloqueo") as HTMLDivElement;
    let spanContador: HTMLElement = document.getElementById("TextoContador") as HTMLElement;

    if (capa != null) {
        let numero: number = Numero(capa.getAttribute('numero-de-capas'));

        if (numero <= 1) {
            // --- ÚLTIMA PETICIÓN: LIMPIAR TODO ---
            ApiControl.ExcluirCss(capa, ltrCss.conCapaDeBloqueo);
            ApiControl.IncluirCss(capa, ltrCss.sinCapaDeBloqueo);
            numero = 1;

            // Detener cronómetro y limpiar texto
            clearInterval(intervaloContador);
            intervaloContador = null;
            if (spanContador) spanContador.innerText = "";
        }

        numero = numero - 1;
        capa.setAttribute('numero-de-capas', numero.toString());
    }
}

function PonerCapaOld() {
    let capa: HTMLDivElement = document.getElementById("CapaDeBloqueo") as HTMLDivElement;
    if (capa != null) {
        let numero: number = Numero(capa.getAttribute('numero-de-capas'));
        if (numero <= 0) {
            ApiControl.ExcluirCss(capa, ltrCss.sinCapaDeBloqueo);
            ApiControl.IncluirCss(capa, ltrCss.conCapaDeBloqueo);
            numero = 0;
        }
        numero = numero + 1;
        capa.setAttribute('numero-de-capas', numero.toString());
    }
}

function QuitarCapaOld() {
    let capa: HTMLDivElement = document.getElementById("CapaDeBloqueo") as HTMLDivElement;
    if (capa != null) {
        let numero: number = Numero(capa.getAttribute('numero-de-capas'));
        if (numero <= 1) {
            ApiControl.ExcluirCss(capa, ltrCss.conCapaDeBloqueo);
            ApiControl.IncluirCss(capa, ltrCss.sinCapaDeBloqueo);
            numero = 1;
        }
        numero = numero - 1;
        capa.setAttribute('numero-de-capas', numero.toString());
    }
}

function StringBuilder(value) {
    this.strings = new Array();
    this.append(value);
}

StringBuilder.prototype.append = function (value) {
    if (value) {
        this.strings.push(value);
    }
};

StringBuilder.prototype.appendLine = function (value) {
    if (value) {
        this.strings.push(value + newLine);
    }
};

StringBuilder.prototype.clear = function () {
    this.strings.length = 0;
};

StringBuilder.prototype.toString = function () {
    return this.strings.join("");
};


function ObtenerIdDeLaFilaChequeada(idCheck) {
    return obtenerValorDeLaColumnaChequeada(idCheck, "id");
}


function obtenerValorDeLaColumnaChequeada(idCheck, columna) {
    let inputId: HTMLInputElement = document.getElementById(idCheck.replace(`.${ltrMantenimiento.CheckDeSeleccion}`, `.${columna}`)) as HTMLInputElement;
    return inputId.value;
}

function EsString(obj: any): boolean {
    try {
        var a = Object.prototype.toString.call(obj).match(/\s([a-z|A-Z]+)/)[1].toLowerCase() === 'string';
        return a;
    }
    catch {
        return false;
    }
}


function EsError(obj: any): boolean {
    try {
        var a = Object.prototype.toString.call(obj).match(/\s([a-z|A-Z]+)/)[1] === 'Error';
        return a;
    }
    catch {
        return false;
    }
}

function EsBool(obj: any): boolean {
    try {
        var a = Object.prototype.toString.call(obj).match(/\s([a-z|A-Z]+)/)[1].toLowerCase() === 'boolean';
        return a;
    }
    catch {
        return false;
    }
}

function EsNumero(obj: any): boolean {
    try {
        var a = Object.prototype.toString.call(obj).match(/\s([a-z|A-Z]+)/)[1].toLowerCase() === 'number';
        return a;
    }
    catch {
        return false;
    }
}

function EsNumeroNoNulo(obj: any) {
    return !IsNullOrEmpty(obj) && !isNaN(obj);
}

function EsDecimal(obj: any): boolean {
    if (EsNumeroNoNulo(obj)) {
        return Numero(Numero(obj).toFixed()) !== obj;
    }
    return false;
}

function EsFecha(obj: any): boolean {
    try {
        var a = Object.prototype.toString.call(obj).match(/\s([a-z|A-Z]+)/)[1].toLowerCase() === 'date';
        return a;
    }
    catch {
        return false;
    }
}

function EsNulo(objeto: any): boolean {
    if (objeto == null)
        return true;

    return false;
}

function Definido(valor: any) {
    if (EsNulo(valor) || valor === undefined)
        return false;
    return !NoDefinido(valor);
};

function NoDefinido(valor: any) {

    if (EsFecha(valor) && !EsNulo(valor)) {
        let f: any = new Date(null);
        return valor.getTime() == 0 || valor.getFullYear() <= 1900 || isNaN(valor - f);
    }

    if (EsNulo(valor) || valor === undefined)
        return true;

    if (EsString(valor) && valor === '')
        return true;

    return false;
};

function FormatearFecha(fecha: Date, formato: string = enumFormato.FechaHora): string {
    if (NoDefinido(fecha))
        return null;
    formato = formato.replace('yyyy', fecha.getFullYear().toString().padStart(2, "0"));
    formato = formato.replace('MM', (fecha.getMonth() + 1).toString().padStart(2, "0"));
    formato = formato.replace('dd', fecha.getDate().toString().padStart(2, '0'));
    formato = formato.replace('HH', fecha.getHours().toString().padStart(2, "0"));
    formato = formato.replace('mm', fecha.getMinutes().toString().padStart(2, "0"));
    formato = formato.replace('ss', fecha.getSeconds().toString().padStart(2, "0"));
    return formato;
}


//function AsignarValor(input: HTMLInputElement, valor: string) {
//    let formato = input.getAttribute(atControl.formato);
//    if (!IsNullOrEmpty(formato) && EsNumeroNoNulo(valor))
//        valor = FormatearNumero(Number(valor), formato);
//    input.value = valor;
//}


//function AsignarNumero(input: HTMLInputElement, valor: number) {
//    let formato = input.getAttribute(atControl.formato);
//    let valorFormateado: string = "";
//    if (EsNumeroNoNulo(valor)) 
//        valorFormateado = FormatearNumero(valor as number, formato)
//    else {
//            valorFormateado = "0" as string;
//    }
//    input.value = valorFormateado;
//}

function AsignarValorConResalto(input: HTMLInputElement, valor: string | number, resalto: string): void {
    AsignarValor(input, valor);
    ApiControl.ResaltarControl(input, resalto)
}

function AsignarValor(input: HTMLInputElement, valor: string | number): void {

    if (!Definido(valor)) {
        input.value = "";
        return;
    }

    const formato = input.getAttribute(atControl.formato);
    if (formato === enumFormato.base64) {
        input.value = valor.toString();
        return;
    }

    if (typeof valor === 'string') {
        if (!IsNullOrEmpty(formato) && EsNumeroNoNulo(valor))
            valor = FormatearNumero(Number(valor), formato);
        input.value = valor;
    }
    else {
        let valorFormateado: string = "";
        if (EsNumeroNoNulo(valor))
            valorFormateado = Definido(formato) ? FormatearNumero(valor as number, formato) : valor.toString();
        else {
            valorFormateado = "0" as string;
        }
        input.value = valorFormateado;
    }
}




function FormatearNumero(numero: number, formato: string): string {

    let formateado: string;
    if (formato.toLowerCase().trim() === enumFormato.Moneda) {
        const formatter = new Intl.NumberFormat('es-ES',
            {
                style: 'currency',
                currency: 'EUR',
                minimumFractionDigits: 2,
                maximumFractionDigits: 2,
            });
        formateado = formatter.format(numero);
        if (numero > 999 && numero < 10000 && formateado.indexOf('.') < 0) {
            formateado = `${formateado.substring(0, 1)}.${formateado.substring(1)}`;
        }
    }
    else if (formato.toLowerCase().trim() === enumFormato.Porcentaje) {
        const formatter = new Intl.NumberFormat('es-ES', { style: 'percent', minimumFractionDigits: 2, maximumFractionDigits: 2 });
        formateado = formatter.format(numero / 100);
    }
    else if (formato.startsWith(enumFormato.Numero)) {
        if (Number.isInteger(numero)) {
            formateado = numero.toString();
        }
        else {
            let decimales = Numero(formato.split('.')[1]);
            let ajuste = Math.pow(10, decimales);
            numero = Math.round(Number(numero) * ajuste) / ajuste;
            let pts = numero.toString().split(".");
            formateado = pts[0].replace(/\B(?=(\d{3})+(?!\d))/g, ".") + (pts[1] ? "," + pts[1] : "");
        }
    }
    else MensajesSe.EmitirExcepcion('FormatearNumero', `No está definido el formato: ${formato}`);

    return formateado;
}

function IsNullOrEmpty(valor: string): boolean {
    return NoDefinido(valor);
}

function ToLista(cadena: string, separador: string = ';'): Array<string> {
    let resultado = new Array<string>();

    if (IsNullOrEmpty(cadena))
        return resultado;

    let subcadenas = cadena.split(`${separador}`);
    for (let i = 0; i < subcadenas.length; i++) {
        if (!IsNullOrEmpty(subcadenas[i])) {
            resultado.push(subcadenas[i].replace("\n", "").trim());
        }
    }
    return resultado;
}

function PadLeft(cadena: string, rellenarCon: string): string {

    if (cadena == null || NoDefinido(cadena))
        return rellenarCon;
    return (rellenarCon + cadena).slice(-rellenarCon.length);
}

function FechaValida(fecha: Date): Date {
    if (EsFechaValida(fecha)) return fecha;
    return undefined;
}

function FechaToLocalIso(fecha: Date) {
    const pad = (n: number) => String(n).padStart(2, '0');

    // Extraer componentes locales
    const horaLocal = [
        fecha.getHours(),
        fecha.getMinutes(),
        fecha.getSeconds(),
        fecha.getMilliseconds()
    ].map(n => pad(n)).join(':'); // Formato: "HH:mm:ss:SSS"

    // Calcular offset de zona horaria
    const offsetMinutos = fecha.getTimezoneOffset();
    const offsetHoras = Math.abs(Math.floor(offsetMinutos / 60));
    const offsetMinutosResto = Math.abs(offsetMinutos % 60);
    const signo = offsetMinutos > 0 ? '-' : '+';

    // Generar cadena ISO con offset
    const isoConOffset = FechaToIso(fecha, horaLocal)
        .replace('Z', `${signo}${pad(offsetHoras)}:${pad(offsetMinutosResto)}`);

    return isoConOffset;
}

function FechaToIso(fecha: Date, hora: string) {
    var fechaIso = fecha.toISOString();
    var oraIso = '00:00:00.000Z'
    if (!IsNullOrEmpty(hora)) {
        var partes = hora.split(':');
        oraIso = partes[0].length == 1 ? '0' + partes[0] : partes[0];
        oraIso = `${oraIso}:${(partes.length > 1 ? partes[1] : '00')}`;
        oraIso = `${oraIso}:${(partes.length > 2 ? partes[2] : '00')}`;
        oraIso = `${oraIso}.${(partes.length > 3 ? partes[3] : '000')}Z`;
    }
    return fechaIso.substring(0, fechaIso.indexOf('T')) + 'T' + oraIso;
}


function FechaToIsoMostrada(fecha: Date, hora: string) {
    var fechaIso = fecha.toISOString();
    var oraIso = '00:00:00.000Z'
    if (!IsNullOrEmpty(hora)) {
        var partes = hora.split(':');
        oraIso = partes[0].length == 1 ? '0' + partes[0] : partes[0];
        oraIso = `${oraIso}:${(partes.length > 1 ? partes[1] : '00')}`;
        oraIso = `${oraIso}:${(partes.length > 2 ? partes[2] : '00')}`;
        oraIso = `${oraIso}.${(partes.length > 3 ? partes[3] : '000')}`;
    }
    return fechaIso.substring(0, fechaIso.indexOf('T')) + 'T' + oraIso;
}

function Fecha_ToString(fecha: Date, valorSiNoEsValida: string = "1970-01-01 00:00:00"): string {
    if (EsFechaValida(fecha)) {
        let dia: number = fecha.getDate();
        let mes: number = fecha.getMonth() + 1;
        let ano: number = fecha.getFullYear();
        return `${ano}-${PadLeft(mes.toString(), "00")}-${PadLeft(dia.toString(), "00")}`;
    }

    return valorSiNoEsValida;
}

function EsFechaValida(fecha: Date | string): boolean {
    if (fecha === undefined || fecha === null)
        return false;

    let date: Date = typeof fecha === 'string' ? new Date(fecha) : fecha;

    if (isNaN(date.getTime()))
        return false;

    return true;
}

function AsignarTiempo(fecha: Date, tiempo: string[], incrementarHoras: number = 0): void {
    fecha.setHours(Numero(tiempo[0]) + incrementarHoras);
    fecha.setMinutes(Numero(tiempo[1]));
    fecha.setSeconds(Numero(tiempo[2]));
    fecha.setMilliseconds(0);
}

function EsMayorDeCero(valor: any): boolean {

    return Numero(valor) > 0;
}

function Importe(valor: string, emitirError: boolean, evaluarImporte: boolean = true): number {
    var numero = evaluarImporte ? EvaluarImporte(valor, emitirError) : Numero(valor, emitirError);
    return numero;
}

function EvaluarImporte(valor: string, emitirError: boolean): number {
    // Función auxiliar para determinar si un carácter es un operador
    function esOperador(char: string, index: number, expr: string): boolean {
        if (char === '-') {
            // No es operador si está al inicio o después de otro operador
            return index > 0 && !esOperador(expr[index - 1], index - 1, expr);
        }
        return ['+', '*', '/'].includes(char);
    }


    // Función para aplicar una operación
    function aplicarOperacion(a: number, b: number, op: string): number {
        switch (op) {
            case '+': return a + b;
            case '-': return a - b;
            case '*': return a * b;
            case '/': return a / b;
            default: throw new Error('Operador no válido');
        }
    }

    // Función para evaluar una expresión sin paréntesis
    function evaluarExpresionSimple(expr: string): number {
        let numeros: number[] = [];
        let operadores: string[] = [];
        let numeroActual = '';

        for (let i = 0; i < expr.length; i++) {
            if (esOperador(expr[i], i, expr)) {
                if (numeroActual !== '') {
                    numeros.push(Numero(numeroActual, emitirError));
                    numeroActual = '';
                }
                operadores.push(expr[i]);
            } else {
                numeroActual += expr[i];
            }
        }

        if (numeroActual !== '') {
            numeros.push(Numero(numeroActual, emitirError));
        }

        // Realizar las operaciones
        for (let i = 0; i < operadores.length; i++) {
            if (operadores[i] === '*' || operadores[i] === '/') {
                numeros[i] = aplicarOperacion(numeros[i], numeros[i + 1], operadores[i]);
                numeros.splice(i + 1, 1);
                operadores.splice(i, 1);
                i--;
            }
        }

        let resultado = numeros[0];
        for (let i = 0; i < operadores.length; i++) {
            resultado = aplicarOperacion(resultado, numeros[i + 1], operadores[i]);
        }

        return resultado;
    }

    // Función principal para evaluar la expresión
    function evaluarExpresion(expr: string): number {
        // Eliminar espacios en blanco
        expr = expr.replace(/\s/g, '');

        // Buscar el paréntesis más interno
        while (expr.includes('(')) {
            expr = expr.replace(/\(([^()]+)\)/g, (match, p1) => {
                return evaluarExpresionSimple(p1).toString();
            });
        }

        return evaluarExpresionSimple(expr);
    }

    return evaluarExpresion(valor);
}


function Numero(valor: any, emitirError: boolean = false, simboloDecimal: string = ','): number {
    if (valor === null || valor === undefined)
        return 0;

    if (EsString(valor)) {
        let simboloDeMillares = simboloDecimal === ',' ? '.' : ',';

        let roturaPorDecimales = valor.split(simboloDecimal)
        let roturaPorEntero = valor.split(simboloDeMillares)
        if (roturaPorDecimales.length === 1 && roturaPorEntero.length === 2 && roturaPorEntero[1].length != 3) {
            let a = simboloDeMillares;
            simboloDeMillares = simboloDecimal;
            simboloDecimal = a;
        } else if (roturaPorDecimales.length === 1 && roturaPorEntero.length === 2 && roturaPorEntero[1].length == 3) {
            if ((Number(roturaPorEntero[0] + roturaPorEntero[1]) / 1000).toLocaleString() !== valor) {
                let a = simboloDeMillares;
                simboloDeMillares = simboloDecimal;
                simboloDecimal = a;
            }
        }

        let valorPasado = valor;
        valor = valor.replace('€', '').trim();
        if (valor.indexOf('%') > 0) {
            valor = ReplaceAll(valor, '%', '');
            valor = ReplaceAll(valor, simboloDeMillares, simboloDecimal).trim();
        }
        let posicionDeUltimoSimboloMillar = valor.lastIndexOf(simboloDeMillares);
        let posicionDeUltimoSimboloDecimal = valor.lastIndexOf(simboloDecimal);

        if (posicionDeUltimoSimboloDecimal === -1 && posicionDeUltimoSimboloMillar === -1)
            return RetornarValor(valorPasado, valor, '', '', emitirError);

        let simbolosDeMillaresEncontrados = valor.split('.').length - 1;
        let simbolosDecimalesEncontrados = valor.split(simboloDecimal).length - 1;

        if (!(simbolosDeMillaresEncontrados === 1 && simbolosDecimalesEncontrados == 0 && valor.length == 5)) {
            simboloDecimal = posicionDeUltimoSimboloDecimal > posicionDeUltimoSimboloMillar ? simboloDecimal : simboloDeMillares;
            simboloDeMillares = posicionDeUltimoSimboloDecimal > posicionDeUltimoSimboloMillar ? simboloDeMillares : simboloDecimal;
        }

        if ((posicionDeUltimoSimboloDecimal === -1 && simbolosDeMillaresEncontrados === 1) || (posicionDeUltimoSimboloMillar === -1 && simbolosDecimalesEncontrados === 1))
            return RetornarValor(valorPasado, valor, simboloDecimal, simboloDeMillares, emitirError);

        valor = ReplaceAll(valor, simboloDeMillares, '');
        return RetornarValor(valorPasado, valor, simboloDecimal, simboloDeMillares, emitirError);
    }

    if (EsBool(valor))
        if (valor)
            return 1;
        else
            return 0;

    if (EsNumero(valor))
        return valor;

    if (isNaN(valor))
        valor;

    return 0;
}

function ReplaceAll(texto: string, p1: string, p2: string): string {
    return texto.split(p1).join(p2);
}


function RetornarValor(valorPasado: string, valor: string, simboloDecimal: string, simboloDeMiles: string, emitirError: boolean): number {
    if (isNaN(Number(valor))) {
        valor = ReplaceAll(valor, simboloDecimal, simboloDeMiles);
        if (isNaN(Number(valor))) {
            if (!emitirError)
                return 0;
            MensajesSe.EmitirExcepcion('RetornarValor', `El valor numérico '${valorPasado}' no tiene un formato válido`);
        }
        else {
            return Number(valor);
        }
    }
    return Number(valor.replace(simboloDeMiles, ''));
}

//function RetornarValor(valorPasado: string, valor: string, simboloDecimal: string, simboloDeMiles: string, emitirError: boolean): number {
//    // Escapa el símbolo de miles para RegExp
//    const simboloDeMilesEscapado = simboloDeMiles.replace(/[-\/\\^$*+?.()|[\]{}]/g, '\\$&');
//    // Elimina todos los separadores de miles
//    let valorNormalizado = valor.replace(new RegExp(simboloDeMilesEscapado, 'g'), '');
//    // Sustituye el separador decimal por punto
//    valorNormalizado = valorNormalizado.replace(simboloDecimal, '.');

//    if (isNaN(Number(valorNormalizado))) {
//        if (!emitirError)
//            return 0;
//        MensajesSe.EmitirExcepcion('RetornarValor', `El valor numérico '${valorPasado}' no tiene un formato válido`);
//    }
//    return Number(valorNormalizado);
//}


function EsTrue(valor: any): boolean {
    if (valor === undefined || valor === null)
        return false;

    if (EsString(valor))
        return (valor as string).toLocaleLowerCase() === 's' || (valor as string).toLocaleLowerCase() === 'true' || (valor as string).toLocaleLowerCase() === 't';

    if (EsBool(valor))
        return valor;

    if (EsNumero(valor))
        return (valor as number) > 0;

    return false;
}

function EsObjetoDe(objeto, constructor) {
    while (objeto != null) {
        if (objeto == constructor.prototype)
            return true;
        objeto = Object.getPrototypeOf(objeto);
    }
    return false;
}

function EsCorreoValido(email) {
    var re = /^(([^<>()[\]\\.,;:\s@\"]+(\.[^<>()[\]\\.,;:\s@\"]+)*)|(\".+\"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
    return re.test(email);
}


async function LeerFicheroDeUna(url: string): Promise<string> {
    const response = await fetch(url);
    if (!response.ok) {
        throw new Error(`No se pudo leer el archivo desde la URL: ${url}`);
    }
    return await response.text();
}

function EsJsonValido(str): { esValido: boolean, json: JSON } {
    try {
        const json = JSON.parse(str);
        return { esValido: true, json: json };
    } catch (e) {
        return { esValido: false, json: undefined };
    }
}

function EsJson(jsonString: string): boolean {
    try {
        JSON.parse(jsonString);
        return true;
    } catch {
        return false;
    }
}

function obtenerExtension(nombreFichero: string): string {
    return nombreFichero.slice(nombreFichero.lastIndexOf(".")).toLowerCase();
}

function EscapeHtml(unsafe: string): string {
    return unsafe
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;")
        .replace(/'/g, "&#039;")
        .replace(/`/g, "&#x60;")
        .replace(/\//g, "&#x2F;")
        .replace(/\\/g, "&#x5C;")
        .replace(/=/g, "&#x3D;")
        .replace(/\n/g, "&#x0A;")
        .replace(/\r/g, "&#x0D;")
        .replace(/\u2028/g, "&#x2028;")
        .replace(/\u2029/g, "&#x2029;")
        .replace(/\u0085/g, "&#x85;")
        .replace(/[\u0000-\u001F\u007F-\u009F]/g, (c) => `&#x${c.charCodeAt(0).toString(16).padStart(2, '0')};`);
}
function UnEscapeHtml(escaped: string): string {
    return escaped
        .replace(/&#x5C;/g, "\\") // Backslash
        .replace(/&#x2F;/g, "/")  // Slash
        .replace(/&#x60;/g, "`")  // Backtick
        .replace(/&#x3D;/g, "=")  // Equal sign
        .replace(/&#x0A;/g, "\n") // Line feed
        .replace(/&#x0D;/g, "\r") // Carriage return
        .replace(/&#x2028;/g, "\u2028") // Line separator
        .replace(/&#x2029;/g, "\u2029") // Paragraph separator
        .replace(/&#x85;/g, "\u0085")   // Next line (NEL)
        .replace(/&#039;/g, "'")       // Single quote
        .replace(/&quot;/g, "\"")      // Double quote
        .replace(/&lt;/g, "<")         // Less than
        .replace(/&gt;/g, ">")         // Greater than
        .replace(/&amp;/g, "&")        // Ampersand
        .replace(/&#x([0-9A-Fa-f]{2});/g, (match, hex) => String.fromCharCode(parseInt(hex, 16))) // Control chars
        .replace(/&#([0-9]+);/g, (match, dec) => String.fromCharCode(parseInt(dec, 10)));       // Decimal entities
}

function LimpiarNif(nif): string {
    if (!Definido(nif))
        return;
    return nif.replace(/[\s.-]/g, "");
}

function EsRenderizable(nombreFichero: string): boolean {
    const extensionesPermitidas = [".pdf", ".png", ".jpg", ".jpeg", ".xml", ".rtf", ".txt", ".docx", ".json", ".xlsx", ".zip", ".7z", ".html", ".csv", ".bat", ".sql"];
    const extension = obtenerExtension(nombreFichero);
    return extensionesPermitidas.includes(extension);
}
function ObtenerPropiedadJson(json: any, ruta: string): any {
    const partes = ruta.toLowerCase().split('.');
    let resultado = json;

    for (const parte of partes) {
        if (parte.includes('[') && parte.includes(']')) {
            const [nombreArray, indiceStr] = parte.split('[');
            const indice = parseInt(indiceStr.replace(']', ''), 10);
            const propArray = Object.keys(resultado).find(key => key.toLowerCase() === nombreArray);
            if (propArray && Array.isArray(resultado[propArray]) && resultado[propArray][indice] !== undefined) {
                resultado = resultado[propArray][indice];
            } else {
                return undefined;
            }
        } else {
            const prop = Object.keys(resultado).find(key => key.toLowerCase() === parte);
            if (resultado && prop) {
                resultado = resultado[prop];
            } else {
                return undefined;
            }
        }
    }

    return resultado;
}

function Json_BuscarValorEn(propiedad: string, valorPropiedadJson: any): any {
    var tipoDeObjeto = typeof valorPropiedadJson;
    if (tipoDeObjeto === "object") {
        for (var p in valorPropiedadJson) {
            if (propiedad.toLowerCase() === p.toLowerCase())
                return valorPropiedadJson[p];
        }
    }
    return null;
}

async function GuardarMenuAccedido(idVistaMvc: string, parametros: string, urlAccedida: string) {

    const params2 = {
        [ltrPropiedades.Entorno.Usuario.id]: Encriptar(literal.ClaveDeEncriptacion, Registro.UsuarioConectado().id),
    };

    const url2 = `/${ltrControladores.Entorno.ArbolDeMenu}/${Ajax.EndPoint.Entorno.ArbolMenu.GuardarMenuAccedido}?${new URLSearchParams(params2)}`;


    const response = await fetch(url2, {
        method: 'POST',
        body: DatosDeAccesoAlMenu(idVistaMvc, parametros, urlAccedida),
        keepalive: true
    });

    if (!response.ok) {
        MensajesSe.Error('GuardarMenuAccedido', `Error al ejecutar GuardarMenuAccedido: ${response.status} - ${response.statusText}`);
        return;
    }
}

function DatosDeAccesoAlMenu(idVistaMvc: string, parametros: string, urlAccedida: string) {
    let datos: Array<Parametro> = new Array<Parametro>();
    datos.push(new Parametro(Ajax.Param.idVista, idVistaMvc));
    datos.push(new Parametro(literal.parametros, parametros));
    datos.push(new Parametro(literal.urlAccedida, urlAccedida));
    return JSON.stringify(datos);
}

async function GuardarRegistroAccedido(negocio: string, idVistaMvc: number, idElemento: number, urlAccedida: string) {

    if (negocio === enumNegocio.No_Definido) return;

    const params2 = {
        [ltrPropiedades.Entorno.Usuario.id]: Encriptar(literal.ClaveDeEncriptacion, Registro.UsuarioConectado().id),
    };

    const url2 = `/${ltrControladores.Entorno.ArbolDeMenu}/${Ajax.EndPoint.Entorno.ArbolMenu.GuardarRegistroAccedido}?${new URLSearchParams(params2)}`;


    const response = await fetch(url2, {
        method: 'POST',
        body: DatosDeAccesoAlRegistro(negocio, idVistaMvc, idElemento, urlAccedida),
        keepalive: true
    });

    if (!response.ok) {
        MensajesSe.Error('GuardarMenuAccedido', `Error al ejecutar GuardarMenuAccedido: ${response.status} - ${response.statusText}`);
        return;
    }
}



function DatosDeAccesoAlRegistro(enumNegocio: string, idVistaMvc: number, idElemento: number, urlAccedida: string) {
    let datos: Array<Parametro> = new Array<Parametro>();
    datos.push(new Parametro(Ajax.Param.idVista, idVistaMvc));
    datos.push(new Parametro(Ajax.Param.idElemento, idElemento));
    datos.push(new Parametro(Ajax.Param.enumNegocio, enumNegocio));
    datos.push(new Parametro(literal.urlAccedida, urlAccedida));
    return JSON.stringify(datos);
}

class ClausulaDeFiltrado {
    clausula: string;
    criterio: string;
    valor: any;

    constructor(clausula: string, criterio: string, valor: any) {
        this.clausula = clausula;
        this.criterio = criterio;
        this.valor = valor;
    }

    EsVacia(): boolean {
        return NoDefinido(this.clausula) || NoDefinido(this.valor) || NoDefinido(this.criterio);
    }
}

class Parametros {
    _parametros: Array<Parametro>;

    public get Parametros(): Array<Parametro> {
        return this._parametros;
    }


    constructor(parametros?: Parametro[]) {
        if (Definido(parametros))
            this._parametros = parametros;
        else
            this._parametros = new Array<Parametro>();
    }

    public ObtenerValorDeParametro(parametro: string): any {
        for (let i: number = 0; i < this._parametros.length; i++) {
            let p = this._parametros[i];
            if (parametro.toLowerCase() !== p.parametro.toLowerCase())
                continue;
            return p.valor;
        }
        return undefined;
    }

    public Copiar(parametros: Parametros) {
        for (let i = 0; i < parametros.Parametros.length; i++) {
            this.push(parametros.Parametros[i]);
        }
    }

    public add(clave: string, valor: any) {
        const claveNormalizada = clave.toLowerCase();
        const parametroExistente = this._parametros.find(p => p.parametro.toLowerCase() === claveNormalizada);

        if (parametroExistente) {
            // Si el parámetro ya existe, actualiza su valor
            parametroExistente.valor = valor;
        } else {
            // Si el parámetro no existe, añade uno nuevo
            this._parametros.push(new Parametro(clave, valor));
        }
    }
    public push(parametro: Parametro) {
        const claveNormalizada = parametro.parametro.toLowerCase();
        const parametroExistente = this._parametros.find(p => p.parametro.toLowerCase() === claveNormalizada);

        if (parametroExistente) {
            // Si el parámetro ya existe, actualiza su valor
            parametroExistente.valor = parametro.valor;
        } else {
            // Si el parámetro no existe, añade el nuevo
            this._parametros.push(parametro);
        }
    }


}

class Parametro {
    parametro: string;
    valor: any;

    constructor(parametro: string, valor: any) {
        this.parametro = parametro;
        this.valor = valor;
    }
}

function RemplazarParametro(parametros: Parametro[], parametro: string, valor: any): void {
    for (let i = 0; i < parametros.length; i++) {
        if (parametros[i].parametro === parametro) {
            parametros.splice(i, 1);
            break;
        }
    }
    parametros.push(new Parametro(parametro, valor));
}

class PlantillaDeImpresion {
    id: number;
    texto: string;
    clase: string;
}

function DefinirRestrictorCadena(propiedad: string, valor: string): string {
    var clausulas = new Array<ClausulaDeFiltrado>();
    var clausula: ClausulaDeFiltrado = new ClausulaDeFiltrado(propiedad, literal.filtro.criterio.igual, `${valor}`);
    clausulas.push(clausula);
    return JSON.stringify(clausulas);
}

function Encriptar(clave: string, objeto: any): any {
    if (EsString(objeto))
        return encodeURIComponent(objeto);

    if (EsNumero(objeto))
        return encodeURIComponent(objeto);

    if (EsBool(objeto))
        return objeto;

    if (EsFechaValida(objeto))
        return encodeURIComponent(objeto);

    if (NoDefinido(objeto))
        return null;

    throw Error(`Falta definir como encriptar el valor de ${objeto}`);
}


function ParsearExpresion(elemento: any, patron: string): string {
    let mostrar: string = patron;
    //se ha pasado una expresión a mostrar que es o debe ser el nombre de un campo de la tabla, para eso no hace falta corchetes
    if (mostrar.indexOf('[') == -1 && mostrar.indexOf(']') == -1) {
        mostrar = `[${patron}]`;
        patron = mostrar;
    }

    for (let i = 0; i < Object.keys(elemento).length; i++) {
        let propiedad = Object.keys(elemento)[i];
        if (patron.includes(`[${propiedad.toLowerCase()}]`))
            mostrar = mostrar.replace(`[${propiedad.toLowerCase()}]`, IsNullOrEmpty(elemento[propiedad]) ? "" : elemento[propiedad]);
    }

    return mostrar;
}

function Duermete(ms: number) {
    return new Promise(resolve => setTimeout(resolve, ms * 1000));
}


function ObtenerSubcadenasEntreCorchetes(cadena): Array<string> {
    return ObtenerSubcadenas(cadena, '[', ']');
}

function ObtenerSubcadenas(cadena: string, delimitadorInicial: string, delimitadorFinal: string): Array<string> {
    let cadenas = new Array<string>();
    if (delimitadorInicial.length !== 1 || delimitadorFinal.length !== 1)
        MensajesSe.EmitirExcepcion('ObtenerSubcadena', "los delimitadores solo han de tener un caracter");

    let buscarDesde = -1;
    while (1) {
        let posIni: number = cadena.indexOf(delimitadorInicial, buscarDesde + 1);
        let posFin: number = cadena.indexOf(delimitadorFinal, buscarDesde + 2);

        if (posIni == -1 || posFin == -1)
            return cadenas;

        if (posIni == buscarDesde)
            return cadenas;

        let subcadena = cadena.substring(posIni + 1, posFin);
        cadenas.push(subcadena);

        buscarDesde = posFin;
    }
}


function SustituirPropiedades(cadena: string, objeto): string {
    let ultimaInicial = -1;
    while (1) {
        let posIni: number = cadena.indexOf('[', ultimaInicial + 1);
        let posFin: number = cadena.indexOf(']', posIni + 1);

        if (posIni == -1 || posFin == -1)
            return cadena;

        if (posIni > posFin)
            return cadena;

        if (posIni == ultimaInicial)
            return cadena;

        ultimaInicial = posIni;
        cadena = SustituirPropiedad(cadena, objeto, posIni, posFin);
    }
}

function SustituirPropiedad(cadena: string, objeto: any, posIni: number, posFin: number): string {
    let propiedad = cadena.substring(posIni, posFin);
    propiedad = propiedad.substring(1);
    propiedad = propiedad.replace(']', '');
    let valor = ObtenerPropiedad(objeto, propiedad);
    if (Definido(valor))
        cadena = cadena.replace(`[${propiedad}]`, valor);
    return cadena;
}

function ExistePropiedad(objeto: any, propiedad: string): boolean {
    for (const p in objeto) {
        if (p.toLocaleLowerCase() === propiedad.toLocaleLowerCase()) {
            return true;
        }
    }
    return false;
}

function ObtenerCampoRestrictor(objeto: any, campoRestrictor: string, campoRestrictorPorDefecto: string = literal.id): number {

    var idElemento = ObtenerPropiedad(objeto, campoRestrictor);
    if (NoDefinido(idElemento))
        idElemento = ObtenerPropiedad(objeto, campoRestrictorPorDefecto);
    return Numero(idElemento);
}

function EstaAlgunEnumerado(lista: Array<string>, tipo, valores: Array<any>): boolean {
    for (var i = 0; i < valores.length; i++)
        if (EstaElEnumerado(lista, tipo, valores[i]))
            return true;

    return false;
}

function EstaElEnumerado(lista: Array<string>, tipo, enumerado): boolean {
    if (!Definido(lista))
        return false;

    if (!Definido(enumerado))
        return false;
    var cadena: string = ObtenerEnumerado(tipo, enumerado);
    var esta = lista.includes(cadena);
    return esta;
}

function ObtenerEnumerado(tipo, enumerado): string {
    let enumerados = Object.values(tipo);
    for (let i = 0; i < enumerados.length; i++) {
        if (enumerados[i] === tipo[enumerado])
            return enumerados[i].toString();
    }
    return undefined;
}

function ParsearEnumerado(tipo, enumerado) {
    let enumerados = Object.values(tipo);
    for (let i = 0; i < enumerados.length; i++) {
        if (enumerados[i] === tipo[enumerado])
            return enumerados[i];
    }
    return undefined;
}

function remplazar(cadena: string, trozo: string, valor: string): string {
    let patron = new RegExp(trozo, "gi");
    return cadena.replace(patron, valor);
}

function BuscarElementoPorIdGenerado(nombreDadoEnElDescriptor: string, funcion: (nombre: string, i: number) => string): HTMLDivElement | null {
    let div: HTMLDivElement = document.getElementById(funcion(nombreDadoEnElDescriptor, 0)) as HTMLDivElement;;
    if (div)
        return div;
    for (var i = 1; i <= 99; i++) {
        var nombrecompleto2 = funcion(nombreDadoEnElDescriptor, i);
        var div2 = document.getElementById(nombrecompleto2) as HTMLDivElement;
        if (div2) {
            return div2;
        }
    }
    MensajesSe.Error('BuscarElementoPorIdGenerado', `No se ha localizado el div: ${funcion(nombreDadoEnElDescriptor, 0)}`)
    return null;
}

function SanitizeHTML(str: string): string {
    const temp = document.createElement('div');
    temp.textContent = str;
    return temp.innerHTML;
}

function CopiarUrlAlPortapapeles(url: string, mensaje: string = undefined): void {
    // Copiar la URL al portapapeles
    navigator.clipboard.writeText(url)
        .then(() => {
            if (mensaje !== '' && mensaje !== Ajax.Mensajes.NoMostrar)
                MensajesSe.Info(Definido(mensaje) ? mensaje : 'La URL para descargar el archivo se ha copiado al porta-papeles con validez de 1 hora');
        })
        .catch((err) => {
            MensajesSe.MostrarExcepcion('DespuesDeRegistrarDescargaConGuid', err);
        });
}

function ObtenerPropiedad(objeto: any, propiedad: string, valorPorDefecto: any = undefined, emitirError: boolean = false): any {
    //Si es un array de parámetros he de buscar dentro de cada uno de los items
    let esArray: boolean = objeto instanceof Array;
    let arrayDeParametros: boolean = esArray && (objeto as Array<any>).length > 0 && Definido(objeto[0]['parametro']) && Definido(objeto[0]['valor']);
    //if (esArray) {
    //    for (let x of objeto) {
    //        arrayDeParametros = x instanceof Parametro;
    //        break;
    //    };
    //};
    if (!arrayDeParametros) {
        for (const p in objeto) {
            if (p.toLocaleLowerCase() === propiedad.toLocaleLowerCase()) {
                let valor = objeto[p];
                if (valorPorDefecto !== undefined && valorPorDefecto instanceof Map) {
                    return new Map(Object.entries(valor));
                }
                return valor;
            }
        }
    }
    else {
        for (const p in objeto)
            if (objeto[p]['parametro'].toLocaleLowerCase() === propiedad.toLocaleLowerCase()) {
                return objeto[p]['valor'];
            }
    }

    if (emitirError)
        MensajesSe.EmitirExcepcion("Obtener propiedad", `la propiedad ${propiedad} no está definida en el objeto ${objeto}`);

    return valorPorDefecto;
}

function esFechaISO(fecha: string): boolean {
    const date = new Date(fecha);
    if (!EsFechaValida(date))
        return false;
    return date.toISOString().substring(0, 10) === fecha.substring(0, 10);
}

function CrearFecha(fechaHora: string, separadorDeFecha: string = ltrSimbolos.separadorDeDDMMYYYY, separadorDeHora: string = ltrSimbolos.separadorDeHHMMSS): Date {
    var partes = fechaHora.split('T');
    var fecha = partes[0];
    var horas = partes.length === 1 ? null : partes[1];

    if (fecha.includes('-')) {
        separadorDeFecha = '-';
    } else if (fecha.includes('/')) {
        separadorDeFecha = '/';
    }

    if (esFechaISO(fechaHora)) {
        //yyyy-mm-ddT00:00:00.000Z
        return new Date(Number(fechaHora.substring(0, 4)), Number(fechaHora.substring(5, 7)) - 1, Number(fechaHora.substring(8, 10)), Numero(fechaHora.substring(11, 13)), Numero(fechaHora.substring(14, 16)));
    }

    var trocearFecha = fecha.split(separadorDeFecha);

    var anio = 0;
    var mes = 0;
    var dia = 0;
    var hora = 0;
    var minutos = 0;
    if (Numero(trocearFecha[0]) > 1900) {
        anio = Numero(trocearFecha[0]);
        mes = Numero(trocearFecha[1]);
        dia = Numero(trocearFecha[2]);
    }
    else {
        anio = Numero(trocearFecha[2]);
        if (Numero(trocearFecha[0]) < 13 && Numero(trocearFecha[1]) > 12) {
            mes = Numero(trocearFecha[0]);
            dia = Numero(trocearFecha[1]);
        }
        else if (Numero(trocearFecha[0]) > 12 && Numero(trocearFecha[1]) < 13) {
            mes = Numero(trocearFecha[1]);
            dia = Numero(trocearFecha[0]);
        }
        else if (navigator.language.startsWith('en')) {
            mes = Numero(trocearFecha[0]);
            dia = Numero(trocearFecha[1]);
        }
    }

    if (horas != null) {
        hora = Numero(horas.split(separadorDeHora)[0])
        minutos = Numero(horas.split(separadorDeHora)[1]);
    }

    return new Date(Number(anio), Number(mes) - 1, Number(dia), Numero(hora), Numero(minutos));
}

/*
 * https://es.stackoverflow.com/questions/445/c%C3%B3mo-obtener-valores-de-la-url-get-en-javascript/457
 * http://www.ejemplo.com.mx/producto?prodId=88 --> var prodId = getParameterByName('prodId');
 * La función getParameterByName recibe un parámetro del tipo String (cadena de texto) que va a ser utilizado para evaluar por medio de una expresión regular que busque todo el contenido entre el final de la cadena recibida 
 * seguido por un símbolo de igual (=) y el final de la cadena a donde buscar (location.search) o hasta encontrar el símbolo «et» también conocido como «ampersand» (&). 
 * Al final dicho texto encontrado decodificado y devuelto. En el remoto caso de no encontrar coincidencias, devolverá una cadena vacía.
 * */
//function ObtenerParametroDeLaUrl(parametro: string): string {
//    parametro = parametro.replace(/[\[]/, "\\[").replace(/[\]]/, "\\]");
//    var regex = new RegExp("[\\?&]" + parametro + "=([^&#]*)"),
//        results = regex.exec(location.search);
//    return results === null ? "" : decodeURIComponent(results[1].replace(/\+/g, " "));
//}

function ObtenerParametroUrlAsync(parametro: string, valorPorDefecto: any = undefined, emitirError: boolean = false, callback: (result: any) => void): void {
    const maxIntentos = 5;
    const tiempoEspera = 100;
    let intento = 0;

    function intentarObtener() {
        if (window.location.href) {
            callback(ObtenerParametroDeUnaUrl(window.location.href, parametro, valorPorDefecto, emitirError));
            return;
        }

        intento++;
        if (intento < maxIntentos) {
            console.log(`Intento ${intento}: window.location.href no está definido. Esperando...`);
            setTimeout(intentarObtener, tiempoEspera);
        } else {
            console.log(`Se agotaron los intentos. window.location.href no está definido.`);
            if (emitirError) {
                throw new Error("No se pudo obtener window.location.href después de varios intentos");
            }
            callback(valorPorDefecto);
        }
    }

    intentarObtener();
}

interface ParametroUrl {
    clave: string;
    valor: string;
}
function ObtenerParametrosDeLaUrl(): ParametroUrl[] {
    // 1. Obtenemos el string de búsqueda (?origen=menu&...) de la URL actual
    const queryString = window.location.search;

    // 2. Usamos URLSearchParams para parsear el string
    const params = new URLSearchParams(queryString);

    // 3. Convertimos los parámetros en un Array de objetos { clave, valor }
    const listaParametros: ParametroUrl[] = [];

    params.forEach((valor, clave) => {
        listaParametros.push({ clave, valor });
    });

    return listaParametros;
}

function ObtenerParametroUrl(parametro: string, valorPorDefecto: any = undefined, emitirError: boolean = false): any {
    return ObtenerParametroDeUnaUrl(window.location.href, parametro, valorPorDefecto, emitirError);
}

function ObtenerParametroDeUnaUrl(url: string, parametro: string, valorPorDefecto: any = undefined, emitirError: boolean = false): any {
    const parametros = new URLSearchParams(url.substring(url.indexOf('?')));
    const claves = parametros.keys();
    for (const clave of claves) {
        if (clave.toLocaleLowerCase() == parametro.toLocaleLowerCase())
            return parametros.get(clave);
    }
    if (emitirError)
        MensajesSe.EmitirExcepcion("ObtenerParametroDeUnaUrl", `El parámetro ${parametro} no está definido en la lista de parámetros de la url ${url}`);

    return valorPorDefecto;
}



function ContieneParametroEnLaUrl(url: string, parametro: string): boolean {
    const parametros = new URLSearchParams(url);
    const claves = parametros.keys();
    for (const clave of claves) {
        if (clave.toLocaleLowerCase() == parametro.toLocaleLowerCase())
            return true;
    }
    return false;
}
/*
 * https://stackoverflow.com/questions/5717093/check-if-a-javascript-string-is-a-url
 * */
function EsUrl(string): boolean {
    let url;
    try {
        url = new URL(string);
    } catch (_) {
        return false;
    }
    return url.protocol === "http:" || url.protocol === "https:";
}


/*
 *  https://stackoverflow.com/questions/105034/how-to-create-a-guid-uuid
 */

function generarUUID() { // Public Domain/MIT
    var d = new Date().getTime();//Timestamp
    var d2 = ((typeof performance !== 'undefined') && performance.now && (performance.now() * 1000)) || 0;//Time in microseconds since page-load or 0 if unsupported
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
        var r = Math.random() * 16;//random number between 0 and 16
        if (d > 0) {//Use timestamp until depleted
            r = (d + r) % 16 | 0;
            d = Math.floor(d / 16);
        } else {//Use microseconds since page-load if supported
            r = (d2 + r) % 16 | 0;
            d2 = Math.floor(d2 / 16);
        }
        return (c === 'x' ? r : (r & 0x3 | 0x8)).toString(16);
    });
}

function CapitalizarPrimeraLetra(string: string): string {
    return Definido(string) ? string.charAt(0).toUpperCase() + string.slice(1) : undefined;
}

function ValidarIbanDelPortaPapeles(event: any) {
    let iban: string = event.clipboardData.getData('text/plain');
    event.preventDefault();
    return ValidarIban(iban);
}

function ValidarIban(iban: string) {
    let rePunto = /\./gi;
    let reBlanco = /\ /gi;
    let reGuion = /\-/gi;
    let reBarra = /\//gi;
    iban = iban.replace(rePunto, '').replace(reBlanco, '').replace(reGuion, '').replace(reBarra, '');
    if (iban.length !== 24)
        MensajesSe.Info(`El iban ${iban} no se puede pegar, los primeros dos caracteres letras, los dos siguientes, números o letras y el resto números`);
    return iban;
}

function MapearCuentaBancaria(panel: HTMLDivElement, cuentaBancaria: any) {

    let tbxIban = ApiControl.BuscarEditor(panel, ltrPropiedades.Maestros.Contabilidad.CuentaBancaria.Iban);
    tbxIban.value = cuentaBancaria.substring(0, 4);

    let tbxEntidad = ApiControl.BuscarEditor(panel, ltrPropiedades.Maestros.Contabilidad.CuentaBancaria.Entidad);
    tbxEntidad.value = cuentaBancaria.substring(4, 8);

    let tbxOficina = ApiControl.BuscarEditor(panel, ltrPropiedades.Maestros.Contabilidad.CuentaBancaria.Oficina);
    tbxOficina.value = cuentaBancaria.substring(8, 12);

    let tbxDc = ApiControl.BuscarEditor(panel, ltrPropiedades.Maestros.Contabilidad.CuentaBancaria.Dc);
    tbxDc.value = cuentaBancaria.substring(12, 14);

    let tbxNumero = ApiControl.BuscarEditor(panel, ltrPropiedades.Maestros.Contabilidad.CuentaBancaria.Numero);
    tbxNumero.value = cuentaBancaria.substring(14, 24);
}


function ValidarQueHayCuentaBancaria(panel: HTMLDivElement) {

    let tbxIban = ApiControl.BuscarEditor(panel, ltrPropiedades.Maestros.Contabilidad.CuentaBancaria.Iban);
    if (tbxIban.value.length != 4) MensajesSe.EmitirExcepcion("ValidarQueHayCuentaBancaria", "El Iban ha de ser 2 carcateres de pais y 2 de Dc");

    let tbxEntidad = ApiControl.BuscarEditor(panel, ltrPropiedades.Maestros.Contabilidad.CuentaBancaria.Entidad);
    if (tbxEntidad.value.length != 4) MensajesSe.EmitirExcepcion("ValidarQueHayCuentaBancaria", "la entidad debe ser de 4 dígitos");

    let tbxOficina = ApiControl.BuscarEditor(panel, ltrPropiedades.Maestros.Contabilidad.CuentaBancaria.Oficina);
    if (tbxOficina.value.length != 4) MensajesSe.EmitirExcepcion("ValidarQueHayCuentaBancaria", "la oficina debe ser de 4 dígitos");

    let tbxDc = ApiControl.BuscarEditor(panel, ltrPropiedades.Maestros.Contabilidad.CuentaBancaria.Dc);
    if (tbxDc.value.length != 2) MensajesSe.EmitirExcepcion("ValidarQueHayCuentaBancaria", "los DC debe ser de 2 dígitos");

    let tbxNumero = ApiControl.BuscarEditor(panel, ltrPropiedades.Maestros.Contabilidad.CuentaBancaria.Numero);
    if (tbxNumero.value.length != 10) MensajesSe.EmitirExcepcion("ValidarQueHayCuentaBancaria", "El número de cuenta son 10 dígitos");
}

function esUrlValida(url: string): boolean {
    const urlPattern = /^(https?:\/\/)?([\da-z.-]+)\.([a-z.]{2,6})([/\w .-]*)*\/?$/;
    return urlPattern.test(url);
}


function ExisteVariable(namespace: any, variable: string): boolean {
    try {
        if (typeof namespace !== 'undefined') {
            return variable in namespace;
        }
        return false;
    }
    catch {
        return false;
    }
}

function SepararCadenaEnNumeroYUnidad(cadena: string): [number, string] {
    if (!Definido(cadena))
        return [0, ''];

    const regex = /(\d+(?:\.\d+)?)(.*)/;
    const match = cadena.match(regex);

    if (match) {
        const numero = parseFloat(match[1]);
        const unidad = match[2].trim();
        return [numero, unidad];
    } else {
        return [0, ''];
    }
}

function Evaluar(origen: string, accion: string, elementoThis?: any) {
    try {
        // 1. Limpiar
        let accionLimpia = accion.trim();
        if (accionLimpia.toLowerCase().startsWith('javascript:')) {
            accionLimpia = accionLimpia.substring(11).trim();
        }
        accionLimpia = accionLimpia.replace(/;$/, '');

        // 2. Expresión Regular
        const regex = /^([\w.]+)\s*\(([\s\S]*)\)$/;
        const match = accionLimpia.match(regex);

        if (!match) {
            MensajesSe.Error(origen, 'Acción no válida', `La cadena de acción no tiene el formato esperado: '${accion}'`);
            return;
        }

        const nombreCompleto = match[1];
        const partesFuncion = nombreCompleto.split('.');
        const nombreModulo = partesFuncion[0];
        const nombreFuncion = partesFuncion[1];
        const argumentosStr = match[2];

        // 3. Parsear argumentos
        const args: any[] = [];

        if (argumentosStr.trim().length > 0) {
            const argumentosSeparados = argumentosStr.split(',').map(arg => arg.trim());

            for (const argLimpio of argumentosSeparados) {
                if (argLimpio.length === 0) continue;

                if (argLimpio === 'this') {
                    if (!Definido(elementoThis)) {
                        MensajesSe.Error(origen, `No se proporciona el objeto 'this' para: '${accion}'`);
                    }
                    args.push(elementoThis);
                }
                // Manejo de Strings (con comillas)
                else if (
                    (argLimpio.startsWith("'") && argLimpio.endsWith("'")) ||
                    (argLimpio.startsWith('"') && argLimpio.endsWith('"'))
                ) {
                    args.push(argLimpio.substring(1, argLimpio.length - 1));
                }
                // Manejo de Booleanos
                else if (argLimpio.toLowerCase() === 'true') {
                    args.push(true);
                }
                else if (argLimpio.toLowerCase() === 'false') {
                    args.push(false);
                }
                // Manejo de Números (Lo que te estaba fallando)
                else {
                    const num = Number(argLimpio);
                    if (!isNaN(num) && argLimpio !== "") {
                        args.push(num); // Se añade como número real
                    } else {
                        args.push(argLimpio); // Si no es número, se queda como string (ej. una variable global)
                    }
                }
            }
        }

        // 4. Ejecución
        const modulo = (window as any)[nombreModulo];
        if (!modulo) {
            MensajesSe.Error(origen, `Módulo no cargado: '${nombreModulo}'`);
            return;
        }

        const funcion = modulo[nombreFuncion];
        if (typeof funcion === 'function') {
            funcion.apply(modulo, args);
        } else {
            MensajesSe.Error(origen, `Función inexistente: '${nombreFuncion}' en ${nombreModulo}`);
        }

    } catch (error: any) {
        MensajesSe.Error(origen, `Error al evaluar: '${accion}'`, error.message);
    }
}

function Evaluar_02(origen: string, accion: string, elementoThis?: any) {
    try {
        // 1. Limpiar: Quitamos 'javascript:', espacios al inicio/final y el punto y coma final
        let accionLimpia = accion.trim();
        if (accionLimpia.toLowerCase().startsWith('javascript:')) {
            accionLimpia = accionLimpia.substring(11).trim();
        }
        accionLimpia = accionLimpia.replace(/;$/, '');

        // 2. Nueva Expresión Regular más flexible
        // Captura el nombre (letras, números, puntos, guiones bajos) 
        // y TODO lo que esté entre los paréntesis inicial y final
        const regex = /^([\w.]+)\s*\(([\s\S]*)\)$/;
        const match = accionLimpia.match(regex);

        if (!match) {
            MensajesSe.Error(origen, 'Acción no válida', `La cadena de acción no tiene el formato esperado: '${accion}'`);
            return;
        }

        const nombreCompleto = match[1];
        const partesFuncion = nombreCompleto.split('.');
        const nombreModulo = partesFuncion[0];
        const nombreFuncion = partesFuncion[1];
        const argumentosStr = match[2];

        // 3. Parsear argumentos manejando comas dentro de strings entrecomillados
        const args: any[] = [];

        if (argumentosStr.trim().length > 0) {
            // Usamos una lógica más avanzada para no romper por comas dentro de strings
            // pero para tu caso actual de strings simples:
            const argumentosSeparados = argumentosStr.split(',').map(arg => arg.trim());

            for (const argLimpio of argumentosSeparados) {
                if (argLimpio.length === 0) continue;

                if (argLimpio === 'this') {
                    if (!Definido(elementoThis)) {
                        MensajesSe.Error(origen, `No se proporciona el objeto 'this' para: '${accion}'`);
                    }
                    args.push(elementoThis);
                } else if (
                    (argLimpio.startsWith("'") && argLimpio.endsWith("'")) ||
                    (argLimpio.startsWith('"') && argLimpio.endsWith('"'))
                ) {
                    // Quitamos las comillas externas
                    args.push(argLimpio.substring(1, argLimpio.length - 1));
                } else {
                    // Números o variables
                    args.push(argLimpio);
                }
            }
        }

        // 4. Ejecución (Mantenemos tu lógica de window[modulo])
        const modulo = (window as any)[nombreModulo];
        if (!modulo) {
            MensajesSe.Error(origen, `Módulo no cargado: '${nombreModulo}'`);
            return;
        }

        const funcion = modulo[nombreFuncion];
        if (typeof funcion === 'function') {
            funcion.apply(modulo, args);
        } else {
            MensajesSe.Error(origen, `Función inexistente: '${nombreFuncion}' en ${nombreModulo}`);
        }

    } catch (error: any) {
        MensajesSe.Error(origen, `Error al evaluar: '${accion}'`, error.message);
    }
}



/**
 * Ejecuta una acción de JavaScript dada por una cadena (ej: 'javascript:Modulo.Funcion(arg1, this)').
 * Esta versión maneja 'this' y strings entrecomillados mediante un parseo simple por comas.
 *
 * @param origen Cadena que indica el contexto donde se originó la llamada (para logs de error).
 * @param accion Cadena de acción completa, que puede incluir el prefijo 'javascript:'.
 * @param elementoThis El elemento HTML al que se refiere la palabra clave 'this' en la cadena de acción.
 */
function EvaluarOld(origen: string, accion: string, elementoThis?: any) {
    try {
        // 1. Limpiar la cadena de 'javascript:' y de ';' al final si existe
        const accionLimpia = accion.split(':')[1]?.trim().replace(/;$/, '') || accion.trim().replace(/;$/, '');

        // 2. Expresión regular para capturar el nombre de la función y los argumentos
        const regex = /^([\w.]+)\((.*)\)$/;
        const match = accionLimpia.match(regex);

        if (!match) {
            MensajesSe.Error(origen, 'Acción no válida', `La cadena de acción no tiene el formato esperado: '${accion}'`);
            return;
        }

        const nombreCompleto = match[1];
        const partesFuncion = nombreCompleto.split('.');
        const nombreModulo = partesFuncion[0];
        const nombreFuncion = partesFuncion[1];
        const argumentosStr = match[2];

        // 3. Parsear los argumentos y manejar 'this' y strings
        const args: any[] = [];

        if (argumentosStr.trim().length > 0) {
            // Dividimos por coma para identificar los argumentos.
            const argumentosSeparados = argumentosStr.split(',').map(arg => arg.trim());

            // ESTE es el bucle que recorre y procesa los argumentos separados por comas
            for (const argLimpio of argumentosSeparados) {
                if (argLimpio.length === 0) {
                    continue; // Saltar si hay espacios vacíos
                }

                if (argLimpio === 'this') {
                    // Manejo del elemento 'this': se inyecta el elemento HTML
                    if (!Definido(elementoThis))
                        MensajesSe.Error(origen, `No se ha podido evaluar: '${accion}' ya que no se proporciona el objeto 'this'`);
                    args.push(elementoThis);
                } else if (
                    (argLimpio.startsWith("'") && argLimpio.endsWith("'")) ||
                    (argLimpio.startsWith('"') && argLimpio.endsWith('"'))
                ) {
                    // Manejo de strings entrecomillados: se quitan las comillas
                    // (Comportamiento estándar de los argumentos de función JS)
                    args.push(argLimpio.substring(1, argLimpio.length - 1));
                } else {
                    // Manejo de otros valores (números, booleanos, variables globales sin comillas)
                    args.push(argLimpio);
                }
            }
        }

        // 4. Buscar el módulo y la función en el ámbito global (window)
        const modulo = (window as any)[nombreModulo];
        const estaElModuloCargado = typeof modulo === 'object' || typeof modulo === 'function';

        if (!estaElModuloCargado) {
            MensajesSe.Error(origen, `El módulo: '${nombreModulo}'`, `no está cargado, no se puede evaluar: '${accionLimpia}'`);
            return;
        }

        const funcion = modulo[nombreFuncion];

        // 5. Ejecutar la función con los argumentos preparados
        if (typeof funcion === 'function') {
            (funcion as Function).apply(modulo, args);
        } else {
            MensajesSe.Error(origen, `La función: '${nombreFuncion}'`, 'no es una función válida o no existe en el módulo.');
        }

    } catch (error: any) {
        // Capturar y reportar cualquier error de ejecución
        MensajesSe.Error(origen, `No se ha podido evaluar: '${accion}'`, `No se ha podido evaluar: '${accion}'\n` + error.message + '\n' + error.stack);
    }
}
function EvaluarOld2(origen: string, accion: string) {
    try {
        // Limpiar la cadena de 'javascript:' y de ';' al final si existe
        const accionLimpia = accion.split(':')[1]?.trim().replace(/;$/, '') || accion.trim().replace(/;$/, '');

        // Expresión regular para capturar el nombre de la función y los argumentos
        const regex = /^([\w.]+)\((.*)\)$/;
        const match = accionLimpia.match(regex);

        if (!match) {
            MensajesSe.Error(origen, 'Acción no válida', `La cadena de acción no tiene el formato esperado: '${accion}'`);
            return;
        }

        const nombreCompleto = match[1];
        const partesFuncion = nombreCompleto.split('.');
        const nombreModulo = partesFuncion[0];
        const nombreFuncion = partesFuncion[1];
        const argumentosStr = match[2];

        // Parsear los argumentos de la cadena
        const args: string[] = [];
        if (argumentosStr.trim().length > 0) {
            // Usar una expresión regular para encontrar todos los argumentos entre comillas
            const argsRegex = /'([^']*)'|"([^"]*)"/g;
            let argMatch;
            while ((argMatch = argsRegex.exec(argumentosStr)) !== null) {
                // El argumento puede estar en el grupo 1 (comillas simples) o 2 (comillas dobles)
                args.push(argMatch[1] || argMatch[2]);
            }
        }

        const modulo = (window as any)[nombreModulo];
        const estaElModuloCargado = typeof modulo === 'object' || typeof modulo === 'function';

        if (!estaElModuloCargado) {
            MensajesSe.Error(origen, `El módulo: '${nombreModulo}'`, `no está cargado, no se puede evaluar: '${accionLimpia}'`);
            return;
        }

        const funcion = modulo[nombreFuncion];
        if (typeof funcion === 'function') {
            (funcion as Function).apply(modulo, args);
        } else {
            MensajesSe.Error(origen, `La función: '${nombreFuncion}'`, 'no es una función válida o no existe en el módulo.');
        }

    } catch (error: any) {
        MensajesSe.Error(origen, `No se ha podido evaluar: '${accion}'`, `No se ha podido evaluar: '${accion}'\n` + error.message + '\n' + error.stack);
    }
}
/**
 * Evalúa una acción dinámica de forma segura, pasando argumentos explícitamente.
 * @param {string} origen - La fuente de la llamada.
 * @param {string} accion - La cadena de acción a ejecutar, ej: 'Juridico.Plv_Antes_De_Buscar_Unitarios'.
 * @param {Array<any>} args - Los argumentos a pasar a la función.
 */
function EvaluarConParametros(origen, accion, args: any[] = []) {
    try {
        // Limpiamos la cadena de 'javascript:' si existe
        const partes = accion.split(':');
        const accionLimpia = partes.length === 2 ? partes[1].trim() : partes[0].trim();
        const partesFuncion = accionLimpia.split('.');

        const nombreModulo = partesFuncion[0];
        const nombreFuncion = partesFuncion[1].replace(/\(.*\)/, ''); // Elimina los paréntesis si existen

        // Verificamos si el módulo está cargado y si es un objeto
        const modulo = window[nombreModulo];
        const estaElModuloCargado = typeof modulo === 'object' || typeof modulo === 'function';
        if (!estaElModuloCargado) {
            MensajesSe.Error(origen, `El módulo: '${nombreModulo}'`, `no está cargado, no se puede evaluar: '${accionLimpia}'`);
            return;
        }

        const funcion = modulo[nombreFuncion];
        if (typeof funcion === 'function') {
            // Llamamos a la función con los argumentos
            // Usamos una aserción de tipo 'as Function' para resolver el error de TypeScript
            (funcion as Function).apply(modulo, args);
        } else {
            MensajesSe.Error(origen, `La función: '${nombreFuncion}'`, 'no es una función válida o no existe en el módulo.');
        }

    } catch (error) {
        MensajesSe.Error(origen, `No se ha podido evaluar: '${accion}'`, `No se ha podido evaluar: '${accion}'\n` + error.message + '\n' + error.stack);
    }
}

function EvaluarConElemento(origen, accion, elemento: HTMLElement) {
    try {
        // Limpiamos la cadena de 'javascript:' si existe
        const partes = accion.split(':');
        const accionLimpia = partes.length === 2 ? partes[1].trim() : partes[0].trim();
        const partesFuncion = accionLimpia.split('.');

        const nombreModulo = partesFuncion[0];
        const nombreFuncion = partesFuncion[1].replace(/\(.*\)/, ''); // Elimina los paréntesis si existen

        // Verificamos si el módulo está cargado y si es un objeto
        const modulo = window[nombreModulo];
        const estaElModuloCargado = typeof modulo === 'object' || typeof modulo === 'function';
        if (!estaElModuloCargado) {
            MensajesSe.Error(origen, `El módulo: '${nombreModulo}'`, `no está cargado, no se puede evaluar: '${accionLimpia}'`);
            return;
        }

        const funcion = modulo[nombreFuncion];
        if (typeof funcion === 'function') {
            // Llamamos a la función con los argumentos
            // Usamos una aserción de tipo 'as Function' para resolver el error de TypeScript
            (funcion as Function).call(modulo, elemento);
        } else {
            MensajesSe.Error(origen, `La función: '${nombreFuncion}'`, 'no es una función válida o no existe en el módulo.');
        }

    } catch (error) {
        MensajesSe.Error(origen, `No se ha podido evaluar: '${accion}'`, `No se ha podido evaluar: '${accion}'\n` + error.message + '\n' + error.stack);
    }
}
function EsDispositvoMovil(): boolean {
    return window.matchMedia('(max-width: 768px)').matches;
}

function ObtenerNumeroFinal(cadena: string): number {
    const coincidencia = cadena.match(/\d+$/);
    return coincidencia ? parseInt(coincidencia[0]) : -1;
}

function inyectarBotonPegarFecha() {
    const selectores = 'input[type="date"][onpaste]';
    const camposFecha = document.querySelectorAll(selectores);

    camposFecha.forEach(inputElement => {
        const botonPegar = document.createElement('button');

        botonPegar.type = 'button';
        botonPegar.classList.add('btn-pegar-icono');
        botonPegar.innerHTML = '📋';
        botonPegar.title = 'Pegar fecha del portapapeles';

        // --- CAMBIO CLAVE AQUÍ ---
        // 1. Necesitamos el ID del input para pasarlo como argumento.
        const inputId = inputElement.id;

        // 2. Establecer el atributo onclick como una CADENA LITERAL,
        // replicando el comportamiento de tu HTML de archivos.
        // La función PegarFecha debe ser accesible globalmente.
        botonPegar.setAttribute('onclick', `PegarFecha(document.getElementById('${inputId}'))`);

        // ------------------------

        inputElement.parentNode.insertBefore(botonPegar, inputElement);
    });

    // NOTA: Asegúrate de eliminar la línea anterior:
    // botonPegar.onclick = () => PegarFecha(inputElement); 
}
/**
 * Intenta leer el portapapeles de forma asíncrona usando el API de lectura de alto
 * riesgo (navigator.clipboard.read()). Si el permiso ya fue concedido (como en el 
 * caso de subir archivos), obtiene el texto y simula el evento para handlePasteDate.
 * Si el permiso es denegado, falla silenciosamente (sin alerts).
 * * @param {HTMLInputElement} inputElement El campo de fecha.
 */
function PegarFecha(inputElement) {
    debugger;

    // 1. Iniciar la lectura asíncrona (Promesas)
    navigator.clipboard.read()
        .then(data => {
            // ÉXITO: Los permisos fueron otorgados y tenemos acceso a los ClipboardItem

            // Buscar el objeto que contenga texto plano (text/plain)
            const itemTexto = data.find(item => item.types.includes('text/plain'));

            if (!itemTexto) {
                console.warn('Portapapeles no contiene texto plano.');
                return;
            }

            // Encadenamos para obtener el Blob y leer el contenido como texto
            itemTexto.getType('text/plain')
                .then(blob => blob.text())
                .then(textoPortapapeles => {

                    if (!textoPortapapeles) {
                        return;
                    }

                    // 2. Crear el evento genérico 'paste' simulado
                    const eventoSimulado = new Event('paste', {
                        bubbles: true,
                        cancelable: true
                    });

                    // 3. Inyectar el texto real obtenido en la propiedad simulada
                    (eventoSimulado as any).simulatedText = textoPortapapeles;

                    // 4. Asignar el target/currentTarget al input
                    Object.defineProperty(eventoSimulado, 'currentTarget', {
                        value: inputElement,
                        writable: true
                    });

                    // 5. Llamar a tu lógica de manejo
                    handlePasteDate(eventoSimulado as any);
                    inputElement.focus();
                })
                .catch(error => {
                    console.error("Error al leer el texto del Blob:", error);
                });
        })
        .catch(err => {
            // FRACASO: El navegador rechazó la promesa de lectura (el problema actual).

            console.error(
                'El botón de pegar no funciona automáticamente. Permiso denegado por seguridad.',
                err
            );

            // Enfocamos el input, forzando al usuario a usar Ctrl+V.
            inputElement.focus();
        });
}
function handlePasteDate(event: ClipboardEvent): void {
    const inputElement = event.currentTarget as HTMLInputElement;

    if (ApiControl.EsSoloLectura(inputElement))
        return;

    // Obtener el texto del portapapeles
    const clipboardData = event.clipboardData || (window as any).clipboardData;
    const pastedText = clipboardData.getData('text');

    // Intentar parsear la fecha
    const dateObject = parseDateString(pastedText);

    if (dateObject) {
        // 1. Prevenir la acción de pegado por defecto
        event.preventDefault();

        // 2. Formatear la fecha al estándar YYYY-MM-DD (requerido por input type="date")
        const year = dateObject.getFullYear();
        const month = String(dateObject.getMonth() + 1).padStart(2, '0');
        const day = String(dateObject.getDate()).padStart(2, '0');
        const formattedDate = `${year}-${month}-${day}`;

        // 3. Establecer el valor
        inputElement.value = formattedDate;

        // Opcional: Ejecutar el onblur original si es relevante
        // Gasto.Far_Tras_Indicar_Fecha_De_Emision(); 

    } else {
        // Si no se detecta una fecha válida, permite que el pegado por defecto continúe
        // Nota: Aquí se podría no hacer nada, ya que por defecto el evento se propaga si no se llama a preventDefault.
        // Pero para garantizar el comportamiento, lo dejamos pasar si no se llamó a preventDefault arriba.
        // Para este caso, no es necesario hacer nada más.
    }
}


// Definición de formatos de fecha comunes
const FORMATS_EUROPEAN = [
    /^(\d{1,2})[./-](\d{1,2})[./-](\d{4})$/, // DD/MM/YYYY, DD.MM.YYYY, DD-MM-YYYY
];
const FORMATS_AMERICAN = [
    /^(\d{1,2})[./-](\d{1,2})[./-](\d{4})$/, // MM/DD/YYYY, MM.DD.YYYY, MM-DD-YYYY
];

function parseDateString(dateString: string): Date | null {
    if (!dateString) return null;

    // Limpia el string de posibles espacios
    const cleanString = dateString.trim();

    // 1. Determinar el formato principal basado en el idioma del navegador
    const lang = navigator.language.toLowerCase();
    // Idiomas europeos (España, Francia, etc.) suelen usar DD/MM/YYYY
    const isEuropeanLocale = lang.startsWith('es') || lang.startsWith('fr') || lang.startsWith('de') || lang.startsWith('it');

    let parsingFormats: { regex: RegExp, isEuropean: boolean }[] = [];

    // Definir el orden de prioridad de análisis
    if (isEuropeanLocale) {
        // Priorizar DD/MM/YYYY
        FORMATS_EUROPEAN.forEach(regex => parsingFormats.push({ regex, isEuropean: true }));
        FORMATS_AMERICAN.forEach(regex => parsingFormats.push({ regex, isEuropean: false }));
    } else {
        // Priorizar MM/DD/YYYY (EE.UU., etc.)
        FORMATS_AMERICAN.forEach(regex => parsingFormats.push({ regex, isEuropean: false }));
        FORMATS_EUROPEAN.forEach(regex => parsingFormats.push({ regex, isEuropean: true }));
    }

    // 2. Intentar analizar la fecha con el orden de prioridad
    for (const { regex, isEuropean } of parsingFormats) {
        const match = cleanString.match(regex);

        if (match) {
            const [_, part1, part2, year] = match;
            let day: number, month: number;

            if (isEuropean) {
                // Formato DD/MM/YYYY: part1=Día, part2=Mes
                day = parseInt(part1, 10);
                month = parseInt(part2, 10);
            } else {
                // Formato MM/DD/YYYY: part1=Mes, part2=Día
                month = parseInt(part1, 10);
                day = parseInt(part2, 10);
            }

            // Las fechas en JavaScript usan el mes basado en 0 (0=Enero, 11=Diciembre)
            const parsedDate = new Date(parseInt(year, 10), month - 1, day);

            // Comprobar si la fecha es válida (ej. no es 30 de febrero)
            if (parsedDate.getFullYear() === parseInt(year, 10) &&
                parsedDate.getMonth() === (month - 1) &&
                parsedDate.getDate() === day) {
                return parsedDate;
            }
        }
    }

    // Si fallan las comprobaciones de regex, intentar la función Date.parse() nativa
    const nativeParsed = new Date(cleanString);
    if (!isNaN(nativeParsed.getTime())) {
        return nativeParsed;
    }

    return null;
}





