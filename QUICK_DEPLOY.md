# 🚀 دليل النشر السريع - HR Aviation System

## ⚡ النشر في 5 دقائق (مجاني 100%)

### 📋 المتطلبات
- ✅ حساب GitHub
- ✅ حساب Netlify  
- ✅ حساب Supabase

---

## 🗄️ الخطوة 1: إعداد قاعدة البيانات (Supabase)

### 1. إنشاء مشروع Supabase
1. اذهب إلى [supabase.com](https://supabase.com)
2. سجل دخول أو أنشئ حساب جديد
3. اضغط "New Project"
4. اختر اسم المشروع: `hr-aviation`
5. اختر كلمة مرور قوية
6. انتظر إنشاء المشروع (2-3 دقائق)

### 2. الحصول على Credentials
بعد إنشاء المشروع، اذهب إلى:
- **Settings** > **Database**
- **Settings** > **API**

سجل هذه المعلومات:
```bash
SUPABASE_URL=https://your-project.supabase.co
SUPABASE_ANON_KEY=your_anon_key_here
SUPABASE_HOST=db.your-project.supabase.co
SUPABASE_DB=postgres
SUPABASE_USER=postgres
SUPABASE_PASSWORD=your_password_here
```

### 3. إنشاء قاعدة البيانات
1. اذهب إلى **SQL Editor**
2. انسخ محتوى ملف `Database_Scripts/Supabase_Setup.sql`
3. اضغط "Run" لتنفيذ السكريبت

---

## 📤 الخطوة 2: رفع المشروع على GitHub

### 1. إنشاء Repository
1. اذهب إلى [github.com](https://github.com)
2. اضغط "New repository"
3. اسم المشروع: `hr-aviation`
4. الوصف: `HR Aviation Management System`
5. اختر Public
6. اضغط "Create repository"

### 2. رفع الكود
```bash
# في مجلد المشروع المحلي
git init
git add .
git commit -m "Initial commit - HR Aviation System"
git branch -M main
git remote add origin https://github.com/username/hr-aviation.git
git push -u origin main
```

---

## 🌐 الخطوة 3: نشر المشروع على Netlify

### 1. ربط GitHub
1. اذهب إلى [netlify.com](https://netlify.com)
2. اضغط "New site from Git"
3. اختر GitHub
4. اختر repository `hr-aviation`

### 2. إعدادات البناء
```bash
Build command: dotnet publish -c Release -o publish
Publish directory: publish
```

### 3. متغيرات البيئة
اضغط "Show advanced" وأضف:

```bash
# Supabase
SUPABASE_URL=https://your-project.supabase.co
SUPABASE_ANON_KEY=your_anon_key_here
SUPABASE_HOST=db.your-project.supabase.co
SUPABASE_DB=postgres
SUPABASE_USER=postgres
SUPABASE_PASSWORD=your_password_here

# Email (اختياري)
EMAIL_SMTP_SERVER=smtp-relay.brevo.com
EMAIL_USERNAME=your_username
EMAIL_PASSWORD=your_password
EMAIL_FROM=noreply@yourdomain.com
```

### 4. النشر
اضغط "Deploy site" وانتظر 2-3 دقائق

---

## 🎯 النتيجة النهائية

### ✅ الموقع متاح على
`https://your-project.netlify.app`

### 🔑 بيانات الدخول
- **اسم المستخدم**: `admin`
- **كلمة المرور**: `password`

### 📱 الميزات المتاحة
- ✅ موقع متجاوب يعمل على جميع الأجهزة
- ✅ SSL مجاني (HTTPS)
- ✅ قاعدة بيانات سحابية آمنة
- ✅ نظام صلاحيات متقدم
- ✅ إدارة شاملة للموارد البشرية

---

## 🚨 استكشاف الأخطاء

### مشكلة: خطأ في الاتصال بقاعدة البيانات
**الحل**: تحقق من متغيرات البيئة في Netlify

### مشكلة: خطأ في البناء
**الحل**: تأكد من أن المشروع يبني محلياً بـ `dotnet build`

### مشكلة: خطأ في تسجيل الدخول
**الحل**: تأكد من تنفيذ سكريبت قاعدة البيانات في Supabase

---

## 📞 المساعدة

### روابط مفيدة
- **Supabase**: [supabase.com/docs](https://supabase.com/docs)
- **Netlify**: [docs.netlify.com](https://docs.netlify.com)
- **GitHub**: [docs.github.com](https://docs.github.com)

### دليل مفصل
راجع ملف `DEPLOYMENT_GUIDE.md` للحصول على تعليمات مفصلة

---

**🎉 مبروك! موقعك الآن متاح على الإنترنت مجاناً!** 🚀✨ 