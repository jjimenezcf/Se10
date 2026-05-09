using AutoMapper;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Negocio;
using Microsoft.EntityFrameworkCore;
using ModeloDeDto.SistemaDocumental;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.SistemaDocumental;
using System.Collections.Generic;
using System.Linq;
using Utilidades;

namespace GestoresDeNegocio.SistemaDocumental
{
    public class GestorDeTiposDeArchivadores : GestorDeTiposDeElemento<ContextoSe, TipoDeArchivadorDtm, TipoDeArchivadorDto>
    {
        public class ltrDeUnTipoDeArchivador
        {

        }

        public class MapearTipoDeArchivador : MapearTipoDeElemento
        {
            public MapearTipoDeArchivador()
            {
                ReglasDeMapeoDelDtmAlDto(CreateMap<TipoDeArchivadorDtm, TipoDeArchivadorDto>())
                .ForMember(dto => dto.Padre, dtm => dtm.MapFrom(x => x.Padre.Expresion))
                .ForMember(dto => dto.Negocio, dtm => enumLiteralesDeNegocio.Plural(enumNegocio.Archivador));

                ReglasDeMapeoDelDtoAlDtm(CreateMap<TipoDeArchivadorDto, TipoDeArchivadorDtm>())
                .ForMember(dtm => dtm.Padre, dto => dto.Ignore());
            }
        }


        public GestorDeTiposDeArchivadores(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador, enumNegocio.Archivador)
        {

        }

        public static GestorDeTiposDeArchivadores Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeTiposDeArchivadores(contexto, mapeador);
        }

        public static int Cfg_Id_Tipo_De_Archivador_De_BackUp(ContextoSe contexto)
        {
            var nombre = CacheDeVariable.Cfg_Tipo_De_Archivador_De_BackUp;
            var cache = ServicioDeCaches.Obtener(CacheDe.Valores);
            if (!cache.ContainsKey(nameof(Cfg_Id_Tipo_De_Archivador_De_BackUp)))
            {
                var idUsuario = contexto.DatosDeConexion.IdUsuario;
                contexto.AsignarUsuario(contexto.Administrador());
                try
                {
                    var tipo = PersistirTipo(contexto, CacheDeVariable.Cfg_Tipo_De_Archivador_De_BackUp, enumClaseDeLibro.POR_CG_TIPO, "BAK", visible: false, delSistema: true, nombreModificable: false);
                    cache[nameof(Cfg_Id_Tipo_De_Archivador_De_BackUp)] = tipo.Id;
                }
                finally
                {
                    contexto.AsignarUsuario(contexto.SeleccionarPorId<UsuarioDtm>(idUsuario));
                }
            }
            return (int)cache[nameof(Cfg_Id_Tipo_De_Archivador_De_BackUp)];
        }

        public static int Cfg_Id_Tipo_De_Archivador_Basico(ContextoSe contexto)
        {
            var nombre = CacheDeVariable.Cfg_Tipo_De_Archivador_Basico;
            var cache = ServicioDeCaches.Obtener(CacheDe.Valores);
            if (!cache.ContainsKey(nameof(Cfg_Id_Tipo_De_Archivador_Basico)))
            {
                var idUsuario = contexto.DatosDeConexion.IdUsuario;
                contexto.AsignarUsuario(contexto.Administrador());
                try
                {
                    var tipo = PersistirTipo(contexto, CacheDeVariable.Cfg_Tipo_De_Archivador_Basico, enumClaseDeLibro.POR_CG_TIPO, "GEN", visible: false);
                    cache[nameof(Cfg_Id_Tipo_De_Archivador_Basico)] = tipo.Id;
                }
                finally
                {
                    contexto.AsignarUsuario(contexto.SeleccionarPorId<UsuarioDtm>(idUsuario));
                }
            }
            return (int)cache[nameof(Cfg_Id_Tipo_De_Archivador_Basico)];
        }

        public static int Cfg_Id_Tipo_De_Archivador_Zip(ContextoSe contexto)
        {
            var nombre = CacheDeVariable.Cfg_Tipo_De_Archivador_Zip;
            var cache = ServicioDeCaches.Obtener(CacheDe.Valores);
            if (!cache.ContainsKey(nameof(Cfg_Id_Tipo_De_Archivador_Zip)))
            {
                var idUsuario = contexto.DatosDeConexion.IdUsuario;
                contexto.AsignarUsuario(contexto.Administrador());
                try
                {
                    var tipo = PersistirTipo(contexto, CacheDeVariable.Cfg_Tipo_De_Archivador_Zip, enumClaseDeLibro.POR_CG_TIPO, "ZIP", visible: false, delSistema: true, nombreModificable: false);
                    cache[nameof(Cfg_Id_Tipo_De_Archivador_Zip)] = tipo.Id;
                }
                finally
                {
                    contexto.AsignarUsuario(contexto.SeleccionarPorId<UsuarioDtm>(idUsuario));
                }
            }
            return (int)cache[nameof(Cfg_Id_Tipo_De_Archivador_Zip)];
        }

