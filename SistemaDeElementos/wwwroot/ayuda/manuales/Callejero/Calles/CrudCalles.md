# Gestión de calles

Callejero: cada calle tiene su **tipo de vía**, nombre, municipio y los tramos por **código postal**, **barrio** y **zona** (para cada
**mano** —pares o impares— los números **desde** / **hasta**). Las direcciones de los elementos se apoyan en el callejero, que se puede
**validar en el Catastro**.

## ¿Cómo hago...?

### ...para buscar una calle?

Por nombre y con los filtros **Pais**, **Provincia**, **Municipio**, **Barrio** y **Zona**.

### ...para dar de alta una calle?

**Crear**: tipo de vía, nombre, municipio y sus tramos (código postal, mano, desde, hasta, barrio, zona). Con **Validar en Catastro** se
comprueba contra el catastro.

### ...para ver las calles de un barrio o una zona?

Desde el barrio o la zona: *Calles de un barrio* / *Calles de una zona*.

---

#### Detalle técnico

- Controlador: `CallesController` (Controllers/Callejero). Vista: `Views/Callejero/CrudCalles.cshtml`. Dto: `CalleDto`.
- Descriptor: `DescriptorDeCalles` (filtros país, provincia, municipio, barrio, zona). ts: `wwwroot/ts/Callejero/Calles.ts`.
