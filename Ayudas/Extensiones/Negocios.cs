using System.ComponentModel;

namespace Utilidades
{

    public static class enumLiteralesDeNegocio
    {
        public static string Plural(this enumNegocio negocio, bool enMinusculas = false)
        {
            switch (negocio)
            {
                case enumNegocio.VistaMvc: return enMinusculas ? "vistas" : "Vistas";
                case enumNegocio.Menu: return enMinusculas ? "menús" : "Menús";
                case enumNegocio.Accion: return enMinusculas ? "acciones" : "Acciones";
                case enumNegocio.Tarea: return enMinusculas ? "tareas" : "Tareas";
                case enumNegocio.Archivador: return enMinusculas ? "archivadores" : "Archivadores";
                case enumNegocio.Registro: return enMinusculas ? "registros de E/S" : "Registros de E/S";
                case enumNegocio.Expediente: return enMinusculas ? "expedientes" : "Expedientes";
                case enumNegocio.Pleito: return enMinusculas ? "pleitos" : "Pleitos";
                case enumNegocio.Contrato: return enMinusculas ? "contratos" : "Contratos";
                case enumNegocio.Unitario: return enMinusculas ? "unitarios" : "Unitarios";
                case enumNegocio.Presupuesto: return enMinusculas ? "presupuestos" : "Presupuestos";
                case enumNegocio.Pais: return enMinusculas ? "paises" : "Paises";
                case enumNegocio.Provincia: return enMinusculas ? "provincias" : "Provincias";
                case enumNegocio.Municipio: return enMinusculas ? "municipios" : "Municipios";
                case enumNegocio.Calle: return enMinusculas ? "calles" : "Calles";
                case enumNegocio.Barrio: return enMinusculas ? "barrios" : "Barrios";
                case enumNegocio.TipoDeVia: return enMinusculas ? "tipos de vía" : "Tipos de vía";
                case enumNegocio.Zona: return enMinusculas ? "zonas" : "Zonas";
                case enumNegocio.LoteDeUnContrato: return enMinusculas ? "lotes de un contrato" : "Lotes de un contrato";
                case enumNegocio.PlanificadorDeVenta: return enMinusculas ? "planificador de venta" : "Planificadores de ventas";
                case enumNegocio.EventoDeAgenda: return enMinusculas ? "eventos de un calendario" : "Eventos de un calendario";
                case enumNegocio.ParteDeTrabajo: return enMinusculas ? "partes de trabajo" : "Partes de trabajo";
                case enumNegocio.FacturaEmitida: return enMinusculas ? "facturas emitida" : "Facturas emitida";
                case enumNegocio.PlanificacionDeVenta: return enMinusculas ? "planificaciones de venta" : "Planificaciones de venta";
                case enumNegocio.PlanificacionDeCompra: return enMinusculas ? "planificaciones de compra" : "Planificaciones de compra";
                case enumNegocio.RemesaFae: return enMinusculas ? "remesas de facturas" : "Remesas de facturas";
                case enumNegocio.CircuitoDoc: return enMinusculas ? "circuitos documentales" : "Circuitos documentales";
                case enumNegocio.Pago: return enMinusculas ? "pagos" : "Pagos";
                case enumNegocio.FacturaRecibida: return enMinusculas ? "facturas recibida" : "Facturas recibidas";
                case enumNegocio.Pedido: return enMinusculas ? "pedidos" : "Pedidos";
                case enumNegocio.Preasiento: return enMinusculas ? "preasientos" : "Preasientos";
                case enumNegocio.RemesaPag: return enMinusculas ? "remesas de pagos" : "Remesas de pagos";
                case enumNegocio.CentroGestor:return enMinusculas ? "centros gestores" : "Centros gestores";
                case enumNegocio.Infante: return enMinusculas ? "niños de la guardería" : "Niños de la guarderia";
                case enumNegocio.CursoDeGuarderia: return enMinusculas ? "cursos" : "Cursos";
                case enumNegocio.Proveedor: return enMinusculas ? "proveedores" : "Proveedores";
                case enumNegocio.Procurador: return enMinusculas ? "procuradores" : "Procuradores";
                case enumNegocio.Interlocutor: return enMinusculas ? "interlocutores" : "Interlocutores"; 
                case enumNegocio.Sociedad: return enMinusculas ? "sociedades" : "Sociedades";
                case enumNegocio.Trabajador: return enMinusculas ? "trabajadores" : "Trabajadores";
                case enumNegocio.Archivos: return enMinusculas ? "archivos" : "Archivos";
            }
            return enMinusculas ? $"{negocio.Descripcion().ToLower()}s" : $"{negocio.Descripcion()}s";
        }
        public static string ConArticulo(this enumNegocio negocio, bool enMinusculas = true)
        {
            switch (negocio)
            {
                case enumNegocio.No_Definido: return $"";
                case enumNegocio.VistaMvc: return $"de la {negocio.Singular(enMinusculas)}";
                case enumNegocio.Menu: return $"de la {negocio.Singular(enMinusculas)}";
                case enumNegocio.Accion: return $"de la {negocio.Singular(enMinusculas)}";
                case enumNegocio.Tarea: return $"de la {negocio.Singular(enMinusculas)}";
                case enumNegocio.Archivador: return $"del {negocio.Singular(enMinusculas)}";
                case enumNegocio.Registro: return $"del {negocio.Singular(enMinusculas)}";
                case enumNegocio.Expediente: return $"del {negocio.Singular(enMinusculas)}";
                case enumNegocio.Pleito: return $"del {negocio.Singular(enMinusculas)}";
                case enumNegocio.Contrato: return $"del {negocio.Singular(enMinusculas)}";
                case enumNegocio.Unitario: return $"del {negocio.Singular(enMinusculas)}";
                case enumNegocio.Presupuesto: return $"del {negocio.Singular(enMinusculas)}";
                case enumNegocio.Pais: return $"del {negocio.Singular(enMinusculas)}";
                case enumNegocio.Provincia: return $"de la {negocio.Singular(enMinusculas)}"; 
                case enumNegocio.Municipio: return $"del {negocio.Singular(enMinusculas)}";
                case enumNegocio.Calle: return $"de la {negocio.Singular(enMinusculas)}";
                case enumNegocio.Barrio: return $"del {negocio.Singular(enMinusculas)}";
                case enumNegocio.TipoDeVia: return $"del {negocio.Singular(enMinusculas)}";
                case enumNegocio.Zona: return $"de la {negocio.Singular(enMinusculas)}";
                case enumNegocio.LoteDeUnContrato: return $"del {negocio.Singular(enMinusculas)}";
                case enumNegocio.PlanificadorDeVenta: return $"del {negocio.Singular(enMinusculas)}";
                case enumNegocio.EventoDeAgenda: return $"del {negocio.Singular(enMinusculas)}";
                case enumNegocio.ParteDeTrabajo: return $"del {negocio.Singular(enMinusculas)}";
                case enumNegocio.FacturaEmitida: return $"de la {negocio.Singular(enMinusculas)}";
                case enumNegocio.PlanificacionDeVenta: return $"de la {negocio.Singular(enMinusculas)}";
                case enumNegocio.PlanificacionDeCompra: return $"de la {negocio.Singular(enMinusculas)}";
                case enumNegocio.RemesaFae: return $"de la {negocio.Singular(enMinusculas)}";
                case enumNegocio.CircuitoDoc: return $"del {negocio.Singular(enMinusculas)}";
                case enumNegocio.Pago: return $"del {negocio.Singular(enMinusculas)}";
                case enumNegocio.FacturaRecibida: return $"de la {negocio.Singular(enMinusculas)}";
                case enumNegocio.Pedido: return $"del {negocio.Singular(enMinusculas)}";
                case enumNegocio.Preasiento: return $"del {negocio.Singular(enMinusculas)}";
                case enumNegocio.RemesaPag: return $"de la {negocio.Singular(enMinusculas)}";
                case enumNegocio.Sociedad: return $"de la {negocio.Singular(enMinusculas)}";
                case enumNegocio.Infante: return $"del {negocio.Singular(enMinusculas)}";
                case enumNegocio.CursoDeGuarderia: return $"del {negocio.Singular(enMinusculas)}";
                case enumNegocio.Procurador: return $"del {negocio.Singular(enMinusculas)}";
                case enumNegocio.Proveedor: return $"del {negocio.Singular(enMinusculas)}";
                case enumNegocio.Archivos: return $"del {negocio.Singular(enMinusculas)}";
                case enumNegocio.Cobro: return $"del {negocio.Singular(enMinusculas)}";
            }
            return negocio.Singular(true).EndsWith("a") ? $"de la {negocio.Singular(enMinusculas)}": $"del {negocio.Singular(enMinusculas)}";
        }
        public static string IniciarFrase(this enumNegocio negocio, bool enMinusculas = false)
        {
            return negocio.Singular(true).EndsWith("a") ? $"La {negocio.Singular(enMinusculas)}" : $"El {negocio.Singular(enMinusculas)}";
        }

