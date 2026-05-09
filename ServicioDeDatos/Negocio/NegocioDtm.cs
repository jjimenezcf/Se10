using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Dapper;
using Gestor.Errores;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.SistemaDocumental;
using Utilidades;

namespace ServicioDeDatos.Negocio
{
    public interface ITieneCampoNegocio
    {
        public int IdNegocio { get; set; }
        public abstract NegocioDtm Negocio { get; }
    }

    [Table(Tablas.NEGOCIO, Schema = Esquemas.NEGOCIO)]
    public class NegocioDtm : RegistroConNombreDtm, ITienePermisoDeAdm, ITienePermisoDeGestor, ITienePermisoDeConsultor, INecesitaSerParametrizador
    {
        public string ElementoDtm { get; set; }
        public string ElementoDto { get; set; }
        public string Icono { get; set; }
        public bool Activo { get; set; }
        public int IdGestor { get; set; }
        public int IdConsultor { get; set; }
        public int IdAdministrador { get; set; }

        public PermisoDtm Gestor { get; set; }
        public PermisoDtm Consultor { get; set; }
        public PermisoDtm Administrador { get; set; }

        public bool UsaSeguridad { get; set; }
        public bool EsDeParametrizacion { get; set; }
        public string Enumerado { get; set; }
        public bool UsaCentroGestor { get; set; }

    }

    [Table(Tablas.NEGOCIO + "_" + Sufijo.PLANTILLA, Schema = Esquemas.NEGOCIO)]
    public class PlantillaDeNegocioDtm : RegistroConNombreDtm, IPlantillaDeNegocio
    {
        public int IdAccion { get; set; }
        public int IdNegocio { get; set; }

        public int IdPermiso { get; set; }

        public NegocioDtm Negocio { get; set; }

        public AccionDtm Accion { get; set; }
        public PermisoDtm Permiso { get; set; }

        public string NombrePa => $"Plt_De_Negocio_{Nombre.NormalizarFichero()}";
        public string fichero => $"Plt_{Nombre}.{enumExtensiones.docx}".NormalizarFichero();

        public int? IdArchivo { get; set; }
        public ArchivoDtm Archivo { get; set; }
    }


    public static class NegocioSqls
    {

        private static readonly string LeerNegocio = @$"
        SELECT [ID] as {nameof(NegocioDtm.Id)}
         ,[{ICampos.ELEMENTO_DTM}] as {nameof(NegocioDtm.ElementoDtm)}
         ,[{ICampos.ICONO}] as {nameof(NegocioDtm.Icono)}
         ,[{ICampos.ACTIVO}] as {nameof(NegocioDtm.Activo)}
         ,[{ICampos.ID_GESTOR}] as {nameof(NegocioDtm.IdGestor)}
         ,[{ICampos.ID_CONSULTOR}] as {nameof(NegocioDtm.IdConsultor)}
         ,[{ICampos.ID_ADM}] as {nameof(NegocioDtm.IdAdministrador)}
         ,[{ICampos.NOMBRE}] as {nameof(NegocioDtm.Nombre)}
         ,[{ICampos.ELEMENTO_DTO}] as  {nameof(NegocioDtm.ElementoDto)}
         ,[{ICampos.USA_SEGURIDAD}] as {nameof(NegocioDtm.UsaSeguridad)}
         ,[{ICampos.ES_DE_PARAMETRIZACION}] as {nameof(NegocioDtm.EsDeParametrizacion)}
         ,[{ICampos.ENUMERADO}] as {nameof(NegocioDtm.Enumerado)}
         ,[{ICampos.USA_CG}] as {nameof(NegocioDtm.UsaCentroGestor)}
        FROM {ApiDeRegistroDtm.EsquemaTabla(typeof(NegocioDtm))} WITH(NOLOCK)";


