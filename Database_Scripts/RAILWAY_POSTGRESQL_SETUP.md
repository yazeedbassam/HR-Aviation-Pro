# 🚀 Railway PostgreSQL Database Setup Guide

## 📋 الخطوات المطلوبة

### 1️⃣ إنشاء قاعدة بيانات PostgreSQL في Railway

1. **اذهب إلى Railway Dashboard**
   - افتح [railway.app](https://railway.app)
   - سجل دخولك إلى حسابك

2. **إنشاء مشروع جديد أو استخدام الموجود**
   - إذا كان لديك مشروع موجود، اختره
   - أو أنشئ مشروع جديد

3. **إضافة قاعدة بيانات PostgreSQL**
   - اضغط على **"+ New"**
   - اختر **"Database"**
   - اختر **"PostgreSQL"**
   - انتظر حتى يتم إنشاء قاعدة البيانات

4. **الحصول على متغيرات البيئة**
   - اضغط على قاعدة البيانات الجديدة
   - اذهب إلى تبويب **"Variables"**
   - سجل القيم التالية:
     - `PGHOST`
     - `PGPORT` 
     - `PGDATABASE`
     - `PGUSER`
     - `PGPASSWORD`

### 2️⃣ تشغيل سكربت إنشاء الجداول

1. **استخدام Railway CLI (الأسهل)**
   ```bash
   # تثبيت Railway CLI
   npm install -g @railway/cli
   
   # تسجيل الدخول
   railway login
   
   # ربط المشروع
   railway link
   
   # تشغيل السكربت
   railway run psql -f Database_Scripts/PostgreSQL_Migration_HR_Aviation.sql
   ```

2. **أو استخدام psql مباشرة**
   ```bash
   # تشغيل السكربت
   psql -h $PGHOST -p $PGPORT -U $PGUSER -d $PGDATABASE -f Database_Scripts/PostgreSQL_Migration_HR_Aviation.sql
   ```

3. **أو استخدام pgAdmin أو أي أداة أخرى**
   - افتح السكربت في أداة إدارة PostgreSQL
   - شغل السكربت كاملاً

### 3️⃣ تحديث متغيرات البيئة في Railway

1. **اذهب إلى مشروع التطبيق في Railway**
2. **اضغط على التطبيق**
3. **اذهب إلى تبويب "Variables"**
4. **أضف/حدث المتغيرات التالية:**

```env
# PostgreSQL Database Variables
PGHOST=your_postgres_host
PGPORT=5432
PGDATABASE=your_database_name
PGUSER=your_username
PGPASSWORD=your_password

# Application Settings
ASPNETCORE_ENVIRONMENT=Production
PORT=8080
```

### 4️⃣ حذف متغيرات Supabase القديمة

**احذف هذه المتغيرات إذا كانت موجودة:**
- `SUPABASE_HOST`
- `SUPABASE_PORT`
- `SUPABASE_DB`
- `SUPABASE_USER`
- `SUPABASE_PASSWORD`

### 5️⃣ إعادة تشغيل التطبيق

1. **في Railway Dashboard**
2. **اذهب إلى التطبيق**
3. **اضغط على "Redeploy" أو "Restart"**

## ✅ التحقق من النجاح

### 1️⃣ فحص الـ Logs
- اذهب إلى تبويب **"Deployments"**
- اضغط على آخر deployment
- ابحث عن هذه الرسائل:
  ```
  ✅ PostgreSQL connection test successful
  🗄️ PGHOST: [your_host]
  🗄️ PGPORT: 5432
  🗄️ PGDATABASE: [your_database]
  🗄️ PGUSER: [your_username]
  🗄️ PGPASSWORD: ***SET***
  ```

### 2️⃣ اختبار تسجيل الدخول
- **Username:** `admin`
- **Password:** `admin123`

### 3️⃣ فحص قاعدة البيانات
```sql
-- التحقق من الجداول
SELECT table_name FROM information_schema.tables WHERE table_schema = 'public';

-- التحقق من المستخدم الإداري
SELECT "UserId", "Username", "RoleName" FROM "Users" WHERE "Username" = 'admin';
```

## 🔧 استكشاف الأخطاء

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

## 📞 الدعم

إذا واجهت أي مشاكل:
1. تحقق من الـ logs في Railway
2. تأكد من صحة متغيرات البيئة
3. تأكد من تشغيل السكربت بنجاح

---

## 🎯 ملخص ما تم إنجازه

✅ **تم إنشاء سكربت PostgreSQL كامل**
- جميع الجداول من SQL Server
- جميع الـ Views والـ Functions
- جميع الـ Indexes
- البيانات الأساسية (المستخدم الإداري)

✅ **تم تحديث التطبيق**
- استخدام PostgreSQL بدلاً من Supabase
- متغيرات البيئة الجديدة
- إزالة المعاملات غير المدعومة

✅ **جاهز للاستخدام**
- قاعدة بيانات نظيفة ومحسنة
- مستخدم إداري جاهز
- جميع الوظائف تعمل

**الآن فقط اتبع الخطوات أعلاه وستكون جاهزاً للعمل! 🚀**