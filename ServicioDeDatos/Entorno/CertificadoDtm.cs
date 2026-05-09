using Microsoft.EntityFrameworkCore;
using Microsoft.SqlServer.Dac.Model;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.SistemaDocumental;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServicioDeDatos.Entorno
{

    public class ltrCertificados
    {
        public static readonly string EsMiCertificado = nameof(EsMiCertificado);

        public static readonly string FiltrarParaSociedad = nameof(FiltrarParaSociedad);
    }



    public class CredencialesDeConexion : RegistroDtm
    {
        public string Password { get; set; }
        public string Login { get; set; }
    }

    public class CredencialesDeCertificado : RegistroDtm
    {
        public string Password { get; set; }
    }

    public enum enumClaseDeCertificados
    {
        [Description("Certificado de Persona Física")]
        Personal = 1,
        [Description("Certificado de Representante ante las AAPP")]
        RepresentanteAAPP = 2,
        [Description("Certificado corporativo")]
        CertificadoCorporativo = 3,
        [Description("Certificado de Representante legal")]
        RepresentanteLegal = 4,
        [Description("Sello electrónico")]
        SelloElectrónico = 5,
        [Description("Sin Firma")]
        SinFirma = 6,
    }

    [Table(Tablas.CERTIFICADO, Schema = Esquemas.ENTORNO)]
    public class CertificadoDtm : RegistroConNombreDtm, IUsaArchivo
    {
        public enumClaseDeCertificados Clase { get; set; }
        public string Password { get; set; }
        public DateTime SubidoEl { get; set; }
        public DateTime? ExpiraEl { get; set; }

        public PermisoDtm Gestor { get; }
        public int IdGestor { get; set; }

        public int? IdArchivo { get; set; }

        public ArchivoDtm Archivo { get; set; }


        public string Datos => $"Certificado: {Expresion} {Environment.NewLine}" +
                   $"Subido el: {SubidoEl} {Environment.NewLine}" +
                   $"Expira el: {ExpiraEl} {Environment.NewLine}";
    }

    public static partial class ModeloDeEntorno
    {
        public static void Certificados(ModelBuilder modelBuilder)
        {
            ApiDeRegistroDtm.DefinirCampoIdDtm<CertificadoDtm>(modelBuilder);
            ApiDeNombreDtm.DefinirCampoNombreDtm<CertificadoDtm>(modelBuilder, unico: true, conIndice: true);
            modelBuilder.Entity<CertificadoDtm>().Property(p => p.Clase).HasColumnName(ICampos.CLASE_CERTIFICADO).HasColumnType(IDominio.VARCHAR_30).IsRequired();

            ApiDeElementoDtm.DefinirCampoArchivo<CertificadoDtm>(modelBuilder, obligatorio: true, unico: true);

            modelBuilder.Entity<CertificadoDtm>().Property(nameof(CertificadoDtm.Password)).HasColumnName(ICampos.PASSWORD).HasColumnType(IDominio.VARCHAR_250).IsRequired(false);
            modelBuilder.Entity<CertificadoDtm>().Property(nameof(CertificadoDtm.SubidoEl)).HasColumnName(ICampos.SUBIDO_EL).HasColumnType(IDominio.DATETIME_2).IsRequired(true);
            modelBuilder.Entity<CertificadoDtm>().Property(nameof(CertificadoDtm.ExpiraEl)).HasColumnName(ICampos.EXPIRA_EL).HasColumnType(IDominio.DATETIME_2).IsRequired(false);
            ApiDeRegistroDtm.DefinirCampoFk<CertificadoDtm>(modelBuilder, nameof(CertificadoDtm.Gestor), nameof(CertificadoDtm.IdGestor), ICampos.ID_GESTOR, requerida: true, unico: true);
        }
    }

}
