using Gestor.Errores;
using GestorDeElementos.Extensores;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ModeloDeDto;
using ModeloDeDto.Contabilidad;
using ModeloDeDto.Negocio;
using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.Seguridad;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using Utilidades;

namespace GestorDeElementos
{
    public class GestorDeVinculos
    {
        public ContextoSe Contexto { get; }
        public enumNegocio Negocio { get; }
        public enumNegocio Vinculado { get; }
        bool _darLaVuelta = false;

        private IGestor _gestorDelNegocio;
        private IGestor _gestorDelVinculado;

        public bool SeLeHaDadoLaVuelta => _darLaVuelta;

        public IRegistro Elemento1(int idElemento1, int idElemento2) =>
            !_darLaVuelta
            ? (IRegistro)_gestorDelNegocio.LeerRegistroPorId(idElemento1, aplicarJoin: true, parametros: new Dictionary<string, object> { { ltrParametrosNeg.ValidarPermisosDeConsulta, false } })
            : (IRegistro)_gestorDelNegocio.LeerRegistroPorId(idElemento2, aplicarJoin: true, parametros: new Dictionary<string, object> { { ltrParametrosNeg.ValidarPermisosDeConsulta, false } });

        public int IdElemento1(int idElemento1, int idElemento2) => !_darLaVuelta ? idElemento1 : idElemento2;

        public IRegistro Elemento2(int idElemento1, int idElemento2) =>
            !_darLaVuelta
            ? (IRegistro)_gestorDelVinculado.LeerRegistroPorId(idElemento2, aplicarJoin: true, parametros: new Dictionary<string, object> { { ltrParametrosNeg.ValidarPermisosDeConsulta, false } })
            : (IRegistro)_gestorDelVinculado.LeerRegistroPorId(idElemento1, aplicarJoin: true, parametros: new Dictionary<string, object> { { ltrParametrosNeg.ValidarPermisosDeConsulta, false } });

        public int IdElemento2(int idElemento1, int idElemento2) => !_darLaVuelta ? idElemento2 : idElemento1;

        private GestorDeVinculos(ContextoSe contexto, enumNegocio negocio1, enumNegocio negocio2)
        {
            Contexto = contexto;
            var tabla = ApiDeVinculos.TablaDeVinculacion(negocio1.TipoDtm(), negocio2, errorSiNoExiste: false);
            if (!tabla.IsNullOrEmpty() && GestorDeMetadatos.ExisteTabla(tabla))
            {
                Negocio = negocio1;
                Vinculado = negocio2;
            }
            else
            {
                tabla = ApiDeVinculos.TablaDeVinculacion(negocio2.TipoDtm(), negocio1);
                if (GestorDeMetadatos.ExisteTabla(tabla))
                {
                    Negocio = negocio2;
                    Vinculado = negocio1;
                    _darLaVuelta = true;
                }
                else GestorDeErrores.Emitir($"No hay vínculos definidos entre los negocios '{negocio1}' y '{negocio2}'");
            }
            _gestorDelNegocio = NegociosDeSe.CrearGestor(contexto, Negocio);
            _gestorDelVinculado = NegociosDeSe.CrearGestor(contexto, Vinculado);
        }

        public static GestorDeVinculos Gestor(ContextoSe contexto, enumNegocio negocio1, enumNegocio negocio2)
        {
            return new GestorDeVinculos(contexto, negocio1, negocio2);
        }

        public static List<T> ElementosVinculados<T>(ContextoSe contexto, enumNegocio negocio, enumNegocio vinculado, int idElemento, Dictionary<string, object> parametros = null, Dictionary<string, object> filtros = null)
        where T : ElementoDto
        =>
        Gestor(contexto, negocio, vinculado).ElementosVinculados<T>(idElemento, parametros == null ? new Dictionary<string, object>() : parametros, filtros);

        public static IList ElementosVinculados(ContextoSe contexto, enumNegocio negocio, enumNegocio vinculado, int idElemento, Dictionary<string, object> parametros = null, Dictionary<string, object> filtros = null)
        {
            var gestor = Gestor(contexto, negocio, vinculado);
            parametros = parametros == null ? new Dictionary<string, object>() : parametros;
            if (gestor.SeLeHaDadoLaVuelta)
                return gestor.ElementosVinculadosAl(idElemento, parametros);

            return gestor.ElementosVinculadosCon(idElemento, parametros);
        }

        public static List<T> RegistrosVinculados<T>(ContextoSe contexto, enumNegocio negocio, int idElemento, Dictionary<string, object> parametros = null, Dictionary<string, object> filtros = null)
        where T : RegistroDtm
        {
            var vinculado = typeof(T).NegocioDeUnDtm();
            return Gestor(contexto, negocio, vinculado).RegistrosVinculados<T>(idElemento,
                parametros == null ? new Dictionary<string, object>() : parametros,
                filtros);
        }

