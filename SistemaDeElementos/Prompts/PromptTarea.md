
### R.Tareas.1 · Solicitantes e Interlocutores (`SolicitantesDeTarea`)
- **Disparador:** Tareas "pedidas por [Nombre]", "solicitadas por [Persona]", "donde [Nombre] sea solicitante".
- **Acción:** Genera:
  1. `{"Clausula": "SolicitantesDeTarea", "Criterio": "contiene", "Valor": "nombre1;nombre2"}`
  2. `{"Clausula": "quemostrar", "Criterio": "igual", "Valor": "9"}`
- **Nota técnica:** Este filtro buscará tanto al solicitante principal como a los interlocutores o solicitantes relacionados en la tarea.
 

### R.Tareas.2 · Responsables y Asignación
**R.Tareas.2.1 · Búsqueda por Responsable (Login) (`ResponsablesDeTarea`)**
- **Disparador:** Tareas "asignadas a [Nombre/Login]", "encargadas a [Persona]", "que lleva [Login]".
- **Acción:** Genera:
  1. `{"Clausula": "ResponsablesDeTarea", "Criterio": "contiene", "Valor": "login_o_nombre1;login_o_nombre2"}`
  2. `{"Clausula": "quemostrar", "Criterio": "igual", "Valor": "9"}`
- **Nota Técnica:** El valor se comparará mediante un `.Contains()` contra el campo Login en C#. No es necesario que el login sea exacto.

**R.Tareas.2.2 · Situación: Con Responsable (`asignacion`)**
- **Disparador:** Tareas "asignadas", "con responsable", "que tengan alguien encargado" (sin especificar quién).
- **Acción:** `{"Clausula": "asignacion", "Criterio": "igual", "Valor": "5"}` (Filtra x.IdResponsable != null).

**R.Tareas.2.3 · Situación: Sin Responsable (`asignacion`)**
- **Disparador:** Tareas "sin asignar", "sin responsable", "libres", "pendientes de asignar".
- **Acción:** `{"Clausula": "asignacion", "Criterio": "igual", "Valor": "6"}` (Filtra x.IdResponsable == null).

**Exclusión:** La detección de un nombre/login específico (R.2.1) anula la creación de filtros de situación (R.2.2 y R.2.3).
  
### R.Tareas.3 · Planificación de Tareas (Agenda)

**R.Tareas.3.1 · Planificación de Inicio (`filtroporplfdeinicio`)**
- **Disparador:** Tareas que "deben empezar", "planificadas para iniciar", "comenzarán", "previstas para", "tienen que arrancar" en [periodo].
- **Acción:** `{"Clausula": "filtroporplfdeinicio", "Criterio": "entreFechas", "Valor": "YYYY-MM-DDTHH:mm:ssZ-YYYY-MM-DDTHH:mm:ssZ"}`
- **Nota:** Úsalo solo cuando el usuario se refiera a la fecha de comienzo prevista, no a la creación.

**R.Tareas.3.2 · Planificación de Fin (`FiltroPorPlfDeFin`)**
- **Disparador:** Tareas que "deben terminar", "planificadas para finalizar", "vencen", "finalizarán", "tienen que acabar" en [periodo].
- **Acción:** `{"Clausula": "FiltroPorPlfDeFin", "Criterio": "entreFechas", "Valor": "YYYY-MM-DDTHH:mm:ssZ-YYYY-MM-DDTHH:mm:ssZ"}`
- **Nota:** Úsalo para fechas de entrega o vencimiento planificado.

**Regla de Ambigüedad:** Si el usuario dice "tareas de marzo" sin especificar si es creación o planificación, prioriza `fechacreacion` (R5), a menos que el contexto sugiera agenda/vencimiento.
### R.Tareas.4 · Facturación de Tareas

**R.Tareas.4.1 · Búsqueda por datos de factura (`facturasDeTareas`)**
- **Disparador:** Frases que mencionen datos específicos de la factura: "facturadas con número [X]", "asociadas a la factura [Nombre]", "referencia de factura [Y]".
- **Acción:** Genera:
  1. `{"Clausula": "facturasDeTareas", "Criterio": "esAlgunoDe", "Valor": "texto1;texto2"}`
  2. `{"Clausula": "quemostrar", "Criterio": "igual", "Valor": "9"}` (Para incluir canceladas/terminadas en la búsqueda).
- **Nota:** Esta regla tiene prioridad sobre 4.2 y 4.3.

**R.Tareas.4.2 · Situación: Tareas Facturadas (`facturada`)**
- **Disparador:** "tareas con factura", "ya facturadas", "que tengan factura", "asociadas a alguna factura".
- **Acción:** `{"Clausula": "facturada", "Criterio": "igual", "Valor": "5"}` (Representa `ltrParametrosNeg.ConRelacion`).

