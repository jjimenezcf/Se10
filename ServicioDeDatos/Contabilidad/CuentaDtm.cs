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
        public static readonly string SueldosDescripcion = "Cuenta de gastos de sueldos y salarios";
        public static readonly string ServiciosProfesionales = "Cuenta de servicios profesionales";
        public static readonly string PrestacionDeServicios = "Cuenta de prestación de servicios";
        public static readonly string IvaRepercutidodescripcion = "Cuenta de Iva repercutido";
        public static readonly string IvaSoportadodescripcion = "Cuenta de Iva soportado";
        public static readonly string IrpfPersonaFisicadescripcion = "Cuenta de IRPF persona física";
    }


    public static class VariablesDeCuentas
    {
        public static string Sueldos => CacheDeVariable.ObtenerVariable(Variable.CTA_Cuenta_De_Sueldos, Descripciones.CTA_Cuenta_De_Sueldos, "640000");
        public static string Proveedores => CacheDeVariable.ObtenerVariable(Variable.CTA_Cuenta_De_Proveedor, Descripciones.CTA_Cuenta_De_Proveedor, "400000");
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
