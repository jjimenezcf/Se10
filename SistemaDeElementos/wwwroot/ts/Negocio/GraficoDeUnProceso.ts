//import sigma from "../../lib/sigma/dist/sigma.min.js";
//import Graph from "../../lib/graphology-types/index.js";

//export function PintarGrafico() {

//    // Crear un nuevo grafo
//    const graph = new sigma.Graph();

//    // Agregar nodos al grafo
//    graph.addNode({
//        id: 'n1',
//        label: 'Nodo 1',
//        x: 0,
//        y: 0,
//        size: 10,
//        color: '#f00'
//    });
//    graph.addNode({
//        id: 'n2',
//        label: 'Nodo 2',
//        x: 150,
//        y: 100,
//        size: 10,
//        color: '#0f0'
//    });
//    graph.addNode({
//        id: 'n3',
//        label: 'Nodo 3',
//        x: 300,
//        y: 0,
//        size: 10,
//        color: '#00f'
//    });
//    graph.addNode({
//        id: 'n4',
//        label: 'Nodo 4',
//        x: 450,
//        y: 100,
//        size: 10,
//        color: '#ff0'
//    });

//    // Agregar bordes al grafo
//    graph.addEdge({
//        id: 'e1',
//        source: 'n1',
//        target: 'n2',
//        type: 'curve',
//        size: 1,
//        color: '#000'
//    });
//    graph.addEdge({
//        id: 'e2',
//        source: 'n2',
//        target: 'n3',
//        type: 'curve',
//        size: 1,
//        color: '#000'
//    });
//    graph.addEdge({
//        id: 'e3',
//        source: 'n3',
//        target: 'n4',
//        type: 'curve',
//        size: 1,
//        color: '#000'
//    });
//    graph.addEdge({
//        id: 'e4',
//        source: 'n4',
//        target: 'n1',
//        type: 'curve',
//        size: 1,
//        color: '#000'
//    });

//    // Configurar Sigma
//    const sigmaInstance = new sigma({
//        container: 'my-sigma-container',
//        graph: graph,
//        settings: {
//            defaultEdgeType: 'curve'
//        }
//    });

//    // Actualizar la visualización
//    sigmaInstance.refresh();
//}

