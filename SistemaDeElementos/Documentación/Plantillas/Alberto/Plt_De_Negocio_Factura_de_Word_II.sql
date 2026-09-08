USE [AB_ALBERTO_MARTINEZ]
GO
/****** Object:  StoredProcedure [NEGOCIO].[Plt_De_Negocio_Factura_de_Word_II] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Versión II de Plt_De_Negocio_Factura_de_Word: usa el nuevo mecanismo de "maestros"
-- ({{{maestro.cliente....}}}) para el NIF y la dirección del cliente, en vez de traerlos
-- explícitamente aquí. Compárese con Plt_De_Negocio_Factura_de_Word.sql: ha desaparecido
-- por completo la consulta "cliente" (nif + dirección) — ya no hace falta, porque
-- FacturaEmtDtm implementa IUsaCliente y DatosMaestros la resuelve sola.
ALTER PROCEDURE [NEGOCIO].[Plt_De_Negocio_Factura_de_Word_II]
  @IdNegocio INT,
  @IdElemento INT
AS
BEGIN
    -- Descriptor de información para la plantilla.
    select tipo, dato
    from (
          select 0 as orden, 'pltFormulasDePlantilla' as tipo, RTRIM(-1) as dato
          union
          select 1 as orden, 'pltDatosDePlantilla' as tipo, '1:sociedad,3:documento' as dato
          union
          select 2 as orden, 'pltFilasDeTabla' as tipo, '2:lineasdefactura' as dato
          union
          select 3 as orden, 'pltMapeosDeTabla' as tipo,
              'lineasdefactura:col0=Concepto,col1=BaseImponible'
              as dato)
       as descriptor
    order by orden

    -- 1) {{{sociedad.*}}}: datos del despacho emisor (nombre, NIF, dirección, contacto e IBAN de cobro)
    select top(1)
          t0.NOMBRE as nombre
        , t0.NIF as nif
        , t6.NOMBRE + ' Nº' + RTRIM(t1.NUMERO) + ISNULL(' ' + t1.PISO, '') + ' — ' + t7.CP + ' ' + t5.NOMBRE as direccion
        , t0.TELEFONO as telefono
        , t0.EMAIL as email
        , (select top(1)
                REPLACE(CONCAT(cb.ISO2, cb.DC_IBAN, ' ', cb.ENTIDAD, ' ', cb.OFICINA, ' ',
                                LEFT(cb.DC_CCC + cb.NUMERO, 4), ' ',
                                SUBSTRING(cb.DC_CCC + cb.NUMERO, 5, 4), ' ',
                                SUBSTRING(cb.DC_CCC + cb.NUMERO, 9, 4)), '  ', ' ')
           from TERCEROS.SOCIEDAD_CUENTA sc
           inner join CONTABILIDAD.CUENTA_BANCARIA cb on cb.ID = sc.ID_CUENTA
           where sc.ID_ELEMENTO = t0.ID
             and sc.ACTIVA = 1
             and sc.CLASE in ('Ingreso', 'Ambas')) as iban
        , p.PIE_DE_FACTURA as piedefactura
    from TERCEROS.SOCIEDAD t0
    inner join TERCEROS.SOCIEDAD_DIRECCION t1 on t1.ID_ELEMENTO = t0.ID
    inner join TERCEROS.CENTRO_GESTOR cg on cg.ID_SOCIEDAD = t0.ID
    inner join VENTA.FACTURA_EMT t2 on t2.ID_CG = cg.ID
    inner join CALLEJERO.PAIS t3 on t3.ID = t1.ID_PAIS
    inner join CALLEJERO.PROVINCIA t4 on t4.ID = t1.ID_PROVINCIA
    inner join CALLEJERO.MUNICIPIO t5 on t5.ID = t1.ID_MUNICIPIO
    inner join CALLEJERO.CALLE t6 on t6.ID = t1.ID_CALLE
    inner join CALLEJERO.CODIGO_POSTAL t7 on t7.ID = t1.ID_CP
    left join TERCEROS.SOCIEDAD_PARAMETRO p on p.ID_ELEMENTO = t0.ID
    where t2.ID = @IdElemento
      and t1.CALIFICADOR = 'fiscal'
      and t1.ACTIVO = 1

    -- 2) {{{lineasdefactura}}}: una fila por línea de factura (Concepto + Base imponible de la línea)
    select
          CONCEPTO as Concepto
        , FORMAT(ISNULL(CANTIDAD, 0) * ISNULL(PRECIO, 0), 'N2') + ' €' as BaseImponible
    from VENTA.FACTURA_EMT_LINEA
    where ID_ELEMENTO = @IdElemento
      and TIPO_LINEA != 'Comentario'
    order by ORDEN

    -- 3) {{{documento.*}}}: cabecera estándar del documento (no usada en el cuerpo de esta plantilla)
    select 'FACTURA' as titulo, 'DateTime.Now' as impresaEl

END
GO
