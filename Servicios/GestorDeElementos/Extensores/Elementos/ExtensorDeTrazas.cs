using Gestor.Errores;
using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.Gastos;
using ServicioDeDatos.Guarderias;
using ServicioDeDatos.Juridico;
using ServicioDeDatos.Logistica;
using ServicioDeDatos.Presupuesto;
using ServicioDeDatos.RegistroEs;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Tarea;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.Ventas;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilidades;

namespace GestorDeElementos.Extensores
{
    public static class ExtensorDeTrazasDtm
    {
        public static T InsertarTraza<T>(this T traza, ContextoSe contexto, Dictionary<string, object> parametros = null)
        where T : TrazaDtm
        {
            var gestor = GestorDeTrazas.Gestor(contexto, traza.Negocio);
            return (T)gestor.PersistirRegistro(traza, new ParametrosDeNegocio(enumTipoOperacion.Insertar) { Parametros = parametros });
        }

        public static ITraza InsertarTraza(this ITraza traza, ContextoSe contexto, Dictionary<string, object> parametros = null)
        {
            if (parametros == null) parametros = new Dictionary<string, object>();
            var gestor = GestorDeTrazas.Gestor(contexto, traza.Negocio);
            return gestor.PersistirRegistro(((TrazaDtm)traza), new ParametrosDeNegocio(enumTipoOperacion.Insertar) { Parametros = parametros });
        }


        public static TrazaDtm CrearTraza<T>(this T elemento, ContextoSe contexto,string nombre, string descripción, Dictionary<string,object> parametros = null)
        where T: IUsaTraza
        {
            var traza = NuevaTraza(elemento.GetType().NegocioDeUnDtm());
            traza.IdElemento =((IRegistro) elemento).Id;
            traza.Nombre = nombre;
            traza.Descripcion = descripción.Left(IDominio.Longitud(IDominio.VARCHAR_2000) - 1);
            var gestor = GestorDeTrazas.Gestor(contexto, traza.Negocio);

            var idUsuario = contexto.DatosDeConexion.IdUsuario;
            contexto.AsignarUsuario(contexto.Administrador());
            try
            {
                return gestor.PersistirRegistro(traza, new ParametrosDeNegocio(enumTipoOperacion.Insertar) { Parametros = parametros });
            }
            finally
            {
                contexto.AsignarUsuario(contexto.SeleccionarPorId<UsuarioDtm>(idUsuario));
            }
        }

        public static void EliminarTraza<T>(this T traza, ContextoSe contexto)
        where T : TrazaDtm
        {
            var gestor = GestorDeTrazas.Gestor(contexto, traza.Negocio);
            var idUsuario = contexto.DatosDeConexion.IdUsuario;
            var parametros = new Dictionary<string, object> { { ltrParametrosNeg.ValidarPermisosDePersistencia, false } };
            contexto.AsignarUsuario(contexto.Administrador());
            try
            {
                gestor.PersistirRegistro(traza, new ParametrosDeNegocio(enumTipoOperacion.Eliminar) { Parametros = parametros });
            }
            finally
            {
                contexto.AsignarUsuario(contexto.SeleccionarPorId<UsuarioDtm>(idUsuario));
            }
        }

        private static TrazaDtm NuevaTraza(enumNegocio negocio)
        {
            if (!negocio.UsaTrazas())
                GestorDeErrores.Emitir($"El negocio {negocio.Singular()} no implementa el uso de trazas");

            var metadatos = negocio.ObtenerMetadatos();

            if (metadatos?.TrazaDtm == null)
            {
                GestorDeErrores.Emitir($"No ha definido en los metadatos del negocio '{negocio.Singular()}' el tipo de objeto '{nameof(Metadatos.TrazaDtm)}'");
            }

            return (TrazaDtm)Activator.CreateInstance(metadatos.TrazaDtm);
        }

        public static void QuitarTraza<T>(this T elemento, ContextoSe contexto, string nombre)
        where T : IUsaTraza
        {
            var negocio = NegociosDeSe.NegocioDeUnDtm(typeof(T));
            var gestor = GestorDeTrazas.Gestor(contexto, negocio );
            var trazas = negocio.Trazas(contexto).Where(t=> t.IdElemento == ((IRegistro)elemento).Id && t.Nombre == nombre).ToList();
            foreach (var traza in trazas)
            {
                gestor.PersistirRegistro(traza, new ParametrosDeNegocio(enumTipoOperacion.Eliminar) { ValidarPermisosDePersistencia = false});
            }
        }

