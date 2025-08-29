-- =====================================================
-- ANSMS Pro - Fix Permission System Errors
-- =====================================================
-- This script fixes the errors in the permission system setup

USE [HR-Aviation]
GO

-- =====================================================
-- STEP 1: CLEAN UP EXISTING DATA
-- =====================================================

-- Disable foreign key constraints temporarily
EXEC sp_MSforeachtable "ALTER TABLE ? NOCHECK CONSTRAINT all"
GO

-- Clear existing data in correct order
IF OBJECT_ID('UserDepartmentPermissions', 'U') IS NOT NULL
    DELETE FROM UserDepartmentPermissions
GO

IF OBJECT_ID('RolePermissions', 'U') IS NOT NULL
    DELETE FROM RolePermissions
GO

IF OBJECT_ID('Permissions', 'U') IS NOT NULL
    DELETE FROM Permissions
GO

-- Re-enable foreign key constraints
EXEC sp_MSforeachtable "ALTER TABLE ? WITH CHECK CHECK CONSTRAINT all"
GO

PRINT 'Existing data cleared successfully'
GO

-- =====================================================
-- STEP 2: FIX PERMISSIONS TABLE
-- =====================================================

-- Drop and recreate Permissions table with correct structure
IF OBJECT_ID('Permissions', 'U') IS NOT NULL
    DROP TABLE Permissions
GO

CREATE TABLE [dbo].[Permissions](
    [PermissionId] [int] IDENTITY(1,1) NOT NULL,
    [PermissionName] [nvarchar](100) NOT NULL,
    [PermissionKey] [nvarchar](50) NOT NULL,
    [PermissionDescription] [nvarchar](500) NULL,
    [CategoryName] [nvarchar](50) NOT NULL,
    [IsActive] [bit] NOT NULL DEFAULT 1,
    [CreatedAt] [datetime] NOT NULL DEFAULT GETDATE(),
    [UpdatedAt] [datetime] NULL,
    CONSTRAINT [PK_Permissions] PRIMARY KEY CLUSTERED ([PermissionId] ASC)
)
GO

PRINT 'Permissions table recreated successfully'
GO

-- =====================================================
-- STEP 3: INSERT DEFAULT PERMISSIONS
-- =====================================================

INSERT INTO [Permissions] ([PermissionName], [PermissionKey], [PermissionDescription], [CategoryName]) VALUES
('View Dashboard', 'DASHBOARD_VIEW', 'Can view the main dashboard', 'Dashboard'),
('Export Dashboard Data', 'DASHBOARD_EXPORT', 'Can export dashboard data', 'Dashboard'),
('View Organization', 'ORGANIZATION_VIEW', 'Can view organization information', 'Organization'),
('Edit Organization', 'ORGANIZATION_EDIT', 'Can edit organization information', 'Organization'),
('View Employees', 'EMPLOYEES_VIEW', 'Can view employee list', 'Staff'),
('Add Employee', 'EMPLOYEES_ADD', 'Can add new employees', 'Staff'),
('Edit Employee', 'EMPLOYEES_EDIT', 'Can edit employee information', 'Staff'),
('Delete Employee', 'EMPLOYEES_DELETE', 'Can delete employees', 'Staff'),
('View Controllers', 'CONTROLLERS_VIEW', 'Can view controller list', 'Staff'),
('Add Controller', 'CONTROLLERS_ADD', 'Can add new controllers', 'Staff'),
('Edit Controller', 'CONTROLLERS_EDIT', 'Can edit controller information', 'Staff'),
('Delete Controller', 'CONTROLLERS_DELETE', 'Can delete controllers', 'Staff'),
('View Licenses', 'LICENSES_VIEW', 'Can view license information', 'Documents'),
('Add License', 'LICENSES_ADD', 'Can add new licenses', 'Documents'),
('Edit License', 'LICENSES_EDIT', 'Can edit license information', 'Documents'),
('Delete License', 'LICENSES_DELETE', 'Can delete licenses', 'Documents'),
('View Certificates', 'CERTIFICATES_VIEW', 'Can view certificate information', 'Documents'),
('Add Certificate', 'CERTIFICATES_ADD', 'Can add new certificates', 'Documents'),
('Edit Certificate', 'CERTIFICATES_EDIT', 'Can edit certificate information', 'Documents'),
('Delete Certificate', 'CERTIFICATES_DELETE', 'Can delete certificates', 'Documents'),
('View Observations', 'OBSERVATIONS_VIEW', 'Can view observations', 'Activities'),
('Add Observation', 'OBSERVATIONS_ADD', 'Can add new observations', 'Activities'),
('Edit Observation', 'OBSERVATIONS_EDIT', 'Can edit observations', 'Activities'),
('Delete Observation', 'OBSERVATIONS_DELETE', 'Can delete observations', 'Activities'),
('View Configuration', 'CONFIGURATION_VIEW', 'Can view system configuration', 'System'),
('Edit Configuration', 'CONFIGURATION_EDIT', 'Can edit system configuration', 'System'),
('View Permissions', 'PERMISSIONS_VIEW', 'Can view permission management', 'System'),
('Manage Permissions', 'PERMISSIONS_MANAGE', 'Can manage user permissions', 'System')
GO

