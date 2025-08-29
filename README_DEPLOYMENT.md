# 🚀 دليل نشر AVIATION HR PRO على GitHub و Railway

## 📋 المتطلبات المسبقة

1. **حساب GitHub** - [إنشاء حساب](https://github.com)
2. **حساب Railway** - [إنشاء حساب](https://railway.app)
3. **حساب DBeaver Cloud** - [إنشاء حساب](https://dbeaver.io/cloud/)
4. **Git** مثبت على جهازك

## 🔧 الخطوات التفصيلية

### **المرحلة الأولى: رفع المشروع على GitHub**

#### 1. إنشاء Repository جديد على GitHub
```bash
# اذهب إلى https://github.com
# اضغط على "New repository"
# اسم الـ Repository: aviation-hr-pro
# الوصف: Professional Aviation Human Resources Management System
# اختر Public أو Private حسب رغبتك
# لا تضع README أو .gitignore (لأنها موجودة)
# اضغط "Create repository"
```

#### 2. ربط المشروع المحلي بـ GitHub
```bash
# في مجلد المشروع، نفذ الأوامر التالية:
git remote add origin https://github.com/YOUR_USERNAME/aviation-hr-pro.git
git branch -M main
git push -u origin main
```

### **المرحلة الثانية: إعداد قاعدة البيانات على DBeaver Cloud**

#### 1. إنشاء قاعدة بيانات جديدة
```bash
# اذهب إلى https://dbeaver.io/cloud/
# سجل دخول أو أنشئ حساب جديد
# اضغط "Create Database"
# اختر "PostgreSQL" أو "MySQL"
# اسم قاعدة البيانات: aviation_hr_pro
# احفظ معلومات الاتصال:
# - Host: (سيتم توفيرها)
# - Port: (سيتم توفيرها)
# - Database: aviation_hr_pro
# - Username: (سيتم توفيرها)
# - Password: (سيتم توفيرها)
```

#### 2. استيراد البيانات
```bash
# استخدم DBeaver Desktop لربط قاعدة البيانات الجديدة
# نفذ جميع ملفات SQL من مجلد Database_Scripts/
# ابدأ بـ:
# 1. Create_Permission_System.sql
# 2. Advanced_Permission_System.sql
# 3. باقي الملفات بالترتيب
```

### **المرحلة الثالثة: نشر التطبيق على Railway**

#### 1. ربط GitHub بـ Railway
```bash
# اذهب إلى https://railway.app
# سجل دخول بحساب GitHub
# اضغط "New Project"
# اختر "Deploy from GitHub repo"
# اختر repository: aviation-hr-pro
# اضغط "Deploy Now"
```

#### 2. إعداد متغيرات البيئة
```bash
# في Railway Dashboard، اذهب إلى Variables
# أضف المتغيرات التالية:

# قاعدة البيانات
DB_SERVER=your-dbeaver-host
DB_NAME=aviation_hr_pro
DB_USER=your-dbeaver-username
DB_PASSWORD=your-dbeaver-password

# إعدادات البريد الإلكتروني (اختياري)
SENDGRID_API_KEY=your-sendgrid-api-key
FROM_EMAIL=your-email@domain.com

# إعدادات التطبيق
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080
```

#### 3. إضافة قاعدة البيانات
```bash
# في Railway Dashboard
# اضغط "New" -> "Database"
# اختر "PostgreSQL" أو "MySQL"
# Railway سيقوم بإنشاء قاعدة بيانات جديدة
# انسخ معلومات الاتصال الجديدة
# حدث متغيرات البيئة DB_* بالمعلومات الجديدة
```

#### 4. ربط قاعدة البيانات بالتطبيق
```bash
# في Railway Dashboard
# اذهب إلى "Connect" في قاعدة البيانات
# انسخ معلومات الاتصال
# حدث متغيرات البيئة في التطبيق
```

### **المرحلة الرابعة: اختبار النشر**

#### 1. التحقق من النشر
```bash
# انتظر حتى يكتمل النشر (عادة 2-5 دقائق)
# اذهب إلى "Deployments" في Railway
# تأكد من أن الحالة "Deployed"
```

#### 2. اختبار التطبيق
```bash
# اذهب إلى "Settings" في Railway
# انسخ الـ Domain URL
# افتح الرابط في المتصفح
# تأكد من أن التطبيق يعمل بشكل صحيح
```

## 🔒 إعدادات الأمان

### 1. تحديث كلمات المرور
```bash
# في قاعدة البيانات، حدث كلمات مرور المستخدمين
# خاصة admin user
UPDATE Users SET PasswordHash = 'new-hashed-password' WHERE Username = 'admin';
```

### 2. إعداد HTTPS
```bash
# Railway يوفر HTTPS تلقائياً
# تأكد من أن جميع الروابط تستخدم HTTPS
```

## 📊 مراقبة التطبيق

### 1. مراقبة الأداء
```bash
# في Railway Dashboard
# اذهب إلى "Metrics"
# راقب استخدام CPU و Memory
# راقب عدد الطلبات
```

### 2. مراقبة السجلات
```bash
# في Railway Dashboard
# اذهب إلى "Deployments"
# اضغط على آخر deployment
# اذهب إلى "Logs"
# راقب الأخطاء والتحذيرات
```

## 🔄 التحديثات المستقبلية

### 1. تحديث الكود
```bash
# في المشروع المحلي
git add .
git commit -m "Update description"
git push origin main
# Railway سيقوم بالنشر التلقائي
```

### 2. تحديث قاعدة البيانات
```bash
# استخدم DBeaver Desktop
# اربط قاعدة البيانات الإنتاجية
# نفذ ملفات SQL الجديدة
```

## 🆘 استكشاف الأخطاء

### مشاكل شائعة وحلولها

#### 1. خطأ في الاتصال بقاعدة البيانات
```bash
# تحقق من متغيرات البيئة
# تأكد من صحة معلومات الاتصال
# تحقق من إعدادات Firewall
```

#### 2. خطأ في النشر
```bash
# تحقق من سجلات Railway
# تأكد من صحة Dockerfile
# تحقق من متغيرات البيئة
```

#### 3. خطأ في التطبيق
```bash
# تحقق من سجلات التطبيق
# تأكد من صحة إعدادات appsettings.Production.json
# تحقق من اتصال قاعدة البيانات
```

## 📞 الدعم

إذا واجهت أي مشاكل:
1. تحقق من سجلات Railway
2. راجع هذا الدليل
3. ابحث في وثائق Railway
4. تواصل مع فريق الدعم

---

**ملاحظة:** تأكد من حفظ جميع معلومات الاتصال والكلمات المرور في مكان آمن! 