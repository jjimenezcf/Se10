using Gestor.Errores;
using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Gastos;
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

        public static void CancelarPreasiento(this PreasientoDtm preasiento, ContextoSe contexto)
        {
            if (preasiento.Estado(contexto).Cancelado)
                return;

            if (preasiento.EstaEnLaEtapa(enumEtapasDePreasiento.SPR_Etapa_Pendiente))
            {
                preasiento.TransitarALaEtapa(contexto, enumEtapasDePreasiento.SPR_Etapa_Cancelado.EstadosDeLaEtapa(), parametros: new Dictionary<string, object>
                            {
                                {ltrParametrosEp.asunto, preasiento.NegocioReferenciado == enumNegocio.Cobro? "Cancelado al eliminar el cobro" : "Cancelado al regenerar el preasiento" },
                                {ltrParametrosEp.detalleAsunto, preasiento.NegocioReferenciado == enumNegocio.Cobro?
                                     $"Se ha eliminado el cobro asociado a la factura {((CobroDeFaeDtm)preasiento.Referenciado(contexto)).Factura(contexto).Referencia}":
                                     $"Se ha regenerado el preasiento desde la interface"
                                }
                            });
            }
            else
            {
                preasiento.TransitarALaEtapa(contexto, enumEtapasDePreasiento.SPR_Etapa_Anulado.EstadosDeLaEtapa(), parametros: new Dictionary<string, object>
                   {
                       {ltrParametrosEp.asunto, "Anulado al regenerar el preasiento" },
                       {ltrParametrosEp.detalleAsunto, $"Se ha anulado el preasiento desde la interface, recuerde cancelar su contabilización" }
                   });
            }
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
                : referenciado.Referencia;

                preasiento.TransitarALaEtapa(contexto, enumEtapasDePreasiento.SPR_Etapa_Cancelado.EstadosDeLaEtapa(), parametros: new Dictionary<string, object>
                            {
                                {ltrParametrosEp.asunto, "Cancelado al intentar contabilizar" },
                                {ltrParametrosEp.detalleAsunto, $"Se ha cancelado por no ser contabilizable el referenciado '{preasiento.NegocioReferenciado}: {referencia}'"
                                }
                            });
            }
        }

        public static void AnularPreasiento(this IUsaPreasiento elemento, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            if (elemento.Preasiento(contexto, errorSiNoHay: false) is not null)
            {
                var estado = elemento.Preasiento(contexto).Estado(contexto);
                if (estado.Terminado)
                {
                    GestorDeErrores.Emitir($"No se puede devolver {elemento.Preasiento(contexto).NegocioReferenciado.ConArticulo()} ya que el preasiento '{elemento.Preasiento(contexto).Referencia}' asociado está en un lote contable");
                }
                else
                {
                    var preasiento = elemento.Preasiento(contexto);
                    if (!preasiento.EstaEnLaEtapa(enumEtapasDePreasiento.SPR_Etapa_Cancelado))
                    {
                        preasiento.TransitarALaEtapa(contexto, enumEtapasDePreasiento.SPR_Etapa_Cancelado.EstadosDeLaEtapa(), parametros: new Dictionary<string, object>
                           {
                               {ltrParametrosEp.asunto, "Anulado el asiento por el sistema" },
                               {ltrParametrosEp.detalleAsunto, $"El asiento ha sido cancelado al transitar {elemento.Preasiento(contexto).NegocioReferenciado.ConArticulo()} '{elemento.Referencia}'" }
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


        public static FacturaRecDtm Preasentar(this FacturaRecDtm recibida, ContextoSe contexto)
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
                Ejercicio = recibida.FacturadaEl.Year,
                CodigoDiario = tipoDePreasiento.CodigoDeDiario(contexto),
                SociedadContable = recibida.Sociedad(contexto).CodigoContable,
                NegocioReferenciado = enumNegocio.FacturaRecibida,
                IdReferenciado = recibida.Id,
                FechaContable = recibida.ContabilizadaEl.Fecha()
            }.InsertarComoAdministrador(contexto);

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
                    Cuenta = naturaleza.CuentaDeGasto.Codigo,
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

        public static FacturaEmtDtm Preasentar(this FacturaEmtDtm emitida, ContextoSe contexto)
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
            }.InsertarComoAdministrador(contexto);

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
                    Cuenta = naturaleza.CuentaDeIngreso.Codigo,
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

        public static CobroDeFaeDtm Preasentar(this CobroDeFaeDtm cobro, ContextoSe contexto)
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
            }.InsertarComoAdministrador(contexto);
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

        public static PagoDtm Preasentar(this PagoDtm pago, ContextoSe contexto)
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
            }.InsertarComoAdministrador(contexto);

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

        public static IUsaPreasiento Referenciado(this PreasientoDtm spr, ContextoSe contexto, bool errorSiNoHay = true)
        {
            if (spr.NegocioReferenciado == enumNegocio.No_Definido) return null;
            var cache = ServicioDeCaches.Obtener(CacheDe.Spr_Referenciado);
            if (!cache.ContainsKey(spr.Id.ToString()))
            {
                cache[spr.Id.ToString()] = (IUsaPreasiento)spr.NegocioReferenciado.SeleccionarPorId(contexto, spr.IdReferenciado.Entero(), parametros: new Dictionary<string, object>
                     {
                         {ltrParametrosNeg.ErrorSiNoLoHay, errorSiNoHay }
                     });
            }
            return (IUsaPreasiento)cache[spr.Id.ToString()];
        }

        public static bool HayQueCancelarlo(this PreasientoDtm spr, ContextoSe contexto)
        {
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
                IdCg = sociedad.IdCgDocumentalDeSpr(),
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
                IdCg = sociedad.IdCgDocumentalDeSpr(),
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
                IdCg = sociedad.IdCgDocumentalDeSpr(),
                IdTipo = tipoArchivador,
                Nombre = nombre,
                Descripcion = descripcion.Left(2000)
            }.InsertarComoAdministrador(contexto);

            return archivador;
        }
    }
}
