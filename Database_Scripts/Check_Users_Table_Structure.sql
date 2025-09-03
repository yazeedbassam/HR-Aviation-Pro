-- Check the structure of users table
USE [HR-Aviation]
GO

-- Check users table structure
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[users]') AND type in (N'U'))
BEGIN
    PRINT 'Users table exists. Structure:'
    
    SELECT 
        COLUMN_NAME,
        DATA_TYPE,
        IS_NULLABLE,
        COLUMN_DEFAULT
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'users' 
    AND TABLE_SCHEMA = 'dbo'
    ORDER BY ORDINAL_POSITION
    
    PRINT 'Sample data from users table:'
    SELECT TOP 5 * FROM [dbo].[users]
END
ELSE
BEGIN
    PRINT 'Users table does not exist!'
END
GO