using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;

namespace ServicioDeDatos.Callejero
{


    [Table(Tablas.CODIGO_POSTAL, Schema = Esquemas.CALLEJERO)]
    public class CodigoPostalDtm : RegistroDtm
    {
        public string Codigo { get; set; }

        public string NombreProvincia { get; }
        public string NombreMunicipio { get; }
        public List<CpsDeUnaProvinciaDtm> Provincias { get; set; }
        public List<CpsDeUnMunicipioDtm> Municipios { get; set; }
        public List<CpsDeUnaCalleDtm> Calles { get; set; }
    }

    public static partial class ModeloDeCallejero
    {
        public static void CodigoPostal(ModelBuilder modelBuilder)
        {            
            modelBuilder.Entity<CodigoPostalDtm>().Property(v => v.Codigo)
                .HasColumnName(ICampos.CP)
                .HasColumnType(IDominio.VARCHAR_10)
                .IsRequired(true);

            modelBuilder.Entity<CodigoPostalDtm>().HasIndex(p => p.Codigo).HasDatabaseName("I_CODIGO_POSTAL_CP").IsUnique(true);

            modelBuilder.Entity<CodigoPostalDtm>().Property(v => v.NombreProvincia).HasColumnName(ICampos.PROVINCIA)
                .HasColumnType(IDominio.VARCHAR_250)
                .HasComputedColumnSql($"CALLEJERO.CC_CODIGO_POSTAL_PROVINCIA({ICampos.ID})");
            
            modelBuilder.Entity<CodigoPostalDtm>().Property(v => v.NombreMunicipio).HasColumnName(ICampos.MUNICIPIOS)
                .HasColumnType(IDominio.VARCHAR_250)
                .HasComputedColumnSql($"CALLEJERO.CC_CODIGO_POSTAL_MUNICIPIOS({ICampos.ID})");

            modelBuilder.Entity<CodigoPostalDtm>()
                    .HasMany(p => p.Provincias)
                    .WithOne(p => p.Cp)
                    .HasForeignKey(p => p.IdProvincia);
        }
    }

}
