using GestorDeElementos;
using GestorDeElementos.Extensores;
using ModeloDeDto;
using ModeloDeDto.Juridico;
using ModeloDeDto.Presupuesto;
using ModeloDeDto.SistemaDocumental;
using ModeloDeDto.Tarea;
using ModeloDeDto.Terceros;
using ModeloDeDto.Ventas;
using MVCSistemaDeElementos.Controllers;
using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Gastos;
using ServicioDeDatos.Juridico;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.Presupuesto;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Tarea;
using ServicioDeDatos.Ventas;
using System;
using System.Collections.Generic;
using Utilidades;
using UtilidadesParaIu;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeFacturasEmt : DescriptorDeCrud<FacturaEmtDto>
    {
        public DescriptorDeFacturasEmt(ContextoSe contexto, string renderCache) : base(contexto,renderCache)
        {
        }
        public DescriptorDeFacturasEmt(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto
               , nameof(FacturasEmtController)
               , nameof(FacturasEmtController.CrudFacturasEmt)
               , modo
               , rutaBase: enumNameSpaceTs.Venta)
        {
            Mnt.Filtro.FiltroDeNombre.CambiarAtributos("Varios", nameof(ltrDeUnaFacturaEmt.BuscarPorVarios), "concepto (c:) o nombre de cliente (d:) o por número (n:año-serie-nº)");
            IncluirFiltros();
            var irpf = DescriptorDeIrpfEmt();                
            DescriptorDePeriodoEmt();
            DescriptorDeVerifactu();
            DescriptorDeRectificativas();
            var lineas = DescriptorDeLineasDeUnaFae();
            Editor.AmpliacionesDetrasDe[irpf.Id] = lineas.Id;
            //if (enumNegocio.FacturaEmitida.Parametro(enumParametrosDeFacturasEmt.FAE_SII_Activo, crearParametro: true, valorPorDefecto: "N").Valor.EsTrue())
            //{
            //    var verifactu = DescriptorDeVerifactu();
            //    Editor.AmpliacionesDetrasDe[verifactu.Id] = irpf.Id;
            //}
            DescriptorDeSpanDeTareas();
            DescriptorDeCobrosDeFae();
            DescriptorDeAbonosDeFae();
            DescriptorDeRemesas();

            modalesParaPedirDatos.Add(new ModalParaPedirDatos(this, typeof(CambiarVencimientoDto), eventosDeMf.Fae_CambiarVencimiento, "Cambiar la fecha de vencimiento"));
            modalesParaPedirDatos.Add(new ModalParaPedirDatos(this, typeof(CopiarFaeDto), eventosDeMf.Fae_CopiarFae, "Seleccionar factura o plantilla"));
            modalesParaPedirDatos.Add(new ModalParaPedirDatos(this, typeof(HacerRectificativaDto), eventosDeMf.Fae_Rectificativa, "Seleccionar factura"));
            modalesParaPedirDatos.Add(new ModalParaPedirDatos(this, typeof(FacturarTareasDto), eventosDeMf.Fae_FacturarTareas, "Facturar tareas"));
            modalesParaPedirDatos.Add(new ModalParaPedirDatos(this, typeof(CambiarDatosFae), eventosDeMf.Fae_CambiarDatos, "Cambiar Datos"));

            Mnt.IncluirMfDeRelacion($"<li id='{menuDeRelaciones}.{enumNegocio.ParteDeTrabajo}' accion-menu='{eventosDeMf.Fae_IrAPartesTr}' {AtributosHtml.Mf(enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Consultor, false)}>{enumNegocio.ParteDeTrabajo.Plural()}</li>");
            Mnt.IncluirMfDeRelacion($"<li id='{menuDeRelaciones}.{enumNegocio.Presupuesto}' accion-menu='{eventosDeMf.Fae_IrAPpts}' {AtributosHtml.Mf(enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Consultor, false)}>{enumNegocio.Presupuesto.Plural()}</li>");
            Mnt.IncluirMfDeRelacion($"<li id='{menuDeRelaciones}.{enumNegocio.Contrato}' accion-menu='{eventosDeMf.Fae_IrAContrato}' {AtributosHtml.Mf(enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Consultor, false)}>{enumNegocio.Contrato.Plural()}</li>");

            Editor.IncluirMfIndividual("Hacer rectificativa", eventosDeMf.Fae_Rectificativa, enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Interventor);
            Editor.IncluirMfIndividual("Cambiar vencimiento", eventosDeMf.Fae_CambiarVencimiento, enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Interventor);
            Editor.IncluirMfIndividual("Facturar tareas", eventosDeMf.Fae_FacturarTareas, enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Gestor);
            Editor.IncluirMfIndividual("Copiar factura", eventosDeMf.Fae_CopiarLa, enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Gestor);
            Editor.IncluirMfIndividual("Cambiar Datos", eventosDeMf.Fae_CambiarDatos, enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Interventor);

            Mnt.IncluirMfContextual($"<li id='{menuContextual}.{eventosDeMf.Fae_CopiarFae}' accion-menu='{eventosDeMf.Fae_CopiarFae}' {AtributosHtml.Mf(enumCssOpcionMenu.DeVista, enumModoDeAccesoDeDatos.Gestor, false)}>Copiar Factura</li>");

            modalesParaPedirDatos.Add(new ModalDeTotales(this, typeof(TotalesDeFacturasEmt), eventosDeMf.Totalizador_Mostrar, $"Mostrar los totales de la selección"));
            Mnt.IncluirMfContextual($"<li id='{menuContextual}.{eventosDeMf.Totalizador_Mostrar}' accion-menu='{eventosDeMf.Totalizador_Mostrar}' {AtributosHtml.Mf(enumCssOpcionMenu.DeVista, enumModoDeAccesoDeDatos.Consultor, false)}>Mostrar totales</li>");

            var idsDeUsuarios = enumNegocio.Preasiento.Parametro(enumParametrosDePreasiento.SPR_Usuarios_Con_Permiso_Para_Generar, valorPorDefecto: Literal.Cero).Valor.ToLista<int>();
            if (idsDeUsuarios.Contains(contexto.DatosDeConexion.IdUsuario))
                Editor.IncluirMfIndividual("Generar preasiento", eventosDeMf.Fae_GenerarPreasiento, enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Administrador);

            if (Contexto.SePuedeParametrizar())
            {
                Editor.IncluirMfIndividual("Sincronizar con la AEAT", eventosDeMf.Fae_SincronizarConAeat, enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Administrador);
            }

            Mnt.OrdenacionInicial = $"{nameof(FacturaEmtDto.NumeroFactura)}:{nameof(FacturaEmtDto.NumeroFactura)}:{enumModoOrdenacion.descendente.Render()}";
            
            foreach (enumEtapasDeFacturasEmt etapa in Enum.GetValues(typeof(enumEtapasDeFacturasEmt)))
            {
                Mnt.Filtro.EtapasDeUnProceso.Opciones[etapa.ToString()] = "Etapa de: " + etapa.Nombre(minusculas: false);
            }
        }

        private void IncluirFiltros()
        {
            var modalDeDatos = new ModalDeFiltrado<FacturaEmtDto>(Mnt.Filtro, "filtrosDeLaFae", "Datos de las facturas");
            Mnt.Filtro.Modales.Add(modalDeDatos);
            FiltroPorCliente(modalDeDatos);
            FiltrosPorImporte(modalDeDatos);
            FiltrosPorFechaDeEmision(modalDeDatos);
            FiltrosPorFechaDeVencimiento(modalDeDatos);
            FiltrosPorNumerosDeFactura(modalDeDatos);
            FiltrosPorImporteDeCobro(modalDeDatos);

            var modalDeRelacion = new ModalDeFiltrado<FacturaEmtDto>(Mnt.Filtro, "filtrosDeRelacionesConFae", "Relaciones con las facturas");
            Mnt.Filtro.Modales.Add(modalDeRelacion);
            FiltroPorPresupuesto(modalDeRelacion);
            FiltroPorPlfVenta(modalDeRelacion);
            FiltroPorParteTr(modalDeRelacion);
            FiltroPorContrato(modalDeRelacion);
            FiltroPorRemesa(modalDeRelacion);
            FiltroPorEstimacionDirecta(modalDeRelacion);
            FiltroPorLoteContable(modalDeRelacion);
            FiltroPorRectificativa(modalDeRelacion);

        }

        private void FiltrosPorImporte(ModalDeFiltrado<FacturaEmtDto> modal)
        {
            var importes = new FiltroEntreImportes<FacturaEmtDto>(modal,
                    etiqueta: "Importe con IVA",
                    propiedad: ltrDeUnaFacturaEmt.FiltroPorImporteSinIva,
                    ayuda: "filtrar por rango de importes (Sin IVA)");
            modal.ControlesDeFiltrado.Add(importes);
        }

        private void FiltroPorCliente(ModalDeFiltrado<FacturaEmtDto> modal)
        {
            modal.ControlesDeFiltrado.Add(new ListasDinamicas<PresupuestoDto>(modal,
            etiqueta: "Cliente",
            filtrarPor: ltrDeUnaFacturaEmt.IdCliente,
            ayuda: "seleccione el cliente",
            seleccionarDe: nameof(ClienteDto),
            buscarPor: nameof(ClienteDto.Expresion),
            mostrarExpresion: nameof(ClienteDto.Expresion),
            criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
            posicion: new Posicion(1, 0),
            controlador: nameof(ClientesController),
            navegarA: nameof(ClientesController.CrudClientes),
            restringirPor: "",
            alSeleccionarBlanquearControl: "")
            { LongitudMinimaParaBuscar = 1 });
        }

        private void FiltrosPorFechaDeEmision(ModalDeFiltrado<FacturaEmtDto> modal)
        {
            var fechas = new FiltroEntreFechas<FacturaEmtDto>(modal,
                    etiqueta: "Emitida",
                    propiedad: ltrDeUnaFacturaEmt.FiltroPorFechaDeEmision,
                    ayuda: "facturas emitidas entre fechas");

            //var entreFechas = new FiltroEntreFechasConCheck<FacturaEmtDto>(modal, fechas, nameof(FacturaEmtDto.FacturadaEl), "Mostrar", columnas: new List<string> { nameof(FacturaEmtDto.FacturadaEl) });
            modal.ControlesDeFiltrado.Add(fechas);
        }

        private void FiltrosPorFechaDeVencimiento(ModalDeFiltrado<FacturaEmtDto> modal)
        {
            var fechas = new FiltroEntreFechas<FacturaEmtDto>(modal,
                    etiqueta: "Vence el",
                    propiedad: nameof(ltrDeUnaFacturaEmt.FiltroPorFechaDeVencimiento),
                    ayuda: "facturas que vencen entre fechas");

            //var entreFechas = new FiltroEntreFechasConCheck<FacturaEmtDto>(modal, fechas, nameof(FacturaEmtDto.VenceEl), "Mostrar", columnas: new List<string> { nameof(FacturaEmtDto.VenceEl) });
            modal.ControlesDeFiltrado.Add(fechas);
        }

        private void FiltrosPorNumerosDeFactura(ModalDeFiltrado<FacturaEmtDto> modal)
        {
            var rangos = new FiltroEntreRangos<FacturaEmtDto>(modal,
                    etiqueta: "Números de factura",
                    propiedad: nameof(ltrDeUnaFacturaEmt.FiltroPorNumerosDeFactura),
                    ayuda: "indicar números de facturas de misma serie");
            rangos.PlaceHolder = "yyyy-s-numero";
            rangos.ExpresioRegular = @"20\d{2}-\A-d[5]";
            modal.ControlesDeFiltrado.Add(rangos);
        }

        private void FiltrosPorImporteDeCobro(ModalDeFiltrado<FacturaEmtDto> modal)
        {
            var importes = new FiltroEntreImportes<FacturaEmtDto>(modal,
                    etiqueta: "Importe cobrado",
                    propiedad: ltrDeUnaFacturaEmt.FiltroPorCobrado,
                    ayuda: "filtrar por importe cobrado");
            //var entreImportes = new FiltroEntreImportesConCheck<FacturaEmtDto>(modal, importes, nameof(FacturaEmtDto.Cobrado), "Mostrar", columnas: new List<string> { nameof(FacturaEmtDto.Cobrado), nameof(FacturaEmtDto.Pendiente) });
            modal.ControlesDeFiltrado.Add(importes);
        }

        private static void FiltroPorPresupuesto(ModalDeFiltrado<FacturaEmtDto> modal)
        {
            var lista = new ListasDinamicas<FacturaEmtDto>(modal,
                etiqueta: enumNegocio.Presupuesto.Singular(),
                filtrarPor: ltrDeUnaFacturaEmt.IdPresupuesto,
                ayuda: "seleccione el presupuesto",
                seleccionarDe: nameof(PresupuestoDto),
                buscarPor: nameof(ltrDeUnPresupuesto.PptDeUnaFactura),
                mostrarExpresion: nameof(PresupuestoDto.Expresion),
                criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                posicion: new Posicion(1, 0),
                controlador: nameof(PresupuestosController),
                navegarA: nameof(PresupuestosController.CrudPresupuestos),
                restringirPor: "",
                alSeleccionarBlanquearControl: "")
            { LongitudMinimaParaBuscar = 1 };

            var opciones = new Dictionary<string, string> {
                      { $"{ltrParametrosNeg.MostrarTodos}", "con o sin ppt" }
                    , { $"{ltrParametrosNeg.ConRelacion}", "facturas de un ppt" }
                    , { $"{ltrParametrosNeg.SinRelacion}", "facturas que no son de un ppt" } };

            modal.ControlesDeFiltrado.Add(new FiltroDeRelacion<FacturaEmtDto>(modal,
                lista: lista,
                opciones: opciones,
                propiedad: ltrDeUnaFacturaEmt.AsociadaAUnPpt,
                filtrarPor: ltrDeUnaFacturaEmt.AsociadaAUnPpt,
                ayuda: "Filtros de presupuesto"
                ));
        }

        private void FiltroPorPlfVenta(ModalDeFiltrado<FacturaEmtDto> modal)
        {
            var lista = new ListasDinamicas<FacturaEmtDto>(modal,
                 etiqueta: enumNegocio.PlanificacionDeVenta.Singular(),
                 filtrarPor: ltrDeUnaFacturaEmt.IdPlfDeVenta,
                 ayuda: $"seleccione la {enumNegocio.PlanificacionDeVenta.Singular(true)}",
                 seleccionarDe: nameof(PlanificacionDeVentaDto),
                 buscarPor: nameof(PlanificacionDeVentaDto.Expresion),
                 mostrarExpresion: nameof(PlanificacionDeVentaDto.Expresion),
                 criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                 posicion: new Posicion(1, 0),
                 controlador: nameof(PlanificacionesDeVentaController),
                 navegarA: nameof(PlanificacionesDeVentaController.CrudPlanificacionesDeVenta),
                 restringirPor: "",
                 alSeleccionarBlanquearControl: "")
            {
                LongitudMinimaParaBuscar = 1
            };

            var opciones = new Dictionary<string, string> {
                      { $"{ltrParametrosNeg.MostrarTodos}", "Con o sin planificaciones" }
                    , { $"{ltrParametrosNeg.ConRelacion}", "Con planificaciones" }
                    , { $"{ltrParametrosNeg.SinRelacion}", "Sin planificaciones" } };

            modal.ControlesDeFiltrado.Add(new FiltroDeRelacion<FacturaEmtDto>(modal,
                lista: lista,
                opciones: opciones,
                propiedad: ltrDeUnaFacturaEmt.AsociadaAUnaPlv,
                filtrarPor: ltrDeUnaFacturaEmt.AsociadaAUnaPlv,
                ayuda: $"Filtros de {enumNegocio.PlanificacionDeVenta.Plural(true)}"
                ));
        }

        private static void FiltroPorParteTr(ModalDeFiltrado<FacturaEmtDto> modal)
        {
            var lista = new ListasDinamicas<FacturaEmtDto>(modal,
                etiqueta: enumNegocio.ParteDeTrabajo.Singular(),
                filtrarPor: ltrDeUnaFacturaEmt.IdParteTr,
                ayuda: "seleccione el parte de trabajo",
                seleccionarDe: nameof(ParteTrDto),
                buscarPor: nameof(ltrDeUnParteTr.PtrDeUnaFactura),
                mostrarExpresion: nameof(ParteTrDto.Expresion),
                criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                posicion: new Posicion(1, 0),
                controlador: nameof(PartesTrController),
                navegarA: nameof(PartesTrController.CrudPartesDeTrabajo),
                restringirPor: "",
                alSeleccionarBlanquearControl: "")
            { LongitudMinimaParaBuscar = 1 };

            var opciones = new Dictionary<string, string> {
                      { $"{ltrParametrosNeg.MostrarTodos}", "con o sin parte" }
                    , { $"{ltrParametrosNeg.ConRelacion}", "facturas de un parte" }
                    , { $"{ltrParametrosNeg.SinRelacion}", "facturas que no son de un parte" } };

            modal.ControlesDeFiltrado.Add(new FiltroDeRelacion<FacturaEmtDto>(modal,
                lista: lista,
                opciones: opciones,
                propiedad: ltrDeUnaFacturaEmt.AsociadaAUnPtr,
                filtrarPor: ltrDeUnaFacturaEmt.AsociadaAUnPtr,
                ayuda: "Filtros de partes de trabajo"
                ));
        }

        private static void FiltroPorContrato(ModalDeFiltrado<FacturaEmtDto> modal)
        {
            var lista = new ListasDinamicas<FacturaEmtDto>(modal,
                etiqueta: enumNegocio.Contrato.Singular(),
                filtrarPor: ltrDeUnaFacturaEmt.IdContrato,
                ayuda: "seleccione el contrato de venta",
                seleccionarDe: nameof(ContratoDto),
                buscarPor: nameof(ltrDeUnContrato.SelectorParaFiltratFacturasEmt),
                mostrarExpresion: nameof(ContratoDto.Expresion),
                criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                posicion: new Posicion(1, 0),
                controlador: nameof(ContratosController),
                navegarA: nameof(ContratosController.CrudContratos),
                restringirPor: "",
                alSeleccionarBlanquearControl: "")
            {
                LongitudMinimaParaBuscar = 1,
                ParametrosParaNavegar = nameof(ltrParametrosEp.Clase) + "=" + nameof(enumClaseDeContrato.Venta)
            };

            var opciones = new Dictionary<string, string> {
                      { $"{ltrParametrosNeg.MostrarTodos}", "con o sin contrato" }
                    , { $"{ltrParametrosNeg.ConRelacion}", "facturas de un contrato" }
                    , { $"{ltrParametrosNeg.SinRelacion}", "facturas que no son de contrato" } };

            modal.ControlesDeFiltrado.Add(new FiltroDeRelacion<FacturaEmtDto>(modal,
                lista: lista,
                opciones: opciones,
                propiedad: ltrDeUnaFacturaEmt.AsociadaAUnContrato,
                filtrarPor: ltrDeUnaFacturaEmt.AsociadaAUnContrato,
                ayuda: "Filtros de contratos"
                ));
        }

        private static void FiltroPorRemesa(ModalDeFiltrado<FacturaEmtDto> modal)
        {
            var lista = new ListasDinamicas<FacturaEmtDto>(modal,
                etiqueta: enumNegocio.RemesaFae.Singular(),
                filtrarPor: ltrDeUnaFacturaEmt.IdRemesaFae,
                ayuda: "seleccione la remesa de facturas",
                seleccionarDe: nameof(RemesaFaeDto),
                buscarPor: nameof(RemesaFaeDto.Expresion),
                mostrarExpresion: nameof(RemesaFaeDto.Expresion),
                criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                posicion: new Posicion(1, 0),
                controlador: nameof(RemesasFaeController),
                navegarA: nameof(RemesasFaeController.CrudRemesasFae),
                restringirPor: "",
                alSeleccionarBlanquearControl: "")
            { LongitudMinimaParaBuscar = 1 };

            var opciones = new Dictionary<string, string> {
                      { $"{ltrParametrosNeg.MostrarTodos}", "con o sin remesar" }
                    , { $"{ltrParametrosNeg.ConRelacion}", "facturas remesadas" }
                    , { $"{ltrParametrosNeg.SinRelacion}", "facturas sin remesar" } };

            modal.ControlesDeFiltrado.Add(new FiltroDeRelacion<FacturaEmtDto>(modal,
                lista: lista,
                opciones: opciones,
                propiedad: ltrDeUnaFacturaEmt.IncluidaEnRemesa,
                filtrarPor: ltrDeUnaFacturaEmt.IncluidaEnRemesa,
                ayuda: "Filtros de remesas"
                ));
        }

        private static void FiltroPorEstimacionDirecta(ModalDeFiltrado<FacturaEmtDto> modal)
        {
            var lista = new ListasDinamicas<FacturaEmtDto>(modal,
                etiqueta: ltrDeUnaEstimacion.EtiquetaEstimacionDirecta,
                filtrarPor: ltrDeUnaFacturaEmt.IdEstimacionDirecta,
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

            modal.ControlesDeFiltrado.Add(new FiltroDeRelacion<FacturaEmtDto>(modal,
                lista: lista,
                opciones: opciones,
                propiedad: ltrDeUnaEstimacion.VinculosAUnaEstimacion,
                filtrarPor: ltrDeUnaEstimacion.VinculosAUnaEstimacion,
                ayuda: "Filtros de facturas por estimación directa"
                ));
        }

        private static void FiltroPorLoteContable(ModalDeFiltrado<FacturaEmtDto> modal)
        {
            var lista = new ListasDinamicas<FacturaEmtDto>(modal,
                etiqueta: ltrDeUnLoteContable.EtiquetaLoteContable,
                filtrarPor: ltrDeUnaFacturaEmt.IdLoteContable,
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

            modal.ControlesDeFiltrado.Add(new FiltroDeRelacion<FacturaEmtDto>(modal,
                lista: lista,
                opciones: opciones,
                propiedad: ltrDeUnLoteContable.VinculosAUnLote,
                filtrarPor: ltrDeUnLoteContable.VinculosAUnLote,
                ayuda: "Filtros de facturas por lote contable"
                ));
        }

        private static void FiltroPorRectificativa(ModalDeFiltrado<FacturaEmtDto> modal)
        {
            var lista = new ListasDinamicas<FacturaEmtDto>(modal,
                etiqueta: ltrDeUnaFacturaEmt.EtiquetaDeFacturaRectificativa,
                filtrarPor: ltrDeUnaFacturaEmt.IdRectificativa,
                ayuda: "seleccione factura rectificativa",
                seleccionarDe: nameof(FacturaEmtDtm),
                buscarPor: nameof(ltrDeUnaFacturaEmt.FiltrarPorRectificativa),
                mostrarExpresion: nameof(FacturaEmtDto.Expresion),
                criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                posicion: new Posicion(1, 0),
                controlador: nameof(FacturasEmtController),
                navegarA: nameof(FacturasEmtController.CrudFacturasEmt),
                restringirPor: "",
                alSeleccionarBlanquearControl: "")
            {
                LongitudMinimaParaBuscar = 1
            };

            var opciones = new Dictionary<string, string> {
                      { $"{ltrParametrosNeg.MostrarTodos}", "rectificadas o no" }
                    , { $"{ltrParametrosNeg.ConRelacion}", "facturas rectificadas" }
                    , { $"{ltrParametrosNeg.SinRelacion}", "facturas no rectificadas" } };

            modal.ControlesDeFiltrado.Add(new FiltroDeRelacion<FacturaEmtDto>(modal,
                lista: lista,
                opciones: opciones,
                propiedad: ltrDeUnaFacturaEmt.Rectificadas,
                filtrarPor: ltrDeUnaFacturaEmt.Rectificadas,
                ayuda: "Filtro para rectificación de facturas"
                ));
        }

        private AmpliacionDeEdicion DescriptorDeIrpfEmt()
        {
            var irpf = new AmpliacionDeEdicion(Editor, Ampliaciones.FacturasEmt.irpfEmt, "Irpf", new Dimension(2, 2), ayuda: "Información sobre el irpf de factura");
            irpf.Dto = typeof(IrpfEmtDto);
            irpf.Controlador = nameof(IrpfsEmtController);
            Editor.Ampliaciones.Add(irpf);
            return irpf;
        }

        private AmpliacionDeEdicion DescriptorDeVerifactu()
        {
            var verifactu = new AmpliacionDeEdicion(Editor, Ampliaciones.FacturasEmt.verifactu, "Verifactu", new Dimension(2, 2), ayuda: "Auditoría de Verifactu");
            verifactu.Dto = typeof(VerifactuDto);
            verifactu.Controlador = nameof(VerifactuController);
            Editor.Ampliaciones.Add(verifactu);
            return verifactu;
        }

        private void DescriptorDePeriodoEmt()
        {
            var periodo = new AmpliacionDeEdicion(Editor, Ampliaciones.FacturasEmt.periodoEmt, "Datos del perido", new Dimension(2, 2), ayuda: "Información sobre el periodo de facturación");
            periodo.Dto = typeof(PeriodoEmtDto);
            periodo.Controlador = nameof(PeriodosEmtController);
            Editor.Ampliaciones.Add(periodo);
        }

        private void DescriptorDeRectificativas()
        {
            var expansor = new DescriptorDeExpansor(Editor, $"{Editor.Id}-rectificadas", "Rectificadas", true, "facturas rectificadas");
            expansor.EsDetalle = true;
            Editor.Expanes.Insert(0, expansor);

            var columnas = new DescriptorDeColumnas("rectificadas");
            columnas.Add(titulo: nameof(RectificativaEmtDto.Rectificada), tamano: 200);
            columnas.Add(titulo: nameof(RectificativaEmtDto.Concepto));
            columnas.Add(titulo: "Facturada el", nameof(RectificativaEmtDto.FacturadaEl), formato: enumFormato.Fecha, tamano: 150);
            columnas.Add(titulo: "Facturado", propiedad: nameof(RectificativaEmtDto.TotalAPagar), alineacion: enumAliniacion.derecha, tamano: 150, formato: enumFormato.Moneda);
            columnas.Add(titulo: "Cobrado", propiedad: nameof(RectificativaEmtDto.Cobrado), alineacion: enumAliniacion.derecha, tamano: 150, formato: enumFormato.Moneda);
            columnas.Add(titulo: "Pendiente", propiedad: nameof(RectificativaEmtDto.Pendiente), alineacion: enumAliniacion.derecha, tamano: 150, formato: enumFormato.Moneda);
            columnas.Add(titulo: nameof(RectificativaEmtDto.IdRectificada), mostrar: false);
            columnas.Add(titulo: nameof(RectificativaEmtDto.IdElemento), mostrar: false);
            columnas.Add(titulo: nameof(RectificativaEmtDto.Id), mostrar: false);


            var parametros = new Dictionary<string, object> {
                   { nameof(GridDeRelacion.Controlador), typeof(RectificativasEmtController) }
                 , { nameof(GridDeRelacion.AccionDeConsulta), nameof(RectificativasEmtController.epLeerElementos)}
                 , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(RectificativaEmtDto.IdElemento) }
                 , { nameof(GridDeRelacion.PaginaDondeNavegarAlEditar) , $"{nameof(FacturasEmtController)}/{nameof(FacturasEmtController.CrudFacturasEmt)}?id={nameof(RectificativaEmtDto.IdRectificada)}"}
                 , { nameof(GridDeRelacion.OcultarSiVacio), false}
                };
            var gridDeRelacion = new GridDeRelacion(expansor, columnas, parametros);
            gridDeRelacion.PermitirBorrar = false;

            var modalDeEdicion = expansor.DescriptorDeEditarRelaciones(Editor.Crud.Contexto, typeof(RectificativaEmtDto), typeof(RectificativasEmtController), "Consultar datos", soloConsulta: true);

            var modalParaVincular = expansor.DescriptorParaVincular(Editor.Crud.Contexto, NegociosDeSe.IdNegocio(enumNegocio.FacturaEmitida), typeof(SelectorFaeParaRectificarDto), nameof(FacturasEmtController), "Añadir factura a rectificar");
            modalParaVincular.AccionTrasAbrirModal = $"javascript: {enumNameSpaceTs.Venta}.{enumFunctionTs.Fae_InicializarModalParaRectificar}();"; ;
        }

        private DescriptorDeExpansor DescriptorDeLineasDeUnaFae()
        {
            var expansor = new DescriptorDeExpansor(Editor, $"{Editor.Id}-lineas-de-una-fae", "Detalle", true, "Lineas de la factura");
            expansor.EsDetalle = true;
            Editor.Expanes.Insert(1, expansor);

            var columnas = new DescriptorDeColumnas("lineasDeUnaFae");
            columnas.Add(titulo: nameof(LineaDeUnaFaeDto.Orden), alineacion: enumAliniacion.derecha, tamano: 100);
            columnas.Add(titulo: nameof(LineaDeUnaFaeDto.Concepto));
            columnas.Add(titulo: nameof(LineaDeUnaFaeDto.Cantidad), alineacion: enumAliniacion.derecha, tamano: 150, formato: enumFormato.Numero_2);
            columnas.Add(titulo: nameof(LineaDeUnaFaeDto.Unidad));
            columnas.Add(titulo: "P. de venta", propiedad: nameof(LineaDeUnaFaeDto.Precio), alineacion: enumAliniacion.derecha, tamano: 150, formato: enumFormato.Moneda);
            columnas.Add(titulo: "Descuento", propiedad: nameof(LineaDeUnaFaeDto.Descuento), alineacion: enumAliniacion.derecha, tamano: 150, formato: enumFormato.Porcentaje);
            columnas.Add(titulo: "B.I.", propiedad: nameof(LineaDeUnaFaeDto.BaseImponible), formato: enumFormato.Moneda, tamano: 150);
            columnas.Add(titulo: "Iva", propiedad: nameof(LineaDeUnaFaeDto.Iva), alineacion: enumAliniacion.derecha, tamano: 150, formato: enumFormato.Porcentaje);
            columnas.Add(titulo: "Importe", propiedad: nameof(LineaDeUnaFaeDto.ImporteDeLinea), alineacion: enumAliniacion.derecha, tamano: 150, formato: enumFormato.Moneda);
            columnas.Add(titulo: nameof(LineaDeUnaFaeDto.IdElemento), mostrar: false);
            columnas.Add(titulo: nameof(LineaDeUnaFaeDto.Id), mostrar: false);

            var orden = $"{nameof(LineaDeUnaFaeDto.Orden)}:{enumModoOrdenacion.ascendente.Render()}";


            var parametros = new Dictionary<string, object> {
                   { nameof(GridDeRelacion.Controlador), typeof(LineasDeUnaFaeController) }
                 , { nameof(GridDeRelacion.AccionDeConsulta), nameof(LineasDeUnaFaeController.epLeerElementos)}
                 , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(LineaDeUnaFaeDto.IdElemento) }
                 , { nameof(GridDeRelacion.OrdenarPor), orden }
                 , { nameof(GridDeRelacion.OcultarSiVacio), false}
                };
            var gridDeRelacion = new GridDeRelacion(expansor, columnas, parametros);
            gridDeRelacion.PermitirBorrar = true;

            var modalDeCreacion = expansor.DescriptorDeCrearRelaciones(Editor.Crud.Contexto, typeof(LineaDeUnaFaeDto), typeof(LineasDeUnaFaeController), nameof(LineaDeUnaFaeDto.IdElemento), "Añadir línea");
            modalDeCreacion.AccionTrasAbrirModal = $"javascript: {enumNameSpaceTs.Venta}.{enumFunctionTs.Fae_InicializarModalParaCrearLineas}()";

            var modalDeEdicion = expansor.DescriptorDeEditarRelaciones(Editor.Crud.Contexto, typeof(LineaDeUnaFaeDto), typeof(LineasDeUnaFaeController), "Editar línea", soloConsulta: false);
            modalDeEdicion.AccionTrasAbrirModal = $"javascript: {enumNameSpaceTs.Venta}.{enumFunctionTs.Fae_InicializarModalParaEditarLineas}();";

            expansor.DescriptorParaVincular(Editor.Crud.Contexto, NegociosDeSe.IdNegocio(enumNegocio.ParteDeTrabajo), typeof(SelectorDeParteTrDto), nameof(PartesTrController), "Añadir Parte de trabajo");
            return expansor;
        }

        private void DescriptorDeCobrosDeFae()
        {
            var expansor = new DescriptorDeExpansor(Editor, $"{Editor.Id}-CobrosDeFae", "Cobros", true, "Cobros de una factura");
            expansor.EsDetalle = true;
            Editor.Expanes.Insert(2, expansor);

            var columnas = new DescriptorDeColumnas("CobrosDeFae");
            columnas.Add(titulo: nameof(CobroDeFaeDto.Clase));
            columnas.Add(titulo: "Cobrado el", propiedad: nameof(CobroDeFaeDto.CobradoEl), alineacion: enumAliniacion.derecha, formato: enumFormato.Fecha, tamano: 150);
            columnas.Add(titulo: "Cuenta de ingreso", propiedad: nameof(CobroDeFaeDto.CuentaDeIngreso), alineacion: enumAliniacion.derecha);
            columnas.Add(titulo: "Cuenta de cargo", propiedad: nameof(CobroDeFaeDto.CuentaDeCargo), alineacion: enumAliniacion.derecha);
            columnas.Add(titulo: "Importe del cobro", propiedad: nameof(CobroDeFaeDto.Cobrado), alineacion: enumAliniacion.derecha, tamano: 150, formato: enumFormato.Moneda);
            columnas.Add(titulo: nameof(CobroDeFaeDto.IdElemento), mostrar: false);
            columnas.Add(titulo: nameof(CobroDeFaeDto.Id), mostrar: false);

            var orden = $"{nameof(CobroDeFaeDto.CobradoEl)}:{enumModoOrdenacion.descendente.Render()}";


            var parametros = new Dictionary<string, object> {
                   { nameof(GridDeRelacion.Controlador), typeof(CobrosDeFaeController) }
                 , { nameof(GridDeRelacion.AccionDeConsulta), nameof(CobrosDeFaeController.epLeerElementos)}
                 , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(CobroDeFaeDto.IdElemento) }
                 , { nameof(GridDeRelacion.OrdenarPor), orden }
                 , { nameof(GridDeRelacion.OcultarSiVacio), false}
                };
            var gridDeRelacion = new GridDeRelacion(expansor, columnas, parametros);
            gridDeRelacion.PermitirBorrar = true;

            var modalDeCreacion = expansor.DescriptorDeCrearRelaciones(Editor.Crud.Contexto, typeof(CobroDeFaeDto), typeof(CobrosDeFaeController), nameof(CobroDeFaeDto.IdElemento), "Añadir cobro");
            modalDeCreacion.AccionTrasAbrirModal = $"javascript: {enumNameSpaceTs.Venta}.{enumFunctionTs.Fae_InicializarModalParaCrearCobros}()";

            var modalDeEdicion = expansor.DescriptorDeEditarRelaciones(Editor.Crud.Contexto, typeof(CobroDeFaeDto), typeof(CobrosDeFaeController), "Consultar cobro", soloConsulta: true);
            //modalDeEdicion.AccionTrasAbrirModal = $"javascript: {enumNameSpaceTs.Venta}.{enumFunctionTs.Fae_InicializarModalParaEditarCobros}();";
        }

        private void DescriptorDeAbonosDeFae()
        {
            var expansor = new DescriptorDeExpansor(Editor, $"{Editor.Id}-AbonosDeFae", "Abonos", true, "Abonos de una factura");
            expansor.EsDetalle = true;
            Editor.Expanes.Insert(2, expansor);

            var columnas = new DescriptorDeColumnas("AbonosDeFae");
            columnas.Add(titulo: nameof(AbonoDeFaeDto.Referencia), tamano: 150);
            columnas.Add(titulo: nameof(AbonoDeFaeDto.Clase));
            columnas.Add(titulo: "Abonado el", propiedad: nameof(AbonoDeFaeDto.AbonadoEl), alineacion: enumAliniacion.derecha, formato: enumFormato.Fecha, tamano: 150);
            columnas.Add(titulo: "Cuenta de abono", propiedad: nameof(AbonoDeFaeDto.CuentaDeAbono), alineacion: enumAliniacion.derecha);
            columnas.Add(titulo: "Cuenta de cargo", propiedad: nameof(AbonoDeFaeDto.CuentaDeCargo), alineacion: enumAliniacion.derecha);
            columnas.Add(nameof(AbonoDeFaeDto.Importe), alineacion: enumAliniacion.derecha, tamano: 150, formato: enumFormato.Moneda);
            columnas.Add(nameof(AbonoDeFaeDto.Estado), alineacion: enumAliniacion.derecha, tamano: 200);
            columnas.Add(titulo: nameof(AbonoDeFaeDto.IdAbono), mostrar: false);
            columnas.Add(titulo: nameof(AbonoDeFaeDto.IdElemento), mostrar: false);
            columnas.Add(titulo: nameof(AbonoDeFaeDto.Id), mostrar: false);

            var orden = $"{nameof(AbonoDeFaeDto.AbonadoEl)}:{nameof(PagoDtm).Replace("Dtm", "")}.{nameof(PagoDtm.PagadoEl)}:{enumModoOrdenacion.descendente.Render()}";

            var parametros = new Dictionary<string, object> {
                   { nameof(GridDeRelacion.Controlador), typeof(AbonosDeFaeController) }
                 , { nameof(GridDeRelacion.AccionDeConsulta), nameof(AbonosDeFaeController.epLeerElementos)}
                 , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(AbonoDeFaeDto.IdElemento) }
                 , { nameof(GridDeRelacion.PaginaDondeNavegarAlEditar) , $"{nameof(PagosController)}/{nameof(PagosController.CrudPagos)}?id={nameof(AbonoDeFaeDto.IdAbono)}"}
                 , { nameof(GridDeRelacion.OrdenarPor), orden }
                 , { nameof(GridDeRelacion.OcultarSiVacio), false}
                };
            var gridDeRelacion = new GridDeRelacion(expansor, columnas, parametros);
            gridDeRelacion.PermitirBorrar = true;

            var modalDeCreacion = expansor.DescriptorDeCrearRelaciones(Editor.Crud.Contexto, typeof(AbonoDeFaeDto), typeof(AbonosDeFaeController), nameof(AbonoDeFaeDto.IdElemento), "Añadir abono");
            modalDeCreacion.AccionTrasAbrirModal = $"javascript: {enumNameSpaceTs.Venta}.{enumFunctionTs.Fae_InicializarModalParaCrearAbonos}()";

            var modalDeEdicion = expansor.DescriptorDeEditarRelaciones(Editor.Crud.Contexto, typeof(AbonoDeFaeDto), typeof(AbonosDeFaeController), "Consultar abono", soloConsulta: true);
            //modalDeEdicion.AccionTrasAbrirModal = $"javascript: {enumNameSpaceTs.Venta}.{enumFunctionTs.Fae_InicializarModalParaEditarCobros}();";
        }

        private void DescriptorDeRemesas()
        {
            var expansor = new DescriptorDeExpansor(Editor, $"{Editor.Id}-remesas", "Remesas", true, "Remesas que incluyen la factura");
            expansor.EsDetalle = true;
            Editor.Expanes.Insert(3, expansor);

            var columnas = new DescriptorDeColumnas("remesa");
            columnas.Add(titulo: nameof(FacturaEmtDeUnaRemesaDto.Elemento));
            columnas.Add(titulo: nameof(FacturaEmtDeUnaRemesaDto.CargadaEl), formato: enumFormato.Fecha, tamano: 150);
            columnas.Add(titulo: nameof(FacturaEmtDeUnaRemesaDto.DevueltoEl), formato: enumFormato.Fecha, tamano: 150);
            columnas.Add(titulo: nameof(FacturaEmtDeUnaRemesaDto.IdElemento), mostrar: false);
            columnas.Add(titulo: nameof(FacturaEmtDeUnaRemesaDto.IdFactura), mostrar: false);
            columnas.Add(titulo: nameof(FacturaEmtDeUnaRemesaDto.Id), mostrar: false);


            var parametros = new Dictionary<string, object> {
                   { nameof(GridDeRelacion.Controlador), typeof(FacturasEmtDeUnaRemesaController) }
                 , { nameof(GridDeRelacion.AccionDeConsulta), nameof(FacturasEmtDeUnaRemesaController.epLeerElementos)}
                 , { nameof(GridDeRelacion.PaginaDondeNavegarAlEditar) , $"{nameof(RemesasFaeController)}/{nameof(RemesasFaeController.CrudRemesasFae)}?id={nameof(FacturaEmtDeUnaRemesaDtm.IdElemento)}"}
                 , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(FacturaEmtDeUnaRemesaDto.IdFactura) }
                 , { nameof(GridDeRelacion.OcultarSiVacio), true}
                };
            var gridDeRelacion = new GridDeRelacion(expansor, columnas, parametros);
            gridDeRelacion.PermitirBorrar = false;

            var modalDeEdicion = expansor.DescriptorDeEditarRelaciones(Editor.Crud.Contexto, typeof(FacturaEmtDeUnaRemesaDto), typeof(FacturasEmtDeUnaRemesaController), "Consultar datos", soloConsulta: true);
        }


        private void DescriptorDeSpanDeTareas()
        {
            var expansor = new DescriptorDeExpansor(Editor, $"{Editor.Id}-tareas-fae", "Tareas facturadas", true, "tareas  facturadas por la factura");
            expansor.EsDetalle = true;
            Editor.Expanes.Insert(2, expansor);
            var columnas = new DescriptorDeColumnas("tareas-fae");
            columnas.Add(titulo: "Tarea", propiedad: nameof(TareaDto.Expresion), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnas.Add(titulo: nameof(TareaDto.Tipo), propiedad: nameof(TareaDto.Tipo), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnas.Add(titulo: "Facturado", propiedad: nameof(TareaDto.Facturado), alineacion: enumAliniacion.derecha, mostrar: true, formato: enumFormato.Numero_2);
            columnas.Add(titulo: "Medido En", propiedad: nameof(TareaDto.Medido), alineacion: enumAliniacion.derecha, mostrar: true, tamano: 100);
            columnas.Add(titulo: nameof(TareaDto.Id), propiedad: nameof(TareaDto.Id), alineacion: enumAliniacion.derecha, mostrar: false);
            var parametros = new Dictionary<string, object> {
               { nameof(GridDeRelacion.Controlador), typeof(TareasController) },
               { nameof(GridDeRelacion.AccionDeConsulta), nameof(TareasController.epLeerElementos)},
               { nameof(GridDeRelacion.PropiedadRestrictora), nameof(TareaDto.IdFacturaEmt) },
               { nameof(GridDeRelacion.PaginaDondeNavegarAlEditar) , $"{nameof(TareasController)}/{nameof(TareasController.CrudTareas)}?id={nameof(TareaDto.Id)}"},
               { nameof(GridDeRelacion.OcultarSiVacio), true}, 
               { nameof(GridDeRelacion.AccionDeBorrado),  nameof(TareasController.epExcluirDeLaFactura) }
            };

            var gridDeRelacion = new GridDeRelacion(expansor, columnas, parametros);
            gridDeRelacion.PermitirBorrar = true;
            gridDeRelacion.PermitirEditar = false;
        }


        public override string RenderControl()
        {
            if (!_renderCache.IsNullOrEmpty())
                return _renderCache;

            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
            var render = base.RenderControl();

            render = render +
                   $@"<script src=¨../../js/{RutaBase}/ApiDeFacturasEmt.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script src=¨../../js/{RutaBase}/FacturasEmt.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{      
                           {RutaBase}.CrearCrudDeFacturasEmt('{Mnt.IdHtml}', '{Creador.IdHtml}', '{Editor.IdHtml}', '{Borrado.IdHtml}') 
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
