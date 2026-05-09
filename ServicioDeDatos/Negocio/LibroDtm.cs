using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;
using Gestor.Errores;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using Utilidades;

namespace ServicioDeDatos.Negocio
{
    public enum enumOpercionesDeSemaforo
    {
        [Description("Persistir cobro de la factura: {Referencia}")]
        COB,
        [Description("Persistir abono de la factura: {Referencia}")]
        ABO,
        [Description("Generar planificador del ctr: {Referencia}")]
        GPFV,
        [Description("Importar Zip: {Referencia}")]
        IZIP,
        [Description("Procesar Far: {Referencia}")]
        IPFA,
        [Description("Exportar archivador: {Referencia}")]
        EARC,
        [Description("Operando con la AEAT: (Referencia)")]
        AEAT,
        [Description("Asociar auditoría Sii: {Referencia}")]
        ASII,
        [Description("Activando verifactu en la sociedad: {Referencia}")]
        VERI,
        [Description("Ejecución de la cola")]
        EJEC,
        [Description("Anular lote contable")]
        ALCT,
        [Description("Anular estimación directa")]
        AETD,
    }

    [Table(Tablas.LIBRO, Schema = Esquemas.NEGOCIO)]
    public class LibroDtm : RegistroDtm
    {
        public int Ejercicio { get; set; }
        public int IdNegocio { get; set; }
        public int IdCg { get; set; }
        public int IdTipo { get; set; }
        public int Valor { get; set; }
    }

    [Table(Tablas.LIBRO_SEMAFORO, Schema = Esquemas.NEGOCIO)]
    public class SemaforoDeUnLibroDtm : RegistroDtm
    {
        public int IdLibro { get; set; }
        public int IdUsuario { get; set; }
        public DateTime Fecha { get; set; }
    }


    [Table(Tablas.SEMAFORO_PROCESO, Schema = Esquemas.NEGOCIO)]
    public class SemaforoDeProcesoDtm : RegistroDtm
    {
        public int IdNegocio { get; set; }
        public int IdElemento { get; set; }
        public Guid IdProceso { get; set; }
        public string Operacion { get; set; }
        public string Login { get; set; }
        public string Anotacion { get; set; }
        public DateTime Fecha { get; set; }
    }

    public class LibroSql
    {
        private static string _crear = $@"
        insert into {ApiDeRegistroDtm.EsquemaTabla(typeof(LibroDtm))} ({ICampos.EJERCICIO}, {ICampos.ID_NEGOCIO}, {ICampos.ID_CG}, {ICampos.ID_TIPO}, {ICampos.VALOR}) 
        values(@{ICampos.EJERCICIO}, @{ICampos.ID_NEGOCIO}, @{ICampos.ID_CG}, @{ICampos.ID_TIPO}, 0)
        ";

        private static string _incrementar = $@"
        update {ApiDeRegistroDtm.EsquemaTabla(typeof(LibroDtm))} set {ICampos.VALOR} = {ICampos.VALOR} + 1 
        where {ICampos.ID} = @{ICampos.ID}
        ";

        private static string _resetear = $@"
        update {ApiDeRegistroDtm.EsquemaTabla(typeof(LibroDtm))} set {ICampos.VALOR} = @{ICampos.VALOR}  
        where {ICampos.ID} = @{ICampos.ID}
        ";

        private static string _leerPorAk = $@"
        select {ICampos.ID} as {nameof(LibroDtm.Id)}
             , {ICampos.EJERCICIO} as {nameof(LibroDtm.Ejercicio)}
             , {ICampos.ID_NEGOCIO} as {nameof(LibroDtm.IdNegocio)}
             , {ICampos.ID_CG} as {nameof(LibroDtm.IdCg)} 
             , {ICampos.ID_TIPO} as {nameof(LibroDtm.IdTipo)} 
             , {ICampos.VALOR} as {nameof(LibroDtm.Valor)} 
        from {ApiDeRegistroDtm.EsquemaTabla(typeof(LibroDtm))} WITH (NOLOCK)
        Where {ICampos.EJERCICIO} = @{ICampos.EJERCICIO}
          and {ICampos.ID_NEGOCIO} =  @{ICampos.ID_NEGOCIO}
          and {ICampos.ID_CG} = @{ICampos.ID_CG}
          and {ICampos.ID_TIPO} = @{ICampos.ID_TIPO}
        ";