        public static List<T> RegistrosVinculados<T>(ContextoSe contexto, enumNegocio negocio, enumNegocio vinculado, int idElemento, Dictionary<string, object> parametros = null, Dictionary<string, object> filtros = null)
        where T : RegistroDtm
        =>
        Gestor(contexto, negocio, vinculado).RegistrosVinculados<T>(idElemento,
            parametros == null ? new Dictionary<string, object>() : parametros,
            filtros);

        public static (RegistroDtm elemento1, RegistroDtm elemento2) BorrarVinculo(ContextoSe contexto, enumNegocio negocio1, enumNegocio negocio2, int idElemento1, int idElemento2, Dictionary<string, object> parametros) =>
            Gestor(contexto, negocio1, negocio2).BorrarVinculo(idElemento1, idElemento2, parametros);

        public static TElemento CrearVinculo<TElemento>(ContextoSe contexto, enumNegocio negocio1, enumNegocio negocio2, int idElemento1, TElemento elemento, Dictionary<string, object> parametros)
        where TElemento : IElementoDto
        {
            var gestor = NegociosDeSe.CrearGestor(contexto, negocio2);
            parametros.Add(ltrParametrosNeg.InsertandoParaVincular, true);
            var t = contexto.IniciarTransaccion();
            try
            {
                negocio1.ValidarAccesoPorEstado(contexto, idElemento1, validarSiTerminado: negocio2 != enumNegocio.EventoDeAgenda);
                var elemento2 = (IElementoDto)gestor.PersistirElementoDto(elemento, new ParametrosDeNegocio(enumTipoOperacion.Insertar, parametros));
                Vincular(contexto, negocio1, negocio2, idElemento1, elemento2.Id, parametros);
                return (TElemento)elemento2;
            }
            catch
            {
                contexto.Rollback(t);
                throw;
            }
        }

        public static IRegistro Vincular(ContextoSe contexto, IRegistro registro1, IRegistro registro2, Dictionary<string, object> parametros = null)
        {
            var negocio1 = NegociosDeSe.NegocioDeUnDtm(registro1.GetType());
            var negocio2 = NegociosDeSe.NegocioDeUnDtm(registro2.GetType());
            if (registro2.Id == 0)
            {
                if (parametros == null) parametros = new Dictionary<string, object>();
                parametros[ltrParametrosNeg.InsertandoParaVincular] = true;
                registro2 = (IRegistro)NegociosDeSe.CrearGestor(contexto, negocio2).PersistirRegistro(registro2, new ParametrosDeNegocio(enumTipoOperacion.Insertar, parametros));
            }
            Gestor(contexto, negocio1, negocio2).CrearVinculo(registro1.Id, registro2.Id, vincularSiNoLoEsta: true);
            return (IRegistro)NegociosDeSe.CrearGestor(contexto, negocio1).LeerRegistroPorId(registro1.Id, true, false, parametros: new Dictionary<string, object> { { ltrParametrosNeg.ValidarPermisosDeConsulta, false } });
        }

        public static int Vincular(ContextoSe contexto, enumNegocio negocio1, enumNegocio negocio2, int idElemento1, int idElemento2, Dictionary<string, object> parametros = null)
        {
            var gestor = Gestor(contexto, negocio1, negocio2);
            var existe = gestor.ExisteElVinculo(idElemento1, idElemento2);
            if (existe && parametros.LeerValor(ltrParametrosNeg.ErrorSiEstaVinculado, false))
                GestorDeErrores.Emitir($"Ya están vinculados los elementos de los negocios {negocio1.Singular()} y {negocio2.Singular()}");

            return existe
            ? VinculoSql.LeerVinculo(contexto, NegociosDeSe.TipoDtm(negocio1), negocio2, idElemento1, idElemento2, erroSiNoHay: true).Id
            : gestor.CrearVinculo(idElemento1, idElemento2, vincularSiNoLoEsta: true, parametros).Id;
        }


        public static List<VinculoDtm> Vinculos<T>(ContextoSe contexto, T elemento1, enumNegocio vinculado, Dictionary<string, object> parametros = null)
        where T : IElementoDtm
        {
            var negocio = typeof(T).NegocioDeUnDtm();
            return Gestor(contexto, negocio, vinculado).Vinculos(elemento1.Id);
        }


        public static bool Existen(ContextoSe contexto, enumNegocio negocio1, enumNegocio negocio2, int idElemento1) =>
            Gestor(contexto, negocio1, negocio2).ContarVinculosCon(idElemento1) > 0;

        public static int Cantidad(ContextoSe contexto, enumNegocio negocio1, enumNegocio negocio2, int idElemento1) =>
            Gestor(contexto, negocio1, negocio2).ContarVinculosCon(idElemento1);

        public static bool Existe(ContextoSe contexto, enumNegocio negocio1, enumNegocio negocio2, int idElemento1, int idElemento2) =>
            Gestor(contexto, negocio1, negocio2).ExisteElVinculo(idElemento1, idElemento2);

        private int ContarVinculosCon(int idElemento)
        {
            if (_darLaVuelta)
                return VinculoSql.ContarVinculosAl(Contexto, Negocio.TipoDtm(), Vinculado, idElemento2: idElemento);

            return VinculoSql.ContarVinculosCon(Contexto, Negocio.TipoDtm(), Vinculado.TipoDtm(), idElemento);
        }

