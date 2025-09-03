-- =====================================================
-- FIX USER PERMISSIONS BY ROLE
-- =====================================================
-- This script fixes user permissions to match their actual roles
-- Controllers and Employees should have limited permissions, not full admin access

USE [HR-Aviation];
GO

PRINT '=== FIXING USER PERMISSIONS BY ROLE ===';

-- Fix Organizational Permissions for Controllers
PRINT 'Fixing organizational permissions for Controllers...';
UPDATE UserOrganizationalPermissions 
SET 
    CanEdit = 0,      -- Controllers cannot edit organizational structure
    CanDelete = 0,    -- Controllers cannot delete organizational structure  
    CanCreate = 0     -- Controllers cannot create organizational structure
WHERE UserId IN (
    SELECT UserId FROM users WHERE RoleName = 'Controller'
) AND IsActive = 1;

-- Fix Organizational Permissions for Employees
PRINT 'Fixing organizational permissions for Employees...';
UPDATE UserOrganizationalPermissions 
SET 
    CanEdit = 0,      -- Employees cannot edit organizational structure
    CanDelete = 0,    -- Employees cannot delete organizational structure
    CanCreate = 0     -- Employees cannot create organizational structure
WHERE UserId IN (
    SELECT UserId FROM users WHERE RoleName = 'Employee'
) AND IsActive = 1;

-- Fix Operation Permissions for Controllers
PRINT 'Fixing operation permissions for Controllers...';
-- Controllers should only have basic permissions, not full admin access
UPDATE UserOperationPermissions 
SET IsAllowed = 0  -- Disable all operations first
WHERE UserId IN (
    SELECT UserId FROM users WHERE RoleName = 'Controller'
) AND IsActive = 1;

-- Then enable only basic operations for Controllers
UPDATE UserOperationPermissions 
SET IsAllowed = 1
WHERE UserId IN (
    SELECT UserId FROM users WHERE RoleName = 'Controller'
) AND IsActive = 1
AND (
    -- Basic profile operations
    (EntityType = 'Profile' AND OperationType IN ('View', 'Edit'))
    OR
    -- Basic observation operations
    (EntityType = 'Observation' AND OperationType IN ('View', 'Create'))
    OR
    -- Basic license operations
    (EntityType = 'License' AND OperationType = 'View')
    OR
    -- Basic certificate operations
    (EntityType = 'Certificate' AND OperationType = 'View')
);

-- Fix Operation Permissions for Employees
PRINT 'Fixing operation permissions for Employees...';
-- Employees should have even more limited permissions
UPDATE UserOperationPermissions 
SET IsAllowed = 0  -- Disable all operations first
WHERE UserId IN (
    SELECT UserId FROM users WHERE RoleName = 'Employee'
) AND IsActive = 1;

-- Then enable only basic operations for Employees
UPDATE UserOperationPermissions 
SET IsAllowed = 1
WHERE UserId IN (
    SELECT UserId FROM users WHERE RoleName = 'Employee'
) AND IsActive = 1
AND (
    -- Basic profile operations only
    (EntityType = 'Profile' AND OperationType IN ('View', 'Edit'))
    OR
    -- Basic license operations
    (EntityType = 'License' AND OperationType = 'View')
    OR
    -- Basic certificate operations
    (EntityType = 'Certificate' AND OperationType = 'View')
);

PRINT '=== VERIFICATION ===';

-- Check organizational permissions by role
SELECT 
    u.RoleName,
    COUNT(*) as TotalPermissions,
    SUM(CASE WHEN CanView = 1 THEN 1 ELSE 0 END) as CanView,
    SUM(CASE WHEN CanEdit = 1 THEN 1 ELSE 0 END) as CanEdit,
    SUM(CASE WHEN CanDelete = 1 THEN 1 ELSE 0 END) as CanDelete,
    SUM(CASE WHEN CanCreate = 1 THEN 1 ELSE 0 END) as CanCreate
FROM users u
JOIN UserOrganizationalPermissions uop ON u.UserId = uop.UserId
WHERE uop.IsActive = 1
GROUP BY u.RoleName
ORDER BY u.RoleName;

-- Check operation permissions by role
SELECT 
    u.RoleName,
    COUNT(*) as TotalPermissions,
    SUM(CASE WHEN IsAllowed = 1 THEN 1 ELSE 0 END) as AllowedPermissions
FROM users u
JOIN UserOperationPermissions uop ON u.UserId = uop.UserId
WHERE uop.IsActive = 1
GROUP BY u.RoleName
ORDER BY u.RoleName;

-- Show specific permissions for yazeed.bassam (Controller)
PRINT '=== PERMISSIONS FOR yazeed.bassam (Controller) ===';
SELECT 'Organizational Permissions:' as Info;
SELECT PermissionType, EntityName, CanView, CanEdit, CanDelete, CanCreate
FROM UserOrganizationalPermissions 
WHERE UserId = 1057 AND IsActive = 1
ORDER BY PermissionType, EntityName;

SELECT 'Operation Permissions:' as Info;
SELECT EntityType, OperationType, IsAllowed
FROM UserOperationPermissions 
WHERE UserId = 1057 AND IsActive = 1
ORDER BY EntityType, OperationType;

PRINT '=== SCRIPT COMPLETED SUCCESSFULLY ===';
PRINT 'User permissions have been fixed according to their roles!';
PRINT 'Controllers and Employees now have appropriate limited permissions.';