PRINT 'Default permissions inserted successfully'
GO

-- =====================================================
-- STEP 4: FIX ROLEPERMISSIONS TABLE
-- =====================================================

-- Drop and recreate RolePermissions table
IF OBJECT_ID('RolePermissions', 'U') IS NOT NULL
    DROP TABLE RolePermissions
GO

CREATE TABLE [dbo].[RolePermissions](
    [RolePermissionId] [int] IDENTITY(1,1) NOT NULL,
    [RoleId] [int] NOT NULL,
    [PermissionId] [int] NOT NULL,
    [IsActive] [bit] NOT NULL DEFAULT 1,
    [CreatedAt] [datetime] NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [PK_RolePermissions] PRIMARY KEY CLUSTERED ([RolePermissionId] ASC),
    CONSTRAINT [FK_RolePermissions_Permissions] FOREIGN KEY([PermissionId]) REFERENCES [dbo].[Permissions] ([PermissionId])
)
GO

PRINT 'RolePermissions table recreated successfully'
GO

-- =====================================================
-- STEP 5: FIX USERDEPARTMENTPERMISSIONS TABLE
-- =====================================================

-- Drop and recreate UserDepartmentPermissions table
IF OBJECT_ID('UserDepartmentPermissions', 'U') IS NOT NULL
    DROP TABLE UserDepartmentPermissions
GO

CREATE TABLE [dbo].[UserDepartmentPermissions](
    [UserDepartmentPermissionId] [int] IDENTITY(1,1) NOT NULL,
    [UserId] [int] NOT NULL,
    [DepartmentId] [int] NOT NULL,
    [PermissionId] [int] NOT NULL,
    [CanView] [bit] NOT NULL DEFAULT 0,
    [CanEdit] [bit] NOT NULL DEFAULT 0,
    [CanDelete] [bit] NOT NULL DEFAULT 0,
    [IsActive] [bit] NOT NULL DEFAULT 1,
    [CreatedAt] [datetime] NOT NULL DEFAULT GETDATE(),
    [UpdatedAt] [datetime] NULL,
    CONSTRAINT [PK_UserDepartmentPermissions] PRIMARY KEY CLUSTERED ([UserDepartmentPermissionId] ASC),
    CONSTRAINT [FK_UserDepartmentPermissions_Permissions] FOREIGN KEY([PermissionId]) REFERENCES [dbo].[Permissions] ([PermissionId])
)
GO

PRINT 'UserDepartmentPermissions table recreated successfully'
GO

-- =====================================================
-- STEP 6: ENSURE ROLES EXIST IN CONFIGURATIONVALUES
-- =====================================================

-- Check if Roles category exists, if not create it
IF NOT EXISTS (SELECT 1 FROM ConfigurationCategories WHERE CategoryName = 'Roles')
BEGIN
    INSERT INTO ConfigurationCategories (CategoryName, DisplayName, IsActive) 
    VALUES ('Roles', 'User Roles', 1)
    PRINT 'Roles category created in ConfigurationCategories'
