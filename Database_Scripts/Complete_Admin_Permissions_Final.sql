-- =====================================================
-- COMPLETE ADMIN PERMISSIONS FINAL - ULTIMATE FIX
-- =====================================================
-- This script ensures admin has ALL permissions including missing ones
-- =====================================================

USE [HR-Aviation]
GO

PRINT '========================================'
PRINT 'COMPLETE ADMIN PERMISSIONS FINAL FIX'
PRINT '========================================'

-- =====================================================
-- STEP 1: ENSURE ADMIN USER EXISTS
-- =====================================================

IF NOT EXISTS (SELECT 1 FROM users WHERE Username = 'admin')
BEGIN
    PRINT 'Creating admin user...'
    INSERT INTO users (Username, PasswordHash, RoleName)
    VALUES ('admin', '$2a$11$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj4J/HS.iK2', 'Admin')
    PRINT 'Admin user created successfully!'
END
ELSE
BEGIN
    PRINT 'Admin user exists. Ensuring Admin role...'
    UPDATE users SET RoleName = 'Admin' WHERE Username = 'admin'
    PRINT 'Admin role confirmed!'
END

-- =====================================================
-- STEP 2: ENSURE ALL PERMISSION TABLES EXIST
-- =====================================================

-- Create Permissions table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Permissions' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[Permissions](
        [PermissionId] [int] IDENTITY(1,1) NOT NULL,
        [PermissionKey] [nvarchar](100) NOT NULL,
        [PermissionName] [nvarchar](200) NOT NULL,
        [PermissionDescription] [nvarchar](500) NULL,
        [CategoryName] [nvarchar](50) NOT NULL DEFAULT 'General',
        [IsActive] [bit] NOT NULL DEFAULT 1,
        [CreatedAt] [datetime] NOT NULL DEFAULT GETDATE(),
        CONSTRAINT [PK_Permissions] PRIMARY KEY CLUSTERED ([PermissionId] ASC),
        CONSTRAINT [UK_Permissions_Key] UNIQUE ([PermissionKey])
    );
    PRINT 'Created Permissions table'
END

-- Create UserMenuPermissions table if not exists
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='UserMenuPermissions' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[UserMenuPermissions](
        [UserMenuPermissionId] [int] IDENTITY(1,1) NOT NULL,
        [UserId] [int] NOT NULL,
        [MenuKey] [nvarchar](50) NOT NULL,
        [IsVisible] [bit] NOT NULL DEFAULT 0,
        [IsActive] [bit] NOT NULL DEFAULT 1,
        [CreatedAt] [datetime] NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] [datetime] NULL,
        CONSTRAINT [PK_UserMenuPermissions] PRIMARY KEY CLUSTERED ([UserMenuPermissionId] ASC),
        CONSTRAINT [UK_UserMenuPermissions_User_Menu] UNIQUE ([UserId], [MenuKey])
    );
    PRINT 'Created UserMenuPermissions table'
END

-- Create UserOperationPermissions table if not exists
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='UserOperationPermissions' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[UserOperationPermissions](
        [UserOperationPermissionId] [int] IDENTITY(1,1) NOT NULL,
        [UserId] [int] NOT NULL,
        [PermissionId] [int] NOT NULL,
        [EntityType] [nvarchar](50) NOT NULL,
        [OperationType] [nvarchar](50) NOT NULL,
        [Scope] [nvarchar](50) NOT NULL DEFAULT 'All',
        [ScopeId] [int] NULL,
        [IsAllowed] [bit] NOT NULL DEFAULT 0,
        [IsActive] [bit] NOT NULL DEFAULT 1,
        [CreatedAt] [datetime] NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] [datetime] NULL,
        CONSTRAINT [PK_UserOperationPermissions] PRIMARY KEY CLUSTERED ([UserOperationPermissionId] ASC),
        CONSTRAINT [UK_UserOperationPermissions_User_Entity_Operation] UNIQUE ([UserId], [EntityType], [OperationType], [Scope], [ScopeId])
    );
    PRINT 'Created UserOperationPermissions table'
END

-- Create UserOrganizationalPermissions table if not exists
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
-- STEP 3: INSERT ALL MISSING PERMISSIONS
-- =====================================================

