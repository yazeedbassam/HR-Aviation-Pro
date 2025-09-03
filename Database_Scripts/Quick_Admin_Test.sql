-- =====================================================
-- QUICK ADMIN TEST SCRIPT
-- =====================================================
-- Run this script to quickly test admin permissions
-- =====================================================

USE [HR-Aviation]
GO

PRINT '========================================'
PRINT 'QUICK ADMIN PERMISSIONS TEST'
PRINT '========================================'

-- Test 1: Check if admin user exists
PRINT 'Test 1: Checking Admin User...'
IF EXISTS (SELECT 1 FROM users WHERE username = 'admin')
BEGIN
    SELECT 
        'ADMIN USER FOUND' as Status,
        userid as UserId,
        username as Username,
        rolename as Role,
        IsActive as IsActive
    FROM users 
    WHERE username = 'admin'
END
ELSE
BEGIN
    PRINT 'ERROR: Admin user not found!'
END

-- Test 2: Check admin menu permissions
PRINT 'Test 2: Checking Admin Menu Permissions...'
IF EXISTS (SELECT * FROM sysobjects WHERE name='UserMenuPermissions' AND xtype='U')
BEGIN
    SELECT 
        'ADMIN MENU PERMISSIONS' as Status,
        COUNT(*) as TotalMenuPermissions,
        STRING_AGG(MenuKey, ', ') as MenuKeys
    FROM UserMenuPermissions ump
    INNER JOIN users u ON ump.UserId = u.userid
    WHERE u.username = 'admin' AND ump.IsVisible = 1
END
ELSE
BEGIN
    PRINT 'WARNING: UserMenuPermissions table does not exist'
END

-- Test 3: Check admin operation permissions
PRINT 'Test 3: Checking Admin Operation Permissions...'
IF EXISTS (SELECT * FROM sysobjects WHERE name='UserOperationPermissions' AND xtype='U')
BEGIN
    SELECT 
        'ADMIN OPERATION PERMISSIONS' as Status,
        COUNT(*) as TotalOperationPermissions
    FROM UserOperationPermissions uop
    INNER JOIN users u ON uop.UserId = u.userid
    WHERE u.username = 'admin' AND uop.IsAllowed = 1
END
ELSE
BEGIN
    PRINT 'WARNING: UserOperationPermissions table does not exist'
END

-- Test 4: Test stored procedures
PRINT 'Test 4: Testing Stored Procedures...'

-- Test CanUserViewMenu for admin
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CanUserViewMenu]') AND type in (N'P', N'PC'))
BEGIN
    DECLARE @AdminUserId INT
    SELECT @AdminUserId = userid FROM users WHERE username = 'admin'
    
    IF @AdminUserId IS NOT NULL
    BEGIN
        EXEC CanUserViewMenu @AdminUserId, 'DASHBOARD'
        EXEC CanUserViewMenu @AdminUserId, 'EMPLOYEES'
        EXEC CanUserViewMenu @AdminUserId, 'PERMISSIONS'
    END
END
ELSE
BEGIN
    PRINT 'WARNING: CanUserViewMenu stored procedure does not exist'
END

-- Test CanUserPerformOperation for admin
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CanUserPerformOperation]') AND type in (N'P', N'PC'))
BEGIN
    DECLARE @AdminUserId2 INT
    SELECT @AdminUserId2 = userid FROM users WHERE username = 'admin'
    
    IF @AdminUserId2 IS NOT NULL
    BEGIN
        EXEC CanUserPerformOperation @AdminUserId2, 'Employee', 'View'
        EXEC CanUserPerformOperation @AdminUserId2, 'Employee', 'Add'
        EXEC CanUserPerformOperation @AdminUserId2, 'Employee', 'Edit'
        EXEC CanUserPerformOperation @AdminUserId2, 'Employee', 'Delete'
    END
END
ELSE
BEGIN
    PRINT 'WARNING: CanUserPerformOperation stored procedure does not exist'
END

PRINT '========================================'
PRINT 'QUICK TEST COMPLETE'
PRINT 'If you see any warnings or errors above,'
PRINT 'run the Admin_Full_Permissions_Setup.sql script first.'
PRINT '========================================'
GO