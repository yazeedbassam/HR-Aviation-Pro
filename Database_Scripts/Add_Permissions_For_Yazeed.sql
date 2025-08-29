
USE [HR-Aviation]
GO

-- Add permissions for yazeed.bassam (User ID: 1047) in Queen department (Department ID: 71)

-- First, ensure all required permissions exist
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

-- Now add user permissions for yazeed.bassam
INSERT INTO UserDepartmentPermissions (UserId, DepartmentId, PermissionId, CanView, CanEdit, CanDelete, IsActive)
SELECT 1047, 71, PermissionId, 1, 1, 0, 1
FROM Permissions 
WHERE PermissionKey IN ('Add Certificate', 'Add License', 'Add Controller', 'Add Employee', 'Add Division')
AND NOT EXISTS (
    SELECT 1 FROM UserDepartmentPermissions udp 
    WHERE udp.UserId = 1047 
    AND udp.DepartmentId = 71 
    AND udp.PermissionId = Permissions.PermissionId
)

-- Show results
SELECT 'Permissions added for yazeed.bassam' as Status 
