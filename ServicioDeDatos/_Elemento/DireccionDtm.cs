using System;
using System.Collections.Generic;
using System.ComponentModel;
using Dapper;
using Gestor.Errores;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Callejero;
using ServicioDeDatos.Entorno;
using Utilidades;

namespace ServicioDeDatos.Elemento
{

    public enum enumCalificadorDireccion
    {
        [Description("Correspondencia")]
        correspondencia,
        [Description("Fiscal")]
        fiscal,
        [Description("Contacto")]
        contacto,
        [Description("Ejecución")]
        ejecucion,
        [Description("Entrega")]
        entrega
    }

    public static class ltrDireccion
    {
        public const string FiltroPorCalle = nameof(FiltroPorCalle);
        public const string FiltroPorMunicipio = nameof(FiltroPorMunicipio);
        public const string FiltroPorZona = nameof(FiltroPorZona);
        public const string FiltroPorBarrio = nameof(FiltroPorBarrio);
        public const string FiltroPorCp = nameof(FiltroPorCp);
        public const string NoIndicada = "(no indicada)";
        public const string NumeroPolicia = nameof(NumeroPolicia);
        public const string RestoDireccion = nameof(RestoDireccion);
    }

    public class DireccionDtm : RegistroDtm, IDireccionDtm
    {
        public int IdElemento { get; set; }
        public enumCalificadorDireccion Calificador { get; set; }
        public int IdPais { get; set; }
        public int IdProvincia { get; set; }
        public int IdMunicipio { get; set; }
        public int IdCalle { get; set; }
        public int? IdZona { get; set; }
        public int? IdBarrio { get; set; }
        public int? IdCp { get; set; }
        public int? Numero { get; set; }
        public string Escalera { get; set; }
        public string Piso { get; set; }
        public string Puerta { get; set; }
        public string Otros { get; set; }
        public string Url { get; set; }
        public bool Activo { get; set; }

        public string Elemento { get; }
        public string Pais { get; }
        public string Provincia { get; }
        public string Municipio { get; }
        public string Calle { get; }
        public string Zona { get; }
        public string Barrio { get; }
        public string Cp { get; }
        public int IdCreador { get; set; }
        public DateTime CreadaEl { get; set; }
        public string Creador { get; }
        public DireccionDtm Copiar()
        {
            return new DireccionDtm()
            {
                Id = 0,
                IdCreador = 0,
                IdElemento = 0,
                Calificador = Calificador,
                IdPais = IdPais,
                IdProvincia = IdProvincia,
                IdMunicipio = IdMunicipio,
                IdCalle = IdCalle,
                IdZona = IdZona,
                IdBarrio = IdBarrio,
                IdCp = IdCp,
                Numero = Numero,
                Escalera = Escalera,
                Piso = Piso,
                Puerta = Puerta,
                Otros = Otros,
                Url = Url,
                Activo = true
            };
        }

        public bool EsIgualA(DireccionDtm direccion, bool compararCalificador)
        {
            if (direccion == null) {  return false; }
            if (compararCalificador && direccion.Calificador != Calificador) { return false; }
            if (direccion.IdPais != IdPais) { return false; }
            if (direccion.IdProvincia != IdProvincia) { return false; }
            if (direccion.IdMunicipio != IdMunicipio) { return false; }
            if (direccion.IdCalle != IdCalle) { return false; }
            if (direccion.IdZona != IdZona) { return false; }
            if (direccion.IdBarrio != IdBarrio) { return false; }
            if (direccion.IdCp != IdCp) { return false; }
            if (direccion.Numero != Numero) { return false; }
            if (direccion.Escalera != Escalera) { return false; }
            if (direccion.Piso != Piso) { return false; }
            if (direccion.Puerta != Puerta) { return false; }

            return true;
        }

        public virtual enumNegocio Negocio { get; set; }

    }

