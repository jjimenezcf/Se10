// Botón de criterio de los editores de texto del filtro: el servidor marca con atControl.selectorDeCriterio los editores de texto que
// siguen el estándar de Filtrar.FiltrarPorTexto y aquí, al inicializar el panel de filtro, se les pone a la derecha un botón que despliega
// los criterios de ltrCriteriosDeTexto. El criterio elegido no cambia el criterio de la cláusula: se traduce a los símbolos del estándar
// ('=', '!', '|' delante o detrás) al leer el valor (ValorConCriterio), así vale también en los filtros propios de cada vista.
// Las plantillas de filtrado guardan el valor ya con los símbolos y, al aplicarlas, MapearValor los quita y vuelve a fijar el criterio.
namespace ApiDeCriterioDeTexto {

    const criterioPorDefecto: string = ltrCriteriosDeTexto[0].criterio;
    let listaAbierta: HTMLDivElement = null;
    let eventosGlobalesRegistrados: boolean = false;

    export function Inicializar(panel: HTMLElement): void {
        if (!Definido(panel))
            return;
        const editores = panel.querySelectorAll(`input[${atControl.selectorDeCriterio}]`) as NodeListOf<HTMLInputElement>;
        for (let i = 0; i < editores.length; i++) {
            if (!Definido(BotonDe(editores[i])))
                CrearSelector(editores[i]);
            Fijar(editores[i], criterioPorDefecto);
        }
        RegistrarEventosGlobales();
    }

    export function TieneSelector(editor: HTMLInputElement): boolean {
        return Definido(editor) && editor.hasAttribute(atControl.selectorDeCriterio);
    }

    export function CriterioElegido(editor: HTMLInputElement): string {
        const criterio = TieneSelector(editor) ? editor.getAttribute(atControl.criterioElegido) : null;
        return IsNullOrEmpty(criterio) ? criterioPorDefecto : criterio;
    }

    export function Fijar(editor: HTMLInputElement, criterio: string): void {
        const opcion = ltrCriteriosDeTexto.find(o => o.criterio === criterio) ?? ltrCriteriosDeTexto[0];
        editor.setAttribute(atControl.criterioElegido, opcion.criterio);

        const boton = BotonDe(editor);
        if (!Definido(boton))
            return;
        boton.textContent = opcion.icono;
        boton.title = opcion.titulo;
        if (opcion.criterio === criterioPorDefecto)
            ApiControl.ExcluirCss(boton, ltrCss.CriterioDeTexto.BotonActivo);
        else
            ApiControl.IncluirCss(boton, ltrCss.CriterioDeTexto.BotonActivo);

        const lista = ListaDe(editor);
        if (!Definido(lista))
            return;
        lista.querySelectorAll(`.${ltrCss.CriterioDeTexto.Opcion}`).forEach((elemento: HTMLElement) => {
            if (elemento.getAttribute(atControl.criterioElegido) === opcion.criterio)
                ApiControl.IncluirCss(elemento, ltrCss.CriterioDeTexto.OpcionElegida);
            else
                ApiControl.ExcluirCss(elemento, ltrCss.CriterioDeTexto.OpcionElegida);
        });
    }

    // valor del editor con el criterio elegido traducido a los símbolos del estándar de filtrado
    export function ValorConCriterio(editor: HTMLInputElement): string {
        return AplicarCriterio(editor.value, CriterioElegido(editor));
    }

    // pone en el editor un valor que puede traer los símbolos del criterio (plantillas, estado): si el editor tiene selector los quita y
    // fija el criterio correspondiente; si no, lo pone tal cual
    export function MapearValor(editor: HTMLInputElement, valor: string): void {
        if (!TieneSelector(editor)) {
            editor.value = valor;
            return;
        }
        const separado = SepararCriterio(valor);
        editor.value = separado.valor;
        Fijar(editor, separado.criterio);
    }

    // Los filtros de algunas vistas eligen el campo con un prefijo 'letra:' (p.e. 'd:garcia' en facturas emitidas): el prefijo se deja
    // delante y los símbolos del criterio se ponen detrás ('d:=garcia'), que es donde el servidor los busca tras quitar el prefijo
    export function AplicarCriterio(valor: string, criterio: string): string {
        const conPrefijo = SepararPrefijo(valor);
        return `${conPrefijo.prefijo}${AplicarCriterioSinPrefijo(conPrefijo.resto, criterio)}`;
    }

