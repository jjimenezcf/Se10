using AutoMapper;
using System.Linq;
using System.Collections.Generic;
using Utilidades;
using Microsoft.EntityFrameworkCore;
using Gestor.Errores;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos;
using ModeloDeDto.Seguridad;
using GestorDeElementos;
using GestoresDeNegocio.Entorno;
using ModeloDeDto.Entorno;
using ServicioDeDatos.Terceros;
using GestorDeElementos.Extensores;

namespace GestoresDeNegocio.Seguridad
{
    public class GestorDePermisos : GestorDeElementos<ContextoSe, PermisoDtm, PermisoDto>
    {
        public override enumNegocio Negocio => enumNegocio.Permiso;

        public class MapearPermiso : Profile
        {
            public MapearPermiso()
            {
                CreateMap<PermisoDtm, PermisoDto>()
                .ForMember(dto => dto.Clase, dtm => dtm.MapFrom(dtm => dtm.Clase.Nombre))
                .ForMember(dto => dto.Tipo, dtm => dtm.MapFrom(dtm => dtm.Tipo.Nombre));

                CreateMap<PermisoDto, PermisoDtm>()
                    .ForMember(dtm => dtm.Clase, dto => dto.Ignore())
                    .ForMember(dtm => dtm.Tipo, dto => dto.Ignore());


            }
        }

        public GestorDePermisos(ContextoSe contexto, IMapper mapeador)
            : base(contexto, mapeador)
        {

        }

        //public static GestorDePermisos Gestor(ContextoSe contexto) => Gestor(contexto, () => new GestorDePermisos(contexto, contexto.Mapeador));

        public static GestorDePermisos Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return  new GestorDePermisos(contexto, mapeador);
        }

        public static void LimpiarCachesDeUsuariosDeUnPuesto(ContextoSe contexto, int idUsuario)
        {
            var parteDeLaClave = $"Usuario:{idUsuario}";

            ServicioDeCaches.EliminarElementos($"{nameof(GestorDeVistaMvc)}.{nameof(GestorDeVistaMvc.TienePermisos)}", parteDeLaClave);
            ServicioDeCaches.EliminarElementos($"{nameof(GestorDeElementos)}.{nameof(ApiDePermisos.ValidarPermisosDePersistencia)}", parteDeLaClave);
            ServicioDeCaches.EliminarElementos(CacheDe.ModoAcceso_AlNegocio, parteDeLaClave);

            ActualizarCachesDePermisos();
        }

        public static void ActualizarCachesDePermisos()
        {

            ServicioDeCaches.EliminarCache($"{nameof(GestorDeVistaMvc)}.{nameof(GestorDeVistaMvc.TienePermisos)}");
            ServicioDeCaches.EliminarCache($"{nameof(GestorDeElementos)}.{nameof(ApiDePermisos.ValidarPermisosDePersistencia)}");
            ServicioDeCaches.EliminarCache($"{nameof(GestorDeElementos)}.{nameof(ApiDePermisos.LeerModoDeAccesoAlNegocio)}");
            ServicioDeCaches.EliminarCache(nameof(GestorDeArbolDeMenu.LeerArbolDeMenu));

            ServicioDeCaches.EliminarCache(CacheDe.ModoAcceso_AlNegocio);

            ServicioDeCaches.EliminarCache(CacheDe.Ent_PuestosDeUnUsuario);
            ServicioDeCaches.EliminarCache(CacheDe.Ent_UsuariosDeUnPuesto);

            ServicioDeCaches.EliminarCache(CacheDe.Permisos_PorNegocio);
            ServicioDeCaches.EliminarCache(CacheDe.Permisos_PorTipo);
            ServicioDeCaches.EliminarCache(CacheDe.Permisos_TiposConGestion);
            ServicioDeCaches.EliminarCache(CacheDe.Permisos_TiposConsultor);
            ServicioDeCaches.EliminarCache(CacheDe.Permisos_PorElemento);
            ServicioDeCaches.EliminarCache(CacheDe.Permisos_AlgunElemento);
            ServicioDeCaches.EliminarCache(CacheDe.Permisos_PermisoOtorgado);
            ServicioDeCaches.EliminarCache(CacheDe.Permisos_PorEstado);
            ServicioDeCaches.EliminarCache(CacheDe.Permisos_PorTransicion);
            ServicioDeCaches.EliminarCache(CacheDe.Permisos_TieneAlgunPermiso);
            PermisosPorCgSql.EliminarCachesDePermisosPorCg();

            ServicioDeCaches.EliminarCaches(CacheDe.ArbolDeMenu);
            ServicioDeCaches.EliminarCachesDeDescriptores();
        }

