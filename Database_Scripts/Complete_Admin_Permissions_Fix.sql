-- =====================================================
-- COMPLETE ADMIN PERMISSIONS FIX
-- =====================================================
-- This script ensures admin has ALL permissions including organizational
-- =====================================================

USE [HR-Aviation]
GO

PRINT '========================================'
PRINT 'COMPLETE ADMIN PERMISSIONS FIX'
PRINT '========================================'

-- =====================================================
-- STEP 1: ENSURE ADMIN USER EXISTS AND HAS ADMIN ROLE
-- =====================================================

-- Check if admin user exists
IF NOT EXISTS (SELECT 1 FROM users WHERE Username = 'admin')
BEGIN
    PRINT 'Admin user does not exist. Creating admin user...'
    
    -- Create admin user with hashed password (password: admin123)
    INSERT INTO users (Username, PasswordHash, RoleName)
    VALUES ('admin', '$2a$11$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj4J/HS.iK2', 'Admin')
    
    PRINT 'Admin user created successfully!'
END
ELSE
BEGIN
    PRINT 'Admin user already exists. Ensuring Admin role...'
    
    -- Ensure admin user has Admin role
    UPDATE users 
    SET RoleName = 'Admin'
    WHERE Username = 'admin'
    
    PRINT 'Admin user role confirmed!'
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
-- STEP 3: INSERT ALL SYSTEM PERMISSIONS
-- =====================================================

-- Insert Menu Permissions
INSERT INTO Permissions (PermissionKey, PermissionName, PermissionDescription, CategoryName, IsActive, CreatedAt)
SELECT 
    'MENU_' + MenuKey + '_VIEW' as PermissionKey,
    'View ' + MenuName as PermissionName,
    'Permission to view ' + LOWER(MenuName) as PermissionDescription,
    'Menu' as CategoryName,
    1 as IsActive,
    GETDATE() as CreatedAt
FROM (
    VALUES 
        ('PROFILE', 'Profile'),
        ('NOTIFICATIONS', 'Notifications'),
        ('DASHBOARD', 'Dashboard'),
        ('EMPLOYEES', 'Employees'),
        ('CONTROLLERS', 'Controllers'),
        ('LICENSES', 'Licenses'),
        ('CERTIFICATES', 'Certificates'),
        ('OBSERVATIONS', 'Observations'),
        ('CONFIGURATION', 'Configuration'),
        ('PERMISSIONS', 'Permissions'),
        ('REPORTS', 'Reports'),
        ('AUDIT', 'Audit Logs')
) AS MenuItems(MenuKey, MenuName)
WHERE NOT EXISTS (
    SELECT 1 FROM Permissions p 
    WHERE p.PermissionKey = 'MENU_' + MenuKey + '_VIEW'
)

-- Insert Operation Permissions
DECLARE @EntityTypes TABLE (EntityType NVARCHAR(50))
INSERT INTO @EntityTypes VALUES ('Employee'), ('Controller'), ('License'), ('Certificate'), ('Observation'), ('Project'), ('Country'), ('Airport')

DECLARE @OperationTypes TABLE (OperationType NVARCHAR(50))
INSERT INTO @OperationTypes VALUES ('View'), ('Add'), ('Edit'), ('Delete'), ('Export')

-- Insert all possible operation permissions
INSERT INTO Permissions (PermissionKey, PermissionName, PermissionDescription, CategoryName, IsActive, CreatedAt)
SELECT 
    UPPER(et.EntityType) + '_' + UPPER(ot.OperationType) as PermissionKey,
    et.EntityType + ' ' + ot.OperationType as PermissionName,
    'Permission to ' + LOWER(ot.OperationType) + ' ' + LOWER(et.EntityType) as PermissionDescription,
    'Operations' as CategoryName,
    1 as IsActive,
    GETDATE() as CreatedAt
FROM @EntityTypes et
CROSS JOIN @OperationTypes ot
WHERE NOT EXISTS (
    SELECT 1 FROM Permissions p 
    WHERE p.PermissionKey = UPPER(et.EntityType) + '_' + UPPER(ot.OperationType)
)

-- Insert System Management Permissions
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
        ('PERMISSIONS_MANAGE', 'Manage Permissions', 'Can manage user permissions', 'System'),
        ('USERS_MANAGE', 'Manage Users', 'Can manage system users', 'System'),
        ('ROLES_MANAGE', 'Manage Roles', 'Can manage system roles', 'System'),
        ('SYSTEM_SETTINGS', 'System Settings', 'Can modify system settings', 'System'),
        ('AUDIT_LOGS_VIEW', 'View Audit Logs', 'Can view system audit logs', 'System'),
        ('BACKUP_RESTORE', 'Backup & Restore', 'Can backup and restore system data', 'System'),
        ('REPORTS_GENERATE', 'Generate Reports', 'Can generate system reports', 'Reports'),
        ('REPORTS_EXPORT', 'Export Reports', 'Can export system reports', 'Reports')
) AS SystemPermissions(PermissionKey, PermissionName, PermissionDescription, CategoryName)
WHERE NOT EXISTS (
    SELECT 1 FROM Permissions p 
    WHERE p.PermissionKey = SystemPermissions.PermissionKey
)

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
    WHERE p.CategoryName = 'Operations'
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

PRINT '========================================'
PRINT 'COMPLETE ADMIN PERMISSIONS SETUP FINISHED!'
PRINT 'Admin user now has FULL access to ALL system features.'
PRINT 'This includes:'
PRINT '- All menu permissions'
PRINT '- All operation permissions' 
PRINT '- All organizational structure permissions'
PRINT '- All system management permissions'
PRINT '========================================'
GO