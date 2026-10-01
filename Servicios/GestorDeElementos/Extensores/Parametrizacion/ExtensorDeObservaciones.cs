using ServicioDeDatos.Elemento;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.Gastos;
using ServicioDeDatos.Juridico;
using ServicioDeDatos.Presupuesto;
using ServicioDeDatos.RegistroEs;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Tarea;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.Ventas;
using ServicioDeDatos;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilidades;
using Gestor.Errores;
using ServicioDeDatos.Guarderias;
using ServicioDeDatos.Logistica;
using ServicioDeDatos.Contabilidad;

namespace GestorDeElementos.Extensores
{
    public static class ExtensorDeObservaciones
    {
        public static ObservacionDtm CrearObservacion<T>(this T elemento, ContextoSe contexto, string nombre, string descripción, Dictionary<string, object> parametros = null)
        where T : IUsaObservacion
        {
            var negocio = elemento.GetType().NegocioDeUnDtm();
            var observacion = Nueva(negocio);
            observacion.IdElemento = ((IRegistro)elemento).Id;
            observacion.Nombre = nombre;
            observacion.Descripcion = descripción;
            
            return (ObservacionDtm) observacion.InsertarObservacion(contexto,  parametros);
        }

        public static IObservacion InsertarObservacion(this IObservacion observacion, ContextoSe contexto, Dictionary<string, object> parametros = null)
        {
            if (parametros == null) parametros = new Dictionary<string, object>();

            var gestor = GestorDeObservaciones.Gestor(contexto, observacion.Negocio);

            if (parametros.LeerValor(ltrDeObservaciones.CreadaPorAdminSe, false) && !parametros.ContieneClave(ltrDeObservaciones.PermitirSiTerminado))
                parametros.Add(ltrDeObservaciones.PermitirSiTerminado, true);

            return gestor.PersistirRegistro((ObservacionDtm)observacion, new ParametrosDeNegocio(enumTipoOperacion.Insertar) { Parametros = parametros });
        }

        public static IObservacion ModificarObservacion(this IObservacion observacion, ContextoSe contexto, Dictionary<string, object> parametros = null)
        {
            if (parametros == null) parametros = new Dictionary<string, object>();

            var gestor = GestorDeObservaciones.Gestor(contexto, observacion.Negocio);

            if (parametros.LeerValor(ltrDeObservaciones.CreadaPorAdminSe, false) && !parametros.ContieneClave(ltrDeObservaciones.PermitirSiTerminado))
                parametros.Add(ltrDeObservaciones.PermitirSiTerminado, true);

            return gestor.PersistirRegistro((ObservacionDtm)observacion, new ParametrosDeNegocio(enumTipoOperacion.Modificar) { Parametros = parametros });
        }

        public static IObservacion EliminarObservacion(this IObservacion observacion, ContextoSe contexto, Dictionary<string, object> parametros = null)
        {
            if (parametros == null) parametros = new Dictionary<string, object>();

            var gestor = GestorDeObservaciones.Gestor(contexto, observacion.Negocio);

            return gestor.PersistirRegistro((ObservacionDtm)observacion, new ParametrosDeNegocio(enumTipoOperacion.Eliminar) { Parametros = parametros });
        }


        /// <summary>
        /// Notifica la observación al responsable del elemento, si no lo hay al último modificador y si no al creador,
        /// siempre que no sea el usuario conectado. Si se ha añadido un archivo se incluye un enlace de descarga.
        /// </summary>
        public static void EnviarMensajeDeObservacion(this ContextoSe contexto, enumNegocio negocio, ObservacionDtm observacion, ParametrosDeNegocio parametros)
        {
            try
            {
                var elemento = (ElementoDtm)negocio.LeerRegistro(contexto, observacion.IdElemento);
                var responsable = elemento.Responsable(contexto);
                var receptor = responsable != null && responsable.Id != contexto.DatosDeConexion.IdUsuario ? responsable : null;

                if (receptor is null && elemento.Modificador(contexto) is not null && elemento.Modificador(contexto).Id != contexto.DatosDeConexion.IdUsuario)
                    receptor = elemento.Modificador(contexto);

                if (receptor is null && elemento.Creador(contexto).Id != contexto.DatosDeConexion.IdUsuario)
                    receptor = elemento.Creador(contexto);

                if (receptor is null)
                    return;

                string refHtml = elemento.CrearHref(contexto);
                var asunto = parametros.Insertando
                ? $"Registro de observación en {negocio.Singular()}. {observacion.Nombre}"
                : parametros.Eliminando
                ? $"Eliminación de observación en {negocio.Singular()}. {observacion.Nombre}"
                : $"Modificación de observación en {negocio.Singular()}. {observacion.Nombre}";

                var cuerpo = $"{negocio.Singular()}: {elemento.Expresion}{Simbolos.br}" +
                             observacion.Descripcion + Simbolos.br +
                             "Enlace: " + refHtml;

                var idArchivo = parametros.Parametros.LeerValor(nameof(IUsaArchivo.IdArchivo), 0);
                var archivo = idArchivo > 0 ? contexto.SeleccionarPorId<ArchivoDtm>(idArchivo, errorSiNoHay: false) : null;
                if (archivo is not null)
                    cuerpo += Simbolos.br + "Archivo añadido: " + archivo.HrefDeDescargaConGuid(contexto);

                contexto.EnviarCorreoPorAdministrador(CacheDeVariable.Cfg_ServidorDeCorreo, new List<string> { receptor.eMail }, asunto, cuerpo);
            }
            catch (Exception e)
            {
                contexto.Traza?.AnotarExcepcion(e);
            }
        }

