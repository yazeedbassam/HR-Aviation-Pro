-- Quick Fix for Country and Airport Permissions - yazeed.bassam
USE [HR-Aviation];

-- Get user ID
DECLARE @UserId INT = (SELECT UserId FROM Users WHERE Username = 'yazeed.bassam');

-- Clear existing permissions
DELETE FROM UserOperationPermissions WHERE UserId = @UserId AND EntityType IN ('Country', 'Airport');

-- Add Country permissions
INSERT INTO UserOperationPermissions (UserId, PermissionId, EntityType, OperationType, IsAllowed, Scope, CreatedAt, UpdatedAt)
VALUES 
    (@UserId, 1, 'Country', 'View', 1, 'All', GETDATE(), GETDATE()),
    (@UserId, 1, 'Country', 'Add', 1, 'All', GETDATE(), GETDATE()),
    (@UserId, 1, 'Country', 'Edit', 1, 'All', GETDATE(), GETDATE()),
    (@UserId, 1, 'Country', 'Delete', 1, 'All', GETDATE(), GETDATE()),
    (@UserId, 1, 'Country', 'Export', 1, 'All', GETDATE(), GETDATE());

-- Add Airport permissions
INSERT INTO UserOperationPermissions (UserId, PermissionId, EntityType, OperationType, IsAllowed, Scope, CreatedAt, UpdatedAt)
VALUES 
    (@UserId, 1, 'Airport', 'View', 1, 'All', GETDATE(), GETDATE()),
    (@UserId, 1, 'Airport', 'Add', 1, 'All', GETDATE(), GETDATE()),
    (@UserId, 1, 'Airport', 'Edit', 1, 'All', GETDATE(), GETDATE()),
    (@UserId, 1, 'Airport', 'Delete', 1, 'All', GETDATE(), GETDATE()),
    (@UserId, 1, 'Airport', 'Export', 1, 'All', GETDATE(), GETDATE());

-- Update user timestamp
UPDATE Users SET LastPermissionUpdate = GETDATE() WHERE UserId = @UserId;

-- Verify
SELECT 'Country and Airport permissions applied successfully' AS Status; 