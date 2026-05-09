using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.Callejero;
using ServicioDeDatos.TrabajosSometidos;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.RegistroEs;
using ServicioDeDatos.Tarea;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.Juridico;
using ServicioDeDatos.MaestrosTecnico;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Presupuesto;
using ServicioDeDatos.Ventas;
using ServicioDeDatos.Gastos;
using ServicioDeDatos.Guarderias;
using ServicioDeDatos.Logistica;

namespace ServicioDeDatos
{
    public partial class ContextoSe : DbContext
    {
        public DbSet<MenuDtm> Menus { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            DefinirTablasDelEsquemaDeEntorno(modelBuilder);

            DefinirEsquemaDeSeguridad(modelBuilder);

            DefinirTablasDelEsquemaSisDoc(modelBuilder);

            DefinirTablasDelEsquemaDeNegocio(modelBuilder);

            DefinirTablasDelCallejero(modelBuilder);

            DefinirTablasDelEsquemaDeTrabajosSometidos(modelBuilder);

            DefinirTablasDelEsquemaDeTerceros(this, modelBuilder);

            DefinirTablasDelEsquemaDeRegistroEs(modelBuilder);

            DefinirTablasDelEsquemaDeTareas(modelBuilder);

            DefinirTablasDelEsquemaDeExpediente(modelBuilder);

            DefinirTablasDelModeloDePleito(modelBuilder);

            DefinirTablasDelModeloDeContratos(modelBuilder);

            DefinirTablasMt(modelBuilder);

            DefinirTablasContables(modelBuilder);

            DefinirTablasDelEsquemaDePresupuesto(modelBuilder);

            DefinirTablasDePartesDeTrabajo(modelBuilder);

            DefinirTablasDeFacturasEmitidas(modelBuilder);

            DefinirTablasDePlanificaciondeVenta(modelBuilder);

            DefinirTablasDeRemesasDeFacturas(modelBuilder);

            DefinirTablasDeCircuitosDoc(modelBuilder);

            DefinirTablasDePagos(modelBuilder);

            DefinirTablasDeGastos(modelBuilder);

            DefinirTablasDeRemesasDePagos(modelBuilder);

            DefinirTablasDeFacturasRecibidas(modelBuilder);

            DefinirTablasDePedidos(modelBuilder);

            DefinirTablasDeGuarderias(modelBuilder);

        }

        private void DefinirTablasDeFacturasRecibidas(ModelBuilder modelBuilder)
        {
            ModeloDeFacturaRec.EstadosDeUnaFacturaRec(modelBuilder);
            ModeloDeFacturaRec.TransicionesDeUnaFacturaRec(modelBuilder);
            ModeloDeFacturaRec.AccionesDeUnaFacturaRec(modelBuilder);
            ModeloDeFacturaRec.TipoDeFacturaRec(modelBuilder);

            ModeloDeFacturaRec.FacturaRec(modelBuilder);
            ModeloDeFacturaRec.Trazas(modelBuilder);
            ModeloDeFacturaRec.Auditoria(modelBuilder);
            ModeloDeFacturaRec.Agenda(modelBuilder);
            ModeloDeFacturaRec.Archivos(modelBuilder);
            ModeloDeFacturaRec.Observaciones(modelBuilder);
            ModeloDeFacturaRec.Permisos(modelBuilder);
            ModeloDeFacturaRec.Direcciones(modelBuilder);
            ModeloDeFacturaRec.Historia(modelBuilder);
            ModeloDeFacturaRec.Archivadores(modelBuilder);
            ModeloDeFacturaRec.Circuitos(modelBuilder);
            ModeloDeFacturaRec.Pagos(modelBuilder);

            ModeloDeFacturaRec.DatosDeLineaDeUnaFacturaRec(modelBuilder);
        }


        private void DefinirTablasDePedidos(ModelBuilder modelBuilder)
        {
            ModeloDePedido.EstadosDeUnPedido(modelBuilder);
            ModeloDePedido.TransicionesDeUnPedido(modelBuilder);
            ModeloDePedido.AccionesDeUnPedido(modelBuilder);
            ModeloDePedido.TipoDePedido(modelBuilder);

            ModeloDePedido.Pedido(modelBuilder);
            ModeloDePedido.Trazas(modelBuilder);
            ModeloDePedido.Auditoria(modelBuilder);
            ModeloDePedido.Agenda(modelBuilder);
            ModeloDePedido.Archivos(modelBuilder);
            ModeloDePedido.Observaciones(modelBuilder);
            ModeloDePedido.Permisos(modelBuilder);
            ModeloDePedido.Direcciones(modelBuilder);
            ModeloDePedido.Historia(modelBuilder);
            ModeloDePedido.Archivadores(modelBuilder);

            ModeloDePedido.DatosDeLineaDeUnaPedido(modelBuilder);
        }

