-- =====================================================
-- إصلاح مشكلة حفظ الصلاحيات
-- =====================================================

PRINT 'بدء إصلاح مشكلة حفظ الصلاحيات...';

-- 1. إزالة جميع صلاحيات DepartmentOverview للمستخدم yazeed.bassam
DELETE FROM UserOperationPermissions 
WHERE UserId = 1057 
AND EntityType = 'DepartmentOverview';

PRINT '✓ تم إزالة جميع صلاحيات DepartmentOverview للمستخدم yazeed.bassam';

-- 2. إضافة صلاحية View فقط للمستخدم yazeed.bassam
DECLARE @PermissionId INT
SELECT @PermissionId = PermissionId FROM Permissions WHERE PermissionKey = 'DEPARTMENT_OVERVIEW'

IF @PermissionId IS NOT NULL
BEGIN
    INSERT INTO UserOperationPermissions (UserId, PermissionId, EntityType, OperationType, IsAllowed, Scope, IsActive, CreatedAt)
    VALUES (1057, @PermissionId, 'DepartmentOverview', 'View', 1, 'Department', 1, GETDATE())
    
    PRINT '✓ تم إضافة صلاحية DepartmentOverview.View للمستخدم yazeed.bassam';
END
ELSE
BEGIN
    PRINT '⚠️ لم يتم العثور على صلاحية DEPARTMENT_OVERVIEW';
END

-- 3. التحقق من النتيجة
SELECT 
    uop.EntityType, 
    uop.OperationType, 
    uop.IsAllowed, 
    uop.Scope,
    uop.IsActive
FROM UserOperationPermissions uop
WHERE uop.UserId = 1057 
AND uop.EntityType = 'DepartmentOverview';

PRINT 'تم الانتهاء من إصلاح مشكلة حفظ الصلاحيات!';
PRINT 'الآن يمكنك اختبار النظام:';
PRINT '1. صلاحية "نظرة عامة على القسم" يجب أن تعمل';
PRINT '2. المستخدم يجب أن يرى إشعارات قسمه';
PRINT '3. يجب أن يظهر قسم "Department Overview" في البروفايل'; 