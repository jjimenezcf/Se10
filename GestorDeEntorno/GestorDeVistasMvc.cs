using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Utilidades;
using ModeloDeDto.Entorno;
using ServicioDeDatos;
using ServicioDeDatos.Entorno;
using System;
using Gestor.Errores;
using ServicioDeDatos.Seguridad;
using ModeloDeDto.Seguridad;

namespace Gestor.Elementos.Entorno
{

    public static partial class Filtros
    {
        public static IQueryable<T> FiltraPorControlador<T>(this IQueryable<T> registros, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros) where T : VistaMvcDtm
        {
            foreach (ClausulaDeFiltrado filtro in filtros)
                if (filtro.Clausula.ToLower() == nameof(VistaMvcDtm.Controlador).ToLower())
                {
                    if (filtro.Criterio == CriteriosDeFiltrado.igual)
                        registros = registros.Where(x => x.Controlador == filtro.Valor);

                    if (filtro.Criterio == CriteriosDeFiltrado.contiene)
                        registros = registros.Where(x => x.Controlador.Contains(filtro.Valor));
                }

            return registros;
        }
        public static IQueryable<T> FiltraPorAccion<T>(this IQueryable<T> registros, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros) where T : VistaMvcDtm
        {
            foreach (ClausulaDeFiltrado filtro in filtros)
                if (filtro.Clausula.ToLower() == nameof(VistaMvcDtm.Accion).ToLower())
                {
                    if (filtro.Criterio == CriteriosDeFiltrado.igual)
                        registros = registros.Where(x => x.Accion == filtro.Valor);
                }

            return registros;
        }
    }


    public class GestorDeVistaMvc : GestorDeElementos<ContextoSe, VistaMvcDtm, VistaMvcDto>
    {

        public class MapearVistaMvc : Profile
        {
            public MapearVistaMvc()
            {
                CreateMap<VistaMvcDtm, VistaMvcDto>()
                .ForMember(dto => dto.Menus , dtm => dtm.MapFrom(x => x.Menus))
                //.ForMember("Permiso", dtm => dtm.MapFrom(x => x.Permiso))
                ;

                CreateMap<VistaMvcDto, VistaMvcDtm>();
            }
        }

        public GestorDeVistaMvc(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {

        }


        public static GestorDeVistaMvc Gestor(IMapper mapeador)
        {
            var contexto = ContextoSe.ObtenerContexto();
            return (GestorDeVistaMvc)CrearGestor<GestorDeVistaMvc>(() => new GestorDeVistaMvc(contexto, mapeador));
        }

        protected override IQueryable<VistaMvcDtm> AplicarFiltros(IQueryable<VistaMvcDtm> registros, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            registros = base.AplicarFiltros(registros, filtros, parametros);

            if (HayFiltroPorId(registros))
                return registros;

            return registros.FiltraPorControlador(filtros, parametros).FiltraPorAccion(filtros, parametros);
        }

        public VistaMvcDtm LeerVistaMvc(string vistaMvc)
        {
            if (vistaMvc.IsNullOrEmpty())
                return null;

            var partes = vistaMvc.Split(".");

            if (partes.Length != 2)
                GestorDeErrores.Emitir($"El valor proporcionado {vistaMvc} no es válido, ha de seguir el patrón Controlador.Vista");


            var filtros = new List<ClausulaDeFiltrado>
                {
                    new ClausulaDeFiltrado { Clausula = nameof(VistaMvcDtm.Controlador), Criterio = CriteriosDeFiltrado.igual, Valor = partes[0] },
                    new ClausulaDeFiltrado { Clausula = nameof(VistaMvcDtm.Accion), Criterio = CriteriosDeFiltrado.igual, Valor = partes[1] }
                };

            var vistas = LeerRegistros(0, -1, filtros);
            if (vistas.Count != 1)
            {
                //if (vistas.Count == 0)
                //    GestorDeErrores.Emitir($"No se ha localizado la vistaMvc {partes[0]}.{partes[1]}");
                //else
                //    GestorDeErrores.Emitir($"Se han localizado {vistas.Count} vistasMvc para {partes[0]}.{partes[1]}");
                return null;
            }

            return vistas[0];
        }

        protected override void AntesDePersistir(VistaMvcDtm registro, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(registro, parametros);
        }

        protected override void DespuesDePersistir(VistaMvcDtm registro, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(registro, parametros);
            var clasePermiso = ObtenerTabla();
            if (parametros.Tipo == TipoOperacion.Insertar) 
                CrearPermiso(clasePermiso, registro.Nombre);
            if (parametros.Tipo == TipoOperacion.Modificar)
                ModificarPermiso(clasePermiso, registro.Nombre);
            if (parametros.Tipo == TipoOperacion.Eliminar)
                BorrarPermiso(clasePermiso, registro.Nombre);
        }

        private void BorrarPermiso(string clasePermiso, string nombre)
        {
            throw new NotImplementedException();
        }

        private void ModificarPermiso(string clasePermiso, string nombre)
        {
            throw new NotImplementedException();
        }

        private void CrearPermiso(string clasePermiso, string nombreDelPermiso)
        {
            var gestor =(GestorDeElementos<ContextoSe, ClasePermisoDtm, ClasePermisoDto>) Generador<ContextoSe, IMapper>.ObtenerGestor("GestorDeSeguridad"
                                                  , "GestorDeClaseDePermisos"
                                                  , new object[] { Contexto, Mapeador });

           

            var claseDePermiso = gestor.LeerRegistroCacheado(nameof(ClasePermisoDtm.Nombre), enumClaseDePermiso.Vista.ToString(),false,false);
            
        }
    }

}

