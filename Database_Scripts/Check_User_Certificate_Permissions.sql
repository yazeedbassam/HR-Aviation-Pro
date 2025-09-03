-- =====================================================
-- فحص صلاحيات الشهادات للمستخدم yazeed.bassam
-- =====================================================

USE [HR-Aviation]
GO

PRINT '========================================'
PRINT 'فحص صلاحيات الشهادات للمستخدم yazeed.bassam'
PRINT '========================================'

-- التحقق من معلومات المستخدم
PRINT '👤 معلومات المستخدم:'
SELECT 
    userid,
    username,
    RoleName,
    IsActive
FROM [users] 
WHERE username = 'yazeed.bassam'

PRINT ''
PRINT '========================================'

-- التحقق من صلاحيات الشهادات للمستخدم
PRINT '🔍 صلاحيات الشهادات للمستخدم yazeed.bassam:'
SELECT 
    u.username,
    u.RoleName,
    uop.EntityType,
    uop.OperationType,
    uop.IsAllowed,
    uop.Scope,
    uop.CreatedAt
FROM [UserOperationPermissions] uop
INNER JOIN [users] u ON uop.UserId = u.userid
WHERE u.username = 'yazeed.bassam'
AND uop.EntityType IN ('ControllerCertificate', 'EmployeeCertificate')
ORDER BY uop.EntityType, uop.OperationType

PRINT ''
PRINT '========================================'

-- التحقق من وجود الصلاحيات في جدول Permissions
PRINT '📋 صلاحيات الشهادات في جدول Permissions:'
SELECT 
    PermissionKey,
    PermissionName,
    CategoryName,
    IsActive
FROM [Permissions] 
WHERE PermissionKey LIKE '%CERTIFICATE%'
ORDER BY PermissionKey

PRINT ''
PRINT '========================================'

-- إحصائيات الصلاحيات
PRINT '📊 إحصائيات الصلاحيات:'
SELECT 
    'إجمالي صلاحيات الشهادات في Permissions' as النوع,
    COUNT(*) as العدد
FROM [Permissions]
WHERE PermissionKey LIKE '%CERTIFICATE%'

UNION ALL

SELECT 
    'صلاحيات الشهادات للمستخدم yazeed.bassam' as النوع,
    COUNT(*) as العدد
FROM [UserOperationPermissions] uop
INNER JOIN [users] u ON uop.UserId = u.userid
WHERE u.username = 'yazeed.bassam'
AND uop.EntityType IN ('ControllerCertificate', 'EmployeeCertificate')

UNION ALL

SELECT 
    'صلاحيات الشهادات المفعلة للمستخدم yazeed.bassam' as النوع,
    COUNT(*) as العدد
FROM [UserOperationPermissions] uop
INNER JOIN [users] u ON uop.UserId = u.userid
WHERE u.username = 'yazeed.bassam'
AND uop.EntityType IN ('ControllerCertificate', 'EmployeeCertificate')
AND uop.IsAllowed = 1

PRINT ''
PRINT '========================================'
PRINT 'تم الانتهاء من الفحص!'
PRINT '========================================'