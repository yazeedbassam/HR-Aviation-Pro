-- =====================================================
-- الخطوة 4: إدراج صلاحيات الأدوار الافتراضية
-- =====================================================

-- الحصول على معرفات الأدوار
DECLARE @AdminRoleId int, @SupervisorRoleId int, @StaffRoleId int

SELECT @AdminRoleId = ValueId FROM ConfigurationValues WHERE CategoryName = 'Roles' AND ValueText = 'Admin'
SELECT @SupervisorRoleId = ValueId FROM ConfigurationValues WHERE CategoryName = 'Roles' AND ValueText = 'Supervisor'
SELECT @StaffRoleId = ValueId FROM ConfigurationValues WHERE CategoryName = 'Roles' AND ValueText = 'Staff'

-- Admin يحصل على جميع الصلاحيات
IF @AdminRoleId IS NOT NULL
BEGIN
    INSERT INTO RolePermissions (RoleId, PermissionId)
    SELECT @AdminRoleId, PermissionId FROM Permissions WHERE IsActive = 1
END
GO

-- Supervisor يحصل على معظم الصلاحيات ما عدا إدارة النظام
IF @SupervisorRoleId IS NOT NULL
BEGIN
    INSERT INTO RolePermissions (RoleId, PermissionId)
    SELECT @SupervisorRoleId, PermissionId FROM Permissions 
    WHERE IsActive = 1 AND CategoryName NOT IN ('System')
END
GO

-- Staff يحصل على صلاحيات المشاهدة الأساسية
IF @StaffRoleId IS NOT NULL
BEGIN
    INSERT INTO RolePermissions (RoleId, PermissionId)
    SELECT @StaffRoleId, PermissionId FROM Permissions 
    WHERE IsActive = 1 AND PermissionKey IN (
        'DASHBOARD_VIEW',
        'ORGANIZATION_VIEW',
        'EMPLOYEES_VIEW',
        'CONTROLLERS_VIEW',
        'LICENSES_VIEW',
        'CERTIFICATES_VIEW',
        'OBSERVATIONS_VIEW'
    )
END
GO