        private static void DefinirEsquemaDeSeguridad(ModelBuilder modelBuilder)
        {
            TablaClasePermiso.Definir(modelBuilder);

            ModeloDeSeguridad.Permiso(modelBuilder);

            ModeloDeSeguridad.PuestoDeTrabajo(modelBuilder);

            ModeloDeSeguridad.Rol(modelBuilder);

            ModeloDeSeguridad.PermisosDeUnRol(modelBuilder);

            ModeloDeSeguridad.RolesDeUnPt(modelBuilder);

            TablaPermisoTipo.Definir(modelBuilder);

            ModeloDeSeguridad.PtsDeUnUsuario(modelBuilder);

            ModeloDeSeguridad.CrearVistaDePermisosPorPuesto(modelBuilder);

            ModeloDeSeguridad.PermisoPorTipo(modelBuilder);
            ModeloDeSeguridad.PermisoPorCg(modelBuilder);
            ModeloDeSeguridad.PermisoPorNegocio(modelBuilder);
            ModeloDeSeguridad.PermisoPorElemento(modelBuilder);
            ModeloDeSeguridad.PermisoPorEstado(modelBuilder);
            ModeloDeSeguridad.PermisoPorTransicion(modelBuilder);

            ModeloDeSeguridad.PermisosDeUnPuesto(modelBuilder);
        }

        private static void DefinirTablasDelEsquemaDeEntorno(ModelBuilder modelBuilder)
        {
            ModeloDeEntorno.VistaMvc(modelBuilder);

            TablaVariable.Definir(modelBuilder);

            ModeloDeEntorno.CrearVistaDePermisosPorUsuario(modelBuilder);

            ApiDeUsuarioDtm.Definir(modelBuilder);

            ModeloDeEntorno.Menus(modelBuilder);

            VistaMenuSe.Definir(modelBuilder);
            ModeloDeEntorno.Acciones(modelBuilder);
            ModeloDeEntorno.Agendas(modelBuilder);
            ModeloDeEntorno.EventosDelCalendario(modelBuilder);
            ModeloDeEntorno.Certificados(modelBuilder);
            ModeloDeEntorno.MiCorreo(modelBuilder);
            ModeloDeEntorno.ParametrosVistaPorUsuario(modelBuilder);
            ModeloDeEntorno.AccesosRecientes(modelBuilder);
            ModeloDeEntorno.IaPreguntas(modelBuilder);
        }

        private static void DefinirTablasDelEsquemaSisDoc(ModelBuilder modelBuilder)
        {
            ModeloDocumental.Archivo(modelBuilder);
            ModeloDocumental.ArchivoSincronizado(modelBuilder);
            ModeloDocumental.TipoDeArchivador(modelBuilder);
            ModeloDocumental.Archivador(modelBuilder);
            ModeloDocumental.AuditoriaDeUnArchivador(modelBuilder);
            ModeloDocumental.ArchivosDeUnArchivador(modelBuilder);
            ModeloDocumental.Carpeta(modelBuilder);
            ModeloDocumental.ArchivosDeUnaCarpeta(modelBuilder);
            ModeloDocumental.ObservacionesDeUnArchivador(modelBuilder);
            ModeloDocumental.PermisosPorArchivador(modelBuilder);
            ModeloDocumental.TrazasDeUnArchivador(modelBuilder);
            ModeloDocumental.Firmado(modelBuilder);
            ModeloDocumental.BloqueoDeUnArchivo(modelBuilder);
            ModeloDocumental.AuditoriaDeUnArchivo(modelBuilder);
            ModeloDocumental.DescargaConGuid(modelBuilder);
        }

        private static void DefinirTablasDelCallejero(ModelBuilder modelBuilder)
        {
            ModeloDeCallejero.Pais(modelBuilder);
            ModeloDeCallejero.PaisAudt(modelBuilder);
            ModeloDeCallejero.Provincia(modelBuilder);
            ModeloDeCallejero.ProvinciaAudt(modelBuilder);
            ModeloDeCallejero.Municipio(modelBuilder);
            ModeloDeCallejero.MunicipioAudt(modelBuilder);
            ModeloDeCallejero.TipoDeVia(modelBuilder);
            ModeloDeCallejero.CodigoPostal(modelBuilder);
            ModeloDeCallejero.ProvinciaCp(modelBuilder);
            ModeloDeCallejero.MunicipioCp(modelBuilder);
            ModeloDeCallejero.Calle(modelBuilder);
            ModeloDeCallejero.CalleAudt(modelBuilder);
            ModeloDeCallejero.CalleCp(modelBuilder);
            ModeloDeCallejero.Barrio(modelBuilder);
            ModeloDeCallejero.BarrioAudt(modelBuilder);
            ModeloDeCallejero.CalleBarrio(modelBuilder);
            ModeloDeCallejero.Zona(modelBuilder);
            ModeloDeCallejero.ZonaAudt(modelBuilder);
            ModeloDeCallejero.CalleZona(modelBuilder);
        }

