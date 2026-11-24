# Gestión de sociedades

Una sociedad es una **persona jurídica**: razón social, **NIF**, código fiscal, correo, teléfono y logo. Hay dos usos:

- **Sociedades gestionadas** (las de la organización): tienen centros gestores, cuentas y tarjetas bancarias, buzones de correo,
  certificado digital, información contable (contabilidad, último cierre), parámetros propios y **facturadores**; emiten facturas y se
  comunican con la AEAT.
- **Terceros** (clientes, proveedores...): a partir de la sociedad se crean sus roles.

## ¿Cómo hago...?

### ...para dar de alta un cliente o proveedor que es una empresa?

**Crear** la sociedad y, en la edición, **Crear cliente**, **Crear proveedor**, **Crear interlocutor**, **Crear procurador** o **Crear abogado**.

### ...para configurar una sociedad gestionada?

En la edición y su menú:

- **Centros Gestores** (y *Editar centros gestores*), **Cuentas bancarias**, **Tarjetas bancarias**: ampliaciones de la sociedad.
- **Asociar certificado**: certificado digital para firmar (facturas, pedidos) y comunicarse con la AEAT.
- **Facturador**: facturadores por centro gestor y tipo de factura (*Facturadores*).
- Ampliaciones de **buzones** (correo), **información contable** y **parámetros** de la sociedad.
- **Activar verifactu** (administrador): activa la comunicación Verifactu de la sociedad.
- **Catálogos judiciales** (administrador): descarga los catálogos judiciales.
- **Gestionar interlocutores**.
- **Agenda de sociedad**.

### ...para dar de baja una sociedad?

Marque **Baja**. Mientras se ejecuta su cierre contable no se puede modificar.

---

#### Detalle técnico

- Controlador: `SociedadesController` (Controllers/Terceros). Vista: `Views/Terceros/CrudSociedades.cshtml`. Dto: `SociedadDto`.
- Descriptor: `DescriptorDeSociedades`: ampliaciones de buzones, cuentas, tarjetas, información contable, parámetros y facturadores;
  opciones *Asociar certificado*, *Centros Gestores*, *Cuentas/Tarjetas bancarias*, *Facturador*, *Activar verifactu*, *Catálogos judiciales*.
- ts: `wwwroot/ts/Terceros/Sociedades.ts`.
