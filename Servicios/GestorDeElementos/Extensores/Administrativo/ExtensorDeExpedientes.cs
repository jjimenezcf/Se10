using AutoMapper.Internal;
using Gestor.Errores;
using ModeloDeDto;
using ModeloDeDto.Gastos;
using ModeloDeDto.Ventas;
using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.Gastos;
using ServicioDeDatos.Juridico;
using ServicioDeDatos.Logistica;
using ServicioDeDatos.MaestrosTecnico;
using ServicioDeDatos.Presupuesto;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Tarea;
using ServicioDeDatos.Ventas;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilidades;
using static ServicioDeDatos.Elemento.Enumerados;

namespace GestorDeElementos.Extensores
{
    public class Tiempos
    {
        public decimal? Horas { get; }
        public DateTime? FecIniPla { get; }
        public DateTime? FecFinPla { get; }
        public string Planificacion => FecIniPla is null && FecFinPla is null ? "" : $"{FecIniPla?.ToString("dd-MM-yyyy")} / {FecFinPla?.ToString("dd-MM-yyyy")}";
        public DateTime? FecIniRea { get; }
        public DateTime? FecFinRea { get; }
        public string Ejecucion => FecIniRea is null && FecFinRea is null ? "" : $"{FecIniRea?.ToString("dd-MM-yyyy")} / {FecFinRea?.ToString("dd-MM-yyyy")}";

        public Tiempos(List<TareaDtm> tareas, ContextoSe contexto)
        {
            decimal minutos = 0;
            DateTime? fecIniPla = null;
            DateTime? fecFinPla = null;
            DateTime? fecIniRea = null;
            DateTime? fecFinRea = null;
            foreach (var tarea in tareas)
            {
                var planificacion = tarea.Planificacion(contexto, false);
                if (planificacion is null) continue;
                if (planificacion.PlfDeInicio is not null && (fecIniPla is null || planificacion.PlfDeInicio < fecIniPla)) fecIniPla = planificacion.PlfDeInicio;
                if (planificacion.PlfDeFin is not null && (fecFinPla is null || planificacion.PlfDeFin > fecFinPla)) fecFinPla = planificacion.PlfDeFin;
                if (planificacion.Iniciada is not null && (fecIniRea is null || planificacion.Iniciada > fecIniRea)) fecIniRea = planificacion.Iniciada;
                if (planificacion.Finalizada is not null && (fecFinRea is null || planificacion.Finalizada > fecFinRea)) fecFinRea = planificacion.Finalizada;
                if (planificacion.EnMinutos() is not null) minutos = minutos + (decimal)planificacion.EnMinutos();
            }
            Horas = minutos == 0 ? null : minutos / 60;
            FecIniPla = fecIniPla;
            FecFinPla = fecFinPla;
            FecIniRea = fecIniRea;
            FecFinRea = fecFinRea;
        }
    }

    public static class ExtensorDeExpedientes
    {

        public static bool UsaElDetalleDe(ContextoSe contexto, int idTipo, Type tipoDeDetalle)
        {
            var tipoDtm = (TipoDeExpedienteDtm)enumNegocio.Expediente.CrearGestorDeTipo(contexto).LeerRegistroPorId(idTipo, aplicarJoin: false);

            if (tipoDtm.UsaPpts && tipoDeDetalle == typeof(ApunteDeExpedienteDtm))
                return true;

            return false;
        }


        public static bool UsaLaAmpliacionDe(ContextoSe contexto, int idTipo, Type tipoAmpliacion)
        {
            var tipoDtm = (TipoDeExpedienteDtm)enumNegocio.Expediente.CrearGestorDeTipo(contexto).LeerRegistroPorId(idTipo, aplicarJoin: false);
            if (tipoDtm.UsaDatosJuridicos && tipoAmpliacion == typeof(DatosJuridicosDtm))
                return true;

            return false;
        }