        private static void DefinirTablasDelEsquemaDeNegocio(ModelBuilder modelBuilder)
        {
            ModeloDeNegocio.Negocio(modelBuilder);
            ModeloDeNegocio.Libro(modelBuilder);
            ModeloDeNegocio.SemaforoDeUnLibro(modelBuilder);
            ModeloDeNegocio.SemaforoDeUnProceso(modelBuilder);
            ModeloDeNegocio.ParametrosDeNegocio(modelBuilder);
            ModeloDeNegocio.ParametrosDeUsuario(modelBuilder);
            ModeloDeNegocio.PlantillasDeCreacion(modelBuilder);
            ModeloDeNegocio.PlantillasDeExportacion(modelBuilder);
            ModeloDeNegocio.PlantillasDeFiltrado(modelBuilder);
            ModeloDeNegocio.ModoDeAcceso(modelBuilder);
            ModeloDeNegocio.AccionesDeRelacion(modelBuilder);
            ModeloDeNegocio.AccionDeNegocio(modelBuilder);
            ModeloDeNegocio.DefinirCamposDePlantillaDeNegocioDtm(modelBuilder);
            ModeloDeNegocio.ClasesDelDeNegocio(modelBuilder);
            ModeloDeNegocio.ConsultasConGuid(modelBuilder);
        }

        private static void DefinirTablasDelEsquemaDeTrabajosSometidos(ModelBuilder modelBuilder)
        {
            TablaTrabajo.Definir(modelBuilder);
            TablaTrabajoDeUsuario.Definir(modelBuilder);
            TablaDeLatrazaDeUnTrabajo.Definir(modelBuilder);
            TablaDeErroresDeUnTrabajo.Definir(modelBuilder);
            TablaSemaforoDeTrabajos.Definir(modelBuilder);
            TablaDeCorreos.Definir(modelBuilder);
        }

        private static void DefinirTablasDelEsquemaDeTerceros(ContextoSe contexto, ModelBuilder modelBuilder)
        {
            ModeloDeTerceros.Sociedad(modelBuilder);
            ModeloDeTerceros.SociedadAudt(modelBuilder);
            ModeloDeTerceros.CertificadosDeUnaSociedad(modelBuilder);
            ModeloDeTerceros.ArchivosDeUnaSociedad(modelBuilder);
            ModeloDeTerceros.CentroGestor(modelBuilder);
            ModeloDeTerceros.CentroGestorAudt(modelBuilder);
            ModeloDeTerceros.PermisosDeUnCg(modelBuilder);
            ModeloDeTerceros.NegociosDeUnCentroGestor(modelBuilder);
            ModeloDeTerceros.ObservacionesDeUnaSociedad(modelBuilder);
            ModeloDeTerceros.PermisosPorSociedad(modelBuilder);
            ModeloDeTerceros.DireccionesDeUnaSociedad(modelBuilder);
            ModeloDeTerceros.TrazasDeUnaSociedad(modelBuilder);
            ModeloDeTerceros.ContactosDeUnaSociedad(modelBuilder);
            ModeloDeTerceros.ArchivadoresDeUnaSociedad(modelBuilder);
            ModeloDeTerceros.CuentaDeMiSociedad(modelBuilder);
            ModeloDeTerceros.TarjetaDeMiSociedad(modelBuilder);            
            ModeloDeTerceros.ParametrosDeMiSociedad(modelBuilder);
            ModeloDeTerceros.BuzonDeMiSociedad(modelBuilder);
            ModeloDeTerceros.FacturadorDeSociedades(modelBuilder);

            ModeloDeTerceros.Persona(modelBuilder);
            ModeloDeTerceros.AuditoriaDeUnaPersona(modelBuilder);
            ModeloDeTerceros.ArchivosDeUnaPersona(modelBuilder);
            ModeloDeTerceros.ObservacionesDeUnaPersona(modelBuilder);
            ModeloDeTerceros.DireccionesDeUnaPersona(modelBuilder);
            ModeloDeTerceros.TrazasDeUnaPersona(modelBuilder);
            ModeloDeTerceros.ArchivadoresDeUnaPersona(modelBuilder);

            ModeloDeTerceros.Interlocutor(modelBuilder);
            ModeloDeTerceros.InterlocutorAudt(modelBuilder);
            ModeloDeTerceros.ArchivosDeUnInterlocutor(modelBuilder);
            ModeloDeTerceros.ObservacionesDeUnInterlocutor(modelBuilder);
            ModeloDeTerceros.DireccionesDeUnInterlocutor(modelBuilder);
            ModeloDeTerceros.TrazasDeUnInterlocutor(modelBuilder);
            ModeloDeTerceros.CuentaDeInterlocutor(modelBuilder);


            ModeloDeTerceros.ClaseDeJuzgado(modelBuilder);
            ModeloDeTerceros.Juzgado(modelBuilder);

            ModeloDeTerceros.Procurador(modelBuilder);
            ModeloDeTerceros.ProcuradorAudt(modelBuilder);
            ModeloDeTerceros.ArchivosDeUnProcurador(modelBuilder);
            ModeloDeTerceros.ObservacionesDeUnProcurador(modelBuilder);
            ModeloDeTerceros.DireccionesDeUnProcurador(modelBuilder);
            ModeloDeTerceros.TrazasDeUnProcurador(modelBuilder);

            ModeloDeTerceros.Banco(modelBuilder);
            ModeloDeTerceros.BancoAudt(modelBuilder);       

            ModeloDeTerceros.Abogado(modelBuilder);
            ModeloDeTerceros.AbogadoAudt(modelBuilder);
            ModeloDeTerceros.ArchivosDeUnAbogado(modelBuilder);
            ModeloDeTerceros.ObservacionesDeUnAbogado(modelBuilder);
            ModeloDeTerceros.DireccionesDeUnAbogado(modelBuilder);
            ModeloDeTerceros.TrazasDeUnAbogado(modelBuilder);

            ModeloDeTerceros.Proveedor(modelBuilder);
            ModeloDeTerceros.ProveedorAudt(modelBuilder);
            ModeloDeTerceros.ArchivosDeUnProveedor(modelBuilder);
            ModeloDeTerceros.ObservacionesDeUnProveedor(modelBuilder);
            ModeloDeTerceros.DireccionesDeUnProveedor(modelBuilder);
            ModeloDeTerceros.TrazasDeUnProveedor(modelBuilder);
            ModeloDeTerceros.CuentaDeProveedor(modelBuilder);

            ModeloDeTerceros.Cliente(modelBuilder);
            ModeloDeTerceros.ClienteAudt(modelBuilder);
            ModeloDeTerceros.ArchivosDeUnCliente(modelBuilder);
            ModeloDeTerceros.ObservacionesDeUnCliente(modelBuilder);
            ModeloDeTerceros.DireccionesDeUnCliente(modelBuilder);
            ModeloDeTerceros.TrazasDeUnCliente(modelBuilder);
            ModeloDeTerceros.CuentaDeCliente(modelBuilder);
            ModeloDeTerceros.UsuarioDeUnCliente(modelBuilder);
            ModeloDeTerceros.PuestoDeUnCliente(modelBuilder);
            ModeloDeTerceros.Archivadores(modelBuilder);
            ModeloDeTerceros.CentroAdministrativo(modelBuilder);

            ModeloDeTerceros.Trabajador(modelBuilder);
            ModeloDeTerceros.TrabajadorAudt(modelBuilder);
            ModeloDeTerceros.ArchivosDeUnTrabajador(modelBuilder);
            ModeloDeTerceros.CircuitosCadDeUnTrabajador(modelBuilder);
            ModeloDeTerceros.ObservacionesDeUnTrabajador(modelBuilder);
            ModeloDeTerceros.DireccionesDeUnTrabajador(modelBuilder);
            ModeloDeTerceros.TrazasDeUnTrabajador(modelBuilder);
            ModeloDeTerceros.CuentaDeTrabajador(modelBuilder);

        }