        private bool ExisteElVinculo(int idElemento1, int idElemento2)
        {
            var cantidad = 0;
            if (_darLaVuelta)
                cantidad = VinculoSql.ExisteElVinculo(Contexto, Vinculado.TipoDtm(), Negocio.TipoDtm(), idElemento2, idElemento1);
            else
                cantidad = VinculoSql.ExisteElVinculo(Contexto, Negocio.TipoDtm(), Vinculado.TipoDtm(), idElemento1, idElemento2);
            return cantidad == 1;
        }

        private List<VinculoDtm> Vinculos(int idElemento)
        {
            return !_darLaVuelta
            ? VinculoSql.LeerVinculosCon(Contexto, Negocio.TipoDtm(), Vinculado.TipoDtm(), idElemento, new Dictionary<string, object>())
            : VinculoSql.LeerVinculosAl(Contexto, Negocio.TipoDtm(), Vinculado, Vinculado.TipoDtm(), idElemento, new Dictionary<string, object>());
        }

        private List<T> ElementosVinculados<T>(int idElemento, Dictionary<string, object> parametros, Dictionary<string, object> filtros)
        where T : ElementoDto
        {
            return !_darLaVuelta ? ElementosVinculadosCon<T>(idElemento, parametros, filtros) : ElementosVinculadosAl<T>(idElemento, parametros);
        }

        private List<T> RegistrosVinculados<T>(int idElemento, Dictionary<string, object> parametros, Dictionary<string, object> filtros)
        where T : RegistroDtm
        {
            return !_darLaVuelta ? RegistrosVinculadosCon<T>(idElemento, parametros, filtros) : RegistrosVinculadosAl<T>(idElemento, parametros, filtros);
        }
        private List<T> RegistrosVinculadosCon<T>(int idElemento1, Dictionary<string, object> parametros, Dictionary<string, object> filtros)
        where T : RegistroDtm
        {
            var vinculos = VinculoSql.LeerVinculosCon(Contexto, Negocio.TipoDtm(), Vinculado.TipoDtm(), idElemento1, filtros);
            var gestor = Vinculado.CrearGestor(Contexto);
            var registros = new List<T>();
            foreach (var vinculo in vinculos)
            {
                T vinculado = (T)gestor.LeerRegistroPorId(vinculo.idElemento2, (bool)parametros.LeerValor(ltrParametrosNeg.AplicarJoin, false),
                    parametros: new Dictionary<string, object> {
                        {ltrParametrosNeg.ValidarPermisosDeConsulta, false }
                    });

                if (vinculado.GetType().ImplementaUsaBaja() && ((IUsaBaja)vinculado).Baja)
                {
                    if (!parametros.LeerValor(ltrParametrosNeg.IncluirBajas, false))
                    {
                        if (Vinculado.UsaArchivos() && Vinculado.Archivos(Contexto, vinculo.idElemento2).Count() == 0)
                            continue;
                    }
                }

                if (vinculado.GetType().ImplementaElementoDeUnProceso())
                {
                    if (((IElementoDeProcesoDtm)vinculado).Estado(Contexto).Cancelado)
                        if (!parametros.LeerValor(ltrParametrosNeg.IncluirCancelados, true) || parametros.LeerValor(ltrParametrosNeg.ExcluirCancelados, false))
                            continue;
                    if (((IElementoDeProcesoDtm)vinculado).Estado(Contexto).Terminado)
                        if (parametros.LeerValor(ltrParametrosNeg.ExcluirTerminados, false))
                            continue;
                }

                registros.Add(vinculado);
            }
            return registros;
        }

        private List<T> RegistrosVinculadosAl<T>(int idElemento2, Dictionary<string, object> parametros, Dictionary<string, object> filtros)
        where T : RegistroDtm
        {
            var vinculos = VinculoSql.LeerVinculosAl(Contexto, Negocio.TipoDtm(), Vinculado, Vinculado.TipoDtm(), idElemento2, filtros);
            var gestor = Negocio.CrearGestor(Contexto);
            var registros = new List<T>();
            foreach (var vinculo in vinculos)
            {
                T vinculado = (T)gestor.LeerRegistroPorId(vinculo.idElemento1, (bool)parametros.LeerValor(ltrParametrosNeg.AplicarJoin, false), parametros: new Dictionary<string, object> { { ltrParametrosNeg.ValidarPermisosDeConsulta, false } });
                if (vinculado.GetType().ImplementaUsaBaja() && ((IUsaBaja)vinculado).Baja)
                    if (!parametros.LeerValor(ltrParametrosNeg.IncluirBajas, false))
                    {
                        if (Vinculado.UsaArchivos() && Vinculado.Archivos(Contexto, vinculo.idElemento2).Count() == 0)
                            continue;
                    }

                if (!(bool)parametros.LeerValor(ltrParametrosNeg.IncluirCancelados, true))
                    continue;

                registros.Add(vinculado);
            }
            return registros;
        }

