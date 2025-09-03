-- Check the current structure of Permissions table
USE [HR-Aviation]
GO

-- Check if Permissions table exists and show its structure
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Permissions]') AND type in (N'U'))
BEGIN
    PRINT 'Permissions table exists. Current structure:'
    
    SELECT 
        COLUMN_NAME,
        DATA_TYPE,
        IS_NULLABLE,
        COLUMN_DEFAULT
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Permissions' 
    AND TABLE_SCHEMA = 'dbo'
    ORDER BY ORDINAL_POSITION
    
    PRINT 'Current data in Permissions table:'
    SELECT TOP 10 * FROM [dbo].[Permissions]
END
ELSE
BEGIN
    PRINT 'Permissions table does not exist!'
END
GO