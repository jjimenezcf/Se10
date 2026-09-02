-- Ejecutar contra la BD de negocio afectada, DESPUÉS de haber creado CALLEJERO.BorrarMunicipio
-- (ver "BorrarMunicipio.sql"). Limpieza de municipios legacy/incorrectos de Murcia (CPRO 30)
-- detectados con la SELECT de LEN(dc) > 4 (y el caso de 'Molina', con dc de 1 solo carácter).

EXEC CALLEJERO.BorrarMunicipio
     @b_cpro = '30', @b_cmuni_dc = '30860', @b_nombre = 'Puerto de Mazarrón',
     @e_cpro = '30', @e_cmuni_dc = '0269';

EXEC CALLEJERO.BorrarMunicipio
     @b_cpro = '30', @b_cmuni_dc = '30565', @b_nombre = 'LAS TORRES DE COTILLAS',
     @e_cpro = '30', @e_cmuni_dc = '0389';

EXEC CALLEJERO.BorrarMunicipio
     @b_cpro = '30', @b_cmuni_dc = '30710', @b_nombre = 'LOS ALCAZARES',
     @e_cpro = '30', @e_cmuni_dc = '9027';

EXEC CALLEJERO.BorrarMunicipio
     @b_cpro = '30', @b_cmuni_dc = '30150', @b_nombre = 'Alberca de las Torres',
     @e_cpro = '30', @e_cmuni_dc = '0308';

EXEC CALLEJERO.BorrarMunicipio
     @b_cpro = '30', @b_cmuni_dc = '30430', @b_nombre = 'Cehegin',
     @e_cpro = '30', @e_cmuni_dc = '0177';

EXEC CALLEJERO.BorrarMunicipio
     @b_cpro = '30', @b_cmuni_dc = '30508', @b_nombre = 'Ribera de Molina',
     @e_cpro = '30', @e_cmuni_dc = '0275';

EXEC CALLEJERO.BorrarMunicipio
     @b_cpro = '30', @b_cmuni_dc = '5', @b_nombre = 'Molina',
     @e_cpro = '30', @e_cmuni_dc = '0275';