-- Insert ALL system permissions including missing ones
INSERT INTO Permissions (PermissionKey, PermissionName, PermissionDescription, CategoryName, IsActive, CreatedAt)
SELECT 
    PermissionKey,
    PermissionName,
    PermissionDescription,
    CategoryName,
    1 as IsActive,
    GETDATE() as CreatedAt
FROM (
    VALUES 
        -- Menu Permissions
        ('MENU_PROFILE_VIEW', 'View Profile', 'Permission to view profile', 'Menu'),
        ('MENU_NOTIFICATIONS_VIEW', 'View Notifications', 'Permission to view notifications', 'Menu'),
        ('MENU_DASHBOARD_VIEW', 'View Dashboard', 'Permission to view dashboard', 'Menu'),
        ('MENU_EMPLOYEES_VIEW', 'View Employees', 'Permission to view employees', 'Menu'),
        ('MENU_CONTROLLERS_VIEW', 'View Controllers', 'Permission to view controllers', 'Menu'),
        ('MENU_LICENSES_VIEW', 'View Licenses', 'Permission to view licenses', 'Menu'),
        ('MENU_CERTIFICATES_VIEW', 'View Certificates', 'Permission to view certificates', 'Menu'),
        ('MENU_OBSERVATIONS_VIEW', 'View Observations', 'Permission to view observations', 'Menu'),
        ('MENU_CONFIGURATION_VIEW', 'View Configuration', 'Permission to view configuration', 'Menu'),
        ('MENU_PERMISSIONS_VIEW', 'View Permissions', 'Permission to view permissions', 'Menu'),
        ('MENU_REPORTS_VIEW', 'View Reports', 'Permission to view reports', 'Menu'),
        ('MENU_AUDIT_VIEW', 'View Audit Logs', 'Permission to view audit logs', 'Menu'),
        
        -- Operation Permissions
        ('EMPLOYEE_VIEW', 'View Employee', 'Permission to view employees', 'Operations'),
        ('EMPLOYEE_ADD', 'Add Employee', 'Permission to add employees', 'Operations'),
        ('EMPLOYEE_EDIT', 'Edit Employee', 'Permission to edit employees', 'Operations'),
        ('EMPLOYEE_DELETE', 'Delete Employee', 'Permission to delete employees', 'Operations'),
        ('EMPLOYEE_EXPORT', 'Export Employee', 'Permission to export employees', 'Operations'),
        ('CONTROLLER_VIEW', 'View Controller', 'Permission to view controllers', 'Operations'),
        ('CONTROLLER_ADD', 'Add Controller', 'Permission to add controllers', 'Operations'),
        ('CONTROLLER_EDIT', 'Edit Controller', 'Permission to edit controllers', 'Operations'),
        ('CONTROLLER_DELETE', 'Delete Controller', 'Permission to delete controllers', 'Operations'),
        ('CONTROLLER_EXPORT', 'Export Controller', 'Permission to export controllers', 'Operations'),
        ('LICENSE_VIEW', 'View License', 'Permission to view licenses', 'Operations'),
        ('LICENSE_ADD', 'Add License', 'Permission to add licenses', 'Operations'),
        ('LICENSE_EDIT', 'Edit License', 'Permission to edit licenses', 'Operations'),
        ('LICENSE_DELETE', 'Delete License', 'Permission to delete licenses', 'Operations'),
        ('LICENSE_EXPORT', 'Export License', 'Permission to export licenses', 'Operations'),
        ('CERTIFICATE_VIEW', 'View Certificate', 'Permission to view certificates', 'Operations'),
        ('CERTIFICATE_ADD', 'Add Certificate', 'Permission to add certificates', 'Operations'),
        ('CERTIFICATE_EDIT', 'Edit Certificate', 'Permission to edit certificates', 'Operations'),
        ('CERTIFICATE_DELETE', 'Delete Certificate', 'Permission to delete certificates', 'Operations'),
        ('CERTIFICATE_EXPORT', 'Export Certificate', 'Permission to export certificates', 'Operations'),
        ('OBSERVATION_VIEW', 'View Observation', 'Permission to view observations', 'Operations'),
        ('OBSERVATION_ADD', 'Add Observation', 'Permission to add observations', 'Operations'),
        ('OBSERVATION_EDIT', 'Edit Observation', 'Permission to edit observations', 'Operations'),
        ('OBSERVATION_DELETE', 'Delete Observation', 'Permission to delete observations', 'Operations'),
        ('OBSERVATION_EXPORT', 'Export Observation', 'Permission to export observations', 'Operations'),
        ('PROJECT_VIEW', 'View Project', 'Permission to view projects', 'Operations'),
        ('PROJECT_ADD', 'Add Project', 'Permission to add projects', 'Operations'),
        ('PROJECT_EDIT', 'Edit Project', 'Permission to edit projects', 'Operations'),
        ('PROJECT_DELETE', 'Delete Project', 'Permission to delete projects', 'Operations'),
        ('PROJECT_EXPORT', 'Export Project', 'Permission to export projects', 'Operations'),
        ('COUNTRY_VIEW', 'View Country', 'Permission to view countries', 'Operations'),
        ('COUNTRY_ADD', 'Add Country', 'Permission to add countries', 'Operations'),
        ('COUNTRY_EDIT', 'Edit Country', 'Permission to edit countries', 'Operations'),
        ('COUNTRY_DELETE', 'Delete Country', 'Permission to delete countries', 'Operations'),
        ('COUNTRY_EXPORT', 'Export Country', 'Permission to export countries', 'Operations'),
        ('AIRPORT_VIEW', 'View Airport', 'Permission to view airports', 'Operations'),
        ('AIRPORT_ADD', 'Add Airport', 'Permission to add airports', 'Operations'),
        ('AIRPORT_EDIT', 'Edit Airport', 'Permission to edit airports', 'Operations'),
        ('AIRPORT_DELETE', 'Delete Airport', 'Permission to delete airports', 'Operations'),
        ('AIRPORT_EXPORT', 'Export Airport', 'Permission to export airports', 'Operations'),
        
        -- System Management Permissions
        ('PERMISSIONS_MANAGE', 'Manage Permissions', 'Can manage user permissions', 'System'),
        ('USERS_MANAGE', 'Manage Users', 'Can manage system users', 'System'),
        ('ROLES_MANAGE', 'Manage Roles', 'Can manage system roles', 'System'),
        ('SYSTEM_SETTINGS', 'System Settings', 'Can modify system settings', 'System'),
        ('AUDIT_LOGS_VIEW', 'View Audit Logs', 'Can view system audit logs', 'System'),
        ('BACKUP_RESTORE', 'Backup & Restore', 'Can backup and restore system data', 'System'),
        ('REPORTS_GENERATE', 'Generate Reports', 'Can generate system reports', 'Reports'),
        ('REPORTS_EXPORT', 'Export Reports', 'Can export system reports', 'Reports'),
        
        -- Missing Controller Permissions (Legacy Support)
        ('CONTROLLERS_VIEW', 'View Controllers', 'Permission to view controllers', 'Legacy'),
        ('CONTROLLERS_EDIT', 'Edit Controllers', 'Permission to edit controllers', 'Legacy'),
        ('CONTROLLERS_ADD', 'Add Controllers', 'Permission to add controllers', 'Legacy'),
        ('CONTROLLERS_DELETE', 'Delete Controllers', 'Permission to delete controllers', 'Legacy'),
        ('CONTROLLERS_EXPORT', 'Export Controllers', 'Permission to export controllers', 'Legacy'),
        ('LICENSES_VIEW', 'View Licenses', 'Permission to view licenses', 'Legacy'),
        ('LICENSES_EDIT', 'Edit Licenses', 'Permission to edit licenses', 'Legacy'),
        ('LICENSES_ADD', 'Add Licenses', 'Permission to add licenses', 'Legacy'),
        ('LICENSES_DELETE', 'Delete Licenses', 'Permission to delete licenses', 'Legacy'),
        ('LICENSES_EXPORT', 'Export Licenses', 'Permission to export licenses', 'Legacy'),
        ('CERTIFICATES_VIEW', 'View Certificates', 'Permission to view certificates', 'Legacy'),
        ('CERTIFICATES_EDIT', 'Edit Certificates', 'Permission to edit certificates', 'Legacy'),
        ('CERTIFICATES_ADD', 'Add Certificates', 'Permission to add certificates', 'Legacy'),
        ('CERTIFICATES_DELETE', 'Delete Certificates', 'Permission to delete certificates', 'Legacy'),
        ('CERTIFICATES_EXPORT', 'Export Certificates', 'Permission to export certificates', 'Legacy'),
        ('OBSERVATIONS_VIEW', 'View Observations', 'Permission to view observations', 'Legacy'),
        ('OBSERVATIONS_EDIT', 'Edit Observations', 'Permission to edit observations', 'Legacy'),
        ('OBSERVATIONS_ADD', 'Add Observations', 'Permission to add observations', 'Legacy'),
        ('OBSERVATIONS_DELETE', 'Delete Observations', 'Permission to delete observations', 'Legacy'),
        ('OBSERVATIONS_EXPORT', 'Export Observations', 'Permission to export observations', 'Legacy'),
        ('EMPLOYEES_VIEW', 'View Employees', 'Permission to view employees', 'Legacy'),
        ('EMPLOYEES_EDIT', 'Edit Employees', 'Permission to edit employees', 'Legacy'),
        ('EMPLOYEES_ADD', 'Add Employees', 'Permission to add employees', 'Legacy'),
        ('EMPLOYEES_DELETE', 'Delete Employees', 'Permission to delete employees', 'Legacy'),
        ('EMPLOYEES_EXPORT', 'Export Employees', 'Permission to export employees', 'Legacy'),
        ('DASHBOARD_VIEW', 'View Dashboard', 'Permission to view dashboard', 'Legacy'),
        ('STAFF_VIEW', 'View Staff', 'Permission to view staff', 'Legacy'),
        ('STAFF_EDIT', 'Edit Staff', 'Permission to edit staff', 'Legacy'),
        ('SYSTEM_SETTINGS_VIEW', 'View System Settings', 'Permission to view system settings', 'Legacy'),
        ('CONFIGURATION_MANAGEMENT', 'Configuration Management', 'Permission to manage configuration', 'Legacy'),
        ('ROLES_MANAGEMENT', 'Roles Management', 'Permission to manage roles', 'Legacy'),
        ('REPORTS_VIEW', 'View Reports', 'Permission to view reports', 'Legacy'),
        ('PERMISSION_LOGS_VIEW', 'View Permission Logs', 'Permission to view permission logs', 'Legacy'),
        
        -- CRITICAL MISSING PERMISSIONS
        ('ORGANIZATION_VIEW', 'View Organization', 'Permission to view organization structure', 'Critical'),
        ('ORGANIZATION_EDIT', 'Edit Organization', 'Permission to edit organization structure', 'Critical'),
        ('ORGANIZATION_ADD', 'Add Organization', 'Permission to add organization structure', 'Critical'),
        ('ORGANIZATION_DELETE', 'Delete Organization', 'Permission to delete organization structure', 'Critical'),
        ('ORGANIZATION_EXPORT', 'Export Organization', 'Permission to export organization structure', 'Critical')
        
) AS AllPermissions(PermissionKey, PermissionName, PermissionDescription, CategoryName)
WHERE NOT EXISTS (
    SELECT 1 FROM Permissions p 
    WHERE p.PermissionKey = AllPermissions.PermissionKey
)

