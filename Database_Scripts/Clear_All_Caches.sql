-- =====================================================
-- CLEAR ALL CACHES AND REFRESH DATA
-- =====================================================
-- This script clears all caches and refreshes permission data
-- =====================================================

USE [HR-Aviation]
GO

PRINT '========================================'
PRINT 'CLEARING ALL CACHES AND REFRESHING DATA'
PRINT '========================================'

-- Clear any cached data by updating timestamps
PRINT 'Updating timestamps to force cache refresh...'

-- Update organizational permissions timestamps
UPDATE UserOrganizationalPermissions 
SET UpdatedAt = GETDATE()
WHERE UserId = 5

-- Update operation permissions timestamps  
UPDATE UserOperationPermissions 
SET UpdatedAt = GETDATE()
WHERE UserId = 5

-- Update menu permissions timestamps
UPDATE UserMenuPermissions 
SET UpdatedAt = GETDATE()
WHERE UserId = 5

PRINT 'Cache refresh completed!'

-- Verify data is still there
PRINT 'Verifying data integrity...'

SELECT 
    'Organizational Permissions' as PermissionType,
    COUNT(*) as TotalCount,
    COUNT(CASE WHEN IsActive = 1 THEN 1 END) as ActiveCount
FROM UserOrganizationalPermissions 
WHERE UserId = 5

UNION ALL

SELECT 
    'Operation Permissions' as PermissionType,
    COUNT(*) as TotalCount,
    COUNT(CASE WHEN IsActive = 1 THEN 1 END) as ActiveCount
FROM UserOperationPermissions 
WHERE UserId = 5

UNION ALL

SELECT 
    'Menu Permissions' as PermissionType,
    COUNT(*) as TotalCount,
    COUNT(CASE WHEN IsActive = 1 THEN 1 END) as ActiveCount
FROM UserMenuPermissions 
WHERE UserId = 5

PRINT '========================================'
PRINT 'CACHE CLEAR COMPLETE!'
PRINT '========================================'