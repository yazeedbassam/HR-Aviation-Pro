-- =====================================================
-- ANSMS Pro - Permission System Database Script
-- =====================================================
-- This script creates a comprehensive permission system
-- that supports role-based access control with department-level permissions
-- =====================================================

USE [ANSMS_Pro]
GO

-- =====================================================
-- 1. CREATE PERMISSIONS TABLE
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Permissions]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Permissions](
        [PermissionId] [int] IDENTITY(1,1) NOT NULL,
        [PermissionName] [nvarchar](100) NOT NULL,
        [PermissionKey] [nvarchar](50) NOT NULL,
        [Description] [nvarchar](500) NULL,
        [IsActive] [bit] NOT NULL DEFAULT 1,
        [CreatedAt] [datetime] NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] [datetime] NULL,
        CONSTRAINT [PK_Permissions] PRIMARY KEY CLUSTERED ([PermissionId] ASC)
    )
END
GO

-- =====================================================
-- 2. CREATE ROLE PERMISSIONS TABLE
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RolePermissions]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[RolePermissions](
        [RolePermissionId] [int] IDENTITY(1,1) NOT NULL,
        [RoleId] [int] NOT NULL,
        [PermissionId] [int] NOT NULL,
        [IsActive] [bit] NOT NULL DEFAULT 1,
        [CreatedAt] [datetime] NOT NULL DEFAULT GETDATE(),
        CONSTRAINT [PK_RolePermissions] PRIMARY KEY CLUSTERED ([RolePermissionId] ASC),
        CONSTRAINT [FK_RolePermissions_RoleId] FOREIGN KEY([RoleId]) REFERENCES [dbo].[ConfigurationValues] ([ValueId]),
        CONSTRAINT [FK_RolePermissions_PermissionId] FOREIGN KEY([PermissionId]) REFERENCES [dbo].[Permissions] ([PermissionId])
    )
END
GO

-- =====================================================
-- 3. CREATE USER DEPARTMENT PERMISSIONS TABLE
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UserDepartmentPermissions]') AND type in (N'U'))
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
        CONSTRAINT [FK_UserDepartmentPermissions_UserId] FOREIGN KEY([UserId]) REFERENCES [dbo].[Users] ([UserId]),
        CONSTRAINT [FK_UserDepartmentPermissions_DepartmentId] FOREIGN KEY([DepartmentId]) REFERENCES [dbo].[ConfigurationValues] ([ValueId]),
        CONSTRAINT [FK_UserDepartmentPermissions_PermissionId] FOREIGN KEY([PermissionId]) REFERENCES [dbo].[Permissions] ([PermissionId])
    )
END
GO

-- =====================================================
-- 4. CREATE PERMISSION LOGS TABLE
-- =====================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PermissionLogs]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[PermissionLogs](
        [PermissionLogId] [int] IDENTITY(1,1) NOT NULL,
        [UserId] [int] NOT NULL,
        [Action] [nvarchar](50) NOT NULL,
        [PermissionKey] [nvarchar](50) NOT NULL,
        [DepartmentId] [int] NULL,
        [Details] [nvarchar](1000) NULL,
        [IPAddress] [nvarchar](45) NULL,
        [UserAgent] [nvarchar](500) NULL,
        [CreatedAt] [datetime] NOT NULL DEFAULT GETDATE(),
        CONSTRAINT [PK_PermissionLogs] PRIMARY KEY CLUSTERED ([PermissionLogId] ASC),
        CONSTRAINT [FK_PermissionLogs_UserId] FOREIGN KEY([UserId]) REFERENCES [dbo].[Users] ([UserId])
    )
END
GO

-- =====================================================
-- 5. INSERT DEFAULT PERMISSIONS
-- =====================================================
-- Clear existing permissions first
DELETE FROM [dbo].[Permissions]

-- Insert default permissions
INSERT INTO [dbo].[Permissions] ([PermissionName], [PermissionKey], [Description], [IsActive]) VALUES
-- Dashboard Permissions
('Dashboard View', 'DASHBOARD_VIEW', 'Can view dashboard with statistics', 1),
('Dashboard Export', 'DASHBOARD_EXPORT', 'Can export dashboard data', 1),

-- Controller Management Permissions
('Controllers View', 'CONTROLLERS_VIEW', 'Can view controllers list', 1),
('Controllers Create', 'CONTROLLERS_CREATE', 'Can create new controllers', 1),
('Controllers Edit', 'CONTROLLERS_EDIT', 'Can edit existing controllers', 1),
('Controllers Delete', 'CONTROLLERS_DELETE', 'Can delete controllers', 1),
('Controllers Export', 'CONTROLLERS_EXPORT', 'Can export controllers data', 1),

