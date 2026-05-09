using Gestor.Errores;
using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Juridico;
using ServicioDeDatos.MaestrosTecnico;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.Ventas;
using System;
using System.Collections.Generic;
using System.Threading;
using Utilidades;
using static Gestor.Errores.GestorDeErrores;
using static ServicioDeDatos.Elemento.Enumerados;

namespace GestorDeElementos.Extensores
{
    public static class ExtensorDePlanificador
    {
        public static void ValidarQueLaLineaDelPlanificadorEsModificable(this LineaDeUnPlfVentaDtm lineaPlf, ContextoSe contexto)
        {
            var planificador = lineaPlf.DetalleDe<PlanificadorDeVentaDtm>(contexto);
            if (planificador.Generado)
                Emitir($"No se pueden crear, modificar o borrar líneas de planificador ya generado");

            if (!ModoDeAcceso.HayPermisosDe(planificador.ModoDeAccesoAlPlanificador(contexto), enumModoDeAccesoDeDatos.Gestor))
                Emitir($"No se pueden crear, modificar o borrar líneas de un planificador sobre un contrato no editable");
        }

        public static void ValidarQueElPlanificadorEsModificable(this PlanificadorDeVentaDtm plfVenta, ContextoSe contexto)
        {
            if (!ModoDeAcceso.HayPermisosDe(plfVenta.ModoDeAccesoAlPlanificador(contexto), enumModoDeAccesoDeDatos.Gestor))
                Emitir($"No se pueden crear, modificar o borrar planificadores sobre un contrato no editable");
        }

        public static decimal Total(this PlanificadorDeVentaDtm elemento, ContextoSe contexto, bool conIva)
        {
            var cacheSinIva = ServicioDeCaches.Obtener(CacheDe.Pldor_TotalSinIva);
            var cacheConIva = ServicioDeCaches.Obtener(CacheDe.Pldor_TotalConIva);
            if (!cacheSinIva.ContainsKey(elemento.Id.ToString()))
            {
                decimal totalSinIva = 0;
                decimal totalConIva = 0;
                var lineas = elemento.Detalles<LineaDeUnPlfVentaDtm>(contexto);
                foreach (var linea in lineas)
                {
                    totalSinIva = totalSinIva + linea.ImporteConDto;
                    totalConIva = totalConIva + linea.ImporteDeLinea;
                }
                cacheSinIva[$"{elemento.Id}"] = totalSinIva;
                cacheConIva[$"{elemento.Id}"] = totalConIva;
            }
            return conIva ? (decimal)cacheConIva[$"{elemento.Id}"] : (decimal)cacheSinIva[$"{elemento.Id}"];
        }

        public static enumModoDeAccesoDeDatos ModoDeAccesoAlPlanificador(this PlanificadorDeVentaDtm plfVenta, ContextoSe contexto)
        {
            if (plfVenta.Generado) return enumModoDeAccesoDeDatos.Consultor;

            var contrato = contexto.SeleccionarPorId<ContratoDtm>(plfVenta.IdContrato, aplicarJoin: true);

            var modoDeAcceso = ApiDePermisos.LeerModoDeAcceso(contexto, enumNegocio.Contrato, contrato.Id);
            if (ModoDeAcceso.HayPermisosDe(modoDeAcceso, enumModoDeAccesoDeDatos.Gestor))
            {
                if (!contrato.EstaEnElaboracion() && !contrato.EstaPdtDeProroga() && !contrato.EstaVigente())
                    modoDeAcceso = enumModoDeAccesoDeDatos.Consultor;
            }
            return modoDeAcceso;
        }

        public static bool GenerarPlanificaciones(this PlanificadorDeVentaDtm planificador, ContextoSe contexto, DateTime fechaDeComienzo, DateTime fechaDeFin)
        {
            ValidarDatos(planificador, contexto);
            ValidarVigenciaDelLote(planificador, contexto, fechaDeFin);

            planificador.ProcesarPlanificador(contexto, planificador.Contrato(contexto).Cliente(contexto, errorSiNoHay: true), fechaDeComienzo, fechaDeFin);
            return true;
        }

        public static PlanificadorDeVentaDtm GenerarPlanificaciones(this PlanificadorDeVentaDtm planificador, ContextoSe contexto)
        {
            ValidarDatos(planificador, contexto);
            var fechas = planificador.Contrato(contexto).FechasDelContrato(contexto);
            //var fechaDeComienzo = fechas.Inicio < planificador.Inicio.Date ? planificador.Inicio.Date : fechas.Inicio;
            var fechaDeFin = fechas.Fin == null || fechas.Fin > planificador.Hasta.Date ? planificador.Hasta.Date : (DateTime)fechas.Fin;
            ValidarVigenciaDelLote(planificador, contexto, fechaDeFin);

            return planificador.ProcesarPlanificador(contexto, planificador.Contrato(contexto).Cliente(contexto, errorSiNoHay: true), fechas.Inicio, fechaDeFin);
        }

