using ServicioDeDatos.Entorno;
using ServicioDeDatos;
using System.Collections.Generic;
using ServicioDeDatos.Elemento;
using Utilidades;
using Gestor.Errores;
using System;

namespace GestorDeElementos.Extensores
{
    public static class ExtensorDeAcciones
    {
        public static Dictionary<string, object> Ejecutar(this AccionDtm accion, EntornoDeUnaAccion entorno,  string parametros)
        {
            entorno.AsignarParametros(parametros);
            entorno.Contexto.Accion = accion;
            try
            {
                return EntornoDeUnaAccion.EjecutarAccion(entorno);
            }
            finally
            {
                entorno.Contexto.Accion = null;
            }
        }

        public static Dictionary<string, object> Ejecutar(this AccionDtm accion, ContextoSe contexto, IRegistro registro, enumNegocio negocio, Dictionary<string, object> entrada, string parametros)
        =>
        Ejecutar(accion, new EntornoDeUnaAccion(contexto, registro, negocio, entrada), parametros);

        public static void PersistirAccion(this AccionDtm accion, ContextoSe contexto)
        {
            accion.Id = 0;
            var porMetodo = ExistePorMetodo(contexto, accion);
            var porNombre = ExistePorNombre(contexto, accion);

            if (porMetodo.existe && porNombre.existe)
            {
                if (accion.Descripcion != porMetodo.accion.Descripcion)
                {
                    accion.Id = porMetodo.accion.Id;
                    accion.Modificar(contexto);
                }
                return;
            };

            if (!porMetodo.existe && !porNombre.existe)
            {
                accion.Insertar(contexto);
                return;
            }

            if (porMetodo.existe && !porNombre.existe)
            {
                var accionLeida = porMetodo.accion is null ? throw new Exception("Debería haber una acción") : porMetodo.accion;
                var hayCambioDeNombre = accion.Nombre != accionLeida.Nombre || accion.Descripcion != accionLeida.Descripcion;
                if (!hayCambioDeNombre)
                    return;
                accion.Id = porMetodo.accion.Id;
                accion.Modificar(contexto);
                return;
            }

            if (!porMetodo.existe && porNombre.existe)
            {
                var _ = porNombre.accion is null ? throw new Exception("Debería haber una acción") : porNombre.accion;
                var hayCambioDeMetodo = accion.Metodo != porNombre.accion.Metodo;
                if (!hayCambioDeMetodo)
                    return;
                accion.Id = porNombre.accion.Id;
                accion.Modificar(contexto);
                return;
            }

            GestorDeErrores.Emitir($"La acción {accion.Nombre} está mal definida");
        }

        private static (bool existe, AccionDtm? accion) ExistePorMetodo(ContextoSe contexto, AccionDtm accion)
        {
            var filtros = new Dictionary<string, object>
            {
                { nameof(AccionDtm.Dll),accion.Dll },
                { nameof(AccionDtm.Clase),accion.Clase },
                { nameof(AccionDtm.Metodo),accion.Metodo }

            };
            var acciones = contexto.SeleccionarTodos<AccionDtm>(filtros);
            return (acciones.Count > 0, acciones.Count == 0 ? null : acciones[0]);
        }

        private static (bool existe, AccionDtm? accion) ExistePorNombre(ContextoSe contexto, AccionDtm accion)
        {
            var filtros = new Dictionary<string, object>
            {
                { nameof(AccionDtm.Nombre),accion.Nombre }
            };
            var acciones = contexto.SeleccionarTodos<AccionDtm>(filtros);
            return (acciones.Count > 0, acciones.Count == 0 ? null : acciones[0]);
        }

    }
}
