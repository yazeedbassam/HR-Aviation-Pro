-- =====================================================
-- AVIATION HR PRO - Advanced Permission System
-- =====================================================
-- This script creates the new advanced permission system
-- that replaces the old simplified and advanced permission systems
-- =====================================================

USE [HR-Aviation]
GO

-- =====================================================
-- STEP 1: ADD NEW PERMISSIONS TO EXISTING TABLE
-- =====================================================

-- Add new detailed permissions for operations
INSERT INTO [Permissions] ([PermissionName], [PermissionKey], [PermissionDescription], [CategoryName]) VALUES

-- Employee Operations
('Add Employee', 'EMPLOYEES_ADD', 'Can add new employees', 'Staff'),
('Edit Employee', 'EMPLOYEES_EDIT', 'Can edit employee information', 'Staff'),
('Delete Employee', 'EMPLOYEES_DELETE', 'Can delete employees', 'Staff'),
('Export Employees', 'EMPLOYEES_EXPORT', 'Can export employee data', 'Staff'),

-- Controller Operations
('Add Controller', 'CONTROLLERS_ADD', 'Can add new controllers', 'Staff'),
('Edit Controller', 'CONTROLLERS_EDIT', 'Can edit controller information', 'Staff'),
('Delete Controller', 'CONTROLLERS_DELETE', 'Can delete controllers', 'Staff'),
('Export Controllers', 'CONTROLLERS_EXPORT', 'Can export controller data', 'Staff'),

-- License Operations
('Add License', 'LICENSES_ADD', 'Can add new licenses', 'Documents'),
('Edit License', 'LICENSES_EDIT', 'Can edit license information', 'Documents'),
('Delete License', 'LICENSES_DELETE', 'Can delete licenses', 'Documents'),
('Export Licenses', 'LICENSES_EXPORT', 'Can export license data', 'Documents'),

-- Certificate Operations
('Add Certificate', 'CERTIFICATES_ADD', 'Can add new certificates', 'Documents'),
('Edit Certificate', 'CERTIFICATES_EDIT', 'Can edit certificate information', 'Documents'),
('Delete Certificate', 'CERTIFICATES_DELETE', 'Can delete certificates', 'Documents'),
('Export Certificates', 'CERTIFICATES_EXPORT', 'Can export certificate data', 'Documents'),

-- Observation Operations
('Add Observation', 'OBSERVATIONS_ADD', 'Can add new observations', 'Activities'),
('Edit Observation', 'OBSERVATIONS_EDIT', 'Can edit observation information', 'Activities'),
('Delete Observation', 'OBSERVATIONS_DELETE', 'Can delete observations', 'Activities'),
('Export Observations', 'OBSERVATIONS_EXPORT', 'Can export observation data', 'Activities'),

-- Menu Visibility Permissions
('View Profile Menu', 'MENU_PROFILE_VIEW', 'Can view profile in sidebar menu', 'Menu'),
('View Notifications Menu', 'MENU_NOTIFICATIONS_VIEW', 'Can view notifications in sidebar menu', 'Menu'),
('View Dashboard Menu', 'MENU_DASHBOARD_VIEW', 'Can view dashboard in sidebar menu', 'Menu'),
('View Employees Menu', 'MENU_EMPLOYEES_VIEW', 'Can view employees in sidebar menu', 'Menu'),
('View Controllers Menu', 'MENU_CONTROLLERS_VIEW', 'Can view controllers in sidebar menu', 'Menu'),
('View Licenses Menu', 'MENU_LICENSES_VIEW', 'Can view licenses in sidebar menu', 'Menu'),
('View Certificates Menu', 'MENU_CERTIFICATES_VIEW', 'Can view certificates in sidebar menu', 'Menu'),
('View Observations Menu', 'MENU_OBSERVATIONS_VIEW', 'Can view observations in sidebar menu', 'Menu'),
('View Configuration Menu', 'MENU_CONFIGURATION_VIEW', 'Can view configuration in sidebar menu', 'Menu'),
('View Permissions Menu', 'MENU_PERMISSIONS_VIEW', 'Can view permissions in sidebar menu', 'Menu'),

-- Permission Management
('Manage Permissions', 'PERMISSIONS_MANAGE', 'Can manage user permissions', 'System'),
('View Permission Logs', 'PERMISSIONS_LOGS_VIEW', 'Can view permission logs', 'System')

GO

-- =====================================================
-- STEP 2: CREATE USER MENU PERMISSIONS TABLE
-- =====================================================

-- Create table for managing sidebar menu visibility
CREATE TABLE [dbo].[UserMenuPermissions](
    [UserMenuPermissionId] [int] IDENTITY(1,1) NOT NULL,
    [UserId] [int] NOT NULL,
    [MenuKey] [nvarchar](50) NOT NULL, -- مثل 'PROFILE', 'NOTIFICATIONS', 'DASHBOARD'
    [IsVisible] [bit] NOT NULL DEFAULT 1,
    [IsActive] [bit] NOT NULL DEFAULT 1,
    [CreatedAt] [datetime] NOT NULL DEFAULT GETDATE(),
    [UpdatedAt] [datetime] NULL,
    CONSTRAINT [PK_UserMenuPermissions] PRIMARY KEY CLUSTERED ([UserMenuPermissionId] ASC),
    CONSTRAINT [FK_UserMenuPermissions_Users] FOREIGN KEY([UserId]) REFERENCES [dbo].[users] ([userid])
)
GO

