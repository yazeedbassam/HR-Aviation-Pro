-- Check menu permissions for user yazeed.bassam (userId = 1057)
-- This script will help identify the discrepancy in menu permissions

-- 1. Check user information
SELECT 
    u.userid,
    u.username,
    u.rolename,
    COALESCE(c.fullname, e.fullname, u.username) as fullname
FROM users u
LEFT JOIN controllers c ON u.userid = c.userid
LEFT JOIN employees e ON u.userid = e.userid
WHERE u.userid = 1057;

-- 2. Check menu permissions count (same as main page query)
SELECT 
    COUNT(DISTINCT CASE WHEN ump.IsVisible = 1 THEN ump.UserMenuPermissionId END) as MenuPermissionsCount
FROM users u
LEFT JOIN UserMenuPermissions ump ON u.userid = ump.UserId AND ump.IsActive = 1
WHERE u.userid = 1057;

-- 3. Check all menu permissions for this user
SELECT 
    ump.UserMenuPermissionId,
    ump.MenuKey,
    ump.IsVisible,
    ump.IsActive,
    ump.CreatedAt,
    ump.UpdatedAt
FROM UserMenuPermissions ump
WHERE ump.UserId = 1057
ORDER BY ump.MenuKey;

-- 4. Check all possible menu permissions that should exist (10 total)
SELECT 
    menuKey,
    CASE 
        WHEN ump.UserMenuPermissionId IS NOT NULL THEN 'EXISTS'
        ELSE 'MISSING'
    END as Status,
    ump.IsVisible,
    ump.IsActive
FROM (
    SELECT 'PROFILE' as menuKey
    UNION SELECT 'NOTIFICATIONS'
    UNION SELECT 'DASHBOARD'
    UNION SELECT 'EMPLOYEES'
    UNION SELECT 'CONTROLLERS'
    UNION SELECT 'LICENSES'
    UNION SELECT 'CERTIFICATES'
    UNION SELECT 'OBSERVATIONS'
    UNION SELECT 'CONFIGURATION'
    UNION SELECT 'PERMISSIONS'
) allMenus
LEFT JOIN UserMenuPermissions ump ON ump.UserId = 1057 
    AND ump.MenuKey = allMenus.menuKey
    AND ump.IsActive = 1
ORDER BY allMenus.menuKey;

-- 5. Check if there are any inactive menu permissions
SELECT 
    ump.UserMenuPermissionId,
    ump.MenuKey,
    ump.IsVisible,
    ump.IsActive
FROM UserMenuPermissions ump
WHERE ump.UserId = 1057
ORDER BY ump.IsActive DESC, ump.MenuKey;