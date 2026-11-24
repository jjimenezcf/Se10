# Gestión de bancos

Catálogo de **entidades bancarias**: nombre, **Bic/Swift**, **código de la entidad** e **ISO** (país). Se usa para identificar el banco de las
cuentas bancarias (de sociedades, clientes, proveedores, trabajadores) y en los ficheros SEPA de las remesas.

## ¿Cómo hago...?

### ...para dar de alta un banco?

**Crear**: nombre, BIC/Swift, código de entidad (los 4 dígitos del IBAN tras el control) e ISO del país.

---

#### Detalle técnico

- Controlador: `BancosController` (Controllers/Terceros). Vista: `Views/Terceros/CrudBancos.cshtml`. Dto: `BancoDto`.
- Descriptor: `DescriptorDeBancos`. ts: `wwwroot/ts/Terceros/Bancos.ts`.
