-- =====================================================
-- FIX CanUserViewMenu PROCEDURE
-- =====================================================
-- This script fixes the CanUserViewMenu procedure to properly check UserMenuPermissions

-- Drop and recreate the CanUserViewMenu procedure
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CanUserViewMenu]') AND type in (N'P', N'PC'))
BEGIN
    PRINT 'Dropping existing CanUserViewMenu procedure...'
    DROP PROCEDURE [dbo].[CanUserViewMenu]
END

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

PRINT '✓ CanUserViewMenu procedure created successfully'

-- Test the procedure with yazeed.bassam
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

PRINT 'CanUserViewMenu procedure fix completed successfully!'