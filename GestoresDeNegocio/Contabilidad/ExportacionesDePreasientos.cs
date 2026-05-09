using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestorDeElementos.Extensores.Contabilidad;
using GestoresDeNegocio.SistemaDocumental;
using GestoresDeNegocio.TrabajosSometidos;
using Microsoft.EntityFrameworkCore;
using ModeloDeDto;
using ModeloDeDto.Contabilidad;
using ModeloDeDto.SistemaDocumental;
using Newtonsoft.Json;
using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Gastos;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.TrabajosSometidos;
using ServicioDeDatos.Ventas;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilidades;

namespace GestoresDeNegocio.Contabilidad
{
    public class ExportacionesDePreasientos
    {
        public static void ContabilizarEnNcs(EntornoDeUnaAccion entorno)
        {
            var contextoDeLaAccion = entorno.Contexto;

            var entornoTrabajo = entorno.Entrada.LeerValor<EntornoDeTrabajo>(nameof(EntornoDeTrabajo), null);
            var trabajo = entornoTrabajo?.TrabajoDeUsuario ?? null; // entorno.Entrada.LeerValor<TrabajoDeUsuarioDtm>(nameof(TrabajoDeUsuarioDtm), null);
            var contextoDelTrabajo = entornoTrabajo?.ContextoDelEntorno ?? null; // entorno.Entrada.LeerValor<ContextoSe>(nameof(ContextoSe), null);


            var filtrosJson = entorno.Entrada.LeerValor(ltrFiltros.filtro, "");
            var filtros = filtrosJson.IsNullOrEmpty() ? new List<ClausulaDeFiltrado>() : JsonConvert.DeserializeObject<List<ClausulaDeFiltrado>>(filtrosJson);

            if (filtros.Any(f => f.Clausula.ToLower() == ltrFiltros.IdsDeEstado.ToLower()))
                GestorDeErrores.Emitir($"Está intentando contabilizar y ha filtrado por estados, esto no está permitido");

            var descontabilizar = entorno.Entrada.LeerValor<bool>(nameof(CrearLoteContableDto.Descontabilizar), false);
            var respetarFechaContable = entorno.Entrada.LeerValor<bool>(nameof(CrearLoteContableDto.RespetarFechaContable), false);
            var idSociedad = entorno.Entrada.LeerValor<int>(nameof(SociedadDtm.Id));
            filtros.Add(new ClausulaDeFiltrado
            {
                Clausula = ltrDeSociedad.FiltroPorIdSociedad,
                Criterio = enumCriteriosDeFiltrado.igual,
                Valor = idSociedad.ToString()
            });
            filtros.Add(new ClausulaDeFiltrado
            {
                Clausula = ltrFiltros.IdsDeEstado,
                Criterio = enumCriteriosDeFiltrado.igual,
                Valor = VariablesDePreasiento.Estados(enumEtapasDePreasiento.SPR_Etapa_Pendiente) + $"{(descontabilizar ? $",{VariablesDePreasiento.Estados(enumEtapasDePreasiento.SPR_Etapa_Contabilizado)}" : "")}"
            });
            var preasientos = contextoDeLaAccion.SeleccionarTodos<PreasientoDtm>(filtros).OrderBy(x => x.NegocioReferenciado).ThenBy(x => x.IdReferenciado).ToList();
            var preasientosParaTransitar = new List<PreasientoDtm>();

            if (descontabilizar)
            {
                foreach (var preasiento in preasientos)
                {
                    if (preasiento.EstaEnLaEtapa(enumEtapasDePreasiento.SPR_Etapa_Contabilizado))
                    {
                        preasiento.AnularContabilizacion(contextoDeLaAccion);
                    }
                }
            }

            foreach (var preasiento in preasientos)
            {
                var mensaje = "";
                if (!preasiento.EstaEnLaEtapa(enumEtapasDePreasiento.SPR_Etapa_Pendiente))
                {
                    mensaje = $"El preasiento '{preasiento.Referencia}' no se puede enviar a Ncs por no estar pendiente";
                    if (trabajo is null)
                        GestorDeErrores.Emitir(mensaje);
                }
                else
                {
                    var circuitos = preasiento.Vinculados<CircuitoDocDtm>(contextoDeLaAccion);
                    if (circuitos.Count > 0)
                    {
                        mensaje = $"El preasiento '{preasiento.Referencia}' no se puede enviar a Ncs por estar vinculado al lote contable '{circuitos[0].Referencia}'";
                        if (trabajo is null)
                            GestorDeErrores.Emitir(mensaje);
                    }
                }

                mensaje = preasiento.DimeSiEstaBien(contextoDeLaAccion);
                if (mensaje.IsNullOrEmpty())
                    preasientosParaTransitar.Add(preasiento);
                else
                {
                    if (entornoTrabajo is not null)
                        entornoTrabajo.AnotarError($"{preasiento.NegocioReferenciado.Singular()}: {preasiento.Referencia}", mensaje);

                    preasiento.CrearTraza(contextoDeLaAccion, "No se pudo enviar a Ncs", "El envío a Ncs no se pudo realizar: " + mensaje);
                }
            }

            var sociedad = contextoDeLaAccion.SeleccionarPorId<SociedadDtm>(idSociedad);
            if (preasientosParaTransitar.Count == 0)
            {
                var m = $"No hay preasientos para enviar a Ncs para la sociedad '{sociedad.Expresion}'";
                if (entornoTrabajo is not null)
                {
                    entornoTrabajo.CrearTraza(m);
                    return;
                }
                else
                    GestorDeErrores.Emitir(m);
            }

            var fechaContable = entorno.Entrada.LeerValor<DateTime?>(nameof(PreasientoDtm.FechaContable));
            var procesarElEjercicio = entorno.Entrada.LeerValor(nameof(CrearLoteContableDto.Ejercicio), DateTime.Now.Year);
            int ejercicio = fechaContable is null ? procesarElEjercicio : ((DateTime)fechaContable).Year;

            if (ejercicio > DateTime.Now.Year)
            {
                var m = $"No se puede procesar el ejercicio '{ejercicio}' porque es mayor que el ejercicio actual '{DateTime.Now.Year}'";
                if (entornoTrabajo is not null)
                {
                    entornoTrabajo.CrearTraza(m);
                    return;
                }
                else
                    GestorDeErrores.Emitir(m);
            }

            if (ejercicio+1 < DateTime.Now.Year)
            {
                var m = $"No se puede procesar el ejercicio '{ejercicio}' porque es anterior a dos años al ejercicio actual '{DateTime.Now.Year}'";
                if (entornoTrabajo is not null)
                {
                    entornoTrabajo.CrearTraza(m);
                    return;
                }
                else
                    GestorDeErrores.Emitir(m);
            }

            var preasientosParaContabilizar = new List<PreasientoDtm>();
            foreach (var preasiento in preasientosParaTransitar)
            {
                try
                {
                    if (preasiento.Ejercicio != ejercicio)
                    {
                        if (entornoTrabajo is null)
                            GestorDeErrores.Emitir($"El preasiento '{preasiento.Referencia}' es del ejercio '{preasiento.Ejercicio}' y el ejercicio contable seleccionado es '{ejercicio}'");
                        else
                        {
                            entornoTrabajo.CrearTraza($"El preasiento '{preasiento.Referencia}' es del ejercio '{preasiento.Ejercicio}' y el ejercicio contable seleccionado es '{ejercicio}'");
                            continue;
                        }
                    }

                    if (preasiento.HayQueCancelarlo(contextoDeLaAccion))
                    {
                        preasiento.CancelarPreasientoPorEstarloSuReferenciado(contextoDeLaAccion);
                        continue;
                    }

                    preasiento.TransitarALaEtapa(contextoDeLaAccion,
                        enumEtapasDePreasiento.SPR_Etapa_Contabilizado.EstadosDeLaEtapa(),
                        new Dictionary<string, object> { { ltrParametrosNeg.EstaEjecutandoUnaAccion, true } },
                        delSistema: true);
                    preasientosParaContabilizar.Add(preasiento);
                }
                catch (Exception e)
                {
                    if (entornoTrabajo is not null)
                        entornoTrabajo.AnotarError($"preasiento {preasiento.Referencia}", e.Message);
                    else
                    {
                        if (preasientosParaTransitar.Count == 1)
                            throw;
                    }
                    preasiento.CrearTraza(contextoDeLaAccion, "No se pudo enviar a Ncs", "El envío de preasiento a Ncs falló por: " + e.Message);
                }
            }

            if (preasientosParaContabilizar.Count != preasientosParaTransitar.Count)
            {
                if (entornoTrabajo is null)
                    GestorDeErrores.Emitir($"No hay preasientos a transitar a la etapa '{enumEtapasDePreasiento.SPR_Etapa_Contabilizado.Nombre()}', consulte los motivos");

                if (preasientosParaContabilizar.Count == 0)
                    return;
            }

            var generadorNcs = new LoteContableConNcs(contextoDeLaAccion, preasientosParaContabilizar, sociedad, ejercicio,respetarFechaContable, fechaContable);
            try
            {
                generadorNcs.GenerarLoteContable();

                var nombre = entorno.Accion.Expresion + $" el {DateTime.Now.ToString("yyyy-MM-dd HH-mm")}";

                var circuito = ExtensorDePreasientos.CrearCircuitoDeLotes(contextoDeLaAccion, sociedad, nombre, ejercicio, fechaContable);
                var archivoDeAsientos = circuito.AnexarArchivo(contextoDeLaAccion, generadorNcs.DiarioXml);
                new BloqueoDeUnArchivoDtm
                {
                    IdArchivo = archivoDeAsientos.Id,
                    Bloqueado = true
                }.Insertar(contextoDeLaAccion, new Dictionary<string, object> { { nameof(AuditoriaDeUnArchivoDtm.Auditoria), ltrDeUnArchivo.Migrado } });

                entornoTrabajo?.CrearTraza($"Preasientos de la sociedad '{sociedad.Expresion}' asociados a documento de envío");
                var terceros = VincularPreasientos(contextoDeLaAccion, trabajo, contextoDelTrabajo, preasientosParaContabilizar, circuito);

                ExportarTercerosNcs(generadorNcs, circuito, terceros);

                EnviarCircuito(entornoTrabajo, circuito, enumVistasSistemaDocumental.CrudLotesContables);
            }
            finally
            {
                generadorNcs.Deshacer();
            }

        }

