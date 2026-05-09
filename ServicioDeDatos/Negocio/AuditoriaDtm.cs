using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Gestor.Errores;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;

namespace ServicioDeDatos.Negocio
{

    public class AuditoriaDtm : RegistroDtm
    {
        public int IdElemento { get; set; }

        public int IdUsuario { get; set; }

        public string Operacion { get; set; }

        public string registroJson { get; set; }

        public DateTime AuditadoEl { get; set; }
    }

    public static class AuditoriaSql
    {
        private static readonly string _Campos = $@"
                        {ICampos.ID} as {nameof(AuditoriaDtm.Id)}
                      , {ICampos.ID_ELEMENTO} as {nameof(AuditoriaDtm.IdElemento)}
                      , {ICampos.ID_USUARIO} as {nameof(AuditoriaDtm.IdUsuario)}
                      , {ICampos.OPERACION} as {nameof(AuditoriaDtm.Operacion)}
                      , {ICampos.REGISTRO} as {nameof(AuditoriaDtm.registroJson)}
                      , {ICampos.AUDITADO_EL} as {nameof(AuditoriaDtm.AuditadoEl)}
                     ";

        private static readonly string FiltroPorUsuario = nameof(FiltroPorUsuario);
        private static readonly string AplicarFiltroPorUsuario = $"And {ICampos.ID_USUARIO} in ([{ClausulasDeConsultas.ListaDeValores}])";

        public static AuditoriaDtm LeerPorId(ContextoSe contexto, string esquemaTabla, int id, bool emitirError = true)
        {
            var _leerPorId = $@"
                 select {_Campos}
                 from {esquemaTabla} T1 WITH(NOLOCK)
                 where {ICampos.ID} = @{ICampos.ID}
                     ";
            var parametrosSql = new Dictionary<string, object>();
            parametrosSql.Add($"@{ICampos.ID}", id);

            var consulta = new ConsultaSql<AuditoriaDtm>(contexto, _leerPorId);
            var registros = consulta.LanzarConsulta(new DynamicParameters(parametrosSql));

            if (registros.Count == 0 && emitirError)
                GestorDeErrores.Emitir($"No se ha localizado el registro de auditoría con Id: {id} en la tabla {esquemaTabla}");

            return registros.Count == 0 ? null: registros[0];
        }

        public static int ContarRegistros(ContextoSe contexto, string esquemaTabla, int idElemento, List<int> usuarios)
        {

            var contar = $@"
               select Count(*) as cantidad
               from {esquemaTabla} T1 WITH(NOLOCK)
               where {ICampos.ID_ELEMENTO} = @{ICampos.ID_ELEMENTO}
               [{FiltroPorUsuario}]
               ";
            var parametrosSql = new Dictionary<string, object>();
            parametrosSql.Add($"@{ICampos.ID_ELEMENTO}", idElemento);

            var consulta = new ConsultaSql<RegistrosAfectados>(contexto, contar);
            consulta.AplicarClausulaIn(FiltroPorUsuario, AplicarFiltroPorUsuario, usuarios);

            var registros = consulta.LanzarConsulta(new DynamicParameters(parametrosSql));

            return registros[0].cantidad;
        }


        public static IEnumerable<AuditoriaDtm> AuditoriaDeUnElemento(ContextoSe contexto, string esquemaTabla, int idElemento, List<int> usuarios, int posicion, int cantidad)
        {
            var _leerObservaciones = $@"
               select {_Campos}
               from {esquemaTabla} T1 WITH(NOLOCK)
               where {ICampos.ID_ELEMENTO} = @{ICampos.ID_ELEMENTO}
               [{FiltroPorUsuario}]
               order by {ICampos.AUDITADO_EL} DESC 
               [OFFSET]
               ";
            var parametrosSql = new Dictionary<string, object>();
            parametrosSql.Add($"@{ICampos.ID_ELEMENTO}", idElemento);

            if (cantidad > 0)
            {
                _leerObservaciones = _leerObservaciones.Replace("[OFFSET]", "OFFSET @posicion ROWS FETCH NEXT @cantidad ROWS ONLY");
                parametrosSql.Add($"@posicion", posicion);
                parametrosSql.Add($"@cantidad", cantidad);
            }
            else _leerObservaciones = _leerObservaciones.Replace("[OFFSET]", $"");

            var consulta = new ConsultaSql<AuditoriaDtm>(contexto, _leerObservaciones);

            consulta.AplicarClausulaIn(FiltroPorUsuario, AplicarFiltroPorUsuario, usuarios);

            return consulta.LanzarConsulta(new DynamicParameters(parametrosSql));
        }