PRINT 'All permissions inserted successfully!'

-- =====================================================
-- STEP 4: GRANT ALL PERMISSIONS TO ADMIN USER
-- =====================================================

DECLARE @AdminUserId INT
SELECT @AdminUserId = UserId FROM users WHERE Username = 'admin'

IF @AdminUserId IS NOT NULL
BEGIN
    PRINT 'Granting ALL permissions to admin user (ID: ' + CAST(@AdminUserId AS VARCHAR(10)) + ')'
    
    -- Grant all menu permissions to admin
    INSERT INTO UserMenuPermissions (UserId, MenuKey, IsVisible, IsActive, CreatedAt)
    SELECT 
        @AdminUserId,
        MenuKey,
        1 as IsVisible,
        1 as IsActive,
        GETDATE() as CreatedAt
    FROM (
        VALUES 
            ('PROFILE'), ('NOTIFICATIONS'), ('DASHBOARD'), ('EMPLOYEES'), 
            ('CONTROLLERS'), ('LICENSES'), ('CERTIFICATES'), ('OBSERVATIONS'), 
            ('CONFIGURATION'), ('PERMISSIONS'), ('REPORTS'), ('AUDIT')
    ) AS MenuItems(MenuKey)
    WHERE NOT EXISTS (
        SELECT 1 FROM UserMenuPermissions ump 
        WHERE ump.UserId = @AdminUserId AND ump.MenuKey = MenuItems.MenuKey
    )
    
    -- Grant all operation permissions to admin
    INSERT INTO UserOperationPermissions (UserId, PermissionId, EntityType, OperationType, Scope, IsAllowed, IsActive, CreatedAt)
    SELECT 
        @AdminUserId,
        p.PermissionId,
        CASE 
            WHEN p.PermissionKey LIKE 'EMPLOYEE_%' THEN 'Employee'
            WHEN p.PermissionKey LIKE 'CONTROLLER_%' THEN 'Controller'
            WHEN p.PermissionKey LIKE 'LICENSE_%' THEN 'License'
            WHEN p.PermissionKey LIKE 'CERTIFICATE_%' THEN 'Certificate'
            WHEN p.PermissionKey LIKE 'OBSERVATION_%' THEN 'Observation'
            WHEN p.PermissionKey LIKE 'PROJECT_%' THEN 'Project'
            WHEN p.PermissionKey LIKE 'COUNTRY_%' THEN 'Country'
            WHEN p.PermissionKey LIKE 'AIRPORT_%' THEN 'Airport'
            ELSE 'General'
        END as EntityType,
        CASE 
            WHEN p.PermissionKey LIKE '%_VIEW' THEN 'View'
            WHEN p.PermissionKey LIKE '%_ADD' THEN 'Add'
            WHEN p.PermissionKey LIKE '%_EDIT' THEN 'Edit'
            WHEN p.PermissionKey LIKE '%_DELETE' THEN 'Delete'
            WHEN p.PermissionKey LIKE '%_EXPORT' THEN 'Export'
            ELSE 'View'
        END as OperationType,
        'All' as Scope,
        1 as IsAllowed,
        1 as IsActive,
        GETDATE() as CreatedAt
    FROM Permissions p
    WHERE p.CategoryName IN ('Operations', 'Legacy', 'Critical')
    AND NOT EXISTS (
        SELECT 1 FROM UserOperationPermissions uop 
        WHERE uop.UserId = @AdminUserId 
        AND uop.PermissionId = p.PermissionId
    )
    
    -- Grant all organizational permissions to admin
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
    
    -- Add specific organizational permissions if tables exist
    -- Countries
    IF EXISTS (SELECT * FROM sysobjects WHERE name='Countries' AND xtype='U')
    BEGIN
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
    END
    
    -- Airports
    IF EXISTS (SELECT * FROM sysobjects WHERE name='Airports' AND xtype='U')
    BEGIN
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
    END
    
    PRINT 'ALL permissions granted to admin user successfully!'