        private static void DefinirTablasDelEsquemaDeRegistroEs(ModelBuilder modelBuilder)
        {
            ModeloDeRegistroEs.EstadosDeUnRegistroEs(modelBuilder);
            ModeloDeRegistroEs.TransicionesDeUnRegistroEs(modelBuilder);
            ModeloDeRegistroEs.AccionesDeUnRegistroEs(modelBuilder);
            ModeloDeRegistroEs.TipoDeRegistroEs(modelBuilder);
            ModeloDeRegistroEs.RegistroEs(modelBuilder);
            ModeloDeRegistroEs.Trazas(modelBuilder);
            ModeloDeRegistroEs.Auditoria(modelBuilder);
            ModeloDeRegistroEs.Archivos(modelBuilder);
            ModeloDeRegistroEs.Agenda(modelBuilder);
            ModeloDeRegistroEs.Observaciones(modelBuilder);
            ModeloDeRegistroEs.Permisos(modelBuilder);
            ModeloDeRegistroEs.Direcciones(modelBuilder);
            ModeloDeRegistroEs.Historia(modelBuilder);
            ModeloDeRegistroEs.Archivadores(modelBuilder);
            ModeloDeRegistroEs.Interlocutores(modelBuilder);
        }
        private static void DefinirTablasDelEsquemaDeTareas(ModelBuilder modelBuilder)
        {
            ModeloDeTarea.EstadosDeUnaTarea(modelBuilder);
            ModeloDeTarea.TransicionesDeUnaTarea(modelBuilder);
            ModeloDeTarea.AccionesDeUnaTarea(modelBuilder);
            ModeloDeTarea.TipoDeTarea(modelBuilder);
            ModeloDeTarea.ClaseDelTipo(modelBuilder);
            ModeloDeTarea.Tarea(modelBuilder);
            ModeloDeTarea.Trazas(modelBuilder);
            ModeloDeTarea.Auditoria(modelBuilder);
            ModeloDeTarea.Archivos(modelBuilder);
            ModeloDeTarea.Observaciones(modelBuilder);
            ModeloDeTarea.Permisos(modelBuilder);
            ModeloDeTarea.Direcciones(modelBuilder);
            ModeloDeTarea.Historia(modelBuilder);
            ModeloDeTarea.Archivadores(modelBuilder);
            ModeloDeTarea.Interlocutores(modelBuilder);
            ModeloDeTarea.Agenda(modelBuilder);
            ModeloDeTarea.PlfDeTarea(modelBuilder);
            ModeloDeRegistroEs.Tareas(modelBuilder);
        }

