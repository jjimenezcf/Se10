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
        AnularEstimacionDirecta,
        [Description("Crear cierre contable")]
        CrearCierreContable,
        [Description("Eliminar cierre contable")]
        EliminarCierreContable
    }

    public class TrabajosContables
    {
        public static TrabajoDeUsuarioDtm SometerCrearLoteContable(ContextoSe contexto, int idSociedad, int ejercicio, bool descontabilizar, bool respetarFechaContable, DateTime? fechaContable, string filtros)
        {
            return SometerLoteContable(contexto, enumTrabajosContables.CrearLoteContable, idSociedad, ejercicio, descontabilizar, respetarFechaContable, fechaContable, filtros);
        }

        // El cierre crea el lote con los preasientos pendientes de la sociedad hasta la fecha indicada, respetando su fecha contable y sin
        // descontabilizar; el ejercicio que se cierra es el de esa fecha
        public static TrabajoDeUsuarioDtm SometerCrearCierreContable(ContextoSe contexto, int idSociedad, DateTime cerrarHasta, enumSistemaContable sistema, int digitos)
        {
            var ejercicio = cerrarHasta.Year;
            ValidarRangoDelEjercicio(ejercicio);
            var sociedad = contexto.SeleccionarPorId<SociedadDtm>(idSociedad);
            if (!sociedad.EsInterventor(contexto))
                GestorDeErrores.Emitir($"No puede cerrar la sociedad '{sociedad.Expresion}' por no ser interventor de la misma");

            var fechaNoValida = GestorDeCierresContables.MotivoFechaDeCierreNoValida(contexto, sociedad, cerrarHasta);
            if (fechaNoValida is not null)
                GestorDeErrores.Emitir(fechaNoValida);

            var sinPreasientos = GestorDeCierresContables.MotivoSinPreasientosParaTraspasar(contexto, sociedad, cerrarHasta);
            if (sinPreasientos is not null)
                GestorDeErrores.Emitir(sinPreasientos);

            GestorDeCierresContables.ValidarEjercicioDelCierre(contexto, sociedad, ejercicio);
            GestorDeCierresContables.CambiarSistemaContable(contexto, sociedad, sistema, digitos);

            return SometerLoteContable(contexto, enumTrabajosContables.CrearCierreContable, idSociedad, ejercicio, descontabilizar: false, respetarFechaContable: true, fechaContable: null, filtros: "", cerrarHasta: cerrarHasta.Date);
        }

        private static void ValidarRangoDelEjercicio(int ejercicio)
        {
            if (ejercicio > DateTime.Now.Year)
                GestorDeErrores.Emitir($"El ejercicio '{ejercicio}' no puede ser mayor que el ejercicio actual.");
            if (ejercicio + 1 < DateTime.Now.Year)
                GestorDeErrores.Emitir($"El ejercicio '{ejercicio}' no puede ser menor dos años que el ejercicio actual.");
        }

        private static TrabajoDeUsuarioDtm SometerLoteContable(ContextoSe contexto, enumTrabajosContables trabajo, int idSociedad, int ejercicio, bool descontabilizar, bool respetarFechaContable, DateTime? fechaContable, string filtros, DateTime? cerrarHasta = null)
        {
            ValidarRangoDelEjercicio(ejercicio);

            var dll = Assembly.GetExecutingAssembly().GetName().Name;
            var clase = typeof(TrabajosContables).FullName;
            var ts = GestorDeTrabajosSometido.CrearObtener(contexto, trabajo.Descripcion(), dll, clase, trabajo.ToString(), comunicarFin: true);

            var parametrosEntrada = new Dictionary<string, object> {
                { nameof(SociedadDtm.Id), idSociedad },
                { nameof(CrearLoteContableDto.Ejercicio), ejercicio },
                { nameof(CrearLoteContableDto.Descontabilizar), descontabilizar },
                { nameof(CrearLoteContableDto.RespetarFechaContable),respetarFechaContable },
                { nameof(PreasientoDtm.FechaContable), fechaContable },
                { nameof(ltrFiltros.filtro), filtros }
            };
            // solo en los cierres: limita los preasientos del lote y es la fecha con la que se registra el cierre
            if (cerrarHasta is not null)
                parametrosEntrada.Add(nameof(CrearCierreContableDto.CerrarHasta), cerrarHasta);

            var datosDeCreacion = new Dictionary<string, object>
            {
                    { nameof(TrabajoDeUsuarioDtm.Parametros), parametrosEntrada.ToJson() }
            };
            return GestorDeTrabajosDeUsuario.CrearSiNoEstaPendiente(contexto, ts, datosDeCreacion);
        }

        public static TrabajoDeUsuarioDtm SometerAnularLoteContable(ContextoSe contexto, int idCircuito)
        {
            return SometerAnulacionDelLote(contexto, enumTrabajosContables.AnularLoteContable, idCircuito);
        }

        // El cierre se elimina anulando su lote contable
        public static TrabajoDeUsuarioDtm SometerEliminarCierreContable(ContextoSe contexto, int idCierre)
        {
            var cierre = contexto.SeleccionarPorId<CierreContableDtm>(idCierre);
            return SometerAnulacionDelLote(contexto, enumTrabajosContables.EliminarCierreContable, cierre.IdLote);
        }

        private static TrabajoDeUsuarioDtm SometerAnulacionDelLote(ContextoSe contexto, enumTrabajosContables trabajo, int idCircuito)
        {
            var circuito = contexto.SeleccionarPorId<CircuitoDocDtm>(idCircuito);
            if (!circuito.EsLoteDePreasientos())
                GestorDeErrores.Emitir($"El trabajo de '{trabajo.Descripcion()}' solo es válido para el tipo definido en el parámetro '{enumParametrosDeCircuitosDoc.CAD_Tipo_Para_Lote_de_Preasientos}'");

            var motivo = GestorDeCierresContables.MotivoParaNoAnularElCierre(contexto, idCircuito);
            if (motivo is not null)
                GestorDeErrores.Emitir(motivo);

            var dll = Assembly.GetExecutingAssembly().GetName().Name;
            var clase = typeof(TrabajosContables).FullName;
            var ts = GestorDeTrabajosSometido.CrearObtener(contexto, trabajo.Descripcion(), dll, clase, trabajo.ToString(), comunicarFin: true);

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

        // Antes de crear el lote, ningún preasiento pendiente puede tener fecha contable anterior al último cierre de la sociedad (no debería pasar, se anota en la traza)
        public static void CrearCierreContable(EntornoDeTrabajo entorno)
        {
            var contexto = entorno.contextoDelProceso;
            Dictionary<string, object> parametros = entorno.TrabajoDeUsuario.Parametros.ToDiccionarioDeParametros();
            var idSociedad = Convert.ToInt32(parametros.LeerValor<long>(nameof(SociedadDtm.Id)));
            var cerrarHasta = parametros.LeerValor<DateTime?>(nameof(CrearCierreContableDto.CerrarHasta), null);
            var sociedad = contexto.SeleccionarPorId<SociedadDtm>(idSociedad);

            if (cerrarHasta is null)
            {
                entorno.CrearTraza($"No se ha indicado la fecha hasta la que se cierra la sociedad '{sociedad.Expresion}'");
                return;
            }

            // entre el sometimiento y la ejecución se ha podido realizar otro cierre de la sociedad
            var fechaNoValida = GestorDeCierresContables.MotivoFechaDeCierreNoValida(contexto, sociedad, (DateTime)cerrarHasta);
            if (fechaNoValida is not null)
            {
                entorno.CrearTraza(fechaNoValida);
                return;
            }

            // lo primero es ver si hay algo que traspasar: si no, se anota y se termina el trabajo
            var sinPreasientos = GestorDeCierresContables.MotivoSinPreasientosParaTraspasar(contexto, sociedad, (DateTime)cerrarHasta);
            if (sinPreasientos is not null)
            {
                entorno.CrearTraza(sinPreasientos);
                return;
            }

            // el lote toma los preasientos con fecha contable desde el último cierre (incluida) hasta la fecha de cierre (no incluida)
            parametros[nameof(CierreContableDto.Desde)] = GestorDeCierresContables.FechaDelUltimoCierre(contexto, idSociedad);

            // el ajuste de fechas y la creación del lote van en la misma transacción: si algo falla se deshace todo
            contexto.IniciarTraza(nameof(CrearCierreContable));
            entorno.CrearTraza($"Traza informativa del proceso");
            var tran = contexto.IniciarTransaccion();
            try
            {
                GestorDeCierresContables.AjustarPreasientosAnterioresAlUltimoCierre(contexto, idSociedad, mensaje => entorno.CrearTraza(mensaje));
                GenerarLoteContable(entorno, contexto, parametros);
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

        public static void CrearLoteContable(EntornoDeTrabajo entorno)
        {
            var contexto = entorno.contextoDelProceso;
            Dictionary<string, object> parametros = entorno.TrabajoDeUsuario.Parametros.ToDiccionarioDeParametros();
            contexto.IniciarTraza(nameof(CrearLoteContable));
            var trazaInfDtm = entorno.CrearTraza($"Traza informativa del proceso");
            var tran = contexto.IniciarTransaccion();
            try
            {
                GenerarLoteContable(entorno, contexto, parametros);
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

        // Genera el lote contable de la sociedad con el sistema contable configurado y registra el cierre; no gestiona la transacción ni
        // captura los errores, lo hace quien lo llama (CrearLoteContable o CrearCierreContable)
        private static void GenerarLoteContable(EntornoDeTrabajo entorno, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            var idSociedad = Convert.ToInt32(parametros.LeerValor<long>(nameof(SociedadDtm.Id)));
            var sociedad = contexto.SeleccionarPorId<SociedadDtm>(idSociedad);
            var comoContabilizar = sociedad.ContabilizarEn();
            var metodo = ApiDeEnsamblados.MetodoEstatico(ApiDeEnsamblados.GestoresDeNegocio, typeof(ExportacionesDePreasientos).FullName, comoContabilizar.Metodo);
            if (metodo == null)
                GestorDeErrores.Emitir($"No está definido el método {comoContabilizar.Metodo} en la clase {typeof(ExportacionesDePreasientos).FullName}");

            parametros[nameof(EntornoDeTrabajo)] = entorno;
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
                var salida = EntornoDeUnaAccion.EjecutarAccion(contabilizar);
                var idLote = salida.LeerValor(nameof(CierreContableDtm.IdLote), 0);
                // en un cierre se registra la fecha hasta la que se cierra; en un lote contable normal, la del momento en que se crea
                var cerradaEl = parametros.LeerValor<DateTime?>(nameof(CrearCierreContableDto.CerrarHasta), null)?.Date ?? DateTime.Now;
                if (idLote > 0)
                    GestorDeCierresContables.RegistrarCierre(contexto, sociedad, contexto.SeleccionarPorId<CircuitoDocDtm>(idLote), cerradaEl);
            }
            finally
            {
                contabilizar.Contexto.Accion = null;
            }
        }


        public static void AnularLoteContable(EntornoDeTrabajo entorno)
        {
            var contexto = entorno.contextoDelProceso;

            Dictionary<string, object> parametros = entorno.TrabajoDeUsuario.Parametros.ToDiccionarioDeParametros();
            var idCircuito = (int)parametros.LeerValor<long>(nameof(CircuitoDocDtm.Id));
            var circuito = contexto.SeleccionarPorId<CircuitoDocDtm>(idCircuito);

            // entre el sometimiento y la ejecución se ha podido crear un cierre posterior de la sociedad
            var motivo = GestorDeCierresContables.MotivoParaNoAnularElCierre(contexto, idCircuito);
            if (motivo is not null)
            {
                entorno.CrearTraza(motivo);
                return;
            }

            var otorgado = contexto.Usuario.OtorgarAdministrador(contexto);
            var idSemaforo = SemaforoDeProcesoSql.PonerSemaforo(entorno.ContextoDelEntorno, entorno.TrabajoSometido.Id, circuito.Id, enumOpercionesDeSemaforo.ALCT, circuito.Referencia).Id;
            contexto.IniciarTraza(nameof(AnularLoteContable));
            var tran = contexto.IniciarTransaccion();
            try
            {
                AnularElLoteContable(entorno, contexto, circuito);
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

        // Elimina el cierre contable anulando su lote; de momento hace lo mismo que AnularLoteContable, pero permite añadir
        // funcionalidad propia del cierre dentro de la misma transacción
        public static void EliminarCierreContable(EntornoDeTrabajo entorno)
        {
            var contexto = entorno.contextoDelProceso;

            Dictionary<string, object> parametros = entorno.TrabajoDeUsuario.Parametros.ToDiccionarioDeParametros();
            var idCircuito = (int)parametros.LeerValor<long>(nameof(CircuitoDocDtm.Id));
            var circuito = contexto.SeleccionarPorId<CircuitoDocDtm>(idCircuito);

            // entre el sometimiento y la ejecución se ha podido crear un cierre posterior de la sociedad
            var motivo = GestorDeCierresContables.MotivoParaNoAnularElCierre(contexto, idCircuito);
            if (motivo is not null)
            {
                entorno.CrearTraza(motivo);
                return;
            }

            var otorgado = contexto.Usuario.OtorgarAdministrador(contexto);
            var idSemaforo = SemaforoDeProcesoSql.PonerSemaforo(entorno.ContextoDelEntorno, entorno.TrabajoSometido.Id, circuito.Id, enumOpercionesDeSemaforo.ALCT, circuito.Referencia).Id;
            contexto.IniciarTraza(nameof(EliminarCierreContable));
            var tran = contexto.IniciarTransaccion();
            try
            {
                AnularElLoteContable(entorno, contexto, circuito);
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

        // Anula el lote contable y elimina su cierre; no gestiona la transacción ni captura los errores, lo hace quien lo llama
        // (AnularLoteContable o EliminarCierreContable)
        private static void AnularElLoteContable(EntornoDeTrabajo entorno, ContextoSe contexto, CircuitoDocDtm circuito)
        {
            entorno.CrearTraza($"Inicio del proceso");
            ExtensorDePreasientos.AnularLoteContable(circuito, contexto, 0);
            GestorDeCierresContables.EliminarCierre(contexto, circuito);
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
