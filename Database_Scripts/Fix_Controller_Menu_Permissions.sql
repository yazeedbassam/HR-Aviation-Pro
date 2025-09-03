-- =====================================================
-- FIX CONTROLLER MENU PERMISSIONS
-- =====================================================
-- This script ensures Controller users only see Profile and Notifications in sidebar

-- First, let's check current menu permissions for Controller role
PRINT 'Current Controller role menu permissions:'
SELECT ump.MenuKey, ump.IsVisible, u.username
FROM UserMenuPermissions ump
JOIN users u ON ump.UserId = u.userid
WHERE u.rolename = 'Controller'
ORDER BY u.username, ump.MenuKey;

-- Remove all existing menu permissions for Controller users
PRINT 'Removing all existing menu permissions for Controller users...'
DELETE ump 
FROM UserMenuPermissions ump
JOIN users u ON ump.UserId = u.userid
WHERE u.rolename = 'Controller';

-- Add only PROFILE and NOTIFICATIONS permissions for Controller users
PRINT 'Adding minimal menu permissions for Controller users...'
INSERT INTO UserMenuPermissions (UserId, MenuKey, IsVisible, IsActive, CreatedAt)
SELECT 
    u.userid,
    'PROFILE',
    1, -- IsVisible
    1, -- IsActive
    GETDATE()
FROM users u
WHERE u.rolename = 'Controller'
AND NOT EXISTS (
    SELECT 1 FROM UserMenuPermissions ump 
    WHERE ump.UserId = u.userid AND ump.MenuKey = 'PROFILE'
);

INSERT INTO UserMenuPermissions (UserId, MenuKey, IsVisible, IsActive, CreatedAt)
SELECT 
    u.userid,
    'NOTIFICATIONS',
    1, -- IsVisible
    1, -- IsActive
    GETDATE()
FROM users u
WHERE u.rolename = 'Controller'
AND NOT EXISTS (
    SELECT 1 FROM UserMenuPermissions ump 
    WHERE ump.UserId = u.userid AND ump.MenuKey = 'NOTIFICATIONS'
);

-- Verify the changes
PRINT 'Updated Controller users menu permissions:'
SELECT ump.MenuKey, ump.IsVisible, u.username
FROM UserMenuPermissions ump
JOIN users u ON ump.UserId = u.userid
WHERE u.rolename = 'Controller'
ORDER BY u.username, ump.MenuKey;

-- Also ensure the CanUserViewMenu procedure works correctly for Controller users
PRINT 'Testing CanUserViewMenu for Controller users...'
DECLARE @TestUserId INT;
SELECT @TestUserId = userid FROM users WHERE rolename = 'Controller' AND username = 'yazeed.bassam';

IF @TestUserId IS NOT NULL
BEGIN
    PRINT 'Testing PROFILE permission for yazeed.bassam:'
    EXEC CanUserViewMenu @UserId = @TestUserId, @MenuKey = 'PROFILE';
    
    PRINT 'Testing NOTIFICATIONS permission for yazeed.bassam:'
    EXEC CanUserViewMenu @UserId = @TestUserId, @MenuKey = 'NOTIFICATIONS';
    
    PRINT 'Testing DASHBOARD permission for yazeed.bassam (should be 0):'
    EXEC CanUserViewMenu @UserId = @TestUserId, @MenuKey = 'DASHBOARD';
    
    PRINT 'Testing CONTROLLERS permission for yazeed.bassam (should be 0):'
    EXEC CanUserViewMenu @UserId = @TestUserId, @MenuKey = 'CONTROLLERS';
END

PRINT 'Controller menu permissions fix completed successfully!'