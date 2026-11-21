using Gestor.Errores;
using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.Gastos;
using ServicioDeDatos.Logistica;
using ServicioDeDatos.MaestrosTecnico;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.Ventas;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilidades;
using ServicioDeDatos.Negocio;
using GestorDeElementos.Extensores.Elementos;

namespace GestorDeElementos.Extensores
{
    public static class ExtensorDePreasientos
    {
        public static bool UsaElDetalleDe(Type tipoDeDetalle)
        {
            if (tipoDeDetalle == typeof(ApunteDeUnPreasientoDtm))
                return true;

            return false;
        }

        // al activar los preasientos de una sociedad (trabajo ActivarPreasientosDeLaSociedad) se crean los de documentos antiguos: el gestor
        // de preasientos no ajusta su fecha contable al último cierre
        private static Dictionary<string, object> ParametrosDeInsercion(bool generandoPreasientosAntiguos) =>
            new Dictionary<string, object> { { ltrDeUnPreasiento.GenerandoPreasientosAntiguos, generandoPreasientosAntiguos } };

        public static PreasientoDtm Preasiento(this IUsaPreasiento elemento, ContextoSe contexto, bool aplicarJoin = false, bool errorSiNoHay = true)
        {
            if (elemento.Preasiento != null)
            {
                if (elemento.Preasiento.IdReferenciado.Entero() == 0) elemento.Preasiento.IdReferenciado = elemento.Id;
                return elemento.Preasiento;
            }

            if (elemento.IdPreasiento is null && errorSiNoHay)
                GestorDeErrores.Emitir($"{elemento.GetType().NegocioDeUnDtm().Singular()}: '{elemento.Referencia}', no tiene preasiento");

            if (elemento.IdPreasiento is null) return null;

            elemento.Preasiento = contexto.SeleccionarPorId<PreasientoDtm>((int)elemento.IdPreasiento, aplicarJoin, usarLaCache: false);
            if (elemento.Preasiento.IdReferenciado.Entero() == 0) elemento.Preasiento.IdReferenciado = elemento.Id;
            return elemento.Preasiento;
        }

        public static void CancelarPreasiento(this IUsaPreasiento elemento, ContextoSe contexto)
        {
            var preasiento = elemento.Preasiento(contexto, errorSiNoHay: false);
            if (preasiento == null)
                return;

            preasiento.CancelarPreasiento(contexto);
            elemento.IdPreasiento = null;
            elemento.Preasiento = null;

        }

        // Sólo se cancela (de pendiente a cancelado) un preasiento pendiente, y si su fecha contable no es anterior al último cierre de su sociedad
        // (ver GestorDePreasientos.AntesDeTransitar). Uno contabilizado está en un lote contable: para cancelarlo se ha de anular antes ese lote
        // o eliminar su cierre, lo que lo devuelve a pendiente. Los ya cancelados o anulados (etapa en desuso) se dejan como están
        public static void CancelarPreasiento(this PreasientoDtm preasiento, ContextoSe contexto)
        {
            if (preasiento.Estado(contexto).Cancelado || preasiento.EstaEnLaEtapa(enumEtapasDePreasiento.SPR_Etapa_Anulado))
                return;

            preasiento.ValidarQueEstaPendiente(contexto, "cancelar");

            // el referenciado de un almacén es un movimiento de almacén, que se borra al cancelar la entrega que lo originó
            var esDeUnAlmacen = preasiento.NegocioReferenciado == enumNegocio.Almacen;
            // el de un pedido es de una entrega o devolución en obra, que se borra al cancelarla
            var esDeUnPedido = preasiento.NegocioReferenciado == enumNegocio.Pedido;
            // el de un expediente es de un apunte de gasto o ingreso, que se cancela al modificarlo o eliminarlo
            var esDeUnExpediente = preasiento.NegocioReferenciado == enumNegocio.Expediente;

            var asunto = preasiento.NegocioReferenciado == enumNegocio.Cobro ? "Cancelado al eliminar el cobro"
                       : esDeUnAlmacen ? "Cancelado al eliminar el movimiento de almacén"
                       : esDeUnPedido ? "Cancelado al cancelar la entrega o la devolución en obra"
                       : esDeUnExpediente ? "Cancelado al modificar o eliminar el apunte del expediente"
                       : "Cancelado al regenerar el preasiento";

            var detalle = preasiento.NegocioReferenciado == enumNegocio.Cobro
                ? $"Se ha eliminado el cobro asociado a la factura {((CobroDeFaeDtm)preasiento.Referenciado(contexto)).Factura(contexto).Referencia}"
                : esDeUnAlmacen
                ? $"Se ha cancelado la entrega que originó el movimiento de almacén Nº {preasiento.IdReferenciado} y éste se ha eliminado"
                : esDeUnPedido
                ? $"Se ha cancelado la {preasiento.Nombre.Left(1).ToLower()}{preasiento.Nombre.Substring(1)}"
                : esDeUnExpediente
                ? $"Se ha modificado o eliminado el {preasiento.Nombre.Left(1).ToLower()}{preasiento.Nombre.Substring(1)}"
                : "Se ha regenerado el preasiento desde la interface";

            preasiento.TransitarALaEtapa(contexto, enumEtapasDePreasiento.SPR_Etapa_Cancelado.EstadosDeLaEtapa(), parametros: new Dictionary<string, object>
                        {
                            {ltrParametrosEp.asunto, asunto },
                            {ltrParametrosEp.detalleAsunto, detalle }
                        });
        }

        // Un preasiento que no está pendiente no se puede cancelar ni regenerar; si está contabilizado se indica qué hacer para devolverlo a pendiente
        public static void ValidarQueEstaPendiente(this PreasientoDtm preasiento, ContextoSe contexto, string accion)
        {
            if (preasiento.EstaEnLaEtapa(enumEtapasDePreasiento.SPR_Etapa_Pendiente))
                return;

            if (preasiento.EstaEnLaEtapa(enumEtapasDePreasiento.SPR_Etapa_Contabilizado))
                GestorDeErrores.Emitir($"No se puede {accion} el preasiento '{preasiento.Referencia}' de {preasiento.NegocioReferenciado.ConArticulo()} por estar " +
                                       $"{enumEtapasDePreasiento.SPR_Etapa_Contabilizado.Nombre()}, {preasiento.ComoDevolverloAPendiente(contexto)}");

            GestorDeErrores.Emitir($"No se puede {accion} el preasiento '{preasiento.Referencia}' por no estar {enumEtapasDePreasiento.SPR_Etapa_Pendiente.Nombre()}");
        }

        // Un preasiento contabilizado vuelve a pendiente al anular el lote contable en el que está; si el lote es el de un cierre, eliminando
        // ese cierre (sólo se puede eliminar el último de la sociedad, así que antes se han de eliminar los posteriores)
        private static string ComoDevolverloAPendiente(this PreasientoDtm preasiento, ContextoSe contexto)
        {
            var lote = GestorDeVinculos.RegistrosVinculados<CircuitoDocDtm>(contexto, enumNegocio.Preasiento, preasiento.Id).FirstOrDefault();
            if (lote is null)
                return "no está en ningún lote contable";

            var cierre = contexto.Set<CierreContableDtm>().FirstOrDefault(c => c.IdLote == lote.Id);
            return cierre is null
                ? $"anule antes el lote contable '{lote.Referencia}'"
                : $"elimine antes el cierre contable del '{cierre.CerradaEl:dd/MM/yyyy}' (lote '{lote.Referencia}') y, si los hay, los posteriores de su sociedad";
        }


        public static void CancelarPreasientoPorEstarloSuReferenciado(this PreasientoDtm preasiento, ContextoSe contexto)
        {
            if (preasiento.Estado(contexto).Cancelado)
                return;

            if (preasiento.EstaEnLaEtapa(enumEtapasDePreasiento.SPR_Etapa_Pendiente))
            {
                var referenciado = preasiento.Referenciado(contexto, errorSiNoHay: false);

                var referencia = referenciado is null
                ? "Eliminado de la BD" :
                preasiento.NegocioReferenciado == enumNegocio.Cobro
                ? ((CobroDeFaeDtm)referenciado).Referencia(contexto)
                : preasiento.NegocioReferenciado == enumNegocio.Almacen
                ? ((MovimientoDeAlmacenDtm)referenciado).Referencia(contexto)
                : referenciado.Referencia;

                preasiento.TransitarALaEtapa(contexto, enumEtapasDePreasiento.SPR_Etapa_Cancelado.EstadosDeLaEtapa(), parametros: new Dictionary<string, object>
                            {
                                {ltrParametrosEp.asunto, "Cancelado al intentar contabilizar" },
                                {ltrParametrosEp.detalleAsunto, $"Se ha cancelado por no ser contabilizable el referenciado '{preasiento.NegocioReferenciado}: {referencia}'"
                                }
                            });
            }
        }

        // complemento: lo que se quiera añadir tras la referencia del elemento en el error, como la remesa en la que está un pago
        public static void AnularPreasiento(this IUsaPreasiento elemento, ContextoSe contexto, Dictionary<string, object> parametros, string complemento = "")
        {
            if (elemento.Preasiento(contexto, errorSiNoHay: false) is not null)
            {
                var preasiento = elemento.Preasiento(contexto);
                if (preasiento.EstaEnLaEtapa(enumEtapasDePreasiento.SPR_Etapa_Contabilizado))
                {
                    GestorDeErrores.Emitir($"No se puede devolver {preasiento.NegocioReferenciado.ConArticulo()} '{elemento.Referencia}'{complemento} ya que el preasiento '{preasiento.Referencia}' asociado está " +
                                           $"{enumEtapasDePreasiento.SPR_Etapa_Contabilizado.Nombre()}, {preasiento.ComoDevolverloAPendiente(contexto)}");
                }
                else
                {
                    if (!preasiento.EstaEnLaEtapa(enumEtapasDePreasiento.SPR_Etapa_Cancelado) && !preasiento.EstaEnLaEtapa(enumEtapasDePreasiento.SPR_Etapa_Anulado))
                    {
                        var mensaje = parametros.ContainsKey(ltrParametrosEp.detalleAsunto)
                        ? parametros[ltrParametrosEp.detalleAsunto].ToString()
                        : $"El preasiento ha sido anulado al transitar {elemento.Preasiento(contexto).NegocioReferenciado.ConArticulo()} '{elemento.Referencia}'";

                        preasiento.TransitarALaEtapa(contexto, enumEtapasDePreasiento.SPR_Etapa_Cancelado.EstadosDeLaEtapa(), parametros: new Dictionary<string, object>
                           {
                               {ltrParametrosEp.asunto, "Anulado el asiento por el sistema" },
                               {ltrParametrosEp.detalleAsunto, mensaje }
                           });
                        elemento.IdPreasiento = null;
                        elemento.Preasiento = null;
                        parametros[ltrParametrosNeg.AccionQueSeEjecuta] = ltrDeUnPreasiento.Accion_AnularPreasiento;
                    }
                }
            }
        }

        public static void AnularContabilizacion(this ArchivadorDtm archivador, ContextoSe contexto)
        {
            var preasientos = VinculoSql.LeerVinculosAl(contexto, typeof(PreasientoDtm), enumNegocio.Archivador, typeof(ArchivadorDtm), archivador.Id, filtros: null);
            foreach (var vinculo in preasientos)
            {
                var preasiento = contexto.SeleccionarPorId<PreasientoDtm>(vinculo.idElemento1);
                preasiento.AnularContabilizacion(contexto, archivador);
            }
        }


        public static void AnularLoteContable(this CircuitoDocDtm loteContable, ContextoSe contexto, int idSemaforo)
        {
            if (contexto.Entorno is null)
                GestorDeErrores.Emitir($"El método de {AnularLoteContable} solo es ejecutable por un trabajo");

            if (contexto.TrazarEnElTrabajo is not null)
                contexto.TrazarEnElTrabajo($"El lote contable se cancela por un trabajo sometido");
            loteContable.TransitarALaEtapa(contexto, enumEtapasDeCircuitosDoc.CAD_Etapa_Cancelado.EstadosDeLaEtapa(), new Dictionary<string, object> {
                { ltrParametrosNeg.EstaEjecutandoUnaAccion, true },
                { ltrParametrosNeg.IdSemaforo, idSemaforo },
                { ltrParametrosEp.detalleAsunto, "Cancelado el lote contable por el trabajo de sistema"} }
                );
        }

