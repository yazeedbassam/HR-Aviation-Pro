-- =====================================================
-- تغيير سريع للصلاحيات - لا حاجة لسكريبتات معقدة
-- =====================================================

USE [HR-Aviation]
GO

-- مثال: إلغاء صلاحية الإضافة للمستخدم yazeed.bassam
UPDATE [UserOperationPermissions] 
SET [IsAllowed] = 0, [UpdatedAt] = GETDATE()
WHERE [UserId] = (SELECT userid FROM users WHERE username = 'yazeed.bassam')
    AND [EntityType] = 'EmployeeLicense' 
    AND [OperationType] = 'Add'

-- مثال: منح صلاحية التعديل للمستخدم yazeed.bassam
UPDATE [UserOperationPermissions] 
SET [IsAllowed] = 1, [UpdatedAt] = GETDATE()
WHERE [UserId] = (SELECT userid FROM users WHERE username = 'yazeed.bassam')
    AND [EntityType] = 'EmployeeLicense' 
    AND [OperationType] = 'Edit'

PRINT 'تم تغيير الصلاحيات! التغييرات ستنعكس فوراً في الواجهة'