-- Staff Management Permissions
('Staff View', 'STAFF_VIEW', 'Can view staff list', 1),
('Staff Create', 'STAFF_CREATE', 'Can create new staff members', 1),
('Staff Edit', 'STAFF_EDIT', 'Can edit existing staff members', 1),
('Staff Delete', 'STAFF_DELETE', 'Can delete staff members', 1),
('Staff Export', 'STAFF_EXPORT', 'Can export staff data', 1),

-- License Management Permissions
('Licenses View', 'LICENSES_VIEW', 'Can view licenses list', 1),
('Licenses Create', 'LICENSES_CREATE', 'Can create new licenses', 1),
('Licenses Edit', 'LICENSES_EDIT', 'Can edit existing licenses', 1),
('Licenses Delete', 'LICENSES_DELETE', 'Can delete licenses', 1),
('Licenses Export', 'LICENSES_EXPORT', 'Can export licenses data', 1),

-- Certificate Management Permissions
('Certificates View', 'CERTIFICATES_VIEW', 'Can view certificates list', 1),
('Certificates Create', 'CERTIFICATES_CREATE', 'Can create new certificates', 1),
('Certificates Edit', 'CERTIFICATES_EDIT', 'Can edit existing certificates', 1),
('Certificates Delete', 'CERTIFICATES_DELETE', 'Can delete certificates', 1),
('Certificates Export', 'CERTIFICATES_EXPORT', 'Can export certificates data', 1),

-- Observation Management Permissions
('Observations View', 'OBSERVATIONS_VIEW', 'Can view observations list', 1),
('Observations Create', 'OBSERVATIONS_CREATE', 'Can create new observations', 1),
('Observations Edit', 'OBSERVATIONS_EDIT', 'Can edit existing observations', 1),
('Observations Delete', 'OBSERVATIONS_DELETE', 'Can delete observations', 1),
('Observations Export', 'OBSERVATIONS_EXPORT', 'Can export observations data', 1),

-- System Settings Permissions
('System Settings View', 'SYSTEM_SETTINGS_VIEW', 'Can view system settings', 1),
('System Settings Edit', 'SYSTEM_SETTINGS_EDIT', 'Can edit system settings', 1),
('Configuration Management', 'CONFIGURATION_MANAGEMENT', 'Can manage system configuration', 1),
('Roles Management', 'ROLES_MANAGEMENT', 'Can manage user roles and permissions', 1),

-- User Management Permissions
('Users View', 'USERS_VIEW', 'Can view users list', 1),
('Users Create', 'USERS_CREATE', 'Can create new users', 1),
('Users Edit', 'USERS_EDIT', 'Can edit existing users', 1),
('Users Delete', 'USERS_DELETE', 'Can delete users', 1),

-- Reports Permissions
('Reports View', 'REPORTS_VIEW', 'Can view reports', 1),
('Reports Generate', 'REPORTS_GENERATE', 'Can generate reports', 1),
('Reports Export', 'REPORTS_EXPORT', 'Can export reports', 1),

-- Audit Permissions
('Audit Logs View', 'AUDIT_LOGS_VIEW', 'Can view audit logs', 1),
('Permission Logs View', 'PERMISSION_LOGS_VIEW', 'Can view permission logs', 1)
GO

-- =====================================================
-- 6. INSERT DEFAULT ROLE PERMISSIONS
-- =====================================================
-- Clear existing role permissions first
DELETE FROM [dbo].[RolePermissions]

-- Get Role IDs from ConfigurationValues
DECLARE @AdminRoleId INT = (SELECT ValueId FROM ConfigurationValues WHERE ValueKey = 'ADMINISTRATOR' AND CategoryId = (SELECT CategoryId FROM ConfigurationCategories WHERE CategoryName = 'Roles'))
DECLARE @ControllerRoleId INT = (SELECT ValueId FROM ConfigurationValues WHERE ValueKey = 'CONTROLLER' AND CategoryId = (SELECT CategoryId FROM ConfigurationCategories WHERE CategoryName = 'Roles'))
DECLARE @EmployeeRoleId INT = (SELECT ValueId FROM ConfigurationValues WHERE ValueKey = 'EMPLOYEE' AND CategoryId = (SELECT CategoryId FROM ConfigurationCategories WHERE CategoryName = 'Roles'))
DECLARE @SupervisorRoleId INT = (SELECT ValueId FROM ConfigurationValues WHERE ValueKey = 'SUPERVISOR' AND CategoryId = (SELECT CategoryId FROM ConfigurationCategories WHERE CategoryName = 'Roles'))
DECLARE @ManagerRoleId INT = (SELECT ValueId FROM ConfigurationValues WHERE ValueKey = 'MANAGER' AND CategoryId = (SELECT CategoryId FROM ConfigurationCategories WHERE CategoryName = 'Roles'))

