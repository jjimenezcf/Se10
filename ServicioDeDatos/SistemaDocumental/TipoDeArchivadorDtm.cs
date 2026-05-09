using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using Utilidades;

namespace ServicioDeDatos.SistemaDocumental
{

    public class ltrTipoArchivador
    {
        public const string TipoClientes = "CLI: Documentación";
        public const string TipoGeneral = "ARC: General";
        public const string TipoBackUp = "ARC: Copia de seguridad";
        public const string TipoExportacion = "ARC: Exportacion";
        public const string TipoZip = "ZIP: Archivo";
        public const string TipoSii = "SII: Auditoría";
        public const string SeleccionarParaCopiar = nameof(SeleccionarParaCopiar);
    }

    [Table(Tablas.ARCHIVADOR + "_" + nameof(Sufijo.TIPO), Schema = Esquemas.SISDOC)]
    public class TipoDeArchivadorDtm : TipoDeElementoDtm, IDelSistema, INombreModificable
    {
        public TipoDeArchivadorDtm Padre { get; set; }
        public bool Visible { get; set; }
        public bool DelSistema { get; set; }
        public static new enumNegocio Negocio => enumNegocio.Archivador;

    }

    public static partial class ModeloDocumental
    {
        public static void TipoDeArchivador(ModelBuilder modelBuilder)
        {
            ApiTipoDeElementoDtm.DefinirCamposDelTipoElementoDtm<TipoDeArchivadorDtm>(modelBuilder);
            
            modelBuilder.Entity<TipoDeArchivadorDtm>().Property(p => p.Visible).HasColumnName(ICampos.VISIBLE).HasColumnType(IDominio.BIT).IsRequired();
            modelBuilder.Entity<TipoDeArchivadorDtm>().Property(p => p.DelSistema).HasColumnName(ICampos.DEL_SISTEMA).HasColumnType(IDominio.BIT).IsRequired();
            ApiDeRegistroDtm.DefinirFk<TipoDeArchivadorDtm>(modelBuilder, nameof(TipoDeArchivadorDtm.Padre), nameof(TipoDeArchivadorDtm.IdPadre), ICampos.ID_PADRE, unico: false);

        }
    }
}
