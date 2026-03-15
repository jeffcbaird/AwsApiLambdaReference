DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_catalog.pg_roles
        WHERE  rolname = 'reference_deployuser') 
    THEN
        RAISE NOTICE 'Creating role [reference_deployuser]...';
        CREATE ROLE reference_deployuser LOGIN PASSWORD 'LocalDeployPass' IN ROLE reference_g_schemaowners;
    ELSE
        RAISE NOTICE 'Role [reference_deployuser] was created previously.';
    END IF;
END $$;