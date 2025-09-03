-- Fix Permission System - Final Version
-- This script ensures all required tables and permissions exist

-- Add CategoryName column to existing Permissions table if it doesn't exist
IF EXISTS (SELECT * FROM sysobjects WHERE name='Permissions' AND xtype='U')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Permissions') AND name = 'CategoryName')
    BEGIN
        ALTER TABLE [dbo].[Permissions] ADD [CategoryName] [nvarchar](50) NOT NULL DEFAULT 'General';
        PRINT 'Added CategoryName column to existing Permissions table';
    END
    
    -- Update existing permissions with NULL CategoryName
    UPDATE [dbo].[Permissions] 
    SET [CategoryName] = 'General' 
    WHERE [CategoryName] IS NULL;
    
    PRINT 'Updated existing permissions with default CategoryName';
END

-- Create Permissions table if not exists
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
        [UpdatedAt] [datetime] NULL,
        CONSTRAINT [PK_Permissions] PRIMARY KEY CLUSTERED ([PermissionId] ASC),
        CONSTRAINT [UK_Permissions_PermissionKey] UNIQUE ([PermissionKey])
    );
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
        [IsAllowed] [bit] NOT NULL DEFAULT 0,
        [Scope] [nvarchar](50) NOT NULL DEFAULT 'All',
        [ScopeId] [int] NULL,
        [IsActive] [bit] NOT NULL DEFAULT 1,
        [CreatedAt] [datetime] NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] [datetime] NULL,
        CONSTRAINT [PK_UserOperationPermissions] PRIMARY KEY CLUSTERED ([UserOperationPermissionId] ASC),
        CONSTRAINT [UK_UserOperationPermissions_User_Permission_Entity_Operation] UNIQUE ([UserId], [PermissionId], [EntityType], [OperationType])
    );
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
END

-- Insert default permissions for all entity types and operations
DECLARE @EntityTypes TABLE (EntityType NVARCHAR(50))
INSERT INTO @EntityTypes VALUES ('Employee'), ('Controller'), ('License'), ('Certificate'), ('Observation')

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

-- Insert menu permissions
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
        ('PERMISSIONS', 'Permissions')
) AS MenuItems(MenuKey, MenuName)
WHERE NOT EXISTS (
    SELECT 1 FROM Permissions p 
    WHERE p.PermissionKey = 'MENU_' + MenuKey + '_VIEW'
)

-- Create stored procedure for getting user menu permissions
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'GetUserMenuPermissions')
    DROP PROCEDURE GetUserMenuPermissions
GO

CREATE PROCEDURE GetUserMenuPermissions
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        MenuKey,
        IsVisible
    FROM UserMenuPermissions
    WHERE UserId = @UserId AND IsActive = 1
END
GO

-- Create stored procedure for checking if user can perform operation
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'CanUserPerformOperation')
    DROP PROCEDURE CanUserPerformOperation
GO

CREATE PROCEDURE CanUserPerformOperation
    @UserId INT,
    @EntityType NVARCHAR(50),
    @OperationType NVARCHAR(50),
    @Scope NVARCHAR(50) = 'All',
    @ScopeId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @PermissionKey NVARCHAR(100) = UPPER(@EntityType) + '_' + UPPER(@OperationType)
    DECLARE @PermissionId INT
    
    -- Get permission ID
    SELECT @PermissionId = PermissionId 
    FROM Permissions 
    WHERE PermissionKey = @PermissionKey AND IsActive = 1
    
    IF @PermissionId IS NULL
    BEGIN
        SELECT 0 as CanPerform
        RETURN
    END
    
    -- Check if user has permission
    SELECT CASE 
        WHEN EXISTS (
            SELECT 1 FROM UserOperationPermissions 
            WHERE UserId = @UserId 
            AND PermissionId = @PermissionId 
            AND EntityType = @EntityType 
            AND OperationType = @OperationType
            AND IsAllowed = 1 
            AND IsActive = 1
            AND (@Scope = 'All' OR Scope = @Scope OR (@ScopeId IS NOT NULL AND ScopeId = @ScopeId))
        ) THEN 1 
        ELSE 0 
    END as CanPerform
END
GO

-- Create stored procedure for getting all users with permissions
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'GetAllUsersWithPermissions')
    DROP PROCEDURE GetAllUsersWithPermissions
GO

CREATE PROCEDURE GetAllUsersWithPermissions
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        u.userid,
        u.username,
        COALESCE(c.fullname, e.fullname, u.username) as fullname,
        u.rolename,
        ISNULL(menu_count.MenuPermissionsCount, 0) as MenuPermissionsCount,
        ISNULL(op_count.OperationPermissionsCount, 0) as OperationPermissionsCount,
        ISNULL(org_count.OrganizationalPermissionsCount, 0) as OrganizationalPermissionsCount
    FROM users u
    LEFT JOIN controllers c ON u.userid = c.userid
    LEFT JOIN employees e ON u.userid = e.userid
    LEFT JOIN (
        SELECT UserId, COUNT(*) as MenuPermissionsCount
        FROM UserMenuPermissions
        WHERE IsActive = 1 AND IsVisible = 1
        GROUP BY UserId
    ) menu_count ON u.userid = menu_count.UserId
    LEFT JOIN (
        SELECT UserId, COUNT(*) as OperationPermissionsCount
        FROM UserOperationPermissions
        WHERE IsActive = 1 AND IsAllowed = 1
        GROUP BY UserId
    ) op_count ON u.userid = op_count.UserId
    LEFT JOIN (
        SELECT UserId, COUNT(*) as OrganizationalPermissionsCount
        FROM UserOrganizationalPermissions
        WHERE IsActive = 1 AND (CanView = 1 OR CanEdit = 1 OR CanDelete = 1 OR CanCreate = 1)
        GROUP BY UserId
    ) org_count ON u.userid = org_count.UserId
    ORDER BY u.username
END
GO

PRINT 'Permission system setup completed successfully!'
PRINT 'All required tables, permissions, and stored procedures have been created.'