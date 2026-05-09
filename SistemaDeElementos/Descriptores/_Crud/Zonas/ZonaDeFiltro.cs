using System;
using System.Collections.Generic;
using Utilidades;
using ModeloDeDto;
using ModeloDeDto.Entorno;
using ServicioDeDatos.Elemento;
using GestorDeElementos;
using ModeloDeDto.Terceros;
using MVCSistemaDeElementos.Controllers;
using ModeloDeDto.Negocio;
using ServicioDeDatos;

namespace MVCSistemaDeElementos.Descriptores
{
    public static class ltrBloques
    {
        public static string General = nameof(General);
        public static string Comun = "Común";
    }


    public class ZonaDeFiltro<TElemento> : ControlFiltroHtml where TElemento : ElementoDto
    {
        public bool EsHistorial => typeof(TElemento) == typeof(HistorialDto);

        public DescriptorDeMantenimiento<TElemento> Mnt => (DescriptorDeMantenimiento<TElemento>)Padre;

        public dynamic Historial => Padre;

        public List<BloqueDeFitro<TElemento>> Bloques { get; private set; } = new List<BloqueDeFitro<TElemento>>();
        public List<ModalDeFiltrado<TElemento>> Modales { get; private set; } = new List<ModalDeFiltrado<TElemento>>();

        public EditorFiltro<TElemento> FiltroDeNombre;
        public EditorFiltro<TElemento> FiltroPorArchivo;
        public ListasDinamicas<TElemento> FiltroDeCg;
        public ListaDeValores<TElemento> FiltroPorBaja;
        public ListaDeValores<TElemento> FiltroPorBloqueo;
        public ListaDeValores<TElemento> EtapasDeUnProceso;


        public ZonaDeFiltro(ControlHtml padre)
        : base(
          padre: padre,
          id: $"{padre.Id}_Filtro",
          etiqueta: null,
          propiedad: null,
          ayuda: null,
          posicion: null
        )
        {
            Tipo = enumTipoControl.ZonaDeFiltro;
            var negocioDelDto = NegociosDeSe.NegocioDeUnDto(typeof(TElemento));

            if (EsHistorial)
            {
                FiltrosDeUnHistorial();
            }
            else
            {
                FiltrosDeUnMantenimiento(negocioDelDto);
            }

        }

        private void FiltrosDeUnHistorial()
        {
            var b1 = new BloqueDeFitro<TElemento>(this, ltrBloques.General, new Dimension(1, 1));
            b1.Plegado = false;
            new EditorFiltro<TElemento>(padre: b1, etiqueta: "Suceso", propiedad: ltrFiltros.Nombre, ayuda: "buscar por nombre del susceso", new Posicion(0, 0));
        }

