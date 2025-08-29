-- =====================================================
-- ADVANCED PERMISSION SYSTEM - FIXED VERSION
-- =====================================================
-- This script creates the advanced permission system with corrected column names
-- Run this script to set up the complete permission system

USE [HR-Aviation]
GO

-- =====================================================
-- STEP 1: CREATE STORED PROCEDURES
-- =====================================================

-- Create or update the CheckUserPermission stored procedure
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
    
    DECLARE @HasPermission BIT = 0;
    DECLARE @UserRole NVARCHAR(50);
    
    -- Get user role
    SELECT @UserRole = rolename FROM users WHERE userid = @UserId;
    
    -- Check if user is Admin (Admin has all permissions)
    IF @UserRole = 'Admin'
    BEGIN
        SET @HasPermission = 1;
    END
    ELSE
    BEGIN
        -- Check role-based permissions
        IF EXISTS (
            SELECT 1 
            FROM RolePermissions rp
            JOIN Permissions p ON rp.PermissionId = p.PermissionId
            JOIN users u ON u.rolename = (
                SELECT cv.ValueText 
                FROM ConfigurationValues cv 
                JOIN ConfigurationCategories cc ON cv.CategoryId = cc.CategoryId 
                WHERE cc.CategoryName = 'Roles' AND cv.ValueId = rp.RoleId
            )
            WHERE u.userid = @UserId 
              AND p.PermissionKey = @PermissionKey
              AND rp.IsActive = 1
              AND p.IsActive = 1
        )
        BEGIN
            SET @HasPermission = 1;
        END
        ELSE
        BEGIN
            -- Check department-based permissions
            IF @DepartmentId IS NOT NULL
            BEGIN
                IF EXISTS (
                    SELECT 1 
                    FROM UserDepartmentPermissions udp
                    JOIN Permissions p ON udp.PermissionId = p.PermissionId
                    WHERE udp.UserId = @UserId 
                      AND udp.DepartmentId = @DepartmentId
                      AND p.PermissionKey = @PermissionKey
                      AND udp.IsActive = 1
                      AND p.IsActive = 1
                      AND udp.CanView = 1
                )
                BEGIN
                    SET @HasPermission = 1;
                END
            END
        END
    END
    
    SELECT @HasPermission AS HasPermission;
END
GO

-- Create or update the GetUserPermissions stored procedure
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
        p.PermissionDescription,
        p.IsActive,
        p.CreatedAt,
        p.UpdatedAt,
        'Role' AS PermissionType,
        NULL AS DepartmentId,
        NULL AS DepartmentName,
        NULL AS CanView,
        NULL AS CanEdit,
        NULL AS CanDelete
    FROM RolePermissions rp
    JOIN Permissions p ON rp.PermissionId = p.PermissionId
    JOIN users u ON u.rolename = (
        SELECT cv.ValueText 
        FROM ConfigurationValues cv 
        JOIN ConfigurationCategories cc ON cv.CategoryId = cc.CategoryId 
        WHERE cc.CategoryName = 'Roles' AND cv.ValueId = rp.RoleId
    )
    WHERE u.userid = @UserId 
      AND rp.IsActive = 1
      AND p.IsActive = 1
    
    UNION ALL
    
    -- Get department-based permissions
    SELECT DISTINCT
        p.PermissionId,
        p.PermissionName,
        p.PermissionKey,
        p.PermissionDescription,
        p.IsActive,
        p.CreatedAt,
        p.UpdatedAt,
        'Department' AS PermissionType,
        udp.DepartmentId,
        dept.ValueText AS DepartmentName,
        udp.CanView,
        udp.CanEdit,
        udp.CanDelete
    FROM UserDepartmentPermissions udp
    JOIN Permissions p ON udp.PermissionId = p.PermissionId
    JOIN ConfigurationValues dept ON udp.DepartmentId = dept.ValueId
    JOIN ConfigurationCategories cc ON dept.CategoryId = cc.CategoryId
    WHERE udp.UserId = @UserId 
      AND udp.IsActive = 1
      AND p.IsActive = 1
      AND (cc.CategoryName = 'Divisions' OR cc.CategoryName = 'Departments')
    
    ORDER BY p.PermissionName;
END
GO

