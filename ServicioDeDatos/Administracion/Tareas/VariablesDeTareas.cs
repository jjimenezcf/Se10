using Gestor.Errores;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Negocio;
using System.Collections.Generic;
using System.ComponentModel;
using Utilidades;

namespace ServicioDeDatos.Tarea
{

    public enum enumEtapasDeTareas
    {
        [Description("Estados en los que una tarea está pendiente de iniar o asignar")]
        TAR_Etapa_Inicial,
        [Description("Estados en los que una tarea está asignada")]
        TAR_Etapa_Asignada,
        [Description("Estados en los que una tarea está en ejecución")]
        TAR_Etapa_En_Resolucion,
        [Description("Estados en los que una tarea está parada")]
        TAR_Etapa_En_Espera,
        [Description("Estados en los que una tarea está pendiente validar")]
        TAR_Etapa_Validacion,
        [Description("Estados en los que una tarea está terminada")]
        TAR_Etapa_Terminada,
        [Description("Estados en los que una tarea está cancelada")]
        TAR_Etapa_Cancelado
    }

    public enum enumTransicionesDeTareas
    {

    }

    public enum enumParametrosDeTareas
    {
        [Description("Indica si se han de blanquear la información de finalización tras rechazar una tarea")]
        TAR_Eliminar_Valoracio_Tras_Rechazar,
        [Description("Indica si se le ha dar un tiempo de ejecución en base a las fechas al finalizar la tarea")]
        TAR_Proponer_Valoracion_Al_Finalizar
    }
    public static class VariablesDeTareas
    {
        internal static readonly string IA_Reglas_de_filtrado = @"

### R.Tareas.1 · Solicitantes e Interlocutores (`SolicitantesDeTarea`)
- **Disparador:** Tareas ""pedidas por [Nombre]"", ""solicitadas por [Persona]"", ""donde [Nombre] sea solicitante"".
- **Acción:** Genera:
  1. `{""Clausula"": ""SolicitantesDeTarea"", ""Criterio"": ""contiene"", ""Valor"": ""nombre1;nombre2""}`
  2. `{""Clausula"": ""quemostrar"", ""Criterio"": ""igual"", ""Valor"": ""9""}`
- **Nota técnica:** Este filtro buscará tanto al solicitante principal como a los interlocutores o solicitantes relacionados en la tarea.
 

### R.Tareas.2 · Responsables y Asignación
**R.Tareas.2.1 · Búsqueda por Responsable (Login) (`ResponsablesDeTarea`)**
- **Disparador:** Tareas ""asignadas a [Nombre/Login]"", ""encargadas a [Persona]"", ""que lleva [Login]"".
- **Acción:** Genera:
  1. `{""Clausula"": ""ResponsablesDeTarea"", ""Criterio"": ""contiene"", ""Valor"": ""login_o_nombre1;login_o_nombre2""}`
  2. `{""Clausula"": ""quemostrar"", ""Criterio"": ""igual"", ""Valor"": ""9""}`
- **Nota Técnica:** El valor se comparará mediante un `.Contains()` contra el campo Login en C#. No es necesario que el login sea exacto.

**R.Tareas.2.2 · Situación: Con Responsable (`asignacion`)**
- **Disparador:** Tareas ""asignadas"", ""con responsable"", ""que tengan alguien encargado"" (sin especificar quién).
- **Acción:** `{""Clausula"": ""asignacion"", ""Criterio"": ""igual"", ""Valor"": ""5""}` (Filtra x.IdResponsable != null).

**R.Tareas.2.3 · Situación: Sin Responsable (`asignacion`)**
- **Disparador:** Tareas ""sin asignar"", ""sin responsable"", ""libres"", ""pendientes de asignar"".
- **Acción:** `{""Clausula"": ""asignacion"", ""Criterio"": ""igual"", ""Valor"": ""6""}` (Filtra x.IdResponsable == null).

**Exclusión:** La detección de un nombre/login específico (R.2.1) anula la creación de filtros de situación (R.2.2 y R.2.3).
  
### R.Tareas.3 · Planificación de Tareas (Agenda)

**R.Tareas.3.1 · Planificación de Inicio (`filtroporplfdeinicio`)**
- **Disparador:** Tareas que ""deben empezar"", ""planificadas para iniciar"", ""comenzarán"", ""previstas para"", ""tienen que arrancar"" en [periodo].
- **Acción:** `{""Clausula"": ""filtroporplfdeinicio"", ""Criterio"": ""entreFechas"", ""Valor"": ""YYYY-MM-DDTHH:mm:ssZ-YYYY-MM-DDTHH:mm:ssZ""}`
- **Nota:** Úsalo solo cuando el usuario se refiera a la fecha de comienzo prevista, no a la creación.

**R.Tareas.3.2 · Planificación de Fin (`FiltroPorPlfDeFin`)**
- **Disparador:** Tareas que ""deben terminar"", ""planificadas para finalizar"", ""vencen"", ""finalizarán"", ""tienen que acabar"" en [periodo].
- **Acción:** `{""Clausula"": ""FiltroPorPlfDeFin"", ""Criterio"": ""entreFechas"", ""Valor"": ""YYYY-MM-DDTHH:mm:ssZ-YYYY-MM-DDTHH:mm:ssZ""}`
- **Nota:** Úsalo para fechas de entrega o vencimiento planificado.

**Regla de Ambigüedad:** Si el usuario dice ""tareas de marzo"" sin especificar si es creación o planificación, prioriza `fechacreacion` (R5), a menos que el contexto sugiera agenda/vencimiento.
### R.Tareas.4 · Facturación de Tareas

**R.Tareas.4.1 · Búsqueda por datos de factura (`facturasDeTareas`)**
- **Disparador:** Frases que mencionen datos específicos de la factura: ""facturadas con número [X]"", ""asociadas a la factura [Nombre]"", ""referencia de factura [Y]"".
- **Acción:** Genera:
  1. `{""Clausula"": ""facturasDeTareas"", ""Criterio"": ""esAlgunoDe"", ""Valor"": ""texto1;texto2""}`
  2. `{""Clausula"": ""quemostrar"", ""Criterio"": ""igual"", ""Valor"": ""9""}` (Para incluir canceladas/terminadas en la búsqueda).
- **Nota:** Esta regla tiene prioridad sobre 4.2 y 4.3.

**R.Tareas.4.2 · Situación: Tareas Facturadas (`facturada`)**
- **Disparador:** ""tareas con factura"", ""ya facturadas"", ""que tengan factura"", ""asociadas a alguna factura"".
- **Acción:** `{""Clausula"": ""facturada"", ""Criterio"": ""igual"", ""Valor"": ""5""}` (Representa `ltrParametrosNeg.ConRelacion`).

**R.Tareas.4.3 · Situación: Tareas Sin Facturar (`facturada`)**
- **Disparador:** ""tareas sin factura"", ""no facturadas"", ""pendientes de facturar"", ""sin asociar a factura"".
- **Acción:** `{""Clausula"": ""facturada"", ""Criterio"": ""igual"", ""Valor"": ""6""}` (Representa `ltrParametrosNeg.SinRelacion`).

**Exclusión:** Si se aplica R.Tareas.4.1, NO generar las cláusulas de R.Tareas.4.2 o 4.3.  

### R.Tareas.5 · Relación con Expedientes

**R.Tareas.5.1 · Búsqueda por datos de Expediente (`expedientesDeTareas`)**
- **Disparador:** Tareas ""del expediente [Nombre/Referencia]"", ""vinculadas al expediente [X]"", ""que pertenecen al expediente [Y]"", ""del exp [Referencia]"".
- **Acción:** Genera dos objetos:
  1. `{""Clausula"": ""expedientesDeTareas"", ""Criterio"": ""contiene"", ""Valor"": ""nombre_o_referencia1;nombre_o_referencia2""}`
  2. Por defecto, añadir: `{""Clausula"": ""quemostrar"", ""Criterio"": ""igual"", ""Valor"": ""9""}`
- **Nota:** Esta regla tiene prioridad absoluta si se menciona un nombre o número de expediente.

**R.Tareas.5.2 · Situación: Tareas con cualquier Expediente (`relacionadaConExpediente`)**
- **Disparador:** Tareas ""con expediente"", ""que tengan expediente"", ""vinculadas a algún expediente"", ""ya relacionadas con expedientes"".
- **Acción:** Genera un objeto:
  1. `{""Clausula"": ""relacionadaConExpediente"", ""Criterio"": ""igual"", ""Valor"": ""5""}` (Equivale a ConRelacion).

**R.Tareas.5.3 · Situación: Tareas sin Expediente (`relacionadaConExpediente`)**
- **Disparador:** Tareas ""sin expediente"", ""que no tengan expediente"", ""no vinculadas"", ""tareas huérfanas de expediente"".
- **Acción:** Genera un objeto:
  1. `{""Clausula"": ""relacionadaConExpediente"", ""Criterio"": ""igual"", ""Valor"": ""6""}` (Equivale a SinRelacion).

**R.Tareas.5.4 · Exclusión para vinculación nueva (`vincularCon`)**
- **Disparador:** Frases tipo ""tareas que NO estén en ningún expediente para poder vincular"", ""excluir las ya relacionadas con expedientes"".
- **Acción:** Genera un objeto:
  1. `{""Clausula"": ""vincularCon"", ""Criterio"": ""igual"", ""Valor"": ""expediente""}`
- **Nota:** Esta regla activa la exclusión mediante la tabla `TareasDeUnExpedienteDtm`.

**Jerarquía:** La aplicación de R.Tareas.5.1 excluye automáticamente a R.Tareas.5.2 y R.Tareas.5.3.

### R.Tareas.6 · Relación con Presupuestos (PPT)

**R.Tareas.6.1 · Búsqueda por datos de Presupuesto (`presupuestosDeTareas`)**
- **Disparador:** Tareas ""del presupuesto [Nombre/Referencia]"", ""vinculadas al PPT [X]"", ""que pertenecen al presupuesto [Y]"", ""del presupuesto con número [Referencia]"".
- **Acción:** Genera dos objetos:
  1. `{""Clausula"": ""presupuestosDeTareas"", ""Criterio"": ""contiene"", ""Valor"": ""nombre_o_referencia1;nombre_o_referencia2""}`
  2. Por defecto, añadir: `{""Clausula"": ""quemostrar"", ""Criterio"": ""igual"", ""Valor"": ""9""}`
- **Nota:** Esta regla busca coincidencias parciales en el nombre o la referencia de los presupuestos vinculados.

**R.Tareas.6.2 · Situación: Tareas con cualquier Presupuesto (`relacionadaConPpt`)**
- **Disparador:** Tareas ""con presupuesto"", ""que tengan PPT"", ""vinculadas a algún presupuesto"", ""tareas presupuestadas"".
- **Acción:** Genera un objeto:
  1. `{""Clausula"": ""relacionadaConPpt"", ""Criterio"": ""igual"", ""Valor"": ""5""}` (Equivale a ConRelacion).

**R.Tareas.6.3 · Situación: Tareas sin Presupuesto (`relacionadaConPpt`)**
- **Disparador:** Tareas ""sin presupuesto"", ""que no tengan PPT"", ""no presupuestadas"", ""pendientes de presupuesto"".
- **Acción:** Genera un objeto:
  1. `{""Clausula"": ""relacionadaConPpt"", ""Criterio"": ""igual"", ""Valor"": ""6""}` (Equivale a SinRelacion).

**Jerarquía:** Si se detecta una búsqueda por nombre o número (R.6.1), se omiten las reglas de situación (R.6.2 y R.6.3).

";