END

-- Ensure basic roles exist
IF NOT EXISTS (SELECT 1 FROM ConfigurationValues WHERE CategoryName = 'Roles' AND ValueText = 'Admin')
BEGIN
    INSERT INTO ConfigurationValues (CategoryName, ValueText, IsActive) 
    VALUES ('Roles', 'Admin', 1)
    PRINT 'Admin role created'
END

IF NOT EXISTS (SELECT 1 FROM ConfigurationValues WHERE CategoryName = 'Roles' AND ValueText = 'Supervisor')
BEGIN
    INSERT INTO ConfigurationValues (CategoryName, ValueText, IsActive) 
    VALUES ('Roles', 'Supervisor', 1)
    PRINT 'Supervisor role created'
END

IF NOT EXISTS (SELECT 1 FROM ConfigurationValues WHERE CategoryName = 'Roles' AND ValueText = 'Staff')
BEGIN
    INSERT INTO ConfigurationValues (CategoryName, ValueText, IsActive) 
    VALUES ('Roles', 'Staff', 1)
    PRINT 'Staff role created'
END

-- Ensure Divisions category exists
IF NOT EXISTS (SELECT 1 FROM ConfigurationCategories WHERE CategoryName = 'Divisions')
BEGIN
    INSERT INTO ConfigurationCategories (CategoryName, DisplayName, IsActive) 
    VALUES ('Divisions', 'Departments and Divisions', 1)
    PRINT 'Divisions category created in ConfigurationCategories'
END

-- Add some basic divisions if they don't exist
IF NOT EXISTS (SELECT 1 FROM ConfigurationValues WHERE CategoryName = 'Divisions' AND ValueText = 'AFTN')
BEGIN
    INSERT INTO ConfigurationValues (CategoryName, ValueText, IsActive) 
    VALUES ('Divisions', 'AFTN', 1)
    PRINT 'AFTN division created'
END

IF NOT EXISTS (SELECT 1 FROM ConfigurationValues WHERE CategoryName = 'Divisions' AND ValueText = 'AIS')
BEGIN
    INSERT INTO ConfigurationValues (CategoryName, ValueText, IsActive) 
    VALUES ('Divisions', 'AIS', 1)
    PRINT 'AIS division created'
END

IF NOT EXISTS (SELECT 1 FROM ConfigurationValues WHERE CategoryName = 'Divisions' AND ValueText = 'CNS')
BEGIN
    INSERT INTO ConfigurationValues (CategoryName, ValueText, IsActive) 
    VALUES ('Divisions', 'CNS', 1)
    PRINT 'CNS division created'
END

PRINT 'Roles and Divisions setup completed'
GO

-- =====================================================
-- STEP 7: INSERT DEFAULT ROLE PERMISSIONS
-- =====================================================

-- Get Role IDs
DECLARE @AdminRoleId int, @SupervisorRoleId int, @StaffRoleId int

SELECT @AdminRoleId = ValueId FROM ConfigurationValues WHERE CategoryName = 'Roles' AND ValueText = 'Admin'
SELECT @SupervisorRoleId = ValueId FROM ConfigurationValues WHERE CategoryName = 'Roles' AND ValueText = 'Supervisor'
SELECT @StaffRoleId = ValueId FROM ConfigurationValues WHERE CategoryName = 'Roles' AND ValueText = 'Staff'

-- Insert role permissions
-- Admin gets all permissions
INSERT INTO RolePermissions (RoleId, PermissionId)
SELECT @AdminRoleId, PermissionId FROM Permissions WHERE IsActive = 1

-- Supervisor gets most permissions except system management
INSERT INTO RolePermissions (RoleId, PermissionId)
SELECT @SupervisorRoleId, PermissionId FROM Permissions 
WHERE IsActive = 1 AND CategoryName NOT IN ('System')

-- Staff gets basic view permissions
INSERT INTO RolePermissions (RoleId, PermissionId)
SELECT @StaffRoleId, PermissionId FROM Permissions 
WHERE IsActive = 1 AND PermissionKey IN (
    'DASHBOARD_VIEW',
    'ORGANIZATION_VIEW',
    'EMPLOYEES_VIEW',
    'CONTROLLERS_VIEW',
    'LICENSES_VIEW',
    'CERTIFICATES_VIEW',
    'OBSERVATIONS_VIEW'
)