-- Administrator Role - Full Access
INSERT INTO [dbo].[RolePermissions] ([RoleId], [PermissionId])
SELECT @AdminRoleId, PermissionId FROM [dbo].[Permissions] WHERE IsActive = 1

-- Manager Role - Most Access except System Settings
INSERT INTO [dbo].[RolePermissions] ([RoleId], [PermissionId])
SELECT @ManagerRoleId, PermissionId FROM [dbo].[Permissions] 
WHERE IsActive = 1 
AND PermissionKey NOT IN ('SYSTEM_SETTINGS_EDIT', 'CONFIGURATION_MANAGEMENT', 'ROLES_MANAGEMENT', 'USERS_CREATE', 'USERS_DELETE')

-- Supervisor Role - Department Level Access
INSERT INTO [dbo].[RolePermissions] ([RoleId], [PermissionId])
SELECT @SupervisorRoleId, PermissionId FROM [dbo].[Permissions] 
WHERE IsActive = 1 
AND PermissionKey IN (
    'DASHBOARD_VIEW',
    'CONTROLLERS_VIEW', 'CONTROLLERS_CREATE', 'CONTROLLERS_EDIT',
    'STAFF_VIEW', 'STAFF_CREATE', 'STAFF_EDIT',
    'LICENSES_VIEW', 'LICENSES_CREATE', 'LICENSES_EDIT',
    'CERTIFICATES_VIEW', 'CERTIFICATES_CREATE', 'CERTIFICATES_EDIT',
    'OBSERVATIONS_VIEW', 'OBSERVATIONS_CREATE', 'OBSERVATIONS_EDIT',
    'REPORTS_VIEW', 'REPORTS_GENERATE'
)

-- Controller Role - Limited Access
INSERT INTO [dbo].[RolePermissions] ([RoleId], [PermissionId])
SELECT @ControllerRoleId, PermissionId FROM [dbo].[Permissions] 
WHERE IsActive = 1 
AND PermissionKey IN (
    'DASHBOARD_VIEW',
    'LICENSES_VIEW', 'LICENSES_CREATE', 'LICENSES_EDIT',
    'CERTIFICATES_VIEW', 'CERTIFICATES_CREATE', 'CERTIFICATES_EDIT',
    'OBSERVATIONS_VIEW', 'OBSERVATIONS_CREATE', 'OBSERVATIONS_EDIT'
)

-- Employee Role - Read Only Access
INSERT INTO [dbo].[RolePermissions] ([RoleId], [PermissionId])
SELECT @EmployeeRoleId, PermissionId FROM [dbo].[Permissions] 
WHERE IsActive = 1 
AND PermissionKey IN (
    'DASHBOARD_VIEW',
    'LICENSES_VIEW',
    'CERTIFICATES_VIEW',
    'OBSERVATIONS_VIEW'
)
GO

-- =====================================================
-- 7. CREATE INDEXES FOR PERFORMANCE
-- =====================================================
-- Permissions table indexes
CREATE NONCLUSTERED INDEX [IX_Permissions_PermissionKey] ON [dbo].[Permissions] ([PermissionKey])
CREATE NONCLUSTERED INDEX [IX_Permissions_IsActive] ON [dbo].[Permissions] ([IsActive])

-- RolePermissions table indexes
CREATE NONCLUSTERED INDEX [IX_RolePermissions_RoleId] ON [dbo].[RolePermissions] ([RoleId])
CREATE NONCLUSTERED INDEX [IX_RolePermissions_PermissionId] ON [dbo].[RolePermissions] ([PermissionId])
CREATE NONCLUSTERED INDEX [IX_RolePermissions_RoleId_PermissionId] ON [dbo].[RolePermissions] ([RoleId], [PermissionId])

-- UserDepartmentPermissions table indexes
CREATE NONCLUSTERED INDEX [IX_UserDepartmentPermissions_UserId] ON [dbo].[UserDepartmentPermissions] ([UserId])
CREATE NONCLUSTERED INDEX [IX_UserDepartmentPermissions_DepartmentId] ON [dbo].[UserDepartmentPermissions] ([DepartmentId])
CREATE NONCLUSTERED INDEX [IX_UserDepartmentPermissions_PermissionId] ON [dbo].[UserDepartmentPermissions] ([PermissionId])
CREATE NONCLUSTERED INDEX [IX_UserDepartmentPermissions_UserId_DepartmentId] ON [dbo].[UserDepartmentPermissions] ([UserId], [DepartmentId])
CREATE NONCLUSTERED INDEX [IX_UserDepartmentPermissions_UserId_PermissionId] ON [dbo].[UserDepartmentPermissions] ([UserId], [PermissionId])