        private static string _bloquear = $@"
        insert into {ApiDeRegistroDtm.EsquemaTabla(typeof(SemaforoDeUnLibroDtm))} ({ICampos.ID_LIBRO}, {ICampos.ID_USUARIO}, {ICampos.FECHA}) 
        values(@{ICampos.ID_LIBRO}, @{ICampos.ID_USUARIO}, @{ICampos.FECHA})
        ";

        private static string _desbloquear = $@"
        delete 
        from  {ApiDeRegistroDtm.EsquemaTabla(typeof(SemaforoDeUnLibroDtm))} 
        where {ICampos.ID_LIBRO} = @{ICampos.ID_LIBRO}
        ";

        private static int Crear(ContextoSe contexto, int ejercicio, int idNegocio, int idCg, int idTipo)
        {
            var parametrosSql = new Dictionary<string, object>
            {
                { $"@{ICampos.EJERCICIO}", ejercicio },
                { $"@{ICampos.ID_NEGOCIO}", idNegocio },
                { $"@{ICampos.ID_TIPO}", idTipo },
                { $"@{ICampos.ID_CG}", idCg }
            };

            var sentencia = new ConsultaSql<LibroDtm>(contexto, _crear);
            return sentencia.EjecutarSentencia(new DynamicParameters(parametrosSql));
        }

        public static int Incrementar(ContextoSe contexto, int idLibro)
        {
            var parametrosSql = new Dictionary<string, object>
            {
                { $"@{ICampos.ID}", idLibro }
            };

            var sentencia = new ConsultaSql<LibroDtm>(contexto, _incrementar);
            return sentencia.EjecutarSentencia(new DynamicParameters(parametrosSql));
        }

        public static int Resetear(ContextoSe contexto, int idLibro, int valor)
        {
            var parametrosSql = new Dictionary<string, object>
            {
                { $"@{ICampos.ID}", idLibro },
                { $"@{ICampos.VALOR}", valor }
            };

            var sentencia = new ConsultaSql<LibroDtm>(contexto, _resetear);
            return sentencia.EjecutarSentencia(new DynamicParameters(parametrosSql));
        }

        private static List<LibroDtm> LeerPorAk(ContextoSe contexto, int ejercicio, int idNegocio, int idCg, int idTipo)
        {
            var parametrosSql = new Dictionary<string, object>
            {
                { $"@{ICampos.EJERCICIO}", ejercicio },
                { $"@{ICampos.ID_NEGOCIO}", idNegocio },
                { $"@{ICampos.ID_TIPO}", idTipo },
                { $"@{ICampos.ID_CG}", idCg }
            };

            var consulta = new ConsultaSql<LibroDtm>(contexto, _leerPorAk);
            return consulta.LanzarConsulta(new DynamicParameters(parametrosSql));
        }

        public static LibroDtm CrearObtener(ContextoSe contexto, int ejercicio, int idNegocio, int idCg, int idTipo)
        {
            var libros = LeerPorAk(contexto, ejercicio, idNegocio, idCg, idTipo);
            if (libros.Count == 0)
            {
                Crear(contexto, ejercicio, idNegocio, idCg, idTipo);
                libros = LeerPorAk(contexto, ejercicio, idNegocio, idCg, idTipo);
            }
            return libros[0];
        }

        public static int Bloquear(int idLibro, int idUsuario)
        {
            var parametrosSql = new Dictionary<string, object>
            {
                { $"@{ICampos.ID_LIBRO}", idLibro },
                { $"@{ICampos.ID_USUARIO}", idUsuario },
                { $"@{ICampos.FECHA}", DateTime.Now }
            };

            var sentencia = new ConsultaSql<LibroDtm>(_bloquear);
            return sentencia.EjecutarSentencia(new DynamicParameters(parametrosSql));
        }

        public static int Desbloquear(int idLibro)
        {
            var parametrosSql = new Dictionary<string, object>
            {
                { $"@{ICampos.ID_LIBRO}", idLibro }
            };

            var sentencia = new ConsultaSql<LibroDtm>(_desbloquear);
            return sentencia.EjecutarSentencia(new DynamicParameters(parametrosSql));
        }
    }

    public class ltrSemaforo
    {
        public const string IdSemaforo = nameof(IdSemaforo);
    }