PRINT 'Default role permissions inserted successfully'
GO

-- =====================================================
-- STEP 8: CREATE STORED PROCEDURES
-- =====================================================

-- Drop existing stored procedures if they exist
IF OBJECT_ID('GetUserPermissions', 'P') IS NOT NULL
    DROP PROCEDURE GetUserPermissions
GO

IF OBJECT_ID('CheckUserPermission', 'P') IS NOT NULL
    DROP PROCEDURE CheckUserPermission
GO

-- Create GetUserPermissions stored procedure
CREATE PROCEDURE [dbo].[GetUserPermissions]
    @UserId int
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT DISTINCT p.PermissionId, p.PermissionName, p.PermissionKey, p.PermissionDescription, p.CategoryName
    FROM Permissions p
    INNER JOIN RolePermissions rp ON p.PermissionId = rp.PermissionId
    INNER JOIN ConfigurationValues cv ON rp.RoleId = cv.ValueId
    INNER JOIN users u ON u.rolename = cv.ValueText
    WHERE u.userid = @UserId AND p.IsActive = 1 AND rp.IsActive = 1
    
    UNION
    
    SELECT DISTINCT p.PermissionId, p.PermissionName, p.PermissionKey, p.PermissionDescription, p.CategoryName
    FROM Permissions p
    INNER JOIN UserDepartmentPermissions udp ON p.PermissionId = udp.PermissionId
    WHERE udp.UserId = @UserId AND p.IsActive = 1 AND udp.IsActive = 1
END
GO

-- Create CheckUserPermission stored procedure
CREATE PROCEDURE [dbo].[CheckUserPermission]
    @UserId int,
    @PermissionKey nvarchar(50),
    @DepartmentId int = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @HasPermission bit = 0
    
    -- Check role-based permissions
    IF EXISTS (
        SELECT 1 FROM Permissions p
        INNER JOIN RolePermissions rp ON p.PermissionId = rp.PermissionId
        INNER JOIN ConfigurationValues cv ON rp.RoleId = cv.ValueId
        INNER JOIN users u ON u.rolename = cv.ValueText
        WHERE u.userid = @UserId AND p.PermissionKey = @PermissionKey 
        AND p.IsActive = 1 AND rp.IsActive = 1
    )
    BEGIN
        SET @HasPermission = 1
    END
    
    -- Check department-specific permissions
    IF @DepartmentId IS NOT NULL AND EXISTS (
        SELECT 1 FROM UserDepartmentPermissions udp
        INNER JOIN Permissions p ON udp.PermissionId = p.PermissionId
        WHERE udp.UserId = @UserId AND udp.DepartmentId = @DepartmentId 
        AND p.PermissionKey = @PermissionKey AND udp.IsActive = 1 AND p.IsActive = 1
    )
    BEGIN
        SET @HasPermission = 1
    END
    
    SELECT @HasPermission as HasPermission
END
GO

PRINT 'Stored procedures created successfully'
GO

-- =====================================================
-- STEP 9: VERIFICATION
-- =====================================================

-- Check if tables were created and have data
SELECT 'Permissions' as TableName, COUNT(*) as RecordCount FROM Permissions
UNION ALL
SELECT 'RolePermissions' as TableName, COUNT(*) as RecordCount FROM RolePermissions
UNION ALL
SELECT 'UserDepartmentPermissions' as TableName, COUNT(*) as RecordCount FROM UserDepartmentPermissions

-- Check if roles exist
SELECT 'Roles in ConfigurationValues' as Description, COUNT(*) as Count 
FROM ConfigurationValues WHERE CategoryName = 'Roles'

-- Check if divisions exist
SELECT 'Divisions in ConfigurationValues' as Description, COUNT(*) as Count 
FROM ConfigurationValues WHERE CategoryName = 'Divisions'

PRINT 'Permission system setup completed successfully!'
GO 
