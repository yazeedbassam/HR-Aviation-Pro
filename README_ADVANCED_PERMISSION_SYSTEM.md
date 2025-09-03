# نظام الصلاحيات المتقدم - AVIATION HR PRO

## نظرة عامة

تم إنشاء نظام صلاحيات متقدم ومفصل يحل محل الأنظمة القديمة (الصلاحيات المبسطة والمتقدمة وإدارة الأدوار القديمة). يوفر هذا النظام تحكماً دقيقاً في صلاحيات المستخدمين على ثلاثة مستويات:

1. **صلاحيات القائمة الجانبية** - التحكم في العناصر المرئية في القائمة
2. **صلاحيات العمليات** - التحكم في العمليات (عرض، إضافة، تعديل، حذف، تصدير)
3. **صلاحيات الهيكل التنظيمي** - التحكم في الوصول حسب الدول والمطارات والأقسام

## الميزات الجديدة

### ✅ صلاحيات القائمة الجانبية
- إمكانية إخفاء/إظهار أي عنصر في القائمة الجانبية
- تحكم دقيق في واجهة المستخدم
- دعم للعناصر: الملف الشخصي، الإشعارات، لوحة التحكم، الموظفين، المراقبين، الرخص، الشهادات، الملاحظات، الإعدادات، الصلاحيات

### ✅ صلاحيات العمليات التفصيلية
- **الموظفين**: عرض، إضافة، تعديل، حذف، تصدير
- **المراقبين**: عرض، إضافة، تعديل، حذف، تصدير
- **الرخص**: عرض، إضافة، تعديل، حذف، تصدير
- **الشهادات**: عرض، إضافة، تعديل، حذف، تصدير
- **الملاحظات**: عرض، إضافة، تعديل، حذف، تصدير

### ✅ صلاحيات الهيكل التنظيمي
- **الدول**: التحكم في الوصول حسب الدولة
- **المطارات**: التحكم في الوصول حسب المطار
- **الأقسام**: التحكم في الوصول حسب القسم
- **العمليات**: عرض، تعديل، حذف، إنشاء

### ✅ قوالب الصلاحيات
- **موظف عادي**: صلاحيات محدودة - عرض فقط
- **مدير قسم**: صلاحيات متوسطة - إدارة القسم
- **مدير عام**: صلاحيات كاملة - إدارة النظام

## الملفات الجديدة

### قاعدة البيانات
- `Database_Scripts/Advanced_Permission_System_New.sql` - سكريبت إنشاء النظام الجديد

### الخدمات
- `Services/AdvancedPermissionManagerService.cs` - الخدمة الرئيسية لإدارة الصلاحيات

### الكونترولرز
- `Controllers/PermissionManagerController.cs` - كونترولر إدارة الصلاحيات

### الواجهات
- `Views/PermissionManager/Index.cshtml` - الصفحة الرئيسية لإدارة الصلاحيات
- `Views/PermissionManager/UserPermissions.cshtml` - صفحة إدارة صلاحيات المستخدم الفردي
- `Views/PermissionManager/PermissionTemplates.cshtml` - صفحة قوالب الصلاحيات

## الجداول الجديدة

### 1. UserMenuPermissions
```sql
CREATE TABLE [dbo].[UserMenuPermissions](
    [UserMenuPermissionId] [int] IDENTITY(1,1) NOT NULL,
    [UserId] [int] NOT NULL,
    [MenuKey] [nvarchar](50) NOT NULL,
    [IsVisible] [bit] NOT NULL DEFAULT 1,
    [IsActive] [bit] NOT NULL DEFAULT 1,
    [CreatedAt] [datetime] NOT NULL DEFAULT GETDATE(),
    [UpdatedAt] [datetime] NULL
)
```

### 2. UserOrganizationalPermissions
```sql
CREATE TABLE [dbo].[UserOrganizationalPermissions](
    [UserOrganizationalPermissionId] [int] IDENTITY(1,1) NOT NULL,
    [UserId] [int] NOT NULL,
    [PermissionType] [nvarchar](50) NOT NULL,
    [EntityId] [int] NOT NULL,
    [EntityName] [nvarchar](100) NOT NULL,
    [CanView] [bit] NOT NULL DEFAULT 1,
    [CanEdit] [bit] NOT NULL DEFAULT 0,
    [CanDelete] [bit] NOT NULL DEFAULT 0,
    [CanCreate] [bit] NOT NULL DEFAULT 0,
    [IsActive] [bit] NOT NULL DEFAULT 1,
    [CreatedAt] [datetime] NOT NULL DEFAULT GETDATE(),
    [UpdatedAt] [datetime] NULL
)
```

### 3. UserOperationPermissions
```sql
CREATE TABLE [dbo].[UserOperationPermissions](
    [UserOperationPermissionId] [int] IDENTITY(1,1) NOT NULL,
    [UserId] [int] NOT NULL,
    [PermissionId] [int] NOT NULL,
    [EntityType] [nvarchar](50) NOT NULL,
    [OperationType] [nvarchar](50) NOT NULL,
    [IsAllowed] [bit] NOT NULL DEFAULT 1,
    [Scope] [nvarchar](50) NOT NULL DEFAULT 'All',
    [ScopeId] [int] NULL,
    [IsActive] [bit] NOT NULL DEFAULT 1,
    [CreatedAt] [datetime] NOT NULL DEFAULT GETDATE(),
    [UpdatedAt] [datetime] NULL
)
```

## الإجراءات المخزنة الجديدة