        public static void AnularEstimacionDirecta(this CircuitoDocDtm estimacion, ContextoSe contexto, int idSemaforo)
        {
            if (contexto.Entorno is null)
                GestorDeErrores.Emitir($"El método de {AnularEstimacionDirecta} solo es ejecutable por un trabajo");

            if (contexto.TrazarEnElTrabajo is not null)
                contexto.TrazarEnElTrabajo($"La estimación directa se cancela por un trabajo sometido");

            estimacion.TransitarALaEtapa(contexto, enumEtapasDeCircuitosDoc.CAD_Etapa_Cancelado.EstadosDeLaEtapa(), new Dictionary<string, object> {
                { ltrParametrosNeg.EstaEjecutandoUnaAccion, true },
                { ltrParametrosNeg.IdSemaforo, idSemaforo },
                { ltrParametrosEp.detalleAsunto, "Cancelado la estimación directa por el trabajo de sistema"} }
                );
        }


        public static void AnularContabilizacion(this PreasientoDtm preasiento, ContextoSe contexto)
        {
            try
            {
                var lotes = GestorDeVinculos.RegistrosVinculados<CircuitoDocDtm>(contexto, enumNegocio.Preasiento, preasiento.Id);
                if (lotes.Count() == 0)
                    preasiento.AnularContabilizacion(contexto, loteContable: null);
                else
                    foreach (var loteContable in lotes)
                    {
                        preasiento.AnularContabilizacion(contexto, loteContable);

                        if (loteContable.Vinculados<PreasientoDtm>(contexto).Count() == 0)
                            loteContable.TransitarALaEtapa(contexto, enumEtapasDeCircuitosDoc.CAD_Etapa_Cancelado.EstadosDeLaEtapa(), new Dictionary<string, object> {
                            { ltrParametrosNeg.EstaEjecutandoUnaAccion, true },
                            { ltrParametrosEp.detalleAsunto, "Cancelado el lote contable por no tener preasientos asociados" } }
                                );

                        //loteContable.CrearObservacion(contexto, nombre: ltrDeUnLoteContable.PreasientoQuitadoDelLote,
                        //    descripción: $"El preasiento '{preasiento.Referencia}' se ha sacado del lote contable por el usuario '{contexto.Usuario.Login}'",
                        //    parametros: new Dictionary<string, object> {
                        //        { ltrParametrosNeg.ValidarPermisosDePersistencia, false },
                        //        { ltrDeObservaciones.CreadaPorAdminSe, true}
                        //    });

                        loteContable.CrearTraza(contexto, nombre: ltrTrazasDeUnLoteContable.PreasientoQuitadoDelLote,
                            descripción: $"El preasiento '{preasiento.Referencia}' se ha sacado del lote contable por el usuario '{contexto.Usuario.Login}'");
                    }
            }
            catch
            {
                preasiento.VaciarCacheDeRegistro(contexto, enumTipoOperacion.Modificar, preasiento.Nombre);
                throw;
            }
        }

        public static void AnularContabilizacion(this PreasientoDtm preasiento, ContextoSe contexto, CircuitoDocDtm loteContable)
        {
            if (preasiento.Etapas().Contains(enumEtapasDePreasiento.SPR_Etapa_Contabilizado))
            {
                preasiento.TransitarALaEtapa(contexto, enumEtapasDePreasiento.SPR_Etapa_Pendiente.EstadosDeLaEtapa(), parametros: new Dictionary<string, object>
                    {
                        {ltrParametrosEp.asunto, "Retrocedido la contabilización" },
                        {ltrParametrosEp.detalleAsunto, $"El asiento ha sido retrocedido anular la contabilización, lote contable: '{loteContable?.Referencia ?? "No estaba en ningún lote contable"}'" }
                    });
            }

            if (loteContable == null)
                return;

            preasiento.Desvincular(contexto, loteContable, parametros: new Dictionary<string, object> { { ltrParametrosNeg.AccionQueSeEjecuta, nameof(AnularContabilizacion) } });
            preasiento.CrearTraza(contexto, ltrTrazasDeUnPreasiento.EliminarPreasientoDelLote, $"El preasiento '{preasiento.Referencia}' se ha elimnado del lote contable '{loteContable.Referencia}'");

            if (contexto.TrazarEnElTrabajo is not null)
                contexto.TrazarEnElTrabajo($"Preasiento '{preasiento.Referencia}' desvinculado del circuito '{loteContable.Referencia}' y devuelto a pendiente");

        }

        private static void AnularContabilizacion(this PreasientoDtm preasiento, ContextoSe contexto, ArchivadorDtm archivador)
        {
            if (preasiento.Etapas().Contains(enumEtapasDePreasiento.SPR_Etapa_Contabilizado))
            {
                preasiento.TransitarALaEtapa(contexto, enumEtapasDePreasiento.SPR_Etapa_Pendiente.EstadosDeLaEtapa(), parametros: new Dictionary<string, object>
                    {
                        {ltrParametrosEp.asunto, "Retrocedido la contabilización" },
                        {ltrParametrosEp.detalleAsunto, $"El asiento ha sido retrocedido por dar de baja el archivador '{archivador.Referencia}'" }
                    });
            }
            QuitarArchivador(preasiento, contexto, archivador);
        }

        private static void QuitarArchivador(this PreasientoDtm preasiento, ContextoSe contexto, ArchivadorDtm archivador)
        {
            VinculoSql.QuitarVinculo(contexto, typeof(PreasientoDtm), enumNegocio.Archivador, preasiento.Id, archivador.Id);

            var negocio = preasiento.NegocioReferenciado;
            VinculoSql.QuitarVinculo(contexto, negocio.TipoDtm(), enumNegocio.Archivador, preasiento.IdReferenciado.Entero(), archivador.Id);

            var archivos = archivador.LeerAnexados(contexto);
            var proveedor = preasiento.Proveedor(contexto, errorSiNoHay: false);
            if (proveedor != null)
            {
                var adjuntos = proveedor.LeerAnexados(contexto);
                foreach (var adjunto in adjuntos)
                {
                    if (archivos.Any(archivo => archivo.Id == adjunto.Id))
                    {
                        proveedor.QuitarAnexado(contexto, adjunto.Id, validarPersistencia: false, QuitarDeRestoDeAnexados: false);
                        proveedor.DesmarcarComoMigrado(contexto, enumSistemaContable.NCS, preasiento.Cg(contexto).Sociedad(contexto).PlanContable().IdPlanContable);
                    }

                }
            }
            var cliente = preasiento.Cliente(contexto, errorSiNoHay: false);
            if (cliente != null)
            {
                var adjuntos = cliente.LeerAnexados(contexto);
                foreach (var adjunto in adjuntos)
                {
                    if (archivos.Any(archivo => archivo.Id == adjunto.Id))
                    {
                        cliente.QuitarAnexado(contexto, adjunto.Id, validarPersistencia: false, QuitarDeRestoDeAnexados: false);
                        cliente.DesmarcarComoMigrado(contexto, enumSistemaContable.NCS, preasiento.Cg(contexto).Sociedad(contexto).PlanContable().IdPlanContable);
                    }
                }
            }
        }


        public static FacturaRecDtm Preasentar(this FacturaRecDtm recibida, ContextoSe contexto, bool generandoPreasientosAntiguos = false)
        {
            if (!recibida.Sociedad(contexto).UsaPreasientos(contexto, enumNegocio.FacturaRecibida))
            {
                return recibida;
            }

            recibida.CancelarPreasiento(contexto);

            var idTipoPreasiento = recibida.IdTipoPreasiento(contexto);
            var tipoDePreasiento = enumNegocio.Preasiento.Tipos(contexto).FirstOrDefault(x => x.Id == idTipoPreasiento);
            if (tipoDePreasiento == null)
            {
                GestorDeErrores.Emitir($"El id del tipo del preasiento parametrizado en '{idTipoPreasiento}' no existe en la BD");
            }

            VariablesDePreasiento.CodigoDeDiario(tipoDePreasiento, contexto);

            VariablesDePreasiento.ValidarNaturalezasContables();

            recibida.Preasiento = new PreasientoDtm
            {
                IdCg = recibida.IdCg,
                IdTipo = recibida.IdTipoPreasiento(contexto),
                Nombre = recibida.Nombre,
                // el ejercicio es el de la fecha contable, no el de la factura, para que el cierre de ese periodo la traspase
                Ejercicio = recibida.ContabilizadaEl.Fecha().Year,
                CodigoDiario = tipoDePreasiento.CodigoDeDiario(contexto),
                SociedadContable = recibida.Sociedad(contexto).CodigoContable,
                NegocioReferenciado = enumNegocio.FacturaRecibida,
                IdReferenciado = recibida.Id,
                FechaContable = recibida.ContabilizadaEl.Fecha()
            }.InsertarComoAdministrador(contexto, parametros: ParametrosDeInsercion(generandoPreasientosAntiguos));

            recibida.IdPreasiento = recibida.Preasiento.Id;

            var orden = 10;
            var bases = recibida.BiDelIvaPorNaturaleza(contexto);

            foreach (var item in bases)
            {
                var naturaleza = contexto.SeleccionarPorId<NaturalezaDtm>(item.IdNaturaleza, aplicarJoin: true);
                if (naturaleza.IdCuentaDeGasto == null)
                    GestorDeErrores.Emitir($"Ha de indicar la cuenta del gasto para la naturaleza '{naturaleza.Expresion}'");
                new ApunteDeUnPreasientoDtm
                {
                    IdElemento = recibida.IdPreasiento.Entero(),
                    Posicion = enumPosicionContable.Debe,
                    Clase = enumClaseDeApunte.Far_Gasto,
                    Tipo = ltrTipoDeApunte.Gasto,
                    Orden = orden,
                    Cuenta = naturaleza.CuentaDeGasto(contexto).Codigo,
                    Importe = item.Bi,
                    IvaDelImporte = item.ImporteDeIva,
                    IdIva = item.idIva,
                    Concepto = item.Concepto,
                }.InsertarComoAdministrador(contexto);

                orden += 10;
            }
            var totalesIvaSoportadoIsp = new Dictionary<int, decimal>();
            var basesDelIvaSoportadoIsp = new Dictionary<int, decimal>();
            decimal totalIsp = 0;
            foreach (var item in recibida.Ivas(contexto))
            {
                var iva = contexto.SeleccionarPorId<IvaSoportadoDtm>(item.IdIva, aplicarJoin: true);
                new ApunteDeUnPreasientoDtm
                {
                    IdElemento = recibida.IdPreasiento.Entero(),
                    Posicion = enumPosicionContable.Debe,
                    Clase = enumClaseDeApunte.Far_Iva,
                    Tipo = item.Tipo.Left(20),
                    Orden = orden,
                    Cuenta = iva.Cuenta.Codigo,
                    Importe = item.Importe,
                    BaseDelImporte = item.BI,
                    Concepto = iva.Expresion,
                }.InsertarComoAdministrador(contexto);
                if (item.EsIsp)
                {
                    if (totalesIvaSoportadoIsp.ContainsKey(item.IdIva))
                    {
                        totalesIvaSoportadoIsp[item.IdIva] = totalesIvaSoportadoIsp[item.IdIva] + item.Importe;
                        basesDelIvaSoportadoIsp[item.IdIva] = totalesIvaSoportadoIsp[item.IdIva] + item.Importe;
                    }
                    else
                    {
                        totalesIvaSoportadoIsp[item.IdIva] = item.Importe;
                        basesDelIvaSoportadoIsp[item.IdIva] = item.Importe;
                    }
                    totalIsp += item.Importe;
                }
                orden += 10;
            }

            var proveedor = recibida.Proveedor(contexto, aplicarJoin: true);

            new ApunteDeUnPreasientoDtm
            {
                IdElemento = recibida.IdPreasiento.Entero(),
                Posicion = enumPosicionContable.Haber,
                Clase = enumClaseDeApunte.Far_Proveedor,
                Tipo = "",
                Orden = orden,
                Cuenta = proveedor.CodigoDeCtaContable(contexto),
                Importe = recibida.TotalDelPago,
                Concepto = recibida.Nombre.Left(IDominio.Longitud(IDominio.VARCHAR_250) - 1),
            }.InsertarComoAdministrador(contexto);

            orden += 10;

            if (totalIsp != 0)
            {
                foreach (var clave in totalesIvaSoportadoIsp.Keys)
                {
                    var ivaSop = contexto.SeleccionarPorId<IvaSoportadoDtm>(clave);
                    var ivaIsp = contexto.Set<IvaRepercutidoDtm>().FirstOrDefault(iva => iva.Clase == enumClasesDeIvaRep.ISP && iva.Porcentaje == ivaSop.Porcentaje);
                    if (ivaIsp == null)
                    {
                        GestorDeErrores.Emitir($"Debe de definir un tipo de iva repercutido para inversión de sujeto pasivo con porcentaje '{ivaSop.Porcentaje}'");
                    }
                    var cuentaDeIvaRepercutido = contexto.SeleccionarPorId<CuentaDtm>(ivaIsp.IdCuenta).Codigo;
                    new ApunteDeUnPreasientoDtm
                    {
                        IdElemento = recibida.IdPreasiento.Entero(),
                        Posicion = enumPosicionContable.Haber,
                        Clase = enumClaseDeApunte.Fae_Iva,
                        Tipo = "",
                        Orden = orden,
                        Cuenta = cuentaDeIvaRepercutido,
                        Importe = totalesIvaSoportadoIsp[clave],
                        BaseDelImporte = basesDelIvaSoportadoIsp[clave],
                        Concepto = ivaIsp.Expresion,
                    }.InsertarComoAdministrador(contexto);

                    orden += 10;
                }
            }

            foreach (var item in recibida.Irpfs(contexto))
            {
                var irpf = contexto.SeleccionarPorId<IrpfDtm>(item.IdIrpf, aplicarJoin: true);
                new ApunteDeUnPreasientoDtm
                {
                    IdElemento = recibida.IdPreasiento.Entero(),
                    Posicion = enumPosicionContable.Haber,
                    Clase = enumClaseDeApunte.Far_Irpf,
                    Tipo = item.Tipo.Left(20),
                    Orden = orden,
                    Cuenta = irpf.Cuenta.Codigo,
                    Importe = item.Importe,
                    BaseDelImporte = item.BI,
                    Concepto = irpf.Expresion,
                }.InsertarComoAdministrador(contexto);

                orden += 10;
            }


            return recibida;
        }

