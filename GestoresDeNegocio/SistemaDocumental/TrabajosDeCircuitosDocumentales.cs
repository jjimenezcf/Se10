using GestoresDeNegocio.TrabajosSometidos;
using ServicioDeDatos.TrabajosSometidos;
using ServicioDeDatos;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using ServicioDeDatos.SistemaDocumental;
using System.Linq;
using Utilidades;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.Gastos;

namespace GestoresDeNegocio.SistemaDocumental
{
    public enum enumTrabajosDeCircuitosDocumentales
    {
        [Description("Fichar de entradas")]
        FicharEntrada,
        [Description("Fichar salida")]
        FicharSalida,
        [Description("Crear histórico de fichadas")]
        CrearHistoricoDeFichadas,
        [Description("Cierra las fichadas abiertas")]
        CerrarFichadasAbiertas,
        [Description("Cierra las fichadas abiertas anteriores a hoy")]
        CerrarFichadasAntiguas
    }

    public class TrabajosDeCircuitosDocumentales
    {

        public static void TrabajoDeCrearHistoricoDeFichadas(ContextoSe contexto)
        {
            var dll = Assembly.GetExecutingAssembly().GetName().Name;
            var clase = typeof(TrabajosDeCircuitosDocumentales).FullName;
            var ts = GestorDeTrabajosSometido.CrearObtener(contexto, enumTrabajosDeCircuitosDocumentales.CrearHistoricoDeFichadas.Descripcion(), dll, clase,
                nameof(enumTrabajosDeCircuitosDocumentales.CrearHistoricoDeFichadas), comunicarFin: false);
        }

        public static TrabajoDeUsuarioDtm SometerFicharEntrada(ContextoSe contexto)
        {
            var dll = Assembly.GetExecutingAssembly().GetName().Name;
            var clase = typeof(TrabajosDeCircuitosDocumentales).FullName;
            var ts = GestorDeTrabajosSometido.CrearObtener(contexto, enumTrabajosDeCircuitosDocumentales.FicharEntrada.Descripcion(), dll, clase,
                nameof(enumTrabajosDeCircuitosDocumentales.FicharEntrada), comunicarFin: false);
            var datosDeCreacion = new Dictionary<string, object>
            {
                { nameof(TrabajoDeUsuarioDtm.Planificado), DateTime.Today.AddHours(8) },
                { nameof(TrabajoDeUsuarioDtm.Periodicidad), 86400}
            };
            return GestorDeTrabajosDeUsuario.CrearSiNoEstaPendiente(contexto, ts, datosDeCreacion);
        }

        public static TrabajoDeUsuarioDtm SometerFicharSalida(ContextoSe contexto, DateTime salida, CircuitoDocDtm circuito, int idTransicion)
        {
            var dll = Assembly.GetExecutingAssembly().GetName().Name;
            var clase = typeof(TrabajosDeCircuitosDocumentales).FullName;
            var ts = GestorDeTrabajosSometido.CrearObtener(contexto, enumTrabajosDeCircuitosDocumentales.FicharSalida.Descripcion(), dll, clase,
                nameof(enumTrabajosDeCircuitosDocumentales.FicharSalida), comunicarFin: false);

            var circuitoCad = new List<Parametro> { new Parametro(nameof(IRegistro.Id), circuito.Id), new Parametro(nameof(MetadatosDeFichada.IdTransicion), idTransicion) }.ToJson();

            var datosDeCreacion = new Dictionary<string, object>
            {
                { nameof(TrabajoDeUsuarioDtm.Planificado), salida },
                { nameof(TrabajoDeUsuarioDtm.Parametros), circuitoCad }
            };
            return GestorDeTrabajosDeUsuario.CrearSiNoEstaPendiente(contexto, ts, datosDeCreacion);
        }

