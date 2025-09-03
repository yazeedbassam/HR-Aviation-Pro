-- =====================================================
-- إضافة صلاحيات الملاحظات المنفصلة
-- =====================================================

USE [HR-Aviation]
GO

PRINT '========================================'
PRINT 'إضافة صلاحيات الملاحظات المنفصلة'
PRINT '========================================'

-- تنظيف الصلاحيات المكررة إن وجدت
PRINT '🧹 تنظيف الصلاحيات المكررة...'
DELETE FROM [UserOperationPermissions] 
WHERE EntityType IN ('ControllerObservation', 'EmployeeObservation')
AND PermissionId IS NULL

-- إضافة صلاحيات ملاحظات المراقبين
PRINT '📋 إضافة صلاحيات ملاحظات المراقبين...'
INSERT INTO [Permissions] (PermissionKey, PermissionName, PermissionDescription, CategoryName, IsActive)
VALUES 
    ('CONTROLLEROBSERVATION_VIEW', 'View Controller Observations', 'View controller observation records', 'Controller Observation Management', 1),
    ('CONTROLLEROBSERVATION_ADD', 'Add Controller Observations', 'Add new controller observation records', 'Controller Observation Management', 1),
    ('CONTROLLEROBSERVATION_EDIT', 'Edit Controller Observations', 'Edit existing controller observation records', 'Controller Observation Management', 1),
    ('CONTROLLEROBSERVATION_DELETE', 'Delete Controller Observations', 'Delete controller observation records', 'Controller Observation Management', 1),
    ('CONTROLLEROBSERVATION_EXPORT', 'Export Controller Observations', 'Export controller observation data', 'Controller Observation Management', 1)

-- إضافة صلاحيات ملاحظات الموظفين
PRINT '📋 إضافة صلاحيات ملاحظات الموظفين...'
INSERT INTO [Permissions] (PermissionKey, PermissionName, PermissionDescription, CategoryName, IsActive)
VALUES 
    ('EMPLOYEEOBSERVATION_VIEW', 'View Employee Observations', 'View employee observation records', 'Employee Observation Management', 1),
    ('EMPLOYEEOBSERVATION_ADD', 'Add Employee Observations', 'Add new employee observation records', 'Employee Observation Management', 1),
    ('EMPLOYEEOBSERVATION_EDIT', 'Edit Employee Observations', 'Edit existing employee observation records', 'Employee Observation Management', 1),
    ('EMPLOYEEOBSERVATION_DELETE', 'Delete Employee Observations', 'Delete employee observation records', 'Employee Observation Management', 1),
    ('EMPLOYEEOBSERVATION_EXPORT', 'Export Employee Observations', 'Export employee observation data', 'Employee Observation Management', 1)

-- إضافة صلاحيات الملاحظات للمستخدمين الموجودين
PRINT '👥 إضافة صلاحيات الملاحظات للمستخدمين...'
INSERT INTO [UserOperationPermissions] (UserId, EntityType, OperationType, IsAllowed, Scope, ScopeId, IsActive, PermissionId)
SELECT 
    u.userid,
    'ControllerObservation',
    'View',
    0, -- غير مفعل افتراضياً
    'All',
    NULL,
    1,
    p.PermissionId
FROM [users] u
CROSS JOIN [Permissions] p
WHERE p.PermissionKey = 'CONTROLLEROBSERVATION_VIEW'
AND u.rolename = 'Controller'

UNION ALL

SELECT 
    u.userid,
    'ControllerObservation',
    'Add',
    0,
    'All',
    NULL,
    1,
    p.PermissionId
FROM [users] u
CROSS JOIN [Permissions] p
WHERE p.PermissionKey = 'CONTROLLEROBSERVATION_ADD'
AND u.rolename = 'Controller'

UNION ALL

SELECT 
    u.userid,
    'ControllerObservation',
    'Edit',
    0,
    'All',
    NULL,
    1,
    p.PermissionId
