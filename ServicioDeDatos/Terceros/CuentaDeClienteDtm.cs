using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.SistemaDocumental;
using Utilidades;

namespace ServicioDeDatos.Terceros
{
    [Table(Tablas.CLIENTE + "_" + nameof(Sufijo.CUENTA), Schema = Esquemas.TERCEROS)]
    public class CuentaDeClienteDtm : RegistroDtm, IDetalle, IUsaCuentaBancaria
    {
        public int IdElemento { get; set; }
        public ClienteDtm Elemento { get; set; }
        IElementoDtm IDetalle.Elemento => Elemento;

        public enumClaseDeCuentaBancaria Clase { get; set; }
        public int IdCuenta { get; set; }
        public CuentaBancariaDtm Cuenta { get; set; }

        public int? IdArchivo { get; set; }
        public ArchivoDtm Archivo { get; set; }

        public bool Activa { get; set; }

        public int IdUsuaCrea { get; set; }
        public DateTime FechaCreacion { get; set; }
        public UsuarioDtm UsuarioCreador { get; set; }

        public int? IdUsuaModi { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public UsuarioDtm UsuarioModificador { get; set; }

        public enumNegocio Negocio => enumNegocio.Cliente;

        public string Alias { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }
    }

    public static partial class ModeloDeTerceros
    {
        public static void CuentaDeCliente(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<CuentaDeClienteDtm>().Ignore(x => x.Negocio);
            modelBuilder.Entity<CuentaDeClienteDtm>().Ignore(x => x.Elemento);

            modelBuilder.Entity<CuentaDeClienteDtm>().Property(nameof(CuentaDeClienteDtm.IdElemento)).HasColumnName(ICampos.ID_ELEMENTO).HasColumnType(IDominio.INT).IsRequired(true);
            ApiDeRegistroDtm.DefinirFk<CuentaDeClienteDtm, ClienteDtm>(modelBuilder, nameof(CuentaDeClienteDtm.IdElemento), ICampos.ID_ELEMENTO, unico: false);
            ApiDeRegistroDtm.DefinirCampoFk<CuentaDeClienteDtm>(modelBuilder, nameof(CuentaDeClienteDtm.Cuenta), nameof(CuentaDeClienteDtm.IdCuenta), ICampos.ID_CUENTA, requerida: true, unico: false);
            modelBuilder.Entity<CuentaDeClienteDtm>().Property(p => p.Clase).HasColumnName(ICampos.CLASE).HasColumnType(IDominio.VARCHAR_30).IsRequired();
            modelBuilder.Entity<CuentaDeClienteDtm>().Property(p => p.Alias).HasColumnName(ICampos.ALIAS).HasColumnType(IDominio.VARCHAR_250).IsRequired(false);
            ApiDeElementoDtm.DefinirCampoArchivo<CuentaDeClienteDtm>(modelBuilder, obligatorio: false, unico: true);
            ApiDeElementoDtm.DefinirCamposDeAuditoria<CuentaDeClienteDtm>(modelBuilder);
            modelBuilder.Entity<CuentaDeClienteDtm>().Property(x=> x.Activa).HasColumnName(ICampos.ACTIVA).HasColumnType(IDominio.BIT).IsRequired(true).HasDefaultValue(true);
        }
    }

}

