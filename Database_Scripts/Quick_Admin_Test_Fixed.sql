-- =====================================================
-- QUICK ADMIN TEST SCRIPT - FIXED VERSION
-- =====================================================
-- Run this script to quickly test admin permissions
-- Fixed for the actual database structure
-- =====================================================

USE [HR-Aviation]
GO

PRINT '========================================'
PRINT 'QUICK ADMIN PERMISSIONS TEST'
PRINT '========================================'

-- Test 1: Check if admin user exists
PRINT 'Test 1: Checking Admin User...'
IF EXISTS (SELECT 1 FROM users WHERE Username = 'admin')
BEGIN
    SELECT 
        'ADMIN USER FOUND' as Status,
        UserId as UserId,
        Username as Username,
        RoleName as Role
    FROM users 
    WHERE Username = 'admin'
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
    INNER JOIN users u ON ump.UserId = u.UserId
    WHERE u.Username = 'admin' AND ump.IsVisible = 1
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
    INNER JOIN users u ON uop.UserId = u.UserId
    WHERE u.Username = 'admin' AND uop.IsAllowed = 1
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
    SELECT @AdminUserId = UserId FROM users WHERE Username = 'admin'
    
    IF @AdminUserId IS NOT NULL
    BEGIN
        PRINT 'Testing CanUserViewMenu for admin user...'
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
    SELECT @AdminUserId2 = UserId FROM users WHERE Username = 'admin'
    
    IF @AdminUserId2 IS NOT NULL
    BEGIN
        PRINT 'Testing CanUserPerformOperation for admin user...'
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

-- Test 5: Check total permissions in system
PRINT 'Test 5: Checking Total System Permissions...'
IF EXISTS (SELECT * FROM sysobjects WHERE name='Permissions' AND xtype='U')
BEGIN
    SELECT 
        'TOTAL SYSTEM PERMISSIONS' as Status,
        COUNT(*) as TotalPermissions,
        COUNT(CASE WHEN CategoryName = 'Menu' THEN 1 END) as MenuPermissions,
        COUNT(CASE WHEN CategoryName = 'Operations' THEN 1 END) as OperationPermissions,
        COUNT(CASE WHEN CategoryName = 'System' THEN 1 END) as SystemPermissions
    FROM Permissions
    WHERE IsActive = 1
END
ELSE
BEGIN
    PRINT 'WARNING: Permissions table does not exist'
END

PRINT '========================================'
PRINT 'QUICK TEST COMPLETE'
PRINT 'If you see any warnings or errors above,'
PRINT 'run the Admin_Permissions_Fixed.sql script first.'
PRINT '========================================'
GO