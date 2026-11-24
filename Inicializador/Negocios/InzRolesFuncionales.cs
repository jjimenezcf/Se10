using GestorDeElementos;
using Gestor.Errores;
using GestoresDeNegocio.Seguridad;
using ServicioDeDatos;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Seguridad;
using System.Collections.Generic;
using System.Linq;
using Utilidades;
using static SistemaDeElementos.Inicializador.InzMenus;

namespace SistemaDeElementos.Inicializador
{
    /// <summary>
    /// Roles de acceso funcional (AFB) por punto de menú: un rol "AFB: [ruta del menú]" por cada menú del que cuelgan opciones
    /// que muestran un crud, con el permiso de la vista de cada una de esas opciones (no incluye las opciones que lanzan procesos:
    /// inicializar, definir maestros, someter trabajos, importar...). Incluye también las vistas secundarias que se abren desde esos
    /// cruds (carpetas de un archivador, barrios, lotes de un contrato...), que exigen su propio permiso de vista.
    /// Documentados en wwwroot/ayuda/seguridad/RolesFuncionales.md
    /// </summary>
    public static class InzRolesFuncionales
    {
        public const string PrefijoDeRol = "AFB: ";
        public const string SeparadorDeRuta = ".";
        private const string DescripcionDelRol = "Acceso funcional a las opciones del menú {0}";
        private const string VistaNoDefinida = "No está definida la vista '{0}' que necesita el rol '{1}'";

        public static string NombreDelRol(params string[] ruta) => $"{PrefijoDeRol}{string.Join(SeparadorDeRuta, ruta)}";

        public static void DefinirRoles(ContextoSe contexto)
        {
            contexto.IniciarTraza(nameof(DefinirRoles));
            var tran = contexto.IniciarTransaccion();
            try
            {
                contexto.DatosDeConexion.CreandoModelo = true;
                var gestor = GestorDePermisosDeUnRol.Gestor(contexto, contexto.Mapeador);
                foreach (var (ruta, vistas) in RolesFuncionales())
                    CrearRolSiNoExiste(contexto, gestor, ruta, vistas);
                contexto.Commit(tran);
            }
            catch
            {
                contexto.Rollback(tran);
                throw;
            }
            finally
            {
                contexto.CerrarTraza();
                contexto.DatosDeConexion.CreandoModelo = false;
            }
        }

        // si el rol ya existe no se toca: sus permisos pueden haberse ajustado a mano
        private static void CrearRolSiNoExiste(ContextoSe contexto, GestorDePermisosDeUnRol gestor, string[] ruta, string[] vistas)
        {
            var nombre = NombreDelRol(ruta);
            if (contexto.SeleccionarPorPropiedad<RolDtm>(nameof(RolDtm.Nombre), nombre, errorSiNoHay: false) != null)
                return;

            var rol = new RolDtm
            {
                Nombre = nombre,
                Descripcion = string.Format(DescripcionDelRol, string.Join(SeparadorDeRuta, ruta))
            }.Insertar(contexto);

            foreach (var vista in vistas.Distinct())
            {
                var vistaMvc = contexto.Set<VistaMvcDtm>().FirstOrDefault(x => x.Nombre == vista);
                if (vistaMvc == null)
                    GestorDeErrores.Emitir(string.Format(VistaNoDefinida, vista, nombre));
                gestor.CrearRelacion(nameof(PermisosDeUnRolDtm.IdRol), rol.Id, vistaMvc.IdPermiso, errorSiYaExiste: false);
            }
        }

        private static (string[] Ruta, string[] Vistas) Rol(string[] ruta, params string[] vistas) => (ruta, vistas);

        private static string[] Ruta(params string[] menus) => menus;

        // estados, transiciones y tipos del negocio que cuelga de la configuración de un módulo
        private static (string[] Ruta, string[] Vistas) ConfiguracionDeProceso(string modulo, string menuDelNegocio, string vistaDeTipo)
        =>
        Rol(Ruta(modulo, Configuracion, menuDelNegocio), enumVistas.Negocio.Estados, enumVistas.Negocio.Transiciones, vistaDeTipo);

