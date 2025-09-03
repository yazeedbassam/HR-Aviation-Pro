-- Advanced Permission System - Fixed Version
-- This script creates the advanced permission system without data insertion errors

USE [HR-Aviation]
GO

-- Create UserMenuPermissions table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UserMenuPermissions]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[UserMenuPermissions](
        [UserMenuPermissionId] [int] IDENTITY(1,1) NOT NULL,
        [UserId] [int] NOT NULL,
        [MenuKey] [nvarchar](50) NOT NULL,
        [IsVisible] [bit] NOT NULL DEFAULT 1,
        [IsActive] [bit] NOT NULL DEFAULT 1,
        [CreatedAt] [datetime] NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] [datetime] NULL,
        CONSTRAINT [PK_UserMenuPermissions] PRIMARY KEY CLUSTERED ([UserMenuPermissionId] ASC),
        CONSTRAINT [FK_UserMenuPermissions_Users] FOREIGN KEY([UserId]) REFERENCES [dbo].[users] ([userid])
    )
    PRINT 'UserMenuPermissions table created successfully'
END
ELSE
    PRINT 'UserMenuPermissions table already exists'
GO

-- Create UserOrganizationalPermissions table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UserOrganizationalPermissions]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[UserOrganizationalPermissions](
        [UserOrganizationalPermissionId] [int] IDENTITY(1,1) NOT NULL,
        [UserId] [int] NOT NULL,
        [PermissionType] [nvarchar](50) NOT NULL,
        [EntityId] [int] NOT NULL,
        [EntityName] [nvarchar](100) NOT NULL,
        [CanView] [bit] NOT NULL DEFAULT 1,
        [CanEdit] [bit] NOT NULL DEFAULT 0,
        [CanDelete] [bit] NOT NULL DEFAULT 0,
        [CanCreate] [bit] NOT NULL DEFAULT 0,
        [IsActive] [bit] NOT NULL DEFAULT 1,
        [CreatedAt] [datetime] NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] [datetime] NULL,
        CONSTRAINT [PK_UserOrganizationalPermissions] PRIMARY KEY CLUSTERED ([UserOrganizationalPermissionId] ASC),
        CONSTRAINT [FK_UserOrganizationalPermissions_Users] FOREIGN KEY([UserId]) REFERENCES [dbo].[users] ([userid])
    )
    PRINT 'UserOrganizationalPermissions table created successfully'
END
ELSE
    PRINT 'UserOrganizationalPermissions table already exists'
GO

-- Create UserOperationPermissions table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UserOperationPermissions]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[UserOperationPermissions](
        [UserOperationPermissionId] [int] IDENTITY(1,1) NOT NULL,
        [UserId] [int] NOT NULL,
        [PermissionId] [int] NOT NULL,
        [EntityType] [nvarchar](50) NOT NULL,
        [OperationType] [nvarchar](50) NOT NULL,
        [IsAllowed] [bit] NOT NULL DEFAULT 1,
        [Scope] [nvarchar](50) NOT NULL DEFAULT 'All',
        [ScopeId] [int] NULL,
        [IsActive] [bit] NOT NULL DEFAULT 1,
        [CreatedAt] [datetime] NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] [datetime] NULL,
        CONSTRAINT [PK_UserOperationPermissions] PRIMARY KEY CLUSTERED ([UserOperationPermissionId] ASC),
        CONSTRAINT [FK_UserOperationPermissions_Users] FOREIGN KEY([UserId]) REFERENCES [dbo].[users] ([userid]),
        CONSTRAINT [FK_UserOperationPermissions_Permissions] FOREIGN KEY([PermissionId]) REFERENCES [dbo].[Permissions] ([PermissionId])
    )
    PRINT 'UserOperationPermissions table created successfully'
END
ELSE
    PRINT 'UserOperationPermissions table already exists'
GO

-- Add new permissions to Permissions table
INSERT INTO [dbo].[Permissions] ([PermissionKey], [PermissionName], [Description], [Category], [IsActive])
SELECT 'DASHBOARD_VIEW', 'View Dashboard', 'Can view the main dashboard', 'Menu', 1
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[Permissions] WHERE [PermissionKey] = 'DASHBOARD_VIEW')

INSERT INTO [dbo].[Permissions] ([PermissionKey], [PermissionName], [Description], [Category], [IsActive])
SELECT 'EMPLOYEES_VIEW', 'View Employees', 'Can view employees section', 'Menu', 1
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[Permissions] WHERE [PermissionKey] = 'EMPLOYEES_VIEW')

INSERT INTO [dbo].[Permissions] ([PermissionKey], [PermissionName], [Description], [Category], [IsActive])
SELECT 'CONTROLLERS_VIEW', 'View Controllers', 'Can view controllers section', 'Menu', 1
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[Permissions] WHERE [PermissionKey] = 'CONTROLLERS_VIEW')

