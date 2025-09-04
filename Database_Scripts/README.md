# 🗄️ HR Aviation Database Migration Scripts

## 📁 محتويات المجلد

### 🚀 الملفات الأساسية
- **`PostgreSQL_Migration_HR_Aviation.sql`** - السكربت الرئيسي لإنشاء قاعدة البيانات كاملة
- **`RAILWAY_POSTGRESQL_SETUP.md`** - دليل خطوة بخطوة لإعداد Railway PostgreSQL
- **`Create_Admin_User.sql`** - سكربت إنشاء المستخدم الإداري فقط

### 📤 تصدير البيانات
- **`Export_Data_From_SQLServer.sql`** - تصدير البيانات من SQL Server
- **`Import_Data_To_PostgreSQL.sql`** - استيراد البيانات إلى PostgreSQL

## 🎯 السيناريوهات المختلفة

### السيناريو 1: قاعدة بيانات جديدة (الأسهل) ⭐
**إذا كنت تريد بداية جديدة:**
1. اتبع `RAILWAY_POSTGRESQL_SETUP.md`
2. شغل `PostgreSQL_Migration_HR_Aviation.sql`
3. استخدم المستخدم الإداري الافتراضي:
   - **Username:** `admin`
   - **Password:** `admin123`

### السيناريو 2: نقل البيانات الموجودة
**إذا كنت تريد نقل البيانات من SQL Server المحلي:**
1. شغل `Export_Data_From_SQLServer.sql` على SQL Server المحلي
2. احفظ النتائج كملفات CSV
3. اتبع `RAILWAY_POSTGRESQL_SETUP.md` لإنشاء قاعدة البيانات
4. شغل `Import_Data_To_PostgreSQL.sql` لاستيراد البيانات

### السيناريو 3: إضافة مستخدم إداري فقط
**إذا كانت قاعدة البيانات موجودة وتريد إضافة مستخدم إداري:**
1. شغل `Create_Admin_User.sql`

## 🔧 المتطلبات

### للسيناريو 1 (الأسهل):
- حساب Railway
- Railway CLI (اختياري)
- psql أو أي أداة إدارة PostgreSQL

### للسيناريو 2 (نقل البيانات):
- SQL Server Management Studio أو أي أداة للوصول لـ SQL Server
- Railway CLI أو psql
- ملفات CSV للبيانات

## 📋 خطوات سريعة

### 1️⃣ إنشاء قاعدة بيانات في Railway
```bash
# تثبيت Railway CLI
npm install -g @railway/cli

# تسجيل الدخول
railway login

# إنشاء مشروع جديد
railway new

# إضافة قاعدة بيانات PostgreSQL
railway add postgresql
```

### 2️⃣ تشغيل السكربت
```bash
# ربط المشروع
railway link

# تشغيل السكربت الرئيسي
railway run psql -f Database_Scripts/PostgreSQL_Migration_HR_Aviation.sql
```

### 3️⃣ تحديث متغيرات البيئة
في Railway Dashboard، أضف:
```env
PGHOST=your_postgres_host
PGPORT=5432
PGDATABASE=your_database_name
PGUSER=your_username
PGPASSWORD=your_password
```

### 4️⃣ إعادة تشغيل التطبيق
- اذهب إلى Railway Dashboard
- اضغط على "Redeploy"

## ✅ التحقق من النجاح

### فحص الـ Logs
ابحث عن هذه الرسائل في Railway logs:
```
✅ PostgreSQL connection test successful
🗄️ PGHOST: [your_host]
🗄️ PGPORT: 5432
🗄️ PGDATABASE: [your_database]
🗄️ PGUSER: [your_username]
🗄️ PGPASSWORD: ***SET***
```

### اختبار تسجيل الدخول
- **Username:** `admin`
- **Password:** `admin123`

### فحص قاعدة البيانات
```sql
-- التحقق من الجداول
SELECT table_name FROM information_schema.tables WHERE table_schema = 'public';

-- التحقق من المستخدم الإداري
SELECT "UserId", "Username", "RoleName" FROM "Users" WHERE "Username" = 'admin';
```

## 🚨 استكشاف الأخطاء

### مشكلة: "Connection failed"
**الحل:**
1. تأكد من صحة متغيرات البيئة
2. تأكد من أن قاعدة البيانات تعمل
3. تحقق من الـ logs

### مشكلة: "Table doesn't exist"
**الحل:**
1. تأكد من تشغيل السكربت بنجاح
2. تحقق من أسماء الجداول (PostgreSQL حساس للأحرف الكبيرة)

### مشكلة: "Login failed"
**الحل:**
1. تأكد من وجود المستخدم الإداري
2. جرب إعادة تشغيل التطبيق

## 📊 ما تم تحويله من SQL Server

### ✅ الجداول (25 جدول)
- Users, Permissions, Roles
- Controllers, Employees, Airports, Countries
- Certificates, Licenses, Observations
- Projects, Notifications
- Configuration tables
- Permission management tables
- Activity logging tables

### ✅ الـ Views (2 view)
- vw_UserPermissionsSummary
- vw_UserActivityLogs

### ✅ الـ Functions (8 functions)
- CanUserPerformOperation
- CanUserViewMenu
- CheckUserOperationPermission
- CheckUserPermission
- GetAllUsersWithPermissions
- GetUserDepartmentPermissions
- GetUserMenuPermissions
- GetUserPermissions
- fn_GetUserActivitySummary
- sp_InsertUserActivityLog

### ✅ الـ Indexes (15 index)
- جميع الـ indexes المهمة للأداء

### ✅ البيانات الأساسية
- المستخدم الإداري (admin/admin123)
- الأدوار الأساسية (Admin, User, Manager)
- فئات التكوين الأساسية
- أنواع المستندات الأساسية

## 🎉 النتيجة النهائية

بعد اتباع هذه الخطوات ستحصل على:
- ✅ قاعدة بيانات PostgreSQL كاملة في Railway
- ✅ جميع الجداول والوظائف
- ✅ مستخدم إداري جاهز
- ✅ تطبيق يعمل بدون مشاكل
- ✅ لا مزيد من مشاكل Supabase!

## 📞 الدعم

إذا واجهت أي مشاكل:
1. تحقق من الـ logs في Railway
2. تأكد من صحة متغيرات البيئة
3. تأكد من تشغيل السكربت بنجاح
4. راجع دليل `RAILWAY_POSTGRESQL_SETUP.md` للتفاصيل

---

**🚀 جاهز للانطلاق! اتبع الخطوات وستكون لديك قاعدة بيانات PostgreSQL تعمل بشكل مثالي مع Railway.**