        public static int Cfg_Id_Tipo_De_Archivador_De_Exportacion(ContextoSe contexto)
        {
            var nombre = CacheDeVariable.Cfg_Tipo_De_Archivador_De_Exportacion;
            var cache = ServicioDeCaches.Obtener(CacheDe.Valores);
            if (!cache.ContainsKey(nameof(Cfg_Id_Tipo_De_Archivador_De_Exportacion)))
            {
                var idUsuario = contexto.DatosDeConexion.IdUsuario;
                contexto.AsignarUsuario(contexto.Administrador());
                try
                {
                    var tipo = PersistirTipo(contexto, CacheDeVariable.Cfg_Tipo_De_Archivador_De_Exportacion, enumClaseDeLibro.POR_CG_TIPO, "XLS", visible: false, delSistema: true, nombreModificable: false);
                    cache[nameof(Cfg_Id_Tipo_De_Archivador_De_Exportacion)] = tipo.Id;
                }
                finally
                {
                    contexto.AsignarUsuario(contexto.SeleccionarPorId<UsuarioDtm>(idUsuario));
                }
            }
            return (int)cache[nameof(Cfg_Id_Tipo_De_Archivador_De_Exportacion)];
        }
        public static TipoDeArchivadorDtm PersistirTipo(ContextoSe contexto, string nombre, enumClaseDeLibro clase, string sigla, bool visible, bool delSistema = false, bool nombreModificable = true, int idPadre = 0, bool permiteCrear = true)
        {
            var gestor = Gestor(contexto, contexto.Mapeador);
            var leido = gestor.LeerRegistroCacheado(nameof(TipoDeElementoDtm.Nombre), nombre, false, false, false);
            if (leido == null)
            {
                var tipo = new TipoDeArchivadorDtm();
                tipo.Nombre = nombre;
                tipo.ClaseDeLibro = clase;
                tipo.Sigla = sigla;
                tipo.Visible = visible;
                tipo.DelSistema = delSistema;
                tipo.NombreModificable = nombreModificable;
                tipo.PermiteCrear = permiteCrear;
                if (idPadre > 0) tipo.IdPadre = idPadre;
                tipo = gestor.PersistirRegistro(tipo, new ParametrosDeNegocio(enumTipoOperacion.Insertar));
                return gestor.LeerRegistroPorId(tipo.Id, false, false, false, true);
            }

            if (leido.Nombre != nombre ||
                leido.Sigla != sigla ||
                leido.ClaseDeLibro != clase ||
                leido.Visible != visible ||
                idPadre != leido.IdPadre.Entero() ||
                delSistema != leido.DelSistema ||
                nombreModificable != leido.NombreModificable ||
                permiteCrear != leido.PermiteCrear)
            {
                leido.Nombre = nombre; leido.Sigla = sigla; leido.ClaseDeLibro = clase; leido.Visible = visible;
                leido.IdPadre = idPadre; leido.DelSistema = delSistema; leido.NombreModificable = nombreModificable;
                leido.PermiteCrear = permiteCrear;
                gestor.PersistirRegistro(leido, new ParametrosDeNegocio(enumTipoOperacion.Modificar));
            }
            return leido;
        }

        protected override IQueryable<TipoDeArchivadorDtm> AplicarJoins(IQueryable<TipoDeArchivadorDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            return consulta.Include(x => x.Padre);
        }

        protected override IQueryable<TipoDeArchivadorDtm> AplicarFiltros(IQueryable<TipoDeArchivadorDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarFiltros(consulta, filtros, parametros);
            consulta = consulta.FiltroDeTiposCopiables(Contexto, filtros);
            consulta = consulta.FiltroParaExcluirLosDelSistema(Contexto, parametros.CargarListaDinamica && filtros.Any(f => f.Clausula.ToLower() == ltrParametrosNeg.SoloEnAlta.ToLower() && f.Valor.EsTrue()));
            return consulta;
        }

    }
}
