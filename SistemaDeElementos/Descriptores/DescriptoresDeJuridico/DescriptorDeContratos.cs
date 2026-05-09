using GestorDeElementos;
using GestoresDeNegocio.Juridico;
using ModeloDeDto;
using ModeloDeDto.Entorno;
using ModeloDeDto.Expediente;
using ModeloDeDto.Gastos;
using ModeloDeDto.Juridico;
using ModeloDeDto.MaestrosTecnico;
using ModeloDeDto.RegistroEs;
using ModeloDeDto.Tarea;
using ModeloDeDto.Terceros;
using ModeloDeDto.Ventas;
using MVCSistemaDeElementos.Controllers;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.Gastos;
using ServicioDeDatos.Juridico;
using ServicioDeDatos.RegistroEs;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Ventas;
using System.Collections.Generic;
using Utilidades;
using UtilidadesParaIu;
using static GestoresDeNegocio.Juridico.GestorDelPlanificadorDeVentas;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeContratos : DescriptorDeCrud<ContratoDto>
    {
        private enumClaseDeContrato _clase;

        public DescriptorDeContratos(ContextoSe contexto, ModoDescriptor modo, enumClaseDeContrato clase)
        : base(contexto
               , nameof(ContratosController)
               , nameof(ContratosController.CrudContratos)
               , modo
               , rutaBase: enumNameSpaceTs.Juridico)
        {
            _clase = clase;

            Mnt.Etiqueta = TituloDelMantenimeinto(_clase);
            Creador.Etiqueta = TituloDeCreacion(_clase);
            Editor.Etiqueta = TituloDeEdicion(_clase);
            IncluirFiltrosDeCabecera();

            DefinirMf(menuEdicion, Editor.OpcionesMf);

            if (_clase == enumClaseDeContrato.Venta)
            {
                MenuDeVentas();
                IncluirFiltroDeRelacionesVenta();
                DescriptorDeVentas();
                DescriptorDePlanificadorDeVentas();
                DescriptorDeSpanDeFacturasEmt();
            }

            if (_clase == enumClaseDeContrato.MatriculaDeGuarderia)
            {
                MenuDeMatriculas();
                DescriptorDeMatriculasDeGuarderia();
                DescriptorDeVentas();
                DescriptorDePlanificadorDeVentas();
                DescriptorDeSpanDeFacturasEmt();
            }

            if (_clase == enumClaseDeContrato.Compra)
            {
                MenuDeCompras(contexto);
                IncluirFiltroDeRelacionesCompra();
                DescriptorDeCompra();
                DescriptorDeSpanDeFacturasRec();
            }
            DescriptorDeImportes();
            DescriptorDeProrrogas();
            DescriptorDeAvances();
            DescriptorDeAvales();
            //DescriptorDeMinuta();
            DescriptorDeSpanDeRegistroEs();
            DescriptorDeLotes();
        }

        private void MenuDeCompras(ContextoSe contexto)
        {

            Mnt.IncluirMfDeRelacion($"<li id='{menuDeRelaciones}.{eventosDeMf.Ctr_ImputarFacturas}' accion-menu='{eventosDeMf.Ctr_ImputarFacturas}' {AtributosHtml.Mf(enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Interventor, false)}>Imputar facturas</li>");
            var modalDeFacturas2 = new ModalParaImputar<ContratoDto, FacturaRecDto>(mantenimiento: Mnt
                   , id: $"{this.Id}-{eventosDeMf.Ctr_ImputarFacturas}"
                   , tituloModal: "Seleccione las facturas a imputar"
                   , crudModal: new DescriptorDeFacturasRec(contexto)
                   , propiedadRestrictora: nameof(FacturaRecDto.IdContrato)
                   , filtrarPor: ltrDeUnaFacturaRec.FacturasPosiblesDelContrato
                   , faltaRestrictor: "Debe seleccionar el contrato al que imputar las facturas");
            modalesParaPedirDatos.Add(modalDeFacturas2);

            Mnt.IncluirMfDeRelacion($"<li id='{menuDeRelaciones}.{eventosDeMf.Ctr_IrAFacturasRec}' accion-menu='{eventosDeMf.Ctr_IrAFacturasRec}' {AtributosHtml.Mf(enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Consultor, false)}>Facturas recibidas</li>");
        }

        private void MenuDeVentas()
        {
            Mnt.IncluirMfDeRelacion($"<li id='{menuDeRelaciones}.{enumNegocio.PlanificacionDeVenta}' accion-menu='{eventosDeMf.Ctr_IrAPlvDeVenta}' {AtributosHtml.Mf(enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Consultor, false)}>Planificaciones</li>");
            Mnt.IncluirMfDeRelacion($"<li id='{menuDeRelaciones}.{enumNegocio.ParteDeTrabajo}' accion-menu='{eventosDeMf.Ctr_IrAPartesTr}' {AtributosHtml.Mf(enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Consultor, false)}>Partes de trabajo</li>");
            Mnt.IncluirMfDeRelacion($"<li id='{menuDeRelaciones}.{enumNegocio.FacturaEmitida}' accion-menu='{eventosDeMf.Ctr_IrAFacturasEmt}' {AtributosHtml.Mf(enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Consultor, false)}>Facturas emitidas</li>");

            Editor.IncluirMfIndividual("Generar planificaciones", eventosDeMf.GenerarPlanificadoresDeUnContrato, enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Gestor);

            //modalesParaPedirDatos.Add(new ModalParaPedirDatos(this, typeof(FechasDeGeneracionDto), eventosDeMf.GenerarLosPlanificadores, "Fechas entre las que se generarán las planificaciones"));
            modalesParaPedirDatos.Add(new ModalParaPedirDatos(this, typeof(FechasDePreparacionDto), eventosDeMf.PrepararPartesDeTrabajo, "Fechas entre las que crean los partes de trabajo"));
            modalesParaPedirDatos.Add(new ModalParaPedirDatos(this, typeof(FechasDePrefacturacionDto), eventosDeMf.EmitirPrefacturasPorParteTr, "Fechas entre las que se emiten las prefacturas"));
            modalesParaPedirDatos.Add(new ModalParaPedirDatos(this, typeof(FacturarPorContratoDto), eventosDeMf.EmitirPrefacturasPorContrato, "Prefacturar por contrato"));

            Mnt.IncluirMfContextual("<hr>");
            Mnt.IncluirMfContextual($"<li id='{menuContextual}.{eventosDeMf.EmitirPrefacturasPorParteTr}' accion-menu='{eventosDeMf.EmitirPrefacturasPorParteTr}' {AtributosHtml.Mf(enumCssOpcionMenu.DeVista, enumModoDeAccesoDeDatos.Gestor, false)}>Prefacturar partes de trabajo</li>");
            Mnt.IncluirMfContextual($"<li id='{menuContextual}.{eventosDeMf.EmitirPrefacturasPorContrato}' accion-menu='{eventosDeMf.EmitirPrefacturasPorContrato}' {AtributosHtml.Mf(enumCssOpcionMenu.DeVista, enumModoDeAccesoDeDatos.Gestor, false)}>Prefacturar contratos</li>");
            Mnt.IncluirMfContextual($"<li id='{menuContextual}.{eventosDeMf.PrepararPartesDeTrabajo}' accion-menu='{eventosDeMf.PrepararPartesDeTrabajo}' {AtributosHtml.Mf(enumCssOpcionMenu.DeVista, enumModoDeAccesoDeDatos.Gestor, false)}>Crear partes de trabajo</li>");
            //Mnt.IncluirMfContextual($"<li id='{menuContextual}.{eventosDeMf.GenerarLosPlanificadores}' accion-menu='{eventosDeMf.GenerarLosPlanificadores}' {AtributosHtml.Mf(enumCssOpcionMenu.DeVista, enumModoDeAccesoDeDatos.Gestor, false)}>Generar planificaciones</li>");
        }

        private void MenuDeMatriculas()
        {
            Mnt.IncluirMfDeRelacion($"<li id='{menuDeRelaciones}.{enumNegocio.PlanificacionDeVenta}' accion-menu='{eventosDeMf.Ctr_IrAPlvDeVenta}' {AtributosHtml.Mf(enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Consultor, false)}>Datos para facturar</li>");

            Editor.IncluirMfIndividual("Generar planificaciones", eventosDeMf.GenerarPlanificadoresDeUnContrato, enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Gestor);


            //modalesParaPedirDatos.Add(new ModalParaPedirDatos(this, typeof(FechasDeGeneracionDto), eventosDeMf.GenerarLosPlanificadores, "Fechas para preparar la facturación"));

            //Mnt.IncluirMfContextual("<hr>");
            //Mnt.IncluirMfContextual($"<li id='{menuContextual}.{eventosDeMf.GenerarLosPlanificadores}' accion-menu='{eventosDeMf.GenerarLosPlanificadores}' {AtributosHtml.Mf(enumCssOpcionMenu.DeVista, enumModoDeAccesoDeDatos.Gestor, false)}>Preparar facturación</li>");
        }

        private void DescriptorDeLotes()
        {
            var expansor = new DescriptorDeExpansor(Editor, $"{Editor.Id}-lotes", "Lotes de un contrato", mostrarPlegado: true, "lotes definidos en un contrato con unitarios");

            Editor.Expanes.Insert(0, expansor);

            //Definimos el grid de detalles del cuerpo
            var columnas = new DescriptorDeColumnas("Lotes");
            columnas.Add(titulo: "Lotes", propiedad: nameof(LoteDeUnContratoDto.Nombre), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnas.Add(titulo: "Inicio", propiedad: nameof(LoteDeUnContratoDto.VigenteDesde), alineacion: enumAliniacion.derecha, mostrar: true, formato: enumFormato.Fecha);
            columnas.Add(titulo: "Fin", propiedad: nameof(LoteDeUnContratoDto.VigenteHasta), alineacion: enumAliniacion.derecha, mostrar: true, formato: enumFormato.Fecha);
            columnas.Add(titulo: "Id", propiedad: nameof(LoteDeUnContratoDto.Id), alineacion: enumAliniacion.derecha, mostrar: false);
            var parametros = new Dictionary<string, object> {
               { nameof(GridDeRelacion.Controlador), typeof(LotesDeUnContratoController) } ,
               { nameof(GridDeRelacion.AccionDeConsulta), nameof(LotesDeUnContratoController.epLeerElementos)} ,
               { nameof(GridDeRelacion.PropiedadRestrictora), nameof(LoteDeUnContratoDto.IdContrato) },
               { nameof(GridDeRelacion.PaginaDondeNavegarAlEditar) , $"{nameof(LotesDeUnContratoController)}/{nameof(LotesDeUnContratoController.CrudLotes)}?id={nameof(LoteDeUnContratoDto.Id)}"},
               { nameof(GridDeRelacion.OcultarSiVacio), false}
            };
            var gridDeRelacion = new GridDeRelacion(expansor, columnas, parametros);
            //gridDeRelacion.PermitirBorrar = false;


            var accion = new AccionDeGetionarDatosDependientes(
                    urlDelCrud: $@"/{nameof(LotesDeUnContratoController)}/{nameof(LotesDeUnContratoController.CrudLotes)}?origen=dependencia"
                  , datosDependientes: nameof(LoteDeUnContratoDto)
                  , nombreDelMnt: DescriptorDeMantenimiento<LoteDeUnContratoDto>.NombreMnt
                  , propiedadQueRestringe: nameof(ContratoDto.Id)
                  , propiedadRestrictora: nameof(LoteDeUnContratoDto.IdContrato)
                  , "Crear o gestionar lotes");

            expansor.DescriptorDeNavegadorRefParaCrear("Crear lote", accion, enumParaQueNavegar.crear);
            expansor.DescriptorDeEditarRelaciones(Editor.Crud.Contexto, typeof(LoteDeUnContratoDto), typeof(LotesDeUnContratoController), "Editar lote", false);

        }

        private void DescriptorDePlanificadorDeVentas()
        {
            var expansor = new DescriptorDeExpansor(Editor, $"{Editor.Id}-planificadorDeVentas", "Planificadores de ventas", mostrarPlegado: true, "planificadores definidos");

            Editor.Expanes.Insert(0, expansor);

            //Definimos el grid de detalles del cuerpo
            var columnas = new DescriptorDeColumnas("PlanificadorDeVentas");
            columnas.Add(titulo: "Planificador", propiedad: nameof(PlanificadorDeVentaDto.Nombre), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnas.Add(titulo: "Inicio", propiedad: nameof(PlanificadorDeVentaDto.Inicio), alineacion: enumAliniacion.derecha, mostrar: true, formato: enumFormato.Fecha);
            columnas.Add(titulo: "Fin", propiedad: nameof(PlanificadorDeVentaDto.Hasta), alineacion: enumAliniacion.derecha, mostrar: true, formato: enumFormato.Fecha);
            columnas.Add(titulo: "Generado", propiedad: nameof(PlanificadorDeVentaDto.Generado), alineacion: enumAliniacion.derecha, mostrar: true);
            columnas.Add(titulo: "BI", propiedad: nameof(PlanificadorDeVentaDto.TotalSinIva), alineacion: enumAliniacion.derecha, mostrar: true);
            columnas.Add(titulo: "Id", propiedad: nameof(PlanificadorDeVentaDto.Id), alineacion: enumAliniacion.derecha, mostrar: false);
            columnas.Add(titulo: "IdContrato", propiedad: nameof(PlanificadorDeVentaDto.IdContrato), alineacion: enumAliniacion.derecha, mostrar: false);
            var parametros = new Dictionary<string, object> {
               { nameof(GridDeRelacion.Controlador), typeof(PlanificadorDeVentasController) } ,
               { nameof(GridDeRelacion.AccionDeConsulta), nameof(PlanificadorDeVentasController.epLeerElementos)} ,
               { nameof(GridDeRelacion.PropiedadRestrictora), nameof(PlanificadorDeVentaDto.IdContrato) },
               { nameof(GridDeRelacion.PaginaDondeNavegarAlEditar) , $"{nameof(PlanificadorDeVentasController)}/{nameof(PlanificadorDeVentasController.CrudPlanificadorDeVentas)}?id={nameof(PlanificadorDeVentaDto.Id)}"},
               { nameof(GridDeRelacion.OcultarSiVacio), false}
            };
            var gridDeRelacion = new GridDeRelacion(expansor, columnas, parametros);
            gridDeRelacion.PermitirBorrar = true;


            var accion = new AccionDeGetionarDatosDependientes(
                    urlDelCrud: $@"/{nameof(PlanificadorDeVentasController)}/{nameof(PlanificadorDeVentasController.CrudPlanificadorDeVentas)}?origen=dependencia"
                  , datosDependientes: nameof(PlanificadorDeVentaDto)
                  , nombreDelMnt: DescriptorDeMantenimiento<PlanificadorDeVentaDto>.NombreMnt
                  , propiedadQueRestringe: nameof(ContratoDto.Id)
                  , propiedadRestrictora: nameof(PlanificadorDeVentaDto.IdContrato)
                  , "Crear o gestionar planificadores");

            expansor.DescriptorDeNavegadorRefParaCrear("Crear", accion, enumParaQueNavegar.crear);
            var modal = expansor.DescriptorDeEditarRelaciones(Editor.Crud.Contexto, typeof(PlanificadorDeVentaDto), typeof(PlanificadorDeVentasController), "Editar planificador", false);
            modal.AccionTrasAbrirModal = $"javascript: {enumNameSpaceTs.Juridico}.{enumFunctionTs.Ctr_AjustarControlesDeEdicionDelPlanificador}('{modal.IdHtml}');";

            var modalDeCopia = expansor.DescriptorDeCrearDetalles(Contexto, typeof(CopiarPlfDeVentaDto), nameof(ContratosController), "Copiar"
                , permisosNecesarios: enumModoDeAccesoDeDatos.Gestor
                , accionControlador: nameof(ContratosController.epCopiarPlfDeVenta));
            modalDeCopia.TituloDelBotonDeCrear = "Copiar";

            modalDeCopia.AccionTrasAbrirModal = $"javascript:{enumNameSpaceTs.Juridico}.{enumFunctionTs.Ctr_TrasAbrirModalDeCopiarPlfDeVenta}('{modal.IdHtml}')";

        }


        private void DescriptorDeSpanDeFacturasRec()
        {
            var expansor = new DescriptorDeExpansor(Editor, $"{Editor.Id}-facturasrec", "Facturas Recibidas", true, "Facturas recibidas del contrato");
            Editor.Expanes.Insert(2, expansor);
            var columnas = new DescriptorDeColumnas("facturasrec");
            columnas.Add(titulo: "Factura", propiedad: nameof(FacturaRecDto.Expresion), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnas.Add(titulo: nameof(FacturaRecDto.Numero), alineacion: enumAliniacion.derecha, mostrar: true);
            columnas.Add(titulo: nameof(FacturaRecDto.Tipo), propiedad: nameof(FacturaRecDto.Tipo), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnas.Add(titulo: nameof(FacturaRecDto.Estado), propiedad: nameof(FacturaRecDto.Estado), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnas.Add(titulo: "B.I", propiedad: nameof(FacturaRecDto.BaseImponible), alineacion: enumAliniacion.derecha, mostrar: true, formato: enumFormato.Moneda);
            columnas.Add(titulo: "Total", propiedad: nameof(FacturaRecDto.TotalDelPago), alineacion: enumAliniacion.derecha, mostrar: true, formato: enumFormato.Moneda);
            columnas.Add(titulo: "Pagado", propiedad: nameof(FacturaRecDto.TotalPagado), alineacion: enumAliniacion.derecha, mostrar: true, formato: enumFormato.Moneda);
            columnas.Add(titulo: nameof(FacturaRecDto.Id), propiedad: nameof(FacturaRecDto.Id), alineacion: enumAliniacion.derecha, mostrar: false);
            var parametros = new Dictionary<string, object> {
               { nameof(GridDeRelacion.Controlador), typeof(FacturasRecController) },
               { nameof(GridDeRelacion.AccionDeConsulta), nameof(FacturasRecController.epLeerElementos)},
               { nameof(GridDeRelacion.PropiedadRestrictora), nameof(FacturaRecDto.IdContrato) },
               { nameof(GridDeRelacion.PaginaDondeNavegarAlEditar) , $"{nameof(FacturasRecController)}/{nameof(FacturasRecController.CrudFacturasRec)}?id={nameof(FacturaRecDto.Id)}"},
               { nameof(GridDeRelacion.OcultarSiVacio), false}
            };

            var gridDeRelacion = new GridDeRelacion(expansor, columnas, parametros);
            gridDeRelacion.PermitirBorrar = false;
            gridDeRelacion.PermitirEditar = false;
            expansor.DescriptorParaVincular(Editor.Crud.Contexto, NegociosDeSe.IdNegocio(enumNegocio.FacturaRecibida), typeof(SelectorDeFarDto), nameof(FacturasRecController), "Imputar factura");
        }


        private void DescriptorDeSpanDeFacturasEmt()
        {
            var expansor = new DescriptorDeExpansor(Editor, $"{Editor.Id}-facturaemt", "Facturas Emitidas", true, "Facturas emitidas de un contrato");
            Editor.Expanes.Insert(3, expansor);
            var columnas = new DescriptorDeColumnas("facturasemt");
            columnas.Add(titulo: "Factura", propiedad: nameof(FacturaEmtDto.Expresion), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnas.Add(titulo: "Número", propiedad:nameof(FacturaEmtDto.NumeroFactura), alineacion: enumAliniacion.derecha, mostrar: true);
            columnas.Add(titulo: nameof(FacturaEmtDto.Tipo), propiedad: nameof(FacturaEmtDto.Tipo), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnas.Add(titulo: nameof(FacturaEmtDto.Estado), propiedad: nameof(FacturaEmtDto.Estado), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnas.Add(titulo: "B.I", propiedad: nameof(FacturaEmtDto.TotalSinIva), alineacion: enumAliniacion.derecha, mostrar: true, formato: enumFormato.Moneda);
            columnas.Add(titulo: "Cobrado", propiedad: nameof(FacturaEmtDto.Cobrado), alineacion: enumAliniacion.derecha, mostrar: true, formato: enumFormato.Moneda);
            columnas.Add(titulo: "Pendiente", propiedad: nameof(FacturaEmtDto.Pendiente), alineacion: enumAliniacion.derecha, mostrar: true, formato: enumFormato.Moneda);
            columnas.Add(titulo: nameof(FacturaEmtDto.Id), propiedad: nameof(FacturaEmtDto.Id), alineacion: enumAliniacion.derecha, mostrar: false);
            var parametros = new Dictionary<string, object> {
               { nameof(GridDeRelacion.Controlador), typeof(FacturasEmtController) },
               { nameof(GridDeRelacion.AccionDeConsulta), nameof(FacturasEmtController.epLeerElementos)},
               { nameof(GridDeRelacion.PropiedadRestrictora), nameof(ltrDeUnaFacturaEmt.IdContrato) },
               { nameof(GridDeRelacion.PaginaDondeNavegarAlEditar) , $"{nameof(FacturasEmtController)}/{nameof(FacturasEmtController.CrudFacturasEmt)}?id={nameof(FacturaEmtDto.Id)}"},
               { nameof(GridDeRelacion.OcultarSiVacio), false}
            };

            var gridDeRelacion = new GridDeRelacion(expansor, columnas, parametros);
            gridDeRelacion.PermitirBorrar = false;
            gridDeRelacion.PermitirEditar = false;
        }

        private string TituloDelMantenimeinto(enumClaseDeContrato clase)
        {
            switch (clase)
            {
                case enumClaseDeContrato.Venta: return "Gestión de contratos de venta";
                case enumClaseDeContrato.Compra: return "Gestión de contratos de compra";
                case enumClaseDeContrato.MatriculaDeGuarderia: return "Gestión de matrículas";
            }
            return "Gestión de contratos";
        }
        private string TituloDeCreacion(enumClaseDeContrato clase)
        {
            switch (clase)
            {
                case enumClaseDeContrato.Venta: return "Contrato de venta";
                case enumClaseDeContrato.Compra: return "Contratos de compra";
                case enumClaseDeContrato.MatriculaDeGuarderia: return "Matrícula";
            }
            return "Contrato";
        }
        private string TituloDeEdicion(enumClaseDeContrato clase)
        {
            switch (clase)
            {
                case enumClaseDeContrato.Venta: return "Contrato de venta";
                case enumClaseDeContrato.Compra: return "Contratos de compra";
                case enumClaseDeContrato.MatriculaDeGuarderia: return "Matrícula";
            }
            return "Contrato";
        }

        private void DescriptorDeMatriculasDeGuarderia()
        {
            var matricula = new AmpliacionDeEdicion(Editor, Ampliaciones.Contratos.MatriculaDeGuarderia, "Datos de matrícula", new Dimension(2, 2), ayuda: "Información sobre los datos de la matrícula");
            matricula.Dto = typeof(MatriculaDeGuarderiaDto);
            matricula.Controlador = nameof(MatriculasDeGuarderiaController);
            Editor.Ampliaciones.Add(matricula);
        }

        private void DescriptorDeImportes()
        {
            var saldos = new AmpliacionDeEdicion(Editor, Ampliaciones.Contratos.Importes, "Importes", new Dimension(2, 2), ayuda: "Información sobre los importes adendas y bloqueos del avance del contrato");
            saldos.Dto = typeof(SaldosDelContratoDto);
            saldos.Controlador = nameof(SaldosDelContratoController);
            Editor.Ampliaciones.Add(saldos);
        }

        private void DescriptorDeVentas()
        {
            var noRenderizar = new List<string> { nameof(DatosDelContratoDto.Proveedor), nameof(DatosDelContratoDto.IdProveedor) };
            var datos = new AmpliacionDeEdicion(Editor, Ampliaciones.Contratos.CtrVenta, "Datos de venta", new Dimension(2, 2), ayuda: "Información jurídica del contrato de venta", noRenderizar);
            datos.Dto = typeof(DatosDelContratoDto);
            datos.Controlador = nameof(CtrVentasController);
            Editor.Ampliaciones.Add(datos);
        }

        private void DescriptorDeCompra()
        {
            var noRenderizar = new List<string> { nameof(DatosDelContratoDto.Cliente), nameof(DatosDelContratoDto.IdCliente) };
            var datos = new AmpliacionDeEdicion(Editor, Ampliaciones.Contratos.CtrCompra, "Datos de compra", new Dimension(2, 2), ayuda: "Información jurídica del contrato de compra", noRenderizar);
            datos.Dto = typeof(DatosDelContratoDto);
            datos.Controlador = nameof(CtrVentasController);
            Editor.Ampliaciones.Add(datos);
        }

        private void DescriptorDeAvales()
        {
            var datos = new AmpliacionDeEdicion(Editor, Ampliaciones.Contratos.avalsolicitado, "Aval", new Dimension(2, 2), ayuda: "Información sobre el aval recibido del contrato");
            datos.Dto = typeof(AvalSolicitadoDto);
            datos.Controlador = nameof(AvalesSolicitadosController);
            Editor.Ampliaciones.Add(datos);
        }

        private void IncluirFiltroDeRelacionesVenta()
        {
            var modal = new ModalDeFiltrado<ContratoDto>(Mnt.Filtro, "filtrosDeVentas", "Filtros sobre ventas");
            Mnt.Filtro.Modales.Add(modal);
            FiltroPorUnitario(modal);
            FiltroPorPlfVenta(modal);
            FiltroPorParteTr(modal);
            FiltroPorFacturaEmt(modal);
        }

        private void IncluirFiltroDeRelacionesCompra()
        {
            var modal = new ModalDeFiltrado<ContratoDto>(Mnt.Filtro, "filtrosDeCompras", "Filtros sobre compras");
            Mnt.Filtro.Modales.Add(modal);

        }

        private void IncluirFiltrosDeCabecera()
        {
            var modal = new ModalDeFiltrado<ContratoDto>(Mnt.Filtro, "filtrosDeCabecera", "Filtros de contratos");
            Mnt.Filtro.Modales.Add(modal);
            FiltroPorExpedientes(modal);
            FiltroPorCliente(modal);
            FiltroPorProveedor(modal);
            FiltroPorResponsable(modal);

            modal.ControlesDeFiltrado.Add(new EditorFiltro<ContratoDto>(modal,
                "Datos contacto", ltrDatosDelContrato.DatosContacto,
                "contacto, teléfono o mail"
                , new Posicion(3, 0)));

            var importes = new FiltroEntreImportes<ContratoDto>(modal,
                    etiqueta: "Importe contrato",
                    propiedad: ltrSaldosDelContrato.FiltroPorImporte,
                    ayuda: "filtrar por rango de importes",
                    posicion: new Posicion() { fila = 1, columna = 1 });
            modal.ControlesDeFiltrado.Add(importes);

            //var checkDeMostrarImporte = new CheckDeMostrarColumna<ContratoDto>(modal,
            //    etiqueta: "Mostra importe",
            //    ayuda: "Muestra el importe del contrato",
            //    valorInicial: false,
            //    columna: nameof(ContratoDto.Importe));
            //modal.ControlesDeFiltrado.Add(checkDeMostrarImporte);

            var opciones = new Dictionary<string, string> {
                { $"{ltrProrrogas.contratosProrrogados}",    "vigentes y prorrogados" },
                { $"{ltrProrrogas.contratosFinalizados}",    "finalizados despues de prorrogarse" },
                { $"{ltrAvalesSolicitados.AvalDevuelto}",    "con el aval devuelto" },
                { $"{ltrAvalesSolicitados.PendienteDeAval}", "pendiente de devolver aval" },
                { $"{ltrLotesDeUnContrato.ConLotes}",    "con lotes" },
                { $"{ltrLotesDeUnContrato.SinLotes}", "sin lotes" },
                { $"{ltrPlanificadorDeVentas.PlanificadoresPdts}",    "planificadores pendientes" },
                { $"{ltrPlanificadorDeVentas.ConPlanificadores}",    "con planificadores" },
                { $"{ltrPlanificadorDeVentas.SinPlanificadores}", "sin planificadores" }
            };
            modal.ControlesDeFiltrado.Add(new ListaDeValoresParaFiltrado<ContratoDto>(modal,
                nameof(ContratoDto.ImporteAval),
                ltrDeUnContrato.FiltrosPorEvolucion,
                opciones,
                "filtrar por situación y elementos del contrato"));
        }

        private static void FiltroPorResponsable(ModalDeFiltrado<ContratoDto> modal)
        {
            var ld = new ListasDinamicas<ContratoDto>(modal,
                 etiqueta: "Responsable",
                 filtrarPor: ltrDeUnContrato.FiltroPorResponsable,
                 ayuda: "seleccione el responsable",
                 seleccionarDe: nameof(UsuarioDto),
                 buscarPor: nameof(UsuarioDto.NombreCompleto),
                 mostrarExpresion: $"[{nameof(UsuarioDto.NombreCompleto)}]",
                 criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                 posicion: new Posicion(1, 0),
                 controlador: nameof(UsuariosController),
                 navegarA: nameof(UsuariosController.CrudUsuario),
                 restringirPor: "",
                 alSeleccionarBlanquearControl: "")
            { LongitudMinimaParaBuscar = 1 };

            var opciones = new Dictionary<string, string> {
                      { $"{ltrDeUnContrato.ConOSinResponsable}", "Con o sin responsable" }
                    , { $"{ltrDeUnContrato.ConResponsable}", "Con responsable" }
                    , { $"{ltrDeUnContrato.SinResponsable}", "Sin responsable" } };

            modal.ControlesDeFiltrado.Add(new FiltroDeRelacion<ContratoDto>(modal,
               lista: ld,
               opciones: opciones,
               propiedad: nameof(ContratoDto.Responsable),
               filtrarPor: ltrDeUnContrato.FiltroPorConOSinResponsable,
               ayuda: "Filtros por responsable"
               ));


            //modal.ControlesDeFiltrado.Add(new ListaDinamicaParaMostrarColumna<ContratoDto>(ld, nameof(ContratoDto.Responsable), ltrDeUnContrato.FiltroPorConOSinResponsable, opciones, "Mostrar responsable"));
        }

        private void FiltroPorExpedientes(ModalDeFiltrado<ContratoDto> modal)
        {
            var lista = new ListasDinamicas<ContratoDto>(modal,
            etiqueta: enumNegocio.Expediente.Singular(),
            filtrarPor: ltrDeUnContrato.IdExpediente,
            ayuda: "seleccione el expediente",
            seleccionarDe: nameof(ExpedienteDto),
            buscarPor: ltrDeUnExpediente.ExpedientePadre,
            mostrarExpresion: nameof(ExpedienteDto.Expresion),
            criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
            posicion: new Posicion(1, 0),
            controlador: nameof(ExpedientesController),
            navegarA: nameof(ExpedientesController.CrudExpedientes),
            restringirPor: "",
            alSeleccionarBlanquearControl: "")
            { LongitudMinimaParaBuscar = 1 };

            var opciones = new Dictionary<string, string> {
                      { $"{ltrParametrosNeg.MostrarTodos}", "Con o sin expediente padre" }
                    , { $"{ltrParametrosNeg.ConRelacion}", "Con expediente" }
                    , { $"{ltrParametrosNeg.SinRelacion}", "Sin expediente" } };

            modal.ControlesDeFiltrado.Add(new FiltroDeRelacion<ContratoDto>(modal,
                lista: lista,
                opciones: opciones,
                propiedad: ltrDeUnExpediente.ExpedientePadre,
                filtrarPor: ltrDeUnContrato.FiltroPorConOSinExpediente,
                ayuda: "Filtros de expedientes"
                ));
        }

        private void FiltroPorCliente(ModalDeFiltrado<ContratoDto> modal)
        {
            if (_clase == enumClaseDeContrato.Venta)
            {
                var ld = new ListasDinamicas<ContratoDto>(modal,
                    etiqueta: enumNegocio.Cliente.Singular(),
                    filtrarPor: ltrDatosDelContrato.FiltroPorCliente,
                    ayuda: $"seleccione el {enumNegocio.Cliente.Singular(true)}",
                    seleccionarDe: nameof(ClienteDto),
                    buscarPor: nameof(ClienteDto.Expresion),
                    mostrarExpresion: nameof(ClienteDto.Expresion),
                    criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                    posicion: new Posicion(1, 0),
                    controlador: nameof(ClientesController),
                    navegarA: nameof(ClientesController.CrudClientes),
                    restringirPor: "",
                    alSeleccionarBlanquearControl: "")
                { LongitudMinimaParaBuscar = 1 };

                var opciones = new Dictionary<string, string> {
                      { $"{ltrDatosDelContrato.ConOSinCliente}", "Con o sin cliente" }
                    , { $"{ltrDatosDelContrato.ConCliente}", "Con clientes" }
                    , { $"{ltrDatosDelContrato.SinCliente}", "Sin clientes" } };


                modal.ControlesDeFiltrado.Add(new FiltroDeRelacion<ContratoDto>(modal,
                   lista: ld,
                   opciones: opciones,
                   propiedad: nameof(ContratoDto.Cliente),
                   filtrarPor: ltrDatosDelContrato.FiltroPorConOSinCliente,
                   ayuda: "Filtros por cliente"
                   ));

                //modal.ControlesDeFiltrado.Add(new ListaDinamicaParaMostrarColumna<ContratoDto>(ld, nameof(ContratoDto.Cliente), ltrDatosDelContrato.FiltroPorConOSinCliente, opciones, "Mostrar cliente"));
            }
        }

        private void FiltroPorProrroga(ModalDeFiltrado<ContratoDto> modal)
        {

            var opciones = new Dictionary<string, string> {
                      { $"{ltrDatosDelContrato.ConOSinCliente}", "Con o sin prórroga" }
                    , { $"{ltrDatosDelContrato.ConCliente}", "Prorrogados" }
                    , { $"{ltrDatosDelContrato.SinCliente}", "Sin prorrogar" } };

            // modal.ControlesDeFiltrado.Add(new ListaDinamicaParaMostrarColumna<ContratoDto>(ld, nameof(ContratoDto.Cliente), ltrDatosDelContrato.FiltroPorConOSinCliente, opciones, "Mostrar cliente"));

        }

        private void FiltroPorProveedor(ModalDeFiltrado<ContratoDto> modal)
        {
            if (_clase == enumClaseDeContrato.Compra)
            {
                var ld = new ListasDinamicas<ContratoDto>(modal,
                     etiqueta: enumNegocio.Proveedor.Singular(),
                     filtrarPor: ltrDatosDelContrato.FiltroPorProveedor,
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

                var opciones = new Dictionary<string, string> {
                      { $"{ltrDatosDelContrato.ConOSinProveedor}", "Con o sin proveedor" }
                    , { $"{ltrDatosDelContrato.ConProveedor}", "Con proveedor" }
                    , { $"{ltrDatosDelContrato.SinProveedor}", "Sin proveedor" } };

                modal.ControlesDeFiltrado.Add(new FiltroDeRelacion<ContratoDto>(modal,
                   lista: ld,
                   opciones: opciones,
                   propiedad: nameof(ContratoDto.Proveedor),
                   filtrarPor: ltrDatosDelContrato.FiltroPorConOSinProveedor,
                   ayuda: "Filtros por proveedor"
                   ));

                //modal.ControlesDeFiltrado.Add(new ListaDinamicaParaMostrarColumna<ContratoDto>(ld, nameof(ContratoDto.Proveedor), ltrDatosDelContrato.FiltroPorConOSinProveedor, opciones, "Mostrar proveedor"));
            }
        }

        private void FiltroPorPlfVenta(ModalDeFiltrado<ContratoDto> modal)
        {
            var lista = new ListasDinamicas<ContratoDto>(modal,
            etiqueta: enumNegocio.PlanificacionDeVenta.Singular(),
            filtrarPor: ltrDeUnContrato.IdPlfDeVenta,
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
            { LongitudMinimaParaBuscar = 1 };

            var opciones = new Dictionary<string, string> {
                      { $"{ltrParametrosNeg.MostrarTodos}", "Con o sin planificaciones" }
                    , { $"{ltrParametrosNeg.ConRelacion}", "Con planificaciones" }
                    , { $"{ltrParametrosNeg.SinRelacion}", "Sin planificaciones" } };

            modal.ControlesDeFiltrado.Add(new FiltroDeRelacion<ContratoDto>(modal,
                lista: lista,
                opciones: opciones,
                propiedad: ltrDeUnContrato.FiltroPorConOSinPlfDeVenta,
                filtrarPor: ltrDeUnContrato.FiltroPorConOSinPlfDeVenta,
                ayuda: $"Filtros de {enumNegocio.PlanificacionDeVenta.Plural(true)}"
                ));
        }

        private void FiltroPorUnitario(ModalDeFiltrado<ContratoDto> modal)
        {
            modal.ControlesDeFiltrado.Add(new ListasDinamicas<ContratoDto>(modal,
                etiqueta: enumNegocio.Unitario.Singular(),
                filtrarPor: ltrDeUnContrato.IdUnitario,
                ayuda: $"seleccione el {enumNegocio.Unitario.Singular(true)}",
                seleccionarDe: nameof(UnitarioDto),
                buscarPor: nameof(UnitarioDto.Expresion),
                mostrarExpresion: nameof(UnitarioDto.Expresion),
                criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                posicion: new Posicion(1, 0),
                controlador: nameof(UnitariosController),
                navegarA: nameof(UnitariosController.CrudUnitarios),
                restringirPor: "",
                alSeleccionarBlanquearControl: "")
            { LongitudMinimaParaBuscar = 1 });
        }

        private void FiltroPorParteTr(ModalDeFiltrado<ContratoDto> modal)
        {
            var lista = new ListasDinamicas<ContratoDto>(modal,
            etiqueta: enumNegocio.ParteDeTrabajo.Singular(),
            filtrarPor: ltrDeUnContrato.IdParteTr,
            ayuda: $"seleccione la {enumNegocio.ParteDeTrabajo.Singular(true)}",
            seleccionarDe: nameof(ParteTrDto),
            buscarPor: nameof(ParteTrDto.Expresion),
            mostrarExpresion: nameof(ParteTrDto.Expresion),
            criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
            posicion: new Posicion(1, 0),
            controlador: nameof(PartesTrController),
            navegarA: nameof(PartesTrController.CrudPartesDeTrabajo),
            restringirPor: "",
            alSeleccionarBlanquearControl: "")
            { LongitudMinimaParaBuscar = 1 };

            var opciones = new Dictionary<string, string> {
                      { $"{ltrParametrosNeg.MostrarTodos}", "Con o sin partes de trabajo" }
                    , { $"{ltrParametrosNeg.ConRelacion}", "Con partes" }
                    , { $"{ltrParametrosNeg.SinRelacion}", "Sin partes" } };

            modal.ControlesDeFiltrado.Add(new FiltroDeRelacion<ContratoDto>(modal,
                lista: lista,
                opciones: opciones,
                propiedad: ltrDeUnContrato.FiltroPorConOSinParteTr,
                filtrarPor: ltrDeUnContrato.FiltroPorConOSinParteTr,
                ayuda: $"Filtros de {enumNegocio.ParteDeTrabajo.Plural(true)}"
                ));
        }

        private void FiltroPorFacturaEmt(ModalDeFiltrado<ContratoDto> modal)
        {
            var lista = new ListasDinamicas<ContratoDto>(modal,
            etiqueta: enumNegocio.FacturaEmitida.Singular(),
            filtrarPor: ltrDeUnContrato.IdFacturaEmt,
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
                      { $"{ltrParametrosNeg.MostrarTodos}", "Con o sin facturas" }
                    , { $"{ltrParametrosNeg.ConRelacion}", "Con facturas" }
                    , { $"{ltrParametrosNeg.SinRelacion}", "Sin facturas" } };

            modal.ControlesDeFiltrado.Add(new FiltroDeRelacion<ContratoDto>(modal,
                lista: lista,
                opciones: opciones,
                propiedad: ltrDeUnContrato.FiltroPorConOSinFacturaEmt,
                filtrarPor: ltrDeUnContrato.FiltroPorConOSinFacturaEmt,
                ayuda: $"Filtros de {enumNegocio.FacturaEmitida.Plural(true)}"
                ));
        }

        private void DefinirMf(string idMenu, List<string> opciones)
        {
            DescriptorDeEdicion<ContratoDto>.IncluirMfIndividual(opciones, "<hr>");
            DescriptorDeEdicion<ContratoDto>.IncluirMfIndividual(opciones, $"<li id='{idMenu}.1' accion-menu='{eventosDeMf.Ctr_VincularRegistroEntrada}' {AtributosHtml.Mf(enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Gestor, false)}>Asociar registro de E/S</li>");
        }

        private void DescriptorDeSpanDeRegistroEs()
        {
            var expansor = new DescriptorDeExpansor(Editor, $"{Editor.Id}-registrosEs", "RegistrosEs", true, "Registros de ES del pleito");
            Editor.Expanes.Insert(1, expansor);

            //Definimos el grid de detalles del cuerpo
            var columnas = new DescriptorDeColumnas("registrosEs");
            columnas.Add(titulo: "Registro", propiedad: nameof(RegistroEsDto.Expresion), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnas.Add(titulo: "Estado", propiedad: nameof(RegistroEsDto.Estado), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnas.Add(titulo: "Id", propiedad: nameof(RegistroEsDtm.Id), alineacion: enumAliniacion.derecha, mostrar: false);
            var parametros = new Dictionary<string, object> {
                   { nameof(GridDeRelacion.Controlador), typeof(RegistrosEsController) }
                 , { nameof(GridDeRelacion.AccionDeConsulta), nameof(BaseController<ContratoDto>.epLeerVinculosCon)}
                 , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(VinculoDtm.idElemento1) }
                 , { nameof(ltrParametrosEp.idVinculado) , NegociosDeSe.IdNegocio(enumNegocio.Registro) }
                 , { nameof(GridDeRelacion.PaginaDondeNavegarAlEditar) , $"{nameof(RegistrosEsController)}/{nameof(RegistrosEsController.CrudRegistrosEs)}"}
                 , { nameof(GridDeRelacion.OcultarSiVacio), true}
                };
            var gridDeRelacion = new GridDeRelacion(expansor, columnas, parametros);
            gridDeRelacion.PermitirBorrar = true;

            expansor.DescriptorParaVincular(Editor.Crud.Contexto, NegociosDeSe.IdNegocio(enumNegocio.Registro), typeof(SelectorDeRegistroEsDto), nameof(RegistrosEsController), "Asociar registro de E/S");
        }

        private void DescriptorDeAvances()
        {
            var avance = new AmpliacionDeEdicion(Editor, Ampliaciones.Contratos.avance, "Avance", new Dimension(2, 2), ayuda: "Información sobre datos del avance del contrato");
            avance.Dto = typeof(AvanceDto);
            avance.Controlador = nameof(AvancesController);
            Editor.Ampliaciones.Add(avance);
        }
        private void DescriptorDeProrrogas()
        {
            var prorroga = new AmpliacionDeEdicion(Editor, Ampliaciones.Contratos.prorroga, "Prórroga", new Dimension(2, 2), ayuda: "Información sobre la prorrogación del contrato");
            prorroga.Dto = typeof(ProrrogaDto);
            prorroga.Controlador = nameof(ProrrogasController);
            Editor.Ampliaciones.Add(prorroga);
        }

        private void DescriptorDeMinuta()
        {
            var expansor = new DescriptorDeExpansor(Editor, $"{Editor.Id}-minuta", "Minuta", true, "Minuta de un pleito");
            expansor.EsDetalle = true;
            Editor.Expanes.Insert(0, expansor);

            //Definimos el grid de detalles de la minuta
            var columnas = new DescriptorDeColumnas("minuta");
            columnas.Add(titulo: "Orden", propiedad: nameof(MinutaDto.Orden), alineacion: enumAliniacion.derecha, mostrar: true, tamano: 100);
            columnas.Add(titulo: "Concepto", propiedad: nameof(MinutaDto.Concepto), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnas.Add(titulo: "Valor", propiedad: nameof(MinutaDto.Valor), alineacion: enumAliniacion.derecha, mostrar: true, tamano: 150);
            columnas.Add(titulo: "Abonado", propiedad: nameof(MinutaDto.Abonado), alineacion: enumAliniacion.derecha, mostrar: true, tamano: 150);
            columnas.Add(titulo: "Pendiente", propiedad: nameof(MinutaDto.Pendiente), alineacion: enumAliniacion.derecha, mostrar: true, tamano: 150);
            columnas.Add(titulo: "IdElemento", propiedad: nameof(MinutaDto.IdElemento), alineacion: enumAliniacion.derecha, mostrar: false);
            columnas.Add(titulo: "Id", propiedad: nameof(MinutaDto.Id), alineacion: enumAliniacion.derecha, mostrar: false);

            var orden = $"{nameof(MinutaDto.Orden)}:{enumModoOrdenacion.ascendente.Render()};" +
                        $"{nameof(MinutaDto.CreadoEl)}:{enumModoOrdenacion.ascendente.Render()}";


            var parametros = new Dictionary<string, object> {
                   { nameof(GridDeRelacion.Controlador), typeof(MinutasController) }
                 , { nameof(GridDeRelacion.AccionDeConsulta), nameof(MinutasController.epLeerElementos)}
                 , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(MinutaDto.IdElemento) }
                ,  { nameof(GridDeRelacion.OrdenarPor), orden }
                 , { nameof(GridDeRelacion.OcultarSiVacio), false}
                };
            var gridDeRelacion = new GridDeRelacion(expansor, columnas, parametros);
            gridDeRelacion.PermitirBorrar = true;

            var modalDeCreacion = expansor.DescriptorDeCrearRelaciones(Editor.Crud.Contexto, typeof(MinutaDto), typeof(MinutasController), nameof(MinutaDto.IdElemento), "Añadir concepto");
            modalDeCreacion.AccionTrasAbrirModal = $"javascript: {enumNameSpaceTs.Juridico}.{enumFunctionTs.InicializarModalParaCrearMinuta}('{modalDeCreacion.IdHtml}')";

            expansor.DescriptorDeEditarRelaciones(Editor.Crud.Contexto, typeof(MinutaDto), typeof(MinutasController), "Editar concepto", soloConsulta: false);
        }


        public override string RenderControl()
        {
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{_clase}-{GetType().FullName}";
            if (ServicioDeCaches.Obtener(CacheDe.RenderCrud).ContainsKey(indice))
                return (string)ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice];
            var render = base.RenderControl();

            render = render +
                   $@"<script src=¨../../js/{RutaBase}/ApiDeContratos.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script src=¨../../js/{RutaBase}/ApiDePlanificadorDeVentas.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script src=¨../../js/{RutaBase}/Contratos.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{      
                           {RutaBase}.CrearCrudDeContratos('{Mnt.IdHtml}', '{Creador.IdHtml}', '{Editor.IdHtml}', '{Borrado.IdHtml}', '{_clase}') 
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
