-- Switch to the schema owner role so the table is owned by reference_g_schemaowners.
-- This triggers the ALTER DEFAULT PRIVILEGES already defined in provisioning,
-- which automatically grants SELECT/INSERT/UPDATE/DELETE to reference_g_readwrite
-- and SELECT to reference_g_read — without needing explicit GRANTs here.
SET ROLE reference_g_schemaowners;

CREATE TABLE reference.items
(
    id          TEXT         NOT NULL,
    name        VARCHAR(100) NOT NULL,
    description VARCHAR(500) NULL,
    created_at  TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    updated_at  TIMESTAMPTZ  NULL,
    CONSTRAINT pk_items PRIMARY KEY (id)
);

RESET ROLE;
