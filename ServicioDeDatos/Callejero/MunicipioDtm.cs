using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Negocio;

namespace ServicioDeDatos.Callejero
{
    //https://ine.es/CDINEbase/consultar.do?mes=&operacion=Relaci%F3n+de+municipios+y+c%F3digos+por+provincias+y+comunidades+aut%F3nomas&id_oper=Ir
    public class ltrDeUnMunicipio
    {
        public const string csvMunicipio = nameof(csvMunicipio);
        public static readonly string filtroPorPaisIso2 = $"{nameof(MunicipioDtm.Provincia)}.{nameof(ProvinciaDtm.Pais)}.{nameof(PaisDtm.ISO2)}";
        public static readonly string filtroPorProvincia = $"{nameof(MunicipioDtm.Provincia)}.{nameof(INombre.Nombre)}";
        public static readonly string filtroPorCodigoProvincia = $"{nameof(MunicipioDtm.Provincia)}.{nameof(ProvinciaDtm.Codigo)}";
        public const string SeleccionarParaDireccion = nameof(SeleccionarParaDireccion);
    }

    [Table(Tablas.MUNICIPIO, Schema = Esquemas.CALLEJERO)]
    public class MunicipioDtm : ElementoDtm
    {
        public string DC { get; set; }
        public int IdProvincia { get; set; }
        public ProvinciaDtm Provincia { get; set; }
        public IEnumerable<CpsDeUnMunicipioDtm> Cps { get; set; }
    }

    [Table(Tablas.MUNICIPIO + "_" + nameof(Sufijo.AUDITORIA), Schema = Esquemas.CALLEJERO)]
    public class AuditoriaDeUnMunicipioDtm : AuditoriaDtm
    {
        //public new virtual MunicipioDtm Elemento { get; set; }
    }

    public static partial class ModeloDeCallejero
    {
        public static void Municipio(ModelBuilder modelBuilder)
        {
            ApiDeElementoDtm.DefinirCamposDelElementoDtm<MunicipioDtm>(modelBuilder);

            modelBuilder.Entity<MunicipioDtm>().Property(v => v.DC)
                .HasColumnName(ICampos.DC)
                .HasColumnType(IDominio.VARCHAR_5)
                .IsRequired(true);

            modelBuilder.Entity<MunicipioDtm>().Property(v => v.IdProvincia)
                .HasColumnName(ICampos.ID_PROVINCIA)
                .HasColumnType(IDominio.INT)
                .IsRequired(true);

            modelBuilder.Entity<MunicipioDtm>()
                        .HasIndex(p => p.IdProvincia)
                        .HasDatabaseName($"I_{Tablas.MUNICIPIO}_{ICampos.ID_PROVINCIA}");

            modelBuilder.Entity<MunicipioDtm>()
            .HasOne(p => p.Provincia)
            .WithMany()
            .HasForeignKey(p => p.IdProvincia)
            .HasConstraintName($"FK_{Tablas.MUNICIPIO}_{ICampos.ID_PROVINCIA}")
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MunicipioDtm>().HasIndex(p => new { p.IdProvincia, p.Nombre }).HasDatabaseName($"AK_{Tablas.MUNICIPIO}_{ICampos.ID_PROVINCIA}_{ICampos.NOMBRE}").IsUnique(true);
        }

        public static void MunicipioAudt(ModelBuilder modelBuilder)
        {
            ApiDeAuditoria.DefinirCamposDeAuditoriaDtm<AuditoriaDeUnMunicipioDtm>(modelBuilder);

            //modelBuilder.Entity<AuditoriaDeUnMunicipioDtm>()
            //.HasOne(p => p.Elemento)
            //.WithMany()
            //.HasForeignKey(p => p.IdElemento)
            //.HasConstraintName($"FK_MUNICIPIO_AUDITORIA_ID_ELEMENTO")
            //.OnDelete(DeleteBehavior.Restrict);
        }
    }
}