END
ELSE
BEGIN
    PRINT 'ERROR: Admin user not found!'
END

-- =====================================================
-- STEP 5: CREATE/UPDATE STORED PROCEDURES
-- =====================================================

-- Drop existing procedures if they exist
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CanUserViewMenu]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[CanUserViewMenu]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CanUserPerformOperation]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[CanUserPerformOperation]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CheckUserPermission]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[CheckUserPermission]
GO

-- Create CanUserViewMenu stored procedure
CREATE PROCEDURE [dbo].[CanUserViewMenu]
    @UserId INT,
    @MenuKey NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @CanView BIT = 0;
    
    -- Check if user is Admin (Admin has all permissions)
    IF EXISTS (SELECT 1 FROM users WHERE UserId = @UserId AND RoleName = 'Admin')
    BEGIN
        SET @CanView = 1;
    END
    ELSE
    BEGIN
        -- Check if user has specific menu permission
        IF EXISTS (
            SELECT 1 FROM UserMenuPermissions ump
            WHERE ump.UserId = @UserId 
            AND ump.MenuKey = @MenuKey 
            AND ump.IsActive = 1 
            AND ump.IsVisible = 1
        )
        BEGIN
            SET @CanView = 1;
        END
    END
    
    SELECT @CanView AS CanView;