        private List<T> ElementosVinculadosCon<T>(int idElemento1, Dictionary<string, object> parametros, Dictionary<string, object> filtros)
        where T : ElementoDto
        {
            var vinculos = VinculoSql.LeerVinculosCon(Contexto, Negocio.TipoDtm(), Vinculado.TipoDtm(), idElemento1, filtros);
            var gestor = Vinculado.CrearGestor(Contexto);
            var elementos = new List<T>();
            parametros.Add(ltrParametrosNeg.ErrorSiNoLoHay, false);
            foreach (var vinculo in vinculos)
            {
                var vinculado = (T)gestor.LeerElementoPorId(vinculo.idElemento2, parametros);

                //Si no lo encuentra es porque se ha aplicado seguridad y el usuario conectado no tiene acceso
                if (vinculado == null)
                    continue;

                if (vinculado.GetType().ImplementaBajaDto() && ((IUsaBajaDto)vinculado).Baja)
                    if (!parametros.LeerValor(ltrParametrosNeg.IncluirBajas, false))
                    {
                        if (Vinculado.UsaArchivos() && Vinculado.Archivos(Contexto, vinculo.idElemento2).Count() == 0)
                            continue;
                    }

                if (!(bool)parametros.LeerValor(ltrParametrosNeg.IncluirCancelados, true))
                    continue;

                if (vinculado.ModoDeAcceso == enumModoDeAccesoDeDatos.SinPermiso)
                    continue;

                elementos.Add(vinculado);
            }
            return elementos;
        }

        private List<T> ElementosVinculadosAl<T>(int idElemento2, Dictionary<string, object> parametros)
        where T : ElementoDto
        {
            var vinculos = VinculoSql.LeerVinculosAl(Contexto, Negocio.TipoDtm(), Vinculado, Vinculado.TipoDtm(), idElemento2, filtros: null);
            var gestor = Negocio.CrearGestor(Contexto);
            var elementos = new List<T>();
            foreach (var vinculo in vinculos)
            {
                var vinculado = (T)gestor.LeerElementoPorId(vinculo.idElemento1, new Dictionary<string, object>());
                if (vinculado.GetType().ImplementaBajaDto() && ((IUsaBajaDto)vinculado).Baja)
                    if (!parametros.LeerValor(ltrParametrosNeg.IncluirBajas, false))
                    {
                        if (Vinculado.UsaArchivos() && Vinculado.Archivos(Contexto, vinculo.idElemento2).Count() == 0)
                            continue;
                    }

                if (!(bool)parametros.LeerValor(ltrParametrosNeg.IncluirCancelados, true))
                    continue;

                elementos.Add(vinculado);
            }
            return elementos;
        }

        private IList ElementosVinculadosAl(int idElemento2, Dictionary<string, object> parametros)
        {
            var vinculos = VinculoSql.LeerVinculosAl(Contexto, Negocio.TipoDtm(), Vinculado, Vinculado.TipoDtm(), idElemento2, filtros: null);
            var elementos = LeerVinculos(Contexto, Negocio, vinculos, parametros, leer1: true);
            return elementos;
        }

        private IList ElementosVinculadosCon(int idElemento1, Dictionary<string, object> parametros)
        {
            var vinculos = VinculoSql.LeerVinculosCon(Contexto, Negocio.TipoDtm(), Vinculado.TipoDtm(), idElemento1, filtros: null);

            var elementos = LeerVinculos(Contexto, Vinculado, vinculos, parametros, leer1: false);
            return elementos;
        }

        private static IList LeerVinculos(ContextoSe contexto, enumNegocio negocio, List<VinculoDtm> vinculos, Dictionary<string, object> parametros, bool leer1)
        {
            Type tipoDto = negocio.TipoDto();
            var gestor = negocio.CrearGestor(contexto);
            IList elementos = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(tipoDto));

            var posicion = parametros.LeerValor(ltrParametrosEp.Posicion, -1);
            var leidos = 0;
            var indice = 0;
            foreach (var vinculo in vinculos)
            {
                if (posicion > -1)
                {
                    if (indice < posicion)
                    {
                        indice++;
                        continue;
                    }
                    if (leidos == 10) break;
                }

                object elemento = gestor.LeerElementoPorId(leer1 ? vinculo.idElemento1 : vinculo.idElemento2, parametros);
                if (elemento.GetType().ImplementaBajaDto() && ((IUsaBajaDto)elemento).Baja)
                {
                    if (!parametros.LeerValor(ltrParametrosNeg.IncluirBajas, false))
                    {
                        if (negocio.UsaArchivos() && negocio.Archivos(contexto, vinculo.idElemento2).Count() == 0)
                            continue;
                    }
                }
                if (!(bool)parametros.LeerValor(ltrParametrosNeg.IncluirCancelados, true))
                    continue;
                elementos.Add(elemento);
                leidos++;
            }
            if (negocio.UsaTipo())
            {
                List<object> listaElementos = elementos.Cast<object>().ToList();

                // Ordenar la lista por Tipo e Id
                listaElementos = listaElementos.OrderBy(x => x.GetType().GetProperty("Tipo")?.GetValue(x, null)).ThenBy(x => x.GetType().GetProperty("Id")?.GetValue(x, null)).ToList();

                // Reconvertir a IList
                elementos = (IList)listaElementos;
            }
            return elementos;
        }