        public static bool HayTiposJuridicos(ContextoSe contexto)
        {
            return contexto.SeleccionarTodos<TipoDeExpedienteDtm>(new Dictionary<string, object>
            {
                { nameof(TipoDeExpedienteDtm.UsaDatosJuridicos), true},
                { nameof(TipoDeExpedienteDtm.Activo), true }
            }).Any();
        }

        public static bool EstaEnLaEtapa(this ExpedienteDtm expediente, string etapa) => etapa.ToLista<int>(",").Contains(expediente.IdEstado);

        //public static bool EstaEnLaEtapa(this ExpedienteDtm expediente, enumEtapasDeExpedientes etapa) => etapa.LeerVariable().ToLista<int>(",").Contains(expediente.IdEstado);

        public static void Persistir(this enumEtapasDeExpedientes etapa, ContextoSe contexto, List<int> listaDeEstados, bool restear = true)
        {
            if (restear)
            {
                etapa.Persistir(contexto, listaDeEstados.ToString(","));
                return;
            }

            var etapaActual = etapa.LeerVariable().ToLista<int>(",");
            if (etapaActual.Count == 1 && etapaActual[0] == 0)
                etapaActual = new List<int>();

            foreach (var estado in listaDeEstados)
            {
                if (estado <= 0)
                    continue;
                if (!etapaActual.Contains(estado)) etapaActual.Add(estado);
            }

            if (etapaActual.Count > 0) etapa.Persistir(contexto, etapaActual.ToString(","));
        }

        public static void AntesDeCancelar2(this ExpedienteDtm expediente, ContextoSe contexto)
        {
            var tipo = expediente.Tipo<TipoDeExpedienteDtm>(contexto);

            if (tipo.UsaTareas && expediente.Tareas(contexto).Count() > 0)
                GestorDeErrores.Emitir($"No se puede cancelar el {enumNegocio.Expediente.Singular(true)} '{expediente.Referencia}' por tener {enumNegocio.Tarea.Plural(true)} tareas realizadas o en proceso");

            expediente.ValidarSiExistenDespendientes<FacturaRecDtm>(contexto);
            expediente.ValidarSiExistenDespendientes<PedidoDtm>(contexto);
            expediente.ValidarSiExistenDespendientes<PresupuestoDtm>(contexto);
            expediente.ValidarSiExistenDespendientes<ContratoDtm>(contexto);
        }

        public static void AntesDeCancelar(this ExpedienteDtm expediente, ContextoSe contexto)
        {
            var tipo = expediente.Tipo<TipoDeExpedienteDtm>(contexto);

            if (tipo.UsaTareas && expediente.Tareas(contexto).Any())
                GestorDeErrores.Emitir($"No se puede cancelar el {enumNegocio.Expediente.Singular(true)} '{expediente.Referencia}' por tener {enumNegocio.Tarea.Plural(true)} realizadas o en proceso");

            // Obtenemos la definición del método genérico
            var metodoGenericoBase = typeof(ExtensorDeElementosDeUnProceso).GetMethod(nameof(ExtensorDeElementosDeUnProceso.ValidarSiExistenDespendientes));

            foreach (var t in ApiDeInterfaceDtm.EncontrarTiposConLaInterface(typeof(IUsaExpediente)))
            {
                // Convertimos el método genérico a la clase específica: ValidarSiExistenDespendientes<T>
                var metodoCerrado = metodoGenericoBase.MakeGenericMethod(t);
                metodoCerrado.Invoke(null, new object[] { expediente, contexto });
            }
        }

        public static void CopiarDireccionesDel(this ExpedienteDtm expediente, ContextoSe contexto, IElementoDtm origen, enumCalificadorDireccion calificador, bool errorSiNoHay = false)
        {
            var direcciones = origen.Direcciones(contexto);

            var copiadas = 0;

            foreach (var direccion in direcciones)
                if (calificador == direccion.Calificador)
                {
                    expediente.AsociarDireccion(contexto, direccion);
                    copiadas++;
                }


            if (errorSiNoHay && copiadas == 0)
                GestorDeErrores.Emitir($"No hay definidas direcciones de '{calificador.Descripcion()}' en el '{origen.Referencia(contexto)}' ");
        }

