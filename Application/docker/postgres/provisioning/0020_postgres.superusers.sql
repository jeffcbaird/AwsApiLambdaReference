SELECT
    'CREATE ROLE "postgres_g_supers" WITH SUPERUSER NOLOGIN'
WHERE
    NOT EXISTS(SELECT FROM pg_catalog.pg_roles WHERE rolname = 'postgres_g_supers')
\gexec