        public static PermisoDtm CrearObtener(ContextoSe contexto, enumNegocio negocio, string nombre, enumClaseDePermiso clase, enumModoDeAccesoDeDatos modoAcceso)
        {
            var nombreDelPermiso = ComponerNombreDelPermisoDeDatos(negocio, nombre, clase, modoAcceso);
            var gestorDePermiso = Gestor(contexto, contexto.Mapeador);
            var permiso = gestorDePermiso.LeerRegistro(nameof(PermisoDtm.Nombre), nombreDelPermiso, errorSiNoHay: false, errorSiHayMasDeUno: true, conBloqueo: false, aplicarJoin: false);
            if (permiso == null)
                permiso = CrearPermisoDeDatos(gestorDePermiso, nombreDelPermiso, clase, modoAcceso);
            return permiso;
        }


        public static PermisoDtm CrearObtener(ContextoSe contexto, string nombre, enumClaseDePermiso clase)
        {
            var nombreDelPermiso = ComponerNombrePermisoFuncional(nombre, clase);
            var gestorDePermiso = Gestor(contexto, contexto.Mapeador);
            var permiso = gestorDePermiso.LeerRegistro(nameof(PermisoDtm.Nombre), nombreDelPermiso, errorSiNoHay: false, errorSiHayMasDeUno: false, conBloqueo: false, aplicarJoin: false);
            if (permiso == null)
                permiso = CrearPermisoFuncional(gestorDePermiso, nombreDelPermiso, clase);
            return permiso;
        }


        public static PermisoDtm ModificarPermisoDeDatos(ContextoSe contexto, enumNegocio negocio, int idPpermiso, string nombre, enumClaseDePermiso clase, enumModoDeAccesoDeDatos modoAcceso)
        {
            var permiso = LeerRegistroPorId(contexto, idPpermiso);
            return ModificarPermisoDeDatos(contexto, negocio, permiso, nombre, clase, modoAcceso);
        }

        public static PermisoDtm ModificarPermisoDeDatos(ContextoSe contexto, enumNegocio negocio, PermisoDtm permiso, string nombre, enumClaseDePermiso clase, enumModoDeAccesoDeDatos modoAcceso)
        {
            var gestorDePermiso = Gestor(contexto, contexto.Mapeador);
            var nuevoNombre = ComponerNombreDelPermisoDeDatos(negocio, nombre, clase, modoAcceso);
            if (nuevoNombre == permiso.Nombre)
                return permiso;
            permiso.Nombre = nuevoNombre;
            return gestorDePermiso.Modificar(permiso);
        }

        public static PermisoDtm ModificarPermisoFuncional(ContextoSe contexto, IMapper mapeador, PermisoDtm permiso, string nombre, enumClaseDePermiso clase)
        {
            var gestorDePermiso = Gestor(contexto, mapeador);
            var nuevoNombre = ComponerNombrePermisoFuncional(nombre, clase);
            if (nuevoNombre == permiso.Nombre)
                return permiso;
            permiso.Nombre = nuevoNombre;
            return gestorDePermiso.Modificar(permiso);
        }

        public static PermisoDtm Eliminar(ContextoSe contexto, IMapper mapeador, PermisoDtm permiso)
        {
            var gestorDePermiso = Gestor(contexto, mapeador);
            return gestorDePermiso.Eliminar(permiso);
        }

