using DocumentFormat.OpenXml.Drawing;
using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Entorno;
using GestoresDeNegocio.Negocio;
using GestoresDeNegocio.TrabajosSometidos;
using ModeloDeDto;
using ModeloDeDto.Entorno;
using ModeloDeDto.Negocio;
using ModeloDeDto.SistemaDocumental;
using ModeloDeDto.Tarea;
using ModeloDeDto.Terceros;
using MVCSistemaDeElementos.Controllers;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.Seguridad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Utilidades;
using UtilidadesParaIu;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeCrud<TElemento> : ControlHtml where TElemento : ElementoDto
    {
        internal static string NombreCrud = $"Crud_{typeof(TElemento).Name}";

        internal static string menuContextual = "menu.contextual";
        internal static string menuIndividual = "menu.individual";
        internal static string menuDeRelaciones = "menu.de.relaciones";
        internal static string menuEdicion = "menu.edicion";
        internal static string menuCreacion = "menu.creacion";
        internal static string menuDeFiltro = "menu.de.filtro";
        internal static string menuHistorial = "menu.de.historial";
        internal static string menuDeDetalles = "menu.de.detalle";


        private List<PropiedaJson> _atributosJson = null;
        internal List<PropiedaJson> AtributosJson
        {
            get
            {
                if (_atributosJson == null)
                    _atributosJson = ApiClasesComunes.ObtenerAtributosJson(typeof(TElemento), enumRutas.RutaDeJson, ServicioDeCaches.UsaCacheParaRenderizar);
                return _atributosJson;
            }
        }


        public string Vista { get; private set; }

        public int IdVista => VistaDtm.Id;
        public VistaMvcDtm VistaDtm => Contexto.SeleccionarPorPropiedad<VistaMvcDtm>(nameof(VistaMvcDtm.Accion), Vista);

        public Type Dto => typeof(TElemento);

        public Type Dtm => Negocio == enumNegocio.No_Definido ? ExtensionesDto.TipoDtm<TElemento>(emitirError: false) : Negocio.TipoDtm();

        public List<ControlHtml> modalesParaPedirDatos = new List<ControlHtml>();

        public DescriptorDeMantenimiento<TElemento> Mnt { get; private set; }
        public DescriptorDeHistorial<TElemento, HistorialDto> Historial { get; private set; }
        public DescriptorDeCreacion<TElemento> Creador { get; private set; }
        public DescriptorDeEdicion<TElemento> Editor { get; private set; }
        public DescriptorDeExportacion<TElemento> Exportador { get; private set; }
        public DescriptorDeEnviarCorreo<TElemento> ModalDeEnviarCorreo { get; private set; }
        public DescriptorDeTransitar<TElemento> ModalDeTransitar { get; private set; }
        public DescriptorDeImprimir<TElemento> ModalDeImpresion { get; private set; }
        public DescriptorDeBorrado<TElemento> Borrado { get; private set; }

        private string _controlador = null;
        public string Controlador { get { return _controlador.Replace(ltrEndPoint.Controller, ""); } private set { _controlador = value; } }
        public bool UsaCompartir { get; private set; }
        public ContextoSe Contexto { get; }
        public ModoDescriptor Modo { get; private set; }

        public bool EliminarCreacion { get; private set; }

        public bool MantenimientoSoloConGrid => (bool)ApiDeAtributos.ValorDelAtributo(typeof(TElemento), nameof(IUDtoAttribute.SoloGrid));

        public bool SinCreacion => !NegocioActivo ||
            Modo == ModoDescriptor.Consulta ||
            !(bool)ApiDeAtributos.ValorDelAtributo(typeof(TElemento), nameof(IUDtoAttribute.OpcionDeCrear)) ||
            EliminarCreacion;

        public bool SinEdicion => !Mnt.Crud.NegocioActivo || Mnt.Crud.Modo == ModoDescriptor.Consulta || !(bool)ApiDeAtributos.ValorDelAtributo(typeof(TElemento), nameof(IUDtoAttribute.OpcionDeEditar));

        public bool EsModal => Modo != ModoDescriptor.Mantenimiento;

        public enumNameSpaceTs RutaBase { get; set; }
        public UsuarioDtm UsuarioConectado { get; internal set; }
        public GestorDeUsuarios GestorDeUsuario { get; internal set; }
        public GestorDeNegocios GestorDeNegocio => GestorDeNegocios.Gestor(Contexto, Contexto.Mapeador);
        public bool NegocioActivo => _negocio.Activo();

        private enumNegocio _negocio = enumNegocio.No_Definido;
        public enumNegocio Negocio
        {
            get
            {
                if (_negocio == enumNegocio.No_Definido)
                {
                    _negocio = NegociosDeSe.NegocioDeUnDto(typeof(TElemento));
                }
                return _negocio;

            }
            internal set { _negocio = value; }
        }

        public bool HayTablaConGraficos { get; set; }

        public string RenderDto => typeof(TElemento).FullName;

        protected string _renderCache { get; set; } 

        public string RenderNegocio => NegocioDtm == null ? Negocio.ToNombre() : NegocioDtm.Nombre;
        public int RenderIdDeNegocio => NegocioDtm == null ? 0 : NegocioDtm.Id;

        public NegocioDtm NegocioDtm { get; internal set; }


        public static DescriptorDeCrud<TElemento> CrearDescriptor(ContextoSe contexto, ModoDescriptor modo, Func<DescriptorDeCrud<TElemento>> constructor, int vecesEjecutado = 0)
        {
            try
            {
                return constructor();
            }
            catch(Exception e)
            {
                if (e.Message.Contains(" ya está asignado") && vecesEjecutado < 3)
                {
                    return CrearDescriptor(contexto, modo, constructor, vecesEjecutado++);
                }
                throw;
            }
        }

        public DescriptorDeCrud(ContextoSe contexto, string renderCache) : base()
        {
            Contexto = contexto;

            if (renderCache.StartsWith("<input id=error>"))
            {
                var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
                ServicioDeCaches.EliminarElemento(CacheDe.RenderCrud, indice);
                contexto.RegistrarConEnvio(asunto: $"Error al renderizar {GetType().Name}", error: $"{indice}{Environment.NewLine}{renderCache}");
                throw new Exception(renderCache);
            }

            RenderizarUltimosMenus(contexto, renderCache);
            RenderizarUltimosRegistros(contexto, renderCache);
        }

        private void RenderizarUltimosMenus(ContextoSe contexto, string renderCache)
        {
            var accesos = GestorDeAccesosRecientes.LeerAccesosRecientes(contexto, enumClaseDeAcceso.Menu);

            var opciones = $@"<ul id='{ltrPanelDeControl.IdUltimosMenusAccedidos}'>{string.Join(Environment.NewLine, accesos.Select(a => a.OpcionHtml))}</ul>";

            var ulPattern = @$"<ul id='{ltrPanelDeControl.IdUltimosMenusAccedidos}'>.*?</ul>";  // Patrón para capturar todo el bloque UL, sin modo dotall

            // Para el .*? funcione en múltiples líneas, debemos usar RegexOptions.Singleline

            if (Regex.IsMatch(renderCache, ulPattern, RegexOptions.Singleline))
            {
                // Reemplazar el bloque UL completo por nuestras opciones
                _renderCache = Regex.Replace(
                    renderCache,
                    ulPattern,
                    opciones,
                    RegexOptions.Singleline);
            }
            else
            {
                // No se encontró el bloque UL, sustituir la marca como antes
                _renderCache = accesos.Count > 0
                    ? renderCache.Replace(ltrPanelDeControl.UltimosMenusAccedidos, opciones)
                    : renderCache.Replace(ltrPanelDeControl.UltimosMenusAccedidos, "");
            }
        }

        private void RenderizarUltimosRegistros(ContextoSe contexto, string renderCache)
        {
            var registros = GestorDeAccesosRecientes.LeerAccesosRecientes(contexto, enumClaseDeAcceso.Registros);

            var opciones = $@"<ul id='{ltrPanelDeControl.IdUltimosRegistros}'>{string.Join(Environment.NewLine, registros.Select(a => a.OpcionHtml))}</ul>";

            var ulPattern = @$"<ul id='{ltrPanelDeControl.IdUltimosRegistros}'>.*?</ul>";  // Patrón para capturar todo el bloque UL, sin modo dotall

            // Para el .*? funcione en múltiples líneas, debemos usar RegexOptions.Singleline

            if (Regex.IsMatch(renderCache, ulPattern, RegexOptions.Singleline))
            {
                // Reemplazar el bloque UL completo por nuestras opciones
                _renderCache = Regex.Replace(
                    renderCache,
                    ulPattern,
                    opciones,
                    RegexOptions.Singleline);
            }
            else
            {
                // No se encontró el bloque UL, sustituir la marca como antes
                _renderCache = registros.Count > 0
                    ? renderCache.Replace(ltrPanelDeControl.UltimosRegistros, opciones)
                    : renderCache.Replace(ltrPanelDeControl.UltimosRegistros, "");
            }
        }

        public DescriptorDeCrud(ContextoSe contexto, string controlador, string vista, ModoDescriptor modo, enumNameSpaceTs rutaBase, enumNegocio negocio, string titulo = null)
        : base(
          padre: null,
          id: $"CRUD_{negocio}",
          etiqueta: titulo.IsNullOrEmpty() ? typeof(TElemento).Name.Replace("Dto", "") : titulo,
          propiedad: null,
          ayuda: null,
          posicion: null
        )
        {
            Contexto = contexto;
            RutaBase = rutaBase;
            Tipo = enumTipoControl.DescriptorDeCrud;
            Negocio = negocio;
            Controlador = controlador.Replace(ltrEndPoint.Controller, "");
            Vista = vista;
            Modo = modo;

            NegocioDtm = Negocio == enumNegocio.No_Definido ? null : GestorDeNegocios.LeerNegocio(contexto, Negocio);
        }
        
        public DescriptorDeCrud(ContextoSe contexto, string controlador, string vista, ModoDescriptor modo, enumNameSpaceTs rutaBase, string id = null, string tituloPlural = "", string tituloSingular = "", bool eliminarCreacion = false)
        : base(
          padre: null,
          id: id == null ? $"{NombreCrud}" : id,
          etiqueta: typeof(TElemento).Name.Replace("Dto", ""),
          propiedad: null,
          ayuda: null,
          posicion: null,
          resetearListaDeIds: true
        )
        {
            Contexto = contexto;

            var titulo = Negocio == enumNegocio.No_Definido
                ? tituloSingular.IsNullOrEmpty()
                ? typeof(TElemento).Name.Replace("Dto", "")
                : tituloSingular
                : Negocio.Singular();

            RutaBase = rutaBase;
            EliminarCreacion = eliminarCreacion;
            Tipo = enumTipoControl.DescriptorDeCrud;
            Vista = vista;
            Modo = modo;
            Controlador = controlador.Replace(ltrEndPoint.Controller, "");
            Mnt = new DescriptorDeMantenimiento<TElemento>(crud: this, etiqueta: tituloPlural);
            Mnt.IaTitulo = $"Resumir usando: '{ExtensorDeUsuarios.IaUsada(contexto).Nombre}'";

            Negocio = NegociosDeSe.NegocioDeUnDto(typeof(TElemento));
            NegocioDtm = Negocio == enumNegocio.No_Definido ? null : GestorDeNegocios.LeerNegocio(contexto, NegociosDeSe.NegocioDeUnDto(typeof(TElemento)));

            DefinirColumnasDelGrid();
            Historial = typeof(TElemento).ImplementaProcesoDto() && Modo == ModoDescriptor.Mantenimiento
            ? new DescriptorDeHistorial<TElemento, HistorialDto>(crud: this, $"Historial de: [Referencia]")
            : null;


            Creador = /*SinCreacion ? null:*/ new DescriptorDeCreacion<TElemento>(crud: this, etiqueta: $"Crear: {titulo}");
            Editor = /*SinEdicion ? null:*/ new DescriptorDeEdicion<TElemento>(crud: this, etiqueta: $"Editar: {titulo}");
            Exportador = new DescriptorDeExportacion<TElemento>(crud: this);

            if (modo == ModoDescriptor.Mantenimiento)
            {
                Mnt.ZonaMenu.AnadirOpcionDeIrACrear();
                Mnt.ZonaMenu.AnadirOpcionDeIrAEditar();
                HayTablaConGraficos = Negocio != enumNegocio.No_Definido && !Mnt.Crud.EsModal && Negocio.TipoDtm().ImplementaUnElemento();
                if (Negocio != enumNegocio.No_Definido && (Negocio.UsaTipo() || Negocio.TipoDtm().ImplementaDatosDeContacto() || Editor?.PermiteConsultasConGuid == true))
                {
                    modalesParaPedirDatos.Add(new ModalParaPedirDatos(this, typeof(EnviarElementoDto), eventosDeMf.Comun_EnviarElemento, "Enviar elemento"));
                    UsaCompartir = true;
                }

                if (Historial is not null)
                {
                    Mnt.ZonaMenu.AnadirOpcionDeIrAHistorial();
                }
                
                //if ((bool)ApiDeAtributos.ValorDelAtributo(typeof(TElemento), nameof(IUDtoAttribute.OpcionDeEnviar)) &&
                //    GestorDeCorreos.HayVistaParaMostrarElDto<TElemento>())
                //    ModalDeEnviarCorreo = new DescriptorDeEnviarCorreo<TElemento>(crud: this);
               
                
                Borrado = new DescriptorDeBorrado<TElemento>(crud: this, etiqueta: $"Eliminar: {titulo}");
                if (!typeof(TElemento).ImplementaProcesoDto()) Mnt.ZonaMenu.AnadirOpcionDeBorrar();

                DefinirDescriptoresDeNegocio();
            }

        }
        private void DefinirDescriptoresDeNegocio()
        {
            if (Negocio == enumNegocio.No_Definido)
                return;

            if (NegociosDeSe.UsaInterlocutores(Negocio)) DefinirDescriptorDeSolicitantes();
            if (NegociosDeSe.UsaHitos(Negocio)) DefinirDescriptorDeHitos();
            if (NegociosDeSe.UsaObservaciones(Negocio)) new EspansorDeObservaciones(Editor).DefinirDescriptorDeObservacion();
            if (NegociosDeSe.UsaDirecciones(Negocio)) DefinirDescriptorDeDirecciones();
            if (NegociosDeSe.UsaArchivadores(Negocio)) DefinirDescriptorDeArchivadores();
            if (NegociosDeSe.UsaTareas(Negocio)) DefinirDescriptorDeTareas();
            if (NegociosDeSe.UsaAgenda(Negocio)) new EspansorDeEventosDeAgenda(Editor).DefinirDescriptorDeEventos();
            if (NegociosDeSe.UsaArchivos(Negocio)) DefinirDescriptorDeArchivos();
            if (NegociosDeSe.UsaTrazas(Negocio)) DefinirDescriptorDeTrazas();
            if (NegociosDeSe.UsaAuditoria(Negocio)) DefinirDescriptorDeAuditoria();
            if (NegociosDeSe.UsaHitos(Negocio)) ModalDeTransitar = new DescriptorDeTransitar<TElemento>(crud: this);
            ModalDeImpresion = new DescriptorDeImprimir<TElemento>(crud: this);
        }

        private void DefinirDescriptorDeHitos()
        {
            var expansor = new DescriptorDeExpansor(Editor, $"{Editor.Id}-hitos", "Hitos", true, "Hitos de un elemento");
            Editor.Expanes.Add(expansor);

            //Definimos el grid de detalles del cuerpo
            var columnas = new DescriptorDeColumnas("hitos");
            columnas.Add(nameof(HitoDto.Estado));
            columnas.Add(titulo: "Asignado por", propiedad: nameof(HitoDto.Usuario), tamano: 200);
            columnas.Add(titulo: nameof(HitoDto.Fecha), formato: enumFormato.FechaTiempo);
            columnas.Add(titulo: "Transición", propiedad: nameof(HitoDto.Transicion));
            columnas.Add(titulo: "idObservacion", propiedad: nameof(HitoDto.IdObservacion), mostrar: false);
            columnas.Add(titulo: nameof(HitoDto.Id), mostrar: false);
            var parametros = new Dictionary<string, object> {
                   { nameof(GridDeRelacion.Controlador), typeof(HitosController) }
                 , { nameof(GridDeRelacion.AccionDeConsulta), nameof(HitosController.epLeerElementos)}
                 , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(HitoDtm.IdElemento) }
                };
            var gridDeRelacion = new GridDeRelacion(expansor, columnas, parametros);
            gridDeRelacion.PermitirBorrar = false;
        }

        private void DefinirDescriptorDeTrazas()
        {
            var expansor = new DescriptorDeExpansor(Editor, $"{Editor.Id}-trazas", "Trazas", true, "Trazas de un elemento");
            Editor.Expanes.Add(expansor);

            //Definimos el grid de detalles del cuerpo
            var columnasDeCps = new DescriptorDeColumnas("trazas");
            columnasDeCps.Add(titulo: "Traza", propiedad: nameof(TrazaDto.Nombre));
            columnasDeCps.Add(titulo: "Creada por", propiedad: nameof(TrazaDto.Creador));
            columnasDeCps.Add(titulo: "Creada el", propiedad: nameof(TrazaDto.CreadaEl), formato: enumFormato.FechaTiempo);
            columnasDeCps.Add(titulo: nameof(TrazaDto.Id), mostrar: false);
            var parametros = new Dictionary<string, object> {
                   { nameof(GridDeRelacion.Controlador), typeof(TrazasController) }
                 , { nameof(GridDeRelacion.AccionDeConsulta), nameof(TrazasController.epLeerElementos)}
                 , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(TrazaDtm.IdElemento) }
                 , { nameof(GridDeRelacion.OcultarSiVacio), true }
                };
            var gridDeRelacion = new GridDeRelacion(expansor, columnasDeCps, parametros);
            gridDeRelacion.PermitirBorrar = false;

            expansor.DescriptorDeEditarRelaciones(Editor.Crud.Contexto, typeof(TrazaDto), typeof(TrazasController), "Editar traza", soloConsulta: true);
        }

        //private void DefinirDescriptorDeObservacion()
        //{
        //    var expansor = new DescriptorDeExpansor(Editor, $"{Editor.Id}-observaciones", "Observaciones", true, "Observaciones de usuario");
        //    Editor.Expanes.Add(expansor);

        //    //Definimos el grid de detalles del cuerpo
        //    var columnasDeCps = new DescriptorDeColumnas("observaciones");
        //    columnasDeCps.Add(titulo: "Asunto", propiedad: nameof(ObservacionDto.Nombre), alineacion: enumAliniacion.izquierda, mostrar: true);
        //    columnasDeCps.Add(titulo: "Creada por", propiedad: nameof(ObservacionDto.Creador), alineacion: enumAliniacion.izquierda, mostrar: true);
        //    columnasDeCps.Add(titulo: "Creada el", propiedad: nameof(ObservacionDto.CreadaEl), formato: enumFormato.FechaTiempo);
        //    columnasDeCps.Add(titulo: "Id", propiedad: nameof(ObservacionDto.Id), alineacion: enumAliniacion.derecha, mostrar: false);
        //    var parametros = new Dictionary<string, object> {
        //           { nameof(GridDeRelacion.Controlador), typeof(ObservacionesController) }
        //         , { nameof(GridDeRelacion.AccionDeConsulta), nameof(ObservacionesController.epLeerElementos)}
        //         , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(ObservacionDtm.IdElemento) }
        //         , { nameof(GridDeRelacion.OcultarSiVacio), true}
        //        };
        //    var gridDeRelacion = new GridDeRelacion(expansor, columnasDeCps, parametros);
        //    gridDeRelacion.PermitirBorrar = false;

        //    expansor.DescriptorDeCrearRelaciones(Editor.Crud.Contexto, typeof(ObservacionDto), typeof(ObservacionesController), nameof(ObservacionDto.IdElemento), "Añadir observación");
        //    expansor.DescriptorDeEditarRelaciones(Editor.Crud.Contexto, typeof(ObservacionDto), typeof(ObservacionesController), "Editar observacion", soloConsulta: false);
        //}

        private void DefinirDescriptorDeDirecciones()
        {
            var expansor = new DescriptorDeExpansor(Editor, $"{Editor.Id}-direcciones", "Direcciones", true, "Direcciones del elemento");
            Editor.Expanes.Add(expansor);

            //Definimos el grid de detalles del cuerpo
            var columnas = new DescriptorDeColumnas("direcciones");
            columnas.Add(titulo: "Dirección", propiedad: nameof(DireccionDto.Expresion), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnas.Add(titulo: "Activa", propiedad: nameof(DireccionDto.Activo), alineacion: enumAliniacion.derecha, mostrar: true, tamano: 130);
            columnas.Add(titulo: "Calle", propiedad: nameof(DireccionDto.Calle), mostrar: false);
            columnas.Add(titulo: "Numero", propiedad: nameof(DireccionDto.Numero), mostrar: false);
            columnas.Add(titulo: "Cp", propiedad: nameof(DireccionDto.CodigoPostal), mostrar: false);
            columnas.Add(titulo: "Municipio", propiedad: nameof(DireccionDto.Municipio), mostrar: false);
            columnas.Add(titulo: "NombreDireccion", propiedad: nameof(DireccionDto.NombreDireccion), mostrar: false);
            columnas.Add(titulo: "Url", propiedad: nameof(DireccionDto.Url), mostrar: false);
            columnas.Add(titulo: "Id", propiedad: nameof(DireccionDto.Id), alineacion: enumAliniacion.derecha, mostrar: false);
            var parametros = new Dictionary<string, object> {
                   { nameof(GridDeRelacion.Controlador), typeof(DireccionesController) }
                 , { nameof(GridDeRelacion.AccionDeConsulta), nameof(DireccionesController.epLeerElementos)}
                 , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(DireccionDtm.IdElemento) }
                 , { nameof(GridDeRelacion.PaginaDondeNavegarAlEditar) , "MostrarDireccion"}
                 , { nameof(GridDeRelacion.OcultarSiVacio), true}
                };
            var gridDeRelacion = new GridDeRelacion(expansor, columnas, parametros);
            gridDeRelacion.PermitirBorrar = false;
            gridDeRelacion.PermitirEditar = true;

            expansor.DescriptorDeCrearRelaciones(Editor.Crud.Contexto, typeof(DireccionDto), typeof(DireccionesController), nameof(DireccionDto.IdElemento), "Añadir dirección");
            var modal = expansor.DescriptorDeEditarRelaciones(Editor.Crud.Contexto, typeof(DireccionDto), typeof(DireccionesController), "Editar dirección", false);
            modal.AccionTrasAbrirModal = $"javascript: ApiDeInicializacion.InicializarLinkDeDireccion('{gridDeRelacion.idModalParaEditar.ToLower()}')";
        }

        private void DefinirDescriptorDeSolicitantes()
        {
            var expansor = new DescriptorDeExpansor(Editor, $"{Editor.Id}-solicitantes", "Solicitantes", true, "Solicitantes asociados");
            Editor.Expanes.Add(expansor);

            //Definimos el grid de detalles del cuerpo
            var columnas = new DescriptorDeColumnas("solicitantes");
            columnas.Add(titulo: "Solicitante", propiedad: nameof(InterlocutorDto.Expresion), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnas.Add(titulo: "Correo", propiedad: nameof(InterlocutorDto.eMail), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnas.Add(titulo: "Teléfono", propiedad: nameof(InterlocutorDto.Telefono), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnas.Add(titulo: "Id", propiedad: nameof(InterlocutorDto.Id), alineacion: enumAliniacion.derecha, mostrar: false);
            var parametros = new Dictionary<string, object> {
                   { nameof(GridDeRelacion.Controlador), typeof(InterlocutoresController) }
                 , { nameof(GridDeRelacion.AccionDeConsulta), nameof(BaseController<TElemento>.epLeerVinculosCon)}
                 , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(VinculoDtm.idElemento1) }
                 , { nameof(ltrParametrosEp.idVinculado) , NegociosDeSe.IdNegocio(enumNegocio.Interlocutor) }
                 , { nameof(GridDeRelacion.PaginaDondeNavegarAlEditar) , $"{nameof(InterlocutoresController)}/{nameof(InterlocutoresController.CrudInterlocutores)}"}
                 , { nameof(GridDeRelacion.OcultarSiVacio), false}
                };
            var gridDeRelacion = new GridDeRelacion(expansor, columnas, parametros);
            gridDeRelacion.PermitirBorrar = true;

            expansor.DescriptorParaVincular(Editor.Crud.Contexto, NegociosDeSe.IdNegocio(enumNegocio.Interlocutor), typeof(SelectorDeInterDto), nameof(InterlocutoresController), "Añadir un solicitante");
            expansor.DescriptorDeEditarRelaciones(Editor.Crud.Contexto, typeof(InterlocutorDto), typeof(InterlocutoresController), "Editar solicitante", true);
        }

        private void DefinirDescriptorDeTareas()
        {
            var expansor = new DescriptorDeExpansor(Editor, $"{Editor.Id}-tareas", "Tareas", true, "Tareas del expediente");
            Editor.Expanes.Insert(0, expansor);

            //Definimos el grid de detalles del cuerpo
            var columnas = new DescriptorDeColumnas("tareas");
            columnas.Add(titulo: "Tarea", propiedad: nameof(TareaDto.Expresion), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnas.Add(titulo: "Tipo", propiedad: nameof(TareaDto.Tipo), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnas.Add(titulo: "Estado", propiedad: nameof(TareaDto.Estado), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnas.Add(titulo: "Id", propiedad: nameof(TareaDto.Id), alineacion: enumAliniacion.derecha, mostrar: false);
            var parametros = new Dictionary<string, object> {
                   { nameof(GridDeRelacion.Controlador), typeof(TareasController) }
                 , { nameof(GridDeRelacion.AccionDeConsulta), nameof(BaseController<TElemento>.epLeerVinculosCon)}
                 , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(VinculoDtm.idElemento1) }
                 , { nameof(ltrParametrosEp.idVinculado) , NegociosDeSe.IdNegocio(enumNegocio.Tarea) }
                 , { nameof(GridDeRelacion.PaginaDondeNavegarAlEditar) , $"{nameof(TareasController)}/{nameof(TareasController.CrudTareas)}"}
                 , { nameof(GridDeRelacion.OcultarSiVacio), false}
                };
            var gridDeRelacion = new GridDeRelacion(expansor, columnas, parametros);
            gridDeRelacion.PermitirBorrar = true;

            expansor.DescriptorDeEditarRelaciones(Editor.Crud.Contexto, typeof(TareaDto), typeof(TareasController), "Editar tarea", false);

            var modal = expansor.DescriptorDeCrearVinculos(Editor.Crud.Contexto, typeof(TareaDto), nameof(TareasController), "Crear una tarea");
            modal.AccionTrasAbrirModal = $"javascript: Crud.{enumGestorDeEventos.EventosDeExpansores}('{eventosDeExpansor.TrasAbrirModal}', '{modal.IdHtml};proponer-cg|proponer-solicitante')";

            expansor.DescriptorParaVincular(Editor.Crud.Contexto, NegociosDeSe.IdNegocio(enumNegocio.Tarea), typeof(SelectorDeTareaDto), nameof(TareasController), "Asociar tarea");
        }

        private void DefinirDescriptorDeArchivadores()
        {
            var expansor = new DescriptorDeExpansor(Editor, $"{Editor.Id}-archivadores", "Archivadores", true, "Archivadores del elemento");
            Editor.Expanes.Add(expansor);

            //Definimos el grid de detalles del cuerpo
            var columnas = new DescriptorDeColumnas("archivadores");
            columnas.Add(titulo: "Archivador", propiedad: nameof(ArchivadorDto.Expresion), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnas.Add(titulo: "Tipo", propiedad: nameof(ArchivadorDto.Tipo), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnas.Add(titulo: "Bloqueado", propiedad: nameof(ArchivadorDto.Bloqueado), alineacion: enumAliniacion.izquierda, mostrar: true, tamano: 100);
            columnas.Add(titulo: "De baja", propiedad: nameof(ArchivadorDto.Baja), alineacion: enumAliniacion.izquierda, mostrar: true, tamano: 100);
            columnas.Add(titulo: "Archivos", propiedad: nameof(ArchivadorDto.Cantidad), alineacion: enumAliniacion.derecha, mostrar: true, tamano: 100);
            columnas.Add(titulo: "Carpetas", propiedad: nameof(ArchivadorDto.ConCarpetas), alineacion: enumAliniacion.derecha, mostrar: true, tamano: 100);
            columnas.Add(titulo: "Id", propiedad: nameof(ArchivadorDto.Id), alineacion: enumAliniacion.derecha, mostrar: false);
            var parametros = new Dictionary<string, object> {
                   { nameof(GridDeRelacion.Controlador), typeof(ArchivadoresController) }
                 , { nameof(GridDeRelacion.AccionDeConsulta), nameof(BaseController<TElemento>.epLeerVinculosCon)}
                 , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(VinculoDtm.idElemento1) }
                 , { nameof(ltrParametrosEp.idVinculado) , NegociosDeSe.IdNegocio(enumNegocio.Archivador) }
                 , { nameof(GridDeRelacion.PaginaDondeNavegarAlEditar) , $"{nameof(ArchivadoresController)}/{nameof(ArchivadoresController.CrudArchivadores)}"}
                 , { nameof(GridDeRelacion.OcultarSiVacio), false}
                };
            var gridDeRelacion = new GridDeRelacion(expansor, columnas, parametros);
            gridDeRelacion.PermitirBorrar = true;

            gridDeRelacion.acciones.Add(new ColumnaAccion
            {
                accion = Referencia.SisDoc_CarpetasDeUnArchivador(expansor),
                titulo = "Carpetas",
                tamano = 100,
                visible = true
            });

            var modal = expansor.DescriptorDeCrearVinculos(Editor.Crud.Contexto, typeof(ArchivadorDto), nameof(ArchivadoresController), "Crear y Anexar un archivador");
            modal.AccionTrasAbrirModal = $"javascript: Crud.{enumGestorDeEventos.EventosDeExpansores}('{eventosDeExpansor.TrasAbrirModal}', '{modal.IdHtml};bloquear-cg')";

            expansor.DescriptorDeEditarRelaciones(Editor.Crud.Contexto, typeof(ArchivadorDto), typeof(ArchivadoresController), "Editar archivador", false);
            expansor.DescriptorParaVincular(Editor.Crud.Contexto, NegociosDeSe.IdNegocio(enumNegocio.Archivador), typeof(SelectorDeArchivadorDto), nameof(ArchivadoresController), "Anexar un archivador");
        }

        private void DefinirDescriptorDeArchivos()
        {
            var expanDeAnexados = new DescriptorDeExpansor(Editor, $"{Editor.Id}-archivos", "Archivos", true, "Archivos anexados");
            Editor.Expanes.Add(expanDeAnexados);
            expanDeAnexados.CuerpoDelExpansor = new ContenedorDeArchivos(expanDeAnexados, Negocio);
        }

        private void DefinirDescriptorDeAuditoria()
        {
            var tipoDtm = NegociosDeSe.TipoDtm(Negocio);
            if (!ApiDeInterfaceDto.ImplementaAuditoriaDto(typeof(TElemento)))
                GestorDeErrores.Emitir($"El dto {typeof(TElemento).Name} debe implementar aditoria, ya que el dtm {tipoDtm.Name} la implementa");
            Editor.Expanes.Add(DescriptorDeExpansor.ExpansorDeAuditoria(Editor, enumNameSpaceTs.Crud, RenderNegocio));
        }

        public ControlFiltroHtml RenombrarEtiqueta(string etiquetaAnterior, string nuevaEtiqueta, string ayuda = null)
        {
            var control = Mnt.Filtro.BuscarControlPorEtiqueta(etiquetaAnterior);
            if (control == null)
                throw new Exception($"Se ha solicitado cambiar la etiqueta {etiquetaAnterior} por {nuevaEtiqueta} y no se ha encontrado el control");
            control.CambiarEtiqueta(nuevaEtiqueta, ayuda);
            return control;
        }

        public ControlFiltroHtml RecolocarControl(ControlFiltroHtml control, Posicion posicion, string nuevaEtiqueta = null, string ayuda = null)
        {
            control.Posicion = posicion;
            //((BloqueDeFitro<TElemento>)control.Padre).AjustarDimensionDeLaTabla();

            if (!nuevaEtiqueta.IsNullOrEmpty())
                control.CambiarEtiqueta(nuevaEtiqueta, ayuda);
            return control;
        }

        public ControlFiltroHtml BuscarControlEnFiltro(string propiedad)
        {
            return Mnt.Filtro.BuscarControlPorPropiedad(propiedad);
        }

        public void CambiarModo(ModoDescriptor modo)
        {
            Modo = modo;
        }

        protected void DefinirColumnasDelGrid()
        {
            Mnt.Datos.AnadirColumna(ZonaDeDatos<TElemento>.ColumnaDeControlDeSeleccion());
            var propiedades = typeof(TElemento).GetProperties();
            var columnasPendientes = new Dictionary<int, ColumnaDelGrid>();
            var hayPosicionCero = false;
            hayPosicionCero = ZonaDeDatos<TElemento>.AnadirPropiedades(Mnt.Datos, propiedades, columnasPendientes, hayPosicionCero, AtributosJson);
            var desplazamiento = hayPosicionCero ? 1 : 0;
            var columnasPendientesOrdenadas = new Dictionary<int, ColumnaDelGrid>(columnasPendientes.ToList().OrderBy(x => x.Key));
            foreach (var item in columnasPendientesOrdenadas)
            {
                Mnt.Datos.InsertarColumna(item.Value, item.Key + desplazamiento);
            }
        }

        protected AmpliacionDeCreacion DescriptorDeDireccion(string nombre, string titulo = "Dirección a crear", Type direccionDto = null)
        {
            var ampliacion = new AmpliacionDeCreacion(Creador, nombre, titulo, new Dimension(2, 2), ayuda: "indicar la dirección a crear y asociar");
            ampliacion.Dto = direccionDto != null ? direccionDto : typeof(CrearDireccionDto);
            ampliacion.Controlador = nameof(Controlador);
            Creador.Ampliaciones.Add(ampliacion);
            return ampliacion;
        }

        public virtual void MapearElementosAlGrid<T>(IEnumerable<T> elementos, int cantidadPorLeer, int posicionInicial)
            where T : ElementoDto
        {
            Mnt.Datos.PosicionInicial = posicionInicial;
            Mnt.Datos.CantidadPorLeer = cantidadPorLeer;

            foreach (var elemento in elementos)
            {
                var fila = new FilaDelGrid(Mnt.Datos, elemento);
                foreach (ColumnaDelGrid columna in Mnt.Datos.Columnas)
                {
                    CeldaDelGrid celda = new CeldaDelGrid(columna);
                    var propiedades = typeof(TElemento).GetProperties();
                    foreach (var p in propiedades)
                    {
                        if (columna.Propiedad == p.Name)
                        {
                            celda.Valor = p.GetValue(elemento);
                            break;
                        }
                    }
                    fila.AnadirCelda(celda);
                }
                Mnt.Datos.AnadirFila(fila);
            }
        }

        public void TotalEnBd(int totalEnBd)
        {
            Mnt.Datos.TotalEnBd = totalEnBd;
        }

        public override string RenderControl()
        {
            try
            {
                var renderCorreo = "";
                if (GestorDeCorreos.HayVistaParaMostrarElDto<TElemento>())
                {
                    renderCorreo = ModalDeEnviarCorreo == null ? "" : $@"
                  <!--  *******************  div de envío de correo *************** -->
                  {ModalDeEnviarCorreo.RenderDeEnvioDeCorreo()}
                  <!--  **********  div de selector de receptor de correo****** -->
                  {ModalDeEnviarCorreo.RenderDeModalesParaSeleccionarReceptores()}";
                }

                var renderModales = "";
                foreach (var modal in modalesParaPedirDatos)
                {
                    renderModales = $@"{renderModales}
                  <!--  ************  div de para pedir datos de: {modal.Etiqueta} *************** -->
                  {modal.RenderControl()}";
                }

                var renderTransitador = "";
                if (NegociosDeSe.UsaHitos(NegociosDeSe.NegocioDeUnDto(typeof(TElemento))))
                {
                    renderTransitador = $@"
                  <!--  *******************  div de panel de transitar *************** -->
                  {ModalDeTransitar.RenderDeTransitar()}";
                }

                var renderImpresor = "";

                renderImpresor = ModalDeImpresion == null || Negocio == enumNegocio.No_Definido
                    ? ""
                    : $@"
                  <!--  *******************  div de panel de transitar *************** -->
                  {ModalDeImpresion.RenderDeImprimir()}";


                var renderCrud = Mnt.RenderDelMantenimiento();
                if (ModoDescriptor.Mantenimiento == Modo)
                {
                    renderCrud = $@"
                               {renderCrud}
                               <!--  ******************* div de historial ****************** -->
                               {(Historial is null ? "<!-- sin historial -->" : Historial.RenderDelHistorial())}
                               <!--  ******************* div de creacion ******************* -->
                               {Creador.RenderDeCreacion()}
                               <!--  *******************  div de edición ******************* -->
                               {Editor.RenderDeEdicion()}
                               <!--  *******************  div de exportacion ******************* -->
                               {Exportador.RenderDeExportacion()}
                               <!--  *******************  div de borrado ******************* -->
                               {Borrado.RenderDelBorrado()}
                               {renderCorreo}
                               {renderTransitador}
                               {renderImpresor}
                               {renderModales}
                               <!--  *******************  modales para seleccionar filtros ******************* -->
                               {Mnt.Filtro.RenderModalesDeFiltro()}
                               <!--  *******************  modales de filtrado ******************* -->
                               {Mnt.Filtro.RenderModalesDeSeleccion()}
                               <!--  *******************  modales de expanes ******************* -->
                               {Editor.RenderizarLasModalesDeLosExpansores()}
                               < !--***************************Menús flotantes * *************************-->
                               {Mnt.RenderMenuFlotanteIndividual(menuIndividual)}
                               {Mnt.RenderMenuFlotanteContextual(menuContextual)}
                               {Mnt.RenderMenuFlotanteDeRelaciones(menuDeRelaciones)}
                               {Mnt.RenderMenuFlotanteDeFiltro(menuDeFiltro)}
                               {Editor.RenderMenuFlotanteDeEdicion(menuEdicion)}
                               {Editor.RenderModalDePermisosPorElemento()}
                               {Creador.RenderMenuFlotanteDeCreacion(menuCreacion)}
                               {(Historial is null ? "" : Historial.RenderMenuFlotanteHistorial(menuHistorial))}
                    ";
                }
                else
                if (ModoDescriptor.Consulta == Modo)
                {
                    renderCrud = $@"{renderCrud}
                                   <!--  *******************  div de edición -->
                                   {Editor?.RenderControl()??""}";
                }

                return PanelDeControl.RenderPagina(Contexto, renderCrud, claseAdicional: SinEdicion && SinCreacion ? enumCssCuerpo.CuerpoSoloConGrid.Render(): null );
            }
            catch (Exception e)
            {
                var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
                ServicioDeCaches.EliminarElemento(CacheDe.RenderCrud, indice);
                _renderCache = null;

                Contexto.RegistrarConEnvio($"Error al renderizar '{indice}'", e);
                var error = GestorDeErrores.Detalle(e).Replace(Environment.NewLine, "<br>");
                return $@"<input id=error>{error}</input>";
            }
            finally
            {
                BlanquearListaDeIds();
            }
        }

        internal string RenderCrudModal(string idModal, enumTipoDeModal tipoDeModal)
        {
            return Mnt.RenderMntModal(idModal, tipoDeModal);
        }

        public static ModoDescriptor ParsearModo(string modo)
        {
            switch (modo)
            {
                case nameof(ModoDescriptor.SeleccionarParaFiltrar):
                    return ModoDescriptor.SeleccionarParaFiltrar;
                case nameof(ModoDescriptor.Mantenimiento):
                    return ModoDescriptor.Mantenimiento;
                case nameof(ModoDescriptor.Consulta):
                    return ModoDescriptor.Consulta;
                case nameof(ModoDescriptor.ModalDeConsulta):
                    return ModoDescriptor.ModalDeConsulta;
                case nameof(ModoDescriptor.Relacion):
                    return ModoDescriptor.Relacion;
                case nameof(ModoDescriptor.Imputar):
                    return ModoDescriptor.Imputar;
            }
            throw new Exception($"El modo {modo} no está definido");
        }

        internal static void AnadirOpciondeRelacion(DescriptorDeMantenimiento<TElemento> Mnt, string controlador, string vista,
            string relacionarCon,
            string navegarAlCrud,
            string nombreOpcion,
            string propiedadQueRestringe,
            string propiedadRestrictora,
            string ayuda,
            enumModoDeAccesoDeDatos permisos = enumModoDeAccesoDeDatos.Gestor,
            enumCssOpcionMenu opcionDe = enumCssOpcionMenu.DeElemento
            )
        {
            var accionDeRelacion = new AccionDeRelacionarElemenetos(
                    urlDelCrud: $@"/{controlador}/{vista}?origen=relacion"
                  , relacionarCon: relacionarCon
                  , nombreDelMnt: navegarAlCrud
                  , propiedadQueRestringe: propiedadQueRestringe
                  , propiedadRestrictora: propiedadRestrictora
                  , ayuda);

            var opcion = new OpcionDeMenu<TElemento>(menu: Mnt.ZonaMenu.Menu, accion: accionDeRelacion, tipoAccion: enumTipoDeLlamada.Post, titulo: $"{nombreOpcion}", permisos, opcionDe);
            Mnt.ZonaMenu.Menu.Add(opcion);
        }

        internal static OpcionDeMenu<TElemento> AnadirOpcionDeDependencias(DescriptorDeMantenimiento<TElemento> Mnt, string controlador, string vista, string datosDependientes, string navegarAlCrud, string nombreOpcion, string propiedadQueRestringe, string propiedadRestrictora, string ayuda)
        {
            var accionDeDependencias = new AccionDeGetionarDatosDependientes(
                    urlDelCrud: $@"/{controlador.Replace(ltrEndPoint.Controller, "")}/{vista}?origen=dependencia"
                  , datosDependientes: datosDependientes
                  , nombreDelMnt: navegarAlCrud
                  , propiedadQueRestringe: propiedadQueRestringe
                  , propiedadRestrictora: propiedadRestrictora
                  , ayuda);

            var opcion = new OpcionDeMenu<TElemento>(menu: Mnt.ZonaMenu.Menu, accion: accionDeDependencias, tipoAccion: enumTipoDeLlamada.Post, titulo: $"{nombreOpcion}", enumModoDeAccesoDeDatos.Consultor, enumCssOpcionMenu.DeElemento);
            Mnt.ZonaMenu.Menu.Add(opcion);
            return opcion;
        }
    }

}



