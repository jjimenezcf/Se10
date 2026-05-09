CREATE SEQUENCE dbo.CodigoContableProveedorSeq
    AS INT
    START WITH 1
    INCREMENT BY 1
    MINVALUE 1
    MAXVALUE 9999
    CYCLE
    CACHE 10;
go

CREATE or alter PROCEDURE dbo.AsignarCodigoContableProveerdor
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE TERCEROS.PROVEEDOR
    SET CODIGO_CONTABLE = NEXT VALUE FOR dbo.CodigoContableProveedorSeq
    WHERE CODIGO_CONTABLE IS NULL;
END
go

EXEC dbo.AsignarCodigoContableProveerdor;
go

DROP SEQUENCE dbo.CodigoContableProveedorSeq;
go

CREATE SEQUENCE dbo.CodigoContableClienteSeq
    AS INT
    START WITH 1
    INCREMENT BY 1
    MINVALUE 1
    MAXVALUE 9999
    CYCLE
    CACHE 10;
go

CREATE or alter PROCEDURE dbo.AsignarCodigoContableProveerdor
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE TERCEROS.Cliente
    SET CODIGO_CONTABLE = NEXT VALUE FOR dbo.CodigoContableClienteSeq
    WHERE CODIGO_CONTABLE IS NULL;
END
go

EXEC dbo.AsignarCodigoContableProveerdor;
go

DROP SEQUENCE dbo.CodigoContableClienteSeq;
go

select codigo_contable, * from TERCEROS.PROVEEDOR
select codigo_contable, * from TERCEROS.CLIENTE