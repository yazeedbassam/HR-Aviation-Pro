-- =====================================================
-- إلغاء صلاحيات الإضافة والتصدير للمستخدم yazeed.bassam
-- =====================================================

USE [HR-Aviation]
GO

PRINT 'إلغاء صلاحيات الإضافة والتصدير للمستخدم yazeed.bassam...'

-- التحقق من وجود المستخدم
DECLARE @UserId int
SELECT @UserId = userid FROM users WHERE username = 'yazeed.bassam'

IF @UserId IS NULL
BEGIN
    PRINT 'خطأ: المستخدم yazeed.bassam غير موجود'
    RETURN
END

PRINT 'معرف المستخدم: ' + CAST(@UserId AS VARCHAR(10))

-- إلغاء صلاحية الإضافة
UPDATE [UserOperationPermissions] 
SET [IsAllowed] = 0
WHERE [UserId] = @UserId 
    AND [EntityType] = 'EmployeeLicense' 
    AND [OperationType] = 'Add'

IF @@ROWCOUNT > 0
    PRINT '✓ تم إلغاء صلاحية الإضافة'
ELSE
    PRINT '✗ لم يتم العثور على صلاحية الإضافة'

-- إلغاء صلاحية التصدير
UPDATE [UserOperationPermissions] 
SET [IsAllowed] = 0
WHERE [UserId] = @UserId 
    AND [EntityType] = 'EmployeeLicense' 
    AND [OperationType] = 'Export'

IF @@ROWCOUNT > 0
    PRINT '✓ تم إلغاء صلاحية التصدير'
ELSE
    PRINT '✗ لم يتم العثور على صلاحية التصدير'

-- عرض الصلاحيات الحالية
PRINT 'الصلاحيات الحالية للمستخدم yazeed.bassam:'

SELECT 
    uop.EntityType,
    uop.OperationType,
    uop.IsAllowed,
    p.PermissionName
FROM [UserOperationPermissions] uop
INNER JOIN [Permissions] p ON uop.PermissionId = p.PermissionId
WHERE uop.UserId = @UserId 
    AND uop.EntityType = 'EmployeeLicense'
    AND uop.IsActive = 1
ORDER BY uop.OperationType

PRINT 'تم الانتهاء من إلغاء الصلاحيات!'
PRINT 'يرجى إعادة تشغيل التطبيق لضمان تطبيق التغييرات'