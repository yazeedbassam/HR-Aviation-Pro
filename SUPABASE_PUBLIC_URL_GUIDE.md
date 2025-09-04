# دليل الحصول على Supabase Public URL

## المشكلة:
Railway لا يدعم IPv6، و Supabase يستخدم IPv6 في بعض الأحيان مما يسبب فشل الاتصال.

## الحل:
استخدام **Supabase Public URL** بدلاً من Private URL.

## الخطوات:

### 1. اذهب إلى Supabase Dashboard
- افتح [Supabase Dashboard](https://supabase.com/dashboard)
- اختر مشروعك

### 2. اذهب إلى Database Settings
- اذهب إلى **Settings** > **Database**
- أو اذهب إلى **Settings** > **API**

### 3. ابحث عن Connection String
- ابحث عن **Connection String** أو **Database URL**
- ستجد خيارين:
  - **Private URL** (يستخدم IPv6 - لا يعمل مع Railway)
  - **Public URL** (يستخدم IPv4 - يعمل مع Railway)

### 4. استخدم Public URL
- انسخ **Public URL** (يجب أن يحتوي على IPv4)
- مثال: `postgresql://postgres:[YOUR-PASSWORD]@db.hzweniqfssqorruiujwc.supabase.co:5432/postgres`

### 5. حدث متغيرات البيئة في Railway
- اذهب إلى **Railway Dashboard** > **Variables**
- حدث `SUPABASE_HOST` إلى:
  ```
  db.hzweniqfssqorruiujwc.supabase.co
  ```
- تأكد من أن `SUPABASE_PORT` = `5432`

### 6. بديل: استخدام DATABASE_URL
- يمكنك أيضاً استخدام `DATABASE_URL` مباشرة:
  ```
  DATABASE_URL=postgresql://postgres:[YOUR-PASSWORD]@db.hzweniqfssqorruiujwc.supabase.co:5432/postgres
  ```

## ملاحظات مهمة:
- **Public URL** يستخدم IPv4 فقط
- **Private URL** قد يستخدم IPv6 (لا يعمل مع Railway)
- تأكد من استخدام كلمة المرور الصحيحة
- تأكد من أن SSL مفعل

## اختبار الاتصال:
بعد التحديث، يجب أن ترى في الـ logs:
```
✅ Supabase connection opened successfully
✅ Supabase test query result: 1
✅ Supabase connection test successful
```

بدلاً من:
```
❌ Failed to connect to 172.64.149.246:5432
❌ Failed to connect to 104.18.38.10:5432
```