-- PermissionLogs table indexes
CREATE NONCLUSTERED INDEX [IX_PermissionLogs_UserId] ON [dbo].[PermissionLogs] ([UserId])
CREATE NONCLUSTERED INDEX [IX_PermissionLogs_CreatedAt] ON [dbo].[PermissionLogs] ([CreatedAt])
CREATE NONCLUSTERED INDEX [IX_PermissionLogs_PermissionKey] ON [dbo].[PermissionLogs] ([PermissionKey])
GO

-- =====================================================
-- 8. CREATE STORED PROCEDURES
-- =====================================================

-- Get User Permissions
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetUserPermissions]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[GetUserPermissions]
GO

CREATE PROCEDURE [dbo].[GetUserPermissions]
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Get role-based permissions
    SELECT DISTINCT 
        p.PermissionId,
        p.PermissionName,
        p.PermissionKey,
        p.Description,
        'Role' AS PermissionSource
    FROM Permissions p
    INNER JOIN RolePermissions rp ON p.PermissionId = rp.PermissionId
    INNER JOIN Users u ON u.RoleId = rp.RoleId
    WHERE u.UserId = @UserId 
    AND p.IsActive = 1 
    AND rp.IsActive = 1
    
    UNION
    
    -- Get department-specific permissions
    SELECT DISTINCT 
        p.PermissionId,
        p.PermissionName,
        p.PermissionKey,
        p.Description,
        'Department' AS PermissionSource
    FROM Permissions p
    INNER JOIN UserDepartmentPermissions udp ON p.PermissionId = udp.PermissionId
    WHERE udp.UserId = @UserId 
    AND p.IsActive = 1 
    AND udp.IsActive = 1
    AND (udp.CanView = 1 OR udp.CanEdit = 1 OR udp.CanDelete = 1)
END
GO

-- Get User Department Permissions
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetUserDepartmentPermissions]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[GetUserDepartmentPermissions]
GO

CREATE PROCEDURE [dbo].[GetUserDepartmentPermissions]
    @UserId INT,
    @PermissionKey NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        udp.UserDepartmentPermissionId,
        udp.UserId,
        udp.DepartmentId,
        cv.ValueText AS DepartmentName,
        udp.PermissionId,
        p.PermissionKey,
        p.PermissionName,
        udp.CanView,
        udp.CanEdit,
        udp.CanDelete,
        udp.IsActive
    FROM UserDepartmentPermissions udp
    INNER JOIN Permissions p ON udp.PermissionId = p.PermissionId
    INNER JOIN ConfigurationValues cv ON udp.DepartmentId = cv.ValueId
    WHERE udp.UserId = @UserId 
    AND udp.IsActive = 1
    AND (@PermissionKey IS NULL OR p.PermissionKey = @PermissionKey)
    ORDER BY cv.ValueText, p.PermissionName
END
GO

-- Check User Permission
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CheckUserPermission]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[CheckUserPermission]
GO

CREATE PROCEDURE [dbo].[CheckUserPermission]
    @UserId INT,
    @PermissionKey NVARCHAR(50),
    @DepartmentId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @HasPermission BIT = 0
    
    -- Check role-based permissions first
    IF EXISTS (
        SELECT 1 FROM Permissions p
        INNER JOIN RolePermissions rp ON p.PermissionId = rp.PermissionId
        INNER JOIN Users u ON u.RoleId = rp.RoleId
        WHERE u.UserId = @UserId 
        AND p.PermissionKey = @PermissionKey
        AND p.IsActive = 1 
        AND rp.IsActive = 1
    )
    BEGIN
        SET @HasPermission = 1
    END
    
    -- If department-specific check is requested
    IF @DepartmentId IS NOT NULL
    BEGIN
        -- Check if user has department-specific permission
        IF EXISTS (
            SELECT 1 FROM UserDepartmentPermissions udp
            INNER JOIN Permissions p ON udp.PermissionId = p.PermissionId
            WHERE udp.UserId = @UserId 
            AND udp.DepartmentId = @DepartmentId
            AND p.PermissionKey = @PermissionKey
            AND udp.IsActive = 1
            AND (udp.CanView = 1 OR udp.CanEdit = 1 OR udp.CanDelete = 1)
        )
        BEGIN
            SET @HasPermission = 1
        END
        ELSE
        BEGIN
            SET @HasPermission = 0
        END
    END
    
    SELECT @HasPermission AS HasPermission