        private static void DefinirTablasDelEsquemaDeExpediente(ModelBuilder modelBuilder)
        {
            ModeloDeExpediente.EstadosDeUnExpediente(modelBuilder);
            ModeloDeExpediente.TransicionesDeUnExpediente(modelBuilder);
            ModeloDeExpediente.AccionesDeUnExpediente(modelBuilder);
            ModeloDeExpediente.TipoDeExpediente(modelBuilder);
            ModeloDeExpediente.Expediente(modelBuilder);
            ModeloDeExpediente.PlantillaPorTipo(modelBuilder);
            ModeloDeExpediente.Trazas(modelBuilder);
            ModeloDeExpediente.Auditoria(modelBuilder);
            ModeloDeExpediente.Agenda(modelBuilder);
            ModeloDeExpediente.Archivos(modelBuilder);
            ModeloDeExpediente.Expedientes(modelBuilder); 
            ModeloDeExpediente.Observaciones(modelBuilder);
            ModeloDeExpediente.Permisos(modelBuilder);
            ModeloDeExpediente.Direcciones(modelBuilder);
            ModeloDeExpediente.Historia(modelBuilder);
            ModeloDeExpediente.Archivadores(modelBuilder);
            ModeloDeExpediente.Interlocutores(modelBuilder);
            ModeloDeExpediente.Tareas(modelBuilder);
            ModeloDeExpediente.Registros(modelBuilder);
            ModeloDeExpediente.DatosJuridicos(modelBuilder);
            ModeloDeExpediente.ApuntesDeExpediente(modelBuilder);
            ModeloDeExpediente.CircuitosCadDeUnExpediente(modelBuilder);
        }

        private static void DefinirTablasDelModeloDePleito(ModelBuilder modelBuilder)
        {
            ModeloDePleito.EstadosDeUnPleito(modelBuilder);
            ModeloDePleito.TransicionesDeUnPleito(modelBuilder);
            ModeloDePleito.AccionesDeUnPleito(modelBuilder);
            ModeloDePleito.TipoDePleito(modelBuilder);
            ModeloDePleito.PlantillaPorTipo(modelBuilder);
            ModeloDePleito.Pleito(modelBuilder);
            ModeloDePleito.Trazas(modelBuilder);
            ModeloDePleito.Auditoria(modelBuilder);
            ModeloDePleito.Agenda(modelBuilder);
            ModeloDePleito.Archivos(modelBuilder);
            ModeloDePleito.Observaciones(modelBuilder);
            ModeloDePleito.Permisos(modelBuilder);
            ModeloDePleito.Direcciones(modelBuilder);
            ModeloDePleito.Historia(modelBuilder);
            ModeloDePleito.Archivadores(modelBuilder);
            ModeloDePleito.Interlocutores(modelBuilder);
            ModeloDePleito.Tareas(modelBuilder);
            ModeloDePleito.Registros(modelBuilder);

            ModeloDePleito.DatosDeRecobro(modelBuilder);
            ModeloDePleito.DatosDeMinuta(modelBuilder);
        }
        private static void DefinirTablasDelModeloDeContratos(ModelBuilder modelBuilder)
        {
            ModeloDeContrato.EstadosDeUnContrato(modelBuilder);
            ModeloDeContrato.TransicionesDeUnContrato(modelBuilder);
            ModeloDeContrato.AccionesDeUnContrato(modelBuilder);
            ModeloDeContrato.TipoDeContrato(modelBuilder);
            ModeloDeContrato.Contrato(modelBuilder);
            ModeloDeContrato.Trazas(modelBuilder);
            ModeloDeContrato.Auditoria(modelBuilder);
            ModeloDeContrato.Agenda(modelBuilder);
            ModeloDeContrato.Archivos(modelBuilder);
            ModeloDeContrato.Observaciones(modelBuilder);
            ModeloDeContrato.Permisos(modelBuilder);
            ModeloDeContrato.Direcciones(modelBuilder);
            ModeloDeContrato.Historia(modelBuilder);
            ModeloDeContrato.Archivadores(modelBuilder);
            ModeloDeContrato.Interlocutores(modelBuilder);
            ModeloDeContrato.Tareas(modelBuilder);
            ModeloDeContrato.Registros(modelBuilder);
            ModeloDeContrato.DatosDeAvance(modelBuilder);
            ModeloDeContrato.DatosDeProrroga(modelBuilder);
            ModeloDeContrato.SaldosDelContrato(modelBuilder);
            ModeloDeContrato.AvalSolicitadoDelContrato(modelBuilder);
            ModeloDeContrato.DatosJuridicosDelContrato(modelBuilder);
            ModeloDeContrato.DatosDeMatriculaDeGuarderia(modelBuilder);

            ModeloDeContrato.LoteDeUnContrato(modelBuilder);
            ModeloDeContrato.LoteDeUnContratoAudt(modelBuilder);
            ModeloDeContrato.UnitariosDeUnLote(modelBuilder);

            ModeloDeContrato.PlanificadorDeVenta(modelBuilder);
            ModeloDeContrato.PlanificadorDeVentaAudt(modelBuilder);
            ModeloDeContrato.DatosDeLineaDeUnPlfVenta(modelBuilder);
        }

