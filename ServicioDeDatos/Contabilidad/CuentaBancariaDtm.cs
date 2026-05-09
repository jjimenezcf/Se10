using Microsoft.EntityFrameworkCore;
using Microsoft.SqlServer.Dac.Model;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Terceros;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServicioDeDatos.Contabilidad
{

    public interface IUsaCuentaBancaria : IUsaArchivo, IAuditoria, IUsaActiva
    {
        public enumClaseDeCuentaBancaria Clase { get; set; }
        public int IdCuenta { get; set; }
        public CuentaBancariaDtm Cuenta { get; set; }

        public string Alias { get; set; }
    }


    public class ltrCuentasBancarias
    {
        public static readonly string SinCertificado = "Sin certificado";
    }

    public enum enumClaseDeCuentaBancaria
    {
        [Description("Cuenta de ingreso")]
        Ingreso = 1,
        [Description("Cuenta de pago")]
        Pago = 2,
        [Description("Cuenta de ingreso y pago")]
        Ambas = 3
    }

    public enum enumModoTarjeta
    {
        [Description("Débito")]
        Debito = 1,
        [Description("Crédito")]
        Credito = 2
    }

    public enum enumClaseDeTarjeta
    {
        [Description("Visa")]
        Visa = 1,
        [Description("Matercad")]
        Matercard = 2
    }

    [Table(Tablas.CUENTA_BANCARIA, Schema = Esquemas.CONTABILIDAD)]
    public class CuentaBancariaDtm : RegistroDtm
    {
        public string IsoPais { get; set; }
        public string DcIban { get; set; }
        public string Entidad { get; set; }
        public string Oficina { get; set; }
        public string DcCcc { get; set; }
        public string Numero { get; set; }
        public string NumeroIban => $"{IsoPais}{DcIban}-{Entidad}-{Oficina}-{DcCcc}-{Numero}";
        public BancoDtm Banco { get; set;}
    }

    public static partial class ModeloContable
    {
        public static void CuentasBancarias(ModelBuilder modelBuilder)
        {
            ApiDeRegistroDtm.DefinirCampoIdDtm<CuentaBancariaDtm>(modelBuilder);
            modelBuilder.Entity<CuentaBancariaDtm>().Property(nameof(CuentaBancariaDtm.IsoPais)).HasColumnName(ICampos.ISO2).HasColumnType(IDominio.VARCHAR_2).IsRequired(false);
            modelBuilder.Entity<CuentaBancariaDtm>().Property(nameof(CuentaBancariaDtm.DcIban)).HasColumnName(ICampos.DC_IBAN).HasColumnType(IDominio.VARCHAR_2).IsRequired(false);
            modelBuilder.Entity<CuentaBancariaDtm>().Property(nameof(CuentaBancariaDtm.Entidad)).HasColumnName(ICampos.ENTIDAD).HasColumnType(IDominio.VARCHAR_4).IsRequired(false);
            modelBuilder.Entity<CuentaBancariaDtm>().Property(nameof(CuentaBancariaDtm.Oficina)).HasColumnName(ICampos.OFICINA).HasColumnType(IDominio.VARCHAR_4).IsRequired(false); 
            modelBuilder.Entity<CuentaBancariaDtm>().Property(nameof(CuentaBancariaDtm.DcCcc)).HasColumnName(ICampos.DC_CCC).HasColumnType(IDominio.VARCHAR_2).IsRequired(false);
            modelBuilder.Entity<CuentaBancariaDtm>().Property(nameof(CuentaBancariaDtm.Numero)).HasColumnName(ICampos.NUMERO).HasColumnType(IDominio.VARCHAR_10).IsRequired(false);
            modelBuilder.Entity<CuentaBancariaDtm>().Ignore(x => x.Banco);

            modelBuilder.Entity<CuentaBancariaDtm>().HasAlternateKey(new string[] {
                nameof(CuentaBancariaDtm.IsoPais),
                nameof(CuentaBancariaDtm.DcIban),
                nameof(CuentaBancariaDtm.Entidad),
                nameof(CuentaBancariaDtm.Oficina),
                nameof(CuentaBancariaDtm.DcCcc),
                nameof(CuentaBancariaDtm.Numero)
            }).HasName($"AK_{Tablas.CUENTA_BANCARIA}");

        }
    }

}