-- Create or update the GetUserDepartmentPermissions stored procedure
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetUserDepartmentPermissions]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[GetUserDepartmentPermissions]
GO

CREATE PROCEDURE [dbo].[GetUserDepartmentPermissions]
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT DISTINCT udp.DepartmentId
    FROM UserDepartmentPermissions udp
    WHERE udp.UserId = @UserId 
      AND udp.IsActive = 1
      AND udp.CanView = 1;
END
GO

-- =====================================================
-- STEP 2: CREATE VIEW
-- =====================================================

-- Create or update the vw_UserPermissionsSummary view
IF EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_UserPermissionsSummary]'))
    DROP VIEW [dbo].[vw_UserPermissionsSummary]
GO

CREATE VIEW [dbo].[vw_UserPermissionsSummary]
AS
SELECT 
    u.userid AS UserId,
    u.username AS UserName,
    u.rolename AS UserFullName,
    u.rolename AS UserRole,
    (SELECT COUNT(DISTINCT p.PermissionId) 
     FROM RolePermissions rp
     JOIN Permissions p ON rp.PermissionId = p.PermissionId
     JOIN users u2 ON u2.rolename = (
         SELECT cv.ValueText 
         FROM ConfigurationValues cv 
         JOIN ConfigurationCategories cc ON cv.CategoryId = cc.CategoryId 
         WHERE cc.CategoryName = 'Roles' AND cv.ValueId = rp.RoleId
     )
     WHERE u2.userid = u.userid 
       AND rp.IsActive = 1
       AND p.IsActive = 1) +
    (SELECT COUNT(DISTINCT p.PermissionId) 
     FROM UserDepartmentPermissions udp
     JOIN Permissions p ON udp.PermissionId = p.PermissionId
     WHERE udp.UserId = u.userid 
       AND udp.IsActive = 1
       AND p.IsActive = 1
       AND udp.CanView = 1) AS TotalPermissions,
    (SELECT COUNT(DISTINCT udp.DepartmentId) 
     FROM UserDepartmentPermissions udp
     WHERE udp.UserId = u.userid 
       AND udp.IsActive = 1
       AND udp.CanView = 1) AS AccessibleDepartments,
    (SELECT COUNT(DISTINCT p.PermissionId) 
     FROM RolePermissions rp
     JOIN Permissions p ON rp.PermissionId = p.PermissionId
     JOIN users u2 ON u2.rolename = (
         SELECT cv.ValueText 
         FROM ConfigurationValues cv 
         JOIN ConfigurationCategories cc ON cv.CategoryId = cc.CategoryId 
         WHERE cc.CategoryName = 'Roles' AND cv.ValueId = rp.RoleId
     )
     WHERE u2.userid = u.userid 
       AND rp.IsActive = 1
       AND p.IsActive = 1) +
    (SELECT COUNT(DISTINCT p.PermissionId) 
     FROM UserDepartmentPermissions udp
     JOIN Permissions p ON udp.PermissionId = p.PermissionId
     WHERE udp.UserId = u.userid 
       AND udp.IsActive = 1
       AND p.IsActive = 1
       AND udp.CanView = 1) AS ActivePermissions
FROM users u;
GO

-- =====================================================
-- STEP 3: INSERT DEFAULT PERMISSIONS
-- =====================================================

