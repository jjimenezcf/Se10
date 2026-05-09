using AutoMapper;
using Gestor.Errores;
using GestorDeElementos;
using ModeloDeDto.Entorno;
using ServicioDeDatos;
using ServicioDeDatos.Entorno;
using System.Collections.Generic;
using Utilidades;

namespace GestoresDeNegocio.Entorno
{
    public class GestorDeArbolDeMenu : GestorDeElementos<ContextoSe, ArbolDeMenuDtm, ArbolDeMenuDto>
    {
        public class MapearMenus : Profile
        {
            public MapearMenus()
            {
                CreateMap<ArbolDeMenuDtm, ArbolDeMenuDto>();
            }
        }

        public GestorDeArbolDeMenu(ContextoSe contexto, IMapper mapeador)
            : base(contexto, mapeador)
        {

        }

        protected override void DespuesDeMapearElElemento(ArbolDeMenuDtm registro, ArbolDeMenuDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(registro, elemento, parametros);
            elemento.Submenus = new List<ArbolDeMenuDto>();
            elemento.VistaMvc = new VistaMvcDto
            {
                Id = registro.IdVistaMvc.GetValueOrDefault(),
                Nombre = registro.Vista,
                Controlador = registro.Controlador,
                Accion = registro.accion,
                Parametros = registro.parametros
            };
        }

        public List<ArbolDeMenuDto> LeerArbolDeMenu(string usuario)
        {
            var gestor = GestorDeUsuarios.Gestor(Contexto, Mapeador);
            var cache = ServicioDeCaches.Obtener(CacheDe.Ent_UsuariosPorLogin);
            if (!cache.ContainsKey(usuario))
            {
                var usuarioDtm = gestor.LeerRegistro(nameof(UsuarioDtm.Login), usuario, false, false, false, false);
                if (usuarioDtm == null)
                {
                    GestorDeErrores.Emitir($"Usuario {usuario} no válido");
                }
                cache[usuario] = usuarioDtm;
            }
            Contexto.AsignarUsuario((UsuarioDtm)cache[usuario]);

            var CacheArbolDeMenu = ServicioDeCaches.Obtener(nameof(ArbolDeMenuSql.LeerArbolDeMenu));

            if (!CacheArbolDeMenu.ContainsKey(((UsuarioDtm)cache[usuario]).Id.ToString()))
            {
                var arbolDeMenu = ArbolDeMenuSql.LeerArbolDeMenu(Contexto);

                var resultadoDto = new List<ArbolDeMenuDto>();
                ProcesarSubMenus(resultadoDto, arbolDeMenu, padre: null);
                CacheArbolDeMenu[((UsuarioDtm)cache[usuario]).Id.ToString()] = resultadoDto;
            }
            return (List<ArbolDeMenuDto>)CacheArbolDeMenu[((UsuarioDtm)cache[usuario]).Id.ToString()];
        }


        public static void LimpiarCacheDeArbolDeMenu()
        {
            ServicioDeCaches.EliminarCache(nameof(ArbolDeMenuSql.LeerArbolDeMenu));
            ServicioDeCaches.EliminarCache(CacheDe.ArbolDeMenu);
        }

        private void ProcesarSubMenus(List<ArbolDeMenuDto> resultadoDto, List<ArbolDeMenuDtm> arbolDeMenu, ArbolDeMenuDto padre)
        {
            List<ArbolDeMenuDtm> procesarMenus = MenusParaProcesar(arbolDeMenu, padre);
            if (procesarMenus.Count == 0)
            {
                return;
            }

            foreach (var menuDtm in procesarMenus)
            {
                var menuDto = MapearElemento(menuDtm);
                if (padre != null)
                {
                    padre.Submenus.Add(menuDto);
                }

                resultadoDto.Add(menuDto);
                if (menuDtm.IdVistaMvc == null)
                {
                    ProcesarSubMenus(resultadoDto, arbolDeMenu, padre: menuDto);
                }
            }
        }


        private List<ArbolDeMenuDtm> MenusParaProcesar(List<ArbolDeMenuDtm> arbolDeMenu, ArbolDeMenuDto padre)
        {
            var resultado = new List<ArbolDeMenuDtm>();
            var procesar = new List<ArbolDeMenuDtm>();

            foreach (var nodo in arbolDeMenu)
            {
                if ((nodo.IdPadre == null && padre == null) || (padre != null && nodo.IdPadre == padre.Id))
                {
                    procesar.Add(nodo);
                }
            }

            if (procesar.Count == 0)
            {
                return resultado;
            }

            while (procesar.Count > 0)
            {
                var orden = procesar[0].Orden;
                var quitar = 0;
                for (var i = 0; i < procesar.Count; i++)
                {
                    if (procesar[i].Orden <= orden)
                    {
                        orden = procesar[i].Orden;
                        quitar = i;
                    }
                }
                resultado.Add(procesar[quitar]);
                procesar.RemoveAt(quitar);
            }

            return resultado;
        }

    }
}
