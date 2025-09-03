-- =====================================================
-- DEBUG PERMISSION DISPLAY ISSUE
-- =====================================================
-- This script helps debug why permissions are not displaying correctly
-- =====================================================

USE [HR-Aviation]
GO

PRINT '========================================'
PRINT 'DEBUGGING PERMISSION DISPLAY ISSUE'
PRINT '========================================'

-- Test 1: Check admin user
PRINT 'Test 1: Admin user details'
SELECT 
    UserId,
    Username,
    RoleName
FROM users 
WHERE Username = 'admin'

-- Test 2: Check organizational permissions count
PRINT 'Test 2: Organizational permissions for admin'
SELECT 
    COUNT(*) as TotalCount,
    COUNT(CASE WHEN IsActive = 1 THEN 1 END) as ActiveCount,
    COUNT(CASE WHEN IsActive = 0 THEN 1 END) as InactiveCount
FROM UserOrganizationalPermissions 
WHERE UserId = 5

-- Test 3: Sample organizational permissions
PRINT 'Test 3: Sample organizational permissions (first 5)'
SELECT TOP 5
    UserOrganizationalPermissionId,
    PermissionType,
    EntityId,
    EntityName,
    CanView,
    CanEdit,
    CanDelete,
    CanCreate,
    IsActive
FROM UserOrganizationalPermissions 
WHERE UserId = 5
ORDER BY UserOrganizationalPermissionId

-- Test 4: Check operation permissions count
PRINT 'Test 4: Operation permissions for admin'
SELECT 
    COUNT(*) as TotalCount,
    COUNT(CASE WHEN IsActive = 1 THEN 1 END) as ActiveCount,
    COUNT(CASE WHEN IsActive = 0 THEN 1 END) as InactiveCount
FROM UserOperationPermissions 
WHERE UserId = 5

-- Test 5: Sample operation permissions
PRINT 'Test 5: Sample operation permissions (first 5)'
SELECT TOP 5
    uop.UserOperationPermissionId,
    uop.EntityType,
    uop.OperationType,
    uop.IsAllowed,
    uop.IsActive,
    p.PermissionKey
FROM UserOperationPermissions uop
LEFT JOIN Permissions p ON uop.PermissionId = p.PermissionId
WHERE uop.UserId = 5
ORDER BY uop.UserOperationPermissionId

-- Test 6: Check menu permissions count
PRINT 'Test 6: Menu permissions for admin'
SELECT 
    COUNT(*) as TotalCount,
    COUNT(CASE WHEN IsActive = 1 THEN 1 END) as ActiveCount,
    COUNT(CASE WHEN IsActive = 0 THEN 1 END) as InactiveCount
FROM UserMenuPermissions 
WHERE UserId = 5

-- Test 7: Sample menu permissions
PRINT 'Test 7: Sample menu permissions (first 5)'
SELECT TOP 5
    UserMenuPermissionId,
    MenuKey,
    IsVisible,
    IsActive
FROM UserMenuPermissions 
WHERE UserId = 5
ORDER BY UserMenuPermissionId

-- Test 8: Check for any NULL values that might cause issues
PRINT 'Test 8: Check for NULL values in organizational permissions'
SELECT 
    COUNT(*) as TotalRecords,
    COUNT(CASE WHEN PermissionType IS NULL THEN 1 END) as NullPermissionType,
    COUNT(CASE WHEN EntityId IS NULL THEN 1 END) as NullEntityId,
    COUNT(CASE WHEN EntityName IS NULL THEN 1 END) as NullEntityName,
    COUNT(CASE WHEN IsActive IS NULL THEN 1 END) as NullIsActive
FROM UserOrganizationalPermissions 
WHERE UserId = 5

PRINT '========================================'
PRINT 'PERMISSION DISPLAY DEBUG COMPLETE!'
PRINT '========================================'