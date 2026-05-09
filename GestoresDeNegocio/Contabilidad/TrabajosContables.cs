using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.TrabajosSometidos;
using ModeloDeDto;
using ModeloDeDto.Contabilidad;
using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.TrabajosSometidos;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using Utilidades;

namespace GestoresDeNegocio.Contabilidad
{
    public enum enumTrabajosContables
    {
        [Description("Crear lote contable")]
        CrearLoteContable,
        [Description("Crear lote de terceros")]
        CrearLoteDeTerceros,
        [Description("Anular lote contable")]
        AnularLoteContable,
        [Description("Anular estimacion directa")]
        AnularEstimacionDirecta
    }

    public class TrabajosContables
    {
        public static TrabajoDeUsuarioDtm SometerCrearLoteContable(ContextoSe contexto, int idSociedad, int ejercicio, bool descontabilizar, bool respetarFechaContable, DateTime? fechaContable, string filtros)
        {
            if (ejercicio > DateTime.Now.Year)
                GestorDeErrores.Emitir($"El ejercicio '{ejercicio}' no puede ser mayor que el ejercicio actual.");
            if (ejercicio + 1 < DateTime.Now.Year)
                GestorDeErrores.Emitir($"El ejercicio '{ejercicio}' no puede ser menor dos años que el ejercicio actual.");

            var dll = Assembly.GetExecutingAssembly().GetName().Name;
            var clase = typeof(TrabajosContables).FullName;
            var ts = GestorDeTrabajosSometido.CrearObtener(contexto, enumTrabajosContables.CrearLoteContable.Descripcion(), dll, clase, nameof(enumTrabajosContables.CrearLoteContable), comunicarFin: true);

            var parametrosEntrada = new Dictionary<string, object> {
                { nameof(SociedadDtm.Id), idSociedad },
                { nameof(CrearLoteContableDto.Ejercicio), ejercicio },
                { nameof(CrearLoteContableDto.Descontabilizar), descontabilizar },
                { nameof(CrearLoteContableDto.RespetarFechaContable),respetarFechaContable },
                { nameof(PreasientoDtm.FechaContable), fechaContable },
                { nameof(ltrFiltros.filtro), filtros }
            };
            var datosDeCreacion = new Dictionary<string, object>
            {
                    { nameof(TrabajoDeUsuarioDtm.Parametros), parametrosEntrada.ToJson() }
            };
            return GestorDeTrabajosDeUsuario.CrearSiNoEstaPendiente(contexto, ts, datosDeCreacion);
        }

        public static TrabajoDeUsuarioDtm SometerAnularLoteContable(ContextoSe contexto, int idCircuito)
        {
            var circuito = contexto.SeleccionarPorId<CircuitoDocDtm>(idCircuito);
            if (!circuito.EsLoteDePreasientos())
                GestorDeErrores.Emitir($"El trabajo de '{enumTrabajosContables.AnularLoteContable.Descripcion()}' solo es válido para el tipo definido en el parámetro '{enumParametrosDeCircuitosDoc.CAD_Tipo_Para_Lote_de_Preasientos}'");

            var dll = Assembly.GetExecutingAssembly().GetName().Name;
            var clase = typeof(TrabajosContables).FullName;
            var ts = GestorDeTrabajosSometido.CrearObtener(contexto, enumTrabajosContables.AnularLoteContable.Descripcion(), dll, clase, nameof(enumTrabajosContables.AnularLoteContable), comunicarFin: true);

            var parametrosEntrada = new Dictionary<string, object> {
                { nameof(CircuitoDocDtm.Id), idCircuito }
            };
            var datosDeCreacion = new Dictionary<string, object>
            {
                    { nameof(TrabajoDeUsuarioDtm.Parametros), parametrosEntrada.ToJson() }
            };
            return GestorDeTrabajosDeUsuario.CrearSiNoEstaPendiente(contexto, ts, datosDeCreacion);
        }

