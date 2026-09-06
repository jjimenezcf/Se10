using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.MaestrosTecnico;
using Utilidades;

namespace ServicioDeDatos.Logistica
{

    [Table(Tablas.REGULARIZACION + "_" + nameof(Sufijo.LINEA), Schema = Esquemas.LOGISTICA)]
    public class LineasDeUnaRegularizacionDtm : RegistroDtm, IDetalle, IAuditoria
    {
        public int IdElemento { get; set; }
        public RegularizacionDtm Elemento { get; set; }
        IElementoDtm IDetalle.Elemento => Elemento;

        public int Orden { get; set; }
        public int IdUnitario { get; set; }
        public UnitarioDtm Unitario { get; set; }

        public decimal Cantidad { get; set; }
        public decimal Precio { get; set; }
        public enumNegocio Negocio => enumNegocio.Regularizacion;

        public int IdUsuaCrea { get; set; }
        public DateTime FechaCreacion { get; set; }
        public UsuarioDtm UsuarioCreador { get; set; }

        public int? IdUsuaModi { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public UsuarioDtm UsuarioModificador { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }
    }

    public static partial class ModeloDeRegularizacion
    {
        internal static void DatosDeLineaDeUnaRegularizacion(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LineasDeUnaRegularizacionDtm>().Ignore(x => x.Negocio);
            modelBuilder.Entity<LineasDeUnaRegularizacionDtm>().Ignore(x => x.Elemento);

            ApiDeRegistroDtm.DefinirCampoFk<LineasDeUnaRegularizacionDtm>(modelBuilder, nameof(LineasDeUnaRegularizacionDtm.Elemento), nameof(LineasDeUnaRegularizacionDtm.IdElemento), ICampos.ID_ELEMENTO, requerida: true, unico: false);

            modelBuilder.Entity<LineasDeUnaRegularizacionDtm>().Property(nameof(LineasDeUnaRegularizacionDtm.Orden)).HasColumnName(ICampos.ORDEN).HasColumnType(IDominio.INT).IsRequired(true);

            ApiDeRegistroDtm.DefinirCampoFk<LineasDeUnaRegularizacionDtm>(modelBuilder, nameof(LineasDeUnaRegularizacionDtm.Unitario), nameof(LineasDeUnaRegularizacionDtm.IdUnitario), ICampos.ID_UNITARIO, requerida: true, unico: false);

            modelBuilder.Entity<LineasDeUnaRegularizacionDtm>().Property(nameof(LineasDeUnaRegularizacionDtm.Cantidad)).HasColumnName(ICampos.CANTIDAD).HasColumnType(IDominio.DECIMAL).IsRequired(true);
            modelBuilder.Entity<LineasDeUnaRegularizacionDtm>().Property(nameof(LineasDeUnaRegularizacionDtm.Precio)).HasColumnName(ICampos.PRECIO).HasColumnType(IDominio.DECIMAL).IsRequired(true);

            ApiDeElementoDtm.DefinirCamposDeAuditoria<LineasDeUnaRegularizacionDtm>(modelBuilder);
        }
    }
}
