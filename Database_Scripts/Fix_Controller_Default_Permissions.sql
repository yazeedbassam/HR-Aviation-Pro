-- =====================================================
-- FIX CONTROLLER DEFAULT PERMISSIONS
-- =====================================================
-- This script fixes the issue where new Controller users get too many permissions by default
-- They should only have access to their profile page

-- First, let's check what permissions Controller role currently has
PRINT 'Current Controller role permissions:'
SELECT p.PermissionKey, p.PermissionName, p.CategoryName
FROM RolePermissions rp
JOIN Permissions p ON rp.PermissionId = p.PermissionId
JOIN ConfigurationValues cv ON rp.RoleId = cv.ValueId
JOIN ConfigurationCategories cc ON cv.CategoryId = cc.CategoryId
WHERE cc.CategoryName = 'Roles' AND cv.ValueText = 'Controller'
AND rp.IsActive = 1
ORDER BY p.PermissionKey;

-- Remove all existing permissions for Controller role
DECLARE @ControllerRoleId INT;
SELECT @ControllerRoleId = cv.ValueId 
FROM ConfigurationValues cv 
JOIN ConfigurationCategories cc ON cv.CategoryId = cc.CategoryId 
WHERE cc.CategoryName = 'Roles' AND cv.ValueText = 'Controller';

IF @ControllerRoleId IS NOT NULL
BEGIN
    PRINT 'Removing all existing permissions for Controller role...'
    DELETE FROM RolePermissions WHERE RoleId = @ControllerRoleId;
    
    -- Add only basic permissions for Controller role
    PRINT 'Adding minimal permissions for Controller role...'
    INSERT INTO RolePermissions (RoleId, PermissionId, IsActive, CreatedAt)
    SELECT @ControllerRoleId, PermissionId, 1, GETDATE()
    FROM Permissions
    WHERE IsActive = 1
    AND PermissionKey IN (
        'PROFILE_VIEW'  -- Only allow access to profile page
    )
    AND PermissionId NOT IN (
        SELECT PermissionId 
        FROM RolePermissions 
        WHERE RoleId = @ControllerRoleId
    );
    
    PRINT 'Controller role permissions updated successfully!'
END
ELSE
BEGIN
    PRINT 'ERROR: Controller role not found in ConfigurationValues!'
END

-- Verify the changes
PRINT 'Updated Controller role permissions:'
SELECT p.PermissionKey, p.PermissionName, p.CategoryName
FROM RolePermissions rp
JOIN Permissions p ON rp.PermissionId = p.PermissionId
JOIN ConfigurationValues cv ON rp.RoleId = cv.ValueId
JOIN ConfigurationCategories cc ON cv.CategoryId = cc.CategoryId
WHERE cc.CategoryName = 'Roles' AND cv.ValueText = 'Controller'
AND rp.IsActive = 1
ORDER BY p.PermissionKey;

-- Also, let's ensure the PROFILE_VIEW permission exists
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE PermissionKey = 'PROFILE_VIEW')
BEGIN
    PRINT 'Creating PROFILE_VIEW permission...'
    INSERT INTO Permissions (PermissionName, PermissionKey, PermissionDescription, CategoryName, IsActive, CreatedAt)
    VALUES ('Profile View', 'PROFILE_VIEW', 'Can view own profile page', 'Profile', 1, GETDATE());
END

PRINT 'Fix completed successfully!'
PRINT 'New Controller users will now only have access to their profile page.'