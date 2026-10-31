using ServicioDeDatos;
using MVCSistemaDeElementos.Controllers;
using UtilidadesParaIu;
using Utilidades;
using System.Collections.Generic;
using ServicioDeDatos.Seguridad;
using ModeloDeDto.Logistica;
using GestorDeElementos;
using ServicioDeDatos.Logistica;
using ServicioDeDatos.Juridico;
using ModeloDeDto.Expediente;
using ServicioDeDatos.Expediente;
using ModeloDeDto.Terceros;
using GestorDeElementos.Extensores;
using ModeloDeDto;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDePedidos : DescriptorDeCrud<PedidoDto>
    {
        public DescriptorDePedidos(ContextoSe contexto, string renderCache) : base(contexto,renderCache)
        {
        }
        public DescriptorDePedidos(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto
               , nameof(PedidosController)
               , nameof(PedidosController.CrudPedidos)
               , modo
               , rutaBase: enumNameSpaceTs.Logistica)
        {
            Mnt.Filtro.FiltroDeNombre.CambiarAtributos("Varios", nameof(ltrDeUnPedido.FiltroPorPedidoReferencia), "Buscar por nombre (n:) o referencia (r:) o proveedor (p:)");
            IncluirFiltros();
            Mnt.IncluirMfDeRelacion($"<li id='{menuDeRelaciones}.{enumNegocio.Expediente}' accion-menu='{eventosDeMf.Ped_IrAExpediente}' {AtributosHtml.Mf(enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Consultor, false)}>{enumNegocio.Expediente.Plural()}</li>");
            Mnt.IncluirMfDeRelacion($"<li id='{menuDeRelaciones}.{enumNegocio.Contrato}' accion-menu='{eventosDeMf.Ped_IrAContrato}' {AtributosHtml.Mf(enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Consultor, false)}>{enumNegocio.Contrato.Plural()}</li>");

            modalesParaPedirDatos.Add(new ModalDeTotales(this, typeof(TotalesDePedidos), eventosDeMf.Totalizador_Mostrar, $"Mostrar los totales de la selección"));
            Mnt.IncluirMfContextual($"<li id='{menuContextual}.{eventosDeMf.Totalizador_Mostrar}' accion-menu='{eventosDeMf.Totalizador_Mostrar}' {AtributosHtml.Mf(enumCssOpcionMenu.DeVista, enumModoDeAccesoDeDatos.Consultor, false)}>Mostrar totales</li>");

            modalesParaPedirDatos.Add(new ModalParaPedirDatos(this, typeof(ModificarCabeceraDePedidoDto), eventosDeMf.Ped_ModificarCabecera, "Modificar cabecera"));
            Editor.IncluirMfIndividual("Modificar cabecera", eventosDeMf.Ped_ModificarCabecera, enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Interventor);

            // se abre desde la acción Recepcionar de las líneas del pedido (ver Logistica.Pedido_Recepcionar)
            modalesParaPedirDatos.Add(new ModalParaPedirDatos(this, typeof(RecepcionarLineaDto), eventosDeMf.Ped_RecepcionarLinea, "Recepcionar línea"));
            // "Dar por entregado": recepciona todas las líneas del pedido (ver CrudEdicionPedido.DespuesDeProcesarOpcionMf)
            modalesParaPedirDatos.Add(new ModalParaPedirDatos(this, typeof(RecepcionarPedidoDto), eventosDeMf.Ped_RecepcionarPedido, "Dar por entregado"));
            Editor.IncluirMfIndividual("Dar por entregado", eventosDeMf.Ped_RecepcionarPedido, enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Gestor);
            // se abre desde la acción Devolver de las líneas del pedido (ver Logistica.Pedido_DevolverEntrega)
            modalesParaPedirDatos.Add(new ModalParaPedirDatos(this, typeof(DevolucionDeEntregaDto), eventosDeMf.Ped_DevolverEntrega, "Devolver entrega"));
            Editor.IncluirMfIndividual("Copiar pedido", eventosDeMf.Ped_CopiarPedido, enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Gestor);
            Editor.IncluirMfIndividual("Quitar expediente", eventosDeMf.Ped_QuitarExpediente, enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Interventor);
            Editor.IncluirMfIndividual("Quitar contrato", eventosDeMf.Ped_QuitarContrato, enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Interventor);

            var ampliacion = DescriptorDeDireccion(Ampliaciones.Pedidos.DireccionAlCrear, "Dirección de entrega", typeof(CrearDireccionDeEntregaDto));
            ampliacion.Plegado = false;
            DescriptorDeLineasDeUnPedido();
            DescriptorDeEntregasDeUnPedido();
            DescriptorDeDevolucionesDeUnPedido();
        }

        private void DescriptorDeLineasDeUnPedido()
        {
            var expansor = new DescriptorDeExpansor(Editor, $"{Editor.Id}-lineasDeUnPedido", "Líneas del pedido", mostrarPlegado: true, "Líneas del pedido");
            expansor.EsDetalle = true;
            Editor.Expanes.Insert(0, expansor);

            //Definimos el grid de detalles del pedido
            var columnas = new DescriptorDeColumnas("lineasDeUnPedido");
            columnas.Add(titulo: nameof(LineaDeUnPedidoDto.Orden), alineacion: enumAliniacion.derecha, tamano: 100);
            columnas.Add(titulo: nameof(LineaDeUnPedidoDto.Concepto));
            columnas.Add(titulo: nameof(LineaDeUnPedidoDto.Unidad), autoAjustable: true);
            columnas.Add(titulo: nameof(LineaDeUnPedidoDto.Cantidad), tamano: 150, formato: enumFormato.Numero_6);
            columnas.Add(titulo:"Tarifa", propiedad: nameof(LineaDeUnPedidoDto.Precio), formato: enumFormato.Moneda, tamano: 150);
            columnas.Add(titulo: nameof(LineaDeUnPedidoDto.Descuento), formato: enumFormato.Porcentaje, tamano: 150);
            columnas.Add(titulo: "Importe", propiedad: nameof(LineaDeUnPedidoDto.ImporteDeLinea), formato: enumFormato.Moneda, tamano: 150);
            // lo que queda por entregar de la línea (ver CrudEdicionPedido.AplicarEtapaAlGridDeLineas); lo entregado y devuelto está en sus grids
            columnas.Add(titulo: "CantidadPendiente", propiedad: nameof(LineaDeUnPedidoDto.CantidadPendiente), mostrar: false);
            columnas.Add(titulo: "IdElemento", propiedad: nameof(LineaDeUnPedidoDto.IdElemento), mostrar: false);
            columnas.Add(titulo: "Id", propiedad: nameof(LineaDeUnPedidoDto.Id), mostrar: false);
            // versión de la línea leída, para validarla en el servidor al recepcionarla (ver LineasDeUnPedidoController.epRecibirLinea)
            columnas.Add(titulo: "RowVersion", propiedad: nameof(LineaDeUnPedidoDto.RowVersion), mostrar: false);

            var orden = $"{nameof(LineaDeUnPedidoDto.Orden)}:{enumModoOrdenacion.ascendente.Render()}";


            var parametros = new Dictionary<string, object> {
                   { nameof(GridDeRelacion.Controlador), typeof(LineasDeUnPedidoController) }
                 , { nameof(GridDeRelacion.AccionDeConsulta), nameof(LineasDeUnPedidoController.epLeerElementos)}
                 , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(LineaDeUnPedidoDto.IdElemento) }
                ,  { nameof(GridDeRelacion.OrdenarPor), orden }
                 , { nameof(GridDeRelacion.OcultarSiVacio), false}
                };
            var gridDeRelacion = new GridDeRelacion(expansor, columnas, parametros);
            gridDeRelacion.PermitirBorrar = true;
            gridDeRelacion.acciones.Add(new ColumnaAccion
            {
                accion = Referencia.Pedido_Recepcionar(expansor),
                titulo = "Recepcionar",
                tamano = 100,
                visible = true
            });
            gridDeRelacion.acciones.Add(new ColumnaAccion
            {
                accion = Referencia.Pedido_RecibirLinea(expansor),
                titulo = "Recibida",
                tamano = 100,
                visible = true
            });

            var modalDeCreacion = expansor.DescriptorDeCrearRelaciones(Editor.Crud.Contexto, typeof(LineaDeUnPedidoDto), typeof(LineasDeUnPedidoController), nameof(LineaDeUnPedidoDto.IdElemento), "Añadir línea");
            modalDeCreacion.AccionTrasAbrirModal = $"javascript: {RutaBase}.{enumFunctionTs.Ped_InicializarModalParaCrearLineas}({ExtensorDePedidos.IncrementarOrdenEn(Contexto)})";

            var modalDeEdicion = expansor.DescriptorDeEditarRelaciones(Editor.Crud.Contexto, typeof(LineaDeUnPedidoDto), typeof(LineasDeUnPedidoController), "Editar línea", soloConsulta: false);
            modalDeEdicion.AccionTrasAbrirModal = $"javascript: {RutaBase}.{enumFunctionTs.Ped_InicializarModalParaEditarLineas}();";


        }

        private void IncluirFiltros()
        {
            var modalDeDatos = new ModalDeFiltrado<PedidoDto>(Mnt.Filtro, "filtrosDelPedido", "Datos del pedido");
            Mnt.Filtro.Modales.Add(modalDeDatos);
            FiltroPorProveedor(modalDeDatos);
            FiltrosPorImporte(modalDeDatos);

            var modalDeRelacion = new ModalDeFiltrado<PedidoDto>(Mnt.Filtro, "filtrosDeRelacionesConPedido", "Relaciones con el pedido");
            Mnt.Filtro.Modales.Add(modalDeRelacion);
            FiltroPorExpediente(modalDeRelacion);
            FiltroPorContrato(modalDeRelacion);
            FiltroPorAlmacen(modalDeRelacion);
        }

        // Entregas de las líneas: se consultan (con la modal de recepción), se cancelan o se devuelven (ver CrudEdicionPedido.AplicarEtapaAlGridDeEntregas)
        private void DescriptorDeEntregasDeUnPedido()
        {
            var expansor = new DescriptorDeExpansor(Editor, $"{Editor.Id}-entregasDeUnPedido", "Entregas", mostrarPlegado: true, "Entregas del pedido");
            expansor.EsDetalle = true;
            Editor.Expanes.Insert(1, expansor);

            var columnas = new DescriptorDeColumnas("entregasDeUnPedido");
            columnas.Add(titulo: "Recibido el", propiedad: nameof(EntregaDeUnPedidoDto.RecepcionadaEl), formato: enumFormato.Fecha, tamano: 120);
            columnas.Add(titulo: nameof(EntregaDeUnPedidoDto.Concepto));
            columnas.Add(titulo: "Cantidad recibida", propiedad: nameof(EntregaDeUnPedidoDto.Cantidad), formato: enumFormato.Numero_6, tamano: 150);
            columnas.Add(titulo: "Precio de recepción", propiedad: nameof(EntregaDeUnPedidoDto.Precio), formato: enumFormato.Moneda, tamano: 150);
            columnas.Add(titulo: "Descuento aplicado", propiedad: nameof(EntregaDeUnPedidoDto.Descuento), formato: enumFormato.Porcentaje, tamano: 150);
            columnas.Add(titulo: "Importe de recepción", propiedad: nameof(EntregaDeUnPedidoDto.Importe), formato: enumFormato.Moneda, tamano: 150);
            // para consultar, cancelar o devolver la entrega
            columnas.Add(titulo: "IdLinea", propiedad: nameof(EntregaDeUnPedidoDto.IdLinea), mostrar: false);
            columnas.Add(titulo: "IdAlmacen", propiedad: nameof(EntregaDeUnPedidoDto.IdAlmacen), mostrar: false);
            columnas.Add(titulo: "Almacen", propiedad: nameof(EntregaDeUnPedidoDto.Almacen), mostrar: false);
            columnas.Add(titulo: "IdExpediente", propiedad: nameof(EntregaDeUnPedidoDto.IdExpediente), mostrar: false);
            columnas.Add(titulo: "Expediente", propiedad: nameof(EntregaDeUnPedidoDto.Expediente), mostrar: false);
            columnas.Add(titulo: "CantidadDevuelta", propiedad: nameof(EntregaDeUnPedidoDto.CantidadDevuelta), mostrar: false);
            columnas.Add(titulo: "Contabilizada", propiedad: nameof(EntregaDeUnPedidoDto.Contabilizada), mostrar: false);
            columnas.Add(titulo: "IdElemento", propiedad: nameof(EntregaDeUnPedidoDto.IdElemento), mostrar: false);
            columnas.Add(titulo: "Id", propiedad: nameof(EntregaDeUnPedidoDto.Id), mostrar: false);
            // versión de la entrega leída, para validarla en el servidor al cancelarla o devolverla
            columnas.Add(titulo: "RowVersion", propiedad: nameof(EntregaDeUnPedidoDto.RowVersion), mostrar: false);

            var parametros = new Dictionary<string, object> {
                   { nameof(GridDeRelacion.Controlador), typeof(EntregasDeUnPedidoController) }
                 , { nameof(GridDeRelacion.AccionDeConsulta), nameof(EntregasDeUnPedidoController.epLeerElementos)}
                 , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(EntregaDeUnPedidoDto.IdElemento) }
                 , { nameof(GridDeRelacion.OrdenarPor), $"{nameof(EntregaDeUnPedidoDto.RecepcionadaEl)}:{enumModoOrdenacion.ascendente.Render()}" }
                 , { nameof(GridDeRelacion.OcultarSiVacio), false}
                };
            var gridDeRelacion = new GridDeRelacion(expansor, columnas, parametros);
            gridDeRelacion.PermitirBorrar = false;
            gridDeRelacion.acciones.Add(new ColumnaAccion { accion = Referencia.Pedido_ConsultarEntrega(expansor), titulo = "Consultar", tamano = 100, visible = true });
            gridDeRelacion.acciones.Add(new ColumnaAccion { accion = Referencia.Pedido_CancelarEntrega(expansor), titulo = "Cancelar", tamano = 100, visible = true });
            gridDeRelacion.acciones.Add(new ColumnaAccion { accion = Referencia.Pedido_DevolverEntrega(expansor), titulo = "Devolver", tamano = 100, visible = true });
        }

        // Devoluciones de las entregas: se consultan (con la modal de devolución) o se cancelan (ver CrudEdicionPedido.AplicarEtapaAlGridDeDevoluciones)
        private void DescriptorDeDevolucionesDeUnPedido()
        {
            var expansor = new DescriptorDeExpansor(Editor, $"{Editor.Id}-devolucionesDeUnPedido", "Devoluciones", mostrarPlegado: true, "Devoluciones del pedido");
            expansor.EsDetalle = true;
            Editor.Expanes.Insert(2, expansor);

            var columnas = new DescriptorDeColumnas("devolucionesDeUnPedido");
            columnas.Add(titulo: "Devuelto el", propiedad: nameof(DevolucionDeUnPedidoDto.DevueltaEl), formato: enumFormato.Fecha, tamano: 120);
            columnas.Add(titulo: nameof(DevolucionDeUnPedidoDto.Concepto));
            columnas.Add(titulo: "Cantidad devuelta", propiedad: nameof(DevolucionDeUnPedidoDto.Cantidad), formato: enumFormato.Numero_6, tamano: 150);
            columnas.Add(titulo: "Precio de devolución", propiedad: nameof(DevolucionDeUnPedidoDto.Precio), formato: enumFormato.Moneda, tamano: 150);
            columnas.Add(titulo: "Descuento aplicado", propiedad: nameof(DevolucionDeUnPedidoDto.Descuento), formato: enumFormato.Porcentaje, tamano: 150);
            columnas.Add(titulo: "Importe de devolución", propiedad: nameof(DevolucionDeUnPedidoDto.Importe), formato: enumFormato.Moneda, tamano: 150);
            // para consultar o cancelar la devolución
            columnas.Add(titulo: "IdLineaDeRecepcion", propiedad: nameof(DevolucionDeUnPedidoDto.IdLineaDeRecepcion), mostrar: false);
            columnas.Add(titulo: "Motivo", propiedad: nameof(DevolucionDeUnPedidoDto.Motivo), mostrar: false);
            columnas.Add(titulo: "IdAlmacen", propiedad: nameof(DevolucionDeUnPedidoDto.IdAlmacen), mostrar: false);
            columnas.Add(titulo: "Almacen", propiedad: nameof(DevolucionDeUnPedidoDto.Almacen), mostrar: false);
            columnas.Add(titulo: "IdExpediente", propiedad: nameof(DevolucionDeUnPedidoDto.IdExpediente), mostrar: false);
            columnas.Add(titulo: "Expediente", propiedad: nameof(DevolucionDeUnPedidoDto.Expediente), mostrar: false);
            columnas.Add(titulo: "Contabilizada", propiedad: nameof(DevolucionDeUnPedidoDto.Contabilizada), mostrar: false);
            columnas.Add(titulo: "IdElemento", propiedad: nameof(DevolucionDeUnPedidoDto.IdElemento), mostrar: false);
            columnas.Add(titulo: "Id", propiedad: nameof(DevolucionDeUnPedidoDto.Id), mostrar: false);
            // versión de la devolución leída, para validarla en el servidor al cancelarla
            columnas.Add(titulo: "RowVersion", propiedad: nameof(DevolucionDeUnPedidoDto.RowVersion), mostrar: false);

            var parametros = new Dictionary<string, object> {
                   { nameof(GridDeRelacion.Controlador), typeof(DevolucionesDeUnPedidoController) }
                 , { nameof(GridDeRelacion.AccionDeConsulta), nameof(DevolucionesDeUnPedidoController.epLeerElementos)}
                 , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(DevolucionDeUnPedidoDto.IdElemento) }
                 , { nameof(GridDeRelacion.OrdenarPor), $"{nameof(DevolucionDeUnPedidoDto.DevueltaEl)}:{enumModoOrdenacion.ascendente.Render()}" }
                 , { nameof(GridDeRelacion.OcultarSiVacio), false}
                };
            var gridDeRelacion = new GridDeRelacion(expansor, columnas, parametros);
            gridDeRelacion.PermitirBorrar = false;
            gridDeRelacion.acciones.Add(new ColumnaAccion { accion = Referencia.Pedido_ConsultarDevolucion(expansor), titulo = "Consultar", tamano = 100, visible = true });
            gridDeRelacion.acciones.Add(new ColumnaAccion { accion = Referencia.Pedido_CancelarDevolucion(expansor), titulo = "Cancelar", tamano = 100, visible = true });

            // imprime todas las devoluciones y anexa el pdf a los archivos del pedido, también con el pedido en consulta
            expansor.ControlesDelPie.Add(new Referencia(expansor, $"{expansor.Id}-imprimir", "Imprimir", Referencia.Pedido_ImprimirDevoluciones(),
                "Imprime las devoluciones del pedido y anexa el pdf a sus archivos", enConsultaOcultar: false));
        }

        private void FiltroPorProveedor(ModalDeFiltrado<PedidoDto> modal)
        {
            var ld = new ListasDinamicas<PedidoDto>(modal,
                 etiqueta: enumNegocio.Proveedor.Singular(),
                 filtrarPor: ltrDeUnPedido.FiltroPorProveedor,
                 ayuda: $"seleccione el {enumNegocio.Proveedor.Singular(true)}",
                 seleccionarDe: nameof(ProveedorDto),
                 buscarPor: nameof(ProveedorDto.Expresion),
                 mostrarExpresion: nameof(ProveedorDto.Expresion),
                 criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                 posicion: new Posicion(1, 0),
                 controlador: nameof(ProveedoresController),
                 navegarA: nameof(ProveedoresController.CrudProveedores),
                 restringirPor: "",
                 alSeleccionarBlanquearControl: "")
            { LongitudMinimaParaBuscar = 1 };
            modal.ControlesDeFiltrado.Add(ld);
        }


        private void FiltrosPorImporte(ModalDeFiltrado<PedidoDto> modal)
        {
            var importes = new FiltroEntreImportes<PedidoDto>(modal,
                    etiqueta: "Importe",
                    propiedad: ltrDeUnPedido.FiltroPorImporte,
                    ayuda: "filtrar por rango de importes");
            modal.ControlesDeFiltrado.Add(importes);
        }

        private void FiltrosPorFechaDePedido(ModalDeFiltrado<PedidoDto> modal)
        {
            var fechas = new FiltroEntreFechas<PedidoDto>(modal,
                    etiqueta: "Emitida",
                    propiedad: ltrDeUnPedido.FiltroPorFechaDePedido,
                    ayuda: "pedidos emitidas entre fechas");

            modal.ControlesDeFiltrado.Add(fechas);
        }

        private void FiltrosPorFechaDeEntrega(ModalDeFiltrado<PedidoDto> modal)
        {
            var fechas = new FiltroEntreFechas<PedidoDto>(modal,
                    etiqueta: "Vence el",
                    propiedad: nameof(ltrDeUnPedido.FiltroPorFechaDeEntrega),
                    ayuda: "pedidos que vencen entre fechas");

            modal.ControlesDeFiltrado.Add(fechas);
        }


        private static void FiltroPorExpediente(ModalDeFiltrado<PedidoDto> modal)
        {
            var lista = new ListasDinamicas<PedidoDto>(modal,
                etiqueta: enumNegocio.Expediente.Singular(),
                filtrarPor: ltrDeUnPedido.IdExpediente,
                ayuda: "seleccione el expediente",
                seleccionarDe: nameof(ExpedienteDto),
                buscarPor: nameof(ltrDeUnExpediente.SelectorParaUnPedido),
                mostrarExpresion: nameof(ExpedienteDto.Expresion),
                criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                posicion: new Posicion(1, 0),
                controlador: nameof(ExpedientesController),
                navegarA: nameof(ExpedientesController.CrudExpedientes),
                restringirPor: "",
                alSeleccionarBlanquearControl: "")
            {
                LongitudMinimaParaBuscar = 1
            };

            var opciones = new Dictionary<string, string> {
                      { $"{ltrParametrosNeg.MostrarTodos}", "con o sin Expediente" }
                    , { $"{ltrParametrosNeg.ConRelacion}", "pedidos con Expediente" }
                    , { $"{ltrParametrosNeg.SinRelacion}", "pedidos sin Expediente" } };

            modal.ControlesDeFiltrado.Add(new FiltroDeRelacion<PedidoDto>(modal,
                lista: lista,
                opciones: opciones,
                propiedad: ltrDeUnPedido.AsociadaAUnExpediente,
                filtrarPor: ltrDeUnPedido.AsociadaAUnExpediente,
                ayuda: "Filtros de expedientes"
                ));
        }


        private static void FiltroPorAlmacen(ModalDeFiltrado<PedidoDto> modal)
        {
            var lista = new ListasDinamicas<PedidoDto>(modal,
                etiqueta: enumNegocio.Almacen.Singular(),
                filtrarPor: ltrDeUnPedido.IdAlmacen,
                ayuda: "seleccione el almacén",
                seleccionarDe: nameof(AlmacenDto),
                buscarPor: nameof(AlmacenDto.Expresion),
                mostrarExpresion: nameof(AlmacenDto.Expresion),
                criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                posicion: new Posicion(1, 0),
                controlador: nameof(AlmacenesController),
                navegarA: nameof(AlmacenesController.CrudAlmacenes),
                restringirPor: "",
                alSeleccionarBlanquearControl: "")
            {
                LongitudMinimaParaBuscar = 1
            };

            var opciones = new Dictionary<string, string> {
                      { $"{ltrParametrosNeg.MostrarTodos}", "con o sin almacén" }
                    , { $"{ltrParametrosNeg.ConRelacion}", "pedidos con almacén" }
                    , { $"{ltrParametrosNeg.SinRelacion}", "pedidos sin almacén" } };

            modal.ControlesDeFiltrado.Add(new FiltroDeRelacion<PedidoDto>(modal,
                lista: lista,
                opciones: opciones,
                propiedad: ltrDeUnPedido.AsociadaAUnAlmacen,
                filtrarPor: ltrDeUnPedido.AsociadaAUnAlmacen,
                ayuda: "Filtros de almacenes"
                ));
        }

        private static void FiltroPorContrato(ModalDeFiltrado<PedidoDto> modal)
        {
            var lista = new ListasDinamicas<PedidoDto>(modal,
                etiqueta: enumNegocio.Contrato.Singular(),
                filtrarPor: ltrDeUnPedido.IdContrato,
                ayuda: "seleccione el contrato de compra",
                seleccionarDe: nameof(PedidoDto),
                buscarPor: nameof(ltrDeUnContrato.SelectorParaUnPedido),
                mostrarExpresion: nameof(PedidoDto.Expresion),
                criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                posicion: new Posicion(1, 0),
                controlador: nameof(ContratosController),
                navegarA: nameof(ContratosController.CrudContratos),
                restringirPor: "",
                alSeleccionarBlanquearControl: "")
            {
                LongitudMinimaParaBuscar = 1,
                ParametrosParaNavegar = nameof(ltrParametrosEp.Clase) + "=" + nameof(enumClaseDeContrato.Compra)
            };

            var opciones = new Dictionary<string, string> {
                      { $"{ltrParametrosNeg.MostrarTodos}", "con o sin contrato" }
                    , { $"{ltrParametrosNeg.ConRelacion}", "pedidos con contrato" }
                    , { $"{ltrParametrosNeg.SinRelacion}", "pedidos sin contrato" } };

            modal.ControlesDeFiltrado.Add(new FiltroDeRelacion<PedidoDto>(modal,
                lista: lista,
                opciones: opciones,
                propiedad: ltrDeUnPedido.AsociadaAUnContrato,
                filtrarPor: ltrDeUnPedido.AsociadaAUnContrato,
                ayuda: "Filtros de contratos"
                ));
        }

        public override string RenderControl()
        {
            if (!_renderCache.IsNullOrEmpty())
                return _renderCache;


            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
            if (ServicioDeCaches.Obtener(CacheDe.RenderCrud).ContainsKey(indice))
                return (string)ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice];

            var render = base.RenderControl();
            render = render +
                   $@"<script src='../../js/{RutaBase}/ApiDeLogistica.js?v={System.DateTime.Now.Ticks}'></script>
                      <script src=¨../../js/{RutaBase}/Pedidos.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{      
                           {RutaBase}.CrearCrudDePedidos('{Mnt.IdHtml}', '{Creador.IdHtml}', '{Editor.IdHtml}', '{Borrado.IdHtml}') 
                         }}
                         catch(error) {{                           
                            MensajesSe.Error('Creando el crud', error.message);
                         }}
                      </script>
                    ";
            ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice] = render.Render();
			return (string)ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice];
        }

    }
}
