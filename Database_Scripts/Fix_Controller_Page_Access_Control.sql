-- =====================================================
-- FIX CONTROLLER PAGE ACCESS CONTROL
-- =====================================================
-- This script ensures Controller users can only access Profile and Notifications pages

-- First, let's check what permissions Controller users currently have
PRINT 'Current Controller users permissions:'
SELECT 
    u.username,
    u.rolename,
    p.PermissionKey,
    p.PermissionName,
    p.CategoryName
FROM users u
LEFT JOIN UserDepartmentPermissions udp ON u.userid = udp.UserId
LEFT JOIN Permissions p ON udp.PermissionId = p.PermissionId
WHERE u.rolename = 'Controller'
ORDER BY u.username, p.PermissionKey;

-- Remove all existing UserDepartmentPermissions for Controller users
PRINT 'Removing all existing UserDepartmentPermissions for Controller users...'
DELETE udp 
FROM UserDepartmentPermissions udp
JOIN users u ON udp.UserId = u.userid
WHERE u.rolename = 'Controller';

-- Remove all existing UserOperationPermissions for Controller users
PRINT 'Removing all existing UserOperationPermissions for Controller users...'
DELETE uop 
FROM UserOperationPermissions uop
JOIN users u ON uop.UserId = u.userid
WHERE u.rolename = 'Controller';

-- Ensure PROFILE_VIEW permission exists
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE PermissionKey = 'PROFILE_VIEW')
BEGIN
    PRINT 'Creating PROFILE_VIEW permission...'
    INSERT INTO Permissions (PermissionName, PermissionKey, PermissionDescription, CategoryName, IsActive, CreatedAt)
    VALUES ('Profile View', 'PROFILE_VIEW', 'Can view own profile page', 'Profile', 1, GETDATE());
END

-- Ensure NOTIFICATIONS_VIEW permission exists
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE PermissionKey = 'NOTIFICATIONS_VIEW')
BEGIN
    PRINT 'Creating NOTIFICATIONS_VIEW permission...'
    INSERT INTO Permissions (PermissionName, PermissionKey, PermissionDescription, CategoryName, IsActive, CreatedAt)
    VALUES ('Notifications View', 'NOTIFICATIONS_VIEW', 'Can view notifications', 'Notifications', 1, GETDATE());
END

-- Get permission IDs
DECLARE @ProfilePermissionId INT, @NotificationsPermissionId INT;
SELECT @ProfilePermissionId = PermissionId FROM Permissions WHERE PermissionKey = 'PROFILE_VIEW';
SELECT @NotificationsPermissionId = PermissionId FROM Permissions WHERE PermissionKey = 'NOTIFICATIONS_VIEW';

-- Add minimal permissions for Controller users
PRINT 'Adding minimal permissions for Controller users...'

-- Add PROFILE_VIEW permission for all Controller users
INSERT INTO UserDepartmentPermissions (UserId, PermissionId, DepartmentId, CanView, CanEdit, CanDelete, IsActive, CreatedAt)
SELECT 
    u.userid,
    @ProfilePermissionId,
    1, -- Default department
    1, -- CanView
    0, -- CanEdit
    0, -- CanDelete
    1, -- IsActive
    GETDATE()
FROM users u
WHERE u.rolename = 'Controller'
AND NOT EXISTS (
    SELECT 1 FROM UserDepartmentPermissions udp 
    WHERE udp.UserId = u.userid AND udp.PermissionId = @ProfilePermissionId
);

-- Add NOTIFICATIONS_VIEW permission for all Controller users
INSERT INTO UserDepartmentPermissions (UserId, PermissionId, DepartmentId, CanView, CanEdit, CanDelete, IsActive, CreatedAt)
SELECT 
    u.userid,
    @NotificationsPermissionId,
    1, -- Default department
    1, -- CanView
    0, -- CanEdit
    0, -- CanDelete
    1, -- IsActive
    GETDATE()
FROM users u
WHERE u.rolename = 'Controller'
AND NOT EXISTS (
    SELECT 1 FROM UserDepartmentPermissions udp 
    WHERE udp.UserId = u.userid AND udp.PermissionId = @NotificationsPermissionId
);

-- Verify the changes
PRINT 'Updated Controller users permissions:'
SELECT 
    u.username,
    u.rolename,
    p.PermissionKey,
    p.PermissionName,
    p.CategoryName,
    udp.CanView,
    udp.CanEdit,
    udp.CanDelete
FROM users u
JOIN UserDepartmentPermissions udp ON u.userid = udp.UserId
JOIN Permissions p ON udp.PermissionId = p.PermissionId
WHERE u.rolename = 'Controller'
ORDER BY u.username, p.PermissionKey;

-- Test permission checking for yazeed.bassam
DECLARE @TestUserId INT;
SELECT @TestUserId = userid FROM users WHERE rolename = 'Controller' AND username = 'yazeed.bassam';

IF @TestUserId IS NOT NULL
BEGIN
    PRINT 'Testing permissions for yazeed.bassam:'
    
    PRINT 'PROFILE_VIEW permission:'
    EXEC CheckUserPermission @UserId = @TestUserId, @PermissionKey = 'PROFILE_VIEW', @DepartmentId = NULL;
    
    PRINT 'NOTIFICATIONS_VIEW permission:'
    EXEC CheckUserPermission @UserId = @TestUserId, @PermissionKey = 'NOTIFICATIONS_VIEW', @DepartmentId = NULL;
    
    PRINT 'DASHBOARD_VIEW permission (should be 0):'
    EXEC CheckUserPermission @UserId = @TestUserId, @PermissionKey = 'DASHBOARD_VIEW', @DepartmentId = NULL;
    
    PRINT 'CONTROLLERS_VIEW permission (should be 0):'
    EXEC CheckUserPermission @UserId = @TestUserId, @PermissionKey = 'CONTROLLERS_VIEW', @DepartmentId = NULL;
END

PRINT 'Controller page access control fix completed successfully!'