        private static void DefinirTablasMt(ModelBuilder modelBuilder)
        {
            ModeloDeMt.UnidadesDeMedida(modelBuilder);
            ModeloDeMt.NaturalezaContable(modelBuilder);
            ModeloDeMt.Unitarios(modelBuilder);
            ModeloDeMt.UnitarioAudt(modelBuilder);
            ModeloDeMt.TarifasDeUnUnitario(modelBuilder);
        }


        private static void DefinirTablasContables(ModelBuilder modelBuilder)
        {
            ModeloContable.Cuentas(modelBuilder);
            ModeloContable.IvasRepercutido(modelBuilder);
            ModeloContable.IvasSoportados(modelBuilder);
            ModeloContable.Irpfs(modelBuilder);
            ModeloContable.CuentasBancarias(modelBuilder);

            ModeloDePreasiento.EstadosDeUnPreasiento(modelBuilder);
            ModeloDePreasiento.TransicionesDeUnPreasiento(modelBuilder);
            ModeloDePreasiento.AccionesDeUnPreasiento(modelBuilder);
            ModeloDePreasiento.TipoDePreasiento(modelBuilder);

            ModeloDePreasiento.Preasiento(modelBuilder);
            ModeloDePreasiento.Trazas(modelBuilder);
            ModeloDePreasiento.Auditoria(modelBuilder);
            ModeloDePreasiento.Observaciones(modelBuilder);
            ModeloDePreasiento.Historia(modelBuilder);
            ModeloDePreasiento.Archivadores(modelBuilder);
            ModeloDePreasiento.Circuitos(modelBuilder);
            ModeloDePreasiento.Permisos(modelBuilder);

            ModeloDePreasiento.ApuntesDeUnPreasiento(modelBuilder);
        }


