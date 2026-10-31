using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Expediente;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServicioDeDatos.Logistica
{
    // Parámetros con los que se entrega una línea o se cancela una entrega (ver GestorDeEntregasDeUnPedido)
    public class ltrEntregasDeUnPedido
    {
        // La línea y el pedido ya leídos y validados, y si el pedido ha de estar en recepción (al darlo por recibido se entrega con el
        // pedido ya cerrado)
        public const string Linea = nameof(Linea);
        public const string Pedido = nameof(Pedido);
        public const string ExigirRecepcion = nameof(ExigirRecepcion);
        // Descripción de la entrega cancelada, se toma antes de borrarla para la traza del pedido
        public const string Descripcion = nameof(Descripcion);
    }

    // Entregas de una línea de un pedido: cada entrega va a un movimiento del almacén (IdAlmacen) o a un movimiento
    // de la obra asociada al expediente (IdExpediente), nunca a ambos (ver CK_PEDIDO_LINEA_RECEPCION_DESTINO)
    [Table(Tablas.PEDIDO + "_" + nameof(Sufijo.LINEA) + "_" + nameof(Sufijo.RECEPCION), Schema = Esquemas.LOGISTICA)]
    public class RecepcionDeLineaDtm : RegistroDtm, IAuditoria
    {
        public int IdLinea { get; set; }
        public LineaDeUnPedidoDtm Linea { get; set; }

        public decimal Cantidad { get; set; }
        public decimal Precio { get; set; }
        public decimal? Descuento { get; set; }
        // calculado, no se guarda; ExtensorDePedidos.Recibido repite la fórmula para sumarlo en la BD, si cambia una ha de cambiar la otra
        public decimal Importe => Cantidad * Precio * (1 - (Descuento ?? 0) / 100);
        public DateTime RecepcionadaEl { get; set; }

        public int? IdExpediente { get; set; }
        public ExpedienteDtm Expediente { get; set; }

        public int? IdAlmacen { get; set; }
        public AlmacenDtm Almacen { get; set; }

        public bool VaAlAlmacen => IdAlmacen != null;
        public bool VaALaObra => IdExpediente != null;

        public int IdUsuaCrea { get; set; }
        public DateTime FechaCreacion { get; set; }
        public UsuarioDtm UsuarioCreador { get; set; }

        public int? IdUsuaModi { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public UsuarioDtm UsuarioModificador { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }
    }


    public static partial class ModeloDePedido
    {
        public static void RecepcionDeLinea(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RecepcionDeLineaDtm>().Ignore(x => x.VaAlAlmacen);
            modelBuilder.Entity<RecepcionDeLineaDtm>().Ignore(x => x.VaALaObra);
            modelBuilder.Entity<RecepcionDeLineaDtm>().Ignore(x => x.Importe);

            ApiDeRegistroDtm.DefinirCampoFk<RecepcionDeLineaDtm>(modelBuilder, nameof(RecepcionDeLineaDtm.Linea), nameof(RecepcionDeLineaDtm.IdLinea), ICampos.ID_LINEA, requerida: true, unico: false);

            modelBuilder.Entity<RecepcionDeLineaDtm>().Property(nameof(RecepcionDeLineaDtm.Cantidad)).HasColumnName(ICampos.CANTIDAD).HasColumnType(IDominio.DECIMAL).IsRequired(true);
            modelBuilder.Entity<RecepcionDeLineaDtm>().Property(nameof(RecepcionDeLineaDtm.Precio)).HasColumnName(ICampos.PRECIO).HasColumnType(IDominio.DECIMAL).IsRequired(true);
            modelBuilder.Entity<RecepcionDeLineaDtm>().Property(nameof(RecepcionDeLineaDtm.Descuento)).HasColumnName(ICampos.DESCUENTO).HasColumnType(IDominio.PORCENTAJE_MENOR_100).IsRequired(false);
            //modelBuilder.Entity<RecepcionDeLineaDtm>().Property(nameof(RecepcionDeLineaDtm.Importe)).HasColumnName(ICampos.IMPORTE).HasColumnType(IDominio.DECIMAL).IsRequired(true);
            modelBuilder.Entity<RecepcionDeLineaDtm>().Property(nameof(RecepcionDeLineaDtm.RecepcionadaEl)).HasColumnName(ICampos.RECEPCIONADA_EL).HasColumnType(IDominio.DATETIME_2).IsRequired(true);

            ApiDeRegistroDtm.DefinirCampoFk<RecepcionDeLineaDtm>(modelBuilder, nameof(RecepcionDeLineaDtm.Expediente), nameof(RecepcionDeLineaDtm.IdExpediente), ICampos.ID_EXPEDIENTE, requerida: false, unico: false);
            ApiDeRegistroDtm.DefinirCampoFk<RecepcionDeLineaDtm>(modelBuilder, nameof(RecepcionDeLineaDtm.Almacen), nameof(RecepcionDeLineaDtm.IdAlmacen), ICampos.ID_ALMACEN, requerida: false, unico: false);

            ApiDeElementoDtm.DefinirCamposDeAuditoria<RecepcionDeLineaDtm>(modelBuilder);

            // la entrega va al almacén o a la obra del expediente, nunca a ambos ni a ninguno
            var tabla = ApiDeRegistroDtm.NombreDeTabla(typeof(RecepcionDeLineaDtm));
            modelBuilder.Entity<RecepcionDeLineaDtm>().ToTable(t => t.HasCheckConstraint($"CK_{tabla}_DESTINO",
                $"([{ICampos.ID_ALMACEN}] IS NOT NULL AND [{ICampos.ID_EXPEDIENTE}] IS NULL) OR ([{ICampos.ID_ALMACEN}] IS NULL AND [{ICampos.ID_EXPEDIENTE}] IS NOT NULL)"));
        }
    }
}
