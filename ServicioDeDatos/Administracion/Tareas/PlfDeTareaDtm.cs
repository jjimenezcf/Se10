
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using Utilidades;
using static ServicioDeDatos.Elemento.Enumerados;

namespace ServicioDeDatos.Tarea
{
    public class ltrPlfDeTarea
    {
        public const string FiltroPorPlfDeInicio = nameof(FiltroPorPlfDeInicio);
        public const string FiltroPorPlfDeFin = nameof(FiltroPorPlfDeFin);
        public const string FiltroPorIniciada = nameof(FiltroPorIniciada);
        public const string FiltroFinalizada = nameof(FiltroFinalizada);
        public const string FiltroDuracion = nameof(FiltroDuracion);
        public const string FiltroMedidoEn = nameof(FiltroMedidoEn);
    }

    [Table(Tablas.TAREA + "_" + Sufijo.PANIFICACION, Schema = Esquemas.TAREA)]
    public class PlfDeTareaDtm : Ampliacion<TareaDtm>
    {
        public override enumNegocio Negocio => enumNegocio.Tarea;
        //public new TareaDtm Elemento;

        public DateTime? PlfDeInicio { get; set; }
        public DateTime? PlfDeFin { get; set; }
        public DateTime? Iniciada { get; set; }
        public DateTime? Finalizada { get; set; }
        public decimal? Duracion { get; set; }
        public enumDurabilidad? MedidoEn { get; set; }

        public bool Igual(PlfDeTareaDtm anterior)
        {
            if (!Iniciada.Igual(anterior.Iniciada)) return false;
            if (!Finalizada.Igual(anterior.Finalizada)) return false;
            if (!PlfDeInicio.Igual(anterior.PlfDeInicio)) return false;
            if (!PlfDeFin.Igual(anterior.PlfDeFin)) return false;

            return true;
        }

        public decimal? Horas => EnMinutos() / 60;

        public decimal? EnMinutos()
        {
            decimal minutos = 0;
            switch (MedidoEn)
            {
                case enumDurabilidad.Minutos: minutos = (decimal)Duracion; break;
                case enumDurabilidad.Horas: minutos = (decimal)Duracion * 60; break;
                case enumDurabilidad.Jornadas: minutos = (decimal)Duracion * 60 * 8; break;
                case enumDurabilidad.Dias: minutos = (decimal)Duracion * 60 * 24; break;
            }
            return minutos == 0 ? null : minutos;
        }

        public decimal? EnHoras()
        {
            decimal horas = 0;
            switch (MedidoEn)
            {
                case enumDurabilidad.Minutos: horas = (decimal)(EnMinutos() / 60); break;
                case enumDurabilidad.Horas: horas = (decimal)Duracion; break;
                case enumDurabilidad.Jornadas: horas = (decimal)Duracion * 8; break;
                case enumDurabilidad.Dias: horas = (decimal)Duracion * 24; break;
            }
            return horas == 0 ? null : horas;
        }

        public decimal? EnJornadas()
        {
            decimal jornadas = 0;
            switch (MedidoEn)
            {
                case enumDurabilidad.Minutos: jornadas = (decimal)(EnHoras() / 8) ; break;
                case enumDurabilidad.Horas: jornadas = (decimal)Duracion / 8 ; break;
                case enumDurabilidad.Jornadas: jornadas = (decimal)Duracion; break;
                case enumDurabilidad.Dias: jornadas = (decimal)(EnHoras() / 8); break;
            }
            return jornadas == 0 ? null : jornadas;
        }

        public decimal? EnDias()
        {
            decimal dias = 0;
            switch (MedidoEn)
            {
                case enumDurabilidad.Minutos: dias = (decimal)(EnHoras() / 24); break;
                case enumDurabilidad.Horas: dias = (decimal)Duracion / 24; break;
                case enumDurabilidad.Jornadas: dias = (decimal)(EnHoras() / 24); break;
                case enumDurabilidad.Dias: dias = (decimal)(Duracion); break;
            }
            return dias == 0 ? null : dias;
        }

        public decimal? DuracionEn(enumDurabilidad medidoEn)
        {
            //return ((decimal)Duracion).DuracionEn((enumDurabilidad)MedidoEn, medidoEn);

            if (medidoEn == enumDurabilidad.Dias) return EnDias();
            if (medidoEn == enumDurabilidad.Jornadas) return EnJornadas();
            if (medidoEn == enumDurabilidad.Horas) return EnHoras();
            return EnMinutos();
        }
    }

