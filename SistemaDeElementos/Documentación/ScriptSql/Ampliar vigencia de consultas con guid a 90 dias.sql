/* ============================================================
   Ampliar a 90 días la vigencia de los enlaces de consulta
   Tabla: NEGOCIO.CONSULTA_CON_GUID
   Nueva caducidad = CREADO_EL + 90 días
   - Nunca acorta una caducidad (solo se actualiza si la nueva fecha es posterior).
   - Los enlaces que ya tienen 90 días o más no se tocan.
   ============================================================ */
SET NOCOUNT ON;

DECLARE @Dias             INT = 90;
DECLARE @IncluirCaducadas BIT = 0;   -- 0 = solo enlaces aún vigentes; 1 = reactivar también los ya caducados

-- 1) Vista previa: qué se va a cambiar
SELECT  ID, ID_NEGOCIO, ID_ELEMENTO, ID_USUARIO, CREADO_EL,
        CADUCA_EL                          AS CADUCA_ACTUAL,
        DATEADD(DAY, @Dias, CREADO_EL)     AS CADUCA_NUEVA,
        DATEDIFF(DAY, CREADO_EL, CADUCA_EL) AS DIAS_ACTUALES
FROM    NEGOCIO.CONSULTA_CON_GUID
WHERE   CADUCA_EL IS NOT NULL
  AND   DATEADD(DAY, @Dias, CREADO_EL) > CADUCA_EL
  AND   (@IncluirCaducadas = 1 OR CADUCA_EL > GETDATE())
ORDER BY ID_NEGOCIO, ID_ELEMENTO;

-- 2) Actualización (dentro de transacción)
BEGIN TRAN;

UPDATE  NEGOCIO.CONSULTA_CON_GUID
SET     CADUCA_EL = DATEADD(DAY, @Dias, CREADO_EL)
WHERE   CADUCA_EL IS NOT NULL
  AND   DATEADD(DAY, @Dias, CREADO_EL) > CADUCA_EL
  AND   (@IncluirCaducadas = 1 OR CADUCA_EL > GETDATE());

PRINT CONCAT('Filas actualizadas: ', @@ROWCOUNT);

-- Revisa el resultado y elige UNA de las dos líneas:
COMMIT TRAN;
-- ROLLBACK TRAN;
