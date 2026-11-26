## Cómo escribir en los filtros de texto

Los filtros de texto (el de la cabecera del filtro y cualquier otro campo de texto de la zona de filtro) no buscan sólo la palabra tal
cual: admiten unos símbolos para afinar la búsqueda. Sin ningún símbolo, se buscan los elementos que **contienen** lo escrito en
cualquier posición.

### El botón de criterio (lo más sencillo)

A la derecha de cada filtro de texto hay un pequeño botón con el criterio de búsqueda. Púlselo y elija en la lista:

| Botón | Criterio      | Escribo `juan;pedro` y encuentra...                           |
|-------|---------------|---------------------------------------------------------------|
| `⊃`   | Contiene      | los que contienen *juan* o *pedro* (es el criterio por defecto) |
| `=`   | Igual a       | los que son exactamente *juan* o *pedro*                      |
| `≠`   | Distinto de   | todos menos los que son exactamente *juan* o *pedro*, vacíos incluidos |
| `⊅`   | No contiene   | los que no contienen ni *juan* ni *pedro*, vacíos incluidos   |
| `a…`  | Comienza por  | los que empiezan por *juan* o por *pedro*                     |
| `…z`  | Termina en    | los que terminan en *juan* o en *pedro*                       |

- El botón se queda mostrando el criterio elegido y se resalta en azul cuando no es *Contiene*.
- Elegir el criterio no lanza la búsqueda: busque como siempre (Intro o el botón de buscar).
- **Blanquear filtros** vuelve todos los botones a *Contiene*.
- Las **plantillas de filtros** guardan el criterio de cada campo y, al aplicarlas, el botón vuelve a mostrarlo.
- Si escribe usted mismo los símbolos que se explican a continuación, mandan los símbolos y el botón no cambia nada.
- El filtro de **estados** también tiene botón: con *No contiene* y `terminado;cancelado` excluye esos estados.

### Resumen de los símbolos (usuarios avanzados)

Lo mismo que hace el botón, y algo más, se puede escribir directamente con estos símbolos:

| Escribo            | Encuentra                                                      | Ejemplo       | Encuentra, p.e.                       |
|--------------------|----------------------------------------------------------------|---------------|---------------------------------------|
| `texto`            | lo que **contiene** el texto en cualquier sitio                | `garcia`      | *Pedro García López*, *Garcia Sl*     |
| `=texto`           | lo que es **exactamente igual** al texto                       | `=F-2026-001` | sólo *F-2026-001*                     |
| `\|texto`          | lo que **empieza por** el texto                                | `\|F-2026`    | *F-2026-001*, pero no *RF-2026-001*   |
| `texto\|`          | lo que **termina en** el texto                                 | `001\|`       | *F-2026-001*, pero no *F-2026-0010*   |
| `uno%otro`         | lo que tiene *uno* y, más adelante, *otro* (`%` = cualquier cosa) | `obra%murcia` | *Obra nueva en Murcia*             |
| `a_c`              | `_` vale por **un solo carácter** cualquiera                   | `f_2026`      | *F-2026*, *F/2026*, *F.2026*          |
| `uno;otro`         | lo que cumple **una u otra** alternativa (`;` = "o")           | `garcia;perez`| los que contienen *García* o *Pérez*  |
| `!texto`          | lo **contrario**: lo que **no** cumple lo escrito, vacíos incluidos | `!juan;jiménez` | los que no tienen ni *Juan* ni *Jiménez* |

### El distinto `!`

`!` delante de todo lo escrito da la vuelta a la búsqueda: devuelve los elementos que **no** cumplen ninguna de las alternativas,
**incluidos los que tienen ese dato vacío**. Se combina con el resto de símbolos, que se escriben a continuación:

| Escribo          | Encuentra                                                                  |
|------------------|----------------------------------------------------------------------------|
| `!juan`         | lo que **no contiene** *juan*                                              |
| `!juan;jiménez` | lo que no contiene *juan* **ni** *jiménez*                                 |
| `!juan%jiménez` | lo que no tiene *juan* seguido, más adelante, de *jiménez* (sí devuelve *Juan Pérez* o *Ana Jiménez*) |
| `!=F-001`       | todo lo que es **distinto de** exactamente *F-001*                          |
| `!\|F-2026`     | lo que **no empieza** por *F-2026*                                         |
| `!001\|`        | lo que **no termina** en *001*                                             |
| `!=F-001;\|R-`  | lo que no es *F-001* ni empieza por *R-*                                   |

- El `!` va **sólo al principio** y afecta a todas las alternativas; en medio (`garcia;!perez`) se busca el texto `!perez` tal cual.
- Fíjese en la diferencia entre `;` y `%`: `!juan;jiménez` quita los que tienen cualquiera de las dos palabras; `!juan%jiménez` sólo
  quita los que tienen las dos y en ese orden.