        private void FiltrosDeUnMantenimiento(enumNegocio negocioDelDto)
        {
            var b1 = new BloqueDeFitro<TElemento>(this, ltrBloques.General, new Dimension(2, NegociosDeSe.UsaBaja(negocioDelDto) ? 3 : 2));
            b1.Plegado = false;
            var usaTipo = NegociosDeSe.UsaTipo(negocioDelDto);
            var usaCg = NegociosDeSe.UsaCg(negocioDelDto);

            if (usaCg)
            {
                FiltroDeCg = new ListasDinamicas<TElemento>(b1,
                        etiqueta: "C.G.",
                        filtrarPor: nameof(IUsaCg.IdCg),
                        ayuda: "seleccione el centro gestor",
                        seleccionarDe: nameof(CentroGestorDto),
                        buscarPor: nameof(CentroGestorDto.Nombre),
                        mostrarExpresion: $"([{nameof(CentroGestorDto.Codigo)}]) [{nameof(CentroGestorDto.Nombre)}]",
                        criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                        posicion: new Posicion(0, 0),
                        controlador: nameof(CentrosGestoresController),
                        navegarA: nameof(CentrosGestoresController.CrudCentrosGestores),
                        restringirPor: "",
                        alSeleccionarBlanquearControl: "")
                {
                    LongitudMinimaParaBuscar = 1
                };
            }
            if (usaTipo)
            {
                new ListasDinamicas<TElemento>(b1,
                     etiqueta: "Tipo",
                     filtrarPor: nameof(IUsaTipo.IdTipo),
                     ayuda: "seleccione el tipo",
                     seleccionarDe: nameof(TipoDeElementoDto),
                     buscarPor: nameof(TipoDeElementoDto.Nombre),
                     mostrarExpresion: $"[{nameof(TipoDeElementoDto.Nombre)}]",
                     criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                     posicion: new Posicion(0, 1),
                     controlador: nameof(TiposDeElementoController),
                     navegarA: enumVistasNegocio.CrudDeTipos(Mnt.Crud.Negocio),
                     restringirPor: "",
                     alSeleccionarBlanquearControl: nameof(IUsaEstado.IdEstado))
                {
                    LongitudMinimaParaBuscar = 1,
                    Negocio = Mnt.Crud.Negocio
                };
            }

            FiltroDeNombre = new EditorFiltro<TElemento>(padre: b1, etiqueta: nameof(INombre.Nombre), propiedad: ltrFiltros.Nombre, ayuda: "buscar por nombre", new Posicion { fila = usaTipo ? 1 : 0, columna = 0 });
            if (Mnt.Crud.Negocio.UsaReferencia())
            {
                new EditorFiltro<TElemento>(padre: b1, etiqueta: nameof(IUsaReferenciaDto.Referencia), propiedad: nameof(IUsaReferenciaDto.Referencia), ayuda: $"buscar por referencia (puede buscarpor =) y concatenar usando {Simbolos.separadorDeCadenasDeFiltrado}", new Posicion { fila = usaTipo ? 1 : 0, columna = 1 });
                //Mnt.OrdenacionInicial = @$"{nameof(IUsaReferenciaDto.Referencia)}:{nameof(IUsaReferenciaDto.Referencia)}:{enumModoOrdenacion.descendente.Render()}";
            }
            if (NegociosDeSe.UsaBaja(negocioDelDto))
            {
                var opciones = new Dictionary<string, string> { { $"{ltrParametrosNeg.MostrarBajas}", "los de baja" }, { $"{ltrParametrosNeg.MostrarTodos}", "activos y de baja" } };

                FiltroPorBaja = new ListaDeValores<TElemento>(b1
                    , "Mostrar"
                    , "Solo los elementos activos"
                    , opciones
                    , nameof(ltrParametrosNeg.FiltrarPorBaja)
                    , new Posicion() { fila = 0, columna = !usaTipo && !usaCg ? 1: 2 });
            }

            if (NegociosDeSe.UsaBloqueos(negocioDelDto))
            {
                var opciones = new Dictionary<string, string> { { $"{ltrParametrosNeg.MostrarBloqueados}", "los bloqueados" }, { $"{ltrParametrosNeg.MostrarTodos}", "todos" } };

                FiltroPorBloqueo = new ListaDeValores<TElemento>(b1
                    , ""
                    , "Excluir bloqueados"
                    , opciones
                    , nameof(ltrParametrosNeg.FiltrarPorBloqueo)
                    , new Posicion() { fila = 0, columna = 2 });
            }


            if (NegociosDeSe.UsaHitos(negocioDelDto))
            {
                var modal = new ModalDeFiltrado<TElemento>(this, "proceso", "Filtros del proceso");
                Modales.Add(modal);
                FiltrosDeEstado(modal);
                FiltrosDeAuditoriaDeEstados(modal);
                FiltrosDeTransiciones(modal);
            }


            if (ApiDeInterfaceDto.ImplementaAuditoriaDto(typeof(TElemento)))
            {
                var modal = new ModalDeFiltrado<TElemento>(this, "auditoria", "Filtros de auditoría");
                Modales.Add(modal);
                FiltrosDeAuditoria(modal, ((DescriptorDeMantenimiento<TElemento>)Padre).Crud.Negocio.UsaObservaciones(), ((DescriptorDeMantenimiento<TElemento>)Padre).Crud.Negocio.UsaArchivos());
            }
        }