        public static TrabajoDeUsuarioDtm SometerCerrarFichadasAbiertas(ContextoSe contexto)
        {
            var dll = Assembly.GetExecutingAssembly().GetName().Name;
            var clase = typeof(TrabajosDeCircuitosDocumentales).FullName;
            var ts = GestorDeTrabajosSometido.CrearObtener(contexto, enumTrabajosDeCircuitosDocumentales.CerrarFichadasAbiertas.Descripcion(), dll, clase,
                nameof(enumTrabajosDeCircuitosDocumentales.CerrarFichadasAbiertas), comunicarFin: false);

            var datosDeCreacion = new Dictionary<string, object>
            {
                { nameof(TrabajoDeUsuarioDtm.Planificado), DateTime.Today.AddHours(8) },
                { nameof(TrabajoDeUsuarioDtm.Periodicidad), 86400}
            };
            return GestorDeTrabajosDeUsuario.CrearSiNoEstaPendiente(contexto, ts, datosDeCreacion);
        }

        public static void SometerCerrarFichadasAntiguas(ContextoSe contexto)
        {
            var dll = Assembly.GetExecutingAssembly().GetName().Name;
            var clase = typeof(TrabajosDeCircuitosDocumentales).FullName;
            var ts = GestorDeTrabajosSometido.CrearObtener(contexto, enumTrabajosDeCircuitosDocumentales.CerrarFichadasAntiguas.Descripcion(), dll, clase,
                nameof(enumTrabajosDeCircuitosDocumentales.CerrarFichadasAntiguas), comunicarFin: false);
        }

        public static void CrearHistoricoDeFichadas(EntornoDeTrabajo entorno)
        {
            var contexto = entorno.contextoDelProceso;
            contexto.IniciarTraza(nameof(CrearHistoricoDeFichadas));
            entorno.CrearTraza($"Inicio del proceso");
            var tran = contexto.IniciarTransaccion();
            var otorgado = contexto.Usuario.OtorgarAdministrador(contexto);
            try
            {

                DateTime fechaInicio = new DateTime(DateTime.Now.Year, 1, 1);
                DateTime fechaFin = DateTime.Today.AddDays(-1);

                var metadatos = VariablesDeCircuitosDoc.ObtenerMetadatosDeFichadas(contexto, errorSiNoDefinido: false);

                if (metadatos is null)
                {
                    entorno.CrearTraza(string.Format(ltrDeUnaEstimacion.Mensaje_FaltaConfigurarParametro, enumParametrosDeCircuitosDoc.CAD_ParametrosParaFichadaAutomatica.ToString()));
                }
                else for (DateTime dia = fechaInicio; dia <= fechaFin; dia = dia.AddDays(1))
                    {
                        bool esFinDeSemana = dia.DayOfWeek == DayOfWeek.Saturday || dia.DayOfWeek == DayOfWeek.Sunday;
                        if (!esFinDeSemana)
                        {
                            IndicarEntradas(entorno, contexto, metadatos, dia, someterSalida: false);
                        }
                    }

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
                contexto.Usuario.AnularAdministrador(contexto, otorgado);
            }
        }

        public static void FicharEntrada(EntornoDeTrabajo entorno)
        {
            var contexto = entorno.contextoDelProceso;
            contexto.IniciarTraza(nameof(FicharEntrada));
            entorno.CrearTraza($"Inicio del proceso");
            var tran = contexto.IniciarTransaccion();
            var otorgado = contexto.Usuario.OtorgarAdministrador(contexto);
            try
            {
                var metadatos = VariablesDeCircuitosDoc.ObtenerMetadatosDeFichadas(contexto);

                if (metadatos is null)
                {
                    entorno.CrearTraza(string.Format(ltrDeUnaEstimacion.Mensaje_FaltaConfigurarParametro, enumParametrosDeCircuitosDoc.CAD_ParametrosParaFichadaAutomatica.ToString()));
                }
                else
                {
                    var dia = DateTime.Today;
                    bool esFinDeSemana = dia.DayOfWeek == DayOfWeek.Saturday || dia.DayOfWeek == DayOfWeek.Sunday;
                    if (!esFinDeSemana)
                    {
                        IndicarEntradas(entorno, contexto, metadatos, dia, someterSalida: true);
                    }
                }
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
                contexto.Usuario.AnularAdministrador(contexto, otorgado);
            }
        }

