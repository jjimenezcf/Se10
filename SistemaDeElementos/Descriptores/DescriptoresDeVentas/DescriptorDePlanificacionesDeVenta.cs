using ServicioDeDatos;
using MVCSistemaDeElementos.Controllers;
using UtilidadesParaIu;
using Utilidades;
using ModeloDeDto.Ventas;
using ServicioDeDatos.Seguridad;
using ModeloDeDto;
using System.Collections.Generic;
using GestorDeElementos;
using ModeloDeDto.Juridico;
using ServicioDeDatos.Ventas;
using ModeloDeDto.MaestrosTecnico;
using ServicioDeDatos.Juridico;
using static GestoresDeNegocio.Juridico.GestorDelPlanificadorDeVentas;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDePlanificacionesDeVenta : DescriptorDeCrud<PlanificacionDeVentaDto>
    {
        public DescriptorDePlanificacionesDeVenta(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto
               , nameof(PlanificacionesDeVentaController)
               , nameof(PlanificacionesDeVentaController.CrudPlanificacionesDeVenta)
               , modo
               , rutaBase: enumNameSpaceTs.Venta)
        {
            Mnt.OrdenacionInicial = @$"{nameof(PlanificacionDeVentaDto.EjecutarEl)}:{nameof(PlanificacionDeVentaDto.EjecutarEl)}:{enumModoOrdenacion.ascendente.Render()}";
            IncluirFiltrosDePlanificaciones();
            LineasDeLaPlanificacion();
            Mnt.IncluirMfDeRelacion($"<li id='{menuDeRelaciones}.{enumNegocio.ParteDeTrabajo}' accion-menu='{eventosDeMf.Plv_IrAPartesTr}' {AtributosHtml.Mf(enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Consultor, false)}>{enumNegocio.ParteDeTrabajo.Plural()}</li>");
            Mnt.IncluirMfDeRelacion($"<li id='{menuDeRelaciones}.{enumNegocio.FacturaEmitida}' accion-menu='{eventosDeMf.Plv_IrAFacturasEmt}' {AtributosHtml.Mf(enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Consultor, false)}>{enumNegocio.FacturaEmitida.Plural()}</li>");
        }


        private void IncluirFiltrosDePlanificaciones()
        {
            var modal = new ModalDeFiltrado<PlanificacionDeVentaDto>(Mnt.Filtro, "filtroDePlanificaciones", "Elementos relacionados");
            Mnt.Filtro.Modales.Add(modal);
            FiltroPorUnitario(modal);
            FiltroPorPlanificador(modal);
            FiltroPorContrato(modal);
            FiltroPorParteTr(modal);
            FiltroPorFacturaEmt(modal);
        }

        private void FiltroPorUnitario(ModalDeFiltrado<PlanificacionDeVentaDto> modal)
        {
            modal.ControlesDeFiltrado.Add(new ListasDinamicas<PlanificacionDeVentaDto>(modal,
                etiqueta: enumNegocio.Unitario.Singular(),
                filtrarPor: ltrDeUnaPlanificacionDeVenta.IdUnitario,
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

        private void FiltroPorPlanificador(ModalDeFiltrado<PlanificacionDeVentaDto> modal)
        {
            var lista = new ListasDinamicas<PlanificacionDeVentaDto>(modal,
                         etiqueta: enumNegocio.PlanificadorDeVenta.Singular(),
                         filtrarPor: ltrDeUnaPlanificacionDeVenta.IdPlanificador,
                         ayuda: $"seleccione el {enumNegocio.PlanificadorDeVenta.Singular(true)}",
                         seleccionarDe: nameof(PlanificadorDeVentaDto),
                         buscarPor: ltrPlanificadorDeVentas.SelectorParaUnaPlvDeVenta,
                         mostrarExpresion: nameof(PlanificadorDeVentaDto.Expresion),
                         criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                         posicion: new Posicion(1, 0),
                         controlador: nameof(PlanificadorDeVentasController),
                         navegarA: nameof(PlanificadorDeVentasController.CrudPlanificadorDeVentas),
                         restringirPor: "",
                         alSeleccionarBlanquearControl: "",
                         trasMapear: $"javascript:{nameof(enumNameSpaceTs.Venta)}.{nameof(enumFunctionTs.Plv_Tras_Mapear_Filtro_IdPlanificador)}({nameof(enumParamTs.lista)})")
            {
                LongitudMinimaParaBuscar = 1,
                ParametrosParaNavegar = nameof(ltrParametrosEp.Clase) + "=" + nameof(enumClaseDeContrato.Venta)
            };

            var opciones = new Dictionary<string, string> {
                      { $"{ltrParametrosNeg.MostrarTodos}", "Con o sin planificadores" }
                    , { $"{ltrParametrosNeg.ConRelacion}", "Con planificadores" }
                    , { $"{ltrParametrosNeg.SinRelacion}", "Sin planificadores" } };

            modal.ControlesDeFiltrado.Add(new FiltroDeRelacion<PlanificacionDeVentaDto>(modal,
                lista: lista,
                opciones: opciones,
                propiedad: ltrDeUnaPlanificacionDeVenta.FiltroPorConOSinPlanificador,
                filtrarPor: ltrDeUnaPlanificacionDeVenta.FiltroPorConOSinPlanificador,
                ayuda: $"Filtros de {enumNegocio.PlanificadorDeVenta.Plural(true)}"
                ));
        }

        private void FiltroPorContrato(ModalDeFiltrado<PlanificacionDeVentaDto> modal)
        {
            var lista = new ListasDinamicas<PlanificacionDeVentaDto>(modal,
                         etiqueta: enumNegocio.Contrato.Singular(),
                         filtrarPor: ltrDeUnaPlanificacionDeVenta.IdContrato,
                         ayuda: $"seleccione la {enumNegocio.Contrato.Singular(true)}",
                         seleccionarDe: nameof(ContratoDto),
                         buscarPor: ltrDeUnContrato.SelectorParaUnaPlvDeVenta,
                         mostrarExpresion: nameof(ContratoDto.Expresion),
                         criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                         posicion: new Posicion(1, 0),
                         controlador: nameof(ContratosController),
                         navegarA: nameof(ContratosController.CrudContratos),
                         restringirPor: "",
                         alSeleccionarBlanquearControl: "",
                         trasMapear: $"javascript:{nameof(enumNameSpaceTs.Venta)}.{nameof(enumFunctionTs.Plv_Tras_Mapear_Filtro_IdContrato)}({nameof(enumParamTs.lista)})")
            {
                LongitudMinimaParaBuscar = 1,
                ParametrosParaNavegar = nameof(ltrParametrosEp.Clase) + "=" + nameof(enumClaseDeContrato.Venta)
            };

            var opciones = new Dictionary<string, string> {
                      { $"{ltrParametrosNeg.MostrarTodos}", "Con o sin contratos" }
                    , { $"{ltrParametrosNeg.ConRelacion}", "Con contratos" }
                    , { $"{ltrParametrosNeg.SinRelacion}", "Sin contratos" } };

            modal.ControlesDeFiltrado.Add(new FiltroDeRelacion<PlanificacionDeVentaDto>(modal,
                lista: lista,
                opciones: opciones,
                propiedad: ltrDeUnaPlanificacionDeVenta.FiltroPorConOSinContrato,
                filtrarPor: ltrDeUnaPlanificacionDeVenta.FiltroPorConOSinContrato,
                ayuda: $"Filtros de {enumNegocio.Contrato.Plural(true)}"
                ));
        }

        private void FiltroPorParteTr(ModalDeFiltrado<PlanificacionDeVentaDto> modal)
        {
            var lista = new ListasDinamicas<PlanificacionDeVentaDto>(modal,
            etiqueta: enumNegocio.ParteDeTrabajo.Singular(),
            filtrarPor: ltrDeUnaPlanificacionDeVenta.IdParteTr,
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

            modal.ControlesDeFiltrado.Add(new FiltroDeRelacion<PlanificacionDeVentaDto>(modal,
                lista: lista,
                opciones: opciones,
                propiedad: ltrDeUnaPlanificacionDeVenta.FiltroPorConOSinParteTr,
                filtrarPor: ltrDeUnaPlanificacionDeVenta.FiltroPorConOSinParteTr,
                ayuda: $"Filtros de {enumNegocio.ParteDeTrabajo.Plural(true)}"
                ));
        }

        private void FiltroPorFacturaEmt(ModalDeFiltrado<PlanificacionDeVentaDto> modal)
        {
            var lista = new ListasDinamicas<PlanificacionDeVentaDto>(modal,
            etiqueta: enumNegocio.FacturaEmitida.Singular(),
            filtrarPor: ltrDeUnaPlanificacionDeVenta.IdFacturaEmt,
            ayuda: $"seleccione la {enumNegocio.FacturaEmitida.Singular(true)}",
            seleccionarDe: nameof(FacturaEmtDto),
            buscarPor: ltrDeUnaFacturaEmt.DependeDePlfVenta,
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

            modal.ControlesDeFiltrado.Add(new FiltroDeRelacion<PlanificacionDeVentaDto>(modal,
                lista: lista,
                opciones: opciones,
                propiedad: nameof(ltrDeUnaPlanificacionDeVenta.FiltroPorConOSinFacturaEmt),
                filtrarPor: nameof(ltrDeUnaPlanificacionDeVenta.FiltroPorConOSinFacturaEmt),
                ayuda: $"Filtros de {enumNegocio.FacturaEmitida.Plural(true)}"
                ));
        }

        private void LineasDeLaPlanificacion()
        {
            var expansor = new DescriptorDeExpansor(Editor, $"{Editor.Id}-lineasdeunaplv", "Detalle", true, "lineas de la planificación");
            expansor.EsDetalle = true;
            Editor.Expanes.Insert(0, expansor);

            var columnas = new DescriptorDeColumnas("lineasdeunaplv");
            columnas.Add(titulo: "Orden", propiedad: nameof(LineaDeUnaPlfVentaDto.Orden), alineacion: enumAliniacion.derecha, mostrar: true, tamano: 100);
            columnas.Add(titulo: "Concepto", propiedad: nameof(LineaDeUnaPlfVentaDto.Expresion), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnas.Add(titulo: "Unidad", propiedad: nameof(LineaDeUnaPlfVentaDto.Unidad), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnas.Add(titulo: "Cantidad", propiedad: nameof(LineaDeUnaPlfVentaDto.Cantidad), alineacion: enumAliniacion.derecha, mostrar: true, tamano: 150, formato: enumFormato.Numero_2);
            columnas.Add(titulo: "P. de venta", propiedad: nameof(LineaDeUnaPlfVentaDto.Venta), alineacion: enumAliniacion.derecha, mostrar: true, tamano: 150, formato: enumFormato.Moneda);
            columnas.Add(titulo: "Descuento", propiedad: nameof(LineaDeUnaPlfVentaDto.Descuento), alineacion: enumAliniacion.derecha, mostrar: true, tamano: 150, formato: enumFormato.Porcentaje);
            columnas.Add(titulo: "B.I.", propiedad: nameof(LineaDeUnaPlfVentaDto.BaseImponible), alineacion: enumAliniacion.derecha, mostrar: true, tamano: 150, formato: enumFormato.Moneda);
            columnas.Add(titulo: "Iva", propiedad: nameof(LineaDeUnaPlfVentaDto.Iva), alineacion: enumAliniacion.derecha, mostrar: true, tamano: 150, formato: enumFormato.Porcentaje);
            columnas.Add(titulo: "Importe", propiedad: nameof(LineaDeUnaPlfVentaDto.ImporteDeLinea), alineacion: enumAliniacion.derecha, mostrar: true, tamano: 150, formato: enumFormato.Moneda);
            columnas.Add(titulo: "IdElemento", propiedad: nameof(LineaDeUnaPlfVentaDto.IdElemento), alineacion: enumAliniacion.derecha, mostrar: false);
            columnas.Add(titulo: "Id", propiedad: nameof(LineaDeUnaPlfVentaDto.Id), alineacion: enumAliniacion.derecha, mostrar: false);

            var orden = $"{nameof(LineaDeUnaPlfVentaDto.Orden)}:{enumModoOrdenacion.ascendente.Render()}";


            var parametros = new Dictionary<string, object> {
                   { nameof(GridDeRelacion.Controlador), typeof(LineasDeUnaPlfVentaController) }
                 , { nameof(GridDeRelacion.AccionDeConsulta), nameof(LineasDeUnaPlfVentaController.epLeerElementos)}
                 , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(LineaDeUnaPlfVentaDto.IdElemento) }
                ,  { nameof(GridDeRelacion.OrdenarPor), orden }
                 , { nameof(GridDeRelacion.OcultarSiVacio), false}
                };
            var gridDeRelacion = new GridDeRelacion(expansor, columnas, parametros);
            gridDeRelacion.PermitirBorrar = true;

            var modalDeCreacion = expansor.DescriptorDeCrearRelaciones(Editor.Crud.Contexto, typeof(LineaDeUnaPlfVentaDto), typeof(LineasDeUnaPlfVentaController), nameof(LineaDeUnaPlfVentaDto.IdElemento), "Añadir línea");
            modalDeCreacion.AccionTrasAbrirModal = $"javascript: {enumNameSpaceTs.Venta}.{enumFunctionTs.Plv_InicializarModalParaCrearLineas}()";

            var modalDeEdicion = expansor.DescriptorDeEditarRelaciones(Editor.Crud.Contexto, typeof(LineaDeUnaPlfVentaDto), typeof(LineasDeUnaPlfVentaController), "Editar línea", soloConsulta: false);
            modalDeEdicion.AccionTrasAbrirModal = $"javascript: {enumNameSpaceTs.Venta}.{enumFunctionTs.Plv_InicializarModalParaEditarLineas}();";

        }

        public override string RenderControl()
        {
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
            if (ServicioDeCaches.Obtener(CacheDe.RenderCrud).ContainsKey(indice))
                return (string)ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice];
            var render = base.RenderControl();

            render = render +
                   $@"<script src=¨../../js/{RutaBase}/ApiDePlanificaciones.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script src=¨../../js/{RutaBase}/PlanificacionDeVenta.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{      
                           {RutaBase}.CrearCrudDePlanificacionesDeVenta('{Mnt.IdHtml}', '{Creador.IdHtml}', '{Editor.IdHtml}', '{Borrado.IdHtml}') 
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
