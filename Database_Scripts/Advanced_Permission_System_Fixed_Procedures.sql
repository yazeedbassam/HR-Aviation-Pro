-- Fix the Stored Procedures with correct column names
USE [HR-Aviation]
GO

-- Drop existing procedures if they exist
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CanUserViewMenu]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[CanUserViewMenu]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CanUserPerformOperation]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[CanUserPerformOperation]
GO

-- Create stored procedure for checking menu permissions (Fixed)
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
            -- Check role permissions (Fixed column names)
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

-- Create stored procedure for checking operation permissions (Fixed)
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
            -- Check role permissions (Fixed column names)
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

PRINT 'Stored Procedures fixed successfully!'
PRINT 'CanUserViewMenu and CanUserPerformOperation procedures created with correct column names'
GO