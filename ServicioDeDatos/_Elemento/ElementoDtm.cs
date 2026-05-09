using System;
using System.ComponentModel.DataAnnotations;
using Gestor.Errores;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Terceros;
using Utilidades;

namespace ServicioDeDatos.Elemento
{
    public class ltrDeUnElemento
    {
        public const string Accion_Renombrar = nameof(Accion_Renombrar);
        public const string Accion_CambiarProveedor = nameof(Accion_CambiarProveedor);
        public const string Accion_Bloquear = nameof(Accion_Bloquear);
        public const string Accion_Desbloquear = nameof(Accion_Desbloquear);
    }


    public interface IUsaTipoConCG : IUsaCg, IPermisosPorCg, IElementoConTipo, IUsaDescripcion
    {

    }

    public interface IElementoDeProcesoDtm : IUsaTipoConCG, IUsaEstado, IUsaObservacion
    {

    }

    public abstract class ElementoDtm : RegistroConNombreDtm, IElementoDtm
    {
        public DateTime FechaCreacion { get; set; }
        public int IdUsuaCrea { get; set; }
        public virtual UsuarioDtm UsuarioCreador { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public int? IdUsuaModi { get; set; }
        public virtual UsuarioDtm UsuarioModificador { get; set; }
        public override string Expresion => $"{(ApiDeInterfaceDtm.ImplementaUsaReferencia(GetType()) ? $"({nameof(IUsaReferencia.Referencia)}) " : "")}{base.Expresion}";

        [Timestamp]
        public byte[] RowVersion { get; set; }
    }


    public abstract class ElementoConCgDtm : ElementoDtm, IUsaTipoConCG, IUsaTraza
    {
        public int IdCg { get; set; }
        public int IdTipo { get; set; }
        public string Descripcion { get; set; }
        public string Referencia { get; set; }
        public TipoDeElementoDtm Tipo { get; set; }
        public CentroGestorDtm Cg { get; set; }
        public override string Expresion => $"({Referencia}) {Nombre}";
        ITipoDtm IUsaTipo.Tipo => Tipo;
    }


