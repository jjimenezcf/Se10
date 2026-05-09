
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Utilidades;

namespace ServicioDeDatos.Ventas
{
    public class ltrDeFacturaRectificada
    {
        public const string SeleccionarEmtParaRectificar = "selemtrect";
        public const string Serie = "R";
        public const string Rectificada = nameof(Rectificada);
    }

    //Documentación
    //https://www.billin.net/blog/como-hacer-factura-rectificactiva/


    [Table(Tablas.FACTURA_EMT + "_" + Sufijo.RECTIFICATIVA, Schema = Esquemas.VENTA)]
    public class RectificativaEmtDtm : RegistroDtm, IDetalle, IAuditoria
    {
        public int IdElemento { get; set; }
        public FacturaEmtDtm Elemento { get; set; }
        IElementoDtm IDetalle.Elemento => Elemento;

        public int IdRectificada { get; set; }
        public FacturaEmtDtm Rectificada { get; set; }

        public string Concepto { get; set; }

        public enumNegocio Negocio => enumNegocio.FacturaEmitida;

        public int IdUsuaCrea { get; set; }
        public DateTime FechaCreacion { get; set; }
        public UsuarioDtm UsuarioCreador { get; set; }

        public int? IdUsuaModi { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public UsuarioDtm UsuarioModificador { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }
    }


    public static partial class ModeloDeFacturaEmt
    {
        internal static void Rectificativa(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RectificativaEmtDtm>().Ignore(x => x.Negocio);
            ApiDeRegistroDtm.DefinirCampoFk<RectificativaEmtDtm>(modelBuilder, nameof(RectificativaEmtDtm.Elemento), nameof(RectificativaEmtDtm.IdElemento), ICampos.ID_ELEMENTO, requerida: true, unico: false);
            ApiDeRegistroDtm.DefinirCampoFk<RectificativaEmtDtm>(modelBuilder, nameof(RectificativaEmtDtm.Rectificada), nameof(RectificativaEmtDtm.IdRectificada), ICampos.ID_FACTURA_EMT, requerida: true, unico: false);

            modelBuilder.Entity<RectificativaEmtDtm>().HasIndex(x => new { x.IdRectificada })
            .IsUnique(true)
            .HasDatabaseName($"I_{ApiDeRegistroDtm.NombreDeTabla(typeof(RectificativaEmtDtm))}_{ICampos.ID_FACTURA_EMT}");

            modelBuilder.Entity<RectificativaEmtDtm>().Property(p => p.Concepto).HasColumnName(ICampos.CONCEPTO).HasColumnType(IDominio.VARCHAR_250).IsRequired(true);

            ApiDeElementoDtm.DefinirCamposDeAuditoria<RectificativaEmtDtm>(modelBuilder);
        }

    }
}
