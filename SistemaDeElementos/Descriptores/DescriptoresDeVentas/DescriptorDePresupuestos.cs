using ServicioDeDatos;
using MVCSistemaDeElementos.Controllers;
using UtilidadesParaIu;
using Utilidades;
using ModeloDeDto.Presupuesto;
using System.Collections.Generic;
using ModeloDeDto.Expediente;
using ServicioDeDatos.Seguridad;
using ModeloDeDto;
using ModeloDeDto.Terceros;
using ModeloDeDto.Entorno;
using GestorDeElementos;
using ServicioDeDatos.Presupuesto;
using ModeloDeDto.Ventas;
using ServicioDeDatos.Ventas;
using ServicioDeDatos.Expediente;
using GestorDeElementos.Extensores;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDePresupuestos : DescriptorDeCrud<PresupuestoDto>
    {
        public DescriptorDePresupuestos(ContextoSe contexto, string renderCache) : base(contexto,renderCache)
        {
        }
        public DescriptorDePresupuestos(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto
               , nameof(PresupuestosController)
               , nameof(PresupuestosController.CrudPresupuestos)
               , modo
               , rutaBase: enumNameSpaceTs.Presupuesto)
        {
            IncluirFiltros();
            DefinirMf(menuEdicion, Editor.OpcionesMf);
            DescriptorDeLineasDeUnPpt();

            modalesParaPedirDatos.Add(new ModalParaPedirDatos(this, typeof(AsociarExpedienteDto), eventosDeMf.Ppt_AsociarUnExpedienteAUnPpt, "Asociar expediente al presupuesto"));
            modalesParaPedirDatos.Add(new ModalParaPedirDatos(this, typeof(RenombrarPptDto), eventosDeMf.Ppt_Renombrar, "Renombrar"));
            modalesParaPedirDatos.Add(new ModalParaPedirDatos(this, typeof(CopiarPptDto), eventosDeMf.Ppt_CopiarPpt, "Seleccionar presupuesto o plantilla"));

            modalesParaPedirDatos.Add(new ModalDeTotales(this, typeof(TotalesDePresupuestos), eventosDeMf.Totalizador_Mostrar, $"Mostrar los totales de la selección"));
            Mnt.IncluirMfContextual($"<li id='{menuContextual}.{eventosDeMf.Totalizador_Mostrar}' accion-menu='{eventosDeMf.Totalizador_Mostrar}' {AtributosHtml.Mf(enumCssOpcionMenu.DeVista, enumModoDeAccesoDeDatos.Consultor, false)}>Mostrar totales</li>");

            EspanDePartesTr();
            EspanDeFacturas();

            DescriptorDeDireccion(Ampliaciones.Presupuestos.DireccionAlCrear, "Dirección de obra/ejecución", typeof(CrearDireccionDeEjecucionDto));

            Mnt.IncluirMfDeRelacion($"<li id='{menuDeRelaciones}.{enumNegocio.Tarea}' accion-menu='{eventosDeMf.Ppt_IrATareas}' {AtributosHtml.Mf(enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Consultor, false)}>Tareas vinculadas</li>");
            Mnt.IncluirMfDeRelacion($"<li id='{menuDeRelaciones}.{enumNegocio.ParteDeTrabajo}' accion-menu='{eventosDeMf.Ppt_IrAPartesTr}' {AtributosHtml.Mf(enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Consultor, false)}>{enumNegocio.ParteDeTrabajo.Plural()}</li>");
            Mnt.IncluirMfDeRelacion($"<li id='{menuDeRelaciones}.{enumNegocio.FacturaEmitida}' accion-menu='{eventosDeMf.Ppt_IrAFacturasEmt}' {AtributosHtml.Mf(enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Consultor, false)}>{enumNegocio.FacturaEmitida.Plural()}</li>");

        }

        private void IncluirFiltros()
        {
            var modal = new ModalDeFiltrado<PresupuestoDto>(Mnt.Filtro, "filtrosDelPpt", "Datos del presupuesto");
            Mnt.Filtro.Modales.Add(modal);
            FiltroPorExpediente(modal);
            FiltroPorSolicitante(modal);
            FiltroPorPartesTr(modal);
            FiltroPorFacturaEmt(modal);
            modal.ControlesDeFiltrado.Add(new FiltroDeDireccion<PresupuestoDto>(modal));
        }

        private void FiltroPorExpediente(ModalDeFiltrado<PresupuestoDto> modal)
        {
            var lista = new ListasDinamicas<PresupuestoDto>(modal,
                etiqueta: "Expediente",
                filtrarPor: ltrDeUnPresupuesto.IdExpediente,
                ayuda: "seleccione el expediente",
                seleccionarDe: nameof(ExpedienteDto),
                buscarPor: nameof(ltrDeUnExpediente.ExpedienteConValoracion),
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

            modal.ControlesDeFiltrado.Add(new FiltroDeRelacion<PresupuestoDto>(modal,
                lista: lista,
                opciones: opciones,
                propiedad: "ExpedientePadre",
                filtrarPor: nameof(ltrDeUnPresupuesto.DependeDeExpediente),
                ayuda: "Filtros de expedientes"
                ));
        }

        private static void FiltroPorSolicitante(ModalDeFiltrado<PresupuestoDto> modal)
        {
            modal.ControlesDeFiltrado.Add(new ListasDinamicas<PresupuestoDto>(modal,
                etiqueta: "Solicitante",
                filtrarPor: ltrDeUnPresupuesto.IdCliente,
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

            modal.ControlesDeFiltrado.Add(new ListasDinamicas<PresupuestoDto>(modal,
                 etiqueta: "Responsable",
                 filtrarPor: ltrDeUnPresupuesto.IdResponsable,
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

            //var idMostrarExpediente = $"{modal.Id}_{enumTipoControl.Check.Render()}_mostrar_expediente";
            //var accionClick = $"onclick = javascript:Crud.{enumGestorDeEventos.EventosDelMantenimiento}('{eventosDeMnt.OcultarMostrarColumnas}','{nameof(PresupuestoDto.Expediente)}');";


            //var columnaExpediente = new CheckDeMostrarColumna<PresupuestoDto>(modal,
            //         etiqueta: "Mostra expediente",
            //         ayuda: "Muestra el expediente asociado al presupuesto",
            //         valorInicial: false,
            //         columna: "Expediente",
            //         columnas: null,
            //         accion: accionClick);

            //var columnaModificacion = new CheckDeAccionFlt<PresupuestoDto>(modal,
            //    id: idMostrarExpediente,
            //    etiqueta: "Mostra expediente",
            //    ayuda: "Muestra los expedientes asociadas al presupuesto",
            //    valorInicial: false,
            //    posicion: new Posicion(3, 1),
            //    accion: accionModificacion);


            //modal.ControlesDeFiltrado.Add(columnaExpediente);
        }

        private void FiltroPorPartesTr(ModalDeFiltrado<PresupuestoDto> modal)
        {
            var lista = new ListasDinamicas<PresupuestoDto>(modal,
                  etiqueta: enumNegocio.ParteDeTrabajo.Singular(),
                  filtrarPor: ltrDeUnPresupuesto.IdParteTr,
                  ayuda: $"seleccione el {enumNegocio.ParteDeTrabajo.Singular(true)}",
                  seleccionarDe: nameof(ParteTrDto),
                  buscarPor: ltrDeUnParteTr.DependeDePpt,
                  mostrarExpresion: nameof(ParteTrDto.Expresion),
                  criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                  posicion: new Posicion(1, 0),
                  controlador: nameof(PartesTrController),
                  navegarA: nameof(PartesTrController.CrudPartesDeTrabajo),
                  restringirPor: "",
                  alSeleccionarBlanquearControl: "")
            { LongitudMinimaParaBuscar = 1 };

            var opciones = new Dictionary<string, string> {
                      { $"{ltrParametrosNeg.MostrarTodos}", "Con o sin partes" }
                    , { $"{ltrParametrosNeg.ConRelacion}", "Con partes" }
                    , { $"{ltrParametrosNeg.SinRelacion}", "Sin partes" }
                    , { $"{ltrDeUnPresupuesto.ConPartesPdtDeFacturar}", "Pendiente facturar" }
                    , { $"{ltrDeUnPresupuesto.ConPartesFacturados}", "Con partes facturados" }
            };

            modal.ControlesDeFiltrado.Add(new FiltroDeRelacion<PresupuestoDto>(modal,
                lista: lista,
                opciones: opciones,
                propiedad: ltrDeUnPresupuesto.FiltroPorConOSinParteTr,
                filtrarPor: ltrDeUnPresupuesto.FiltroPorConOSinParteTr,
                ayuda: $"Filtros de {enumNegocio.ParteDeTrabajo.Plural(true)}"
                ));
        }

        private void FiltroPorFacturaEmt(ModalDeFiltrado<PresupuestoDto> modal)
        {
            var lista = new ListasDinamicas<PresupuestoDto>(modal,
                  etiqueta: enumNegocio.FacturaEmitida.Singular(),
                  filtrarPor: ltrDeUnPresupuesto.IdFacturaEmt,
                  ayuda: $"seleccione la {enumNegocio.FacturaEmitida.Singular(true)}",
                  seleccionarDe: nameof(FacturaEmtDto),
                  buscarPor: ltrDeUnaFacturaEmt.DependeDePpt,
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

            modal.ControlesDeFiltrado.Add(new FiltroDeRelacion<PresupuestoDto>(modal,
                lista: lista,
                opciones: opciones,
                propiedad: ltrDeUnPresupuesto.FiltroPorConOSinFacturaEmt,
                filtrarPor: ltrDeUnPresupuesto.FiltroPorConOSinFacturaEmt,
                ayuda: $"Filtros de {enumNegocio.FacturaEmitida.Plural(true)}"
                ));
        }

        private void DefinirMf(string idMenu, List<string> opciones)
        {
            DescriptorDeEdicion<PresupuestoDto>.IncluirMfIndividual(opciones, "<hr>");
            DescriptorDeEdicion<PresupuestoDto>.IncluirMfIndividual(opciones, $"<li id='{idMenu}.{eventosDeMf.Ppt_AsociarUnExpedienteAUnPpt}' accion-menu='{eventosDeMf.Ppt_AsociarUnExpedienteAUnPpt}' {AtributosHtml.Mf(enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Gestor, false)}>Asociar a Expediente</li>");
            DescriptorDeEdicion<PresupuestoDto>.IncluirMfIndividual(opciones, $"<li id='{idMenu}.{eventosDeMf.Ppt_Renombrar}' accion-menu='{eventosDeMf.Ppt_Renombrar}' {AtributosHtml.Mf(enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Interventor, false)}>Renombrar</li>");

            Mnt.IncluirMfContextual("<hr>");
            Mnt.IncluirMfContextual($"<li id='{menuContextual}.{eventosDeMf.Ppt_CopiarPpt}' accion-menu='{eventosDeMf.Ppt_CopiarPpt}' {AtributosHtml.Mf(enumCssOpcionMenu.DeVista, enumModoDeAccesoDeDatos.Gestor, false)}>Copiar presupuesto</li>");
        }

        private void DescriptorDePptDeVenta()
        {
            var datosVenta = new AmpliacionDeEdicion(Editor, Ampliaciones.Presupuestos.PptDeVenta, "Propuesta por línea", new Dimension(2, 2), ayuda: "Propuesta del porcentaje para el descuento e IVA por línea");
            datosVenta.Dto = typeof(PptDeVentaDto);
            datosVenta.Controlador = nameof(PptsDeVentaController);
            Editor.Ampliaciones.Add(datosVenta);
        }

        private void DescriptorDeLineasDeUnPpt()
        {
            var expansor = new DescriptorDeExpansor(Editor, $"{Editor.Id}-lineasdeunppt", "Detalle", true, "Líneas del presupuesto");
            expansor.EsDetalle = true;
            Editor.Expanes.Insert(0, expansor);

            //Definimos el grid de detalles del presupuesto
            var columnas = new DescriptorDeColumnas("lineasDeUnPpt");
            columnas.Add(titulo: nameof(LineaDeUnPptDto.Orden), alineacion: enumAliniacion.derecha, tamano: 100);
            columnas.Add(titulo: nameof(LineaDeUnPptDto.Concepto));
            columnas.Add(titulo: nameof(LineaDeUnPptDto.Unidad), autoAjustable: true);
            columnas.Add(titulo: nameof(LineaDeUnPptDto.Cantidad), tamano: 150, formato: enumFormato.Numero_6);
            columnas.Add(titulo: nameof(LineaDeUnPptDto.Precio), formato: enumFormato.Moneda, tamano: 150);
            columnas.Add(titulo: nameof(LineaDeUnPptDto.Descuento), formato: enumFormato.Porcentaje, tamano: 150);
            columnas.Add(titulo: "B.I.", propiedad: nameof(LineaDeUnPptDto.BaseImponible), formato: enumFormato.Moneda, tamano: 150);
            columnas.Add(titulo: nameof(LineaDeUnPptDto.Iva), formato: enumFormato.Porcentaje, tamano: 150);
            columnas.Add(titulo: "Importe", propiedad: nameof(LineaDeUnPptDto.ImporteDeLinea), formato: enumFormato.Moneda, tamano: 150);
            columnas.Add(titulo: "IdElemento", propiedad: nameof(LineaDeUnPptDto.IdElemento), mostrar: false);
            columnas.Add(titulo: "Id", propiedad: nameof(LineaDeUnPptDto.Id), mostrar: false);

            var orden = $"{nameof(LineaDeUnPptDto.Orden)}:{enumModoOrdenacion.ascendente.Render()}";


            var parametros = new Dictionary<string, object> {
                   { nameof(GridDeRelacion.Controlador), typeof(LineasDeUnPptController) }
                 , { nameof(GridDeRelacion.AccionDeConsulta), nameof(LineasDeUnPptController.epLeerElementos)}
                 , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(LineaDeUnPptDto.IdElemento) }
                ,  { nameof(GridDeRelacion.OrdenarPor), orden }
                 , { nameof(GridDeRelacion.OcultarSiVacio), false}
                };
            var gridDeRelacion = new GridDeRelacion(expansor, columnas, parametros);
            gridDeRelacion.PermitirBorrar = true;
            
            var modalDeCreacion = expansor.DescriptorDeCrearRelaciones(Editor.Crud.Contexto, typeof(LineaDeUnPptDto), typeof(LineasDeUnPptController), nameof(LineaDeUnPptDto.IdElemento), "Añadir línea");
            modalDeCreacion.AccionTrasAbrirModal = $"javascript: {RutaBase}.{enumFunctionTs.Ppt_InicializarModalParaCrearLineas}({ExtensorDePresupuestos.IncrementarOrdenEn(Contexto)})";

            var modalDeEdicion = expansor.DescriptorDeEditarRelaciones(Editor.Crud.Contexto, typeof(LineaDeUnPptDto), typeof(LineasDeUnPptController), "Editar línea", soloConsulta: false);
            modalDeEdicion.AccionTrasAbrirModal = $"javascript: {RutaBase}.{enumFunctionTs.Ppt_InicializarModalParaEditarLineas}();";

        }

        private void EspanDePartesTr()
        {
            var expansor = new DescriptorDeExpansor(Editor, $"{Editor.Id}-partes", "Partes", true, "partes del presupuesto");
            Editor.Expanes.Insert(1, expansor);
            var columnas = new DescriptorDeColumnas("partes");
            columnas.Add(titulo: enumNegocio.ParteDeTrabajo.Singular(), propiedad: nameof(ParteTrDto.Expresion), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnas.Add(titulo: nameof(ParteTrDto.Tipo), propiedad: nameof(ParteTrDto.Tipo), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnas.Add(titulo: nameof(ParteTrDto.Estado), propiedad: nameof(ParteTrDto.Estado), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnas.Add(titulo: "Total sin Iva", propiedad: nameof(ParteTrDto.TotalSinIva), formato: enumFormato.Moneda);
            columnas.Add(titulo: nameof(ParteTrDto.Id), propiedad: nameof(ParteTrDto.Id), alineacion: enumAliniacion.derecha, mostrar: false);
            var parametros = new Dictionary<string, object> {
               { nameof(GridDeRelacion.Controlador), typeof(PartesTrController) },
               { nameof(GridDeRelacion.AccionDeConsulta), nameof(PartesTrController.epLeerElementos)},
               { nameof(GridDeRelacion.PropiedadRestrictora), nameof(ParteTrDto.IdPresupuesto) },
               { nameof(GridDeRelacion.PaginaDondeNavegarAlEditar) , $"{nameof(PartesTrController)}/{nameof(PartesTrController.CrudPartesDeTrabajo)}?id={nameof(ParteTrDto.Id)}"},
               { nameof(GridDeRelacion.OcultarSiVacio), false}
            };

            var gridDeRelacion = new GridDeRelacion(expansor, columnas, parametros);
            gridDeRelacion.PermitirBorrar = false;

            var accion = new AccionDeGetionarDatosDependientes(
                    urlDelCrud: $@"/{nameof(PartesTrController)}/{nameof(PartesTrController.CrudPartesDeTrabajo)}?origen=dependencia"
                  , datosDependientes: nameof(ParteTrDto)
                  , nombreDelMnt: DescriptorDeMantenimiento<ParteTrDto>.NombreMnt
                  , propiedadQueRestringe: nameof(PresupuestoDto.Id)
                  , propiedadRestrictora: nameof(ParteTrDto.IdPresupuesto)
                  , "Crear o gestionar partes");

            expansor.DescriptorDeNavegadorRefParaCrear("Crear parte", accion, enumParaQueNavegar.crear);
            expansor.DescriptorDeEditarRelaciones(Editor.Crud.Contexto, typeof(ParteTrDto), typeof(PartesTrController), "Editar parte de trabajo", false);
        }

        private void EspanDeFacturas()
        {
            var expansor = new DescriptorDeExpansor(Editor, $"{Editor.Id}-facturas", "Facturas", true, "facturas del presupuesto");
            Editor.Expanes.Insert(2, expansor);
            var columnas = new DescriptorDeColumnas("facturas");
            columnas.Add(titulo: enumNegocio.FacturaEmitida.Singular(), propiedad: nameof(FacturaEmtDto.Expresion), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnas.Add(titulo: nameof(FacturaEmtDto.Tipo), propiedad: nameof(FacturaEmtDto.Tipo), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnas.Add(titulo: nameof(FacturaEmtDto.Estado), propiedad: nameof(FacturaEmtDto.Estado), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnas.Add(titulo: "Total sin Iva", propiedad: nameof(FacturaEmtDto.TotalSinIva), formato: enumFormato.Moneda);
            columnas.Add(titulo: nameof(FacturaEmtDto.Id), propiedad: nameof(FacturaEmtDto.Id), alineacion: enumAliniacion.derecha, mostrar: false);
            var parametros = new Dictionary<string, object> {
               { nameof(GridDeRelacion.Controlador), typeof(FacturasEmtController) },
               { nameof(GridDeRelacion.AccionDeConsulta), nameof(FacturasEmtController.epLeerElementos)},
               { nameof(GridDeRelacion.PropiedadRestrictora), nameof(FacturaEmtDto.IdPresupuesto) },
               { nameof(GridDeRelacion.PaginaDondeNavegarAlEditar) , $"{nameof(FacturasEmtController)}/{nameof(FacturasEmtController.CrudFacturasEmt)}?id={nameof(FacturaEmtDto.Id)}"},
               { nameof(GridDeRelacion.OcultarSiVacio), false}
            };

            var gridDeRelacion = new GridDeRelacion(expansor, columnas, parametros);
            gridDeRelacion.PermitirBorrar = false;

            var accion = new AccionDeGetionarDatosDependientes(
                    urlDelCrud: $@"/{nameof(FacturasEmtController)}/{nameof(FacturasEmtController.CrudFacturasEmt)}?origen=dependencia"
                  , datosDependientes: nameof(FacturaEmtDto)
                  , nombreDelMnt: DescriptorDeMantenimiento<FacturaEmtDto>.NombreMnt
                  , propiedadQueRestringe: nameof(PresupuestoDto.Id)
                  , propiedadRestrictora: nameof(FacturaEmtDto.IdPresupuesto)
                  , "Crear o gestionar facturas");

            expansor.DescriptorDeNavegadorRefParaCrear("Crear factura", accion, enumParaQueNavegar.crear);
            expansor.DescriptorDeEditarRelaciones(Editor.Crud.Contexto, typeof(FacturaEmtDto), typeof(FacturasEmtController), "Editar factura", false);
            //expansor.DescriptorParaVincular(Editor.Crud.Contexto, NegociosDeSe.IdNegocio(enumNegocio.Presupuesto), typeof(SelectorDePptDto), nameof(FacturasEmtController), "Adjuntar un presupuesto");
        }


        public override string RenderControl()
        {
            if (!_renderCache.IsNullOrEmpty())
                return _renderCache;

            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
            var render = base.RenderControl();

            render = render +
                   $@"<script src=¨../../js/{RutaBase}/Presupuestos.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{      
                           {RutaBase}.CrearCrudDePresupuestos('{Mnt.IdHtml}', '{Creador.IdHtml}', '{Editor.IdHtml}', '{Borrado.IdHtml}') 
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
