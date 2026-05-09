using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Entorno;
using GestoresDeNegocio.Seguridad;
using ModeloDeDto.Negocio;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Seguridad;
using Utilidades;

namespace GestoresDeNegocio.Negocio
{
    public class GestorDeEstados : GestorDeElementos<ContextoSe, EstadoDtm, EstadoDto>, IGestorGenerico
    {
        private string _tabla { get; set; }

        private enumNegocio _Negocio;

        public override enumNegocio Negocio => _Negocio;

        private IGestor gestor => NegociosDeSe.CrearGestor(Contexto, Negocio);


        public class MapearEstados : Profile
        {
            public MapearEstados()
            {
                CreateMap<EstadoDtm, EstadoDto>();
                CreateMap<EstadoDto, EstadoDtm>();
            }
        }

        public GestorDeEstados(ContextoSe contexto)
        : base(contexto)
        {
            Contexto = contexto;
        }

        public GestorDeEstados(ContextoSe contexto, enumNegocio negocio)
        : base(contexto, contexto.Mapeador)
        {
            AsignarNegocio(negocio);
        }

        public void AsignarNegocio(enumNegocio negocio)
        {
            _tabla = ApiDeEstado.TablaDeEstados(negocio.TipoDtm());
            _Negocio = negocio;
        }

        public static EstadoDtm LeerPorNombre(ContextoSe contexto, enumNegocio negocio, string nombre) =>
            Gestor(contexto, negocio).LeerRegistro(nameof(INombre.Nombre), nombre, true, true, false, false);

        public static EstadoDtm PersistirEstado(ContextoSe contexto, enumNegocio negocio, string nombre, bool inicial = false, bool terminado = false, bool cancelado = false, int orden = 0)
        {
            var gestor = Gestor(contexto, negocio);
            var leido = gestor.LeerRegistroCacheado(nameof(EstadoDtm.Nombre), nombre, false, false, false);
            if (leido == null)
            {
                EstadoDtm estado = negocio.CrearEstado();
                estado.Inicial = inicial;
                estado.Cancelado = cancelado;
                estado.Terminado = terminado;
                estado.Nombre = nombre;
                estado.Orden = orden;
                estado = gestor.PersistirRegistro(estado, new ParametrosDeNegocio(enumTipoOperacion.Insertar));
                return gestor.LeerRegistroPorId(estado.Id);
            }

            if (leido.Orden != orden || leido.Cancelado != cancelado || leido.Terminado != terminado || leido.Inicial != inicial)
            {
                leido.Orden = orden; leido.Inicial = inicial; leido.Cancelado = cancelado; leido.Terminado = terminado;
                gestor.PersistirRegistro(leido, new ParametrosDeNegocio(enumTipoOperacion.Modificar));
            }

            return leido;
        }

        public static GestorDeEstados Gestor(ContextoSe contexto, enumNegocio negocio) => new GestorDeEstados(contexto, negocio);

        public EstadoDtm LeerRegistroPorId(int id)
            => EstadoSql.LeerEstadoPorId(Contexto, _tabla, id);

        public IEnumerable<EstadoDtm> LeerRegistros(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros, List<ClausulaDeOrdenacion> orden)
            => EstadoSql.LeerEstados(Contexto, _tabla, posicion, cantidad, filtros, orden);

        public int ContarRegistros(List<ClausulaDeFiltrado> filtros)
            => EstadoSql.ContarRegistros(Contexto, _tabla, filtros);


        public override EstadoDtm LeerRegistroPorId(int id, bool usarLaCache, bool traqueado, bool conBloqueo, bool aplicarJoin, bool errorSiNoHay, Dictionary<string, object> parametros = null)
        {
            return LeerRegistroPorId(id);
        }

        public override List<EstadoDtm> LeerRegistros(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros = null, List<ClausulaDeOrdenacion> orden = null, ParametrosDeNegocio parametros = null)
        {
            return LeerRegistros(posicion, cantidad, filtros, orden).ToList();
        }