### 1. GetUserMenuPermissions
```sql
CREATE PROCEDURE [dbo].[GetUserMenuPermissions]
    @UserId int
AS
BEGIN
    SELECT MenuKey, IsVisible
    FROM UserMenuPermissions
    WHERE UserId = @UserId AND IsActive = 1
END
```

### 2. CanUserViewMenu
```sql
CREATE PROCEDURE [dbo].[CanUserViewMenu]
    @UserId int,
    @MenuKey nvarchar(50)
AS
BEGIN
    -- التحقق من صلاحيات القائمة
END
```

### 3. CheckUserOperationPermission
```sql
CREATE PROCEDURE [dbo].[CheckUserOperationPermission]
    @UserId int,
    @EntityType nvarchar(50),
    @OperationType nvarchar(50),
    @ScopeId int = NULL
AS
BEGIN
    -- التحقق من صلاحيات العمليات
END
```

## كيفية الاستخدام

### 1. تشغيل سكريبت قاعدة البيانات
```sql
-- تشغيل السكريبت في SQL Server Management Studio
-- أو استخدام أي أداة إدارة قاعدة بيانات أخرى
```

### 2. الوصول لإدارة الصلاحيات
- تسجيل الدخول كمدير
- الانتقال إلى "System Settings" > "إدارة الصلاحيات المتقدمة"

### 3. إدارة صلاحيات المستخدم
1. اختيار المستخدم من القائمة
2. تحديد صلاحيات القائمة الجانبية
3. تحديد صلاحيات العمليات
4. تحديد صلاحيات الهيكل التنظيمي
5. حفظ التغييرات

### 4. استخدام قوالب الصلاحيات
1. الانتقال إلى "قوالب الصلاحيات"
2. اختيار القالب المناسب
3. تطبيقه على المستخدم

## أمثلة عملية

### مثال 1: موظف عادي
```csharp
// صلاحيات القائمة
- الملف الشخصي: ✓
- الإشعارات: ✓
- باقي العناصر: ✗

// صلاحيات العمليات
- عرض الموظفين: ✓ (خاص فقط)
- عرض الرخص: ✓ (خاص فقط)
- باقي العمليات: ✗
```

### مثال 2: مدير قسم
```csharp
// صلاحيات القائمة
- جميع عناصر القائمة عدا الإعدادات والصلاحيات: ✓

// صلاحيات العمليات
- عرض الموظفين: ✓ (القسم)
- إضافة الموظفين: ✓ (القسم)
- تعديل الموظفين: ✓ (القسم)
- حذف الموظفين: ✗
```

### مثال 3: مدير عام
```csharp
// صلاحيات القائمة
- جميع عناصر القائمة: ✓

// صلاحيات العمليات
- جميع العمليات على جميع الأنواع: ✓ (الكل)
```

## التكامل مع النظام الحالي

### 1. تحديث Layout
تم تحديث `Views/Shared/_Layout.cshtml` لاستخدام النظام الجديد:
```csharp
@inject IAdvancedPermissionManagerService PermissionManagerService

// التحقق من صلاحيات القائمة
var canViewDashboard = await PermissionManagerService.CanViewMenuAsync(currentUserId, "DASHBOARD");
var canViewEmployees = await PermissionManagerService.CanViewMenuAsync(currentUserId, "EMPLOYEES");
// ... إلخ
```

### 2. تحديث Program.cs
```csharp
// إضافة الخدمة الجديدة
builder.Services.AddScoped<IAdvancedPermissionManagerService, AdvancedPermissionManagerService>();
```

### 3. دعم النظام القديم
النظام الجديد يدعم النظام القديم من خلال:
```csharp
// دعم للصلاحيات القديمة
public async Task<bool> HasPermissionAsync(int userId, string permissionKey)
{
    // التحقق من النظام الجديد أولاً
    // ثم النظام القديم كبديل
}
```

## الأمان والأداء

### 1. التخزين المؤقت
- استخدام Memory Cache لتحسين الأداء
- مدة التخزين: 10 دقائق
- مسح التخزين عند تحديث الصلاحيات

### 2. الفهرسة
- فهارس على UserId في جميع الجداول
- فهارس على MenuKey و EntityType
- تحسين استعلامات قاعدة البيانات

### 3. السجلات
- تسجيل جميع العمليات
- تتبع التغييرات
- معلومات IP و User Agent

## استكشاف الأخطاء

### 1. مشاكل الأداء
- التحقق من الفهارس
- مراجعة استعلامات قاعدة البيانات
- تنظيف التخزين المؤقت

### 2. مشاكل الصلاحيات
- التحقق من البيانات في الجداول الجديدة
- مراجعة سجلات النظام
- اختبار الصلاحيات يدوياً

### 3. مشاكل الواجهة
- التحقق من JavaScript
- مراجعة CSS
- اختبار المتصفحات المختلفة

## التطوير المستقبلي

### 1. ميزات مقترحة
- صلاحيات زمنية (مؤقتة)
- صلاحيات جغرافية
- صلاحيات متقدمة للتقارير
- واجهة API للصلاحيات

### 2. تحسينات الأداء
- تحسين الاستعلامات
- إضافة المزيد من الفهارس
- تحسين التخزين المؤقت

### 3. تحسينات الأمان
- تشفير البيانات الحساسة
- مراجعة دورية للصلاحيات
- تنبيهات الأمان

## الدعم والمساعدة

للحصول على المساعدة:
1. مراجعة هذا الدليل
2. فحص سجلات النظام
3. اختبار الصلاحيات يدوياً
4. التواصل مع فريق التطوير

---

**تم إنشاء هذا النظام لتحسين إدارة الصلاحيات وتوفير تحكم دقيق في وصول المستخدمين إلى مختلف أجزاء النظام.**