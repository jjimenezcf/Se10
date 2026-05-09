using GestorDeElementos;
using GestoresDeNegocio.Entorno;
using ServicioDeDatos;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.Juridico;
using Utilidades;

namespace SistemaDeElementos.Inicializador
{
    public static class InzMenus
    {
        public const string Maestros = "Maestros";
        public const string Configuracion = "Configuración";
        public const string SistemaDocumental = "Sistema Documental";
        public const string GestionAdministrativa = "Gestión Administrativa";
        public const string Estados = "Estados";
        public const string Transiciones = "Transiciones";
        public const string ModuloJuridico = "Módulo Jurídico";
        public const string ModuloDeVentas = "Módulo de Ventas";
        public const string ModuloDeGastos = "Módulo de Gastos";
        public const string ModuloDeLogistica = "Módulo de Logística";
        public const string ModuloDeGuarderias = "Módulo de Guarderías";
        private const string ModuloMts = "Módulo Mt";
        private const string GrupsoDeNaturaleza = "Grupos por Naturaleza";
        private const string NaturalezasContables = "Naturalezas";
        private const string Unidades = "Unidades";
        private const string ModuloContable = "Módulo Contable";
        private const string ModuloRRHH = "Módulo RRHH";
        private const string Cuentas = "Cuentas";
        private const string IvaRepercutido = "Iva repercutido";
        private const string IvaSoportado = "Iva soportado";
        private const string Irpf = "Irpf";


        public const string Agendas = nameof(Agendas);
        public const string Certificados = nameof(Certificados);