INSERT INTO [dbo].[Permissions] ([PermissionKey], [PermissionName], [Description], [Category], [IsActive])
SELECT 'LICENSES_VIEW', 'View Licenses', 'Can view licenses section', 'Menu', 1
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[Permissions] WHERE [PermissionKey] = 'LICENSES_VIEW')

INSERT INTO [dbo].[Permissions] ([PermissionKey], [PermissionName], [Description], [Category], [IsActive])
SELECT 'CERTIFICATES_VIEW', 'View Certificates', 'Can view certificates section', 'Menu', 1
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[Permissions] WHERE [PermissionKey] = 'CERTIFICATES_VIEW')

INSERT INTO [dbo].[Permissions] ([PermissionKey], [PermissionName], [Description], [Category], [IsActive])
SELECT 'OBSERVATIONS_VIEW', 'View Observations', 'Can view observations section', 'Menu', 1
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[Permissions] WHERE [PermissionKey] = 'OBSERVATIONS_VIEW')

INSERT INTO [dbo].[Permissions] ([PermissionKey], [PermissionName], [Description], [Category], [IsActive])
SELECT 'CONFIGURATION_VIEW', 'View Configuration', 'Can view configuration section', 'Menu', 1
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[Permissions] WHERE [PermissionKey] = 'CONFIGURATION_VIEW')

INSERT INTO [dbo].[Permissions] ([PermissionKey], [PermissionName], [Description], [Category], [IsActive])
SELECT 'PERMISSIONS_VIEW', 'View Permissions', 'Can view permissions section', 'Menu', 1
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[Permissions] WHERE [PermissionKey] = 'PERMISSIONS_VIEW')

-- Operation permissions
INSERT INTO [dbo].[Permissions] ([PermissionKey], [PermissionName], [Description], [Category], [IsActive])
SELECT 'EMPLOYEES_CREATE', 'Create Employee', 'Can create new employees', 'Operation', 1
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[Permissions] WHERE [PermissionKey] = 'EMPLOYEES_CREATE')

INSERT INTO [dbo].[Permissions] ([PermissionKey], [PermissionName], [Description], [Category], [IsActive])
SELECT 'EMPLOYEES_EDIT', 'Edit Employee', 'Can edit existing employees', 'Operation', 1
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[Permissions] WHERE [PermissionKey] = 'EMPLOYEES_EDIT')

INSERT INTO [dbo].[Permissions] ([PermissionKey], [PermissionName], [Description], [Category], [IsActive])
SELECT 'EMPLOYEES_DELETE', 'Delete Employee', 'Can delete employees', 'Operation', 1
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[Permissions] WHERE [PermissionKey] = 'EMPLOYEES_DELETE')

INSERT INTO [dbo].[Permissions] ([PermissionKey], [PermissionName], [Description], [Category], [IsActive])
SELECT 'CONTROLLERS_CREATE', 'Create Controller', 'Can create new controllers', 'Operation', 1
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[Permissions] WHERE [PermissionKey] = 'CONTROLLERS_CREATE')

INSERT INTO [dbo].[Permissions] ([PermissionKey], [PermissionName], [Description], [Category], [IsActive])
SELECT 'CONTROLLERS_EDIT', 'Edit Controller', 'Can edit existing controllers', 'Operation', 1
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[Permissions] WHERE [PermissionKey] = 'CONTROLLERS_EDIT')

INSERT INTO [dbo].[Permissions] ([PermissionKey], [PermissionName], [Description], [Category], [IsActive])
SELECT 'CONTROLLERS_DELETE', 'Delete Controller', 'Can delete controllers', 'Operation', 1
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[Permissions] WHERE [PermissionKey] = 'CONTROLLERS_DELETE')

INSERT INTO [dbo].[Permissions] ([PermissionKey], [PermissionName], [Description], [Category], [IsActive])
SELECT 'LICENSES_CREATE', 'Create License', 'Can create new licenses', 'Operation', 1
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[Permissions] WHERE [PermissionKey] = 'LICENSES_CREATE')

INSERT INTO [dbo].[Permissions] ([PermissionKey], [PermissionName], [Description], [Category], [IsActive])
SELECT 'LICENSES_EDIT', 'Edit License', 'Can edit existing licenses', 'Operation', 1
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[Permissions] WHERE [PermissionKey] = 'LICENSES_EDIT')

INSERT INTO [dbo].[Permissions] ([PermissionKey], [PermissionName], [Description], [Category], [IsActive])
SELECT 'LICENSES_DELETE', 'Delete License', 'Can delete licenses', 'Operation', 1
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[Permissions] WHERE [PermissionKey] = 'LICENSES_DELETE')

