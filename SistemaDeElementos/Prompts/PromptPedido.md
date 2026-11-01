
### R.Pedidos.1 · Pendientes de recepcionar (`FiltroPorEtapa`)
- **Disparador:** Pedidos "pendientes de recepcionar", "pendientes de recibir", "sin recibir", "que faltan por recibir", "pendientes de entrega".
- **Acción:** `{"Clausula": "FiltroPorEtapa", "Criterio": "igual", "Valor": "PED_Etapa_De_Solicitud|PED_Etapa_De_Recepcion"}`
- **Nota técnica:** Un pedido pendiente de recepcionar es el que ya se ha solicitado al proveedor y aún no está cerrado: `PED_Etapa_De_Solicitud` (todavía no se ha entregado nada) o `PED_Etapa_De_Recepcion` (entregado parcialmente). Ambas etapas van en un único objeto separadas por `|` (unión). No generes `quemostrar`: son pedidos en curso.

### R.Pedidos.2 · Importe del pedido (`FiltroPorImporte`)
- **Disparador:** Pedidos "cuyo importe sea más de [X]", "por más de [X]€", "de entre [X] y [Y] euros", "de menos de [X]", "con un total de [X]".
- **Acción:** Genera:
  1. `{"Clausula": "FiltroPorImporte", "Criterio": "entreImportes", "Valor": "min;max"}`
  2. `{"Clausula": "quemostrar", "Criterio": "igual", "Valor": "9"}` (Para incluir también cerrados/anulados).
- **Nota:** "más de [X]" → `X;` · "menos de [X]" → `0;X` · "entre [X] y [Y]" → `X;Y`. Es el importe del pedido sin IVA (lo pedido en sus líneas, con descuento). El valor es numérico, sin símbolo de moneda ("mil euros" → `1000`).

### R.Pedidos.3 · Importe entregado (`FiltroPorImporteEntregado`)
- **Disparador:** Pedidos "en donde se haya entregado ya [X] euros", "con entregas por más de [X]", "recepcionado/recibido más de [X]€", "sin nada entregado".
- **Acción:** Genera:
  1. `{"Clausula": "FiltroPorImporteEntregado", "Criterio": "entreImportes", "Valor": "min;max"}`
  2. `{"Clausula": "quemostrar", "Criterio": "igual", "Valor": "9"}`
- **Nota:** Mismo formato de valor que R.Pedidos.2. "Sin entregas" / "sin nada entregado" → `0;0`. Es el importe de todas las entregas, sin restar lo devuelto. Si la cantidad se expresa en porcentaje ("el 50%"), usa R.Pedidos.4.

### R.Pedidos.4 · Porcentaje recepcionado (`FiltroPorPorcentajeEntregado`)
- **Disparador:** Pedidos "que haya recepcionado ya el [P]%", "entregados al menos el [P]%", "recibidos al completo / al 100%", "con menos del [P]% recibido".
- **Acción:** Genera:
  1. `{"Clausula": "FiltroPorPorcentajeEntregado", "Criterio": "entreRangos", "Valor": "min;max"}`
  2. `{"Clausula": "quemostrar", "Criterio": "igual", "Valor": "9"}`
- **Nota:** Porcentaje (de 0 a 100) del importe del pedido que ya se ha entregado. "el [P]%" / "al menos [P]%" / "más de [P]%" → `P;` · "menos de [P]%" → `0;P` · "entre [P] y [Q]%" → `P;Q` · "completamente recibidos" → `100;`. El valor va sin el símbolo %.

### R.Pedidos.5 · Devoluciones (`FiltroPorImporteDevuelto`)
- **Disparador:** Pedidos "que tengan devoluciones", "con devoluciones", "con algo devuelto", "con devoluciones por más de [X]€", "sin devoluciones".
- **Acción:** Genera:
  1. `{"Clausula": "FiltroPorImporteDevuelto", "Criterio": "entreImportes", "Valor": "0.01;"}`
  2. `{"Clausula": "quemostrar", "Criterio": "igual", "Valor": "9"}`
