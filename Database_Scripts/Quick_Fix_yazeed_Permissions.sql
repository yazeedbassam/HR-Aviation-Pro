-- Quick Fix for yazeed.bassam Country and Airport Permissions
USE [HR-Aviation];

-- Step 1: Get user ID
DECLARE @UserId INT = (SELECT UserId FROM Users WHERE Username = 'yazeed.bassam');

-- Step 2: Clear old permissions
DELETE FROM UserOperationPermissions WHERE UserId = @UserId AND EntityType IN ('Country', 'Airport');

-- Step 3: Add Country permissions
INSERT INTO UserOperationPermissions (UserId, PermissionId, EntityType, OperationType, IsAllowed, Scope, CreatedAt, UpdatedAt)
VALUES 
    (@UserId, 1, 'Country', 'View', 1, 'All', GETDATE(), GETDATE()),
    (@UserId, 1, 'Country', 'Add', 1, 'All', GETDATE(), GETDATE()),
    (@UserId, 1, 'Country', 'Edit', 1, 'All', GETDATE(), GETDATE()),
    (@UserId, 1, 'Country', 'Delete', 1, 'All', GETDATE(), GETDATE()),
    (@UserId, 1, 'Country', 'Export', 1, 'All', GETDATE(), GETDATE());

-- Step 4: Add Airport permissions
INSERT INTO UserOperationPermissions (UserId, PermissionId, EntityType, OperationType, IsAllowed, Scope, CreatedAt, UpdatedAt)
VALUES 
    (@UserId, 1, 'Airport', 'View', 1, 'All', GETDATE(), GETDATE()),
    (@UserId, 1, 'Airport', 'Add', 1, 'All', GETDATE(), GETDATE()),
    (@UserId, 1, 'Airport', 'Edit', 1, 'All', GETDATE(), GETDATE()),
    (@UserId, 1, 'Airport', 'Delete', 1, 'All', GETDATE(), GETDATE()),
    (@UserId, 1, 'Airport', 'Export', 1, 'All', GETDATE(), GETDATE());

-- Step 5: Update timestamp
UPDATE Users SET LastPermissionUpdate = GETDATE() WHERE UserId = @UserId;

-- Step 6: Verify
SELECT 'SUCCESS: Country and Airport permissions applied to yazeed.bassam' AS Status; 