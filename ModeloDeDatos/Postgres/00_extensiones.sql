-- Extensiones que necesita la aplicación en PostgreSQL
-- pgcrypto: cifrado de passwords (equivale a ENCRYPTBYPASSPHRASE / DECRYPTBYPASSPHRASE de SQL Server)
CREATE EXTENSION IF NOT EXISTS pgcrypto;
