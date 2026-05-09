using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Utilidades;
using ServicioDeDatos;
using ServicioDeDatos.Entorno;
using ModeloDeDto.Entorno;

namespace Gestor.Elementos.Entorno
{
    public static partial class Joins
    {
        public static IQueryable<T> JoinConMenus<T>(this IQueryable<T> registros, List<ClausulaDeJoin> joins, ParametrosDeNegocio parametros) where T : MenuDtm
        {
            foreach (ClausulaDeJoin join in joins)
            {
                if (join.Dtm == typeof(MenuDtm))
                    registros = registros.Include(p => p.Padre);
                if (join.Dtm == typeof(VistaMvcDtm))
                    registros = registros.Include(p => p.VistaMvc);
            }

            return registros;
        }
    }

    public static partial class Filtros
    {
        public static IQueryable<T> FiltrarMenus<T>(this IQueryable<T> registros, List<ClausulaDeFiltrado> filtros) where T : MenuDtm
        {
            foreach (ClausulaDeFiltrado filtro in filtros)
                if (filtro.Clausula.ToLower() == nameof(MenuDtm.IdPadre).ToLower())
                {
                    if (filtro.Criterio == CriteriosDeFiltrado.esNulo)
                        registros = registros.Where(x => x.IdPadre == null);

                    if (filtro.Criterio == CriteriosDeFiltrado.noEsNulo)
                        registros = registros.Where(x => x.IdPadre != null);

                    if (filtro.Criterio == CriteriosDeFiltrado.igual && filtro.Valor.Entero() > 0)
                        registros = registros.Where(x => x.IdPadre == filtro.Valor.Entero());
                }

            return registros;
        }

        public static IQueryable<T> FiltrarPorMenuPadre<T>(this IQueryable<T> registros, List<ClausulaDeFiltrado> filtros) where T : MenuDtm
        {
            foreach (ClausulaDeFiltrado filtro in filtros)
                if (filtro.Clausula.ToLower() == nameof(MenuDtm.Padre).ToLower())
                {
                    registros = registros.Where(x => x.IdPadre == filtro.Valor.Entero());
                }

            return registros;
        }
        public static IQueryable<T> FiltrarPorActivo<T>(this IQueryable<T> registros, List<ClausulaDeFiltrado> filtros) where T : MenuDtm
        {
            foreach (ClausulaDeFiltrado filtro in filtros)
                if (filtro.Clausula.ToLower() == nameof(MenuDtm.Activo).ToLower())
                {
                    registros = registros.Where(x => x.Activo == bool.Parse(filtro.Valor));
                }

            return registros;
        }
    }

    public static partial class Ordenaciones
    {
        public static IQueryable<T> OrdenarMenus<T>(this IQueryable<T> registros, List<ClausulaDeOrdenacion> ordenacion) where T : MenuDtm
        {
            foreach (ClausulaDeOrdenacion orden in ordenacion)
            {
                if (orden.Propiedad.ToLower() == nameof(MenuDtm.Padre).ToLower())
                {
                    registros = orden.Modo == ModoDeOrdenancion.ascendente
                    ? registros.OrderBy(x => x.Padre.Orden)
                    : registros.OrderByDescending(x => x.Padre.Orden);

                    break;
                }

                if (orden.Propiedad.ToLower() == nameof(MenuDtm.Nombre).ToLower())
                {
                    registros = orden.Modo == ModoDeOrdenancion.ascendente
                    ? registros.OrderBy(x => x.Padre).ThenBy(x => x.Nombre)
                    : registros.OrderBy(x => x.Padre).ThenByDescending(x => x.Nombre);
                    break;
                }

                if (orden.Propiedad.ToLower() == nameof(MenuDtm.Orden).ToLower())
                {
                    registros = orden.Modo == ModoDeOrdenancion.ascendente
                    ? registros.OrderBy(x => x.Padre).ThenBy(x => x.Orden).ThenBy(x=>x.Nombre)
                    : registros.OrderBy(x => x.Padre).ThenByDescending(x => x.Orden).ThenBy(x => x.Nombre);
                    break;
                }
            }

            return registros;
        }
    }

