using System;
using System.Collections.Generic;
using System.ComponentModel;
using Dapper;
using Gestor.Errores;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using ServicioDeDatos.Entorno;
using Utilidades;

namespace ServicioDeDatos.Elemento
{
    public enum enumTraza
    {
        [Description("Enviada por correo")]
        envioDeCorreo
    }

    public static class ltrDeTrazas
    {
        public static readonly string transicionRealizada = "Transición realizada: ";
        public static readonly string asuntoTransicion = $"Asunto: {transicionRealizada}";
        public static readonly string asuntoCorreo = "Asunto: ";
        public static readonly string cuerpoCorreo = "Cuerpo:";
        public static readonly string Bloqueada = "Bloqueo";
        public static readonly string Desbloqueada = "Desbloqueo";
        public static readonly string Error_De_Bloqueo = "No puede bloquear 'Referencia' ya que ya está bloqueado por 'Bloqueador'";
        public static readonly string Error_De_Quitar_Vinculo = "No puede quitar el vínculo a 'Referencia' ya que ya está bloqueado por 'Bloqueador'";
        public static readonly string Error_De_Desbloqueo = "No puede desbloquear 'Referencia' ya que ya está desbloqueado por 'desbloqueador'";
        public static readonly string Error_De_NoHayBloqueo = "No puede desbloquear 'Referencia' ya que nunca estuvo bloqueado";
        public static readonly string Error_De_Modificar_Bloqueado = "No puede modificar 'Referencia' ya que ya está bloqueado por 'Bloqueador'";
    }

    public class TrazaDtm : RegistroConNombreDtm, ITraza
    {
        public int IdElemento { get; set; }
        public string Descripcion { get; set; }
        public int IdCreador { get; set; }
        public DateTime CreadaEl { get; set; }
        public string Creador { get; }
        public string Elemento { get; }

        public virtual enumNegocio Negocio { get; set; }    
    }

    public class TrazaDtmSql
    {
        private static readonly string _Campos = $@" {ICampos.ID} as {nameof(TrazaDtm.Id)}
                          , {ICampos.NOMBRE} as {nameof(TrazaDtm.Nombre)}
                          , {ICampos.DESCRIPCION} as {nameof(TrazaDtm.Descripcion)}
                          , {ICampos.ID_ELEMENTO} as {nameof(TrazaDtm.IdElemento)}
                          , (select {ICampos.NOMBRE} from [TablaDeElementos] where {ICampos.ID} = t1.{ICampos.ID_ELEMENTO}) as {nameof(TrazaDtm.Elemento)}
                          , {ICampos.ID_CREADOR} as {nameof(TrazaDtm.IdCreador)}
                          , {ICampos.CREADO_EL} as {nameof(TrazaDtm.CreadaEl)}
                          , {ICampos.CREADOR} as {nameof(TrazaDtm.Creador)}
                     ";
       
        public static TrazaDtm LeerPorId(ContextoSe contexto, string esquemaTabla, Type tipoDeElemento, int id, bool emitirError = true)
        {
            var _leerPorId = $@"
                     select {_Campos}
                     from {esquemaTabla} T1 WITH(NOLOCK)
                     where {ICampos.ID} = @{ICampos.ID}
                     ".Replace("[TablaDeElementos]", $"{ApiDeRegistroDtm.EsquemaTabla(tipoDeElemento)}");

            var parametrosSql = new Dictionary<string, object>();
            parametrosSql.Add($"@{ICampos.ID}", id);

            var consulta = new ConsultaSql<TrazaDtm>(contexto, _leerPorId);
            var registros = consulta.LanzarConsulta(new DynamicParameters(parametrosSql));

            if (registros.Count == 0 && emitirError)
                GestorDeErrores.Emitir($"No se ha localizado la traza con Id: {id} en la tabla {esquemaTabla}");

            return registros.Count == 0 ? null : registros[0];
        }

        public static List<TrazaDtm> TrazasDeUnElemento(ContextoSe contexto, string esquemaTabla, Type tipoDeElemento, int idElemento, int posicion, int cantidad)
        {
            var _leerTrazaes = $@"
                     select {_Campos}
                     from {esquemaTabla} T1 WITH(NOLOCK)
                     where {ICampos.ID_ELEMENTO} = @{ICampos.ID_ELEMENTO}
                     order by {ICampos.CREADO_EL} DESC 
                     ".Replace("[TablaDeElementos]", $"{ApiDeRegistroDtm.EsquemaTabla(tipoDeElemento)}");

            var parametrosSql = new Dictionary<string, object>
            {
                { $"@{ICampos.ID_ELEMENTO}", idElemento }
            };
            if (cantidad > 0)
            {
                _leerTrazaes = $@"{_leerTrazaes}{Environment.NewLine}OFFSET @posicion ROWS FETCH NEXT @cantidad ROWS ONLY";
                parametrosSql.Add($"@posicion", posicion);
                parametrosSql.Add($"@cantidad", cantidad);
            }

            var consulta = new ConsultaSql<TrazaDtm>(contexto, _leerTrazaes);
            var registros = consulta.LanzarConsulta(new DynamicParameters(parametrosSql));

            return registros;
        }

