USE [AB_ALBERTO_MARTINEZ]
GO
/****** Object:  StoredProcedure [NEGOCIO].[Plt_De_Negocio_Factura_de_Word_III] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Versión III de Plt_De_Negocio_Factura_de_Word: NO tiene ni una sola línea de SQL
-- específica de "factura". Todo lo que antes traía este PA a mano (cliente, sociedad
-- emisora, líneas de factura) ahora sale solo de etiquetas predefinidas:
--   - {{{FacturaEmt.*}}}                -> datos del propio elemento (automático, ya existía)
--   - {{{maestro.cliente.*}}}           -> Cliente de la factura (IUsaCliente)          [nuevo]
--   - {{{maestro.MiSociedad.*}}}        -> Sociedad de su Centro Gestor (IUsaCg)         [nuevo]
--   - {{{maestro.MiCg.*}}}              -> Centro Gestor de la factura (IUsaCg)          [nuevo]
--   - {{{LineaDeUnaFae}}} / {{{Concepto}}} / {{{BaseImponible}}}
--                                       -> líneas de la factura como "detalle" automático (ya existía,
--                                          LineaDeUnaFaeDtm ya es un IDetalle de FacturaEmt)
--
-- Lo único que queda aquí es boilerplate: el alias "documento" (idéntico en todos los PA de
-- plantillas del sistema) y una tabla "de relleno" sin usar, porque el descriptor exige que
-- pltDatosDePlantilla y pltFilasDeTabla tengan al menos un bloque bien formado -- no admiten
-- cadena vacía (ver ExtensorDePlantillas.MapearCampoDatos/MapearCampoFilas). No aporta ningún
-- dato de negocio: si el día de mañana se permite dejarlos vacíos, este PA se podría reducir
-- solo al descriptor.
ALTER PROCEDURE [NEGOCIO].[Plt_De_Negocio_Factura_de_Word_III]
  @IdNegocio INT,
  @IdElemento INT
AS
BEGIN
    select tipo, dato
    from (
          select 0 as orden, 'pltFormulasDePlantilla' as tipo, RTRIM(-1) as dato
          union
          select 1 as orden, 'pltDatosDePlantilla' as tipo, '1:documento' as dato
          union
          select 2 as orden, 'pltFilasDeTabla' as tipo, '2:tablasinuso' as dato
          union
          select 3 as orden, 'pltMapeosDeTabla' as tipo, 'tablasinuso:col0=col0' as dato)
       as descriptor
    order by orden

    -- 1) {{{documento.*}}}: boilerplate genérico, igual en cualquier plantilla del sistema
    select 'FACTURA' as titulo, 'DateTime.Now' as impresaEl

    -- 2) tabla de relleno, no referenciada en el Word -- ver comentario de cabecera
    select 'x' as col0

END
GO