END
GO

-- Create CanUserPerformOperation stored procedure
CREATE PROCEDURE [dbo].[CanUserPerformOperation]
    @UserId INT,
    @EntityType NVARCHAR(50),
    @OperationType NVARCHAR(50),
    @Scope NVARCHAR(50) = 'All',
    @ScopeId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @CanPerform BIT = 0;
    
    -- Check if user is Admin (Admin has all permissions)
    IF EXISTS (SELECT 1 FROM users WHERE UserId = @UserId AND RoleName = 'Admin')
    BEGIN
        SET @CanPerform = 1;
    END
    ELSE
    BEGIN
        -- Check if user has specific operation permission
        IF EXISTS (
            SELECT 1 FROM UserOperationPermissions uop
            WHERE uop.UserId = @UserId 
            AND uop.EntityType = @EntityType
            AND uop.OperationType = @OperationType
            AND uop.IsActive = 1 
            AND uop.IsAllowed = 1
            AND (uop.Scope = 'All' OR (uop.Scope = @Scope AND uop.ScopeId = @ScopeId))
        )
        BEGIN
            SET @CanPerform = 1;
        END
    END
    
    SELECT @CanPerform AS CanPerform;
END
GO

-- Create CheckUserPermission stored procedure (legacy support)
CREATE PROCEDURE [dbo].[CheckUserPermission]
    @UserId INT,
    @PermissionKey NVARCHAR(50),
    @DepartmentId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @HasPermission BIT = 0;
    
    -- Check if user is Admin (Admin has all permissions)
    IF EXISTS (SELECT 1 FROM users WHERE UserId = @UserId AND RoleName = 'Admin')
    BEGIN
        SET @HasPermission = 1;
    END
    ELSE
    BEGIN
        -- Check if user has specific permission
        IF EXISTS (
            SELECT 1 FROM UserOperationPermissions uop
            INNER JOIN Permissions p ON uop.PermissionId = p.PermissionId
            WHERE uop.UserId = @UserId 
            AND p.PermissionKey = @PermissionKey
            AND uop.IsActive = 1 
            AND uop.IsAllowed = 1
        )
        BEGIN
            SET @HasPermission = 1;
        END
    END
    
    SELECT @HasPermission AS HasPermission;
