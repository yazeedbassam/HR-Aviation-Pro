-- =====================================================
-- إضافة صلاحيات الشهادات للمستخدم yazeed.bassam
-- =====================================================

USE [HR-Aviation]
GO

PRINT '========================================'
PRINT 'إضافة صلاحيات الشهادات للمستخدم yazeed.bassam'
PRINT '========================================'

-- الحصول على معرف المستخدم
DECLARE @UserId INT
SELECT @UserId = userid FROM [users] WHERE username = 'yazeed.bassam'

IF @UserId IS NULL
BEGIN
    PRINT '❌ المستخدم yazeed.bassam غير موجود!'
    RETURN
END

PRINT '👤 تم العثور على المستخدم: yazeed.bassam (ID: ' + CAST(@UserId AS VARCHAR(10)) + ')'

-- حذف الصلاحيات الموجودة أولاً
DELETE FROM [UserOperationPermissions] 
WHERE UserId = @UserId 
AND EntityType IN ('ControllerCertificate', 'EmployeeCertificate')

PRINT '🗑️ تم حذف الصلاحيات الموجودة'

-- إضافة صلاحيات شهادات المراقبين
INSERT INTO [UserOperationPermissions] ([UserId], [PermissionId], [EntityType], [OperationType], [IsAllowed], [Scope], [ScopeId], [CreatedAt])
SELECT 
    @UserId,
    p.PermissionId,
    'ControllerCertificate',
    'View',
    1, -- مفعل
    'All',
    NULL,
    GETDATE()
FROM [Permissions] p
WHERE p.PermissionKey = 'CONTROLLERCERTIFICATE_VIEW'

INSERT INTO [UserOperationPermissions] ([UserId], [PermissionId], [EntityType], [OperationType], [IsAllowed], [Scope], [ScopeId], [CreatedAt])
SELECT 
    @UserId,
    p.PermissionId,
    'ControllerCertificate',
    'Add',
    1, -- مفعل
    'All',
    NULL,
    GETDATE()
FROM [Permissions] p
WHERE p.PermissionKey = 'CONTROLLERCERTIFICATE_ADD'

INSERT INTO [UserOperationPermissions] ([UserId], [PermissionId], [EntityType], [OperationType], [IsAllowed], [Scope], [ScopeId], [CreatedAt])
SELECT 
    @UserId,
    p.PermissionId,
    'ControllerCertificate',
    'Edit',
    1, -- مفعل
    'All',
    NULL,
    GETDATE()
FROM [Permissions] p
WHERE p.PermissionKey = 'CONTROLLERCERTIFICATE_EDIT'

INSERT INTO [UserOperationPermissions] ([UserId], [PermissionId], [EntityType], [OperationType], [IsAllowed], [Scope], [ScopeId], [CreatedAt])
SELECT 
    @UserId,
    p.PermissionId,
    'ControllerCertificate',
    'Delete',
    1, -- مفعل
    'All',
    NULL,
    GETDATE()
FROM [Permissions] p
WHERE p.PermissionKey = 'CONTROLLERCERTIFICATE_DELETE'

INSERT INTO [UserOperationPermissions] ([UserId], [PermissionId], [EntityType], [OperationType], [IsAllowed], [Scope], [ScopeId], [CreatedAt])
SELECT 
    @UserId,
    p.PermissionId,
    'ControllerCertificate',
    'Export',
    1, -- مفعل
    'All',
    NULL,
    GETDATE()
FROM [Permissions] p
WHERE p.PermissionKey = 'CONTROLLERCERTIFICATE_EXPORT'

-- إضافة صلاحيات شهادات الموظفين
INSERT INTO [UserOperationPermissions] ([UserId], [PermissionId], [EntityType], [OperationType], [IsAllowed], [Scope], [ScopeId], [CreatedAt])
SELECT 
    @UserId,
    p.PermissionId,
    'EmployeeCertificate',
    'View',
    1, -- مفعل
    'All',
    NULL,
    GETDATE()
FROM [Permissions] p
WHERE p.PermissionKey = 'EMPLOYEECERTIFICATE_VIEW'

INSERT INTO [UserOperationPermissions] ([UserId], [PermissionId], [EntityType], [OperationType], [IsAllowed], [Scope], [ScopeId], [CreatedAt])
SELECT 
    @UserId,
    p.PermissionId,
    'EmployeeCertificate',
    'Add',
    1, -- مفعل
    'All',
    NULL,
    GETDATE()
FROM [Permissions] p
WHERE p.PermissionKey = 'EMPLOYEECERTIFICATE_ADD'

INSERT INTO [UserOperationPermissions] ([UserId], [PermissionId], [EntityType], [OperationType], [IsAllowed], [Scope], [ScopeId], [CreatedAt])
SELECT 
    @UserId,
    p.PermissionId,
    'EmployeeCertificate',
    'Edit',
    1, -- مفعل
    'All',
    NULL,
    GETDATE()
FROM [Permissions] p
WHERE p.PermissionKey = 'EMPLOYEECERTIFICATE_EDIT'

INSERT INTO [UserOperationPermissions] ([UserId], [PermissionId], [EntityType], [OperationType], [IsAllowed], [Scope], [ScopeId], [CreatedAt])
SELECT 
    @UserId,
    p.PermissionId,
    'EmployeeCertificate',
    'Delete',
    1, -- مفعل
    'All',
    NULL,
    GETDATE()
FROM [Permissions] p
WHERE p.PermissionKey = 'EMPLOYEECERTIFICATE_DELETE'

INSERT INTO [UserOperationPermissions] ([UserId], [PermissionId], [EntityType], [OperationType], [IsAllowed], [Scope], [ScopeId], [CreatedAt])
SELECT 
    @UserId,
    p.PermissionId,
    'EmployeeCertificate',
    'Export',
    1, -- مفعل
    'All',
    NULL,
    GETDATE()
FROM [Permissions] p
WHERE p.PermissionKey = 'EMPLOYEECERTIFICATE_EXPORT'

PRINT '✅ تم إضافة جميع صلاحيات الشهادات للمستخدم yazeed.bassam'

-- التحقق من النتيجة
PRINT ''
PRINT '📊 النتيجة النهائية:'
SELECT 
    uop.EntityType,
    uop.OperationType,
    uop.IsAllowed,
    CASE WHEN uop.IsAllowed = 1 THEN '✅ مفعل' ELSE '❌ غير مفعل' END as الحالة
FROM [UserOperationPermissions] uop
WHERE uop.UserId = @UserId
AND uop.EntityType IN ('ControllerCertificate', 'EmployeeCertificate')
ORDER BY uop.EntityType, uop.OperationType

PRINT ''
PRINT '========================================'
PRINT 'تم الانتهاء بنجاح!'
PRINT '========================================'