using GestorDeElementos;
using GestorDeElementos.Extensores;
using ModeloDeDto;
using ModeloDeDto.Entorno;
using ModeloDeDto.Expediente;
using ModeloDeDto.Presupuesto;
using ModeloDeDto.RegistroEs;
using ModeloDeDto.Tarea;
using ModeloDeDto.Terceros;
using ModeloDeDto.Ventas;
using MVCSistemaDeElementos.Controllers;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.Presupuesto;
using ServicioDeDatos.RegistroEs;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Tarea;
using ServicioDeDatos.Ventas;
using System;
using System.Collections.Generic;
using Utilidades;
using UtilidadesParaIu;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeTareas : DescriptorDeCrud<TareaDto>
    {
        public DescriptorDeTareas(ContextoSe contexto, string renderCache) : base(contexto, renderCache)
        {
        }
        public DescriptorDeTareas(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto
               , nameof(TareasController)
               , nameof(TareasController.CrudTareas)
               , modo
               , rutaBase: enumNameSpaceTs.Administracion)
        {
            var modal = new ModalDeFiltrado<TareaDto>(Mnt.Filtro, "filtroDeDatosDeTareas", "Filtros de tareas");
            Mnt.Filtro.Modales.Add(modal);
            FiltroPorSolicitante(modal);
            FiltroPorFacturaEmt(modal);
            FiltroPorAsignacion(modal);
            IncluirFiltroDeAmpliaciones(modal);

            var modalDeRelacion = new ModalDeFiltrado<TareaDto>(Mnt.Filtro, "filtrosDeRelacionesConTareas", "Relaciones con las tareas");
            Mnt.Filtro.Modales.Add(modalDeRelacion);
            FiltroPorExpediente(modalDeRelacion);
            FiltroPorPresupuesto(modalDeRelacion);

            foreach (enumEtapasDeTareas etapa in Enum.GetValues(typeof(enumEtapasDeTareas)))
            {
                Mnt.Filtro.EtapasDeUnProceso.Opciones[etapa.ToString()] = "Etapa de: " + etapa.Nombre(minusculas: false);
            }
            Historial.OpcionesDeFiltrado.Remove(ltrSucesosExcluir.tareas);

            DescriptorDePlanificacion();
            DescriptorDeSpanDeRegistroEs();

            modalesParaPedirDatos.Add(new ModalParaPedirDatos(this, typeof(CopiarTareaDto), eventosDeMf.Tar_CopiarTarea, "Seleccionar tarea a copiar"));
            Mnt.IncluirMfContextual($"<li id='{menuContextual}.{eventosDeMf.Tar_CopiarTarea}' accion-menu='{eventosDeMf.Tar_CopiarTarea}' {AtributosHtml.Mf(enumCssOpcionMenu.DeVista, enumModoDeAccesoDeDatos.Gestor, false)}>Copiar tarea</li>");

            modalesParaPedirDatos.Add(new ModalDeTotales(this, typeof(TotalesDeTareas), eventosDeMf.Totalizador_Mostrar, $"Mostrar los totales de la selección"));
            Mnt.IncluirMfContextual($"<li id='{menuContextual}.{eventosDeMf.Totalizador_Mostrar}' accion-menu='{eventosDeMf.Totalizador_Mostrar}' {AtributosHtml.Mf(enumCssOpcionMenu.DeVista, enumModoDeAccesoDeDatos.Consultor, false)}>Mostrar totales</li>");


        }

        private void FiltroPorSolicitante(ModalDeFiltrado<TareaDto> modal)
        {
            modal.ControlesDeFiltrado.Add(new ListasDinamicas<InterlocutorDto>(modal,
                etiqueta: "Solicitante",
                filtrarPor: ltrDeUnaTarea.IdSolicitante,
                ayuda: "seleccione el solicitante",
                seleccionarDe: nameof(InterlocutorDto),
                buscarPor: nameof(InterlocutorDto.Expresion),
                mostrarExpresion: nameof(InterlocutorDto.Expresion),
                criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                posicion: new Posicion(1, 0),
                controlador: nameof(InterlocutoresController),
                navegarA: nameof(InterlocutoresController.CrudInterlocutores),
                restringirPor: "",
                alSeleccionarBlanquearControl: "")
            { LongitudMinimaParaBuscar = 1 });
        }

        private void FiltroPorFacturaEmt(ModalDeFiltrado<TareaDto> modal)
        {
            var lista = new ListasDinamicas<FacturaEmtDto>(modal,
                etiqueta: enumNegocio.FacturaEmitida.Singular(),
                filtrarPor: ltrDeUnaTarea.IdFacturaEmt,
                ayuda: "seleccione la factura (nº, referencia o nombre)",
                seleccionarDe: nameof(FacturaEmtDto),
                buscarPor: nameof(ltrDeUnaFacturaEmt.FacturaDeUnaTarea),
                mostrarExpresion: nameof(FacturaEmtDto.Expresion),
                criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                posicion: null,
                controlador: nameof(FacturasEmtController),
                navegarA: nameof(FacturasEmtController.CrudFacturasEmt),
                restringirPor: "",
                alSeleccionarBlanquearControl: "")
            {
                LongitudMinimaParaBuscar = 1
            };

            var opciones = new Dictionary<string, string> {
                      { $"{ltrParametrosNeg.MostrarTodos}", "con o sin facturas" }
                    , { $"{ltrParametrosNeg.ConRelacion}", "facturadas" }
                    , { $"{ltrParametrosNeg.SinRelacion}", "sin facturar" } };

            modal.ControlesDeFiltrado.Add(new FiltroDeRelacion<FacturaEmtDto>(modal,
                lista: lista,
                opciones: opciones,
                propiedad: ltrDeUnaTarea.Facturada,
                filtrarPor: ltrDeUnaTarea.Facturada,
                ayuda: "Filtros de facturación"
                ));
        }
        private static void FiltroPorAsignacion(ModalDeFiltrado<TareaDto> modal)
        {
            var lista = new ListasDinamicas<TareaDto>(modal,
                etiqueta: enumNegocio.Usuario.Singular(),
                filtrarPor: ltrDeUnaTarea.IdResponsable,
                ayuda: "seleccione el responsable",
                seleccionarDe: nameof(UsuarioDto),
                buscarPor: nameof(ltrDeUnUsuario.UsuariosConTareas),
                mostrarExpresion: nameof(UsuarioDto.NombreCompleto),
                criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                posicion: null,
                controlador: nameof(UsuariosController),
                navegarA: nameof(UsuariosController.CrudUsuario),
                restringirPor: "",
                alSeleccionarBlanquearControl: "")
            {
                LongitudMinimaParaBuscar = 1
            };

            var opciones = new Dictionary<string, string> {
                      { $"{ltrParametrosNeg.MostrarTodos}", "con o sin responsable" }
                    , { $"{ltrParametrosNeg.ConRelacion}", "asignadas" }
                    , { $"{ltrParametrosNeg.SinRelacion}", "no asignadas" } };

            modal.ControlesDeFiltrado.Add(new FiltroDeRelacion<TareaDto>(modal,
                lista: lista,
                opciones: opciones,
                propiedad: ltrDeUnaTarea.Asignacion,
                filtrarPor: ltrDeUnaTarea.Asignacion,
                ayuda: "Filtros d asignaciones"
                ));
        }
        private static void FiltroPorExpediente(ModalDeFiltrado<TareaDto> modal)
        {
            var lista = new ListasDinamicas<TareaDto>(modal,
                etiqueta: enumNegocio.Expediente.Singular(),
                filtrarPor: ltrDeUnaTarea.IdExpediente,
                ayuda: "seleccione el expediente",
                seleccionarDe: nameof(ExpedienteDto),
                buscarPor: nameof(ltrDeUnExpediente.ExpedientesConTareas),
                mostrarExpresion: nameof(ExpedienteDto.Expresion),
                criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                posicion: null,
                controlador: nameof(ExpedientesController),
                navegarA: nameof(ExpedientesController.CrudExpedientes),
                restringirPor: "",
                alSeleccionarBlanquearControl: "")
            {
                LongitudMinimaParaBuscar = 1,
                SoloEnAlta = true
            };

            var opciones = new Dictionary<string, string> {
                      { $"{ltrParametrosNeg.MostrarTodos}", "con o sin expediente" }
                    , { $"{ltrParametrosNeg.ConRelacion}", "incluidas en un expediente" }
                    , { $"{ltrParametrosNeg.SinRelacion}", "no incluidas en un expediente" } };

            modal.ControlesDeFiltrado.Add(new FiltroDeRelacion<TareaDto>(modal,
                lista: lista,
                opciones: opciones,
                propiedad: ltrDeUnaTarea.RelacionadaConExpediente,
                filtrarPor: ltrDeUnaTarea.RelacionadaConExpediente,
                ayuda: "Filtros de expedientes"
                ));
        }

        private static void FiltroPorPresupuesto(ModalDeFiltrado<TareaDto> modal)
        {
            var lista = new ListasDinamicas<TareaDto>(modal,
                etiqueta: enumNegocio.Presupuesto.Singular(),
                filtrarPor: ltrDeUnaTarea.IdPresupuesto,
                ayuda: "seleccione el presupuesto",
                seleccionarDe: nameof(PresupuestoDto),
                buscarPor: nameof(ltrDeUnPresupuesto.PptsConTareas),
                mostrarExpresion: nameof(PresupuestoDto.Expresion),
                criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                posicion: new Posicion(1, 0),
                controlador: nameof(PresupuestosController),
                navegarA: nameof(PresupuestosController.CrudPresupuestos),
                restringirPor: "",
                alSeleccionarBlanquearControl: "")
            {
                LongitudMinimaParaBuscar = 1,
                SoloEnAlta = true
            };

            var opciones = new Dictionary<string, string> {
                      { $"{ltrParametrosNeg.MostrarTodos}", "con o sin ppt" }
                    , { $"{ltrParametrosNeg.ConRelacion}", "incluidas en un ppt" }
                    , { $"{ltrParametrosNeg.SinRelacion}", "no incluidas en un ppt" } };

            modal.ControlesDeFiltrado.Add(new FiltroDeRelacion<TareaDto>(modal,
                lista: lista,
                opciones: opciones,
                propiedad: ltrDeUnaTarea.RelacionadaConPpt,
                filtrarPor: ltrDeUnaTarea.RelacionadaConPpt,
                ayuda: "Filtros de presupuesto"
                ));
        }

        private void IncluirFiltroDeAmpliaciones(ModalDeFiltrado<TareaDto> modal)
        {

            var fechasPlfIni = new FiltroEntreFechas<PlfDeTareaDto>(modal,
                    etiqueta: "Iniciar El",
                    propiedad: ltrPlfDeTarea.FiltroPorPlfDeInicio,
                    ayuda: "tareas a iniciar entre fechas");
            modal.ControlesDeFiltrado.Add(fechasPlfIni);
            fechasPlfIni.ModificarId(fechasPlfIni.Id + "-i");
            var fechasPlfFin = new FiltroEntreFechas<PlfDeTareaDto>(modal,
                    etiqueta: "Terminar antes del",
                    propiedad: ltrPlfDeTarea.FiltroPorPlfDeFin,
                    ayuda: "tareas a finalizar entre fechas");
            fechasPlfIni.ModificarId(fechasPlfIni.Id + "-f");
            modal.ControlesDeFiltrado.Add(fechasPlfFin);

            //var durabilidad = new CheckDeMostrarColumna<TareaDto>(modal,
            //    etiqueta: "Mostrar datos de planificación",
            //    ayuda: "muestra información de la durabilidad y la ejecución",
            //    valorInicial: false,
            //    columna: "plf-de-durabilidad",
            //    columnas: new List<string> { nameof(TareaDto.Planificada), nameof(TareaDto.Ejecutada), nameof(TareaDto.Durabilidad) });
            //modal.ControlesDeFiltrado.Add(durabilidad);
        }

        private void DescriptorDePlanificacion()
        {
            var plfDeTarea = new AmpliacionDeEdicion(Editor, Ampliaciones.Tareas.planificaciones, "Planificación de la tarea", new Dimension(2, 2), ayuda: "datos de la planificación de la tarea");
            plfDeTarea.Dto = typeof(PlfDeTareaDto);
            plfDeTarea.Controlador = nameof(PlfDeTareasController);
            Editor.Ampliaciones.Add(plfDeTarea);
        }

        private void DescriptorDeSpanDeRegistroEs()
        {
            var expansor = new DescriptorDeExpansor(Editor, $"{Editor.Id}-registrosEs", "RegistrosEs", true, "Registros de entrada asociados");
            Editor.Expanes.Insert(0, expansor);

            //Definimos el grid de detalles del cuerpo
            var columnas = new DescriptorDeColumnas("registrosEs");
            columnas.Add(titulo: "Registro", propiedad: nameof(RegistroEsDto.Expresion), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnas.Add(titulo: "Id", propiedad: nameof(RegistroEsDtm.Id), alineacion: enumAliniacion.derecha, mostrar: false);
            var parametros = new Dictionary<string, object> {
                   { nameof(GridDeRelacion.Controlador), typeof(RegistrosEsController) }
                 , { nameof(GridDeRelacion.AccionDeConsulta), nameof(BaseController<TareaDto>.epLeerVinculosCon)}
                 , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(VinculoDtm.idElemento1) }
                 , { nameof(ltrParametrosEp.idVinculado) , NegociosDeSe.IdNegocio(enumNegocio.Registro) }
                 , { nameof(GridDeRelacion.PaginaDondeNavegarAlEditar) , $"{nameof(RegistrosEsController)}/{nameof(RegistrosEsController.CrudRegistrosEs)}"}
                 , { nameof(GridDeRelacion.OcultarSiVacio), true}
                };
            var gridDeRelacion = new GridDeRelacion(expansor, columnas, parametros);
            gridDeRelacion.PermitirBorrar = true;

            expansor.DescriptorDeEditarRelaciones(Editor.Crud.Contexto, typeof(RegistroEsDto), typeof(RegistrosEsController), "Editar registro", false);
        }

        public override string RenderControl()
        {
            if (!_renderCache.IsNullOrEmpty())
                return _renderCache;


            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
            var render = base.RenderControl();

            render = render +
                   $@"<script src=¨../../js/{RutaBase}/ApiDeAdministracion.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script src=¨../../js/{RutaBase}/Tareas.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{      
                           {RutaBase}.CrearCrudDeTareas('{Mnt.IdHtml}', '{Creador.IdHtml}', '{Editor.IdHtml}', '{Borrado.IdHtml}') 
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
