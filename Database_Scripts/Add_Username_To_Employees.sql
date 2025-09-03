-- =====================================================
-- SQL Script to Add Username Field to Employees Table
-- This script adds a Username field to the Employees table
-- and populates it with existing usernames from the Users table
-- =====================================================

USE [HR-Aviation]; -- Replace with your actual database name
GO

-- Function to check if column exists
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ColumnExists]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
BEGIN
    EXEC('CREATE FUNCTION [dbo].[ColumnExists](@TableName NVARCHAR(128), @ColumnName NVARCHAR(128))
    RETURNS BIT
    AS
    BEGIN
        DECLARE @Result BIT = 0
        IF EXISTS (
            SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
            WHERE TABLE_NAME = @TableName AND COLUMN_NAME = @ColumnName
        )
            SET @Result = 1
        RETURN @Result
    END')
END
GO

-- Add Username field to Employees table (only if it doesn't exist)
IF dbo.ColumnExists('Employees', 'Username') = 0
BEGIN
    ALTER TABLE Employees ADD Username NVARCHAR(100) NULL;
    PRINT 'Added Username column to Employees table';
END
ELSE
    PRINT 'Username column already exists in Employees table';
GO

-- Update existing Employees with Username from Users table
PRINT 'Updating existing Employees with Username from Users table...';

UPDATE e
SET e.Username = u.Username
FROM Employees e
INNER JOIN Users u ON e.UserID = u.UserID
WHERE e.Username IS NULL OR e.Username = '';

PRINT 'Updated ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' employee records with usernames';

-- Verify the update
PRINT '=== Verifying Username field update ===';
SELECT 
    e.EmployeeID,
    e.FullName,
    e.UserID,
    e.Username,
    u.Username AS UsersTableUsername,
    CASE 
        WHEN e.Username = u.Username THEN '✅ Match'
        WHEN e.Username IS NULL THEN '❌ NULL in Employees'
        WHEN u.Username IS NULL THEN '❌ NULL in Users'
        ELSE '❌ Mismatch'
    END AS Status
FROM Employees e
LEFT JOIN Users u ON e.UserID = u.UserID
ORDER BY e.EmployeeID;

-- Show summary
PRINT '=== Summary ===';
SELECT 
    COUNT(*) AS TotalEmployees,
    COUNT(e.Username) AS EmployeesWithUsername,
    COUNT(*) - COUNT(e.Username) AS EmployeesWithoutUsername
FROM Employees e;

-- Clean up the helper function
DROP FUNCTION IF EXISTS [dbo].[ColumnExists];
GO

PRINT 'Username field added successfully to Employees table!';
PRINT 'The system now stores usernames in both Users and Employees tables for better performance and consistency.';
GO 