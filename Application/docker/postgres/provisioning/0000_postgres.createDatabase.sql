/*
Create database
*/
SELECT 'CREATE DATABASE reference'
WHERE NOT EXISTS(SELECT FROM pg_catalog.pg_database WHERE datname = 'reference')\gexec