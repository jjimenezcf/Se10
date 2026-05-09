CREATE or alter PROCEDURE VENTA.Plt_EmisionDeFactura
    @IdNegocio INT,
    @IdFactura INT
AS
BEGIN

     -- Descriptor de información para una plantilla
	 select tipo, dato
	 from (
        select 0 as orden, 'pltFormulasDePlantilla' as tipo, RTRIM(-1) as dato
        union
        select 1 as orden,'pltDatosDePlantilla' as tipo, '1:factura,2:cliente,3:sociedad, 6:documento, 7:cobrado' as dato
        union
        select 2 as orden, 'pltFilasDeTabla' as tipo, '4:lineasdefactura,5:Cobros' as dato
        union
        select 3 as orden, 'pltMapeosDeTabla' as tipo, 
	           'lineasdefactura:col0=Concepto,col1=Cantidad,col2=Unidad,col3=Precio,col4=Importe,col5=Iva,col6=Total' + '|' +
	           'cobros:col0=Fecha,col1=Importe,col2=Pendiente,col3=Resto' 
	    		as dato)
     as descriptor
	 order by orden

    -- Consulta para obtener los datos de la factura
       SELECT 
	     rtrim(ANO)+'-'+ SERIE +RTRIM(NUMERO) as numero
	   , FORMAT(FACTURADA_EL, 'dd-MM-yyyy') AS emitida
	   , FORMAT(VENCE_EL, 'dd-MM-yyyy') AS vence
	   , isNull(CLASE_RECTIFICATIVA, '') as clase
	   , format((select sum(isnull(precio,0) * isnull(cantidad,0)) from venta.FACTURA_EMT_LINEA where ID_ELEMENTO = @IdFactura and TIPO_LINEA != 'Comentario'), 'N2') as bi
	   , format((select sum(ISNULL(IVA, 0)) from venta.FACTURA_EMT_LINEA where ID_ELEMENTO = @IdFactura and TIPO_LINEA != 'Comentario'), 'N2') as iva
	   , format((select sum(isnull(precio,0) * isnull(cantidad,0)) from venta.FACTURA_EMT_LINEA where ID_ELEMENTO = @IdFactura and TIPO_LINEA != 'Comentario') +
	     (select sum(ISNULL(IVA, 0)) from venta.FACTURA_EMT_LINEA where ID_ELEMENTO = @IdFactura and TIPO_LINEA != 'Comentario'), 'N2')
		 as total
       FROM VENTA.FACTURA_EMT
       WHERE ID = @IdFactura

    -- Consulta para obtener los datos del cliente
    select top(1)  
            t0.NOMBRE as nombre
          , t6.NOMBRE + ' N:' + rtrim(t1.NUMERO ) as direccion
          , t5.NOMBRE + ' - ' + t7.CP  as Municipio
          , t4.NOMBRE + ' (' + t3.NOMBRE +')' as Provincia
        from TERCEROS.CLIENTE  as t0 
        inner join TERCEROS.cliente_DIRECCION t1 on t1.ID_ELEMENTO = t0.ID
        inner join VENTA.FACTURA_EMT t2 on t2.ID_CLIENTE = t1.ID
        inner join CALLEJERO.PAIS t3 on t3.id = t1.ID_PAIS
        inner join CALLEJERO.PROVINCIA t4 on t3.id = t1.ID_PROVINCIA
        inner join CALLEJERO.MUNICIPIO t5 on t5.id = t1.ID_MUNICIPIO
        inner join CALLEJERO.CALLE t6 on t6.id = t1.ID_CALLE
        inner join CALLEJERO.CODIGO_POSTAL t7 on t7.id = t1.ID_CP
        where t2.id = @IdFactura
         and t1.CALIFICADOR = 'fiscal'
         and t1.ACTIVO = 1 

    -- Consulta para obtener los datos de la sociedad facturadora
    select top(1)  
        t0.NOMBRE as nombre
      , t6.NOMBRE + ' N:' + rtrim(t1.NUMERO ) as direccion
      , t5.NOMBRE + ' - ' + t7.CP  as Municipio
      , t4.NOMBRE + ' (' + t3.NOMBRE +')' as Provincia
    from TERCEROS.SOCIEDAD as t0
    inner join TERCEROS.SOCIEDAD_DIRECCION t1 on t1.ID_ELEMENTO = t0.ID
    inner join TERCEROS.CENTRO_GESTOR CG on CG.ID_SOCIEDAD = t0.ID
    inner join VENTA.FACTURA_EMT t2 on t2.ID_CG = CG.ID
    inner join CALLEJERO.PAIS t3 on t3.id = t1.ID_PAIS
    inner join CALLEJERO.PROVINCIA t4 on t3.id = t1.ID_PROVINCIA
    inner join CALLEJERO.MUNICIPIO t5 on t5.id = t1.ID_MUNICIPIO
    inner join CALLEJERO.CALLE t6 on t6.id = t1.ID_CALLE
    inner join CALLEJERO.CODIGO_POSTAL t7 on t7.id = t1.ID_CP
    where t2.id = 5
     and t1.CALIFICADOR = 'fiscal'
     and t1.ACTIVO = 1

    -- Consulta para obtener el detalle de lo facturado
    SELECT 
	  CONCEPTO as Concepto
	, FORMAT(CANTIDAD, 'N2') as Cantidad
	, COALESCE((select sigla from mt.MT_UNIDAD where ID = isNUll(id_unidad,0)), '--') as unidad
	, FORMAT(PRECIO, 'N2') as Precio
	, FORMAT(CANTIDAD * PRECIO, 'N2') as Importe
	, FORMAT(IVA, 'N2') as Iva
	, FORMAT((CANTIDAD * PRECIO) + IVA, 'N2') as Total
    FROM VENTA.FACTURA_EMT_LINEA 
    WHERE ID_ELEMENTO = @IdFactura;

	    -- Consulta para obtener el detalle de lo cobrado
    SELECT 
	    FORMAT(COBRADO_EL, 'dd-MM-yyyy') AS fecha
	  , '--' as pendiente
	  , FORMAT(IMPORTE, 'N2') as Precio
	  , '--' as resto
    FROM VENTA.FACTURA_EMT_COBRO 
    WHERE ID_ELEMENTO = @IdFactura;

	-- Consulta cabecera del documento
	select 'Factura de prueba' as titulo, 'DateTime.Now' as impresa
		
	-- Consulta de lo cobrado
	select rtrim(isNUll(sum(IMPORTE),0)) as total from VENTA.FACTURA_EMT_COBRO where ID_ELEMENTO = @IdFactura


END