-- Check operation permissions for user yazeed.bassam (userId = 1057)
-- This script will help identify the discrepancy between the main page and detail page

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

-- 2. Check operation permissions count (same as main page query)
SELECT 
    COUNT(DISTINCT CASE WHEN uop.IsAllowed = 1 THEN uop.UserOperationPermissionId END) as OperationPermissionsCount
FROM users u
LEFT JOIN UserOperationPermissions uop ON u.userid = uop.UserId AND uop.IsActive = 1
WHERE u.userid = 1057;

-- 3. Check all operation permissions for this user
SELECT 
    uop.UserOperationPermissionId,
    uop.EntityType,
    uop.OperationType,
    uop.IsAllowed,
    uop.Scope,
    uop.ScopeId,
    p.PermissionName,
    p.PermissionDescription,
    uop.IsActive
FROM UserOperationPermissions uop
INNER JOIN Permissions p ON uop.PermissionId = p.PermissionId
WHERE uop.UserId = 1057 AND uop.IsActive = 1 AND p.IsActive = 1
ORDER BY uop.EntityType, uop.OperationType;

-- 4. Check if there are any inactive permissions
SELECT 
    uop.UserOperationPermissionId,
    uop.EntityType,
    uop.OperationType,
    uop.IsAllowed,
    uop.IsActive,
    p.IsActive as PermissionIsActive
FROM UserOperationPermissions uop
INNER JOIN Permissions p ON uop.PermissionId = p.PermissionId
WHERE uop.UserId = 1057
ORDER BY uop.IsActive DESC, uop.EntityType, uop.OperationType;

-- 5. Check all possible permissions that should exist (25 total)
SELECT 
    et.EntityType,
    ot.OperationType,
    CASE 
        WHEN uop.UserOperationPermissionId IS NOT NULL THEN 'EXISTS'
        ELSE 'MISSING'
    END as Status,
    uop.IsAllowed,
    uop.IsActive
FROM (
    SELECT 'Employee' as EntityType
    UNION SELECT 'Controller'
    UNION SELECT 'License'
    UNION SELECT 'Certificate'
    UNION SELECT 'Observation'
) et
CROSS JOIN (
    SELECT 'View' as OperationType
    UNION SELECT 'Add'
    UNION SELECT 'Edit'
    UNION SELECT 'Delete'
    UNION SELECT 'Export'
) ot
LEFT JOIN UserOperationPermissions uop ON uop.UserId = 1057 
    AND uop.EntityType = et.EntityType 
    AND uop.OperationType = ot.OperationType
    AND uop.IsActive = 1
ORDER BY et.EntityType, ot.OperationType;