        public static void EstimacionDirectaEnNcs(EntornoDeUnaAccion entorno)
        {
            var contexto = entorno.Contexto;

            var entornoTrabajo = entorno.Entrada.LeerValor<EntornoDeTrabajo>(nameof(EntornoDeTrabajo), null);
            var trabajo = entornoTrabajo?.TrabajoDeUsuario ?? null; // entorno.Entrada.LeerValor<TrabajoDeUsuarioDtm>(nameof(TrabajoDeUsuarioDtm), null);
            var contextoDelTrabajo = entornoTrabajo?.ContextoDelEntorno ?? null; // entorno.Entrada.LeerValor<ContextoSe>(nameof(ContextoSe), null);


            var ejercicio = entorno.Entrada.LeerValor(nameof(CrearLoteContableDto.Ejercicio), DateTime.Now.Year);

            var idSociedad = entorno.Entrada.LeerValor<int>(nameof(SociedadDtm.Id));
            var sociedad = contexto.SeleccionarPorId<SociedadDtm>(idSociedad);
            var actEdDeLaSociedad = sociedad.ActividadesDeEstimacionDirecto();
            var actividades = actEdDeLaSociedad.Select(a => a.Actividad).Distinct();
            foreach (var actividad in actividades)
            {
                var tran = contexto.IniciarTransaccion();
                try
                {
                    GenerarEstimacionDirecta(entorno, entornoTrabajo, sociedad, actEdDeLaSociedad, actividad, ejercicio);
                    entornoTrabajo?.CrearTraza($"Actividad '{actividad}' procesada para la sociedad '{sociedad.RazonSocial}' en el ejercicio '{ejercicio}'");
                    contexto.Commit(tran);
                }
                catch (Exception ex)
                {
                    entornoTrabajo?.AnotarError(ex);
                    contexto.Rollback(tran);

                    if (entornoTrabajo == null) throw;
                }
            }

        }

