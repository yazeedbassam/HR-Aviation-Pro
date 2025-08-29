-- =====================================================
-- Add Permissions for yazeed.bassam (Updated Version)
-- =====================================================

USE [HR-Aviation]
GO

-- First, ensure all required permissions exist
PRINT '=== Ensuring Required Permissions Exist ==='

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

-- Now get user and department IDs
DECLARE @UserId int
DECLARE @DepartmentId int

SELECT @UserId = userid FROM users WHERE username = 'yazeed.bassam'
SELECT @DepartmentId = ValueId FROM ConfigurationValues WHERE ValueText = 'Queen' AND IsActive = 1

PRINT 'User ID for yazeed.bassam: ' + CAST(@UserId AS VARCHAR(10))
PRINT 'Department ID for Queen: ' + CAST(@DepartmentId AS VARCHAR(10))

-- Get permission IDs (now they should exist)
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
PRINT 'Add Certificate: ' + CAST(@AddCertificatePermissionId AS VARCHAR(10))
PRINT 'Add License: ' + CAST(@AddLicensePermissionId AS VARCHAR(10))
PRINT 'Add Controller: ' + CAST(@AddControllerPermissionId AS VARCHAR(10))
PRINT 'Add Employee: ' + CAST(@AddEmployeePermissionId AS VARCHAR(10))
PRINT 'Add Division: ' + CAST(@AddDivisionPermissionId AS VARCHAR(10))

-- Add permissions for the user
PRINT '=== Adding User Permissions ==='

-- Add Certificate permission
IF NOT EXISTS (SELECT 1 FROM UserDepartmentPermissions WHERE UserId = @UserId AND PermissionId = @AddCertificatePermissionId)
BEGIN
    INSERT INTO UserDepartmentPermissions (UserId, DepartmentId, PermissionId, CanView, CanEdit, CanDelete, IsActive, CreatedAt)
    VALUES (@UserId, @DepartmentId, @AddCertificatePermissionId, 1, 1, 0, 1, GETDATE())
    PRINT 'Added Add Certificate permission for user'
END
ELSE
BEGIN
    PRINT 'Add Certificate permission already exists for user'
END

-- Add License permission
IF NOT EXISTS (SELECT 1 FROM UserDepartmentPermissions WHERE UserId = @UserId AND PermissionId = @AddLicensePermissionId)
BEGIN
    INSERT INTO UserDepartmentPermissions (UserId, DepartmentId, PermissionId, CanView, CanEdit, CanDelete, IsActive, CreatedAt)
    VALUES (@UserId, @DepartmentId, @AddLicensePermissionId, 1, 1, 0, 1, GETDATE())
    PRINT 'Added Add License permission for user'
END
ELSE
BEGIN
    PRINT 'Add License permission already exists for user'
END

-- Add Controller permission
IF NOT EXISTS (SELECT 1 FROM UserDepartmentPermissions WHERE UserId = @UserId AND PermissionId = @AddControllerPermissionId)
BEGIN
    INSERT INTO UserDepartmentPermissions (UserId, DepartmentId, PermissionId, CanView, CanEdit, CanDelete, IsActive, CreatedAt)
    VALUES (@UserId, @DepartmentId, @AddControllerPermissionId, 1, 1, 0, 1, GETDATE())
    PRINT 'Added Add Controller permission for user'
END
ELSE
BEGIN
    PRINT 'Add Controller permission already exists for user'
END

-- Add Employee permission
IF NOT EXISTS (SELECT 1 FROM UserDepartmentPermissions WHERE UserId = @UserId AND PermissionId = @AddEmployeePermissionId)
BEGIN
    INSERT INTO UserDepartmentPermissions (UserId, DepartmentId, PermissionId, CanView, CanEdit, CanDelete, IsActive, CreatedAt)
    VALUES (@UserId, @DepartmentId, @AddEmployeePermissionId, 1, 1, 0, 1, GETDATE())
    PRINT 'Added Add Employee permission for user'
END
ELSE
BEGIN
    PRINT 'Add Employee permission already exists for user'
END

-- Add Division permission
IF NOT EXISTS (SELECT 1 FROM UserDepartmentPermissions WHERE UserId = @UserId AND PermissionId = @AddDivisionPermissionId)
BEGIN
    INSERT INTO UserDepartmentPermissions (UserId, DepartmentId, PermissionId, CanView, CanEdit, CanDelete, IsActive, CreatedAt)
    VALUES (@UserId, @DepartmentId, @AddDivisionPermissionId, 1, 1, 0, 1, GETDATE())
    PRINT 'Added Add Division permission for user'
END
ELSE
BEGIN
    PRINT 'Add Division permission already exists for user'
END

-- Verify the permissions were added
PRINT '=== Final User Permissions ==='
SELECT 
    udp.UserDepartmentPermissionId,
    u.username as UserName,
    dept.ValueText as DepartmentName,
    p.PermissionName,
    p.PermissionKey,
    udp.CanView,
    udp.CanEdit,
    udp.CanDelete,
    udp.IsActive,
    udp.CreatedAt
FROM UserDepartmentPermissions udp
JOIN users u ON udp.UserId = u.userid
JOIN ConfigurationValues dept ON udp.DepartmentId = dept.ValueId
JOIN Permissions p ON udp.PermissionId = p.PermissionId
WHERE udp.UserId = @UserId
ORDER BY p.PermissionName

-- Test the stored procedures
PRINT '=== Testing Stored Procedures ==='
EXEC CheckUserPermission @UserId = @UserId, @PermissionKey = 'Add Certificate', @DepartmentId = @DepartmentId
EXEC CheckUserPermission @UserId = @UserId, @PermissionKey = 'Add License', @DepartmentId = @DepartmentId
EXEC CheckUserPermission @UserId = @UserId, @PermissionKey = 'Add Controller', @DepartmentId = @DepartmentId
EXEC CheckUserPermission @UserId = @UserId, @PermissionKey = 'Add Employee', @DepartmentId = @DepartmentId
EXEC CheckUserPermission @UserId = @UserId, @PermissionKey = 'Add Division', @DepartmentId = @DepartmentId

GO 
