# نظام إدارة الصلاحيات - ANSMS Pro

## نظرة عامة

تم تطوير نظام إدارة الصلاحيات الشامل لتوفير تحكم دقيق في الوصول إلى مختلف أجزاء النظام بناءً على الأدوار والمستخدمين والأقسام.

## الميزات الرئيسية

### 1. إدارة الصلاحيات على مستوى الأدوار
- إضافة وإزالة صلاحيات من الأدوار المختلفة
- دعم الأدوار: Administrator, Manager, Supervisor, Controller, Employee
- واجهة سهلة الاستخدام لإدارة الصلاحيات

### 2. إدارة الصلاحيات على مستوى المستخدمين والأقسام
- منح صلاحيات محددة للمستخدمين على أقسام معينة
- تحكم دقيق في الصلاحيات (عرض، تعديل، حذف)
- إمكانية تفعيل/إلغاء تفعيل الصلاحيات

### 3. نظام تسجيل شامل
- تسجيل جميع محاولات الوصول للصلاحيات
- تتبع IP Address و User Agent
- إمكانية تصدير السجلات

### 4. واجهة إدارية متقدمة
- عرض ملخص شامل لصلاحيات كل مستخدم
- إحصائيات مفصلة
- تصفية وبحث متقدم

## هيكل قاعدة البيانات

### الجداول الرئيسية

#### 1. Permissions
```sql
CREATE TABLE [dbo].[Permissions](
    [PermissionId] [int] IDENTITY(1,1) NOT NULL,
    [PermissionName] [nvarchar](100) NOT NULL,
    [PermissionKey] [nvarchar](50) NOT NULL,
    [Description] [nvarchar](500) NULL,
    [IsActive] [bit] NOT NULL DEFAULT 1,
    [CreatedAt] [datetime] NOT NULL DEFAULT GETDATE(),
    [UpdatedAt] [datetime] NULL
)
```

#### 2. RolePermissions
```sql
CREATE TABLE [dbo].[RolePermissions](
    [RolePermissionId] [int] IDENTITY(1,1) NOT NULL,
    [RoleId] [int] NOT NULL,
    [PermissionId] [int] NOT NULL,
    [IsActive] [bit] NOT NULL DEFAULT 1,
    [CreatedAt] [datetime] NOT NULL DEFAULT GETDATE()
)
```

#### 3. UserDepartmentPermissions
```sql
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
    [UpdatedAt] [datetime] NULL
)
```

#### 4. PermissionLogs
```sql
CREATE TABLE [dbo].[PermissionLogs](
    [LogId] [int] IDENTITY(1,1) NOT NULL,
    [UserId] [int] NOT NULL,
    [PermissionId] [int] NULL,
    [DepartmentId] [int] NULL,
    [Status] [nvarchar](20) NOT NULL,
    [Details] [nvarchar](500) NULL,
    [IpAddress] [nvarchar](45) NULL,
    [UserAgent] [nvarchar](500) NULL,
    [Timestamp] [datetime] NOT NULL DEFAULT GETDATE()
)
```

## الصلاحيات المتاحة

### صلاحيات لوحة التحكم
- `DASHBOARD_VIEW` - عرض لوحة التحكم
- `DASHBOARD_ANALYTICS` - عرض التحليلات

### صلاحيات المراقبين
- `CONTROLLERS_VIEW` - عرض المراقبين
- `CONTROLLERS_CREATE` - إنشاء مراقب جديد
- `CONTROLLERS_EDIT` - تعديل المراقبين
- `CONTROLLERS_DELETE` - حذف المراقبين

### صلاحيات الموظفين
- `EMPLOYEES_VIEW` - عرض الموظفين
- `EMPLOYEES_CREATE` - إنشاء موظف جديد
- `EMPLOYEES_EDIT` - تعديل الموظفين
- `EMPLOYEES_DELETE` - حذف الموظفين

### صلاحيات الرخص
- `LICENSES_VIEW` - عرض الرخص
- `LICENSES_CREATE` - إنشاء رخصة جديدة
- `LICENSES_EDIT` - تعديل الرخص
- `LICENSES_DELETE` - حذف الرخص

### صلاحيات الشهادات
- `CERTIFICATES_VIEW` - عرض الشهادات
- `CERTIFICATES_CREATE` - إنشاء شهادة جديدة
- `CERTIFICATES_EDIT` - تعديل الشهادات
- `CERTIFICATES_DELETE` - حذف الشهادات

### صلاحيات الملاحظات
- `OBSERVATIONS_VIEW` - عرض الملاحظات
- `OBSERVATIONS_CREATE` - إنشاء ملاحظة جديدة
- `OBSERVATIONS_EDIT` - تعديل الملاحظات
- `OBSERVATIONS_DELETE` - حذف الملاحظات

### صلاحيات النظام
- `SYSTEM_SETTINGS_VIEW` - عرض إعدادات النظام
- `SYSTEM_SETTINGS_EDIT` - تعديل إعدادات النظام
- `PERMISSIONS_MANAGE` - إدارة الصلاحيات
- `USERS_MANAGE` - إدارة المستخدمين

## كيفية الاستخدام

### 1. الوصول إلى نظام الصلاحيات
1. سجل دخولك كمدير النظام
2. اذهب إلى "System Settings" في القائمة الجانبية
3. اختر "Roles Management"

### 2. إدارة صلاحيات الأدوار
1. في صفحة "Role Permissions Management"
2. اضغط على "Add Role Permission"
3. اختر الدور والصلاحية
4. اضغط "Add Permission"

