using System;
using ServicioDeDatos;
using Utilidades;
using ServicioDeDatos.Presupuesto;
using ServicioDeDatos.Ventas;
using System.Linq;
using Gestor.Errores;
using System.Collections.Generic;
using ModeloDeDto.Presupuesto;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.Elemento;
using ModeloDeDto.Negocio;
using ServicioDeDatos.MaestrosTecnico;
using ServicioDeDatos.Juridico;

namespace GestorDeElementos.Extensores
{
    public static class ExtensorDePresupuestos
    {
        public static bool UsaLaAmpliacionDe(ContextoSe contexto, int idTipo, Type tipoAmpliacion)
        {
            var tipoDtm = (TipoDePresupuestoDtm)enumNegocio.Presupuesto.CrearGestorDeTipo(contexto).LeerRegistroPorId(idTipo, aplicarJoin: false);
            if (tipoDtm.ClaseDePresupuesto == enumClaseDePresupuesto.venta && tipoAmpliacion == typeof(PptDeVentaDtm))
                return true;

            return false;
        }

        public static bool UsaElDetalleDe(ContextoSe contexto, int idTipo, Type tipoDeDetalle)
        {
            var tipoDtm = (TipoDePresupuestoDtm)enumNegocio.Presupuesto.CrearGestorDeTipo(contexto).LeerRegistroPorId(idTipo, aplicarJoin: false);

            if (tipoDtm.ClaseDePresupuesto == enumClaseDePresupuesto.venta && tipoDeDetalle == typeof(LineaDeUnPptDtm))
                return true;

            return false;
        }

        public static decimal Total(this PresupuestoDtm elemento, ContextoSe contexto, bool conIva)
        {
            var cacheSinIva = ServicioDeCaches.Obtener(CacheDe.Ppt_TotalSinIva);
            var cacheConIva = ServicioDeCaches.Obtener(CacheDe.Ppt_TotalConIva);
            if (!cacheSinIva.ContainsKey(elemento.Id.ToString()))
            {
                decimal totalSinIva = 0;
                decimal totalConIva = 0;
                var lineas = elemento.Detalles<LineaDeUnPptDtm>(contexto);
                foreach (var linea in lineas)
                {
                    if (linea.TipoDeLinea == ServicioDeDatos.Elemento.Enumerados.enumTipoDeLinea.Comentario) continue;
                    totalSinIva = totalSinIva + linea.ImporteConDto;
                    totalConIva = totalConIva + (decimal)linea.ImporteDeLinea;
                }
                cacheSinIva[$"{elemento.Id}"] = totalSinIva;
                cacheConIva[$"{elemento.Id}"] = totalConIva;
            }
            return conIva ? (decimal)cacheConIva[$"{elemento.Id}"] : (decimal)cacheSinIva[$"{elemento.Id}"];
        }

        public static void AntesDeEntregarElPpt(this PresupuestoDtm ppt, ContextoSe contexto)
        {
            var lineas = ppt.Detalles<LineaDeUnPptDtm>(contexto);

            if (lineas.Count == 0)
                GestorDeErrores.Emitir($"Ha de definir el detalle del {enumNegocio.Presupuesto.Singular(true)} '{ppt.Referencia}' antes de entregarlo");

            foreach (var linea in lineas)
                linea.ValidarIva(ppt.Referencia);
        }

        public static void AntesDeCanelar(this PresupuestoDtm ppt, ContextoSe contexto)
        {
            if (ppt.TieneFacturas(contexto))
                GestorDeErrores.Emitir($"No se puede cancelar el {enumNegocio.Presupuesto.Singular(true)} '{ppt.Referencia}' por tener asociada alguna {enumNegocio.FacturaEmitida.Singular(true)} no anulada, indique que no se ejecuta");

            if (ppt.TienePartesTr(contexto))
                GestorDeErrores.Emitir($"No se puede cancelar el {enumNegocio.Presupuesto.Singular(true)} '{ppt.Referencia}' por tener asociado algún {enumNegocio.ParteDeTrabajo.Singular(true)} no anulado, indique que no se ejecuta");

        }

