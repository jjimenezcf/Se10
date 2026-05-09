using ServicioDeDatos.Expediente;
using ServicioDeDatos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.TrabajosSometidos;
using ModeloDeDto;
using ServicioDeDatos.TrabajosSometidos;
using System.Collections.Generic;
using Utilidades;
using GestorDeElementos;
using ServicioDeDatos.Presupuesto;
using Gestor.Errores;
using ServicioDeDatos.Tarea;
using System.Linq.Dynamic.Core;
using System.Linq;
using ServicioDeDatos.MaestrosTecnico;

namespace GestoresDeNegocio.Expediente
{
    public class AccionesDeExpedientes
    {
        public const string N_CerrarArchivadoresDeExpedientes = "Cierra los archivadores de un expediente";

        public const string N_ReabrirArchivadoresDeExpedientes = "Cierra los archivadores de un expediente";

        public const string N_EnviarCorreoAlSolicitante = "Enviar correo al solicitante";

        public const string N_ValidarQueHayPresupuesto = "Validar que hay al menos un presupuesto";

        public const string N_ValidarQueHayTareasRelacionadas = "Validar que hay al menos una tarea de algunos de los tipo relacionada";

        public const string N_ValidarQueLasTareasRelacionadasEstanPlanificadas = "Validar que las tarea estan planificadas";

        public const string N_ValidarQueSiHayPresupuestosNoEstanPrefacturados = "Validar que si hay algún presupuesto en el expediente, ninguno de ellos está prefacturado";

        public const string N_PresupuestarTareasRelacionadas = "Presupuesta las tareas relacionadas con un expediente";

        public enum enumParametros { IdTiposDeTarea, IdTipoPpt, Naturaleza, PrecioPorHora };

        public static readonly string parametrosParaValidarTareasRelacionadas = @"[{""parametro"": ""p_1"",""valor"": ""@p_1"" }]".Replace("p_1", enumParametros.IdTiposDeTarea.ToString());

        public static readonly string ParametrosEnviarCorreoAlSolicitante = $@"[
  {{
    'parametro': 'Asunto',
    'valor':'Expediente [{nameof(ExpedienteDtm.Referencia)}] .... '
  }},
  {{
    'parametro': 'Cuerpo',
    'valor':'....'
  }}
]";

