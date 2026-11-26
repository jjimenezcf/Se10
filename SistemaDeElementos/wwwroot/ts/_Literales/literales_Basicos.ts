// Literales básicos: separadores, valores genéricos (literal), prefijos/posfijos, estilos e iconos.
// No depende de ningún otro fichero de literales.
// Orden de carga: ListaDeScripts.cs. Al definirse, un literal solo puede leer los de ficheros anteriores de esa lista.

const newLine = '\n';

const ltrSimbolos = {
    dosPuntosConEspacio: ': ',
    separadorDeRangos: ';',
    separadorDeDosFechas: '-',
    separadorDeDDMMYYYY: '/',
    separadorDeHHMMSS: ':',
    separadorDeCss: ';',
    separadorDeParametrosJs: '#',
    separadorDeValores: '|',
    separadorDeEnteros: ',',
    ArchivoCancelado: '(_)',
    subrayado: '_',
    igual: '=',
    distinto: '!',
    comienzaTermina: '|',
    separadorDeAlternativas: ';',
    separadorDePrefijoDeCampo: ':',
    separadorDeRestrictoresFijos: '~',
    separadorDeEtapas: '|',
    separadorDentroDeUnRestrictorFijo:';'
};

const literal = {
    ClaveDeEncriptacion: 'SistemaDeElementos',
    controlador: 'controlador',
    accion: 'accion',
    vista: 'vista',
    parametros: 'parametros',
    urlAccedida: 'urlAccedida',
    idNegocio: 'id-negocio',
    nombreNegocio: 'nombre-negocio',
    negocio: 'negocio',
    // Nombres de parámetros y propiedades que se repiten en muchos sitios (peticiones ajax, atributos html, propiedades de los dto):
    // se definen sólo aquí y el resto de literales los referencian, para poder seguir su pista
    idElemento: 'idElemento',
    idArchivo: 'idArchivo',
    guid: 'guid',
    enumNegocio: 'enumNegocio',
    idCg: 'idcg',
    Cg: 'cg',
    IdSolicitante: 'idsolicitante',
    Solicitante: 'solicitante',
    id: 'id',
    IdTipo: 'idtipo',
    Tipo: 'Tipo',
    true: 'true',
    false: 'false',
    cero: '0',
    uno: '1',
    Hoy: 'Hoy',
    undefined: 'undefined',
    nombre: 'nombre',
    expresion: 'expresion',
    padre: 'padre',
    ModoDeAcceso: 'ModoDeAcceso',
    filtro: {
        clausulaId: 'id',
        idEditado: 'ideditado',
        vincularCon: 'vincularCon',
        PorVariable: 'porvariable',
        criterio: {
            igual: 'igual',
            diferente: 'diferente',
            entreFechas: 'entrefechas',
            entreImportes: 'entreimportes',
            entreRangos: 'entrerangos'
        }
    },
    menos1: '-1',
    ExpanDeArchivos: "detalle-archivos"
};

const ltrPrefijo = {
    Modal: 'modal-'
}

const ltrPosfijo = {
    Firma: '-firma'
}

const ltrVariables = {
    Cuentas: {
        Sueldos: 'Sueldos'
    }
};

const ltrStyle = {
    display: {
        none: 'none',
        block: 'block'
    },
    visibility: {
        hidden: 'hidden'
    }
};

const ltrIconos = {
    papelera: 'Papelera.png',
    descargar: 'DescargarArchivo.png',
    verArchivo: 'verarchivo.svg',
    firmar: 'firmar.svg',
    bloquearArchivo: 'bloqueararchivo.svg',
    desbloquearArchivo: 'desbloqueararchivo.svg'
};
