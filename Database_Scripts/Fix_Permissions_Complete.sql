-- =====================================================
-- Fix Permissions System Complete
-- =====================================================

USE [HR-Aviation]
GO

PRINT '=== Step 1: Check Current Permissions ==='
SELECT PermissionId, PermissionName, PermissionKey, CategoryName, IsActive
FROM Permissions
WHERE IsActive = 1
ORDER BY CategoryName, PermissionName

PRINT '=== Step 2: Add Missing Permissions ==='

-- Add Certificate permission if not exists
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE PermissionKey = 'Add Certificate')
BEGIN
    INSERT INTO Permissions (PermissionName, PermissionKey, PermissionDescription, CategoryName, IsActive, CreatedAt)
    VALUES ('Add Certificate', 'Add Certificate', 'Permission to add new certificates', 'Certificates', 1, GETDATE())
    PRINT 'Added Add Certificate permission'
END

-- Add License permission if not exists
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE PermissionKey = 'Add License')
BEGIN
    INSERT INTO Permissions (PermissionName, PermissionKey, PermissionDescription, CategoryName, IsActive, CreatedAt)
    VALUES ('Add License', 'Add License', 'Permission to add new licenses', 'Licenses', 1, GETDATE())
    PRINT 'Added Add License permission'
END

-- Add Controller permission if not exists
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE PermissionKey = 'Add Controller')
BEGIN
    INSERT INTO Permissions (PermissionName, PermissionKey, PermissionDescription, CategoryName, IsActive, CreatedAt)
    VALUES ('Add Controller', 'Add Controller', 'Permission to add new controllers', 'Controllers', 1, GETDATE())
    PRINT 'Added Add Controller permission'
END

-- Add Employee permission if not exists
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE PermissionKey = 'Add Employee')
BEGIN
    INSERT INTO Permissions (PermissionName, PermissionKey, PermissionDescription, CategoryName, IsActive, CreatedAt)
    VALUES ('Add Employee', 'Add Employee', 'Permission to add new employees', 'Employees', 1, GETDATE())
    PRINT 'Added Add Employee permission'
END

-- Add Division permission if not exists
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE PermissionKey = 'Add Division')
BEGIN
    INSERT INTO Permissions (PermissionName, PermissionKey, PermissionDescription, CategoryName, IsActive, CreatedAt)
    VALUES ('Add Division', 'Add Division', 'Permission to add new divisions', 'Divisions', 1, GETDATE())
    PRINT 'Added Add Division permission'
END

PRINT '=== Step 3: Get User and Department IDs ==='

-- Get user ID for yazeed.bassam
DECLARE @UserId int
SELECT @UserId = userid FROM users WHERE username = 'yazeed.bassam'
PRINT 'User ID for yazeed.bassam: ' + CAST(@UserId AS VARCHAR(10))

-- Get department ID for Queen (user''s department)
DECLARE @DepartmentId int
SELECT @DepartmentId = ValueId FROM ConfigurationValues WHERE ValueText = 'Queen' AND IsActive = 1
PRINT 'Department ID for Queen: ' + CAST(@DepartmentId AS VARCHAR(10))

PRINT '=== Step 4: Get Permission IDs ==='

-- Get permission IDs
DECLARE @AddCertificatePermissionId int
DECLARE @AddLicensePermissionId int
DECLARE @AddControllerPermissionId int
DECLARE @AddEmployeePermissionId int
DECLARE @AddDivisionPermissionId int

SELECT @AddCertificatePermissionId = PermissionId FROM Permissions WHERE PermissionKey = 'Add Certificate'
SELECT @AddLicensePermissionId = PermissionId FROM Permissions WHERE PermissionKey = 'Add License'
SELECT @AddControllerPermissionId = PermissionId FROM Permissions WHERE PermissionKey = 'Add Controller'
SELECT @AddEmployeePermissionId = PermissionId FROM Permissions WHERE PermissionKey = 'Add Employee'
SELECT @AddDivisionPermissionId = PermissionId FROM Permissions WHERE PermissionKey = 'Add Division'

PRINT 'Permission IDs:'
PRINT 'Add Certificate: ' + CAST(ISNULL(@AddCertificatePermissionId, 0) AS VARCHAR(10))
PRINT 'Add License: ' + CAST(ISNULL(@AddLicensePermissionId, 0) AS VARCHAR(10))
PRINT 'Add Controller: ' + CAST(ISNULL(@AddControllerPermissionId, 0) AS VARCHAR(10))
PRINT 'Add Employee: ' + CAST(ISNULL(@AddEmployeePermissionId, 0) AS VARCHAR(10))
PRINT 'Add Division: ' + CAST(ISNULL(@AddDivisionPermissionId, 0) AS VARCHAR(10))

