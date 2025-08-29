-- =====================================================
-- ANSMS Pro - Permission System Database Script (FIXED)
-- =====================================================
-- This script fixes the issues found in the original permission system script
-- Issues fixed:
-- 1. RoleId NULL insertion problem
-- 2. Invalid column names in stored procedures and views
-- 3. Missing table structure corrections
-- 4. Fixed Users table column references

USE [HR-Aviation]
GO

-- =====================================================
-- STEP 1: DROP EXISTING OBJECTS (if they exist)
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
-- STEP 2: CREATE TABLES
-- =====================================================

-- Create Permissions table
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
    CONSTRAINT [FK_UserDepartmentPermissions_Users] FOREIGN KEY([UserId]) REFERENCES [dbo].[Users] ([UserId]),
    CONSTRAINT [FK_UserDepartmentPermissions_ConfigurationValues] FOREIGN KEY([DepartmentId]) REFERENCES [dbo].[ConfigurationValues] ([ValueId]),
    CONSTRAINT [FK_UserDepartmentPermissions_Permissions] FOREIGN KEY([PermissionId]) REFERENCES [dbo].[Permissions] ([PermissionId])
)
GO

-- Create PermissionLogs table
CREATE TABLE [dbo].[PermissionLogs](
    [LogId] [int] IDENTITY(1,1) NOT NULL,
    [UserId] [int] NOT NULL,
    [PermissionId] [int] NULL,
    [DepartmentId] [int] NULL,
    [Status] [nvarchar](20) NOT NULL,
    [Details] [nvarchar](500) NULL,
    [IpAddress] [nvarchar](45) NULL,
    [UserAgent] [nvarchar](500) NULL,
    [Timestamp] [datetime] NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [PK_PermissionLogs] PRIMARY KEY CLUSTERED ([LogId] ASC),
    CONSTRAINT [FK_PermissionLogs_Users] FOREIGN KEY([UserId]) REFERENCES [dbo].[Users] ([UserId]),
    CONSTRAINT [FK_PermissionLogs_Permissions] FOREIGN KEY([PermissionId]) REFERENCES [dbo].[Permissions] ([PermissionId]),
    CONSTRAINT [FK_PermissionLogs_ConfigurationValues] FOREIGN KEY([DepartmentId]) REFERENCES [dbo].[ConfigurationValues] ([ValueId])
)
GO

-- =====================================================
-- STEP 3: CREATE INDEXES
-- =====================================================

-- Indexes for better performance
CREATE NONCLUSTERED INDEX [IX_RolePermissions_RoleId] ON [dbo].[RolePermissions] ([RoleId])
GO

CREATE NONCLUSTERED INDEX [IX_RolePermissions_PermissionId] ON [dbo].[RolePermissions] ([PermissionId])
GO

CREATE NONCLUSTERED INDEX [IX_UserDepartmentPermissions_UserId] ON [dbo].[UserDepartmentPermissions] ([UserId])
GO

CREATE NONCLUSTERED INDEX [IX_UserDepartmentPermissions_DepartmentId] ON [dbo].[UserDepartmentPermissions] ([DepartmentId])
GO

CREATE NONCLUSTERED INDEX [IX_UserDepartmentPermissions_PermissionId] ON [dbo].[UserDepartmentPermissions] ([PermissionId])
GO

CREATE NONCLUSTERED INDEX [IX_PermissionLogs_UserId] ON [dbo].[PermissionLogs] ([UserId])
GO

CREATE NONCLUSTERED INDEX [IX_PermissionLogs_Timestamp] ON [dbo].[PermissionLogs] ([Timestamp])
GO

-- =====================================================
-- STEP 4: INSERT DEFAULT PERMISSIONS
-- =====================================================

