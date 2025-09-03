-- =====================================================
-- ADD PROFILE PERMISSION FOR NEW USERS
-- =====================================================
-- This script ensures that new Controller users get access to their profile page

-- First, ensure the PROFILE_VIEW permission exists
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE PermissionKey = 'PROFILE_VIEW')
BEGIN
    PRINT 'Creating PROFILE_VIEW permission...'
    INSERT INTO Permissions (PermissionName, PermissionKey, PermissionDescription, CategoryName, IsActive, CreatedAt)
    VALUES ('Profile View', 'PROFILE_VIEW', 'Can view own profile page', 'Profile', 1, GETDATE());
END

-- Get the PROFILE_VIEW permission ID
DECLARE @ProfilePermissionId INT;
SELECT @ProfilePermissionId = PermissionId FROM Permissions WHERE PermissionKey = 'PROFILE_VIEW';

-- Get the Controller role ID
DECLARE @ControllerRoleId INT;
SELECT @ControllerRoleId = cv.ValueId 
FROM ConfigurationValues cv 
JOIN ConfigurationCategories cc ON cv.CategoryId = cc.CategoryId 
WHERE cc.CategoryName = 'Roles' AND cv.ValueText = 'Controller';

-- Add PROFILE_VIEW permission to Controller role if not already exists
IF @ControllerRoleId IS NOT NULL AND @ProfilePermissionId IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM RolePermissions WHERE RoleId = @ControllerRoleId AND PermissionId = @ProfilePermissionId)
    BEGIN
        PRINT 'Adding PROFILE_VIEW permission to Controller role...'
        INSERT INTO RolePermissions (RoleId, PermissionId, IsActive, CreatedAt)
        VALUES (@ControllerRoleId, @ProfilePermissionId, 1, GETDATE());
    END
    ELSE
    BEGIN
        PRINT 'PROFILE_VIEW permission already exists for Controller role.'
    END
END

-- Also add PROFILE_VIEW permission to Employee role
DECLARE @EmployeeRoleId INT;
SELECT @EmployeeRoleId = cv.ValueId 
FROM ConfigurationValues cv 
JOIN ConfigurationCategories cc ON cv.CategoryId = cc.CategoryId 
WHERE cc.CategoryName = 'Roles' AND cv.ValueText = 'Employee';

IF @EmployeeRoleId IS NOT NULL AND @ProfilePermissionId IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM RolePermissions WHERE RoleId = @EmployeeRoleId AND PermissionId = @ProfilePermissionId)
    BEGIN
        PRINT 'Adding PROFILE_VIEW permission to Employee role...'
        INSERT INTO RolePermissions (RoleId, PermissionId, IsActive, CreatedAt)
        VALUES (@EmployeeRoleId, @ProfilePermissionId, 1, GETDATE());
    END
END

-- Verify the changes
PRINT 'Controller role permissions after update:'
SELECT p.PermissionKey, p.PermissionName, p.CategoryName
FROM RolePermissions rp
JOIN Permissions p ON rp.PermissionId = p.PermissionId
JOIN ConfigurationValues cv ON rp.RoleId = cv.ValueId
JOIN ConfigurationCategories cc ON cv.CategoryId = cc.CategoryId
WHERE cc.CategoryName = 'Roles' AND cv.ValueText = 'Controller'
AND rp.IsActive = 1
ORDER BY p.PermissionKey;

PRINT 'Employee role permissions after update:'
SELECT p.PermissionKey, p.PermissionName, p.CategoryName
FROM RolePermissions rp
JOIN Permissions p ON rp.PermissionId = p.PermissionId
JOIN ConfigurationValues cv ON rp.RoleId = cv.ValueId
JOIN ConfigurationCategories cc ON cv.CategoryId = cc.CategoryId
WHERE cc.CategoryName = 'Roles' AND cv.ValueText = 'Employee'
AND rp.IsActive = 1
ORDER BY p.PermissionKey;

PRINT 'Profile permission setup completed successfully!'