        public static TrabajoDeUsuarioDtm SometerAnularEstimacionDirecta(ContextoSe contexto, int idCircuito)
        {
            var circuito = contexto.SeleccionarPorId<CircuitoDocDtm>(idCircuito);
            if (!circuito.EsEstimacionDirecta())
                GestorDeErrores.Emitir($"El trabajo de '{enumTrabajosContables.AnularEstimacionDirecta.Descripcion()}' solo es válido para el tipo definido en el parámetro '{enumParametrosDeCircuitosDoc.CAD_Tipo_Para_Estimacion_Directa}'");

            var dll = Assembly.GetExecutingAssembly().GetName().Name;
            var clase = typeof(TrabajosContables).FullName;
            var ts = GestorDeTrabajosSometido.CrearObtener(contexto, enumTrabajosContables.AnularEstimacionDirecta.Descripcion(), dll, clase, nameof(enumTrabajosContables.AnularEstimacionDirecta), comunicarFin: true);

            var parametrosEntrada = new Dictionary<string, object> {
                { nameof(CircuitoDocDtm.Id), idCircuito }
            };
            var datosDeCreacion = new Dictionary<string, object>
            {
                    { nameof(TrabajoDeUsuarioDtm.Parametros), parametrosEntrada.ToJson() }
            };
            return GestorDeTrabajosDeUsuario.CrearSiNoEstaPendiente(contexto, ts, datosDeCreacion);
        }

        public static TrabajoDeUsuarioDtm SometerCrearLoteDeTerceros(ContextoSe contexto, int idSociedad)
        {
            var dll = Assembly.GetExecutingAssembly().GetName().Name;
            var clase = typeof(TrabajosContables).FullName;
            var ts = GestorDeTrabajosSometido.CrearObtener(contexto, enumTrabajosContables.CrearLoteDeTerceros.Descripcion(), dll, clase, nameof(enumTrabajosContables.CrearLoteDeTerceros), comunicarFin: true);

            var parametrosEntrada = new Dictionary<string, object> {
                { nameof(CircuitoDocDtm.Id), idSociedad }
            };
            var datosDeCreacion = new Dictionary<string, object>
            {
                    { nameof(TrabajoDeUsuarioDtm.Parametros), parametrosEntrada.ToJson() }
            };
            return GestorDeTrabajosDeUsuario.CrearSiNoEstaPendiente(contexto, ts, datosDeCreacion);
        }