        private static string etapaInicial => enumNegocio.Tarea.Parametro(enumEtapasDeTareas.TAR_Etapa_Inicial, valorPorDefecto:0).Valor;
        private static string etapaAsignada => enumNegocio.Tarea.Parametro(enumEtapasDeTareas.TAR_Etapa_Asignada, valorPorDefecto:0).Valor;
        private static string etapaEnResolucion => enumNegocio.Tarea.Parametro(enumEtapasDeTareas.TAR_Etapa_En_Resolucion, valorPorDefecto:0).Valor;
        private static string etapaEnEspera => enumNegocio.Tarea.Parametro(enumEtapasDeTareas.TAR_Etapa_En_Espera, valorPorDefecto:0).Valor;
        private static string etapaEnValidacion => enumNegocio.Tarea.Parametro(enumEtapasDeTareas.TAR_Etapa_Validacion, valorPorDefecto:0).Valor;
        private static string etapaTerminada => enumNegocio.Tarea.Parametro(enumEtapasDeTareas.TAR_Etapa_Terminada, valorPorDefecto:0).Valor;
        private static string etapaCancelado => enumNegocio.Tarea.Parametro(enumEtapasDeTareas.TAR_Etapa_Cancelado, valorPorDefecto:0).Valor;

        public static string Estados(this enumEtapasDeTareas etapa)
        {
            string estados = null;
            switch (etapa)
            {
                case enumEtapasDeTareas.TAR_Etapa_Inicial: estados = etapaInicial; break;
                case enumEtapasDeTareas.TAR_Etapa_Asignada: estados = etapaAsignada; break;
                case enumEtapasDeTareas.TAR_Etapa_En_Resolucion: estados = etapaEnResolucion; break;
                case enumEtapasDeTareas.TAR_Etapa_En_Espera: estados = etapaEnEspera; break;
                case enumEtapasDeTareas.TAR_Etapa_Validacion: estados = etapaEnValidacion; break;
                case enumEtapasDeTareas.TAR_Etapa_Terminada: estados = etapaTerminada; break;
                case enumEtapasDeTareas.TAR_Etapa_Cancelado: estados = etapaCancelado; break;
            }
            return estados.IsNullOrEmpty() ? enumNegocio.Tarea.DefinirEtapaSiLoIndicaConfiguracion(etapa, ltrEstados.EstadoNulo) : estados;
        }

