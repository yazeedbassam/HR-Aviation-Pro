# 🗄️ دليل إعداد SQL Server المحلي - AVIATION HR PRO

## 📋 المتطلبات

### 1. تثبيت SQL Server Express
- ✅ **SQL Server Express 2019/2022** مثبت
- ✅ **SQL Server Management Studio (SSMS)** مثبت
- ✅ **SQL Server Browser** يعمل

### 2. التحقق من التثبيت
```cmd
# فتح Command Prompt كـ Administrator
sqlcmd -S localhost\SQLEXPRESS -E
```

## 🚀 خطوات الإعداد

### الخطوة 1: فتح SQL Server Management Studio
1. افتح **SQL Server Management Studio (SSMS)**
2. اتصل بالسيرفر: `localhost\SQLEXPRESS`
3. استخدم **Windows Authentication**

### الخطوة 2: تشغيل سكريبت الإعداد
1. افتح ملف: `Database_Scripts/SQL_Server_Local_Setup.sql`
2. انسخ والصق المحتوى في SSMS
3. اضغط **Execute (F5)**

### الخطوة 3: التحقق من النجاح
يجب أن ترى هذه الرسائل:
```
Database HR-Aviation created successfully
Users table created successfully
Employees table created successfully
Certificates table created successfully
Projects table created successfully
Notifications table created successfully
UserActivityLogs table created successfully
Admin user created successfully
Sample employee created successfully
Sample project created successfully
```

## 🔧 إعدادات الاتصال

### في `appsettings.Local.json`:
```json
{
  "ConnectionStrings": {
    "SqlServerDbConnection": "Server=localhost\\SQLEXPRESS;Database=HR-Aviation;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true;"
  }
}
```

### في `appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "SqlServerDbConnection": "Server=localhost\\SQLEXPRESS;Database=HR-Aviation;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true;"
  }
}
```

## 🧪 اختبار الاتصال

### 1. تشغيل التطبيق
```bash
dotnet run --urls="http://localhost:5070"
```

### 2. اختبار تسجيل الدخول
- **الرابط**: `http://localhost:5070/Account/Login`
- **اسم المستخدم**: `admin`
- **كلمة المرور**: `admin123`
- **نوع قاعدة البيانات**: `Local SQL Server`

## 🔍 استكشاف الأخطاء

### مشكلة: "Cannot connect to SQL Server"
**الحل:**
1. تأكد من تشغيل **SQL Server Browser**
2. تأكد من تشغيل **SQL Server (SQLEXPRESS)**
3. جرب الاتصال بـ: `localhost\SQLEXPRESS`

### مشكلة: "Database HR-Aviation does not exist"
**الحل:**
1. شغل سكريبت `SQL_Server_Local_Setup.sql`
2. تأكد من إنشاء قاعدة البيانات

### مشكلة: "Login failed for user"
**الحل:**
1. تأكد من استخدام **Windows Authentication**
2. تأكد من أن حسابك له صلاحيات على SQL Server

## 📊 التحقق من البيانات

### فحص الجداول:
```sql
USE [HR-Aviation];
SELECT * FROM [Users];
SELECT * FROM [Employees];
SELECT * FROM [Projects];
```

### فحص مستخدم Admin:
```sql
SELECT [UserId], [Username], [FullName], [RoleName], [IsActive]
FROM [Users] 
WHERE [Username] = 'admin';
```

## 🎯 النتيجة المتوقعة

بعد الإعداد الصحيح:
- ✅ قاعدة البيانات `HR-Aviation` موجودة
- ✅ جميع الجداول تم إنشاؤها
- ✅ مستخدم `admin` موجود
- ✅ يمكن تسجيل الدخول بنجاح
- ✅ جميع الوظائف تعمل

## 📞 الدعم

إذا واجهت مشاكل:
1. تحقق من **SQL Server Configuration Manager**
2. تأكد من تشغيل الخدمات المطلوبة
3. راجع **Windows Event Viewer** للأخطاء
4. تأكد من **Firewall** لا يحجب الاتصال

---
**🎉 مبروك! قاعدة البيانات المحلية جاهزة للعمل!**