        public static List<(string[] Ruta, string[] Vistas)> RolesFuncionales() =>
        [
            // Configuración del entorno
            Rol(Ruta(Configuracion), enumVistas.Entorno.Correos, enumVistas.Entorno.TrabajosSe, enumVistas.Entorno.Acciones),
            Rol(Ruta(Configuracion, Funcionalidad), enumVistas.Entorno.VistasDeSe, enumVistas.Entorno.Variables, enumVistas.Entorno.MenusSe, enumVistas.Entorno.ArbolDeMenus, enumVistas.Entorno.Agendas, enumVistas.Entorno.Certificados,
                enumVistas.Entorno.VisorDeAgenda),
            Rol(Ruta(Configuracion, Accesos), enumVistas.Entorno.Usuarios, enumVistas.Entorno.PuestosDeTrabajo, enumVistas.Entorno.Roles, enumVistas.Entorno.Permisos, enumVistas.Entorno.Seguridad,
                enumVistas.Entorno.ClasesDePermisos, enumVistas.Entorno.PuestosDeUnUsuario, enumVistas.Entorno.RolesDeUnPuesto, enumVistas.Entorno.PermisosDeUnRol, enumVistas.Entorno.PermisosDeUnUsuario,
                enumVistas.Entorno.RolesDeUnPermiso, enumVistas.Entorno.PuestosDeUnRol, enumVistas.Entorno.UsuariosDeUnPuesto, enumVistas.Entorno.PermisosDeUnPuesto),
            Rol(Ruta(Configuracion, Negocios), enumVistas.Configuracion.NegociosDeSE, enumVistas.Negocio.AccionesDeRelacion,
                enumVistas.Configuracion.ParametrosDeNegocio, enumVistas.Configuracion.PlantillasDeExportacion),

            // Maestros
            Rol(Ruta(Maestros), enumVistas.Terceros.CentrosGestores),
            Rol(Ruta(Maestros, Callejero), enumVistas.Callejero.TipoDeVia, enumVistas.Callejero.Paises, enumVistas.Callejero.Provincias, enumVistas.Callejero.Municipios, enumVistas.Callejero.Calles, enumVistas.Callejero.CodigosPostales,
                enumVistas.Callejero.CpsDeProv, enumVistas.Callejero.CpsDeMuni, enumVistas.Callejero.Barrios, enumVistas.Callejero.CallesDeUnBarrio, enumVistas.Callejero.Zonas, enumVistas.Callejero.CallesDeUnaZona),
            Rol(Ruta(Maestros, Terceros), enumVistas.Terceros.Personas, enumVistas.Terceros.Sociedades, enumVistas.Terceros.Interlocutores, enumVistas.Terceros.Bancos),
            Rol(Ruta(Maestros, Logistica), enumVistas.Terceros.Proveedores, enumVistas.Terceros.Clientes),
            Rol(Ruta(Maestros, Juridico), enumVistas.Terceros.ClasesDeJuzgados, enumVistas.Terceros.Juzgados, enumVistas.Terceros.Procuradores, enumVistas.Terceros.Abogados),

            // Maestros técnicos
            Rol(Ruta(ModuloMts), enumVistas.Mts.Unitarios),
            Rol(Ruta(ModuloMts, Configuracion), enumVistas.Mts.Unidades, enumVistas.Mts.Naturalezas),

            // Sistema documental
            Rol(Ruta(SistemaDocumental), enumVistas.SistemaDocumental.Archivadores, enumVistas.SistemaDocumental.CircuitosDoc,
                enumVistas.SistemaDocumental.Carpetas),
            Rol(Ruta(SistemaDocumental, Configuracion), enumVistas.SistemaDocumental.TipoDeArchivadores),
            ConfiguracionDeProceso(SistemaDocumental, enumNegocio.CircuitoDoc.MenuConfiguaracion(), enumVistas.SistemaDocumental.TipoDeCircuitosDoc),

            // Contabilidad
            Rol(Ruta(ModuloContable), enumVistas.Contabilidad.CierresContables, enumVistas.Contabilidad.Preasientos, enumVistas.SistemaDocumental.EstimacionesDirectas, enumVistas.SistemaDocumental.LotesContables),
            Rol(Ruta(ModuloContable, Configuracion), enumVistas.Contabilidad.Cuentas, enumVistas.Contabilidad.IvasSoportado, enumVistas.Contabilidad.Irpfs, enumVistas.Contabilidad.IvasRepercutido),
            ConfiguracionDeProceso(ModuloContable, enumNegocio.Preasiento.MenuConfiguaracion(), enumVistas.Contabilidad.TipoDePreasiento),

            // Gestión administrativa
            Rol(Ruta(GestionAdministrativa), enumVistas.Administracion.RegistrosEs, enumVistas.Administracion.Tareas, enumVistas.Administracion.Expedientes, enumVistas.Administracion.Actividades,
                enumVistas.SistemaDocumental.ActividadesFormativas),
            ConfiguracionDeProceso(GestionAdministrativa, enumNegocio.Registro.ToString(), enumVistas.Administracion.TipoDeRegistros),
            ConfiguracionDeProceso(GestionAdministrativa, enumNegocio.Tarea.ToString(), enumVistas.Administracion.TipoDeTareas),
            ConfiguracionDeProceso(GestionAdministrativa, enumNegocio.Expediente.ToString(), enumVistas.Administracion.TipoDeExpedientes),

            // Recursos humanos
            Rol(Ruta(ModuloRRHH), enumVistas.Terceros.Trabajadores, enumVistas.SistemaDocumental.Fichadas),

            // Guarderías (las matrículas se gestionan con la vista de contratos)
            Rol(Ruta(ModuloDeGuarderias), enumVistas.Guarderias.Aulas, enumVistas.Guarderias.Infantes, enumVistas.Guarderias.Cursos, enumVistas.Juridico.Contratos,
                enumVistas.Guarderias.InfantesDeUnCurso),

            // Jurídico (los procedimientos se gestionan con la vista de expedientes; los contratos de venta y compra con la de contratos)
            Rol(Ruta(ModuloJuridico), enumVistas.Administracion.Expedientes, enumVistas.Juridico.Pleitos, enumVistas.Juridico.Contratos,
                enumVistas.Juridico.Lotes, enumVistas.Juridico.UnitariosDeUnLote, enumVistas.Juridico.PlanificadorDeVentas),
            ConfiguracionDeProceso(ModuloJuridico, enumNegocio.Pleito.MenuConfiguaracion(), enumVistas.Juridico.TipoDePleitos),
            ConfiguracionDeProceso(ModuloJuridico, enumNegocio.Contrato.MenuConfiguaracion(), enumVistas.Juridico.TipoDeContratos),

            // Ventas
            Rol(Ruta(ModuloDeVentas), enumVistas.Ventas.Presupuestos, enumVistas.Ventas.PlanificacionesDeVenta, enumVistas.Ventas.PartesTr, enumVistas.Ventas.AsignacionesDePtr, enumVistas.Ventas.FacturasEmt, enumVistas.Ventas.RemesasFae, enumVistas.Ventas.FacturasAeat,
                enumVistas.Ventas.FacturasEmtDeUnaRemesa, enumVistas.Juridico.PlanificadorDeVentas),
            ConfiguracionDeProceso(ModuloDeVentas, enumNegocio.Presupuesto.ToString(), enumVistas.Ventas.TipoDePresupuesto),
            ConfiguracionDeProceso(ModuloDeVentas, enumNegocio.PlanificacionDeVenta.MenuConfiguaracion(), enumVistas.Ventas.TipoDePlanificacionDeVenta),
            ConfiguracionDeProceso(ModuloDeVentas, enumNegocio.ParteDeTrabajo.MenuConfiguaracion(), enumVistas.Ventas.TipoDeParteTr),
            ConfiguracionDeProceso(ModuloDeVentas, enumNegocio.FacturaEmitida.MenuConfiguaracion(), enumVistas.Ventas.TipoDeFacturaEmt),
            ConfiguracionDeProceso(ModuloDeVentas, enumNegocio.RemesaFae.MenuConfiguaracion(), enumVistas.Ventas.TipoDeRemesaFae),

            // Logística
            Rol(Ruta(ModuloDeLogistica), enumVistas.Logistica.Almacenes, enumVistas.Logistica.Pedidos, enumVistas.Logistica.Ventas, enumVistas.Logistica.DevolucionesDeVenta, enumVistas.Logistica.Regularizaciones, enumVistas.Logistica.MovimientosDeAlmacen, enumVistas.Logistica.Inventario),
            ConfiguracionDeProceso(ModuloDeLogistica, enumNegocio.Almacen.MenuConfiguaracion(), enumVistas.Logistica.TipoDeAlmacen),
            ConfiguracionDeProceso(ModuloDeLogistica, enumNegocio.Pedido.MenuConfiguaracion(), enumVistas.Logistica.TipoDePedido),
            ConfiguracionDeProceso(ModuloDeLogistica, enumNegocio.Venta.MenuConfiguaracion(), enumVistas.Logistica.TipoDeVenta),
            ConfiguracionDeProceso(ModuloDeLogistica, enumNegocio.DevolucionVenta.MenuConfiguaracion(), enumVistas.Logistica.TipoDeDevolucionDeVenta),
            ConfiguracionDeProceso(ModuloDeLogistica, enumNegocio.Regularizacion.MenuConfiguaracion(), enumVistas.Logistica.TipoDeRegularizacion),

            // Gastos
            Rol(Ruta(ModuloDeGastos), enumVistas.Gastos.FacturasRec, enumVistas.Gastos.Pagos, enumVistas.Gastos.RemesasPag,
                enumVistas.Gastos.PagosDeUnaRemesa),
            ConfiguracionDeProceso(ModuloDeGastos, enumNegocio.FacturaRecibida.MenuConfiguaracion(), enumVistas.Gastos.TipoDeFacturaRec),
            ConfiguracionDeProceso(ModuloDeGastos, enumNegocio.Pago.MenuConfiguaracion(), enumVistas.Gastos.TipoDePago),
            ConfiguracionDeProceso(ModuloDeGastos, enumNegocio.RemesaPag.MenuConfiguaracion(), enumVistas.Gastos.TipoDeRemesaPag),
        ];
    }
}
