-- =====================================================
-- COMPLETE CONTROLLER PERMISSIONS FIX
-- =====================================================
-- This script applies all fixes for Controller user permissions
-- Run this script to fix the issue where Controller users get too many permissions

PRINT 'Starting Complete Controller Permissions Fix...'
PRINT '================================================'

-- Step 1: Check required tables
PRINT 'Step 1: Checking required tables...'
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UserMenuPermissions]') AND type in (N'U'))
BEGIN
    PRINT 'ERROR: UserMenuPermissions table does not exist!'
    PRINT 'Please run Advanced_Permission_System_New.sql first'
    RETURN
END

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UserDepartmentPermissions]') AND type in (N'U'))
BEGIN
    PRINT 'ERROR: UserDepartmentPermissions table does not exist!'
    PRINT 'Please run Advanced_Permission_System_New.sql first'
    RETURN
END

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Permissions]') AND type in (N'U'))
BEGIN
    PRINT 'ERROR: Permissions table does not exist!'
    PRINT 'Please run Advanced_Permission_System_New.sql first'
    RETURN
END

PRINT '✓ All required tables exist'

-- Step 2: Fix Controller Menu Permissions
PRINT 'Step 2: Fixing Controller Menu Permissions...'

-- Remove all existing menu permissions for Controller users
DELETE ump 
FROM UserMenuPermissions ump
JOIN users u ON ump.UserId = u.userid
WHERE u.rolename = 'Controller';

-- Add only PROFILE and NOTIFICATIONS permissions for Controller users
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

PRINT '✓ Controller menu permissions fixed'

-- Step 3: Fix Controller Page Access Control
PRINT 'Step 3: Fixing Controller Page Access Control...'

-- Remove all existing UserDepartmentPermissions for Controller users
DELETE udp 
FROM UserDepartmentPermissions udp
JOIN users u ON udp.UserId = u.userid
WHERE u.rolename = 'Controller';

-- Remove all existing UserOperationPermissions for Controller users
DELETE uop 
FROM UserOperationPermissions uop
JOIN users u ON uop.UserId = u.userid
WHERE u.rolename = 'Controller';

-- Ensure PROFILE_VIEW permission exists
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE PermissionKey = 'PROFILE_VIEW')
BEGIN
    INSERT INTO Permissions (PermissionName, PermissionKey, PermissionDescription, CategoryName, IsActive, CreatedAt)
    VALUES ('Profile View', 'PROFILE_VIEW', 'Can view own profile page', 'Profile', 1, GETDATE());
END

-- Ensure NOTIFICATIONS_VIEW permission exists
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE PermissionKey = 'NOTIFICATIONS_VIEW')
BEGIN
    INSERT INTO Permissions (PermissionName, PermissionKey, PermissionDescription, CategoryName, IsActive, CreatedAt)
    VALUES ('Notifications View', 'NOTIFICATIONS_VIEW', 'Can view notifications', 'Notifications', 1, GETDATE());
END

-- Get permission IDs
DECLARE @ProfilePermissionId INT, @NotificationsPermissionId INT;
SELECT @ProfilePermissionId = PermissionId FROM Permissions WHERE PermissionKey = 'PROFILE_VIEW';
SELECT @NotificationsPermissionId = PermissionId FROM Permissions WHERE PermissionKey = 'NOTIFICATIONS_VIEW';

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

PRINT '✓ Controller page access control fixed'

-- Step 4: Fix Controller Default Permissions
PRINT 'Step 4: Fixing Controller Default Permissions...'

-- Remove all existing permissions for Controller role
DECLARE @ControllerRoleId INT;
SELECT @ControllerRoleId = cv.ValueId 
FROM ConfigurationValues cv 
JOIN ConfigurationCategories cc ON cv.CategoryId = cc.CategoryId 
WHERE cc.CategoryName = 'Roles' AND cv.ValueText = 'Controller';

IF @ControllerRoleId IS NOT NULL
BEGIN
    DELETE FROM RolePermissions WHERE RoleId = @ControllerRoleId;
    
    -- Add only PROFILE_VIEW permission for Controller role
    INSERT INTO RolePermissions (RoleId, PermissionId, IsActive, CreatedAt)
    SELECT @ControllerRoleId, PermissionId, 1, GETDATE()
    FROM Permissions
    WHERE IsActive = 1
    AND PermissionKey = 'PROFILE_VIEW'
    AND PermissionId NOT IN (
        SELECT PermissionId 
        FROM RolePermissions 
        WHERE RoleId = @ControllerRoleId
    );
    
    PRINT '✓ Controller default permissions fixed'
END
ELSE
BEGIN
    PRINT '⚠ Controller role not found in ConfigurationValues - skipping role permissions fix'
END

-- Step 5: Verify the changes
PRINT 'Step 5: Verifying changes...'

-- Check menu permissions
PRINT 'Controller users menu permissions:'
SELECT ump.MenuKey, ump.IsVisible, u.username
FROM UserMenuPermissions ump
JOIN users u ON ump.UserId = u.userid
WHERE u.rolename = 'Controller'
ORDER BY u.username, ump.MenuKey;

-- Check page permissions
PRINT 'Controller users page permissions:'
SELECT 
    u.username,
    p.PermissionKey,
    p.PermissionName,
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

PRINT '================================================'
PRINT 'Complete Controller Permissions Fix completed successfully!'
PRINT 'Controller users now only have access to Profile and Notifications pages.'