using GestoresDeNegocio.Entorno;
using ModeloDeDto.Callejero;
using ModeloDeDto.Contabilidad;
using ModeloDeDto.Entorno;
using ModeloDeDto.Expediente;
using ModeloDeDto.Gastos;
using ModeloDeDto.Guarderias;
using ModeloDeDto.Juridico;
using ModeloDeDto.Logistica;
using ModeloDeDto.MaestrosTecnico;
using ModeloDeDto.Negocio;
using ModeloDeDto.Presupuesto;
using ModeloDeDto.RegistroEs;
using ModeloDeDto.Seguridad;
using ModeloDeDto.SistemaDocumental;
using ModeloDeDto.Tarea;
using ModeloDeDto.Terceros;
using ModeloDeDto.TrabajosSometidos;
using ModeloDeDto.Ventas;
using ServicioDeDatos;
using Utilidades;

namespace SistemaDeElementos.Inicializador
{
    public static class enumVistas
    {
        public static class Configuracion
        {
            public static readonly string InicializarBd = "Inicializar BD";
            public static readonly string VariablesDeEntorno = "Variables de entorno";
            public static readonly string NegociosDeSE = "Negocios de SE";
            public static readonly string AuditoríaDeElementos = "Auditoría de elementos";
            public static readonly string ParametrosDeNegocio = "Parámetros de Negocio";
            public static readonly string PlantillasDeExportacion = "Plantillas de exportación";
            public static readonly string AplicarSeguridad = "Aplicar seguridad";
            public static readonly string InicializarMaestros = "Definir maestros";
        }
        public static class Entorno
        {
            public static readonly string ArbolDeMenus = "Árbol de menús";
            public static readonly string MenusSe = "Menú definidos";
            public static readonly string VistasDeSe = "Vistas del sistema";
            public static readonly string TrabajosSe = "Trabajos sometidos";
            public static readonly string TrabajosDeUsuario = "Trabajos de usuario";
            public static readonly string ErroresDeUnTrabajo = "Errores de un trabajo";
            public static readonly string TrazasDeUnTrabajo = "Traza de un trabajo";
            public static readonly string Correos = "Correos de usuario";
            public static readonly string Variables = "Variables de entorno";
            public static readonly string Agendas = "Agendas del entorno";
            public static readonly string MiCalendario = "Eventos de mis agendas";
            public static readonly string VisorDeAgenda = "Visor de agenda";
            public static readonly string MiCorreo = "Visor de la bandeja de entrada";
            public static readonly string MiCorreoImap = "Mi correo (Imap)";
            public static readonly string MiCorreoApiKey = "Mi correo (ApiKey)";
            public static readonly string Acciones = "Acciones del sistema";
            public static readonly string Usuarios = "Usuarios del sistema";
            public static readonly string PuestosDeTrabajo = "Puestos de trabajo";
            public static readonly string Roles = "Roles";
            public static readonly string Permisos = "Permisos";
            public static readonly string Certificados = "Certificados";
            public static readonly string InicializarEntorno = "Inicializar entorno";

        }

        public static class SistemaDocumental
        {
            public static readonly string TipoDeArchivadores = "Tipos de archivadores";
            public static readonly string Archivadores = nameof(Archivadores);
            public static readonly string Carpetas = nameof(Carpetas);
            public static readonly string TipoDeCircuitosDoc = $"Tipos de {enumNegocio.CircuitoDoc.Singular().ToLower()}";
            public static readonly string CircuitosDoc = enumNegocio.CircuitoDoc.Plural();
            public static readonly string MaestrosDeCircuitosDoc = $"Maestros de {enumNegocio.CircuitoDoc.Plural().ToLower()}";
            public static readonly string CrearArchivadoresEco = "Crear tipos ECO";

            public static readonly string EstimacionesDirectas = "Estimaciones";
            public static readonly string LotesContables = "Lotes Contables";
            public static readonly string LoteContable = nameof(LoteContable);
            public static readonly string Fichadas = nameof(Fichadas);
            public static readonly string ActividadesFormativas = "Actividades Formativas";
        }
        public static class Terceros
        {
            public static readonly string CentrosGestores = enumNegocio.CentroGestor.Plural();
            public static readonly string Sociedades = enumNegocio.Sociedad.Plural();
            public static readonly string Personas = enumNegocio.Persona.Plural();
            public static readonly string Interlocutores = enumNegocio.Interlocutor.Plural();
            public static readonly string Procuradores = enumNegocio.Procurador.Plural();
            public static readonly string Abogados = enumNegocio.Abogado.Plural();
            public static readonly string Proveedores = enumNegocio.Proveedor.Plural();
            public static readonly string Clientes = enumNegocio.Cliente.Plural();
            public static readonly string ClasesDeJuzgados = ltrClasesDeJuzgado.Clases;
            public static readonly string Juzgados = ltrJuzgado.Juzgados;
            public static readonly string Trabajadores = ltrTrabajador.Trabajadores;
            public static readonly string Bancos = ltrBanco.Bancos;
        }

        public static class Callejero
        {
            public static readonly string TipoDeVia = "Tipos de vía";
            public static readonly string Paises = "Paises";
            public static readonly string Provincias = "Provincias";
            public static readonly string Municipios = "Municipios";
            public static readonly string Calles = "Calles";
            public static readonly string CodigosPostales = "Codigos postales";
            public static readonly string ImportacionCallejero = "Importación callejero";
            public static readonly string CpsDeProv = "Cps de una provincia";
            public static readonly string CpsDeMuni = "Cps de un municipio";
            public static readonly string Barrios = "Barrios";
            public static readonly string CallesDeUnBarrio = "Calles de un barrio";
            public static readonly string Zonas = "Zonas";
            public static readonly string CallesDeUnaZona = "Calles de una Zona";

        }
        public static class Negocio
        {
            public static readonly string Estados = "Estados";
            public static readonly string Transiciones = "Transiciones";
            public static readonly string AccionesDeRelacion = "Acciones de relación";
        }

        public static class Administracion
        {
            public static readonly string TipoDeRegistros = "Tipos de registros";
            public static readonly string RegistrosEs = "Registros de E/S";
            public static readonly string TipoDeTareas = "Tipos de tareas";
            public static readonly string Tareas = "Tareas";
            public static readonly string TipoDeExpedientes = "Tipos de expedientes";
            public static readonly string Expedientes = "Expedientes";
            public static readonly string Actividades = "Actividades";
            public static readonly string MaestrosDeExpedientes = "Maestros de expedientes";
            public static readonly string MaestrosDeTareas = "Maestros de tareas";
        }

