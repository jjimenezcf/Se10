DECLARE @DatabaseName NVARCHAR(255)
DECLARE @SQL NVARCHAR(MAX)

DECLARE db_cursor CURSOR FOR
SELECT name
FROM sys.databases
WHERE state_desc = 'ONLINE'
AND (name LIKE 'SE_%' or name LIKE 'AB_%')
AND name NOT IN ('master', 'tempdb', 'model', 'msdb')

OPEN db_cursor
FETCH NEXT FROM db_cursor INTO @DatabaseName

WHILE @@FETCH_STATUS = 0
BEGIN
print @DatabaseName
    SET @SQL = '
    USE [' + @DatabaseName + '];
    update NEGOCIO.NEGOCIO set ES_DE_PARAMETRIZACION = 0 where	NOMBRE = ''Correos''
    '

    EXEC sp_executesql @SQL

    FETCH NEXT FROM db_cursor INTO @DatabaseName
END

CLOSE db_cursor
DEALLOCATE db_cursor