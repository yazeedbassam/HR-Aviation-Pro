-- =====================================================
-- إضافة صلاحيات الشهادات الجديدة - الحل النهائي
-- =====================================================

USE [HR-Aviation]
GO

-- التحقق من وجود الصلاحيات الجديدة وإضافتها إذا لم تكن موجودة
IF NOT EXISTS (SELECT 1 FROM [Permissions] WHERE PermissionKey = 'CONTROLLERCERTIFICATE_VIEW')
BEGIN
    INSERT INTO [Permissions] ([PermissionKey], [PermissionName], [PermissionDescription], [CategoryName], [IsActive], [CreatedAt])
    VALUES ('CONTROLLERCERTIFICATE_VIEW', 'عرض شهادات المراقبين', 'إمكانية عرض شهادات المراقبين', 'Certificates', 1, GETDATE())
    PRINT 'تم إضافة صلاحية: CONTROLLERCERTIFICATE_VIEW'
END

IF NOT EXISTS (SELECT 1 FROM [Permissions] WHERE PermissionKey = 'CONTROLLERCERTIFICATE_ADD')
BEGIN
    INSERT INTO [Permissions] ([PermissionKey], [PermissionName], [PermissionDescription], [CategoryName], [IsActive], [CreatedAt])
    VALUES ('CONTROLLERCERTIFICATE_ADD', 'إضافة شهادات المراقبين', 'إمكانية إضافة شهادات جديدة للمراقبين', 'Certificates', 1, GETDATE())
    PRINT 'تم إضافة صلاحية: CONTROLLERCERTIFICATE_ADD'
END

IF NOT EXISTS (SELECT 1 FROM [Permissions] WHERE PermissionKey = 'CONTROLLERCERTIFICATE_EDIT')
BEGIN
    INSERT INTO [Permissions] ([PermissionKey], [PermissionName], [PermissionDescription], [CategoryName], [IsActive], [CreatedAt])
    VALUES ('CONTROLLERCERTIFICATE_EDIT', 'تعديل شهادات المراقبين', 'إمكانية تعديل شهادات المراقبين', 'Certificates', 1, GETDATE())
    PRINT 'تم إضافة صلاحية: CONTROLLERCERTIFICATE_EDIT'
END

IF NOT EXISTS (SELECT 1 FROM [Permissions] WHERE PermissionKey = 'CONTROLLERCERTIFICATE_DELETE')
BEGIN
    INSERT INTO [Permissions] ([PermissionKey], [PermissionName], [PermissionDescription], [CategoryName], [IsActive], [CreatedAt])
    VALUES ('CONTROLLERCERTIFICATE_DELETE', 'حذف شهادات المراقبين', 'إمكانية حذف شهادات المراقبين', 'Certificates', 1, GETDATE())
    PRINT 'تم إضافة صلاحية: CONTROLLERCERTIFICATE_DELETE'
END

IF NOT EXISTS (SELECT 1 FROM [Permissions] WHERE PermissionKey = 'CONTROLLERCERTIFICATE_EXPORT')
BEGIN
    INSERT INTO [Permissions] ([PermissionKey], [PermissionName], [PermissionDescription], [CategoryName], [IsActive], [CreatedAt])
    VALUES ('CONTROLLERCERTIFICATE_EXPORT', 'تصدير شهادات المراقبين', 'إمكانية تصدير شهادات المراقبين', 'Certificates', 1, GETDATE())
    PRINT 'تم إضافة صلاحية: CONTROLLERCERTIFICATE_EXPORT'
END

IF NOT EXISTS (SELECT 1 FROM [Permissions] WHERE PermissionKey = 'EMPLOYEECERTIFICATE_VIEW')
BEGIN
    INSERT INTO [Permissions] ([PermissionKey], [PermissionName], [PermissionDescription], [CategoryName], [IsActive], [CreatedAt])
    VALUES ('EMPLOYEECERTIFICATE_VIEW', 'عرض شهادات الموظفين', 'إمكانية عرض شهادات الموظفين', 'Certificates', 1, GETDATE())
    PRINT 'تم إضافة صلاحية: EMPLOYEECERTIFICATE_VIEW'
END

IF NOT EXISTS (SELECT 1 FROM [Permissions] WHERE PermissionKey = 'EMPLOYEECERTIFICATE_ADD')
BEGIN
    INSERT INTO [Permissions] ([PermissionKey], [PermissionName], [PermissionDescription], [CategoryName], [IsActive], [CreatedAt])
    VALUES ('EMPLOYEECERTIFICATE_ADD', 'إضافة شهادات الموظفين', 'إمكانية إضافة شهادات جديدة للموظفين', 'Certificates', 1, GETDATE())
    PRINT 'تم إضافة صلاحية: EMPLOYEECERTIFICATE_ADD'
END

