# Parámetros de negocio

**Parámetros propios de cada negocio**: nombre, descripción y **valor**. Configuran su comportamiento: las **etapas** (qué estados son, por
ejemplo, *prefactura* o *de cobro*), plazos (`REM_DiasDeEsperaDeCargo`), validaciones (`FAR_PermitirFacturasNegativas`,
`PAG_Validar_Justificante_De_Pago`), tipos usados por el sistema (`CAD_Tipo_Para_Lote_de_Preasientos`), prompts de la IA... Los manuales
de cada vista nombran los parámetros que les afectan.

## ¿Cómo hago...?

### ...para cambiar un comportamiento de un negocio?

Busque el parámetro (por negocio y nombre) y cambie su **valor**. Afecta a todo el negocio: hágalo con cuidado. Muchos se crean solos con
su valor por defecto la primera vez que se usan.

---

#### Detalle técnico

- Controlador: `ParametrosDeNegocioController` (Controllers/Negocio). Vista: `Views/Negocio/CrudDeParametrosDeNegocio.cshtml`. Dto:
  `ParametroDeNegocioDto`. Descriptor: `DescriptorDeParametrosDeNegocio`.
