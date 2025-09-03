-- Fix Organizational Permissions for User yazeed bassam (userId=1057)
-- This script grants full organizational permissions to the user

USE [HR-Aviation];
GO

-- First, let's check what organizational entities exist
PRINT '=== CHECKING EXISTING ORGANIZATIONAL ENTITIES ===';

-- Check Countries
SELECT 'Countries' as EntityType, COUNT(*) as Count FROM Countries;
SELECT TOP 5 CountryId, CountryName FROM Countries;

-- Check Airports  
SELECT 'Airports' as EntityType, COUNT(*) as Count FROM Airports;
SELECT TOP 5 AirportId, AirportName, CountryId FROM Airports;

-- Check if user exists
SELECT 'User Check' as Info, UserId, Username, RoleName FROM users WHERE UserId = 1057;

PRINT '=== GRANTING ORGANIZATIONAL PERMISSIONS ===';

-- Grant permissions for all Countries
INSERT INTO UserOrganizationalPermissions (UserId, PermissionType, EntityId, EntityName, CanView, CanEdit, CanDelete, CanCreate, IsActive, CreatedAt, UpdatedAt)
SELECT 
    1057 as UserId,
    'Country' as PermissionType,
    CountryId as EntityId,
    CountryName as EntityName,
    1 as CanView,
    1 as CanEdit, 
    1 as CanDelete,
    1 as CanCreate,
    1 as IsActive,
    GETDATE() as CreatedAt,
    GETDATE() as UpdatedAt
FROM Countries
WHERE NOT EXISTS (
    SELECT 1 FROM UserOrganizationalPermissions 
    WHERE UserId = 1057 AND PermissionType = 'Country' AND EntityId = Countries.CountryId
);

PRINT 'Granted Country permissions: ' + CAST(@@ROWCOUNT as VARCHAR(10));

-- Grant permissions for all Airports
INSERT INTO UserOrganizationalPermissions (UserId, PermissionType, EntityId, EntityName, CanView, CanEdit, CanDelete, CanCreate, IsActive, CreatedAt, UpdatedAt)
SELECT 
    1057 as UserId,
    'Airport' as PermissionType,
    AirportId as EntityId,
    AirportName as EntityName,
    1 as CanView,
    1 as CanEdit,
    1 as CanDelete, 
    1 as CanCreate,
    1 as IsActive,
    GETDATE() as CreatedAt,
    GETDATE() as UpdatedAt
FROM Airports
WHERE NOT EXISTS (
    SELECT 1 FROM UserOrganizationalPermissions 
    WHERE UserId = 1057 AND PermissionType = 'Airport' AND EntityId = Airports.AirportId
);

PRINT 'Granted Airport permissions: ' + CAST(@@ROWCOUNT as VARCHAR(10));

-- Grant permissions for all Departments (if they exist)
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Departments')
BEGIN
    INSERT INTO UserOrganizationalPermissions (UserId, PermissionType, EntityId, EntityName, CanView, CanEdit, CanDelete, CanCreate, IsActive, CreatedAt, UpdatedAt)
    SELECT 
        1057 as UserId,
        'Department' as PermissionType,
        DepartmentId as EntityId,
        DepartmentName as EntityName,
        1 as CanView,
        1 as CanEdit,
        1 as CanDelete,
        1 as CanCreate,
        1 as IsActive,
        GETDATE() as CreatedAt,
        GETDATE() as UpdatedAt
    FROM Departments
    WHERE NOT EXISTS (
        SELECT 1 FROM UserOrganizationalPermissions 
        WHERE UserId = 1057 AND PermissionType = 'Department' AND EntityId = Departments.DepartmentId
    );
    
    PRINT 'Granted Department permissions: ' + CAST(@@ROWCOUNT as VARCHAR(10));
END
ELSE
BEGIN
    PRINT 'Departments table does not exist - skipping';
END

-- Grant permissions for all Divisions (if they exist)
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Divisions')
BEGIN
    INSERT INTO UserOrganizationalPermissions (UserId, PermissionType, EntityId, EntityName, CanView, CanEdit, CanDelete, CanCreate, IsActive, CreatedAt, UpdatedAt)
    SELECT 
        1057 as UserId,
        'Division' as PermissionType,
        DivisionId as EntityId,
        DivisionName as EntityName,
        1 as CanView,
        1 as CanEdit,
        1 as CanDelete,
        1 as CanCreate,
        1 as IsActive,
        GETDATE() as CreatedAt,
        GETDATE() as UpdatedAt
    FROM Divisions
    WHERE NOT EXISTS (
        SELECT 1 FROM UserOrganizationalPermissions 
        WHERE UserId = 1057 AND PermissionType = 'Division' AND EntityId = Divisions.DivisionId
    );
    
    PRINT 'Granted Division permissions: ' + CAST(@@ROWCOUNT as VARCHAR(10));
END
ELSE
BEGIN
    PRINT 'Divisions table does not exist - skipping';
END

PRINT '=== VERIFICATION ===';

-- Check final count of organizational permissions for user 1057
SELECT 
    'Final Count' as Info,
    COUNT(*) as TotalPermissions,
    SUM(CASE WHEN PermissionType = 'Country' THEN 1 ELSE 0 END) as CountryPermissions,
    SUM(CASE WHEN PermissionType = 'Airport' THEN 1 ELSE 0 END) as AirportPermissions,
    SUM(CASE WHEN PermissionType = 'Department' THEN 1 ELSE 0 END) as DepartmentPermissions,
    SUM(CASE WHEN PermissionType = 'Division' THEN 1 ELSE 0 END) as DivisionPermissions
FROM UserOrganizationalPermissions 
WHERE UserId = 1057 AND IsActive = 1;

-- Show sample permissions
SELECT TOP 10 
    PermissionType, EntityId, EntityName, CanView, CanEdit, CanDelete, CanCreate
FROM UserOrganizationalPermissions 
WHERE UserId = 1057 AND IsActive = 1
ORDER BY PermissionType, EntityId;

PRINT '=== SCRIPT COMPLETED SUCCESSFULLY ===';
PRINT 'User yazeed bassam (1057) now has full organizational permissions!';