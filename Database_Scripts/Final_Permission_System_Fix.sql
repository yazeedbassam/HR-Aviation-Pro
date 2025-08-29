-- =====================================================
-- ANSMS Pro - Final Permission System Database Fix
-- =====================================================
-- This script completely recreates the permission system with correct structure

USE [HR-Aviation]
GO

-- =====================================================
-- STEP 1: DROP EXISTING OBJECTS
-- =====================================================

-- Drop views first
IF OBJECT_ID('vw_UserPermissionsSummary', 'V') IS NOT NULL
    DROP VIEW vw_UserPermissionsSummary
GO

IF OBJECT_ID('vw_DepartmentPermissions', 'V') IS NOT NULL
    DROP VIEW vw_DepartmentPermissions
GO

-- Drop stored procedures
IF OBJECT_ID('GetUserPermissions', 'P') IS NOT NULL
    DROP PROCEDURE GetUserPermissions
GO

IF OBJECT_ID('GetUserDepartmentPermissions', 'P') IS NOT NULL
    DROP PROCEDURE GetUserDepartmentPermissions
GO

IF OBJECT_ID('CheckUserPermission', 'P') IS NOT NULL
    DROP PROCEDURE CheckUserPermission
GO

IF OBJECT_ID('LogPermissionAccess', 'P') IS NOT NULL
    DROP PROCEDURE LogPermissionAccess
GO

-- Drop tables (in reverse order of dependencies)
IF OBJECT_ID('PermissionLogs', 'U') IS NOT NULL
    DROP TABLE PermissionLogs
GO

IF OBJECT_ID('UserDepartmentPermissions', 'U') IS NOT NULL
    DROP TABLE UserDepartmentPermissions
GO

IF OBJECT_ID('RolePermissions', 'U') IS NOT NULL
    DROP TABLE RolePermissions
GO

IF OBJECT_ID('Permissions', 'U') IS NOT NULL
    DROP TABLE Permissions
GO

-- =====================================================
-- STEP 2: CREATE TABLES WITH CORRECT STRUCTURE
-- =====================================================

-- Create Permissions table with CategoryName and PermissionDescription
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

-- Create RolePermissions table
CREATE TABLE [dbo].[RolePermissions](
    [RolePermissionId] [int] IDENTITY(1,1) NOT NULL,
    [RoleId] [int] NOT NULL,
    [PermissionId] [int] NOT NULL,
    [IsActive] [bit] NOT NULL DEFAULT 1,
    [CreatedAt] [datetime] NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [PK_RolePermissions] PRIMARY KEY CLUSTERED ([RolePermissionId] ASC),
    CONSTRAINT [FK_RolePermissions_Permissions] FOREIGN KEY([PermissionId]) REFERENCES [dbo].[Permissions] ([PermissionId]),
    CONSTRAINT [FK_RolePermissions_ConfigurationValues] FOREIGN KEY([RoleId]) REFERENCES [dbo].[ConfigurationValues] ([ValueId])
)
GO

-- Create UserDepartmentPermissions table
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
    CONSTRAINT [FK_UserDepartmentPermissions_Permissions] FOREIGN KEY([PermissionId]) REFERENCES [dbo].[Permissions] ([PermissionId]),
    CONSTRAINT [FK_UserDepartmentPermissions_ConfigurationValues] FOREIGN KEY([DepartmentId]) REFERENCES [dbo].[ConfigurationValues] ([ValueId]),
    CONSTRAINT [FK_UserDepartmentPermissions_Users] FOREIGN KEY([UserId]) REFERENCES [dbo].[users] ([userid])
)
GO

-- Create PermissionLogs table
CREATE TABLE [dbo].[PermissionLogs](
    [LogId] [int] IDENTITY(1,1) NOT NULL,
    [UserId] [int] NOT NULL,
    [PermissionKey] [nvarchar](50) NOT NULL,
    [DepartmentId] [int] NULL,
    [Action] [nvarchar](50) NOT NULL,
    [Result] [bit] NOT NULL,
    [Timestamp] [datetime] NOT NULL DEFAULT GETDATE(),
    [IPAddress] [nvarchar](45) NULL,
    [UserAgent] [nvarchar](500) NULL,
    CONSTRAINT [PK_PermissionLogs] PRIMARY KEY CLUSTERED ([LogId] ASC),
    CONSTRAINT [FK_PermissionLogs_Users] FOREIGN KEY([UserId]) REFERENCES [dbo].[users] ([userid]),
    CONSTRAINT [FK_PermissionLogs_ConfigurationValues] FOREIGN KEY([DepartmentId]) REFERENCES [dbo].[ConfigurationValues] ([ValueId])
)
GO

-- =====================================================
-- STEP 3: INSERT DEFAULT PERMISSIONS
-- =====================================================

-- Dashboard Permissions
INSERT INTO [Permissions] ([PermissionName], [PermissionKey], [PermissionDescription], [CategoryName]) VALUES
('View Dashboard', 'DASHBOARD_VIEW', 'Can view the main dashboard', 'Dashboard'),
('Export Dashboard Data', 'DASHBOARD_EXPORT', 'Can export dashboard data', 'Dashboard');

-- Organization Permissions
INSERT INTO [Permissions] ([PermissionName], [PermissionKey], [PermissionDescription], [CategoryName]) VALUES
('View Organization', 'ORGANIZATION_VIEW', 'Can view organization information', 'Organization'),
('Edit Organization', 'ORGANIZATION_EDIT', 'Can edit organization information', 'Organization');

