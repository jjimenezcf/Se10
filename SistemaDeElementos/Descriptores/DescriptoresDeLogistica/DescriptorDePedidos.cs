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

            Editor.IncluirMfIndividual("Quitar expediente", eventosDeMf.Ped_QuitarExpediente, enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Interventor);
            Editor.IncluirMfIndividual("Quitar contrato", eventosDeMf.Ped_QuitarContrato, enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Interventor);

            var ampliacion = DescriptorDeDireccion(Ampliaciones.Pedidos.DireccionAlCrear, "Dirección de entrega", typeof(CrearDireccionDeEntregaDto));
            ampliacion.Plegado = false;
            DescriptorDeLineasDeUnPedido();
        }

        private void DescriptorDeLineasDeUnPedido()
        {
            var expansor = new DescriptorDeExpansor(Editor, $"{Editor.Id}-lineasDeUnPedido", "Detalle", mostrarPlegado: true, "Líneas del pedido");
            expansor.EsDetalle = true;
            Editor.Expanes.Insert(0, expansor);

            //Definimos el grid de detalles del pedido
            var columnas = new DescriptorDeColumnas("lineasDeUnPedido");
            columnas.Add(titulo: nameof(LineaDeUnPedidoDto.Orden), alineacion: enumAliniacion.derecha, tamano: 100);
            columnas.Add(titulo: nameof(LineaDeUnPedidoDto.Concepto));
            columnas.Add(titulo: nameof(LineaDeUnPedidoDto.Unidad), autoAjustable: true);
            columnas.Add(titulo: nameof(LineaDeUnPedidoDto.Cantidad), tamano: 150, formato: enumFormato.Numero_6);
            columnas.Add(titulo: nameof(LineaDeUnPedidoDto.Precio), formato: enumFormato.Moneda, tamano: 150);
            columnas.Add(titulo: nameof(LineaDeUnPedidoDto.Descuento), formato: enumFormato.Porcentaje, tamano: 150);
            columnas.Add(titulo: "Importe", propiedad: nameof(LineaDeUnPedidoDto.ImporteDeLinea), formato: enumFormato.Moneda, tamano: 150);
            columnas.Add(titulo: "IdElemento", propiedad: nameof(LineaDeUnPedidoDto.IdElemento), mostrar: false);
            columnas.Add(titulo: "Id", propiedad: nameof(LineaDeUnPedidoDto.Id), mostrar: false);

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

            var modalDeCreacion = expansor.DescriptorDeCrearRelaciones(Editor.Crud.Contexto, typeof(LineaDeUnPedidoDto), typeof(LineasDeUnPedidoController), nameof(LineaDeUnPedidoDto.IdElemento), "Añadir línea");
            modalDeCreacion.AccionTrasAbrirModal = $"javascript: {RutaBase}.{enumFunctionTs.Ped_InicializarModalParaCrearLineas}({ExtensorDePresupuestos.IncrementarOrdenEn(Contexto)})";

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
