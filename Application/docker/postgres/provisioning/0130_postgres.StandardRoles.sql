DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_catalog.pg_roles
        WHERE  rolname = 'reference_g_read') 
    THEN
        RAISE NOTICE 'Creating role [reference_g_read]...';
        CREATE ROLE reference_g_read WITH NOLOGIN;
    ELSE
        RAISE NOTICE 'Role [reference_g_read] was created previously.';
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_catalog.pg_roles
        WHERE  rolname = 'reference_g_readwrite') 
    THEN
        RAISE NOTICE 'Creating role [reference_g_readwrite]...';
        CREATE ROLE reference_g_readwrite WITH NOLOGIN;
    ELSE
        RAISE NOTICE 'Role [reference_g_readwrite] was created previously.';
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_catalog.pg_roles
        WHERE  rolname = 'reference_externaldata_g_etl') 
    THEN
        RAISE NOTICE 'Creating role [reference_externaldata_g_etl]...';
        CREATE ROLE reference_externaldata_g_etl WITH NOLOGIN;
    ELSE
        RAISE NOTICE 'Role [reference_externaldata_g_etl] was created previously.';
    END IF;
END $$;

GRANT CONNECT, TEMPORARY ON DATABASE reference TO reference_g_read;
GRANT CONNECT, TEMPORARY ON DATABASE reference TO reference_g_readwrite;
GRANT CONNECT, TEMPORARY ON DATABASE reference TO reference_externaldata_g_etl;

GRANT USAGE ON SCHEMA
		reference_api,
		external_data,
		reference,
		deployer_schema
	TO
		reference_g_read,
		reference_g_readwrite;

GRANT USAGE ON SCHEMA
		external_data
	TO
		reference_externaldata_g_etl;

ALTER DEFAULT PRIVILEGES FOR ROLE reference_g_schemaowners 
	IN SCHEMA 
		reference_api,
		external_data,
		reference
    GRANT SELECT ON TABLES TO reference_g_read;

ALTER DEFAULT PRIVILEGES FOR ROLE reference_g_schemaowners 
	IN SCHEMA 
		reference_api,
		external_data,
		reference
    GRANT USAGE, SELECT ON SEQUENCES TO reference_g_read;

ALTER DEFAULT PRIVILEGES FOR ROLE reference_g_schemaowners 
	IN SCHEMA 
		reference_api,
		external_data,
		reference
    GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO reference_g_readwrite;

ALTER DEFAULT PRIVILEGES FOR ROLE reference_g_schemaowners 
	IN SCHEMA 
		reference_api,
		external_data,
		reference
    GRANT USAGE, SELECT, UPDATE ON SEQUENCES TO reference_g_readwrite;

ALTER DEFAULT PRIVILEGES FOR ROLE reference_g_schemaowners 
	IN SCHEMA 
		external_data
    GRANT USAGE, SELECT, UPDATE ON SEQUENCES TO reference_externaldata_g_etl;