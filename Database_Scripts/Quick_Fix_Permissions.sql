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

-- Get user and department IDs
DECLARE @UserId int = (SELECT userid FROM users WHERE username = 'yazeed.bassam')
DECLARE @DepartmentId int = (SELECT ValueId FROM ConfigurationValues WHERE ValueText = 'Queen' AND IsActive = 1)

-- Add user permissions
INSERT INTO UserDepartmentPermissions (UserId, DepartmentId, PermissionId, CanView, CanEdit, CanDelete, IsActive)
SELECT @UserId, @DepartmentId, PermissionId, 1, 1, 0, 1
FROM Permissions 
WHERE PermissionKey IN ('Add Certificate', 'Add License', 'Add Controller', 'Add Employee', 'Add Division')
AND NOT EXISTS (
    SELECT 1 FROM UserDepartmentPermissions udp 
    WHERE udp.UserId = @UserId 
    AND udp.DepartmentId = @DepartmentId 
    AND udp.PermissionId = Permissions.PermissionId
)

-- Show results
SELECT 'Permissions added successfully' as Status 
