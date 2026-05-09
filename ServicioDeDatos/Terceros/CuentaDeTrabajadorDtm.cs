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
    [Table(Tablas.TRABAJADOR + "_" + nameof(Sufijo.CUENTA), Schema = Esquemas.TERCEROS)]
    public class CuentaDeTrabajadorDtm : RegistroDtm, IDetalle, IUsaCuentaBancaria
    {
        public int IdElemento { get; set; }
        public TrabajadorDtm Elemento { get; set; }
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

        public enumNegocio Negocio => enumNegocio.Trabajador;

        public string Alias { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }
    }

    public static partial class ModeloDeTerceros
    {
        public static void CuentaDeTrabajador(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CuentaDeTrabajadorDtm>().Ignore(x => x.Negocio);
            modelBuilder.Entity<CuentaDeTrabajadorDtm>().Ignore(x => x.Elemento);

            modelBuilder.Entity<CuentaDeTrabajadorDtm>().Property(nameof(CuentaDeTrabajadorDtm.IdElemento)).HasColumnName(ICampos.ID_ELEMENTO).HasColumnType(IDominio.INT).IsRequired(true);
            ApiDeRegistroDtm.DefinirFk<CuentaDeTrabajadorDtm, TrabajadorDtm>(modelBuilder, nameof(CuentaDeTrabajadorDtm.IdElemento), ICampos.ID_ELEMENTO, unico: false);
            ApiDeRegistroDtm.DefinirCampoFk<CuentaDeTrabajadorDtm>(modelBuilder, nameof(CuentaDeTrabajadorDtm.Cuenta), nameof(CuentaDeTrabajadorDtm.IdCuenta), ICampos.ID_CUENTA, requerida: true, unico: false);
            modelBuilder.Entity<CuentaDeTrabajadorDtm>().Property(p => p.Clase).HasColumnName(ICampos.CLASE).HasColumnType(IDominio.VARCHAR_30).IsRequired();
            modelBuilder.Entity<CuentaDeTrabajadorDtm>().Property(p => p.Alias).HasColumnName(ICampos.ALIAS).HasColumnType(IDominio.VARCHAR_250).IsRequired(false);
            ApiDeElementoDtm.DefinirCampoArchivo<CuentaDeTrabajadorDtm>(modelBuilder, obligatorio: false, unico: true);
            ApiDeElementoDtm.DefinirCamposDeAuditoria<CuentaDeTrabajadorDtm>(modelBuilder);
            modelBuilder.Entity<CuentaDeTrabajadorDtm>().Property(x=> x.Activa).HasColumnName(ICampos.ACTIVA).HasColumnType(IDominio.BIT).IsRequired(true).HasDefaultValue(true);
        }
    }

}