        private static string ComponerNombreDelPermisoDeDatos(enumNegocio negocio, string nombre, enumClaseDePermiso clase, enumModoDeAccesoDeDatos modoAcceso)
        {
            var prefijo = "";
            if (clase == enumClaseDePermiso.Tipo) prefijo = $"{negocio.ToString().ToUpper()}.";
            if (clase == enumClaseDePermiso.Elemento) prefijo = $"{negocio.ToString().ToUpper()}.";
            if (clase == enumClaseDePermiso.Estado)
            {
                prefijo = $"{negocio.ToString().ToUpper()}.";
                return $"{prefijo}{ClaseDePermiso.ToString(clase).ToUpper()}: {nombre}";
            }
            if (clase == enumClaseDePermiso.Transicion)
            {
                prefijo = $"{negocio.ToString().ToUpper()}.";
                return $"{prefijo}{ClaseDePermiso.ToString(clase).ToUpper()}: {nombre}";
            }
            if (clase == enumClaseDePermiso.Plantilla)
            {
                prefijo = $"{negocio.ToString().ToUpper()}.";
                return $"{prefijo}{ClaseDePermiso.ToString(clase).ToUpper()}: {nombre}";
            }
            return $"{prefijo}{ClaseDePermiso.ToString(clase).ToUpper()} ({ModoDeAcceso.Nombre(modoAcceso)}): {nombre}";
        }

        private static string ComponerNombrePermisoFuncional(string nombre, enumClaseDePermiso clase)
        {
            return $"{ClaseDePermiso.ToString(clase).ToUpper()}: {nombre}";
        }

        private static PermisoDtm CrearPermisoDeDatos(GestorDePermisos gestorDePermiso, string nombreDelPermiso, enumClaseDePermiso clase, enumModoDeAccesoDeDatos modoDeAcceso)
        {
            PermisoDtm permiso;
            var gestorDeClase = GestorDeClaseDePermisos.Gestor(gestorDePermiso.Contexto, gestorDePermiso.Mapeador);
            var claseDePermiso = gestorDeClase.LeerRegistro(nameof(ClasePermisoDtm.Nombre), clase.ToString(), errorSiNoHay: false, errorSiHayMasDeUno: false, conBloqueo: false, aplicarJoin: false);
            if (claseDePermiso == null)
                claseDePermiso = gestorDeClase.Crear(clase);


            var gestorDeTipo = GestorDeTipoPermiso.Gestor(gestorDePermiso.Contexto, gestorDePermiso.Mapeador);
            var tipoDePermiso = gestorDeTipo.LeerRegistro(nameof(TipoPermisoDtm.Nombre), ModoDeAcceso.Nombre(modoDeAcceso), errorSiNoHay: false, errorSiHayMasDeUno: false, conBloqueo: false, aplicarJoin: false);
            if (tipoDePermiso == null)
                tipoDePermiso = gestorDeTipo.CrearTipoPermisoDeDatos(modoDeAcceso);

            permiso = gestorDePermiso.Crear(nombreDelPermiso, tipoDePermiso, claseDePermiso);
            return permiso;
        }


        private static PermisoDtm CrearPermisoFuncional(GestorDePermisos gestorDePermiso, string nombreDelPermiso, enumClaseDePermiso clase)
        {
            PermisoDtm permiso;
            var gestorDeClase = GestorDeClaseDePermisos.Gestor(gestorDePermiso.Contexto, gestorDePermiso.Mapeador);
            var claseDePermiso = gestorDeClase.LeerRegistro(nameof(ClasePermisoDtm.Nombre), clase.ToString(), errorSiNoHay: false, errorSiHayMasDeUno: false, conBloqueo: false, aplicarJoin: false);
            if (claseDePermiso == null)
                claseDePermiso = gestorDeClase.Crear(clase);


            var gestorDeTipo = GestorDeTipoPermiso.Gestor(gestorDePermiso.Contexto, gestorDePermiso.Mapeador);
            var tipoDePermiso = gestorDeTipo.LeerRegistro(nameof(TipoPermisoDtm.Nombre), ModoDeAcceso.ToString(enumModoDeAccesoFuncional.Acceso), errorSiNoHay: false, errorSiHayMasDeUno: false, conBloqueo: false, aplicarJoin: false);
            if (tipoDePermiso == null)
                tipoDePermiso = gestorDeTipo.CrearTipoPermisoFuncional(enumModoDeAccesoFuncional.Acceso);

            permiso = gestorDePermiso.Crear(nombreDelPermiso, tipoDePermiso, claseDePermiso);
            return permiso;
        }