    public class DireccionSql
    {
        private static readonly string _Campos = $@"
                            {ICampos.ID}               as {nameof(DireccionDtm.Id)}
                          , {ICampos.CALIFICADOR}      as {nameof(DireccionDtm.Calificador)}
                          , {ICampos.ID_ELEMENTO}      as {nameof(DireccionDtm.IdElemento)}
                          , {ICampos.ID_PAIS}          as {nameof(DireccionDtm.IdPais)}
                          , {ICampos.ID_PROVINCIA}     as {nameof(DireccionDtm.IdProvincia)}
                          , {ICampos.ID_MUNICIPIO}     as {nameof(DireccionDtm.IdMunicipio)}
                          , {ICampos.ID_CALLE}         as {nameof(DireccionDtm.IdCalle)}
                          , {ICampos.ID_BARRIO}        as {nameof(DireccionDtm.IdBarrio)}
                          , {ICampos.ID_ZONA}          as {nameof(DireccionDtm.IdZona)}
                          , {ICampos.ID_CP}            as {nameof(DireccionDtm.IdCp)}
                          , (select {ICampos.NOMBRE} from {ApiDeRegistroDtm.EsquemaTabla(typeof(PaisDtm))}  where {ICampos.ID} = t1.{ICampos.ID_PAIS}) as {nameof(DireccionDtm.Pais)}
                          , (select {ICampos.NOMBRE} from {ApiDeRegistroDtm.EsquemaTabla(typeof(ProvinciaDtm))}  where {ICampos.ID} = t1.{ICampos.ID_PROVINCIA}) as {nameof(DireccionDtm.Provincia)}
                          , (select {ICampos.NOMBRE} from {ApiDeRegistroDtm.EsquemaTabla(typeof(MunicipioDtm))}  where {ICampos.ID} = t1.{ICampos.ID_MUNICIPIO}) as {nameof(DireccionDtm.Municipio)}
                          , (select {ICampos.NOMBRE} from {ApiDeRegistroDtm.EsquemaTabla(typeof(CalleDtm))}  where {ICampos.ID} = t1.{ICampos.ID_CALLE}) as {nameof(DireccionDtm.Calle)}
                          , (select {ICampos.NOMBRE} from {ApiDeRegistroDtm.EsquemaTabla(typeof(BarrioDtm))}  where {ICampos.ID} = IsNull(t1.{ICampos.ID_BARRIO},0)) as {nameof(DireccionDtm.Barrio)}
                          , (select {ICampos.NOMBRE} from {ApiDeRegistroDtm.EsquemaTabla(typeof(ZonaDtm))}  where {ICampos.ID} = IsNull(t1.{ICampos.ID_ZONA},0)) as {nameof(DireccionDtm.Zona)}
                          , (select {ICampos.CP}     from {ApiDeRegistroDtm.EsquemaTabla(typeof(CodigoPostalDtm))}  where {ICampos.ID} = IsNull(t1.{ICampos.ID_CP},0)) as {nameof(DireccionDtm.Cp)}
                          , (select {ICampos.NOMBRE} from [TablaDeElementos] where {ICampos.ID} = t1.{ICampos.ID_ELEMENTO}) as {nameof(DireccionDtm.Elemento)}
                          , {ICampos.NUMERO}           as {nameof(DireccionDtm.Numero)}
                          , {ICampos.ESCALERA}         as {nameof(DireccionDtm.Escalera)}
                          , {ICampos.PISO}             as {nameof(DireccionDtm.Piso)}
                          , {ICampos.PUERTA}           as {nameof(DireccionDtm.Puerta)}
                          , {ICampos.OTROS}            as {nameof(DireccionDtm.Otros)}
                          , {ICampos.URL}              as {nameof(DireccionDtm.Url)}
                          , {ICampos.ACTIVO}           as {nameof(DireccionDtm.Activo)}
                          , {ICampos.ID_CREADOR}       as {nameof(DireccionDtm.IdCreador)}
                          , {ICampos.CREADO_EL}        as {nameof(DireccionDtm.CreadaEl)}
                          , {ICampos.CREADOR}          as {nameof(DireccionDtm.Creador)}
                     ";

