-- =====================================================
-- إصلاح شامل لجميع المشاكل
-- =====================================================

PRINT 'بدء الإصلاح الشامل...';

-- 1. إضافة عمود LastPermissionUpdate إلى جدول Users
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'LastPermissionUpdate')
BEGIN
    ALTER TABLE [dbo].[Users] ADD [LastPermissionUpdate] [datetime] NULL;
    PRINT '✓ تم إضافة عمود LastPermissionUpdate إلى جدول Users';
END
ELSE
BEGIN
    PRINT '✓ عمود LastPermissionUpdate موجود بالفعل في جدول Users';
END

-- 2. تحديث LastPermissionUpdate للبيانات الموجودة
UPDATE [dbo].[Users] 
SET [LastPermissionUpdate] = GETDATE() 
WHERE [LastPermissionUpdate] IS NULL;

PRINT '✓ تم تحديث LastPermissionUpdate لجميع المستخدمين الموجودين';

-- 3. إضافة صلاحية DEPARTMENT_OVERVIEW إذا لم تكن موجودة
IF NOT EXISTS (SELECT 1 FROM [dbo].[Permissions] WHERE [PermissionKey] = 'DEPARTMENT_OVERVIEW')
BEGIN
    INSERT INTO [dbo].[Permissions] ([PermissionKey], [PermissionName], [PermissionDescription], [CategoryName], [IsActive])
    VALUES ('DEPARTMENT_OVERVIEW', 'Department Overview', 'Can view department overview including employees and notifications', 'System Settings', 1)
    
    PRINT '✓ تم إضافة صلاحية DEPARTMENT_OVERVIEW';
END
ELSE
BEGIN
    PRINT '✓ صلاحية DEPARTMENT_OVERVIEW موجودة بالفعل';
END

-- 4. إضافة صلاحية DEPARTMENT_OVERVIEW للأدمن
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
        PRINT '✓ تم إضافة صلاحية DEPARTMENT_OVERVIEW للأدمن';
    END
    ELSE
    BEGIN
        PRINT '✓ صلاحية DEPARTMENT_OVERVIEW موجودة بالفعل للأدمن';
    END
END

-- 5. إضافة صلاحية DEPARTMENT_OVERVIEW للمستخدم yazeed.bassam
DECLARE @YazeedUserId INT
SELECT @YazeedUserId = UserId FROM Users WHERE Username = 'yazeed.bassam'

IF @YazeedUserId IS NOT NULL AND @PermissionId IS NOT NULL
BEGIN
    -- إضافة صلاحية عرض (View) للمستخدم
    IF NOT EXISTS (SELECT 1 FROM UserOperationPermissions 
                  WHERE UserId = @YazeedUserId AND EntityType = 'DepartmentOverview' AND OperationType = 'View')
    BEGIN
        INSERT INTO UserOperationPermissions (UserId, PermissionId, EntityType, OperationType, IsAllowed, Scope, IsActive, CreatedAt)
        VALUES (@YazeedUserId, @PermissionId, 'DepartmentOverview', 'View', 1, 'Department', 1, GETDATE())
        PRINT '✓ تم إضافة صلاحية DEPARTMENT_OVERVIEW للمستخدم yazeed.bassam';
    END
    ELSE
    BEGIN
        -- تحديث الصلاحية الموجودة
        UPDATE UserOperationPermissions 
        SET IsAllowed = 1, Scope = 'Department', UpdatedAt = GETDATE()
        WHERE UserId = @YazeedUserId AND EntityType = 'DepartmentOverview' AND OperationType = 'View'
        PRINT '✓ تم تحديث صلاحية DEPARTMENT_OVERVIEW للمستخدم yazeed.bassam';
    END
END

-- 6. إنشاء Trigger لتحديث LastPermissionUpdate تلقائياً
IF NOT EXISTS (SELECT * FROM sys.triggers WHERE name = 'TR_Users_UpdateLastPermissionUpdate')
BEGIN
    EXEC('
    CREATE TRIGGER TR_Users_UpdateLastPermissionUpdate
    ON Users
    AFTER UPDATE
    AS
    BEGIN
        IF UPDATE(LastPermissionUpdate)
            RETURN;
            
        UPDATE Users 
        SET LastPermissionUpdate = GETDATE() 
        FROM Users u
        INNER JOIN inserted i ON u.UserId = i.UserId;
    END');
    
    PRINT '✓ تم إنشاء Trigger لتحديث LastPermissionUpdate تلقائياً';
END
ELSE
BEGIN
    PRINT '✓ Trigger TR_Users_UpdateLastPermissionUpdate موجود بالفعل';
END

-- 7. مسح الكاش لجميع المستخدمين
UPDATE Users SET LastPermissionUpdate = GETDATE();

PRINT '✓ تم مسح الكاش لجميع المستخدمين';

PRINT 'تم الانتهاء من الإصلاح الشامل!';
PRINT 'الآن يمكنك اختبار النظام:';
PRINT '1. معلومات الملف الشخصي يجب أن تظهر';
PRINT '2. صلاحية "نظرة عامة على القسم" يجب أن تعمل';
PRINT '3. لا يجب أن تظهر أخطاء LastPermissionUpdate'; 