# إزالة التشفير من كلمات المرور - Plain Text Passwords

## 📋 نظرة عامة

تم إزالة جميع آليات التشفير من النظام. الآن كلمات المرور تُخزن كنص عادي في قاعدة البيانات.

## 🔧 التغييرات المطبقة

### 1. SqlServerDb.cs
- ✅ `ValidateCredentials()` - مقارنة نص عادي
- ✅ `CreateUser()` - تخزين نص عادي
- ✅ `HashPassword()` - إرجاع النص كما هو

### 2. PostgreSQLDb.cs  
- ✅ `ValidateCredentials()` - مقارنة نص عادي
- ✅ `CreateUserAsync()` - تخزين نص عادي
- ✅ `HashPassword()` - إرجاع النص كما هو

## 🗄️ سكريبت تحديث قاعدة البيانات

### للمطورين:
استخدم الملف: `Database_Scripts/Update_Admin_Password_Plain_Text.sql`

### للـ SQL Server:
```sql
UPDATE Users 
SET PasswordHash = '123' 
WHERE Username = 'admin';
```

### للـ PostgreSQL (Supabase):
```sql
UPDATE "Users" 
SET "PasswordHash" = '123' 
WHERE "Username" = 'admin';
```

## 🚀 كيفية التطبيق

### 1. تحديث قاعدة البيانات المحلية (SQL Server):
```sql
-- في SQL Server Management Studio
UPDATE Users 
SET PasswordHash = '123' 
WHERE Username = 'admin';
```

### 2. تحديث قاعدة البيانات السحابية (PostgreSQL/Supabase):
```sql
-- في Supabase Dashboard > SQL Editor
UPDATE "Users" 
SET "PasswordHash" = '123' 
WHERE "Username" = 'admin';
```

### 3. التحقق من التحديث:
```sql
-- SQL Server
SELECT Username, PasswordHash, RoleName FROM Users WHERE Username = 'admin';

-- PostgreSQL
SELECT "Username", "PasswordHash", "RoleName" FROM "Users" WHERE "Username" = 'admin';
```

## ✅ النتيجة المتوقعة

بعد تطبيق السكريبت:
- **Username:** `admin`
- **Password:** `123` (نص عادي)
- **Role:** `Admin`

## 🔐 تسجيل الدخول

الآن يمكن تسجيل الدخول باستخدام:
- **Username:** `admin`
- **Password:** `123`
- **Database:** أي من قواعد البيانات (SQL Server أو PostgreSQL)

## ⚠️ تحذيرات أمنية

### ⚠️ تحذير مهم:
- **هذا الحل مخصص للتطوير والاختبار فقط**
- **لا تستخدمه في الإنتاج**
- **تخزين كلمات المرور كنص عادي غير آمن**

### للحماية في الإنتاج:
1. استخدم خوارزميات تشفير قوية (bcrypt, Argon2)
2. أضف Salt عشوائي لكل كلمة مرور
3. استخدم HTTPS دائماً
4. نفذ سياسات كلمات مرور قوية

## 🛠️ استكشاف الأخطاء

### إذا لم يعمل تسجيل الدخول:

1. **تحقق من قاعدة البيانات:**
   ```sql
   SELECT Username, PasswordHash FROM Users WHERE Username = 'admin';
   ```

2. **تأكد من أن PasswordHash = '123'**

3. **تحقق من نوع قاعدة البيانات المختار في صفحة تسجيل الدخول**

4. **راجع logs التطبيق للبحث عن أخطاء**

## 📞 الدعم

إذا واجهت مشاكل:
1. تحقق من logs التطبيق
2. تأكد من اتصال قاعدة البيانات
3. تحقق من صحة السكريبت
4. تأكد من تطبيق التغييرات على قاعدة البيانات الصحيحة