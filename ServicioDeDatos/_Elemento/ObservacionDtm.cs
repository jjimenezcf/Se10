using System;
using System.Collections.Generic;
using Dapper;
using Gestor.Errores;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Entorno;
using Utilidades;

namespace ServicioDeDatos.Elemento
{

    public class ltrDeObservaciones 
    { 
        public const string CreadaPorAdminSe = nameof(CreadaPorAdminSe);
        public const string PermitirSiTerminado = nameof(PermitirSiTerminado);
    }

    public class ObservacionDtm : RegistroConNombreDtm, IObservacion
    {
        public int IdElemento { get; set; }
        public string Descripcion { get; set; }
        public int IdCreador { get; set; }
        public DateTime CreadaEl { get; set; }
        public string Creador { get; }
        public string Elemento { get; }

        public virtual enumNegocio Negocio => enumNegocio.No_Definido;

    }

    public class ObservacionSql
    {
        public static ObservacionDtm LeerPorId(ContextoSe contexto, string esquemaTabla, int id, bool emitirError = true)
        {
            var _leerPorId = $@"
                     select {ICampos.ID} as {nameof(ObservacionDtm.Id)}
                          , {ICampos.NOMBRE} as {nameof(ObservacionDtm.Nombre)}
                          , {ICampos.DESCRIPCION} as {nameof(ObservacionDtm.Descripcion)}
                          , {ICampos.ID_ELEMENTO} as {nameof(ObservacionDtm.IdElemento)}
                          , {ICampos.ELEMENTO} as {nameof(ObservacionDtm.Elemento)}
                          , {ICampos.ID_CREADOR} as {nameof(ObservacionDtm.IdCreador)}
                          , {ICampos.CREADO_EL} as {nameof(ObservacionDtm.CreadaEl)}
                          , {ICampos.CREADOR} as {nameof(ObservacionDtm.Creador)}
                     from {esquemaTabla} T1 WITH(NOLOCK)
                     where {ICampos.ID} = @{ICampos.ID}
                     ";

            var parametrosSql = new Dictionary<string, object>();
            parametrosSql.Add($"@{ICampos.ID}", id);

            var consulta = new ConsultaSql<ObservacionDtm>(contexto, _leerPorId);
            var registros = consulta.LanzarConsulta(new DynamicParameters(parametrosSql));

            if (registros.Count == 0 && emitirError)
                GestorDeErrores.Emitir($"No se ha localizado la observación con Id: {id} en la tabla {esquemaTabla}");

            return registros.Count == 0 ? null : registros[0];
        }

        public static List<ObservacionDtm> ObservacionesDeUnElemento(ContextoSe contexto, string esquemaTabla, int idElemento, int posicion, int cantidad)
        {
            var _leerObservaciones = $@"
                     select {ICampos.ID} as {nameof(ObservacionDtm.Id)}
                          , {ICampos.NOMBRE} as {nameof(ObservacionDtm.Nombre)}
                          , {ICampos.DESCRIPCION} as {nameof(ObservacionDtm.Descripcion)}
                          , {ICampos.ID_ELEMENTO} as {nameof(ObservacionDtm.IdElemento)}
                          , {ICampos.ELEMENTO} as {nameof(ObservacionDtm.Elemento)}
                          , {ICampos.ID_CREADOR} as {nameof(ObservacionDtm.IdCreador)}
                          , {ICampos.CREADO_EL} as {nameof(ObservacionDtm.CreadaEl)}
                          , {ICampos.CREADOR} as {nameof(ObservacionDtm.Creador)}
                     from {esquemaTabla} T1 WITH(NOLOCK)
                     where {ICampos.ID_ELEMENTO} = @{ICampos.ID_ELEMENTO}
                     order by {ICampos.CREADO_EL} DESC 
                     ";
            var parametrosSql = new Dictionary<string, object>();
            parametrosSql.Add($"@{ICampos.ID_ELEMENTO}", idElemento);

            if (cantidad > 0)
            {
                _leerObservaciones = $@"{_leerObservaciones}{Environment.NewLine}OFFSET @posicion ROWS FETCH NEXT @cantidad ROWS ONLY";
                parametrosSql.Add($"@posicion", posicion);
                parametrosSql.Add($"@cantidad", cantidad);
            }

            var consulta = new ConsultaSql<ObservacionDtm>(contexto, _leerObservaciones);

            return consulta.LanzarConsulta(new DynamicParameters(parametrosSql));
        }

