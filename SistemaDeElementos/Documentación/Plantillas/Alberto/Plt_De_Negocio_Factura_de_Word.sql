USE [AB_ALBERTO_MARTINEZ]
GO
/****** Object:  StoredProcedure [NEGOCIO].[Plt_De_Negocio_Factura_de_Word] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [NEGOCIO].[Plt_De_Negocio_Factura_de_Word]
  @IdNegocio INT,
  @IdElemento INT
AS
BEGIN
    -- Descriptor de información para la plantilla.
    -- Los índices de "pltDatosDePlantilla"/"pltFilasDeTabla" indican, en orden,
    -- cuál de los SELECT siguientes (después de este descriptor) alimenta cada alias/tabla.
    -- OJO: no hace falta traer aquí ningún dato que ya esté en FacturaEmtDto: esos se
    -- referencian directamente en el Word como {{{FacturaEmt.Campo}}} (p.ej. NumeroFactura,
    -- FacturadaEl, Cliente, TotalSinIva, TotalIva, APagar...), sin tocar este procedimiento.
    select tipo, dato
    from (
          select 0 as orden, 'pltFormulasDePlantilla' as tipo, RTRIM(-1) as dato
          union
          select 1 as orden, 'pltDatosDePlantilla' as tipo, '1:cliente,2:sociedad,4:documento' as dato
          union
          select 2 as orden, 'pltFilasDeTabla' as tipo, '3:lineasdefactura' as dato
          union
          select 3 as orden, 'pltMapeosDeTabla' as tipo,
              'lineasdefactura:col0=Concepto,col1=BaseImponible'
              as dato)
       as descriptor
    order by orden

    -- 1) {{{cliente.*}}}: lo único del cliente que NO viene ya en FacturaEmt (NIF y dirección fiscal)
    select top(1)
          t0.VAT as nif
        , t6.NOMBRE + ' Nº' + RTRIM(t1.NUMERO) + ' ' + t7.CP + ' ' + t5.NOMBRE as direccion
    from TERCEROS.CLIENTE t0
    inner join TERCEROS.CLIENTE_DIRECCION t1 on t1.ID_ELEMENTO = t0.ID
    inner join VENTA.FACTURA_EMT t2 on t2.ID_CLIENTE = t0.ID
    inner join CALLEJERO.PAIS t3 on t3.ID = t1.ID_PAIS
    inner join CALLEJERO.PROVINCIA t4 on t4.ID = t1.ID_PROVINCIA
    inner join CALLEJERO.MUNICIPIO t5 on t5.ID = t1.ID_MUNICIPIO
    inner join CALLEJERO.CALLE t6 on t6.ID = t1.ID_CALLE
    inner join CALLEJERO.CODIGO_POSTAL t7 on t7.ID = t1.ID_CP
    where t2.ID = @IdElemento
      and t1.CALIFICADOR = 'fiscal'
      and t1.ACTIVO = 1

    -- 2) {{{sociedad.*}}}: datos del despacho emisor (nombre, NIF, dirección, contacto e IBAN de cobro)
    select top(1)
          t0.NOMBRE as nombre
        , t0.NIF as nif
        , t6.NOMBRE + ' Nº' + RTRIM(t1.NUMERO) + ISNULL(' ' + t1.PISO, '') + ' — ' + t7.CP + ' ' + t5.NOMBRE as direccion
        , t0.TELEFONO as telefono
        , t0.EMAIL as email
        -- Cuenta bancaria activa de cobro del despacho (TERCEROS.SOCIEDAD_CUENTA + CONTABILIDAD.CUENTA_BANCARIA).
        -- Si tienes varias cuentas activas de clase 'Ingreso'/'Ambas', ajusta este TOP/ORDER a la que corresponda.
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
        -- Pie de factura configurado en "Mi sociedad" (aviso de protección de datos, despedida y firma).
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

    -- 3) {{{lineasdefactura}}}: una fila por línea de factura (Concepto + Base imponible de la línea)
    select
          CONCEPTO as Concepto
        , FORMAT(ISNULL(CANTIDAD, 0) * ISNULL(PRECIO, 0), 'N2') + ' €' as BaseImponible
    from VENTA.FACTURA_EMT_LINEA
    where ID_ELEMENTO = @IdElemento
      and TIPO_LINEA != 'Comentario'
    order by ORDEN

    -- 4) {{{documento.*}}}: cabecera estándar del documento (no usada en el cuerpo de esta plantilla,
    -- se deja por si se quiere añadir p.ej. "Impresa el: {{{documento.impresaEl}}}")
    select 'FACTURA' as titulo, 'DateTime.Now' as impresaEl

END
GO
