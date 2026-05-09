using GestorDeElementos;
using ModeloDeDto;
using ModeloDeDto.Gastos;
using ModeloDeDto.SistemaDocumental;
using ModeloDeDto.Terceros;
using MVCSistemaDeElementos.Controllers;
using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Gastos;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.SistemaDocumental;
using System.Collections.Generic;
using Utilidades;
using UtilidadesParaIu;
namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDePagos : DescriptorDeCrud<PagoDto>
    {
        public DescriptorDePagos(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto
               , nameof(PagosController)
               , nameof(PagosController.CrudPagos)
               , modo
               , rutaBase: enumNameSpaceTs.Gasto)
        {
            var modalDeRelacion = new ModalDeFiltrado<PagoDto>(Mnt.Filtro, "filtrosDePagos", "Filtro de pagos");
            Mnt.Filtro.Modales.Add(modalDeRelacion);
            FiltroPorAcreedor(modalDeRelacion);
            FiltroPorFactura(modalDeRelacion);
            FiltroPorCuentasBancarias(modalDeRelacion);
            FiltroPorTarjetas(modalDeRelacion);
            FiltrosPorFechaDePagarEl(modalDeRelacion);
            FiltrosPorFechaDePagadoEl(modalDeRelacion);
            FiltrosPorImporteDelPago(modalDeRelacion);
            FiltroPorRemesa(modalDeRelacion);
            FiltrosPorFormaDePago(modalDeRelacion);
            FiltrosParaPreasientos(modalDeRelacion);
            FiltroPorLoteContable(modalDeRelacion);
            DescriptorDeRemesas();

            modalesParaPedirDatos.Add(new ModalDeTotales(this, typeof(TotalesDePago), eventosDeMf.Totalizador_Mostrar, $"Mostrar los totales de la selección"));
            Mnt.IncluirMfContextual($"<li id='{menuContextual}.{eventosDeMf.Totalizador_Mostrar}' accion-menu='{eventosDeMf.Totalizador_Mostrar}' {AtributosHtml.Mf(enumCssOpcionMenu.DeVista, enumModoDeAccesoDeDatos.Consultor, false)}>Mostrar totales</li>");


            var idsDeUsuarios = enumNegocio.Preasiento.Parametro(enumParametrosDePreasiento.SPR_Usuarios_Con_Permiso_Para_Generar, valorPorDefecto: Literal.Cero).Valor.ToLista<int>();

            if (idsDeUsuarios.Contains(contexto.DatosDeConexion.IdUsuario))
            {
                Mnt.IncluirMfContextual($"<li id='{menuContextual}.{eventosDeMf.Pag_CancelarPreasientos}' accion-menu='{eventosDeMf.Far_CancelarPreasientos}' {AtributosHtml.Mf(enumCssOpcionMenu.DeVista, enumModoDeAccesoDeDatos.Administrador, false)}>Cancelar preasientos</li>");
                Editor.IncluirMfIndividual("Generar preasiento", eventosDeMf.Pag_GenerarPreasiento, enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Administrador);
            }

            Mnt.OrdenacionInicial = $"{nameof(PagoDto.Referencia)}:{nameof(PagoDto.Referencia)}:{enumModoOrdenacion.descendente.Render()}";
        }
        public DescriptorDePagos(ContextoSe contexto, string renderCache) : base(contexto, renderCache)
        {
        }


        private static void FiltrosPorFormaDePago(ModalDeFiltrado<PagoDto> modalDeRelacion)
        {
            var opciones = new Dictionary<string, string> {
                { $"{ltrDeUnPago.FiltroDePagosContado}", "Pago al contado" },
                { $"{ltrDeUnPago.FiltroDePagosTarjeta}", "Pago con tarjeta" },
                { $"{ltrDeUnPago.FiltroDePagosDomiciliado}", "Pago por domicialición" },
                { $"{ltrDeUnPago.FiltroDePagosTransferencia}", "Pago por transferencia" },
                { $"{ltrDeUnPago.FiltroDePagosRemesa}", "Pago remesado" },
            };
            modalDeRelacion.ControlesDeFiltrado.Add(new ListaDeValoresParaFiltrado<PagoDto>(modalDeRelacion,
                nameof(PagoDtm) + "-" + nameof(PagoDtm.Clase),
                ltrDeUnPago.FiltroPorFormaDePago,
                opciones,
                "filtrar por clase y modo de pago"));
        }

        private static void FiltrosParaPreasientos(ModalDeFiltrado<PagoDto> modal)
        {
            var opciones = new Dictionary<string, string> {
                { $"{ltrDeUnPago.FiltroConSpr}", "Pagos con preasiento" },
                { $"{ltrDeUnPago.FiltroSinSpr}", "Pagos sin preasiento" },
                { $"{ltrDeUnPago.FiltroConFacSin}", "Pagos de facturas sin preasiento" },
                { $"{ltrDeUnPago.FiltroConSprCan}", "Con preasiento cancelado" }
            };
            var lv = new ListaDeValoresParaFiltrado<PagoDto>(modal,
                nameof(PagoDto) + "-" + nameof(PagoDto.Preasiento),
                ltrDeUnPago.FiltroSiHayPreasiento,
                opciones,
                "filtrar si hay preasientos");
            modal.ControlesDeFiltrado.Add(lv);
        }


        private static void FiltroPorLoteContable(ModalDeFiltrado<PagoDto> modal)
        {
            var lista = new ListasDinamicas<PagoDto>(modal,
                etiqueta: ltrDeUnLoteContable.EtiquetaLoteContable,
                filtrarPor: ltrDeUnPago.IdLoteContable,
                ayuda: "seleccione lote contable",
                seleccionarDe: nameof(CircuitoDocDtm),
                buscarPor: nameof(ltrDeUnCircuito.SeleccionarParaFiltrarPorLoteContable),
                mostrarExpresion: nameof(CircuitoDocDto.Expresion),
                criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                posicion: new Posicion(1, 0),
                controlador: nameof(CircuitosDocController),
                navegarA: nameof(CircuitosDocController.CrudLotesContables),
                restringirPor: "",
                alSeleccionarBlanquearControl: "")
            {
                LongitudMinimaParaBuscar = 1
            };

            var opciones = new Dictionary<string, string> {
                      { $"{ltrParametrosNeg.MostrarTodos}", "incluidos o no en un lote" }
                    , { $"{ltrParametrosNeg.ConRelacion}", "pagos en un lote" }
                    , { $"{ltrParametrosNeg.SinRelacion}", "pagos sin lote" } };

            modal.ControlesDeFiltrado.Add(new FiltroDeRelacion<PagoDto>(modal,
                lista: lista,
                opciones: opciones,
                propiedad: ltrDeUnLoteContable.VinculosAUnLote,
                filtrarPor: ltrDeUnLoteContable.VinculosAUnLote,
                ayuda: "Filtros de pagos por lote contable"
                ));
        }

        private static void FiltroPorCuentasBancarias(ModalDeFiltrado<PagoDto> modal)
        {
            var lista = new ListaDeElemento<PagoDto>(modal,
                etiqueta: "cuenta deudora",
                ayuda: "seleccione la cuenta deudora",
                seleccionarDe: nameof(CuentaDeMiSociedadDto),
                filtraPor: ltrDeUnPago.IdCuentaDePago,
                mostrarExpresion: nameof(CuentaDeMiSociedadDto.Cuenta),
                posicion: new Posicion(1, 0),
                controlador: nameof(CuentasDeMiSociedadController));

            modal.ControlesDeFiltrado.Add(lista);
        }

        private static void FiltroPorTarjetas(ModalDeFiltrado<PagoDto> modal)
        {
            var lista = new ListaDeElemento<PagoDto>(modal,
                etiqueta: "tarjeta",
                ayuda: "seleccione tarjeta del pago",
                seleccionarDe: nameof(TarjetaDeMiSociedadDto),
                filtraPor: nameof(PagoDto.IdTarjetaDePago),
                mostrarExpresion: nameof(TarjetaDeMiSociedadDto.Expresion),
                posicion: new Posicion(1, 0),
                controlador: nameof(TarjetasDeMiSociedadController));

            modal.ControlesDeFiltrado.Add(lista);
        }

        private static void FiltroPorAcreedor(ModalDeFiltrado<PagoDto> modal)
        {
            var lista = new ListasDinamicas<PagoDto>(modal,
                etiqueta: enumNegocio.Interlocutor.Singular(),
                filtrarPor: ltrDeUnPago.IdAcreedor,
                ayuda: "seleccione el acreedor",
                seleccionarDe: nameof(InterlocutorDto),
                buscarPor: nameof(ltrDeUnPago.IdAcreedor),
                mostrarExpresion: nameof(InterlocutorDto.Expresion),
                criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                posicion: new Posicion(1, 0),
                controlador: nameof(InterlocutoresController),
                navegarA: nameof(InterlocutoresController.CrudInterlocutores),
                restringirPor: "",
                alSeleccionarBlanquearControl: "")
            { LongitudMinimaParaBuscar = 1 };

            var opciones = new Dictionary<string, string> {
                      { $"{ltrDeUnPago.CualquierAcreedor}", "Cualquier acreedor" }
                    , { $"{ltrDeUnPago.SoloProveedores}", "Solo proveedores" }
                    , { $"{ltrDeUnPago.SoloTrabajadores}", "Solo trabajadores" } };

            modal.ControlesDeFiltrado.Add(new FiltroDeRelacion<PagoDto>(modal,
                lista: lista,
                opciones: opciones,
                propiedad: ltrDeUnPago.ClaseDeAcreedor,
                filtrarPor: ltrDeUnPago.ClaseDeAcreedor,
                ayuda: "Filtro por acreedor"
                ));
        }

        private static void FiltroPorFactura(ModalDeFiltrado<PagoDto> modal)
        {
            var lista = new ListasDinamicas<PagoDto>(modal,
                etiqueta: enumNegocio.FacturaRecibida.Singular(),
                filtrarPor: ltrDeUnPago.IdFacturaRec,
                ayuda: $"seleccione la {enumNegocio.FacturaRecibida.Singular()}",
                seleccionarDe: nameof(FacturaRecDto),
                buscarPor: nameof(ltrDeUnPago.IdFacturaRec),
                mostrarExpresion: nameof(FacturaRecDtm.Expresion),
                criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                posicion: new Posicion(1, 0),
                controlador: nameof(FacturasRecController),
                navegarA: nameof(FacturasRecController.CrudFacturasRec),
                restringirPor: "",
                alSeleccionarBlanquearControl: "")
            { LongitudMinimaParaBuscar = 1 };

            var opciones = new Dictionary<string, string> {
                      { $"{ltrDeUnPago.CualquierAcreedor}", "Cualquier pago" }
                    , { $"{ltrDeUnPago.SoloConFacturas}", "Solo con facturas" }
                    , { $"{ltrDeUnPago.SoloSinFacturas}", "Solo sin facturas" } };

            modal.ControlesDeFiltrado.Add(new FiltroDeRelacion<PagoDto>(modal,
                lista: lista,
                opciones: opciones,
                propiedad: ltrDeUnPago.FiltroPorFactura,
                filtrarPor: ltrDeUnPago.FiltroPorFactura,
                ayuda: "Filtro por facturas"
                ));
        }
        private void FiltrosPorFechaDePagarEl(ModalDeFiltrado<PagoDto> modal)
        {
            var fechas = new FiltroEntreFechas<PagoDto>(modal,
                    etiqueta: "Pagar El",
                    propiedad: ltrDeUnPago.FiltroPorPagarEl,
                    ayuda: "pagos a realizar entre fechas");

            //var entreFechas = new FiltroEntreFechasConCheck<PagoDto>(modal, fechas, nameof(PagoDto.PagarEl), "Mostrar", columnas: new List<string> { nameof(PagoDto.PagarEl) });
            modal.ControlesDeFiltrado.Add(fechas);
        }

        private void FiltrosPorFechaDePagadoEl(ModalDeFiltrado<PagoDto> modal)
        {
            var fechas = new FiltroEntreFechas<PagoDto>(modal,
                    etiqueta: "Pagado El",
                    propiedad: ltrDeUnPago.FiltroPorPagadoEl,
                    ayuda: "pagos realizados entre fechas");

            //var entreFechas = new FiltroEntreFechasConCheck<PagoDto>(modal, fechas, nameof(PagoDto.PagadoEl), "Mostrar", columnas: new List<string> { nameof(PagoDto.PagadoEl) });
            modal.ControlesDeFiltrado.Add(fechas);
        }

        private void FiltrosPorImporteDelPago(ModalDeFiltrado<PagoDto> modal)
        {
            var importes = new FiltroEntreImportes<PagoDto>(modal,
                    etiqueta: "Importe a pagar",
                    propiedad: ltrDeUnPago.FiltroPorImporte,
                    ayuda: "filtrar por importe pagado o a pagar");
            //var entreImportes = new FiltroEntreImportesConCheck<PagoDto>(modal, importes, nameof(PagoDto.Importe), "Ocultar", columnas: new List<string> { nameof(PagoDto.Importe)});
            modal.ControlesDeFiltrado.Add(importes);
        }

        private static void FiltroPorRemesa(ModalDeFiltrado<PagoDto> modal)
        {
            var lista = new ListasDinamicas<PagoDto>(modal,
                etiqueta: enumNegocio.RemesaPag.Singular(),
                filtrarPor: ltrDeUnPago.IdRemesaPag,
                ayuda: "seleccione la remesa de pagos",
                seleccionarDe: nameof(RemesaPagDto),
                buscarPor: nameof(RemesaPagDto.Expresion),
                mostrarExpresion: nameof(RemesaPagDto.Expresion),
                criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                posicion: new Posicion(1, 0),
                controlador: nameof(RemesasPagController),
                navegarA: nameof(RemesasPagController.CrudRemesasPag),
                restringirPor: "",
                alSeleccionarBlanquearControl: "")
            { LongitudMinimaParaBuscar = 1 };

            var opciones = new Dictionary<string, string> {
                      { $"{ltrParametrosNeg.MostrarTodos}", "con o sin remesar" }
                    , { $"{ltrParametrosNeg.ConRelacion}", "pagos remesados" }
                    , { $"{ltrParametrosNeg.SinRelacion}", "pagos sin remesar" } };

            modal.ControlesDeFiltrado.Add(new FiltroDeRelacion<PagoDto>(modal,
                lista: lista,
                opciones: opciones,
                propiedad: ltrDeUnPago.IncluidaEnRemesa,
                filtrarPor: ltrDeUnPago.IncluidaEnRemesa,
                ayuda: "Filtros de remesas"
                ));
        }


        private void DescriptorDeRemesas()
        {
            var expansor = new DescriptorDeExpansor(Editor, $"{Editor.Id}-remesas", "Remesas", true, "Remesas que incluyen la factura");
            expansor.EsDetalle = true;
            Editor.Expanes.Insert(0, expansor);

            var columnas = new DescriptorDeColumnas("remesa");
            columnas.Add(titulo: nameof(PagoDeUnaRemesaDto.Elemento));
            columnas.Add(titulo: "Pagar El", propiedad: nameof(PagoDeUnaRemesaDto.PagarEl), formato: enumFormato.Fecha, tamano: 150);
            columnas.Add(titulo: "Pagado El", propiedad: nameof(PagoDeUnaRemesaDto.PagadoEl), formato: enumFormato.Fecha, tamano: 150);
            columnas.Add(titulo: "Anulado El", propiedad: nameof(PagoDeUnaRemesaDto.AnuladoEl), formato: enumFormato.Fecha, tamano: 150);
            columnas.Add(titulo: nameof(PagoDeUnaRemesaDto.IdElemento), mostrar: false);
            columnas.Add(titulo: nameof(PagoDeUnaRemesaDto.IdPago), mostrar: false);
            columnas.Add(titulo: nameof(PagoDeUnaRemesaDto.Id), mostrar: false);


            var parametros = new Dictionary<string, object> {
                   { nameof(GridDeRelacion.Controlador), typeof(PagosDeUnaRemesaController) }
                 , { nameof(GridDeRelacion.AccionDeConsulta), nameof(PagosDeUnaRemesaController.epLeerElementos)}
                 , { nameof(GridDeRelacion.PaginaDondeNavegarAlEditar) , $"{nameof(RemesasPagController)}/{nameof(RemesasPagController.CrudRemesasPag)}?id={nameof(PagoDeUnaRemesaDtm.IdElemento)}"}
                 , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(PagoDeUnaRemesaDto.IdPago) }
                 , { nameof(GridDeRelacion.OcultarSiVacio), true}
                };
            var gridDeRelacion = new GridDeRelacion(expansor, columnas, parametros);
            gridDeRelacion.PermitirBorrar = false;

            var modalDeEdicion = expansor.DescriptorDeEditarRelaciones(Editor.Crud.Contexto, typeof(PagoDeUnaRemesaDto), typeof(PagosDeUnaRemesaController), "Consultar datos", soloConsulta: true);
        }

        public override string RenderControl()
        {
            if (!_renderCache.IsNullOrEmpty())
                return _renderCache;

            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
            var render = base.RenderControl();
            render = render +
                   $@"<script src='../../js/{RutaBase}/Pagos.js?v={System.DateTime.Now.Ticks}'></script>
                      <script src='../../js/{RutaBase}/ApiDeGastos.js?v={System.DateTime.Now.Ticks}'></script>
                      <script>
                         try {{      
                           {RutaBase}.CrearCrudDePagos('{Mnt.IdHtml}', '{Creador.IdHtml}', '{Editor.IdHtml}', '{Borrado.IdHtml}') 
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
