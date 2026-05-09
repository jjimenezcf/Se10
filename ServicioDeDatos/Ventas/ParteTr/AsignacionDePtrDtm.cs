
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Terceros;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using Utilidades;
using static ServicioDeDatos.Elemento.Enumerados;

namespace ServicioDeDatos.Ventas
{
    public class ltrDeUnaAsignacion
    {
        public const string IdUnitario = nameof(IdUnitario);
        public const string IdContrato = nameof(IdContrato);
        public const string IdParteTr = nameof(IdParteTr);
        public const string IdCliente = nameof(IdCliente);
        public const string IdPresupuesto = nameof(IdPresupuesto);

        public const string FiltroPorConOSinContrato = nameof(FiltroPorConOSinContrato);
        public const string FiltroPorConOSinPresupuesto = nameof(FiltroPorConOSinPresupuesto);

        public const string MostrarLasPendientes = nameof(MostrarLasPendientes);
    }

    [Table(Tablas.PARTE_TR + "_" + Sufijo.ASIGNACION, Schema = Esquemas.VENTA)]
    public class AsignacionDePtrDtm : RegistroDtm, IDetalle
    {
        public int IdElemento { get; set; }
        public ParteTrDtm Elemento { get; set; }
        IElementoDtm IDetalle.Elemento => Elemento;

        public int IdTrabajador { get; set; }
        public TrabajadorDtm Trabajador { get; set; }

        public DateTime? PlfDeInicio { get; set; }
        public DateTime? PlfDeFin { get; set; }
        public DateTime? Iniciada { get; set; }
        public DateTime? Finalizada { get; set; }
        public decimal? Duracion { get; set; }
        public enumDurabilidad? MedidoEn { get; set; }

        public enumNegocio Negocio => enumNegocio.ParteDeTrabajo;

        public bool InformadoLosDatosRealizacion => Finalizada.HasValue && Iniciada.HasValue;
        public bool InformadoLosDatosPlanificacion => PlfDeInicio.HasValue && PlfDeFin.HasValue;
        public bool InformadoLosDatosDuracion => Duracion.HasValue && MedidoEn.HasValue;
        public bool Informada => InformadoLosDatosRealizacion && InformadoLosDatosPlanificacion && InformadoLosDatosDuracion;

    }


    public static partial class ModeloDeParteTr
    {
        internal static void AsignacionDePtR(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AsignacionDePtrDtm>().Ignore(x => x.Negocio);

            ApiDeRegistroDtm.DefinirCampoFk<AsignacionDePtrDtm>(modelBuilder, nameof(AsignacionDePtrDtm.Elemento), nameof(AsignacionDePtrDtm.IdElemento), ICampos.ID_ELEMENTO, requerida: true, unico: false);
            ApiDeRegistroDtm.DefinirCampoFk<AsignacionDePtrDtm>(modelBuilder, nameof(AsignacionDePtrDtm.Trabajador), nameof(AsignacionDePtrDtm.IdTrabajador), ICampos.ID_TRABAJADOR, requerida: true, unico: false);

            modelBuilder.Entity<AsignacionDePtrDtm>().Property(p => p.PlfDeInicio).HasColumnName(ICampos.P_INICIO).HasColumnType(IDominio.DATETIME_2).IsRequired(false);
            modelBuilder.Entity<AsignacionDePtrDtm>().Property(p => p.PlfDeFin).HasColumnName(ICampos.P_FIN).HasColumnType(IDominio.DATETIME_2).IsRequired(false);
            modelBuilder.Entity<AsignacionDePtrDtm>().Property(p => p.Iniciada).HasColumnName(ICampos.R_INICIO).HasColumnType(IDominio.DATETIME_2).IsRequired(false);
            modelBuilder.Entity<AsignacionDePtrDtm>().Property(p => p.Finalizada).HasColumnName(ICampos.R_FIN).HasColumnType(IDominio.DATETIME_2).IsRequired(false);

            modelBuilder.Entity<AsignacionDePtrDtm>().Property(p => p.Duracion).HasColumnName(ICampos.VALOR).HasColumnType(IDominio.DECIMAL).IsRequired(false);
            modelBuilder.Entity<AsignacionDePtrDtm>().Property(p => p.MedidoEn).HasColumnName(ICampos.MEDIDO_EN).HasColumnType(IDominio.VARCHAR_30).IsRequired(false);
        }

    }
}
