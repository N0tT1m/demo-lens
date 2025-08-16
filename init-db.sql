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
PRINT 'Database initialization completed.';
GO