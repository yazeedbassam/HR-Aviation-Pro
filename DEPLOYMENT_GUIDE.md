# 🚀 دليل النشر التفصيلي - AVIATION HR PRO

## 📋 نظرة عامة
هذا الدليل سيرشدك خطوة بخطوة لنشر تطبيق AVIATION HR PRO على الإنترنت باستخدام GitHub + Netlify + Supabase.

## 🎯 المتطلبات الأساسية
- [x] حساب GitHub
- [x] حساب Netlify
- [x] حساب Supabase
- [x] .NET 8.0 SDK مثبت محلياً

## 🔧 الخطوة 1: إعداد GitHub Repository

### 1.1 إنشاء Repository جديد
1. اذهب إلى [GitHub](https://github.com)
2. اضغط على **"New repository"**
3. أدخل اسم المشروع: `aviation-hr-pro`
4. اختر **Public** أو **Private** حسب رغبتك
5. اضغط **"Create repository"**

### 1.2 رفع الكود إلى GitHub
```bash
# في مجلد المشروع المحلي
git init
git add .
git commit -m "Initial commit: AVIATION HR PRO System"
git branch -M main
git remote add origin https://github.com/YOUR_USERNAME/aviation-hr-pro.git
git push -u origin main
```

### 1.3 التحقق من الرفع
- تأكد من ظهور جميع الملفات في GitHub
- تأكد من وجود ملف `.github/workflows/deploy.yml`

## 🗄️ الخطوة 2: إعداد Supabase

### 2.1 إنشاء مشروع جديد
1. اذهب إلى [Supabase](https://supabase.com)
2. اضغط **"Start your project"**
3. اختر **"New Project"**
4. أدخل اسم المشروع: `aviation-hr-pro`
5. أدخل كلمة مرور قوية لقاعدة البيانات
6. اختر المنطقة الأقرب لك
7. اضغط **"Create new project"**

### 2.2 الحصول على معلومات الاتصال
بعد إنشاء المشروع، اذهب إلى **Settings > Database** واحصل على:

```
Host: db.xxxxxxxxxxxxx.supabase.co
Database name: postgres
Port: 5432
User: postgres
Password: [كلمة المرور التي أدخلتها]
```

### 2.3 الحصول على API Keys
اذهب إلى **Settings > API** واحصل على:

```
URL: https://xxxxxxxxxxxxx.supabase.co
anon public: [مفتاح anon]
service_role secret: [مفتاح service role]
```

### 2.4 إنشاء قاعدة البيانات
اذهب إلى **SQL Editor** واكتب الأوامر التالية:

```sql
-- إنشاء جدول المستخدمين
CREATE TABLE controller_users (
    userid SERIAL PRIMARY KEY,
    username VARCHAR(50) UNIQUE NOT NULL,
    password VARCHAR(255) NOT NULL,
    fullname VARCHAR(100) NOT NULL,
    email VARCHAR(100),
    role VARCHAR(50) NOT NULL,
    department VARCHAR(100),
    isactive BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    last_login TIMESTAMP
);

-- إنشاء جدول الموظفين
CREATE TABLE employees (
    employeeid SERIAL PRIMARY KEY,
    fullname VARCHAR(100) NOT NULL,
    position VARCHAR(100),
    department VARCHAR(100),
    hire_date DATE,
    email VARCHAR(100),
    phone VARCHAR(20),
    isactive BOOLEAN DEFAULT true
);

-- إنشاء جدول التراخيص
CREATE TABLE certificates (
    certificateid SERIAL PRIMARY KEY,
    employeeid INTEGER REFERENCES employees(employeeid),
    certificatetype VARCHAR(100),
    issuedate DATE,
    expirydate DATE,
    status VARCHAR(50),
    notes TEXT
);

-- إنشاء جدول المشاريع
CREATE TABLE projects (
    projectid SERIAL PRIMARY KEY,
    projectname VARCHAR(200) NOT NULL,
    description TEXT,
    startdate DATE,
    enddate DATE,
    status VARCHAR(50),
    managerid INTEGER REFERENCES controller_users(userid),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- إنشاء جدول الإشعارات
CREATE TABLE notifications (
    notificationid SERIAL PRIMARY KEY,
    userid INTEGER REFERENCES controller_users(userid),
    controllerid INTEGER REFERENCES controller_users(userid),
    message TEXT NOT NULL,
    link VARCHAR(500),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    is_read BOOLEAN DEFAULT false,
    note TEXT,
    licensetype VARCHAR(100),
    licenseexpirydate DATE
);

-- إنشاء جدول سجل النشاطات
CREATE TABLE user_activity_log (
    logid SERIAL PRIMARY KEY,
    userid INTEGER REFERENCES controller_users(userid),
    username VARCHAR(50),
    action VARCHAR(100),
    details TEXT,
    ip_address VARCHAR(45),
    user_agent TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- إنشاء مستخدم admin افتراضي
INSERT INTO controller_users (username, password, fullname, email, role, department, isactive)
VALUES (
    'admin',
    '$2a$11$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', -- كلمة المرور: password
    'System Administrator',
    'admin@aviationhr.com',
    'Admin',
    'IT Department',
    true
);
```

### 2.5 اختبار الاتصال
اذهب إلى **Table Editor** وتأكد من إنشاء الجداول بنجاح.

## 🌐 الخطوة 3: إعداد Netlify

### 3.1 ربط GitHub مع Netlify
1. اذهب إلى [Netlify](https://netlify.com)
2. اضغط **"Sign up"** واختر **"GitHub"**
3. اربط حساب GitHub مع Netlify

### 3.2 إنشاء موقع جديد
1. اضغط **"New site from Git"**
2. اختر **GitHub**
3. اختر repository: `aviation-hr-pro`
4. اضبط إعدادات البناء:

```
Build command: dotnet publish -c Release -o bin/Release/net8.0/publish
Publish directory: bin/Release/net8.0/publish
```

### 3.3 إعداد Environment Variables
اذهب إلى **Site settings > Environment variables** وأضف:

#### Supabase Database:
```
SUPABASE_HOST=db.xxxxxxxxxxxxx.supabase.co
SUPABASE_DB=postgres
SUPABASE_USER=postgres
SUPABASE_PASSWORD=your-password
SUPABASE_PORT=5432
SUPABASE_URL=https://xxxxxxxxxxxxx.supabase.co
SUPABASE_ANON_KEY=your-anon-key
SUPABASE_SERVICE_ROLE_KEY=your-service-role-key
```

#### Email Settings:
```
EMAIL_SMTP_SERVER=smtp.gmail.com
EMAIL_SMTP_PORT=587
EMAIL_USERNAME=your-email@gmail.com
EMAIL_PASSWORD=your-app-password
EMAIL_FROM=your-email@gmail.com
EMAIL_FROM_NAME=AVIATION HR PRO
```

#### Database Settings:
```
DB_SERVER=localhost
DB_NAME=HR-Aviation
DB_USER=sa
DB_PASSWORD=your-password
DB_PORT=1433
```

### 3.4 إعداد Domain
1. اذهب إلى **Domain management**
2. اضغط **"Add custom domain"**
3. أدخل اسم النطاق المطلوب
4. اتبع التعليمات لإعداد DNS

## 🔄 الخطوة 4: اختبار النشر

### 4.1 النشر الأولي
1. في Netlify، اضغط **"Deploy site"**
2. انتظر حتى يكتمل البناء
3. تأكد من عدم وجود أخطاء

### 4.2 اختبار التطبيق
1. اذهب إلى الرابط المقدم من Netlify
2. جرب تسجيل الدخول:
   - Username: `admin`
   - Password: `password`
   - Database Type: `supabase`

### 4.3 اختبار قاعدة البيانات
1. تأكد من عمل تسجيل الدخول
2. جرب إضافة موظف جديد
3. جرب إضافة ترخيص جديد
4. تأكد من عمل التقارير

## 🚨 حل المشاكل الشائعة

### مشكلة: فشل البناء
```bash
# تأكد من وجود .NET 8.0
dotnet --version

# امسح cache
dotnet clean
dotnet restore
dotnet build
```

### مشكلة: خطأ في قاعدة البيانات
1. تأكد من صحة معلومات Supabase
2. تأكد من إنشاء الجداول
3. راجع سجلات Netlify

### مشكلة: خطأ في البريد الإلكتروني
1. تأكد من صحة إعدادات SMTP
2. استخدم App Password لـ Gmail
3. تأكد من تفعيل 2FA

## 📱 الخطوة 5: النشر التلقائي

### 5.1 اختبار GitHub Actions
1. عدّل أي ملف في المشروع
2. ارفع التغييرات:
```bash
git add .
git commit -m "Test auto-deploy"
git push
```

### 5.2 مراقبة النشر
1. اذهب إلى **Actions** في GitHub
2. راقب عملية البناء
3. تأكد من النشر التلقائي في Netlify

## 🔒 الخطوة 6: الأمان

### 6.1 HTTPS
- Netlify يوفر HTTPS تلقائياً
- تأكد من تفعيل **Force HTTPS**

### 6.2 Environment Variables
- لا تشارك Environment Variables
- استخدم قيم مختلفة للإنتاج والتطوير

### 6.3 قاعدة البيانات
- تأكد من صحة كلمة المرور
- استخدم Firewall Rules في Supabase

## 📊 الخطوة 7: المراقبة

### 7.1 Netlify Analytics
- اذهب إلى **Analytics** في Netlify
- راقب عدد الزيارات
- راقب الأداء

### 7.2 Supabase Monitoring
- اذهب إلى **Dashboard** في Supabase
- راقب استخدام قاعدة البيانات
- راقب الأخطاء

### 7.3 GitHub Insights
- اذهب إلى **Insights** في GitHub
- راقب نشاط المشروع
- راقب المساهمات

## 🎉 النشر الناجح!

بعد اتباع جميع الخطوات، سيكون لديك:
- ✅ موقع يعمل على الإنترنت
- ✅ قاعدة بيانات Supabase
- ✅ نشر تلقائي من GitHub
- ✅ HTTPS مفعل
- ✅ نظام إشعارات يعمل
- ✅ تقارير PDF/Excel

## 📞 الدعم

إذا واجهت أي مشاكل:
1. راجع سجلات Netlify
2. راجع سجلات Supabase
3. راجع GitHub Actions
4. تواصل مع الدعم الفني

---

**🎯 النشر الناجح = تطبيق يعمل على الإنترنت + قاعدة بيانات آمنة + نشر تلقائي** 