        public static ObservacionDtm UltimaCreada(ContextoSe contexto, string esquemaTabla, int idElemento)
        {
            var _UltimaCreada = $@"
                     select top(1) {ICampos.ID} as {nameof(ObservacionDtm.Id)}
                          , {ICampos.NOMBRE} as {nameof(ObservacionDtm.Nombre)}
                          , {ICampos.DESCRIPCION} as {nameof(ObservacionDtm.Descripcion)}
                          , {ICampos.ID_ELEMENTO} as {nameof(ObservacionDtm.IdElemento)}
                          , {ICampos.ELEMENTO} as {nameof(ObservacionDtm.Elemento)}
                          , {ICampos.ID_CREADOR} as {nameof(ObservacionDtm.IdCreador)}
                          , {ICampos.CREADO_EL} as {nameof(ObservacionDtm.CreadaEl)}
                          , {ICampos.CREADOR} as {nameof(ObservacionDtm.Creador)}
                     from {esquemaTabla} T1 WITH(NOLOCK)
                     where {ICampos.ID_ELEMENTO} = @{ICampos.ID_ELEMENTO}
                       and {ICampos.ID_CREADOR} = @{ICampos.ID_CREADOR}
                     order by {ICampos.ID} DESC 
                     ";
            var parametrosSql = new Dictionary<string, object>();
            parametrosSql.Add($"@{ICampos.ID_ELEMENTO}", idElemento);
            parametrosSql.Add($"@{ICampos.ID_CREADOR}", contexto.Usuario.Id);

            var consulta = new ConsultaSql<ObservacionDtm>(contexto, _UltimaCreada);

            var resultado = consulta.LanzarConsulta(new DynamicParameters(parametrosSql));

            return resultado.Count == 1 ? resultado[0] : null;

        }

        public static int ContarRegistros(ContextoSe contexto, string esquemaTabla, int idElemento)
        {
            var _leerObservaciones = $@"
               select Count(*) as cantidad
               from {esquemaTabla} T1 WITH(NOLOCK)
               where {ICampos.ID_ELEMENTO} = @{ICampos.ID_ELEMENTO}
               ";
            var parametrosSql = new Dictionary<string, object>();
            parametrosSql.Add($"@{ICampos.ID_ELEMENTO}", idElemento);

            var consulta = new ConsultaSql<RegistrosAfectados>(contexto, _leerObservaciones);

            var registros = consulta.LanzarConsulta(new DynamicParameters(parametrosSql));

            return registros[0].cantidad;
        }

        public static RegistroDtm Insertar(ContextoSe contexto, string esquemaTabla, int idElemento, string nombre, string descripcion, int idUsuario)
        {
            var sentencia = $@"Insert into {esquemaTabla} (
                                        {ICampos.ID_ELEMENTO}
                                      , {ICampos.NOMBRE}
                                      , {ICampos.DESCRIPCION}
                                      , {ICampos.ID_CREADOR}
                                      , {ICampos.CREADO_EL})
                               values ( @{ICampos.ID_ELEMENTO}
                                      , @{ICampos.NOMBRE}
                                      , @{ICampos.DESCRIPCION}
                                      , @{ICampos.ID_CREADOR}
                                      , @{ICampos.CREADO_EL})
                               SELECT SCOPE_IDENTITY() as {nameof(RegistroDtm.Id)}
             ";

            var parametrosSql = new Dictionary<string, object>
            {
                { $"@{ICampos.ID_ELEMENTO}", idElemento },
                { $"@{ICampos.NOMBRE}", nombre },
                { $"@{ICampos.DESCRIPCION}", descripcion },
                { $"@{ICampos.ID_CREADOR}", idUsuario },
                { $"@{ICampos.CREADO_EL}", DateTime.Now }
            };

