using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using ServicioDeDatos.Negocio;
using ServicioDeDatos;
using ModeloDeDto.Negocio;
using GestorDeElementos;
using ServicioDeDatos.Seguridad;
using GestoresDeNegocio.Seguridad;
using Microsoft.EntityFrameworkCore;
using Utilidades;
using Gestor.Errores;
using System;
using System.Reflection;
using ModeloDeDto;
using ServicioDeDatos.Elemento;
using GestoresDeNegocio.Entorno;
using ServicioDeDatos.Entorno;
using GestoresDeNegocio.Terceros;

namespace GestoresDeNegocio.Negocio
{

    public class GestorDeNegocios : GestorDeElementos<ContextoSe, NegocioDtm, NegocioDto>
    {
        public override enumNegocio Negocio => enumNegocio.Negocio;

        public class MapearNegocio : Profile
        {
            public MapearNegocio()
            {
                CreateMap<NegocioDtm, NegocioDto>()
                .ForMember(dto => dto.PermisoDeGestor, dtm => dtm.MapFrom(x => x.Gestor.Nombre))
                .ForMember(dto => dto.PermisoDeAdministrador, dtm => dtm.MapFrom(x => x.Administrador.Nombre))
                .ForMember(dto => dto.PermisoDeConsultor, dtm => dtm.MapFrom(x => x.Consultor.Nombre));

                CreateMap<NegocioDto, NegocioDtm>()
                .ForMember(dtm => dtm.Gestor, dto => dto.Ignore())
                .ForMember(dtm => dtm.Administrador, dto => dto.Ignore())
                .ForMember(dtm => dtm.Consultor, dto => dto.Ignore());
            }
        }

        public GestorDeNegocios(ContextoSe contexto, IMapper mapeador)
            : base(contexto, mapeador)
        {

        }


        //public static GestorDeNegocios Gestor(ContextoSe contexto) => Gestor(contexto, () => new GestorDeNegocios(contexto, contexto.Mapeador));

        //public static GestorDeNegocios Gestor(ContextoSe contexto, IMapper mapeador)
        //{
        //    return Gestor(contexto); // new GestorDePermisos(contexto, mapeador);
        //}
        public static GestorDeNegocios Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeNegocios(contexto, mapeador);
        }

        public static NegocioDtm LeerNegocio(ContextoSe contexto, enumNegocio negocio)
        {
            var gestor = Gestor(contexto, contexto.Mapeador);
            return gestor.LeerNegocio(negocio);
        }

        public static NegocioDtm LeerNegocio(ContextoSe contexto, int idNegocio)
        {
            var gestor = Gestor(contexto, contexto.Mapeador);
            return gestor.LeerRegistroPorId(idNegocio, true, false, false, aplicarJoin: false);
        }

        public static NegocioDtm CrearNegocioSiNoExiste(GestorDeNegocios gestor, enumNegocio negocio, string nombre, Type dtm, Type dto, string icono, bool esDeParametrizacion, bool usaCg, bool usaSeguridad = true)
        {
            var negocioDtm = gestor.LeerNegocioParaModificar(negocio, errorSiNoHay: false);
            if (negocioDtm == null)
            {
                negocioDtm = CrearNegocio(gestor, negocio, nombre, dtm, dto, icono, esDeParametrizacion, usaCg, usaSeguridad);
            }
            else
            {
                negocioDtm = ActualizarNegocio(gestor, negocioDtm, dtm, dto, esDeParametrizacion, usaCg);
            }
            return negocioDtm;
        }

        private static NegocioDtm CrearNegocio(GestorDeNegocios gestor, enumNegocio negocio, string nombre, Type dtm, Type dto, string icono, bool esDeParametrizacion, bool usaCg, bool usaSeguridad = true)
        {
            var negocioDtm = new NegocioDtm();
            negocioDtm.Enumerado = negocio.ToString();
            negocioDtm.Nombre = nombre;
            negocioDtm.ElementoDtm = dtm.FullName;
            negocioDtm.ElementoDto = dto.FullName;
            negocioDtm.Icono = icono;
            negocioDtm.Activo = true;
            negocioDtm.EsDeParametrizacion = esDeParametrizacion;
            negocioDtm.UsaCentroGestor = usaCg;
            negocioDtm.UsaSeguridad = usaSeguridad;
            var p = new ParametrosDeNegocio(enumTipoOperacion.Insertar);
            return gestor.PersistirRegistro(negocioDtm, p);
        }

