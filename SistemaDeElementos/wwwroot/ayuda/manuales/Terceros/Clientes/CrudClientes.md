# Gestión de clientes

Un cliente es una persona o sociedad **a la que se vende**: presupuestos, partes de trabajo, documentos de venta, facturas emitidas,
contratos de venta y expedientes. Guarda su correo, teléfono, **cuenta contable** (*Cuenta* / *Ext.Cta*) y, si es extranjero, el **VAT**.

## ¿Cómo hago...?

### ...para dar de alta un cliente?

Desde la **persona** o la **sociedad** (*Crear cliente*), o **Crear** aquí eligiendo el interlocutor.

### ...para preparar un cliente para facturar?

- **Centro administrativo** (*Añadir Centro Administrativo*): dirección fiscal / centros para la facturación electrónica. Ha de estar activo
  para emitir.
- **Cuentas** del cliente: la cuenta bancaria de pago es necesaria para cobrarle por **remesa**.
- **Validar Nif en AEAT**: comprueba en la AEAT que NIF y nombre coinciden (lo exige la emisión).

### ...para dar acceso web al cliente?

**Añadir Puesto de trabajo** y **Añadir Usuario web**: puestos y usuarios del cliente (para que consulte sus documentos). Se ven en sus
apartados de la edición, junto con sus **expedientes**.

### ...para dar de baja un cliente?

Marque **Baja**.

---

#### Detalle técnico

- Controlador: `ClientesController` (Controllers/Terceros). Vista: `Views/Terceros/CrudClientes.cshtml`. Dto: `ClienteDto`.
- Descriptor: `DescriptorDeClientes`: ampliaciones de centro administrativo, cuentas, expedientes, puestos y usuarios; opciones de
  añadir y *Validar Nif en AEAT*.
- ts: `wwwroot/ts/Terceros/Clientes.ts`.