        protected override IQueryable<PermisoDtm> AplicarFiltros(IQueryable<PermisoDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarFiltros(consulta, filtros, parametros);

            if (HayFiltroPorId(filtros))
                return consulta;

            foreach (ClausulaDeFiltrado filtro in filtros)
            {
                if (filtro.Clausula.ToLower() == ltrDeUnPermisoDtm.PermisosDeElemento.ToLower())
                {
                    consulta = consulta.Where(p => p.Nombre.Contains(filtro.Valor) && p.Clase.Nombre == enumClaseDePermiso.Elemento.ToString());
                    filtro.Aplicado = true;
                }

                if (filtro.Clausula.ToLower() == ltrDeUnPermisoDtm.PermisosDeNegocio.ToLower())
                {
                    consulta = consulta.Where(p => p.Nombre.Contains(filtro.Valor) && p.Clase.Nombre == enumClaseDePermiso.Negocio.ToString());
                    filtro.Aplicado = true;
                }

                if (filtro.Clausula.ToLower() == ltrDeUnPermisoDtm.PermisosDeTipo.ToLower())
                {
                    consulta = consulta.Where(p => p.Nombre.Contains(filtro.Valor) && p.Clase.Nombre == enumClaseDePermiso.Tipo.ToString());
                    filtro.Aplicado = true;
                }

                if (filtro.Clausula.ToLower() == ltrDeUnPermisoDtm.PermisosDeEstado.ToLower())
                {
                    consulta = consulta.Where(p => p.Nombre.Contains(filtro.Valor) && p.Clase.Nombre == enumClaseDePermiso.Estado.ToString());
                    filtro.Aplicado = true;
                }

                if (filtro.Clausula.ToLower() == ltrDeUnPermisoDtm.PermisosDeTransicion.ToLower())
                {
                    consulta = consulta.Where(p => p.Nombre.Contains(filtro.Valor) && p.Clase.Nombre == enumClaseDePermiso.Transicion.ToString());
                    filtro.Aplicado = true;
                }

                if (filtro.Clausula.ToLower() == ltrDeUnPermisoDtm.PermisosDeCg.ToLower())
                {
                    consulta = consulta.Where(p => p.Nombre.Contains(filtro.Valor) && p.Clase.Nombre == enumClaseDePermiso.CentroGestor.ToString());
                    filtro.Aplicado = true;
                }

                if (filtro.Clausula.ToLower() == UsuariosPor.AlgunUsuario)
                {
                    var listaIds = filtro.Valor.ToLista<int>();
                    foreach (int id in listaIds)
                        consulta = consulta.Where(p => p.Usuarios.Any(up => up.IdUsuario == id && up.IdPermiso == p.Id));
                    filtro.Aplicado = true;
                }

                if (filtro.Clausula.Equals(nameof(PermisosDeUnUsuarioDto.IdUsuario), System.StringComparison.InvariantCultureIgnoreCase))
                {
                    consulta = consulta.Where(p => p.Usuarios.Any(up => up.IdUsuario == filtro.Valor.Entero() && up.IdPermiso == p.Id));
                    filtro.Aplicado = true;
                }


                if (filtro.Clausula.ToLower() == PermisoPor.PermisoDeUnRol)
                {
                    var listaIds = filtro.Valor.ToLista<int>();
                    foreach (int id in listaIds)
                        consulta = consulta.Where(x => x.Roles.Any(i => i.IdPermiso == id));
                    filtro.Aplicado = true;
                }

                if (filtro.Clausula.ToLower() == nameof(PermisosDeUnRolDtm.IdRol).ToLower() && filtro.Criterio == enumCriteriosDeFiltrado.diferente)
                {
                    consulta = consulta.AplicarPredicado(filtro, i => !i.Roles.Any(r => r.IdRol == filtro.Valor.Entero()));
                    filtro.Aplicado = true;
                }

                if (filtro.Clausula.ToLower() == nameof(NegociosDeUnCgDtm.IdCg).ToLower())
                {
                    var permisosDeGestor = Contexto.Set<NegociosDeUnCgDtm>().AsNoTracking().Where(x => x.IdCg == filtro.Valor.Entero());
                    var lstGestor = permisosDeGestor.Select(c => c.IdGestor);
                    var permisosDeConsultor = Contexto.Set<NegociosDeUnCgDtm>().AsNoTracking().Where(x => x.IdCg == filtro.Valor.Entero());
                    var lstConsultor = permisosDeConsultor.Select(c => c.IdConsultor);
                    consulta = consulta.Where(p => lstConsultor.Contains(p.Id) || lstGestor.Contains(p.Id));
                    filtro.Aplicado = true;
                }

                if (filtro.Clausula.ToLower() == nameof(RolesDeUnPermisoDto.IdRol).ToLower() && filtro.Criterio == enumCriteriosDeFiltrado.igual)
                {
                    consulta = consulta.Where(x => Contexto.Set<PermisosDeUnRolDtm>().AsNoTracking().Where(r => r.IdRol == filtro.Valor.Entero()).Select(r => r.IdPermiso).Contains(x.Id));
                    filtro.Aplicado = true;
                }

                if (filtro.Clausula.ToLower() == nameof(PermisosDeUnPuestoDto.IdPuesto).ToLower() && filtro.Criterio == enumCriteriosDeFiltrado.igual)
                {
                    consulta = consulta.Where(x => Contexto.Set<PermisosHeredadosDtm>().AsNoTracking().Where(r => r.IdPuesto == filtro.Valor.Entero()).Select(r => r.IdPermiso).Contains(x.Id)
                                            || Contexto.Set<PermisosDirectosDtm>().AsNoTracking().Where(r => r.IdPuesto == filtro.Valor.Entero()).Select(r => r.IdPermiso).Contains(x.Id));
                    filtro.Aplicado = true;
                }

            }

            return consulta;

        }

