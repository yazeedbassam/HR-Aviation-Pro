# 🚁 AVIATION HR PRO - Professional Aviation Human Resources Management System

## 🌟 نظرة عامة
نظام إدارة الموارد البشرية المتخصص في مجال الطيران المدني، يوفر إدارة شاملة للتراخيص والشهادات والتدريب لموظفي الطيران.

## ✨ المميزات الرئيسية
- **إدارة المستخدمين**: نظام صلاحيات متقدم مع إدارة الأدوار
- **إدارة الموظفين**: بيانات شاملة مع تتبع التواريخ المهمة
- **إدارة التراخيص**: تتبع صلاحية التراخيص مع تنبيهات تلقائية
- **إدارة المشاريع**: تتبع المشاريع والأنشطة
- **تقارير متقدمة**: تصدير PDF/Excel مع رسوم بيانية
- **نظام إشعارات**: تنبيهات ذكية للتواريخ المهمة
- **واجهة متجاوبة**: تصميم يعمل على جميع الأجهزة

## 🏗️ التقنيات المستخدمة
- **Backend**: ASP.NET Core 8.0
- **Frontend**: Razor Pages + Bootstrap 5
- **Database**: SQL Server, MySQL, PostgreSQL (Supabase)
- **Authentication**: ASP.NET Core Identity
- **PDF Generation**: QuestPDF
- **Email**: SendGrid
- **Caching**: In-Memory Cache

## 🚀 النشر على الإنترنت

### المتطلبات
- حساب GitHub
- حساب Netlify
- حساب Supabase

### خطوات النشر

#### 1. إعداد GitHub Repository
```bash
# إنشاء repository جديد
git init
git add .
git commit -m "Initial commit"
git branch -M main
git remote add origin https://github.com/YOUR_USERNAME/YOUR_REPO_NAME.git
git push -u origin main
```