        public static void DefinirMenus(ContextoSe contexto)
        {
            using (var gestor = GestorDeMenus.Gestor(contexto, contexto.Mapeador))
            {
                gestor.Contexto.IniciarTraza(nameof(DefinirMenus));
                var tran = gestor.Contexto.IniciarTransaccion();
                try
                {
                    gestor.Contexto.DatosDeConexion.CreandoModelo = true;
                    MenusDeConfiguracion(gestor);
                    MenusDelCallejero(gestor);
                    MenusDeTerceros(gestor);
                    MenusDeGestionDeMts(gestor);
                    MenusDelSistemaDocumental(gestor);
                    MenusDeGestionContable(gestor);
                    MenusDeGestionAdministrativa(gestor);
                    MenusDeRRHH(gestor);
                    MenusDeGuarderias(gestor);
                    MenusDeGestionJuridica(gestor);
                    MenusDeVentas(gestor);
                    MenusDeLogistica(gestor);
                    MenusDeGastos(gestor);
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

        private static void MenusDeConfiguracion(GestorDeMenus gestor)
        {
            var padre = Configuracion;
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: Configuracion, descripcion: "Configuración de entorno", icono: "configuracion.svg", padre: null, vista: "", orden: 1);
            MenusDeFuncionalidad(gestor, padre);
            MenusDeAccesos(gestor, padre);
            MenusDeNegociosSe(gestor, padre);
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: enumVistas.Entorno.Correos, descripcion: "Registro de Correos", icono: "Correo.svg", padre: Configuracion, vista: enumVistas.Entorno.Correos, orden: 50);
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: enumVistas.Entorno.TrabajosSe, descripcion: "Catálogo de trabajos sometidos", icono: "Programas.svg", padre: Configuracion, vista: enumVistas.Entorno.TrabajosSe, orden: 55);
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: enumVistas.Entorno.TrabajosDeUsuario, descripcion: "Registros de trabajos de usuario", icono: "puestoDeTrabajo_1.svg", padre: Configuracion, vista: enumVistas.Entorno.TrabajosDeUsuario, orden: 60);
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: enumVistas.Entorno.Acciones, descripcion: "Acciones del sistema", icono: "Acciones.svg", padre: Configuracion, vista: enumVistas.Entorno.Acciones, orden: 80);
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: enumVistas.Configuracion.AplicarSeguridad, descripcion: "Aplicar seguridad parametrizada", icono: "seguridad_1.svg", padre: Configuracion, vista: enumVistas.Configuracion.AplicarSeguridad, orden: 98);
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: enumVistas.Configuracion.InicializarBd, descripcion: "Inicializa registros de BD", icono: "cambio_1.svg", padre: Configuracion, vista: enumVistas.Configuracion.InicializarBd, orden: 99);
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: enumVistas.Configuracion.InicializarMaestros, descripcion: "Inicializa maestros", icono: "DatosMaestros_Color.svg", padre: Configuracion, vista: enumVistas.Configuracion.InicializarMaestros, orden: 100);
        }

        private static void MenusDeFuncionalidad(GestorDeMenus gestor, string padre)
        {
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: "Funcionalidad", descripcion: "Funcionalidad del sistema", icono: "funcionalidad-3.svg", padre: padre, vista: "", orden: 1);

            padre = $"{padre}.Funcionalidad";
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: "Vistas", descripcion: "Gestión de las vistas del sistema", icono: "vista.svg", padre: padre, vista: enumVistas.Entorno.VistasDeSe, orden: 0);
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: "Variables", descripcion: "Mantenimiento de variables del sistema", icono: "cog-solid.svg", padre: padre, vista: enumVistas.Entorno.Variables, orden: 10);
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: "Menus", descripcion: "Menús a mostrar en el árbol de la funcionalidad del sistema de elementos...", icono: "cp.svg", padre: padre, vista: enumVistas.Entorno.MenusSe, orden: 20);
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: "Árbol de menús", descripcion: "Menús del sistema SE", icono: "arbol.svg", padre: padre, vista: enumVistas.Entorno.ArbolDeMenus, orden: 40);
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: Agendas, descripcion: "Mantenimiento de agendas del sistema", icono: "Agenda.svg", padre: padre, vista: enumVistas.Entorno.Agendas, orden: 50);
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: Certificados, descripcion: "Mantenimiento de certificados del sistema", icono: "Certificado.svg", padre: padre, vista: enumVistas.Entorno.Certificados, orden: 52);
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: enumVistas.Entorno.InicializarEntorno, descripcion: "Inicializa entorno", icono: "DatosMaestros_Color.svg", padre: padre, vista: enumVistas.Entorno.InicializarEntorno, orden: 100);

        }

        private static void MenusDeAccesos(GestorDeMenus gestor, string padre)
        {
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: "Accesos", descripcion: "Menú para definir los accesos", icono: "accesos.svg", padre: padre, vista: "", orden: 15);

            padre = $"{padre}.Accesos";
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: "Usuarios", descripcion: "Usuarios del sistema", icono: "usuario.svg", padre: padre, vista: enumVistas.Entorno.Usuarios, orden: 1);
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: "Puestos", descripcion: "Puestos de trabajo por CG", icono: "puestoDeTrabajo.svg", padre: padre, vista: enumVistas.Entorno.PuestosDeTrabajo, orden: 10);
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: "Roles", descripcion: "Roles del siste", icono: "roles.svg", padre: padre, vista: enumVistas.Entorno.Roles, orden: 20);
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: "Permisos", descripcion: "Permisos de accesos", icono: "acceso.svg", padre: padre, vista: enumVistas.Entorno.Permisos, orden: 40);
        }

        private static void MenusDeNegociosSe(GestorDeMenus gestor, string padre)
        {
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: "Negocios", descripcion: "Menús de gestión de Negocios", icono: "bars-solid.svg", padre: padre, vista: "", orden: 20);

            padre = $"{padre}.Negocios";
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: enumVistas.Configuracion.NegociosDeSE, descripcion: "Declaración de los negocios del sistema", icono: "red.svg", padre: padre, vista: enumVistas.Configuracion.NegociosDeSE, orden: 1);
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: enumVistas.Negocio.AccionesDeRelacion, descripcion: "Acciones al relacionar dos elemenetos del sistema", icono: "AccionesDeRelacion.svg", padre: padre, vista: enumVistas.Negocio.AccionesDeRelacion, orden: 1);
        }

        private static void MenusDeTerceros(GestorDeMenus gestor)
        {
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: "Terceros", descripcion: "Gestión de terceros", icono: "Terceros.svg", padre: Maestros, vista: "", orden: 5);
            string maestrosTerceros = $"{Maestros}.Terceros";
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: "Personas", descripcion: "Personas del sistema", icono: "Persona.svg", padre: maestrosTerceros, vista: enumVistas.Terceros.Personas, orden: 5);
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: "Sociedades", descripcion: "Sociedades del sistema", icono: "Sociedad.svg", padre: maestrosTerceros, vista: enumVistas.Terceros.Sociedades, orden: 10);
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: "Interlocutores", descripcion: "Interlocutores del sistema", icono: "Interlocutor.svg", padre: maestrosTerceros, vista: enumVistas.Terceros.Interlocutores, orden: 12);
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: enumVistas.Terceros.Bancos, descripcion: "Bancos del sistema", icono: enumNegocio.Banco.Icono(), padre: maestrosTerceros, vista: enumVistas.Terceros.Bancos, orden: 20);

            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: "Logística", descripcion: "Gestión de maestros de logística", icono: "Logistica.svg", padre: Maestros, vista: "", orden: 5);
            string maestrosLogistica = $"{Maestros}.Logística";
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: enumVistas.Terceros.Proveedores, descripcion: "Proveedores del sistema", icono: "Proveedor.svg", padre: maestrosLogistica, vista: enumVistas.Terceros.Proveedores, orden: 5);
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: enumVistas.Terceros.Clientes, descripcion: "Clientes del sistema", icono: "Cliente.svg", padre: maestrosLogistica, vista: enumVistas.Terceros.Clientes, orden: 10);


            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: "Jurídico", descripcion: "Gestión de maestros jurídicos", icono: "Juridico.svg", padre: Maestros, vista: "", orden: 10);
            string maestrosJuridico = $"{Maestros}.Jurídico";

            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: enumVistas.Terceros.ClasesDeJuzgados, descripcion: "Clases de juzgado del sistema", icono: "ClaseDeJuzgado.svg", padre: maestrosJuridico, vista: enumVistas.Terceros.ClasesDeJuzgados, orden: 1);
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: enumVistas.Terceros.Juzgados, descripcion: "Juzgado del sistema", icono: "Juzgado.svg", padre: maestrosJuridico, vista: enumVistas.Terceros.Juzgados, orden: 5);
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: enumVistas.Terceros.Procuradores, descripcion: "Procuradores del sistema", icono: "Procurador.svg", padre: maestrosJuridico, vista: enumVistas.Terceros.Procuradores, orden: 10);
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: enumVistas.Terceros.Abogados, descripcion: "Abogados del sistema", icono: "Abogado.svg", padre: maestrosJuridico, vista: enumVistas.Terceros.Abogados, orden: 20);

            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: "Centros gestores", descripcion: "Centros gestores de una sociedad", icono: "CentroGestor.svg", padre: Maestros, vista: enumVistas.Terceros.CentrosGestores, orden: 15, buscarPorPadre: false);
        }

        private static void MenusDelSistemaDocumental(GestorDeMenus gestor)
        {
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: SistemaDocumental, descripcion: "Módulo del sistema documental", icono: "SistemaDocumental.svg", padre: null, vista: "", orden: 7);
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: Configuracion, descripcion: "Configuración del sistema documental", icono: "configuration-menu.svg", padre: SistemaDocumental, vista: "", orden: 1);
            MenusDeConfiguracionDeProceso(gestor, enumNegocio.CircuitoDoc, padre: $"{SistemaDocumental}.{Configuracion}", enumVistas.SistemaDocumental.TipoDeCircuitosDoc, enumVistas.SistemaDocumental.MaestrosDeCircuitosDoc, orden: 25);

            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: "Series documentales", descripcion: "Configuración de series documentales", icono: "serie-documental.svg", padre: $"{SistemaDocumental}.{Configuracion}", vista: enumVistas.SistemaDocumental.TipoDeArchivadores, orden: 1);
            /* ************************************************************************
            /* Permite crear los tipos documentales asociados a una asesoría financiera 
            GestorDeMenus.CrearMenuSiNoExiste(gestor
                , nombre: enumVistas.SistemaDocumental.CrearArchivadoresEco
                , descripcion: "Crear tipos de archivadores ecónomico financieros"
                , icono: "DatosMaestros_Color.svg"
                , padre: $"{SistemaDocumental}.{Configuracion}"
                , vista: enumVistas.SistemaDocumental.CrearArchivadoresEco
                , orden: 5);
           */
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: "Archivadores ", descripcion: "Archivadores del sistema", icono: "Archivadores.svg", padre: SistemaDocumental, vista: enumVistas.SistemaDocumental.Archivadores, orden: 10);

            GestorDeMenus.CrearMenuSiNoExiste(gestor
                , nombre: enumVistas.SistemaDocumental.CircuitosDoc
                , descripcion: $"Gestión de los {enumNegocio.CircuitoDoc.Plural()}"
                , icono: enumNegocio.CircuitoDoc.Icono(), padre: SistemaDocumental, vista: enumVistas.SistemaDocumental.CircuitosDoc, orden: 20);

            //ActualizarMenu(gestor, padre: "", nombre: "Gestión documental", nuevoNombre: "Sistema Documental", descripcion: "Modulo de la gestión documental", icono: "sistema-documental.svg", vista: "", orden: 3);
        }

        private static void MenusDeGestionJuridica(GestorDeMenus gestor)
        {
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: ModuloJuridico, descripcion: "Módulo de la gestión jurídica", icono: "ModuloJuridico.svg", padre: null, vista: "", orden: 20);
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: Configuracion, descripcion: "Configuración jurídica", icono: "configuration-menu.svg", padre: ModuloJuridico, vista: "", orden: 1);

            ConfiguracionDeProcedimientos(gestor);
            ConfiguracionDePleitos(gestor);
            ConfiguracionDeContratos(gestor);

            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: "Procedimientos", descripcion: "Autos o procedimientos judiciales", icono: "Procedimientos.svg", 
                padre: ModuloJuridico, 
                vista: enumVistas.Administracion.Expedientes,
                orden: 20,
                parametros: $"{ltrParametrosEp.Clase}={enumClaseDeExpediente.juridico}");

            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: enumNegocio.Pleito.Plural(), descripcion: enumNegocio.Pleito.Descripcion(), icono: enumNegocio.Pleito.Icono(), padre: ModuloJuridico, vista: enumVistas.Juridico.Pleitos, orden: 30);

            var parametros = $"{ltrParametrosEp.Clase}={enumClaseDeContrato.Venta}";
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: $"{enumNegocio.Contrato.Plural()} de venta",
                descripcion: "Contratos getionados de venta",
                icono: "ContratodDeVenta.svg", padre: ModuloJuridico, vista: enumVistas.Juridico.Contratos, orden: 30, parametros);

            parametros = $"{ltrParametrosEp.Clase}={enumClaseDeContrato.Compra}";
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: $"{enumNegocio.Contrato.Plural()} de compra",
                descripcion: "Contratos getionados de compra",
                icono: enumNegocio.Contrato.Icono(), padre: ModuloJuridico, vista: enumVistas.Juridico.Contratos, orden: 35, parametros);

        }

        private static void ConfiguracionDeProcedimientos(GestorDeMenus gestor)
        {
            var padre = $"{ModuloJuridico}.{Configuracion}";
            
            GestorDeMenus.CrearMenuSiNoExiste(gestor
                , nombre: enumVistas.Juridico.MaestrosDeProcedimientos
                , descripcion: "Inicializa maestros de procedimientos judiciales"
                , icono: "DatosMaestros_Color.svg"
                , padre: padre
                , vista: enumVistas.Juridico.MaestrosDeProcedimientos
                , orden: 3);

        }

        private static void ConfiguracionDeContratos(GestorDeMenus gestor)
        {
            var padre = $"{ModuloJuridico}.{Configuracion}";
            GestorDeMenus.CrearMenuSiNoExiste(gestor, enumNegocio.Contrato.MenuConfiguaracion(), enumMenuDeConfiguaracion.configuracion.Descripcion(enumNegocio.Contrato), enumNegocio.Contrato.Icono(), padre: padre, vista: "", orden: 15);

            padre = $"{ModuloJuridico}.{Configuracion}.{enumNegocio.Contrato.Plural()}";
            var parametros = $"negocio={NegociosDeSe.ToNombre(enumNegocio.Contrato)}";
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: Estados, enumMenuDeConfiguaracion.estado.Descripcion(enumNegocio.Contrato), enumMenuDeConfiguaracion.estado.Icono(), padre, vista: enumVistas.Negocio.Estados, orden: 5, parametros);
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: Transiciones, enumMenuDeConfiguaracion.transicion.Descripcion(enumNegocio.Contrato), enumMenuDeConfiguaracion.transicion.Icono(), padre, vista: enumVistas.Negocio.Transiciones, orden: 10, parametros);

            GestorDeMenus.CrearMenuSiNoExiste(gestor
                , nombre: enumVistas.Juridico.TipoDeContratos
                , enumMenuDeConfiguaracion.tipo.Descripcion(enumNegocio.Contrato)
                , enumMenuDeConfiguaracion.tipo.Icono()
                , padre: padre
                , vista: enumVistas.Juridico.TipoDeContratos
                , orden: 20);

            //GestorDeMenus.CrearMenuSiNoExiste(gestor
            //    , nombre: enumVistas.Juridico.SometerTrabajos
            //    , descripcion: "Somete los trabajos asociados a procesos de contratos"
            //    , icono: "Trabajos_del_sistema.svg"
            //    , padre: padre
            //    , vista: enumVistas.Juridico.SometerTrabajos
            //    , orden: 1);

            //GestorDeMenus.CrearMenuSiNoExiste(gestor
            //    , nombre: enumVistas.Juridico.Etapas
            //    , descripcion: "Resetea las variables que definen las etapas de un contrato"
            //    , icono: "Circuito.svg"
            //    , padre: padre
            //    , vista: enumVistas.Juridico.Etapas
            //    , orden: 2);

            GestorDeMenus.CrearMenuSiNoExiste(gestor
                , nombre: enumVistas.Juridico.MaestrosDeContratos
                , descripcion: "Inicializa maestros"
                , icono: "DatosMaestros_Color.svg"
                , padre: padre
                , vista: enumVistas.Juridico.MaestrosDeContratos
                , orden: 3);
        }

        private static void ConfiguracionDePleitos(GestorDeMenus gestor)
        {
            var padre = $"{ModuloJuridico}.{Configuracion}";
            GestorDeMenus.CrearMenuSiNoExiste(gestor, enumNegocio.Pleito.MenuConfiguaracion(), enumMenuDeConfiguaracion.configuracion.Descripcion(enumNegocio.Pleito), enumNegocio.Pleito.Icono(), padre: padre, vista: "", orden: 5);

            padre = $"{ModuloJuridico}.{Configuracion}.{enumNegocio.Pleito.Plural()}";
            var parametros = $"negocio={NegociosDeSe.ToNombre(enumNegocio.Pleito)}";
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: Estados, descripcion: "estados de un pleito", icono: "Estados.svg", padre, vista: enumVistas.Negocio.Estados, orden: 5, parametros);
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: Transiciones, descripcion: "transiciones de un pleito", icono: "Transiciones.svg", padre, vista: enumVistas.Negocio.Transiciones, orden: 10, parametros);
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: enumVistas.Juridico.TipoDePleitos
                , descripcion: "Configuración de tipos de pleito", icono: "TiposDeRegistro.svg", padre: padre
                , vista: enumVistas.Juridico.TipoDePleitos, orden: 20);
        }

        private static void MenusDeVentas(GestorDeMenus gestor)
        {
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: ModuloDeVentas, descripcion: "Módulo de ventas", icono: "ModuloDeVentas.svg", padre: null, vista: "", orden: 30);
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: Configuracion, descripcion: "Configuración ventas", icono: "configuration-menu.svg", padre: ModuloDeVentas, vista: "", orden: 1);
            MenusDeConfiguracionDePpts(gestor);
            MenusDeConfiguracionDeProceso(gestor, enumNegocio.ParteDeTrabajo, padre: $"{ModuloDeVentas}.{Configuracion}", enumVistas.Ventas.TipoDeParteTr, enumVistas.Ventas.MaestrosDePartesTr, orden: 15);
            MenusDeConfiguracionDeProceso(gestor, enumNegocio.FacturaEmitida, padre: $"{ModuloDeVentas}.{Configuracion}", enumVistas.Ventas.TipoDeFacturaEmt, enumVistas.Ventas.MaestrosDeFacturasEmt, orden: 20);
            MenusDeConfiguracionDeProceso(gestor, enumNegocio.PlanificacionDeVenta, padre: $"{ModuloDeVentas}.{Configuracion}", enumVistas.Ventas.TipoDePlanificacionDeVenta, enumVistas.Ventas.MaestrosDePlanificacionesDeVenta, orden: 10);
            MenusDeConfiguracionDeProceso(gestor, enumNegocio.RemesaFae, padre: $"{ModuloDeVentas}.{Configuracion}", enumVistas.Ventas.TipoDeRemesaFae, enumVistas.Ventas.MaestrosDeRemesasFae, orden: 25);


            GestorDeMenus.CrearMenuSiNoExiste(gestor
                , nombre: enumVistas.Ventas.SometerTrabajos
                , descripcion: "Somete los trabajos asociados a procesos de venta"
                , icono: "Trabajos_del_sistema.svg"
                , padre: $"{ModuloDeVentas}.{Configuracion}"
                , vista: enumVistas.Ventas.SometerTrabajos
                , orden: 999);

            GestorDeMenus.CrearMenuSiNoExiste(gestor
                , nombre: enumVistas.Ventas.Presupuestos
                , descripcion: $"Gestión de los {enumNegocio.Presupuesto.Plural().ToLower()} de la empresa"
                , icono: enumNegocio.Presupuesto.Icono(), padre: ModuloDeVentas, vista: enumVistas.Ventas.Presupuestos, orden: 30);

            GestorDeMenus.CrearMenuSiNoExiste(gestor
                , nombre: enumVistas.Ventas.PlanificacionesDeVenta
                , descripcion: $"Gestión de las {enumNegocio.PlanificacionDeVenta.Plural()} por contrato o sin él"
                , icono: enumNegocio.PlanificacionDeVenta.Icono(), padre: ModuloDeVentas, vista: enumVistas.Ventas.PlanificacionesDeVenta, orden: 30);

            GestorDeMenus.CrearMenuSiNoExiste(gestor
                , nombre: enumVistas.Ventas.PartesTr
                , descripcion: $"Gestión de las {enumNegocio.ParteDeTrabajo.Plural()}"
                , icono: enumNegocio.ParteDeTrabajo.Icono(), padre: ModuloDeVentas, vista: enumVistas.Ventas.PartesTr, orden: 40);

            GestorDeMenus.CrearMenuSiNoExiste(gestor
                , nombre: enumVistas.Ventas.AsignacionesDePtr
                , descripcion: $"Gestión de las {AsignacionesDePartes.Menu}"
                , icono: AsignacionesDePartes.Icono, padre: ModuloDeVentas, vista: enumVistas.Ventas.AsignacionesDePtr, orden: 45);

            GestorDeMenus.CrearMenuSiNoExiste(gestor
                , nombre: enumVistas.Ventas.FacturasEmt
                , descripcion: $"Gestión de las {enumNegocio.FacturaEmitida.Plural()}"
                , icono: enumNegocio.FacturaEmitida.Icono(), padre: ModuloDeVentas, vista: enumVistas.Ventas.FacturasEmt, orden: 50);

            GestorDeMenus.CrearMenuSiNoExiste(gestor
                , nombre: enumVistas.Ventas.RemesasFae
                , descripcion: $"Gestión de las {enumNegocio.RemesaFae.Plural()}"
                , icono: enumNegocio.RemesaFae.Icono(), padre: ModuloDeVentas, vista: enumVistas.Ventas.RemesasFae, orden: 55);

            GestorDeMenus.CrearMenuSiNoExiste(gestor
                , nombre: enumVistas.Ventas.FacturasAeat
                , descripcion: $"Gestión de las facturas registradas en la AEAT con verifactu"
                , icono: "aeat.png", padre: ModuloDeVentas, vista: enumVistas.Ventas.FacturasAeat, orden: 60);
        }

        private static void MenusDeGastos(GestorDeMenus gestor)
        {
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: ModuloDeGastos, descripcion: "Módulo de gastos", icono: "ModuloDeGastos.svg", padre: null, vista: "", orden: 35);
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: Configuracion, descripcion: "Configuración gastos", icono: "configuration-menu.svg", padre: ModuloDeGastos, vista: "", orden: 1);

            MenusDeConfiguracionDeProceso(gestor, enumNegocio.FacturaRecibida, padre: $"{ModuloDeGastos}.{Configuracion}", enumVistas.Gastos.TipoDeFacturaRec, enumVistas.Gastos.MaestrosDeFacturasRec, orden: 10);
            MenusDeConfiguracionDeProceso(gestor, enumNegocio.Pago, padre: $"{ModuloDeGastos}.{Configuracion}", enumVistas.Gastos.TipoDePago, enumVistas.Gastos.MaestrosDePagos, orden: 20);
            MenusDeConfiguracionDeProceso(gestor, enumNegocio.RemesaPag, padre: $"{ModuloDeGastos}.{Configuracion}", enumVistas.Gastos.TipoDeRemesaPag, enumVistas.Gastos.MaestrosDeRemesasPag, orden: 25);

            GestorDeMenus.CrearMenuSiNoExiste(gestor
                , nombre: enumVistas.Gastos.FacturasRec
                , descripcion: $"Gestión de {enumNegocio.FacturaRecibida.Plural()}"
                , icono: enumNegocio.FacturaRecibida.Icono(), padre: ModuloDeGastos, vista: enumVistas.Gastos.FacturasRec, orden: 45);

            GestorDeMenus.CrearMenuSiNoExiste(gestor
                , nombre: enumVistas.Gastos.Pagos
                , descripcion: $"Gestión de {enumNegocio.Pago.Plural()}"
                , icono: enumNegocio.Pago.Icono(), padre: ModuloDeGastos, vista: enumVistas.Gastos.Pagos, orden: 50);

            GestorDeMenus.CrearMenuSiNoExiste(gestor
                , nombre: enumVistas.Gastos.RemesasPag
                , descripcion: $"Gestión de las {enumNegocio.RemesaPag.Plural()}"
                , icono: enumNegocio.RemesaPag.Icono(), padre: ModuloDeGastos, vista: enumVistas.Gastos.RemesasPag, orden: 55);

        }


        private static void MenusDeLogistica(GestorDeMenus gestor)
        {
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: ModuloDeLogistica, descripcion: "Módulo de logística", icono: "ModuloDeLogistica.svg", padre: null, vista: "", orden: 32);
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: Configuracion, descripcion: "Configuración logística", icono: "configuration-menu.svg", padre: ModuloDeLogistica, vista: "", orden: 1);

            MenusDeConfiguracionDeProceso(gestor, enumNegocio.Pedido, padre: $"{ModuloDeLogistica}.{Configuracion}", enumVistas.Logistica.TipoDePedido, enumVistas.Logistica.MaestrosDePedidos, orden: 10);

            GestorDeMenus.CrearMenuSiNoExiste(gestor
                , nombre: enumVistas.Logistica.Pedidos
                , descripcion: $"Gestión de {enumNegocio.Pedido.Plural()}"
                , icono: enumNegocio.Pedido.Icono(), padre: ModuloDeLogistica, vista: enumVistas.Logistica.Pedidos, orden: 45);
        }

        private static void MenusDeGuarderias(GestorDeMenus gestor)
        {
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: ModuloDeGuarderias, descripcion: enumMenu.ModuloDeGuarderias.Descripcion(), icono: enumMenu.ModuloDeGuarderias.Icono(), padre: null, vista: "", orden: 17);

            GestorDeMenus.CrearMenuSiNoExiste(gestor
                , nombre: enumMenu.Aulas.ToString()
                , descripcion: enumMenu.Aulas.Descripcion()
                , icono: enumMenu.Aulas.Icono()
                , padre: ModuloDeGuarderias
                , vista: enumVistas.Guarderias.Aulas, orden: 10);

            GestorDeMenus.CrearMenuSiNoExiste(gestor
                , nombre: enumNegocio.Infante.NombreMenu()
                , descripcion: $"Gestión de {enumNegocio.Infante.Plural()}"
                , icono: enumNegocio.Infante.Icono()
                , padre: ModuloDeGuarderias
                , vista: enumVistas.Guarderias.Infantes, orden: 20);

            GestorDeMenus.CrearMenuSiNoExiste(gestor
                , nombre: enumNegocio.CursoDeGuarderia.NombreMenu()
                , descripcion: $"Gestión de {enumNegocio.CursoDeGuarderia.Plural()}"
                , icono: enumNegocio.CursoDeGuarderia.Icono()
                , padre: ModuloDeGuarderias
                , vista: enumVistas.Guarderias.Cursos, orden: 30);

            var parametros = $"{ltrParametrosEp.Clase}={enumClaseDeContrato.MatriculaDeGuarderia}";
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: $"Matrículas",
                descripcion: "Matrículas de alumnos",
                icono: "matrículas.svg", padre: ModuloDeGuarderias, vista: enumVistas.Juridico.Contratos, orden: 40, parametros);
        }

        private static void MenusDeConfiguracionDePpts(GestorDeMenus gestor)
        {
            var padre = $"{ModuloDeVentas}.{Configuracion}";
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: $"{enumNegocio.Presupuesto}", descripcion: "Configuración de presupuestos", icono: "presupuesto.svg", padre: padre, vista: "", orden: 1);

            padre = $"{ModuloDeVentas}.{Configuracion}.{enumNegocio.Presupuesto}";
            var parametros = $"negocio={NegociosDeSe.ToNombre(enumNegocio.Presupuesto)}";
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: Estados, descripcion: "estados de un presupuesto", icono: "Estados.svg", padre, vista: enumVistas.Negocio.Estados, orden: 5, parametros);
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: Transiciones, descripcion: "transiciones de un presupuesto", icono: "Transiciones.svg", padre, vista: enumVistas.Negocio.Transiciones, orden: 10, parametros);
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: enumVistas.Ventas.TipoDePresupuesto
                , descripcion: "Configuración de tipos de presupuesto", icono: "TiposDeRegistro.svg", padre: padre
                , vista: enumVistas.Ventas.TipoDePresupuesto, orden: 20);
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: enumVistas.Ventas.MaestrosDePpts, descripcion: "Inicializa maestros", icono: "DatosMaestros_Color.svg", padre: padre, vista: enumVistas.Ventas.MaestrosDePpts, orden: 100);
        }

        private static void MenusDeConfiguracionDeProceso(GestorDeMenus gestor, enumNegocio negocio, string padre, string vistaDeTipo, string vistaDeMt, int orden)
        {
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: $"{negocio.MenuConfiguaracion()}", descripcion: $"Configuración de {negocio.Descripcion()}", icono: negocio.Icono(), padre: padre, vista: "", orden);

            padre = $"{padre}.{negocio.MenuConfiguaracion()}";
            var parametros = $"negocio={NegociosDeSe.ToNombre(negocio)}";
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: Estados, descripcion: $"estados de un {negocio.Singular()}", icono: "Estados.svg", padre, vista: enumVistas.Negocio.Estados, orden: 5, parametros);
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: Transiciones, descripcion: $"transiciones de un {negocio.Singular()}", icono: "Transiciones.svg", padre, vista: enumVistas.Negocio.Transiciones, orden: 10, parametros);
            GestorDeMenus.CrearMenuSiNoExiste(gestor
                , nombre: vistaDeTipo
                , descripcion: $"Configuración de tipos de {negocio.Singular()}"
                , icono: "TiposDeRegistro.svg", padre: padre
                , vista: vistaDeTipo
                , orden: 20);
            GestorDeMenus.CrearMenuSiNoExiste(gestor
                , nombre: vistaDeMt
                , descripcion: "Inicializa maestros"
                , icono: "DatosMaestros_Color.svg"
                , padre: padre
                , vista: vistaDeMt
                , orden: 100);
        }

        private static void MenusDeGestionDeMts(GestorDeMenus gestor)
        {
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: ModuloMts, descripcion: "Módulo de Maestros ténicos", icono: "MaestrosTecnicos.svg", padre: null, vista: "", orden: 6);
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: Configuracion, descripcion: "Configuración Mts", icono: "configuration-menu.svg", padre: ModuloMts, vista: "", orden: 1);

            var padre = $"{ModuloMts}.{Configuracion}";
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: Unidades, descripcion: "Unidades de medida", icono: "unidades.svg", padre, vista: enumVistas.Mts.Unidades, orden: 1);
            var menuNaturaleza = gestor.Contexto.SeleccionarPorNombre<MenuDtm>(GrupsoDeNaturaleza, errorSiNoHay: false);
            if (menuNaturaleza != null)
            {
                menuNaturaleza.Nombre = NaturalezasContables;
                menuNaturaleza.Descripcion = "Naturalezas contables";
                menuNaturaleza.Modificar(gestor.Contexto);
            }
            else
                GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: NaturalezasContables, descripcion: "Naturalezas contables", icono: "naturaleza.svg", padre, vista: enumVistas.Mts.Naturalezas, orden: 5);


            padre = $"{ModuloMts}";
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: $"{enumNegocio.Unitario}", descripcion: "Unitarios", icono: "Unitario.svg", padre: padre, vista: enumVistas.Mts.Unitarios, orden: 20);
        }

        private static void MenusDeRRHH(GestorDeMenus gestor)
        {
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: ModuloRRHH, descripcion: "Módulo de recursos humanos", icono: "rrhh.svg", padre: null, vista: "", orden: 16);

            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: enumVistas.Terceros.Trabajadores, descripcion: "Trabajadores del sistema", icono: enumNegocio.Trabajador.Icono(), padre: ModuloRRHH, vista: enumVistas.Terceros.Trabajadores, orden: 20, buscarPorPadre: false);
            GestorDeMenus.CrearMenuSiNoExiste(gestor
                , nombre: enumVistas.RecusosHumanos.Fichadas
                , descripcion: $"Gestión del registro de fichadas del personal"
                , icono: enumVistas.SistemaDocumental.Fichadas + ".png", padre: ModuloRRHH, vista: enumVistas.SistemaDocumental.Fichadas, orden: 40);
        }

        private static void MenusDeGestionContable(GestorDeMenus gestor)
        {
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: ModuloContable, descripcion: "Módulo de Contabilidad", icono: "contabilidad.svg", padre: null, vista: "", orden: 10);
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: Configuracion, descripcion: "Configuración contable", icono: "configuration-menu.svg", padre: ModuloContable, vista: "", orden: 1);

            var padre = $"{ModuloContable}.{Configuracion}";
            MenusDeConfiguracionDeProceso(gestor, enumNegocio.Preasiento, padre: $"{ModuloContable}.{Configuracion}", enumVistas.Contabilidad.TipoDePreasiento, enumVistas.Contabilidad.MaestrosDePreasientos, orden: 10);
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: Cuentas, descripcion: "Cuentas contables", icono: "cuenta-contable.svg", padre, vista: enumVistas.Contabilidad.Cuentas, orden: 10);
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: IvaSoportado, descripcion: "Iva soportado", icono: "iva-soportado.svg", padre, vista: enumVistas.Contabilidad.IvasSoportado, orden: 15);
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: IvaRepercutido, descripcion: "Iva repercutido", icono: "iva-repercutido.svg", padre, vista: enumVistas.Contabilidad.IvasRepercutido, orden: 20);
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: Irpf, descripcion: "Irpf", icono: "irpf.svg", padre, vista: enumVistas.Contabilidad.Irpfs, orden: 15);

            GestorDeMenus.CrearMenuSiNoExiste(gestor
                , nombre: enumVistas.Contabilidad.Preasientos
                , descripcion: $"Gestión de {enumNegocio.Preasiento.Plural()}"
                , icono: enumNegocio.Preasiento.Icono(), padre: ModuloContable, vista: enumVistas.Contabilidad.Preasientos, orden: 45);
            GestorDeMenus.CrearMenuSiNoExiste(gestor
                , nombre: enumVistas.SistemaDocumental.EstimacionesDirectas
                , descripcion: $"Gestión de {enumVistas.SistemaDocumental.EstimacionesDirectas}"
                , icono: enumVistas.SistemaDocumental.EstimacionesDirectas + ".png", padre: ModuloContable, vista: enumVistas.SistemaDocumental.EstimacionesDirectas, orden: 50);
            GestorDeMenus.CrearMenuSiNoExiste(gestor
                , nombre: enumVistas.SistemaDocumental.LotesContables
                , descripcion: $"Gestión de {enumVistas.SistemaDocumental.LotesContables}"
                , icono: enumVistas.SistemaDocumental.LoteContable + ".png", padre: ModuloContable, vista: enumVistas.SistemaDocumental.LotesContables, orden: 50);

        }


        private static void MenusDeGestionAdministrativa(GestorDeMenus gestor)
        {
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: GestionAdministrativa, descripcion: "Módulo de la gestión administrativa", icono: "GestionAdministrativa2.svg", padre: null, vista: "", orden: 15);
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: Configuracion, descripcion: "Configuración administrativa", icono: "configuration-menu.svg", padre: GestionAdministrativa, vista: "", orden: 1);

            var padre = $"{GestionAdministrativa}.{Configuracion}";
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: $"{enumNegocio.Registro}", descripcion: "Configuración del registro", icono: "RegistroEs.svg", padre: padre, vista: "", orden: 1);

            padre = $"{GestionAdministrativa}.{Configuracion}.{enumNegocio.Registro}";
            var parametros = $"negocio={NegociosDeSe.ToNombre(enumNegocio.Registro)}";
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: Estados, descripcion: "estados del registro", icono: "Estados.svg", padre, vista: enumVistas.Negocio.Estados, orden: 5, parametros);
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: Transiciones, descripcion: "transiciones del registro", icono: "Transiciones.svg", padre, vista: enumVistas.Negocio.Transiciones, orden: 10, parametros);
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: enumVistas.Administracion.TipoDeRegistros
                , descripcion: "Configuración de tipos de registro de E/S", icono: "TiposDeRegistro.svg", padre: padre
                , vista: enumVistas.Administracion.TipoDeRegistros, orden: 20);

            padre = $"{GestionAdministrativa}.{Configuracion}";
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: $"{enumNegocio.Tarea}", descripcion: "Configuración de tareas", icono: "Tareas.svg", padre: padre, vista: "", orden: 5);

            padre = $"{GestionAdministrativa}.{Configuracion}.{enumNegocio.Tarea}";
            parametros = $"negocio={NegociosDeSe.ToNombre(enumNegocio.Tarea)}";
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: Estados, descripcion: "estados de la tarea", icono: "Estados.svg", padre, vista: enumVistas.Negocio.Estados, orden: 5, parametros);
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: Transiciones, descripcion: "transiciones de la tarea", icono: "Transiciones.svg", padre, vista: enumVistas.Negocio.Transiciones, orden: 10, parametros);
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: enumVistas.Administracion.TipoDeTareas
                , descripcion: "Configuración de tipos de tareas", icono: "TiposDeRegistro.svg", padre: padre
                , vista: enumVistas.Administracion.TipoDeTareas, orden: 20);
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: enumVistas.Administracion.MaestrosDeTareas, descripcion: "Inicializa maestros", icono: "DatosMaestros_Color.svg", padre: padre, vista: enumVistas.Administracion.MaestrosDeTareas, orden: 100);



            padre = $"{GestionAdministrativa}.{Configuracion}";
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: $"{enumNegocio.Expediente}", descripcion: "Configuración de expedientes", icono: "Expedientes.svg", padre: padre, vista: "", orden: 10);

            padre = $"{GestionAdministrativa}.{Configuracion}.{enumNegocio.Expediente}";
            parametros = $"negocio={NegociosDeSe.ToNombre(enumNegocio.Expediente)}";
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: Estados, descripcion: "estados de un expediente", icono: "Estados.svg", padre, vista: enumVistas.Negocio.Estados, orden: 5, parametros);
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: Transiciones, descripcion: "transiciones de un expediente", icono: "Transiciones.svg", padre, vista: enumVistas.Negocio.Transiciones, orden: 10, parametros);
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: enumVistas.Administracion.TipoDeExpedientes
                , descripcion: "Configuración de tipos de expedientes", icono: "TiposDeRegistro.svg", padre: padre
                , vista: enumVistas.Administracion.TipoDeExpedientes, orden: 20);
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: enumVistas.Administracion.MaestrosDeExpedientes, descripcion: "Inicializa maestros", icono: "DatosMaestros_Color.svg", padre: padre, vista: enumVistas.Administracion.MaestrosDeExpedientes, orden: 100);



            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: enumVistas.Administracion.RegistrosEs
                , descripcion: "Registro de Entrada y salida"
                , icono: "RegistroEs.svg", padre: GestionAdministrativa, vista: enumVistas.Administracion.RegistrosEs, orden: 10);

            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: enumVistas.Administracion.Tareas
                , descripcion: "Tareas"
                , icono: "Tareas.svg", padre: GestionAdministrativa, vista: enumVistas.Administracion.Tareas, orden: 20);

            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: enumVistas.Administracion.Expedientes
                , descripcion: "Expedientes"
                , icono: "Expedientes.svg", padre: GestionAdministrativa, vista: enumVistas.Administracion.Expedientes, orden: 30);


            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: "Actividades", descripcion: "Actividades formativas", icono: "Actividades.svg",
                padre: GestionAdministrativa,
                vista: enumVistas.Administracion.Actividades,
                orden: 40,
                parametros: string.Empty);


        }

        private static void MenusDelCallejero(GestorDeMenus gestor)
        {
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: Maestros, descripcion: "Información de datos maestros", icono: "home-solid.svg", padre: null, vista: "", orden: 5);
            string maestrosCallejero = $"{Maestros}.Callejero";
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: "Callejero", descripcion: "Gestión del callejero", icono: "callejero.svg", padre: Maestros, vista: "", orden: 1);
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: "Tipos de vía", descripcion: "Mantenimiento de tipos de vía", icono: "TipoDeVia.svg", padre: maestrosCallejero, vista: enumVistas.Callejero.TipoDeVia, orden: 1);
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: "Paises", descripcion: "Gestión de paises", icono: "paises.svg", padre: maestrosCallejero, vista: enumVistas.Callejero.Paises, orden: 10);
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: "Provincias", descripcion: "Mantenimiento de provincias", icono: "provincias.svg", padre: maestrosCallejero, vista: enumVistas.Callejero.Provincias, orden: 20);
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: "Municipios", descripcion: "Mantenimiento de municipios", icono: "municipio2.svg", padre: maestrosCallejero, vista: enumVistas.Callejero.Municipios, orden: 30);
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: "Calles", descripcion: "Mantenimiento de calles", icono: "callejero.svg", padre: maestrosCallejero, vista: enumVistas.Callejero.Calles, orden: 40);
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: "Codigos postales", descripcion: "Mantenimiento de Cps", icono: "codigos-postales.svg", padre: maestrosCallejero, vista: enumVistas.Callejero.CodigosPostales, orden: 99);
            GestorDeMenus.CrearMenuSiNoExiste(gestor, nombre: "Importar", descripcion: "Importa entidades del callejero", icono: "importar.svg", padre: maestrosCallejero, vista: enumVistas.Callejero.ImportacionCallejero, orden: 999);
        }

    }

}
