-- =====================================================
-- ADD MISSING MENU PERMISSIONS FOR SIDEBAR
-- =====================================================
-- This script adds the missing permissions that control sidebar menu visibility
-- The sidebar checks for specific permissions like ORGANIZATION_VIEW, STAFF_VIEW, etc.

USE [HR-Aviation];
GO

PRINT '=== ADDING MISSING MENU PERMISSIONS FOR SIDEBAR ===';

-- Add missing permissions to the Permissions table if they don't exist
PRINT 'Adding missing permissions to Permissions table...';

-- Organization permissions
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE PermissionKey = 'ORGANIZATION_VIEW')
    INSERT INTO Permissions (PermissionKey, PermissionName, CategoryName, IsActive, CreatedAt, UpdatedAt)
    VALUES ('ORGANIZATION_VIEW', 'View Organization Structure', 'Organization', 1, GETDATE(), GETDATE());

IF NOT EXISTS (SELECT 1 FROM Permissions WHERE PermissionKey = 'STRUCTURE_VIEW')
    INSERT INTO Permissions (PermissionKey, PermissionName, CategoryName, IsActive, CreatedAt, UpdatedAt)
    VALUES ('STRUCTURE_VIEW', 'View Structure', 'Organization', 1, GETDATE(), GETDATE());

IF NOT EXISTS (SELECT 1 FROM Permissions WHERE PermissionKey = 'DIVISIONS_VIEW')
    INSERT INTO Permissions (PermissionKey, PermissionName, CategoryName, IsActive, CreatedAt, UpdatedAt)
    VALUES ('DIVISIONS_VIEW', 'View Divisions', 'Organization', 1, GETDATE(), GETDATE());

-- Staff permissions
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE PermissionKey = 'STAFF_VIEW')
    INSERT INTO Permissions (PermissionKey, PermissionName, CategoryName, IsActive, CreatedAt, UpdatedAt)
    VALUES ('STAFF_VIEW', 'View Staff', 'Staff', 1, GETDATE(), GETDATE());

IF NOT EXISTS (SELECT 1 FROM Permissions WHERE PermissionKey = 'CONTROLLERS_VIEW')
    INSERT INTO Permissions (PermissionKey, PermissionName, CategoryName, IsActive, CreatedAt, UpdatedAt)
    VALUES ('CONTROLLERS_VIEW', 'View Controllers', 'Staff', 1, GETDATE(), GETDATE());

-- Document permissions (these already exist, but let's make sure)
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE PermissionKey = 'LICENSES_VIEW')
    INSERT INTO Permissions (PermissionKey, PermissionName, CategoryName, IsActive, CreatedAt, UpdatedAt)
    VALUES ('LICENSES_VIEW', 'View Licenses', 'Documents', 1, GETDATE(), GETDATE());

IF NOT EXISTS (SELECT 1 FROM Permissions WHERE PermissionKey = 'CERTIFICATES_VIEW')
    INSERT INTO Permissions (PermissionKey, PermissionName, CategoryName, IsActive, CreatedAt, UpdatedAt)
    VALUES ('CERTIFICATES_VIEW', 'View Certificates', 'Documents', 1, GETDATE(), GETDATE());

IF NOT EXISTS (SELECT 1 FROM Permissions WHERE PermissionKey = 'OBSERVATIONS_VIEW')
    INSERT INTO Permissions (PermissionKey, PermissionName, CategoryName, IsActive, CreatedAt, UpdatedAt)
    VALUES ('OBSERVATIONS_VIEW', 'View Observations', 'Activities', 1, GETDATE(), GETDATE());

PRINT 'Permissions added to Permissions table.';

-- Now assign these permissions to users based on their roles
PRINT 'Assigning permissions to users based on their roles...';

-- Admin gets all permissions
INSERT INTO UserOperationPermissions (UserId, PermissionId, EntityType, OperationType, IsAllowed, Scope, ScopeId, IsActive, CreatedAt, UpdatedAt)
SELECT 
    u.UserId,
    p.PermissionId,
    p.PermissionKey,
    'View',
    1,  -- Admin gets all permissions
    'All',
    NULL,
    1,
    GETDATE(),
    GETDATE()
FROM users u
CROSS JOIN Permissions p
WHERE u.RoleName = 'Admin'
AND p.PermissionKey IN ('ORGANIZATION_VIEW', 'STRUCTURE_VIEW', 'DIVISIONS_VIEW', 'STAFF_VIEW', 'CONTROLLERS_VIEW', 'LICENSES_VIEW', 'CERTIFICATES_VIEW', 'OBSERVATIONS_VIEW')
AND NOT EXISTS (
    SELECT 1 FROM UserOperationPermissions uop 
    WHERE uop.UserId = u.UserId 
    AND uop.EntityType = p.PermissionKey
);

-- Controllers get limited permissions
INSERT INTO UserOperationPermissions (UserId, PermissionId, EntityType, OperationType, IsAllowed, Scope, ScopeId, IsActive, CreatedAt, UpdatedAt)
SELECT 
    u.UserId,
    p.PermissionId,
    p.PermissionKey,
    'View',
    1,  -- Controllers can view these sections
    'All',
    NULL,
    1,
    GETDATE(),
    GETDATE()
FROM users u
CROSS JOIN Permissions p
WHERE u.RoleName = 'Controller'
AND p.PermissionKey IN ('ORGANIZATION_VIEW', 'STRUCTURE_VIEW', 'DIVISIONS_VIEW', 'STAFF_VIEW', 'CONTROLLERS_VIEW', 'LICENSES_VIEW', 'CERTIFICATES_VIEW', 'OBSERVATIONS_VIEW')
AND NOT EXISTS (
    SELECT 1 FROM UserOperationPermissions uop 
    WHERE uop.UserId = u.UserId 
    AND uop.EntityType = p.PermissionKey
);

-- Employees get basic permissions
INSERT INTO UserOperationPermissions (UserId, PermissionId, EntityType, OperationType, IsAllowed, Scope, ScopeId, IsActive, CreatedAt, UpdatedAt)
SELECT 
    u.UserId,
    p.PermissionId,
    p.PermissionKey,
    'View',
    1,  -- Employees can view basic sections
    'All',
    NULL,
    1,
    GETDATE(),
    GETDATE()
FROM users u
CROSS JOIN Permissions p
WHERE u.RoleName = 'Employee'
AND p.PermissionKey IN ('ORGANIZATION_VIEW', 'STRUCTURE_VIEW', 'DIVISIONS_VIEW', 'LICENSES_VIEW', 'CERTIFICATES_VIEW')
AND NOT EXISTS (
    SELECT 1 FROM UserOperationPermissions uop 
    WHERE uop.UserId = u.UserId 
    AND uop.EntityType = p.PermissionKey
);

PRINT '=== VERIFICATION ===';

-- Check what permissions each user now has
SELECT 
    u.Username,
    u.RoleName,
    uop.EntityType,
    uop.IsAllowed
FROM users u
JOIN UserOperationPermissions uop ON u.UserId = uop.UserId
WHERE uop.EntityType IN ('ORGANIZATION_VIEW', 'STRUCTURE_VIEW', 'DIVISIONS_VIEW', 'STAFF_VIEW', 'CONTROLLERS_VIEW', 'LICENSES_VIEW', 'CERTIFICATES_VIEW', 'OBSERVATIONS_VIEW')
AND uop.IsActive = 1
ORDER BY u.Username, uop.EntityType;

PRINT '=== SCRIPT COMPLETED SUCCESSFULLY ===';
PRINT 'Missing menu permissions have been added!';
PRINT 'Users should now see the appropriate sidebar menu items.';