        public static class Ventas
        {
            public static readonly string TipoDePresupuesto = "Tipos de presupuesto";
            public static readonly string TipoDeParteTr = $"Tipos de {enumNegocio.ParteDeTrabajo.Singular().ToLower()}";
            public static readonly string TipoDeFacturaEmt = $"Tipos de {enumNegocio.FacturaEmitida.Singular().ToLower()}";
            public static readonly string TipoDeRemesaFae = $"Tipos de {enumNegocio.RemesaFae.Singular().ToLower()}";
            public static readonly string TipoDePlanificacionDeVenta = $"Tipos de {enumNegocio.PlanificacionDeVenta.Singular().ToLower()}";
            public static readonly string Presupuestos = enumNegocio.Presupuesto.Plural();
            public static readonly string PartesTr = enumNegocio.ParteDeTrabajo.Plural();
            public static readonly string FacturasEmt = enumNegocio.FacturaEmitida.Plural();
            public static readonly string FacturasAeat = ltrVistasTitulos.CrudDeFacturasAeat;
            public static readonly string RemesasFae = enumNegocio.RemesaFae.Plural();
            public static readonly string FacturasEmtDeUnaRemesa = $"{enumNegocio.FacturaEmitida.Plural()} de una {enumNegocio.RemesaFae.Singular(true)}";

            public static readonly string PlanificacionesDeVenta = enumNegocio.PlanificacionDeVenta.Plural();
            public static readonly string MaestrosDePpts = $"Maestros de {enumNegocio.Presupuesto.Plural().ToLower()}";
            public static readonly string MaestrosDePartesTr = $"Maestros de {enumNegocio.ParteDeTrabajo.Plural().ToLower()}";
            public static readonly string MaestrosDeFacturasEmt = $"Maestros de {enumNegocio.FacturaEmitida.Plural().ToLower()}";
            public static readonly string MaestrosDeRemesasFae = $"Maestros de {enumNegocio.RemesaFae.Plural().ToLower()}";
            public static readonly string MaestrosDePlanificacionesDeVenta = $"Maestros de {enumNegocio.PlanificacionDeVenta.Plural().ToLower()}";
            public static readonly string SometerTrabajos = "Trabajos de ventas";
            public static readonly string AsignacionesDePtr = "Asignaciones de parte";
        }

        public static class Logistica
        {
            public static readonly string TipoDePedido = $"Tipos de {enumNegocio.Pedido.Singular().ToLower()}";
            public static readonly string MaestrosDePedidos = $"Maestros de {enumNegocio.Pedido.Plural().ToLower()}";
            public static readonly string Pedidos = enumNegocio.Pedido.Plural();
        }


        public static class Gastos
        {
            public static readonly string TipoDePago = $"Tipos de {enumNegocio.Pago.Singular().ToLower()}";
            public static readonly string TipoDeRemesaPag = $"Tipos de {enumNegocio.RemesaPag.Singular().ToLower()}";
            public static readonly string TipoDeFacturaRec = $"Tipos de {enumNegocio.FacturaRecibida.Singular().ToLower()}";

            public static readonly string MaestrosDePagos = $"Maestros de {enumNegocio.Pago.Plural().ToLower()}";
            public static readonly string MaestrosDeRemesasPag = $"Maestros de {enumNegocio.RemesaPag.Plural().ToLower()}";
            public static readonly string MaestrosDeFacturasRec = $"Maestros de {enumNegocio.FacturaRecibida.Plural().ToLower()}";

            public static readonly string Pagos = enumNegocio.Pago.Plural();
            public static readonly string RemesasPag = enumNegocio.RemesaPag.Plural();
            public static readonly string PagosDeUnaRemesa = $"{enumNegocio.Pago.Plural()} de una {enumNegocio.RemesaPag.Singular(true)}";
            public static readonly string FacturasRec = enumNegocio.FacturaRecibida.Plural();
        }

        public static class Juridico
        {
            public static readonly string TipoDePleitos = "Tipos de pleito";
            public static readonly string Pleitos = "Pleitos";
            public static readonly string TipoDeContratos = "Tipos de contratos";
            public static readonly string Contratos = "Contratos";
            public static readonly string Etapas = "Definir etapas";
            public static readonly string SometerTrabajos = "Someter trabajos";
            public static readonly string MaestrosDeContratos = $"Maestros de {enumNegocio.Contrato.Plural().ToLower()}";
            public static readonly string MaestrosDeProcedimientos = $"Maestros de procedimientos";
            public static readonly string Lotes = "Lotes de un contrato";
            public static readonly string UnitariosDeUnLote = "Unitarios de un lote";
            public static readonly string PlanificadorDeVentas = "Planificador de ventas";
        }
        public static class Mts
        {
            public static readonly string Unitarios = "Unitarios";
            public static readonly string TablasDePrecio = "TablasDePrecios";
            public static readonly string Unidades = "Unidades";
            public static readonly string Naturalezas = "Naturalezas";
        }
        public static class RecusosHumanos
        {
            public static readonly string Fichadas = "Registro de Fichadas";
        }
        public static class Contabilidad
        {
            public static readonly string Cuentas = "Cuentas";
            public static readonly string IvasSoportado = "Ivas soportado";
            public static readonly string IvasRepercutido = "Ivas repercutido";
            public static readonly string Irpfs = "Irpfs";
            public static readonly string TipoDePreasiento = $"Tipos de {enumNegocio.Preasiento.Singular().ToLower()}";
            public static readonly string MaestrosDePreasientos = $"Maestros de {enumNegocio.Preasiento.Plural().ToLower()}";
            public static readonly string Preasientos = enumNegocio.Preasiento.Plural();
        }

        public static class Guarderias
        {
            public static readonly string Aulas = nameof(Aulas);
            public static readonly string Infantes = enumNegocio.Infante.Plural();
            public static readonly string Cursos = enumNegocio.CursoDeGuarderia.Plural();
            public static readonly string InfantesDeUnCurso = $"{enumNegocio.Infante.Plural()} de un {enumNegocio.CursoDeGuarderia.Singular(true)}";
        }
    }

