using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;

namespace ServicioDeDatos.Tarea
{


    [Table(Tablas.TAREA + "_" + Sufijo.TIPO_CLASE, Schema = Esquemas.TAREA)]
    public class ClaseDelTipoTareaDtm : ClaseDelTipoDtm
    {
        public new TipoDeTareaDtm Tipo { get; set; }
    }
    
    public static partial class ModeloDeTarea
    {
        public static void ClaseDelTipo(ModelBuilder modelBuilder)
        {
            ApiDeElementoDtm.DefinirCampoTipo<ClaseDelTipoTareaDtm>(modelBuilder, nameof(ClaseDelTipoTareaDtm.Tipo));
            ApiDeElementoDtm.DefinirCampoClase<ClaseDelTipoTareaDtm>(modelBuilder, nameof(ClaseDelTipoTareaDtm.Clase));

            modelBuilder.Entity<ClaseDelTipoTareaDtm>().HasAlternateKey(p => new {p.IdClase, p.IdTipo}).HasName($"AK_{ApiDeRegistroDtm.NombreDeTabla(typeof(ClaseDelTipoTareaDtm))}_{ICampos.ID_CLASE}_{ICampos.ID_TIPO}");
        }
    }

}