        public static void InsertarAuditoria(ContextoSe contexto, Type tipo, int idElemento, string operacion, string valor)
        {
            var sentencia = $@"Insert into {ApiDeElementoDtm.TablaDeAuditoria(tipo)} (
                                        {ICampos.ID_ELEMENTO}
                                      , {ICampos.ID_USUARIO}
                                      , {ICampos.OPERACION}
                                      , {ICampos.REGISTRO}
                                      , {ICampos.AUDITADO_EL}) 
                               values (@{ICampos.ID_ELEMENTO}
                                      ,@{ICampos.ID_USUARIO}
                                      ,@{ICampos.OPERACION}
                                      ,@{ICampos.REGISTRO}
                                      ,@{ICampos.AUDITADO_EL})";


            var parametrosSql = new Dictionary<string, object>
            {
                { $"@{ICampos.ID_ELEMENTO}", idElemento },
                { $"@{ICampos.ID_USUARIO}", contexto.DatosDeConexion.IdUsuario },
                { $"@{ICampos.OPERACION}", operacion },
                { $"@{ICampos.REGISTRO}", valor },
                { $"@{ICampos.AUDITADO_EL}", DateTime.Now }
            };
            var consulta = new ConsultaSql<AuditoriaDtm>(contexto, sentencia);
            consulta.EjecutarSentencia(new DynamicParameters(parametrosSql));

            //contexto.Database.ExecuteSqlRaw(sentencia);
        }

    }

    public static class ApiDeAuditoria
    {
        internal static void DefinirCamposDeAuditoriaDtm<TEntity>(ModelBuilder modelBuilder) where TEntity : AuditoriaDtm
        {
            var nombreDeTabla = ApiDeRegistroDtm.NombreDeTabla(typeof(TEntity));

            modelBuilder.Entity<TEntity>().Property(p => p.IdElemento).HasColumnName("ID_ELEMENTO");
            modelBuilder.Entity<TEntity>().Property(p => p.IdElemento).HasColumnType("INT");
            modelBuilder.Entity<TEntity>().Property(p => p.IdElemento).IsRequired(true);
            modelBuilder.Entity<TEntity>().HasIndex(p => p.IdElemento).HasDatabaseName($"I_{nombreDeTabla}_ID_ELEMENTO");

            modelBuilder.Entity<TEntity>().Property(p => p.IdUsuario).HasColumnName("ID_USUARIO");
            modelBuilder.Entity<TEntity>().Property(p => p.IdUsuario).HasColumnType("INT");
            modelBuilder.Entity<TEntity>().Property(p => p.IdUsuario).IsRequired(true);
            modelBuilder.Entity<TEntity>().HasIndex(p => p.IdUsuario).HasDatabaseName($"I_{nombreDeTabla}_ID_USUARIO");

            modelBuilder.Entity<TEntity>().Property(p => p.Operacion).HasColumnName("OPERACION");
            modelBuilder.Entity<TEntity>().Property(p => p.Operacion).HasColumnType("CHAR(1)");
            modelBuilder.Entity<TEntity>().Property(p => p.Operacion).IsRequired(true);

            modelBuilder.Entity<TEntity>().Property(p => p.registroJson).HasColumnName("REGISTRO");
            modelBuilder.Entity<TEntity>().Property(p => p.registroJson).HasColumnType("VARCHAR(MAX)");
            modelBuilder.Entity<TEntity>().Property(p => p.registroJson).IsRequired(true);

            modelBuilder.Entity<TEntity>().Property(p => p.AuditadoEl).HasColumnName("AUDITADO_EL");
            modelBuilder.Entity<TEntity>().Property(p => p.AuditadoEl).HasColumnType("DATETIME2(7)");
            modelBuilder.Entity<TEntity>().Property(p => p.AuditadoEl).IsRequired(true);

        }
        public static bool ImplementaAuditoria(this RegistroDtm registro)
        {
            return registro.GetType().GetInterfaces().Contains(typeof(IAuditoria));
        }
    }

}
