using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Utilidades;
using ServicioDeDatos;
using ServicioDeDatos.Entorno;
using ModeloDeDto.Entorno;
using GestorDeElementos;
using ModeloDeDto;
using System;
using Gestor.Errores;
using ModeloDeDto.Negocio;
using ServicioDeDatos.Elemento;
using Dapper;
using Newtonsoft.Json;
using ServicioDeDatos.Terceros;
using GestorDeElementos.Extensores;

namespace GestoresDeNegocio.Entorno
{
    public class GestorDeMenus : GestorDeElementos<ContextoSe, MenuDtm, MenuDto>
    {
        public override enumNegocio Negocio => enumNegocio.Menu;

        public class ltrDeMenus
        {
            public static readonly string mostrarJerarquia = nameof(mostrarJerarquia);
            public static string filtroPorNoActivo => MenuSql.Filtro.NoActivos;
            public static string filtroPorUsuario => MenuSql.Filtro.PorUsuario;
            public static string filtroPorPuesto => MenuSql.Filtro.PorPuesto;
            public static string filtroPorRol => MenuSql.Filtro.PorRol;
            public static string filtroPorVista => MenuSql.Filtro.PorVista;
        }

        public class MapearMenus : Profile
        {
            public MapearMenus()
            {
                CreateMap<MenuDtm, MenuDto>()
                .ForMember(dto => dto.Padre, dtm => dtm.MapFrom(dtm => dtm.Padre.Nombre))
                .ForMember(dto => dto.VistaMvc, dtm => dtm.MapFrom(dtm => dtm.VistaMvc.Nombre))
                ;

                CreateMap<MenuDto, MenuDtm>()
                .ForMember(dtm => dtm.IdVistaMvc, dto => dto.MapFrom(dto => dto.idVistaMvc == 0 ? null : dto.idVistaMvc))
                .ForMember(dtm => dtm.IdPadre, dto => dto.MapFrom(dto => dto.idPadre == 0 ? null : dto.idPadre))
                .ForMember(dtm => dtm.Padre, dto => dto.Ignore())
                .ForMember(dtm => dtm.VistaMvc, dto => dto.Ignore());
            }
        }

        public GestorDeMenus(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {

        }

        public static GestorDeMenus Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeMenus(contexto, mapeador);
        }

        public static JerarquiaDto LeerJerarquia(ContextoSe contexto, int? idPadre, string filtrosJson)
        {
            var filtros = filtrosJson.ToDiccionario();
            var gestor = Gestor(contexto, contexto.Mapeador);
            List<NodoDtm> tiposLeidosDtm = gestor.LeerJerarquiaDeMenus(idPadre, filtros);

            var mostrarJerarquia = (bool)filtros[ltrDeMenus.mostrarJerarquia];
            return mostrarJerarquia || MenuSql.HayFiltrosConSoloPermisos(filtros)
                ? ApiDeJerarquias.EstructurarJerarquica(enumNegocio.Menu, tiposLeidosDtm, typeof(MenuDto))
                : ApiDeJerarquias.EstructuraPlana(enumNegocio.Menu, tiposLeidosDtm, typeof(MenuDto));
        }

        public static MenuDto PersistirMenuJson(ContextoSe contexto, string cgJson, ParametrosDeNegocio parametros)
        {
            var cgDto = JsonConvert.DeserializeObject<MenuDto>(cgJson);
            var gestor = Gestor(contexto, contexto.Mapeador);
            return gestor.PersistirElementoDto(cgDto, parametros);
        }