INSERT INTO [dbo].[Permissions] ([PermissionKey], [PermissionName], [Description], [Category], [IsActive])
SELECT 'CERTIFICATES_CREATE', 'Create Certificate', 'Can create new certificates', 'Operation', 1
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[Permissions] WHERE [PermissionKey] = 'CERTIFICATES_CREATE')

INSERT INTO [dbo].[Permissions] ([PermissionKey], [PermissionName], [Description], [Category], [IsActive])
SELECT 'CERTIFICATES_EDIT', 'Edit Certificate', 'Can edit existing certificates', 'Operation', 1
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[Permissions] WHERE [PermissionKey] = 'CERTIFICATES_EDIT')

INSERT INTO [dbo].[Permissions] ([PermissionKey], [PermissionName], [Description], [Category], [IsActive])
SELECT 'CERTIFICATES_DELETE', 'Delete Certificate', 'Can delete certificates', 'Operation', 1
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[Permissions] WHERE [PermissionKey] = 'CERTIFICATES_DELETE')

INSERT INTO [dbo].[Permissions] ([PermissionKey], [PermissionName], [Description], [Category], [IsActive])
SELECT 'OBSERVATIONS_CREATE', 'Create Observation', 'Can create new observations', 'Operation', 1
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[Permissions] WHERE [PermissionKey] = 'OBSERVATIONS_CREATE')

INSERT INTO [dbo].[Permissions] ([PermissionKey], [PermissionName], [Description], [Category], [IsActive])
SELECT 'OBSERVATIONS_EDIT', 'Edit Observation', 'Can edit existing observations', 'Operation', 1
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[Permissions] WHERE [PermissionKey] = 'OBSERVATIONS_EDIT')

INSERT INTO [dbo].[Permissions] ([PermissionKey], [PermissionName], [Description], [Category], [IsActive])
SELECT 'OBSERVATIONS_DELETE', 'Delete Observation', 'Can delete observations', 'Operation', 1
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[Permissions] WHERE [PermissionKey] = 'OBSERVATIONS_DELETE')

PRINT 'New permissions added successfully'
GO

-- Create stored procedure for checking menu permissions
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CanUserViewMenu]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[CanUserViewMenu]
GO

CREATE PROCEDURE [dbo].[CanUserViewMenu]
    @UserId INT,
    @MenuKey NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @CanView BIT = 0;
    
    -- Check if user has direct menu permission
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
    ELSE
    BEGIN
        -- Check if user has role-based permission
        DECLARE @UserRole NVARCHAR(50);
        SELECT @UserRole = rolename FROM users WHERE userid = @UserId;
        
        IF @UserRole = 'Admin'
        BEGIN
            SET @CanView = 1;
        END
        ELSE
        BEGIN
            -- Check role permissions
            IF EXISTS (
                SELECT 1 FROM RolePermissions rp
                INNER JOIN Permissions p ON rp.PermissionId = p.PermissionId
                INNER JOIN Roles r ON rp.RoleId = r.RoleId
                INNER JOIN users u ON u.rolename = r.RoleName
                WHERE u.userid = @UserId 
                AND p.PermissionKey = @MenuKey + '_VIEW'
                AND rp.IsActive = 1
            )
            BEGIN
                SET @CanView = 1;
            END
        END
    END
    
    SELECT @CanView AS CanView;
END
GO

-- Create stored procedure for checking operation permissions
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CanUserPerformOperation]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[CanUserPerformOperation]
GO

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
    
    -- Check if user has direct operation permission
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
    ELSE
    BEGIN
        -- Check if user has role-based permission
        DECLARE @UserRole NVARCHAR(50);
        SELECT @UserRole = rolename FROM users WHERE userid = @UserId;
        
        IF @UserRole = 'Admin'
        BEGIN
            SET @CanPerform = 1;
        END
        ELSE
        BEGIN
            -- Check role permissions
            IF EXISTS (
                SELECT 1 FROM RolePermissions rp
                INNER JOIN Permissions p ON rp.PermissionId = p.PermissionId
                INNER JOIN Roles r ON rp.RoleId = r.RoleId
                INNER JOIN users u ON u.rolename = r.RoleName
                WHERE u.userid = @UserId 
                AND p.PermissionKey = @EntityType + '_' + @OperationType
                AND rp.IsActive = 1
            )
            BEGIN
                SET @CanPerform = 1;
            END
        END
    END
    
    SELECT @CanPerform AS CanPerform;
END
GO

PRINT 'Advanced Permission System created successfully!'
PRINT 'New tables created: UserMenuPermissions, UserOrganizationalPermissions, UserOperationPermissions'
PRINT 'New permissions added for detailed operations and menu visibility'
PRINT 'Stored procedures created for permission checking'
PRINT 'No data insertion errors - tables are ready for use!'
GO