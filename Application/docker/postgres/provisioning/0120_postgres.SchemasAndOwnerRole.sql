DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_catalog.pg_roles
        WHERE  rolname = 'reference_g_schemaowners') 
    THEN
        RAISE NOTICE 'Creating role [reference_g_schemaowners]...';
        CREATE ROLE reference_g_schemaowners WITH NOLOGIN;
    ELSE
        RAISE NOTICE 'Role [reference_g_schemaowners] was created previously.';
    END IF;
END $$;

GRANT reference_g_schemaowners TO postgres_g_supers;

GRANT CONNECT, TEMPORARY ON DATABASE reference TO reference_g_schemaowners;

CREATE SCHEMA IF NOT EXISTS reference  AUTHORIZATION reference_g_schemaowners;
CREATE SCHEMA IF NOT EXISTS reference_api  AUTHORIZATION reference_g_schemaowners;
CREATE SCHEMA IF NOT EXISTS external_data  AUTHORIZATION reference_g_schemaowners;

CREATE SCHEMA IF NOT EXISTS deployer_schema AUTHORIZATION reference_g_schemaowners;