    public abstract class ElementoDeProcesoDtm : ElementoConCgDtm, IElementoDeProcesoDtm
    {
        public int IdEstado { get; set; }
        public EstadoDtm Estado { get; set; }
        IEstado IUsaEstado.Estado => Estado;
    }
    public static class ApiDeElementoDtm
    {
        public static string TablaDeTipo(Type t) => $"{ApiDeRegistroDtm.EsquemaDeTabla(t)}.{ApiDeRegistroDtm.NombreDeTabla(t)}_{Sufijo.TIPO}";
        public static string TablaDeAuditoria(Type t) => $"{ApiDeRegistroDtm.EsquemaDeTabla(t)}.{ApiDeRegistroDtm.NombreDeTabla(t)}_{Sufijo.AUDITORIA}";
        public static string TablaDeObservacion(Type t) => $"{ApiDeRegistroDtm.EsquemaDeTabla(t)}.{ApiDeRegistroDtm.NombreDeTabla(t)}_{Sufijo.OBSERVACION}";
        public static string TablaDeArchivosVinculados(Type t) => $"{ApiDeRegistroDtm.EsquemaDeTabla(t)}.{ApiDeRegistroDtm.NombreDeTabla(t)}_{Sufijo.ARCHIVO}";
        public static string TablaDePermisos(Type t) => $"{ApiDeRegistroDtm.EsquemaDeTabla(t)}.{ApiDeRegistroDtm.NombreDeTabla(t)}_{Sufijo.PERMISO}";
        public static string TablaDeDireccion(Type t) => $"{ApiDeRegistroDtm.EsquemaDeTabla(t)}.{ApiDeRegistroDtm.NombreDeTabla(t)}_{Sufijo.DIRECCION}";
        public static string TablaDeTrazas(Type t) => $"{ApiDeRegistroDtm.EsquemaDeTabla(t)}.{ApiDeRegistroDtm.NombreDeTabla(t)}_{Sufijo.TRAZA}";
        public static string TablaDeArchivadoresVinculados(Type t) => $"{ApiDeRegistroDtm.EsquemaDeTabla(t)}.{ApiDeRegistroDtm.NombreDeTabla(t)}_{Sufijo.ARCHIVADOR}";
        public static string TablaDeEventosVinculados(Type t) => $"{ApiDeRegistroDtm.EsquemaDeTabla(t)}.{ApiDeRegistroDtm.NombreDeTabla(t)}_{Sufijo.AGENDA_EVENTO}";
        public static string TablaDeHistoria(Type t) => $"{ApiDeRegistroDtm.EsquemaDeTabla(t)}.{ApiDeRegistroDtm.NombreDeTabla(t)}_{Sufijo.HISTORIA}";
        public static string TablaDeInterlocutoresVinculados(Type t) => $"{ApiDeRegistroDtm.EsquemaDeTabla(t)}.{ApiDeRegistroDtm.NombreDeTabla(t)}_{Sufijo.INTERLOCUTOR}";
        public static string TablaDeTareasVinculadas(Type t) => $"{ApiDeRegistroDtm.EsquemaDeTabla(t)}.{ApiDeRegistroDtm.NombreDeTabla(t)}_{Sufijo.TAREA}";
        public static string TablaDeRegistrosEsVinculados(Type t) => $"{ApiDeRegistroDtm.EsquemaDeTabla(t)}.{ApiDeRegistroDtm.NombreDeTabla(t)}_{Sufijo.REGISTRO}";
        public static string TablaDeEstados(Type t) => $"{ApiDeRegistroDtm.EsquemaDeTabla(t)}.{ApiDeRegistroDtm.NombreDeTabla(t)}_{Sufijo.ESTADO}";
        public static string TablaDeTransiciones(Type t) => $"{ApiDeRegistroDtm.EsquemaDeTabla(t)}.{ApiDeRegistroDtm.NombreDeTabla(t)}_{Sufijo.TRANSICION}";
        public static string TablaDeAcciones(Type t) => $"{ApiDeRegistroDtm.EsquemaDeTabla(t)}.{ApiDeRegistroDtm.NombreDeTabla(t)}_{Sufijo.ACCION}";
        public static string TablaDeCertificados(Type t) => $"{ApiDeRegistroDtm.EsquemaDeTabla(t)}.{ApiDeRegistroDtm.NombreDeTabla(t)}_{Sufijo.CERTIFICADO}";
        public static string TablaDePagos(Type t) => $"{ApiDeRegistroDtm.EsquemaDeTabla(t)}.{ApiDeRegistroDtm.NombreDeTabla(t)}_{Tablas.PAGO}";
        public static string TablaDeExpedintes(Type t) => $"{ApiDeRegistroDtm.EsquemaDeTabla(t)}.{ApiDeRegistroDtm.NombreDeTabla(t)}_{Tablas.EXPEDIENTE}";
        public static string TablaDeCircuitosDoc(Type t) => $"{ApiDeRegistroDtm.EsquemaDeTabla(t)}.{ApiDeRegistroDtm.NombreDeTabla(t)}_{Tablas.CIRCUITO_DOC}";

        internal static void DefinirCampoAgenda<TEntity>(ModelBuilder modelBuilder, bool obligatorio = false, bool unico = false) where TEntity : RegistroConNombreDtm
        {
            modelBuilder.Entity<TEntity>().Property(nameof(IPuedeUsarAgenda.IdAgenda)).HasColumnName(ICampos.ID_AGENDA).HasColumnType(IDominio.INT).IsRequired(obligatorio);
            ApiDeRegistroDtm.DefinirFk<TEntity>(modelBuilder, nameof(IPuedeUsarAgenda.Agenda), nameof(IPuedeUsarAgenda.IdAgenda), ICampos.ID_AGENDA, unico: unico);
        }

        internal static void DefinirCampoArchivo<TEntity>(ModelBuilder modelBuilder, bool obligatorio = false, bool unico = false)
        where TEntity : RegistroDtm
        {
            modelBuilder.Entity<TEntity>().Property(nameof(IUsaArchivo.IdArchivo)).HasColumnName(ICampos.ID_ARCHIVO).HasColumnType(IDominio.INT).IsRequired(obligatorio);
            ApiDeRegistroDtm.DefinirFk<TEntity>(modelBuilder, nameof(IUsaArchivo.Archivo), nameof(IUsaArchivo.IdArchivo), ICampos.ID_ARCHIVO, unico: unico);
        }