        public static DireccionDtm LeerPorId(ContextoSe contexto, string esquemaTabla, Type tipoDeElemento, int id, bool emitirError = true)
        {
            var _leerPorId = $@"
                     select {_Campos}
                     from {esquemaTabla} T1 WITH(NOLOCK)
                     where {ICampos.ID} = @{ICampos.ID}
                     ".Replace("[TablaDeElementos]", $"{ApiDeRegistroDtm.EsquemaTabla(tipoDeElemento)}");

            var parametrosSql = new Dictionary<string, object>
            {
                { $"@{ICampos.ID}", id }
            };

            var consulta = new ConsultaSql<DireccionDtm>(contexto, _leerPorId);
            var registros = consulta.LanzarConsulta(new DynamicParameters(parametrosSql));

            if (registros.Count == 0 && emitirError)
                GestorDeErrores.Emitir($"No se ha localizado la dirección con Id: {id} en la tabla {esquemaTabla}");

            return registros.Count == 0 ? null : registros[0];
        }

        public static List<DireccionDtm> DireccionesDeUnElemento(ContextoSe contexto, string esquemaTabla, Type tipoDeElemento, int idElemento, int posicion, int cantidad)
        {
            var _leerDirecciones = $@"
                     select {_Campos}
                     from {esquemaTabla} T1
                     where {ICampos.ID_ELEMENTO} = @{ICampos.ID_ELEMENTO}
                     order by {ICampos.CREADO_EL} DESC 
                     ".Replace("[TablaDeElementos]", $"{ApiDeRegistroDtm.EsquemaTabla(tipoDeElemento)}");

            var parametrosSql = new Dictionary<string, object>
            {
                { $"@{ICampos.ID_ELEMENTO}", idElemento }
            };
            if (cantidad > 0)
            {
                _leerDirecciones = $@"{_leerDirecciones}{Environment.NewLine}OFFSET @posicion ROWS FETCH NEXT @cantidad ROWS ONLY";
                parametrosSql.Add($"@posicion", posicion);
                parametrosSql.Add($"@cantidad", cantidad);
            }

            var consulta = new ConsultaSql<DireccionDtm>(contexto, _leerDirecciones);
            var registros = consulta.LanzarConsulta(new DynamicParameters(parametrosSql));

            return registros;
        }

        public static int ContarRegistros(ContextoSe contexto, string esquemaTabla, int idElemento)
        {
            var _leerDirecciones = $@"
               select Count(*) as cantidad
               from {esquemaTabla} T1 WITH(NOLOCK)
               where {ICampos.ID_ELEMENTO} = @{ICampos.ID_ELEMENTO}
               ";
            var parametrosSql = new Dictionary<string, object>
            {
                { $"@{ICampos.ID_ELEMENTO}", idElemento }
            };

            var consulta = new ConsultaSql<RegistrosAfectados>(contexto, _leerDirecciones);

            var registros = consulta.LanzarConsulta(new DynamicParameters(parametrosSql));

            return registros[0].cantidad;
        }

        public static RegistroDtm Insertar(ContextoSe contexto, string esquemaTabla, int idElemento, string calificador, int idPais, int idProvincia, int idMunicipio, int idCalle, int? idBarrio, int? idZona, int? idCp, int? numero, string escalera, string piso, string puerta, string otros, string url)
        {
            var sentencia = $@"Insert into {esquemaTabla} (  
                                         {ICampos.CALIFICADOR} 
                                       , {ICampos.ID_ELEMENTO} 
                                       , {ICampos.ID_PAIS}     
                                       , {ICampos.ID_PROVINCIA}
                                       , {ICampos.ID_MUNICIPIO}
                                       , {ICampos.ID_CALLE}    
                                       , {ICampos.ID_BARRIO}   
                                       , {ICampos.ID_ZONA}     
                                       , {ICampos.ID_CP}       
                                       , {ICampos.NUMERO}      
                                       , {ICampos.ESCALERA}    
                                       , {ICampos.PISO}        
                                       , {ICampos.PUERTA}      
                                       , {ICampos.OTROS}       
                                       , {ICampos.URL}         
                                       , {ICampos.ACTIVO}      
                                       , {ICampos.CREADO_EL}
                                       , {ICampos.ID_CREADOR} 
                                       )
                               values (        
                                         @{ICampos.CALIFICADOR} 
                                       , @{ICampos.ID_ELEMENTO} 
                                       , @{ICampos.ID_PAIS}     
                                       , @{ICampos.ID_PROVINCIA}
                                       , @{ICampos.ID_MUNICIPIO}
                                       , @{ICampos.ID_CALLE}    
                                       , @{ICampos.ID_BARRIO}   
                                       , @{ICampos.ID_ZONA}     
                                       , @{ICampos.ID_CP}       
                                       , @{ICampos.NUMERO}      
                                       , @{ICampos.ESCALERA}    
                                       , @{ICampos.PISO}        
                                       , @{ICampos.PUERTA}      
                                       , @{ICampos.OTROS}       
                                       , @{ICampos.URL}         
                                       , @{ICampos.ACTIVO}          
                                       , @{ICampos.CREADO_EL}
                                       , @{ICampos.ID_CREADOR} 
                                      )
                               SELECT SCOPE_IDENTITY() as {nameof(RegistroDtm.Id)}
             ";

