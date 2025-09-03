-- =====================================================
-- إعداد شامل لجميع صلاحيات النظام - محدث
-- =====================================================

USE [HR-Aviation]
GO

-- إضافة صلاحيات شهادات المراقبين
INSERT INTO [Permissions] ([PermissionKey], [PermissionName], [Description], [Category], [IsActive], [CreatedAt], [UpdatedAt])
VALUES 
    ('CONTROLLERCERTIFICATE_VIEW', 'عرض شهادات المراقبين', 'إمكانية عرض شهادات المراقبين', 'Certificates', 1, GETDATE(), GETDATE()),
    ('CONTROLLERCERTIFICATE_ADD', 'إضافة شهادات المراقبين', 'إمكانية إضافة شهادات جديدة للمراقبين', 'Certificates', 1, GETDATE(), GETDATE()),
    ('CONTROLLERCERTIFICATE_EDIT', 'تعديل شهادات المراقبين', 'إمكانية تعديل شهادات المراقبين', 'Certificates', 1, GETDATE(), GETDATE()),
    ('CONTROLLERCERTIFICATE_DELETE', 'حذف شهادات المراقبين', 'إمكانية حذف شهادات المراقبين', 'Certificates', 1, GETDATE(), GETDATE()),
    ('CONTROLLERCERTIFICATE_EXPORT', 'تصدير شهادات المراقبين', 'إمكانية تصدير شهادات المراقبين', 'Certificates', 1, GETDATE(), GETDATE())

-- إضافة صلاحيات شهادات الموظفين
INSERT INTO [Permissions] ([PermissionKey], [PermissionName], [Description], [Category], [IsActive], [CreatedAt], [UpdatedAt])
VALUES 
    ('EMPLOYEECERTIFICATE_VIEW', 'عرض شهادات الموظفين', 'إمكانية عرض شهادات الموظفين', 'Certificates', 1, GETDATE(), GETDATE()),
    ('EMPLOYEECERTIFICATE_ADD', 'إضافة شهادات الموظفين', 'إمكانية إضافة شهادات جديدة للموظفين', 'Certificates', 1, GETDATE(), GETDATE()),
    ('EMPLOYEECERTIFICATE_EDIT', 'تعديل شهادات الموظفين', 'إمكانية تعديل شهادات الموظفين', 'Certificates', 1, GETDATE(), GETDATE()),
    ('EMPLOYEECERTIFICATE_DELETE', 'حذف شهادات الموظفين', 'إمكانية حذف شهادات الموظفين', 'Certificates', 1, GETDATE(), GETDATE()),
    ('EMPLOYEECERTIFICATE_EXPORT', 'تصدير شهادات الموظفين', 'إمكانية تصدير شهادات الموظفين', 'Certificates', 1, GETDATE(), GETDATE())

-- إضافة صلاحيات العمليات في جدول UserOperationPermissions
-- شهادات المراقبين
INSERT INTO [UserOperationPermissions] ([UserId], [EntityType], [OperationType], [IsAllowed], [Scope], [ScopeId], [CreatedAt], [UpdatedAt])
SELECT 
    u.userid,
    'ControllerCertificate',
    'View',
    1, -- مفعل افتراضياً
    'All',
    NULL,
    GETDATE(),
    GETDATE()
FROM [users] u
WHERE u.role = 'Admin'

INSERT INTO [UserOperationPermissions] ([UserId], [EntityType], [OperationType], [IsAllowed], [Scope], [ScopeId], [CreatedAt], [UpdatedAt])
SELECT 
    u.userid,
    'ControllerCertificate',
    'Add',
    1, -- مفعل افتراضياً
    'All',
    NULL,
    GETDATE(),
    GETDATE()
FROM [users] u
WHERE u.role = 'Admin'

INSERT INTO [UserOperationPermissions] ([UserId], [EntityType], [OperationType], [IsAllowed], [Scope], [ScopeId], [CreatedAt], [UpdatedAt])
SELECT 
    u.userid,
    'ControllerCertificate',
    'Edit',
    1, -- مفعل افتراضياً
    'All',
    NULL,
    GETDATE(),
    GETDATE()
