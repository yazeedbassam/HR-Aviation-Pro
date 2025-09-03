-- =====================================================
-- إصلاح صلاحيات رخص الموظفين للمستخدم yazeed.bassam
-- =====================================================
-- هذا السكريبت يضمن أن المستخدم yazeed.bassam لديه الصلاحيات الصحيحة
-- لعرض وتعديل وحذف رخص الموظفين
-- =====================================================

USE [HR-Aviation]
GO

PRINT 'بدء إصلاح صلاحيات رخص الموظفين...'

-- التحقق من وجود المستخدم yazeed.bassam
DECLARE @UserId int
SELECT @UserId = userid FROM users WHERE username = 'yazeed.bassam'

IF @UserId IS NULL
BEGIN
    PRINT 'خطأ: المستخدم yazeed.bassam غير موجود في قاعدة البيانات'
    RETURN
END

PRINT 'تم العثور على المستخدم yazeed.bassam مع ID: ' + CAST(@UserId AS VARCHAR(10))

-- التحقق من وجود جدول UserOperationPermissions
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UserOperationPermissions]') AND type in (N'U'))
BEGIN
    PRINT 'إنشاء جدول UserOperationPermissions...'
    
    CREATE TABLE [dbo].[UserOperationPermissions](
        [UserOperationPermissionId] [int] IDENTITY(1,1) NOT NULL,
        [UserId] [int] NOT NULL,
        [PermissionId] [int] NOT NULL,
        [EntityType] [nvarchar](50) NOT NULL,
        [OperationType] [nvarchar](50) NOT NULL,
        [IsAllowed] [bit] NOT NULL DEFAULT 1,
        [Scope] [nvarchar](50) NOT NULL DEFAULT 'All',
        [ScopeId] [int] NULL,
        [IsActive] [bit] NOT NULL DEFAULT 1,
        [CreatedAt] [datetime] NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] [datetime] NULL,
        CONSTRAINT [PK_UserOperationPermissions] PRIMARY KEY CLUSTERED ([UserOperationPermissionId] ASC),
        CONSTRAINT [FK_UserOperationPermissions_Users] FOREIGN KEY([UserId]) REFERENCES [dbo].[users] ([userid]),
        CONSTRAINT [FK_UserOperationPermissions_Permissions] FOREIGN KEY([PermissionId]) REFERENCES [dbo].[Permissions] ([PermissionId])
    )
    
    PRINT '✓ تم إنشاء جدول UserOperationPermissions'
END

-- إدراج صلاحيات رخص الموظفين إذا لم تكن موجودة
PRINT 'إدراج صلاحيات رخص الموظفين...'

-- صلاحية عرض رخص الموظفين
IF NOT EXISTS (SELECT 1 FROM [Permissions] WHERE [PermissionKey] = 'EMPLOYEELICENSE_VIEW')
BEGIN
    INSERT INTO [Permissions] ([PermissionName], [PermissionKey], [PermissionDescription], [CategoryName], [IsActive])
    VALUES ('View Employee Licenses', 'EMPLOYEELICENSE_VIEW', 'Can view employee licenses', 'Documents', 1)
    PRINT '✓ تم إدراج صلاحية EMPLOYEELICENSE_VIEW'
END

-- صلاحية إضافة رخص الموظفين
IF NOT EXISTS (SELECT 1 FROM [Permissions] WHERE [PermissionKey] = 'EMPLOYEELICENSE_ADD')
BEGIN
    INSERT INTO [Permissions] ([PermissionName], [PermissionKey], [PermissionDescription], [CategoryName], [IsActive])
    VALUES ('Add Employee License', 'EMPLOYEELICENSE_ADD', 'Can add new employee licenses', 'Documents', 1)
    PRINT '✓ تم إدراج صلاحية EMPLOYEELICENSE_ADD'
END

-- صلاحية تعديل رخص الموظفين
IF NOT EXISTS (SELECT 1 FROM [Permissions] WHERE [PermissionKey] = 'EMPLOYEELICENSE_EDIT')
BEGIN
    INSERT INTO [Permissions] ([PermissionName], [PermissionKey], [PermissionDescription], [CategoryName], [IsActive])
    VALUES ('Edit Employee License', 'EMPLOYEELICENSE_EDIT', 'Can edit employee licenses', 'Documents', 1)
    PRINT '✓ تم إدراج صلاحية EMPLOYEELICENSE_EDIT'
END

-- صلاحية حذف رخص الموظفين
IF NOT EXISTS (SELECT 1 FROM [Permissions] WHERE [PermissionKey] = 'EMPLOYEELICENSE_DELETE')
BEGIN
    INSERT INTO [Permissions] ([PermissionName], [PermissionKey], [PermissionDescription], [CategoryName], [IsActive])
    VALUES ('Delete Employee License', 'EMPLOYEELICENSE_DELETE', 'Can delete employee licenses', 'Documents', 1)
    PRINT '✓ تم إدراج صلاحية EMPLOYEELICENSE_DELETE'
END