        // generandoPreasientosAntiguos: al activar los preasientos de la sociedad, el preasiento se crea con la fecha de la factura aunque sea
        // anterior al último cierre (el trabajo de activación lo da por contabilizado)
        public static FacturaEmtDtm Preasentar(this FacturaEmtDtm emitida, ContextoSe contexto, bool generandoPreasientosAntiguos = false)
        {
            if (!emitida.Sociedad(contexto).UsaPreasientos(contexto, enumNegocio.FacturaEmitida))
            {
                return emitida;
            }

            emitida.CancelarPreasiento(contexto);
            var idTipoPreasiento = emitida.IdTipoPreasiento(contexto);
            var tipoDePreasiento = enumNegocio.Preasiento.Tipos(contexto).FirstOrDefault(x => x.Id == idTipoPreasiento);
            if (tipoDePreasiento == null)
            {
                GestorDeErrores.Emitir($"El id del tipo del preasiento parametrizado en '{idTipoPreasiento}' no existe en la BD");
            }

            VariablesDePreasiento.CodigoDeDiario(tipoDePreasiento, contexto);

            VariablesDePreasiento.ValidarNaturalezasContables();

            emitida.Preasiento = new PreasientoDtm
            {
                IdCg = emitida.IdCg,
                IdTipo = emitida.IdTipoPreasiento(contexto),
                Nombre = emitida.Nombre,
                Ejercicio = ((DateTime)emitida.FacturadaEl).Year,
                CodigoDiario = tipoDePreasiento.CodigoDeDiario(contexto),
                SociedadContable = emitida.Sociedad(contexto).CodigoContable,
                NegocioReferenciado = enumNegocio.FacturaEmitida,
                IdReferenciado = emitida.Id,
                FechaContable = emitida.FacturadaEl.Fecha()
            }.InsertarComoAdministrador(contexto, parametros: ParametrosDeInsercion(generandoPreasientosAntiguos));

            emitida.IdPreasiento = emitida.Preasiento.Id;

            var orden = 10;
            var bases = emitida.BiDelIvaPorNaturaleza(contexto);
            foreach (var item in bases)
            {
                var naturaleza = contexto.SeleccionarPorId<NaturalezaDtm>(item.IdNaturaleza, aplicarJoin: true);
                if (naturaleza.IdCuentaDeIngreso == null)
                    GestorDeErrores.Emitir($"Ha de indicar la cuenta del ingreso para la naturaleza '{naturaleza.Expresion}'");
                new ApunteDeUnPreasientoDtm
                {
                    IdElemento = emitida.IdPreasiento.Entero(),
                    Posicion = enumPosicionContable.Haber,
                    Clase = enumClaseDeApunte.Fae_Ingreso,
                    Tipo = ltrTipoDeApunte.Ingreso,
                    Orden = orden,
                    Cuenta = naturaleza.CuentaDeIngreso(contexto).Codigo,
                    Importe = item.Bi,
                    IvaDelImporte = item.ImporteDeIva,
                    IdIva = item.idIva,
                    Concepto = item.Concepto,
                }.InsertarComoAdministrador(contexto);

                orden += 10;
            }
            var totalesIvaRepercutidoIsp = new Dictionary<int, decimal>();
            var basesDelIvaRepercutidoIsp = new Dictionary<int, decimal>();
            decimal totalIsp = 0;
            foreach (var item in emitida.Ivas(contexto))
            {
                var iva = contexto.SeleccionarPorId<IvaRepercutidoDtm>(item.IdIva, aplicarJoin: true);
                new ApunteDeUnPreasientoDtm
                {
                    IdElemento = emitida.IdPreasiento.Entero(),
                    Posicion = enumPosicionContable.Haber,
                    Clase = enumClaseDeApunte.Fae_Iva,
                    Tipo = item.Tipo.Left(20),
                    Orden = orden,
                    Cuenta = iva.Cuenta.Codigo,
                    Importe = item.Importe,
                    BaseDelImporte = item.BI,
                    Concepto = iva.Expresion,
                }.InsertarComoAdministrador(contexto);
                if (item.EsIsp)
                {
                    if (totalesIvaRepercutidoIsp.ContainsKey(item.IdIva))
                    {
                        totalesIvaRepercutidoIsp[item.IdIva] = totalesIvaRepercutidoIsp[item.IdIva] + item.Importe;
                        basesDelIvaRepercutidoIsp[item.IdIva] = basesDelIvaRepercutidoIsp[item.IdIva] + item.Importe;
                    }
                    else
                    {
                        totalesIvaRepercutidoIsp[item.IdIva] = item.Importe;
                        basesDelIvaRepercutidoIsp[item.IdIva] = item.Importe;
                    }
                    totalIsp += item.Importe;
                }
                orden += 10;
            }

            var cliente = emitida.Cliente(contexto, aplicarJoin: true);

            new ApunteDeUnPreasientoDtm
            {
                IdElemento = emitida.IdPreasiento.Entero(),
                Posicion = enumPosicionContable.Debe,
                Clase = enumClaseDeApunte.Fae_Cliente,
                Tipo = "",
                Orden = orden,
                Cuenta = cliente.CodigoDeCtaContable(contexto),
                Importe = emitida.APagar(contexto),
                Concepto = emitida.Nombre.Left(IDominio.Longitud(IDominio.VARCHAR_250) - 1),
            }.InsertarComoAdministrador(contexto);

            orden += 10;

            if (totalIsp != 0)
            {
                foreach (var clave in totalesIvaRepercutidoIsp.Keys)
                {
                    var ivaRep = contexto.SeleccionarPorId<IvaRepercutidoDtm>(clave);
                    var ivaIsp = contexto.Set<IvaSoportadoDtm>().FirstOrDefault(iva => iva.Clase == enumClasesDeIvaSop.ISP && iva.Porcentaje == ivaRep.Porcentaje);
                    if (ivaIsp == null)
                    {
                        GestorDeErrores.Emitir($"Debe de definir un tipo de iva soportado para inversión de sujeto pasivo con porcentaje '{ivaRep.Porcentaje}'");
                    }
                    var cuentaDeIvaSoportado = contexto.SeleccionarPorId<CuentaDtm>(ivaIsp.IdCuenta).Codigo;
                    new ApunteDeUnPreasientoDtm
                    {
                        IdElemento = emitida.IdPreasiento.Entero(),
                        Posicion = enumPosicionContable.Debe,
                        Clase = enumClaseDeApunte.Far_Iva,
                        Tipo = "",
                        Orden = orden,
                        Cuenta = cuentaDeIvaSoportado,
                        Importe = totalesIvaRepercutidoIsp[clave],
                        BaseDelImporte = basesDelIvaRepercutidoIsp[clave],
                        Concepto = ivaIsp.Expresion,
                    }.InsertarComoAdministrador(contexto);

                    orden += 10;
                }
            }

            foreach (var item in emitida.Irpfs(contexto))
            {
                var irpf = contexto.SeleccionarPorId<IrpfDtm>(item.IdIrpf, aplicarJoin: true);
                new ApunteDeUnPreasientoDtm
                {
                    IdElemento = emitida.IdPreasiento.Entero(),
                    Posicion = enumPosicionContable.Debe,
                    Clase = enumClaseDeApunte.Fae_Irpf,
                    Tipo = item.Tipo.Left(20),
                    Orden = orden,
                    Cuenta = irpf.Cuenta.Codigo,
                    Importe = item.Importe,
                    BaseDelImporte = item.BI,
                    Concepto = irpf.Expresion,
                }.InsertarComoAdministrador(contexto);

                orden += 10;
            }


            return emitida;
        }

