-- =====================================================
-- إضافة الأعمدة المفقودة لجدول Permissions
-- =====================================================

USE [HR-Aviation]
GO

-- إضافة عمود CategoryName إذا لم يكن موجوداً
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Permissions' AND COLUMN_NAME = 'CategoryName')
BEGIN
    ALTER TABLE [Permissions] ADD [CategoryName] [nvarchar](50) NOT NULL DEFAULT 'General'
    PRINT 'تم إضافة عمود CategoryName إلى جدول Permissions'
END
ELSE
BEGIN
    PRINT 'عمود CategoryName موجود بالفعل'
END

-- إضافة عمود PermissionDescription إذا لم يكن موجوداً
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Permissions' AND COLUMN_NAME = 'PermissionDescription')
BEGIN
    ALTER TABLE [Permissions] ADD [PermissionDescription] [nvarchar](500) NULL
    PRINT 'تم إضافة عمود PermissionDescription إلى جدول Permissions'
END
ELSE
BEGIN
    PRINT 'عمود PermissionDescription موجود بالفعل'
END

-- تحديث البيانات الموجودة
UPDATE [Permissions] SET [CategoryName] = 'Dashboard' WHERE [PermissionKey] LIKE '%DASHBOARD%'
UPDATE [Permissions] SET [CategoryName] = 'Staff' WHERE [PermissionKey] LIKE '%EMPLOYEES%' OR [PermissionKey] LIKE '%CONTROLLERS%'
UPDATE [Permissions] SET [CategoryName] = 'Documents' WHERE [PermissionKey] LIKE '%LICENSES%' OR [PermissionKey] LIKE '%CERTIFICATES%'
UPDATE [Permissions] SET [CategoryName] = 'Activities' WHERE [PermissionKey] LIKE '%OBSERVATIONS%'
UPDATE [Permissions] SET [CategoryName] = 'System' WHERE [PermissionKey] LIKE '%CONFIGURATION%' OR [PermissionKey] LIKE '%PERMISSIONS%'

-- تحديث الوصف إذا كان فارغاً
UPDATE [Permissions] SET [PermissionDescription] = 'Can view the main dashboard' WHERE [PermissionKey] = 'DASHBOARD_VIEW' AND ([PermissionDescription] IS NULL OR [PermissionDescription] = '')
UPDATE [Permissions] SET [PermissionDescription] = 'Can view employee list' WHERE [PermissionKey] = 'EMPLOYEES_VIEW' AND ([PermissionDescription] IS NULL OR [PermissionDescription] = '')
UPDATE [Permissions] SET [PermissionDescription] = 'Can view controller list' WHERE [PermissionKey] = 'CONTROLLERS_VIEW' AND ([PermissionDescription] IS NULL OR [PermissionDescription] = '')
UPDATE [Permissions] SET [PermissionDescription] = 'Can view license information' WHERE [PermissionKey] = 'LICENSES_VIEW' AND ([PermissionDescription] IS NULL OR [PermissionDescription] = '')
UPDATE [Permissions] SET [PermissionDescription] = 'Can view certificate information' WHERE [PermissionKey] = 'CERTIFICATES_VIEW' AND ([PermissionDescription] IS NULL OR [PermissionDescription] = '')
UPDATE [Permissions] SET [PermissionDescription] = 'Can view observations' WHERE [PermissionKey] = 'OBSERVATIONS_VIEW' AND ([PermissionDescription] IS NULL OR [PermissionDescription] = '')
UPDATE [Permissions] SET [PermissionDescription] = 'Can view system configuration' WHERE [PermissionKey] = 'CONFIGURATION_VIEW' AND ([PermissionDescription] IS NULL OR [PermissionDescription] = '')
UPDATE [Permissions] SET [PermissionDescription] = 'Can view permission management' WHERE [PermissionKey] = 'PERMISSIONS_VIEW' AND ([PermissionDescription] IS NULL OR [PermissionDescription] = '')

PRINT 'تم إصلاح الجداول بنجاح!'
GO 
