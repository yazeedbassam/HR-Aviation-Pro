-- Add new menu permissions for ORGANIZATION_VIEW and TRAINING_VIEW
-- This script adds the new menu permissions to the UserMenuPermissions table

-- First, let's check if these permissions already exist
SELECT DISTINCT MenuKey FROM UserMenuPermissions WHERE MenuKey IN ('ORGANIZATION_VIEW', 'TRAINING_VIEW');

-- Add ORGANIZATION_VIEW permission for all users (default: false)
INSERT INTO UserMenuPermissions (UserId, MenuKey, IsVisible, IsActive, CreatedAt, UpdatedAt)
SELECT 
    u.UserId,
    'ORGANIZATION_VIEW',
    0, -- Default to false (not visible)
    1, -- Active
    GETDATE(),
    GETDATE()
FROM Users u
WHERE NOT EXISTS (
    SELECT 1 FROM UserMenuPermissions ump 
    WHERE ump.UserId = u.UserId AND ump.MenuKey = 'ORGANIZATION_VIEW'
);

-- Add TRAINING_VIEW permission for all users (default: false)
INSERT INTO UserMenuPermissions (UserId, MenuKey, IsVisible, IsActive, CreatedAt, UpdatedAt)
SELECT 
    u.UserId,
    'TRAINING_VIEW',
    0, -- Default to false (not visible)
    1, -- Active
    GETDATE(),
    GETDATE()
FROM Users u
WHERE NOT EXISTS (
    SELECT 1 FROM UserMenuPermissions ump 
    WHERE ump.UserId = u.UserId AND ump.MenuKey = 'TRAINING_VIEW'
);

-- Verify the permissions were added
SELECT 
    u.UserName,
    ump.MenuKey,
    ump.IsVisible,
    ump.IsActive
FROM Users u
INNER JOIN UserMenuPermissions ump ON u.UserId = ump.UserId
WHERE ump.MenuKey IN ('ORGANIZATION_VIEW', 'TRAINING_VIEW')
ORDER BY u.UserName, ump.MenuKey;

-- Show count of permissions per user
SELECT 
    u.UserName,
    COUNT(*) as TotalMenuPermissions,
    SUM(CASE WHEN ump.IsVisible = 1 THEN 1 ELSE 0 END) as VisiblePermissions
FROM Users u
INNER JOIN UserMenuPermissions ump ON u.UserId = ump.UserId
WHERE ump.IsActive = 1
GROUP BY u.UserId, u.UserName
ORDER BY u.UserName;