        public static CobroDeFaeDtm Preasentar(this CobroDeFaeDtm cobro, ContextoSe contexto, bool generandoPreasientosAntiguos = false)
        {
            if (!cobro.Sociedad(contexto).UsaPreasientos(contexto, enumNegocio.Cobro))
            {
                return cobro;
            }

            cobro.CancelarPreasiento(contexto);

            cobro.CalcularReferencia(contexto);
            var idTipoPreasiento = cobro.IdTipoPreasiento(contexto);
            var tipoDePreasiento = enumNegocio.Preasiento.Tipos(contexto).FirstOrDefault(x => x.Id == idTipoPreasiento);
            if (tipoDePreasiento == null)
            {
                GestorDeErrores.Emitir($"El id del tipo del preasiento parametrizado en '{idTipoPreasiento}' no existe en la BD");
            }

            if (cobro.Sociedad(contexto).CodigoFiscal.IsNullOrEmpty())
            {
                GestorDeErrores.Emitir($"Debe asociar el 'código fiscal' a la sociedad pagadora que tenga en contabilidad '{cobro.Sociedad(contexto).NIF}'");
            }

            VariablesDePreasiento.CodigoDeDiario(tipoDePreasiento, contexto);

            cobro.Preasiento = new PreasientoDtm
            {
                IdCg = cobro.Factura(contexto).IdCg,
                IdTipo = cobro.IdTipoPreasiento(contexto),
                Nombre = $"Cobro de la factura '{cobro.Factura(contexto).Referencia}' de importe {cobro.Cobrado.ToMoneda()}",
                Ejercicio = cobro.CobradoEl.Year,
                CodigoDiario = tipoDePreasiento.CodigoDeDiario(contexto),
                SociedadContable = cobro.Sociedad(contexto).CodigoContable,
                NegocioReferenciado = enumNegocio.Cobro,
                IdReferenciado = cobro.Id,
                FechaContable = cobro.CobradoEl
            }.InsertarComoAdministrador(contexto, parametros: ParametrosDeInsercion(generandoPreasientosAntiguos));
            cobro.IdPreasiento = cobro.Preasiento.Id;

            var orden = 10;
            var cuentadeudora = cobro.Cliente(contexto).CodigoDeCtaContable(contexto);
            var deudor = cobro.Cliente(contexto);
            var posicionBanco = $"Cobro: ({deudor.NIF(contexto)}) {cobro.Referencia}".Left(IDominio.Longitud(IDominio.VARCHAR_250) - 1);
            var posicionDeudora = $"Factura '{cobro.Factura(contexto).Referencia}' del cliente {deudor.NIF(contexto)}".Left(IDominio.Longitud(IDominio.VARCHAR_250) - 1);
            new ApunteDeUnPreasientoDtm
            {
                IdElemento = cobro.IdPreasiento.Entero(),
                Posicion = enumPosicionContable.Haber,
                Clase = enumClaseDeApunte.Cob_Deudor,
                Tipo = ltrTipoDeApunte.Cobro,
                Orden = orden,
                Cuenta = cuentadeudora,
                Importe = cobro.Cobrado,
                Concepto = posicionDeudora,
            }.InsertarComoAdministrador(contexto);

            orden += 10;
            var cuentaDeCobro = cobro.CuentaContableDelCobro(contexto);
            new ApunteDeUnPreasientoDtm
            {
                IdElemento = cobro.IdPreasiento.Entero(),
                Posicion = enumPosicionContable.Debe,
                Clase = enumClaseDeApunte.Cob_Cobro,
                Tipo = "",
                Orden = orden,
                Cuenta = cuentaDeCobro,
                Importe = cobro.Cobrado,
                Concepto = posicionBanco,
            }.InsertarComoAdministrador(contexto);

            return cobro;
        }

        public static PagoDtm Preasentar(this PagoDtm pago, ContextoSe contexto, bool generandoPreasientosAntiguos = false)
        {
            if (!pago.Sociedad(contexto).UsaPreasientos(contexto, enumNegocio.Pago))
            {
                return pago;
            }

            pago.CancelarPreasiento(contexto);

            var idTipoPreasiento = pago.IdTipoPreasiento(contexto);
            var tipoDePreasiento = enumNegocio.Preasiento.Tipos(contexto).FirstOrDefault(x => x.Id == idTipoPreasiento);
            if (tipoDePreasiento == null)
            {
                GestorDeErrores.Emitir($"El id del tipo del preasiento parametrizado en '{idTipoPreasiento}' no existe en la BD");
            }

            if (pago.Sociedad(contexto).CodigoFiscal.IsNullOrEmpty())
            {
                GestorDeErrores.Emitir($"Debe asociar el 'código fiscal' a la sociedad pagadora que tenga en contabilidad '{pago.Sociedad(contexto).NIF}'");
            }

            VariablesDePreasiento.CodigoDeDiario(tipoDePreasiento, contexto);

            pago.Preasiento = new PreasientoDtm
            {
                IdCg = pago.IdCg,
                IdTipo = pago.IdTipoPreasiento(contexto),
                Nombre = pago.Nombre,
                Ejercicio = pago.PagadoEl.Fecha().Year,
                CodigoDiario = tipoDePreasiento.CodigoDeDiario(contexto),
                SociedadContable = pago.Sociedad(contexto).CodigoContable,
                NegocioReferenciado = enumNegocio.Pago,
                IdReferenciado = pago.Id,
                FechaContable = pago.PagadoEl.Fecha()
            }.InsertarComoAdministrador(contexto, parametros: ParametrosDeInsercion(generandoPreasientosAntiguos));

            pago.IdPreasiento = pago.Preasiento.Id;

            var orden = 10;
            var acreedor = pago.Solicitante(contexto);
            var esAbono = pago.EsAbono(contexto);
            var cuentaacreedora = pago.IdNaturaleza is null
                      ? esAbono
                      ? pago.Cliente(contexto).CodigoDeCtaContable(contexto)
                      : pago.Proveedor(contexto).CodigoDeCtaContable(contexto)
                      : null;
            var cuentaDeGasto = cuentaacreedora is not null ? null : pago.Naturaleza(contexto).CuentaDeGasto(contexto).Codigo;

            var textoDePosicion = cuentaDeGasto is null
                ? pago.Importe < 0
                ? $"Dev. de pago: {acreedor.NIF(contexto)}: {pago.Referencia}".Left(IDominio.Longitud(IDominio.VARCHAR_250) - 1)
                : esAbono
                ? $"Dev. de cobro: {acreedor.NIF(contexto)}: {pago.Referencia}".Left(IDominio.Longitud(IDominio.VARCHAR_250) - 1)
                : $"Pago: {acreedor.NIF(contexto)}: {pago.Referencia}".Left(IDominio.Longitud(IDominio.VARCHAR_250) - 1)
                : $"Pago: {acreedor.NIF(contexto)}: {pago.Referencia}".Left(IDominio.Longitud(IDominio.VARCHAR_250) - 1);


            if (pago.IdNaturaleza is null)

                new ApunteDeUnPreasientoDtm
                {
                    IdElemento = pago.IdPreasiento.Entero(),
                    Posicion = enumPosicionContable.Debe,
                    Clase = enumClaseDeApunte.Pag_Acreedor,
                    Tipo = ltrTipoDeApunte.Pago,
                    Orden = orden,
                    Cuenta = cuentaacreedora,
                    Importe = pago.Importe,
                    Concepto = textoDePosicion,
                }.InsertarComoAdministrador(contexto);
            else
                new ApunteDeUnPreasientoDtm
                {
                    IdElemento = pago.IdPreasiento.Entero(),
                    Posicion = enumPosicionContable.Debe,
                    Clase = enumClaseDeApunte.Pag_Gasto,
                    Tipo = ltrTipoDeApunte.Pago,
                    Orden = orden,
                    Cuenta = cuentaDeGasto,
                    Importe = pago.Importe,
                    Concepto = textoDePosicion,
                }.InsertarComoAdministrador(contexto);

            orden += 10;
            var cuentaDePago = pago.CuentaContableDelPago(contexto);
            new ApunteDeUnPreasientoDtm
            {
                IdElemento = pago.IdPreasiento.Entero(),
                Posicion = enumPosicionContable.Haber,
                Clase = enumClaseDeApunte.Pag_Pago,
                Tipo = "",
                Orden = orden,
                Cuenta = cuentaDePago,
                Importe = pago.Importe,
                Concepto = textoDePosicion,
            }.InsertarComoAdministrador(contexto);

            return pago;
        }

        public static MovimientoDeAlmacenDtm Preasentar(this MovimientoDeAlmacenDtm movimiento, ContextoSe contexto, bool generandoPreasientosAntiguos = false)
        {
            var almacen = movimiento.Almacen(contexto);

            if (almacen.Sociedad(contexto).Autonomo)
                return movimiento;

            if (almacen.EstaEnLaEtapa(enumEtapasDeAlmacen.ALM_Etapa_Cancelado) || almacen.EstaEnLaEtapa(enumEtapasDeAlmacen.ALM_Etapa_Cerrado))
                GestorDeErrores.Emitir($"El almacén '{almacen.Referencia}' está en la etapa '{almacen.Etapas().First()}' y no se pueden preasentar movimientos de almacén");

            if (!almacen.Sociedad(contexto).UsaPreasientos(contexto, enumNegocio.Almacen))
                return movimiento;

            var clase = movimiento.TipoDeMovimiento(contexto).ClaseMovimiento;

            switch (clase)
            {
                case enumClaseDeMovimiento.Entrada: movimiento.PreasentarEntradaDeAlmacen(contexto, generandoPreasientosAntiguos); break;
                case enumClaseDeMovimiento.Salida: movimiento.PreasentarSalidaDelmacen(contexto, generandoPreasientosAntiguos); break;
                case enumClaseDeMovimiento.Inicial: movimiento.PreasentarStockInicial(contexto, generandoPreasientosAntiguos); break;
                case enumClaseDeMovimiento.Ajuste: movimiento.PreasentarNuevoPrecioDeUnitario(contexto, generandoPreasientosAntiguos); break;
                case enumClaseDeMovimiento.Regularizacion: movimiento.PreasentarRegularizacion(contexto, generandoPreasientosAntiguos); break;
            }

            // si el movimiento ya existe (se modifica o se barre) el preasiento nuevo se engancha aquí; al insertar aún no hay Id
            // y lo engancha GestorDeMovimientosDeAlmacen.DespuesDePersistir
            if (movimiento.Id > 0 && movimiento.IdPreasiento is not null)
            {
                var preasiento = movimiento.Preasiento(contexto);
                if (preasiento.IdReferenciado != movimiento.Id)
                {
                    preasiento.IdReferenciado = movimiento.Id;
                    preasiento.ModificarComoAdministrador(contexto);
                }
            }

            return movimiento;
        }

        // Entrada de compra (recepción de una línea de pedido en un almacén), como al registrar una factura recibida pero sin impuestos:
        //   Debe:  cuenta de existencias del almacén (311) o, si la sociedad usa la variación de existencias, la de gasto de la naturaleza
        //   Haber: cuenta de albaranes pendientes de recibir factura (VariablesDeCuentas.Albaranes) con la extensión del proveedor
        // por la base imponible de la entrega (cantidad * precio - % de descuento). El resto de entradas aún no se preasientan.
        // El tipo de preasiento es el que tiene como origen el negocio de almacén (parámetro SPR_Negocio_Origen); el referenciado es el
        // movimiento, que al insertarlo aún no tiene Id (lo engancha GestorDeMovimientosDeAlmacen.DespuesDePersistir)
        public static void PreasentarEntradaDeAlmacen(this MovimientoDeAlmacenDtm movimiento, ContextoSe contexto, bool generandoPreasientosAntiguos = false)
        {
            if (movimiento.IdLineaDeRecepcion is null)
                GestorDeErrores.Emitir($"No se puede preasentar la entrada de compra del movimiento de almacén '{movimiento.Referencia(contexto)}' porque no tiene línea de recepción asociada");

            var recepcion = contexto.SeleccionarPorId<RecepcionDeLineaDtm>((int)movimiento.IdLineaDeRecepcion);
            var linea = recepcion.LineaDePedido(contexto);
            var pedido = linea.DetalleDe<PedidoDtm>(contexto);
            movimiento.PreasentarCompraDeAlmacen(contexto, recepcion, Math.Round(recepcion.Importe, 2),
                $"Recepción de la línea nº {linea.Orden} del pedido '{pedido.Referencia}'", esUnaDevolucion: false, generandoPreasientosAntiguos);
        }

        // Devolución de compra (salida de lo devuelto de una entrega en un almacén): el preasiento de la entrada al revés, por el importe devuelto
        // (cantidad devuelta * precio - % de descuento de la entrega).
        // Venta de material (salida de lo vendido en la línea de un documento de venta): baja de existencias valorada al precio medio vigente,
        // al debe la cuenta de variación (61x) y al haber la de existencias (3xx), salvo que el almacén use la variación de existencias
        // (ver PreasentarVariacionDeExistencias). El resto de salidas aún no se preasientan
        public static void PreasentarSalidaDelmacen(this MovimientoDeAlmacenDtm movimiento, ContextoSe contexto, bool generandoPreasientosAntiguos = false)
        {
            if (movimiento.IdLineaDeVenta is not null)
            {
                var lineaDeVenta = contexto.SeleccionarPorId<LineaDeUnaVentaDtm>((int)movimiento.IdLineaDeVenta);
                var venta = lineaDeVenta.Venta(contexto);
                movimiento.PreasentarVariacionDeExistencias(contexto, -(movimiento.Cantidad * movimiento.Precio),
                    $"Venta de la línea nº {lineaDeVenta.Orden} del documento de venta '{venta.Referencia}'", generandoPreasientosAntiguos);
                return;
            }

            if (movimiento.IdLineaDevolucion is null)
                return;

            var devolucion = contexto.SeleccionarPorId<DevolucionDeEntregaDtm>((int)movimiento.IdLineaDevolucion);
            var recepcion = devolucion.Recepcion(contexto);
            var linea = recepcion.LineaDePedido(contexto);
            var pedido = linea.DetalleDe<PedidoDtm>(contexto);
            movimiento.PreasentarCompraDeAlmacen(contexto, recepcion, Math.Round(devolucion.Importe(recepcion), 2),
                $"Devolución de la línea nº {linea.Orden} del pedido '{pedido.Referencia}'", esUnaDevolucion: true, generandoPreasientosAntiguos);
        }