        private void FiltrosDeTransiciones(ModalDeFiltrado<TElemento> modal)
        {
            var transiciones = new ListasDinamicas<TElemento>(modal,
                 etiqueta: "Transiciones entre fechas",
                 filtrarPor: ltrTransiciones.IdTransicionAuditada,
                 ayuda: "seleccione los procesos a los que se les aplicó la transición ... entre las fechas indicadas",
                 seleccionarDe: nameof(TransicionDto),
                 buscarPor: nameof(TransicionDto.Nombre),
                 mostrarExpresion: $"[{nameof(TransicionDto.Nombre)}]",
                 criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                 posicion: new Posicion(5, 0),
                 controlador: nameof(TransicionesController),
                 navegarA: nameof(TransicionesController.CrudDeTransiciones),
                 restringirPor: "",
                 alSeleccionarBlanquearControl: "")
            {
                LongitudMinimaParaBuscar = 1,
                Negocio = Mnt.Crud.Negocio
            };
            modal.ControlesDeFiltrado.Add(transiciones);

            var fechas = new FiltroEntreFechas<TElemento>(modal,
                                etiqueta: "En las fechas",
                                propiedad: nameof(ltrTransiciones.FechaTransicionAuditado),
                                ayuda: "buscar los procesos a los que se les aplicó la transición entre las fechas",
                                posicion: new Posicion() { fila = 5, columna = 1 },
                                renderEtiqueta: false);
            modal.ControlesDeFiltrado.Add(fechas);
        }

        private void FiltrosDeAuditoriaDeEstados(ModalDeFiltrado<TElemento> modal)
        {
            var estados = new ListasDinamicas<TElemento>(modal,
                   etiqueta: "Estado entre fechas",
                   filtrarPor: ltrEstados.IdEstadoAuditado,
                   ayuda: "seleccione los procesos que estuvieron en el estado ... entre las fechas indicadas",
                   seleccionarDe: nameof(EstadoDto),
                   buscarPor: nameof(EstadoDto.Nombre),
                   mostrarExpresion: $"[{nameof(EstadoDto.Nombre)}]",
                   criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                   posicion: new Posicion(4, 0),
                   controlador: nameof(EstadosController),
                   navegarA: nameof(EstadosController.CrudDeEstados),
                   restringirPor: "",
                   alSeleccionarBlanquearControl: "")
            {
                LongitudMinimaParaBuscar = 1,
                Negocio = Mnt.Crud.Negocio
            };
            modal.ControlesDeFiltrado.Add(estados);

            var fechas = new FiltroEntreFechas<TElemento>(padre: modal,
                                 etiqueta: "En las fechas",
                                 propiedad: nameof(ltrEstados.FechaEstadoAuditado),
                                 ayuda: "buscar los procesos que estuvieron en el estado entre las fechas indicadas",
                                 posicion: new Posicion() { fila = 4, columna = 1 },
                                 renderEtiqueta: false);
            modal.ControlesDeFiltrado.Add(fechas);
        }

        private void FiltrosDeEstado(ModalDeFiltrado<TElemento> modal)
        {
            var estados = new ListasDinamicas<TElemento>(modal,
            etiqueta: "Estado",
            filtrarPor: nameof(IUsaEstado.IdEstado),
            ayuda: "seleccione los procesos que están en el estado ...",
            seleccionarDe: nameof(EstadoDto),
            buscarPor: nameof(EstadoDto.Nombre),
            mostrarExpresion: $"[{nameof(EstadoDto.Nombre)}]",
            criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
            posicion: new Posicion(1, 2),
            controlador: nameof(EstadosController),
            navegarA: nameof(EstadosController.CrudDeEstados),
            restringirPor: "",
            alSeleccionarBlanquearControl: "")
            {
                LongitudMinimaParaBuscar = 1,
                Negocio = Mnt.Crud.Negocio
            };

            var mostrarColores = new CheckFiltro<TElemento>(padre: modal,
                etiqueta: "Mostra colores",
                filtrarPor: "",
                ayuda: "Mostrar colores por estado",
                valorInicial: true,
                filtrarPorFalse: false,
                posicion: new Posicion(0, 1))
            {
                EsOnOff = true,
            };
            mostrarColores.Accion = "onclick = javascript:" + nameof(enumNameSpaceTs.Crud) + "." + nameof(enumFunctionTs.Neg_Tras_Pulsar_Mostrar_Colores) + $"(this)";
            mostrarColores.ModificarId("mostrar-colores");

            modal.ControlesDeFiltrado.Add(new ListaDinamicaConCheck<TElemento>(estados, mostrarColores));

            var opciones = new Dictionary<string, string> {
                     { $"{ltrParametrosNeg.TodosMenosCanceladas}", "todos menos cancelados" }
                    ,  { $"{ltrParametrosNeg.Iniciales}", "los pendientes de iniciar" }
                    , { $"{ltrParametrosNeg.Terminados}", "los terminado" }
                    , { $"{ltrParametrosNeg.Cancelados}", "los cancelados" }
                    , { $"{ltrParametrosNeg.MostrarTodos}", "todos" } };

            EtapasDeUnProceso = new ListaDeValores<TElemento>(modal
                  , "Mostrar"
                  , "Solo los elementos activos"
                  , opciones
                  , nameof(ltrParametrosNeg.QueMostrar)
                  , new Posicion() { fila = 0, columna = 2 });

            var nombreDeEstado = new FiltroConEditor<TElemento>(modal, "Estados", ltrFiltros.Estados, $@"parte del nombre de estados: xxx{Simbolos.separadorDeCadenasDeFiltrado}yyy (si empieza por {Simbolos.filtroPorDistinto} excluye)");

            modal.ControlesDeFiltrado.Add(new FiltroDeListaValoresConNombre<TElemento>(modal, EtapasDeUnProceso, nombreDeEstado));
        }