-- Insert default permissions
INSERT INTO [dbo].[Permissions] ([PermissionName], [PermissionKey], [Description], [IsActive])
VALUES 
    -- Dashboard Permissions
    ('View Dashboard', 'DASHBOARD_VIEW', 'Can view the main dashboard', 1),
    ('Export Dashboard', 'DASHBOARD_EXPORT', 'Can export dashboard data', 1),
    
    -- Controller Management Permissions
    ('View Controllers', 'CONTROLLERS_VIEW', 'Can view controller list', 1),
    ('Create Controllers', 'CONTROLLERS_CREATE', 'Can create new controllers', 1),
    ('Edit Controllers', 'CONTROLLERS_EDIT', 'Can edit existing controllers', 1),
    ('Delete Controllers', 'CONTROLLERS_DELETE', 'Can delete controllers', 1),
    ('Export Controllers', 'CONTROLLERS_EXPORT', 'Can export controller data', 1),
    
    -- Staff Management Permissions
    ('View Staff', 'STAFF_VIEW', 'Can view staff list', 1),
    ('Create Staff', 'STAFF_CREATE', 'Can create new staff members', 1),
    ('Edit Staff', 'STAFF_EDIT', 'Can edit existing staff', 1),
    ('Delete Staff', 'STAFF_DELETE', 'Can delete staff members', 1),
    ('Export Staff', 'STAFF_EXPORT', 'Can export staff data', 1),
    
    -- License Management Permissions
    ('View Licenses', 'LICENSES_VIEW', 'Can view license list', 1),
    ('Create Licenses', 'LICENSES_CREATE', 'Can create new licenses', 1),
    ('Edit Licenses', 'LICENSES_EDIT', 'Can edit existing licenses', 1),
    ('Delete Licenses', 'LICENSES_DELETE', 'Can delete licenses', 1),
    ('Export Licenses', 'LICENSES_EXPORT', 'Can export license data', 1),
    
    -- Certificate Management Permissions
    ('View Certificates', 'CERTIFICATES_VIEW', 'Can view certificate list', 1),
    ('Create Certificates', 'CERTIFICATES_CREATE', 'Can create new certificates', 1),
    ('Edit Certificates', 'CERTIFICATES_EDIT', 'Can edit existing certificates', 1),
    ('Delete Certificates', 'CERTIFICATES_DELETE', 'Can delete certificates', 1),
    ('Export Certificates', 'CERTIFICATES_EXPORT', 'Can export certificate data', 1),
    
    -- Observation Management Permissions
    ('View Observations', 'OBSERVATIONS_VIEW', 'Can view observation list', 1),
    ('Create Observations', 'OBSERVATIONS_CREATE', 'Can create new observations', 1),
    ('Edit Observations', 'OBSERVATIONS_EDIT', 'Can edit existing observations', 1),
    ('Delete Observations', 'OBSERVATIONS_DELETE', 'Can delete observations', 1),
    ('Export Observations', 'OBSERVATIONS_EXPORT', 'Can export observation data', 1),
    
    -- System Settings Permissions
    ('View System Settings', 'SYSTEM_SETTINGS_VIEW', 'Can view system settings', 1),
    ('Edit System Settings', 'SYSTEM_SETTINGS_EDIT', 'Can edit system settings', 1),
    ('Configuration Management', 'CONFIGURATION_MANAGEMENT', 'Can manage system configuration', 1),
    ('Roles Management', 'ROLES_MANAGEMENT', 'Can manage user roles and permissions', 1),
    
    -- User Management Permissions
    ('View Users', 'USERS_VIEW', 'Can view user list', 1),
    ('Create Users', 'USERS_CREATE', 'Can create new users', 1),
    ('Edit Users', 'USERS_EDIT', 'Can edit existing users', 1),
    ('Delete Users', 'USERS_DELETE', 'Can delete users', 1),
    
    -- Reports Permissions
    ('View Reports', 'REPORTS_VIEW', 'Can view reports', 1),
    ('Generate Reports', 'REPORTS_GENERATE', 'Can generate new reports', 1),
    ('Export Reports', 'REPORTS_EXPORT', 'Can export reports', 1),
    
    -- Audit Permissions
    ('View Audit Logs', 'AUDIT_LOGS_VIEW', 'Can view audit logs', 1),
    ('View Permission Logs', 'PERMISSION_LOGS_VIEW', 'Can view permission access logs', 1)
GO

-- =====================================================
-- STEP 5: INSERT ROLE-BASED PERMISSIONS
-- =====================================================

-- Get Role IDs from ConfigurationValues
DECLARE @AdminRoleId int, @ManagerRoleId int, @SupervisorRoleId int, @ControllerRoleId int, @EmployeeRoleId int