### 3. إدارة صلاحيات المستخدمين
1. اذهب إلى "User Department Permissions"
2. اضغط على "Add User Permission"
3. اختر المستخدم والقسم والصلاحية
4. حدد الصلاحيات المطلوبة (عرض، تعديل، حذف)
5. اضغط "Create Permission"

### 4. عرض سجلات الصلاحيات
1. اذهب إلى "Logs" في قائمة الصلاحيات
2. استخدم الفلاتر للبحث في السجلات
3. يمكن تصدير السجلات بصيغة Excel

### 5. عرض ملخص صلاحيات المستخدم
1. اذهب إلى "User Summary"
2. اختر المستخدم لعرض ملخص صلاحياته
3. ستجد إحصائيات مفصلة عن الصلاحيات والأقسام

## الملفات الرئيسية

### Controllers
- `PermissionController.cs` - التحكم في واجهة الصلاحيات

### Services
- `PermissionService.cs` - منطق إدارة الصلاحيات
- `IPermissionService.cs` - واجهة خدمة الصلاحيات

### Models
- `PermissionModels.cs` - نماذج البيانات للصلاحيات

### Attributes
- `RequirePermissionAttribute.cs` - خاصية التحقق من الصلاحيات

### Views
- `Views/Permission/Index.cshtml` - الصفحة الرئيسية
- `Views/Permission/RolePermissions.cshtml` - إدارة صلاحيات الأدوار
- `Views/Permission/UserDepartmentPermissions.cshtml` - إدارة صلاحيات المستخدمين
- `Views/Permission/CreateUserDepartmentPermission.cshtml` - إنشاء صلاحية جديدة
- `Views/Permission/EditUserDepartmentPermission.cshtml` - تعديل الصلاحيات
- `Views/Permission/Logs.cshtml` - عرض السجلات
- `Views/Permission/UserSummary.cshtml` - ملخص صلاحيات المستخدم

## إضافة صلاحيات جديدة

### 1. إضافة الصلاحية في قاعدة البيانات
```sql
INSERT INTO Permissions (PermissionName, PermissionKey, Description)
VALUES ('New Permission', 'NEW_PERMISSION_KEY', 'Description of the new permission');
```

### 2. إضافة الصلاحية في الكود
```csharp
// في PermissionKeys.cs
public static class PermissionKeys
{
    public const string NewPermission = "NEW_PERMISSION_KEY";
}

// في PermissionCategories.cs
public static class PermissionCategories
{
    public static readonly Dictionary<string, string> Categories = new()
    {
        { PermissionKeys.NewPermission, "New Category" }
    };
}
```

### 3. استخدام الصلاحية في Controller
```csharp
[RequirePermission(PermissionKeys.NewPermission)]
public IActionResult NewAction()
{
    // Your action code
}
```

## الأمان والتحقق

### 1. التحقق من الصلاحيات
```csharp
// في Controller
public async Task<IActionResult> SomeAction()
{
    var hasPermission = await _permissionService.HasPermissionAsync(userId, "PERMISSION_KEY");
    if (!hasPermission)
    {
        return Forbid();
    }
    // Continue with action
}
```

### 2. التحقق من صلاحيات القسم
```csharp
// التحقق من صلاحية على قسم معين
var hasPermission = await _permissionService.HasPermissionAsync(userId, "PERMISSION_KEY", departmentId);
```

### 3. الحصول على الأقسام المتاحة للمستخدم
```csharp
var accessibleDepartments = await _permissionService.GetUserAccessibleDepartmentsAsync(userId);
```

## أفضل الممارسات

### 1. تسمية الصلاحيات
- استخدم أسماء واضحة ووصفية
- اتبع نمط `MODULE_ACTION` (مثل: `EMPLOYEES_VIEW`)
- استخدم أحرف كبيرة فقط

### 2. تنظيم الصلاحيات
- قم بتجميع الصلاحيات في فئات منطقية
- استخدم وصف واضح لكل صلاحية
- حافظ على تناسق في التسمية

### 3. إدارة الصلاحيات
- راجع الصلاحيات بانتظام
- احذف الصلاحيات غير المستخدمة
- وثق أي تغييرات في الصلاحيات

### 4. المراقبة والتسجيل
- راقب سجلات الوصول بانتظام
- تحقق من محاولات الوصول المرفوضة
- استخدم التقارير لتحديد أنماط الاستخدام

## استكشاف الأخطاء

### مشاكل شائعة

#### 1. المستخدم لا يستطيع الوصول لصفحة معينة
- تحقق من وجود الصلاحية المطلوبة
- تأكد من أن الصلاحية مفعلة
- تحقق من صلاحيات القسم إذا كانت مطلوبة

#### 2. خطأ في قاعدة البيانات
- تحقق من وجود جميع الجداول المطلوبة
- تأكد من صحة العلاقات بين الجداول
- تحقق من وجود البيانات الأساسية

#### 3. مشاكل في الأداء
- تحقق من فهارس قاعدة البيانات
- راقب استخدام الذاكرة المؤقتة
- تحقق من استعلامات قاعدة البيانات

## الدعم والمساعدة

للمساعدة في استخدام نظام الصلاحيات أو الإبلاغ عن مشاكل، يرجى التواصل مع فريق التطوير.

---

**ملاحظة**: هذا النظام مصمم لتوفير أمان عالي ومرونة في إدارة الصلاحيات. يرجى اختباره جيداً قبل استخدامه في البيئة الإنتاجية. 