- Si el campo ya tiene un criterio que niega (*no contiene*, *distinto de*), el `!` lo anula y se busca lo que sí contiene.
- En el filtro de **estados** también vale: `!terminado;cancelado` excluye esos estados.

### Reglas que conviene saber

- **El `;` separa alternativas** y cada alternativa lleva sus propios símbolos: `=F-001;|R-2026;003|` encuentra la referencia exacta
  *F-001*, o las que empiezan por *R-2026*, o las que terminan en *003*.
- **Los espacios de alrededor no cuentan**: `garcia ; perez` es lo mismo que `garcia;perez`. Los espacios de dentro sí: `obra nueva`
  busca esas dos palabras juntas y en ese orden; si pueden estar separadas use `obra%nueva`.
- **El `=` manda**: con `=` delante se busca el texto exacto y los `%` y `_` se toman como caracteres normales. No lo combine con `|`.
- **`|` delante o detrás, no en los dos lados**: `|abc|` sólo se toma como *empieza por* "abc|". Para "exactamente abc" use `=abc`.
- **`%` y `|` se combinan**: `|F-2026%01` empieza por *F-2026* y contiene después *01*; `obra%murcia|` contiene *obra* y termina en
  *murcia*.
- **Sin texto no se filtra**: un campo vacío no restringe nada. Para volver a ver todo use, en el menú de filtro, **Blanquear filtros**.

### Filtros con prefijo (`letra:`)

En algunas vistas el filtro **Varios** elige el dato por el que buscar con una letra y dos puntos delante (por ejemplo, en facturas
emitidas `d:` busca por el nombre del cliente). El manual de cada vista dice qué letras admite. Los símbolos y el botón de criterio
se aplican a lo que va **detrás** del prefijo:

| Escribo                 | Encuentra                                              |
|-------------------------|--------------------------------------------------------|
| `d:garcia`              | facturas de clientes cuyo nombre contiene *garcia*     |
| `d:!garcia`             | facturas de clientes cuyo nombre **no** contiene *garcia* |
| `d:=Garcia Sl;=Pérez Sa`| facturas de esos dos clientes exactamente              |

- El prefijo va siempre lo primero: escriba `d:!garcia`, no `!d:garcia`.
- Con el botón de criterio basta con escribir `d:garcia` y elegir el criterio: el sistema pone los símbolos detrás del prefijo.
- Los prefijos que buscan un valor exacto con su propio formato (un número de factura, la referencia de un preasiento) ignoran
  el criterio y los símbolos.

### Filtros por nombre y referencia

En muchas vistas el filtro de la cabecera busca a la vez por **nombre o referencia**: `garcia` encuentra los elementos cuyo nombre *o*
referencia contienen *garcia*. Si escribe `=` delante (`=F-2026-001`) se busca la **misma referencia**, exactamente esa.

### Cuando el criterio ya viene elegido

Algunos campos del filtro tienen un criterio fijo (por ejemplo *empieza por*, *no contiene* o *distinto de*). En ellos los símbolos se
aplican igual sobre ese criterio:

- *no contiene* / *distinto de* devuelven los elementos que **no cumplen ninguna** de las alternativas escritas
  (`garcia;perez` → ni García ni Pérez), incluidos los que tienen el dato vacío.
- *empieza por* / *termina en* ya anclan el principio o el final; no hace falta escribir el `|`.

### Listas de números, importes y fechas

- Donde se pide una **lista de números** (identificadores, ejercicios...) sepárelos con **coma**: `12,15,20`. Si usa `;` por error,
  el sistema avisa de que hay valores no numéricos.
- Los **importes** se escriben con el separador decimal habitual (`1250,50`) y se eligen con el criterio del campo (igual, mayor o
  igual, menor o igual...).
- Las **fechas** se eligen con el selector de *desde* / *hasta*; se puede dejar vacía una de las dos para buscar "desde tal día" o
  "hasta tal día". La fecha *hasta* incluye el día entero.

### Si no encuentra lo que espera

- Pruebe sin símbolos: `garcia` busca en cualquier posición y es lo más amplio.
- Revise que no queden otros bloques del filtro rellenos (fechas, estados, relaciones): todos los criterios se suman.
- Compruebe el botón de criterio de cada campo: si quedó en *Igual a* o *Comienza por*, la búsqueda es más estricta de lo que parece.
- En algún filtro particular de una vista, un `_` suelto puede no aplicarse; si ocurre, combínelo con `;` o `%` (por ejemplo `f_2026;`).
  Hay filtros propios de alguna vista que no siguen estas reglas: en ellos ni los símbolos ni el botón cambian la búsqueda.