        public static string Singular(this enumNegocio negocio, bool enMinuscula = false)
        {
            switch (negocio)
            {
                case enumNegocio.Archivos: return enMinuscula ? "archivo" : "Archivo";
                case enumNegocio.Cobro: return enMinuscula ? "cobro" : "Cobro";
                default:
                    return enMinuscula ? negocio.Descripcion().ToLower(): negocio.Descripcion();
            }
        }

        public static string Controlador(this enumNegocio negocio)
        {
            switch (negocio)
            {
                case enumNegocio.Archivador: return enumControladoresSistemaDocumental.Archivadores.ToString();
                case enumNegocio.CircuitoDoc: return enumControladoresSistemaDocumental.CircuitosDoc.ToString();
                case enumNegocio.Registro: return enumControladoresAdministrativos.RegistrosEs.ToString();
                case enumNegocio.Tarea: return enumControladoresAdministrativos.Tareas.ToString();
                case enumNegocio.Expediente: return enumControladoresAdministrativos.Expedientes.ToString();
                case enumNegocio.Contrato: return enumControladoresJuridicos.Contratos.ToString();
                case enumNegocio.Pleito: return enumControladoresJuridicos.Pleitos.ToString();
                case enumNegocio.PlanificacionDeVenta: return enumControladoresVentas.PlanificacionesDeVenta.ToString();
                case enumNegocio.Presupuesto: return enumControladoresVentas.Presupuestos.ToString();
                case enumNegocio.ParteDeTrabajo: return enumControladoresVentas.PartesTr.ToString();
                case enumNegocio.FacturaEmitida: return enumControladoresVentas.FacturasEmt.ToString();
                case enumNegocio.RemesaFae: return enumControladoresVentas.RemesasFae.ToString();
                case enumNegocio.PlanificacionDeCompra: return enumControladoresLogistica.PlanificacionesDeCompra.ToString();
                case enumNegocio.Cliente: return enumControladoresTerceros.Clientes.ToString();
                case enumNegocio.Proveedor: return enumControladoresTerceros.Proveedores.ToString();
                case enumNegocio.Interlocutor: return enumControladoresTerceros.Interlocutores.ToString();
                case enumNegocio.Pago: return enumControladoresGastos.Pagos.ToString();
                case enumNegocio.RemesaPag: return enumControladoresGastos.RemesasPag.ToString();
                case enumNegocio.FacturaRecibida: return enumControladoresGastos.FacturasRec.ToString();
                case enumNegocio.Pedido: return enumControladoresLogistica.Pedidos.ToString();
                case enumNegocio.Preasiento: return enumControladoresContables.Preasientos.ToString();
                case enumNegocio.Infante: return enumControladoresGuarderias.Infantes.ToString();
                case enumNegocio.CursoDeGuarderia: return enumControladoresGuarderias.CursosDeGuarderia.ToString();
            }

            throw new System.Exception($"Ha de definir el controlador asociado al negocio '{negocio.Singular(true)}'");
        }