        protected override IQueryable<PermisoDtm> AplicarJoins(IQueryable<PermisoDtm> registros, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            registros = base.AplicarJoins(registros, filtros, parametros);
            registros = registros.Include(p => p.Clase);
            registros = registros.Include(p => p.Tipo);
            return registros;
        }

        protected override void DespuesDeMapearElElemento(PermisoDtm registro, PermisoDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(registro, elemento, parametros);
            elemento.ModoDeAcceso = ExtensorDeUsuarios.SePuedeParametrizar(Contexto) ? enumModoDeAccesoDeDatos.Administrador : enumModoDeAccesoDeDatos.Consultor;
        }

        public List<ClasePermisoDto> LeerClases()
        {
            return LeerClases(0, -1, new List<ClausulaDeFiltrado>());
        }

        public List<ClasePermisoDto> LeerClases(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros)
        {
            var gestor = GestorDeClaseDePermisos.Gestor(Contexto, Mapeador);
            var registros = gestor.LeerRegistros(posicion, cantidad, filtros);
            return gestor.MapearElementos(registros).ToList();
        }

        public List<TipoPermisoDto> LeerTipos()
        {
            return LeerTipos(0, -1, new List<ClausulaDeFiltrado>());
        }

        public List<TipoPermisoDto> LeerTipos(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros)
        {
            var gestor = GestorDeTipoPermiso.Gestor(Contexto, Mapeador);
            var registros = gestor.LeerRegistros(posicion, cantidad, filtros);
            return gestor.MapearElementos(registros).ToList();
        }