        public static IQueryable<TrazaDtm> Trazas(this enumNegocio negocio, ContextoSe contexto)
        {
            switch (negocio)
            {
                case enumNegocio.Registro:
                    return contexto.Set<TrazasDeUnRegistroEsDtm>().Cast<TrazaDtm>();
                case enumNegocio.Tarea:
                    return contexto.Set<TrazasDeUnaTareaDtm>().Cast<TrazaDtm>();
                case enumNegocio.Expediente:
                    return contexto.Set<TrazasDeUnExpedienteDtm>().Cast<TrazaDtm>();
                case enumNegocio.Pleito:
                    return contexto.Set<TrazasDeUnPleitoDtm>().Cast<TrazaDtm>();
                case enumNegocio.Presupuesto:
                    return contexto.Set<TrazasDeUnPresupuestoDtm>().Cast<TrazaDtm>();
                case enumNegocio.Contrato:
                    return contexto.Set<TrazasDeUnContratoDtm>().Cast<TrazaDtm>();
                case enumNegocio.Archivador:
                    return contexto.Set<TrazasDeUnArchivadorDtm>().Cast<TrazaDtm>();
                case enumNegocio.CircuitoDoc:
                    return contexto.Set<TrazasDeUnCircuitoDocDtm>().Cast<TrazaDtm>();
                case enumNegocio.FacturaEmitida:
                    return contexto.Set<TrazasDeUnaFacturaEmtDtm>().Cast<TrazaDtm>();
                case enumNegocio.ParteDeTrabajo:
                    return contexto.Set<TrazasDeUnParteTrDtm>().Cast<TrazaDtm>();
                case enumNegocio.PlanificacionDeVenta:
                    return contexto.Set<TrazasDeUnaPlanificacionDeVentaDtm>().Cast<TrazaDtm>();
                case enumNegocio.RemesaFae:
                    return contexto.Set<TrazasDeUnaRemesaFaeDtm>().Cast<TrazaDtm>();
                case enumNegocio.FacturaRecibida:
                    return contexto.Set<TrazasDeUnaFacturaRecDtm>().Cast<TrazaDtm>();
                case enumNegocio.Pedido:
                    return contexto.Set<TrazasDeUnPedidoDtm>().Cast<TrazaDtm>();
                case enumNegocio.Preasiento:
                    return contexto.Set<TrazasDeUnPreasientoDtm>().Cast<TrazaDtm>();
                case enumNegocio.Pago:
                    return contexto.Set<TrazasDeUnPagoDtm>().Cast<TrazaDtm>();
                case enumNegocio.RemesaPag:
                    return contexto.Set<TrazasDeUnaRemesaPagDtm>().Cast<TrazaDtm>();
                case enumNegocio.Abogado:
                    return contexto.Set<TrazasDeUnAbogadoDtm>().Cast<TrazaDtm>();
                case enumNegocio.Cliente:
                    return contexto.Set<TrazasDeUnClienteDtm>().Cast<TrazaDtm>();
                case enumNegocio.Interlocutor:
                    return contexto.Set<TrazasDeUnInterlocutorDtm>().Cast<TrazaDtm>();
                case enumNegocio.Persona:
                    return contexto.Set<TrazasDeUnaPersonaDtm>().Cast<TrazaDtm>();
                case enumNegocio.Procurador:
                    return contexto.Set<TrazasDeUnProcuradorDtm>().Cast<TrazaDtm>();
                case enumNegocio.Proveedor:
                    return contexto.Set<TrazasDeUnProveedorDtm>().Cast<TrazaDtm>();
                case enumNegocio.Sociedad:
                    return contexto.Set<TrazasDeUnaSociedadDtm>().Cast<TrazaDtm>();
                case enumNegocio.Trabajador:
                    return contexto.Set<TrazasDeUnTrabajadorDtm>().Cast<TrazaDtm>();
                case enumNegocio.Infante:
                    return contexto.Set<TrazasDeUnInfanteDtm>().Cast<TrazaDtm>();
                case enumNegocio.CursoDeGuarderia:
                    return contexto.Set<TrazasDeUnCursoDeGuarderiaDtm>().Cast<TrazaDtm>();
            }

            throw new Exception($"Se debe indicar como obtener los tipos del negocio: {negocio}");
        }

    }
}
