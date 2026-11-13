using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServicioDeDatos.Logistica
{
    // Parámetros con los que se devuelve una entrega o se cancela su devolución (ver GestorDeDevolucionesDeUnPedido)
    public class ltrDevolucionesDeUnPedido
    {
        // RowVersion de la entrega que el usuario vio en el grid de entregas al devolverla, se valida en AntesDePersistir
        public const string RowVersionDeLaEntrega = nameof(RowVersionDeLaEntrega);
        // Lo leído y validado en AntesDePersistir que se usa en DespuesDePersistir
        public const string Entrega = nameof(Entrega);
        public const string Linea = nameof(Linea);
        public const string Pedido = nameof(Pedido);
        // Acción con la que se graba el preasiento de una devolución en obra ya registrada (al activar los preasientos de pedidos); es la
        // única modificación que admite una devolución
        public const string Accion_GenerarPreasiento = nameof(Accion_GenerarPreasiento);
    }

    // Devolución al proveedor de parte (o todo) lo entregado en una recepción de una línea de un pedido; una recepción puede tener varias
    // devoluciones y entre todas no pueden superar lo entregado. Se valora al precio y descuento de la entrega (ver ExtensorDeDevolucionesDePedido).
    // La de una entrega en un almacén se preasienta con su salida; la de una entrega en obra lleva su propio preasiento (IdPreasiento)
    [Table(Tablas.PEDIDO + "_" + nameof(Sufijo.LINEA) + "_" + nameof(Sufijo.DEVOLUCION), Schema = Esquemas.LOGISTICA)]
    public class DevolucionDeEntregaDtm : RegistroDtm, IAuditoria, IUsaPreasiento
    {
        public int IdLineaDeRecepcion { get; set; }
        public RecepcionDeLineaDtm LineaDeRecepcion { get; set; }

        public DateTime DevueltaEl { get; set; }
        public decimal Cantidad { get; set; }
        public string Motivo { get; set; }

        // sólo las devoluciones de entregas en la obra de un expediente
        public int? IdPreasiento { get; set; }
        public PreasientoDtm Preasiento { get; set; }
        // no se guarda, la calcula ExtensorDeDevolucionesDePedido.CalcularReferencia
        public string Referencia { get; set; }

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
        public static void DevolucionDeEntrega(ModelBuilder modelBuilder)
        {
            ApiDeRegistroDtm.DefinirCampoFk<DevolucionDeEntregaDtm>(modelBuilder, nameof(DevolucionDeEntregaDtm.LineaDeRecepcion), nameof(DevolucionDeEntregaDtm.IdLineaDeRecepcion), ICampos.ID_LINEA_RECEPCION, requerida: true, unico: false);

            modelBuilder.Entity<DevolucionDeEntregaDtm>().Property(nameof(DevolucionDeEntregaDtm.DevueltaEl)).HasColumnName(ICampos.DEVUELTA_EL).HasColumnType(IDominio.DATETIME_2).IsRequired(true);
            modelBuilder.Entity<DevolucionDeEntregaDtm>().Property(nameof(DevolucionDeEntregaDtm.Cantidad)).HasColumnName(ICampos.CANTIDAD).HasColumnType(IDominio.DECIMAL).IsRequired(true);
            modelBuilder.Entity<DevolucionDeEntregaDtm>().Property(nameof(DevolucionDeEntregaDtm.Motivo)).HasColumnName(ICampos.MOTIVO).HasColumnType(IDominio.VARCHAR_2000).IsRequired(true);

            modelBuilder.Entity<DevolucionDeEntregaDtm>().Ignore(x => x.Referencia);
            ApiDeRegistroDtm.DefinirCampoFk<DevolucionDeEntregaDtm>(modelBuilder, nameof(DevolucionDeEntregaDtm.Preasiento), nameof(DevolucionDeEntregaDtm.IdPreasiento), ICampos.ID_PREASIENTO, requerida: false, unico: false);

            ApiDeElementoDtm.DefinirCamposDeAuditoria<DevolucionDeEntregaDtm>(modelBuilder);

            // no se devuelve nada en negativo ni nada vacío
            var tabla = ApiDeRegistroDtm.NombreDeTabla(typeof(DevolucionDeEntregaDtm));
            modelBuilder.Entity<DevolucionDeEntregaDtm>().ToTable(t => t.HasCheckConstraint($"CK_{tabla}_CANTIDAD", $"[{ICampos.CANTIDAD}] > 0"));
        }
    }
}
