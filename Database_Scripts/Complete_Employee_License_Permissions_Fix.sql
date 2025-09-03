-- =====================================================
-- إصلاح شامل لصلاحيات رخص الموظفين
-- =====================================================
-- هذا السكريبت يحل جميع مشاكل الصلاحيات المتعلقة برخص الموظفين
-- ويضمن أن المستخدم yazeed.bassam يمكنه عرض وتعديل وحذف رخص الموظفين
-- =====================================================

USE [HR-Aviation]
GO

PRINT '====================================================='
PRINT 'بدء الإصلاح الشامل لصلاحيات رخص الموظفين'
PRINT '====================================================='

-- الخطوة 1: التحقق من وجود المستخدم yazeed.bassam
DECLARE @UserId int
SELECT @UserId = userid FROM users WHERE username = 'yazeed.bassam'

IF @UserId IS NULL
BEGIN
    PRINT 'خطأ: المستخدم yazeed.bassam غير موجود في قاعدة البيانات'
    PRINT 'الرجاء التحقق من اسم المستخدم أو إنشاء المستخدم أولاً'
    RETURN
END

PRINT '✓ تم العثور على المستخدم yazeed.bassam مع ID: ' + CAST(@UserId AS VARCHAR(10))

-- الخطوة 2: إنشاء جدول UserOperationPermissions إذا لم يكن موجوداً
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
ELSE
BEGIN
    PRINT '✓ جدول UserOperationPermissions موجود بالفعل'
END

-- الخطوة 3: إنشاء إجراء CanUserPerformOperation إذا لم يكن موجوداً
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CanUserPerformOperation]') AND type in (N'P', N'PC'))
BEGIN
    PRINT 'إنشاء إجراء CanUserPerformOperation...'
    
    CREATE PROCEDURE [dbo].[CanUserPerformOperation]
        @UserId int,
        @EntityType nvarchar(50),
        @OperationType nvarchar(50),
        @Scope nvarchar(50) = 'All',
        @ScopeId int = NULL
    AS
    BEGIN
        SET NOCOUNT ON;
        
        DECLARE @IsAllowed bit = 0
        
        -- التحقق من وجود المستخدم
        IF NOT EXISTS (SELECT 1 FROM users WHERE userid = @UserId)
        BEGIN
            SELECT 0 as IsAllowed
            RETURN
        END
        
        -- التحقق من أن المستخدم هو Admin (لديه جميع الصلاحيات)
        IF EXISTS (SELECT 1 FROM users WHERE userid = @UserId AND rolename = 'Admin')
        BEGIN
            SELECT 1 as IsAllowed
            RETURN
        END
        
        -- التحقق من الصلاحية المحددة في UserOperationPermissions
        SELECT @IsAllowed = uop.IsAllowed
        FROM UserOperationPermissions uop
        INNER JOIN Permissions p ON uop.PermissionId = p.PermissionId
        WHERE uop.UserId = @UserId 
            AND uop.EntityType = @EntityType 
            AND uop.OperationType = @OperationType
            AND uop.IsActive = 1
            AND p.IsActive = 1
            AND (uop.Scope = @Scope OR uop.Scope = 'All')
            AND (@ScopeId IS NULL OR uop.ScopeId = @ScopeId OR uop.ScopeId IS NULL)
        
        -- إذا لم توجد صلاحية محددة، افترض عدم السماح
        IF @IsAllowed IS NULL
        BEGIN
            SET @IsAllowed = 0
        END
        
        SELECT @IsAllowed as IsAllowed
    END
    
    PRINT '✓ تم إنشاء إجراء CanUserPerformOperation'
END
ELSE
BEGIN
    PRINT '✓ إجراء CanUserPerformOperation موجود بالفعل'
END

-- الخطوة 4: إدراج صلاحيات رخص الموظفين
PRINT 'إدراج صلاحيات رخص الموظفين...'

-- صلاحية عرض رخص الموظفين
IF NOT EXISTS (SELECT 1 FROM [Permissions] WHERE [PermissionKey] = 'EMPLOYEELICENSE_VIEW')
BEGIN
    INSERT INTO [Permissions] ([PermissionName], [PermissionKey], [PermissionDescription], [CategoryName], [IsActive])
    VALUES ('View Employee Licenses', 'EMPLOYEELICENSE_VIEW', 'Can view employee licenses', 'Documents', 1)
    PRINT '✓ تم إدراج صلاحية EMPLOYEELICENSE_VIEW'
