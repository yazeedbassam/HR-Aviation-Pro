USE [HR-Aviation]
GO

-- Step 1: Add missing permissions
INSERT INTO Permissions (PermissionName, PermissionKey, PermissionDescription, CategoryName, IsActive, CreatedAt)
SELECT 'Add Certificate', 'Add Certificate', 'Permission to add new certificates', 'Certificates', 1, GETDATE()
WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE PermissionKey = 'Add Certificate')

INSERT INTO Permissions (PermissionName, PermissionKey, PermissionDescription, CategoryName, IsActive, CreatedAt)
SELECT 'Add License', 'Add License', 'Permission to add new licenses', 'Licenses', 1, GETDATE()
WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE PermissionKey = 'Add License')

INSERT INTO Permissions (PermissionName, PermissionKey, PermissionDescription, CategoryName, IsActive, CreatedAt)
SELECT 'Add Controller', 'Add Controller', 'Permission to add new controllers', 'Controllers', 1, GETDATE()
WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE PermissionKey = 'Add Controller')

INSERT INTO Permissions (PermissionName, PermissionKey, PermissionDescription, CategoryName, IsActive, CreatedAt)
SELECT 'Add Employee', 'Add Employee', 'Permission to add new employees', 'Employees', 1, GETDATE()
WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE PermissionKey = 'Add Employee')

INSERT INTO Permissions (PermissionName, PermissionKey, PermissionDescription, CategoryName, IsActive, CreatedAt)
SELECT 'Add Division', 'Add Division', 'Permission to add new divisions', 'Divisions', 1, GETDATE()
WHERE NOT EXISTS (SELECT 1 FROM Permissions WHERE PermissionKey = 'Add Division')

-- Step 2: Get user and department IDs safely
DECLARE @UserId int
DECLARE @DepartmentId int

SELECT @UserId = userid FROM users WHERE username = 'yazeed.bassam'
SELECT @DepartmentId = ValueId FROM ConfigurationValues WHERE ValueText = 'Queen' AND IsActive = 1

PRINT 'User ID: ' + CAST(ISNULL(@UserId, 0) AS VARCHAR(10))
PRINT 'Department ID: ' + CAST(ISNULL(@DepartmentId, 0) AS VARCHAR(10))

-- Step 3: Add user permissions only if both IDs are valid
IF @UserId IS NOT NULL AND @DepartmentId IS NOT NULL
BEGIN
    INSERT INTO UserDepartmentPermissions (UserId, DepartmentId, PermissionId, CanView, CanEdit, CanDelete, IsActive)
    SELECT @UserId, @DepartmentId, PermissionId, 1, 1, 0, 1
    FROM Permissions 
    WHERE PermissionKey IN ('Add Certificate', 'Add License', 'Add Controller', 'Add Employee', 'Add Division')
    AND NOT EXISTS (
        SELECT 1 FROM UserDepartmentPermissions udp 
        WHERE udp.UserId = @UserId 
        AND udp.DepartmentId = @DepartmentId 
        AND udp.PermissionId = Permissions.PermissionId
    )
    
    PRINT 'Permissions added successfully for user ' + CAST(@UserId AS VARCHAR(10)) + ' in department ' + CAST(@DepartmentId AS VARCHAR(10))
END
ELSE
BEGIN
    PRINT 'ERROR: User or Department not found!'
    PRINT 'User ID: ' + CAST(ISNULL(@UserId, 0) AS VARCHAR(10))
    PRINT 'Department ID: ' + CAST(ISNULL(@DepartmentId, 0) AS VARCHAR(10))
END

-- Step 4: Show current permissions for the user
SELECT 
    u.username as UserName,
    dept.ValueText as DepartmentName,
    p.PermissionName,
    p.PermissionKey,
    udp.CanView,
    udp.CanEdit,
    udp.CanDelete
FROM UserDepartmentPermissions udp
JOIN users u ON udp.UserId = u.userid
JOIN ConfigurationValues dept ON udp.DepartmentId = dept.ValueId
JOIN Permissions p ON udp.PermissionId = p.PermissionId
WHERE u.username = 'yazeed.bassam'
ORDER BY p.PermissionName 
