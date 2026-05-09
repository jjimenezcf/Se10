using ServicioDeDatos;
using MVCSistemaDeElementos.Controllers;
using UtilidadesParaIu;
using Utilidades;
using System.Collections.Generic;
using ServicioDeDatos.Seguridad;
using ModeloDeDto;
using ModeloDeDto.Ventas;
using GestoresDeNegocio.Juridico;
using ModeloDeDto.MaestrosTecnico;
using GestorDeElementos;
using ModeloDeDto.Juridico;
using ServicioDeDatos.Ventas;
using ModeloDeDto.Tarea;
using GestorDeElementos.Extensores;
using ServicioDeDatos.Juridico;
using ModeloDeDto.Expediente;
using ModeloDeDto.Presupuesto;
using ServicioDeDatos.Presupuesto;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDePartesTr : DescriptorDeCrud<ParteTrDto>
    {
        public DescriptorDePartesTr(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto
               , nameof(PartesTrController)
               , nameof(PartesTrController.CrudPartesDeTrabajo)
               , modo
               , rutaBase: enumNameSpaceTs.Venta)
        {
            Mnt.IncluirMfDeRelacion($"<li id='{menuDeRelaciones}.{enumNegocio.PlanificacionDeVenta}' accion-menu='{eventosDeMf.Ptr_IrAPlvDeVenta}' {AtributosHtml.Mf(enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Consultor, false)}>{enumNegocio.PlanificacionDeVenta.Plural()}</li>");
            Mnt.IncluirMfDeRelacion($"<li id='{menuDeRelaciones}.{enumNegocio.FacturaEmitida}' accion-menu='{eventosDeMf.Ptr_IrAFacturasEmt}' {AtributosHtml.Mf(enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Consultor, false)}>{enumNegocio.FacturaEmitida.Plural()}</li>");
            Mnt.IncluirMfDeRelacion($"<li id='{menuDeRelaciones}.{enumNegocio.Presupuesto}' accion-menu='{eventosDeMf.Ptr_IrAPpts}' {AtributosHtml.Mf(enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Consultor, false)}>{enumNegocio.Presupuesto.Plural()}</li>");
            Mnt.IncluirMfDeRelacion($"<li id='{menuDeRelaciones}.{enumNegocio.Contrato}' accion-menu='{eventosDeMf.Ptr_IrAContrato}' {AtributosHtml.Mf(enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Consultor, false)}>{enumNegocio.Contrato.Plural()}</li>");

            IncluirFiltrosDePartesTr();
            DescriptorDeAsignaciones();
            DescriptorDeLineasDeUnPtr();
        }

        private void IncluirFiltrosDePartesTr()
        {
            var modal = new ModalDeFiltrado<ParteTrDto>(Mnt.Filtro, "filtroDePartesTr", "Elementos relacionados");
            Mnt.Filtro.Modales.Add(modal);
            FiltroPorUnitario(modal);
            FiltroPorContrato(modal);
            FiltroPorPresupuesto(modal);
            FiltroPorPlfVenta(modal);
            FiltroPorFacturaEmt(modal);
            FiltroPorTarea(modal);
        }

        private void FiltroPorPlfVenta(ModalDeFiltrado<ParteTrDto> modal)
        {
            var lista = new ListasDinamicas<ParteTrDto>(modal,
                 etiqueta: enumNegocio.PlanificacionDeVenta.Singular(),
                 filtrarPor: ltrDeUnParteTr.IdPlfDeVenta,
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

            modal.ControlesDeFiltrado.Add(new FiltroDeRelacion<ParteTrDto>(modal,
                lista: lista,
                opciones: opciones,
                propiedad: ltrDeUnParteTr.FiltroPorConOSinPlfDeVenta,
                filtrarPor: ltrDeUnParteTr.FiltroPorConOSinPlfDeVenta,
                ayuda: $"Filtros de {enumNegocio.PlanificacionDeVenta.Plural(true)}"
                ));
        }

        private void FiltroPorUnitario(ModalDeFiltrado<ParteTrDto> modal)
        {
            modal.ControlesDeFiltrado.Add(new ListasDinamicas<ParteTrDto>(modal,
                etiqueta: enumNegocio.Unitario.Singular(),
                filtrarPor: ltrDeUnParteTr.IdUnitario,
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

        private void FiltroPorContrato(ModalDeFiltrado<ParteTrDto> modal)
        {
            var lista = new ListasDinamicas<ParteTrDto>(modal,
                   etiqueta: enumNegocio.Contrato.Singular(),
                   filtrarPor: ltrDeUnParteTr.IdContrato,
                   ayuda: $"seleccione la {enumNegocio.Contrato.Singular(true)}",
                   seleccionarDe: nameof(ContratoDto),
                   buscarPor: ltrDeUnContrato.SelectorParaUnParteTr,
                   mostrarExpresion: nameof(ContratoDto.Expresion),
                   criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                   posicion: new Posicion(1, 0),
                   controlador: nameof(ContratosController),
                   navegarA: nameof(ContratosController.CrudContratos),
                   restringirPor: "",
                   alSeleccionarBlanquearControl: "",
                   trasMapear: $"javascript:{nameof(enumNameSpaceTs.Venta)}.{nameof(enumFunctionTs.Ptr_Tras_Mapear_Filtro_IdContrato)}({nameof(enumParamTs.lista)})")
            {
                LongitudMinimaParaBuscar = 1,
                ParametrosParaNavegar = nameof(ltrParametrosEp.Clase) + "=" + nameof(enumClaseDeContrato.Venta)
            };

            var opciones = new Dictionary<string, string> {
                      { $"{ltrParametrosNeg.MostrarTodos}", "Con o sin contratos" }
                    , { $"{ltrParametrosNeg.ConRelacion}", "Con contratos" }
                    , { $"{ltrParametrosNeg.SinRelacion}", "Sin contratos" } };

            modal.ControlesDeFiltrado.Add(new FiltroDeRelacion<ParteTrDto>(modal,
                lista: lista,
                opciones: opciones,
                propiedad: ltrDeUnParteTr.FiltroPorConOSinContrato,
                filtrarPor: ltrDeUnParteTr.FiltroPorConOSinContrato,
                ayuda: $"Filtros de {enumNegocio.Contrato.Plural(true)}"
                ));
        }

        private static void FiltroPorPresupuesto(ModalDeFiltrado<ParteTrDto> modal)
        {
            var lista = new ListasDinamicas<ParteTrDto>(modal,
                etiqueta: "Presupuesto",
                filtrarPor: ltrDeUnParteTr.IdPresupuesto,
                ayuda: "seleccione el presupuesto",
                seleccionarDe: nameof(PresupuestoDto),
                buscarPor: nameof(ltrDeUnPresupuesto.PptDeUnPartTr),
                mostrarExpresion: nameof(ExpedienteDto.Expresion),
                criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                posicion: new Posicion(1, 0),
                controlador: nameof(PresupuestosController),
                navegarA: nameof(PresupuestosController.CrudPresupuestos),
                restringirPor: "",
                alSeleccionarBlanquearControl: "")
            { LongitudMinimaParaBuscar = 1 };

            var opciones = new Dictionary<string, string> {
                      { $"{ltrParametrosNeg.MostrarTodos}", "con o sin ppt" }
                    , { $"{ltrParametrosNeg.ConRelacion}", "partes de un ppt" }
                    , { $"{ltrParametrosNeg.SinRelacion}", "partes que no son de un ppt" } };

            modal.ControlesDeFiltrado.Add(new FiltroDeRelacion<ParteTrDto>(modal,
                lista: lista,
                opciones: opciones,
                propiedad: nameof(ltrDeUnParteTr.FiltroPorConOSinPresupuesto),
                filtrarPor: nameof(ltrDeUnParteTr.FiltroPorConOSinPresupuesto),
                ayuda: "Filtros de presupuesto"
                ));
        }
        private void FiltroPorFacturaEmt(ModalDeFiltrado<ParteTrDto> modal)
        {
            var lista = new ListasDinamicas<ParteTrDto>(modal,
                  etiqueta: enumNegocio.FacturaEmitida.Singular(),
                  filtrarPor: ltrDeUnParteTr.IdFacturaEmt,
                  ayuda: $"seleccione la {enumNegocio.FacturaEmitida.Singular(true)}",
                  seleccionarDe: nameof(FacturaEmtDto),
                  buscarPor: ltrDeUnaFacturaEmt.DependeDeParteTr,
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

            modal.ControlesDeFiltrado.Add(new FiltroDeRelacion<ParteTrDto>(modal,
                lista: lista,
                opciones: opciones,
                propiedad: ltrDeUnParteTr.FiltroPorConOSinFacturaEmt,
                filtrarPor: ltrDeUnParteTr.FiltroPorConOSinFacturaEmt,
                ayuda: $"Filtros de {enumNegocio.FacturaEmitida.Plural(true)}"
                ));
        }

        private void FiltroPorTarea(ModalDeFiltrado<ParteTrDto> modal)
        {
            var lista = new ListasDinamicas<ParteTrDto>(modal,
                etiqueta: enumNegocio.Tarea.Singular(),
                filtrarPor: ltrDeUnParteTr.IdTarea,
                ayuda: $"seleccione la {enumNegocio.Tarea.Singular(true)}",
                seleccionarDe: nameof(TareaDto),
                buscarPor: ltrDeUnaTarea.DependeDeParteTr,
                mostrarExpresion: nameof(TareaDto.Expresion),
                criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                posicion: new Posicion(1, 0),
                controlador: nameof(TareasController),
                navegarA: nameof(TareasController.CrudTareas),
                restringirPor: "",
                alSeleccionarBlanquearControl: "",
                trasMapear: $"javascript:{nameof(enumNameSpaceTs.Venta)}.{nameof(enumFunctionTs.Ptr_Tras_Mapear_Filtro_IdTarea)}({nameof(enumParamTs.lista)})")
            { LongitudMinimaParaBuscar = 1 };

            var opciones = new Dictionary<string, string> {
                      { $"{ltrParametrosNeg.MostrarTodos}", "Con o sin tareas" }
                    , { $"{ltrParametrosNeg.ConRelacion}", "Con tareas" }
                    , { $"{ltrParametrosNeg.SinRelacion}", "Sin tareas" } };

            modal.ControlesDeFiltrado.Add(new FiltroDeRelacion<ParteTrDto>(modal,
                lista: lista,
                opciones: opciones,
                propiedad: ltrDeUnParteTr.FiltroPorConOSinTarea,
                filtrarPor: ltrDeUnParteTr.FiltroPorConOSinTarea,
                ayuda: $"Filtros de {enumNegocio.Tarea.Plural(true)}"
                ));
        }


        private void DescriptorDeLineasDeUnPtr()
        {
            var expansor = new DescriptorDeExpansor(Editor, $"{Editor.Id}-lineasdeunptr", "Detalle", true, "Lineas del parte de trabajo");
            expansor.EsDetalle = true;
            Editor.Expanes.Insert(1, expansor);

            var columnas = new DescriptorDeColumnas("lineasDeUnPtr");
            columnas.Add(titulo: nameof(LineaDeUnPtrDto.Orden), alineacion: enumAliniacion.derecha, tamano: 100);
            columnas.Add(titulo: nameof(LineaDeUnPtrDto.Concepto));
            columnas.Add(titulo: nameof(LineaDeUnPtrDto.Unidad));
            columnas.Add(titulo: nameof(LineaDeUnPtrDto.Cantidad), formato: enumFormato.Numero_6, tamano: 150);
            columnas.Add(titulo: nameof(LineaDeUnPtrDto.Precio), formato: enumFormato.Moneda, tamano: 150);
            columnas.Add(titulo: nameof(LineaDeUnPtrDto.Descuento), formato: enumFormato.Porcentaje, tamano: 150);
            columnas.Add(titulo: "B.I.", propiedad: nameof(LineaDeUnPtrDto.BaseImponible), formato: enumFormato.Moneda, tamano: 150);
            columnas.Add(titulo: nameof(LineaDeUnPtrDto.Iva), formato: enumFormato.Porcentaje, tamano: 150);
            columnas.Add(titulo: "Importe", propiedad: nameof(LineaDeUnPtrDto.ImporteDeLinea), formato: enumFormato.Moneda, tamano: 150);
            columnas.Add(titulo: nameof(LineaDeUnPtrDto.IdElemento), mostrar: false);
            columnas.Add(titulo: nameof(LineaDeUnPtrDto.Id), mostrar: false);

            var orden = $"{nameof(LineaDeUnPtrDto.Orden)}:{enumModoOrdenacion.ascendente.Render()}";


            var parametros = new Dictionary<string, object> {
                   { nameof(GridDeRelacion.Controlador), typeof(LineasDeUnPtrController) }
                 , { nameof(GridDeRelacion.AccionDeConsulta), nameof(LineasDeUnPtrController.epLeerElementos)}
                 , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(LineaDeUnPtrDto.IdElemento) }
                ,  { nameof(GridDeRelacion.OrdenarPor), orden }
                 , { nameof(GridDeRelacion.OcultarSiVacio), false}
                };
            var gridDeRelacion = new GridDeRelacion(expansor, columnas, parametros);
            gridDeRelacion.PermitirBorrar = true;

            var modalDeCreacion = expansor.DescriptorDeCrearRelaciones(Editor.Crud.Contexto, typeof(LineaDeUnPtrDto), typeof(LineasDeUnPtrController), nameof(LineaDeUnPtrDto.IdElemento), "Añadir línea");
            modalDeCreacion.AccionTrasAbrirModal = $"javascript: {enumNameSpaceTs.Venta}.{enumFunctionTs.Ptr_InicializarModalParaCrearLineas}()";

            var modalDeEdicion = expansor.DescriptorDeEditarRelaciones(Editor.Crud.Contexto, typeof(LineaDeUnPtrDto), typeof(LineasDeUnPtrController), "Editar línea", soloConsulta: false);
            modalDeEdicion.AccionTrasAbrirModal = $"javascript: {enumNameSpaceTs.Venta}.{enumFunctionTs.Ptr_InicializarModalParaEditarLineas}();";

        }

        private void DescriptorDeAsignaciones()
        {
            var expansor = new DescriptorDeExpansor(Editor, $"{Editor.Id}-asignaciones", "Trabajadores", true, "Asignaciones de un parte de trabajo");
            expansor.EsDetalle = true;
            Editor.Expanes.Insert(0, expansor);

            //Definimos el grid de detalles de la tarifas
            var columnas = new DescriptorDeColumnas("asignaciones");
            columnas.Add(titulo: "Trabajador", propiedad: nameof(AsignacionDePtrDto.Trabajador), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnas.Add(titulo: "Cuando empezar", propiedad: nameof(AsignacionDePtrDto.PlfDeInicio), alineacion: enumAliniacion.derecha, mostrar: true, tamano: 200, formato: enumFormato.FechaHoraMinutos);
            columnas.Add(titulo: "Cuando terminar", propiedad: nameof(AsignacionDePtrDto.PlfDeFin), alineacion: enumAliniacion.derecha, mostrar: true, tamano: 200, formato: enumFormato.FechaHoraMinutos);
            columnas.Add(titulo: "Empezó", propiedad: nameof(AsignacionDePtrDto.Iniciada), alineacion: enumAliniacion.derecha, mostrar: true, tamano: 200, formato: enumFormato.FechaHoraMinutos);
            columnas.Add(titulo: "Terminó", propiedad: nameof(AsignacionDePtrDto.Finalizada), alineacion: enumAliniacion.derecha, mostrar: true, tamano: 200, formato: enumFormato.FechaHoraMinutos);
            columnas.Add(titulo: "Tiempo dedicado", propiedad: nameof(AsignacionDePtrDto.Duracion), alineacion: enumAliniacion.derecha, mostrar: true, tamano: 200);
            columnas.Add(titulo: "", propiedad: nameof(AsignacionDePtrDto.LtrMedidoEn), alineacion: enumAliniacion.izquierda, mostrar: true, tamano: 200);
            columnas.Add(titulo: "IdElemento", propiedad: nameof(AsignacionDePtrDto.IdElemento), alineacion: enumAliniacion.derecha, mostrar: false);
            columnas.Add(titulo: "IdTrabajador", propiedad: nameof(AsignacionDePtrDto.IdTrabajador), alineacion: enumAliniacion.derecha, mostrar: false);
            columnas.Add(titulo: "Id", propiedad: nameof(AsignacionDePtrDto.Id), alineacion: enumAliniacion.derecha, mostrar: false);

            var orden = $"{nameof(AsignacionDePtrDtm.Trabajador)}.{nameof(AsignacionDePtrDtm.Trabajador.Nombre)}:{enumModoOrdenacion.ascendente.Render()}";


            var parametros = new Dictionary<string, object> {
                   { nameof(GridDeRelacion.Controlador), typeof(AsignacionesDePtrController) }
                 , { nameof(GridDeRelacion.AccionDeConsulta), nameof(AsignacionesDePtrController.epLeerElementos)}
                 , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(AsignacionDePtrDto.IdElemento) }
                ,  { nameof(GridDeRelacion.OrdenarPor), orden }
                 , { nameof(GridDeRelacion.PaginaDondeNavegarAlEditar) , $"{nameof(TrabajadoresController)}/{nameof(TrabajadoresController.CrudTrabajadores)}?id={nameof(AsignacionDePtrDto.IdTrabajador)}" }
                 , { nameof(GridDeRelacion.OcultarSiVacio), false}
                };
            var gridDeRelacion = new GridDeRelacion(expansor, columnas, parametros);
            gridDeRelacion.PermitirBorrar = true;

            var modalDeCreacion = expansor.DescriptorDeCrearRelaciones(Editor.Crud.Contexto, typeof(AsignacionDePtrDto), typeof(AsignacionesDePtrController), nameof(AsignacionDePtrDto.IdElemento), "Asignar parte de trabajo");
            modalDeCreacion.AccionTrasAbrirModal = $"javascript: Crud.{enumGestorDeEventos.EventosDeExpansores}('{eventosDeExpansor.TrasAbrirModal}', '{modalDeCreacion.IdHtml};proponer-cg')";

            expansor.DescriptorDeEditarRelaciones(Editor.Crud.Contexto, typeof(AsignacionDePtrDto), typeof(AsignacionesDePtrController), "Editar asignación", soloConsulta: false);
        }



        public override string RenderControl()
        {
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
if (ServicioDeCaches.Obtener(CacheDe.RenderCrud).ContainsKey(indice))
				 return (string)ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice];
var render = base.RenderControl();

            render = render +
                   $@"<script src=¨../../js/{RutaBase}/ApiDePartesTr.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script src=¨../../js/{RutaBase}/PartesTr.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{      
                           {RutaBase}.CrearCrudDePartesTr('{Mnt.IdHtml}', '{Creador.IdHtml}', '{Editor.IdHtml}', '{Borrado.IdHtml}') 
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
