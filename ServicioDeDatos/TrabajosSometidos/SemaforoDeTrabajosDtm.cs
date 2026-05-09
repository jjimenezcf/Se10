using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;



namespace ServicioDeDatos.TrabajosSometidos
{

    [Table(Tablas.SEMAFORO, Schema = Esquemas.TRABAJO)]
    public class SemaforoDeTrabajosDtm : RegistroDtm
    {
        public int IdTrabajoUsuario { get; set; }
        public DateTime Iniciado { get; set; }
        public string login { get; set; }
    }

    public static class TablaSemaforoDeTrabajos
    {
        public static string Tabla => ApiDeRegistroDtm.EsquemaTabla(typeof(SemaforoDeTrabajosDtm));
        public static void Definir(ModelBuilder mb)
        {
            mb.Entity<SemaforoDeTrabajosDtm>().Property(p => p.IdTrabajoUsuario).HasColumnName(ICampos.ID_TRABAJO).IsRequired().HasColumnType(IDominio.INT);
            mb.Entity<SemaforoDeTrabajosDtm>().Property(p => p.Iniciado).HasColumnName(ICampos.INICIADO).IsRequired().HasColumnType(IDominio.DATETIME_2);
            mb.Entity<SemaforoDeTrabajosDtm>().Property(p => p.login).HasColumnName(ICampos.LOGIN).IsRequired().HasColumnType(IDominio.VARCHAR_50);
            mb.Entity<SemaforoDeTrabajosDtm>().HasAlternateKey(p => p.IdTrabajoUsuario).HasName($"AK_{Tablas.SEMAFORO}_{ICampos.ID_TRABAJO}");
        }
    }

    public static class SemaforoDeTrabajosSql
    {
        public static string CrearSemaforo = $"insert into {Esquemas.TRABAJO}.{Tablas.SEMAFORO} ({ICampos.ID_TRABAJO}, {ICampos.INICIADO}, {ICampos.LOGIN}) VALUES(@Id,@Iniciado,@Login)";
        public static string BorrarSemaforo = $"Delete from {Esquemas.TRABAJO}.{Tablas.SEMAFORO} where {ICampos.ID_TRABAJO} = @Id";
    }
}
