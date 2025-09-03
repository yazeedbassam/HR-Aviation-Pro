-- =====================================================
-- إضافة صلاحية DEPARTMENT_OVERVIEW
-- =====================================================

-- إضافة الصلاحية الجديدة إلى جدول Permissions
IF NOT EXISTS (SELECT 1 FROM [dbo].[Permissions] WHERE [PermissionKey] = 'DEPARTMENT_OVERVIEW')
BEGIN
    INSERT INTO [dbo].[Permissions] ([PermissionKey], [PermissionName], [PermissionDescription], [CategoryName], [IsActive])
    VALUES ('DEPARTMENT_OVERVIEW', 'Department Overview', 'Can view department overview including employees and notifications', 'System Settings', 1)
    
    PRINT '✓ تم إضافة صلاحية DEPARTMENT_OVERVIEW'
END
ELSE
BEGIN
    PRINT '✓ صلاحية DEPARTMENT_OVERVIEW موجودة بالفعل'
END

-- إضافة الصلاحية للأدمن (إذا لم تكن موجودة)
DECLARE @AdminRoleId INT
DECLARE @PermissionId INT

SELECT @AdminRoleId = ValueId FROM ConfigurationValues 
WHERE ValueKey = 'Administrator' AND CategoryId = (SELECT CategoryId FROM ConfigurationCategories WHERE CategoryName = 'Roles')

SELECT @PermissionId = PermissionId FROM Permissions WHERE PermissionKey = 'DEPARTMENT_OVERVIEW'

IF @AdminRoleId IS NOT NULL AND @PermissionId IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM RolePermissions WHERE RoleId = @AdminRoleId AND PermissionId = @PermissionId)
    BEGIN
        INSERT INTO RolePermissions (RoleId, PermissionId, IsActive)
        VALUES (@AdminRoleId, @PermissionId, 1)
        PRINT '✓ تم إضافة صلاحية DEPARTMENT_OVERVIEW للأدمن'
    END
    ELSE
    BEGIN
        PRINT '✓ صلاحية DEPARTMENT_OVERVIEW موجودة بالفعل للأدمن'
    END
END

PRINT 'تم الانتهاء من إضافة صلاحية DEPARTMENT_OVERVIEW' 