USE [HR-Aviation]
GO

-- Add missing permissions
INSERT INTO Permissions (PermissionName, PermissionKey, PermissionDescription, CategoryName, IsActive, CreatedAt)
SELECT 'Add Certificate', 'Add Certificate', 'Permission to add new certificates', 'Certificates', 1, GETDATE()
WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE PermissionKey = 'Add Certificate')

INSERT INTO Permissions (PermissionName, PermissionKey, PermissionDescription, CategoryName, IsActive, CreatedAt)
SELECT 'Add License', 'Add License', 'Permission to add new licenses', 'Licenses', 1, GETDATE()
WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE PermissionKey = 'Add License')

INSERT INTO Permissions (PermissionName, PermissionKey, PermissionDescription, CategoryName, IsActive, CreatedAt)
SELECT 'Add Controller', 'Add Controller', 'Permission to add new controllers', 'Controllers', 1, GETDATE()
WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE PermissionKey = 'Add Controller')

INSERT INTO Permissions (PermissionName, PermissionKey, PermissionDescription, CategoryName, IsActive, CreatedAt)
SELECT 'Add Employee', 'Add Employee', 'Permission to add new employees', 'Employees', 1, GETDATE()
WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE PermissionKey = 'Add Employee')

INSERT INTO Permissions (PermissionName, PermissionKey, PermissionDescription, CategoryName, IsActive, CreatedAt)
SELECT 'Add Division', 'Add Division', 'Permission to add new divisions', 'Divisions', 1, GETDATE()
WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE PermissionKey = 'Add Division')

-- Add permissions for yazeed.bassam
DECLARE @UserId int
DECLARE @DepartmentId int

SELECT @UserId = userid FROM users WHERE username = 'yazeed.bassam'
SELECT @DepartmentId = ValueId FROM ConfigurationValues WHERE ValueText = 'Queen' AND IsActive = 1

PRINT 'User ID: ' + CAST(@UserId AS VARCHAR(10))
PRINT 'Department ID: ' + CAST(@DepartmentId AS VARCHAR(10))

-- Add Certificate permission for user
INSERT INTO UserDepartmentPermissions (UserId, DepartmentId, PermissionId, CanView, CanEdit, CanDelete, IsActive, CreatedAt)
SELECT @UserId, @DepartmentId, PermissionId, 1, 1, 0, 1, GETDATE()
FROM Permissions WHERE PermissionKey = 'Add Certificate'
AND NOT EXISTS (SELECT 1 FROM UserDepartmentPermissions WHERE UserId = @UserId AND PermissionId = Permissions.PermissionId)

-- Add License permission for user
INSERT INTO UserDepartmentPermissions (UserId, DepartmentId, PermissionId, CanView, CanEdit, CanDelete, IsActive, CreatedAt)
SELECT @UserId, @DepartmentId, PermissionId, 1, 1, 0, 1, GETDATE()
FROM Permissions WHERE PermissionKey = 'Add License'
AND NOT EXISTS (SELECT 1 FROM UserDepartmentPermissions WHERE UserId = @UserId AND PermissionId = Permissions.PermissionId)

-- Add Controller permission for user
INSERT INTO UserDepartmentPermissions (UserId, DepartmentId, PermissionId, CanView, CanEdit, CanDelete, IsActive, CreatedAt)
SELECT @UserId, @DepartmentId, PermissionId, 1, 1, 0, 1, GETDATE()
FROM Permissions WHERE PermissionKey = 'Add Controller'
AND NOT EXISTS (SELECT 1 FROM UserDepartmentPermissions WHERE UserId = @UserId AND PermissionId = Permissions.PermissionId)

-- Add Employee permission for user
INSERT INTO UserDepartmentPermissions (UserId, DepartmentId, PermissionId, CanView, CanEdit, CanDelete, IsActive, CreatedAt)
SELECT @UserId, @DepartmentId, PermissionId, 1, 1, 0, 1, GETDATE()
FROM Permissions WHERE PermissionKey = 'Add Employee'
AND NOT EXISTS (SELECT 1 FROM UserDepartmentPermissions WHERE UserId = @UserId AND PermissionId = Permissions.PermissionId)

-- Add Division permission for user
INSERT INTO UserDepartmentPermissions (UserId, DepartmentId, PermissionId, CanView, CanEdit, CanDelete, IsActive, CreatedAt)
SELECT @UserId, @DepartmentId, PermissionId, 1, 1, 0, 1, GETDATE()
FROM Permissions WHERE PermissionKey = 'Add Division'
AND NOT EXISTS (SELECT 1 FROM UserDepartmentPermissions WHERE UserId = @UserId AND PermissionId = Permissions.PermissionId)

-- Show results
SELECT 'Permissions added successfully' as Status

-- Verify permissions were added
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

GO 
