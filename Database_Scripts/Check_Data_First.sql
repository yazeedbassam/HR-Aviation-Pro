USE [HR-Aviation]
GO

-- Check if user exists
SELECT 'User Check' as CheckType, username, userid, rolename 
FROM users 
WHERE username = 'yazeed.bassam'

-- Check if Queen department exists
SELECT 'Department Check' as CheckType, ValueId, ValueText, CategoryName
FROM ConfigurationValues cv
JOIN ConfigurationCategories cc ON cv.CategoryId = cc.CategoryId
WHERE cv.ValueText = 'Queen' AND cv.IsActive = 1

-- Check existing permissions
SELECT 'Permissions Check' as CheckType, PermissionId, PermissionName, PermissionKey, CategoryName
FROM Permissions
WHERE PermissionKey IN ('Add Certificate', 'Add License', 'Add Controller', 'Add Employee', 'Add Division')
AND IsActive = 1

-- Check existing user permissions
SELECT 'User Permissions Check' as CheckType, 
       u.username,
       dept.ValueText as DepartmentName,
       p.PermissionName,
       udp.CanView,
       udp.CanEdit,
       udp.CanDelete
FROM UserDepartmentPermissions udp
JOIN users u ON udp.UserId = u.userid
JOIN ConfigurationValues dept ON udp.DepartmentId = dept.ValueId
JOIN Permissions p ON udp.PermissionId = p.PermissionId
WHERE u.username = 'yazeed.bassam'
ORDER BY p.PermissionName 