        public static void ExportarTodosLosTercerosNcs(EntornoDeUnaAccion entorno)
        {
            var idSociedad = entorno.Entrada.LeerValor<int>(nameof(SociedadDtm.Id));
            var sociedad = entorno.Contexto.SeleccionarPorId<SociedadDtm>(idSociedad);
            var generadorDelote = new LoteContableConNcs(entorno.Contexto, new List<PreasientoDtm>(), sociedad, ejercicio: DateTime.Now.Year,respetarFechaContable: true, fechaContable: null);
            var proveedores = entorno.Contexto.Set<ProveedorDtm>().Where(p => p.Baja == false).ToList();
            var clientes = entorno.Contexto.Set<ClienteDtm>().Where(c => c.Baja == false).ToList();

            var circuito = ExtensorDePreasientos.CrearCircuitoDeLotesDeTerceros(entorno.Contexto, sociedad, $"Lote de todos los terceros '{DateTime.Now.ToString()}'", DateTime.Now.Year);

            ExportarTercerosNcs(generadorDelote, circuito, (proveedores, clientes));

            var entornoTrabajo = entorno.Entrada.LeerValor<EntornoDeTrabajo>(nameof(EntornoDeTrabajo), null);
            entornoTrabajo.Enlaces.Add(TipoDtoElmento.Crear<CircuitoDocDto>(circuito));
        }

