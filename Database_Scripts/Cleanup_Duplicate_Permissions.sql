-- =====================================================
-- تنظيف الصلاحيات المكررة قبل إضافة الجديدة
-- =====================================================

USE [HR-Aviation]
GO

PRINT '========================================'
PRINT 'تنظيف الصلاحيات المكررة'
PRINT '========================================'

-- حذف صلاحيات الشهادات المكررة من UserOperationPermissions
DELETE FROM [UserOperationPermissions] 
WHERE EntityType IN ('ControllerCertificate', 'EmployeeCertificate')

PRINT 'تم حذف جميع صلاحيات الشهادات المكررة من UserOperationPermissions'

-- التحقق من عدد الصلاحيات الحالي
SELECT 
    'صلاحيات الشهادات في Permissions' as النوع,
    COUNT(*) as العدد
FROM [Permissions]
WHERE PermissionKey LIKE '%CERTIFICATE%'

UNION ALL

SELECT 
    'صلاحيات الشهادات في UserOperationPermissions' as النوع,
    COUNT(*) as العدد
FROM [UserOperationPermissions]
WHERE EntityType IN ('ControllerCertificate', 'EmployeeCertificate')

PRINT '========================================'
PRINT 'تم الانتهاء من التنظيف!'
PRINT '========================================'