SELECT @AdminRoleId = ValueId FROM ConfigurationValues WHERE ValueKey = 'Administrator' AND CategoryId = (SELECT CategoryId FROM ConfigurationCategories WHERE CategoryName = 'Roles')
SELECT @ManagerRoleId = ValueId FROM ConfigurationValues WHERE ValueKey = 'Manager' AND CategoryId = (SELECT CategoryId FROM ConfigurationCategories WHERE CategoryName = 'Roles')
SELECT @SupervisorRoleId = ValueId FROM ConfigurationValues WHERE ValueKey = 'Supervisor' AND CategoryId = (SELECT CategoryId FROM ConfigurationCategories WHERE CategoryName = 'Roles')
SELECT @ControllerRoleId = ValueId FROM ConfigurationValues WHERE ValueKey = 'Controller' AND CategoryId = (SELECT CategoryId FROM ConfigurationCategories WHERE CategoryName = 'Roles')
SELECT @EmployeeRoleId = ValueId FROM ConfigurationValues WHERE ValueKey = 'Employee' AND CategoryId = (SELECT CategoryId FROM ConfigurationCategories WHERE CategoryName = 'Roles')

-- Administrator gets all permissions
INSERT INTO RolePermissions (RoleId, PermissionId, IsActive)
SELECT @AdminRoleId, PermissionId, 1
FROM Permissions
WHERE @AdminRoleId IS NOT NULL

-- Manager gets most permissions except system settings
INSERT INTO RolePermissions (RoleId, PermissionId, IsActive)
SELECT @ManagerRoleId, PermissionId, 1
FROM Permissions
WHERE @ManagerRoleId IS NOT NULL
AND PermissionKey NOT IN ('SYSTEM_SETTINGS_EDIT', 'CONFIGURATION_MANAGEMENT', 'ROLES_MANAGEMENT', 'USERS_DELETE')

-- Supervisor gets limited permissions
INSERT INTO RolePermissions (RoleId, PermissionId, IsActive)
SELECT @SupervisorRoleId, PermissionId, 1
FROM Permissions
WHERE @SupervisorRoleId IS NOT NULL
AND PermissionKey IN (
    'DASHBOARD_VIEW',
    'CONTROLLERS_VIEW', 'CONTROLLERS_EDIT',
    'STAFF_VIEW', 'STAFF_EDIT',
    'LICENSES_VIEW', 'LICENSES_EDIT',
    'CERTIFICATES_VIEW', 'CERTIFICATES_EDIT',
    'OBSERVATIONS_VIEW', 'OBSERVATIONS_CREATE', 'OBSERVATIONS_EDIT',
    'REPORTS_VIEW'
)

-- Controller gets basic permissions
INSERT INTO RolePermissions (RoleId, PermissionId, IsActive)
SELECT @ControllerRoleId, PermissionId, 1
FROM Permissions
WHERE @ControllerRoleId IS NOT NULL
AND PermissionKey IN (
    'DASHBOARD_VIEW',
    'LICENSES_VIEW', 'LICENSES_EDIT',
    'CERTIFICATES_VIEW', 'CERTIFICATES_EDIT',
    'OBSERVATIONS_VIEW', 'OBSERVATIONS_CREATE', 'OBSERVATIONS_EDIT'
)

-- Employee gets minimal permissions
INSERT INTO RolePermissions (RoleId, PermissionId, IsActive)
SELECT @EmployeeRoleId, PermissionId, 1
FROM Permissions
WHERE @EmployeeRoleId IS NOT NULL
AND PermissionKey IN (
    'DASHBOARD_VIEW',
    'LICENSES_VIEW',
    'CERTIFICATES_VIEW',
    'OBSERVATIONS_VIEW'
)
GO

-- =====================================================
-- STEP 6: CREATE STORED PROCEDURES
-- =====================================================