    public static class InzVistas
    {
        public static void DefinirVistas(ContextoSe contexto)
        {
            using (var gestor = GestorDeVistaMvc.Gestor(contexto, contexto.Mapeador))
            {
                gestor.Contexto.IniciarTraza(nameof(DefinirVistas), true);
                var tran = gestor.Contexto.IniciarTransaccion();
                try
                {
                    gestor.Contexto.DatosDeConexion.CreandoModelo = true;
                    gestor.CrearVistaSiNoExiste(enumVistas.Configuracion.InicializarBd, enumControladoresEntorno.Inicializador, enumVistasEntorno.InicializarBD, true, typeof(UsuarioDto).FullName);
                    gestor.CrearVistaSiNoExiste(enumVistas.Configuracion.InicializarMaestros, enumControladoresEntorno.Inicializador, enumVistasEntorno.InicializarMaestros, true, null);

                    gestor.CrearVistaSiNoExiste(enumVistas.Configuracion.VariablesDeEntorno, enumControladoresEntorno.Variables, enumVistasEntorno.CrudVariable, true, typeof(VariableDto).FullName);
                    gestor.CrearVistaSiNoExiste(enumVistas.Configuracion.AplicarSeguridad, enumControladoresEntorno.Inicializador, enumVistasEntorno.AplicarSeguridad, true, typeof(UsuarioDto).FullName);

                    gestor.CrearVistaSiNoExiste(enumVistas.Configuracion.NegociosDeSE, enumControladoresNegocio.Negocio, enumVistasNegocio.CrudDeNegocios, false, typeof(NegocioDto).FullName);
                    gestor.CrearVistaSiNoExiste(enumVistas.Configuracion.AuditoríaDeElementos, enumControladoresNegocio.Auditoria, enumVistasNegocio.CrudDeAuditoria, false, typeof(AuditoriaDto).FullName);
                    gestor.CrearVistaSiNoExiste(enumVistas.Configuracion.ParametrosDeNegocio, enumControladoresNegocio.ParametrosDeNegocio, enumVistasNegocio.CrudDeParametrosDeNegocio, true, typeof(ParametroDeNegocioDto).FullName);
                    gestor.CrearVistaSiNoExiste(enumVistas.Configuracion.PlantillasDeExportacion, enumControladoresNegocio.PlantillasDeExportacion, enumVistasNegocio.CrudDePlantillasDeExportacion, true, typeof(PlantillaDeExportacionDto).FullName);
                    gestor.CrearVistaSiNoExiste(enumVistas.Negocio.AccionesDeRelacion, enumControladoresNegocio.AccionesDeRelacion, enumVistasNegocio.CrudDeAccionesDeRelacion, false, typeof(AccionesDeRelacionDto).FullName);

                    CreaVistasDeEntorno(gestor);
                    CrearVistasDeSeguridad(gestor);
                    CrearVistasDelCallejero(gestor);
                    CrearVistasDeTerceros(gestor);
                    CrearVistasDelSistemaDocumental(gestor);
                    CrearVistasDelModuloAdministrativo(gestor);
                    CrearVistasDelModuloRRHH(gestor);
                    CrearVistasDelModuloJuridico(gestor);
                    CrearVistasDelModuloMaestrosTecnicos(gestor);
                    CrearVistasDelModuloContable(gestor);
                    CrearVistasDelModuloDeLogistica(gestor);
                    CrearVistasDelModuloDeVentas(gestor);
                    CrearVistasDelModuloDeGastos(gestor);
                    CrearVistasDelModuloDeGuarderias(gestor);

                    gestor.Contexto.Commit(tran);
                }
                catch
                {
                    gestor.Contexto.Rollback(tran);
                    throw;
                }
                finally
                {
                    gestor.Contexto.CerrarTraza();
                    gestor.Contexto.DatosDeConexion.CreandoModelo = false;
                }
            }
        }

        private static void CrearVistasDeTerceros(GestorDeVistaMvc gestor)
        {
            gestor.CrearVistaSiNoExiste(enumVistas.Terceros.CentrosGestores, enumControladoresTerceros.CentrosGestores, enumVistasTerceros.CrudCentrosGestores, true, typeof(CentroGestorDto).FullName);
            gestor.CrearVistaSiNoExiste(enumVistas.Terceros.Sociedades, enumControladoresTerceros.Sociedades, enumVistasTerceros.CrudSociedades, false, typeof(SociedadDto).FullName);
            gestor.CrearVistaSiNoExiste(enumVistas.Terceros.Personas, enumControladoresTerceros.Personas, enumVistasTerceros.CrudPersonas, modal: false, typeof(PersonaDto).FullName);
            gestor.CrearVistaSiNoExiste(enumVistas.Terceros.Interlocutores, enumControladoresTerceros.Interlocutores, enumVistasTerceros.CrudInterlocutores, modal: false, typeof(InterlocutorDto).FullName);

            gestor.CrearVistaSiNoExiste(enumVistas.Terceros.ClasesDeJuzgados, enumControladoresTerceros.ClasesDeJuzgado, enumVistasTerceros.CrudClasesDeJuzgado, modal: true, typeof(ClaseDeJuzgadoDto).FullName);
            gestor.CrearVistaSiNoExiste(enumVistas.Terceros.Juzgados, enumControladoresTerceros.Juzgados, enumVistasTerceros.CrudJuzgados, modal: true, typeof(JuzgadoDto).FullName);
            gestor.CrearVistaSiNoExiste(enumVistas.Terceros.Procuradores, enumControladoresTerceros.Procuradores, enumVistasTerceros.CrudProcuradores, modal: false, typeof(ProcuradorDto).FullName);
            gestor.CrearVistaSiNoExiste(enumVistas.Terceros.Abogados, enumControladoresTerceros.Abogados, enumVistasTerceros.CrudAbogados, modal: false, typeof(AbogadoDto).FullName);
            gestor.CrearVistaSiNoExiste(enumVistas.Terceros.Proveedores, enumControladoresTerceros.Proveedores, enumVistasTerceros.CrudProveedores, modal: false, typeof(ProveedorDto).FullName);
            gestor.CrearVistaSiNoExiste(enumVistas.Terceros.Clientes, enumControladoresTerceros.Clientes, enumVistasTerceros.CrudClientes, modal: false, typeof(ClienteDto).FullName);
            gestor.CrearVistaSiNoExiste(enumVistas.Terceros.Trabajadores, enumControladoresTerceros.Trabajadores, enumVistasTerceros.CrudTrabajadores, modal: false, typeof(TrabajadorDto).FullName);
            gestor.CrearVistaSiNoExiste(enumVistas.Terceros.Bancos, enumControladoresTerceros.Bancos, enumVistasTerceros.CrudBancos, modal: true, typeof(BancoDto).FullName);
        }

