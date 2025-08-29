-- =====================================================
-- Check User Permissions for yazeed.bassam
-- =====================================================

USE [HR-Aviation]
GO

-- Get user ID for yazeed.bassam
DECLARE @UserId int
SELECT @UserId = userid FROM users WHERE username = 'yazeed.bassam'
PRINT 'User ID for yazeed.bassam: ' + CAST(@UserId AS VARCHAR(10))

-- Check user's role
SELECT username, rolename FROM users WHERE userid = @UserId

-- Check user's department permissions
SELECT 
    udp.UserDepartmentPermissionId,
    u.username as UserName,
    dept.ValueText as DepartmentName,
    p.PermissionName,
    p.PermissionKey,
    udp.CanView,
    udp.CanEdit,
    udp.CanDelete,
    udp.IsActive,
    udp.CreatedAt
FROM UserDepartmentPermissions udp
JOIN users u ON udp.UserId = u.userid
JOIN ConfigurationValues dept ON udp.DepartmentId = dept.ValueId
JOIN Permissions p ON udp.PermissionId = p.PermissionId
WHERE udp.UserId = @UserId
ORDER BY dept.ValueText, p.PermissionName

-- Check specific permissions
SELECT 
    p.PermissionName,
    p.PermissionKey,
    CASE 
        WHEN EXISTS (
            SELECT 1 FROM UserDepartmentPermissions udp 
            WHERE udp.UserId = @UserId 
            AND udp.PermissionId = p.PermissionId 
            AND udp.IsActive = 1
        ) THEN 'YES'
        ELSE 'NO'
    END as HasPermission
FROM Permissions p
WHERE p.PermissionKey IN ('Add Certificate', 'Add License', 'Add Controller', 'Add Employee', 'Add Division')
ORDER BY p.PermissionName

-- Check all permissions for this user
SELECT 
    p.PermissionName,
    p.PermissionKey,
    p.CategoryName,
    COUNT(udp.UserDepartmentPermissionId) as PermissionCount
FROM Permissions p
LEFT JOIN UserDepartmentPermissions udp ON p.PermissionId = udp.PermissionId AND udp.UserId = @UserId AND udp.IsActive = 1
WHERE p.IsActive = 1
GROUP BY p.PermissionId, p.PermissionName, p.PermissionKey, p.CategoryName
ORDER BY p.CategoryName, p.PermissionName

-- Test the stored procedure
EXEC CheckUserPermission @UserId = @UserId, @PermissionKey = 'Add Certificate', @DepartmentId = NULL
EXEC CheckUserPermission @UserId = @UserId, @PermissionKey = 'Add License', @DepartmentId = NULL
EXEC CheckUserPermission @UserId = @UserId, @PermissionKey = 'Add Controller', @DepartmentId = NULL
EXEC CheckUserPermission @UserId = @UserId, @PermissionKey = 'Add Employee', @DepartmentId = NULL

GO 