        private void EjecutarAccionesAntesDeVincular(int idElemento1, int idElemento2, Dictionary<string, object> parametros = null)
        {

            if (Negocio.UsaCg() && Vinculado.UsaCg())
            {
                if (((IUsaCg)Elemento1(idElemento1, idElemento2)).Cg.IdSociedad != ((IUsaCg)Elemento2(idElemento1, idElemento2)).Cg.IdSociedad)
                {
                    var e1 = Negocio.UsaReferencia() ? ((IUsaReferencia)Elemento1(idElemento1, idElemento2)).Referencia : ((INombre)Elemento1(idElemento1, idElemento2)).Nombre;
                    var e2 = Vinculado.UsaReferencia() ? ((IUsaReferencia)Elemento2(idElemento1, idElemento2)).Referencia : ((INombre)Elemento2(idElemento1, idElemento2)).Nombre;
                    GestorDeErrores.Emitir($"No se pueden vincular los elementos {e1} y {e2} ya que no pertenecen a la misma sociedad");
                }
            }

            var accionesRn = LeerAccionesDeRelacion(enumMomentoDeRelacion.AR);
            if (accionesRn.Count > 0)
                EjecutarAccionesDeRelacion(idElemento1, idElemento2, accionesRn);
        }

        public VinculoDtm CrearVinculo(int idElemento1, int idElemento2, bool vincularSiNoLoEsta = true, Dictionary<string, object> parametros = null)
        {
            if (parametros == null)
                parametros = new Dictionary<string, object>();

            if (parametros.Count == 0 || !parametros.ContainsKey(ltrParametrosNeg.Peticion))
                parametros[ltrParametrosNeg.EstaEjecutandoUnaAccion] = true;

            var tran = Contexto.IniciarTransaccion();
            try
            {
                ValidarPermisosDePersistencia(Elemento1(idElemento1, idElemento2).Id, Elemento2(idElemento1, idElemento2).Id, parametros,
                    $"No tiene permisos para crear el vínculo entre '{Negocio}' y '{Vinculado}'");

                AntesDeVincular(_gestorDelNegocio, IdElemento1(idElemento1, idElemento2), IdElemento2(idElemento1, idElemento2), parametros);
                AntesDeVincular(_gestorDelVinculado, IdElemento2(idElemento1, idElemento2), IdElemento1(idElemento1, idElemento2), parametros);

                if (Negocio.EsNegocioDeBD() && Vinculado.EsNegocioDeBD()) EjecutarAccionesAntesDeVincular(idElemento1, idElemento2, parametros);

                var vinculo = VinculoSql.CrearVinculo(Contexto, Negocio.TipoDtm(), Vinculado, IdElemento1(idElemento1, idElemento2), IdElemento2(idElemento1, idElemento2), vincularSiNoLoEsta);

                if (Negocio.EsNegocioDeBD() && Vinculado.EsNegocioDeBD()) EjecutarAccionesDespuesDeVincular(idElemento1, idElemento2, parametros);

                DespuesDeVincular(_gestorDelNegocio, IdElemento1(idElemento1, idElemento2), IdElemento2(idElemento1, idElemento2), parametros);
                DespuesDeVincular(_gestorDelVinculado, IdElemento2(idElemento1, idElemento2), IdElemento1(idElemento1, idElemento2), parametros);

                Contexto.ChangeTracker.Clear();
                Contexto.Commit(tran);
                return vinculo;
            }
            catch
            {
                Contexto.Rollback(tran);
                throw;
            }
            finally
            {
                Contexto.ChangeTracker.Clear();
            }
        }

        private void DespuesDeVincular(IGestor gestor, int idElemento, int idVinculado, Dictionary<string, object> parametros)
        {
            var metodo = ApiDeEnsamblados.MetodoEstatico(ApiDeEnsamblados.GestoresDeNegocio, gestor.GetType().FullName, ApiDeEnsamblados.DespuesDeVincular);
            if (metodo != null)
            {
                var entorno = new EntornoDeUnaAccion(Contexto, (IRegistro)gestor.LeerRegistroPorId(idElemento, aplicarJoin: true, parametros: new Dictionary<string, object> { { ltrParametrosNeg.ValidarPermisosDeConsulta, false } }), gestor.Negocio, parametros);
                entorno.Contexto.Accion = new AccionDtm { Dll = ApiDeEnsamblados.GestoresDeNegocio, Clase = gestor.GetType().FullName, Metodo = ApiDeEnsamblados.DespuesDeVincular, Nombre = "Después de crear un vínculo", ClaseDeAccion = enumClaseDeAccion.DLL.ToString() };
                var parametrosFijos = new Dictionary<string, object> {
                        {nameof(ltrParametrosNeg.Negocio), gestor.Negocio == Vinculado ?  Vinculado: Negocio},
                        {nameof(ltrParametrosNeg.Vinculado), gestor.Negocio == Vinculado ? Negocio: Vinculado},
                        {nameof(ltrParametrosNeg.IdElemento), idElemento},
                        {nameof(ltrParametrosNeg.IdVinculado), idVinculado}
                    };
                entorno.AsignarParametros(parametros is null ? parametrosFijos : parametrosFijos.Concatenar(parametros));
                try
                {
                    EntornoDeUnaAccion.EjecutarAccion(entorno);
                }
                finally
                {
                    entorno.Contexto.Accion = null;
                }
            }
        }