        private static void IndicarEntradas(EntornoDeTrabajo entorno, ContextoSe contexto, List<MetadatosDeFichada> metadatos, DateTime dia, bool someterSalida)
        {
            foreach (MetadatosDeFichada item in metadatos)
            {
                var excluido = item.PeriodosExcluidos.Where(periodo => dia.Month >= periodo.MesDesde && dia.Day >= periodo.DiaDesde && dia.Month <= periodo.MesHasta && dia.Day <= periodo.DiaHasta).FirstOrDefault();
                if (excluido != null)
                {
                    entorno.CrearTraza($"El día de hoy '{dia.ToString("dd-MM-yyyy")}' para '{item.Trabajador(contexto).Nombre}' a sido excluido por este periodo '{excluido.MesDesde}:{excluido.DiaDesde};{excluido.MesHasta}:{excluido.DiaHasta}'");
                    continue;
                }

                if (item.PeriodosIncluidos.Count > 0)
                {
                    var incluido = item.PeriodosIncluidos.Where(periodo => dia.Month >= periodo.MesDesde && dia.Day >= periodo.DiaDesde && dia.Month <= periodo.MesHasta && dia.Day <= periodo.DiaHasta).FirstOrDefault();
                    if (incluido == null)
                    {
                        entorno.CrearTraza($"El día '{dia.ToString("dd-MM-yyyy")}' para '{item.Trabajador(contexto).Nombre}' no está incluido en ningún periodo de los definidos");
                        continue;
                    }
                }

                var fichada = contexto.Set<CircuitoDocDtm>().Where(circuito => circuito.IdCg == item.IdCg && circuito.IdTipo == item.IdTipo &&
                circuito.Nombre == item.Trabajador(contexto).Nombre &&
                circuito.FechaCreacion > dia &&
                circuito.FechaCreacion < dia.AddDays(1) &&
                contexto.Set<EstadoDeUnCircuitoDocDtm>().Any(e => !e.Cancelado && e.Id == circuito.IdEstado)).FirstOrDefault();
                if (fichada != null)
                {
                    entorno.CrearTraza($"Ya hay una fichada para el día '{dia.ToString("dd-MM-yyyy")}' a nombre '{item.Trabajador(contexto).Nombre}' de , referencia '{fichada.Referencia}'");
                    continue;
                }

                Random random = new Random();
                var entrada = dia.AddHours(item.Entrada.Split(Simbolos.separadorDePartesHorarias)[0].Entero())
                                 .AddMinutes(item.Entrada.Split(Simbolos.separadorDePartesHorarias)[1].Entero());
                entrada = entrada.AddMinutes(random.Next(-5, 6));

                var salida = dia.AddHours(item.Salida.Split(Simbolos.separadorDePartesHorarias)[0].Entero())
                                .AddMinutes(item.Salida.Split(Simbolos.separadorDePartesHorarias)[1].Entero());
                salida = salida.AddMinutes(random.Next(-5, 6));

                if (entrada >= salida)
                    entorno.CrearTraza($"Están mal indicadas las horas de entrada y salida para '{item.Trabajador(contexto).Nombre}'");

                var circuito = new CircuitoDocDtm
                {
                    IdCg = item.IdCg,
                    IdTipo = item.IdTipo,
                    Nombre = item.Trabajador(contexto).Nombre,
                }.Insertar(contexto, parametros: new Dictionary<string, object> { { ltrParametrosNeg.FechaDeCreacion, entrada } });

                GestorDeVinculos.Vincular(contexto, item.Trabajador(contexto), circuito);

                if (someterSalida)
                {
                    SometerFicharSalida(contexto, salida, circuito, item.IdTransicion);
                }
                else
                {
                    circuito.Transitar(contexto, item.IdTransicion, new Dictionary<string, object> { { ltrParametrosNeg.FechaDeTransicion, salida } });
                    entorno.CrearTraza($"Fichada creada para '{item.Trabajador(contexto).Nombre}', entrada a las  '{entrada.ToString()}' salida a las '{salida.ToString()}'");
                }
            }
        }

