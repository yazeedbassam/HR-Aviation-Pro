-- =====================================================
-- FIX ADMIN ORGANIZATIONAL PERMISSIONS
-- =====================================================
-- This script adds organizational structure permissions to admin user
-- =====================================================

USE [HR-Aviation]
GO

PRINT '========================================'
PRINT 'Adding Organizational Permissions to Admin'
PRINT '========================================'

-- =====================================================
-- STEP 1: ENSURE UserOrganizationalPermissions TABLE EXISTS
-- =====================================================

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='UserOrganizationalPermissions' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[UserOrganizationalPermissions](
        [UserOrganizationalPermissionId] [int] IDENTITY(1,1) NOT NULL,
        [UserId] [int] NOT NULL,
        [PermissionType] [nvarchar](50) NOT NULL,
        [EntityId] [int] NOT NULL,
        [EntityName] [nvarchar](200) NOT NULL,
        [CanView] [bit] NOT NULL DEFAULT 0,
        [CanEdit] [bit] NOT NULL DEFAULT 0,
        [CanDelete] [bit] NOT NULL DEFAULT 0,
        [CanCreate] [bit] NOT NULL DEFAULT 0,
        [IsActive] [bit] NOT NULL DEFAULT 1,
        [CreatedAt] [datetime] NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] [datetime] NULL,
        CONSTRAINT [PK_UserOrganizationalPermissions] PRIMARY KEY CLUSTERED ([UserOrganizationalPermissionId] ASC),
        CONSTRAINT [UK_UserOrganizationalPermissions_User_Type_Entity] UNIQUE ([UserId], [PermissionType], [EntityId])
    );
    PRINT 'Created UserOrganizationalPermissions table'
END

-- =====================================================
-- STEP 2: GET ADMIN USER ID
-- =====================================================

DECLARE @AdminUserId INT
SELECT @AdminUserId = UserId FROM users WHERE Username = 'admin'

IF @AdminUserId IS NULL
BEGIN
    PRINT 'ERROR: Admin user not found!'
    RETURN
END

PRINT 'Admin User ID: ' + CAST(@AdminUserId AS VARCHAR(10))

-- =====================================================
-- STEP 3: ADD ORGANIZATIONAL PERMISSIONS FOR COUNTRIES
-- =====================================================

-- Check if Countries table exists and add permissions
IF EXISTS (SELECT * FROM sysobjects WHERE name='Countries' AND xtype='U')
BEGIN
    PRINT 'Adding Country permissions for admin...'
    
    INSERT INTO UserOrganizationalPermissions (UserId, PermissionType, EntityId, EntityName, CanView, CanEdit, CanDelete, CanCreate, IsActive, CreatedAt)
    SELECT 
        @AdminUserId,
        'Country' as PermissionType,
        CountryId as EntityId,
        CountryName as EntityName,
        1 as CanView,
        1 as CanEdit,
        1 as CanDelete,
        1 as CanCreate,
        1 as IsActive,
        GETDATE() as CreatedAt
    FROM Countries
    WHERE NOT EXISTS (
        SELECT 1 FROM UserOrganizationalPermissions uop 
        WHERE uop.UserId = @AdminUserId 
        AND uop.PermissionType = 'Country'
        AND uop.EntityId = Countries.CountryId
    )
    
    PRINT 'Country permissions added successfully!'
END
ELSE
BEGIN
    PRINT 'WARNING: Countries table does not exist'
END

-- =====================================================
-- STEP 4: ADD ORGANIZATIONAL PERMISSIONS FOR AIRPORTS
-- =====================================================

-- Check if Airports table exists and add permissions
IF EXISTS (SELECT * FROM sysobjects WHERE name='Airports' AND xtype='U')
BEGIN
    PRINT 'Adding Airport permissions for admin...'
    
    INSERT INTO UserOrganizationalPermissions (UserId, PermissionType, EntityId, EntityName, CanView, CanEdit, CanDelete, CanCreate, IsActive, CreatedAt)
    SELECT 
        @AdminUserId,
        'Airport' as PermissionType,
        AirportId as EntityId,
        AirportName as EntityName,
        1 as CanView,
        1 as CanEdit,
        1 as CanDelete,
        1 as CanCreate,
        1 as IsActive,
        GETDATE() as CreatedAt
    FROM Airports
    WHERE NOT EXISTS (
        SELECT 1 FROM UserOrganizationalPermissions uop 
        WHERE uop.UserId = @AdminUserId 
        AND uop.PermissionType = 'Airport'
        AND uop.EntityId = Airports.AirportId
    )
    
    PRINT 'Airport permissions added successfully!'
END
ELSE
BEGIN
    PRINT 'WARNING: Airports table does not exist'
END

-- =====================================================
-- STEP 5: ADD ORGANIZATIONAL PERMISSIONS FOR DEPARTMENTS
-- =====================================================