        // Asiento de una compra de almacén, como al registrar una factura recibida pero sin impuestos, con la información contable del almacén
        // (ha de estar definida, ver ExtensorDeAlmacenes.ValidarInformacionContable): en la entrada va al debe
        //  - sin variación de existencias, su cuenta de existencias (3xx)
        //  - con ella, su cuenta de compras (60x) o, en la devolución, la de devoluciones (608); si no las tiene, la de gasto de la naturaleza
        // al haber la de albaranes pendientes de recibir factura (VariablesDeCuentas.Albaranes, 4009) con la extensión del proveedor; en la
        // devolución, al revés. El tipo de preasiento es el que tiene como origen el negocio de almacén (parámetro SPR_Negocio_Origen); el
        // referenciado es el movimiento, que al insertarlo aún no tiene Id (lo engancha GestorDeMovimientosDeAlmacen.DespuesDePersistir)
        private static void PreasentarCompraDeAlmacen(this MovimientoDeAlmacenDtm movimiento, ContextoSe contexto, RecepcionDeLineaDtm recepcion, decimal importe, string concepto, bool esUnaDevolucion, bool generandoPreasientosAntiguos)
        {
            var almacen = movimiento.Almacen(contexto);
            var sociedad = almacen.Sociedad(contexto);
            var linea = recepcion.LineaDePedido(contexto);
            var pedido = linea.DetalleDe<PedidoDtm>(contexto);
            var unitario = movimiento.Unitario(contexto, aplicarJoin: true);
            var conceptoDelDebe = linea.Concepto.IsNullOrEmpty() ? unitario.Expresion : linea.Concepto;

            var (cuentaDelDebe, tipoDelDebe) = (almacen.CuentaDeExistencias(contexto).Codigo, "");
            if (almacen.UsaVariacionDeExistencias)
            {
                var cuentaDelAlmacen = esUnaDevolucion ? almacen.CuentaDeDevoluciones(contexto) : almacen.CuentaDeCompras(contexto);
                if (cuentaDelAlmacen is not null)
                    (cuentaDelDebe, tipoDelDebe) = (cuentaDelAlmacen.Codigo, ltrTipoDeApunte.Gasto);
                else
                {
                    var naturaleza = unitario.Naturaleza(contexto, aplicarJoin: true);
                    if (naturaleza.IdCuentaDeGasto == null)
                        GestorDeErrores.Emitir($"Ha de indicar la cuenta del gasto para la naturaleza '{naturaleza.Expresion}' o la cuenta de {(esUnaDevolucion ? "devoluciones" : "compras")} del almacén '{almacen.Referencia}'");
                    // la naturaleza puede venir de la caché del unitario sin su cuenta de gasto cargada (p.e. al barrer): se lee aparte
                    (cuentaDelDebe, tipoDelDebe) = (naturaleza.CuentaDeGasto(contexto).Codigo, ltrTipoDeApunte.Gasto);
                }
            }

            movimiento.Preasiento = PreasentarCompra(contexto, enumNegocio.Almacen, movimiento.Id, almacen.IdCg, sociedad, movimiento.RealizadoEl,
                pedido, linea, cuentaDelDebe, tipoDelDebe, conceptoDelDebe, importe, concepto, esUnaDevolucion, generandoPreasientosAntiguos);
            movimiento.IdPreasiento = movimiento.Preasiento.Id;
        }

        // Entrega de una línea de pedido en la obra de un expediente: como la entrada de compra en un almacén (ver PreasentarCompraDeAlmacen)
        // pero lo entregado se consume en la obra, al debe va la cuenta de gasto (grupo 6) de la naturaleza de la línea en lugar de la de
        // existencias, y al haber la de albaranes (4009), por el importe de la entrega. Sólo si la sociedad
        // del pedido genera preasientos de pedidos (SPR_Generar_Preasiento_De_Pedido). Se llama antes de insertar la entrega, que aún no
        // tiene Id, por eso se pasan la línea y el pedido (ver GestorDeEntregasDeUnPedido.AntesDePersistir)
        public static RecepcionDeLineaDtm Preasentar(this RecepcionDeLineaDtm recepcion, ContextoSe contexto, LineaDeUnPedidoDtm linea, PedidoDtm pedido, bool generandoPreasientosAntiguos = false)
        {
            if (!recepcion.VaAlExpediente)
                return recepcion;

            var expediente = contexto.SeleccionarPorId<ExpedienteDtm>((int)recepcion.IdExpediente);
            recepcion.Preasiento = pedido.PreasentarCompraEnObra(contexto, expediente, linea, recepcion.RecepcionadaEl, Math.Round(recepcion.Importe, 2),
                $"Entrega de la línea nº {linea.Orden} del pedido '{pedido.Referencia}' en la obra del expediente '{expediente.Referencia}'", esUnaDevolucion: false, generandoPreasientosAntiguos);
            recepcion.IdPreasiento = recepcion.Preasiento?.Id;
            return recepcion;
        }

        // Devolución de una entrega en la obra de un expediente: el preasiento de la entrega al revés, por el importe devuelto (cantidad
        // devuelta * precio - % de descuento de la entrega)
        public static DevolucionDeEntregaDtm Preasentar(this DevolucionDeEntregaDtm devolucion, ContextoSe contexto, RecepcionDeLineaDtm recepcion, LineaDeUnPedidoDtm linea, PedidoDtm pedido, bool generandoPreasientosAntiguos = false)
        {
            if (!recepcion.VaAlExpediente)
                return devolucion;

            var expediente = contexto.SeleccionarPorId<ExpedienteDtm>((int)recepcion.IdExpediente);
            devolucion.Preasiento = pedido.PreasentarCompraEnObra(contexto, expediente, linea, devolucion.DevueltaEl, Math.Round(devolucion.Importe(recepcion), 2),
                $"Devolución de la línea nº {linea.Orden} del pedido '{pedido.Referencia}' entregada en la obra del expediente '{expediente.Referencia}'", esUnaDevolucion: true, generandoPreasientosAntiguos);
            devolucion.IdPreasiento = devolucion.Preasiento?.Id;
            return devolucion;
        }

        // El negocio referenciado es el pedido y el referenciado su Id: un pedido tiene varias entregas y devoluciones en obra, cada una con su
        // preasiento, y el documento que lo generó se busca por su IdPreasiento (ver Referenciado). Devuelve null si no se preasienta.
        // La cuenta de gasto de un material (naturaleza de clase Material) es la de compra directa del tipo del expediente si la tiene; la del
        // resto (servicios, mano de obra) y la de los materiales sin ella, la de gasto de la naturaleza
        private static PreasientoDtm PreasentarCompraEnObra(this PedidoDtm pedido, ContextoSe contexto, ExpedienteDtm expediente, LineaDeUnPedidoDtm linea, DateTime fecha, decimal importe, string concepto, bool esUnaDevolucion, bool generandoPreasientosAntiguos)
        {
            var sociedad = pedido.Sociedad(contexto);
            if (sociedad.Autonomo || !sociedad.UsaPreasientos(contexto, enumNegocio.Pedido))
                return null;

            var naturaleza = linea.Naturaleza(contexto, aplicarJoin: true);
            if (naturaleza is null)
                GestorDeErrores.Emitir($"Para preasentar la {(esUnaDevolucion ? "devolución" : "entrega")} de la línea {linea.Orden} del pedido '{pedido.Referencia}' ésta ha de tener naturaleza");

            var cuentaDeGasto = naturaleza.Clase == enumClaseUnitario.Material
                ? ExtensorDeCuentas.LeerCuenta(contexto, contexto.SeleccionarPorId<TipoDeExpedienteDtm>(expediente.IdTipo).IdCuentaDeCompraDirecta)
                : null;

            if (cuentaDeGasto is null)
            {
                if (naturaleza.IdCuentaDeGasto == null)
                    GestorDeErrores.Emitir($"Ha de indicar la cuenta del gasto para la naturaleza '{naturaleza.Expresion}'" +
                                           (naturaleza.Clase == enumClaseUnitario.Material ? " o la cuenta de compra directa del tipo del expediente" : ""));
                // la naturaleza puede venir de la caché sin su cuenta de gasto cargada (p.e. al barrer): se lee aparte
                cuentaDeGasto = naturaleza.CuentaDeGasto(contexto);
            }

            return PreasentarCompra(contexto, enumNegocio.Pedido, pedido.Id, pedido.IdCg, sociedad, fecha,
                pedido, linea, cuentaDeGasto.Codigo, ltrTipoDeApunte.Gasto, linea.Concepto.IsNullOrEmpty() ? naturaleza.Expresion : linea.Concepto, importe, concepto, esUnaDevolucion, generandoPreasientosAntiguos);
        }

        // Asiento de una compra (entrega o devolución de una línea de pedido) en un almacén o en la obra de un expediente, como al registrar una
        // factura recibida pero sin impuestos: en la entrega va al debe la cuenta indicada (existencias en el almacén, gasto de la naturaleza en
        // la obra) y al haber la de albaranes pendientes de recibir factura (VariablesDeCuentas.Albaranes) con la extensión del proveedor; en la
        // devolución, al revés. El tipo de preasiento es el que tiene como origen el negocio indicado (parámetro SPR_Negocio_Origen). Los apuntes
        // son de las clases Alm_Gasto (la cuenta del debe) y Alm_Albaran en ambos casos, así se exportan igual (ver LoteContableConNcs.AsientoDeCompra)
        private static PreasientoDtm PreasentarCompra(ContextoSe contexto, enumNegocio negocio, int idReferenciado, int idCg, SociedadDtm sociedad, DateTime fecha,
            PedidoDtm pedido, LineaDeUnPedidoDtm linea, string cuentaDelDebe, string tipoDelDebe, string conceptoDelDebe, decimal importe, string concepto, bool esUnaDevolucion, bool generandoPreasientosAntiguos)
        {
            var proveedor = pedido.Proveedor(contexto, aplicarJoin: true);
            if (proveedor.CodigoContable is null)
                GestorDeErrores.Emitir($"Para preasentar la {(esUnaDevolucion ? "devolución" : "entrada")} de compra del pedido '{pedido.Referencia}' el proveedor '{proveedor.Expresion}' ha de tener código contable");

            var idTipoPreasiento = VariablesDePreasiento.IdTipoPreasientoDelNegocio(negocio.IdNegocio(), negocio.Singular());
            var tipoDePreasiento = enumNegocio.Preasiento.Tipos(contexto).FirstOrDefault(x => x.Id == idTipoPreasiento);
            if (tipoDePreasiento == null)
                GestorDeErrores.Emitir($"El id del tipo del preasiento parametrizado en '{idTipoPreasiento}' no existe en la BD");

            VariablesDePreasiento.CodigoDeDiario(tipoDePreasiento, contexto);
            VariablesDePreasiento.ValidarNaturalezasContables();

            var preasiento = new PreasientoDtm
            {
                IdCg = idCg,
                IdTipo = idTipoPreasiento,
                Nombre = concepto.Left(IDominio.Longitud(IDominio.VARCHAR_250) - 1),
                Ejercicio = fecha.Year,
                CodigoDiario = tipoDePreasiento.CodigoDeDiario(contexto),
                SociedadContable = sociedad.CodigoContable,
                NegocioReferenciado = negocio,
                IdReferenciado = idReferenciado,
                FechaContable = fecha.Date
            }.InsertarComoAdministrador(contexto, parametros: ParametrosDeInsercion(generandoPreasientosAntiguos));

            var gasto = new ApunteDeUnPreasientoDtm
            {
                IdElemento = preasiento.Id,
                Posicion = esUnaDevolucion ? enumPosicionContable.Haber : enumPosicionContable.Debe,
                Clase = enumClaseDeApunte.Alm_Gasto,
                Tipo = tipoDelDebe,
                Cuenta = cuentaDelDebe,
                Importe = importe,
                Concepto = conceptoDelDebe.Left(IDominio.Longitud(IDominio.VARCHAR_250) - 1),
            };

            var albaran = new ApunteDeUnPreasientoDtm
            {
                IdElemento = preasiento.Id,
                Posicion = esUnaDevolucion ? enumPosicionContable.Debe : enumPosicionContable.Haber,
                Clase = enumClaseDeApunte.Alm_Albaran,
                Tipo = "",
                Cuenta = VariablesDeCuentas.Albaranes + proveedor.CodigoContable.ToString().PadLeft(4, '0'),
                Importe = importe,
                Concepto = $"{proveedor.NIF(contexto)} - {pedido.Referencia}: {linea.Orden}".Left(IDominio.Longitud(IDominio.VARCHAR_250) - 1),
            };

            // primero el apunte del debe
            var (primero, segundo) = esUnaDevolucion ? (albaran, gasto) : (gasto, albaran);
            primero.Orden = 10;
            segundo.Orden = 20;
            primero.InsertarComoAdministrador(contexto);
            segundo.InsertarComoAdministrador(contexto);
            return preasiento;
        }