- **Casos especiales:** "con devoluciones" → `0.01;` · "sin devoluciones" → `0;0` · con importe concreto → como R.Pedidos.2 (`X;`, `0;X`, `X;Y`).
- **Nota:** No lo confundas con la etapa `PED_Etapa_Devuelto` (R13): si el usuario pide pedidos "en etapa/estado devuelto" usa R13; "con devoluciones" siempre se resuelve aquí.

### R.Pedidos.6 · Unitarios / materiales (`UnitariosDePedidos`)
- **Disparador:** Pedidos "con el unitario [X]", "que lleven el material [X]", "con los materiales cuyas referencias son [X], [Y], [Z]", "con líneas del unitario [X]".
- **Acción:** Genera:
  1. `{"Clausula": "UnitariosDePedidos", "Criterio": "contiene", "Valor": "texto1;texto2;texto3"}`
  2. `{"Clausula": "quemostrar", "Criterio": "igual", "Valor": "9"}`
- **Nota técnica:** Devuelve los pedidos con alguna línea de un unitario cuyo nombre o referencia contenga alguno de los valores (coincidencia parcial, sin distinguir mayúsculas). Separa los valores con `;` y consérvalos tal cual los dio el usuario: son referencias o nombres, no apliques la limpieza de palabras vacías.

### R.Pedidos.7 · Expediente
**R.Pedidos.7.1 · Por datos del expediente (`ExpedientesDePedidos`)**
- **Disparador:** Pedidos "para el expediente [Nombre/Referencia]", "del expediente [X]", "imputados al expediente [X]".
- **Acción:** Genera:
  1. `{"Clausula": "ExpedientesDePedidos", "Criterio": "contiene", "Valor": "nombre_o_referencia1;nombre_o_referencia2"}`
  2. `{"Clausula": "quemostrar", "Criterio": "igual", "Valor": "9"}`

**R.Pedidos.7.2 · Con o sin expediente (`AsociadaAUnExpediente`)**
- **Disparador:** "con expediente", "imputados a algún expediente" (Valor `5`) · "sin expediente", "no imputados a ningún expediente" (Valor `6`).
- **Acción:** Genera:
  1. `{"Clausula": "AsociadaAUnExpediente", "Criterio": "igual", "Valor": "5"}` (o `6` para "sin expediente")
  2. `{"Clausula": "quemostrar", "Criterio": "igual", "Valor": "9"}`

**Jerarquía:** Si se indica un nombre o referencia de expediente (R.Pedidos.7.1), no generes R.Pedidos.7.2.

### R.Pedidos.8 · Almacén
**R.Pedidos.8.1 · Por datos del almacén (`AlmacenesDePedidos`)**
- **Disparador:** Pedidos "del almacén [Nombre/Referencia]", "con destino al almacén [X]", "para el almacén [X]".
- **Acción:** Genera:
  1. `{"Clausula": "AlmacenesDePedidos", "Criterio": "contiene", "Valor": "nombre_o_referencia1;nombre_o_referencia2"}`
  2. `{"Clausula": "quemostrar", "Criterio": "igual", "Valor": "9"}`

**R.Pedidos.8.2 · Con o sin almacén (`AsociadaAUnAlmacen`)**
- **Disparador:** "pedidos de almacén", "con destino a almacén", "que van a un almacén" (Valor `5`) · "sin almacén", "que no van a un almacén", "que van a obra" (Valor `6`).
- **Acción:** Genera:
  1. `{"Clausula": "AsociadaAUnAlmacen", "Criterio": "igual", "Valor": "5"}` (o `6` para "sin almacén")
  2. `{"Clausula": "quemostrar", "Criterio": "igual", "Valor": "9"}`

**Jerarquía:** Si se indica un nombre o referencia de almacén (R.Pedidos.8.1), no generes R.Pedidos.8.2.
