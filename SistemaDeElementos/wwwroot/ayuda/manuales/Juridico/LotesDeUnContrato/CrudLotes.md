# Lotes de un contrato

Algunos contratos se dividen en **lotes**: partes del contrato con su propia vigencia (**Fecha de inicio** / **Fecha de fin**) y su
lista de **unitarios** con precio de **coste** y de **venta (PVP)** acordados. Los planificadores de ventas de un contrato con lotes
indican a qué lote pertenecen.

Se abre desde el contrato (apartado *Lotes*) y está filtrado por él (filtro **Contrato**); sólo se crean desde un contrato.

## ¿Cómo hago...?

### ...para dar de alta un lote?

Desde el contrato, **Crear**: nombre del lote, fechas de vigencia y si está **Activo**. Después, en el apartado de unitarios del lote,
añada los materiales o servicios con su coste y su precio de venta.

### ...para cambiar los precios de un lote?

Abra el lote y edite sus **unitarios** (*Coste*, *Venta*); también desde *Unitarios de un lote*.

### ...para dejar de usar un lote?

Desmarque **Activo** o ajuste su fecha de fin.

---

#### Detalle técnico

- Controlador: `LotesDeUnContratoController` (Controllers/Juridico). Vista: `Views/Juridico/CrudLotes.cshtml`. Dto:
  `LoteDeUnContratoDto`.
- Descriptor: `DescriptorDeLotesDeUnContrato`: restrictor por contrato, filtro *Lote*, ampliación de unitarios (*Unitario*, *Coste*,
  *PVP*).
- ts: `wwwroot/ts/Juridico/LotesDeUnContrato.ts`: exige el contrato al crear.