            var parametrosSql = new Dictionary<string, object>
            {
                { $"@{ICampos.CALIFICADOR}", calificador },
                { $"@{ICampos.ID_ELEMENTO}", idElemento },
                { $"@{ICampos.ID_PAIS}", idPais },
                { $"@{ICampos.ID_PROVINCIA}", idProvincia },
                { $"@{ICampos.ID_MUNICIPIO}", idMunicipio },
                { $"@{ICampos.ID_CALLE}", idCalle },
                { $"@{ICampos.ID_BARRIO}", idBarrio },
                { $"@{ICampos.ID_ZONA}", idZona },
                { $"@{ICampos.ID_CP}", idCp },
                { $"@{ICampos.NUMERO}", numero },
                { $"@{ICampos.ESCALERA}", escalera },
                { $"@{ICampos.PISO}", piso },
                { $"@{ICampos.PUERTA}", puerta },
                { $"@{ICampos.OTROS}", otros },
                { $"@{ICampos.URL}", url },
                { $"@{ICampos.ACTIVO}", true },
                { $"@{ICampos.ID_CREADOR}", contexto.DatosDeConexion.IdUsuario },
                { $"@{ICampos.CREADO_EL}", DateTime.Now }
            };

            var consulta = new ConsultaSql<RegistroDtm>(contexto, sentencia);
            var registros = consulta.LanzarConsulta(new DynamicParameters(parametrosSql))[0];
            return registros;
        }

        public static void Modificar(ContextoSe contexto, string esquemaTabla, int id, string calificador, int idPais, int idProvincia, int idMunicipio, int idCalle, int? idBarrio, int? idZona, int? idCp, int? numero, string escalera, string piso, string puerta, string otros, string url, bool activo)
        {
            var sentencia = $@"update {esquemaTabla} set 
                                         {ICampos.CALIFICADOR} = @{ICampos.CALIFICADOR} 
                                       , {ICampos.ID_PAIS}     = @{ICampos.ID_PAIS}     
                                       , {ICampos.ID_PROVINCIA}= @{ICampos.ID_PROVINCIA}
                                       , {ICampos.ID_MUNICIPIO}= @{ICampos.ID_MUNICIPIO}
                                       , {ICampos.ID_CALLE}    = @{ICampos.ID_CALLE}    
                                       , {ICampos.ID_BARRIO}   = @{ICampos.ID_BARRIO}   
                                       , {ICampos.ID_ZONA}     = @{ICampos.ID_ZONA}     
                                       , {ICampos.ID_CP}       = @{ICampos.ID_CP}       
                                       , {ICampos.NUMERO}      = @{ICampos.NUMERO}      
                                       , {ICampos.ESCALERA}    = @{ICampos.ESCALERA}    
                                       , {ICampos.PISO}        = @{ICampos.PISO}        
                                       , {ICampos.PUERTA}      = @{ICampos.PUERTA}      
                                       , {ICampos.OTROS}       = @{ICampos.OTROS}       
                                       , {ICampos.URL}         = @{ICampos.URL}         
                                       , {ICampos.ACTIVO}      = @{ICampos.ACTIVO}      
                               where {ICampos.ID} = @{ICampos.ID}";

            var parametrosSql = new Dictionary<string, object>
            {
                { $"@{ICampos.ID}", id },
                { $"@{ICampos.CALIFICADOR}", calificador },
                { $"@{ICampos.ID_PAIS}", idPais },
                { $"@{ICampos.ID_PROVINCIA}", idProvincia },
                { $"@{ICampos.ID_MUNICIPIO}", idMunicipio },
                { $"@{ICampos.ID_CALLE}", idCalle },
                { $"@{ICampos.ID_BARRIO}", idBarrio },
                { $"@{ICampos.ID_ZONA}", idZona },
                { $"@{ICampos.ID_CP}", idCp },
                { $"@{ICampos.NUMERO}", numero },
                { $"@{ICampos.ESCALERA}", escalera },
                { $"@{ICampos.PISO}", piso },
                { $"@{ICampos.PUERTA}", puerta },
                { $"@{ICampos.OTROS}", otros },
                { $"@{ICampos.URL}", url },
                { $"@{ICampos.ACTIVO}", activo }
            };

            var consulta = new ConsultaSql<DireccionDtm>(contexto, sentencia);
            consulta.EjecutarSentencia(new DynamicParameters(parametrosSql));
        }

