USE master
GO

CREATE OR ALTER PROCEDURE dbo.ObtenerValorVariable
    @NombreVariable NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;

    CREATE TABLE #Results (
        DatabaseName NVARCHAR(255),
        Nombre NVARCHAR(255),
        Valor NVARCHAR(MAX)
    )

    DECLARE @SQL NVARCHAR(MAX);
    DECLARE @DatabaseName NVARCHAR(255);

    DECLARE db_cursor CURSOR FOR 
    SELECT name
    FROM sys.databases
    WHERE state_desc = 'ONLINE'
    AND (name LIKE 'SE_%' or name LIKE 'AB_%')
    AND name NOT IN ('master', 'tempdb', 'model', 'msdb')

    OPEN db_cursor;
    FETCH NEXT FROM db_cursor INTO @DatabaseName;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @SQL = N'
        USE [' + @DatabaseName + '];
        IF OBJECT_ID(''ENTORNO.VARIABLE'') IS NOT NULL 
        BEGIN
            INSERT INTO #Results (DatabaseName, Nombre, Valor)
            SELECT ''' + @DatabaseName + ''', @NombreVariable, VALOR
            FROM ENTORNO.VARIABLE
            WHERE NOMBRE = @NombreVariable;
        END';

        EXEC sp_executesql @SQL, N'@NombreVariable NVARCHAR(255)', @NombreVariable;

        FETCH NEXT FROM db_cursor INTO @DatabaseName;
    END

    CLOSE db_cursor;
    DEALLOCATE db_cursor;

    SELECT DatabaseName, Nombre, COALESCE(Valor, 'N/A') AS Valor
    FROM #Results;

    DROP TABLE #Results;
END
GO