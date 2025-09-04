# Supabase Setup Instructions

## المشكلة
النظام يبحث عن جدول `controller_users` في Supabase لكن هذا الجدول غير موجود.

## الحل
1. افتح Supabase Dashboard
2. اذهب إلى SQL Editor
3. انسخ والصق الكود التالي:

```sql
-- Create controller_users table
CREATE TABLE IF NOT EXISTS controller_users (
    userid SERIAL PRIMARY KEY,
    username VARCHAR(100) UNIQUE NOT NULL,
    password VARCHAR(255) NOT NULL,
    fullname VARCHAR(255) NOT NULL,
    email VARCHAR(255),
    role VARCHAR(50) NOT NULL DEFAULT 'Controller',
    department VARCHAR(100),
    isactive BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    last_login TIMESTAMP NULL
);

-- Create admin user with hashed password (password: admin123)
INSERT INTO controller_users (username, password, fullname, email, role, department, isactive, created_at) 
VALUES ('admin', '$2a$11$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 'System Administrator', 'admin@aviation.com', 'Admin', 'IT', true, CURRENT_TIMESTAMP)
ON CONFLICT (username) DO UPDATE SET 
    password = EXCLUDED.password,
    fullname = EXCLUDED.fullname,
    email = EXCLUDED.email,
    role = EXCLUDED.role,
    department = EXCLUDED.department,
    isactive = EXCLUDED.isactive;
```

4. اضغط Run
5. تأكد من أن الجدول تم إنشاؤه بنجاح

## بيانات تسجيل الدخول
- **اسم المستخدم**: `admin`
- **كلمة المرور**: `admin123`
- **نوع قاعدة البيانات**: `Supabase Online`

## التحقق من النجاح
بعد تشغيل السكريبت، يجب أن ترى:
- جدول `controller_users` في قائمة الجداول
- مستخدم admin في الجدول
- إمكانية تسجيل الدخول بنجاح

## استكشاف الأخطاء
إذا لم يعمل:
1. تأكد من أن Supabase يعمل
2. تحقق من سلسلة الاتصال في appsettings.json
3. تأكد من أن الجدول تم إنشاؤه بنجاح
4. تحقق من الـ console logs في التطبيق