        protected override void AntesDeMapearElRegistroParaEliminar(PermisoDto elemento, ParametrosDeNegocio opciones)
        {
            base.AntesDeMapearElRegistroParaEliminar(elemento, opciones);

            var gestor = GestorDePermisosDeUnRol.Gestor(Contexto,Contexto.Mapeador);
            var filtro = new ClausulaDeFiltrado { Clausula = nameof(PermisosDeUnRolDtm.IdPermiso), Criterio = enumCriteriosDeFiltrado.igual, Valor = elemento.Id.ToString() };
            var filtros = new List<ClausulaDeFiltrado> { filtro };
            var r = gestor.LeerRegistros(0, 1, filtros);
            if (r.Count > 0)
            {
                var roles = "";
                foreach (var r1 in r)
                    roles = $"{(roles == "" ? "" : $"{roles},")} {r1.Rol.Nombre}";

                GestorDeErrores.Emitir($"El permiso está incluido en {(r.Count == 1 ? "el rol" : "los roles") }: '{roles}'");
            }
        }

        private PermisoDtm Crear(string nombrePermiso, TipoPermisoDtm tipoDePermiso, ClasePermisoDtm claseDePermiso)
        {
            var registro = new PermisoDtm();
            registro.Nombre = nombrePermiso;
            registro.IdClase = claseDePermiso.Id;
            registro.IdTipo = tipoDePermiso.Id;
            var p = new ParametrosDeNegocio(enumTipoOperacion.Insertar, new Dictionary<string, object> { {ltrParametrosNeg.ValidarPermisosDePersistencia,false } });
            PersistirRegistro(registro, p);
            return registro;
        }

        private PermisoDtm Modificar(PermisoDtm permiso)
        {
            PersistirRegistro(permiso, new ParametrosDeNegocio(enumTipoOperacion.Modificar, new Dictionary<string, object> { { ltrParametrosNeg.ValidarPermisosDePersistencia, false } }));
            return permiso;
        }

        private PermisoDtm Eliminar(PermisoDtm permiso)
        {
            PersistirRegistro(permiso, new ParametrosDeNegocio(enumTipoOperacion.Eliminar, new Dictionary<string, object> { { ltrParametrosNeg.ValidarPermisosDePersistencia, false } }));
            return permiso;
        }

        protected override void AntesDePersistir(PermisoDtm registro, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(registro, parametros);
            if (parametros.Eliminando) ValidarQueElPermisoNoEstaEnUnRol(registro, parametros);
            if (parametros.Modificando && parametros.EsUnaPeticion)
                GestorDeErrores.Emitir($"El permiso '{registro.Nombre}' no se puede modificar por una petición'");
        }

        private void ValidarQueElPermisoNoEstaEnUnRol(PermisoDtm registro, ParametrosDeNegocio parametros)
        {
            if (!parametros.Operacion.Equals(enumTipoOperacion.Eliminar)) return;

            var gestor = new GestorDePermisosDeUnRol(Contexto, Mapeador);
            var filtro = new ClausulaDeFiltrado { Clausula = nameof(PermisosDeUnRolDtm.IdPermiso), Criterio = enumCriteriosDeFiltrado.igual, Valor = registro.Id.ToString() };
            var filtros = new List<ClausulaDeFiltrado> { filtro };
            var numero = gestor.Contar(filtros);
            if (numero > 0)
            {
                var filtroPermisoDeUnRol = new List<ClausulaDeFiltrado> { { new ClausulaDeFiltrado { Clausula = nameof(PermisosDeUnRolDtm.IdPermiso), Criterio = enumCriteriosDeFiltrado.igual, Valor = registro.Id.ToString() } } };
                var rolesDeUnPermiso = GestorDeRolesDeUnPermiso.Gestor(Contexto, Contexto.Mapeador).LeerRegistros(0, numero, filtroPermisoDeUnRol);
                var roles = "";
                foreach (var rolDeUnPermiso in rolesDeUnPermiso)
                    roles = $"{roles}; '{rolDeUnPermiso.Rol.Nombre}'";
                roles = roles.Substring(1);
                GestorDeErrores.Emitir($"El permiso '{registro.Nombre}' esta incluido en {numero} rol/roles, desasígnelo primero de : '{roles}'");
            }
        }
    }

}