        private static void DefinirTablasDelEsquemaDePresupuesto(ModelBuilder modelBuilder)
        {
            ModeloDePresupuesto.EstadosDeUnPresupuesto(modelBuilder);
            ModeloDePresupuesto.TransicionesDeUnPresupuesto(modelBuilder);
            ModeloDePresupuesto.AccionesDeUnPresupuesto(modelBuilder);
            ModeloDePresupuesto.TipoDePresupuesto(modelBuilder);

            ModeloDePresupuesto.Presupuesto(modelBuilder);
            ModeloDePresupuesto.Trazas(modelBuilder);
            ModeloDePresupuesto.Auditoria(modelBuilder);
            ModeloDePresupuesto.Agenda(modelBuilder);
            ModeloDePresupuesto.Archivos(modelBuilder);
            ModeloDePresupuesto.Observaciones(modelBuilder);
            ModeloDePresupuesto.Permisos(modelBuilder);
            ModeloDePresupuesto.Direcciones(modelBuilder);
            ModeloDePresupuesto.Historia(modelBuilder);
            ModeloDePresupuesto.Archivadores(modelBuilder);
            ModeloDePresupuesto.Tareas(modelBuilder);

            ModeloDePresupuesto.PptDeVenta(modelBuilder);
            ModeloDePresupuesto.DatosDeLineaDeUnPpt(modelBuilder);
        }
        private static void DefinirTablasDePartesDeTrabajo(ModelBuilder modelBuilder)
        {
            ModeloDeParteTr.EstadosDeUnParteTr(modelBuilder);
            ModeloDeParteTr.TransicionesDeUnParteTr(modelBuilder);
            ModeloDeParteTr.AccionesDeUnParteTr(modelBuilder);
            ModeloDeParteTr.TipoDeParteTr(modelBuilder);

            ModeloDeParteTr.ParteTr(modelBuilder);
            ModeloDeParteTr.Trazas(modelBuilder);
            ModeloDeParteTr.Auditoria(modelBuilder);
            ModeloDeParteTr.Agenda(modelBuilder);
            ModeloDeParteTr.Archivos(modelBuilder);
            ModeloDeParteTr.Observaciones(modelBuilder);
            ModeloDeParteTr.Permisos(modelBuilder);
            ModeloDeParteTr.Direcciones(modelBuilder);
            ModeloDeParteTr.Historia(modelBuilder);
            ModeloDeParteTr.Archivadores(modelBuilder);
            ModeloDeParteTr.Tareas(modelBuilder);

            ModeloDeParteTr.DatosDeLineaDeUnParteTr(modelBuilder);
            ModeloDeParteTr.AsignacionDePtR(modelBuilder);
        }
        private static void DefinirTablasDeFacturasEmitidas(ModelBuilder modelBuilder)
        {
            ModeloDeFacturaEmt.EstadosDeUnaFacturaEmt(modelBuilder);
            ModeloDeFacturaEmt.TransicionesDeUnaFacturaEmt(modelBuilder);
            ModeloDeFacturaEmt.AccionesDeUnaFacturaEmt(modelBuilder);
            ModeloDeFacturaEmt.TipoDeFacturaEmt(modelBuilder);

            ModeloDeFacturaEmt.FacturaEmt(modelBuilder);
            ModeloDeFacturaEmt.PlantillaPorTipo(modelBuilder);
            ModeloDeFacturaEmt.Trazas(modelBuilder);
            ModeloDeFacturaEmt.Auditoria(modelBuilder);
            ModeloDeFacturaEmt.Agenda(modelBuilder);
            ModeloDeFacturaEmt.Archivos(modelBuilder);
            ModeloDeFacturaEmt.Observaciones(modelBuilder);
            ModeloDeFacturaEmt.Permisos(modelBuilder);
            ModeloDeFacturaEmt.Direcciones(modelBuilder);
            ModeloDeFacturaEmt.Historia(modelBuilder);
            ModeloDeFacturaEmt.Archivadores(modelBuilder);
            ModeloDeFacturaEmt.Circuitos(modelBuilder);
            ModeloDeFacturaEmt.CobroDeFae(modelBuilder);
            ModeloDeFacturaEmt.AbonoDeFae(modelBuilder);

            ModeloDeFacturaEmt.DatosDeLineaDeUnaFacturaEmt(modelBuilder);
            ModeloDeFacturaEmt.Rectificativa(modelBuilder);
            ModeloDeFacturaEmt.PeriodoEmt(modelBuilder);
            ModeloDeFacturaEmt.IrpfEmt(modelBuilder);
            ModeloDeFacturaEmt.VerifactuBd(modelBuilder);
            ModeloDeFacturaEmt.LogDeEnvioDeFactura(modelBuilder);
            ModeloDeFacturaEmt.PeticionDeFacturaEmt(modelBuilder);
        }
        private static void DefinirTablasDePlanificaciondeVenta(ModelBuilder modelBuilder)
        {
            ModeloDePlanificacionDeVenta.EstadosDeUnaPlanificacionDeVenta(modelBuilder);
            ModeloDePlanificacionDeVenta.TransicionesDeUnaPlanificacionDeVenta(modelBuilder);
            ModeloDePlanificacionDeVenta.AccionesDeUnaPlanificacionDeVenta(modelBuilder);
            ModeloDePlanificacionDeVenta.TipoDePlanificacionDeVenta(modelBuilder);

            ModeloDePlanificacionDeVenta.PlanificacionDeVenta(modelBuilder);
            ModeloDePlanificacionDeVenta.Trazas(modelBuilder);
            ModeloDePlanificacionDeVenta.Auditoria(modelBuilder);
            ModeloDePlanificacionDeVenta.Agenda(modelBuilder);
            ModeloDePlanificacionDeVenta.Archivos(modelBuilder);
            ModeloDePlanificacionDeVenta.Observaciones(modelBuilder);
            ModeloDePlanificacionDeVenta.Permisos(modelBuilder);
            ModeloDePlanificacionDeVenta.Direcciones(modelBuilder);
            ModeloDePlanificacionDeVenta.Historia(modelBuilder);

            ModeloDePlanificacionDeVenta.DatosDeLineaDeUnaPlfVenta(modelBuilder);
        }
        private static void DefinirTablasDeRemesasDeFacturas(ModelBuilder modelBuilder)
        {
            ModeloDeRemesaFae.EstadosDeUnaRemesaFae(modelBuilder);
            ModeloDeRemesaFae.TransicionesDeUnaRemesaFae(modelBuilder);
            ModeloDeRemesaFae.AccionesDeUnaRemesaFae(modelBuilder);
            ModeloDeRemesaFae.TipoDeRemesaFae(modelBuilder);

            ModeloDeRemesaFae.RemesaFae(modelBuilder);
            ModeloDeRemesaFae.Trazas(modelBuilder);
            ModeloDeRemesaFae.Auditoria(modelBuilder);
            ModeloDeRemesaFae.Agenda(modelBuilder);
            ModeloDeRemesaFae.Archivos(modelBuilder);
            ModeloDeRemesaFae.Observaciones(modelBuilder);
            ModeloDeRemesaFae.Permisos(modelBuilder);
            ModeloDeRemesaFae.Historia(modelBuilder);
            ModeloDeRemesaFae.Archivadores(modelBuilder);
            ModeloDeRemesaFae.FacturasEmtDeUnaRemesa(modelBuilder);
        }