        public static readonly string ParametrosParaPresupuestarTareasRelacionadas = $@"[
  {{
    'parametro': '{enumParametros.IdTipoPpt}',
    'valor':'@{enumParametros.IdTipoPpt}'
  }},
  {{
    'parametro': '{enumParametros.Naturaleza}',
    'valor':'@{enumParametros.Naturaleza}'
  }},
  {{
    'parametro': '{enumParametros.PrecioPorHora}',
    'valor': '@{enumParametros.Naturaleza}'
  }}
]";
        public static void EnviarCorreoAlSolicitante(EntornoDeUnaAccion entorno)
        {
            var expediente = (ExpedienteDtm)entorno.Registro;
            string receptor = expediente.eMail.IsNullOrEmpty()
            ? expediente.Tipo.ClaseDeExpediente == enumClaseDeExpediente.DeCliente
            ? expediente.Cliente(entorno.Contexto, crearCliente: false).eMail
            : expediente.Solicitante.eMail
            : expediente.eMail;

            GestorDeCorreos.CrearCorreoPara(entorno.Contexto,
             receptores: new List<string>(receptor.Split(Simbolos.separadorDeCorreos)),
             asunto: entorno.Parametros[nameof(CorreoDtm.Asunto)].ToString(),
             cuerpo: entorno.Parametros[nameof(CorreoDtm.Cuerpo)].ToString(),
             elementos: new List<TipoDtoElmento>(),
             archivos: new List<string>());
        }

        public static void ValidarQueHayPresupuesto(EntornoDeUnaAccion entorno)
        {
            var expediente = (ExpedienteDtm)entorno.Registro;
            if (!entorno.Contexto.Existen<PresupuestoDtm>(nameof(PresupuestoDtm.IdExpediente), expediente.Id))
                GestorDeErrores.Emitir($"No puede ejecutar la transición '{entorno.Transicion.Nombre}' ya que el expediente '{expediente.Referencia}' no tiene valoración adjunta");
        }

        public static void ValidarQueSiHayPresupuestosNoEstanPrefacturados(EntornoDeUnaAccion entorno)
        {
            var expediente = (ExpedienteDtm)entorno.Registro;
            var ppts = entorno.Contexto.SeleccionarTodos<PresupuestoDtm>(nameof(PresupuestoDtm.IdExpediente), expediente.Id, parametros: new Dictionary<string, object> { { ltrParametrosNeg.ExcluirCancelados, true } });
            foreach (var ppt in ppts)
            {
                if (ppt.TieneFacturas(entorno.Contexto))
                    GestorDeErrores.Emitir($"No puede ejecutar la transición '{entorno.Transicion.Nombre}' ya que el expediente {expediente.Referencia} está facturado");
            }
        }

        public static void ValidarQueHayTareasRelacionadas(EntornoDeUnaAccion entorno)
        {
            var expediente = (ExpedienteDtm)entorno.Registro;

            if (entorno.EsEventodeNegocio)
                return;

            if (entorno.Parametros.LeerValor(enumParametros.IdTiposDeTarea.ToString(), "").IsNullOrEmpty())
                GestorDeErrores.Emitir($"No puede ejecutar la transición '{entorno.Transicion.Nombre}' ya que no ha parametrizado el parámetro '{enumParametros.IdTiposDeTarea}' en la acción '{entorno.Accion.Nombre}'");

            var tipos = entorno.Parametros.LeerValor<string>(enumParametros.IdTiposDeTarea.ToString()).ToLista<int>();
            if (tipos.Count == 0)
                GestorDeErrores.Emitir($"No puede ejecutar la transición '{entorno.Transicion.Nombre}' ya que ha parametrizado el parámetro '{enumParametros.IdTiposDeTarea}' en la acción '{entorno.Accion.Nombre}' de manera incorrecta, son una lista de ids de tipos separados por '{Simbolos.PuntoComa}'");

            var tareas = expediente.Tareas(entorno.Contexto).ToList();
            if (tareas.Count() == 0)
                GestorDeErrores.Emitir($"No puede ejecutar la transición '{entorno.Transicion.Nombre}' ya que el expediente '{expediente.Referencia}' no tiene tareas activas o terminadas relacionadas");

            if (tareas.Any(tarea => tipos.Contains(tarea.IdTipo)))
            {
                return;
            }

            GestorDeErrores.Emitir($"No puede ejecutar la transición '{entorno.Transicion.Nombre}' ya que el expediente '{expediente.Referencia}' no tiene tareas activas o terminadas relacionadas de los tipos parametrizados");
        }

        public static void ValidarQueLasTareasRelacionadasEstanPlanificadas(EntornoDeUnaAccion entorno)
        {
            var expediente = (ExpedienteDtm)entorno.Registro;

            if (entorno.EsEventodeNegocio)
                return;

            var tareas = expediente.Tareas(entorno.Contexto).ToList();
            foreach ( var tarea in tareas)
            {
                if (!tarea.Tipo<TipoDeTareaDtm>(contexto: entorno.Contexto).UsaPlanificacion) continue;
                var planificacion = tarea.Ampliacion<PlfDeTareaDtm>(entorno.Contexto, errorSiNoHay: false);
                if (planificacion == null)
                    GestorDeErrores.Emitir($"No puede ejecutar la transición '{entorno.Transicion.Nombre}' ya que la tarea '{tarea.Referencia}' ha de estar planificada");
                if (planificacion.PlfDeInicio is null || planificacion.PlfDeFin is null)
                    GestorDeErrores.Emitir($"No puede ejecutar la transición '{entorno.Transicion.Nombre}' ya que la tarea '{tarea.Referencia}' ha de estar planificada");
            }

        }

        public static void PresupuestarTareasRelacionadas(EntornoDeUnaAccion entorno)
        {
            var expediente = (ExpedienteDtm)entorno.Registro;

            if (entorno.EsEventodeNegocio)
                return;

            if (entorno.Parametros.LeerValor(enumParametros.IdTipoPpt.ToString(), "").IsNullOrEmpty())
                GestorDeErrores.Emitir($"No puede ejecutar la transición '{entorno.Transicion.Nombre}' ya que no ha parametrizado el parámetro '{enumParametros.IdTipoPpt}' en la acción '{entorno.Accion.Nombre}'");

            if (entorno.Parametros.LeerValor(enumParametros.Naturaleza.ToString(), "").IsNullOrEmpty())
                GestorDeErrores.Emitir($"No puede ejecutar la transición '{entorno.Transicion.Nombre}' ya que no ha indicado la sigla de la naturaleza con la que presupuestar en el parámetro '{enumParametros.Naturaleza}' en la acción '{entorno.Accion.Nombre}'");

            if (entorno.Parametros.LeerValor(enumParametros.PrecioPorHora.ToString(), "").IsNullOrEmpty())
                GestorDeErrores.Emitir($"No puede ejecutar la transición '{entorno.Transicion.Nombre}' ya que no ha indicado el precio por hora con el que presupuestar en el parámetro '{enumParametros.PrecioPorHora}' en la acción '{entorno.Accion.Nombre}'");


            var tipoPpt = entorno.Contexto.SeleccionarPorId<TipoDePresupuestoDtm>(entorno.Parametros.LeerValor<string>(enumParametros.IdTipoPpt.ToString()).Entero());
            var naturaleza = entorno.Contexto.SeleccionarPorPropiedad<NaturalezaDtm>(nameof(NaturalezaDtm.Sigla), entorno.Parametros.LeerValor<string>(enumParametros.Naturaleza.ToString()));
            var precio = entorno.Parametros.LeerValor<string>(enumParametros.PrecioPorHora.ToString()).Decimal();

            if (precio <= 0)
                GestorDeErrores.Emitir($"No puede ejecutar la transición '{entorno.Transicion.Nombre}' ya que el precio por hora '{entorno.Parametros.LeerValor<string>(enumParametros.PrecioPorHora.ToString())}' indicado en el parámetro '{enumParametros.PrecioPorHora}' en la acción '{entorno.Accion.Nombre}' no es válido");

            expediente.PresupuestarTareasRealizadas(entorno.Contexto, tipoPpt, naturaleza,precio);

        }

    }
}
