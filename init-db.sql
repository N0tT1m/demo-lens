-- Initialize database for local development
USE master;
GO

-- Create database if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'demos')
BEGIN
    CREATE DATABASE demos;
END
GO

USE demos;
GO

-- The application will handle EF migrations automatically on startup
-- This script just ensures the database exists

-- Wait for application to create tables, then apply performance indexes
-- Note: This will run after EF migrations have created the base schema
PRINT 'Database initialization completed. Performance indexes will be applied via separate script.';
GO