        private static NegocioDtm ActualizarNegocio(GestorDeNegocios gestor, NegocioDtm leido, Type dtm, Type dto, bool esDeParametrizacion, bool usaCg)
        {
            leido.ElementoDtm = dtm.FullName;
            leido.ElementoDto = dto.FullName;
            leido.EsDeParametrizacion = esDeParametrizacion;
            leido.UsaCentroGestor = usaCg;

            var p = new ParametrosDeNegocio(enumTipoOperacion.Modificar);
            p.Parametros[NegociosDeSe.ActualizarSeguridad] = true;
            return gestor.PersistirRegistro(leido, p);
        }

        public NegocioDtm LeerNegocio(enumNegocio negocio, bool errorSiNoHay = true)
        {
            var cache = ServicioDeCaches.Obtener(nameof(LeerNegocio));
            if (!cache.ContainsKey(negocio.ToString()))
            {
                cache[negocio.ToString()] = LeerRegistro(nameof(NegocioDtm.Enumerado), negocio.ToString(), errorSiNoHay, true, false, aplicarJoin: false);
            }
            return (NegocioDtm)cache[negocio.ToString()];
        }

        public NegocioDtm LeerNegocioParaModificar(enumNegocio negocio, bool errorSiNoHay = true)
        {
            var negocioDtm = LeerRegistro(nameof(NegocioDtm.Enumerado), negocio.ToString(), errorSiNoHay, true, false, aplicarJoin: false);
            return negocioDtm;
        }

        public bool TienePermisos(enumModoDeAccesoDeDatos permisosNecesarios, enumNegocio negocio)
        {
            var modoAcceso = ApiDePermisos.LeerModoDeAccesoAlNegocio(Contexto, negocio);
            if (modoAcceso.Equals(enumModoDeAccesoDeDatos.Administrador))
                return true;

            if (modoAcceso.Equals(enumModoDeAccesoDeDatos.Consultor) && (permisosNecesarios.Equals(enumModoDeAccesoDeDatos.Administrador) || permisosNecesarios.Equals(enumModoDeAccesoDeDatos.Gestor)))
                return false;

            if (modoAcceso.Equals(enumModoDeAccesoDeDatos.Gestor) && permisosNecesarios.Equals(enumModoDeAccesoDeDatos.Administrador))
                return false;

            return true;
        }

        internal static TipoDtoElmento ValidarElementoDto(TipoDtoElmento elemento)
        {
            if (elemento.IdElemento <= 0)
                GestorDeErrores.Emitir("El elemento dto a validar no tiene indicado el Id");

            if (elemento.TipoDto.IsNullOrEmpty())
                GestorDeErrores.Emitir("El TipoDto a validar no puede ser nulo");

            if (elemento.Referencia.IsNullOrEmpty())
                GestorDeErrores.Emitir("La referencia del elemnto ser nula");

            try
            {
                elemento.ClaseDto();
            }
            catch (Exception e)
            {
                GestorDeErrores.Emitir($"Error al obtener la clase del TipoDto {elemento.TipoDto}", e);
            }

            return elemento;
        }

