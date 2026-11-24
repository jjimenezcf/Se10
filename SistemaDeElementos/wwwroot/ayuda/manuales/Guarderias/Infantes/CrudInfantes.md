# Gestión de infantes

Los **niños de la guardería**: nombre, fecha de nacimiento (**Nacido el**, con su **edad**), contacto, **madre**, **padre**, fotografía, las
**direcciones de los familiares** y el **curso asignado**. La matrícula de cada niño se gestiona como contrato (*matrícula de guardería*).

## ¿Cómo hago...?

### ...para asignar o cambiar de curso a un niño?

Menú → **Cambiar de curso** (*Cambiar o asignar curso*): elija el curso.

### ...para dar de baja a un niño?

Marque **Baja**.

---

#### Detalle técnico

- Controlador: `InfantesController` (Controllers/Guarderias). Vista: `Views/Guarderias/CrudInfantes.cshtml`. Dto: `InfanteDto`.
  Descriptor: `DescriptorDeInfantes` (direcciones de familiares, modal `AsociarCursoDto`).
