-- =====================================================
-- FIX CONTROLLER PERMISSIONS FINAL
-- =====================================================
-- This script fixes the permissions for Controller role to have appropriate access

USE [HR-Aviation];
GO

PRINT '=== FIXING CONTROLLER PERMISSIONS FINAL ===';

-- Fix Operation Permissions for Controllers
PRINT 'Fixing operation permissions for Controllers...';

-- Enable basic operations for Controllers
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
    OR
    -- Basic employee operations (view only)
    (EntityType = 'Employee' AND OperationType = 'View')
    OR
    -- Basic controller operations (view only)
    (EntityType = 'Controller' AND OperationType = 'View')
);

-- Fix Organizational Permissions for Controllers
PRINT 'Fixing organizational permissions for Controllers...';

-- Enable view permissions for all organizational entities
UPDATE UserOrganizationalPermissions 
SET CanView = 1
WHERE UserId IN (
    SELECT UserId FROM users WHERE RoleName = 'Controller'
) AND IsActive = 1;

-- Keep edit/delete/create disabled for Controllers
UPDATE UserOrganizationalPermissions 
SET 
    CanEdit = 0,
    CanDelete = 0,
    CanCreate = 0
WHERE UserId IN (
    SELECT UserId FROM users WHERE RoleName = 'Controller'
) AND IsActive = 1;

PRINT '=== VERIFICATION ===';

-- Check operation permissions for Controllers
SELECT 
    u.Username,
    u.RoleName,
    uop.EntityType,
    uop.OperationType,
    uop.IsAllowed
FROM users u
JOIN UserOperationPermissions uop ON u.UserId = uop.UserId
WHERE u.RoleName = 'Controller'
AND uop.IsActive = 1
AND uop.IsAllowed = 1
ORDER BY u.Username, uop.EntityType, uop.OperationType;

-- Check organizational permissions for Controllers
SELECT 
    u.Username,
    u.RoleName,
    uop.PermissionType,
    uop.EntityName,
    uop.CanView,
    uop.CanEdit,
    uop.CanDelete,
    uop.CanCreate
FROM users u
JOIN UserOrganizationalPermissions uop ON u.UserId = uop.UserId
WHERE u.RoleName = 'Controller'
AND uop.IsActive = 1
ORDER BY u.Username, uop.PermissionType, uop.EntityName;

PRINT '=== SCRIPT COMPLETED SUCCESSFULLY ===';
PRINT 'Controller permissions have been fixed!';
PRINT 'Controllers now have appropriate view permissions for operations and organizational structure.';