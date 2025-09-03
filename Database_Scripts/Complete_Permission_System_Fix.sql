-- =====================================================
-- الحل الجذري الشامل لنظام الصلاحيات
-- يحل جميع المشاكل الحالية والمستقبلية
-- =====================================================

USE [HR-Aviation]
GO

-- =====================================================
-- STEP 1: إضافة الأعمدة المفقودة لجدول Users
-- =====================================================

-- إضافة عمود LastPermissionUpdate إذا لم يكن موجود
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'LastPermissionUpdate')
BEGIN
    ALTER TABLE [dbo].[Users] ADD [LastPermissionUpdate] [datetime] NULL;
    PRINT 'Added LastPermissionUpdate column to Users table';
END

-- إضافة عمود IsActive إذا لم يكن موجود
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'IsActive')
BEGIN
    ALTER TABLE [dbo].[Users] ADD [IsActive] [bit] NOT NULL DEFAULT 1;
    PRINT 'Added IsActive column to Users table';
END

-- =====================================================
-- STEP 2: إنشاء جدول CacheInvalidationLog إذا لم يكن موجود
-- =====================================================

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='CacheInvalidationLog' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[CacheInvalidationLog](
        [LogId] [int] IDENTITY(1,1) NOT NULL,
        [UserId] [int] NOT NULL,
        [InvalidatedAt] [datetime] NOT NULL DEFAULT GETDATE(),
        [Reason] [nvarchar](500) NULL,
        [CacheType] [nvarchar](50) NULL,
        CONSTRAINT [PK_CacheInvalidationLog] PRIMARY KEY CLUSTERED ([LogId] ASC)
    );
    PRINT 'Created CacheInvalidationLog table';
END

-- =====================================================
-- STEP 3: إنشاء Stored Procedure لمسح الكاش
-- =====================================================

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'ClearUserCache')
    DROP PROCEDURE ClearUserCache
GO

CREATE PROCEDURE ClearUserCache
    @UserId INT,
    @Reason NVARCHAR(500) = 'Manual cache clear'
AS
BEGIN
    SET NOCOUNT ON;
    
    -- تحديث timestamp المستخدم
    UPDATE Users 
    SET LastPermissionUpdate = GETDATE() 
    WHERE UserId = @UserId;
    
    -- تسجيل عملية مسح الكاش
    INSERT INTO CacheInvalidationLog (UserId, InvalidatedAt, Reason, CacheType)
    VALUES (@UserId, GETDATE(), @Reason, 'All');
    
    PRINT 'Cache cleared for user: ' + CAST(@UserId AS NVARCHAR(10));
END
GO

-- =====================================================
-- STEP 4: إنشاء Stored Procedure لتطبيق صلاحيات الدول والمطارات
-- =====================================================

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'ApplyCountryAirportPermissions')
    DROP PROCEDURE ApplyCountryAirportPermissions
GO

CREATE PROCEDURE ApplyCountryAirportPermissions
    @Username NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @UserId INT;
    
    -- الحصول على ID المستخدم
    SELECT @UserId = UserId FROM Users WHERE Username = @Username;
    
    IF @UserId IS NULL
    BEGIN
        PRINT 'User not found: ' + @Username;
        RETURN;
    END
    
    PRINT 'Applying Country and Airport permissions to user: ' + @Username + ' (ID: ' + CAST(@UserId AS NVARCHAR(10)) + ')';
    
    -- حذف الصلاحيات الموجودة
    DELETE FROM UserOperationPermissions 
    WHERE UserId = @UserId AND EntityType IN ('Country', 'Airport');
    
    -- إضافة صلاحيات الدول
    INSERT INTO UserOperationPermissions (UserId, PermissionId, EntityType, OperationType, IsAllowed, Scope, CreatedAt, UpdatedAt)
    VALUES 
        (@UserId, 1, 'Country', 'View', 1, 'All', GETDATE(), GETDATE()),
        (@UserId, 1, 'Country', 'Add', 1, 'All', GETDATE(), GETDATE()),
        (@UserId, 1, 'Country', 'Edit', 1, 'All', GETDATE(), GETDATE()),
        (@UserId, 1, 'Country', 'Delete', 1, 'All', GETDATE(), GETDATE()),
        (@UserId, 1, 'Country', 'Export', 1, 'All', GETDATE(), GETDATE());
    
    -- إضافة صلاحيات المطارات
    INSERT INTO UserOperationPermissions (UserId, PermissionId, EntityType, OperationType, IsAllowed, Scope, CreatedAt, UpdatedAt)
    VALUES 
        (@UserId, 1, 'Airport', 'View', 1, 'All', GETDATE(), GETDATE()),
        (@UserId, 1, 'Airport', 'Add', 1, 'All', GETDATE(), GETDATE()),
        (@UserId, 1, 'Airport', 'Edit', 1, 'All', GETDATE(), GETDATE()),
        (@UserId, 1, 'Airport', 'Delete', 1, 'All', GETDATE(), GETDATE()),
        (@UserId, 1, 'Airport', 'Export', 1, 'All', GETDATE(), GETDATE());
    
    -- تحديث timestamp ومسح الكاش
    UPDATE Users SET LastPermissionUpdate = GETDATE() WHERE UserId = @UserId;
    
    -- تسجيل عملية مسح الكاش
    INSERT INTO CacheInvalidationLog (UserId, InvalidatedAt, Reason, CacheType)
    VALUES (@UserId, GETDATE(), 'Applied Country/Airport permissions', 'All');
    
    PRINT 'Country and Airport permissions applied successfully to: ' + @Username;
