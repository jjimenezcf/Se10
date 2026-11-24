# Planificadores de ventas (de un contrato)

Un planificador de ventas define **qué se factura o se hace periódicamente** en un contrato de venta: sus **líneas** (unitarios,
cantidades y precios), cada cuánto (**Repetir cada** + **Periodicidad**), entre qué fechas (**Fecha de comienzo** / **Fecha de fin**) y qué
genera cada vez: un **parte de trabajo** (*Tipo del P.T. a generar*) o una **prefactura** (*Tipo de factura emitir*). A partir de él se
crean las **planificaciones de venta**, que en su fecha generan el parte o la prefactura.

Se abre desde el contrato y está filtrado por él (filtro **Contrato**). Los planificadores sólo se crean desde un contrato.

## ¿Cómo hago...?

### ...para planificar la facturación periódica de un contrato?

1. Desde el contrato (apartado de planificadores), **Crear**: nombre, **CG de la planificación**, **Tipo de planificación**, y el tipo de
   parte o de factura a generar. Si el contrato usa lotes, elija el **Lote**.
2. Indique las fechas de comienzo y fin y la repetición (p. ej. *1* + *mensual*).
3. Añada las **líneas** (sólo líneas de unitario o libres: **no se admiten partidas alzadas**). Se totalizan la *B.I.* y el *Total*.
4. **Generar planificaciones** (menú individual, gestor): crea las planificaciones de venta del planificador. La columna *Generado* indica
   si ya se generaron.

También se generan desde *Gestión de contratos* → **Generar planificaciones** (para todos los contratos vigentes, entre fechas).

### ...para ver lo planificado?

En **Gestión de planificaciones de venta**, filtre por *Planificador de ventas* o por *Contrato*.

---

#### Detalle técnico

- Controlador: `PlanificadorDeVentasController` (Controllers/Juridico). Vista: `Views/Juridico/CrudPlanificadorDeVentas.cshtml`.
  Dto: `PlanificadorDeVentaDto`.
- Descriptor: `DescriptorDePlanificadorDeVentas`: restrictor por contrato, filtro *Planificador*, ampliación de líneas, opción *Generar
  planificaciones*.
- Reglas: `GestorDeContratos` (generación) y el trabajo *Generar planificadores de un contrato* / *Motor de planificador*
  (`TrabajosDeContratos`).
- ts: `wwwroot/ts/Juridico/PlanificadorDeVentas.ts`: exige el contrato al crear y aplica el tipo de línea (rechaza partidas alzadas).