    function AplicarCriterioSinPrefijo(valor: string, criterio: string): string {
        if (IsNullOrEmpty(valor) || criterio === criterioPorDefecto || LlevaSimbolos(valor))
            return valor;

        switch (criterio) {
            case atCriterio.igual: return MarcarAlternativas(valor, a => `${ltrSimbolos.igual}${a}`);
            case atCriterio.diferente: return `${ltrSimbolos.distinto}${MarcarAlternativas(valor, a => `${ltrSimbolos.igual}${a}`)}`;
            case atCriterio.noContiene: return `${ltrSimbolos.distinto}${valor.trim()}`;
            case atCriterio.comienza: return MarcarAlternativas(valor, a => `${ltrSimbolos.comienzaTermina}${a}`);
            case atCriterio.termina: return MarcarAlternativas(valor, a => `${a}${ltrSimbolos.comienzaTermina}`);
        }
        return valor;
    }

    // inversa de AplicarCriterio (respeta el prefijo 'letra:' igual que ella)
    export function SepararCriterio(valor: string): { criterio: string, valor: string } {
        const conPrefijo = SepararPrefijo(valor);
        const separado = SepararCriterioSinPrefijo(conPrefijo.resto);
        return { criterio: separado.criterio, valor: `${conPrefijo.prefijo}${separado.valor}` };
    }

    // prefijo 'letra:' con el que algunas vistas eligen el campo por el que filtrar; sin prefijo, todo es resto
    function SepararPrefijo(valor: string): { prefijo: string, resto: string } {
        const sinPrefijo = { prefijo: '', resto: valor };
        if (IsNullOrEmpty(valor))
            return sinPrefijo;
        const texto = valor.trimStart();
        const separador = ltrSimbolos.separadorDePrefijoDeCampo;
        const esLetra = texto.length > 0 && texto[0].toLowerCase() !== texto[0].toUpperCase();
        if (!esLetra || texto.substring(1, 1 + separador.length) !== separador)
            return sinPrefijo;
        const largo = 1 + separador.length;
        return { prefijo: texto.substring(0, largo), resto: texto.substring(largo) };
    }

    // si todas las alternativas llevan el mismo símbolo, se quita y se devuelve su criterio; si no (símbolos mezclados o escritos a
    // mano que no corresponden a una opción) se deja el valor tal cual con el criterio por defecto
    function SepararCriterioSinPrefijo(valor: string): { criterio: string, valor: string } {
        const sinCambios = { criterio: criterioPorDefecto, valor: valor };
        if (IsNullOrEmpty(valor))
            return sinCambios;

        const negar = valor.startsWith(ltrSimbolos.distinto);
        const resto = negar ? valor.substring(ltrSimbolos.distinto.length).trim() : valor;
        const alternativas = Alternativas(resto);
        if (alternativas.length === 0)
            return sinCambios;

        const unir = (quitar: (a: string) => string) => alternativas.map(quitar).join(ltrSimbolos.separadorDeAlternativas);
        const empiezaPor = (simbolo: string) => alternativas.every(a => a.startsWith(simbolo) && a.length > simbolo.length);
        const terminaEn = (simbolo: string) => alternativas.every(a => a.endsWith(simbolo) && !a.startsWith(simbolo) && a.length > simbolo.length);

        if (empiezaPor(ltrSimbolos.igual))
            return { criterio: negar ? atCriterio.diferente : atCriterio.igual, valor: unir(a => a.substring(ltrSimbolos.igual.length)) };

        if (negar)
            return LlevaSimbolos(resto) ? sinCambios : { criterio: atCriterio.noContiene, valor: resto };

        if (empiezaPor(ltrSimbolos.comienzaTermina))
            return { criterio: atCriterio.comienza, valor: unir(a => a.substring(ltrSimbolos.comienzaTermina.length)) };

        if (terminaEn(ltrSimbolos.comienzaTermina))
            return { criterio: atCriterio.termina, valor: unir(a => a.substring(0, a.length - ltrSimbolos.comienzaTermina.length)) };

        return sinCambios;
    }

    // si el usuario ya ha escrito los símbolos se respeta lo que ha escrito
    function LlevaSimbolos(valor: string): boolean {
        if (valor.trim().startsWith(ltrSimbolos.distinto))
            return true;
        return Alternativas(valor).some(a => a.startsWith(ltrSimbolos.igual)
            || a.startsWith(ltrSimbolos.comienzaTermina)
            || a.endsWith(ltrSimbolos.comienzaTermina));
    }