IF NOT EXISTS (SELECT 1 FROM [Permissions] WHERE PermissionKey = 'EMPLOYEECERTIFICATE_EDIT')
BEGIN
    INSERT INTO [Permissions] ([PermissionKey], [PermissionName], [PermissionDescription], [CategoryName], [IsActive], [CreatedAt])
    VALUES ('EMPLOYEECERTIFICATE_EDIT', 'تعديل شهادات الموظفين', 'إمكانية تعديل شهادات الموظفين', 'Certificates', 1, GETDATE())
    PRINT 'تم إضافة صلاحية: EMPLOYEECERTIFICATE_EDIT'
END

IF NOT EXISTS (SELECT 1 FROM [Permissions] WHERE PermissionKey = 'EMPLOYEECERTIFICATE_DELETE')
BEGIN
    INSERT INTO [Permissions] ([PermissionKey], [PermissionName], [PermissionDescription], [CategoryName], [IsActive], [CreatedAt])
    VALUES ('EMPLOYEECERTIFICATE_DELETE', 'حذف شهادات الموظفين', 'إمكانية حذف شهادات الموظفين', 'Certificates', 1, GETDATE())
    PRINT 'تم إضافة صلاحية: EMPLOYEECERTIFICATE_DELETE'
END

IF NOT EXISTS (SELECT 1 FROM [Permissions] WHERE PermissionKey = 'EMPLOYEECERTIFICATE_EXPORT')
BEGIN
    INSERT INTO [Permissions] ([PermissionKey], [PermissionName], [PermissionDescription], [CategoryName], [IsActive], [CreatedAt])
    VALUES ('EMPLOYEECERTIFICATE_EXPORT', 'تصدير شهادات الموظفين', 'إمكانية تصدير شهادات الموظفين', 'Certificates', 1, GETDATE())
    PRINT 'تم إضافة صلاحية: EMPLOYEECERTIFICATE_EXPORT'
END

-- إضافة صلاحيات العمليات للمديرين مع PermissionId
-- شهادات المراقبين
INSERT INTO [UserOperationPermissions] ([UserId], [PermissionId], [EntityType], [OperationType], [IsAllowed], [Scope], [ScopeId], [CreatedAt])
SELECT 
    u.userid,
    p.PermissionId,
    'ControllerCertificate',
    'View',
    1, -- مفعل افتراضياً
    'All',
    NULL,
    GETDATE()
FROM [users] u
CROSS JOIN [Permissions] p
WHERE u.RoleName = 'Admin'
AND p.PermissionKey = 'CONTROLLERCERTIFICATE_VIEW'
AND NOT EXISTS (SELECT 1 FROM [UserOperationPermissions] 
                WHERE UserId = u.userid AND EntityType = 'ControllerCertificate' AND OperationType = 'View')

INSERT INTO [UserOperationPermissions] ([UserId], [PermissionId], [EntityType], [OperationType], [IsAllowed], [Scope], [ScopeId], [CreatedAt])
SELECT 
    u.userid,
    p.PermissionId,
    'ControllerCertificate',
    'Add',
    1, -- مفعل افتراضياً
    'All',
    NULL,
    GETDATE()
FROM [users] u
CROSS JOIN [Permissions] p
WHERE u.RoleName = 'Admin'
AND p.PermissionKey = 'CONTROLLERCERTIFICATE_ADD'
AND NOT EXISTS (SELECT 1 FROM [UserOperationPermissions] 
                WHERE UserId = u.userid AND EntityType = 'ControllerCertificate' AND OperationType = 'Add')

INSERT INTO [UserOperationPermissions] ([UserId], [PermissionId], [EntityType], [OperationType], [IsAllowed], [Scope], [ScopeId], [CreatedAt])
SELECT 
    u.userid,
    p.PermissionId,
    'ControllerCertificate',
    'Edit',
    1, -- مفعل افتراضياً
    'All',
    NULL,
    GETDATE()
FROM [users] u
CROSS JOIN [Permissions] p
WHERE u.RoleName = 'Admin'
AND p.PermissionKey = 'CONTROLLERCERTIFICATE_EDIT'
AND NOT EXISTS (SELECT 1 FROM [UserOperationPermissions] 
                WHERE UserId = u.userid AND EntityType = 'ControllerCertificate' AND OperationType = 'Edit')

INSERT INTO [UserOperationPermissions] ([UserId], [PermissionId], [EntityType], [OperationType], [IsAllowed], [Scope], [ScopeId], [CreatedAt])
SELECT 
    u.userid,
    p.PermissionId,
    'ControllerCertificate',
    'Delete',
    1, -- مفعل افتراضياً
    'All',
    NULL,
    GETDATE()
FROM [users] u
CROSS JOIN [Permissions] p
WHERE u.RoleName = 'Admin'
AND p.PermissionKey = 'CONTROLLERCERTIFICATE_DELETE'
AND NOT EXISTS (SELECT 1 FROM [UserOperationPermissions] 
                WHERE UserId = u.userid AND EntityType = 'ControllerCertificate' AND OperationType = 'Delete')

INSERT INTO [UserOperationPermissions] ([UserId], [PermissionId], [EntityType], [OperationType], [IsAllowed], [Scope], [ScopeId], [CreatedAt])
SELECT 
    u.userid,
    p.PermissionId,
    'ControllerCertificate',
    'Export',
    1, -- مفعل افتراضياً
    'All',
    NULL,
    GETDATE()
