# Cursos de guardería

Un curso agrupa a los niños de una edad en un **aula**: **para la edad de** (y en qué se mide), el **profesor titular**, los **puestos de
consultor y de gestor** (quién ve y quién gestiona el curso) y si está **activo**. En la edición se ven sus **infantes** y **profesores**.

## ¿Cómo hago...?

### ...para crear un curso?

**Crear**: aula, edad, nombre, profesor titular y puestos. Después asigne los niños (*Infantes de un curso* o *Cambiar de curso* en el niño).

---

#### Detalle técnico

- Controlador: `CursosDeGuarderiaController` (Controllers/Guarderias). Vista: `Views/Guarderias/CrudCursosDeGuarderia.cshtml`. Dto:
  `CursoDeGuarderiaDto`. Descriptor: `DescriptorDeCursosDeGuarderia` (infantes y profesores).