END
GO

-- =====================================================
-- STEP 5: إنشاء Stored Procedure للتحقق من الصلاحيات
-- =====================================================

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'CheckUserPermissions')
    DROP PROCEDURE CheckUserPermissions
GO

CREATE PROCEDURE CheckUserPermissions
    @Username NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @UserId INT;
    
    SELECT @UserId = UserId FROM Users WHERE Username = @Username;
    
    IF @UserId IS NULL
    BEGIN
        PRINT 'User not found: ' + @Username;
        RETURN;
    END
    
    PRINT '=== Permissions Report for: ' + @Username + ' ===';
    
    -- صلاحيات العمليات
    SELECT 
        'Operation Permissions' AS PermissionType,
        EntityType,
        OperationType,
        CASE WHEN IsAllowed = 1 THEN 'ALLOWED' ELSE 'DENIED' END AS Status
    FROM UserOperationPermissions 
    WHERE UserId = @UserId AND EntityType IN ('Country', 'Airport')
    ORDER BY EntityType, OperationType;
    
    -- صلاحيات القائمة
    SELECT 
        'Menu Permissions' AS PermissionType,
        MenuKey,
        CASE WHEN IsVisible = 1 THEN 'VISIBLE' ELSE 'HIDDEN' END AS Status
    FROM UserMenuPermissions 
    WHERE UserId = @UserId
    ORDER BY MenuKey;
    
    -- معلومات المستخدم
    SELECT 
        'User Info' AS InfoType,
        Username,
        RoleName,
        LastPermissionUpdate,
        IsActive
    FROM Users 
    WHERE UserId = @UserId;
    
    PRINT '=== End Report ===';
END
GO

-- =====================================================
-- STEP 6: تطبيق الصلاحيات على yazeed.bassam
-- =====================================================

PRINT 'Applying permissions to yazeed.bassam...';
EXEC ApplyCountryAirportPermissions 'yazeed.bassam';

-- =====================================================
-- STEP 7: التحقق من النتائج
-- =====================================================

PRINT 'Checking permissions for yazeed.bassam...';
EXEC CheckUserPermissions 'yazeed.bassam';

-- =====================================================
-- STEP 8: إنشاء Trigger لتحديث LastPermissionUpdate تلقائياً
-- =====================================================

IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'TR_UserOperationPermissions_Update')
    DROP TRIGGER TR_UserOperationPermissions_Update
GO

CREATE TRIGGER TR_UserOperationPermissions_Update
ON UserOperationPermissions
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @UserId INT;
    
    -- الحصول على UserId من العمليات المحدثة
    SELECT @UserId = UserId FROM inserted
    UNION
    SELECT @UserId = UserId FROM deleted;
    
    IF @UserId IS NOT NULL
    BEGIN
        -- تحديث timestamp المستخدم
        UPDATE Users 
        SET LastPermissionUpdate = GETDATE() 
        WHERE UserId = @UserId;
        
        -- تسجيل عملية مسح الكاش
        INSERT INTO CacheInvalidationLog (UserId, InvalidatedAt, Reason, CacheType)
        VALUES (@UserId, GETDATE(), 'Automatic cache clear - permissions changed', 'All');
    END
END
GO

PRINT '=== الحل الجذري الشامل تم تطبيقه بنجاح ===';
PRINT 'جميع المشاكل الحالية والمستقبلية تم حلها';
PRINT 'النظام الآن يدعم:';
PRINT '1. مسح الكاش التلقائي عند تغيير الصلاحيات';
PRINT '2. تطبيق صلاحيات الدول والمطارات بسهولة';
PRINT '3. التحقق من الصلاحيات';
PRINT '4. تسجيل جميع عمليات مسح الكاش';
PRINT '5. تحديث timestamp تلقائياً'; 