// Iconos SVG fijos del front (los que se pintan con un innerHTML). Los que llevan datos (gráficos, marcadores del mapa)
// se componen en el ts que los pinta.
// No depende de ningún otro literal.
// Orden de carga: ListaDeScripts.cs. Al definirse, un literal solo puede leer los de ficheros anteriores de esa lista.
// El robot también está en C#, en UtilidadesIu/Api/Extensiones.cs (IconosIa.Robot): si se cambia uno, cambiar el otro.

const ltrSvg = {
    // robot verde con antenas: pregunta a la IA (fichas del panel de control, modal del manual de una vista y visor de archivos)
    Robot: `<svg width="14" height="14" viewBox="0 0 16 16" fill="none"
        stroke="currentColor" stroke-width="1.3" stroke-linecap="round" stroke-linejoin="round">
        <line x1="5" y1="1.8" x2="6.2" y2="4"/>
        <line x1="11" y1="1.8" x2="9.8" y2="4"/>
        <circle cx="5" cy="1.6" r="0.7" fill="currentColor" stroke="none"/>
        <circle cx="11" cy="1.6" r="0.7" fill="currentColor" stroke="none"/>
        <rect x="2.5" y="4" width="11" height="9.5" rx="2.2"/>
        <circle cx="6" cy="8" r="1.1" fill="currentColor" stroke="none"/>
        <circle cx="10" cy="8" r="1.1" fill="currentColor" stroke="none"/>
        <line x1="6" y1="11.2" x2="10" y2="11.2"/>
    </svg>`,

    // Contenido (no el <svg>) de la i de información: sustituye al lápiz de la cabecera de las modales de sólo consulta
    // (se asigna al innerHTML del svg que ya trae la plantilla, con su viewBox 0 0 24 24)
    ContenidoDeInformacion: '<circle cx="12" cy="12" r="10"/><line x1="12" y1="16" x2="12" y2="12"/><line x1="12" y1="8" x2="12.01" y2="8"/>',

    // botones de modo de vista de las tarjetas del panel de control
    Tarta: `<svg width="13" height="13" viewBox="0 0 14 14" fill="currentColor">
        <circle cx="7" cy="7" r="6" fill="none" stroke="currentColor" stroke-width="1.5"/>
        <path d="M7,7 L7,1 A6,6 0,0,1 13,7 Z" opacity="0.85"/>
        <path d="M7,7 L13,7 A6,6 0,0,1 3.5,12.2 Z" opacity="0.55"/>
        <path d="M7,7 L3.5,12.2 A6,6 0,0,1 1,7 Z" opacity="0.35"/>
    </svg>`,

    Barras: `<svg width="13" height="13" viewBox="0 0 14 14" fill="currentColor">
        <rect x="1"   y="7"  width="3" height="6"/>
        <rect x="5.5" y="4"  width="3" height="9"/>
        <rect x="10"  y="1"  width="3" height="12"/>
        <line x1="1" y1="13" x2="13" y2="13" stroke="currentColor" stroke-width="1"/>
    </svg>`,

    Matriz: `<svg width="13" height="13" viewBox="0 0 15 15" fill="currentColor">
        <rect x="1" y="1" width="4" height="4" opacity="0.9"/>
        <rect x="6" y="1" width="4" height="4" opacity="0.5"/>
        <rect x="1" y="6" width="4" height="4" opacity="0.5"/>
        <rect x="6" y="6" width="4" height="4" opacity="0.9"/>
        <rect x="11" y="1" width="3" height="12" opacity="0.25"/>
        <rect x="1" y="11" width="12" height="3" opacity="0.25"/>
    </svg>`,

    FlechaDerecha: `<svg width="10" height="10" viewBox="0 0 10 10"
        fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
        <polyline points="3,1 7,5 3,9"/>
    </svg>`,

    FlechaIzquierda: `<svg width="10" height="10" viewBox="0 0 10 10"
        fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
        <polyline points="7,1 3,5 7,9"/>
    </svg>`,

    // embudo del botón de aplicar filtro de la modal de dirección del mapa
    Embudo: '<svg xmlns="http://www.w3.org/2000/svg" width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><polygon points="22 3 2 3 10 12.46 10 19 14 21 14 12.46 22 3"/></svg>'
};