        public static void AntesDeCerrar(this PresupuestoDtm ppt, ContextoSe contexto)
        {
            if (!ppt.TieneFacturas(contexto) && !ppt.TienePartesTr(contexto))
                GestorDeErrores.Emitir($"No se puede cerrar el {enumNegocio.Presupuesto.Singular(true)} '{ppt.Referencia}' por no tener ningún parte ni ninguna factura, si no lo va a ejecutar cancélelo");

            var partes = ppt.PartesTr(contexto, excluirTerminados: true);
            foreach (var parte in partes)
            {
                if (parte.EstaEnAlgunaDeLasEtapa(new List<enumEtapasDePartesTr> { enumEtapasDePartesTr.PTR_Etapa_Pendiente }))
                    GestorDeErrores.Emitir($"No se puede cerrar el {enumNegocio.Presupuesto.Singular(true)} '{ppt.Referencia}' por tener el parte '{parte.Referencia}' pendiente de realizar");
            }
        }

        public static void AntesDeDevolverUnPptFacturable(this PresupuestoDtm ppt, ContextoSe contexto)
        {
            var facturas = ppt.FacturasEmt(contexto);
            var totalFacturado = facturas.Sum(f => f.BiConIva(contexto));
            if (facturas.Count() > 0 && totalFacturado != 0)
                GestorDeErrores.Emitir($"No se puede retroceder el {enumNegocio.Presupuesto.Singular(true)} '{ppt.Referencia}' por tener {(facturas.Count() == 1 ? $"la {enumNegocio.FacturaEmitida.Singular(true)} asociada" : $"las {enumNegocio.FacturaEmitida.Plural(true)} asociadas")}: {string.Join(',', facturas.Select(x => x.Referencia).ToList())}");
        }

        public static void AntesDeDevolverUnPptEjecutable(this PresupuestoDtm ppt, ContextoSe contexto)
        {
            var partes = ppt.PartesTr(contexto);

            if (partes.Count() > 0)
                GestorDeErrores.Emitir($"No se puede anular la ejecución del {enumNegocio.Presupuesto.Singular(true)} '{ppt.Referencia}' por tener {(partes.Count() == 1 ? $"el {enumNegocio.ParteDeTrabajo.Singular(true)} asociado" : $"los {enumNegocio.ParteDeTrabajo.Plural(true)} asociados")}: {string.Join(',', partes.Select(x => x.Referencia).ToList())}");
        }

        public static void TrasAceptarElPresupuesto(this PresupuestoDtm ppt, ContextoSe contexto)
        {
            ppt.Solicitante(contexto).Cliente(contexto, crearCliente: true);
        }

        public static List<ParteTrDtm> PartesTr(this PresupuestoDtm presupuesto, ContextoSe contexto, bool excluirCancelados = true, bool excluirTerminados = false)
        {
            var filtro = new Dictionary<string, object> { { nameof(ParteTrDtm.IdPresupuesto), presupuesto.Id } };

            var partes = contexto.SeleccionarTodos<ParteTrDtm>(filtro, parametros: new Dictionary<string, object>
                {
                    { ltrParametrosNeg.ExcluirCancelados, excluirCancelados },
                    { ltrParametrosNeg.ExcluirTerminados, excluirTerminados }
                });
            return partes;
        }

        public static List<FacturaEmtDtm> FacturasEmt(this PresupuestoDtm presupuesto, ContextoSe contexto, bool excluirCancelados = true, bool excluirTerminados = false)
        {
            var filtro = new Dictionary<string, object> { { nameof(ParteTrDtm.IdPresupuesto), presupuesto.Id } };

            var facturas = contexto.SeleccionarTodos<FacturaEmtDtm>(filtro, parametros: new Dictionary<string, object>
                {
                    { ltrParametrosNeg.ExcluirCancelados, excluirCancelados },
                    { ltrParametrosNeg.ExcluirTerminados, excluirTerminados }
                });
            return facturas;
        }

        public static void ValidarQueLosPptsEstanEnEtapaDe(ContextoSe contexto, List<int> idsDePpts, enumEtapasDePpts etapa)
        {
            foreach (var id in idsDePpts)
            {
                var ppt = contexto.SeleccionarPorId<PresupuestoDtm>(id);
                if (!ppt.EstaEnLaEtapa(etapa))
                    GestorDeErrores.Emitir($"El presupuesto {ppt.Referencia} no está en la etapa {etapa.Nombre(true)}");
            }
        }