        public override EstadoDtm LeerRegistro(string propiedad, string valor, bool errorSiNoHay, bool errorSiHayMasDeUno, bool conBloqueo, bool aplicarJoin, Dictionary<string, object> parametros = null)
        {
            List<EstadoDtm> registros = EstadoSql.LeerEstadoPorNombre(Contexto, _tabla, valor);

            if (errorSiNoHay && registros.Count == 0)
                GestorDeErrores.Emitir($"No se ha localizado el registro solicitado para el valor '{valor}' en la clase '{typeof(EstadoDtm).Name}'");

            if (errorSiHayMasDeUno && registros.Count > 1)
                GestorDeErrores.Emitir($"Hay más de un registro para el valor '{valor}' en la clase '{typeof(EstadoDtm).Name}'");

            if (registros.Count == 0)
                return null;

            if (registros.Count > 1)
                return registros[0];

            return registros[0];
        }

        public override int Contar(List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros = null)
        {
            CacheDeRecuentos[typeof(EstadoSql).FullName] = false;
            return EstadoSql.ContarRegistros(Contexto, _tabla, filtros);
        }
        protected override void AntesDePersistir(EstadoDtm registro, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(registro, parametros);
            if (parametros.Operacion == enumTipoOperacion.Insertar)
                registro.IdPermiso = GestorDePermisos.CrearObtener(Contexto, Negocio, registro.Nombre, enumClaseDePermiso.Estado, enumModoDeAccesoDeDatos.Gestor).Id;

            if (parametros.Operacion == enumTipoOperacion.Insertar || parametros.Operacion == enumTipoOperacion.Modificar)
            {
                if (parametros.Operacion == enumTipoOperacion.Insertar)
                    registro.IdPermiso = GestorDePermisos.CrearObtener(Contexto, Negocio, registro.Nombre, enumClaseDePermiso.Estado, enumModoDeAccesoDeDatos.Consultor).Id;
                else
                {
                    var permiso = GestorDePermisos.LeerRegistroPorId(Contexto, registro.IdPermiso);
                    GestorDePermisos.ModificarPermisoDeDatos(Contexto, Negocio, permiso, registro.Nombre, enumClaseDePermiso.Estado, enumModoDeAccesoDeDatos.Consultor);
                }
            }
        }
        protected override void Persistir(EstadoDtm registro, ParametrosDeNegocio parametros)
        {
            if (parametros.Operacion == enumTipoOperacion.Insertar)
                registro.Id = EstadoSql.Insertar(Contexto, _tabla, registro).Id;
            else if (parametros.Operacion == enumTipoOperacion.Modificar)
                EstadoSql.Modificar(Contexto, _tabla, registro);
            else
                EstadoSql.Borrar(Contexto, _tabla, registro);
        }

        protected override void DespuesDePersistir(EstadoDtm registro, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(registro, parametros);

            if (parametros.Operacion == enumTipoOperacion.Modificar && registro.Nombre != ((INombre)parametros.registroEnBd).Nombre)
            {
                var filtros = new List<ClausulaDeFiltrado>
                {
                    new ClausulaDeFiltrado(ltrTransiciones.filtroOrPorIdDeEstado, enumCriteriosDeFiltrado.igual, registro.Id.ToString())
                };
                var transiciones = TransicionSql.LeerTransiciones(Contexto, ApiDeTransicion.TablaDeTransiciones(NegociosDeSe.TipoDtm(Negocio)), 0, -1, filtros, new List<ClausulaDeOrdenacion>());
                var gestor = GestorDeTransiciones.Gestor(Contexto, Negocio);
                foreach (var transicion in transiciones)
                    gestor.PersistirRegistro(transicion, new ParametrosDeNegocio(enumTipoOperacion.Modificar));
            }

            if (parametros.Operacion == enumTipoOperacion.Eliminar)
                GestorDePermisos.Eliminar(Contexto, Mapeador, GestorDePermisos.LeerRegistroPorId(Contexto, registro.IdPermiso));
        }