    public class SemaforoDeProcesoSql
    {
        public static int SemaforosColocados(ContextoSe contexto, int idNegocio)
        {
            var consulta = $@"select count(*) as Cantidad 
                              from {ApiDeRegistroDtm.EsquemaTabla(typeof(SemaforoDeProcesoDtm))} 
                              with(nolock)
                              where {ICampos.ID_NEGOCIO} = @{ICampos.ID_NEGOCIO}";

            var parametrosSql = new Dictionary<string, object>
            {
                { $"@{ICampos.ID_NEGOCIO}", idNegocio }
            };

            var sql = new ConsultaSql<RegistrosAfectados>(contexto, consulta);
            var registros = sql.LanzarConsulta(new DynamicParameters(parametrosSql));
            return registros[0].cantidad;
        }

        private static bool HaySemaforo(ContextoSe contexto, int idNegocio, int idElemento, Guid idProceso)
        {
            var consulta = $@"select count(*) as Cantidad 
                              from {ApiDeRegistroDtm.EsquemaTabla(typeof(SemaforoDeProcesoDtm))} 
                              with(nolock)
                              where {ICampos.ID_NEGOCIO} = @{ICampos.ID_NEGOCIO}
                              and {ICampos.ID_ELEMENTO} = @{ICampos.ID_ELEMENTO}
                              and {ICampos.ID_PROCESO} = @{ICampos.ID_PROCESO}";

            var parametrosSql = new Dictionary<string, object>
            {
                { $"@{ICampos.ID_NEGOCIO}", idNegocio },
                { $"@{ICampos.ID_ELEMENTO}", idElemento },
                { $"@{ICampos.ID_PROCESO}", idProceso }
            };

            var sql = new ConsultaSql<RegistrosAfectados>(contexto, consulta);
            var registros = sql.LanzarConsulta(new DynamicParameters(parametrosSql));
            return registros[0].cantidad > 0;
        }

        public static bool HaySemaforoPara(ContextoSe contexto, int idNegocio, int idElemento, List<enumOpercionesDeSemaforo> operaciones)
        {
            var cadena = string.Join(",", operaciones.Select(op => $"'{op}'"));
            var consulta = $@"select Count(*) as cantidad
                              from {ApiDeRegistroDtm.EsquemaTabla(typeof(SemaforoDeProcesoDtm))} 
                              with(nolock)
                              where {ICampos.ID_NEGOCIO} = @{ICampos.ID_NEGOCIO}
                              and {ICampos.ID_ELEMENTO} = @{ICampos.ID_ELEMENTO}
                              and {ICampos.OPERACION} IN ({cadena})";

            var parametrosSql = new Dictionary<string, object>
            {
                { $"@{ICampos.ID_NEGOCIO}", idNegocio },
                { $"@{ICampos.ID_ELEMENTO}", idElemento }
            };

            var sql = new ConsultaSql<RegistrosAfectados>(consulta);
            var registros = sql.LanzarConsulta(new DynamicParameters(parametrosSql));
            return registros[0].cantidad > 0;
        }

        public static List<enumOpercionesDeSemaforo> ObtenerOperaciones(ContextoSe contexto, int idNegocio, int idElemento)
        {
            var registros = LeerSemaforos(contexto, idNegocio, idElemento);
            var operaciones = new List<enumOpercionesDeSemaforo>();
            foreach (var registro in registros)
            {
                operaciones.Add(ApiDeEnsamblados.ToEnumerado<enumOpercionesDeSemaforo>(registro.Operacion));
            }

            return operaciones;
        }

        public static int SemaforoPara(ContextoSe contexto, int idNegocio, int idElemento, List<enumOpercionesDeSemaforo> operaciones)
        {
            var cadena = string.Join(",", operaciones.Select(op => $"'{op}'"));
            var consulta = $@"select {ICampos.ID}
                              from {ApiDeRegistroDtm.EsquemaTabla(typeof(SemaforoDeProcesoDtm))} 
                              with(nolock)
                              where {ICampos.ID_NEGOCIO} = @{ICampos.ID_NEGOCIO}
                              and {ICampos.ID_ELEMENTO} = @{ICampos.ID_ELEMENTO}
                              and {ICampos.OPERACION} IN ({cadena})";

            var parametrosSql = new Dictionary<string, object>
            {
                { $"@{ICampos.ID_NEGOCIO}", idNegocio },
                { $"@{ICampos.ID_ELEMENTO}", idElemento }
            };

            var sql = new ConsultaSql<RegistrosAfectados>(consulta);
            var registros = sql.LanzarConsulta(new DynamicParameters(parametrosSql));
            return registros[0].cantidad;
        }

