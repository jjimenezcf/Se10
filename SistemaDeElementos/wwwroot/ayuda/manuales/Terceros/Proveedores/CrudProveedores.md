# Gestión de proveedores

Un proveedor es una persona o sociedad **a la que se compra**: pedidos, facturas recibidas, pagos y contratos de compra. Además de correo,
teléfono y **cuenta contable**, guarda los **valores por defecto de sus facturas**, que se proponen al dar de alta una factura recibida suya.

## ¿Cómo hago...?

### ...para dar de alta un proveedor?

Desde la **persona** o la **sociedad** (*Crear proveedor*), o **Crear** aquí. También desde una eFactura (*Importar Proveedor* en
*Facturas recibidas*).

### ...para que sus facturas se rellenen solas?

En el proveedor indique: **CG propuesto**, **Tipo de factura**, **Naturaleza**, **Unidad**, **Concepto**, **Base Imponible propuesta**,
**Iva**, **Irpf** y el **Modo de pago** (con **Tarjeta bancaria** o **Domiciliada en** si procede).

### ...para usar una tarifa de precios del proveedor?

Marque **Usa tarifa** e **Importar tarifa** (menú de la edición): se carga la tarifa del proveedor, que se ve en su apartado de tarifas y se
usa en los pedidos.

### ...para pagarle por transferencia o remesa?

Añada sus **cuentas bancarias** en el apartado de cuentas (la de ingreso se propone en los pagos).

### ...para dar de baja un proveedor?

Marque **Baja** (al solicitar un pedido a un proveedor de baja se avisa).

---

#### Detalle técnico

- Controlador: `ProveedoresController` (Controllers/Terceros). Vista: `Views/Terceros/CrudProveedores.cshtml`. Dto: `ProveedorDto`.
- Descriptor: `DescriptorDeProveedores`: cuentas, tarifas y modal *Importar la tarifa del proveedor*.
- ts: `wwwroot/ts/Terceros/Proveedores.ts`.