        // El referenciado de un preasiento de pedidos no es el pedido sino la entrega o la devolución en obra que lo lleva (ver
        // PreasentarCompraEnObra); null si se ha cancelado (se borra) y no se exige que lo haya
        private static IUsaPreasiento CompraEnObra(ContextoSe contexto, PreasientoDtm spr, bool errorSiNoHay)
        {
            IUsaPreasiento compra = contexto.Set<RecepcionDeLineaDtm>().FirstOrDefault(x => x.IdPreasiento == spr.Id)?.CalcularReferencia(contexto);
            compra ??= contexto.Set<DevolucionDeEntregaDtm>().FirstOrDefault(x => x.IdPreasiento == spr.Id)?.CalcularReferencia(contexto);
            if (compra is null && errorSiNoHay)
                GestorDeErrores.Emitir($"No se encuentra la entrega ni la devolución en obra del preasiento '{spr.Referencia}', se ha cancelado");
            return compra;
        }

        // Fecha en la que se entregó o se devolvió en la obra lo preasentado: la del documento de un preasiento de pedidos
        public static DateTime FechaDeLaCompraEnObra(this PreasientoDtm spr, ContextoSe contexto) => spr.Referenciado(contexto) switch
        {
            RecepcionDeLineaDtm recepcion => recepcion.RecepcionadaEl.Date,
            DevolucionDeEntregaDtm devolucion => devolucion.DevueltaEl.Date,
            _ => throw new Exception($"El preasiento '{spr.Referencia}' no es de una entrega ni de una devolución en obra")
        };

        // Apunte de gasto o de ingreso de un expediente, como un pago o un cobro por naturaleza pero sin tercero: el gasto va al debe en la
        // cuenta de gasto de su naturaleza y al haber en la de partidas pendientes de aplicación (VariablesDeCuentas.PendientesDeAplicacion); el
        // ingreso al haber en la cuenta de ingreso de su naturaleza y al debe en la de pendientes de aplicación. Sólo si la sociedad del expediente genera
        // preasientos de expedientes (SPR_Generar_Preasiento_De_Expediente). Como en las entregas en obra, el negocio referenciado es el
        // expediente y el documento que lo generó se busca por su IdPreasiento (ver ApunteDeExpedienteDelPreasiento). Se llama antes de
        // insertar o modificar el apunte (ver GestorDeApuntesDeExpediente.AntesDePersistir)
        public static ApunteDeExpedienteDtm Preasentar(this ApunteDeExpedienteDtm apunte, ContextoSe contexto, bool generandoPreasientosAntiguos = false)
        {
            var expediente = contexto.SeleccionarPorId<ExpedienteDtm>(apunte.IdElemento);
            var sociedad = expediente.Sociedad(contexto);
            if (sociedad.Autonomo || !sociedad.UsaPreasientos(contexto, enumNegocio.Expediente))
                return apunte;

            var esUnGasto = apunte.Clase == enumApunteDeExpediente.Gasto;
            var naturaleza = contexto.SeleccionarPorId<NaturalezaDtm>(apunte.IdNaturaleza);
            var cuentaDeLaNaturaleza = esUnGasto ? naturaleza.CuentaDeGasto(contexto) : naturaleza.CuentaDeIngreso(contexto);

            var idTipoPreasiento = VariablesDePreasiento.IdTipoPreasientoDelNegocio(enumNegocio.Expediente.IdNegocio(), enumNegocio.Expediente.Singular());
            var tipoDePreasiento = enumNegocio.Preasiento.Tipos(contexto).FirstOrDefault(x => x.Id == idTipoPreasiento);
            if (tipoDePreasiento == null)
                GestorDeErrores.Emitir($"El id del tipo del preasiento parametrizado en '{idTipoPreasiento}' no existe en la BD");

            VariablesDePreasiento.CodigoDeDiario(tipoDePreasiento, contexto);
            VariablesDePreasiento.ValidarNaturalezasContables();

            var preasiento = new PreasientoDtm
            {
                IdCg = expediente.IdCg,
                IdTipo = idTipoPreasiento,
                Nombre = $"{(esUnGasto ? "Gasto" : "Ingreso")} nº {apunte.Orden} del expediente '{expediente.Referencia}': {apunte.Concepto}".Left(IDominio.Longitud(IDominio.VARCHAR_250) - 1),
                Ejercicio = apunte.ImputadoEl.Year,
                CodigoDiario = tipoDePreasiento.CodigoDeDiario(contexto),
                SociedadContable = sociedad.CodigoContable,
                NegocioReferenciado = enumNegocio.Expediente,
                IdReferenciado = expediente.Id,
                FechaContable = apunte.ImputadoEl.Date
            }.InsertarComoAdministrador(contexto, parametros: ParametrosDeInsercion(generandoPreasientosAntiguos));

            var importe = Math.Round(apunte.Valor, 2);
            var deLaNaturaleza = new ApunteDeUnPreasientoDtm
            {
                IdElemento = preasiento.Id,
                Posicion = esUnGasto ? enumPosicionContable.Debe : enumPosicionContable.Haber,
                Clase = enumClaseDeApunte.Exp_Naturaleza,
                Tipo = esUnGasto ? ltrTipoDeApunte.Gasto : ltrTipoDeApunte.Ingreso,
                Cuenta = cuentaDeLaNaturaleza.Codigo,
                Importe = importe,
                Concepto = apunte.Concepto.Left(IDominio.Longitud(IDominio.VARCHAR_250) - 1),
            };

            var contrapartida = new ApunteDeUnPreasientoDtm
            {
                IdElemento = preasiento.Id,
                Posicion = esUnGasto ? enumPosicionContable.Haber : enumPosicionContable.Debe,
                Clase = enumClaseDeApunte.Exp_Contrapartida,
                Tipo = "",
                Cuenta = VariablesDeCuentas.PendientesDeAplicacion,
                Importe = importe,
                Concepto = $"{expediente.Referencia}: {apunte.Concepto}".Left(IDominio.Longitud(IDominio.VARCHAR_250) - 1),
            };

            // primero el apunte del debe
            var (primero, segundo) = esUnGasto ? (deLaNaturaleza, contrapartida) : (contrapartida, deLaNaturaleza);
            primero.Orden = 10;
            segundo.Orden = 20;
            primero.InsertarComoAdministrador(contexto);
            segundo.InsertarComoAdministrador(contexto);

            apunte.Preasiento = preasiento;
            apunte.IdPreasiento = preasiento.Id;
            return apunte;
        }

        // El referenciado de un preasiento de expedientes no es el expediente sino el apunte que lo lleva (ver Preasentar del apunte); null
        // si el apunte se ha eliminado y no se exige que lo haya
        private static ApunteDeExpedienteDtm ApunteDeExpedienteDelPreasiento(ContextoSe contexto, PreasientoDtm spr, bool errorSiNoHay)
        {
            var apunte = contexto.Set<ApunteDeExpedienteDtm>().FirstOrDefault(x => x.IdPreasiento == spr.Id)?.CalcularReferencia(contexto);
            if (apunte is null && errorSiNoHay)
                GestorDeErrores.Emitir($"No se encuentra el apunte de expediente del preasiento '{spr.Referencia}', se ha eliminado");
            return apunte;
        }

        // Fecha en la que se imputó el apunte de gasto o ingreso de un preasiento de expedientes
        public static DateTime FechaDelApunteDeExpediente(this PreasientoDtm spr, ContextoSe contexto) =>
            ((ApunteDeExpedienteDtm)spr.Referenciado(contexto)).ImputadoEl.Date;

        // Stock inicial: el de un inventario inicial no se preasienta (son los saldos de apertura, ver ExtensorDeAlmacenes.MovimientosDeApertura).
        // Si lo crea el recuento de un material que no tenía movimientos, es un aumento de existencias como cualquier regularización: por el
        // valor del stock encontrado (ver PreasentarVariacionDeExistencias)
        public static void PreasentarStockInicial(this MovimientoDeAlmacenDtm movimiento, ContextoSe contexto, bool generandoPreasientosAntiguos = false)
        {
            if (movimiento.IdLineaInventario is null)
                return;

            var regularizacion = contexto.SeleccionarPorId<LineasDeUnaRegularizacionDtm>((int)movimiento.IdLineaInventario).DetalleDe<RegularizacionDtm>(contexto);
            if (((TipoDeRegularizacionDtm)regularizacion.Tipo(contexto)).ClaseDeRegularizacion == enumRegularizacionAlm.Inicial)
                return;

            movimiento.PreasentarVariacionDeExistencias(contexto, movimiento.Stock * movimiento.Valor,
                $"Stock encontrado en el recuento '{regularizacion.Referencia}'", generandoPreasientosAntiguos);
        }

        // Ajuste del precio medio: la diferencia de valor es el stock por lo que cambia el precio medio
        public static void PreasentarNuevoPrecioDeUnitario(this MovimientoDeAlmacenDtm movimiento, ContextoSe contexto, bool generandoPreasientosAntiguos = false)
        {
            var anterior = movimiento.ObtenerMovimientoAnterior(contexto);
            if (anterior is null)
                return;

            movimiento.PreasentarVariacionDeExistencias(contexto, movimiento.Stock * (movimiento.Valor - anterior.Valor),
                $"Ajuste del precio medio de {anterior.Valor:0.######} a {movimiento.Valor:0.######}", generandoPreasientosAntiguos);
        }

        // Regularización de stock: el movimiento lleva el stock contado (Cantidad) y se valora al precio medio vigente, así que la diferencia de
        // valor es lo que aumenta o disminuye el stock por ese precio
        public static void PreasentarRegularizacion(this MovimientoDeAlmacenDtm movimiento, ContextoSe contexto, bool generandoPreasientosAntiguos = false)
        {
            var anterior = movimiento.ObtenerMovimientoAnterior(contexto);
            if (anterior is null)
                return;

            movimiento.PreasentarVariacionDeExistencias(contexto, (movimiento.Stock - anterior.Stock) * anterior.Valor,
                $"Regularización del stock de {anterior.Stock:0.######} a {movimiento.Stock:0.######}", generandoPreasientosAntiguos);
        }

