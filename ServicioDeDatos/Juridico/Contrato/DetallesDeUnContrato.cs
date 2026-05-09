using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.SqlServer.Dac.Model;
using ServicioDeDatos.Elemento;
using Utilidades;

namespace ServicioDeDatos.Juridico
{

    [Table(Tablas.CONTRATO + "_" + nameof(Sufijo.NATURALEZAS), Schema = Esquemas.JURIDICO)]
    public class NaturalezasDeUnContratoDtm : RegistroDtm, IDetalle
    {
        public int IdElemento { get; set; }
        ContratoDtm Elemento { get; set; }
        IElementoDtm IDetalle.Elemento => Elemento;
        public enumNegocio Negocio => enumNegocio.Contrato;
    }

    [Table(Tablas.CONTRATO + "_" + nameof(Sufijo.SERVICIOS), Schema = Esquemas.JURIDICO)]
    public class ServiciosDeUnContratoDtm : RegistroDtm, IDetalle
    {
        public int IdElemento { get; set; }
        ContratoDtm Elemento { get; set; }
        IElementoDtm IDetalle.Elemento => Elemento;
        public enumNegocio Negocio => enumNegocio.Contrato;
    }
}
