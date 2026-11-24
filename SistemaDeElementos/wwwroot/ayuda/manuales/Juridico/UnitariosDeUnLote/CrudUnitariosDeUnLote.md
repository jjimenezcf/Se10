# Unitarios de un lote

Lista de los **unitarios** (materiales o servicios) de un **lote de un contrato**, con el precio de **Coste** y de **Venta** acordados
para ese lote. Se abre desde el lote y está filtrada por él (filtro **Lote**).

## ¿Cómo hago...?

### ...para añadir un unitario al lote?

**Crear**: elija el **Unitario** e indique *Coste* y *Venta*. Necesita permiso de gestor sobre el lote.

### ...para buscar un unitario?

Filtro **Unitario** (busca por el nombre del unitario).

---

#### Detalle técnico

- Controlador: `UnitariosDeUnLoteController` (Controllers/Juridico). Vista: `Views/Juridico/CrudUnitariosDeUnLote.cshtml`. Dto:
  `UnitariosDeUnLoteDto`.
- Descriptor: `DescriptorDeUnitariosDeUnLote`: restrictor por lote y filtro *Unitario*.
- ts: `wwwroot/ts/Juridico/UnitariosDeUnLote.ts` (sin comportamiento propio relevante).