        private void AntesDeVincular(IGestor gestor, int idElemento, int idVinculado, Dictionary<string, object> parametros)
        {
            var metodo = ApiDeEnsamblados.MetodoEstatico(ApiDeEnsamblados.GestoresDeNegocio, gestor.GetType().FullName, ApiDeEnsamblados.AntesDeVincular);
            if (metodo != null)
            {
                var entorno = new EntornoDeUnaAccion(Contexto, (IRegistro)gestor.LeerRegistroPorId(idElemento, aplicarJoin: true, parametros: new Dictionary<string, object> { { ltrParametrosNeg.ValidarPermisosDeConsulta, false } }), gestor.Negocio, parametros);
                entorno.Contexto.Accion = new AccionDtm { Dll = ApiDeEnsamblados.GestoresDeNegocio, Clase = gestor.GetType().FullName, Metodo = ApiDeEnsamblados.AntesDeVincular, Nombre = "Antes de crear un vínculo", ClaseDeAccion = enumClaseDeAccion.DLL.ToString() };

                var parametrosFijos = new Dictionary<string, object> {
                        {nameof(ltrParametrosNeg.Negocio), gestor.Negocio},
                        {nameof(ltrParametrosNeg.Vinculado), gestor.Negocio == Vinculado ? Negocio: Vinculado},
                        {nameof(ltrParametrosNeg.IdElemento), idElemento},
                        {nameof(ltrParametrosNeg.IdVinculado), idVinculado}
                    };
                entorno.AsignarParametros(parametros is null ? parametrosFijos : parametrosFijos.Concatenar(parametros));

                try
                {
                    EntornoDeUnaAccion.EjecutarAccion(entorno);
                }
                finally
                {
                    entorno.Contexto.Accion = null;
                }
            }
        }

        private void EjecutarAccionesDespuesDeVincular(int idElemento1, int idElemento2, Dictionary<string, object> parametros)
        {
            var accionesRn = LeerAccionesDeRelacion(enumMomentoDeRelacion.DR);
            if (accionesRn.Count > 0)
                EjecutarAccionesDeRelacion(idElemento1, idElemento2, accionesRn);
        }

        private void AntesDeEliminarVinculo(int idElemento1, int idElemento2, Dictionary<string, object> parametros)
        {

            var accionesRn = LeerAccionesDeRelacion(enumMomentoDeRelacion.AB);
            if (accionesRn.Count > 0)
                EjecutarAccionesDeRelacion(idElemento1, idElemento2, accionesRn);
        }

        private (RegistroDtm elemento1, RegistroDtm elemento2) BorrarVinculo(int idElemento1, int idElemento2, Dictionary<string, object> parametros)
        {
            var tran = Contexto.IniciarTransaccion();
            try
            {
                if (parametros == null) parametros = new Dictionary<string, object>();

                AntesDeQuitarVinculo(_gestorDelNegocio, IdElemento1(idElemento1, idElemento2), IdElemento2(idElemento1, idElemento2), parametros);
                AntesDeQuitarVinculo(_gestorDelVinculado, IdElemento2(idElemento1, idElemento2), IdElemento1(idElemento1, idElemento2), parametros);

                if (Negocio.EsNegocioDeBD() && Vinculado.EsNegocioDeBD()) AntesDeEliminarVinculo(idElemento1, idElemento2, parametros);

                ValidarPermisosDePersistencia(idElemento1, idElemento2, parametros, $"No tiene permisos para eliminar el vínculo entre '{Negocio}' y '{Vinculado}'");

                VinculoSql.QuitarVinculo(Contexto, Negocio.TipoDtm(), Vinculado, ((RegistroDtm)Elemento1(idElemento1, idElemento2)).Id, ((RegistroDtm)Elemento2(idElemento1, idElemento2)).Id);

                if (Negocio.EsNegocioDeBD() && Vinculado.EsNegocioDeBD()) DespuesDeEliminarVinculo(idElemento1, idElemento2, parametros);
                Contexto.ChangeTracker.Clear();

                DespuesDeQuitarVinculo(_gestorDelNegocio, IdElemento1(idElemento1, idElemento2), IdElemento2(idElemento1, idElemento2), parametros);
                DespuesDeQuitarVinculo(_gestorDelVinculado, IdElemento2(idElemento1, idElemento2), IdElemento1(idElemento1, idElemento2), parametros);

                Contexto.Commit(tran);
                return
                (
                  Negocio.SeleccionarPorId(Contexto, idElemento1, parametros: new Dictionary<string, object> { { nameof(ltrParametrosNeg.ErrorSiNoLoHay), false } }),
                  Vinculado.SeleccionarPorId(Contexto, idElemento2, parametros: new Dictionary<string, object> { { nameof(ltrParametrosNeg.ErrorSiNoLoHay), false } })
                );
            }
            catch
            {
                Contexto.Rollback(tran);
                throw;
            }
            finally
            {
                Contexto.ChangeTracker.Clear();
            }
        }

