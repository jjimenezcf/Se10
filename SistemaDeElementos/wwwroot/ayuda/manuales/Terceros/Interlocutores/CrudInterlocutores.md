# Gestión de interlocutores

Un interlocutor es **cualquier tercero con el que se trata** (persona o sociedad): es la pieza común de la que cuelgan sus roles
(**cliente**, **proveedor**, **trabajador**, **procurador**, **abogado**). Guarda correo, teléfono y sus **cuentas bancarias**. Se usa, por
ejemplo, en los interlocutores de un expediente o un pleito (*Gestionar interlocutores*).

## ¿Cómo hago...?

### ...para ver qué papeles tiene un tercero?

Seleccione el interlocutor y use el menú de relaciones: **Cliente**, **Proveedor**, **Trabajador**, **Procurador** o **Abogado**.

### ...para añadir una cuenta bancaria?

Apartado de **cuentas** de la edición.

### ...para dar de baja un interlocutor?

Marque **Baja**.

---

#### Detalle técnico

- Controlador: `InterlocutoresController` (Controllers/Terceros). Vista: `Views/Terceros/CrudInterlocutores.cshtml`. Dto: `InterlocutorDto`.
- Descriptor: `DescriptorDeInterlocutores` (relaciones a los roles, cuentas). ts: `wwwroot/ts/Terceros/Interlocutores.ts`.
