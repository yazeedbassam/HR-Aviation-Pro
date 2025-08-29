# 🔧 إصلاح مشاكل قاعدة البيانات - تعليمات سريعة

## المشاكل الموجودة:
1. **مشاكل قاعدة البيانات**: `Invalid column name 'CategoryName'` و `Invalid column name 'PermissionDescription'`
2. **مشاكل JavaScript**: `DataTable is not a function`

## الحلول المطبقة:

### ✅ 1. إصلاح JavaScript (تم تطبيقه):
- تم إضافة مكتبة DataTables إلى `_Layout.cshtml`
- الآن الجداول ستعمل بشكل صحيح

### 🔄 2. إصلاح قاعدة البيانات (مطلوب تنفيذه):

**الخطوات:**
1. افتح SQL Server Management Studio
2. اتصل بقاعدة البيانات `HR-Aviation`
3. نفذ السكريبت التالي:

```sql
-- =====================================================
-- إصلاح نظام الصلاحيات - تنفيذ فوري
-- =====================================================

USE [HR-Aviation]
GO

-- حذف الكائنات الموجودة
IF OBJECT_ID('vw_UserPermissionsSummary', 'V') IS NOT NULL DROP VIEW vw_UserPermissionsSummary
IF OBJECT_ID('vw_DepartmentPermissions', 'V') IS NOT NULL DROP VIEW vw_DepartmentPermissions
IF OBJECT_ID('GetUserPermissions', 'P') IS NOT NULL DROP PROCEDURE GetUserPermissions
IF OBJECT_ID('GetUserDepartmentPermissions', 'P') IS NOT NULL DROP PROCEDURE GetUserDepartmentPermissions
IF OBJECT_ID('CheckUserPermission', 'P') IS NOT NULL DROP PROCEDURE CheckUserPermission
IF OBJECT_ID('LogPermissionAccess', 'P') IS NOT NULL DROP PROCEDURE LogPermissionAccess
IF OBJECT_ID('PermissionLogs', 'U') IS NOT NULL DROP TABLE PermissionLogs
IF OBJECT_ID('UserDepartmentPermissions', 'U') IS NOT NULL DROP TABLE UserDepartmentPermissions
IF OBJECT_ID('RolePermissions', 'U') IS NOT NULL DROP TABLE RolePermissions
IF OBJECT_ID('Permissions', 'U') IS NOT NULL DROP TABLE Permissions
GO

-- إنشاء جدول الصلاحيات مع الأعمدة الصحيحة
CREATE TABLE [dbo].[Permissions](
    [PermissionId] [int] IDENTITY(1,1) NOT NULL,
    [PermissionName] [nvarchar](100) NOT NULL,
    [PermissionKey] [nvarchar](50) NOT NULL,
    [PermissionDescription] [nvarchar](500) NULL,
    [CategoryName] [nvarchar](50) NOT NULL,
    [IsActive] [bit] NOT NULL DEFAULT 1,
    [CreatedAt] [datetime] NOT NULL DEFAULT GETDATE(),
    [UpdatedAt] [datetime] NULL,
    CONSTRAINT [PK_Permissions] PRIMARY KEY CLUSTERED ([PermissionId] ASC)
)
GO

-- إنشاء باقي الجداول
CREATE TABLE [dbo].[RolePermissions](
    [RolePermissionId] [int] IDENTITY(1,1) NOT NULL,
    [RoleId] [int] NOT NULL,
    [PermissionId] [int] NOT NULL,
    [IsActive] [bit] NOT NULL DEFAULT 1,
    [CreatedAt] [datetime] NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [PK_RolePermissions] PRIMARY KEY CLUSTERED ([RolePermissionId] ASC),
    CONSTRAINT [FK_RolePermissions_Permissions] FOREIGN KEY([PermissionId]) REFERENCES [dbo].[Permissions] ([PermissionId]),
    CONSTRAINT [FK_RolePermissions_ConfigurationValues] FOREIGN KEY([RoleId]) REFERENCES [dbo].[ConfigurationValues] ([ValueId])
)
GO

CREATE TABLE [dbo].[UserDepartmentPermissions](
    [UserDepartmentPermissionId] [int] IDENTITY(1,1) NOT NULL,
    [UserId] [int] NOT NULL,
    [DepartmentId] [int] NOT NULL,
    [PermissionId] [int] NOT NULL,
    [CanView] [bit] NOT NULL DEFAULT 0,
    [CanEdit] [bit] NOT NULL DEFAULT 0,
    [CanDelete] [bit] NOT NULL DEFAULT 0,
    [IsActive] [bit] NOT NULL DEFAULT 1,
    [CreatedAt] [datetime] NOT NULL DEFAULT GETDATE(),
    [UpdatedAt] [datetime] NULL,
    CONSTRAINT [PK_UserDepartmentPermissions] PRIMARY KEY CLUSTERED ([UserDepartmentPermissionId] ASC),
    CONSTRAINT [FK_UserDepartmentPermissions_Permissions] FOREIGN KEY([PermissionId]) REFERENCES [dbo].[Permissions] ([PermissionId]),
    CONSTRAINT [FK_UserDepartmentPermissions_ConfigurationValues] FOREIGN KEY([DepartmentId]) REFERENCES [dbo].[ConfigurationValues] ([ValueId]),
    CONSTRAINT [FK_UserDepartmentPermissions_Users] FOREIGN KEY([UserId]) REFERENCES [dbo].[users] ([userid])
)
GO

CREATE TABLE [dbo].[PermissionLogs](
    [LogId] [int] IDENTITY(1,1) NOT NULL,
    [UserId] [int] NOT NULL,
    [PermissionKey] [nvarchar](50) NOT NULL,
    [DepartmentId] [int] NULL,
    [Action] [nvarchar](50) NOT NULL,
    [Result] [bit] NOT NULL,
    [Timestamp] [datetime] NOT NULL DEFAULT GETDATE(),
    [IPAddress] [nvarchar](45) NULL,
    [UserAgent] [nvarchar](500) NULL,
    CONSTRAINT [PK_PermissionLogs] PRIMARY KEY CLUSTERED ([LogId] ASC),
    CONSTRAINT [FK_PermissionLogs_Users] FOREIGN KEY([UserId]) REFERENCES [dbo].[users] ([userid]),
    CONSTRAINT [FK_PermissionLogs_ConfigurationValues] FOREIGN KEY([DepartmentId]) REFERENCES [dbo].[ConfigurationValues] ([ValueId])
)
GO

-- إدراج الصلاحيات الافتراضية
INSERT INTO [Permissions] ([PermissionName], [PermissionKey], [PermissionDescription], [CategoryName]) VALUES
('View Dashboard', 'DASHBOARD_VIEW', 'Can view the main dashboard', 'Dashboard'),
('Export Dashboard Data', 'DASHBOARD_EXPORT', 'Can export dashboard data', 'Dashboard'),
('View Organization', 'ORGANIZATION_VIEW', 'Can view organization information', 'Organization'),
('Edit Organization', 'ORGANIZATION_EDIT', 'Can edit organization information', 'Organization'),
('View Employees', 'EMPLOYEES_VIEW', 'Can view employee list', 'Staff'),
('Add Employee', 'EMPLOYEES_ADD', 'Can add new employees', 'Staff'),
('Edit Employee', 'EMPLOYEES_EDIT', 'Can edit employee information', 'Staff'),
('Delete Employee', 'EMPLOYEES_DELETE', 'Can delete employees', 'Staff'),
('View Controllers', 'CONTROLLERS_VIEW', 'Can view controller list', 'Staff'),
('Add Controller', 'CONTROLLERS_ADD', 'Can add new controllers', 'Staff'),
('Edit Controller', 'CONTROLLERS_EDIT', 'Can edit controller information', 'Staff'),
('Delete Controller', 'CONTROLLERS_DELETE', 'Can delete controllers', 'Staff'),
('View Licenses', 'LICENSES_VIEW', 'Can view license information', 'Documents'),
('Add License', 'LICENSES_ADD', 'Can add new licenses', 'Documents'),
('Edit License', 'LICENSES_EDIT', 'Can edit license information', 'Documents'),
('Delete License', 'LICENSES_DELETE', 'Can delete licenses', 'Documents'),
('View Certificates', 'CERTIFICATES_VIEW', 'Can view certificate information', 'Documents'),
('Add Certificate', 'CERTIFICATES_ADD', 'Can add new certificates', 'Documents'),
('Edit Certificate', 'CERTIFICATES_EDIT', 'Can edit certificate information', 'Documents'),
('Delete Certificate', 'CERTIFICATES_DELETE', 'Can delete certificates', 'Documents'),
('View Observations', 'OBSERVATIONS_VIEW', 'Can view observations', 'Activities'),
('Add Observation', 'OBSERVATIONS_ADD', 'Can add new observations', 'Activities'),
('Edit Observation', 'OBSERVATIONS_EDIT', 'Can edit observations', 'Activities'),
('Delete Observation', 'OBSERVATIONS_DELETE', 'Can delete observations', 'Activities'),
('View Configuration', 'CONFIGURATION_VIEW', 'Can view system configuration', 'System'),
('Edit Configuration', 'CONFIGURATION_EDIT', 'Can edit system configuration', 'System'),
('View Permissions', 'PERMISSIONS_VIEW', 'Can view permission management', 'System'),
('Manage Permissions', 'PERMISSIONS_MANAGE', 'Can manage user permissions', 'System')
GO

-- إدراج صلاحيات الأدوار الافتراضية
DECLARE @AdminRoleId int, @SupervisorRoleId int, @StaffRoleId int

SELECT @AdminRoleId = ValueId FROM ConfigurationValues WHERE CategoryName = 'Roles' AND ValueText = 'Admin'
SELECT @SupervisorRoleId = ValueId FROM ConfigurationValues WHERE CategoryName = 'Roles' AND ValueText = 'Supervisor'
SELECT @StaffRoleId = ValueId FROM ConfigurationValues WHERE CategoryName = 'Roles' AND ValueText = 'Staff'

-- Admin يحصل على جميع الصلاحيات
INSERT INTO RolePermissions (RoleId, PermissionId)
SELECT @AdminRoleId, PermissionId FROM Permissions WHERE IsActive = 1

-- Supervisor يحصل على معظم الصلاحيات ما عدا إدارة النظام
INSERT INTO RolePermissions (RoleId, PermissionId)
SELECT @SupervisorRoleId, PermissionId FROM Permissions 
WHERE IsActive = 1 AND CategoryName NOT IN ('System')

-- Staff يحصل على صلاحيات المشاهدة الأساسية
INSERT INTO RolePermissions (RoleId, PermissionId)
SELECT @StaffRoleId, PermissionId FROM Permissions 
WHERE IsActive = 1 AND PermissionKey IN (
    'DASHBOARD_VIEW',
    'ORGANIZATION_VIEW',
    'EMPLOYEES_VIEW',
    'CONTROLLERS_VIEW',
    'LICENSES_VIEW',
    'CERTIFICATES_VIEW',
    'OBSERVATIONS_VIEW'
)
GO

PRINT 'تم إصلاح نظام الصلاحيات بنجاح!'
GO
```

## بعد تنفيذ السكريبت:

### ✅ 3. اختبار النظام:
1. اذهب إلى: `http://localhost:5070/Permission`
2. يجب أن تعمل جميع الصفحات بدون أخطاء
3. يمكنك الآن إضافة صلاحيات للمستخدمين

### 🎯 النتيجة المتوقعة:
- ✅ لا توجد أخطاء في قاعدة البيانات
- ✅ الجداول تعمل بشكل صحيح
- ✅ يمكن إضافة وإدارة الصلاحيات
- ✅ النظام جاهز للاستخدام

---

**ملاحظة**: إذا واجهت أي مشاكل، تأكد من:
1. تنفيذ السكريبت بالكامل
2. إعادة تشغيل التطبيق
3. مسح ذاكرة التخزين المؤقت للمتصفح 