        private void AntesDeQuitarVinculo(IGestor gestor, int idElemento, int idVinculado, Dictionary<string, object> parametros)
        {
            var metodo = ApiDeEnsamblados.MetodoEstatico(ApiDeEnsamblados.GestoresDeNegocio, gestor.GetType().FullName, ApiDeEnsamblados.AntesDeQuitarVinculo);
            if (metodo != null)
            {
                var entorno = new EntornoDeUnaAccion(Contexto, (IRegistro)gestor.LeerRegistroPorId(idElemento, aplicarJoin: true, parametros: new Dictionary<string, object> { { ltrParametrosNeg.ValidarPermisosDeConsulta, false } }), gestor.Negocio, parametros);
                entorno.Contexto.Accion = new AccionDtm { Dll = ApiDeEnsamblados.GestoresDeNegocio, Clase = gestor.GetType().FullName, Metodo = ApiDeEnsamblados.AntesDeQuitarVinculo, Nombre = "Antes de quitar un vínculo", ClaseDeAccion = enumClaseDeAccion.DLL.ToString() };
                var parametrosFijos = new Dictionary<string, object> {
                        {nameof(ltrParametrosNeg.Negocio), gestor.Negocio},
                        {nameof(ltrParametrosNeg.Vinculado), gestor.Negocio == Vinculado ? Negocio: Vinculado},
                        {nameof(ltrParametrosNeg.IdElemento), idElemento},
                        {nameof(ltrParametrosNeg.IdVinculado), idVinculado}
                    };
                entorno.AsignarParametros(parametros is null ? parametrosFijos : parametrosFijos.Concatenar(parametros));
                try
                {
                    EntornoDeUnaAccion.EjecutarAccion(entorno);
                }
                finally
                {
                    entorno.Contexto.Accion = null;
                }
            }
        }

        private void DespuesDeEliminarVinculo(int idElemento1, int idElemento2, Dictionary<string, object> parametros)
        {
            var accionesRn = LeerAccionesDeRelacion(enumMomentoDeRelacion.DB);
            if (accionesRn.Count > 0)
                EjecutarAccionesDeRelacion(idElemento1, idElemento2, accionesRn);

            if (Vinculado == enumNegocio.EventoDeAgenda)
                Contexto.EliminarPorId<EventoDeAgendaDtm>(((RegistroDtm)Elemento2(idElemento1, idElemento2)).Id, parametros);
        }

        private void DespuesDeQuitarVinculo(IGestor gestor, int idElemento, int idVinculado, Dictionary<string, object> parametros)
        {
            var metodo = ApiDeEnsamblados.MetodoEstatico(ApiDeEnsamblados.GestoresDeNegocio, gestor.GetType().FullName, ApiDeEnsamblados.DespuesDeQuitarVinculo);
            if (metodo != null)
            {
                var entorno = new EntornoDeUnaAccion(Contexto, (IRegistro)gestor.LeerRegistroPorId(idElemento, aplicarJoin: true), gestor.Negocio, parametros);
                entorno.Contexto.Accion = new AccionDtm { Dll = ApiDeEnsamblados.GestoresDeNegocio, Clase = gestor.GetType().FullName, Metodo = ApiDeEnsamblados.DespuesDeQuitarVinculo, Nombre = "Despues de quitar un vínculo", ClaseDeAccion = enumClaseDeAccion.DLL.ToString() };
                var parametrosFijos = new Dictionary<string, object> {
                        {nameof(ltrParametrosNeg.Negocio), gestor.Negocio},
                        {nameof(ltrParametrosNeg.Vinculado), gestor.Negocio == Vinculado ? Negocio: Vinculado},
                        {nameof(ltrParametrosNeg.IdElemento), idElemento},
                        {nameof(ltrParametrosNeg.IdVinculado), idVinculado}
                    };
                entorno.AsignarParametros(parametros is null ? parametrosFijos : parametrosFijos.Concatenar(parametros));
                try
                {
                    EntornoDeUnaAccion.EjecutarAccion(entorno);
                }
                finally
                {
                    entorno.Contexto.Accion = null;
                }
            }
        }

