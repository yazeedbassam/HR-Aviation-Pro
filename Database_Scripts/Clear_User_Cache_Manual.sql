-- Clear user cache manually for user yazeed.bassam
USE [HR-Aviation];

-- Update user permission timestamp to invalidate all caches
-- This will force the application to reload permissions from database
UPDATE Users 
SET UpdatedAt = GETDATE()
WHERE Username = 'yazeed.bassam';

-- Verify the user exists and get their ID
SELECT UserId, Username, RoleName, UpdatedAt 
FROM Users 
WHERE Username = 'yazeed.bassam';

-- Check current permissions for the user
SELECT uop.UserId, u.Username, uop.EntityType, uop.OperationType, uop.IsAllowed, uop.Scope
FROM UserOperationPermissions uop
INNER JOIN Users u ON uop.UserId = u.UserId
WHERE u.Username = 'yazeed.bassam' 
AND (uop.EntityType = 'Country' OR uop.EntityType = 'Airport')
ORDER BY uop.EntityType, uop.OperationType;