        public static void CrearLoteContable(EntornoDeTrabajo entorno)
        {
            var contexto = entorno.contextoDelProceso;
            Dictionary<string, object> parametros = entorno.TrabajoDeUsuario.Parametros.ToDiccionarioDeParametros();
            contexto.IniciarTraza(nameof(CrearLoteContable));
            var trazaInfDtm = entorno.CrearTraza($"Traza informativa del proceso");
            var tran = contexto.IniciarTransaccion();
            try
            {
                var idSociedad = Convert.ToInt32(parametros.LeerValor<long>(nameof(SociedadDtm.Id)));
                var sociedad = contexto.SeleccionarPorId<SociedadDtm>(idSociedad);
                var comoContabilizar = sociedad.ContabilizarEn();
                var metodo = ApiDeEnsamblados.MetodoEstatico(ApiDeEnsamblados.GestoresDeNegocio, typeof(ExportacionesDePreasientos).FullName, comoContabilizar.Metodo);
                if (metodo != null)
                {
                    parametros.Add(nameof(EntornoDeTrabajo), entorno);
                    var contabilizar = new EntornoDeUnaAccion(contexto, enumNegocio.Preasiento, parametros);

                    contabilizar.Contexto.Accion = new AccionDtm
                    {
                        Dll = ApiDeEnsamblados.GestoresDeNegocio,
                        Clase = typeof(ExportacionesDePreasientos).FullName,
                        Metodo = comoContabilizar.Metodo,
                        Nombre = comoContabilizar.Nombre,
                        ClaseDeAccion = enumClaseDeAccion.DLL.ToString()
                    };

                    try
                    {
                        EntornoDeUnaAccion.EjecutarAccion(contabilizar);
                    }
                    finally
                    {
                        contabilizar.Contexto.Accion = null;
                    }
                }
                else GestorDeErrores.Emitir($"No está definido el método {metodo} en la clase {typeof(ExportacionesDePreasientos).FullName}");

                contexto.Commit(tran);
            }
            catch (Exception e)
            {
                contexto.Rollback(tran);
                entorno.AnotarError(e);
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }


        public static void AnularLoteContable(EntornoDeTrabajo entorno)
        {
            var contexto = entorno.contextoDelProceso;

            Dictionary<string, object> parametros = entorno.TrabajoDeUsuario.Parametros.ToDiccionarioDeParametros();
            var idCircuito = (int)parametros.LeerValor<long>(nameof(CircuitoDocDtm.Id));
            var circuito = contexto.SeleccionarPorId<CircuitoDocDtm>(idCircuito);

            var otorgado = contexto.Usuario.OtorgarAdministrador(contexto);
            var idSemaforo = SemaforoDeProcesoSql.PonerSemaforo(entorno.ContextoDelEntorno, entorno.TrabajoSometido.Id, circuito.Id, enumOpercionesDeSemaforo.ALCT, circuito.Referencia).Id;
            contexto.IniciarTraza(nameof(AnularLoteContable));
            var tran = contexto.IniciarTransaccion();
            try
            {
                entorno.CrearTraza($"Inicio del proceso");
                ExtensorDePreasientos.AnularLoteContable(circuito, contexto, 0);
                contexto.Commit(tran);
            }
            catch (Exception e)
            {
                contexto.Rollback(tran);
                entorno.AnotarError(e);
            }
            finally
            {
                if (otorgado) contexto.Usuario.AnularAdministrador(contexto, otorgado);
                SemaforoDeProcesoSql.QuitarSemaforo(entorno.ContextoDelEntorno, idSemaforo);
                contexto.CerrarTraza();
            }
        }

        public static void AnularEstimacionDirecta(EntornoDeTrabajo entorno)
        {
            var contexto = entorno.contextoDelProceso;

            Dictionary<string, object> parametros = entorno.TrabajoDeUsuario.Parametros.ToDiccionarioDeParametros();
            var idCircuito = (int)parametros.LeerValor<long>(nameof(CircuitoDocDtm.Id));
            var circuito = contexto.SeleccionarPorId<CircuitoDocDtm>(idCircuito);

            var otorgado = contexto.Usuario.OtorgarAdministrador(contexto);
            var idSemaforo = SemaforoDeProcesoSql.PonerSemaforo(entorno.ContextoDelEntorno, entorno.TrabajoSometido.Id, circuito.Id, enumOpercionesDeSemaforo.AETD, circuito.Referencia).Id;
            contexto.IniciarTraza(nameof(AnularEstimacionDirecta));
            var tran = contexto.IniciarTransaccion();
            try
            {
                entorno.CrearTraza($"Inicio del proceso");
                ExtensorDePreasientos.AnularEstimacionDirecta(circuito, contexto, 0);
                contexto.Commit(tran);
            }
            catch (Exception e)
            {
                contexto.Rollback(tran);
                entorno.AnotarError(e);
            }
            finally
            {
                if (otorgado) contexto.Usuario.AnularAdministrador(contexto, otorgado);
                SemaforoDeProcesoSql.QuitarSemaforo(entorno.ContextoDelEntorno, idSemaforo);
                contexto.CerrarTraza();
            }
        }


        public static void CrearLoteDeTerceros(EntornoDeTrabajo entorno)
        {
            var contexto = entorno.contextoDelProceso;
            Dictionary<string, object> parametros = entorno.TrabajoDeUsuario.Parametros.ToDiccionarioDeParametros();
            contexto.IniciarTraza(nameof(CrearLoteDeTerceros));
            var trazaInfDtm = entorno.CrearTraza($"Traza informativa del proceso");
            var tran = contexto.IniciarTransaccion();
            try
            {
                var idSociedad = Convert.ToInt32(parametros.LeerValor<long>(nameof(SociedadDtm.Id)));
                var sociedad = contexto.SeleccionarPorId<SociedadDtm>(idSociedad);
                var comoContabilizar = sociedad.CuentasDeTerceros();
                var metodo = ApiDeEnsamblados.MetodoEstatico(ApiDeEnsamblados.GestoresDeNegocio, typeof(ExportacionesDePreasientos).FullName, comoContabilizar.Metodo);
                if (metodo != null)
                {
                    parametros.Add(nameof(EntornoDeTrabajo), entorno);
                    var contabilizar = new EntornoDeUnaAccion(contexto, enumNegocio.Preasiento, parametros);

                    contabilizar.Contexto.Accion = new AccionDtm
                    {
                        Dll = ApiDeEnsamblados.GestoresDeNegocio,
                        Clase = typeof(ExportacionesDePreasientos).FullName,
                        Metodo = comoContabilizar.Metodo,
                        Nombre = comoContabilizar.Nombre,
                        ClaseDeAccion = enumClaseDeAccion.DLL.ToString()
                    };

                    try
                    {
                        EntornoDeUnaAccion.EjecutarAccion(contabilizar);
                    }
                    finally
                    {
                        contabilizar.Contexto.Accion = null;
                    }
                }
                else GestorDeErrores.Emitir($"No está definido el método {metodo} en la clase {typeof(ExportacionesDePreasientos).FullName}");

                contexto.Commit(tran);
            }
            catch (Exception e)
            {
                contexto.Rollback(tran);
                entorno.AnotarError(e);
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }

    }
}