-- Get User Permissions
CREATE PROCEDURE [dbo].[GetUserPermissions]
    @UserId int
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Get role-based permissions (using RoleName from Users table)
    SELECT DISTINCT 
        p.PermissionId,
        p.PermissionName,
        p.PermissionKey,
        p.Description,
        'Role' as PermissionSource,
        rp.IsActive
    FROM Permissions p
    INNER JOIN RolePermissions rp ON p.PermissionId = rp.PermissionId
    INNER JOIN ConfigurationValues cv ON rp.RoleId = cv.ValueId
    INNER JOIN Users u ON u.RoleName = cv.ValueKey
    WHERE u.UserId = @UserId AND rp.IsActive = 1 AND p.IsActive = 1
    
    UNION
    
    -- Get department-specific permissions
    SELECT DISTINCT 
        p.PermissionId,
        p.PermissionName,
        p.PermissionKey,
        p.Description,
        'Department' as PermissionSource,
        udp.IsActive
    FROM Permissions p
    INNER JOIN UserDepartmentPermissions udp ON p.PermissionId = udp.PermissionId
    WHERE udp.UserId = @UserId AND udp.IsActive = 1 AND p.IsActive = 1
END
GO

-- Get User Department Permissions
CREATE PROCEDURE [dbo].[GetUserDepartmentPermissions]
    @UserId int
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        udp.UserDepartmentPermissionId,
        udp.UserId,
        udp.DepartmentId,
        udp.PermissionId,
        udp.CanView,
        udp.CanEdit,
        udp.CanDelete,
        udp.IsActive,
        udp.CreatedAt,
        udp.UpdatedAt,
        u.Username as UserName,
        u.Username as UserFullName, -- Using Username as FullName since FullName doesn't exist
        cv.ValueText as DepartmentName,
        p.PermissionName,
        p.PermissionKey
    FROM UserDepartmentPermissions udp
    INNER JOIN Users u ON udp.UserId = u.UserId
    INNER JOIN ConfigurationValues cv ON udp.DepartmentId = cv.ValueId
    INNER JOIN Permissions p ON udp.PermissionId = p.PermissionId
    WHERE udp.UserId = @UserId
    ORDER BY cv.ValueText, p.PermissionName
END
GO

-- Check User Permission
CREATE PROCEDURE [dbo].[CheckUserPermission]
    @UserId int,
    @PermissionKey nvarchar(50),
    @DepartmentId int = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @HasPermission bit = 0
    
    -- Check role-based permissions first (using RoleName from Users table)
    IF EXISTS (
        SELECT 1 FROM Permissions p
        INNER JOIN RolePermissions rp ON p.PermissionId = rp.PermissionId
        INNER JOIN ConfigurationValues cv ON rp.RoleId = cv.ValueId
        INNER JOIN Users u ON u.RoleName = cv.ValueKey
        WHERE u.UserId = @UserId 
        AND p.PermissionKey = @PermissionKey 
        AND rp.IsActive = 1 
        AND p.IsActive = 1
    )
    BEGIN
        SET @HasPermission = 1
    END
    
    -- If department-specific check is requested, also check department permissions
    IF @DepartmentId IS NOT NULL
    BEGIN
        IF EXISTS (
            SELECT 1 FROM UserDepartmentPermissions udp
            INNER JOIN Permissions p ON udp.PermissionId = p.PermissionId
            WHERE udp.UserId = @UserId 
            AND udp.DepartmentId = @DepartmentId
            AND p.PermissionKey = @PermissionKey 
            AND udp.IsActive = 1 
            AND p.IsActive = 1
        )
        BEGIN
            SET @HasPermission = 1
        END
    END
    
    SELECT @HasPermission as HasPermission
END
GO

