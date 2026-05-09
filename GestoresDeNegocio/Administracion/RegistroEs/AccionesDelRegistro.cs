using System;
using System.Collections.Generic;
using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Negocio;
using ServicioDeDatos;
using ServicioDeDatos.RegistroEs;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Tarea;
using Utilidades;

namespace GestoresDeNegocio.RegistroEs
{
    public static class AccionesDelRegistro
    {
        public static readonly string N_CrearTareaDeRegistroEs = "Crear una tarea de un tipo y en un Cg";
        public static readonly string N_ValidarTareasNoEnCircuito = "Valida que las tareas no están en circuito";
        public static readonly string N_VincularArchivadores = "Vincula los archivadores del registro con el objeto indicado";
        public static readonly string N_DesvincularArchivadores = "Desvincula los archivadores del registro con el objeto indicado";
        public static readonly string N_ValidarDesvincularUnaTareaDeUnRegistro = "Validar que se puede desvincular la tarea del registro";
        public static readonly string N_ValidarTareasCanceladas = "Valida que las tareas vinculadas están canceladas";

        public static readonly string D_ValidarDesvincularUnaTareaDeUnRegistro = "Al desvincular una tarea del registro, validar que la tarea está cancelada";
        public static readonly string D_ValidarTareasCanceladas = "Valida que no hay tareas vinculadas en circuito sin cancelar. No hay parámetros";


        public static void CrearTareaDeRegistroEs(EntornoDeUnaAccion entorno)
        {
            if (!entorno.Parametros.ContainsKey(nameof(TareaDtm.IdCg)))
                GestorDeErrores.Emitir($"No se puede crear la tarea asociada al registro por no tener definido el parámetro del centro gestor");
            if (!entorno.Parametros.ContainsKey(nameof(TareaDtm.IdTipo)))
                GestorDeErrores.Emitir($"No se puede crear la tarea asociada al registro por no tener definido el parámetro del tipo de tarea");

            var registroEs = (RegistroEsDtm)entorno.Registro;
            var tarea = new TareaDtm();
            tarea.IdCg = (int)((long)entorno.Parametros[nameof(TareaDtm.IdCg)]);
            tarea.IdTipo = (int)((long)entorno.Parametros[nameof(TareaDtm.IdTipo)]);
            tarea.Nombre = $"Tarea de resolución del registro {registroEs.Referencia}";
            tarea.Descripcion = registroEs.Nombre + Environment.NewLine + registroEs.Descripcion;
            tarea.IdSolicitante = registroEs.IdSolicitante;
            tarea.IdArchivador = registroEs.IdArchivadorInterno;
            tarea.Contacto = registroEs.Contacto;
            tarea.Telefono = registroEs.Telefono;
            tarea.eMail = registroEs.eMail;

            tarea.Insertar(entorno.Contexto, new Dictionary<string, object> { { nameof(ParametrosDeNegocio.EstaEjecutandoUnaAccion), true } });
            registroEs.Vincular(entorno.Contexto, tarea, new Dictionary<string, object>() { { nameof(ParametrosDeNegocio.EstaEjecutandoUnaAccion), true } });
        }

        public static void ValidarTareasNoEnCircuito(EntornoDeUnaAccion entorno)
        {
            var r = (RegistroEsDtm)entorno.Registro;
            foreach (var tarea in r.Vinculados<TareaDtm>(entorno.Contexto))
            {
                var estado = GestorDeEstados.Gestor(entorno.Contexto, enumNegocio.Tarea).LeerRegistroPorId(tarea.IdEstado);
                if (!estado.Terminado && !estado.Cancelado)
                    GestorDeErrores.Emitir($"Antes de ejecutar la transición '{entorno.Transicion.Nombre}' has de finalizar la tarea {tarea.Referencia}");
            }
        }
        public static void ValidarTareasCanceladas(EntornoDeUnaAccion entorno)
        {
            var r = (RegistroEsDtm)entorno.Registro;
            foreach (var tarea in r.Vinculados<TareaDtm>(entorno.Contexto))
            {
                var estado = GestorDeEstados.Gestor(entorno.Contexto, enumNegocio.Tarea).LeerRegistroPorId(tarea.IdEstado);
                if (!estado.Cancelado)
                    GestorDeErrores.Emitir($"Antes de ejecutar la transición '{entorno.Transicion.Nombre}' has de cancelar la tarea {tarea.Referencia}");
            }
        }

        public static void VincularArchivadores(EntornoDeUnaAccion entorno)
        {
            var idRegistro = entorno.Entrada.LeerValor<int>(nameof(ltrParametrosNeg.IdElemento));
            var vinculado = entorno.Entrada.LeerValor<enumNegocio>(nameof(ltrParametrosNeg.Vinculado));
            var idVinculado = entorno.Entrada.LeerValor<int>(nameof(ltrParametrosNeg.IdVinculado));

            var archivadores = GestorDeVinculos.RegistrosVinculados<ArchivadorDtm>(entorno.Contexto, enumNegocio.Registro, enumNegocio.Archivador, idRegistro, new Dictionary<string, object>());
            foreach (var archivador in archivadores)
                GestorDeVinculos.Vincular(entorno.Contexto, vinculado, enumNegocio.Archivador, idVinculado, archivador.Id, new Dictionary<string, object>
                {
                    {ltrParametrosNeg.AccionQueSeEjecuta, nameof(VincularArchivadores)},
                    {ltrParametrosNeg.ValidarPermisosDePersistencia, false }
                });
        }

        public static void DesvincularArchivadores(EntornoDeUnaAccion entorno)
        {
            var idRegistro = entorno.Entrada.LeerValor<int>(nameof(ltrParametrosNeg.IdElemento));
            var vinculado = entorno.Entrada.LeerValor<enumNegocio>(nameof(ltrParametrosNeg.Vinculado));
            var idVinculado = entorno.Entrada.LeerValor<int>(nameof(ltrParametrosNeg.IdVinculado));

            var archivadores = GestorDeVinculos.RegistrosVinculados<ArchivadorDtm>(entorno.Contexto, enumNegocio.Registro, enumNegocio.Archivador, idRegistro, new Dictionary<string, object>());
            foreach (var archivador in archivadores)
                GestorDeVinculos.BorrarVinculo(entorno.Contexto, vinculado, enumNegocio.Archivador, idVinculado, archivador.Id, new Dictionary<string, object>());
        }

        public static void ValidarDesvincularUnaTareaDeUnRegistro(EntornoDeUnaAccion entorno)
        {
            var idRegistro = entorno.Entrada.LeerValor<int>(nameof(ltrParametrosNeg.IdElemento));
            var idTarea = entorno.Entrada.LeerValor<int>(nameof(ltrParametrosNeg.IdVinculado));

            var tarea = entorno.Contexto.SeleccionarPorId<TareaDtm>(idTarea, aplicarJoin:true);
            if (!tarea.Estado.Cancelado)
                GestorDeErrores.Emitir($"Para desvincular la tarea {tarea.Referencia} del registro {entorno.Contexto.SeleccionarPorId<RegistroEsDtm>(idRegistro).Referencia} ha de estar cancelada");
        }
    }
}