        private static List<SemaforoDeProcesoDtm> LeerSemaforos(ContextoSe contexto, int idNegocio, int idElemento)
        {
            var consulta = $@"select {ICampos.ID} as {nameof(SemaforoDeProcesoDtm.Id)}
                              , {ICampos.ID_NEGOCIO} as {nameof(SemaforoDeProcesoDtm.IdNegocio)}
                              , {ICampos.ID_ELEMENTO} as {nameof(SemaforoDeProcesoDtm.IdElemento)}
                              , {ICampos.ID_PROCESO} as {nameof(SemaforoDeProcesoDtm.IdProceso)} 
                              , {ICampos.OPERACION} as {nameof(SemaforoDeProcesoDtm.Operacion)}  
                              , {ICampos.LOGIN} as {nameof(SemaforoDeProcesoDtm.Login)}
                              , {ICampos.ANOTACION} as {nameof(SemaforoDeProcesoDtm.Anotacion)} 
                              , {ICampos.FECHA} as {nameof(SemaforoDeProcesoDtm.Fecha)} 
                              from {ApiDeRegistroDtm.EsquemaTabla(typeof(SemaforoDeProcesoDtm))} 
                              with(nolock)
                              where {ICampos.ID_NEGOCIO} = @{ICampos.ID_NEGOCIO}
                              and {ICampos.ID_ELEMENTO} = @{ICampos.ID_ELEMENTO}";

            var parametrosSql = new Dictionary<string, object>
            {
                { $"@{ICampos.ID_NEGOCIO}", idNegocio },
                { $"@{ICampos.ID_ELEMENTO}", idElemento }
            };

            var sql = new ConsultaSql<SemaforoDeProcesoDtm>(consulta);
            var registros = sql.LanzarConsulta(new DynamicParameters(parametrosSql));
            return registros;
        }

        public static List<SemaforoDeProcesoDtm> LeerSemaforos(ContextoSe contexto, int idNegocio, enumOpercionesDeSemaforo operacion)
        {
            var consulta = $@"select {ICampos.ID} as {nameof(SemaforoDeProcesoDtm.Id)}
                              , {ICampos.ID_NEGOCIO} as {nameof(SemaforoDeProcesoDtm.IdNegocio)}
                              , {ICampos.ID_ELEMENTO} as {nameof(SemaforoDeProcesoDtm.IdElemento)}
                              , {ICampos.ID_PROCESO} as {nameof(SemaforoDeProcesoDtm.IdProceso)} 
                              , {ICampos.OPERACION} as {nameof(SemaforoDeProcesoDtm.Operacion)}  
                              , {ICampos.LOGIN} as {nameof(SemaforoDeProcesoDtm.Login)}
                              , {ICampos.ANOTACION} as {nameof(SemaforoDeProcesoDtm.Anotacion)} 
                              , {ICampos.FECHA} as {nameof(SemaforoDeProcesoDtm.Fecha)} 
                              from {ApiDeRegistroDtm.EsquemaTabla(typeof(SemaforoDeProcesoDtm))} 
                              with(nolock)
                              where {ICampos.ID_NEGOCIO} = @{ICampos.ID_NEGOCIO}
                              and {ICampos.OPERACION} = @{ICampos.OPERACION}";

            var parametrosSql = new Dictionary<string, object>
            {
                { $"@{ICampos.ID_NEGOCIO}", idNegocio },
                { $"@{ICampos.OPERACION}", operacion.ToString() }
            };

            var sql = new ConsultaSql<SemaforoDeProcesoDtm>(consulta);
            var registros = sql.LanzarConsulta(new DynamicParameters(parametrosSql));
            return registros;
        }

        public static RegistroDtm PonerSemaforo(ContextoSe contexto, int idNegocio, int idElemento, enumOpercionesDeSemaforo operacion, string referencia)
        =>
        PonerSemaforo(contexto, idNegocio, idElemento, operacion.ToString(), operacion.Descripcion().Replace("{Referencia}", referencia));