-- Insert default permissions if they don't exist
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE PermissionKey = 'DASHBOARD_VIEW')
BEGIN
    INSERT INTO Permissions (PermissionName, PermissionKey, PermissionDescription, CategoryName, IsActive, CreatedAt)
    VALUES 
    ('Dashboard View', 'DASHBOARD_VIEW', 'Can view dashboard', 'Dashboard', 1, GETDATE()),
    ('Organization View', 'ORGANIZATION_VIEW', 'Can view organization structure', 'Organization', 1, GETDATE()),
    ('Structure View', 'STRUCTURE_VIEW', 'Can view structure', 'Organization', 1, GETDATE()),
    ('Divisions View', 'DIVISIONS_VIEW', 'Can view divisions', 'Organization', 1, GETDATE()),
    ('Staff View', 'STAFF_VIEW', 'Can view staff', 'Staff', 1, GETDATE()),
    ('Controllers View', 'CONTROLLERS_VIEW', 'Can view controllers', 'Staff', 1, GETDATE()),
    ('AIS View', 'AIS_VIEW', 'Can view AIS staff', 'Staff', 1, GETDATE()),
    ('CNS View', 'CNS_VIEW', 'Can view CNS staff', 'Staff', 1, GETDATE()),
    ('AFTN View', 'AFTN_VIEW', 'Can view AFTN staff', 'Staff', 1, GETDATE()),
    ('Ops Staff View', 'OPS_STAFF_VIEW', 'Can view operations staff', 'Staff', 1, GETDATE()),
    ('Licenses View', 'LICENSES_VIEW', 'Can view licenses', 'Documents', 1, GETDATE()),
    ('Certificates View', 'CERTIFICATES_VIEW', 'Can view certificates', 'Documents', 1, GETDATE()),
    ('Observations View', 'OBSERVATIONS_VIEW', 'Can view observations', 'Activities', 1, GETDATE()),
    ('Courses View', 'COURSES_VIEW', 'Can view courses', 'Activities', 1, GETDATE()),
    ('System Settings View', 'SYSTEM_SETTINGS_VIEW', 'Can view system settings', 'System', 1, GETDATE()),
    ('Configuration Management', 'CONFIGURATION_MANAGEMENT', 'Can manage configuration', 'System', 1, GETDATE()),
    ('Roles Management', 'ROLES_MANAGEMENT', 'Can manage roles', 'System', 1, GETDATE()),
    ('Notifications View', 'NOTIFICATIONS_VIEW', 'Can view notifications', 'System', 1, GETDATE()),
    ('Users View All', 'USERS_VIEW_ALL', 'Can view all users data', 'System', 1, GETDATE()),
    ('Observations Create', 'OBSERVATIONS_CREATE', 'Can create observations', 'Activities', 1, GETDATE()),
    ('Observations Edit', 'OBSERVATIONS_EDIT', 'Can edit observations', 'Activities', 1, GETDATE()),
    ('Observations Delete', 'OBSERVATIONS_DELETE', 'Can delete observations', 'Activities', 1, GETDATE()),
    ('Observations Export', 'OBSERVATIONS_EXPORT', 'Can export observations', 'Activities', 1, GETDATE());
END

-- =====================================================
-- STEP 4: GRANT PERMISSIONS TO ROLES
-- =====================================================

-- Grant permissions to Admin role
DECLARE @AdminRoleId INT;
SELECT @AdminRoleId = cv.ValueId 
FROM ConfigurationValues cv 
JOIN ConfigurationCategories cc ON cv.CategoryId = cc.CategoryId 
WHERE cc.CategoryName = 'Roles' AND cv.ValueText = 'Admin';

IF @AdminRoleId IS NOT NULL
BEGIN
    -- Insert all permissions for Admin role
    INSERT INTO RolePermissions (RoleId, PermissionId, IsActive, CreatedAt)
    SELECT @AdminRoleId, PermissionId, 1, GETDATE()
    FROM Permissions
    WHERE IsActive = 1
    AND PermissionId NOT IN (
        SELECT PermissionId 
        FROM RolePermissions 
        WHERE RoleId = @AdminRoleId
    );
END

-- Grant permissions to Controller role
DECLARE @ControllerRoleId INT;
SELECT @ControllerRoleId = cv.ValueId 
FROM ConfigurationValues cv 
JOIN ConfigurationCategories cc ON cv.CategoryId = cc.CategoryId 
WHERE cc.CategoryName = 'Roles' AND cv.ValueText = 'Controller';

IF @ControllerRoleId IS NOT NULL
BEGIN
    -- Insert basic permissions for Controller role
    INSERT INTO RolePermissions (RoleId, PermissionId, IsActive, CreatedAt)
    SELECT @ControllerRoleId, PermissionId, 1, GETDATE()
    FROM Permissions
    WHERE IsActive = 1
    AND PermissionKey IN (
        'DASHBOARD_VIEW',
        'NOTIFICATIONS_VIEW',
        'OBSERVATIONS_VIEW',
        'OBSERVATIONS_CREATE',
        'OBSERVATIONS_EDIT'
    )
    AND PermissionId NOT IN (
        SELECT PermissionId 
        FROM RolePermissions 
        WHERE RoleId = @ControllerRoleId
    );
END

PRINT 'Advanced Permission System setup completed successfully!';
GO 