        public static void ActualizarMenu(GestorDeMenus gestorDeMenu, string padre, string nombre, string nuevoNombre, string descripcion, string icono, string vista, int orden)
        {
            var menus = padre.Split(".");
            int idPadre = 0;
            foreach (var menu in menus)
            {
                if (menu.IsNullOrEmpty())
                    continue;

                var padresDtm = BuscarMenu(gestorDeMenu, menu, idPadre);
                if (padresDtm.Count == 0)
                    throw new Exception($"No está definido el padre {menu}, para el idPadre {idPadre}");

                idPadre = padresDtm[0].Id;
            }

            List<MenuDtm> menusDtm = BuscarMenu(gestorDeMenu, nombre, idPadre);

            if (menusDtm.Count == 1)
            {
                var menuDtm = menusDtm[0];
                menuDtm.Nombre = nuevoNombre.IsNullOrEmpty() ? nombre : nuevoNombre;
                menuDtm.Descripcion = descripcion;
                menuDtm.Icono = icono;
                menuDtm.Orden = orden;
                menuDtm.Activo = true;

                menuDtm.IdPadre = idPadre == 0 ? null : idPadre;

                if (!vista.IsNullOrEmpty())
                {
                    var gestorDeVista = GestorDeVistaMvc.Gestor(gestorDeMenu.Contexto, gestorDeMenu.Contexto.Mapeador);
                    var vistaDtm = gestorDeVista.LeerRegistro(nameof(VistaMvcDtm.Nombre), vista, true, true, false, aplicarJoin: false);
                    menuDtm.IdVistaMvc = vistaDtm.Id;
                }

                gestorDeMenu.PersistirRegistro(menuDtm, new ParametrosDeNegocio(enumTipoOperacion.Modificar));
            }
            else
                GestorDeErrores.Emitir($"Se han localizado {menusDtm.Count} registros para el menu {nombre} del padre {padre}");
        }

        public static void CrearMenuSiNoExiste(GestorDeMenus gestorDeMenu,
            string nombre,
            string descripcion,
            string icono,
            string padre,
            string vista,
            int orden,
            string parametros = null,
            bool buscarPorPadre = true)
        {
            int idPadre = 0;
            if (padre == null)
            {
                idPadre = 0;
            }
            else
            {
                var menus = padre.Split(".");
                foreach (var menu in menus)
                {
                    var padresDtm = BuscarMenu(gestorDeMenu, menu, idPadre);
                    if (padresDtm.Count == 0)
                        throw new Exception($"No está definido el padre {menu}, para el idPadre {idPadre}");

                    idPadre = padresDtm[0].Id;
                }
            }

            List<MenuDtm> menusDtm = BuscarMenu(gestorDeMenu, nombre, idPadre, buscarPorPadre);

            if (menusDtm.Count == 0)
            {
                CrearMenu(gestorDeMenu, nombre, descripcion, icono, vista, orden, parametros, idPadre);
            }
            else if (menusDtm.Count == 1)
            {
                ModificarMenu(gestorDeMenu, menusDtm[0], nombre, descripcion, icono, vista, orden, parametros, idPadre);
            }
            else
            {
                var desactivarMenus = menusDtm.Where(x => x.Nombre == nombre && idPadre != x.IdPadre && x.Activo);
                foreach (var menu in desactivarMenus)
                {
                    menu.Activo = false;
                    menu.Modificar(gestorDeMenu.Contexto);
                }
            }
        }