        protected override IQueryable<NegocioDtm> AplicarFiltros(IQueryable<NegocioDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarFiltros(consulta, filtros, parametros);

            if (HayFiltroPorId(filtros))
                return consulta;

            foreach (ClausulaDeFiltrado filtro in filtros)
            {
                if (filtro.Clausula.ToLower() == ltrDeUnPermisoDtm.PermisosDeNegocio.ToLower())
                {
                    consulta = consulta.Where(x => x.IdAdministrador.Equals(filtro.Valor.Entero()) || x.IdGestor.Equals(filtro.Valor.Entero()) || x.IdConsultor.Equals(filtro.Valor.Entero()));
                    filtro.Aplicado = true;
                }

                if (filtro.Clausula.ToLower() == nameof(NegocioDtm.ElementoDtm).ToLower())
                {
                    if (filtro.Criterio == enumCriteriosDeFiltrado.igual)
                        consulta = consulta.Where(x => x.ElementoDtm == filtro.Valor);

                    if (filtro.Criterio == enumCriteriosDeFiltrado.contiene)
                        consulta = consulta.Where(x => x.ElementoDtm.Contains(filtro.Valor));

                    if (filtro.Criterio == enumCriteriosDeFiltrado.esAlgunoDe)
                    {
                        var ids = filtro.Valor.Split(',');
                        int[] lista = Array.Empty<int>();
                        int i = 0;
                        foreach (string s in ids)
                        {
                            lista[i] = s.Entero();
                            i++;
                        }

                        consulta = consulta.Where(x => lista.Contains(x.Id));
                    }
                    filtro.Aplicado = true;
                }
            }

            return consulta;
        }

        protected override IQueryable<NegocioDtm> AplicarJoins(IQueryable<NegocioDtm> registros, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            registros = base.AplicarJoins(registros, filtros, parametros);
            registros = registros.Include(p => p.Administrador);
            registros = registros.Include(p => p.Consultor);
            registros = registros.Include(p => p.Gestor);
            return registros;
        }

        protected override void AntesDePersistir(NegocioDtm registro, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(registro, parametros);

            ValidarDatos(registro, parametros);
            var negocio = NegociosDeSe.ToEnumerado(registro.Nombre);

            if (parametros.Operacion == enumTipoOperacion.Insertar)
            {
                registro.IdAdministrador = GestorDePermisos.CrearObtener(Contexto, negocio, registro.Nombre, enumClaseDePermiso.Negocio, enumModoDeAccesoDeDatos.Administrador).Id;
                registro.IdGestor = GestorDePermisos.CrearObtener(Contexto, negocio, registro.Nombre, enumClaseDePermiso.Negocio, enumModoDeAccesoDeDatos.Gestor).Id;
                registro.IdConsultor = GestorDePermisos.CrearObtener(Contexto, negocio, registro.Nombre, enumClaseDePermiso.Negocio, enumModoDeAccesoDeDatos.Consultor).Id;
            }

            if (parametros.Operacion == enumTipoOperacion.Modificar && (!parametros.Parametros.ContainsKey(NegociosDeSe.ActualizarSeguridad) || (bool)parametros.Parametros[NegociosDeSe.ActualizarSeguridad]))
            {
                var registroEnBD = registro;
                var permisosBd = Contexto.Set<PermisoDtm>();
                var administrador = permisosBd.LeerCacheadoPorId(registroEnBD.IdAdministrador);
                var gestor = permisosBd.LeerCacheadoPorId(registroEnBD.IdGestor);
                var consultor = permisosBd.LeerCacheadoPorId(registroEnBD.IdConsultor);

                registro.IdAdministrador = GestorDePermisos.ModificarPermisoDeDatos(Contexto, negocio, administrador, registro.Nombre, enumClaseDePermiso.Negocio, enumModoDeAccesoDeDatos.Administrador).Id;
                registro.IdGestor = GestorDePermisos.ModificarPermisoDeDatos(Contexto, negocio, gestor, registro.Nombre, enumClaseDePermiso.Negocio, enumModoDeAccesoDeDatos.Gestor).Id;
                registro.IdConsultor = GestorDePermisos.ModificarPermisoDeDatos(Contexto, negocio, consultor, registro.Nombre, enumClaseDePermiso.Negocio, enumModoDeAccesoDeDatos.Consultor).Id;
            }
        }


