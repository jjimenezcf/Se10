using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Negocio;

namespace ServicioDeDatos.Callejero
{
    public class ltrCalles
    {
        public const string csvCalle = nameof(csvCalle);
        public const string SeleccionarParaDireccion = nameof(SeleccionarParaDireccion);
        public static readonly string filtroPorBarrio = $"{nameof(CalleDtm.Barrios)}.{nameof(BarrioDtm.Nombre)}";
        public static readonly string filtroPorZona = $"{nameof(CalleDtm.Zonas)}.{nameof(ZonaDtm.Nombre)}";
        public static readonly string filtroPorPais = $"{nameof(CalleDtm.Municipio)}.{nameof(MunicipioDtm.Provincia)}.{nameof(ProvinciaDtm.Pais)}.{nameof(INombre.Nombre)}";
        public static readonly string filtroPorProvincia = $"{nameof(CalleDtm.Municipio)}.{nameof(MunicipioDtm.Provincia)}.{nameof(INombre.Nombre)}";
        public static readonly string filtroPorMunicipio = $"{nameof(CalleDtm.Municipio)}.{nameof(INombre.Nombre)}";

        public static readonly string JoinConMunicipio = nameof(JoinConMunicipio);
        public static readonly string JoinConProvincia = nameof(JoinConProvincia);
        public static readonly string JoinConPais = nameof(JoinConPais);
        public static readonly string JoinConTipoDeVia = nameof(JoinConTipoDeVia);

        public static string filtroPorIdPais = $"{nameof(CalleDtm.Municipio)}.{nameof(MunicipioDtm.Provincia)}.{nameof(ProvinciaDtm.Pais)}.{nameof(RegistroDtm.Id)}";
        public static string filtroPorIdProvincia = $"{nameof(CalleDtm.Municipio)}.{nameof(MunicipioDtm.Provincia)}.{nameof(RegistroDtm.Id)}";
    }


    [Table(Tablas.CALLE, Schema = Esquemas.CALLEJERO)]
    public class CalleDtm : ElementoDtm
    {
        public int IdMunicipio { get; set; }
        public MunicipioDtm Municipio { get; set; }
        public int IdTipoDeVia { get; set; }
        public TipoDeViaDtm TipoDeVia { get; set; }

        public IEnumerable<CpsDeUnaCalleDtm> Cps { get; set; }
        public IEnumerable<BarriosDeUnaCalleDtm> Barrios { get; set; }
        public IEnumerable<ZonasDeUnaCalleDtm> Zonas { get; set; }

        public new string Expresion { get; }

    }

    [Table(Tablas.CALLE + "_" + nameof(Sufijo.AUDITORIA), Schema = Esquemas.CALLEJERO)]
    public class AuditoriaDeUnCalleDtm : AuditoriaDtm
    {
        //public new virtual CalleDtm Elemento { get; set; }
    }
    public static partial class ModeloDeCallejero
    {
        public static void Calle(ModelBuilder modelBuilder)
        {
            ApiDeElementoDtm.DefinirCamposDelElementoDtm<CalleDtm>(modelBuilder);

            modelBuilder.Entity<CalleDtm>().Property(v => v.IdMunicipio)
                .HasColumnName(ICampos.ID_MUNICIPIO)
                .HasColumnType(IDominio.INT)
                .IsRequired(true);

            modelBuilder.Entity<CalleDtm>().Property(v => v.IdTipoDeVia)
                .HasColumnName(ICampos.ID_TIPO_DE_VIA)
                .HasColumnType(IDominio.INT)
                .IsRequired(true);

            modelBuilder.Entity<CalleDtm>()
            .HasOne(p => p.Municipio)
            .WithMany()
            .HasForeignKey(p => p.IdMunicipio)
            .HasConstraintName($"FK_{Tablas.CALLE}_{ICampos.ID_MUNICIPIO}")
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CalleDtm>()
            .HasOne(p => p.TipoDeVia)
            .WithMany()
            .HasForeignKey(p => p.IdTipoDeVia)
            .HasConstraintName($"FK_{Tablas.CALLE}_{ICampos.ID_TIPO_DE_VIA}")
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CalleDtm>().HasIndex(p => p.IdMunicipio).HasDatabaseName($"I_{Tablas.CALLE}_{ICampos.ID_MUNICIPIO}");
            modelBuilder.Entity<CalleDtm>().HasIndex(p => p.IdTipoDeVia).HasDatabaseName($"I_{Tablas.CALLE}_{ICampos.ID_TIPO_DE_VIA}");
            modelBuilder.Entity<CalleDtm>().HasIndex(p => new { p.IdMunicipio, p.IdTipoDeVia, p.Nombre }).HasDatabaseName($"I_{Tablas.CALLE}_{ICampos.ID_MUNICIPIO}_{ICampos.ID_TIPO_DE_VIA}_{ICampos.NOMBRE}").IsUnique();

            modelBuilder.Entity<CalleDtm>().Property(v => v.Expresion).HasColumnName(ICampos.EXPRESION).HasColumnType(IDominio.VARCHAR_MAX)
            .HasComputedColumnSql($"{Esquemas.CALLEJERO}.{Funciones.CC_CALLE_EXPRESION}({ICampos.NOMBRE}, {ICampos.ID_MUNICIPIO}, {ICampos.ID_TIPO_DE_VIA})");

        }

        public static void CalleAudt(ModelBuilder modelBuilder)
        {
            Negocio.ApiDeAuditoria.DefinirCamposDeAuditoriaDtm<AuditoriaDeUnCalleDtm>(modelBuilder);

            //modelBuilder.Entity<AuditoriaDeUnCalleDtm>()
            //.HasOne(p => p.Elemento)
            //.WithMany()
            //.HasForeignKey(p => p.IdElemento)
            //.HasConstraintName($"FK_CALLE_AUDITORIA_ID_ELEMENTO")
            //.OnDelete(DeleteBehavior.Restrict);
        }
    }
}