-- Log Permission Access
CREATE PROCEDURE [dbo].[LogPermissionAccess]
    @UserId int,
    @Status nvarchar(20),
    @PermissionKey nvarchar(50),
    @DepartmentId int = NULL,
    @Details nvarchar(500) = NULL,
    @IpAddress nvarchar(45) = NULL,
    @UserAgent nvarchar(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @PermissionId int = NULL
    
    -- Get PermissionId if permission exists
    SELECT @PermissionId = PermissionId 
    FROM Permissions 
    WHERE PermissionKey = @PermissionKey
    
    INSERT INTO PermissionLogs (UserId, PermissionId, DepartmentId, Status, Details, IpAddress, UserAgent, Timestamp)
    VALUES (@UserId, @PermissionId, @DepartmentId, @Status, @Details, @IpAddress, @UserAgent, GETDATE())
END
GO

-- =====================================================
-- STEP 7: CREATE VIEWS
-- =====================================================

-- User Permissions Summary View
CREATE VIEW [dbo].[vw_UserPermissionsSummary]
AS
SELECT 
    u.UserId,
    u.Username,
    u.Username as UserFullName, -- Using Username as FullName since FullName doesn't exist
    u.RoleName as RoleName,
    COUNT(DISTINCT rp.PermissionId) as RolePermissionsCount,
    COUNT(DISTINCT udp.PermissionId) as DepartmentPermissionsCount,
    COUNT(DISTINCT rp.PermissionId) + COUNT(DISTINCT udp.PermissionId) as TotalPermissionsCount
FROM Users u
LEFT JOIN RolePermissions rp ON EXISTS (
    SELECT 1 FROM ConfigurationValues cv 
    WHERE cv.ValueKey = u.RoleName 
    AND cv.ValueId = rp.RoleId 
    AND rp.IsActive = 1
)
LEFT JOIN UserDepartmentPermissions udp ON u.UserId = udp.UserId AND udp.IsActive = 1
GROUP BY u.UserId, u.Username, u.RoleName
GO

-- Department Permissions View
CREATE VIEW [dbo].[vw_DepartmentPermissions]
AS
SELECT 
    udp.UserDepartmentPermissionId,
    udp.UserId,
    udp.DepartmentId,
    udp.PermissionId,
    udp.CanView,
    udp.CanEdit,
    udp.CanDelete,
    udp.IsActive,
    u.Username as UserName,
    u.Username as UserFullName, -- Using Username as FullName since FullName doesn't exist
    cv.ValueText as DepartmentName,
    p.PermissionName,
    p.PermissionKey
FROM UserDepartmentPermissions udp
INNER JOIN Users u ON udp.UserId = u.UserId
INNER JOIN ConfigurationValues cv ON udp.DepartmentId = cv.ValueId
INNER JOIN Permissions p ON udp.PermissionId = p.PermissionId
GO

-- =====================================================
-- STEP 8: VERIFICATION
-- =====================================================

-- Verify tables were created
SELECT 'Tables Created:' as Status
UNION ALL
SELECT 'Permissions' WHERE OBJECT_ID('Permissions', 'U') IS NOT NULL
UNION ALL
SELECT 'RolePermissions' WHERE OBJECT_ID('RolePermissions', 'U') IS NOT NULL
UNION ALL
SELECT 'UserDepartmentPermissions' WHERE OBJECT_ID('UserDepartmentPermissions', 'U') IS NOT NULL
UNION ALL
SELECT 'PermissionLogs' WHERE OBJECT_ID('PermissionLogs', 'U') IS NOT NULL

-- Verify stored procedures were created
SELECT 'Stored Procedures Created:' as Status
UNION ALL
SELECT 'GetUserPermissions' WHERE OBJECT_ID('GetUserPermissions', 'P') IS NOT NULL
UNION ALL
SELECT 'GetUserDepartmentPermissions' WHERE OBJECT_ID('GetUserDepartmentPermissions', 'P') IS NOT NULL
UNION ALL
SELECT 'CheckUserPermission' WHERE OBJECT_ID('CheckUserPermission', 'P') IS NOT NULL
UNION ALL
SELECT 'LogPermissionAccess' WHERE OBJECT_ID('LogPermissionAccess', 'P') IS NOT NULL

-- Verify views were created
SELECT 'Views Created:' as Status
UNION ALL
SELECT 'vw_UserPermissionsSummary' WHERE OBJECT_ID('vw_UserPermissionsSummary', 'V') IS NOT NULL
UNION ALL
SELECT 'vw_DepartmentPermissions' WHERE OBJECT_ID('vw_DepartmentPermissions', 'V') IS NOT NULL

-- Show permission count
SELECT 'Permission System Setup Complete!' as Status
SELECT COUNT(*) as TotalPermissions FROM Permissions
SELECT COUNT(*) as TotalRolePermissions FROM RolePermissions

PRINT 'Permission System Database Script completed successfully!'
PRINT 'All tables, stored procedures, and views have been created.'
PRINT 'Default permissions and role assignments have been configured.'
GO 
