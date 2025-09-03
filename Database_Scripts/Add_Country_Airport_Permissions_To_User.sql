-- Add Country and Airport permissions to user yazeed.bassam
USE [HR-Aviation];

-- Get user ID
DECLARE @UserId INT;
SELECT @UserId = UserId FROM Users WHERE Username = 'yazeed.bassam';

-- Add Country permissions
INSERT INTO UserOperationPermissions (UserId, PermissionId, EntityType, OperationType, IsAllowed, Scope, CreatedAt, UpdatedAt)
SELECT @UserId, p.PermissionId, 'Country', 'View', 1, 'All', GETDATE(), GETDATE()
FROM Permissions p
WHERE p.PermissionKey = 'COUNTRY_VIEW'
AND NOT EXISTS (SELECT 1 FROM UserOperationPermissions WHERE UserId = @UserId AND EntityType = 'Country' AND OperationType = 'View');

INSERT INTO UserOperationPermissions (UserId, PermissionId, EntityType, OperationType, IsAllowed, Scope, CreatedAt, UpdatedAt)
SELECT @UserId, p.PermissionId, 'Country', 'Add', 1, 'All', GETDATE(), GETDATE()
FROM Permissions p
WHERE p.PermissionKey = 'COUNTRY_ADD'
AND NOT EXISTS (SELECT 1 FROM UserOperationPermissions WHERE UserId = @UserId AND EntityType = 'Country' AND OperationType = 'Add');

-- Add Airport permissions
INSERT INTO UserOperationPermissions (UserId, PermissionId, EntityType, OperationType, IsAllowed, Scope, CreatedAt, UpdatedAt)
SELECT @UserId, p.PermissionId, 'Airport', 'View', 1, 'All', GETDATE(), GETDATE()
FROM Permissions p
WHERE p.PermissionKey = 'AIRPORT_VIEW'
AND NOT EXISTS (SELECT 1 FROM UserOperationPermissions WHERE UserId = @UserId AND EntityType = 'Airport' AND OperationType = 'View');

INSERT INTO UserOperationPermissions (UserId, PermissionId, EntityType, OperationType, IsAllowed, Scope, CreatedAt, UpdatedAt)
SELECT @UserId, p.PermissionId, 'Airport', 'Add', 1, 'All', GETDATE(), GETDATE()
FROM Permissions p
WHERE p.PermissionKey = 'AIRPORT_ADD'
AND NOT EXISTS (SELECT 1 FROM UserOperationPermissions WHERE UserId = @UserId AND EntityType = 'Airport' AND OperationType = 'Add');

-- Check the results
SELECT uop.UserId, u.Username, uop.EntityType, uop.OperationType, uop.IsAllowed, uop.Scope
FROM UserOperationPermissions uop
INNER JOIN Users u ON uop.UserId = u.UserId
WHERE u.Username = 'yazeed.bassam' 
AND (uop.EntityType = 'Country' OR uop.EntityType = 'Airport')
ORDER BY uop.EntityType, uop.OperationType;