-- =====================================================
-- STEP 3: CREATE USER ORGANIZATIONAL PERMISSIONS TABLE
-- =====================================================

-- Create table for managing organizational structure permissions
CREATE TABLE [dbo].[UserOrganizationalPermissions](
    [UserOrganizationalPermissionId] [int] IDENTITY(1,1) NOT NULL,
    [UserId] [int] NOT NULL,
    [PermissionType] [nvarchar](50) NOT NULL, -- 'Country', 'Airport', 'Department'
    [EntityId] [int] NOT NULL, -- ID of Country, Airport, or Department
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
GO

-- =====================================================
-- STEP 4: CREATE USER OPERATION PERMISSIONS TABLE
-- =====================================================

-- Create table for managing detailed operation permissions
CREATE TABLE [dbo].[UserOperationPermissions](
    [UserOperationPermissionId] [int] IDENTITY(1,1) NOT NULL,
    [UserId] [int] NOT NULL,
    [PermissionId] [int] NOT NULL,
    [EntityType] [nvarchar](50) NOT NULL, -- 'Employee', 'Controller', 'License', etc.
    [OperationType] [nvarchar](50) NOT NULL, -- 'View', 'Add', 'Edit', 'Delete', 'Export'
    [IsAllowed] [bit] NOT NULL DEFAULT 1,
    [Scope] [nvarchar](50) NOT NULL DEFAULT 'All', -- 'All', 'Department', 'Own'
    [ScopeId] [int] NULL, -- Department ID or other scope identifier
    [IsActive] [bit] NOT NULL DEFAULT 1,
    [CreatedAt] [datetime] NOT NULL DEFAULT GETDATE(),
    [UpdatedAt] [datetime] NULL,
    CONSTRAINT [PK_UserOperationPermissions] PRIMARY KEY CLUSTERED ([UserOperationPermissionId] ASC),
    CONSTRAINT [FK_UserOperationPermissions_Users] FOREIGN KEY([UserId]) REFERENCES [dbo].[users] ([userid]),
    CONSTRAINT [FK_UserOperationPermissions_Permissions] FOREIGN KEY([PermissionId]) REFERENCES [dbo].[Permissions] ([PermissionId])
)
GO

-- =====================================================
-- STEP 5: CREATE INDEXES FOR PERFORMANCE
-- =====================================================

-- Indexes for UserMenuPermissions
CREATE NONCLUSTERED INDEX [IX_UserMenuPermissions_UserId] ON [dbo].[UserMenuPermissions] ([UserId])
CREATE NONCLUSTERED INDEX [IX_UserMenuPermissions_MenuKey] ON [dbo].[UserMenuPermissions] ([MenuKey])

-- Indexes for UserOrganizationalPermissions
CREATE NONCLUSTERED INDEX [IX_UserOrganizationalPermissions_UserId] ON [dbo].[UserOrganizationalPermissions] ([UserId])
CREATE NONCLUSTERED INDEX [IX_UserOrganizationalPermissions_EntityType] ON [dbo].[UserOrganizationalPermissions] ([PermissionType])

-- Indexes for UserOperationPermissions
CREATE NONCLUSTERED INDEX [IX_UserOperationPermissions_UserId] ON [dbo].[UserOperationPermissions] ([UserId])
CREATE NONCLUSTERED INDEX [IX_UserOperationPermissions_PermissionId] ON [dbo].[UserOperationPermissions] ([PermissionId])
CREATE NONCLUSTERED INDEX [IX_UserOperationPermissions_EntityType] ON [dbo].[UserOperationPermissions] ([EntityType])

GO

-- =====================================================
-- STEP 6: INSERT DEFAULT MENU PERMISSIONS FOR ADMIN
-- =====================================================

-- Give admin user access to all menu items
INSERT INTO [UserMenuPermissions] ([UserId], [MenuKey], [IsVisible]) VALUES
(1, 'PROFILE', 1),
(1, 'NOTIFICATIONS', 1),
(1, 'DASHBOARD', 1),
(1, 'EMPLOYEES', 1),
(1, 'CONTROLLERS', 1),
(1, 'LICENSES', 1),
(1, 'CERTIFICATES', 1),
(1, 'OBSERVATIONS', 1),
(1, 'CONFIGURATION', 1),
(1, 'PERMISSIONS', 1)

GO

-- =====================================================
-- STEP 7: INSERT DEFAULT OPERATION PERMISSIONS FOR ADMIN
-- =====================================================

-- Give admin user all operation permissions
INSERT INTO [UserOperationPermissions] ([UserId], [PermissionId], [EntityType], [OperationType], [IsAllowed], [Scope])
SELECT 
    1, -- Admin User ID
    p.PermissionId,
    CASE 
        WHEN p.PermissionKey LIKE 'EMPLOYEES_%' THEN 'Employee'
        WHEN p.PermissionKey LIKE 'CONTROLLERS_%' THEN 'Controller'
        WHEN p.PermissionKey LIKE 'LICENSES_%' THEN 'License'
        WHEN p.PermissionKey LIKE 'CERTIFICATES_%' THEN 'Certificate'
        WHEN p.PermissionKey LIKE 'OBSERVATIONS_%' THEN 'Observation'
        ELSE 'System'
    END,
    CASE 
        WHEN p.PermissionKey LIKE '%_VIEW' THEN 'View'
        WHEN p.PermissionKey LIKE '%_ADD' THEN 'Add'
        WHEN p.PermissionKey LIKE '%_EDIT' THEN 'Edit'
        WHEN p.PermissionKey LIKE '%_DELETE' THEN 'Delete'
        WHEN p.PermissionKey LIKE '%_EXPORT' THEN 'Export'
        ELSE 'Manage'
    END,
    1, -- IsAllowed
    'All' -- Scope
FROM [Permissions] p
WHERE p.PermissionKey IN (
    'EMPLOYEES_VIEW', 'EMPLOYEES_ADD', 'EMPLOYEES_EDIT', 'EMPLOYEES_DELETE', 'EMPLOYEES_EXPORT',
    'CONTROLLERS_VIEW', 'CONTROLLERS_ADD', 'CONTROLLERS_EDIT', 'CONTROLLERS_DELETE', 'CONTROLLERS_EXPORT',
    'LICENSES_VIEW', 'LICENSES_ADD', 'LICENSES_EDIT', 'LICENSES_DELETE', 'LICENSES_EXPORT',
    'CERTIFICATES_VIEW', 'CERTIFICATES_ADD', 'CERTIFICATES_EDIT', 'CERTIFICATES_DELETE', 'CERTIFICATES_EXPORT',
    'OBSERVATIONS_VIEW', 'OBSERVATIONS_ADD', 'OBSERVATIONS_EDIT', 'OBSERVATIONS_DELETE', 'OBSERVATIONS_EXPORT',
    'PERMISSIONS_MANAGE', 'PERMISSIONS_LOGS_VIEW'
)

GO

-- =====================================================
-- STEP 8: CREATE STORED PROCEDURES
-- =====================================================

-- Procedure to get user menu permissions
CREATE PROCEDURE [dbo].[GetUserMenuPermissions]
    @UserId int
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT MenuKey, IsVisible
    FROM UserMenuPermissions
    WHERE UserId = @UserId AND IsActive = 1
END
GO

-- Procedure to check if user can view menu item
CREATE PROCEDURE [dbo].[CanUserViewMenu]
    @UserId int,
    @MenuKey nvarchar(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @CanView bit = 0
    
    -- Check if user has specific menu permission
    SELECT @CanView = IsVisible
    FROM UserMenuPermissions
    WHERE UserId = @UserId AND MenuKey = @MenuKey AND IsActive = 1
    
    -- If no specific permission found, check if user has admin role
    IF @CanView IS NULL
    BEGIN
        SELECT @CanView = CASE WHEN u.rolename = 'Admin' THEN 1 ELSE 0 END
        FROM users u
        WHERE u.userid = @UserId
    END
    
    SELECT @CanView as CanView
END
GO

-- Procedure to check user operation permission
CREATE PROCEDURE [dbo].[CheckUserOperationPermission]
    @UserId int,
    @EntityType nvarchar(50),
    @OperationType nvarchar(50),
    @ScopeId int = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @IsAllowed bit = 0
    
    -- Check if user has specific operation permission
    SELECT @IsAllowed = uop.IsAllowed
    FROM UserOperationPermissions uop
    INNER JOIN Permissions p ON uop.PermissionId = p.PermissionId
    WHERE uop.UserId = @UserId 
        AND uop.EntityType = @EntityType 
        AND uop.OperationType = @OperationType
        AND uop.IsActive = 1
        AND p.IsActive = 1
        AND (uop.Scope = 'All' OR (uop.Scope = 'Department' AND uop.ScopeId = @ScopeId))
    
    -- If no specific permission found, check if user has admin role
    IF @IsAllowed IS NULL
    BEGIN
        SELECT @IsAllowed = CASE WHEN u.rolename = 'Admin' THEN 1 ELSE 0 END
        FROM users u
        WHERE u.userid = @UserId
    END
    
    SELECT @IsAllowed as IsAllowed
END
GO

PRINT 'Advanced Permission System created successfully!'
PRINT 'New tables created: UserMenuPermissions, UserOrganizationalPermissions, UserOperationPermissions'
PRINT 'New permissions added for detailed operations and menu visibility'
PRINT 'Stored procedures created for permission checking'
GO