        private static void CrearVistasDelSistemaDocumental(GestorDeVistaMvc gestor)
        {
            gestor.CrearVistaSiNoExiste(enumVistas.SistemaDocumental.TipoDeArchivadores, enumControladoresNegocio.TiposDeElemento, enumVistasSistemaDocumental.TiposDeArchivador, true, typeof(TipoDeArchivadorDto).FullName);
            gestor.CrearVistaSiNoExiste(enumVistas.SistemaDocumental.CrearArchivadoresEco, enumControladoresSistemaDocumental.Archivadores, enumVistasSistemaDocumental.CrearArchivadoresEco, true, null);

            gestor.CrearVistaSiNoExiste(enumVistas.SistemaDocumental.Archivadores, enumControladoresSistemaDocumental.Archivadores, enumVistasSistemaDocumental.CrudArchivadores, false, typeof(ArchivadorDto).FullName);
            gestor.CrearVistaSiNoExiste(enumVistas.SistemaDocumental.Carpetas, enumControladoresSistemaDocumental.Carpetas, enumVistasSistemaDocumental.CrudCarpetas, true, typeof(CarpetaDto).FullName);

            gestor.CrearVistaSiNoExiste(enumVistas.SistemaDocumental.MaestrosDeCircuitosDoc, enumControladoresSistemaDocumental.CircuitosDoc, enumVistasSistemaDocumental.MaestrosDeCircuitosDoc, true, null);
            gestor.CrearVistaSiNoExiste(enumVistas.SistemaDocumental.TipoDeCircuitosDoc, enumControladoresNegocio.TiposDeElemento, enumVistasNegocio.CrudDeTipos(enumNegocio.CircuitoDoc), true, typeof(TipoDeCircuitoDocDto).FullName);
            gestor.CrearVistaSiNoExiste(enumVistas.SistemaDocumental.CircuitosDoc, enumControladoresSistemaDocumental.CircuitosDoc, enumVistasSistemaDocumental.CrudCircuitosDoc, false, typeof(CircuitoDocDto).FullName);
        }
        private static void CrearVistasDelModuloAdministrativo(GestorDeVistaMvc gestor)
        {
            gestor.CrearVistaSiNoExiste(enumVistas.Negocio.Estados, enumControladoresNegocio.Estados, enumVistasNegocio.CrudDeEstados, false, typeof(EstadoDto).FullName);
            gestor.CrearVistaSiNoExiste(enumVistas.Negocio.Transiciones, enumControladoresNegocio.Transiciones, enumVistasNegocio.CrudDeTransiciones, false, typeof(TransicionDto).FullName);

            gestor.CrearVistaSiNoExiste(enumVistas.Administracion.TipoDeRegistros, enumControladoresNegocio.TiposDeElemento, enumVistasNegocio.CrudDeTipos(enumNegocio.Registro), true, typeof(TipoDeRegistroEsDto).FullName);
            gestor.CrearVistaSiNoExiste(enumVistas.Administracion.RegistrosEs, enumControladoresAdministrativos.RegistrosEs, enumVistasAdministrativo.CrudRegistrosEs, false, typeof(RegistroEsDto).FullName);

            gestor.CrearVistaSiNoExiste(enumVistas.Administracion.TipoDeTareas, enumControladoresNegocio.TiposDeElemento, enumVistasNegocio.CrudDeTipos(enumNegocio.Tarea), true, typeof(TipoDeTareaDto).FullName);
            gestor.CrearVistaSiNoExiste(enumVistas.Administracion.Tareas, enumControladoresAdministrativos.Tareas, enumVistasAdministrativo.CrudTareas, false, typeof(TareaDto).FullName);
            gestor.CrearVistaSiNoExiste(enumVistas.Administracion.MaestrosDeTareas, enumControladoresAdministrativos.Tareas, enumVistasAdministrativo.MaestrosDeTareas, true, null);

            gestor.CrearVistaSiNoExiste(enumVistas.Administracion.TipoDeExpedientes, enumControladoresNegocio.TiposDeElemento, enumVistasNegocio.CrudDeTipos(enumNegocio.Expediente), true, typeof(TipoDeExpedienteDto).FullName);
            gestor.CrearVistaSiNoExiste(enumVistas.Administracion.Expedientes, enumControladoresAdministrativos.Expedientes, enumVistasAdministrativo.CrudExpedientes, false, typeof(ExpedienteDto).FullName);
            gestor.CrearVistaSiNoExiste(enumVistas.Administracion.MaestrosDeExpedientes, enumControladoresAdministrativos.Expedientes, enumVistasAdministrativo.MaestrosDeExpedientes, true, null);
            gestor.CrearVistaSiNoExiste(enumVistas.Administracion.Actividades, enumControladoresAdministrativos.Expedientes, enumVistasAdministrativo.CrudActividades, false, typeof(ExpedienteDto).FullName);


            gestor.CrearVistaSiNoExiste(enumVistas.SistemaDocumental.ActividadesFormativas, enumControladoresSistemaDocumental.CircuitosDoc, enumVistasSistemaDocumental.CrudActividadesFormativas, false, typeof(CircuitoDocDto).FullName);

        }


