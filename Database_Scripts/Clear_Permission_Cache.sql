-- =====================================================
-- CLEAR PERMISSION CACHE
-- =====================================================
-- This script clears any cached permission data
-- =====================================================

USE [HR-Aviation]
GO

PRINT '========================================'
PRINT 'CLEARING PERMISSION CACHE'
PRINT '========================================'

-- Clear any cached data by updating timestamps
UPDATE UserMenuPermissions SET UpdatedAt = GETDATE() WHERE UserId = 5
UPDATE UserOperationPermissions SET UpdatedAt = GETDATE() WHERE UserId = 5  
UPDATE UserOrganizationalPermissions SET UpdatedAt = GETDATE() WHERE UserId = 5

PRINT 'Permission cache cleared successfully!'
PRINT 'Please restart the application to ensure changes take effect.'

-- Verify admin permissions
SELECT 
    'Admin User Verification' as CheckType,
    UserId,
    Username,
    RoleName
FROM users 
WHERE Username = 'admin'

-- Verify ORGANIZATION_VIEW permission exists
SELECT 
    'ORGANIZATION_VIEW Permission' as CheckType,
    PermissionId,
    PermissionKey,
    PermissionName,
    CategoryName
FROM Permissions 
WHERE PermissionKey = 'ORGANIZATION_VIEW'

-- Test stored procedure
DECLARE @HasPermission BIT
EXEC CheckUserPermission 5, 'ORGANIZATION_VIEW', NULL
SELECT @HasPermission as HasPermission

PRINT '========================================'
PRINT 'CACHE CLEAR COMPLETE!'
PRINT '========================================'
GO