        private static void DefinirTablasDeCircuitosDoc(ModelBuilder modelBuilder)
        {
            ModeloDeCircuitoDoc.EstadosDeUnCircuitoDoc(modelBuilder);
            ModeloDeCircuitoDoc.TransicionesDeUnCircuitoDoc(modelBuilder);
            ModeloDeCircuitoDoc.AccionesDeUnCircuitoDoc(modelBuilder);
            ModeloDeCircuitoDoc.TipoDeCircuitoDoc(modelBuilder);

            ModeloDeCircuitoDoc.CircuitoDoc(modelBuilder);
            ModeloDeCircuitoDoc.Trazas(modelBuilder);
            ModeloDeCircuitoDoc.Auditoria(modelBuilder);
            ModeloDeCircuitoDoc.Agenda(modelBuilder);
            ModeloDeCircuitoDoc.Archivos(modelBuilder);
            ModeloDeCircuitoDoc.Observaciones(modelBuilder);
            ModeloDeCircuitoDoc.Permisos(modelBuilder);
            ModeloDeCircuitoDoc.Historia(modelBuilder);
            ModeloDeCircuitoDoc.Archivadores(modelBuilder);
            ModeloDeCircuitoDoc.DatosDeActividadFormativa(modelBuilder);
            ModeloDeCircuitoDoc.InscritosEnActividad(modelBuilder);
            ModeloDeCircuitoDoc.VoluntarioDeActivida(modelBuilder);
        }

        private static void DefinirTablasDePagos(ModelBuilder modelBuilder)
        {
            ModeloDePago.EstadosDeUnPago(modelBuilder);
            ModeloDePago.TransicionesDeUnPago(modelBuilder);
            ModeloDePago.AccionesDeUnPago(modelBuilder);
            ModeloDePago.TipoDePago(modelBuilder);

            ModeloDePago.Pago(modelBuilder);
            ModeloDePago.Trazas(modelBuilder);
            ModeloDePago.Auditoria(modelBuilder);
            ModeloDePago.Agenda(modelBuilder);
            ModeloDePago.Archivos(modelBuilder);
            ModeloDePago.Observaciones(modelBuilder);
            ModeloDePago.Permisos(modelBuilder);
            ModeloDePago.Historia(modelBuilder);
            ModeloDePago.Archivadores(modelBuilder);
            ModeloDePago.Circuitos(modelBuilder);
        }


        private static void DefinirTablasDeGastos(ModelBuilder modelBuilder)
        {
            //ModeloDeGasto.EstadosDeUnGasto(modelBuilder);
            //ModeloDeGasto.TransicionesDeUnGasto(modelBuilder);
            //ModeloDeGasto.AccionesDeUnGasto(modelBuilder);
            //ModeloDeGasto.TipoDeGasto(modelBuilder);
        }

        private static void DefinirTablasDeRemesasDePagos(ModelBuilder modelBuilder)
        {
            ModeloDeRemesaPag.EstadosDeUnaRemesaPag(modelBuilder);
            ModeloDeRemesaPag.TransicionesDeUnaRemesaPag(modelBuilder);
            ModeloDeRemesaPag.AccionesDeUnaRemesaPag(modelBuilder);
            ModeloDeRemesaPag.TipoDeRemesaPag(modelBuilder);

            ModeloDeRemesaPag.RemesaPag(modelBuilder);
            ModeloDeRemesaPag.Trazas(modelBuilder);
            ModeloDeRemesaPag.Auditoria(modelBuilder);
            ModeloDeRemesaPag.Agenda(modelBuilder);
            ModeloDeRemesaPag.Archivos(modelBuilder);
            ModeloDeRemesaPag.Observaciones(modelBuilder);
            ModeloDeRemesaPag.Permisos(modelBuilder);
            ModeloDeRemesaPag.Historia(modelBuilder);
            ModeloDeRemesaPag.Archivadores(modelBuilder);
            ModeloDeRemesaPag.PagosDeUnaRemesa(modelBuilder);
        }

        private static void DefinirTablasDeGuarderias(ModelBuilder modelBuilder)
        {
            ModeloDeGuarderias.AulaDeGuarderia(modelBuilder);
            ModeloDeGuarderias.Infante(modelBuilder);
            ModeloDeGuarderias.CursoDeGuarderia(modelBuilder);
            ModeloDeGuarderias.InfanteDeUnCurso(modelBuilder);

            ModeloDeGuarderias.ArchivadoresDeUnInfante(modelBuilder);
            ModeloDeGuarderias.AuditoriaDeUnInfante(modelBuilder);
            ModeloDeGuarderias.ArchivosDeUnInfante(modelBuilder);
            ModeloDeGuarderias.ObservacionesDeUnInfante(modelBuilder);
            ModeloDeGuarderias.TrazasDeUnInfante(modelBuilder);
            ModeloDeGuarderias.AgendaDeUnInfante(modelBuilder);

            ModeloDeGuarderias.ArchivadoresDeUnCursoDeGuarderia(modelBuilder);
            ModeloDeGuarderias.AuditoriaDeUnCursoDeGuarderia(modelBuilder);
            ModeloDeGuarderias.ArchivosDeUnCursoDeGuarderia(modelBuilder);
            ModeloDeGuarderias.ObservacionesDeUnCursoDeGuarderia(modelBuilder);
            ModeloDeGuarderias.TrazasDeUnCursoDeGuarderia(modelBuilder);
            ModeloDeGuarderias.ArchivadoresDeUnCursoDeGuarderia(modelBuilder);
            ModeloDeGuarderias.AgendaDeUnCurso(modelBuilder);
            ModeloDeGuarderias.ProfesoresDeUnCurso(modelBuilder);
        }
    }
}
