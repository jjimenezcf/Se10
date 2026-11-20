using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServicioDeDatos.Contabilidad
{
    public static class ltrCuenta
    {
        public static readonly string Cuentas = nameof(Cuentas);
        public static readonly string Cuenta = nameof(Cuenta);
        public static readonly string ClienteCodigo = "430000";
        public static readonly string ProveedorCodigo = "400000";
        public static readonly string ConsultoriaCodigo = "623000";
        public static readonly string SueldosCodigo = "640000";
        public static readonly string IvaRepercutido = "477000";
        public static readonly string IvaSoportado = "472000";
        public static readonly string IrpfPersonaFisica = "475100";
        public static readonly string IngresosConsultoriaCodigo = "705000";
        public static readonly string Caja = "570000";
        public static readonly string Banco = "572000";
        public static readonly string Tarjeta = "520100";


        public static readonly string ClienteDescripcion = "Cuenta de clientes";
        public static readonly string ProveedorDescripcion = "Cuenta de proveedores";
        public static readonly string AlbaranDescripcion = "Cuenta de albaranes";
        public static readonly string ExistenciasDescripcion = "Cuenta de existencias del almacén";
        public static readonly string VariacionDeExistenciasDescripcion = "Cuenta de variación de existencias";
        public static readonly string PendienteDeAplicacionDescripcion = "Cuenta de partidas pendientes de aplicación";
        public static readonly string SueldosDescripcion = "Cuenta de gastos de sueldos y salarios";
        public static readonly string ServiciosProfesionales = "Cuenta de servicios profesionales";
        public static readonly string PrestacionDeServicios = "Cuenta de prestación de servicios";
        public static readonly string IvaRepercutidodescripcion = "Cuenta de Iva repercutido";
        public static readonly string IvaSoportadodescripcion = "Cuenta de Iva soportado";
        public static readonly string IrpfPersonaFisicadescripcion = "Cuenta de IRPF persona física";

        //------------------------------------------------------------------------------------------- almacenes y obras (ver ExtensorDeCuentas)
        // grupos y subgrupos del PGC de las cuentas de un almacén
        public const string GrupoDeExistenciasDeMercaderias = "30";
        public const string GrupoDeExistenciasDeMateriasPrimas = "31";
        public const string GrupoDeOtrosAprovisionamientos = "32";
        public const string GrupoDeComprasYGastos = "60";
        public const string SubgrupoDeComprasDeMercaderias = "600";
        public const string SubgrupoDeComprasDeMateriasPrimas = "601";
        public const string SubgrupoDeComprasDeOtrosAprovisionamientos = "602";
        public const string SubgrupoDeDevolucionesDeCompras = "608";
        public const string SubgrupoDeVariacionDeMercaderias = "610";
        public const string SubgrupoDeVariacionDeMateriasPrimas = "611";
        public const string SubgrupoDeVariacionDeOtrosAprovisionamientos = "612";

        // cuentas que se crean al inicializar los maestros (ver InzContable)
        public const string MercaderiasCodigo = "300000";
        public const string MercaderiasNombre = "Mercaderías";
        public const string MateriasPrimasCodigo = "310000";
        public const string MateriasPrimasNombre = "Materias primas";
        public const string OtrosAprovisionamientosCodigo = "320000";
        public const string OtrosAprovisionamientosNombre = "Otros aprovisionamientos";

        public const string ComprasDeMercaderiasCodigo = "600000";
        public const string ComprasDeMercaderiasNombre = "Compras de mercaderías";
        public const string ComprasDeMateriasPrimasCodigo = "601000";
        public const string ComprasDeMateriasPrimasNombre = "Compras de materias primas";
        public const string ComprasDeOtrosAprovisionamientosCodigo = "602000";
        public const string ComprasDeOtrosAprovisionamientosNombre = "Compras de otros aprovisionamientos";

        public const string DevolucionesDeMercaderiasCodigo = "608000";
        public const string DevolucionesDeMercaderiasNombre = "Devoluciones de compras de mercaderías";
        public const string DevolucionesDeMateriasPrimasCodigo = "608100";
        public const string DevolucionesDeMateriasPrimasNombre = "Devoluciones de compras de materias primas";
        public const string DevolucionesDeOtrosAprovisionamientosCodigo = "608200";
        public const string DevolucionesDeOtrosAprovisionamientosNombre = "Devoluciones de compras de otros aprovisionamientos";

        public const string VariacionDeMercaderiasCodigo = "610000";
        public const string VariacionDeMercaderiasNombre = "Variación de existencias de mercaderías";
        public const string VariacionDeMateriasPrimasCodigo = "611000";
        public const string VariacionDeMateriasPrimasNombre = "Variación de existencias de materias primas";
        public const string VariacionDeOtrosAprovisionamientosCodigo = "612000";
        public const string VariacionDeOtrosAprovisionamientosNombre = "Variación de existencias de otros aprovisionamientos";

        // el código de estas dos es el de su variable (VariablesDeCuentas.Albaranes y PendientesDeAplicacion)
        public const string AlbaranesNombre = "Proveedores, facturas pendientes de recibir";
        public const string PendientesDeAplicacionNombre = "Partidas pendientes de aplicación";
    }


    // Restrictor fijo de las listas dinámicas de cuentas ('FiltroPorUso;<uso>'): sólo se muestran las cuentas utilizables para ese uso
    // (ver FiltrosDeCuentasContables.FiltrarPorUso)
    public static class ltrUsoDeCuenta
    {
        public const string FiltroPorUso = nameof(FiltroPorUso);
        public const string Existencias = nameof(Existencias);
        public const string Variacion = nameof(Variacion);
        public const string Compras = nameof(Compras);
        public const string Devoluciones = nameof(Devoluciones);
        public const string GastoDeMateriales = nameof(GastoDeMateriales);

        // restrictores fijos de cada uso (constantes para poder usarlos en los atributos de los Dtos)
        public const string RestrictorDeExistencias = FiltroPorUso + ";" + Existencias;
        public const string RestrictorDeVariacion = FiltroPorUso + ";" + Variacion;
        public const string RestrictorDeCompras = FiltroPorUso + ";" + Compras;
        public const string RestrictorDeDevoluciones = FiltroPorUso + ";" + Devoluciones;
        public const string RestrictorDeGastoDeMateriales = FiltroPorUso + ";" + GastoDeMateriales;
    }

    public static class VariablesDeCuentas
    {
        public static string Sueldos => CacheDeVariable.ObtenerVariable(Variable.CTA_Cuenta_De_Sueldos, Descripciones.CTA_Cuenta_De_Sueldos, "640000");
        public static string Proveedores => CacheDeVariable.ObtenerVariable(Variable.CTA_Cuenta_De_Proveedor, Descripciones.CTA_Cuenta_De_Proveedor, "400000");
        public static string Albaranes => CacheDeVariable.ObtenerVariable(Variable.CTA_Cuenta_De_Albaran, Descripciones.CTA_Cuenta_De_Albaran, "400900");
        // las cuentas de existencias y de variación de existencias son de cada almacén (ver AlmacenDtm)
        // contrapartida de los apuntes de gasto e ingreso de un expediente: contabilidad la reclasifica cuando llega el documento (factura, pago, cobro)
        public static string PendientesDeAplicacion => CacheDeVariable.ObtenerVariable(Variable.CTA_Cuenta_Pendiente_De_Aplicacion, Descripciones.CTA_Cuenta_Pendiente_De_Aplicacion, "555000");
        public static string Clientes => CacheDeVariable.ObtenerVariable(Variable.CTA_Cuenta_De_Cliente, Descripciones.CTA_Cuenta_De_Cliente, "430000");
    }

    [Table(Tablas.CUENTA, Schema = Esquemas.CONTABILIDAD)]
    public class CuentaDtm : RegistroConNombreDtm, IRegistroDeParametrizacion
    {
        public string Codigo { get; set; }
        public override string Expresion => $"({Codigo}) {Nombre}";
    }

    public static partial class ModeloContable
    {
        public static void Cuentas(ModelBuilder modelBuilder)
        {
            ApiDeRegistroDtm.DefinirCampoIdDtm<CuentaDtm>(modelBuilder);
            ApiDeNombreDtm.DefinirCampoNombreDtm<CuentaDtm>(modelBuilder, unico: true);

            modelBuilder.Entity<CuentaDtm>().Property(p => p.Codigo).HasColumnName(ICampos.CODIGO).HasColumnType($"{IDominio.CUENTA_CONTABLE})").IsRequired();
            modelBuilder.Entity<CuentaDtm>()
                   .HasIndex(p => p.Codigo)
                   .HasDatabaseName($"I_{Tablas.CUENTA}_{ICampos.CODIGO}").IsUnique(true);

        }
    }
}
