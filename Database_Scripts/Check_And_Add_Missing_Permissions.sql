-- =====================================================
-- Check and Add Missing Permissions
-- =====================================================

USE [HR-Aviation]
GO

-- Check existing permissions
PRINT '=== Existing Permissions ==='
SELECT PermissionId, PermissionName, PermissionKey, CategoryName, IsActive
FROM Permissions
WHERE IsActive = 1
ORDER BY CategoryName, PermissionName

-- Check if required permissions exist
PRINT '=== Checking Required Permissions ==='
SELECT 
    'Add Certificate' as RequiredPermission,
    CASE WHEN EXISTS (SELECT 1 FROM Permissions WHERE PermissionKey = 'Add Certificate') 
         THEN 'EXISTS' ELSE 'MISSING' END as Status

UNION ALL

SELECT 
    'Add License' as RequiredPermission,
    CASE WHEN EXISTS (SELECT 1 FROM Permissions WHERE PermissionKey = 'Add License') 
         THEN 'EXISTS' ELSE 'MISSING' END as Status

UNION ALL

SELECT 
    'Add Controller' as RequiredPermission,
    CASE WHEN EXISTS (SELECT 1 FROM Permissions WHERE PermissionKey = 'Add Controller') 
         THEN 'EXISTS' ELSE 'MISSING' END as Status

UNION ALL

SELECT 
    'Add Employee' as RequiredPermission,
    CASE WHEN EXISTS (SELECT 1 FROM Permissions WHERE PermissionKey = 'Add Employee') 
         THEN 'EXISTS' ELSE 'MISSING' END as Status

UNION ALL

SELECT 
    'Add Division' as RequiredPermission,
    CASE WHEN EXISTS (SELECT 1 FROM Permissions WHERE PermissionKey = 'Add Division') 
         THEN 'EXISTS' ELSE 'MISSING' END as Status

-- Add missing permissions
PRINT '=== Adding Missing Permissions ==='

-- Add Certificate permission
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE PermissionKey = 'Add Certificate')
BEGIN
    INSERT INTO Permissions (PermissionName, PermissionKey, PermissionDescription, CategoryName, IsActive, CreatedAt)
    VALUES ('Add Certificate', 'Add Certificate', 'Permission to add new certificates', 'Certificates', 1, GETDATE())
    PRINT 'Added Add Certificate permission'
END
ELSE
BEGIN
    PRINT 'Add Certificate permission already exists'
END

-- Add License permission
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE PermissionKey = 'Add License')
BEGIN
    INSERT INTO Permissions (PermissionName, PermissionKey, PermissionDescription, CategoryName, IsActive, CreatedAt)
    VALUES ('Add License', 'Add License', 'Permission to add new licenses', 'Licenses', 1, GETDATE())
    PRINT 'Added Add License permission'
END
ELSE
BEGIN
    PRINT 'Add License permission already exists'
END

-- Add Controller permission
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE PermissionKey = 'Add Controller')
BEGIN
    INSERT INTO Permissions (PermissionName, PermissionKey, PermissionDescription, CategoryName, IsActive, CreatedAt)
    VALUES ('Add Controller', 'Add Controller', 'Permission to add new controllers', 'Controllers', 1, GETDATE())
    PRINT 'Added Add Controller permission'
END
ELSE
BEGIN
    PRINT 'Add Controller permission already exists'
END

-- Add Employee permission
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE PermissionKey = 'Add Employee')
BEGIN
    INSERT INTO Permissions (PermissionName, PermissionKey, PermissionDescription, CategoryName, IsActive, CreatedAt)
    VALUES ('Add Employee', 'Add Employee', 'Permission to add new employees', 'Employees', 1, GETDATE())
    PRINT 'Added Add Employee permission'
END
ELSE
BEGIN
    PRINT 'Add Employee permission already exists'
END

-- Add Division permission
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE PermissionKey = 'Add Division')
BEGIN
    INSERT INTO Permissions (PermissionName, PermissionKey, PermissionDescription, CategoryName, IsActive, CreatedAt)
    VALUES ('Add Division', 'Add Division', 'Permission to add new divisions', 'Divisions', 1, GETDATE())
    PRINT 'Added Add Division permission'
END
ELSE
BEGIN
    PRINT 'Add Division permission already exists'
END

-- Verify permissions were added
PRINT '=== Final Permissions List ==='
SELECT PermissionId, PermissionName, PermissionKey, CategoryName, IsActive
FROM Permissions
WHERE PermissionKey IN ('Add Certificate', 'Add License', 'Add Controller', 'Add Employee', 'Add Division')
ORDER BY PermissionName

GO 