        private static void CrearVistasDelModuloJuridico(GestorDeVistaMvc gestor)
        {
            gestor.CrearVistaSiNoExiste(enumVistas.Juridico.TipoDePleitos, enumControladoresNegocio.TiposDeElemento, enumVistasNegocio.CrudDeTipos(enumNegocio.Pleito), true, typeof(TipoDePleitoDto).FullName);
            gestor.CrearVistaSiNoExiste(enumVistas.Juridico.Pleitos, enumControladoresJuridicos.Pleitos, enumVistasJuridicos.CrudPleitos, false, typeof(PleitoDto).FullName);
            gestor.CrearVistaSiNoExiste(enumVistas.Juridico.TipoDeContratos, enumControladoresNegocio.TiposDeElemento, enumVistasNegocio.CrudDeTipos(enumNegocio.Contrato), true, typeof(TipoDeContratoDto).FullName);
            gestor.CrearVistaSiNoExiste(enumVistas.Juridico.Contratos, enumControladoresJuridicos.Contratos, enumVistasJuridicos.CrudContratos, false, typeof(ContratoDto).FullName);
            gestor.BorrarVistaSiExiste(enumVistas.Juridico.Etapas, enumControladoresJuridicos.Contratos, enumVistasJuridicos.InicializarEtapas, true, null);
            gestor.BorrarVistaSiExiste(enumVistas.Juridico.SometerTrabajos, enumControladoresJuridicos.Contratos, enumVistasJuridicos.SometerTrabajos, true, null);
            gestor.CrearVistaSiNoExiste(enumVistas.Juridico.MaestrosDeContratos, enumControladoresJuridicos.Contratos, enumVistasJuridicos.MaestrosDeContratos, true, null);
            gestor.CrearVistaSiNoExiste(enumVistas.Juridico.MaestrosDeProcedimientos, enumControladoresAdministrativos.Expedientes, enumVistasAdministrativo.MaestrosDeProcedimientos, true, null);
            gestor.CrearVistaSiNoExiste(enumVistas.Juridico.Lotes, enumControladoresJuridicos.LotesDeUnContrato, enumVistasJuridicos.CrudLotes, false, typeof(LoteDeUnContratoDto).FullName);
            gestor.CrearVistaSiNoExiste(enumVistas.Juridico.UnitariosDeUnLote, enumControladoresJuridicos.UnitariosDeUnLote, enumVistasJuridicos.CrudUnitariosDeUnLote, true, typeof(UnitariosDeUnLoteDto).FullName);
            gestor.CrearVistaSiNoExiste(enumVistas.Juridico.PlanificadorDeVentas, enumControladoresJuridicos.PlanificadorDeVentas, enumVistasJuridicos.CrudPlanificadorDeVentas, false, typeof(PlanificadorDeVentaDto).FullName);
        }

        private static void CrearVistasDelModuloDeVentas(GestorDeVistaMvc gestor)
        {
            gestor.CrearVistaSiNoExiste(enumVistas.Ventas.MaestrosDePpts, enumControladoresVentas.Presupuestos, enumVistasVentas.MaestrosDePresupuestos, true, null);
            gestor.CrearVistaSiNoExiste(enumVistas.Ventas.TipoDePresupuesto, enumControladoresNegocio.TiposDeElemento, enumVistasNegocio.CrudDeTipos(enumNegocio.Presupuesto), true, typeof(TipoDePresupuestoDto).FullName);
            gestor.CrearVistaSiNoExiste(enumVistas.Ventas.Presupuestos, enumControladoresVentas.Presupuestos, enumVistasVentas.CrudPresupuestos, false, typeof(PresupuestoDto).FullName);

            gestor.CrearVistaSiNoExiste(enumVistas.Ventas.MaestrosDePartesTr, enumControladoresVentas.PartesTr, enumVistasVentas.MaestrosDePartesTr, true, null);
            gestor.CrearVistaSiNoExiste(enumVistas.Ventas.TipoDeParteTr, enumControladoresNegocio.TiposDeElemento, enumVistasNegocio.CrudDeTipos(enumNegocio.ParteDeTrabajo), true, typeof(TipoDeParteTrDto).FullName);
            gestor.CrearVistaSiNoExiste(enumVistas.Ventas.PartesTr, enumControladoresVentas.PartesTr, enumVistasVentas.CrudPartesDeTrabajo, false, typeof(ParteTrDto).FullName);
            gestor.CrearVistaSiNoExiste(enumVistas.Ventas.AsignacionesDePtr, enumControladoresVentas.AsignacionesDePtr, enumVistasVentas.CrudAsignacionesPtr, false, typeof(AsignacionDePtrDto).FullName);


            gestor.CrearVistaSiNoExiste(enumVistas.Ventas.MaestrosDeFacturasEmt, enumControladoresVentas.FacturasEmt, enumVistasVentas.MaestrosDeFacturasEmt, true, null);
            gestor.CrearVistaSiNoExiste(enumVistas.Ventas.TipoDeFacturaEmt, enumControladoresNegocio.TiposDeElemento, enumVistasNegocio.CrudDeTipos(enumNegocio.FacturaEmitida), true, typeof(TipoDeFacturaEmtDto).FullName);
            gestor.CrearVistaSiNoExiste(enumVistas.Ventas.FacturasEmt, enumControladoresVentas.FacturasEmt, enumVistasVentas.CrudFacturasEmt, false, typeof(FacturaEmtDto).FullName);
            gestor.CrearVistaSiNoExiste(enumVistas.Ventas.FacturasAeat, enumControladoresVentas.FacturasEmt, enumVistasVentas.CrudFacturasAeat, false, typeof(FacturaAeatDto).FullName);

            gestor.CrearVistaSiNoExiste(enumVistas.Ventas.MaestrosDeRemesasFae, enumControladoresVentas.RemesasFae, enumVistasVentas.MaestrosDeRemesasFae, true, null);
            gestor.CrearVistaSiNoExiste(enumVistas.Ventas.TipoDeRemesaFae, enumControladoresNegocio.TiposDeElemento, enumVistasNegocio.CrudDeTipos(enumNegocio.RemesaFae), true, typeof(TipoDeRemesaFaeDto).FullName);
            gestor.CrearVistaSiNoExiste(enumVistas.Ventas.RemesasFae, enumControladoresVentas.RemesasFae, enumVistasVentas.CrudRemesasFae, false, typeof(RemesaFaeDto).FullName);
            gestor.CrearVistaSiNoExiste(enumVistas.Ventas.FacturasEmtDeUnaRemesa, enumControladoresVentas.FacturasEmtDeUnaRemesa, enumVistasVentas.CrudFacturasEmtDeUnaRemesa, true, typeof(FacturaEmtDeUnaRemesaDto).FullName);

            gestor.CrearVistaSiNoExiste(enumVistas.Ventas.MaestrosDePlanificacionesDeVenta, enumControladoresVentas.PlanificacionesDeVenta, enumVistasVentas.MaestrosDePlanificacionesDeVenta, true, null);
            gestor.CrearVistaSiNoExiste(enumVistas.Ventas.TipoDePlanificacionDeVenta, enumControladoresNegocio.TiposDeElemento, enumVistasNegocio.CrudDeTipos(enumNegocio.PlanificacionDeVenta), true, typeof(TipoDePlanificacionDeVentaDto).FullName);
            gestor.CrearVistaSiNoExiste(enumVistas.Ventas.PlanificacionesDeVenta, enumControladoresVentas.PlanificacionesDeVenta, enumVistasVentas.CrudPlanificacionesDeVenta, false, typeof(PlanificacionDeVentaDto).FullName);

            gestor.CrearVistaSiNoExiste(enumVistas.Ventas.SometerTrabajos, enumControladoresVentas.Presupuestos, enumVistasVentas.SometerTrabajos, true, null);
        }

