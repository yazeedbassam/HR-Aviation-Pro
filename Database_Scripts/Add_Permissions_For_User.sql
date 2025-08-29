-- =====================================================
-- Add Permissions for yazeed.bassam
-- =====================================================

USE [HR-Aviation]
GO

-- Get user ID for yazeed.bassam
DECLARE @UserId int
SELECT @UserId = userid FROM users WHERE username = 'yazeed.bassam'
PRINT 'User ID for yazeed.bassam: ' + CAST(@UserId AS VARCHAR(10))

-- Get department ID for Queen (user's department)
DECLARE @DepartmentId int
SELECT @DepartmentId = ValueId FROM ConfigurationValues WHERE ValueText = 'Queen' AND IsActive = 1
PRINT 'Department ID for Queen: ' + CAST(@DepartmentId AS VARCHAR(10))

-- Get permission IDs
DECLARE @AddCertificatePermissionId int
DECLARE @AddLicensePermissionId int
DECLARE @AddControllerPermissionId int
DECLARE @AddEmployeePermissionId int

SELECT @AddCertificatePermissionId = PermissionId FROM Permissions WHERE PermissionKey = 'Add Certificate'
SELECT @AddLicensePermissionId = PermissionId FROM Permissions WHERE PermissionKey = 'Add License'
SELECT @AddControllerPermissionId = PermissionId FROM Permissions WHERE PermissionKey = 'Add Controller'
SELECT @AddEmployeePermissionId = PermissionId FROM Permissions WHERE PermissionKey = 'Add Employee'

PRINT 'Permission IDs:'
PRINT 'Add Certificate: ' + CAST(@AddCertificatePermissionId AS VARCHAR(10))
PRINT 'Add License: ' + CAST(@AddLicensePermissionId AS VARCHAR(10))
PRINT 'Add Controller: ' + CAST(@AddControllerPermissionId AS VARCHAR(10))
PRINT 'Add Employee: ' + CAST(@AddEmployeePermissionId AS VARCHAR(10))

-- Add permissions for the user
-- Add Certificate permission
IF NOT EXISTS (SELECT 1 FROM UserDepartmentPermissions WHERE UserId = @UserId AND PermissionId = @AddCertificatePermissionId)
BEGIN
    INSERT INTO UserDepartmentPermissions (UserId, DepartmentId, PermissionId, CanView, CanEdit, CanDelete, IsActive, CreatedAt)
    VALUES (@UserId, @DepartmentId, @AddCertificatePermissionId, 1, 1, 0, 1, GETDATE())
    PRINT 'Added Add Certificate permission'
END
ELSE
BEGIN
    PRINT 'Add Certificate permission already exists'
END

-- Add License permission
IF NOT EXISTS (SELECT 1 FROM UserDepartmentPermissions WHERE UserId = @UserId AND PermissionId = @AddLicensePermissionId)
BEGIN
    INSERT INTO UserDepartmentPermissions (UserId, DepartmentId, PermissionId, CanView, CanEdit, CanDelete, IsActive, CreatedAt)
    VALUES (@UserId, @DepartmentId, @AddLicensePermissionId, 1, 1, 0, 1, GETDATE())
    PRINT 'Added Add License permission'
END
ELSE
BEGIN
    PRINT 'Add License permission already exists'
END

-- Add Controller permission
IF NOT EXISTS (SELECT 1 FROM UserDepartmentPermissions WHERE UserId = @UserId AND PermissionId = @AddControllerPermissionId)
BEGIN
    INSERT INTO UserDepartmentPermissions (UserId, DepartmentId, PermissionId, CanView, CanEdit, CanDelete, IsActive, CreatedAt)
    VALUES (@UserId, @DepartmentId, @AddControllerPermissionId, 1, 1, 0, 1, GETDATE())
    PRINT 'Added Add Controller permission'
END
ELSE
BEGIN
    PRINT 'Add Controller permission already exists'
END

-- Add Employee permission
IF NOT EXISTS (SELECT 1 FROM UserDepartmentPermissions WHERE UserId = @UserId AND PermissionId = @AddEmployeePermissionId)
BEGIN
    INSERT INTO UserDepartmentPermissions (UserId, DepartmentId, PermissionId, CanView, CanEdit, CanDelete, IsActive, CreatedAt)
    VALUES (@UserId, @DepartmentId, @AddEmployeePermissionId, 1, 1, 0, 1, GETDATE())
    PRINT 'Added Add Employee permission'
END
ELSE
BEGIN
    PRINT 'Add Employee permission already exists'
END

-- Verify the permissions were added
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

-- Test the stored procedure
PRINT 'Testing stored procedures:'
EXEC CheckUserPermission @UserId = @UserId, @PermissionKey = 'Add Certificate', @DepartmentId = @DepartmentId
EXEC CheckUserPermission @UserId = @UserId, @PermissionKey = 'Add License', @DepartmentId = @DepartmentId
EXEC CheckUserPermission @UserId = @UserId, @PermissionKey = 'Add Controller', @DepartmentId = @DepartmentId
EXEC CheckUserPermission @UserId = @UserId, @PermissionKey = 'Add Employee', @DepartmentId = @DepartmentId

GO 
