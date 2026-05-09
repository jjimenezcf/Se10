using Microsoft.EntityFrameworkCore;
using Utilidades;

namespace ServicioDeDatos.Elemento
{

    public interface IUsaAmpliaciones : IRegistro
    {
    }

    public interface IAmpliacion: IRegistro
    {
        public int IdElemento { get; set; }

        public enumNegocio Negocio { get; }

        public object Elemento { get; }
        void CargarElemento(IElementoDtm elemento); 

    }

    public class Ampliacion<T>: RegistroDtm, IAmpliacion where T : ElementoDtm
    {
        public int IdElemento { get; set; }

        public virtual enumNegocio Negocio { get; }

        public T Elemento { get; set; }

        object IAmpliacion.Elemento => Elemento;

        public void CargarElemento(IElementoDtm registro)
        {
            this.Elemento = (T)registro;
        }
    }

    public interface IUsaDetalles : IRegistro
    {
    }

    public interface IDetalle : IRegistro
    {
        public int IdElemento { get; set; }

        public IElementoDtm Elemento { get; }

        public enumNegocio Negocio { get; }
    }

    public static class ModeloDeAmpliaciones
    {
        public static void DefinirAmpliacion<TPadre, TAmpliacion>(ModelBuilder modelBuilder, string conUno)
        where TPadre : ElementoDtm
        where TAmpliacion : Ampliacion<TPadre>
        {
            ApiDeRegistroDtm.DefinirCampoIdDtm<TAmpliacion>(modelBuilder);

            modelBuilder.Entity<TAmpliacion>().Ignore(x => x.Negocio);
            modelBuilder.Entity<TAmpliacion>().Property(nameof(Ampliacion<TPadre>.IdElemento)).HasColumnName(ICampos.ID_ELEMENTO).HasColumnType(IDominio.INT).IsRequired();
            ApiDeRegistroDtm.DefinirFk<TAmpliacion, TPadre>(modelBuilder, nameof(Ampliacion<TPadre>.IdElemento), ICampos.ID_ELEMENTO, unico: true);

            ApiDeRegistroDtm.DefinirFkUnoUno<TAmpliacion>(modelBuilder,
                nameof(Ampliacion<TPadre>.Elemento),
                nameof(Ampliacion<TPadre>.IdElemento),
                conUno,
                ICampos.ID_ELEMENTO);
        }
    }
}