    public class GestorDeMenus : GestorDeElementos<ContextoSe, MenuDtm, MenuDto>
    {
        public class MapearMenus : Profile
        {
            public MapearMenus()
            {
                CreateMap<MenuDtm, MenuDto>()
                .ForMember(dto => dto.Padre, dtm => dtm.MapFrom(dtm => dtm.Padre.Nombre))
                .ForMember(dto => dto.VistaMvc, dtm => dtm.MapFrom(dtm => dtm.VistaMvc.Nombre));

                CreateMap<MenuDto, MenuDtm>()
                    .ForMember(dtm => dtm.IdVistaMvc, dto => dto.MapFrom(dto => dto.idVistaMvc == 0 ? null : dto.idVistaMvc))
                    .ForMember(dtm => dtm.IdPadre, dto => dto.MapFrom(dto => dto.idPadre == 0 ? null : dto.idPadre));
            }
        }

        public GestorDeMenus(ContextoSe contexto, IMapper mapeador)
            : base(contexto, mapeador)
        {

        }

        protected override void DefinirJoins(List<ClausulaDeFiltrado> filtros, List<ClausulaDeJoin> joins, ParametrosDeNegocio parametros)
        {
            base.DefinirJoins(filtros, joins, parametros);

            foreach (var filtro in filtros)
                if (filtro.Clausula == nameof(MenuDtm.IdPadre) && filtro.Criterio == CriteriosDeFiltrado.esNulo)
                    return;

            joins.Add(new ClausulaDeJoin { Dtm = typeof(MenuDtm) });
            joins.Add(new ClausulaDeJoin { Dtm = typeof(VistaMvcDtm) });
        }

        protected override IQueryable<MenuDtm> AplicarFiltros(IQueryable<MenuDtm> registros, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            registros = base.AplicarFiltros(registros, filtros, parametros);

            if (HayFiltroPorId(registros))
                return registros;

            registros = registros
                .FiltrarMenus(filtros)
                .FiltrarPorMenuPadre(filtros)
                .FiltrarPorActivo(filtros);

            return registros;
        }

        protected override IQueryable<MenuDtm> AplicarOrden(IQueryable<MenuDtm> registros, List<ClausulaDeOrdenacion> ordenacion)
        {
            registros = base.AplicarOrden(registros, ordenacion);
            return registros.OrdenarMenus(ordenacion);
        }

        protected override IQueryable<MenuDtm> AplicarJoins(IQueryable<MenuDtm> registros, List<ClausulaDeJoin> joins, ParametrosDeNegocio parametros)
        {
            return registros.JoinConMenus(joins, parametros);
        }

        public List<MenuDto> LeerPadres()
        {
            var registros = Contexto
                            .Menus
                            .FromSqlInterpolated($@"select 
                                                      t1.ID
                                                    , case
                                                         WHEN t2.Nombre is null THEN t1.nombre
                                                         ELSE t2.nombre+'.'+t1.nombre
                                                      END as NOMBRE
                                                    , t1.DESCRIPCION
                                                    , t1.icono
                                                    , t1.ACTIVO
                                                    , t1.IDPADRE
                                                    , t1.IDVISTA_MVC
                                                    , T1.ORDEN
                                                    from entorno.MENU_SE t1
                                                    left join entorno.menu t2 on t2.id = t1.IDPADRE
                                                    where vista is null
                                                    order by t1.IDPADRE, T1.ORDEN, T1.NOMBRE")
                            .ToList();

            var elementos = MapearElementos(registros).ToList();
            return elementos;
        }

        public static List<ArbolDeMenuDto> LeerArbolDeMenu(IMapper mapeador)
        {
            var gestor = GestorDeArbolDeMenu.Gestor(mapeador);

            return gestor.LeerArbolDeMenu();
        }

        protected override void DespuesDePersistir(MenuDtm registro, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(registro, parametros);
            var gestor = GestorDeArbolDeMenu.Gestor(Mapeador);
            gestor.LimpiarCacheDeArbolDeMenu();
        }

        public List<VistaMvcDto> LeerVistas(int posicion, int cantidad, string valorDeFiltro)
        {
            var gestor = GestorDeVistaMvc.Gestor(Mapeador);
            var filtros = new List<ClausulaDeFiltrado>();
            if (!valorDeFiltro.IsNullOrEmpty())
                filtros.Add(new ClausulaDeFiltrado { Criterio = CriteriosDeFiltrado.contiene, Clausula = nameof(VistaMvcDto.Nombre), Valor = valorDeFiltro });

            var clasesDtm = gestor.LeerRegistros(posicion, cantidad, filtros);
            return gestor.MapearElementos(clasesDtm).ToList();
        }
    }

}