END
ELSE
BEGIN
    PRINT '✓ صلاحية EMPLOYEELICENSE_VIEW موجودة بالفعل'
END

-- صلاحية إضافة رخص الموظفين
IF NOT EXISTS (SELECT 1 FROM [Permissions] WHERE [PermissionKey] = 'EMPLOYEELICENSE_ADD')
BEGIN
    INSERT INTO [Permissions] ([PermissionName], [PermissionKey], [PermissionDescription], [CategoryName], [IsActive])
    VALUES ('Add Employee License', 'EMPLOYEELICENSE_ADD', 'Can add new employee licenses', 'Documents', 1)
    PRINT '✓ تم إدراج صلاحية EMPLOYEELICENSE_ADD'
END
ELSE
BEGIN
    PRINT '✓ صلاحية EMPLOYEELICENSE_ADD موجودة بالفعل'
END

-- صلاحية تعديل رخص الموظفين
IF NOT EXISTS (SELECT 1 FROM [Permissions] WHERE [PermissionKey] = 'EMPLOYEELICENSE_EDIT')
BEGIN
    INSERT INTO [Permissions] ([PermissionName], [PermissionKey], [PermissionDescription], [CategoryName], [IsActive])
    VALUES ('Edit Employee License', 'EMPLOYEELICENSE_EDIT', 'Can edit employee licenses', 'Documents', 1)
    PRINT '✓ تم إدراج صلاحية EMPLOYEELICENSE_EDIT'
END
ELSE
BEGIN
    PRINT '✓ صلاحية EMPLOYEELICENSE_EDIT موجودة بالفعل'
END

-- صلاحية حذف رخص الموظفين
IF NOT EXISTS (SELECT 1 FROM [Permissions] WHERE [PermissionKey] = 'EMPLOYEELICENSE_DELETE')
BEGIN
    INSERT INTO [Permissions] ([PermissionName], [PermissionKey], [PermissionDescription], [CategoryName], [IsActive])
    VALUES ('Delete Employee License', 'EMPLOYEELICENSE_DELETE', 'Can delete employee licenses', 'Documents', 1)
    PRINT '✓ تم إدراج صلاحية EMPLOYEELICENSE_DELETE'
END
ELSE
BEGIN
    PRINT '✓ صلاحية EMPLOYEELICENSE_DELETE موجودة بالفعل'
END

-- صلاحية تصدير رخص الموظفين
IF NOT EXISTS (SELECT 1 FROM [Permissions] WHERE [PermissionKey] = 'EMPLOYEELICENSE_EXPORT')
BEGIN
    INSERT INTO [Permissions] ([PermissionName], [PermissionKey], [PermissionDescription], [CategoryName], [IsActive])
    VALUES ('Export Employee License', 'EMPLOYEELICENSE_EXPORT', 'Can export employee licenses', 'Documents', 1)
    PRINT '✓ تم إدراج صلاحية EMPLOYEELICENSE_EXPORT'
END
ELSE
BEGIN
    PRINT '✓ صلاحية EMPLOYEELICENSE_EXPORT موجودة بالفعل'
END

-- الخطوة 5: منح الصلاحيات للمستخدم yazeed.bassam
PRINT 'منح صلاحيات رخص الموظفين للمستخدم yazeed.bassam...'

-- حذف الصلاحيات الموجودة مسبقاً (إذا كانت موجودة)
DELETE FROM [UserOperationPermissions] 
WHERE [UserId] = @UserId AND [EntityType] = 'EmployeeLicense'

PRINT '✓ تم حذف الصلاحيات القديمة (إن وجدت)'

-- منح الصلاحيات الجديدة
INSERT INTO [UserOperationPermissions] ([UserId], [PermissionId], [EntityType], [OperationType], [IsAllowed], [Scope], [IsActive])
SELECT @UserId, [PermissionId], 'EmployeeLicense', 'View', 1, 'All', 1
FROM [Permissions] WHERE [PermissionKey] = 'EMPLOYEELICENSE_VIEW'

INSERT INTO [UserOperationPermissions] ([UserId], [PermissionId], [EntityType], [OperationType], [IsAllowed], [Scope], [IsActive])
SELECT @UserId, [PermissionId], 'EmployeeLicense', 'Add', 1, 'All', 1
FROM [Permissions] WHERE [PermissionKey] = 'EMPLOYEELICENSE_ADD'