        internal static void DefinirCamposDelElementoDtm<TEntity>(ModelBuilder modelBuilder, bool indiceUnicoPorNombre = false) where TEntity : ElementoDtm
        {
            ApiDeRegistroDtm.DefinirCampoIdDtm<TEntity>(modelBuilder);
            ApiDeNombreDtm.DefinirCampoNombreDtm<TEntity>(modelBuilder, unico: indiceUnicoPorNombre);

            DefinirCamposDeAuditoria<TEntity>(modelBuilder);

            if (typeof(TEntity).ImplementaUsaBaja()) DefinirCampoBaja<TEntity>(modelBuilder);
            if (typeof(TEntity).ImplementaUsaBloqueo()) DefinirCampoBloqueo<TEntity>(modelBuilder);
            if (typeof(TEntity).ImplementaUsaDescripcion()) DefinirCampoDescripcion<TEntity>(modelBuilder);
            if (typeof(TEntity).ImplementaUsaReferencia()) DefinirCampoReferencia<TEntity>(modelBuilder);
            if (typeof(TEntity).ImplementaUsaClaseDeElemento()) DefinirCampoIdClase<TEntity>(modelBuilder);


            if (typeof(TEntity).ImplementaUsaPreasiento()) DefinirPreasiento<TEntity>(modelBuilder, obligatorio: false, unico: true);
        }

        private static void DefinirCampoIdClase<T>(ModelBuilder modelBuilder) where T : ElementoDtm
        {

            ApiDeElementoDtm.DefinirDependenciaDe<T>(modelBuilder, nameof(IUsaCalseDeElemento.ClaseDeElemento), nameof(IUsaCalseDeElemento.IdClaseDeElemento), ICampos.ID_CLASE_ELEMENTO, requerido: false, unico: false);
        }

        internal static void DefinirCamposDeAuditoria<TEntity>(ModelBuilder modelBuilder) where TEntity : RegistroDtm
        {
            if (!typeof(TEntity).ImplementaAuditoria())
                GestorDeErrores.Emitir($"La entidad {typeof(TEntity).Name} debe implementar {nameof(IAuditoria)}");

            modelBuilder.Entity<TEntity>().Property(p => ((IAuditoria)p).FechaCreacion).HasColumnName(ICampos.FECCRE).HasColumnType(IDominio.DATETIME_2).IsRequired();
            modelBuilder.Entity<TEntity>().Property(p => ((IAuditoria)p).IdUsuaCrea).HasColumnName(ICampos.ID_CREADOR).HasColumnType(IDominio.INT).IsRequired();
            ApiDeRegistroDtm.DefinirFk<TEntity>(modelBuilder, nameof(ElementoDtm.UsuarioCreador), nameof(ElementoDtm.IdUsuaCrea), ICampos.ID_CREADOR, unico: false);

            modelBuilder.Entity<TEntity>().Property(p => ((IAuditoria)p).FechaModificacion).HasColumnName(ICampos.FECMOD).HasColumnType(IDominio.DATETIME_2).IsRequired(false);
            modelBuilder.Entity<TEntity>().Property(p => ((IAuditoria)p).IdUsuaModi).HasColumnName(ICampos.ID_MODIFICADOR).HasColumnType(IDominio.INT).IsRequired(false);
            ApiDeRegistroDtm.DefinirFk<TEntity>(modelBuilder, nameof(ElementoDtm.UsuarioModificador), nameof(ElementoDtm.IdUsuaModi), ICampos.ID_MODIFICADOR, unico: false);

            modelBuilder.Entity<TEntity>().Property(e => ((IAuditoria)e).RowVersion).IsRowVersion();
        }

        internal static void DefinirCampoBaja<TEntity>(ModelBuilder modelBuilder) where TEntity : ElementoDtm
        {
            ApiDeRegistroDtm.DefinirCampoBaja<TEntity>(modelBuilder);
        }

        internal static void DefinirCampoBloqueo<TEntity>(ModelBuilder modelBuilder) where TEntity : ElementoDtm
        {
            modelBuilder.Entity<TEntity>().Property(nameof(IUsaBloqueo.Bloqueado))
                   .HasColumnName(ICampos.BLOQUEO)
                   .HasColumnType(IDominio.BIT).IsRequired(true)
                   .HasDefaultValue(false);
        }

        internal static void DefinirCampoDescripcion<TEntity>(ModelBuilder modelBuilder) where TEntity : RegistroConNombreDtm
        {
            ApiDeNombreDtm.DefinirCampoDescripcion<TEntity>(modelBuilder);
        }

        internal static void DefinirCampoReferencia<TEntity>(ModelBuilder modelBuilder) where TEntity : ElementoDtm
        {
            var nombreDeTabla = ApiDeRegistroDtm.NombreDeTabla(typeof(TEntity));

            modelBuilder.Entity<TEntity>().Property(nameof(IUsaReferencia.Referencia))
                .HasColumnName(ICampos.REFERENCIA)
                .HasColumnType(IDominio.VARCHAR)
                .HasMaxLength(20)
                .IsRequired(true);

            modelBuilder.Entity<TEntity>().HasIndex(nameof(IUsaReferencia.Referencia))
                .HasDatabaseName($"I_{nombreDeTabla}_{ICampos.REFERENCIA}")
                .HasFilter(null)
                .IsUnique(true);
        }

