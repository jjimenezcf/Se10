using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Seguridad;
using Utilidades;

namespace ServicioDeDatos.Expediente
{

    public enum enumClaseDeExpediente
    {
        [Description("Administrativo")]
        administrativo,
        [Description("Jurídico")]
        juridico,
        [Description("Solicitud de contrato")]
        solicitudContrato,
        [Description("De cliente")]
        DeCliente,
        [Description("Con Valoración")]
        ConValoracion
    }

    public static class ExtensionesDeExpedientes
    {
        public static Dictionary<enumClaseDeExpediente, Type> extensiones { get; set; } = new Dictionary<enumClaseDeExpediente, Type>
        {
            {enumClaseDeExpediente.juridico, typeof(DatosJuridicosDtm) }
        };
    }


    [Table(Tablas.EXPEDIENTE + "_" + Sufijo.ESTADO, Schema = Esquemas.EXPEDIENTE)]
    public class EstadoDeUnExpedienteDtm : EstadoDtm, IInstanciaEstado
    {
        public static new enumNegocio Negocio => enumNegocio.Expediente;
    }

    [Table(Tablas.EXPEDIENTE + "_" + Sufijo.TRANSICION, Schema = Esquemas.EXPEDIENTE)]
    public class TransicionesDeUnExpedienteDtm : TransicionDtm
    {
    }

    [Table(Tablas.EXPEDIENTE + "_" + Sufijo.ACCION, Schema = Esquemas.EXPEDIENTE)]
    public class AccionesDeUnExpedienteDtm : AccionesDeTrnDtm
    {
    }


    [Table(Tablas.EXPEDIENTE + "_" + Sufijo.TIPO, Schema = Esquemas.EXPEDIENTE)]
    public class TipoDeExpedienteDtm : TipoConFlujoDtm, IPermisoDeInterventor
    {

        public TipoDeExpedienteDtm Padre { get; set; }
        public int IdPermisoInterventor { get; set; }
        public enumClaseDeExpediente ClaseDeExpediente { get; set; }
        public new EstadoDeUnExpedienteDtm Estado { get ; set ; }
        public PermisoDtm PermisoDeInterventor { get; set; }

        public override IEstado iEstado => Estado;

        public bool ScDeVenta { get; set; }
        public bool ScDeCompra { get; set; }
        public bool UsaTareas { get; set; }
        public bool UsaPpts { get; set; }
        public bool UsaDatosJuridicos { get; set; }
        public static new enumNegocio Negocio => enumNegocio.Expediente;

    }


    [Table(Tablas.EXPEDIENTE + "_" + Sufijo.PLANTILLA + "_" + Sufijo.TIPO, Schema = Esquemas.EXPEDIENTE)]
    public class PlantillaPorTipoDeExpedienteDtm : PlantillaPorTipoDtm
    {

        public new enumNegocio Negocio => enumNegocio.Expediente;

        public new TipoDeExpedienteDtm Tipo { get; set ; }

    }

    public static partial class ModeloDeExpediente
    {
        internal static void EstadosDeUnExpediente(ModelBuilder modelBuilder)
        {
            ApiDeEstado.DefinirCampos<EstadoDeUnExpedienteDtm>(modelBuilder);
        }

        internal static void TransicionesDeUnExpediente(ModelBuilder modelBuilder)
        {
            ApiDeTransicion.DefinirCampos<TransicionesDeUnExpedienteDtm, EstadoDeUnExpedienteDtm>(modelBuilder);
        }

        internal static void AccionesDeUnExpediente(ModelBuilder modelBuilder)
        {
            ApiDeAccionDeTrn.DefinirCampos<AccionesDeUnExpedienteDtm, TransicionesDeUnExpedienteDtm>(modelBuilder);
        }

        internal static void TipoDeExpediente(ModelBuilder modelBuilder)
        {
            ApiTipoDeElementoDtm.DefinirCamposDelTipoElementoDtm<TipoDeExpedienteDtm>(modelBuilder);

            modelBuilder.Entity<TipoDeExpedienteDtm>().Property(x => x.IdEstado).HasColumnType(IDominio.INT).HasColumnName(ICampos.ID_ESTADO).IsRequired();
            modelBuilder.Entity<TipoDeExpedienteDtm>().Property(nameof(TipoDeExpedienteDtm.ClaseDeExpediente)).HasColumnName(ICampos.CLASE_EXPEDIENTE).HasColumnType(IDominio.VARCHAR_30).IsRequired();

            ApiDeRegistroDtm.DefinirFk<TipoDeExpedienteDtm>(modelBuilder, nameof(TipoDeExpedienteDtm.Padre), nameof(TipoDeExpedienteDtm.IdPadre), ICampos.ID_PADRE, unico: false);
            ApiDeRegistroDtm.DefinirFk<TipoDeExpedienteDtm>(modelBuilder, nameof(TipoDeExpedienteDtm.Estado), nameof(TipoDeExpedienteDtm.IdEstado), ICampos.ID_ESTADO, unico: false);
            modelBuilder.Entity<TipoDeExpedienteDtm>().Property(p => p.UsaTareas).HasColumnName(ICampos.USA_TAREAS).HasColumnType(IDominio.BIT).HasDefaultValue(true).IsRequired();
            modelBuilder.Entity<TipoDeExpedienteDtm>().Property(p => p.UsaPpts).HasColumnName(ICampos.USA_PPTS).HasColumnType(IDominio.BIT).HasDefaultValue(false).IsRequired();
            modelBuilder.Entity<TipoDeExpedienteDtm>().Property(p => p.ScDeVenta).HasColumnName(ICampos.SC_VENTA).HasColumnType(IDominio.BIT).HasDefaultValue(false).IsRequired();
            modelBuilder.Entity<TipoDeExpedienteDtm>().Property(p => p.UsaDatosJuridicos).HasColumnName(ICampos.USA_DATOS_JURIDICOS).HasColumnType(IDominio.BIT).HasDefaultValue(false).IsRequired();
            modelBuilder.Entity<TipoDeExpedienteDtm>().Property(p => p.ScDeCompra).HasColumnName(ICampos.SC_COMPRA).HasColumnType(IDominio.BIT).HasDefaultValue(false).IsRequired();
        }

        internal static void PlantillaPorTipo(ModelBuilder modelBuilder)
        {
            Elemento.PlantillaPorTipo.DefinirCamposDePlantillaPorTipoDtm<PlantillaPorTipoDeExpedienteDtm>(modelBuilder);
        }
    }
}
