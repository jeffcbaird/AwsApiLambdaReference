DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_catalog.pg_roles
        WHERE  rolname = 'reference_app') 
    THEN
        RAISE NOTICE 'Creating role [reference_app]...';
        CREATE ROLE reference_app WITH LOGIN PASSWORD 'LocalDockerPass' IN GROUP reference_g_readwrite;
    ELSE
        RAISE NOTICE 'Role [reference_app] was created previously.';
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_catalog.pg_roles
        WHERE  rolname = 'reference_app_iam') 
    THEN
        RAISE NOTICE 'Creating role [reference_app_iam]...';
        CREATE ROLE reference_app_iam WITH LOGIN;
    ELSE
        RAISE NOTICE 'Role [reference_app_iam] was created previously.';
    END IF;
END $$;

GRANT reference_g_readwrite TO reference_app_iam;