        public static int ContarRegistros(ContextoSe contexto, string esquemaTabla, int idElemento)
        {
            var _leerTrazas = $@"
               select Count(*) as cantidad
               from {esquemaTabla} T1 WITH(NOLOCK)
               where {ICampos.ID_ELEMENTO} = @{ICampos.ID_ELEMENTO}
               ";
            var parametrosSql = new Dictionary<string, object>
            {
                { $"@{ICampos.ID_ELEMENTO}", idElemento }
            };

            var consulta = new ConsultaSql<RegistrosAfectados>(contexto, _leerTrazas);

            var registros = consulta.LanzarConsulta(new DynamicParameters(parametrosSql));

            return registros[0].cantidad;
        }

        public static RegistroDtm Insertar(ContextoSe contexto, string esquemaTabla, int idElemento, string nombre, string descripcion)
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
                { $"@{ICampos.ID_CREADOR}", contexto.DatosDeConexion.IdUsuario },
                { $"@{ICampos.CREADO_EL}", DateTime.Now }
            };

            var consulta = new ConsultaSql<RegistroDtm>(contexto, sentencia);
            var registros = consulta.LanzarConsulta(new DynamicParameters(parametrosSql))[0];
            return registros;
        }

        public static void Eliminar(ContextoSe contexto, string esquemaTabla, int id)
        {
            var sentencia = $@"Delete from {esquemaTabla} where {ICampos.ID} = @{ICampos.ID}";

            var parametrosSql = new Dictionary<string, object>
            {
                { $"@{ICampos.ID}", id }
            };

            var consulta = new ConsultaSql<RegistroDtm>(contexto, sentencia);
            consulta.EjecutarSentencia(new DynamicParameters(parametrosSql));
        }

    }

    public static class ApiTraza
    {
        internal static void DefinirCampos<TEntity, TPadre>(ModelBuilder modelBuilder)
            where TEntity : TrazaDtm
            where TPadre : ElementoDtm
        {
            var nombreDeTabla = ApiDeRegistroDtm.NombreDeTabla(typeof(TEntity));

            modelBuilder.Entity<TEntity>().Ignore(x => x.Negocio);

            ApiDeRegistroDtm.DefinirCampoIdDtm<TEntity>(modelBuilder);
            ApiDeElementoDtm.DefinirCampoIdElemento<TEntity>(modelBuilder);
            ApiDeNombreDtm.DefinirCampoNombreDtm<TEntity>(modelBuilder, conIndice: false);
            ApiDeNombreDtm.DefinirCampoDescripcion<TEntity>(modelBuilder);

            modelBuilder.Entity<TEntity>().Property(nameof(TrazaDtm.Creador))
            .HasColumnName(ICampos.CREADOR)
            .HasColumnType(IDominio.VARCHAR_255)
            .HasComputedColumnSql($@"{ApiDeRegistroDtm.EsquemaDeTabla(typeof(UsuarioDtm))}.CC_{ApiDeRegistroDtm.NombreDeTabla(typeof(UsuarioDtm))}_{ICampos.EXPRESION}({ICampos.ID_CREADOR})");

            modelBuilder.Entity<TEntity>().Property(x => x.IdCreador).HasColumnType(IDominio.INT).HasColumnName(ICampos.ID_CREADOR).IsRequired();
            modelBuilder.Entity<TEntity>().Property(x => x.CreadaEl).HasColumnType(IDominio.DATETIME_2).HasColumnName(ICampos.CREADO_EL).IsRequired();

            ApiDeRegistroDtm.DefinirFk<TEntity,UsuarioDtm>(modelBuilder,  nameof(TrazaDtm.IdCreador), ICampos.ID_CREADOR, unico: false);
            ApiDeRegistroDtm.DefinirFk<TEntity, TPadre>(modelBuilder, nameof(TrazaDtm.IdElemento), ICampos.ID_ELEMENTO, false);
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