        internal static void DefinirCampoTipo<TEntity>(ModelBuilder modelBuilder, string propiedad) where TEntity : RegistroDtm
        {
            var nombreDeTabla = ApiDeRegistroDtm.NombreDeTabla(typeof(TEntity));

            modelBuilder.Entity<TEntity>().Property(nameof(IUsaTipo.IdTipo)).HasColumnName(ICampos.ID_TIPO);
            modelBuilder.Entity<TEntity>().Property(nameof(IUsaTipo.IdTipo)).HasColumnType(IDominio.INT);
            modelBuilder.Entity<TEntity>().Property(nameof(IUsaTipo.IdTipo)).IsRequired(true);

            modelBuilder.Entity<TEntity>()
                        .HasIndex(nameof(IUsaTipo.IdTipo))
                        .HasDatabaseName($"I_{nombreDeTabla}_{ICampos.ID_TIPO}");

            modelBuilder.Entity<TEntity>()
                        .HasOne(propiedad)
                        .WithMany()
                        .HasForeignKey(nameof(IUsaTipo.IdTipo))
                        .HasConstraintName($"FK_{nombreDeTabla}_{ICampos.ID_TIPO}")
                        .OnDelete(DeleteBehavior.Restrict);
        }

        internal static void DefinirCampoClase<TEntity>(ModelBuilder modelBuilder, string propiedad) where TEntity : RegistroDtm
        {
            var nombreDeTabla = ApiDeRegistroDtm.NombreDeTabla(typeof(TEntity));

            modelBuilder.Entity<TEntity>().Property(nameof(IUsaClase.IdClase)).HasColumnName(ICampos.ID_CLASE);
            modelBuilder.Entity<TEntity>().Property(nameof(IUsaClase.IdClase)).HasColumnType(IDominio.INT);
            modelBuilder.Entity<TEntity>().Property(nameof(IUsaClase.IdClase)).IsRequired(true);

            modelBuilder.Entity<TEntity>()
                        .HasIndex(nameof(IUsaClase.IdClase))
                        .HasDatabaseName($"I_{nombreDeTabla}_{ICampos.ID_CLASE}");

            modelBuilder.Entity<TEntity>()
                        .HasOne(propiedad)
                        .WithMany()
                        .HasForeignKey(nameof(IUsaClase.IdClase))
                        .HasConstraintName($"FK_{nombreDeTabla}_{ICampos.ID_CLASE}")
                        .OnDelete(DeleteBehavior.Restrict);
        }


        internal static void DefinirCampoEstado<TEntity>(ModelBuilder modelBuilder, string propiedad) where TEntity : ElementoDtm
        {
            var nombreDeTabla = ApiDeRegistroDtm.NombreDeTabla(typeof(TEntity));

            modelBuilder.Entity<TEntity>().Property(nameof(IUsaEstado.IdEstado)).HasColumnName(ICampos.ID_ESTADO)
                .HasColumnType(IDominio.INT)
                .IsRequired(true);

            modelBuilder.Entity<TEntity>()
                        .HasIndex(nameof(IUsaEstado.IdEstado))
                        .HasDatabaseName($"I_{nombreDeTabla}_{ICampos.ID_ESTADO}");

            modelBuilder.Entity<TEntity>()
                        .HasOne(propiedad)
                        .WithMany()
                        .HasForeignKey(nameof(IUsaEstado.IdEstado))
                        .HasConstraintName($"FK_{nombreDeTabla}_{ICampos.ID_ESTADO}")
                        .OnDelete(DeleteBehavior.Restrict);
        }


        internal static void DefinirDependenciaDe<TEntity>(ModelBuilder modelBuilder, string dependeDe, string apuntadoPor, string idCampo
            , bool requerido = true
            , bool unico = false) where TEntity : ElementoDtm
        {
            ApiDeRegistroDtm.DefinirDependencia<TEntity>(modelBuilder, dependeDe, apuntadoPor, idCampo, requerido, unico);
        }

