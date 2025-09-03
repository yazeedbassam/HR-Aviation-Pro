-- =====================================================
-- FIX MISSING STORED PROCEDURES
-- =====================================================
-- This script creates missing stored procedures for the permission system

-- Create CanUserPerformOperation procedure if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CanUserPerformOperation]') AND type in (N'P', N'PC'))
BEGIN
    PRINT 'Creating CanUserPerformOperation procedure...'
    
    CREATE PROCEDURE [dbo].[CanUserPerformOperation]
        @UserId int,
        @EntityType nvarchar(50),
        @OperationType nvarchar(50),
        @Scope nvarchar(50) = 'All',
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
            AND (uop.Scope = @Scope OR (uop.Scope = 'All'))
            AND (@ScopeId IS NULL OR uop.ScopeId = @ScopeId OR uop.ScopeId IS NULL)
        
        -- If no specific permission found, check if user has admin role
        IF @IsAllowed IS NULL
        BEGIN
            SELECT @IsAllowed = CASE WHEN u.rolename = 'Admin' THEN 1 ELSE 0 END
            FROM users u
            WHERE u.userid = @UserId
        END
        
        SELECT @IsAllowed as IsAllowed
    END
    
    PRINT '✓ CanUserPerformOperation procedure created successfully'
END
ELSE
BEGIN
    PRINT '✓ CanUserPerformOperation procedure already exists'
END

-- Create GetUserOperationPermissions procedure if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetUserOperationPermissions]') AND type in (N'P', N'PC'))
BEGIN
    PRINT 'Creating GetUserOperationPermissions procedure...'
    
    CREATE PROCEDURE [dbo].[GetUserOperationPermissions]
        @UserId int
    AS
    BEGIN
        SET NOCOUNT ON;
        
        SELECT 
            uop.UserOperationPermissionId,
            uop.EntityType,
            uop.OperationType,
            uop.IsAllowed,
            uop.Scope,
            uop.ScopeId,
            p.PermissionName,
            p.PermissionDescription
        FROM UserOperationPermissions uop
        INNER JOIN Permissions p ON uop.PermissionId = p.PermissionId
        WHERE uop.UserId = @UserId 
            AND uop.IsActive = 1
            AND p.IsActive = 1
        ORDER BY uop.EntityType, uop.OperationType
    END
    
    PRINT '✓ GetUserOperationPermissions procedure created successfully'
END
ELSE
BEGIN
    PRINT '✓ GetUserOperationPermissions procedure already exists'
END

-- Create GetUserMenuPermissions procedure if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetUserMenuPermissions]') AND type in (N'P', N'PC'))
BEGIN
    PRINT 'Creating GetUserMenuPermissions procedure...'
    
    CREATE PROCEDURE [dbo].[GetUserMenuPermissions]
        @UserId int
    AS
    BEGIN
        SET NOCOUNT ON;
        
        SELECT 
            ump.UserMenuPermissionId,
            ump.MenuKey,
            ump.IsVisible,
            ump.IsActive,
            ump.CreatedAt,
            ump.UpdatedAt
        FROM UserMenuPermissions ump
        WHERE ump.UserId = @UserId 
            AND ump.IsActive = 1
        ORDER BY ump.MenuKey
    END
    
    PRINT '✓ GetUserMenuPermissions procedure created successfully'
END
ELSE
BEGIN
    PRINT '✓ GetUserMenuPermissions procedure already exists'
END

-- Create GetUserOrganizationalPermissions procedure if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetUserOrganizationalPermissions]') AND type in (N'P', N'PC'))
BEGIN
    PRINT 'Creating GetUserOrganizationalPermissions procedure...'
    
    CREATE PROCEDURE [dbo].[GetUserOrganizationalPermissions]
        @UserId int
    AS
    BEGIN
        SET NOCOUNT ON;
        
        SELECT 
            uop.UserOrganizationalPermissionId,
            uop.PermissionType,
            uop.EntityId,
            uop.EntityName,
            uop.CanView,
            uop.CanEdit,
            uop.CanDelete,
            uop.CanCreate,
            uop.IsActive,
            uop.CreatedAt,
            uop.UpdatedAt
        FROM UserOrganizationalPermissions uop
        WHERE uop.UserId = @UserId 
            AND uop.IsActive = 1
        ORDER BY uop.PermissionType, uop.EntityId
    END
    
    PRINT '✓ GetUserOrganizationalPermissions procedure created successfully'
END
ELSE
BEGIN
    PRINT '✓ GetUserOrganizationalPermissions procedure already exists'
END

PRINT '================================================'
PRINT 'Missing stored procedures fix completed successfully!'