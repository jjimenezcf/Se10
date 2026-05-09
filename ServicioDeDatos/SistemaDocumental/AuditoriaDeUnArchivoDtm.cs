using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServicioDeDatos.SistemaDocumental
{

    public static class ltrDeAuditoriaDeArchivo
    {
        //Bloqueado es un texto antiguo, pendiente de sustituir en BD

        public enum enuAccion { Cancelar, Reactivar, Descargar, Bloquear,Bloqueado, Desbloquear, Renombrar, Desenlazar, Enlazar, Copiar, Mover }
        public static readonly string Cancelar = $"{enuAccion.Cancelar}: Al cancelar el elemento: [0]";
        public static readonly string Reactivar = $"{enuAccion.Reactivar}: Al reactivar el elemento: [0]";
        public static readonly string Descargar = $"{enuAccion.Descargar}: [0] ([1])";
        public static readonly string Renombrar = $"{enuAccion.Renombrar}: [0] ([1]) - Anterior: [2]";
        public static readonly string Bloquear = $"{enuAccion.Bloquear}: [0] ([1]) - Motivo: [2]";
        public static readonly string Desbloquear = $"{enuAccion.Desbloquear}: [0] ([1]) - Motivo: [2]";
        public static readonly string Desenlazar = $"{enuAccion.Desenlazar}: [0] ([1]) - Mensaje: [2]";
        public static readonly string Enlazar = $"{enuAccion.Enlazar}: [0] ([1]) - Mensaje: [2]";
        public static readonly string Copiar = $"{enuAccion.Copiar}: [0] ([1]) - Mensaje: [2]";
        public static readonly string Mover = $"{enuAccion.Mover}: [0] ([1]) - Mensaje: [2]";
        public static readonly string DescargaConGuid = $"{enuAccion.Descargar}: El usuario [0] ha creado la descarga con Guid [1]";
        public static readonly string DescargaRealizada = $"{enuAccion.Descargar}: La descarga con Guid [0] se ha realizado";
    }


    [Table(Tablas.ARCHIVO + "_" + Sufijo.AUDITORIA, Schema = Esquemas.SISDOC)]
    public class AuditoriaDeUnArchivoDtm : RegistroDtm
    {
        public int IdArchivo { get; set; }
        public ArchivoDtm Archivo { get; set; }
        public string Auditoria { get; set; }
    }

    public static partial class ModeloDocumental
    {
        public static void AuditoriaDeUnArchivo(ModelBuilder modelBuilder)
        {
            ApiDeElementoDtm.DefinirCampoArchivo<AuditoriaDeUnArchivoDtm>(modelBuilder, obligatorio: true, unico: false);
            modelBuilder.Entity<AuditoriaDeUnArchivoDtm>().Property(b => b.Auditoria).HasColumnName(ICampos.AUDITORIA).HasColumnType(IDominio.VARCHAR_250).IsRequired(true);
        }

    }


}