        public static void RegenerarLoteContable(EntornoDeUnaAccion entorno)
        {
            var contexto = entorno.Contexto;
            var idLote = entorno.Entrada.LeerValor<int>(ltrDeUnLoteContable.IdLoteContable);
            var lote = entorno.Contexto.SeleccionarPorId<CircuitoDocDtm>(idLote);
            var preasientos = lote.Vinculados<PreasientoDtm>(entorno.Contexto);
            if (preasientos.Count == 0)
            {
                GestorDeErrores.Emitir($"El lote contable '{lote.Referencia}' no tiene preasientos asociados, no se puede regenerar el lote contable");
            }

            lote.MarcarArchivosComoCancelados(contexto);
            var generadorNcs = new LoteContableConNcs(entorno.Contexto, preasientos, preasientos.First().Sociedad(contexto), ejercicio: DateTime.Now.Year, respetarFechaContable: true, fechaContable: null);
            try
            {
                generadorNcs.GenerarLoteContable();
                var nombre = entorno.Accion.Expresion + $" el {DateTime.Now.ToString("yyyy-MM-dd HH-mm")}";
                var archivoDeAsientos = lote.AnexarArchivo(entorno.Contexto, generadorNcs.DiarioXml);
                new BloqueoDeUnArchivoDtm
                {
                    IdArchivo = archivoDeAsientos.Id,
                    Bloqueado = true
                }.Insertar(entorno.Contexto, new Dictionary<string, object> { { nameof(AuditoriaDeUnArchivoDtm.Auditoria), ltrDeUnArchivo.Migrado } });
                lote.CrearTraza(contexto, nombre: ltrTrazasDeUnLoteContable.RegenerarlLote, descripción: $"El lote ha sido regenerado por el usuario '{contexto.Usuario.Login}'");
            }
            finally
            {
                generadorNcs.Deshacer();
            }
        }

        private static (List<ProveedorDtm> proveedoresContables, List<ClienteDtm> clientesContables) VincularPreasientos(ContextoSe contextoDeLaAccion, TrabajoDeUsuarioDtm trabajo, ContextoSe contextoDelTrabajo, List<PreasientoDtm> contabilizarEnNcs, CircuitoDocDtm circuito)
        {
            var proveedoresContables = new List<ProveedorDtm>();
            var clientesContables = new List<ClienteDtm>();

            foreach (var preasiento in contabilizarEnNcs)
            {
                if (trabajo is not null && contextoDelTrabajo is not null)
                    GestorDeTrazasDeUnTrabajo.AnotarTraza(contextoDelTrabajo, trabajo, $"preasiento '{preasiento.Referencia}' enviado a Ncs");

                if (circuito is not null)
                {
                    GestorDeVinculos.Vincular(contextoDeLaAccion, preasiento, circuito);
                }

                var proveedor = preasiento.Proveedor(contextoDeLaAccion, errorSiNoHay: false);
                if (proveedor is not null && !proveedoresContables.Any(p => p.Id == proveedor.Id))
                {
                    if (!proveedor.Migrado(contextoDeLaAccion, enumSistemaContable.NCS, preasiento.Cg(contextoDeLaAccion).Sociedad(contextoDeLaAccion).PlanContable().IdPlanContable))
                    {
                        proveedoresContables.Add(proveedor);
                    }
                }

                var cliente = preasiento.Cliente(contextoDeLaAccion, errorSiNoHay: false);
                if (cliente is not null && !clientesContables.Any(c => c.Id == cliente.Id))
                {
                    if (!cliente.Migrado(contextoDeLaAccion, enumSistemaContable.NCS, preasiento.Cg(contextoDeLaAccion).Sociedad(contextoDeLaAccion).PlanContable().IdPlanContable))
                    {
                        clientesContables.Add(cliente);
                    }
                }
            }

            return (proveedoresContables, clientesContables);
        }

