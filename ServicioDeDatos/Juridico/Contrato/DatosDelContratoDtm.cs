using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.SqlServer.Dac.Model;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Terceros;
using Utilidades;

namespace ServicioDeDatos.Juridico
{
    public class ltrDatosDelContrato
    {
        public static string ContratoSinFechaDeFin => "Contrato sin fecha de fin";
        public static string AvisoDeFinDeContrato => "Fin de contrato";
        public static string AvisoPrevioDeFinDeContrato => "Aviso previo de fin de contrato";
        public static string EstoyProrrogando => nameof(EstoyProrrogando);
        public static string FiltroPorCliente => nameof(FiltroPorCliente);
        public static string FiltroPorProveedor => nameof(FiltroPorProveedor);
        public static string FiltroPorConOSinCliente => nameof(FiltroPorConOSinCliente);

        public static string FiltroPorConOSinProveedor => nameof(FiltroPorConOSinProveedor);
        public static string DatosContacto => nameof(DatosContacto);

        public static string ConOSinCliente = nameof(ConOSinCliente);
        public static string ConCliente = nameof(ConCliente);
        public static string SinCliente = nameof(SinCliente);

        public static string ConOSinProveedor = nameof(ConOSinProveedor);
        public static string ConProveedor = nameof(ConProveedor);
        public static string SinProveedor = nameof(SinProveedor);

    }

    [Table(Tablas.CONTRATO + "_" + Sufijo.INF_JURIDICA, Schema = Esquemas.JURIDICO)]
    public class DatosDelContratoDtm : Ampliacion<ContratoDtm>, IPuedeUsarCliente, IPuedeUsarProveedor
    {
        public override enumNegocio Negocio => enumNegocio.Contrato;
        //public new ContratoDtm Elemento;

        public DateTime InicioContrato { get; set; }
        public DateTime? FinContrato { get; set; }
        public int? AvisarAntesDe { get; set; }
        public bool? RecordatorioEnviado { get; set; }

        public int? IdCliente { get; set ; }
        public ClienteDtm Cliente { get; }

        public int? IdProveedor { get; set; }
        public ProveedorDtm Proveedor { get; }

        public string Contacto { get; set; }
        public string eMail { get; set; }
        public string Telefono { get; set; }

        public DateTime? FechaDeAvisoPrevio
        {
            get
            {
                if (AvisarAntesDe == default) return default(DateTime?);
                if (FinContrato == default) return default(DateTime?);
                return ((DateTime)FinContrato).AddMonths(-(int)AvisarAntesDe);
            }
        }
    }


    public static partial class ModeloDeContrato
    {
        internal static void DatosJuridicosDelContrato(ModelBuilder modelBuilder)
        {
            
            ModeloDeAmpliaciones.DefinirAmpliacion<ContratoDtm, DatosDelContratoDtm>(modelBuilder, nameof(ContratoDtm.Datos));

            modelBuilder.Entity<DatosDelContratoDtm>().Property(p => p.InicioContrato).HasColumnName(ICampos.INICIO).HasColumnType(IDominio.DATETIME_2).IsRequired();
            modelBuilder.Entity<DatosDelContratoDtm>().Property(p => p.FinContrato).HasColumnName(ICampos.FIN).HasColumnType(IDominio.DATETIME_2).IsRequired(false);
            modelBuilder.Entity<DatosDelContratoDtm>().Property(p => p.AvisarAntesDe).HasColumnName(ICampos.MESES).HasColumnType(IDominio.INT).IsRequired(false);
            modelBuilder.Entity<DatosDelContratoDtm>().Property(p => p.RecordatorioEnviado).HasColumnName(ICampos.AVISO).HasColumnType(IDominio.BIT).IsRequired(false).HasDefaultValue(false);

            ApiDeRegistroDtm.DefinirCliente<DatosDelContratoDtm>(modelBuilder);
            ApiDeRegistroDtm.DefinirProveedor<DatosDelContratoDtm>(modelBuilder);

            modelBuilder.Entity<DatosDelContratoDtm>().Ignore(x => x.FechaDeAvisoPrevio);
        }

        public static void InsertarDatosJuridicos(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
                insert into {Esquemas.JURIDICO}.{Tablas.CONTRATO}_{Sufijo.INF_JURIDICA}
                   ({ICampos.ID_ELEMENTO}
                   ,{ICampos.INICIO}
                   ,{ICampos.FIN}
                   ,{ICampos.MESES}
                   ,{ICampos.AVISO}) 
                select {ICampos.ID},{ICampos.FECCRE},null,null,null 
                from {Esquemas.JURIDICO}.{Tablas.CONTRATO}
                go
                ");
        }
    }
}