        private static void FiltrosDeAuditoria(ModalDeFiltrado<TElemento> modal, bool usaObservaciones, bool usaArchivos)
        {
            if (usaObservaciones)
            {
                var anotacion = new FiltroConEditor<TElemento>(modal,
                    etiqueta: "Observación",
                    propiedad: ltrFiltros.Observacion,
                    ayuda: "indique texto de la observacion (asunto o descripción)");
                modal.ControlesDeFiltrado.Add(anotacion);
            }

            if (usaArchivos)
            {
                var nombreArchivo = new FiltroConEditor<TElemento>(modal,
                    etiqueta: "Archivo",
                    propiedad: ltrFiltros.NombreDeArchivo,
                           ayuda: "buscar por nombre de archivo");
                modal.ControlesDeFiltrado.Add(nombreArchivo);
            }

            var creador = new ListasDinamicas<TElemento>(modal,
                 etiqueta: "Creador",
                 filtrarPor: nameof(ElementoDtm.IdUsuaCrea),
                 ayuda: $"seleccione el usuario creador",
                 seleccionarDe: nameof(UsuarioDto),
                 buscarPor: nameof(UsuarioDto.NombreCompleto),
                 mostrarExpresion: $"[{nameof(UsuarioDto.NombreCompleto)}]",
                 criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                 posicion: new Posicion(0, 1),
                 controlador: nameof(UsuariosController),
                 navegarA: nameof(UsuariosController.CrudUsuario),
                 restringirPor: "",
                 alSeleccionarBlanquearControl: "");
            creador.LongitudMinimaParaBuscar = 1;
            modal.ControlesDeFiltrado.Add(creador);

            var modificador = new ListasDinamicas<TElemento>(modal,
                 etiqueta: "Modificador",
                 filtrarPor: nameof(ElementoDtm.IdUsuaModi),
                 ayuda: $"seleccione el usuario modificador",
                 seleccionarDe: nameof(UsuarioDto),
                 buscarPor: nameof(UsuarioDto.NombreCompleto),
                 mostrarExpresion: $"[{nameof(UsuarioDto.NombreCompleto)}]",
                 criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                 posicion: new Posicion(0, 1),
                 controlador: nameof(UsuariosController),
                 navegarA: nameof(UsuariosController.CrudUsuario),
                 restringirPor: "",
                 alSeleccionarBlanquearControl: "");
            modificador.LongitudMinimaParaBuscar = 1;
            modal.ControlesDeFiltrado.Add(modificador);

            var fechasDeCreacion = new FiltroEntreFechas<TElemento>(modal,
                                etiqueta: "Creado entre",
                                propiedad: nameof(ElementoDtm.FechaCreacion),
                                ayuda: "filtrar por rango de fechas",
                                posicion: new Posicion() { fila = 1, columna = 1 });
            modal.ControlesDeFiltrado.Add(fechasDeCreacion);

            var fechasDeModificacion = new FiltroEntreFechas<TElemento>(modal,
                                etiqueta: "Modificado entre",
                                propiedad: nameof(ElementoDtm.FechaModificacion),
                                ayuda: "filtrar por rango de fechas",
                                posicion: new Posicion() { fila = 1, columna = 1 });
            modal.ControlesDeFiltrado.Add(fechasDeModificacion);

            //var accionCreacion = $"onclick = javascript:{enumNameSpaceTs.Crud}.{enumGestorDeEventos.EventosDelMantenimiento}('{eventosDeMnt.OcultarMostrarColumnas}','{nameof(IAuditadoDto.CreadoEl)}{Simbolos.separadorDeColumnas}{nameof(IAuditadoDto.Creador)}');";
            //var columnaCreacion = new CheckDeMostrarColumna<TElemento>(modal,
            //    etiqueta: "Auditoría de creación",
            //    ayuda: "muestra la fecha y usuario de creación",
            //    valorInicial: false,
            //    columna: "auditoria-de-creacion",
            //    columnas: null,
            //    accion: accionCreacion);
            //modal.ControlesDeFiltrado.Add(columnaCreacion);

            //var accionModificacion = $"onclick = javascript:{enumNameSpaceTs.Crud}.{enumGestorDeEventos.EventosDelMantenimiento}('{eventosDeMnt.OcultarMostrarColumnas}','{nameof(IAuditadoDto.ModificadoEl)}{Simbolos.separadorDeColumnas}{nameof(IAuditadoDto.Modificador)}');";
            //var columnaModificacion = new CheckDeMostrarColumna<TElemento>(modal,
            //    etiqueta: "Auditoría de modificación",
            //    ayuda: "muestra la última fecha y usuario de modificación",
            //    valorInicial: false,
            //    columna: "auditoria-de-modificacion",
            //    columnas: null,
            //    accion: accionModificacion);
            //modal.ControlesDeFiltrado.Add(columnaModificacion);
        }

