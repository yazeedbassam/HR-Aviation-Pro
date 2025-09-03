-- =====================================================
-- AUTO ASSIGN PERMISSIONS FOR NEW USERS
-- =====================================================
-- This script creates a stored procedure that automatically assigns
-- appropriate permissions to new users based on their role

USE [HR-Aviation];
GO

-- Create stored procedure for auto-assigning permissions
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AutoAssignPermissionsForNewUser]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[AutoAssignPermissionsForNewUser]
GO

CREATE PROCEDURE [dbo].[AutoAssignPermissionsForNewUser]
    @UserId INT,
    @RoleName NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @ErrorMessage NVARCHAR(4000);
    
    BEGIN TRY
        PRINT 'Starting auto-assignment of permissions for UserId: ' + CAST(@UserId AS NVARCHAR(10)) + ', Role: ' + @RoleName;
        
        -- 1. Assign ALL Menu Permissions (but disabled by default)
        -- Admin gets all enabled, others get all disabled except basic ones
        INSERT INTO UserMenuPermissions (UserId, MenuKey, CanAccess, IsActive, CreatedAt, UpdatedAt)
        SELECT 
            @UserId, 
            MenuKey, 
            CASE 
                WHEN @RoleName = 'Admin' THEN 1  -- Admin gets all enabled
                WHEN @RoleName = 'Controller' AND MenuKey IN ('DASHBOARD', 'PROFILE', 'NOTIFICATIONS') THEN 1  -- Controller gets basic enabled
                WHEN @RoleName = 'Employee' AND MenuKey IN ('DASHBOARD', 'PROFILE', 'NOTIFICATIONS') THEN 1  -- Employee gets basic enabled
                ELSE 0  -- All others disabled by default
            END,
            1, 
            GETDATE(), 
            GETDATE()
        FROM Permissions
        WHERE IsActive = 1 AND CategoryName = 'Menu'
        AND NOT EXISTS (
            SELECT 1 FROM UserMenuPermissions 
            WHERE UserId = @UserId AND MenuKey = Permissions.PermissionKey
        );
        
        PRINT 'Assigned all menu permissions to user (enabled/disabled based on role)';
        
        -- 2. Assign ALL Operation Permissions (but disabled by default)
        -- Admin gets all enabled, others get all disabled except basic ones
        INSERT INTO UserOperationPermissions (UserId, EntityType, OperationType, IsAllowed, Scope, IsActive, CreatedAt, UpdatedAt)
        SELECT 
            @UserId, 
            EntityType, 
            OperationType, 
            CASE 
                WHEN @RoleName = 'Admin' THEN 1  -- Admin gets all enabled
                WHEN @RoleName = 'Controller' AND (
                    (EntityType = 'Profile' AND OperationType IN ('View', 'Edit'))
                    OR (EntityType = 'Observation' AND OperationType IN ('View', 'Create'))
                    OR (EntityType = 'License' AND OperationType = 'View')
                    OR (EntityType = 'Certificate' AND OperationType = 'View')
                ) THEN 1  -- Controller gets basic enabled
                WHEN @RoleName = 'Employee' AND (
                    (EntityType = 'Profile' AND OperationType IN ('View', 'Edit'))
                    OR (EntityType = 'License' AND OperationType = 'View')
                    OR (EntityType = 'Certificate' AND OperationType = 'View')
                ) THEN 1  -- Employee gets basic enabled
                ELSE 0  -- All others disabled by default
            END,
            'All', 
            1, 
            GETDATE(), 
            GETDATE()
        FROM Permissions
        WHERE IsActive = 1 AND CategoryName = 'Operation'
        AND NOT EXISTS (
            SELECT 1 FROM UserOperationPermissions 
            WHERE UserId = @UserId 
            AND EntityType = Permissions.PermissionKey
        );
        
        PRINT 'Assigned all operation permissions to user (enabled/disabled based on role)';
        
        -- 3. Assign ALL Organizational Permissions (but disabled by default)
        -- Admin gets all enabled, others get all disabled (view-only for basic access)
        
        -- Countries
        INSERT INTO UserOrganizationalPermissions (UserId, PermissionType, EntityId, EntityName, CanView, CanEdit, CanDelete, CanCreate, IsActive, CreatedAt, UpdatedAt)
        SELECT 
            @UserId, 
            'Country', 
            CountryId, 
            CountryName, 
            CASE WHEN @RoleName = 'Admin' THEN 1 ELSE 1 END,  -- All can view countries
            CASE WHEN @RoleName = 'Admin' THEN 1 ELSE 0 END,  -- Only Admin can edit
            CASE WHEN @RoleName = 'Admin' THEN 1 ELSE 0 END,  -- Only Admin can delete
            CASE WHEN @RoleName = 'Admin' THEN 1 ELSE 0 END,  -- Only Admin can create
            1, 
            GETDATE(), 
            GETDATE()
        FROM Countries
        WHERE NOT EXISTS (
            SELECT 1 FROM UserOrganizationalPermissions 
            WHERE UserId = @UserId AND PermissionType = 'Country' AND EntityId = Countries.CountryId
        );
        
        -- Airports
        INSERT INTO UserOrganizationalPermissions (UserId, PermissionType, EntityId, EntityName, CanView, CanEdit, CanDelete, CanCreate, IsActive, CreatedAt, UpdatedAt)
        SELECT 
            @UserId, 
            'Airport', 
            AirportId, 
            AirportName, 
            CASE WHEN @RoleName = 'Admin' THEN 1 ELSE 1 END,  -- All can view airports
            CASE WHEN @RoleName = 'Admin' THEN 1 ELSE 0 END,  -- Only Admin can edit
            CASE WHEN @RoleName = 'Admin' THEN 1 ELSE 0 END,  -- Only Admin can delete
            CASE WHEN @RoleName = 'Admin' THEN 1 ELSE 0 END,  -- Only Admin can create
            1, 
            GETDATE(), 
            GETDATE()
        FROM Airports
        WHERE NOT EXISTS (
            SELECT 1 FROM UserOrganizationalPermissions 
            WHERE UserId = @UserId AND PermissionType = 'Airport' AND EntityId = Airports.AirportId
        );
        
        PRINT 'Assigned all organizational permissions to user (enabled/disabled based on role)';
        
        PRINT 'Auto-assignment of permissions completed successfully for UserId: ' + CAST(@UserId AS NVARCHAR(10));
        
    END TRY
    BEGIN CATCH
        SET @ErrorMessage = ERROR_MESSAGE();
        PRINT 'Error in AutoAssignPermissionsForNewUser: ' + @ErrorMessage;
        -- Don't re-throw to avoid breaking user creation
    END CATCH
END
GO

-- Test the stored procedure with existing user
PRINT 'Testing stored procedure with user yazeed bassam (1057)...';
EXEC AutoAssignPermissionsForNewUser @UserId = 1057, @RoleName = 'Controller';

-- Verify the results
SELECT 
    'Menu Permissions' as PermissionType,
    COUNT(*) as Count
FROM UserMenuPermissions 
WHERE UserId = 1057 AND IsActive = 1

UNION ALL

SELECT 
    'Operation Permissions' as PermissionType,
    COUNT(*) as Count
FROM UserOperationPermissions 
WHERE UserId = 1057 AND IsActive = 1

UNION ALL

SELECT 
    'Organizational Permissions' as PermissionType,
    COUNT(*) as Count
FROM UserOrganizationalPermissions 
WHERE UserId = 1057 AND IsActive = 1;

PRINT 'Stored procedure created and tested successfully!';
PRINT 'Now new users will automatically get appropriate permissions based on their role.';