        protected override void EliminarCaches(EstadoDtm registro, ParametrosDeNegocio parametros)
        {
            base.EliminarCaches(registro, parametros);
            ServicioDeCaches.EliminarElemento(CacheDe.Estados, Negocio.ToString());
            ServicioDeCaches.EliminarElemento(CacheDe.Estado, $"{Negocio}-{registro.Id}");
            ServicioDeCaches.EliminarElemento(nameof(EstadoSql.LeerEstadoPorId), $"{_tabla}.{registro.Id}");
        }

        protected override void DespuesDeMapearElElemento(EstadoDtm registro, EstadoDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(registro, elemento, parametros);
            elemento.IdNegocio = NegociosDeSe.IdNegocio(Negocio);
            elemento.Negocio = NegociosDeSe.ToNombre(Negocio);
        }

        public static void GenerarSeguridad(ContextoSe contexto, Action<string> logger = null)
        {
            PermisosPorEstadoSql.EliminarTodos(contexto);
            foreach (var negocio in Enum.GetValues(typeof(enumNegocio)))
            {
                if (!NegociosDeSe.UsaEstado((enumNegocio)negocio))
                    continue;

                var negocioDtm = NegociosDeSe.LeerNegocioPorEnumerado((enumNegocio)negocio);
                var estados = (List<EstadoDtm>)contexto.CrearGestorDeEstados((enumNegocio)negocio).LeerRegistros(0, -1, new List<ClausulaDeFiltrado>(), false);

                logger?.Invoke($"Procesando los estados del negocio de {(enumNegocio)negocio}");
                OtorgarSeguridadPorEstados(contexto, negocioDtm.Id, estados);
            }
        }

        public static void GenerarSeguridadParaElUsuario(ContextoSe contexto,int idUsuario, Action<string> logger = null)
        {
            PermisosPorEstadoSql.QuitarPermisos(contexto, idUsuario, calculado: true);
            var permisosPorEstados = ApiDePermisos.PermisosCalculadosDe(contexto, idUsuario, enumClaseDePermiso.Estado);
            foreach (var permiso in permisosPorEstados)
            {
                var negocio = enumNegocio.Parse<enumNegocio>(permiso.Nombre.Split(".")[0], true);
                var estado = contexto.Estados(negocio).First(x => x.IdPermiso == permiso.IdPermiso);
                PermisosPorEstadoSql.Otorgar(contexto, negocio.IdNegocio(), estado.Id, idUsuario, estado.IdPermiso, calculado: true);
            }

            PermisosPorEstadoSql.QuitarPermisos(contexto, idUsuario, calculado: false);
            var permisosDirectos = ApiDePermisos.PermisosCalculadosDe(contexto, idUsuario, enumClaseDePermiso.Estado);
            foreach (var permiso in permisosDirectos)
            {
                var negocio = enumNegocio.Parse<enumNegocio>(permiso.Nombre.Split(".")[0], true);
                var estado = contexto.Estados(negocio).First(x => x.IdPermiso == permiso.IdPermiso);
                PermisosPorEstadoSql.Otorgar(contexto, negocio.IdNegocio(), estado.Id, idUsuario, estado.IdPermiso, calculado: true);
            }
        }

        private static void OtorgarSeguridadPorEstados(ContextoSe contexto, int idNegocio, List<EstadoDtm> estados)
        {
            foreach (var estado in estados)
            {
                var filtro = new ClausulaDeFiltrado(nameof(PermisosDeUnRolDtm.IdPermiso), enumCriteriosDeFiltrado.igual, estado.IdPermiso);
                var usuariosConElPermiso = GestorDePermisosDeUnUsuario.Gestor(contexto, contexto.Mapeador).LeerRegistros(0, -1, new List<ClausulaDeFiltrado> { filtro });
                foreach (var usuario in usuariosConElPermiso)
                    PermisosPorEstadoSql.Otorgar(contexto, idNegocio, estado.Id, usuario.IdUsuario, estado.IdPermiso, calculado: true);
            }
        }
    }


}