        private static void EnviarCircuito(EntornoDeTrabajo entornoTrabajo, CircuitoDocDtm circuito, string vista)
        {
            if (entornoTrabajo == null) { return; }

            var contextoDeLaAccion = entornoTrabajo.contextoDelProceso;

            var enlace = TipoDtoElmento.Crear<CircuitoDocDto>(circuito, vista);
            entornoTrabajo.Enlaces.Add(enlace);

            GestorDeCorreos.CrearCorreoPara(contextoDeLaAccion
                , new List<string> { contextoDeLaAccion.SeleccionarPorId<UsuarioDtm>(entornoTrabajo.TrabajoDeUsuario.IdEjecutor).eMail }
                , "Preasientos a importar en Ncs"
                , "Se adjunta el enlaces a los xmls de los preasientos a importar en Ncs"
                , new List<TipoDtoElmento> { enlace }
                , new List<string>()
                );
        }

        private static void ExportarTercerosNcs(LoteContableConNcs generadorDeLote, CircuitoDocDtm circuito, (List<ProveedorDtm> proveedores, List<ClienteDtm> clientes) terceros)
        {
            ContextoSe contextoDeLaAccion = generadorDeLote.Contexto;
            if (terceros.proveedores.Any() || terceros.clientes.Any())
            {
                ArchivoDtm archivoAsociado = null;

                generadorDeLote.GenerarTerceros(terceros);
                foreach (var proveedor in terceros.proveedores)
                {
                    if (archivoAsociado is null)
                        archivoAsociado = ServidorDocumental.AnexarArchivo(contextoDeLaAccion, enumNegocio.Proveedor, proveedor.Id, generadorDeLote.TercerosXml, sanitizar: false);
                    else
                        GestorDeVinculos.Vincular(contextoDeLaAccion, proveedor, archivoAsociado);
                    proveedor.MarcarComoMigrado(contextoDeLaAccion, enumSistemaContable.NCS, generadorDeLote.Sociedad.PlanContable().IdPlanContable);
                }

                foreach (var cliente in terceros.clientes)
                {
                    if (archivoAsociado is null)
                        archivoAsociado = ServidorDocumental.AnexarArchivo(contextoDeLaAccion, enumNegocio.Cliente, cliente.Id, generadorDeLote.TercerosXml, sanitizar: false);
                    else
                        GestorDeVinculos.Vincular(contextoDeLaAccion, cliente, archivoAsociado);
                    cliente.MarcarComoMigrado(contextoDeLaAccion, enumSistemaContable.NCS, generadorDeLote.Sociedad.PlanContable().IdPlanContable);
                }

                circuito.Vincular(contextoDeLaAccion, archivoAsociado);
                new BloqueoDeUnArchivoDtm
                {
                    IdArchivo = archivoAsociado.Id,
                    Bloqueado = true
                }.Insertar(contextoDeLaAccion, new Dictionary<string, object> { { nameof(AuditoriaDeUnArchivoDtm.Auditoria), ltrDeUnArchivo.Migrado } });

            }

        }

        private static void GenerarEstimacionDirecta(EntornoDeUnaAccion entorno, EntornoDeTrabajo entornoTrabajo, SociedadDtm sociedad, List<ActividadesEstimacionDirecta> actividades, string actividad, int ejercicio)
        {
            ContextoSe contexto = entorno.Contexto;

            var filtrosJson = entorno.Entrada.LeerValor(ltrFiltros.filtro, "");
            var filtros = filtrosJson.IsNullOrEmpty() ? new List<ClausulaDeFiltrado>() : JsonConvert.DeserializeObject<List<ClausulaDeFiltrado>>(filtrosJson);
            List<FacturaEmtDtm> emitidas = FacturasEmitidasParaEstimar(sociedad, actividades, actividad, ejercicio, contexto, filtros);
            List<FacturaRecDtm> recibidas = FacturasRecibidasParaEstimar(sociedad, actividades, actividad, ejercicio, contexto, filtros);
            List<PagoDtm> pagos = PagosParaEstimar(sociedad, actividades, actividad, ejercicio, contexto, filtros);

            var ed = new EstimacionDirectaNcs(contexto, sociedad, ejercicio, actividad);
            try
            {
                ed.GenerarAsientosEd(emitidas, recibidas, pagos);
                var hoy = DateTime.Now;
                var nombre = entorno.Accion.Expresion + $" el {hoy.ToString("yyyy-MM-dd HH-mm")}";
                var circuito = ExtensorDePreasientos.CrearCircuitoDeLotes(contexto, sociedad, nombre, ejercicio, null);
                var archivoDeAsientos = circuito.AnexarArchivoConBloqueo(contexto, ed.DiarioXml, ltrDeUnArchivo.Migrado);
                if (ed.HayTerceros)
                {
                    var archivosDeTerceros = circuito.AnexarArchivoConBloqueo(contexto, ed.TercerosXml, ltrDeUnArchivo.Migrado);
                }
                foreach (var emitida in emitidas) GestorDeVinculos.Vincular(contexto, emitida, circuito);
                foreach (var recibida in recibidas) GestorDeVinculos.Vincular(contexto, recibida, circuito);
                foreach (var pago in pagos) GestorDeVinculos.Vincular(contexto, pago, circuito);
                EnviarCircuito(entornoTrabajo, circuito, enumVistasSistemaDocumental.CrudEstimacionesDirectas);
            }
            finally
            {
                ed.Deshacer();
            }
        }

