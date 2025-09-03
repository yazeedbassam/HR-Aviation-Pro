# Supabase Database Setup Instructions

## 🚀 **الخطوات المطلوبة لإنشاء قاعدة البيانات:**

### **1. اذهب إلى Supabase Dashboard:**
- افتح [https://supabase.com/dashboard](https://supabase.com/dashboard)
- اختر مشروعك: **HR-Aviation-Pro**

### **2. اذهب إلى SQL Editor:**
- في الشريط الجانبي الأيسر، اضغط على **"SQL Editor"**
- اضغط على **"New query"**

### **3. انسخ والصق السكريبت:**
- انسخ محتوى ملف `Supabase_Schema.sql`
- الصقه في SQL Editor

### **4. نفذ السكريبت:**
- اضغط على زر **"Run"** (أو Ctrl+Enter)
- انتظر حتى ينتهي التنفيذ

### **5. تحقق من إنشاء الجداول:**
- اذهب إلى **"Table Editor"** في الشريط الجانبي
- تأكد من وجود الجداول التالية:
  - Users
  - Employees
  - Certificates
  - Projects
  - Notifications
  - UserActivityLog
  - Permissions
  - UserPermissions
  - Airports
  - Countries
  - Observations
  - Licenses
  - Configuration

---

## 📋 **بيانات الاتصال المطلوبة:**

### **Connection String:**
```
Host=db.hzweniqfssqorruiujwc.supabase.co
Port=5432
Database=postgres
Username=postgres
Password=Y@Z105213eed
SSL Mode=Require
Trust Server Certificate=true
```

### **API Keys:**
- **Project URL**: `https://hzweniqfssqorruiujwc.supabase.co`
- **Anon Key**: `eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...`
- **Service Role Key**: `eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...`

---

## ✅ **بعد تنفيذ السكريبت:**

1. **سيتم إنشاء جميع الجداول المطلوبة**
2. **سيتم إدخال مستخدم Admin افتراضي:**
   - Username: `admin`
   - Password: `password`
   - Email: `admin@aviation.com`

3. **سيتم إنشاء الصلاحيات الأساسية**
4. **سيتم إدخال بيانات عينة (دول ومطارات)**

---

## 🔧 **اختبار الاتصال:**

بعد إنشاء الجداول، يمكنك اختبار الاتصال من خلال:
1. **تسجيل الدخول** باستخدام `admin` / `password`
2. **اختيار "Supabase Online"** من قائمة أنواع قاعدة البيانات
3. **التحقق من عمل النظام**

---

## 📞 **إذا واجهت أي مشاكل:**

1. تأكد من أن كلمة المرور صحيحة
2. تأكد من أن المشروع نشط
3. تحقق من رسائل الخطأ في SQL Editor
4. تأكد من أن جميع الجداول تم إنشاؤها بنجاح 