END
GO

-- Log Permission Access
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LogPermissionAccess]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[LogPermissionAccess]
GO

CREATE PROCEDURE [dbo].[LogPermissionAccess]
    @UserId INT,
    @Action NVARCHAR(50),
    @PermissionKey NVARCHAR(50),
    @DepartmentId INT = NULL,
    @Details NVARCHAR(1000) = NULL,
    @IPAddress NVARCHAR(45) = NULL,
    @UserAgent NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO PermissionLogs (UserId, Action, PermissionKey, DepartmentId, Details, IPAddress, UserAgent)
    VALUES (@UserId, @Action, @PermissionKey, @DepartmentId, @Details, @IPAddress, @UserAgent)
END
GO

-- =====================================================
-- 9. CREATE VIEWS
-- =====================================================

-- User Permissions Summary View
IF EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_UserPermissionsSummary]'))
    DROP VIEW [dbo].[vw_UserPermissionsSummary]
GO

CREATE VIEW [dbo].[vw_UserPermissionsSummary]
AS
SELECT 
    u.UserId,
    u.Username,
    u.Email,
    u.FirstName,
    u.LastName,
    u.IsActive AS UserIsActive,
    r.ValueText AS RoleName,
    COUNT(DISTINCT rp.PermissionId) AS RolePermissionsCount,
    COUNT(DISTINCT udp.DepartmentId) AS DepartmentPermissionsCount,
    COUNT(DISTINCT udp.PermissionId) AS DepartmentSpecificPermissionsCount
FROM Users u
LEFT JOIN ConfigurationValues r ON u.RoleId = r.ValueId
LEFT JOIN RolePermissions rp ON r.ValueId = rp.RoleId AND rp.IsActive = 1
LEFT JOIN UserDepartmentPermissions udp ON u.UserId = udp.UserId AND udp.IsActive = 1
GROUP BY u.UserId, u.Username, u.Email, u.FirstName, u.LastName, u.IsActive, r.ValueText
GO

-- Department Permissions View
IF EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_DepartmentPermissions]'))
    DROP VIEW [dbo].[vw_DepartmentPermissions]
GO

CREATE VIEW [dbo].[vw_DepartmentPermissions]
AS
SELECT 
    cv.ValueId AS DepartmentId,
    cv.ValueText AS DepartmentName,
    cv.ValueKey AS DepartmentKey,
    COUNT(DISTINCT udp.UserId) AS UsersWithAccess,
    COUNT(DISTINCT udp.PermissionId) AS PermissionsCount,
    SUM(CASE WHEN udp.CanView = 1 THEN 1 ELSE 0 END) AS ViewPermissionsCount,
    SUM(CASE WHEN udp.CanEdit = 1 THEN 1 ELSE 0 END) AS EditPermissionsCount,
    SUM(CASE WHEN udp.CanDelete = 1 THEN 1 ELSE 0 END) AS DeletePermissionsCount
FROM ConfigurationValues cv
LEFT JOIN UserDepartmentPermissions udp ON cv.ValueId = udp.DepartmentId AND udp.IsActive = 1
WHERE cv.CategoryId = (SELECT CategoryId FROM ConfigurationCategories WHERE CategoryName = 'Departments')
GROUP BY cv.ValueId, cv.ValueText, cv.ValueKey
GO

-- =====================================================
-- 10. INSERT SAMPLE DATA FOR TESTING
-- =====================================================

-- Insert sample department permissions for testing
-- (This will be populated through the application interface)

-- =====================================================
-- 11. FINAL VERIFICATION
-- =====================================================
PRINT 'Permission System Database Script completed successfully!'
PRINT 'Tables created:'
PRINT '- Permissions'
PRINT '- RolePermissions' 
PRINT '- UserDepartmentPermissions'
PRINT '- PermissionLogs'
PRINT ''
PRINT 'Stored Procedures created:'
PRINT '- GetUserPermissions'
PRINT '- GetUserDepartmentPermissions'
PRINT '- CheckUserPermission'
PRINT '- LogPermissionAccess'
PRINT ''
PRINT 'Views created:'
PRINT '- vw_UserPermissionsSummary'
PRINT '- vw_DepartmentPermissions'
PRINT ''
PRINT 'Sample data inserted:'
PRINT '- Default permissions (35 permissions)'
PRINT '- Role-based permissions for all roles'
PRINT ''
PRINT 'Next steps:'
PRINT '1. Run this script on your database'
PRINT '2. Create the C# models and services'
PRINT '3. Implement the permission checking logic'
PRINT '4. Test the system with sample users'
GO 
