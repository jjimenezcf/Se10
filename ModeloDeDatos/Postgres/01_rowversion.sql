-- Equivalente a la columna ROWVERSION de SQL Server (control de concurrencia optimista).
-- El modelo de EF declara RowVersion como byte[] y IsRowVersion(); en PostgreSQL la columna es bytea
-- y la rellena este trigger con un contador global creciente de 8 bytes, igual que hace SQL Server.
-- Ejecutar DESPUÉS de crear las tablas (migración) y cada vez que se añada una tabla con RowVersion.

CREATE SEQUENCE IF NOT EXISTS entorno.rowversion_seq;

CREATE OR REPLACE FUNCTION entorno.fn_rowversion() RETURNS trigger AS $$
BEGIN
    NEW.rowversion := int8send(nextval('entorno.rowversion_seq'));
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

DO $$
DECLARE
    t record;
BEGIN
    FOR t IN
        SELECT c.table_schema, c.table_name
        FROM information_schema.columns c
        JOIN information_schema.tables tb
          ON tb.table_schema = c.table_schema AND tb.table_name = c.table_name AND tb.table_type = 'BASE TABLE'
        WHERE c.column_name = 'rowversion'
    LOOP
        EXECUTE format('DROP TRIGGER IF EXISTS trg_rowversion ON %I.%I', t.table_schema, t.table_name);
        EXECUTE format('CREATE TRIGGER trg_rowversion BEFORE INSERT OR UPDATE ON %I.%I FOR EACH ROW EXECUTE FUNCTION entorno.fn_rowversion()',
                       t.table_schema, t.table_name);
    END LOOP;
END $$;
