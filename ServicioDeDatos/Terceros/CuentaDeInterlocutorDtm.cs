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
    [Table(Tablas.INTERLOCUTOR + "_" + nameof(Sufijo.CUENTA), Schema = Esquemas.TERCEROS)]
    public class CuentaDeInterlocutorDtm : RegistroDtm, IDetalle, IUsaCuentaBancaria
    {
        public int IdElemento { get; set; }
        public InterlocutorDtm Elemento { get; set; }
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

        public enumNegocio Negocio => enumNegocio.Interlocutor;

        public string Alias { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }
    }

    public static partial class ModeloDeTerceros
    {
        public static void CuentaDeInterlocutor(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CuentaDeInterlocutorDtm>().Ignore(x => x.Negocio);
            modelBuilder.Entity<CuentaDeInterlocutorDtm>().Ignore(x => x.Elemento);

            modelBuilder.Entity<CuentaDeInterlocutorDtm>().Property(nameof(CuentaDeInterlocutorDtm.IdElemento)).HasColumnName(ICampos.ID_ELEMENTO).HasColumnType(IDominio.INT).IsRequired(true);
            ApiDeRegistroDtm.DefinirFk<CuentaDeInterlocutorDtm, InterlocutorDtm>(modelBuilder, nameof(CuentaDeInterlocutorDtm.IdElemento), ICampos.ID_ELEMENTO, unico: false);
            ApiDeRegistroDtm.DefinirCampoFk<CuentaDeInterlocutorDtm>(modelBuilder, nameof(CuentaDeInterlocutorDtm.Cuenta), nameof(CuentaDeInterlocutorDtm.IdCuenta), ICampos.ID_CUENTA, requerida: true, unico: false);
            modelBuilder.Entity<CuentaDeInterlocutorDtm>().Property(p => p.Clase).HasColumnName(ICampos.CLASE).HasColumnType(IDominio.VARCHAR_30).IsRequired();
            modelBuilder.Entity<CuentaDeInterlocutorDtm>().Property(p => p.Alias).HasColumnName(ICampos.ALIAS).HasColumnType(IDominio.VARCHAR_250).IsRequired(false);
            ApiDeElementoDtm.DefinirCampoArchivo<CuentaDeInterlocutorDtm>(modelBuilder, obligatorio: false, unico: true);
            ApiDeElementoDtm.DefinirCamposDeAuditoria<CuentaDeInterlocutorDtm>(modelBuilder);
            modelBuilder.Entity<CuentaDeInterlocutorDtm>().Property(x=> x.Activa).HasColumnName(ICampos.ACTIVA).HasColumnType(IDominio.BIT).IsRequired(true).HasDefaultValue(true);
        }
    }

}

