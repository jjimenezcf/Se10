using ServicioDeDatos;
using MVCSistemaDeElementos.Controllers;
using UtilidadesParaIu;
using Utilidades;
using ServicioDeDatos.Seguridad;
using ModeloDeDto.Ventas;
using ModeloDeDto;
using System.Collections.Generic;
using GestorDeElementos;
using ModeloDeDto.MaestrosTecnico;
using ServicioDeDatos.Ventas;
using ModeloDeDto.Juridico;
using ServicioDeDatos.Juridico;
using ModeloDeDto.Terceros;
using ServicioDeDatos.Presupuesto;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeAsignacionesPtr : DescriptorDeCrud<AsignacionDePtrDto>
    {
        public DescriptorDeAsignacionesPtr(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto
               , nameof(AsignacionesDePtrController)
               , nameof(AsignacionesDePtrController.CrudAsignacionesPtr)
               , modo
               , rutaBase: enumNameSpaceTs.Venta)
        {
            Mnt.Etiqueta = AsignacionesDePartes.TituloMantenimiento;
            Editor.Etiqueta = AsignacionesDePartes.TituloEdicion;
            Editor.Editable = false;

            RenombrarEtiqueta("Nombre", "Trabajador", "Buscar apellido, correo, o nif");

            DefinirMf(menuIndividual, Mnt.OpcionesPorElemento);
            DescriptorDeLineasDeUnPtr();
            IncluirFiltrosDeAsignaciones();
            modalesParaPedirDatos.Add(new ModalParaPedirDatos(this, typeof(FechasDeEjecucionDto), eventosDeMf.Ptr_SolicitarFechaDeEjecucion, "Fechas de ejecución"));
        }


        private void DefinirMf(string idMenu, List<string> opciones)
        {
            DescriptorDeEdicion<AsignacionDePtrDto>.IncluirMfIndividual(opciones, "<hr>");
            DescriptorDeEdicion<AsignacionDePtrDto>.IncluirMfIndividual(opciones, $"<li id='{idMenu}.plan' accion-menu='{eventosDeMf.Ptr_DarPorRealizadasSegunPlan}' {AtributosHtml.Mf(enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Gestor, permiteMultiSeleccion: true)}>Aplicar fecha de planificación</li>");
            DescriptorDeEdicion<AsignacionDePtrDto>.IncluirMfIndividual(opciones, $"<li id='{idMenu}.hoy' accion-menu='{eventosDeMf.Ptr_DarPorRealizadasHoy}' {AtributosHtml.Mf(enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Gestor, permiteMultiSeleccion: true)}>Dar por realizadas hoy</li>");
            DescriptorDeEdicion<AsignacionDePtrDto>.IncluirMfIndividual(opciones, $"<li id='{idMenu}.solfeceje' accion-menu='{eventosDeMf.Ptr_SolicitarFechaDeEjecucion}' {AtributosHtml.Mf(enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Gestor, permiteMultiSeleccion: true)}>Aignar datos de ejecución</li>");
        }

        private void IncluirFiltrosDeAsignaciones()
        {

            var modal_2 = new ModalDeFiltrado<AsignacionDePtrDto>(Mnt.Filtro, "filtroDeFechas", "Filtros de fechas");
            Mnt.Filtro.Modales.Add(modal_2);
            FiltrosPorFechaDePlanificación(modal_2);

            var modal_1 = new ModalDeFiltrado<AsignacionDePtrDto>(Mnt.Filtro, "filtroDeAsignaciones", "Elementos relacionados");
            Mnt.Filtro.Modales.Add(modal_1);
            FiltroPorParteTr(modal_1);
            FiltroPorCliente(modal_1);
            FiltroPorUnitario(modal_1);
            FiltroPorContrato(modal_1);
            FiltroPorPresupuesto(modal_1);
        }

        private void FiltrosPorFechaDePlanificación(ModalDeFiltrado<AsignacionDePtrDto> modal)
        {
            var fechasPlf = new FiltroEntreFechas<AsignacionDePtrDto>(modal,
                    etiqueta: "Inicio planificado",
                    propiedad: nameof(AsignacionDePtrDto.PlfDeInicio),
                    ayuda: "inicio planificado entre fechas");
            modal.ControlesDeFiltrado.Add(fechasPlf);

            var fechasIni = new FiltroEntreFechas<AsignacionDePtrDto>(modal,
                    etiqueta: "Inicio ejecución",
                    propiedad: nameof(AsignacionDePtrDto.Iniciada),
                    ayuda: "inicio pde ejecución");
            modal.ControlesDeFiltrado.Add(fechasIni);

            var checkDeMostrarFechasPlan = new CheckDeMostrarColumna<AsignacionDePtrDto>(modal,
                 etiqueta: "Mostra fechas plan",
                 ayuda: "Muestra la fecha del plan de asignación",
                 valorInicial: true,
                 columna: nameof(AsignacionDePtrDto.PlfDeInicio),
                 columnas: new List<string> { nameof(AsignacionDePtrDto.PlfDeInicio), nameof(AsignacionDePtrDto.PlfDeFin) });
            modal.ControlesDeFiltrado.Add(checkDeMostrarFechasPlan);

            var checkDeMostrarFechasEjecucion = new CheckDeMostrarColumna<AsignacionDePtrDto>(modal,
                 etiqueta: "Mostra fechas ejecución",
                 ayuda: "Muestra la fecha de ejecución",
                 valorInicial: false,
                 columna: nameof(AsignacionDePtrDto.Iniciada),
                 columnas: new List<string> { nameof(AsignacionDePtrDto.Iniciada), nameof(AsignacionDePtrDto.Finalizada) });
            modal.ControlesDeFiltrado.Add(checkDeMostrarFechasEjecucion);

            modal.ControlesDeFiltrado.Add(new CheckFiltro<AsignacionDePtrDto>(modal,
                etiqueta: "Partes no iniciados",
                filtrarPor: ltrDeUnParteTr.MostrarPtrsAsignadosPendientes,
                ayuda: "Muestra los partes asignados que están pendientes de ejecución",
                valorInicial: true,
                filtrarPorFalse: true));

            modal.ControlesDeFiltrado.Add(new CheckFiltro<AsignacionDePtrDto>(modal,
                etiqueta: "Partes ejecutados",
                filtrarPor: ltrDeUnParteTr.MostrarPtrsAsignadosEjecutados,
                ayuda: "Muestra los partes asignados que están ya ejecutados",
                valorInicial: false,
                filtrarPorFalse: true));
        }

        private void FiltroPorCliente(ModalDeFiltrado<AsignacionDePtrDto> modal)
        {
            modal.ControlesDeFiltrado.Add(new ListasDinamicas<AsignacionDePtrDto>(modal,
                    etiqueta: enumNegocio.Cliente.Singular(),
                    filtrarPor: ltrDeUnaAsignacion.IdCliente,
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
            { LongitudMinimaParaBuscar = 1 });
        }

        private void FiltroPorParteTr(ModalDeFiltrado<AsignacionDePtrDto> modal)
        {
            modal.ControlesDeFiltrado.Add(new ListasDinamicas<AsignacionDePtrDto>(modal,
            etiqueta: enumNegocio.ParteDeTrabajo.Singular(),
            filtrarPor: ltrDeUnaAsignacion.IdParteTr,
            ayuda: $"seleccione el {enumNegocio.ParteDeTrabajo.Singular(true)}",
            seleccionarDe: nameof(ParteTrDto),
            buscarPor: nameof(ParteTrDto.Expresion),
            mostrarExpresion: nameof(ParteTrDto.Expresion),
            criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
            posicion: new Posicion(1, 0),
            controlador: nameof(PartesTrController),
            navegarA: nameof(PartesTrController.CrudPartesDeTrabajo),
            restringirPor: "",
            alSeleccionarBlanquearControl: "")
            { LongitudMinimaParaBuscar = 1 });
        }
        private void FiltroPorUnitario(ModalDeFiltrado<AsignacionDePtrDto> modal)
        {
            modal.ControlesDeFiltrado.Add(new ListasDinamicas<AsignacionDePtrDto>(modal,
                etiqueta: enumNegocio.Unitario.Singular(),
                filtrarPor: ltrDeUnaAsignacion.IdUnitario,
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


        private void FiltroPorContrato(ModalDeFiltrado<AsignacionDePtrDto> modal)
        {
            var lista = new ListasDinamicas<AsignacionDePtrDto>(modal,
                   etiqueta: enumNegocio.Contrato.Singular(),
                   filtrarPor: ltrDeUnaAsignacion.IdContrato,
                   ayuda: $"seleccione un {enumNegocio.Contrato.Singular(true)}",
                   seleccionarDe: nameof(ContratoDto),
                   buscarPor: ltrDeUnContrato.SelectorParaUnaAsignacionDeParte,
                   mostrarExpresion: nameof(ContratoDto.Expresion),
                   criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                   posicion: new Posicion(1, 0),
                   controlador: nameof(ContratosController),
                   navegarA: nameof(ContratosController.CrudContratos),
                   restringirPor: "",
                   alSeleccionarBlanquearControl: "",
                   trasMapear: $"javascript:{nameof(enumNameSpaceTs.Venta)}.{nameof(enumFunctionTs.AsiPtr_Tras_Mapear_Filtro_IdContrato)}({nameof(enumParamTs.lista)})")
            {
                LongitudMinimaParaBuscar = 1,
                ParametrosParaNavegar = nameof(ltrParametrosEp.Clase) + "=" + nameof(enumClaseDeContrato.Venta)
            };

            var opciones = new Dictionary<string, string> {
                      { $"{ltrParametrosNeg.MostrarTodos}", "Con o sin contratos" }
                    , { $"{ltrParametrosNeg.ConRelacion}", "Con contratos" }
                    , { $"{ltrParametrosNeg.SinRelacion}", "Sin contratos" } };

            modal.ControlesDeFiltrado.Add(new FiltroDeRelacion<AsignacionDePtrDto>(modal,
                lista: lista,
                opciones: opciones,
                propiedad: ltrDeUnaAsignacion.FiltroPorConOSinContrato,
                filtrarPor: ltrDeUnaAsignacion.FiltroPorConOSinContrato,
                ayuda: $"Filtros de {enumNegocio.Contrato.Plural(true)}"
                ));
        }


        private void FiltroPorPresupuesto(ModalDeFiltrado<AsignacionDePtrDto> modal)
        {
            var lista = new ListasDinamicas<AsignacionDePtrDto>(modal,
                   etiqueta: enumNegocio.Presupuesto.Singular(),
                   filtrarPor: ltrDeUnaAsignacion.IdPresupuesto,
                   ayuda: $"seleccione un {enumNegocio.Contrato.Singular(true)}",
                   seleccionarDe: nameof(ContratoDto),
                   buscarPor: ltrDeUnPresupuesto.PptDeUnaAsignacionPtr,
                   mostrarExpresion: nameof(ContratoDto.Expresion),
                   criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                   posicion: new Posicion(1, 0),
                   controlador: nameof(PresupuestosController),
                   navegarA: nameof(PresupuestosController.CrudPresupuestos),
                   restringirPor: "",
                   alSeleccionarBlanquearControl: "",
                   trasMapear: $"javascript:{nameof(enumNameSpaceTs.Venta)}.{nameof(enumFunctionTs.AsiPtr_Tras_Mapear_Filtro_IdPresupuesto)}({nameof(enumParamTs.lista)})")
            {
                LongitudMinimaParaBuscar = 1,
                ParametrosParaNavegar = nameof(ltrParametrosEp.Clase) + "=" + nameof(enumClaseDeContrato.Venta)
            };

            var opciones = new Dictionary<string, string> {
                      { $"{ltrParametrosNeg.MostrarTodos}", "Con o sin presupuestos" }
                    , { $"{ltrParametrosNeg.ConRelacion}", "Con presupuestos" }
                    , { $"{ltrParametrosNeg.SinRelacion}", "Sin presupuestos" } };

            modal.ControlesDeFiltrado.Add(new FiltroDeRelacion<AsignacionDePtrDto>(modal,
                lista: lista,
                opciones: opciones,
                propiedad: ltrDeUnaAsignacion.FiltroPorConOSinPresupuesto,
                filtrarPor: ltrDeUnaAsignacion.FiltroPorConOSinPresupuesto,
                ayuda: $"Filtros de {enumNegocio.Presupuesto.Plural(true)}"
                ));
        }


        private void DescriptorDeLineasDeUnPtr()
        {
            var expansor = new DescriptorDeExpansor(Editor, $"{Editor.Id}-lineasdeunptr", "Detalle", true, "Lineas del parte de trabajo");
            expansor.EsDetalle = true;
            expansor.IdNegocio = enumNegocio.ParteDeTrabajo.IdNegocio();
            Editor.Expanes.Insert(0, expansor);

            var columnas = new DescriptorDeColumnas("lineasDeUnPtr");
            columnas.Add(titulo: "Orden", propiedad: nameof(LineaDeUnPtrDto.Orden), alineacion: enumAliniacion.derecha, mostrar: true, tamano: 100);
            columnas.Add(titulo: "Concepto", propiedad: nameof(LineaDeUnPtrDto.Concepto), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnas.Add(titulo: "Cantidad", propiedad: nameof(LineaDeUnPtrDto.Cantidad), alineacion: enumAliniacion.derecha, mostrar: true, tamano: 150);
            columnas.Add(titulo: "Unidad", propiedad: nameof(LineaDeUnPtrDto.Unidad), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnas.Add(titulo: "P. de venta", propiedad: nameof(LineaDeUnPtrDto.Precio), alineacion: enumAliniacion.derecha, mostrar: true, tamano: 150);
            columnas.Add(titulo: "Descuento", propiedad: nameof(LineaDeUnPtrDto.Descuento), alineacion: enumAliniacion.derecha, mostrar: true, tamano: 150);
            columnas.Add(titulo: "Iva", propiedad: nameof(LineaDeUnPtrDto.Iva), alineacion: enumAliniacion.derecha, mostrar: true, tamano: 150);
            columnas.Add(titulo: "Importe", propiedad: nameof(LineaDeUnPtrDto.ImporteDeLinea), alineacion: enumAliniacion.derecha, mostrar: true, tamano: 150);
            columnas.Add(titulo: "IdElemento", propiedad: nameof(LineaDeUnPtrDto.IdElemento), alineacion: enumAliniacion.derecha, mostrar: false);
            columnas.Add(titulo: "Id", propiedad: nameof(LineaDeUnPtrDto.Id), alineacion: enumAliniacion.derecha, mostrar: false);

            var orden = $"{nameof(LineaDeUnPtrDto.Orden)}:{enumModoOrdenacion.ascendente.Render()}";

            var parametros = new Dictionary<string, object> {
                   { nameof(GridDeRelacion.Controlador), typeof(LineasDeUnPtrController) }
                 , { nameof(GridDeRelacion.AccionDeConsulta), nameof(LineasDeUnPtrController.epLeerElementos)}
                 , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(LineaDeUnPtrDto.IdElemento) }
                 , { nameof(GridDeRelacion.OrdenarPor), orden }
                 , { nameof(GridDeRelacion.OcultarSiVacio), false}
                };
            var gridDeRelacion = new GridDeRelacion(expansor, columnas, parametros);
            gridDeRelacion.PermitirBorrar = false;
            gridDeRelacion.CampoRestrictor = nameof(LineaDeUnPtrDto.IdElemento);

            //var modalDeCreacion = expansor.DescriptorDeCrearRelaciones(Editor.Crud.Contexto, typeof(LineaDeUnPtrDto), typeof(LineasDeUnPtrController), nameof(LineaDeUnPtrDto.IdElemento), "Añadir línea");
            //modalDeCreacion.AccionTrasAbrirModal = $"javascript: {enumNameSpaceTs.Venta}.{enumFunctionTs.Ptr_InicializarModalParaCrearLineas}()";

            var modalDeEdicion = expansor.DescriptorDeEditarRelaciones(Editor.Crud.Contexto, typeof(LineaDeUnPtrDto), typeof(LineasDeUnPtrController), "Consultar línea", soloConsulta: true);
            //modalDeEdicion.AccionTrasAbrirModal = $"javascript: {enumNameSpaceTs.Venta}.{enumFunctionTs.Ptr_InicializarModalParaEditarLineas}();";

        }

        public override string RenderControl()
        {
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
            if (ServicioDeCaches.Obtener(CacheDe.RenderCrud).ContainsKey(indice))
				 return (string)ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice];
            
            var render = base.RenderControl();
            render = render +
                   $@"<script src=¨../../js/{RutaBase}/ApiDePartesTr.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script src=¨../../js/{RutaBase}/AsignacionesPtr.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{      
                           {RutaBase}.CrearCrudDeAsignacionesPtr('{Mnt.IdHtml}', '{Creador.IdHtml}', '{Editor.IdHtml}', '{Borrado.IdHtml}') 
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
