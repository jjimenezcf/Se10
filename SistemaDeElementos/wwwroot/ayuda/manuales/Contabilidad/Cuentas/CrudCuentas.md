# Cuentas contables

Maestro del **plan de cuentas**: cada cuenta tiene su **Código** y su descripción (**Cuenta contable**). Las usan las naturalezas (cuenta
de gasto), los almacenes (existencias, variación, compras, devoluciones), los IVA, los IRPF, los bancos... y los preasientos que se
generan con ellas.

## ¿Cómo hago...?

### ...para buscar una cuenta?

Filtro **Cuenta**: busca por código o por descripción.

### ...para dar de alta una cuenta?

**Crear**: código y descripción. Algunas cuentas las crea el sistema cuando hacen falta (p. ej. la de variación de existencias de un
almacén si no se indica).

---

#### Detalle técnico

- Controlador: `CuentasController` (Controllers/Contabilidad). Vista: `Views/Contabilidad/CrudCuentas.cshtml`. Dto: `CuentaDto`.
- Descriptor: `DescriptorDeCuentas` (filtro *Cuenta* por código o descripción). ts: `wwwroot/ts/Contabilidad/Cuentas.ts`.