        public ControlFiltroHtml BuscarControlPorPropiedad(string propiedad)
        {
            ControlFiltroHtml c = null;
            foreach (var b in Bloques)
            {
                c = b.BuscarControlPorPropiedad(propiedad);
                if (c != null)
                    return c;
            }
            return c;
        }

        public ControlFiltroHtml BuscarControlPorEtiqueta(string etiqueta)
        {
            ControlFiltroHtml c = null;
            foreach (var b in Bloques)
            {
                c = b.BuscarControlPorEtiqueta(etiqueta);
                if (c != null)
                    return c;
            }
            return c;
        }


        public void AnadirBloque(BloqueDeFitro<TElemento> bloque)
        {
            if (!EstaElBloqueAnadido(bloque.Etiqueta))
                Bloques.Add(bloque);
        }

        public BloqueDeFitro<TElemento> ObtenerBloque(string identificador)
        {
            foreach (BloqueDeFitro<TElemento> b in Bloques)
            {
                if (b.Id == identificador)
                    return b;
            }

            throw new Exception($"El bloque {identificador} no está en la zona de filtrado");
        }


        public BloqueDeFitro<TElemento> ObtenerBloquePorEtiqueta(string etiqueta)
        {
            foreach (BloqueDeFitro<TElemento> b in Bloques)
            {
                if (b.Etiqueta == etiqueta)
                    return b;
            }

            return new BloqueDeFitro<TElemento>(this, etiqueta, new Dimension(2, 2));
        }

        private bool EstaElBloqueAnadido(string etiqueta)
        {
            foreach (BloqueDeFitro<TElemento> b in Bloques)
            {
                if (b.Etiqueta == etiqueta)
                    return true;
            }
            return false;
        }

        public string RenderModalesDeSeleccion()
        {
            var htmlModalesEnFiltro = "";
            foreach (BloqueDeFitro<TElemento> b in Bloques)
                htmlModalesEnFiltro = $"{htmlModalesEnFiltro}{b.RenderModalesBloque()}";

            return htmlModalesEnFiltro;
        }

        public string RenderFiltroDeUnaModal(enumTipoDeModal tipoDeModal)
        {
            string evento;
            switch (tipoDeModal)
            {
                case enumTipoDeModal.ModalDeSeleccion:
                    evento = $"javascript:Crud.{enumGestorDeEventos.EventosModalDeSeleccion}('{eventosDeMnt.TeclaPulsada}', '{Mnt.Datos.IdHtmlModal}');";
                    break;
                case enumTipoDeModal.ModalDeRelacion:
                    evento = $"javascript:Crud.{enumGestorDeEventos.EventosModalDeCrearRelaciones}('{eventosDeMnt.TeclaPulsada}', '{Mnt.Datos.IdHtmlModal}');";
                    break;
                case enumTipoDeModal.ModalParaImputar:
                    evento = $"javascript:Crud.{enumGestorDeEventos.EventosModalParaImputar}('{eventosDeMnt.TeclaPulsada}', '{Mnt.Datos.IdHtmlModal}');";
                    break;
                case enumTipoDeModal.ModalDeConsulta:
                    evento = $"javascript:Crud.{enumGestorDeEventos.EventosModalDeConsultaDeRelaciones}('{eventosDeMnt.TeclaPulsada}', '{Mnt.Datos.IdHtmlModal}');";
                    break;
                case enumTipoDeModal.ModalParaSeleccionar:
                    evento = $"javascript:Crud.{enumGestorDeEventos.EventosModalParaSeleccionar}('{eventosDeMnt.TeclaPulsada}', '{Mnt.Datos.IdHtmlModal}');";
                    break;
                default:
                    throw new Exception($"Ha de definir el evento de pulsar una tecla para la modal del tipo {tipoDeModal}");
            }
            return RenderControl().Replace("eventoTeclaPulsada", evento);
        }