        // Asiento de una regularización de existencias, entre las cuentas de existencias (3xx, clase Alm_Existencias) y de variación de
        // existencias (61x, clase Alm_Variacion) del almacén: si el valor sube, la de existencias va al debe y la de variación al haber; si
        // baja, al revés, por el valor absoluto. No se preasienta si no cambia el valor ni si el almacén usa la variación de existencias (sus
        // existencias se cuadran al cerrar, ver CuadreDeExistencias). Como en el resto de preasientos de almacén, el referenciado es el
        // movimiento, que al insertarlo aún no tiene Id (lo engancha GestorDeMovimientosDeAlmacen.DespuesDePersistir), y la fecha contable la
        // del movimiento
        private static void PreasentarVariacionDeExistencias(this MovimientoDeAlmacenDtm movimiento, ContextoSe contexto, decimal variacion, string concepto, bool generandoPreasientosAntiguos)
        {
            var importe = Math.Round(Math.Abs(variacion), 2);
            if (importe == 0)
                return;

            var almacen = movimiento.Almacen(contexto);
            var sociedad = almacen.Sociedad(contexto);
            almacen.ValidarInformacionContable(contexto);
            if (almacen.UsaVariacionDeExistencias)
                return;

            var existencias = almacen.CuentaDeExistencias(contexto).Codigo;
            var variacionDeExistencias = almacen.CuentaDeVariacion(contexto).Codigo;

            var idTipoPreasiento = VariablesDePreasiento.IdTipoPreasientoDelNegocio(enumNegocio.Almacen.IdNegocio(), enumNegocio.Almacen.Singular());
            var tipoDePreasiento = enumNegocio.Preasiento.Tipos(contexto).FirstOrDefault(x => x.Id == idTipoPreasiento);
            if (tipoDePreasiento == null)
                GestorDeErrores.Emitir($"El id del tipo del preasiento parametrizado en '{idTipoPreasiento}' no existe en la BD");

            VariablesDePreasiento.CodigoDeDiario(tipoDePreasiento, contexto);
            VariablesDePreasiento.ValidarNaturalezasContables();

            var unitario = movimiento.Unitario(contexto, aplicarJoin: true);
            var nombre = $"{concepto} del material '{unitario.Referencia}' en el almacén '{almacen.Referencia}'";
            var preasiento = new PreasientoDtm
            {
                IdCg = almacen.IdCg,
                IdTipo = idTipoPreasiento,
                Nombre = nombre.Left(IDominio.Longitud(IDominio.VARCHAR_250) - 1),
                Ejercicio = movimiento.RealizadoEl.Year,
                CodigoDiario = tipoDePreasiento.CodigoDeDiario(contexto),
                SociedadContable = sociedad.CodigoContable,
                NegocioReferenciado = enumNegocio.Almacen,
                IdReferenciado = movimiento.Id,
                FechaContable = movimiento.RealizadoEl.Date
            }.InsertarComoAdministrador(contexto, parametros: ParametrosDeInsercion(generandoPreasientosAntiguos));

            var existenciasAlDebe = variacion > 0;
            var conceptoDelApunte = $"{unitario.Referencia}: {concepto}".Left(IDominio.Longitud(IDominio.VARCHAR_250) - 1);
            // primero el apunte del debe
            new ApunteDeUnPreasientoDtm
            {
                IdElemento = preasiento.Id,
                Orden = 10,
                Posicion = enumPosicionContable.Debe,
                Clase = existenciasAlDebe ? enumClaseDeApunte.Alm_Existencias : enumClaseDeApunte.Alm_Variacion,
                Tipo = "",
                Cuenta = existenciasAlDebe ? existencias : variacionDeExistencias,
                Importe = importe,
                Concepto = conceptoDelApunte,
            }.InsertarComoAdministrador(contexto);

            new ApunteDeUnPreasientoDtm
            {
                IdElemento = preasiento.Id,
                Orden = 20,
                Posicion = enumPosicionContable.Haber,
                Clase = existenciasAlDebe ? enumClaseDeApunte.Alm_Variacion : enumClaseDeApunte.Alm_Existencias,
                Tipo = "",
                Cuenta = existenciasAlDebe ? variacionDeExistencias : existencias,
                Importe = importe,
                Concepto = conceptoDelApunte,
            }.InsertarComoAdministrador(contexto);

            movimiento.Preasiento = preasiento;
            movimiento.IdPreasiento = preasiento.Id;
        }

        // Cuadre de las existencias de un almacén que usa la variación de existencias al cerrar su sociedad (ver
        // GestoresDeNegocio.Contabilidad.CuadreDeExistencias). Cada apunte es (cuenta del debe, cuenta del haber, importe, concepto) entre
        // las cuentas de existencias (3xx, clase Alm_Existencias) y de variación de existencias (61x, clase Alm_Variacion) del almacén; los de
        // importe 0 no se anotan y, si no queda ninguno, no se crea. No hay documento contable: el referenciado es la sociedad, el tipo el de
        // almacén y la fecha la indicada (hoy si no se indica). Devuelve null si no se crea
        public static PreasientoDtm PreasentarCuadreDeExistencias(this AlmacenDtm almacen, ContextoSe contexto, SociedadDtm sociedad, string nombre,
            List<(bool existenciasAlDebe, decimal importe, string concepto)> cuadres, DateTime? fechaContable = null)
        {
            cuadres = cuadres.Where(c => c.importe != 0).ToList();
            if (cuadres.Count == 0)
                return null;

            var existencias = almacen.CuentaDeExistencias(contexto).Codigo;
            var variacionDeExistencias = almacen.CuentaDeVariacion(contexto).Codigo;

            var idTipoPreasiento = VariablesDePreasiento.IdTipoPreasientoDelNegocio(enumNegocio.Almacen.IdNegocio(), enumNegocio.Almacen.Singular());
            var tipoDePreasiento = enumNegocio.Preasiento.Tipos(contexto).FirstOrDefault(x => x.Id == idTipoPreasiento);
            if (tipoDePreasiento == null)
                GestorDeErrores.Emitir($"El id del tipo del preasiento parametrizado en '{idTipoPreasiento}' no existe en la BD");

            var fecha = (fechaContable ?? DateTime.Today).Date;
            var preasiento = new PreasientoDtm
            {
                IdCg = almacen.IdCg,
                IdTipo = idTipoPreasiento,
                Nombre = nombre.Left(IDominio.Longitud(IDominio.VARCHAR_250) - 1),
                Ejercicio = fecha.Year,
                CodigoDiario = tipoDePreasiento.CodigoDeDiario(contexto),
                SociedadContable = sociedad.CodigoContable,
                NegocioReferenciado = enumNegocio.Sociedad,
                IdReferenciado = sociedad.Id,
                FechaContable = fecha
            }.InsertarComoAdministrador(contexto, parametros: ParametrosDeInsercion(false));

            var orden = 0;
            foreach (var (existenciasAlDebe, importe, concepto) in cuadres)
            {
                var conceptoDelApunte = concepto.Left(IDominio.Longitud(IDominio.VARCHAR_250) - 1);
                // primero el apunte del debe
                new ApunteDeUnPreasientoDtm
                {
                    IdElemento = preasiento.Id,
                    Orden = orden += 10,
                    Posicion = enumPosicionContable.Debe,
                    Clase = existenciasAlDebe ? enumClaseDeApunte.Alm_Existencias : enumClaseDeApunte.Alm_Variacion,
                    Tipo = "",
                    Cuenta = existenciasAlDebe ? existencias : variacionDeExistencias,
                    Importe = importe,
                    Concepto = conceptoDelApunte,
                }.InsertarComoAdministrador(contexto);

                new ApunteDeUnPreasientoDtm
                {
                    IdElemento = preasiento.Id,
                    Orden = orden += 10,
                    Posicion = enumPosicionContable.Haber,
                    Clase = existenciasAlDebe ? enumClaseDeApunte.Alm_Variacion : enumClaseDeApunte.Alm_Existencias,
                    Tipo = "",
                    Cuenta = existenciasAlDebe ? variacionDeExistencias : existencias,
                    Importe = importe,
                    Concepto = conceptoDelApunte,
                }.InsertarComoAdministrador(contexto);
            }
            return preasiento;
        }

        // El cuadre de existencias es el único preasiento referenciado a una sociedad: no tiene documento contable (ver PreasentarCuadreDeExistencias)
        public static bool EsUnCuadreDeExistencias(this PreasientoDtm spr) => spr.NegocioReferenciado == enumNegocio.Sociedad;