        /// <summary>
        /// Función obsoleta, ya se usa el acceso mirando la tabla temporal SEGURIDAD.PERMISO_POR_NEGOCIO
        /// </summary>
        /// <param name="contexto"></param>
        /// <param name="idNegocio"></param>
        /// <returns></returns>
        public static List<ModoDeAccesoAlNegocioDtm> LeerModoDeAcceso(ContextoSe contexto, int idNegocio)
        {
            var _leer = @"
                 SELECT ID
                 , ADMINISTRADOR
                 , GESTOR
                 , CONSULTOR
                 , IDUSUA
                 , IDPERMISO
                 , ORIGEN
                 FROM NEGOCIO.MODO_ACCESO_AL_NEGOCIO_POR_USUARIO('[negocio]',[idUsuario])
            ";
            var sentenciaSql = _leer
                .Replace("[idNnegocio]", idNegocio.ToString())
                .Replace("[idUsuario]", contexto.DatosDeConexion.IdUsuario.ToString());

            var consulta = new ConsultaSql<ModoDeAccesoAlNegocioDtm>(contexto, sentenciaSql);
            return consulta.LanzarConsulta(new DynamicParameters(new Dictionary<string, object>()));
        }

        public static NegocioDtm LeerPorId(int id)
        {
            string _leer = $@"{LeerNegocio}{Environment.NewLine} WHERE [{ICampos.ID}] like @{ICampos.ID}";
            var consulta = new ConsultaSql<NegocioDtm>(_leer);
            var parametros = new Dictionary<string, object> { { $"@{ICampos.ID}", id } };
            return consulta.LanzarConsulta(new DynamicParameters(parametros))[0];
        }

        public static List<NegocioDtm> LeerPorNombre(string nombre)
        {
            string _leer = $@"{LeerNegocio}{Environment.NewLine} WHERE [{ICampos.NOMBRE}] like @{ICampos.NOMBRE}";
            var consulta = new ConsultaSql<NegocioDtm>(_leer);
            var parametros = new Dictionary<string, object> { { $"@{ICampos.NOMBRE}", nombre } };
            return consulta.LanzarConsulta(new DynamicParameters(parametros));
        }

        public static List<NegocioDtm> LeerNegocioPorDtm(string dtm)
        {
            string _leer = $@"{LeerNegocio}{Environment.NewLine} WHERE {ICampos.ELEMENTO_DTM} like @{ICampos.ELEMENTO_DTM}";
            var consulta = new ConsultaSql<NegocioDtm>(_leer);
            var parametros = new Dictionary<string, object> { { $"@{ICampos.ELEMENTO_DTM}", dtm } };
            return consulta.LanzarConsulta(new DynamicParameters(parametros));
        }

        public static List<NegocioDtm> LeerNegocioPorDto(string dto)
        {
            string _leer = $@"{LeerNegocio}{Environment.NewLine} WHERE {ICampos.ELEMENTO_DTO} like @{ICampos.ELEMENTO_DTO}";
            var consulta = new ConsultaSql<NegocioDtm>(_leer);
            var parametros = new Dictionary<string, object> { { $"@{ICampos.ELEMENTO_DTO}", dto } };
            return consulta.LanzarConsulta(new DynamicParameters(parametros));
        }

        public static NegocioDtm LeerNegocioPorEnumerado(enumNegocio negocio, bool erroSinoHay = true, bool errorSiHayMasDeUno = true)
        {
            string _leer = $@"{LeerNegocio}{Environment.NewLine} WHERE {ICampos.ENUMERADO} like @{ICampos.ENUMERADO}";
            var consulta = new ConsultaSql<NegocioDtm>(_leer);
            var parametros = new Dictionary<string, object> { { $"@{ICampos.ENUMERADO}", negocio.ToString() } };
            var negocios = consulta.LanzarConsulta(new DynamicParameters(parametros));

            if (negocios.Count == 0)
            {
                if (erroSinoHay) GestorDeErrores.Emitir($"No se ha definido el negocio '{negocio}'");
                return null;
            }
            if (negocios.Count > 1)
            {
                if (errorSiHayMasDeUno) GestorDeErrores.Emitir($"No se ha definido el negocio '{negocio}'");
                return null;
            } 
            return negocios[0];
        }


    }