        private static PlanificadorDeVentaDtm ProcesarPlanificador(this PlanificadorDeVentaDtm planificador, ContextoSe contexto, ClienteDtm cliente, DateTime fechaDePlanificacion, DateTime ultimaPlanificacion)
        {
            var tran = contexto.IniciarTransaccion();
            var idSemaforo = SemaforoDeProcesoSql.PonerSemaforo(contexto, enumNegocio.PlanificadorDeVenta.IdNegocio(), planificador.Id, enumOpercionesDeSemaforo.GPFV, planificador.Contrato(contexto).Referencia).Id;
            try
            {
                int numero = 1;
                while (fechaDePlanificacion < ultimaPlanificacion)
                {
                    //se crea una planificación de venta con fecha de ejecución la fecha de generación 
                    planificador.CrearPlanificacion(contexto, cliente, fechaDePlanificacion, numero);
                    numero++;

                    //sumamos a la fecha de generación los días, semanas, meses o años indicados
                    switch (planificador.Periodicidad)
                    {
                        case enumPeriodicidad.Diaria:
                            fechaDePlanificacion = fechaDePlanificacion.AddDays(planificador.RepetirCada);
                            break;
                        case enumPeriodicidad.Semanal:
                            fechaDePlanificacion = fechaDePlanificacion.AddDays(planificador.RepetirCada * 7);
                            break;
                        case enumPeriodicidad.Mensual:
                            fechaDePlanificacion = fechaDePlanificacion.AddMonths(planificador.RepetirCada);
                            break;
                        case enumPeriodicidad.Anual:
                            fechaDePlanificacion = fechaDePlanificacion.AddYears(planificador.RepetirCada);
                            break; ;
                    }

                }
                planificador.Generado = true;
                planificador.Contrato = null;
                planificador = planificador.Modificar(contexto);
                contexto.Commit(tran);
                return planificador;
            }
            catch
            {
                contexto.Rollback(tran);
                throw;
            }
            finally
            {
                SemaforoDeProcesoSql.QuitarSemaforo(contexto, idSemaforo);
            }
        }

        public static void ValidarQueHayPlanificaciones(ContextoSe contexto, List<int> list)
        {
            throw new NotImplementedException();
        }

        public static PlanificadorDeVentaDtm Copiar(this PlanificadorDeVentaDtm planificador, ContextoSe contexto, ContratoDtm contratoDestino, DateTime inicio, DateTime hasta)
        {
            var idOrigen = planificador.Id;
            var fechas = contratoDestino.FechasDelContrato(contexto);
            
            if (fechas.Inicio > inicio) Emitir($"La fecha de inicio del contrato '{fechas.Inicio.ToString("dd/MM/yyyy")}' es posterior al inicio de la planificación '{inicio.ToString("dd/MM/yyyy")}'");
            if (fechas.Fin != null && fechas.Fin < hasta) Emitir($"La fecha de fin del contrato '{fechas.Fin.Fecha().ToString("dd/MM/yyyy")}' es anterior al final de la planificación '{hasta.ToString("dd/MM/yyyy")}'");
            if (fechas.Fin == null && fechas.Inicio.AddYears(1) < hasta) Emitir($"Si no hay fecha de fin de contrato, no se puede planificar más de un año");

            var planificadores = contratoDestino.PlanificadoresDeVenta(contexto);
            var nombreNuevo = planificador.Nombre.ToLower();    
            while(planificadores.Exists(x => x.Nombre.ToLower() == nombreNuevo))
            {
                nombreNuevo = nombreNuevo.IncrementarSufijo();
            }            
            planificador.Id = 0;
            planificador.IdContrato = contratoDestino.Id;
            planificador.Nombre = nombreNuevo;
            planificador.Generado = false;
            planificador.Inicio = inicio;
            planificador.Hasta = hasta;
            var nuevo = planificador.Insertar(contexto);
            planificador = contexto.SeleccionarPorId<PlanificadorDeVentaDtm>(idOrigen, usarLaCache: false);
            nuevo.CopiarLineas(contexto, planificador);
            return nuevo;
        }
        
        public static ContratoDtm Contrato(this PlanificadorDeVentaDtm planificador, ContextoSe contexto)
        =>
        planificador.Contrato ??= contexto.SeleccionarPorId<ContratoDtm>(planificador.IdContrato, aplicarJoin: true);

