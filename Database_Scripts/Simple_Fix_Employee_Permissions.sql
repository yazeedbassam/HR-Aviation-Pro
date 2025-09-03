-- =====================================================
-- إصلاح مبسط لصلاحيات رخص الموظفين
-- =====================================================

USE [HR-Aviation]
GO

PRINT 'بدء الإصلاح المبسط...'

-- التحقق من وجود المستخدم yazeed.bassam
DECLARE @UserId int
SELECT @UserId = userid FROM users WHERE username = 'yazeed.bassam'

IF @UserId IS NULL
BEGIN
    PRINT 'خطأ: المستخدم yazeed.bassam غير موجود'
    RETURN
END

PRINT 'تم العثور على المستخدم مع ID: ' + CAST(@UserId AS VARCHAR(10))

-- إنشاء جدول UserOperationPermissions إذا لم يكن موجوداً
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UserOperationPermissions]') AND type in (N'U'))
BEGIN
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
        CONSTRAINT [PK_UserOperationPermissions] PRIMARY KEY CLUSTERED ([UserOperationPermissionId] ASC)
    )
    PRINT 'تم إنشاء جدول UserOperationPermissions'
END

-- إدراج الصلاحيات
IF NOT EXISTS (SELECT 1 FROM [Permissions] WHERE [PermissionKey] = 'EMPLOYEELICENSE_VIEW')
BEGIN
    INSERT INTO [Permissions] ([PermissionName], [PermissionKey], [PermissionDescription], [CategoryName], [IsActive])
    VALUES ('View Employee Licenses', 'EMPLOYEELICENSE_VIEW', 'Can view employee licenses', 'Documents', 1)
END

IF NOT EXISTS (SELECT 1 FROM [Permissions] WHERE [PermissionKey] = 'EMPLOYEELICENSE_EDIT')
BEGIN
    INSERT INTO [Permissions] ([PermissionName], [PermissionKey], [PermissionDescription], [CategoryName], [IsActive])
    VALUES ('Edit Employee License', 'EMPLOYEELICENSE_EDIT', 'Can edit employee licenses', 'Documents', 1)
END

IF NOT EXISTS (SELECT 1 FROM [Permissions] WHERE [PermissionKey] = 'EMPLOYEELICENSE_DELETE')
BEGIN
    INSERT INTO [Permissions] ([PermissionName], [PermissionKey], [PermissionDescription], [CategoryName], [IsActive])
    VALUES ('Delete Employee License', 'EMPLOYEELICENSE_DELETE', 'Can delete employee licenses', 'Documents', 1)
END

-- حذف الصلاحيات القديمة
DELETE FROM [UserOperationPermissions] WHERE [UserId] = @UserId AND [EntityType] = 'EmployeeLicense'

-- منح الصلاحيات الجديدة
INSERT INTO [UserOperationPermissions] ([UserId], [PermissionId], [EntityType], [OperationType], [IsAllowed], [Scope], [IsActive])
SELECT @UserId, [PermissionId], 'EmployeeLicense', 'View', 1, 'All', 1
FROM [Permissions] WHERE [PermissionKey] = 'EMPLOYEELICENSE_VIEW'

INSERT INTO [UserOperationPermissions] ([UserId], [PermissionId], [EntityType], [OperationType], [IsAllowed], [Scope], [IsActive])
SELECT @UserId, [PermissionId], 'EmployeeLicense', 'Edit', 1, 'All', 1
FROM [Permissions] WHERE [PermissionKey] = 'EMPLOYEELICENSE_EDIT'

INSERT INTO [UserOperationPermissions] ([UserId], [PermissionId], [EntityType], [OperationType], [IsAllowed], [Scope], [IsActive])
SELECT @UserId, [PermissionId], 'EmployeeLicense', 'Delete', 1, 'All', 1
FROM [Permissions] WHERE [PermissionKey] = 'EMPLOYEELICENSE_DELETE'

PRINT 'تم منح الصلاحيات بنجاح'

-- عرض الصلاحيات الممنوحة
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

PRINT 'تم الانتهاء من الإصلاح المبسط!'
GO