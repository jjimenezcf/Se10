using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Terceros;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Utilidades;

namespace ServicioDeDatos.Entorno
{
    public static class ltrDeUnUsuario
    {
        public const string Usuarios = nameof(Usuarios);
        public const string UsuariosConTareas = nameof(UsuariosConTareas);
        public const string Logout = "logout";
        public const string Accion_SolicitudDeNuevaContrasena = nameof(Accion_SolicitudDeNuevaContrasena);
        public const string Accion_ContraseñaErronea = nameof(Accion_ContraseñaErronea);
        public const string Accion_ResetearIntentosFallidos = nameof(Accion_ResetearIntentosFallidos);
        public const string Motivo_OlvidoDeContraseña = "1";
        public const string Motivo_ReglasDeContraseña = "2";
        public const string PasswordPorDefecto = "Se@12345678";
    }

    [Table(Tablas.USUARIO, Schema = Esquemas.ENTORNO)]
    public class UsuarioDtm : RegistroConNombreDtm, IUsaArchivo
    {
        [Required]
        [Column(ICampos.LOGIN, TypeName = IDominio.VARCHAR_50)]
        public string Login { get; set; }

        [Required]
        [Column(ICampos.APELLIDO, TypeName = IDominio.VARCHAR_250)]
        public string Apellido { get; set; }
                
        [Required]
        [Column(ICampos.F_ALTA, TypeName = IDominio.DATE)]
        public DateTime Alta { get; set; }

        [Required]
        [Column(ICampos.PASSWORD, TypeName =IDominio.VARCHAR_250)]
        public string password { get; set; }

        public int? IdArchivo { get; set; }
        public virtual ArchivoDtm Archivo { get; set; }

        [Column(ICampos.ADMINISTRADOR, TypeName = IDominio.BIT)]
        public bool EsAdministrador { get; set; }

        public string eMail { get; set; }

        public int IdAgenda { get; set; }

        public int? IdCertificado { get; set; }

        public bool Activo { get; set; }

        public Guid? Guid { get; set; }

        public DateTime? SolicitadaEl { get; set; }
        
        public int IntentosFallidos { get; set; }

        public DateTime? BloqueadoEl { get; set; }

        public override string Expresion => $"({Login}) {Apellido}, {Nombre}" ;
        
        public virtual ICollection<UsuariosDeUnPermisoDtm> Permisos { get; private set; }

        public virtual ICollection<PuestosDeUnUsuarioDtm> Puestos { get; private set; }

        public static string NombreCompleto(UsuarioDtm x)
        {
            return x == null ? "" : x.Expresion;
        }

        public static bool EsClienteWeb(ContextoSe contexto)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Ter_EsClienteWeb);
            if (!cache.ContainsKey(contexto.Usuario.Id.ToString()))
            {
                cache[contexto.Usuario.Id.ToString()] = contexto.Set<UsuarioDeClienteDtm>().Any(x => x.IdUsuario == contexto.Usuario.Id);
            }
            return (bool)cache[contexto.Usuario.Id.ToString()];
        }

        public string MiAgenda => $"Agenda de {Login}";
        public string MiCertificado => $"Cerificado personal de {Login}";

    }

    public static class ApiDeUsuarioDtm
    {
        public static void Definir(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<UsuarioDtm>().Property(p => p.Nombre).HasColumnName("NOMBRE").HasColumnType("VARCHAR(50)").IsRequired();

            ApiDeNombreDtm.DefinirCampoNombreDtm<UsuarioDtm>(modelBuilder, 50);

            modelBuilder.Entity<UsuarioDtm>().Property(p => p.eMail).HasColumnName(ICampos.EMAIL).HasColumnType(IDominio.VARCHAR_50).IsRequired().HasDefaultValue("pendiente@se.com");
            modelBuilder.Entity<UsuarioDtm>().Property(p => p.EsAdministrador).IsRequired(true).HasDefaultValue(false);
            modelBuilder.Entity<UsuarioDtm>().Property(p => p.IdAgenda).HasColumnName(ICampos.ID_AGENDA).HasColumnType(IDominio.INT).IsRequired();
            modelBuilder.Entity<UsuarioDtm>().Property(p => p.IdCertificado).HasColumnName(ICampos.ID_CERTIFICADO).HasColumnType(IDominio.INT).IsRequired(false);
            
            ApiDeRegistroDtm.DefinirFk<UsuarioDtm, AgendaDtm>(modelBuilder, nameof(UsuarioDtm.IdAgenda), ICampos.ID_AGENDA, unico: true);
            ApiDeRegistroDtm.DefinirFk<UsuarioDtm, CertificadoDtm>(modelBuilder, nameof(UsuarioDtm.IdCertificado), ICampos.ID_CERTIFICADO, unico: true);

            modelBuilder.Entity<UsuarioDtm>().Property(usuario => usuario.Activo).HasColumnName(ICampos.ACTIVO).HasColumnType(IDominio.BIT).IsRequired(true).HasDefaultValue(true);


            modelBuilder.Entity<UsuarioDtm>().Property(x => x.Guid).HasColumnName(ICampos.GUID).HasColumnType(IDominio.UNIQUEIDENTIFIER).IsRequired(false);
            modelBuilder.Entity<UsuarioDtm>().Property(nameof(UsuarioDtm.SolicitadaEl)).HasColumnName(ICampos.SOLICITADO_EL).HasColumnType(IDominio.DATETIME_2).IsRequired(false);
            modelBuilder.Entity<UsuarioDtm>().Property(nameof(UsuarioDtm.BloqueadoEl)).HasColumnName(ICampos.BLOQUEADO_EL).HasColumnType(IDominio.DATETIME_2).IsRequired(false);
            modelBuilder.Entity<UsuarioDtm>().Property(p => p.IntentosFallidos).HasColumnName(ICampos.FALLIDOS).HasColumnType(IDominio.INT).IsRequired().HasDefaultValue(0);

            modelBuilder.Entity<UsuarioDtm>().HasIndex(v => new { v.Login }).IsUnique(true).HasDatabaseName($"I_{Tablas.USUARIO}_{ICampos.LOGIN}");
            modelBuilder.Entity<UsuarioDtm>().HasMany(tu => tu.Puestos).WithOne(p => p.Usuario).HasForeignKey(p => p.IdUsuario);
            modelBuilder.Entity<UsuarioDtm>().HasMany(tu => tu.Permisos).WithOne(p => p.Usuario).HasForeignKey(p => p.IdUsuario);

            ApiDeElementoDtm.DefinirCampoArchivo<UsuarioDtm>(modelBuilder);            

        }

        internal static void DefinirResponsable<T>(ModelBuilder modelBuilder, bool obligatorio) where T:RegistroDtm
        {
            modelBuilder.Entity<T>().Property(nameof(IPuedeUsarResponsable.IdResponsable)).HasColumnName(ICampos.ID_RESPONSABLE).HasColumnType("INT").IsRequired(obligatorio);
            ApiDeRegistroDtm.DefinirFk<T>(modelBuilder, nameof(IPuedeUsarResponsable.Responsable), nameof(IPuedeUsarResponsable.IdResponsable), ICampos.ID_RESPONSABLE, unico: false);
        }
    }

}