        private static void ValidarDatos(PlanificadorDeVentaDtm planificador, ContextoSe contexto)
        {
            //validamos que esté vigente            
            if (!planificador.Contrato(contexto).EstaVigente())
                Emitir($"No se pueden generar planificaciones para {planificador.Contrato(contexto).LaClaseDelContrato} '{planificador.Contrato(contexto).Referencia}' ya que no está vigente");

            if (planificador.Generado)
                Emitir($"las planificaciones del planificador '{planificador.Referencia(contexto)}' ya se han generado");

            //indicamos que la fecha de generación es la fecha inicial
            //var fechas = planificador.Contrato.FechasDelContrato(contexto);
            //if (fechas.Inicio > DateTime.Today)
            //    Emitir($"Aunque {planificador.Contrato(contexto).LaClaseDelContrato} '{planificador.Contrato(contexto).Referencia}' está vigente, su fecha de inicio es posterión {fechas.Inicio.Date} a hoy");

            if (planificador.Contrato(contexto).Cliente(contexto) == null)
                Emitir($"Se debe indicar el cliente {planificador.Contrato(contexto).DeLaClaseDeContrato} '{planificador.Contrato(contexto).Referencia}' ");
        }


        private static void ValidarVigenciaDelLote(PlanificadorDeVentaDtm planificador, ContextoSe contexto, DateTime ultimaPlanificacion)
        {
            if (planificador.IdLote.Entero() > 0 && !EstaVigenteEllLote(planificador, contexto, ultimaPlanificacion))
            {
                var lote = contexto.SeleccionarPorId<LoteDeUnContratoDtm>(planificador.IdLote.Entero());
                Emitir($"El lote en el que se basa el planificador '{planificador.Nombre}' tiene una fecha de vigencia '{lote.VigenteHasta.Fecha().ToShortDateString()}' anterior a la de la última planificación '{ultimaPlanificacion.ToShortDateString()}', actualice los datos antes de generar las planificaciones");
            }

        }

        private static bool EstaVigenteEllLote(PlanificadorDeVentaDtm planificador, ContextoSe contexto, DateTime ultimaPlanificacion)
        {
            var lote = contexto.SeleccionarPorId<LoteDeUnContratoDtm>(planificador.IdLote.Entero());
            return !(lote.VigenteHasta != null && ultimaPlanificacion > lote.VigenteHasta);
        }


        private static void CopiarLineas(this PlanificadorDeVentaDtm destino, ContextoSe contexto, PlanificadorDeVentaDtm origen)
        {
            var lineasDePlanificador = origen.Detalles<LineaDeUnPlfVentaDtm>(contexto);
            foreach (var linea in lineasDePlanificador)
            {
                new LineaDeUnPlfVentaDtm
                {
                    IdElemento = destino.Id,
                    Orden = linea.Orden,
                    TipoDeLinea = linea.TipoDeLinea,
                    IdUnitario = linea.IdUnitario,
                    Concepto = linea.Concepto,
                    Cantidad = linea.Cantidad,
                    Coste = linea.Coste,
                    Venta = linea.Venta,
                    Anotacion = linea.Anotacion,
                    Descuento = linea.Descuento,
                    IdIvaR = linea.IdIvaR,
                    Iva = linea.Iva,
                    Clase = linea.Clase,
                    IdUnidad = linea.IdUnidad,
                    IdNaturaleza = linea.IdNaturaleza
                }
                .Insertar(contexto);
            }
        }

        public static UnitarioDtm Unitario(this LineaDeUnPlfVentaDtm linea, ContextoSe contexto, bool errorSiNoHay = true, bool aplicarJoin= false)
        {
            if (linea.IdUnitario.Entero() == 0)
            {
                if (errorSiNoHay) Emitir($"La línea '{linea.Orden}' del planificador '{linea.DetalleDe<PlanificadorDeVentaDtm>(contexto).Referencia(contexto)}' no tiene asociado ningún unitario");
                return null;
            }

            return linea.Unitario == null ? contexto.SeleccionarPorId<UnitarioDtm>((int)linea.IdUnitario, aplicarJoin) : linea.Unitario;
        }


        public static decimal PorcentageDeIva(this LineaDeUnPlfVentaDtm linea, ContextoSe contexto, bool errorSiNoHay = true)
        {
            if (linea.IdIvaR.Entero() == 0)
            {
                if (errorSiNoHay) Emitir($"La línea '{linea.Orden}' del planificador '{linea.DetalleDe<PlanificadorDeVentaDtm>(contexto).Referencia(contexto)}' no tiene iva indicado");
                return 0;
            }

            return contexto.SeleccionarPorId<IvaRepercutidoDtm>(linea.IdIvaR.Entero()).Porcentaje;
        }
    }
}