        private static void CrearMenu(GestorDeMenus gestorDeMenu, string nombre, string descripcion, string icono, string vista, int orden, string parametros, int idPadre)
        {
            var menuDtm = new MenuDtm();
            menuDtm.Nombre = nombre;
            menuDtm.Descripcion = descripcion;
            menuDtm.Icono = icono;
            menuDtm.Orden = orden;
            menuDtm.Activo = true;
            menuDtm.IdPadre = idPadre == 0 ? null : idPadre;
            menuDtm.Parametros = parametros;

            if (!vista.IsNullOrEmpty())
            {
                var gestorDeVista = GestorDeVistaMvc.Gestor(gestorDeMenu.Contexto, gestorDeMenu.Contexto.Mapeador);
                var vistaDtm = gestorDeVista.LeerRegistro(nameof(VistaMvcDtm.Nombre), vista, true, true, false, aplicarJoin: false);
                menuDtm.IdVistaMvc = vistaDtm.Id;
            }

            gestorDeMenu.PersistirRegistro(menuDtm, new ParametrosDeNegocio(enumTipoOperacion.Insertar));
        }
        private static void ModificarMenu(GestorDeMenus gestorDeMenu, MenuDtm menuDtm, string nombre, string descripcion, string icono, string vista, int orden, string parametros, int idPadre)
        {
            int? idVistaLeida = null;
            if (!vista.IsNullOrEmpty())
            {
                var gestorDeVista = GestorDeVistaMvc.Gestor(gestorDeMenu.Contexto, gestorDeMenu.Contexto.Mapeador);
                var vistaDtm = gestorDeVista.LeerRegistro(nameof(VistaMvcDtm.Nombre), vista, true, true, false, aplicarJoin: false);
                idVistaLeida = vistaDtm.Id;
            }

            if (menuDtm.Parametros.IsNullOrEmpty() && menuDtm.Parametros != null) menuDtm.Parametros = null;

            var modificada = false;
            modificada = modificada || menuDtm.Nombre != nombre;
            modificada = modificada || menuDtm.Descripcion != descripcion;
            modificada = modificada || menuDtm.Icono != icono;
            modificada = modificada || menuDtm.Orden != orden;
            modificada = modificada || menuDtm.IdPadre != idPadre;
            modificada = modificada || menuDtm.Parametros != parametros;
            modificada = modificada || menuDtm.IdVistaMvc != idVistaLeida;
            if (modificada)
            {
                menuDtm.Nombre = nombre.Trim();
                menuDtm.Descripcion = descripcion.Trim();
                menuDtm.Icono = icono.Trim();
                menuDtm.Orden = orden;
                menuDtm.Activo = menuDtm.Activo;
                menuDtm.IdPadre = idPadre == 0 ? null : idPadre;
                menuDtm.Parametros = parametros.IsNullOrEmpty() ? null : parametros.Trim();
                menuDtm.IdVistaMvc = idVistaLeida;

                gestorDeMenu.PersistirRegistro(menuDtm, new ParametrosDeNegocio(enumTipoOperacion.Modificar));
            }
        }

        private static List<MenuDtm> BuscarMenu(GestorDeMenus gestorDeMenu, string nombre, int idPadre, bool buscarPorPadre = true)
        {
            var filtros = new List<ClausulaDeFiltrado>();
            var filtro1 = new ClausulaDeFiltrado(nameof(MenuDtm.Nombre), enumCriteriosDeFiltrado.igual, nombre);
            filtros.Add(filtro1);
            if (buscarPorPadre)
            {
                var filtro2 = new ClausulaDeFiltrado(nameof(MenuDtm.IdPadre), enumCriteriosDeFiltrado.esNulo);
                var filtro3 = new ClausulaDeFiltrado(nameof(MenuDtm.IdPadre), enumCriteriosDeFiltrado.igual, idPadre.ToString());
                filtros.Add(idPadre == 0 ? filtro2 : filtro3);
            }
            List<MenuDtm> menusDtm = gestorDeMenu.LeerRegistros(0, -1, filtros);
            return menusDtm;
        }


        public List<NodoDtm> LeerJerarquiaDeMenus(int? idPadre, Dictionary<string, object> filtros)
        {
            var mostrarJerarquia = (bool)filtros[ltrDeMenus.mostrarJerarquia];
            var parametrosSql = new Dictionary<string, object>();

            var sentenciaSql = mostrarJerarquia || MenuSql.HayFiltrosConSoloPermisos(filtros)
                ? MenuSql.JerarquiaDeMenus.Replace("[Padre]", "")
                : MenuSql.JerarquiaDeMenus.Replace("[Padre]", $"iif(T2.{ICampos.NOMBRE} is NULL,'', T2.{ICampos.NOMBRE} + '.') +");

            sentenciaSql = MenuSql.AplicarFiltros(sentenciaSql, idPadre, filtros, parametrosSql);

            var consulta = new ConsultaSql<NodoDtm>(Contexto, sentenciaSql);
            var registros = consulta.LanzarConsulta(new DynamicParameters(parametrosSql));
            return registros;
        }