        private static void CrearVistasDelModuloDeLogistica(GestorDeVistaMvc gestor)
        {
            gestor.CrearVistaSiNoExiste(enumVistas.Logistica.MaestrosDePedidos, enumControladoresLogistica.Pedidos, enumVistasLogisticas.MaestrosDePedidos, true, null);
            gestor.CrearVistaSiNoExiste(enumVistas.Logistica.TipoDePedido, enumControladoresNegocio.TiposDeElemento, enumVistasNegocio.CrudDeTipos(enumNegocio.Pedido), true, typeof(TipoDePedidoDto).FullName);
            gestor.CrearVistaSiNoExiste(enumVistas.Logistica.Pedidos, enumControladoresLogistica.Pedidos, enumVistasLogisticas.CrudPedidos, false, typeof(PedidoDto).FullName);
        }

        private static void CrearVistasDelModuloDeGastos(GestorDeVistaMvc gestor)
        {
            gestor.CrearVistaSiNoExiste(enumVistas.Gastos.MaestrosDePagos, enumControladoresGastos.Pagos, enumVistasGastos.MaestrosDePagos, true, null);
            gestor.CrearVistaSiNoExiste(enumVistas.Gastos.TipoDePago, enumControladoresNegocio.TiposDeElemento, enumVistasNegocio.CrudDeTipos(enumNegocio.Pago), true, typeof(TipoDePagoDto).FullName);
            gestor.CrearVistaSiNoExiste(enumVistas.Gastos.Pagos, enumControladoresGastos.Pagos, enumVistasGastos.CrudPagos, false, typeof(PagoDto).FullName);

            gestor.CrearVistaSiNoExiste(enumVistas.Gastos.MaestrosDeRemesasPag, enumControladoresGastos.RemesasPag, enumVistasGastos.MaestrosDeRemesasPag, true, null);
            gestor.CrearVistaSiNoExiste(enumVistas.Gastos.TipoDeRemesaPag, enumControladoresNegocio.TiposDeElemento, enumVistasNegocio.CrudDeTipos(enumNegocio.RemesaPag), true, typeof(TipoDeRemesaPagDto).FullName);
            gestor.CrearVistaSiNoExiste(enumVistas.Gastos.RemesasPag, enumControladoresGastos.RemesasPag, enumVistasGastos.CrudRemesasPag, false, typeof(RemesaPagDto).FullName);
            gestor.CrearVistaSiNoExiste(enumVistas.Gastos.PagosDeUnaRemesa, enumControladoresGastos.PagosDeUnaRemesa, enumVistasGastos.CrudPagosDeUnaRemesa, true, typeof(PagoDeUnaRemesaDto).FullName);

            gestor.CrearVistaSiNoExiste(enumVistas.Gastos.MaestrosDeFacturasRec, enumControladoresGastos.FacturasRec, enumVistasGastos.MaestrosDeFacturasRec, modal: true, null);
            gestor.CrearVistaSiNoExiste(enumVistas.Gastos.TipoDeFacturaRec, enumControladoresNegocio.TiposDeElemento, enumVistasNegocio.CrudDeTipos(enumNegocio.FacturaRecibida), modal: true, typeof(TipoDeFacturaRecDto).FullName);
            gestor.CrearVistaSiNoExiste(enumVistas.Gastos.FacturasRec, enumControladoresGastos.FacturasRec, enumVistasGastos.CrudFacturasRec, false, typeof(FacturaRecDto).FullName);
        }

        private static void CrearVistasDelModuloDeGuarderias(GestorDeVistaMvc gestor)
        {
            gestor.CrearVistaSiNoExiste(enumVistas.Guarderias.Aulas, enumControladoresGuarderias.AulasDeGuarderia, enumVistasGuarderias.CrudAulasDeGuarderia, modal: true, typeof(AulaDeGuarderiaDto).FullName);
            gestor.CrearVistaSiNoExiste(enumVistas.Guarderias.Infantes, enumNegocio.Infante.Controlador(), enumVistasGuarderias.CrudInfantes, modal: false, typeof(InfanteDto).FullName);
            gestor.CrearVistaSiNoExiste(enumVistas.Guarderias.Cursos, enumNegocio.CursoDeGuarderia.Controlador(), enumVistasGuarderias.CrudCursosDeGuarderia, modal: false, typeof(CursoDeGuarderiaDto).FullName);
            gestor.CrearVistaSiNoExiste(enumVistas.Guarderias.InfantesDeUnCurso, enumControladoresGuarderias.InfantesDeUnCurso, enumVistasGuarderias.CrudInfantesDeUnCurso, true, typeof(InfanteDeUnCursoDto).FullName);
        }

        private static void CrearVistasDelModuloMaestrosTecnicos(GestorDeVistaMvc gestor)
        {
            gestor.CrearVistaSiNoExiste(enumVistas.Mts.Unidades, enumControladoresMt.Unidades, enumVistasMts.CrudUnidades, modal: true, typeof(UnidadDto).FullName);
            gestor.CrearVistaSiNoExiste(enumVistas.Mts.Naturalezas, enumControladoresMt.Naturalezas, enumVistasMts.CrudNaturalezas, modal: true, typeof(NaturalezaDto).FullName);
            gestor.CrearVistaSiNoExiste(enumVistas.Mts.Unitarios, enumControladoresMt.Unitarios, enumVistasMts.CrudUnitarios, modal: false, typeof(UnitarioDto).FullName);
        }

        private static void CrearVistasDelModuloRRHH(GestorDeVistaMvc gestor)
        {
            gestor.CrearVistaSiNoExiste(enumVistas.SistemaDocumental.Fichadas, enumControladoresSistemaDocumental.CircuitosDoc, enumVistasSistemaDocumental.CrudFichadas, false, typeof(CircuitoDocDto).FullName);
        }