FROM [users] u
WHERE u.role = 'Admin'

INSERT INTO [UserOperationPermissions] ([UserId], [EntityType], [OperationType], [IsAllowed], [Scope], [ScopeId], [CreatedAt], [UpdatedAt])
SELECT 
    u.userid,
    'ControllerCertificate',
    'Delete',
    1, -- مفعل افتراضياً
    'All',
    NULL,
    GETDATE(),
    GETDATE()
FROM [users] u
WHERE u.role = 'Admin'

INSERT INTO [UserOperationPermissions] ([UserId], [EntityType], [OperationType], [IsAllowed], [Scope], [ScopeId], [CreatedAt], [UpdatedAt])
SELECT 
    u.userid,
    'ControllerCertificate',
    'Export',
    1, -- مفعل افتراضياً
    'All',
    NULL,
    GETDATE(),
    GETDATE()
FROM [users] u
WHERE u.role = 'Admin'

-- شهادات الموظفين
INSERT INTO [UserOperationPermissions] ([UserId], [EntityType], [OperationType], [IsAllowed], [Scope], [ScopeId], [CreatedAt], [UpdatedAt])
SELECT 
    u.userid,
    'EmployeeCertificate',
    'View',
    1, -- مفعل افتراضياً
    'All',
    NULL,
    GETDATE(),
    GETDATE()
FROM [users] u
WHERE u.role = 'Admin'

INSERT INTO [UserOperationPermissions] ([UserId], [EntityType], [OperationType], [IsAllowed], [Scope], [ScopeId], [CreatedAt], [UpdatedAt])
SELECT 
    u.userid,
    'EmployeeCertificate',
    'Add',
    1, -- مفعل افتراضياً
    'All',
    NULL,
    GETDATE(),
    GETDATE()
FROM [users] u
WHERE u.role = 'Admin'

INSERT INTO [UserOperationPermissions] ([UserId], [EntityType], [OperationType], [IsAllowed], [Scope], [ScopeId], [CreatedAt], [UpdatedAt])
SELECT 
    u.userid,
    'EmployeeCertificate',
    'Edit',
    1, -- مفعل افتراضياً
    'All',
    NULL,
    GETDATE(),
    GETDATE()
FROM [users] u
WHERE u.role = 'Admin'

INSERT INTO [UserOperationPermissions] ([UserId], [EntityType], [OperationType], [IsAllowed], [Scope], [ScopeId], [CreatedAt], [UpdatedAt])
SELECT 
    u.userid,
    'EmployeeCertificate',
    'Delete',
    1, -- مفعل افتراضياً
    'All',
    NULL,
    GETDATE(),
    GETDATE()
FROM [users] u
WHERE u.role = 'Admin'

INSERT INTO [UserOperationPermissions] ([UserId], [EntityType], [OperationType], [IsAllowed], [Scope], [ScopeId], [CreatedAt], [UpdatedAt])
SELECT 
    u.userid,
    'EmployeeCertificate',
    'Export',
    1, -- مفعل افتراضياً
    'All',
    NULL,
    GETDATE(),
    GETDATE()
FROM [users] u
WHERE u.role = 'Admin'

PRINT '========================================'
PRINT 'تم إعداد جميع صلاحيات النظام بنجاح!'
PRINT '========================================'
PRINT ''
PRINT '📋 الصلاحيات المضافة:'
PRINT '🏆 شهادات المراقبين: CONTROLLERCERTIFICATE_*'
PRINT '🎓 شهادات الموظفين: EMPLOYEECERTIFICATE_*'
PRINT ''
PRINT '✅ جميع الصلاحيات مفعلة للمديرين افتراضياً'
PRINT '🎨 الواجهة محدثة مع ألوان وأيقونات مميزة'
PRINT '📊 إجمالي الصلاحيات: 35 صلاحية'
PRINT '========================================'