        internal static void DefinirCampoCg<TEntity>(ModelBuilder modelBuilder, string propiedad) where TEntity : RegistroDtm
        {
            var nombreDeTabla = ApiDeRegistroDtm.NombreDeTabla(typeof(TEntity));

            modelBuilder.Entity<TEntity>().Property(nameof(IUsaCg.IdCg)).HasColumnName(ICampos.ID_CG);
            modelBuilder.Entity<TEntity>().Property(nameof(IUsaCg.IdCg)).HasColumnType(IDominio.INT);
            modelBuilder.Entity<TEntity>().Property(nameof(IUsaCg.IdCg)).IsRequired(true);

            modelBuilder.Entity<TEntity>()
                        .HasIndex(nameof(IUsaCg.IdCg))
                        .HasDatabaseName($"I_{nombreDeTabla}_{ICampos.ID_CG}");

            modelBuilder.Entity<TEntity>()
                        .HasOne(propiedad)
                        .WithMany()
                        .HasForeignKey(nameof(IUsaCg.IdCg))
                        .HasConstraintName($"FK_{nombreDeTabla}_{ICampos.ID_CG}")
                        .OnDelete(DeleteBehavior.Restrict);
        }

        internal static void DefinirCamposDeDetalle<TEntity>(ModelBuilder modelBuilder) where TEntity : RegistroDtm, IDetalle
        {
            modelBuilder.Entity<TEntity>().Ignore(x => x.Negocio);
            modelBuilder.Entity<TEntity>().Ignore(x => x.Elemento);

            ApiDeRegistroDtm.DefinirCampoFk<TEntity>(modelBuilder, nameof(IDetalle.Elemento), nameof(IDetalle.IdElemento), ICampos.ID_ELEMENTO, requerida: true, unico: false);
        }


        internal static void DefinirCampoIdElemento<TEntity>(ModelBuilder modelBuilder, bool unico = false) where TEntity : RegistroDtm
        {
            var nombreDeTabla = ApiDeRegistroDtm.NombreDeTabla(typeof(TEntity));

            modelBuilder.Entity<TEntity>().Property(nameof(IUsaElemento.IdElemento)).HasColumnName(ICampos.ID_ELEMENTO).HasColumnType(IDominio.INT).IsRequired(true);

            modelBuilder.Entity<TEntity>()
                        .HasIndex(nameof(IUsaElemento.IdElemento))
                        .HasDatabaseName($"I_{nombreDeTabla}_{ICampos.ID_ELEMENTO}")
                        .IsUnique(unico);
        }

        internal static void DefinirSolicitante<TEntity>(ModelBuilder modelBuilder) where TEntity : ElementoDtm
        {
            if (!typeof(TEntity).ImplementaUsaSolicitante())
                throw new Exception($"La entidad {typeof(TEntity).Name} que está intentando definir no implementa {nameof(IUsaSolicitante)}");

            modelBuilder.Entity<TEntity>().Ignore(x => ((IUsaSolicitante)x).Solicitante);
            DefinirDependenciaDe<TEntity>(modelBuilder, nameof(IUsaSolicitante.Solicitante), nameof(IUsaSolicitante.IdSolicitante), ICampos.ID_SOLICITANTE);

            modelBuilder.Entity<TEntity>().Property(p => ((IUsaSolicitante)p).Contacto).HasColumnName(ICampos.CONTACTO).HasColumnType(IDominio.VARCHAR_250).IsRequired();

            ApiDeRegistroDtm.DefinirDatosDeContacto<TEntity>(modelBuilder);
        }

        internal static void DefinirCliente<TEntity>(ModelBuilder modelBuilder) where TEntity : ElementoDtm
        =>
        ApiDeRegistroDtm.DefinirCliente<TEntity>(modelBuilder);

        internal static void DefinirProveedor<TEntity>(ModelBuilder modelBuilder) where TEntity : ElementoDtm
        =>
        ApiDeRegistroDtm.DefinirProveedor<TEntity>(modelBuilder);

        internal static void DefinirTrabajador<TEntity>(ModelBuilder modelBuilder) where TEntity : ElementoDtm
        =>
        ApiDeRegistroDtm.DefinirTrabajador<TEntity>(modelBuilder);

        internal static void DefinirPreasiento<TEntity>(ModelBuilder modelBuilder, bool obligatorio, bool unico) 
        where TEntity : RegistroDtm
        {
            if (!typeof(TEntity).ImplementaUsaPreasiento())
                throw new Exception($"La entidad {typeof(TEntity).Name} que está intentando definir no implementa {nameof(IUsaPreasiento)}");

            modelBuilder.Entity<TEntity>().Property(nameof(IUsaPreasiento.IdPreasiento)).HasColumnName(ICampos.ID_PREASIENTO).HasColumnType(IDominio.INT).IsRequired(obligatorio);
            ApiDeRegistroDtm.DefinirFk<TEntity>(modelBuilder, nameof(IUsaPreasiento.Preasiento), nameof(IUsaPreasiento.IdPreasiento), ICampos.ID_PREASIENTO, unico: unico);
        }
    }

}