        private static void CrearVistasDelModuloContable(GestorDeVistaMvc gestor)
        {
            gestor.CrearVistaSiNoExiste(enumVistas.Contabilidad.Cuentas, enumControladoresContables.Cuentas, enumVistasContables.CrudCuentas, modal: true, typeof(CuentaDto).FullName);
            gestor.CrearVistaSiNoExiste(enumVistas.Contabilidad.IvasSoportado, enumControladoresContables.IvasSoportado, enumVistasContables.CrudIvasSoportado, modal: true, typeof(IvaSoportadoDto).FullName);
            gestor.CrearVistaSiNoExiste(enumVistas.Contabilidad.IvasRepercutido, enumControladoresContables.IvasRepercutido, enumVistasContables.CrudIvasRepercutido, modal: true, typeof(IvaRepercutidoDto).FullName);
            gestor.CrearVistaSiNoExiste(enumVistas.Contabilidad.Irpfs, enumControladoresContables.Irpfs, enumVistasContables.CrudIrpfs, modal: true, typeof(IrpfDto).FullName);

            gestor.CrearVistaSiNoExiste(enumVistas.Contabilidad.MaestrosDePreasientos, enumControladoresContables.Preasientos, enumVistasContables.MaestrosDePreasientos, true, null);
            gestor.CrearVistaSiNoExiste(enumVistas.Contabilidad.TipoDePreasiento, enumControladoresNegocio.TiposDeElemento, enumVistasNegocio.CrudDeTipos(enumNegocio.Preasiento), true, typeof(TipoDePreasientoDto).FullName);
            gestor.CrearVistaSiNoExiste(enumVistas.Contabilidad.Preasientos, enumControladoresContables.Preasientos, enumVistasContables.CrudPreasientos, false, typeof(PreasientoDto).FullName);
            gestor.CrearVistaSiNoExiste(enumVistas.SistemaDocumental.EstimacionesDirectas, enumControladoresSistemaDocumental.CircuitosDoc, enumVistasSistemaDocumental.CrudEstimacionesDirectas, false, typeof(CircuitoDocDto).FullName);
            gestor.CrearVistaSiNoExiste(enumVistas.SistemaDocumental.LotesContables, enumControladoresSistemaDocumental.CircuitosDoc, enumVistasSistemaDocumental.CrudLotesContables, false, typeof(CircuitoDocDto).FullName);
        }


        private static void CreaVistasDeEntorno(GestorDeVistaMvc gestor)
        {
            gestor.CrearVistaSiNoExiste(enumVistas.Entorno.ArbolDeMenus, enumControladoresEntorno.JerarqiaMenus, enumVistasEntorno.CrudJerarqiaMenus, false, typeof(MenuDto).FullName);
            gestor.CrearVistaSiNoExiste(enumVistas.Entorno.MenusSe, enumControladoresEntorno.Menus, enumVistasEntorno.CrudMenu, false, typeof(MenuDto).FullName);
            gestor.CrearVistaSiNoExiste(enumVistas.Entorno.VistasDeSe, enumControladoresEntorno.VistaMvc, enumVistasEntorno.CrudVistaMvc, true, typeof(VistaMvcDto).FullName);
            gestor.CrearVistaSiNoExiste(enumVistas.Entorno.TrabajosSe, enumControladoresEntorno.TrabajosSometido, enumVistasEntorno.CrudDeTrabajosSometido, false, typeof(TrabajoSometidoDto).FullName);
            gestor.CrearVistaSiNoExiste(enumVistas.Entorno.TrabajosDeUsuario, enumControladoresEntorno.TrabajosDeUsuario, enumVistasEntorno.CrudDeTrabajosDeUsuario, false, typeof(TrabajoDeUsuarioDto).FullName);
            gestor.CrearVistaSiNoExiste(enumVistas.Entorno.ErroresDeUnTrabajo, enumControladoresEntorno.ErroresDeUnTrabajo, enumVistasEntorno.CrudDeErroresDeUnTrabajo, true, typeof(ErrorDeUnTrabajoDto).FullName);
            gestor.CrearVistaSiNoExiste(enumVistas.Entorno.TrazasDeUnTrabajo, enumControladoresEntorno.TrazasDeUnTrabajo, enumVistasEntorno.CrudDeTrazasDeUnTrabajo, true, typeof(TrazaDeUnTrabajoDto).FullName);
            gestor.CrearVistaSiNoExiste(enumVistas.Entorno.Correos, enumControladoresEntorno.Correos, enumVistasEntorno.CrudDeCorreos, false, typeof(CorreoDto).FullName);
            gestor.CrearVistaSiNoExiste(enumVistas.Entorno.Acciones, enumControladoresEntorno.Acciones, enumVistasEntorno.CrudDeAcciones, false, typeof(AccionDto).FullName);
            gestor.CrearVistaSiNoExiste(enumVistas.Entorno.Usuarios, enumControladoresEntorno.Usuarios, enumVistasEntorno.CrudUsuario, false, typeof(UsuarioDto).FullName);
            gestor.CrearVistaSiNoExiste(enumVistas.Entorno.Agendas, enumControladoresEntorno.Agendas, enumVistasEntorno.CrudAgendas, true, typeof(AgendaDto).FullName);
            gestor.CrearVistaSiNoExiste(enumVistas.Entorno.Certificados, enumControladoresEntorno.Certificados, enumVistasEntorno.CrudCertificados, false, typeof(CertificadoDto).FullName);
            gestor.CrearVistaSiNoExiste(enumVistas.Entorno.VisorDeAgenda, enumControladoresEntorno.VisorDeAgenda, enumVistasEntorno.VisorDeAgenda, true, typeof(EventoDeAgendaDto).FullName);
            gestor.CrearVistaSiNoExiste(enumVistas.Entorno.MiCorreo, enumControladoresEntorno.MiCorreo, enumVistasEntorno.CrudDeMiCorreo, modal: true, typeof(MiCorreoDto).FullName);
            gestor.CrearVistaSiNoExiste(enumVistas.Entorno.MiCorreoImap, enumControladoresEntorno.MiCorreo, enumVistasEntorno.CrudDeMiCorreoImap, modal: true, typeof(MiCorreoDto).FullName);
            gestor.CrearVistaSiNoExiste(enumVistas.Entorno.MiCorreoApiKey, enumControladoresEntorno.MiCorreo, enumVistasEntorno.CrudDeMiCorreoApiKey, modal: true, typeof(MiCorreoDto).FullName);
            gestor.CrearVistaSiNoExiste(enumVistas.Entorno.MiCalendario, enumControladoresEntorno.VisorDeAgenda, enumVistasEntorno.MiCalendario, true, typeof(EventoDeAgendaDto).FullName);
            gestor.CrearVistaSiNoExiste(enumVistas.Entorno.InicializarEntorno, enumControladoresEntorno.ArbolDeMenu, enumVistasEntorno.InicializarEntorno, true, null);
        }