        protected override void DespuesDePersistir(NegocioDtm registro, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(registro, parametros);

            var cache = CacheDe.ModoAcceso_AlNegocio;
            var patron = $"Negocio:registro.Nombre";
            ServicioDeCaches.EliminarElementos(cache, patron);

            cache = $"{nameof(NegociosDeSe)}.{nameof(NegociosDeSe.LeerNegocioPorEnumerado)}";
            var indice = $"{nameof(enumNegocio)}-{registro.Enumerado}";
            ServicioDeCaches.EliminarElemento(cache, indice);

            cache = $"{nameof(NegociosDeSe)}.{nameof(NegociosDeSe.LeerNegocioPorId)}";
            indice = $"{nameof(IRegistro)}-{registro.Id}";
            ServicioDeCaches.EliminarElemento(cache, indice);

            cache = $"{nameof(NegociosDeSe)}.{nameof(NegociosDeSe.LeerNegocioPorNombre)}";
            indice = $"{nameof(INombre)}-{registro.Nombre}";
            ServicioDeCaches.EliminarElemento(cache, indice);

            cache = $"{nameof(NegociosDeSe)}.{nameof(NegociosDeSe.LeerNegocioPorDto)}";
            indice = $"{nameof(NegociosDeSe.Dto)}-{registro.ElementoDto}";
            ServicioDeCaches.EliminarElemento(cache, indice);

            cache = $"{nameof(NegociosDeSe)}.{nameof(NegociosDeSe.LeerNegocioPorDtm)}";
            indice = $"{nameof(NegociosDeSe.Dtm)}-{registro.ElementoDtm}";
            ServicioDeCaches.EliminarElemento(cache, indice);

            cache = $"{nameof(LeerNegocio)}";
            indice = $"{registro.Enumerado}";
            ServicioDeCaches.EliminarElemento(cache, indice);

            ServicioDeCaches.EliminarCache(nameof(APiDeElementos.VinculosCon));

            if (registro.UsaCentroGestor && parametros.Operacion == enumTipoOperacion.Modificar  && !Contexto.DatosDeConexion.CreandoModelo)
                GestorDeNegociosDeUnCg.CrearModificarNegocios(contexto: Contexto, negocio: registro);

            if (registro.UsaCentroGestor &&  parametros.Operacion == enumTipoOperacion.Insertar)
                GestorDeNegociosDeUnCg.CrearModificarNegocios(contexto: Contexto, negocio: registro);


        }

        private void ValidarDatos(NegocioDtm registro, ParametrosDeNegocio parametros)
        {
            if (parametros.Operacion.Equals(enumTipoOperacion.Eliminar)) return;

            if (registro.ElementoDtm.IsNullOrEmpty())
                GestorDeErrores.Emitir($"Ha de indicar la clase del objeto {registro.Nombre} es obligatorio");

            var encontrado = false;
            var ensamblado = Assembly.Load(nameof(ServicioDeDatos));
            foreach (var clase in ensamblado.DefinedTypes)
            {
                if (clase.FullName == registro.ElementoDtm)
                {
                    encontrado = true;
                    break;
                }
            }

            if (!encontrado)
                GestorDeErrores.Emitir($"La clase del elemento {registro.ElementoDtm} del negocio {registro.Nombre} debe existir");
        }

        public static void GenerarSeguridadParaElUsuario(ContextoSe contexto, int idUsuario, Action<string> logger = null)
        {
            PermisosPorNegocioSql.QuitarPermisos(contexto, idUsuario, calculado: true);
            var permisosPorNegocio = ApiDePermisos.PermisosCalculadosDe(contexto, idUsuario, enumClaseDePermiso.Negocio);
            foreach (var permiso in permisosPorNegocio)
            {
                var negocio = contexto.Set<NegocioDtm>().First(x => x.IdGestor == permiso.IdPermiso || x.IdConsultor == permiso.IdPermiso || x.IdAdministrador == permiso.IdPermiso);
                var tipoPermiso = enumModoDeAccesoDeDatos.Parse<enumModoDeAccesoDeDatos>(permiso.Tipo, true);
                OtorgarSeguridadDeAUnUsuarioParaUnNegocio(contexto, negocio, tipoPermiso, idUsuario, calculado: true);
            }
            PermisosPorNegocioSql.QuitarPermisos(contexto, idUsuario, calculado: false);
            var permisosDirectos = ApiDePermisos.PermisosDirectosDe(contexto, idUsuario, enumClaseDePermiso.Negocio);
            foreach (var permiso in permisosDirectos)
            {
                var negocio = contexto.Set<NegocioDtm>().First(x => x.IdGestor == permiso.IdPermiso || x.IdConsultor == permiso.IdPermiso || x.IdAdministrador == permiso.IdPermiso);
                var tipoPermiso = enumModoDeAccesoDeDatos.Parse<enumModoDeAccesoDeDatos>(permiso.Tipo, true);
                OtorgarSeguridadDeAUnUsuarioParaUnNegocio(contexto, negocio, tipoPermiso, idUsuario, calculado: false);
            }
        }

