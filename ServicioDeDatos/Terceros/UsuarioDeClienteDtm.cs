using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using Utilidades;

namespace ServicioDeDatos.Terceros
{
    public class ltrUsuariosDeUnCliente
    {
        public const string FiltroPorUsuarioDeCliente= nameof(FiltroPorUsuarioDeCliente);
        public const string ExcluirUsuariosDeCliente = nameof(ExcluirUsuariosDeCliente);
    }

    [Table(Tablas.CLIENTE + "_" + Sufijo.USUARIO, Schema = Esquemas.TERCEROS)]
    public class UsuarioDeClienteDtm : RegistroDtm, IDetalle
    {
        public int IdElemento { get; set; }
        public ClienteDtm Elemento { get; set; }
        IElementoDtm IDetalle.Elemento => Elemento;

        public int IdUsuario { get; set; }
        public UsuarioDtm Usuario { get; set; }
        public enumNegocio Negocio => enumNegocio.Cliente;

    }
    
    public static partial class ModeloDeTerceros
    {
        public static void UsuarioDeUnCliente(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UsuarioDeClienteDtm>().Ignore(x => x.Negocio);
            modelBuilder.Entity<UsuarioDeClienteDtm>().Ignore(x => x.Elemento);

            modelBuilder.Entity<UsuarioDeClienteDtm>().Property(nameof(UsuarioDeClienteDtm.IdElemento)).HasColumnName(ICampos.ID_ELEMENTO).HasColumnType(IDominio.INT).IsRequired(true);
            ApiDeRegistroDtm.DefinirFk<UsuarioDeClienteDtm, ClienteDtm>(modelBuilder, nameof(UsuarioDeClienteDtm.IdElemento), ICampos.ID_ELEMENTO, unico: false);


            ApiDeRegistroDtm.DefinirCampoFk<UsuarioDeClienteDtm>(modelBuilder, nameof(UsuarioDeClienteDtm.Usuario), nameof(UsuarioDeClienteDtm.IdUsuario), ICampos.ID_USUARIO, requerida: true, unico: true);
        }
    }

}