PRINT '=== Step 5: Add User Permissions ==='

-- Add permissions if they don''t exist for the user and department
IF @AddCertificatePermissionId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM UserDepartmentPermissions WHERE UserId = @UserId AND DepartmentId = @DepartmentId AND PermissionId = @AddCertificatePermissionId)
BEGIN
    INSERT INTO UserDepartmentPermissions (UserId, DepartmentId, PermissionId, CanView, CanEdit, CanDelete, IsActive)
    VALUES (@UserId, @DepartmentId, @AddCertificatePermissionId, 1, 1, 0, 1)
    PRINT 'Added "Add Certificate" permission for yazeed.bassam in Queen department.'
END

IF @AddLicensePermissionId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM UserDepartmentPermissions WHERE UserId = @UserId AND DepartmentId = @DepartmentId AND PermissionId = @AddLicensePermissionId)
BEGIN
    INSERT INTO UserDepartmentPermissions (UserId, DepartmentId, PermissionId, CanView, CanEdit, CanDelete, IsActive)
    VALUES (@UserId, @DepartmentId, @AddLicensePermissionId, 1, 1, 0, 1)
    PRINT 'Added "Add License" permission for yazeed.bassam in Queen department.'
END

IF @AddControllerPermissionId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM UserDepartmentPermissions WHERE UserId = @UserId AND DepartmentId = @DepartmentId AND PermissionId = @AddControllerPermissionId)
BEGIN
    INSERT INTO UserDepartmentPermissions (UserId, DepartmentId, PermissionId, CanView, CanEdit, CanDelete, IsActive)
    VALUES (@UserId, @DepartmentId, @AddControllerPermissionId, 1, 1, 0, 1)
    PRINT 'Added "Add Controller" permission for yazeed.bassam in Queen department.'
END

IF @AddEmployeePermissionId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM UserDepartmentPermissions WHERE UserId = @UserId AND DepartmentId = @DepartmentId AND PermissionId = @AddEmployeePermissionId)
BEGIN
    INSERT INTO UserDepartmentPermissions (UserId, DepartmentId, PermissionId, CanView, CanEdit, CanDelete, IsActive)
    VALUES (@UserId, @DepartmentId, @AddEmployeePermissionId, 1, 1, 0, 1)
    PRINT 'Added "Add Employee" permission for yazeed.bassam in Queen department.'
END

IF @AddDivisionPermissionId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM UserDepartmentPermissions WHERE UserId = @UserId AND DepartmentId = @DepartmentId AND PermissionId = @AddDivisionPermissionId)
BEGIN
    INSERT INTO UserDepartmentPermissions (UserId, DepartmentId, PermissionId, CanView, CanEdit, CanDelete, IsActive)
    VALUES (@UserId, @DepartmentId, @AddDivisionPermissionId, 1, 1, 0, 1)
    PRINT 'Added "Add Division" permission for yazeed.bassam in Queen department.'
END

PRINT '=== Step 6: Verify Permissions ==='

-- Show user''s permissions
SELECT
    u.username as UserName,
    dept.ValueText as DepartmentName,
    p.PermissionName,
    p.PermissionKey,
    udp.CanView,
    udp.CanEdit,
    udp.CanDelete,
    udp.IsActive
FROM UserDepartmentPermissions udp
JOIN users u ON udp.UserId = u.userid
JOIN ConfigurationValues dept ON udp.DepartmentId = dept.ValueId
JOIN Permissions p ON udp.PermissionId = p.PermissionId
WHERE u.username = 'yazeed.bassam'
ORDER BY p.PermissionName

PRINT '=== Step 7: Test Stored Procedure ==='

-- Test the stored procedure
DECLARE @TestResult bit
EXEC CheckUserPermission @UserId, 'Add Certificate', @DepartmentId, @TestResult OUTPUT
PRINT 'Test result for Add Certificate: ' + CAST(@TestResult AS VARCHAR(5))

EXEC CheckUserPermission @UserId, 'Add License', @DepartmentId, @TestResult OUTPUT
PRINT 'Test result for Add License: ' + CAST(@TestResult AS VARCHAR(5))

EXEC CheckUserPermission @UserId, 'Add Controller', @DepartmentId, @TestResult OUTPUT
PRINT 'Test result for Add Controller: ' + CAST(@TestResult AS VARCHAR(5))

PRINT '=== Permissions Fix Complete ===' 