        private static ObservacionDtm Nueva(enumNegocio negocio)
        {
            if (!negocio.UsaObservaciones())
                GestorDeErrores.Emitir($"El negocio {negocio.Singular()} no implementa el uso de observaciones");

            var metadatos = negocio.ObtenerMetadatos();

            if (metadatos?.ObservacionesDtm == null)
            {
                GestorDeErrores.Emitir($"No ha definido en los metadatos del negocio '{negocio.Singular()}' el tipo de objeto '{nameof(Metadatos.ObservacionesDtm)}'");
            }

            return (ObservacionDtm)Activator.CreateInstance(metadatos.ObservacionesDtm);
        }

        public static enumNegocio Negocio(Type tipo)
        {
            if (!ApiDeEnsamblados.HeredaDe(tipo, typeof(ObservacionDtm)))
                GestorDeErrores.Emitir($"El clase {tipo.Name} no hereda de {nameof(ObservacionDtm)}");

            var tablaDeObservacion = ApiDeRegistroDtm.NombreDeTabla(tipo);
            var tablaDeNegocio = tablaDeObservacion.Replace("_" + nameof(Sufijo.OBSERVACION), "");

            return ApiDeEnsamblados.ToEnumerado<enumNegocio>(tablaDeNegocio);
        }

        public static IQueryable<ObservacionDtm> Observaciones(this enumNegocio negocio, ContextoSe contexto)
        {
            switch (negocio)
            {
                case enumNegocio.Registro:
                    return contexto.Set<ObservacionesDeUnRegistroEsDtm>().Cast<ObservacionDtm>();
                case enumNegocio.Tarea:
                    return contexto.Set<ObservacionesDeUnaTareaDtm>().Cast<ObservacionDtm>();
                case enumNegocio.Expediente:
                    return contexto.Set<ObservacionesDeUnExpedienteDtm>().Cast<ObservacionDtm>();
                case enumNegocio.Pleito:
                    return contexto.Set<ObservacionesDeUnPleitoDtm>().Cast<ObservacionDtm>();
                case enumNegocio.Presupuesto:
                    return contexto.Set<ObservacionesDeUnPresupuestoDtm>().Cast<ObservacionDtm>();
                case enumNegocio.Contrato:
                    return contexto.Set<ObservacionesDeUnContratoDtm>().Cast<ObservacionDtm>();
                case enumNegocio.Archivador:
                    return contexto.Set<ObservacionesDeUnArchivadorDtm>().Cast<ObservacionDtm>();
                case enumNegocio.CircuitoDoc:
                    return contexto.Set<ObservacionesDeUnCircuitoDocDtm>().Cast<ObservacionDtm>();
                case enumNegocio.FacturaEmitida:
                    return contexto.Set<ObservacionesDeUnaFacturaEmtDtm>().Cast<ObservacionDtm>();
                case enumNegocio.ParteDeTrabajo:
                    return contexto.Set<ObservacionesDeUnParteTrDtm>().Cast<ObservacionDtm>();
                case enumNegocio.PlanificacionDeVenta:
                    return contexto.Set<ObservacionesDeUnaPlanificacionDeVentaDtm>().Cast<ObservacionDtm>();
                case enumNegocio.RemesaFae:
                    return contexto.Set<ObservacionesDeUnaRemesaFaeDtm>().Cast<ObservacionDtm>();
                case enumNegocio.FacturaRecibida:
                    return contexto.Set<ObservacionesDeUnaFacturaRecDtm>().Cast<ObservacionDtm>();
                case enumNegocio.Pedido:
                    return contexto.Set<ObservacionesDeUnPedidoDtm>().Cast<ObservacionDtm>();
                case enumNegocio.Preasiento:
                    return contexto.Set<ObservacionesDeUnPreasientoDtm>().Cast<ObservacionDtm>();
                case enumNegocio.Pago:
                    return contexto.Set<ObservacionesDeUnPagoDtm>().Cast<ObservacionDtm>();
                case enumNegocio.RemesaPag:
                    return contexto.Set<ObservacionesDeUnaRemesaPagDtm>().Cast<ObservacionDtm>();
                case enumNegocio.Abogado:
                    return contexto.Set<ObservacionesDeUnAbogadoDtm>().Cast<ObservacionDtm>();
                case enumNegocio.Cliente:
                    return contexto.Set<ObservacionesDeUnClienteDtm>().Cast<ObservacionDtm>();
                case enumNegocio.Interlocutor:
                    return contexto.Set<ObservacionesDeUnInterlocutorDtm>().Cast<ObservacionDtm>();
                case enumNegocio.Persona:
                    return contexto.Set<ObservacionesDeUnaPersonaDtm>().Cast<ObservacionDtm>();
                case enumNegocio.Procurador:
                    return contexto.Set<ObservacionesDeUnProcuradorDtm>().Cast<ObservacionDtm>();
                case enumNegocio.Proveedor:
                    return contexto.Set<ObservacionesDeUnProveedorDtm>().Cast<ObservacionDtm>();
                case enumNegocio.Sociedad:
                    return contexto.Set<ObservacionesDeUnaSociedadDtm>().Cast<ObservacionDtm>();
                case enumNegocio.Trabajador:
                    return contexto.Set<ObservacionesDeUnTrabajadorDtm>().Cast<ObservacionDtm>();
                case enumNegocio.Infante:
                    return contexto.Set<ObservacionesDeUnInfanteDtm>().Cast<ObservacionDtm>();
                case enumNegocio.CursoDeGuarderia:
                    return contexto.Set<ObservacionesDeUnCursoDeGuarderiaDtm>().Cast<ObservacionDtm>();
            }

            throw new Exception($"Se debe indicar como obtener los tipos del negocio: {negocio}");
        }

    }
}
