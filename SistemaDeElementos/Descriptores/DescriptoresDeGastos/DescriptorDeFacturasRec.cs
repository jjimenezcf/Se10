using GestorDeElementos;
using GestorDeElementos.Extensores;
using ModeloDeDto;
using ModeloDeDto.Expediente;
using ModeloDeDto.Gastos;
using ModeloDeDto.MaestrosTecnico;
using ModeloDeDto.SistemaDocumental;
using ModeloDeDto.Terceros;
using MVCSistemaDeElementos.Controllers;
using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.Gastos;
using ServicioDeDatos.Juridico;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Ventas;
using System;
using System.Collections.Generic;
using Utilidades;
using UtilidadesParaIu;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeFacturasRec : DescriptorDeCrud<FacturaRecDto>
    {
        public DescriptorDeFacturasRec(ContextoSe contexto, string renderCache) : base(contexto, renderCache)
        {
        }
        public DescriptorDeFacturasRec(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto
               , nameof(FacturasRecController)
               , nameof(FacturasRecController.CrudFacturasRec)
               , modo
               , rutaBase: enumNameSpaceTs.Gasto)
        {
            Mnt.Filtro.FiltroDeNombre.CambiarAtributos("Varios", nameof(ltrDeUnaFacturaRec.AsuntoNumeroReferencia), "Buscar por asunto (a:) o referencia (r:) o proveedor (p:) o factura (f:)");
            Mnt.IaAccion = UtilidadesIu.enumAccionVisorArchivo.AnalizarFactura;
            Mnt.IaTitulo = $"Analizar Factura con: '{ExtensorDeUsuarios.IaUsada(contexto).Nombre}'";
            IncluirFiltros();
            DescriptorDeLineasDeUnaFar();
            DefinirDescriptorDePagos();
            Mnt.IncluirMfDeRelacion($"<li id='{menuDeRelaciones}.{enumNegocio.Expediente}' accion-menu='{eventosDeMf.Far_IrAExpediente}' {AtributosHtml.Mf(enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Consultor, false)}>{enumNegocio.Expediente.Plural()}</li>");
            Mnt.IncluirMfDeRelacion($"<li id='{menuDeRelaciones}.{enumNegocio.Contrato}' accion-menu='{eventosDeMf.Far_IrAContrato}' {AtributosHtml.Mf(enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Consultor, false)}>{enumNegocio.Contrato.Plural()}</li>");
            Mnt.IncluirMfDeRelacion($"<li id='{menuDeRelaciones}.{enumNegocio.Pago}' accion-menu='{eventosDeMf.Far_IrAPago}' {AtributosHtml.Mf(enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Consultor, false)}>{enumNegocio.Pago.Plural()}</li>");

            modalesParaPedirDatos.Add(new ModalParaPedirDatos(this, typeof(CopiarFarDto), eventosDeMf.Far_CopiarFar, "Seleccionar factura o plantilla"));
            Mnt.IncluirMfContextual($"<li id='{menuContextual}.{eventosDeMf.Far_CopiarFar}' accion-menu='{eventosDeMf.Far_CopiarFar}' {AtributosHtml.Mf(enumCssOpcionMenu.DeVista, enumModoDeAccesoDeDatos.Gestor, false)}>Copiar factura</li>");

            modalesParaPedirDatos.Add(new ModalParaPedirDatos(this, typeof(CrearFarConIaDto), eventosDeMf.Far_CrearFarConIa, "Seleccionar fichero con facturas a crear"));
            Mnt.IncluirMfContextual($"<li id='{menuContextual}.{eventosDeMf.Far_CrearFarConIa}' accion-menu='{eventosDeMf.Far_CrearFarConIa}' {AtributosHtml.Mf(enumCssOpcionMenu.DeVista, enumModoDeAccesoDeDatos.Gestor, false)}>Subir y crear facturas</li>");

            modalesParaPedirDatos.Add(new ModalParaPedirDatos(this, typeof(ImportarPrvXml), eventosDeMf.Far_ImportarPrv, "Seleccionar efactura de la que importar el proveedor"));
            Mnt.IncluirMfContextual($"<li id='{menuContextual}.{eventosDeMf.Far_ImportarPrv}' accion-menu='{eventosDeMf.Far_ImportarPrv}' {AtributosHtml.Mf(enumCssOpcionMenu.DeVista, enumModoDeAccesoDeDatos.Gestor, false)}>Importar Proveedor</li>");

            modalesParaPedirDatos.Add(new ModalParaPedirDatos(this, typeof(ImportarFarXml), eventosDeMf.Far_ImportarXml, "Seleccionar efactura a importar"));
            Mnt.IncluirMfContextual($"<li id='{menuContextual}.{eventosDeMf.Far_ImportarXml}' accion-menu='{eventosDeMf.Far_ImportarXml}' {AtributosHtml.Mf(enumCssOpcionMenu.DeVista, enumModoDeAccesoDeDatos.Gestor, false)}>Importar eFactura</li>");

            modalesParaPedirDatos.Add(new ModalDeTotales(this, typeof(TotalesDeFacturasRec), eventosDeMf.Totalizador_Mostrar, $"Mostrar los totales de la selección"));
            Mnt.IncluirMfContextual($"<li id='{menuContextual}.{eventosDeMf.Totalizador_Mostrar}' accion-menu='{eventosDeMf.Totalizador_Mostrar}' {AtributosHtml.Mf(enumCssOpcionMenu.DeVista, enumModoDeAccesoDeDatos.Consultor, false)}>Mostrar totales</li>");

            DefinirMf(menuIndividual, Mnt.OpcionesPorElemento);

            modalesParaPedirDatos.Add(new ModalParaPedirDatos(this, typeof(RenombrarFarDto), eventosDeMf.Far_Renombrar, "Cambiar Datos"));
            modalesParaPedirDatos.Add(new ModalParaPedirDatos(this, typeof(CambiarProveedorDto), eventosDeMf.Far_CambiarProveedor, "Cambiar proveedor"));

            Editor.IncluirMfIndividual("Cambiar Datos", eventosDeMf.Far_Renombrar, enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Interventor);
            Editor.IncluirMfIndividual("Cambiar Proveedor", eventosDeMf.Far_CambiarProveedor, enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Interventor);
            Editor.IncluirMfIndividual("Quitar expediente", eventosDeMf.Far_QuitarExpediente, enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Interventor);
            Editor.IncluirMfIndividual("Quitar contrato", eventosDeMf.Far_QuitarContrato, enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Interventor);

            var idsDeUsuarios = enumNegocio.Preasiento.Parametro(enumParametrosDePreasiento.SPR_Usuarios_Con_Permiso_Para_Generar, valorPorDefecto: Literal.Cero).Valor.ToLista<int>();
            if (idsDeUsuarios.Contains(contexto.DatosDeConexion.IdUsuario))
            {
                Mnt.IncluirMfContextual($"<li id='{menuContextual}.{eventosDeMf.Far_CancelarPreasientos}' accion-menu='{eventosDeMf.Far_CancelarPreasientos}' {AtributosHtml.Mf(enumCssOpcionMenu.DeVista, enumModoDeAccesoDeDatos.Administrador, false)}>Cancelar preasientos</li>");
                Editor.IncluirMfIndividual("Generar preasiento", eventosDeMf.Far_GenerarPreasiento, enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Administrador);
            }

            foreach (enumEtapasDeFacturasRec etapa in Enum.GetValues(typeof(enumEtapasDeFacturasRec)))
            {
                Mnt.Filtro.EtapasDeUnProceso.Opciones[etapa.ToString()] = "Etapa de: " + etapa.Nombre(minusculas: false);
            }
        }

        public DescriptorDeFacturasRec(ContextoSe contexto)
        : base(contexto
               , nameof(FacturasRecController)
               , nameof(FacturasRecController.CrudFacturasRec)
               , ModoDescriptor.Imputar
               , rutaBase: enumNameSpaceTs.Gasto
               , id: $"Imputar_Facturas")
        {
        }

        private void DefinirMf(string idMenu, List<string> opciones)
        {
            modalesParaPedirDatos.Add(new ModalParaPedirDatos(this, typeof(RectificarFarDto), eventosDeMf.Far_RectificarFar, "Seleccionar factura a rectificar"));

            DescriptorDeEdicion<FacturaRecDto>.IncluirMfIndividual(opciones, $"<li id='{idMenu}.{eventosDeMf.Far_RectificarFar}'" +
                                               $"accion-menu='{eventosDeMf.Far_RectificarFar}' " +
                                               $"{AtributosHtml.Mf(enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Gestor, false)} >Rectificar factura</li>");
        }

        private void IncluirFiltros()
        {
            var modalDeDatos = new ModalDeFiltrado<FacturaRecDto>(Mnt.Filtro, "filtrosDeLaFar", "Datos de las facturas");
            Mnt.Filtro.Modales.Add(modalDeDatos);
            FiltroPorProveedor(modalDeDatos);
            FiltrosPorImporte(modalDeDatos);
            FiltrosPorFechaDeEmision(modalDeDatos);
            FiltrosPorFechaDeVencimiento(modalDeDatos);
            FiltrosPorImporteDePago(modalDeDatos);
            FiltrosPorFormaDePago(modalDeDatos);
            FiltrosPorImpuestos(modalDeDatos);
            FiltrosPorNaturalezas(modalDeDatos);
            FiltrosParaPreasientos(modalDeDatos);

            var modalDeRelacion = new ModalDeFiltrado<FacturaRecDto>(Mnt.Filtro, "filtrosDeRelacionesConFar", "Relaciones con las facturas");
            Mnt.Filtro.Modales.Add(modalDeRelacion);
            FiltroPorExpediente(modalDeRelacion);
            FiltroPorContrato(modalDeRelacion);
            FiltroPorRemesa(modalDeRelacion);
            FiltroPorEstimacionDirecta(modalDeRelacion);
            FiltroPorLoteContable(modalDeRelacion);
        }

        private static void FiltrosPorFormaDePago(ModalDeFiltrado<FacturaRecDto> modal)
        {
            var opciones = new Dictionary<string, string> {
                { $"{ltrDeUnPago.FiltroDePagosContado}", "Pago al contado" },
                { $"{ltrDeUnPago.FiltroDePagosTarjeta}", "Pago con tarjeta" },
                { $"{ltrDeUnPago.FiltroDePagosDomiciliado}", "Pago por domicialición" },
                { $"{ltrDeUnPago.FiltroDePagosTransferencia}", "Pago por transferencia" },
                { $"{ltrDeUnPago.FiltroDePagosRemesa}", "Pago remesado" },
            };
            var lv = new ListaDeValoresParaFiltrado<FacturaRecDto>(modal,
                nameof(PagoDtm) + "-" + nameof(PagoDtm.Clase),
                ltrDeUnPago.FiltroPorFormaDePago,
                opciones,
                "filtrar facturas con algún pago como el seleccionado");
            modal.ControlesDeFiltrado.Add(lv);
        }

        private static void FiltrosPorImpuestos(ModalDeFiltrado<FacturaRecDto> modal)
        {
            var opciones = new Dictionary<string, string> {
                { $"{ltrDeUnPago.FiltroConIva}",       "Facturas solo con Iva" },
                { $"{ltrDeUnPago.FiltroConIrpf}",      "Facturas con IRPF" },
                { $"{ltrDeUnPago.FiltroConIvaExento}", "Facturas con iva exento" },
                { $"{ltrDeUnPago.FiltroConIvaIsp}",    "Facturas de sujeto pasivo" },
                { $"{ltrDeUnPago.FiltroConIvaNsj}",    "Facturas con Iva 'No sujeto'" },
                { $"{ltrDeUnPago.FiltroSinIvaNiIrpf}", "Facturas sin Iva ni retención" },
            };
            var lv = new ListaDeValoresParaFiltrado<FacturaRecDto>(modal,
                nameof(IvaSoportadoDtm) + "-" + nameof(IvaSoportadoDtm.Clase),
                ltrDeUnPago.FiltroDeIvaIrpf,
                opciones,
                "filtrar por iva y retenciones");
            modal.ControlesDeFiltrado.Add(lv);
        }


        private static void FiltrosPorNaturalezas(ModalDeFiltrado<FacturaRecDto> modal)
        {
            var lv = new ListaDeElemento<FacturaRecDto>(modal,
                "Naturaleza",
                "Seleccione la naturaleza",
                nameof(NaturalezaDto),
                ltrDeUnaFacturaRec.FiltroPorNaturaleza,
                nameof(NaturalezaDto.Expresion), 
                null,
                nameof(NaturalezasController));
            modal.ControlesDeFiltrado.Add(lv);
        }

        private static void FiltrosParaPreasientos(ModalDeFiltrado<FacturaRecDto> modal)
        {
            var opciones = new Dictionary<string, string> {
                { $"{ltrDeUnaFacturaRec.FiltroConSpr}", "Facturas con preasiento" },
                { $"{ltrDeUnaFacturaRec.FiltroSinSpr}", "Facturas sin preasiento" },
                { $"{ltrDeUnaFacturaRec.FiltroConSprCan}", "Con preasiento cancelado" }
            };
            var lv = new ListaDeValoresParaFiltrado<FacturaRecDto>(modal,
                nameof(FacturaRecDto) + "-" + nameof(FacturaRecDto.Preasiento),
                ltrDeUnaFacturaRec.FiltroSiHayPreasiento,
                opciones,
                "filtrar si hay preasientos");
            modal.ControlesDeFiltrado.Add(lv);
        }

        private void FiltroPorProveedor(ModalDeFiltrado<FacturaRecDto> modal)
        {
            var ld = new ListasDinamicas<FacturaRecDto>(modal,
                 etiqueta: enumNegocio.Proveedor.Singular(),
                 filtrarPor: ltrDeUnaFacturaRec.FiltroPorProveedor,
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


        private void FiltrosPorImporte(ModalDeFiltrado<FacturaRecDto> modal)
        {
            var bis = new FiltroEntreImportes<FacturaRecDto>(modal,
                    etiqueta: "Base Imponible",
                    propiedad: ltrDeUnaFacturaRec.FiltroPorImporteSinIva,
                    ayuda: "filtrar por rango de importes (Sin IVA)");
            modal.ControlesDeFiltrado.Add(bis);

            var pagar = new FiltroEntreImportes<FacturaRecDto>(modal,
                    etiqueta: "Total factura",
                    propiedad: ltrDeUnaFacturaRec.FiltroPorTotalFactura,
                    ayuda: "filtrar por rango de importes (total a pagar)");
            modal.ControlesDeFiltrado.Add(pagar);
        }
        private void FiltrosPorFechaDeEmision(ModalDeFiltrado<FacturaRecDto> modal)
        {
            var fechas = new FiltroEntreFechas<FacturaRecDto>(modal,
                    etiqueta: "Emitida",
                    propiedad: ltrDeUnaFacturaRec.FiltroPorFechaDeEmision,
                    ayuda: "facturas emitidas entre fechas");
            modal.ControlesDeFiltrado.Add(fechas);
        }

        private void FiltrosPorFechaDeVencimiento(ModalDeFiltrado<FacturaRecDto> modal)
        {
            var fechas = new FiltroEntreFechas<FacturaRecDto>(modal,
                    etiqueta: "Vence el",
                    propiedad: nameof(ltrDeUnaFacturaRec.FiltroPorFechaDeVencimiento),
                    ayuda: "facturas que vencen entre fechas");
            modal.ControlesDeFiltrado.Add(fechas);
        }


        private void FiltrosPorImporteDePago(ModalDeFiltrado<FacturaRecDto> modal)
        {/*
            var importes = new FiltroEntreImportes<FacturaRecDto>(modal,
                    etiqueta: "Importe cobrado",
                    propiedad: ltrDeUnaFacturaRec.FiltroPorCobrado,
                    ayuda: "filtrar por importe cobrado");
            var entreImportes = new FiltroEntreImportesConCheck<FacturaRecDto>(modal, importes, nameof(FacturaRecDto.Cobrado), "Mostrar", columnas: new List<string> { nameof(FacturaRecDto.Cobrado), nameof(FacturaRecDto.Pendiente) });
            modal.ControlesDeFiltrado.Add(entreImportes);*/
        }

        private static void FiltroPorExpediente(ModalDeFiltrado<FacturaRecDto> modal)
        {
            var lista = new ListasDinamicas<FacturaRecDto>(modal,
                etiqueta: enumNegocio.Expediente.Singular(),
                filtrarPor: ltrDeUnaFacturaRec.IdExpediente,
                ayuda: "seleccione el expediente",
                seleccionarDe: nameof(ExpedienteDto),
                buscarPor: nameof(ltrDeUnExpediente.SelectorParaFiltrarFacturasRec),
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
                    , { $"{ltrParametrosNeg.ConRelacion}", "facturas con Expediente" }
                    , { $"{ltrParametrosNeg.SinRelacion}", "facturas sin Expediente" } };

            modal.ControlesDeFiltrado.Add(new FiltroDeRelacion<FacturaRecDto>(modal,
                lista: lista,
                opciones: opciones,
                propiedad: ltrDeUnaFacturaRec.AsociadaAUnExpediente,
                filtrarPor: ltrDeUnaFacturaRec.AsociadaAUnExpediente,
                ayuda: "Filtros de expedientes"
                ));
        }

        private static void FiltroPorEstimacionDirecta(ModalDeFiltrado<FacturaRecDto> modal)
        {
            var lista = new ListasDinamicas<FacturaRecDto>(modal,
                etiqueta: ltrDeUnaEstimacion.EtiquetaEstimacionDirecta,
                filtrarPor: ltrDeUnaFacturaRec.IdEstimacionDirecta,
                ayuda: "seleccione estimación directa",
                seleccionarDe: nameof(CircuitoDocDtm),
                buscarPor: nameof(ltrDeUnCircuito.SeleccionarParaFiltrarPorEstimacion),
                mostrarExpresion: nameof(CircuitoDocDto.Expresion),
                criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                posicion: new Posicion(1, 0),
                controlador: nameof(CircuitosDocController),
                navegarA: nameof(CircuitosDocController.CrudEstimacionesDirectas),
                restringirPor: "",
                alSeleccionarBlanquearControl: "")
            {
                LongitudMinimaParaBuscar = 1
            };

            var opciones = new Dictionary<string, string> {
                      { $"{ltrParametrosNeg.MostrarTodos}", "incluidas o no en una estimación" }
                    , { $"{ltrParametrosNeg.ConRelacion}", "facturas en una estimación" }
                    , { $"{ltrParametrosNeg.SinRelacion}", "facturas pendientes" } };

            modal.ControlesDeFiltrado.Add(new FiltroDeRelacion<FacturaRecDto>(modal,
                lista: lista,
                opciones: opciones,
                propiedad: ltrDeUnaEstimacion.VinculosAUnaEstimacion,
                filtrarPor: ltrDeUnaEstimacion.VinculosAUnaEstimacion,
                ayuda: "Filtros de facturas por estimación directa"
                ));
        }

        private static void FiltroPorLoteContable(ModalDeFiltrado<FacturaRecDto> modal)
        {
            var lista = new ListasDinamicas<FacturaRecDto>(modal,
                etiqueta: ltrDeUnLoteContable.EtiquetaLoteContable,
                filtrarPor: ltrDeUnaFacturaRec.IdLoteContable,
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
                      { $"{ltrParametrosNeg.MostrarTodos}", "incluidas o no en un lote" }
                    , { $"{ltrParametrosNeg.ConRelacion}", "facturas en un lote" }
                    , { $"{ltrParametrosNeg.SinRelacion}", "facturas pendientes" } };

            modal.ControlesDeFiltrado.Add(new FiltroDeRelacion<FacturaRecDto>(modal,
                lista: lista,
                opciones: opciones,
                propiedad: ltrDeUnLoteContable.VinculosAUnLote,
                filtrarPor: ltrDeUnLoteContable.VinculosAUnLote,
                ayuda: "Filtros de facturas por lote contable"
                ));
        }

        private static void FiltroPorContrato(ModalDeFiltrado<FacturaRecDto> modal)
        {
            var lista = new ListasDinamicas<FacturaRecDto>(modal,
                etiqueta: enumNegocio.Contrato.Singular(),
                filtrarPor: ltrDeUnaFacturaRec.IdContrato,
                ayuda: "seleccione el contrato de compra",
                seleccionarDe: nameof(FacturaRecDto),
                buscarPor: nameof(ltrDeUnContrato.SelectorParaFiltrarFacturasRec),
                mostrarExpresion: nameof(FacturaRecDto.Expresion),
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
                    , { $"{ltrParametrosNeg.ConRelacion}", "facturas con contrato" }
                    , { $"{ltrParametrosNeg.SinRelacion}", "facturas sin contrato" } };

            modal.ControlesDeFiltrado.Add(new FiltroDeRelacion<FacturaRecDto>(modal,
                lista: lista,
                opciones: opciones,
                propiedad: ltrDeUnaFacturaRec.AsociadaAUnContrato,
                filtrarPor: ltrDeUnaFacturaRec.AsociadaAUnContrato,
                ayuda: "Filtros de contratos"
                ));
        }

        private static void FiltroPorRemesa(ModalDeFiltrado<FacturaRecDto> modal)
        {
            var lista = new ListasDinamicas<FacturaRecDto>(modal,
                etiqueta: enumNegocio.RemesaPag.Singular(),
                filtrarPor: ltrDeUnaFacturaRec.IdRemesaPag,
                ayuda: "seleccione la remesa de facturas",
                seleccionarDe: nameof(RemesaPagDto),
                buscarPor: nameof(RemesaPagDto.Expresion),
                mostrarExpresion: nameof(RemesaPagDto.Expresion),
                criterioDeBusqueda: enumCriteriosDeFiltrado.porReferencia,
                posicion: new Posicion(1, 0),
                controlador: nameof(RemesasPagController),
                navegarA: nameof(RemesasPagController.CrudRemesasPag),
                restringirPor: "",
                alSeleccionarBlanquearControl: "")
            { LongitudMinimaParaBuscar = 1 };

            var opciones = new Dictionary<string, string> {
                      { $"{ltrParametrosNeg.MostrarTodos}", "con o sin remesar" }
                    , { $"{ltrParametrosNeg.ConRelacion}", "facturas remesadas" }
                    , { $"{ltrParametrosNeg.SinRelacion}", "facturas sin remesar" } };

            modal.ControlesDeFiltrado.Add(new FiltroDeRelacion<FacturaRecDto>(modal,
                lista: lista,
                opciones: opciones,
                propiedad: ltrDeUnaFacturaRec.FiltroPorRemesaPag,
                filtrarPor: ltrDeUnaFacturaRec.FiltroPorRemesaPag,
                ayuda: "Filtros de remesas"
                ));
        }

        private void DescriptorDeLineasDeUnaFar()
        {
            var expansor = new DescriptorDeExpansor(Editor, $"{Editor.Id}-lineasDeUnaFar", "Detalle", true, "Lineas de la factura");
            expansor.EsDetalle = true;
            Editor.Expanes.Insert(0, expansor);

            var columnas = new DescriptorDeColumnas("lineasDeUnaFar");
            columnas.Add(titulo: nameof(LineaDeUnaFarDto.Orden), alineacion: enumAliniacion.derecha, tamano: 100);
            columnas.Add(titulo: nameof(LineaDeUnaFarDto.Clase), propiedad: nameof(LineaDeUnaFarDto.DescripcionDeClase), tamano: 150);
            columnas.Add(titulo: nameof(LineaDeUnaFarDto.Concepto));
            columnas.Add(titulo: "Base imponible", nameof(LineaDeUnaFarDto.BaseImponible), alineacion: enumAliniacion.derecha, tamano: 150, formato: enumFormato.Moneda);
            columnas.Add(titulo: "Iva", propiedad: nameof(LineaDeUnaFarDto.PorcentajeIva), alineacion: enumAliniacion.derecha, tamano: 150, formato: enumFormato.Porcentaje);
            columnas.Add(titulo: "Irpf", propiedad: nameof(LineaDeUnaFarDto.PorcentajeIrpf), alineacion: enumAliniacion.derecha, tamano: 150, formato: enumFormato.Porcentaje);
            columnas.Add(titulo: "Importe", propiedad: nameof(LineaDeUnaFarDto.importeLinea), alineacion: enumAliniacion.derecha, tamano: 150, formato: enumFormato.Moneda);
            columnas.Add(titulo: "Naturaleza", propiedad: nameof(LineaDeUnaFarDto.Sigla), alineacion: enumAliniacion.derecha, tamano: 100);
            columnas.Add(titulo: nameof(LineaDeUnaFarDto.IdElemento), mostrar: false);
            columnas.Add(titulo: nameof(LineaDeUnaFarDto.Id), mostrar: false);

            var orden = $"{nameof(LineaDeUnaFarDto.Orden)}:{enumModoOrdenacion.ascendente.Render()}";


            var parametros = new Dictionary<string, object> {
                   { nameof(GridDeRelacion.Controlador), typeof(LineasDeUnaFarController) }
                 , { nameof(GridDeRelacion.AccionDeConsulta), nameof(LineasDeUnaFarController.epLeerElementos)}
                 , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(LineaDeUnaFarDto.IdElemento) }
                 , { nameof(GridDeRelacion.OrdenarPor), orden }
                 , { nameof(GridDeRelacion.OcultarSiVacio), false}
                };
            var gridDeRelacion = new GridDeRelacion(expansor, columnas, parametros);
            gridDeRelacion.PermitirBorrar = true;

            var modalDeCreacion = expansor.DescriptorDeCrearRelaciones(Editor.Crud.Contexto, typeof(LineaDeUnaFarDto), typeof(LineasDeUnaFarController), nameof(LineaDeUnaFarDto.IdElemento), "Añadir línea");
            modalDeCreacion.AccionTrasAbrirModal = $"javascript: {enumNameSpaceTs.Gasto}.{enumFunctionTs.Far_InicializarModalParaCrearLineas}()";

            var modalDeEdicion = expansor.DescriptorDeEditarRelaciones(Editor.Crud.Contexto, typeof(LineaDeUnaFarDto), typeof(LineasDeUnaFarController), "Editar línea", soloConsulta: false);
            modalDeEdicion.AccionTrasAbrirModal = $"javascript: {enumNameSpaceTs.Gasto}.{enumFunctionTs.Far_InicializarModalParaEditarLineas}()";
        }

        private void DefinirDescriptorDePagos()
        {
            var expansor = new DescriptorDeExpansor(Editor, $"{Editor.Id}-pagos", "Pagos", true, "Pagos de la factura");
            Editor.Expanes.Insert(1, expansor);

            //Definimos el grid de detalles del cuerpo
            var columnas = new DescriptorDeColumnas("pagos");
            columnas.Add(titulo: "Tipo", propiedad: nameof(PagoDto.Tipo), alineacion: enumAliniacion.izquierda, tamano: 300, mostrar: true);
            columnas.Add(titulo: "Clase", propiedad: nameof(PagoDto.Clase), tamano: 150, alineacion: enumAliniacion.izquierda, mostrar: true);
            columnas.Add(titulo: "Modo", propiedad: nameof(PagoDto.ModoDePago), tamano: 150, alineacion: enumAliniacion.izquierda, mostrar: true);
            columnas.Add(titulo: "Pago", propiedad: nameof(PagoDto.Expresion), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnas.Add(titulo: "Importe", propiedad: nameof(PagoDto.Importe), formato: enumFormato.Moneda);
            columnas.Add(titulo: "Estado", propiedad: nameof(PagoDto.Estado), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnas.Add(titulo: "Id", propiedad: nameof(PagoDto.Id), alineacion: enumAliniacion.derecha, mostrar: false);
            var parametros = new Dictionary<string, object> {
                   { nameof(GridDeRelacion.Controlador), typeof(PagosController) }
                 , { nameof(GridDeRelacion.AccionDeConsulta), nameof(BaseController<FacturaRecDto>.epLeerVinculosCon)}
                 , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(VinculoDtm.idElemento1) }
                 , { nameof(ltrParametrosEp.idVinculado) , NegociosDeSe.IdNegocio(enumNegocio.Pago) }
                 , { nameof(GridDeRelacion.PaginaDondeNavegarAlEditar) , $"{nameof(PagosController)}/{nameof(PagosController.CrudPagos)}"}
                 , { nameof(GridDeRelacion.OcultarSiVacio), false}
                };
            var gridDeRelacion = new GridDeRelacion(expansor, columnas, parametros);
            gridDeRelacion.PermitirBorrar = false;
            gridDeRelacion.PermitirEditar = false;

            //expansor.DescriptorDeEditarRelaciones(Editor.Crud.Contexto, typeof(PagoDto), typeof(PagosController), "Editar pago", true);

            var accion = new AccionDeGetionarDatosDependientes(
                    urlDelCrud: $@"/{nameof(PagosController)}/{nameof(PagosController.CrudPagos)}?origen=dependencia"
                  , datosDependientes: nameof(PagoDto)
                  , nombreDelMnt: DescriptorDeMantenimiento<PagoDto>.NombreMnt
                  , propiedadQueRestringe: nameof(FacturaRecDtm.Id)
                  , propiedadRestrictora: nameof(PagoDto.IdFacturaRec)
                  , "Crear pago de la factura");
            expansor.DescriptorDeNavegadorRefParaCrear("Crear pago", accion, enumParaQueNavegar.crear);
        }


        public override string RenderControl()
        {
            if (!_renderCache.IsNullOrEmpty())
                return _renderCache;

            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
            var render = base.RenderControl();

            render = render +
                   $@"<script src='../../js/{RutaBase}/ApiDeGastos.js?v={System.DateTime.Now.Ticks}'></script>
                      <script src=¨../../js/{RutaBase}/FacturasRec.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{      
                           {RutaBase}.CrearCrudDeFacturasRec('{Mnt.IdHtml}', '{Creador.IdHtml}', '{Editor.IdHtml}', '{Borrado.IdHtml}') 
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
