# مقارنة أسماء الجداول بين الكود و Supabase

## الجداول الموجودة في Supabase:
✅ `Users` (موجود)
✅ `Controllers` (موجود)
✅ `Countries` (موجود)
✅ `DocumentTypes` (موجود)
✅ `Employees` (موجود)
✅ `Licenses` (موجود)
✅ `Notifications` (موجود)
✅ `Observations` (موجود)
✅ `PermissionLogs` (موجود)
✅ `Permissions` (موجود)
✅ `ProjectDivisions` (موجود)
✅ `ProjectParticipants` (موجود)
✅ `ProjectPhases` (موجود)
✅ `Projects` (موجود)
✅ `RolePermissions` (موجود)
✅ `Roles` (موجود)
✅ `UserActivityLogs` (موجود)
✅ `UserDepartmentPermissions` (موجود)
✅ `UserMenuPermissions` (موجود)
✅ `UserOperationPermissions` (موجود)
✅ `UserOrganizationalPermissions` (موجود)
✅ `vw_UserActivityLogs` (موجود)
✅ `vw_UserPermissionsSummary` (موجود)

## الجداول المستخدمة في الكود:
❌ `controller_users` (غير موجود - يجب استخدام `Users`)
❌ `users` (غير موجود - يجب استخدام `Users`)
❌ `certificates` (غير موجود - يجب استخدام `Certificates`)
❌ `licenses` (غير موجود - يجب استخدام `Licenses`)
❌ `observations` (غير موجود - يجب استخدام `Observations`)
❌ `projects` (غير موجود - يجب استخدام `Projects`)
❌ `notifications` (غير موجود - يجب استخدام `Notifications`)
❌ `employees` (غير موجود - يجب استخدام `Employees`)
❌ `controllers` (غير موجود - يجب استخدام `Controllers`)
❌ `countries` (غير موجود - يجب استخدام `Countries`)
❌ `documenttypes` (غير موجود - يجب استخدام `DocumentTypes`)

## المشاكل المكتشفة:

### 1. مشكلة الحروف الكبيرة/الصغيرة:
- الكود يستخدم: `users`, `controllers`, `employees`
- Supabase يحتوي على: `Users`, `Controllers`, `Employees`

### 2. مشكلة أسماء الجداول المختلفة:
- الكود يستخدم: `controller_users`
- Supabase يحتوي على: `Users`

### 3. مشكلة أسماء الأعمدة:
- الكود يستخدم: `userid`, `username`, `password`
- Supabase يحتوي على: `id`, `Username`, `PasswordHash`

## الحلول المطلوبة:

### 1. إصلاح SupabaseDb.cs:
- تغيير `controller_users` إلى `Users`
- تغيير أسماء الأعمدة لتتطابق مع Supabase
- استخدام علامات الاقتباس المزدوجة للأعمدة

### 2. إصلاح SqlServerDb.cs:
- التأكد من أن أسماء الجداول تتطابق مع Supabase
- استخدام الحروف الكبيرة في أسماء الجداول

### 3. إصلاح Services:
- تحديث جميع الاستعلامات لتستخدم أسماء الجداول الصحيحة
- استخدام أسماء الأعمدة الصحيحة