        public static string Icono(this enumNegocio negocio)
        {
            switch (negocio)
            {
                default:
                    return $"{negocio}.svg";
            }
        }

        public static string Configuracion(this enumNegocio negocio)
        {
            switch (negocio)
            {
                default:
                    return $"Configuración de {negocio.Plural().ToLower()}";
            }
        }

        public static string Descripcion(this enumMenuDeConfiguaracion menu, enumNegocio negocio) => menu.Descripcion() + " " + negocio.Plural().ToLower();

        public static string MenuConfiguaracion(this enumNegocio negocio) => negocio.Plural();

        public static string NombreMenu(this enumNegocio negocio)
        {
            switch (negocio)
            {
                case enumNegocio.Infante: return "Niños";
                case enumNegocio.CursoDeGuarderia: return "Cursos";
                default:
                    return negocio.Plural();
            }
        }

        public static string Icono(this enumMenuDeConfiguaracion menu)
        {
            switch (menu)
            {
                case enumMenuDeConfiguaracion.estado: return "Estados.svg";
                case enumMenuDeConfiguaracion.transicion: return "Transiciones.svg";
                case enumMenuDeConfiguaracion.tipo: return "TiposDeRegistro.svg";

                default:
                    return $"{menu}.svg";
            }
        }

        public static string Icono(this enumMenu menu)
        {
            switch (menu)
            {
                case enumMenu.ModuloDeGuarderias: return "ModuloDeGuarderias.svg";
                case enumMenu.Aulas: return "AulaDeGuarderia.png";

                default:
                    return $"{menu}.svg";
            }
        }


    }

