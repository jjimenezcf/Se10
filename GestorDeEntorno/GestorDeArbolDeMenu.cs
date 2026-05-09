using System.Collections.Generic;
using AutoMapper;
using ModeloDeDto.Entorno;
using ServicioDeDatos;
using ServicioDeDatos.Entorno;

namespace Gestor.Elementos.Entorno
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


        internal static GestorDeArbolDeMenu Gestor(IMapper mapeador)
        {
            var contexto = ContextoSe.ObtenerContexto();
            return (GestorDeArbolDeMenu)CrearGestor<GestorDeArbolDeMenu>(() => new GestorDeArbolDeMenu(contexto, mapeador));
        }

        protected override void DespuesDeMapearElemento(ArbolDeMenuDtm registro, ArbolDeMenuDto elemento, ParametrosDeMapeo parametros)
        {
            base.DespuesDeMapearElemento(registro, elemento, parametros);
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

        private static List<ArbolDeMenuDto> CacheArbolDeMenu;

        public List<ArbolDeMenuDto> LeerArbolDeMenu()
        {
            if (CacheArbolDeMenu == null)
            {
                var resultadoDto = new List<ArbolDeMenuDto>();
                List<ArbolDeMenuDtm> arbolDeMenu = LeerRegistros(0, -1);
                procesarSubMenus(resultadoDto, arbolDeMenu, padre: null);
                CacheArbolDeMenu = resultadoDto;
            }
            return CacheArbolDeMenu;
        }


        internal void LimpiarCacheDeArbolDeMenu()
        {
            CacheArbolDeMenu = null;
        }

        private void procesarSubMenus(List<ArbolDeMenuDto> resultadoDto, List<ArbolDeMenuDtm> arbolDeMenu, ArbolDeMenuDto padre)
        {
            List<ArbolDeMenuDtm> procesarMenus = MenusParaProcesar(arbolDeMenu, padre);
            if (procesarMenus.Count == 0)
                return;

            foreach (var menuDtm in procesarMenus)
            {
                var menuDto = MapearElemento(menuDtm);
                if (padre != null)
                    padre.Submenus.Add(menuDto);

                resultadoDto.Add(menuDto);
                if (menuDtm.IdVistaMvc == null)
                {
                    procesarSubMenus(resultadoDto, arbolDeMenu, padre: menuDto);
                }
            }
        }


        private List<ArbolDeMenuDtm> MenusParaProcesar(List<ArbolDeMenuDtm> arbolDeMenu, ArbolDeMenuDto padre)
        {
            var resultado = new List<ArbolDeMenuDtm>();
            var procesar = new List<ArbolDeMenuDtm>();

            foreach (var nodo in arbolDeMenu)
                if ((nodo.IdPadre == null && padre == null) || (padre != null && nodo.IdPadre == padre.Id))
                    procesar.Add(nodo);

            if (procesar.Count == 0)
                return resultado;

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