        private static List<FacturaEmtDtm> FacturasEmitidasParaEstimar(SociedadDtm sociedad, List<ActividadesEstimacionDirecta> actividades, string actividad, int ejercicio, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            List<FacturaEmtDtm> emitidas;

            if (filtros.Count == 1 && (filtros.Any(f => f.Clausula == ltrDeUnPreasiento.IdFacturaRecibida) || filtros.Any(f => f.Clausula == ltrDeUnPreasiento.IdPago)))
                return new List<FacturaEmtDtm>();

            var filtro = filtros.FirstOrDefault(f => f.Clausula == nameof(ltrDeUnPreasiento.IdFacturaEmitida));
            if (filtro != null)
            {
                filtro.Clausula = nameof(RegistroDtm.Id);
                emitidas = contexto.SeleccionarTodos<FacturaEmtDtm>(filtros, aplicarJoin: true);
            }
            else
            {
                var tipos = actividades.Where(act => act.Actividad == actividad && act.IdNegocio == enumNegocio.FacturaEmitida.IdNegocio()).Select(act => act.IdTipo).Distinct();
                var estadosDeFacturasNoContabilizables = (enumEtapasDeFacturasEmt.FAE_Etapa_Prefactura.Estados() + "," + enumEtapasDeFacturasEmt.FAE_Etapa_Anulada.Estados()).ToLista<int>(separador: ",");
                var circuitosCancelados = enumEtapasDeCircuitosDoc.CAD_Etapa_Cancelado.Estados().ToLista<int>();

                var consulta = contexto.Set<FacturaEmtDtm>().Include(emt => emt.Cliente).Where(emt =>
                       sociedad.CentrosGestores(contexto).Any(cg => cg.Id == emt.IdCg) &&
                       tipos.Contains(emt.IdTipo) &&
                       !estadosDeFacturasNoContabilizables.Contains(emt.IdEstado) &&
                       emt.Ano != null && emt.Ano == ejercicio &&
                       (
                        //La factura no está en ningún circuito
                        !contexto.Set<CircuitoDocDeUnaFacturaEmtDtm>().Any(cir => cir.idElemento1 == emt.Id)
                          ||
                        //o si lo está, en todos los que esté, están cancelados
                        contexto.Set<CircuitoDocDeUnaFacturaEmtDtm>().Where(cir => cir.idElemento1 == emt.Id)
                        .All(cir => contexto.Set<CircuitoDocDtm>().Any(fc => fc.Id == cir.idElemento2 && circuitosCancelados.Contains(fc.IdEstado)))
                       )
                );

                emitidas = consulta.ToList();
            }

            return emitidas;
        }

