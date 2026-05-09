using ServicioDeDatos;
using MVCSistemaDeElementos.Controllers;
using UtilidadesParaIu;
using Utilidades;
using ModeloDeDto.Expediente;
using ModeloDeDto;
using System.Collections.Generic;
using GestorDeElementos;
using ServicioDeDatos.Elemento;
using ModeloDeDto.RegistroEs;
using ServicioDeDatos.RegistroEs;
using ModeloDeDto.Tarea;
using ServicioDeDatos.Seguridad;
using ModeloDeDto.Presupuesto;
using ModeloDeDto.Entorno;
using ModeloDeDto.Terceros;
using GestorDeElementos.Extensores;
using ModeloDeDto.Juridico;
using ServicioDeDatos.Juridico;
using ServicioDeDatos.Presupuesto;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.Tarea;
using System.Linq;
using ModeloDeDto.Gastos;
using ServicioDeDatos.Gastos;
using ModeloDeDto.Ventas;
using ServicioDeDatos.Ventas;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeExpedientes : DescriptorDeCrud<ExpedienteDto>
    {
        public enumClaseDeExpediente? _clase { get; set; }

        public DescriptorDeExpedientes(ContextoSe contexto, string renderCache, string clase) 
        : base(contexto, renderCache)
        {
            if (clase != ltrDeUnExpediente.ExpedieteNoJuridico)
                _clase = ApiDeEnsamblados.ToEnumerado<enumClaseDeExpediente>(clase);
        }

        public DescriptorDeExpedientes(ContextoSe contexto, ModoDescriptor modo, string clase)
        : base(contexto
               , nameof(ExpedientesController)
               , nameof(ExpedientesController.CrudExpedientes)
               , modo
               , rutaBase: enumNameSpaceTs.Administracion)
        {
            if (clase == enumClaseDeExpediente.juridico.ToString())
            {
                _clase = ApiDeEnsamblados.ToEnumerado<enumClaseDeExpediente>(clase);
                Mnt.Etiqueta = "Procedimientos judiciales";
                Creador.Etiqueta = "Procedimiento";
                Editor.Etiqueta = "Procedimiento";
            }


            if (_clase != enumClaseDeExpediente.juridico)
            {
                IncluirFiltrosDatosExpedientes();
                IncluirFiltrosRelacionados();
            }


            modalesParaPedirDatos.Add(new ModalDeTotales(this, typeof(TotalesDeExpedientes), eventosDeMf.Totalizador_Mostrar, $"Mostrar los totales de la selección"));
            Mnt.IncluirMfContextual($"<li id='{menuContextual}.{eventosDeMf.Totalizador_Mostrar}' accion-menu='{eventosDeMf.Totalizador_Mostrar}' {AtributosHtml.Mf(enumCssOpcionMenu.DeVista, enumModoDeAccesoDeDatos.Consultor, false)}>Mostrar totales</li>");

            Mnt.Filtro.EtapasDeUnProceso.Opciones[enumEtapasDeExpedientes.EXP_Etapa_Asociar_Tareas.ToString()] = "Etapa de: " + enumEtapasDeExpedientes.EXP_Etapa_Asociar_Tareas.Nombre(minusculas: false);
            Mnt.Filtro.EtapasDeUnProceso.Opciones[enumEtapasDeExpedientes.EXP_Etapa_Asociar_Presupuestos.ToString()] = "Etapa de: " + enumEtapasDeExpedientes.EXP_Etapa_Asociar_Presupuestos.Nombre(minusculas: false);
            Mnt.Filtro.EtapasDeUnProceso.Opciones[enumEtapasDeExpedientes.EXP_Etapa_Ejecucion.ToString()] = "Etapa de: " + enumEtapasDeExpedientes.EXP_Etapa_Ejecucion.Nombre(minusculas: false);

            if (_clase != enumClaseDeExpediente.juridico)
            {
                Mnt.Filtro.EtapasDeUnProceso.Opciones[enumEtapasDeExpedientes.EXP_Etapa_Asociar_SC_Venta.ToString()] = "Etapa de: " + enumEtapasDeExpedientes.EXP_Etapa_Asociar_SC_Venta.Nombre(minusculas: false);
                Mnt.Filtro.EtapasDeUnProceso.Opciones[enumEtapasDeExpedientes.EXP_Etapa_Asociar_SC_Compra.ToString()] = "Etapa de: " + enumEtapasDeExpedientes.EXP_Etapa_Asociar_SC_Compra.Nombre(minusculas: false);
            }

            Mnt.IncluirMfDeRelacion($"<li id='{menuDeRelaciones}.{enumNegocio.Tarea}' accion-menu='{eventosDeMf.Exp_IrATareas}' {AtributosHtml.Mf(enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Consultor, false)}>Tareas vinculadas</li>");

            if (_clase == enumClaseDeExpediente.juridico) DescriptorDeDatosJuridicos();
            DescriptorDeSpanDePpts();
            if (_clase != enumClaseDeExpediente.juridico)
            {
                DescriptorDeSpanDeContratos(enumClaseDeContrato.Venta);
                DescriptorDeSpanDeContratos(enumClaseDeContrato.Compra);
            }
            DescriptorDeSpanDeRegistroEs();
            DescriptorDeSpanDeFacturasRec();
            DescriptorDeSpanDeFacturasEmt();
            DescriptorDeApuntes();

            DefinirMf(menuEdicion, Editor.OpcionesMf);


            Mnt.IncluirMfDeRelacion($"<li id='{menuDeRelaciones}.{eventosDeMf.Exp_ImputarFacturas}' accion-menu='{eventosDeMf.Exp_ImputarFacturas}' {AtributosHtml.Mf(enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Interventor, false)}>Imputar facturas</li>");
            var modalDeFacturas2 = new ModalParaImputar<ExpedienteDto, FacturaRecDto>(mantenimiento: Mnt
                   , id: $"{this.Id}-{eventosDeMf.Exp_ImputarFacturas}"
                   , tituloModal: "Seleccione las facturas a imputar"
                   , crudModal: new DescriptorDeFacturasRec(contexto)
                   , propiedadRestrictora: nameof(FacturaRecDto.IdExpediente)
                   , filtrarPor: ltrDeUnaFacturaRec.FacturasImputablesEnUnExpediente
                   , faltaRestrictor: "Debe seleccionar el expediente al que imputar las facturas");
            modalDeFacturas2.Criterio = enumCriteriosDeFiltrado.diferente;
            modalesParaPedirDatos.Add(modalDeFacturas2);

            Mnt.IncluirMfDeRelacion($"<li id='{menuDeRelaciones}.{eventosDeMf.Exp_IrAFacturasRec}' accion-menu='{eventosDeMf.Exp_IrAFacturasRec}' {AtributosHtml.Mf(enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Consultor, false)}>Facturas recibidas</li>");


            var nuevoElemento = new KeyValuePair<string, string>(ltrSucesosExcluir.presupuestos, "Presupuestos");

            var index = Historial.OpcionesDeFiltrado.ToList().FindIndex(x => x.Key == ltrSucesosExcluir.nivel2hitos);
            Historial.OpcionesDeFiltrado = Historial.OpcionesDeFiltrado.Take(index + 1)
                .Concat(new[] { nuevoElemento })
                .Concat(Historial.OpcionesDeFiltrado.Skip(index + 1))
                .ToDictionary(x => x.Key, x => x.Value);
        }


        private void IncluirFiltrosRelacionados()
        {
            var modal = new ModalDeFiltrado<ExpedienteDto>(Mnt.Filtro, "filtrosDeRelaciones", "Elementos relacionados");
            Mnt.Filtro.Modales.Add(modal);
            FiltroRelacionadosConTareas(modal);
            FiltroRelacionadosConPpts(modal);
            FiltroRelacionadosConRegistroEs(modal);
        }

        private static void FiltroRelacionadosConPpts(ModalDeFiltrado<ExpedienteDto> modal)
        {
            var lista = new ListasDinamicas<ExpedienteDto>(modal,
                 etiqueta: enumNegocio.Presupuesto.Singular(),
                 filtrarPor: ltrDeUnExpediente.IdPresupuesto,
                 ayuda: "seleccione el presupuesto",
                 seleccionarDe: nameof(PresupuestoDto),
                 buscarPor: ltrDeUnPresupuesto.PptDeUnExpediente,
                 mostrarExpresion: nameof(PresupuestoDto.Expresion),
                 criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                 posicion: new Posicion(1, 0),
                 controlador: nameof(PresupuestosController),
                 navegarA: nameof(PresupuestosController.CrudPresupuestos),
                 restringirPor: "",
                 alSeleccionarBlanquearControl: "")
            { LongitudMinimaParaBuscar = 1 };

            var opciones = new Dictionary<string, string> {
                      { $"{ltrParametrosNeg.MostrarTodos}", "Con o sin presupuestos asociados" }
                    , { $"{ltrParametrosNeg.ConRelacion}", "Con presupuestos" }
                    , { $"{ltrParametrosNeg.SinRelacion}", "Sin presupuestos" } };

            modal.ControlesDeFiltrado.Add(new FiltroDeRelacion<ExpedienteDto>(modal,
                lista: lista,
                opciones: opciones,
                propiedad: ltrDeUnExpediente.PresupuestoHijo,
                filtrarPor: ltrDeUnExpediente.ExpedientesConPpts,
                ayuda: "Filtros de presupuestos"
                ));

            modal.ControlesDeFiltrado.Add(new FiltroDelTipoRelacionado<ExpedienteDto>(modal,
                lista: lista,
                opciones: opciones,
                propiedad: ltrDeUnExpediente.TiposDePptsAsociados,
                negocioVinculado: enumNegocio.Presupuesto,
                ayuda: "Filtros de tipos de presupuestos asociados"
                ));
        }
        private static void FiltroRelacionadosConTareas(ModalDeFiltrado<ExpedienteDto> modal)
        {
            var lista = new ListasDinamicas<ExpedienteDto>(modal,
                etiqueta: "Tarea",
                filtrarPor: ltrDeUnaTarea.IdTarea,
                ayuda: "seleccione la tarea",
                seleccionarDe: nameof(TareaDto),
                buscarPor: nameof(TareaDto.Expresion),
                mostrarExpresion: nameof(TareaDto.Expresion),
                criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                posicion: new Posicion(1, 0),
                controlador: nameof(TareasController),
                navegarA: nameof(TareasController.CrudTareas),
                restringirPor: "",
                alSeleccionarBlanquearControl: "")
            { LongitudMinimaParaBuscar = 1 };

            var opciones = new Dictionary<string, string> {
                      { $"{ltrParametrosNeg.MostrarTodos}", "Con o sin tareas relacionadas" }
                    , { $"{ltrParametrosNeg.ConRelacion}", "Con tareas" }
                    , { $"{ltrParametrosNeg.SinRelacion}", "Sin tareas" } };

            modal.ControlesDeFiltrado.Add(new FiltroDeRelacion<ExpedienteDto>(modal,
                lista: lista,
                opciones: opciones,
                propiedad: nameof(ltrDeUnaTarea.VinculosATareas),
                filtrarPor: nameof(ltrDeUnaTarea.VinculosATareas),
                ayuda: "Filtros de tareas"
                ));

            modal.ControlesDeFiltrado.Add(new FiltroDelTipoRelacionado<ExpedienteDto>(modal,
                lista: lista,
                opciones: opciones,
                propiedad: "tiposDeTareasRelacionados",
                negocioVinculado: enumNegocio.Tarea,
                ayuda: "Filtros de tipos de tareas relacionadas"
                ));
        }
        private static void FiltroRelacionadosConRegistroEs(ModalDeFiltrado<ExpedienteDto> modal)
        {
            var lista = new ListasDinamicas<ExpedienteDto>(modal,
                etiqueta: "Registro E/S",
                filtrarPor: ltrDeUnRegistroEs.IdRegistroEs,
                ayuda: "seleccione un registro de entrada o salida",
                seleccionarDe: nameof(RegistroEsDto),
                buscarPor: nameof(RegistroEsDto.Expresion),
                mostrarExpresion: nameof(RegistroEsDto.Expresion),
                criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                posicion: new Posicion(1, 0),
                controlador: nameof(RegistrosEsController),
                navegarA: nameof(RegistrosEsController.CrudRegistrosEs),
                restringirPor: "",
                alSeleccionarBlanquearControl: "")
            { LongitudMinimaParaBuscar = 1 };

            var opciones = new Dictionary<string, string> {
                      { $"{ltrParametrosNeg.MostrarTodos}", "Con o sin registros relacionados" }
                    , { $"{ltrParametrosNeg.ConRelacion}", "Con registros de E/S" }
                    , { $"{ltrParametrosNeg.SinRelacion}", "Sin registro de E/S" } };

            modal.ControlesDeFiltrado.Add(new FiltroDeRelacion<ExpedienteDto>(modal,
                lista: lista,
                opciones: opciones,
                propiedad: "RegistrosEsRelacionados",
                filtrarPor: nameof(ltrDeUnRegistroEs.MostraRegistrosEsRelacionados),
                ayuda: "Filtros de registros de entrada o salida"
                ));

            modal.ControlesDeFiltrado.Add(new FiltroDelTipoRelacionado<ExpedienteDto>(modal,
                lista: lista,
                opciones: opciones,
                propiedad: "tiposDeRegistrosRelacionados",
                negocioVinculado: enumNegocio.Registro,
                ayuda: "Filtros de tipos de registros relacionados"
                ));
        }

        private void IncluirFiltrosDatosExpedientes()
        {
            var modal = new ModalDeFiltrado<ExpedienteDto>(Mnt.Filtro, "filtrosDeCabecera", "Filtros de expedientes");
            Mnt.Filtro.Modales.Add(modal);

            modal.ControlesDeFiltrado.Add(new ListasDinamicas<ExpedienteDto>(modal,
                etiqueta: "Solicitante",
                filtrarPor: ltrDeUnExpediente.IdCliente,
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

            modal.ControlesDeFiltrado.Add(new ListasDinamicas<ExpedienteDto>(modal,
                 etiqueta: "Responsable",
                 filtrarPor: ltrDeUnExpediente.IdResponsable,
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
            { LongitudMinimaParaBuscar = 1 });

            modal.ControlesDeFiltrado.Add(new EditorFiltro<ExpedienteDto>(modal,
                "Datos contacto", ltrDeUnExpediente.DatosContacto,
                "contacto, teléfono o mail"
                , new Posicion(3, 0)));
        }

        private void DefinirMf(string idMenu, List<string> opciones)
        {
            DescriptorDeEdicion<ExpedienteDto>.IncluirMfIndividual(opciones, "<hr>");
            DescriptorDeEdicion<ExpedienteDto>.IncluirMfIndividual(opciones, $"<li id='{idMenu}.1' accion-menu='{eventosDeMf.Exp_VincularRegistroEntrada}' {AtributosHtml.Mf(enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Gestor, false)}>Asociar registro de E/S</li>");
            //DescriptorDeEdicion<ExpedienteDto>.IncluirMfIndividual(opciones, $"<li id='{idMenu}.3' accion-menu='{eventosDeMf.Interlocutores}' {AtributosHtml.Mf(enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Consultor, false)}>Gestionar interlocutores</li>");
        }

        private void DescriptorDeDatosJuridicos()
        {
            var datos = new AmpliacionDeEdicion(Editor, Ampliaciones.Expedientes.DatosJuridicos, "Datos Jurídicos", new Dimension(2, 2), ayuda: "Información de los datos jurídicos del expediente");
            datos.Dto = typeof(DatosJuridicosDto);
            datos.Controlador = nameof(DatosJuridicosController);
            Editor.Ampliaciones.Add(datos);
        }

        private void DescriptorDeSpanDePpts()
        {
            var expansor = new DescriptorDeExpansor(Editor, $"{Editor.Id}-ppts", "Presupuestos", true, "Presupuestos del expediente");
            Editor.Expanes.Insert(0, expansor);
            var columnas = new DescriptorDeColumnas("presupuestos");
            columnas.Add(titulo: "Presupuesto", propiedad: nameof(PresupuestoDto.Expresion), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnas.Add(titulo: nameof(PresupuestoDto.Tipo), propiedad: nameof(PresupuestoDto.Tipo), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnas.Add(titulo: nameof(PresupuestoDto.Estado), propiedad: nameof(PresupuestoDto.Estado), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnas.Add(titulo: "Total sin iva", propiedad: nameof(PresupuestoDto.TotalSinIva), alineacion: enumAliniacion.derecha, mostrar: true, formato: enumFormato.Moneda);
            columnas.Add(titulo: "Facturado", propiedad: nameof(PresupuestoDto.Facturado), alineacion: enumAliniacion.derecha, mostrar: true, formato: enumFormato.Moneda);
            columnas.Add(titulo: nameof(PresupuestoDto.Id), propiedad: nameof(PresupuestoDto.Id), alineacion: enumAliniacion.derecha, mostrar: false);
            var parametros = new Dictionary<string, object> {
               { nameof(GridDeRelacion.Controlador), typeof(PresupuestosController) },
               { nameof(GridDeRelacion.AccionDeConsulta), nameof(PresupuestosController.epLeerElementos)},
               { nameof(GridDeRelacion.PropiedadRestrictora), nameof(PresupuestoDto.idExpediente) },
               { nameof(GridDeRelacion.PaginaDondeNavegarAlEditar) , $"{nameof(PresupuestosController)}/{nameof(PresupuestosController.CrudPresupuestos)}?id={nameof(PresupuestoDto.Id)}"},
               { nameof(GridDeRelacion.OcultarSiVacio), false}
            };

            var gridDeRelacion = new GridDeRelacion(expansor, columnas, parametros);
            gridDeRelacion.PermitirBorrar = true;
            gridDeRelacion.PermitirEditar = false;


            var accion = new AccionDeGetionarDatosDependientes(
                    urlDelCrud: $@"/{nameof(PresupuestosController)}/{nameof(PresupuestosController.CrudPresupuestos)}?origen=dependencia"
                  , datosDependientes: nameof(PresupuestoDto)
                  , nombreDelMnt: DescriptorDeMantenimiento<PresupuestoDto>.NombreMnt
                  , propiedadQueRestringe: nameof(ExpedienteDto.Id)
                  , propiedadRestrictora: nameof(PresupuestoDto.idExpediente)
                  , "Crear o gestionar presupuestos");

            expansor.DescriptorParaVincular(Editor.Crud.Contexto, NegociosDeSe.IdNegocio(enumNegocio.Presupuesto), typeof(SelectorDePptDto), nameof(PresupuestosController), "Asociar presupuesto");
            expansor.DescriptorDeNavegadorRefParaCrear("Crear presupuesto", accion, enumParaQueNavegar.crear);
            //expansor.DescriptorDeEditarRelaciones(Editor.Crud.Contexto, typeof(PresupuestoDto), typeof(PresupuestosController), "Editar presupuesto", false);

            var modal = expansor.DescriptorDeCrearDetalles(Contexto, typeof(ValoracionDto), nameof(ExpedientesController), "Añadir valoración"
                , permisosNecesarios: enumModoDeAccesoDeDatos.Gestor
                , accionControlador: nameof(ExpedientesController.epCrearValoracion));
            modal.AccionTrasAbrirModal = $"javascript:{enumNameSpaceTs.Administracion}.{enumFunctionTs.Exp_TrasAbrirModalDeCrearValoracion}('{modal.IdHtml}')";
        }

        private void DescriptorDeSpanDeContratos(enumClaseDeContrato claseContrato)
        {
            var crud = claseContrato == enumClaseDeContrato.Compra ? nameof(ContratosController.CrudContratosDeCompras) : nameof(ContratosController.CrudContratosDeVenta);
            var postId = claseContrato == enumClaseDeContrato.Compra ? "contratosCompra" : "contratosVenta";
            var expansor = new DescriptorDeExpansor(Editor, $"{Editor.Id}-{postId}", $"Contratos de {(claseContrato == enumClaseDeContrato.Compra ? "compra" : "venta")}", true, "Contratos de la solicitud de contratos");
            Editor.Expanes.Insert(0, expansor);
            var columnas = new DescriptorDeColumnas(postId);
            columnas.Add(titulo: "Contrato", propiedad: nameof(ContratoDto.Expresion), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnas.Add(titulo: nameof(ContratoDto.Tipo), propiedad: nameof(ContratoDto.Tipo), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnas.Add(titulo: nameof(ContratoDto.Estado), propiedad: nameof(ContratoDto.Estado), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnas.Add(titulo: nameof(ContratoDto.Importe), propiedad: nameof(ContratoDto.Importe), alineacion: enumAliniacion.derecha, mostrar: true, formato: enumFormato.Moneda);
            columnas.Add(titulo: nameof(ContratoDto.Id), propiedad: nameof(ContratoDto.Id), alineacion: enumAliniacion.derecha, mostrar: false);
            var parametros = new Dictionary<string, object> {
               { nameof(GridDeRelacion.Controlador), typeof(ContratosController) },
               { nameof(GridDeRelacion.AccionDeConsulta), nameof(ContratosController.epLeerElementos)},
               { nameof(GridDeRelacion.PropiedadRestrictora), nameof(ContratoDto.IdExpediente) },
               { nameof(GridDeRelacion.RestrictorFijo), nameof(ContratoDto.ClaseDeContrato) + "=" + claseContrato},
               { nameof(GridDeRelacion.PaginaDondeNavegarAlEditar) , $"{nameof(ContratosController)}/{crud}?id={nameof(ContratoDto.Id)}"},
               { nameof(GridDeRelacion.OcultarSiVacio), false}
            };

            var gridDeRelacion = new GridDeRelacion(expansor, columnas, parametros);
            gridDeRelacion.PermitirBorrar = false;
            var accion = new AccionDeGetionarDatosDependientes(
                    urlDelCrud: $@"/{nameof(ContratosController)}/{crud}?origen=dependencia"
                  , datosDependientes: nameof(ContratoDto)
                  , nombreDelMnt: DescriptorDeMantenimiento<ContratoDto>.NombreMnt
                  , propiedadQueRestringe: nameof(ExpedienteDto.Id)
                  , propiedadRestrictora: nameof(ContratoDto.IdExpediente)
                  , "Crear o gestionar Contratos");

            expansor.DescriptorDeNavegadorRefParaCrear("Crear Contrato", accion, enumParaQueNavegar.crear);
            expansor.DescriptorDeEditarRelaciones(Editor.Crud.Contexto, typeof(ContratoDto), typeof(ContratosController), "Editar Contrato", false);
            expansor.PostFijoNombreIdDeLaTabla = "-" + claseContrato.ToString().ToLower();

            //expansor.DescriptorParaVincular(Editor.Crud.Contexto, NegociosDeSe.IdNegocio(enumNegocio.Contrato), typeof(SelectorDePptDto), nameof(ContratosController), "Adjuntar un Contrato");
        }

        private void DescriptorDeSpanDeRegistroEs()
        {
            var expansor = new DescriptorDeExpansor(Editor, $"{Editor.Id}-registrosEs", "RegistrosEs", true, "Registros de ES del expediente");
            Editor.Expanes.Insert(1, expansor);

            //Definimos el grid de detalles del cuerpo
            var columnas = new DescriptorDeColumnas("registrosEs");
            columnas.Add(titulo: "Registro", propiedad: nameof(RegistroEsDto.Expresion), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnas.Add(titulo: "Estado", propiedad: nameof(RegistroEsDto.Estado), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnas.Add(titulo: "Id", propiedad: nameof(RegistroEsDtm.Id), alineacion: enumAliniacion.derecha, mostrar: false);
            var parametros = new Dictionary<string, object> {
                   { nameof(GridDeRelacion.Controlador), typeof(RegistrosEsController) }
                 , { nameof(GridDeRelacion.AccionDeConsulta), nameof(BaseController<ExpedienteDto>.epLeerVinculosCon)}
                 , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(VinculoDtm.idElemento1) }
                 , { nameof(ltrParametrosEp.idVinculado) , NegociosDeSe.IdNegocio(enumNegocio.Registro) }
                 , { nameof(GridDeRelacion.PaginaDondeNavegarAlEditar) , $"{nameof(RegistrosEsController)}/{nameof(RegistrosEsController.CrudRegistrosEs)}"}
                 , { nameof(GridDeRelacion.OcultarSiVacio), true}
                };
            var gridDeRelacion = new GridDeRelacion(expansor, columnas, parametros);
            gridDeRelacion.PermitirBorrar = true;

            expansor.DescriptorParaVincular(Editor.Crud.Contexto, NegociosDeSe.IdNegocio(enumNegocio.Registro), typeof(SelectorDeRegistroEsDto), nameof(RegistrosEsController), "Asociar registro de E/S");
        }

        private void DescriptorDeSpanDeFacturasRec()
        {
            var expansor = new DescriptorDeExpansor(Editor, $"{Editor.Id}-facturasrec", "Facturas Recibidas", true, "Facturas recibidas del expediente");
            Editor.Expanes.Insert(2, expansor);
            var columnas = new DescriptorDeColumnas("facturasrec");
            columnas.Add(titulo: "Factura", propiedad: nameof(FacturaRecDto.Expresion), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnas.Add(titulo: nameof(FacturaRecDto.Proveedor), alineacion: enumAliniacion.izquierda, mostrar: true);
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
               { nameof(GridDeRelacion.PropiedadRestrictora), nameof(FacturaRecDto.IdExpediente) },
               { nameof(GridDeRelacion.PaginaDondeNavegarAlEditar) , $"{nameof(FacturasRecController)}/{nameof(FacturasRecController.CrudFacturasRec)}?id={nameof(FacturaRecDto.Id)}"},
               { nameof(GridDeRelacion.OcultarSiVacio), false}
            };

            var gridDeRelacion = new GridDeRelacion(expansor, columnas, parametros);
            gridDeRelacion.PermitirBorrar = true;
            gridDeRelacion.PermitirEditar = false;

            var accion = new AccionDeGetionarDatosDependientes(
                    urlDelCrud: $@"/{nameof(FacturasRecController)}/{nameof(FacturasRecController.CrudFacturasRec)}?origen=dependencia"
                  , datosDependientes: nameof(FacturaRecDto)
                  , nombreDelMnt: DescriptorDeMantenimiento<FacturaRecDto>.NombreMnt
                  , propiedadQueRestringe: nameof(ExpedienteDto.Id)
                  , propiedadRestrictora: nameof(FacturaRecDto.IdExpediente)
                  , "Crear o gestionar facturas");
            expansor.DescriptorParaVincular(Editor.Crud.Contexto, NegociosDeSe.IdNegocio(enumNegocio.FacturaRecibida), typeof(SelectorDeFarDto), nameof(FacturasRecController), "Asociar factura");
            expansor.DescriptorDeNavegadorRefParaCrear("Crear factura", accion, enumParaQueNavegar.crear);
        }

        private void DescriptorDeSpanDeFacturasEmt()
        {
            var expansor = new DescriptorDeExpansor(Editor, $"{Editor.Id}-facturaemt", "Facturas Emitidas", true, "Facturas emitidas del expediente");
            Editor.Expanes.Insert(3, expansor);
            var columnas = new DescriptorDeColumnas("facturasemt");
            columnas.Add(titulo: "Factura", propiedad: nameof(FacturaEmtDto.Expresion), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnas.Add(titulo: nameof(FacturaEmtDto.Cliente), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnas.Add(titulo: "Número", propiedad: nameof(FacturaEmtDto.NumeroFactura), alineacion: enumAliniacion.derecha, mostrar: true);
            columnas.Add(titulo: nameof(FacturaEmtDto.Tipo), propiedad: nameof(FacturaEmtDto.Tipo), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnas.Add(titulo: nameof(FacturaEmtDto.Estado), propiedad: nameof(FacturaEmtDto.Estado), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnas.Add(titulo: "B.I", propiedad: nameof(FacturaEmtDto.TotalSinIva), alineacion: enumAliniacion.derecha, mostrar: true, formato: enumFormato.Moneda);
            columnas.Add(titulo: "Cobrado", propiedad: nameof(FacturaEmtDto.Cobrado), alineacion: enumAliniacion.derecha, mostrar: true, formato: enumFormato.Moneda);
            columnas.Add(titulo: nameof(FacturaEmtDto.Id), propiedad: nameof(FacturaEmtDto.Id), alineacion: enumAliniacion.derecha, mostrar: false);
            var parametros = new Dictionary<string, object> {
               { nameof(GridDeRelacion.Controlador), typeof(FacturasEmtController) },
               { nameof(GridDeRelacion.AccionDeConsulta), nameof(FacturasEmtController.epLeerElementos)},
               { nameof(GridDeRelacion.PropiedadRestrictora), nameof(ltrDeUnaFacturaEmt.IdExpediente) },
               { nameof(GridDeRelacion.PaginaDondeNavegarAlEditar) , $"{nameof(FacturasEmtController)}/{nameof(FacturasEmtController.CrudFacturasEmt)}?id={nameof(FacturaEmtDto.Id)}"},
               { nameof(GridDeRelacion.OcultarSiVacio), false}
            };

            var gridDeRelacion = new GridDeRelacion(expansor, columnas, parametros);
            gridDeRelacion.PermitirBorrar = false;
            gridDeRelacion.PermitirEditar = false;

        }

        private DescriptorDeExpansor DescriptorDeApuntes()
        {
            var expansor = new DescriptorDeExpansor(Editor, $"{Editor.Id}-apuntes", "Otros apuntes", true, "apuntes económicos");
            Editor.Expanes.Insert(4, expansor);

            var columnas = new DescriptorDeColumnas("apuntes");
            columnas.Add(titulo: nameof(ApunteDeExpedienteDto.Orden), alineacion: enumAliniacion.derecha, tamano: 100);
            columnas.Add(titulo: nameof(ApunteDeExpedienteDto.Clase), tamano: 100);
            columnas.Add(titulo: nameof(ApunteDeExpedienteDto.Naturaleza));
            columnas.Add(titulo: nameof(ApunteDeExpedienteDto.Concepto));
            columnas.Add(titulo: nameof(ApunteDeExpedienteDto.Valor), alineacion: enumAliniacion.derecha, tamano: 150, formato: enumFormato.Moneda);
            columnas.Add(titulo: nameof(ApunteDeExpedienteDto.IdElemento), mostrar: false);
            columnas.Add(titulo: nameof(ApunteDeExpedienteDto.Id), mostrar: false);

            var orden = $"{nameof(ApunteDeExpedienteDto.Orden)}:{enumModoOrdenacion.ascendente.Render()}";


            var parametros = new Dictionary<string, object> {
                   { nameof(GridDeRelacion.Controlador), typeof(ApuntesDeExpedienteController) }
                 , { nameof(GridDeRelacion.AccionDeConsulta), nameof(ApuntesDeExpedienteController.epLeerElementos)}
                 , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(ApunteDeExpedienteDto.IdElemento) }
                 , { nameof(GridDeRelacion.OrdenarPor), orden }
                 , { nameof(GridDeRelacion.OcultarSiVacio), false}
                };
            var gridDeRelacion = new GridDeRelacion(expansor, columnas, parametros);
            gridDeRelacion.PermitirBorrar = true;

            var modalDeCreacion = expansor.DescriptorDeCrearRelaciones(Editor.Crud.Contexto, typeof(ApunteDeExpedienteDto), typeof(ApuntesDeExpedienteController), nameof(ApunteDeExpedienteDto.IdElemento), "Añadir apunte");
            modalDeCreacion.AccionTrasAbrirModal = $"javascript: {enumNameSpaceTs.Administracion}.{enumFunctionTs.Exp_InicializarModalParaCrearApuntes}()";

            var modalDeEdicion = expansor.DescriptorDeEditarRelaciones(Editor.Crud.Contexto, typeof(ApunteDeExpedienteDto), typeof(ApuntesDeExpedienteController), "Editar apunte", soloConsulta: false);
            modalDeEdicion.AccionTrasAbrirModal = $"javascript: {enumNameSpaceTs.Administracion}.{enumFunctionTs.Exp_InicializarModalParaEditarApuntes}();";

            return expansor;
        }


        public override string RenderControl()
        {
            if (!_renderCache.IsNullOrEmpty())
                return _renderCache;

            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
            var render = base.RenderControl();
            render = render +
                   $@"<script src=¨../../js/{enumNameSpaceTs.Negocio}/TiposDeElemento.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script src=¨../../js/{enumNameSpaceTs.Venta}/ApiDeTipoDePpt.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script src=¨../../js/{RutaBase}/ApiDeAdministracion.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script src=¨../../js/{RutaBase}/Expedientes.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{      
                           {RutaBase}.CrearCrudDeExpedientes('{Mnt.IdHtml}', '{Creador.IdHtml}', '{Editor.IdHtml}', '{Borrado.IdHtml}', '{_clase}') 
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