    public enum enumNegocio
    {
        No_Definido,
        Usuario,
        [Description("Vista del sistema")]
        VistaMvc,
        Agenda,
        [Description("Evento de calendario")]
        EventoDeAgenda,
        Variable,
        [Description("Acción")]
        Accion,
        [Description("Menú")]
        Menu,
        Puesto,
        PermisosDeUnUsuario,
        Permiso,
        Negocio,
        Rol,
        ClasePermiso,
        PermisosDeUnPuesto,
        PermisosDeUnRol,
        PuestosDeUnRol,
        PuestosDeUnUsuario,
        RolesDeUnPermiso,
        RolesDeUnPuesto,
        TipoPermiso,
        Archivos,
        Pais,
        Provincia,
        Municipio,
        [Description("Tipo de vía")]
        TipoDeVia,
        Calle,
        Barrio,
        Zona,
        Correo,
        Sociedad,
        CentroGestor,
        Archivador,
        [Description("Circuito documental")]
        CircuitoDoc,
        Carpeta,
        Persona,
        Contacto,
        Interlocutor,
        Registro,
        Tarea,
        Expediente,
        Procurador,
        Abogado,
        Juzgado,
        Banco,
        [Description("Pleito judicial")]
        Pleito,
        Certificado,
        Unitario,
        Proveedor,
        Cliente,
        Trabajador,
        Presupuesto,
        [Description("Parte de trabajo")]
        ParteDeTrabajo,
        [Description("Contrato con empresas")]
        Contrato,
        [Description("Lote de un contrato")]
        LoteDeUnContrato,
        [Description("Planificador de venta")]
        PlanificadorDeVenta,
        [Description("Factura emitida")]
        FacturaEmitida,
        Cobro,
        [Description("Planificacion de venta")]
        PlanificacionDeVenta,
        [Description("Planificacion de compra")]
        PlanificacionDeCompra,
        [Description("Remesa de facturas")]
        RemesaFae,
        [Description("Gasto")]
        Gasto,
        [Description("Pago")]
        Pago,
        [Description("Remesa de pagos")]
        RemesaPag,
        [Description("Factura recibida")]
        FacturaRecibida,
        [Description("Curso de guardería")]
        CursoDeGuarderia,
        [Description("Niño de la guardería")]
        Infante,
        [Description("Pedido")]
        Pedido,
        [Description("Preasiento")]
        Preasiento
    }

    public enum enumMenuDeConfiguaracion
    {
        [Description("Configuración de ")]
        configuracion,
        [Description("Estado de un")]
        estado,
        [Description("Estado de una")]
        estada,
        [Description("Transiciones de un")]
        transicion,
        [Description("Transiciones de una")]
        transiciona,
        [Description("Configuración de tipos de")]
        tipo
    }

    public enum enumMenu
    {
        [Description("Módulo de guarderías")]
        ModuloDeGuarderias,
        [Description("Gestión de Aulas")]
        Aulas
    }
}
