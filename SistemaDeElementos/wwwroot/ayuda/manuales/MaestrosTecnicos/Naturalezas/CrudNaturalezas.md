# Naturalezas contables

Una **naturaleza** clasifica contablemente lo que se compra o se vende: **clase**, **sigla**, nombre (*Naturaleza contable*), **unidad** por
defecto y sus **cuenta de gasto** y **cuenta de ingreso**. Las líneas de facturas, pedidos y unitarios llevan naturaleza, y de ella salen las
cuentas de los **preasientos**.

## ¿Cómo hago...?

### ...para que una factura se pueda contabilizar?

Todas sus líneas de base han de tener naturaleza, y la naturaleza su **cuenta de gasto** (facturas recibidas) o **de ingreso** (emitidas).
Si falta la cuenta, el preasiento no se genera.

---

#### Detalle técnico

- Controlador: `NaturalezasController` (Controllers/MaestrosTecnicos). Vista: `Views/MaestrosTecnico/CrudNaturalezas.cshtml`. Dto:
  `NaturalezaDto`. Descriptor: `DescriptorDeNaturalezas`.
