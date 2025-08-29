-- Fix Permissions Table Structure
-- This script fixes the Permissions table structure and adds missing permissions

-- Check if Permissions table exists, if not create it
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Permissions' AND xtype='U')
BEGIN
    CREATE TABLE Permissions (
        PermissionId INT PRIMARY KEY,
        PermissionName NVARCHAR(255) NOT NULL,
        PermissionKey NVARCHAR(255) NOT NULL UNIQUE
    );
    PRINT 'Created Permissions table';
END
ELSE
BEGIN
    -- Add missing columns if they don't exist
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Permissions') AND name = 'PermissionName')
    BEGIN
        ALTER TABLE Permissions ADD PermissionName NVARCHAR(255) NOT NULL DEFAULT '';
        PRINT 'Added PermissionName column';
    END
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Permissions') AND name = 'PermissionKey')
    BEGIN
        ALTER TABLE Permissions ADD PermissionKey NVARCHAR(255) NOT NULL DEFAULT '';
        PRINT 'Added PermissionKey column';
    END
END

-- Check if UserDepartmentPermissions table exists, if not create it
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='UserDepartmentPermissions' AND xtype='U')
BEGIN
    CREATE TABLE UserDepartmentPermissions (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL,
        PermissionId INT NOT NULL,
        DepartmentId INT NOT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CONSTRAINT FK_UserDepartmentPermissions_Users FOREIGN KEY (UserId) REFERENCES users(userid),
        CONSTRAINT FK_UserDepartmentPermissions_Permissions FOREIGN KEY (PermissionId) REFERENCES Permissions(PermissionId)
    );
    PRINT 'Created UserDepartmentPermissions table';
END

-- Insert or update basic permissions
MERGE Permissions AS target
USING (VALUES 
    (1, N'عرض لوحة التحكم', 'DASHBOARD_VIEW'),
    (2, N'عرض المنظمة', 'ORGANIZATION_VIEW'),
    (3, N'عرض الهيكل', 'STRUCTURE_VIEW'),
    (4, N'عرض الأقسام', 'DIVISIONS_VIEW'),
    (5, N'عرض الموظفين', 'STAFF_VIEW'),
    (6, N'عرض المراقبين', 'CONTROLLERS_VIEW'),
    (7, N'عرض AIS', 'AIS_VIEW'),
    (8, N'عرض CNS', 'CNS_VIEW'),
    (9, N'عرض AFTN', 'AFTN_VIEW'),
    (10, N'عرض Ops Staff', 'OPS_STAFF_VIEW'),
    (11, N'عرض الرخص', 'LICENSES_VIEW'),
    (12, N'عرض الشهادات', 'CERTIFICATES_VIEW'),
    (13, N'عرض الملاحظات', 'OBSERVATIONS_VIEW'),
    (14, N'عرض الدورات', 'COURSES_VIEW'),
    (15, N'إدارة الصلاحيات', 'PERMISSIONS_MANAGE'),
    (16, N'إعدادات النظام', 'SYSTEM_SETTINGS_VIEW'),
    (17, N'إدارة التكوين', 'CONFIGURATION_MANAGEMENT'),
    (18, N'إدارة الأدوار', 'ROLES_MANAGEMENT'),
    (19, N'عرض جميع المستخدمين', 'USERS_VIEW_ALL'),
    (20, N'إضافة', 'ADD'),
    (21, N'تعديل', 'EDIT'),
    (22, N'حذف', 'DELETE'),
    (23, N'تصدير', 'EXPORT')
) AS source (PermissionId, PermissionName, PermissionKey)
ON target.PermissionId = source.PermissionId
WHEN MATCHED THEN
    UPDATE SET 
        PermissionName = source.PermissionName,
        PermissionKey = source.PermissionKey
WHEN NOT MATCHED THEN
    INSERT (PermissionId, PermissionName, PermissionKey)
    VALUES (source.PermissionId, source.PermissionName, source.PermissionKey);

PRINT 'Permissions table updated successfully';

-- Grant admin user all permissions
DECLARE @AdminUserId INT = (SELECT userid FROM users WHERE username = 'admin');

IF @AdminUserId IS NOT NULL
BEGIN
    -- Clear existing admin permissions
    DELETE FROM UserDepartmentPermissions WHERE UserId = @AdminUserId;
    
    -- Add all permissions to admin user
    INSERT INTO UserDepartmentPermissions (UserId, PermissionId, DepartmentId, IsActive)
    SELECT @AdminUserId, p.PermissionId, 1, 1
    FROM Permissions p;
    
    PRINT 'Granted all permissions to admin user';
END
ELSE
BEGIN
    PRINT 'Admin user not found in users table';
END

PRINT 'Database fix completed successfully'; 