        protected override IQueryable<MenuDtm> AplicarOrden(IQueryable<MenuDtm> registros, List<ClausulaDeOrdenacion> ordenacion)
        {
            if (ordenacion.Count == 0)
            {
                registros = registros.OrderBy(x => x.IdPadre).ThenBy(x => x.Orden).ThenBy(x => x.Nombre);
                return registros;
            }

            foreach (ClausulaDeOrdenacion orden in ordenacion)
            {
                if (orden.OrdenarPor.ToLower() == nameof(MenuDtm.Id).ToLower())
                    return registros = OrdenPorId(registros, orden);

                if (orden.OrdenarPor == nameof(MenuDtm.IdPadre))
                {
                    registros = registros.OrderBy(x => x.IdPadre).ThenBy(x => x.Orden).ThenBy(x => x.Nombre);
                    break;
                }

                if (orden.OrdenarPor.ToLower() == nameof(MenuDtm.Padre).ToLower())
                {
                    registros = orden.Modo == ModoDeOrdenancion.ascendente
                    ? registros.OrderBy(x => x.Padre.Orden)
                    : registros.OrderByDescending(x => x.Padre.Orden);

                    break;
                }

                if (orden.OrdenarPor.ToLower() == nameof(MenuDtm.Nombre).ToLower())
                {
                    registros = orden.Modo == ModoDeOrdenancion.ascendente
                    ? registros.OrderBy(x => x.Padre).ThenBy(x => x.Nombre)
                    : registros.OrderBy(x => x.Padre).ThenByDescending(x => x.Nombre);
                    break;
                }

                if (orden.OrdenarPor.ToLower() == nameof(MenuDtm.Orden).ToLower())
                {
                    registros = orden.Modo == ModoDeOrdenancion.ascendente
                    ? registros.OrderBy(x => x.Padre).ThenBy(x => x.Orden).ThenBy(x => x.Nombre)
                    : registros.OrderBy(x => x.Padre).ThenByDescending(x => x.Orden).ThenBy(x => x.Nombre);
                    break;
                }
            }

            return registros;
        }

        protected override IQueryable<MenuDtm> AplicarJoins(IQueryable<MenuDtm> registros, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            registros = base.AplicarJoins(registros, filtros, parametros);

            foreach (var filtro in filtros)
                if (filtro.Clausula == nameof(MenuDtm.IdPadre) && filtro.Criterio == enumCriteriosDeFiltrado.esNulo)
                    return registros;

            registros = registros.Include(p => p.Padre);
            registros = registros.Include(p => p.VistaMvc);

            return registros;
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
                                                    --, T1.IDPERMISO
                                                    from entorno.MENU_SE t1
                                                    left join entorno.menu t2 on t2.id = t1.IDPADRE
                                                    where vista is null
                                                    order by t1.IDPADRE, T1.ORDEN, T1.NOMBRE")
                            .ToList();

            var elementos = MapearElementos(registros).ToList();
            return elementos;
        }

        public List<MenuDto> LeerMenus(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros)
        {
            List<ClausulaDeOrdenacion> orden = new List<ClausulaDeOrdenacion>();
            orden.Add(new ClausulaDeOrdenacion() { OrdenarPor = nameof(MenuDto.idPadre), Modo = ModoDeOrdenancion.ascendente });

            var registros = LeerRegistros(posicion, cantidad, filtros, orden);
            return MapearElementos(registros).ToList();
        }

        protected override void DespuesDePersistir(MenuDtm registro, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(registro, parametros);
            GestorDeArbolDeMenu.LimpiarCacheDeArbolDeMenu();
            ServicioDeCaches.EliminarCache(ServicioDeCaches.PorId(typeof(MenuDtm).FullName));
            ServicioDeCaches.EliminarCache(ServicioDeCaches.PorNombre(typeof(MenuDtm).FullName));
            ServicioDeCaches.EliminarCache(ServicioDeCaches.PorAk(typeof(MenuDtm).FullName));
            ServicioDeCaches.EliminarCache(ServicioDeCaches.PorPropiedad(typeof(MenuDtm).FullName));
        }

        public void SeleccionarIa(enumIa ia)
        {
            ExtensorDeUsuarios.GuardarIaUsada(Contexto, ia);
            ServicioDeCaches.EliminarCache(CacheDe.RenderCrud);
        }
    }

}
