-- Check yazeed.bassam's menu permissions in detail
-- Get user ID first
SELECT UserId, UserName FROM Users WHERE LOWER(UserName) = 'yazeed.bassam';

-- Check UserMenuPermissions table
SELECT 
    u.UserName,
    ump.MenuKey,
    ump.IsVisible,
    ump.IsActive,
    ump.CreatedDate,
    ump.UpdatedDate
FROM Users u
INNER JOIN UserMenuPermissions ump ON u.UserId = ump.UserId
WHERE LOWER(u.UserName) = 'yazeed.bassam'
ORDER BY ump.MenuKey;

-- Check if user has any other permissions that might affect menu visibility
SELECT 
    u.UserName,
    uop.EntityType,
    uop.OperationType,
    uop.IsAllowed,
    uop.Scope
FROM Users u
INNER JOIN UserOperationPermissions uop ON u.UserId = uop.UserId
WHERE LOWER(u.UserName) = 'yazeed.bassam' AND uop.IsAllowed = 1;

-- Check if user has any organizational permissions
SELECT 
    u.UserName,
    uorg.PermissionType,
    uorg.PermissionId,
    uorg.CanView,
    uorg.CanEdit,
    uorg.CanDelete,
    uorg.CanCreate
FROM Users u
INNER JOIN UserOrganizationalPermissions uorg ON u.UserId = uorg.UserId
WHERE LOWER(u.UserName) = 'yazeed.bassam' 
AND (uorg.CanView = 1 OR uorg.CanEdit = 1 OR uorg.CanDelete = 1 OR uorg.CanCreate = 1);

-- Check all menu permissions for comparison
SELECT MenuKey, COUNT(*) as TotalUsers, 
       SUM(CASE WHEN IsVisible = 1 THEN 1 ELSE 0 END) as UsersWithAccess
FROM UserMenuPermissions 
WHERE IsActive = 1
GROUP BY MenuKey
ORDER BY MenuKey;