-- 1) Vista previa: cuántas naturalezas se van a actualizar y con qué clase
SELECT
    n.ID,
    n.NOMBRE,
    n.CLASE          AS ClaseActual,
    x.CLASE          AS ClaseAProponer
FROM MT.MT_NATURALEZA n
CROSS APPLY (
    SELECT TOP 1 u.CLASE
    FROM MT.UNITARIO u
    WHERE u.ID_NATURALEZA = n.ID
) x
WHERE n.CLASE IS NULL;

-- 2) Backfill: solo toca las naturalezas que aún no tienen clase
BEGIN TRAN;

UPDATE n
SET n.CLASE = x.CLASE
FROM MT.MT_NATURALEZA n
CROSS APPLY (
    SELECT TOP 1 u.CLASE
    FROM MT.UNITARIO u
    WHERE u.ID_NATURALEZA = n.ID
) x
WHERE n.CLASE IS NULL;

-- revisa el número de filas afectadas y, si es el esperado:
COMMIT TRAN;
-- si algo no cuadra:
-- ROLLBACK TRAN;


update mt.MT_NATURALEZA set clase = 'Servicio' where clase is null and sigla <> 'MAT'
update mt.MT_NATURALEZA set clase = 'Material' where clase is null and sigla = 'MAT'

select * from mt.MT_NATURALEZA where clase is null