    function Alternativas(valor: string): string[] {
        return valor.split(ltrSimbolos.separadorDeAlternativas).map(a => a.trim()).filter(a => a.length > 0);
    }

    function MarcarAlternativas(valor: string, marcar: (alternativa: string) => string): string {
        return Alternativas(valor).map(marcar).join(ltrSimbolos.separadorDeAlternativas);
    }

    function BotonDe(editor: HTMLInputElement): HTMLButtonElement {
        const contenedor = editor.parentElement;
        return Definido(contenedor) ? contenedor.querySelector(`:scope > .${ltrCss.CriterioDeTexto.Boton}`) as HTMLButtonElement : null;
    }

    function ListaDe(editor: HTMLInputElement): HTMLDivElement {
        const contenedor = editor.parentElement;
        return Definido(contenedor) ? contenedor.querySelector(`:scope > .${ltrCss.CriterioDeTexto.Lista}`) as HTMLDivElement : null;
    }

    function CrearSelector(editor: HTMLInputElement): void {
        const contenedor = editor.parentElement;
        if (!Definido(contenedor))
            return;
        ApiControl.IncluirCss(contenedor, ltrCss.CriterioDeTexto.Contenedor);

        const boton = document.createElement('button');
        boton.type = 'button';
        ApiControl.IncluirCss(boton, ltrCss.CriterioDeTexto.Boton);
        boton.addEventListener('click', (evento: MouseEvent) => {
            evento.preventDefault();
            evento.stopPropagation();
            AlternarLista(editor);
        });

        const lista = document.createElement('div');
        ApiControl.IncluirCss(lista, ltrCss.CriterioDeTexto.Lista);
        for (const opcion of ltrCriteriosDeTexto) {
            const elemento = document.createElement('div');
            ApiControl.IncluirCss(elemento, ltrCss.CriterioDeTexto.Opcion);
            elemento.setAttribute(atControl.criterioElegido, opcion.criterio);

            const icono = document.createElement('span');
            ApiControl.IncluirCss(icono, ltrCss.CriterioDeTexto.Icono);
            icono.textContent = opcion.icono;
            elemento.appendChild(icono);
            elemento.appendChild(document.createTextNode(opcion.titulo));

            elemento.addEventListener('click', (evento: MouseEvent) => {
                evento.stopPropagation();
                Fijar(editor, opcion.criterio);
                CerrarLista();
                editor.focus();
            });
            lista.appendChild(elemento);
        }

        editor.insertAdjacentElement('afterend', boton);
        boton.insertAdjacentElement('afterend', lista);
    }

    // la lista se posiciona fija respecto a la ventana para que no la recorten los paneles y modales de filtro con overflow
    function AlternarLista(editor: HTMLInputElement): void {
        const lista = ListaDe(editor);
        const abierta = Definido(lista) && lista === listaAbierta;
        CerrarLista();
        if (!Definido(lista) || abierta)
            return;

        ApiControl.IncluirCss(lista, ltrCss.CriterioDeTexto.ListaDesplegada);
        const rectangulo = BotonDe(editor).getBoundingClientRect();
        const izquierda = Math.max(0, Math.min(rectangulo.right - lista.offsetWidth, window.innerWidth - lista.offsetWidth));
        lista.style.top = `${rectangulo.bottom}px`;
        lista.style.left = `${izquierda}px`;
        listaAbierta = lista;
    }

    function CerrarLista(): void {
        if (!Definido(listaAbierta))
            return;
        ApiControl.ExcluirCss(listaAbierta, ltrCss.CriterioDeTexto.ListaDesplegada);
        listaAbierta = null;
    }

    function RegistrarEventosGlobales(): void {
        if (eventosGlobalesRegistrados)
            return;
        eventosGlobalesRegistrados = true;
        document.addEventListener('click', () => CerrarLista());
        document.addEventListener('keydown', (evento: KeyboardEvent) => { if (evento.key === 'Escape') CerrarLista(); });
        window.addEventListener('resize', () => CerrarLista());
        window.addEventListener('scroll', (evento: Event) => {
            if (Definido(listaAbierta) && listaAbierta.contains(evento.target as Node))
                return;
            CerrarLista();
        }, true);
    }
}
