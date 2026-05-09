using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Utilidades;
using ModeloDeDto.Entorno;
using ServicioDeDatos;
using ServicioDeDatos.Entorno;
using Gestor.Errores;
using ServicioDeDatos.Seguridad;
using GestorDeElementos;
using GestoresDeNegocio.Seguridad;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using ModeloDeDto;

namespace GestoresDeNegocio.Entorno
{

    public class GestorDeVistaMvc : GestorDeElementos<ContextoSe, VistaMvcDtm, VistaMvcDto>
    {
        public override enumNegocio Negocio => enumNegocio.VistaMvc;

        public class MapearVistaMvc : Profile
        {
            public MapearVistaMvc()
            {
                CreateMap<VistaMvcDtm, VistaMvcDto>()
                .ForMember(dto => dto.Menus, dtm => dtm.MapFrom(x => x.Menus))
                .ForMember(dto => dto.IdPermiso, dtm => dtm.MapFrom(x => x.Permiso.Id))
                .ForMember(dto => dto.Permiso, dtm => dtm.MapFrom(x => x.Permiso.Nombre));

                CreateMap<VistaMvcDto, VistaMvcDtm>()
                .ForMember(dtm => dtm.Permiso, dto => dto.Ignore())
                ;
            }
        }

        public GestorDeVistaMvc(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {

        }


        public static GestorDeVistaMvc Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeVistaMvc(contexto, mapeador);
        }

        public VistaMvcDtm CrearVistaSiNoExiste(string nombre, object controlador, string vista, bool modal, string elementoDto)
        {
           // var v = LeerRegistroCacheado(nameof(VistaMvcDtm.Nombre), nombre, false, true, false);

            var vistas = Contexto.SeleccionarTodos<VistaMvcDtm>(new Dictionary<string, object> {
                { nameof(VistaMvcDtm.Controlador), controlador.ToString().Replace(ltrEndPoint.Controller, "")},
                { nameof(VistaMvcDtm.Accion), vista}
                });

            if (vistas.Count >1)
            {
                GestorDeErrores.Emitir($"No se puede crear la vista '{nombre}' ya que hay deinida para el controlador y acción '{controlador.ToString().Replace(ltrEndPoint.Controller, "")}/{vista}' '{vistas.Count}' registros");
            }

            if (vistas.Count == 0)
                return CrearVistaMvc(nombre, controlador.ToString(), vista, modal, elementoDto);

            var v = vistas[0];
            if (v.Controlador != controlador.ToString().Replace(ltrEndPoint.Controller, "") || v.Accion != vista || v.MostrarEnModal != modal || v.ElementoDto != elementoDto)
            {
                v.Nombre = nombre;
                v.Controlador = controlador.ToString().Replace(ltrEndPoint.Controller, "");
                v.Accion = vista;
                v.MostrarEnModal = modal;
                v.ElementoDto = elementoDto;
                return v.Modificar(Contexto);
            }

            return vistas[0];
        }

        public void BorrarVistaSiExiste(string nombre, object controlador, string vista, bool modal, string elementoDto)
        {
            var v = LeerRegistroCacheado(nameof(VistaMvcDtm.Nombre), nombre, false, true, false);
            if (v is not null)
            {
                Contexto.EliminarPorAk<MenuDtm>(new Dictionary<string, object> { { nameof(MenuDtm.IdVistaMvc), v.Id } });
                Contexto.EliminarPorId<VistaMvcDtm>(v.Id);
            }
        }

        private VistaMvcDtm CrearVistaMvc(string nombre, string controlador, string accion, bool mostrarEnModal, string elementoDto)
        {
            var v = new VistaMvcDtm();
            v.Nombre = nombre;
            v.Controlador = controlador.Replace(ltrEndPoint.Controller, "");
            v.Accion = accion;
            v.MostrarEnModal = mostrarEnModal;
            v.ElementoDto = elementoDto;
            v = PersistirRegistro(v, new ParametrosDeNegocio(enumTipoOperacion.Insertar));
            return v;
        }

        protected override IQueryable<VistaMvcDtm> AplicarJoins(IQueryable<VistaMvcDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(p => p.Permiso);
            return consulta;
        }


        protected override IQueryable<VistaMvcDtm> AplicarFiltros(IQueryable<VistaMvcDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarFiltros(consulta, filtros, parametros);

            if (HayFiltroPorId(filtros))
                return consulta;

            foreach (ClausulaDeFiltrado filtro in filtros)
            {
                if (filtro.Clausula.ToLower() == nameof(VistaMvcDtm.Controlador).ToLower())
                {
                    if (filtro.Criterio == enumCriteriosDeFiltrado.igual)
                        consulta = consulta.Where(x => x.Controlador == filtro.Valor);

                    if (filtro.Criterio == enumCriteriosDeFiltrado.contiene)
                        consulta = consulta.Where(x => x.Controlador.Contains(filtro.Valor));
                }
                if (filtro.Clausula.ToLower() == nameof(VistaMvcDtm.Accion).ToLower())
                {
                    if (filtro.Criterio == enumCriteriosDeFiltrado.igual)
                        consulta = consulta.Where(x => x.Accion == filtro.Valor);
                }
            }

            return consulta;
        }

