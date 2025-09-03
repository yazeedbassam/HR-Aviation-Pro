-- =====================================================
-- اختبار صلاحيات الشهادات الجديدة - مصحح
-- =====================================================

USE [HR-Aviation]
GO

PRINT '========================================'
PRINT 'اختبار صلاحيات الشهادات الجديدة'
PRINT '========================================'
PRINT ''

-- التحقق من وجود الصلاحيات في جدول Permissions
PRINT '📋 التحقق من صلاحيات الشهادات في جدول Permissions:'
PRINT ''

SELECT 
    PermissionKey,
    PermissionName,
    PermissionDescription,
    CategoryName,
    IsActive
FROM [Permissions] 
WHERE PermissionKey LIKE '%CERTIFICATE%'
ORDER BY PermissionKey

PRINT ''
PRINT '========================================'

-- التحقق من صلاحيات المستخدمين
PRINT '👥 التحقق من صلاحيات المستخدمين للشهادات:'
PRINT ''

SELECT 
    u.username,
    u.RoleName,
    uop.EntityType,
    uop.OperationType,
    uop.IsAllowed,
    uop.Scope
FROM [UserOperationPermissions] uop
INNER JOIN [users] u ON uop.UserId = u.userid
WHERE uop.EntityType IN ('ControllerCertificate', 'EmployeeCertificate')
ORDER BY u.username, uop.EntityType, uop.OperationType

PRINT ''
PRINT '========================================'

-- إحصائيات الصلاحيات
PRINT '📊 إحصائيات الصلاحيات:'
PRINT ''

SELECT 
    'إجمالي الصلاحيات' as النوع,
    COUNT(*) as العدد
FROM [Permissions]
WHERE IsActive = 1

UNION ALL

SELECT 
    'صلاحيات الشهادات' as النوع,
    COUNT(*) as العدد
FROM [Permissions]
WHERE IsActive = 1 AND PermissionKey LIKE '%CERTIFICATE%'

UNION ALL

SELECT 
    'صلاحيات العمليات للمستخدمين' as النوع,
    COUNT(*) as العدد
FROM [UserOperationPermissions]
WHERE IsActive = 1

UNION ALL

SELECT 
    'صلاحيات الشهادات للمستخدمين' as النوع,
    COUNT(*) as العدد
FROM [UserOperationPermissions]
WHERE IsActive = 1 AND EntityType IN ('ControllerCertificate', 'EmployeeCertificate')

PRINT ''
PRINT '========================================'
PRINT 'تم الانتهاء من اختبار الصلاحيات!'
PRINT '========================================'