using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Negocio;

namespace ServicioDeDatos.Callejero
{

    public class ltrDeUnaProvincia
    {
        public const string ParametroProvincia = "csvProvincia";
        public const string SeleccionarParaDireccion = nameof(SeleccionarParaDireccion);
    }


    [Table(Tablas.PROVINCIA, Schema = Esquemas.CALLEJERO)]
    public class ProvinciaDtm : ElementoDtm
    {
        public string Codigo { get; set; }
        public string Sigla { get; set; }
        public string Prefijo { get; set; }
        public int IdPais { get; set; }
        public PaisDtm Pais { get; set; }
        public IEnumerable<CpsDeUnaProvinciaDtm> Cps { get; set; }

        public new string Expresion => $"({Codigo}) {Nombre}";
    }

    [Table(Tablas.PROVINCIA + "_" + nameof(Sufijo.AUDITORIA), Schema = Esquemas.CALLEJERO)]
    public class AuditoriaDeUnaProvinciaDtm: AuditoriaDtm
    {
        //public new virtual ProvinciaDtm Elemento { get; set; }
    }

    public static partial class ModeloDeCallejero
    {
        public static void Provincia(ModelBuilder modelBuilder)
        {
            ApiDeElementoDtm.DefinirCamposDelElementoDtm<ProvinciaDtm>(modelBuilder);
            modelBuilder.Entity<ProvinciaDtm>().Property(v => v.Codigo)
                .HasColumnName(ICampos.CODIGO)
                .HasColumnType(IDominio.VARCHAR_2)
                .IsRequired(true);

            modelBuilder.Entity<ProvinciaDtm>().Property(v => v.Sigla)
                .HasColumnName(ICampos.SIGLA)
                .HasColumnType(IDominio.VARCHAR_3)
                .IsRequired(true);

            modelBuilder.Entity<ProvinciaDtm>().Property(v => v.IdPais)
                .HasColumnName(ICampos.ID_PAIS)
                .HasColumnType(IDominio.INT)
                .IsRequired(true);

            modelBuilder.Entity<ProvinciaDtm>().Property(v => v.Prefijo)
                .HasColumnName(ICampos.PREFIJO)
                .HasColumnType(IDominio.VARCHAR_10)
                .IsRequired(true);

            modelBuilder.Entity<ProvinciaDtm>().HasIndex(p => p.Codigo).HasDatabaseName($"I_{Tablas.PROVINCIA}_{ICampos.CODIGO}").IsUnique(true);
            modelBuilder.Entity<ProvinciaDtm>().HasIndex(p => p.Sigla).HasDatabaseName($"I_{Tablas.PROVINCIA}_{ICampos.SIGLA}").IsUnique(true);

            modelBuilder.Entity<ProvinciaDtm>()
                        .HasIndex(p => p.IdPais)
                        .HasDatabaseName($"I_{Tablas.PROVINCIA}_{ICampos.ID_PAIS}");

            modelBuilder.Entity<ProvinciaDtm>()
            .HasOne(p => p.Pais)
            .WithMany()
            .HasForeignKey(p => p.IdPais)
            .HasConstraintName($"FK_{Tablas.PROVINCIA}_{ICampos.ID_PAIS}")
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProvinciaDtm>().Ignore(x => x.Expresion);
        }

        public static void ProvinciaAudt(ModelBuilder modelBuilder)
        {
            Negocio.ApiDeAuditoria.DefinirCamposDeAuditoriaDtm<AuditoriaDeUnaProvinciaDtm>(modelBuilder);

            //modelBuilder.Entity<AuditoriaDeUnaProvinciaDtm>()
            //.HasOne(p => p.Elemento)
            //.WithMany()
            //.HasForeignKey(p => p.IdElemento)
            //.HasConstraintName($"FK_PROVINCIA_AUDITORIA_ID_ELEMENTO")
            //.OnDelete(DeleteBehavior.Restrict);
        }
    }

}

