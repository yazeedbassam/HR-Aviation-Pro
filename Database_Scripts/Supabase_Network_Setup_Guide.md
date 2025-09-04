# دليل إعداد Network Restrictions لـ Supabase

## المشكلة الحالية
التطبيق لا يستطيع الاتصال بـ Supabase بسبب Network Restrictions. Supabase يحجب الاتصالات من عناوين IP غير مصرح بها.

## الحل المطلوب

### الخطوة 1: الدخول إلى Supabase Dashboard
1. اذهب إلى: https://supabase.com/dashboard
2. اختر مشروعك: `hzweniqfssqorruiujwc`

### الخطوة 2: إعداد Network Restrictions
1. في الشريط الجانبي الأيسر، انقر على **"Database"**
2. انقر على **"Settings"**
3. ابحث عن قسم **"Network Restrictions"**
4. انقر على **"Add restriction"**

### الخطوة 3: إضافة IP Address
#### الخيار 1: السماح لجميع IPs (للتجربة فقط)
- **IPv4 address**: `0.0.0.0`
- **CIDR Block Size**: `0`
- انقر **"Save restriction"**

#### الخيار 2: إضافة IP الخاص بك فقط (أكثر أماناً)
1. اذهب إلى: https://whatismyipaddress.com/
2. انسخ عنوان IP الخاص بك
3. **IPv4 address**: [عنوان IP الخاص بك]
4. **CIDR Block Size**: `32`
5. انقر **"Save restriction"**

### الخطوة 4: الحصول على Connection String الصحيح
1. في نفس صفحة Database Settings
2. ابحث عن قسم **"Connection string"**
3. انقر على **"Copy"** لنسخ Connection string
4. تأكد من أن المنفذ هو `5432` وليس `6543`

### الخطوة 5: تحديث ملف appsettings.Local.json
استبدل `SupabaseConnection` بـ Connection string الجديد من Supabase Dashboard.

## مثال على Connection String الصحيح:
```
Host=hzweniqfssqorruiujwc.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=Y@Z105213eed;SSL Mode=Require;Trust Server Certificate=true;
```

## ملاحظات مهمة:
- تأكد من أن كلمة المرور صحيحة
- تأكد من أن SSL Mode=Require
- تأكد من أن المنفذ 5432
- بعد إضافة Network Restriction، قد تحتاج إلى انتظار دقيقة أو دقيقتين

## إذا لم يعمل:
1. تأكد من أن مشروع Supabase نشط
2. تأكد من أن كلمة المرور صحيحة
3. جرب إعادة تشغيل التطبيق
4. تحقق من logs للتطبيق

## للاختبار:
1. اذهب إلى: http://localhost:5070/Account/Login
2. اختر "Supabase Online"
3. ادخل: admin / admin123