        public string RenderZonaDeFiltroNoModal()
        {
            var evento = $"javascript:{enumNameSpaceTs.Crud}.{enumGestorDeEventos.EventosDelMantenimiento}('{eventosDeMnt.TeclaPulsada}', '');";
            var h = RenderControl().Replace("eventoTeclaPulsada", evento);
            return h;

        }


        public override string RenderControl()
        {
            var numeroBloques = 0;
            var areas = "";
            foreach (BloqueDeFitro<TElemento> b in Bloques)
                if (b.Tabla.Controles.Count > 0)
                    numeroBloques = numeroBloques + 1;
            //numeroBloques = Modales.Count == 0 ? numeroBloques : 1 + numeroBloques;
            var tamano = 1.00 / numeroBloques;
            var tamanos = "";

            if (Modales.Count > 0)
            {
                areas = $"'{enumCssCuerpo.CuerpoDatosFiltroReferencias.Render()}'";
                tamanos = $"auto";
            }

            foreach (BloqueDeFitro<TElemento> b in Bloques)
            {
                if (b.Tabla.Controles.Count > 0)
                {
                    //numeroBloques = numeroBloques + 1;
                    if (areas.IsNullOrEmpty())
                    {
                        areas = $"'{enumCssCuerpo.CuerpoDatosFiltroBloque.Render()}'";
                        tamanos = $"{tamano}fr";
                    }
                    else
                    {
                        areas = $"{areas} '{enumCssCuerpo.CuerpoDatosFiltroBloque.Render()}'";
                        tamanos = $"{tamanos} {tamano}fr";
                    }
                }
            }

            var estilo = $@"style = ¨grid-template-rows: {tamanos.Replace(',', '.')}; grid-template-areas: {areas};¨";

            var renderModales = EsHistorial
                ? ""
                : Mnt.Crud.EsModal
                ? ""
                : (Modales.Count > 0 ? RenderBloqueParaAbrirLasModalesDeFiltrado() : "");

            return $@"<!-- ******************* Filtro ******************* -->
                      <div id = ¨{IdHtml}¨ class=¨{Css.Render(enumCssCuerpo.CuerpoDatosFiltro)}¨ onkeypress=¨eventoTeclaPulsada¨ {estilo}>  
                         {renderModales}
                         {RenderDeBloquesDeFiltro()}
                      </div> 
                     ";
        }

        private string RenderDeBloquesDeFiltro()
        {
            var htmlBloques = "";

            for (var i = 0; i < Bloques.Count; i++)
            {
                var bloque = Bloques[i];
                if (bloque.Tabla.Controles.Count > 0)
                    htmlBloques = $"{htmlBloques}{Environment.NewLine}{bloque.RenderControl()}";
            }

            return htmlBloques;
        }

        public string RenderModalesDeFiltro()
        {
            var htmlModales = "";

            for (var i = 0; i < Modales.Count; i++)
            {
                var modal = Modales[i];
                htmlModales = $"{htmlModales}{Environment.NewLine}{modal.RenderModalDeFiltrado()}";
            }

            return htmlModales;
        }


        private string RenderBloqueParaAbrirLasModalesDeFiltrado()
        {
            var htmlReferencias = "";
            for (var i = 0; i < Modales.Count; i++)
            {
                var modal = Modales[i];
                htmlReferencias = $"{htmlReferencias}{Environment.NewLine}{modal.RenderReferenciaParaAbrirModalDeFiltrado()}";
            }
            var bloque = $@"
                <div id='mostrar.{IdHtml}' class='{enumCssCuerpo.CuerpoDatosFiltroReferencias.Render()}'> 
                   {htmlReferencias}
                 </div>";

            return bloque;
        }
    }

}