        public static void Eliminar(ContextoSe contexto, string esquemaTabla, int id)
        {
            var sentencia = $@"delete from {esquemaTabla} where {ICampos.ID} = @{ICampos.ID}";

            var parametrosSql = new Dictionary<string, object>
            {
                { $"@{ICampos.ID}", id }
            };

            var consulta = new ConsultaSql<DireccionDtm>(contexto, sentencia);
            consulta.EjecutarSentencia(new DynamicParameters(parametrosSql));
        }

    }


    internal static class ApiDireccionDtm
    {
        internal static void DefinirCampos<TEntity, TPadre>(ModelBuilder modelBuilder)
            where TEntity : DireccionDtm
            where TPadre : ElementoDtm
        {
            var nombreDeTabla = ApiDeRegistroDtm.NombreDeTabla(typeof(TEntity));

            ApiDeRegistroDtm.DefinirCampoIdDtm<TEntity>(modelBuilder);


            modelBuilder.Entity<TEntity>().Ignore(x => x.Negocio);

            modelBuilder.Entity<TEntity>().Property(x => x.IdElemento).HasColumnType(IDominio.INT).HasColumnName(ICampos.ID_ELEMENTO).IsRequired();
            modelBuilder.Entity<TEntity>().Property(x => x.IdPais).HasColumnType(IDominio.INT).HasColumnName(ICampos.ID_PAIS).IsRequired();
            modelBuilder.Entity<TEntity>().Property(x => x.IdProvincia).HasColumnType(IDominio.INT).HasColumnName(ICampos.ID_PROVINCIA).IsRequired();
            modelBuilder.Entity<TEntity>().Property(x => x.IdMunicipio).HasColumnType(IDominio.INT).HasColumnName(ICampos.ID_MUNICIPIO).IsRequired();
            modelBuilder.Entity<TEntity>().Property(x => x.IdCalle).HasColumnType(IDominio.INT).HasColumnName(ICampos.ID_CALLE).IsRequired();
            modelBuilder.Entity<TEntity>().Property(x => x.IdZona).HasColumnType(IDominio.INT).HasColumnName(ICampos.ID_ZONA).IsRequired(false);
            modelBuilder.Entity<TEntity>().Property(x => x.IdBarrio).HasColumnType(IDominio.INT).HasColumnName(ICampos.ID_BARRIO).IsRequired(false);
            modelBuilder.Entity<TEntity>().Property(x => x.IdCp).HasColumnType(IDominio.INT).HasColumnName(ICampos.ID_CP).IsRequired(false);
            modelBuilder.Entity<TEntity>().Property(x => x.Calificador).HasColumnType(IDominio.VARCHAR_20).HasColumnName(ICampos.CALIFICADOR).IsRequired();
            modelBuilder.Entity<TEntity>().Property(x => x.Numero).HasColumnType(IDominio.INT).HasColumnName(ICampos.NUMERO).IsRequired(false);
            modelBuilder.Entity<TEntity>().Property(x => x.Escalera).HasColumnType(IDominio.VARCHAR_4).HasColumnName(ICampos.ESCALERA).IsRequired(false);
            modelBuilder.Entity<TEntity>().Property(x => x.Puerta).HasColumnType(IDominio.VARCHAR_15).HasColumnName(ICampos.PUERTA).IsRequired(false);
            modelBuilder.Entity<TEntity>().Property(x => x.Piso).HasColumnType(IDominio.VARCHAR_4).HasColumnName(ICampos.PISO).IsRequired(false);
            modelBuilder.Entity<TEntity>().Property(x => x.Otros).HasColumnType(IDominio.VARCHAR_2000).HasColumnName(ICampos.OTROS).IsRequired(false);
            modelBuilder.Entity<TEntity>().Property(x => x.Url).HasColumnType(IDominio.URL).HasColumnName(ICampos.URL).IsRequired(false);
            modelBuilder.Entity<TEntity>().Property(x => x.Activo).HasColumnType(IDominio.BIT).HasColumnName(ICampos.ACTIVO).IsRequired();

            ApiDeRegistroDtm.DefinirFk<TEntity, PaisDtm>(modelBuilder, nameof(DireccionDtm.IdPais), ICampos.ID_PAIS, false);
            ApiDeRegistroDtm.DefinirFk<TEntity, ProvinciaDtm>(modelBuilder, nameof(DireccionDtm.IdProvincia), ICampos.ID_PROVINCIA, false);
            ApiDeRegistroDtm.DefinirFk<TEntity, MunicipioDtm>(modelBuilder, nameof(DireccionDtm.IdMunicipio), ICampos.ID_MUNICIPIO, false);
            ApiDeRegistroDtm.DefinirFk<TEntity, CalleDtm>(modelBuilder, nameof(DireccionDtm.IdCalle), ICampos.ID_CALLE, false);
            ApiDeRegistroDtm.DefinirFk<TEntity, ZonaDtm>(modelBuilder, nameof(DireccionDtm.IdZona), ICampos.ID_ZONA, false);
            ApiDeRegistroDtm.DefinirFk<TEntity, BarrioDtm>(modelBuilder, nameof(DireccionDtm.IdBarrio), ICampos.ID_BARRIO, false);
            ApiDeRegistroDtm.DefinirFk<TEntity, CodigoPostalDtm>(modelBuilder, nameof(DireccionDtm.IdCp), ICampos.ID_CP, false);

            ApiDeRegistroDtm.DefinirFk<TEntity, TPadre>(modelBuilder, nameof(DireccionDtm.IdElemento), ICampos.ID_ELEMENTO, false);

            modelBuilder.Entity<TEntity>()
                        .HasIndex(new string[] {
                          nameof(DireccionDtm.Calificador)
                        , nameof(DireccionDtm.IdElemento)
                        , nameof(DireccionDtm.IdPais)
                        , nameof(DireccionDtm.IdProvincia)
                        , nameof(DireccionDtm.IdMunicipio)
                        , nameof(DireccionDtm.IdCalle)
                        , nameof(DireccionDtm.Numero)
                        , nameof(DireccionDtm.Puerta)
                        , nameof(DireccionDtm.Escalera)
                        , nameof(DireccionDtm.Piso)
                        })
                        .IsUnique(true)
                        .HasDatabaseName($"AK_{nombreDeTabla}");

            modelBuilder.Entity<TEntity>().Property(x => x.IdCreador).HasColumnType(IDominio.INT).HasColumnName(ICampos.ID_CREADOR).IsRequired();
            modelBuilder.Entity<TEntity>().Property(x => x.CreadaEl).HasColumnType(IDominio.DATETIME_2).HasColumnName(ICampos.CREADO_EL).IsRequired();

            modelBuilder.Entity<TEntity>().Property(nameof(ObservacionDtm.Creador))
            .HasColumnName(ICampos.CREADOR)
            .HasColumnType(IDominio.VARCHAR_255)
            .HasComputedColumnSql($@"{ApiDeRegistroDtm.EsquemaDeTabla(typeof(UsuarioDtm))}.CC_{ApiDeRegistroDtm.NombreDeTabla(typeof(UsuarioDtm))}_{ICampos.EXPRESION}({ICampos.ID_CREADOR})");

            ApiDeRegistroDtm.DefinirFk<TEntity, UsuarioDtm>(modelBuilder, nameof(ObservacionDtm.IdCreador), ICampos.ID_CREADOR, unico: false);

        }
    }
}