        private void ValidarPermisosDePersistencia(int idElemento1, int idElemento2, Dictionary<string, object> parametros, string mensajeDeError)
        {
            if (parametros.LeerValor(ltrParametrosNeg.EstaEjecutandoUnaAccion, false))
                return;

            if (!parametros.LeerValor(ltrParametrosNeg.ValidarPermisosDePersistencia, true))
                return;

            var p = new Dictionary<string, object> { { ltrParametrosNeg.ErrorSiNoLoHay, false } };
            var r1 = Negocio.SeleccionarPorId(Contexto, idElemento1, parametros: p);
            var r2 = Vinculado.SeleccionarPorId(Contexto, idElemento2, parametros: p);

            if (r1 is not null && Negocio.UsaBaja() && ((IUsaBaja)r1).Baja)
                GestorDeErrores.Emitir($"{Negocio.Singular()} '{r1.Expresion}' está de baja, no se le puede vincular/desvincular otros elementos");

            if (r2 is not null && Vinculado.UsaBaja() && ((IUsaBaja)r2).Baja)
                GestorDeErrores.Emitir($"{Vinculado.Singular()} '{r2.Expresion}' está de baja, no se le puede vincular/desvincular otros elementos");

            if (r1 is not null && Negocio.UsaFlujo() && Vinculado != enumNegocio.EventoDeAgenda)
                ((ElementoDeProcesoDtm)r1).ValidarQueEstaActivo(Contexto, ", no se le puede vincular/desvincular otros elementos");

            if (r2 is not null && Vinculado.UsaFlujo())
                ((ElementoDeProcesoDtm)r2).ValidarQueEstaActivo(Contexto, ", no se le puede vincular/desvincular otros elementos");

            if (!parametros.LeerValor(ltrParametrosNeg.ValidarPermisosDePersistencia, false))
                return;

            var modo = ApiDePermisos.LeerModoDeAccesoAlVinculo(Contexto, Negocio, Vinculado, IdElemento1(idElemento1, idElemento2), IdElemento2(idElemento1, idElemento2));
            var modoNecesario = parametros.LeerValor(nameof(ParametrosDeNegocio.PermisosNecesariosParaVincular), enumModoDeAccesoDeDatos.Gestor);

            if (!modo.HayPermisosDe(modoNecesario))
                GestorDeErrores.Emitir(mensajeDeError);
        }


        private List<AccionesDeRelacionDtm> LeerAccionesDeRelacion(enumMomentoDeRelacion momento)
        {

            var negocio = !_darLaVuelta ? Negocio : Vinculado;
            var vinculado = !_darLaVuelta ? Vinculado : Negocio;

            var filtros = FiltrosPorMomento(negocio, vinculado, momento);
            var orden = new ClausulaDeOrdenacion(nameof(AccionesDeRelacionDtm.Momento), ModoDeOrdenancion.ascendente);
            var ordenacion = new List<ClausulaDeOrdenacion> { orden };
            var pn = new ParametrosDeNegocio(enumTipoOperacion.LeerSinBloqueo, true);

            var gestor = NegociosDeSe.CrearGestor(Contexto, typeof(AccionesDeRelacionDtm), typeof(AccionesDeRelacionDto));

            var accionesRn = (List<AccionesDeRelacionDtm>)gestor.LeerRegistros(0, -1, filtros, ordenacion, pn);
            return accionesRn;
        }

        private void EjecutarAccionesDeRelacion(int idElemento1, int idElemento2, List<AccionesDeRelacionDtm> accionesRn)
        {
            var parametros = new Dictionary<string, object>
            {
                { nameof(ltrParametrosNeg.Negocio), Negocio },
                { nameof(ltrParametrosNeg.Vinculado), Vinculado },
                { nameof(ltrParametrosNeg.IdElemento), Elemento1(idElemento1, idElemento2).Id },
                { nameof(ltrParametrosNeg.IdVinculado), Elemento2(idElemento1, idElemento2).Id }
            };
            var entorno = new EntornoDeUnaAccion(Contexto, Elemento1(idElemento1, idElemento2), Negocio, parametros);
            foreach (var accionRn in accionesRn)
            {
                entorno.Contexto.Accion = accionRn.Accion;
                entorno.AsignarParametros(accionRn.Parametros);
                try
                {
                    EntornoDeUnaAccion.EjecutarAccion(entorno);
                }
                finally
                {
                    entorno.Contexto.Accion = null;
                }
            }
        }

        public static List<ClausulaDeFiltrado> FiltrosPorMomento(enumNegocio negocio, enumNegocio vinculado, enumMomentoDeRelacion momento)
        {
            var idNegocio = NegociosDeSe.IdNegocio(negocio);
            var idVinculado = NegociosDeSe.IdNegocio(vinculado);
            var f1 = new ClausulaDeFiltrado(nameof(AccionesDeRelacionDtm.IdNegocio), enumCriteriosDeFiltrado.igual, idNegocio);
            var f2 = new ClausulaDeFiltrado(nameof(AccionesDeRelacionDtm.IdVinculado), enumCriteriosDeFiltrado.igual, idVinculado);
            var f3 = new ClausulaDeFiltrado(nameof(AccionesDeRelacionDtm.Momento), enumCriteriosDeFiltrado.igual, momento.ToString());
            return new List<ClausulaDeFiltrado> { f1, f2, f3 };
        }

    }

}