**R.Tareas.4.3 · Situación: Tareas Sin Facturar (`facturada`)**
- **Disparador:** "tareas sin factura", "no facturadas", "pendientes de facturar", "sin asociar a factura".
- **Acción:** `{"Clausula": "facturada", "Criterio": "igual", "Valor": "6"}` (Representa `ltrParametrosNeg.SinRelacion`).

**Exclusión:** Si se aplica R.Tareas.4.1, NO generar las cláusulas de R.Tareas.4.2 o 4.3.  

### R.Tareas.5 · Relación con Expedientes

**R.Tareas.5.1 · Búsqueda por datos de Expediente (`expedientesDeTareas`)**
- **Disparador:** Tareas "del expediente [Nombre/Referencia]", "vinculadas al expediente [X]", "que pertenecen al expediente [Y]", "del exp [Referencia]".
- **Acción:** Genera dos objetos:
  1. `{"Clausula": "expedientesDeTareas", "Criterio": "contiene", "Valor": "nombre_o_referencia1;nombre_o_referencia2"}`
  2. Por defecto, añadir: `{"Clausula": "quemostrar", "Criterio": "igual", "Valor": "9"}`
- **Nota:** Esta regla tiene prioridad absoluta si se menciona un nombre o número de expediente.

**R.Tareas.5.2 · Situación: Tareas con cualquier Expediente (`relacionadaConExpediente`)**
- **Disparador:** Tareas "con expediente", "que tengan expediente", "vinculadas a algún expediente", "ya relacionadas con expedientes".
- **Acción:** Genera un objeto:
  1. `{"Clausula": "relacionadaConExpediente", "Criterio": "igual", "Valor": "5"}` (Equivale a ConRelacion).

**R.Tareas.5.3 · Situación: Tareas sin Expediente (`relacionadaConExpediente`)**
- **Disparador:** Tareas "sin expediente", "que no tengan expediente", "no vinculadas", "tareas huérfanas de expediente".
- **Acción:** Genera un objeto:
  1. `{"Clausula": "relacionadaConExpediente", "Criterio": "igual", "Valor": "6"}` (Equivale a SinRelacion).

**R.Tareas.5.4 · Exclusión para vinculación nueva (`vincularCon`)**
- **Disparador:** Frases tipo "tareas que NO estén en ningún expediente para poder vincular", "excluir las ya relacionadas con expedientes".
- **Acción:** Genera un objeto:
  1. `{"Clausula": "vincularCon", "Criterio": "igual", "Valor": "expediente"}`
- **Nota:** Esta regla activa la exclusión mediante la tabla `TareasDeUnExpedienteDtm`.

**Jerarquía:** La aplicación de R.Tareas.5.1 excluye automáticamente a R.Tareas.5.2 y R.Tareas.5.3.

### R.Tareas.6 · Relación con Presupuestos (PPT)

**R.Tareas.6.1 · Búsqueda por datos de Presupuesto (`presupuestosDeTareas`)**
- **Disparador:** Tareas "del presupuesto [Nombre/Referencia]", "vinculadas al PPT [X]", "que pertenecen al presupuesto [Y]", "del presupuesto con número [Referencia]".
- **Acción:** Genera dos objetos:
  1. `{"Clausula": "presupuestosDeTareas", "Criterio": "contiene", "Valor": "nombre_o_referencia1;nombre_o_referencia2"}`
  2. Por defecto, añadir: `{"Clausula": "quemostrar", "Criterio": "igual", "Valor": "9"}`
- **Nota:** Esta regla busca coincidencias parciales en el nombre o la referencia de los presupuestos vinculados.

**R.Tareas.6.2 · Situación: Tareas con cualquier Presupuesto (`relacionadaConPpt`)**
- **Disparador:** Tareas "con presupuesto", "que tengan PPT", "vinculadas a algún presupuesto", "tareas presupuestadas".
- **Acción:** Genera un objeto:
  1. `{"Clausula": "relacionadaConPpt", "Criterio": "igual", "Valor": "5"}` (Equivale a ConRelacion).

**R.Tareas.6.3 · Situación: Tareas sin Presupuesto (`relacionadaConPpt`)**
- **Disparador:** Tareas "sin presupuesto", "que no tengan PPT", "no presupuestadas", "pendientes de presupuesto".
- **Acción:** Genera un objeto:
  1. `{"Clausula": "relacionadaConPpt", "Criterio": "igual", "Valor": "6"}` (Equivale a SinRelacion).

**Jerarquía:** Si se detecta una búsqueda por nombre o número (R.6.1), se omiten las reglas de situación (R.6.2 y R.6.3).
