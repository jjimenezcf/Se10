using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.SqlServer.Dac.Model;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.SistemaDocumental;
using Utilidades;

namespace ServicioDeDatos.Ventas
{

    [Table(Tablas.FACTURA_EMT + "_" + nameof(Sufijo.VERIFACTU), Schema = Esquemas.VENTA)]
    public class VerifactuDtm : Ampliacion<FacturaEmtDtm>
    {
        public override enumNegocio Negocio => enumNegocio.FacturaEmitida;

        //public new FacturaEmtDtm Elemento;
        public int IdArchivador { get; set; }
        public ArchivadorDtm Archivador { get; set; }
        public int IdBlockChain { get; set; }
        public ArchivadorDtm BlockChain { get; set; }
        public string  CSV { get; set; }
        public string Url { get; set; }
        public string Respuesta { get; set; }

        public string Huella { get; set; }

        public bool Cancelada { get; set; } = false;

        public ArchivadorDtm ObtenerBlockChain(ContextoSe contexto, bool errorSiNoHay = true)
        {
            if (IdBlockChain == 0 && !errorSiNoHay)
                return null;

            return BlockChain ??= contexto.Set<ArchivadorDtm>().First(x => x.Id == IdBlockChain);
        }
        public ArchivadorDtm ObtenerArchivador(ContextoSe contexto, bool errorSiNoHay = true)
        {
            if (IdArchivador == 0 && !errorSiNoHay)
                return null;

            return Archivador ??= contexto.Set<ArchivadorDtm>().First(x => x.Id == IdArchivador);
        }
    }


    public static partial class ModeloDeFacturaEmt
    {
        internal static void VerifactuBd(ModelBuilder modelBuilder)
        {
            ModeloDeAmpliaciones.DefinirAmpliacion<FacturaEmtDtm, VerifactuDtm>(modelBuilder, nameof(FacturaEmtDtm.Verifactu));

            ApiDeRegistroDtm.DefinirCampoFk<VerifactuDtm>(modelBuilder, nameof(VerifactuDtm.Archivador), nameof(VerifactuDtm.IdArchivador), ICampos.ID_ARCHIVADOR, requerida: true, unico: true);
            ApiDeRegistroDtm.DefinirCampoFk<VerifactuDtm>(modelBuilder, nameof(VerifactuDtm.BlockChain), nameof(VerifactuDtm.IdBlockChain), ICampos.ID_BLOCKCHAIN, requerida: true, unico: false);

            modelBuilder.Entity<VerifactuDtm>().Property(nameof(VerifactuDtm.CSV)).HasColumnName(ICampos.CSV).HasColumnType(IDominio.VARCHAR_20).IsRequired(false);
            modelBuilder.Entity<VerifactuDtm>().Property(nameof(VerifactuDtm.Url)).HasColumnName(ICampos.URL).HasColumnType(IDominio.URL).IsRequired(false);
            modelBuilder.Entity<VerifactuDtm>().Property(nameof(VerifactuDtm.Respuesta)).HasColumnName(ICampos.RESPUESTA).HasColumnType(IDominio.VARCHAR_255).IsRequired(true);

            modelBuilder.Entity<VerifactuDtm>().Property(p => p.Huella).HasColumnName(ICampos.HUELLA).HasColumnType(IDominio.CHAR_64).IsRequired(false);
            modelBuilder.Entity<VerifactuDtm>().Property(p => p.Cancelada).HasColumnName(ICampos.CANCELADO).HasColumnType(IDominio.BIT).IsRequired(true).HasDefaultValue(false);

        }

    }
}