FROM [users] u
CROSS JOIN [Permissions] p
WHERE p.PermissionKey = 'CONTROLLEROBSERVATION_EDIT'
AND u.rolename = 'Controller'

UNION ALL

SELECT 
    u.userid,
    'ControllerObservation',
    'Delete',
    0,
    'All',
    NULL,
    1,
    p.PermissionId
FROM [users] u
CROSS JOIN [Permissions] p
WHERE p.PermissionKey = 'CONTROLLEROBSERVATION_DELETE'
AND u.rolename = 'Controller'

UNION ALL

SELECT 
    u.userid,
    'ControllerObservation',
    'Export',
    0,
    'All',
    NULL,
    1,
    p.PermissionId
FROM [users] u
CROSS JOIN [Permissions] p
WHERE p.PermissionKey = 'CONTROLLEROBSERVATION_EXPORT'
AND u.rolename = 'Controller'

UNION ALL

SELECT 
    u.userid,
    'EmployeeObservation',
    'View',
    0,
    'All',
    NULL,
    1,
    p.PermissionId
FROM [users] u
CROSS JOIN [Permissions] p
WHERE p.PermissionKey = 'EMPLOYEEOBSERVATION_VIEW'
AND u.rolename = 'Controller'

UNION ALL

SELECT 
    u.userid,
    'EmployeeObservation',
    'Add',
    0,
    'All',
    NULL,
    1,
    p.PermissionId
FROM [users] u
CROSS JOIN [Permissions] p
WHERE p.PermissionKey = 'EMPLOYEEOBSERVATION_ADD'
AND u.rolename = 'Controller'

UNION ALL

SELECT 
    u.userid,
    'EmployeeObservation',
    'Edit',
    0,
    'All',
    NULL,
    1,
    p.PermissionId
FROM [users] u
CROSS JOIN [Permissions] p
WHERE p.PermissionKey = 'EMPLOYEEOBSERVATION_EDIT'
AND u.rolename = 'Controller'

UNION ALL

SELECT 
    u.userid,
    'EmployeeObservation',
    'Delete',
    0,
    'All',
    NULL,
    1,
    p.PermissionId
FROM [users] u
CROSS JOIN [Permissions] p
WHERE p.PermissionKey = 'EMPLOYEEOBSERVATION_DELETE'
AND u.rolename = 'Controller'

UNION ALL

SELECT 
    u.userid,
    'EmployeeObservation',
    'Export',
    0,
    'All',
    NULL,
    1,
    p.PermissionId
FROM [users] u
CROSS JOIN [Permissions] p
WHERE p.PermissionKey = 'EMPLOYEEOBSERVATION_EXPORT'
AND u.rolename = 'Controller'

-- التحقق من النتائج
PRINT '✅ التحقق من النتائج...'
SELECT 
    'صلاحيات ملاحظات المراقبين' as النوع,
    COUNT(*) as العدد
FROM [Permissions]
WHERE PermissionKey LIKE 'CONTROLLEROBSERVATION_%'

UNION ALL

SELECT 
    'صلاحيات ملاحظات الموظفين' as النوع,
    COUNT(*) as العدد
FROM [Permissions]
WHERE PermissionKey LIKE 'EMPLOYEEOBSERVATION_%'

UNION ALL

SELECT 
    'صلاحيات ملاحظات المراقبين للمستخدمين' as النوع,
    COUNT(*) as العدد
FROM [UserOperationPermissions]
WHERE EntityType = 'ControllerObservation'

UNION ALL

SELECT 
    'صلاحيات ملاحظات الموظفين للمستخدمين' as النوع,
    COUNT(*) as العدد
FROM [UserOperationPermissions]
WHERE EntityType = 'EmployeeObservation'

PRINT ''
PRINT '========================================'
PRINT '✅ تم إضافة صلاحيات الملاحظات المنفصلة بنجاح!'
PRINT '📝 ملاحظة: الآن يمكن إدارة صلاحيات ملاحظات المراقبين والموظفين بشكل منفصل'
PRINT '========================================'