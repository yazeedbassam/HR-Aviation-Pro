-- =====================================================
-- FIX PERMISSIONLOGS TABLE STRUCTURE
-- =====================================================
-- This script fixes the PermissionLogs table structure to match the code
-- =====================================================

USE [HR-Aviation]
GO

PRINT '========================================'
PRINT 'FIXING PERMISSIONLOGS TABLE STRUCTURE'
PRINT '========================================'

-- Check if PermissionLogs table exists
IF EXISTS (SELECT * FROM sysobjects WHERE name='PermissionLogs' AND xtype='U')
BEGIN
    PRINT 'PermissionLogs table exists. Checking structure...'
    
    -- Check if Status column exists
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PermissionLogs' AND COLUMN_NAME = 'Status')
    BEGIN
        PRINT 'Adding Status column...'
        ALTER TABLE PermissionLogs ADD Status NVARCHAR(20) NOT NULL DEFAULT 'UNKNOWN'
    END
    
    -- Check if Details column exists
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PermissionLogs' AND COLUMN_NAME = 'Details')
    BEGIN
        PRINT 'Adding Details column...'
        ALTER TABLE PermissionLogs ADD Details NVARCHAR(500) NULL
    END
    
    -- Check if PermissionKey column exists
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PermissionLogs' AND COLUMN_NAME = 'PermissionKey')
    BEGIN
        PRINT 'Adding PermissionKey column...'
        ALTER TABLE PermissionLogs ADD PermissionKey NVARCHAR(50) NULL
    END
    
    -- Check if IpAddress column exists (case sensitive)
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PermissionLogs' AND COLUMN_NAME = 'IpAddress')
    BEGIN
        -- Check if IPAddress column exists (different case)
        IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PermissionLogs' AND COLUMN_NAME = 'IPAddress')
        BEGIN
            PRINT 'Renaming IPAddress to IpAddress...'
            EXEC sp_rename 'PermissionLogs.IPAddress', 'IpAddress', 'COLUMN'
        END
        ELSE
        BEGIN
            PRINT 'Adding IpAddress column...'
            ALTER TABLE PermissionLogs ADD IpAddress NVARCHAR(45) NULL
        END
    END
    
    -- Check if UserAgent column exists
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PermissionLogs' AND COLUMN_NAME = 'UserAgent')
    BEGIN
        PRINT 'Adding UserAgent column...'
        ALTER TABLE PermissionLogs ADD UserAgent NVARCHAR(500) NULL
    END
    
    -- Check if Timestamp column exists
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PermissionLogs' AND COLUMN_NAME = 'Timestamp')
    BEGIN
        PRINT 'Adding Timestamp column...'
        ALTER TABLE PermissionLogs ADD Timestamp DATETIME NOT NULL DEFAULT GETDATE()
    END
    
    PRINT 'PermissionLogs table structure updated successfully!'
END
ELSE
BEGIN
    PRINT 'Creating PermissionLogs table...'
    
    CREATE TABLE [dbo].[PermissionLogs](
        [LogId] [int] IDENTITY(1,1) NOT NULL,
        [UserId] [int] NOT NULL,
        [PermissionId] [int] NULL,
        [PermissionKey] [nvarchar](50) NULL,
        [DepartmentId] [int] NULL,
        [Status] [nvarchar](20) NOT NULL,
        [Details] [nvarchar](500) NULL,
        [IpAddress] [nvarchar](45) NULL,
        [UserAgent] [nvarchar](500) NULL,
        [Timestamp] [datetime] NOT NULL DEFAULT GETDATE(),
        CONSTRAINT [PK_PermissionLogs] PRIMARY KEY CLUSTERED ([LogId] ASC)
    );
    
    PRINT 'PermissionLogs table created successfully!'
END

-- Verify the table structure
PRINT '========================================'
PRINT 'VERIFYING PERMISSIONLOGS TABLE STRUCTURE'
PRINT '========================================'

SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'PermissionLogs'
ORDER BY ORDINAL_POSITION

PRINT '========================================'
PRINT 'PERMISSIONLOGS TABLE FIX COMPLETE!'
PRINT '========================================'
GO