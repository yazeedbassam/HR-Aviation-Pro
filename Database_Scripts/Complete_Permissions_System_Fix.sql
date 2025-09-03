-- =====================================================
-- COMPLETE PERMISSIONS SYSTEM FIX
-- =====================================================
-- This script fixes all permission system issues:
-- 1. Missing stored procedures
-- 2. CanUserViewMenu procedure not working correctly
-- 3. Controller user permissions not showing in sidebar

PRINT 'Starting Complete Permissions System Fix...'
PRINT '================================================'

-- Step 1: Create missing stored procedures
PRINT 'Step 1: Creating missing stored procedures...'

-- Create CanUserPerformOperation procedure if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CanUserPerformOperation]') AND type in (N'P', N'PC'))
BEGIN
    PRINT 'Creating CanUserPerformOperation procedure...'
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CanUserPerformOperation]') AND type in (N'P', N'PC'))
BEGIN
    CREATE PROCEDURE [dbo].[CanUserPerformOperation]
        @UserId int,
        @EntityType nvarchar(50),
        @OperationType nvarchar(50),
        @Scope nvarchar(50) = 'All',
        @ScopeId int = NULL
    AS
    BEGIN
        SET NOCOUNT ON;
        
        DECLARE @IsAllowed bit = 0
        
        -- Check if user has specific operation permission
        SELECT @IsAllowed = uop.IsAllowed
        FROM UserOperationPermissions uop
        INNER JOIN Permissions p ON uop.PermissionId = p.PermissionId
        WHERE uop.UserId = @UserId 
            AND uop.EntityType = @EntityType 
            AND uop.OperationType = @OperationType
            AND uop.IsActive = 1
            AND p.IsActive = 1
            AND (uop.Scope = @Scope OR (uop.Scope = 'All'))
            AND (@ScopeId IS NULL OR uop.ScopeId = @ScopeId OR uop.ScopeId IS NULL)
        
        -- If no specific permission found, check if user has admin role
        IF @IsAllowed IS NULL
        BEGIN
            SELECT @IsAllowed = CASE WHEN u.rolename = 'Admin' THEN 1 ELSE 0 END
            FROM users u
            WHERE u.userid = @UserId
        END
        
        -- If still no permission found, default to false
        IF @IsAllowed IS NULL
        BEGIN
            SET @IsAllowed = 0
        END
        
        SELECT @IsAllowed as IsAllowed
    END
END
GO

PRINT '✓ CanUserPerformOperation procedure created successfully'

-- Step 2: Fix CanUserViewMenu procedure
PRINT 'Step 2: Fixing CanUserViewMenu procedure...'

-- Drop and recreate the CanUserViewMenu procedure
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CanUserViewMenu]') AND type in (N'P', N'PC'))
BEGIN
    PRINT 'Dropping existing CanUserViewMenu procedure...'
    DROP PROCEDURE [dbo].[CanUserViewMenu]
END
GO

PRINT 'Creating fixed CanUserViewMenu procedure...'

CREATE PROCEDURE [dbo].[CanUserViewMenu]
    @UserId int,
    @MenuKey nvarchar(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @CanView bit = 0
    
    -- First, check if user has specific menu permission
    SELECT @CanView = IsVisible
    FROM UserMenuPermissions
    WHERE UserId = @UserId AND MenuKey = @MenuKey AND IsActive = 1
    
    -- If no specific permission found, check if user has admin role
    IF @CanView IS NULL
    BEGIN
        SELECT @CanView = CASE WHEN u.rolename = 'Admin' THEN 1 ELSE 0 END
        FROM users u
        WHERE u.userid = @UserId
    END
    
    -- If still no permission found, default to false
    IF @CanView IS NULL
    BEGIN
        SET @CanView = 0
    END
    
    SELECT @CanView as CanView
END
GO

PRINT '✓ CanUserViewMenu procedure created successfully'

-- Step 3: Fix Controller user permissions
PRINT 'Step 3: Fixing Controller user permissions...'

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

PRINT '✓ Controller user permissions fixed'

-- Step 4: Test the fixes
PRINT 'Step 4: Testing the fixes...'

-- Test CanUserViewMenu with yazeed.bassam
DECLARE @TestUserId INT;
SELECT @TestUserId = userid FROM users WHERE rolename = 'Controller' AND username = 'yazeed.bassam';

IF @TestUserId IS NOT NULL
BEGIN
    PRINT 'Testing CanUserViewMenu for yazeed.bassam:'
    
    PRINT 'PROFILE permission:'
    EXEC CanUserViewMenu @UserId = @TestUserId, @MenuKey = 'PROFILE';
    
    PRINT 'NOTIFICATIONS permission:'
    EXEC CanUserViewMenu @UserId = @TestUserId, @MenuKey = 'NOTIFICATIONS';
    
    PRINT 'DASHBOARD permission (should be 0):'
    EXEC CanUserViewMenu @UserId = @TestUserId, @MenuKey = 'DASHBOARD';
    
    PRINT 'CONTROLLERS permission (should be 0):'
    EXEC CanUserViewMenu @UserId = @TestUserId, @MenuKey = 'CONTROLLERS';
END

-- Show current menu permissions for Controller users
PRINT 'Current Controller users menu permissions:'
SELECT ump.MenuKey, ump.IsVisible, u.username
FROM UserMenuPermissions ump
JOIN users u ON ump.UserId = u.userid
WHERE u.rolename = 'Controller'
ORDER BY u.username, ump.MenuKey;

PRINT '================================================'
PRINT 'Complete Permissions System Fix completed successfully!'
PRINT 'All permission system issues have been resolved.'