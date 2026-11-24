# Gestión de unitarios

Un **unitario** es un **material o servicio** del catálogo: nombre, **referencia**, **naturaleza** (que da sus cuentas contables), **unidad**,
**clase**, descripción, **coste** y **precio de venta**, y la **referencia en la tarifa del proveedor**. Se usa en las líneas de presupuestos,
partes de trabajo, pedidos, documentos de venta, facturas y almacenes.

## ¿Cómo hago...?

### ...para dar de alta materiales en bloque?

Menú → **Importar catálogo** (*Seleccionar catálogo a importar*).

### ...para cargar precios de un proveedor?

Menú → **Importar tarifa** (*Importar la tarifa de un proveedor*): se cargan sus precios, que se ven en el apartado de **tarifas** del
unitario. Con **proponer referencia** se propone la referencia de la tarifa.

### ...para retirar un unitario?

Marque **Baja**.

---

#### Detalle técnico

- Controlador: `UnitariosController` (Controllers/MaestrosTecnicos). Vista: `Views/MaestrosTecnico/CrudUnitarios.cshtml`. Dto:
  `UnitarioDto`. Descriptor: `DescriptorDeUnitarios` (tarifas; modales de importar catálogo y tarifa).