        public static void AntesDeIniciar(this ExpedienteDtm expediente, ContextoSe contexto)
        {
            var hayDireccionDeContacto = expediente.Direccion(contexto, enumCalificadorDireccion.contacto) != null;
            var hayDireccionFiscal = expediente.Direccion(contexto, enumCalificadorDireccion.fiscal) != null;
            if (expediente.TienePresupuestos(contexto) && !hayDireccionDeContacto && !hayDireccionFiscal)
                GestorDeErrores.Emitir($"No se puede iniciar el {enumNegocio.Expediente.Singular(true)} '{expediente.Referencia}' por no tener dirección de '{enumCalificadorDireccion.contacto.Descripcion()}' o '{enumCalificadorDireccion.fiscal.Descripcion()}' asociada, o porque aun teníendolas no están activa");
        }

        public static void TrasIniciar(this ExpedienteDtm expediente, ContextoSe contexto)
        {
            //var tipo = expediente.Tipo<TipoDeExpedienteDtm>(contexto);
            //if (tipo.UsaPpts)
            //{
            //    if (!expediente.TienePresupuestos(contexto))
            //        GestorDeErrores.Emitir($"No se puede iniciar el {enumNegocio.Expediente.Singular(true)} '{expediente.Referencia}' por no tener {enumNegocio.Presupuesto.Plural(true)} asociados");
            //}
        }

        public static void AntesDeTerminar(this ExpedienteDtm expediente, ContextoSe contexto)
        {
            var tipo = expediente.Tipo<TipoDeExpedienteDtm>(contexto);

            if (tipo.UsaTareas && expediente.Tareas(contexto, excluirTerminadas: true).Count() > 0)
                GestorDeErrores.Emitir($"No se puede cerrar el {enumNegocio.Expediente.Singular(true)} '{expediente.Referencia}' por tener {enumNegocio.Tarea.Plural(true)} en proceso");

            //if (tipo.UsaPpts && !expediente.TienePresupuestos(contexto, excluirTerminados: false))
            //    GestorDeErrores.Emitir($"No se puede cerrar el {enumNegocio.Expediente.Singular(true)} '{expediente.Referencia}' por no tener {enumNegocio.Presupuesto.Plural(true)}");

            if (expediente.Presupuestos(contexto, excluirTerminados: true).Count > 0)
                GestorDeErrores.Emitir($"No se puede cerrar el {enumNegocio.Expediente.Singular(true)} '{expediente.Referencia}' por tener {enumNegocio.Presupuesto.Plural(true)} en proceso");

        }

        public static List<PresupuestoDtm> Presupuestos(this ExpedienteDtm expediente, ContextoSe contexto, bool excluirCancelados = true, bool excluirTerminados = false)
        {
            var filtro = new Dictionary<string, object> { { nameof(PresupuestoDtm.IdExpediente), expediente.Id } };

            var presupuestos = contexto.SeleccionarTodos<PresupuestoDtm>(filtro, parametros: new Dictionary<string, object>
                {
                    { ltrParametrosNeg.ExcluirCancelados, excluirCancelados },
                    { ltrParametrosNeg.ExcluirTerminados, excluirTerminados }
                });
            return presupuestos;
        }

