-- =====================================================
-- Create UserDepartmentPermissions Table
-- =====================================================

USE [HR-Aviation]
GO

-- Check if table exists
IF OBJECT_ID('UserDepartmentPermissions', 'U') IS NOT NULL
BEGIN
    PRINT 'UserDepartmentPermissions table already exists'
    
    -- Show table structure
    SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'UserDepartmentPermissions'
    ORDER BY ORDINAL_POSITION
    
    -- Show data count
    DECLARE @RowCount int
    SELECT @RowCount = COUNT(*) FROM UserDepartmentPermissions
    PRINT 'Total rows in UserDepartmentPermissions: ' + CAST(@RowCount AS VARCHAR(10))
    
    -- Show sample data if exists
    IF @RowCount > 0
    BEGIN
        SELECT TOP 5 * FROM UserDepartmentPermissions
    END
    ELSE
    BEGIN
        PRINT 'No data found in UserDepartmentPermissions table'
    END
END
ELSE
BEGIN
    PRINT 'Creating UserDepartmentPermissions table...'
    
    CREATE TABLE UserDepartmentPermissions (
        UserDepartmentPermissionId INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL,
        DepartmentId INT NOT NULL,
        PermissionId INT NOT NULL,
        CanView BIT DEFAULT 1,
        CanEdit BIT DEFAULT 0,
        CanDelete BIT DEFAULT 0,
        IsActive BIT DEFAULT 1,
        CreatedAt DATETIME DEFAULT GETDATE(),
        UpdatedAt DATETIME NULL
    )
    
    PRINT 'UserDepartmentPermissions table created successfully'
    
    -- Add some sample data for testing
    INSERT INTO UserDepartmentPermissions (UserId, DepartmentId, PermissionId, CanView, CanEdit, CanDelete, IsActive)
    VALUES 
        (1012, 10, 110, 1, 1, 0, 1),  -- a.tabtah - Administration - Add Certificate
        (1012, 8, 102, 1, 1, 1, 1),   -- a.tabtah - AIS - Add Controller
        (1026, 9, 98, 1, 0, 0, 1),    -- abd.arbyat - CNS - Add Employee
        (1016, 67, 110, 1, 1, 0, 1);  -- abdalkareem.shiab - AFTN - Add Certificate
    
    PRINT 'Sample data inserted successfully'
END

-- Verify the table and data
SELECT 'Verification' as Status, COUNT(*) as TotalRecords 
FROM UserDepartmentPermissions

SELECT TOP 10 
    udp.UserDepartmentPermissionId,
    u.username as UserName,
    dept.ValueText as DepartmentName,
    p.PermissionName,
    udp.CanView,
    udp.CanEdit,
    udp.CanDelete,
    udp.IsActive,
    udp.CreatedAt
FROM UserDepartmentPermissions udp
JOIN users u ON udp.UserId = u.userid
JOIN ConfigurationValues dept ON udp.DepartmentId = dept.ValueId
JOIN Permissions p ON udp.PermissionId = p.PermissionId
ORDER BY udp.CreatedAt DESC

GO 
