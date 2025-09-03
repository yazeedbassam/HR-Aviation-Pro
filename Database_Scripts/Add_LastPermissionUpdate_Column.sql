-- =====================================================
-- إضافة عمود LastPermissionUpdate إلى جدول Users
-- =====================================================

-- التحقق من وجود العمود
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'LastPermissionUpdate')
BEGIN
    -- إضافة العمود
    ALTER TABLE [dbo].[Users] ADD [LastPermissionUpdate] [datetime] NULL;
    PRINT '✓ تم إضافة عمود LastPermissionUpdate إلى جدول Users';
END
ELSE
BEGIN
    PRINT '✓ عمود LastPermissionUpdate موجود بالفعل في جدول Users';
END

-- تحديث العمود للبيانات الموجودة
UPDATE [dbo].[Users] 
SET [LastPermissionUpdate] = GETDATE() 
WHERE [LastPermissionUpdate] IS NULL;

PRINT '✓ تم تحديث LastPermissionUpdate لجميع المستخدمين الموجودين';

-- إنشاء Trigger لتحديث العمود تلقائياً عند تغيير الصلاحيات
IF NOT EXISTS (SELECT * FROM sys.triggers WHERE name = 'TR_Users_UpdateLastPermissionUpdate')
BEGIN
    EXEC('
    CREATE TRIGGER TR_Users_UpdateLastPermissionUpdate
    ON Users
    AFTER UPDATE
    AS
    BEGIN
        IF UPDATE(LastPermissionUpdate)
            RETURN;
            
        UPDATE Users 
        SET LastPermissionUpdate = GETDATE() 
        FROM Users u
        INNER JOIN inserted i ON u.UserId = i.UserId;
    END');
    
    PRINT '✓ تم إنشاء Trigger لتحديث LastPermissionUpdate تلقائياً';
END
ELSE
BEGIN
    PRINT '✓ Trigger TR_Users_UpdateLastPermissionUpdate موجود بالفعل';
END

PRINT 'تم الانتهاء من إضافة عمود LastPermissionUpdate'; 