        public static bool EstaEnLaEtapa(this TareaDtm tarea, enumEtapasDeTareas etapa) => etapa.Lista().Contains(tarea.IdEstado);

        public static bool EstaEnAlgunaDeLasEtapa(this TareaDtm tarea, List<enumEtapasDeTareas> etapas)
        {
            var etapasDeLaTarea = tarea.Etapas();
            foreach (var etapa in etapas)
                if (etapasDeLaTarea.Contains(etapa)) return true;
            return false;
        }

        public static List<int> Lista(this enumEtapasDeTareas etapa) => etapa.Estados().ToLista<int>(Simbolos.Coma);

        public static List<string> Lista(this TareaDtm tarea) => tarea.CadenaDeEtapas().ToLista<string>(Simbolos.separadorDeEtapas);

        public static enumEtapasDeTareas Etapa(this TareaDtm tarea)
        {
            if (tarea.EstaEnLaEtapa(etapaEnEspera)) return enumEtapasDeTareas.TAR_Etapa_En_Espera;
            else
            if (tarea.EstaEnLaEtapa(etapaAsignada)) return enumEtapasDeTareas.TAR_Etapa_Asignada;
            else
            if (tarea.EstaEnLaEtapa(etapaCancelado)) return enumEtapasDeTareas.TAR_Etapa_Cancelado;
            else
            if (tarea.EstaEnLaEtapa(etapaEnValidacion)) return enumEtapasDeTareas.TAR_Etapa_Validacion;
            else
            if (tarea.EstaEnLaEtapa(etapaEnResolucion)) return enumEtapasDeTareas.TAR_Etapa_En_Resolucion;
            else
            if (tarea.EstaEnLaEtapa(etapaInicial)) return enumEtapasDeTareas.TAR_Etapa_Inicial;
            else
            if (tarea.EstaEnLaEtapa(etapaTerminada)) return enumEtapasDeTareas.TAR_Etapa_Terminada;

            throw Excepciones.Emitir($"No se ha definido la etapa de la tarea, " +
                $"cuando ésta está en el estado {tarea.Propiedad<EstadoDtm>(typeof(EstadoDeUnaTareaDtm)).Nombre}");
        }