        public static RegistroDtm PonerSemaforo(ContextoSe contexto, int idNegocio, int idElemento, string operacion, string anotacion)
        {
            if (HaySemaforo(contexto, idNegocio, idElemento, (Guid)contexto.IdOperacion))
                return new RegistroDtm { Id = 0 };

            var sentencia = $@"Insert into {ApiDeRegistroDtm.EsquemaTabla(typeof(SemaforoDeProcesoDtm))} (  
                                         {ICampos.ID_NEGOCIO} 
                                       , {ICampos.ID_ELEMENTO} 
                                       , {ICampos.ID_PROCESO} 
                                       , {ICampos.OPERACION}     
                                       , {ICampos.LOGIN}
                                       , {ICampos.ANOTACION} 
                                       , {ICampos.FECHA} 
                                       )
                               values (        
                                         @{ICampos.ID_NEGOCIO} 
                                       , @{ICampos.ID_ELEMENTO}
                                       , @{ICampos.ID_PROCESO}  
                                       , @{ICampos.OPERACION}    
                                       , @{ICampos.LOGIN}
                                       , @{ICampos.ANOTACION} 
                                       , @{ICampos.FECHA} 
                                      )
                               SELECT SCOPE_IDENTITY() as {nameof(RegistroDtm.Id)}
             ";

            var parametrosSql = new Dictionary<string, object>
            {
                { $"@{ICampos.ID_NEGOCIO}", idNegocio },
                { $"@{ICampos.ID_ELEMENTO}", idElemento },
                { $"@{ICampos.ID_PROCESO}", contexto.IdOperacion},
                { $"@{ICampos.OPERACION}", operacion },
                { $"@{ICampos.LOGIN}", contexto.DatosDeConexion.Login },
                { $"@{ICampos.ANOTACION}", anotacion },
                { $"@{ICampos.FECHA}", DateTime.Now }
            };

            var consulta = new ConsultaSql<RegistroDtm>(sentencia);