            var consulta = new ConsultaSql<RegistroDtm>(contexto, sentencia);
            var registros = consulta.LanzarConsulta(new DynamicParameters(parametrosSql))[0];
            return registros;
        }

        public static void Modificar(ContextoSe contexto, string esquemaTabla, int id, string descripcion)
        {
            var sentencia = $@"update {esquemaTabla} 
                               set {ICampos.DESCRIPCION} = @{ICampos.DESCRIPCION}
                               where {ICampos.ID} = @{ICampos.ID}";

            var parametrosSql = new Dictionary<string, object>();
            parametrosSql.Add($"@{ICampos.ID}", id);
            parametrosSql.Add($"@{ICampos.DESCRIPCION}", descripcion);

            var consulta = new ConsultaSql<ObservacionDtm>(contexto, sentencia);
            consulta.EjecutarSentencia(new DynamicParameters(parametrosSql));
        }

    }

    public static class ApiDeObservaciones
    {
        internal static void DefinirCampos<TEntity, TPadre>(ModelBuilder modelBuilder)
            where TEntity : ObservacionDtm
            where TPadre : ElementoDtm
        {
            var nombreDeTabla = ApiDeRegistroDtm.NombreDeTabla(typeof(TEntity));

            ApiDeElementoDtm.DefinirCampoIdElemento<TEntity>(modelBuilder);
            ApiDeRegistroDtm.DefinirCampoIdDtm<TEntity>(modelBuilder);
            ApiDeNombreDtm.DefinirCampoNombreDtm<TEntity>(modelBuilder, conIndice: false);
            ApiDeNombreDtm.DefinirCampoDescripcion<TEntity>(modelBuilder);

            modelBuilder.Entity<TEntity>().Property(nameof(ObservacionDtm.Creador))
            .HasColumnName(ICampos.CREADOR)
            .HasColumnType(IDominio.VARCHAR_255)
            .HasComputedColumnSql($@"{ApiDeRegistroDtm.EsquemaDeTabla(typeof(UsuarioDtm))}.CC_{ApiDeRegistroDtm.NombreDeTabla(typeof(UsuarioDtm))}_{ICampos.EXPRESION}({ICampos.ID_CREADOR})");

            modelBuilder.Entity<TEntity>().Property(nameof(ObservacionDtm.Elemento))
            .HasColumnName(ICampos.ELEMENTO)
            .HasColumnType(IDominio.VARCHAR_255)
            .HasComputedColumnSql($@"{ApiDeRegistroDtm.EsquemaDeTabla(typeof(TPadre))}.CC_{ApiDeRegistroDtm.NombreDeTabla(typeof(TPadre))}_{ICampos.NOMBRE}({ICampos.ID_ELEMENTO})");

            modelBuilder.Entity<TEntity>().Property(x => x.IdCreador).HasColumnType(IDominio.INT).HasColumnName(ICampos.ID_CREADOR).IsRequired();
            modelBuilder.Entity<TEntity>().Property(x => x.CreadaEl).HasColumnType(IDominio.DATETIME_2).HasColumnName(ICampos.CREADO_EL).IsRequired();

            ApiDeRegistroDtm.DefinirFk<TEntity,UsuarioDtm>(modelBuilder,  nameof(ObservacionDtm.IdCreador), ICampos.ID_CREADOR, unico: false);
            ApiDeRegistroDtm.DefinirFk<TEntity, TPadre>(modelBuilder, nameof(ObservacionDtm.IdElemento), ICampos.ID_ELEMENTO, false);
        }


        /*
         *
            migrationBuilder.Sql($@"CREATE FUNCTION [ENTORNO].[CC_USUARIO_EXPRESION] (@id_creador int)
            RETURNS VarChar(250)
            AS
            begin
              declare @resultado VARCHAR(250)
              
              select @resultado = '('+ LOGIN +') ' + APELLIDO +' ' + NOMBRE from ENTORNO.USUARIO where id = @id_creador
              return @resultado
            END
            GO");

         */

    }
}