FROM [users] u
CROSS JOIN [Permissions] p
WHERE u.RoleName = 'Admin'
AND p.PermissionKey = 'CONTROLLERCERTIFICATE_EXPORT'
AND NOT EXISTS (SELECT 1 FROM [UserOperationPermissions] 
                WHERE UserId = u.userid AND EntityType = 'ControllerCertificate' AND OperationType = 'Export')

-- شهادات الموظفين
INSERT INTO [UserOperationPermissions] ([UserId], [PermissionId], [EntityType], [OperationType], [IsAllowed], [Scope], [ScopeId], [CreatedAt])
SELECT 
    u.userid,
    p.PermissionId,
    'EmployeeCertificate',
    'View',
    1, -- مفعل افتراضياً
    'All',
    NULL,
    GETDATE()
FROM [users] u
CROSS JOIN [Permissions] p
WHERE u.RoleName = 'Admin'
AND p.PermissionKey = 'EMPLOYEECERTIFICATE_VIEW'
AND NOT EXISTS (SELECT 1 FROM [UserOperationPermissions] 
                WHERE UserId = u.userid AND EntityType = 'EmployeeCertificate' AND OperationType = 'View')

INSERT INTO [UserOperationPermissions] ([UserId], [PermissionId], [EntityType], [OperationType], [IsAllowed], [Scope], [ScopeId], [CreatedAt])
SELECT 
    u.userid,
    p.PermissionId,
    'EmployeeCertificate',
    'Add',
    1, -- مفعل افتراضياً
    'All',
    NULL,
    GETDATE()
FROM [users] u
CROSS JOIN [Permissions] p
WHERE u.RoleName = 'Admin'
AND p.PermissionKey = 'EMPLOYEECERTIFICATE_ADD'
AND NOT EXISTS (SELECT 1 FROM [UserOperationPermissions] 
                WHERE UserId = u.userid AND EntityType = 'EmployeeCertificate' AND OperationType = 'Add')

INSERT INTO [UserOperationPermissions] ([UserId], [PermissionId], [EntityType], [OperationType], [IsAllowed], [Scope], [ScopeId], [CreatedAt])
SELECT 
    u.userid,
    p.PermissionId,
    'EmployeeCertificate',
    'Edit',
    1, -- مفعل افتراضياً
    'All',
    NULL,
    GETDATE()
FROM [users] u
CROSS JOIN [Permissions] p
WHERE u.RoleName = 'Admin'
AND p.PermissionKey = 'EMPLOYEECERTIFICATE_EDIT'
AND NOT EXISTS (SELECT 1 FROM [UserOperationPermissions] 
                WHERE UserId = u.userid AND EntityType = 'EmployeeCertificate' AND OperationType = 'Edit')

INSERT INTO [UserOperationPermissions] ([UserId], [PermissionId], [EntityType], [OperationType], [IsAllowed], [Scope], [ScopeId], [CreatedAt])
SELECT 
    u.userid,
    p.PermissionId,
    'EmployeeCertificate',
    'Delete',
    1, -- مفعل افتراضياً
    'All',
    NULL,
    GETDATE()
FROM [users] u
CROSS JOIN [Permissions] p
WHERE u.RoleName = 'Admin'
AND p.PermissionKey = 'EMPLOYEECERTIFICATE_DELETE'
AND NOT EXISTS (SELECT 1 FROM [UserOperationPermissions] 
                WHERE UserId = u.userid AND EntityType = 'EmployeeCertificate' AND OperationType = 'Delete')

INSERT INTO [UserOperationPermissions] ([UserId], [PermissionId], [EntityType], [OperationType], [IsAllowed], [Scope], [ScopeId], [CreatedAt])
SELECT 
    u.userid,
    p.PermissionId,
    'EmployeeCertificate',
    'Export',
    1, -- مفعل افتراضياً
    'All',
    NULL,
    GETDATE()
FROM [users] u
CROSS JOIN [Permissions] p
WHERE u.RoleName = 'Admin'
AND p.PermissionKey = 'EMPLOYEECERTIFICATE_EXPORT'
AND NOT EXISTS (SELECT 1 FROM [UserOperationPermissions] 
                WHERE UserId = u.userid AND EntityType = 'EmployeeCertificate' AND OperationType = 'Export')

PRINT '========================================'
PRINT 'تم إضافة جميع صلاحيات الشهادات بنجاح!'
PRINT '========================================'
PRINT ''
PRINT '📋 الصلاحيات المضافة:'
PRINT '🏆 شهادات المراقبين: CONTROLLERCERTIFICATE_*'
PRINT '🎓 شهادات الموظفين: EMPLOYEECERTIFICATE_*'
PRINT ''
PRINT '✅ جميع الصلاحيات مفعلة للمديرين افتراضياً'
PRINT '📊 إجمالي الصلاحيات الآن: 35 صلاحية'
PRINT '========================================'