        public static void GenerarSeguridad(ContextoSe contexto, Action<string> logger = null)
        {
            var filtros = new List<ClausulaDeFiltrado>();
            var gNegocios = Gestor(contexto, contexto.Mapeador);
            var negocios = gNegocios.LeerRegistros(0, -1, filtros);
            PermisosPorNegocioSql.EliminarTodos(contexto);
            foreach (var negocio in negocios)
            {
                if (!negocio.UsaSeguridad) continue;

                logger?.Invoke($"Procesando seguridad del negocio: {negocio.Expresion}");
                OtorgarSeguridadPorNegocio(contexto, negocio, enumModoDeAccesoDeDatos.Administrador);
                OtorgarSeguridadPorNegocio(contexto, negocio, enumModoDeAccesoDeDatos.Gestor);
                OtorgarSeguridadPorNegocio(contexto, negocio, enumModoDeAccesoDeDatos.Consultor);
            }
        }

        private static void OtorgarSeguridadPorNegocio(ContextoSe contexto, NegocioDtm negocio, enumModoDeAccesoDeDatos tipoPermiso)
        {
            var filtro = new ClausulaDeFiltrado(nameof(UsuariosDeUnPermisoDtm.IdPermiso), enumCriteriosDeFiltrado.igual);
            filtro.Valor = tipoPermiso.Equals(enumModoDeAccesoDeDatos.Administrador)
                ? negocio.IdAdministrador.ToString()
                : tipoPermiso.Equals(enumModoDeAccesoDeDatos.Gestor)
                    ? negocio.IdGestor.ToString()
                    : negocio.IdConsultor.ToString();

            var usuariosConElPermiso = GestorDePermisosDeUnUsuario.Gestor(contexto, contexto.Mapeador).LeerRegistros(0, -1, new List<ClausulaDeFiltrado> { filtro });
            foreach (var usuario in usuariosConElPermiso)
            {
                OtorgarSeguridadDeAUnUsuarioParaUnNegocio(contexto, negocio, tipoPermiso, usuario.IdUsuario, true);
            }
        }

        private static void OtorgarSeguridadDeAUnUsuarioParaUnNegocio(ContextoSe contexto, NegocioDtm negocio, enumModoDeAccesoDeDatos tipoPermiso, int idUsuario, bool calculado)
        {
            if (tipoPermiso.Equals(enumModoDeAccesoDeDatos.Administrador))
            {
                PermisosPorNegocioSql.Otorgar(contexto, negocio.Id, idUsuario, negocio.IdAdministrador, calculado);
                PermisosPorNegocioSql.Otorgar(contexto, negocio.Id, idUsuario, negocio.IdGestor, calculado);
                PermisosPorNegocioSql.Otorgar(contexto, negocio.Id, idUsuario, negocio.IdConsultor, calculado);
            }
            else if (tipoPermiso.Equals(enumModoDeAccesoDeDatos.Gestor))
            {
                PermisosPorNegocioSql.Otorgar(contexto, negocio.Id, idUsuario, negocio.IdGestor, calculado);
                PermisosPorNegocioSql.Otorgar(contexto, negocio.Id, idUsuario, negocio.IdConsultor, calculado);
            }
            else
            {
                PermisosPorNegocioSql.Otorgar(contexto, negocio.Id, idUsuario, negocio.IdConsultor, calculado);
            }
        }

    }
}