-- Staff Permissions
INSERT INTO [Permissions] ([PermissionName], [PermissionKey], [PermissionDescription], [CategoryName]) VALUES
('View Employees', 'EMPLOYEES_VIEW', 'Can view employee list', 'Staff'),
('Add Employee', 'EMPLOYEES_ADD', 'Can add new employees', 'Staff'),
('Edit Employee', 'EMPLOYEES_EDIT', 'Can edit employee information', 'Staff'),
('Delete Employee', 'EMPLOYEES_DELETE', 'Can delete employees', 'Staff'),
('View Controllers', 'CONTROLLERS_VIEW', 'Can view controller list', 'Staff'),
('Add Controller', 'CONTROLLERS_ADD', 'Can add new controllers', 'Staff'),
('Edit Controller', 'CONTROLLERS_EDIT', 'Can edit controller information', 'Staff'),
('Delete Controller', 'CONTROLLERS_DELETE', 'Can delete controllers', 'Staff');

-- Documents Permissions
INSERT INTO [Permissions] ([PermissionName], [PermissionKey], [PermissionDescription], [CategoryName]) VALUES
('View Licenses', 'LICENSES_VIEW', 'Can view license information', 'Documents'),
('Add License', 'LICENSES_ADD', 'Can add new licenses', 'Documents'),
('Edit License', 'LICENSES_EDIT', 'Can edit license information', 'Documents'),
('Delete License', 'LICENSES_DELETE', 'Can delete licenses', 'Documents'),
('View Certificates', 'CERTIFICATES_VIEW', 'Can view certificate information', 'Documents'),
('Add Certificate', 'CERTIFICATES_ADD', 'Can add new certificates', 'Documents'),
('Edit Certificate', 'CERTIFICATES_EDIT', 'Can edit certificate information', 'Documents'),
('Delete Certificate', 'CERTIFICATES_DELETE', 'Can delete certificates', 'Documents');

-- Activities Permissions
INSERT INTO [Permissions] ([PermissionName], [PermissionKey], [PermissionDescription], [CategoryName]) VALUES
('View Observations', 'OBSERVATIONS_VIEW', 'Can view observations', 'Activities'),
('Add Observation', 'OBSERVATIONS_ADD', 'Can add new observations', 'Activities'),
('Edit Observation', 'OBSERVATIONS_EDIT', 'Can edit observations', 'Activities'),
('Delete Observation', 'OBSERVATIONS_DELETE', 'Can delete observations', 'Activities');

-- System Settings Permissions
INSERT INTO [Permissions] ([PermissionName], [PermissionKey], [PermissionDescription], [CategoryName]) VALUES
('View Configuration', 'CONFIGURATION_VIEW', 'Can view system configuration', 'System'),
('Edit Configuration', 'CONFIGURATION_EDIT', 'Can edit system configuration', 'System'),
('View Permissions', 'PERMISSIONS_VIEW', 'Can view permission management', 'System'),
('Manage Permissions', 'PERMISSIONS_MANAGE', 'Can manage user permissions', 'System');

-- =====================================================
-- STEP 4: INSERT DEFAULT ROLE PERMISSIONS
-- =====================================================

-- Get Role IDs from ConfigurationValues
DECLARE @AdminRoleId int, @SupervisorRoleId int, @StaffRoleId int

SELECT @AdminRoleId = ValueId FROM ConfigurationValues WHERE CategoryName = 'Roles' AND ValueText = 'Admin'
SELECT @SupervisorRoleId = ValueId FROM ConfigurationValues WHERE CategoryName = 'Roles' AND ValueText = 'Supervisor'
SELECT @StaffRoleId = ValueId FROM ConfigurationValues WHERE CategoryName = 'Roles' AND ValueText = 'Staff'

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

-- =====================================================
-- STEP 5: CREATE STORED PROCEDURES
-- =====================================================

-- GetUserPermissions stored procedure
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

-- CheckUserPermission stored procedure
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

-- =====================================================
-- STEP 6: CREATE VIEWS
-- =====================================================

-- User Permissions Summary View
CREATE VIEW [dbo].[vw_UserPermissionsSummary] AS
SELECT 
    u.userid as UserId,
    u.username as UserName,
    u.rolename as UserRole,
    COUNT(DISTINCT p.PermissionId) as TotalPermissions,
    COUNT(DISTINCT udp.DepartmentId) as AccessibleDepartments
FROM users u
LEFT JOIN RolePermissions rp ON rp.RoleId = (SELECT ValueId FROM ConfigurationValues WHERE CategoryName = 'Roles' AND ValueText = u.rolename)
LEFT JOIN Permissions p ON rp.PermissionId = p.PermissionId AND p.IsActive = 1
LEFT JOIN UserDepartmentPermissions udp ON udp.UserId = u.userid AND udp.IsActive = 1
GROUP BY u.userid, u.username, u.rolename
GO

-- Department Permissions View
CREATE VIEW [dbo].[vw_DepartmentPermissions] AS
SELECT 
    udp.UserDepartmentPermissionId,
    u.username as UserName,
    u.rolename as UserRole,
    dept.ValueText as DepartmentName,
    p.PermissionName,
    udp.CanView,
    udp.CanEdit,
    udp.CanDelete
FROM UserDepartmentPermissions udp
INNER JOIN users u ON udp.UserId = u.userid
INNER JOIN ConfigurationValues dept ON udp.DepartmentId = dept.ValueId
INNER JOIN Permissions p ON udp.PermissionId = p.PermissionId
WHERE udp.IsActive = 1 AND dept.CategoryName = 'Divisions'
GO

PRINT 'Permission system database setup completed successfully!'
GO 