INSERT INTO [UserOperationPermissions] ([UserId], [PermissionId], [EntityType], [OperationType], [IsAllowed], [Scope], [IsActive])
SELECT @UserId, [PermissionId], 'EmployeeLicense', 'Edit', 1, 'All', 1
FROM [Permissions] WHERE [PermissionKey] = 'EMPLOYEELICENSE_EDIT'

INSERT INTO [UserOperationPermissions] ([UserId], [PermissionId], [EntityType], [OperationType], [IsAllowed], [Scope], [IsActive])
SELECT @UserId, [PermissionId], 'EmployeeLicense', 'Delete', 1, 'All', 1
FROM [Permissions] WHERE [PermissionKey] = 'EMPLOYEELICENSE_DELETE'

INSERT INTO [UserOperationPermissions] ([UserId], [PermissionId], [EntityType], [OperationType], [IsAllowed], [Scope], [IsActive])
SELECT @UserId, [PermissionId], 'EmployeeLicense', 'Export', 1, 'All', 1
FROM [Permissions] WHERE [PermissionKey] = 'EMPLOYEELICENSE_EXPORT'

PRINT '✓ تم منح جميع صلاحيات رخص الموظفين للمستخدم yazeed.bassam'

-- الخطوة 6: التحقق من الصلاحيات الممنوحة
PRINT '====================================================='
PRINT 'التحقق من الصلاحيات الممنوحة:'
PRINT '====================================================='

SELECT 
    uop.EntityType,
    uop.OperationType,
    uop.IsAllowed,
    p.PermissionName,
    p.PermissionKey
FROM [UserOperationPermissions] uop
INNER JOIN [Permissions] p ON uop.PermissionId = p.PermissionId
WHERE uop.UserId = @UserId 
    AND uop.EntityType = 'EmployeeLicense'
    AND uop.IsActive = 1
ORDER BY uop.OperationType

-- الخطوة 7: اختبار الصلاحيات
PRINT '====================================================='
PRINT 'اختبار الصلاحيات باستخدام CanUserPerformOperation:'
PRINT '====================================================='

DECLARE @ViewResult bit, @EditResult bit, @DeleteResult bit

-- اختبار صلاحية العرض
EXEC CanUserPerformOperation @UserId, 'EmployeeLicense', 'View'
SELECT @ViewResult = IsAllowed FROM (EXEC CanUserPerformOperation @UserId, 'EmployeeLicense', 'View') AS Result

-- اختبار صلاحية التعديل
EXEC CanUserPerformOperation @UserId, 'EmployeeLicense', 'Edit'
SELECT @EditResult = IsAllowed FROM (EXEC CanUserPerformOperation @UserId, 'EmployeeLicense', 'Edit') AS Result

-- اختبار صلاحية الحذف
EXEC CanUserPerformOperation @UserId, 'EmployeeLicense', 'Delete'
SELECT @DeleteResult = IsAllowed FROM (EXEC CanUserPerformOperation @UserId, 'EmployeeLicense', 'Delete') AS Result

-- عرض النتائج
PRINT 'نتائج الاختبار:'
PRINT 'صلاحية العرض: ' + CASE WHEN @ViewResult = 1 THEN 'مسموح' ELSE 'ممنوع' END
PRINT 'صلاحية التعديل: ' + CASE WHEN @EditResult = 1 THEN 'مسموح' ELSE 'ممنوع' END
PRINT 'صلاحية الحذف: ' + CASE WHEN @DeleteResult = 1 THEN 'مسموح' ELSE 'ممنوع' END

-- الخطوة 8: تنظيف ذاكرة التخزين المؤقت
PRINT '====================================================='
PRINT 'تنظيف ذاكرة التخزين المؤقت...'
PRINT '====================================================='

-- ملاحظة: في التطبيق الحقيقي، يجب إعادة تشغيل التطبيق أو مسح ذاكرة التخزين المؤقت
PRINT 'ملاحظة: يرجى إعادة تشغيل التطبيق لضمان تطبيق التغييرات'

PRINT '====================================================='
PRINT 'تم الانتهاء من الإصلاح الشامل بنجاح!'
PRINT '====================================================='
PRINT 'الآن يجب أن يتمكن المستخدم yazeed.bassam من:'
PRINT '- عرض رخص الموظفين'
PRINT '- تعديل رخص الموظفين'
PRINT '- حذف رخص الموظفين'
PRINT '- إضافة رخص جديدة للموظفين'
PRINT '- تصدير رخص الموظفين'
PRINT '====================================================='

GO