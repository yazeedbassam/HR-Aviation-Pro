-- =====================================================
-- FORCE REFRESH PERMISSIONS - FINAL SOLUTION
-- =====================================================
-- This script forces a complete refresh of all permission data
-- =====================================================

USE [HR-Aviation]
GO

PRINT '========================================'
PRINT 'FORCING COMPLETE PERMISSION REFRESH'
PRINT '========================================'

-- Step 1: Clear any potential cache issues
PRINT 'Step 1: Updating all timestamps to force cache refresh...'

UPDATE UserOrganizationalPermissions 
SET UpdatedAt = GETDATE()
WHERE UserId = 5

UPDATE UserOperationPermissions 
SET UpdatedAt = GETDATE()
WHERE UserId = 5

UPDATE UserMenuPermissions 
SET UpdatedAt = GETDATE()
WHERE UserId = 5

-- Step 2: Ensure all permissions are active
PRINT 'Step 2: Ensuring all permissions are active...'

UPDATE UserOrganizationalPermissions 
SET IsActive = 1
WHERE UserId = 5 AND IsActive = 0

UPDATE UserOperationPermissions 
SET IsActive = 1
WHERE UserId = 5 AND IsActive = 0

UPDATE UserMenuPermissions 
SET IsActive = 1
WHERE UserId = 5 AND IsActive = 0

-- Step 3: Verify final state
PRINT 'Step 3: Final verification...'

SELECT 
    'Organizational Permissions' as PermissionType,
    COUNT(*) as TotalCount,
    COUNT(CASE WHEN IsActive = 1 THEN 1 END) as ActiveCount,
    MAX(UpdatedAt) as LastUpdated
FROM UserOrganizationalPermissions 
WHERE UserId = 5

UNION ALL

SELECT 
    'Operation Permissions' as PermissionType,
    COUNT(*) as TotalCount,
    COUNT(CASE WHEN IsActive = 1 THEN 1 END) as ActiveCount,
    MAX(UpdatedAt) as LastUpdated
FROM UserOperationPermissions 
WHERE UserId = 5

UNION ALL

SELECT 
    'Menu Permissions' as PermissionType,
    COUNT(*) as TotalCount,
    COUNT(CASE WHEN IsActive = 1 THEN 1 END) as ActiveCount,
    MAX(UpdatedAt) as LastUpdated
FROM UserMenuPermissions 
WHERE UserId = 5

PRINT '========================================'
PRINT 'PERMISSION REFRESH COMPLETE!'
PRINT '========================================'