-- Check if Departments table exists and add permissions
IF EXISTS (SELECT * FROM sysobjects WHERE name='Departments' AND xtype='U')
BEGIN
    PRINT 'Adding Department permissions for admin...'
    
    INSERT INTO UserOrganizationalPermissions (UserId, PermissionType, EntityId, EntityName, CanView, CanEdit, CanDelete, CanCreate, IsActive, CreatedAt)
    SELECT 
        @AdminUserId,
        'Department' as PermissionType,
        departmentid as EntityId,
        department_name as EntityName,
        1 as CanView,
        1 as CanEdit,
        1 as CanDelete,
        1 as CanCreate,
        1 as IsActive,
        GETDATE() as CreatedAt
    FROM Departments
    WHERE NOT EXISTS (
        SELECT 1 FROM UserOrganizationalPermissions uop 
        WHERE uop.UserId = @AdminUserId 
        AND uop.PermissionType = 'Department'
        AND uop.EntityId = Departments.departmentid
    )
    
    PRINT 'Department permissions added successfully!'
END
ELSE
BEGIN
    PRINT 'WARNING: Departments table does not exist'
END

-- =====================================================
-- STEP 6: ADD ORGANIZATIONAL PERMISSIONS FOR DIVISIONS
-- =====================================================

-- Check if Divisions table exists and add permissions
IF EXISTS (SELECT * FROM sysobjects WHERE name='Divisions' AND xtype='U')
BEGIN
    PRINT 'Adding Division permissions for admin...'
    
    INSERT INTO UserOrganizationalPermissions (UserId, PermissionType, EntityId, EntityName, CanView, CanEdit, CanDelete, CanCreate, IsActive, CreatedAt)
    SELECT 
        @AdminUserId,
        'Division' as PermissionType,
        divisionid as EntityId,
        division_name as EntityName,
        1 as CanView,
        1 as CanEdit,
        1 as CanDelete,
        1 as CanCreate,
        1 as IsActive,
        GETDATE() as CreatedAt
    FROM Divisions
    WHERE NOT EXISTS (
        SELECT 1 FROM UserOrganizationalPermissions uop 
        WHERE uop.UserId = @AdminUserId 
        AND uop.PermissionType = 'Division'
        AND uop.EntityId = Divisions.divisionid
    )
    
    PRINT 'Division permissions added successfully!'
END
ELSE
BEGIN
    PRINT 'WARNING: Divisions table does not exist'
END

-- =====================================================
-- STEP 7: ADD GENERAL ORGANIZATIONAL PERMISSIONS
-- =====================================================

-- Add general organizational permissions even if specific tables don't exist
PRINT 'Adding general organizational permissions for admin...'

INSERT INTO UserOrganizationalPermissions (UserId, PermissionType, EntityId, EntityName, CanView, CanEdit, CanDelete, CanCreate, IsActive, CreatedAt)
SELECT 
    @AdminUserId,
    PermissionType,
    EntityId,
    EntityName,
    1 as CanView,
    1 as CanEdit,
    1 as CanDelete,
    1 as CanCreate,
    1 as IsActive,
    GETDATE() as CreatedAt
FROM (
    VALUES 
        ('Country', 0, 'All Countries'),
        ('Airport', 0, 'All Airports'),
        ('Department', 0, 'All Departments'),
        ('Division', 0, 'All Divisions'),
        ('Region', 0, 'All Regions'),
        ('Section', 0, 'All Sections')
) AS GeneralPermissions(PermissionType, EntityId, EntityName)
WHERE NOT EXISTS (
    SELECT 1 FROM UserOrganizationalPermissions uop 
    WHERE uop.UserId = @AdminUserId 
    AND uop.PermissionType = GeneralPermissions.PermissionType
    AND uop.EntityId = GeneralPermissions.EntityId
)

PRINT 'General organizational permissions added successfully!'

-- =====================================================
-- STEP 8: VERIFICATION
-- =====================================================

PRINT '========================================'
PRINT 'VERIFICATION RESULTS'
PRINT '========================================'

-- Check admin organizational permissions
SELECT 
    'Admin Organizational Permissions' as CheckType,
    COUNT(*) as TotalOrganizationalPermissions,
    COUNT(CASE WHEN PermissionType = 'Country' THEN 1 END) as CountryPermissions,
    COUNT(CASE WHEN PermissionType = 'Airport' THEN 1 END) as AirportPermissions,
    COUNT(CASE WHEN PermissionType = 'Department' THEN 1 END) as DepartmentPermissions,
    COUNT(CASE WHEN PermissionType = 'Division' THEN 1 END) as DivisionPermissions
FROM UserOrganizationalPermissions uop
INNER JOIN users u ON uop.UserId = u.UserId
WHERE u.Username = 'admin' AND uop.IsActive = 1

-- Show sample organizational permissions
SELECT TOP 10
    'Sample Organizational Permissions' as CheckType,
    PermissionType,
    EntityName,
    CanView,
    CanEdit,
    CanDelete,
    CanCreate
FROM UserOrganizationalPermissions uop
INNER JOIN users u ON uop.UserId = u.UserId
WHERE u.Username = 'admin' AND uop.IsActive = 1
ORDER BY PermissionType, EntityName

PRINT '========================================'
PRINT 'Admin Organizational Permissions Setup Complete!'
PRINT 'Admin user now has full organizational structure permissions.'
PRINT '========================================'
GO