        public static string Nombre(this enumEtapasDeTareas etapa, bool minusculas)
        {
            switch (etapa)
            {
                case enumEtapasDeTareas.TAR_Etapa_Inicial: return minusculas ? "pendiente de iniar o asignar" : "Pendiente de iniar o asignar";
                case enumEtapasDeTareas.TAR_Etapa_Asignada: return minusculas ? "asignación" : "Asignación";
                case enumEtapasDeTareas.TAR_Etapa_En_Espera: return minusculas ? "en espera" : "En espera";
                case enumEtapasDeTareas.TAR_Etapa_Cancelado: return minusculas ? "cancelación" : "Cancelación";
                case enumEtapasDeTareas.TAR_Etapa_Validacion: return minusculas ? "validación" : "Validación";
                case enumEtapasDeTareas.TAR_Etapa_En_Resolucion: return minusculas ? "en resolución" : "En Resolución";
                case enumEtapasDeTareas.TAR_Etapa_Terminada: return minusculas ? "terminada" : "Terminada";
            }
            return etapa.ToString();
        }

        private static bool EstaEnLaEtapa(this TareaDtm tarea, string etapa) => etapa.ToLista<int>(Simbolos.Coma).Contains(tarea.IdEstado);

        private static List<enumEtapasDeTareas> Etapas(this TareaDtm tarea)
        {
            var etapas = new List<enumEtapasDeTareas>();

            if (tarea.EstaEnLaEtapa(etapaEnEspera)) etapas.Add(enumEtapasDeTareas.TAR_Etapa_En_Espera);

            if (tarea.EstaEnLaEtapa(etapaAsignada)) etapas.Add(enumEtapasDeTareas.TAR_Etapa_Asignada);

            if (tarea.EstaEnLaEtapa(etapaCancelado)) etapas.Add(enumEtapasDeTareas.TAR_Etapa_Cancelado);

            if (tarea.EstaEnLaEtapa(etapaEnValidacion)) etapas.Add(enumEtapasDeTareas.TAR_Etapa_Validacion);

            if (tarea.EstaEnLaEtapa(etapaEnResolucion)) etapas.Add(enumEtapasDeTareas.TAR_Etapa_En_Resolucion);

            if (tarea.EstaEnLaEtapa(etapaInicial)) etapas.Add(enumEtapasDeTareas.TAR_Etapa_Inicial);

            if (tarea.EstaEnLaEtapa(etapaTerminada)) etapas.Add(enumEtapasDeTareas.TAR_Etapa_Terminada);

            if (etapas.Count == 0)
                throw Excepciones.Emitir($"No se ha definido la etapa de la tarea, " +
                $"cuando ésta está en el estado {tarea.Propiedad<EstadoDtm>(typeof(EstadoDeUnaTareaDtm)).Nombre}");

            return etapas;
        }

        private static string CadenaDeEtapas(this TareaDtm tarea) => string.Join(Simbolos.separadorDeEtapas, tarea.Etapas());


    }

}