        public static decimal PresupuestadoEn(this ExpedienteDtm expediente, ContextoSe contexto)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Exp_Presupuestado);
            if (!cache.ContainsKey(expediente.Id.ToString()))
            {
                if (expediente.Tipo<TipoDeExpedienteDtm>(contexto).UsaPpts)
                {
                    var ppts = expediente.Presupuestos(contexto);
                    cache[expediente.Id.ToString()] = ppts.Sum(p => p.Total(contexto, conIva: false));
                }
                else
                    cache[expediente.Id.ToString()] = (decimal)0;
            }
            return (decimal)cache[expediente.Id.ToString()];
        }

        public static List<FacturaRecDtm> FacturasRec(this ExpedienteDtm expediente, ContextoSe contexto, bool excluirCancelados = true, bool excluirTerminados = false)
        {
            var filtro = new Dictionary<string, object> { { nameof(FacturaRecDtm.IdExpediente), expediente.Id } };

            var facturas = contexto.SeleccionarTodos<FacturaRecDtm>(filtro, parametros: new Dictionary<string, object>
                {
                    { ltrParametrosNeg.ExcluirCancelados, excluirCancelados },
                    { ltrParametrosNeg.ExcluirTerminados, excluirTerminados }
                });
            return facturas;
        }


        public static decimal Gastos(this ExpedienteDtm expediente, ContextoSe contexto)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Exp_Gastos);
            if (!cache.ContainsKey(expediente.Id.ToString()))
            {
                if (expediente.Tipo<TipoDeExpedienteDtm>(contexto).UsaPpts)
                {
                    var fars = expediente.FacturasRec(contexto);
                    var apuntes = expediente.Detalles<ApunteDeExpedienteDtm>(contexto).Where(a => a.Clase == enumApunteDeExpediente.Gasto);

                    cache[expediente.Id.ToString()] = fars.Sum(f => f.BaseImponible) + apuntes.Sum(a => a.Valor);
                }
                else
                    cache[expediente.Id.ToString()] = (decimal)0;
            }
            return (decimal)cache[expediente.Id.ToString()];
        }

        public static List<ContratoDtm> Contratos(this ExpedienteDtm expediente, ContextoSe contexto, bool excluirCancelados = true, bool excluirTerminados = false)
        {
            var filtro = new Dictionary<string, object> { { nameof(ContratoDtm.IdExpediente), expediente.Id } };

            var contratos = contexto.SeleccionarTodos<ContratoDtm>(filtro, parametros: new Dictionary<string, object>
                {
                    { ltrParametrosNeg.ExcluirCancelados, excluirCancelados },
                    { ltrParametrosNeg.ExcluirTerminados, excluirTerminados }
                });
            return contratos;
        }


        public static decimal Ingresos(this ExpedienteDtm expediente, ContextoSe contexto)
        {
            if (!expediente.Tipo<TipoDeExpedienteDtm>(contexto).UsaPpts)
                return 0;

            var cache = ServicioDeCaches.Obtener(CacheDe.Exp_Ingresos);
            if (!cache.ContainsKey(expediente.Id.ToString()))
            {
                var idsPpts = expediente.Presupuestos(contexto).Select(p => p.Id).ToList();
                var idsCtrs = expediente.Contratos(contexto).Select(c => c.Id).ToList();
                var faesDelPpt = contexto.Set<FacturaEmtDtm>().Where(f => f.IdPresupuesto != null && idsPpts.Contains((int)f.IdPresupuesto)).ToList();
                var faesDelCtr = contexto.Set<FacturaEmtDtm>().Where(f => f.IdContrato != null && idsCtrs.Contains((int)f.IdContrato)).ToList();

                var faes = faesDelPpt
                .Concat(faesDelCtr) // Combinar ambas listas
                .GroupBy(f => f.Id) // Agrupar por el Id de la Factura
                .Select(g => g.First()) // Seleccionar el primer elemento de cada grupo (garantizando unicidad por Id)
                .ToList();

                var apuntes = expediente.Detalles<ApunteDeExpedienteDtm>(contexto).Where(a => a.Clase == enumApunteDeExpediente.Ingreso);
                var facturas = faes.Where(f => !f.EstaEnAlgunaDeLasEtapa(new List<enumEtapasDeFacturasEmt> { enumEtapasDeFacturasEmt.FAE_Etapa_Prefactura, enumEtapasDeFacturasEmt.FAE_Etapa_Anulada }));
                cache[expediente.Id.ToString()] = facturas.Sum(f => f.Total(contexto, enumImporteEmt.BaseImponible)) + apuntes.Sum(a => a.Valor);
            }
            return (decimal)cache[expediente.Id.ToString()];
        }

        public static decimal Pagos(this ExpedienteDtm expediente, ContextoSe contexto)
        {
            if (!expediente.Tipo<TipoDeExpedienteDtm>(contexto).UsaPpts)
                return 0;

            var cache = ServicioDeCaches.Obtener(CacheDe.Exp_Pagos);
            if (!cache.ContainsKey(expediente.Id.ToString()))
            {
                var farsImp = expediente.FacturasRec(contexto);
                var idsCtrsImp = expediente.Contratos(contexto).Select(c => c.Id).ToList();
                var faesDelCtr = contexto.Set<FacturaRecDtm>().Where(f => f.IdContrato != null && idsCtrsImp.Contains((int)f.IdContrato)).ToList().Where(f => f.EstaContabilizada());
                var fars = farsImp
                .Concat(faesDelCtr) // Combinar ambas listas
                .GroupBy(f => f.Id) // Agrupar por el Id de la Factura
                .Select(g => g.First()) // Seleccionar el primer elemento de cada grupo (garantizando unicidad por Id)
                .ToList();

                cache[expediente.Id.ToString()] = fars.Sum(f => f.PagosRealizados(contexto).Sum(p => p.Importe));
            }
            return (decimal)cache[expediente.Id.ToString()];
        }

        public static decimal Cobros(this ExpedienteDtm expediente, ContextoSe contexto)
        {
            if (!expediente.Tipo<TipoDeExpedienteDtm>(contexto).UsaPpts)
                return 0;

            var cache = ServicioDeCaches.Obtener(CacheDe.Exp_Cobros);
            if (!cache.ContainsKey(expediente.Id.ToString()))
            {
                var ppts = contexto.Set<PresupuestoDtm>().Where(p => p.IdExpediente == expediente.Id);
                var ctrs = contexto.Set<ContratoDtm>().Where(c => c.IdExpediente == expediente.Id);
                var faesDelPpt = contexto.Set<FacturaEmtDtm>().Where(f => ppts.Any(p => p.Id == f.IdPresupuesto)).ToList();
                var faesDelCtr = contexto.Set<FacturaEmtDtm>().Where(f => ctrs.Any(c => c.Id == f.IdContrato)).ToList();

                var faes = faesDelPpt
                .Concat(faesDelCtr) // Combinar ambas listas
                .GroupBy(f => f.Id) // Agrupar por el Id de la Factura
                .Select(g => g.First()) // Seleccionar el primer elemento de cada grupo (garantizando unicidad por Id)
                .ToList();

                cache[expediente.Id.ToString()] = faes.Sum(f => f.Cobrado(contexto));
            }
            return (decimal)cache[expediente.Id.ToString()];
        }

        public static bool TienePresupuestos(this ExpedienteDtm expediente, ContextoSe contexto, bool excluirCancelados = true, bool excluirTerminados = false)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Exp_TienePresupuestos);
            if (!cache.ContainsKey(expediente.Id.ToString()))
                cache[expediente.Id.ToString()] = expediente.Presupuestos(contexto, excluirCancelados, excluirTerminados).Count() > 0;
            return (bool)cache[expediente.Id.ToString()];
        }

        public static PresupuestoDtm CrearPresupuesto(this ExpedienteDtm expediente, ContextoSe contexto, string tipo)
        {
            var pptDeObra = new PresupuestoDtm
            {
                IdTipo = contexto.SeleccionarPorNombre<TipoDePresupuestoDtm>(tipo).Id,
                IdCg = expediente.IdCg,
                Nombre = $"Valoración del expediente {expediente.Referencia}",
                Descripcion = $"{expediente.Descripcion}",
                IdExpediente = expediente.Id,
                ClaseDePresupuesto = enumClaseDePresupuesto.venta,
                IdSolicitante = expediente.IdSolicitante
            }.Insertar(contexto);

            return pptDeObra;
        }

        public static PresupuestoDtm PresupuestarTareasRealizadas(this ExpedienteDtm expediente, ContextoSe contexto, TipoDePresupuestoDtm tipoPpt, NaturalezaDtm naturaleza, decimal precio)
        {
            var pptDeTareas = new PresupuestoDtm
            {
                IdTipo = tipoPpt.Id,
                IdCg = expediente.IdCg,
                Nombre = $"Valoración del expediente {expediente.Referencia}",
                Descripcion = $"{expediente.Descripcion}",
                IdExpediente = expediente.Id,
                ClaseDePresupuesto = enumClaseDePresupuesto.venta,
                IdSolicitante = expediente.IdSolicitante
            }.Insertar(contexto);

            var iva = contexto.SeleccionarPorPropiedad<IvaRepercutidoDtm>(nameof(IvaRepercutidoDtm.Clase), VariablesDeIvaRep.General);
            var unidad = contexto.SeleccionarPorPropiedad<UnidadDtm>(nameof(UnidadDtm.Sigla), VariablesDeUnidadDeMedida.Horas);
            var tareas = expediente.Tareas(contexto).ToList(); ;
            var numero = 10;
            foreach (var tarea in tareas)
            {
                var planificacion = tarea.Planificacion(contexto, errorSiNoHay: false);
                if (planificacion is null || planificacion.MedidoEn is null) continue;
                decimal minutos = 0;
                switch (planificacion.MedidoEn)
                {
                    case Enumerados.enumDurabilidad.Minutos: minutos = minutos + (decimal)planificacion.Duracion; break;
                    case Enumerados.enumDurabilidad.Horas: minutos = minutos + (decimal)planificacion.Duracion * 60; break;
                    case Enumerados.enumDurabilidad.Jornadas: minutos = minutos + (decimal)planificacion.Duracion * 60 * 8; break;
                    case Enumerados.enumDurabilidad.Dias: minutos = minutos + (decimal)planificacion.Duracion * 60 * 24; break;
                }
                if (minutos == 0) continue;
                if (contexto.Set<TareasDeUnPresupuestoDtm>().Any(x => x.idElemento2 == tarea.Id &&
                    contexto.Set<PresupuestoDtm>().Any(p => p.Id == x.idElemento2 &&
                    contexto.Set<EstadoDeUnPresupuestoDtm>().Any(e => e.Id == p.IdEstado && !e.Cancelado))))
                    continue;

                pptDeTareas.Vincular(contexto, enumNegocio.Tarea, tarea.Id, new Dictionary<string, object> { { nameof(ParametrosDeNegocio.EstaEjecutandoUnaAccion), true } });
                new LineaDeUnPptDtm
                {
                    Concepto = tarea.Expresion,
                    TipoDeLinea = Enumerados.enumTipoDeLinea.Alzada,
                    Orden = numero,
                    IdElemento = pptDeTareas.Id,
                    IdIvaR = iva.Id,
                    Clase = enumClaseUnitario.Servicio,
                    IdNaturaleza = naturaleza.Id,
                    IdUnidad = unidad.Id,
                    Cantidad = minutos / 60,
                    Precio = precio,
                    Descuento = 0
                }.Insertar(contexto);
            }

            return pptDeTareas;
        }

        public static TareaDtm CrearTarea(this ExpedienteDtm expediente, ContextoSe contexto, string tipo)
        {
            var tipoTarea = contexto.SeleccionarPorNombre<TipoDeTareaDtm>(tipo);
            var tarea = new TareaDtm
            {
                IdTipo = tipoTarea.Id,
                IdCg = expediente.IdCg,
                Nombre = $"Tarea del expediente {expediente.Referencia}",
                Descripcion = $"{expediente.Descripcion}",
                ClaseDeTarea = tipoTarea.ClaseDeTarea,
                IdSolicitante = expediente.IdSolicitante
            }.Insertar(contexto);

            expediente.Vincular(contexto, tarea);
            return tarea;
        }

        public static void DefinirEtapasBasicas(ContextoSe contexto)
        {
            var estados = enumNegocio.Expediente.Estados(contexto);
            var iniciales = "";
            foreach (EstadoDtm estado in estados)
            {
                if (!estado.Inicial) continue;
                iniciales = $"{(iniciales.IsNullOrEmpty() ? estado.Id.ToString() : $"{iniciales},{estado.Id}")}";
            }
            enumNegocio.Expediente.ResetearParametro(contexto, enumEtapasDeExpedientes.EXP_Etapa_Inicial, iniciales);

            var estadosCancelados = "";
            foreach (EstadoDtm cancelado in estados)
            {
                if (cancelado.Cancelado)
                    estadosCancelados = $"{(estadosCancelados.IsNullOrEmpty() ? cancelado.Id.ToString() : $"{estadosCancelados},{cancelado.Id}")}";
            }
            enumNegocio.Expediente.ResetearParametro(contexto, enumEtapasDeExpedientes.EXP_Etapa_Cancelada, estadosCancelados);

            var estadosTerminados = "";
            foreach (EstadoDtm terminado in estados)
            {
                if (!terminado.Terminado) continue;
                estadosTerminados = $"{(estadosTerminados.IsNullOrEmpty() ? terminado.Id.ToString() : $"{estadosTerminados},{terminado.Id}")}";
            }
            enumNegocio.Expediente.ResetearParametro(contexto, enumEtapasDeExpedientes.EXP_Etapa_Terminada, estadosTerminados);
        }

        public static void HistorialDePresupuestos(this ExpedienteDtm expediente, ContextoSe contexto, List<HistorialDto> sucesos, List<ClausulaDeFiltrado> filtros, int nivel = 1)
        {
            ClausulaDeFiltrado filtro = filtros.First(f => f.Clausula == ltrSucesosFiltros.excluir);
            var valores = filtro.Valor.Split(Simbolos.separadorDeValores);
            if (valores.Contains(ltrSucesosExcluir.presupuestos))
                return;
            var presupuestos = contexto.SeleccionarTodos<PresupuestoDtm>(nameof(PresupuestoDtm.IdExpediente), expediente.Id, parametros: new Dictionary<string, object> { { ltrParametrosNeg.IncluirCancelados, true } });
            for (var i = presupuestos.Count() - 1; i >= 0; i--)
            {
                var ppt = presupuestos[i];
                var suceso = new HistorialDto();
                suceso.Id = sucesos.Count + 1;
                suceso.IdRegistro = ppt.Id;
                suceso.Elemento = ppt.Referencia(contexto); suceso.Clase = $"{ltrSucesosCss.suceso}{Simbolos.separadorCss}{ltrSucesosCss.presupuesto}";
                suceso.Suceso = $"Ppt. asociado: {ppt.Nombre}"; suceso.EstaCancelada = enumNegocio.Presupuesto.Estado(contexto, ppt.IdEstado).Cancelado;
                suceso.EstaTerminada = enumNegocio.Presupuesto.Estado(contexto, ppt.IdEstado).Terminado;
                suceso.Usuario = contexto.SeleccionarPorId<UsuarioDtm>(ppt.IdUsuaCrea).Login;
                suceso.OcurridoEl = ppt.FechaCreacion;
                suceso.Detalle = ppt.Descripcion;
                suceso.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor;
                suceso.Accion = "Abrir";
                suceso.AccionJs = ltrAccionesDeSucesos.AbrirPpt;

                suceso.Negocio = enumNegocio.Presupuesto.Singular();
                suceso.enumNegocio = enumNegocio.Presupuesto;
                suceso.IdNegocio = enumNegocio.Presupuesto.IdNegocio();
                suceso.Nivel = nivel;
                suceso.IdElemento = expediente.Id;

                sucesos.Add(suceso);

                ppt.HistorialDeHitos(contexto, sucesos, filtros, nivel + 1);
                ppt.HistorialDeObservaciones(contexto, sucesos, filtros, nivel + 1);
                ppt.HistorialDeArchivadores(contexto, sucesos, filtros);
                ppt.HistorialDeArchivos(contexto, sucesos, filtros);
            }
        }

        public static Tiempos Tiempos(this ExpedienteDtm expediente, ContextoSe contexto)
        {
            var tareas = expediente.TareasAsociadas(contexto);
            return new Tiempos(tareas, contexto);
        }

        public static List<TareaDtm> TareasAsociadas(this ExpedienteDtm expediente, ContextoSe contexto)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Exp_Tareas);
            if (!cache.ContainsKey($"{expediente.Id}"))
            {
                var tareas = expediente.Tareas(contexto).ToList();
                foreach (var tarea in tareas)
                {
                    tarea.Planificacion = tarea.Planificacion(contexto, errorSiNoHay: false);
                }
                cache[$"{expediente.Id}"] = tareas;
            }
            return (List<TareaDtm>)cache[$"{expediente.Id}"];
        }

        public static void IncluirLasFacturasRecDeSusContratos(this ExpedienteDtm expediente, ContextoSe contexto, List<FacturaRecDto> facturas)
        {
            var contratos = contexto.SeleccionarTodos<ContratoDtm>(nameof(ContratoDtm.IdExpediente), expediente.Id);
            foreach (var contrato in contratos)
            {
                var imputadas = contexto.SeleccionarTodos<FacturaRecDtm>(nameof(FacturaRecDtm.IdContrato), contrato.Id);
                foreach (var imputada in imputadas)
                {
                    if (facturas.Any(f => f.Id == imputada.Id)) continue;
                    facturas.Append(imputada.MapearDto(contexto, enumNegocio.FacturaRecibida));
                }
            }
        }

        public static void IncluirLasFacturasEmtDeSusPresupuestos(this ExpedienteDtm expediente, ContextoSe contexto, List<FacturaEmtDto> facturas)
        {
            var presupuestos = contexto.SeleccionarTodos<PresupuestoDtm>(nameof(PresupuestoDtm.IdExpediente), expediente.Id);
            foreach (var presupuesto in presupuestos)
            {
                var imputadas = contexto.SeleccionarTodos<FacturaEmtDtm>(nameof(FacturaEmtDtm.IdPresupuesto), presupuesto.Id);
                foreach (var imputada in imputadas)
                {
                    if (facturas.Any(f => f.Id == imputada.Id)) continue;
                    facturas.Append(imputada.MapearDto(contexto, enumNegocio.FacturaEmitida));
                }
            }
        }
        public static void IncluirLasFacturasEmtDeSusContratos(this ExpedienteDtm expediente, ContextoSe contexto, List<FacturaEmtDto> facturas)
        {
            var contratos = contexto.SeleccionarTodos<ContratoDtm>(nameof(ContratoDtm.IdExpediente), expediente.Id);
            foreach (var contrato in contratos)
            {
                var facturado = contexto.SeleccionarTodos<FacturaEmtDtm>(nameof(FacturaEmtDtm.IdContrato), contrato.Id);
                foreach (var emitida in facturado)
                {
                    if (facturas.Any(f => f.Id == emitida.Id)) continue;
                    facturas.Append(emitida.MapearDto(contexto, enumNegocio.FacturaEmitida));
                }
            }
        }

        public static bool EsUnaActividad(this ExpedienteDtm expediente)
        => 
        expediente.IdTipo == VariablesDeExpedientes.IdDelTipoParaActividades(errorSiNoEstaDefinido: false);
    }
}