        public static PresupuestoDtm CrearValoracion(ContextoSe contexto, ValoracionDto valoracion)
        {
            if (valoracion.idExpediente.Entero() == 0)
                GestorDeErrores.Emitir("Una valoración debe depender de un expediente");

            var expediente = contexto.SeleccionarPorId<ExpedienteDtm>((int)valoracion.idExpediente);
            if (expediente.PermisosDe(contexto) == ServicioDeDatos.Seguridad.enumModoDeAccesoDeDatos.SinPermiso)
                GestorDeErrores.Emitir($"No tiene permisos sobre el expediente '{expediente.Referencia}'");

            if (!expediente.EstaEnLaEtapa(enumEtapasDeExpedientes.EXP_Etapa_Asociar_Presupuestos))
                GestorDeErrores.Emitir($"El expediente '{expediente.Referencia}' no está en la etapa de asociación de presupuestos");

            var ppt = new PresupuestoDtm
            {
                Nombre = valoracion.Concepto,
                IdCg = valoracion.IdCg,
                IdTipo = valoracion.IdTipo,
                Descripcion = valoracion.Descripcion,
                IdSolicitante = valoracion.IdSolicitante,
                IdExpediente = valoracion.idExpediente,
                Contacto = expediente.Contacto,
                eMail = expediente.eMail,
                Telefono = expediente.Telefono
            }.Insertar(contexto);

            new LineaDeUnPptDtm
            {
                Concepto = valoracion.Concepto,
                TipoDeLinea = Enumerados.enumTipoDeLinea.Alzada,
                Orden = IncrementarOrdenEn(contexto),
                IdElemento = ppt.Id,
                IdIvaR = valoracion.IdIvaR,
                Clase = ApiDeEnsamblados.ToEnumerado<enumClaseUnitario>(valoracion.Clase),
                IdNaturaleza = valoracion.IdNaturaleza,
                IdUnidad = valoracion.IdUnidad,
                Cantidad = valoracion.Cantidad,
                Precio = valoracion.Precio,
                Descuento = valoracion.Descuento
            }.Insertar(contexto);

            if (valoracion.IdArchivo is not null) ppt.Vincular(contexto, enumNegocio.Archivos, (int)valoracion.IdArchivo);

            return ppt;
        }
        public static PresupuestoDtm Copiar(this PresupuestoDtm pptOrigen, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            var idExpediente = (int)parametros.LeerValor<long>(nameof(CopiarPptDto.idExpediente), 0);
            var transaccion = contexto.IniciarTransaccion();
            try
            {
                var pptNuevo = new PresupuestoDtm
                {
                    IdCg = contexto.SeleccionarPorId<CentroGestorDtm>((int)(long)parametros[nameof(CopiarPptDto.IdCg)]).Id,
                    IdSolicitante = contexto.SeleccionarPorId<InterlocutorDtm>((int)(long)parametros[nameof(CopiarPptDto.IdSolicitante)]).Id,
                    IdTipo = contexto.SeleccionarPorId<TipoDePresupuestoDtm>((int)(long)parametros[nameof(CopiarPptDto.IdTipo)]).Id,
                    IdEstado = 0,
                    Nombre = parametros.LeerValor<string>(nameof(CopiarPptDto.Nombre)),
                    Descripcion = parametros.LeerValor<string>(nameof(CopiarPptDto.Descripcion)),
                    ClaseDePresupuesto = pptOrigen.ClaseDePresupuesto,
                    IdExpediente = idExpediente == 0 ? null : idExpediente,
                }.Insertar(contexto, new Dictionary<string, object> { { ltrDeUnPresupuesto.TrazaDelCambioDeExpediente, $"Copia del presupuesto {pptOrigen.Expresion}" } });

                var datosPropuestos = contexto.SeleccionarPorAk<PptDeVentaDtm>(new Dictionary<string, object> { { nameof(PptDeVentaDtm.IdElemento), pptOrigen.Id } }, errorSiNoHay: false);
                if (datosPropuestos != null)
                {
                    var iva = datosPropuestos.IdIvaR != null
                        ? contexto.SeleccionarPorId<IvaRepercutidoDtm>((int)datosPropuestos.IdIvaR)
                        : null;
                    new PptDeVentaDtm
                    {
                        IdElemento = pptNuevo.Id,
                        IdIvaR = datosPropuestos.IdIvaR,
                        Iva = iva.Porcentaje,
                        Descuento = datosPropuestos.Descuento
                    }.Insertar(contexto);
                }

                var lineasPpts = contexto.SeleccionarTodos<LineaDeUnPptDtm>(nameof(LineaDeUnPptDtm.IdElemento), pptOrigen.Id);
                foreach (var linea in lineasPpts)
                {
                    linea.Id = 0;
                    linea.IdElemento = pptNuevo.Id;
                    linea.Insertar(contexto);
                }
                contexto.Commit(transaccion);
                return pptNuevo;
            }
            catch
            {
                contexto.Rollback(transaccion);
                throw;
            }
        }

