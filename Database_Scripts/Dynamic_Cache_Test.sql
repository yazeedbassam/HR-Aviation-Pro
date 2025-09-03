-- =====================================================
-- اختبار الحل الديناميكي لمسح الكاش
-- =====================================================

USE [HR-Aviation]
GO

PRINT 'اختبار الحل الديناميكي لمسح الكاش...'

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
SET [IsAllowed] = 0, [UpdatedAt] = GETDATE()
WHERE [UserId] = @UserId 
    AND [EntityType] = 'EmployeeLicense' 
    AND [OperationType] = 'Add'

IF @@ROWCOUNT > 0
    PRINT '✓ تم إلغاء صلاحية الإضافة'
ELSE
    PRINT '✗ لم يتم العثور على صلاحية الإضافة'

-- إلغاء صلاحية التصدير
UPDATE [UserOperationPermissions] 
SET [IsAllowed] = 0, [UpdatedAt] = GETDATE()
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
    p.PermissionName,
    uop.UpdatedAt,
    CASE 
        WHEN uop.IsAllowed = 1 THEN 'مفعل'
        ELSE 'غير مفعل'
    END as Status
FROM [UserOperationPermissions] uop
INNER JOIN [Permissions] p ON uop.PermissionId = p.PermissionId
WHERE uop.UserId = @UserId 
    AND uop.EntityType = 'EmployeeLicense'
    AND uop.IsActive = 1
ORDER BY uop.OperationType

PRINT 'تم الانتهاء من إلغاء الصلاحيات!'
PRINT 'الآن يجب أن تختفي أزرار الإضافة والتصدير فوراً'
PRINT 'الحل الديناميكي سيمسح الكاش تلقائياً عند فحص الصلاحيات'