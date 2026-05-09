using ServicioDeDatos.Elemento;
using ServicioDeDatos;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilidades;
using ServicioDeDatos.Juridico;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.Presupuesto;
using ServicioDeDatos.RegistroEs;
using ServicioDeDatos.Tarea;
using ServicioDeDatos.Ventas;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Gastos;
using ServicioDeDatos.Logistica;
using ServicioDeDatos.Contabilidad;

namespace GestorDeElementos.Extensores
{

    public static class ExtensorDeAccionesDeTrn
    {

        public static enumNegocio Negocio(this Type tipo)
        {
            if (typeof(AccionesDeUnPleitoDtm) == tipo) return enumNegocio.Pleito;
            if (typeof(AccionesDeUnaTareaDtm) == tipo) return enumNegocio.Tarea;
            if (typeof(AccionesDeUnRegistroEsDtm) == tipo) return enumNegocio.Registro;
            if (typeof(AccionesDeUnExpedienteDtm) == tipo) return enumNegocio.Expediente;
            if (typeof(AccionesDeUnPresupuestoDtm) == tipo) return enumNegocio.Presupuesto;
            if (typeof(AccionesDeUnContratoDtm) == tipo) return enumNegocio.Contrato;
            if (typeof(AccionesDeUnParteTrDtm) == tipo) return enumNegocio.ParteDeTrabajo;
            if (typeof(AccionesDeUnaFacturaEmtDtm) == tipo) return enumNegocio.FacturaEmitida;
            if (typeof(AccionesDeUnaPlanificacionDeVentaDtm) == tipo) return enumNegocio.PlanificacionDeVenta;
            if (typeof(AccionesDeUnaRemesaFaeDtm) == tipo) return enumNegocio.RemesaFae;
            if (typeof(AccionesDeUnCircuitoDocDtm) == tipo) return enumNegocio.CircuitoDoc;
            if (typeof(AccionesDeUnPagoDtm) == tipo) return enumNegocio.Pago;
            if (typeof(AccionesDeUnaRemesaPagDtm) == tipo) return enumNegocio.RemesaPag;
            if (typeof(AccionesDeUnaFacturaRecDtm) == tipo) return enumNegocio.FacturaRecibida;
            if (typeof(AccionesDeUnPedidoDtm) == tipo) return enumNegocio.Pedido;
            if (typeof(AccionesDeUnPreasientoDtm) == tipo) return enumNegocio.Preasiento;

            throw new Exception($"No se ha definido cual es el negocio para la clase accionTrn {tipo.Name}");
        }

        public static T PersistirAccionesDeTrn<T>(this T accionDeTrn, ContextoSe contexto, bool errorSiYaExiste = false, bool aplicarJoin = false, Dictionary<string, object> parametros = null)
        where T : AccionesDeTrnDtm
        {
            if (accionDeTrn.Id > 0)
                return accionDeTrn.Modificar(contexto);

            var ak = new Dictionary<string, object>();
            ak.Add(nameof(accionDeTrn.IdTransicion), accionDeTrn.IdTransicion);
            ak.Add(nameof(accionDeTrn.IdAccion), accionDeTrn.IdAccion);
            ak.Add(nameof(accionDeTrn.Momento), accionDeTrn.Momento);

            var leido = contexto.SeleccionarAccionesPorAk<T>(ak, errorSiYaExiste, true, false);
            if (leido == null)
                return accionDeTrn.Insertar(contexto, parametros, aplicarJoin);

            accionDeTrn.Id = leido.Id;
            return accionDeTrn.Modificar(contexto, parametros, aplicarJoin);
        }

        public static AccionesDeTrnDtm SeleccionarAccionesPorAk<T>(this ContextoSe contexto, Dictionary<string, object> filtrosPorAk, bool errorSiNoHay = true, bool errorSiMasDeuno = true, bool aplicarJoin = false)
        where T : AccionesDeTrnDtm
        {
            var filtros = filtrosPorAk.ToFiltros();

            var negocio = Negocio(typeof(T));
            var seleccionados = negocio.SeleccionarPorFiltro<AccionesDeTrnDtm>(contexto, filtros, aplicarJoin);
            var mensaje = $"del negocio {negocio} para los criterios '{filtrosPorAk.Keys.ToList().ToString(",")}'";
            return seleccionados.DevolverSeleccionado(mensaje, errorSiNoHay, errorSiMasDeuno);
        }

        public static IQueryable<AccionesDeTrnDtm> Acciones(this enumNegocio negocio, ContextoSe contexto)
        {
            switch (negocio)
            {
                case enumNegocio.Registro:
                    return contexto.Set<AccionesDeUnRegistroEsDtm>().Cast<AccionesDeTrnDtm>();
                case enumNegocio.Tarea:
                    return contexto.Set<AccionesDeUnaTareaDtm>().Cast<AccionesDeTrnDtm>();
                case enumNegocio.Expediente:
                    return contexto.Set<AccionesDeUnExpedienteDtm>().Cast<AccionesDeTrnDtm>();
                case enumNegocio.Pleito:
                    return contexto.Set<AccionesDeUnPleitoDtm>().Cast<AccionesDeTrnDtm>();
                case enumNegocio.Contrato:
                    return contexto.Set<AccionesDeUnContratoDtm>().Cast<AccionesDeTrnDtm>();
                case enumNegocio.Presupuesto:
                    return contexto.Set<AccionesDeUnPresupuestoDtm>().Cast<AccionesDeTrnDtm>();
            }

            throw new Exception($"Se debe indicar como obtener el dbSet de las acciones del negocio: {negocio}");
        }
    }
}