            try
            {
                var registro = consulta.LanzarConsulta(new DynamicParameters(parametrosSql))[0];
                contexto.AnotarTraza("Semáforo puesto", $"Semáforo: {registro.Id}{Environment.NewLine}" +
                    $"Guid: {contexto.IdOperacion}{Environment.NewLine}" +
                    $"Login: {contexto.DatosDeConexion.Login}{Environment.NewLine}" +
                    $"Anotación: {anotacion}");

                return registro;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains($"AK_{ApiDeRegistroDtm.NombreDeTabla(typeof(SemaforoDeProcesoDtm))}"))
                {
                    var semaforos = LeerSemaforos(contexto, idNegocio, idElemento);
                    foreach (var semaforo in semaforos)
                    {
                        TimeSpan diferencia = DateTime.Now - semaforo.Fecha;
                        if (diferencia.TotalMinutes > 5)
                        {
                            QuitarSemaforo(contexto, semaforo.Id);
                        }
                        else
                        {
                            GestorDeErrores.Emitir($"El proceso '{semaforo.IdProceso}' del usuario '{semaforo.Login}' está ejecutándose desde '{semaforo.Fecha.ToLongDateString()}, {semaforo.Fecha.ToShortTimeString()}', motivo:'{semaforo.Anotacion}' espere unos minutos a que finalice.");
                        }
                    }
                    GestorDeErrores.Emitir($"Había un bloqueo en la BD, ya se ha liberado, vuelva a intentar la operación.");
                }
                throw;
            }
        }

        public static void QuitarSemaforo(ContextoSe contexto, int id)
        {
            if (id == 0)
                return;

            var sentencia = $@"delete from {ApiDeRegistroDtm.EsquemaTabla(typeof(SemaforoDeProcesoDtm))} where {ICampos.ID} = @{ICampos.ID}";

            var parametrosSql = new Dictionary<string, object>
            {
                { $"@{ICampos.ID}", id }
            };

            var consulta = new ConsultaSql<RegistroDtm>(sentencia);
            consulta.LanzarConsulta(new DynamicParameters(parametrosSql));
            contexto.AnotarTraza("Quitar semáforo", $"Semáforo: {id}");
        }
    }

    public static partial class ModeloDeNegocio
    {

        internal static void Libro(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LibroDtm>().Property(x => x.Ejercicio).HasColumnName(ICampos.EJERCICIO);
            modelBuilder.Entity<LibroDtm>().Property(x => x.Ejercicio).HasColumnType(IDominio.INT);
            modelBuilder.Entity<LibroDtm>().Property(x => x.Ejercicio).IsRequired(true);

            modelBuilder.Entity<LibroDtm>().Property(x => x.IdNegocio).HasColumnName(ICampos.ID_NEGOCIO);
            modelBuilder.Entity<LibroDtm>().Property(x => x.IdNegocio).HasColumnType(IDominio.INT);
            modelBuilder.Entity<LibroDtm>().Property(x => x.IdNegocio).IsRequired(true);

            modelBuilder.Entity<LibroDtm>().Property(x => x.IdCg).HasColumnName(ICampos.ID_CG);
            modelBuilder.Entity<LibroDtm>().Property(x => x.IdCg).HasColumnType(IDominio.INT);
            modelBuilder.Entity<LibroDtm>().Property(x => x.IdCg).IsRequired(true);

            modelBuilder.Entity<LibroDtm>().Property(x => x.IdTipo).HasColumnName(ICampos.ID_TIPO);
            modelBuilder.Entity<LibroDtm>().Property(x => x.IdTipo).HasColumnType(IDominio.INT);
            modelBuilder.Entity<LibroDtm>().Property(x => x.IdTipo).IsRequired(true);

            modelBuilder.Entity<LibroDtm>().Property(x => x.Valor).HasColumnName(ICampos.VALOR);
            modelBuilder.Entity<LibroDtm>().Property(x => x.Valor).HasColumnType(IDominio.INT);
            modelBuilder.Entity<LibroDtm>().Property(x => x.Valor).IsRequired(true);

            modelBuilder.Entity<LibroDtm>().HasAlternateKey(x => new { x.Ejercicio, x.IdNegocio, x.IdCg, x.IdTipo }).HasName($"AK_{ApiDeRegistroDtm.NombreDeTabla(typeof(LibroDtm))}");
        }

        internal static void SemaforoDeUnLibro(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SemaforoDeUnLibroDtm>().Property(x => x.IdLibro).HasColumnName(ICampos.ID_LIBRO);
            modelBuilder.Entity<SemaforoDeUnLibroDtm>().Property(x => x.IdLibro).HasColumnType(IDominio.INT);
            modelBuilder.Entity<SemaforoDeUnLibroDtm>().Property(x => x.IdLibro).IsRequired(true);

            modelBuilder.Entity<SemaforoDeUnLibroDtm>().Property(x => x.IdUsuario).HasColumnName(ICampos.ID_USUARIO);
            modelBuilder.Entity<SemaforoDeUnLibroDtm>().Property(x => x.IdUsuario).HasColumnType(IDominio.INT);
            modelBuilder.Entity<SemaforoDeUnLibroDtm>().Property(x => x.IdUsuario).IsRequired(true);

            modelBuilder.Entity<SemaforoDeUnLibroDtm>().Property(x => x.Fecha).HasColumnName(ICampos.FECHA)
                .HasColumnType(IDominio.DATETIME_2)
                .IsRequired(true);

            modelBuilder.Entity<SemaforoDeUnLibroDtm>().HasAlternateKey(x => new { x.IdLibro }).HasName($"AK_{ApiDeRegistroDtm.NombreDeTabla(typeof(SemaforoDeUnLibroDtm))}");
        }
        internal static void SemaforoDeUnProceso(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SemaforoDeProcesoDtm>().Property(x => x.IdNegocio).HasColumnName(ICampos.ID_NEGOCIO).HasColumnType(IDominio.INT).IsRequired(true);
            modelBuilder.Entity<SemaforoDeProcesoDtm>().Property(x => x.IdElemento).HasColumnName(ICampos.ID_ELEMENTO).HasColumnType(IDominio.INT).IsRequired(true);
            modelBuilder.Entity<SemaforoDeProcesoDtm>().Property(x => x.IdProceso).HasColumnName(ICampos.ID_PROCESO).HasColumnType(IDominio.UNIQUEIDENTIFIER).IsRequired(true);
            modelBuilder.Entity<SemaforoDeProcesoDtm>().Property(x => x.Operacion).HasColumnName(ICampos.OPERACION).HasColumnType(IDominio.VARCHAR_4).IsRequired(true);
            modelBuilder.Entity<SemaforoDeProcesoDtm>().Property(x => x.Login).HasColumnName(ICampos.LOGIN).HasColumnType(IDominio.VARCHAR_50).IsRequired(false);
            modelBuilder.Entity<SemaforoDeProcesoDtm>().Property(x => x.Anotacion).HasColumnName(ICampos.ANOTACION).HasColumnType(IDominio.VARCHAR_250).IsRequired(false);
            modelBuilder.Entity<SemaforoDeProcesoDtm>().Property(x => x.Fecha).HasColumnName(ICampos.FECHA).HasColumnType(IDominio.DATETIME_2).IsRequired(true);

            modelBuilder.Entity<SemaforoDeProcesoDtm>().HasAlternateKey(x => new { x.IdNegocio, x.IdElemento }).HasName($"AK_{ApiDeRegistroDtm.NombreDeTabla(typeof(SemaforoDeProcesoDtm))}");
        }



    }
}