        public static void FicharSalida(EntornoDeTrabajo entorno)
        {
            var contexto = entorno.contextoDelProceso;
            Dictionary<string, object> parametros = entorno.TrabajoDeUsuario.Parametros.ToDiccionarioDeParametros();
            var idCircuito = Convert.ToInt32(parametros.LeerValor<long>(nameof(IRegistro.Id)));
            var idTransicion = Convert.ToInt32(parametros.LeerValor<long>(nameof(MetadatosDeFichada.IdTransicion)));
            var circuito = contexto.SeleccionarPorId<CircuitoDocDtm>(idCircuito);
            contexto.IniciarTraza(nameof(FicharSalida));
            entorno.CrearTraza($"Inicio del proceso");
            var tran = contexto.IniciarTransaccion();
            try
            {
                var transicion = entorno.contextoDelProceso.Set<TransicionesDeUnCircuitoDocDtm>().Where(x => x.Id == idTransicion).FirstOrDefault();
                if (transicion.IdOrigen == circuito.IdEstado)
                {
                    circuito.Transitar(contexto, idTransicion, new Dictionary<string, object> { { ltrParametrosNeg.FechaDeTransicion, entorno.TrabajoDeUsuario.Planificado } });
                    entorno.CrearTraza($"transición realizada en el cad '{circuito.Referencia}'");
                }
                else
                {
                    entorno.CrearTraza($"El cad '{circuito.Referencia}' ya había sido transitado");
                }
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

        public static void CerrarFichadasAbiertas(EntornoDeTrabajo entorno)
        {
            var contexto = entorno.contextoDelProceso;
            contexto.IniciarTraza(nameof(CerrarFichadasAbiertas));
            entorno.CrearTraza($"Inicio del proceso");
            var tran = contexto.IniciarTransaccion();
            var otorgado = contexto.Usuario.OtorgarAdministrador(contexto);
            try
            {
                if (entorno.TrabajoDeUsuario.Planificado.Day != DateTime.Now.Day)
                {
                    entorno.CrearTraza($"El trabajo ya no es ejecutable, ya que la fecha de ejecución era para el día  '{entorno.TrabajoDeUsuario.Planificado.ToString("yyyy-MM-dd HH.mm")}' y hoy es '{DateTime.Now.Day}'");
                }
                else if (entorno.TrabajoDeUsuario.Planificado.Hour > DateTime.Now.Hour)
                {
                    entorno.CrearTraza($"El trabajo se resometerá a las  '{entorno.TrabajoDeUsuario.Planificado.Hour}' horas, aun no puede ser ejecutado");
                    entorno.FechaDeResometimiento = entorno.TrabajoDeUsuario.Planificado;
                }
                else
                {
                    var tipos = enumNegocio.CircuitoDoc.Parametro(enumParametrosDeCircuitosDoc.CAD_IdsDeTiposDeFichadas, crearParametro: true, valorPorDefecto: Literal.Cero).Valor.ToLista<int>(quitarNegativos: true);
                    if (tipos.Count > 0)
                    {
                        var filtroPorEstados = new ClausulaDeFiltrado
                        {
                            Clausula = nameof(IUsaEstado.IdEstado),
                            Criterio = enumCriteriosDeFiltrado.esAlgunoDe,
                            Valor = enumEtapasDeCircuitosDoc.CAD_Etapa_Abierto.Estados()
                        };

                        var filtroPorTipos = new ClausulaDeFiltrado
                        {
                            Clausula = nameof(IUsaTipo.IdTipo),
                            Criterio = enumCriteriosDeFiltrado.esAlgunoDe,
                            Valor = string.Join(',', tipos)
                        };
                        var abiertos = entorno.contextoDelProceso.SeleccionarTodos<CircuitoDocDtm>(new List<ClausulaDeFiltrado> { filtroPorTipos, filtroPorEstados });
                        var estadosCerrados = enumEtapasDeCircuitosDoc.CAD_Etapa_Cerrado.EstadosDeLaEtapa();
                        var horaDeCierre = enumNegocio.CircuitoDoc.Parametro(enumParametrosDeCircuitosDoc.CAD_HoraDeCierre, crearParametro: true, valorPorDefecto: "19").Valor.Entero();
                        foreach (var fichada in abiertos)
                        {
                            var fechaCierre = fichada.FechaCreacion.AddHours(8);
                            while (fechaCierre.Hour > horaDeCierre) fechaCierre = fechaCierre.AddHours(-1);

                            var transicion = fichada.TransicionPosible(contexto, estadosCerrados.etapa, estadosCerrados.estados, delSistema: true);
                            fichada.Transitar(contexto, transicion.Id, new Dictionary<string, object> { { ltrParametrosNeg.FechaDeTransicion, fechaCierre } });
                            entorno.CrearTraza($"fichada de salida realizada '{fichada.Referencia}' a las '{fechaCierre.ToString("yyyy-MM-dd HH.mm")}'");

                        }

                    }
                }
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
                contexto.Usuario.AnularAdministrador(contexto, otorgado);
            }
        }


        public static void CerrarFichadasAntiguas(EntornoDeTrabajo entorno)
        {
            var contexto = entorno.contextoDelProceso;
            contexto.IniciarTraza(nameof(CerrarFichadasAntiguas));
            entorno.CrearTraza($"Inicio del proceso");
            var tran = contexto.IniciarTransaccion();
            var otorgado = contexto.Usuario.OtorgarAdministrador(contexto);
            try
            {
                var tipos = enumNegocio.CircuitoDoc.Parametro(enumParametrosDeCircuitosDoc.CAD_IdsDeTiposDeFichadas, crearParametro: true, valorPorDefecto: Literal.Cero).Valor.ToLista<int>(quitarNegativos: true);
                if (tipos.Count > 0)
                {
                    var filtroPorEstados = new ClausulaDeFiltrado
                    {
                        Clausula = nameof(IUsaEstado.IdEstado),
                        Criterio = enumCriteriosDeFiltrado.esAlgunoDe,
                        Valor = enumEtapasDeCircuitosDoc.CAD_Etapa_Abierto.Estados()
                    };

                    var filtroPorTipos = new ClausulaDeFiltrado
                    {
                        Clausula = nameof(IUsaTipo.IdTipo),
                        Criterio = enumCriteriosDeFiltrado.esAlgunoDe,
                        Valor = string.Join(',', tipos)
                    };
                    var abiertos = entorno.contextoDelProceso.SeleccionarTodos<CircuitoDocDtm>(new List<ClausulaDeFiltrado> { filtroPorTipos, filtroPorEstados });
                    var estadosCerrados = enumEtapasDeCircuitosDoc.CAD_Etapa_Cerrado.EstadosDeLaEtapa();
                    var horaDeCierre = enumNegocio.CircuitoDoc.Parametro(enumParametrosDeCircuitosDoc.CAD_HoraDeCierre, crearParametro: true, valorPorDefecto: "19").Valor.Entero();
                    foreach (var fichada in abiertos)
                    {
                        if (fichada.FechaCreacion.Date == DateTime.Today)
                            continue;

                        var fechaCierre = fichada.FechaCreacion.AddHours(8);
                        while (fechaCierre.Hour > horaDeCierre) fechaCierre = fechaCierre.AddHours(-1);

                        var transicion = fichada.TransicionPosible(contexto, estadosCerrados.etapa, estadosCerrados.estados, delSistema: true);
                        fichada.Transitar(contexto, transicion.Id, new Dictionary<string, object> { { ltrParametrosNeg.FechaDeTransicion, fechaCierre } });
                        entorno.CrearTraza($"fichada de salida realizada '{fichada.Referencia}' a las '{fechaCierre.ToString("yyyy-MM-dd HH.mm")}'");

                    }

                }
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
                contexto.Usuario.AnularAdministrador(contexto, otorgado);
            }
        }

    }
}