        private static void CrearVistasDeSeguridad(GestorDeVistaMvc gestor)
        {
            gestor.CrearVistaSiNoExiste("Clases de permisos", enumControladoresSeguridad.ClaseDePermiso, enumVistasSeguridad.CrudClaseDePermiso, true, typeof(ClasePermisoDto).FullName);

            gestor.CrearVistaSiNoExiste("Puestos de trabajo", enumControladoresSeguridad.PuestoDeTrabajo, enumVistasSeguridad.CrudPuestoDeTrabajo, false, typeof(PuestoDto).FullName);
            gestor.CrearVistaSiNoExiste("Roles", enumControladoresSeguridad.Rol, enumVistasSeguridad.CrudRol, false, typeof(RolDto).FullName);
            gestor.CrearVistaSiNoExiste("Permisos", enumControladoresSeguridad.Permisos, enumVistasSeguridad.CrudPermiso, false, typeof(PermisoDto).FullName);

            gestor.CrearVistaSiNoExiste("Puestos de trabajo de un usuario", enumControladoresSeguridad.PuestosDeUnUsuario, enumVistasSeguridad.CrudPuestosDeUnUsuario, true, typeof(PuestosDeUnUsuarioDto).FullName);
            gestor.CrearVistaSiNoExiste("Roles de un puesto", enumControladoresSeguridad.RolesDeUnPuesto, enumVistasSeguridad.CrudRolesDeUnPuesto, true, typeof(RolesDeUnPuestoDto).FullName);
            gestor.CrearVistaSiNoExiste("Permisos de un rol", enumControladoresSeguridad.PermisosDeUnRol, enumVistasSeguridad.CrudPermisosDeUnRol, true, typeof(PermisosDeUnRolDto).FullName);
            gestor.CrearVistaSiNoExiste("Permisos de un usuario", enumControladoresSeguridad.PermisosDeUnUsuario, enumVistasSeguridad.CrudPermisosDeUnUsuario, true, typeof(PermisosDeUnUsuarioDto).FullName);
            gestor.CrearVistaSiNoExiste("Roles de un permiso", enumControladoresSeguridad.RolesDeUnPermiso, enumVistasSeguridad.CrudRolesDeUnPermiso, false, typeof(RolesDeUnPermisoDto).FullName);
            gestor.CrearVistaSiNoExiste("Puestos de un rol", enumControladoresSeguridad.PuestosDeUnRol, enumVistasSeguridad.CrudPuestosDeUnRol, false, typeof(PuestosDeUnRolDto).FullName);
            gestor.CrearVistaSiNoExiste("Usuarios de un puesto", enumControladoresSeguridad.UsuariosDeUnPuesto, enumVistasSeguridad.CrudUsuariosDeUnPuesto, false, typeof(UsuariosDeUnPuestoDto).FullName);
            gestor.CrearVistaSiNoExiste("Permisos de un puesto de trabajo", enumControladoresSeguridad.PermisosHeredados, enumVistasSeguridad.CrudPermisosHeredados, false, typeof(PermisosDeUnPuestoDto).FullName);
        }

        private static void CrearVistasDelCallejero(GestorDeVistaMvc gestor)
        {
            gestor.CrearVistaSiNoExiste(enumVistas.Callejero.Paises, enumControladoresCallejero.Paises, enumVistasCallejero.CrudPaises, true, typeof(PaisDto).FullName);
            gestor.CrearVistaSiNoExiste(enumVistas.Callejero.Provincias, enumControladoresCallejero.Provincias, enumVistasCallejero.CrudProvincias, false, typeof(ProvinciaDto).FullName);
            gestor.CrearVistaSiNoExiste(enumVistas.Callejero.Municipios, enumControladoresCallejero.Municipios, enumVistasCallejero.CrudMunicipios, true, typeof(MunicipioDto).FullName);
            gestor.CrearVistaSiNoExiste(enumVistas.Callejero.TipoDeVia, enumControladoresCallejero.TiposDeVia, enumVistasCallejero.CrudTiposDeVia, true, typeof(TipoDeViaDto).FullName);
            gestor.CrearVistaSiNoExiste(enumVistas.Callejero.CodigosPostales, enumControladoresCallejero.CodigosPostales, enumVistasCallejero.CrudCodigosPostales, true, typeof(CodigoPostalDto).FullName);
            gestor.CrearVistaSiNoExiste(enumVistas.Callejero.CpsDeProv, enumControladoresCallejero.CpsDeUnaProvincia, enumVistasCallejero.CrudCpsDeUnaProvincia, true, typeof(CpsDeUnaProvinciaDto).FullName);
            gestor.CrearVistaSiNoExiste(enumVistas.Callejero.CpsDeMuni, enumControladoresCallejero.CpsDeUnMunicipio, enumVistasCallejero.CrudCpsDeUnMunicipio, true, typeof(CpsDeUnMunicipioDto).FullName);
            gestor.CrearVistaSiNoExiste(enumVistas.Callejero.Calles, enumControladoresCallejero.Calles, enumVistasCallejero.CrudCalles, false, typeof(CalleDto).FullName);
            gestor.CrearVistaSiNoExiste(enumVistas.Callejero.Barrios, enumControladoresCallejero.Barrios, enumVistasCallejero.CrudBarrios, true, typeof(BarrioDto).FullName);
            gestor.CrearVistaSiNoExiste(enumVistas.Callejero.CallesDeUnBarrio, enumControladoresCallejero.CallesDeUnBarrio, enumVistasCallejero.CrudCallesDeUnBarrio, true, typeof(CallesDeUnBarrioDto).FullName);
            gestor.CrearVistaSiNoExiste(enumVistas.Callejero.Zonas, enumControladoresCallejero.Zonas, enumVistasCallejero.CrudZonas, true, typeof(ZonaDto).FullName);
            gestor.CrearVistaSiNoExiste(enumVistas.Callejero.CallesDeUnaZona, enumControladoresCallejero.CallesDeUnaZona, enumVistasCallejero.CrudCallesDeUnaZona, true, typeof(CallesDeUnaZonaDto).FullName);
            gestor.CrearVistaSiNoExiste(enumVistas.Callejero.ImportacionCallejero, enumControladoresCallejero.ImportarCallejero, enumVistasCallejero.ImportarCallejero, true, null);

        }
    }

}