END
GO

-- =====================================================
-- STEP 6: FINAL VERIFICATION
-- =====================================================

PRINT '========================================'
PRINT 'FINAL VERIFICATION RESULTS'
PRINT '========================================'

-- Check admin user
SELECT 
    'Admin User' as CheckType,
    UserId as UserId,
    Username as Username,
    RoleName as Role
FROM users 
WHERE Username = 'admin'

-- Check admin menu permissions
SELECT 
    'Admin Menu Permissions' as CheckType,
    COUNT(*) as TotalMenuPermissions
FROM UserMenuPermissions ump
INNER JOIN users u ON ump.UserId = u.UserId
WHERE u.Username = 'admin' AND ump.IsVisible = 1

-- Check admin operation permissions
SELECT 
    'Admin Operation Permissions' as CheckType,
    COUNT(*) as TotalOperationPermissions
FROM UserOperationPermissions uop
INNER JOIN users u ON uop.UserId = u.UserId
WHERE u.Username = 'admin' AND uop.IsAllowed = 1

-- Check admin organizational permissions
SELECT 
    'Admin Organizational Permissions' as CheckType,
    COUNT(*) as TotalOrganizationalPermissions
FROM UserOrganizationalPermissions uop
INNER JOIN users u ON uop.UserId = u.UserId
WHERE u.Username = 'admin' AND uop.IsActive = 1

-- Check total permissions in system
SELECT 
    'Total System Permissions' as CheckType,
    COUNT(*) as TotalPermissions
FROM Permissions
WHERE IsActive = 1

-- Check for ORGANIZATION_VIEW permission specifically
SELECT 
    'ORGANIZATION_VIEW Permission' as CheckType,
    CASE WHEN EXISTS (SELECT 1 FROM Permissions WHERE PermissionKey = 'ORGANIZATION_VIEW') 
         THEN 'EXISTS' 
         ELSE 'MISSING' 
    END as Status

PRINT '========================================'
PRINT 'COMPLETE ADMIN PERMISSIONS FINAL FIX COMPLETE!'
PRINT 'Admin user now has FULL access to ALL system features.'
PRINT 'This includes:'
PRINT '- All menu permissions'
PRINT '- All operation permissions' 
PRINT '- All organizational structure permissions'
PRINT '- All system management permissions'
PRINT '- All missing permissions including ORGANIZATION_VIEW'
PRINT '========================================'
GO