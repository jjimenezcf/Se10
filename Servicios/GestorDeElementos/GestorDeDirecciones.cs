using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Gestor.Errores;
using GestorDeElementos.Extensores;
using ModeloDeDto.Negocio;
using ServicioDeDatos;
using ServicioDeDatos.Callejero;
using ServicioDeDatos.Elemento;
using Utilidades;

namespace GestorDeElementos
{
    public class GestorDeDirecciones : GestorDeElementos<ContextoSe, DireccionDtm, DireccionDto>
    {
        public string _Tabla { get; }

        private enumNegocio _Negocio;

        public override enumNegocio Negocio => _Negocio;

        private IGestor gestor => NegociosDeSe.CrearGestor(Contexto, Negocio);

        public class MapearDirecciones : Profile
        {
            public MapearDirecciones()
            {
                CreateMap<DireccionDtm, DireccionDto>().ForMember(dto => dto.CodigoPostal, dtm => dtm.MapFrom(dtm => dtm.Cp));
                CreateMap<DireccionDto, DireccionDtm>().ForMember(dtm => dtm.Negocio, dto => dto.Ignore());
            }
        }

        public GestorDeDirecciones(ContextoSe contexto, enumNegocio negocio)
        : base(contexto, contexto.Mapeador)
        {
            _Tabla = ApiDeElementoDtm.TablaDeDireccion(negocio.TipoDtm());
            _Negocio = negocio;
            Contexto = contexto;
        }

        public static GestorDeDirecciones Gestor(ContextoSe contexto, enumNegocio negocio) => new GestorDeDirecciones(contexto, negocio);

        public static IEnumerable<DireccionDto> LeerDirecciones(ContextoSe contexto, enumNegocio negocio, int idElemento)
        {
            var gestor = Gestor(contexto, negocio);
            return gestor.LeerElementos(idElemento);
        }

        public static IEnumerable<DireccionDtm> LeerRegistros(ContextoSe contexto, enumNegocio negocio, int idElemento)
        {
            var gestor = Gestor(contexto, negocio);
            return gestor.LeerRegistros(idElemento, 0, -1);
        }

        public static DireccionDtm AsociarDireccion(ContextoSe contexto, enumNegocio negocio, int idElemento, DireccionDtm direccion, bool validaPermisosDePersistencia = false)
        {
            var gestor = Gestor(contexto, negocio);
            direccion.IdElemento = idElemento;
            direccion.Id = 0;
            return gestor.PersistirRegistro(direccion, new ParametrosDeNegocio(enumTipoOperacion.Insertar) { ValidarPermisosDePersistencia = validaPermisosDePersistencia });
        }

        public DireccionDtm LeerRegistroPorId(int id, bool emitirError = true)
        {
            var d = DireccionSql.LeerPorId(Contexto, _Tabla, Negocio.TipoDtm(), id, emitirError);
            if (d is not null) d.Negocio = Negocio;
            return d;
        }

        public IEnumerable<DireccionDtm> LeerRegistros(int idElemento, int posicion, int cantidad)
        {
            if (cantidad <= 0)
            {
                var cache = ServicioDeCaches.Obtener(CacheDe.Callejero_Direcciones);
                var indice = $"{Negocio.IdNegocio()}-{idElemento}";
                if (!cache.ContainsKey(indice))
                {
                    var direccionesParaCache = DireccionSql.DireccionesDeUnElemento(Contexto, _Tabla, Negocio.TipoDtm(), idElemento, posicion, cantidad);
                    foreach (var d in direccionesParaCache) d.Negocio = Negocio;
                    cache[indice] = direccionesParaCache;
                }
                return (IEnumerable<DireccionDtm>)cache[indice];
            }

            var direcciones = DireccionSql.DireccionesDeUnElemento(Contexto, _Tabla, Negocio.TipoDtm(), idElemento, posicion, cantidad);
            foreach (var d in direcciones) d.Negocio = Negocio;
            return direcciones;
        }

