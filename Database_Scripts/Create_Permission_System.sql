-- =====================================================
-- ANSMS Pro - Create Permission System
-- =====================================================
-- This script creates the basic permission system tables and data

USE [HR-Aviation]
GO

-- =====================================================
-- STEP 1: CREATE TABLES
-- =====================================================

-- Create Permissions table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Permissions' AND xtype='U')
BEGIN
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
    PRINT 'Permissions table created successfully'
END
ELSE
BEGIN
    PRINT 'Permissions table already exists'
END
GO

-- Create RolePermissions table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='RolePermissions' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[RolePermissions](
        [RolePermissionId] [int] IDENTITY(1,1) NOT NULL,
        [RoleId] [int] NOT NULL,
        [PermissionId] [int] NOT NULL,
        [IsActive] [bit] NOT NULL DEFAULT 1,
        [CreatedAt] [datetime] NOT NULL DEFAULT GETDATE(),
        CONSTRAINT [PK_RolePermissions] PRIMARY KEY CLUSTERED ([RolePermissionId] ASC),
        CONSTRAINT [FK_RolePermissions_Permissions] FOREIGN KEY([PermissionId]) REFERENCES [dbo].[Permissions] ([PermissionId])
    )
    PRINT 'RolePermissions table created successfully'
END
ELSE
BEGIN
    PRINT 'RolePermissions table already exists'
END
GO

-- Create UserDepartmentPermissions table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='UserDepartmentPermissions' AND xtype='U')
BEGIN
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
    PRINT 'UserDepartmentPermissions table created successfully'
END
ELSE
BEGIN
    PRINT 'UserDepartmentPermissions table already exists'
END
GO

-- =====================================================
-- STEP 2: INSERT DEFAULT PERMISSIONS
-- =====================================================

-- Clear existing permissions
DELETE FROM [Permissions]
GO

-- Insert default permissions
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
-- STEP 3: INSERT DEFAULT ROLE PERMISSIONS
-- =====================================================

-- Clear existing role permissions
DELETE FROM [RolePermissions]
GO

-- Get Role IDs from ConfigurationValues (if they exist)
DECLARE @AdminRoleId int = NULL, @SupervisorRoleId int = NULL, @StaffRoleId int = NULL

-- Try to get role IDs from ConfigurationValues
SELECT @AdminRoleId = ValueId FROM ConfigurationValues WHERE CategoryName = 'Roles' AND ValueText = 'Admin'
SELECT @SupervisorRoleId = ValueId FROM ConfigurationValues WHERE CategoryName = 'Roles' AND ValueText = 'Supervisor'
SELECT @StaffRoleId = ValueId FROM ConfigurationValues WHERE CategoryName = 'Roles' AND ValueText = 'Staff'

-- If roles don't exist in ConfigurationValues, create them
IF @AdminRoleId IS NULL
BEGIN
    INSERT INTO ConfigurationValues (CategoryName, ValueText, IsActive) VALUES ('Roles', 'Admin', 1)
    SET @AdminRoleId = SCOPE_IDENTITY()
    PRINT 'Admin role created in ConfigurationValues'
END

IF @SupervisorRoleId IS NULL
BEGIN
    INSERT INTO ConfigurationValues (CategoryName, ValueText, IsActive) VALUES ('Roles', 'Supervisor', 1)
    SET @SupervisorRoleId = SCOPE_IDENTITY()
    PRINT 'Supervisor role created in ConfigurationValues'
END

IF @StaffRoleId IS NULL
BEGIN
    INSERT INTO ConfigurationValues (CategoryName, ValueText, IsActive) VALUES ('Roles', 'Staff', 1)
    SET @StaffRoleId = SCOPE_IDENTITY()
    PRINT 'Staff role created in ConfigurationValues'
END

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
-- STEP 4: CREATE STORED PROCEDURES
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
-- STEP 5: VERIFICATION
-- =====================================================

-- Check if tables were created
SELECT 'Permissions' as TableName, COUNT(*) as RecordCount FROM Permissions
UNION ALL
SELECT 'RolePermissions' as TableName, COUNT(*) as RecordCount FROM RolePermissions
UNION ALL
SELECT 'UserDepartmentPermissions' as TableName, COUNT(*) as RecordCount FROM UserDepartmentPermissions

PRINT 'Permission system setup completed successfully!'
GO 