    public static class CalculoDeDurabilidades
    {
        public static decimal? DuracionEn(this decimal valor, enumDurabilidad medidoEn, enumDurabilidad convertirA)
        {
            if (convertirA == enumDurabilidad.Dias) return valor.EnDias(medidoEn);
            if (convertirA == enumDurabilidad.Jornadas) return valor.EnJornadas(medidoEn);
            if (convertirA == enumDurabilidad.Horas) return valor.EnHoras(medidoEn);
            return valor.EnMinutos(medidoEn);

        }

        private static decimal EnDias(this decimal valor, enumDurabilidad medidoEn)
        {
            decimal dias = 0;
            switch (medidoEn)
            {
                case enumDurabilidad.Minutos: dias = (decimal)(valor.EnHoras(medidoEn) / 24); break;
                case enumDurabilidad.Horas: dias = valor / 24; break;
                case enumDurabilidad.Jornadas: dias = (decimal)(valor.EnHoras(medidoEn) / 24); break;
                case enumDurabilidad.Dias: dias = valor; break;
            }
            return dias;

        }

        private static decimal EnJornadas(this decimal valor, enumDurabilidad medidoEn)
        {
            decimal jornadas = 0;
            switch (medidoEn)
            {
                case enumDurabilidad.Minutos: jornadas = (decimal)(valor.EnHoras(medidoEn) / 8); break;
                case enumDurabilidad.Horas: jornadas = valor / 8; break;
                case enumDurabilidad.Jornadas: jornadas = valor; break;
                case enumDurabilidad.Dias: jornadas = (decimal)(valor.EnHoras(medidoEn) / 8); break;
            }
            return jornadas;

        }

        private static decimal EnHoras(this decimal valor, enumDurabilidad medidoEn)
        {
            decimal horas = 0;
            switch (medidoEn)
            {
                case enumDurabilidad.Minutos: horas = (decimal)(valor.EnMinutos(medidoEn) / 60); break;
                case enumDurabilidad.Horas: horas = valor; break;
                case enumDurabilidad.Jornadas: horas = valor * 8; break;
                case enumDurabilidad.Dias: horas = valor * 24; break;
            }
            return horas;
        }

        private static decimal EnMinutos(this decimal valor, enumDurabilidad medidoEn)
        {
            decimal minutos = 0;
            switch (medidoEn)
            {
                case enumDurabilidad.Minutos: minutos = valor; break;
                case enumDurabilidad.Horas: minutos = valor * 60; break;
                case enumDurabilidad.Jornadas: minutos = valor * 60 * 8; break;
                case enumDurabilidad.Dias: minutos = valor * 60 * 24; break;
            }
            return minutos;
        }
    }


    public static partial class ModeloDeTarea
    {
        internal static void PlfDeTarea(ModelBuilder modelBuilder)
        {
            ModeloDeAmpliaciones.DefinirAmpliacion<TareaDtm, PlfDeTareaDtm>(modelBuilder, nameof(TareaDtm.Planificacion));

            modelBuilder.Entity<PlfDeTareaDtm>().Property(p => p.PlfDeInicio).HasColumnName(ICampos.P_INICIO).HasColumnType(IDominio.DATETIME_2).IsRequired(false);
            modelBuilder.Entity<PlfDeTareaDtm>().Property(p => p.PlfDeFin).HasColumnName(ICampos.P_FIN).HasColumnType(IDominio.DATETIME_2).IsRequired(false);
            modelBuilder.Entity<PlfDeTareaDtm>().Property(p => p.Iniciada).HasColumnName(ICampos.R_INICIO).HasColumnType(IDominio.DATETIME_2).IsRequired(false);
            modelBuilder.Entity<PlfDeTareaDtm>().Property(p => p.Finalizada).HasColumnName(ICampos.R_FIN).HasColumnType(IDominio.DATETIME_2).IsRequired(false);

            modelBuilder.Entity<PlfDeTareaDtm>().Property(p => p.Duracion).HasColumnName(ICampos.VALOR).HasColumnType(IDominio.DECIMAL).IsRequired(false);
            modelBuilder.Entity<PlfDeTareaDtm>().Property(p => p.MedidoEn).HasColumnName(ICampos.MEDIDO_EN).HasColumnType(IDominio.VARCHAR_30).IsRequired(false);

        }

    }
}