-- صلاحية تصدير رخص الموظفين
IF NOT EXISTS (SELECT 1 FROM [Permissions] WHERE [PermissionKey] = 'EMPLOYEELICENSE_EXPORT')
BEGIN
    INSERT INTO [Permissions] ([PermissionName], [PermissionKey], [PermissionDescription], [CategoryName], [IsActive])
    VALUES ('Export Employee License', 'EMPLOYEELICENSE_EXPORT', 'Can export employee licenses', 'Documents', 1)
    PRINT '✓ تم إدراج صلاحية EMPLOYEELICENSE_EXPORT'
END

-- منح الصلاحيات للمستخدم yazeed.bassam
PRINT 'منح صلاحيات رخص الموظفين للمستخدم yazeed.bassam...'

-- صلاحية العرض
IF NOT EXISTS (SELECT 1 FROM [UserOperationPermissions] WHERE [UserId] = @UserId AND [EntityType] = 'EmployeeLicense' AND [OperationType] = 'View')
BEGIN
    INSERT INTO [UserOperationPermissions] ([UserId], [PermissionId], [EntityType], [OperationType], [IsAllowed], [Scope], [IsActive])
    SELECT @UserId, [PermissionId], 'EmployeeLicense', 'View', 1, 'All', 1
    FROM [Permissions] WHERE [PermissionKey] = 'EMPLOYEELICENSE_VIEW'
    PRINT '✓ تم منح صلاحية عرض رخص الموظفين'
END

-- صلاحية الإضافة
IF NOT EXISTS (SELECT 1 FROM [UserOperationPermissions] WHERE [UserId] = @UserId AND [EntityType] = 'EmployeeLicense' AND [OperationType] = 'Add')
BEGIN
    INSERT INTO [UserOperationPermissions] ([UserId], [PermissionId], [EntityType], [OperationType], [IsAllowed], [Scope], [IsActive])
    SELECT @UserId, [PermissionId], 'EmployeeLicense', 'Add', 1, 'All', 1
    FROM [Permissions] WHERE [PermissionKey] = 'EMPLOYEELICENSE_ADD'
    PRINT '✓ تم منح صلاحية إضافة رخص الموظفين'
END

-- صلاحية التعديل
IF NOT EXISTS (SELECT 1 FROM [UserOperationPermissions] WHERE [UserId] = @UserId AND [EntityType] = 'EmployeeLicense' AND [OperationType] = 'Edit')
BEGIN
    INSERT INTO [UserOperationPermissions] ([UserId], [PermissionId], [EntityType], [OperationType], [IsAllowed], [Scope], [IsActive])
    SELECT @UserId, [PermissionId], 'EmployeeLicense', 'Edit', 1, 'All', 1
    FROM [Permissions] WHERE [PermissionKey] = 'EMPLOYEELICENSE_EDIT'
    PRINT '✓ تم منح صلاحية تعديل رخص الموظفين'
END

-- صلاحية الحذف
IF NOT EXISTS (SELECT 1 FROM [UserOperationPermissions] WHERE [UserId] = @UserId AND [EntityType] = 'EmployeeLicense' AND [OperationType] = 'Delete')
BEGIN
    INSERT INTO [UserOperationPermissions] ([UserId], [PermissionId], [EntityType], [OperationType], [IsAllowed], [Scope], [IsActive])
    SELECT @UserId, [PermissionId], 'EmployeeLicense', 'Delete', 1, 'All', 1
    FROM [Permissions] WHERE [PermissionKey] = 'EMPLOYEELICENSE_DELETE'
    PRINT '✓ تم منح صلاحية حذف رخص الموظفين'
END

-- صلاحية التصدير
IF NOT EXISTS (SELECT 1 FROM [UserOperationPermissions] WHERE [UserId] = @UserId AND [EntityType] = 'EmployeeLicense' AND [OperationType] = 'Export')
BEGIN
    INSERT INTO [UserOperationPermissions] ([UserId], [PermissionId], [EntityType], [OperationType], [IsAllowed], [Scope], [IsActive])
    SELECT @UserId, [PermissionId], 'EmployeeLicense', 'Export', 1, 'All', 1
    FROM [Permissions] WHERE [PermissionKey] = 'EMPLOYEELICENSE_EXPORT'
    PRINT '✓ تم منح صلاحية تصدير رخص الموظفين'
END

-- التحقق من الصلاحيات الممنوحة
PRINT 'التحقق من الصلاحيات الممنوحة للمستخدم yazeed.bassam:'

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

-- اختبار الصلاحيات باستخدام CanUserPerformOperation
PRINT 'اختبار الصلاحيات باستخدام CanUserPerformOperation:'

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CanUserPerformOperation]') AND type in (N'P', N'PC'))
BEGIN
    PRINT 'اختبار صلاحية العرض:'
    EXEC CanUserPerformOperation @UserId, 'EmployeeLicense', 'View'
    
    PRINT 'اختبار صلاحية التعديل:'
    EXEC CanUserPerformOperation @UserId, 'EmployeeLicense', 'Edit'
    
    PRINT 'اختبار صلاحية الحذف:'
    EXEC CanUserPerformOperation @UserId, 'EmployeeLicense', 'Delete'
END
ELSE
BEGIN
    PRINT 'تحذير: CanUserPerformOperation stored procedure غير موجود'
END

PRINT 'تم الانتهاء من إصلاح صلاحيات رخص الموظفين بنجاح!'
GO