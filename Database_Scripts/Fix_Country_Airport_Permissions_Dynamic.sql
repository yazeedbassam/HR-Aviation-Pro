-- Fix Country and Airport Permissions for yazeed.bassam - Dynamic Solution
USE [HR-Aviation];

-- Get user ID for yazeed.bassam
DECLARE @UserId INT;
SELECT @UserId = UserId FROM Users WHERE Username = 'yazeed.bassam';

IF @UserId IS NULL
BEGIN
    PRINT 'User yazeed.bassam not found!';
    RETURN;
END

PRINT 'Processing user: yazeed.bassam (ID: ' + CAST(@UserId AS NVARCHAR(10)) + ')';

-- Step 1: Clear all existing Country and Airport permissions for this user
DELETE FROM UserOperationPermissions 
WHERE UserId = @UserId AND EntityType IN ('Country', 'Airport');

PRINT 'Cleared existing Country and Airport permissions';

-- Step 2: Add Country permissions (View, Add, Edit, Delete, Export)
INSERT INTO UserOperationPermissions (UserId, PermissionId, EntityType, OperationType, IsAllowed, Scope, ScopeId, IsActive, CreatedAt, UpdatedAt)
VALUES 
    (@UserId, 1, 'Country', 'View', 1, 'All', NULL, 1, GETDATE(), GETDATE()),
    (@UserId, 1, 'Country', 'Add', 1, 'All', NULL, 1, GETDATE(), GETDATE()),
    (@UserId, 1, 'Country', 'Edit', 1, 'All', NULL, 1, GETDATE(), GETDATE()),
    (@UserId, 1, 'Country', 'Delete', 1, 'All', NULL, 1, GETDATE(), GETDATE()),
    (@UserId, 1, 'Country', 'Export', 1, 'All', NULL, 1, GETDATE(), GETDATE());

PRINT 'Added Country permissions';

-- Step 3: Add Airport permissions (View, Add, Edit, Delete, Export)
INSERT INTO UserOperationPermissions (UserId, PermissionId, EntityType, OperationType, IsAllowed, Scope, ScopeId, IsActive, CreatedAt, UpdatedAt)
VALUES 
    (@UserId, 1, 'Airport', 'View', 1, 'All', NULL, 1, GETDATE(), GETDATE()),
    (@UserId, 1, 'Airport', 'Add', 1, 'All', NULL, 1, GETDATE(), GETDATE()),
    (@UserId, 1, 'Airport', 'Edit', 1, 'All', NULL, 1, GETDATE(), GETDATE()),
    (@UserId, 1, 'Airport', 'Delete', 1, 'All', NULL, 1, GETDATE(), GETDATE()),
    (@UserId, 1, 'Airport', 'Export', 1, 'All', NULL, 1, GETDATE(), GETDATE());

PRINT 'Added Airport permissions';

-- Step 4: Update user permission timestamp to invalidate cache
UPDATE Users SET LastPermissionUpdate = GETDATE() WHERE UserId = @UserId;

-- Step 5: Verify the permissions were added correctly
SELECT 
    u.Username,
    uop.EntityType,
    uop.OperationType,
    uop.IsAllowed,
    uop.Scope,
    uop.CreatedAt
FROM UserOperationPermissions uop
INNER JOIN Users u ON uop.UserId = u.UserId
WHERE uop.UserId = @UserId 
    AND uop.EntityType IN ('Country', 'Airport')
ORDER BY uop.EntityType, uop.OperationType;

PRINT 'Country and Airport permissions have been successfully applied to yazeed.bassam';
PRINT 'Please restart the application or clear the cache to see the changes immediately.'; 