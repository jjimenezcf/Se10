using AutoMapper;
using Gestor.Errores;
using GestorDeElementos.Extensores;
using GestorDeElementos.Extensores.Elementos;
using GestoresDeNegocio.Negocio;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ModeloDeDto;
using ModeloDeDto.Negocio;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Terceros;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Utilidades;
namespace GestorDeElementos
{

    public class GestorDeElementos<TContexto, TRegistro, TElemento> : IGestor, IDisposable
        where TRegistro : RegistroDtm
        where TElemento : ElementoDto
        where TContexto : ContextoSe
    {
        public TContexto Contexto;
        public IMapper Mapeador => Contexto.Mapeador;

        protected static readonly ConcurrentDictionary<string, bool> CacheDeRecuentos = new ConcurrentDictionary<string, bool>();

        public bool HayFiltroPorId(List<ClausulaDeFiltrado> filtros) => filtros.Where(x => x.Clausula.ToLower() == nameof(IRegistro.Id).ToLower() && x.Criterio == enumCriteriosDeFiltrado.igual).Count() > 0;
        public Type TipoDeNegocioDto => typeof(TElemento);
        public Type TipoDtmDelNegocio => typeof(TRegistro);
        public string NombreDeLaClaseDto => TipoDeNegocioDto.FullName;
        public string NombreDeLaClaseDtm => TipoDtmDelNegocio.FullName;

        public virtual enumNegocio Negocio => NegociosDeSe.NegocioDeUnDtm(TipoDtmDelNegocio);

        private int? _idNegocio;
        protected int IdNegocio => _idNegocio is null ? (int)(_idNegocio = NegociosDeSe.EsNegocioDeBD(Negocio) ? NegociosDeSe.IdNegocio(Negocio) : 0) : (int)_idNegocio;

        public virtual Metadatos Metadatos => Negocio.ObtenerMetadatos();
        public virtual IGestorDeTipos GestorDeTipos => null;

        ContextoSe IGestor.Contexto => Contexto;

        public GestorDeElementos(TContexto contexto, IMapper mapeador)
        : this(contexto)
        {
            if (mapeador == null)
                throw new Exception("Falta definir el mapeador");

            if (Negocio == enumNegocio.No_Definido && typeof(TRegistro).ImplementaUnElemento())
                GestorDeErrores.Emitir($"Falta definir la propiedad Negocio en la clase {GetType().Name}");

            Contexto.Mapeador = mapeador;
        }

        public GestorDeElementos(TContexto contexto)
        {
            Contexto = contexto;
        }

        public GestorDeElementos(Func<TContexto> generadorDeContexto, IMapper mapeador)
        : this(generadorDeContexto(), mapeador)
        {
        }


        #region ASYNC

        #region Métodos de inserción ASYN

        public async Task InsertarElementoAsync(TElemento elemento, ParametrosDeNegocio parametros = null)
        {
            if (parametros == null)
                parametros = new ParametrosDeNegocio(enumTipoOperacion.Insertar);

            TRegistro elementoBD = MapearRegistro(elemento, parametros);
            Contexto.Add(elementoBD);
            await Contexto.SaveChangesAsync();
        }

        #endregion

        #region Métodos de modificación

        public async Task ModificarElementoAsync(TElemento elemento, ParametrosDeNegocio parametros = null)
        {
            if (parametros == null)
                parametros = new ParametrosDeNegocio(enumTipoOperacion.Modificar);

            TRegistro registro = MapearRegistro(elemento, parametros);
            await ModificarRegistroAsync(registro);
        }

        protected async Task ModificarRegistroAsync(TRegistro registro, ParametrosDeNegocio parametros = null)
        {
            if (parametros == null)
                parametros = new ParametrosDeNegocio(enumTipoOperacion.Modificar);

            Contexto.Update(registro);
            await Contexto.SaveChangesAsync();
        }

        #endregion

        #endregion

        #region Métodos de persistencia

        public bool IniciarTransaccion()
        {
            return Contexto.IniciarTransaccion();
        }

        public void Rollback(bool transaccion)
        {
            Contexto.Rollback(transaccion);
        }
        public void Commit(bool transaccion)
        {
            Contexto.Commit(transaccion);
        }


        public List<TElemento> PersistirElementosDto(List<TElemento> elementosDto, ParametrosDeNegocio parametros)
        {
            var lista = new List<TElemento>();
            foreach (var elementoDto in elementosDto)
                lista.Add(PersistirElementoDto(elementoDto, parametros));
            return lista;
        }

        public TElemento PersistirElementoDto(TElemento elementoDto, ParametrosDeNegocio parametros)
        {
            TRegistro registro;
            try
            {
                registro = MapearRegistro(elementoDto, parametros);
            }
            catch (Exception ex)
            {
                Contexto.AnotarExcepcion(ex);
                throw;
            }
            registro = PersistirRegistro(registro, parametros);

            if (parametros.Operacion.Equals(enumTipoOperacion.Eliminar))
                return elementoDto;

            return DespuesDePersistirElementoDto(elementoDto, registro, parametros);
        }

        protected virtual TElemento DespuesDePersistirElementoDto(TElemento elementoDto, TRegistro registro, ParametrosDeNegocio parametros)
        {
            var nuevoDto = MapearElemento(registro, parametros);
            return nuevoDto;
        }

        protected void PersistirRegistros(List<TRegistro> registros, ParametrosDeNegocio parametros)
        {
            var transaccion = Contexto.IniciarTransaccion();
            try
            {
                foreach (var registro in registros)
                {
                    PersistirRegistro(registro, parametros);
                }

                Contexto.Commit(transaccion);
            }
            catch (Exception)
            {
                Contexto.Rollback(transaccion);
                throw;
            }
        }

        public virtual TRegistro PersistirRegistro(TRegistro registro, ParametrosDeNegocio parametros)
        {
            if (parametros.Operacion != enumTipoOperacion.Insertar)
            {
                parametros.registroEnBd = LeerRegistroPorId(registro.Id, usarLaCache: false, traqueado: false, conBloqueo: false, aplicarJoin: parametros.AplicarJoin,
                   parametros: new Dictionary<string, object> { { ltrParametrosNeg.ValidarPermisosDeConsulta, false } });
            }

            if (parametros.Operacion == enumTipoOperacion.Insertar && registro.Id > 0)
            {
                GestorDeErrores.Emitir($"No puede insertar un registro del tipo {typeof(TRegistro).Name} con un id {registro.Id}. debe ser 0");
            }

            if (parametros.Modificando
                && Negocio.UsaBaja()
                && ApiDeInterfaceDtm.ImplementaUsaBaja(typeof(TRegistro))
                && ((IUsaBaja)parametros.registroEnBd).Baja
                && ((IUsaBaja)registro).Baja)
                GestorDeErrores.Emitir($"El registro '{((INombre)registro).Nombre}' está de baja, no es modificable");

            var idSemaforo = 0;
            if (!parametros.Insertando && IdNegocio > 0 && registro.GetType().ImplementaUnElemento())
                idSemaforo = SemaforoDeProcesoSql.PonerSemaforo(Contexto, IdNegocio
                             , registro.Id
                             , parametros.Operacion.ToBd(), $"{parametros.Operacion}: {typeof(TRegistro).Name}").Id;

            if (!parametros.Parametros.ContainsKey(ltrParametrosNeg.AccionQueSeEjecuta) && Contexto.Accion is not null)
            {
                parametros.Parametros[ltrParametrosNeg.AccionQueSeEjecuta] = Contexto.Accion.Nombre;
            }

            var transaccion = Contexto.IniciarTransaccion();
            EliminarCaches(registro, parametros);
            try
            {
                ValidarPermisosDePersistencia(registro, parametros);
                AntesDePersistir(registro, parametros);
                try
                {
                    Negocio.EjecutarEventos(Contexto, registro, parametros.Insertando ? enumMomentoDeAccion.AC : parametros.Modificando ? enumMomentoDeAccion.AM : enumMomentoDeAccion.AE);
                    Persistir(registro, parametros);
                    Negocio.EjecutarEventos(Contexto, registro, parametros.Insertando ? enumMomentoDeAccion.DC : parametros.Modificando ? enumMomentoDeAccion.DM : enumMomentoDeAccion.DE);
                }
                finally
                {
                    EliminarCaches(registro, parametros);
                }

                DespuesDePersistir(registro, parametros);

                Contexto.Commit(transaccion);
            }
            catch (Exception e)
            {
                Contexto.Rollback(transaccion, e);
                throw;
            }
            finally
            {
                EliminarCaches(registro, parametros);
                LiberarLibro(registro, parametros);
                if (!parametros.Insertando && IdNegocio > 0 && registro.GetType().ImplementaUnElemento())
                    SemaforoDeProcesoSql.QuitarSemaforo(Contexto, idSemaforo);
            }

            if (parametros.Operacion == enumTipoOperacion.Eliminar)
                return (TRegistro)parametros.registroEnBd;

            return parametros.AplicarJoin
            ? LeerRegistroPorId(registro.Id, usarLaCache: false, traqueado: false, conBloqueo: false, aplicarJoin: true)
            : registro;
        }


        protected virtual void Persistir(TRegistro registro, ParametrosDeNegocio parametros)
        {
            var referenciados = registro.PropiedadesDelObjeto().Where(x => x.PropertyType.HeredaDe(typeof(RegistroDtm))).ToList();
            foreach (var referenciado in referenciados.Where(x => x.SetMethod != null)) referenciado.SetValue(registro, null);
            if (parametros.Operacion == enumTipoOperacion.Insertar)
                Contexto.Add(registro);
            else
            if (parametros.Operacion == enumTipoOperacion.Modificar)
                Contexto.Update(registro);
            else
            if (parametros.Operacion == enumTipoOperacion.Eliminar)
                Contexto.Remove(registro);
            else
                GestorDeErrores.Emitir($"Solo se pueden persistir operaciones del tipo {enumTipoOperacion.Insertar} o  {enumTipoOperacion.Modificar} o {enumTipoOperacion.Eliminar}");
            try
            {
                Contexto.SaveChanges();
            }
            catch (Exception e)
            {
                var mensaje = e.MensajeCompleto().ToLower();
                if (parametros.Eliminando && mensaje.Contains("The DELETE statement conflicted with the REFERENCE".ToLower()))
                {
                    var referencia = Negocio.UsaReferencia() ? ((IUsaReferencia)registro).Referencia : registro.ImplementaNombre() ? ((INombre)registro).Nombre : registro.Id.ToString();
                    GestorDeErrores.Emitir($"No se puede eliminar el registro '{referencia}' del negocio '{Negocio.Singular()}' ya que está siendo referenciado, de lo de baja");
                }

                if ((parametros.Modificando || parametros.Eliminando) && e.MensajeCompleto().ToLower().Contains("concurrency exceptions"))
                {
                    var referencia = Negocio.UsaReferencia() ? ((IUsaReferencia)registro).Referencia : registro.ImplementaNombre() ? ((INombre)registro).Nombre : registro.Id.ToString();
                    GestorDeErrores.Emitir($"No se puede modificar el registro '{referencia}' del negocio '{Negocio.Singular()}' ya que ha sido modificado por otro usuario, vuelva a leerlo");
                }

                if (!parametros.AccionQueSeEjecuta.IsNullOrEmpty())
                {
                    var referencia = Negocio.UsaReferencia() ? ((IUsaReferencia)registro).Referencia : registro.ImplementaNombre() ? ((INombre)registro).Nombre : registro.Id.ToString();
                    throw new Exception($"Error al ejecutar la acción '{parametros.AccionQueSeEjecuta}' usando el registro '{referencia}' del negocio '{Negocio.Singular()}'", e);
                }

                if (mensaje.Contains("was deadlocked on lock resources with another process and has been chosen as the deadlock victim. Rerun the transaction"))
                {
                    var referencia = Negocio.UsaReferencia() ? ((IUsaReferencia)registro).Referencia : registro.ImplementaNombre() ? ((INombre)registro).Nombre : registro.Id.ToString();
                    GestorDeErrores.Emitir($"El registro '{referencia}' del negocio '{Negocio.Singular()}' estaba bloqueado, vuelva a intentarlo");
                }

                throw;
            }
            finally
            {
                Contexto.ChangeTracker.Clear();
                EliminarCaches(registro, parametros);
            }


        }

        protected virtual void ValidarPermisosDePersistencia(TRegistro registro, ParametrosDeNegocio parametros)
        {
            ApiDePermisos.ValidarPermisosDePersistencia(Contexto, Negocio, TipoDtmDelNegocio, parametros, registro);
        }

        private void LiberarLibro(TRegistro registro, ParametrosDeNegocio parametros)
        {
            if (parametros.Operacion == enumTipoOperacion.Insertar && NegociosDeSe.UsaTipo(Negocio) && typeof(TRegistro).ImplementaUsaReferencia() && parametros.Libro != null)
            {
                LibroSql.Desbloquear(parametros.Libro.Id);
            }
        }

        public virtual void BorrarRegistros(IQueryable<TRegistro> consulta)
        {
            var transaccion = Contexto.IniciarTransaccion();
            try
            {
                Contexto.RemoveRange(consulta);
                Contexto.SaveChanges();
                Contexto.Commit(transaccion);
            }
            catch (Exception)
            {
                Contexto.Rollback(transaccion);
                throw;
            }
            finally
            {
                ServicioDeCaches.EliminarCaches(typeof(TRegistro).FullName);
                if (typeof(TRegistro).ImplemtaUnDetalle())
                {
                    VaciarCacheDeDetalle(typeof(TRegistro));
                }
            }
        }

        protected virtual void DespuesDePersistir(TRegistro registro, ParametrosDeNegocio parametros)
        {
            if (!Contexto.DatosDeConexion.CreandoModelo && typeof(TRegistro).ImplementaUnElemento() && Negocio.UsaAuditoria())
            {
                var negocio = NegociosDeSe.NegocioDeUnDtm(typeof(TRegistro));
                var auditar = parametros.Operacion == enumTipoOperacion.Modificar ? parametros.registroEnBd : registro;
                AuditoriaDeElementos.RegistrarAuditoria(Contexto, negocio, parametros.Operacion, (IElementoDtm)auditar);
            }

            if (parametros.Insertando)
            {
                if (typeof(TRegistro).ImplementaUsaFlujo()) GestorDeHitos.IniciarFlujo(Contexto, Negocio,
                    registro.Id,
                    parametros.TipoConFujo,
                    parametros.FechaDeCreacion);

                if (typeof(TRegistro).ImplementaUnElemento() && typeof(TRegistro).ImplementaTieneReferencia() && parametros.Parametros.LeerValor(ltrParametrosNeg.CrearPermisosDelElemento, false))
                    ((IElementoDtm)registro).CrearPermisosDelElemento(Contexto);

                var crearDireccionJson = parametros.Parametros.LeerValor<JObject>(Ampliaciones.Comunes.DireccionAlCrear, null);
                if (crearDireccionJson != null)
                {
                    var crearDireccionDto = crearDireccionJson.ToObject<CrearDireccionDto>();
                    if (crearDireccionDto.IdCalle.Entero() > 0) ((IElementoDtm)registro).CrearDireccion(Contexto, crearDireccionDto);
                }
                else if (parametros.Parametros.ContieneClave(Ampliaciones.Comunes.DireccionAlCrear))
                {
                    var crearDireccionDto = parametros.Parametros.LeerValor<CrearDireccionDto>(Ampliaciones.Comunes.DireccionAlCrear);
                    if (crearDireccionDto.IdCalle.Entero() > 0) ((IElementoDtm)registro).CrearDireccion(Contexto, crearDireccionDto);
                }
            }

            if (parametros.Operacion == enumTipoOperacion.Modificar && typeof(TRegistro).ImplementaUsaBaja())
            {
                if (parametros.EstaDandoDeBaja((IUsaBaja)registro)) AlDarDeBaja(registro, parametros);
                if (parametros.EstaDandoDeAlta((IUsaBaja)registro)) AlDarDeAlta(registro, parametros);
            }

            var indice = typeof(TRegistro).FullName;
            CacheDeRecuentos[indice] = true;
        }

        protected virtual void EliminarCaches(TRegistro registro, ParametrosDeNegocio parametros)
        {
            VaciarCacheDeRegistro(registro, parametros);
        }

        protected void VaciarCacheDeRegistro(TRegistro registro, ParametrosDeNegocio parametros)
        {
            VaciarCacheDeRegistro(registro, parametros.Operacion,
                     typeof(TRegistro).ImplementaNombre()
                     ? parametros.Operacion == enumTipoOperacion.Modificar
                     ? (string)parametros.registroEnBd.LeerPropiedad(nameof(INombre.Nombre))
                     : ((INombre)registro).Nombre
                     : "");

            if (typeof(TRegistro).ImplementaElementoTipado())
                ServicioDeCaches.EliminarElementosComiencenPor(CacheDe.Ent_SeleccionarPorPropiedad, patron: typeof(TRegistro).FullName);
        }

        protected void VaciarCacheDeRegistro(TRegistro registro, enumTipoOperacion operacion, string nombre)
        =>
        registro.VaciarCacheDeRegistro(Contexto, operacion, nombre);
        //{
        //if (operacion != enumTipoOperacion.Insertar && typeof(TRegistro).ImplementaNombre())
        //    VaciarCacheDeRegistroPorNombre(nombre);

        //VaciarCacheDeRegistroPorId(registro.Id);

        //ServicioDeCaches.EliminarCache(ServicioDeCaches.PorAk(typeof(TRegistro).FullName));
        //ServicioDeCaches.EliminarCache(ServicioDeCaches.PorPropiedad(typeof(TRegistro).FullName));
        //ServicioDeCaches.EliminarElemento(CacheDe.Dtm_HayRegistros, typeof(TRegistro).FullName);

        //if (registro.ImplemtaUnaAmpliacion())
        //{
        //    var elemento = ((IAmpliacion)registro).AmpliacionDe(Contexto);
        //    var clave = $"{typeof(TRegistro).Name}-{((IAmpliacion)registro).IdElemento}";
        //    if (operacion != enumTipoOperacion.Insertar)
        //    {
        //        ServicioDeCaches.EliminarElemento(CacheDe.AmpliacionesSinJoin, clave);
        //        ServicioDeCaches.EliminarElemento(CacheDe.AmpliacionesConJoin, clave);
        //    }
        //    VaciarCacheDeRegistroPorId(elemento.GetType(), ((IAmpliacion)registro).IdElemento);
        //    VaciarCacheDeRegistroPorNombre(elemento.Nombre);
        //    ServicioDeCaches.EliminarCache(ServicioDeCaches.PorAk(elemento.GetType().FullName));
        //    ServicioDeCaches.EliminarCache(ServicioDeCaches.PorPropiedad(elemento.GetType().FullName));
        //}

        //if (registro.ImplemtaUnDetalle())
        //{
        //    var elemento = ((IDetalle)registro).DetalleDe(Contexto);
        //    VaciarCacheDeDetalle(typeof(TRegistro), ((IDetalle)registro).IdElemento);
        //    VaciarCacheDeRegistroPorId(elemento.GetType(), ((IDetalle)registro).IdElemento);
        //    VaciarCacheDeRegistroPorNombre(elemento.Nombre);
        //    ServicioDeCaches.EliminarCache(ServicioDeCaches.PorAk(elemento.GetType().FullName));
        //    ServicioDeCaches.EliminarCache(ServicioDeCaches.PorPropiedad(elemento.GetType().FullName));
        //}

        //var tiposQueReferencian = ApiDeRegistroDtm.EncontrarTiposQueReferencian<TRegistro>();
        //foreach (var tipo in tiposQueReferencian)
        //{
        //    ServicioDeCaches.EliminarCachesDelTipo(tipo);
        //}
        //}


        protected static void VaciarCacheDeDetalle(Type tipo, int idElemento)
        =>
        ExtensorDeCachesRegistro.VaciarCacheDeDetalle(tipo, idElemento);
        //{
        //    var clave = $"{tipo.Name}-{idElemento}";
        //    ServicioDeCaches.EliminarElemento(CacheDe.HayDetalle, clave);
        //    ServicioDeCaches.EliminarElemento(CacheDe.Detalle, clave + "-S");
        //    ServicioDeCaches.EliminarElemento(CacheDe.Detalle, clave + "-N");
        //}

        protected static void VaciarCacheDeDetalle(Type tipo)
        =>
        ExtensorDeCachesRegistro.VaciarCacheDeDetalle(tipo);
        //{
        //    var patron = $"{tipo.Name}";
        //    ServicioDeCaches.EliminarElemento(CacheDe.HayDetalle, patron);
        //    ServicioDeCaches.EliminarElementos(CacheDe.Detalle, patron);
        //}

        private static void VaciarCacheDeRegistroPorId(int id) => VaciarCacheDeRegistroPorId(typeof(TRegistro), id);

        protected static void VaciarCacheDeRegistroPorId(Type tipo, int id)
        =>
        ExtensorDeCachesRegistro.VaciarCacheDeRegistroPorId(tipo, id);
        //{
        //    var clavePorId = $"{nameof(IRegistro.Id)}-{id}";
        //    ServicioDeCaches.EliminarElemento(ServicioDeCaches.PorId(tipo.FullName), $"{clavePorId}-{ServicioDeCaches.enumConJoin.si}");
        //    ServicioDeCaches.EliminarElemento(ServicioDeCaches.PorId(tipo.FullName), $"{clavePorId}-{ServicioDeCaches.enumConJoin.no}");
        //}

        //protected static void VaciarCacheDeRegistroPorNombre(string nombre)
        //{
        //    var clavePorNombre = $"{nameof(INombre.Nombre)}-{nombre}";
        //    ServicioDeCaches.EliminarElemento(ServicioDeCaches.PorNombre(typeof(TRegistro).FullName), $"{clavePorNombre}-{ServicioDeCaches.enumConJoin.si}");
        //    ServicioDeCaches.EliminarElemento(ServicioDeCaches.PorNombre(typeof(TRegistro).FullName), $"{clavePorNombre}-{ServicioDeCaches.enumConJoin.no}");
        //}


        protected virtual void AlDarDeAlta(TRegistro registro, ParametrosDeNegocio parametros)
        {
            GestorDeTrazas.Alta(Contexto, Negocio, registro);
        }

        protected virtual void AlDarDeBaja(TRegistro registro, ParametrosDeNegocio parametros)
        {
            GestorDeTrazas.Baja(Contexto, Negocio, registro);
        }

        protected virtual void AntesDePersistir(TRegistro registro, ParametrosDeNegocio parametros)
        {
            if (registro.ImplementaAuditoria() && (parametros.Modificando || parametros.Eliminando))
            {
                if (((IAuditoria)parametros.registroEnBd).FechaModificacion.HasValue &&
                    ((IAuditoria)parametros.registroEnBd).FechaModificacion > ((IAuditoria)registro).FechaModificacion)
                    GestorDeErrores.Emitir($"El registro ha sido modificado por el usuario {Contexto.SeleccionarPorId<UsuarioDtm>((int)((IAuditoria)parametros.registroEnBd).IdUsuaModi).Login} vuelva a leerlo");
            }

            if (registro.GetType().ImplementaUsaBloqueo() && !parametros.Insertando) ValidacionesDeBloqueos(registro, parametros);

            if (registro.ImplementaNombre())
            {
                if (((INombre)registro).Nombre.IsNullOrEmpty())
                    GestorDeErrores.Emitir($"La propiedad {nameof(INombre.Nombre)} del objeto {typeof(TRegistro).Name} es obligatorio");
                else
                    ((INombre)registro).Nombre = ((INombre)registro).Nombre.Trim();
            }

            ValidarLongitudes(registro);

            if (registro.ImplementaUnaRelacion())
            {
                if (((IRelacion)registro).IdElemento1 == 0)
                    GestorDeErrores.Emitir($"El id asociado a la propiedad {((IRelacion)registro).PropiedadDelIdElemento1} no puede ser 0");
                if (((IRelacion)registro).IdElemento2 == 0)
                    GestorDeErrores.Emitir($"El id asociado a la propiedad {((IRelacion)registro).PropiedadDelIdElemento2} no puede ser 0");
            }

            if (EsTregistroDeParametrizacion())
                return;

            var tipoDeElemento = typeof(TRegistro).ImplementaElementoConTipo() ? ((IElementoConTipo)registro).AntesDePersistirValidarTipo(Contexto, parametros) : null;

            if (registro.ImplemtaUnaAmpliacion())
            {
                var elementoDeLaAmpliacion = ((IAmpliacion)registro).Negocio.ElementoPorId(Contexto, ((IAmpliacion)registro).IdElemento);
                var negocio = ((IAmpliacion)registro).Negocio;
                if (!parametros.Parametros.ContainsKey(negocio.TipoDtm().Name)) parametros.Parametros[negocio.TipoDtm().Name] = ((IAmpliacion)registro).AmpliacionDe(Contexto);
            }

            if (ApiDeInterfaceDtm.ImplementaUsaCg(typeof(TRegistro))) ExtensorDeElementos.AntesDePersistirValidarCg(Contexto, (IUsaCg)registro, parametros);

            if (typeof(TRegistro).ImplementaUsaReferencia())
            {
                if (parametros.Operacion == enumTipoOperacion.Insertar)
                    BloquearLibro(registro, parametros, tipoDeElemento);
                if (((IUsaReferencia)registro).Referencia.IsNullOrEmpty()) GestorDeErrores.Emitir($"El campo referencia es obligatorio");
            }

            if (registro.ImplementaAuditoria())
            {
                if (parametros.Insertando)
                {
                    if (parametros.FechaDeCreacion != default && ((IAuditoria)registro).FechaCreacion > DateTime.Now)
                    {
                        GestorDeErrores.Emitir($"Está indicando una fecha de creación posterior al día de hoy '{((IAuditoria)registro).FechaCreacion.ToString()}' y eso no se permite");
                    }
                    ((IAuditoria)registro).IdUsuaCrea = Contexto.DatosDeConexion.IdUsuario;
                    ((IAuditoria)registro).FechaCreacion = parametros.FechaDeCreacion == default ? DateTime.Now : parametros.FechaDeCreacion;
                }
                else
                if (parametros.Operacion == enumTipoOperacion.Modificar)
                {
                    ((IAuditoria)registro).IdUsuaCrea = ((IAuditoria)parametros.registroEnBd).IdUsuaCrea;
                    ((IAuditoria)registro).FechaCreacion = ((IAuditoria)parametros.registroEnBd).FechaCreacion;
                    ((IAuditoria)registro).IdUsuaModi = Contexto.DatosDeConexion.IdUsuario;
                    if (parametros.Parametros.LeerValor<DateTime>(ltrParametrosNeg.FechaDeTransicion, default) == default)
                        ((IAuditoria)registro).FechaModificacion = DateTime.Now;
                    else
                        ((IAuditoria)registro).FechaModificacion = parametros.Parametros.LeerValor<DateTime>(ltrParametrosNeg.FechaDeTransicion, default);
                }
            }

            if (registro.ImplementaUsaSolicitante())
            {
                if (parametros.Insertando)
                {
                    var solicitante = Contexto.SeleccionarPorId<InterlocutorDtm>(((IUsaSolicitante)registro).IdSolicitante, aplicarJoin: true);
                    parametros.Parametros[nameof(InterlocutorDtm)] = solicitante;
                    ((IUsaSolicitante)registro).Contacto = ((IUsaSolicitante)registro).Contacto.IsNullOrEmpty() ? solicitante.Expresion : ((IUsaSolicitante)registro).Contacto;
                    ((IUsaSolicitante)registro).Telefono = ((IUsaSolicitante)registro).Telefono.IsNullOrEmpty() ? solicitante.Telefono : ((IUsaSolicitante)registro).Telefono;
                    ((IUsaSolicitante)registro).eMail = ((IUsaSolicitante)registro).eMail.IsNullOrEmpty() ? solicitante.eMail : ((IUsaSolicitante)registro).eMail;
                }
            }


            if (parametros.Eliminando && Negocio.UsaArchivos() && typeof(TRegistro).ImplementaUnElemento())
            {
                if (((IElementoDtm)registro).Archivos(Contexto).Any())
                    GestorDeErrores.Emitir($"No se puede eliminar el '{((IElementoDtm)registro).Referencia(Contexto)}' por tener archivos adjuntos, elimínelos previamente o de de baja el '{Negocio.Singular()}'");
            }
        }

        private void ValidacionesDeBloqueos(TRegistro registro, ParametrosDeNegocio parametros)
        {
            if (parametros.AccionQueSeEjecuta != ltrDeUnElemento.Accion_Bloquear && parametros.AccionQueSeEjecuta != ltrDeUnElemento.Accion_Desbloquear && ((IUsaBloqueo)parametros.registroEnBd).Bloqueado)
            {
                var bloqueador = ((IUsaBloqueo)parametros.registroEnBd).Bloqueador(Contexto);
                if (bloqueador is not null && parametros.ValidarTrazaDeBloqueo)
                {
                    var referencia = ((IElementoDtm)registro).Referencia(Contexto);
                    var mensaje = ltrDeTrazas.Error_De_Modificar_Bloqueado.Replace("Referencia", referencia);
                    mensaje = mensaje.Replace("Bloqueador", bloqueador.Login);
                    GestorDeErrores.Emitir(mensaje);
                }
            }

            if (parametros.AccionQueSeEjecuta == ltrDeUnElemento.Accion_Bloquear && ((IUsaBloqueo)parametros.registroEnBd).Bloqueado)
                GestorDeErrores.Emitir(ltrDeTrazas.Error_De_Bloqueo.Replace("Referencia", ((IElementoDtm)registro).Referencia(Contexto)).Replace("Bloqueador", ((IUsaBloqueo)registro).Bloqueador(Contexto).Login));

            if (parametros.AccionQueSeEjecuta == ltrDeUnElemento.Accion_Desbloquear && !((IUsaBloqueo)parametros.registroEnBd).Bloqueado)
            {
                var desbloqueador = ((IUsaBloqueo)registro).Desbloqueador(Contexto);
                if (desbloqueador is null)
                    GestorDeErrores.Emitir(ltrDeTrazas.Error_De_NoHayBloqueo.Replace("Referencia", ((IElementoDtm)registro).Referencia(Contexto)));
                else
                    GestorDeErrores.Emitir(ltrDeTrazas.Error_De_Desbloqueo.Replace("Referencia", ((IElementoDtm)registro).Referencia(Contexto)).Replace("desbloqueador", desbloqueador.Login));
            }
        }

        private void ValidarLongitudes(TRegistro registro)
        {
            var tipoTabla = Contexto.Model.FindEntityType(typeof(TRegistro));
            if (tipoTabla == null)
                return;
            var propiedades = tipoTabla.GetProperties();

            foreach (var propiedad in propiedades)
            {
                if (propiedad.ClrType != typeof(string))
                    continue;

                var longitudMaxima = propiedad.GetMaxLength();
                if (longitudMaxima is null)
                {
                    var tipoColumna = propiedad.GetColumnType();
                    if (tipoColumna != null && tipoColumna.StartsWith(IDominio.VARCHAR, StringComparison.CurrentCultureIgnoreCase))
                    {
                        var match = Regex.Match(tipoColumna, $@"{IDominio.VARCHAR}\((\d+)\)", RegexOptions.IgnoreCase);
                        if (match.Success)
                        {
                            longitudMaxima = int.Parse(match.Groups[1].Value);
                        }
                    }
                    else if (tipoColumna != null && tipoColumna.StartsWith(IDominio.CHAR, StringComparison.CurrentCultureIgnoreCase))
                    {
                        var match = Regex.Match(tipoColumna, $@"{IDominio.CHAR}\((\d+)\)", RegexOptions.IgnoreCase);
                        if (match.Success)
                        {
                            longitudMaxima = int.Parse(match.Groups[1].Value);
                        }
                    }
                }
                if (longitudMaxima != null && longitudMaxima > 0 && registro.LeerPropiedad(propiedad.Name)?.ToString().Length > longitudMaxima)
                {
                    GestorDeErrores.Emitir($"La propiedad '{propiedad.Name}' tiene una longitud de '{registro.LeerPropiedad(propiedad.Name)?.ToString().Length ?? 0}', valor: '{registro.LeerPropiedad(propiedad.Name)?.ToString() ?? "nulo"}', y ésta excede de '{longitudMaxima}', longitud permitida en la base de datos.");
                }
            }
        }

        private bool EsTregistroDeParametrizacion()
        {

            if (typeof(TRegistro).ImplementaUnTipoDeElemento())
                return true;

            if (typeof(TRegistro).ImplementaUnEstado())
                return true;

            if (typeof(TRegistro).ImplementaUnaTransicion())
                return true;

            if (typeof(TRegistro).ImplementaUnaAccionDeTrn())
                return true;

            if (typeof(TRegistro).ImplementaUnaAccionDeRelacion())
                return true;

            if (typeof(TRegistro).ImplementaUnaDireccion())
                return true;

            if (typeof(TRegistro).ImplementaPermisosDelElemento())
                return true;

            if (typeof(TRegistro).ImplementaUnaTraza())
                return true;

            if (typeof(TRegistro).ImplementaUnaObservacion())
                return true;

            if (typeof(TRegistro).ImplementaUnHito())
                return true;

            return false;
        }

        private void BloquearLibro(TRegistro registro, ParametrosDeNegocio parametros, TipoDeElementoDtm tipoDeElemento)
        {
            if (!NegociosDeSe.UsaTipo(Negocio))
            {
                // GestorDeErrores.Emitir($"Para poder definir nº de referencia se necesita que el negocio {Negocio} implemente tipo");
                return;
            }

            //if (tipoDeElemento.ClaseDeLibro.Equals(enumClaseDeLibro.SIN_LIBRO))
            //    return;

            var idNegocio = NegociosDeSe.LeerNegocioPorEnumerado(Negocio).Id;
            var ejercicio = DateTime.Now.Year;
            var idCg = 0;
            var idTipo = 0;
            string siglaCg = null;
            if (tipoDeElemento.ClaseDeLibro.Equals(enumClaseDeLibro.POR_CG) || tipoDeElemento.ClaseDeLibro.Equals(enumClaseDeLibro.POR_CG_TIPO))
            {
                if (!typeof(TRegistro).ImplementaPermisosPorCg())
                    GestorDeErrores.Emitir($"Ha indicado que el libro de registro para un elemento del negocio {Negocio} y tipo {tipoDeElemento.Nombre} usa el CG, y el elemento no usa centro gestor");

                idCg = ((IUsaCg)registro).IdCg;
                var cg = CentroGestorSql.LeerCgPorId(Contexto, idCg);
                siglaCg = cg.Sigla;

                if (siglaCg.IsNullOrEmpty())
                    GestorDeErrores.Emitir($"Ha indicado que el libro de registro usa la sigla del CG para la referencia, y le no le ha asignado la sigla al CG {cg.Nombre}");
            }

            if (tipoDeElemento.ClaseDeLibro.Equals(enumClaseDeLibro.POR_TIPO) || tipoDeElemento.ClaseDeLibro.Equals(enumClaseDeLibro.POR_CG_TIPO))
                idTipo = ((IUsaTipo)registro).IdTipo;

            parametros.Libro = LibroSql.CrearObtener(Contexto, ejercicio, idNegocio, idCg, idTipo);
            LibroSql.Bloquear(parametros.Libro.Id, Contexto.DatosDeConexion.IdUsuario);

            if (CacheDeVariable.Cfg_HayResetearLibros && parametros.Libro.Valor == 0) ResetearLibroDeRegistro(parametros, idCg, idTipo);

            ((IUsaReferencia)registro).Referencia = $"{ejercicio}{(siglaCg.IsNullOrEmpty() ? "" : $"-{siglaCg}")}-{tipoDeElemento.Sigla}-{parametros.Libro.Valor + 1}";
            LibroSql.Incrementar(Contexto, parametros.Libro.Id);
        }

        private void ResetearLibroDeRegistro(ParametrosDeNegocio parametros, int idCg, int idTipo)
        {
            var rango = $"{new DateTime(DateTime.Now.Year, 1, 1)}-{DateTime.Now}";
            var porFecha = new ClausulaDeFiltrado(nameof(ElementoDtm.FechaCreacion), enumCriteriosDeFiltrado.entreFechas, rango);
            var filtro = new List<ClausulaDeFiltrado> { porFecha };
            filtro.Add(new ClausulaDeFiltrado(ltrParametrosNeg.QueMostrar, enumCriteriosDeFiltrado.igual, ltrParametrosNeg.MostrarTodos));
            if (idTipo > 0) filtro.Add(new ClausulaDeFiltrado(nameof(IUsaTipo.IdTipo), enumCriteriosDeFiltrado.igual, idTipo));
            if (idCg > 0) filtro.Add(new ClausulaDeFiltrado(nameof(IUsaCg.IdCg), enumCriteriosDeFiltrado.igual, idCg));
            var ultimo = Contexto.SeleccionarUltimo<TRegistro>(nameof(IElementoDtm.FechaCreacion), filtro, new ParametrosDeNegocio(enumTipoOperacion.LeerSinBloqueo));
            if (ultimo != null)
            {
                var partes = ((IUsaReferencia)ultimo).Referencia.Split("-");
                var valor = partes[partes.Length - 1].Entero();
                LibroSql.Resetear(Contexto, parametros.Libro.Id, valor);
                parametros.Libro.Valor = valor;
            }
        }

        internal static void ValidarQueSonIguales(TRegistro elementoLeido, TRegistro elementoPorPersistir)
        {
            var a = ApiDeEnsamblados.PropiedadesDelObjeto(elementoPorPersistir);
            foreach (var propiedad in a)
            {
                if (ApiDeEnsamblados.TienenLaPropiedad(elementoLeido.GetType(), propiedad.Name))
                {
                    if (!elementoLeido.GetType().GetProperty(propiedad.Name).GetValue(elementoLeido).Equals(elementoPorPersistir.GetType().GetProperty(propiedad.Name).GetValue(elementoPorPersistir)))
                        GestorDeErrores.Emitir($"La información que está intentando persistir en la BD es diferente de la que hay en la BD, actualice y vuelva a intentarlo");
                }
            }
        }

        #endregion

        #region Métodos de lectura
        object IGestor.LeerElementoPorId(int id, Dictionary<string, object> opcionesDelMapeo)
        {
            return LeerElementoPorId(id, opcionesDelMapeo);
        }

        public TElemento LeerElementoPorId(int id, Dictionary<string, object> opcionesDelMapeo = null)
        {
            TRegistro registro;
            var parametros = new ParametrosDeNegocio(enumTipoOperacion.LeerSinBloqueo);
            if (opcionesDelMapeo != null) foreach (var opcion in opcionesDelMapeo)
                    parametros.Parametros[opcion.Key] = opcion.Value;

            if (opcionesDelMapeo.ContieneClave(ltrParametrosNeg.ErrorSiNoLoHay))
                registro = LeerRegistroPorId(id, usarLaCache: opcionesDelMapeo.LeerValor(ltrParametrosNeg.UsarLaCache, true),
                    traqueado: false, conBloqueo: false, aplicarJoin: true,
                    errorSiNoHay: opcionesDelMapeo.LeerValor(ltrParametrosNeg.ErrorSiNoLoHay, true),
                    parametros: opcionesDelMapeo);
            else
                registro = LeerRegistroPorId(id, usarLaCache: opcionesDelMapeo.LeerValor(ltrParametrosNeg.UsarLaCache, true), traqueado: false, conBloqueo: false, aplicarJoin: true, parametros: opcionesDelMapeo);

            if (registro == null)
                return null;

            var elemento = MapearElemento(registro, parametros);
            return elemento;
        }

        object IGestor.LeerElementos(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros, List<ClausulaDeOrdenacion> orden, Dictionary<string, object> opcionesDeMapeo)
        {
            return LeerElementos(posicion, cantidad, filtros, orden, opcionesDeMapeo);
        }

        object IGestor.LeerRegistros(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros, List<ClausulaDeOrdenacion> orden, ParametrosDeNegocio parametros)
        {
            return LeerRegistros(posicion, cantidad, filtros, orden, parametros);
        }

        public virtual IEnumerable<TElemento> LeerElementos(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros, List<ClausulaDeOrdenacion> orden, Dictionary<string, object> parametros)
        {
            if (!parametros.ContainsKey(nameof(ParametrosDeNegocio.Operacion)))
                parametros.Add(nameof(ParametrosDeNegocio.Operacion), enumTipoOperacion.LeerSinBloqueo.ToString());

            var operacion = parametros[nameof(ParametrosDeNegocio.Operacion)].ToTipoOperacion();
            var aplicarJoin = parametros.ContainsKey(nameof(ParametrosDeNegocio.AplicarJoin)) ? (bool)parametros[nameof(ParametrosDeNegocio.AplicarJoin)] : true;

            var p = new ParametrosDeNegocio(operacion);


            p.Parametros = parametros;
            p.AplicarJoin = aplicarJoin;

            if (parametros.ContainsKey(nameof(ltrParametrosNeg.Peticion)))
                p.Peticion = (enumPeticion)parametros[nameof(ltrParametrosNeg.Peticion)];

            List<TRegistro> registros = LeerRegistros(posicion, cantidad, filtros, orden, p);

            return MapearElementos(registros, p);
        }

        public List<TElemento> ProyectarElementos(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros, List<ClausulaDeOrdenacion> orden, ParametrosDeNegocio parametros = null)
        {
            IQueryable<TRegistro> consulta = DefinirConsulta(posicion, cantidad, filtros, orden, parametros);

            return Mapeador.ProjectTo<TElemento>(consulta).AsNoTracking().ToList();
        }


        public static TRegistro LeerRegistroPorId(ContextoSe contexto, int id, bool aplicarJoin = false, bool usarLaCache = false, enumNegocio negocio = enumNegocio.No_Definido)
        {
            var iGestor = ApiDeInterfaceDtm.ImplementaUnEstado(typeof(TRegistro)) || ApiDeInterfaceDtm.ImplementaUnaTransicion(typeof(TRegistro))
                ? NegociosDeSe.CrearGestor(contexto, typeof(TRegistro), typeof(TElemento), negocio)
                : NegociosDeSe.CrearGestor(contexto, typeof(TRegistro), typeof(TElemento));
            var registro = (TRegistro)iGestor.LeerRegistroPorId(id, aplicarJoin, usarLaCache);
            return registro;
        }

        public static TRegistro EliminarRegistroPorId(ContextoSe contexto, int id, enumNegocio negocio = enumNegocio.No_Definido, Dictionary<string, object> parametros = null)
        {
            var iGestor = ApiDeInterfaceDtm.ImplementaUnEstado(typeof(TRegistro)) || ApiDeInterfaceDtm.ImplementaUnaTransicion(typeof(TRegistro))
                ? NegociosDeSe.CrearGestor(contexto, typeof(TRegistro), typeof(TElemento), negocio)
                : NegociosDeSe.CrearGestor(contexto, typeof(TRegistro), typeof(TElemento));
            var registro = (TRegistro)iGestor.EliminarRegistroPorId(id, parametros);
            return registro;
        }

        object IGestor.EliminarRegistroPorId(int id, Dictionary<string, object> parametros)
        {
            return PersistirRegistro(LeerRegistroPorId(id, true, false, false, false), new ParametrosDeNegocio(enumTipoOperacion.Eliminar, parametros));
        }

        object IGestor.LeerRegistroPorId(int id, bool aplicarJoin, bool usarLaCache, Dictionary<string, object> parametros)
        {
            return LeerRegistroPorId(id, usarLaCache, traqueado: false, conBloqueo: false, aplicarJoin, parametros);
        }

        public virtual TRegistro LeerRegistroPorId(int id, bool usarLaCache, bool traqueado, bool conBloqueo, bool aplicarJoin, Dictionary<string, object> parametros = null)
        =>
        LeerRegistroPorId(id, usarLaCache, traqueado, conBloqueo, aplicarJoin, errorSiNoHay: (bool)parametros.LeerValor(nameof(ltrParametrosNeg.ErrorSiNoLoHay), true), parametros);

        public virtual TRegistro LeerRegistroPorId(int id, bool usarLaCache, bool traqueado, bool conBloqueo, bool aplicarJoin, bool errorSiNoHay, Dictionary<string, object> parametros)
        {
            if (!usarLaCache)
                VaciarCacheDeRegistroPorId(id);

            try
            {
                if (parametros == null) parametros = new Dictionary<string, object>();
                parametros[ltrParametrosNeg.FiltroPorId] = true;
                var resultado = LeerRegistroCacheado(nameof(IRegistro.Id), id.ToString(), errorSiNoHay: errorSiNoHay, errorSiHayMasDeUno: true, aplicarJoin, parametros);
                return resultado;
            }
            catch (Exception ex)
            {
                if (errorSiNoHay && ex.Message.Contains("No se ha localizado el registro solicitado para el valor") && typeof(TRegistro).ImplementaElementoDeUnProceso())
                {
                    if (parametros == null)
                        parametros = new Dictionary<string, object>();
                    else
                    if (parametros.ContainsKey(ltrParametrosNeg.ValidarPermisosDeConsulta) && !(bool)parametros[ltrParametrosNeg.ValidarPermisosDeConsulta])
                        throw;

                    parametros[ltrParametrosNeg.ValidarPermisosDeConsulta] = false;

                    var r = LeerRegistroCacheado(nameof(IRegistro.Id), id.ToString(), errorSiNoHay: false, errorSiHayMasDeUno: true, true, parametros);
                    if (r != null)
                    {
                        if (r.GetType().ImplementaElementoConTipo())
                        {
                            if (ApiDePermisos.ModoDeAccesoAlTipo(Contexto, Negocio, r) == enumModoDeAccesoDeDatos.SinPermiso)
                                throw new Exception($"El usuario '{Contexto.DatosDeConexion.Login}' no tiene permiso para el tipo '{r.Valor<TipoDeElementoDtm>(x => x.Name == nameof(IUsaTipo.Tipo)).Nombre}' del negocio '{Negocio.Singular(true)}'", ex);
                        }

                        if (r.GetType().ImplementaUsaCg())
                        {
                            var filtros = new Dictionary<string, object> { { nameof(NegociosDeUnCgDtm.IdCg), ((IElementoDeProcesoDtm)r).IdCg }, { nameof(NegociosDeUnCgDtm.IdNegocio), Negocio.IdNegocio() } };
                            var permisos = Contexto.SeleccionarTodos<NegociosDeUnCgDtm>(filtros);
                            if (Contexto.TieneElPermiso(permisos[0].IdConsultor))
                                throw new Exception($"El usuario '{Contexto.DatosDeConexion.Login}' no tiene permiso para el CG '{((IElementoDeProcesoDtm)r).Cg.Expresion}' y el negocio {Negocio}'", ex);
                        }

                        throw;
                    }
                }
                Contexto.AnotarExcepcion(ex);
                throw;
            }
        }


        public TRegistro LeerRegistroCacheadoPoAk(List<ClausulaDeFiltrado> filtros, bool aplicarJoin, bool errorSiNoHay = true, bool errorSiHayMasDeUno = true)
        {
            var cache = ServicioDeCaches.Obtener(ServicioDeCaches.PorAk(typeof(TRegistro).FullName));

            string indice = "";
            foreach (var filtro in filtros)
            {
                indice = $"{indice}-{filtro.Clausula}-{filtro.Criterio}-{filtro.Valor}";
            }

            indice = $"{indice}-{(!aplicarJoin ? $"{ServicioDeCaches.enumConJoin.no}" : $"{ServicioDeCaches.enumConJoin.si}")}";

            if (!cache.ContainsKey(indice))
            {
                var registros = LeerRegistros(0, -1, filtros, null, new ParametrosDeNegocio(enumTipoOperacion.LeerSinBloqueo, aplicarJoin));

                if (errorSiNoHay && registros.Count == 0)
                    GestorDeErrores.Emitir($"No se ha localizado el registro solicitada para el filtro proporcionado");

                if (errorSiHayMasDeUno && registros.Count > 1)
                    GestorDeErrores.Emitir($"Hay más de un registro para el filtro proporcionado");

                if (registros.Count == 0)
                    return null;

                if (registros.Count > 1)
                    return registros[0];

                cache[indice] = registros[0];
            }
            return (TRegistro)cache[indice];
        }

        public TRegistro LeerRegistroCacheado(string propiedad, string valor, bool errorSiNoHay, bool errorSiHayMasDeUno, bool aplicarJoin, Dictionary<string, object> parametros = null)
        {

            var cache = ServicioDeCaches.ObtenerCache(typeof(TRegistro).FullName, propiedad);
            var indice = $"{propiedad}-{valor}";
            if (!aplicarJoin)
            {
                var indiceSi = $"{indice}-{ServicioDeCaches.enumConJoin.si}";
                indice = !cache.ContainsKey(indiceSi) ? $"{indice}-{ServicioDeCaches.enumConJoin.no}" : $"{propiedad}-{valor}-{ServicioDeCaches.enumConJoin.si}";
            }
            else
                indice = $"{propiedad}-{valor}-{ServicioDeCaches.enumConJoin.si}";

            if (!cache.ContainsKey(indice))
            {
                var a = LeerRegistro(propiedad, valor, errorSiNoHay, errorSiHayMasDeUno, conBloqueo: false, aplicarJoin, parametros);
                if (a == null)
                    return null;

                cache[indice] = a;
            }
            return (TRegistro)cache[indice];
        }


        public object LeerRegistro(string propiedad, string valor, bool errorSiNoHay = true, bool errorSiHayMasDeUno = true, bool aplicarJoin = true)
        {
            return LeerRegistro(propiedad, valor, errorSiNoHay, errorSiHayMasDeUno, conBloqueo: false, aplicarJoin);
        }

        public virtual TRegistro LeerRegistro(string propiedad, string valor, bool errorSiNoHay, bool errorSiHayMasDeUno, bool conBloqueo, bool aplicarJoin, Dictionary<string, object> parametros = null)
        {
            List<TRegistro> registros = LeerRegistroInterno(propiedad, valor, conBloqueo, aplicarJoin, parametros);

            if (errorSiNoHay && registros.Count == 0)
                GestorDeErrores.Emitir($"No se ha localizado el registro solicitado para el valor '{valor}' en la clase '{typeof(TRegistro).Name}'");

            if (errorSiHayMasDeUno && registros.Count > 1)
                GestorDeErrores.Emitir($"Hay más de un registro para el valor {valor} en la clase {typeof(TRegistro).Name}");

            if (registros.Count == 0)
                return null;

            if (registros.Count > 1)
                return registros[0];

            return registros[0];
        }

        public TRegistro LeerRegistro(List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros, bool errorSiNoHay, bool errorSiHayMasDeUno)
        {
            List<TRegistro> registros = LeerRegistros(0, -1, filtros, null, parametros);

            if (errorSiNoHay && registros.Count == 0)
                GestorDeErrores.Emitir($"No se ha localizado el registro solicitada para los filtros indicados en la clase {typeof(TRegistro).Name}");

            if (errorSiHayMasDeUno && registros.Count > 1)
                GestorDeErrores.Emitir($"Hay más de un registro para los filtros indicados en la clase {typeof(TRegistro).Name}");

            if (registros.Count == 0)
                return null;

            if (registros.Count > 1)
                return registros[0];

            return registros[0];
        }

        private List<TRegistro> LeerRegistroInterno(string propiedad, string valor, bool ConBloqueo, bool aplicarJoin, Dictionary<string, object> parametros)
        {
            var filtro = new ClausulaDeFiltrado()
            {
                Criterio = enumCriteriosDeFiltrado.igual,
                Clausula = propiedad,
                Valor = valor
            };
            var filtros = new List<ClausulaDeFiltrado>() { filtro };
            var orden = new List<ClausulaDeOrdenacion> { new ClausulaDeOrdenacion { OrdenarPor = propiedad, Modo = ModoDeOrdenancion.ascendente } };

            var parametrosDeNegocio = new ParametrosDeNegocio(ConBloqueo ? enumTipoOperacion.LeerConBloqueo : enumTipoOperacion.LeerSinBloqueo, aplicarJoin);
            parametrosDeNegocio.IncluirBajas = true;
            if (parametros != null)

            {
                foreach (var parametro in parametros)
                {
                    parametrosDeNegocio.Parametros[parametro.Key] = parametro.Value;
                }
            }

            return LeerRegistros(0, -1, filtros, orden, parametrosDeNegocio);
        }

        public List<TRegistro> LeerRegistros(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros)
        {
            return LeerRegistros(posicion, cantidad, filtros, false);
        }

        public List<TRegistro> LeerRegistros(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros, bool aplicarJoin, Dictionary<string, object> parametros = null)
        {
            List<ClausulaDeOrdenacion> orden = parametros != null && parametros.ContainsKey(ltrParametrosNeg.AplicarElOrden)
                ? (List<ClausulaDeOrdenacion>)parametros[ltrParametrosNeg.AplicarElOrden]
                : new List<ClausulaDeOrdenacion>();

            if (orden.Count == 0 && typeof(TRegistro).ImplementaNombre())
                orden.Add(new ClausulaDeOrdenacion() { OrdenarPor = nameof(INombre.Nombre), Modo = ModoDeOrdenancion.ascendente });

            var pn = new ParametrosDeNegocio(enumTipoOperacion.LeerSinBloqueo, aplicarJoin);
            if (parametros != null)
                foreach (var parametro in parametros) pn.Parametros[parametro.Key] = parametro.Value;

            return LeerRegistros(posicion, cantidad, filtros, orden, pn);
        }

        object IGestor.LeerRegistros(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros, bool aplicarJoin, Dictionary<string, object> parametros)
        {
            return LeerRegistros(posicion, cantidad, filtros, aplicarJoin, parametros);
        }

        IEnumerable<RegistroDtm> IGestor.Registros(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros, bool aplicarJoin, Dictionary<string, object> parametros)
        {
            return LeerRegistros(posicion, cantidad, filtros, aplicarJoin, parametros);
        }

        int IGestor.Contar(List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            return Contar(filtros, parametros);
        }

        bool IGestor.Existen(List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            return Existen(filtros, parametros);
        }


        object IGestor.PersistirElementoDto(object elementoDto, ParametrosDeNegocio parametros)
        {
            return PersistirElementoDto((TElemento)elementoDto, parametros);
        }
        object IGestor.PersistirRegistro(object registro, ParametrosDeNegocio parametrosDeNegocio)
        {
            return PersistirRegistro((TRegistro)registro, parametrosDeNegocio);
        }


        public virtual List<TRegistro> LeerRegistros(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros = null, List<ClausulaDeOrdenacion> orden = null, ParametrosDeNegocio parametros = null)
        {
            if (parametros == null)
                parametros = new ParametrosDeNegocio(enumTipoOperacion.LeerSinBloqueo);

            if (parametros.Parametros.ContainsKey(ltrParametrosNeg.ExcluirCancelados) && filtros.Count(x => x.Clausula == ltrParametrosNeg.ExcluirCancelados) == 0)
                filtros.Add(new ClausulaDeFiltrado(ltrParametrosNeg.ExcluirCancelados, enumCriteriosDeFiltrado.igual, parametros.Parametros[ltrParametrosNeg.ExcluirCancelados]));

            if (parametros.Parametros.ContainsKey(ltrParametrosNeg.ExcluirTerminados) && filtros.Count(x => x.Clausula == ltrParametrosNeg.ExcluirTerminados) == 0)
                filtros.Add(new ClausulaDeFiltrado(ltrParametrosNeg.ExcluirTerminados, enumCriteriosDeFiltrado.igual, parametros.Parametros[ltrParametrosNeg.ExcluirTerminados]));

            IQueryable<TRegistro> consulta = DefinirConsulta(posicion, cantidad, filtros, orden, parametros);
            var traza = parametros.Parametros.LeerValor(ltrParametrosNeg.NombreDelFicheroParaTrazarLaConsulta, "");
            var trazaActiva = Contexto.Traza;
            if (!traza.IsNullOrEmpty()) Contexto.IniciarTraza(traza, debugar: true);
            List<TRegistro> leidos;
            try
            {
                leidos = LanzarConsulta(consulta, parametros);
            }
            finally
            {
                if (!traza.IsNullOrEmpty())
                {
                    Contexto.CerrarTraza();
                    Contexto.Debuggar = CacheDeVariable.Cfg_HayQueDebuggar;
                }
                Contexto.Traza = trazaActiva;
            }

            DespuesDeLeer(leidos, parametros);
            return leidos;
        }

        private List<TRegistro> LanzarConsulta(IQueryable<TRegistro> consulta, ParametrosDeNegocio parametros)
        {
            if (!Contexto.HayTransaccion && parametros.Operacion == enumTipoOperacion.LeerSinBloqueo)
            {
                var tran = Contexto.IniciarTransaccion(IsolationLevel.ReadUncommitted);
                try
                {
                    Contexto.Commit(tran);
                }
                catch
                {
                    Contexto.Rollback(tran);
                    throw;
                }
            }
            return consulta.ToList();
        }

        protected virtual void DespuesDeLeer(List<TRegistro> registros, ParametrosDeNegocio parametros)
        {

        }

        public TRegistro LeerUltimoRegistro(List<ClausulaDeFiltrado> filtros = null, ParametrosDeNegocio parametros = null)
        {
            if (parametros == null)
                parametros = new ParametrosDeNegocio(enumTipoOperacion.LeerSinBloqueo);

            var orden = new ClausulaDeOrdenacion() { OrdenarPor = nameof(IRegistro.Id), Modo = ModoDeOrdenancion.descendente };

            var consulta = DefinirConsulta(0, 1, filtros, new List<ClausulaDeOrdenacion> { orden }, parametros);

            var registro = parametros.Traquear ? consulta.FirstOrDefault() : consulta.AsNoTracking().FirstOrDefault();

            return registro;
        }

        private IQueryable<TRegistro> DefinirConsulta(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros, List<ClausulaDeOrdenacion> orden, ParametrosDeNegocio parametros)
        {

            if (!parametros.Parametros.ContainsKey(ltrParametrosNeg.AccionQueSeEjecuta) && Contexto.Accion is not null)
            {
                parametros.Parametros[ltrParametrosNeg.AccionQueSeEjecuta] = Contexto.Accion.Nombre;
            }

            if (filtros == null)
                filtros = new List<ClausulaDeFiltrado>();

            parametros.Filtros = filtros;

            IQueryable<TRegistro> consulta = Contexto.Set<TRegistro>();


            if (filtros.Any(x => x.Clausula.ToLower().Equals(nameof(IRegistro.Id).ToLower()) && x.Criterio.Equals(enumCriteriosDeFiltrado.igual)))
                parametros.FiltroPorId = true;

            if (parametros.AplicarJoin)
                consulta = AplicarJoins(consulta, filtros, parametros);

            PrepararFiltroPorBaja(filtros, parametros);
            PrepararFiltroPorEstado(filtros, parametros);

            var filtroPoNombreDeArchivo = filtros.FirstOrDefault(x => x.Clausula == ltrFiltros.NombreDeArchivo);
            if (filtroPoNombreDeArchivo is not null)
            {
                if (Negocio.UsaArchivadores())
                {
                    consulta = consulta.Archivos(Contexto, Negocio, filtroPoNombreDeArchivo);
                }
                else
                {
                    // Separamos los nombres por ';' y quitamos espacios o entradas vacías
                    var nombresBuscados = filtroPoNombreDeArchivo.Valor.Split(Simbolos.separadorDeCadenasDeFiltrado, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();

                    // Filtramos los vínculos basándonos en si el nombre del archivo coincide con cualquiera de los términos
                    var archivos = Negocio.Archivos(Contexto).Where(vin =>
                        Contexto.Set<ArchivoDtm>().Any(a =>
                            a.Id == vin.idElemento2 &&
                            nombresBuscados.Any(nb => a.Nombre.Contains(nb))
                        )
                    );

                    consulta = consulta.Where(x => archivos.Any(a => a.idElemento1 == x.Id));
                    filtroPoNombreDeArchivo.Aplicado = true;
                }
            }

            if (filtros.Count > 0)
            {
                foreach (ClausulaDeFiltrado filtro in filtros.Where(x => x.Aplicado)) filtro.Aplicado = false;
                if (filtros.Any(f => f.Clausula.ToLower() == nameof(IUsaReferencia.Referencia).ToLower() && f.Criterio == enumCriteriosDeFiltrado.contiene && f.Valor.StartsWith(Simbolos.Igual)))
                {
                    var filtro = filtros.FirstOrDefault(f => f.Clausula.ToLower() == ltrParametrosNeg.QueMostrar.ToLower() &&
                                 f.Criterio == enumCriteriosDeFiltrado.diferente &&
                                 f.Valor == $"{ltrParametrosNeg.Cancelados}{Simbolos.separadorDeRangos}{ltrParametrosNeg.Terminados}");
                    if (filtro is not null)
                    {
                        filtro.Aplicado = true;
                    }
                }
                consulta = AplicarFiltros(consulta, filtros, parametros);
                consulta = consulta.FiltrarPorExpresion(filtros);
                consulta = consulta.AplicarFiltroPorPropiedades(filtros);

                foreach (ClausulaDeFiltrado filtro in filtros.Where(x => !x.Aplicado))
                    consulta = consulta.FiltrarPorEstado(Contexto, Negocio, filtro, parametros.AplicarFiltroQueMostrar);
            }

            if (parametros.ValidarPermisosDeConsulta)
                consulta = AplicarSeguridad(consulta, filtros, parametros);

            if (orden == null)
                orden = new List<ClausulaDeOrdenacion>();


            DefinirOrden(orden, parametros);

            consulta = AplicarOrden(consulta, orden);

            consulta = consulta.Skip(posicion);

            if (cantidad > 0)
            {
                consulta = consulta.Take(cantidad);
            }

            return consulta;
        }

        protected virtual void DefinirOrden(List<ClausulaDeOrdenacion> orden, ParametrosDeNegocio parametros)
        {

            if (parametros.CargarListaDinamica && typeof(TRegistro).ImplementaElementoConTipo())
            {
                orden.Insert(0, new ClausulaDeOrdenacion(nameof(IAuditoria.FechaCreacion), ModoDeOrdenancion.descendente));
            }
            else if (orden.Count == 0)
            {
                if (Negocio.UsaReferencia() && typeof(TRegistro).ImplementaUsaTipoConCg())
                {
                    orden.Add(new ClausulaDeOrdenacion(nameof(IUsaReferencia.Referencia), ModoDeOrdenancion.ascendente));
                }
                else if (Negocio == enumNegocio.Usuario)
                {
                    orden.Add(new ClausulaDeOrdenacion(nameof(UsuarioDtm.Login), ModoDeOrdenancion.ascendente));
                }
                else if (typeof(TRegistro).ImplementaNombre())
                {
                    orden.Add(new ClausulaDeOrdenacion(nameof(INombre.Nombre), ModoDeOrdenancion.ascendente));
                }
                else if (typeof(TRegistro).PropiedadesDelTipo().Any(p => p.Name == ltrPropiedades.Orden))
                {
                    orden.Add(new ClausulaDeOrdenacion(nameof(ltrPropiedades.Orden), ModoDeOrdenancion.ascendente));
                }
                else
                    orden.Add(new ClausulaDeOrdenacion(nameof(RegistroDtm.Id), ModoDeOrdenancion.ascendente));
            }

        }

        private void PrepararFiltroPorBaja(List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            if (!ApiDeInterfaceDtm.ImplementaUsaBaja(typeof(TRegistro)))
            {
                if (ApiDeInterfaceDtm.ImplementaUsaEstado(typeof(TRegistro)) && filtros.Any(x => x.Clausula.Equals(ltrParametrosNeg.SoloEnAlta, StringComparison.CurrentCultureIgnoreCase)))
                {

                    if (filtros.Where(x =>
                         x.Clausula.Equals(nameof(IUsaEstado.IdEstado), StringComparison.CurrentCultureIgnoreCase)
                      || x.Clausula.Equals(ltrParametrosNeg.QueMostrar, StringComparison.CurrentCultureIgnoreCase)
                      || x.Clausula.Equals(ltrParametrosNeg.ExcluirTerminados, StringComparison.CurrentCultureIgnoreCase)
                      || x.Clausula.Equals(ltrParametrosNeg.ExcluirCancelados, StringComparison.CurrentCultureIgnoreCase)).Count() > 0)
                        return;
                    filtros.Add(new ClausulaDeFiltrado(ltrParametrosNeg.ExcluirCancelados, enumCriteriosDeFiltrado.igual, true));
                }

                if (filtros.Where(x => x.Clausula.Equals(ltrParametrosNeg.IncluirBajas, StringComparison.CurrentCultureIgnoreCase)
                  || x.Clausula.Equals(ltrParametrosNeg.FiltrarPorBaja, StringComparison.CurrentCultureIgnoreCase)
                  || x.Clausula.Equals(ltrParametrosNeg.SoloEnAlta, StringComparison.CurrentCultureIgnoreCase)).Count() > 0)
                    return;

                if (parametros.Peticion == enumPeticion.epLeerDatosParaElGrid || parametros.EstoyExportando)
                    filtros.Add(new ClausulaDeFiltrado(ltrParametrosNeg.IncluirBajas, enumCriteriosDeFiltrado.igual, false.ToString()));
            }
            else
            {
                if ((parametros.Peticion == enumPeticion.epLeerDatosParaElGrid || parametros.EstoyExportando) && filtros.Count == 0)
                    filtros.Add(new ClausulaDeFiltrado(ltrParametrosNeg.IncluirBajas, enumCriteriosDeFiltrado.igual, false.ToString()));
            }
        }

        private void PrepararFiltroPorEstado(List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            if (!ApiDeInterfaceDtm.ImplementaUsaEstado(typeof(TRegistro)))
                return;

            if (filtros.Where(x =>
                 x.Clausula.Equals(nameof(IUsaEstado.IdEstado), StringComparison.CurrentCultureIgnoreCase)
              || x.Clausula.Equals(ltrParametrosNeg.QueMostrar, StringComparison.CurrentCultureIgnoreCase)).Count() > 0)
                return;

            if (parametros.ExcluirCancelados.HasValue && parametros.ExcluirTerminados.HasValue)
            {
                filtros.Add(new ClausulaDeFiltrado(ltrParametrosNeg.ExcluirCancelados, enumCriteriosDeFiltrado.igual, parametros.ExcluirCancelados));
                filtros.Add(new ClausulaDeFiltrado(ltrParametrosNeg.ExcluirTerminados, enumCriteriosDeFiltrado.igual, parametros.ExcluirTerminados));
            }
            else
            if (parametros.Peticion == enumPeticion.epLeerDatosParaElGrid || parametros.EstoyExportando || parametros.EstoyTotalizando)
                filtros.Add(new ClausulaDeFiltrado(ltrParametrosNeg.QueMostrar, enumCriteriosDeFiltrado.diferente, $"{ltrParametrosNeg.Cancelados};{ltrParametrosNeg.Terminados}"));
        }

        protected virtual IQueryable<TRegistro> AplicarSeguridad(IQueryable<TRegistro> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            return consulta;
        }

        /// <summary>
        /// se indican que joins se han de montar cuando se defina la consulta en función de los filtros y los parámetros de negocio
        /// </summary>
        /// <param name="filtros">filtros que se van a aplicar</param>
        /// <param name="joins">join a incluir</param>
        /// <param name="parametros">parámetros de negocio que modifican el comportamiento</param>
        protected virtual IQueryable<TRegistro> AplicarOrden(IQueryable<TRegistro> consulta, List<ClausulaDeOrdenacion> ordenacion)
        {
            return consulta.AplicarOrdenesBasicos(Contexto, ordenacion);
        }

        protected static IQueryable<TRegistro> OrdenPorId(IQueryable<TRegistro> consulta, ClausulaDeOrdenacion orden)
        {
            return orden.Modo == ModoDeOrdenancion.ascendente
                ? consulta.OrderBy(x => x.Id)
                : consulta.OrderByDescending(x => x.Id);
        }


        protected virtual IQueryable<TRegistro> AplicarFiltros(IQueryable<TRegistro> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            var filtroEstado = filtros.FirstOrDefault(x => x.Clausula.Equals(ltrParametrosNeg.QueMostrar, StringComparison.InvariantCultureIgnoreCase));
            if (filtroEstado != null && (
                   !parametros.AplicarFiltroQueMostrar ||
                   parametros.ExcluirCancelados != null ||
                   parametros.ExcluirTerminados != null ||
                   filtros.Any(f => f.Clausula.Equals(nameof(IRegistro.Id), StringComparison.InvariantCultureIgnoreCase))
                  )
                )
                filtroEstado.Aplicado = true;

            (EstadoAuditado estado, TransicionAuditada transicion) hito = (null, null);
            foreach (ClausulaDeFiltrado filtro in filtros.Where(x => x.Aplicado == false))
            {
                if (filtro.Clausula.ToLower() == nameof(IRegistro.Id).ToLower() && filtro.Criterio == enumCriteriosDeFiltrado.igual)
                {
                    consulta = consulta.Where(x => x.Id == filtro.Valor.Entero());
                    parametros.FiltroPorId = true;
                    for (var i = 0; i < filtros.Count; i++) filtros[i].Aplicado = true;
                    return consulta;
                }

                if (parametros.FiltroPorId)
                    continue;

                if (filtro.Clausula.ToLower() == nameof(INombre.Nombre).ToLower() && filtro.Criterio == enumCriteriosDeFiltrado.igual)
                {
                    if (typeof(TRegistro).ImplementaUsaReferencia())
                        consulta = consulta.Where(x => ((INombre)x).Nombre == filtro.Valor || ((IUsaReferencia)x).Referencia == filtro.Valor);
                    else
                        consulta = consulta.Where(x => ((INombre)x).Nombre == filtro.Valor);
                    filtro.Aplicado = true;
                    parametros.FiltroPorNombreIgual = true;
                }

                if (filtro.Clausula.ToLower() == nameof(INombre.Nombre).ToLower() && typeof(TRegistro).ImplementaUsaReferencia())
                {
                    filtro.Criterio = enumCriteriosDeFiltrado.porReferencia;
                }

                hito = ExtensorDeElementos.PrepararFiltrosPorHitos<TRegistro>(filtro, hito);

                consulta = consulta.FiltrarPorCg(Contexto, filtro);

                consulta = consulta.FiltrarPorTipo(Contexto, Negocio, filtro, parametros);

                //consulta = consulta.FiltrarPorEstado(Contexto, Negocio, filtro, parametros.AplicarFiltroQueMostrar);

            }

            consulta = consulta.FiltrosPorEtapas<TRegistro>(Negocio, filtros);
            consulta = consulta.FiltrosPorArchivadores(Contexto, filtros);
            consulta = consulta.FiltrosPorTareas(Contexto, filtros);
            consulta = consulta.FiltrosPorHitos(Contexto, Negocio, hito, parametros);
            consulta = consulta.FiltrosPorHitos(Contexto, Negocio, filtros, parametros);
            consulta = consulta.FiltrarPorBaja(filtros, parametros);

            if (Negocio.UsaObservaciones())
            {
                var filtroObs = filtros.FirstOrDefault(x => x.Clausula == ltrFiltros.Observacion);
                if (filtroObs != null)
                {
                    // 1. Creamos la lista de términos (limpiando vacíos y espacios)
                    var terminosObservacion = filtroObs.Valor.Split(Simbolos.separadorDeCadenasDeFiltrado, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();

                    // 2. Filtramos usando .Any() sobre la lista de términos
                    // Esto se traducirá a SQL como: (o.Nombre LIKE %term1% OR o.Nombre LIKE %term2%...)
                    consulta = consulta.Where(x =>
                        Negocio.Observaciones(Contexto).Any(o =>
                            o.IdElemento == x.Id &&
                            terminosObservacion.Any(term => o.Nombre.Contains(term) || o.Descripcion.Contains(term))
                        )
                    );

                    // Marcamos como aplicado si es necesario para tu lógica de filtros
                    filtroObs.Aplicado = true;
                }
            }

            if (Negocio.UsaArchivos())
            {
                var filtro = filtros.FirstOrDefault(x => x.Clausula.Equals(ltrFiltros.SeleccionarDestino) && !x.Aplicado);
                if (filtro != null)
                {
                    filtro.Clausula = nameof(ElementoDtm.Expresion);
                    var filtroPorIdDiferente = filtros.First(x => x.Clausula.Equals(ltrFiltros.IdOrigenDiferente, StringComparison.InvariantCultureIgnoreCase));
                    filtroPorIdDiferente.Criterio = enumCriteriosDeFiltrado.diferente;
                    consulta = consulta.AplicarFiltroPorEntero(filtroPorIdDiferente, nameof(ElementoDtm.Id));
                }
            }

            if (Negocio.UsaDirecciones())
            {
                consulta = FiltrosPorDireccion.FiltroPorDireccion<TRegistro>(consulta, Contexto, Negocio, filtros);
            }

            if (typeof(TRegistro).ImplementaUsaBloqueo())
            {
                var filtro = filtros.FirstOrDefault(filtro => filtro.Clausula.ToLower() == ltrParametrosNeg.FiltrarPorBloqueo.ToLower());
                if (filtro is null)
                {
                    consulta = consulta.Where(x => !((IUsaBloqueo)x).Bloqueado);
                }
                else
                {
                    if (filtro.Valor.Entero() == ltrParametrosNeg.MostrarBloqueados)
                        consulta = consulta.Where(x => ((IUsaBloqueo)x).Bloqueado);
                    filtro.Aplicado = true;
                }
            }

            return consulta;
        }

        protected virtual IQueryable<TRegistro> AplicarJoins(IQueryable<TRegistro> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            if (ApiDeInterfaceDtm.ImplementaUnElemento(typeof(TRegistro)) && parametros.HacerJoinCon(ltrParametrosNeg.JoinConUsuarioDtm))
            {
                consulta = consulta.Include(e => ((IElementoDtm)e).UsuarioCreador);
                consulta = consulta.Include(e => ((IElementoDtm)e).UsuarioModificador);
            }

            if (ApiDeInterfaceDtm.ImplementaUsaArchivo(typeof(TRegistro)))
                consulta = consulta.Include(p => ((IUsaArchivo)p).Archivo);

            if (ApiDeInterfaceDtm.ImplementaElementoConTipo(typeof(TRegistro)))
                consulta = consulta.Include(p => ((IUsaTipo)p).Tipo);

            if (ApiDeInterfaceDtm.ImplementaUsaCg(typeof(TRegistro)))
                consulta = consulta.Include(p => ((IUsaCg)p).Cg);

            if (ApiDeInterfaceDtm.ImplementaUsaEstado(typeof(TRegistro)))
                consulta = consulta.Include(p => ((IUsaEstado)p).Estado);

            return consulta;
        }


        #endregion

        #region Métodos de acceso a BD
        public bool ExisteObjetoEnBd(int id)
        {
            return Contexto.Set<TRegistro>().AsNoTracking().Any(e => e.Id == id);
        }

        public virtual bool Existen(List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros = null)
        {
            if (parametros == null)
                parametros = new ParametrosDeNegocio(enumTipoOperacion.Contar);

            var registros = DefinirConsulta(0, 1, filtros, null, parametros);
            return registros.Count() > 0;
        }

        public virtual int Contar(List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros = null)
        {

            if (parametros == null)
                parametros = new ParametrosDeNegocio(enumTipoOperacion.Contar);

            var registros = DefinirConsulta(0, -1, filtros, null, parametros);
            var total = registros.Count();

            CacheDeRecuentos[typeof(TRegistro).FullName] = false;

            return total;
        }

        public int Recontar(List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros = null)
        {
            if (!CacheDeRecuentos.ContainsKey(typeof(TRegistro).FullName) || CacheDeRecuentos[typeof(TRegistro).FullName])
            {
                return Contar(filtros, parametros);
            }

            return 0;
        }

        #endregion

        #region Métodos de mapeo

        public List<TRegistro> MapearRegistros(List<TElemento> elementos, ParametrosDeNegocio opciones)
        {
            var registros = new List<TRegistro>();
            foreach (var elemento in elementos)
            {
                var registro = MapearRegistro(elemento, opciones);
                registros.Add(registro);
            }
            return registros;
        }

        public virtual TRegistro MapearRegistro(TElemento elemento, ParametrosDeNegocio opciones)
        {
            var registro = Mapeador.Map<TElemento, TRegistro>(elemento,
                   opt =>
                   {
                       opt.BeforeMap((src, dest) => AntesMapearRegistro(elemento, opciones));
                       opt.AfterMap((src, dest) => DespuesDeMapearElRegistro(elemento, dest, opciones));
                   }
                );

            return registro;
        }

        protected virtual void DespuesDeMapearElRegistro(TElemento elemento, TRegistro registro, ParametrosDeNegocio opciones)
        {
            if (enumTipoOperacion.Insertar == opciones.Operacion)
                registro.Id = 0;

            if (registro.ImplementaUnElemento() && opciones.Operacion == enumTipoOperacion.MapearElDtoAlDtm)
            {
                ((IElementoDtm)registro).FechaCreacion = ((IAuditadoDto)elemento).CreadoEl;
                ((IElementoDtm)registro).FechaModificacion = ((IAuditadoDto)elemento).ModificadoEl;
                ((IElementoDtm)registro).IdUsuaCrea = ((IAuditadoDto)elemento).IdCreador;
                ((IElementoDtm)registro).IdUsuaModi = ((IAuditadoDto)elemento).IdModificador;
            }

            var propiedades = typeof(TRegistro).PropiedadesDelTipo();
            foreach (var propiedad in propiedades)
            {
                var tipoNulable = Nullable.GetUnderlyingType(propiedad.PropertyType);
                if (tipoNulable != null
                    && tipoNulable == typeof(int)
                    && propiedad.Name.StartsWith("id", StringComparison.InvariantCultureIgnoreCase)
                    && registro.LeerPropiedad(propiedad.Name) != null
                    && (int?)registro.LeerPropiedad(propiedad.Name) == 0)
                    registro.EscribirPropiedad(propiedad.Name, null);
            }
        }

        private void AntesMapearRegistro(TElemento elemento, ParametrosDeNegocio opciones)
        {

            if (opciones.Operacion == enumTipoOperacion.Insertar)
                AntesDeMapearElRegistroParaInsertar(elemento, opciones);
            else
            if (opciones.Operacion == enumTipoOperacion.Modificar)
                AntesDeMapearElRegistroParaModificar(elemento, opciones);
            else
            if (opciones.Operacion == enumTipoOperacion.Eliminar)
                AntesDeMapearElRegistroParaEliminar(elemento, opciones);
        }

        protected virtual void AntesDeMapearElRegistroParaEliminar(TElemento elemento, ParametrosDeNegocio opciones)
        {
            if (elemento.Id == 0)
                GestorDeErrores.Emitir($"No puede eliminar un elemento {typeof(TElemento).Name} con id 0");
        }

        protected virtual void AntesDeMapearElRegistroParaModificar(TElemento elemento, ParametrosDeNegocio opciones)
        {
            if (elemento.Id == 0)
                GestorDeErrores.Emitir($"No puede modificar un elemento {typeof(TElemento).Name} con id 0");
        }

        protected virtual void AntesDeMapearElRegistroParaInsertar(TElemento elemento, ParametrosDeNegocio opciones)
        {
            if (elemento.Id > 0)
                GestorDeErrores.Emitir($"No puede crear un elemento {typeof(TElemento).Name} con id {elemento.Id}");
        }

        public IEnumerable<TElemento> MapearElementos(List<TRegistro> registros, ParametrosDeNegocio parametros = null)
        {
            if (parametros == null)
                parametros = new ParametrosDeNegocio(enumTipoOperacion.LeerSinBloqueo);

            var lista = new List<TElemento>();
            try
            {
                DatosParaElMapeo(registros, parametros.Parametros);
                foreach (var registro in registros)
                {
                    var elemento = MapearElemento(registro, parametros);
                    if (elemento != null)
                        lista.Add(elemento);
                }

                lista = OrdenarElementosLeidos(lista, parametros);
                return lista.AsEnumerable();
            }
            finally
            {
                parametros.Parametros.Remove(nameof(DatosParaElMapeo));
            }
        }

        protected virtual void DatosParaElMapeo(List<TRegistro> registros, Dictionary<string, object> parametros)
        {
        }

        protected List<TElemento> OrdenarElementosLeidos(List<TElemento> lista, ParametrosDeNegocio parametros)
        {
            return lista;
        }

        protected virtual void AntesDeMapearElElemento(TRegistro registro, ParametrosDeNegocio parametros)
        {

        }

        public TElemento MapearElemento(TRegistro registro, ParametrosDeNegocio parametros = null)
        {
            if (parametros == null)
                parametros = new ParametrosDeNegocio(enumTipoOperacion.LeerSinBloqueo);

            TElemento elemento = null;
            try
            {
                elemento = Mapeador.Map<TRegistro, TElemento>(registro,
                    opt =>
                    {
                        opt.BeforeMap((registro, elemento) => AntesDeMapearElElemento(registro, parametros));
                        opt.AfterMap((registro, elemento) => DespuesDeMapearElElemento(registro, elemento, parametros));
                    }
                    );
                return elemento;
            }
            finally
            {
                if (parametros.Operacion == enumTipoOperacion.Insertar ||
                    parametros.Operacion == enumTipoOperacion.Modificar ||
                    parametros.Operacion == enumTipoOperacion.Eliminar ||
                    parametros.Operacion == enumTipoOperacion.Transitar
                    )
                    VaciarCacheDeRegistro(registro, parametros);
            }
        }


        protected virtual void DespuesDeMapearElElemento(TRegistro registro, TElemento elemento, ParametrosDeNegocio parametros)
        {
            if (typeof(TElemento).ImplementaAuditoriaDto() && registro.ImplementaAuditoria())
            {
                ((IAuditadoDto)elemento).CreadoEl = ((IAuditoria)registro).FechaCreacion;
                ((IAuditadoDto)elemento).ModificadoEl = ((IAuditoria)registro).FechaModificacion;

                ((IAuditadoDto)elemento).IdCreador = ((IAuditoria)registro).UsuarioCreador == null ? 0 : ((IElementoDtm)registro).UsuarioCreador.Id;
                ((IAuditadoDto)elemento).IdModificador = ((IAuditoria)registro).UsuarioModificador == null ? null : ((IElementoDtm)registro).UsuarioModificador.Id;

                ((IAuditadoDto)elemento).Creador = ((IAuditoria)registro).UsuarioCreador == null
                    ? ((IAuditadoDto)elemento).IdCreador > 0
                        ? Contexto.SeleccionarPorId<UsuarioDtm>(((IAuditadoDto)elemento).IdCreador).Expresion
                        : ""
                    : UsuarioDtm.NombreCompleto(((IAuditoria)registro).UsuarioCreador);
                ((IAuditadoDto)elemento).Modificador = ((IAuditoria)registro).UsuarioModificador == null
                    ? ((IAuditadoDto)elemento).IdModificador.Entero() > 0
                        ? Contexto.SeleccionarPorId<UsuarioDtm>(((IAuditadoDto)elemento).IdModificador.Entero()).Expresion
                        : ""
                    : UsuarioDtm.NombreCompleto(((IAuditoria)registro).UsuarioModificador);
            }

            if (typeof(TElemento).TienenLaPropiedad(nameof(IUsaArchivoDto.Archivo)))
            {
                if (registro.ImplementaUsaArchivo() && ((IUsaArchivo)registro).Archivo != null
                    && (bool)parametros.Parametros.LeerValor(ltrParametrosDto.DescargarGestionDocumental, false))
                {
                    var solicitadoPorLaCola = (bool)parametros.Parametros.LeerValor(ltrParametrosNeg.solicitadoPorLaCola, false);

                    if (!solicitadoPorLaCola)
                        ((IUsaArchivoDto)elemento).Archivo = ApiDeArchivos.SolicitarDescargarArchivo(Negocio, registro.Id, ((IUsaArchivo)registro).Archivo.Id);
                    else
                        ((IUsaArchivoDto)elemento).Archivo = ApiDeArchivos.DescargarUrlDeArchivo(((IUsaArchivo)registro).Archivo.Id
                                   , ((IUsaArchivo)registro).Archivo.Nombre
                                   , ((IUsaArchivo)registro).Archivo.AlmacenadoEn
                                   , solicitadoPorLaCola);
                }
            }

            if (Negocio.UsaTipo() && typeof(TRegistro).ImplementaElementoConTipo())
            {
                var tipo = ((IUsaTipo)registro).Tipo(Contexto);
                elemento.NombreModificable = tipo.NombreModificable;
                parametros.Parametros[nameof(TipoDeElementoDtm)] = tipo;
                ((IUsaClasePorTipoDto)elemento).UsaClasePorTipo = Negocio.UsaClasesDelTipo();
            }
            else
            {
                elemento.NombreModificable = true;
            }

            if (typeof(TRegistro).ImplementaUsaTipoConCg())
            {
                ((IElementoConTipoConCgDto)elemento).IdSociedadDelCg = ((IUsaTipoConCG)registro).Sociedad(Contexto).Id;
                ((IElementoConTipoConCgDto)elemento).Cg = ((IUsaTipoConCG)registro).Cg(Contexto).Expresion;
            }

            if (typeof(TElemento).HeredaDe(typeof(ElementoDeUnProcesoDto)))
            {
                var estado = Negocio.Estado(Contexto, ((IUsaEstado)registro).IdEstado);
                elemento.EstaCancelada = estado.Cancelado;
                elemento.EstaTerminada = estado.Terminado;
                ((IElementoDeUnProcesoDto)elemento).Tipo = ((ITipoDeElementoDtm)parametros.Parametros[nameof(TipoDeElementoDtm)]).Expresion;
                ((IElementoDeUnProcesoDto)elemento).Estado = estado.Nombre;

                if (parametros.LeerDatosParaElGridOParaExportar && parametros.ColumnasDelGrid.Any(item =>
                          item == nameof(IElementoDeUnProcesoDto.TransitadoEl).ToLowerInvariant() ||
                          item == nameof(IElementoDeUnProcesoDto.TransitadoAl).ToLowerInvariant()
                ))
                {
                    var hito = ((IElementoDeProcesoDtm)registro).HitoAnteriorAlActual(Contexto, errorSiNoHay: false);
                    ((IElementoDeUnProcesoDto)elemento).TransitadoEl = hito is null ? null : hito.Fin;
                    ((IElementoDeUnProcesoDto)elemento).TransitadoAl = hito is null ? null : hito.Transicion;
                }

                if (parametros.LeerPorIdParaEditar)
                {
                    var anterior = ((IElementoDeProcesoDtm)registro).HitoAnteriorAlPrimero(Contexto, errorSiNoHay: false);
                    if (anterior is not null)
                    {
                        ((IElementoDeUnProcesoDto)elemento).EstadoAnterior = anterior.Estado;
                        ((IElementoDeUnProcesoDto)elemento).IdEstadoAnterior = anterior.IdEstado;
                        var transicionParaDevolver = ((IElementoDeProcesoDtm)registro).ComoDevolverA(Contexto, anterior.IdEstado, delSistema: false);
                        if (transicionParaDevolver == null)
                        {
                            ((IElementoDeUnProcesoDto)elemento).EstadoAnterior = null;
                            ((IElementoDeUnProcesoDto)elemento).IdEstadoAnterior = null;
                        }
                    }

                    var transicionesAplicables = ((IElementoDeProcesoDtm)registro).TransicionesAplicables(Contexto, delSistema: false, conObservacion: false);
                    var transicionesDeAvance = transicionesAplicables.Where(t => anterior != null ? t.IdEstadoDestino != anterior.Id : true);
                    var porDefecto = transicionesDeAvance.FirstOrDefault(t => t.PorDefecto);
                    if (transicionesDeAvance.Count() > 1 && porDefecto != null)
                    {
                        var i = transicionesDeAvance.ToList().IndexOf(porDefecto);
                        ((IElementoDeUnProcesoDto)elemento).IdTransicionAplicable = transicionesAplicables[i].IdTransicion;
                        ((IElementoDeUnProcesoDto)elemento).TransicionAplicable = transicionesAplicables[i].Transicion;
                        ((IElementoDeUnProcesoDto)elemento).IdEstadoDestino = transicionesAplicables[i].IdEstadoDestino;
                        ((IElementoDeUnProcesoDto)elemento).TransicionesDisponibles = 1;
                    }
                    else
                    {
                        ((IElementoDeUnProcesoDto)elemento).IdTransicionAplicable = transicionesAplicables.Count > 0 ? transicionesAplicables[0].IdTransicion : null;
                        ((IElementoDeUnProcesoDto)elemento).TransicionAplicable = transicionesAplicables.Count > 0 ? transicionesAplicables[0].Transicion : null;
                        ((IElementoDeUnProcesoDto)elemento).IdEstadoDestino = transicionesAplicables.Count > 0 ? transicionesAplicables[0].IdEstadoDestino : null;
                        ((IElementoDeUnProcesoDto)elemento).TransicionesDisponibles = transicionesDeAvance.Count();
                    }

                    if (((IElementoDeUnProcesoDto)elemento).IdTransicionAplicable != null && ((IElementoDeUnProcesoDto)elemento).IdEstadoDestino == ((IElementoDeUnProcesoDto)elemento).IdEstadoAnterior)
                    {
                        ((IElementoDeUnProcesoDto)elemento).IdTransicionAplicable = null;
                        ((IElementoDeUnProcesoDto)elemento).TransicionAplicable = null;
                        ((IElementoDeUnProcesoDto)elemento).IdEstadoDestino = null;
                        ((IElementoDeUnProcesoDto)elemento).TransicionesDisponibles = 0;
                    }
                }

            }

            if (typeof(TElemento).HeredaDe(typeof(ElmentoAuditadoDto)))
            {
                ((IElmentoAuditadoDto)elemento).HayClases = Negocio.HayClasesDelNegocio(Contexto);
                if (!registro.PropiedadesDelObjeto().Any(x => x.Name == nameof(IElmentoAuditadoDto.IdNegocio)))
                    ((IElmentoAuditadoDto)elemento).IdNegocio = Negocio.IdNegocio();

                if (elemento.GetType().ImplementaUsaBloqueo())
                {
                    ((IUsaBloqueoDto)elemento).Bloqueado = false;
                    ((IUsaBloqueoDto)elemento).Bloqueador = ((IUsaBloqueo)registro).Bloqueador(Contexto).Login;
                }
            }

            if (parametros.EnConsulta)
            {
                elemento.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor;
                return;
            }

            if (typeof(TElemento).HeredaDe(typeof(ElementoDto)) && typeof(TRegistro).ImplementaRowVersion())
            {
                elemento.RowVersion = ((IRowVersion)registro).RowVersion;
            }

            ObtenerModoDeAccesoAlElementoQueSeDevuelve(registro, elemento, parametros);
        }

        protected virtual void ObtenerModoDeAccesoAlElementoQueSeDevuelve(TRegistro registro, TElemento elemento, ParametrosDeNegocio parametros)
        {
            if (typeof(TRegistro).ImplementaUsaBaja() && Negocio.UsaBaja() && ((IUsaBaja)registro).Baja)
            {
                var referencia = Negocio.UsaReferencia()
                ? ((IUsaReferencia)registro).Referencia
                : registro.ImplementaUsaSolicitante()
                ? ((IUsaSolicitante)registro).Solicitante(Contexto).Referencia(Contexto)
                : registro.ImplementaNombre()
                ? ((INombre)registro).Nombre
                : registro.Id.ToString();

                elemento.EstaCancelada = true;

                elemento.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor;
                elemento.informacion = $"El {Negocio.Singular(enMinuscula: true)} '{referencia}' está de baja, por tanto no es editable";
                return;
            }

            elemento.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor;

            //el modo de acceso al elemnto no hace falta pra mostrar los datos en la iU
            if (parametros.Peticion == enumPeticion.epLeerDatosParaElGrid)
                return;

            if (typeof(TRegistro).Equals(typeof(AuditoriaDtm)))
            {
                if (Contexto.DatosDeConexion.EsAdministrador)
                    elemento.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor;
                else
                    elemento.ModoDeAcceso = enumModoDeAccesoDeDatos.SinPermiso;
                return;
            }

            if (typeof(TElemento).ImplementaElementoConTipoDto())
            {
                ((IElementoConTipoDto)elemento).EsInterventor = ((IElementoDtm)registro).EsInterventor(Contexto);
                ((IElementoConTipoDto)elemento).EsGestor = ((IElementoDtm)registro).EsGestor(Contexto);
            }

            if (typeof(TRegistro).ImplementaAmpliacion())
            {
                elemento.ModoDeAcceso = ApiDePermisos.LeerModoDeAcceso(Contexto, ((IAmpliacion)registro).Negocio, ((IAmpliacion)registro).IdElemento);
                return;
            }

            if (typeof(TRegistro).ImplementaDetalle())
            {
                elemento.ModoDeAcceso = ApiDePermisos.LeerModoDeAcceso(Contexto, ((IDetalle)registro).Negocio, ((IDetalle)registro).IdElemento);
                return;
            }

            if (NegociosDeSe.EsDeParametrizacion(typeof(TRegistro)) && Contexto.DatosDeConexion.EsAdministrador && Contexto.SePuedeParametrizar())
            {
                elemento.ModoDeAcceso = enumModoDeAccesoDeDatos.Administrador;
                return;
            }

            if (NegociosDeSe.NegocioDeUnDtm(typeof(TRegistro)).EsUnNegocio())
                elemento.ModoDeAcceso = ApiDePermisos.LeerModoDeAcceso(Contexto, Negocio, parametros.Insertando ? 0 : registro.Id, parametros.Parametros);

            if (registro.ImplementaNecesitaSerParametrizador())
            {
                elemento.ModoDeAcceso = Contexto.SePuedeParametrizar() ? enumModoDeAccesoDeDatos.Administrador : enumModoDeAccesoDeDatos.Consultor;
            }
        }


        #endregion

        #region  Métodos de seguridad


        object IGestor.ValidarPermisosDePersistencia(enumTipoOperacion operacion, enumNegocio negocio, int id, Dictionary<string, object> parametros)
        {
            var registro = LeerRegistroPorId(id, true, false, false, false);
            ApiDePermisos.ValidarPermisosDePersistencia(Contexto, negocio, TipoDtmDelNegocio, new ParametrosDeNegocio(operacion, parametros), registro);
            return registro;
        }


        #endregion


        public void Dispose()
        {

        }

        object IGestor.TransitarElemento(object elemento, int idTransicion, Dictionary<string, object> parametros)
        {
            return TransitarElemento((TElemento)elemento, idTransicion, parametros);
        }

        ElementoDto IGestor.MapearDto(IRegistro registro, ParametrosDeNegocio parametros)
        {
            return MapearElemento((TRegistro)registro, parametros);
        }

        RegistroDtm IGestor.MapearDtm(IElementoDto elemento, ParametrosDeNegocio parametros)
        {
            return MapearRegistro((TElemento)elemento, parametros);
        }

        public TElemento TransitarElemento(TElemento elemento, int idTransicion, Dictionary<string, object> parametros)
        {
            try
            {
                TRegistro registro = (TRegistro)Negocio.LeerRegistro(Contexto, elemento.Id, aplicarJoin: false, parametros: new Dictionary<string, object>
                {
                    {ltrParametrosNeg.ValidarPermisosDeConsulta, false },
                    {ltrParametrosNeg.UsarLaCache, false }
                });
                registro = TransitarRegistro(registro, idTransicion, parametros);
                var idArchivo = parametros.LeerValor<int>(nameof(IUsaArchivo.IdArchivo), (int)0);
                if (idArchivo > 0)
                {
                    var archivo = Contexto.SeleccionarPorId<ArchivoDtm>(idArchivo);
                    var hito = ((IElementoDeProcesoDtm)registro).UltimoHito(Contexto);
                    var ext = Path.GetExtension(archivo.Nombre);
                    archivo.Nombre = $"Trn.{hito.Id}-{((TransicionDtm)parametros[nameof(TransicionDtm)]).Nombre}".Left(240).NormalizarFichero() + $"{ext}";
                    archivo.ModificarComoAdministrador(Contexto, accionQueSeEjecuta: ltrDeUnArchivo.Accion_Permitir_Modificar_Nombre);
                    GestorDeVinculos.Vincular(Contexto, Negocio, enumNegocio.Archivos, registro.Id, (int)idArchivo, new Dictionary<string, object> { { ltrParametrosNeg.ValidarPermisosDePersistencia, false } });
                }

                TElemento nuevo = MapearElemento(registro);
                return nuevo;
            }
            finally
            {
                VaciarCacheDeRegistroPorId(elemento.Id);
            }
        }


        object IGestor.TransitarRegistro(object registro, int idTransicion, Dictionary<string, object> parametros)
        {
            return TransitarRegistro((TRegistro)registro, idTransicion, parametros);
        }

        public TRegistro TransitarRegistro(TRegistro registro, int idTransicion, Dictionary<string, object> parametros)
        {
            var transicion = ApiDePermisos.ValidarTransicion(Contexto, Negocio, (IElementoDeProcesoDtm)registro, idTransicion, parametros);
            if (transicion == null)
                return registro;

            parametros[nameof(TransicionDtm)] = transicion;
            parametros[ltrParametrosNeg.EstadoOrigen] = Negocio.Estado(Contexto, transicion.IdOrigen);
            parametros[ltrParametrosNeg.EstadoDestino] = Negocio.Estado(Contexto, transicion.IdDestino);
            parametros[nameof(TipoDeElementoDtm)] = GestorDeTipos.LeerRegistroPorId(((IElementoDeProcesoDtm)registro).IdTipo, true);
            parametros[ltrParametrosNeg.EsUnaTransicion] = true;
            var transaccion = Contexto.IniciarTransaccion();
            var idSemaforo = SemaforoDeProcesoSql.PonerSemaforo(Contexto, Negocio.IdNegocio(), registro.Id, enumTipoOperacion.Transitar.ToBd(), $"{enumTipoOperacion.Transitar}: {transicion.Nombre}").Id;
            try
            {
                registro = AntesDeTransitar(registro, transicion, parametros);
                registro = (TRegistro)GestorDeHitos.Transitar(Contexto, Negocio, (IElementoDeProcesoDtm)registro, transicion, parametros);
                registro = DespuesDeTransitar(registro, transicion, parametros);
                registro = AlFinalizarDeTransitar(registro, transicion, parametros);
                Contexto.Commit(transaccion);
                return registro;
            }
            catch
            {
                Contexto.Rollback(transaccion);
                ServicioDeCaches.EliminarTodas();
                throw;
            }
            finally
            {
                SemaforoDeProcesoSql.QuitarSemaforo(Contexto, idSemaforo);
                EliminarCaches(registro, new ParametrosDeNegocio(enumTipoOperacion.Transitar) { Parametros = parametros });
                ServicioDeCaches.EliminarElementos(CacheDe.elemento_HitoAnterior_AlActual, $"{Negocio.ToString()}-{registro.Id}-");
                ServicioDeCaches.EliminarElementos(CacheDe.elemento_HitoAnterior_AlPrimero, $"{Negocio.ToString()}-{registro.Id}-");
            }
        }

        protected virtual TRegistro AntesDeTransitar(TRegistro registro, TransicionDtm transicion, Dictionary<string, object> parametros)
        {
            if (registro.ImplementaUsaSolicitante())
            {
                var solicitante = ((IUsaSolicitante)registro).Solicitante(Contexto);
                if (solicitante.Baja)
                    GestorDeErrores.Emitir($"No se puede aplicar la transición '{transicion.Nombre}' en '{((IUsaReferencia)registro).Referencia}' por estar el solicitante asociado '{solicitante.Referencia(Contexto)}' de baja");
            }

            parametros[ltrParametrosNeg.RegistroAntesTransitar] = registro;
            if (typeof(TRegistro).ImplementaUsaEtapas()) parametros[ltrTransiciones.EtapasOrigen] = ((IUsaEtapas)registro).Etapa;
            parametros[nameof(GestorDeTipos)] = GestorDeTipos;
            parametros[nameof(Metadatos)] = Metadatos;
            parametros[nameof(enumMomentoDeEjecucion)] = enumMomentoDeEjecucion.A;
            var acciones = AccionDeTrnSql.LeerAcciones(Contexto, ApiDeAccionDeTrn.TablaDeAcciones(Negocio.TipoDtm()), transicion.Id, enumMomentoDeEjecucion.A);
            return acciones.Count > 0 ? ProcesarAcciones(registro, acciones, parametros) : registro;
        }

        protected virtual TRegistro DespuesDeTransitar(TRegistro registro, TransicionDtm transicion, Dictionary<string, object> parametros)
        {
            parametros[nameof(enumMomentoDeEjecucion)] = enumMomentoDeEjecucion.D;
            if (typeof(TRegistro).ImplementaUsaEtapas()) parametros[ltrTransiciones.EtapasDestino] = ((IUsaEtapas)registro).Etapa;
            var accionesDeTrn = AccionDeTrnSql.LeerAcciones(Contexto, ApiDeAccionDeTrn.TablaDeAcciones(Negocio.TipoDtm()), transicion.Id, enumMomentoDeEjecucion.D);
            return accionesDeTrn.Count() > 0 ? ProcesarAcciones(registro, accionesDeTrn, parametros) : registro;
        }

        protected virtual TRegistro AlFinalizarDeTransitar(TRegistro registro, TransicionDtm transicion, Dictionary<string, object> parametros)
        {
            if (Negocio.UsaArchivos())
            {
                if (!Negocio.Estado(Contexto, transicion.IdOrigen).Cancelado && Negocio.Estado(Contexto, transicion.IdDestino).Cancelado)
                {
                    ((IElementoDeProcesoDtm)registro).MarcarArchivosComoCancelados(Contexto);
                }

                if (Negocio.Estado(Contexto, transicion.IdOrigen).Cancelado && !Negocio.Estado(Contexto, transicion.IdDestino).Cancelado)
                {
                    ((IElementoDeProcesoDtm)registro).DesmarcarArchivosCancelados(Contexto);
                }
            }

            parametros[nameof(enumMomentoDeEjecucion)] = enumMomentoDeEjecucion.T;
            if (typeof(TRegistro).ImplementaUsaEtapas()) parametros[ltrTransiciones.EtapasDestino] = ((IUsaEtapas)registro).Etapa;
            var accionesDeTrn = AccionDeTrnSql.LeerAcciones(Contexto, ApiDeAccionDeTrn.TablaDeAcciones(Negocio.TipoDtm()), transicion.Id, enumMomentoDeEjecucion.T);
            return accionesDeTrn.Count() > 0 ? ProcesarAcciones(registro, accionesDeTrn, parametros) : registro;
        }

        private TRegistro ProcesarAcciones(TRegistro registro, List<AccionesDeTrnDtm> accionesDeTrn, Dictionary<string, object> entrada)
        {
            var entorno = new EntornoDeUnaAccion(Contexto,
                registro,
                Negocio,
                entrada);
            entorno.Transicion = (TransicionDtm)entrada[nameof(TransicionDtm)];

            foreach (var accionDeTrn in accionesDeTrn)
            {
                if (!accionDeTrn.Activo)
                    continue;
                var accion = (AccionDtm)NegociosDeSe.CrearGestor(Contexto, enumNegocio.Accion).LeerRegistroPorId(accionDeTrn.IdAccion, false);

                accion.Ejecutar(entorno, accionDeTrn.Parametros);

            }
            registro = LeerRegistroPorId(registro.Id, false, false, false, true, new Dictionary<string, object> { { ltrParametrosNeg.ValidarPermisosDeConsulta, false } });
            return registro;
        }

        public dynamic DeserializarDto(string elementoJson)
        {
            return JsonConvert.DeserializeObject<TElemento>(elementoJson);
        }

        public (IEnumerable<HistorialDto> elementos, int total) LeerHistorial(int posicion, int cantidad, Dictionary<string, object> parametros)
        {
            if (!typeof(TRegistro).ImplementaElementoDeUnProceso())
                GestorDeErrores.Emitir($"Se ha solicitado el historial de un elemento del negocio '{Negocio.Singular()}', y dicho negocio no registra historial");

            List<ClausulaDeFiltrado> filtros = JsonConvert.DeserializeObject<List<ClausulaDeFiltrado>>(parametros.LeerValor<string>(ltrParametrosEp.Filtro));
            ClausulaDeFiltrado filtro = filtros.First(f => f.Clausula == ltrParametrosEp.id);

            var registro = (IElementoDeProcesoDtm)Negocio.LeerRegistro(Contexto, filtro.Valor.Entero());
            if (!registro.EsConsultor(Contexto))
                GestorDeErrores.Emitir($"Se ha solicitado el historial de un elemento del negocio '{Negocio.Singular()}', y el usuario '{Contexto.DatosDeConexion.Login}' no tiene permiso de consulta sobre dicho elemento");

            var sucesos = new List<HistorialDto>();

            HistorialDeUnElemento(registro, sucesos, filtros);

            var filtroDeReferencia = filtros.First(f => f.Clausula == ltrSucesosFiltros.referencia).Valor;
            var referenciasBuscadas = filtroDeReferencia.Split(Simbolos.separadorDeCadenasDeFiltrado);

            var sucesosFiltradosPorReferencia = referenciasBuscadas
            .Where(c => !string.IsNullOrEmpty(c)).Any()
            ? sucesos.Where(s => referenciasBuscadas.Any(c => s.Elemento.Equals(c, StringComparison.CurrentCultureIgnoreCase))).ToList() :
            sucesos.ToList();

            var filtroDeSuceso = filtros.First(f => f.Clausula == ltrSucesosFiltros.suceso).Valor;
            var cadenasBuscadas = filtroDeSuceso.Split(Simbolos.separadorDeCadenasDeFiltrado);
            var sucesosFiltrados = cadenasBuscadas.Where(c => !string.IsNullOrEmpty(c)).Any()
            ? sucesosFiltradosPorReferencia.Where(s => cadenasBuscadas.Any(c => s.Suceso.Contains(c, StringComparison.CurrentCultureIgnoreCase))).ToList()
            : sucesosFiltradosPorReferencia.ToList();

            var sucesosOrdenados = sucesosFiltrados.OrderByDescending(s => s.OcurridoEl).ToList();
            return (sucesosOrdenados, sucesosOrdenados.Count);
        }

        protected virtual void HistorialDeUnElemento(IElementoDeProcesoDtm registro, List<HistorialDto> sucesos, List<ClausulaDeFiltrado> filtros)
        {
            registro.HistorialDeHitos(Contexto, sucesos, filtros);
            registro.HistorialDeObservaciones(Contexto, sucesos, filtros);
            registro.HistorialDeTrazas(Contexto, sucesos, filtros, enumTraza.envioDeCorreo);
            if (Negocio.UsaAgenda()) registro.HistorialDeEventos(Contexto, sucesos, filtros);
            if (Negocio.ObtenerMetadatos().TareasDtm is not null) registro.HistorialDeTareas(Contexto, sucesos, filtros);
            if (Negocio.ObtenerMetadatos().ArchivadoresDtm is not null) registro.HistorialDeArchivadores(Contexto, sucesos, filtros);
            if (Negocio.UsaArchivos()) registro.HistorialDeArchivos(Contexto, sucesos, filtros);
        }
    }

}

