-- Check the structure of Roles and RolePermissions tables
USE [HR-Aviation]
GO

-- Check Roles table structure
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Roles]') AND type in (N'U'))
BEGIN
    PRINT 'Roles table exists. Structure:'
    
    SELECT 
        COLUMN_NAME,
        DATA_TYPE,
        IS_NULLABLE,
        COLUMN_DEFAULT
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Roles' 
    AND TABLE_SCHEMA = 'dbo'
    ORDER BY ORDINAL_POSITION
    
    PRINT 'Sample data from Roles table:'
    SELECT TOP 5 * FROM [dbo].[Roles]
END
ELSE
BEGIN
    PRINT 'Roles table does not exist!'
END
GO

-- Check RolePermissions table structure
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RolePermissions]') AND type in (N'U'))
BEGIN
    PRINT 'RolePermissions table exists. Structure:'
    
    SELECT 
        COLUMN_NAME,
        DATA_TYPE,
        IS_NULLABLE,
        COLUMN_DEFAULT
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'RolePermissions' 
    AND TABLE_SCHEMA = 'dbo'
    ORDER BY ORDINAL_POSITION
    
    PRINT 'Sample data from RolePermissions table:'
    SELECT TOP 5 * FROM [dbo].[RolePermissions]
END
ELSE
BEGIN
    PRINT 'RolePermissions table does not exist!'
END
GO