CREATE OR ALTER PROCEDURE [dbo].[Bloquear_Cola]
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @idNegocio int = (SELECT TOP 1 id FROM NEGOCIO.NEGOCIO WHERE nombre LIKE 'Variables');
    DECLARE @idElemento int = 0; -- (SELECT id FROM entorno.VARIABLE WHERE nombre LIKE 'CFG_Cola_En_Ejecucion');
    DECLARE @idProceso UNIQUEIDENTIFIER = NEWID();
    DECLARE @Operacion varchar(4) = 'BLOC';
    DECLARE @login varchar(50) = 'admin.se';
    DECLARE @anotacion varchar(255) = 'Bloquear cola para despliegue';
    DECLARE @fecha datetime = GETDATE();

    BEGIN TRY
        INSERT INTO [NEGOCIO].[SEMAFORO_PROCESO]
               ([ID_NEGOCIO]
               ,[ID_ELEMENTO]
               ,[ID_PROCESO]
               ,[OPERACION]
               ,[LOGIN]
               ,[ANOTACION]
               ,[FECHA])
         VALUES
               (@idNegocio
               ,@idElemento
               ,@idProceso
               ,@Operacion
               ,@login
               ,@anotacion
               ,@fecha)

        SELECT 'Cola de la BD [' + DB_NAME() + '] parada' AS Result
    END TRY
    BEGIN CATCH
        SELECT 'ERROR: No se ha podido para la cola en ' + DB_NAME() AS Result
    END CATCH
END
go

CREATE OR ALTER PROCEDURE [dbo].[Desbloquear_Cola]
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @idNegocio int = (SELECT TOP 1 id FROM NEGOCIO.NEGOCIO WHERE nombre LIKE 'Variables');
    DECLARE @idElemento int = 0; -- (SELECT id FROM entorno.VARIABLE WHERE nombre LIKE 'CFG_Cola_En_Ejecucion');
    DECLARE @Operacion varchar(4) = 'BLOC';

    BEGIN TRY
        delete from [NEGOCIO].[SEMAFORO_PROCESO] where id_negocio = @idNegocio and ID_ELEMENTO = @idElemento and OPERACION = @Operacion
        SELECT 'Cola de la BD [' + DB_NAME() + '] arrancada' AS Result
    END TRY
    BEGIN CATCH
        SELECT 'ERROR: No se ha podido arrancar la cola en ' + DB_NAME() AS Result
    END CATCH
END
go