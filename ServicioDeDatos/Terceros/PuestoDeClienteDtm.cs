using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Seguridad;
using Utilidades;

namespace ServicioDeDatos.Terceros
{
    public class ltrPuestosDeUnCliente
    {
        public const string FiltroPorPuestoDeCliente= nameof(FiltroPorPuestoDeCliente);
    }

    [Table(Tablas.CLIENTE + "_" + Sufijo.PUESTO, Schema = Esquemas.TERCEROS)]
    public class PuestoDeClienteDtm : RegistroDtm, IDetalle
    {
        public int IdElemento { get; set; }
        public ClienteDtm Elemento { get; set; }
        IElementoDtm IDetalle.Elemento => Elemento;

        public int IdPuesto { get; set; }
        public PuestoDtm Puesto { get; set; }
        public enumNegocio Negocio => enumNegocio.Cliente;

    }
    
    public static partial class ModeloDeTerceros
    {
        public static void PuestoDeUnCliente(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PuestoDeClienteDtm>().Ignore(x => x.Negocio);
            modelBuilder.Entity<PuestoDeClienteDtm>().Ignore(x => x.Elemento);

            modelBuilder.Entity<PuestoDeClienteDtm>().Property(nameof(PuestoDeClienteDtm.IdElemento)).HasColumnName(ICampos.ID_ELEMENTO).HasColumnType(IDominio.INT).IsRequired(true);
            ApiDeRegistroDtm.DefinirFk<PuestoDeClienteDtm, ClienteDtm>(modelBuilder, nameof(PuestoDeClienteDtm.IdElemento), ICampos.ID_ELEMENTO, unico: false);


            ApiDeRegistroDtm.DefinirCampoFk<PuestoDeClienteDtm>(modelBuilder, nameof(PuestoDeClienteDtm.Puesto), nameof(PuestoDeClienteDtm.IdPuesto), ICampos.IDPUESTO, requerida: true, unico: true);
        }
    }

}
