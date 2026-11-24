# Gestión de personas

Una persona es el **dato básico de un tercero** (persona física): nombre, apellidos, **NIF** (o NIE), correo y teléfono. A partir de una
persona se crean sus **roles**: interlocutor, cliente, proveedor, trabajador, procurador o abogado. Las sociedades (personas jurídicas) se
gestionan en *Gestión de sociedades*.

## ¿Cómo hago...?

### ...para dar de alta un cliente, proveedor o trabajador que es una persona física?

1. **Crear** la persona: nombre, apellidos, NIF (marque **Es nie** si es un NIE), correo y teléfono.
2. En la edición, use los enlaces **Crear interlocutor**, **Crear cliente**, **Crear proveedor**, **Crear trabajador**, **Crear procurador**
   o **Crear abogado** según el papel que tenga. El indicador **Es interlocutor** muestra si ya lo es.

### ...para ir al interlocutor de una persona?

Menú de relaciones → **Interlocutor**.

### ...para dar de baja una persona?

Marque **Baja**.

---

#### Detalle técnico

- Controlador: `PersonasController` (Controllers/Terceros). Vista: `Views/Terceros/CrudPersonas.cshtml`. Dto: `PersonaDto`.
- Descriptor: `DescriptorDePersonas` (relación *Interlocutor*). ts: `wwwroot/ts/Terceros/Personas.ts`.
