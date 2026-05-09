using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Utilidades;

namespace ServicioDeDatos.Contabilidad
{
    /// <summary>
    /// comentarios asociados a la clase de IVA:
    /// https://www.boe.es/buscar/pdf/1992/BOE-A-1992-28740-consolidado.pdf
    /// </summary>
    public enum enumClasesDeIvaRep
    {
        [Description("General")]
        IRG,
        [Description("Reducido")]
        IRR,
        [Description("Super reducido")]
        IRS,
        [Description("No sujeto")]
        NSJ,
        [Description("Inversión sujeto pasivo")]
        ISP
    }

    public static class VariablesDeIvaRep
    {
        public static string General => CacheDeVariable.ObtenerVariable(Variable.IVA_General, Descripciones.IVA_General, enumClasesDeIvaRep.IRG.Descripcion());
        public static string Reducido => CacheDeVariable.ObtenerVariable(Variable.IVA_Reducido, Descripciones.IVA_Reducido, enumClasesDeIvaRep.IRR.Descripcion());
        public static string SuperReducido => CacheDeVariable.ObtenerVariable(Variable.IVA_Super_Reducido, Descripciones.IVA_Super_Reducido, enumClasesDeIvaRep.IRS.Descripcion());
        public static string Exportacion => CacheDeVariable.ObtenerVariable(Variable.IVA_Exportacion, Descripciones.IVA_Exportacion, enumClasesDeIvaRep.NSJ.Descripcion());
        public static string NoSujeto => CacheDeVariable.ObtenerVariable(Variable.IVA_NoSujeto, Descripciones.IVA_NoSujeto, enumClasesDeIvaRep.NSJ.Descripcion());
    }


    [Table(Tablas.IVA_REPERCUTIDO, Schema = Esquemas.CONTABILIDAD)]
    public class IvaRepercutidoDtm : RegistroConNombreDtm, IRegistroDeParametrizacion
    {
        public enumClasesDeIvaRep Clase { get; set; }
        public override string Expresion => $"({Clase}) {Porcentaje.Porcentaje(alineacion: true)}";
        public string Detalle => $"({Porcentaje}) {Nombre}";
        public string Exencion => $"({Clase}) {Nombre}";
        public decimal Porcentaje { get; set; }
        public bool Exento { get; set; }
        public int IdCuenta { get; set; }
        public CuentaDtm Cuenta { get; set; }

        public string DescripcionFiscal { get; set; }
    }

    public static partial class ModeloContable
    {
        public static void IvasRepercutido(ModelBuilder modelBuilder)
        {
            ApiDeRegistroDtm.DefinirCampoIdDtm<IvaRepercutidoDtm>(modelBuilder);
            ApiDeNombreDtm.DefinirCampoNombreDtm<IvaRepercutidoDtm>(modelBuilder, unico: true);

            modelBuilder.Entity<IvaRepercutidoDtm>().Property(p => p.Clase).HasColumnName(ICampos.CLASE).HasColumnType($"{IDominio.VARCHAR_10}").IsRequired();
            modelBuilder.Entity<IvaRepercutidoDtm>()
            .HasIndex(p => p.Clase)
            .HasDatabaseName($"I_{Tablas.IVA_REPERCUTIDO}_{ICampos.CLASE}").IsUnique(false);

            modelBuilder.Entity<IvaRepercutidoDtm>().Property(nameof(IvaRepercutidoDtm.Exento)).HasColumnName(ICampos.EXENTO).HasColumnType(IDominio.BIT).IsRequired(true);
            modelBuilder.Entity<IvaRepercutidoDtm>().Property(nameof(IvaRepercutidoDtm.Porcentaje)).HasColumnName(ICampos.PORCENTAJE).HasColumnType(IDominio.PORCENTAJE_MENOR_100).IsRequired(true);
            modelBuilder.Entity<IvaRepercutidoDtm>().Ignore(p => p.Detalle);

            ApiDeRegistroDtm.DefinirCampoFk<IvaRepercutidoDtm>(modelBuilder, nameof(IvaRepercutidoDtm.Cuenta), nameof(IvaRepercutidoDtm.IdCuenta), ICampos.ID_CUENTA, requerida: true, unico: false);

            modelBuilder.Entity<IvaRepercutidoDtm>().Property(p => p.DescripcionFiscal).HasColumnName(ICampos.DES_FISCAL).HasColumnType(IDominio.VARCHAR_250).IsRequired(false);

        }

    }
}