        public static decimal Facturado(this PresupuestoDtm ppt, ContextoSe contexto)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Ppt_Facturado);
            if (!cache.ContainsKey(ppt.Id.ToString()))
            {
                var facturas = contexto.SeleccionarTodos<FacturaEmtDtm>(nameof(FacturaEmtDtm.IdPresupuesto), ppt.Id, parametros: new Dictionary<string, object>
                {
                    { ltrParametrosNeg.ExcluirCancelados, true }
                });

                var total = (decimal)0.0;
                foreach (var factura in facturas)
                {
                    if (factura.EstaEnAlgunaDeLasEtapa(new List<enumEtapasDeFacturasEmt> {
                        enumEtapasDeFacturasEmt.FAE_Etapa_Prefactura,
                        enumEtapasDeFacturasEmt.FAE_Etapa_Anulada,
                        enumEtapasDeFacturasEmt.FAE_Etapa_No_Cobrable }))
                        continue;
                    total = total + factura.Bi(contexto);
                }

                cache[ppt.Id.ToString()] = total;
            }
            return (decimal)(cache[ppt.Id.ToString()]);
        }

        public static decimal Ejecutando(this PresupuestoDtm ppt, ContextoSe contexto)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Ppt_Ejecutando);
            if (!cache.ContainsKey(ppt.Id.ToString()))
            {
                var partes = contexto.SeleccionarTodos<ParteTrDtm>(nameof(ParteTrDtm.IdPresupuesto), ppt.Id, parametros: new Dictionary<string, object>
                {
                    { ltrParametrosNeg.ExcluirCancelados, true }
                });

                var total = (decimal)0.0;
                foreach (var parte in partes)
                {
                    if (parte.EstaEnAlgunaDeLasEtapa(new List<enumEtapasDePartesTr> { enumEtapasDePartesTr.PTR_Etapa_Facturado }))
                        continue;
                    total = total + parte.Total(contexto, conIva: false);
                }

                cache[ppt.Id.ToString()] = total;
            }
            return (decimal)(cache[ppt.Id.ToString()]);
        }

        public static decimal Ejecutado(this PresupuestoDtm ppt, ContextoSe contexto)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Ppt_Ejecutado);
            if (!cache.ContainsKey(ppt.Id.ToString()))
            {

                var filtro = new Dictionary<string, object> { { ltrDeUnParteTr.EjecutadoDeUnPpt, ppt.Id } };
                var partes = contexto.SeleccionarTodos<ParteTrDtm>(filtro, parametros: new Dictionary<string, object>
                {
                    { ltrParametrosNeg.ExcluirCancelados, true }
                });

                var total = (decimal)0.0;
                foreach (var parte in partes)
                {
                    total = total + parte.Total(contexto, conIva: false);
                }
                cache[ppt.Id.ToString()] = total;

            }
            return (decimal)(cache[ppt.Id.ToString()]);
        }

        public static decimal Prefacturado(this PresupuestoDtm ppt, ContextoSe contexto)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Ppt_Prefacturado);
            if (!cache.ContainsKey(ppt.Id.ToString()))
            {
                var filtro = new Dictionary<string, object> { { ltrDeUnaFacturaEmt.PrefacturaDeUnPpt, ppt.Id } };
                var prefacturas = contexto.SeleccionarTodos<FacturaEmtDtm>(filtro, parametros: new Dictionary<string, object>
                {
                    { ltrParametrosNeg.ExcluirCancelados, true }
                });

                var total = (decimal)0.0;
                foreach (var prefactura in prefacturas)
                {
                    total = total + prefactura.Bi(contexto);
                }
                cache[ppt.Id.ToString()] = total;
            }
            return (decimal)(cache[ppt.Id.ToString()]);
        }

        public static bool TieneFacturas(this PresupuestoDtm ppt, ContextoSe contexto)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Ppt_TieneFacturas);
            if (!cache.ContainsKey(ppt.Id.ToString()))
            {
                var facturas = contexto.SeleccionarTodos<FacturaEmtDtm>(nameof(FacturaEmtDtm.IdPresupuesto), ppt.Id, parametros: new Dictionary<string, object>
                {
                    { ltrParametrosNeg.ExcluirCancelados, true }
                });

                cache[ppt.Id.ToString()] = facturas.Count() > 0;
            }
            return (bool)(cache[ppt.Id.ToString()]);
        }

        public static bool TienePartesTr(this PresupuestoDtm ppt, ContextoSe contexto)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Ppt_TienePartesTr);
            if (!cache.ContainsKey(ppt.Id.ToString()))
            {
                var partes = contexto.SeleccionarTodos<ParteTrDtm>(nameof(ParteTrDtm.IdPresupuesto), ppt.Id, parametros: new Dictionary<string, object>
                {
                    { ltrParametrosNeg.ExcluirCancelados, true }
                });

                cache[ppt.Id.ToString()] = partes.Count() > 0;
            }
            return (bool)(cache[ppt.Id.ToString()]);
        }

        public static ExpedienteDtm Expediente(this PresupuestoDtm presupuesto, ContextoSe contexto)
        {
            if (presupuesto.IdExpediente is null)
                return null;
            if (presupuesto.Expediente is not null)
                return presupuesto.Expediente;
            return presupuesto.Expediente = contexto.SeleccionarPorId<ExpedienteDtm>((int)presupuesto.IdExpediente);
        }

        public static DireccionDtm CopiarDireccionDelExpediente(this PresupuestoDtm presupuesto, ContextoSe contexto, enumCalificadorDireccion calificador)
        {
            var expediente = presupuesto.Expediente(contexto);

            var direccion = expediente.Direccion(contexto, calificador);
            if (direccion is not null)
                direccion = presupuesto.AsignarDireccion(contexto, direccion);
            return direccion;
        }

        public static DireccionDto DireccionDeEjecucion(this PresupuestoDtm presupuesto, ContextoSe contexto, bool errorSiNoHay = true)
        {
            var direccionesPpt = GestorDeDirecciones.LeerRegistros(contexto, enumNegocio.Presupuesto, presupuesto.Id).ToList();
            var direccion = direccionesPpt.FirstOrDefault(x => x.Calificador == enumCalificadorDireccion.ejecucion && x.Activo);
            if (direccion is not null)
                return direccion.MapearDto(contexto, direccion.Negocio);

            direccion = direccionesPpt.FirstOrDefault(x => x.Calificador == enumCalificadorDireccion.fiscal && x.Activo);
            if (direccion is not null)
                return direccion.MapearDto(contexto, direccion.Negocio);

            direccion = direccionesPpt.FirstOrDefault(x => x.Calificador == enumCalificadorDireccion.contacto && x.Activo);
            if (direccion is not null)
                return direccion.MapearDto(contexto, direccion.Negocio);

            var solicitante = presupuesto.Solicitante(contexto);
            direccion = solicitante.Direccion(contexto,  errorSiNoHay: false);
            if (direccion is not null)
                return direccion.MapearDto(contexto, direccion.Negocio);

            if (direccion is null && errorSiNoHay)
                GestorDeErrores.Emitir($"Debe definir una dirección de '{enumCalificadorDireccion.ejecucion.Descripcion().ToLower()}' para el {enumNegocio.Presupuesto.Singular(true)} '{presupuesto.Referencia}' " +
                    $"o una de '{enumCalificadorDireccion.contacto.Descripcion().ToLower()}' para el {enumNegocio.Interlocutor.Singular(true)} '{presupuesto.Solicitante(contexto).NIF(contexto)}'");

            return direccion is null ? null : direccion.MapearDto(contexto, direccion.Negocio);
        }

        public static int IncrementarOrdenEn(ContextoSe contexto)
        {
            var incremeto = enumNegocio.Presupuesto.LeerCrearParametro(contexto, enumParametrosDePresupuesto.PPT_IncrementarOrdenEn, "10");
            if (incremeto is null || incremeto.Valor.Entero() == 0)
                GestorDeErrores.Emitir($"Ha de definir el parámetro '{enumParametrosDePresupuesto.PPT_IncrementarOrdenEn}' del negocio de '{enumNegocio.Presupuesto.Singular()}' con un valor mayor de 0");
            return incremeto.Valor.Entero();
        }

        public static PresupuestoDtm Renombrar(this PresupuestoDtm presupuesto, ContextoSe contexto, string nombre)
        {
            if (!presupuesto.EsInterventor(contexto))
                GestorDeErrores.Emitir($"Para poder cambiar el nombre al presupuesto '{presupuesto.Referencia}' necesita permisos de intervención");
            var tran = contexto.IniciarTransaccion();
            try
            {
                var nombreAnterior = presupuesto.Nombre;
                presupuesto.Nombre = nombre;
                presupuesto = presupuesto.Modificar(contexto, accionEjecutada: ltrDeUnElemento.Accion_Renombrar);
                presupuesto.CrearTraza(contexto, "Renombrado", $"El usuario '{contexto.DatosDeConexion.Login}' ha renombrado el presupuesto, antes llamado '{nombreAnterior}'");
                contexto.Commit(tran);
                return presupuesto;
            }
            catch
            {
                contexto.Rollback(tran);
                throw;
            }
        }

    }
}
