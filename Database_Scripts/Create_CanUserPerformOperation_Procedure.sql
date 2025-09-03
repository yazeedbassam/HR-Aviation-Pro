-- =====================================================
-- إنشاء إجراء CanUserPerformOperation المخزن
-- =====================================================
-- هذا السكريبت ينشئ الإجراء المخزن المطلوب للتحقق من صلاحيات العمليات
-- =====================================================

USE [HR-Aviation]
GO

PRINT 'إنشاء إجراء CanUserPerformOperation المخزن...'

-- حذف الإجراء إذا كان موجوداً
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CanUserPerformOperation]') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE [dbo].[CanUserPerformOperation]
    PRINT 'تم حذف الإجراء الموجود'
END

-- إنشاء الإجراء الجديد
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
GO

PRINT '✓ تم إنشاء إجراء CanUserPerformOperation بنجاح'

-- اختبار الإجراء
PRINT 'اختبار الإجراء مع مستخدم admin:'

DECLARE @AdminUserId int
SELECT @AdminUserId = userid FROM users WHERE rolename = 'Admin'

IF @AdminUserId IS NOT NULL
BEGIN
    PRINT 'اختبار صلاحيات Admin:'
    EXEC CanUserPerformOperation @AdminUserId, 'EmployeeLicense', 'View'
    EXEC CanUserPerformOperation @AdminUserId, 'EmployeeLicense', 'Edit'
    EXEC CanUserPerformOperation @AdminUserId, 'EmployeeLicense', 'Delete'
END

PRINT 'تم الانتهاء من إنشاء الإجراء بنجاح!'
GO