        public bool TienePermisos(UsuarioDtm usuarioConectado, string vista)
        {
            if (usuarioConectado.EsAdministrador)
                return true;

            var vistaDtm = LeerVistaMvc(vista);
            var cache = ServicioDeCaches.Obtener($"{nameof(GestorDeVistaMvc)}.{nameof(TienePermisos)}");
            var indice = $"Usuario:{usuarioConectado.Id} Permiso:{vistaDtm.IdPermiso}";

            if (!cache.ContainsKey(indice))
            {
                var gestor = GestorDePermisosDeUnUsuario.Gestor(Contexto, Mapeador);

                var filtros = new List<ClausulaDeFiltrado>
                {
                    new ClausulaDeFiltrado { Clausula = nameof(UsuariosDeUnPermisoDtm.IdUsuario), Criterio = enumCriteriosDeFiltrado.igual, Valor = usuarioConectado.Id.ToString()},
                    new ClausulaDeFiltrado { Clausula = nameof(UsuariosDeUnPermisoDtm.IdPermiso), Criterio = enumCriteriosDeFiltrado.igual, Valor = vistaDtm.IdPermiso.ToString() }
                };

                cache[indice] = gestor.Contar(filtros) > 0;
            }
            return (bool)cache[indice];

        }

        public VistaMvcDtm LeerVistaMvc(string vistaMvc)
        {
            var vista = ValidarParametroAntesDeLeerVistaMvc(vistaMvc);

            var cache = ServicioDeCaches.Obtener(nameof(LeerVistaMvc));
            if (!cache.ContainsKey(vista))
            {
                var filtros = new List<ClausulaDeFiltrado>
                {
                    new ClausulaDeFiltrado { Clausula = nameof(VistaMvcDtm.Controlador), Criterio = enumCriteriosDeFiltrado.igual, Valor = vista.Split(".")[0]},
                    new ClausulaDeFiltrado { Clausula = nameof(VistaMvcDtm.Accion), Criterio = enumCriteriosDeFiltrado.igual, Valor = vista.Split(".")[1] }
                };
                var vistas = LeerRegistros(0, -1, filtros);
                if (vistas.Count != 1)
                {
                    if (vistas.Count == 0)
                        GestorDeErrores.Emitir($"Debe crear el registro en la tabla de VistaMvc {vista}");
                    else
                        GestorDeErrores.Emitir($"Se han localizado {vistas.Count} vistasMvc para {vista}");
                }

                if (vistas == null)
                {
                    GestorDeErrores.Emitir($"Defina la vista {vista} en BD");
                }

                cache[$"{vista}"] = vistas[0];
            }

            return (VistaMvcDtm)cache[$"{vista}"];
        }

        private static string ValidarParametroAntesDeLeerVistaMvc(string vistaMvc)
        {
            if (vistaMvc.IsNullOrEmpty())
                GestorDeErrores.Emitir($"Debe indicar el nombre del controlador y vista a buscar");

            var partes = vistaMvc.Split(".");

            if (partes.Length != 2)
                GestorDeErrores.Emitir($"El valor proporcionado {vistaMvc} no es válido, ha de seguir el patrón Controlador.Vista");

            var nombreDelControlador = partes[0];
            var nombreDeLaVista = partes[1];
            if (nombreDelControlador.IsNullOrEmpty() || nombreDeLaVista.IsNullOrEmpty())
                GestorDeErrores.Emitir($"falta información del controlador o la vista a buscar, usted ha proporcionado, controlado: {nombreDelControlador}, vista: {nombreDeLaVista}");

            return $"{nombreDelControlador}.{nombreDeLaVista}";
        }

        protected override void AntesDePersistir(VistaMvcDtm registro, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(registro, parametros);

            if (!registro.ElementoDto.IsNullOrEmpty())
                ExtensionesDto.ObtenerTypoDto(registro.ElementoDto);

            if (parametros.Operacion == enumTipoOperacion.Insertar)
            {
                var permiso = GestorDePermisos.CrearObtener(Contexto, registro.Nombre, enumClaseDePermiso.Vista);
                registro.IdPermiso = permiso.Id;
            }
            if (parametros.Operacion == enumTipoOperacion.Modificar)
            {
                registro.IdPermiso = ((VistaMvcDtm)parametros.registroEnBd).IdPermiso;
            }
        }

        protected override void DespuesDePersistir(VistaMvcDtm registro, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(registro, parametros);
            var RegistroEnBD = ((VistaMvcDtm)parametros.registroEnBd);

            if (parametros.Operacion == enumTipoOperacion.Modificar && RegistroEnBD.Nombre != registro.Nombre)
                GestorDePermisos.ModificarPermisoFuncional(Contexto, Mapeador, RegistroEnBD.Permiso, registro.Nombre, enumClaseDePermiso.Vista);

            if (parametros.Operacion == enumTipoOperacion.Eliminar)
            {
                if (RegistroEnBD.Permiso == null)
                    RegistroEnBD.Permiso = Contexto.SeleccionarPorId<PermisoDtm>(RegistroEnBD.IdPermiso);
                GestorDePermisos.Eliminar(Contexto, Mapeador, RegistroEnBD.Permiso);
            }

            ServicioDeCaches.EliminarElemento(nameof(LeerVistaMvc), $"{registro.Controlador}.{registro.Accion}");
            ServicioDeCaches.EliminarElementos(nameof(ExtensionesDto.UrlBaseDeUnDto), patron: registro.ElementoDto);

            GestorDeArbolDeMenu.LimpiarCacheDeArbolDeMenu();
        }

        protected override void DespuesDeMapearElElemento(VistaMvcDtm registro, VistaMvcDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(registro, elemento, parametros);
        }



    }

}