        private static List<FacturaRecDtm> FacturasRecibidasParaEstimar(SociedadDtm sociedad, List<ActividadesEstimacionDirecta> actividades, string actividad, int ejercicio, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            List<FacturaRecDtm> recibidas;
            if (filtros.Count == 1 && (filtros.Any(f => f.Clausula == ltrDeUnPreasiento.IdFacturaEmitida) || filtros.Any(f => f.Clausula == ltrDeUnPreasiento.IdPago)))
                return new List<FacturaRecDtm>();

            var filtro = filtros.FirstOrDefault(f => f.Clausula == nameof(ltrDeUnPreasiento.IdFacturaRecibida));
            if (filtro != null)
            {
                filtro.Clausula = nameof(RegistroDtm.Id);
                recibidas = contexto.SeleccionarTodos<FacturaRecDtm>(filtros, aplicarJoin: true);
            }
            else
            {
                var tipos = actividades.Where(act => act.Actividad == actividad && act.IdNegocio == enumNegocio.FacturaRecibida.IdNegocio()).Select(act => act.IdTipo).Distinct();
                var estadosDeFacturas = (enumEtapasDeFacturasRec.FAR_Etapa_De_Pago.Estados() + "," + enumEtapasDeFacturasRec.FAR_Etapa_Pagada.Estados()).ToLista<int>(separador: Simbolos.Coma);
                var circuitosCancelados = enumEtapasDeCircuitosDoc.CAD_Etapa_Cancelado.Estados().ToLista<int>();

                var consulta = contexto.Set<FacturaRecDtm>().Include(rec => rec.Proveedor).Where(rec =>
                       sociedad.CentrosGestores(contexto).Any(cg => cg.Id == rec.IdCg) &&
                       tipos.Contains(rec.IdTipo) &&
                       estadosDeFacturas.Contains(rec.IdEstado) &&
                       rec.ContabilizadaEl != null && ((DateTime)rec.FacturadaEl).Year == ejercicio &&
                       (
                         // La factura no está en ningún circuito
                         !contexto.Set<CircuitoDocDeUnaFacturaRecDtm>().Any(cir => cir.idElemento1 == rec.Id)
                            ||
                         //o si lo está , entonces todos los circuitos donde está la factura están cancelados
                         contexto.Set<CircuitoDocDeUnaFacturaRecDtm>().Where(cir => cir.idElemento1 == rec.Id)
                        .All(cir => contexto.Set<CircuitoDocDtm>().Any(fc => fc.Id == cir.idElemento2 && circuitosCancelados.Contains(fc.IdEstado)))
                      )
                 );

                recibidas = consulta.ToList();
            }

            return recibidas;
        }

        private static List<PagoDtm> PagosParaEstimar(SociedadDtm sociedad, List<ActividadesEstimacionDirecta> actividades, string actividad, int ejercicio, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            List<PagoDtm> pagos;
            if (filtros.Count == 1 && (filtros.Any(f => f.Clausula == ltrDeUnPreasiento.IdFacturaEmitida) || filtros.Any(f => f.Clausula == ltrDeUnPreasiento.IdFacturaRecibida)))
                return new List<PagoDtm>();

            var filtro = filtros.FirstOrDefault(f => f.Clausula == nameof(ltrDeUnPreasiento.IdPago));
            if (filtro != null)
            {
                filtro.Clausula = nameof(RegistroDtm.Id);
                pagos = contexto.SeleccionarTodos<PagoDtm>(filtros, aplicarJoin: true);
            }
            else
            {
                var tipos = actividades.Where(act => act.Actividad == actividad && act.IdNegocio == enumNegocio.Pago.IdNegocio()).Select(act => act.IdTipo).Distinct();
                var estados = enumEtapasDePagos.PAG_Etapa_Pagado.Estados().ToLista<int>(separador: ",");
                var circuitosCancelados = enumEtapasDeCircuitosDoc.CAD_Etapa_Cancelado.Estados().ToLista<int>();


                pagos = contexto.Set<PagoDtm>().Where(pag =>
                       sociedad.CentrosGestores(contexto).Any(cg => cg.Id == pag.IdCg) &&
                       pag.IdNaturaleza != null &&
                       tipos.Contains(pag.IdTipo) &&
                       estados.Contains(pag.IdEstado) &&
                       pag.PagadoEl != null && ((DateTime)pag.PagadoEl).Year == ejercicio &&
                       (
                            // El pago no está en ningún circuito
                            !contexto.Set<CircuitoDocDeUnPagoDtm>().Any(cir => cir.idElemento1 == pag.Id)
                            ||
                            //o si lo está, entonces todos los circuitos donde está el pago están cancelados
                            contexto.Set<CircuitoDocDeUnPagoDtm>().Where(cir => cir.idElemento1 == pag.Id)
                            .All(cir => contexto.Set<CircuitoDocDtm>().Any(fc => fc.Id == cir.idElemento2 && circuitosCancelados.Contains(fc.IdEstado)))
                       )
                       ).ToList();
            }

            return pagos;
        }
    }
}