#### 2. إعداد Supabase
1. اذهب إلى [supabase.com](https://supabase.com)
2. أنشئ مشروع جديد
3. احصل على معلومات الاتصال:
   - Host
   - Database name
   - Username
   - Password
   - API Key

#### 3. إعداد Netlify
1. اذهب إلى [netlify.com](https://netlify.com)
2. انقر "New site from Git"
3. اختر GitHub واختر repository الخاص بك
4. إعدادات البناء:
   - **Build command**: `dotnet publish -c Release -o ./publish`
   - **Publish directory**: `./publish/wwwroot`
   - **Environment variables**:
     ```
     ASPNETCORE_ENVIRONMENT=Production
     SUPABASE_URL=https://your-project.supabase.co
     SUPABASE_ANON_KEY=your-anon-key
     SUPABASE_SERVICE_KEY=your-service-key
     ```

#### 4. إعداد متغيرات البيئة في Netlify
في Netlify Dashboard → Site settings → Environment variables:
- `ASPNETCORE_ENVIRONMENT`: `Production`
- `SUPABASE_URL`: `https://hzweniqfssqorruiujwc.supabase.co`
- `SUPABASE_ANON_KEY`: `eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...`
- `SUPABASE_SERVICE_KEY`: `eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...`
   - Password
   - Port

#### 3. إعداد Netlify
1. اذهب إلى [netlify.com](https://netlify.com)
2. اربط حساب GitHub
3. اختر repository المشروع
4. اضبط إعدادات البناء:
   - Build command: `dotnet publish -c Release -o bin/Release/net8.0/publish`
   - Publish directory: `bin/Release/net8.0/publish`

#### 4. إعداد Environment Variables
في Netlify، أضف المتغيرات التالية:

**Supabase Database:**
```
SUPABASE_HOST=your-supabase-host
SUPABASE_DB=your-database-name
SUPABASE_USER=your-username
SUPABASE_PASSWORD=your-password
SUPABASE_PORT=5432
SUPABASE_URL=https://your-project.supabase.co
SUPABASE_ANON_KEY=your-anon-key
SUPABASE_SERVICE_ROLE_KEY=your-service-role-key
```

**Email Settings:**
```
EMAIL_SMTP_SERVER=your-smtp-server
EMAIL_SMTP_PORT=587
EMAIL_USERNAME=your-email
EMAIL_PASSWORD=your-password
EMAIL_FROM=your-from-email
EMAIL_FROM_NAME=AVIATION HR PRO
```

**Database Settings:**
```
DB_SERVER=your-db-server
DB_NAME=your-db-name
DB_USER=your-db-user
DB_PASSWORD=your-db-password
DB_PORT=1433
```

#### 5. إعداد GitHub Actions
الملف `.github/workflows/deploy.yml` سيقوم بالنشر التلقائي عند كل push.

## 🗄️ قاعدة البيانات

### الجداول الرئيسية
- `controller_users`: بيانات المستخدمين
- `employees`: بيانات الموظفين
- `certificates`: التراخيص والشهادات
- `observations`: الملاحظات والتقييمات
- `projects`: المشاريع والأنشطة
- `notifications`: الإشعارات
- `user_activity_log`: سجل النشاطات

### إنشاء قاعدة البيانات في Supabase
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
```

## 🔧 التطوير المحلي

### المتطلبات
- .NET 8.0 SDK
- Visual Studio 2022 أو VS Code
- SQL Server أو MySQL أو PostgreSQL

### التشغيل
   ```bash
# استنساخ المشروع
git clone https://github.com/YOUR_USERNAME/YOUR_REPO_NAME.git
cd YOUR_REPO_NAME

# استعادة الحزم
   dotnet restore

# البناء
   dotnet build

# التشغيل
   dotnet run
   ```

### إعداد قاعدة البيانات المحلية
1. عدّل `appsettings.json` بإعدادات قاعدة البيانات المحلية
2. أو استخدم متغيرات البيئة:
```bash
set DB_SERVER=localhost
set DB_NAME=HR-Aviation
set DB_USER=sa
set DB_PASSWORD=your-password
```

## 📱 الميزات المتقدمة

### نظام الصلاحيات
- إدارة الأدوار والصلاحيات
- صلاحيات على مستوى الصفحات
- صلاحيات على مستوى البيانات
- تسجيل جميع العمليات

### نظام الإشعارات
- تنبيهات انتهاء صلاحية التراخيص
- إشعارات المشاريع
- إشعارات النظام
- إرسال إشعارات عبر البريد الإلكتروني

### التقارير
- تقارير الموظفين
- تقارير التراخيص
- تقارير المشاريع
- تصدير PDF/Excel
- رسوم بيانية تفاعلية

## 🚨 الأمان

### ميزات الأمان
- تشفير كلمات المرور باستخدام BCrypt
- حماية من CSRF
- تسجيل جميع العمليات
- إدارة الجلسات
- حماية البيانات الحساسة

### أفضل الممارسات
- استخدام HTTPS دائماً
- تحديث الحزم بانتظام
- مراجعة السجلات بانتظام
- نسخ احتياطية دورية

## 📞 الدعم

### معلومات الاتصال
- **البريد الإلكتروني**: info@HRAviation.com
- **الهاتف**: 00962 7 76619258
- **العنوان**: Professional Aviation Human Resources Management System

### الدعم التقني
- توثيق شامل للنظام
- دليل المستخدم
- فيديوهات تعليمية
- دعم فني متخصص

## 📄 الترخيص

هذا المشروع مرخص تحت رخصة MIT. راجع ملف `LICENSE` للتفاصيل.

## 🤝 المساهمة

نرحب بالمساهمات! يرجى:
1. عمل Fork للمشروع
2. إنشاء branch للميزة الجديدة
3. عمل commit للتغييرات
4. عمل Push للbranch
5. إنشاء Pull Request

## 📈 خطة التطوير المستقبلية

### المرحلة القادمة
- [ ] تطبيق موبايل (React Native)
- [ ] API RESTful
- [ ] تكامل مع أنظمة أخرى
- [ ] ذكاء اصطناعي للتنبؤات
- [ ] دعم متعدد اللغات

### التحسينات المطلوبة
- [ ] تحسين الأداء
- [ ] تحسين الأمان
- [ ] تحسين واجهة المستخدم
- [ ] إضافة اختبارات
- [ ] تحسين التوثيق

---

**AVIATION HR PRO** - نظام إدارة الموارد البشرية المتخصص في مجال الطيران المدني 🚁✈️ 