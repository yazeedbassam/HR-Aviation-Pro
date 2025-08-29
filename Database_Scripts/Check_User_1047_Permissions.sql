USE [HR-Aviation]
GO

-- Check user 1047 (yazeed.bassam) details
SELECT 'User Details' as CheckType, userid, username, rolename, department
FROM users 
WHERE userid = 1047

-- Check if user 1047 has any permissions
SELECT 'User Permissions' as CheckType, 
       udp.UserDepartmentPermissionId,
       udp.UserId,
       udp.DepartmentId,
       udp.PermissionId,
       udp.CanView,
       udp.CanEdit,
       udp.CanDelete,
       udp.IsActive
FROM UserDepartmentPermissions udp
WHERE udp.UserId = 1047

-- Check specific permissions for user 1047
SELECT 'Detailed Permissions' as CheckType,
       u.username as UserName,
       dept.ValueText as DepartmentName,
       p.PermissionName,
       p.PermissionKey,
       udp.CanView,
       udp.CanEdit,
       udp.CanDelete,
       udp.IsActive
FROM UserDepartmentPermissions udp
JOIN users u ON udp.UserId = u.userid
JOIN ConfigurationValues dept ON udp.DepartmentId = dept.ValueId
JOIN Permissions p ON udp.PermissionId = p.PermissionId
WHERE u.userid = 1047
ORDER BY p.PermissionName

-- Check if required permissions exist
SELECT 'Required Permissions Check' as CheckType,
       PermissionId,
       PermissionName,
       PermissionKey,
       CategoryName,
       IsActive
FROM Permissions
WHERE PermissionKey IN ('Add Certificate', 'Add License', 'Add Controller', 'Add Employee', 'Add Division')
AND IsActive = 1

-- Check Queen department
SELECT 'Queen Department' as CheckType,
       ValueId,
       ValueText,
       CategoryName
FROM ConfigurationValues cv
JOIN ConfigurationCategories cc ON cv.CategoryId = cc.CategoryId
WHERE cv.ValueText = 'Queen' AND cv.IsActive = 1 
