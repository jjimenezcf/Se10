using ServicioDeDatos;
using MVCSistemaDeElementos.Controllers;
using UtilidadesParaIu;
using Utilidades;
using ModeloDeDto;
using ModeloDeDto.Logistica;
using ModeloDeDto.Ventas;
using ModeloDeDto.MaestrosTecnico;
using System.Collections.Generic;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using ServicioDeDatos.Seguridad;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeVentas : DescriptorDeCrud<VentaDto>
    {
        public DescriptorDeVentas(ContextoSe contexto, string renderCache) : base(contexto, renderCache)
        {
        }

        public DescriptorDeVentas(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto
               , nameof(VentasController)
               , nameof(VentasController.CrudVentas)
               , modo
               , rutaBase: enumNameSpaceTs.Logistica)
        {
            // en el bloque general no se filtra por centro gestor: en su sitio va el almacén (filtro, y restrictor al llegar desde el menú del
            // almacén)
            Mnt.BloqueGeneral.QuitarControl(Mnt.Filtro.FiltroDeCg.Propiedad);
            new ListasDinamicas<VentaDto>(Mnt.BloqueGeneral,
                etiqueta: enumNegocio.Almacen.Singular(),
                filtrarPor: nameof(VentaDto.IdAlmacen),
                ayuda: "seleccione el almacén",
                seleccionarDe: nameof(AlmacenDto),
                buscarPor: nameof(AlmacenDto.Expresion),
                mostrarExpresion: nameof(AlmacenDto.Expresion),
                criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                posicion: new Posicion(0, 0),
                controlador: nameof(AlmacenesController),
                navegarA: nameof(AlmacenesController.CrudAlmacenes),
                restringirPor: "",
                alSeleccionarBlanquearControl: "")
            { LongitudMinimaParaBuscar = 1 };

            // se abre desde la acción Devolver de las líneas de un documento de venta facturado (ver Logistica.Dsa_DevolverLinea)
            modalesParaPedirDatos.Add(new ModalParaPedirDatos(this, typeof(DevolverVentaDto), eventosDeMf.Dsa_DevolverLinea, "Devolver línea"));
            modalesParaPedirDatos.Add(new ModalParaPedirDatos(this, typeof(ModificarLineaVentaDto), eventosDeMf.Dsa_ModificarLineaServida, "Modificar lo servido"));

            // "Cambiar datos" del menú de edición, debajo de "Permiso del elemento" (ver Logistica.CrudEdicionVenta.AjustarCambiarDatos y
            // VentasController.epCambiarDatos)
            modalesParaPedirDatos.Add(new ModalParaPedirDatos(this, typeof(CambiarDatosDeVentaDto), eventosDeMf.Dsa_CambiarDatos, "Cambiar datos del documento de venta"));
            // con permiso de consultor: en servido la edición está en consulta; el permiso de gestor lo valida epCambiarDatos
            Editor.IncluirMfIndividual("Cambiar datos", eventosDeMf.Dsa_CambiarDatos, enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Consultor);

            IncluirFiltrosDeVentas();
            DescriptorDeLineasDeUnaVenta();
            DescriptorDeDevolucionesDeUnaVenta();
        }

        // Líneas devueltas del documento de venta (las de las devoluciones de venta de sus líneas), con el check para aceptar la devolución
        // si aún está solicitada (ver Logistica.Dsa_AceptarDevolucion y CrudEdicionVenta.AjustarGridDeDevoluciones)
        private void DescriptorDeDevolucionesDeUnaVenta()
        {
            var expansor = new DescriptorDeExpansor(Editor, $"{Editor.Id}-devolucionesDeUnaVenta", "Devoluciones", mostrarPlegado: true, "Líneas devueltas del documento de venta");
            expansor.EsDetalle = true;
            Editor.Expanes.Insert(1, expansor);

            var columnas = new DescriptorDeColumnas("devolucionesDeUnaVenta");
            columnas.Add(titulo: nameof(LineaDeUnaDevolucionDeVentaDto.Devolucion), propiedad: nameof(LineaDeUnaDevolucionDeVentaDto.Devolucion), tamano: 150);
            columnas.Add(titulo: "Artículo devuelto", propiedad: nameof(LineaDeUnaDevolucionDeVentaDto.LineaDeVenta));
            columnas.Add(titulo: "Cantidad devuelta", propiedad: nameof(LineaDeUnaDevolucionDeVentaDto.Cantidad), formato: enumFormato.Numero_6, tamano: 150);
            columnas.Add(titulo: "Estado", propiedad: nameof(LineaDeUnaDevolucionDeVentaDto.EstadoDeLaDevolucion), tamano: 120);
            columnas.Add(titulo: "Aceptada", propiedad: nameof(LineaDeUnaDevolucionDeVentaDto.Aceptada), mostrar: false);
            columnas.Add(titulo: "Cancelada", propiedad: nameof(LineaDeUnaDevolucionDeVentaDto.Cancelada), mostrar: false);
            columnas.Add(titulo: "IdVenta", propiedad: nameof(LineaDeUnaDevolucionDeVentaDto.IdVenta), mostrar: false);
            columnas.Add(titulo: "IdElemento", propiedad: nameof(LineaDeUnaDevolucionDeVentaDto.IdElemento), mostrar: false);
            columnas.Add(titulo: "Id", propiedad: nameof(LineaDeUnaDevolucionDeVentaDto.Id), mostrar: false);

            var parametros = new Dictionary<string, object> {
                   { nameof(GridDeRelacion.Controlador), typeof(LineasDeUnaDevolucionDeVentaController) }
                 , { nameof(GridDeRelacion.AccionDeConsulta), nameof(LineasDeUnaDevolucionDeVentaController.epLeerElementos)}
                 , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(LineaDeUnaDevolucionDeVentaDto.IdVenta) }
                 , { nameof(GridDeRelacion.OrdenarPor), $"{nameof(LineaDeUnaDevolucionDeVentaDto.Id)}:{enumModoOrdenacion.ascendente.Render()}" }
                 , { nameof(GridDeRelacion.OcultarSiVacio), false}
                 // icono de navegar a la edición de la devolución (el IdElemento de la línea)
                 , { nameof(GridDeRelacion.PaginaDondeNavegarAlEditar), $"{nameof(DevolucionesDeVentaController)}/{nameof(DevolucionesDeVentaController.CrudDevolucionesDeVenta)}?id={nameof(LineaDeUnaDevolucionDeVentaDto.IdElemento)}" }
                };
            var gridDeRelacion = new GridDeRelacion(expansor, columnas, parametros);
            gridDeRelacion.PermitirBorrar = false;
            gridDeRelacion.acciones.Add(new ColumnaAccion { accion = Referencia.Venta_AceptarDevolucion(expansor), titulo = "Aceptar", tamano = 100, visible = true });
            // en lugar de borrar, se cancela la devolución (DDV: Cancelar); aceptar y cancelar sólo mientras se tramita
            gridDeRelacion.acciones.Add(new ColumnaAccion { accion = Referencia.Venta_CancelarDevolucion(expansor), titulo = "Cancelar", tamano = 100, visible = true });
        }

        // Bloque de filtrado "Datos de ventas", como "Relaciones con el pedido" en DescriptorDePedidos
        private void IncluirFiltrosDeVentas()
        {
            var modal = new ModalDeFiltrado<VentaDto>(Mnt.Filtro, "filtrosDeVentas", "Datos de ventas");
            Mnt.Filtro.Modales.Add(modal);
            FiltroPorUnitario(modal);
            FiltroPorImporte(modal);
            FiltroPorFacturaEmt(modal);
        }

        // documentos de venta con alguna línea del material (ver FiltrosDeDocumentosDeVenta.FiltroPorUnitario)
        private static void FiltroPorUnitario(ModalDeFiltrado<VentaDto> modal)
        {
            var ld = new ListasDinamicas<VentaDto>(modal,
                 etiqueta: "Unitario",
                 filtrarPor: ServicioDeDatos.Logistica.ltrDeUnaVenta.FiltroPorUnitario,
                 ayuda: "documentos de venta con alguna línea del material",
                 seleccionarDe: nameof(UnitarioDto),
                 buscarPor: nameof(UnitarioDto.Expresion),
                 mostrarExpresion: nameof(UnitarioDto.Expresion),
                 criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                 posicion: new Posicion(1, 0),
                 controlador: nameof(UnitariosController),
                 navegarA: nameof(UnitariosController.CrudUnitarios),
                 restringirPor: "",
                 alSeleccionarBlanquearControl: "")
            { LongitudMinimaParaBuscar = 1 };
            modal.ControlesDeFiltrado.Add(ld);
        }

        // rango de la base imponible del documento de venta, sin iva (ver FiltrosDeDocumentosDeVenta.FiltroPorImporte)
        private static void FiltroPorImporte(ModalDeFiltrado<VentaDto> modal)
        {
            modal.ControlesDeFiltrado.Add(new FiltroEntreImportes<VentaDto>(modal,
                    etiqueta: "Importe",
                    propiedad: ServicioDeDatos.Logistica.ltrDeUnaVenta.FiltroPorImporte,
                    ayuda: "filtrar por rango de la base imponible"));
        }

        // Por su prefactura (también restrictor al navegar desde las relaciones de una factura emitida) o con / sin factura (ver
        // FiltrosDeDocumentosDeVenta.FiltroPorFactura)
        private static void FiltroPorFacturaEmt(ModalDeFiltrado<VentaDto> modal)
        {
            var lista = new ListasDinamicas<VentaDto>(modal,
                etiqueta: enumNegocio.FacturaEmitida.Singular(),
                filtrarPor: nameof(VentaDto.IdFacturaEmt),
                ayuda: $"seleccione la {enumNegocio.FacturaEmitida.Singular(true)}",
                seleccionarDe: nameof(FacturaEmtDto),
                buscarPor: nameof(FacturaEmtDto.Expresion),
                mostrarExpresion: nameof(FacturaEmtDto.Expresion),
                criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                posicion: new Posicion(1, 0),
                controlador: nameof(FacturasEmtController),
                navegarA: nameof(FacturasEmtController.CrudFacturasEmt),
                restringirPor: "",
                alSeleccionarBlanquearControl: "")
            { LongitudMinimaParaBuscar = 1 };

            var opciones = new Dictionary<string, string> {
                      { $"{ltrParametrosNeg.MostrarTodos}", "con o sin factura" }
                    , { $"{ltrParametrosNeg.ConRelacion}", "facturados" }
                    , { $"{ltrParametrosNeg.SinRelacion}", "sin facturar" } };

            modal.ControlesDeFiltrado.Add(new FiltroDeRelacion<VentaDto>(modal,
                lista: lista,
                opciones: opciones,
                propiedad: ServicioDeDatos.Logistica.ltrDeUnaVenta.Facturado,
                filtrarPor: ServicioDeDatos.Logistica.ltrDeUnaVenta.Facturado,
                ayuda: $"Filtros de {enumNegocio.FacturaEmitida.Plural(true)}"
                ));
        }

        private void DescriptorDeLineasDeUnaVenta()
        {
            var expansor = new DescriptorDeExpansor(Editor, $"{Editor.Id}-lineasDeUnaVenta", "Detalle", mostrarPlegado: true, "Líneas del documento de venta");
            expansor.EsDetalle = true;
            Editor.Expanes.Insert(0, expansor);

            //Definimos el grid de detalles del documento de venta
            var columnas = new DescriptorDeColumnas("lineasDeUnaVenta");
            columnas.Add(titulo: nameof(LineaDeUnaVentaDto.Orden), alineacion: enumAliniacion.derecha, tamano: 100);
            columnas.Add(titulo: nameof(LineaDeUnaVentaDto.Unitario));
            columnas.Add(titulo: nameof(LineaDeUnaVentaDto.Cantidad), tamano: 150, formato: enumFormato.Numero_6);
            columnas.Add(titulo: nameof(LineaDeUnaVentaDto.Precio), formato: enumFormato.Moneda, tamano: 150);
            columnas.Add(titulo: nameof(LineaDeUnaVentaDto.Descuento), formato: enumFormato.Porcentaje, tamano: 100);
            columnas.Add(titulo: nameof(LineaDeUnaVentaDto.Importe), formato: enumFormato.Moneda, tamano: 150);
            columnas.Add(titulo: "Precio almacén", propiedad: nameof(LineaDeUnaVentaDto.PrecioAlmacen), formato: enumFormato.Moneda, tamano: 150);
            columnas.Add(titulo: nameof(LineaDeUnaVentaDto.Margen), formato: enumFormato.Porcentaje, tamano: 100);
            columnas.Add(titulo: "CantidadDevuelta", propiedad: nameof(LineaDeUnaVentaDto.CantidadDevuelta), mostrar: false);
            columnas.Add(titulo: "IdElemento", propiedad: nameof(LineaDeUnaVentaDto.IdElemento), mostrar: false);
            columnas.Add(titulo: "Id", propiedad: nameof(LineaDeUnaVentaDto.Id), mostrar: false);

            var orden = $"{nameof(LineaDeUnaVentaDto.Orden)}:{enumModoOrdenacion.ascendente.Render()}";

            var parametros = new Dictionary<string, object> {
                   { nameof(GridDeRelacion.Controlador), typeof(LineasDeUnaVentaController) }
                 , { nameof(GridDeRelacion.AccionDeConsulta), nameof(LineasDeUnaVentaController.epLeerElementos)}
                 , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(LineaDeUnaVentaDto.IdElemento) }
                ,  { nameof(GridDeRelacion.OrdenarPor), orden }
                 , { nameof(GridDeRelacion.OcultarSiVacio), false}
                };
            var gridDeRelacion = new GridDeRelacion(expansor, columnas, parametros);
            gridDeRelacion.PermitirBorrar = true;
            // sólo se ve con el documento de venta servido (modifica lo servido) o facturado (devuelve), ver Logistica.Dsa_DevolverLinea; en la
            // solicitud se ve la papelera de borrar la línea
            gridDeRelacion.acciones.Add(new ColumnaAccion { accion = Referencia.Venta_DevolverLinea(expansor), titulo = "Devolver", tamano = 100, visible = true });

            var modalDeCreacion = expansor.DescriptorDeCrearRelaciones(Editor.Crud.Contexto, typeof(LineaDeUnaVentaDto), typeof(LineasDeUnaVentaController), nameof(LineaDeUnaVentaDto.IdElemento), "Añadir línea");
            modalDeCreacion.AccionTrasAbrirModal = $"javascript: {RutaBase}.{enumFunctionTs.Dsa_InicializarModalParaCrearLineas}({ExtensorDeVentas.IncrementarOrdenEn(Contexto)})";

            expansor.DescriptorDeEditarRelaciones(Editor.Crud.Contexto, typeof(LineaDeUnaVentaDto), typeof(LineasDeUnaVentaController), "Editar línea", soloConsulta: false);
        }

        public override string RenderControl()
        {
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
            if (ServicioDeCaches.Obtener(CacheDe.RenderCrud).ContainsKey(indice))
                return (string)ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice];
            var render = base.RenderControl();

            render = render +
                   $@"<script src='../../js/{RutaBase}/ApiDeLogistica.js?v={System.DateTime.Now.Ticks}'></script>
                      <script src=¨../../js/{RutaBase}/Ventas.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{
                           {RutaBase}.CrearCrudDeVentas('{Mnt.IdHtml}', '{Creador.IdHtml}', '{Editor.IdHtml}', '{Borrado.IdHtml}')
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