    public static partial class ModeloDeNegocio
    {
        internal static void Negocio(ModelBuilder modelBuilder)
        {
            ApiDeNombreDtm.DefinirCampoNombreDtm<NegocioDtm>(modelBuilder, 250, "", true);

            modelBuilder.Entity<NegocioDtm>().Property(p => p.Activo).HasColumnName(ICampos.ACTIVO).HasColumnType(IDominio.BIT).IsRequired();
            modelBuilder.Entity<NegocioDtm>().Property(p => p.ElementoDtm).HasColumnName(ICampos.ELEMENTO_DTM).HasColumnType(IDominio.VARCHAR_250).IsRequired();
            modelBuilder.Entity<NegocioDtm>().Property(p => p.ElementoDto).HasColumnName(ICampos.ELEMENTO_DTO).HasColumnType(IDominio.VARCHAR_250).IsRequired(false);
            modelBuilder.Entity<NegocioDtm>().Property(p => p.Icono).HasColumnName(ICampos.ICONO).HasColumnType(IDominio.VARCHAR_250).IsRequired(false);

            modelBuilder.Entity<NegocioDtm>().Property(p => p.UsaSeguridad).HasColumnName(ICampos.USA_SEGURIDAD).HasColumnType(IDominio.BIT).IsRequired();
            modelBuilder.Entity<NegocioDtm>().Property(p => p.EsDeParametrizacion).HasColumnName(ICampos.ES_DE_PARAMETRIZACION).HasColumnType(IDominio.BIT).IsRequired();
            modelBuilder.Entity<NegocioDtm>().Property(p => p.UsaCentroGestor).HasColumnName(ICampos.USA_CG).HasColumnType(IDominio.BIT).IsRequired();

            modelBuilder.Entity<NegocioDtm>().Property(p => p.Enumerado).HasColumnName(ICampos.ENUMERADO).HasColumnType(IDominio.NEGOCIO_ENUMERADO).IsRequired();

            ApiDeRegistroDtm.DefinirCampoFk<NegocioDtm>(modelBuilder, nameof(NegocioDtm.Administrador), nameof(NegocioDtm.IdAdministrador), ICampos.ID_ADM, requerida: true, unico: false);
            ApiDeRegistroDtm.DefinirCampoFk<NegocioDtm>(modelBuilder, nameof(NegocioDtm.Gestor), nameof(NegocioDtm.IdGestor), ICampos.ID_GESTOR, requerida: true, unico: false);
            ApiDeRegistroDtm.DefinirCampoFk<NegocioDtm>(modelBuilder, nameof(NegocioDtm.Consultor), nameof(NegocioDtm.IdConsultor), ICampos.ID_CONSULTOR, requerida: true, unico: false);

        }


        internal static void DefinirCamposDePlantillaDeNegocioDtm(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PlantillaDeNegocioDtm>().Ignore(nameof(PlantillaDeNegocioDtm.NombrePa));
            modelBuilder.Entity<PlantillaDeNegocioDtm>().Ignore(nameof(PlantillaDeNegocioDtm.fichero));

            ApiDeRegistroDtm.DefinirCampoIdDtm<PlantillaDeNegocioDtm>(modelBuilder);
            ApiDeNombreDtm.DefinirCampoNombreDtm<PlantillaDeNegocioDtm>(modelBuilder, unico: true);
            ApiDeElementoDtm.DefinirCampoArchivo<PlantillaDeNegocioDtm>(modelBuilder, obligatorio: true, unico: true);
            modelBuilder.Entity<PlantillaDeNegocioDtm>().Property(x => ((IPlantillaDeNegocio)x).IdPermiso).HasColumnType(IDominio.INT).HasColumnName(ICampos.ID_PERMISO).IsRequired();
            ApiDeRegistroDtm.DefinirCampoFk<PlantillaDeNegocioDtm>(modelBuilder, nameof(IPlantillaDeNegocio.Permiso), nameof(IPlantillaDeNegocio.IdPermiso), ICampos.ID_PERMISO, requerida: true, unico: true);
            ApiDeRegistroDtm.DefinirCampoFk<PlantillaDeNegocioDtm>(modelBuilder, nameof(IPlantillaDeNegocio.Accion), nameof(IPlantillaDeNegocio.IdAccion), ICampos.ID_ACCION, requerida: true, unico: false);
            DefinirCampoNegocio<PlantillaDeNegocioDtm>(modelBuilder, unico: false);
        }

        internal static void DefinirCampoNegocio<TEntity>(ModelBuilder modelBuilder, bool unico) where TEntity : RegistroDtm
        {
            if (!typeof(TEntity).ImplementaTieneNegocio())
                GestorDeErrores.Emitir($"La entidad {typeof(TEntity).Name} debe implementar la interface {typeof(ITieneCampoNegocio).Name}");

            ApiDeRegistroDtm.DefinirCampoFk<TEntity>(modelBuilder, nameof(ITieneCampoNegocio.Negocio), nameof(ITieneCampoNegocio.IdNegocio), ICampos.ID_NEGOCIO, requerida: true, unico: unico);
        }
    }



}