        // Nº de asientos del lote del cierre por negocio de origen (el referenciado de sus preasientos); se cachea por cierre, ya que su
        // lote no cambia mientras el cierre existe
        public static Dictionary<enumNegocio, int> AsientosPorOrigen(this CierreContableDtm cierre, ContextoSe contexto)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Cierre_AsientosPorOrigen);
            if (!cache.ContainsKey(cierre.Id.ToString()))
            {
                cache[cierre.Id.ToString()] = contexto.Set<CircuitoDocDeUnPreasientoDtm>()
                    .Where(v => v.idElemento2 == cierre.IdLote)
                    .Join(contexto.Set<PreasientoDtm>(), v => v.idElemento1, p => p.Id, (v, p) => p.NegocioReferenciado)
                    .GroupBy(negocio => negocio)
                    .Select(g => new { Negocio = g.Key, Cantidad = g.Count() })
                    .ToList()
                    .ToDictionary(a => a.Negocio, a => a.Cantidad);
            }
            return (Dictionary<enumNegocio, int>)cache[cierre.Id.ToString()];
        }

        // Al regenerar un lote contable, los asientos por origen de su cierre (si lo tiene) se han de volver a calcular
        public static void EliminarCacheDeAsientosPorOrigen(ContextoSe contexto, int idLote)
        {
            var idCierre = contexto.Set<CierreContableDtm>().Where(c => c.IdLote == idLote).Select(c => (int?)c.Id).FirstOrDefault();
            if (idCierre is not null)
                ServicioDeCaches.EliminarElemento(CacheDe.Cierre_AsientosPorOrigen, idCierre.ToString());
        }

        public static IUsaPreasiento Referenciado(this PreasientoDtm spr, ContextoSe contexto, bool errorSiNoHay = true)
        {
            // el cuadre de existencias no tiene documento contable, su referenciado es la sociedad
            if (spr.NegocioReferenciado == enumNegocio.No_Definido || spr.EsUnCuadreDeExistencias()) return null;
            var cache = ServicioDeCaches.Obtener(CacheDe.Spr_Referenciado);
            if (!cache.ContainsKey(spr.Id.ToString()))
            {
                // el referenciado de un almacén no es el almacén sino uno de sus movimientos (ver PreasentarEntradaDeAlmacen), y el de un
                // pedido una de sus entregas o devoluciones en obra (ver CompraEnObra)
                cache[spr.Id.ToString()] = spr.NegocioReferenciado == enumNegocio.Almacen
                    ? contexto.SeleccionarPorId<MovimientoDeAlmacenDtm>(spr.IdReferenciado.Entero(), errorSiNoHay: errorSiNoHay)
                    : spr.NegocioReferenciado == enumNegocio.Pedido
                    ? CompraEnObra(contexto, spr, errorSiNoHay)
                    : spr.NegocioReferenciado == enumNegocio.Expediente
                    ? ApunteDeExpedienteDelPreasiento(contexto, spr, errorSiNoHay)
                    : (IUsaPreasiento)spr.NegocioReferenciado.SeleccionarPorId(contexto, spr.IdReferenciado.Entero(), parametros: new Dictionary<string, object>
                     {
                         {ltrParametrosNeg.ErrorSiNoLoHay, errorSiNoHay }
                     });
            }
            return (IUsaPreasiento)cache[spr.Id.ToString()];
        }

        public static bool HayQueCancelarlo(this PreasientoDtm spr, ContextoSe contexto)
        {
            // un movimiento de almacén no tiene etapas: mientras existe es contabilizable; si se ha eliminado (al anular la entrada
            // de compra) y su preasiento quedó pendiente, se cancela (ver CancelarPreasientoPorEstarloSuReferenciado)
            // igual con las entregas y devoluciones en obra de un pedido, que se borran al cancelarlas
            // y con los apuntes de gastos e ingresos de un expediente, que al eliminarlos cancelan su preasiento
            if (spr.NegocioReferenciado == enumNegocio.Almacen || spr.NegocioReferenciado == enumNegocio.Pedido || spr.NegocioReferenciado == enumNegocio.Expediente)
                return spr.Referenciado(contexto, errorSiNoHay: false) is null;

            // el cuadre de existencias no depende de ningún documento: siempre es contabilizable
            if (spr.EsUnCuadreDeExistencias())
                return false;

            var referenciado = spr.Referenciado(contexto);

            if (spr.NegocioReferenciado == enumNegocio.FacturaRecibida)
            {
                return ((FacturaRecDtm)referenciado).EstaEnEtapaNoContabilizada();
            }
            if (spr.NegocioReferenciado == enumNegocio.FacturaEmitida)
            {
                return ((FacturaEmtDtm)referenciado).EstaEnEtapaNoContabilizada();
            }
            if (spr.NegocioReferenciado == enumNegocio.Pago)
            {
                return ((PagoDtm)referenciado).EstaEnEtapaNoContabilizada();
            }
            if (spr.NegocioReferenciado == enumNegocio.Cobro)
            {
                var fae = ((CobroDeFaeDtm)referenciado).Factura(contexto);
                return fae.EstaEnEtapaNoContabilizada();
            }
            throw new Exception($"Los elementos del negocio '{spr.NegocioReferenciado}' no usan preasientos");
        }

        public static ProveedorDtm Proveedor(this PreasientoDtm spr, ContextoSe contexto, bool errorSiNoHay = true)
        {
            if (spr.NegocioReferenciado == enumNegocio.No_Definido)
                return null;

            if (spr.NegocioReferenciado == enumNegocio.FacturaRecibida)
                return contexto.SeleccionarPorId<ProveedorDtm>(((FacturaRecDtm)spr.Referenciado(contexto)).IdProveedor);

            if (spr.NegocioReferenciado == enumNegocio.Pago)
            {
                var pago = (PagoDtm)spr.Referenciado(contexto);
                if (pago.IdProveedor is not null)
                {
                    return contexto.SeleccionarPorId<ProveedorDtm>(pago.IdProveedor.Entero());
                }
            }

            // entrada de compra: el proveedor es el del pedido de la línea recepcionada (ver PreasentarEntradaDeAlmacen)
            if (spr.NegocioReferenciado == enumNegocio.Almacen)
            {
                var movimiento = (MovimientoDeAlmacenDtm)spr.Referenciado(contexto);
                if (movimiento.IdLineaDeRecepcion is not null)
                {
                    var recepcion = contexto.SeleccionarPorId<RecepcionDeLineaDtm>((int)movimiento.IdLineaDeRecepcion);
                    var pedido = recepcion.LineaDePedido(contexto).DetalleDe<PedidoDtm>(contexto);
                    return pedido.Proveedor(contexto, aplicarJoin: true);
                }
            }

            // entrega o devolución en obra: el referenciado es el pedido (ver PreasentarCompraEnObra)
            if (spr.NegocioReferenciado == enumNegocio.Pedido)
                return contexto.SeleccionarPorId<PedidoDtm>(spr.IdReferenciado.Entero()).Proveedor(contexto, aplicarJoin: true);

            if (errorSiNoHay)
                GestorDeErrores.Emitir($"El Asiento '{spr.Referencia}' no es de proveedores");

            return null;
        }

        public static ClienteDtm Cliente(this PreasientoDtm spr, ContextoSe contexto, bool errorSiNoHay = true)
        {
            if (spr.NegocioReferenciado == enumNegocio.No_Definido)
                return null;

            if (spr.NegocioReferenciado == enumNegocio.FacturaEmitida)
                return contexto.SeleccionarPorId<ClienteDtm>(((FacturaEmtDtm)spr.Referenciado(contexto)).IdCliente);

            if (errorSiNoHay)
                GestorDeErrores.Emitir($"El Asiento '{spr.Referencia}' no es de proveedores");

            return null;
        }

        public static string DimeSiEstaBien(this PreasientoDtm preasiento, ContextoSe contexto)
        {
            //var lineas = factura.Detalles<LineaDeUnaFarDtm>(contexto, errorSiNoHay: false);
            //if (lineas.Count == 0) return $"La factura '{factura.Referencia}' ha de tener el detalle de lo facturado";

            //if (factura.HayDiferenciaConLaBi(contexto))
            //    return $"La factura '{factura.Referencia}' no tiene la misma BI indicada en la cabecera '{factura.BaseImponible.ToMoneda()}' que en el detalle '{factura.Total(contexto, enumImporteFar.BaseImponible).ToMoneda()}'";

            //if (factura.HayDiferenciaConElTotalPagar(contexto))
            //    return $"La factura '{factura.Referencia}' indica un total a pagar '{factura.TotalDelPago.ToMoneda()}' que no coincide con el indicado en sus líneas '{factura.Total(contexto, enumImporteFar.TotalPagar).ToMoneda()}'";

            //if (factura.IdArchivo is null)
            //    return $"La factura '{factura.Referencia}' no se le ha asociado el archivo de la factura";

            //if (factura.FacturadaEl == DateTime.MinValue)
            //    return $"La fecha de emisión '{factura.FacturadaEl.ToString("dd/MM/yyyy")}' de la factura '{factura.Referencia}' no es válida";

            //if (factura.FacturadaEl.Year < DateTime.Now.Year - 1)
            //    return $"La factura '{factura.Referencia}' se ha facturado con año '{factura.FacturadaEl.Year}', no se puede facturar con fecha anterior al año '{DateTime.Now.Year - 1}', devuélvala y que la corrija";

            return "";
        }

        public static bool EsMaterial(this PreasientoDtm preasiento, ContextoSe contexto)
        {
            if (preasiento.NegocioReferenciado == enumNegocio.FacturaEmitida)
            {
                var fae = (FacturaEmtDtm)preasiento.Referenciado(contexto);
                return fae.EsMaterial(contexto);
            }
            if (preasiento.NegocioReferenciado == enumNegocio.FacturaRecibida)
            {
                var far = (FacturaRecDtm)preasiento.Referenciado(contexto);
                foreach (var item in far.BiDelIvaPorNaturaleza(contexto))
                {
                    var naturaleza = contexto.SeleccionarPorId<NaturalezaDtm>(item.IdNaturaleza);
                    if (VariablesDePreasiento.EsMaterial(naturaleza))
                        return true;
                }
                return false;
            }
            return false;
        }


        public static bool EsMaterial(this FacturaEmtDtm emitida, ContextoSe contexto)
        {
            foreach (var item in emitida.BiDelIvaPorNaturaleza(contexto))
            {
                var naturaleza = contexto.SeleccionarPorId<NaturalezaDtm>(item.IdNaturaleza);
                if (VariablesDePreasiento.EsMaterial(naturaleza))
                    return true;
            }
            return false;
        }

        public static bool EsMaterial(this FacturaRecDtm recibida, ContextoSe contexto)
        {
            foreach (var item in recibida.BiDelIvaPorNaturaleza(contexto))
            {
                var naturaleza = contexto.SeleccionarPorId<NaturalezaDtm>(item.IdNaturaleza);
                if (VariablesDePreasiento.EsMaterial(naturaleza))
                    return true;
            }
            return false;
        }


        public static List<CodigosPorNaturaleza> CodigosContablesPorNaturaleza(this FacturaRecDtm recibida, ContextoSe contexto)
        {
            var resultado = new List<CodigosPorNaturaleza>();
            foreach (var item in recibida.BiDelIvaPorNaturaleza(contexto))
            {
                var naturaleza = contexto.SeleccionarPorId<NaturalezaDtm>(item.IdNaturaleza);
                resultado.Add(VariablesDePreasiento.CodigosPorNaturaleza(naturaleza));
            }
            return resultado;
        }

        public static bool EsSuministro(this FacturaRecDtm recibida, ContextoSe contexto)
        {
            foreach (var item in recibida.BiDelIvaPorNaturaleza(contexto))
            {
                var naturaleza = contexto.SeleccionarPorId<NaturalezaDtm>(item.IdNaturaleza);
                if (VariablesDePreasiento.EsSuministro(naturaleza))
                    return true;
            }
            return false;
        }

        public static bool EsArrendamiento(this FacturaRecDtm recibida, ContextoSe contexto)
        {
            foreach (var item in recibida.BiDelIvaPorNaturaleza(contexto))
            {
                var naturaleza = contexto.SeleccionarPorId<NaturalezaDtm>(item.IdNaturaleza);
                if (VariablesDePreasiento.EsArrendamiento(naturaleza))
                    return true;
            }
            return false;
        }

        public static CircuitoDocDtm CrearCircuitoDeLotes(ContextoSe contexto, SociedadDtm sociedad, string nombre, int ejercicio, DateTime? fechaContable)
        {
            var tipoCad = sociedad.Autonomo ? VariablesDeCircuitosDoc.IdDelTipoCircuitoDocParaEstimacionDirecta() : VariablesDeCircuitosDoc.IdDelTipoCircuitoDocParaLoteDePreasientos();

            var descripcion = $"Lote generado el '{DateTime.Now.ToString("dd/MM/yyyy")}' para los movimientos contables del ejercicio '{ejercicio}' de la sociedad '{sociedad.NIF}'";

            if (fechaContable != null)
            {
                descripcion = descripcion + $" con fecha contable '{fechaContable.Fecha().ToString("dd/MM/yyyy")}'";
            }
            else
            {
                descripcion = descripcion + $" con fecha contable según el movimiento";
            }


            var cad = new CircuitoDocDtm
            {
                IdCg = sociedad.IdCgDocumentalDeSpr(contexto),
                IdTipo = tipoCad,
                Nombre = nombre,
                Descripcion = descripcion.Left(2000)
            }.InsertarComoAdministrador(contexto);

            return cad;
        }

        public static CircuitoDocDtm CrearCircuitoDeLotesDeTerceros(ContextoSe contexto, SociedadDtm sociedad, string nombre, int ejercicio)
        {
            var tipoCad = VariablesDeCircuitosDoc.IdDelTipoCircuitoDocParaLoteDePreasientos();

            var descripcion = $"Lote generado el '{DateTime.Now.ToString("dd/MM/yyyy")}' para los terceros del ejercicio '{ejercicio}' de la sociedad '{sociedad.NIF}'";

            var cad = new CircuitoDocDtm
            {
                IdCg = sociedad.IdCgDocumentalDeSpr(contexto),
                IdTipo = tipoCad,
                Nombre = nombre,
                Descripcion = descripcion.Left(2000)
            }.InsertarComoAdministrador(contexto);

            return cad;
        }

        public static ArchivadorDtm CrearArchivadorDePreasientosPendientes(ContextoSe contexto, SociedadDtm sociedad, string nombre, List<PreasientoDtm> preasientos, DateTime? fechaContable)
        {
            var tipoArchivador = enumNegocio.Preasiento.Parametro(enumParametrosDePreasiento.SPR_Tipo_De_Archivador, valorPorDefecto: "0").Valor.Entero();
            if (tipoArchivador == 0)
            {
                GestorDeErrores.Emitir($"Defina el {enumParametrosDePreasiento.SPR_Tipo_De_Archivador} para '{enumParametrosDePreasiento.SPR_Tipo_De_Archivador.Descripcion()}'");
            }

            var descripcion = $"Lote generado para los preasientos vinculados con " +
                $"fecha contable '{(fechaContable == null ? "la indicada por cada preasiento" : fechaContable.Fecha().ToString("dd/MM/yyyy"))}' y " +
                $"sociedad '{sociedad.NIF}'{Environment.NewLine}" +
                $"Preasientos vinculados ({preasientos.Count}):{Environment.NewLine}";
            foreach (var preasiento in preasientos)
            {
                descripcion = descripcion + preasiento.Referencia + " -> " +
                    preasiento.NegocioReferenciado + ": " +
                    preasiento.Referenciado(contexto).Referencia +
                    Environment.NewLine;
            }
            var archivador = new ArchivadorDtm
            {
                IdCg = sociedad.IdCgDocumentalDeSpr(contexto),
                IdTipo = tipoArchivador,
                Nombre = nombre,
                Descripcion = descripcion.Left(2000)
            }.InsertarComoAdministrador(contexto);

            return archivador;
        }
    }
}