        public int ContarRegistros(int idElemento)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Callejero_ContarDirecciones);
            var indice = $"{Negocio.IdNegocio()}-{idElemento}";
            if (!cache.ContainsKey(indice))
            {
                cache[indice] = DireccionSql.ContarRegistros(Contexto, _Tabla, idElemento);
            }
            return (int)cache[indice];
        }

        public override DireccionDtm LeerRegistroPorId(int id, bool usarLaCache, bool traqueado, bool conBloqueo, bool aplicarJoin, bool errorSiNoHay, Dictionary<string, object> parametros = null)
        {
            return LeerRegistroPorId(id);
        }

        public IEnumerable<DireccionDto> LeerElementos(int idElemento)
        {
            var registros = LeerRegistros(idElemento, 0, -1);
            return MapearElementos(registros.ToList());
        }

        public new IEnumerable<DireccionDto> LeerElementos(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros, List<ClausulaDeOrdenacion> orden, Dictionary<string, object> parametros)
        {
            int idElemento = 0;
            foreach (var f in filtros.Where(f => f.Clausula == nameof(DireccionDtm.IdElemento)))
            {
                idElemento = f.Valor.Entero();
            }

            return LeerElementos(idElemento);
        }

        protected override void AntesDeMapearElRegistroParaInsertar(DireccionDto elemento, ParametrosDeNegocio opciones)
        {
            base.AntesDeMapearElRegistroParaInsertar(elemento, opciones);
            if (elemento.IdPais == 0 || elemento.IdProvincia == 0)
            {
                var municipio = (MunicipioDtm)NegociosDeSe.CrearGestor(Contexto, enumNegocio.Municipio).LeerRegistroPorId(elemento.IdMunicipio, true);
                elemento.IdPais = municipio.Provincia.IdPais;
                elemento.IdProvincia = municipio.IdProvincia;
            }
        }
        protected override void AntesDePersistir(DireccionDtm registro, ParametrosDeNegocio parametros)
        {
            registro.Negocio = Negocio;
            base.AntesDePersistir(registro, parametros);
            if (Negocio.UsaBaja())
            {
                var elemento = gestor.LeerRegistroPorId(registro.IdElemento, false);
                if (((IUsaBaja)elemento).Baja)
                    GestorDeErrores.Emitir($"No se pueden añadir direcciones al '{(((IRegistro)elemento).ImplementaUnElemento() ? ((IElementoDtm)elemento).Referencia() : ((INombre)elemento).Nombre)}' del negocio '{gestor.Negocio.Singular()}' por estar de baja.");
            }
        }

        protected override void Persistir(DireccionDtm registro, ParametrosDeNegocio parametros)
        {
            if (parametros.Operacion == enumTipoOperacion.Insertar)
            {
                registro.Id = DireccionSql.Insertar(Contexto, _Tabla,
                      registro.IdElemento
                    , registro.Calificador.ToString()
                    , registro.IdPais
                    , registro.IdProvincia
                    , registro.IdMunicipio
                    , registro.IdCalle
                    , registro.IdBarrio
                    , registro.IdZona
                    , registro.IdCp
                    , registro.Numero
                    , registro.Escalera
                    , registro.Piso
                    , registro.Puerta
                    , registro.Otros
                    , registro.Url).Id;
            }
            else
            if (parametros.Operacion == enumTipoOperacion.Modificar)
            {
                //if (registro.IdCreador != Contexto.DatosDeConexion.IdUsuario)
                //    GestorDeErrores.Emitir("No se pueden modificar las direcciones no añadidas por Ud.");

                if (Negocio.UsaTrazas()) parametros.registroEnBd = DireccionSql.LeerPorId(Contexto, _Tabla, Negocio.TipoDtm(), registro.Id);
                DireccionSql.Modificar(Contexto, _Tabla,
                                 registro.Id
                               , registro.Calificador.ToString()
                               , registro.IdPais
                               , registro.IdProvincia
                               , registro.IdMunicipio
                               , registro.IdCalle
                               , registro.IdBarrio
                               , registro.IdZona
                               , registro.IdCp
                               , registro.Numero
                               , registro.Escalera
                               , registro.Piso
                               , registro.Puerta
                               , registro.Otros
                               , registro.Url
                               , registro.Activo);
            }
            else
            if (parametros.Operacion == enumTipoOperacion.Eliminar)
            {

                if (Negocio.UsaTrazas()) parametros.registroEnBd = DireccionSql.LeerPorId(Contexto, _Tabla, Negocio.TipoDtm(), registro.Id);
                DireccionSql.Eliminar(Contexto, _Tabla, registro.Id);
            }
            else
                GestorDeErrores.Emitir($"La operacion {parametros.Operacion} no está permitida para las direcciones del negocio {Negocio.ToNombre()}");
        }

        protected override void DespuesDePersistir(DireccionDtm direccion, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(direccion, parametros);

            if (Negocio.UsaTrazas())
            {
                if (parametros.Operacion == enumTipoOperacion.Modificar)
                    ExtensorDeDirecciones.DireccionModificada(Contexto, Negocio, direccion, (DireccionDtm)parametros.registroEnBd);
                else if (parametros.Operacion == enumTipoOperacion.Eliminar)
                    ExtensorDeDirecciones.DireccionEliminada(Contexto, Negocio, (DireccionDtm)parametros.registroEnBd);
            }
        }

        protected override void EliminarCaches(DireccionDtm registro, ParametrosDeNegocio parametros)
        {
            base.EliminarCaches(registro, parametros);
            var indice = $"{Negocio.IdNegocio()}-{registro.IdElemento}";
            ServicioDeCaches.EliminarElemento(CacheDe.Callejero_Direcciones, indice);
            ServicioDeCaches.EliminarElemento(CacheDe.Callejero_ContarDirecciones, indice);
        }

        protected override void DespuesDeMapearElElemento(DireccionDtm direccion, DireccionDto direccionDto, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(direccion, direccionDto, parametros);
            direccionDto.IdNegocio = NegociosDeSe.IdNegocio(Negocio);
            direccionDto.Negocio = NegociosDeSe.ToNombre(Negocio);
            direccionDto.NombreDireccion = direccion.Nombre(Contexto);
            direccionDto.Expresion = direccion.Expresion(Contexto);

            direccionDto.IntraComunitaria = direccion.EsIntraComunitario(Contexto);
            direccionDto.ExtraComunitaria = direccion.EsExtraComunitario(Contexto);

            if (direccion.Url.IsNullOrEmpty()) direccionDto.Url = direccionDto.NombreDireccion;

            if (parametros.EnConsulta)
                direccionDto.ModoDeAcceso = ServicioDeDatos.Seguridad.enumModoDeAccesoDeDatos.Consultor;
            else
            {
                var elemento = (IElementoDtm)gestor.LeerRegistroPorId(direccion.IdElemento, false);
                direccionDto.ModoDeAcceso = Contexto.DatosDeConexion.IdUsuario == direccionDto.IdCreador || ExtensorDeElementos.EsInterventor(elemento, Contexto) ?
                    ServicioDeDatos.Seguridad.enumModoDeAccesoDeDatos.Gestor :
                    ServicioDeDatos.Seguridad.enumModoDeAccesoDeDatos.Consultor;
            }
        }
    }
}
