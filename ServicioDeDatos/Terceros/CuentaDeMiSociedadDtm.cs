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
    [Table(Tablas.SOCIEDAD + "_" + nameof(Sufijo.CUENTA), Schema = Esquemas.TERCEROS)]
    public class CuentaDeMiSociedadDtm : RegistroDtm, IDetalle, IUsaCuentaBancaria
    {
        public int IdElemento { get; set; }
        public SociedadDtm Elemento { get; set; }
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

        public enumNegocio Negocio => enumNegocio.Sociedad;

        public string Alias { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }
    }

    public static partial class ModeloDeTerceros
    {
        public static void CuentaDeMiSociedad(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<CuentaDeMiSociedadDtm>().Ignore(x => x.Negocio);
            modelBuilder.Entity<CuentaDeMiSociedadDtm>().Ignore(x => x.Elemento);

            modelBuilder.Entity<CuentaDeMiSociedadDtm>().Property(nameof(CuentaDeMiSociedadDtm.IdElemento)).HasColumnName(ICampos.ID_ELEMENTO).HasColumnType(IDominio.INT).IsRequired(true);
            ApiDeRegistroDtm.DefinirFk<CuentaDeMiSociedadDtm, SociedadDtm>(modelBuilder, nameof(CuentaDeMiSociedadDtm.IdElemento), ICampos.ID_ELEMENTO, unico: false);
            ApiDeRegistroDtm.DefinirCampoFk<CuentaDeMiSociedadDtm>(modelBuilder, nameof(CuentaDeMiSociedadDtm.Cuenta), nameof(CuentaDeMiSociedadDtm.IdCuenta), ICampos.ID_CUENTA, requerida: true, unico: true);
            modelBuilder.Entity<CuentaDeMiSociedadDtm>().Property(p => p.Clase).HasColumnName(ICampos.CLASE).HasColumnType(IDominio.VARCHAR_30).IsRequired();
            modelBuilder.Entity<CuentaDeMiSociedadDtm>().Property(p => p.Alias).HasColumnName(ICampos.ALIAS).HasColumnType(IDominio.VARCHAR_250).IsRequired(false);
            ApiDeElementoDtm.DefinirCampoArchivo<CuentaDeMiSociedadDtm>(modelBuilder, obligatorio: false, unico: true);
            ApiDeElementoDtm.DefinirCamposDeAuditoria<CuentaDeMiSociedadDtm>(modelBuilder);
            modelBuilder.Entity<CuentaDeMiSociedadDtm>().Property(x=> x.Activa).HasColumnName(ICampos.ACTIVA).HasColumnType(IDominio.BIT).IsRequired(true).HasDefaultValue(true);
        }
    }

}

