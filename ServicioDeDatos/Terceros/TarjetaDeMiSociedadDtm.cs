using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using Utilidades;
using static Dapper.SqlMapper;

namespace ServicioDeDatos.Terceros
{
    [Table(Tablas.SOCIEDAD + "_" + nameof(Sufijo.TARJETA), Schema = Esquemas.TERCEROS)]
    public class TarjetaDeMiSociedadDtm : RegistroDtm, IDetalle, IUsaActiva, IAuditoria
    {
        public int IdElemento { get; set; }
        public SociedadDtm Elemento { get; set; }
        IElementoDtm IDetalle.Elemento => Elemento;

        public enumClaseDeTarjeta Clase { get; set; }
        public enumModoTarjeta Modo { get; set; }
        public int IdCuentaDeCargo { get; set; }
        public CuentaDeMiSociedadDtm CuentaDeCargo { get; set; }

        public bool Activa { get; set; }

        public int IdUsuaCrea { get; set; }
        public DateTime FechaCreacion { get; set; }
        public UsuarioDtm UsuarioCreador { get; set; }

        public int? IdUsuaModi { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public UsuarioDtm UsuarioModificador { get; set; }

        public enumNegocio Negocio => enumNegocio.Sociedad;

        public string Numero { get; set; }

        public string Alias { get; set; }

        public string Expresion => $"({Alias}) {Numero}";

        [Timestamp]
        public byte[] RowVersion { get; set; }
    }

    public static partial class ModeloDeTerceros
    {
        public static void TarjetaDeMiSociedad(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<TarjetaDeMiSociedadDtm>().Ignore(x => x.Negocio);
            modelBuilder.Entity<TarjetaDeMiSociedadDtm>().Ignore(x => x.Elemento);
            modelBuilder.Entity<TarjetaDeMiSociedadDtm>().Ignore(x => x.Expresion);

            modelBuilder.Entity<TarjetaDeMiSociedadDtm>().Property(nameof(TarjetaDeMiSociedadDtm.IdElemento)).HasColumnName(ICampos.ID_ELEMENTO).HasColumnType(IDominio.INT).IsRequired(true);
            ApiDeRegistroDtm.DefinirFk<TarjetaDeMiSociedadDtm, SociedadDtm>(modelBuilder, nameof(TarjetaDeMiSociedadDtm.IdElemento), ICampos.ID_ELEMENTO, unico: false);
            ApiDeRegistroDtm.DefinirCampoFk<TarjetaDeMiSociedadDtm>(modelBuilder, nameof(TarjetaDeMiSociedadDtm.CuentaDeCargo), nameof(TarjetaDeMiSociedadDtm.IdCuentaDeCargo), ICampos.ID_CUENTA, requerida: true, unico: false);
            modelBuilder.Entity<TarjetaDeMiSociedadDtm>().Property(p => p.Clase).HasColumnName(ICampos.CLASE).HasColumnType(IDominio.VARCHAR_30).IsRequired();
            modelBuilder.Entity<TarjetaDeMiSociedadDtm>().Property(p => p.Modo).HasColumnName(ICampos.MODO).HasColumnType(IDominio.VARCHAR_30).IsRequired();
            modelBuilder.Entity<TarjetaDeMiSociedadDtm>().Property(p => p.Alias).HasColumnName(ICampos.ALIAS).HasColumnType(IDominio.VARCHAR_250).IsRequired();
            ApiDeElementoDtm.DefinirCamposDeAuditoria<TarjetaDeMiSociedadDtm>(modelBuilder);
            modelBuilder.Entity<TarjetaDeMiSociedadDtm>().Property(x=> x.Activa).HasColumnName(ICampos.ACTIVA).HasColumnType(IDominio.BIT).IsRequired(true).HasDefaultValue(true);
            modelBuilder.Entity<TarjetaDeMiSociedadDtm>().HasIndex(x => new { x.IdElemento, x.Numero}).IsUnique(true).HasDatabaseName($"I_{ApiDeRegistroDtm.NombreDeTabla(typeof(TarjetaDeMiSociedadDtm))}_{ICampos.ID_ELEMENTO}_{ICampos.NUMERO}");

            modelBuilder.Entity<TarjetaDeMiSociedadDtm>().Property(p => p.Numero).HasColumnName(ICampos.NUMERO).HasColumnType(IDominio.VARCHAR_20).IsRequired();
        }
    }

}

