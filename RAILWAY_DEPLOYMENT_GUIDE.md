# 🚂 دليل النشر على Railway - AVIATION HR PRO

## 📋 المتطلبات
- ✅ حساب Railway
- ✅ مشروع GitHub
- ✅ قاعدة بيانات Supabase

## 🚀 خطوات النشر

### 1. إنشاء مشروع جديد على Railway
1. اذهب إلى [railway.app](https://railway.app)
2. اضغط "New Project"
3. اختر "Deploy from GitHub repo"
4. اختر repository: `yazeedbassam/HR-Aviation-Pro`

### 2. إضافة متغيرات البيئة
في Railway Dashboard، اذهب إلى Variables وأضف:

#### 🔗 متغيرات Supabase:
```
SUPABASE_HOST=hzweniqfssqorruiujwc.supabase.co
SUPABASE_PORT=5432
SUPABASE_DB=postgres
SUPABASE_USER=postgres
SUPABASE_PASSWORD=Y@Z105213eed
SUPABASE_URL=https://hzweniqfssqorruiujwc.supabase.co
SUPABASE_ANON_KEY=eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Imh6d2VuaXFmc3Nxb3JydWl1andjIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NTY5MjE4MTIsImV4cCI6MjA3MjQ5NzgxMn0.U4GomCprtgLqKzwwX64DCD1P5lAdw2jQgH78_EjBr_U
SUPABASE_SERVICE_ROLE_KEY=eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Imh6d2VuaXFmc3Nxb3JydWl1andjIiwicm9sZSI6InNlcnZpY2Vfcm9sZSIsImlhdCI6MTc1NjkyMTgxMiwiZXhwIjoyMDcyNDk3ODEyfQ.S--2fv9J8Ebrdn79W0R_Bjh-BkmVSTi--XfgXf75q8s
```

#### 🌐 متغيرات البيئة:
```
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://0.0.0.0:$PORT
PORT=8080
```

#### 📧 متغيرات البريد الإلكتروني (اختيارية):
```
EMAIL_SMTP_SERVER=smtp.gmail.com
EMAIL_SMTP_PORT=587
EMAIL_USERNAME=your-email@gmail.com
EMAIL_PASSWORD=your-app-password
EMAIL_FROM=your-email@gmail.com
EMAIL_FROM_NAME=AVIATION HR PRO
```

### 3. إعدادات النشر
- **Build Command**: `dotnet publish -c Release -o ./publish`
- **Start Command**: `dotnet WebApplication1.dll`
- **Port**: `8080`

### 4. النشر
1. اضغط "Deploy"
2. انتظر حتى يكتمل البناء
3. احصل على الرابط من Railway

## 🔧 استكشاف الأخطاء

### مشاكل شائعة:
1. **خطأ في قاعدة البيانات**: تأكد من متغيرات Supabase
2. **خطأ في البناء**: تأكد من وجود `WebApplication1.csproj`
3. **خطأ في المنفذ**: تأكد من `PORT=8080`

### فحص السجلات:
- اذهب إلى Railway Dashboard
- اضغط على "Deployments"
- اضغط على آخر نشر
- اذهب إلى "Logs"

## ✅ اختبار النشر
1. زر الرابط الذي حصلت عليه من Railway
2. يجب أن ترى صفحة تسجيل الدخول
3. جرب تسجيل الدخول بـ:
   - **المستخدم**: `admin`
   - **كلمة المرور**: `password`
   - **قاعدة البيانات**: `Supabase`

## 🎯 النتيجة المتوقعة
- ✅ التطبيق يعمل كاملاً
- ✅ قاعدة البيانات متصلة
- ✅ جميع الوظائف تعمل
- ✅ التقارير تعمل
- ✅ نظام الإشعارات يعمل

## 📞 الدعم
إذا واجهت أي مشاكل، راجع:
- [Railway Documentation](https://docs.railway.app)
- [Supabase Documentation](https://supabase.com/docs)
- سجلات Railway للتفاصيل

---
**🎉 مبروك! تطبيق AVIATION HR PRO يعمل على Railway!**