
/****** Object:  StoredProcedure [VENTA].[Plt_Por_Tipo_Anexo_de_factura_de_desarrollo]    Script Date: 3/17/2024 10:08:49 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

create or ALTER PROCEDURE [VENTA].[Plt_Por_Tipo_Anexo_de_factura_de_desarrollo]
  @IdNegocio INT,
  @IdElemento INT
AS
BEGIN   
    -- Descriptor de información para una plantilla
    select tipo, dato
    from (
          select 0 as orden, 'pltFormulasDePlantilla' as tipo, RTRIM(-1) as dato
          union
          select 1 as orden, 'pltDatosDePlantilla' as tipo, '1: Sociedad, 2:totalhoras, 3:documento' as dato
          union
          select 2 as orden, 'pltFilasDeTabla' as tipo, '4:TotalDeHorasPorClase' as dato
          union
          select 3 as orden, 'pltMapeosDeTabla' as tipo, 'TotalDeHorasPorClase:col0=ClaseDeTarea,col1=TotalHoras' as dato)
       as descriptor
    order by orden
	
    -- Consulta para obtener los datos de la sociedad facturadora --> Sociedad
    select top(1)  
        t0.NOMBRE as nombre
      , t6.NOMBRE + ' N:' + rtrim(t1.NUMERO ) as direccion
      , t5.NOMBRE + ' - ' + t7.CP  as Municipio
      , t4.NOMBRE + ' (' + t3.NOMBRE +')' as Provincia
	  , t0.TELEFONO as telefono
    from TERCEROS.SOCIEDAD as t0
    inner join TERCEROS.SOCIEDAD_DIRECCION t1 on t1.ID_ELEMENTO = t0.ID
    inner join TERCEROS.CENTRO_GESTOR CG on CG.ID_SOCIEDAD = t0.ID
    inner join VENTA.FACTURA_EMT t2 on t2.ID_CG = CG.ID
    inner join CALLEJERO.PAIS t3 on t3.id = t1.ID_PAIS
    inner join CALLEJERO.PROVINCIA t4 on t4.id = t1.ID_PROVINCIA
    inner join CALLEJERO.MUNICIPIO t5 on t5.id = t1.ID_MUNICIPIO
    inner join CALLEJERO.CALLE t6 on t6.id = t1.ID_CALLE
    inner join CALLEJERO.CODIGO_POSTAL t7 on t7.id = t1.ID_CP
    where t2.id = @IdElemento
     and t1.CALIFICADOR = 'fiscal'
     and t1.ACTIVO = 1

	--totalhoras: Consulta de total horas de facturas
	SELECT CONVERT(DECIMAL(10,2),SUM(CASE WHEN t3.Medido_en = 'Jornadas' THEN t3.Valor * 8 
		            WHEN t3.Medido_en = 'Dias' THEN t3.Valor * 24 
		            WHEN t3.Medido_en = 'Minutos' THEN t3.Valor / 60 
                       ELSE t3.Valor 
                  END)) AS horas
    FROM TAREA.TAREA t1
    INNER JOIN TAREA.TAREA_PANIFICACION t3 ON t1.id = t3.ID_ELEMENTO
    inner join NEGOCIO.CLASE t4 on t4.id = t1.ID_CLASE_ELEMENTO
    Where t1.ID_FACTURA_EMT = @IdElemento

    
    -- Consulta para obtener los datos del documento
    select (select 'Anexo: ' + REFERENCIA from VENTA.FACTURA_EMT where ID = @IdElemento) as titulo, 'DateTime.Now' as impresaEl
    
    -- Consulta para obtener tablas: TotalDeHorasPorClase --> Agrupación de horas por clase de tarea
	SELECT t1.ID_CLASE_ELEMENTO AS id_Clase_Tarea
	, t4.NOMBRE as ClaseDeTarea
    , CONVERT(DECIMAL(10,2),SUM(CASE WHEN t3.Medido_en = 'Jornadas' THEN t3.Valor * 8 
			            WHEN t3.Medido_en = 'Dias' THEN t3.Valor * 24 
			            WHEN t3.Medido_en = 'Minutos' THEN t3.Valor / 60 
                        ELSE t3.Valor 
                   END)) AS TotalHoras
    FROM TAREA.TAREA t1
    INNER JOIN TAREA.TAREA_PANIFICACION t3 ON t1.id = t3.ID_ELEMENTO
	inner join NEGOCIO.CLASE t4 on t4.id = t1.ID_CLASE_ELEMENTO
	Where t1.ID_FACTURA_EMT = @IdElemento
    GROUP BY t1.ID_CLASE_ELEMENTO, t4.NOMBRE
	order by t4.NOMBRE

END

