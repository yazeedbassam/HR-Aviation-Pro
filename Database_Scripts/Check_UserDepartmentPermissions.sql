-- =====================================================
-- Check UserDepartmentPermissions Table and Data
-- =====================================================

USE [HR-Aviation]
GO

-- Check if table exists
IF OBJECT_ID('UserDepartmentPermissions', 'U') IS NOT NULL
BEGIN
    PRINT 'UserDepartmentPermissions table exists'
    
    -- Check table structure
    SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'UserDepartmentPermissions'
    ORDER BY ORDINAL_POSITION
    
    -- Check if there's any data
    DECLARE @RowCount int
    SELECT @RowCount = COUNT(*) FROM UserDepartmentPermissions
    PRINT 'Total rows in UserDepartmentPermissions: ' + CAST(@RowCount AS VARCHAR(10))
    
    -- Show sample data if exists
    IF @RowCount > 0
    BEGIN
        SELECT TOP 10 * FROM UserDepartmentPermissions
    END
    ELSE
    BEGIN
        PRINT 'No data found in UserDepartmentPermissions table'
    END
END
ELSE
BEGIN
    PRINT 'UserDepartmentPermissions table does not exist!'
    
    -- Create the table if it doesn't exist
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
END

-- Check related tables
PRINT 'Checking related tables...'

-- Check Permissions table
IF OBJECT_ID('Permissions', 'U') IS NOT NULL
BEGIN
    DECLARE @PermCount int
    SELECT @PermCount = COUNT(*) FROM Permissions
    PRINT 'Permissions table exists with ' + CAST(@PermCount AS VARCHAR(10)) + ' records'
END
ELSE
BEGIN
    PRINT 'Permissions table does not exist!'
END

-- Check ConfigurationValues for departments
DECLARE @DeptCount int
SELECT @DeptCount = COUNT(*) 
FROM ConfigurationValues cv 
JOIN ConfigurationCategories cc ON cv.CategoryId = cc.CategoryId 
WHERE (cc.CategoryName = 'Divisions' OR cc.CategoryName = 'Departments')
PRINT 'Total departments in ConfigurationValues: ' + CAST(@DeptCount AS VARCHAR(10))

-- Check users table
IF OBJECT_ID('users', 'U') IS NOT NULL
BEGIN
    DECLARE @UserCount int
    SELECT @UserCount = COUNT(*) FROM users
    PRINT 'Users table exists with ' + CAST(@UserCount AS VARCHAR(10)) + ' records'
END
ELSE
BEGIN
    PRINT 'Users table does not exist!'
END

GO 
